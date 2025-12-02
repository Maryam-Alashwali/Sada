using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Tailor
{
    public class TailorServiceListViewModel
    {
        public List<TailorServiceItemViewModel> Services { get; set; } = new();
        public List<CategorySelectItem> Categories { get; set; } = new();
    }

    public class TailorServiceItemViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; } 
        public decimal? BasePrice { get; set; }
        public int? Duration { get; set; }
        public int ReviewCount { get; set; }
        public double? AverageRating { get; set; }
        public int OrderCount { get; set; } 
    }

    public class TailorServiceFormViewModel
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string ServiceDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required")]
        [Range(0, 999999, ErrorMessage = "Price must be between 0 and 999,999")]
        [Display(Name = "Base Price")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 10000, ErrorMessage = "Duration must be at least 1 minute")]
        [Display(Name = "Duration (minutes)")]
        public int Duration { get; set; }
    }

    public class CategorySelectItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
