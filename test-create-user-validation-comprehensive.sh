#!/bin/bash

echo "=== Comprehensive User Creation Validation Tests ==="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

API_URL="http://localhost:5134/v1.0/users"
AUTH_URL="http://localhost:5134/v1.0/auth"

# Function to get auth token
get_auth_token() {
    echo -e "${BLUE}Registering a test user for authentication...${NC}"
    REGISTER_RESPONSE=$(curl -s -X POST "$AUTH_URL/register" \
      -H "Content-Type: application/json" \
      -d '{
        "firstName": "Test",
        "lastName": "User",
        "email": "test.user@example.com", 
        "password": "password123",
        "phoneNumber": "555-000-0000",
        "initialBalance": 1000.00
      }')
    
    if [[ $REGISTER_RESPONSE == *"token"* ]]; then
        TOKEN=$(echo $REGISTER_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        echo -e "${GREEN}✓ Registration and authentication successful${NC}"
        echo ""
    else
        echo -e "${BLUE}Registration failed, trying to login with existing user...${NC}"
        LOGIN_RESPONSE=$(curl -s -X POST "$AUTH_URL/login" \
          -H "Content-Type: application/json" \
          -d '{
            "email": "test.user@example.com", 
            "password": "password123"
          }')
        
        if [[ $LOGIN_RESPONSE == *"token"* ]]; then
            TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
            echo -e "${GREEN}✓ Login successful${NC}"
            echo ""
        else
            echo -e "${RED}✗ Authentication failed. Register Response: $REGISTER_RESPONSE${NC}"
            echo -e "${RED}✗ Login Response: $LOGIN_RESPONSE${NC}"
            exit 1
        fi
    fi
}

# Function to test endpoint and check status code
test_endpoint() {
    local test_name="$1"
    local data="$2" 
    local expected_code="$3"
    
    echo -e "${BLUE}Test: $test_name${NC}"
    
    if [ "$data" == "null" ]; then
        # Test with null body
        RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL" \
          -H "Content-Type: application/json" \
          -H "Authorization: Bearer $TOKEN")
    else
        RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_URL" \
          -H "Content-Type: application/json" \
          -H "Authorization: Bearer $TOKEN" \
          -d "$data")
    fi
    
    STATUS_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n -1)
    
    if [ "$STATUS_CODE" == "$expected_code" ]; then
        echo -e "${GREEN}✓ Expected status code $expected_code received${NC}"
    else
        echo -e "${RED}✗ Expected $expected_code but got $STATUS_CODE${NC}"
    fi
    
    echo "Response body: $BODY"
    echo "----------------------------------------"
    echo ""
}

# Get authentication token first
get_auth_token

echo -e "${YELLOW}Testing various validation scenarios...${NC}"
echo ""

# Test 1: Valid user creation (should return 201)
test_endpoint "Valid user creation" '{
  "firstName": "Jane",
  "lastName": "Smith", 
  "email": "jane.smith@example.com",
  "phoneNumber": "555-123-4567",
  "password": "securepass123",
  "initialBalance": 1000.50
}' "201"

# Test 2: Missing required fields (should return 400)
test_endpoint "Missing required fields" '{
  "phoneNumber": "555-123-4567",
  "initialBalance": 500.00
}' "400"

# Test 3: Empty required fields (should return 400)
test_endpoint "Empty required fields" '{
  "firstName": "",
  "lastName": "",
  "email": "",
  "phoneNumber": "555-123-4567",
  "initialBalance": 100.00
}' "400"

# Test 4: Invalid email format (should return 400)
test_endpoint "Invalid email format" '{
  "firstName": "Bob",
  "lastName": "Wilson",
  "email": "invalid-email-format",
  "phoneNumber": "555-987-6543",
  "initialBalance": 250.00
}' "400"

# Test 5: Negative initial balance (should return 400)
test_endpoint "Negative initial balance" '{
  "firstName": "Alice",
  "lastName": "Johnson",
  "email": "alice.johnson@example.com",
  "phoneNumber": "555-456-7890",
  "initialBalance": -100.00
}' "400"

# Test 6: Invalid phone number format (should return 400)
test_endpoint "Invalid phone number format" '{
  "firstName": "Charlie",
  "lastName": "Brown",
  "email": "charlie.brown@example.com",
  "phoneNumber": "abc-def-ghij",
  "initialBalance": 300.00
}' "400"

# Test 7: Name too long (should return 400)
test_endpoint "Name too long" '{
  "firstName": "ThisIsAnExtremelyLongFirstNameThatExceedsTheMaximumAllowedLengthOfOneHundredCharactersForTheFirstNameField",
  "lastName": "Smith",
  "email": "toolong@example.com",
  "phoneNumber": "555-123-4567",
  "initialBalance": 100.00
}' "400"

# Test 8: Email too long (should return 400)
test_endpoint "Email too long" '{
  "firstName": "Test",
  "lastName": "User",
  "email": "averylongemailaddressthatexceedsthemaximumallowedlengthoftwohundredfiftyfivecharactersforthetestingpurposesandshouldreturnabadrequest@verylongdomainnamethatisalsousedfortestingvalidation.com",
  "phoneNumber": "555-123-4567",
  "initialBalance": 100.00
}' "400"

# Test 9: Password too short (should return 400)
test_endpoint "Password too short" '{
  "firstName": "Test",
  "lastName": "User",
  "email": "shortpass@example.com",
  "phoneNumber": "555-123-4567",
  "password": "12345",
  "initialBalance": 100.00
}' "400"

# Test 10: Null request body (should return 400)
test_endpoint "Null request body" "null" "400"

# Test 11: Duplicate email (should return 409 Conflict)
test_endpoint "Duplicate email" '{
  "firstName": "Another",
  "lastName": "User",
  "email": "jane.smith@example.com",
  "phoneNumber": "555-999-8888",
  "initialBalance": 500.00
}' "409"

echo -e "${YELLOW}All validation tests completed!${NC}"
