# 統一代碼系統詳細範例（CodeKind 與 CodeItem）

## 系統角色與資料表
- 代碼資料集中儲存在資料表 CODE_CD，欄位包含 CODE_KIND（代碼類別）、CODE_CD（代碼值）、CODE_DESC（顯示名稱）、CODE_PCD（父代碼）與 SEQ_NO、DEL_MK 等維護欄位（Areas/BACKMIN/Utils/CodeUtils.cs:366）。
- CODE_KIND 對應功能領域，例如 F_CASE_STATUS、F1_ACTION_TYPE_1；CODE_CD 則是實際項目，搭配 CODE_PCD 可構成樹狀階層（Areas/BACKMIN/Utils/CodeUtils.cs:415）。
- 統一代碼設計讓下拉選單、檢核邏輯與報表顯示共用同一來源，降低硬編碼風險。

## 主要存取方法
- CodeUtils.GetCodeDesc 依代碼類別與父代碼取得單筆說明文字，常用於顯示流程節點名稱或郵遞區名稱（Areas/BACKMIN/Utils/CodeUtils.cs:17）。
- CodeUtils.GetCodeSelectList 產生下拉選單集合 List of SelectListItem，支援預設值與是否加入空白項目（Areas/BACKMIN/Utils/CodeUtils.cs:35）。
- CodeUtils.GetCodeCheckBoxList 回傳多選集合，適用於申請表勾選項目（Areas/BACKMIN/Utils/CodeUtils.cs:115）。
- 其他輔助方法如 GetNumSelectList、GetSharedList、GetCase1List 讓不同模組可重複使用統一資料來源（Areas/BACKMIN/Utils/CodeUtils.cs:70）。

## 使用情境範例
- 前台申請：Apply_005001 視圖透過 CodeUtils.GetCodeSelectList 產生藥品許可證下拉選單，並以 CodeUtils.GetCodeDesc 顯示代碼文字（Views/Apply_005001/Apply.cshtml:96）。
- 後台審查：CaseController、AssignController 等頁面在篩選條件或表單顯示時使用代碼系統取得狀態名稱，維持顯示與流程一致（Areas/BACKMIN/Controllers/CaseController.cs:99）。
- 報表統計：BackApplyDAO 的統計方法在轉換 JSON 前可搭配代碼表將服務代碼與狀態代碼轉為可讀文字（DataLayers/BackApplyDAO.cs:2263）。

## 代碼管理與效能
- DataUtils.GetConfig 會快取 SETUP 表內容；若代碼讀取頻繁，建議在 CodeUtils 中比照設計加入快取以降低資料庫負載（Utils/DataUtils.cs:13）。
- 調整 CODE_CD 後需重啟網站或清除自訂快取，避免舊值被快取。

## 維護流程建議
1. **命名規則**：CODE_KIND 建議採「模組縮寫 + 功能描述」，CODE_CD 保持固定格式或數值區間。
2. **版本控管**：新增或調整代碼時建立 SQL Script 納入部署流程，避免環境差異。
3. **使用影響分析**：調整前先搜尋程式與報表是否引用該代碼，避免刪除或修改造成例外。
4. **維護介面**：可使用 CodeUtils.GetMenuList (SqlConnection) 或自建管理頁面，讓維運人員經由 CRUD 操作維護代碼並記錄異動。

## 常見擴充
- 支援多語系欄位（例如 CODE_DESC_EN）供介面切換語言。
- 增加 IS_ACTIVE、EFFECTIVE_DATE 等欄位控制代碼啟用期間。
- 建立代碼查詢 API 供前後端分離或外部系統整合使用。
