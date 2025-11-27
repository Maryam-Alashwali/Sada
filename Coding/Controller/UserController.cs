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
    public class UserController : Controller
    {
        private readonly SadaDbContext _context;

        public UserController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? role, string? status, int page = 1)
        {
            var pageSize = 10;

            var query = _context.Users
                .Include(u => u.Tailors)
                .Include(u => u.Clients)
                .Where(u => u.Role != "admin");

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.Tailors.Any(t => (t.TailorFirstName != null && t.TailorFirstName.Contains(search)) ||
                                       (t.TailorLastName != null && t.TailorLastName.Contains(search))) ||
                    u.Clients.Any(c => (c.ClientFirstName != null && c.ClientFirstName.Contains(search)) ||
                                       (c.ClientLastName != null && c.ClientLastName.Contains(search)))
                );
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.Status == status);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Get paginated data
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to ViewModel (now we can use null-conditional operators safely)
            var userViewModels = users.Select(u => new UserItemViewModel
            {
                UserId = u.UserId,
                Email = u.Email,
                Role = u.Role,
                Status = u.Status ?? "active",
                FullName = u.Role == "tailor"
                    ? $"{u.Tailors.FirstOrDefault()?.TailorFirstName} {u.Tailors.FirstOrDefault()?.TailorLastName}"
                    : u.Role == "client"
                    ? $"{u.Clients.FirstOrDefault()?.ClientFirstName} {u.Clients.FirstOrDefault()?.ClientLastName}"
                    : "N/A",
                PhoneNumber = u.Role == "tailor"
                    ? u.Tailors.FirstOrDefault()?.TailorPhone ?? "N/A"
                    : u.Role == "client"
                    ? u.Clients.FirstOrDefault()?.PhoneNumber ?? "N/A"
                    : "N/A",
                IsApproved = u.Role == "tailor" ? u.Tailors.FirstOrDefault()?.IsApproved : null,
                CreatedAt = u.CreatedAt
            }).ToList();

            var viewModel = new UserManagementViewModel
            {
                Users = userViewModels,
                SearchTerm = search,
                RoleFilter = role,
                StatusFilter = status,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            var user = await _context.Users
                .Include(u => u.Tailors)
                .Include(u => u.Clients)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (user.Role == "admin")
            {
                return Json(new { success = false, message = "Cannot block admin users." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.Status = "blocked";

                var notification = new Notification
                {
                    Message = request.Reason ?? "Your account has been temporarily suspended due to violation of platform policies.",
                    Type = "account_blocked",
                    Status = "unread",
                    Date = DateTime.Now
                };

                switch (user.Role)
                {
                    case "client":
                        notification.ClientId = user.Clients.FirstOrDefault()?.ClientId;
                        break;
                    case "tailor":
                        notification.TailorId = user.Tailors.FirstOrDefault()?.TailorId;
                        var tailor = user.Tailors.FirstOrDefault();
                        if (tailor != null)
                        {
                            tailor.IsApproved = false;
                        }
                        break;
                }

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "User account has been blocked successfully.",
                    userStatus = user.Status
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred while blocking the user." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.Status = "active";

                var notification = new Notification
                {
                    Message = "Your account has been restored. You can now access all platform features.",
                    Type = "account_restored",
                    Status = "unread",
                    Date = DateTime.Now
                };

                switch (user.Role)
                {
                    case "client":
                        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
                        notification.ClientId = client?.ClientId;
                        break;
                    case "tailor":
                        var tailor = await _context.Tailors.FirstOrDefaultAsync(t => t.UserId == userId);
                        notification.TailorId = tailor?.TailorId;
                        break;
                }

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "User account has been unblocked successfully.",
                    userStatus = user.Status
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred while unblocking the user." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction([FromBody] BulkActionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            if (request.Action != "block" && request.Action != "unblock")
            {
                return Json(new { success = false, message = "Invalid action." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var userId in request.UserIds)
                {
                    var user = await _context.Users
                        .Include(u => u.Tailors)
                        .Include(u => u.Clients)
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (user != null && user.Role != "admin")
                    {
                        user.Status = request.Action == "block" ? "blocked" : "active";

                        var message = request.Action == "block"
                            ? "Your account has been suspended due to violation of platform policies."
                            : "Your account has been restored. Welcome back!";

                        var notification = new Notification
                        {
                            Message = message,
                            Type = request.Action == "block" ? "account_blocked" : "account_restored",
                            Status = "unread",
                            Date = DateTime.Now
                        };

                        switch (user.Role)
                        {
                            case "client":
                                notification.ClientId = user.Clients.FirstOrDefault()?.ClientId;
                                break;
                            case "tailor":
                                notification.TailorId = user.Tailors.FirstOrDefault()?.TailorId;
                                break;
                        }

                        _context.Notifications.Add(notification);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = $"{char.ToUpper(request.Action[0])}{request.Action.Substring(1)} action completed for selected users."
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred during bulk action." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTailor(int tailorId)
        {
            var tailor = await _context.Tailors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TailorId == tailorId);

            if (tailor == null)
            {
                return Json(new { success = false, message = "Tailor not found." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                tailor.IsApproved = true;

                var notification = new Notification
                {
                    TailorId = tailor.TailorId,
                    Message = "🎉 Your tailor account has been approved! You can now start accepting orders.",
                    Type = "account_approved",
                    Status = "unread",
                    Date = DateTime.Now
                };

                _context.Notifications.Add(notification);

                if (tailor.User.Status != "active")
                {
                    tailor.User.Status = "active";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Tailor account has been approved successfully.",
                    isApproved = true
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred while approving the tailor." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTailor(int tailorId, string? reason)
        {
            var tailor = await _context.Tailors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TailorId == tailorId);

            if (tailor == null)
            {
                return Json(new { success = false, message = "Tailor not found." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                tailor.IsApproved = false;

                if (tailor.User != null)
                {
                    tailor.User.Status = "blocked";
                }

                var notification = new Notification
                {
                    TailorId = tailor.TailorId,
                    Message = reason ?? "Your tailor account application has been rejected.",
                    Type = "account_rejected",
                    Status = "unread",
                    Date = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Tailor account has been rejected.",
                    isApproved = false
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred while rejecting the tailor." });
            }
        }
    }
}