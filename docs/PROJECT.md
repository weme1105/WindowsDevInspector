# Project Overview

## Project Name

WindowsDevInspector

## Purpose

WindowsDevInspector is a Windows desktop application for inspecting whether a developer workstation is ready for selected technology stacks.

The product explains what is installed, missing, risky, or ready to use. It prioritizes read-only diagnostics first, then explicit and auditable remediation through a separate elevated worker.

## Business Domain

Windows developer workstation diagnostics and safe local environment remediation.

## Technology Selection

- Language: C#.
- Runtime: .NET 10.
- Desktop UI: WPF.
- Architecture: MVVM-oriented WPF with clear project boundaries.
- Dependency injection: `Microsoft.Extensions.DependencyInjection`.
- Logging: `Microsoft.Extensions.Logging` or Serilog.
- JSON: `System.Text.Json`.
- Tests: xUnit.
- Windows integration: Registry, services, optional features, process execution, file system, and environment variables.
- Installer/remediation tooling: winget where appropriate.
- Commercial or restrictive-license dependencies require approval.

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
│  ├─ WindowsDevInspector.Remediation.Tests
│  └─ WindowsDevInspector.ElevatedWorker.Tests
├─ installer
│  └─ WindowsDevInspector.Installer
├─ profiles
├─ docs
└─ artifacts
```

When source or test projects are added, removed, or renamed, update this
structure and the related architecture sections in this document in the same
change.

## Target Users

- Windows-based software developers.
- Developers working across frontend, backend, database, QA, DevOps, mobile, or desktop stacks.
- Developers who want clear remediation context before making workstation changes.

## Core Features

- Common baseline environment checks.
- Multi-select technology catalog.
- Searchable grouped technology selector.
- Selected technology persistence beside the executable.
- Check catalog resolution from selected technology IDs to deduplicated check IDs.
- Read-only Windows checks for directories, registry values, services, optional features, PATH health, and command versions.
- Result sorting with non-pass results before pass results.
- Optional hiding of PASS results in the result list.
- Result detail panel with full current value visibility.
- Environment score calculation.
- Safe remediation metadata and whitelist validation.
- Directory remediation for approved local folders.
- Elevated registry remediation for approved DWORD values.
- DPAPI-protected backup files.
- Backup browser and elevated rollback flow.
- JSON scan report export.
- MVP privacy statement, release notes, and demo screenshot capture plan.

## System Scope

In scope:

- Inspecting Windows development prerequisites.
- Explaining current value, expected value, impact, risk, remediation availability, elevation, restart, and rollback metadata.
- Running safe approved remediation only through known remediation IDs.

Out of scope for the current implementation:

- Arbitrary command execution.
- Arbitrary registry editing.
- Disabling Windows security features.
- Broad package installation automation.
- External technology selection loading.
- Installer, signing, and update distribution.
- Runtime telemetry or remote report upload.

## High-Level Architecture

```text
WindowsDevInspector.App
  -> WindowsDevInspector.Core
  -> WindowsDevInspector.Windows
  -> WindowsDevInspector.Remediation
  -> WindowsDevInspector.ElevatedWorker process boundary
```

`WindowsDevInspector.Core` owns shared domain concepts. `WindowsDevInspector.Windows` owns read-only system inspection. `WindowsDevInspector.Remediation` owns remediation definitions, validation, backup, and rollback models. `WindowsDevInspector.ElevatedWorker` is the privileged execution boundary.

## Main Modules

- `WindowsDevInspector.App`: WPF UI, technology selection, result display, PASS-result filtering, report export, backup selection, and App-layer orchestration services.
- `WindowsDevInspector.Core`: check definitions, technology definitions, check catalog, result model, sorting, risk, severity, and environment score.
- `WindowsDevInspector.Windows`: read-only environment checks and Windows abstractions.
- `WindowsDevInspector.Remediation`: change plan validation, remediation whitelist, directory remediation, registry DWORD remediation, backup, and rollback support.
- `WindowsDevInspector.ElevatedWorker`: elevated registry remediation, rollback, and controlled single-package installation execution.
- `WindowsDevInspector.Installer`: unsigned WiX v5 MSI authoring for WindowsDevInspector files, Major Upgrade, and Start Menu shortcut lifecycle; generated MSI/CAB artifacts are not versioned.
- `scripts/Test-InstallerPackage.ps1`: read-only MSI database validation for product identity, App/Worker payload, Major Upgrade rows, shortcut target, and prohibited bundled software names.

## External Integrations

- Windows Registry.
- Windows Services.
- Windows Optional Features through command-line inspection.
- Local file system.
- External CLI tools such as `dotnet`, `git`, `winget`, `wsl`, `docker`, `npm`, `go`, `python`, and related developer tools.
- DPAPI for backup protection.

## Runtime and Deployment Overview

- Runtime: .NET 10.
- Desktop UI: WPF.
- Deployment: framework-dependent local Windows desktop app. The unsigned x64 WiX MSI requires .NET 10 Desktop Runtime x64 and does not bundle it; signing, auto update, and external release publishing remain deferred.

## Current Project Status

The project has completed the initial diagnostic MVP plus Windows baseline checks, executable checks for all current Core catalog IDs, clarified WSL/Docker diagnostics, advanced read-only Windows diagnostics for firewall profiles, localhost bind health, Code Integrity events, and Smart App Control state, the first safe remediation, reversible backup and rollback for approved remediation IDs, scan report, scoring, App-layer service extraction, App service test coverage, WPF smoke-test documentation, MVP technology scope, select-all technology selection, and first-pass result remediation selection behavior.

The next development phase should focus on installer/signing decisions and eventual WPF automation or ViewModel extraction.

## Known Constraints

- Normal UI code must not perform privileged system modifications directly.
- Registry modifications must go through the elevated worker and approved remediation IDs.
- External command output is untrusted and may require robust decoding.
- Windows PowerShell sessions should use explicit UTF-8 settings when reading or piping Traditional Chinese project files.
- Commercial or restrictive-license dependencies require approval.
