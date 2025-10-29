using Colossal.Logging;
using DemandModifier;
using DemandModifier.Patches;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace DemandModifier.Utils
{
    /// <summary>
    /// Harmony 補丁工具函式類別
    /// 提供常用的補丁操作和反射工具
    /// 相容 .NET Framework 4.7.2
    /// </summary>
    public static class PatchUtils
    {
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}", nameof(DemandModifier), nameof(PatchUtils))
        ).SetShowsErrorsInUI(false);

        /// <summary>
        /// 應用所有定義的 Harmony 補丁
        /// 掃描指定命名空間中的所有補丁類別並應用
        /// </summary>
        /// <param name="harmonyId">Harmony 實例 ID (通常為模組名稱)</param>
        /// <returns>成功應用的補丁數量</returns>
        public static int ApplyPatches(string harmonyId)
        {
            try
            {
                Harmony harmony = new Harmony(harmonyId);
                
                // 取得所有定義在 DemandModifier.Patches 命名空間的補丁
                Type[] patchTypes = GetPatchTypes();
                
                if (patchTypes.Length == 0)
                {
                    log.Warn("未找到任何補丁類別");
                    return 0;
                }

                int appliedCount = 0;
                foreach (Type patchType in patchTypes)
                {
                    try
                    {
                        harmony.CreateClassProcessor(patchType).Patch();
                        log.Info(string.Format("✓ 補丁已應用: {0}", patchType.Name));
                        appliedCount++;
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("✗ 補丁應用失敗 {0}: {1}", patchType.Name, ex.Message));
                    }
                }

                log.Info(string.Format("共應用 {0} 個補丁", appliedCount));
                return appliedCount;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("應用補丁時發生異常: {0}", ex.Message));
                return 0;
            }
        }

        /// <summary>
        /// 撤銷所有 Harmony 補丁
        /// </summary>
        /// <param name="harmonyId">Harmony 實例 ID</param>
        public static void UnpatchAll(string harmonyId)
        {
            try
            {
                Harmony harmony = new Harmony(harmonyId);
                harmony.UnpatchAll(harmonyId);
                log.Info(string.Format("✓ 已撤銷所有補丁: {0}", harmonyId));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("撤銷補丁時發生異常: {0}", ex.Message));
            }
        }

        /// <summary>
        /// 取得所有補丁類別
        /// 掃描當前組件中定義的補丁
        /// </summary>
        private static Type[] GetPatchTypes()
        {
            List<Type> patchTypes = new List<Type>();
            
            try
            {
                // 掃描 DemandModifier.Patches 命名空間
                Type[] allTypes = typeof(DemandModifierMod).Assembly.GetTypes();
                
                foreach (Type type in allTypes)
                {
                    // 檢查型別是否具有 HarmonyPatch 屬性
                    object[] attributes = type.GetCustomAttributes(typeof(HarmonyPatch), false);
                    if (attributes.Length > 0)
                    {
                        patchTypes.Add(type);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("掃描補丁類別時發生異常: {0}", ex.Message));
            }

            return patchTypes.ToArray();
        }

        /// <summary>
        /// 驗證補丁是否已正確應用
        /// 檢查目標方法是否已被補丁修改
        /// </summary>
        /// <param name="targetType">目標型別</param>
        /// <param name="methodName">目標方法名稱</param>
        /// <returns>是否已補丁</returns>
        public static bool IsPatchApplied(Type targetType, string methodName)
        {
            try
            {
                if (targetType == null || string.IsNullOrEmpty(methodName))
                {
                    return false;
                }

                var method = targetType.GetMethod(methodName, 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Static);

                if (method == null)
                {
                    return false;
                }

                // 檢查方法是否已被修改
                var patches = Harmony.GetPatchInfo(method);
                return patches != null && (patches.Prefixes.Count > 0 || patches.Postfixes.Count > 0);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("驗證補丁時發生異常: {0}", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 記錄補丁資訊
        /// 用於除錯
        /// </summary>
        public static void LogPatchInfo()
        {
            try
            {
                log.Info("=== Harmony 補丁資訊 ===");
                
                Type[] patchTypes = GetPatchTypes();
                log.Info(string.Format("找到 {0} 個補丁類別", patchTypes.Length));

                foreach (Type patchType in patchTypes)
                {
                    log.Info(string.Format("  - {0}", patchType.Name));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("記錄補丁資訊時發生異常: {0}", ex.Message));
            }
        }
    }
}
