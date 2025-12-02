using Sada.ViewModels.Validation;
using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Client
{
    public class ClientProfileViewModel
    {
        public int ClientId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ClientFirstName { get; set; } = string.Empty;
        public string ClientLastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ClientAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string FullName => $"{ClientFirstName} {ClientLastName}";
    }

    public class ClientProfilePageViewModel
    {
        public ClientProfileViewModel Client { get; set; } = new();
        public List<OrderListItemViewModel> Orders { get; set; } = new();
        public List<ReviewListItemViewModel> Reviews { get; set; } = new();
        public MeasurementViewModel? Measurements { get; set; }
        public string ActiveTab { get; set; } = "orders";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Display(Name = "First Name")]
        public string ClientFirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Display(Name = "Last Name")]
        public string ClientLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [PhoneNumber] // ← Custom validation
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string ClientAddress { get; set; } = string.Empty;
    }

    public class UpdatePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string NewPasswordConfirmed { get; set; } = string.Empty;
    }
}
