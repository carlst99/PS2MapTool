function New-MapToolArchive
{
    param
    (
        [Parameter(Mandatory)]
        [string]
        $ArchiveName
    )

    $archiveOutput = ".\bin\Publish\" + $ArchiveName
    Get-ChildItem -Path ".\bin\Publish\raw" | Compress-Archive -DestinationPath $archiveOutput -Force
    if (-not $?)
    {
        Write-Error "Failed to zip publish files."
        exit
    }
}

# This script is intended to be run from the solution root
# Save the current working directory and navigate into the CLI project
$currDir = Get-location
Set-Location ".\PS2MapTool.Cli"

# Remove existing publish output
Remove-Item ".\bin\Publish\raw\*"

# Publish a new single-file, self-contained binary for win-x64
dotnet publish -o ".\bin\Publish\raw" --self-contained -r win-x64 -p:PublishSingleFile=true -p:PublishAot=true -c Release
if (-not $?)
{
    Write-Error "Failed to publish self-contained executable."
    exit
}

# Compress all output files into a ZIP archive
New-MapToolArchive "PS2MapTool_win-x64.zip"

# Restore the original working directory
Set-Location $currDir
