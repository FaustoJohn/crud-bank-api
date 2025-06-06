#!/bin/bash

# Test script for user creation validation

BASE_URL="http://localhost:5134/api/v1/users"

echo "Testing User Creation Validation"
echo "================================"

echo
echo "Test 1: Valid user creation (should return 201)"
curl -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "+1-555-123-4567",
    "address": "123 Main St, Anytown, USA"
  }' \
  -w "\nStatus Code: %{http_code}\n" \
  -s

echo
echo "----------------------------------------"
echo "Test 2: Empty name (should return 400)"
curl -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "email": "john.doe@example.com",
    "phoneNumber": "+1-555-123-4567",
    "address": "123 Main St, Anytown, USA"
  }' \
  -w "\nStatus Code: %{http_code}\n" \
  -s

echo
echo "----------------------------------------"
echo "Test 3: Invalid email (should return 400)"
curl -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "invalid-email",
    "phoneNumber": "+1-555-123-4567",
    "address": "123 Main St, Anytown, USA"
  }' \
  -w "\nStatus Code: %{http_code}\n" \
  -s

echo
echo "----------------------------------------"
echo "Test 4: Invalid phone number (should return 400)"
curl -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "123",
    "address": "123 Main St, Anytown, USA"
  }' \
  -w "\nStatus Code: %{http_code}\n" \
  -s

echo
echo "----------------------------------------"
echo "Test 5: Multiple validation errors (should return 400)"
curl -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "email": "invalid-email",
    "phoneNumber": "123",
    "address": ""
  }' \
  -w "\nStatus Code: %{http_code}\n" \
  -s

echo
echo "----------------------------------------"
echo "Test 6: Missing required fields (should return 400)"
curl -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe"
  }' \
  -w "\nStatus Code: %{http_code}\n" \
  -s

echo
echo "========================================"
echo "Validation tests completed!"
