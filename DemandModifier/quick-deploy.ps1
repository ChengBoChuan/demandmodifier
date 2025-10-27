# 快速建置並部署到遊戲的腳本
param([switch]$SkipBuild)

$ErrorActionPreference = "Stop"

Write-Host "`n=== Demand Modifier 快速部署 ===" -ForegroundColor Cyan

# 建置專案
if (-not $SkipBuild) {
    Write-Host "`n[1/3] 建置專案..." -ForegroundColor Yellow
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ 建置失敗！" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ 建置成功" -ForegroundColor Green
} else {
    Write-Host "`n[1/3] 跳過建置" -ForegroundColor Gray
}

# 複製到遊戲 Mods 資料夾
Write-Host "`n[2/3] 部署到遊戲..." -ForegroundColor Yellow
$modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"

# 確保目標資料夾存在
New-Item -ItemType Directory -Force -Path $modsPath | Out-Null

# 複製檔案
Copy-Item "bin\Release\net48\DemandModifier.dll" $modsPath -Force
Copy-Item "bin\Release\net48\l10n" $modsPath -Recurse -Force

Write-Host "✓ 已部署到: $modsPath" -ForegroundColor Green

# 顯示後續步驟
Write-Host "`n[3/3] 測試步驟:" -ForegroundColor Yellow
Write-Host "  1. 啟動 Cities: Skylines 2" -ForegroundColor White
Write-Host "  2. 進入 選項 → Mods → Demand Modifier" -ForegroundColor White
Write-Host "  3. 檢查 '一般設定' 分頁是否出現" -ForegroundColor White
Write-Host "  4. 測試 '介面語言' 下拉選單功能" -ForegroundColor White

Write-Host "`n✓ 部署完成！" -ForegroundColor Green
Write-Host "請重新啟動遊戲以載入更新的 Mod" -ForegroundColor Cyan
