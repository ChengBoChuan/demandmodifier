using System;
using System.Collections.Generic;
using Colossal;

namespace DemandModifier
{
    /// <summary>
    /// 英文語言定義 - 嚴格參考 Traffic 專案的 LocaleEN 實作
    /// 實作 IDictionarySource 介面，提供所有英文翻譯的字典
    /// 
    /// 關鍵：此類別在建構子中將自己註冊到 Localization.LocaleSources 字典
    /// 這樣 GetLanguageOptions() 才能讀取到英文選項
    /// </summary>
    public partial class Localization
    {
        public class LocaleEN : IDictionarySource
        {
            private readonly DemandModifierSettings _settings;
            private Dictionary<string, string> _translations;

            /// <summary>
            /// 建構子 - 將此英文語言源加入 LocaleSources 字典
            /// 供其他模組（如設定 UI）讀取可用的語言清單
            /// </summary>
            public LocaleEN(DemandModifierSettings settings)
            {
                _settings = settings;

                // 關鍵：自動註冊此語言源到靜態字典
                // 格式：LocaleSources[語言代碼] = (顯示名稱, 翻譯進度%, 此實例)
                LocaleSources["en-US"] = new Tuple<string, string, IDictionarySource>(
                    "English",  // 顯示在語言下拉選單中的名稱
                    "100",      // 翻譯進度百分比
                    this        // 此類別提供翻譯字典
                );

                _translations = Load();
            }

            /// <summary>
            /// 讀取翻譯字典條目 - 遊戲引擎會呼叫此方法取得翻譯
            /// </summary>
            public IEnumerable<KeyValuePair<string, string>> ReadEntries(
                IList<IDictionaryEntryError> errors, 
                Dictionary<string, int> indexCounts)
            {
                return _translations;
            }

            /// <summary>
            /// 建立並返回所有英文翻譯的字典
            /// 
            /// 關鍵要點：
            /// 1. 使用 _settings 的輔助方法產生標準化的鍵值格式
            ///    - GetSettingsLocaleID() = 模組顯示名稱
            ///    - GetOptionTabLocaleID("TabName") = 分頁標題
            ///    - GetOptionGroupLocaleID("GroupName") = 群組名稱
            ///    - GetOptionLabelLocaleID(nameof(Property)) = 選項標題
            ///    - GetOptionDescLocaleID(nameof(Property)) = 選項描述
            /// 
            /// 2. 這些方法自動產生正確的鍵值格式，無需手動拼接
            /// 
            /// 3. 使用 nameof() 確保屬性名稱一致性
            /// </summary>
            public Dictionary<string, string> Load(bool dumpTranslations = false)
            {
                var translations = new Dictionary<string, string>
                {
                    // ===== 模組識別 =====
                    // 模組名稱 - 遊戲會自動在模組列表中顯示此名稱
                    { _settings.GetSettingsLocaleID(), "Demand Modifier" },

                    // ===== 分頁標題 (Tab) =====
                    { _settings.GetOptionTabLocaleID("DemandControl"), "Demand Control" },
                    { _settings.GetOptionTabLocaleID("ServiceControl"), "Service Control" },
                    { _settings.GetOptionTabLocaleID("EconomyControl"), "Economy Control" },

                    // ===== 群組名稱 (Group) =====
                    { _settings.GetOptionGroupLocaleID("ResidentialDemand"), "Residential Demand" },
                    { _settings.GetOptionGroupLocaleID("CommercialDemand"), "Commercial Demand" },
                    { _settings.GetOptionGroupLocaleID("IndustrialDemand"), "Industrial Demand" },
                    { _settings.GetOptionGroupLocaleID("ServiceSettings"), "Service Settings" },
                    { _settings.GetOptionGroupLocaleID("EconomySettings"), "Economy Settings" },

                    // ===== 需求控制 - 住宅 =====
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), 
                        "Residential Demand Level" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.ResidentialDemandLevel)), 
                        "Select the residential zone demand level" },

                    // ===== 需求控制 - 商業 =====
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), 
                        "Commercial Demand Level" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.CommercialDemandLevel)), 
                        "Select the commercial zone demand level" },

                    // ===== 需求控制 - 工業 =====
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), 
                        "Industrial Demand Level" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.IndustrialDemandLevel)), 
                        "Select the industrial zone demand level" },

                    // ===== 服務控制 =====
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), 
                        "Unlimited Electricity" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedElectricity)), 
                        "Never run out of electricity - all buildings receive power" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), 
                        "Unlimited Water" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedWater)), 
                        "Never run out of water - all buildings receive water" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), 
                        "Unlimited Sewage" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedSewage)), 
                        "Never overflow sewage systems" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), 
                        "Unlimited Garbage" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedGarbage)), 
                        "Never run out of garbage disposal" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), 
                        "Unlimited Healthcare" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedHealthcare)), 
                        "All buildings always have healthcare access" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), 
                        "Unlimited Education" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedEducation)), 
                        "All buildings always have education access" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), 
                        "Unlimited Police" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedPolice)), 
                        "All buildings always have police protection" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), 
                        "Unlimited Fire Safety" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedFire)), 
                        "All buildings always have fire safety coverage" },

                    // ===== 經濟控制 =====
                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), 
                        "Unlimited Money" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableUnlimitedMoney)), 
                        "Never run out of money" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), 
                        "Free Construction" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableFreeConstruction)), 
                        "All buildings and infrastructure cost nothing to construct" },

                    { _settings.GetOptionLabelLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), 
                        "No Upkeep Cost" },
                    { _settings.GetOptionDescLocaleID(nameof(DemandModifierSettings.EnableNoUpkeep)), 
                        "All buildings and infrastructure have no maintenance costs" },

                    // ===== 列舉值翻譯 (Enum Values) =====
                    // 這些值由 GetDemandLevelOptions() 在下拉選單中使用
                    // 格式：Common.ENUM[<CommandNamespace>.<EnumTypeName>.<EnumValueName>]
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Off]", 
                        "Off (Game Default)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Low]", 
                        "Low (25%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Medium]", 
                        "Medium (50%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.High]", 
                        "High (75%)" },
                    { "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]", 
                        "Maximum (100%)" },
                };

                return translations;
            }

            /// <summary>
            /// 卸載此語言源 - 清理資源
            /// </summary>
            public void Unload() { }

            /// <summary>
            /// 返回此語言源的識別字串
            /// </summary>
            public override string ToString()
            {
                return "DemandModifier.Locale.en-US";
            }
        }
    }
}
