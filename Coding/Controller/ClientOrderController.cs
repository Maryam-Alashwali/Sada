using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sada.Models;
using Sada.Data;
using Sada.ViewModels.Client;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sada.Controllers.Client
{
    [Authorize(Roles = "client")]
    [Area("Client")]
    public class ClientOrderController : Controller
    {
        private readonly SadaDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ClientOrderController> _logger;

        public ClientOrderController(
            SadaDbContext context,
            IWebHostEnvironment environment,
            ILogger<ClientOrderController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var pageSize = 10;
            var query = _context.Orders
                .Where(o => o.ClientId == client.ClientId)
                .Include(o => o.Tailor)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .Include(o => o.Payments)
                .Include(o => o.Reviews)
                .OrderByDescending(o => o.DateCreated);

            if (!string.IsNullOrEmpty(status))
            {
                query = (IOrderedQueryable<Order>)query.Where(o => o.OrderStatus == status);
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderItemViewModel
                {
                    OrderId = o.OrderId,
                    TailorName = $"{o.Tailor.TailorFirstName} {o.Tailor.TailorLastName}",
                    OrderStatus = o.OrderStatus,
                    TotalPrice = o.TotalPrice,
                    DateCreated = o.DateCreated,
                    Services = o.OrderServices.Select(os => new OrderServiceItemViewModel
                    {
                        ServiceName = os.Service.ServiceName ?? "",
                        Price = os.Price
                    }).ToList(),
                    HasPayment = o.Payments.Any(),
                    HasReview = o.Reviews.Any()
                })
                .ToListAsync();

            var statuses = new Dictionary<string, string>
            {
                { "requested", "Requested" },
                { "accepted", "Accepted" },
                { "in_progress", "In Progress" },
                { "completed", "Completed" },
                { "cancelled", "Cancelled" }
            };

            var viewModel = new OrderListViewModel
            {
                Orders = orders,
                Statuses = statuses,
                StatusFilter = status,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Show(int id)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = await _context.Orders
                .Include(o => o.Tailor)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .Include(o => o.Payments)
                .Include(o => o.Reviews)
                    .ThenInclude(r => r.Client)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.ClientId == client.ClientId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new OrderDetailViewModel
            {
                OrderId = order.OrderId,
                TailorName = $"{order.Tailor.TailorFirstName} {order.Tailor.TailorLastName}",
                TailorPhone = order.Tailor.TailorPhone ?? "",
                OrderStatus = order.OrderStatus,
                OrderAddress = order.OrderAddress,
                ClientNotes = order.ClientNotes,
                ClientUploadedImages = string.IsNullOrEmpty(order.ClientUploadedImage)
                    ? null
                    : JsonSerializer.Deserialize<List<string>>(order.ClientUploadedImage),
                TotalPrice = order.TotalPrice,
                PlatformCommission = order.PlatformCommission,
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
                    PaymentId = order.Payments.First().PaymentId,
                    PaymentAmount = order.Payments.First().PaymentAmount,
                    PaymentMethod = order.Payments.First().PaymentMethod,
                    PaymentStatus = order.Payments.First().PaymentStatus,
                    PaymentDate = order.Payments.First().PaymentDate,
                    PaymentTransactionId = order.Payments.First().PaymentTransactionId
                } : null,
                Reviews = order.Reviews.Select(r => new ReviewInfoViewModel
                {
                    ReviewId = r.ReviewId,
                    ClientName = $"{r.Client.ClientFirstName} {r.Client.ClientLastName}",
                    Rating = r.Rating,
                    Comment = r.Comment
                }).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Track(int id)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = await _context.Orders
                .Include(o => o.Tailor)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.ClientId == client.ClientId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var orderDetail = new OrderDetailViewModel
            {
                OrderId = order.OrderId,
                TailorName = $"{order.Tailor.TailorFirstName} {order.Tailor.TailorLastName}",
                OrderStatus = order.OrderStatus,
                TotalPrice = order.TotalPrice,
                DateCreated = order.DateCreated,
                Services = order.OrderServices.Select(os => new OrderServiceItemViewModel
                {
                    ServiceName = os.Service.ServiceName ?? "",
                    Price = os.Price
                }).ToList()
            };

            var viewModel = new OrderTrackViewModel
            {
                Order = orderDetail,
                StatusTimeline = GetStatusTimeline(order)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = await _context.Orders
                .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                .Include(o => o.Tailor)
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.ClientId == client.ClientId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new OrderDetailViewModel
            {
                OrderId = order.OrderId,
                TailorName = $"{order.Tailor.TailorFirstName} {order.Tailor.TailorLastName}",
                OrderStatus = order.OrderStatus,
                TotalPrice = order.TotalPrice,
                DateCreated = order.DateCreated,
                Services = order.OrderServices.Select(os => new OrderServiceItemViewModel
                {
                    ServiceName = os.Service.ServiceName ?? "",
                    Price = os.Price
                }).ToList(),
                Payment = order.Payments.FirstOrDefault() != null ? new PaymentInfoViewModel
                {
                    PaymentAmount = order.Payments.First().PaymentAmount,
                    PaymentMethod = order.Payments.First().PaymentMethod,
                    PaymentDate = order.Payments.First().PaymentDate,
                    PaymentTransactionId = order.Payments.First().PaymentTransactionId
                } : null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please provide a cancellation reason.";
                return RedirectToAction(nameof(Show), new { id = model.OrderId });
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.ClientId == client.ClientId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            if (order.OrderStatus != "requested" && order.OrderStatus != "accepted")
            {
                TempData["Error"] = "Order cannot be cancelled at this stage.";
                return RedirectToAction(nameof(Show), new { id = order.OrderId });
            }

            order.OrderStatus = "cancelled";
            order.ClientNotes = $"{order.ClientNotes}\n\nCancellation Reason: {model.CancellationReason}";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order has been cancelled successfully.";
            return RedirectToAction(nameof(Show), new { id = order.OrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(AddReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Show), new { id = model.OrderId });
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = await _context.Orders
                .Include(o => o.OrderServices)
                .Include(o => o.Reviews)
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.ClientId == client.ClientId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Reviews.Any(r => r.ClientId == client.ClientId))
            {
                TempData["Error"] = "You have already reviewed this order.";
                return RedirectToAction(nameof(Show), new { id = order.OrderId });
            }

            var adminId = await _context.Admins.Select(a => a.AdminId).FirstOrDefaultAsync();

            var review = new Review
            {
                ClientId = client.ClientId,
                ServiceId = order.OrderServices.FirstOrDefault()?.ServiceId ?? 0,
                OrderId = order.OrderId,
                AdminId = adminId > 0 ? adminId : 1,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewCreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction(nameof(Show), new { id = order.OrderId });
        }

        public async Task<IActionResult> Create(int? tailorId, int? serviceId)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!tailorId.HasValue)
            {
                TempData["Error"] = "Tailor not specified.";
                return RedirectToAction("Index", "Guest");
            }

            var tailor = await _context.Tailors
                .Include(t => t.Services)
                    .ThenInclude(s => s.Category)
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.TailorId == tailorId.Value);

            if (tailor == null)
            {
                TempData["Error"] = "Tailor not found.";
                return RedirectToAction("Index", "Guest");
            }

            var services = tailor.Services.Select(s => new ServiceSelectViewModel
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName ?? "",
                ServiceDescription = s.ServiceDescription,
                BasePrice = s.BasePrice,
                Duration = s.Duration,
                CategoryName = s.Category.Category1
            }).ToList();

            var measurements = await _context.Measurements
                .Where(m => m.ClientId == client.ClientId)
                .OrderByDescending(m => m.MeasurementId)
                .Select(m => new MeasurementViewModel
                {
                    MeasurementId = m.MeasurementId,
                    Chest = m.Chest,
                    Waist = m.Waist,
                    Hips = m.Hips,
                    Length = m.Length,
                    SleeveLength = m.SleeveLength,
                    OtherNotes = m.OtherNotes
                })
                .FirstOrDefaultAsync();

            ServiceSelectViewModel? selectedService = null;
            if (serviceId.HasValue)
            {
                var service = await _context.Services
                    .Include(s => s.Category)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId.Value);

                if (service != null)
                {
                    selectedService = new ServiceSelectViewModel
                    {
                        ServiceId = service.ServiceId,
                        ServiceName = service.ServiceName ?? "",
                        ServiceDescription = service.ServiceDescription,
                        BasePrice = service.BasePrice,
                        Duration = service.Duration,
                        CategoryName = service.Category.Category1
                    };
                }
            }

            var viewModel = new CreateOrderPageViewModel
            {
                Tailor = new TailorInfoViewModel
                {
                    TailorId = tailor.TailorId,
                    TailorFirstName = tailor.TailorFirstName ?? "",
                    TailorLastName = tailor.TailorLastName ?? "",
                    TailorPhone = tailor.TailorPhone,
                    TailorAddress = tailor.TailorAddress,
                    TailorProfilePicture = tailor.TailorProfilePicture,
                    Availabilities = tailor.Availabilities.Select(a => new AvailabilityViewModel
                    {
                        AvailabilityId = a.AvailabilityId,
                        DayOfWeek = a.DayOfWeek,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        IsAvailable = a.IsAvailable
                    }).ToList()
                },
                Services = services,
                Measurements = measurements,
                SelectedService = selectedService
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(CreateOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Create), new { tailorId = model.TailorId });
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var selectedServices = await _context.Services
                .Where(s => model.ServiceIds.Contains(s.ServiceId))
                .ToListAsync();

            var totalPrice = selectedServices.Sum(s => s.BasePrice ?? 0);
            var platformCommission = totalPrice * 0.10m;
            var tailorPayout = totalPrice - platformCommission;

            var imagePaths = new List<string>();
            if (model.ClientUploadedImages != null)
            {
                foreach (var image in model.ClientUploadedImages)
                {
                    var path = await UploadFileAsync(image, "orders");
                    if (path != null)
                    {
                        imagePaths.Add(path);
                    }
                }
            }

            var order = new Order
            {
                ClientId = client.ClientId,
                TailorId = model.TailorId,
                OrderStatus = "requested",
                OrderAddress = model.OrderAddress,
                ClientNotes = model.ClientNotes,
                ClientUploadedImage = imagePaths.Any() ? JsonSerializer.Serialize(imagePaths) : null,
                TotalPrice = totalPrice,
                PlatformCommission = platformCommission,
                TailorPayout = tailorPayout,
                ScheduledPick = model.ServiceType == "pickup" ? model.ScheduledPick : null,
                ScheduledVisitDate = model.ServiceType == "home_visit" ? model.ScheduledVisitDate : null,
                ServiceType = model.ServiceType,
                DateCreated = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add services to order
            foreach (var service in selectedServices)
            {
                var orderService = new OrderService
                {
                    OrderId = order.OrderId,
                    ServiceId = service.ServiceId,
                    Price = service.BasePrice,
                    Note = model.ClientNotes
                };
                _context.OrderServices.Add(orderService);
            }

            // Save measurements if provided
            if (model.MeasurementChest.HasValue || model.MeasurementWaist.HasValue || model.MeasurementHips.HasValue)
            {
                var measurement = new Measurement
                {
                    ClientId = client.ClientId,
                    Chest = model.MeasurementChest,
                    Waist = model.MeasurementWaist,
                    Hips = model.MeasurementHips,
                    Length = model.MeasurementLength,
                    SleeveLength = model.MeasurementSleeveLength,
                    OtherNotes = model.MeasurementNotes
                };
                _context.Measurements.Add(measurement);
            }

            // Create invoice
            var invoice = new Invoice
            {
                OrderId = order.OrderId,
                ClientId = client.ClientId,
                InvoiceTotalAmount = totalPrice * 1.15m, // Including 15% VAT
                PaymentStatus = "pending"
            };
            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order created successfully! Please complete the payment.";
            return RedirectToAction(nameof(Payment), new { id = order.OrderId });
        }

        public async Task<IActionResult> Payment(int id)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = await _context.Orders
                .Include(o => o.OrderServices)
                    .ThenInclude(os => os.Service)
                .Include(o => o.Tailor)
                .Include(o => o.Client)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.ClientId == client.ClientId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.OrderId == order.OrderId);

            var viewModel = new PaymentViewModel
            {
                Order = new OrderDetailViewModel
                {
                    OrderId = order.OrderId,
                    TailorName = $"{order.Tailor.TailorFirstName} {order.Tailor.TailorLastName}",
                    TotalPrice = order.TotalPrice,
                    Services = order.OrderServices.Select(os => new OrderServiceItemViewModel
                    {
                        ServiceName = os.Service.ServiceName ?? "",
                        Price = os.Price
                    }).ToList()
                },
                Invoice = invoice != null ? new InvoiceViewModel
                {
                    InvoiceId = invoice.InvoiceId,
                    OrderId = invoice.OrderId,
                    InvoiceTotalAmount = invoice.InvoiceTotalAmount,
                    PaymentStatus = invoice.PaymentStatus
                } : new InvoiceViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(ProcessPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors and try again.";
                return RedirectToAction(nameof(Payment), new { id = model.OrderId });
            }

            var client = await GetCurrentClientAsync();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.ClientId == client.ClientId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            // Simulate payment processing
            var paymentSuccess = SimulatePayment(model);

            if (paymentSuccess)
            {
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    ClientId = client.ClientId,
                    PaymentAmount = order.TotalPrice * 1.15m, // Including VAT
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "completed",
                    PaymentDate = DateTime.Now,
                    PaymentTransactionId = $"TXN_{DateTime.Now.Ticks}_{new Random().Next(1000, 9999)}"
                };

                _context.Payments.Add(payment);

                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.OrderId == order.OrderId);

                if (invoice != null)
                {
                    invoice.PaymentId = payment.PaymentId;
                    invoice.PaymentStatus = "paid";
                }

                order.OrderStatus = "accepted";

                await _context.SaveChangesAsync();

                TempData["Success"] = "Payment completed successfully! Your order has been confirmed.";
                return RedirectToAction(nameof(Show), new { id = order.OrderId });
            }
            else
            {
                TempData["Error"] = "Payment failed. Please try again or use a different payment method.";
                return RedirectToAction(nameof(Payment), new { id = order.OrderId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetTimeSlots([FromBody] TimeSlotRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new TimeSlotResponse
                {
                    Success = false,
                    Message = "Invalid request."
                });
            }

            var dayOfWeek = request.Date.ToString("dddd").ToLower();

            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.TailorId == request.TailorId &&
                                         a.DayOfWeek == dayOfWeek &&
                                         a.IsAvailable);

            if (availability == null)
            {
                return Json(new TimeSlotResponse
                {
                    Success = false,
                    Message = "Tailor is not available on this day",
                    Slots = new List<TimeSlot>()
                });
            }

            var availableSlots = GenerateTimeSlots(availability, request.ServiceType);
            var bookedSlots = await GetBookedSlots(request.TailorId, request.Date);

            var timeSlots = availableSlots.Select(slot => new TimeSlot
            {
                Time = slot,
                Available = !IsSlotBooked(slot, bookedSlots),
                Display = FormatTimeSlot(slot, request.ServiceType)
            }).ToList();

            return Json(new TimeSlotResponse
            {
                Success = true,
                Slots = timeSlots,
                Availability = new AvailabilityViewModel
                {
                    AvailabilityId = availability.AvailabilityId,
                    DayOfWeek = availability.DayOfWeek,
                    StartTime = availability.StartTime,
                    EndTime = availability.EndTime,
                    IsAvailable = availability.IsAvailable
                }
            });
        }

        #region File Upload Methods

        /// <summary>
        /// Upload a file with slug-based naming
        /// </summary>
        private async Task<string?> UploadFileAsync(IFormFile? file, string path = "uploads", string slug = "dummy-slug")
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // Create slug (replace spaces with hyphens, remove special chars)
                var cleanSlug = CreateSlug(slug);

                // Get current date
                var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // Get file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Generate unique filename
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var fileName = $"{cleanSlug}-{currentDate}-{uniqueId}{extension}";

                // Create full path
                var uploadPath = Path.Combine(_environment.WebRootPath, path);

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Full file path
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path
                return $"/{path}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                return null;
            }
        }

        /// <summary>
        /// Create a URL-friendly slug from text
        /// </summary>
        private string CreateSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "file";

            // Convert to lowercase
            text = text.ToLowerInvariant();

            // Replace spaces with hyphens
            text = text.Replace(" ", "-");

            // Remove invalid characters
            text = Regex.Replace(text, @"[^a-z0-9\-]", "");

            // Remove multiple consecutive hyphens
            text = Regex.Replace(text, @"-+", "-");

            // Trim hyphens from start and end
            text = text.Trim('-');

            return string.IsNullOrEmpty(text) ? "file" : text;
        }

        #endregion

        #region Helper Methods

        private async Task<Models.Client?> GetCurrentClientAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == userEmail);
        }

        private List<StatusTimelineItem> GetStatusTimeline(Order order)
        {
            var timeline = new List<StatusTimelineItem>
            {
                new StatusTimelineItem
                {
                    Status = "requested",
                    Label = "Order Requested",
                    Description = "Your order has been placed and is waiting for tailor acceptance",
                    Date = order.DateCreated,
                    Completed = true,
                    Active = order.OrderStatus == "requested"
                },
                new StatusTimelineItem
                {
                    Status = "accepted",
                    Label = "Order Accepted",
                    Description = "Tailor has accepted your order and will start working soon",
                    Date = order.OrderStatus == "accepted" ? DateTime.Now : null,
                    Completed = new[] { "accepted", "in_progress", "completed" }.Contains(order.OrderStatus ?? ""),
                    Active = order.OrderStatus == "accepted"
                },
                new StatusTimelineItem
                {
                    Status = "in_progress",
                    Label = "In Progress",
                    Description = "Tailor is currently working on your order",
                    Date = order.OrderStatus == "in_progress" ? DateTime.Now : null,
                    Completed = new[] { "in_progress", "completed" }.Contains(order.OrderStatus ?? ""),
                    Active = order.OrderStatus == "in_progress"
                },
                new StatusTimelineItem
                {
                    Status = "completed",
                    Label = "Completed",
                    Description = "Your order has been completed and delivered",
                    Date = order.OrderStatus == "completed" ? order.CompletionDate : null,
                    Completed = order.OrderStatus == "completed",
                    Active = order.OrderStatus == "completed"
                }
            };

            if (order.OrderStatus == "cancelled")
            {
                timeline.Add(new StatusTimelineItem
                {
                    Status = "cancelled",
                    Label = "Cancelled",
                    Description = "Order has been cancelled",
                    Date = DateTime.Now,
                    Completed = true,
                    Active = true
                });
            }

            return timeline;
        }

        private bool SimulatePayment(ProcessPaymentViewModel paymentData)
        {
            // Simulate payment processing - 90% success rate for demo
            return new Random().Next(1, 11) <= 9;
        }

        private List<TimeSlotTime> GenerateTimeSlots(Availability availability, string serviceType)
        {
            var slots = new List<TimeSlotTime>();

            if (availability.StartTime == null || availability.EndTime == null)
                return slots;

            var startTime = availability.StartTime.Value;
            var endTime = availability.EndTime.Value;

            var slotDuration = serviceType == "pickup" ? 60 : 120; // minutes
            var bufferTime = 30; // minutes

            var currentTime = startTime;
            while (currentTime < endTime)
            {
                var slotEnd = currentTime.AddMinutes(slotDuration);
                if (slotEnd <= endTime)
                {
                    slots.Add(new TimeSlotTime
                    {
                        Start = currentTime.ToString("HH:mm"),
                        End = slotEnd.ToString("HH:mm")
                    });
                }
                currentTime = currentTime.AddMinutes(slotDuration + bufferTime);
            }

            return slots;
        }

        private async Task<List<TimeSlotTime>> GetBookedSlots(int tailorId, DateTime date)
        {
            var orders = await _context.Orders
                .Where(o => o.TailorId == tailorId &&
                           (o.ScheduledPick.HasValue && o.ScheduledPick.Value.Date == date.Date ||
                            o.ScheduledVisitDate.HasValue && o.ScheduledVisitDate.Value.Date == date.Date) &&
                           new[] { "requested", "accepted", "in_progress" }.Contains(o.OrderStatus ?? ""))
                .ToListAsync();

            return orders.Select(o => new TimeSlotTime
            {
                Start = o.ScheduledPick.HasValue
                    ? o.ScheduledPick.Value.ToString("HH:mm")
                    : o.ScheduledVisitDate?.ToString("HH:mm") ?? "",
                End = "" // You might want to calculate end time based on service duration
            }).ToList();
        }

        private bool IsSlotBooked(TimeSlotTime slot, List<TimeSlotTime> bookedSlots)
        {
            return bookedSlots.Any(b => b.Start == slot.Start);
        }

        private string FormatTimeSlot(TimeSlotTime slot, string serviceType)
        {
            var start = DateTime.ParseExact(slot.Start, "HH:mm", null).ToString("h:mm tt");
            var end = DateTime.ParseExact(slot.End, "HH:mm", null).ToString("h:mm tt");
            return $"{start} - {end}";
        }

        #endregion
    }
}