# AGENTS.md

本文件是 Codex 與其他 AI Coding Agent 在此 Repository 中必須遵守的專案規則。

## 專案目標

建立一套可擴充、安全、可驗證的 Windows 開發環境檢查與修復桌面程式。

產品名稱：`WindowsDevInspector`

## 技術選型

- 語言：C#
- Runtime：.NET 10
- 桌面 UI：WPF
- 架構：MVVM
- DI：Microsoft.Extensions.DependencyInjection
- Logging：Microsoft.Extensions.Logging 或 Serilog
- JSON：System.Text.Json
- 測試：xUnit
- Windows 整合：Registry、Services、Optional Features、Process、Environment Variables
- 安裝工具：winget
- 原則上不引入商業套件

## Solution 結構

```text
WindowsDevInspector.sln
├─ src
│  ├─ WindowsDevInspector.App
│  ├─ WindowsDevInspector.Core
│  ├─ WindowsDevInspector.Windows
│  ├─ WindowsDevInspector.Remediation
│  └─ WindowsDevInspector.ElevatedWorker
├─ tests
│  ├─ WindowsDevInspector.Core.Tests
│  ├─ WindowsDevInspector.Windows.Tests
│  └─ WindowsDevInspector.Remediation.Tests
├─ profiles
├─ docs
└─ artifacts
```

## 分層責任

### WindowsDevInspector.App

只負責 UI、ViewModel、導航、狀態呈現與使用者操作。

不得直接：

- 寫 Registry
- 修改 PATH
- 執行系統管理員命令
- 安裝軟體
- 修改 Windows Feature

### WindowsDevInspector.Core

只放：

- Domain models
- Interfaces
- Profile model
- CheckResult
- RemediationPlan
- RiskLevel
- Scoring
- 共用驗證規則

不得依賴 WPF 或 Windows 特定 API。

### WindowsDevInspector.Windows

只負責 Windows 唯讀檢查。

所有 `IEnvironmentCheck`：

- 預設唯讀
- 不可修改系統
- 不可在建構函式執行外部命令
- 必須支援 CancellationToken
- 必須處理 command 不存在、逾時與權限不足
- 必須回傳可理解的 CurrentValue、ExpectedValue、Impact

### WindowsDevInspector.Remediation

負責：

- 修正定義
- 修正預覽
- 備份
- 執行後驗證
- Rollback
- 白名單驗證

### WindowsDevInspector.ElevatedWorker

唯一允許以系統管理員權限執行修改的程式。

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

## UI 行為規格

1. 左側選擇技術細項；分類可用角色語意輔助瀏覽，但不可因選擇角色而自動帶入整包技術。
2. 點擊「開始檢查」後執行唯讀掃描。
3. 非 PASS 項目排在前面。
4. PASS 項目排在最後。
5. PASS 項目不顯示勾選框。
6. 點擊列表項目時，下方顯示詳細修正內容。
7. 「批次修正」選取所有低風險且已支援的修正。
8. 右下角「開始修正」執行目前選取的修正。
9. 執行前顯示 UAC、備份、重開機與 Rollback 資訊。
10. 執行後重新掃描。

## 安全規則

禁止：

- 關閉 Defender
- 關閉 Smart App Control
- 關閉 UAC
- 任意修改 ACL
- 接受任意 PowerShell 字串
- 執行遠端下載後的腳本
- 使用 `cmd /c` 執行由 UI 組合的命令
- 在 Log 中寫入 Token、Password、Cookie、Private Key
- 自動刪除不明 PATH 項目
- 在沒有備份的情況下修改 Registry

允許自動修正的第一階段項目：

- 建立 `D:\Source`
- 建立 `D:\Projects`
- 建立 `D:\Note`
- 啟用 Long Paths
- 啟用 Developer Mode

## Coding Style

- 使用 file-scoped namespace。
- 啟用 nullable reference types。
- 優先使用 immutable records 或 init-only properties。
- 非同步方法加上 `Async`。
- 所有外部 Process 都要設定 timeout。
- 不使用 catch-all 後完全忽略錯誤。
- 錯誤訊息要能讓一般開發者理解。
- 不要過度抽象；先完成可測試的垂直切片。
- 不要一次建立大量 placeholder class。
- 每次變更保持小而可審查。

## Git 規則

分支命名：

```text
feature/<name>
fix/<name>
refactor/<name>
docs/<name>
```

Commit 使用 Conventional Commits：

```text
feat: add long path remediation
fix: hide checkbox for passed checks
refactor: separate scan orchestration
docs: add codex development guide
test: add registry check tests
```

不得直接修改 `main`。每個功能使用 Branch + Pull Request。

## 完成定義

任何任務完成前必須：

1. `dotnet build` 成功。
2. `dotnet test` 成功。
3. 不新增 compiler warning。
4. 說明修改檔案。
5. 說明安全影響。
6. 說明測試方式。
7. 若無法執行測試，明確說明原因。
8. 不宣稱未實際驗證的功能已可用。

## Codex 執行任務時

開始修改前：

1. 閱讀 `README.md`。
2. 閱讀 `docs/PROJECT_SPEC.md`。
3. 閱讀本文件。
4. 檢查現有架構，不要重建整個 Solution。
5. 提出簡短修改計畫。
6. 只修改與任務直接相關的檔案。

任務完成後回報：

```text
Summary
Files changed
Build result
Test result
Security considerations
Remaining work
```
