@cw2
Feature: AccountWise
  As a user
  I want to verify the AccountWise menu items
  So that I can ensure the correct options are available.

  Background:
    Given CW2 is started and logged in as user MARTIN in administration CW2
    
  Scenario: 01. Verify AccountWise menu items
    When I navigate to AccountWise in the modules
    Then the window heading AccountWise · Algemeen is shown
    
  Scenario: 02. Verify AccountWise Debiteuren
    Given I have navigated to Debiteuren in the main menu of AccountWise
    Then I should see the following items are in the Debiteuren sub menu of AccountWise
      | Menu item                 |
      | Startpagina               |
      | Autorisatie debiteuren    |
      | Kredietcheck              |
      | Debiteuren                |
      | Betaalweek debiteuren     |
      | Debiteuren saldi          |
      | Debiteuren analyse        |
      | Debiteuren facturen       |
      | Mandaten                  |
      | Factuur referenties       |
      | Inkoopfactuur referenties |

  Scenario: 03. Verify AccountWise Crediteuren
    Given I have navigated to Crediteuren in the main menu of AccountWise
    Then I should see the following items are in the Crediteuren sub menu of AccountWise
      | Menu item               |
      | Startpagina             |
      | Crediteuren             |
      | Betaalweek crediteuren  |
      | Crediteuren saldi       |
      | Autorisatie crediteuren |

  Scenario: 04. Verify AccountWise Activiteiten
    Given I have navigated to Activiteiten in the main menu of AccountWise
    Then I should see the following items are in the Activiteiten sub menu of AccountWise
      | Menu item                            |
      | Startpagina                          |
      | Factuur registraties                 |
      | Te accorderen facturen               |
      | Analyse retouren                     |
      | Boeken geregistreerde inkoopfacturen |
      | Te ontvangen facturen                |
      | Vaste journaalposten                 |
      | Activa                               |
      | Afschrijvingsstaat                   |
      | Exportruns                           |
      | Import definities                    |
      | Import log                           |
