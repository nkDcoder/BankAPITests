Feature: User Management
  Background:
    Given I create a new user

  Scenario Outline: User can be created with valid name
    Given I create a new user with name <Username>
    Then the status code is <Status>
    And the user should get a message containing <Message>
    Examples: 
    | Username          | Status | Message                     |
    | "Naveen Kumar"    | 201 | "User created successfully" |

 Scenario Outline: User gets error with invalid name while create
    Given I create a new user with name <Username>
    Then the status code is <Status>
    And the user should get a message containing <Message>
    Examples: 
    | Username          | Status | Message                     |
    | "$%^$%^^% ^&%&^%" | 400  | "Invalid user name"         |
    | "             "   | 400  | "Invalid user name"         |
    

  Scenario: Create a User and check he got one account by default with balance 100
    Given the endpoint is "/api/users"
    And the payload is random username
    And  content type is "application/json"
    And  method is "POST"
    When the request is made
    Then the status code is 201
    And the response message for the created user should contain "User created successfully"
    And check the new user is created with an account of minimum balance of 100

  Scenario: User account can be added when its none
    Given I have a random user with accounts
    And I delete all accounts for that user
    When I added an account for the user
    Then the status code is 201
    And the account should be added for the user

  Scenario: Create a user account
    Given I have a random user with accounts
    When I added an account for the user
    Then the status code is 201
    And the account should be added for the user

  Scenario: User can add amount to an account
    Given I have a random user with accounts
    And I pick a random Account for the user
    When I make a deposit of 1000
    Then the status code is 204
    And the user account should be updated with 1000

   Scenario: User can deduct amount to an account
    Given I have a random user with accounts
    And I pick a random Account for the user
    When I make a deposit of 5000
    And I make a withdrawal of 2000
    Then the status code is 204
    And the user account should be updated with 3000

  Scenario: User cannot deduct invalid amount from a user account
    Given I have a random user with accounts
    And I pick a random Account for the user
    When I make a withdrawal of 91 percent
    Then the status code is 400
    And the user should get a message containing "Invalid withdrawal amount."

  Scenario: User cannot deduct below 100 from a user account
    Given I have a random user with accounts
    And I pick a random Account for the user
    When I set the account balance to 400
    And I make a withdrawal of 350
    Then the status code is 400
    And the user should get a message containing "Invalid withdrawal amount."
    

  Scenario: User can delete an account
    Given I have a random user with accounts
    And I pick a random Account for the user
    When I delete the account for that user
    Then the user should get a message containing "deleted successfully"
    And the status code is 200
    And finding the not existing account should give message "Account with ID <accountId> not found for user <userId>."

  Scenario: User can not be deleted if has account(s) associated
    Given I have a random user with accounts
    When I delete the user
    Then the status code is 400
    And the user should get a message containing "You have to delete all associated accounts first for the user."

  Scenario: User can be deleted post has all account(s) associated
    Given I have a random user with accounts
    When I delete all accounts for that user
    And I delete the user
    Then the status code is 200
    And the user should get a message containing "deleted successfully"
    And finding the not existing user should give message "User with ID <userId> not found."
