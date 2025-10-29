````markdown
# 🎮 DemandModifier 模組 - 快速使用指南

## 📥 安裝方式

模組已自動部署到遊戲目錄：
```
C:\Users\{用戶名}\AppData\LocalLow\Colossal Order\
  Cities Skylines II\Mods\DemandModifier\
```

## ✨ 功能概述

### 🏘️ 需求控制
在遊戲設定中調整城市需求等級：

- **Off** (0%) - 使用遊戲預設計算
- **Low** (25%) - 低需求
- **Medium** (50%) - 中等需求
- **High** (75%) - 高需求  
- **Maximum** (100%) - 最大需求

支援：
- 住宅需求
- 商業需求
- 工業需求

### 🗣️ 支援語言

✅ 英文 (English)  
✅ 繁體中文 (Traditional Chinese)  
✅ 簡體中文 (Simplified Chinese)  
✅ 日文 (日本語)  
✅ 德文 (Deutsch)  
✅ 西班牙文 (Español)  
✅ 法文 (Français)  

## 🎮 遊戲內使用

### 1️⃣ 啟用模組
1. 啟動 Cities: Skylines 2
2. 進入 **Mods** 列表
3. 找到 **DemandModifier**
4. 點擊 **啟用** (Enable)

### 2️⃣ 打開設定
遊戲中按 **Esc** → **設定 (Settings)** → **Mods** → **DemandModifier**

### 3️⃣ 調整需求等級
在 "需求控制 (Demand Control)" 分頁中：
- 選擇 **住宅需求等級**
- 選擇 **商業需求等級**
- 選擇 **工業需求等級**

變更會立即生效！

## 📖 分頁說明

### 需求控制 (Demand Control)
修改三種建築類型的需求等級

| 選項 | 說明 |
|------|------|
| 住宅需求等級 | 控制住宅區的需求水平 |
| 商業需求等級 | 控制商業區的需求水平 |
| 工業需求等級 | 控制工業區的需求水平 |

### 服務控制 (Service Control) 
🔜 計劃中 (v0.3.0)

### 經濟控制 (Economy Control)
🔜 計劃中 (v0.4.0)

## 🧪 測試步驟

### 驗證模組是否正常工作

1. **啟用模組後進入遊戲**
2. **打開需求統計**
   - 按 **Tab** 查看統計面板
   - 或在 **資訊視圖** 中查看需求

3. **設定住宅需求為 Maximum**
   - 進入設定 > Mods > DemandModifier
   - 住宅需求等級改為 "Maximum (100%)"
   - 回到遊戲

4. **觀察變化**
   - 住宅需求應立即變滿
   - 住宅建築應開始快速出現

5. **切換回 Off**
   - 需求應恢復正常波動

## 🔍 故障排查

### 模組未出現在列表中
- ❌ 問題：模組未被遊戲偵測
- ✅ 解決：檢查 DemandModifier 資料夾是否在正確位置
  ```
  C:\Users\{用戶名}\AppData\LocalLow\Colossal Order\
    Cities Skylines II\Mods\DemandModifier\
  ```

### 下拉選單顯示密鑰而非文字
- ❌ 問題：語言未正確載入
- ✅ 解決：
  1. 切換到其他語言
  2. 再切換回當前語言
  3. 重啟遊戲

### 需求未改變
- ❌ 問題：補丁未正確應用
- ✅ 解決：
  1. 檢查 Player.log 中的錯誤
  2. 禁用並重新啟用模組
  3. 重啟遊戲

## 📋 Player.log 位置

日誌檔案位於：
```
C:\Users\{用戶名}\AppData\LocalLow\Colossal Order\
  Cities Skylines II\Logs\Player.log
```

查看此檔案以診斷問題。

## 🌐 更改遊戲語言

1. 進入遊戲設定
2. 找到 **語言 (Language)**
3. 選擇所需語言
4. 模組設定會自動翻譯

## 📝 相容性

- ✅ Cities: Skylines 2 v1.3+
- ✅ Windows 10/11
- 🔜 macOS (待測)
- 🔜 Linux (待測)

## 🐛 回報問題

如遇 bug 或建議，可在 GitHub 提交 Issue：
```
https://github.com/ChengBoChuan/demandmodifier
```

## 🙏 致謝

感謝 Traffic Mod 和 CSL2DemandControl 提供的靈感！

---

**版本**: 0.2.0  
**最後更新**: 2025-10-29  
**狀態**: ✅ 可用


````