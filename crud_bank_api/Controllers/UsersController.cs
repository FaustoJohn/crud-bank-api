

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using crud_bank_api.Models;
using crud_bank_api.Services;
using crud_bank_api.Validators;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace crud_bank_api.Controllers
{
    /// <summary>
    /// Manages user operations including CRUD operations, authentication, and user management
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Authorize] // Require authentication for all endpoints
    [Produces("application/json")]
    [Consumes("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<CreateUserDto> _createUserValidator;
        private readonly IValidator<(int, UpdateUserDto)> _updateUserValidator;

        public UsersController(
            IUserService userService,
            IValidator<CreateUserDto> createUserValidator,
            IValidator<(int, UpdateUserDto)> updateUserValidator)
        {
            _userService = userService;
            _createUserValidator = createUserValidator;
            _updateUserValidator = updateUserValidator;
        }

        /// <summary>
        /// Retrieves all active users in the system
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all active users in the system. 
        /// Inactive or deleted users are not included in the response.
        /// 
        /// Sample request:
        /// 
        ///     GET /v1/users
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <returns>A list of all active users</returns>
        /// <response code="200">Returns the list of users successfully</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by their unique identifier
        /// </summary>
        /// <remarks>
        /// This endpoint returns detailed information about a specific user identified by their ID.
        /// The user must be active (not deleted) to be returned.
        /// Users can only retrieve their own details - attempting to access another user's details will result in a 403 Forbidden response.
        /// 
        /// Sample request:
        /// 
        ///     GET /v1/users/123
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <param name="id">The unique identifier of the user to retrieve</param>
        /// <returns>Detailed information about the specified user</returns>        /// <response code="200">Returns the user details successfully</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user attempts to access another user's details</response>
        /// <response code="404">If the user with the specified ID is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            // Check if the authenticated user is trying to access their own details
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }
            
            if (currentUserId.Value != id)
            {
                return Forbid("You can only access your own user details.");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <remarks>
        /// This endpoint allows searching for a user using their email address.
        /// The email comparison is case-insensitive and the user must be active.
        /// 
        /// Sample request:
        /// 
        ///     GET /v1/users/by-email?email=john.doe@example.com
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <param name="email">The email address of the user to search for</param>
        /// <returns>User details matching the specified email</returns>        /// <response code="200">Returns the user details successfully</response>
        /// <response code="400">If the email parameter is missing or invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no user with the specified email is found</response>
        [HttpGet("by-email")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetUserByEmail([FromQuery] string email)
        {
            var validationResult = ParameterValidator.ValidateEmailParameter(email);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.First());
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"User with email {email} not found.");
            }

            return Ok(user);
        }

        /// <summary>
        /// Creates a new user account in the system
        /// </summary>
        /// <remarks>
        /// This endpoint creates a new user with the provided information. 
        /// All required fields must be provided and the email must be unique.
        /// The initial balance, if provided, cannot be negative.
        /// 
        /// Sample request:
        /// 
        ///     POST /v1/users
        ///     Authorization: Bearer {token}
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "firstName": "John",
        ///         "lastName": "Doe",
        ///         "email": "john.doe@example.com",
        ///         "phoneNumber": "+1234567890",
        ///         "initialBalance": 1000.00
        ///     }
        /// 
        /// </remarks>
        /// <param name="createUserDto">The user creation data containing all required information</param>
        /// <returns>Details of the newly created user</returns>
        /// <response code="201">Returns the newly created user details</response>
        /// <response code="400">If the request data is invalid or validation fails</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="409">If a user with the specified email already exists</response>
        /// <response code="422">If the request contains validation errors</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Use validator for comprehensive validation
            var validationResult = await _createUserValidator.ValidateAsync(createUserDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { 
                    message = "Validation failed",
                    errors = validationResult.Errors 
                });
            }

            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Updates an existing user's information
        /// </summary>
        /// <remarks>
        /// This endpoint allows updating user information for an existing user.
        /// Only provided fields will be updated; omitted fields will remain unchanged.
        /// The email must be unique if being updated.
        /// 
        /// Sample request:
        /// 
        ///     PUT /v1/users/123
        ///     Authorization: Bearer {token}
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "firstName": "Jane",
        ///         "lastName": "Smith",
        ///         "phoneNumber": "+1987654321"
        ///     }
        /// 
        /// </remarks>
        /// <param name="id">The unique identifier of the user to update</param>
        /// <param name="updateUserDto">The user update data containing the fields to modify</param>
        /// <returns>Details of the updated user</returns>        /// <response code="200">Returns the updated user details</response>
        /// <response code="400">If the request data is invalid or validation fails</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user with the specified ID is not found</response>
        /// <response code="409">If the new email already exists for another user</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Use validator for comprehensive validation
            var validationResult = await _updateUserValidator.ValidateAsync((id, updateUserDto));
            if (!validationResult.IsValid)
            {
                // Check if the error is about user not found (404) or email conflict (409)
                if (validationResult.Errors.Any(e => e.Contains("not found")))
                {
                    return NotFound(validationResult.Errors.First(e => e.Contains("not found")));
                }
                if (validationResult.Errors.Any(e => e.Contains("already exists")))
                {
                    return Conflict(validationResult.Errors.First(e => e.Contains("already exists")));
                }
                
                return BadRequest(new { 
                    message = "Validation failed",
                    errors = validationResult.Errors 
                });
            }

            var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(updatedUser);
        }

        /// <summary>
        /// Soft deletes a user (marks as inactive rather than permanent deletion)
        /// </summary>
        /// <remarks>
        /// This endpoint performs a soft delete operation, marking the user as inactive
        /// rather than permanently removing them from the database. This preserves
        /// data integrity and allows for potential recovery.
        /// 
        /// Sample request:
        /// 
        ///     DELETE /v1/users/123
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <param name="id">The unique identifier of the user to delete</param>
        /// <returns>No content on successful deletion</returns>        /// <response code="204">User successfully marked as deleted</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user with the specified ID is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// Checks if a user exists in the system
        /// </summary>
        /// <remarks>
        /// This endpoint performs a lightweight check to determine if a user
        /// with the specified ID exists and is active. Returns HTTP status
        /// codes without a response body for efficiency.
        /// 
        /// Sample request:
        /// 
        ///     HEAD /v1/users/123
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <param name="id">The unique identifier of the user to check</param>
        /// <returns>HTTP status indicating existence</returns>        /// <response code="200">User exists and is active</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user with the specified ID is not found</response>
        [HttpHead("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserExists(int id)
        {
            var exists = await _userService.UserExistsAsync(id);
            return exists ? Ok() : NotFound();
        }

        /// <summary>
        /// Retrieves enhanced user summary with additional metadata (API Version 2.0 only)
        /// </summary>
        /// <remarks>
        /// This endpoint provides comprehensive user information along with additional
        /// metadata such as account age, API version details, and enhanced features.
        /// This is an enhanced endpoint available only in API version 2.0.
        /// 
        /// Sample request:
        /// 
        ///     GET /v2/users/123/summary
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>User details with enhanced metadata</returns>        /// <response code="200">Returns the user summary with metadata successfully</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user with the specified ID is not found</response>
        [HttpGet("{id}/summary")]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// Retrieves users with pagination support (API Version 2.0 only)
        /// </summary>
        /// <remarks>
        /// This endpoint provides paginated access to users with enhanced metadata.
        /// It includes pagination information such as total pages, current page,
        /// and navigation indicators. This is an enhanced endpoint available only in API version 2.0.
        /// 
        /// Sample request:
        /// 
        ///     GET /v2/users/paginated?page=1&amp;pageSize=10
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <param name="page">Page number (minimum: 1, default: 1)</param>
        /// <param name="pageSize">Number of users per page (minimum: 1, maximum: 100, default: 10)</param>
        /// <returns>Paginated list of users with navigation metadata</returns>        /// <response code="200">Returns the paginated user list successfully</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("paginated")]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> GetUsersPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (validationResult, normalizedPage, normalizedPageSize) = ParameterValidator.ValidatePaginationParameters(page, pageSize);
            
            var allUsers = await _userService.GetAllUsersAsync();
            var totalUsers = allUsers.Count();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)normalizedPageSize);
            
            var pagedUsers = allUsers
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize);

            var result = new
            {
                Data = pagedUsers,
                Pagination = new
                {
                    CurrentPage = normalizedPage,
                    PageSize = normalizedPageSize,
                    TotalPages = totalPages,
                    TotalUsers = totalUsers,
                    HasNextPage = normalizedPage < totalPages,
                    HasPreviousPage = normalizedPage > 1
                },
                ApiVersion = "2.0"
            };

            return Ok(result);
        }

        /// <summary>
        /// Gets the current authenticated user's ID from JWT claims
        /// </summary>
        /// <returns>The authenticated user's ID, or null if not found</returns>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
