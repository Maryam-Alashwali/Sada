using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.ViewModels.Admin;

namespace Sada.Controllers.Admin
{
    [Authorize(Roles = "admin")]
    [Area("Admin")]
    public class ReportController : Controller
    {
        private readonly SadaDbContext _context;

        public ReportController(SadaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Revenue()
        {
            var viewModel = new RevenueReportViewModel
            {
                Stats = await GetRevenueStatsAsync(),
                MonthlyRevenue = await GetMonthlyRevenueAsync(),
                TopTailors = await GetTopTailorsRevenueAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var viewModel = new UserReportViewModel
            {
                Stats = await GetUserStatsAsync(),
                UserRegistration = await GetUserRegistrationDataAsync(),
                UsersByRole = await GetUsersByRoleAsync(),
                RecentRegistrations = await GetRecentRegistrationsAsync()
            };

            return View(viewModel);
        }

        private async Task<RevenueReportStats> GetRevenueStatsAsync()
        {
            var completedOrders = _context.Orders.Where(o => o.OrderStatus == "completed");

            return new RevenueReportStats
            {
                TotalRevenue = await completedOrders.SumAsync(o => o.TotalPrice ?? 0),
                PlatformCommission = await completedOrders.SumAsync(o => o.PlatformCommission ?? 0),
                TailorPayout = await completedOrders.SumAsync(o => o.TailorPayout ?? 0),
                TotalOrders = await _context.Orders.CountAsync(),
                CompletedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "completed"),
                PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "pending" || o.OrderStatus == "Pending")
            };
        }

        private async Task<List<MonthlyRevenueData>> GetMonthlyRevenueAsync()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            return await _context.Orders
                .Where(o => o.OrderStatus == "completed" && o.DateCreated.HasValue && o.DateCreated >= sixMonthsAgo)
                .GroupBy(o => new { o.DateCreated!.Value.Year, o.DateCreated.Value.Month })
                .Select(g => new MonthlyRevenueData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.TotalPrice ?? 0)
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .Take(6)
                .ToListAsync();
        }

        private async Task<List<TopTailorRevenueData>> GetTopTailorsRevenueAsync()
        {
            var tailorRevenue = await _context.Orders
                .Where(o => o.OrderStatus == "completed")
                .GroupBy(o => o.TailorId)
                .Select(g => new
                {
                    TailorId = g.Key,
                    CompletedOrdersCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalPrice ?? 0)
                })
                .Where(t => t.CompletedOrdersCount > 0)
                .OrderByDescending(t => t.TotalRevenue)
                .Take(10)
                .ToListAsync();

            var tailorIds = tailorRevenue.Select(t => t.TailorId).ToList();
            var tailors = await _context.Tailors
                .Where(t => tailorIds.Contains(t.TailorId))
                .ToDictionaryAsync(t => t.TailorId);

            return tailorRevenue.Select(t => new TopTailorRevenueData
            {
                TailorId = t.TailorId,
                TailorName = tailors.ContainsKey(t.TailorId)
                    ? $"{tailors[t.TailorId].TailorFirstName} {tailors[t.TailorId].TailorLastName}"
                    : "Unknown",
                CompletedOrdersCount = t.CompletedOrdersCount,
                TotalRevenue = t.TotalRevenue
            }).ToList();
        }

        private async Task<UserReportStats> GetUserStatsAsync()
        {
            return new UserReportStats
            {
                TotalUsers = await _context.Users.CountAsync(u => u.Role != "admin"),
                TotalTailors = await _context.Tailors.CountAsync(),
                TotalClients = await _context.Clients.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.Status == "active" && u.Role != "admin"),
                BlockedUsers = await _context.Users.CountAsync(u => u.Status == "blocked" && u.Role != "admin"),
                PendingTailors = await _context.Tailors.CountAsync(t => t.IsApproved == false),
                ApprovedTailors = await _context.Tailors.CountAsync(t => t.IsApproved == true)
            };
        }

        private async Task<List<UserRegistrationData>> GetUserRegistrationDataAsync()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            return await _context.Users
                .Where(u => u.Role != "admin" && u.CreatedAt.HasValue && u.CreatedAt >= sixMonthsAgo)
                .GroupBy(u => new { u.CreatedAt!.Value.Year, u.CreatedAt.Value.Month })
                .Select(g => new UserRegistrationData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderByDescending(u => u.Year)
                .ThenByDescending(u => u.Month)
                .Take(6)
                .ToListAsync();
        }

        private async Task<List<UsersByRoleData>> GetUsersByRoleAsync()
        {
            return await _context.Users
                .Where(u => u.Role != "admin")
                .GroupBy(u => u.Role)
                .Select(g => new UsersByRoleData
                {
                    Role = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
        }

        private async Task<List<RecentRegistrationData>> GetRecentRegistrationsAsync()
        {
            var users = await _context.Users
                .Include(u => u.Tailors)
                .Include(u => u.Clients)
                .Where(u => u.Role != "admin")
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            return users.Select(u => new RecentRegistrationData
            {
                UserId = u.UserId,
                Email = u.Email,
                Role = u.Role,
                FullName = u.Role == "tailor"
                    ? $"{u.Tailors.FirstOrDefault()?.TailorFirstName} {u.Tailors.FirstOrDefault()?.TailorLastName}"
                    : u.Role == "client"
                    ? $"{u.Clients.FirstOrDefault()?.ClientFirstName} {u.Clients.FirstOrDefault()?.ClientLastName}"
                    : "N/A",
                CreatedAt = u.CreatedAt
            }).ToList();
        }
    }
}