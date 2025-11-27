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
    public class OrderController : Controller
    {
        private readonly SadaDbContext _context;

        public OrderController(SadaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var pageSize = 10;
            var skip = (page - 1) * pageSize;

            // FIXED: Properly chain the Include statements
            var query = _context.Orders
                .Where(o => o.TailorId == tailor.TailorId)
                .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .AsQueryable(); // This fixes the error

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            var totalOrders = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.DateCreated)
                .Skip(skip)
                .Take(pageSize)
                .Select(o => new TailorOrderItemViewModel
                {
                    OrderId = o.OrderId,
                    ClientName = $"{o.Client.ClientFirstName} {o.Client.ClientLastName}",
                    ClientEmail = o.Client.User.Email,
                    OrderStatus = o.OrderStatus ?? "Pending",
                    TotalPrice = o.TotalPrice,
                    DateCreated = o.DateCreated,
                    Services = o.OrderServices.Select(os => new OrderServiceItemViewModel
                    {
                        ServiceName = os.Service.ServiceName ?? "",
                        Price = os.Price
                    }).ToList()
                })
                .ToListAsync();

            var stats = new TailorOrderStats
            {
                All = await _context.Orders.CountAsync(o => o.TailorId == tailor.TailorId),
                Requested = await _context.Orders.CountAsync(o => o.TailorId == tailor.TailorId && o.OrderStatus == "requested"),
                Accepted = await _context.Orders.CountAsync(o => o.TailorId == tailor.TailorId && o.OrderStatus == "accepted"),
                InProgress = await _context.Orders.CountAsync(o => o.TailorId == tailor.TailorId && o.OrderStatus == "in_progress"),
                Completed = await _context.Orders.CountAsync(o => o.TailorId == tailor.TailorId && o.OrderStatus == "completed"),
                Cancelled = await _context.Orders.CountAsync(o => o.TailorId == tailor.TailorId && o.OrderStatus == "cancelled")
            };

            var viewModel = new TailorOrderListViewModel
            {
                Orders = orders,
                Stats = stats,
                CurrentStatus = status ?? "all",
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Show(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders
                .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.TailorId == tailor.TailorId);

            if (order == null) return NotFound();

            var viewModel = new TailorOrderDetailViewModel
            {
                OrderId = order.OrderId,
                ClientName = $"{order.Client.ClientFirstName} {order.Client.ClientLastName}",
                ClientEmail = order.Client.User.Email,
                ClientPhone = order.Client.PhoneNumber ?? "",
                OrderStatus = order.OrderStatus ?? "Pending",
                OrderAddress = order.OrderAddress,
                ClientNotes = order.ClientNotes,
                ClientUploadedImage = order.ClientUploadedImage,
                TotalPrice = order.TotalPrice,
                TailorPayout = order.TailorPayout,
                ScheduledPick = order.ScheduledPick,
                ScheduledVisitDate = order.ScheduledVisitDate,
                CompletionDate = order.CompletionDate,
                DateCreated = order.DateCreated,
                ServiceType = order.ServiceType,
                Services = order.OrderServices.Select(os => new OrderServiceItemViewModel
                {
                    ServiceName = os.Service.ServiceName ?? "",
                    Price = os.Price
                }).ToList(),
                Payment = order.Payments.FirstOrDefault() != null ? new PaymentInfoViewModel
                {
                    PaymentMethod = order.Payments.First().PaymentMethod,
                    PaymentStatus = order.Payments.First().PaymentStatus,
                    PaymentAmount = order.Payments.First().PaymentAmount,
                    PaymentDate = order.Payments.First().PaymentDate
                } : null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateOrderStatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(Show), new { id = model.OrderId });
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders
                .Include(o => o.Client)
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.TailorId == tailor.TailorId);

            if (order == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldStatus = order.OrderStatus;
                order.OrderStatus = model.Status;

                if (model.Status == "completed")
                {
                    order.CompletionDate = DateTime.Now;
                }

                var notification = new Notification
                {
                    ClientId = order.ClientId,
                    Message = $"Your order #ORD-{order.OrderId.ToString().PadLeft(4, '0')} status has been updated from {oldStatus?.Replace("_", " ")} to {model.Status.Replace("_", " ")}",
                    Date = DateTime.Now,
                    Type = "order_status",
                    Status = "unread"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Order status updated successfully!";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while updating the order.";
            }

            return RedirectToAction(nameof(Show), new { id = model.OrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.TailorId != tailor.TailorId) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                order.OrderStatus = "accepted";

                var notification = new Notification
                {
                    ClientId = order.ClientId,
                    Message = $"Your order #ORD-{order.OrderId.ToString().PadLeft(4, '0')} has been accepted by the tailor.",
                    Date = DateTime.Now,
                    Type = "order_accepted",
                    Status = "unread"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Order accepted successfully!";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred.";
            }

            return RedirectToAction(nameof(Show), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id)
        {
            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.TailorId != tailor.TailorId) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                order.OrderStatus = "cancelled";

                var notification = new Notification
                {
                    ClientId = order.ClientId,
                    Message = $"Your order #ORD-{order.OrderId.ToString().PadLeft(4, '0')} has been declined by the tailor.",
                    Date = DateTime.Now,
                    Type = "order_declined",
                    Status = "unread"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Order declined successfully!";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int orderId, string messageText)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Show), new { id = orderId });
            }

            var tailor = await GetCurrentTailorAsync();
            if (tailor == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.TailorId != tailor.TailorId) return NotFound();

            var message = new Message
            {
                SenderId = tailor.TailorId,
                SenderType = "tailor",
                ReceiverId = order.ClientId,
                ReceiverType = "client",
                MessageText = messageText,
                SentDate = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction(nameof(Show), new { id = orderId });
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