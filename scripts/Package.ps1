[CmdletBinding()]
param([string]$OutputDirectory = 'artifacts\packages')
# EN: Package tested local outputs; never install or invoke remote services.
# JA: テスト済みのローカル出力を梱包し、インストールや遠隔サービス実行は行いません。
. "$PSScriptRoot\Common.ps1"
$destination = if ([IO.Path]::IsPathRooted($OutputDirectory)) { Assert-InsideRepository $OutputDirectory } else { Assert-InsideRepository (Join-Path $RepoRoot $OutputDirectory) }
if (Test-Path -LiteralPath $destination) { throw 'Package output already exists. Use a fresh output directory to preserve deliveries.' }
$release = Get-Content -LiteralPath (Join-Path $RepoRoot 'release.json') -Raw | ConvertFrom-Json
$stage = Join-Path $RepoRoot 'artifacts\staging\TrainReDoor'
if (-not (Test-Path -LiteralPath (Join-Path $stage 'TrainReDoor.dll'))) { throw 'Run Build.ps1 first' }
$checks = Join-Path $RepoRoot 'artifacts\checks'
$testResults = Get-Content -LiteralPath (Join-Path $checks 'TestResults.json') -Raw | ConvertFrom-Json
if ($testResults.dll_sha256 -ne (Get-FileHash -LiteralPath (Join-Path $stage 'TrainReDoor.dll') -Algorithm SHA256).Hash.ToLower()) {
    throw 'DLL changed since tests; run Test.ps1 before packaging'
}
$validation = Get-Content -LiteralPath (Join-Path $checks 'Validation.json') -Raw | ConvertFrom-Json
if ($validation.checks -lt 1) { throw 'Run Validate.ps1 before packaging' }
foreach ($required in @('TrainReDoor.json','TrainReDoor.png','LICENSE','docs\CREATOR_GUIDE.zh-CN.md','docs\INSTALL.zh-CN.md','docs\CREATOR_GUIDE.en.md','docs\CREATOR_GUIDE.ja.md')) {
    if (-not (Test-Path -LiteralPath (Join-Path $stage $required))) { throw "Missing runtime payload: $required" }
}
$dlls = @(Get-ChildItem -LiteralPath $stage -Recurse -File -Filter '*.dll')
foreach ($dll in $dlls) {
    if ($dll.Name -notin @('TrainReDoor.dll','TrainReDoor_win_x86_64.dll')) { throw "Unexpected dependency DLL in payload: $($dll.Name)" }
}
New-Item -ItemType Directory -Path $destination | Out-Null
$version = $release.releaseVersion
$runtimeZip = Join-Path $destination "TrainReDoor_$version.zip"
Compress-Archive -LiteralPath $stage -DestinationPath $runtimeZip -CompressionLevel Optimal
# EN: Flat content directory, matching the official ModPublisher -c argument.
# JA: 公式 ModPublisher の -c 引数に渡す平面の内容ディレクトリです。
$paradox = Join-Path $destination 'Paradox\content\TrainReDoor'
New-Item -ItemType Directory -Path (Split-Path $paradox -Parent) -Force | Out-Null
Copy-Item -LiteralPath $stage -Destination $paradox -Recurse
Copy-Item -LiteralPath (Join-Path $RepoRoot 'src\TrainReDoor\Properties') -Destination (Join-Path $destination 'Paradox\Properties') -Recurse
$sourceStage = Join-Path $destination 'GitHub\TrainReDoor'
New-Item -ItemType Directory -Path $sourceStage -Force | Out-Null
$sourceItems = @('README.md','README.en.md','README.ja.md','README.zh-CN.md','LICENSE','THIRD_PARTY_NOTICES.md','CHANGELOG.md','CONTRIBUTING.md','.gitignore','.gitattributes','TrainReDoor.sln','release.json','src','runtime','examples','docs','scripts','tests','release')
foreach ($name in $sourceItems) { Copy-Item -LiteralPath (Join-Path $RepoRoot $name) -Destination $sourceStage -Recurse }
# EN: Exclude build products even if the project was built in-place before copying.
# JA: コピー前にビルド済みでも、生成物はソースパッケージから除外します。
foreach ($relative in @('src\TrainReDoor\bin','src\TrainReDoor\obj','src\TrainReDoor\Library','src\TrainReDoor\Temp')) {
    $target = Assert-InsideRepository (Join-Path $sourceStage $relative)
    if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Recurse -Force }
}
$sourceZip = Join-Path $destination "TrainReDoor_$version`_Source.zip"
Compress-Archive -Path (Join-Path $sourceStage '*') -DestinationPath $sourceZip -CompressionLevel Optimal
$hashes = @($runtimeZip,$sourceZip | ForEach-Object { $f = Get-FileHash -LiteralPath $_ -Algorithm SHA256; $f.Hash.ToLower() + '  ' + [IO.Path]::GetFileName($_) })
$hashes | Set-Content -LiteralPath (Join-Path $destination 'SHA256SUMS.txt') -Encoding ascii
Write-Output "Local packages prepared: $destination"
Write-Output 'Nothing was installed or uploaded. Paradox/content/TrainReDoor is the flat publisher content folder.'
