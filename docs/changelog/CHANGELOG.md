````markdown
# 變更日誌 (CHANGELOG)

所有顯著的變更都記錄在此檔案中。

此專案遵循 [Semantic Versioning](https://semver.org/lang/zh_TW/) 版本規則。

## [未發佈]

### 規劃中
- [ ] 服務控制系統（電力、水、污水、垃圾等）
- [ ] 經濟控制系統（無限金錢、免費建造、零維護）
- [ ] 進階 UI：滑桿以自訂需求百分比
- [ ] 快速鍵支援
- [ ] 模組 API 供其他模組使用

## [0.2.0] - 2025-01-28

### 新增
- ✨ 完整的多國語言系統（8 種語言支援）
  - 英文 (en-US)
  - 繁體中文 (zh-HANT)
  - 簡體中文 (zh-HANS)
  - 日文 (ja-JP)
  - 德文 (de-DE)
  - 西班牙文 (es-ES)
  - 法文 (fr-FR)
- ✨ 下拉選單支援需求等級選擇（Off/Low/Medium/High/Maximum）
- ✨ 完整的架構文件和開發指南
- ✨ Harmony 補丁系統完全實作（住宅、商業、工業需求）
- ✨ 本地化指南和補丁系統指南

### 改進
- 🔧 優化日誌記錄系統
- 🔧 改進錯誤處理機制
- 🔧 添加詳細的 XML 註解

### 修復
- 🐛 修正模組設定的初始化順序
- 🐛 修正 .NET 4.7.2 相容性問題

### 文件
- 📝 添加架構文檔 (ARCHITECTURE.md)
- 📝 添加開發指南 (DEVELOPMENT_GUIDE.md)
- 📝 添加本地化指南 (LOCALIZATION_GUIDE.md)
- 📝 添加補丁指南 (PATCH_GUIDE.md)
- 📝 添加快速參考 (QUICK_REFERENCE.md)

## [0.1.0] - 2025-01-27

### 新增
- ✨ 初版發佈
- ✨ 需求控制系統（住宅、商業、工業）
  - 5 個預設等級（Off/Low/Medium/High/Maximum）
  - 實時修改需求值的能力
- ✨ 遊戲內設定介面
- ✨ 基本的多語言架構

### 限制
- ⚠️ 僅支援英文（未來將添加多國語言）
- ⚠️ 服務控制和經濟控制功能規劃中

---

## 版本命名規則

### Major 版本 (X.0.0)
- 重大功能完成（如服務控制系統）
- 破壞性 API 變更
- 完整重構

### Minor 版本 (0.X.0)
- 新功能添加
- 向下相容的改進
- 新語言支援

### Patch 版本 (0.0.X)
- Bug 修復
- 性能優化
- 文件更新

---

## 貢獻者

- **ChengBoChuan** - 主要開發者

## 致謝

- **krzychu124** - Traffic 模組作者，提供了優秀的實作參考
- **Cities: Skylines 2 社群** - 反饋和建議

---

## 許可證

此專案基於 MIT 許可證。詳見 LICENSE 檔案。

---

## 更新檢查方式

### 檢查當前版本

查看 `DemandModifier.csproj` 的 `<Version>` 欄位：
```xml
<Version>0.2.0</Version>
```

或在 `PublishConfiguration.xml` 查看：
```xml
<ModVersion Value="0.2.0" />
```

### 查看遊戲中的版本

1. 在主選單 > Mods
2. 選擇 DemandModifier
3. 點擊詳細資訊
4. 查看版本號

---

## 標籤說明

在變更日誌中使用的 emoji 標籤：

- ✨ **新功能** (Feature)
- 🔧 **改進** (Improvement)
- 🐛 **修復** (Bugfix)
- ⚡ **性能** (Performance)
- 📝 **文件** (Documentation)
- 🔒 **安全** (Security)
- ⚠️ **注意** (Warning)
- 🗑️ **移除** (Removed)
- 📦 **相依性** (Dependencies)

---

## 計劃路線圖 (Roadmap)

### v0.3.0（預期 Q1 2025）
- [ ] 服務控制系統基本功能
- [ ] 自訂需求百分比滑桿

### v0.4.0（預期 Q2 2025）
- [ ] 經濟控制系統基本功能
- [ ] 快速鍵支援

### v1.0.0（預期 Q3 2025）
- [ ] 穩定版本（所有核心功能完成）
- [ ] 完整 API 文件

---

## 回報問題

如果發現 bug 或有功能建議，請：

1. 在 GitHub 上提交 Issue
2. 提供詳細的重現步驟
3. 附加相關的日誌檔案
4. 說明使用的遊戲版本和模組版本

### 推薦的 Issue 模板

```
**描述問題**
簡短清晰的描述

**重現步驟**
1. ...
2. ...
3. ...

**預期行為**
應該發生什麼

**實際行為**
實際發生了什麼

**環境資訊**
- Cities Skylines 2 版本：
- DemandModifier 版本：
- 其他相關模組：

**日誌或截圖**
附加相關的日誌或截圖
```

---

## 相容性

| 項目 | 版本要求 |
|------|----------|
| Cities Skylines 2 | v1.2.* |
| .NET Framework | 4.7.2 |
| Harmony | 2.2.2 |
| C# | 9.0 |

---

**最後更新**：2025-01-28  
**維護者**：ChengBoChuan  
**官方 GitHub**：https://github.com/ChengBoChuan/demandmodifier

````