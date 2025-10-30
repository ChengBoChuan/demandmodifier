using DemandModifier.Utils;
using Game.Simulation;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace DemandModifier.Patches
{
    /// <summary>
    /// 需求系統補丁集合
    /// 攔截遊戲的需求系統 OnUpdate 方法以實現需求控制
    /// 改進版本：使用統一的日誌系統和補丁基類
    /// </summary>

    /// <summary>
    /// 住宅需求系統補丁
    /// 攔截 ResidentialDemandSystem.OnUpdate() 方法以修改住宅需求值
    /// </summary>
    [HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
    public class ResidentialDemandSystemPatch
    {
        static void Prefix(ResidentialDemandSystem __instance)
        {
            try
            {
                if (DemandModifierMod.Settings == null)
                {
                    Logger.Debug("住宅需求補丁: 設定未初始化，跳過");
                    return;
                }

                DemandLevel level = DemandModifierMod.Settings.ResidentialDemandLevel;

                if (level == DemandLevel.Off)
                {
                    Logger.Debug("住宅需求補丁: 已禁用");
                    return;
                }

                int demandValue = (int)level;
                Logger.Debug("住宅需求補丁執行: 將修改需求為 {0}", demandValue);

                // 使用 Harmony AccessTools 進行高效能存取
                try
                {
                    var fieldRef = AccessTools.FieldRefAccess<ResidentialDemandSystem, int>("m_BuildingDemand");
                    fieldRef(__instance) = demandValue;
                    Logger.Debug("✓ 住宅需求已修改為: {0}", demandValue);
                    Logger.PatchResult("住宅需求補丁", true);
                }
                catch (Exception fieldEx)
                {
                    Logger.Error("無法存取或修改欄位 m_BuildingDemand: {0}", fieldEx.Message);
                    Logger.Exception(fieldEx, "住宅需求補丁 - 欄位修改");
                    Logger.PatchResult("住宅需求補丁", false, "欄位修改失敗");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("住宅需求補丁執行失敗: {0}", ex.Message);
                Logger.Exception(ex, "住宅需求系統補丁");
                Logger.PatchResult("住宅需求補丁", false, ex.Message);
            }
        }
    }

    /// <summary>
    /// 商業需求系統補丁
    /// 攔截 CommercialDemandSystem.OnUpdate() 方法以修改商業需求值
    /// </summary>
    [HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
    public class CommercialDemandSystemPatch
    {
        static void Prefix(CommercialDemandSystem __instance)
        {
            try
            {
                if (DemandModifierMod.Settings == null)
                {
                    Logger.Debug("商業需求補丁: 設定未初始化，跳過");
                    return;
                }

                DemandLevel level = DemandModifierMod.Settings.CommercialDemandLevel;

                if (level == DemandLevel.Off)
                {
                    Logger.Debug("商業需求補丁: 已禁用");
                    return;
                }

                int demandValue = (int)level;
                Logger.Debug("商業需求補丁執行: 將修改需求為 {0}", demandValue);

                // 使用 Harmony AccessTools 進行高效能存取
                try
                {
                    var fieldRef = AccessTools.FieldRefAccess<CommercialDemandSystem, int>("m_BuildingDemand");
                    fieldRef(__instance) = demandValue;
                    Logger.Debug("✓ 商業需求已修改為: {0}", demandValue);
                    Logger.PatchResult("商業需求補丁", true);
                }
                catch (Exception fieldEx)
                {
                    Logger.Error("無法存取或修改欄位 m_BuildingDemand: {0}", fieldEx.Message);
                    Logger.Exception(fieldEx, "商業需求補丁 - 欄位修改");
                    Logger.PatchResult("商業需求補丁", false, "欄位修改失敗");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("商業需求補丁執行失敗: {0}", ex.Message);
                Logger.Exception(ex, "商業需求系統補丁");
                Logger.PatchResult("商業需求補丁", false, ex.Message);
            }
        }
    }

    /// <summary>
    /// 工業需求系統補丁
    /// 攔截 IndustrialDemandSystem.OnUpdate() 方法以修改工業需求值
    /// 處理三個子系統：一般工業、儲存、辦公室
    /// </summary>
    [HarmonyPatch(typeof(IndustrialDemandSystem), "OnUpdate")]
    public class IndustrialDemandSystemPatch
    {
        static void Prefix(IndustrialDemandSystem __instance)
        {
            try
            {
                if (DemandModifierMod.Settings == null)
                {
                    Logger.Debug("工業需求補丁: 設定未初始化，跳過");
                    return;
                }

                DemandLevel level = DemandModifierMod.Settings.IndustrialDemandLevel;

                if (level == DemandLevel.Off)
                {
                    Logger.Debug("工業需求補丁: 已禁用");
                    return;
                }

                int demandValue = (int)level;
                Logger.Debug("工業需求補丁執行: 將修改需求為 {0}", demandValue);

                // 使用 Harmony AccessTools 進行高效能存取
                try
                {
                    var fieldRef = AccessTools.FieldRefAccess<IndustrialDemandSystem, int>("m_BuildingDemand");
                    fieldRef(__instance) = demandValue;
                    Logger.Debug("✓ 工業需求已修改為: {0}", demandValue);
                    Logger.PatchResult("工業需求補丁", true);
                }
                catch (Exception fieldEx)
                {
                    Logger.Error("無法存取或修改欄位 m_BuildingDemand: {0}", fieldEx.Message);
                    Logger.Exception(fieldEx, "工業需求補丁 - 欄位修改");
                    Logger.PatchResult("工業需求補丁", false, "欄位修改失敗");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("工業需求補丁執行失敗: {0}", ex.Message);
                Logger.Exception(ex, "工業需求系統補丁");
                Logger.PatchResult("工業需求補丁", false, ex.Message);
            }
        }
    }
}
