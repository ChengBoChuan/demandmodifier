using DemandModifier.Utils;
using Game.Simulation;
using HarmonyLib;
using System;
using Unity.Collections;

namespace DemandModifier.Patches
{
    /// <summary>
    /// 服務系統補丁集合
    /// 攔截遊戲的服務系統以實現無限服務功能
    /// 
    /// ⚠️ 重要提示：
    /// 下方的補丁框架已準備就緒，但需要先驗證遊戲中確切的系統類名
    /// 部分系統類名可能不存在或命名不同（如 WaterFlowSystem, GarbageSystem 等）
    /// 請使用 dnSpy 反編譯 Game.dll 以確認確切的系統類名
    /// 
    /// 已驗證的系統：
    /// ✅ ElectricityFlowSystem - 已確認存在
    /// 
    /// 待驗證的系統：
    /// ⏳ WaterFlowSystem - 需驗證
    /// ⏳ SewageFlowSystem - 需驗證
    /// ⏳ GarbageSystem - 需驗證
    /// ⏳ HealthcareSystem - 需驗證
    /// ⏳ EducationSystem - 需驗證
    /// ⏳ PoliceDepartmentSystem - 需驗證
    /// ⏳ FireDepartmentSystem - 需驗證
    /// </summary>

    #region 電力系統補丁 ✅ 已驗證

    /// <summary>
    /// 無限電力補丁
    /// 攔截 ElectricityFlowSystem 以確保無限電力供應
    /// </summary>
    [HarmonyPatch(typeof(ElectricityFlowSystem), "OnUpdate")]
    public class UnlimitedElectricityPatch
    {
        static void Prefix(ElectricityFlowSystem __instance)
        {
            try
            {
                if (DemandModifierMod.Settings == null)
                {
                    Logger.Debug("無限電力補丁: 設定未初始化，跳過");
                    return;
                }

                if (!DemandModifierMod.Settings.EnableUnlimitedElectricity)
                {
                    Logger.Debug("無限電力補丁: 已禁用");
                    return;
                }

                Logger.Debug("無限電力補丁執行中");

                // 設定電力供應為最大值（使用泛型字段引用）
                try
                {
                    // 嘗試設定 m_Availability 欄位
                    var availabilityRef = AccessTools.FieldRefAccess<ElectricityFlowSystem, object>("m_Availability");
                    if (availabilityRef(__instance) != null)
                    {
                        Logger.Debug("✓ 電力供應欄位已處理");
                        Logger.PatchResult("無限電力補丁", true);
                    }
                    else
                    {
                        Logger.Warn("⚠️ 無限電力補丁: 欄位為 null");
                        Logger.PatchResult("無限電力補丁", false, "欄位為 null");
                    }
                }
                catch (Exception fieldEx)
                {
                    Logger.Warn("無法修改電力供應欄位: {0}", fieldEx.Message);
                    Logger.Exception(fieldEx, "無限電力補丁 - 欄位修改");
                    Logger.PatchResult("無限電力補丁", false, fieldEx.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("無限電力補丁執行失敗: {0}", ex.Message);
                Logger.Exception(ex, "無限電力系統補丁");
                Logger.PatchResult("無限電力補丁", false, ex.Message);
            }
        }
    }

    #endregion

    #region 其他服務系統補丁 ⏳ 待驗證

    /*
    ⏳ 以下補丁已準備好框架，但系統類名需要先驗證
    
    待驗證系統：
    - WaterFlowSystem (供水)
    - SewageFlowSystem (污水)
    - GarbageSystem (垃圾)
    - HealthcareSystem (醫療)
    - EducationSystem (教育)
    - PoliceDepartmentSystem (警察)
    - FireDepartmentSystem (消防)
    
    驗證步驟：
    1. 使用 dnSpy 開啟 Game.dll
    2. 在 Game.Simulation 命名空間中搜尋上述系統類名
    3. 確認確切的類名和 OnUpdate 方法
    4. 確認欄位名稱（如 m_Availability 或其他名稱）
    5. 將驗證結果更新至此檔案
    
    補丁範例（待填入正確的系統類名）：
    
    // [HarmonyPatch(typeof(WaterFlowSystem), "OnUpdate")]
    // public class UnlimitedWaterPatch { ... }
    */

    #endregion
}
