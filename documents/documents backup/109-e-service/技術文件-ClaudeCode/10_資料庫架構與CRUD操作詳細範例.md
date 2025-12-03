# 資料庫架構與 CRUD 操作詳細範例

## 架構總覽
- 專案以 SQL Server 為後端資料庫，資料表多以 APPLY、SERVICE、ADMIN、CODE_CD 等命名，由 DataLayers 內的 DAO 透過 ADO.NET 與 Dapper 直接操作（DataLayers/ApplyDAO.cs:70）。
- Models/Entities 目錄提供與資料表對應的 POCO 類別（如 TblAPPLY、TblSERVICE），即使未使用 EF 亦能保持欄位對應清晰（Models/Entities/SERVICE.cs:9）。
- Action/BaseAction 為所有 DAO 的共用基底，封裝連線、分頁、Insert/Update Helper 以及交易控制（Action/BaseAction.cs:21）。

## 連線與設定
- DataUtils.GetConnection() 讀取 Web.config 的 DefaultConnection 字串，建立 SqlConnection 物件供 DAO 使用（Utils/DataUtils.cs:25）。
- DataUtils.AddParameters 依值是否為 null 自動設定 SQL 參數，降低注入風險並統一 Null 處理（Utils/DataUtils.cs:103）。
- DataUtils.GetConfig 快取 SETUP 表資料至 Dictionary，減少重複查詢並統一系統參數來源（Utils/DataUtils.cs:13）。

## CRUD 操作範例
### 建立（Create）
- ApplyDAO.AppendApply005001 建立產銷證明書案件：於交易中先 Insert APPLY 主檔，再 Insert APPLY_005001 子檔及繳費資料、通知紀錄（DataLayers/ApplyDAO.cs:9096）。
- 透過 where.InjectFrom(form) 把 ViewModel 值注入 TblAPPLY，搭配 base.Insert 實際執行 SQL Insert 並寫入操作資訊（DataLayers/ApplyDAO.cs:9149）。

### 讀取（Read）
- ApplyDAO.GetApplyFee 使用 Dapper Query<T> 取得服務費用，回傳 TblSERVICE POCO 物件（DataLayers/ApplyDAO.cs:64）。
- BackApplyDAO.GetCaseList 回傳 DataTable 給後台統計頁 CaseSearch 使用，顯示年月與筆數（DataLayers/BackApplyDAO.cs:2346；Areas/BACKMIN/Views/Main/CaseSearch.cshtml:35）。

### 更新（Update）
- ShareDAO.SavePayFile 更新 APPLY_FILE 與補件狀態，並在必要時寄送通知信件（Controllers/PayFileULController.cs:58）。
- AssignAction.BatchUpdateApply 於分文處理流程中批次更新承辦人、流程節點與操作帳號，確保批次作業在交易中完成（Areas/BACKMIN/Controllers/AssignController.cs:102）。

### 刪除/停用（Delete）
- 系統採軟刪除設計，多數資料表以 DEL_MK 標記是否刪除，例如 CodeUtils.GetCodeSelectList 會篩選 DEL_MK = 'N' 的代碼項目（Areas/BACKMIN/Utils/CodeUtils.cs:52）。
- 若需實體刪除，建議在 DAO 中封裝專用方法並記錄異動資料，以符合稽核需求。

## 交易控制與錯誤處理
- AppendApply005001、AssignAction.BatchUpdateApply 等重要流程皆使用 SqlTransaction，成功後 Commit、失敗則 Rollback 並寫入 log4net（DataLayers/ApplyDAO.cs:9106）。
- CustomHandleErrorAttribute 與 Global.asax.Application_Error 會捕捉例外並記錄錯誤，避免 SQL 例外直接曝露給使用者（Filter/CustomHandleErrorAttribute.cs:14；Global.asax.cs:102）。

## 表結構與命名慣例
- APPLY 類表：APPLY、APPLY_005001、APPLY_PAY… 以 APP_ID 為主鍵/外鍵串聯，欄位包含流程狀態 FLOW_CD、付款資訊及建立/更新帳號時間。
- SERVICE 類表：SERVICE、SERVICE_CATE、SERVICE_FILE 等記錄服務基本資料、分類與附件設定，搭配代碼表 CODE_CD 管理分類代碼。
- 欄位命名多採大寫加底線，例如 ADD_TIME、UPD_ACC，方便與程式 POCO 屬性對應並利於稽核。

## 維運建議
1. 統一 DAO 風格：新增 DAO 時繼承 BaseAction，統一連線與分頁邏輯，並善用 DataUtils 工具方法。
2. SQL Script 管控：資料表或欄位變更應建立版本化 SQL Script，並在部署時同步執行。
3. 測試與監控：針對核心 CRUD 流程建立 Integration Test；正式環境可利用 SQL Profiler 或 Application Insights 監控長時間查詢。
4. 審核與稽核：重要表格保持 ADD_*, UPD_*, DEL_MK 欄位，定期導出與審查異動紀錄；建議結合 LOG 日誌便於追蹤。
