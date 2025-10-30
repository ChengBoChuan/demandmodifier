using Colossal.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DemandModifier.Utils
{
    /// <summary>
    /// 高級日誌系統 - 參考 Traffic 專案實作
    /// 提供多個日誌等級、條件編譯、效能追蹤等功能
    /// </summary>
    public static class Logger
    {
        private static ILog _logger;
        private static Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();

        /// <summary>
        /// 初始化日誌系統
        /// </summary>
        public static void Initialize(string modName, string className)
        {
            _logger = LogManager.GetLogger(
                string.Format("{0}.{1}", modName, className)
            ).SetShowsErrorsInUI(false);
        }

        /// <summary>
        /// 日誌等級列舉
        /// </summary>
        public enum LogLevel
        {
            Trace = 0,      // 詳細追蹤資訊
            Debug = 1,      // 除錯資訊
            Info = 2,       // 一般資訊
            Warn = 3,       // 警告訊息
            Error = 4,      // 錯誤訊息
            Critical = 5    // 嚴重錯誤
        }

        /// <summary>
        /// 最詳細的除錯資訊 - 記錄變數值、執行流程
        /// </summary>
        [Conditional("DEBUG_DEMAND")]
        public static void Trace(string message, params object[] args)
        {
            if (_logger == null) return;
            string formatted = FormatMessage("[TRACE]", message, args);
            _logger.Debug(formatted);
        }

        /// <summary>
        /// 除錯資訊 - 開發階段的詳細資訊
        /// </summary>
        public static void Debug(string message, params object[] args)
        {
            if (_logger == null) return;
            string formatted = FormatMessage("[DEBUG]", message, args);
            _logger.Debug(formatted);
        }

        /// <summary>
        /// 一般資訊 - 記錄重要事件（模組載入、設定變更）
        /// </summary>
        public static void Info(string message, params object[] args)
        {
            if (_logger == null) return;
            string formatted = FormatMessage("[INFO]", message, args);
            _logger.Info(formatted);
        }

        /// <summary>
        /// 警告訊息 - 非預期但可處理的情況
        /// </summary>
        public static void Warn(string message, params object[] args)
        {
            if (_logger == null) return;
            string formatted = FormatMessage("[WARN]", message, args);
            _logger.Warn(formatted);
        }

        /// <summary>
        /// 錯誤訊息 - 功能失敗但不崩潰遊戲
        /// </summary>
        public static void Error(string message, params object[] args)
        {
            if (_logger == null) return;
            string formatted = FormatMessage("[ERROR]", message, args);
            _logger.Error(formatted);
        }

        /// <summary>
        /// 嚴重錯誤 - 可能導致崩潰的錯誤
        /// </summary>
        public static void Critical(string message, params object[] args)
        {
            if (_logger == null) return;
            string formatted = FormatMessage("[CRITICAL]", message, args);
            _logger.Error(formatted);  // 使用 Error 以確保記錄
        }

        /// <summary>
        /// 記錄例外情況
        /// </summary>
        public static void Exception(Exception ex, string message = "發生例外")
        {
            if (_logger == null) return;
            string formatted = string.Format(
                "[EXCEPTION] {0}\n異常訊息: {1}\n堆疊追蹤: {2}",
                message,
                ex.Message,
                ex.StackTrace
            );
            _logger.Error(formatted);
        }

        /// <summary>
        /// 記錄方法進入
        /// </summary>
        public static void MethodEnter(string methodName, params object[] args)
        {
            if (_logger == null) return;
            string argsStr = args.Length > 0 
                ? string.Format("({0})", string.Join(", ", args)) 
                : "()";
            string formatted = string.Format("[METHOD_ENTER] {0}{1}", methodName, argsStr);
            _logger.Debug(formatted);
        }

        /// <summary>
        /// 記錄方法退出
        /// </summary>
        public static void MethodExit(string methodName, object result = null)
        {
            if (_logger == null) return;
            string resultStr = result != null 
                ? string.Format(" → {0}", result) 
                : "";
            string formatted = string.Format("[METHOD_EXIT] {0}{1}", methodName, resultStr);
            _logger.Debug(formatted);
        }

        /// <summary>
        /// 開始效能計時 - 用於衡量某段程式碼的執行時間
        /// </summary>
        public static void StartTimer(string label)
        {
            if (!_stopwatches.ContainsKey(label))
            {
                _stopwatches[label] = new Stopwatch();
            }
            _stopwatches[label].Restart();
            Debug("計時開始: {0}", label);
        }

        /// <summary>
        /// 停止計時並記錄耗時
        /// </summary>
        public static long StopTimer(string label, long thresholdMs = 0)
        {
            if (!_stopwatches.ContainsKey(label))
            {
                Warn("計時標籤不存在: {0}", label);
                return -1;
            }

            _stopwatches[label].Stop();
            long elapsedMs = _stopwatches[label].ElapsedMilliseconds;
            
            string formatted = string.Format("計時結束: {0} = {1}ms", label, elapsedMs);
            
            if (thresholdMs > 0 && elapsedMs > thresholdMs)
            {
                Warn("⚠️ 超過閾值! {0} (閾值: {1}ms)", formatted, thresholdMs);
            }
            else
            {
                Debug(formatted);
            }

            return elapsedMs;
        }

        /// <summary>
        /// 記錄流程進度 - 用於追蹤複雜邏輯的執行步驟
        /// </summary>
        public static void Progress(int step, int total, string description = "")
        {
            if (_logger == null) return;
            int percentage = (int)((step / (float)total) * 100);
            string formatted = string.Format(
                "[進度] {0}/{1} ({2}%) {3}",
                step, total, percentage, description
            );
            _logger.Info(formatted);
        }

        /// <summary>
        /// 記錄條件檢查結果
        /// </summary>
        public static void CheckCondition(string conditionName, bool result)
        {
            if (_logger == null) return;
            string status = result ? "✓ 通過" : "✗ 失敗";
            string formatted = string.Format("[條件檢查] {0}: {1}", conditionName, status);
            _logger.Debug(formatted);
        }

        /// <summary>
        /// 記錄補丁應用結果
        /// </summary>
        public static void PatchResult(string patchName, bool success, string reason = "")
        {
            if (_logger == null) return;
            string status = success ? "✓ 成功套用" : "✗ 套用失敗";
            string formatted = success
                ? string.Format("[補丁] {0}: {1}", patchName, status)
                : string.Format("[補丁] {0}: {1} ({2})", patchName, status, reason);
            
            if (success)
                _logger.Info(formatted);
            else
                _logger.Error(formatted);
        }

        /// <summary>
        /// 記錄設定變更
        /// </summary>
        public static void SettingChanged(string settingName, object oldValue, object newValue)
        {
            if (_logger == null) return;
            string formatted = string.Format(
                "[設定變更] {0}: {1} → {2}",
                settingName, oldValue, newValue
            );
            _logger.Info(formatted);
        }

        /// <summary>
        /// 記錄資源初始化
        /// </summary>
        public static void ResourceInitialized(string resourceName)
        {
            if (_logger == null) return;
            string formatted = string.Format("[資源初始化] {0} 已就緒", resourceName);
            _logger.Info(formatted);
        }

        /// <summary>
        /// 記錄資源清理
        /// </summary>
        public static void ResourceDisposed(string resourceName)
        {
            if (_logger == null) return;
            string formatted = string.Format("[資源清理] {0} 已清理", resourceName);
            _logger.Info(formatted);
        }

        /// <summary>
        /// 記錄檢驗點
        /// </summary>
        public static void Checkpoint(string checkpointName)
        {
            if (_logger == null) return;
            string formatted = string.Format("[檢驗點] {0}", checkpointName);
            _logger.Debug(formatted);
        }

        /// <summary>
        /// 記錄分隔符（用於視覺分隔日誌）
        /// </summary>
        public static void Separator(string title = "")
        {
            if (_logger == null) return;
            string formatted = title.Length > 0
                ? string.Format("═════════════ {0} ═════════════", title)
                : "═══════════════════════════════════════";
            _logger.Info(formatted);
        }

        /// <summary>
        /// 內部方法：格式化訊息
        /// </summary>
        private static string FormatMessage(string level, string message, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return string.Format("{0} {1}", level, message);
            }

            try
            {
                return string.Format("{0} {1}", level, string.Format(message, args));
            }
            catch
            {
                // 若格式化失敗，返回原始訊息
                return string.Format("{0} {1}", level, message);
            }
        }

        /// <summary>
        /// 取得內部 ILog 實例（進階用法）
        /// </summary>
        public static ILog GetRawLogger()
        {
            return _logger;
        }
    }
}
