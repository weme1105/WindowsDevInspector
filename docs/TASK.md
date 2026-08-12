# Tasks

## Current Objective

Complete release-readiness refactoring and publish the first unsigned prerelease.

## In Progress

Runtime-safe WPF action availability and typed ElevatedWorker CLI parsing/routing are implemented and validated on `refactor/release-readiness`. Existing Worker CLI and JSON contracts remain compatible. Integration into `mvp` and the deliberate `v0.1.0` prerelease tag remain in progress.

## Ready

- [ ] Complete the Debug simulation-row WPF smoke checklist when computer-use or manual verification is available.
  - Confirm red installation text/button, single-selection disabling, two-stage confirmation, and non-executable Debug simulation behavior.

## Blocked

- [ ] Installer, code signing, and auto update.
  - Blocker: code-signing certificate and auto-update strategy are intentionally deferred.
  - Required decision: signing and update strategy before external release.

## Remaining TODO

- [ ] Capture and inspect non-sensitive demo screenshots before adding image assets to the Repository.

## Known Issues

- [ ] `docs/CHECK_CATALOG.md` contains planning rows that are not yet in the Core catalog; they must remain documentation-only until selected for implementation.
- [ ] Automated validation does not currently include a WPF UI smoke test.

## Latest Verification

- Build: `dotnet build WindowsDevInspector.sln --configuration Release --no-restore` passed with 0 warnings and 0 errors.
- Debug build and tests passed with 0 warnings/errors and 228 tests before the executor slice.
- Focused Release tests passed: Remediation 49/49, ElevatedWorker 3/3, and App 49/49 after the controlled execution slice.
- Final Debug and Release solution builds passed with 0 warnings/errors; 257 tests passed in each configuration after PATH refresh, verification, shim, timeout, and compatibility coverage was added.
- Release solution restore/build passed with the WiX installer project, 0 warnings/errors, and 260 tests after winget availability gating.
- `scripts/Test-InstallerPackage.ps1` passed against MSI version 0.1.0 with 13 payload files and no prohibited bundled software installers.
- Initial non-elevated per-machine install failed with MSI error 1925/exit 1603 and left no Program Files or shortcut residue, confirming transactional rollback for that failure path.
- Elevated x64 MSI installation passed with exit 0. Product 0.1.0 registered under HKLM, installed 13 files under `C:\Program Files\WindowsDevInspector`, created the Start Menu shortcut, and launched a responsive main window that closed without a leftover process.
- Same-version repair by ProductCode exposed expected Windows Installer source retention behavior: after the original MSI was overwritten by a rebuild with a different PackageCode, repair failed with error 1706 while the installed product remained intact. Release MSI files must be immutable artifacts.
- Major Upgrade from 0.1.0 to 0.1.1 passed with exit 0, removed the old ProductCode, retained one 0.1.1 registration, and preserved all 13 product files and the shortcut.
- Downgrade from 0.1.1 to 0.1.0 was rejected by `WIX_DOWNGRADE_DETECTED`/LaunchConditions with no state change.
- Uninstall of 0.1.1 passed with exit 0 and removed product registration, Program Files payload, shortcut folder, and installer HKCU key. All six pre-existing LocalAppData report/remediation files remained present.
- Binary metadata check confirmed the simulation marker is present in Debug App output and absent from Release App output.
- Installation planning tests include validator, candidate mapping, preview/coordinator, and independent single-selection behavior.
- WPF visual smoke test was attempted again but not completed because computer-use initialization still fails with `EPERM: operation not permitted` before any window control begins.
- Framework-dependent App tests passed 56/56 and Windows checks passed 120/120 after Runtime prerequisite gating. WiX MSI build passed with 0 warnings/errors; MSI validation confirmed x64 version 0.1.0, the .NET 10 Desktop Runtime launch condition, 12 payload files, and no bundled runtime host files.
- Release and Debug solution builds passed with 0 warnings/errors after action-state and Worker CLI routing refactoring; all 285 tests passed in each configuration.
- Release MSI validation passed for x64 version 0.1.0 with 12 payload files and no bundled software/runtime installers.
- Debug App process smoke passed: the App created a main window, remained responsive, and closed without a leftover process. Full visual interaction remained unavailable because the computer-use helper failed initialization twice with `EPERM` before controlling any window.

## Next Recommended Task

Run complete Release/Debug/MSI/UI-smoke validation, merge the release-readiness slice into mvp, then publish the immutable v0.1.0 unsigned prerelease.
