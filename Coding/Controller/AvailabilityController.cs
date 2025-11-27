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
    public class AvailabilityController : Controller
    {
        private readonly SadaDbContext _context;

        public AvailabilityController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tailor = await GetCurrentTailorAsync();
            //if (tailor == null) return RedirectToAction("Login", "Account");
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var availabilities = await _context.Availabilities
                .Where(a => a.TailorId == tailor.TailorId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var daysOrder = new Dictionary<string, int>
            {
                { "Monday", 1 }, { "Tuesday", 2 }, { "Wednesday", 3 },
                { "Thursday", 4 }, { "Friday", 5 }, { "Saturday", 6 }, { "Sunday", 7 }
            };

            var groupedAvailabilities = availabilities
                .GroupBy(a => a.DayOfWeek)
                .OrderBy(g => daysOrder.GetValueOrDefault(g.Key ?? "", 8))
                .ToDictionary(
                    g => g.Key ?? "",
                    g => g.Select(a => new AvailabilityViewModel
                    {
                        AvailabilityId = a.AvailabilityId,
                        DayOfWeek = a.DayOfWeek ?? "",
                        StartTime = a.StartTime ?? TimeOnly.MinValue,
                        EndTime = a.EndTime ?? TimeOnly.MinValue,
                        IsAvailable = a.IsAvailable
                    }).ToList()
                );

            var viewModel = new AvailabilityListViewModel
            {
                AvailabilitiesByDay = groupedAvailabilities
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            // Check for overlapping time slots
            var overlapping = await _context.Availabilities
                .Where(a => a.TailorId == tailor.TailorId &&
                           a.DayOfWeek == model.DayOfWeek &&
                           a.IsAvailable == true)
                .AnyAsync(a =>
                    (a.StartTime >= model.StartTime && a.StartTime < model.EndTime) ||
                    (a.EndTime > model.StartTime && a.EndTime <= model.EndTime) ||
                    (a.StartTime <= model.StartTime && a.EndTime >= model.EndTime));

            if (overlapping)
            {
                TempData["Error"] = "This time slot overlaps with an existing availability slot.";
                return RedirectToAction(nameof(Index));
            }

            var availability = new Availability
            {
                TailorId = tailor.TailorId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                IsAvailable = model.IsAvailable
            };

            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Availability slot added successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var availability = await _context.Availabilities.FindAsync(model.AvailabilityId);
            if (availability == null || availability.TailorId != tailor.TailorId)
            {
                return Forbid();
            }

            // Check for overlapping time slots
            var overlapping = await _context.Availabilities
                .Where(a => a.TailorId == tailor.TailorId &&
                           a.DayOfWeek == availability.DayOfWeek &&
                           a.AvailabilityId != availability.AvailabilityId &&
                           a.IsAvailable == true)
                .AnyAsync(a =>
                    (a.StartTime >= model.StartTime && a.StartTime < model.EndTime) ||
                    (a.EndTime > model.StartTime && a.EndTime <= model.EndTime) ||
                    (a.StartTime <= model.StartTime && a.EndTime >= model.EndTime));

            if (overlapping)
            {
                TempData["Error"] = "This time slot overlaps with an existing availability slot.";
                return RedirectToAction(nameof(Index));
            }

            availability.StartTime = model.StartTime;
            availability.EndTime = model.EndTime;
            availability.IsAvailable = model.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Availability slot updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var availability = await _context.Availabilities.FindAsync(id);
            if (availability == null || availability.TailorId != tailor.TailorId)
            {
                return Forbid();
            }

            _context.Availabilities.Remove(availability);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Availability slot deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkAvailabilityUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return Json(new { success = false, message = "Tailor not found." });

            var existingSlots = _context.Availabilities
                .Where(a => a.TailorId == tailor.TailorId && a.DayOfWeek == model.DayOfWeek);

            _context.Availabilities.RemoveRange(existingSlots);

            if (model.IsAvailable)
            {
                var defaultSlot = new Availability
                {
                    TailorId = tailor.TailorId,
                    DayOfWeek = model.DayOfWeek,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0),
                    IsAvailable = true
                };

                _context.Availabilities.Add(defaultSlot);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
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