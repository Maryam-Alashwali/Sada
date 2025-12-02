using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Client
{
    public class OrderListItemViewModel
    {
        public int OrderId { get; set; }
        public string TailorName { get; set; } = string.Empty;
        public string? OrderStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? DateCreated { get; set; }
        public List<string> Services { get; set; } = new();
        public bool CanCancel => OrderStatus == "requested" || OrderStatus == "accepted";
        public bool CanReview => OrderStatus == "completed";
    }

    public class OrderListViewModel
    {
        public List<OrderItemViewModel> Orders { get; set; } = new();
        public Dictionary<string, string> Statuses { get; set; } = new();
        public string? StatusFilter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }

    public class OrderItemViewModel
    {
        public int OrderId { get; set; }
        public string TailorName { get; set; } = string.Empty;
        public string? OrderStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? DateCreated { get; set; }
        public List<OrderServiceItemViewModel> Services { get; set; } = new();
        public bool HasPayment { get; set; }
        public bool HasReview { get; set; }

        // Add the missing properties
        public bool CanCancel => OrderStatus == "requested" || OrderStatus == "accepted";
        public bool CanReview => OrderStatus == "completed" && !HasReview;
    }

    public class OrderServiceItemViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public string TailorName { get; set; } = string.Empty;
        public string TailorPhone { get; set; } = string.Empty;
        public string? OrderStatus { get; set; }
        public string? OrderAddress { get; set; }
        public string? ClientNotes { get; set; }
        public List<string>? ClientUploadedImages { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? PlatformCommission { get; set; }
        public decimal? TailorPayout { get; set; }
        public DateTime? ScheduledPick { get; set; }
        public DateTime? ScheduledVisitDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? ServiceType { get; set; }
        public List<OrderServiceItemViewModel> Services { get; set; } = new();
        public PaymentInfoViewModel? Payment { get; set; }
        public List<ReviewInfoViewModel> Reviews { get; set; } = new();
    }

    public class PaymentInfoViewModel
    {
        public int PaymentId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentTransactionId { get; set; }
    }

    public class ReviewInfoViewModel
    {
        public int ReviewId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class OrderTrackViewModel
    {
        public OrderDetailViewModel Order { get; set; } = new();
        public List<StatusTimelineItem> StatusTimeline { get; set; } = new();
    }

    public class StatusTimelineItem
    {
        public string Status { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public bool Completed { get; set; }
        public bool Active { get; set; }
    }

    public class CancelOrderViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Cancellation reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [Display(Name = "Cancellation Reason")]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public class AddReviewViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        [Display(Name = "Comment")]
        public string? Comment { get; set; }
    }

    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Tailor is required")]
        public int TailorId { get; set; }

        [Required(ErrorMessage = "Please select at least one service")]
        [MinLength(1, ErrorMessage = "Please select at least one service")]
        public List<int> ServiceIds { get; set; } = new();

        [Required(ErrorMessage = "Order address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Delivery Address")]
        public string OrderAddress { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Additional Notes")]
        public string? ClientNotes { get; set; }

        [Display(Name = "Upload Images")]
        public List<IFormFile>? ClientUploadedImages { get; set; }

        [Display(Name = "Pickup Date")]
        public DateTime? ScheduledPick { get; set; }

        [Display(Name = "Visit Date")]
        public DateTime? ScheduledVisitDate { get; set; }

        [Required(ErrorMessage = "Service type is required")]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a time slot")]
        public string SelectedTimeSlot { get; set; } = string.Empty;

        // Measurement fields
        public decimal? MeasurementChest { get; set; }
        public decimal? MeasurementWaist { get; set; }
        public decimal? MeasurementHips { get; set; }
        public decimal? MeasurementLength { get; set; }
        public decimal? MeasurementSleeveLength { get; set; }
        public string? MeasurementNotes { get; set; }
    }

    public class CreateOrderPageViewModel
    {
        public TailorInfoViewModel Tailor { get; set; } = new();
        public List<ServiceSelectViewModel> Services { get; set; } = new();
        public MeasurementViewModel? Measurements { get; set; }
        public ServiceSelectViewModel? SelectedService { get; set; }
    }

    public class TailorInfoViewModel
    {
        public int TailorId { get; set; }
        public string TailorFirstName { get; set; } = string.Empty;
        public string TailorLastName { get; set; } = string.Empty;
        public string? TailorPhone { get; set; }
        public string? TailorAddress { get; set; }
        public string? TailorProfilePicture { get; set; }
        public string FullName => $"{TailorFirstName} {TailorLastName}";
        public List<AvailabilityViewModel> Availabilities { get; set; } = new();
    }

    public class ServiceSelectViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }
        public decimal? BasePrice { get; set; }
        public int? Duration { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class AvailabilityViewModel
    {
        public int AvailabilityId { get; set; }
        public string? DayOfWeek { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class PaymentViewModel
    {
        public OrderDetailViewModel Order { get; set; } = new();
        public InvoiceViewModel Invoice { get; set; } = new();
    }

    public class InvoiceViewModel
    {
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }
        public decimal? InvoiceTotalAmount { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class ProcessPaymentViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Card Number")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Card number must be 16 digits")]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiry Date")]
        public string? ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV must be 3 digits")]
        public string? Cvv { get; set; }

        [Display(Name = "Card Holder Name")]
        [StringLength(255)]
        public string? CardHolder { get; set; }
    }

    public class TimeSlotRequest
    {
        [Required]
        public int TailorId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string ServiceType { get; set; } = string.Empty;
    }

    public class TimeSlotResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<TimeSlot> Slots { get; set; } = new();
        public AvailabilityViewModel? Availability { get; set; }
    }

    public class TimeSlot
    {
        public TimeSlotTime Time { get; set; } = new();
        public bool Available { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    public class TimeSlotTime
    {
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
    }
}