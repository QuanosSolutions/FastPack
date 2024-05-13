param (
    [Parameter(Mandatory=$false)]
    [bool]
    $CreateGlobalTool = $true,

    [Parameter(Mandatory=$false)]
    [bool]
    $CreateNugetPackage = $true,

    [Parameter(Mandatory=$false)]
    [bool]
    $CreateExecutables = $true,

    [Parameter(Mandatory=$false)]
    [string]
    $Configuration = 'Release',

    [Parameter(Mandatory=$false)]
    [string]
    $ReleaseRootDirectoryPath,

    [Parameter(Mandatory=$false)]
    [string[]]
    $Runtimes = @("win-x64", "linux-x64", "osx-x64"),

    [Parameter(Mandatory=$false)]
    [string]
    $Verbosity = 'q',

    [Parameter(Mandatory=$false)]
    [bool]
    $AuthenticodeSign = $false
)

# Activate backward compatibility.
$PSNativeCommandArgumentPassing = 'Legacy'

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$stopWatch = [System.Diagnostics.Stopwatch]::StartNew()
Import-Module (Join-Path $PSScriptRoot 'CreateRelease.psm1') -Force

Write-Host "> Creating release"
Write-Host "> "

if (!$ReleaseRootDirectoryPath) {
  $ReleaseRootDirectoryPath = Join-Path $PSScriptRoot "../release"
}

New-Item -Path $releaseRootDirectoryPath -ItemType Directory -Force | Out-Null

if ($CreateGlobalTool) {
  Publish-GlobalTool -ReleaseRootDirectoryPath $releaseRootDirectoryPath -Verbosity $Verbosity -AuthenticodeSign $AuthenticodeSign
}

if ($CreateNugetPackage) {
  Publish-NugetPackage -ReleaseRootDirectoryPath $releaseRootDirectoryPath -Configuration $Configuration -Verbosity $Verbosity -AuthenticodeSign $AuthenticodeSign
}

if ($CreateExecutables) {
  Publish-Executables -ReleaseRootDirectoryPath $releaseRootDirectoryPath -Configuration $Configuration -Runtimes $Runtimes -Verbosity $Verbosity -AuthenticodeSign $AuthenticodeSign
}

Write-Host "> Finished in $($stopWatch.Elapsed)"