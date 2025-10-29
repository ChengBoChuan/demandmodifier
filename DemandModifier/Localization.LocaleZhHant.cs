using System;
using System.Collections.Generic;
using Colossal;

namespace DemandModifier
{
    /// <summary>
    /// 繁體中文 (台灣) 語言定義
    /// 嚴格參考 Traffic 專案的 Locale 實作
    /// </summary>
    public partial class Localization
    {
        public class LocaleZhHant : IDictionarySource
        {
            private readonly DemandModifierSettings _settings;
            private Dictionary<string, string> _translations;

            public LocaleZhHant(DemandModifierSettings settings)
            {
                _settings = settings;

                // 自動註冊此語言源到靜態字典
                LocaleSources["zh-HANT"] = new Tuple<string, string, IDictionarySource>(
                    "繁體中文",
                    "100",
                    this
                );

                _translations = Load();
            }

            public IEnumerable<KeyValuePair<string, string>> ReadEntries(
                IList<IDictionaryEntryError> errors, 
                Dictionary<string, int> indexCounts)
            {
                return _translations;
            }

            public Dictionary<string, string> Load(bool dumpTranslations = false)
            {
                var translations = new Dictionary<string, string>
                {
                    { _settings.GetSettingsLocaleID(), "需求修改器" },

                    { _settings.GetOptionTabLocaleID("DemandControl"), "需求控制" },
                    { _settings.GetOptionTabLocaleID("ServiceControl"), "服務控制" },
                    { _settings.GetOptionTabLocaleID("EconomyControl"), "經濟控制" },

                    { _settings.GetOptionGroupLocaleID("ResidentialDemand"), "住宅需求" },
                    { _settings.GetOptionGroupLocaleID("CommercialDemand"), "商業需求" },
                    { _settings.GetOptionGroupLocaleID("IndustrialDemand"), "工業需求" },
                    { _settings.GetOptionGroupLocaleID("ServiceSettings"), "服務設定" },
                    { _settings.GetOptionGroupLocaleID("EconomySettings"), "經濟設定" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), 
                        "住宅需求等級" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), 
                        "選擇住宅區域的需求等級" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), 
                        "商業需求等級" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), 
                        "選擇商業區域的需求等級" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), 
                        "工業需求等級" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), 
                        "選擇工業區域的需求等級" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), 
                        "無限電力" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), 
                        "永不缺電 - 所有建築始終有電力供應" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), 
                        "無限清水" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), 
                        "永不缺水 - 所有建築始終有清潔水供應" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), 
                        "無限污水處理" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), 
                        "污水系統永不爆滿" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), 
                        "無限垃圾處理" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), 
                        "垃圾系統永不爆滿" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), 
                        "無限醫療" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), 
                        "所有建築始終有醫療覆蓋" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), 
                        "無限教育" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), 
                        "所有建築始終有教育覆蓋" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), 
                        "無限警力" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), 
                        "所有建築始終有警察保護" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), 
                        "無限消防" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), 
                        "所有建築始終有消防覆蓋" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), 
                        "無限金錢" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), 
                        "城市預算永不耗盡" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), 
                        "免費建造" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), 
                        "所有建築和基礎設施的建設成本為零" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), 
                        "零維護成本" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), 
                        "所有建築和基礎設施沒有維護費用" },

                    // Enum 值翻譯
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]", 
                        "關閉 (遊戲預設)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]", 
                        "低 (25%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Medium]", 
                        "中 (50%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.High]", 
                        "高 (75%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]", 
                        "最大 (100%)" },
                };

                return translations;
            }

            public void Unload() { }

            public override string ToString()
            {
                return "DemandModifier.Locale.zh-HANT";
            }
        }
    }
}
