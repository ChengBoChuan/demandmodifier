# 縮圖快速檢查與解決腳本

param(
    [switch]$Create
)

Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║           DemandModifier 縮圖檢查工具                      ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

$thumbnailPath = "Properties\Thumbnail.png"

# 檢查縮圖是否存在
if (Test-Path $thumbnailPath) {
    Write-Host "✓ 縮圖檔案已存在: $thumbnailPath" -ForegroundColor Green
    
    # 取得檔案資訊
    $file = Get-Item $thumbnailPath
    $sizeKB = [math]::Round($file.Length / 1KB, 2)
    
    Write-Host "`n檔案資訊：" -ForegroundColor Yellow
    Write-Host "  大小: $sizeKB KB" -ForegroundColor Gray
    Write-Host "  建立時間: $($file.CreationTime)" -ForegroundColor Gray
    Write-Host "  修改時間: $($file.LastWriteTime)" -ForegroundColor Gray
    
    # 嘗試讀取圖片尺寸
    try {
        Add-Type -AssemblyName System.Drawing
        $img = [System.Drawing.Image]::FromFile((Resolve-Path $thumbnailPath))
        Write-Host "  尺寸: $($img.Width) x $($img.Height) 像素" -ForegroundColor Gray
        
        if ($img.Width -ne 256 -or $img.Height -ne 256) {
            Write-Host "`n⚠ 警告: 縮圖尺寸應為 256x256 像素" -ForegroundColor Yellow
            Write-Host "  目前尺寸: $($img.Width) x $($img.Height)" -ForegroundColor Red
        } else {
            Write-Host "`n✓ 縮圖尺寸正確 (256x256)" -ForegroundColor Green
        }
        
        $img.Dispose()
    } catch {
        Write-Host "`n⚠ 無法讀取圖片資訊: $_" -ForegroundColor Yellow
    }
    
} else {
    Write-Host "✗ 縮圖檔案不存在: $thumbnailPath" -ForegroundColor Red
    
    Write-Host @"

這會導致發行失敗！

解決方案：

1. 使用線上工具建立縮圖 (推薦)：
   • Canva: https://www.canva.com/
   • Photopea: https://www.photopea.com/
   
   尺寸要求: 256 x 256 像素
   格式: PNG
   
2. 使用現有圖片調整尺寸：
   • 使用 Windows 小畫家
   • 調整影像大小為 256x256
   • 另存為 PNG 格式到 Properties\Thumbnail.png

3. 暫時移除縮圖設定 (不推薦)：
   在 Properties\PublishConfiguration.xml 中註解掉：
   <!--<Thumbnail Value="Properties/Thumbnail.png" />-->

"@ -ForegroundColor Yellow

    if ($Create) {
        Write-Host "正在建立預設縮圖..." -ForegroundColor Cyan
        
        try {
            # 建立 Properties 目錄（如果不存在）
            if (!(Test-Path "Properties")) {
                New-Item -ItemType Directory -Force -Path "Properties" | Out-Null
            }
            
            Add-Type -AssemblyName System.Drawing
            
            # 建立 256x256 的圖片
            $bmp = New-Object System.Drawing.Bitmap 256, 256
            $graphics = [System.Drawing.Graphics]::FromImage($bmp)
            
            # 設定背景顏色 (藍色漸層)
            $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
                [System.Drawing.Point]::new(0, 0),
                [System.Drawing.Point]::new(256, 256),
                [System.Drawing.Color]::DodgerBlue,
                [System.Drawing.Color]::MidnightBlue
            )
            $graphics.FillRectangle($brush, 0, 0, 256, 256)
            
            # 繪製文字 "DM"
            $font = New-Object System.Drawing.Font("Arial", 80, [System.Drawing.FontStyle]::Bold)
            $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
            $format = New-Object System.Drawing.StringFormat
            $format.Alignment = [System.Drawing.StringAlignment]::Center
            $format.LineAlignment = [System.Drawing.StringAlignment]::Center
            
            $rect = New-Object System.Drawing.RectangleF 0, 0, 256, 256
            $graphics.DrawString("DM", $font, $textBrush, $rect, $format)
            
            # 儲存圖片
            $bmp.Save((Resolve-Path "Properties" | Join-Path -ChildPath "Thumbnail.png"))
            
            # 清理資源
            $graphics.Dispose()
            $brush.Dispose()
            $font.Dispose()
            $textBrush.Dispose()
            $format.Dispose()
            $bmp.Dispose()
            
            Write-Host "`n✓ 已建立預設縮圖: $thumbnailPath" -ForegroundColor Green
            Write-Host "  這是一個簡單的預設縮圖" -ForegroundColor Gray
            Write-Host "  建議使用專業工具建立更精美的縮圖" -ForegroundColor Yellow
            
        } catch {
            Write-Host "`n✗ 建立縮圖失敗: $_" -ForegroundColor Red
            Write-Host "  請手動建立縮圖檔案" -ForegroundColor Yellow
        }
    } else {
        Write-Host "`n提示: 使用 -Create 參數可自動建立簡易預設縮圖" -ForegroundColor Cyan
        Write-Host "範例: .\check-thumbnail.ps1 -Create" -ForegroundColor Gray
    }
}

Write-Host ""
