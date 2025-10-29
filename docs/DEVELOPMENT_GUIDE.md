# DemandModifier 開發指南

## 快速開始

### 環境設定

1. **安裝 Cities: Skylines 2 Modding SDK**

   設定環境變數：
   ```powershell
   [System.Environment]::SetEnvironmentVariable(
       'CSII_TOOLPATH', 
       'C:\Path\To\CS2ModdingSDK', 
       'User'
   )
   ```

2. **驗證環境**
   ```powershell
   $env:CSII_TOOLPATH  # 應顯示 SDK 路徑
   ```

### 建置專案

```bash
cd DemandModifier
dotnet build -c Release
```

### 部署到遊戲

```bash
.\scripts\test-deploy.ps1
```

此腳本會：
1. 建置 Release 版本
2. 複製 DLL 和語言檔案到遊戲 Mods 資料夾
3. 顯示部署位置

### 測試模組

1. 重新啟動 Cities: Skylines 2
2. 在主選單點擊 **Mods**
3. 啟用 **DemandModifier**
4. 在遊戲中 > 設定 > Mods > DemandModifier 配置選項

## 專案結構詳解

```
DemandModifier/
├── Code/                          # 程式源碼
│   ├── Localization/              # 多國語言系統
│   │   ├── LocalizationInitializer.cs
│   │   └── ModLocale.cs
│   ├── Patches/                   # Harmony 補丁
│   │   └── DemandSystemPatch.cs
│   ├── Systems/                   # 輔助系統
│   └── Utils/                     # 工具函式
├── l10n/                          # 多國語言檔案 (8 種)
│   ├── en-US.json
│   ├── zh-HANT.json
│   ├── zh-HANS.json
│   ├── ja-JP.json
│   ├── de-DE.json
│   ├── es-ES.json
│   └── fr-FR.json
├── docs/                          # 文件
│   ├── architecture/              # 架構文件
│   ├── localization/              # 多語言指南
│   └── patches/                   # 補丁指南
├── Properties/                    # 專案配置
│   ├── PublishConfiguration.xml   # 發佈設定
│   └── PublishProfiles/           # 發佈配置
├── DemandModifierMod.cs           # 模組入口
├── DemandModifierSettings.cs      # 設定介面
├── DemandSystemPatch.cs           # 需求補丁
└── DemandModifier.csproj          # 專案檔
```

## 核心檔案說明

### DemandModifierMod.cs

模組的主入口點，實作 `IMod` 介面。

**主要職責**：
- 初始化多國語言系統
- 建立模組設定實例
- 註冊 Harmony 補丁
- 處理模組生命週期

**關鍵方法**：
```csharp
public void OnLoad(UpdateSystem updateSystem)    // 模組載入
public void OnDispose()                          // 模組卸載
```

### DemandModifierSettings.cs

模組設定類別，定義遊戲內設定 UI。

**主要職責**：
- 定義所有設定選項
- 提供下拉選單和翻譯支援
- 儲存使用者偏好設定

**三個分頁**：
1. **需求控制** (DemandControl)
   - 住宅需求 (ResidentialDemandLevel)
   - 商業需求 (CommercialDemandLevel)
   - 工業需求 (IndustrialDemandLevel)

2. **服務控制** (ServiceControl) - 規劃中
   - 無限電力、水、污水等

3. **經濟控制** (EconomyControl) - 規劃中
   - 無限金錢、免費建造等

### DemandSystemPatch.cs

Harmony 補丁實作，包含三個補丁類別：

- `ResidentialDemandSystemPatch`：攔截住宅需求
- `CommercialDemandSystemPatch`：攔截商業需求
- `IndustrialDemandSystemPatch`：攔截工業需求

## 新增功能的完整流程

### 範例：新增「無限垃圾」功能

#### 步驟 1：在 DemandModifierSettings.cs 中添加屬性

```csharp
/// <summary>
/// 無限垃圾 - 垃圾處理永不爆滿
/// </summary>
[SettingsUISection("ServiceControl", "ServiceSettings")]
public bool EnableUnlimitedGarbage { get; set; }
```

#### 步驟 2：在所有語言檔案中添加翻譯

編輯 8 個 `l10n/*.json` 檔案，在每個檔案中添加：

**en-US.json**：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedGarbage]": "Unlimited Garbage",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedGarbage]": "Garbage system never overflow - all buildings can handle garbage disposal."
}
```

**zh-HANT.json**：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedGarbage]": "無限垃圾",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.EnableUnlimitedGarbage]": "垃圾系統永遠不會滿溢 - 所有建築都能處理垃圾。"
}
```

重複為其他 6 種語言。

#### 步驟 3：使用 dnSpy 找到遊戲的垃圾系統

1. 打開 `[Steam]\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll`
2. 搜尋類別：`GarbageFlow` 或 `GarbageSystem`
3. 記下私有欄位名稱（例如 `m_AvailableCapacity`）

#### 步驟 4：建立補丁類別

在 `DemandSystemPatch.cs` 中添加：

```csharp
[HarmonyPatch(typeof(GarbageFlowSystem), "OnUpdate")]
public class GarbageFlowSystemPatch
{
    private static readonly ILog log = LogManager.GetLogger(
        string.Format("{0}.{1}", nameof(DemandModifier), "Patches.Garbage")
    ).SetShowsErrorsInUI(false);

    private static readonly AccessTools.FieldRef<GarbageFlowSystem, NativeValue<int>> AvailabilityRef =
        AccessTools.FieldRefAccess<GarbageFlowSystem, NativeValue<int>>("m_AvailableCapacity");

    static void Prefix(GarbageFlowSystem __instance)
    {
        try
        {
            if (DemandModifierMod.Settings == null)
                return;

            if (!DemandModifierMod.Settings.EnableUnlimitedGarbage)
                return;

            AvailabilityRef(__instance).value = int.MaxValue;
        }
        catch (System.Exception ex)
        {
            log.Error(string.Format("垃圾系統補丁發生錯誤: {0}", ex.Message));
        }
    }
}
```

#### 步驟 5：測試

```bash
dotnet build -c Release
.\scripts\test-deploy.ps1
# 遊戲中啟用「無限垃圾」，檢查垃圾容量
```

#### 步驟 6：更新版本資訊

編輯 `Properties\PublishConfiguration.xml`：

```xml
<ModVersion Value="0.2.0" />  <!-- 新功能 = Minor 版本升級 -->
<ChangeLog>
## v0.2.0
- ✨ Added unlimited garbage feature
- 🐛 Fixed demand level selection display
</ChangeLog>
```

## .NET 4.7.2 相容性檢查清單

- [ ] 不使用 C# 10+ 新語法 (`record`, `file` namespace, etc.)
- [ ] 字串連接使用 `string.Format()` 而非 `$` 插值
- [ ] 陣列初始化使用 `new Type[] { }` 而非 `[ ]`
- [ ] 使用 `switch` 語句而非 `switch` 表達式
- [ ] 顯式 `null` 檢查，不使用 `?.` 或 `??`
- [ ] 檢查所有引用的 DLL 在遊戲中可用

## 日誌記錄最佳實踐

### 建立 Logger

```csharp
private static readonly ILog log = LogManager.GetLogger(
    string.Format("{0}.{1}", nameof(DemandModifier), "ModuleName")
).SetShowsErrorsInUI(false);
```

### 使用日誌等級

```csharp
log.Debug("詳細除錯資訊");           // 開發階段
log.Info("一般資訊 - 重要事件");     // 關鍵操作
log.Warn("警告訊息 - 非預期情況");   // 潛在問題
log.Error("錯誤訊息 - 功能失敗");    // 錯誤但可恢復
log.Critical("嚴重錯誤");             // 致命錯誤
```

### 查看日誌

日誌位置：
```
%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log
```

即時監控：
```powershell
Get-Content "...\Player.log" -Wait -Tail 50
```

## 除錯技巧

### 使用 dnSpy 反編譯遊戲 DLL

1. 下載 dnSpy：https://github.com/dnSpy/dnSpy
2. 打開 `Game.dll`
3. 搜尋目標類別或方法
4. 查看私有欄位名稱
5. 確認方法簽名

### 測試補丁是否生效

1. 添加日誌：
   ```csharp
   log.Info("補丁已執行");
   ```

2. 檢查日誌檔案看是否輸出

3. 遊戲內觀察功能是否生效

### 常見問題排查

| 問題 | 原因 | 解決方案 |
|------|------|---------|
| 模組未出現在列表 | DLL 路徑錯誤 | 確認 `Mods\DemandModifier\DemandModifier.dll` |
| 設定介面空白 | 語言檔案遺失 | 確認 `l10n\*.json` 存在並被複製 |
| 功能不生效 | 補丁未註冊或邏輯錯誤 | 檢查日誌，驗證補丁類別有 `[HarmonyPatch]` |
| 遊戲崩潰 | 欄位名稱或類型錯誤 | 用 dnSpy 驗證類別和欄位 |

## 發佈到 PDX Mods

### 版本號規則 (Semantic Versioning)

```
Major.Minor.Patch

Major：重大變更、破壞性更新
Minor：新功能、向下相容
Patch：Bug 修復、小改進
```

範例：
- `0.1.0`：初版發佈
- `0.2.0`：新增需求控制功能
- `0.2.1`：修復需求不生效的 bug
- `1.0.0`：穩定版本

### 發佈命令

首次發佈（獲得 ModId）：
```bash
dotnet publish /p:PublishProfile=PublishNewMod
```

更新版本：
```bash
dotnet publish /p:PublishProfile=PublishNewVersion
```

## 參考資源

- **Harmony 文件**：https://harmony.pardeike.net/
- **dnSpy**：https://github.com/dnSpy/dnSpy
- **Traffic Mod**：https://github.com/krzychu124/Traffic
- **Cities Skylines 2 Modding Discord**：官方社群
- **PDX Mods**：https://mods.paradoxplaza.com

## 常見問題 (FAQ)

**Q: 如何在其他模組中使用 DemandModifier 的設定？**

A: 目前不支援，但計劃在未來版本中提供 API。

**Q: 可以修改其他系統（如交通、建築）嗎？**

A: 可以使用相同的 Harmony 補丁模式。在 `Code\Patches\` 中建立新的補丁類別。

**Q: 如何貢獻程式碼？**

A: 提交 Pull Request 到專案 GitHub 倉庫。

**Q: 如何報告 bug？**

A: 在 GitHub Issues 中報告，提供詳細的重現步驟和日誌。

