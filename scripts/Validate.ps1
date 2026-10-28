[CmdletBinding()]
param()
# EN: Validate metadata, language-neutral terminology, document links and staged files.
# JA: メタデータ、汎用表記、文書リンク、出力済みファイルを検査します。
. "$PSScriptRoot\Common.ps1"
$count = 0
function Check([bool]$Pass, [string]$Name) {
    if (-not $Pass) { throw "Validation failed: $Name" }
    $script:count++
    Write-Output "PASS $Name"
}
$logDir = Assert-InsideRepository (Join-Path $RepoRoot 'artifacts\checks')
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$release = Get-Content -LiteralPath (Join-Path $RepoRoot 'release.json') -Raw | ConvertFrom-Json
$projectDir = Join-Path $RepoRoot 'src\TrainReDoor'
[xml]$publish = Get-Content -LiteralPath (Join-Path $projectDir 'Properties\PublishConfiguration.xml') -Raw
[xml]$project = Get-Content -LiteralPath (Join-Path $projectDir 'TrainReDoor.csproj') -Raw
Check ($publish.Publish.ModVersion.Value -eq $release.releaseVersion) 'Release and Paradox version agree'
Check ($publish.Publish.GameVersion.Value -eq $release.recommendedGameVersion) 'Recommended game version pattern agrees'
Check ($publish.Publish.ModId.Value -eq [string]$release.paradoxModId) 'Existing platform Mod ID agrees'
Check ($release.paradoxModId -eq 164831 -and $null -eq $release.repositoryUrl) 'Known platform ID retained; repository URL is not embedded'
Check (-not $release.gameRuntimeVerified) 'Gameplay verification record stays accurate'
Check ($publish.Publish.AccessLevel.Value -eq 'Unlisted') 'Initial listing visibility is Unlisted'
Check ($publish.Publish.DisplayName.Value -eq 'Train ReDoor') 'Display name is correct'
$playerText = $publish.SelectSingleNode('/Publish/LongDescription').InnerText
Check ($playerText -match 'How to use' -and $playerText -match '使い方' -and $playerText -match '使用方法') 'Player description includes English, Japanese and Chinese'
Check ($playerText -notmatch 'RenderPrefab|JSON|None=0|synthetic|assembly|procedural|bone|ソース|合成|程序集|骨骼|蒙皮') 'Platform description stays player-facing'

Check ((Get-Content -LiteralPath (Join-Path $RepoRoot 'LICENSE') -Raw) -match 'MIT License') 'MIT license is present'
Check (@($project.Project.Target | Where-Object Name -EQ 'RequireExplicitRemotePublish').Count -eq 1) 'Explicit remote-publish guard exists'
$guard = @($project.Project.Target | Where-Object Name -EQ 'RequireExplicitRemotePublish')[0]
Check ($guard.BeforeTargets -eq 'Publish;RunModPublisher' -and $guard.Error.Condition -match 'EnableRemotePublish') 'Guard precedes both publisher entry points'
foreach ($pair in @(@('PublishNewMod','Publish'),@('PublishNewVersion','NewVersion'),@('UpdatePublishedConfiguration','Update'))) {
    [xml]$profile = Get-Content -LiteralPath (Join-Path $projectDir "Properties\PublishProfiles\$($pair[0]).pubxml") -Raw
    Check ($profile.Project.PropertyGroup.ModPublisherCommand -eq $pair[1]) "Official publish profile: $($pair[0])"
}
$thumbnail = Join-Path $projectDir $publish.Publish.Thumbnail.Value
$bytes = [IO.File]::ReadAllBytes($thumbnail)
Check ([BitConverter]::ToString($bytes[0..7]) -eq '89-50-4E-47-0D-0A-1A-0A') 'Thumbnail is a real PNG'
$widthBytes = $bytes[16..19]; [Array]::Reverse($widthBytes)
$heightBytes = $bytes[20..23]; [Array]::Reverse($heightBytes)
Check ([BitConverter]::ToInt32($widthBytes,0) -eq 1024 -and [BitConverter]::ToInt32($heightBytes,0) -eq 1024) 'Thumbnail dimensions are 1024 square'
Check ((Get-FileHash -LiteralPath $thumbnail -Algorithm SHA256).Hash -eq (Get-FileHash -LiteralPath (Join-Path $RepoRoot 'runtime/TrainReDoor.png') -Algorithm SHA256).Hash) 'Platform thumbnail and bundled preview have identical bytes'
foreach ($name in @('runtime\TrainReDoor.json','examples\CustomTrain.example.json')) {
    $config = Get-Content -LiteralPath (Join-Path $RepoRoot $name) -Raw | ConvertFrom-Json
    Check ($config.formatVersion -eq 1 -and $config.openSeconds -ge 0.05 -and $config.closeSeconds -le 60) "JSON shape: $name"
}
$textFiles = @(Get-ChildItem -LiteralPath $RepoRoot -Recurse -File | Where-Object {
    $_.Extension -in @('.cs','.csproj','.ps1','.md','.json','.xml','.pubxml','.sln') -and
    $_.FullName -notmatch '[\\/](artifacts|bin|obj|\.git)[\\/]'
})
foreach ($file in $textFiles) {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    Check ($text -notmatch ('(?i)' + 'e' + '501')) "Generic public terminology: $($file.Name)"
    Check ($text -notmatch '(?i)[A-WY-Z]:[\\/](?:Users[\\/]|CSII_Models|SteamLibrary|Cities Skylines II)') "No private machine path: $($file.Name)"
    if ($file.Extension -eq '.md') {
        foreach ($match in [regex]::Matches($text,'\[[^\]]*\]\(([^\s\)]+)\)')) {
            $link = $match.Groups[1].Value
            if ($link -match '^https?://' -or $link.StartsWith('#')) { continue }
            $path = [Uri]::UnescapeDataString(($link -split '#')[0])
            Check (Test-Path -LiteralPath (Join-Path $file.DirectoryName $path)) "Document link: $($file.Name) -> $link"
        }
    }
}
$stage = Join-Path $RepoRoot 'artifacts\staging\TrainReDoor'
if (Test-Path -LiteralPath $stage) {
    $dll = Join-Path $stage 'TrainReDoor.dll'
    Check (Test-Path -LiteralPath $dll) 'Staged runtime DLL exists'
    Check ([Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString() -eq $release.assemblyVersion) 'Actual DLL assembly version agrees'
    foreach ($item in Get-ChildItem -LiteralPath $stage -Recurse -File -Filter '*.dll') {
        Check ($item.Name -in @('TrainReDoor.dll','TrainReDoor_win_x86_64.dll')) "Only own runtime/native DLL: $($item.Name)"
    }
    foreach ($name in @('TrainReDoor.json','docs\CREATOR_GUIDE.zh-CN.md','docs\CREATOR_GUIDE.en.md','docs\CREATOR_GUIDE.ja.md','README.ja.md','TrainReDoor.png','LICENSE')) {
        Check (Test-Path -LiteralPath (Join-Path $stage $name)) "Staged payload contains $name"
    }
}
[ordered]@{checks=$count;game_runtime_verified=$false;remote_upload_executed=$false} |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $logDir 'Validation.json') -Encoding utf8
Write-Output "TOTAL_LOCAL_CHECKS=$count"
