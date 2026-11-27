# CreateLeaseContract

Preparation project for automating CarWise (CW2) lease contract and RentWise flows with Reqnroll, NUnit and Playwright.

The CarWise software is currently a Windows desktop application (CW2) without a URL, so the scenarios cannot run with Playwright yet. This project is set up for when a web version becomes available.

## Contents

- `Features/Cw2LeaseContract.feature`: lease quotation → contract → order → register car as delivered, up to action "Definitieve auto inzetten/Investering boeken". Recorded from a manual run in CW2. Tagged `@ignore` until step definitions exist.
- `Features/Cw2RentWiseDigitalSigning.feature`: RentWise rental agreement → tablet signing → verify the signed document in DocWise. Recorded from a manual CW2 flow. Tagged `@ignore` until step definitions exist.
- `Tests/PlaywrightHooks.cs`: starts and stops a Chromium browser per scenario and registers the `IPage` for step definitions.
- `Tests/Steps/`: place step definitions here.

## Build

```powershell
dotnet build
```

## Module navigation

`I navigate to LeaseWise Activiteiten in the modules` opens the module, collapses
`Algemeen`, expands `Activiteiten`, and clicks that group's `Startpagina`.
The heading then shows `LeaseWise · Activiteiten`. The `Algemeen` suffix opens
that group's `Startpagina`; without a suffix, the module's default navigation
state is preserved.

For explicit menu navigation, open the module without a suffix, use
`I close Algemeen` to collapse that group, then
`I open Activiteiten -> Leaseoffertes in the main menu`. Each group in the path
is expanded before the final item is clicked. Closing an already collapsed group
does not reopen it.

## RentWise reservation conditions

Scenario 03 selects `Basis Soort` from the table and the first valid `Basis Eenheid`
offered by its lookup list, recording the actual unit in the reservation model.
Available units depend on the selected vehicle; `MAAND20` is not assumed.
The test waits for each selected code's resolved description before continuing,
because resolving Soort can reset Eenheid to a default value. Tariff popups are
closed only by the registered locator handler, avoiding competing close actions.
