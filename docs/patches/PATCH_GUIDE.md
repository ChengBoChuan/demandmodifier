# Harmony 補丁系統指南

## 概述

DemandModifier 使用 Harmony 2.x 執行時修補系統來攔截遊戲的需求和服務系統，實現功能控制。

## 核心概念

### 什麼是 Harmony 補丁？

Harmony 是一個函式庫，允許在遊戲執行時動態修改方法行為。DemandModifier 使用 **Prefix 補丁** 在原始遊戲邏輯執行**前**攔截方法呼叫。

### 為什麼使用 Prefix？

1. **在原始邏輯前執行**：可以修改輸入或直接設定輸出值
2. **保留原始方法**：返回 true 使原始方法繼續執行
3. **效能高效**：相比其他方式開銷最小

## 補丁架構

### 目錄結構

```
Code/
└── Patches/
    ├── DemandSystemPatch.cs       # 需求系統補丁
    ├── ServiceSystemPatch.cs      # 服務系統補丁 (規劃中)
    └── PatchRegistry.cs           # 補丁註冊 (規劃中)
```

### 補丁命名規則

- 類別名稱：`<系統>Patch` (如 `ResidentialDemandSystemPatch`)
- 命名空間：`DemandModifier` (頂級命名空間)

## 實作範例

### 基本補丁結構

```csharp
using HarmonyLib;
using Game.Simulation;
using Colossal.Logging;
using Unity.Collections;

namespace DemandModifier
{
    [HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
    public class ResidentialDemandSystemPatch
    {
        private static readonly ILog log = LogManager.GetLogger(
            string.Format("{0}.{1}", nameof(DemandModifier), "Patches.Residential")
        ).SetShowsErrorsInUI(false);

        // 反射快取私有欄位
        private static readonly AccessTools.FieldRef<ResidentialDemandSystem, NativeValue<int>> BuildingDemandRef =
            AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeValue<int>>("m_BuildingDemand");

        static void Prefix(ResidentialDemandSystem __instance)
        {
            if (DemandModifierMod.Settings == null)
                return;

            if (DemandModifierMod.Settings.ResidentialDemandLevel == DemandLevel.Off)
                return;

            int demandValue = (int)DemandModifierMod.Settings.ResidentialDemandLevel;
            BuildingDemandRef(__instance).value = demandValue;
        }
    }
}
```

## 關鍵技術詳解

### 1. Harmony 屬性標記

```csharp
[HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
```

- `typeof(ResidentialDemandSystem)`：被攔截的類別
- `"OnUpdate"`：被攔截的方法名稱
- 遊戲每幀執行 `OnUpdate`，補丁在前執行

### 2. 反射存取私有欄位

```csharp
private static readonly AccessTools.FieldRef<ResidentialDemandSystem, NativeValue<int>> BuildingDemandRef =
    AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeValue<int>>("m_BuildingDemand");
```

**為什麼需要反射？**
- 遊戲系統的狀態存儲在私有欄位中
- 無公開 API 修改這些值
- Harmony 的 `AccessTools` 提供安全的反射機制

**性能優化**：
- `FieldRef` 快取在類別載入時完成
- 避免在補丁執行時重複反射查詢

### 3. 特殊參數注入

```csharp
static void Prefix(ResidentialDemandSystem __instance)
```

**Harmony 特殊參數** (以 `__` 開頭)：
- `__instance`：被攔截的物件實例（非靜態方法）
- `__result`：原始方法的返回值（Postfix 和 Transpiler）
- `__state`：Prefix/Postfix 間的狀態傳遞

### 4. 條件檢查最佳實踐

```csharp
if (DemandModifierMod.Settings == null)
    return;

if (DemandModifierMod.Settings.ResidentialDemandLevel == DemandLevel.Off)
    return;
```

**為什麼重要？**
- 需求控制關閉時（設為 `Off`），應使用遊戲預設邏輯
- `Settings` 可能未初始化時應安全跳過
- 提高效能：避免不必要的修改

## 補丁類型詳解

### Prefix 補丁（本專案使用）

在原始方法**前**執行，可修改參數或跳過原始方法。

```csharp
static void Prefix(ResidentialDemandSystem __instance)
{
    // 補丁邏輯
    // 如果方法有返回值，使用 __result 修改
}

static bool Prefix(ResidentialDemandSystem __instance)
{
    // 返回 false 會跳過原始方法
    return false;
}
```

### Postfix 補丁（進階，未使用）

在原始方法**後**執行，可修改返回值。

```csharp
static void Postfix(ResidentialDemandSystem __instance, ref int __result)
{
    __result = 255;  // 修改返回值
}
```

### Transpiler 補丁（高級，未使用）

修改方法的 IL（中間語言）程式碼，可以進行精細控制但複雜且易碎。

## 遊戲系統架構認識

### Unity ECS/DOTS 系統

Cities: Skylines 2 使用 Unity 的 ECS 架構：

```
主執行緒                              工作執行緒 (Jobs)
    │                                      │
    ├─> OnUpdate() 被呼叫
    │   ├─> [Harmony Prefix] 
    │   │   └─> 修改 m_BuildingDemand    │
    │   │                                 │
    │   ├─> ScheduleJobs()
    │   │   └─> 派送到工作執行緒────────┼─> Job 1: 計算
    │   │                                 ├─> Job 2: 分析
    │   │                                 └─> Job 3: 輸出
    │   │
    │   └─> CompleteJobs() <──────────────┤
    │       (等待 Jobs 完成)              │
    │
    └─> 使用修改後的值
```

**關鍵點**：
- Jobs 在工作執行緒平行執行
- Harmony Prefix 在主執行緒執行
- 修改在 Jobs 排程**前**發生

## 常見錯誤和解決方案

### 1. 欄位名稱不正確

**症狀**：補丁執行但沒有效果，或拋出異常

**原因**：私有欄位名稱錯誤或不存在

**解決**：
1. 使用 dnSpy 反編譯 `Game.dll`
2. 找到目標類別的私有欄位
3. 確認確切的欄位名稱（區分大小寫）

### 2. Settings 為 null

**症狀**：補丁執行時 NullReferenceException

**原因**：模組尚未完全初始化

**解決**：
```csharp
if (DemandModifierMod.Settings == null)
    return;
```

### 3. 欄位類型不匹配

**症狀**：型別轉換異常

**原因**：`FieldRef` 的型別參數錯誤

**解決**：
```csharp
// ❌ 錯誤
var ref = AccessTools.FieldRefAccess<ResidentialDemandSystem, int>("m_BuildingDemand");
// Game 使用 NativeValue<int>，不是 int

// ✅ 正確
var ref = AccessTools.FieldRefAccess<ResidentialDemandSystem, NativeValue<int>>("m_BuildingDemand");
```

## 補丁註冊機制

### 自動註冊

在 `DemandModifierMod.OnLoad()` 中：

```csharp
harmony = new Harmony("DemandModifier.DemandModifierMod");
harmony.PatchAll(typeof(DemandModifierMod).Assembly);
```

Harmony 會自動掃描程式集中所有帶 `[HarmonyPatch]` 的類別。

### 手動註冊（進階）

```csharp
var method = AccessTools.Method(typeof(ResidentialDemandSystem), "OnUpdate");
var patch = new HarmonyMethod(typeof(ResidentialDemandSystemPatch), nameof(ResidentialDemandSystemPatch.Prefix));
harmony.Patch(method, prefix: patch);
```

## 效能考量

### FieldRef 快取

```csharp
// ✅ 最佳：只反射一次
private static readonly AccessTools.FieldRef<...> BuildingDemandRef = ...;

// ❌ 低效：每次補丁執行都反射
static void Prefix(...)
{
    var field = AccessTools.FieldRefAccess<...>("m_BuildingDemand");
}
```

### 條件檢查順序

```csharp
// ✅ 最佳：快速檢查先執行
if (DemandModifierMod.Settings == null) return;
if (DemandModifierMod.Settings.ResidentialDemandLevel == DemandLevel.Off) return;

// ❌ 低效：重複檢查相同條件
```

## 新增補丁的步驟

1. **確定目標方法**：使用 dnSpy 找到遊戲的目標類別和方法
2. **建立補丁類別**：在 `Code\Patches\` 中建立新檔案
3. **實作補丁邏輯**：使用 Prefix/Postfix/Transpiler
4. **測試補丁**：在遊戲中測試功能是否生效
5. **記錄補丁**：添加完整的 XML 註解

## 參考資源

- **Harmony 官方文件**：https://harmony.pardeike.net/
- **dnSpy 反編譯工具**：https://github.com/dnSpy/dnSpy
- **Cities: Skylines 2 Modding Discord**：官方社群

## 常見遊戲系統補丁目標

| 系統 | 類別名稱 | 方法 | 用途 |
|------|--------|------|------|
| 住宅需求 | `ResidentialDemandSystem` | `OnUpdate` | 修改住宅需求值 |
| 商業需求 | `CommercialDemandSystem` | `OnUpdate` | 修改商業需求值 |
| 工業需求 | `IndustrialDemandSystem` | `OnUpdate` | 修改工業需求值 |
| 電力系統 | `ElectricityFlowSystem` | `OnUpdate` | 修改電力供應 |
| 水系統 | `WaterFlowSystem` | `OnUpdate` | 修改水供應 |

