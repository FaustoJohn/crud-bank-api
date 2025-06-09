using crud_bank_api.Models;
using crud_bank_api.Services;
using System.Net.Mail;

namespace crud_bank_api.Validators
{
    /// <summary>
    /// Validator for UpdateUserDto with business logic validation
    /// </summary>
    public class UpdateUserValidator : IValidator<(int id, UpdateUserDto dto)>
    {
        private readonly IUserService _userService;

        public UpdateUserValidator(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Synchronous validation for basic field validation
        /// </summary>
        /// <param name="item">Tuple containing user ID and update data</param>
        /// <returns>Validation result with any errors found</returns>
        public ValidationResult Validate((int id, UpdateUserDto dto) item)
        {
            var result = new ValidationResult();
            var (id, updateUserDto) = item;

            if (updateUserDto == null)
            {
                result.AddError("Request body cannot be null.");
                return result;
            }

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && !IsValidEmail(updateUserDto.Email))
            {
                result.AddError("Email format is invalid.");
            }

            // Validate phone number format if provided
            if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber) && !IsValidPhoneNumber(updateUserDto.PhoneNumber))
            {
                result.AddError("Phone number format is invalid.");
            }

            // Validate first name if provided
            if (updateUserDto.FirstName != null && string.IsNullOrWhiteSpace(updateUserDto.FirstName))
            {
                result.AddError("FirstName cannot be empty if provided.");
            }

            // Validate last name if provided
            if (updateUserDto.LastName != null && string.IsNullOrWhiteSpace(updateUserDto.LastName))
            {
                result.AddError("LastName cannot be empty if provided.");
            }

            return result;
        }

        /// <summary>
        /// Asynchronous validation including database checks
        /// </summary>
        /// <param name="item">Tuple containing user ID and update data</param>
        /// <returns>Validation result with any errors found</returns>
        public async Task<ValidationResult> ValidateAsync((int id, UpdateUserDto dto) item)
        {
            var result = Validate(item);
            var (id, updateUserDto) = item;

            // If basic validation failed, don't proceed with async validations
            if (!result.IsValid)
                return result;

            // Check if user exists
            if (!await _userService.UserExistsAsync(id))
            {
                result.AddError($"User with ID {id} not found.");
                return result;
            }

            // Check if email is being updated and already exists
            if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            {
                var existingUser = await _userService.GetUserByEmailAsync(updateUserDto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    result.AddError($"User with email {updateUserDto.Email} already exists.");
                }
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
