  @cw2 @CRM
  Feature: CRM Module Navigation
    As a user
    I want to navigate to the CRM module
    So that I can view and manage CRM activities

  Background:
    Given CW2 is started and logged in as user MARTIN in administration CW2
  
  Scenario: 01. Navigate to CRM
    When I navigate to CRM Algemeen in the modules
    Then the window heading CRM · Algemeen is shown
    And the following menu items are present
      | Kenmerkgroepen | Kenmerk lijsten | Relatiekenmerken | Actiesoorten | Views | Contactsoorten | Ontvangstwijzen | AVG beheer | Default relaties | KvK SBI codes | Self service soorten | 
    When I open Views in the main menu
    Then the window heading Views is shown
