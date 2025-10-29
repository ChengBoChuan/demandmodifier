using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;

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
        /// 這個方法由 SettingsUIDropdown attribute 呼叫
        /// 返回的 displayName 必須是**已翻譯的字串**，而不是語系鍵值
        /// 
        /// 此實作參考 Traffic 專案的 GetLanguageOptions() 方法，
        /// 從 Localization.LocaleSources 字典中讀取翻譯文字
        /// </summary>
        private DropdownItem<DemandLevel>[] GetDemandLevelOptions()
        {
            return new DropdownItem<DemandLevel>[]
            {
                new DropdownItem<DemandLevel>
                {
                    value = DemandLevel.Off,
                    displayName = GetLocalizedDemandLevelName(DemandLevel.Off)
                },
                new DropdownItem<DemandLevel>
                {
                    value = DemandLevel.Low,
                    displayName = GetLocalizedDemandLevelName(DemandLevel.Low)
                },
                new DropdownItem<DemandLevel>
                {
                    value = DemandLevel.Medium,
                    displayName = GetLocalizedDemandLevelName(DemandLevel.Medium)
                },
                new DropdownItem<DemandLevel>
                {
                    value = DemandLevel.High,
                    displayName = GetLocalizedDemandLevelName(DemandLevel.High)
                },
                new DropdownItem<DemandLevel>
                {
                    value = DemandLevel.Maximum,
                    displayName = GetLocalizedDemandLevelName(DemandLevel.Maximum)
                }
            };
        }

        /// <summary>
        /// 從 LocaleSources 字典中讀取翻譯的需求等級名稱
        /// 採用 Traffic 專案的正確模式 - 從 LocaleSources 而不是 localizationManager
        /// 
        /// LocaleSources 由各個 LocaleXX 類別在其建構子中填入
        /// 這確保在設定 UI 初始化時，翻譯已經可用
        /// </summary>
        private string GetLocalizedDemandLevelName(DemandLevel level)
        {
            // 構建語系鍵值 - 格式：Common.ENUM[完整命名空間.枚舉型別.值]
            string localeKey = $"Common.ENUM[DemandModifier.DemandModifier.DemandLevel.{level}]";

            try
            {
                // 從 LocaleSources[\"en-US\"] 的英文語言源讀取翻譯
                if (Localization.LocaleSources.TryGetValue("en-US", out var localeSource))
                {
                    // localeSource = (\"English\", \"100\", IDictionarySource實例)
                    // 第 3 個元素是 IDictionarySource，包含翻譯字典
                    var dictionarySource = localeSource.Item3;
                    if (dictionarySource != null)
                    {
                        // 獲取 IDictionarySource 的翻譯字典
                        var entries = dictionarySource.ReadEntries(null, null);
                        if (entries != null)
                        {
                            foreach (var entry in entries)
                            {
                                if (entry.Key == localeKey)
                                {
                                    return entry.Value;  // 返回翻譯文字
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // 若讀取失敗，使用降級方案
            }

            // 降級機制：返回英文預設名稱
            // 確保即使語系檔案遺失或載入失敗，仍能顯示有意義的文字
            return level switch
            {
                DemandLevel.Off => "Off (Game Default)",
                DemandLevel.Low => "Low (25%)",
                DemandLevel.Medium => "Medium (50%)",
                DemandLevel.High => "High (75%)",
                DemandLevel.Maximum => "Maximum (100%)",
                _ => level.ToString()
            };
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
