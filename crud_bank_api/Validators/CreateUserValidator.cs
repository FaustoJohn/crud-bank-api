using crud_bank_api.Models;
using crud_bank_api.Services;
using System.Net.Mail;

namespace crud_bank_api.Validators
{
    /// <summary>
    /// Validator for CreateUserDto with comprehensive business logic validation
    /// </summary>
    public class CreateUserValidator : IValidator<CreateUserDto>
    {
        private readonly IUserService _userService;

        public CreateUserValidator(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Synchronous validation for basic field validation
        /// </summary>
        /// <param name="createUserDto">The user creation data to validate</param>
        /// <returns>Validation result with any errors found</returns>
        public ValidationResult Validate(CreateUserDto createUserDto)
        {
            var result = new ValidationResult();

            if (createUserDto == null)
            {
                result.AddError("Request body cannot be null.");
                return result;
            }

            // Validate required fields explicitly
            if (string.IsNullOrWhiteSpace(createUserDto.FirstName))
            {
                result.AddError("FirstName is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.LastName))
            {
                result.AddError("LastName is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Email))
            {
                result.AddError("Email is required and cannot be empty.");
            }
            else if (!IsValidEmail(createUserDto.Email))
            {
                result.AddError("Email format is invalid.");
            }

            // Validate initial balance
            if (createUserDto.InitialBalance < 0)
            {
                result.AddError("Initial balance cannot be negative.");
            }

            // Validate phone number if provided
            if (!string.IsNullOrWhiteSpace(createUserDto.PhoneNumber) && !IsValidPhoneNumber(createUserDto.PhoneNumber))
            {
                result.AddError("Phone number format is invalid.");
            }

            return result;
        }

        /// <summary>
        /// Asynchronous validation including database checks
        /// </summary>
        /// <param name="createUserDto">The user creation data to validate</param>
        /// <returns>Validation result with any errors found</returns>
        public async Task<ValidationResult> ValidateAsync(CreateUserDto createUserDto)
        {
            var result = Validate(createUserDto);

            // If basic validation failed, don't proceed with async validations
            if (!result.IsValid)
                return result;

            // Check if email already exists (async validation)
            if (await _userService.EmailExistsAsync(createUserDto.Email))
            {
                result.AddError($"User with email {createUserDto.Email} already exists.");
            }

            return result;
        }

        /// <summary>
        /// Validates email format using System.Net.Mail.MailAddress
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates phone number format
        /// </summary>
        /// <param name="phoneNumber">Phone number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            // Remove common phone number formatting characters
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
            
            // Check if it contains only digits and has reasonable length (7-15 digits)
            return cleaned.All(char.IsDigit) && cleaned.Length >= 7 && cleaned.Length <= 15;
        }
    }
}
