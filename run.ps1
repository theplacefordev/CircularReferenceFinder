#Requires -Version 4.0
# globally suppress progress bar for interactive cmdlets
$global:ProgressPreference = "SilentlyContinue"

Function New-Directory {
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param([string] $path)

    Write-Verbose "Creating directory $path"
    
    If ($PSCmdlet.ShouldProcess($path)) {
        New-Item -ItemType Directory $path -ErrorAction SilentlyContinue | Out-Null
    }
}

Function Remove-Contents {
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param([string] $path)

    Write-Verbose "Removing contents of $path"

    Get-ChildItem -Recurse -Path $path | %{ 
        if ($PSCmdlet.ShouldProcess($path)) {
            Remove-Item $PSItem.FullName -Force -Recurse -ErrorAction SilentlyContinue | Out-Null
        } 
    } 
}

Function Remove-Directory {
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param([string] $path)

    Write-Verbose "Removing directory $path"

    If ($PSCmdlet.ShouldProcess($path)) {
        Remove-Item -Path $path -Force -Recurse -ErrorAction SilentlyContinue | Out-Null
    }
}

$SourcesPath = $Env:SourcesPath
If ($SourcesPath -eq $null) {
    $SourcesPath = $(Split-Path -Parent $MyInvocation.MyCommand.path)
}

$NuGet = $Env:NuGet
If (($NuGet -eq $null)) {
    $NuGet = "$SourcesPath\.nuget\nuget.exe"
    If (-not (Test-Path $NuGet)) {
        Write-Host "Downloading NuGet command-line tool ..."
        New-Directory(Split-Path -Path $NuGet)
        Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $NuGet
    }
}
$PackagesDir = "$SourcesPath\packages"
$InvokeBuildScript = "$PackagesDir\Invoke-Build\tools\Invoke-Build.ps1"

# Install Invoke-Build from nuget
If (-not (Test-Path $InvokeBuildScript)) {
    Write-Host "Installing Invoke-Build scripts ..."
    & $NuGet install Invoke-Build -ExcludeVersion -OutputDirectory $PackagesDir
    If ($LastExitCode -ne 0) {
        Write-Error "NuGet returned error exit code: $LastExitCode"
        Exit 1;
    }
}

. $InvokeBuildScript
Invoke-Expression "Invoke-Build $args -Safe -Result Result -NuGet $NuGet"

If ($Result.Error) {
    Write-Error("Invoke-Build script failed with error: {0}" -f $Result.Error)
    Exit 2
}

