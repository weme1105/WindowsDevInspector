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
