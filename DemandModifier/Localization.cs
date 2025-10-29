using System;
using System.Collections.Generic;
using System.IO;
using Colossal;
using Game.Modding;
using Game.SceneFlow;

namespace DemandModifier
{
    /// <summary>
    /// 多國語系管理系統 - 嚴格按照 Traffic 專案實作模式
    /// 提供語系鍵值的產生和翻譯字典的管理
    /// </summary>
    public partial class Localization
    {
        /// <summary>
        /// 靜態字典，儲存所有可用的語言資訊
        /// 格式：Key = 語言代碼 (如 "en-US")
        /// Value = (顯示名稱如"English", 翻譯進度%如"100", IDictionarySource實例)
        /// 
        /// 此字典由各個 LocaleXX 類別在其建構子中自動填入
        /// </summary>
        internal static readonly Dictionary<string, Tuple<string, string, IDictionarySource>> LocaleSources = 
            new Dictionary<string, Tuple<string, string, IDictionarySource>>();

        /// <summary>
        /// 語言來源版本號，用於追蹤翻譯更新
        /// 當任何翻譯有更新時，此值會改變以通知遊戲
        /// </summary>
        private static int _languageSourceVersion = 1;

        /// <summary>
        /// 取得語言來源版本號 - 當翻譯有更新時，此值會改變
        /// </summary>
        internal static int LanguageSourceVersion => _languageSourceVersion;

        /// <summary>
        /// 從磁碟載入所有語言檔案 (l10n/*.json)
        /// 遊戲引擎會自動呼叫此方法載入模組的語系檔案
        /// 
        /// 流程：
        /// 1. 建立英文語言源並填入 LocaleSources
        /// 2. 掃描 l10n/ 資料夾載入所有 JSON 檔案
        /// 3. 註冊到遊戲的語系管理器
        /// 
        /// 嚴格參考 Traffic 專案的 LoadLocales 實作模式
        /// 注意：此方法在 OnLoad 中只是註冊磁碟位置，
        /// 實際的 JSON 載入由遊戲引擎自動處理
        /// </summary>
        /// <param name="mod">模組實例</param>
        /// <param name="refTranslationCount">參考翻譯計數，用於進度追蹤</param>
        internal static void LoadLocales(IMod mod, float refTranslationCount)
        {
            try
            {
                if (GameManager.instance.modManager.TryGetExecutableAsset(mod, out var asset))
                {
                    string directory = Path.Combine(
                        Path.GetDirectoryName(asset.path) ?? "", 
                        "l10n"  // 遊戲期望的語系檔案夾名稱
                    );

                    if (Directory.Exists(directory))
                    {
                        DemandModifierMod.log.Info($"Loading locales from: {directory}");
                        int localeCount = 0;

                        // 掃描所有 *.json 檔案
                        foreach (string localeFile in Directory.EnumerateFiles(directory, "*.json"))
                        {
                            string localeId = Path.GetFileNameWithoutExtension(localeFile);
                            
                            DemandModifierMod.log.Debug(
                                $"Registering locale {localeId} from: {localeFile}"
                            );

                            // 建立 ModLocale 實例
                            // 遊戲引擎會自動掃描 l10n/ 資料夾並從 JSON 檔案載入翻譯
                            var locale = new ModLocale(localeId, localeFile);
                            
                            // 註冊到遊戲的語系管理器
                            if (GameManager.instance.localizationManager != null)
                            {
                                GameManager.instance.localizationManager.AddSource(localeId, locale);
                                localeCount++;
                            }
                        }
                        
                        DemandModifierMod.log.Info($"Locales registered: {localeCount}");
                    }
                    else
                    {
                        DemandModifierMod.log.Warn($"Locale directory not found: {directory}");
                    }
                }
                else
                {
                    DemandModifierMod.log.Error("Unable to get mod asset path");
                }
            }
            catch (Exception ex)
            {
                DemandModifierMod.log.Error($"Error loading locales: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
