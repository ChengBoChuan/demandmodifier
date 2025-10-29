using System;
using System.Collections.Generic;
using System.IO;
using Colossal;
using Game.Modding;
using Game.SceneFlow;

namespace DemandModifier
{
    /// <summary>
    /// 多國語系管理系統 - 參考 Traffic 專案實作模式
    /// 提供語系鍵值的產生和翻譯字典的管理
    /// </summary>
    public partial class Localization
    {
        /// <summary>
        /// 靜態字典，儲存所有可用的語言資訊
        /// 格式：Key = 語言代碼 (如 "en-US")，Value = (顯示名稱, 翻譯進度%, IDictionarySource)
        /// </summary>
        internal static readonly Dictionary<string, Tuple<string, string, IDictionarySource>> LocaleSources = 
            new Dictionary<string, Tuple<string, string, IDictionarySource>>();
    }
}
