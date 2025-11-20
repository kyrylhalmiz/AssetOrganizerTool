using System.Collections.Generic;

namespace Editor.AssetsOrganizer.Model
{
    public enum ValidationSeverity
    {
        None,
        Warning,
        Error
    }

    public class ValidationResult
    {
        public ValidationSeverity Severity;
        public string Message;
    }

    public static class GameItemValidator
    {
        public static List<ValidationResult> Validate(GameItemConfig item)
        {
            List<ValidationResult> results = new();

            if (string.IsNullOrWhiteSpace(item.DisplayName))
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Error,
                    Message = "Display Name is empty"
                });
            }

            if (item.Icon == null)
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Error,
                    Message = "Missing icon"
                });
            }

            if (item.Price <= 0)
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Warning,
                    Message = "Price is zero or negative"
                });
            }

            return results;
        }
    }
}