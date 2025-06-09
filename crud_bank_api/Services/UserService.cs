using crud_bank_api.Models;
using crud_bank_api.Repositories;
using BCrypt.Net;

namespace crud_bank_api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllActiveUsersAsync();
            return users.Select(u => MapUserToResponseDto(u));
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetActiveUserByIdAsync(id);
            return user != null ? MapUserToResponseDto(user) : null;
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetActiveUserByEmailAsync(email);
            return user != null ? MapUserToResponseDto(user) : null;
        }

        public async Task<User?> GetUserEntityByIdAsync(int id)
        {
            return await _userRepository.GetActiveUserByIdAsync(id);
        }

        public async Task<User?> GetUserEntityByEmailAsync(string email)
        {
            return await _userRepository.GetActiveUserByEmailAsync(email);
        }        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                PasswordHash = !string.IsNullOrEmpty(createUserDto.Password)
                    ? BCrypt.Net.BCrypt.HashPassword(createUserDto.Password)
                    : BCrypt.Net.BCrypt.HashPassword("defaultpassword123"),
                PhoneNumber = createUserDto.PhoneNumber,
                Balance = createUserDto.InitialBalance,
                AccountNumber = await GenerateUniqueAccountNumberAsync(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateUserAsync(user);
            
            return MapUserToResponseDto(createdUser);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                return null;

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

            var updatedUser = await _userRepository.UpdateUserAsync(user);
            
            return updatedUser != null ? MapUserToResponseDto(updatedUser) : null;
        }

        public async Task<bool> UpdateUserPasswordAsync(int id, string newPassword)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return await _userRepository.UpdateUserPasswordAsync(id, passwordHash);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _userRepository.UserExistsAsync(id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
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

        private async Task<string> GenerateUniqueAccountNumberAsync()
        {
            string accountNumber;
            bool exists;
            
            do
            {
                var random = new Random();
                accountNumber = $"ACC{random.Next(100000, 999999)}";
                exists = await _userRepository.AccountNumberExistsAsync(accountNumber);
            }
            while (exists);
            
            return accountNumber;
        }
    }
}
