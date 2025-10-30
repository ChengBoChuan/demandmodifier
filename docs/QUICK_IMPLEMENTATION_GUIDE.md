# Traffic 最佳實踐 - 快速應用指南

本指南提供可直接應用於 DemandModifier 的改進方案。

---

## 立即改進 #1：建立專用 Logger 類別

### 建立檔案：`Code/Utils/Logger.cs`

```csharp
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Colossal.Logging;

namespace DemandModifier
{
    /// <summary>
    /// 集中管理模組的日誌輸出
    /// 提供條件式編譯的除錯紀錄
    /// </summary>
    public static class Logger
    {
        private static ILog _log = LogManager.GetLogger(
            $"{nameof(DemandModifier)}.{nameof(DemandModifierMod)}")
            .SetShowsErrorsInUI(false);

        /// <summary>
        /// 資訊日誌（總是記錄）
        /// </summary>
        public static void Info(string message, [CallerMemberName] string methodName = null)
        {
            _log.Info($"[{methodName}] {message}");
        }

        /// <summary>
        /// 除錯日誌（除錯組態時記錄）
        /// </summary>
        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            _log.Info($"[DEBUG] {message}");
        }

        /// <summary>
        /// 需求系統除錯（條件符號：DEBUG_DEMAND）
        /// </summary>
        [Conditional("DEBUG_DEMAND")]
        public static void DebugDemand(string message)
        {
            _log.Info($"[DEMAND] {message}");
        }

        /// <summary>
        /// 補丁系統除錯（條件符號：DEBUG_PATCHES）
        /// </summary>
        [Conditional("DEBUG_PATCHES")]
        public static void DebugPatches(string message)
        {
            _log.Info($"[PATCHES] {message}");
        }

        /// <summary>
        /// 語言系統除錯（條件符號：DEBUG_LOCALE）
        /// </summary>
        [Conditional("DEBUG_LOCALE")]
        public static void DebugLocale(string message)
        {
            _log.Info($"[LOCALE] {message}");
        }

        /// <summary>
        /// 警告日誌（總是記錄）
        /// </summary>
        public static void Warning(string message)
        {
            _log.Warn($"[WARNING] {message}");
        }

        /// <summary>
        /// 錯誤日誌（總是記錄）
        /// </summary>
        public static void Error(string message)
        {
            _log.Error($"[ERROR] {message}");
        }

        /// <summary>
        /// 例外日誌
        /// </summary>
        public static void Exception(System.Exception ex, string context = null)
        {
            _log.Error($"[EXCEPTION{(context != null ? $":{context}" : "")}] {ex.Message}\n{ex.StackTrace}");
        }
    }
}
```

### 在 `DemandModifierMod.cs` 中替換

```csharp
// ❌ 舊方式
public static ILog log = LogManager.GetLogger(...);

// ✅ 新方式
public void OnLoad(UpdateSystem updateSystem)
{
    Logger.Info("DemandModifier 模組載入中");
    // ...
}

public void OnDispose()
{
    Logger.Info("DemandModifier 模組卸載");
}
```

---

## 立即改進 #2：增強 ModSettings 屬性

### 改進 `DemandModifierSettings.cs`

```csharp
public partial class DemandModifierSettings : ModSetting
{
    // ✅ 使用常數（便於重構）
    internal const string SETTINGS_ASSET_NAME = "DemandModifier General Settings";
    internal static DemandModifierSettings Instance { get; private set; }
    
    internal const string GeneralTab = "General";
    internal const string DemandControlTab = "DemandControl";
    internal const string ServiceControlTab = "ServiceControl";
    internal const string EconomyControlTab = "EconomyControl";
    
    internal const string ResidentialDemandSection = "ResidentialDemand";
    internal const string CommercialDemandSection = "CommercialDemand";
    internal const string IndustrialDemandSection = "IndustrialDemand";

    public DemandModifierSettings(IMod mod) : base(mod)
    {
        Instance = this;
        SetDefaults();
    }

    // ✅ 使用 SettingsUIValueVersion（語言變更時重新整理）
    [SettingsUISection(DemandControlTab, ResidentialDemandSection)]
    [SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
    [SettingsUIValueVersion(typeof(Localization), nameof(Localization.LanguageSourceVersion))]
    [SettingsUISetter(typeof(DemandModifierSettings), nameof(OnResidentialDemandChanged))]
    public DemandLevel ResidentialDemandLevel { get; set; } = DemandLevel.Off;

    // ✅ 新增回調方法
    public void OnResidentialDemandChanged(DemandLevel value)
    {
        Logger.DebugDemand($"ResidentialDemand changed to: {value}");
        ApplyAndSave();
    }

    // ✅ 條件方法
    public bool IsInGame()
    {
        return (GameManager.instance.gameMode & GameMode.Game) != 0;
    }

    // ✅ SetDefaults 實作
    public sealed override void SetDefaults()
    {
        ResidentialDemandLevel = DemandLevel.Off;
        CommercialDemandLevel = DemandLevel.Off;
        IndustrialDemandLevel = DemandLevel.Off;
    }

    // ✅ Apply 實作（設定變更時呼叫）
    public override void Apply()
    {
        Logger.Info("應用設定變更");
        base.Apply();
    }
}
```

---

## 立即改進 #3：改良語言下拉選單

### 增強型下拉選單實作

```csharp
// ✅ 改進的語言選項提供方法
private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
{
    // 安全檢查
    try
    {
        return new DropdownItem<DemandLevel>[]
        {
            new DropdownItem<DemandLevel>
            {
                value = DemandLevel.Off,
                displayName = GetLocalizedEnumName(DemandLevel.Off)
            },
            new DropdownItem<DemandLevel>
            {
                value = DemandLevel.Low,
                displayName = GetLocalizedEnumName(DemandLevel.Low)
            },
            new DropdownItem<DemandLevel>
            {
                value = DemandLevel.Medium,
                displayName = GetLocalizedEnumName(DemandLevel.Medium)
            },
            new DropdownItem<DemandLevel>
            {
                value = DemandLevel.High,
                displayName = GetLocalizedEnumName(DemandLevel.High)
            },
            new DropdownItem<DemandLevel>
            {
                value = DemandLevel.Maximum,
                displayName = GetLocalizedEnumName(DemandLevel.Maximum)
            }
        };
    }
    catch (Exception ex)
    {
        Logger.Exception(ex, "GetDemandLevelOptions");
        return new DropdownItem<DemandLevel>[0];
    }
}

// ✅ 改進的本地化名稱取得（含降級機制）
private string GetLocalizedEnumName(DemandLevel level)
{
    try
    {
        // 1. 嘗試從活動字典取得翻譯
        var manager = GameManager.instance?.localizationManager;
        if (manager?.activeDictionary != null)
        {
            string localeKey = $"Common.ENUM[DemandModifier.DemandLevel.{level}]";
            if (manager.activeDictionary.TryGetValue(localeKey, out string translated) && 
                !string.IsNullOrEmpty(translated))
            {
                Logger.DebugLocale($"Translated: {localeKey} = {translated}");
                return translated;
            }
        }
    }
    catch (Exception ex)
    {
        Logger.DebugLocale($"Translation lookup failed: {ex.Message}");
    }

    // 2. 降級：使用英文預設（含百分比）
    return level switch
    {
        DemandLevel.Off => "Off (0% - Game Default)",
        DemandLevel.Low => "Low (25%)",
        DemandLevel.Medium => "Medium (50%)",
        DemandLevel.High => "High (75%)",
        DemandLevel.Maximum => "Maximum (100%)",
        _ => level.ToString()
    };
}
```

---

## 立即改進 #4：模組化語言系統

### 新建 `Code/Localization/LocaleManager.cs`

```csharp
using System;
using System.Collections.Generic;
using Colossal.Localization;
using Game.SceneFlow;

namespace DemandModifier
{
    public partial class Localization
    {
        /// <summary>
        /// 管理模組語言的切換和初始化
        /// </summary>
        internal class LocaleManager : IDisposable
        {
            private LocalizationManager _vanillaLocalizationManager;
            private string _prevGameLocale;
            private VanillaLocalizationObserver _localizationObserver;
            private bool _disposed;

            public LocaleManager()
            {
                _vanillaLocalizationManager = GameManager.instance.localizationManager;
                _prevGameLocale = _vanillaLocalizationManager.activeLocaleId;
                Logger.DebugLocale("LocaleManager 初始化");
            }

            /// <summary>
            /// 註冊遊戲語言變更觀察器
            /// </summary>
            public void RegisterVanillaLocalizationObserver(ModSettings settings)
            {
                _localizationObserver?.Dispose();
                _localizationObserver = new VanillaLocalizationObserver(this, settings);
            }

            /// <summary>
            /// 清理資源
            /// </summary>
            public void Dispose()
            {
                if (_disposed) return;

                _localizationObserver?.Dispose();
                _localizationObserver = null;
                _vanillaLocalizationManager = null;

                _disposed = true;
                Logger.DebugLocale("LocaleManager 已清理");
            }

            /// <summary>
            /// 遊戲語言變更觀察器（內部類別）
            /// </summary>
            private class VanillaLocalizationObserver : IDisposable
            {
                private LocaleManager _localeManager;
                private ModSettings _settings;
                private bool _disableChangeCallback;

                public VanillaLocalizationObserver(LocaleManager localeManager, ModSettings settings)
                {
                    _localeManager = localeManager;
                    _settings = settings;
                    GameManager.instance.localizationManager.onActiveDictionaryChanged += OnActiveDictionaryChanged;
                    Logger.DebugLocale("VanillaLocalizationObserver 已訂閱");
                }

                public void EnableObserver() => _disableChangeCallback = false;
                public void DisableObserver() => _disableChangeCallback = true;

                private void OnActiveDictionaryChanged()
                {
                    if (_disableChangeCallback) return;

                    Logger.DebugLocale("遊戲語言已變更");

                    var manager = GameManager.instance.localizationManager;
                    string newLocale = manager.activeLocaleId;

                    if (_settings.UseGameLanguage)
                    {
                        Logger.DebugLocale($"同步為遊戲語言: {newLocale}");
                        _settings.CurrentLocale = newLocale;
                    }
                }

                public void Dispose()
                {
                    GameManager.instance.localizationManager.onActiveDictionaryChanged -= OnActiveDictionaryChanged;
                    _localeManager = null;
                    _settings = null;
                    Logger.DebugLocale("VanillaLocalizationObserver 已清理");
                }
            }
        }
    }
}
```

### 新建 `Code/Localization/UIKeys.cs`

```csharp
namespace DemandModifier
{
    /// <summary>
    /// 集中管理所有遊戲內 UI 顯示的鍵值
    /// </summary>
    public static class UIKeys
    {
        // 主要功能描述
        public const string DEMAND_MODIFIER = "UI.DEMAND_MODIFIER";
        public const string DEMAND_MODIFIER_DESCRIPTION = "UI.DEMAND_MODIFIER_DESCRIPTION";

        // 需求控制
        public const string RESIDENTIAL_DEMAND = "UI.RESIDENTIAL_DEMAND";
        public const string COMMERCIAL_DEMAND = "UI.COMMERCIAL_DEMAND";
        public const string INDUSTRIAL_DEMAND = "UI.INDUSTRIAL_DEMAND";

        // 工具提示
        public const string TOOLTIP_RESIDENTIAL = "UI.TOOLTIP_RESIDENTIAL";
        public const string TOOLTIP_COMMERCIAL = "UI.TOOLTIP_COMMERCIAL";
        public const string TOOLTIP_INDUSTRIAL = "UI.TOOLTIP_INDUSTRIAL";

        // 錯誤訊息
        public const string ERROR_SETTINGS_NOT_LOADED = "UI.ERROR_SETTINGS_NOT_LOADED";
        public const string ERROR_TRANSLATION_FAILED = "UI.ERROR_TRANSLATION_FAILED";
    }
}
```

---

## 立即改進 #5：補丁最佳實踐

### 改進 `Code/Patches/DemandSystemPatch.cs`

```csharp
using HarmonyLib;
using Unity.Collections;
using Traffic; // 參考使用 Logger 模式

namespace DemandModifier.Code.Patches
{
    /// <summary>
    /// Harmony 補丁基類（提供共用功能）
    /// </summary>
    public abstract class DemandSystemPatchBase
    {
        /// <summary>
        /// 驗證補丁是否可以安全應用
        /// </summary>
        protected static bool ValidateSettings()
        {
            if (DemandModifierMod.Settings == null)
            {
                Logger.Warning("Settings 未初始化");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 安全地取得欄位參考
        /// </summary>
        protected static AccessTools.FieldRef<T, TField> GetFieldRef<T, TField>(string fieldName)
        {
            try
            {
                return AccessTools.FieldRefAccess<T, TField>(fieldName);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"GetFieldRef({fieldName})");
                throw;
            }
        }
    }

    /// <summary>
    /// 住宅需求系統補丁
    /// </summary>
    [HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
    public class ResidentialDemandSystemPatch : DemandSystemPatchBase
    {
        private static AccessTools.FieldRef<ResidentialDemandSystem, NativeValue<int>> 
            BuildingDemandRef = GetFieldRef<ResidentialDemandSystem, NativeValue<int>>("m_BuildingDemand");

        static void Prefix(ResidentialDemandSystem __instance)
        {
            Logger.DebugPatches("ResidentialDemandSystemPatch.Prefix 執行中");

            if (!ValidateSettings()) return;

            if (DemandModifierMod.Settings.ResidentialDemandLevel != DemandLevel.Off)
            {
                var demand = (int)DemandModifierMod.Settings.ResidentialDemandLevel;
                BuildingDemandRef(__instance).value = demand;
                Logger.DebugDemand($"住宅需求修改為: {demand}");
            }
        }
    }

    /// <summary>
    /// 商業需求系統補丁
    /// </summary>
    [HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
    public class CommercialDemandSystemPatch : DemandSystemPatchBase
    {
        private static AccessTools.FieldRef<CommercialDemandSystem, NativeValue<int>> 
            BuildingDemandRef = GetFieldRef<CommercialDemandSystem, NativeValue<int>>("m_BuildingDemand");

        static void Prefix(CommercialDemandSystem __instance)
        {
            Logger.DebugPatches("CommercialDemandSystemPatch.Prefix 執行中");

            if (!ValidateSettings()) return;

            if (DemandModifierMod.Settings.CommercialDemandLevel != DemandLevel.Off)
            {
                var demand = (int)DemandModifierMod.Settings.CommercialDemandLevel;
                BuildingDemandRef(__instance).value = demand;
                Logger.DebugDemand($"商業需求修改為: {demand}");
            }
        }
    }

    /// <summary>
    /// 工業需求系統補丁
    /// </summary>
    [HarmonyPatch(typeof(IndustrialDemandSystem), "OnUpdate")]
    public class IndustrialDemandSystemPatch : DemandSystemPatchBase
    {
        private static AccessTools.FieldRef<IndustrialDemandSystem, NativeValue<int>> 
            BuildingDemandRef = GetFieldRef<IndustrialDemandSystem, NativeValue<int>>("m_BuildingDemand");

        static void Prefix(IndustrialDemandSystem __instance)
        {
            Logger.DebugPatches("IndustrialDemandSystemPatch.Prefix 執行中");

            if (!ValidateSettings()) return;

            if (DemandModifierMod.Settings.IndustrialDemandLevel != DemandLevel.Off)
            {
                var demand = (int)DemandModifierMod.Settings.IndustrialDemandLevel;
                BuildingDemandRef(__instance).value = demand;
                Logger.DebugDemand($"工業需求修改為: {demand}");
            }
        }
    }
}
```

---

## 改進檢查清單

- [ ] **日誌系統**
  - [ ] 建立 `Code/Utils/Logger.cs`
  - [ ] 在 `DemandModifierMod.cs` 使用 `Logger.Info/Error` 取代 `log`
  - [ ] 在所有補丁加入 `Logger.DebugPatches`

- [ ] **ModSettings 增強**
  - [ ] 新增常數（Tab, Section）
  - [ ] 新增 `SettingsUIValueVersion` 屬性修飾器
  - [ ] 新增回調方法（OnChanged）
  - [ ] 實作 `SetDefaults()` 和 `Apply()` 方法

- [ ] **語言系統**
  - [ ] 建立 `LocaleManager.cs`
  - [ ] 建立 `UIKeys.cs`
  - [ ] 改進下拉選單翻譯邏輯
  - [ ] 測試所有語言切換

- [ ] **補丁改進**
  - [ ] 建立補丁基類
  - [ ] 新增驗證方法
  - [ ] 新增詳細日誌

- [ ] **文檔更新**
  - [ ] 更新 README 說明改進
  - [ ] 新增遷移指南
  - [ ] 記錄 API 變更

---

## 測試驗證方案

### 日誌驗證
```powershell
# 在遊戲日誌中查看
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Tail 20

# 應該看到：
# [DemandModifier.DemandModifierMod] DemandModifier 模組載入中
# [DEMAND] 住宅需求修改為: 255
```

### 設定驗證
- 遊戲設定 → Mods → DemandModifier
- [ ] 下拉選單顯示翻譯文本（非鍵值）
- [ ] 回調方法正確執行
- [ ] 值變更實時反映在遊戲

### 語言驗證
- [ ] 測試所有 7 種語言
- [ ] 驗證下拉選單每種語言均顯示
- [ ] 檢查日誌無翻譯警告

---

## 下一步行動

1. **即刻實施**
   - 複製 Logger.cs 到您的專案
   - 更新補丁使用新 Logger

2. **短期改進**（1-2 週）
   - 建立語言管理器
   - 重組設定系統

3. **長期演進**（1 個月）
   - 完整文檔化
   - 社群反饋整合

---

## 參考資源

- **Traffic 原始碼**：https://github.com/krzychu124/Traffic
- **Harmony 文件**：https://harmony.pardeike.net/
- **Cities Skylines 2 API**：官方 Modding 文件
- **本專案指南**：見主 README 和相關文件

