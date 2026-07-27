# Codex 開發環境設定

## 1. 建議的全域資料夾

```text
D:\Source
├─ WindowsDevInspector
├─ TasteMate
└─ 其他 Git repositories

D:\Projects
├─ sandbox
├─ prototypes
└─ outputs

D:\GoNote
├─ notes
├─ scripts
└─ diagnostics
```

定義：

- `D:\Source`：正式 Git Repository。
- `D:\Projects`：暫存專案、實驗與輸出。
- `D:\GoNote`：學習筆記、診斷腳本與個人工具。

WindowsDevInspector 應放在：

```text
D:\Source\WindowsDevInspector
```

## 2. 本機必要工具

建議安裝：

```text
Git
.NET 10 SDK
PowerShell 7
Node.js LTS
Codex CLI
Visual Studio 2022/2026 或 Rider
VS Code
```

Codex CLI 可透過 npm 安裝：

```powershell
npm install -g @openai/codex
```

在 Windows 上，若原生執行遇到相容性問題，可改在 WSL 中使用。OpenAI 文件目前仍建議 Windows 使用者在需要時透過 WSL 執行 CLI。

## 3. Clone Repository

```powershell
cd D:\Source
git clone https://github.com/weme1105/WindowsDevInspector.git
cd WindowsDevInspector
```

## 4. 首次驗證

```powershell
dotnet --info
dotnet restore
dotnet build
dotnet test
```

若 Repository 尚未有完整 Solution，先讓 Codex依 `AGENTS.md` 與 `docs/PROJECT_SPEC.md` 建立骨架。

## 5. Codex 啟動位置

必須在 Repository root 啟動：

```powershell
cd D:\Source\WindowsDevInspector
codex
```

不要在：

```text
D:\
D:\Source
C:\Users\Administrator
```

啟動後再要求修改特定 Repository，否則 Agent 可讀取的上下文範圍會過大或不正確。

## 6. Repository 級規則

本專案使用根目錄的：

```text
AGENTS.md
```

Codex 開始工作前應閱讀此文件。

Repository 級 `AGENTS.md` 適合放：

- 架構規則
- 技術選型
- 安全限制
- Build / Test 指令
- Git 規則
- 完成定義

## 7. 建議的全域 Codex 指令

你之前提到的「全域環境」，主要目的不是把所有專案規則塞到同一份檔案，而是建立一套共用開發慣例。

建議分成兩層：

### 全域層

放所有專案都適用的規則，例如：

- 修改前先讀 Repository 文件。
- 不直接修改 main。
- 每次修改前提出簡短計畫。
- 修改後執行 build/test。
- 不假裝測試成功。
- 不寫入 Secret。
- 優先小型 Commit。
- 不刪除不理解的程式碼。
- 不使用破壞性 Git 指令。

### Repository 層

放在每個專案根目錄的 `AGENTS.md`：

- 專案架構
- Domain 規則
- 技術限制
- 專案指令
- 安全白名單
- 特定測試要求

全域規則應保持通用；WindowsDevInspector 的 Registry、UAC 與 Worker 規則只應放在本 Repository。

## 8. 建議建立的全域工作目錄

```text
D:\Source\_templates
├─ dotnet
├─ go
├─ angular
└─ agents

D:\Source\_scripts
├─ new-project.ps1
├─ verify-dotnet.ps1
├─ verify-go.ps1
└─ verify-node.ps1
```

用途：

- `_templates`：共用 Solution、`.editorconfig`、CI 與 AGENTS 範本。
- `_scripts`：建立專案與環境驗證腳本。

不要把真正專案放進 `_templates`。

## 9. 建議的 Git 全域設定

先確認使用者資訊：

```powershell
git config --global user.name "weme1105"
git config --global user.email "你的 GitHub Email"
```

Windows 建議：

```powershell
git config --global core.autocrlf true
git config --global init.defaultBranch main
git config --global fetch.prune true
git config --global pull.rebase false
```

不要設定：

```powershell
git config --global safe.directory "*"
```

這會過度放寬 Git 安全檢查。

## 10. 建議的 Repository 基礎檔案

```text
AGENTS.md
README.md
.editorconfig
.gitignore
Directory.Build.props
Directory.Packages.props
global.json
docs/
.github/workflows/build.yml
```

### global.json

固定 SDK major/minor，避免不同電腦使用不一致的 SDK：

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```

實際版本要先依 `dotnet --list-sdks` 調整，不要直接照抄。

## 11. Codex 第一個任務

可直接貼：

```text
Read AGENTS.md, README.md, docs/PROJECT_SPEC.md, and docs/ROADMAP.md.

Inspect the current repository before changing anything.

Create or normalize the .NET 10 WPF solution structure described in AGENTS.md.
Do not implement all planned checks.

Implement only the first vertical slice:
1. Common profile selection.
2. Read-only checks for D:\Source, D:\Projects, D:\GoNote.
3. Long Paths check.
4. Developer Mode check.
5. Result sorting with PASS items last.
6. Hide selection checkbox for PASS items.
7. Unit tests for sorting and profile filtering.

Before editing, provide a concise implementation plan.
After editing, run dotnet build and dotnet test.
Report changed files, build result, test result, and remaining work.
Do not modify main directly; use a feature branch.
```

## 12. 後續任務拆分

不要一次叫 Codex 完成整個產品。

建議順序：

```text
Task 1: Solution 與 Core models
Task 2: Scan orchestration
Task 3: WPF result UI
Task 4: Directory remediation
Task 5: Registry remediation
Task 6: Elevated Worker
Task 7: Backup
Task 8: Rollback
Task 9: PATH diagnostics
Task 10: winget package installation
Task 11: Profile plugin
Task 12: GitHub Actions
```

## 13. GitHub 與 Codex

ChatGPT 的一般 GitHub App 主要用於讀取、分析與搜尋 Repository；要讓 Agent 直接修改並建立 PR，應使用 Codex 的 GitHub Repository 環境。Codex Cloud 會在隔離環境中執行任務，產生可供審查的變更與 Pull Request。

Private Repository 必須在 Codex / GitHub 連線設定中明確授權該 Repository。

## 14. 建議工作方式

每次任務：

```text
建立 feature branch
→ Codex 修改
→ build
→ test
→ 查看 diff
→ 建立 PR
→ 人工 Review
→ merge
```

不要使用「直接幫我完成整個專案」這種任務描述。

最佳 Prompt 格式：

```text
Context
Goal
Scope
Out of scope
Acceptance criteria
Build command
Test command
Security constraints
Expected report
```
