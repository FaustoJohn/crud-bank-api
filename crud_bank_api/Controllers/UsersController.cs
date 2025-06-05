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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
    }
}
