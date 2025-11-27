using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.ViewModels.Tailor;

namespace Sada.Controllers.Tailor
{
    [Authorize(Roles = "tailor")]
    [Area("Tailor")]
    public class DashboardController : Controller
    {
        private readonly SadaDbContext _context;

        public DashboardController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tailor = await GetCurrentTailorAsync();
            //if (tailor == null) return RedirectToAction("Login", "Account");
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var viewModel = new TailorDashboardViewModel
            {
                Stats = await GetDashboardStatsAsync(tailor.TailorId),
                RecentOrders = await GetRecentOrdersAsync(tailor.TailorId),
                RecentReviews = await GetRecentReviewsAsync(tailor.TailorId)
            };

            return View(viewModel);
        }

        private async Task<TailorDashboardStats> GetDashboardStatsAsync(int tailorId)
        {
            var newRequests = await _context.Orders
                .CountAsync(o => o.TailorId == tailorId && o.OrderStatus == "requested");

            var activeOrders = await _context.Orders
                .CountAsync(o => o.TailorId == tailorId &&
                    (o.OrderStatus == "accepted" || o.OrderStatus == "in_progress"));

            var totalEarnings = await _context.Orders
                .Where(o => o.TailorId == tailorId && o.OrderStatus == "completed")
                .SumAsync(o => o.TailorPayout ?? 0);

            var averageRating = await _context.Reviews
                .Where(r => r.Service.TailorId == tailorId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            var totalServices = await _context.Services
                .CountAsync(s => s.TailorId == tailorId);

            var unreadMessages = await _context.Messages
                .CountAsync(m => m.ReceiverId == tailorId &&
                    m.ReceiverType == "tailor" && !m.IsRead);

            return new TailorDashboardStats
            {
                NewRequests = newRequests,
                ActiveOrders = activeOrders,
                TotalEarnings = totalEarnings,
                AverageRating = Math.Round(averageRating, 1),
                TotalServices = totalServices,
                UnreadMessages = unreadMessages
            };
        }

        private async Task<List<TailorRecentOrderViewModel>> GetRecentOrdersAsync(int tailorId)
        {
            return await _context.Orders
                .Where(o => o.TailorId == tailorId)
                .Include(o => o.Client)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .OrderByDescending(o => o.DateCreated)
                .Take(5)
                .Select(o => new TailorRecentOrderViewModel
                {
                    OrderId = o.OrderId,
                    ClientName = $"{o.Client.ClientFirstName} {o.Client.ClientLastName}",
                    OrderStatus = o.OrderStatus ?? "Pending",
                    TotalPrice = o.TotalPrice,
                    DateCreated = o.DateCreated,
                    Services = o.OrderServices.Select(os => os.Service.ServiceName ?? "").ToList()
                })
                .ToListAsync();
        }

        private async Task<List<TailorRecentReviewViewModel>> GetRecentReviewsAsync(int tailorId)
        {
            return await _context.Reviews
                .Where(r => r.Service.TailorId == tailorId)
                .Include(r => r.Client)
                .Include(r => r.Service)
                .OrderByDescending(r => r.ReviewCreatedAt)
                .Take(3)
                .Select(r => new TailorRecentReviewViewModel
                {
                    ReviewId = r.ReviewId,
                    ClientName = $"{r.Client.ClientFirstName} {r.Client.ClientLastName}",
                    ServiceName = r.Service.ServiceName ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewCreatedAt = r.ReviewCreatedAt
                })
                .ToListAsync();
        }

        private async Task<Models.Tailor?> GetCurrentTailorAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Tailors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Email == userEmail);
        }
    }
}