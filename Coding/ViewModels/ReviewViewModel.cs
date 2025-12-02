namespace Sada.ViewModels.Tailor
{
    public class TailorReviewListViewModel
    {
        public List<TailorReviewItemViewModel> Reviews { get; set; } = new();
        public TailorReviewStats Stats { get; set; } = new();
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class TailorReviewStats
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int OneStarCount { get; set; }
    }

    public class TailorReviewItemViewModel
    {
        public int ReviewId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewCreatedAt { get; set; }
        public int OrderId { get; set; }
    }

    public class TailorReviewDetailViewModel
    {
        public int ReviewId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewCreatedAt { get; set; }
        public int OrderId { get; set; }
        public List<OrderServiceItemViewModel> OrderServices { get; set; } = new();
    }
}
