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
- Create `D:\GoNote`.
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

