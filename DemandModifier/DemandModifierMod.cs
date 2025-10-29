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
        /// </summary>
        /// <param name="updateSystem">遊戲更新系統</param>
        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            // 記錄 Mod 資產路徑
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            // 建立並註冊模組設定
            Settings = new DemandModifierSettings(this);
            Settings.RegisterInOptionsUI();
            
            // 從全域資料庫載入設定
            AssetDatabase.global.LoadSettings(nameof(DemandModifier), Settings, new DemandModifierSettings(this));
            
            log.Info($"設定已載入 - 住宅需求: {Settings.ResidentialDemandLevel}, 商業需求: {Settings.CommercialDemandLevel}, 工業需求: {Settings.IndustrialDemandLevel}");
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
