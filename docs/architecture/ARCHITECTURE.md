# DemandModifier 架構文件

## 專案概述

本文件介紹 DemandModifier 模組的完整架構、設計模式和實作細節。

### 核心組件

1. **需求控制系統 (Demand Control)**
   - 住宅需求 (Residential Demand)
   - 商業需求 (Commercial Demand)
   - 工業需求 (Industrial Demand)
   - 支援 5 個等級：Off / Low / Medium / High / Maximum

2. **服務控制系統 (Service Control)**
   - 電力、水、污水、垃圾等基礎設施
   - 醫療、教育、警察、消防等公共服務

3. **經濟控制系統 (Economy Control)**
   - 無限金錢
   - 免費建造
   - 零維護成本

## 檔案結構

```
DemandModifier/
├── Code/
│   ├── Localization/          # 多國語言系統
│   │   ├── LocalizationInitializer.cs
│   │   └── ModLocale.cs
│   ├── Patches/               # Harmony 補丁系統
│   │   ├── DemandSystemPatch.cs
│   │   ├── ServiceSystemPatch.cs
│   │   └── PatchRegistry.cs
│   ├── Systems/               # 輔助系統
│   │   └── DemandSystemHelper.cs
│   └── Utils/                 # 工具函式
│       └── PatchUtils.cs
├── l10n/                      # 多國語言檔案 (8 種語言)
├── DemandModifierMod.cs       # 模組入口點
├── DemandModifierSettings.cs  # 設定介面
└── ...
```

## 核心架構模式

### 1. Harmony 補丁架構

使用 Prefix 補丁攔截遊戲系統的 OnUpdate 方法，在原始邏輯執行前修改需求值。

**為何選擇 Prefix**：
- 在原始計算前執行，可覆蓋遊戲計算結果
- 保留原始方法執行，維持其他遊戲邏輯
- 效能開銷最小

### 2. 多國語言系統

所有使用者介面文本都透過多國語言系統提供，支援 8 種語言：
- 英文 (en-US)
- 繁體中文 (zh-HANT)
- 簡體中文 (zh-HANS)
- 日文 (ja-JP)
- 德文 (de-DE)
- 西班牙文 (es-ES)
- 法文 (fr-FR)

### 3. 設定系統

使用遊戲原生的 ModSetting 系統提供遊戲內設定介面，支援：
- 下拉選單 (Enum)
- 布林開關 (Boolean)
- 數值滑桿 (Slider)

## 開發指南

### 新增功能的步驟

1. 在 `DemandModifierSettings.cs` 中定義新屬性
2. 在 8 個語言檔案 (`l10n/*.json`) 中添加翻譯
3. 在 `Code\Patches\` 中建立對應的 Harmony 補丁
4. 在 `PatchRegistry.cs` 中註冊補丁

### 命名規則

- 命名空間：`DemandModifier.<SubModule>`
- 類別名稱：`<系統>Patch` (如 `CommercialDemandSystemPatch`)
- 語言鍵值：`Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.<屬性名>]`

## 技術細節

### .NET 4.7.2 相容性

- 不使用 C# 10+ 新語法
- 字串連接使用 `string.Format()` 而非 `$` 插值
- 陣列初始化使用 `new Type[] { }` 而非 `[ ]`
- 顯式 null 檢查，不使用 null-conditional operators

### Harmony 補丁簽名

```csharp
[HarmonyPatch(typeof(TargetClass), "OnUpdate")]
public class TargetClassPatch
{
    static void Prefix(TargetClass __instance)
    {
        // 補丁邏輯
    }
}
```

**特殊參數**：
- `__instance`: 被攔截的物件實例
- `__result`: 原始方法的返回值 (Postfix 使用)
- `__state`: Prefix/Postfix 間的狀態傳遞

## 常見問題

### Q: 為什麼需求沒有立即改變?
A: 需求值在 OnUpdate 中每幀重新計算，補丁在下一幀執行時才會生效。

### Q: 多語言翻譯如何新增?
A: 在所有 8 個 `l10n/*.json` 檔案中添加相同的鍵值即可，遊戲引擎會自動根據玩家語言載入。

### Q: 補丁可以跳過原始方法執行嗎?
A: 可以，Prefix 返回 false 會跳過原始方法，但通常應保持原始方法執行以維持遊戲穩定性。

## 參考資料

- Harmony 文件: https://harmony.pardeike.net/
- Cities: Skylines 2 Modding Discord: 官方社群
- Traffic Mod: https://github.com/krzychu124/Traffic (參考實作)
