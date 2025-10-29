using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using System;
using System.Collections.Generic;

namespace DemandModifier
{
    /// <summary>
    /// 需求等級列舉 - 定義5級需求強度
    /// 值對應到遊戲內部的需求數值 (0-255)
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
    /// DemandModifier 模組設定類別
    /// 使用 ModSetting 基類提供的語系支援
    /// 參考 Traffic 專案實作模式
    /// </summary>
    [FileLocation(nameof(DemandModifier))]
    [SettingsUITabOrder("DemandControl", "ServiceControl", "EconomyControl")]
    [SettingsUIGroupOrder(
        "ResidentialDemand", "CommercialDemand", "IndustrialDemand", 
        "ServiceSettings", "EconomySettings")]
    [SettingsUIShowGroupName(
        "ResidentialDemand", "CommercialDemand", "IndustrialDemand", 
        "ServiceSettings", "EconomySettings")]
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
            // ===== 需求控制 - 預設設為最大需求 =====
            ResidentialDemandLevel = DemandLevel.Maximum;
            CommercialDemandLevel = DemandLevel.Maximum;
            IndustrialDemandLevel = DemandLevel.Maximum;

            // ===== 服務控制 - 預設全部停用 =====
            // (避免遊戲過於簡單，讓玩家自行選擇啟用)
            EnableUnlimitedElectricity = false;
            EnableUnlimitedWater = false;
            EnableUnlimitedSewage = false;
            EnableUnlimitedGarbage = false;
            EnableUnlimitedHealthcare = false;
            EnableUnlimitedEducation = false;
            EnableUnlimitedPolice = false;
            EnableUnlimitedFire = false;

            // ===== 經濟控制 - 預設全部停用 =====
            EnableUnlimitedMoney = false;
            EnableFreeConstruction = false;
            EnableNoUpkeep = false;
        }

        // ==================== 需求控制分頁 ====================

        /// <summary>
        /// 住宅需求等級 - 使用下拉選單
        /// </summary>
        [SettingsUISection("DemandControl", "ResidentialDemand")]
        [SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
        public DemandLevel ResidentialDemandLevel { get; set; }

        /// <summary>
        /// 商業需求等級 - 使用下拉選單
        /// </summary>
        [SettingsUISection("DemandControl", "CommercialDemand")]
        [SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
        public DemandLevel CommercialDemandLevel { get; set; }

        /// <summary>
        /// 工業需求等級 - 使用下拉選單
        /// </summary>
        [SettingsUISection("DemandControl", "IndustrialDemand")]
        [SettingsUIDropdown(typeof(DemandModifierSettings), nameof(GetDemandLevelOptions))]
        public DemandLevel IndustrialDemandLevel { get; set; }

        /// <summary>
        /// 提供需求等級下拉選單的選項
        /// 返回已翻譯的下拉選單項目
        /// </summary>
        private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
        {
            var items = new List<DropdownItem<DemandLevel>>();
            
            // 定義需求等級的順序
            var levels = new DemandLevel[] 
            { 
                DemandLevel.Off, 
                DemandLevel.Low, 
                DemandLevel.Medium, 
                DemandLevel.High, 
                DemandLevel.Maximum 
            };
            
            foreach (var level in levels)
            {
                // 構建語系鍵值
                string localeKey = string.Format("Common.ENUM[DemandModifier.DemandModifier.DemandLevel.{0}]", level.ToString());
                
                // 嘗試從當前活躍語言字典中讀取翻譯
                string displayName = GetLocalizedEnumName(level);
                
                items.Add(new DropdownItem<DemandLevel>
                {
                    value = level,
                    displayName = displayName
                });
            }
            
            return items.ToArray();
        }

        /// <summary>
        /// 取得 Enum 值的本地化名稱
        /// 使用遊戲內建的語言系統
        /// </summary>
        private string GetLocalizedEnumName(DemandLevel level)
        {
            try
            {
                string localeKey = string.Format("Common.ENUM[DemandModifier.DemandModifier.DemandLevel.{0}]", level.ToString());
                
                if (Game.SceneFlow.GameManager.instance != null && 
                    Game.SceneFlow.GameManager.instance.localizationManager != null)
                {
                    var dict = Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary;
                    if (dict != null && dict.TryGetValue(localeKey, out string translated))
                    {
                        return translated;
                    }
                }
            }
            catch
            {
                // 若讀取失敗，使用預設值
            }
            
            // 降級機制：返回英文預設名稱
            switch (level)
            {
                case DemandLevel.Off:
                    return "Off (Game Default)";
                case DemandLevel.Low:
                    return "Low (25%)";
                case DemandLevel.Medium:
                    return "Medium (50%)";
                case DemandLevel.High:
                    return "High (75%)";
                case DemandLevel.Maximum:
                    return "Maximum (100%)";
                default:
                    return level.ToString();
            }
        }

        // ==================== 服務控制分頁 ====================

        /// <summary>
        /// 無限電力 - 所有建築始終有電力供應
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedElectricity { get; set; }

        /// <summary>
        /// 無限水 - 所有建築始終有清潔水供應
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedWater { get; set; }

        /// <summary>
        /// 無限污水 - 污水系統永不滿溢
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedSewage { get; set; }

        /// <summary>
        /// 無限垃圾 - 垃圾處理永不爆滿
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedGarbage { get; set; }

        /// <summary>
        /// 無限醫療 - 所有建築始終有醫療覆蓋
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedHealthcare { get; set; }

        /// <summary>
        /// 無限教育 - 所有建築始終有教育覆蓋
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedEducation { get; set; }

        /// <summary>
        /// 無限警力 - 所有建築始終有警察保護
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedPolice { get; set; }

        /// <summary>
        /// 無限消防 - 所有建築始終有消防覆蓋
        /// </summary>
        [SettingsUISection("ServiceControl", "ServiceSettings")]
        public bool EnableUnlimitedFire { get; set; }

        // ==================== 經濟控制分頁 ====================

        /// <summary>
        /// 無限金錢 - 城市預算永不耗盡
        /// </summary>
        [SettingsUISection("EconomyControl", "EconomySettings")]
        public bool EnableUnlimitedMoney { get; set; }

        /// <summary>
        /// 免費建造 - 所有建築和基礎設施的建設成本為零
        /// </summary>
        [SettingsUISection("EconomyControl", "EconomySettings")]
        public bool EnableFreeConstruction { get; set; }

        /// <summary>
        /// 零維護成本 - 所有建築和基礎設施沒有維護費用
        /// </summary>
        [SettingsUISection("EconomyControl", "EconomySettings")]
        public bool EnableNoUpkeep { get; set; }
    }
}
