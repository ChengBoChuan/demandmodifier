# Traffic vs DemandModifier - 詳細對比分析

本文檔提供兩個專案的詳細技術對比，幫助您從 Traffic 中汲取最佳實踐。

---

## 一、專案規模與複雜度

| 指標 | Traffic | DemandModifier | 說明 |
|------|---------|-----------------|------|
| 主要功能 | 3 個工具 (Lane Connector, Priorities, etc.) | 3 個系統 (Demand, Service, Economy) | 複雜度相似 |
| 檔案數量 | ~40+ 個 | ~15 個 | Traffic 更模組化 |
| 程式碼行數 | ~5000+ 行 | ~2000+ 行 | Traffic 功能更完整 |
| 語言支援 | 多語言 (Crowdin) | 7 種語言 | 都有完整支援 |
| Harmony 補丁 | 無（使用 ECS）| 有（Demand/Service/Economy） | 架構差異 |

---

## 二、檔案組織對比

### Traffic 的檔案結構
```
Traffic/Code/
├── Mod.cs                              # IMod 實作（100+ 行）
├── ModSettings.cs                      # 設定類別（300+ 行）
├── ModSettings.Keybindings.cs          # 按鍵綁定（分離檔案）
├── Logger.cs                           # 日誌工具
├── Localization.cs                     # 協調器
├── Localization.LocaleEN.cs            # 英文翻譯
├── Localization.LocaleManager.cs       # 語言管理
├── Localization.ModLocale.cs           # JSON 載入
├── Localization.UIKeys.cs              # UI 鍵值集合
├── CommonData/
│   ├── Enums.cs
│   ├── EntityData.cs
│   └── Components.cs
├── Components/                         # ECS 元件定義
├── Systems/                            # ECS 系統實作
│   ├── LaneConnections/
│   ├── Priorities/
│   └── Serialization/
├── Tools/                              # 工具實作
├── UISystems/                          # UI 系統
├── Rendering/                          # 渲染系統
├── Helpers/                            # 輔助類別
├── Utils/                              # 工具函式
└── Patches/                            # Harmony 補丁（如需要）
```

### DemandModifier 的檔案結構
```
DemandModifier/
├── DemandModifierMod.cs               # IMod 實作
├── DemandModifierSettings.cs          # 設定類別（700+ 行，需分離）
├── Code/
│   ├── Localization/
│   │   ├── LocalizationInitializer.cs
│   │   └── ModLocale.cs
│   ├── Patches/
│   │   ├── DemandSystemPatch.cs
│   │   └── ServiceSystemPatch.cs
│   ├── Systems/
│   │   └── DemandSystemHelper.cs
│   └── Utils/
│       └── PatchUtils.cs
└── l10n/                              # 語系檔案
    ├── en-US.json
    ├── zh-HANT.json
    └── ... 5 more
```

### 改進建議
| 面向 | 現狀 | 改進 | 優先級 |
|------|------|------|--------|
| 日誌 | 單一靜態欄位 | 專用 Logger 類別 | ⭐⭐⭐ |
| 設定 | 單檔 700+ 行 | 分離 Keybindings | ⭐⭐ |
| 語言 | 單檔初始化 | 多檔 + LocaleManager | ⭐⭐⭐ |
| 補丁 | 基本組織 | 加入基類和驗證 | ⭐⭐ |
| 文檔 | 基本 README | 資料夾層級文檔 | ⭐ |

---

## 三、ModSettings 屬性修飾器對比

### 1. 基本屬性

#### Traffic 的做法
```csharp
[SettingsUISection(GeneralTab, MainSection)]
public bool UseGameLanguage { get; set; }
```

#### DemandModifier 的做法
```csharp
[SettingsUISection("DemandControl", "ResidentialDemand")]
public bool EnableResidentialDemand { get; set; }
```

#### 改進版本
```csharp
// ✅ 使用常數 + 新增修飾器
[SettingsUISection(GeneralTab, ResidentialDemandSection)]
[SettingsUIValueVersion(typeof(Localization), nameof(Localization.LanguageSourceVersion))]
[SettingsUISetter(typeof(DemandModifierSettings), nameof(OnResidentialDemandChanged))]
public bool EnableResidentialDemand { get; set; }
```

### 2. 下拉選單

#### Traffic 的做法
```csharp
[SettingsUISection(GeneralTab, MainSection)]
[SettingsUIDropdown(typeof(ModSettings), nameof(GetLanguageOptions))]
[SettingsUIValueVersion(typeof(Localization), nameof(Localization.LanguageSourceVersion))]
[SettingsUISetter(typeof(ModSettings), nameof(ChangeModLanguage))]
[SettingsUIDisableByCondition(typeof(ModSettings), nameof(UseGameLanguage))]
public string CurrentLocale { get; set; } = "en-US";

private DropdownItem<string>[] GetLanguageOptions()
{
    return Localization.LocaleSources.Select(pair => new DropdownItem<string>()
    {
        value = pair.Key,
        displayName = pair.Value.Item1
    }).ToArray();
}
```

#### DemandModifier 的做法
```csharp
[SettingsUISection("DemandControl", "ResidentialDemand")]
[SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
public DemandLevel ResidentialDemandLevel { get; set; }

private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
{
    return new DropdownItem<DemandLevel>[]
    {
        new DropdownItem<DemandLevel> { value = DemandLevel.Off, displayName = GetLocalizedEnumName(DemandLevel.Off) },
        // ...
    };
}

private string GetLocalizedEnumName(DemandLevel level)
{
    // 複雜的翻譯邏輯...
}
```

#### 對比分析
| 特性 | Traffic | DemandModifier | 評價 |
|------|---------|-----------------|------|
| 修飾器數量 | 5 個 | 2 個 | Traffic 更完整 |
| 版本控制 | 有 (ValueVersion) | 無 | Traffic 更健壯 |
| 變更回調 | 有 (Setter) | 無 | Traffic 更靈活 |
| 條件禁用 | 支援 | 無 | Traffic 更強大 |
| 翻譯複雜度 | 低 (自動) | 高 (手動) | DemandModifier 可簡化 |

### 3. 滑桿

#### Traffic 的做法
```csharp
[SettingsUISlider(min = 0.2f, max = 2f, step = 0.1f, 
    unit = Game.UI.Unit.kFloatSingleFraction)]
[SettingsUISection(GeneralTab, OverlaysSection)]
public float ConnectionLaneWidth { get; set; }
```

#### DemandModifier 中缺少的功能
```csharp
// ❌ DemandModifier 沒有滑桿使用
// ✅ 可改進為
[SettingsUISlider(min = 0, max = 255, step = 1)]
[SettingsUISection(DemandControlTab, ResidentialDemandSection)]
[SettingsUISetter(typeof(DemandModifierSettings), nameof(OnDemandSliderChanged))]
public int ResidentialDemandValue { get; set; } = 255;
```

### 4. 按鈕

#### Traffic 的做法
```csharp
[SettingsUIButton()]
[SettingsUISection(GeneralTab, LaneConnectorSection)]
[SettingsUIConfirmation()]
[SettingsUIDisableByCondition(typeof(ModSettings), nameof(IsNotGameOrEditor))]
[SettingsUIHideByCondition(typeof(ModSettings), nameof(IsNotGameOrEditor))]
public bool ResetLaneConnections
{
    set
    {
        World.DefaultGameObjectInjectionWorld
            .GetExistingSystemManaged<LaneConnectorToolSystem>()
            .ResetAllConnections();
    }
}

public bool IsNotGameOrEditor()
{
    return (GameManager.instance.gameMode & GameMode.GameOrEditor) == 0;
}
```

#### DemandModifier 的缺陷
```csharp
// ❌ DemandModifier 沒有按鈕功能
// ✅ 可新增重設功能
[SettingsUIButton()]
[SettingsUISection(GeneralTab, ResidentialDemandSection)]
[SettingsUIConfirmation()]
public bool ResetAllDemands
{
    set
    {
        ResidentialDemandLevel = DemandLevel.Off;
        CommercialDemandLevel = DemandLevel.Off;
        IndustrialDemandLevel = DemandLevel.Off;
        ApplyAndSave();
    }
}
```

---

## 四、日誌系統對比

### Traffic 的 Logger.cs
```csharp
public static class Logger
{
    private static ILog _log = LogManager.GetLogger(...);

    public static void Info(string message, [CallerMemberName] string methodName = null)
    {
        _log.Info(message);
    }

    [Conditional("DEBUG_TOOL")]
    public static void DebugTool(string message) { }

    [Conditional("DEBUG_CONNECTIONS")]
    public static void DebugConnections(string message) { }

    [Conditional("DEBUG_LOCALE")]
    public static void DebugLocale(string message) { }

    public static void Warning(string message) { }
    public static void Error(string message) { }
}
```

### DemandModifier 的做法
```csharp
// ❌ 在 DemandModifierMod.cs 中定義
public static ILog log = LogManager.GetLogger(...);
log.Info("message");
log.Error("error");
```

### 改進對比
| 功能 | Traffic | DemandModifier | 改進度 |
|------|---------|-----------------|--------|
| 集中管理 | ✅ Logger 類別 | ❌ 靜態欄位 | 需要分離 |
| 條件編譯 | ✅ 7 種 | ❌ 0 種 | 新增 DEBUG_* 符號 |
| 呼叫者資訊 | ✅ CallerMemberName | ❌ 無 | 增強追蹤 |
| 異常記錄 | ⚠️ 基本 | ❌ 無 | 新增 Exception() |

---

## 五、多國語言系統對比

### 檔案結構對比

#### Traffic 的結構
```
Code/
├── Localization.cs                 # 協調器 (50 行)
├── Localization.LocaleEN.cs        # 英文 (400+ 行)
├── Localization.LocaleManager.cs   # 管理器 (150+ 行)
├── Localization.ModLocale.cs       # JSON 載入 (100+ 行)
└── Localization.UIKeys.cs          # UI 鍵值 (50+ 行)

UI/ (外部)
└── lang/ (可選 JSON 檔案)
```

#### DemandModifier 的結構
```
Code/Localization/
├── LocalizationInitializer.cs   # 初始化 (150+ 行)
└── ModLocale.cs                 # 模組本地化

l10n/
├── en-US.json
├── zh-HANT.json
└── ... 5 more
```

### 語言管理機制對比

#### Traffic 的 LocaleManager
```csharp
// ✅ 主動監聽遊戲語言變更
private class VanillaLocalizationObserver : IDisposable
{
    public void OnActiveDictionaryChanged()
    {
        if (_settings.UseGameLanguage)
        {
            // 同步為遊戲語言
            _settings.CurrentLocale = newLocale;
        }
        else
        {
            // 保持自訂語言
            manager.RemoveSource(lastLocale, ...);
            manager.AddSource(newLocale, ...);
        }
    }
}

// ✅ 三種語言模式
public void UseVanillaLanguage(string currentLanguage) { }
public void UseCustomLanguage(string customLanguage) { }
public void UseLocale(string locale, string currentLocale, bool useGameLocale) { }
```

#### DemandModifier 的做法
```csharp
// ⚠️ 被動初始化，無動態更新
public class LocalizationInitializer
{
    public static void Initialize(IMod mod)
    {
        // 一次性載入，無語言切換邏輯
    }
}
```

### 翻譯來源管理對比

#### Traffic 的做法
```csharp
// ✅ 全域字典管理
public static Dictionary<string, Tuple<string, string, IDictionarySource>> LocaleSources 
    { get; } = new Dictionary<...>();

// ✅ 版本控制
public static int LanguageSourceVersion { get; private set; } = 1;

// ✅ 動態載入
public static void LoadLocales(IMod mod, int expectedKeyCount)
{
    var locales = new[] { "en-US", "zh-HANT", ... };
    foreach (var locale in locales)
    {
        var modLocale = new ModLocale(locale, $"{mod.basePath}l10n/{locale}.json");
        modLocale.Load(expectedKeyCount);
    }
}
```

#### DemandModifier 的做法
```csharp
// ⚠️ 無全域字典
// ⚠️ 無版本控制
// ⚠️ 語言檔案需手動註冊
```

### 翻譯完成度檢測

#### Traffic 的做法
```csharp
// ✅ 自動計算並顯示
string coverage = $"{Convert.ToInt32((_translations.Count / refTranslationCount) * 100)}%";

LocaleSources[_localeId] = new Tuple<string, string, IDictionarySource>(
    _translations.GetValueOrDefault(GetLanguageNameLocaleID(), _localeId),
    coverage,  // 在 UI 中顯示
    this
);
```

#### DemandModifier 的做法
```csharp
// ❌ 無完成度跟蹤
```

---

## 六、Harmony 補丁對比

### Traffic 的方式（主要使用 ECS）
```csharp
// ✅ Traffic 主要使用 ECS Systems，而非 Harmony
public class Mod : IMod
{
    public void OnLoad(UpdateSystem updateSystem)
    {
        updateSystem.UpdateAt<LaneConnectorToolSystem>(SystemUpdatePhase.ToolUpdate);
        updateSystem.UpdateAt<PriorityToolSystem>(SystemUpdatePhase.ToolUpdate);
        // ... 許多 System 註冊
    }
}
```

### DemandModifier 的方式（Harmony 補丁）
```csharp
// ✅ DemandModifier 使用 Harmony 注入式修改
[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
public class CommercialDemandSystemPatch
{
    private static AccessTools.FieldRef<CommercialDemandSystem, NativeValue<int>> 
        BuildingDemandRef = AccessTools.FieldRefAccess<...>("m_BuildingDemand");

    static void Prefix(CommercialDemandSystem __instance)
    {
        if (DemandModifierMod.Settings != null && 
            DemandModifierMod.Settings.CommercialDemandLevel != DemandLevel.Off)
        {
            BuildingDemandRef(__instance).value = ...;
        }
    }
}
```

### 補丁改進對比
| 功能 | DemandModifier | 改進建議 | 好處 |
|------|-----------------|----------|------|
| 基類 | 無 | 新增 DemandSystemPatchBase | 程式碼重用 |
| 驗證 | 無 | 新增 ValidateSettings() | 更安全 |
| 日誌 | 無 | 新增 Logger.DebugPatches | 更好追蹤 |
| 例外處理 | 無 | 新增 try-catch | 更健壯 |
| 欄位快取 | 有 | 優化 | 保持 |

---

## 七、相容性與版本管理

### Traffic 的方式
```csharp
// ✅ 主動檢測其他模組
public static bool IsTLEEnabled => _isTLEEnabled ??= 
    GameManager.instance.modManager.ListModsEnabled()
        .Any(x => x.StartsWith("C2VM.CommonLibraries.LaneSystem"));

// ✅ 動態修複相容性問題
private static void TLECompatibilityFix()
{
    if (IsTLEEnabled)
    {
        Type type = tleAsset.assembly.GetType(...);
        ComponentSystemBase tleLaneSystem = 
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged(type);
        
        if (tleLaneSystem != null)
        {
            tleLaneSystem.Enabled = false;
            // 啟動遷移系統...
        }
    }
}

// ✅ 版本自動同步
public static string Version => 
    Assembly.GetExecutingAssembly().GetName().Version.ToString(4);

public static string InformationalVersion => 
    Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        .InformationalVersion;
```

### DemandModifier 的做法
```csharp
// ⚠️ 無相容性檢測
// ⚠️ 版本需手動管理
```

### 改進建議
| 功能 | 優先級 | 說明 |
|------|--------|------|
| 模組檢測 | ⭐ | 低優先（非關鍵） |
| 版本同步 | ⭐⭐ | 中等優先（自動化有益） |
| 資料遷移 | ⭐⭐ | 中等優先（save 相容性） |

---

## 八、完整改進路線圖

### 第一階段：基礎設施（1-2 週）
| 任務 | 工作量 | 優先級 | 文件 |
|------|--------|--------|------|
| 建立 Logger.cs | 2 小時 | ⭐⭐⭐ | QUICK_IMPLEMENTATION_GUIDE.md |
| 建立 LocaleManager.cs | 4 小時 | ⭐⭐⭐ | QUICK_IMPLEMENTATION_GUIDE.md |
| 建立 UIKeys.cs | 1 小時 | ⭐⭐ | TRAFFIC_BEST_PRACTICES.md |
| 補丁基類化 | 2 小時 | ⭐⭐ | QUICK_IMPLEMENTATION_GUIDE.md |

### 第二階段：ModSettings 重組（2-3 週）
| 任務 | 工作量 | 優先級 |
|------|--------|--------|
| 分離 Keybindings | 2 小時 | ⭐⭐ |
| 分離 Conditions | 1 小時 | ⭐⭐ |
| 新增 SettingsUIValueVersion | 2 小時 | ⭐⭐ |
| 新增 SettingsUISetter 回調 | 3 小時 | ⭐⭐ |
| 新增按鈕功能 | 2 小時 | ⭐ |

### 第三階段：功能拓展（3-4 週）
| 任務 | 工作量 | 優先級 |
|------|--------|--------|
| 服務控制系統 | 8 小時 | ⭐⭐⭐ |
| 經濟控制系統 | 8 小時 | ⭐⭐⭐ |
| 相容性檢測 | 3 小時 | ⭐ |
| 資料遷移系統 | 5 小時 | ⭐ |

### 第四階段：文檔與測試（1-2 週）
| 任務 | 工作量 | 優先級 |
|------|--------|--------|
| 更新 README | 1 小時 | ⭐⭐⭐ |
| 遷移指南 | 2 小時 | ⭐⭐ |
| 單元測試 | 4 小時 | ⭐⭐ |
| 使用者文檔 | 2 小時 | ⭐ |

---

## 九、技術對標參考

```
DemandModifier               Traffic              建議
─────────────────────────────────────────────────────────
  L 檔案                    40+ 檔案          ➜ 增至 25+ 檔案
  1500 行程式碼             5000+ 行           ➜ 可達 2500 行
  單一日誌欄位              Logger 類別        ➜ 採用 Logger 類別
  手動語言管理              LocaleManager      ➜ 採用 LocaleManager
  基本設定                  豐富的修飾器       ➜ 新增 5+ 修飾器
  靜態補丁                  ECS+Harmony        ➜ 優化補丁架構
  無相容性檢測              完整檢測系統       ➜ 後期實施（非關鍵）
```

---

## 十、快速參考清單

### ✅ Traffic 的優勢
- [x] 完整的日誌系統
- [x] 健壯的語言管理
- [x] 豐富的設定修飾器
- [x] 相容性檢測機制
- [x] 模組化文件結構

### ⚠️ DemandModifier 的現況
- [ ] 無專用 Logger 類別
- [ ] 語言管理不動態
- [ ] 設定修飾器有限
- [ ] 無相容性檢測
- [ ] 檔案組織可優化

### 📋 建議改進優先級
1. **必做** (⭐⭐⭐)
   - Logger.cs - 提升可維護性
   - LocaleManager.cs - 支援語言切換

2. **應做** (⭐⭐)
   - 分離 ModSettings
   - 補丁基類化
   - UIKeys 集合

3. **可做** (⭐)
   - 相容性檢測
   - 資料遷移
   - 高級功能按鈕

---

*本分析基於 Traffic v2023.11 和 DemandModifier v0.2.1 的程式碼審查*

