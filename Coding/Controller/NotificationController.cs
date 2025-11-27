using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sada.Data;
using Sada.ViewModels.Tailor;

namespace Sada.Controllers.Tailor
{
    [Authorize(Roles = "tailor")]
    [Area("Tailor")]
    public class NotificationController : Controller
    {
        private readonly SadaDbContext _context;

        public NotificationController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var pageSize = 20;
            var skip = (page - 1) * pageSize;

            var totalNotifications = await _context.Notifications
                .CountAsync(n => n.TailorId == tailor.TailorId);

            var notifications = await _context.Notifications
                .Where(n => n.TailorId == tailor.TailorId)
                .OrderByDescending(n => n.Date)
                .Skip(skip)
                .Take(pageSize)
                .Select(n => new NotificationItemViewModel
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message ?? "",
                    Type = n.Type ?? "",
                    Status = n.Status ?? "",
                    Date = n.Date
                })
                .ToListAsync();

            // Mark all as read when viewing
            await _context.Notifications
                .Where(n => n.TailorId == tailor.TailorId && n.Status == "unread")
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Status, "read"));

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.TailorId == tailor.TailorId && n.Status == "unread");

            var viewModel = new NotificationListViewModel
            {
                Notifications = notifications,
                UnreadCount = unreadCount,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalNotifications / (double)pageSize)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return Json(new { success = false });

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.TailorId != tailor.TailorId)
            {
                return Json(new { success = false });
            }

            notification.Status = "read";
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return Json(new { success = false });

            await _context.Notifications
                .Where(n => n.TailorId == tailor.TailorId && n.Status == "unread")
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Status, "read"));

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return Json(new { count = 0 });

            var count = await _context.Notifications
                .CountAsync(n => n.TailorId == tailor.TailorId && n.Status == "unread");

            return Json(new { count });
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