#!/bin/bash

# CRUD Bank API - Swagger Documentation Test Script
# This script tests the documented API endpoints to ensure they work as documented

BASE_URL="http://localhost:5134"
echo "Testing CRUD Bank API Swagger Documentation..."
echo "Base URL: $BASE_URL"
echo "========================================="

# Test 1: Check Swagger UI accessibility
echo "1. Testing Swagger UI accessibility..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/swagger")
if [ $STATUS -eq 200 ] || [ $STATUS -eq 301 ]; then
    echo "   ✅ Swagger UI accessible (HTTP $STATUS)"
else
    echo "   ❌ Swagger UI not accessible (HTTP $STATUS)"
fi

# Test 2: Check OpenAPI specification v1
echo "2. Testing OpenAPI specification v1..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/swagger/v1/swagger.json")
if [ $STATUS -eq 200 ]; then
    echo "   ✅ OpenAPI v1 specification accessible (HTTP $STATUS)"
else
    echo "   ❌ OpenAPI v1 specification not accessible (HTTP $STATUS)"
fi

# Test 3: Check OpenAPI specification v2
echo "3. Testing OpenAPI specification v2..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/swagger/v2/swagger.json")
if [ $STATUS -eq 200 ]; then
    echo "   ✅ OpenAPI v2 specification accessible (HTTP $STATUS)"
else
    echo "   ❌ OpenAPI v2 specification not accessible (HTTP $STATUS)"
fi

# Test 4: Test login endpoint (should return 401 for invalid credentials)
echo "4. Testing login endpoint with invalid credentials..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/v1/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"invalid@test.com","password":"wrongpassword"}')
if [ $STATUS -eq 401 ]; then
    echo "   ✅ Login endpoint returns correct 401 for invalid credentials"
else
    echo "   ⚠️  Login endpoint returned $STATUS (expected 401)"
fi

# Test 5: Test users endpoint without authentication (should return 401)
echo "5. Testing users endpoint without authentication..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/v1/users")
if [ $STATUS -eq 401 ]; then
    echo "   ✅ Users endpoint correctly requires authentication (HTTP $STATUS)"
else
    echo "   ⚠️  Users endpoint returned $STATUS (expected 401)"
fi

# Test 6: Test login with valid credentials
echo "6. Testing login with valid credentials..."
RESPONSE=$(curl -s -X POST "$BASE_URL/v1/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@crudbank.com","password":"admin123"}')

if echo "$RESPONSE" | grep -q "token"; then
    echo "   ✅ Login successful - JWT token received"
    
    # Extract token for further tests
    TOKEN=$(echo "$RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
    
    if [ ! -z "$TOKEN" ]; then
        echo "   📝 Token extracted for authentication tests"
        
        # Test 7: Test authenticated endpoint
        echo "7. Testing authenticated users endpoint..."
        STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/v1/users" \
            -H "Authorization: Bearer $TOKEN")
        if [ $STATUS -eq 200 ]; then
            echo "   ✅ Authenticated users endpoint works correctly (HTTP $STATUS)"
        else
            echo "   ⚠️  Authenticated users endpoint returned $STATUS (expected 200)"
        fi
        
        # Test 8: Test current user endpoint
        echo "8. Testing current user endpoint..."
        STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/v1/auth/me" \
            -H "Authorization: Bearer $TOKEN")
        if [ $STATUS -eq 200 ]; then
            echo "   ✅ Current user endpoint works correctly (HTTP $STATUS)"
        else
            echo "   ⚠️  Current user endpoint returned $STATUS (expected 200)"
        fi
    fi
else
    echo "   ⚠️  Login may have failed - no token in response"
fi

# Test 9: Test user creation with missing data (should return 400)
echo "9. Testing user creation with invalid data..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/v1/auth/register" \
    -H "Content-Type: application/json" \
    -d '{"email":"","password":""}')
if [ $STATUS -eq 400 ] || [ $STATUS -eq 422 ]; then
    echo "   ✅ User creation correctly validates input (HTTP $STATUS)"
else
    echo "   ⚠️  User creation returned $STATUS (expected 400 or 422)"
fi

echo "========================================="
echo "✅ Swagger Documentation Test Complete!"
echo ""
echo "Access Swagger UI at: $BASE_URL/swagger"
echo "OpenAPI v1 Spec: $BASE_URL/swagger/v1/swagger.json"
echo "OpenAPI v2 Spec: $BASE_URL/swagger/v2/swagger.json"
