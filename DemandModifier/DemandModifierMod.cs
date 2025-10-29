using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
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
        public static ILog log = LogManager.GetLogger($"{nameof(DemandModifier)}.{nameof(DemandModifierMod)}").SetShowsErrorsInUI(false);
        
        /// <summary>
        /// 模組設定實例 - 提供全域存取的設定選項
        /// </summary>
        public static DemandModifierSettings Settings { get; private set; }

        /// <summary>
        /// Mod 載入時執行 - 註冊設定介面和 Harmony 補丁到遊戲系統
        /// 
        /// 關鍵流程（嚴格參考 Traffic 專案 OnLoad 實作）：
        /// 1. 建立英文語言源（填入 LocaleSources["en-US"]）
        /// 2. 建立並註冊模組設定實例
        /// 3. 從全域資料庫載入儲存的設定
        /// 4. 掃描並載入所有 JSON 語系檔案
        /// 5. 建立 Harmony 實例並套用所有補丁
        /// 
        /// ⚠️ 順序很重要：必須先建立 LocaleEN（填入 LocaleSources），
        /// 然後建立設定實例，最後註冊 UI
        /// 這樣設定 UI 初始化時，LocaleSources 字典才會有內容可讀
        /// </summary>
        /// <param name="updateSystem">遊戲更新系統</param>
        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            // 記錄 Mod 資產路徑
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info(string.Format("Current mod asset at {0}", asset.path));

            // 步驟 1：建立英文語言源
            // ⚠️ 必須在建立 Settings 之前執行
            // LocaleEN 建構子會自動填入 LocaleSources["en-US"]
            var dummySettings = new DemandModifierSettings(this);
            var localeEN = new Localization.LocaleEN(dummySettings);
            log.Info("英文語言源已建立");

            // 步驟 2：建立並註冊模組設定
            Settings = dummySettings;
            Settings.RegisterInOptionsUI();
            log.Info("模組設定已註冊到 UI");
            
            // 步驟 3：從全域資料庫載入設定（或使用預設值）
            AssetDatabase.global.LoadSettings(nameof(DemandModifier), Settings, new DemandModifierSettings(this));
            
            log.Info(string.Format(
                "設定已載入 - 住宅需求: {0}, 商業需求: {1}, 工業需求: {2}",
                Settings.ResidentialDemandLevel,
                Settings.CommercialDemandLevel,
                Settings.IndustrialDemandLevel
            ));

            // 步驟 4：從磁碟載入所有 JSON 語系檔案並註冊
            // LoadLocales 會掃描 l10n/ 資料夾
            Localization.LoadLocales(this, localeEN.ReadEntries(null, null).Count());
            
            log.Info("多國語系載入完成");

            // 步驟 5：建立 Harmony 實例並自動套用所有標記的補丁
            var harmony = new HarmonyLib.Harmony("net.johnytoxic.demandmodifier");
            harmony.PatchAll();
            
            log.Info("Harmony 補丁已全部套用");
        }

        /// <summary>
        /// Mod 卸載時執行 - 清理資源
        /// </summary>
        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            
            // 儲存並清理設定
            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
