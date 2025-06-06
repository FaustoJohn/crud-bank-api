using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using crud_bank_api.Models;
using crud_bank_api.Services;
using System.ComponentModel.DataAnnotations;

namespace crud_bank_api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        /// <summary>
        /// Get a user by email address
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User details</returns>
        [HttpGet("by-email")]
        public async Task<ActionResult<UserResponseDto>> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email parameter is required.");
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"User with email {email} not found.");
            }

            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user details</returns>
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            // Check if the request body is null
            if (createUserDto == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Additional validation
            var validationErrors = new List<string>();

            // Validate required fields explicitly
            if (string.IsNullOrWhiteSpace(createUserDto.FirstName))
            {
                validationErrors.Add("FirstName is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.LastName))
            {
                validationErrors.Add("LastName is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Email))
            {
                validationErrors.Add("Email is required and cannot be empty.");
            }
            else if (!IsValidEmail(createUserDto.Email))
            {
                validationErrors.Add("Email format is invalid.");
            }

            // Validate initial balance
            if (createUserDto.InitialBalance < 0)
            {
                validationErrors.Add("Initial balance cannot be negative.");
            }

            // Validate phone number if provided
            if (!string.IsNullOrWhiteSpace(createUserDto.PhoneNumber) && !IsValidPhoneNumber(createUserDto.PhoneNumber))
            {
                validationErrors.Add("Phone number format is invalid.");
            }

            // Return validation errors if any
            if (validationErrors.Any())
            {
                return BadRequest(new { 
                    message = "Validation failed",
                    errors = validationErrors 
                });
            }

            // Check if email already exists
            if (await _userService.EmailExistsAsync(createUserDto.Email))
            {
                return Conflict($"User with email {createUserDto.Email} already exists.");
            }

            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">User update data</param>
        /// <returns>Updated user details</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user exists
            if (!await _userService.UserExistsAsync(id))
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Check if email is being updated and already exists
            if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            {
                var existingUser = await _userService.GetUserByEmailAsync(updateUserDto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return Conflict($"User with email {updateUserDto.Email} already exists.");
                }
            }

            var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(updatedUser);
        }

        /// <summary>
        /// Soft delete a user (deactivate)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!await _userService.UserExistsAsync(id))
            {
                return NotFound($"User with ID {id} not found.");
            }

            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return NoContent();
        }

        /// <summary>
        /// Check if a user exists
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Boolean indicating if user exists</returns>
        [HttpHead("{id}")]
        public async Task<IActionResult> UserExists(int id)
        {
            var exists = await _userService.UserExistsAsync(id);
            return exists ? Ok() : NotFound();
        }

        /// <summary>
        /// Get user summary with additional metadata (V2 only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User summary with metadata</returns>
        [HttpGet("{id}/summary")]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<object>> GetUserSummary(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            var summary = new
            {
                User = user,
                Metadata = new
                {
                    AccountAge = DateTime.UtcNow - user.CreatedAt,
                    ApiVersion = "2.0",
                    LastAccessed = DateTime.UtcNow,
                    Features = new[] { "Enhanced User Data", "Metadata Support", "Account Analytics" }
                }
            };

            return Ok(summary);
        }

        /// <summary>
        /// Get users with pagination (V2 enhancement)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet("paginated")]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<object>> GetUsersPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var allUsers = await _userService.GetAllUsersAsync();
            var totalUsers = allUsers.Count();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            
            var pagedUsers = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var result = new
            {
                Data = pagedUsers,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalUsers = totalUsers,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                },
                ApiVersion = "2.0"
            };

            return Ok(result);
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
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
