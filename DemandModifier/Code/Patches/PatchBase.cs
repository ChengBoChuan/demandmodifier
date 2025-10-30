using Colossal.Logging;
using HarmonyLib;
using System;
using Unity.Collections;
using DemandModifier.Utils;

namespace DemandModifier.Patches
{
    /// <summary>
    /// 補丁基類 - 提供通用的補丁邏輯和日誌記錄
    /// 參考 Traffic 專案的補丁設計模式
    /// </summary>
    public abstract class DemandSystemPatchBase
    {
        /// <summary>
        /// 補丁系統名稱（用於日誌）
        /// </summary>
        protected abstract string PatchName { get; }

        /// <summary>
        /// 補丁系統類別型別
        /// </summary>
        protected abstract Type TargetSystemType { get; }

        /// <summary>
        /// 目標欄位名稱
        /// </summary>
        protected abstract string TargetFieldName { get; }

        /// <summary>
        /// 執行前檢查 - 在修改值前檢查條件
        /// </summary>
        protected virtual bool PrePatchCheck()
        {
            // 檢查設定是否已初始化
            if (DemandModifierMod.Settings == null)
            {
                Logger.Debug("{0}: 設定未初始化，跳過補丁", PatchName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 執行後檢查 - 驗證修改是否成功
        /// </summary>
        protected virtual void PostPatchCheck(bool success, string reason = "")
        {
            Logger.PatchResult(PatchName, success, reason);
        }

        /// <summary>
        /// 記錄補丁應用
        /// </summary>
        protected void LogPatchApplied(int value)
        {
            Logger.Debug("{0} 已套用: 值 = {1}", PatchName, value);
        }

        /// <summary>
        /// 記錄補丁異常
        /// </summary>
        protected void LogPatchException(Exception ex)
        {
            Logger.Error("{0} 異常: {1}", PatchName, ex.Message);
            Logger.Exception(ex, string.Format("{0} 補丁執行失敗", PatchName));
        }
    }

    /// <summary>
    /// 需求系統補丁基類 - 用於住宅、商業、工業需求
    /// </summary>
    public abstract class DemandSystemPatchBase<T> : DemandSystemPatchBase where T : class
    {
        /// <summary>
        /// 取得需求值
        /// </summary>
        protected abstract int GetDemandValue();

        /// <summary>
        /// 修改私有欄位的通用方法
        /// </summary>
        protected void ModifyFieldValue(T instance, string fieldName, int value)
        {
            try
            {
                Logger.MethodEnter("ModifyFieldValue", fieldName, value);

                if (!PrePatchCheck())
                {
                    return;
                }

                var fieldInfo = typeof(T).GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (fieldInfo == null)
                {
                    Logger.Warn("{0}: 無法找到欄位 '{1}'", PatchName, fieldName);
                    PostPatchCheck(false, "欄位不存在");
                    return;
                }

                object fieldValue = fieldInfo.GetValue(instance);
                Logger.Checkpoint(string.Format("欄位原始值: {0}", fieldValue));

                fieldInfo.SetValue(instance, value);
                LogPatchApplied(value);
                PostPatchCheck(true);

                Logger.MethodExit("ModifyFieldValue");
            }
            catch (Exception ex)
            {
                LogPatchException(ex);
                PostPatchCheck(false, ex.Message);
            }
        }

        /// <summary>
        /// 修改 NativeValue 型別欄位（Unity ECS 執行緒安全容器）
        /// </summary>
        protected void ModifyNativeValueField<TValue>(T instance, string fieldName, TValue value) where TValue : struct
        {
            try
            {
                Logger.MethodEnter("ModifyNativeValueField", fieldName, value);

                if (!PrePatchCheck())
                {
                    return;
                }

                var fieldInfo = typeof(T).GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (fieldInfo == null)
                {
                    Logger.Warn("{0}: 無法找到 NativeValue 欄位 '{1}'", PatchName, fieldName);
                    PostPatchCheck(false, "NativeValue 欄位不存在");
                    return;
                }

                // 使用通用反射方式修改 NativeValue 欄位
                try
                {
                    var fieldValue = fieldInfo.GetValue(instance);
                    if (fieldValue != null)
                    {
                        var valueProperty = fieldValue.GetType().GetProperty("value",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (valueProperty != null && valueProperty.CanWrite)
                        {
                            valueProperty.SetValue(fieldValue, value);
                        }
                    }
                }
                catch
                {
                    Logger.Warn("{0}: 無法設定 NativeValue 欄位的 value 屬性", PatchName);
                }

                Logger.Checkpoint(string.Format("NativeValue 已修改: {0}", value));
                PostPatchCheck(true);

                Logger.MethodExit("ModifyNativeValueField");
            }
            catch (Exception ex)
            {
                LogPatchException(ex);
                PostPatchCheck(false, ex.Message);
            }
        }

        /// <summary>
        /// 修改 NativeArray 型別欄位（使用通用方式）
        /// </summary>
        protected void ModifyNativeArrayField<TElement>(T instance, string fieldName, int index, TElement value) where TElement : struct
        {
            try
            {
                Logger.MethodEnter("ModifyNativeArrayField", fieldName, index, value);

                if (!PrePatchCheck())
                {
                    return;
                }

                var fieldInfo = typeof(T).GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (fieldInfo == null)
                {
                    Logger.Warn("{0}: 無法找到 NativeArray 欄位 '{1}'", PatchName, fieldName);
                    PostPatchCheck(false, "NativeArray 欄位不存在");
                    return;
                }

                // 使用通用反射方式存取 NativeArray
                try
                {
                    var arrayObj = fieldInfo.GetValue(instance);
                    if (arrayObj != null)
                    {
                        // 使用 Item 屬性設定陣列元素
                        var indexProperty = arrayObj.GetType().GetProperty("Item",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (indexProperty != null && indexProperty.CanWrite)
                        {
                            indexProperty.SetValue(arrayObj, value, new object[] { index });
                            Logger.Checkpoint(string.Format("NativeArray[{0}] 已修改: {1}", index, value));
                        }
                    }
                }
                catch
                {
                    Logger.Warn("{0}: 無法修改 NativeArray 元素", PatchName);
                }

                PostPatchCheck(true);
                Logger.MethodExit("ModifyNativeArrayField");
            }
            catch (Exception ex)
            {
                LogPatchException(ex);
                PostPatchCheck(false, ex.Message);
            }
        }
    }

    /// <summary>
    /// 簡單需求系統補丁基類 - 用於只需修改單一欄位的系統
    /// </summary>
    public abstract class SimpleDemandPatchBase<T> : DemandSystemPatchBase<T> where T : class
    {
        /// <summary>
        /// 執行補丁前綴
        /// </summary>
        protected void ExecutePatch(T instance)
        {
            try
            {
                Logger.MethodEnter("ExecutePatch");

                if (!PrePatchCheck())
                {
                    Logger.Debug("{0}: 前置檢查未通過", PatchName);
                    return;
                }

                int demandValue = GetDemandValue();

                Logger.Checkpoint(string.Format("即將套用 {0}，值: {1}", PatchName, demandValue));

                ModifyNativeValueField(instance, TargetFieldName, demandValue);

                Logger.MethodExit("ExecutePatch");
            }
            catch (Exception ex)
            {
                LogPatchException(ex);
            }
        }

        protected override int GetDemandValue()
        {
            // 預設實現：返回 0，由子類別覆寫
            return 0;
        }
    }
}
