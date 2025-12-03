# 身份驗證與授權機制詳細範例（Authentication 與 Authorization）

## 驗證流程總覽
- 前台登入頁 LoginController.Index 先檢查 Session 是否已有會員資訊，有則導向會員專區（Controllers/LoginController.cs:34）。
- POST Index(LoginViewModel model) 依 Hide_loginType 判斷登入方式，檢核 PIN、憑證資料並呼叫 MemberAction.CheckCAData 取得會員資訊（Controllers/LoginController.cs:72）。
- 登入成功後透過 LoginDAO.LoginValidate 載入角色與帳號屬性，寫入 LoginUserInfo 並序列化保存於 SessionModel.UserInfo（Controllers/LoginController.cs:118）。
- 最後產生 FormsAuthenticationTicket，將 Roles、MemberAccount 等欄位寫入 Cookie 供全站授權判斷使用（Controllers/LoginController.cs:134）。

## Session 與使用者狀態
- SessionModel.Get() 建立 HttpSessionStateWrapper，設定 20 分鐘逾時並提供 UserInfo、LastErrorMessage、CaseApply 等屬性（Models/SessionModel.cs:33）。
- UserInfo 以 JSON 序列化與反序列化 LoginUserInfo，確保跨網域或重新載入時仍能還原完整資訊（Models/SessionModel.cs:121）。
- LastErrorMessage、LastResultMessage 讀取後即清空，配合 TempData 呈現登入錯誤或操作成功訊息（Models/SessionModel.cs:180）。

## FormsAuthentication 與角色
- Login 成功後使用 FormsAuthenticationTicket 保存使用者資料，UserData 內含 Roles、MemberAccount 等欄位（Controllers/LoginController.cs:134）。
- Global.asax.Application_AuthenticateRequest 解析 Ticket 並建立 GenericPrincipal，將 Roles 指派給目前要求的使用者（Global.asax.cs:85）。
- Application_EndRequest 將所有回應 Cookie 標記為 Secure 與 HttpOnly，避免身分資訊被前端腳本或非 HTTPS 取得（Global.asax.cs:165）。

## 授權檢查
- BaseController.OnActionExecuting 若 Action 未標註 Commons.AllowAnonymousAttribute，會檢查 SessionModel.UserInfo 是否存在，否則導向登入頁（Controllers/BaseController.cs:139）。
- CustomAuthorizeAttribute 對已登入會員補齊 Session：透過 Forms Ticket 重新查詢會員資料並放入 Session，確保權限判斷一致（Filter/CustomAuthorizeAttribute.cs:13）。
- BACKMIN 區域控制器以 Authorize(Roles="Admin") 修飾，搭配 MainController.Index 的權限檢查，保障未授權者無法進入後台（Areas/BACKMIN/Controllers/MainController.cs:17）。

## 自訂登入保護
- 前台登入記錄成功與失敗事件，包含來源 IP、錯誤訊息與憑證狀態，並寫入 LOGIN_LOG 供稽核（Controllers/LoginController.cs:170）。
- 後台登入提供驗證碼與鎖帳機制：連續輸入錯誤會暫停 15 分鐘並提示客服電話（Areas/BACKMIN/Controllers/LoginController.cs:96）。
- 兩種登入流程皆利用 SessionModel.LoginValidateCode 驗證圖形碼，驗證後即清空降低重複提交風險（Areas/BACKMIN/Controllers/LoginController.cs:87）。

## 跨站請求與錯誤處理
- Global.asax.Application_BeginRequest 強制 HTTP 轉為 HTTPS，並拒絕異常路徑存取，降低目錄探勘與中間人攻擊（Global.asax.cs:28）。
- SecurityHelper.HasInjection 檢查 Query/Form/Cookie 中的 SQL、XSS、LDAP 關鍵字，若偵測到攻擊會直接拒絕（Commons/SecurityHelper.cs:31）。
- CustomHandleErrorAttribute 擴充 MVC 錯誤處理，記錄 log 並在逾時時建立 reset.okgo 檔案給維運監控（Filter/CustomHandleErrorAttribute.cs:14）。

## 授權資料結構
- LoginUserInfo 同時保存前台會員與後台承辦資訊，並紀錄 LoginAuth、LoginIP、是否需變更密碼等屬性（Models/LoginUserInfo.cs:64）。
- LoginDAO.LoginValidate 讀取 ADMIN、ADMIN_LEVEL 等資料表，建立權限範圍與等級清單供後續控制器判斷（DataLayers/LoginDAO.cs:17）。
- 後台版型會從 Forms Ticket 解析 AdminAccount，顯示於畫面並決定左側選單載入方式（Areas/BACKMIN/Views/Shared/_Layout.cshtml:4）。

## 建議作法
1. 統一 Ticket 結構：新增欄位時同步更新登入程式與 CustomAuthorizeAttribute，確保 Session 注入資料一致。
2. Session 逾時提示：可在版型加入倒數提醒或於 AJAX 呼叫檢查 SessionModel.UserInfo，避免表單編輯逾時。
3. 角色管理：將 Roles 與權限對應集中管理，可結合後台帳號維護頁面 AccountController（Areas/BACKMIN/Controllers/AccountController.cs:1）。
4. 審計與警示：定期檢視 LOGIN_LOG 與排程寄送異常登入報表，搭配 WAF/AD 監控提升安全性。
