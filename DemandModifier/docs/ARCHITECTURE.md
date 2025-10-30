# DemandModifier - 架構設計文件

## 📋 目錄
1. [系統架構](#系統架構)
2. [專案結構](#專案結構)
3. [核心模組](#核心模組)
4. [Harmony 補丁系統](#harmony-補丁系統)
5. [多國語言系統](#多國語言系統)
6. [日誌系統](#日誌系統)

---

## 系統架構

### 高層架構圖

```
┌─────────────────────────────────────────────┐
│         DemandModifier 模組 (IMod)           │
│                  主入口點                    │
└────────────────┬────────────────────────────┘
                 │
     ┌───────────┼────────────────┬─────────────────┐
     │           │                │                 │
┌────▼───┐  ┌───▼──────┐  ┌─────▼────────┐  ┌────▼────────┐
│ 設定系統 │  │語言系統   │  │日誌系統      │  │ Harmony     │
│Manager │  │Manager   │  │Logger    │  │補丁系統     │
└────────┘  └──────────┘  └──────────┘  └─────────────┘
     │           │               │            │
     └───────────┴───────────────┴────────────┤
                                              │
                             ┌────────────────▼─────────────┐
                             │   補丁包 (Patches)          │
                             ├────────────────────────────┤
                             │ • DemandSystemPatch        │
                             │ • ServiceSystemPatch       │
                             │ • EconomySystemPatch       │
                             │ • PatchBase (基類)         │
                             └────────────────────────────┘
```

### 主要流程

```
遊戲啟動
   │
   ▼
DemandModifierMod.OnLoad()
   │
   ├─► Logger.Initialize()        [初始化日誌系統]
   │
   ├─► LocalizationInitializer    [初始化多語言]
   │    .Initialize()
   │
   ├─► DemandModifierSettings     [載入設定]
   │    .LoadSettings()
   │
   └─► Harmony.PatchAll()         [套用所有補丁]
        │
        ├─► ResidentialDemandSystemPatch.Prefix()
        ├─► CommercialDemandSystemPatch.Prefix()
        ├─► IndustrialDemandSystemPatch.Prefix()
        ├─► UnlimitedElectricityPatch.Prefix()
        └─► ... [其他補丁]

遊戲運行時（每幀）
   │
   ├─► 需求系統.OnUpdate()
   │    ▼
   │   [Harmony 攔截]
   │    ├─► Prefix() 檢查設定值
   │    ├─► 修改私有欄位
   │    └─► 返回 true，允許原方法執行
   │
   └─► UI 更新、遊戲邏輯等...

遊戲卸載
   │
   ▼
DemandModifierMod.OnDispose()
   ├─► Harmony.UnpatchAll()
   └─► Settings.Cleanup()
```

---

## 專案結構

```
DemandModifier/
│
├── DemandModifierMod.cs              # IMod 入口點、主引擎
├── DemandModifierSettings.cs         # ModSetting 設定類別
│
├── Code/
│   ├── Utils/
│   │   └── Logger.cs                # 進階日誌系統（8+ 日誌等級）
│   │
│   ├── Localization/
│   │   ├── LocalizationInitializer.cs # 語言系統初始化
│   │   ├── LocaleManager.cs          # 語言管理器（核心新增）
│   │   └── ModLocale.cs              # 語言本地化類別
│   │
│   ├── Patches/
│   │   ├── PatchBase.cs              # 補丁基類（核心新增）
│   │   ├── DemandSystemPatch.cs      # 需求系統補丁（改進版）
│   │   └── ServiceSystemPatch.cs     # 服務系統補丁
│   │
│   ├── Systems/
│   │   └── DemandSystemHelper.cs     # 輔助類別
│   │
│   └── Utils/
│       └── PatchUtils.cs             # 補丁工具函式
│
├── l10n/                             # 7 種語言翻譯
│   ├── en-US.json                   # 英文
│   ├── zh-HANS.json                 # 簡體中文
│   ├── zh-HANT.json                 # 繁體中文
│   ├── ja-JP.json                   # 日文
│   ├── de-DE.json                   # 德文
│   ├── es-ES.json                   # 西班牙文
│   └── fr-FR.json                   # 法文
│
├── Properties/
│   └── PublishConfiguration.xml      # 發佈設定
│
├── docs/                             # 文檔（組織結構新增）
│   ├── ARCHITECTURE.md              # 本檔案
│   ├── architecture/
│   │   └── ...
│   ├── guides/
│   │   ├── QUICK_START.md
│   │   └── IMPLEMENTATION_GUIDE.md
│   ├── setup/
│   │   └── BUILD_GUIDE.md
│   └── troubleshooting/
│       └── FIX_CHECKLIST.md
│
└── DemandModifier.csproj            # 專案檔
```

---

## 核心模組

### 1. DemandModifierMod.cs - 主引擎

**責務**：
- 管理模組生命週期 (OnLoad / OnDispose)
- 初始化所有系統（日誌、語言、設定、補丁）
- 提供全域存取的 `Settings` 靜態屬性

**關鍵方法**：
```csharp
public void OnLoad(UpdateSystem updateSystem)
{
    // 1. 初始化日誌系統
    // 2. 初始化多國語言
    // 3. 載入模組設定
    // 4. 套用 Harmony 補丁
}

public void OnDispose()
{
    // 1. 卸載 Harmony 補丁
    // 2. 清理設定
}
```

### 2. DemandModifierSettings.cs - 設定管理

**責務**：
- 定義所有遊戲內設定選項
- 管理分頁、群組、下拉選單
- 提供語言翻譯

**關鍵結構**：
```csharp
[FileLocation(nameof(DemandModifier))]
[SettingsUITabOrder("DemandControl", "ServiceControl", "EconomyControl")]
public class DemandModifierSettings : ModSetting
{
    // 需求控制 (Demand Tab)
    public DemandLevel ResidentialDemandLevel { get; set; }
    public DemandLevel CommercialDemandLevel { get; set; }
    public DemandLevel IndustrialDemandLevel { get; set; }
    
    // 服務控制 (Service Tab)
    public bool EnableUnlimitedElectricity { get; set; }
    // ... 其他服務選項
    
    // 經濟控制 (Economy Tab)
    public bool EnableUnlimitedMoney { get; set; }
    // ... 其他經濟選項
}
```

### 3. Logger.cs - 日誌系統（新增）

**責務**：
- 提供統一的日誌介面
- 支援 8+ 日誌等級
- 條件編譯、效能計時、進度追蹤

**日誌等級**：
1. **Trace**: 最詳細的追蹤 (條件編譯)
2. **Debug**: 除錯資訊
3. **Info**: 一般資訊
4. **Warn**: 警告訊息
5. **Error**: 錯誤
6. **Critical**: 嚴重錯誤

**特殊方法**：
```csharp
Logger.StartTimer("operation");
// ... 執行操作
Logger.StopTimer("operation", thresholdMs: 100);

Logger.MethodEnter("MethodName", arg1, arg2);
// ... 執行邏輯
Logger.MethodExit("MethodName", result);

Logger.PatchResult("PatchName", success: true);
Logger.CheckCondition("condition_name", result: true);
```

### 4. LocaleManager.cs - 語言管理系統（新增）

**責務**：
- 動態檢測和切換語言
- 提供語言親和性映射（語言降級）
- 快取翻譯字典
- 建立標準化的語系鍵值

**核心功能**：
```csharp
LocaleManager.Initialize();          // 初始化並偵測當前語言
LocaleManager.SetCurrentLocale("de-DE");  // 切換語言

string translation = LocaleManager.GetTranslation(localeKey, fallback);
string optionKey = LocaleManager.BuildOptionLocaleKey(...);
```

**支援語言**：
- en-US（英文 - 美國）
- de-DE（德文 - 德國）
- es-ES（西班牙文 - 西班牙）
- fr-FR（法文 - 法國）
- ja-JP（日文 - 日本）
- zh-HANS（簡體中文 - 中國）
- zh-HANT（繁體中文 - 台灣）

---

## Harmony 補丁系統

### 補丁架構

```
補丁基類層級
┌─────────────────────────────────────┐
│   DemandSystemPatchBase             │ 抽象基類
│   - PrePatchCheck()                 │ 前置檢查
│   - PostPatchCheck()                │ 後置檢查
│   - LogPatchException()             │ 例外日誌
└─────────────────────────────────────┘
           │
    ┌──────┴──────┬─────────────┐
    │             │             │
┌───▼──┐  ┌──────▼──────┐  ┌──▼─────────┐
│具體  │  │簡單需求補丁  │  │複雜補丁    │
│補丁  │  │SimpleDemand │  │可重用邏輯  │
│(各系│ │PatchBase<T> │  │           │
│統)  │  │             │  │           │
└────┘  └─────────────┘  └────────────┘
```

### 補丁實現模式

#### Prefix 補丁（本專案主要使用）
```csharp
[HarmonyPatch(typeof(ResidentialDemandSystem), "OnUpdate")]
public class ResidentialDemandSystemPatch
{
    static void Prefix(ResidentialDemandSystem __instance)
    {
        // 1. 檢查設定
        if (DemandModifierMod.Settings == null)
            return;
        
        // 2. 取得目標值
        int demandValue = (int)DemandModifierMod.Settings.ResidentialDemandLevel;
        
        // 3. 修改私有欄位 (使用 AccessTools)
        var fieldRef = AccessTools.FieldRefAccess<ResidentialDemandSystem, int>("m_BuildingDemand");
        fieldRef(__instance) = demandValue;
        
        // 4. 日誌記錄
        Logger.PatchResult("住宅需求補丁", true);
    }
}
```

### 補丁列表

| 補丁類別 | 目標系統 | 功能 |
|---------|---------|------|
| **ResidentialDemandSystemPatch** | ResidentialDemandSystem | 控制住宅需求 |
| **CommercialDemandSystemPatch** | CommercialDemandSystem | 控制商業需求 |
| **IndustrialDemandSystemPatch** | IndustrialDemandSystem | 控制工業需求 |
| **UnlimitedElectricityPatch** | ElectricityFlowSystem | 無限電力 |
| **UnlimitedWaterPatch** | WaterFlowSystem | 無限供水 |
| **UnlimitedSewagePatch** | SewageFlowSystem | 無限污水 |
| **UnlimitedGarbagePatch** | GarbageSystem | 無限垃圾 |
| **UnlimitedHealthcarePatch** | HealthcareSystem | 無限醫療 |
| **UnlimitedEducationPatch** | EducationSystem | 無限教育 |
| **UnlimitedPolicePatch** | PoliceDepartmentSystem | 無限警力 |
| **UnlimitedFirePatch** | FireDepartmentSystem | 無限消防 |

---

## 多國語言系統

### 語言檔案結構

每個 `l10n/*.json` 檔案包含所有語言鍵值：

```json
{
  // 分頁標題
  "Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.DemandControl]": "需求控制",
  
  // 群組標題
  "Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemand]": "住宅需求",
  
  // 選項標題
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "住宅需求級別",
  
  // 選項描述
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.ResidentialDemandLevel]": "控制住宅需求...",
  
  // Enum 值翻譯
  "Common.ENUM[DemandModifier.DemandModifier.DemandLevel.Maximum]": "最大 (100%)"
}
```

### 語言鍵值命名規則

- **分頁**: `Options.SECTION[<Namespace>.<ClassName>.<TabName>]`
- **群組**: `Options.GROUP[<Namespace>.<ClassName>.<GroupName>]`
- **選項**: `Options.OPTION[<Namespace>.<ClassName>.<PropertyName>]`
- **描述**: `Options.OPTION_DESCRIPTION[<Namespace>.<ClassName>.<PropertyName>]`
- **Enum**: `Common.ENUM[<Namespace>.<EnumName>.<ValueName>]`

---

## 日誌系統

### 日誌輸出位置

遊戲日誌位置：
```
%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log
```

### 日誌範例

```
[INFO] ▶ ════════════ DemandModifier 載入開始 ════════════
[DEBUG] 初始化進階日誌系統...
[INFO] ✓ 日誌系統已初始化
[DEBUG] 初始化多國語言系統...
[INFO] ✓ 語言系統已初始化
[INFO] 設定已載入 - 住宅: Maximum, 商業: Maximum, 工業: Maximum
[DEBUG] [檢驗點] ResidentialDemandSystem 補丁執行
[INFO] ✓ 住宅需求已修改為: 255
[INFO] ▶ ════════════ DemandModifier 載入完成 ════════════
```

---

## 資訊流動圖

```
使用者操作遊戲設定
        │
        ▼
DemandModifierSettings
    (ModelSetting)
        │
        ├─► 存儲到本地設定檔
        │   (%LocalAppData%\...\ModsSettings\DemandModifier.coc)
        │
        └─► DemandModifierMod.Settings
            (全域靜態屬性)
            │
            ├─► DemandSystemPatch.Prefix()
            │   └─► 讀取設定值
            │       └─► 修改需求值
            │
            └─► ServiceSystemPatch.Prefix()
                └─► 讀取設定值
                    └─► 修改服務供應

Logger 在各個階段記錄:
    │
    ├─► 初始化階段
    ├─► 補丁套用階段
    ├─► 執行時階段
    └─► 例外處理階段
```

---

## 性能考量

### 優化策略

1. **AccessTools 快取**: 補丁中使用 `AccessTools.FieldRefAccess` 快取欄位參考（而非每次調用反射）
2. **條件檢查**: 優先檢查是否啟用（避免不必要的修改）
3. **批次日誌**: 使用條件編譯避免 Debug 模式下的日誌開銷
4. **語言字典快取**: LocaleManager 快取已載入的語言字典

### 性能指標

預期性能影響（相對於遊戲基線）：
- 補丁調用開銷: ~0.1-0.5ms （每幀，取決於補丁數量）
- 語言查詢: ~0.05ms （已快取）
- 日誌記錄: ~0.1-1ms （取決於日誌等級和內容）

---

## 版本相容性

**支援遊戲版本**: Cities: Skylines 2 v1.2.* 及以上

**相容性檢查**:
- ✅ .NET Framework 4.8.1
- ✅ Harmony 2.x
- ✅ Unity ECS/DOTS
- ✅ Colossal Order 遊戲框架

---

**文件版本**: 2.0  
**最後更新**: 2025-10-30  
**作者**: 基於 Traffic 專案最佳實踐
