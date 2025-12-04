<#
.SYNOPSIS
    編譯 e-service 專案並準備部署檔案
    Build e-service project and prepare deployment files

.DESCRIPTION
    此腳本執行以下操作：
    1. 還原 NuGet 套件
    2. 編譯 Release 版本
    3. 發佈到指定輸出目錄

.PARAMETER OutputPath
    Build 輸出目錄

.PARAMETER Configuration
    編譯組態 (預設: Release)

.EXAMPLE
    .\build-release.ps1 -OutputPath "C:\Build\e-service"

.NOTES
    版本: 1.0
    建立日期: 2025-12-04
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[SUCCESS] $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

# 專案路徑
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$solutionPath = Join-Path $projectRoot "e-service\ES.sln"
$projectPath = Join-Path $projectRoot "e-service\ES\ES.csproj"

Write-Info "=========================================="
Write-Info "開始編譯 e-service 專案"
Write-Info "時間: $(Get-Date)"
Write-Info "=========================================="

# 檢查專案檔案
if (!(Test-Path $projectPath)) {
    Write-Error "找不到專案檔案: $projectPath"
    exit 1
}

# 建立輸出目錄
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

try {
    # Step 1: 還原 NuGet 套件
    Write-Info "Step 1: 還原 NuGet 套件..."
    $nugetPath = Get-Command nuget -ErrorAction SilentlyContinue
    if ($nugetPath) {
        & nuget restore $solutionPath
    } else {
        Write-Info "  使用 dotnet restore..."
        & dotnet restore $solutionPath
    }
    Write-Success "NuGet 套件還原完成"

    # Step 2: 編譯專案
    Write-Info "Step 2: 編譯專案 ($Configuration)..."
    $msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
    
    if (!$msbuildPath) {
        # 嘗試使用 .NET Framework MSBuild
        $msbuildPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
    }
    
    if (!(Test-Path $msbuildPath)) {
        Write-Error "找不到 MSBuild，請確認已安裝 Visual Studio 或 .NET Framework SDK"
        exit 1
    }

    Write-Info "  使用 MSBuild: $msbuildPath"
    & $msbuildPath $projectPath /p:Configuration=$Configuration /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:publishUrl=$OutputPath /p:DeleteExistingFiles=True /v:minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "編譯失敗"
        exit 1
    }
    Write-Success "編譯完成"

    # Step 3: 驗證輸出
    Write-Info "Step 3: 驗證輸出檔案..."
    $requiredFiles = @(
        "bin\ES.dll",
        "Views\Login\New.cshtml",
        "Views\Login\Edit1.cshtml"
    )

    $allFilesExist = $true
    foreach ($file in $requiredFiles) {
        $fullPath = Join-Path $OutputPath $file
        if (Test-Path $fullPath) {
            Write-Info "  ✓ $file"
        } else {
            Write-Warning "  ✗ $file (未找到)"
            $allFilesExist = $false
        }
    }

    if ($allFilesExist) {
        Write-Success "所有必要檔案已產生"
    } else {
        Write-Warning "部分檔案未找到，請檢查編譯輸出"
    }

    Write-Info "=========================================="
    Write-Success "Build 完成！"
    Write-Info "輸出目錄: $OutputPath"
    Write-Info "完成時間: $(Get-Date)"
    Write-Info "=========================================="
    Write-Info ""
    Write-Info "下一步: 執行部署腳本"
    Write-Info ".\deploy-user-registration-fix.ps1 -SourcePath '$OutputPath' -TargetPath '<正式環境路徑>'"
}
catch {
    Write-Error "編譯過程發生錯誤: $_"
    exit 1
}

