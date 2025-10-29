using System;
using System.Collections.Generic;
using Colossal;

namespace DemandModifier
{
    public partial class Localization
    {
        public class LocaleFrFr : IDictionarySource
        {
            private readonly DemandModifierSettings _settings;
            private Dictionary<string, string> _translations;

            public LocaleFrFr(DemandModifierSettings settings)
            {
                _settings = settings;
                LocaleSources["fr-FR"] = new Tuple<string, string, IDictionarySource>(
                    "Français", "100", this
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
                    { _settings.GetSettingsLocaleID(), "Modificateur de Demande" },
                    { _settings.GetOptionTabLocaleID("DemandControl"), "Contrôle de la Demande" },
                    { _settings.GetOptionTabLocaleID("ServiceControl"), "Contrôle des Services" },
                    { _settings.GetOptionTabLocaleID("EconomyControl"), "Contrôle Économique" },
                    { _settings.GetOptionGroupLocaleID("ResidentialDemand"), "Demande Résidentielle" },
                    { _settings.GetOptionGroupLocaleID("CommercialDemand"), "Demande Commerciale" },
                    { _settings.GetOptionGroupLocaleID("IndustrialDemand"), "Demande Industrielle" },
                    { _settings.GetOptionGroupLocaleID("ServiceSettings"), "Paramètres de Service" },
                    { _settings.GetOptionGroupLocaleID("EconomySettings"), "Paramètres Économiques" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "Niveau de Demande Résidentielle" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "Sélectionnez le niveau de demande des zones résidentielles" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "Niveau de Demande Commerciale" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "Sélectionnez le niveau de demande des zones commerciales" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "Niveau de Demande Industrielle" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "Sélectionnez le niveau de demande des zones industrielles" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "Électricité Illimitée" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "Tous les bâtiments reçoivent toujours de l'électricité" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "Eau Illimitée" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "Tous les bâtiments reçoivent toujours de l'eau" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "Égout Illimité" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "Les systèmes d'égout ne débordent jamais" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "Collecte des Ordures Illimitée" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "Les systèmes de déchets ne débordent jamais" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "Santé Illimitée" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "Tous les bâtiments ont toujours une couverture médicale" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "Éducation Illimitée" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "Tous les bâtiments ont toujours une couverture éducative" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "Police Illimitée" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "Tous les bâtiments ont toujours une protection policière" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "Pompiers Illimités" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "Tous les bâtiments ont toujours une couverture des pompiers" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "Argent Illimité" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "Le budget de la ville n'est jamais épuisé" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "Construction Gratuite" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "Tous les bâtiments et infrastructures coûtent zéro à construire" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "Aucun Coût d'Entretien" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "Tous les bâtiments et infrastructures n'ont pas de coûts d'entretien" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]", "Désactivé (Par Défaut du Jeu)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]", "Bas (25%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Medium]", "Moyen (50%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.High]", "Haut (75%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]", "Maximum (100%)" },
                };
                return translations;
            }

            public void Unload() { }
            public override string ToString() => "DemandModifier.Locale.fr-FR";
        }
    }
}
