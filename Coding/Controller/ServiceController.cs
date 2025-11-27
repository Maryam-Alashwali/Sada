using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.Models;
using Sada.ViewModels.Tailor;

namespace Sada.Controllers.Tailor
{
    [Authorize(Roles = "tailor")]
    [Area("Tailor")]
    public class ServiceController : Controller
    {
        private readonly SadaDbContext _context;

        public ServiceController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var services = await _context.Services
                .Where(s => s.TailorId == tailor.TailorId)
                .Include(s => s.Category)
                .Include(s => s.Reviews)
                .Select(s => new TailorServiceItemViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName ?? "",
                    ServiceDescription = s.ServiceDescription ?? "",
                    CategoryName = s.Category.Category1,
                    BasePrice = s.BasePrice,
                    Duration = s.Duration,
                    ReviewCount = s.Reviews.Count,
                    AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => (double?)r.Rating) : null
                })
                .ToListAsync();

            var categories = await _context.Categories
                .Select(c => new CategorySelectItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category1
                })
                .ToListAsync();

            var viewModel = new TailorServiceListViewModel
            {
                Services = services,
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TailorServiceFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            // Check if service name already exists for this tailor
            var existingService = await _context.Services
                .AnyAsync(s => s.TailorId == tailor.TailorId && s.ServiceName == model.ServiceName);

            if (existingService)
            {
                TempData["Error"] = "You already have a service with this name.";
                return RedirectToAction(nameof(Index));
            }

            var service = new Service
            {
                TailorId = tailor.TailorId,
                CategoryId = model.CategoryId,
                ServiceName = model.ServiceName,
                ServiceDescription = model.ServiceDescription,
                BasePrice = model.BasePrice,
                Duration = model.Duration
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(TailorServiceFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var service = await _context.Services.FindAsync(model.ServiceId);
            if (service == null || service.TailorId != tailor.TailorId)
            {
                return Forbid();
            }

            // Check if service name already exists for this tailor (excluding current service)
            var existingService = await _context.Services
                .AnyAsync(s => s.TailorId == tailor.TailorId &&
                              s.ServiceName == model.ServiceName &&
                              s.ServiceId != model.ServiceId);

            if (existingService)
            {
                TempData["Error"] = "You already have a service with this name.";
                return RedirectToAction(nameof(Index));
            }

            service.CategoryId = model.CategoryId;
            service.ServiceName = model.ServiceName;
            service.ServiceDescription = model.ServiceDescription;
            service.BasePrice = model.BasePrice;
            service.Duration = model.Duration;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Service updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var service = await _context.Services
                .Include(s => s.OrderServices)
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.TailorId == tailor.TailorId);

            if (service == null) return NotFound();

            if (service.OrderServices.Any())
            {
                TempData["Error"] = "Cannot delete service that has associated orders.";
                return RedirectToAction(nameof(Index));
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var service = await _context.Services.FindAsync(id);
            if (service == null || service.TailorId != tailor.TailorId)
            {
                return Forbid();
            }

            TempData["Info"] = "Status toggle feature coming soon.";
            return RedirectToAction(nameof(Index));
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