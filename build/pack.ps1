Param([string]$version = "", [string] $configuration = "Release")
$ErrorActionPreference = "Stop"

# Set location to the Solution directory
(Get-Item $PSScriptRoot).Parent.FullName | Push-Location

Import-Module .\build\exechelper.ps1

# Install .NET tooling
exec .\build\dotnet-cli-install.ps1

if($version -eq "")
{
    $version = . .\build\get-version.ps1
    $version = "$version-developerbuild"
}

[xml] $versionFile = Get-Content ".\src\Optimizely.CMS.MassTransit.Events\Optimizely.CMS.MassTransit.Events.csproj"
$node = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='EPiServer.CMS.AspNetCore']")
$cmsVersion = $node.Attributes["Version"].Value
$parts = $cmsVersion.Split(".")
$major = [int]::Parse($parts[0]) + 1
$cmsNextMajorVersion = ($major.ToString() + ".0.0") 

# Packaging public packages
exec "dotnet" "pack --no-restore --no-build -c $configuration /p:PackageVersion=$version /p:CmsVersion=$cmsVersion /p:CmsNextMajorVersion=$cmsNextMajorVersion Optimizely.CMS.MassTransit.Events.sln"

Pop-Location