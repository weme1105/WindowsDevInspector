# Project Overview

## Project Name

WindowsDevInspector

## Purpose

WindowsDevInspector is a Windows desktop application for inspecting whether a developer workstation is ready for selected technology stacks.

The product explains what is installed, missing, risky, or ready to use. It prioritizes read-only diagnostics first, then explicit and auditable remediation through a separate elevated worker.

## Business Domain

Windows developer workstation diagnostics and safe local environment remediation.

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
- `WindowsDevInspector.ElevatedWorker`: elevated registry remediation and rollback execution.

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
- Deployment: local Windows desktop app. Installer, signing, auto update, and release packaging are not yet implemented.

## Current Project Status

The project has completed the initial diagnostic MVP plus the first safe remediation, backup, rollback, scan report, scoring, App-layer service extraction, and App service test coverage slices.

There is an uncommitted UI polish change that moves report export to the result header, adds a "hide PASS results" toggle, and constrains the selected-fix button width. After that is reviewed and committed, the next development phase should focus on WPF smoke-test coverage and Phase 4/5 diagnostic expansion.

## Known Constraints

- Normal UI code must not perform privileged system modifications directly.
- Registry modifications must go through the elevated worker and approved remediation IDs.
- External command output is untrusted and may require robust decoding.
- Windows PowerShell sessions should use explicit UTF-8 settings when reading or piping Traditional Chinese project files.
- Commercial or restrictive-license dependencies require approval.
