# WindowsDevInspector 撠?閬

## 1. ??摰儔

Windows ??啣?撣詨?銝???撠?撌亙憭望?嚗?
- PATH 畾???摨隤?- Long Path ?芸???- Developer Mode ?芸???- WSL?ocker?irtual Machine Platform ???銝??- SDK 摰?雿?CLI ?⊥??瑁?
- 憭???Node.js?o??NET 銵?
- WinNAT?NS?irewall ??localhost bind ??
- Windows Insider Build 銵撌桃
- 撌亙摮嚗??銝泵??銵ㄖ閬?

?暹?閮箸?單?芾?Ｙ??勗?嚗瘜?閫蝭拚嚗?蝻箏?摰??汗??儔?耨甇??蝔?
## 2. ?Ｗ??格?

WindowsDevInspector ??嚗?
1. 靘??脰??銵ㄖ?豢?瑼Ｘ蝭???2. ?瑁??航??啣?????3. 隞乩??渲??芋???曄???4. 憿舐內??敶梢?◢?芥?潸????潦?5. ???桅??甈∩耨甇??6. ?? UAC Worker ?瑁?摰?賢??桐耨甇??7. 靽格迤??隞踝?靽格迤敺?霅?8. ?芯??舀 Rollback???箄? Profile Plugin??
## 3. ?璅?
MVP 銝???

- 摰隡平蝡舫?蝞∠?
- 蝢斤????葉?函蔡
- 蝬脣?蝞∠?
- 憭折??垢?餉蝞∠?
- 撽?蝔??芸??湔
- ?? Windows 摰?
- 隞餅?蝚砌??寡?擃耨敺?- ?芸??????Windows Insider ??

## 4. 雿輻瘚?

```text
?豢?銝餉?閫
???豢??銵敦??????瑼Ｘ
???亦???敺?????暺???亦?靽格迤?批捆
???暸?桅????甈∩耨甇???蝣箄? Change Plan
???? Elevated Worker
??UAC
???遢
???瑁?
??撽?
?????
```

## 5. 閫??銵?Profile

### 閫

| ID | ?迂 |
|---|---|
| common | ???啣? |
| frontend | ?垢撌亦?撣?|
| backend | 敺垢撌亦?撣?|
| fullstack | ?函垢撌亦?撣?|
| cloud | ?脩垢 / DevOps / SRE |
| dba | DBA / 鞈?摨怠極蝔葦 |

### ?銵敦??
| ID | ?銵?|
|---|---|
| dotnet | .NET |
| go | Go |
| angular | Angular |
| node | Node.js |
| docker | Docker |
| wsl | WSL |
| kubernetes | Kubernetes |
| azure | Azure CLI |
| sqlserver | SQL Server |


### Environment Profile Update

Profiles should be modeled as multi-select roles and multi-select technologies, not as mutually exclusive presets.

Role selections:

- Frontend Engineer
- Backend Engineer
- DBA / Data Engineer
- QA / Test Engineer
- DevOps / SRE
- Mobile Engineer
- Desktop Engineer

Derived labels:

- Fullstack = Frontend + Backend
- Cloud Developer = Backend or DevOps + cloud platform/tool
- Data Platform = Backend + DBA
- Test Automation = QA + Frontend or Backend

Technology selection should be a searchable grouped multi-select dropdown. The initial catalog should include 30 frontend technologies, 30 backend technologies, and 30 database technologies. The source of truth is `docs/ENVIRONMENT_PROFILES.md`.

Selected technologies that do not yet have automated checks should still appear in the result model as `Info`, so users can see that the selection is recognized but diagnostics are not implemented yet.
## 6. CheckResult 鞈?璅∪?

瘥炎?亦??撠??恬?

```text
Id
Category
Name
Severity
CurrentValue
ExpectedValue
Impact
CanFix
Risk
RequiresElevation
RequiresRestart
SupportsRollback
RemediationId
```

Severity嚗?
- Pass
- Info
- Warning
- Critical

Risk嚗?
- None
- Low
- Medium
- High

## 7. ??閬?

蝯????芸???嚗?
1. Critical
2. Warning
3. Info
4. Pass

????嚗?
1. Category
2. Name

PASS嚗?
- ??敺?- 銝＊蝷箏?豢?
- 銝??交甈∩耨甇?
## 8. MVP 瑼Ｘ?

### ?

- Windows ???Build
- 蝟餌絞?嗆?
- Developer Mode
- Long Paths
- PATH 銝??券???- PATH ???
- `D:\Source`
- `D:\Projects`
- `D:\GoNote`
- PowerShell 7
- Git
- winget

### .NET

- dotnet CLI
- SDK ?”
- Runtime ?”
- NuGet source
- ASP.NET Core runtime
- Visual Studio / Build Tools

### Go

- go CLI
- Go version
- GOROOT
- GOPATH
- Go bin PATH
- `go env`
- Smart App Control / Code Integrity ?賊?閮箸?內

### Angular / Node.js

- node
- npm
- pnpm
- Angular CLI
- Node.js LTS ?詨捆??- npm global prefix

### Cloud / DevOps

- Docker CLI
- Docker Desktop
- WSL
- WSL version
- WSL distro
- Virtual Machine Platform
- Hyper-V
- kubectl
- Azure CLI
- Terraform
- localhost bind
- WinNAT
- HNS

### DBA / SQL Server

- SQL Server tooling
- sqlcmd
- ODBC Driver
- LocalDB
- SSMS
- TCP connectivity prerequisites

## 9. 蝚砌??嫣耨甇?
### 撱箇?鞈?憭?
- `D:\Source`
- `D:\Projects`
- `D:\GoNote`

### Long Paths

Registry嚗?
```text
HKLM\SYSTEM\CurrentControlSet\Control\FileSystem
LongPathsEnabled = 1
```

### Developer Mode

Registry嚗?
```text
HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock
AllowDevelopmentWithoutDevLicense = 1
```

## 10. Remediation 摰璅∪?

UI 銝??喳隞餅?蝟餌絞?賭誘??
UI ?芾?喳嚗?
```json
{
  "remediationId": "enable-long-paths"
}
```

Worker ?折???箏? mapping 瘙箏?撖阡???嚗?
```text
enable-long-paths
???箏? Registry hive
???箏? Registry path
???箏? value name
???箏? value
```

銝??迂嚗?
```json
{
  "command": "powershell ...",
  "registryPath": "...",
  "value": "..."
}
```

## 11. ?遢??Rollback

?遢?身雿蔭嚗?
```text
C:\ProgramData\WindowsDevInspector\Backups
```

瘥活?瑁??Ｙ?嚗?
```text
<plan-id>.backup.json
<plan-id>.result.json
```

?遢閮?嚗?
- 靽格憿?
- ?格?
- ?臬?摮
- ????- ?瑁???
- App version

Rollback 敹??活?? Elevated Worker??
## 12. 皜祈岫蝑

### Unit Test

- Profile filtering
- Score calculation
- Result sorting
- Change Plan validation
- Remediation whitelist
- Registry value conversion

### Integration Test

- Temporary directory create / rollback
- Fake registry abstraction
- Fake process runner
- Worker plan validation
- Unsupported remediation rejection

### Manual Test

- Standard user ??
- UAC ??
- UAC ?亙?
- Registry 撖怠憭望?
- Folder 撌脣???- Worker 蝯?雿?result 蝻箏仃
- 靽格迤敺??唳???
## 13. 閮剛???

?????嚗?閬???撱箇? 400 ?炎?乓?
瘥????恬?

```text
Check
??UI 憿舐內
??Preview
??Remediation
??Backup
??Verification
??Test
```

