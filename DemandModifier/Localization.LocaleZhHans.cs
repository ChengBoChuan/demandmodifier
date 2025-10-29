using System;
using System.Collections.Generic;
using Colossal;

namespace DemandModifier
{
    public partial class Localization
    {
        public class LocaleZhHans : IDictionarySource
        {
            private readonly DemandModifierSettings _settings;
            private Dictionary<string, string> _translations;

            public LocaleZhHans(DemandModifierSettings settings)
            {
                _settings = settings;
                LocaleSources["zh-HANS"] = new Tuple<string, string, IDictionarySource>(
                    "简体中文", "100", this
                );
                _translations = Load();
            }

            public IEnumerable<KeyValuePair<string, string>> ReadEntries(
                IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
            {
                return _translations;
            }

            public Dictionary<string, string> Load(bool dumpTranslations = false)
            {
                var translations = new Dictionary<string, string>
                {
                    { _settings.GetSettingsLocaleID(), "需求修改器" },
                    { _settings.GetOptionTabLocaleID("DemandControl"), "需求控制" },
                    { _settings.GetOptionTabLocaleID("ServiceControl"), "服务控制" },
                    { _settings.GetOptionTabLocaleID("EconomyControl"), "经济控制" },
                    { _settings.GetOptionGroupLocaleID("ResidentialDemand"), "住宅需求" },
                    { _settings.GetOptionGroupLocaleID("CommercialDemand"), "商业需求" },
                    { _settings.GetOptionGroupLocaleID("IndustrialDemand"), "工业需求" },
                    { _settings.GetOptionGroupLocaleID("ServiceSettings"), "服务设置" },
                    { _settings.GetOptionGroupLocaleID("EconomySettings"), "经济设置" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "住宅需求等级" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "选择住宅区域的需求等级" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "商业需求等级" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "选择商业区域的需求等级" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "工业需求等级" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "选择工业区域的需求等级" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "无限电力" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "永不缺电 - 所有建筑始终有电力供应" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "无限清水" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "永不缺水 - 所有建筑始终有清洁水供应" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "无限污水处理" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "污水系统永不爆满" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "无限垃圾处理" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "垃圾系统永不爆满" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "无限医疗" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "所有建筑始终有医疗覆盖" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "无限教育" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "所有建筑始终有教育覆盖" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "无限警力" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "所有建筑始终有警察保护" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "无限消防" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "所有建筑始终有消防覆盖" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "无限金钱" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "城市预算永不耗尽" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "免费建造" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "所有建筑和基础设施的建设成本为零" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "零维护成本" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "所有建筑和基础设施没有维护费用" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]", "关闭 (游戏预设)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]", "低 (25%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Medium]", "中 (50%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.High]", "高 (75%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]", "最大 (100%)" },
                };
                return translations;
            }

            public void Unload() { }
            public override string ToString() => "DemandModifier.Locale.zh-HANS";
        }
    }
}
