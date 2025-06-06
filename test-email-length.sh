#!/bin/bash

echo "Testing Email Length Validation"
echo "==============================="

# Get token first
echo "Getting authentication token..."
REGISTER_RESPONSE=$(curl -s -X POST "http://localhost:5134/v1.0/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "EmailTest",
    "lastName": "User",
    "email": "emailtest.user@example.com", 
    "password": "password123",
    "phoneNumber": "555-000-0001",
    "initialBalance": 1000.00
  }')

if [[ $REGISTER_RESPONSE == *"token"* ]]; then
    TOKEN=$(echo $REGISTER_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
    echo "✓ Authentication successful"
else
    # Try login instead
    LOGIN_RESPONSE=$(curl -s -X POST "http://localhost:5134/v1.0/auth/login" \
      -H "Content-Type: application/json" \
      -d '{
        "email": "emailtest.user@example.com", 
        "password": "password123"
      }')
    
    if [[ $LOGIN_RESPONSE == *"token"* ]]; then
        TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
        echo "✓ Login successful"
    else
        echo "✗ Authentication failed"
        exit 1
    fi
fi

echo ""
echo "Testing with 300+ character email (should return 400):"

LONG_EMAIL="thisIsAnExtremelyLongEmailAddressThatDefinitelyExceedsTheTwoHundredFiftyFiveCharacterLimitSetInTheValidationAttributesAndShouldCauseA400BadRequestResponseFromTheServerBecauseItViolatesTheStringLengthValidationRulesThatAreDefinedInTheCreateUserDtoModel@verylongdomainnamethatisalsousedfortestingpurposesandexceedsthelimitssetinthevalidationattributes.com"

echo "Email length: $(echo -n "$LONG_EMAIL" | wc -c) characters"

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "http://localhost:5134/v1.0/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Test\",
    \"lastName\": \"User\",
    \"email\": \"$LONG_EMAIL\",
    \"phoneNumber\": \"555-123-4567\",
    \"initialBalance\": 100.00
  }")

STATUS_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n -1)

echo "Status Code: $STATUS_CODE"
echo "Response: $BODY"

if [ "$STATUS_CODE" == "400" ]; then
    echo "✓ Email length validation working correctly"
else
    echo "✗ Email length validation failed - expected 400 but got $STATUS_CODE"
fi
