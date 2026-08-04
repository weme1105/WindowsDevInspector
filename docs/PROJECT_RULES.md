# Project Rules

本文件記錄 WindowsDevInspector 的架構、開發、Git、驗證與 Agent 工作規則。安全與 UI 的詳細條件因規則數量較多，分別記錄於 [`SECURITY_RULES.md`](SECURITY_RULES.md) 與 [`UI_RULES.md`](UI_RULES.md)。

## Architecture

1. App 只負責 UI、ViewModel、導航、狀態呈現與使用者操作。
2. App 不得直接寫 Registry、修改 PATH、執行系統管理員命令、安裝軟體或修改 Windows Feature。
3. Core 只放 domain models、interfaces、profile model、CheckResult、RemediationPlan、RiskLevel、scoring 與共用驗證規則，且不得依賴 WPF 或 Windows 特定 API。
4. Windows 只負責唯讀檢查；所有 `IEnvironmentCheck` 必須支援 `CancellationToken`，不得在建構函式執行外部命令，並處理 command 不存在、逾時與權限不足。
5. 檢查結果必須提供一般開發者可理解的 `CurrentValue`、`ExpectedValue` 與 `Impact`。
6. Remediation 負責修正定義、預覽、備份、執行後驗證、Rollback 與白名單驗證。
7. ElevatedWorker 是唯一允許以系統管理員權限執行修改的程式，並必須遵循 `SECURITY_RULES.md`。
8. 新增、移除、重新命名 project 或改變分層責任時，必須在同一個變更中更新 `docs/PROJECT.md` 與本文件。

## Development

9. C# 使用 file-scoped namespace 並啟用 nullable reference types。
10. 適合的 domain model 優先使用 immutable record 或 init-only property。
11. 非同步方法使用 `Async` 後綴。
12. 所有外部 Process 都必須設定 timeout，並在適用時支援取消。
13. 錯誤不得被靜默忽略，使用者可見訊息必須讓一般開發者理解。
14. 優先完成小型、可測試的垂直切片，不建立大量 placeholder 或推測性抽象。
15. 分支使用 `feature/<name>`、`fix/<name>`、`refactor/<name>` 或 `docs/<name>`，並遵守較高優先級的 workspace branch 規則。
16. Commit 使用 Conventional Commits；不得直接修改 `main`，功能變更透過 Branch 與 Pull Request。

## Verification and completion

17. 完成前執行 `dotnet build` 並確認沒有新增 compiler warning。
18. 完成前執行 `dotnet test`；需要既有 build output 時使用 `--no-build`。
19. 只回報實際執行並觀察到的驗證結果，不宣稱未驗證的功能已可用。
20. 若必要檢查無法執行，說明原因、剩餘風險與後續應執行的命令。
21. 完成回報包含修改檔案、Build result、Test result、安全影響與 Remaining work。

## Agent workflow

22. 非簡單修改前閱讀 `README.md`、`docs/PROJECT_SPEC.md`、`docs/AI_CONTEXT.md`、`docs/TASK.md` 及適用規則文件。
23. 修改前檢查目前 branch、Git 狀態與既有架構，不重建整個 Solution。
24. 提出簡短修改計畫，並只修改任務直接需要的檔案。
25. 一般授權、秘密保護、使用者既有修改與驗證誠信遵循全域 `AGENTS.md`，本文件不重複維護。

## Handoff

26. Handoff 時必須更新 `docs/TASK.md` 的 Current Objective、In Progress、Blocked、Remaining TODO、Known Issues 與 Next Recommended Task，使其只反映目前狀態。
27. `TASK.md` 的 completed item 若形成持久的架構、安全、技術、產品、相容性、部署或工作流程決策，必須先移至 `docs/DECISION.md`，再從 `TASK.md` 移除。
28. 一般實作完成紀錄不得偽裝成決策；確認 Git、PR、測試或 Release Notes 已有證據後，從 `TASK.md` 移除。若使用者要求保留完整完成歷史，放在 `DECISION.md` 的獨立 Completed Work Archive，不混入 decision index。
29. Handoff 只在穩定技術事實改變時更新 `docs/AI_CONTEXT.md`，並以連結取代重複的規格或規則內容。
30. Handoff 必須記錄最後實際驗證、阻塞與一個可執行的下一步；不得留下含糊的「繼續開發」。
