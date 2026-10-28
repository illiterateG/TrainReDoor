[CmdletBinding()]
param([string]$ManagedPath)
# EN: Test the real built DLL with generic synthetic inputs.
# JA: 実際のビルド済み DLL を汎用の合成入力で検証します。
. "$PSScriptRoot\Common.ps1"
$managed = Resolve-Managed $ManagedPath
$stage = Join-Path $RepoRoot 'artifacts\staging\TrainReDoor'
$testDir = Assert-InsideRepository (Join-Path $RepoRoot 'artifacts\tests')
$logDir = Assert-InsideRepository (Join-Path $RepoRoot 'artifacts\checks')
New-Item -ItemType Directory -Path $testDir,$logDir -Force | Out-Null
$library = Join-Path $stage 'TrainReDoor.dll'
if (-not (Test-Path -LiteralPath $library)) { throw 'Run scripts/Build.ps1 first' }
Copy-Item -LiteralPath $library -Destination $testDir -Force
$framework = @('mscorlib.dll','System.dll','System.Core.dll','System.Runtime.Serialization.dll','System.Xml.dll')
$refs = @($framework | ForEach-Object { '/reference:' + (Join-Path $managed $_) })
$exe = Join-Path $testDir 'ProtocolTests.exe'
Invoke-Compiler (@('/nologo','/noconfig','/nostdlib+','/target:exe',"/out:$exe","/reference:$library") + $refs + (Join-Path $RepoRoot 'tests\ProtocolTests.cs'))
& $exe $library $managed 2>&1 | Tee-Object -FilePath (Join-Path $logDir 'ProtocolTests.log')
if ($LASTEXITCODE -ne 0) { throw 'Protocol/DLL tests failed' }
$assetExe = Join-Path $testDir 'GenericAssetTests.exe'
Invoke-Compiler (@('/nologo','/noconfig','/nostdlib+','/target:exe',"/out:$assetExe","/reference:$library") + $refs + (Join-Path $RepoRoot 'tests\GenericAssetTests.cs'))
$fixtures = @(Get-ChildItem -LiteralPath (Join-Path $RepoRoot 'tests\fixtures') -Filter '*.json' | Sort-Object Name | ForEach-Object FullName)
& $assetExe $library @fixtures 2>&1 | Tee-Object -FilePath (Join-Path $logDir 'GenericAssetTests.log')
if ($LASTEXITCODE -ne 0) { throw 'Synthetic generic asset tests failed' }
$configExe = Join-Path $testDir 'ConfigurationExamplesTests.exe'
Invoke-Compiler (@('/nologo','/noconfig','/nostdlib+','/target:exe',"/out:$configExe","/reference:$library") + $refs + (Join-Path $RepoRoot 'tests\ConfigurationExamplesTests.cs'))
& $configExe $library $managed (Join-Path $RepoRoot 'runtime\TrainReDoor.json') (Join-Path $RepoRoot 'examples\CustomTrain.example.json') 2>&1 | Tee-Object -FilePath (Join-Path $logDir 'ConfigurationExamplesTests.log')
if ($LASTEXITCODE -ne 0) { throw 'Bundled configuration examples failed actual DLL parsing' }
[ordered]@{
    dll_sha256=(Get-FileHash -LiteralPath $library -Algorithm SHA256).Hash.ToLower()
    protocol_checks=[int]([regex]::Match((Get-Content -LiteralPath (Join-Path $logDir 'ProtocolTests.log') -Raw),'TOTAL_PASS=(\d+)').Groups[1].Value)
    synthetic_asset_cases=4
    synthetic_door_bindings=16
    config_examples_passed=$true
    game_runtime_verified=$false
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $logDir 'TestResults.json') -Encoding utf8
Write-Output 'Synthetic protocol fixtures only; no game simulation was started.'
