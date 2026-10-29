param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('x64', 'x86', 'AnyCPU')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (!(Test-Path $vswhere)) { throw 'Visual Studio or Visual Studio Build Tools is required.' }
$msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (!$msbuild) { throw 'MSBuild was not found in Visual Studio.' }

$packages = Join-Path $PSScriptRoot '.build\packages'
$frameworkRoot = Join-Path $packages 'microsoft.netframework.referenceassemblies.net48\1.0.3\build\'
$projects = @(
    (Join-Path $PSScriptRoot 'TaskbarX\TaskbarX\TaskbarX.vbproj'),
    (Join-Path $PSScriptRoot 'TaskbarX Configurator\TaskbarX Configurator\TaskbarX Configurator.vbproj')
)

foreach ($project in $projects) {
    & $msbuild $project -t:Restore "-p:RestorePackagesPath=$packages" -verbosity:minimal -nologo
    if ($LASTEXITCODE -ne 0) { throw "Package restore failed: $project" }
    # Pass this globally so WPF's temporary project also finds the reference assemblies.
    & $msbuild $project -m "-p:Configuration=$Configuration" "-p:Platform=$Platform" "-p:TargetFrameworkRootPath=$frameworkRoot" -verbosity:minimal -nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $project" }
}
