[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string] $MsiPath,

    [string] $ExpectedVersion
)

$ErrorActionPreference = 'Stop'
$resolvedMsiPath = (Resolve-Path -LiteralPath $MsiPath).Path
$installer = New-Object -ComObject WindowsInstaller.Installer
$database = $installer.GetType().InvokeMember(
    'OpenDatabase',
    'InvokeMethod',
    $null,
    $installer,
    @($resolvedMsiPath, 0))
$summaryInformation = $installer.GetType().InvokeMember(
    'SummaryInformation',
    'GetProperty',
    $null,
    $database,
    @(0))
$templateSummary = $summaryInformation.GetType().InvokeMember(
    'Property',
    'GetProperty',
    $null,
    $summaryInformation,
    @(7))

function Read-MsiRows {
    param([Parameter(Mandatory = $true)][string] $Sql)

    $view = $database.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $database, @($Sql))
    try {
        $view.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $view, $null) | Out-Null
        $rows = @()
        while ($record = $view.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $view, $null)) {
            $fieldCount = $record.GetType().InvokeMember('FieldCount', 'GetProperty', $null, $record, $null)
            $fields = for ($index = 1; $index -le $fieldCount; $index++) {
                $record.GetType().InvokeMember('StringData', 'GetProperty', $null, $record, @($index))
            }
            $rows += [pscustomobject]@{ Fields = [object[]] $fields }
        }
        return $rows
    }
    finally {
        $view.GetType().InvokeMember('Close', 'InvokeMethod', $null, $view, $null) | Out-Null
    }
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool] $Condition,
        [Parameter(Mandatory = $true)][string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

$properties = @{}
foreach ($row in Read-MsiRows -Sql 'SELECT `Property`, `Value` FROM `Property`') {
    $properties[$row.Fields[0]] = $row.Fields[1]
}

Assert-True ($properties['ProductName'] -eq 'WindowsDevInspector') 'Unexpected MSI ProductName.'
Assert-True ($properties['Manufacturer'] -eq 'WindowsDevInspector') 'Unexpected MSI Manufacturer.'
Assert-True ($properties['ALLUSERS'] -eq '1') 'MSI must remain a per-machine package.'
Assert-True (-not [string]::IsNullOrWhiteSpace($properties['UpgradeCode'])) 'MSI UpgradeCode is missing.'
if (-not [string]::IsNullOrWhiteSpace($ExpectedVersion)) {
    Assert-True ($properties['ProductVersion'] -eq $ExpectedVersion) (
        "Expected MSI version $ExpectedVersion but found $($properties['ProductVersion']).")
}
$isX64Template = $templateSummary.StartsWith('x64;', [StringComparison]::OrdinalIgnoreCase) -or
    $templateSummary.StartsWith('Intel64;', [StringComparison]::OrdinalIgnoreCase)
Assert-True $isX64Template (
    "MSI must target x64. Template summary: $templateSummary")

$fileNames = @(Read-MsiRows -Sql 'SELECT `FileName` FROM `File`' | ForEach-Object { $_.Fields[0] })
$longFileNames = @($fileNames | ForEach-Object { ($_ -split '\|')[-1] })
Assert-True ($longFileNames -contains 'WindowsDevInspector.App.exe') 'App executable is missing from MSI.'
Assert-True ($longFileNames -contains 'WindowsDevInspector.ElevatedWorker.exe') 'ElevatedWorker executable is missing from MSI.'

$forbiddenPatterns = @(
    'pnpm',
    'azure-cli',
    'kubectl',
    'terraform',
    'winget',
    'dotnet-runtime',
    'windowsdesktop-runtime',
    '.msi',
    '.msix',
    '.exe.config',
    'coreclr.dll',
    'hostfxr.dll',
    'hostpolicy.dll'
)

Assert-True ($longFileNames.Count -le 50) (
    "MSI payload unexpectedly contains $($longFileNames.Count) files; possible runtime or publish-output contamination.")

foreach ($fileName in $longFileNames) {
    foreach ($pattern in $forbiddenPatterns) {
        Assert-True ($fileName.IndexOf($pattern, [StringComparison]::OrdinalIgnoreCase) -lt 0) (
            "Forbidden bundled software payload detected: $fileName")
    }
}

$upgradeRows = @(Read-MsiRows -Sql 'SELECT `UpgradeCode`, `ActionProperty` FROM `Upgrade`')
Assert-True ($upgradeRows.Count -ge 2) 'Major Upgrade and downgrade detection rows are missing.'

$launchRows = @(Read-MsiRows -Sql 'SELECT `Condition`, `Description` FROM `LaunchCondition`')
$runtimeLaunch = @($launchRows | Where-Object {
    $_.Fields[0] -eq 'Installed OR WDI_DOTNET_DESKTOP_RUNTIME_CHECK = 0'
})
Assert-True ($runtimeLaunch.Count -eq 1) '.NET 10 Desktop Runtime launch condition is missing.'
Assert-True ($runtimeLaunch[0].Fields[1] -like '*NET 10 Desktop Runtime x64*') (
    '.NET runtime launch condition must provide a clear prerequisite message.')

$shortcutRows = @(Read-MsiRows -Sql 'SELECT `Target`, `WkDir` FROM `Shortcut`')
Assert-True ($shortcutRows.Count -eq 1) 'Expected exactly one Start Menu shortcut.'
Assert-True ($shortcutRows[0].Fields[0] -eq '[INSTALLFOLDER]WindowsDevInspector.App.exe') 'Shortcut target is unexpected.'
Assert-True ($shortcutRows[0].Fields[1] -eq 'INSTALLFOLDER') 'Shortcut working directory is unexpected.'

Write-Output "MSI validation passed: $resolvedMsiPath"
Write-Output "ProductVersion: $($properties['ProductVersion'])"
Write-Output "TemplateSummary: $templateSummary"
Write-Output "Payload file count: $($longFileNames.Count)"
