# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$env:APPVEYOR_REPO_BRANCH -ne $NULL];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "master" -and $revision -ne "local"]
$commitHash = $(git rev-parse --short HEAD)
$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]
$versionSuffix = @{ $true = "--version-suffix=$($suffix)"; $false = ""}[$suffix -ne ""]

echo "build: Package version suffix is $suffix"
echo "build: Build version suffix is $buildSuffix" 

exec { & dotnet build -c Release --version-suffix=$buildSuffix }

Push-Location -Path .\AutoMapper.OData.EFCore.Tests

try {
    exec { & dotnet test -c Release --no-build --no-restore }
} finally {
    Pop-Location
}

exec { & dotnet pack .\AutoMapper.AspNetCore.OData.EFCore\AutoMapper.AspNetCore.OData.EFCore.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --no-restore $versionSuffix }
exec { & dotnet pack .\AutoMapper.AspNetCore.OData.EF6\AutoMapper.AspNetCore.OData.EF6.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --no-restore $versionSuffix }
exec { & dotnet pack .\AutoMapper.AspNet.OData.EF6\AutoMapper.AspNet.OData.EF6.csproj -c Release -o ..\..\artifacts --include-symbols --no-build --no-restore $versionSuffix }

