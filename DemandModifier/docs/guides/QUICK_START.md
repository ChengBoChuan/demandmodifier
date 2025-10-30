# 快速入門指南

## ⚡ 5 分鐘快速開始

### 1. 環境設定

確保您已經安裝：
- Visual Studio 2022
- .NET Framework 4.8.1
- Cities: Skylines 2 遊戲

### 2. 設定 SDK 環境變數

在 PowerShell 中執行：

```powershell
[System.Environment]::SetEnvironmentVariable(
    'CSII_TOOLPATH',
    'C:\Path\To\CS2ModdingSDK',
    'User'
)
```

### 3. 建置專案

```powershell
cd DemandModifier
dotnet build -c Release
```

### 4. 部署到遊戲

```powershell
.\scripts\test-deploy.ps1
```

### 5. 啟動遊戲並測試

- 啟動 Cities: Skylines 2
- 進入遊戲設定 > Mods
- 啟用 "Demand Modifier"
- 開啟遊戲內設定查看選項

---

## 📖 核心概念

### 需求控制

住宅、商業、工業區域的需求可分為 5 級：
- **Off**: 使用遊戲預設需求
- **Low**: 25% 需求
- **Medium**: 50% 需求
- **High**: 75% 需求
- **Maximum**: 100% 需求（始終最高）

### 服務控制

啟用無限服務：
- 無限電力 / 供水 / 污水 / 垃圾
- 無限醫療 / 教育 / 警察 / 消防

### 經濟控制

- 無限金錢：城市預算永不耗盡
- 免費建造：建築和基礎設施零成本
- 零維護：沒有維護費用

---

## 🔧 日誌查看

### 查看日誌檔案

```
%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log
```

### PowerShell 即時監控

```powershell
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Wait -Tail 50 | Select-String "DemandModifier"
```

### 日誌等級

```
[INFO]      一般資訊 - 模組載入、設定變更
[DEBUG]     除錯資訊 - 補丁執行詳情
[WARN]      警告訊息 - 非預期的情況
[ERROR]     錯誤訊息 - 功能失敗
[CRITICAL]  嚴重錯誤 - 可能導致崩潰
```

---

## 🌍 多國語言支援

支援的語言：
- 英文 (en-US)
- 德文 (de-DE)
- 西班牙文 (es-ES)
- 法文 (fr-FR)
- 日文 (ja-JP)
- 簡體中文 (zh-HANS)
- 繁體中文 (zh-HANT)

遊戲內切換語言時會自動應用翻譯。

---

## 🎯 常見問題

### Q: 模組無法載入？

**A**: 檢查以下項目：
1. 確認 `Mods\DemandModifier\DemandModifier.dll` 存在
2. 查看日誌檔案中的錯誤訊息
3. 確認遊戲版本相容（v1.2.* 及以上）

### Q: 設定無法保存？

**A**: 設定檔位置：
```
%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\ModsSettings\DemandModifier.coc
```

確保此目錄可寫入。

### Q: 下拉選單顯示鍵值而非翻譯文本？

**A**: 這表示語言檔案未正確載入。檢查：
1. `Mods\DemandModifier\l10n\` 資料夾存在
2. JSON 檔案格式正確（使用 JSON 驗證工具）
3. 查看日誌中的語言載入訊息

### Q: 補丁未生效？

**A**: 檢查日誌中的補丁狀態：
```
[INFO] ✓ Harmony 補丁註冊完成
```

如果未見此訊息，查看異常日誌。

---

## 🚀 開發與擴展

### 添加新功能的步驟

1. **在 DemandModifierSettings.cs 中加入設定選項**
   ```csharp
   [SettingsUISection("ServiceControl", "ServiceSettings")]
   public bool EnableNewFeature { get; set; }
   ```

2. **更新所有 7 個語言檔案 (l10n/*.json)**
   ```json
   "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableNewFeature]": "New Feature"
   ```

3. **建立 Harmony 補丁**
   ```csharp
   [HarmonyPatch(typeof(TargetSystem), "OnUpdate")]
   public class NewFeaturePatch
   {
       static void Prefix(TargetSystem __instance)
       {
           if (DemandModifierMod.Settings?.EnableNewFeature != true)
               return;
           
           Logger.Info("新功能已套用");
           // 實作邏輯...
       }
   }
   ```

4. **測試並提交**

---

## 📦 發佈到 PDX Mods

### 首次發佈

```powershell
dotnet publish /p:PublishProfile=PublishNewMod
```

系統會返回 ModId，請保存此值。

### 更新現有模組

1. 更新版本號 `Properties/PublishConfiguration.xml`
2. 更新更改日誌
3. 執行發佈命令

```powershell
dotnet publish /p:PublishProfile=PublishNewVersion
```

---

## 📚 進階主題

- [完整架構文件](./ARCHITECTURE.md)
- [Harmony 補丁指南](./guides/PATCH_GUIDE.md)
- [語言本地化指南](./guides/LOCALIZATION_GUIDE.md)
- [除錯與故障排查](./troubleshooting/FIX_CHECKLIST.md)

---

**版本**: 1.0  
**最後更新**: 2025-10-30
