using crud_bank_api.Models;

namespace crud_bank_api.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllActiveUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetActiveUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetActiveUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> AccountNumberExistsAsync(string accountNumber);
        Task<bool> UpdateUserPasswordAsync(int id, string passwordHash);
        Task<IEnumerable<User>> GetPaginatedActiveUsersAsync(int page, int pageSize);
        Task<int> GetActiveUserCountAsync();
    }
}
