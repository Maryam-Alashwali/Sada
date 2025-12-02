using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Client
{
    public class MeasurementViewModel
    {
        public int MeasurementId { get; set; }

        [Range(0, 999.99, ErrorMessage = "Chest measurement must be between 0 and 999.99")]
        [Display(Name = "Chest (cm)")]
        public decimal? Chest { get; set; }

        [Range(0, 999.99, ErrorMessage = "Waist measurement must be between 0 and 999.99")]
        [Display(Name = "Waist (cm)")]
        public decimal? Waist { get; set; }

        [Range(0, 999.99, ErrorMessage = "Hips measurement must be between 0 and 999.99")]
        [Display(Name = "Hips (cm)")]
        public decimal? Hips { get; set; }

        [Range(0, 999.99, ErrorMessage = "Length measurement must be between 0 and 999.99")]
        [Display(Name = "Length (cm)")]
        public decimal? Length { get; set; }

        [Range(0, 999.99, ErrorMessage = "Sleeve length must be between 0 and 999.99")]
        [Display(Name = "Sleeve Length (cm)")]
        public decimal? SleeveLength { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Additional Notes")]
        public string? OtherNotes { get; set; }
    }
}
