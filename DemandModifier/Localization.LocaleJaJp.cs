using System;
using System.Collections.Generic;
using Colossal;

namespace DemandModifier
{
    public partial class Localization
    {
        public class LocaleJaJp : IDictionarySource
        {
            private readonly DemandModifierSettings _settings;
            private Dictionary<string, string> _translations;

            public LocaleJaJp(DemandModifierSettings settings)
            {
                _settings = settings;
                LocaleSources["ja-JP"] = new Tuple<string, string, IDictionarySource>(
                    "日本語", "100", this
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
                    { _settings.GetSettingsLocaleID(), "需要モディファイア" },
                    { _settings.GetOptionTabLocaleID("DemandControl"), "需要制御" },
                    { _settings.GetOptionTabLocaleID("ServiceControl"), "サービス制御" },
                    { _settings.GetOptionTabLocaleID("EconomyControl"), "経済制御" },
                    { _settings.GetOptionGroupLocaleID("ResidentialDemand"), "住宅需要" },
                    { _settings.GetOptionGroupLocaleID("CommercialDemand"), "商業需要" },
                    { _settings.GetOptionGroupLocaleID("IndustrialDemand"), "工業需要" },
                    { _settings.GetOptionGroupLocaleID("ServiceSettings"), "サービス設定" },
                    { _settings.GetOptionGroupLocaleID("EconomySettings"), "経済設定" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "住宅需要レベル" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "住宅ゾーンの需要レベルを選択" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "商業需要レベル" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "商業ゾーンの需要レベルを選択" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "工業需要レベル" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "工業ゾーンの需要レベルを選択" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "無制限電力" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "すべての建物が常に電力を供給" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "無制限水道" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "すべての建物が常にきれいな水を供給" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "無制限下水道" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "下水道システムが常に満杯にならない" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "無制限ごみ処理" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "ごみシステムが常に満杯にならない" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "無制限医療" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "すべての建物が常に医療サービスをカバー" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "無制限教育" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "すべての建物が常に教育サービスをカバー" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "無制限警察" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "すべての建物が常に警察保護を受ける" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "無制限消防" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "すべての建物が常に消防カバレッジを受ける" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "無制限資金" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "都市予算が常に底を尽きない" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "無料建設" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "すべての建物とインフラの建設費用がゼロ" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "メンテナンスなし" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "すべての建物とインフラにメンテナンス費用がない" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]", "オフ (ゲーム標準)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]", "低 (25%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Medium]", "中 (50%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.High]", "高 (75%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]", "最大 (100%)" },
                };
                return translations;
            }

            public void Unload() { }
            public override string ToString() => "DemandModifier.Locale.ja-JP";
        }
    }
}
