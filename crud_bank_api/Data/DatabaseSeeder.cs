using Microsoft.EntityFrameworkCore;
using crud_bank_api.Data;
using crud_bank_api.Models;
using BCrypt.Net;

namespace crud_bank_api.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(BankDbContext context)
        {
            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Seed initial users
            var users = new List<User>
            {
                new User
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    PhoneNumber = "+1234567890",
                    AccountNumber = GenerateAccountNumber(),
                    Balance = 1000.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsActive = true
                },
                new User
                {
                    FirstName = "Jane",
                    LastName = "Smith", 
                    Email = "jane.smith@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    PhoneNumber = "+1987654321",
                    AccountNumber = GenerateAccountNumber(),
                    Balance = 2500.50m,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    IsActive = true
                },
                new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@crudbank.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    PhoneNumber = "+1555000000",
                    AccountNumber = GenerateAccountNumber(),
                    Balance = 10000.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    IsActive = true
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static string GenerateAccountNumber()
        {
            var random = new Random();
            return $"ACC{random.Next(100000, 999999)}";
        }
    }
}
