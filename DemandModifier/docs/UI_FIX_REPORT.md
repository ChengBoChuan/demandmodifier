# UI 問題修正報告 - 2025-10-27

## 問題分析

根據您的截圖和需求，發現了三個主要問題：

1. **語言選單顯示錯誤**：下拉選單顯示的是模組設定值（如 "EnableResidentialDemand"）而非語言選項
2. **分頁列表空白**：上方的分頁列表（Tab Bar）沒有顯示正確的分頁名稱
3. **模組名稱過長**：在遊戲中顯示的模組名稱過於冗長

## 問題根源

### 1. `SettingsUISection` 參數順序錯誤

**原程式碼（錯誤）：**
```csharp
[SettingsUISection("住宅需求", "需求控制")]  // 群組名稱, 分頁名稱 ❌
public bool EnableResidentialDemand { get; set; }
```

**正確寫法：**
```csharp
[SettingsUISection("DemandTab", "ResidentialGroup")]  // 分頁名稱, 群組名稱 ✅
public bool EnableResidentialDemand { get; set; }
```

根據官方文件 [Options UI](https://cs2.paradoxwikis.com/Options_UI)：
```csharp
// 正確的參數順序是：Tab, Group
[SettingsUISection("Tab name", "Group name")]
```

### 2. 分頁/群組識別符使用中文

遊戲引擎的內部機制需要使用**英文識別符**（ID），然後透過語系檔案對應到各語言的翻譯。

**問題：**
- 使用中文作為 Tab/Group ID：`"需求控制"`, `"住宅需求"`
- 導致系統無法正確識別和映射翻譯

**解決方案：**
- Tab ID 改為：`DemandTab`, `ServiceTab`, `EconomicTab`
- Group ID 改為：`ResidentialGroup`, `CommercialGroup`, `IndustrialGroup`, 等

### 3. 不必要的語言選擇功能

原程式碼包含了 `ModLanguage` 列舉和 `Language` 屬性，試圖手動實作語言切換功能。但 Cities: Skylines 2 的遊戲引擎**已經內建完整的多國語系系統**，會根據玩家的遊戲語言自動載入對應的 `l10n/*.json` 檔案。

## 修正內容

### 1. DemandModifierSettings.cs

**變更項目：**

#### A. 移除 ModLanguage 列舉
```csharp
// ❌ 刪除整個 ModLanguage enum
public enum ModLanguage { ... }
```

#### B. 修正類別標記
```csharp
[SettingsUITabOrder("DemandTab", "ServiceTab", "EconomicTab")]  // 使用英文 ID
[SettingsUIGroupOrder("ResidentialGroup", "CommercialGroup", "IndustrialGroup", "ServiceGroup", "EconomicGroup")]
[SettingsUIShowGroupName("ResidentialGroup", "CommercialGroup", "IndustrialGroup", "ServiceGroup", "EconomicGroup")]
```

#### C. 修正所有屬性的 SettingsUISection 標記
```csharp
// 需求控制分頁
[SettingsUISection("DemandTab", "ResidentialGroup")]  // 分頁, 群組
public bool EnableResidentialDemand { get; set; }

[SettingsUISection("DemandTab", "CommercialGroup")]
public bool EnableCommercialDemand { get; set; }

[SettingsUISection("DemandTab", "IndustrialGroup")]
public bool EnableIndustrialDemand { get; set; }

// 服務控制分頁
[SettingsUISection("ServiceTab", "ServiceGroup")]
public bool EnableUnlimitedElectricity { get; set; }
// ... 其他服務選項

// 經濟控制分頁
[SettingsUISection("EconomicTab", "EconomicGroup")]
public bool EnableUnlimitedMoney { get; set; }
// ... 其他經濟選項
```

#### D. 移除 Language 屬性
```csharp
// ❌ 刪除
[SettingsUISection("介面設定", "一般設定")]
public ModLanguage Language { get; set; }
```

### 2. 語系檔案更新

所有 7 個 `l10n/*.json` 檔案都已更新：

**新的鍵值結構：**
```json
{
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandTab]": "需求控制",
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.ServiceTab]": "服務控制",
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.EconomicTab]": "經濟控制",
  
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialGroup]": "住宅需求",
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.CommercialGroup]": "商業需求",
  // ... 其他群組
}
```

**移除的鍵值：**
```json
// ❌ 不再需要
"Options.OPTION[...Language]"
"Options.ENUM[...ModLanguage.SystemDefault]"
// ... 所有語言選項相關翻譯
```

### 3. PublishConfiguration.xml

**DisplayName 保持簡潔：**
```xml
<DisplayName Value="Demand Modifier" />
```

已經是簡短易讀的名稱，不需修改。如果遊戲中顯示過長，可能是因為其他 metadata 欄位的影響。

## 技術原理說明

### SettingsUISection 運作機制

```
[SettingsUISection("TabID", "GroupID")]
                    ↓         ↓
                分頁識別符   群組識別符
                    ↓         ↓
                透過語系檔案映射
                    ↓         ↓
        Options.SECTION[...]  Options.GROUP[...]
                    ↓         ↓
               遊戲UI顯示翻譯文字
```

### 語系系統運作流程

1. **遊戲啟動時**：讀取玩家的語言設定（如：繁體中文）
2. **載入模組時**：掃描 `l10n/` 資料夾
3. **對應檔案**：載入 `l10n/zh-HANT.json`
4. **映射翻譯**：
   - `DemandTab` → "需求控制"
   - `ResidentialGroup` → "住宅需求"
   - `EnableResidentialDemand` → "住宅需求永遠滿格"

### 為何移除手動語言選擇？

**原因 1：冗餘設計**
- 遊戲已有完整的語系系統
- 玩家在遊戲設定中切換語言時，所有模組會自動跟隨

**原因 2：維護負擔**
- 需要為每個語言選項建立翻譯
- 需要額外邏輯處理語言切換
- 可能與遊戲內建系統衝突

**原因 3：使用者體驗**
- 玩家期望所有模組跟隨遊戲語言
- 獨立的模組語言設定會造成困惑
- 標準做法是跟隨遊戲語言

## 測試建議

### 1. 本機測試
```powershell
# 部署到遊戲 Mods 資料夾
$modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
Remove-Item $modsPath -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item "bin\Release\net48\*" $modsPath -Recurse -Force
```

### 2. 檢查清單

**設定 UI：**
- [ ] 上方分頁列表顯示：「需求控制」「服務控制」「經濟控制」
- [ ] 點擊各分頁能正確切換內容
- [ ] 不再出現語言選擇下拉選單
- [ ] 所有開關選項正常顯示（Toggle 樣式）

**多語言測試：**
- [ ] 切換到英文：分頁顯示 "Demand Control", "Service Control", "Economic Control"
- [ ] 切換到簡體中文：分頁顯示對應的簡體翻譯
- [ ] 切換到日文/德文/等：對應語言正確顯示

**功能測試：**
- [ ] 住宅需求開關：啟用後需求跳至最大
- [ ] 商業需求開關：功能正常
- [ ] 工業需求開關：功能正常

### 3. 疑難排解

**問題：分頁仍然空白**
- 檢查 `l10n/zh-HANT.json` 是否正確複製到輸出目錄
- 確認鍵值格式完全正確（大小寫、標點符號）

**問題：翻譯未生效**
- 重新啟動遊戲（熱重載可能不適用於 metadata）
- 檢查日誌檔案：`%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log`

**問題：模組名稱顯示錯誤**
- 檢查 `PublishConfiguration.xml` 的 `<DisplayName>` 欄位
- 確認沒有不可見字元（BOM, 零寬空白）

## 參考文件

1. **Options UI 官方文件**：https://cs2.paradoxwikis.com/Options_UI
   - `SettingsUISectionAttribute` 說明
   - Tab/Group 順序設定

2. **Modding Toolchain**：https://cs2.paradoxwikis.com/Modding_Toolchain
   - 語系檔案結構
   - 發佈流程

3. **Unity Localization**（遊戲使用的底層系統）
   - Key-Value 對應機制
   - Fallback 邏輯（找不到翻譯時降級到 en-US）

## 後續建議

### 1. 版本號更新
```xml
<ModVersion Value="0.1.0" />
<ChangeLog Value="修正設定介面問題 - Fixed settings UI issues: corrected tab/group structure, removed redundant language selector" />
```

### 2. 文件更新
更新 `README.md` 和開發指南，記錄此次問題的根本原因，避免未來重複錯誤。

### 3. 測試其他模組
檢查是否有其他流行模組使用類似的 UI 結構，作為最佳實踐參考。

## 總結

此次修正的核心問題是**混淆了遊戲 UI 系統的識別符（ID）和顯示文字（Display Text）**：

- **識別符**：應該是穩定的英文 ID（如 `DemandTab`）
- **顯示文字**：透過語系檔案映射，支援多語言

正確的設計模式：
```
程式碼 (ID) → 語系檔案 (Translation) → 遊戲 UI (Display)
   ↓               ↓                    ↓
DemandTab  →  "需求控制"/"Demand Control" → 玩家看到的文字
```

這符合國際化（i18n）的標準實作方式，也是 Cities: Skylines 2 模組開發的推薦做法。
