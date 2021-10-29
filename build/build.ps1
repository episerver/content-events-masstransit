# NOTE: This script must currently be executed from the solution dir (due to specs)
Param([string] $configuration = "Release", [string] $logger="trx", [string] $verbosity="normal")
$ErrorActionPreference = "Stop"

# Set location to the Solution directory
(Get-Item $PSScriptRoot).Parent.FullName | Push-Location

Import-Module .\build\exechelper.ps1

# Install .NET tooling

exec .\build\dotnet-cli-install.ps1

# Build dotnet projects

exec "dotnet" "build -c $configuration"

# Run XUnit test projects

exec "dotnet"  "test Optimizely.CMS.MassTransit.Events.sln -l $logger -v $verbosity -c $configuration --blame-crash --no-build --no-restore"

Pop-Location