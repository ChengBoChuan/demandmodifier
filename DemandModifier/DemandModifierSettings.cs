using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

namespace DemandModifier
{
    /// &lt;summary&gt;
    /// Demand Modifier 模組設定類別 - 提供需求控制、服務控制和經濟控制的開關
    /// &lt;/summary&gt;
    [FileLocation(nameof(DemandModifier))]
    [SettingsUITabOrder("DemandControl", "ServiceControl", "EconomyControl")]
    [SettingsUIGroupOrder("ResidentialDemand", "CommercialDemand", "IndustrialDemand", "ServiceSettings", "EconomySettings")]
    [SettingsUIShowGroupName("ResidentialDemand", "CommercialDemand", "IndustrialDemand", "ServiceSettings", "EconomySettings")]
    public class DemandModifierSettings : ModSetting
    {
        public DemandModifierSettings(IMod mod) : base(mod)
        {
            SetDefaults();
        }

        /// &lt;summary&gt;
        /// 設定所有選項的預設值
        /// &lt;/summary&gt;
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

        [SettingsUISection("DemandControl", "ResidentialDemand")]
        public bool EnableResidentialDemand { get; set; }

        [SettingsUISection("DemandControl", "CommercialDemand")]
        public bool EnableCommercialDemand { get; set; }

        [SettingsUISection("DemandControl", "IndustrialDemand")]
        public bool EnableIndustrialDemand { get; set; }

        // ==================== 服務控制分頁 ====================

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedElectricity { get; set; }

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedWater { get; set; }

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedSewage { get; set; }

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedGarbage { get; set; }

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedHealthcare { get; set; }

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedEducation { get; set; }

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedPolice { get; set; }

        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedFire { get; set; }

        // ==================== 經濟控制分頁 ====================

        [SettingsUISection("EconomyControl", "EconomySettings")]
        public bool EnableUnlimitedMoney { get; set; }

        [SettingsUISection("EconomyControl", "EconomySettings")]
        public bool EnableFreeConstruction { get; set; }

        [SettingsUISection("EconomyControl", "EconomySettings")]
        public bool EnableNoUpkeep { get; set; }
    }
}
