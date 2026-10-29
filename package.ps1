param(
    [string]$Version = '0.1.0',
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'

if (-not $NoBuild) {
    & (Join-Path $PSScriptRoot 'build.ps1') -Configuration Release -Platform x64
    if (-not $?) { throw 'TaskbarZ build failed.' }
}

$source = Join-Path $PSScriptRoot 'APPX\TaskbarZ64\Release'
$dist = Join-Path $PSScriptRoot 'dist'
$stage = Join-Path $PSScriptRoot ('.build\package-' + [guid]::NewGuid().ToString('N') + '\TaskbarZ')
$zip = Join-Path $dist "TaskbarZ-v$Version-win-x64.zip"

$runtimeFiles = @(
    'TaskbarZ.exe',
    'TaskbarZ.exe.config',
    'TaskbarZ.Core.dll',
    'Emoji.Wpf.dll',
    'Microsoft.Win32.TaskScheduler.dll',
    'ModernWpf.dll',
    'ModernWpf.Controls.dll',
    'Typography.GlyphLayout.dll',
    'Typography.OpenFont.dll',
    'System.ValueTuple.dll'
)

New-Item -ItemType Directory -Path $stage -Force | Out-Null
New-Item -ItemType Directory -Path $dist -Force | Out-Null
foreach ($file in $runtimeFiles) {
    $path = Join-Path $source $file
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing release file: $file" }
    Copy-Item -LiteralPath $path -Destination $stage
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'LICENSE') -Destination $stage
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination $stage

if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip }
Compress-Archive -Path $stage -DestinationPath $zip
Get-FileHash -Algorithm SHA256 -LiteralPath $zip | Select-Object Path, Hash
