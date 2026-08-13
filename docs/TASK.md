# Tasks

## Current Objective

No active implementation task. The remediation-button availability fix is committed locally on `mvp`, and the unsigned immutable `v0.1.0` prerelease remains published.

## In Progress

None.

## Ready

- [ ] Complete the Debug simulation-row WPF smoke checklist when computer-use or manual verification is available.
  - Confirm red installation text/button, single-selection disabling, two-stage confirmation, and non-executable Debug simulation behavior.

## Blocked

- [ ] Code signing and auto update for a stable external release.
  - Blocker: code-signing certificate and auto-update strategy are intentionally deferred.
  - Required decision: signing and update strategy before external release.

## Remaining TODO

- [ ] Capture and inspect non-sensitive demo screenshots before adding image assets to the Repository.

## Known Issues

- [ ] `docs/CHECK_CATALOG.md` contains planning rows that are not yet in the Core catalog; they must remain documentation-only until selected for implementation.
- [ ] Automated validation does not currently include a WPF UI smoke test.

## Latest Verification

- Local Release solution build passed after commit `4df889d`; all 286 tests passed with 0 failures and 0 skipped.
- The focused remediation command-runner test also passed after `C:\Program Files\dotnet` was added to the user PATH. The active SDK is 10.0.303, compatible with the repository `global.json` roll-forward policy.
- Release MSI validation passed for x64 version 0.1.0 with 12 payload files, the .NET 10 Desktop Runtime launch condition, and no bundled software/runtime installer.
- Debug App process smoke passed: the App created a responsive main window and closed without a leftover process. Full automated visual interaction remains unverified because the computer-use helper failed initialization with `EPERM` before window control.
- PR #3, #4, #5, and #6 GitHub build/test checks passed before merge.
- Release workflow run `31627841551` passed restore, versioned build, tests, MSI validation, and immutable prerelease creation.
- GitHub Release `v0.1.0` is published as an unsigned immutable prerelease with MSI and `.sha256` assets. Downloaded MSI SHA-256 `d5ef07add67544a3991cfc7ca6e1ad070273eb643fca2e3acb498621359c6e70` matched the published checksum.
- Handoff re-verification on 2026-08-14 confirmed the Release is published, non-draft, prerelease, immutable, contains exactly one 872,448-byte MSI plus one checksum asset, and the independently downloaded MSI still matches that SHA-256. Temporary verification files were removed.
- Codex sandbox build attempts can still fail while reading the user-level NuGet configuration; use checkout-local `DOTNET_CLI_HOME`, repository `NuGet.Config`, and a normal user PowerShell for complete local validation.

## Next Recommended Task

Complete the Debug simulation-row WPF smoke checklist manually or repair the computer-use helper, then record the observed red labels, mutual exclusion, two-stage confirmation, non-executable simulation behavior, and remediation-button selection transitions.
