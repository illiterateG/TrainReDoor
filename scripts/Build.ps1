[CmdletBinding()]
param([string]$ManagedPath, [string]$ToolPath)
# EN: Keep the official build and post-processor; stage locally without installing.
# JA: 公式ビルドと後処理を維持し、インストールせず工程内に出力します。
. "$PSScriptRoot\Common.ps1"
$managed = Resolve-Managed $ManagedPath
$stageRoot = Assert-InsideRepository (Join-Path $RepoRoot 'artifacts\staging')
$logDir = Assert-InsideRepository (Join-Path $RepoRoot 'artifacts\checks')
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$buildArgs = @('build', (Join-Path $RepoRoot 'src\TrainReDoor\TrainReDoor.csproj'), '-c', 'Release',
    "-p:ManagedPath=$managed", "-p:MSCORLIBPath=$(Join-Path $managed 'mscorlib.dll')", "-p:LocalModsPath=$stageRoot", '-v:minimal')
if ($ToolPath) { $buildArgs += "-p:CSIIToolPath=$ToolPath" }
Push-Location $RepoRoot
try {
    & dotnet @buildArgs 2>&1 | Tee-Object -FilePath (Join-Path $logDir 'Build.log')
    if ($LASTEXITCODE -ne 0) { throw "Official toolchain build failed: $LASTEXITCODE" }
} finally { Pop-Location }
$dll = Join-Path $stageRoot 'TrainReDoor\TrainReDoor.dll'
if (-not (Test-Path -LiteralPath $dll)) { throw 'Official build did not stage the DLL' }
Get-FileHash -LiteralPath $dll -Algorithm SHA256
