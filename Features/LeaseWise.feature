@cw2 @desktop
Feature: CW2 lease contract without down payment up to "Definitieve auto inzetten/Investering boeken"
  As lease employee MARTIN
  I want to turn a lease quotation into a contract, order the car and register it as delivered
  So that the contract gets action "Definitieve auto inzetten/Investering boeken"

  # Recorded from the CW2 desktop (WinForms) application. To be automated with Playwright once a web version with a URL exists.
  # Remove @ignore once step definitions exist.
  # Notes from the manual desktop run:
  # - New dialogs sometimes open on the secondary monitor: move them to the primary screen first.
  # - Ribbon/toolbar buttons are owner-drawn and not exposed by UI Automation; click by position relative to the window.
  # - Open a dropdown menu ("Acties", "Bestelbon") and click the item within the same step, otherwise the menu closes.

  Background:
    Given CW2 is started and logged in as user MARTIN in administration CW2
    # And the window "CW2 - Hoofdmenu" is open and maximized on the primary screen

  @createLeaseContractWithoutDownPayment
  Scenario Outline: 01. Lease quotation to contract with action Definitieve auto inzetten/Investering boeken
    # Quotation
    When I navigate to LeaseWise Activiteiten in the modules
    # Then the window "CW2 - LeaseWise activiteiten" is shown
    # When I open "Leaseoffertes" in navigation group "Offerte"
    # And I click "Nieuw" in the toolbar
    # Then the window "CW2 - Leaseofferte" is shown with title "Nieuwe offerte"
    # When I fill in Contractant name "<debtor>" and press Tab
    # Then Contractant shows name "<debtorName>"
    # And Medewerker is filled with "<employee>"
    # And Debiteur is filled with "<debtor>"
    # And Debiteur aanbetaling is filled with "<debtor>"
    # When I click "Opslaan" in the quotation
    # Then the quotation title is "Wijzigen offerte" followed by the new quotation number

    # Calculation (AutoDisk)
    # When I open tab "Offerte"
    # Then Mantelnummer is "1" with description "Default instellingen"
    # And object type "Auto geel" is selected
    # And the list "Offerte regels" is empty
    # When I click "Calculatie" under "Offerte regels"
    # Then the AutoDisk window "Zoek en calculeer" is shown for the new quotation, line 1
    # When I double-click a car in the list
    # Then the window "Instellingen leasecalculatie" is shown for line 1 of the new quotation
    # And the message "Eigen restwaarde is niet bekend" is shown
    # When I confirm the message with "OK"
    # Then the lease term is 48 months and the mileage is 30.000 km per year
    # And the lease price per month excl. VAT is greater than 0,00
    # When I click "Opslaan" in the calculation
    # And I answer "Ja" to "Weet u zeker dat u op wilt slaan?"
    # Then the message "Calculatie opgeslagen!" is shown
    # When I click "Klaar!"
    # Then "Offerte regels" contains 1 line with the chosen car and the calculated lease price
    # When I click "Opslaan" in the quotation

    # Print quotation
    # When I choose "Acties" > "Printen" in the quotation
    # Then the window "Printen offerte" for the new quotation is shown with "Gesloten" and "Printer" selected
    # When I click "OK"
    # And I click "Print" in the Windows print dialog
    # Then the printed quotation shows customer "<debtorName>", the new quotation number and lease advisor "Martin"
    # When I close the Snagit Editor

    # Convert to contract
    # When I select line 1 under "Offerte regels"
    # And I click "Contract"
    # Then the window "Maak contract van offerte" for line 1 of the new quotation is shown
    # And the new contract number is the last contract number + 1
    # And "Wilt u het offerte archiveren", "Investering" and "Investering verplicht boeken" are checked
    # When I click "OK"
    # And I open "Leasecontracten" in navigation group "Contracten"
    # Then the new contract is at the top with contractant "<debtorName>" and action "Contractbevestiging printen"

    # Contract confirmation and return
    # When I open the new contract
    # And I choose "Acties" > "Printen" in the contract
    # Then the window "Printen contract" is shown with "Contractbevestiging" selected
    # When I click "OK"
    # And I click "Print" in the Windows print dialog
    # Then the printed document is titled "Contractbevestiging leasecontract" with the new contract number
    # When I close the Snagit Editor
    # Then the contract status is "Het contract is geprint op" today and the action is "Contract retour melden"
    # When I choose "Acties" > "Retour melden" in the contract
    # And I answer "Ja" to "Wilt u dit contract retour melden?"
    # Then the contract status is "Contract retour op" today and the action is "Auto bestellen"

    # Order car
    # When I choose "Acties" > "Nieuwe bestelling" in the contract
    # Then the window "CW2 - Auto bestellen" is shown with title "Nieuwe bestelling" for the new contract
    # When I fill in Crediteur "<creditor>" and press Tab
    # Then Crediteur shows "<creditorName>"
    # When I click "Opslaan" in the order
    # Then the order title is "Wijzigen bestelbon" followed by the new order number
    # When I click "Retourmelden" in the order
    # And I answer "Ja" to "Wilt u deze bestelbon retour melden?"
    # Then Datum retour is today by "<employee>"

    # Register car as delivered
    # When I click "Binnenmelden" in the order
    # And I answer "Ja" to "U heeft deze bestelling nog niet geprint. Toch binnenmelden?"
    # Then the window "Binnenmelden bestelnummer" for the new order is shown
    # When I fill in Kenteken "<licensePlate>" and press Tab
    # Then no existing car is linked to Kenteken "<licensePlate>"
    # Voertuigsoort must be set after Kenteken, because changing Kenteken resets it
    # When I fill in Voertuigsoort "BA" and press Tab
    # Then Voertuigsoort shows "Bestelauto < 3500 / 4 wielen"
    # And Object configuratie is "BA - Bestelauto | 4 banden"
    # When I fill in Chassisnummer "<chassisNumber>" and press Tab
    # Then Meldcode is the last 4 digits of "<chassisNumber>"
    # When I click "OK" in the window "Auto binnenmelden"
    # And I confirm the warning "Bestand C:\Carwise\CW2\ini\ALLIANZ.ini niet gevonden." with "OK"
    # Then the window "Melden verzekeringpolis - Nieuw" is shown for the new contract and Kenteken "<licensePlate>"
    # When I fill in Maatschappij "<insurer>" and press Tab
    # And I select "<insurerCode>" in "Selecteer verzekeringsmaatschappij"
    # Then Maatschappij shows "<insurerName>"
    # And the policy status is "Definitieve auto inzetten/Investering boeken"
    # When I click "OK" in the window "Melden verzekeringpolis"
    # Then the order shows Datum binnen today by "<employee>"
    # And the button "Herstel binnenmelden" is shown
    # When I click "OK" in the order

    # Verification
    # Then the contract shows Kenteken "<licensePlate>", the new order number and Crediteur "<creditorName>"
    # And the contract status is "Definitieve auto inzetbaar"
    # And the contract action is "Definitieve auto inzetten/Investering boeken"
    # When I open "Leasecontracten" in navigation group "Contracten" and refresh
    # Then the new contract has Kenteken "<licensePlate>", soort voertuig "Auto geel" and action "Definitieve auto inzetten/Investering boeken"

    #Examples:
    #  | debtor   | debtorName   | employee | creditor | creditorName                | licensePlate | chassisNumber     | insurer | insurerCode | insurerName           |
    #  | DEBITEUR | mbu_debiteur | MARTIN   | CO2      | Consulting Schouten Experts | MB-MB-19     | 73920581647290318 | BOVEMIJ | BOV         | Bovemij Verzekeringen |
