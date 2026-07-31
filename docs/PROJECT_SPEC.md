# WindowsDevInspector Project Spec

## 1. Product Purpose

WindowsDevInspector is a Windows desktop application for checking whether a developer workstation is ready for selected technology stacks.

The product should help a developer answer:

- What is installed and working?
- What is missing or misconfigured?
- Why does the issue matter?
- Is an automated fix available?
- What risk, elevation, restart, backup, and rollback considerations apply before fixing it?

The first product principle is safety: inspection is read-only by default, and remediation must be explicit, constrained, auditable, and reversible where practical.

## 2. Technology Choices

- Language: C#
- Runtime: .NET 10
- Desktop UI: WPF
- Architecture: MVVM-oriented WPF with clear project boundaries
- Dependency injection: Microsoft.Extensions.DependencyInjection
- Logging: Microsoft.Extensions.Logging or Serilog
- JSON: System.Text.Json
- Tests: xUnit
- Windows integration: Registry, services, optional features, process execution, file system, and environment variables
- Installer/remediation tooling: winget where appropriate

Commercial or restrictive-license dependencies should not be introduced without approval.

## 3. Solution Boundaries

```text
WindowsDevInspector.App
WindowsDevInspector.Core
WindowsDevInspector.Windows
WindowsDevInspector.Remediation
WindowsDevInspector.ElevatedWorker
```

### WindowsDevInspector.App

Responsible for UI, selection state, commands, result display, details, and user confirmation flows.

The app must not directly:

- Write registry values.
- Modify PATH.
- Install software.
- Enable or disable Windows features.
- Execute arbitrary administrator commands.

### WindowsDevInspector.Core

Responsible for domain models and shared rules:

- Check definitions.
- Check results.
- Severity and risk.
- Technology definitions.
- Result sorting.
- Remediation metadata shape.

Core must not depend on WPF or Windows-specific APIs.

### WindowsDevInspector.Windows

Responsible for read-only Windows checks.

All environment checks must:

- Support `CancellationToken`.
- Avoid external command execution in constructors.
- Handle missing commands, timeouts, access denial, and unexpected output.
- Return understandable `CurrentValue`, `ExpectedValue`, and `Impact`.
- Avoid changing system state.

### WindowsDevInspector.Remediation

Responsible for remediation definitions, preview metadata, whitelist validation, backup concepts, rollback concepts, and execution result models.

### WindowsDevInspector.ElevatedWorker

Responsible for the future elevated execution boundary.

The worker must:

1. Verify that it is running elevated.
2. Validate the change plan schema.
3. Execute only approved remediation IDs.
4. Reject arbitrary shell commands.
5. Create backups before changes.
6. Record each remediation result independently.
7. Re-read state after changes.
8. Produce a JSON execution result.
9. Refuse arbitrary registry paths or executable paths from the UI.

## 4. User Flow

1. User selects one or more technologies from grouped searchable lists.
2. User clicks start check.
3. App runs common checks plus checks mapped from selected technologies.
4. Results are sorted with non-pass items first and pass items last.
5. User selects a result to inspect details.
6. Supported low-risk fixes may be selected for batch remediation.
7. Before remediation, the app explains UAC, backup, restart, risk, and rollback.
8. Elevated worker executes only approved changes.
9. App runs checks again after remediation.

## 5. Result Model

Each check result should expose:

```text
Id
Category
Name
Severity
CurrentValue
ExpectedValue
Impact
CanFix
Risk
RequiresElevation
RequiresRestart
SupportsRollback
RemediationId
```

Severity order:

1. Critical
2. Warning
3. Info
4. Pass

Sorting rules:

1. Non-pass results appear before pass results.
2. Higher severity appears first.
3. Category and name provide stable secondary sorting.
4. Pass results do not show a remediation checkbox.

## 6. Environment Selection

Technology selection is the only source of technology-driven diagnostics.

The app should not keep a separate role-selection state such as Frontend, Backend, DBA, QA, DevOps, Mobile, or Desktop because those role labels can imply too many unrelated tools. For example, a backend developer who only needs C# should not automatically get Go, Python, Java, Docker, or Redis checks just because Backend was selected.

Technology selection should be searchable, grouped, and multi-select. Groups may use role-like labels such as Frontend, Backend, Database, QA, DevOps, Mobile, or Desktop for browsing, but selecting a group label must not implicitly select every technology in that group.

The catalog source of truth is `docs/ENVIRONMENT_PROFILES.md`.

## 7. MVP Check Scope

Common baseline checks:

- Windows version and build.
- Processor architecture.
- PATH invalid entries.
- PATH duplicate entries.
- Long Paths.
- Developer Mode.
- `D:\Source`.
- `D:\Projects`.
- `D:\Note`.
- PowerShell 7.
- Git.
- winget.

Technology-driven checks are tracked in `docs/CHECK_CATALOG.md`. Selected technologies without implemented diagnostics should still produce informational visibility instead of disappearing silently.

## 8. Initial Safe Remediation Scope

The first automatic remediation items are limited to:

- Create `D:\Source`.
- Create `D:\Projects`.
- Create `D:\Note`.
- Enable Long Paths.
- Enable Developer Mode.

All remediation must be represented by approved remediation IDs, not arbitrary commands or user-supplied registry paths.

## 9. Security Rules

The product must not:

- Disable Defender.
- Disable Smart App Control.
- Disable UAC.
- Modify arbitrary ACLs.
- Accept arbitrary PowerShell strings from UI.
- Execute remote downloaded scripts.
- Use `cmd /c` with UI-composed commands.
- Log tokens, passwords, cookies, private keys, or connection strings.
- Automatically delete unknown PATH entries.
- Modify registry values without backup.

External input and command output must be treated as untrusted.

## 10. Testing Strategy

Unit tests should cover:

- Check result sorting.
- Check catalog filtering and deduplication.
- Registry value conversion.
- Directory check behavior.
- Command check behavior for missing command, timeout, failure, and success.
- PATH health checks.
- Remediation whitelist validation.

Future integration tests should cover:

- Temporary directory remediation and rollback.
- Fake registry abstractions.
- Fake process runner behavior.
- Worker plan validation.
- Unsupported remediation rejection.

## 11. Current Implementation Notes

The current repository already contains the solution structure, the initial WPF app, core models, Windows read-only check abstractions, remediation whitelist tests, and a growing check catalog.

The immediate direction is to keep expanding small vertical slices:

1. Strengthen scan orchestration and result presentation.
2. Complete first safe remediation flow.
3. Add backup and rollback.
4. Expand PATH, WSL, Docker, .NET, Go, Node, and database diagnostics.
5. Add packaging and release documentation.
