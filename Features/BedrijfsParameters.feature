@cw2 @bedrijfsparameters
Feature: Bedrijfsparameters 
  As a RentWise user
  I want to manage bedrijfsparameters
  So that I can configure the system correctly

  Background:
    Given CW2 is started and logged in as user MARTIN in administration CW2

  @bedrijfsparameters
  Scenario: 01. Manage bedrijfsparameters
    When I navigate to Bedrijfsparameters in the modules
    Then the window Bedrijfsparameters is shown