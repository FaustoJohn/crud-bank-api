#!/bin/bash

echo "=== Bank API Authentication Test ==="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

API_URL="http://localhost:5001/api"

echo -e "${BLUE}1. Testing Login with John Doe${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "password123"
  }')

if [[ $LOGIN_RESPONSE == *"token"* ]]; then
    echo -e "${GREEN}✓ Login successful${NC}"
    # Extract token (simple approach)
    TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
    echo "Token received: ${TOKEN:0:50}..."
else
    echo -e "${RED}✗ Login failed${NC}"
    echo "Response: $LOGIN_RESPONSE"
    exit 1
fi

echo ""
echo -e "${BLUE}2. Testing Protected Endpoint (Get Current User)${NC}"
USER_RESPONSE=$(curl -s -X GET "$API_URL/auth/me" \
  -H "Authorization: Bearer $TOKEN")

if [[ $USER_RESPONSE == *"john.doe@example.com"* ]]; then
    echo -e "${GREEN}✓ Protected endpoint accessible with token${NC}"
    echo "User data: $(echo $USER_RESPONSE | grep -o '"fullName":"[^"]*"' | cut -d'"' -f4)"
else
    echo -e "${RED}✗ Protected endpoint failed${NC}"
    echo "Response: $USER_RESPONSE"
fi

echo ""
echo -e "${BLUE}3. Testing Protected Endpoint Without Token${NC}"
UNAUTH_RESPONSE=$(curl -s -X GET "$API_URL/auth/me")

if [[ $UNAUTH_RESPONSE == *"401"* ]] || [[ $UNAUTH_RESPONSE == "" ]]; then
    echo -e "${GREEN}✓ Protected endpoint correctly rejects requests without token${NC}"
else
    echo -e "${RED}✗ Protected endpoint should reject unauthorized requests${NC}"
    echo "Response: $UNAUTH_RESPONSE"
fi

echo ""
echo -e "${BLUE}4. Testing User Registration${NC}"
REGISTER_RESPONSE=$(curl -s -X POST "$API_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test.user@example.com",
    "password": "testpassword123",
    "phoneNumber": "+1555000001"
  }')

if [[ $REGISTER_RESPONSE == *"token"* ]]; then
    echo -e "${GREEN}✓ User registration successful${NC}"
    NEW_USER_EMAIL=$(echo $REGISTER_RESPONSE | grep -o '"email":"[^"]*"' | cut -d'"' -f4)
    echo "New user registered: $NEW_USER_EMAIL"
else
    echo -e "${RED}✗ User registration failed${NC}"
    echo "Response: $REGISTER_RESPONSE"
fi

echo ""
echo -e "${BLUE}5. Testing Protected Users Endpoint${NC}"
USERS_RESPONSE=$(curl -s -X GET "$API_URL/users" \
  -H "Authorization: Bearer $TOKEN")

if [[ $USERS_RESPONSE == *"john.doe@example.com"* ]]; then
    echo -e "${GREEN}✓ Users endpoint accessible with authentication${NC}"
    USER_COUNT=$(echo $USERS_RESPONSE | grep -o '"email"' | wc -l)
    echo "Found $USER_COUNT users in the system"
else
    echo -e "${RED}✗ Users endpoint failed${NC}"
    echo "Response: $USERS_RESPONSE"
fi

echo ""
echo -e "${GREEN}=== Authentication Test Complete ===${NC}"
