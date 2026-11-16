# CreateLeaseContract

Preparation project for automating the CarWise (CW2) lease contract flow with Reqnroll, NUnit and Playwright.

The CarWise software is currently a Windows desktop application (CW2) without a URL, so the scenarios cannot run with Playwright yet. This project is set up for when a web version becomes available.

## Contents

- `Features/Cw2LeaseContract.feature`: lease quotation → contract → order → register car as delivered, up to action "Definitieve auto inzetten/Investering boeken". Recorded from a manual run in CW2. Tagged `@ignore` until step definitions exist.
- `Tests/PlaywrightHooks.cs`: starts and stops a Chromium browser per scenario and registers the `IPage` for step definitions.
- `Tests/Steps/`: place step definitions here.

## Build

```powershell
dotnet build
```
