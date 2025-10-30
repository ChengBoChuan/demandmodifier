# DemandModifier - Cities: Skylines 2 需求管制模組

![License](https://img.shields.io/github/license/ChengBoChuan/demandmodifier)
![Version](https://img.shields.io/badge/version-0.2.0-blue)
![.NET](https://img.shields.io/badge/.NET-4.7.2-blue)
![C#](https://img.shields.io/badge/C%23-9.0-blue)

完全控制你的城市需求！DemandModifier 讓你可以精確控制住宅、商業和工業區的需求等級，並支援多種服務和經濟控制功能。

## 主要功能

### 🏘️ 需求控制
- **住宅需求**：控制住宅區域的需求等級
- **商業需求**：管理商業區的繁榮度
- **工業需求**：調整工業生產需求
- 5 個預設等級：Off / Low (25%) / Medium (50%) / High (75%) / Maximum (100%)

### 🔧 服務控制（規劃中）
- 無限電力、水、污水
- 無限垃圾處理、醫療、教育
- 無限警察、消防服務

### 💰 經濟控制（規劃中）
- 無限金錢
- 免費建造
- 零維護成本

### 🌍 多國語言支援
支援 8 種語言的完整本地化：
- 🇺🇸 English (en-US)
- 🇹🇼 繁體中文 (zh-HANT)
- 🇨🇳 簡體中文 (zh-HANS)
- 🇯🇵 日本語 (ja-JP)
- 🇩🇪 Deutsch (de-DE)
- 🇪🇸 Español (es-ES)
- 🇫🇷 Français (fr-FR)

## 安裝

### 方式 1：直接從 PDX Mods 下載（推薦）

1. 訪問 [PDX Mods](https://mods.paradoxplaza.com/)
2. 搜尋 "DemandModifier"
3. 點擊「Subscribe」自動下載和安裝

### 方式 2：手動安裝

1. 從 [GitHub Releases](https://github.com/ChengBoChuan/demandmodifier/releases) 下載最新版本
2. 解壓縮到 `%LocalAppData%\..\LocalLow\Colossal Order\Cities Skylines II\Mods\DemandModifier\`
3. 啟動遊戲

## 使用方式

1. 啟動 Cities: Skylines 2
2. 在主選單點擊 **Mods**
3. 勾選 **DemandModifier** 啟用模組
4. 進入遊戲後，打開 **設定 > Mods > DemandModifier**
5. 調整所需的需求等級
6. 設定立即生效！

### 設定介面

DemandModifier 提供三個分頁的設定選項：

#### 📊 需求控制分頁
- **住宅需求等級**：控制住宅區域的需求
- **商業需求等級**：控制商業區域的需求
- **工業需求等級**：控制工業區域的需求

每個選項都可以設為：
- `Off (遊戲預設)` - 使用遊戲原生的需求計算
- `Low (25%)` - 需求保持在 25%
- `Medium (50%)` - 需求保持在 50%
- `High (75%)` - 需求保持在 75%
- `Maximum (100%)` - 需求始終在最高

## 系統需求

- **Cities: Skylines 2** v1.2.* 或更高版本
- **.NET Framework** 4.7.2

## 相容性

✅ **相容的模組**：
- Traffic Mod
- Extended Road Upgrades
- 大多數其他模組

⚠️ **可能衝突**：
- 其他修改需求系統的模組
- 其他修改城市管理的模組

## 文件

### 用戶文件
- **[快速參考](docs/QUICK_REFERENCE.md)** - 常用命令和代碼片段
- **[變更日誌](CHANGELOG.md)** - 版本歷史和更新內容

### 開發文件
- **[架構文檔](docs/architecture/ARCHITECTURE.md)** - 專案架構和設計
- **[開發指南](docs/DEVELOPMENT_GUIDE.md)** - 完整的開發指南
- **[本地化指南](docs/localization/LOCALIZATION_GUIDE.md)** - 多國語言系統實作
- **[補丁指南](docs/patches/PATCH_GUIDE.md)** - Harmony 補丁系統詳解

## 開發

### 環境設定

```bash
# 設定 Cities: Skylines 2 Modding SDK
$env:CSII_TOOLPATH = "C:\Path\To\CS2ModdingSDK"

# 驗證
$env:CSII_TOOLPATH
```

### 建置

```bash
# Debug 建置
dotnet build -c Debug

# Release 建置（用於發佈）
dotnet build -c Release
```

### 部署到遊戲

```bash
# 快速部署
.\scripts\test-deploy.ps1

# 清理後重新部署
.\scripts\test-deploy.ps1 -Clean
```

### 檢查日誌

```powershell
Get-Content "$env:LOCALAPPDATA\..\LocalLow\Colossal Order\Cities Skylines II\Logs\Player.log" -Wait -Tail 50
```

## 常見問題

### Q: 為什麼需求沒有立即改變？
A: 需求值在每幀重新計算。如果在遊戲運行中改變設定，最多需要等待幾秒才能看到效果。

### Q: 可以和其他需求模組一起使用嗎？
A: 可能會衝突。建議只使用一個需求相關模組。

### Q: 模組可以修改交通或其他系統嗎？
A: 目前只支援需求控制。未來版本計劃添加更多功能。

### Q: 如何報告 bug？
A: 在 [GitHub Issues](https://github.com/ChengBoChuan/demandmodifier/issues) 上提交詳細報告。

### Q: 可以貢獻翻譯嗎？
A: 歡迎！詳見 [本地化指南](docs/localization/LOCALIZATION_GUIDE.md)。

## 性能影響

✅ **低性能開銷**
- 使用高效的 Harmony 補丁
- 補丁在需求關閉時不執行
- 快取所有反射操作

遊戲性能不會受到明顯影響。

## 貢獻

歡迎所有形式的貢獻！

### 貢獻方式

1. Fork 此倉庫
2. 建立特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交變更 (`git commit -m '新增令人驚豔的功能'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 提交 Pull Request

### 貢獻指南

詳見 [開發指南](docs/DEVELOPMENT_GUIDE.md)。

## 路線圖 (Roadmap)

- [ ] v0.3.0：服務控制系統
- [ ] v0.4.0：經濟控制系統
- [ ] v0.5.0：進階 UI（滑桿、預設）
- [ ] v1.0.0：穩定版本

詳見 [CHANGELOG.md](CHANGELOG.md) 的完整路線圖。

## 許可證

此專案採用 MIT 許可證。詳見 [LICENSE](LICENSE) 檔案。

## 致謝

- **krzychu124** - [Traffic Mod](https://github.com/krzychu124/Traffic) 作者，提供了優秀的實作參考
- **Paradox Interactive** - Cities: Skylines 2 和 Modding SDK
- **Cities: Skylines 2 社群** - 反饋和支援

## 聯絡方式

- **GitHub Issues**：[報告 Bug 和功能建議](https://github.com/ChengBoChuan/demandmodifier/issues)
- **GitHub Discussions**：[討論和問題](https://github.com/ChengBoChuan/demandmodifier/discussions)
- **Cities: Skylines 2 Modding Discord**：官方社群

## 相關資源

- 📖 [Harmony 文件](https://harmony.pardeike.net/)
- 🛠️ [Cities: Skylines 2 Modding Wiki](https://wiki.modding.paradoxplaza.com/)
- 🎮 [PDX Mods](https://mods.paradoxplaza.com/)
- 💬 [Cities: Skylines 2 Modding Discord](https://discord.gg/csl2modding)

---

**版本**：0.2.0  
**最後更新**：2025-01-28  
**作者**：ChengBoChuan

---

如果你喜歡這個模組，請考慮在 PDX Mods 上給予評分和評論！⭐

Made with ❤️ for Cities: Skylines 2 Modding Community
