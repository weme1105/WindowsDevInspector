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

- [ ] Move worker launch orchestration out of `MainWindow.xaml.cs`.

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

- [ ] Add consistency tests for documented check IDs versus executable check IDs.

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

- [ ] Add a WPF smoke-test checklist or UI automation strategy.
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
