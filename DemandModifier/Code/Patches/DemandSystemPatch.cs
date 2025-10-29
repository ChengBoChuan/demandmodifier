using Colossal.Logging;
using DemandModifier;
using Game.Simulation;
using HarmonyLib;
using System;
using Unity.Collections;

namespace DemandModifier.Patches
{
    /// <summary>
    /// 需求系統補丁集合
    /// 攔截遊戲的需求系統 OnUpdate 方法以實現需求控制
    /// </summary>

    /// <summary>
    /// 住宅需求系統補丁
    /// 攔截 ResidentialDemandSystem.OnUpdate() 方法以修改住宅需求值
    /// </summary>
    [HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
    public class ResidentialDemandSystemPatch
    {
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}", nameof(DemandModifier), "Patches.Residential")
        ).SetShowsErrorsInUI(false);

        static void Prefix(ResidentialDemandSystem __instance)
        {
            try
            {
                if (null == DemandModifierMod.Settings)
                    return;

                DemandLevel level = DemandModifierMod.Settings.ResidentialDemandLevel;

                if (level == DemandLevel.Off)
                    return;

                int demandValue = (int)level;

                // 使用反射設置住宅需求
                var fieldInfo = typeof(ResidentialDemandSystem).GetField("m_BuildingDemand", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(__instance, demandValue);
                    log.Debug(string.Format("住宅需求已設置為: {0}", demandValue));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("住宅需求補丁失敗: {0}", ex.Message));
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
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}", nameof(DemandModifier), "Patches.Commercial")
        ).SetShowsErrorsInUI(false);

        static void Prefix(CommercialDemandSystem __instance)
        {
            try
            {
                if (null == DemandModifierMod.Settings)
                    return;

                DemandLevel level = DemandModifierMod.Settings.CommercialDemandLevel;

                if (level == DemandLevel.Off)
                    return;

                int demandValue = (int)level;

                // 使用反射設置商業需求
                var fieldInfo = typeof(CommercialDemandSystem).GetField("m_BuildingDemand",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(__instance, demandValue);
                    log.Debug(string.Format("商業需求已設置為: {0}", demandValue));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("商業需求補丁失敗: {0}", ex.Message));
            }
        }
    }

    /// <summary>
    /// 工業需求系統補丁
    /// 攔截 IndustrialDemandSystem.OnUpdate() 方法以修改工業需求值
    /// </summary>
    [HarmonyPatch(typeof(IndustrialDemandSystem), "OnUpdate")]
    public class IndustrialDemandSystemPatch
    {
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}", nameof(DemandModifier), "Patches.Industrial")
        ).SetShowsErrorsInUI(false);

        static void Prefix(IndustrialDemandSystem __instance)
        {
            try
            {
                if (null == DemandModifierMod.Settings)
                    return;

                DemandLevel level = DemandModifierMod.Settings.IndustrialDemandLevel;

                if (level == DemandLevel.Off)
                    return;

                int demandValue = (int)level;

                // 使用反射設置工業需求
                var fieldInfo = typeof(IndustrialDemandSystem).GetField("m_BuildingDemand",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(__instance, demandValue);
                    log.Debug(string.Format("工業需求已設置為: {0}", demandValue));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("工業需求補丁失敗: {0}", ex.Message));
            }
        }
    }
}
