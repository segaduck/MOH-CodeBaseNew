# 衛福部人民線上申辦系統 - 資料庫存取方法詳細範例：EF 與 Direct SQL 對應

## 功能概述

本文詳細說明 e-service 系統中的資料庫存取方法，包含直接 SQL 查詢（使用 SqlCommand 和 Dapper）的實作方式。系統主要使用 Direct SQL 方式進行資料庫操作，提供高效能和靈活性。

## 資料庫存取策略

```
e-service 資料庫存取策略：

Direct SQL + Dapper：
    ↓ 適用場景
所有 CRUD 操作、複雜查詢、統計分析
    ↓ 技術特點
原生 SQL、高效能、Dapper 物件對應
    ↓ 範例功能
申請案件管理、會員管理、報表查詢

Direct SQL + SqlCommand：
    ↓ 適用場景
批次更新、交易處理、複雜業務邏輯
    ↓ 技術特點
完全控制、交易管理、參數化查詢
    ↓ 範例功能
案件儲存、狀態更新、資料同步
```

## 1. Dapper 資料存取方法

### 1.1 Dapper 簡介

Dapper 是一個輕量級的 ORM（Object-Relational Mapping）框架，提供以下特點：

- **高效能**：接近原生 ADO.NET 的效能
- **簡單易用**：擴充 IDbConnection 介面，使用簡單
- **物件對應**：自動將查詢結果對應到物件
- **參數化查詢**：支援參數化查詢，防止 SQL Injection

### 1.2 Dapper 查詢範例

**檔案位置：** `ES/DataLayers/ApplyDAO.cs`

#### 1.2.1 查詢單筆資料

```csharp
/// <summary>
/// 使用 Dapper 查詢服務費用
/// </summary>
/// <param name="serviceId">服務代碼</param>
/// <returns>申請費用</returns>
public int? GetApplyFee(string serviceId)
{
    int? fee = 0;

    try
    {
        // 建立資料庫連線
        using (SqlConnection conn = DataUtils.GetConnection())
        {
            // 準備查詢參數
            var dictionary = new Dictionary<string, object>
            {
                { "@SRV_ID", serviceId }
            };
            var parameters = new DynamicParameters(dictionary);

            // SQL 查詢語句
            string _sql = @"SELECT APP_FEE FROM SERVICE WHERE SRV_ID = @SRV_ID";

            // 使用 Dapper 執行查詢
            // Query<T> 方法會自動將結果對應到 TblSERVICE 物件
            var result = conn.Query<TblSERVICE>(_sql, parameters).FirstOrDefault();

            if (result != null)
            {
                fee = result.APP_FEE;
            }

            conn.Close();
            conn.Dispose();
        }
    }
    catch (Exception ex)
    {
        logger.Warn(ex.Message, ex);
        throw new Exception("GetApplyFee failed:" + ex.Message, ex);
    }

    return fee;
}
```

**程式碼說明：**

1. **DynamicParameters**：Dapper 提供的參數類別，支援動態參數
2. **Query<T>**：Dapper 的查詢方法，自動對應到指定型別
3. **FirstOrDefault**：取得第一筆資料，沒有資料則回傳 null
4. **自動對應**：Dapper 自動將欄位對應到物件屬性（欄位名稱需相同）

#### 1.2.2 查詢多筆資料

**檔案位置：** `ES/DataLayers/LoginDAO.cs`

```csharp
/// <summary>
/// 使用 Dapper 查詢會員資料
/// </summary>
/// <param name="userNo">帳號</param>
/// <param name="userPwd">密碼（已加密）</param>
/// <returns>會員資料列表</returns>
public IList<ClamMember> GetMemberList(string userNo, string userPwd)
{
    // 準備查詢參數
    var dictionary = new Dictionary<string, object>
    {
        { "@ACC_NO", userNo },
        { "@PSWD", userPwd }
    };
    var parameters = new DynamicParameters(dictionary);

    IList<ClamMember> result = null;

    // SQL 查詢語句
    string _sql = @"
        SELECT
            ACC_NO,
            UPPER(IDN) AS IDN,
            SEX_CD,
            NAME,
            ENAME,
            CNT_NAME,
            CNT_ENAME,
            CNT_TEL,
            TEL,
            FAX,
            MAIL,
            CITY_CD,
            ADDR,
            BIRTHDAY,
            EADDR,
            MEDICO,
            CHR_NAME,
            CHR_ENAME
        FROM MEMBER
        WHERE 1=1
            AND ACC_NO = @ACC_NO
            AND PSWD = @PSWD";

    using (SqlConnection conn = DataUtils.GetConnection())
    {
        conn.Open();

        // 使用 Dapper 查詢，回傳 List
        result = conn.Query<ClamMember>(_sql.ToString(), parameters).ToList();

        conn.Close();
        conn.Dispose();
    }

    return result;
}
```

**程式碼說明：**

1. **ToList()**：將查詢結果轉換為 List 集合
2. **多欄位對應**：Dapper 自動對應所有欄位到物件屬性
3. **欄位別名**：使用 AS 可以將欄位對應到不同名稱的屬性
4. **WHERE 1=1**：方便動態組合查詢條件

#### 1.2.3 查詢單一值

```csharp
/// <summary>
/// 使用 Dapper 查詢會員資料（單筆）
/// </summary>
/// <param name="userNo">帳號</param>
/// <param name="userPwd_encry">密碼（已加密）</param>
/// <returns>會員資料</returns>
public TblMEMBER GetMember(string userNo, string userPwd_encry)
{
    var dictionary = new Dictionary<string, object>
    {
        { "@ACC_NO", userNo },
        { "@PSWD", userPwd_encry }
    };
    var parameters = new DynamicParameters(dictionary);

    TblMEMBER result = null;

    string _sql = @"
        SELECT *
        FROM MEMBER
        WHERE 1 = 1
            AND ISNULL(DEL_MK,'N') = 'N'
            AND ACC_NO = @ACC_NO";

    if (!string.IsNullOrEmpty(userPwd_encry))
    {
        _sql += " AND PSWD = @PSWD";
    }

    using (SqlConnection conn = DataUtils.GetConnection())
    {
        try
        {
            // QueryFirst：查詢第一筆資料，沒有資料會拋出例外
            // QueryFirstOrDefault：查詢第一筆資料，沒有資料回傳 null
            result = conn.QueryFirst<TblMEMBER>(_sql, parameters);
        }
        catch (Exception ex)
        {
            logger.Warn(ex.Message, ex);
            result = null;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    return result;
}
```

**程式碼說明：**

1. **QueryFirst vs QueryFirstOrDefault**：
   - `QueryFirst`：沒有資料會拋出例外
   - `QueryFirstOrDefault`：沒有資料回傳 null
2. **動態 SQL**：根據條件動態組合 SQL 語句
3. **ISNULL 函數**：處理 NULL 值的 SQL 函數

## 2. Direct SQL 資料存取方法

### 2.1 使用 SqlCommand 執行查詢

**檔案位置：** `ES/Utils/DataUtils.cs`

#### 2.1.1 查詢設定資料

```csharp
/// <summary>
/// 從資料庫讀取系統設定
/// 使用 SqlCommand 執行查詢
/// </summary>
/// <param name="key">設定鍵值</param>
/// <returns>設定值</returns>
public static string GetConfig(string key)
{
    if (config == null)
    {
        using (SqlConnection conn = GetConnection())
        {
            OpenDbConn(conn);  // 開啟連線

            // SQL 查詢語句
            string querySQL = "SELECT SETUP_CD, SETUP_VAL FROM SETUP WHERE DEL_MK = 'N'";

            // 建立 SqlCommand
            SqlCommand cmd = new SqlCommand(querySQL, conn);

            // 執行查詢並讀取資料
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                config = new Dictionary<string, string>();

                // 逐筆讀取資料
                while (dr.Read())
                {
                    // 使用工具方法安全讀取欄位值
                    config.Add(GetDBString(dr, 0), GetDBString(dr, 1));
                }

                dr.Close();
            }

            CloseDbConn(conn);  // 關閉連線
        }
    }

    // 回傳設定值
    if (config.ContainsKey(key))
    {
        return config[key];
    }

    logger.Debug("GetConfig: " + key + " NOT exists.");
    return "";
}
```

**程式碼說明：**

1. **SqlCommand**：ADO.NET 的命令物件，用於執行 SQL 語句
2. **SqlDataReader**：向前唯讀的資料讀取器，效能最佳
3. **GetDBString**：自訂工具方法，安全讀取欄位值並處理 NULL
4. **快取機制**：將設定資料快取在記憶體中，避免重複查詢

#### 2.1.2 參數化查詢

```csharp
/// <summary>
/// 使用參數化查詢取得付款帳號資訊
/// </summary>
/// <param name="serialNo">序號</param>
/// <returns>付款帳號資訊</returns>
public static Dictionary<string, string> GetPayAccount(int serialNo)
{
    Dictionary<string, string> item = new Dictionary<string, string>();

    using (SqlConnection conn = GetConnection())
    {
        OpenDbConn(conn);

        // 參數化 SQL 查詢
        string querySQL = @"
            SELECT ACCOUNT, PSWD, OID, SID, PAY_DESC
            FROM PAY_ACCOUNT
            WHERE SRL_NO = @SRL_NO";

        SqlCommand cmd = new SqlCommand(querySQL, conn);

        // 加入參數（防止 SQL Injection）
        AddParameters(cmd, "SRL_NO", serialNo);

        using (SqlDataReader dr = cmd.ExecuteReader())
        {
            while (dr.Read())
            {
                item.Add("ACCOUNT", GetDBString(dr, 0));
                item.Add("PSWD", GetDBString(dr, 1));
                item.Add("OID", GetDBString(dr, 2));
                item.Add("SID", GetDBString(dr, 3));
                item.Add("PAY_DESC", GetDBString(dr, 4));
            }
            dr.Close();
        }

        CloseDbConn(conn);
    }

    return item;
}
```

**程式碼說明：**

1. **參數化查詢**：使用 `@參數名稱` 佔位符
2. **AddParameters**：自訂工具方法，加入 SqlParameter
3. **SQL Injection 防護**：參數化查詢可防止 SQL Injection 攻擊
4. **型別安全**：參數會自動轉換為正確的 SQL 型別

### 2.2 使用 SqlCommand 執行更新

**檔案位置：** `ES/DataLayers/ApplyDAO.cs`

#### 2.2.1 新增資料

```csharp
/// <summary>
/// 儲存申請案件主檔
/// 使用 SqlCommand 執行 INSERT 或 UPDATE
/// </summary>
/// <param name="model">申請案件資料</param>
/// <returns>影響的資料筆數</returns>
public int SaveApply_001008(Apply_001008Model model)
{
    int result = 0;

    try
    {
        using (SqlConnection conn = DataUtils.GetConnection())
        {
            conn.Open();

            // 檢查資料是否已存在
            string checkSql = "SELECT COUNT(*) FROM Apply_001008 WHERE APP_ID = @APP_ID";
            SqlCommand checkCmd = new SqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@APP_ID", model.APP_ID);

            int count = (int)checkCmd.ExecuteScalar();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;

            if (count > 0)
            {
                // 更新現有資料
                cmd.CommandText = @"
                    UPDATE Apply_001008 SET
                        NAME = @NAME,
                        ENAME = @ENAME,
                        IDN = @IDN,
                        TEL = @TEL,
                        EMAIL = @EMAIL,
                        ADDR_ZIP_ADDR = @ADDR_ZIP_ADDR,
                        ADDR_ZIP_DETAIL = @ADDR_ZIP_DETAIL,
                        APP_STATUS = @APP_STATUS,
                        UPD_TIME = GETDATE(),
                        UPD_USER = @UPD_USER
                    WHERE APP_ID = @APP_ID";
            }
            else
            {
                // 新增資料
                cmd.CommandText = @"
                    INSERT INTO Apply_001008 (
                        APP_ID, NAME, ENAME, IDN, TEL, EMAIL,
                        ADDR_ZIP_ADDR, ADDR_ZIP_DETAIL, APP_STATUS,
                        CRE_TIME, CRE_USER, DEL_MK
                    ) VALUES (
                        @APP_ID, @NAME, @ENAME, @IDN, @TEL, @EMAIL,
                        @ADDR_ZIP_ADDR, @ADDR_ZIP_DETAIL, @APP_STATUS,
                        GETDATE(), @CRE_USER, 'N'
                    )";
            }

            // 加入參數
            cmd.Parameters.AddWithValue("@APP_ID", model.APP_ID ?? "");
            cmd.Parameters.AddWithValue("@NAME", model.NAME ?? "");
            cmd.Parameters.AddWithValue("@ENAME", model.ENAME ?? "");
            cmd.Parameters.AddWithValue("@IDN", model.IDN ?? "");
            cmd.Parameters.AddWithValue("@TEL", model.TEL ?? "");
            cmd.Parameters.AddWithValue("@EMAIL", model.EMAIL ?? "");
            cmd.Parameters.AddWithValue("@ADDR_ZIP_ADDR", model.ADDR_ZIP_ADDR ?? "");
            cmd.Parameters.AddWithValue("@ADDR_ZIP_DETAIL", model.ADDR_ZIP_DETAIL ?? "");
            cmd.Parameters.AddWithValue("@APP_STATUS", model.APP_STATUS ?? "");
            cmd.Parameters.AddWithValue("@CRE_USER", model.CRE_USER ?? "");
            cmd.Parameters.AddWithValue("@UPD_USER", model.UPD_USER ?? "");

            // 執行 SQL 命令
            result = cmd.ExecuteNonQuery();

            conn.Close();
        }
    }
    catch (Exception ex)
    {
        logger.Error("儲存申請案件失敗", ex);
        throw;
    }

    return result;
}
```

**程式碼說明：**

1. **ExecuteScalar**：執行查詢並回傳第一列第一欄的值
2. **ExecuteNonQuery**：執行 INSERT、UPDATE、DELETE，回傳影響的資料筆數
3. **NULL 處理**：使用 `??` 運算子處理 NULL 值
4. **GETDATE()**：SQL Server 函數，取得目前日期時間

#### 2.2.2 批次刪除與新增

```csharp
/// <summary>
/// 儲存醫事人員明細資料
/// 先刪除舊資料，再批次新增新資料
/// </summary>
/// <param name="APP_ID">案件編號</param>
/// <param name="meList">醫事人員清單</param>
/// <returns>影響的資料筆數</returns>
public int SaveApply_001008_Me(string APP_ID, List<Apply_001008_MeModel> meList)
{
    int result = 0;

    try
    {
        using (SqlConnection conn = DataUtils.GetConnection())
        {
            conn.Open();

            // 開始交易
            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                // 第一步：刪除舊資料
                string deleteSql = "DELETE FROM Apply_001008_Me WHERE APP_ID = @APP_ID";
                SqlCommand deleteCmd = new SqlCommand(deleteSql, conn, transaction);
                deleteCmd.Parameters.AddWithValue("@APP_ID", APP_ID);
                deleteCmd.ExecuteNonQuery();

                // 第二步：批次新增新資料
                if (meList != null && meList.Count > 0)
                {
                    foreach (var me in meList)
                    {
                        string insertSql = @"
                            INSERT INTO Apply_001008_Me (
                                APP_ID, ME_TYPE, ME_NO, ME_NAME, ME_ENAME, ME_DATE,
                                CRE_TIME, CRE_USER, DEL_MK
                            ) VALUES (
                                @APP_ID, @ME_TYPE, @ME_NO, @ME_NAME, @ME_ENAME, @ME_DATE,
                                GETDATE(), @CRE_USER, 'N'
                            )";

                        SqlCommand insertCmd = new SqlCommand(insertSql, conn, transaction);
                        insertCmd.Parameters.AddWithValue("@APP_ID", APP_ID);
                        insertCmd.Parameters.AddWithValue("@ME_TYPE", me.ME_TYPE ?? "");
                        insertCmd.Parameters.AddWithValue("@ME_NO", me.ME_NO ?? "");
                        insertCmd.Parameters.AddWithValue("@ME_NAME", me.ME_NAME ?? "");
                        insertCmd.Parameters.AddWithValue("@ME_ENAME", me.ME_ENAME ?? "");
                        insertCmd.Parameters.AddWithValue("@ME_DATE",
                            me.ME_DATE.HasValue ? (object)me.ME_DATE.Value : DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@CRE_USER", me.CRE_USER ?? "");

                        result += insertCmd.ExecuteNonQuery();
                    }
                }

                // 提交交易
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // 發生錯誤，回復交易
                transaction.Rollback();
                logger.Error("儲存醫事人員明細失敗", ex);
                throw;
            }

            conn.Close();
        }
    }
    catch (Exception ex)
    {
        logger.Error("儲存醫事人員明細失敗", ex);
        throw;
    }

    return result;
}
```

**程式碼說明：**

1. **交易處理**：使用 `BeginTransaction` 確保資料一致性
2. **先刪後增**：先刪除舊資料，再新增新資料
3. **批次處理**：使用迴圈批次新增多筆資料
4. **DBNull.Value**：處理 NULL 值的正確方式
5. **Commit/Rollback**：成功提交，失敗回復

## 3. 資料讀取工具方法

### 3.1 安全讀取欄位值

**檔案位置：** `ES/Utils/DataUtils.cs`

```csharp
/// <summary>
/// 安全讀取字串欄位
/// 處理 NULL 值和 DBNull
/// </summary>
public static string GetDBString(SqlDataReader dr, int index)
{
    if (dr.IsDBNull(index))
    {
        return "";
    }
    return dr.GetString(index);
}

/// <summary>
/// 安全讀取字串欄位（使用欄位名稱）
/// </summary>
public static string GetDBString(SqlDataReader dr, string columnName)
{
    int index = dr.GetOrdinal(columnName);
    return GetDBString(dr, index);
}

/// <summary>
/// 安全讀取整數欄位
/// </summary>
public static int GetDBInt(SqlDataReader dr, int index)
{
    if (dr.IsDBNull(index))
    {
        return 0;
    }
    return dr.GetInt32(index);
}

/// <summary>
/// 安全讀取整數欄位（使用欄位名稱）
/// </summary>
public static int GetDBInt(SqlDataReader dr, string columnName)
{
    int index = dr.GetOrdinal(columnName);
    return GetDBInt(dr, index);
}

/// <summary>
/// 安全讀取日期時間欄位
/// </summary>
public static DateTime? GetDBDateTime(SqlDataReader dr, int index)
{
    if (dr.IsDBNull(index))
    {
        return null;
    }
    return dr.GetDateTime(index);
}

/// <summary>
/// 安全讀取日期時間欄位（使用欄位名稱）
/// </summary>
public static DateTime? GetDBDateTime(SqlDataReader dr, string columnName)
{
    int index = dr.GetOrdinal(columnName);
    return GetDBDateTime(dr, index);
}

/// <summary>
/// 安全讀取 Decimal 欄位
/// </summary>
public static decimal GetDBDecimal(SqlDataReader dr, int index)
{
    if (dr.IsDBNull(index))
    {
        return 0;
    }
    return dr.GetDecimal(index);
}
```

**程式碼說明：**

1. **IsDBNull**：檢查欄位是否為 NULL
2. **GetOrdinal**：取得欄位索引
3. **預設值**：NULL 值回傳預設值（空字串、0、null）
4. **多載方法**：支援索引和欄位名稱兩種方式

### 3.2 參數處理工具方法

```csharp
/// <summary>
/// 加入 SQL 參數
/// </summary>
public static void AddParameters(SqlCommand cmd, string paramName, object value)
{
    if (value == null)
    {
        cmd.Parameters.AddWithValue("@" + paramName, DBNull.Value);
    }
    else
    {
        cmd.Parameters.AddWithValue("@" + paramName, value);
    }
}

/// <summary>
/// 加入 SQL 參數（指定型別）
/// </summary>
public static void AddParameters(SqlCommand cmd, string paramName, object value, SqlDbType dbType)
{
    SqlParameter param = new SqlParameter("@" + paramName, dbType);
    param.Value = value ?? DBNull.Value;
    cmd.Parameters.Add(param);
}

/// <summary>
/// 加入 SQL 參數（指定型別和大小）
/// </summary>
public static void AddParameters(SqlCommand cmd, string paramName, object value,
    SqlDbType dbType, int size)
{
    SqlParameter param = new SqlParameter("@" + paramName, dbType, size);
    param.Value = value ?? DBNull.Value;
    cmd.Parameters.Add(param);
}
```

**程式碼說明：**

1. **NULL 處理**：自動將 null 轉換為 DBNull.Value
2. **型別指定**：可指定 SQL 資料型別
3. **大小指定**：可指定欄位大小（VARCHAR、NVARCHAR 等）
4. **@ 符號**：自動加上參數前綴

## 4. 連線管理

### 4.1 取得資料庫連線

```csharp
/// <summary>
/// 取得資料庫連線
/// 從 Web.config 讀取連線字串
/// </summary>
public static SqlConnection GetConnection()
{
    string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    return new SqlConnection(connStr);
}

/// <summary>
/// 取得 MDOD 資料庫連線
/// </summary>
public static SqlConnection GetMDODConnection()
{
    string connStr = ConfigurationManager.ConnectionStrings["MDODConnection"].ConnectionString;
    return new SqlConnection(connStr);
}

/// <summary>
/// 開啟資料庫連線
/// 加入錯誤處理和日誌記錄
/// </summary>
public static void OpenDbConn(SqlConnection conn)
{
    try
    {
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
    }
    catch (Exception ex)
    {
        logger.Error("開啟資料庫連線失敗", ex);
        throw;
    }
}

/// <summary>
/// 關閉資料庫連線
/// </summary>
public static void CloseDbConn(SqlConnection conn)
{
    try
    {
        if (conn != null && conn.State != ConnectionState.Closed)
        {
            conn.Close();
        }
    }
    catch (Exception ex)
    {
        logger.Error("關閉資料庫連線失敗", ex);
    }
}
```

**程式碼說明：**

1. **連線字串**：從 Web.config 讀取
2. **狀態檢查**：開啟前檢查連線狀態
3. **錯誤處理**：記錄錯誤日誌
4. **多資料庫**：支援多個資料庫連線

## 5. Dapper vs Direct SQL 比較

### 5.1 查詢比較

#### 5.1.1 Dapper 查詢

```csharp
// Dapper 查詢 - 簡潔易讀
public List<Apply_001008Model> GetApplyList_Dapper(string status)
{
    using (SqlConnection conn = DataUtils.GetConnection())
    {
        string sql = @"
            SELECT * FROM Apply_001008
            WHERE APP_STATUS = @STATUS AND DEL_MK = 'N'
            ORDER BY CRE_TIME DESC";

        var parameters = new { STATUS = status };

        return conn.Query<Apply_001008Model>(sql, parameters).ToList();
    }
}
```

#### 5.1.2 Direct SQL 查詢

```csharp
// Direct SQL 查詢 - 完全控制
public List<Apply_001008Model> GetApplyList_DirectSQL(string status)
{
    List<Apply_001008Model> list = new List<Apply_001008Model>();

    using (SqlConnection conn = DataUtils.GetConnection())
    {
        conn.Open();

        string sql = @"
            SELECT * FROM Apply_001008
            WHERE APP_STATUS = @STATUS AND DEL_MK = 'N'
            ORDER BY CRE_TIME DESC";

        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@STATUS", status);

        using (SqlDataReader dr = cmd.ExecuteReader())
        {
            while (dr.Read())
            {
                Apply_001008Model model = new Apply_001008Model
                {
                    APP_ID = DataUtils.GetDBString(dr, "APP_ID"),
                    NAME = DataUtils.GetDBString(dr, "NAME"),
                    ENAME = DataUtils.GetDBString(dr, "ENAME"),
                    APP_STATUS = DataUtils.GetDBString(dr, "APP_STATUS"),
                    CRE_TIME = DataUtils.GetDBDateTime(dr, "CRE_TIME")
                    // ... 其他欄位
                };
                list.Add(model);
            }
        }

        conn.Close();
    }

    return list;
}
```

### 5.2 優缺點比較

| 特性             | Dapper           | Direct SQL (SqlCommand) |
| ---------------- | ---------------- | ----------------------- |
| **程式碼簡潔度** | ★★★★★ 非常簡潔   | ★★★☆☆ 較冗長            |
| **效能**         | ★★★★★ 接近原生   | ★★★★★ 原生效能          |
| **物件對應**     | ★★★★★ 自動對應   | ★★☆☆☆ 手動對應          |
| **靈活性**       | ★★★★☆ 高度靈活   | ★★★★★ 完全控制          |
| **學習曲線**     | ★★★★☆ 容易學習   | ★★★☆☆ 需要經驗          |
| **除錯難度**     | ★★★★☆ 容易除錯   | ★★★☆☆ 較難除錯          |
| **型別安全**     | ★★★★☆ 編譯時檢查 | ★★☆☆☆ 執行時檢查        |

### 5.3 使用建議

#### 5.3.1 建議使用 Dapper 的情況

```csharp
// 1. 簡單的 CRUD 操作
var user = conn.QueryFirst<Member>("SELECT * FROM MEMBER WHERE ACC_NO = @ACC_NO",
    new { ACC_NO = accNo });

// 2. 查詢結果直接對應到物件
var list = conn.Query<Apply_001008Model>(
    "SELECT * FROM Apply_001008 WHERE APP_STATUS = @STATUS",
    new { STATUS = "P" }).ToList();

// 3. 多表關聯查詢
var result = conn.Query<ApplyViewModel>(@"
    SELECT A.*, S.SRV_NAME, M.NAME AS MEMBER_NAME
    FROM APPLY A
    LEFT JOIN SERVICE S ON A.SRV_ID = S.SRV_ID
    LEFT JOIN MEMBER M ON A.ACC_NO = M.ACC_NO
    WHERE A.APP_ID = @APP_ID",
    new { APP_ID = appId }).FirstOrDefault();
```

#### 5.3.2 建議使用 Direct SQL 的情況

```csharp
// 1. 需要精確控制交易
using (SqlConnection conn = DataUtils.GetConnection())
{
    conn.Open();
    SqlTransaction trans = conn.BeginTransaction();
    try
    {
        // 多個操作在同一交易中
        SqlCommand cmd1 = new SqlCommand(sql1, conn, trans);
        cmd1.ExecuteNonQuery();

        SqlCommand cmd2 = new SqlCommand(sql2, conn, trans);
        cmd2.ExecuteNonQuery();

        trans.Commit();
    }
    catch
    {
        trans.Rollback();
        throw;
    }
}

// 2. 需要使用 OUTPUT 參數
SqlCommand cmd = new SqlCommand("INSERT INTO ... OUTPUT INSERTED.ID", conn);
int newId = (int)cmd.ExecuteScalar();

// 3. 需要處理複雜的資料型別
SqlParameter param = new SqlParameter("@Data", SqlDbType.Xml);
param.Value = xmlData;
cmd.Parameters.Add(param);
```

## 6. 最佳實務

### 6.1 連線管理

```csharp
// 好的做法：使用 using 自動釋放資源
using (SqlConnection conn = DataUtils.GetConnection())
{
    conn.Open();
    // 執行操作
}  // 自動關閉連線

// 不好的做法：手動管理連線
SqlConnection conn = DataUtils.GetConnection();
conn.Open();
// 執行操作
conn.Close();  // 可能因例外而未執行
```

### 6.2 參數化查詢

```csharp
// 好的做法：使用參數化查詢
string sql = "SELECT * FROM MEMBER WHERE ACC_NO = @ACC_NO";
cmd.Parameters.AddWithValue("@ACC_NO", accNo);

// 不好的做法：字串串接（有 SQL Injection 風險）
string sql = "SELECT * FROM MEMBER WHERE ACC_NO = '" + accNo + "'";
```

### 6.3 錯誤處理

```csharp
// 好的做法：完整的錯誤處理
try
{
    using (SqlConnection conn = DataUtils.GetConnection())
    {
        conn.Open();
        // 執行操作
    }
}
catch (SqlException ex)
{
    logger.Error("資料庫操作失敗", ex);
    throw new ApplicationException("資料庫操作失敗，請稍後再試", ex);
}
catch (Exception ex)
{
    logger.Error("未預期的錯誤", ex);
    throw;
}
```

## 7. 總結

e-service 系統的資料庫存取策略：

### 7.1 技術特色

- **Dapper 為主**：大部分查詢使用 Dapper，簡潔高效
- **Direct SQL 為輔**：複雜交易使用 Direct SQL，完全控制
- **參數化查詢**：所有查詢都使用參數化，防止 SQL Injection
- **工具方法**：提供完整的工具方法，簡化開發

### 7.2 效能優化

- **連線池**：使用 ADO.NET 連線池管理連線
- **參數重用**：使用 DynamicParameters 重用參數
- **批次處理**：大量資料使用批次處理
- **交易管理**：適當使用交易確保資料一致性

### 7.3 安全性

- **參數化查詢**：防止 SQL Injection
- **權限控制**：資料庫層級的權限控制
- **錯誤處理**：完整的錯誤處理和日誌記錄
- **連線加密**：使用 SSL/TLS 加密連線

這套資料庫存取方法為團隊提供了高效、安全、易維護的資料存取解決方案。
