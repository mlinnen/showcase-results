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

# Create zip package
Compress-Archive -Path $componentPath -DestinationPath $outputZip -Force

if (Test-Path $outputZip) {
    $zipInfo = Get-Item $outputZip
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Output: $($zipInfo.FullName)" -ForegroundColor Green
    Write-Host "Size: $([math]::Round($zipInfo.Length / 1KB, 2)) KB" -ForegroundColor Green
} else {
    Write-Host "`nBuild failed - zip file not created" -ForegroundColor Red
    exit 1
}
