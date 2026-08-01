# WindowsDevInspector

WindowsDevInspector is a Windows desktop tool for inspecting a developer workstation and explaining what is missing, risky, or ready to use.

The project focuses on read-only diagnostics first, then safe and auditable remediation through a separate elevated worker.

## Goals

- Inspect common Windows developer environment prerequisites.
- Let users explicitly select technologies from grouped searchable lists instead of forcing one fixed preset.
- Show clear results with current value, expected value, impact, risk, and remediation availability.
- Keep normal UI code read-only and route system changes through a validated elevated worker.
- Build a small, testable vertical slice before expanding the check catalog.

## Current Scope

The current implementation covers the initial diagnostics and safe remediation slices:

- Common baseline checks.
- Searchable grouped technology selection.
- Technology catalog and selected technology persistence.
- Check result sorting with non-pass items before pass items.
- Result detail display.
- Environment score calculation.
- Read-only Windows checks for directories, registry values, services, optional features, PATH health, and command versions.
- Read-only WSL, Docker, NuGet source, Visual Studio Build Tools, browser availability, Android SDK, and .NET MAUI diagnostics.
- Safe remediation metadata and whitelist validation.
- Approved directory remediation and registry DWORD remediation through the elevated worker.
- DPAPI-protected backup files and rollback support.
- Backup browsing and JSON scan report export.

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
│  ├─ WindowsDevInspector.App.Tests
│  ├─ WindowsDevInspector.Windows.Tests
│  └─ WindowsDevInspector.Remediation.Tests
└─ docs
```

## Architecture

- `WindowsDevInspector.App`: WPF UI, selection state, scan orchestration, result presentation, report export, backup selection, and worker launch.
- `WindowsDevInspector.Core`: domain models, check definitions, result sorting, scoring, severity, and risk concepts.
- `WindowsDevInspector.Windows`: read-only Windows environment checks.
- `WindowsDevInspector.Remediation`: remediation definitions, whitelist validation, backup, rollback, and change plan models.
- `WindowsDevInspector.ElevatedWorker`: elevated execution boundary for approved fixes and rollback.

The app project must not directly write registry values, modify PATH, install software, enable Windows features, or execute administrator changes.

## Environment Selection

Users select technologies from grouped searchable lists. Groups may use familiar labels such as Frontend, Backend, Database, QA, DevOps, Mobile, or Desktop for browsing, but group labels do not implicitly select every technology in that area.

Unsupported selected technologies should still be recognized and reported as informational items when automated diagnostics are not implemented yet.

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
- [UI Smoke Tests](docs/UI_SMOKE_TESTS.md)
- [Privacy Statement](docs/PRIVACY.md)
- [Release Notes](docs/RELEASE_NOTES.md)
- [Demo Screenshots](docs/DEMO_SCREENSHOTS.md)
- [Roadmap](docs/ROADMAP.md)
- [Codex Development Setup](docs/CODEX_DEVELOPMENT_SETUP.md)
- [Agent Instructions](AGENTS.md)
