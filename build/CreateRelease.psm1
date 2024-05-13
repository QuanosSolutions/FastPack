# Activate backward compatibility.
$PSNativeCommandArgumentPassing = 'Legacy'

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-SourceDirectory {
  return Join-Path $PSScriptRoot "../src" -Resolve
}

function Get-FastPackProjectFilePath {
  return Join-Path (Get-SourceDirectory) "FastPack/FastPack.csproj" -Resolve
}

function Get-FastPackCommonProjectFilePath {
  return Join-Path (Get-SourceDirectory) "FastPack.Lib/FastPack.Lib.csproj" -Resolve
}

function Invoke-AuthenticodeSigning {
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $Path,

    [Parameter(Mandatory=$false)]
    [string]
    $SignToolExecutablePath = 'C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe',

    [Parameter(Mandatory=$false)]
    [string]
    $TimeStampServer = 'http://timestamp.digicert.com',

    [Parameter(Mandatory=$false)]
    [string]
    $AuthenticodeSignCertificateName = 'Quanos Solutions GmbH'
  )

  $signtoolArguments = @(
    'sign',
    '/q',
    '/a',
    '/n', "`"$AuthenticodeSignCertificateName`"",
    '/t', $TimeStampServer,
    "`"$Path`""
  )

  & $SignToolExecutablePath $signtoolArguments

  if ($LASTEXITCODE -ne 0) {
    throw "Failed to sign '$Path'. Exit code was: $LASTEXITCODE"
  }
}

function Publish-NugetPackage {
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $ReleaseRootDirectoryPath,

    [Parameter(Mandatory=$false)]
    [string]
    $Configuration = 'Release',

    [Parameter(Mandatory=$false)]
    [string]
    $Verbosity = 'q',

    [Parameter(Mandatory=$false)]
    [bool]
    $AuthenticodeSign = $false
  )

  Write-Host "> Publishing nuget package ..."
  $PackageOutputPath = Join-Path $ReleaseRootDirectoryPath "NugetPackage"
  Remove-Item -Path $PackageOutputPath -Force -Recurse -ErrorAction SilentlyContinue
  New-Item -Path $PackageOutputPath -ItemType Directory -Force | Out-Null

  & dotnet @('pack', "`"$(Get-FastPackCommonProjectFilePath)`"", '-c', $Configuration, '-v', $Verbosity, '--include-source', '--force', '--nologo', "-p:PackageOutputPath=`"$PackageOutputPath`"", "-p:SignFastpackAssemblies=$AuthenticodeSign")

  if ($LASTEXITCODE -ne 0) {
    throw "Publish of nuget package failed with exit code $LASTEXITCODE"
  }

  Write-Host "> Finished publishing nuget package"
}

function Publish-GlobalTool {
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $ReleaseRootDirectoryPath,

    [Parameter(Mandatory=$false)]
    [string]
    $Verbosity = 'q',

    [Parameter(Mandatory=$false)]
    [bool]
    $AuthenticodeSign = $false
  )

  Write-Host "> Publishing global tool ..."
  $PackageOutputPath = Join-Path $ReleaseRootDirectoryPath "GlobalTool"
  Remove-Item -Path $PackageOutputPath -Force -Recurse -ErrorAction SilentlyContinue
  New-Item -Path $PackageOutputPath -ItemType Directory -Force | Out-Null

  & dotnet @('pack', "`"$(Get-FastPackProjectFilePath)`"", '-c', 'GlobalTool', '-v', $Verbosity, "--force", '--nologo',"-p:PackageOutputPath=`"$PackageOutputPath`"", "-p:SignFastpackAssemblies=$AuthenticodeSign")

  if ($LASTEXITCODE -ne 0) {
    throw "Publish of global tool failed with exit code $LASTEXITCODE"
  }

  Write-Host "> Finished publishing global tool"
}

function Publish-Executables {
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $ReleaseRootDirectoryPath,

    [Parameter(Mandatory=$false)]
    [string]
    $Configuration = 'Release',

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

  Write-Host "> Publishing executables for all runtimes ..."
  $BuildOutputPath = Join-Path $PSScriptRoot "../build-output"
  $ExecutablesOutputPath = Join-Path $BuildOutputPath "Executables"
  Remove-Item -Path $ExecutablesOutputPath -Force -Recurse -ErrorAction SilentlyContinue
  New-Item -Path $ExecutablesOutputPath -ItemType Directory -Force | Out-Null

  $Runtimes | % {
    $ExecutableOutputPath = Join-Path $ExecutablesOutputPath $_

    $runtime = $_
    if (!$runtime.StartsWith("win")) {
      $AuthenticodeSign = $false
    }

    Invoke-FastpackBuild -Configuration $Configuration -OutputDirectory (Join-Path $ExecutableOutputPath "RuntimeIncluded") `
                         -Verbosity $Verbosity -RuntimeIdentifier $_ -IncludeRuntime $true -TrimmExecutable $true -AuthenticodeSign $AuthenticodeSign
    Invoke-FastpackBuild -Configuration $Configuration -OutputDirectory (Join-Path $ExecutableOutputPath "RuntimeExcluded") `
                         -Verbosity $Verbosity -RuntimeIdentifier $_ -IncludeRuntime $false -TrimmExecutable $false -AuthenticodeSign $AuthenticodeSign

    if ($runtime.StartsWith("win")) {
      # we have to sign the windows executables here
      Invoke-AuthenticodeSigning -Path (Join-Path $ExecutableOutputPath "RuntimeIncluded/FastPack.exe")
      Invoke-AuthenticodeSigning -Path (Join-Path $ExecutableOutputPath "RuntimeExcluded/FastPack.exe")
    }

    Compress-Archive -Path (Join-Path $ExecutableOutputPath "RuntimeIncluded/*") -DestinationPath (Join-Path $ReleaseRootDirectoryPath "FastPack-$runtime-RuntimeIncluded.zip") -CompressionLevel Optimal -Force
    Compress-Archive -Path (Join-Path $ExecutableOutputPath "RuntimeExcluded/*") -DestinationPath (Join-Path $ReleaseRootDirectoryPath "FastPack-$runtime-RuntimeExcluded.zip") -CompressionLevel Optimal -Force

	if ($runtime.StartsWith("linux") -or $runtime.StartsWith("osx")) {
		tar -czvf (Join-Path $ReleaseRootDirectoryPath "FastPack-$runtime-RuntimeIncluded.tar.gz") -C (Join-Path $ExecutableOutputPath "RuntimeIncluded" -Resolve) .
		tar -czvf (Join-Path $ReleaseRootDirectoryPath "FastPack-$runtime-RuntimeExcluded.tar.gz") -C (Join-Path $ExecutableOutputPath "RuntimeExcluded" -Resolve) .
	}
  }

  Write-Host "> Finished publishing executables for all runtimes"
}

function Invoke-FastpackBuild {
  param (
      [Parameter(Mandatory=$false)]
      [string]
      $ProjectFilePath = (Get-FastPackProjectFilePath),

      [Parameter(Mandatory=$true)]
      [string]
      $Configuration,

      [Parameter(Mandatory=$true)]
      [string]
      $OutputDirectory,

      [Parameter(Mandatory=$true)]
      [string]
      $Verbosity,

      [Parameter(Mandatory=$true)]
      [string]
      $RuntimeIdentifier,

      [Parameter(Mandatory=$true)]
      [bool]
      $IncludeRuntime,

      [Parameter(Mandatory=$true)]
      [bool]
      $TrimmExecutable,

      [Parameter(Mandatory=$true)]
      [bool]
      $AuthenticodeSign
  )

    Write-Host "> Publishing: Runtime=$($_), Config=$Configuration, self-contained=$IncludeRuntime, PublishTrimmed=$($TrimmExecutable -and $IncludeRuntime), IncludeNativeLibrariesForSelfExtract=$IncludeRuntime, OutputFolder=$OutputDirectory"
    & dotnet @("publish", "-r", "$RuntimeIdentifier", "-c", "$Configuration", "-o", "`"$OutputDirectory`"", "--verbosity", "$Verbosity", "--self-contained", "$IncludeRuntime", "-p:PublishTrimmed=$($TrimmExecutable -and $IncludeRuntime)", "-p:IncludeNativeLibrariesForSelfExtract=$($IncludeRuntime)", "`"$ProjectFilePath`"", "-p:EnableCompressionInSingleFile=$IncludeRuntime")

    if ($LASTEXITCODE -ne 0 ) {
        throw "Compile failed with exit code $LASTEXITCODE"
    }
}

export-modulemember -function Publish-NugetPackage, Publish-GlobalTool, Publish-Executables