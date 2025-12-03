# PowerShell script to update all .md file footers
# This script updates the footer section of all .md files to have a consistent 3-line format
# 使用 UTF8 無 BOM 編碼

param(
    [string]$TargetDir = ""
)

if ($TargetDir -eq "") {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
} else {
    $scriptDir = $TargetDir
}

# Get all .md files recursively
$mdFiles = Get-ChildItem -Path $scriptDir -Filter "*.md" -Recurse | Where-Object { $_.Name -ne "update_footers.ps1" }

Write-Host "Target directory: $scriptDir"
Write-Host "Found $($mdFiles.Count) .md files to process"
Write-Host ""

foreach ($file in $mdFiles) {
    $relativePath = $file.FullName.Replace($scriptDir, "").TrimStart("\")
    Write-Host "Processing: $relativePath"

    try {
        # Read the file content with UTF8 encoding (no BOM)
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.UTF8Encoding]::new($false))

        # Find all occurrences of "---" line
        $lines = $content -split "`r`n|`n"
        $lastDashIndex = -1

        # Find the last "---" line
        for ($i = $lines.Count - 1; $i -ge 0; $i--) {
            if ($lines[$i] -match '^---\s*$') {
                $lastDashIndex = $i
                break
            }
        }

        # If found, keep only content before the last "---"
        if ($lastDashIndex -gt 0) {
            $lines = $lines[0..($lastDashIndex - 1)]
        }

        # Remove trailing empty lines
        while ($lines.Count -gt 0 -and $lines[$lines.Count - 1] -match '^\s*$') {
            $lines = $lines[0..($lines.Count - 2)]
        }

        # Join lines
        $content = $lines -join "`r`n"

        # Add the new footer with correct encoding
        $footer = "`r`n`r`n---`r`n`r`n**版本：** 1.0`r`n**日期：** 2025-10-20`r`n**作者：** 柏通股份有限公司`r`n"

        $content = $content + $footer

        # Write back to file with UTF8 encoding (no BOM)
        $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
        [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)

        Write-Host "  ✓ Updated successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Error: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "All files processed!" -ForegroundColor Cyan

