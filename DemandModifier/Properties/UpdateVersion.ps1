# 自動更新 ModVersion 的 PowerShell 腳本
param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigPath
)

function Update-ModVersion {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        Write-Error "找不到設定檔: $Path"
        exit 1
    }
    
    try {
        # 讀取 XML 檔案
        [xml]$xml = Get-Content $Path -Encoding UTF8
        
        # 取得當前版本
        $currentVersion = $xml.Publish.ModVersion.Value
        
        if ([string]::IsNullOrEmpty($currentVersion)) {
            Write-Warning "⚠️ ModVersion 為空，設定為 0.0.1"
            $xml.Publish.ModVersion.Value = "0.0.1"
            $xml.Save($Path)
            Write-Host "✅ 版本已更新: 0.0.1" -ForegroundColor Green
            return
        }
        
        # 解析版本號 (Major.Minor.Patch)
        $versionParts = $currentVersion -split '\.'
        
        if ($versionParts.Count -ne 3) {
            Write-Warning "⚠️ 版本格式不正確，應為 Major.Minor.Patch"
            exit 1
        }
        
        $major = [int]$versionParts[0]
        $minor = [int]$versionParts[1]
        $patch = [int]$versionParts[2]
        
        # 遞增 Patch 版本號
        $patch++
        
        # 組合新版本
        $newVersion = "$major.$minor.$patch"
        
        # 更新 XML
        $xml.Publish.ModVersion.Value = $newVersion
        
        # 儲存（保留 UTF-8 編碼和格式化）
        $settings = New-Object System.Xml.XmlWriterSettings
        $settings.Indent = $true
        $settings.IndentChars = "`t"
        $settings.Encoding = [System.Text.Encoding]::UTF8
        
        $writer = [System.Xml.XmlWriter]::Create($Path, $settings)
        $xml.Save($writer)
        $writer.Close()
        
        Write-Host "✅ ModVersion 已自動更新: $currentVersion → $newVersion" -ForegroundColor Green
        
    } catch {
        Write-Error "更新版本時發生錯誤: $_"
        exit 1
    }
}

# 執行更新
Update-ModVersion -Path $ConfigPath
