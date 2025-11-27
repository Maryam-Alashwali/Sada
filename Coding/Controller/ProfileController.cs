using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.Models;
using Sada.ViewModels.Tailor;

namespace Sada.Controllers.Tailor
{
    [Authorize(Roles = "tailor")]
    [Area("Tailor")]
    public class ProfileController : Controller
    {
        private readonly SadaDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            SadaDbContext context,
            IPasswordHasher<User> passwordHasher,
            IWebHostEnvironment environment)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _environment = environment;
        }

        public async Task<IActionResult> Show()
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var viewModel = new TailorProfileViewModel
            {
                TailorId = tailor.TailorId,
                TailorFirstName = tailor.TailorFirstName ?? "",
                TailorLastName = tailor.TailorLastName ?? "",
                TailorPhone = tailor.TailorPhone ?? "",
                TailorAddress = tailor.TailorAddress ?? "",
                ExistingProfilePicture = tailor.TailorProfilePicture,
                Email = tailor.User.Email,
                IsApproved = tailor.IsApproved
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Edit()
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var viewModel = new TailorProfileViewModel
            {
                TailorId = tailor.TailorId,
                TailorFirstName = tailor.TailorFirstName ?? "",
                TailorLastName = tailor.TailorLastName ?? "",
                TailorPhone = tailor.TailorPhone ?? "",
                TailorAddress = tailor.TailorAddress ?? "",
                ExistingProfilePicture = tailor.TailorProfilePicture,
                Email = tailor.User.Email,
                IsApproved = tailor.IsApproved
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(TailorProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            // Check if email is already taken
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.UserId != tailor.UserId);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View("Edit", model);
            }

            // Update tailor information
            tailor.TailorFirstName = model.TailorFirstName;
            tailor.TailorLastName = model.TailorLastName;
            tailor.TailorPhone = model.TailorPhone;
            tailor.TailorAddress = model.TailorAddress;

            // Handle profile picture upload
            if (model.ProfilePictureFile != null)
            {
                var profilePicturePath = await UploadProfilePictureAsync(model.ProfilePictureFile);
                if (profilePicturePath != null)
                {
                    tailor.TailorProfilePicture = profilePicturePath;
                }
            }

            // Update user email
            tailor.User.Email = model.Email;
            tailor.User.UpdatedAt = DateTime.Now;

            // Update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                tailor.User.Password = _passwordHasher.HashPassword(tailor.User, model.Password);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Show));
        }

        private async Task<string?> UploadProfilePictureAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/profiles/{uniqueFileName}";
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