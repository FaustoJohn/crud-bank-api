using crud_bank_api.Models;

namespace crud_bank_api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<UserResponseDto?> GetCurrentUserAsync(int userId);
        string GenerateJwtToken(User user);
    }
}
