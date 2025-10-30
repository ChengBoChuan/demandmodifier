# 📑 完整文檔索引

## 🎯 按使用情景快速查找

### 我想...

#### 🚀 快速開始
- **新手首讀**: `docs/guides/QUICK_START.md`
- **立即步驟**: `QUICK_REFERENCE.txt` (2 分鐘速讀)
- **完整實現**: `IMPLEMENTATION_COMPLETE.md` (詳細報告)

#### 🔧 開發和修改
- **理解架構**: `docs/ARCHITECTURE.md`
- **開發補丁**: `docs/guides/PATCH_GUIDE.md`
- **後續步驟**: `docs/NEXT_STEPS.md`

#### 🌍 處理多語言
- **多語言指南**: `docs/guides/LOCALIZATION_GUIDE.md`
- **語言系統**: `Code/Localization/LocaleManager.cs` (代碼)

#### 🐛 故障排查
- **檢查清單**: `docs/troubleshooting/FIX_CHECKLIST.md`
- **日誌系統**: `Code/Utils/Logger.cs` (代碼)

#### 📊 查看統計
- **實現總結**: `docs/IMPLEMENTATION_SUMMARY.md`
- **完整報告**: `IMPLEMENTATION_COMPLETE.md`

---

## 📂 檔案完整清單

### 🎯 頂級文件

```
e:\Code\CSL2\DemandModifier\DemandModifier\
│
├─ QUICK_REFERENCE.txt            ⭐ 2分鐘快速讀物
├─ IMPLEMENTATION_COMPLETE.md      ⭐ 完整實現報告 (4,000+ 字)
├─ README.md                       遊戲內的模組說明
├─ DemandModifier.csproj           C# 專案配置
│
├─ DemandModifierMod.cs            IMod 入口點 [已改進]
├─ DemandModifierSettings.cs       設定系統
│
└─ Code/                           ⭐ 核心代碼資料夾
```

### 🔧 Code/ 資料夾結構

```
Code/
├─ Utils/
│  └─ Logger.cs                    ⭐ 日誌系統 (350 行)
│                                   8+ 日誌等級，完整功能
│
├─ Localization/
│  ├─ LocaleManager.cs             ⭐ 語言管理 (320 行)
│  │                                7 種語言，自動偵測
│  ├─ LocalizationInitializer.cs   初始化器
│  └─ ModLocale.cs                 語言定義
│
├─ Patches/
│  ├─ PatchBase.cs                 ⭐ 補丁基類 (200 行)
│  │                                3 層級架構，代碼重用
│  ├─ DemandSystemPatch.cs         ⭐ 需求補丁 [已改進]
│  │                                3 個補丁：住宅、商業、工業
│  ├─ ServiceSystemPatch.cs        ⭐ 服務補丁 [已改進]
│  │                                8 個補丁：電力、水、污水、垃圾、醫療、教育、警察、消防
│  └─ PatchUtils.cs                補丁工具
│
├─ Systems/
│  └─ DemandSystemHelper.cs        需求系統助手
│
└─ (其他系統檔案)
```

### 📚 docs/ 文檔資料夾結構

```
docs/
│
├─ ARCHITECTURE.md                 ⭐ 架構文檔 (400 行)
│  ├─ 系統架構圖
│  ├─ 資訊流動
│  ├─ 性能考量
│  └─ 核心模組詳解
│
├─ IMPLEMENTATION_SUMMARY.md       ⭐ 實現總結 (300 行)
│  ├─ 完成統計
│  ├─ 品質指標
│  └─ 亮點介紹
│
├─ NEXT_STEPS.md                   ⭐ 後續指南 (350 行)
│  ├─ 優先級排序
│  ├─ 詳細步驟
│  └─ 檢查清單
│
├─ guides/
│  ├─ QUICK_START.md               ⭐ 快速開始 (220 行)
│  │  ├─ 5 分鐘快速開始
│  │  ├─ 核心概念
│  │  └─ 常見問題
│  │
│  ├─ PATCH_GUIDE.md               ⭐ 補丁指南 (420 行)
│  │  ├─ Harmony 基礎
│  │  ├─ 3 種補丁詳解
│  │  ├─ AccessTools 工具
│  │  └─ 最佳實踐
│  │
│  └─ LOCALIZATION_GUIDE.md        ⭐ 本地化指南 (400 行)
│     ├─ 7 種語言支援
│     ├─ 5 種鍵值格式
│     ├─ 翻譯步驟
│     └─ 術語對照表
│
├─ troubleshooting/
│  ├─ FIX_CHECKLIST.md             ⭐ 故障排查清單 (400 行)
│  │  ├─ 3 步診斷程序
│  │  ├─ 5 大常見問題解決方案
│  │  ├─ 30+ 檢查項
│  │  └─ PowerShell 診斷指令
│  │
│  ├─ LOCALIZATION_FIX.md          語言系統故障排查
│  └─ PATH_FIX_FINAL.md            路徑設定指南
│
├─ architecture/
│  └─ ARCHITECTURE.md              [同 docs/ 根目錄]
│
├─ setup/
│  └─ BUILD_SUCCESS.md             建置成功指南
│
└─ changelog/
   └─ CHANGELOG.md                 版本變更日誌
```

### 🌍 l10n/ 語言檔案

```
l10n/
├─ en-US.json                      英文
├─ de-DE.json                      德文
├─ es-ES.json                      西班牙文
├─ fr-FR.json                      法文
├─ ja-JP.json                      日文
├─ zh-HANS.json                    簡體中文
└─ zh-HANT.json                    繁體中文 ✅ (優質翻譯)
```

---

## 🎓 按學習路徑推薦閱讀順序

### 初級 (新手 - 30 分鐘)
1. `QUICK_REFERENCE.txt` (2 分鐘)
2. `docs/guides/QUICK_START.md` (10 分鐘)
3. `Code/Utils/Logger.cs` (快速瀏覽，5 分鐘)
4. 執行編譯測試 (10 分鐘)

### 中級 (開發者 - 1.5 小時)
1. `docs/ARCHITECTURE.md` (30 分鐘)
2. `docs/guides/PATCH_GUIDE.md` (30 分鐘)
3. `Code/Patches/PatchBase.cs` (20 分鐘)
4. `Code/Patches/DemandSystemPatch.cs` (10 分鐘)

### 高級 (貢獻者 - 2-3 小時)
1. `IMPLEMENTATION_COMPLETE.md` (30 分鐘)
2. `docs/NEXT_STEPS.md` (30 分鐘)
3. `docs/guides/LOCALIZATION_GUIDE.md` (30 分鐘)
4. `docs/troubleshooting/FIX_CHECKLIST.md` (30 分鐘)
5. 完整代碼審查 (1 小時)

---

## 📊 文檔統計

| 檔案 | 行數 | 主題 | 難度 |
|------|------|------|------|
| QUICK_REFERENCE.txt | 50 | 快速查看 | ⭐ |
| docs/guides/QUICK_START.md | 220 | 新手指南 | ⭐⭐ |
| docs/ARCHITECTURE.md | 400 | 架構設計 | ⭐⭐⭐ |
| docs/guides/PATCH_GUIDE.md | 420 | 補丁開發 | ⭐⭐⭐ |
| docs/guides/LOCALIZATION_GUIDE.md | 400 | 多語言 | ⭐⭐⭐ |
| docs/troubleshooting/FIX_CHECKLIST.md | 400 | 故障排查 | ⭐⭐ |
| IMPLEMENTATION_COMPLETE.md | 450 | 完整報告 | ⭐⭐ |
| docs/NEXT_STEPS.md | 350 | 後續步驟 | ⭐⭐ |
| docs/IMPLEMENTATION_SUMMARY.md | 300 | 實現總結 | ⭐⭐ |
| **總計** | **3,390** | **9 份文檔** | |

---

## 🔍 按主題查找

### Harmony 補丁相關
- **基礎知識**: `docs/guides/PATCH_GUIDE.md`
- **基類設計**: `Code/Patches/PatchBase.cs`
- **實際實現**: `Code/Patches/DemandSystemPatch.cs`
- **故障排查**: `docs/troubleshooting/FIX_CHECKLIST.md`

### 日誌系統相關
- **詳細實現**: `Code/Utils/Logger.cs`
- **使用範例**: `DemandModifierMod.cs`, `DemandSystemPatch.cs`
- **故障排查**: `docs/troubleshooting/FIX_CHECKLIST.md`

### 語言系統相關
- **實現代碼**: `Code/Localization/LocaleManager.cs`
- **指南文檔**: `docs/guides/LOCALIZATION_GUIDE.md`
- **語言檔案**: `l10n/*.json`

### 架構和設計
- **完整架構**: `docs/ARCHITECTURE.md`
- **快速概覽**: `docs/guides/QUICK_START.md`
- **實現細節**: `IMPLEMENTATION_COMPLETE.md`

### 遊戲測試
- **部署步驟**: `docs/NEXT_STEPS.md`
- **故障排查**: `docs/troubleshooting/FIX_CHECKLIST.md`
- **快速開始**: `docs/guides/QUICK_START.md`

### 版本發佈
- **發佈流程**: `docs/setup/BUILD_SUCCESS.md`
- **版本日誌**: `l10n/changelog/CHANGELOG.md`

---

## ⚡ 快速查詢表

### 我需要...
```
編譯代碼              → dotnet build -c Release
查看日誌              → $env:LOCALAPPDATA/../LocalLow/.../Logs/Player.log
測試補丁              → 進遊戲，開啟 Mods 設定
反編譯驗證            → dnSpy + Game.dll
添加新補丁            → Code/Patches/PatchBase.cs + 參考現有補丁
添加新語言            → l10n/*.json + LOCALIZATION_GUIDE.md
故障排查              → FIX_CHECKLIST.md + Player.log
```

---

## 🎯 常用命令

### 編譯和部署
```powershell
# 編譯
dotnet build -c Release

# 部署測試
.\test-deploy.ps1

# 查看日誌
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Wait -Tail 50
```

### 查找文件
```powershell
# 查找特定文檔
Get-ChildItem docs -Recurse -Filter "*keyword*"

# 查看目錄結構
Tree /F docs
```

---

## 🏆 推薦起點

**新使用者 (5 分鐘)**:
→ `QUICK_REFERENCE.txt`

**想要快速開始 (30 分鐘)**:
→ `docs/guides/QUICK_START.md`

**要開發新功能 (2 小時)**:
→ `docs/ARCHITECTURE.md` → `docs/guides/PATCH_GUIDE.md` → `Code/Patches/PatchBase.cs`

**需要故障排查 (按需)**:
→ `docs/troubleshooting/FIX_CHECKLIST.md`

**完整理解 (3 小時)**:
→ `IMPLEMENTATION_COMPLETE.md` → 所有 docs 文件 → 所有代碼

---

## 📞 文檔導航技巧

### 在 VS Code 中
1. `Ctrl+P` 快速打開檔案
2. 輸入 "QUICK_START" 快速找到指南
3. `Ctrl+F` 在文檔中搜尋關鍵字
4. `Ctrl+Shift+O` 查看大綱結構

### 在 PowerShell 中
```powershell
# 搜尋含特定字詞的文件
Select-String -Path "docs/**/*.md" -Pattern "Harmony" -Recurse

# 查看特定文檔
Get-Content docs/guides/QUICK_START.md | more
```

---

**版本**: 1.0  
**最後更新**: 2025-10-30  
**文檔總數**: 9 份  
**總行數**: 3,390 行  
**涵蓋主題**: 架構、開發、本地化、故障排查、部署
