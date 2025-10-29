# 多國語言系統實作指南

## 概述

DemandModifier 支援 8 種語言，使用遊戲內建的本地化系統。所有使用者介面文本都完全本地化。

## 支援的語言

1. **English (US)** - `en-US.json` - 英文
2. **繁體中文 (台灣)** - `zh-HANT.json` - 繁體中文
3. **简体中文 (中国)** - `zh-HANS.json` - 簡體中文
4. **日本語** - `ja-JP.json` - 日文
5. **Deutsch** - `de-DE.json` - 德文
6. **Español** - `es-ES.json` - 西班牙文
7. **Français** - `fr-FR.json` - 法文

## 檔案位置

所有語言檔案位於：
```
DemandModifier/
└── l10n/
    ├── en-US.json
    ├── zh-HANT.json
    ├── zh-HANS.json
    ├── ja-JP.json
    ├── de-DE.json
    ├── es-ES.json
    └── fr-FR.json
```

## 語言鍵值格式

### 1. 分頁標題 (Tab)

分頁是設定介面的頂層分類。

**格式**：`Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.<分頁名稱>]`

**範例**：
```json
"Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandControl]": "Demand Control"
```

### 2. 群組名稱 (Group)

群組是分頁內的分類。

**格式**：`Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.<群組名稱>]`

**範例**：
```json
"Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemand]": "Residential Demand"
```

### 3. 選項標題 (Option)

選項標題顯示在設定項目旁邊。

**格式**：`Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.<屬性名>]`

**範例**：
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Unlimited Electricity"
```

### 4. 選項描述 (Description)

選項描述顯示在選項下方，提供詳細說明。

**格式**：`Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.<屬性名>]`

**範例**：
```json
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Never run out of electricity - all buildings always receive power."
```

### 5. Enum 值翻譯

Enum 下拉選單的每個選項值都需要翻譯。

**格式**：`Common.ENUM[DemandModifier.DemandModifier.<EnumType>.<值名>]`

**範例**：
```json
"Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]": "Off (Game Default)",
"Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]": "Low (25%)",
"Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]": "Maximum (100%)"
```

## 新增翻譯的步驟

### 1. 在程式中定義新屬性

編輯 `DemandModifierSettings.cs`：

```csharp
[SettingsUISection("DemandControl", "ResidentialDemand")]
[SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
public DemandLevel ResidentialDemandLevel { get; set; }
```

### 2. 在所有 7 個語言檔案中添加翻譯

編輯 `l10n/en-US.json` 等所有語言檔案，添加：

```json
{
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandControl]": "Demand Control",
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemand]": "Residential Demand",
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "Residential Demand Level",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "控制住宅需求等級..."
}
```

### 3. 構建並測試

```bash
dotnet build -c Release
```

啟動遊戲，在 Mods 設定中測試新選項是否正確顯示和翻譯。

## 翻譯品質指南

### 一般原則

1. **白話通順**：避免機器翻譯，使用自然的語言表達
2. **術語一致**：同一術語應在整份文件中保持一致
3. **簡潔明確**：選項描述應簡短、清晰
4. **文化適配**：考慮不同文化和區域的用詞習慣

### 中文翻譯指南

- 使用繁體中文 (zh-HANT) 用於台灣和香港
- 使用簡體中文 (zh-HANS) 用於中國大陸
- 避免過度正式的文言文
- 遊戲術語應保持一致（例如「需求」、「服務」）

### 日文翻譯指南

- 使用標準日本語
- 敬語應適度使用
- 遊戲術語應使用常見的日文遊戲用詞

### 歐洲語言翻譯指南

- 德文：使用正式用詞，注意性數的一致性
- 西班牙文：注意動詞變位的準確性
- 法文：注意介詞的正確使用

## 自動化翻譯工具

建議使用以下工具進行初稿翻譯：

- **DeepL** (推薦)：https://www.deepl.com/ - 高品質翻譯
- **Google Translate**：https://translate.google.com/ - 快速翻譯
- **Microsoft Translator**：https://www.microsoft.com/translator/ - 專業翻譯

**重要**：自動翻譯僅作為參考，最終應由母語使用者核對。

## 除錯多語言系統

### 檢查語言檔案是否正確載入

查看遊戲日誌（位置：`%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log`）：

```
✓ 已註冊語言: en-US (...)
✓ 已註冊語言: zh-HANT (...)
...
```

### 測試特定語言翻譯

1. 啟動遊戲
2. 在設定中切換語言
3. 打開 Mods 設定，檢查 DemandModifier 是否顯示該語言的翻譯

### 常見問題

**Q: 選項顯示未翻譯的鍵值（如 `Common.ENUM[...]`）**

A: 檢查以下事項：
- 確認 JSON 檔案格式正確
- 確認鍵值拼寫完全匹配（區分大小寫）
- 確認 `ModLocale.cs` 正確載入了 JSON 檔案

**Q: 某些語言選項沒有顯示**

A: 可能原因：
- 該語言的 JSON 檔案不完整
- JSON 檔案有語法錯誤
- 檔案編碼不是 UTF-8

## 提交翻譯貢獻

如果您想為 DemandModifier 貢獻翻譯，請：

1. 複製現有的語言檔案
2. 修改所有鍵值的翻譯部分
3. 確保 JSON 格式正確
4. 提交 Pull Request

感謝您的貢獻！
