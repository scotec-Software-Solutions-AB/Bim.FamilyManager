#Requires -Version 5.1
param([string]$PkgVersion = "0.9.0-local")

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir        = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot         = Split-Path -Parent $scriptDir
$addinProject     = Join-Path $repoRoot "Source\Bim.FamilyManager\Bim.FamilyManager.csproj"
$installerProject = Join-Path $repoRoot "Installation\Bim.FamilyManager.Installer\Bim.FamilyManager.Installer.csproj"
$installerExe     = Join-Path $repoRoot "Installation\Bim.FamilyManager.Installer\bin\Release\Bim.FamilyManager.Installer.exe"

$msiVersionRaw = ($PkgVersion -split "[-+]")[0]
$parts = $msiVersionRaw -split "\."
if ([int]$parts[0] -gt 256) { $parts[0] = [int]$parts[0] % 100 }
$msiVersion = $parts -join "."

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  BIM.FamilyManager Installer Builder  " -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  PkgVersion : $PkgVersion"
Write-Host "  MsiVersion : $msiVersion"
Write-Host "  Repo root  : $repoRoot"
Write-Host ""

# Step 1 - Publish the add-in for each Revit version
Write-Host "--- Step 1: Publishing add-in ---" -ForegroundColor Cyan
$env:PkgVersion = $PkgVersion

foreach ($year in @("2025", "2026", "2027")) {
    Write-Host "  Publishing for Revit $year ..." -ForegroundColor Yellow
    $defineConstants = "REVIT$year"
    dotnet publish $addinProject `
        --configuration Release `
        -p:PublishProfile=Properties/PublishProfiles/FolderProfile.pubxml `
        -p:Platform=x64 `
        -p:RevitYear=$year `
        -p:DefineConstants=$defineConstants `
        -p:Version=$PkgVersion `
        -p:PackageVersion=$PkgVersion
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for Revit $year (exit code $LASTEXITCODE)." }
    $publishedDir = Join-Path $repoRoot "Publish\$year\Bim.FamilyManager"
    if (-not (Test-Path $publishedDir)) { throw "Publish output not found at: $publishedDir" }
    $dllCount = (Get-ChildItem $publishedDir -Filter "*.dll").Count
    Write-Host "    OK - $dllCount DLLs in $publishedDir" -ForegroundColor Green
}

# Step 2 - Build the installer project
Write-Host ""
Write-Host "--- Step 2: Building installer project ---" -ForegroundColor Cyan
dotnet build $installerProject --configuration Release -p:Version=$msiVersion
if ($LASTEXITCODE -ne 0) { throw "Installer project build failed (exit code $LASTEXITCODE)." }
Write-Host "  OK" -ForegroundColor Green

# Step 3 - Install WiX if not already installed
Write-Host ""
Write-Host "--- Step 3: Checking WiX installation ---" -ForegroundColor Cyan
$wixInstalled = $false
try {
    $null = & wix --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  WiX already installed" -ForegroundColor Green
        $wixInstalled = $true
    }
} catch {
    $wixInstalled = $false
}
if (-not $wixInstalled) {
    Write-Host "  Installing WiX 7.0.0-rc.1 ..." -ForegroundColor Yellow
    dotnet tool install --global wix --version 7.0.0-rc.1
    if ($LASTEXITCODE -ne 0) { throw "WiX installation failed." }
    wix eula accept wix7
    if ($LASTEXITCODE -ne 0) { throw "WiX EULA acceptance failed." }
    Write-Host "  OK" -ForegroundColor Green
}

# Step 4 - Run the WixSharp executable to produce the .msi
# $ErrorActionPreference is temporarily set to Continue so that stderr output from
# the native exe does not trigger a PowerShell terminating error. The exit code is
# used instead to detect failures.
Write-Host ""
Write-Host "--- Step 4: Building MSI ---" -ForegroundColor Cyan
if (-not (Test-Path $installerExe)) { throw "Installer executable not found: $installerExe" }
$env:PkgVersion   = $PkgVersion
$env:RevitVersion = ""
$installerDir = Split-Path $installerExe -Parent
Push-Location $installerDir
try {
    $ErrorActionPreference = "Continue"
    & $installerExe
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = "Stop"
} finally {
    Pop-Location
}
if ($exitCode -ne 0) { throw "MSI build failed (exit code $exitCode). Check output above for details." }

$msi = Get-ChildItem (Join-Path $installerDir "BIM.FamilyManager*.msi") | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -eq $msi) { throw "No .msi file found in: $installerDir" }

$newMsiName = "BIM.FamilyManager_$PkgVersion.msi"
$newMsiPath = Join-Path $installerDir $newMsiName
if ($msi.Name -ne $newMsiName) { Move-Item $msi.FullName $newMsiPath -Force }

Write-Host ""
Write-Host "=======================================" -ForegroundColor Green
Write-Host "  MSI ready:" -ForegroundColor Green
Write-Host "  $newMsiPath" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host ""
Write-Host "To install:"
Write-Host "  msiexec /i `"$newMsiPath`" /l*v install.log"
msiexec /i "$newMsiPath" /l*v install.log
