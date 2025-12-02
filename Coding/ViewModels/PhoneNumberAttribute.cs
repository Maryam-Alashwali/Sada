using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sada.ViewModels.Validation
{
    /// <summary>
    /// Validates Saudi phone numbers in format: +9665XXXXXXXX or 05XXXXXXXX
    /// </summary>
    public class PhoneNumberAttribute : ValidationAttribute
    {
        private const string Pattern = @"^((\+9665\d{8})|(05\d{8}))$";

        public PhoneNumberAttribute()
        {
            ErrorMessage = "{0} must be a valid phone number. Example: +966566193395 or 0566193395";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // If the field is not required, return success for null/empty values
                // If it's required, the [Required] attribute will handle it
                return ValidationResult.Success;
            }

            var phoneNumber = value.ToString();
            var regex = new Regex(Pattern);

            if (!regex.IsMatch(phoneNumber!))
            {
                var memberName = validationContext.MemberName ?? "Phone number";
                return new ValidationResult(
                    string.Format(ErrorMessage ?? "Invalid phone number", memberName),
                    new[] { validationContext.MemberName! }
                );
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage ?? "Invalid phone number", name);
        }
    }
}
