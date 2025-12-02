using Sada.ViewModels.Validation;
using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Tailor
{
    public class TailorProfileViewModel
    {
        public int TailorId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Display(Name = "First Name")]
        public string TailorFirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Display(Name = "Last Name")]
        public string TailorLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        [PhoneNumber] // ← Custom validation
        [Display(Name = "Phone Number")]
        public string TailorPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public string TailorAddress { get; set; } = string.Empty;

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }

        public string? ExistingProfilePicture { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "New Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public bool? IsApproved { get; set; }
    }
}
