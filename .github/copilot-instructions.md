# DemandModifier - Cities: Skylines 2 Mod 開發指南

## 專案概述

這是一個 Cities: Skylines 2 遊戲模組，使用 **Harmony 函式庫**攔截並修改遊戲需求系統。三大核心功能：需求控制（已實作）、服務控制（規劃中）、經濟控制（規劃中）。

### 核心架構
- **DemandModifierMod.cs**: IMod 實作，處理模組生命週期和 Harmony 補丁註冊（`OnLoad`/`OnDispose`）
- **DemandModifierSettings.cs**: ModSetting 子類別，定義遊戲內設定 UI（三分頁：需求/服務/經濟）
- **DemandSystemPatch.cs**: 三個 Harmony 補丁類別（住宅/商業/工業），使用 Prefix 在 `OnUpdate()` 執行前修改私有欄位
- **l10n/*.json**: 7 種語言翻譯（en-US, zh-HANT, zh-HANS, ja-JP, de-DE, es-ES, fr-FR）

## 遊戲架構：Unity ECS/DOTS 系統

### Unity DOTS (Data-Oriented Technology Stack)
Cities: Skylines 2 使用 Unity 的 ECS (Entity Component System) 架構，關鍵概念：

1. **System 類別**：遊戲邏輯執行單位
   - `CommercialDemandSystem`, `IndustrialDemandSystem`, `ResidentialDemandSystem` 都是 `SystemBase` 子類別
   - 每幀執行 `OnUpdate()` 方法處理需求計算
   - 使用 `NativeValue<T>` 和 `NativeArray<T>` 作為執行緒安全的資料容器

2. **Job System**：多執行緒平行處理
   - System 類別內部使用 Jobs 進行運算
   - `NativeValue` 和 `NativeArray` 可在 Job 間共享資料
   - Harmony 補丁在主執行緒執行，攔截 `OnUpdate` 可在 Job 執行前修改資料

3. **私有欄位存取**：
   - ECS System 的狀態儲存在私有欄位中（如 `m_BuildingDemand`）
   - 無公開 API 修改這些值 → 必須使用 Harmony 反射存取

### 為何攔截 OnUpdate
```csharp
[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
```
- `OnUpdate` 在每次系統更新時執行（每幀或定期）
- Prefix 補丁在原始邏輯前執行，可預先設定需求值
- 原始邏輯仍會執行，但我們的值會覆蓋其計算結果

## 關鍵技術模式

### Harmony 補丁完整指南

#### 1. Prefix 補丁（本專案使用）
在原始方法執行**前**執行，可修改輸入參數或跳過原方法。**本專案的核心模式**：

```csharp
[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
public class CommercialDemandSystemPatch
{
    // 使用 AccessTools.FieldRef 快取私有欄位參考（效能最佳）
    private static AccessTools.FieldRef<CommercialDemandSystem, NativeValue<int>> BuildingDemandRef =
        AccessTools.FieldRefAccess<CommercialDemandSystem, NativeValue<int>>("m_BuildingDemand");

    static void Prefix(CommercialDemandSystem __instance)
    {
        // ⚠️ 必須顯式 null 檢查（.NET 4.7.2 限制，不可用 null-conditional）
        if (DemandModifierMod.Settings != null && 
            DemandModifierMod.Settings.CommercialDemandLevel != DemandLevel.Off)
        {
            // 修改 NativeValue<int>.value（Unity ECS 執行緒安全容器）
            BuildingDemandRef(__instance).value = (int)DemandModifierMod.Settings.CommercialDemandLevel;
        }
    }
}
```

**關鍵設計決策**：
- 使用 `void` 返回值讓原方法繼續執行（保持遊戲其他邏輯正常）
- `__instance` 是 Harmony 特殊參數，自動注入目標物件實例
- 攔截 `OnUpdate()` 而非 `OnCreate()`，因為需求每幀重新計算
- 住宅需求需修改 3 個欄位：`m_BuildingDemand` (int3)、`m_HouseholdDemand` (int)、11 個因素陣列

#### 2. Postfix 補丁（未使用，但可擴充）
在原始方法執行**後**執行，可修改返回值：
```csharp
static void Postfix(ref int __result)
{
    __result = 255; // 修改返回值
}
```

#### 3. Transpiler 補丁（進階，未使用）
修改方法的 IL 程式碼，用於精細控制執行流程，但複雜且易碎。

#### 4. AccessTools 反射工具
```csharp
// 存取私有欄位（本專案模式）
AccessTools.FieldRefAccess<ClassType, FieldType>("m_FieldName");

// 存取私有方法
AccessTools.Method(typeof(ClassName), "MethodName");

// 存取私有屬性
AccessTools.Property(typeof(ClassName), "PropertyName");
```

#### 5. 條件檢查最佳實踐
```csharp
// ✅ 正確：先檢查 Settings 非 null
if (DemandModifierMod.Settings != null && DemandModifierMod.Settings.EnableXXX == true)

// ❌ 錯誤：可能 NullReferenceException (.NET 4.8.1 限制)
if (DemandModifierMod.Settings?.EnableXXX == true)
```

### 設定系統架構

#### ModSetting 繼承結構
```csharp
[FileLocation(nameof(DemandModifier))] // 設定檔儲存位置
[SettingsUITabOrder("需求控制", "服務控制", "經濟控制")] // 分頁順序
[SettingsUIGroupOrder("住宅需求", "商業需求", ...)] // 群組順序
[SettingsUIShowGroupName(...)] // 顯示群組標題
public class DemandModifierSettings : ModSetting
```

#### 屬性標記模式

##### 1. 布林值開關（Boolean）
```csharp
[SettingsUISection("分頁名稱", "群組名稱")]
public bool EnableFeature { get; set; }
```

##### 2. 下拉選單（Enum Dropdown）- **⭐ 本專案實作範例**

**本專案解決方案**（見 `DemandModifierSettings.cs` 第 55-111 行）：
```csharp
using Game.UI.Widgets; // 必須引用

// 1. 定義 enum（作為值儲存）
public enum DemandLevel { Off = 0, Low = 64, Medium = 128, High = 192, Maximum = 255 }

// 2. 使用 SettingsUIDropdown 指定提供選項的方法
[SettingsUISection("DemandControl", "ResidentialDemand")]
[SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
public DemandLevel ResidentialDemandLevel { get; set; }

// 3. 提供 DropdownItem[] 陣列（displayName 必須是已翻譯字串）
private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
{
    return new DropdownItem<DemandLevel>[]
    {
        new DropdownItem<DemandLevel> { value = DemandLevel.Off, displayName = GetLocalizedEnumName(DemandLevel.Off) },
        new DropdownItem<DemandLevel> { value = DemandLevel.Low, displayName = GetLocalizedEnumName(DemandLevel.Low) },
        new DropdownItem<DemandLevel> { value = DemandLevel.Medium, displayName = GetLocalizedEnumName(DemandLevel.Medium) },
        new DropdownItem<DemandLevel> { value = DemandLevel.High, displayName = GetLocalizedEnumName(DemandLevel.High) },
        new DropdownItem<DemandLevel> { value = DemandLevel.Maximum, displayName = GetLocalizedEnumName(DemandLevel.Maximum) }
    };
}

// 4. 手動讀取語系翻譯（帶降級機制）
private string GetLocalizedEnumName(DemandLevel level)
{
    string localeKey = string.Format("Common.ENUM[DemandModifier.DemandLevel.{0}]", level.ToString());
    try
    {
        if (Game.SceneFlow.GameManager.instance?.localizationManager != null)
        {
            var dict = Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary;
            if (dict != null && dict.TryGetValue(localeKey, out string translated))
                return translated;
        }
    }
    catch { }
    
    // 降級：返回英文預設名稱（帶百分比說明）
    return level switch
    {
        DemandLevel.Off => "Off (Game Default)",
        DemandLevel.Low => "Low (25%)",
        DemandLevel.Medium => "Medium (50%)",
        DemandLevel.High => "High (75%)",
        DemandLevel.Maximum => "Maximum (100%)",
        _ => level.ToString()
    };
}
```

**語系檔案配對**（`l10n/en-US.json` 第 22-26 行）：
```json
"Common.ENUM[DemandModifier.DemandLevel.Off]": "Off",
"Common.ENUM[DemandModifier.DemandLevel.Low]": "Low (25%)",
"Common.ENUM[DemandModifier.DemandLevel.Medium]": "Medium (50%)",
"Common.ENUM[DemandModifier.DemandLevel.High]": "High (75%)",
"Common.ENUM[DemandModifier.DemandLevel.Maximum]": "Maximum (100%)"
```

**關鍵要點**：
- ❌ 直接使用 enum 屬性會顯示 `Common.ENUM[...]` 鍵值而非翻譯
- ✅ 必須手動讀取 `activeDictionary` 並轉換為 `displayName`
- ✅ 降級機制確保語系載入失敗時仍能顯示預設名稱
- ✅ enum 值可直接用於 Harmony 補丁（`(int)DemandLevel.Maximum` = 255）

##### 3. 數值滑桿（Slider）
```csharp
[SettingsUISlider(min = 0, max = 255, step = 1)]
[SettingsUISection("需求控制", "住宅需求")]
public int ResidentialDemandValue { get; set; } = 255;
```

#### 必要命名規則
- 分頁名稱必須在 `SettingsUITabOrder` 中宣告
- 群組名稱必須在 `SettingsUIGroupOrder` 中宣告
- 所有屬性名稱會自動對應到語系檔案的翻譯鍵值（除了 enum 下拉選單需手動處理）

#### 設定生命週期
1. **OnLoad 載入**：
```csharp
Settings = new DemandModifierSettings(this);
Settings.RegisterInOptionsUI(); // 註冊到遊戲 UI
AssetDatabase.global.LoadSettings(nameof(DemandModifier), Settings, new DemandModifierSettings(this));
```

2. **全域存取**：
```csharp
DemandModifierMod.Settings.EnableResidentialDemand // 靜態屬性，任何地方可存取
```

3. **自動儲存**：設定變更時遊戲自動儲存到 `%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\ModsSettings\DemandModifier.coc`

4. **OnDispose 清理**：
```csharp
Settings.UnregisterInOptionsUI();
Settings = null;
```

### .NET Framework 4.7.2 語法限制與轉換

⚠️ **本專案使用 .NET Framework 4.7.2**（見 `DemandModifier.csproj` 第 7 行：`<TargetFramework>net472</TargetFramework>`）

#### 禁止使用的現代 C# 語法

| 現代語法 (.NET 8) | .NET 4.7.2 替代方案 | 原因 |
|-------------------|---------------------|------|
| `[item1, item2]` | `new Type[] { item1, item2 }` | 集合表達式不支援 |
| `$"{nameof(X)}.{nameof(Y)}"` | `string.Format("{0}.{1}", nameof(X), nameof(Y))` | 編譯時字串插值問題 |
| `Settings?.Property == true` | `Settings != null && Settings.Property == true` | null-conditional 在條件中失效 |
| `record class` | `class` with manual equality | Records 不支援 |
| `init` accessor | `set` accessor | init-only 屬性不支援 |
| File-scoped namespace | Block-scoped namespace | C# 10 特性 |
| `required` modifier | Constructor validation | C# 11 特性 |

#### 實際轉換範例（本專案已遵循）

**✅ 相容程式碼**（見 `DemandSystemPatch.cs`）：
```csharp
// 明確陣列初始化（第 96-108 行）
private static DemandFactor[] Factors = new DemandFactor[]
{
    DemandFactor.StorageLevels,
    DemandFactor.EducatedWorkforce,
    // ...
};

// 顯式 null 檢查（第 116-117 行）
if (DemandModifierMod.Settings != null && 
    DemandModifierMod.Settings.ResidentialDemandLevel != DemandLevel.Off)
{
    // 安全存取
}

// string.Format（DemandModifierMod.cs 第 19 行）
public static ILog log = LogManager.GetLogger($"{nameof(DemandModifier)}.{nameof(DemandModifierMod)}").SetShowsErrorsInUI(false);
```

## 多國語系系統

### 檔案結構與命名規則
```
l10n/
├── en-US.json      # 英文（美國）
├── zh-HANT.json    # 繁體中文（台灣）
├── zh-HANS.json    # 简体中文（中國）
├── ja-JP.json      # 日文
├── de-DE.json      # 德文
├── es-ES.json      # 西班牙文
└── fr-FR.json      # 法文
```

### 語系鍵值完整格式

#### 1. 分頁標題（Tab）
```json
"Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.需求控制]": "Demand Control"
```
- `Options.SECTION` = 固定前綴
- `[完整命名空間.類別名稱.分頁名稱]` = 唯一識別符
- **注意**：分頁名稱來自 `SettingsUISection` 的第一個參數

#### 2. 群組名稱（Group）
```json
"Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.住宅需求]": "Residential Demand"
```
- `Options.GROUP` = 固定前綴
- 群組名稱來自 `SettingsUISection` 的第二個參數

#### 3. 選項標題（Option）
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableResidentialDemand]": "Enable Max Residential Demand"
```
- `Options.OPTION` = 固定前綴
- 使用屬性的實際名稱（EnableResidentialDemand）

#### 4. 選項描述（Description）
```json
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableResidentialDemand]": "Always keep residential demand at maximum for all density types"
```
- `Options.OPTION_DESCRIPTION` = 固定前綴
- 提供詳細說明，顯示在設定項目下方

### 語系載入機制
遊戲引擎自動處理，無需額外程式碼：
1. 掃描模組根目錄的 `l10n/` 資料夾
2. 根據玩家的遊戲語言設定載入對應 JSON
3. 自動套用翻譯到 UI 元件
4. 若找不到對應語言，降級到 `en-US.json`

### 新增翻譯的步驟
1. 在 `DemandModifierSettings.cs` 加入新屬性
2. 在**全部 7 個** `l10n/*.json` 檔案加入 4 個鍵值（若有新分頁/群組）：
   - `SECTION` (如需新分頁)
   - `GROUP` (如需新群組)
   - `OPTION`
   - `OPTION_DESCRIPTION`
3. 使用線上翻譯工具確保品質（DeepL, Google Translate）
4. 建置並在遊戲中切換語言測試

## 建置與發佈流程

### 環境設定

#### 必要環境變數
```powershell
# 設定 Cities: Skylines 2 Modding SDK 路徑
[System.Environment]::SetEnvironmentVariable('CSII_TOOLPATH', 'C:\Path\To\CS2ModdingSDK', 'User')
```

SDK 提供的檔案（在 `.csproj` 第 11-12 行自動引用）：
- **Mod.props**: 遊戲 DLL 參考、編譯設定
- **Mod.targets**: 發佈邏輯、PDX Mods 整合

### 建置命令

```powershell
# Release 建置（發佈用）- 目標 .NET Framework 4.7.2
dotnet build -c Release

# 清理建置產物
dotnet clean
```

### 快速測試部署

**使用專案腳本**（推薦，見 `scripts/test-deploy.ps1`）：
```powershell
cd DemandModifier
.\scripts\test-deploy.ps1          # 建置並部署到遊戲目錄
.\scripts\test-deploy.ps1 -Clean   # 清理後重新建置部署
.\scripts\test-deploy.ps1 -SkipBuild  # 僅部署（不重新建置）
```

**腳本功能**：
1. 建置 Release 版本
2. 驗證建置產物（DLL + 7 個語系檔案）
3. 部署到 `%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier`
4. 顯示檔案清單和測試步驟提示
5. 可選開啟遊戲日誌監控

建置產物位置：
```
bin/Release/net48/
├── DemandModifier.dll     # 主模組 DLL
└── l10n/                  # 自動複製的 7 個語系檔案
    ├── en-US.json
    ├── zh-HANT.json
    ├── zh-HANS.json
    ├── ja-JP.json
    ├── de-DE.json
    ├── es-ES.json
    └── fr-FR.json
```

### 發佈到 PDX Mods

#### 1. PublishNewMod.pubxml（首次發佈）
```powershell
cd DemandModifier
dotnet publish /p:PublishProfile=PublishNewMod
```
- 建立新模組，獲得 ModId（本專案 ModId: 123170）
- 發佈後更新 `Properties/PublishConfiguration.xml` 的 `<ModId>` 欄位

#### 2. PublishNewVersion.pubxml（更新版本）
```powershell
dotnet publish /p:PublishProfile=PublishNewVersion
```
- 發佈現有模組的新版本（需要正確的 ModId）
- 更新前必須修改 `<ModVersion>` 和 `<ChangeLog>`

#### PublishConfiguration.xml 關鍵欄位（見 `Properties/PublishConfiguration.xml`）
```xml
<Publish>
    <ModId Value="123170" />  <!-- PDX Mods 模組 ID -->
    <DisplayName Value="需求修改器 Demand Modifier (Beta)" />
    <ModVersion Value="0.2.1" />  <!-- 當前版本：0.2.1 -->
    <GameVersion Value="1.3.*" />  <!-- 相容遊戲版本 -->
    <ShortDescription Value="全面的城市需求、服務與經濟控制模組" />
    <LongDescription>...</LongDescription>  <!-- 支援 Markdown -->
    <Thumbnail Value="Properties/Thumbnail.png" />
    <ChangeLog Value="v0.2.1 - Fixed UI dropdown menu localization display issue" />
    <ExternalLink Type="github" Url="https://github.com/ChengBoChuan/demandmodifier" />
</Publish>
```

**自動版本更新**（見 `Properties/UpdateVersion.targets`）：
- 建置時自動同步版本號到 `PublishConfiguration.xml`
- 確保 `.csproj` 的 `<Version>` 與發佈版本一致

## 專案結構慣例

### 重要目錄與檔案

```
DemandModifier/
├── DemandModifier.csproj           # 專案檔（參考、建置設定）
├── DemandModifierMod.cs            # IMod 入口點
├── DemandModifierSettings.cs       # ModSetting 設定類別
├── DemandSystemPatch.cs            # Harmony 補丁實作
├── l10n/                           # 多國語系資料夾（7 種語言）
├── Properties/
│   ├── PublishConfiguration.xml    # PDX Mods Metadata
│   └── PublishProfiles/
│       ├── PublishNewMod.pubxml           # 首次發佈
│       ├── PublishNewVersion.pubxml       # 版本更新
│       └── UpdatePublishedConfiguration.pubxml # Metadata 更新
└── bin/
    └── [Debug|Release]/net48/
        ├── DemandModifier.dll      # 編譯產物
        └── l10n/                   # 自動複製的語系檔案
```

### csproj 關鍵設定

#### 1. SDK 整合
```xml
<Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.props" />
<Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.targets" />
```

#### 2. 遊戲 DLL 參考
```xml
<Reference Include="Game">
    <Private>false</Private>  <!-- 不複製到輸出目錄 -->
</Reference>
```
- **為何 `Private=false`**：遊戲 DLL 已存在於遊戲目錄，複製會造成版本衝突

#### 3. 語系檔案自動複製
```xml
<ItemGroup>
    <None Include="l10n\**\*.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```
- `**` = 遞迴所有子目錄
- `PreserveNewest` = 僅在檔案較新時複製

## 除錯與測試策略

### 日誌記錄最佳實踐

#### 建立 Logger（見 `DemandModifierMod.cs` 第 19 行）
```csharp
using Colossal.Logging;

public static ILog log = LogManager.GetLogger($"{nameof(DemandModifier)}.{nameof(DemandModifierMod)}").SetShowsErrorsInUI(false);
```

**SetShowsErrorsInUI(false) 的重要性**：
- `false`：錯誤僅寫入日誌檔，適合正式版本（本專案設定）
- `true`：錯誤會跳出遊戲內通知，干擾玩家體驗

#### 查看日誌
日誌檔位置：
```
%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\
├── Player.log          # 主日誌
└── Player-prev.log     # 上次啟動的日誌
```

即時監控（PowerShell，`test-deploy.ps1` 提供互動選項）：
```powershell
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Wait -Tail 50
```

### 在遊戲中測試模組（`test-deploy.ps1` 自動提示的步驟）

#### 驗證檢查清單
**設定 UI 測試**：
- [ ] 所有分頁正確顯示（需求控制/服務控制/經濟控制）
- [ ] 下拉選單顯示 5 個翻譯選項，**不是**語系鍵值（如 `Common.ENUM[...]`）
- [ ] 切換 7 種語言測試翻譯

**遊戲內功能測試**：
- [ ] 住宅需求設為 Maximum，觀察需求立即變滿
- [ ] 設為 Off，需求恢復正常波動
- [ ] 商業/工業需求同樣測試
- [ ] 檢查日誌無 Harmony 錯誤

#### 常見問題排查

| 問題 | 可能原因 | 解決方案 |
|------|----------|----------|
| 模組未出現在列表中 | DLL 路徑錯誤 | 確認 `Mods\DemandModifier\DemandModifier.dll` |
| 下拉選單顯示 `Common.ENUM[...]` | `GetLocalizedEnumName()` 未實作 | 檢查 `GetDemandLevelOptions()` 回傳 `displayName` |
| 需求未改變 | Harmony 補丁失敗 | 查看日誌中的 Harmony 錯誤訊息 |
| 遊戲崩潰 | 欄位名稱錯誤 | 用 dnSpy 反編譯 `Game.dll` 確認私有欄位名 |
| 語系檔案遺失 | `.csproj` 未設定自動複製 | 檢查第 73-76 行 `<CopyToOutputDirectory>` |

## 新增功能完整流程

### 範例：新增「無限電力」功能

#### 步驟 1：加入設定選項
`DemandModifierSettings.cs`（已預留，見第 147-154 行）：
```csharp
[SettingsUISection("ServiceControl", "ServiceSettings")]
public bool EnableUnlimitedElectricity { get; set; }
```

#### 步驟 2：更新所有語系檔案（7 個）
`l10n/en-US.json`（已預留，見第 28-29 行）：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Unlimited Electricity",
  "Options.OPTION_DESCRIPTION[...]": "Never run out of electricity - all buildings receive power"
}
```

重複更新：`zh-HANT.json`, `zh-HANS.json`, `ja-JP.json`, `de-DE.json`, `es-ES.json`, `fr-FR.json`

#### 步驟 3：建立 Harmony 補丁
新增檔案 `ElectricitySystemPatch.cs`：
```csharp
using HarmonyLib;
using Game.Simulation;
using Unity.Collections;

namespace DemandModifier
{
    [HarmonyPatch(typeof(ElectricityFlowSystem), "OnUpdate")]
    public class ElectricityFlowSystemPatch
    {
        // 1. 使用 dnSpy 找到私有欄位名稱
        private static AccessTools.FieldRef<ElectricityFlowSystem, NativeValue<int>> AvailabilityRef =
            AccessTools.FieldRefAccess<ElectricityFlowSystem, NativeValue<int>>("m_Availability");

        static void Prefix(ElectricityFlowSystem __instance)
        {
            // 2. 顯式 null 檢查（.NET 4.7.2 要求）
            if (DemandModifierMod.Settings != null && 
                DemandModifierMod.Settings.EnableUnlimitedElectricity == true)
            {
                AvailabilityRef(__instance).value = int.MaxValue;
            }
        }
    }
}
```

#### 步驟 4：測試
```powershell
cd DemandModifier
.\scripts\test-deploy.ps1
# 遊戲內啟用「無限電力」，觀察建築供電狀態
```

#### 步驟 5：更新版本
`PublishConfiguration.xml`：
```xml
<ModVersion Value="0.3.0" />  <!-- Minor 版本升級 -->
<ChangeLog Value="v0.3.0 - Added unlimited electricity feature" />
```

### 檢查清單

- [ ] `DemandModifierSettings.cs` 加入屬性 + `[SettingsUISection]` 標記
- [ ] 更新 7 個 `l10n/*.json` 檔案（OPTION + OPTION_DESCRIPTION）
- [ ] 建立 `[HarmonyPatch]` 類別，實作 Prefix 邏輯
- [ ] 使用 `AccessTools.FieldRefAccess` 存取私有欄位
- [ ] 加入 `Settings != null && Settings.Property == true` 檢查
- [ ] 更新 `PublishConfiguration.xml`（ModVersion, ChangeLog）
- [ ] 本機測試所有語言的 UI 顯示
- [ ] 遊戲內測試功能是否生效
- [ ] 檢查日誌確認無錯誤
- [ ] 提交程式碼並標記版本

## 遊戲版本相容性

### 目前支援版本
**Cities: Skylines 2 v1.3.***（見 `PublishConfiguration.xml` 第 86 行）

### 版本更新影響評估

#### 高風險變更（遊戲更新時需檢查）
1. **內部欄位名稱變更**
   - 範例：`m_BuildingDemand` → `m_Demand`
   - 影響：`AccessTools.FieldRefAccess` 呼叫失敗（見 `DemandSystemPatch.cs` 第 13-16, 35-43, 82-99 行）
   - 修復：使用 dnSpy 反編譯 `Game.dll`，查找新欄位名

2. **System 類別重構**
   - 範例：`CommercialDemandSystem` 被拆分
   - 影響：`[HarmonyPatch]` 無法找到目標類別
   - 修復：更新補丁目標類別

3. **住宅需求因素陣列變更**（見 `DemandSystemPatch.cs` 第 96-108 行）
   - 當前 11 個因素：StorageLevels, EducatedWorkforce, CompanyWealth, 等
   - 若遊戲新增/移除因素，需更新 `Factors` 陣列

### 反編譯工具（查找私有欄位）
- **dnSpy**：https://github.com/dnSpy/dnSpy（推薦）
- **ILSpy**：https://github.com/icsharpcode/ILSpy

反編譯步驟：
1. 找到 `[Steam]\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll`
2. 用 dnSpy 開啟，搜尋 `CommercialDemandSystem`
3. 查看私有欄位（如 `m_BuildingDemand`）
4. 更新 `DemandSystemPatch.cs` 中的欄位名

## 進階技巧

### 動態調整需求值
目前是固定 255，可改為可調整的滑桿：
```csharp
// DemandModifierSettings.cs
[SettingsUISlider(min = 0, max = 255, step = 1)]
[SettingsUISection("需求控制", "住宅需求")]
public int ResidentialDemandValue { get; set; } = 255;

// Patch
BuildingDemandRef(__instance).value = DemandModifierMod.Settings.ResidentialDemandValue;
```

### 條件式補丁（Harmony Transpiler）
若要精細控制執行流程，可使用 Transpiler 修改 IL：
```csharp
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    // 進階：修改方法的 IL 程式碼
    // 警告：容易因遊戲更新而失效
}
```

### 多個補丁協作
若需修改多個相關系統，確保補丁執行順序：
```csharp
[HarmonyPatch(typeof(SystemA), "Method")]
[HarmonyPriority(Priority.First)] // 先執行
public class PatchA { }

[HarmonyPatch(typeof(SystemB), "Method")]
[HarmonyPriority(Priority.Last)] // 後執行
public class PatchB { }
```

## 參考資源

- **Harmony 文件**：https://harmony.pardeike.net/
- **Cities: Skylines 2 Modding Discord**：官方 Modding 社群
- **Unity ECS 文件**：https://docs.unity3d.com/Packages/com.unity.entities@latest
- **C# 語言版本對照**：https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/

