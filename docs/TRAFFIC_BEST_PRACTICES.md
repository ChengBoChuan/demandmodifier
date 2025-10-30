# Traffic 專案架構最佳實踐分析

本文檔對比 Traffic 專案與 DemandModifier 的架構差異，展示業界標準的實踐模式。

---

## 一、專案結構對比

### Traffic 專案架構（推薦）
```
Code/
├── Mod.cs                           # IMod 入口點（清晰）
├── ModSettings.cs                   # 設定類別（主檔案）
├── ModSettings.Keybindings.cs       # 按鍵綁定（分離關注點）
├── Logger.cs                        # 日誌工具類別
├── Localization.cs                  # 本地化協調器
├── Localization.LocaleEN.cs         # 每個語言單獨檔案
├── Localization.LocaleManager.cs    # 語言管理器
├── Localization.ModLocale.cs        # Mod 本地化模組
├── Localization.UIKeys.cs           # UI 鍵值集合
├── CommonData/                      # 共用資料結構
├── Components/                      # ECS 元件
├── Systems/                         # ECS 系統
├── Tools/                           # 工具實作
├── UISystems/                       # UI 系統
├── Rendering/                       # 渲染系統
├── Patches/                         # Harmony 補丁（如果有）
├── Helpers/                         # 輔助函式
└── Utils/                           # 工具函式
```

### DemandModifier 現有結構
```
Code/
├── Localization/
│   ├── LocalizationInitializer.cs
│   ├── ModLocale.cs
├── Patches/
│   ├── DemandSystemPatch.cs
│   ├── ServiceSystemPatch.cs
├── Systems/
│   ├── DemandSystemHelper.cs
└── Utils/
    ├── PatchUtils.cs
```

### 改進建議
- ✅ **分離關注點**：效仿 Traffic 的多檔案策略，按功能分離
- ✅ **集中日誌**：建立專用 `Logger.cs` 工具類別
- ✅ **語言分離**：每個語言建立獨立的 `LocaleXX.cs` 檔案
- ✅ **增加 README**：每個子資料夾加入簡要說明

---

## 二、ModSettings 實作最佳實踐

### Traffic 的設定類別架構

#### 1. **類別宣告與屬性修飾**
```csharp
// ✅ 完整的屬性修飾設定 (Traffic 模式)
[FileLocation("Traffic")]  // 設定檔位置標識符
[SettingsUITabOrder(GeneralTab, KeybindingsTab)]  // 分頁順序
[SettingsUIGroupOrder(MainSection, LaneConnectorSection, PrioritiesSection, OverlaysSection, AboutSection)]
[SettingsUIShowGroupName(MainSection, LaneConnectorSection, PrioritiesSection, OverlaysSection, AboutSection)]
public partial class ModSettings : ModSetting
{
    // 常數定義（集中管理）
    internal const string SETTINGS_ASSET_NAME = "Traffic General Settings";
    internal static ModSettings Instance { get; private set; }
    
    internal const string GeneralTab = "General";
    internal const string KeybindingsTab = "Keybindings";
    internal const string MainSection = "General";
    internal const string LaneConnectorSection = "LaneConnections";
}
```

#### 2. **屬性修飾器完整應用**
```csharp
// ✅ 布林值開關（最簡單）
[SettingsUISection(GeneralTab, MainSection)]
public bool UseGameLanguage { get; set; }

// ✅ 下拉選單（必須提供方法）
[SettingsUISection(GeneralTab, MainSection)]
[SettingsUIDropdown(typeof(ModSettings), nameof(GetLanguageOptions))]
[SettingsUIValueVersion(typeof(Localization), nameof(Localization.LanguageSourceVersion))]
[SettingsUISetter(typeof(ModSettings), nameof(ChangeModLanguage))]
[SettingsUIDisableByCondition(typeof(ModSettings), nameof(UseGameLanguage))]
public string CurrentLocale { get; set; } = "en-US";

// ✅ 滑桿（帶單位）
[SettingsUISlider(min = 0.2f, max = 2f, step = 0.1f, 
    unit = Game.UI.Unit.kFloatSingleFraction)]
[SettingsUISection(GeneralTab, OverlaysSection)]
public float ConnectionLaneWidth { get; set; }

// ✅ 按鈕（帶確認對話框）
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

// ✅ 多行文本（顯示資訊）
[SettingsUIMultilineText("coui://ui-mods/traffic-images/crowdin-icon-white.svg")]
[SettingsUISection(GeneralTab, MainSection)]
public string TranslationCoverageStatus => string.Empty;
```

#### 3. **條件式屬性修飾器**
```csharp
// ✅ 條件隱藏
[SettingsUIHideByCondition(typeof(ModSettings), nameof(IsNotGameOrEditor))]
public bool ResetLaneConnections { set { /* ... */ } }

// ✅ 條件禁用
[SettingsUIDisableByCondition(typeof(ModSettings), nameof(UseGameLanguage))]
public string CurrentLocale { get; set; }

// ✅ 支援方法
public bool IsNotGameOrEditor()
{
    return (GameManager.instance.gameMode & GameMode.GameOrEditor) == 0;
}
```

#### 4. **下拉選單選項提供方法**
```csharp
// ✅ 標準實作（Traffic 模式）
private DropdownItem<string>[] GetLanguageOptions()
{
    return Localization.LocaleSources.Select(pair => new DropdownItem<string>()
    {
        value = pair.Key,
        displayName = pair.Value.Item1  // 預先翻譯的顯示名稱
    }).ToArray();
}
```

#### 5. **設定變更回調**
```csharp
// ✅ 自訂 Setter（變更時執行回調）
[SettingsUISetter(typeof(ModSettings), nameof(OnUseGameLanguageSet))]
public bool UseGameLanguage { get; set; }

// ✅ 回調方法
public void OnUseGameLanguageSet(bool value)
{
    if (UseGameLanguage == value)
    {
        Logger.Info($"(OnUseGameLanguageSet) No state changed {value}");
        return;
    }

    if (value)
    {
        _localeManager.UseVanillaLanguage(CurrentLocale);
        CurrentLocale = GameManager.instance.localizationManager.activeLocaleId;
    }
    else
    {
        _localeManager.UseCustomLanguage(CurrentLocale);
    }
}
```

#### 6. **設定套用與重設**
```csharp
// ✅ Apply 方法（變更時呼叫）
public override void Apply()
{
    ToolOverlaySystem toolOverlaySystem = 
        World.DefaultGameObjectInjectionWorld
            .GetExistingSystemManaged<ToolOverlaySystem>();
    
    toolOverlaySystem.ApplyOverlayParams(new ToolOverlayParameterData()
    {
        feedbackLinesWidth = FeedbackOutlineWidth,
        laneConnectorSize = ConnectorSize,
        laneConnectorLineWidth = ConnectionLaneWidth,
        showLaneConnectionsPriority = ShowConnectionsOverlayWhenEditing,
    });

    base.Apply();
}

// ✅ SetDefaults 方法（重設為預設）
public sealed override void SetDefaults()
{
    ConnectorSize = 1f;
    ConnectionLaneWidth = 0.4f;
    FeedbackOutlineWidth = 0.3f;
    UseGameLanguage = true;
    UseVanillaToolActions = true;
}
```

### 對比 DemandModifier

```csharp
// ❌ DemandModifier 目前做法
[SettingsUISection("DemandControl", "ResidentialDemand")]
[SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
public DemandLevel ResidentialDemandLevel { get; set; }

// ✅ Traffic 推薦改進
[SettingsUISection(GeneralTab, ResidentialDemandSection)]  // 使用常數
[SettingsUIDropdown(typeof(ModSettings), nameof(GetDemandLevelOptions))]
[SettingsUIValueVersion(typeof(Localization), nameof(Localization.LanguageSourceVersion))]
[SettingsUISetter(typeof(ModSettings), nameof(ApplyDemandChange))]
public DemandLevel ResidentialDemandLevel { get; set; }

// 新增回調方法
public void ApplyDemandChange(DemandLevel value)
{
    // 直接應用需求變更
    // 更新相關系統狀態
}
```

---

## 三、多國語言系統最佳實踐

### Traffic 的語言架構

#### 1. **語言管理器（LocaleManager）**
```csharp
// ✅ 語言管理器核心功能
internal class LocaleManager: IDisposable
{
    private LocalizationManager _vanillaLocalizationManager;
    private VanillaLocalizationObserver _localizationObserver;

    public LocaleManager()
    {
        _vanillaLocalizationManager = GameManager.instance.localizationManager;
        _prevGameLocale = _vanillaLocalizationManager.activeLocaleId;
    }

    // ✅ 三種語言使用模式
    public void UseVanillaLanguage(string currentLanguage)
    {
        // 使用遊戲的原生語言
    }

    public void UseCustomLanguage(string customLanguage)
    {
        // 使用 Mod 的自訂語言
    }

    public void UseLocale(string locale, string currentLocale, bool useGameLocale)
    {
        // 切換語言
    }

    public void RegisterVanillaLocalizationObserver(ModSettings settings)
    {
        _localizationObserver = new VanillaLocalizationObserver(this, settings);
    }

    public void Dispose()
    {
        _localizationObserver?.Dispose();
        _localizationObserver = null;
    }
}
```

#### 2. **遊戲語言變更觀察器**
```csharp
// ✅ 內部觀察器類別（監聽遊戲語言變更）
private class VanillaLocalizationObserver: IDisposable
{
    private bool _disableChangeCallback;

    public VanillaLocalizationObserver(LocaleManager localeManager, ModSettings settings)
    {
        GameManager.instance.localizationManager.onActiveDictionaryChanged 
            += OnActiveDictionaryChanged;
    }

    private void OnActiveDictionaryChanged()
    {
        if (_disableChangeCallback) return;

        var manager = GameManager.instance.localizationManager;
        string newLocale = manager.activeLocaleId;

        if (_settings.UseGameLanguage)
        {
            // 更新為遊戲語言
            _settings.CurrentLocale = newLocale;
        }
        else
        {
            // 保持自訂語言
            if (LocaleSources.ContainsKey(newLocale))
            {
                manager.RemoveSource(newLocale, LocaleSources[newLocale].Item3);
                manager.AddSource(newLocale, LocaleSources[_settings.CurrentLocale].Item3);
            }
        }
    }

    public void Dispose()
    {
        GameManager.instance.localizationManager.onActiveDictionaryChanged 
            -= OnActiveDictionaryChanged;
    }
}
```

#### 3. **每語言檔案（LocaleEN.cs）**
```csharp
// ✅ 語言檔案結構（LocaleEN.cs）
public class LocaleEN : IDictionarySource
{
    private Dictionary<string, string> _translations;
    private readonly ModSettings _setting;

    public LocaleEN(ModSettings setting)
    {
        _setting = setting;
        LocaleSources["en-US"] = new Tuple<string, string, IDictionarySource>(
            "English",  // 顯示名稱
            "100",      // 翻譯完成度
            this        // 翻譯來源
        );
        _translations = Load();
    }

    // ✅ 使用輔助方法建立鍵值
    public Dictionary<string, string> Load(bool dumpTranslations = false)
    {
        return new Dictionary<string, string>
        {
            { _setting.GetSettingsLocaleID(), "Traffic" },
            { GetLanguageNameLocaleID(), "English" },
            { _setting.GetOptionTabLocaleID(ModSettings.GeneralTab), "General" },
            { _setting.GetOptionGroupLocaleID(ModSettings.MainSection), "General" },
            { _setting.GetOptionLabelLocaleID(nameof(ModSettings.UseGameLanguage)), "Use Game Language" },
            { _setting.GetOptionDescLocaleID(nameof(ModSettings.UseGameLanguage)), 
                "Disabling will allow selecting different language..." },
            // ... 更多翻譯項目
        };
    }

    public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, 
        Dictionary<string, int> indexCounts)
    {
        return _translations;
    }

    public void Unload() { }
}
```

#### 4. **模組化語言檔案（ModLocale.cs）**
```csharp
// ✅ 從 JSON 檔案載入語言（模組化方式）
public class ModLocale : IDictionarySource
{
    private string _localeId;
    private string _localePath;
    private Dictionary<string, string> _translations;

    public ModLocale Load(float refTranslationCount)
    {
        if (File.Exists(_localePath))
        {
            try
            {
                Variant variant = JSON.Load(File.ReadAllText(_localePath));
                _translations = variant.Make<Dictionary<string, string>>();

                // ✅ 計算翻譯完成度
                string coverage = $"{Convert.ToInt32((_translations.Count / refTranslationCount) * 100)}%";

                // ✅ 降級機制：缺失鍵值使用英文版本
                var fallback = LocaleSources["en-US"].Item3
                    .ReadEntries(null, null)
                    .ToDictionary(k => k.Key, k => k.Value);
                
                foreach (KeyValuePair<string, string> keyValuePair in fallback)
                {
                    _translations.TryAdd(keyValuePair.Key, keyValuePair.Value);
                }

                LocaleSources[_localeId] = new Tuple<string, string, IDictionarySource>(
                    _translations.GetValueOrDefault(GetLanguageNameLocaleID(), _localeId),
                    coverage,
                    this
                );
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load locale {_localeId}: {e}");
            }
        }
        return this;
    }
}
```

#### 5. **語言來源管理（全域字典）**
```csharp
// ✅ 集中管理所有語言來源
public partial class Localization
{
    // 全域語言源字典
    public static Dictionary<string, Tuple<string, string, IDictionarySource>> LocaleSources { get; } 
        = new Dictionary<string, Tuple<string, string, IDictionarySource>>();

    // 版本控制（用於 SettingsUIValueVersion）
    public static int LanguageSourceVersion { get; private set; } = 1;

    // ✅ 載入所有語言
    public static void LoadLocales(IMod mod, int expectedKeyCount)
    {
        var manager = GameManager.instance.localizationManager;
        
        var locales = new[] { "en-US", "zh-HANT", "zh-HANS", "ja-JP", "de-DE", "es-ES", "fr-FR" };
        foreach (var locale in locales)
        {
            var modLocale = new ModLocale(locale, $"{mod.basePath}l10n/{locale}.json");
            modLocale.Load(expectedKeyCount);
            manager.AddSource(locale, modLocale);
        }
    }

    // ✅ 設定值初始化
    public static (string, bool) ApplySettings(string gameLocale, bool useGameLanguage, string currentLanguage)
    {
        if (!useGameLanguage)
        {
            if (!LocaleSources.ContainsKey(currentLanguage))
            {
                Logger.Info($"Locale {currentLanguage} not found, fallback to English");
                return (currentLanguage, true);
            }
            // 切換到自訂語言
        }
        return (currentLanguage, useGameLanguage);
    }
}
```

#### 6. **UI 鍵值集合（UIKeys.cs）**
```csharp
// ✅ 集中管理所有 UI 鍵值（避免重複）
public static class UIKeys
{
    public const string TRAFFIC_MOD = "UI.TRAFFIC_MOD";
    public const string SHORTCUT = "UI.SHORTCUT";
    public const string LANE_CONNECTOR_TOOL = "UI.LANE_CONNECTOR_TOOL";
    public const string LANE_CONNECTOR_TOOL_DESCRIPTION = "UI.LANE_CONNECTOR_TOOL_DESCRIPTION";
    public const string SELECT_INTERSECTION = "UI.SELECT_INTERSECTION";
    public const string REMOVE_ALL_CONNECTIONS = "UI.REMOVE_ALL_CONNECTIONS";
    // ... 更多鍵值
}
```

### 對比 DemandModifier 的改進

```csharp
// ❌ DemandModifier 現有做法
// 所有語言在一個方法內定義，難以維護

// ✅ 建議改為
// 1. 建立 LocaleManager.cs - 管理語言切換
// 2. 為每個語言建立檔案：LocaleEN.cs, LocaleZHT.cs, LocaleZHS.cs 等
// 3. 建立 UIKeys.cs - 集中管理 UI 鍵值
// 4. ModLocale.cs - 從 JSON 檔案動態載入（提高可維護性）
```

---

## 四、Harmony 補丁組織最佳實踐

### Traffic 的設計（若有補丁）

Traffic 主要使用 ECS Systems 而非 Harmony，但本最佳實踐基於通用模式：

```csharp
// ✅ 補丁組織建議
namespace Traffic.Systems.Patches
{
    // 補丁檔案命名：系統名稱 + Patch
    // 路徑：Code/Systems/Patches/
}

// ✅ 如果需要補丁（參考 DemandModifier）
[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
public class CommercialDemandSystemPatch
{
    // 快取欄位參考（效能最佳）
    private static AccessTools.FieldRef<CommercialDemandSystem, NativeValue<int>> 
        BuildingDemandRef = AccessTools.FieldRefAccess<CommercialDemandSystem, NativeValue<int>>(
            "m_BuildingDemand");

    // 區分不同修改型別
    static void Prefix(CommercialDemandSystem __instance)
    {
        // 使用 Logger
        Logger.DebugConnections("Patching CommercialDemandSystem");

        if (DemandModifierMod.Settings != null && 
            DemandModifierMod.Settings.CommercialDemandLevel != DemandLevel.Off)
        {
            BuildingDemandRef(__instance).value = 
                (int)DemandModifierMod.Settings.CommercialDemandLevel;
        }
    }
}
```

---

## 五、日誌系統最佳實踐

### Traffic 的日誌設計（Logger.cs）

```csharp
// ✅ 完整的日誌工具類別
public static class Logger
{
    private static ILog _log = LogManager.GetLogger(
        $"{nameof(Traffic)}.{nameof(Mod)}");

    // ✅ 資訊日誌（最常用）
    public static void Info(string message, [CallerMemberName] string methodName = null)
    {
        _log.Info(message);
    }

    // ✅ 條件式日誌（使用編譯符號控制）
    [Conditional("DEBUG_TOOL")]
    public static void DebugTool(string message)
    {
        _log.Info(message);
    }

    [Conditional("DEBUG_CONNECTIONS")]
    public static void DebugConnections(string message)
    {
        _log.Info(message);
    }

    [Conditional("DEBUG_CONNECTIONS_SYNC")]
    public static void DebugConnectionsSync(string message)
    {
        _log.Info(message);
    }

    [Conditional("DEBUG")]
    public static void Debug(string message)
    {
        _log.Info(message);
    }

    [Conditional("DEBUG_LOCALE")]
    public static void DebugLocale(string message)
    {
        _log.Info(message);
    }

    [Conditional("DEBUG")]
    public static void DebugError(string message)
    {
        _log.Error(message);
    }

    [Conditional("DEBUG_LANE_SYS")]
    public static void DebugLaneSystem(string message)
    {
        _log.Info(message);
    }

    [Conditional("SERIALIZATION")]
    public static void Serialization(string message)
    {
        _log.Info(message);
    }

    // ✅ 警告和錯誤（總是記錄）
    public static void Warning(string message)
    {
        _log.Warn(message);
    }

    public static void Error(string message)
    {
        _log.Error(message);
    }
}
```

### 使用情境

```csharp
// ✅ 使用日誌
Logger.Info("Module loaded successfully");
Logger.DebugConnections("Processing lane connections");
Logger.Warning("Fallback to English language");
Logger.Error("Failed to load translation");

// ✅ 在 Mod.cs 中
public void OnLoad(UpdateSystem updateSystem)
{
    Logger.Info($"{nameof(OnLoad)}, version: {InformationalVersion}");
    // ...
}

public void OnDispose()
{
    Logger.Info(nameof(OnDispose));
    // ...
}
```

### 對比 DemandModifier

```csharp
// ✅ DemandModifier 現有做法（已不錯）
public static ILog log = LogManager.GetLogger(
    $"{nameof(DemandModifier)}.{nameof(DemandModifierMod)}")
    .SetShowsErrorsInUI(false);

// 建議改進：
// 1. 建立專用 Logger.cs 類別
// 2. 新增條件式日誌（Debug, DebugConnections 等）
// 3. 添加 CallerMemberName 自動記錄方法名稱
```

---

## 六、IMod 生命週期最佳實踐

### Traffic 的 Mod 類別（Mod.cs）

```csharp
// ✅ 完整的 IMod 實作
[UsedImplicitly]
public class Mod : IMod
{
    public const string MOD_NAME = "Traffic";
    
    // ✅ 版本資訊
    public static string Version => 
        Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
    
    public static string InformationalVersion => 
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

    // ✅ 相容性檢測
    public static bool IsTLEEnabled => _isTLEEnabled ??= 
        GameManager.instance.modManager.ListModsEnabled()
            .Any(x => x.StartsWith("C2VM.CommonLibraries.LaneSystem"));

    private static bool? _isTLEEnabled;

    internal ModSettings Settings { get; private set; }

    // ✅ 完整的 OnLoad 方法
    public void OnLoad(UpdateSystem updateSystem)
    {
        Logger.Info($"{nameof(OnLoad)}, version: {InformationalVersion}");

        // 1. 相容性檢測
        TrySearchingForIncompatibleTLEOnBepinEx();

        // 2. 建立設定
        Settings = new ModSettings(this, false);
        Settings.RegisterKeyBindings();
        Settings.RegisterInOptionsUI();

        // 3. 載入設定資料
        Colossal.IO.AssetDatabase.AssetDatabase.global.LoadSettings(
            ModSettings.SETTINGS_ASSET_NAME, 
            Settings, 
            new ModSettings(this, true));

        // 4. 載入語言
        if (!GameManager.instance.localizationManager
            .activeDictionary
            .ContainsID(Settings.GetSettingsLocaleID()))
        {
            var source = new Localization.LocaleEN(Settings);
            GameManager.instance.localizationManager.AddSource("en-US", source);
            Localization.LoadLocales(this, source.ReadEntries(null, null).Count());
        }

        Settings.ApplyLoadedSettings();

        // 5. 註冊 UI 系統
        updateSystem.UpdateAt<ModUISystem>(SystemUpdatePhase.UIUpdate);

        // 6. 註冊其他系統
        updateSystem.UpdateBefore<PreDeserialize<ModUISystem>>(SystemUpdatePhase.Deserialize);
        updateSystem.UpdateAfter<ToolOverlaySystem, AreaRenderSystem>(SystemUpdatePhase.Rendering);

        // 7. 相容性固定
        GameManager.instance.RegisterUpdater(TLECompatibilityFix);
        GameManager.instance.RegisterUpdater(RoadBuilderCompatibilityHandler);
    }

    // ✅ 完整的 OnDispose 方法
    public void OnDispose()
    {
        Logger.Info(nameof(OnDispose));
        
        Settings?.UnregisterInOptionsUI();
        Settings?.Unload();
        Settings = null;
    }

    // ✅ 相容性檢測方法
    private static void TLECompatibilityFix()
    {
        if (IsTLEEnabled)
        {
            Logger.Info("Detected TLE installed and enabled!");
            try
            {
                Type type = /* ... 尋找 TLE 類型 ... */;
                if (type == null) return;

                ComponentSystemBase tleLaneSystem = 
                    World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged(type);
                
                if (tleLaneSystem != null)
                {
                    Logger.Info("TLE custom LaneSystem found!");
                    tleLaneSystem.Enabled = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
```

---

## 七、DemandModifier 改進行動計畫

### Phase 1: 架構重組（優先）
- [ ] 建立 `Code/Logger.cs` - 集中日誌管理
- [ ] 分離 `ModSettings.Keybindings.cs` - 按鍵綁定
- [ ] 分離 `ModSettings.Conditions.cs` - 條件方法

### Phase 2: 語言系統升級
- [ ] 建立 `Code/Localization/LocaleManager.cs` - 管理語言切換
- [ ] 為每個語言建立檔案：
  - [ ] `Localization.LocaleEN.cs`
  - [ ] `Localization.LocaleZHT.cs`
  - [ ] `Localization.LocaleZHS.cs`
  - [ ] 其他語言...
- [ ] 建立 `Localization.UIKeys.cs` - 集中 UI 鍵值
- [ ] 建立 `Localization.ModLocale.cs` - 從 JSON 載入

### Phase 3: 補丁優化
- [ ] 建立 `Code/Patches/` 資料夾
- [ ] 移動並重組補丁檔案
- [ ] 添加補丁文件說明

### Phase 4: 文檔完善
- [ ] 新增 README 在各子資料夾
- [ ] 更新主 README 反映架構變更

---

## 八、核心差異對比表

| 功能 | DemandModifier | Traffic | 推薦改進 |
|------|-----------------|---------|--------|
| **日誌管理** | 單一靜態欄位 | Logger 類別 + 條件式 | 移至 Logger.cs |
| **設定檔案** | 單檔案 | ModSettings + Keybindings | 分離關注點 |
| **語言系統** | 單檔 LocalizationInitializer | 多檔 + LocaleManager | 分離語言實作 |
| **補丁組織** | Code/Patches/ | 根據系統分類 | 保持現狀或優化 |
| **版本管理** | 手動更新 | Assembly 版本自動同步 | 參考 Traffic 模式 |
| **相容性檢測** | 無 | 完整檢測系統 | 參考 Mod.cs |
| **文檔結構** | 基本 | 模組化資料夾結構 | 改善可發現性 |

---

## 九、快速參考程式碼範本

### Logger.cs 完整模板
```csharp
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Colossal.Logging;

namespace DemandModifier
{
    public static class Logger
    {
        private static ILog _log = LogManager.GetLogger(
            $"{nameof(DemandModifier)}.{nameof(DemandModifierMod)}")
            .SetShowsErrorsInUI(false);

        public static void Info(string message, 
            [CallerMemberName] string methodName = null)
        {
            _log.Info($"[{methodName}] {message}");
        }

        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            _log.Info(message);
        }

        [Conditional("DEBUG_DEMAND")]
        public static void DebugDemand(string message)
        {
            _log.Info(message);
        }

        public static void Warning(string message)
        {
            _log.Warn(message);
        }

        public static void Error(string message)
        {
            _log.Error(message);
        }
    }
}
```

### LocaleManager.cs 基礎模板
```csharp
using System;
using Colossal.Localization;
using Game.SceneFlow;

namespace DemandModifier
{
    public partial class Localization
    {
        internal class LocaleManager : IDisposable
        {
            private LocalizationManager _vanillaLocalizationManager;
            private VanillaLocalizationObserver _localizationObserver;

            public LocaleManager()
            {
                _vanillaLocalizationManager = GameManager.instance.localizationManager;
                Logger.Info("LocaleManager initialized");
            }

            public void RegisterVanillaLocalizationObserver(ModSettings settings)
            {
                _localizationObserver = new VanillaLocalizationObserver(this, settings);
            }

            public void Dispose()
            {
                _localizationObserver?.Dispose();
                _localizationObserver = null;
                Logger.Info("LocaleManager disposed");
            }

            // 內部觀察器...
        }
    }
}
```

---

## 結論

Traffic 專案展現的最佳實踐：

1. **模組化設計**：單一責任原則，每檔案專注一個功能
2. **條件式編譯**：使用編譯符號進行精細控制
3. **語言管理**：分離語言實作，支持動態切換
4. **設定系統**：豐富的屬性修飾器，回調機制
5. **文檔完善**：清晰的資料夾結構和命名規則
6. **相容性處理**：主動檢測和修正其他模組相容性問題

這些模式可直接應用於 DemandModifier 的升級計畫中。

