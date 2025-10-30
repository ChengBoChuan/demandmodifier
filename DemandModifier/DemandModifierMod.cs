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
        /// 模組版本 - 與 PublishConfiguration.xml 中的 ModVersion 同步
        /// 請在發佈新版本時手動更新此值
        /// </summary>
        private const string MOD_VERSION = "0.3.2";
        
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
            log.Info(string.Format("📦 模組版本: {0}", MOD_VERSION));
            log.Info(string.Format("🕐 載入時間: {0}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)));
            log.Info(string.Format(".NET Framework: {0}", System.Environment.Version));
            Utils.Logger.Separator(string.Format("DemandModifier v{0} 初始化", MOD_VERSION));

            try
            {
                // ===== 初始化新日誌系統 =====
                log.Info("初始化進階日誌系統...");
                Utils.Logger.Initialize(nameof(DemandModifier), nameof(DemandModifierMod));
                log.Info("✓ 日誌系統已初始化成功");
                Utils.Logger.Info("✓ 日誌系統已初始化成功");
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format("❌ 日誌系統初始化失敗: {0}", ex.Message));
                log.Error(string.Format("堆疊追蹤: {0}", ex.StackTrace));
            }

            try
            {
                // ===== 初始化多國語言系統 =====
                log.Info("初始化多國語言系統...");
                Utils.Logger.MethodEnter("LocalizationInitializer.Initialize");
                LocalizationInitializer.Initialize();
                log.Info("✓ 語言系統初始化完成");
                LocalizationInitializer.ListAvailableLocales();
                log.Info("✓ 語言系統已驗證");
                Utils.Logger.Info("✓ 語言系統已初始化");
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format("❌ 語言系統初始化失敗: {0}", ex.Message));
                log.Error(string.Format("堆疊追蹤: {0}", ex.StackTrace));
                Utils.Logger.Error("語言系統初始化失敗: " + ex.Message);
                Utils.Logger.Exception(ex, "多國語言系統初始化");
            }

            try
            {
                // ===== 初始化模組設定 =====
                log.Info("初始化模組設定...");
                log.Info("建立設定實例...");
                Settings = new DemandModifierSettings(this);
                log.Info("✓ 設定實例已建立");
                
                log.Info("在選項 UI 中註冊設定...");
                Settings.RegisterInOptionsUI();
                log.Info("✓ 設定已在 UI 中註冊");
                
                // 從全域資料庫載入設定
                log.Info("從全域資料庫載入設定...");
                AssetDatabase.global.LoadSettings(nameof(DemandModifier), Settings, new DemandModifierSettings(this));
                log.Info("✓ 設定已從資料庫載入");
                
                log.Info("✓ 設定初始化完成");
                log.Info(string.Format("  • 住宅需求: {0}", Settings.ResidentialDemandLevel));
                log.Info(string.Format("  • 商業需求: {0}", Settings.CommercialDemandLevel));
                log.Info(string.Format("  • 工業需求: {0}", Settings.IndustrialDemandLevel));
                log.Info(string.Format("  • 無限電力: {0}", Settings.EnableUnlimitedElectricity));
                log.Info(string.Format("  • 無限水: {0}", Settings.EnableUnlimitedWater));
                Utils.Logger.Info("設定已載入 - 住宅: {0}, 商業: {1}, 工業: {2}",
                    Settings.ResidentialDemandLevel, 
                    Settings.CommercialDemandLevel,
                    Settings.IndustrialDemandLevel);
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format("❌ 設定系統初始化失敗: {0}", ex.Message));
                log.Error(string.Format("堆疊追蹤: {0}", ex.StackTrace));
                Utils.Logger.Error("設定系統初始化失敗: " + ex.Message);
                Utils.Logger.Exception(ex, "設定系統初始化");
            }

            try
            {
                // ===== 初始化 Harmony 補丁系統 =====
                log.Info("初始化 Harmony 補丁系統...");
                string harmonyId = string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod));
                log.Info(string.Format("Harmony ID: {0}", harmonyId));
                
                harmony = new Harmony(harmonyId);
                log.Info("✓ Harmony 實例已建立");
                
                log.Info("開始套用補丁...");
                harmony.PatchAll(typeof(DemandModifierMod).Assembly);
                log.Info("✓ 所有補丁已套用");
                
                log.Info("✓ Harmony 補丁系統初始化完成");
                Utils.Logger.Info("✓ Harmony 補丁系統已初始化");
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format("❌ Harmony 補丁系統初始化失敗: {0}", ex.Message));
                log.Error(string.Format("堆疊追蹤: {0}", ex.StackTrace));
                Utils.Logger.Error("Harmony 補丁系統初始化失敗: " + ex.Message);
                Utils.Logger.Exception(ex, "Harmony 補丁系統初始化");
            }

            log.Info(string.Format("✅ 載入完成時間: {0}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)));
            log.Info("▶ ════════════ DemandModifier 載入完成 ════════════");
        }

        /// <summary>
        /// Mod 卸載時執行 - 清理資源
        /// </summary>
        public void OnDispose()
        {
            log.Info("═══════════════ DemandModifier 卸載開始 ═══════════════");
            log.Info(string.Format("卸載時間: {0}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)));
            
            try
            {
                // 清理 Harmony 補丁
                log.Info("卸載 Harmony 補丁...");
                if (harmony != null)
                {
                    string harmonyId = string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod));
                    log.Info(string.Format("Harmony ID: {0}", harmonyId));
                    harmony.UnpatchAll(harmonyId);
                    log.Info("✓ Harmony 補丁已成功卸載");
                    harmony = null;
                }
                else
                {
                    log.Warn("⚠️ Harmony 實例為 null，無法卸載");
                }
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format("❌ Harmony 補丁卸載失敗: {0}", ex.Message));
                log.Error(string.Format("堆疊追蹤: {0}", ex.StackTrace));
            }

            try
            {
                // 儲存並清理設定
                log.Info("卸載設定...");
                if (Settings != null)
                {
                    log.Info("從 UI 移除設定...");
                    Settings.UnregisterInOptionsUI();
                    log.Info("✓ 設定已從 UI 移除");
                    Settings = null;
                    log.Info("✓ 設定已清理");
                }
                else
                {
                    log.Warn("⚠️ 設定實例為 null");
                }
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format("❌ 設定卸載失敗: {0}", ex.Message));
                log.Error(string.Format("堆疊追蹤: {0}", ex.StackTrace));
            }

            log.Info("═══════════════ DemandModifier 卸載完成 ═══════════════");
        }
    }
}
