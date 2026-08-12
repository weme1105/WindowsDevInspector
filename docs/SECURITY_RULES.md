# Security Rules

本文件記錄 WindowsDevInspector 的安全限制與 remediation 邊界。任何新增或修改修正能力，都必須同步檢查本文件、`docs/PROJECT_SPEC.md` 與相關測試。

## ElevatedWorker Requirements

Worker 必須：

1. 驗證自身確實已提升權限。
2. 驗證 Change Plan schema。
3. 僅執行明確白名單內的 RemediationId。
4. 拒絕任意 shell command。
5. 修改前建立備份。
6. 每項修正獨立記錄結果。
7. 修改後重新讀取系統狀態驗證。
8. 產生 JSON execution result。
9. 不接受由 UI 傳入的任意 Registry path 或任意 executable。

## Prohibited Operations

禁止：

- 關閉 Defender、Smart App Control 或 UAC。
- 任意修改 ACL。
- 接受任意 PowerShell 字串。
- 執行遠端下載後的腳本。
- 使用 `cmd /c` 執行由 UI 組合的命令。
- 在 Log 中寫入 Token、Password、Cookie、Private Key 或 connection string。
- 自動刪除不明 PATH 項目。
- 在沒有備份的情況下修改 Registry。

## Initial Automatic Remediation Scope

第一階段允許自動修正的項目只有：

- 建立 `D:\Source`。
- 建立 `D:\Projects`。
- 建立 `D:\Note`。
- 啟用 Long Paths。
- 啟用 Developer Mode。

所有 remediation 必須透過明確白名單內的 remediation ID 執行，不得接受任意命令、Registry path 或 executable path。

## Package Installation Planning

Package installation is enabled only through the controlled ElevatedWorker route. Installation plans must:

- Reference only an exact package ID and source present in `BuiltInInstallationCatalog`.
- Use a supported typed action; the current schema permits only `Install`.
- Derive risk, elevation, restart, PATH refresh, verification executable, and verification arguments from the approved catalog rather than UI input.
- Contain exactly one item and reject unknown JSON properties, package IDs, sources, actions, invalid plan IDs, and additional items.
- Never accept an executable, argument list, shell command, package source URL, or arbitrary version from the UI.
- Be shown in App only when the matching catalog-owned `winget show --id <id> --exact --source winget --disable-interactivity` availability check returned PASS during the current scan. Missing winget, unresolved IDs, failures, and timeouts must hide the installation action.

App selection only creates a preview. Execution requires a separate explicit confirmation, UAC, and ElevatedWorker `--install` invocation. The worker reconstructs and validates the fixed command contract before starting winget:

`winget install --id <approved-package-id> --exact --source winget --accept-package-agreements --accept-source-agreements`

The worker uses a five-minute timeout and kills the process tree on timeout or cancellation. Silent mode, override, forced scope/version, arbitrary arguments, automatic uninstall, and automatic rollback are prohibited. Debug simulation rows can never invoke installation.

After a successful winget exit, the worker may refresh only its own process `PATH` from current machine and user environment values. It must not persist or rewrite user/machine PATH. CLI verification must use only the executable and argument tokens stored in `BuiltInInstallationCatalog`, use a 30-second timeout, and must not return raw stdout/stderr in the App-facing result. Native executables run directly. Catalog-owned `.cmd`/`.bat` shims may use only the system `cmd.exe` fixed wrapper; verification arguments containing shell metacharacters must be rejected.

## MSI Packaging Boundary

The WiX MSI may contain only WindowsDevInspector App, ElevatedWorker, their application dependencies, and MSI metadata/shortcuts. It must not bundle pnpm, Azure CLI, kubectl, Terraform, winget, a .NET Runtime installer, certificate private keys, or any other third-party software installer. Generated `.msi`/`.cab` files are build artifacts and must remain ignored by Git. The initial MSI is unsigned and intended only for local/internal validation.

The MSI must remain framework-dependent and require the x64 .NET 10 Desktop Runtime through a compatibility launch condition. It must not recursively package runtime-specific publish directories. The App's Common runtime check must fail closed by clearing and disabling all remediation and package-installation actions whenever the prerequisite result is absent or non-PASS.
