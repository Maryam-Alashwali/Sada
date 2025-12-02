namespace Sada.ViewModels.Tailor
{
    public class TailorDashboardViewModel
    {
        public TailorDashboardStats Stats { get; set; } = new();
        public List<TailorRecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<TailorRecentReviewViewModel> RecentReviews { get; set; } = new();
    }

    public class TailorDashboardStats
    {
        public int NewRequests { get; set; }
        public int ActiveOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        public double AverageRating { get; set; }
        public int TotalServices { get; set; }
        public int UnreadMessages { get; set; }
        public int CompletedOrders { get; set; } 
    }

    public class TailorRecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal? TotalPrice { get; set; }
        public DateTime? DateCreated { get; set; }
        public List<string> Services { get; set; } = new();
    }

    public class TailorRecentReviewViewModel
    {
        public int ReviewId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewCreatedAt { get; set; }
    }
}
