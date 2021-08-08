function New-MapToolArchive
{
    param
    (
        [Parameter(Mandatory)]
        [string]
        $ArchiveName
    )

    $archiveOutput = ".\bin\publish\" + $ArchiveName
    Get-ChildItem -Path ".\bin\publish\raw" | Compress-Archive -DestinationPath $archiveOutput -Force
    if (-not $?)
    {
        Write-Error "Failed to zip publish files."
        exit
    }
}

$currDir = Get-location
Set-Location ".\PS2MapTool.Cli"

dotnet publish -o ".\bin\publish\raw" --no-self-contained -r win-x64 -p:PublishSingleFile=true -c Release
if (-not $?)
{
    Write-Error "Failed to publish framework-dependent executable."
    exit
}

New-MapToolArchive "PS2MapTool_Framework-Dependent.zip"

dotnet publish -o ".\bin\publish\raw" --self-contained -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -c Release
if (-not $?)
{
    Write-Error "Failed to publish self-contained executable."
    exit
}

New-MapToolArchive "PS2MapTool_Self-Contained.zip"

Set-Location $currDir
