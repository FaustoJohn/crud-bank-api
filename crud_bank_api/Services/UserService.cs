using crud_bank_api.Models;
using BCrypt.Net;

namespace crud_bank_api.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users;
        private int _nextId = 1;

        public UserService()
        {
            _users = new List<User>
            {
                new User
                {
                    Id = _nextId++,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    PhoneNumber = "+1234567890",
                    AccountNumber = GenerateAccountNumber(),
                    Balance = 1000.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new User
                {
                    Id = _nextId++,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    PhoneNumber = "+1987654321",
                    AccountNumber = GenerateAccountNumber(),
                    Balance = 2500.50m,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };
        }

        public Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = _users.Where(u => u.IsActive)
                              .Select(MapUserToResponseDto)
                              .ToList();
            return Task.FromResult<IEnumerable<UserResponseDto>>(users);
        }

        public Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            return Task.FromResult(user != null ? MapUserToResponseDto(user) : null);
        }

        public Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
            return Task.FromResult(user != null ? MapUserToResponseDto(user) : null);
        }

        public Task<User?> GetUserEntityByIdAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            return Task.FromResult(user);
        }

        public Task<User?> GetUserEntityByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
            return Task.FromResult(user);
        }

        public Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new User
            {
                Id = _nextId++,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                PasswordHash = !string.IsNullOrEmpty(createUserDto.Password) 
                    ? BCrypt.Net.BCrypt.HashPassword(createUserDto.Password)
                    : BCrypt.Net.BCrypt.HashPassword("defaultpassword123"), // Default password for admin-created users
                PhoneNumber = createUserDto.PhoneNumber,
                Balance = createUserDto.InitialBalance,
                AccountNumber = GenerateAccountNumber(),
                CreatedAt = DateTime.UtcNow
            };

            _users.Add(user);
            return Task.FromResult(MapUserToResponseDto(user));
        }

        public Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return Task.FromResult<UserResponseDto?>(null);

            if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
                user.FirstName = updateUserDto.FirstName;
            
            if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
                user.LastName = updateUserDto.LastName;
            
            if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
                user.Email = updateUserDto.Email;
            
            if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber))
                user.PhoneNumber = updateUserDto.PhoneNumber;
            
            if (updateUserDto.IsActive.HasValue)
                user.IsActive = updateUserDto.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<UserResponseDto?>(MapUserToResponseDto(user));
        }

        public Task<bool> UpdateUserPasswordAsync(int id, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            if (user == null)
                return Task.FromResult(false);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteUserAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return Task.FromResult(false);

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }

        public Task<bool> UserExistsAsync(int id)
        {
            return Task.FromResult(_users.Any(u => u.Id == id && u.IsActive));
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            return Task.FromResult(_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive));
        }

        private static UserResponseDto MapUserToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AccountNumber = user.AccountNumber,
                Balance = user.Balance,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive
            };
        }

        private static string GenerateAccountNumber()
        {
            var random = new Random();
            return $"ACC{random.Next(100000, 999999)}";
        }
    }
}
