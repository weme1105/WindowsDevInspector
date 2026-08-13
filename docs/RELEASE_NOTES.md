# Release Notes

## MVP

Status: `v0.1.0` published as an unsigned immutable GitHub prerelease.

### Highlights

- WPF desktop app for inspecting Windows developer workstation readiness.
- Searchable explicit technology selection instead of broad role presets.
- Common baseline checks for Windows version, processor architecture, PATH health, Long Paths, Developer Mode, standard folders, PowerShell 7, Git, winget, and Chocolatey.
- Executable checks for the current Core catalog, including .NET, Node.js, npm, Go, Python, Java, PHP, Ruby, Rust, database CLIs, DevOps CLIs, QA tooling, Android SDK, .NET MAUI workload, Visual Studio, Build Tools, Windows SDK, WSL, and Docker.
- Read-only advanced Windows diagnostics for firewall profiles, localhost bind health, recent Code Integrity events, and Smart App Control state.
- Result sorting with non-pass results before pass results.
- Environment score calculation.
- Result detail panel with current value, expected value, impact, fixability, risk, elevation, restart, rollback, and remediation context.
- JSON scan report export.
- First safe remediation flow for approved low-risk items.
- DPAPI-protected backup and rollback support for approved remediation.
- A unified result-list action column labels remediation as `修正` and package candidates as red `安裝`; selecting one candidate clears and disables competing actions, then a separate red button requires a second safety confirmation before ElevatedWorker execution.
- A fail-closed package executor shell produces fixed command previews but requires an explicit enable option; the only built-in runner still returns Disabled and never starts winget.

### Safety Model

- Environment checks are read-only.
- Package installation is limited to explicitly approved catalog entries and always requires preview, separate confirmation, UAC, and ElevatedWorker validation.
- The app does not require installed tools to be the latest version.
- Normal WPF app code does not directly write registry values, modify PATH, enable Windows features, install software, or run arbitrary administrator commands.
- Privileged changes are constrained to approved remediation IDs and executed through the elevated worker boundary.
- NuGet source diagnostics redact credential-bearing URLs and token/password/API-key style values before display.

### Validated

- Release build passed with 0 warnings and 0 errors.
- The latest Release solution build passed after the remediation-button availability fix; all 286 tests passed with 0 failures and 0 skipped. The earlier Debug validation passed 285 tests before that focused regression test was added.
- Release workflow restore, versioned build, tests, MSI validation, and immutable prerelease creation passed.
- Published MSI SHA-256 was independently downloaded and matched the published checksum.
- WPF App process smoke passed. The final Debug simulation-row visual checklist remains pending because the computer-use helper could not initialize.

### Known Limitations

- Controlled package installation is limited to one approved catalog package through ElevatedWorker, fixed winget tokens, UAC, and a five-minute timeout. No silent/override/forced scope/version flags or automatic rollback are used.
- After a successful winget exit, ElevatedWorker refreshes its process-only PATH and runs the catalog-owned CLI verification command with a 30-second timeout; verification failure keeps the overall result unsuccessful and raw command output is not exposed to App.
- Installation actions now appear only after the matching exact winget package availability check passes; missing winget, unresolved IDs, failures, and timeouts hide unsupported actions.
- Added an unsigned WiX v5 MSI authoring project with Major Upgrade, downgrade blocking, Program Files payload, and Start Menu shortcut lifecycle. It packages no third-party tools or runtime installers, and generated MSI/CAB files are ignored.
- Added a read-only MSI database validation script that verifies App/Worker payload, upgrade metadata, shortcut target, and prohibited bundled-software names without installing the package.
- The x64 MSI was installed with explicit approval, registered product 0.1.0, installed 13 files under Program Files, created the Start Menu shortcut, and launched a responsive App window. A prior non-elevated attempt failed cleanly with no residue.
- MSI lifecycle validation passed for 0.1.0→0.1.1 Major Upgrade, downgrade rejection, and 0.1.1 uninstall. Uninstall removed MSI-owned state while preserving six existing LocalAppData report/remediation files.
- Windows Installer repair requires retention of the exact original MSI. Replacing an installed version's source package with a rebuild using a different PackageCode produced repair error 1706, so versioned release artifacts must be immutable.
- Selected framework-dependent deployment: MSI requires .NET 10 Desktop Runtime x64 without bundling it, and App scans disable all modifying actions in red when the Common Runtime prerequisite is not PASS.
- Hardened MSI payload validation against stale self-contained output by packaging only root application files, limiting payload count, and rejecting .NET runtime host binaries.
- Added a tag-driven GitHub prerelease workflow that derives MSI version from `vMAJOR.MINOR.PATCH`, runs full validation, publishes a versioned MSI plus SHA-256, and refuses overwrite of an existing release. A failed pre-publication run can safely retry the same unmoved tag through the default-branch workflow.
- Centralized WPF modifying-action availability so failed Runtime prerequisites cannot be overwritten by workflow cleanup, and added pure state-policy regression tests.
- Disabled `修正勾選項目` until at least one executable remediation is selected; clearing the selection disables it again.
- Replaced ad hoc ElevatedWorker CLI mode parsing with typed command parsing/routing while preserving all existing argument forms, JSON contracts, usage behavior, and exit codes.
- Code signing and auto update are not implemented. The current unsigned MSI is a prerelease for local/internal validation only.
- Automated validation does not yet include a Windows UI automation harness.
- `docs/CHECK_CATALOG.md` contains planning rows that are not yet in the Core catalog.

### Recommended Next Work

- Complete the Debug simulation-row WPF visual smoke checklist.
- Capture and inspect non-sensitive demo screenshots before committing image assets.
- Decide code-signing and auto-update strategy before an external stable release.
