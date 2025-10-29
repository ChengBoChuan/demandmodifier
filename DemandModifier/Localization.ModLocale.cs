using System;
using System.Collections.Generic;
using System.IO;
using Colossal;

namespace DemandModifier
{
    /// <summary>
    /// 模組語言檔案載入器 - 從 JSON 檔案讀取翻譯
    /// 嚴格參考 Traffic 專案的 ModLocale 實作
    /// 
    /// 注意：此類別不需要手動載入 JSON 內容
    /// 遊戲引擎會自動掃描 l10n/ 資料夾並透過此介面提供翻譯
    /// </summary>
    public partial class Localization
    {
        public class ModLocale : IDictionarySource
        {
            private readonly string _localeId;
            private readonly string _localeFilePath;

            public ModLocale(string localeId, string localeFilePath)
            {
                _localeId = localeId;
                _localeFilePath = localeFilePath;
            }

            /// <summary>
            /// 讀取所有翻譯條目 - 遊戲引擎會呼叫此方法
            /// 由於遊戲引擎自動掃描 JSON，此方法返回空字典
            /// 實際的翻譯載入由遊戲內部處理
            /// </summary>
            public IEnumerable<KeyValuePair<string, string>> ReadEntries(
                IList<IDictionaryEntryError> errors, 
                Dictionary<string, int> indexCounts)
            {
                // 返回空字典 - 遊戲引擎會從 JSON 檔案直接讀取
                return new Dictionary<string, string>();
            }

            /// <summary>
            /// 卸載此語言檔案
            /// </summary>
            public void Unload()
            {
                // 清理資源（如有）
            }

            /// <summary>
            /// 返回此語言檔案的識別字串
            /// </summary>
            public override string ToString()
            {
                return string.Format("DemandModifier.Locale.{0}", _localeId);
            }
        }
    }
}
