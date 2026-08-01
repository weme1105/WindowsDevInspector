# Demo Screenshots

This file defines the MVP screenshot set for README, portfolio, release notes, and PR review.

Use screenshots that do not expose secrets, private source paths beyond the repository path, browser data, tokens, cookies, or customer information.

## Required MVP Screenshots

| ID | Screenshot | Purpose | Status |
|---|---|---|---|
| demo-01 | Main window and technology selection | Shows searchable grouped technology selection and explicit user choice. | Needs capture |
| demo-02 | Scan results with score | Shows non-pass-first result ordering and environment score. | Needs capture |
| demo-03 | Result detail panel | Shows current value, expected value, impact, fixability, risk, elevation, restart, rollback, and remediation metadata. | Needs capture |
| demo-04 | Remediation confirmation | Shows UAC, backup, restart, rollback, and whitelist context before any fix runs. | Needs capture without approving UAC |
| demo-05 | Report export success | Shows scan report export flow and success feedback. | Needs capture |
| demo-06 | Backup and rollback entry point | Shows backup browser and rollback controls without executing elevated rollback. | Needs capture |

## Capture Guidance

1. Use a disposable or non-sensitive workstation state when possible.
2. Run a Release build before capturing.
3. Start the app with:

```powershell
dotnet run --project src\WindowsDevInspector.App\WindowsDevInspector.App.csproj --configuration Release --no-build
```

4. Do not approve UAC during screenshot capture.
5. Inspect every image before adding it to the repository.
6. Store approved screenshots under `docs/images/` using the screenshot IDs above.

## Current MVP Validation

- Release build passed.
- Release tests passed.
- WPF launch smoke passed.
- Manual WPF UI smoke checklist was completed by the user.

Actual screenshot image files have not been added yet because they should be reviewed for local paths and machine-specific data before committing.
