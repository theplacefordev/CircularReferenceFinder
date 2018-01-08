[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string] $NuGet,

    [string] $Configuration = $null,

    [string] $Platform = $null,

    [string] $RevisionNumber = $null
)

use 14.0 MSBuild

If ([String]::IsNullOrEmpty($Configuration)) {
    $Configuration = "Release"
}
If ([String]::IsNullOrEmpty($Platform)) {
    $Platform = "Any CPU"
}

$SolutionFiles = Get-ChildItem -Path $BuildRoot -Filter *.sln
$SourcesDir = "$BuildRoot\src"
$ToolsDir = "$BuildRoot\tools"
$PackagesDir = "$BuildRoot\packages"
$OutputDir = "$BuildRoot\.build"
$ArtifactsDir = "$BuildRoot\.artifacts"
$NugetPackagesDir = "$BuildRoot\.packages"
$LogsDir = "$BuildRoot\.logs"

$NUnit = "$packagesDir\NUnit.ConsoleRunner\tools\nunit3-console.exe"
$Git = ($Env:GitPath, "git" -ne $null)[0]
If ([String]::IsNullOrEmpty($RevisionNumber)) {
    $RevisionNumber = exec { & $Git rev-list --count --first-parent HEAD }
}

# Synopsis: Default task
task . Test

# Synopsis: Checks environment
task Check-Environment `
{
    assert (Test-Path $SolutionFiles) "No .sln files found '$SolutionFiles'"
    assert (Test-Path $SourcesDir) "Sources folder not found '$SourcesDir'"
}

# Synopsis: Clean up build junk and initialize temp directories
task Clean `
    Check-Environment, `
{
    Remove-Directory $OutputDir
    New-Directory $OutputDir

    Remove-Directory $ArtifactsDir
    New-Directory $ArtifactsDir

    Remove-Directory $NugetPackagesDir
    New-Directory $NugetPackagesDir

    New-Directory $LogsDir

    Get-ChildItem $SourcesDir -Include bin,obj -Recurse -Directory | ForEach-Object { Remove-Directory $_.FullName }
}

# Synopsis: Restore NuGet packages
task Restore-Packages `
{
    exec { & $NuGet restore -PackagesDirectory $PackagesDir }
}

# Synopsis: Build them... Build them all
task Build `
    Clean, Restore-Packages, `
{
    Foreach ($solutionFile in $SolutionFiles) {
        $file = $solutionFile.FullName
        Write-Build Green "Building '$file' ..."
        exec { msbuild $file /verbosity:quiet /clp:ErrorsOnly /p:Configuration=$Configuration /p:Platform=$Platform /p:VersionPart=$RevisionNumber /p:OutDir=$OutputDir }
    }
}

# Synopsis: Run all tests
task Test `
    Build, `
{
    If (-not (Test-Path $NUnit)) {
        exec { & $NuGet install NUnit.ConsoleRunner -Version "3.7.0" -ExcludeVersion -OutputDirectory $PackagesDir }
    }
    $assemblies = Get-ChildItem -Path $OutputDir *.Tests.dll | ForEach-Object { $PSItem.FullName }
    assert ($assemblies.Count -gt 0) "No *.Tests.dll files found in '$OutputDir'"
    $testResultsDir = "$ArtifactsDir\NUnit"
    New-Directory $testResultsDir
    Foreach ($assembly in $assemblies) {
        $assemblyFilename = [System.IO.Path]::GetFileNameWithoutExtension($assembly)
        $testResultsXml = "$testResultsDir\$assemblyFilename.Results.xml"
        exec { & $NUnit "$assembly" /result="$testResultsXml" }
    }
}

# Synopsis: Create NuGet packages
task Package `
    Build, `
{
    $nuspecFiles = Get-ChildItem -Path $SourcesDir -Recurse *.nuspec | ForEach-Object { $PSItem.FullName }
    assert ($nuspecFiles.Count -gt 0) "No .nuspec files found in '$SourcesDir' and it subdirectories"
    # Prepare global release notes
    $releaseNotes = (exec { & $Git log -1 --format="%B" }) -Join "`n"
    #$releaseNotesText
    Foreach ($nuspecFile in $nuspecFiles) {
        Write-Output("Processing file: $nuspecFile")
        $csprojFile = [System.IO.Path]::ChangeExtension($nuspecFile, '.csproj')
        assert (Test-Path $csprojFile) "Corresponding .csproj file not found '$csprojFile'"
        $addNugetOptions = "-Symbols"
        # get original assembly version
        [xml] $projectXml = Get-Content $csprojFile
        $assemblyFile = "$OutputDir\$($projectXml.Project.PropertyGroup[0].AssemblyName.Trim()).dll"
        If (-not (Test-Path $assemblyFile)) {
            $assemblyFile = [System.IO.Path]::ChangeExtension($assemblyFile, '.exe')
            $addNugetOptions = "-Tool"
        }
        $assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($assemblyFile).Version.ToString()
        # HACK: it seems weird, but spaces even in a quoted variable breaks nuget CLI argument parser
        $releaseNotesText = $releaseNotes
        # Another problem caused by nuget argument parser, platform name should not contain any spaces e.g. Any CPU -> AnyCPU
        $platform = $Platform.Replace(" ", "") 
        $properties = "Configuration=$Configuration;Platform=$platform;VersionPart=$RevisionNumber;releaseNotes=<![CDATA[$releaseNotesText]]>;"
        exec { & $NuGet pack $csprojFile -IncludeReferencedProjects `
                                         -Version $assemblyVersion `
                                         -Build `
                                         -MSBuildVersion 14 `
                                         $addNugetOptions `
                                         -OutputDirectory $NugetPackagesDir `
                                         -Properties $properties }
    }
}

# Synopsis: Build, test and package
task All `
    Test, Package, `
{
}
