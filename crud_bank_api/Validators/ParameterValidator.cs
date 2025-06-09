namespace crud_bank_api.Validators
{
    /// <summary>
    /// Validator for basic parameter validation (email, string parameters, etc.)
    /// </summary>
    public class ParameterValidator
    {
        /// <summary>
        /// Validates email parameter
        /// </summary>
        /// <param name="email">Email parameter to validate</param>
        /// <returns>Validation result</returns>
        public static ValidationResult ValidateEmailParameter(string email)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(email))
            {
                result.AddError("Email parameter is required.");
            }

            return result;
        }

        /// <summary>
        /// Validates pagination parameters
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Validation result with normalized values</returns>
        public static (ValidationResult result, int normalizedPage, int normalizedPageSize) ValidatePaginationParameters(int page, int pageSize)
        {
            var result = new ValidationResult();
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedPageSize = pageSize < 1 || pageSize > 100 ? 10 : pageSize;

            // These are more like warnings/corrections than errors, so we don't add errors
            // The normalization handles invalid values automatically

            return (result, normalizedPage, normalizedPageSize);
        }

        /// <summary>
        /// Validates user ID parameter
        /// </summary>
        /// <param name="id">User ID to validate</param>
        /// <returns>Validation result</returns>
        public static ValidationResult ValidateUserId(int id)
        {
            var result = new ValidationResult();

            if (id <= 0)
            {
                result.AddError("User ID must be a positive integer.");
            }

            return result;
        }
    }
}
