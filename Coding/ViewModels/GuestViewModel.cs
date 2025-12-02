// Remove the line: using Sada.ViewModels.Tailor;
// Everything needed is now in this file

namespace Sada.ViewModels.Guest
{
    public class HomeViewModel
    {
        public List<FeaturedTailorViewModel> FeaturedTailors { get; set; } = new();
        public List<AdvertisementViewModel> Advertisements { get; set; } = new();
        public List<CategoryWithCountViewModel> Categories { get; set; } = new();
    }

    public class FeaturedTailorViewModel
    {
        public int TailorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int ServiceCount { get; set; }
    }

    public class AdvertisementViewModel
    {
        public int AdsId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? TailorName { get; set; }
    }

    public class CategoryWithCountViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
    }

    public class TailorSearchViewModel
    {
        public List<TailorListItemViewModel> Tailors { get; set; } = new();
        public List<CategoryWithCountViewModel> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? Location { get; set; }
        public int? CategoryId { get; set; }
        public double? MinRating { get; set; }
        public string SortBy { get; set; } = "rating";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class TailorListItemViewModel
    {
        public int TailorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int ServiceCount { get; set; }
        public int CompletedOrdersCount { get; set; }
        public List<string> ServiceNames { get; set; } = new();
    }

    public class TailorProfileViewModel
    {
        public int TailorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<TailorServiceViewModel> Services { get; set; } = new();
        public List<TailorReviewViewModel> Reviews { get; set; } = new();
        public List<AvailabilityViewModel> Availabilities { get; set; } = new();
    }

    public class TailorServiceViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal? BasePrice { get; set; }
        public int? Duration { get; set; }
        public double? AverageRating { get; set; }
    }

    public class TailorReviewViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewCreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for displaying tailor availability to guests (read-only)
    /// Uses nullable TimeOnly to handle cases where times might not be set
    /// </summary>
    public class AvailabilityViewModel
    {
        public int AvailabilityId { get; set; }
        public string? DayOfWeek { get; set; }

        /// <summary>
        /// Nullable to handle cases where availability exists but times aren't set
        /// </summary>
        public TimeOnly? StartTime { get; set; }

        /// <summary>
        /// Nullable to handle cases where availability exists but times aren't set
        /// </summary>
        public TimeOnly? EndTime { get; set; }

        public bool IsAvailable { get; set; }
    }
}