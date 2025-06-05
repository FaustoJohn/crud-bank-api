# CRUD Bank API - User Operations

This .NET 8 Web API provides comprehensive user management operations for a banking application.

## Features

### User Controller (`/api/users`)

The `UsersController` provides full CRUD operations for user management:

#### Endpoints

- **GET /api/users** - Get all active users
- **GET /api/users/{id}** - Get a specific user by ID
- **GET /api/users/by-email?email={email}** - Get user by email address
- **POST /api/users** - Create a new user
- **PUT /api/users/{id}** - Update an existing user
- **DELETE /api/users/{id}** - Soft delete a user (deactivate)
- **HEAD /api/users/{id}** - Check if a user exists

### User Model

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public string? AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}
```

### Data Transfer Objects (DTOs)

#### CreateUserDto
Used for creating new users:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "initialBalance": 1000.00
}
```

#### UpdateUserDto
Used for updating existing users (all fields optional):
```json
{
  "firstName": "John Updated",
  "lastName": "Doe Updated",
  "email": "john.updated@example.com",
  "phoneNumber": "+1999888777",
  "isActive": true
}
```

#### UserResponseDto
Returned by API endpoints:
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "accountNumber": "ACC123456",
  "balance": 1000.00,
  "createdAt": "2025-06-05T19:30:00Z",
  "updatedAt": null,
  "isActive": true
}
```

## Architecture

### Services Layer
- **IUserService** - Interface defining user operations
- **UserService** - In-memory implementation of user operations
- Dependency injection configured in `Program.cs`

### Features
- **Validation** - Model validation using Data Annotations
- **Soft Delete** - Users are deactivated instead of permanently deleted
- **Email Uniqueness** - Prevents duplicate email addresses
- **Auto-generated Account Numbers** - Unique account numbers for banking operations
- **Swagger Documentation** - Interactive API documentation at `/swagger`

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### Running the Application

1. Clone the repository
2. Navigate to the project directory:
   ```bash
   cd crud_bank_api
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. Access the API:
   - Swagger UI: http://localhost:5134/swagger
   - API Base URL: http://localhost:5134/api

### Sample Data

The application starts with two sample users:
1. John Doe (john.doe@example.com) - Balance: $1,000.00
2. Jane Smith (jane.smith@example.com) - Balance: $2,500.50

## Testing the API

### Using HTTP Files
Use the provided `crud_bank_api.http` file with Visual Studio Code's REST Client extension to test all endpoints.

### Using cURL

#### Get all users:
```bash
curl -X GET http://localhost:5134/api/users
```

#### Get user by ID:
```bash
curl -X GET http://localhost:5134/api/users/1
```

#### Create a new user:
```bash
curl -X POST http://localhost:5134/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Alice",
    "lastName": "Johnson",
    "email": "alice.johnson@example.com",
    "phoneNumber": "+1555123456",
    "initialBalance": 500.00
  }'
```

#### Update a user:
```bash
curl -X PUT http://localhost:5134/api/users/1 \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John Updated",
    "phoneNumber": "+1999888777"
  }'
```

#### Delete a user:
```bash
curl -X DELETE http://localhost:5134/api/users/1
```

## Error Handling

The API includes comprehensive error handling:
- **400 Bad Request** - Invalid input data or validation errors
- **404 Not Found** - User not found
- **409 Conflict** - Email address already exists

All error responses include descriptive messages to help with debugging.

## Future Enhancements

- Database integration (Entity Framework Core)
- Authentication and authorization
- Account transaction history
- Balance transfer operations
- Email verification
- Password management
- Role-based access control
