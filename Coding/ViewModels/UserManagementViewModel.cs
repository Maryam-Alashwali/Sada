using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Admin
{
    public class UserManagementViewModel
    {
        public List<UserItemViewModel> Users { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }

    public class UserItemViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool? IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class BlockUserRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }

    public class BulkActionRequest
    {
        [Required(ErrorMessage = "Please select at least one user")]
        [MinLength(1, ErrorMessage = "Please select at least one user")]
        public List<int> UserIds { get; set; } = new();

        [Required(ErrorMessage = "Action is required")]
        public string Action { get; set; } = string.Empty;
    }
}
