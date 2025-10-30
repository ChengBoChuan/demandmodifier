# 故障排查與除錯檢查清單

## 🔍 診斷步驟

### 第 1 步：驗證模組載入

**在遊戲中檢查**：
- [ ] Mods 列表中看到 "Demand Modifier"
- [ ] 模組顯示為 "已啟用"

**在日誌中查找**：
```
[INFO] ▶ ════════════ DemandModifier 載入開始 ════════════
[INFO] ✓ 日誌系統已初始化
[INFO] ✓ 語言系統已初始化
[INFO] 設定已載入
[INFO] ✓ Harmony 補丁註冊完成
[INFO] ▶ ════════════ DemandModifier 載入完成 ════════════
```

**若未見上述訊息**：
- 檢查 `Mods/DemandModifier/` 目錄是否存在
- 確認 `DemandModifier.dll` 檔案存在且未損壞
- 查看完整日誌以尋找錯誤訊息

---

### 第 2 步：驗證設定 UI

**在遊戲中檢查**：
- [ ] 進入遊戲設定 > 選項 > Mods > Demand Modifier
- [ ] 看到三個分頁：需求控制、服務控制、經濟控制

**若看不到設定介面**：
```
日誌查找關鍵詞：
[ERROR] 設定系統初始化失敗
```

**可能原因和解決方案**：
| 症狀 | 可能原因 | 解決方案 |
|------|---------|---------|
| 設定項為空 | 語言檔案遺失 | 確認 `Mods/DemandModifier/l10n/*.json` 存在 |
| 下拉選單顯示 `Common.ENUM[...]` | 翻譯未載入 | 重啟遊戲，查看日誌中的語言載入訊息 |
| 無法開啟設定 | 模組初始化失敗 | 查看完整日誌尋找異常 |

---

### 第 3 步：驗證功能是否生效

#### 需求控制測試

```
測試程序：
1. 設定 "住宅需求級別" = "最大"
2. 開始遊戲或載入存檔
3. 查看城市中的住宅區域
4. 觀察需求指示器（應始終顯示滿）
5. 設定改為 "關閉" 並重新觀察（應恢復正常波動）

期望結果：
- 設定為 "最大" 時：需求條始終為滿
- 設定為 "關閉" 時：需求條正常波動
- 其他等級：相應顯示 25%, 50%, 75% 需求
```

**日誌驗證**：
```
查找：
[DEBUG] ✓ 住宅需求已修改為: 255
[DEBUG] ✓ 商業需求已修改為: 255
[DEBUG] ✓ 工業需求已修改為: 255
```

#### 服務控制測試

```
測試程序：
1. 啟用 "無限電力"
2. 觀察城市電力狀態（應始終充足）
3. 停用此功能並觀察（電力狀態應恢復正常）

期望結果：
- 啟用時：所有建築始終有電力
- 停用時：電力狀態恢復正常（可能缺電）
```

---

## 🐛 常見問題與解決方案

### 問題 1：模組根本不載入

**症狀**：
- Mods 列表中不見 Demand Modifier
- 日誌中無任何模組訊息

**診斷**：
1. 檢查檔案位置
   ```powershell
   $path = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
   if (Test-Path $path) { Write-Host "✓ 目錄存在" } else { Write-Host "✗ 目錄不存在" }
   Get-ChildItem $path -Recurse
   ```

2. 驗證 DLL 簽名
   ```powershell
   $dll = "$path\DemandModifier.dll"
   [System.Reflection.Assembly]::LoadFile($dll) | Select-Object FullName
   ```

**解決方案**：
- [ ] 重新部署模組 `.\test-deploy.ps1`
- [ ] 清理並重新建置 `dotnet clean && dotnet build -c Release`
- [ ] 確認遊戲版本相容（v1.2.* 及以上）

---

### 問題 2：Harmony 補丁套用失敗

**症狀**：
- 日誌顯示 `[ERROR] Harmony 補丁系統初始化失敗`
- 功能完全無效

**診斷**：
查看完整錯誤訊息：
```
日誌搜尋：
[ERROR] Harmony 補丁系統初始化失敗: ...
```

常見原因：
- 目標系統類別名稱錯誤
- 欄位名稱變更（遊戲更新）
- Harmony 版本不相容

**解決方案**：
- [ ] 使用 dnSpy 驗證系統類別名稱
  ```
  開啟: [Steam]\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll
  搜尋: ResidentialDemandSystem
  確認欄位: m_BuildingDemand
  ```
- [ ] 更新補丁中的類別/欄位名稱
- [ ] 檢查遊戲版本是否符合支援範圍

---

### 問題 3：設定無法保存

**症狀**：
- 修改設定後重啟遊戲，設定恢復預設值
- 日誌無異常訊息

**診斷**：
檢查設定檔路徑和權限：
```powershell
$settingsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\ModsSettings"
# 驗證目錄存在
if (Test-Path $settingsPath) {
    Write-Host "✓ ModsSettings 目錄存在"
    Get-Item $settingsPath -Force | Select-Object -Property FullName, Mode
} else {
    Write-Host "✗ ModsSettings 目錄不存在"
}

# 檢查 DemandModifier.coc 檔案
$settingsFile = "$settingsPath\DemandModifier.coc"
if (Test-Path $settingsFile) {
    Write-Host "✓ 設定檔存在"
    Get-Item $settingsFile | Select-Object -Property FullName, Length, LastWriteTime
}
```

**解決方案**：
- [ ] 確保目錄可寫入：`Icacls $settingsPath /grant "$env:USERNAME:(OI)(CI)F"`
- [ ] 檢查磁碟空間是否充足
- [ ] 刪除損壞的設定檔：`Remove-Item $settingsFile`（遊戲會重新建立）

---

### 問題 4：下拉選單顯示未翻譯的鍵值

**症狀**：
- 需求級別下拉選單顯示 `Common.ENUM[DemandModifier.DemandLevel.Off]`
- 而非預期的翻譯文本（如 "關閉" 或 "Off"）

**診斷**：
查看日誌中的語言初始化訊息：
```
搜尋：
[INFO] 語言系統已初始化
[DEBUG] 支援的語言列表
[INFO] 當前語言: ...
```

**原因分析**：
```
可能原因 1: 語言檔案遺失
├─ 檢查: Mods/DemandModifier/l10n/en-US.json 等
└─ 解決: 重新部署模組

可能原因 2: JSON 檔案格式錯誤
├─ 檢查: 使用 JSON 驗證工具檢查 l10n/*.json
├─ 常見錯誤: 缺少逗號、不匹配的引號
└─ 解決: 修正 JSON 格式

可能原因 3: 翻譯鍵值不匹配
├─ 檢查: DemandModifierSettings.cs 中的 GetDemandLevelOptions()
├─ 方法: 查看 GetLocalizedEnumName() 返回值
└─ 解決: 確保鍵值完全匹配
```

**解決方案**：
- [ ] 驗證 JSON 檔案
  ```powershell
  Get-Content "Mods\DemandModifier\l10n\en-US.json" | ConvertFrom-Json | Out-Null
  # 如果沒有錯誤，JSON 格式正確
  ```
- [ ] 重啟遊戲以重新載入語言檔案
- [ ] 檢查 LocalizationInitializer.cs 中的語言載入邏輯

---

### 問題 5：效能問題（遊戲卡頓）

**症狀**：
- 啟用模組後遊戲 FPS 下降
- 存檔載入時間增加

**診斷**：
使用日誌中的計時資訊：
```
搜尋：
[DEBUG] 計時結束: ... = XXXms
```

**可能原因**：
| 原因 | 特徵 | 解決方案 |
|------|------|---------|
| 日誌等級過高 | 大量 DEBUG 訊息 | 調整日誌等級至 INFO |
| 補丁數量過多 | 每幀多個補丁執行 | 停用不需要的功能 |
| 語言查詢效率低 | 語言載入訊息頻繁 | 確保語言已快取 |

**解決方案**：
- [ ] 臨時禁用不使用的功能
- [ ] 清除日誌檔案並重新啟動（基線測試）
- [ ] 查看具體的計時訊息以定位瓶頸

---

## 📋 完整檢查清單

### 初始安裝檢查

- [ ] Visual Studio 2022 已安裝
- [ ] .NET Framework 4.8.1 已安裝
- [ ] Cities: Skylines 2 已安裝（v1.2.* 或更新）
- [ ] CS2 Modding SDK 已安裝
- [ ] 環境變數 `CSII_TOOLPATH` 已設定

### 建置檢查

- [ ] `dotnet build -c Release` 完成無錯誤
- [ ] `bin/Release/net48/` 生成 `DemandModifier.dll`
- [ ] `bin/Release/net48/l10n/` 包含 7 個 JSON 檔案

### 部署檢查

- [ ] `.\test-deploy.ps1` 執行成功
- [ ] 檔案複製到 `%LocalAppData%\..\LocalLow\...\Mods\DemandModifier\`
- [ ] 目錄結構正確

### 遊戲內檢查

- [ ] 重啟遊戲
- [ ] Mods 列表中看到 "Demand Modifier"
- [ ] 設定 > Mods > Demand Modifier 可以開啟
- [ ] 看到所有三個分頁
- [ ] 所有選項均以當前語言顯示

### 功能測試

- [ ] 修改需求等級並查看效果
- [ ] 啟用服務控制功能
- [ ] 切換遊戲語言並驗證翻譯
- [ ] 重啟遊戲並驗證設定已保存

### 日誌驗證

- [ ] 遊戲日誌中無 `[ERROR]` 訊息
- [ ] Harmony 補丁成功套用
- [ ] 功能執行時有適當的 `[DEBUG]` 訊息

---

## 🆘 獲取幫助

如果問題未在上述列表中解決：

### 1. 收集診斷資訊

```powershell
# 收集系統資訊
Write-Host "=== 系統資訊 ===" 
$PSVersionTable.PSVersion
[System.Environment]::OSVersion
dotnet --version

# 收集遊戲日誌（最後 100 行涉及 DemandModifier）
$logPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log"
Get-Content $logPath -Tail 100 | Select-String "DemandModifier" > demandmodifier_log.txt

# 收集模組檔案清單
Get-ChildItem "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier" -Recurse > demandmodifier_files.txt
```

### 2. 重現問題

按照特定步驟重現問題，並記錄：
- 操作步驟
- 期望結果
- 實際結果
- 相關日誌訊息

### 3. 提交報告

在 GitHub Issues 中提交，包含：
- 診斷資訊（系統、版本）
- 重現步驟
- 日誌檔案（或摘錄）
- 模組檔案清單

---

**版本**: 2.0  
**最後更新**: 2025-10-30
