using System;
using System.Collections.Generic;
using Colossal;

namespace DemandModifier
{
    public partial class Localization
    {
        public class LocaleEsEs : IDictionarySource
        {
            private readonly DemandModifierSettings _settings;
            private Dictionary<string, string> _translations;

            public LocaleEsEs(DemandModifierSettings settings)
            {
                _settings = settings;
                LocaleSources["es-ES"] = new Tuple<string, string, IDictionarySource>(
                    "Español", "100", this
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
                    { _settings.GetSettingsLocaleID(), "Modificador de Demanda" },
                    { _settings.GetOptionTabLocaleID("DemandControl"), "Control de Demanda" },
                    { _settings.GetOptionTabLocaleID("ServiceControl"), "Control de Servicios" },
                    { _settings.GetOptionTabLocaleID("EconomyControl"), "Control Económico" },
                    { _settings.GetOptionGroupLocaleID("ResidentialDemand"), "Demanda Residencial" },
                    { _settings.GetOptionGroupLocaleID("CommercialDemand"), "Demanda Comercial" },
                    { _settings.GetOptionGroupLocaleID("IndustrialDemand"), "Demanda Industrial" },
                    { _settings.GetOptionGroupLocaleID("ServiceSettings"), "Configuración de Servicios" },
                    { _settings.GetOptionGroupLocaleID("EconomySettings"), "Configuración Económica" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "Nivel de Demanda Residencial" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), "Selecciona el nivel de demanda de zonas residenciales" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "Nivel de Demanda Comercial" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), "Selecciona el nivel de demanda de zonas comerciales" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "Nivel de Demanda Industrial" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), "Selecciona el nivel de demanda de zonas industriales" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "Electricidad Ilimitada" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), "Todos los edificios siempre reciben energía" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "Agua Ilimitada" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), "Todos los edificios siempre reciben agua" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "Alcantarillado Ilimitado" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), "Los sistemas de alcantarillado nunca se desbordan" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "Recolección de Basura Ilimitada" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), "Los sistemas de basura nunca se desbordan" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "Asistencia Médica Ilimitada" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), "Todos los edificios siempre tienen cobertura médica" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "Educación Ilimitada" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), "Todos los edificios siempre tienen cobertura educativa" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "Policía Ilimitada" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), "Todos los edificios siempre tienen protección policial" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "Bomberos Ilimitados" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), "Todos los edificios siempre tienen cobertura de bomberos" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "Dinero Ilimitado" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), "El presupuesto de la ciudad nunca se agota" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "Construcción Gratuita" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), "Todos los edificios e infraestructuras se construyen sin costo" },
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "Sin Costos de Mantenimiento" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), "Todos los edificios e infraestructuras no tienen costos de mantenimiento" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]", "Desactivado (Predeterminado del Juego)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]", "Bajo (25%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Medium]", "Medio (50%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.High]", "Alto (75%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]", "Máximo (100%)" },
                };
                return translations;
            }

            public void Unload() { }
            public override string ToString() => "DemandModifier.Locale.es-ES";
        }
    }
}
