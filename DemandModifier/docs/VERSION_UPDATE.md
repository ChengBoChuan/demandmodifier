# 自動版本更新系統

## 功能說明

每次執行 **Release 建置**時，會自動將 `PublishConfiguration.xml` 中的 `ModVersion` 遞增。

## 版本號規則

- 使用語意化版本 (Semantic Versioning)：`Major.Minor.Patch`
- 每次建置自動遞增 **Patch** 版本號
- 範例：`0.0.3` → `0.0.4` → `0.0.5`

## 何時會觸發

✅ **會自動更新**：
```powershell
dotnet build -c Release
dotnet publish /p:PublishProfile=PublishNewVersion
```

❌ **不會更新**：
```powershell
dotnet build -c Debug  # Debug 建置不觸發
```

## 手動調整版本號

如果需要手動變更 Major 或 Minor 版本，直接編輯 `Properties/PublishConfiguration.xml`：

```xml
<ModVersion Value="1.0.0" />  <!-- 大版本更新 -->
<ModVersion Value="0.5.0" />  <!-- 功能更新 -->
```

下次 Release 建置時會從新版本繼續遞增 Patch。

## 技術細節

### 檔案結構
```
Properties/
├── PublishConfiguration.xml    # 版本資訊儲存位置
├── UpdateVersion.ps1           # PowerShell 更新腳本
└── UpdateVersion.targets       # MSBuild 任務定義
```

### 執行流程
1. Release 建置完成後觸發 `UpdateModVersion` 任務
2. 執行 `UpdateVersion.ps1` 腳本
3. 解析當前 `ModVersion` 值
4. 遞增 Patch 版本號
5. 寫回 XML 檔案（保留格式化）
6. 在建置輸出視窗顯示更新結果

### 停用自動更新

如果需要暫時停用，在 `.csproj` 中移除此行：

```xml
<Import Project="Properties\UpdateVersion.targets" />
```

## 範例輸出

```
✅ ModVersion 已自動更新: 0.0.3 → 0.0.4
```

## 注意事項

1. **僅在 Release 建置時觸發**，避免 Debug 時頻繁更改版本
2. **Git 提交前檢查**：版本號會自動更新，記得 commit 變更
3. **發佈前確認**：發佈到 PDX Mods 前確認版本號正確
4. **手動調整優先**：手動修改版本後，下次建置會從新版本繼續
