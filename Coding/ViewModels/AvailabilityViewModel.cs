using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Tailor
{
    public class AvailabilityViewModel
    {
        public int AvailabilityId { get; set; }

        [Required(ErrorMessage = "Day of week is required")]
        [Display(Name = "Day of Week")]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        [TimeGreaterThan("StartTime", ErrorMessage = "End time must be after start time")]
        public TimeOnly EndTime { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;
    }

    public class AvailabilityListViewModel
    {
        public Dictionary<string, List<AvailabilityViewModel>> AvailabilitiesByDay { get; set; } = new();
        public List<string> DaysOfWeek { get; set; } = new()
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };
    }

    public class BulkAvailabilityUpdateViewModel
    {
        [Required(ErrorMessage = "Day of week is required")]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required(ErrorMessage = "Availability status is required")]
        public bool IsAvailable { get; set; }
    }

    // Custom validation attribute for TimeOnly
    public class TimeGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public TimeGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = (TimeOnly?)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = (TimeOnly?)property.GetValue(validationContext.ObjectInstance);

            if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
            {
                return new ValidationResult(ErrorMessage ?? $"Time must be greater than {_comparisonProperty}");
            }

            return ValidationResult.Success;
        }
    }
}
