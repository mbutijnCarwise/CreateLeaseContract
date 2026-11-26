@cw2 @desktop
Feature: RentWise rental agreement tablet signing
  As a RentWise user
  I want to prepare a rental agreement for tablet signing and find it in DocWise
  So that I can verify the signature and document ID

  Background:
    Given CW2 is started and logged in as user MARTIN in administration CW2
    #And the window "CW2 - Hoofdmenu" is open and maximized on the primary screen

  Scenario: 01. Navigate to RentWise Activiteiten
    When I navigate to RentWise Activiteiten in the modules
    Then the window RentWise · Algemeen is shown
    When I open Verhuuractiviteiten in the main menu
    Then the following items are shown in the sub menu:
      | Categorie                |
      | Vandaag                  |
      | Planning                 |
      | Reserveringen            |
      | Overeenkomsten           |
      | Offertes                 |
      | Beschikbare auto's       |
      | Wagenpark                |
      | Aflever- en retourbonnen |
      | Uitgifte / Inname        |
      | Overige functies         |


  Scenario: 02. Make a new quotation in Offertes
    Given I have navigated to Offertes in the sub menu of Verhuuractiviteiten
    When I click on button Toevoegen
    Then the label Toevoegen offerte is present
    # And I fill in these remaining fields: (these fields are not fillable yet)
    #  | Telefoon  | Mobiel    | Email                   |
    #  | 123456789 | 987654321 | mbu_debiteur@carwise.nl |
    When I fill in these remaining fields:
      | Huurder  | Datum opvolging | Datum/tijd vertrek | Datum/tijd retour | Categorie | Kenteken | Te rijden kilometers | Aanbetaling | Waarborg | Opmerking | Soort | Eenheid |
      | DEBITEUR | 20-11-2026      | 20-11-2026 16:24   | 20-12-2026 16:00  | A         | G-108-DV | 250                  | 10000       | 5000     | opmerking | SL    | MAAND20 |
    And I click on button Opslaan
    Then the new quotation has been given a number
    When I click on button OK
    Then the label Offertes is present
    And the new added quotation is present


  Scenario: 03. Make a new reservation in Beschikbare auto's
    Given I have navigated to Beschikbare auto's in the sub menu of Verhuuractiviteiten
    Then the label Beschikbare auto's is present
    When I select the first vehicle
    And I make a new reservation
    And I click on tab Huurder / debiteur
    And I fill in the Huurder field with DEBITEUR
    And I click on tab Voorwaarden
    And I fill in these fields in the Voorwaarden tab:
      | Soort | Eenheid |
      | SL    | MAAND20 |
    And I click on button Toepassen
    And I click on button Nee
    And I click on button Opslaan
    And I click on button Reserveringen
    Then this new reservation should be vissible in the table
    

  #Scenario: 04. Navigate in Wagenpark
  #  Given I have navigated to Wagenpark in the sub menu of Verhuuractiviteiten
  #  When I click on button Wagenpark

  @rentWiseTabletSigning
  Scenario: 04. Prepare a rental agreement for smartphone signing and verify it in DocWise (this scenario is based on 309220
    Given I have navigated to Overeenkomsten in the sub menu of Verhuuractiviteiten
    When I double-click the top digitally unsigned vehicle agreement
    #Then the selected rental agreement is shown
    #When I enter "DEBITEUR" in the Huurder field
    #And I press Tab to resolve the debiteur
    #Then the Huurder resolves to "mbu_debiteur"
    #When I open the customer details with the person button
    #And I open "Klant"
    #And I ensure Tekenbevoegde 1 is "mbu_bestuurder" with role "Ondertekenaar"
    #And I ensure Tekenbevoegde 2 is "G. van den Mulder" with role "Zichtbare goedkeurder"
    #Then the two signatories are different people
    #When I save the customer record with "Opslaan", or "OK" if "Opslaan" is unavailable
    #And I open tab "Voorwaarden"
    #And I select basis tariff "ROUTZ"
    #And if "ROUTZ" is unavailable or has no valid rate, I use "SL" (Shortlease)
    #And I choose basis unit "MAAND20" if it is listed for the selected tariff
    #And if "MAAND20" is not listed, I choose a valid listed unit
    #And I record the displayed tariff unit and agreement amounts
    #And I save the rental agreement
    #When I click "Printen"
    #Then the print dialog is shown
    #When I select "Digitaal ondertekenen (tablet aan de balie)" under "Uitvoer"
    #And I enable "Wachten op tablet"
    #And I click "OK"
    #Then the tablet signing request is prepared
    #And the signing-code prompt is shown
    #When the human user enters the full signing code directly in CW2
    #And the human user clicks "Digitaal ondertekenen" in CW2
    #And the human user completes signing in the smartphone app
    #Then CW2 confirms that signing is complete
    #When I click "Sluiten"
    #And I open "DocWise"
    #Then the signed agreement is present in DocWise
    #And the agreement number in the document matches the selected agreement
    #And the monetary amounts in the document match the saved agreement values
    #And the tariff unit and billing-period text match the saved agreement settings
    #And the signature page and verification show a completed signing
    #And the Document ID is read from the document details, not the DocWise row number
    #When I close "DocWise"
    #And I close the rental agreement
