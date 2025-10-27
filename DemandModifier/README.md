# DemandModifier - 無限需求模組

這是一個 Cities: Skylines 2 的模組，用於控制遊戲中的住宅、商業和工業需求。

**相容遊戲版本**: Cities: Skylines 2 v1.2.*

## 主要功能

### 需求控制
- **住宅需求**: 可選擇性地將所有密度等級（低/中/高）的住宅需求設為最大值
- **商業需求**: 可選擇性地將商業建築需求設為最大值
- **工業需求**: 可選擇性地將工業、倉儲和辦公建築需求設為最大值

### 服務控制（預留功能）
- 無限電力
- 無限水資源
- 無限污水處理
- 無限垃圾處理
- 無限醫療服務
- 無限教育服務
- 無限警察服務
- 無限消防服務

### 經濟控制（預留功能）
- 無限金錢
- 免費建設
- 無維護費用

## 多國語系支援

本模組支援以下語言：
- 🇺🇸 English (en-US)
- 🇹🇼 繁體中文 (zh-HANT) - 台灣
- 🇨🇳 简体中文 (zh-HANS) - 中國
- 🇯🇵 日本語 (ja-JP)
- 🇩🇪 Deutsch (de-DE)
- 🇪🇸 Español (es-ES)
- 🇫🇷 Français (fr-FR)

語系檔案位於 `l10n` 資料夾中，遊戲會根據您的語言設定自動載入對應的翻譯。

## 技術規格

- **目標框架**: .NET Framework 4.8.1
- **遊戲版本**: Cities: Skylines 2 v1.2.*
- **使用技術**: 
  - Harmony 2.2.2 (用於執行時程式碼修改)
  - Cities: Skylines 2 Modding API

## 檔案說明

### DemandModifierMod.cs
模組的主要入口點，負責：
- 初始化和載入模組設定
- 套用 Harmony 補丁
- 處理模組的生命週期
- 記錄多國語系載入資訊

### DemandModifierSettings.cs
模組設定類別，提供：
- 使用者介面設定選項
- 各項功能的開關控制
- 預設值設定
- 多語言支援的設定項目

### DemandSystemPatch.cs
包含三個 Harmony 補丁類別：
- `CommercialDemandSystemPatch`: 修改商業需求
- `IndustrialDemandSystemPatch`: 修改工業需求
- `ResidentialDemandSystemPatch`: 修改住宅需求

### l10n/ (語系資料夾)
包含所有支援語言的 JSON 翻譯檔案：
- `en-US.json` - 英文
- `zh-HANT.json` - 繁體中文（台灣）
- `zh-HANS.json` - 简体中文（中國）
- `ja-JP.json` - 日文
- `de-DE.json` - 德文
- `es-ES.json` - 西班牙文
- `fr-FR.json` - 法文

## .NET 8 到 .NET Framework 4.8.1 的語法轉換

本專案從 .NET 8 轉換為 .NET Framework 4.8.1，主要變更包括：

1. **字串插值**: `$"{nameof(X)}.{nameof(Y)}"` → `string.Format("{0}.{1}", nameof(X), nameof(Y))`
2. **陣列初始化**: `[item1, item2]` → `new Type[] { item1, item2 }`
3. **可空類型檢查**: `Settings?.Property` → `Settings != null && Settings.Property`
4. **目標框架**: `net8.0` → `net481`

## 建置專案

```powershell
dotnet clean
dotnet build
```

建置後，語系檔案會自動複製到輸出目錄的 `l10n` 資料夾中。

## 安裝

將編譯後的 DLL 檔案連同 `l10n` 資料夾一起複製到 Cities: Skylines 2 的 Mods 資料夾中。

## 使用方式

1. 啟動遊戲
2. 在選項中找到 "DemandModifier" 設定
3. 在「需求控制」分頁中啟用所需的需求類型
4. 遊戲中的相應需求將自動設為最大值
5. 介面會根據您的遊戲語言設定自動顯示對應的翻譯

## 版本歷史

### v0.0.1 (2025-10-27)
- 初始版本
- 支援住宅、商業、工業需求控制
- 新增 7 種語言支援
- 更新遊戲相容版本至 1.2.*

## 注意事項

- 服務控制和經濟控制功能目前僅為介面，尚未實作
- 所有需求修改功能預設為啟用
- 可以在遊戲中隨時開關各項功能
- 語系檔案由遊戲系統自動載入，無需額外設定

## 授權

本專案遵循 MIT 授權條款。
