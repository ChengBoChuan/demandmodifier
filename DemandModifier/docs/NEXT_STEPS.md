# 立即後續步驟指南

## 🎯 優先級排序

### 🔴 優先級 1：驗證與修正（今日完成）

#### Step 1: 編譯驗證
**目標**: 確保代碼編譯成功

```powershell
cd e:\Code\CSL2\DemandModifier\DemandModifier
dotnet clean
dotnet build -c Release
```

**預期結果**:
- ✅ 成功：無編譯錯誤，生成 DLL
- ⚠️ 失敗：ServiceSystemPatch.cs 中的系統類型不存在

**若失敗，執行 Step 2**

---

#### Step 2: 反編譯驗證（如需要）
**目標**: 確認遊戲中的確切系統類名

**使用 dnSpy 反編譯**:
1. 安裝 dnSpy: https://github.com/dnSpy/dnSpy/releases
2. 開啟文件: `[Steam]\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll`
3. 搜尋確切的系統類名:
   - 搜尋 "WaterFlowSystem" → 確認是否存在及命名空間
   - 搜尋 "HealthcareSystem" → 確認名稱和命名空間
   - 搜尋 "ElectricityFlowSystem" 等

**需驗證的系統類名**:
```csharp
// ServiceSystemPatch.cs 中的類名
Game.Simulation.WaterFlowSystem              ← 確認
Game.Simulation.HealthcareSystem             ← 確認
Game.Simulation.EducationSystem              ← 確認
Game.Simulation.PoliceDepartmentSystem       ← 確認
Game.Simulation.FireDepartmentSystem         ← 確認
Game.Simulation.GarbageSystem                ← 確認
Game.Simulation.ElectricityFlowSystem        ← 確認
Game.Simulation.WasteWaterSystem             ← 確認
```

**修正 ServiceSystemPatch.cs**:
若系統類名不正確，使用以下範例修正：
```csharp
// 若 WaterFlowSystem 實際名稱是 WaterSystemV2
[HarmonyPatch(typeof(WaterSystemV2), "OnUpdate")]
public class UnlimitedWaterPatch
{
    // 維持相同邏輯
}
```

---

#### Step 3: 補丁系統欄位驗證（如需要）
**目標**: 確認 NativeValue 欄位名稱

**在 dnSpy 中搜尋**:
1. 開啟 WaterFlowSystem 類
2. 查看私有欄位（黑色方塊圖示）
3. 尋找類似 `m_Availability` 或 `m_FlowLevel` 的欄位
4. 確認欄位類型（應為 `NativeValue<int>`）

**更新 ServiceSystemPatch.cs** 欄位引用：
```csharp
// 若欄位名稱不同
private static AccessTools.FieldRef<WaterFlowSystem, NativeValue<int>> AvailabilityRef =
    AccessTools.FieldRefAccess<WaterFlowSystem, NativeValue<int>>("m_ActualFieldName");
```

---

### 🟡 優先級 2：功能擴展（1-2 天）

#### Step 4: 補丁測試
**目標**: 驗證補丁在遊戲中的實際效果

```powershell
# 部署到遊戲
.\test-deploy.ps1

# 檢查部署結果
$modPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
Get-ChildItem $modPath -Recurse
```

**在遊戲中測試**:
1. 啟動 Cities: Skylines 2
2. 開啟任何遊戲存檔
3. 進入 Mod 設定
4. 啟用各個功能
5. 觀察遊戲內的效果：
   - [ ] 住宅需求立即滿
   - [ ] 商業需求立即滿
   - [ ] 工業需求立即滿
   - [ ] 電力充足
   - [ ] 供水充足
   - [ ] 污水處理正常

**查看日誌**:
```powershell
$logPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log"
Get-Content $logPath -Wait -Tail 100 | Select-String -Pattern "DemandModifier|PATCH_RESULT|ERROR"
```

---

#### Step 5: 經濟補丁實現
**目標**: 實現經濟系統補丁

**建立 EconomySystemPatch.cs**:

```csharp
using HarmonyLib;
using Game.Simulation;
using Unity.Collections;

namespace DemandModifier.Code.Patches
{
    /// <summary>
    /// 經濟系統補丁 - 實現免費建造、無限金錢等功能
    /// </summary>
    
    [HarmonyPatch]
    public class UnlimitedMoneyPatch
    {
        // 使用 dnSpy 查詢 EconomySystem 的金錢欄位
        private static AccessTools.FieldRef<EconomySystem, NativeValue<long>> MoneyRef =
            AccessTools.FieldRefAccess<EconomySystem, NativeValue<long>>("m_Money");

        [HarmonyPatch(typeof(EconomySystem), "OnUpdate")]
        static void Prefix(EconomySystem __instance)
        {
            Logger.Debug("UnlimitedMoneyPatch", "Checking unlimited money setting...");
            
            try
            {
                if (DemandModifierMod.Settings != null && 
                    DemandModifierMod.Settings.EnableUnlimitedMoney == true)
                {
                    MoneyRef(__instance).value = long.MaxValue;
                    Logger.PatchResult("UnlimitedMoney", true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UnlimitedMoneyPatch", $"Failed to patch money: {ex.Message}");
            }
        }
    }

    [HarmonyPatch]
    public class FreeConstructionPatch
    {
        // 查詢建造成本計算方法
        private static AccessTools.Method CalculateCostMethod =
            AccessTools.Method(typeof(EconomySystem), "CalculateBuildingCost");

        [HarmonyPatch(typeof(EconomySystem), "CalculateBuildingCost")]
        static void Postfix(ref long __result)
        {
            Logger.Debug("FreeConstructionPatch", "Checking free construction...");
            
            try
            {
                if (DemandModifierMod.Settings != null && 
                    DemandModifierMod.Settings.EnableFreeConstruction == true)
                {
                    __result = 0;
                    Logger.PatchResult("FreeConstruction", true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("FreeConstructionPatch", $"Failed to patch construction cost: {ex.Message}");
            }
        }
    }

    [HarmonyPatch]
    public class ZeroMaintenancePatch
    {
        private static AccessTools.FieldRef<MaintenanceSystem, NativeValue<long>> CostRef =
            AccessTools.FieldRefAccess<MaintenanceSystem, NativeValue<long>>("m_MaintenanceCost");

        [HarmonyPatch(typeof(MaintenanceSystem), "OnUpdate")]
        static void Prefix(MaintenanceSystem __instance)
        {
            Logger.Debug("ZeroMaintenancePatch", "Checking zero maintenance...");
            
            try
            {
                if (DemandModifierMod.Settings != null && 
                    DemandModifierMod.Settings.EnableZeroMaintenance == true)
                {
                    CostRef(__instance).value = 0;
                    Logger.PatchResult("ZeroMaintenance", true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ZeroMaintenancePatch", $"Failed to patch maintenance: {ex.Message}");
            }
        }
    }
}
```

**更新 DemandModifierSettings.cs**:
在經濟控制分頁添加 3 個選項

---

#### Step 6: 多語言完整化
**目標**: 所有 7 種語言都有完整翻譯

**檢查清單**:
- [ ] en-US.json - 英文
- [ ] de-DE.json - 德文
- [ ] es-ES.json - 西班牙文
- [ ] fr-FR.json - 法文
- [ ] ja-JP.json - 日文
- [ ] zh-HANS.json - 簡體中文
- [ ] zh-HANT.json - 繁體中文

**提示**: 使用線上翻譯工具（DeepL, Google Translate），然後人工審核

---

### 🟢 優先級 3：優化與部署（可選）

#### Step 7: 性能優化
**目標**: 確保補丁不會影響遊戲性能

**檢查項目**:
- [ ] 使用 `StartTimer()` / `StopTimer()` 測量補丁耗時
- [ ] 確保補丁執行時間 < 1ms
- [ ] 檢查記憶體使用

**優化建議**:
```csharp
// 使用 lazy 初始化避免重複反射
private static bool _isSettingsInitialized = false;
private static DemandLevel _cachedLevel = DemandLevel.Off;

static void Prefix(ResidentialDemandSystem __instance)
{
    // 只在首次或設定變更時更新快取
    if (!_isSettingsInitialized || SettingsChanged())
    {
        _cachedLevel = DemandModifierMod.Settings.ResidentialDemandLevel;
        _isSettingsInitialized = true;
    }
    
    if (_cachedLevel != DemandLevel.Off)
    {
        BuildingDemandRef(__instance).value = (int)_cachedLevel;
    }
}
```

---

#### Step 8: 版本發佈
**目標**: 發佈到 PDX Mods

```powershell
# 更新版本號
# 編輯 DemandModifier.csproj 的 <Version> 標籤

# 更新發佈配置
# 編輯 Properties/PublishConfiguration.xml

# 發佈新版本
dotnet publish /p:PublishProfile=PublishNewVersion
```

---

## 📋 檢查清單

### 編譯階段
- [ ] `dotnet clean` 成功
- [ ] `dotnet build -c Release` 成功
- [ ] 無編譯警告
- [ ] DLL 大小合理 (< 500KB)

### 驗證階段
- [ ] dnSpy 反編譯驗證
- [ ] 系統類名正確
- [ ] 欄位名稱正確
- [ ] 命名空間正確

### 部署階段
- [ ] DLL 複製到 Mods 資料夾
- [ ] 語言檔案複製完整
- [ ] test-deploy.ps1 成功

### 測試階段
- [ ] 遊戲啟動無崩潰
- [ ] Mod 在列表中顯示
- [ ] UI 設定能打開
- [ ] 功能生效
- [ ] 日誌輸出正常

### 發佈階段
- [ ] 版本號更新
- [ ] Changelog 更新
- [ ] README 更新
- [ ] 發佈成功

---

## 🆘 故障排除

### 常見問題 1: 編譯失敗 - "WaterFlowSystem 不存在"
**解決方案**:
1. 使用 dnSpy 找到正確的類名
2. 檢查命名空間（可能是 `Game.Simulation` 或其他）
3. 更新 ServiceSystemPatch.cs

### 常見問題 2: 補丁不生效
**解決方案**:
1. 查看 Player.log 中是否有 Harmony 錯誤
2. 確認欄位名稱正確（使用 dnSpy）
3. 檢查設定是否啟用
4. 添加日誌檢查點

### 常見問題 3: 遊戲崩潰
**解決方案**:
1. 查看 Player.log 末尾的錯誤
2. 注釋掉 ServiceSystemPatch 中的某個補丁
3. 逐一測試找出問題補丁
4. 檢查 NativeValue 使用方式

---

## 📊 進度追蹤

```
整體進度
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✅ 核心代碼            ████████████████░░ 90%
✅ 文檔系統            ████████████████░░ 95%
⏳ 編譯驗證            ░░░░░░░░░░░░░░░░░░ 0%
⏳ 補丁測試            ░░░░░░░░░░░░░░░░░░ 0%
⏳ 經濟補丁            ░░░░░░░░░░░░░░░░░░ 0%
⏳ 多語言翻譯          ░░░░░░░░░░░░░░░░░░ 0%
⏳ 版本發佈            ░░░░░░░░░░░░░░░░░░ 0%

總進度                 ████████░░░░░░░░░░ 52%
```

---

## 📞 技術支援資源

### 文檔參考
1. `docs/ARCHITECTURE.md` - 系統設計
2. `docs/guides/QUICK_START.md` - 快速入門
3. `docs/guides/PATCH_GUIDE.md` - 補丁開發
4. `docs/troubleshooting/FIX_CHECKLIST.md` - 故障排查

### 工具
- **dnSpy** - 反編譯遊戲 DLL
- **Visual Studio Code** - 代碼編輯
- **PowerShell** - 自動化工具

### 外部資源
- Harmony 文檔: https://harmony.pardeike.net/
- Cities: Skylines 2 Modding: https://cs2.paradoxwikis.com/
- Unity ECS 文檔: https://docs.unity3d.com/Packages/com.unity.entities@latest

---

**版本**: 1.0  
**最後更新**: 2025-10-30  
**預估完成時間**: 3-5 小時  
**負責人**: 開發團隊
