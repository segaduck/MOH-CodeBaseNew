# 側邊選單權限控制詳細範例（Menu 與 Permission 設計）

## 選單載入流程
- 後台版型 _Layout.cshtml 在載入時初始化 zTree，使用 AJAX 呼叫 MainController.Menu 取得節點資料並動態建立左側樹狀選單（Areas/BACKMIN/Views/Shared/_Layout.cshtml:66）。
- Layout 會從 FormsAuthenticationTicket 解析 AdminAccount 以便查詢選單時帶入帳號，並透過 ViewBag.zTreeExpandNodeId 決定預設展開的節點（Areas/BACKMIN/Views/Shared/_Layout.cshtml:4）。
- zTree 的 treeFilter 會將節點名稱與 URL 轉換為頁面可使用的格式，將節點路徑拼回控制器名稱後再提供給前端（Areas/BACKMIN/Views/Shared/_Layout.cshtml:78）。

## 後端選單資料來源
- MainController.Menu 依 parentId 與目前帳號呼叫 CodeUtils.GetMenuList，回傳 zTree 所需的 id、isParent、name、url、target 等欄位（Areas/BACKMIN/Controllers/MainController.cs:93）。
- CodeUtils.GetMenuList 查詢 ADMIN_MENU 表，並於有傳入帳號時判斷該節點是否存在於 ADMIN_LEVEL 權限列表；另保留 995、1000 兩個系統節點不受限制（Areas/BACKMIN/Utils/CodeUtils.cs:415）。
- 若節點為功能頁（MN_TYPE 不等於 F），會提供 URL 與 Target 讓前端轉為可導向的超連結；否則視為資料夾節點僅供展開（Areas/BACKMIN/Utils/CodeUtils.cs:435）。

## 權限資料表與模型
- ADMIN_MENU 儲存後台選單樹狀結構、排序（SEQ_NO）、控制器與 Action 名稱等欄位，是共用選單來源（Areas/BACKMIN/Utils/CodeUtils.cs:469）。
- ADMIN_LEVEL 紀錄帳號與節點對應，CodeUtils.GetMenuList 會透過 EXISTS 判斷是否具備該節點瀏覽權限（Areas/BACKMIN/Utils/CodeUtils.cs:424）。
- MenuModel 定義 Id、Text、ParentId、IsFolder、Target、Control、Action、Code 等欄位，提供後台管理介面或匯出功能使用（Areas/BACKMIN/Models/MainModel.cs:84）。

## 視圖與互動細節
- Layout 在樹狀結構初始化完成後會展開指定節點，並在使用者點擊節點時將 URL 導向對應控制器頁面（Areas/BACKMIN/Views/Shared/_Layout.cshtml:95）。
- 前台選單（民眾端）透過 HomeController 與 Session 快取 LeftMenuList，邏輯較單純但保留類似的遞迴結構（Controllers/HomeController.cs:31）。
- AssignController 等頁面於 Initialize 時設定 zTreeExpandNodeId，確保開啟指定功能時能自動展開相關節點（Areas/BACKMIN/Controllers/AssignController.cs:18）。

## 權限維護流程建議
1. **帳號權限維護**：使用後台帳號管理功能（Areas/BACKMIN/Controllers/AccountController.cs:1）調整權限範圍或等級後，應同步更新 ADMIN_LEVEL 以反映選單顯示。
2. **節點新增順序**：新增功能頁時先建立 ADMIN_MENU 節點，再配置 ADMIN_LEVEL 或角色對應，避免頁面存在但選單不可見。
3. **權限稽核**：定期導出 ADMIN_MENU 與 ADMIN_LEVEL，檢查是否有孤兒節點或權限重複，必要時清理歷史設定。
4. **畫面顯示控制**：如需對按鈕或分頁等細節進行權限控管，可在 View 中檢查 SessionModel.UserInfo.Admin.LevelList 或加入自訂 Helper。

## 擴充建議
- **快取化處理**：可將常用節點快取在記憶體或 Redis，並透過版本號在權限異動時清除快取，降低資料庫查詢次數。
- **API 化**：若改採前後端分離，可將 MainController.Menu 改寫為 REST API，登入後一次取得完整節點與權限資訊。
- **多層授權**：針對同一節點的操作細項，可在 ADMIN_LEVEL 增加權限旗標（例如新增、修改、刪除）並於程式內判斷，建立更精細的存取控制。
