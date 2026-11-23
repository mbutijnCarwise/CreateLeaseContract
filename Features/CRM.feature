  @cw2 @CRM
  Feature: CRM Module Navigation
    As a user
    I want to navigate to the CRM module
    So that I can view and manage CRM activities

  Background:
    Given CW2 is started and logged in as user MARTIN in administration CW2
  
  Scenario: 01. Navigate to CRM
    When I navigate to CRM Algemeen in the modules
    Then the window CRM · Algemeen is shown
    When I open Views in the main menu
    Then the window Views is shown
