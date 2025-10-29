using Colossal;
using Colossal.Localization;
using Colossal.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace DemandModifier.Localization
{
    /// <summary>
    /// 模組語言資料來源
    /// 實作 IDictionarySource 介面以支援多國語言
    /// 相容 .NET Framework 4.7.2（使用手動 JSON 解析）
    /// </summary>
    public class ModLocale : Colossal.IDictionarySource
    {
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}.{2}", nameof(DemandModifier), nameof(Localization), nameof(ModLocale))
        ).SetShowsErrorsInUI(false);

        private readonly string localeId;
        private readonly string jsonPath;
        private Dictionary<string, string> translations;

        /// <summary>
        /// 建立新的模組語言實例
        /// </summary>
        /// <param name="localeId">語言代碼 (如 en-US, zh-HANT)</param>
        /// <param name="jsonPath">JSON 檔案的完整路徑</param>
        public ModLocale(string localeId, string jsonPath)
        {
            this.localeId = localeId;
            this.jsonPath = jsonPath;
            this.translations = new Dictionary<string, string>();
        }

        /// <summary>
        /// 從 JSON 檔案載入翻譯
        /// 使用手動 JSON 解析以相容 .NET 4.7.2
        /// </summary>
        public void Load()
        {
            try
            {
                if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
                {
                    throw new FileNotFoundException(string.Format("語言檔案不存在: {0}", jsonPath));
                }

                string jsonContent = File.ReadAllText(jsonPath);
                ParseJsonManually(jsonContent);
                log.Info(string.Format("已載入語言 {0}: 包含 {1} 個翻譯條目", localeId, translations.Count));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("讀取語言檔案 {0} 時發生錯誤: {1}", jsonPath, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// 手動解析 JSON 物件
        /// 因為 System.Text.Json 在 .NET 4.7.2 不可用
        /// 簡化實作：直接從行中提取鍵值對
        /// </summary>
        private void ParseJsonManually(string jsonContent)
        {
            translations.Clear();

            // 移除首尾的花括號和空白
            string content = jsonContent.Trim();
            if (content.StartsWith("{"))
            {
                content = content.Substring(1);
            }
            if (content.EndsWith("}"))
            {
                content = content.Substring(0, content.Length - 1);
            }

            // 分割每行並處理
            string[] lines = content.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                
                // 跳過空行和註解
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.Equals(","))
                {
                    continue;
                }

                // 移除末尾的逗號
                if (trimmedLine.EndsWith(","))
                {
                    trimmedLine = trimmedLine.Substring(0, trimmedLine.Length - 1);
                }

                // 尋找冒號分隔符
                int colonIndex = trimmedLine.IndexOf(":");
                if (colonIndex <= 0)
                {
                    continue;
                }

                // 提取鍵名
                string key = trimmedLine.Substring(0, colonIndex).Trim();
                key = RemoveQuotes(key);

                // 提取值
                string value = trimmedLine.Substring(colonIndex + 1).Trim();
                value = RemoveQuotes(value);

                // 解碼轉義字元
                value = UnescapeJsonString(value);

                if (!string.IsNullOrEmpty(key))
                {
                    translations[key] = value;
                }
            }
        }

        /// <summary>
        /// 移除字串前後的雙引號
        /// </summary>
        private string RemoveQuotes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (input.StartsWith("\"") && input.EndsWith("\"") && input.Length >= 2)
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }

        /// <summary>
        /// 解碼 JSON 轉義字元序列
        /// </summary>
        private string UnescapeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\/", "/");
        }

        /// <summary>
        /// IDictionarySource 實作 - ReadEntries 方法
        /// 讀取所有翻譯條目供遊戲引擎使用
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return translations;
        }

        /// <summary>
        /// IDictionarySource 實作 - Unload 方法
        /// 在語言卸載時清理資源
        /// </summary>
        public void Unload()
        {
            if (translations != null)
            {
                translations.Clear();
            }
        }

        /// <summary>
        /// 取得特定鍵值的翻譯
        /// </summary>
        public string GetTranslation(string key)
        {
            string value;
            if (translations.TryGetValue(key, out value))
            {
                return value;
            }
            return key;
        }

        /// <summary>
        /// 檢查特定鍵值是否存在
        /// </summary>
        public bool ContainsKey(string key)
        {
            return translations.ContainsKey(key);
        }

        /// <summary>
        /// 取得已載入翻譯的數量
        /// </summary>
        public int Count
        {
            get { return translations.Count; }
        }

        /// <summary>
        /// 列出所有已載入的鍵值
        /// 用於除錯和驗證
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            return translations.Keys;
        }
    }
}
