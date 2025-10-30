# 多國語言本地化完整指南

## 🌍 支援語言

本專案支援 7 種語言：

| 語言代碼 | 語言名稱 | 檔案 |
|---------|---------|------|
| en-US | English (USA) | l10n/en-US.json |
| de-DE | Deutsch (Deutschland) | l10n/de-DE.json |
| es-ES | Español (España) | l10n/es-ES.json |
| fr-FR | Français (France) | l10n/fr-FR.json |
| ja-JP | 日本語 (日本) | l10n/ja-JP.json |
| zh-HANS | 简体中文 (中国) | l10n/zh-HANS.json |
| zh-HANT | 繁體中文 (台灣) | l10n/zh-HANT.json |

---

## 語系鍵值結構

### 命名規則

所有鍵值遵循統一格式：

```
Options.SECTION[<Namespace>.<ClassName>.<ObjectName>]
Options.GROUP[<Namespace>.<ClassName>.<ObjectName>]
Options.OPTION[<Namespace>.<ClassName>.<ObjectName>]
Options.OPTION_DESCRIPTION[<Namespace>.<ClassName>.<ObjectName>]
Common.ENUM[<Namespace>.<EnumName>.<ValueName>]
```

### 具體範例

#### 1. 分頁 (Tab)
```json
"Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandControl]": "需求控制"
```
**組成**：
- Namespace: `DemandModifier`
- ClassName: `DemandModifier`
- TabName: `DemandControl`（來自 `SettingsUISection` 的第一個參數）

#### 2. 群組 (Group)
```json
"Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemand]": "住宅需求"
```
**組成**：
- Namespace: `DemandModifier`
- ClassName: `DemandModifier`
- GroupName: `ResidentialDemand`（來自 `SettingsUISection` 的第二個參數）

#### 3. 選項標題 (Option Label)
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "住宅需求級別"
```
**組成**：
- PropertyName: `ResidentialDemandLevel`（C# 屬性名稱）

#### 4. 選項描述 (Option Description)
```json
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "控制住宅區域的需求級別..."
```

#### 5. Enum 值 (Enum Values)
```json
"Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]": "關閉 (遊戲預設)"
"Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]": "最大 (100%)"
```
**組成**：
- EnumName: `DemandLevel`
- ValueName: `Off`, `Low`, `Medium`, `High`, `Maximum`

---

## 新增翻譯的完整步驟

### 場景 1：新增新選項

#### 步驟 1：在 DemandModifierSettings.cs 中加入屬性

```csharp
[SettingsUISection("ServiceControl", "ServiceSettings")]
public bool EnableNewFeature { get; set; }
```

#### 步驟 2：更新 7 個 l10n/*.json 檔案

**en-US.json**：
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableNewFeature]": "New Feature",
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableNewFeature]": "Detailed description of the feature..."
```

**zh-HANT.json**：
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableNewFeature]": "新功能",
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableNewFeature]": "功能的詳細說明..."
```

**zh-HANS.json**：
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableNewFeature]": "新功能",
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableNewFeature]": "功能的详细说明..."
```

**其他 4 種語言**（de-DE, es-ES, fr-FR, ja-JP）：類似翻譯

#### 步驟 3：測試

- 建置專案：`dotnet build -c Release`
- 部署到遊戲：`.\test-deploy.ps1`
- 進入遊戲，切換各語言驗證翻譯

---

### 場景 2：新增新分頁和群組

#### 步驟 1：在 DemandModifierSettings.cs 更新屬性

```csharp
[SettingsUITabOrder("DemandControl", "ServiceControl", "NewTab")]
[SettingsUIGroupOrder("Group1", "Group2", "NewGroup")]
public class DemandModifierSettings : ModSetting
{
    [SettingsUISection("NewTab", "NewGroup")]
    public bool NewOption { get; set; }
}
```

#### 步驟 2：更新所有語言檔案

新增分頁鍵值：
```json
"Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.NewTab]": "新分頁",
```

新增群組鍵值：
```json
"Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.NewGroup]": "新群組",
```

新增選項鍵值：
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.NewOption]": "新選項",
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.NewOption]": "新選項的說明",
```

---

### 場景 3：新增 Enum 值

#### 步驟 1：擴展 Enum

```csharp
public enum DemandLevel
{
    Off = 0,
    Low = 64,
    Medium = 128,
    High = 192,
    Maximum = 255,
    Custom = 128  // 新值
}
```

#### 步驟 2：在 DemandModifierSettings 中更新 GetDemandLevelOptions()

```csharp
private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
{
    var levels = new DemandLevel[] 
    { 
        DemandLevel.Off, 
        DemandLevel.Low, 
        DemandLevel.Medium, 
        DemandLevel.High,
        DemandLevel.Custom,    // 新增
        DemandLevel.Maximum 
    };
    // ... 迴圈建立項目
}
```

#### 步驟 3：更新所有語言檔案

```json
"Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Custom]": "自訂 (50%)",
```

---

## 翻譯最佳實踐

### Do's ✅

- ✅ 使用簡潔、清晰的語言
- ✅ 保持術語一致（使用術語表）
- ✅ 測試所有語言的文本長度（可能導致 UI 布局問題）
- ✅ 保留原始功能説明的要點
- ✅ 針對目標語言進行本地化（不只是直譯）

### Don'ts ❌

- ❌ 逐字直譯（會顯得不自然）
- ❌ 混合語言
- ❌ 使用機器翻譯未核實
- ❌ 忽略文化差異
- ❌ 過度簡化或添加過多詞語

### 翻譯指南

#### 通用術語

| 英文 | 繁體中文 | 簡體中文 | 說明 |
|------|---------|---------|------|
| Demand | 需求 | 需求 | 遊戲機制 |
| Zone | 區域 | 区域 | 城市規劃 |
| Service | 服務 | 服务 | 公共服務 |
| Residential | 住宅 | 住宅 | 住房區域 |
| Commercial | 商業 | 商业 | 商業區域 |
| Industrial | 工業 | 工业 | 工業區域 |
| Unlimited | 無限 | 无限 | 無上限 |
| Maintenance Cost | 維護成本 | 维护成本 | 運營費用 |

#### 上下文相關翻譯

**"Off" 的多種翻譯**：
- 在選項中：「關閉」
- 在電源語境：「斷開」
- 一般禁用：「停用」

選擇時考慮上下文。

---

## JSON 格式驗證

### 常見錯誤

```json
// ❌ 錯誤 1: 缺少逗號
{
  "key1": "value1"
  "key2": "value2"
}

// ❌ 錯誤 2: 尾部逗號
{
  "key1": "value1",
  "key2": "value2",
}

// ❌ 錯誤 3: 未轉義的特殊字元
{
  "key": "value with "quotes" inside"
}

// ✅ 正確
{
  "key1": "value1",
  "key2": "value2",
  "key3": "value with \"escaped quotes\""
}
```

### 驗證工具

**PowerShell**：
```powershell
$json = Get-Content "l10n/en-US.json" -Raw
try {
    $json | ConvertFrom-Json | Out-Null
    Write-Host "✓ JSON 格式正確"
} catch {
    Write-Host "✗ JSON 格式錯誤:"
    Write-Host $_.Exception.Message
}
```

**線上工具**：
- https://jsonlint.com/
- https://www.jsonschemavalidator.net/

---

## LocaleManager 整合

### 自動語言切換

遊戲內語言變更時自動應用翻譯：

```csharp
// LocaleManager 訂閱語言變更事件
LocaleManager.OnLocaleChanged += (newLocale) =>
{
    Logger.Info("語言已切換為: {0}", newLocale);
    // 刷新 UI（如需要）
};
```

### 手動翻譯查詢

```csharp
// 獲取翻譯文本
string translated = LocaleManager.GetTranslation(
    "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]",
    fallback: "Residential Demand Level"  // 若翻譯不存在
);

// 建立標準鍵值
string key = LocaleManager.BuildOptionLocaleKey(
    "DemandModifier",
    "DemandModifier", 
    "ResidentialDemandLevel"
);
```

---

## 多語言測試檢查清單

### 完整性檢查

- [ ] 所有 7 個語言檔案都已更新
- [ ] 所有鍵值在 7 個檔案中一致
- [ ] JSON 格式在所有檔案中正確
- [ ] 沒有缺少的分頁或群組
- [ ] 所有 Enum 值都已翻譯

### 功能測試

- [ ] 遊戲內所有選項都正確顯示翻譯
- [ ] 下拉選單顯示翻譯文本（非鍵值）
- [ ] 切換遊戲語言時翻譯更新
- [ ] 選項描述完整可讀
- [ ] 無 UI 重疊或截斷

### 品質檢查

- [ ] 翻譯術語一致
- [ ] 沒有機器翻譯的奇怪措辭
- [ ] 語調與遊戲風格一致
- [ ] 文化適當性（無冒犯或不尊重）
- [ ] 簡體和繁體中文區分正確

---

## 翻譯資源

### 推薦翻譯工具

- **DeepL Translator**: https://www.deepl.com/ (高品質)
- **Google Translate**: https://translate.google.com/ (快速參考)
- **Microsoft Translator**: https://www.microsoft.com/en-us/translator (備選)

### 本地化指南

- [Apple Localization Guidelines](https://developer.apple.com/localization/)
- [Microsoft Localization Guidelines](https://docs.microsoft.com/en-us/windows/uwp/design/globalizing/design-for-localization)
- [Google Material Design Localization](https://material.io/design/platform-guidance/android-bars.html)

---

**版本**: 1.0  
**最後更新**: 2025-10-30
