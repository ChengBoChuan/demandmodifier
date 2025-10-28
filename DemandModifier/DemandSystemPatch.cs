using Colossal.Collections;
using Game.Simulation;
using HarmonyLib;
using Unity.Collections;
using Unity.Mathematics;

namespace DemandModifier
{
    /// <summary>
    /// 商業需求系統補丁 - 攔截商業需求更新並設置為最大值
    /// </summary>
    [HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
    public class CommercialDemandSystemPatch
    {
        /// <summary>
        /// 取得商業建築需求私有欄位的參考
        /// </summary>
        private static AccessTools.FieldRef<CommercialDemandSystem, NativeValue<int>> BuildingDemandRef =
            AccessTools.FieldRefAccess<CommercialDemandSystem, NativeValue<int>>("m_BuildingDemand");

        /// <summary>
        /// Harmony 前綴補丁 - 在原始方法執行前將商業建築需求設為指定值
        /// </summary>
        /// <param name="__instance">CommercialDemandSystem 的實例</param>
        static void Prefix(CommercialDemandSystem __instance)
        {
            // 僅在設定存在且未設為 Off 時才修改需求值
            if (DemandModifierMod.Settings != null && DemandModifierMod.Settings.CommercialDemandLevel != DemandLevel.Off)
            {
                // 設定商業建築需求為使用者選擇的值
                BuildingDemandRef(__instance).value = (int)DemandModifierMod.Settings.CommercialDemandLevel;
            }
        }
    }

    /// <summary>
    /// 工業需求系統補丁 - 攔截工業需求更新並設置所有工業類型需求為最大值
    /// </summary>
    [HarmonyPatch(typeof(IndustrialDemandSystem), "OnUpdate")]
    public class IndustrialDemandSystemPatch
    {
        /// <summary>
        /// 取得工業建築需求私有欄位的參考
        /// </summary>
        private static AccessTools.FieldRef<IndustrialDemandSystem, NativeValue<int>> IndustrialBuildingDemandRef =
            AccessTools.FieldRefAccess<IndustrialDemandSystem, NativeValue<int>>("m_IndustrialBuildingDemand");
        
        /// <summary>
        /// 取得倉儲建築需求私有欄位的參考
        /// </summary>
        private static AccessTools.FieldRef<IndustrialDemandSystem, NativeValue<int>> StorageBuildingDemand =
            AccessTools.FieldRefAccess<IndustrialDemandSystem, NativeValue<int>>("m_StorageBuildingDemand");
        
        /// <summary>
        /// 取得辦公建築需求私有欄位的參考
        /// </summary>
        private static AccessTools.FieldRef<IndustrialDemandSystem, NativeValue<int>> OfficeBuildingDemand =
            AccessTools.FieldRefAccess<IndustrialDemandSystem, NativeValue<int>>("m_OfficeBuildingDemand");

        /// <summary>
        /// Harmony 前綴補丁 - 在原始方法執行前將所有工業相關需求設為指定值
        /// </summary>
        /// <param name="__instance">IndustrialDemandSystem 的實例</param>
        static void Prefix(IndustrialDemandSystem __instance)
        {
            // 僅在設定存在且未設為 Off 時才修改需求值
            if (DemandModifierMod.Settings != null && DemandModifierMod.Settings.IndustrialDemandLevel != DemandLevel.Off)
            {
                int demandValue = (int)DemandModifierMod.Settings.IndustrialDemandLevel;
                // 設定工業建築需求為使用者選擇的值
                IndustrialBuildingDemandRef(__instance).value = demandValue;
                // 設定倉儲建築需求為使用者選擇的值
                StorageBuildingDemand(__instance).value = demandValue;
                // 設定辦公建築需求為使用者選擇的值
                OfficeBuildingDemand(__instance).value = demandValue;
            }
        }
    }

    /// <summary>
    /// 住宅需求系統補丁 - 攔截住宅需求更新並設置所有密度等級的需求為最大值
    /// </summary>
    [HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
    public class ResidentialDemandSystemPatch
    {
        /// <summary>
        /// 取得住宅建築需求私有欄位的參考 (int3 包含低/中/高密度)
        /// </summary>
        private static AccessTools.FieldRef<ResidentialDemandSystem, NativeValue<int3>> BuildingDemandRef =
            AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeValue<int3>>("m_BuildingDemand");
        
        /// <summary>
        /// 取得家庭需求私有欄位的參考
        /// </summary>
        private static AccessTools.FieldRef<ResidentialDemandSystem, NativeValue<int>> HouseholdDemandRef =
            AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeValue<int>>("m_HouseholdDemand");

        /// <summary>
        /// 取得低密度住宅需求因素陣列的參考
        /// </summary>
        private static AccessTools.FieldRef<ResidentialDemandSystem, NativeArray<int>> LowDemandFactorsRef =
            AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeArray<int>>("m_LowDemandFactors");
        
        /// <summary>
        /// 取得中密度住宅需求因素陣列的參考
        /// </summary>
        private static AccessTools.FieldRef<ResidentialDemandSystem, NativeArray<int>> MediumDemandFactorsRef =
            AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeArray<int>>("m_MediumDemandFactors");
        
        /// <summary>
        /// 取得高密度住宅需求因素陣列的參考
        /// </summary>
        private static AccessTools.FieldRef<ResidentialDemandSystem, NativeArray<int>> HighDemandFactorsRef =
            AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeArray<int>>("m_HighDemandFactors");

        /// <summary>
        /// 影響住宅需求的 11 個因素清單
        /// </summary>
        private static DemandFactor[] Factors = new DemandFactor[]
        {
            DemandFactor.StorageLevels,         // 儲存水平
            DemandFactor.EducatedWorkforce,     // 受教育的勞動力
            DemandFactor.CompanyWealth,         // 公司財富
            DemandFactor.LocalDemand,           // 本地需求
            DemandFactor.FreeWorkplaces,        // 可用工作場所
            DemandFactor.Happiness,             // 幸福度
            DemandFactor.TouristDemand,         // 觀光客需求
            DemandFactor.LocalInputs,           // 本地投入
            DemandFactor.Taxes,                 // 稅收
            DemandFactor.Students,              // 學生
            DemandFactor.Warehouses,            // 倉庫
        };

        /// <summary>
        /// Harmony 前綴補丁 - 在原始方法執行前將所有住宅需求設為指定值
        /// </summary>
        /// <param name="__instance">ResidentialDemandSystem 的實例</param>
        static void Prefix(ResidentialDemandSystem __instance)
        {
            // 僅在設定存在且未設為 Off 時才修改需求值
            if (DemandModifierMod.Settings != null && DemandModifierMod.Settings.ResidentialDemandLevel != DemandLevel.Off)
            {
                int demandValue = (int)DemandModifierMod.Settings.ResidentialDemandLevel;
                
                // 設定建築需求為指定值 (包含低/中/高密度)
                BuildingDemandRef(__instance).value = demandValue;
                // 設定家庭需求為指定值
                HouseholdDemandRef(__instance).value = demandValue;

                // 遍歷所有需求因素，將低/中/高密度的每個因素都設為指定值
                foreach (var factor in Factors)
                {
                    LowDemandFactorsRef(__instance)[(int)factor] = demandValue;
                    MediumDemandFactorsRef(__instance)[(int)factor] = demandValue;
                    HighDemandFactorsRef(__instance)[(int)factor] = demandValue;
                }
            }
        }
    }
}
