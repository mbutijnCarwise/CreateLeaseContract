@cw2 @desktop
Feature: RentWise rental agreement tablet signing
  As a RentWise user
  I want to prepare a rental agreement for tablet signing and find it in DocWise
  So that I can verify the signature and document ID

  Background:
    Given CW2 is started and logged in as user MARTIN in administration CW2
    #And the window "CW2 - Hoofdmenu" is open and maximized on the primary screen

  @rentWiseTabletSigning
  Scenario: 01. Prepare a rental agreement for smartphone signing and verify it in DocWise (this scenario is based on 309220
    When I navigate to RentWise Activiteiten in the modules
    #Then the window "CW2 - RentWise activiteiten" is shown
    When I open Verhuuractiviteiten in the navigation
    And I go to Overeenkomsten in the navigation
    And I double-click a randomly selected vehicle agreement that hadn't been digitally signed
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
