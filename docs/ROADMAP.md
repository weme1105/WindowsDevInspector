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

- [ ] Profile 選擇
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

- [ ] winget abstraction
- [ ] PowerShell 7
- [ ] pnpm
- [ ] Azure CLI
- [ ] kubectl
- [ ] Terraform
- [ ] 安裝後 PATH refresh
- [ ] 安裝後版本驗證

## Phase 5：進階 Windows 診斷

- [ ] WSL
- [ ] Virtual Machine Platform
- [ ] Hyper-V
- [ ] Docker Desktop
- [ ] WinNAT
- [ ] HNS
- [ ] Firewall
- [ ] localhost bind
- [ ] Code Integrity events
- [ ] Smart App Control diagnostics

## Phase 6：Profile Plugin

- [ ] JSON profile schema
- [ ] Profile validation
- [ ] Profile versioning
- [ ] Built-in profiles
- [ ] External profile loading
- [ ] 不允許 profile 定義任意命令

## Phase 7：產品化

- [x] Report export
- [ ] MSIX / installer
- [ ] Code signing
- [ ] Auto update
- [ ] Privacy statement
- [ ] Release notes
- [ ] Demo screenshots
- [ ] Portfolio documentation
