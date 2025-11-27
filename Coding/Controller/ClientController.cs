using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sada.Models;
using Sada.Data;
using Sada.ViewModels.Client;

namespace Sada.Controllers.Client
{
    [Authorize(Roles = "client")]
    [Area("Client")]
    public class ClientController : Controller
    {
        private readonly SadaDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public ClientController(SadaDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Dashboard()
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var stats = await GetDashboardStatsAsync(client.ClientId);
            var recentOrders = await GetRecentOrdersAsync(client.ClientId, 5);
            var recentReviews = await GetRecentReviewsAsync(client.ClientId, 5);
            var latestMeasurements = await _context.Measurements
                .Where(m => m.ClientId == client.ClientId)
                .OrderByDescending(m => m.MeasurementId)
                .Select(m => new MeasurementViewModel
                {
                    MeasurementId = m.MeasurementId,
                    Chest = m.Chest,
                    Waist = m.Waist,
                    Hips = m.Hips,
                    Length = m.Length,
                    SleeveLength = m.SleeveLength,
                    OtherNotes = m.OtherNotes
                })
                .FirstOrDefaultAsync();

            var viewModel = new ClientDashboardViewModel
            {
                Client = new ClientProfileViewModel
                {
                    ClientId = client.ClientId,
                    Email = client.User.Email,
                    ClientFirstName = client.ClientFirstName ?? "",
                    ClientLastName = client.ClientLastName ?? "",
                    PhoneNumber = client.PhoneNumber,
                    ClientAddress = client.ClientAddress,
                    CreatedAt = client.CreatedAt
                },
                Stats = stats,
                RecentOrders = recentOrders,
                RecentReviews = recentReviews,
                LatestMeasurements = latestMeasurements
            };

            return View(viewModel);
            //return View("~/Views/Client/Dashboard.cshtml", viewModel);
        }

        public async Task<IActionResult> Profile(string? tab, int page = 1)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                //return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Auth");
            }

            var activeTab = tab ?? "orders";
            var pageSize = 10;

            var orders = await GetOrdersPagedAsync(client.ClientId, page, pageSize);
            var reviews = await GetReviewsPagedAsync(client.ClientId, page, pageSize);
            var measurements = await _context.Measurements
                .Where(m => m.ClientId == client.ClientId)
                .OrderByDescending(m => m.MeasurementId)
                .Select(m => new MeasurementViewModel
                {
                    MeasurementId = m.MeasurementId,
                    Chest = m.Chest,
                    Waist = m.Waist,
                    Hips = m.Hips,
                    Length = m.Length,
                    SleeveLength = m.SleeveLength,
                    OtherNotes = m.OtherNotes
                })
                .FirstOrDefaultAsync();

            var viewModel = new ClientProfilePageViewModel
            {
                Client = new ClientProfileViewModel
                {
                    ClientId = client.ClientId,
                    Email = client.User.Email,
                    ClientFirstName = client.ClientFirstName ?? "",
                    ClientLastName = client.ClientLastName ?? "",
                    PhoneNumber = client.PhoneNumber,
                    ClientAddress = client.ClientAddress,
                    CreatedAt = client.CreatedAt
                },
                Orders = orders.Items,
                Reviews = reviews.Items,
                Measurements = measurements,
                ActiveTab = activeTab,
                CurrentPage = page,
                TotalPages = activeTab == "orders" ? orders.TotalPages : reviews.TotalPages
            };

            return View(viewModel);
            //return View("~/Views/Client/Profile.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Profile));
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                //return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Auth");
            }

            client.ClientFirstName = model.ClientFirstName;
            client.ClientLastName = model.ClientLastName;
            client.PhoneNumber = model.PhoneNumber;
            client.ClientAddress = model.ClientAddress;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Profile));
            }

            var userEmail = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                //return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Auth");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user, user.Password, model.CurrentPassword);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Profile));
            }

            user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Password updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreMeasurement(MeasurementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Dashboard));
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                //return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Auth");
            }

            var measurement = new Measurement
            {
                ClientId = client.ClientId,
                Chest = model.Chest,
                Waist = model.Waist,
                Hips = model.Hips,
                Length = model.Length,
                SleeveLength = model.SleeveLength,
                OtherNotes = model.OtherNotes
            };

            _context.Measurements.Add(measurement);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Measurements saved successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMeasurement(MeasurementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Dashboard));
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                //return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Auth");
            }

            var measurement = await _context.Measurements
                .Where(m => m.MeasurementId == model.MeasurementId && m.ClientId == client.ClientId)
                .FirstOrDefaultAsync();

            if (measurement == null)
            {
                TempData["Error"] = "Measurement not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            measurement.Chest = model.Chest;
            measurement.Waist = model.Waist;
            measurement.Hips = model.Hips;
            measurement.Length = model.Length;
            measurement.SleeveLength = model.SleeveLength;
            measurement.OtherNotes = model.OtherNotes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Measurements updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        #region Helper Methods

        private async Task<Models.Client?> GetCurrentClientAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == userEmail);
        }

        private async Task<DashboardStatsViewModel> GetDashboardStatsAsync(int clientId)
        {
            var orders = await _context.Orders.Where(o => o.ClientId == clientId).ToListAsync();
            var reviews = await _context.Reviews.Where(r => r.ClientId == clientId).ToListAsync();
            var measurements = await _context.Measurements.Where(m => m.ClientId == clientId).ToListAsync();

            return new DashboardStatsViewModel
            {
                TotalOrders = orders.Count,
                CompletedOrders = orders.Count(o => o.OrderStatus == "completed"),
                PendingOrders = orders.Count(o => o.OrderStatus == "pending" || o.OrderStatus == "Pending" || o.OrderStatus == "in_progress"),
                AverageRating = reviews.Any() ? reviews.Where(r => r.Rating.HasValue).Average(r => r.Rating ?? 0) : 0,
                TotalReviews = reviews.Count,
                MeasurementsCount = measurements.Count
            };
        }

        private async Task<List<RecentOrderViewModel>> GetRecentOrdersAsync(int clientId, int count)
        {
            return await _context.Orders
                .Where(o => o.ClientId == clientId)
                .Include(o => o.Tailor)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .OrderByDescending(o => o.DateCreated)
                .Take(count)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.OrderId,
                    TailorName = $"{o.Tailor.TailorFirstName} {o.Tailor.TailorLastName}",
                    OrderStatus = o.OrderStatus,
                    TotalPrice = o.TotalPrice,
                    DateCreated = o.DateCreated,
                    Services = o.OrderServices.Select(os => os.Service.ServiceName ?? "").ToList()
                })
                .ToListAsync();
        }

        private async Task<List<RecentReviewViewModel>> GetRecentReviewsAsync(int clientId, int count)
        {
            return await _context.Reviews
                .Where(r => r.ClientId == clientId)
                .Include(r => r.Service)
                .OrderByDescending(r => r.ReviewCreatedAt)
                .Take(count)
                .Select(r => new RecentReviewViewModel
                {
                    ReviewId = r.ReviewId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewCreatedAt = r.ReviewCreatedAt,
                    ServiceName = r.Service.ServiceName ?? "N/A",
                    OrderId = r.OrderId
                })
                .ToListAsync();
        }

        private async Task<(List<OrderListItemViewModel> Items, int TotalPages)> GetOrdersPagedAsync(int clientId, int page, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.ClientId == clientId)
                .Include(o => o.Tailor)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderByDescending(o => o.DateCreated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListItemViewModel
                {
                    OrderId = o.OrderId,
                    TailorName = $"{o.Tailor.TailorFirstName} {o.Tailor.TailorLastName}",
                    OrderStatus = o.OrderStatus,
                    TotalPrice = o.TotalPrice,
                    DateCreated = o.DateCreated,
                    Services = o.OrderServices.Select(os => os.Service.ServiceName ?? "").ToList()
                })
                .ToListAsync();

            return (items, totalPages);
        }

        private async Task<(List<ReviewListItemViewModel> Items, int TotalPages)> GetReviewsPagedAsync(int clientId, int page, int pageSize)
        {
            var query = _context.Reviews
                .Where(r => r.ClientId == clientId)
                .Include(r => r.Service);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderByDescending(r => r.ReviewCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewListItemViewModel
                {
                    ReviewId = r.ReviewId,
                    ServiceName = r.Service.ServiceName ?? "N/A",
                    OrderId = r.OrderId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewCreatedAt = r.ReviewCreatedAt
                })
                .ToListAsync();

            return (items, totalPages);
        }

        #endregion
    }
}