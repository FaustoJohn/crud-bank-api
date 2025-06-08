# Test User Authorization Script
# This script tests the new authorization logic in the GetUser endpoint

Write-Host "Testing User Authorization Implementation" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Define base URL
$baseUrl = "http://localhost:5000"

Write-Host "`n1. Testing login for User 1 (john.doe@example.com)..." -ForegroundColor Yellow

# Login as User 1
$loginRequest1 = @{
    email = "john.doe@example.com"
    password = "password123"
} | ConvertTo-Json

try {
    $loginResponse1 = Invoke-RestMethod -Uri "$baseUrl/v1/auth/login" -Method Post -Body $loginRequest1 -ContentType "application/json"
    $token1 = $loginResponse1.token
    Write-Host "✅ User 1 login successful" -ForegroundColor Green
    
    # Test accessing own details (should work)
    Write-Host "`n2. Testing User 1 accessing own details (ID: 1)..." -ForegroundColor Yellow
    try {
        $headers1 = @{ Authorization = "Bearer $token1" }
        $userResponse1 = Invoke-RestMethod -Uri "$baseUrl/v1/users/1" -Method Get -Headers $headers1
        Write-Host "✅ User 1 can access own details successfully" -ForegroundColor Green
        Write-Host "   User: $($userResponse1.firstName) $($userResponse1.lastName)" -ForegroundColor Cyan
    }
    catch {
        Write-Host "❌ Unexpected error accessing own details: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # Test accessing another user's details (should fail with 403)
    Write-Host "`n3. Testing User 1 accessing another user's details (ID: 2)..." -ForegroundColor Yellow
    try {
        $userResponse2 = Invoke-RestMethod -Uri "$baseUrl/v1/users/2" -Method Get -Headers $headers1
        Write-Host "❌ SECURITY ISSUE: User 1 was able to access User 2's details!" -ForegroundColor Red
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 403) {
            Write-Host "✅ Correctly blocked with 403 Forbidden" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️  Unexpected error code: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
        }
    }
    
}
catch {
    Write-Host "❌ Login failed for User 1: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n4. Testing login for User 2 (jane.smith@example.com)..." -ForegroundColor Yellow

# Login as User 2
$loginRequest2 = @{
    email = "jane.smith@example.com"
    password = "password123"
} | ConvertTo-Json

try {
    $loginResponse2 = Invoke-RestMethod -Uri "$baseUrl/v1/auth/login" -Method Post -Body $loginRequest2 -ContentType "application/json"
    $token2 = $loginResponse2.token
    Write-Host "✅ User 2 login successful" -ForegroundColor Green
    
    # Test accessing own details (should work)
    Write-Host "`n5. Testing User 2 accessing own details (ID: 2)..." -ForegroundColor Yellow
    try {
        $headers2 = @{ Authorization = "Bearer $token2" }
        $userResponse2 = Invoke-RestMethod -Uri "$baseUrl/v1/users/2" -Method Get -Headers $headers2
        Write-Host "✅ User 2 can access own details successfully" -ForegroundColor Green
        Write-Host "   User: $($userResponse2.firstName) $($userResponse2.lastName)" -ForegroundColor Cyan
    }
    catch {
        Write-Host "❌ Unexpected error accessing own details: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # Test accessing another user's details (should fail with 403)
    Write-Host "`n6. Testing User 2 accessing another user's details (ID: 1)..." -ForegroundColor Yellow
    try {
        $userResponse1 = Invoke-RestMethod -Uri "$baseUrl/v1/users/1" -Method Get -Headers $headers2
        Write-Host "❌ SECURITY ISSUE: User 2 was able to access User 1's details!" -ForegroundColor Red
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 403) {
            Write-Host "✅ Correctly blocked with 403 Forbidden" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️  Unexpected error code: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
        }
    }
    
}
catch {
    Write-Host "❌ Login failed for User 2: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n7. Testing unauthorized access (no token)..." -ForegroundColor Yellow
try {
    $unauthorizedResponse = Invoke-RestMethod -Uri "$baseUrl/v1/users/1" -Method Get
    Write-Host "❌ SECURITY ISSUE: Unauthorized access was allowed!" -ForegroundColor Red
}
catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✅ Correctly blocked with 401 Unauthorized" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️  Unexpected error code: $($_.Exception.Response.StatusCode)" -ForegroundColor Orange
    }
}

Write-Host "`n=======================================" -ForegroundColor Green
Write-Host "Authorization Test Complete" -ForegroundColor Green
Write-Host "Expected Results:" -ForegroundColor Cyan
Write-Host "• Users can access their own details (200 OK)" -ForegroundColor Cyan
Write-Host "• Users cannot access other users' details (403 Forbidden)" -ForegroundColor Cyan
Write-Host "• Unauthenticated requests are blocked (401 Unauthorized)" -ForegroundColor Cyan
