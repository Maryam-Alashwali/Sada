using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.Models;
using Sada.ViewModels.Client;

namespace Sada.Controllers.Client
{
    [Authorize(Roles = "client")]
    [Area("Client")]
    public class ClientReviewController : Controller
    {
        private readonly SadaDbContext _context;

        public ClientReviewController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                //return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Auth");
            }

            var pageSize = 10;
            var skip = (page - 1) * pageSize;

            var totalReviews = await _context.Reviews
                .Where(r => r.ClientId == client.ClientId)
                .CountAsync();

            var reviews = await _context.Reviews
                .Where(r => r.ClientId == client.ClientId)
                .Include(r => r.Service)
                .Include(r => r.Order)
                .OrderByDescending(r => r.ReviewCreatedAt)
                .Skip(skip)
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

            var viewModel = new ReviewListViewModel
            {
                Reviews = reviews,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalReviews / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalReviews
            };

            return View(viewModel);
            //return View("~/Views/Client/Reviews.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                //return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Auth");
            }

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == model.ReviewId && r.ClientId == client.ClientId);

            if (review == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction(nameof(Index));
            }

            review.Rating = model.Rating;
            review.Comment = model.Comment;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Review updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.ClientId == client.ClientId);

            if (review == null)
            {
                return Json(new { success = false, message = "Review not found." });
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Review deleted successfully." });
        }

        private async Task<Models.Client?> GetCurrentClientAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == userEmail);
        }
    }
}