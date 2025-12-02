using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Tailor
{
    public class TailorOrderListViewModel
    {
        public List<TailorOrderItemViewModel> Orders { get; set; } = new();
        public TailorOrderStats Stats { get; set; } = new();
        public string CurrentStatus { get; set; } = "all";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class TailorOrderStats
    {
        public int All { get; set; }
        public int Requested { get; set; }
        public int Accepted { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class TailorOrderItemViewModel
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal? TotalPrice { get; set; }
        public DateTime? DateCreated { get; set; }
        public List<OrderServiceItemViewModel> Services { get; set; } = new();
    }

    public class OrderServiceItemViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
    }

    public class TailorOrderDetailViewModel
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string? OrderAddress { get; set; }
        public string? ClientNotes { get; set; }
        public string? ClientUploadedImage { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? TailorPayout { get; set; }
        public DateTime? ScheduledPick { get; set; }
        public DateTime? ScheduledVisitDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? ServiceType { get; set; }
        public List<OrderServiceItemViewModel> Services { get; set; } = new();
        public PaymentInfoViewModel? Payment { get; set; }
    }

    public class PaymentInfoViewModel
    {
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public decimal? PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class UpdateOrderStatusViewModel
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;

        public int OrderId { get; set; }
    }
}
