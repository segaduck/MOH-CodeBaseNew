# MIT 微笑標章管理系統 - 資料庫存取方法詳細範例：Entity Framework 與 Direct SQL 對比

## 功能概述

本文詳細說明 MITAP2024 系統中兩種不同的資料庫存取方法：Entity Framework 標準方法和 Direct SQL Scripts。透過具體的程式碼範例，展示在不同場景下如何選擇適當的資料庫存取策略，以及兩種方法的優缺點和適用時機。

## 資料庫存取策略分類

```
MITAP2024 資料庫存取策略：

Entity Framework 標準方法：
    ↓ 適用場景
單一 Table CRUD 操作
    ↓ 技術特點
強型別、LINQ 查詢、自動追蹤變更
    ↓ 範例功能
使用者管理、部門管理、基本資料維護

Direct SQL Scripts：
    ↓ 適用場景
複雜查詢、批次更新、統計分析
    ↓ 技術特點
原生 SQL、高效能、複雜邏輯
    ↓ 範例功能
統計報表、多表關聯查詢、大量資料處理
```

## 1. Entity Framework 標準方法範例

### 1.1 單一 Table 的 CRUD 操作

**檔案位置：** `MITAP2024/MITAP2024.Server/Main/MIT10/Service/MIT1001Service.cs`

#### 1.1.1 查詢單筆資料 (Read)

```csharp
/// <summary>
/// 使用 Entity Framework 查詢單筆主分類資料
/// 展示基本的 Find 方法和關聯資料查詢
/// </summary>
/// <param name="model">查詢條件模型</param>
/// <param name="sessiondata">使用者 Session 資料</param>
/// <returns>查詢結果</returns>
public async Task<ActionResult<PagedQueryResult>> QueryOne(MIT1001QueryModel model, SessionDataModel? sessiondata)
{
    return await Task.Run(() =>
    {
        PagedQueryResult result = new PagedQueryResult()
        {
            jwtkey = model.jwtkey,
            IsSuccess = false
        };

        try
        {
            // 第一步：參數驗證
            if (string.IsNullOrEmpty(model.guid) || Guid.Parse(model.guid) == Guid.Empty)
            {
                result.IsSuccess = false;
                result.message = "未傳入主分類代碼";
            }
            else
            {
                // 第二步：使用 Entity Framework 的 Find 方法查詢主要資料
                // Find 方法會先檢查 ChangeTracker，如果沒有才查詢資料庫
                IndustryDetail? entity = _context.IndustryDetail
                    .Where(x => x.Guid == Guid.Parse(model.guid))
                    .FirstOrDefault();

                if (entity != null)
                {
                    // 第三步：查詢關聯資料
                    // 使用 Find 方法查詢關聯的部門資料
                    Departments? dept = _context.Departments.Find(entity.DeptGuId);

                    // 查詢關聯的使用者資料
                    Models.Sys.Users? user = _context.Users.Find(entity.UserGuId);

                    // 第四步：轉換為 ViewModel
                    MIT1001Model data = ToViewModel(entity, dept, user);
                    data.jwtkey = model.jwtkey;

                    result.Data = data;
                    result.IsSuccess = true;
                }
                else
                {
                    result.IsSuccess = false;
                    result.message = "未查詢到主分類";
                }
            }
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "查詢單筆主分類失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **Find 方法優勢**：自動檢查 ChangeTracker，避免重複查詢
2. **強型別查詢**：使用 LINQ 表達式，編譯時期檢查型別安全
3. **關聯資料處理**：分別查詢關聯資料，保持程式碼清晰
4. **自動物件追蹤**：Entity Framework 自動追蹤物件狀態變更
5. **異常處理**：統一的異常處理機制

#### 1.1.2 新增資料 (Create)

```csharp
/// <summary>
/// 使用 Entity Framework 新增主分類資料
/// 展示 Add 方法和 SaveChanges 的使用
/// </summary>
/// <param name="model">新增資料模型</param>
/// <param name="sessiondata">使用者 Session 資料</param>
/// <returns>新增結果</returns>
public async Task<ActionResult<PagedQueryResult>> Create(MIT1001Model model, SessionDataModel? sessiondata)
{
    return await Task.Run(() =>
    {
        PagedQueryResult result = new PagedQueryResult()
        {
            jwtkey = model.jwtkey,
            IsSuccess = false
        };

        try
        {
            // 第一步：資料驗證
            if (CheckModelDataHasError(model, ref result, true))
            {
                result.IsSuccess = false;
                return result;
            }

            // 第二步：檢查重複資料
            // 使用 LINQ 查詢檢查是否已存在相同的主分類代碼
            var existingEntity = _context.IndustryDetail
                .Where(x => x.IndustryCode == model.IndustryCode && x.IsDelete == "N")
                .FirstOrDefault();

            if (existingEntity != null)
            {
                result.message = "主分類代碼已存在";
                result.IsSuccess = false;
                return result;
            }

            // 第三步：建立新的 Entity 物件
            IndustryDetail entity = new IndustryDetail()
            {
                Guid = Guid.NewGuid(),                    // 產生新的 GUID
                IndustryCode = model.IndustryCode,        // 主分類代碼
                IndustryName = model.IndustryName,        // 主分類名稱
                DeptGuId = Guid.Parse(model.DeptGuId),    // 審查單位 GUID
                UserGuId = Guid.Parse(model.UserGuId),    // 聯絡人 GUID
                IsBatchInspection = model.IsBatchInspection ?? "N", // 是否逐批驗證
                GrantsConPdtNum = model.GrantsConPdtNum ?? 0,       // 精進資格產品數量
                IsDelete = "N",                           // 刪除標記
                CreateDate = DateTime.Now,                // 建立時間
                CreateUser = sessiondata.Name,            // 建立者
                ModifyDate = DateTime.Now,                // 修改時間
                ModifyUser = sessiondata.Name             // 修改者
            };

            // 第四步：使用 Entity Framework 新增資料
            // Add 方法將 Entity 標記為 Added 狀態
            _context.IndustryDetail.Add(entity);

            // SaveChanges 方法執行實際的資料庫 INSERT 操作
            _context.SaveChanges();

            // 第五步：查詢關聯資料並回傳結果
            Departments? dept = _context.Departments.Find(entity.DeptGuId);
            Models.Sys.Users? users = _context.Users.Find(entity.UserGuId);

            result.Data = ToViewModel(entity, dept, users);
            result.IsSuccess = true;
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "新增主分類失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **Add 方法**：將 Entity 加入 ChangeTracker 並標記為 Added 狀態
2. **SaveChanges**：批次執行所有變更，支援交易處理
3. **GUID 產生**：使用 `Guid.NewGuid()` 產生唯一識別碼
4. **重複檢查**：使用 LINQ 查詢檢查資料重複性
5. **審計欄位**：自動設定建立時間、建立者等審計資訊

#### 1.1.3 更新資料 (Update)

```csharp
/// <summary>
/// 使用 Entity Framework 更新主分類資料
/// 展示 Update 方法和變更追蹤的使用
/// </summary>
/// <param name="model">更新資料模型</param>
/// <param name="sessiondata">使用者 Session 資料</param>
/// <returns>更新結果</returns>
public async Task<ActionResult<PagedQueryResult>> Update(MIT1001Model model, SessionDataModel? sessiondata)
{
    return await Task.Run(() =>
    {
        PagedQueryResult result = new PagedQueryResult()
        {
            jwtkey = model.jwtkey,
            IsSuccess = false
        };

        try
        {
            // 第一步：資料驗證
            if (CheckModelDataHasError(model, ref result, false))
            {
                result.IsSuccess = false;
                return result;
            }

            // 第二步：查詢要更新的資料
            // 使用 Where 查詢確保資料存在
            IndustryDetail? entity = _context.IndustryDetail
                .Where(x => x.Guid == Guid.Parse(model.Guid))
                .FirstOrDefault();

            if (entity == null)
            {
                result.message = "主分類不存在";
                result.IsSuccess = false;
                return result;
            }

            // 第三步：檢查重複資料（排除自己）
            var duplicateEntity = _context.IndustryDetail
                .Where(x => x.IndustryCode == model.IndustryCode &&
                           x.IsDelete == "N" &&
                           x.Guid != Guid.Parse(model.Guid))
                .FirstOrDefault();

            if (duplicateEntity != null)
            {
                result.message = "主分類代碼已存在";
                result.IsSuccess = false;
                return result;
            }

            // 第四步：更新 Entity 屬性
            // Entity Framework 會自動追蹤這些變更
            entity.IndustryCode = model.IndustryCode;
            entity.IndustryName = model.IndustryName;
            entity.DeptGuId = Guid.Parse(model.DeptGuId);
            entity.UserGuId = Guid.Parse(model.UserGuId);
            entity.IsBatchInspection = model.IsBatchInspection ?? "N";
            entity.GrantsConPdtNum = model.GrantsConPdtNum ?? 0;
            entity.ModifyDate = DateTime.Now;        // 更新修改時間
            entity.ModifyUser = sessiondata.Name;    // 更新修改者

            // 第五步：儲存變更
            // Entity Framework 會自動偵測變更並產生 UPDATE SQL
            _context.SaveChanges();

            // 第六步：查詢關聯資料並回傳結果
            Departments? dept = _context.Departments.Find(entity.DeptGuId);
            Models.Sys.Users? users = _context.Users.Find(entity.UserGuId);
            result.Data = ToViewModel(entity, dept, users);

            result.IsSuccess = true;
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "更新主分類失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **變更追蹤**：Entity Framework 自動追蹤屬性變更
2. **樂觀鎖定**：透過查詢確認資料存在，避免併發問題
3. **部分更新**：只更新變更的欄位，提升效能
4. **重複檢查**：排除自己的重複性檢查邏輯
5. **審計更新**：自動更新修改時間和修改者資訊

### 1.2 LINQ 複雜查詢範例

**檔案位置：** `MITAP2024/MITAP2024.Server/Main/SYS10/Service/SYS1005Service.cs`

```csharp
/// <summary>
/// 使用 LINQ 進行複雜的關聯查詢
/// 展示 Join 操作和子查詢的使用
/// </summary>
/// <param name="queryModel">查詢條件</param>
/// <returns>查詢結果</returns>
public async Task<ActionResult<PagedQueryResult>> QueryOne(SYS1005QueryModel queryModel, SessionDataModel? sessiondata)
{
    return await Task.Run(() =>
    {
        // 省略基本驗證邏輯...

        // 第一步：查詢主要角色資料
        Roles? entity = _context.Roles
            .Where(x => x.Guid == Guid.Parse(queryModel.guid))
            .FirstOrDefault();

        if (entity != null)
        {
            SYS1005Model data = ToViewModel(entity);

            // 第二步：查詢關聯的系統應用程式資料
            SysApp? sysapp = _context.SysApp
                .Where(x => x.Guid == entity.SysAppGuid)
                .FirstOrDefault();

            if (sysapp != null)
            {
                data.SysName = sysapp.SysName;
            }

            // 第三步：使用 LINQ Join 查詢角色關聯的使用者
            // 這個查詢展示了多表關聯和投影的使用
            data.Users = (from u in _context.Users                    // 主表：使用者
                         join ru in _context.RoleUsers                // 關聯表：角色使用者關聯
                         on u.Guid equals ru.UserGuid                 // Join 條件
                         where ru.RoleGuid == entity.Guid             // 篩選條件：特定角色
                         select new SYS1005ModelSub1()               // 投影到 ViewModel
                         {
                             Guid = TextUtils.GuidToString(u.Guid),
                             UserAct = u.UserAct,
                             Name = u.Name
                         }).ToList();

            result.Data = data;
            result.IsSuccess = true;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **LINQ Join**：使用 `join` 關鍵字進行表格關聯
2. **投影查詢**：使用 `select new` 投影到特定的 ViewModel
3. **延遲執行**：LINQ 查詢在 `ToList()` 時才實際執行
4. **強型別**：整個查詢過程都有型別檢查
5. **可讀性**：LINQ 語法接近 SQL，易於理解和維護

## 2. Direct SQL Scripts 範例

### 2.1 複雜統計查詢範例

**檔案位置：** `MITAP2024/MITAP2024.Server/Main/MIT10/Service/MIT1002Service.cs`

#### 2.1.1 動態 SQL 查詢組合

```csharp
/// <summary>
/// 使用 Direct SQL 進行複雜的分頁查詢
/// 展示動態 SQL 組合和參數化查詢
/// </summary>
/// <param name="model">查詢條件模型</param>
/// <param name="sessiondata">使用者 Session 資料</param>
/// <returns>分頁查詢結果</returns>
public async Task<ActionResult<PagedQueryResult>> Query(MIT1002QueryModel model, SessionDataModel? sessiondata)
{
    return await Task.Run(() =>
    {
        PagedQueryResult result = new PagedQueryResult()
        {
            jwtkey = model.jwtkey,
            TotalCount = 0,
            DataList = new List<MIT1002ListModel>(),
            IsSuccess = false
        };

        try
        {
            // 第一步：定義基本的 SQL 查詢語句
            // 使用複雜的多表關聯查詢
            var QueryCountCmd = @"
SELECT count(*)
FROM ProductIndustry pi2
    INNER JOIN IndustryDetail id ON pi2.IndustryGuId = id.Guid
    INNER JOIN Departments d ON id.DeptGuId = d.Guid
    INNER JOIN Users u ON id.UserGuId = u.Guid
WHERE pi2.IsDelete = 'N'
    AND id.IsDelete = 'N'
    AND (@IndustryGuId IS NULL OR pi2.IndustryGuId = @IndustryGuId)
    AND (@ProductIndustryName IS NULL OR pi2.ProductIndustryName LIKE CONCAT('%', @ProductIndustryName, '%'))
";

            var QueryCmd = @"
SELECT
    pi2.Guid,
    pi2.ProductIndustryName,
    pi2.ProductIndustryCode,
    pi2.VerifyId,
    pi2.CreateDate,
    pi2.CreateUser,
    pi2.ModifyDate,
    pi2.ModifyUser,
    id.IndustryName,
    d.DisplayName as DeptName,
    u.Name as UserName
FROM ProductIndustry pi2
    INNER JOIN IndustryDetail id ON pi2.IndustryGuId = id.Guid
    INNER JOIN Departments d ON id.DeptGuId = d.Guid
    INNER JOIN Users u ON id.UserGuId = u.Guid
WHERE pi2.IsDelete = 'N'
    AND id.IsDelete = 'N'
    AND (@IndustryGuId IS NULL OR pi2.IndustryGuId = @IndustryGuId)
    AND (@ProductIndustryName IS NULL OR pi2.ProductIndustryName LIKE CONCAT('%', @ProductIndustryName, '%'))
ORDER BY pi2.ProductIndustryName
OFFSET @startrows ROWS
FETCH NEXT @readrows ROWS ONLY
";

            // 第二步：動態處理查詢條件
            // 根據使用者輸入動態調整 SQL 語句
            if (string.IsNullOrEmpty(model.q_industryGuid))
            {
                QueryCountCmd = QueryCountCmd.Replace("AND (@IndustryGuId IS NULL OR pi2.IndustryGuId = @IndustryGuId)", "");
                QueryCmd = QueryCmd.Replace("AND (@IndustryGuId IS NULL OR pi2.IndustryGuId = @IndustryGuId)", "");
            }

            if (string.IsNullOrEmpty(model.q_productIndustryName))
            {
                QueryCountCmd = QueryCountCmd.Replace("AND (@ProductIndustryName IS NULL OR pi2.ProductIndustryName LIKE CONCAT('%', @ProductIndustryName, '%'))", "");
                QueryCmd = QueryCmd.Replace("AND (@ProductIndustryName IS NULL OR pi2.ProductIndustryName LIKE CONCAT('%', @ProductIndustryName, '%'))", "");
            }

            // 第三步：動態處理排序條件
            // 根據使用者選擇的排序欄位動態調整 ORDER BY
            if (model.q_sortColumn > 0)
            {
                switch (model.q_sortColumn)
                {
                    case 1: QueryCmd = QueryCmd.Replace("ORDER BY pi2.ProductIndustryName", "ORDER BY pi2.ProductIndustryName"); break;
                    case 2: QueryCmd = QueryCmd.Replace("ORDER BY pi2.ProductIndustryName", "ORDER BY pi2.ProductIndustryCode"); break;
                    case 3: QueryCmd = QueryCmd.Replace("ORDER BY pi2.ProductIndustryName", "ORDER BY id.IndustryName"); break;
                    case 4: QueryCmd = QueryCmd.Replace("ORDER BY pi2.ProductIndustryName", "ORDER BY d.DisplayName"); break;
                    case 5: QueryCmd = QueryCmd.Replace("ORDER BY pi2.ProductIndustryName", "ORDER BY pi2.CreateDate DESC"); break;
                    default: break;
                }
            }

            // 第四步：動態處理分頁
            // 如果不需要分頁，移除 OFFSET 和 FETCH 子句
            if (model.q_startRow == 0 && model.q_readRow == 0)
            {
                QueryCmd = QueryCmd.Replace("OFFSET @startrows ROWS", "");
                QueryCmd = QueryCmd.Replace("FETCH NEXT @readrows ROWS ONLY", "");
            }

            // 第五步：執行 SQL 查詢
            using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
            {
                conn.Open();

                // 第六步：執行計數查詢
                DbCommand countCmd = conn.CreateCommand();
                countCmd.CommandText = QueryCountCmd;

                // 設定查詢參數
                if (!string.IsNullOrEmpty(model.q_industryGuid))
                {
                    countCmd.Parameters.Add(new SqlParameter("@IndustryGuId", Guid.Parse(model.q_industryGuid)));
                }
                if (!string.IsNullOrEmpty(model.q_productIndustryName))
                {
                    countCmd.Parameters.Add(new SqlParameter("@ProductIndustryName", model.q_productIndustryName));
                }

                // 執行計數查詢
                var totalCountObj = countCmd.ExecuteScalar();
                result.TotalCount = Convert.ToInt32(totalCountObj);

                // 第七步：執行資料查詢
                DbCommand dataCmd = conn.CreateCommand();
                dataCmd.CommandText = QueryCmd;

                // 設定查詢參數
                if (!string.IsNullOrEmpty(model.q_industryGuid))
                {
                    dataCmd.Parameters.Add(new SqlParameter("@IndustryGuId", Guid.Parse(model.q_industryGuid)));
                }
                if (!string.IsNullOrEmpty(model.q_productIndustryName))
                {
                    dataCmd.Parameters.Add(new SqlParameter("@ProductIndustryName", model.q_productIndustryName));
                }
                if (model.q_startRow > 0 || model.q_readRow > 0)
                {
                    dataCmd.Parameters.Add(new SqlParameter("@startrows", model.q_startRow));
                    dataCmd.Parameters.Add(new SqlParameter("@readrows", model.q_readRow));
                }

                // 第八步：讀取查詢結果
                List<MIT1002ListModel> retrieveDataList = new List<MIT1002ListModel>();
                using (var reader = dataCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        retrieveDataList.Add(new MIT1002ListModel()
                        {
                            Guid = TextUtils.GetDbDataString(reader, "Guid"),
                            ProductIndustryName = TextUtils.GetDbDataString(reader, "ProductIndustryName"),
                            ProductIndustryCode = TextUtils.GetDbDataString(reader, "ProductIndustryCode"),
                            VerifyId = TextUtils.GetDbDataString(reader, "VerifyId"),
                            CreateDate = TextUtils.GetDbDataString(reader, "CreateDate"),
                            CreateUser = TextUtils.GetDbDataString(reader, "CreateUser"),
                            ModifyDate = TextUtils.GetDbDataString(reader, "ModifyDate"),
                            ModifyUser = TextUtils.GetDbDataString(reader, "ModifyUser"),
                            IndustryName = TextUtils.GetDbDataString(reader, "IndustryName"),
                            DeptName = TextUtils.GetDbDataString(reader, "DeptName"),
                            UserName = TextUtils.GetDbDataString(reader, "UserName")
                        });
                    }
                }

                result.DataList = retrieveDataList;
                conn.Close();
                result.IsSuccess = true;
            }
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "查詢次分類失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **複雜關聯查詢**：使用多個 INNER JOIN 關聯不同資料表
2. **動態 SQL 組合**：根據查詢條件動態調整 SQL 語句
3. **參數化查詢**：使用 SqlParameter 防止 SQL Injection 攻擊
4. **分頁處理**：使用 OFFSET 和 FETCH NEXT 實現高效分頁
5. **動態排序**：根據使用者選擇動態調整 ORDER BY 子句
6. **資料讀取**：使用 SqlDataReader 高效讀取大量資料
7. **連線管理**：使用 using 語句確保連線正確關閉

### 2.2 統計分析查詢範例

**檔案位置：** `MITAP2024/documents/04_全端功能詳細範例_數據統計分析報表.md`

#### 2.2.1 複雜統計查詢

```csharp
/// <summary>
/// 使用 Direct SQL 進行複雜的統計分析
/// 展示 GROUP BY、CASE WHEN、聚合函數的使用
/// </summary>
/// <param name="model">統計查詢條件</param>
/// <param name="sessiondata">使用者 Session 資料</param>
/// <returns>統計分析結果</returns>
private async Task<MIT0201SummaryDataModel> GetSummaryStatistics(MIT0201StatisticsQueryModel model, MitfSessionDataModel sessiondata)
{
    // 第一步：建立複雜的統計查詢 SQL
    // 使用 CASE WHEN 進行條件統計，一次查詢取得所有摘要數據
    var summaryQuery = @"
SELECT
    COUNT(*) as TotalCount,                                    -- 總申請案件數
    SUM(CASE WHEN af.FormProcess = 'C' THEN 1 ELSE 0 END) as ApprovedCount,    -- 已核准案件數
    SUM(CASE WHEN af.FormProcess = 'F' THEN 1 ELSE 0 END) as RejectedCount,    -- 已駁回案件數
    SUM(CASE WHEN af.FormProcess IN ('T','B','U','M') THEN 1 ELSE 0 END) as PendingCount,  -- 審核中案件數
    AVG(CASE
        WHEN af.FormProcess = 'C' AND af.ModifyDate IS NOT NULL
        THEN DATEDIFF(day, af.CreateDate, af.ModifyDate)
        ELSE NULL
    END) as AvgProcessDays                                     -- 平均處理天數（僅計算已完成案件）
FROM ApplyForm af
    INNER JOIN SysCode sc ON af.FormType = sc.Code AND sc.Kind = 'FORMTYPE'
WHERE af.ComId = @comid                                        -- 公司篩選條件
    AND af.IsDelete = 'N'                                      -- 排除已刪除資料
    AND af.CreateDate >= @startdate                            -- 開始日期條件
    AND af.CreateDate <= @enddate                              -- 結束日期條件
    AND (@formtype IS NULL OR af.FormType = @formtype)        -- 申請類型篩選（可選）
    AND (@formprocess IS NULL OR af.FormProcess = @formprocess) -- 申請狀態篩選（可選）
";

    // 第二步：執行統計查詢
    using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
    {
        conn.Open();
        using (var cmd = new SqlCommand(summaryQuery, conn))
        {
            // 第三步：設定查詢參數
            cmd.Parameters.AddWithValue("@comid", sessiondata.ComId);
            cmd.Parameters.AddWithValue("@startdate", DateTime.Parse(model.StartDate));
            cmd.Parameters.AddWithValue("@enddate", DateTime.Parse(model.EndDate));
            cmd.Parameters.AddWithValue("@formtype", string.IsNullOrEmpty(model.FormType) ? DBNull.Value : model.FormType);
            cmd.Parameters.AddWithValue("@formprocess", string.IsNullOrEmpty(model.FormProcess) ? DBNull.Value : model.FormProcess);

            // 第四步：執行查詢並讀取結果
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var summaryData = new MIT0201SummaryDataModel()
                    {
                        TotalCount = reader.GetInt32("TotalCount"),
                        ApprovedCount = reader.GetInt32("ApprovedCount"),
                        RejectedCount = reader.GetInt32("RejectedCount"),
                        PendingCount = reader.GetInt32("PendingCount"),
                        AvgProcessDays = reader.IsDBNull("AvgProcessDays") ? 0 : reader.GetDouble("AvgProcessDays")
                    };

                    // 第五步：計算衍生統計數據
                    if (summaryData.TotalCount > 0)
                    {
                        summaryData.ApprovalRate = (double)summaryData.ApprovedCount / summaryData.TotalCount * 100;
                    }

                    return summaryData;
                }
            }
        }
    }

    return new MIT0201SummaryDataModel();
}
```

**程式碼說明：**

1. **聚合函數**：使用 COUNT、SUM、AVG 進行統計計算
2. **條件聚合**：使用 CASE WHEN 進行條件式統計
3. **日期函數**：使用 DATEDIFF 計算處理天數
4. **NULL 處理**：使用 DBNull.Value 和 IsDBNull 處理空值
5. **複雜條件**：使用 IN 和 OR 進行複雜條件篩選
6. **效能優化**：一次查詢取得所有統計數據，減少資料庫往返次數

### 2.3 批次更新操作範例

**檔案位置：** `MITAP2024/轉檔SQL/轉檔SQL.sql`

#### 2.3.1 大量資料批次更新

```sql
-- 使用 Direct SQL 進行批次更新操作
-- 展示複雜的批次更新邏輯和動態 SQL 生成

-- 第一步：批次更新預設聯絡人
-- 根據舊系統資料批次設定部門的預設聯絡人
UPDATE DeptUsers
SET IsContact = 1
WHERE DeptGuid IN (
    SELECT d.Guid
    FROM [2020mit_DATA].dbo.account_dept_main adm
        INNER JOIN [2020mit_DATA].dbo.account_user au ON au.dept_id = adm.dept_id AND au.user_limit = '1'
        INNER JOIN Departments d ON adm.dept_id = d.deptid
        INNER JOIN Users u ON u.UserAct = au.user_account
    WHERE DeptUsers.DeptGuid = d.Guid AND DeptUsers.UserGuid = u.Guid
);

-- 第二步：批次插入產品資料
-- 使用複雜的子查詢和條件判斷進行批次插入
INSERT INTO ProductData (
    Guid, FormGuid, ProductSysid, MitId, ProductId, ProductName,
    ProductType, ProductSpec, ProductPrice, CreateDate, CreateUser
)
SELECT
    NEWID() as Guid,
    af.Guid as FormGuid,
    pd.product_sysid as ProductSysid,
    pd.mit_id as MitId,
    pd.product_id as ProductId,
    pd.product_name as ProductName,
    pt.ProductTypeName as ProductType,
    pd.product_spec as ProductSpec,
    pd.product_price as ProductPrice,
    GETDATE() as CreateDate,
    'SYSTEM' as CreateUser
FROM [2020mit_DATA].dbo.product_data pd
    INNER JOIN ProductType pt ON pt.ProductTypeSysid = pd.product_type_id
    INNER JOIN ApplyForm af ON af.FormSysId = pd.form_sysid
WHERE pd.product_sysid NOT IN (
    -- 排除已存在的產品資料，避免重複插入
    SELECT DataNum06
    FROM ProductData pd2
    WHERE pd2.FormGuid = af.Guid
)
AND af.FormType IN ('a003', 'a004')  -- 只處理特定類型的申請案件
AND pd.is_delete = 'N';               -- 排除已刪除的產品

-- 第三步：批次更新序號計數器
-- 更新系統序號計數器，確保新資料的序號正確
UPDATE SnCounter
SET Seq = (SELECT MAX(ProductSysid) FROM ProductData)
WHERE [key] = 'ProductData' AND [Type] = 'M';

-- 第四步：複雜的條件批次更新
-- 根據複雜條件批次更新產品狀態
UPDATE MitProduct
SET ProductStatus = 'E',  -- 設定為過期狀態
    ModifyDate = GETDATE(),
    ModifyUser = 'SYSTEM'
WHERE ProductStatus = 'O'  -- 目前狀態為有效
    AND GETDATE() > DATEADD(day,
        CAST((SELECT CodeName FROM SysCode WHERE Kind = 'MITPARAMS' AND Code = 'EXTENDDAYS') AS int) * -1,
        MitEndDate)  -- 超過延展天數
    AND Guid IN (
        -- 只更新符合特定條件的產品
        SELECT mp.Guid
        FROM MitProduct mp
            INNER JOIN ProductType pt ON mp.ProductTypeGuid = pt.Guid
            INNER JOIN ProductIndustry pi2 ON pt.ProductIndustryTypeGuid = pi2.Guid
            INNER JOIN IndustryDetail id ON id.Guid = pi2.IndustryGuId
        WHERE id.IsBatchInspection = 'N'  -- 非逐批驗證
            AND (mp.MitId + '-' + mp.ProductId) NOT IN (
                -- 排除已有延展申請的產品
                SELECT (pd.MitId + '-' + pd.ProductId)
                FROM ProductData pd
                    INNER JOIN ApplyForm fm ON pd.FormGuid = fm.Guid
                WHERE fm.FormType = 'a005'
                    AND fm.FormProcess IN ('T','F','B','C','U')
            )
    );
```

**程式碼說明：**

1. **批次更新**：使用 UPDATE 搭配子查詢進行大量資料更新
2. **批次插入**：使用 INSERT INTO ... SELECT 進行大量資料插入
3. **複雜條件**：使用多層子查詢和 EXISTS/NOT IN 進行複雜條件判斷
4. **資料完整性**：透過條件檢查避免重複資料和無效更新
5. **效能優化**：使用集合操作而非逐筆處理，大幅提升效能
6. **交易安全**：批次操作具有原子性，確保資料一致性

#### 2.3.2 動態批次更新程式碼

```csharp
/// <summary>
/// 使用 Direct SQL 進行動態批次更新
/// 展示程式碼中的批次更新邏輯
/// </summary>
/// <param name="updateList">要更新的資料清單</param>
/// <param name="sessiondata">使用者 Session 資料</param>
/// <returns>更新結果</returns>
public async Task<ActionResult<PagedQueryResult>> BatchUpdateProductStatus(List<ProductUpdateModel> updateList, MitfSessionDataModel sessiondata)
{
    return await Task.Run(() =>
    {
        PagedQueryResult result = new PagedQueryResult()
        {
            IsSuccess = false
        };

        try
        {
            using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 第一步：建立批次更新的 SQL 語句
                        var batchUpdateSql = @"
UPDATE ProductData
SET ProductStatus = @status,
    ModifyDate = @modifydate,
    ModifyUser = @modifyuser
WHERE Guid = @guid
    AND FormGuid IN (
        SELECT Guid FROM ApplyForm
        WHERE ComId = @comid AND IsDelete = 'N'
    )";

                        // 第二步：準備批次更新命令
                        using (var cmd = new SqlCommand(batchUpdateSql, conn, transaction))
                        {
                            // 第三步：逐筆設定參數並執行更新
                            int updatedCount = 0;
                            foreach (var item in updateList)
                            {
                                // 清除之前的參數
                                cmd.Parameters.Clear();

                                // 設定當前項目的參數
                                cmd.Parameters.AddWithValue("@status", item.ProductStatus);
                                cmd.Parameters.AddWithValue("@modifydate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@modifyuser", sessiondata.Name);
                                cmd.Parameters.AddWithValue("@guid", Guid.Parse(item.Guid));
                                cmd.Parameters.AddWithValue("@comid", sessiondata.ComId);

                                // 執行更新並累計影響的資料筆數
                                int affectedRows = cmd.ExecuteNonQuery();
                                updatedCount += affectedRows;

                                // 記錄更新日誌
                                if (affectedRows > 0)
                                {
                                    logger.Info($"批次更新產品狀態：{item.Guid} -> {item.ProductStatus}");
                                }
                            }

                            // 第四步：檢查更新結果
                            if (updatedCount > 0)
                            {
                                // 提交交易
                                transaction.Commit();
                                result.IsSuccess = true;
                                result.message = $"成功更新 {updatedCount} 筆產品資料";

                                logger.Info($"批次更新完成，共更新 {updatedCount} 筆資料");
                            }
                            else
                            {
                                // 回滾交易
                                transaction.Rollback();
                                result.IsSuccess = false;
                                result.message = "沒有資料被更新";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 發生錯誤時回滾交易
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "批次更新產品狀態失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **交易管理**：使用 SqlTransaction 確保批次操作的原子性
2. **參數重用**：重複使用 SqlCommand 物件，提升效能
3. **錯誤處理**：發生錯誤時自動回滾交易，保持資料一致性
4. **日誌記錄**：詳細記錄批次更新的過程和結果
5. **影響筆數統計**：統計實際更新的資料筆數
6. **安全檢查**：透過子查詢確保只更新授權範圍內的資料

## 3. Entity Framework vs Direct SQL 對比分析

### 3.1 適用場景對比

| 比較項目            | Entity Framework | Direct SQL      |
| ------------------- | ---------------- | --------------- |
| **單一 Table CRUD** | ✅ 優秀          | ⚠️ 過度複雜     |
| **簡單關聯查詢**    | ✅ 優秀          | ⚠️ 可用但不必要 |
| **複雜統計查詢**    | ❌ 困難且效能差  | ✅ 優秀         |
| **批次更新操作**    | ❌ 效能差        | ✅ 優秀         |
| **動態查詢條件**    | ⚠️ 複雜          | ✅ 靈活         |
| **大量資料處理**    | ❌ 記憶體消耗大  | ✅ 高效能       |
| **複雜業務邏輯**    | ⚠️ 受限於 LINQ   | ✅ 完全控制     |

### 3.2 技術特點對比

#### 3.2.1 Entity Framework 優勢

```csharp
// 優勢 1：強型別安全
var user = _context.Users.Find(userId);  // 編譯時期檢查
user.Name = "新名稱";                      // IntelliSense 支援

// 優勢 2：自動變更追蹤
entity.PropertyName = newValue;           // 自動追蹤變更
_context.SaveChanges();                   // 自動產生 UPDATE SQL

// 優勢 3：LINQ 查詢語法
var result = _context.Users
    .Where(u => u.IsActive == true)       // 強型別條件
    .OrderBy(u => u.Name)                 // 型別安全排序
    .ToList();                            // 延遲執行

// 優勢 4：關聯資料載入
var userWithDepts = _context.Users
    .Include(u => u.DeptUsers)            // 預先載入關聯資料
    .ThenInclude(du => du.Department)     // 多層關聯載入
    .FirstOrDefault();
```

#### 3.2.2 Direct SQL 優勢

```csharp
// 優勢 1：完全控制 SQL 語句
var complexQuery = @"
SELECT u.Name, d.DisplayName, COUNT(af.Guid) as FormCount,
       AVG(DATEDIFF(day, af.CreateDate, af.ModifyDate)) as AvgDays
FROM Users u
    INNER JOIN DeptUsers du ON u.Guid = du.UserGuid
    INNER JOIN Departments d ON du.DeptGuid = d.Guid
    LEFT JOIN ApplyForm af ON af.CreateUser = u.Name
WHERE af.CreateDate >= @startDate
GROUP BY u.Name, d.DisplayName
HAVING COUNT(af.Guid) > 10
ORDER BY AvgDays DESC";

// 優勢 2：高效能批次操作
var batchInsert = @"
INSERT INTO ProductData (Guid, FormGuid, ProductName, CreateDate)
SELECT NEWID(), @formGuid, ProductName, GETDATE()
FROM TempProductImport
WHERE BatchId = @batchId";

// 優勢 3：複雜統計查詢
var statisticsQuery = @"
SELECT
    FormType,
    COUNT(*) as TotalCount,
    SUM(CASE WHEN FormProcess = 'C' THEN 1 ELSE 0 END) as ApprovedCount,
    AVG(CASE WHEN FormProcess = 'C'
        THEN DATEDIFF(day, CreateDate, ModifyDate)
        ELSE NULL END) as AvgProcessDays
FROM ApplyForm
WHERE CreateDate BETWEEN @startDate AND @endDate
GROUP BY FormType
ORDER BY TotalCount DESC";

// 優勢 4：動態 SQL 組合
if (!string.IsNullOrEmpty(searchName))
{
    query += " AND u.Name LIKE @searchName";
    parameters.Add(new SqlParameter("@searchName", $"%{searchName}%"));
}
```

### 3.3 效能對比分析

#### 3.3.1 記憶體使用對比

```csharp
// Entity Framework：載入大量資料時的記憶體消耗
var allUsers = _context.Users
    .Include(u => u.DeptUsers)
    .ThenInclude(du => du.Department)
    .ToList();  // 將所有資料載入記憶體，可能造成 OutOfMemoryException

// Direct SQL：串流式讀取，記憶體使用量固定
using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())  // 逐筆讀取，記憶體使用量恆定
    {
        // 處理單筆資料
        ProcessSingleRecord(reader);
    }
}
```

#### 3.3.2 查詢效能對比

```csharp
// Entity Framework：可能產生 N+1 查詢問題
foreach (var user in users)  // 主查詢：1 次
{
    var deptName = user.DeptUsers.First().Department.Name;  // 每個使用者：N 次查詢
}

// Direct SQL：一次查詢解決
var query = @"
SELECT u.Name, d.DisplayName
FROM Users u
    INNER JOIN DeptUsers du ON u.Guid = du.UserGuid
    INNER JOIN Departments d ON du.DeptGuid = d.Guid";  // 只有 1 次查詢
```

### 3.4 選擇建議與最佳實務

#### 3.4.1 選擇 Entity Framework 的情況

```csharp
// 情況 1：簡單的 CRUD 操作
public async Task<User> CreateUser(CreateUserModel model)
{
    var user = new User()
    {
        Guid = Guid.NewGuid(),
        UserAct = model.UserAct,
        Name = model.Name,
        // ... 其他屬性
    };

    _context.Users.Add(user);      // 簡潔明瞭
    await _context.SaveChangesAsync();
    return user;
}

// 情況 2：需要強型別安全的查詢
var activeUsers = await _context.Users
    .Where(u => u.IsActive == true)        // 編譯時期檢查
    .OrderBy(u => u.Name)                  // IntelliSense 支援
    .ToListAsync();

// 情況 3：需要變更追蹤的更新操作
var user = await _context.Users.FindAsync(userId);
user.Name = newName;                       // 自動追蹤變更
user.ModifyDate = DateTime.Now;
await _context.SaveChangesAsync();         // 自動產生最佳化的 UPDATE
```

#### 3.4.2 選擇 Direct SQL 的情況

```csharp
// 情況 1：複雜的統計分析查詢
var statisticsQuery = @"
SELECT
    d.DisplayName,
    COUNT(DISTINCT u.Guid) as UserCount,
    COUNT(af.Guid) as FormCount,
    AVG(CASE WHEN af.FormProcess = 'C'
        THEN DATEDIFF(day, af.CreateDate, af.ModifyDate)
        ELSE NULL END) as AvgProcessDays,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY
        CASE WHEN af.FormProcess = 'C'
        THEN DATEDIFF(day, af.CreateDate, af.ModifyDate)
        ELSE NULL END) as MedianProcessDays
FROM Departments d
    LEFT JOIN DeptUsers du ON d.Guid = du.DeptGuid
    LEFT JOIN Users u ON du.UserGuid = u.Guid
    LEFT JOIN ApplyForm af ON af.CreateUser = u.Name
WHERE af.CreateDate >= @startDate
GROUP BY d.DisplayName
HAVING COUNT(af.Guid) > 0
ORDER BY FormCount DESC";

// 情況 2：大量資料的批次處理
var batchUpdateQuery = @"
UPDATE ProductData
SET ProductStatus = 'EXPIRED',
    ModifyDate = GETDATE(),
    ModifyUser = 'SYSTEM'
WHERE ProductStatus = 'ACTIVE'
    AND EndDate < DATEADD(day, -30, GETDATE())
    AND Guid IN (
        SELECT pd.Guid
        FROM ProductData pd
            INNER JOIN ApplyForm af ON pd.FormGuid = af.Guid
        WHERE af.ComId = @comId
    )";

// 情況 3：動態查詢條件組合
var baseQuery = "SELECT * FROM Users WHERE 1=1";
var parameters = new List<SqlParameter>();

if (!string.IsNullOrEmpty(searchName))
{
    baseQuery += " AND Name LIKE @searchName";
    parameters.Add(new SqlParameter("@searchName", $"%{searchName}%"));
}

if (departmentId.HasValue)
{
    baseQuery += " AND Guid IN (SELECT UserGuid FROM DeptUsers WHERE DeptGuid = @deptGuid)";
    parameters.Add(new SqlParameter("@deptGuid", departmentId.Value));
}
```

### 3.5 混合使用策略

在 MITAP2024 系統中，最佳實務是根據具體需求混合使用兩種方法：

#### 3.5.1 分層使用策略

```csharp
public class MIT1001Service : BaseService, IMIT1001Service
{
    private readonly MainDbContext _context;

    // 使用 Entity Framework 處理簡單 CRUD
    public async Task<ActionResult<PagedQueryResult>> Create(MIT1001Model model, SessionDataModel? sessiondata)
    {
        var entity = new IndustryDetail()
        {
            // 設定屬性...
        };

        _context.IndustryDetail.Add(entity);    // EF 處理新增
        _context.SaveChanges();

        return result;
    }

    // 使用 Direct SQL 處理複雜查詢
    public async Task<ActionResult<PagedQueryResult>> GetStatisticsReport(StatisticsQueryModel model, SessionDataModel? sessiondata)
    {
        var complexQuery = @"
        SELECT id.IndustryName, COUNT(*) as ProductCount,
               AVG(pi.VerifyId) as AvgVerifyId
        FROM IndustryDetail id
            LEFT JOIN ProductIndustry pi ON id.Guid = pi.IndustryGuId
        WHERE id.CreateDate >= @startDate
        GROUP BY id.IndustryName
        ORDER BY ProductCount DESC";

        // Direct SQL 處理統計查詢
        using (var conn = new SqlConnection(connectionString))
        {
            // 執行複雜查詢...
        }

        return result;
    }
}
```

#### 3.5.2 效能優化策略

```csharp
// 策略 1：讀取使用 Direct SQL，寫入使用 Entity Framework
public class OptimizedService
{
    // 讀取：使用 Direct SQL 獲得最佳效能
    public async Task<List<ProductSummary>> GetProductSummary(string category)
    {
        var query = @"
        SELECT p.ProductName, p.ProductPrice, pt.ProductTypeName
        FROM ProductData p
            INNER JOIN ProductType pt ON p.ProductTypeGuid = pt.Guid
        WHERE pt.Category = @category
        ORDER BY p.ProductPrice DESC";

        // 使用 Direct SQL 快速讀取
    }

    // 寫入：使用 Entity Framework 確保資料完整性
    public async Task<bool> UpdateProduct(ProductUpdateModel model)
    {
        var product = await _context.ProductData.FindAsync(model.Guid);
        if (product != null)
        {
            product.ProductName = model.ProductName;
            product.ModifyDate = DateTime.Now;
            await _context.SaveChangesAsync();  // EF 確保變更追蹤
        }
        return true;
    }
}
```

## 4. 總結與建議

### 4.1 核心原則

1. **簡單操作使用 Entity Framework**：單一資料表的 CRUD 操作、簡單關聯查詢
2. **複雜操作使用 Direct SQL**：統計分析、批次處理、複雜業務邏輯
3. **效能優先使用 Direct SQL**：大量資料處理、高頻率查詢
4. **維護性優先使用 Entity Framework**：需要強型別安全、變更追蹤的場景

### 4.2 實務建議

1. **團隊技能考量**：Entity Framework 學習曲線較平緩，適合初級開發者
2. **專案時程考量**：Entity Framework 開發速度較快，適合快速原型開發
3. **效能需求考量**：Direct SQL 效能較佳，適合高負載系統
4. **維護成本考量**：Entity Framework 維護成本較低，適合長期維護的系統

### 4.3 MITAP2024 系統的最佳實務

根據 MITAP2024 系統的實際應用，建議採用以下策略：

- **基本資料維護**（如部門、使用者、系統代碼）：使用 Entity Framework
- **申請案件查詢**：使用 Direct SQL 處理複雜條件和分頁
- **統計報表**：使用 Direct SQL 進行高效能統計分析
- **批次資料處理**：使用 Direct SQL 進行大量資料更新
- **檔案上傳和基本 CRUD**：使用 Entity Framework 確保資料完整性

這種混合策略既能發揮 Entity Framework 的開發效率優勢，又能在需要時利用 Direct SQL 的效能和靈活性優勢，為 MITAP2024 系統提供最佳的資料庫存取解決方案。
