# Build script for Joomla com_showcaseresults component
# Creates installable .zip package

$ErrorActionPreference = "Stop"

$componentPath = "com_showcaseresults"
$outputZip = "com_showcaseresults.zip"

Write-Host "Building Joomla component package..." -ForegroundColor Cyan

# Remove existing zip if present
if (Test-Path $outputZip) {
    Remove-Item $outputZip -Force
    Write-Host "Removed existing $outputZip" -ForegroundColor Yellow
}

# Create zip package with forward-slash entry names (required for Joomla on Linux/PHP)
Add-Type -Assembly System.IO.Compression
Add-Type -Assembly System.IO.Compression.FileSystem
$fullComponentPath = (Resolve-Path $componentPath).Path
$fullOutputZip = Join-Path (Get-Location) $outputZip

$stream = [System.IO.File]::Open($fullOutputZip, [System.IO.FileMode]::Create)
$archive = New-Object System.IO.Compression.ZipArchive($stream, [System.IO.Compression.ZipArchiveMode]::Create)

Get-ChildItem -Path $fullComponentPath -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($fullComponentPath.Length).TrimStart('\', '/')
    $entryName = $relativePath.Replace('\', '/')   # ZIP spec requires forward slashes
    $entry = $archive.CreateEntry($entryName, [System.IO.Compression.CompressionLevel]::Optimal)
    $entryStream = $entry.Open()
    $fileStream = [System.IO.File]::OpenRead($_.FullName)
    $fileStream.CopyTo($entryStream)
    $fileStream.Dispose()
    $entryStream.Dispose()
}

$archive.Dispose()
$stream.Dispose()

if (Test-Path $outputZip) {
    $zipInfo = Get-Item $outputZip
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Output: $($zipInfo.FullName)" -ForegroundColor Green
    Write-Host "Size: $([math]::Round($zipInfo.Length / 1KB, 2)) KB" -ForegroundColor Green
} else {
    Write-Host "`nBuild failed - zip file not created" -ForegroundColor Red
    exit 1
}
