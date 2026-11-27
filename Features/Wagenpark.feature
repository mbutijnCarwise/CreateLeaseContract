@cw2
Feature: Wagenpark
  As lease contract manager
  I want to access the Wagenpark module
  So that I can manage the vehicles in the fleet

  Background:
  Given CW2 is started and logged in as user MARTIN in administration CW2

  Scenario: Navigate and verify Auto's in Wagenpark module
    When I navigate to Wagenpark in the modules
    Then the window heading Wagenpark · Algemeen is shown
    When I close Algemeen
    And I open Activiteiten -> Auto's in the main menu
    Then the window heading Auto's is shown