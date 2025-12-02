using Sada.ViewModels.Validation;
using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Admin
{
    public class AdvertisementViewModel
    {
        public int AdsId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Advertisement Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(255, ErrorMessage = "Content cannot exceed 255 characters")]
        [Display(Name = "Advertisement Content")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Tailor")]
        public int? TailorId { get; set; }

        [Display(Name = "Advertisement Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImage { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Date")]
        [DateGreaterThan("StartDate", ErrorMessage = "End date must be after start date")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        public string? TailorName { get; set; }
    }

    public class AdvertisementListViewModel
    {
        public List<AdvertisementViewModel> Advertisements { get; set; } = new();
        public List<TailorSelectItem> Tailors { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }

    public class TailorSelectItem
    {
        public int TailorId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

}
