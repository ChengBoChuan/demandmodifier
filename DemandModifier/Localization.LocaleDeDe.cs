using System;
using System.Collections.Generic;
using Colossal;

namespace DemandModifier
{
    public partial class Localization
    {
        public class LocaleDeDe : IDictionarySource
        {
            private readonly DemandModifierSettings _settings;
            private Dictionary<string, string> _translations;

            public LocaleDeDe(DemandModifierSettings settings)
            {
                _settings = settings;
                LocaleSources["de-DE"] = new Tuple<string, string, IDictionarySource>(
                    "Deutsch", "100", this
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
                    { _settings.GetSettingsLocaleID(), "Nachfrage-Modifikator" },
                    { _settings.GetOptionTabLocaleID("DemandControl"), "Nachfragekontrolle" },
                    { _settings.GetOptionTabLocaleID("ServiceControl"), "Servicekontrolle" },
                    { _settings.GetOptionTabLocaleID("EconomyControl"), "Wirtschaftskontrolle" },
                    { _settings.GetOptionGroupLocaleID("ResidentialDemand"), "Wohnbedarf" },
                    { _settings.GetOptionGroupLocaleID("CommercialDemand"), "Gewerbebedarf" },
                    { _settings.GetOptionGroupLocaleID("IndustrialDemand"), "Industriebedarf" },
                    { _settings.GetOptionGroupLocaleID("ServiceSettings"), "Serviceeinstellungen" },
                    { _settings.GetOptionGroupLocaleID("EconomySettings"), "Wirtschaftseinstellungen" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "Wohnbedarfsebene" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "Wählen Sie die Nachfrageebene für Wohnzonen" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "Gewerbebedarfsebene" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "Wählen Sie die Nachfrageebene für Gewerbezonen" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "Industriebedarfsebene" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "Wählen Sie die Nachfrageebene für Industriezonen" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "Unbegrenzte Elektrizität" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "Alle Gebäude erhalten immer Strom" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "Unbegrenztes Wasser" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "Alle Gebäude erhalten immer Wasser" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "Unbegrenzter Abwasserbetrieb" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "Abwassersysteme laufen nie über" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "Unbegrenzte Müllwirtschaft" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "Müllsysteme laufen nie über" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "Unbegrenzte Gesundheitsversorgung" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "Alle Gebäude haben immer Gesundheitsversorgung" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "Unbegrenzte Bildung" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "Alle Gebäude haben immer Bildungsversorgung" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "Unbegrenzte Polizei" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "Alle Gebäude haben immer Polizeischutz" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "Unbegrenzte Feuerwehr" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "Alle Gebäude haben immer Feuerwehrschutz" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "Unbegrenztes Geld" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "Das Stadtbudget wird nie aufgebraucht" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "Freies Bauen" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "Alle Gebäude und Infrastrukturen kosten nichts zum Bauen" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "Keine Wartungskosten" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "Alle Gebäude und Infrastrukturen haben keine Wartungskosten" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]", "Aus (Spielstandard)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]", "Niedrig (25%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Medium]", "Mittel (50%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.High]", "Hoch (75%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]", "Maximum (100%)" },
                };
                return translations;
            }

            public void Unload() { }
            public override string ToString() => "DemandModifier.Locale.de-DE";
        }
    }
}
