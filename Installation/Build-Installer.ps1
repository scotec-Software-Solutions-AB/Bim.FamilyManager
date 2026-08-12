#Requires -Version 5.1
param(
    [string]$PkgVersion = "0.9.0-local",
    [switch]$Install = $True
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir        = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot         = Split-Path -Parent $scriptDir
$addinProject     = Join-Path $repoRoot "Source\Bim.FamilyManager\Bim.FamilyManager.csproj"
$installerProject = Join-Path $repoRoot "Installation\Bim.FamilyManager.Installer\Bim.FamilyManager.Installer.csproj"
$installerDir     = Join-Path $repoRoot "Installation\Bim.FamilyManager.Installer\bin\x64\Release"
$installerExe     = Join-Path $installerDir "Bim.FamilyManager.Installer.exe"
$publishRoot      = Join-Path $repoRoot "Publish"
$requiredWixVersion = "6.0.2"

$msiVersionRaw = ($PkgVersion -split "[-+]")[0]
$parts = $msiVersionRaw -split "\."
if ($parts.Count -ne 3) {
    throw "Invalid PkgVersion '$PkgVersion'. Expected semantic version Major.Minor.Patch."
}

$parsedParts = @()
foreach ($part in $parts) {
    $value = 0
    if (-not [int]::TryParse($part, [ref]$value) -or $value -lt 0) {
        throw "Invalid PkgVersion '$PkgVersion'. Version components must be non-negative integers."
    }
    $parsedParts += $value
}

if ($parsedParts[0] -ge 256) { $parsedParts[0] = $parsedParts[0] % 100 }
if ($parsedParts[1] -gt 255 -or $parsedParts[2] -gt 65535) {
    throw "Invalid PkgVersion '$PkgVersion'. MSI minor must be <= 255 and build must be <= 65535."
}
$parts = $parsedParts
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
    $addinManifest = Join-Path $repoRoot "Publish\$year\Bim.FamilyManager.addin"
    $addinAssembly = Join-Path $publishedDir "Bim.FamilyManager.dll"
    if (-not (Test-Path $addinManifest -PathType Leaf)) { throw "Add-in manifest not found at: $addinManifest" }
    if (-not (Test-Path $addinAssembly -PathType Leaf)) { throw "Add-in assembly not found at: $addinAssembly" }
    $dllCount = (Get-ChildItem $publishedDir -Filter "*.dll").Count
    Write-Host "    OK - $dllCount DLLs in $publishedDir" -ForegroundColor Green
}

# Step 2 - Build the installer project
Write-Host ""
Write-Host "--- Step 2: Building installer project ---" -ForegroundColor Cyan
dotnet build $installerProject --configuration Release -p:Platform=x64 -p:Version=$msiVersion
if ($LASTEXITCODE -ne 0) { throw "Installer project build failed (exit code $LASTEXITCODE)." }
Write-Host "  OK" -ForegroundColor Green

# Step 3 - Install WiX if not already installed
Write-Host ""
Write-Host "--- Step 3: Checking WiX installation ---" -ForegroundColor Cyan
$wixInstalled = $false
try {
    $wixVersionOutput = & wix --version 2>&1
    $wixVersionExitCode = $LASTEXITCODE
    $installedWixVersion = (($wixVersionOutput | Select-Object -First 1) -replace '\+.*$', '').Trim()
    if ($wixVersionExitCode -eq 0 -and $installedWixVersion -eq $requiredWixVersion) {
        Write-Host "  WiX $requiredWixVersion already installed" -ForegroundColor Green
        $wixInstalled = $true
    } elseif ($wixVersionExitCode -eq 0) {
        throw "WiX $requiredWixVersion is required, but $installedWixVersion is active."
    }
} catch {
    if ($_.Exception.Message -like "WiX $requiredWixVersion is required*") { throw }
}
if (-not $wixInstalled) {
    Write-Host "  Installing WiX $requiredWixVersion ..." -ForegroundColor Yellow
    dotnet tool install --global wix --version $requiredWixVersion
    if ($LASTEXITCODE -ne 0) { throw "WiX installation failed." }
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
$env:BIM_FAMILYMANAGER_PUBLISH_ROOT = $publishRoot
$newMsiName = "BIM.FamilyManager_$PkgVersion.msi"
$newMsiPath = Join-Path $installerDir $newMsiName
$env:BIM_FAMILYMANAGER_MSI_PATH = $newMsiPath
if (Test-Path $newMsiPath) {
    try {
        Remove-Item $newMsiPath -Force
    } catch {
        throw "Cannot replace existing MSI because it is in use: $newMsiPath. Close Windows Installer tools or processes using the file and retry."
    }
}
Push-Location $installerDir
try {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    & $installerExe
    $exitCode = $LASTEXITCODE
} finally {
    $ErrorActionPreference = $previousErrorActionPreference
    Pop-Location
}
if ($exitCode -ne 0) { throw "MSI build failed (exit code $exitCode). Check output above for details." }

if (-not (Test-Path $newMsiPath -PathType Leaf)) { throw "MSI not found at expected path: $newMsiPath" }

Write-Host ""
Write-Host "=======================================" -ForegroundColor Green
Write-Host "  MSI ready:" -ForegroundColor Green
Write-Host "  $newMsiPath" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host ""
Write-Host "To install:"
Write-Host "  msiexec /i `"$newMsiPath`" /l*v install.log"
if ($Install) {
    $installLog = Join-Path $installerDir "install.log"
    $process = Start-Process msiexec.exe -ArgumentList @('/i', "`"$newMsiPath`"", '/l*v', "`"$installLog`"") -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Installation failed (exit code $($process.ExitCode)). See: $installLog" }
}
