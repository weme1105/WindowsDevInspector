# WindowsDevInspector

WindowsDevInspector is a Windows desktop tool for inspecting a developer workstation and explaining what is missing, risky, or ready to use.

The project focuses on read-only diagnostics first, then safe and auditable remediation through a separate elevated worker.

## Goals

- Inspect common Windows developer environment prerequisites.
- Let users select roles and technologies instead of forcing one fixed profile.
- Show clear results with current value, expected value, impact, risk, and remediation availability.
- Keep normal UI code read-only and route system changes through a validated elevated worker.
- Build a small, testable vertical slice before expanding the check catalog.

## Current Scope

The current implementation is an early MVP for:

- Common baseline checks.
- Role and technology selection.
- Technology catalog and selected technology persistence.
- Check result sorting with non-pass items before pass items.
- Result detail display.
- Initial read-only Windows checks for directories, registry values, services, optional features, PATH health, and command versions.
- Initial remediation metadata and whitelist validation.

## Solution Structure

```text
WindowsDevInspector.sln
├─ src
│  ├─ WindowsDevInspector.App
│  ├─ WindowsDevInspector.Core
│  ├─ WindowsDevInspector.Windows
│  ├─ WindowsDevInspector.Remediation
│  └─ WindowsDevInspector.ElevatedWorker
├─ tests
│  ├─ WindowsDevInspector.Core.Tests
│  ├─ WindowsDevInspector.Windows.Tests
│  └─ WindowsDevInspector.Remediation.Tests
└─ docs
```

## Architecture

- `WindowsDevInspector.App`: WPF UI, selection state, view models, navigation, and result presentation.
- `WindowsDevInspector.Core`: domain models, check definitions, result sorting, severity, and risk concepts.
- `WindowsDevInspector.Windows`: read-only Windows environment checks.
- `WindowsDevInspector.Remediation`: remediation definitions and whitelist validation.
- `WindowsDevInspector.ElevatedWorker`: future elevated execution boundary for approved fixes.

The app project must not directly write registry values, modify PATH, install software, enable Windows features, or execute administrator changes.

## Environment Selection

Users can select one or more roles:

- Frontend Engineer
- Backend Engineer
- DBA / Data Engineer
- QA / Test Engineer
- DevOps / SRE
- Mobile Engineer
- Desktop Engineer

Users can also select technologies from grouped searchable lists. Unsupported technologies should still be recognized and reported as informational items when automated diagnostics are not implemented yet.

## Build And Test

```powershell
dotnet restore
dotnet build
dotnet test
```

For consistent UTF-8 terminal behavior on Windows PowerShell:

```powershell
.\scripts\Use-Utf8.ps1
```

## Documentation

- [Project Spec](docs/PROJECT_SPEC.md)
- [Check Catalog](docs/CHECK_CATALOG.md)
- [Environment Profiles](docs/ENVIRONMENT_PROFILES.md)
- [Roadmap](docs/ROADMAP.md)
- [Codex Development Setup](docs/CODEX_DEVELOPMENT_SETUP.md)
- [Agent Instructions](AGENTS.md)
