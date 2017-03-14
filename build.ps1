<#
.SYNOPSIS
Custom Powershell script to bootstrap a Cake build.
.DESCRIPTION
Script restores helper packages for build pipeline and starts build.cake script.
.PARAMETER target
Build target to run.
.PARAMETER configuration
Configuration to use.
.PARAMETER verbosity
Specifies the amount of information to be displayed.
.PARAMETER scriptArgs
Remaining arguments are added here.
#>

[CmdletBinding()]
Param(
    [ValidateSet("Default", "Coverity")]
    [string]$target = "Default",

    [ValidateSet("Release", "Debug")]
    [string]$configuration = "Release",

    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$verbosity = "Verbose",

    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$scriptArgs
)

$solutionRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

###########################################################################
# Prepare Cake and helper tools
###########################################################################

$buildPath = Join-Path $solutionRoot "build"
$toolsPath = Join-Path $solutionRoot "tools"

$toolsProjectJson = Join-Path $toolsPath "project.json"
$toolsProjectJsonSource = Join-Path $buildPath "project.json"

$cakeFeed = "https://api.nuget.org/v3/index.json"

# make sure tools folder exists
if (!(Test-Path $toolsPath))
{
    Write-Verbose -Message "Creating tools directory"
    New-Item -Path $toolsPath -Type directory | Out-Null
}

# project.json defines packages used in build process
Copy-Item $toolsProjectJsonSource $toolsProjectJson -ErrorAction Stop

Write-Host "Restoring build tools (into $toolsPath)"
Invoke-Expression "& dotnet restore `"$toolsPath`" --packages `"$toolsPath`" -f `"$cakeFeed`"" | Out-Null;
if ($LastExitCode -ne 0)
{
    throw "Error occurred while restoring build tools"
}

$cakeExe = (Get-ChildItem (Join-Path $toolsPath "Cake.CoreCLR/*/Cake.dll") -ErrorAction Stop).FullName | `
            Sort-Object $_ | `
            Select-Object -Last 1

# NuGet client is used only for uploading packages to MyGet and NuGet repos
$NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$NugetPath = Join-Path $toolsPath "nuget.exe"

if (!(Test-Path $NugetPath)) {
    Write-Host "Downloading Nuget client"
    (New-Object System.Net.WebClient).DownloadFile($NugetUrl, $NugetPath);
}

# prepare Sharpbrake.Build dll
$sharpbrakeBuildPath = Join-Path $buildPath "Sharpbrake.Build"

Invoke-Expression "& dotnet restore `"$sharpbrakeBuildPath`"" | Out-Null;
if ($LastExitCode -ne 0)
{
    throw "Error occurred while restoring Sharpbrake.Build packages"
}

Invoke-Expression "& dotnet build `"$sharpbrakeBuildPath`" --configuration $configuration" | Out-Null;
if ($LastExitCode -ne 0)
{
    throw "Error occurred while building Sharpbrake.Build project"
}

$sharpbrakeBuildDll = (Get-ChildItem (Join-Path $sharpbrakeBuildPath "bin/$configuration/*/Sharpbrake.Build.dll") -ErrorAction Stop).FullName | Select-Object -Last 1

Copy-Item $sharpbrakeBuildDll $toolsPath -ErrorAction Stop

###########################################################################
# Run build script
###########################################################################

$arguments = @{
    target=$target;
    configuration=$configuration;
    verbosity=$verbosity;
}.GetEnumerator() | %{"--{0}=`"{1}`"" -f $_.key, $_.value };

# Start Cake
Invoke-Expression "& dotnet `"$cakeExe`" `"build.cake`" $arguments $scriptArgs";
exit $LastExitCode
