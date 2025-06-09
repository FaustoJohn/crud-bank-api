using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using crud_bank_api.Models;
using crud_bank_api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace crud_bank_api.Controllers
{
    /// <summary>
    /// Handles authentication and authorization operations including login, registration, and user management
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <remarks>
        /// This endpoint authenticates a user using their email address and password.
        /// Upon successful authentication, it returns a JWT token that can be used
        /// for subsequent API calls requiring authentication.
        /// 
        /// Sample request:
        /// 
        ///     POST /v1/auth/login
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "email": "john.doe@example.com",
        ///         "password": "SecurePassword123!"
        ///     }
        /// 
        /// Sample successful response:
        /// 
        ///     {
        ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///         "expiresAt": "2025-06-09T15:30:00Z",
        ///         "user": {
        ///             "id": 1,
        ///             "firstName": "John",
        ///             "lastName": "Doe",
        ///             "email": "john.doe@example.com",
        ///             "accountNumber": "ACC123456",
        ///             "balance": 1000.00
        ///         }
        ///     }
        /// 
        /// </remarks>
        /// <param name="loginDto">Login credentials containing email and password</param>
        /// <returns>JWT token and user information upon successful authentication</returns>
        /// <response code="200">Returns the JWT token and user details successfully</response>
        /// <response code="400">If the request data is invalid or missing required fields</response>
        /// <response code="401">If the email or password is incorrect</response>
        /// <response code="422">If the request contains validation errors</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(result);
        }        /// <summary>
        /// Registers a new user account in the system
        /// </summary>
        /// <remarks>
        /// This endpoint creates a new user account with the provided registration information.
        /// The email must be unique and all required fields must be provided.
        /// Upon successful registration, the user is automatically logged in and receives a JWT token.
        /// 
        /// Sample request:
        /// 
        ///     POST /v1/auth/register
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "firstName": "John",
        ///         "lastName": "Doe",
        ///         "email": "john.doe@example.com",
        ///         "password": "SecurePassword123!",
        ///         "phoneNumber": "+1234567890"
        ///     }
        /// 
        /// </remarks>
        /// <param name="registerDto">Registration information for the new user</param>
        /// <returns>JWT token and user information for the newly registered user</returns>
        /// <response code="201">Returns the JWT token and user details for the new account</response>
        /// <response code="400">If the request data is invalid or validation fails</response>
        /// <response code="409">If a user with the specified email already exists</response>
        /// <response code="422">If the request contains validation errors</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
            {
                return Conflict("User with this email already exists.");
            }

            return CreatedAtAction(nameof(GetCurrentUser), null, result);
        }        /// <summary>
        /// Changes the password for the currently authenticated user
        /// </summary>
        /// <remarks>
        /// This endpoint allows authenticated users to change their password.
        /// The current password must be provided for verification before the new password is set.
        /// Requires valid JWT token authentication.
        /// 
        /// Sample request:
        /// 
        ///     POST /v1/auth/change-password
        ///     Authorization: Bearer {token}
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "currentPassword": "OldPassword123!",
        ///         "newPassword": "NewSecurePassword456!"
        ///     }
        /// 
        /// </remarks>
        /// <param name="changePasswordDto">Password change information including current and new passwords</param>
        /// <returns>Success confirmation message</returns>        /// <response code="200">Password changed successfully</response>
        /// <response code="400">If the request data is invalid or the current password is incorrect</response>
        /// <response code="401">If the user is not authenticated or the token is invalid</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _authService.ChangePasswordAsync(userId.Value, changePasswordDto);
            if (!success)
            {
                return BadRequest("Invalid current password or user not found.");
            }

            return Ok(new { message = "Password changed successfully." });
        }        /// <summary>
        /// Retrieves the current authenticated user's information
        /// </summary>
        /// <remarks>
        /// This endpoint returns detailed information about the currently authenticated user
        /// based on the JWT token provided in the Authorization header.
        /// 
        /// Sample request:
        /// 
        ///     GET /v1/auth/me
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <returns>Current user's detailed information</returns>        /// <response code="200">Returns the current user's details successfully</response>
        /// <response code="401">If the user is not authenticated or the token is invalid</response>
        /// <response code="404">If the authenticated user is not found in the system</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _authService.GetCurrentUserAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }        /// <summary>
        /// Logs out the current user (client-side token invalidation)
        /// </summary>
        /// <remarks>
        /// This endpoint provides a logout mechanism for the authenticated user.
        /// Since JWT tokens are stateless, actual logout is handled client-side
        /// by removing the token from storage. This endpoint serves as a confirmation
        /// and provides instructions for proper logout.
        /// 
        /// Sample request:
        /// 
        ///     POST /v1/auth/logout
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <returns>Success message with logout instructions</returns>        /// <response code="200">Logout successful with instructions</response>
        /// <response code="401">If the user is not authenticated or the token is invalid</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            // JWT tokens are stateless, so logout is typically handled client-side
            // by removing the token from storage
            return Ok(new { message = "Logged out successfully. Please remove the token from client storage." });
        }        /// <summary>
        /// Retrieves detailed authentication status and token information (API Version 2.0 only)
        /// </summary>
        /// <remarks>
        /// This endpoint provides comprehensive authentication status information including
        /// token details, claims, expiration, and security metadata. This is an enhanced
        /// endpoint available only in API version 2.0.
        /// 
        /// Sample request:
        /// 
        ///     GET /v2/auth/status
        ///     Authorization: Bearer {token}
        /// 
        /// </remarks>
        /// <returns>Detailed authentication status with token metadata</returns>        /// <response code="200">Returns comprehensive authentication status</response>
        /// <response code="401">If the user is not authenticated or the token is invalid</response>
        [HttpGet("status")]
        [Authorize]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetAuthStatus()
        {
            var userId = GetCurrentUserId();
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var tokenExp = User.FindFirst(ClaimTypes.Expiration)?.Value;
            
            var status = new
            {
                IsAuthenticated = true,
                UserId = userId,
                Email = userEmail,
                TokenExpiration = tokenExp,
                ApiVersion = "2.0",
                AuthenticationMethod = "JWT Bearer",
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            return Ok(status);
        }        /// <summary>
        /// Enhanced login with additional security features and metadata (API Version 2.0 only)
        /// </summary>
        /// <remarks>
        /// This endpoint provides enhanced authentication with additional security features
        /// and comprehensive metadata. It includes security information, login timestamps,
        /// and enhanced response data. This is an enhanced endpoint available only in API version 2.0.
        /// 
        /// Sample request:
        /// 
        ///     POST /v2/auth/login/enhanced
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "email": "john.doe@example.com",
        ///         "password": "SecurePassword123!"
        ///     }
        /// 
        /// </remarks>
        /// <param name="loginDto">Login credentials containing email and password</param>
        /// <returns>Enhanced JWT token response with comprehensive security metadata</returns>        /// <response code="200">Returns enhanced authentication response with security metadata</response>
        /// <response code="400">If the request data is invalid or missing required fields</response>
        /// <response code="401">If the email or password is incorrect</response>
        [HttpPost("login/enhanced")]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> EnhancedLogin([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Enhanced response with additional security metadata
            var enhancedResult = new
            {
                Token = result.Token,
                User = result.User,
                SecurityInfo = new
                {
                    LoginTime = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(1), // Assuming 1 hour expiration
                    TokenType = "Bearer",
                    ApiVersion = "2.0",
                    SecurityLevel = "Standard",
                    RequiresMFA = false, // Future enhancement
                    LastLoginAttempt = DateTime.UtcNow
                }
            };

            return Ok(enhancedResult);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
