# UI 問題修正報告 v2.0

## 修正日期
2025年10月28日

## 問題分析

根據截圖和線上文件 (https://cs2.paradoxwikis.com/Options_UI)，發現以下問題：

### 問題 1: 多國語言選單顯示錯誤
**症狀**: 下拉選單中顯示的是我們的設定值（EnableResidentialDemand等），而不是語言選項

**根本原因**: 
- 之前有一個 `ModLanguage` enum 和 `Language` 屬性，但後來被移除了
- 語系檔案中仍保留了 enum 翻譯鍵值，但程式碼中已不存在該 enum

**解決方案**: 
- ✅ 從程式碼中完全移除 `ModLanguage` enum 和 `Language` 屬性
- ✅ 從所有語系檔案中移除相關翻譯（OPTION[Language] 和 ENUM[ModLanguage.*]）

### 問題 2: 上方標籤列為空白
**症狀**: 設定介面頂部的 Tab 列表完全空白，無法切換分頁

**根本原因**: 
根據 [Options UI 文件](https://cs2.paradoxwikis.com/Options_UI#SettingsUISectionAttribute)：

```csharp
[SettingsUISection("Tab name", "Group name")]
```

我們的程式碼順序錯誤：
```csharp
// ❌ 錯誤：Tab 和 Group 順序顛倒
[SettingsUISection("住宅需求", "需求控制")]  
// 第一個參數應該是 Tab，第二個才是 Group
```

正確應該是：
```csharp
// ✅ 正確：Tab, Group 順序
[SettingsUISection("需求控制", "住宅需求")]
```

**額外發現**: 使用中文作為識別符可能造成顯示問題

**解決方案**:
1. ✅ 修正所有 `SettingsUISection` 的參數順序
2. ✅ 將所有 Tab 和 Group 名稱改為英文識別符（DemandControl, ServiceControl 等）
3. ✅ 更新 `SettingsUITabOrder` 和 `SettingsUIGroupOrder` 為英文名稱
4. ✅ 更新所有 7 個語系檔案的翻譯鍵值

### 問題 3: 模組名稱過長
**症狀**: 遊戲中顯示的模組名稱為完整的命名空間路徑

**根本原因**: 
- `PublishConfiguration.xml` 中的 `DisplayName` 僅為英文 "Demand Modifier"
- 遊戲可能在某些地方使用命名空間全名顯示

**解決方案**:
✅ 更新 DisplayName 為更簡潔的雙語名稱：`需求修改器 Demand Modifier`

## 完整修正內容

### 1. DemandModifierSettings.cs

#### 類別屬性修正
```csharp
// 修正前
[SettingsUITabOrder("一般設定", "需求控制", "服務控制", "經濟控制")]
[SettingsUIGroupOrder("介面設定", "住宅需求", ...)]

// 修正後 - 使用英文識別符
[SettingsUITabOrder("DemandControl", "ServiceControl", "EconomyControl")]
[SettingsUIGroupOrder("ResidentialDemand", "CommercialDemand", "IndustrialDemand", "ServiceSettings", "EconomySettings")]
```

#### 屬性標記修正
```csharp
// 修正前 - 參數順序錯誤 + 中文識別符
[SettingsUISection("住宅需求", "需求控制")]
public bool EnableResidentialDemand { get; set; }

// 修正後 - 正確順序 + 英文識別符
[SettingsUISection("DemandControl", "ResidentialDemand")]
public bool EnableResidentialDemand { get; set; }
```

#### 移除內容
```csharp
// 已完全移除
public enum ModLanguage { ... }
public ModLanguage Language { get; set; }
```

### 2. 語系檔案更新

所有 7 個語言檔案 (en-US, zh-HANT, zh-HANS, ja-JP, de-DE, es-ES, fr-FR) 都已更新：

#### 修正前的鍵值格式（錯誤）
```json
{
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.一般設定]": "...",
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.住宅需求]": "..."
}
```

#### 修正後的鍵值格式（正確）
```json
{
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandControl]": "需求控制",
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemand]": "住宅需求"
}
```

### 3. PublishConfiguration.xml

```xml
<!-- 修正前 -->
<DisplayName Value="Demand Modifier" />

<!-- 修正後 -->
<DisplayName Value="需求修改器 Demand Modifier" />
```

## 技術說明

### SettingsUISection 屬性的正確用法

根據官方文件：

```csharp
[SettingsUISection("Tab name")]                              // 僅指定 Tab（使用預設 Group）
[SettingsUISection("Tab name", "Group name")]                 // 簡單模式和進階模式相同 Group
[SettingsUISection("Tab name", "Simple group", "Advanced group")] // 不同模式不同 Group
```

**關鍵要點**:
1. **第一個參數永遠是 Tab 名稱**
2. **第二個參數是 Group 名稱**
3. Tab 名稱必須在 `SettingsUITabOrder` 中宣告
4. Group 名稱必須在 `SettingsUIGroupOrder` 中宣告
5. 識別符應使用英文，翻譯透過語系檔案提供

### 語系鍵值命名規則

```
Options.SECTION[完整命名空間.類別名.Tab識別符]      → Tab 標題
Options.GROUP[完整命名空間.類別名.Group識別符]      → Group 標題  
Options.OPTION[完整命名空間.類別名.屬性名]          → 選項名稱
Options.OPTION_DESCRIPTION[完整命名空間.類別名.屬性名] → 選項描述
Options.ENUM[完整命名空間.枚舉名.枚舉值]            → Enum 下拉選單項目
```

**重要**: 識別符（Tab/Group 名稱）應該：
- 使用 PascalCase 英文命名
- 簡潔但具描述性
- 不含空格或特殊字元
- 在整個專案中保持一致

## 驗證清單

測試時請檢查以下項目：

### UI 顯示
- [ ] 頂部 Tab 列表正確顯示（需求控制、服務控制、經濟控制）
- [ ] 每個 Tab 下的 Group 標題正確顯示
- [ ] 所有選項名稱正確顯示
- [ ] 所有選項描述正確顯示
- [ ] 模組名稱顯示為「需求修改器 Demand Modifier」

### 多國語言
- [ ] 英文介面顯示正確
- [ ] 繁體中文介面顯示正確
- [ ] 簡體中文介面顯示正確
- [ ] 日文介面顯示正確（至少 Section/Group 正確）
- [ ] 德文介面顯示正確（至少 Section/Group 正確）
- [ ] 西班牙文介面顯示正確（至少 Section/Group 正確）
- [ ] 法文介面顯示正確（至少 Section/Group 正確）

### 功能測試
- [ ] 切換 Tab 功能正常
- [ ] 開關按鈕切換正常
- [ ] 設定會被正確儲存
- [ ] 重新開啟遊戲後設定保持

## 建置狀態

```
✅ dotnet clean - 成功
✅ dotnet build -c Release - 成功
✅ 無編譯錯誤
✅ 無編譯警告
```

## 後續步驟

1. 在遊戲中測試所有語言的顯示
2. 確認 Tab 切換功能正常
3. 驗證所有設定項目可正常操作
4. 更新版本號並發布

## 參考資源

- [Options UI 官方文件](https://cs2.paradoxwikis.com/Options_UI)
- [Modding Toolchain 官方文件](https://cs2.paradoxwikis.com/Modding_Toolchain)
- [SettingsUISection 屬性說明](https://cs2.paradoxwikis.com/Options_UI#SettingsUISectionAttribute)

## 版本資訊

- 修正版本: 0.0.5 (待發布)
- 相容遊戲版本: 1.2.*
- .NET 版本: .NET Framework 4.8.1
