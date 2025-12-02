using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Client
{
    public class ReviewListViewModel
    {
        public List<ReviewListItemViewModel> Reviews { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }

    public class ReviewListItemViewModel
    {
        public int ReviewId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewCreatedAt { get; set; }
    }

    public class UpdateReviewViewModel
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        [Display(Name = "Comment")]
        public string? Comment { get; set; }
    }
}
