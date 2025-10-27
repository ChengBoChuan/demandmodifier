# DemandModifier - Cities: Skylines 2 Mod 開發指南

## 專案概述

這是一個 Cities: Skylines 2 遊戲模組，使用 **Harmony 函式庫**攔截並修改遊戲需求系統。核心架構：
- **DemandModifierMod.cs**: IMod 實作，處理模組生命週期和 Harmony 補丁註冊
- **DemandModifierSettings.cs**: ModSetting 子類別，定義遊戲內設定 UI
- **DemandSystemPatch.cs**: 三個 Harmony 補丁類別（住宅/商業/工業），使用 Prefix 攔截在原始方法執行前修改私有欄位

## 遊戲架構：Unity ECS/DOTS 系統

### Unity DOTS (Data-Oriented Technology Stack)
Cities: Skylines 2 使用 Unity 的 ECS (Entity Component System) 架構，關鍵概念：

1. **System 類別**：遊戲邏輯執行單位
   - `CommercialDemandSystem`, `IndustrialDemandSystem`, `ResidentialDemandSystem` 都是 `SystemBase` 子類別
   - 每幀執行 `OnUpdate()` 方法處理需求計算
   - 使用 `NativeValue<T>` 和 `NativeArray<T>` 作為執行緒安全的資料容器

2. **Job System**：多執行緒平行處理
   - System 類別內部使用 Jobs 進行運算
   - `NativeValue` 和 `NativeArray` 可在 Job 間共享資料
   - Harmony 補丁在主執行緒執行，攔截 `OnUpdate` 可在 Job 執行前修改資料

3. **私有欄位存取**：
   - ECS System 的狀態儲存在私有欄位中（如 `m_BuildingDemand`）
   - 無公開 API 修改這些值 → 必須使用 Harmony 反射存取

### 為何攔截 OnUpdate
```csharp
[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
```
- `OnUpdate` 在每次系統更新時執行（每幀或定期）
- Prefix 補丁在原始邏輯前執行，可預先設定需求值
- 原始邏輯仍會執行，但我們的值會覆蓋其計算結果

## 關鍵技術模式

### Harmony 補丁完整指南

#### 1. Prefix 補丁（本專案使用）
在原始方法執行**前**執行，可修改輸入參數或跳過原方法：
```csharp
[HarmonyPatch(typeof(CommercialDemandSystem), "OnUpdate")]
public class CommercialDemandSystemPatch
{
    private static AccessTools.FieldRef<CommercialDemandSystem, NativeValue<int>> BuildingDemandRef =
        AccessTools.FieldRefAccess<CommercialDemandSystem, NativeValue<int>>("m_BuildingDemand");

    static void Prefix(CommercialDemandSystem __instance)
    {
        if (DemandModifierMod.Settings != null && DemandModifierMod.Settings.EnableCommercialDemand == true)
        {
            BuildingDemandRef(__instance).value = 255;
        }
    }
}
```
- `__instance` 是特殊參數名，Harmony 自動注入目標物件實例
- 返回 `bool` 可控制是否執行原方法（`false` = 跳過）
- 本專案使用 `void`，讓原方法繼續執行（保持遊戲其他邏輯正常）

#### 2. Postfix 補丁（未使用，但可擴充）
在原始方法執行**後**執行，可修改返回值：
```csharp
static void Postfix(ref int __result)
{
    __result = 255; // 修改返回值
}
```

#### 3. Transpiler 補丁（進階，未使用）
修改方法的 IL 程式碼，用於精細控制執行流程，但複雜且易碎。

#### 4. AccessTools 反射工具
```csharp
// 存取私有欄位（本專案模式）
AccessTools.FieldRefAccess<ClassType, FieldType>("m_FieldName");

// 存取私有方法
AccessTools.Method(typeof(ClassName), "MethodName");

// 存取私有屬性
AccessTools.Property(typeof(ClassName), "PropertyName");
```

#### 5. 條件檢查最佳實踐
```csharp
// ✅ 正確：先檢查 Settings 非 null
if (DemandModifierMod.Settings != null && DemandModifierMod.Settings.EnableXXX == true)

// ❌ 錯誤：可能 NullReferenceException (.NET 4.8.1 限制)
if (DemandModifierMod.Settings?.EnableXXX == true)
```

### 設定系統架構

#### ModSetting 繼承結構
```csharp
[FileLocation(nameof(DemandModifier))] // 設定檔儲存位置
[SettingsUITabOrder("需求控制", "服務控制", "經濟控制")] // 分頁順序
[SettingsUIGroupOrder("住宅需求", "商業需求", ...)] // 群組順序
[SettingsUIShowGroupName(...)] // 顯示群組標題
public class DemandModifierSettings : ModSetting
```

#### 屬性標記模式
```csharp
[SettingsUISection("分頁名稱", "群組名稱")]
public bool EnableFeature { get; set; }
```
- 分頁名稱必須在 `SettingsUITabOrder` 中宣告
- 群組名稱必須在 `SettingsUIGroupOrder` 中宣告
- 所有屬性名稱會自動對應到語系檔案的翻譯鍵值

#### 設定生命週期
1. **OnLoad 載入**：
```csharp
Settings = new DemandModifierSettings(this);
Settings.RegisterInOptionsUI(); // 註冊到遊戲 UI
AssetDatabase.global.LoadSettings(nameof(DemandModifier), Settings, new DemandModifierSettings(this));
```

2. **全域存取**：
```csharp
DemandModifierMod.Settings.EnableResidentialDemand // 靜態屬性，任何地方可存取
```

3. **自動儲存**：設定變更時遊戲自動儲存到 `%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\ModsSettings\DemandModifier.coc`

4. **OnDispose 清理**：
```csharp
Settings.UnregisterInOptionsUI();
Settings = null;
```

### .NET Framework 4.8.1 語法限制與轉換

#### 禁止使用的現代 C# 語法

| 現代語法 (.NET 8) | .NET 4.8.1 替代方案 | 原因 |
|-------------------|---------------------|------|
| `[item1, item2]` | `new Type[] { item1, item2 }` | 集合表達式不支援 |
| `$"{nameof(X)}.{nameof(Y)}"` | `string.Format("{0}.{1}", nameof(X), nameof(Y))` | 編譯時字串插值問題 |
| `Settings?.Property` | `Settings != null && Settings.Property` | null-conditional 在某些情境失效 |
| `record class` | `class` with manual equality | Records 不支援 |
| `init` accessor | `set` accessor | init-only 屬性不支援 |
| File-scoped namespace | Block-scoped namespace | C# 10 特性 |
| `required` modifier | Constructor validation | C# 11 特性 |

#### 實際轉換範例

**❌ 不相容程式碼：**
```csharp
// 集合表達式
var factors = [DemandFactor.Taxes, DemandFactor.Students];

// 字串插值與 nameof
log.Info($"{nameof(DemandModifier)}.{nameof(DemandModifierMod)}");

// null-conditional 在條件中
if (Settings?.EnableFeature == true) { }
```

**✅ 相容程式碼：**
```csharp
// 明確陣列初始化
private static DemandFactor[] Factors = new DemandFactor[]
{
    DemandFactor.Taxes,
    DemandFactor.Students,
};

// string.Format
public static ILog log = LogManager.GetLogger(
    string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod))
).SetShowsErrorsInUI(false);

// 顯式 null 檢查
if (DemandModifierMod.Settings != null && DemandModifierMod.Settings.EnableFeature == true)
{
    // 安全存取
}
```

## 多國語系系統

### 檔案結構與命名規則
```
l10n/
├── en-US.json      # 英文（美國）
├── zh-HANT.json    # 繁體中文（台灣）
├── zh-HANS.json    # 简体中文（中國）
├── ja-JP.json      # 日文
├── de-DE.json      # 德文
├── es-ES.json      # 西班牙文
└── fr-FR.json      # 法文
```

### 語系鍵值完整格式

#### 1. 分頁標題（Tab）
```json
"Options.SECTION[DemandModifier.DemandModifier.DemandModifierSettings.需求控制]": "Demand Control"
```
- `Options.SECTION` = 固定前綴
- `[完整命名空間.類別名稱.分頁名稱]` = 唯一識別符
- **注意**：分頁名稱來自 `SettingsUISection` 的第一個參數

#### 2. 群組名稱（Group）
```json
"Options.GROUP[DemandModifier.DemandModifier.DemandModifierSettings.住宅需求]": "Residential Demand"
```
- `Options.GROUP` = 固定前綴
- 群組名稱來自 `SettingsUISection` 的第二個參數

#### 3. 選項標題（Option）
```json
"Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableResidentialDemand]": "Enable Max Residential Demand"
```
- `Options.OPTION` = 固定前綴
- 使用屬性的實際名稱（EnableResidentialDemand）

#### 4. 選項描述（Description）
```json
"Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableResidentialDemand]": "Always keep residential demand at maximum for all density types"
```
- `Options.OPTION_DESCRIPTION` = 固定前綴
- 提供詳細說明，顯示在設定項目下方

### 語系載入機制
遊戲引擎自動處理，無需額外程式碼：
1. 掃描模組根目錄的 `l10n/` 資料夾
2. 根據玩家的遊戲語言設定載入對應 JSON
3. 自動套用翻譯到 UI 元件
4. 若找不到對應語言，降級到 `en-US.json`

### 新增翻譯的步驟
1. 在 `DemandModifierSettings.cs` 加入新屬性
2. 在**全部 7 個** `l10n/*.json` 檔案加入 4 個鍵值（若有新分頁/群組）：
   - `SECTION` (如需新分頁)
   - `GROUP` (如需新群組)
   - `OPTION`
   - `OPTION_DESCRIPTION`
3. 使用線上翻譯工具確保品質（DeepL, Google Translate）
4. 建置並在遊戲中切換語言測試

## 建置與發佈流程

### 環境設定

#### 必要環境變數
```powershell
# 設定 Cities: Skylines 2 Modding SDK 路徑
[System.Environment]::SetEnvironmentVariable('CSII_TOOLPATH', 'C:\Path\To\CS2ModdingSDK', 'User')
```

SDK 提供的檔案：
- **Mod.props**: 遊戲 DLL 參考、編譯設定
- **Mod.targets**: 發佈邏輯、PDX Mods 整合

驗證設定：
```powershell
$env:CSII_TOOLPATH # 應顯示 SDK 路徑
```

### 建置命令

```powershell
# 清理舊建置產物
dotnet clean

# Debug 建置（開發用）
dotnet build -c Debug

# Release 建置（發佈用）
dotnet build -c Release
```

建置產物位置：
- Debug: `bin/Debug/net48/DemandModifier.dll`
- Release: `bin/Release/net48/DemandModifier.dll`
- 語系檔案自動複製到 `bin/[Debug|Release]/net48/l10n/`

### 發佈到 PDX Mods

#### 1. PublishNewMod.pubxml（首次發佈）
```powershell
dotnet publish /p:PublishProfile=PublishNewMod
```
- 建立新模組，獲得 ModId
- 發佈後更新 `PublishConfiguration.xml` 的 `<ModId>` 欄位

#### 2. PublishNewVersion.pubxml（更新版本）
```powershell
dotnet publish /p:PublishProfile=PublishNewVersion
```
- 發佈現有模組的新版本
- 需要先設定正確的 ModId
- 更新 `<ModVersion>` 和 `<ChangeLog>`

#### 3. UpdatePublishedConfiguration.pubxml（更新 Metadata）
```powershell
dotnet publish /p:PublishProfile=UpdatePublishedConfiguration
```
- 僅更新描述、截圖、標籤等資訊
- 不重新上傳 DLL

#### PublishConfiguration.xml 關鍵欄位
```xml
<Publish>
    <ModId Value="123136" />                    <!-- PDX Mods 模組 ID -->
    <DisplayName Value="Demand Modifier" />     <!-- 顯示名稱 -->
    <ModVersion Value="0.0.1" />                <!-- 版本號（語意化版本） -->
    <GameVersion Value="1.2.*" />               <!-- 相容遊戲版本 -->
    <ShortDescription Value="..." />            <!-- 簡短描述（1-2 句） -->
    <LongDescription>...</LongDescription>      <!-- 詳細描述（支援 Markdown） -->
    <Thumbnail Value="Properties/Thumbnail.png" /> <!-- 縮圖（256x256） -->
    <ChangeLog Value="..." />                   <!-- 更新日誌 -->
</Publish>
```

### 本機測試部署

手動複製到遊戲 Mods 資料夾：
```powershell
# 遊戲 Mods 路徑
$modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods"

# 複製 DLL 和語系檔案
Copy-Item "bin\Release\net48\DemandModifier.dll" "$modsPath\DemandModifier\"
Copy-Item "bin\Release\net48\l10n" "$modsPath\DemandModifier\" -Recurse -Force
```

## 專案結構慣例

### 重要目錄與檔案

```
DemandModifier/
├── DemandModifier.csproj           # 專案檔（參考、建置設定）
├── DemandModifierMod.cs            # IMod 入口點
├── DemandModifierSettings.cs       # ModSetting 設定類別
├── DemandSystemPatch.cs            # Harmony 補丁實作
├── l10n/                           # 多國語系資料夾（7 種語言）
├── Properties/
│   ├── PublishConfiguration.xml    # PDX Mods Metadata
│   └── PublishProfiles/
│       ├── PublishNewMod.pubxml           # 首次發佈
│       ├── PublishNewVersion.pubxml       # 版本更新
│       └── UpdatePublishedConfiguration.pubxml # Metadata 更新
└── bin/
    └── [Debug|Release]/net48/
        ├── DemandModifier.dll      # 編譯產物
        └── l10n/                   # 自動複製的語系檔案
```

### csproj 關鍵設定

#### 1. SDK 整合
```xml
<Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.props" />
<Import Project="$([System.Environment]::GetEnvironmentVariable('CSII_TOOLPATH', 'EnvironmentVariableTarget.User'))\Mod.targets" />
```

#### 2. 遊戲 DLL 參考
```xml
<Reference Include="Game">
    <Private>false</Private>  <!-- 不複製到輸出目錄 -->
</Reference>
```
- **為何 `Private=false`**：遊戲 DLL 已存在於遊戲目錄，複製會造成版本衝突

#### 3. 語系檔案自動複製
```xml
<ItemGroup>
    <None Include="l10n\**\*.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```
- `**` = 遞迴所有子目錄
- `PreserveNewest` = 僅在檔案較新時複製

## 除錯與測試策略

### 日誌記錄最佳實踐

#### 建立 Logger
```csharp
using Colossal.Logging;

public static ILog log = LogManager.GetLogger(
    string.Format("{0}.{1}", nameof(DemandModifier), nameof(DemandModifierMod))
).SetShowsErrorsInUI(false);
```

**SetShowsErrorsInUI(false) 的重要性**：
- `true`：錯誤會跳出遊戲內通知，干擾玩家體驗
- `false`：錯誤僅寫入日誌檔，適合正式版本

#### 日誌等級使用指南
```csharp
log.Debug("詳細除錯資訊");           // 開發階段
log.Info("一般資訊");                // 重要事件（模組載入、設定變更）
log.Warn("警告訊息");                // 非致命問題
log.Error("錯誤訊息");               // 執行失敗但不崩潰
log.Critical("嚴重錯誤");            // 致命錯誤
```

#### 查看日誌
日誌檔位置：
```
%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\
├── Player.log          # 主日誌
└── Player-prev.log     # 上次啟動的日誌
```

即時監控（PowerShell）：
```powershell
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Wait -Tail 50
```

### 在遊戲中測試模組

#### 1. 安裝測試版本
```powershell
# 建置 Release 版本
dotnet build -c Release

# 複製到遊戲 Mods 目錄
$modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
New-Item -ItemType Directory -Force -Path $modsPath
Copy-Item "bin\Release\net48\*" $modsPath -Recurse -Force
```

#### 2. 啟用模組
1. 啟動 Cities: Skylines 2
2. 主選單 → **Mods**
3. 找到 "Demand Modifier" 並啟用
4. 重新啟動遊戲（某些模組需要）

#### 3. 驗證功能
- **設定 UI**：選項 → Mods → Demand Modifier
  - 檢查所有分頁是否顯示
  - 切換語言測試翻譯
  - 開關各項功能
  
- **需求系統**：進入遊戲後
  1. 開啟需求面板（UI 下方的需求指示器）
  2. 啟用「住宅需求」設定
  3. 觀察住宅需求是否立即跳至最大值
  4. 重複測試商業和工業需求

#### 4. 測試語系切換
```
遊戲選項 → Language → 切換語言 → 重新啟動遊戲
檢查 Demand Modifier 設定介面是否正確顯示翻譯
```

#### 5. 常見問題排查

| 問題 | 可能原因 | 解決方案 |
|------|----------|----------|
| 模組未出現在列表中 | DLL 路徑錯誤 | 確認複製到 `Mods\DemandModifier\DemandModifier.dll` |
| 設定介面空白 | 語系檔案未複製 | 檢查 `Mods\DemandModifier\l10n\` 是否存在 |
| 需求未改變 | Harmony 補丁失敗 | 查看日誌中的 Harmony 錯誤訊息 |
| 遊戲崩潰 | 欄位名稱錯誤 | 檢查 `AccessTools.FieldRefAccess` 的欄位名 |

### 自動化測試腳本（開發用）

建立 `test-deploy.ps1`：
```powershell
# 快速建置並部署到遊戲目錄
param([switch]$Clean)

if ($Clean) { dotnet clean }

dotnet build -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
Remove-Item $modsPath -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item "bin\Release\net48\*" $modsPath -Recurse -Force

Write-Host "✓ 已部署到 $modsPath" -ForegroundColor Green
Write-Host "請重新啟動 Cities: Skylines 2" -ForegroundColor Yellow
```

使用：
```powershell
.\test-deploy.ps1          # 快速部署
.\test-deploy.ps1 -Clean   # 清理後重新建置部署
```

## 新增功能完整流程

### 範例：新增「無限電力」功能

#### 步驟 1：加入設定選項
`DemandModifierSettings.cs`：
```csharp
[SettingsUISection("服務控制", "服務設定")]
public bool EnableUnlimitedElectricity { get; set; }
```

#### 步驟 2：更新所有語系檔案
需更新 7 個檔案：`l10n/en-US.json`, `l10n/zh-HANT.json` 等。

`l10n/en-US.json`：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Unlimited Electricity",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "Never run out of electricity - all buildings receive power"
}
```

`l10n/zh-HANT.json`：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "無限電力",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedElectricity]": "永不斷電 - 所有建築都能獲得電力供應"
}
```

#### 步驟 3：建立 Harmony 補丁
`DemandSystemPatch.cs` 新增：
```csharp
using Game.Simulation;
using HarmonyLib;

[HarmonyPatch(typeof(ElectricityFlowSystem), "OnUpdate")]
public class ElectricityFlowSystemPatch
{
    // 1. 找到需要修改的私有欄位
    private static AccessTools.FieldRef<ElectricityFlowSystem, int> AvailabilityRef =
        AccessTools.FieldRefAccess<ElectricityFlowSystem, int>("m_Availability");

    static void Prefix(ElectricityFlowSystem __instance)
    {
        // 2. 檢查設定是否啟用
        if (DemandModifierMod.Settings != null && 
            DemandModifierMod.Settings.EnableUnlimitedElectricity == true)
        {
            // 3. 修改欄位值
            AvailabilityRef(__instance) = int.MaxValue;
            
            // 4. 可選：記錄日誌
            // DemandModifierMod.log.Debug("電力供應已設為無限");
        }
    }
}
```

#### 步驟 4：測試
1. 建置專案：`dotnet build -c Release`
2. 部署到遊戲
3. 進入遊戲，啟用「無限電力」
4. 建造建築，確認無需發電廠也能供電

#### 步驟 5：更新 Metadata
`Properties/PublishConfiguration.xml`：
```xml
<ModVersion Value="0.1.0" />
<GameVersion Value="1.2.*" />
<ChangeLog Value="Added unlimited electricity feature" />
```

### 檢查清單

- [ ] `DemandModifierSettings.cs` 加入屬性 + `[SettingsUISection]` 標記
- [ ] 更新 7 個 `l10n/*.json` 檔案（OPTION + OPTION_DESCRIPTION）
- [ ] 建立 `[HarmonyPatch]` 類別，實作 Prefix 邏輯
- [ ] 使用 `AccessTools.FieldRefAccess` 存取私有欄位
- [ ] 加入 `Settings != null && Settings.Property == true` 檢查
- [ ] 更新 `PublishConfiguration.xml`（ModVersion, ChangeLog）
- [ ] 本機測試所有語言的 UI 顯示
- [ ] 遊戲內測試功能是否生效
- [ ] 檢查日誌確認無錯誤
- [ ] 提交程式碼並標記版本

## 遊戲版本相容性

### 目前支援版本
**Cities: Skylines 2 v1.2.***

### 版本更新影響評估

#### 高風險變更（遊戲更新時需檢查）
1. **內部欄位名稱**
   - 範例：`m_BuildingDemand` → `m_Demand`
   - 影響：`AccessTools.FieldRefAccess` 呼叫失敗
   - 修復：使用 dnSpy 或 ILSpy 反編譯遊戲 DLL，找到新欄位名

2. **System 類別重構**
   - 範例：`CommercialDemandSystem` 被拆分或合併
   - 影響：`[HarmonyPatch]` 無法找到目標類別
   - 修復：更新補丁目標類別和方法名

3. **方法簽章變更**
   - 範例：`OnUpdate()` → `OnUpdate(SystemState state)`
   - 影響：Harmony 無法配對方法
   - 修復：更新補丁方法參數

#### 中風險變更
- 設定 UI 屬性標記變更（`SettingsUISection` → 新 API）
- 日誌 API 變更（`ILog` → 新介面）

#### 低風險變更
- 遊戲內容更新（新建築、新區域類型）
- UI/圖形改進
- 效能最佳化（不影響 API）

### 遊戲更新後的驗證流程
1. **檢查編譯**：`dotnet build` 是否成功
2. **檢查日誌**：啟動遊戲，查看是否有 Harmony 錯誤
3. **功能測試**：驗證需求修改是否仍生效
4. **效能測試**：確認無明顯卡頓或記憶體洩漏
5. **更新 GameVersion**：`PublishConfiguration.xml` 更新為新版本

### 反編譯工具（查找欄位名）
- **dnSpy**：https://github.com/dnSpy/dnSpy
- **ILSpy**：https://github.com/icsharpcode/ILSpy

反編譯步驟：
1. 找到遊戲 DLL：`[Steam]\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll`
2. 用 dnSpy 開啟 `Game.dll`
3. 搜尋類別：`CommercialDemandSystem`
4. 查看私有欄位：`m_BuildingDemand`
5. 更新程式碼中的欄位名

## 進階技巧

### 動態調整需求值
目前是固定 255，可改為可調整的滑桿：
```csharp
// DemandModifierSettings.cs
[SettingsUISlider(min = 0, max = 255, step = 1)]
[SettingsUISection("需求控制", "住宅需求")]
public int ResidentialDemandValue { get; set; } = 255;

// Patch
BuildingDemandRef(__instance).value = DemandModifierMod.Settings.ResidentialDemandValue;
```

### 條件式補丁（Harmony Transpiler）
若要精細控制執行流程，可使用 Transpiler 修改 IL：
```csharp
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    // 進階：修改方法的 IL 程式碼
    // 警告：容易因遊戲更新而失效
}
```

### 多個補丁協作
若需修改多個相關系統，確保補丁執行順序：
```csharp
[HarmonyPatch(typeof(SystemA), "Method")]
[HarmonyPriority(Priority.First)] // 先執行
public class PatchA { }

[HarmonyPatch(typeof(SystemB), "Method")]
[HarmonyPriority(Priority.Last)] // 後執行
public class PatchB { }
```

## 參考資源

- **Harmony 文件**：https://harmony.pardeike.net/
- **Cities: Skylines 2 Modding Discord**：官方 Modding 社群
- **Unity ECS 文件**：https://docs.unity3d.com/Packages/com.unity.entities@latest
- **C# 語言版本對照**：https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/

