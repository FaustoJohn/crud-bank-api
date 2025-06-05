# API Versioning Implementation

This document describes the API versioning implementation for the CRUD Bank API.

## Overview

The API now supports multiple versions to ensure backward compatibility while allowing for new features and improvements. The current implementation supports:

- **Version 1.0 (v1.0)**: The original API functionality
- **Version 2.0 (v2.0)**: Enhanced API with additional features and metadata

## Versioning Strategy

The API uses **URL-based versioning** as the primary method, with support for multiple versioning approaches:

### 1. URL Segment Versioning (Primary)
```
GET /v1.0/users
GET /v2.0/users
```

### 2. Query Parameter Versioning
```
GET /v1.0/users?version=2.0
```

### 3. Header Versioning
```
GET /v1.0/users
X-Version: 2.0
```

### 4. Media Type Versioning
```
GET /v1.0/users
Accept: application/json;ver=2.0
```

## Version Differences

### Authentication Controller (`/v{version}/auth`)

#### Common Endpoints (Available in both v1.0 and v2.0)
- `POST /login` - Standard login
- `POST /register` - User registration
- `POST /change-password` - Change password
- `GET /me` - Get current user info
- `POST /logout` - Logout

#### V2.0 Exclusive Endpoints
- `POST /login/enhanced` - Enhanced login with security metadata
- `GET /status` - Authentication status with token details

**Enhanced Login Response (v2.0)**:
```json
{
  "token": "jwt_token_here",
  "user": { /* user details */ },
  "securityInfo": {
    "loginTime": "2025-06-05T10:30:00Z",
    "expiresAt": "2025-06-05T11:30:00Z",
    "tokenType": "Bearer",
    "apiVersion": "2.0",
    "securityLevel": "Standard",
    "requiresMFA": false,
    "lastLoginAttempt": "2025-06-05T10:30:00Z"
  }
}
```

### Users Controller (`/v{version}/users`)

#### Common Endpoints (Available in both v1.0 and v2.0)
- `GET /` - Get all users
- `GET /{id}` - Get user by ID
- `GET /by-email` - Get user by email
- `POST /` - Create user
- `PUT /{id}` - Update user
- `DELETE /{id}` - Delete user
- `HEAD /{id}` - Check if user exists

#### V2.0 Exclusive Endpoints
- `GET /{id}/summary` - Get user with metadata
- `GET /paginated` - Get paginated users list

**User Summary Response (v2.0)**:
```json
{
  "user": { /* standard user object */ },
  "metadata": {
    "accountAge": "30.12:45:30",
    "apiVersion": "2.0",
    "lastAccessed": "2025-06-05T10:30:00Z",
    "features": ["Enhanced User Data", "Metadata Support", "Account Analytics"]
  }
}
```

**Paginated Users Response (v2.0)**:
```json
{
  "data": [ /* array of users */ ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalUsers": 42,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "apiVersion": "2.0"
}
```

## Configuration

The API versioning is configured in `Program.cs`:

```csharp
// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
```

## Swagger Documentation

The Swagger UI now displays separate documentation for each API version:

- **v1.0 Documentation**: Shows original API endpoints
- **v2.0 Documentation**: Shows enhanced API endpoints with v2.0 exclusive features

Access the Swagger UI at: `http://localhost:5134/swagger`

## Controller Implementation

Controllers are decorated with version attributes:

```csharp
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    // Common endpoints available in both versions
    
    [HttpGet("{id}/summary")]
    [MapToApiVersion("2.0")]  // V2.0 exclusive endpoint
    public async Task<ActionResult<object>> GetUserSummary(int id)
    {
        // V2.0 implementation
    }
}
```

## Testing API Versions

Use the provided `versioning-tests.http` file to test different versions:

1. Register users in both versions
2. Login using standard and enhanced endpoints
3. Test version-specific features
4. Verify different versioning methods (URL, query, header)

## Best Practices

1. **Backward Compatibility**: V1.0 endpoints remain unchanged
2. **Additive Changes**: New features are added to V2.0 without breaking existing functionality
3. **Clear Documentation**: Each version is clearly documented in Swagger
4. **Graceful Degradation**: Unsupported versions return appropriate error responses
5. **Consistent Responses**: Version information is included in V2.0 responses for clarity

## Migration Path

When upgrading from V1.0 to V2.0:

1. Update your API base URL from `/v1.0/` to `/v2.0/`
2. Take advantage of new features like paginated responses
3. Update response parsing to handle additional metadata
4. Consider using enhanced authentication endpoints for better security insights

## Future Versions

The versioning system is designed to be extensible. Future versions can be added by:

1. Adding new version numbers to controller attributes
2. Implementing version-specific endpoints with `[MapToApiVersion]`
3. Updating Swagger configuration for new version documentation
4. Adding comprehensive tests for new version features

## Error Handling

Invalid API versions will return appropriate HTTP error responses:

- **400 Bad Request**: For malformed version strings
- **404 Not Found**: For unsupported API versions
- **406 Not Acceptable**: For media type versioning conflicts
