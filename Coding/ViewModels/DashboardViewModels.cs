namespace Sada.ViewModels.Client
{
    public class ClientDashboardViewModel
    {
        public ClientProfileViewModel Client { get; set; } = new();
        public DashboardStatsViewModel Stats { get; set; } = new();
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<RecentReviewViewModel> RecentReviews { get; set; } = new();
        public MeasurementViewModel? LatestMeasurements { get; set; }
    }

    public class DashboardStatsViewModel
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int MeasurementsCount { get; set; }
    }

    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string TailorName { get; set; } = string.Empty;
        public string? OrderStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? DateCreated { get; set; }
        public List<string> Services { get; set; } = new();
    }

    public class RecentReviewViewModel
    {
        public int ReviewId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewCreatedAt { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OrderId { get; set; }
    }
}
