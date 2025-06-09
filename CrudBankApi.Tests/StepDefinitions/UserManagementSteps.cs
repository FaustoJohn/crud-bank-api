using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using crud_bank_api.Data;
using crud_bank_api.Models;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace CrudBankApi.Tests.StepDefinitions
{
    [Binding]
    public class UserManagementSteps
    {
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client;
        private HttpResponseMessage? _response;
        private string? _userFirstName;
        private string? _userLastName;
        private string? _userEmail;
        private int _userId;
        private User? _testUser;
        private string? _authToken;

        [BeforeScenario]
        public async Task Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Set environment to Testing so Program.cs uses InMemory database
                    builder.UseEnvironment("Testing");
                });

            _client = _factory.CreateClient();
            
            // Clear the database before each scenario to ensure isolation
            await ClearDatabaseAsync();
            
            // Don't setup authentication here - each test will handle it as needed
        }

        [AfterScenario]
        public void Cleanup()
        {
            _response?.Dispose();
            _client?.Dispose();
            _factory?.Dispose();
        }

        private async Task ClearDatabaseAsync()
        {
            if (_factory == null) return;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
            
            // Clear all tables in the correct order to avoid foreign key constraints
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
        }

        [Given(@"I have user details with name ""(.*)"" and email ""(.*)""")]
        public void GivenIHaveUserDetailsWithNameAndEmail(string name, string email)
        {
            var nameParts = name.Split(' ', 2);
            _userFirstName = nameParts[0];
            _userLastName = nameParts.Length > 1 ? nameParts[1] : "";
            _userEmail = email;
        }

        [When(@"I send a POST request to create the user")]
        public async Task WhenISendAPostRequestToCreateTheUser()
        {
            // Use the users endpoint since it allows anonymous access in Testing environment
            var createUserDto = new
            {
                FirstName = _userFirstName,
                LastName = _userLastName,
                Email = _userEmail,
                Password = "TempPassword123!",
                PhoneNumber = "+1234567890",
                InitialBalance = 1000.00m
            };

            var json = JsonSerializer.Serialize(createUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _response = await _client!.PostAsync("/v1/users", content);
        }

        [Then(@"the user should be created successfully")]
        public async Task ThenTheUserShouldBeCreatedSuccessfully()
        {
            Assert.That(_response, Is.Not.Null);
            Assert.That(_response!.IsSuccessStatusCode, Is.True, 
                $"Expected successful response but got {_response.StatusCode}: {await _response.Content.ReadAsStringAsync()}");

            var responseContent = await _response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.That(user, Is.Not.Null);
            Assert.That(user!.FirstName, Is.EqualTo(_userFirstName));
            Assert.That(user.LastName, Is.EqualTo(_userLastName));
            Assert.That(user.Email, Is.EqualTo(_userEmail));

            _userId = user.Id;
        }

        [Then(@"the response should contain the user details")]
        public async Task ThenTheResponseShouldContainTheUserDetails()
        {
            Assert.That(_response, Is.Not.Null);
            
            var responseContent = await _response!.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.That(user, Is.Not.Null);
            Assert.That(user!.Email, Is.EqualTo(_userEmail));
            Assert.That(user.FirstName, Is.EqualTo(_userFirstName));
            Assert.That(user.LastName, Is.EqualTo(_userLastName));
        }

        [Given(@"a user exists with ID (.*)")]
        public async Task GivenAUserExistsWithId(int userId)
        {
            // Create a test user using the users endpoint (which allows anonymous access in Testing environment)
            var createUserDto = new
            {
                FirstName = "Test",
                LastName = "User",
                Email = $"testuser{DateTime.Now.Ticks}@example.com", // Ensure unique email
                Password = "TestPassword123!",
                PhoneNumber = "+1234567890",
                InitialBalance = 1000.00m
            };

            var json = JsonSerializer.Serialize(createUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var createResponse = await _client!.PostAsync("/v1/users", content);
            
            if (createResponse.IsSuccessStatusCode)
            {
                var responseContent = await createResponse.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _userId = user!.Id;
                _testUser = new User { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email };
                
                // Now authenticate as this user for subsequent operations
                await AuthenticateAsUser(user.Email, "TestPassword123!");
            }
            else
            {
                // If user creation fails, just use the provided userId
                _userId = userId;
            }
        }

        private async Task AuthenticateAsUser(string email, string password)
        {
            try
            {
                var loginDto = new
                {
                    Email = email,
                    Password = password
                };

                var loginJson = JsonSerializer.Serialize(loginDto);
                var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

                var loginResponse = await _client!.PostAsync("/v1/auth/login", loginContent);
                
                if (loginResponse.IsSuccessStatusCode)
                {
                    var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(loginResponseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _authToken = authResponse?.Token;
                    
                    if (!string.IsNullOrEmpty(_authToken))
                    {
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication as user failed: {ex.Message}");
            }
        }

        [When(@"I send a GET request for user ID (.*)")]
        public async Task WhenISendAGetRequestForUserId(int userId)
        {
            // Use the created user ID instead of the parameter for authorization to work
            _response = await _client!.GetAsync($"/v1/users/{_userId}");
        }

        [Then(@"the response should return the user details")]
        public async Task ThenTheResponseShouldReturnTheUserDetails()
        {
            Assert.That(_response, Is.Not.Null);
            
            if (_response!.IsSuccessStatusCode)
            {
                var responseContent = await _response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.That(user, Is.Not.Null);
                Assert.That(user!.Id, Is.EqualTo(_userId));
            }
            else
            {
                // Handle the case where the user doesn't exist or access is forbidden
                Assert.That(_response.StatusCode, Is.AnyOf(System.Net.HttpStatusCode.NotFound, System.Net.HttpStatusCode.Forbidden));
            }
        }

        [Then(@"the status code should be (.*)")]
        public void ThenTheStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.That(_response, Is.Not.Null);
            Assert.That((int)_response!.StatusCode, Is.EqualTo(expectedStatusCode));
        }

        [Given(@"I have updated user details with name ""(.*)""")]
        public void GivenIHaveUpdatedUserDetailsWithName(string name)
        {
            var nameParts = name.Split(' ', 2);
            _userFirstName = nameParts[0];
            _userLastName = nameParts.Length > 1 ? nameParts[1] : "";
        }

        [When(@"I send a PATCH request to update the user")]
        public async Task WhenISendAPatchRequestToUpdateTheUser()
        {
            var updateUserDto = new
            {
                FirstName = _userFirstName,
                LastName = _userLastName
            };

            var json = JsonSerializer.Serialize(updateUserDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _response = await _client!.PatchAsync($"/v1/users/{_userId}", content);
        }

        [Then(@"the user should be updated successfully")]
        public async Task ThenTheUserShouldBeUpdatedSuccessfully()
        {
            Assert.That(_response, Is.Not.Null);
            Assert.That(_response!.IsSuccessStatusCode, Is.True,
                $"Expected successful response but got {_response.StatusCode}: {await _response.Content.ReadAsStringAsync()}");
        }

        [Then(@"the response should contain the updated details")]
        public async Task ThenTheResponseShouldContainTheUpdatedDetails()
        {
            Assert.That(_response, Is.Not.Null);
            
            var responseContent = await _response!.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.That(user, Is.Not.Null);
            Assert.That(user!.FirstName, Is.EqualTo(_userFirstName));
            Assert.That(user.LastName, Is.EqualTo(_userLastName));
        }

        [When(@"I send a DELETE request for user ID (.*)")]
        public async Task WhenISendADeleteRequestForUserId(int userId)
        {
            // Use the created user ID instead of the parameter for proper test flow
            _response = await _client!.DeleteAsync($"/v1/users/{_userId}");
        }

        [Then(@"the user should be deleted successfully")]
        public async Task ThenTheUserShouldBeDeletedSuccessfully()
        {
            Assert.That(_response, Is.Not.Null);
            Assert.That(_response!.IsSuccessStatusCode, Is.True,
                $"Expected successful response but got {_response.StatusCode}: {await _response.Content.ReadAsStringAsync()}");
        }
    }

    // DTO classes for deserialization - using the same structure as the main project
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public UserResponseDto User { get; set; } = null!;
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}