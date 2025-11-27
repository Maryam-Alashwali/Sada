using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.Models;
using Sada.ViewModels.Admin;

namespace Sada.Controllers.Admin
{
    [Authorize(Roles = "admin")]
    [Area("Admin")]
    public class AdvertisementController : Controller
    {
        private readonly SadaDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdvertisementController(SadaDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var pageSize = 10;

            var adsQuery = _context.Advertisements
                .Include(a => a.Tailor)
                .OrderByDescending(a => a.StartDate)
                .Select(a => new AdvertisementViewModel
                {
                    AdsId = a.AdsId,
                    Title = a.Title ?? "",
                    Content = a.Content ?? "",
                    TailorId = a.TailorId,
                    ExistingImage = a.Image,
                    StartDate = a.StartDate ?? DateTime.Now,
                    EndDate = a.EndDate ?? DateTime.Now.AddDays(7),
                    TailorName = a.Tailor != null
                        ? $"{a.Tailor.TailorFirstName} {a.Tailor.TailorLastName}"
                        : "General"
                });

            var paginatedAds = await PaginatedList<AdvertisementViewModel>.CreateAsync(adsQuery, page, pageSize);

            var tailors = await _context.Tailors
                .Where(t => t.IsApproved == true)
                .Select(t => new TailorSelectItem
                {
                    TailorId = t.TailorId,
                    FullName = $"{t.TailorFirstName} {t.TailorLastName}"
                })
                .ToListAsync();

            var viewModel = new AdvertisementListViewModel
            {
                Advertisements = paginatedAds.Items,
                Tailors = tailors,
                CurrentPage = paginatedAds.PageIndex,
                TotalPages = paginatedAds.TotalPages,
                PageSize = paginatedAds.PageSize,
                TotalCount = paginatedAds.TotalCount
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdvertisementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = await GetCurrentAdminIdAsync();
            if (adminId == null)
            {
                TempData["Error"] = "Admin not found.";
                return RedirectToAction(nameof(Index));
            }

            var imagePath = await UploadImageAsync(model.ImageFile);

            var advertisement = new Advertisement
            {
                AdminId = adminId.Value,
                TailorId = model.TailorId,
                Title = model.Title,
                Content = model.Content,
                Image = imagePath,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            _context.Advertisements.Add(advertisement);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Advertisement created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AdvertisementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Index));
            }

            var ad = await _context.Advertisements.FindAsync(model.AdsId);
            if (ad == null)
            {
                TempData["Error"] = "Advertisement not found.";
                return RedirectToAction(nameof(Index));
            }

            var imagePath = ad.Image;
            if (model.ImageFile != null)
            {
                imagePath = await UploadImageAsync(model.ImageFile);
            }

            ad.TailorId = model.TailorId;
            ad.Title = model.Title;
            ad.Content = model.Content;
            ad.Image = imagePath;
            ad.StartDate = model.StartDate;
            ad.EndDate = model.EndDate;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Advertisement updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null)
            {
                TempData["Error"] = "Advertisement not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Advertisements.Remove(ad);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Advertisement deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> GetCurrentAdminIdAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            var admin = await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.User.Email == userEmail);

            return admin?.AdminId;
        }

        private async Task<string?> UploadImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "advertisements");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/advertisements/{uniqueFileName}";
        }

        // Move PaginatedList class here
        public class PaginatedList<T>
        {
            public List<T> Items { get; }
            public int PageIndex { get; }
            public int TotalPages { get; }
            public int TotalCount { get; }
            public int PageSize { get; }

            public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
            {
                PageIndex = pageIndex;
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                TotalCount = count;
                PageSize = pageSize;
                Items = items;
            }

            public bool HasPreviousPage => PageIndex > 1;
            public bool HasNextPage => PageIndex < TotalPages;

            public static async Task<PaginatedList<T>> CreateAsync(
                IQueryable<T> source, int pageIndex, int pageSize)
            {
                var count = await source.CountAsync();
                var items = await source
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedList<T>(items, count, pageIndex, pageSize);
            }
        }
    }
}