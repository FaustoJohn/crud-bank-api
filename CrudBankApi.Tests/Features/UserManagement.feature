Feature: User Management API
    As an API user
    I want to manage users through the API
    So that I can create, read, update, and delete user records

    @SpecFlow
    Scenario: Create a new user
        Given I have user details with name "John Doe" and email "john.doe@example.com"
        When I send a POST request to create the user
        Then the user should be created successfully
        And the response should contain the user details

    @SpecFlow
    Scenario: Get user by ID
        Given a user exists with ID 1
        When I send a GET request for user ID 1
        Then the response should return the user details
        And the status code should be 200

    @SpecFlow
    Scenario: Update existing user
        Given a user exists with ID 1
        And I have updated user details with name "Jane Doe"
        When I send a PATCH request to update the user
        Then the user should be updated successfully
        And the response should contain the updated details

    @SpecFlow
    Scenario: Delete user
        Given a user exists with ID 1
        When I send a DELETE request for user ID 1
        Then the user should be deleted successfully
        And the status code should be 204
