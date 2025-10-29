# 快速參考指南 (Quick Reference)

## 常用命令

### 建置和部署

```bash
# Debug 建置
dotnet build -c Debug

# Release 建置（用於發佈）
dotnet build -c Release

# 清理建置
dotnet clean

# 快速部署到遊戲
.\scripts\test-deploy.ps1

# 清理後重新部署
.\scripts\test-deploy.ps1 -Clean
```

### 版本管理

```bash
# 查看目前版本
Get-Content DemandModifier.csproj | Select-String "<Version>"

# 檢查發佈設定
Get-Content Properties\PublishConfiguration.xml | Select-String "<ModVersion>"
```

### 日誌查看

```bash
# 即時監控日誌
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Wait -Tail 50

# 搜尋特定文字
Select-String -Path "Player.log" -Pattern "DemandModifier"
```

## 文件快速導航

| 文件 | 用途 |
|------|------|
| `ARCHITECTURE.md` | 專案架構概述 |
| `DEVELOPMENT_GUIDE.md` | 完整開發指南 |
| `LOCALIZATION_GUIDE.md` | 多國語言系統 |
| `PATCH_GUIDE.md` | Harmony 補丁系統 |

## 代碼片段

### 記錄日誌

```csharp
private static readonly ILog log = LogManager.GetLogger(
    string.Format("{0}.{1}", nameof(DemandModifier), "ModuleName")
).SetShowsErrorsInUI(false);

log.Info(string.Format("訊息: {0}", value));
log.Error(string.Format("錯誤: {0}", exception.Message));
```

### 檢查設定

```csharp
if (DemandModifierMod.Settings == null)
    return;

if (DemandModifierMod.Settings.ResidentialDemandLevel == DemandLevel.Off)
    return;
```

### 建立 Harmony 補丁

```csharp
[HarmonyPatch(typeof(TargetClass), "OnUpdate")]
public class TargetClassPatch
{
    private static readonly AccessTools.FieldRef<TargetClass, NativeValue<int>> FieldRef =
        AccessTools.FieldRefAccess<TargetClass, NativeValue<int>>("m_FieldName");

    static void Prefix(TargetClass __instance)
    {
        if (DemandModifierMod.Settings == null) return;
        
        FieldRef(__instance).value = 255;
    }
}
```

### 新增翻譯

在 `l10n/en-US.json` 中：
```json
{
  "Options.OPTION[DemandModifier.DemandModifier.DemandModifierSettings.OptionName]": "Display Name",
  "Options.OPTION_DESCRIPTION[DemandModifier.DemandModifier.DemandModifierSettings.OptionName]": "Description"
}
```

在所有 7 個其他語言檔案中重複此步驟。

## 常見錯誤速查表

| 錯誤 | 原因 | 解決 |
|------|------|------|
| `NullReferenceException` | Settings 未初始化 | 添加 `if (Settings == null) return;` |
| 補丁無效 | 類別或方法名稱錯誤 | 用 dnSpy 驗證確切名稱 |
| 翻譯未顯示 | JSON 檔案遺失或格式錯誤 | 檢查 `l10n\` 資料夾和 JSON 語法 |
| 模組未載入 | DLL 複製失敗 | 確認 `Mods\DemandModifier\` 路徑 |
| 編譯失敗 | .NET 版本不相容 | 確認使用 .NET 4.7.2 語法 |

## 檔案位置速查表

| 資源 | 位置 |
|------|------|
| 主程式碼 | `Code/` |
| 語言檔案 | `l10n/` |
| 文件 | `docs/` |
| 補丁 | `Code/Patches/` |
| 設定 | `DemandModifierSettings.cs` |
| 遊戲日誌 | `%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log` |
| 遊戲 DLL | `[Steam]\steamapps\common\Cities Skylines II\Cities2_Data\Managed\Game.dll` |
| Mods 目錄 | `%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Mods\` |

## 需求等級對應值

| 名稱 | 值 | 百分比 |
|------|-----|--------|
| Off | 0 | 0% (遊戲預設) |
| Low | 64 | 25% |
| Medium | 128 | 50% |
| High | 192 | 75% |
| Maximum | 255 | 100% |

## 語言代碼

| 代碼 | 語言 | 檔案 |
|------|------|------|
| en-US | 英文 | en-US.json |
| zh-HANT | 繁體中文 | zh-HANT.json |
| zh-HANS | 簡體中文 | zh-HANS.json |
| ja-JP | 日文 | ja-JP.json |
| de-DE | 德文 | de-DE.json |
| es-ES | 西班牙文 | es-ES.json |
| fr-FR | 法文 | fr-FR.json |

## Harmony 特殊參數

| 參數 | 用途 |
|------|------|
| `__instance` | 被攔截的物件實例 |
| `__result` | 原始方法的返回值 (Postfix) |
| `__state` | Prefix/Postfix 間傳遞狀態 |
| `__originalMethod` | 被攔截方法的資訊 |

## 版本更新清單

- [ ] 更新 `.csproj` 中的 `<Version>`
- [ ] 更新 `PublishConfiguration.xml` 中的 `<ModVersion>`
- [ ] 編寫 `<ChangeLog>`
- [ ] 測試所有功能
- [ ] 檢查所有語言翻譯
- [ ] 驗證日誌無錯誤
- [ ] 建立 Git tag：`git tag -a v<版本> -m "Release v<版本>"`

## 有用的連結

- [Harmony 文件](https://harmony.pardeike.net/)
- [dnSpy GitHub](https://github.com/dnSpy/dnSpy)
- [Traffic Mod GitHub](https://github.com/krzychu124/Traffic)
- [Cities Skylines 2 Modding Wiki](https://wiki.modding.paradoxplaza.com)
- [PDX Mods](https://mods.paradoxplaza.com)
