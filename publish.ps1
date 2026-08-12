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

$currDir = Get-location
Set-Location ".\PS2MapTool.Cli"

dotnet publish -o ".\bin\Publish\raw" --self-contained -r win-x64 -p:PublishSingleFile=true -p:PublishAot=true -c Release
if (-not $?)
{
    Write-Error "Failed to publish self-contained executable."
    exit
}

New-MapToolArchive "PS2MapTool.zip"

Set-Location $currDir
