# DemandModifier - Cities: Skylines 2 模組開發完整指南 (基於 Traffic 專案最佳實踐)

## 📋 目錄
1. [專案概述](#專案概述)
2. [核心架構](#核心架構)
3. [設定系統深度解析](#設定系統深度解析)
4. [Harmony 補丁系統](#harmony-補丁系統)
5. [多國語系系統](#多國語系系統)
6. [建置與發佈](#建置與發佈)
7. [除錯與測試](#除錯與測試)
8. [參考專案分析](#參考專案分析)

---

## 專案概述

### 模組功能
**DemandModifier** 是一個 Cities: Skylines 2 遊戲模組，提供以下三大核心功能：

1. **需求控制 (Demand Control)**
   - 住宅需求 (Residential Demand)
   - 商業需求 (Commercial Demand)
   - 工業需求 (Industrial Demand)
   - 支援五級強度：關閉/低/中/高/最大

2. **服務控制 (Service Control)** ⚠️ 規劃中
   - 無限電力、水、污水處理
   - 無限垃圾處理、醫療、教育
   - 無限警力、消防服務

3. **經濟控制 (Economy Control)** ⚠️ 規劃中
   - 無限金錢
   - 免費建造
   - 零維護成本

### 技術棧
- **框架**: .NET Framework 4.8.1
- **語言**: C# 9.0
- **目標平台**: Cities: Skylines 2 v1.2.*
- **修改技術**: Harmony 2.x (Runtime Patching)
- **遊戲架構**: Unity ECS/DOTS

### 核心檔案結構
```
DemandModifier/
├── DemandModifierMod.cs          # IMod 入口點，Harmony 補丁註冊
├── DemandModifierSettings.cs     # ModSetting 設定類別
├── DemandSystemPatch.cs          # 需求系統 Harmony 補丁
├── l10n/                         # 多國語系資料夾
│   ├── en-US.json               # 英文
│   ├── zh-HANT.json             # 繁體中文
│   ├── zh-HANS.json             # 简体中文
│   ├── ja-JP.json               # 日文
│   ├── de-DE.json               # 德文
│   ├── es-ES.json               # 西班牙文
│   └── fr-FR.json               # 法文
└── Properties/
    ├── PublishConfiguration.xml  # PDX Mods 發佈設定
    └── PublishProfiles/          # 發佈配置檔
```

---

## 核心架構

### Unity ECS/DOTS 系統深度解析

Cities: Skylines 2 使用 Unity 的 **DOTS (Data-Oriented Technology Stack)**，這是一個面向資料的多執行緒架構：

#### 1. System 生命週期
```csharp
public partial class DemandSystem : SystemBase
{
    private NativeValue<int> m_BuildingDemand;  // 執行緒安全的資料容器
    
    protected override void OnCreate()
    {
        // 系統初始化，建立資料容器
        m_BuildingDemand = new NativeValue<int>(Allocator.Persistent);
    }
    
    protected override void OnUpdate()
    {
        // 每幀執行，處理需求計算
        CalculateDemand();  // 內部使用 Job System 平行運算
    }
    
    protected override void OnDestroy()
    {
        // 清理資源
        m_BuildingDemand.Dispose();
    }
}
```

#### 2. 為何攔截 OnUpdate
```csharp
[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
public class CommercialDemandSystemPatch
{
    static void Prefix(CommercialDemandSystem __instance)
    {
        // ✅ 在原始計算邏輯執行前修改資料
        // ✅ 原始方法仍會執行（保持其他遊戲邏輯）
        // ✅ 我們的值會覆蓋計算結果
    }
}
```

**關鍵時機**：
- `Prefix` 在 `OnUpdate` 開始前執行
- 修改 `NativeValue<int> m_BuildingDemand` 的值
- 遊戲計算邏輯執行後讀取我們設定的值

#### 3. Job System 與多執行緒
```
主執行緒                         工作執行緒 (Jobs)
    │                                 │
    ├─> OnUpdate()                   │
    │   ├─> [Harmony Prefix] ───> 修改 m_BuildingDemand
    │   │                            │
    │   ├─> ScheduleJobs() ─────────┼─> Job 1: 計算人口
    │   │                            ├─> Job 2: 計算經濟
    │   │                            └─> Job 3: 計算交通
    │   │                            │
    │   └─> CompleteJobs() <─────────┴─ 合併結果
    │                                 │
    └─> 讀取 m_BuildingDemand         │
```

**為何 Prefix 有效**：
- Harmony 補丁在主執行緒執行
- 修改發生在 Jobs 排程前
- Jobs 讀取已修改的值

---

## 設定系統深度解析

### ModSetting 架構完整指南

#### 1. 類別標記 (Class Attributes)
```csharp
[FileLocation(nameof(DemandModifier))]  // 設定檔名稱: DemandModifier.coc
[SettingsUITabOrder("DemandControl", "ServiceControl", "EconomyControl")]  // 分頁順序
[SettingsUIGroupOrder("ResidentialDemand", "CommercialDemand", "IndustrialDemand")]  // 群組順序
[SettingsUIShowGroupName("ResidentialDemand", "CommercialDemand")]  // 顯示群組標題
public class DemandModifierSettings : ModSetting
{
    public DemandModifierSettings(IMod mod) : base(mod) { }
}
```

**標記說明**：
- `FileLocation`: 設定檔儲存名稱（`.coc` 二進位格式）
- `TabOrder`: 分頁在 UI 中的顯示順序（左到右）
- `GroupOrder`: 群組在分頁內的顯示順序（上到下）
- `ShowGroupName`: 是否顯示群組標題（預設不顯示）

#### 2. 屬性標記模式全集

##### 布林值開關 (Boolean Checkbox)
```csharp
[SettingsUISection("ServiceControl", "ServiceSettings")]
public bool EnableUnlimitedElectricity { get; set; }
```
**翻譯鍵值**（自動對應）：
- `Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]`
- `Options.OPTION_DESCRIPTION[...]`

##### 數值滑桿 (Numeric Slider)
```csharp
[SettingsUISlider(min = 0f, max = 2f, step = 0.1f, unit = Unit.kFloatSingleFraction)]
[SettingsUISection("Overlays", "Style")]
public float ConnectorSize { get; set; } = 1.0f;
```
**參數說明**：
- `min`/`max`: 數值範圍
- `step`: 調整間隔
- `unit`: 顯示單位（百分比、整數、浮點等）

##### 下拉選單 (Enum Dropdown) - ⭐ 關鍵實作
**❌ 錯誤做法**（會顯示未翻譯的語系鍵值）：
```csharp
[SettingsUISection("DemandControl", "ResidentialDemand")]
public DemandLevel ResidentialDemandLevel { get; set; }
// UI 顯示: "Common.ENUM[DemandModifier.DemandLevel.Off]"
```

**✅ 正確做法**（參考 Traffic 專案）：
```csharp
using Game.UI.Widgets;  // 必須引用

[SettingsUISection("DemandControl", "ResidentialDemand")]
[SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
public DemandLevel ResidentialDemandLevel { get; set; }

private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
{
    return new DropdownItem<DemandLevel>[]
    {
        new DropdownItem<DemandLevel> 
        { 
            value = DemandLevel.Off,
            displayName = GetLocalizedEnumName(DemandLevel.Off)  // 翻譯後的字串
        },
        new DropdownItem<DemandLevel> 
        { 
            value = DemandLevel.Maximum,
            displayName = GetLocalizedEnumName(DemandLevel.Maximum)
        }
    };
}

private string GetLocalizedEnumName(DemandLevel level)
{
    string localeKey = $"Common.ENUM[DemandModifier.DemandLevel.{level}]";
    
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
    
    // 降級機制：返回英文預設名稱
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

**關鍵要點**：
1. `displayName` 必須是**已翻譯的字串**，不能是語系鍵值
2. 使用 `GameManager.instance.localizationManager.activeDictionary` 讀取翻譯
3. 必須提供降級機制（語系載入失敗時顯示英文）
4. 遊戲引擎**不會自動**翻譯 enum，必須手動處理

##### 按鈕 (Button Action)
```csharp
[SettingsUIButton]
[SettingsUIConfirmation]  // 顯示確認對話框
[SettingsUISection("LaneConnector", "Actions")]
public bool ResetLaneConnections
{
    set 
    {
        // 按鈕點擊時執行
        World.DefaultGameObjectInjectionWorld
            .GetExistingSystemManaged<LaneConnectorToolSystem>()
            .ResetAllConnections();
    }
}
```

##### 條件隱藏/停用
```csharp
[SettingsUIDisableByCondition(typeof(ModSettings), nameof(UseGameLanguage))]
public string CurrentLocale { get; set; }

public bool UseGameLanguage { get; set; }  // 用於條件判斷
```

#### 3. 設定生命週期管理

```csharp
// === OnLoad 階段 ===
public void OnLoad(UpdateSystem updateSystem)
{
    // 1. 建立設定實例
    Settings = new DemandModifierSettings(this);
    
    // 2. 註冊到遊戲 UI
    Settings.RegisterInOptionsUI();
    
    // 3. 從磁碟載入設定（或使用預設值）
    AssetDatabase.global.LoadSettings(
        nameof(DemandModifier),  // 資產名稱
        Settings,                // 目標實例
        new DemandModifierSettings(this)  // 預設值
    );
    
    log.Info($"設定已載入: {Settings.ResidentialDemandLevel}");
}

// === OnDispose 階段 ===
public void OnDispose()
{
    Settings?.UnregisterInOptionsUI();
    Settings = null;
}
```

**設定檔位置**：
```
%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\ModsSettings\DemandModifier.coc
```

---

## Harmony 補丁系統

### 完整補丁模式指南

#### 1. Prefix 補丁（本專案使用）

**用途**：在原始方法執行**前**攔截，修改輸入或跳過執行

```csharp
using HarmonyLib;
using Game.Simulation;
using Unity.Collections;

[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
public class CommercialDemandSystemPatch
{
    // 反射存取私有欄位
    private static AccessTools.FieldRef<CommercialDemandSystem, NativeValue<int>> BuildingDemandRef =
        AccessTools.FieldRefAccess<CommercialDemandSystem, NativeValue<int>>("m_BuildingDemand");

    static void Prefix(CommercialDemandSystem __instance)
    {
        // 條件檢查（必須顯式 null 檢查，不可用 null-conditional）
        if (DemandModifierMod.Settings != null && 
            DemandModifierMod.Settings.CommercialDemandLevel != DemandLevel.Off)
        {
            // 修改私有欄位的值
            BuildingDemandRef(__instance).value = (int)DemandModifierMod.Settings.CommercialDemandLevel;
        }
    }
}
```

**特殊參數名稱**（Harmony 自動注入）：
- `__instance`: 被攔截的物件實例
- `__result`: 原始方法的返回值（Postfix 可用）
- `__state`: 在 Prefix 和 Postfix 間傳遞資料
- `__originalMethod`: 被攔截的原始方法資訊

**返回值控制**：
```csharp
static bool Prefix(...)
{
    // 返回 false = 跳過原始方法執行
    // 返回 true = 繼續執行原始方法
    return false;
}
```

#### 2. Postfix 補丁

**用途**：在原始方法執行**後**修改結果

```csharp
[HarmonyPatch(typeof(EconomySystem), "CalculateProfit")]
static class EconomySystemPatch
{
    static void Postfix(ref int __result)
    {
        if (DemandModifierMod.Settings.EnableUnlimitedMoney)
            __result = 1000000;  // 覆寫返回值
    }
}
```

#### 3. AccessTools 反射工具箱

```csharp
// 存取私有欄位（推薦：使用 FieldRef 快取）
var fieldRef = AccessTools.FieldRefAccess<ClassName, FieldType>("m_FieldName");
fieldRef(instance) = newValue;

// 存取私有方法
var method = AccessTools.Method(typeof(ClassName), "MethodName");
method.Invoke(instance, new object[] { args });

// 存取私有屬性
var property = AccessTools.Property(typeof(ClassName), "PropertyName");
property.SetValue(instance, value);

// 存取內部類型
var innerType = AccessTools.Inner(typeof(OuterClass), "InnerClass");
```

#### 4. 補丁優先順序

```csharp
[HarmonyPatch(typeof(SystemA), "Method")]
[HarmonyPriority(Priority.First)]  // 最先執行
public class PatchA { }

[HarmonyPatch(typeof(SystemB), "Method")]
[HarmonyPriority(Priority.Last)]   // 最後執行
public class PatchB { }

[HarmonyPriority(800)]  // 自訂優先順序（預設 400）
```

#### 5. 多個方法的批次補丁

```csharp
[HarmonyPatch]
public class MultiMethodPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(System1), "OnUpdate");
        yield return AccessTools.Method(typeof(System2), "OnUpdate");
        yield return AccessTools.Method(typeof(System3), "OnUpdate");
    }

    static void Prefix(object __instance)
    {
        // 對所有目標方法套用相同邏輯
    }
}
```

### .NET 4.8.1 語法限制與轉換

#### 禁止使用的現代語法

| ❌ 現代語法 (C# 10+) | ✅ .NET 4.8.1 替代方案 |
|---------------------|----------------------|
| `[item1, item2]` | `new Type[] { item1, item2 }` |
| `file-scoped namespace` | `namespace X { }` |
| `record class` | `class` + 手動實作 |
| `init` accessor | `set` accessor |
| `required` modifier | Constructor validation |
| `${expr1}{expr2}` 混合插值 | `string.Format()` |

#### 實際轉換範例

**❌ 不相容程式碼**：
```csharp
// 1. 集合表達式
var items = [1, 2, 3];

// 2. File-scoped namespace
namespace DemandModifier;

// 3. null-conditional 在條件中（.NET 4.8.1 bug）
if (Settings?.EnableFeature == true) { }

// 4. 字串插值 + nameof 混合
log.Info($"{nameof(Mod)}.{nameof(Class)}");
```

**✅ 相容程式碼**：
```csharp
// 1. 明確陣列初始化
var items = new int[] { 1, 2, 3 };

// 2. Block-scoped namespace
namespace DemandModifier
{
    // code
}

// 3. 顯式 null 檢查
if (Settings != null && Settings.EnableFeature == true) { }

// 4. string.Format
log.Info(string.Format("{0}.{1}", nameof(Mod), nameof(Class)));
```

---

## 多國語系系統

### 語系檔案結構

```
l10n/
├── en-US.json      # 英文（美國）- 必須提供
├── zh-HANT.json    # 繁體中文（台灣）
├── zh-HANS.json    # 简体中文（中國）
├── ja-JP.json      # 日文
├── de-DE.json      # 德文
├── es-ES.json      # 西班牙文
└── fr-FR.json      # 法文
```

### 語系鍵值格式完整規範

#### 1. 分頁標題 (Tab)
```json
"Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandControl]": "Demand Control"
```
**格式**: `Options.SECTION[<Namespace>.<ClassName>.<TabName>]`

#### 2. 群組名稱 (Group)
```json
"Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemand]": "Residential Demand"
```
**格式**: `Options.GROUP[<Namespace>.<ClassName>.<GroupName>]`

#### 3. 選項標題 (Option Label)
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Unlimited Electricity"
```
**格式**: `Options.OPTION[<Namespace>.<ClassName>.<PropertyName>]`

#### 4. 選項描述 (Option Description)
```json
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Never run out of electricity - all buildings receive power"
```
**格式**: `Options.OPTION_DESCRIPTION[<Namespace>.<ClassName>.<PropertyName>]`

#### 5. Enum 值翻譯（手動讀取）
```json
"Common.ENUM[DemandModifier.DemandLevel.Off]": "Off (Game Default)",
"Common.ENUM[DemandModifier.DemandLevel.Low]": "Low (25%)",
"Common.ENUM[DemandModifier.DemandLevel.Medium]": "Medium (50%)",
"Common.ENUM[DemandModifier.DemandLevel.High]": "High (75%)",
"Common.ENUM[DemandModifier.DemandLevel.Maximum]": "Maximum (100%)"
```
**格式**: `Common.ENUM[<Namespace>.<EnumTypeName>.<EnumValueName>]`

⚠️ **注意**：Enum 翻譯需要手動實作 `GetLocalizedEnumName()` 方法讀取

### 語系載入機制

**自動載入**（遊戲引擎處理）：
1. 掃描 `<ModRoot>/l10n/*.json`
2. 根據玩家語言設定載入對應檔案
3. 自動套用到 ModSetting 屬性
4. 若找不到對應語言，降級到 `en-US.json`

**手動載入**（參考 Traffic 專案）：
```csharp
public void OnLoad(UpdateSystem updateSystem)
{
    // 1. 註冊預設英文語系
    if (!GameManager.instance.localizationManager.activeDictionary.ContainsID(Settings.GetSettingsLocaleID()))
    {
        var source = new LocaleEN(Settings);
        GameManager.instance.localizationManager.AddSource("en-US", source);
        
        // 2. 動態載入其他語系
        LoadLocales(this, source.ReadEntries(null, null).Count());
    }
}

internal static void LoadLocales(IMod mod, float refTranslationCount)
{
    if (GameManager.instance.modManager.TryGetExecutableAsset(mod, out var asset))
    {
        string directory = Path.Combine(Path.GetDirectoryName(asset.path), "Localization");
        if (Directory.Exists(directory))
        {
            foreach (string localeFile in Directory.EnumerateFiles(directory, "*.json"))
            {
                string localeId = Path.GetFileNameWithoutExtension(localeFile);
                ModLocale locale = new ModLocale(localeId, localeFile).Load(refTranslationCount);
                GameManager.instance.localizationManager.AddSource(localeId, locale);
            }
        }
    }
}
```

### 完整翻譯範例（en-US.json）

```json
{
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandControl]": "Demand Control",
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.ServiceControl]": "Service Control",
  
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemand]": "Residential Demand",
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ServiceSettings]": "Service Settings",
  
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "Residential Demand Level",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "Control the level of residential zone demand (Off = game default, Maximum = always max demand)",
  
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Unlimited Electricity",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Never run out of electricity - all buildings receive power",
  
  "Common.ENUM[DemandModifier.DemandLevel.Off]": "Off (Game Default)",
  "Common.ENUM[DemandModifier.DemandLevel.Low]": "Low (25%)",
  "Common.ENUM[DemandModifier.DemandLevel.Medium]": "Medium (50%)",
  "Common.ENUM[DemandModifier.DemandLevel.High]": "High (75%)",
  "Common.ENUM[DemandModifier.DemandLevel.Maximum]": "Maximum (100%)"
}
```

---

## 建置與發佈

### 環境設定

#### 1. Cities: Skylines 2 Modding SDK
```powershell
# 設定環境變數（永久）
[System.Environment]::SetEnvironmentVariable(
    'CSII_TOOLPATH', 
    'C:\Path\To\CS2ModdingSDK', 
    'User'
)

# 驗證設定
$env:CSII_TOOLPATH  # 應顯示 SDK 路徑
```

**SDK 提供的檔案**：
- `Mod.props`: 遊戲 DLL 參考、編譯設定
- `Mod.targets`: 發佈邏輯、PDX Mods 整合

#### 2. .csproj 關鍵設定

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net48</TargetFramework>
        <LangVersion>9</LangVersion>  <!-- C# 9.0 -->
        <Configurations>Debug;Release</Configurations>
        <PublishConfigurationPath>Properties\PublishConfiguration.xml</PublishConfigurationPath>
    </PropertyGroup>

    <!-- SDK 整合 -->
    <Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.props" />
    <Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.targets" />

    <!-- 遊戲 DLL 參考 -->
    <ItemGroup>
        <Reference Include="Game">
            <Private>false</Private>  <!-- 不複製到輸出目錄 -->
        </Reference>
        <Reference Include="Colossal.Logging">
            <Private>false</Private>
        </Reference>
        <Reference Include="Unity.Entities">
            <Private>false</Private>
        </Reference>
    </ItemGroup>

    <!-- 語系檔案自動複製 -->
    <ItemGroup>
        <None Include="l10n\**\*.json">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>
</Project>
```

**為何 `Private=false`**：
- 遊戲 DLL 已存在於遊戲目錄
- 複製會造成版本衝突和載入失敗

### 建置命令

```powershell
# Debug 建置（開發用，包含符號）
dotnet build -c Debug

# Release 建置（發佈用，最佳化）
dotnet build -c Release

# 清理建置產物
dotnet clean
```

**輸出位置**：
```
bin/
├── Debug/net48/
│   ├── DemandModifier.dll
│   ├── DemandModifier.pdb  # Debug 符號
│   └── l10n/               # 自動複製的語系檔案
└── Release/net48/
    ├── DemandModifier.dll
    └── l10n/
```

### 發佈到 PDX Mods

#### 1. PublishConfiguration.xml 設定

```xml
<?xml version="1.0" encoding="utf-8"?>
<Publish>
    <!-- ModId: 首次發佈後由 PDX Mods 提供 -->
    <ModId Value="123136" />
    
    <!-- 基本資訊 -->
    <DisplayName Value="Demand Modifier" />
    <ShortDescription Value="Control residential, commercial and industrial demand levels" />
    
    <!-- 版本資訊 -->
    <ModVersion Value="0.2.1" />
    <GameVersion Value="1.2.*" />  <!-- 支援 1.2.x 所有版本 -->
    
    <!-- 詳細描述（支援 Markdown） -->
    <LongDescription>
# Demand Modifier

Control demand levels for different zone types in Cities: Skylines 2.

## Features
- **Residential Demand**: Adjust housing demand
- **Commercial Demand**: Control shop demand  
- **Industrial Demand**: Manage factory demand

Each demand type supports 5 levels: Off / Low / Medium / High / Maximum
    </LongDescription>
    
    <!-- 視覺資源 -->
    <Thumbnail Value="Properties/Thumbnail.png" />
    <Screenshot Value="Properties/Screenshots/Screenshot1.png" />
    
    <!-- 標籤 -->
    <Tag Value="Code Mod" />
    
    <!-- 外部連結 -->
    <ExternalLink Type="github" Url="https://github.com/YourUsername/DemandModifier" />
    <ExternalLink Type="discord" Url="https://discord.gg/YourServer" />
    
    <!-- 更新日誌 -->
    <ChangeLog>
## v0.2.1
- Added dropdown support for demand levels
- Fixed localization issues
- Improved performance
    </ChangeLog>
</Publish>
```

#### 2. 發佈命令

```powershell
# 首次發佈（獲得 ModId）
dotnet publish /p:PublishProfile=PublishNewMod

# 更新版本（需要 ModId）
dotnet publish /p:PublishProfile=PublishNewVersion

# 僅更新 Metadata（不上傳 DLL）
dotnet publish /p:PublishProfile=UpdatePublishedConfiguration
```

#### 3. 發佈前檢查清單

- [ ] 更新 `<ModVersion>`（語意化版本：Major.Minor.Patch）
- [ ] 更新 `<ChangeLog>`（列出所有變更）
- [ ] 確認 `<GameVersion>` 正確（測試過的遊戲版本）
- [ ] 準備縮圖 256x256 PNG
- [ ] 準備截圖 1920x1080 PNG（最多 6 張）
- [ ] 測試所有語系的 UI 顯示
- [ ] 驗證功能在遊戲中運作
- [ ] 檢查日誌無錯誤

### 本機測試部署

```powershell
# 快速部署腳本（test-deploy.ps1）
param([switch]$Clean)

if ($Clean) { dotnet clean }

dotnet build -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
Remove-Item $modsPath -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $modsPath | Out-Null
Copy-Item "bin\Release\net48\*" $modsPath -Recurse -Force

Write-Host "✓ 已部署到 $modsPath" -ForegroundColor Green
Write-Host "請重新啟動 Cities: Skylines 2" -ForegroundColor Yellow
```

**使用**：
```powershell
.\test-deploy.ps1          # 快速部署
.\test-deploy.ps1 -Clean   # 清理後部署
```

---

## 除錯與測試

### 日誌系統

#### 1. Logger 初始化

```csharp
using Colossal.Logging;

public static ILog log = LogManager.GetLogger(
    string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod))
).SetShowsErrorsInUI(false);
```

**SetShowsErrorsInUI(false) 重要性**：
- `true`: 錯誤彈出遊戲內通知（干擾玩家）
- `false`: 僅寫入日誌檔（推薦正式版）

#### 2. 日誌等級使用指南

```csharp
log.Debug("詳細除錯資訊 - 變數值、執行流程");           // 開發階段
log.Info("一般資訊 - 模組載入、設定變更");             // 重要事件
log.Warn("警告訊息 - 非預期但可處理的情況");           // 非致命問題
log.Error("錯誤訊息 - 功能失敗但不崩潰");              // 執行失敗
log.Critical("嚴重錯誤 - 可能導致崩潰");               // 致命錯誤
```

#### 3. 查看日誌

**日誌檔位置**：
```
%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\
├── Player.log          # 當前執行的日誌
└── Player-prev.log     # 上次執行的日誌
```

**即時監控**：
```powershell
# PowerShell 持續監控（類似 tail -f）
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Wait -Tail 50

# 搜尋特定模組的日誌
Select-String -Path "Player.log" -Pattern "DemandModifier" -Context 2,2
```

### 測試流程

#### 1. 功能測試檢查清單

**設定 UI 測試**：
- [ ] 所有分頁正確顯示
- [ ] 群組標題顯示（若啟用）
- [ ] 下拉選單顯示翻譯文字（非鍵值）
- [ ] 滑桿可拖曳且即時更新
- [ ] 按鈕執行正確動作
- [ ] 切換 7 種語言，UI 正確翻譯

**遊戲內功能測試**：
- [ ] 住宅需求設為 Maximum，觀察需求條立即變滿
- [ ] 住宅需求設為 Off，觀察需求條恢復正常波動
- [ ] 商業/工業需求同樣測試
- [ ] 檢查日誌無 Harmony 錯誤
- [ ] 確認不影響其他遊戲系統（建築、經濟等）

#### 2. 常見問題診斷

| 問題 | 可能原因 | 解決方案 |
|------|----------|----------|
| 模組不在 Mods 列表 | DLL 路徑錯誤 | 確認 `Mods\DemandModifier\DemandModifier.dll` |
| 設定介面空白 | 語系檔案遺失 | 檢查 `Mods\DemandModifier\l10n\` 存在 |
| 下拉選單顯示鍵值 | enum 未手動翻譯 | 實作 `GetLocalizedEnumName()` |
| 需求未改變 | Harmony 補丁失敗 | 檢查日誌 Harmony 錯誤 |
| 遊戲崩潰 | 欄位名稱錯誤 | 用 dnSpy 反編譯遊戲 DLL 確認欄位 |
| 設定無法儲存 | `FileLocation` 錯誤 | 確認 attribute 拼寫正確 |

#### 3. 反編譯工具使用

**工具**：
- [dnSpy](https://github.com/dnSpy/dnSpy) - 推薦，支援除錯
- [ILSpy](https://github.com/icsharpcode/ILSpy) - 輕量級

**步驟**：
1. 找到遊戲 DLL：
   ```
   [Steam]\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll
   ```

2. 用 dnSpy 開啟 `Game.dll`

3. 搜尋類別（Ctrl+Shift+K）：
   ```
   CommercialDemandSystem
   ```

4. 查看私有欄位：
   ```csharp
   private NativeValue<int> m_BuildingDemand;  // 確認欄位名稱
   ```

5. 更新 Harmony 補丁程式碼

---

## 參考專案分析

### Traffic Mod 最佳實踐

**專案連結**：https://github.com/krzychu124/Traffic

#### 1. 下拉選單實作（關鍵學習）

**Traffic 的語系管理**：
```csharp
// ModSettings.cs
[SettingsUIDropdown(typeof(ModSettings), nameof(GetLanguageOptions))]
public string CurrentLocale { get; set; } = "en-US";

private DropdownItem<string>[] GetLanguageOptions()
{
    return Localization.LocaleSources.Select(pair => new DropdownItem<string>()
    {
        value = pair.Key,
        displayName = pair.Value.Item1  // 已翻譯的語言名稱
    }).ToArray();
}
```

**關鍵學習點**：
- ✅ `displayName` 必須是翻譯後的字串
- ✅ 使用 Dictionary 快取翻譯
- ✅ 提供降級機制

#### 2. 語系系統架構

**Traffic 的 LocaleEN 類別**：
```csharp
public class LocaleEN : IDictionarySource
{
    private readonly ModSettings _setting;
    private Dictionary<string, string> _translations;

    public LocaleEN(ModSettings setting)
    {
        _setting = setting;
        LocaleSources["en-US"] = new Tuple<string, string, IDictionarySource>(
            "English", "100", this
        );
        _translations = Load();
    }

    public IEnumerable<KeyValuePair<string, string>> ReadEntries(
        IList<IDictionaryEntryError> errors, 
        Dictionary<string, int> indexCounts)
    {
        return _translations;
    }

    private Dictionary<string, string> Load()
    {
        return new Dictionary<string, string>
        {
            { _setting.GetSettingsLocaleID(), "Traffic" },
            { _setting.GetOptionTabLocaleID("General"), "General" },
            { _setting.GetOptionLabelLocaleID(nameof(ModSettings.UseVanillaToolActions)), "Use Vanilla Tool bindings" },
            // ... 完整翻譯
        };
    }
}
```

**學習點**：
- ✅ 實作 `IDictionarySource` 介面
- ✅ 使用 `GetSettingsLocaleID()` 等輔助方法
- ✅ 集中管理所有翻譯

#### 3. Keybindings 系統

**Traffic 的鍵位綁定**：
```csharp
[SettingsUIKeyboardAction(KeyBindAction.ToggleLaneConnectorTool, 
    Usages.kDefaultUsage, Usages.kEditorUsage, Usages.kToolUsage)]
public partial class ModSettings
{
    [SettingsUISection(KeybindingsTab, ToolsSection)]
    [SettingsUIKeyboardBinding(BindingKeyboard.R, KeyBindAction.ToggleLaneConnectorTool, ctrl: true)]
    public ProxyBinding LaneConnectorToolAction { get; set; }
}
```

**可能應用**：
- DemandModifier 未來可加入快捷鍵
- 例如：Ctrl+D 開啟設定介面

#### 4. 條件式 UI 控制

**Traffic 的動態停用**：
```csharp
[SettingsUISection(GeneralTab, MainSection)]
[SettingsUIDisableByCondition(typeof(ModSettings), nameof(UseGameLanguage))]
public string CurrentLocale { get; set; } = "en-US";

public bool UseGameLanguage { get; set; }
```

**應用於 DemandModifier**：
```csharp
// 當 ResidentialDemandLevel = Off 時，停用滑桿
[SettingsUISlider(min = 0, max = 255)]
[SettingsUIDisableByCondition(typeof(DemandModifierSettings), nameof(IsResidentialOff))]
public int ResidentialDemandValue { get; set; }

public bool IsResidentialOff() => ResidentialDemandLevel == DemandLevel.Off;
```

---

## 新增功能完整工作流程

### 範例：實作「無限電力」功能

#### 步驟 1：設定選項

**DemandModifierSettings.cs**：
```csharp
[SettingsUISection("ServiceControl", "ServiceSettings")]
public bool EnableUnlimitedElectricity { get; set; }
```

#### 步驟 2：語系翻譯（7 個檔案）

**l10n/en-US.json**：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Unlimited Electricity",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Never run out of electricity - all buildings receive power"
}
```

**l10n/zh-HANT.json**：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "無限電力",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "永不斷電 - 所有建築都能獲得電力供應"
}
```

重複 5 種語言：`zh-HANS`, `ja-JP`, `de-DE`, `es-ES`, `fr-FR`

#### 步驟 3：Harmony 補丁

**ElectricitySystemPatch.cs**（新檔案）：
```csharp
using HarmonyLib;
using Game.Simulation;
using Unity.Collections;

namespace DemandModifier
{
    [HarmonyPatch(typeof(ElectricityFlowSystem), "OnUpdate")]
    public class ElectricityFlowSystemPatch
    {
        // 1. 反射存取私有欄位
        private static AccessTools.FieldRef<ElectricityFlowSystem, NativeValue<int>> AvailabilityRef =
            AccessTools.FieldRefAccess<ElectricityFlowSystem, NativeValue<int>>("m_Availability");

        static void Prefix(ElectricityFlowSystem __instance)
        {
            // 2. 檢查設定（顯式 null 檢查）
            if (DemandModifierMod.Settings != null && 
                DemandModifierMod.Settings.EnableUnlimitedElectricity == true)
            {
                // 3. 設定為最大值
                AvailabilityRef(__instance).value = int.MaxValue;
                
                // 4. 可選日誌（Debug 建置）
                #if DEBUG
                DemandModifierMod.log.Debug("電力供應已設為無限");
                #endif
            }
        }
    }
}
```

#### 步驟 4：測試

```powershell
# 1. 建置
dotnet build -c Release

# 2. 部署
.\test-deploy.ps1

# 3. 遊戲內測試
# - 啟動遊戲
# - 設定 > Mods > Demand Modifier > 啟用「無限電力」
# - 建造建築，觀察電力始終充足
# - 檢查日誌無錯誤
```

#### 步驟 5：版本發佈

**PublishConfiguration.xml**：
```xml
<ModVersion Value="0.3.0" />  <!-- 新功能 = Minor 版本升級 -->
<ChangeLog>
## v0.3.0
- ✨ Added unlimited electricity feature
- 🐛 Fixed demand level not applying immediately
- 📝 Updated documentation
</ChangeLog>
```

---

## 進階技巧

### 1. 動態調整需求值

**目前**：固定 5 級 (Off/Low/Medium/High/Maximum)

**改進**：可調滑桿 (0-255)

```csharp
// Settings
[SettingsUISlider(min = 0, max = 255, step = 1)]
[SettingsUISection("DemandControl", "ResidentialDemand")]
public int ResidentialDemandValue { get; set; } = 255;

// Patch
BuildingDemandRef(__instance).value = DemandModifierMod.Settings.ResidentialDemandValue;
```

### 2. 條件式需求控制

```csharp
// 根據遊戲時間自動調整需求
static void Prefix(CommercialDemandSystem __instance)
{
    if (DemandModifierMod.Settings.EnableDynamicDemand)
    {
        var gameTime = World.DefaultGameObjectInjectionWorld
            .GetExistingSystemManaged<TimeSystem>()
            .GetCurrentTime();
        
        // 白天高需求，夜晚低需求
        int demandValue = gameTime.Hour >= 6 && gameTime.Hour <= 22 ? 255 : 128;
        BuildingDemandRef(__instance).value = demandValue;
    }
}
```

### 3. 多個補丁協作

```csharp
[HarmonyPatch(typeof(ElectricityFlowSystem), "OnUpdate")]
[HarmonyPriority(Priority.First)]  // 先執行
public class ElectricityPatchA { }

[HarmonyPatch(typeof(WaterSystem), "OnUpdate")]
[HarmonyPriority(Priority.Last)]   // 後執行
public class WaterPatchB { }
```

### 4. Transpiler 進階修改

```csharp
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    var codes = new List<CodeInstruction>(instructions);
    for (int i = 0; i < codes.Count; i++)
    {
        // 替換特定 IL 指令
        if (codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 100)
        {
            codes[i].operand = 255;  // 將常數 100 改為 255
        }
    }
    return codes.AsEnumerable();
}
```

---

## 版本相容性管理

### 遊戲版本追蹤

**目前支援**：Cities: Skylines 2 v1.2.*

**更新影響評估**：

| 變更類型 | 風險等級 | 範例 | 應對策略 |
|---------|---------|------|---------|
| 欄位名稱變更 | 🔴 高 | `m_BuildingDemand` → `m_Demand` | dnSpy 反編譯確認 |
| System 重構 | 🔴 高 | `CommercialDemandSystem` 被拆分 | 重新定位目標類別 |
| 方法簽章變更 | 🟡 中 | `OnUpdate()` → `OnUpdate(SystemState state)` | 更新補丁參數 |
| UI API 變更 | 🟡 中 | `SettingsUISection` → 新 API | 更新 attribute |
| 遊戲內容更新 | 🟢 低 | 新建築、新區域 | 通常無影響 |

### 更新後驗證流程

```powershell
# 1. 確認編譯
dotnet build -c Release
if ($LASTEXITCODE -ne 0) { Write-Error "編譯失敗" }

# 2. 部署測試
.\test-deploy.ps1

# 3. 遊戲內測試
# - 檢查 Mod 載入成功
# - 測試所有功能
# - 檢查日誌無錯誤

# 4. 效能測試
# - 開大地圖運行 30 分鐘
# - 監控記憶體使用
# - 確認無卡頓

# 5. 更新版本資訊
# PublishConfiguration.xml:
# <GameVersion Value="1.3.*" />
# <ChangeLog>Compatibility update for game version 1.3.x</ChangeLog>
```

---

## 參考資源

### 官方文件
- **Harmony 文件**: https://harmony.pardeike.net/
- **Unity ECS 文件**: https://docs.unity3d.com/Packages/com.unity.entities@latest
- **C# 語言參考**: https://learn.microsoft.com/en-us/dotnet/csharp/

### 社群資源
- **Cities: Skylines 2 Modding Discord**: 官方 Modding 社群
- **PDX Mods 論壇**: https://forum.paradoxplaza.com/forum/forums/cities-skylines-ii-user-mods.1167/
- **Traffic Mod GitHub**: https://github.com/krzychu124/Traffic （最佳實踐參考）

### 工具
- **dnSpy**: https://github.com/dnSpy/dnSpy （反編譯 + 除錯）
- **ILSpy**: https://github.com/icsharpcode/ILSpy （輕量反編譯）
- **DeepL**: https://www.deepl.com/ （高品質翻譯）

---

## 附錄

### A. 完整檔案清單

```
DemandModifier/
├── DemandModifier.csproj
├── DemandModifierMod.cs
├── DemandModifierSettings.cs
├── DemandSystemPatch.cs
├── l10n/
│   ├── en-US.json
│   ├── zh-HANT.json
│   ├── zh-HANS.json
│   ├── ja-JP.json
│   ├── de-DE.json
│   ├── es-ES.json
│   └── fr-FR.json
├── Properties/
│   ├── PublishConfiguration.xml
│   ├── Thumbnail.png
│   └── PublishProfiles/
│       ├── PublishNewMod.pubxml
│       ├── PublishNewVersion.pubxml
│       └── UpdatePublishedConfiguration.pubxml
└── scripts/
    ├── test-deploy.ps1
    └── check-thumbnail.ps1
```

### B. Git 工作流程

```powershell
# 開發新功能
git checkout -b feature/unlimited-electricity

# 提交變更
git add .
git commit -m "✨ Add unlimited electricity feature"

# 合併到主分支
git checkout master
git merge feature/unlimited-electricity

# 標記版本
git tag -a v0.3.0 -m "Release v0.3.0: Unlimited Electricity"
git push origin master --tags
```

### C. 版本號規則 (Semantic Versioning)

```
Major.Minor.Patch  (例: 0.2.1)

Major: 重大變更、不相容變更
Minor: 新功能、向下相容
Patch: Bug 修復、小改進
```

**範例**：
- `0.1.0`: 初版發佈
- `0.2.0`: 新增服務控制功能
- `0.2.1`: 修復需求不生效的 bug
- `1.0.0`: 穩定版本（完整功能、充分測試）

---

**文件版本**: 2.0  
**最後更新**: 2025-01-28  
**作者**: 基於 Traffic 專案最佳實踐整理  
**授權**: MIT
