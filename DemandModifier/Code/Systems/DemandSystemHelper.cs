using Colossal.Logging;
using DemandModifier;
using System;

namespace DemandModifier.Systems
{
    /// <summary>
    /// 需求系統輔助函式類別
    /// 提供需求值計算和驗證
    /// 相容 .NET Framework 4.7.2
    /// </summary>
    public static class DemandSystemHelper
    {
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandSystemHelper))
        ).SetShowsErrorsInUI(false);

        /// <summary>
        /// 需求值常數定義
        /// </summary>
        public const int DEMAND_OFF = 0;
        public const int DEMAND_LOW = 64;
        public const int DEMAND_MEDIUM = 128;
        public const int DEMAND_HIGH = 192;
        public const int DEMAND_MAXIMUM = 255;

        /// <summary>
        /// 將 DemandLevel 列舉轉換為整數值
        /// </summary>
        /// <param name="level">需求等級</param>
        /// <returns>對應的整數值 (0-255)</returns>
        public static int DemandLevelToValue(DemandLevel level)
        {
            switch (level)
            {
                case DemandLevel.Off:
                    return DEMAND_OFF;
                case DemandLevel.Low:
                    return DEMAND_LOW;
                case DemandLevel.Medium:
                    return DEMAND_MEDIUM;
                case DemandLevel.High:
                    return DEMAND_HIGH;
                case DemandLevel.Maximum:
                    return DEMAND_MAXIMUM;
                default:
                    log.Warn(string.Format("未知的需求等級: {0}, 使用預設值", level));
                    return DEMAND_OFF;
            }
        }

        /// <summary>
        /// 將整數值轉換為 DemandLevel 列舉
        /// </summary>
        /// <param name="value">整數值 (0-255)</param>
        /// <returns>最接近的需求等級</returns>
        public static DemandLevel ValueToDemandLevel(int value)
        {
            if (value <= 0)
                return DemandLevel.Off;
            else if (value <= 64)
                return DemandLevel.Low;
            else if (value <= 128)
                return DemandLevel.Medium;
            else if (value <= 192)
                return DemandLevel.High;
            else
                return DemandLevel.Maximum;
        }

        /// <summary>
        /// 驗證需求值是否在有效範圍內
        /// </summary>
        /// <param name="value">需求值</param>
        /// <returns>是否有效 (0-255)</returns>
        public static bool IsValidDemandValue(int value)
        {
            return value >= DEMAND_OFF && value <= DEMAND_MAXIMUM;
        }

        /// <summary>
        /// 將需求值限制在有效範圍內
        /// </summary>
        /// <param name="value">需求值</param>
        /// <returns>限制後的值</returns>
        public static int ClampDemandValue(int value)
        {
            if (value < DEMAND_OFF)
                return DEMAND_OFF;
            if (value > DEMAND_MAXIMUM)
                return DEMAND_MAXIMUM;
            return value;
        }

        /// <summary>
        /// 計算百分比需求值
        /// 將百分比 (0-100) 轉換為遊戲需求值 (0-255)
        /// </summary>
        /// <param name="percentage">百分比 (0-100)</param>
        /// <returns>對應的遊戲需求值</returns>
        public static int PercentageToDemandValue(float percentage)
        {
            if (percentage < 0f)
                percentage = 0f;
            if (percentage > 100f)
                percentage = 100f;

            int value = (int)((percentage / 100f) * DEMAND_MAXIMUM);
            return ClampDemandValue(value);
        }

        /// <summary>
        /// 計算需求值的百分比
        /// 將遊戲需求值 (0-255) 轉換為百分比 (0-100)
        /// </summary>
        /// <param name="value">遊戲需求值</param>
        /// <returns>對應的百分比</returns>
        public static float DemandValueToPercentage(int value)
        {
            value = ClampDemandValue(value);
            return (float)value / (float)DEMAND_MAXIMUM * 100f;
        }

        /// <summary>
        /// 獲取需求等級的百分比表示
        /// </summary>
        /// <param name="level">需求等級</param>
        /// <returns>百分比字串 (如 "50%")</returns>
        public static string GetDemandLevelPercentage(DemandLevel level)
        {
            int value = DemandLevelToValue(level);
            float percentage = DemandValueToPercentage(value);
            return string.Format("{0:F0}%", percentage);
        }

        /// <summary>
        /// 記錄需求值資訊
        /// 用於除錯
        /// </summary>
        /// <param name="demandType">需求類型 (如 "住宅", "商業")</param>
        /// <param name="level">需求等級</param>
        /// <param name="value">實際需求值</param>
        public static void LogDemandValue(string demandType, DemandLevel level, int value)
        {
            log.Debug(string.Format(
                "{0}需求 - 等級: {1}, 值: {2}/255 ({3})",
                demandType,
                level,
                value,
                GetDemandLevelPercentage(level)
            ));
        }

        /// <summary>
        /// 驗證需求配置有效性
        /// </summary>
        public static void ValidateDemandConfiguration()
        {
            try
            {
                if (null == DemandModifierMod.Settings)
                {
                    log.Warn("需求設定未初始化");
                    return;
                }

                log.Info("=== 需求配置驗證 ===");
                log.Info(string.Format("住宅需求: {0} ({1})", 
                    DemandModifierMod.Settings.ResidentialDemandLevel,
                    GetDemandLevelPercentage(DemandModifierMod.Settings.ResidentialDemandLevel)));
                log.Info(string.Format("商業需求: {0} ({1})", 
                    DemandModifierMod.Settings.CommercialDemandLevel,
                    GetDemandLevelPercentage(DemandModifierMod.Settings.CommercialDemandLevel)));
                log.Info(string.Format("工業需求: {0} ({1})", 
                    DemandModifierMod.Settings.IndustrialDemandLevel,
                    GetDemandLevelPercentage(DemandModifierMod.Settings.IndustrialDemandLevel)));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("驗證配置時發生異常: {0}", ex.Message));
            }
        }
    }
}
