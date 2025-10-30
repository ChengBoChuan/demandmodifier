using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using DemandModifier.Localization;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using System.Linq;

namespace DemandModifier
{
    /// <summary>
    /// Demand Modifier Mod 主類別 - 實作 IMod 介面以整合到 Cities: Skylines 2
    /// 提供需求控制、服務控制和經濟控制功能
    /// </summary>
    public class DemandModifierMod : IMod
    {
        /// <summary>
        /// 日誌記錄器 - 用於記錄 mod 的資訊和錯誤訊息
        /// SetShowsErrorsInUI(false) 防止錯誤訊息顯示在遊戲 UI 中
        /// </summary>
        public static ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod))
        ).SetShowsErrorsInUI(false);
        
        /// <summary>
        /// 模組設定實例 - 提供全域存取的設定選項
        /// </summary>
        public static DemandModifierSettings Settings { get; private set; }

        private Harmony harmony;

        /// <summary>
        /// Mod 載入時執行 - 註冊設定介面、語言系統和 Harmony 補丁到遊戲系統
        /// </summary>
        /// <param name="updateSystem">遊戲更新系統</param>
        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info("▶ ════════════ DemandModifier 載入開始 ════════════");

            try
            {
                // ===== 初始化新日誌系統 =====
                log.Info("初始化進階日誌系統...");
                Utils.Logger.Initialize(nameof(DemandModifier), nameof(DemandModifierMod));
                Utils.Logger.Info("✓ 日誌系統已初始化");
            }
            catch (System.Exception ex)
            {
                log.Error("日誌系統初始化失敗: " + ex.Message);
            }

            try
            {
                // ===== 初始化多國語言系統 =====
                log.Info("初始化多國語言系統...");
                LocalizationInitializer.Initialize();
                LocalizationInitializer.ListAvailableLocales();
                Utils.Logger.Info("✓ 語言系統已初始化");
            }
            catch (System.Exception ex)
            {
                log.Error("語言系統初始化失敗: " + ex.Message);
                Utils.Logger.Error("語言系統初始化失敗: " + ex.Message);
            }

            try
            {
                // ===== 初始化模組設定 =====
                log.Info("初始化模組設定...");
                Settings = new DemandModifierSettings(this);
                Settings.RegisterInOptionsUI();
                
                // 從全域資料庫載入設定
                AssetDatabase.global.LoadSettings(nameof(DemandModifier), Settings, new DemandModifierSettings(this));
                
                log.Info("✓ 設定已載入");
                Utils.Logger.Info("設定已載入 - 住宅: {0}, 商業: {1}, 工業: {2}",
                    Settings.ResidentialDemandLevel, 
                    Settings.CommercialDemandLevel,
                    Settings.IndustrialDemandLevel);
            }
            catch (System.Exception ex)
            {
                log.Error("設定系統初始化失敗: " + ex.Message);
                Utils.Logger.Error("設定系統初始化失敗: " + ex.Message);
            }

            try
            {
                // ===== 初始化 Harmony 補丁系統 =====
                log.Info("初始化 Harmony 補丁系統...");
                harmony = new Harmony(string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod)));
                harmony.PatchAll(typeof(DemandModifierMod).Assembly);
                
                log.Info("✓ Harmony 補丁註冊完成");
                Utils.Logger.Info("✓ Harmony 補丁系統已初始化");
            }
            catch (System.Exception ex)
            {
                log.Error("Harmony 補丁系統初始化失敗: " + ex.Message);
                Utils.Logger.Error("Harmony 補丁系統初始化失敗: " + ex.Message);
            }

            log.Info("▶ ════════════ DemandModifier 載入完成 ════════════");
        }

        /// <summary>
        /// Mod 卸載時執行 - 清理資源
        /// </summary>
        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            
            try
            {
                // 清理 Harmony 補丁
                if (harmony != null)
                {
                    harmony.UnpatchAll(string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod)));
                    log.Info("✓ Harmony 補丁已卸載");
                }
            }
            catch (System.Exception ex)
            {
                log.Error("Harmony 補丁卸載失敗: " + ex.Message);
            }

            try
            {
                // 儲存並清理設定
                if (Settings != null)
                {
                    Settings.UnregisterInOptionsUI();
                    Settings = null;
                    log.Info("✓ 設定已卸載");
                }
            }
            catch (System.Exception ex)
            {
                log.Error("設定卸載失敗: " + ex.Message);
            }
        }
    }
}
