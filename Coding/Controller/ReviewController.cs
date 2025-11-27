using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.ViewModels.Tailor;

namespace Sada.Controllers.Tailor
{
    [Authorize(Roles = "tailor")]
    [Area("Tailor")]
    public class ReviewController : Controller
    {
        private readonly SadaDbContext _context;

        public ReviewController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var pageSize = 15;
            var skip = (page - 1) * pageSize;

            var reviewsQuery = _context.Reviews
                .Where(r => r.Service.TailorId == tailor.TailorId)
                .Include(r => r.Client)
                    .ThenInclude(c => c.User)
                .Include(r => r.Service)
                .Include(r => r.Order);

            var totalReviews = await reviewsQuery.CountAsync();

            var reviews = await reviewsQuery
                .OrderByDescending(r => r.ReviewCreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(r => new TailorReviewItemViewModel
                {
                    ReviewId = r.ReviewId,
                    ClientName = $"{r.Client.ClientFirstName} {r.Client.ClientLastName}",
                    ServiceName = r.Service.ServiceName ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewCreatedAt = r.ReviewCreatedAt,
                    OrderId = r.OrderId
                })
                .ToListAsync();

            var averageRating = await _context.Reviews
                .Where(r => r.Service.TailorId == tailor.TailorId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            var fiveStarCount = await _context.Reviews
                .CountAsync(r => r.Service.TailorId == tailor.TailorId && r.Rating == 5);

            var oneStarCount = await _context.Reviews
                .CountAsync(r => r.Service.TailorId == tailor.TailorId && r.Rating == 1);

            var stats = new TailorReviewStats
            {
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 1),
                FiveStarCount = fiveStarCount,
                OneStarCount = oneStarCount
            };

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 5; i >= 1; i--)
            {
                var count = await _context.Reviews
                    .CountAsync(r => r.Service.TailorId == tailor.TailorId && r.Rating == i);
                ratingDistribution[i] = count;
            }

            var viewModel = new TailorReviewListViewModel
            {
                Reviews = reviews,
                Stats = stats,
                RatingDistribution = ratingDistribution,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalReviews / (double)pageSize)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Show(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var review = await _context.Reviews
                .Include(r => r.Client)
                    .ThenInclude(c => c.User)
                .Include(r => r.Service)
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderServices)
                        .ThenInclude(os => os.Service)
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.Service.TailorId == tailor.TailorId);

            if (review == null) return NotFound();

            var viewModel = new TailorReviewDetailViewModel
            {
                ReviewId = review.ReviewId,
                ClientName = $"{review.Client.ClientFirstName} {review.Client.ClientLastName}",
                ClientEmail = review.Client.User.Email,
                ServiceName = review.Service.ServiceName ?? "",
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewCreatedAt = review.ReviewCreatedAt,
                OrderId = review.OrderId,
                OrderServices = review.Order.OrderServices.Select(os => new OrderServiceItemViewModel
                {
                    ServiceName = os.Service.ServiceName ?? "",
                    Price = os.Price
                }).ToList()
            };

            return View(viewModel);
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