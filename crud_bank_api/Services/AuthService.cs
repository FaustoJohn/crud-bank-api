using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using crud_bank_api.Models;
using Microsoft.Extensions.Options;
using BCrypt.Net;

namespace crud_bank_api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserService userService, IOptions<JwtSettings> jwtSettings)
        {
            _userService = userService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Get user by email
            var user = await _userService.GetUserByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return null;
            }

            // Get the full user entity to access password hash
            var userEntity = await _userService.GetUserEntityByEmailAsync(loginDto.Email);
            if (userEntity == null || !userEntity.IsActive)
            {
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, userEntity.PasswordHash))
            {
                return null;
            }

            // Generate JWT token
            var token = GenerateJwtToken(userEntity);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = user
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var userEntity = await _userService.GetUserEntityByIdAsync(userId);
            if (userEntity == null || !userEntity.IsActive)
            {
                return false;
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, userEntity.PasswordHash))
            {
                return false;
            }

            // Update password
            return await _userService.UpdateUserPasswordAsync(userId, changePasswordDto.NewPassword);
        }

        public async Task<UserResponseDto?> GetCurrentUserAsync(int userId)
        {
            return await _userService.GetUserByIdAsync(userId);
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new("AccountNumber", user.AccountNumber ?? ""),
                new("Balance", user.Balance.ToString("F2"))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
