using Colossal.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DemandModifier.Localization
{
    /// <summary>
    /// 語言管理器 - 參考 Traffic 專案實作
    /// 提供動態語言切換、字典快取、語言偵測等功能
    /// </summary>
    public static class LocaleManager
    {
        /// <summary>
        /// 支援的語言列表
        /// </summary>
        public static readonly string[] SupportedLocales = new string[]
        {
            "en-US",      // 英文（美國）
            "de-DE",      // 德文（德國）
            "es-ES",      // 西班牙文（西班牙）
            "fr-FR",      // 法文（法國）
            "ja-JP",      // 日文（日本）
            "zh-HANS",    // 簡體中文（中國）
            "zh-HANT"     // 繁體中文（台灣）
        };

        /// <summary>
        /// 語言親和性映射 - 若使用者語言不支援，尋找最相近的語言
        /// </summary>
        private static readonly Dictionary<string, string> LocaleFallbacks = new Dictionary<string, string>
        {
            { "de", "de-DE" },
            { "es", "es-ES" },
            { "fr", "fr-FR" },
            { "ja", "ja-JP" },
            { "zh", "zh-HANS" },
            { "pt-BR", "es-ES" },    // 葡萄牙文降級到西班牙文
            { "pt", "es-ES" },
            { "it", "es-ES" },       // 義大利文降級到西班牙文
            { "ko", "ja-JP" },       // 韓文降級到日文
            { "ru", "en-US" }        // 俄文降級到英文
        };

        /// <summary>
        /// 語言顯示名稱映射
        /// </summary>
        private static readonly Dictionary<string, string> LocaleDisplayNames = new Dictionary<string, string>
        {
            { "en-US", "English" },
            { "de-DE", "Deutsch" },
            { "es-ES", "Español" },
            { "fr-FR", "Français" },
            { "ja-JP", "日本語" },
            { "zh-HANS", "简体中文" },
            { "zh-HANT", "繁體中文" }
        };

        /// <summary>
        /// 快取的語言字典
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> _localeDictionaries = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// 當前活躍的語言
        /// </summary>
        private static string _currentLocale = "en-US";

        /// <summary>
        /// 語言變更事件
        /// </summary>
        public static event Action<string> OnLocaleChanged;

        /// <summary>
        /// 初始化語言管理器
        /// </summary>
        public static void Initialize()
        {
            try
            {
                string detectedLocale = DetectCurrentLocale();
                SetCurrentLocale(detectedLocale);
                Utils.Logger.Info("語言管理器已初始化: {0}", _currentLocale);
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("語言管理器初始化失敗: {0}", ex.Message);
                _currentLocale = "en-US";
            }
        }

        /// <summary>
        /// 偵測當前系統語言
        /// </summary>
        public static string DetectCurrentLocale()
        {
            try
            {
                if (Game.SceneFlow.GameManager.instance?.localizationManager == null)
                {
                    return "en-US";
                }

                // 嘗試從遊戲定位管理器取得當前語言
                var currentDict = Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary;
                if (currentDict != null)
                {
                    // 檢查是否為支援的語言
                    foreach (var locale in SupportedLocales)
                    {
                        if (IsLocaleActive(locale, currentDict))
                        {
                            return locale;
                        }
                    }
                }

                // 回退到系統語言偵測
                CultureInfo culture = CultureInfo.CurrentUICulture;
                string localeCode = culture.Name;  // 例如 "en-US", "de-DE"

                if (Array.Exists(SupportedLocales, element => element == localeCode))
                {
                    return localeCode;
                }

                // 嘗試僅使用語言代碼的親和性匹配
                string languageOnly = localeCode.Split('-')[0].ToLower();
                if (LocaleFallbacks.ContainsKey(languageOnly))
                {
                    return LocaleFallbacks[languageOnly];
                }

                return "en-US";
            }
            catch (Exception ex)
            {
                Utils.Logger.Warn("語言偵測失敗，使用預設語言: {0}", ex.Message);
                return "en-US";
            }
        }

        /// <summary>
        /// 設定當前語言
        /// </summary>
        public static void SetCurrentLocale(string locale)
        {
            if (!Array.Exists(SupportedLocales, element => element == locale))
            {
                Utils.Logger.Warn("不支援的語言: {0}，使用英文", locale);
                locale = "en-US";
            }

            if (_currentLocale == locale)
            {
                return;  // 已經是此語言
            }

            _currentLocale = locale;
            Utils.Logger.Info("語言已變更為: {0} ({1})", locale, GetLocaleDisplayName(locale));
            OnLocaleChanged?.Invoke(locale);
        }

        /// <summary>
        /// 取得當前語言
        /// </summary>
        public static string GetCurrentLocale()
        {
            return _currentLocale;
        }

        /// <summary>
        /// 取得語言的顯示名稱
        /// </summary>
        public static string GetLocaleDisplayName(string locale)
        {
            return LocaleDisplayNames.ContainsKey(locale)
                ? LocaleDisplayNames[locale]
                : locale;
        }

        /// <summary>
        /// 取得翻譯字串 - 參數為語系鍵值
        /// </summary>
        public static string GetTranslation(string localeKey)
        {
            try
            {
                if (Game.SceneFlow.GameManager.instance?.localizationManager?.activeDictionary == null)
                {
                    return localeKey;  // 無法取得字典，返回鍵值
                }

                var dict = Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary;
                if (dict.TryGetValue(localeKey, out string translated))
                {
                    return translated;
                }

                // 降級：嘗試英文版本
                if (_currentLocale != "en-US")
                {
                    Utils.Logger.Debug("翻譯鍵值不存在，嘗試降級: {0}", localeKey);
                    return localeKey;  // 返回鍵值作為備用
                }

                return localeKey;
            }
            catch (Exception ex)
            {
                Utils.Logger.Warn("翻譯查詢失敗 ({0}): {1}", localeKey, ex.Message);
                return localeKey;
            }
        }

        /// <summary>
        /// 取得翻譯字串 - 帶降級機制
        /// </summary>
        public static string GetTranslation(string localeKey, string fallback)
        {
            try
            {
                string result = GetTranslation(localeKey);
                // 若結果是鍵值本身（表示未找到翻譯），返回降級值
                if (result == localeKey)
                {
                    return fallback;
                }
                return result;
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// 檢查語系鍵值是否存在於字典
        /// </summary>
        public static bool HasTranslation(string localeKey)
        {
            try
            {
                if (Game.SceneFlow.GameManager.instance?.localizationManager?.activeDictionary == null)
                {
                    return false;
                }

                return Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary.ContainsID(localeKey);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 建立完整的語系鍵值
        /// </summary>
        public static string BuildOptionLocaleKey(string @namespace, string className, string propertyName)
        {
            return string.Format("Options.OPTION[{0}.{1}.{2}]", @namespace, className, propertyName);
        }

        /// <summary>
        /// 建立選項描述語系鍵值
        /// </summary>
        public static string BuildOptionDescriptionLocaleKey(string @namespace, string className, string propertyName)
        {
            return string.Format("Options.OPTION_DESCRIPTION[{0}.{1}.{2}]", @namespace, className, propertyName);
        }

        /// <summary>
        /// 建立分頁語系鍵值
        /// </summary>
        public static string BuildTabLocaleKey(string @namespace, string className, string tabName)
        {
            return string.Format("Options.SECTION[{0}.{1}.{2}]", @namespace, className, tabName);
        }

        /// <summary>
        /// 建立群組語系鍵值
        /// </summary>
        public static string BuildGroupLocaleKey(string @namespace, string className, string groupName)
        {
            return string.Format("Options.GROUP[{0}.{1}.{2}]", @namespace, className, groupName);
        }

        /// <summary>
        /// 建立列舉值語系鍵值
        /// </summary>
        public static string BuildEnumLocaleKey(string @namespace, string enumName, string valueName)
        {
            return string.Format("Common.ENUM[{0}.{1}.{2}]", @namespace, enumName, valueName);
        }

        /// <summary>
        /// 列出所有支援的語言
        /// </summary>
        public static void ListSupportedLocales()
        {
            Utils.Logger.Separator("支援的語言列表");
            foreach (var locale in SupportedLocales)
            {
                Utils.Logger.Info("  • {0} ({1})", locale, GetLocaleDisplayName(locale));
            }
            Utils.Logger.Info("當前語言: {0}", _currentLocale);
        }

        /// <summary>
        /// 內部方法：檢查字典是否為指定語言
        /// </summary>
        private static bool IsLocaleActive(string locale, Colossal.Localization.LocalizationDictionary dict)
        {
            // 嘗試查詢該語言的特定鍵值
            try
            {
                // 檢查是否包含該語言的鍵值（簡單啟發式方法）
                return dict.ContainsID("Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得語言統計資訊（用於除錯）
        /// </summary>
        public static void PrintStatistics()
        {
            Utils.Logger.Separator("語言管理器統計");
            Utils.Logger.Info("支援語言數: {0}", SupportedLocales.Length);
            Utils.Logger.Info("當前語言: {0}", _currentLocale);
            Utils.Logger.Info("快取字典數: {0}", _localeDictionaries.Count);
            
            foreach (var locale in SupportedLocales)
            {
                bool isCached = _localeDictionaries.ContainsKey(locale);
                string status = isCached ? "✓ 已快取" : "○ 未快取";
                Utils.Logger.Debug("  {0}: {1}", locale, status);
            }
        }
    }
}
