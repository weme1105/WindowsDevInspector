# WindowsDevInspector Roadmap

## Phase 0：Repository 基礎

- [ ] 建立 Solution
- [ ] 建立 `.editorconfig`
- [ ] 建立 `global.json`
- [ ] 建立測試專案
- [ ] 建立 GitHub Actions
- [ ] 確認 `dotnet build`
- [ ] 確認 `dotnet test`

## Phase 1：唯讀掃描 MVP

- [ ] Profile 選擇
- [ ] Check catalog
- [ ] Directory checks
- [ ] Long Path check
- [ ] Developer Mode check
- [ ] PATH invalid entry check
- [ ] Tool command checks
- [ ] 結果排序
- [ ] 評分
- [ ] 點擊項目顯示詳細內容

## Phase 2：第一批安全修正

- [ ] Change Plan
- [ ] Remediation whitelist
- [ ] Elevated Worker
- [ ] 建立資料夾
- [ ] Long Path remediation
- [ ] Developer Mode remediation
- [ ] 執行後驗證
- [ ] JSON result

## Phase 3：Rollback

- [ ] Backup schema
- [ ] Backup browser
- [ ] Rollback plan
- [ ] Elevated rollback
- [ ] Rollback verification

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

- [ ] Report export
- [ ] MSIX / installer
- [ ] Code signing
- [ ] Auto update
- [ ] Privacy statement
- [ ] Release notes
- [ ] Demo screenshots
- [ ] Portfolio documentation
