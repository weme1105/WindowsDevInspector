# WindowsDevInspector Roadmap

## Phase 0：Repository 基礎

- [x] 建立 Solution
- [x] 建立 `.editorconfig`
- [x] 建立 `global.json`
- [x] 建立測試專案
- [x] 建立 GitHub Actions
- [x] 確認 `dotnet build`
- [x] 確認 `dotnet test`

## Phase 1：唯讀掃描 MVP

- [x] 技術選擇
- [x] 選定第一版 MVP 技術範圍
- [x] 收斂 UI 技術清單到 MVP 範圍
- [x] Check catalog (`docs/CHECK_CATALOG.md`)
- [x] Directory checks
- [x] Long Path check
- [x] Developer Mode check
- [x] PATH invalid entry check
- [x] Tool command checks
- [x] 結果排序
- [x] 評分
- [x] 點擊項目顯示詳細內容

## Phase 2：第一批安全修正

- [x] Change Plan
- [x] Remediation whitelist
- [x] Elevated Worker validation shell
- [x] 建立資料夾
- [x] Long Path remediation
- [x] Developer Mode remediation
- [x] 執行後驗證
- [x] JSON result

## Phase 3：Rollback

- [x] Backup schema
- [x] Backup browser
- [x] Rollback plan
- [x] Elevated rollback
- [x] Rollback verification

## Phase 4：工具安裝

- [x] Read-only winget package availability checks
- [ ] winget abstraction for future installation plans
- [ ] PowerShell 7 package planning
- [x] pnpm package planning
- [x] Azure CLI package planning
- [x] kubectl package planning
- [x] Terraform package planning
- [ ] 安裝後 PATH refresh
- [ ] 安裝後 CLI 可用性驗證
- [ ] 舊版工具 Info 提示，不強制升級

## Phase 5：進階 Windows 診斷

- [x] WSL
- [x] Virtual Machine Platform
- [x] Hyper-V
- [x] Docker Desktop
- [x] WinNAT
- [x] HNS
- [x] Firewall
- [x] localhost bind
- [x] Code Integrity events
- [x] Smart App Control diagnostics

## Phase 6：產品化

- [x] Report export
- [ ] MSIX / installer
- [ ] Code signing
- [ ] Auto update
- [ ] Privacy statement
- [ ] Release notes
- [ ] Demo screenshots
- [ ] Portfolio documentation
