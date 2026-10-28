# EN: Shared local build helpers; output paths stay inside the repository.
# JA: ローカルビルド用の共通処理です。出力先はリポジトリ内に限定します。
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path $PSScriptRoot -Parent
function Resolve-Managed([string]$Path) {
    # EN: Resolve installed game references without committing machine-specific paths.
    # JA: 個別 PC のパスを公開ソースに埋め込まず、インストール済み参照先を解決します。
    if (-not $Path) { $Path = $env:CSII_MANAGEDPATH }
    if (-not $Path) { $Path = [Environment]::GetEnvironmentVariable('CSII_MANAGEDPATH', 'User') }
    if (-not $Path -or -not (Test-Path -LiteralPath (Join-Path $Path 'Game.dll'))) {
        throw 'Provide -ManagedPath <Cities2_Data\Managed>, or repair the in-game Modding Toolchain.'
    }
    return (Resolve-Path -LiteralPath $Path).Path
}
function Assert-InsideRepository([string]$Path) {
    # EN: Validate the absolute destination before writing or removing generated copies.
    # JA: 生成物の書き込みや削除の前に、絶対パスが工程内であることを確認します。
    $full = [IO.Path]::GetFullPath($Path)
    $prefix = [IO.Path]::GetFullPath($RepoRoot).TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar
    if (-not $full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Output must stay within repository: $full" }
    return $full
}
function Invoke-Compiler([string[]]$CompilerArgs) {
    $sdk = @(& dotnet --list-sdks | Where-Object { $_ -match '^\d' })[-1]
    if ($LASTEXITCODE -ne 0 -or $sdk -notmatch '^([^ ]+) \[(.+)\]') { throw '.NET SDK not found' }
    $csc = Join-Path (Join-Path $Matches[2] $Matches[1]) 'Roslyn\bincore\csc.dll'
    & dotnet $csc @CompilerArgs
    if ($LASTEXITCODE -ne 0) { throw "C# test compiler failed: $LASTEXITCODE" }
}
