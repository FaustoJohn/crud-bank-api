# Bank API Authentication Management

## Overview
This document describes the authentication system implemented in the CRUD Bank API. The system uses JWT (JSON Web Tokens) for secure authentication and authorization.

## Features Implemented

### 1. JWT-Based Authentication
- **Token Generation**: Secure JWT tokens with configurable expiration
- **Token Validation**: Automatic validation of incoming requests
- **Claims-Based Authorization**: User information stored in token claims

### 2. Password Security
- **BCrypt Hashing**: Passwords are hashed using BCrypt with salt
- **Password Validation**: Minimum 6 character requirement
- **Password Change**: Secure password update functionality

### 3. Authentication Endpoints

#### POST `/api/auth/register`
Register a new user account.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "password123",
  "phoneNumber": "+1234567890"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2025-06-05T15:30:00Z",
  "user": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "+1234567890",
    "accountNumber": "ACC123456",
    "balance": 0.00,
    "createdAt": "2025-06-05T14:30:00Z",
    "updatedAt": null,
    "isActive": true
  }
}
```

#### POST `/api/auth/login`
Authenticate with email and password.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "password123"
}
```

**Response:** Same as register response.

#### GET `/api/auth/me`
Get current authenticated user information. **Requires Authentication**

**Headers:**
```
Authorization: Bearer {your-jwt-token}
```

**Response:**
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
  "createdAt": "2025-06-05T14:30:00Z",
  "updatedAt": null,
  "isActive": true
}
```

#### POST `/api/auth/change-password`
Change password for authenticated user. **Requires Authentication**

**Request Body:**
```json
{
  "currentPassword": "oldpassword123",
  "newPassword": "newpassword123"
}
```

**Response:**
```json
{
  "message": "Password changed successfully."
}
```

#### POST `/api/auth/logout`
Logout (client-side token invalidation). **Requires Authentication**

**Response:**
```json
{
  "message": "Logged out successfully. Please remove the token from client storage."
}
```

### 4. Protected User Endpoints
All `/api/users` endpoints now require authentication:

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/by-email?email=...` - Get user by email
- `POST /api/users` - Create new user (admin functionality)
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Soft delete user
- `HEAD /api/users/{id}` - Check if user exists

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long!",
    "Issuer": "crud-bank-api",
    "Audience": "crud-bank-api-users",
    "ExpirationInMinutes": 60
  }
}
```

### Security Features
- **HTTPS Redirect**: Automatic redirect to HTTPS in production
- **Token Expiration**: Configurable token lifetime
- **Claims Validation**: Comprehensive token validation
- **Password Complexity**: Minimum length requirements

## Test Users
The system comes with pre-seeded test users:

1. **John Doe**
   - Email: `john.doe@example.com`
   - Password: `password123`
   - Balance: $1,000.00

2. **Jane Smith**
   - Email: `jane.smith@example.com`
   - Password: `password123`
   - Balance: $2,500.50

## Usage Examples

### 1. Login and Get Token
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "password123"
  }'
```

### 2. Access Protected Endpoint
```bash
curl -X GET http://localhost:5001/api/users \
  -H "Authorization: Bearer {your-jwt-token}"
```

### 3. Register New User
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "New",
    "lastName": "User",
    "email": "new.user@example.com",
    "password": "securepassword123",
    "phoneNumber": "+1987654321"
  }'
```

## Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["The Password field is required."]
  }
}
```

### 409 Conflict (User Already Exists)
```json
"User with this email already exists."
```

## Swagger Documentation
The API includes Swagger documentation with JWT authentication support:
- URL: `http://localhost:5001/swagger`
- JWT Bearer token authentication is configured
- Test endpoints directly from the Swagger UI

## Architecture

### Services
- **IAuthService / AuthService**: Handles authentication logic
- **IUserService / UserService**: Manages user operations
- **JWT Token Generation**: Secure token creation with claims

### Models
- **LoginDto**: Login request model
- **RegisterDto**: Registration request model
- **ChangePasswordDto**: Password change model
- **AuthResponseDto**: Authentication response with token
- **JwtSettings**: JWT configuration model

### Security Middleware
- **JWT Bearer Authentication**: Validates tokens on protected routes
- **Authorization**: Enforces authentication requirements
- **HTTPS Redirection**: Ensures secure communication

This implementation provides a complete authentication system suitable for a banking application with proper security practices and comprehensive API documentation.
