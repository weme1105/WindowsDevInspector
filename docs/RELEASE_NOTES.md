# Release Notes

## MVP

Status: validated locally.

### Highlights

- WPF desktop app for inspecting Windows developer workstation readiness.
- Searchable explicit technology selection instead of broad role presets.
- Common baseline checks for Windows version, processor architecture, PATH health, Long Paths, Developer Mode, standard folders, PowerShell 7, Git, winget, and Chocolatey.
- Executable checks for the current Core catalog, including .NET, Node.js, npm, Go, Python, Java, PHP, Ruby, Rust, database CLIs, DevOps CLIs, QA tooling, Android SDK, .NET MAUI workload, Visual Studio, Build Tools, Windows SDK, WSL, and Docker.
- Result sorting with non-pass results before pass results.
- Environment score calculation.
- Result detail panel with current value, expected value, impact, fixability, risk, elevation, restart, rollback, and remediation context.
- JSON scan report export.
- First safe remediation flow for approved low-risk items.
- DPAPI-protected backup and rollback support for approved remediation.

### Safety Model

- Environment checks are read-only.
- The app does not install tools in the MVP.
- The app does not require installed tools to be the latest version.
- Normal WPF app code does not directly write registry values, modify PATH, enable Windows features, install software, or run arbitrary administrator commands.
- Privileged changes are constrained to approved remediation IDs and executed through the elevated worker boundary.
- NuGet source diagnostics redact credential-bearing URLs and token/password/API-key style values before display.

### Validated

- Release build passed with 0 warnings and 0 errors.
- Release tests passed with 174 tests.
- WPF app launch smoke passed.
- Manual WPF UI smoke checklist completed by the user.

### Known Limitations

- Package installation remains design-only. Approved installation plan schema is still future work.
- Installer, code signing, and auto update are not implemented.
- Automated validation does not yet include a Windows UI automation harness.
- `docs/CHECK_CATALOG.md` contains planning rows that are not yet in the Core catalog.

### Recommended Next Work

- Design approved installation plan schema before enabling package installation.
- Add privacy/release artifacts to future packaged builds.
- Decide installer, code signing, and auto-update strategy.
