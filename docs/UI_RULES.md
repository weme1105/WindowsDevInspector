# UI Rules and Validation

本文件記錄 WindowsDevInspector 的 UI 行為、手動 smoke test 與展示畫面驗證。UI 行為變更時，必須同步檢查 `docs/PROJECT_SPEC.md`、相關自動測試與本文件。

## Behavior

1. 左側選擇技術細項；分類可用角色語意輔助瀏覽，但不可因選擇角色而自動帶入整包技術。
2. 點擊「開始檢查」後執行唯讀掃描。
3. 非 PASS 項目排在前面，PASS 項目排在最後。
4. PASS 項目不顯示 remediation 勾選框。
5. 點擊列表項目時，下方顯示 CurrentValue、ExpectedValue、Impact、fixability、risk、elevation、restart、Rollback 與 remediation 資訊。
6. 「批次修正」只選取低風險、已支援且非 PASS 的修正，不立即執行。
7. 「開始修正」只執行目前勾選的修正。
8. 修正前顯示 UAC、白名單、備份、風險、重開機與 Rollback 資訊。
9. 修正或 Rollback 後重新掃描。
10. Package installation candidates and remediation share one `操作` column. Each row shows one checkbox labeled `修正` or red `安裝` according to its action type.
11. Only one package installation candidate may be selected at a time. After confirmation, other package candidates and all remediation checkboxes are disabled until the installation selection is cleared.
12. Selecting an installation candidate must show its approved package ID, source, action, risk, UAC, restart, PATH refresh, post-install verification, and a warning that installation may modify system state.
13. Cancelling the warning clears only the pending installation selection and preserves existing remediation selections. Accepting it clears existing remediation selections and creates only a read-only preview; it must not start winget, request UAC, or modify the system.
14. Debug builds add exactly one simulated remediation row and one simulated installation row for UI verification. The simulated remediation row must be excluded from execution, and Release builds must not insert either row.
15. Every scan includes the .NET 10 Desktop Runtime x64 prerequisite. If it is missing, incompatible, or cannot be verified, every remediation and installation checkbox plus modifying action button is disabled, any pending action selection is cleared, and the reason is shown in red.
16. Modifying-control availability must be derived from MainWindowActionState. Scan, remediation, rollback, and installation finally blocks must refresh the policy and must not unconditionally enable controls.

## When to run the smoke test

修改 WPF UI、scan orchestration、result presentation、report export、remediation、backup browser 或 rollback entry point 時，先執行：

```powershell
dotnet build WindowsDevInspector.sln --no-restore
dotnet test WindowsDevInspector.sln --no-build
```

再啟動 App：

```powershell
dotnet run --project src\WindowsDevInspector.App\WindowsDevInspector.App.csproj
```

## Manual smoke checklist

- 主視窗正常開啟，technology grouping、search 與 selection controls 可操作。
- Group 展開或瀏覽不會自動選取整組 technology。
- Select All／Clear All toggle 的文字、選取狀態與可見項目一致。
- Toggle 不會在使用者按下 Save Options 前改寫已儲存設定。
- Search 不會清除不可見項目的既有選取。
- 多個 technology 對應的 shared checks 在掃描後保持 deduplicated。
- Start Check 執行唯讀掃描，完成後重新啟用 UI。
- 非 PASS 排在 PASS 前，PASS 不顯示 remediation checkbox。
- Result detail、environment score 與 remediation metadata 正確顯示。
- Batch selection 只勾選支援的低風險非 PASS 項目，且不立即執行。
- Remediation confirmation 在任何修改前顯示 UAC、白名單、備份、restart 與 Rollback context。
- 取消 confirmation 不改變系統，App 仍可繼續使用。
- Directory remediation 建立加密備份；Rollback 只移除工具建立且仍為空的目錄。
- Report export 建立 JSON 並顯示清楚的成功或失敗訊息。
- 沒有 backup 時，backup browser 不會 crash，Rollback control 不可用或說明原因。
- 選取 backup 啟動 Rollback 前，顯示 UAC、backup validation、approved target 與 rescan context。
- Non-pass CLI diagnostics with an approved package show a red `安裝` checkbox in the shared `操作` column; supported remediation rows show `修正` in the same column.
- Selecting one installation candidate shows the safety warning; cancelling leaves candidates enabled and preserves existing remediation selections.
- Accepting one candidate keeps it selected, clears existing remediation selections, disables the other installation candidates, and disables every remediation checkbox and remediation action button.
- Clearing the selected installation candidate re-enables the other candidates and remediation controls.
- Installation planning never starts winget, requests UAC, or changes package state.
- In a Debug build, `[DEBUG] 模擬修正` and `[DEBUG] 模擬套件安裝` appear once each before and after scans; verify the unified action column without approving or executing a real change.
- A non-PASS or missing Runtime prerequisite clears and disables both Debug and real action rows and displays the red Runtime requirement; a PASS result restores normal action eligibility.

## Safety during manual testing

- 除非任務明確要求在 disposable machine 驗證 elevated execution，否則不得核准 UAC。
- Normal App process 不得直接寫 Registry、修改 PATH、安裝軟體、切換 Windows Feature 或執行任意 administrator command。
- 專用 elevated test 必須確認 Worker 只接受 approved remediation ID，並產生 JSON execution result。

## Exit criteria

- Build 與 unit tests 通過。
- 本次變更涉及的 manual checklist 項目通過。
- 沒有 privileged operation 在缺少明確 confirmation 的情況下執行。
- 跳過的項目與理由記錄在 `docs/TASK.md` 或 Pull Request notes。

## Demo screenshots

需要展示畫面時，使用不含秘密、私人路徑、browser data、token、cookie 或客戶資訊的 workstation state。建議畫面包括：

- Main window 與 technology selection。
- Scan results 與 score。
- Result detail panel。
- 不核准 UAC 的 remediation confirmation。
- Report export success。
- Backup 與 rollback entry point。

使用 Release build 擷取，擷取後逐張檢查，核准的圖片存放於 `docs/images/`。目前實際 screenshot image 尚未加入 Repository。

## Automation direction

- 短期以 manual checklist 搭配 App-layer fakes 與 unit tests。
- 中期將 selection state、command enablement、result projection 與 remediation selection 移入可測試的 ViewModel。
- 長期評估 Windows-only UI automation，優先使用 fake scan/remediation services，且不得自動核准 UAC 或修改真實 Registry、PATH 或 package state。
