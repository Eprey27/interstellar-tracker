# PowerShell Script to Extract Copilot Chat Session
# Run this script from PowerShell to attempt chat export

param(
    [string]$OutputDir = ".\.github\sessions\raw-data"
)

Write-Host "🔍 Attempting to export Copilot Chat history..." -ForegroundColor Cyan

# Method 1: Check VS Code storage locations
$storageLocations = @(
    "$env:APPDATA\Code\User\globalStorage\github.copilot-chat",
    "$env:APPDATA\Code\User\workspaceStorage",
    "$env:LOCALAPPDATA\Code\User\workspaceStorage"
)

$foundFiles = @()

foreach ($location in $storageLocations) {
    if (Test-Path $location) {
        Write-Host "📁 Searching in: $location" -ForegroundColor Yellow
        
        $files = Get-ChildItem -Path $location -Recurse -File -ErrorAction SilentlyContinue | 
        Where-Object { 
            $_.Name -match "chat|session|history|conversation" -or 
            $_.Extension -in @('.json', '.db', '.sqlite', '.log')
        } | 
        Where-Object { $_.LastWriteTime -gt (Get-Date).AddHours(-12) } # Last 12 hours
        
        foreach ($file in $files) {
            Write-Host "  ✓ Found: $($file.Name) ($(($file.Length/1KB).ToString('F2')) KB)" -ForegroundColor Green
            $foundFiles += $file
        }
    }
}

# Method 2: Check workspace-specific storage
$workspaceId = Get-Content "$env:APPDATA\Code\User\workspaceStorage\*\workspace.json" -ErrorAction SilentlyContinue | 
Select-String "interstellar-tracker" -Context 0, 5 | 
Select-Object -First 1

if ($workspaceId) {
    Write-Host "`n📌 Found workspace reference" -ForegroundColor Cyan
}

# Method 3: Copy found files to output directory
if ($foundFiles.Count -gt 0) {
    Write-Host "`n📦 Copying files to $OutputDir..." -ForegroundColor Cyan
    
    if (-not (Test-Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
    $exportDir = Join-Path $OutputDir "export-$timestamp"
    New-Item -ItemType Directory -Path $exportDir -Force | Out-Null
    
    foreach ($file in $foundFiles) {
        $destPath = Join-Path $exportDir $file.Name
        Copy-Item -Path $file.FullName -Destination $destPath -Force
        Write-Host "  ✓ Copied: $($file.Name)" -ForegroundColor Green
    }
    
    # Create index file
    $indexContent = @"
# Copilot Chat Export - $timestamp

## Files Exported

$($foundFiles | ForEach-Object { "- $($_.Name) - $($_.Length) bytes - Modified: $($_.LastWriteTime)" } | Out-String)

## Export Location
$exportDir

## Notes
These files may contain chat history in proprietary format.
Manual extraction may be required depending on format.
"@
    
    $indexPath = Join-Path $exportDir "README.md"
    Set-Content -Path $indexPath -Value $indexContent -Encoding UTF8
    
    Write-Host "`n✅ Export complete! Files saved to:" -ForegroundColor Green
    Write-Host "   $exportDir" -ForegroundColor White
    
    # Attempt to compress
    $zipPath = "$exportDir.zip"
    Compress-Archive -Path $exportDir -DestinationPath $zipPath -Force
    Write-Host "`n📦 Compressed to: $zipPath" -ForegroundColor Green
    
}
else {
    Write-Host "`n⚠️ No recent chat history files found." -ForegroundColor Yellow
    Write-Host "   Copilot may not persist chat history locally, or it's in an inaccessible location." -ForegroundColor Yellow
}

# Method 4: Instruction for manual export
Write-Host "`n📋 Manual Export Instructions:" -ForegroundColor Cyan
Write-Host "   1. In VS Code Chat panel, select all text (Ctrl+A)" -ForegroundColor White
Write-Host "   2. Copy (Ctrl+C)" -ForegroundColor White
Write-Host "   3. Run: Get-Clipboard | Out-File '$OutputDir\chat-manual-export.txt' -Encoding UTF8" -ForegroundColor White
Write-Host "   4. Or paste into .github\sessions\session-2025-11-23-tdd-integration.md" -ForegroundColor White

Write-Host "`n💡 Alternative: Use VS Code Developer Tools" -ForegroundColor Cyan
Write-Host "   Help → Toggle Developer Tools → Console → Run export script" -ForegroundColor White
