# Demand Modifier - 開發指南補充

## 新增功能

### 1. 自動版本更新系統 ✅

每次執行 Release 建置時，會自動遞增 `ModVersion`。

#### 使用方式

```powershell
# Release 建置（自動更新版本）
dotnet build -c Release

# 查看建置輸出，會顯示：
# ✅ ModVersion 已自動更新: 0.0.3 → 0.0.4

# 發佈到 PDX Mods（自動更新版本）
dotnet publish /p:PublishProfile=PublishNewVersion
```

#### 版本規則
- 自動遞增 **Patch** 版本（第三位數字）
- 格式：`Major.Minor.Patch`（例如：`0.0.4`）
- 手動調整 Major/Minor 後，下次會從新版本繼續遞增

#### 相關檔案
- `Properties/UpdateVersion.ps1` - PowerShell 更新腳本
- `Properties/UpdateVersion.targets` - MSBuild 任務定義
- `Properties/VERSION_UPDATE.md` - 詳細說明文件

---

### 2. 多國語系系統說明 ℹ️

#### 重要：語系無法由使用者在模組設定中選擇

Cities: Skylines 2 的語系系統由遊戲引擎自動管理：

```
玩家在遊戲主選單設定語言
        ↓
遊戲自動載入 l10n/[語言].json
        ↓
所有模組的 UI 自動套用翻譯
```

#### 支援的語言

| 語言 | 檔案 | 狀態 |
|------|------|------|
| 繁體中文 | `l10n/zh-HANT.json` | ✅ 完整翻譯 |
| 簡體中文 | `l10n/zh-HANS.json` | ✅ 完整翻譯 |
| 英文 | `l10n/en-US.json` | ✅ 完整翻譯 |
| 日文 | `l10n/ja-JP.json` | ✅ 完整翻譯 |
| 德文 | `l10n/de-DE.json` | ✅ 完整翻譯 |
| 西班牙文 | `l10n/es-ES.json` | ✅ 完整翻譯 |
| 法文 | `l10n/fr-FR.json` | ✅ 完整翻譯 |

#### 語系切換方法（玩家操作）

1. 啟動 Cities: Skylines 2
2. 主選單 → 選項 (Options) → 語言 (Language)
3. 選擇想要的語言
4. 重新啟動遊戲
5. 進入遊戲後，Demand Modifier 的設定介面會顯示對應語言

#### 開發者測試語系

```powershell
# 1. 建置模組
dotnet build -c Release

# 2. 部署到遊戲
$modsPath = "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier"
Copy-Item "bin\Release\net48\*" $modsPath -Recurse -Force

# 3. 啟動遊戲並切換語言測試
```

#### 為什麼無法在模組設定中切換語言？

這是遊戲引擎的限制，原因如下：

1. **語系在遊戲啟動時載入**：所有 UI 字串在遊戲初始化時就已經載入完成
2. **全域性影響**：語言設定影響整個遊戲（主選單、所有模組、遊戲內 UI）
3. **效能考量**：動態切換語言需要重新載入所有 UI 資源，會造成明顯卡頓
4. **架構限制**：`ModSetting` 類別無法存取語系切換 API

#### 如果玩家想要不同語言的模組？

**解決方案：請玩家在遊戲設定中切換語言**

模組會自動適應遊戲語言，無需額外操作。

---

## 開發工作流程更新

### 完整建置與發佈流程

```powershell
# 1. 開發階段（Debug 建置，不更新版本）
dotnet build -c Debug

# 2. 測試功能
# ... 在遊戲中測試 ...

# 3. 準備發佈（Release 建置，自動更新版本）
dotnet build -c Release
# 輸出：✅ ModVersion 已自動更新: 0.0.4 → 0.0.5

# 4. 更新變更日誌（手動編輯）
# 編輯 Properties/PublishConfiguration.xml 的 <ChangeLog>

# 5. 發佈到 PDX Mods
dotnet publish /p:PublishProfile=PublishNewVersion

# 6. 提交 Git（包含更新的版本號）
git add .
git commit -m "Release v0.0.5: [功能描述]"
git push
```

### 版本號管理建議

| 變更類型 | 版本遞增 | 操作 |
|---------|---------|------|
| Bug 修復 | Patch | 自動遞增（0.0.4 → 0.0.5） |
| 新增小功能 | Patch | 自動遞增 |
| 新增重要功能 | Minor | 手動改為 0.1.0，之後自動遞增 |
| 重大架構變更 | Major | 手動改為 1.0.0，之後自動遞增 |

### 手動調整版本號

編輯 `Properties/PublishConfiguration.xml`：

```xml
<!-- 新增重要功能時 -->
<ModVersion Value="0.1.0" />

<!-- 正式版本發佈時 -->
<ModVersion Value="1.0.0" />
```

下次 Release 建置會從新版本繼續遞增（例如：1.0.0 → 1.0.1）。

---

## 常見問題

### Q: 為什麼 Debug 建置不更新版本？

**A:** 避免開發階段頻繁變更版本號。只有正式發佈時（Release 建置）才需要更新版本。

### Q: 可以停用自動版本更新嗎？

**A:** 可以。在 `DemandModifier.csproj` 中移除這一行：

```xml
<Import Project="Properties\UpdateVersion.targets" />
```

### Q: 版本號更新失敗怎麼辦？

**A:** 檢查：
1. `Properties/PublishConfiguration.xml` 是否存在
2. `<ModVersion Value="x.x.x" />` 格式是否正確
3. PowerShell 執行政策是否允許執行腳本

手動執行測試：
```powershell
pwsh -ExecutionPolicy Bypass -File "Properties\UpdateVersion.ps1" "Properties\PublishConfiguration.xml"
```

### Q: 可以在模組內加入語言選擇器嗎？

**A:** 技術上不可行。遊戲的語系系統在模組載入前就已經初始化完成，模組無法動態切換語言。這是遊戲引擎的設計限制，所有官方和社群模組都遵循此規則。

### Q: 玩家看到的是哪種語言？

**A:** 取決於玩家在遊戲主選單設定的語言：
- 繁體中文玩家 → 看到 `l10n/zh-HANT.json` 的翻譯
- 英文玩家 → 看到 `l10n/en-US.json` 的翻譯
- 其他語言同理

如果沒有對應語言檔案，遊戲會降級到英文。

---

## 檔案結構更新

```
DemandModifier/
├── DemandModifier.csproj           # 已更新：匯入 UpdateVersion.targets
├── Properties/
│   ├── PublishConfiguration.xml    # 版本資訊會自動更新
│   ├── UpdateVersion.targets       # ✨ 新增：MSBuild 任務
│   ├── UpdateVersion.ps1           # ✨ 新增：版本更新腳本
│   └── VERSION_UPDATE.md           # ✨ 新增：功能說明
├── l10n/                           # 7 種語言檔案（由遊戲自動載入）
│   ├── zh-HANT.json
│   ├── zh-HANS.json
│   ├── en-US.json
│   ├── ja-JP.json
│   ├── de-DE.json
│   ├── es-ES.json
│   └── fr-FR.json
└── DEVELOPMENT_GUIDE_SUPPLEMENT.md # ✨ 本文件
```

---

## 總結

### ✅ 已實現
1. **自動版本更新**：每次 Release 建置自動遞增 Patch 版本號
2. **完整多國語系**：支援 7 種語言，由遊戲自動管理

### ℹ️ 技術限制說明
- **語系無法在模組設定中切換**：這是遊戲引擎的設計，所有模組都遵循此規則
- **玩家需在遊戲主選單切換語言**：切換後重啟遊戲，模組會自動套用新語言

### 📚 參考文件
- 完整開發指南：`.github/copilot-instructions.md`
- 版本更新說明：`Properties/VERSION_UPDATE.md`
- 本補充文件：`DEVELOPMENT_GUIDE_SUPPLEMENT.md`
