using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.ViewModels.Guest;

namespace Sada.Controllers
{
    public class GuestController : Controller
    {
        private readonly SadaDbContext _context;

        public GuestController(SadaDbContext context)
        {
            _context = context;
        }

        // GET: /Guest/Index or just /
        public async Task<IActionResult> Index()
        {
            // Get featured tailors (approved tailors with highest ratings)
            var tailors = await _context.Tailors
                .Where(t => t.IsApproved == true)
                .Include(t => t.User)
                .Include(t => t.Services)
                .ToListAsync();

            // Get all reviews for these tailors
            var tailorIds = tailors.Select(t => t.TailorId).ToList();
            var reviews = await _context.Reviews
                .Where(r => tailorIds.Contains(r.Service.TailorId))
                .Include(r => r.Service)
                .ToListAsync();

            // Create featured tailor view models with ratings
            var featuredTailorViewModels = tailors
                .Select(t =>
                {
                    var tailorReviews = reviews.Where(r => r.Service.TailorId == t.TailorId).ToList();
                    var avgRating = tailorReviews.Any() ? tailorReviews.Average(r => (double?)r.Rating) : null;

                    return new FeaturedTailorViewModel
                    {
                        TailorId = t.TailorId,
                        FullName = $"{t.TailorFirstName} {t.TailorLastName}",
                        Address = t.TailorAddress,
                        ProfilePicture = t.TailorProfilePicture,
                        AverageRating = avgRating,
                        ReviewCount = tailorReviews.Count,
                        ServiceCount = t.Services.Count
                    };
                })
                .OrderByDescending(t => t.AverageRating)
                .ThenByDescending(t => t.ReviewCount)
                .Take(6)
                .ToList();

            // Get active advertisements
            var ads = await _context.Advertisements
                .Include(a => a.Tailor)
                .Select(a => new AdvertisementViewModel
                {
                    AdsId = a.AdsId,
                    Title = a.Title ?? "",
                    Content = a.Content ?? "",
                    Image = a.Image,
                    TailorName = a.Tailor != null
                        ? $"{a.Tailor.TailorFirstName} {a.Tailor.TailorLastName}"
                        : null
                })
                .ToListAsync();

            // Get categories with service counts
            var categories = await _context.Categories
                .Select(c => new CategoryWithCountViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category1,
                    ServiceCount = c.Services.Count
                })
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedTailors = featuredTailorViewModels,
                Advertisements = ads,
                Categories = categories
            };

            return View(viewModel);
        }

        // GET: /Guest/Tailors
        public async Task<IActionResult> Tailors(
            string? search,
            string? location,
            int? category,
            double? minRating,
            string sort = "rating",
            int page = 1)
        {
            var pageSize = 9;
            var skip = (page - 1) * pageSize;

            var query = _context.Tailors
                .Where(t => t.IsApproved == true)
                .Include(t => t.Services)
                    .ThenInclude(s => s.Category)
                .Include(t => t.Availabilities)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    (t.TailorFirstName != null && t.TailorFirstName.Contains(search)) ||
                    (t.TailorLastName != null && t.TailorLastName.Contains(search)) ||
                    (t.TailorAddress != null && t.TailorAddress.Contains(search)) ||
                    t.Services.Any(s =>
                        (s.ServiceName != null && s.ServiceName.Contains(search)) ||
                        (s.ServiceDescription != null && s.ServiceDescription.Contains(search))
                    )
                );
            }

            // Apply location filter
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(t => t.TailorAddress != null && t.TailorAddress.Contains(location));
            }

            // Apply category filter
            if (category.HasValue)
            {
                query = query.Where(t => t.Services.Any(s => s.CategoryId == category.Value));
            }

            var tailors = await query.ToListAsync();

            // Get all reviews and orders for these tailors
            var tailorIds = tailors.Select(t => t.TailorId).ToList();

            var reviews = await _context.Reviews
                .Where(r => tailorIds.Contains(r.Service.TailorId))
                .Include(r => r.Service)
                .ToListAsync();

            var orders = await _context.Orders
                .Where(o => tailorIds.Contains(o.TailorId))
                .ToListAsync();

            // Create view models with ratings and counts
            var tailorViewModels = tailors.Select(t =>
            {
                var tailorReviews = reviews.Where(r => r.Service.TailorId == t.TailorId).ToList();
                var avgRating = tailorReviews.Any() ? tailorReviews.Average(r => (double?)r.Rating) ?? 0 : 0;

                return new TailorListItemViewModel
                {
                    TailorId = t.TailorId,
                    FullName = $"{t.TailorFirstName} {t.TailorLastName}",
                    Address = t.TailorAddress,
                    ProfilePicture = t.TailorProfilePicture,
                    AverageRating = avgRating,
                    ReviewCount = tailorReviews.Count,
                    ServiceCount = t.Services.Count,
                    CompletedOrdersCount = orders.Count(o => o.TailorId == t.TailorId && o.OrderStatus == "completed"),
                    ServiceNames = t.Services.Select(s => s.ServiceName ?? "").Take(3).ToList()
                };
            }).AsQueryable();

            // Apply rating filter
            if (minRating.HasValue)
            {
                tailorViewModels = tailorViewModels.Where(t => t.AverageRating >= minRating.Value);
            }

            // Apply sorting
            tailorViewModels = sort switch
            {
                "name" => tailorViewModels.OrderBy(t => t.FullName),
                "newest" => tailorViewModels.OrderByDescending(t => t.TailorId), // Assuming higher ID = newer
                "reviews" => tailorViewModels.OrderByDescending(t => t.ReviewCount),
                _ => tailorViewModels.OrderByDescending(t => t.AverageRating)
                                    .ThenByDescending(t => t.ReviewCount)
            };

            var totalTailors = tailorViewModels.Count();
            var paginatedTailors = tailorViewModels.Skip(skip).Take(pageSize).ToList();

            // Get categories for filter
            var categories = await _context.Categories
                .Select(c => new CategoryWithCountViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category1,
                    ServiceCount = c.Services.Count
                })
                .ToListAsync();

            var viewModel = new TailorSearchViewModel
            {
                Tailors = paginatedTailors,
                Categories = categories,
                SearchTerm = search,
                Location = location,
                CategoryId = category,
                MinRating = minRating,
                SortBy = sort,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalTailors / (double)pageSize)
            };

            return View(viewModel);
        }

        // GET: /Guest/TailorProfile/5
        public async Task<IActionResult> TailorProfile(int id)
        {
            var tailor = await _context.Tailors
                .Include(t => t.Services)
                    .ThenInclude(s => s.Category)
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.TailorId == id);

            if (tailor == null || tailor.IsApproved != true)
            {
                return NotFound();
            }

            // Get all reviews for this tailor's services
            var reviews = await _context.Reviews
                .Where(r => r.Service.TailorId == tailor.TailorId)
                .Include(r => r.Client)
                .Include(r => r.Service)
                .OrderByDescending(r => r.ReviewCreatedAt)
                .ToListAsync();

            var viewModel = new ViewModels.Guest.TailorProfileViewModel
            {
                TailorId = tailor.TailorId,
                FullName = $"{tailor.TailorFirstName} {tailor.TailorLastName}",
                Phone = tailor.TailorPhone,
                Address = tailor.TailorAddress,
                ProfilePicture = tailor.TailorProfilePicture,
                AverageRating = reviews.Any() ? reviews.Average(r => (double?)r.Rating) : null,
                ReviewCount = reviews.Count,
                Services = tailor.Services.Select(s =>
                {
                    var serviceReviews = reviews.Where(r => r.ServiceId == s.ServiceId).ToList();
                    return new TailorServiceViewModel
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName ?? "",
                        ServiceDescription = s.ServiceDescription ?? "",
                        CategoryName = s.Category.Category1,
                        BasePrice = s.BasePrice,
                        Duration = s.Duration,
                        AverageRating = serviceReviews.Any() ? serviceReviews.Average(r => (double?)r.Rating) : null
                    };
                }).ToList(),
                Reviews = reviews.Select(r => new TailorReviewViewModel
                {
                    ClientName = $"{r.Client.ClientFirstName} {r.Client.ClientLastName}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewCreatedAt = r.ReviewCreatedAt
                }).ToList(),
                Availabilities = tailor.Availabilities
                    .OrderBy(a => a.DayOfWeek)
                    .ThenBy(a => a.StartTime)
                    .Select(a => new AvailabilityViewModel
                    {
                        AvailabilityId = a.AvailabilityId,
                        DayOfWeek = a.DayOfWeek ?? "",
                        StartTime = a.StartTime ?? TimeOnly.MinValue,
                        EndTime = a.EndTime ?? TimeOnly.MinValue,
                        IsAvailable = a.IsAvailable
                    }).ToList()
            };

            return View(viewModel);
        }
    }
}