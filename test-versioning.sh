#!/bin/bash

echo "=== API Versioning Test Script ==="
echo "Testing CRUD Bank API versioning implementation"
echo ""

BASE_URL="http://localhost:5134"

echo "1. Testing V1.0 Auth Register endpoint..."
curl -w "\nStatus: %{http_code}\n" -X POST "$BASE_URL/v1.0/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "TestPassword123!",
    "firstName": "Test",
    "lastName": "User"
  }'

echo -e "\n\n2. Testing V2.0 Auth Register endpoint..."
curl -w "\nStatus: %{http_code}\n" -X POST "$BASE_URL/v2.0/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser2@example.com",
    "password": "TestPassword123!",
    "firstName": "Test V2",
    "lastName": "User"
  }'

echo -e "\n\n3. Testing V1.0 Users endpoint (should require auth)..."
curl -w "\nStatus: %{http_code}\n" -X GET "$BASE_URL/v1.0/users"

echo -e "\n\n4. Testing V2.0 Users endpoint (should require auth)..."
curl -w "\nStatus: %{http_code}\n" -X GET "$BASE_URL/v2.0/users"

echo -e "\n\n5. Testing V2.0 Enhanced Auth endpoint (should not exist in V1.0)..."
curl -w "\nStatus: %{http_code}\n" -X GET "$BASE_URL/v1.0/auth/status"

echo -e "\n\n6. Testing V2.0 Enhanced Auth endpoint (should exist in V2.0)..."
curl -w "\nStatus: %{http_code}\n" -X GET "$BASE_URL/v2.0/auth/status"

echo -e "\n\n7. Testing invalid version..."
curl -w "\nStatus: %{http_code}\n" -X GET "$BASE_URL/v3.0/users"

echo -e "\n\n8. Testing Swagger endpoints..."
echo "V1.0 Swagger: $BASE_URL/swagger/v1/swagger.json"
curl -w "\nStatus: %{http_code}\n" -X GET "$BASE_URL/swagger/v1/swagger.json" | head -n 1

echo -e "\nV2.0 Swagger: $BASE_URL/swagger/v2/swagger.json"
curl -w "\nStatus: %{http_code}\n" -X GET "$BASE_URL/swagger/v2/swagger.json" | head -n 1

echo -e "\n\n=== API Versioning Test Complete ==="
