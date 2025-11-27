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
    public class MessageController : Controller
    {
        private readonly SadaDbContext _context;

        public MessageController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var messages = await _context.Messages
                .Where(m => (m.SenderId == tailor.TailorId && m.SenderType == "tailor") ||
                           (m.ReceiverId == tailor.TailorId && m.ReceiverType == "tailor"))
                .ToListAsync();

            var conversations = messages
                .GroupBy(m => m.SenderType == "client" ? m.SenderId : m.ReceiverId)
                .Select(g => new ConversationViewModel
                {
                    ClientId = (int)(g.Key ?? 0),
                    LastMessageDate = g.Max(m => m.SentDate),
                    UnreadCount = g.Count(m => m.ReceiverId == tailor.TailorId && !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessageDate)
                .ToList();

            // Get client details
            var clientIds = conversations.Select(c => c.ClientId).ToList();
            var clients = await _context.Clients
                .Include(c => c.User)
                .Where(c => clientIds.Contains(c.ClientId))
                .ToDictionaryAsync(c => c.ClientId);

            foreach (var conversation in conversations)
            {
                if (clients.TryGetValue(conversation.ClientId, out var client))
                {
                    conversation.ClientName = $"{client.ClientFirstName} {client.ClientLastName}";
                    conversation.ClientEmail = client.User.Email;
                }
            }

            var viewModel = new MessageListViewModel
            {
                Conversations = conversations.Where(c => !string.IsNullOrEmpty(c.ClientName)).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Conversation(int clientId)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var client = await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null) return NotFound();

            var messages = await _context.Messages
                .Where(m => (m.SenderId == tailor.TailorId && m.SenderType == "tailor" &&
                            m.ReceiverId == clientId && m.ReceiverType == "client") ||
                           (m.SenderId == clientId && m.SenderType == "client" &&
                            m.ReceiverId == tailor.TailorId && m.ReceiverType == "tailor"))
                .OrderBy(m => m.SentDate)
                .Select(m => new MessageItemViewModel
                {
                    MessageId = m.MessageId,
                    MessageText = m.MessageText ?? "",
                    SentDate = m.SentDate,
                    SenderType = m.SenderType ?? "",
                    IsRead = m.IsRead,
                    IsFromTailor = m.SenderType == "tailor"
                })
                .ToListAsync();

            // Mark received messages as read
            await _context.Messages
                .Where(m => m.SenderId == clientId && m.SenderType == "client" &&
                           m.ReceiverId == tailor.TailorId && m.ReceiverType == "tailor" &&
                           !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

            var orders = await _context.Orders
                .Where(o => o.TailorId == tailor.TailorId && o.ClientId == clientId)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .Select(o => new ClientOrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderStatus = o.OrderStatus ?? "",
                    TotalPrice = o.TotalPrice,
                    Services = o.OrderServices.Select(os => os.Service.ServiceName ?? "").ToList()
                })
                .ToListAsync();

            var viewModel = new ConversationDetailViewModel
            {
                ClientId = clientId,
                ClientName = $"{client.ClientFirstName} {client.ClientLastName}",
                ClientEmail = client.User.Email,
                Messages = messages,
                Orders = orders
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(SendMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter a message.";
                return RedirectToAction(nameof(Conversation), new { clientId = model.ClientId });
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var message = new Message
            {
                SenderId = tailor.TailorId,
                SenderType = "tailor",
                ReceiverId = model.ClientId,
                ReceiverType = "client",
                MessageText = model.MessageText,
                SentDate = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);

            // Create notification for client
            var notification = new Notification
            {
                ClientId = model.ClientId,
                Message = $"New message from {tailor.TailorFirstName}",
                Date = DateTime.Now,
                Type = "message",
                Status = "unread"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = message });
            }

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction(nameof(Conversation), new { clientId = model.ClientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null)
                return Json(new { success = false, error = "Unauthorized" });

            var message = await _context.Messages.FindAsync(id);

            if (message == null ||
                message.ReceiverId != tailor.TailorId ||
                message.ReceiverType != "tailor")
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return Json(new { count = 0 });

            var count = await _context.Messages
                .CountAsync(m => m.ReceiverId == tailor.TailorId &&
                    m.ReceiverType == "tailor" && !m.IsRead);

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