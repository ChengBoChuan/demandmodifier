using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

namespace DemandModifier
{
    /// <summary>
    /// Demand Modifier 模組設定類別 - 提供需求控制、服務控制和經濟控制的開關
    /// </summary>
    [FileLocation(nameof(DemandModifier))]
    [SettingsUITabOrder("需求控制", "服務控制", "經濟控制")]
    [SettingsUIGroupOrder("住宅需求", "商業需求", "工業需求", "服務設定", "資源設定", "經濟設定")]
    [SettingsUIShowGroupName("住宅需求", "商業需求", "工業需求", "服務設定", "資源設定", "經濟設定")]
    public class DemandModifierSettings : ModSetting
    {
        public DemandModifierSettings(IMod mod) : base(mod)
        {
            SetDefaults();
        }

        /// <summary>
        /// 設定所有選項的預設值
        /// </summary>
        public override void SetDefaults()
        {
            // 需求系統 - 預設全部啟用
            EnableResidentialDemand = true;
            EnableCommercialDemand = true;
            EnableIndustrialDemand = true;
            
            // 服務系統 - 預設全部停用（避免過度簡化遊戲）
            EnableUnlimitedElectricity = false;
            EnableUnlimitedWater = false;
            EnableUnlimitedSewage = false;
            EnableUnlimitedGarbage = false;
            EnableUnlimitedHealthcare = false;
            EnableUnlimitedEducation = false;
            EnableUnlimitedPolice = false;
            EnableUnlimitedFire = false;
            
            // 經濟系統 - 預設全部停用
            EnableUnlimitedMoney = false;
            EnableFreeConstruction = false;
            EnableNoUpkeep = false;
        }

        // ==================== 需求控制分頁 ====================

        [SettingsUISection("需求控制", "住宅需求")]
        public bool EnableResidentialDemand { get; set; }

        [SettingsUISection("需求控制", "商業需求")]
        public bool EnableCommercialDemand { get; set; }

        [SettingsUISection("需求控制", "工業需求")]
        public bool EnableIndustrialDemand { get; set; }

        // ==================== 服務控制分頁 ====================

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedElectricity { get; set; }

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedWater { get; set; }

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedSewage { get; set; }

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedGarbage { get; set; }

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedHealthcare { get; set; }

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedEducation { get; set; }

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedPolice { get; set; }

        [SettingsUISection("服務控制", "服務設定")]
        public bool EnableUnlimitedFire { get; set; }

        // ==================== 經濟控制分頁 ====================

        [SettingsUISection("經濟控制", "經濟設定")]
        public bool EnableUnlimitedMoney { get; set; }

        [SettingsUISection("經濟控制", "經濟設定")]
        public bool EnableFreeConstruction { get; set; }

        [SettingsUISection("經濟控制", "經濟設定")]
        public bool EnableNoUpkeep { get; set; }
    }
}
