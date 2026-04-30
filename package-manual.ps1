$projectName = "grid-video-player"
$targetFramework = "net10.0-windows10.0.19041.0"
$runtime = "win-x64"
$publishDir = "grid-video-player\bin\Release\$targetFramework\$runtime\publish"
$makeAppxPath = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe"
$signtoolPath = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
$packageDir = "PackageBuild"
$outputMsix = "GridVideoPlayer.msix"
$publisher = "CN=2ABC7F34-7AF6-4C95-8D60-39CD8D415607"

Write-Host "--- 1/3 Publishing project (Multi-file) ---" -ForegroundColor Cyan
dotnet publish $projectName\$projectName.csproj -c Release -r $runtime --self-contained false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "`n--- 2/3 Preparing package folder ---" -ForegroundColor Cyan
if (Test-Path $packageDir) { Remove-Item -Recurse -Force $packageDir }
New-Item -ItemType Directory -Path $packageDir | Out-Null
Copy-Item -Path "$publishDir\*" -Destination $packageDir -Recurse

# --- FIX: Flatten ALL native DLLs from runtimes to root ---
Write-Host "Flattening all native libraries for maximum compatibility..." -ForegroundColor Yellow
Get-ChildItem -Path "$packageDir\runtimes\win-x64\native\*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $packageDir -Force
    Write-Host "  Copied: $($_.Name)"
}

if (Test-Path "$packageDir\$projectName.exe") {
    Move-Item -Path "$packageDir\$projectName.exe" -Destination "$packageDir\GridPlayer.exe"
}
Copy-Item -Path "package\Package.appxmanifest" -Destination "$packageDir\AppxManifest.xml"
Copy-Item -Path "package\Images" -Destination "$packageDir\Images" -Recurse

Get-ChildItem "$packageDir\Images\*.scale-200.png" | ForEach-Object {
    $newName = $_.Name -replace '\.scale-200', ''
    Move-Item $_.FullName (Join-Path $_.DirectoryName $newName) -Force
}

Write-Host "`n--- 3/3 Packing MSIX ---" -ForegroundColor Cyan
& $makeAppxPath pack /d $packageDir /p $outputMsix /o
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "`n--- 4/4 Signing MSIX ---" -ForegroundColor Cyan
$cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $publisher } | Select-Object -First 1
& $signtoolPath sign /fd SHA256 /a /sha1 $cert.Thumbprint $outputMsix

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSUCCESS! $outputMsix is ready." -ForegroundColor Green
    Write-Host "Re-installing package..." -ForegroundColor Cyan
    Add-AppxPackage -Path $outputMsix -ErrorAction SilentlyContinue
    if ($error[0].Exception.HRESULT -eq 0x80073B1F -or $error[0].Exception.HRESULT -eq 0x80073CFB) {
        Write-Host "Update failed (version/existing package issue). Uninstalling old version..." -ForegroundColor Yellow
        Get-AppxPackage "5A5F4AA2.GridVideoPlayer" | Remove-AppxPackage
        Add-AppxPackage -Path $outputMsix
    } else {
        Add-AppxPackage -Path $outputMsix
    }
}
