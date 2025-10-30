# Harmony 補丁完整實作指南

## 📚 目錄
1. [補丁基礎](#補丁基礎)
2. [補丁類型](#補丁類型)
3. [實作模式](#實作模式)
4. [進階技術](#進階技術)
5. [除錯補丁](#除錯補丁)

---

## 補丁基礎

### Harmony 是什麼？

Harmony 是一個 .NET 庫，允許在運行時修改方法的行為，無需修改原始程式碼。

### 為什麼用 Harmony？

Cities: Skylines 2 中，遊戲邏輯都是編譯的 DLL，無法直接修改。Harmony 提供：
- **Runtime Patching**: 在執行時修改方法
- **多個補丁點**: Prefix、Postfix、Transpiler
- **安全性**: 不會破壞遊戲完整性（可隨時卸載）

### Harmony 如何工作？

```
遊戲方法調用
    │
    ├─► [Prefix 補丁執行]  ◄─── 你的程式碼可以修改參數或跳過原方法
    │
    ├─► [原始方法執行]    ◄─── 遊戲邏輯（可能被跳過）
    │
    └─► [Postfix 補丁執行] ◄─── 你的程式碼可以修改返回值
```

---

## 補丁類型

### 1. Prefix 補丁（本專案主要使用）

**用途**：在原始方法執行前攔截

**特徵**：
- 可修改參數
- 可跳過原始方法（返回 false）
- 無法訪問返回值

**簽名**：
```csharp
static void Prefix(ClassName __instance, params...)
// 或
static bool Prefix(ClassName __instance, params...)  // false = 跳過原方法
```

**本專案範例**：
```csharp
[HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
public class ResidentialDemandSystemPatch
{
    static void Prefix(ResidentialDemandSystem __instance)
    {
        // 在 OnUpdate() 執行前修改需求值
        if (DemandModifierMod.Settings?.ResidentialDemandLevel != DemandLevel.Off)
        {
            var fieldRef = AccessTools.FieldRefAccess<ResidentialDemandSystem, int>("m_BuildingDemand");
            fieldRef(__instance) = (int)DemandModifierMod.Settings.ResidentialDemandLevel;
        }
    }
}
```

---

### 2. Postfix 補丁

**用途**：在原始方法執行後修改結果

**特徵**：
- 可訪問返回值
- 可修改返回值
- 無法跳過原始方法

**簽名**：
```csharp
static void Postfix(ClassName __instance, ref ReturnType __result)
```

**範例**（不在本專案中）：
```csharp
[HarmonyPatch(typeof(Economy), "GetBudget")]
public class EconomyBudgetPatch
{
    static void Postfix(ref int __result)
    {
        if (DemandModifierMod.Settings?.EnableUnlimitedMoney == true)
        {
            __result = int.MaxValue;  // 修改返回的預算值
        }
    }
}
```

---

### 3. Transpiler 補丁

**用途**：修改方法的 IL 程式碼

**特徵**：
- 最低層操作
- 最強大但也最複雜
- 容易因遊戲更新而失效

**當使用 Transpiler**：
- 無法用 Prefix/Postfix 實現時
- 需要插入複雜邏輯時

**範例**（進階，本專案未使用）：
```csharp
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    var codes = new List<CodeInstruction>(instructions);
    // 修改 IL 程式碼...
    return codes.AsEnumerable();
}
```

---

## 實作模式

### 標準補丁結構

```csharp
using HarmonyLib;
using DemandModifier.Utils;

namespace DemandModifier.Patches
{
    /// <summary>
    /// 補丁說明文件
    /// 目標：要攔截的系統類別
    /// 功能：簡單描述補丁的用途
    /// </summary>
    [HarmonyPatch(typeof(TargetSystem), "MethodName")]
    public class TargetSystemPatch
    {
        static void Prefix(TargetSystem __instance)
        {
            try
            {
                // 1. 檢查設定是否啟用
                if (DemandModifierMod.Settings == null)
                    return;
                
                // 2. 取得目標值
                int targetValue = GetTargetValue();
                
                // 3. 修改私有欄位
                ModifyTargetField(__instance, targetValue);
                
                // 4. 日誌記錄
                Logger.PatchResult("補丁名稱", true);
            }
            catch (Exception ex)
            {
                Logger.Error("補丁執行失敗: {0}", ex.Message);
                Logger.Exception(ex, "補丁名稱");
            }
        }
    }
}
```

### 特殊參數

Harmony 自動注入的參數（使用特殊名稱）：

| 參數名 | 類型 | 用途 | 示例 |
|--------|------|------|------|
| `__instance` | TargetClass | 目標物件實例 | `ResidentialDemandSystem` |
| `__result` | object | 返回值（Postfix）| `ref int __result` |
| `__state` | object | 在 Prefix-Postfix 間傳遞資料 | `object __state` |
| `__originalMethod` | MethodInfo | 原始方法資訊 | Reflection 用 |

---

## 進階技術

### AccessTools 反射工具

#### 快取欄位參考（推薦）

```csharp
// 一次性快取，之後高效使用
private static AccessTools.FieldRef<TargetType, FieldType> FieldRef =
    AccessTools.FieldRefAccess<TargetType, FieldType>("m_PrivateFieldName");

// 使用時
FieldRef(__instance) = newValue;  // 修改
var value = FieldRef(__instance);  // 讀取
```

#### 存取私有方法

```csharp
var method = AccessTools.Method(typeof(ClassName), "MethodName");
var result = method.Invoke(__instance, new object[] { arg1, arg2 });
```

#### 存取私有屬性

```csharp
var property = AccessTools.Property(typeof(ClassName), "PropertyName");
property.SetValue(__instance, value);
var value = property.GetValue(__instance);
```

---

### NativeValue 和 NativeArray

Cities: Skylines 2 使用 Unity ECS，許多欄位是執行緒安全的容器。

#### NativeValue（單一值）

```csharp
// 宣告
private NativeValue<int> m_Availability;

// 透過 Harmony 修改
var availRef = AccessTools.FieldRefAccess<SystemType, NativeValue<int>>("m_Availability");
availRef(__instance).value = 255;  // 注意 .value 屬性
```

#### NativeArray（陣列）

```csharp
// 宣告
private NativeArray<int> m_Factors;

// 透過 Harmony 修改
var arrRef = AccessTools.FieldRefAccess<SystemType, NativeArray<int>>("m_Factors");
NativeArray<int> array = arrRef(__instance);
array[0] = newValue;  // 修改陣列元素
```

---

### 多補丁協調

#### 補丁優先順序

```csharp
[HarmonyPatch(...)]
[HarmonyPriority(Priority.First)]  // 最先執行
public class PatchA { }

[HarmonyPatch(...)]
[HarmonyPriority(Priority.Last)]   // 最後執行
public class PatchB { }

// 自訂優先順序（預設 400）
[HarmonyPriority(800)]
public class PatchC { }
```

#### 多方法補丁

```csharp
[HarmonyPatch]
public class MultiMethodPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(System1), "OnUpdate");
        yield return AccessTools.Method(typeof(System2), "OnUpdate");
    }

    static void Prefix(object __instance)
    {
        // 對所有目標方法套用相同邏輯
    }
}
```

---

## 除錯補丁

### 補丁驗證

使用 Harmony 提供的檢查方法：

```csharp
// 驗證補丁是否成功套用
var harmony = new Harmony("DemandModifier.Test");
var patches = harmony.GetPatchedMethods();

foreach (var method in patches)
{
    Logger.Info("已補丁: {0}", method.FullDescription());
}
```

### 日誌記錄

本專案提供完整的日誌系統：

```csharp
// 補丁執行流程
Logger.Checkpoint("補丁開始執行");
Logger.Debug("修改欄位: {0}", fieldName);
Logger.PatchResult("補丁名稱", success: true);

// 異常處理
Logger.Exception(ex, "補丁名稱");
Logger.Error("補丁失敗: {0}", ex.Message);
```

### 常見錯誤

| 錯誤 | 原因 | 解決方案 |
|------|------|---------|
| `NullReferenceException` | 補丁方法簽名錯誤 | 檢查參數名稱和型別 |
| 無法找到方法 | 目標方法名稱錯誤 | 使用 dnSpy 驗證方法名 |
| 補丁未執行 | 沒有正確註冊 | 確保有 `[HarmonyPatch]` 屬性 |
| 性能問題 | 補丁邏輯過重 | 優化或使用條件檢查 |

---

## 本專案的補丁清單

### 需求系統補丁

```
ResidentialDemandSystemPatch
├─ 目標: ResidentialDemandSystem.OnUpdate()
├─ 修改: m_BuildingDemand (int)
└─ 功能: 控制住宅需求

CommercialDemandSystemPatch
├─ 目標: CommercialDemandSystem.OnUpdate()
├─ 修改: m_BuildingDemand (int)
└─ 功能: 控制商業需求

IndustrialDemandSystemPatch
├─ 目標: IndustrialDemandSystem.OnUpdate()
├─ 修改: m_BuildingDemand (int)
└─ 功能: 控制工業需求
```

### 服務系統補丁

```
UnlimitedElectricityPatch
├─ 目標: ElectricityFlowSystem.OnUpdate()
└─ 功能: 無限電力

UnlimitedWaterPatch
├─ 目標: WaterFlowSystem.OnUpdate()
└─ 功能: 無限供水

... 等共 8 個服務系統補丁
```

---

## 最佳實踐

### Do's ✅

- ✅ 使用 `AccessTools.FieldRefAccess` 快取欄位參考
- ✅ 在 Prefix 開始時檢查是否啟用
- ✅ 使用 Logger 記錄補丁執行
- ✅ 提供詳細的 XML 註解
- ✅ 在 try-catch 中包裝補丁邏輯
- ✅ 返回 true 允許原方法繼續執行

### Don'ts ❌

- ❌ 每次都使用 Reflection 獲取欄位（低效）
- ❌ 忽略異常（無法除錯）
- ❌ 無條件修改所有值
- ❌ 修改不必要的欄位
- ❌ 使用 null-conditional 操作符在條件中（.NET 4.7.2 bug）
- ❌ 返回 false 除非你想跳過原方法

---

**版本**: 1.0  
**最後更新**: 2025-10-30
