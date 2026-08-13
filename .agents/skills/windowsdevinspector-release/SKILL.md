---
name: windowsdevinspector-release
description: Prepare, publish, recover, verify, and hand off WindowsDevInspector GitHub MSI releases using the repository's .NET 10, WiX x64, Runtime prerequisite, immutable prerelease, branch, security, checksum, and documentation rules. Use for WindowsDevInspector version tags, MSI releases, release workflow failures, release verification, or post-release handoff.
---

# WindowsDevInspector Release

Use the global `publish-github-release` Skill when available, then apply every project rule below. If unavailable, follow the same conservative sequence: inspect, validate, integrate, tag, publish through the canonical workflow, independently verify, and hand off.

## Inspect

- Read `AGENTS.md`, `docs/PROJECT_RULES.md`, `docs/SECURITY_RULES.md`, `docs/UI_RULES.md`, `docs/TASK.md`, `docs/DECISION.md`, `docs/AI_CONTEXT.md`, `docs/RELEASE_NOTES.md`, and `.github/workflows/release.yml` as applicable.
- Preserve untracked `codex-insights/`, `docs/images/`, and unrelated user work.
- Require `vMAJOR.MINOR.PATCH`; the tagged commit must be reachable from `main` or `mvp`.
- Confirm GitHub Immutable Releases remains enabled before first publication of a version.

## Validate the candidate

Use checkout-local `DOTNET_CLI_HOME` and repository `NuGet.Config`. Restore before `--no-restore`; build before `--no-build`.

The release workflow must restore `WindowsDevInspector.sln`, build Release with `-p:WdiProductVersion=<version-without-v>`, run the full solution tests, and run `scripts/Test-InstallerPackage.ps1` against the generated MSI with the same version. Run Debug and Release solution builds/tests locally when code changed.

The MSI must be x64 and framework-dependent. It may contain only WindowsDevInspector App, ElevatedWorker, application dependencies, and MSI metadata/shortcuts. It must not bundle .NET Runtime, winget packages, third-party installers, or signing private keys. Do not assume a historical test count; report the actual count observed.

## Integrate and tag

- Use the repository branch/PR policy; do not release from an unmerged feature branch.
- Wait for GitHub `Build and test` checks before merge.
- Confirm the exact tag and Release do not exist before pushing a new tag.
- Tag the merged `mvp` or `main` commit. Never move or recreate a pushed release tag during recovery.

## Publish and recover

- Publish only through `.github/workflows/release.yml`.
- Unsigned packages must be `prerelease=true`, `latest=false`, and clearly titled unsigned.
- Expected assets are `WindowsDevInspector-<version>-win-x64.msi` and its `.sha256` file.
- Refuse existing Releases and never overwrite assets.
- `workflow_dispatch` must exist on the default branch. A retry must take an explicit existing tag, checkout that tag, resolve its commit, repeat `main`/`mvp` reachability and unused-release checks, and build from tagged source.
- Treat API 404 as the expected unused-release condition only after checking the actual result. Explicitly exit successfully after an expected nonzero lookup.
- If a run fails after Release creation, stop and inspect immutable state; do not retry blindly.

## Verify publication

- Confirm `isDraft=false`, `isPrerelease=true`, and `isImmutable=true`.
- Confirm both assets are uploaded exactly once.
- Download both to a unique temporary directory and independently compare the MSI SHA-256 with the checksum file. The global Skill script may perform this check.
- Report release/workflow URLs, tag commit, MSI size, SHA-256, signing state, tests, and skipped UI validation.

## Hand off

Use `prepare-project-handoff`. Update only materially affected `docs/TASK.md`, `docs/DECISION.md`, `docs/AI_CONTEXT.md`, and `docs/RELEASE_NOTES.md`. Record present state, durable decisions, actual verification, limitations, blockers, and one concrete next task. Merge through a PR into `mvp`, wait for CI, return to an up-to-date `mvp`, and report unrelated files left untouched.

Code signing and auto-update remain separate approval gates for a stable external release. A missing WPF visual smoke result must be reported; it does not become verified through unit tests or process-launch smoke alone.
