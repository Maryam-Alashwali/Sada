namespace Sada.ViewModels.Admin
{
    public class RevenueReportViewModel
    {
        public RevenueReportStats Stats { get; set; } = new();
        public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
        public List<TopTailorRevenueData> TopTailors { get; set; } = new();
    }

    public class RevenueReportStats
    {
        public decimal TotalRevenue { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal TailorPayout { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
    }

    public class MonthlyRevenueData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }

    public class TopTailorRevenueData
    {
        public int TailorId { get; set; }
        public string TailorName { get; set; } = string.Empty;
        public int CompletedOrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class UserReportViewModel
    {
        public UserReportStats Stats { get; set; } = new();
        public List<UserRegistrationData> UserRegistration { get; set; } = new();
        public List<UsersByRoleData> UsersByRole { get; set; } = new();
        public List<RecentRegistrationData> RecentRegistrations { get; set; } = new();
    }

    public class UserReportStats
    {
        public int TotalUsers { get; set; }
        public int TotalTailors { get; set; }
        public int TotalClients { get; set; }
        public int ActiveUsers { get; set; }
        public int BlockedUsers { get; set; }
        public int PendingTailors { get; set; }
        public int ApprovedTailors { get; set; }
    }

    public class UserRegistrationData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }

    public class UsersByRoleData
    {
        public string Role { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecentRegistrationData
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
