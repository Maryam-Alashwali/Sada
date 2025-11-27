using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.Models;
using Sada.ViewModels.Auth;
using System.Security.Claims;

namespace Sada.Controllers
{
    public class AuthController : Controller
    {
        private readonly SadaDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            SadaDbContext context,
            IPasswordHasher<User> passwordHasher,
            IWebHostEnvironment environment)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _environment = environment;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create user
                var user = new User
                {
                    Email = model.Email,
                    Password = _passwordHasher.HashPassword(new User(), model.Password),
                    Role = model.Role,
                    Status = model.Role == "tailor" ? "pending" : "active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Handle profile picture upload
                string? profilePicturePath = null;
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    profilePicturePath = await UploadProfilePictureAsync(
                        model.ProfilePicture,
                        model.FirstName,
                        model.LastName
                    );
                }

                // Create role-specific record
                if (model.Role == "tailor")
                {
                    var tailor = new Models.Tailor
                    {
                        UserId = user.UserId,
                        TailorFirstName = model.FirstName,
                        TailorLastName = model.LastName,
                        TailorPhone = model.Phone,
                        TailorAddress = model.Address,
                        TailorProfilePicture = profilePicturePath,
                        IsApproved = false
                    };
                    _context.Tailors.Add(tailor);
                }
                else if (model.Role == "client")
                { //Mahmoud
                    var client = new Sada.Models.Client               
                    {
                        UserId = user.UserId,
                        ClientFirstName = model.FirstName,
                        ClientLastName = model.LastName,
                        PhoneNumber = model.Phone,
                        ClientAddress = model.Address,
                        CreatedAt = DateTime.Now
                    };
                    _context.Clients.Add(client);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Sign in the user
                await SignInUserAsync(user);

                TempData["Success"] = "Registration successful!";

                // Redirect based on role
                if (model.Role == "tailor")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Tailor" });
                }
                else if (model.Role == "client")
                {
                    return RedirectToAction("Index", "Guest");
                }
                else
                {
                    return RedirectToAction("Index", "Guest");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "The provided credentials do not match our records.");
                return View(model);
            }

            // Verify password
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "The provided credentials do not match our records.");
                return View(model);
            }

            // Check if account is active
            if (user.Status != "active")
            {
                ModelState.AddModelError("", "Your account is not active yet. Please wait for approval or contact support.");
                return View(model);
            }

            // Sign in the user
            await SignInUserAsync(user, model.RememberMe);

            TempData["Success"] = "Login successful!";

            // Redirect based on role
            if (user.Role == "admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (user.Role == "tailor")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Tailor" });
            }
            else // client
            {
                return RedirectToAction("Index", "Guest");
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Success"] = "You have been logged out successfully.";

            return RedirectToAction("Index", "Guest");
        }

        // Helper method to sign in user
        private async Task SignInUserAsync(User user, bool rememberMe = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }

        // Helper method to upload profile picture
        private async Task<string?> UploadProfilePictureAsync(IFormFile? file, string firstName, string lastName)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // Validate file size (max 2MB)
                if (file.Length > 2 * 1024 * 1024)
                {
                    throw new InvalidOperationException("File size cannot exceed 2MB");
                }

                // Validate file extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Invalid file type. Only JPG, PNG, and GIF are allowed.");
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{firstName}-{lastName}-{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/profile-pictures/{fileName}";
            }
            catch (Exception)
            {
                // Log the error if you have logging configured
                return null;
            }
        }
    }
}