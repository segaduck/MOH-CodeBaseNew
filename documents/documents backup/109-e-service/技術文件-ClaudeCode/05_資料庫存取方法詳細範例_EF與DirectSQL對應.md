# 資料庫存取方法詳細範例 - EF 型 POCO 與 Direct SQL 對應

## 架構概覽
- 專案以 DataLayers 為資料存取核心，主要採 ADO.NET 與 Dapper 直接執行 SQL；Models/Entities 則提供 POCO 類別對應資料表欄位（DataLayers/ApplyDAO.cs:70；Models/Entities/ADMIN.cs:1）。
- 雖未使用 Entity Framework Context，但藉由 POCO 命名為 TblXXXX 類別，可模擬 EF 的實體映射並搭配 ValueInjecter 將 ViewModel 轉換為資料列（DataLayers/ApplyDAO.cs:9113；Models/ViewModels/Apply_005001FormModel.cs:1）。
- 重要的 CRUD 邏輯集中於 DAO，並透過 DataUtils.GetConnection() 取得 SqlConnection、BaseAction 管理交易與分頁，形成清楚的資料層責任分工（Utils/DataUtils.cs:25；Action/BaseAction.cs:21）。

## Direct SQL + Dapper 典型案例
- 查詢單筆：ApplyDAO.GetApplyFee 以 SQL 參數取得費用，使用 Dapper Query<T> 將結果映射到 TblSERVICE POCO（DataLayers/ApplyDAO.cs:64）。
- 批次查詢：BackApplyDAO.GetBarChartData 以 UNION 組合 12 個月份統計，最後 JsonConvert.SerializeObject 成前端所需的 JSON 結構（DataLayers/BackApplyDAO.cs:2071）。
- 交易寫入：AppendApply005001 在 SqlTransaction 中同時插入 APPLY、APPLY_005001、APPLY_PAY 等多張表，失敗即 Rollback（DataLayers/ApplyDAO.cs:9096）。
- 資料表轉模型：CaseSearchQuery 取得 DataTable，供 View 直接使用 foreach(DataRow) 繪製列表（Areas/BACKMIN/Views/Main/CaseSearch.cshtml:35）。

### Direct SQL 設計重點
1. 參數化查詢：所有 SQL 皆透過 DataUtils.AddParameters 或 Dapper 參數傳入，避免 SQL Injection（Utils/DataUtils.cs:103）。
2. 資料庫快取：DataUtils.GetConfig 首次讀取 SETUP 表後快取於 Dictionary，避免每次查詢都觸發資料庫（Utils/DataUtils.cs:13）。
3. 轉換與驗證：ValueInjecter 將 ViewModel 直接注入實體，搭配 Regex 驗證確保欄位格式正確（Controllers/Apply_005001Controller.cs:132）。
4. 共用基底：BaseAction.Insert 等方法集中於父類別，提供簡化的 CRUD API 與分頁工具（Action/BaseAction.cs:131）。

## 模擬 EF 的 POCO 使用方式
- Models/Entities 底下的 TblSERVICE、TblAPPLY… 等類別與資料表一一對應，命名遵循資料庫結構；即使未啟用 EF，也能享有型別安全與 IntelliSense（Models/Entities/SERVICE.cs:9）。
- 在 DAO 中透過 InjectFrom 將 ViewModel 轉為 TblAPPLY 再呼叫 base.Insert，流程類似 EF 的 context.Add(entity)（DataLayers/ApplyDAO.cs:9111）。
- 若未來需要導入 EF，可將這些 POCO 直接納入 DbContext，並把目前的 SQL 重構為 LINQ 查詢；同時保留既有 SQL 以應對複雜報表或批次效能需求。

### EF 與 Direct SQL 的取捨
| 項目 | Direct SQL (目前做法) | 假設導入 EF DbContext |
|------|----------------------|-----------------------|
| 查詢語法 | 手寫 SQL/Memo | LINQ to Entities，自動生成 |
| 效能控制 | 可精調索引/批次/NOLOCK | 需調整 LINQ、Profiling |
| 交易管理 | SqlTransaction 手動控制 | EF TransactionScope 或 SaveChanges |
| 模型維護 | 以 POCO 自行維護 | 需更新 EDMX/Code First Migrations |
| 與現有程式整合 | 直接複製既有 DAO | 需重寫為 LINQ 或 Stored Procedure Mapping |

## 混合使用策略建議
1. 維持 Direct SQL 為主：批次與報表多為複雜 SQL，Direct SQL 便於調校；可搭配 Dapper/ADO.NET 維持效能。
2. POCO 持續完善：即使不導入 EF，也應持續更新 Models/Entities，確保欄位對應與命名一致，並方便單元測試。
3. 引入 Repository Layer：若未來需要支援 EF，可建立 Repository 介面，於開發期由 Direct SQL 實作，日後再增加 EF Implementation。
4. 交易封裝：可將常用交易模式包裝為共用方法，例如 ExecuteInTransaction(Action<SqlConnection, SqlTransaction>)，降低重複程式碼。

## 實作範例：新增案件流程對照
1. ViewModel 驗證：Apply_005001FormModel 接收表單，前端透過 jQuery 驗證、後端以 Regex 再檢查（Controllers/Apply_005001Controller.cs:72）。
2. 共用驗證：ShareDAO.checkApply 檢查身份證、電話、Email（DataLayers/SHAREDAO.cs:108）。
3. 實體注入：where.InjectFrom(form) 將 ViewModel 轉換為 TblAPPLY（DataLayers/ApplyDAO.cs:9113）。
4. 直接寫入：base.Insert(where)、base.Insert(apply005001) 實際執行 SQL Insert（DataLayers/ApplyDAO.cs:9149；DataLayers/ApplyDAO.cs:9196）。
5. 補充資料：ShareDAO.SavePayFile 以 SqlCommand 更新附件資訊，同步更新 Case Flow（Controllers/PayFileULController.cs:58）。

## 測試與維運建議
- SQL 檢視：針對複雜統計 SQL 建議建立同名 Stored Procedure 以利 DB Admin 測試與調校。
- 單元測試：可利用 MemoryDB 或仿製 connection string 執行 Integration Test，確保 DAO 在 Schema 變更後仍正常。
- 效能監控：搭配 SQL Profiler 或 Application Insights 監看長時間或高頻率 SQL，適時調整索引或快取。
- 文件維護：更新資料表欄位時，同步調整 Models/Entities 以及 ValueInjecter 相關邏輯，避免 Runtime 錯誤。
