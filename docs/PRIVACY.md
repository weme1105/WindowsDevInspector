# Privacy Statement

WindowsDevInspector is designed as a local Windows developer workstation inspection tool.

## Data Processing Model

- The app runs locally on the user's Windows machine.
- The MVP does not upload scan results, configuration, backups, logs, or reports to a remote service.
- The MVP does not require a cloud account or external telemetry endpoint.
- Exported scan reports are written only when the user explicitly chooses to export them.

## Information Inspected

The app may inspect local development environment state, including:

- Windows version and processor architecture.
- PATH entries.
- Selected Windows registry values used by approved read-only checks.
- Selected Windows service status values.
- Selected Windows optional feature status values.
- Local directory existence for approved developer folders.
- Installed command-line tool availability and command output.
- NuGet source names and URLs after sensitive values are redacted.
- SDK or tool paths such as Android SDK, Windows SDK, Visual Studio, and Docker or WSL tooling.

## Information Not Inspected

The MVP is not intended to inspect:

- Browser profiles, cookies, saved passwords, browsing history, or extension data.
- Source code contents.
- SSH private keys, certificates, API keys, access tokens, or refresh tokens.
- Email, chat, documents, or personal files unrelated to the selected checks.
- Cloud account contents or remote project data.

## Sensitive Output Handling

Command output is treated as untrusted. Checks that may expose sensitive values must redact them before returning `CheckResult` values.

Current MVP examples:

- NuGet source diagnostics redact credential-bearing URLs and token/password/API-key style values.
- Browser availability diagnostics check known executable paths only and do not launch browsers.
- Android SDK diagnostics inspect environment variables and common SDK paths without modifying them.

## Reports

JSON scan reports may include local machine details such as:

- Selected technology IDs.
- Check IDs and names.
- Current values, expected values, impacts, severity, and remediation metadata.
- Local paths or tool output summaries.

Users should review reports before sharing them outside their machine or organization.

## Remediation And Backups

Approved remediation is explicit and constrained.

- Normal app code must not directly modify registry values, PATH, Windows features, or install software.
- Privileged registry remediation and rollback go through `WindowsDevInspector.ElevatedWorker`.
- Backup files for approved remediation are protected locally using DPAPI where applicable.

## MVP Limitations

- The MVP does not implement package installation automation.
- The MVP does not include a production telemetry, update, or crash-reporting pipeline.
- Installer, code signing, and auto update are not yet implemented.
