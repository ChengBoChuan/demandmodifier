# PowerShell 腳本說明

本資料夾包含專案相關的 PowerShell 自動化腳本。

## 腳本清單

### 1. check-thumbnail.ps1
檢查縮圖檔案是否符合 PDX Mods 的要求。

**使用方法**：
```powershell
.\scripts\check-thumbnail.ps1
```

### 2. quick-deploy.ps1
快速建置並部署模組到本機遊戲目錄進行測試。

**使用方法**：
```powershell
.\scripts\quick-deploy.ps1          # 快速部署
.\scripts\quick-deploy.ps1 -Clean   # 清理後重新建置部署
```

**功能**：
- 編譯 Release 版本
- 自動複製到遊戲 Mods 目錄
- 包含語系檔案

### 3. quick-publish.ps1
快速發佈新版本到 PDX Mods。

**使用方法**：
```powershell
.\scripts\quick-publish.ps1
```

**注意事項**：
- 執行前請先關閉遊戲（避免檔案被鎖定）
- 會自動執行 `dotnet publish /p:PublishProfile=PublishNewVersion`
- 確認 `PublishConfiguration.xml` 中的版本號和更新日誌已更新

### 4. UpdateVersion.ps1
自動更新 `PublishConfiguration.xml` 中的版本號（由建置系統自動呼叫）。

**使用方法**：
```powershell
.\scripts\UpdateVersion.ps1 "路徑\PublishConfiguration.xml"
```

**注意**：此腳本通常不需要手動執行，在 Release 建置時會自動觸發。

## 常見問題

### 發佈失敗：Access Denied
**錯誤訊息**：`Access to the path 'DemandModifier_win_x86_64.dll' is denied.`

**原因**：遊戲正在執行中，DLL 檔案被鎖定。

**解決方案**：
1. 關閉 Cities: Skylines 2
2. 重新執行發佈命令

### 腳本無法執行
**錯誤訊息**：`無法載入，因為這個系統上已停用指令碼執行。`

**解決方案**：
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## 開發工作流程

1. **開發階段**：編輯程式碼
2. **本機測試**：執行 `.\scripts\quick-deploy.ps1`
3. **啟動遊戲**：測試功能
4. **更新版本資訊**：編輯 `Properties\PublishConfiguration.xml`
   - 更新 `<ModVersion>`
   - 更新 `<ChangeLog>`
5. **關閉遊戲**
6. **發佈**：執行 `.\scripts\quick-publish.ps1`
