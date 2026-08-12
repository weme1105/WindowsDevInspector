# Important Decisions

## Decision Index

| ID | Decision | Status | Date |
|----|----------|--------|------|
| DEC-001 | Use a separate elevated worker for privileged remediation | Accepted | 2026-07-29 |
| DEC-002 | Keep environment checks read-only by default | Accepted | 2026-07-29 |
| DEC-003 | Resolve selected technologies through a check catalog | Accepted | 2026-07-29 |
| DEC-004 | Limit initial automatic remediation to approved low-risk items | Accepted | 2026-07-29 |
| DEC-005 | Protect registry rollback backups with DPAPI | Accepted | 2026-07-29 |
| DEC-006 | Use explicit command-output decoding for Windows CLI checks | Accepted | 2026-07-29 |
| DEC-007 | Keep App orchestration in testable App-layer services | Accepted | 2026-07-29 |
| DEC-008 | Use `D:\Note` as the standard notes directory | Accepted | 2026-07-31 |
| DEC-009 | Use `mvp` as the integration branch before `main` | Accepted | 2026-07-31 |
| DEC-010 | Use explicit technology selection instead of independent role selection | Accepted | 2026-07-31 |
| DEC-011 | Keep Phase 4 package planning read-only until install plans are approved | Accepted | 2026-07-31 |
| DEC-012 | Keep TASK focused on active handoff state | Accepted | 2026-08-04 |
| DEC-013 | Derive installation execution metadata from an approved package catalog | Accepted | 2026-08-12 |
| DEC-014 | Keep package execution behind two disabled gates | Accepted | 2026-08-12 |
| DEC-015 | Enable controlled single-package installation through ElevatedWorker | Accepted | 2026-08-12 |
| DEC-016 | Verify installed CLI from refreshed process PATH | Accepted | 2026-08-12 |
| DEC-017 | Use unsigned WiX v5 MSI without bundled third-party tools | Accepted | 2026-08-12 |
| DEC-018 | Hide installation actions until exact winget package availability passes | Accepted | 2026-08-12 |
| DEC-019 | Framework-dependent deployment with a required .NET 10 Desktop Runtime | Accepted | 2026-08-12 |
| DEC-020 | Retain versioned MSI files as GitHub Release assets | Accepted | 2026-08-13 |
| DEC-021 | Centralize WPF action availability and type the Worker CLI router | Accepted | 2026-08-13 |

---

## DEC-001: Use a Separate Elevated Worker for Privileged Remediation

### Status

Accepted

### Date

2026-07-29

### Context

The app needs to inspect and eventually fix Windows developer environment settings, but normal UI code must remain safe and must not directly perform privileged system changes.

### Decision

Privileged remediation is executed by `WindowsDevInspector.ElevatedWorker`. The app writes a change plan and launches the worker with elevation. The worker validates elevation, validates the plan, executes only approved remediation IDs, and writes a JSON result.

### Alternatives Considered

- Perform registry writes directly from the WPF app.
  - Advantages: simpler implementation.
  - Disadvantages: violates safety boundary and makes arbitrary UI-triggered modification harder to constrain.

### Rationale

A separate worker creates an auditable process boundary and keeps privileged behavior constrained.

### Consequences

#### Positive

- Clear privilege boundary.
- Safer review surface for remediation.
- Easier to reject arbitrary commands and registry paths.

#### Negative

- More process orchestration complexity in the app.
- Requires UAC and result-file handling.

### Impacted Areas

- App
- Remediation
- ElevatedWorker
- Security
- Testing

### Follow-up Actions

- [x] Move worker launch orchestration out of `MainWindow.xaml.cs`.

---

## DEC-002: Keep Environment Checks Read-Only by Default

### Status

Accepted

### Date

2026-07-29

### Context

Developer workstation checks must be safe to run repeatedly and should not modify system state without explicit user action.

### Decision

All `IEnvironmentCheck` implementations in `WindowsDevInspector.Windows` are read-only. They may inspect registry, services, optional features, PATH, files, and commands, but must not modify system state.

### Alternatives Considered

- Let checks fix small issues while scanning.
  - Advantages: fewer user steps.
  - Disadvantages: surprising side effects and weaker auditability.

### Rationale

Read-only diagnostics keep the scan safe and predictable.

### Consequences

#### Positive

- Scans are low risk.
- Check behavior is easier to test.
- User consent is preserved.

#### Negative

- Fixing requires a separate flow.

### Impacted Areas

- Windows checks
- App workflow
- Remediation

### Follow-up Actions

- [ ] Keep future checks read-only unless a separate remediation item is explicitly added.

---

## DEC-003: Resolve Selected Technologies Through a Check Catalog

### Status

Accepted

### Date

2026-07-29

### Context

Users can select multiple technologies that share underlying checks, such as Node.js being used by frontend, backend, QA, and mobile workflows.

### Decision

Technology selections resolve to check IDs through `BuiltInCheckCatalog` and `CheckCatalog`. Shared check IDs are deduplicated before execution.

### Alternatives Considered

- Hardcode checks directly in UI selection handlers.
  - Advantages: initially simple.
  - Disadvantages: duplicates behavior and makes catalog growth difficult.

### Rationale

A catalog separates user-facing technology grouping from executable diagnostics.

### Consequences

#### Positive

- Shared checks run once.
- Technology groups remain display concerns.
- Unsupported or pending checks can remain visible.

#### Negative

- Catalog and executable check factory must stay synchronized.

### Impacted Areas

- Core
- App
- Windows checks
- Documentation

### Follow-up Actions

- [x] Add catalog/factory consistency tests for current Core check IDs versus executable check IDs.

---

## DEC-004: Limit Initial Automatic Remediation to Approved Low-Risk Items

### Status

Accepted

### Date

2026-07-29

### Context

The project needs remediation, but early automation should avoid high-risk or broad workstation changes.

### Decision

Initial remediation is limited to:

- Create `D:\Source`.
- Create `D:\Projects`.
- Create `D:\Note`.
- Enable Long Paths.
- Enable Developer Mode.

Each item is represented by a stable remediation ID and must be allowed by the remediation whitelist.

### Alternatives Considered

- Add general winget installation and arbitrary registry fixes immediately.
  - Advantages: broader automation.
  - Disadvantages: larger security and support surface.

### Rationale

The first remediation slice should be small, reviewable, reversible where practical, and safe enough to validate.

### Consequences

#### Positive

- Lower blast radius.
- Easier to test.
- Clear user trust model.

#### Negative

- Many diagnostics remain informational without automated repair.

### Impacted Areas

- Remediation
- ElevatedWorker
- App
- Security

### Follow-up Actions

- [ ] Design winget remediation separately before enabling package installation.

---

## DEC-005: Protect Registry Rollback Backups with DPAPI

### Status

Accepted

### Date

2026-07-29

### Context

Registry remediation needs rollback data. Backup files should not be plain unrestricted state files if they may contain machine-specific configuration values.

### Decision

Registry rollback backups are written through `BackupFileService` and protected with DPAPI using `DpapiBackupProtector`.

### Alternatives Considered

- Store backup JSON as plain text.
  - Advantages: easier debugging.
  - Disadvantages: weaker protection for local machine state.

### Rationale

DPAPI is available on Windows and fits local machine backup protection without adding external services.

### Consequences

#### Positive

- Backup contents are protected at rest.
- Backup files are bound to local Windows protection capabilities.

#### Negative

- Backups may not be portable across machines or users.
- Debugging backup contents requires controlled tooling.

### Impacted Areas

- Remediation
- ElevatedWorker
- Rollback
- Security

### Follow-up Actions

- [ ] Document backup portability limitations in user-facing remediation confirmation.

---

## DEC-006: Use Explicit Command-Output Decoding for Windows CLI Checks

### Status

Accepted

### Date

2026-07-29

### Context

Windows command output can use different encodings. Some commands, including WSL status output, may emit UTF-16LE without BOM.

### Decision

`ProcessCommandRunner` reads raw output bytes and decodes using BOM detection, UTF-16LE heuristics, UTF-8, console encoding, and ANSI fallback with penalty scoring.

### Alternatives Considered

- Rely only on PowerShell or .NET default stream encoding.
  - Advantages: simpler.
  - Disadvantages: produces unreadable output for some Windows commands.

### Rationale

Raw-byte decoding gives reliable diagnostic text without requiring permanent system encoding changes.

### Consequences

#### Positive

- Better output quality for localized Windows environments.
- Avoids mutating global console or OS settings.

#### Negative

- More decoding logic to test and maintain.

### Impacted Areas

- Windows checks
- Process execution
- Diagnostics

### Follow-up Actions

- [ ] Add direct tests for UTF-16LE-without-BOM command output decoding if not already covered.

---

## DEC-007: Keep App Orchestration in Testable App-Layer Services

### Status

Accepted

### Date

2026-07-29

### Context

`MainWindow.xaml.cs` had accumulated scan execution, report export, remediation, elevated worker launch, rollback, and backup listing logic. This made behavior harder to test without WPF UI automation.

### Decision

Keep WPF event handling and UI state in `MainWindow.xaml.cs`, but delegate workflow logic to App-layer services such as `EnvironmentScanService`, `ScanReportExporter`, `RemediationCoordinator`, and `TechnologySelectionConfigStore`.

### Alternatives Considered

- Move directly to a full MVVM rewrite.
  - Advantages: cleaner long-term WPF architecture.
  - Disadvantages: larger change and higher regression risk while core behavior is still evolving.
- Leave all logic in code-behind.
  - Advantages: fewer files.
  - Disadvantages: weak testability and harder future maintenance.

### Rationale

Small App-layer services provide immediate testability while preserving the current UI behavior and avoiding a broad rewrite.

### Consequences

#### Positive

- Scan and configuration behavior can be tested without UI automation.
- `MainWindow.xaml.cs` is smaller and more focused on UI state.
- Future MVVM extraction has clearer seams.

#### Negative

- Some UI behavior still requires manual smoke testing.
- `RemediationCoordinator` still launches external elevated processes, so only part of the flow is unit-test friendly.

### Impacted Areas

- App
- Tests
- Documentation

### Follow-up Actions

- [x] Add a WPF smoke-test checklist and staged UI automation strategy.
- [ ] Continue moving UI state into ViewModels when behavior stabilizes.

---

## DEC-008: Use `D:\Note` as the Standard Notes Directory

### Status

Accepted

### Date

2026-07-31

### Context

The product direction was clarified to use the shorter standard directory name `D:\Note` for learning notes and diagnostics.

### Decision

Use `D:\Note` for the common notes directory check and approved directory remediation.

The related check ID and remediation ID are:

- Check ID: `common.directory-note`
- Remediation ID: `create-note-directory`

### Consequences

#### Positive

- Directory naming is shorter and easier to explain.
- Check catalog, remediation whitelist, and setup documentation use one consistent path.

#### Negative

- Existing local notes in any older personal directory are not automatically migrated.

### Impacted Areas

- Core check catalog
- Windows checks
- Remediation catalog
- Documentation

---

## DEC-009: Use `mvp` as the Integration Branch Before `main`

### Status

Accepted

### Date

2026-07-31

### Context

The repository now has `main`, `mvp`, and feature branches. The project needs a predictable flow that keeps `main` stable while allowing MVP work to be integrated and tested before promotion.

### Decision

Use this branch model:

```text
main = stable validated baseline
mvp = MVP integration and validation branch
feature/<task-name> = focused development branch
```

Feature work must branch from the latest active integration branch and open PRs back into that same integration branch.

For MVP work:

```text
main
  -> mvp
      -> feature/<task-name>
      -> PR back to mvp
```

For later production release work:

```text
main or master
  -> prd/<version>
      -> feature/<task-name>
      -> PR back to prd/<version>
```

Do not continue new feature work on a feature branch after its PR has been merged. A merged feature branch is closed for new work. Start the next task from the current integration branch instead.

After MVP validation is complete, open a separate PR from `mvp` into `main`.

When a production release branch is ready to update the stable line, open a PR from `prd/<version>` into `main` or `master`.

CI should run for pull requests that target a mainline branch (`mvp` or `main`) and for pushes to those mainline branches after merge. Feature branch pushes do not need CI by default; a feature branch should be validated when it opens or updates a PR into a mainline branch.

### Rationale

This keeps `main` reserved for validated states while letting the MVP branch collect small reviewed slices.

### Consequences

#### Positive

- `main` remains stable.
- MVP validation can happen before promotion.
- Feature PR targets are consistent.
- Feature branch pushes do not spend CI time until they are proposed for a mainline branch.
- Merged feature branches do not accumulate unrelated future work.
- Production release branches can stabilize independently before updating `main` or `master`.

#### Negative

- Work must be kept synchronized with `mvp`.
- PR base branches need to be checked before creation.
- Mistaken work on an already merged feature branch must be moved to a new feature branch from the current integration branch, normally by cherry-picking the relevant commits.

### Follow-up Actions

- [ ] Keep draft feature PRs targeted at `mvp` unless the task is a hotfix for `main`.
- [ ] Promote `mvp` to `main` only after build, tests, and manual MVP validation pass.
- [ ] For PRD releases, branch `prd/<version>` from `main` or `master`, merge feature PRs into that PRD branch, and merge PRD back to the stable line only when releasing.

---

## DEC-010: Use Explicit Technology Selection Instead of Independent Role Selection

### Status

Accepted

### Date

2026-07-31

### Context

Role labels such as Backend, Frontend, DevOps, or QA are too broad for environment diagnostics. A backend developer may only need C# and .NET checks, while a broad Backend role could imply Go, Python, Java, Docker, Redis, and other tools that are irrelevant to that user.

Automatically adding checks from selected roles would create noisy results and force users to remove technologies they never asked to inspect.

### Decision

Remove independent role selection as a product concept.

Technology selection is the source of truth for technology-driven diagnostics. Role-like labels may remain only as browsing groups, filters, or display categories. Selecting a group label must not implicitly select every technology in that group.

Common baseline checks still run even when no technology is selected.

### Consequences

#### Positive

- Scans reflect explicit user intent.
- Result noise is lower.
- Check catalog behavior remains easier to reason about and test.

#### Negative

- First-time users need a clear grouped technology picker.
- Future selection persistence must keep using explicit technology IDs and avoid reintroducing broad implicit role presets.

### Impacted Areas

- Product spec
- Environment selection documentation
- Future UI selection model
- Technology selection config persistence

---

## DEC-011: Keep Phase 4 Package Planning Read-Only Until Install Plans Are Approved

### Status

Accepted

### Date

2026-07-31

### Context

Phase 4 introduces tool installation planning for packages such as PowerShell 7, pnpm, Azure CLI, kubectl, and Terraform. Installing packages can modify Program Files, PATH, shims, services, and user or machine configuration.

### Decision

The first Phase 4 slice only checks whether known winget package IDs are available from configured sources.

The app may run read-only commands such as:

```text
winget show --id <knownPackageId> --exact --accept-source-agreements
```

It must not install packages, accept arbitrary package IDs from the UI, or generate remediation plans for package installation until an approved installation plan schema and safety model exist.

The app should not require a tool to be on the newest available package version. If a user already has an older working version, the scan may show that version as informational context, but it should not force installation or upgrade unless the user explicitly chooses that future action.

### Consequences

#### Positive

- Users can see whether planned package IDs are resolvable before any installation feature exists.
- Package checks stay auditable and non-mutating.
- Older installed tools can remain acceptable when they are still usable for the selected workflow.
- Future install automation has a clear design gate.

#### Negative

- Missing packages still require manual installation outside the app.
- winget source availability depends on the user's local winget configuration and network state.

### Impacted Areas

- Windows checks
- Check catalog
- Future remediation design
- Security

---

## DEC-012: Keep TASK Focused on Active Handoff State

### Status

Accepted

### Date

2026-08-04

### Context

`docs/TASK.md` was originally used to preserve state across Codex conversations, but its Completed section accumulated a long implementation history. That makes the current objective, blockers, verification, and next action harder to find and duplicates information already available from Git, pull requests, tests, release notes, and durable decision records.

### Decision

At handoff, keep `docs/TASK.md` focused on current work and the next actionable slice.

- Move completed work that creates a durable architecture, security, technology, product, compatibility, deployment, or workflow decision into `docs/DECISION.md` before removing it from `docs/TASK.md`.
- Do not represent routine implementation history as a decision. Remove routine completed items after confirming durable evidence in Git, pull requests, tests, or release notes.
- If complete historical retention is explicitly required, use a separate Completed Work Archive section rather than mixing routine work into the decision index.
- Update Current Objective, In Progress, blockers, remaining work, known issues, latest verification, and Next Recommended Task during every handoff.
- Update `docs/AI_CONTEXT.md` only when stable technical context changes.

The reusable execution workflow is provided by the personal `prepare-project-handoff` Skill.

### Consequences

#### Positive

- New conversations can identify the current state and next step quickly.
- Decision history retains rationale instead of becoming a generic completion log.
- Routine implementation history remains available from its authoritative evidence.

#### Negative

- Handoff requires classifying completed work before removing it from TASK.
- Existing historical Completed entries require a one-time cleanup pass.

### Impacted Areas

- Repository Agent rules
- TASK handoff process
- Decision records
- AI context maintenance

---

## DEC-013: Derive Installation Execution Metadata from an Approved Package Catalog

### Status

Accepted

### Date

2026-08-12

### Context

Package installation can modify installed programs, PATH, shims, and machine configuration. Allowing the UI to provide package commands, executable paths, arguments, or source URLs would broaden the trusted input boundary and make a plan difficult to audit.

### Decision

An installation plan contains only a GUID plan ID and typed items identifying an exact package ID, source, and action. `InstallationPlanValidator` accepts only packages represented by `BuiltInInstallationCatalog`, rejects unknown JSON fields and unsupported values, and limits plan size. Approved packages also map to an exact diagnostic check ID so App-layer candidate selection can derive installation choices only from known non-pass CLI diagnostics.

Risk, elevation, restart, PATH refresh, verification executable, and verification arguments are trusted metadata owned by the approved catalog. They are not accepted from UI-authored plan items.

This schema and validator do not execute installations. Package installation remains disabled until a separate execution design and approval gate are completed.

### Consequences

#### Positive

- Installation intent is narrow and auditable.
- Arbitrary commands, executables, arguments, sources, and package IDs are excluded from the plan contract.
- Confirmation and future execution can use one trusted metadata source.
- Passing and unrelated diagnostics do not become installation candidates.

#### Negative

- Adding or changing an installable package requires a reviewed catalog change.
- The current schema supports only winget and the install action.

### Impacted Areas

- Remediation
- Future App confirmation flow
- Future ElevatedWorker installation flow
- Security
- Testing

## DEC-014: Keep Package Execution Behind Two Disabled Gates

### Status

Accepted

### Date

2026-08-12

### Context

Installation plans and UI previews now exist, but enabling a real process runner before agreement, elevation, scope, timeout, verification, and non-rollback behavior are approved would cross the read-only planning boundary.

### Decision

`PackageInstallationExecutor` requires both `PackageInstallationExecutorOptions.ExecutionEnabled` and an injected `IPackageInstallationProcessRunner`. The option defaults to `false`, and the only built-in runner is `DisabledPackageInstallationProcessRunner`, which never starts a process.

The shared command preview is limited to fixed tokens derived from the approved catalog:

```text
winget install --id <approved-package-id> --exact --source winget
```

The preview intentionally excludes agreement acceptance, silent mode, version, scope, override, and arbitrary arguments until those policies are explicitly approved.

### Consequences

- Valid plans can be exercised through result and preview models without changing the workstation.
- UI and executor previews share one builder and cannot silently drift.
- Enabling the option alone is insufficient because the built-in runner remains disabled.
- A real runner and ElevatedWorker routing require a separate approved change.

### Impacted Areas

- Remediation
- App confirmation
- Future ElevatedWorker installation flow
- Security
- Testing

---

## DEC-015: Enable Controlled Single-Package Installation Through ElevatedWorker

### Status

Accepted

### Date

2026-08-12

### Decision

Enable only one approved package per installation plan. App first creates a preview and disables competing actions; a separate red execution button requires a second explicit safety confirmation before launching ElevatedWorker with `--install` and UAC.

ElevatedWorker validates the plan again and permits only this fixed-token command:

```text
winget install --id <approved-package-id> --exact --source winget --accept-package-agreements --accept-source-agreements
```

The runner uses `ProcessStartInfo.ArgumentList`, a five-minute timeout, and process-tree termination for timeout or cancellation. It does not add silent, override, forced scope, or forced version flags. Failure does not trigger automatic uninstall or rollback. Debug simulation rows remain non-executable.

### Compatibility

The `--install <installation-plan.json> [result.json]` form is additive. Existing `<change-plan.json> [result.json]` and `--rollback <backup.json> [result.json]` forms remain supported. Invalid input returns exit code 2 for usage, 3 for unreadable JSON, 4 for rejected plans, and 6 for an attempted installation that did not succeed.

### Consequences

- App never executes winget directly.
- Package identity and all execution tokens remain catalog/worker-owned.
- Post-install PATH refresh and CLI verification are implemented by DEC-016.

---

## DEC-016: Verify Installed CLI From Refreshed Process PATH

### Status

Accepted

### Date

2026-08-12

### Decision

After winget reports success, ElevatedWorker refreshes only its current process PATH by combining current machine and user PATH values. It does not write persistent environment variables. The worker then resolves the approved package catalog's verification executable and runs its fixed arguments with a 30-second timeout. Native executables run directly; `.cmd`/`.bat` shims use the system `cmd.exe` with a fixed wrapper and reject catalog arguments containing shell metacharacters.

Verification success is required for the overall package-installation result to be successful. Missing CLI, non-zero exit, or timeout preserves the fact that winget succeeded in the message but marks the structured item outcome as failed. Raw verification stdout/stderr is not returned to App.

The `verification` result field is additive and optional so a new App can still deserialize results produced by an older Worker during a mixed-version transition.

### Consequences

- UI-authored plans cannot select a verification executable or arguments.
- PATH refresh cannot persistently alter machine or user configuration.
- Installation and verification have independent timeout/result semantics.

---

## DEC-017: Use Unsigned WiX v5 MSI Without Bundled Third-Party Tools

### Status

Accepted

### Date

2026-08-12

### Decision

Use an unsigned WiX Toolset v5.0.2 SDK-style MSI project for the first installer slice. WiX v5 uses the MS-RL package and avoids the maintenance-fee policy introduced in WiX v6+. The per-machine MSI installs only WindowsDevInspector application output into Program Files, manages a per-user Start Menu shortcut, supports Major Upgrade/downgrade blocking, and leaves signing for a later release gate.

The MSI must not bundle third-party developer tools, winget packages, a .NET Runtime installer, or certificate material. Generated MSI/CAB artifacts stay under ignored build output and are never committed.

Every distributed MSI must also be retained as an immutable release artifact outside Git. Rebuilding another package at the same path/name does not replace the exact original source expected by Windows Installer repair; losing or overwriting that package can cause repair error 1706.

### Consequences

- Windows Installer owns rollback/uninstall for WindowsDevInspector product files and installer-created shortcut/registry metadata.
- Third-party tool installation remains an independent, approved online winget operation through ElevatedWorker.
- The first MSI is framework-dependent; runtime prerequisite versus self-contained deployment remains a later release decision.
- CI validates the generated unsigned MSI database but does not publish or upload it as a release artifact.
- Before external release, artifact storage must retain each exact signed/unsigned MSI by version and package identity for repair and support scenarios.

---

## DEC-018: Hide Installation Actions Until Exact Winget Package Availability Passes

### Status

Accepted

### Date

2026-08-12

### Decision

Each approved package maps to a catalog-owned availability check. App exposes an installation candidate only when both the tool diagnostic is non-PASS and the matching current-scan availability check is PASS. The check uses an exact package ID, explicit `winget` source, accepted source agreements, and disabled interactivity.

Missing winget, unresolved package ID, non-zero exit, or timeout means no installation action is shown. App does not fall back to arbitrary download URLs or alternate shell commands.

### Consequences

- Unsupported installations stay absent instead of failing after user selection.
- Availability remains machine/source dependent and is refreshed by each scan.
- Debug-only simulation rows remain exempt solely for UI smoke testing and cannot execute.

---

## DEC-019: Framework-dependent Deployment With a Required .NET 10 Desktop Runtime

### Status

Accepted

### Date

2026-08-12

### Decision

Production packaging remains framework-dependent. The MSI does not bundle or install a .NET Runtime. WiX `DotNetCompatibilityCheck` requires the x64 .NET 10 Desktop Runtime before a first install and displays a clear prerequisite message when compatibility fails.

The same runtime diagnostic is a Common check in every App scan. It requires `Microsoft.WindowsDesktop.App` 10.x. If the runtime result is missing or non-PASS, all remediation and package-installation selections are cleared and disabled, and the App displays the prerequisite reason in red. This in-App gate protects version-mismatch and diagnostic-failure cases; a machine with no compatible runtime cannot launch the framework-dependent App, so the MSI gate remains authoritative for first installation.

Installer payload authoring includes only root framework-dependent application output. MSI validation limits the expected payload size and rejects known runtime host files to prevent a stale self-contained publish directory from being bundled accidentally.

### Consequences

- The MSI remains small and contains no runtime installer or self-contained runtime payload.
- Users must install the official .NET 10 Desktop Runtime x64 independently before first installation.
- Runtime-dependent operations fail closed when the current scan cannot prove the prerequisite is ready.

---

## DEC-020: Retain Versioned MSI Files as GitHub Release Assets

### Status

Accepted

### Date

2026-08-13

### Decision

GitHub Releases is the canonical immutable storage for distributed MSI files. A pushed `vMAJOR.MINOR.PATCH` tag may create a prerelease only when the tagged commit is reachable from `main` or `mvp`. The workflow derives `WdiProductVersion` from the tag, restores, builds, tests, validates the MSI database, renames the asset to `WindowsDevInspector-<version>-win-x64.msi`, and publishes a matching `.sha256` file.

The workflow refuses an existing release and never uses `gh release upload --clobber`. Repository-level immutable releases are enabled so published release tags and assets cannot be modified or deleted. MSI/CAB files remain ignored and are never committed to Git.

Unsigned packages are explicitly labeled prerelease and not latest. Signing remains a separate external-release gate.

### Consequences

- Windows Installer repair can retrieve the exact original package associated with a released version.
- A release tag cannot originate directly from an unmerged feature branch.
- Publishing requires a deliberate version tag after branch integration and GitHub `contents: write` permission.
- GitHub authentication and the enabled immutable-release repository setting must remain operational before the first tag is pushed.

---

## DEC-021: Centralize WPF Action Availability and Type the Worker CLI Router

### Status

Accepted

### Date

2026-08-13

### Decision

MainWindowActionState is the single App-layer policy for Runtime readiness, busy state, installation-selection mode, executable installation selection, and backup availability. WPF controls refresh from the evaluated policy instead of workflow finally blocks directly enabling buttons. This prevents a failed Runtime prerequisite from being overwritten after scan, remediation, rollback, or installation workflows finish.

ElevatedWorker command-line parsing and dispatch use typed WorkerCommand, WorkerCommandParser, and WorkerCommandRouter models. Existing remediation, --rollback, and --install argument forms, usage text, result paths, JSON contracts, and exit-code semantics remain unchanged.

### Consequences

- Action eligibility is testable without starting WPF.
- Runtime prerequisite handling remains fail closed after every workflow transition.
- Program.cs retains composition and execution handlers but no longer owns ad hoc mode parsing and routing.
- A full MainWindow ViewModel rewrite remains optional follow-up work rather than a release prerequisite.
