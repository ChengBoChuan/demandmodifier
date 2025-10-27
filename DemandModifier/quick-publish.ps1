# DemandModifier 快速發行腳本
# 使用方法：.\quick-publish.ps1 -Version "0.0.14" -ChangeLog "你的更新說明"

param(
    [Parameter(Mandatory=$false)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$ChangeLog,
    
    [switch]$SkipBuild,
    [switch]$SkipTest,
    [switch]$TestOnly
)

# 顏色輸出函式
function Write-Step {
    param([string]$Message)
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

# 檢查是否在正確的目錄
if (!(Test-Path "DemandModifier.csproj")) {
    Write-Error-Custom "請在 DemandModifier 專案目錄執行此腳本"
    exit 1
}

# 讀取目前版本
[xml]$pubConfig = Get-Content "Properties\PublishConfiguration.xml"
$currentVersion = $pubConfig.Publish.ModVersion.Value

Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║       DemandModifier 快速發行工具 v1.0                    ║
║       目前版本: $currentVersion                              ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

# 如果是測試模式
if ($TestOnly) {
    Write-Step "測試模式 - 僅建置和本機部署"
    
    # 建置
    Write-Step "清理舊建置產物"
    dotnet clean | Out-Null
    
    Write-Step "建置 Release 版本"
    dotnet build -c Release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "建置失敗"
        exit 1
    }
    
    Write-Success "建置成功"
    
    # 本機部署
    Write-Step "部署到遊戲 Mods 目錄"
    $modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
    
    if (Test-Path $modsPath) {
        Remove-Item $modsPath -Recurse -Force
    }
    
    New-Item -ItemType Directory -Force -Path $modsPath | Out-Null
    Copy-Item "bin\Release\net48\*" $modsPath -Recurse -Force
    
    Write-Success "已部署到 $modsPath"
    Write-Warning-Custom "請重新啟動 Cities: Skylines 2 測試模組"
    exit 0
}

# 互動式版本輸入
if ([string]::IsNullOrEmpty($Version)) {
    Write-Host "目前版本: $currentVersion" -ForegroundColor Yellow
    $Version = Read-Host "請輸入新版本號 (例如: 0.0.14)"
    
    if ([string]::IsNullOrEmpty($Version)) {
        Write-Error-Custom "必須提供版本號"
        exit 1
    }
}

# 驗證版本號格式
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error-Custom "版本號格式錯誤，必須是 X.Y.Z 格式 (例如: 0.0.14)"
    exit 1
}

# 互動式變更日誌輸入
if ([string]::IsNullOrEmpty($ChangeLog)) {
    Write-Host "`n請輸入變更日誌 (簡短說明):" -ForegroundColor Yellow
    $ChangeLog = Read-Host
    
    if ([string]::IsNullOrEmpty($ChangeLog)) {
        Write-Warning-Custom "未提供變更日誌，使用預設值"
        $ChangeLog = "Version $Version - 功能更新"
    }
}

# 顯示發行資訊
Write-Host @"

發行資訊：
-----------
版本號:    $currentVersion -> $Version
變更日誌:  $ChangeLog

"@ -ForegroundColor Yellow

$confirm = Read-Host "確認發行? (y/n)"
if ($confirm -ne 'y') {
    Write-Warning-Custom "已取消發行"
    exit 0
}

# 步驟 1：檢查必要檔案
Write-Step "步驟 1/6: 檢查必要檔案"

$requiredFiles = @(
    "DemandModifier.csproj",
    "DemandModifierMod.cs",
    "DemandModifierSettings.cs",
    "Properties\PublishConfiguration.xml",
    "l10n\en-US.json",
    "l10n\zh-HANT.json"
)

$missingFiles = @()
foreach ($file in $requiredFiles) {
    if (!(Test-Path $file)) {
        $missingFiles += $file
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Error-Custom "缺少必要檔案："
    $missingFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    exit 1
}

Write-Success "所有必要檔案已就緒"

# 檢查縮圖 (警告但不中斷)
if (!(Test-Path "Properties\Thumbnail.png")) {
    Write-Warning-Custom "未找到縮圖檔案 (Properties\Thumbnail.png)"
    Write-Host "  建議建立 256x256 的 PNG 縮圖以提升模組頁面品質" -ForegroundColor Gray
}

# 步驟 2：更新 PublishConfiguration.xml
Write-Step "步驟 2/6: 更新版本資訊"

try {
    [xml]$pubConfig = Get-Content "Properties\PublishConfiguration.xml"
    $pubConfig.Publish.ModVersion.Value = $Version
    $pubConfig.Publish.ChangeLog.Value = $ChangeLog
    $pubConfig.Save((Resolve-Path "Properties\PublishConfiguration.xml"))
    Write-Success "PublishConfiguration.xml 已更新"
} catch {
    Write-Error-Custom "更新 PublishConfiguration.xml 失敗: $_"
    exit 1
}

# 步驟 3：建置專案
if (!$SkipBuild) {
    Write-Step "步驟 3/6: 建置專案"
    
    Write-Host "  清理舊建置產物..." -ForegroundColor Gray
    dotnet clean | Out-Null
    
    Write-Host "  建置 Release 版本..." -ForegroundColor Gray
    dotnet build -c Release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "建置失敗，請檢查錯誤訊息"
        exit 1
    }
    
    Write-Success "建置成功"
} else {
    Write-Step "步驟 3/6: 跳過建置 (使用 -SkipBuild)"
}

# 步驟 4：驗證建置產物
Write-Step "步驟 4/6: 驗證建置產物"

$artifacts = @(
    "bin\Release\net48\DemandModifier.dll",
    "bin\Release\net48\l10n\en-US.json",
    "bin\Release\net48\l10n\zh-HANT.json"
)

$missingArtifacts = @()
foreach ($artifact in $artifacts) {
    if (!(Test-Path $artifact)) {
        $missingArtifacts += $artifact
    }
}

if ($missingArtifacts.Count -gt 0) {
    Write-Error-Custom "建置產物缺失："
    $missingArtifacts | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    exit 1
}

# 顯示檔案大小
$dllSize = (Get-Item "bin\Release\net48\DemandModifier.dll").Length / 1KB
$l10nCount = (Get-ChildItem "bin\Release\net48\l10n\*.json").Count
Write-Success "DLL: $([math]::Round($dllSize, 2)) KB, 語系檔案: $l10nCount 個"

# 步驟 5：本機測試 (可選)
if (!$SkipTest) {
    Write-Step "步驟 5/6: 本機測試部署"
    
    $testDeploy = Read-Host "是否部署到遊戲目錄進行測試? (y/n)"
    
    if ($testDeploy -eq 'y') {
        $modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
        
        if (Test-Path $modsPath) {
            Remove-Item $modsPath -Recurse -Force
        }
        
        New-Item -ItemType Directory -Force -Path $modsPath | Out-Null
        Copy-Item "bin\Release\net48\*" $modsPath -Recurse -Force
        
        Write-Success "已部署到 $modsPath"
        Write-Warning-Custom "請啟動遊戲測試後按任意鍵繼續..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
} else {
    Write-Step "步驟 5/6: 跳過測試 (使用 -SkipTest)"
}

# 步驟 6：發行到 Paradox Mods
Write-Step "步驟 6/6: 發行到 Paradox Mods"

Write-Host "  上傳中，請稍候..." -ForegroundColor Gray

try {
    dotnet publish /p:PublishProfile=PublishNewVersion 2>&1 | Tee-Object -Variable publishOutput
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "發行成功！"
        Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║                   🎉 發行完成！                            ║
╚═══════════════════════════════════════════════════════════╝

版本:     $Version
ModId:    123136
頁面:     https://mods.paradoxplaza.com/mods/123136/Windows

"@ -ForegroundColor Green
        
        Write-Host "後續步驟:" -ForegroundColor Yellow
        Write-Host "  1. 前往 Paradox Mods 頁面確認更新" -ForegroundColor Gray
        Write-Host "  2. 提交 Git: git add . && git commit -m 'Release v$Version'" -ForegroundColor Gray
        Write-Host "  3. 建立標籤: git tag v$Version && git push origin v$Version" -ForegroundColor Gray
        
    } else {
        Write-Error-Custom "發行失敗"
        Write-Host "`n錯誤輸出：" -ForegroundColor Red
        $publishOutput | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
        
        Write-Host "`n常見問題排查：" -ForegroundColor Yellow
        Write-Host "  1. 檢查 ChangeLog 是否為空" -ForegroundColor Gray
        Write-Host "  2. 檢查 Thumbnail.png 是否存在" -ForegroundColor Gray
        Write-Host "  3. 檢查網路連線" -ForegroundColor Gray
        Write-Host "  4. 查看完整指南: docs\版本發行完整指南.md" -ForegroundColor Gray
        
        exit 1
    }
} catch {
    Write-Error-Custom "發行過程發生錯誤: $_"
    exit 1
}
