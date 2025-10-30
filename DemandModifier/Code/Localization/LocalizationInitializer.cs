using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game.SceneFlow;
using System;
using System.Collections.Generic;
using System.IO;

namespace DemandModifier.Localization
{
    /// <summary>
    /// 多國語言系統初始化器
    /// 自動掃描並載入 l10n 資料夾中的所有語言檔案
    /// 參考 Traffic 專案實作模式
    /// </summary>
    public static class LocalizationInitializer
    {
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}.{2}", nameof(DemandModifier), nameof(Localization), nameof(LocalizationInitializer))
        ).SetShowsErrorsInUI(false);

        /// <summary>
        /// 初始化多國語言系統
        /// 掃描 l10n 資料夾並向遊戲本地化系統註冊所有語言來源
        /// </summary>
        public static void Initialize()
        {
            try
            {
                log.Info("================== 開始初始化多國語言系統 ==================");

                // 取得模組資產路徑
                if (null == GameManager.instance)
                {
                    log.Error("❌ 遊戲管理器未初始化，無法載入語言");
                    return;
                }

                log.Info("✓ 遊戲管理器已初始化");

                // 使用 AssetDatabase 取得模組位置
                string modDirectory = null;
                
                // 方法 0: 直接使用 Mods\DemandModifier（優先，最可靠）
                try
                {
                    string modsPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "..",
                        "LocalLow",
                        "Colossal Order",
                        "Cities Skylines II",
                        "Mods",
                        "DemandModifier"
                    );
                    
                    log.Debug(string.Format("方法 0: 嘗試路徑 {0}", modsPath));
                    
                    if (Directory.Exists(modsPath))
                    {
                        modDirectory = modsPath;
                        log.Info(string.Format("✓ 方法 0 成功: {0}", modDirectory));
                    }
                    else
                    {
                        log.Debug("方法 0: 路徑不存在");
                    }
                }
                catch (Exception ex0)
                {
                    log.Debug(string.Format("❌ 方法 0 (直接 Mods 搜索) 失敗: {0}", ex0.Message));
                }

                // 方法 1: 使用 Assembly CodeBase，並在路徑中尋找 \Mods\DemandModifier
                if (string.IsNullOrEmpty(modDirectory))
                {
                    try
                    {
                        var executingModule = typeof(DemandModifierMod);
                        string codeBasePath = executingModule.Assembly.CodeBase;
                        log.Debug(string.Format("方法 1: Assembly CodeBase = {0}", codeBasePath));
                        
                        if (!string.IsNullOrEmpty(codeBasePath))
                        {
                            // 移除 file:// 字首
                            if (codeBasePath.StartsWith("file:///"))
                            {
                                codeBasePath = codeBasePath.Substring(8);
                            }
                            else if (codeBasePath.StartsWith("file://"))
                            {
                                codeBasePath = codeBasePath.Substring(7);
                            }
                            
                            log.Debug(string.Format("方法 1: 清理後路徑 = {0}", codeBasePath));
                            
                            // 尋找路徑中的 \Mods\DemandModifier 部分
                            int modsIndex = codeBasePath.IndexOf("\\Mods\\DemandModifier", StringComparison.OrdinalIgnoreCase);
                            if (modsIndex >= 0)
                            {
                                modDirectory = codeBasePath.Substring(0, modsIndex + "\\Mods\\DemandModifier".Length);
                                log.Info(string.Format("✓ 方法 1 成功 (CodeBase): {0}", modDirectory));
                            }
                            else
                            {
                                // 降級: 使用檔案所在目錄（假設在 DemandModifier 目錄內）
                                modDirectory = Path.GetDirectoryName(codeBasePath);
                                log.Info(string.Format("✓ 方法 1 降級: {0}", modDirectory));
                            }
                        }
                        else
                        {
                            log.Debug("方法 1: CodeBase 為空");
                        }
                    }
                    catch (Exception ex1)
                    {
                        log.Debug(string.Format("❌ 方法 1 (CodeBase) 失敗: {0}", ex1.Message));
                    }
                }

                // 方法 2: 使用 Location 屬性，並在路徑中尋找 \Mods\DemandModifier
                if (string.IsNullOrEmpty(modDirectory))
                {
                    try
                    {
                        var executingModule = typeof(DemandModifierMod);
                        string location = executingModule.Assembly.Location;
                        log.Debug(string.Format("方法 2: Assembly Location = {0}", location));
                        
                        if (!string.IsNullOrEmpty(location))
                        {
                            // 尋找路徑中的 \Mods\DemandModifier 部分
                            int modsIndex = location.IndexOf("\\Mods\\DemandModifier", StringComparison.OrdinalIgnoreCase);
                            if (modsIndex >= 0)
                            {
                                modDirectory = location.Substring(0, modsIndex + "\\Mods\\DemandModifier".Length);
                                log.Info(string.Format("✓ 方法 2 成功 (Location): {0}", modDirectory));
                            }
                            else
                            {
                                // 降級: 使用檔案所在目錄
                                modDirectory = Path.GetDirectoryName(location);
                                log.Info(string.Format("✓ 方法 2 降級: {0}", modDirectory));
                            }
                        }
                        else
                        {
                            log.Debug("方法 2: Location 為空");
                        }
                    }
                    catch (Exception ex2)
                    {
                        log.Debug(string.Format("❌ 方法 2 (Location) 失敗: {0}", ex2.Message));
                    }
                }

                // 檢查結果是否為有效的 DemandModifier 模組目錄
                if (!string.IsNullOrEmpty(modDirectory))
                {
                    // 驗證是否在 Mods\DemandModifier 目錄中
                    if (!modDirectory.EndsWith("DemandModifier", StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warn(string.Format("⚠️ 識別的目錄不是 DemandModifier: {0}", modDirectory));
                        modDirectory = null;
                    }
                    else
                    {
                        log.Info(string.Format("✓ 模組目錄已驗證: {0}", modDirectory));
                    }
                }

                if (string.IsNullOrEmpty(modDirectory))
                {
                    log.Error("❌ 無法使用任何方法取得模組位置");
                    return;
                }

                string localizationPath = Path.Combine(modDirectory, "l10n");

                log.Info(string.Format("模組位置: {0}", modDirectory));
                log.Info(string.Format("語言資料夾: {0}", localizationPath));

                if (!Directory.Exists(localizationPath))
                {
                    log.Error(string.Format("❌ 語言資料夾不存在: {0}", localizationPath));
                    return;
                }

                log.Info(string.Format("✓ 語言資料夾存在"));

                // 掃描所有 JSON 檔案
                string[] jsonFiles = Directory.GetFiles(localizationPath, "*.json");
                log.Info(string.Format("找到 {0} 個語言檔案", jsonFiles.Length));

                if (jsonFiles.Length == 0)
                {
                    log.Error("❌ 未找到語言檔案 (.json)");
                    return;
                }

                foreach (string jsonFile in jsonFiles)
                {
                    log.Debug(string.Format("  • {0}", Path.GetFileName(jsonFile)));
                }

                // 支援的語言對應表
                Dictionary<string, string> localeMapping = new Dictionary<string, string>();
                localeMapping.Add("en-US", "en-US");
                localeMapping.Add("zh-HANT", "zh-HANT");
                localeMapping.Add("zh-HANS", "zh-HANS");
                localeMapping.Add("ja-JP", "ja-JP");
                localeMapping.Add("de-DE", "de-DE");
                localeMapping.Add("es-ES", "es-ES");
                localeMapping.Add("fr-FR", "fr-FR");

                int successCount = 0;
                int failureCount = 0;

                foreach (string jsonFile in jsonFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(jsonFile);
                    
                    try
                    {
                        log.Info(string.Format("載入語言檔案: {0}", fileName));
                        
                        // 建立語言來源實例
                        ModLocale locale = new ModLocale(fileName, jsonFile);
                        log.Debug(string.Format("  • ModLocale 實例已建立"));
                        
                        locale.Load();
                        log.Info(string.Format("  ✓ 語言資料已載入"));

                        // 取得對應的語言 ID
                        string localeId = fileName;
                        if (localeMapping.ContainsKey(fileName))
                        {
                            localeId = localeMapping[fileName];
                            log.Debug(string.Format("  • 語言 ID 對應: {0} → {1}", fileName, localeId));
                        }

                        // 向遊戲本地化管理器註冊
                        if (GameManager.instance != null && GameManager.instance.localizationManager != null)
                        {
                            GameManager.instance.localizationManager.AddSource(localeId, locale);
                            log.Info(string.Format("  ✓ 已向遊戲本地化系統註冊: {0}", localeId));
                            successCount++;
                        }
                        else
                        {
                            log.Error(string.Format("  ❌ 本地化管理器不可用: {0}", fileName));
                            failureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("  ❌ 語言載入失敗 {0}: {1}", fileName, ex.Message));
                        log.Debug(string.Format("     堆疊追蹤: {0}", ex.StackTrace));
                        failureCount++;
                    }
                }

                log.Info(string.Format("==================== 語言系統初始化完成 ===================="));
                log.Info(string.Format("成功: {0}, 失敗: {1}", successCount, failureCount));
                
                if (failureCount > 0)
                {
                    log.Warn(string.Format("⚠️ 有 {0} 個語言載入失敗，請檢查日誌", failureCount));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("❌ 多國語言系統初始化異常: {0}", ex.Message));
                log.Error(string.Format("堆疊追蹤: {0}", ex.StackTrace));
            }
        }

        /// <summary>
        /// 列出所有已載入的可用語言
        /// 用於除錯和驗證
        /// </summary>
        public static void ListAvailableLocales()
        {
            try
            {
                if (GameManager.instance == null || GameManager.instance.localizationManager == null)
                {
                    log.Warn("⚠️ 本地化管理器無法存取");
                    return;
                }

                log.Info("═════════════════════════════════════");
                log.Info("可用語言列表");
                log.Info(string.Format("當前活躍語言: {0}", 
                    GameManager.instance.localizationManager.activeLocaleId));
                log.Info("═════════════════════════════════════");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("❌ 列出語言時發生異常: {0}", ex.Message));
                log.Debug(string.Format("堆疊追蹤: {0}", ex.StackTrace));
            }
        }
    }
}
