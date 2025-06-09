using crud_bank_api.Models;

namespace crud_bank_api.Validators
{
    /// <summary>
    /// Validation result containing validation state and error messages
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        
        public ValidationResult()
        {
            IsValid = true;
        }
        
        public ValidationResult(params string[] errors)
        {
            IsValid = false;
            Errors.AddRange(errors);
        }
        
        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }
        
        public void AddErrors(IEnumerable<string> errors)
        {
            IsValid = false;
            Errors.AddRange(errors);
        }
    }
    
    /// <summary>
    /// Base interface for validators
    /// </summary>
    /// <typeparam name="T">Type to validate</typeparam>
    public interface IValidator<T>
    {
        ValidationResult Validate(T item);
        Task<ValidationResult> ValidateAsync(T item);
    }
}
