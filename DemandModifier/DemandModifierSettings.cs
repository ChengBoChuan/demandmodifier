using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;

namespace DemandModifier
{
    /// <summary>
    /// 需求等級枚舉 - 定義需求強度選項
    /// </summary>
    public enum DemandLevel
    {
        Off = 0,           // 關閉(使用遊戲預設)
        Low = 64,          // 低需求(25%)
        Medium = 128,      // 中需求(50%)
        High = 192,        // 高需求(75%)
        Maximum = 255      // 最大需求(100%)
    }

    /// <summary>
    /// Demand Modifier 模組設定類別 - 提供需求控制、服務控制和經濟控制的開關
    /// </summary>
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

        /// <summary>
        /// 設定所有選項的預設值
        /// </summary>
        public override void SetDefaults()
        {
            // 需求系統 - 預設使用最大值
            ResidentialDemandLevel = DemandLevel.Maximum;
            CommercialDemandLevel = DemandLevel.Maximum;
            IndustrialDemandLevel = DemandLevel.Maximum;
            
            // 服務系統 - 預設全部停用(避免過度簡化遊戲)
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
        [SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
        public DemandLevel ResidentialDemandLevel { get; set; }

        [SettingsUISection("DemandControl", "CommercialDemand")]
        [SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
        public DemandLevel CommercialDemandLevel { get; set; }

        [SettingsUISection("DemandControl", "IndustrialDemand")]
        [SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
        public DemandLevel IndustrialDemandLevel { get; set; }

        /// <summary>
        /// 提供需求等級下拉選單的選項（支援多國語系）
        /// </summary>
        /// <returns>下拉選單項目陣列</returns>
        private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
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

        /// <summary>
        /// 從語系檔案讀取 enum 值的翻譯，失敗時使用預設英文名稱
        /// </summary>
        /// <param name="level">需求等級</param>
        /// <returns>翻譯後的顯示名稱</returns>
        private string GetLocalizedEnumName(DemandLevel level)
        {
            string localeKey = string.Format("Common.ENUM[DemandModifier.DemandLevel.{0}]", level.ToString());
            
            try
            {
                // 嘗試從遊戲的語系管理器獲取翻譯
                if (Game.SceneFlow.GameManager.instance != null && 
                    Game.SceneFlow.GameManager.instance.localizationManager != null)
                {
                    var dictionary = Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary;
                    if (dictionary != null && dictionary.TryGetValue(localeKey, out string translated))
                    {
                        return translated;
                    }
                }
            }
            catch
            {
                // 忽略任何錯誤，使用降級方案
            }
            
            // 降級：根據 enum 值提供預設英文名稱（帶百分比說明）
            switch (level)
            {
                case DemandLevel.Off: return "Off (Game Default)";
                case DemandLevel.Low: return "Low (25%)";
                case DemandLevel.Medium: return "Medium (50%)";
                case DemandLevel.High: return "High (75%)";
                case DemandLevel.Maximum: return "Maximum (100%)";
                default: return level.ToString();
            }
        }

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
