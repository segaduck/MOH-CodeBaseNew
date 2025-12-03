# 統一安全設計詳細範例（OWASP 防護機制與驗證）

## 輸入驗證與防注入
- SecurityHelper.HasInjection 針對 QueryString、Form、Cookie 做黑名單比對，涵蓋 SQL meta 字元、UNION、exec、XSS 與 LDAP 攻擊關鍵字；若偵測到惡意字串會直接拒絕請求（Commons/SecurityHelper.cs:31）。
- Global.asax.Application_BeginRequest 會阻擋可疑路徑（如 /scripts、/views）與非 HTTPS 連線，降低目錄探勘與中間人攻擊風險（Global.asax.cs:28）。
- 各申請控制器透過多組 Regex 驗證輸入，例如 Apply_005001Controller 針對英文、數字、EMAIL、電話等欄位逐一檢查，符合 OWASP Input Validation 建議（Controllers/Apply_005001Controller.cs:132）。

## 認證與 Session 保護
- 登入成功後使用 FormsAuthenticationTicket 容納使用者角色與帳號；Global.asax.Application_AuthenticateRequest 解析 Ticket 並建立 GenericPrincipal，確保之後的授權判斷有完整角色資料（Global.asax.cs:85）。
- SessionModel 將 LoginUserInfo 以 JSON 序列化保存於 Session，並提供 LastErrorMessage、LastResultMessage 等屬性協助顯示一次性訊息（Models/SessionModel.cs:121）。
- Application_EndRequest 將所有回應 Cookie 標記為 Secure 與 HttpOnly，符合 OWASP Session Management 建議，避免 Cookie 被前端腳本或非加密通道取得（Global.asax.cs:165）。

## 授權控制
- BaseController.OnActionExecuting 未標註 AllowAnonymous 的 Action 都會檢查 SessionModel.UserInfo 是否存在，否則導向登入頁，防止未授權存取（Controllers/BaseController.cs:139）。
- CustomAuthorizeAttribute 於授權階段補齊會員資訊，確保 Session 中的使用者資料與最新資料庫狀態一致，避免權限提升漏洞（Filter/CustomAuthorizeAttribute.cs:13）。
- 後台控制器普遍使用 Authorize(Roles="Admin")；MainController.Index 亦再次檢查角色與權限清單，降低授權繞過的可能（Areas/BACKMIN/Controllers/MainController.cs:17）。

## 錯誤處理與稽核
- CustomHandleErrorAttribute 擴充 MVC 錯誤處理機制，記錄完整例外資訊並在逾時時建立 reset.okgo 檔案給維運監控，避免將堆疊資訊暴露給使用者（Filter/CustomHandleErrorAttribute.cs:14）。
- Global.asax.Application_Error 捕捉未處理例外後記錄 log4net，並對外回傳 404 防止洩漏系統詳細資訊（Global.asax.cs:102）。
- LoginController 與 BACKMIN/LoginController 皆將登入事件寫入 LOGIN_LOG，包括帳號、時間、IP、結果碼，可滿足稽核追蹤需求（Controllers/LoginController.cs:170）。

## 檔案與輸出安全
- 上傳檔案透過 ShareDAO.SavePayFile、UploadFile 類別檢查檔案大小與副檔名，並根據服務設定控制路徑與命名，避免任意檔案上傳（Controllers/PayFileULController.cs:25）。
- DocumentUtils 產生的下載檔案皆存放於設定檔指定的目錄 DOWNLOAD_DOCUMENT_PATH，並於匯出後可打包壓縮或刪除臨時檔，降低敏感資料外洩風險（Utils/DocumentUtils.cs:18）。

## 防護策略建議
1. **輸入白名單**：對常用欄位（電話、郵遞區號、證號）採白名單格式檢驗並附上錯誤訊息，維持一致驗證邏輯。
2. **統一日誌**：設定 log4net 日誌等級與輸出路徑，搭配排程檢視錯誤與登入紀錄，及早發現異常行為。
3. **憑證與 Cookie**：建議啟用 SameSite 屬性與 Cookie 版本管理，並定期檢測 Session Timeout 機制是否符合資安規範。
4. **安全測試**：導入 OWASP ZAP 或 Burp Suite 等工具對主要流程進行掃描，檢查 Injection、XSS、CSRF、敏感資訊暴露等風險。
5. **教育與流程**：建立安全開發指引，要求新增功能遵循輸入驗證、錯誤處理與授權檢查模式；重要修補應更新文件與單元測試。
