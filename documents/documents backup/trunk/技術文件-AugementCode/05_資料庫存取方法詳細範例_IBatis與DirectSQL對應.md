# 電子病歷申請系統 (EECOnline) - 資料庫存取方法詳細範例

## IBatis.NET 與 Direct SQL 對應

## 1. IBatis.NET 簡介

### 1.1 什麼是 IBatis.NET？

IBatis.NET 是一個輕量級的 ORM（Object-Relational Mapping）框架，它提供了以下特點：

- **SQL 與程式碼分離**：SQL 語句定義在 XML 檔案中，與程式碼分離
- **參數化查詢**：自動處理參數綁定，防止 SQL Injection
- **結果映射**：自動將查詢結果映射到物件
- **動態 SQL**：支援條件式 SQL 語句
- **快取機制**：支援查詢結果快取

### 1.2 與 Dapper 的比較

| 特性 | IBatis.NET | Dapper |
|------|-----------|--------|
| **SQL 定義位置** | XML 檔案 | C# 程式碼中 |
| **學習曲線** | 較陡峭（需學習 XML 設定） | 較平緩（直接寫 SQL） |
| **動態 SQL** | XML 標籤支援 | 需手動拼接字串 |
| **快取機制** | 內建支援 | 需自行實作 |
| **效能** | 良好 | 優秀 |
| **維護性** | SQL 集中管理 | SQL 分散在程式碼中 |

## 2. IBatis.NET 設定

### 2.1 SqlMap.config 主設定檔

**檔案位置：** `trunk/SqlMap.config`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<sqlMapConfig
  xmlns="http://ibatis.apache.org/dataMapper"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

    <properties resource="properties.config"/>

    <settings>
        <!-- 啟用命名空間，避免 SQL ID 衝突 -->
        <setting useStatementNamespaces="true"/>
    </settings>

    <providers resource="providers.config"/>

    <!-- 資料庫連線設定 -->
    <database>
        <!-- 使用 SQL Server 2005+ 提供者 -->
        <provider name="${providerSQLClient4}"/>
        <!-- 連線字串來自 Web.config -->
        <dataSource name="EECOnline" connectionString="${connectionEUSERVICE}" />
    </database>

    <!-- SQL Map 檔案清單 -->
    <sqlMaps>
        <!-- 基本屬性 -->
        <sqlMap resource="./SqlMaps/Commons.xml" />
        <!-- 共用代碼 -->
        <sqlMap resource="./SqlMaps/KeyMap.xml" />
        <!-- 共用功能 -->
        <sqlMap resource="./SqlMaps/SHARE.xml" />
        <!-- 權限管理 -->
        <sqlMap resource="./SqlMaps/AM.xml" />
        <!-- 報表 -->
        <sqlMap resource="./SqlMaps/Report.xml" />
        <!-- 登入 -->
        <sqlMap resource="./SqlMaps/Login.xml" />
        <!-- 首頁 -->
        <sqlMap resource="./SqlMaps/Front.xml" />
        <!-- 病歷設定管理 (A1) -->
        <sqlMap resource="./SqlMaps/A1.xml" />
        <!-- 申辦案件管理 (A2) -->
        <sqlMap resource="./SqlMaps/A2.xml" />
        <!-- 帳務計算管理 (A3) -->
        <sqlMap resource="./SqlMaps/A3.xml" />
        <!-- 報表統計管理 (A4) -->
        <sqlMap resource="./SqlMaps/A4.xml" />
        <!-- 帳號管理 (A5) -->
        <sqlMap resource="./SqlMaps/A5.xml" />
        <!-- 系統權限管理 (A6) -->
        <sqlMap resource="./SqlMaps/A6.xml" />
        <!-- 病歷設定管理 (A7) -->
        <sqlMap resource="./SqlMaps/A7.xml" />
        <!-- 紀錄查詢 (A8) -->
        <sqlMap resource="./SqlMaps/A8.xml" />
        <!-- 後台案件 -->
        <sqlMap resource="./SqlMaps/BackApply.xml" />
    </sqlMaps>

</sqlMapConfig>
```

### 2.2 Web.config 連線字串設定

```xml
<connectionStrings>
  <add name="connectionEUSERVICE" 
       connectionString="Data Source=YOUR_SERVER;initial catalog=EECOnline;user id=YOUR_USER;password=YOUR_PASSWORD;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

## 3. BaseDAO 基礎類別

### 3.1 BaseDAO 類別定義

**檔案位置：** `DataLayers/BaseDAO.cs`

```csharp
using EECOnline.Models;
using EECOnline.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using Turbo.DataLayer;

namespace EECOnline.DataLayers
{
    /// <summary>
    /// 資料存取基礎類別
    /// 繼承自 Turbo.DataLayer.RowBaseDAO，提供 IBatis.NET 的封裝
    /// </summary>
    public class BaseDAO : RowBaseDAO
    {
        /// <summary>
        /// 以預設的 SqlMap config 連接資料庫
        /// </summary>
        public BaseDAO() : base()
        {
            // 設定預設分頁大小
            base.PageSize = ConfigModel.DefaultPageSize;

            // 植入客制化資料異動記錄的功能
            base.SetExecuteTracert(new TransLogDAO());
        }

        /// <summary>
        /// 以指定的 SqlMap config 連接資料庫
        /// </summary>
        /// <param name="sqlMapConfig">SqlMap 設定檔路徑</param>
        public BaseDAO(string sqlMapConfig) : base(sqlMapConfig)
        {
            base.PageSize = ConfigModel.DefaultPageSize;

            // 植入客制化資料異動記錄的功能
            base.SetExecuteTracert(new TransLogDAO());
        }
    }
}
```

### 3.2 BaseDAO 提供的主要方法

| 方法名稱 | 說明 | 對應 SQL 操作 |
|---------|------|--------------|
| `GetRow(T where)` | 取得單筆資料 | SELECT TOP 1 |
| `GetRowList(T where)` | 取得多筆資料 | SELECT |
| `Insert(T row)` | 新增資料 | INSERT |
| `Update(T row, T where)` | 更新資料 | UPDATE |
| `Delete(T where)` | 刪除資料 | DELETE |
| `QueryForObject<T>(string sqlId, object param)` | 執行 SQL 取得單筆 | SELECT TOP 1 |
| `QueryForList<T>(string sqlId, object param)` | 執行 SQL 取得多筆 | SELECT |
| `QueryForListAll<T>(string sqlId, object param)` | 執行 SQL 取得所有資料 | SELECT |
| `Execute(string sqlId, object param)` | 執行 SQL（INSERT/UPDATE/DELETE） | INSERT/UPDATE/DELETE |

## 4. SQL Map XML 檔案結構

### 4.1 Front.xml 範例

**檔案位置：** `SqlMaps/Front.xml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<sqlMap namespace="Front" 
        xmlns="http://ibatis.apache.org/mapping" 
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    
    <!-- 結果映射定義 -->
    <resultMaps>
        <!-- 可定義複雜的結果映射 -->
    </resultMaps>
    
    <!-- 快取模型定義 -->
    <cacheModel id="WDASE-cache" implementation="MEMORY">
        <!-- 每小時清除快取 -->
        <flushInterval hours="1" />
        <!-- 使用弱引用 -->
        <property name="Type" value="WEAK" />
    </cacheModel>
    
    <!-- SQL 語句定義 -->
    <statements>
        <!-- 查詢語句範例 -->
        <select id="getSearchApplyList1" 
                resultClass="EECOnline.Models.SearchGridModel" 
                parameterClass="Hashtable" 
                cacheModel="WDASE-cache">
            <![CDATA[
            SELECT 
                a.keyid,
                a.apply_no, a.apply_no_sub, a.user_idno,
                a.hospital_code, a.hospital_name,
                a.his_range1, a.his_range2, a.his_types, a.pay_deadline,
                a.payed, a.payed_datetime,
                (SELECT SUM(b.price) FROM EEC_ApplyDetailPrice b 
                 WHERE b.apply_no=a.apply_no AND b.apply_no_sub=a.apply_no_sub) AS price_sum,
                (SELECT createdatetime FROM EEC_Apply c WHERE c.apply_no=a.apply_no) AS createdatetime,
                b.user_birthday
            FROM EEC_ApplyDetail a
            LEFT JOIN EEC_Apply b ON a.apply_no=b.apply_no
            WHERE 1=1
              AND a.user_idno=#user_idno#
            ]]>
            <!-- 動態 SQL -->
            <dynamic>
                <isParameterPresent>
                    <isNotEmpty prepend="" property="FilterMonth">
                        <![CDATA[
                        AND DATEDIFF(MONTH, (SELECT createdatetime FROM EEC_Apply c WHERE c.apply_no=a.apply_no), GETDATE()) <= $FilterMonth$
                        ]]>
                    </isNotEmpty>
                </isParameterPresent>
            </dynamic>
            <![CDATA[
            ORDER BY a.pay_deadline, a.keyid
            ]]>
        </select>
    </statements>
</sqlMap>
```

### 4.2 XML 標籤說明

#### 4.2.1 基本標籤

| 標籤 | 說明 | 範例 |
|------|------|------|
| `<sqlMap>` | SQL Map 根元素 | `<sqlMap namespace="Front">` |
| `<statements>` | SQL 語句集合 | `<statements>...</statements>` |
| `<select>` | 查詢語句 | `<select id="getUser">` |
| `<insert>` | 新增語句 | `<insert id="insertUser">` |
| `<update>` | 更新語句 | `<update id="updateUser">` |
| `<delete>` | 刪除語句 | `<delete id="deleteUser">` |

#### 4.2.2 參數綁定

| 語法 | 說明 | 範例 |
|------|------|------|
| `#參數名#` | 參數化查詢（推薦） | `WHERE user_idno=#user_idno#` |
| `$參數名$` | 字串替換（小心 SQL Injection） | `ORDER BY $orderBy$` |

**重要：** 
- 使用 `#參數名#` 會產生參數化查詢，防止 SQL Injection
- 使用 `$參數名$` 會直接替換字串，僅用於欄位名稱、排序等無法參數化的場景

#### 4.2.3 動態 SQL 標籤

| 標籤 | 說明 | 範例 |
|------|------|------|
| `<dynamic>` | 動態 SQL 區塊 | `<dynamic>...</dynamic>` |
| `<isParameterPresent>` | 參數存在時 | `<isParameterPresent>...</isParameterPresent>` |
| `<isNotEmpty>` | 參數不為空時 | `<isNotEmpty property="name">...</isNotEmpty>` |
| `<isEmpty>` | 參數為空時 | `<isEmpty property="name">...</isEmpty>` |
| `<isEqual>` | 參數等於某值時 | `<isEqual property="status" compareValue="A">...</isEqual>` |
| `<isNotEqual>` | 參數不等於某值時 | `<isNotEqual property="status" compareValue="D">...</isNotEqual>` |

## 5. FrontDAO 實作範例

### 5.1 FrontDAO 類別定義

**檔案位置：** `DataLayers/FrontDAO.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using EECOnline.Models;
using EECOnline.Models.Entities;

namespace EECOnline.DataLayers
{
    public class FrontDAO : BaseDAO
    {
        /// <summary>
        /// 取得申請案件清單（使用 IBatis SQL Map）
        /// </summary>
        /// <param name="TabType">頁籤類型（1:全部 2:待繳費 3:已繳費/逾期）</param>
        /// <param name="FilterModel">篩選條件</param>
        /// <returns>申請案件清單</returns>
        public IList<SearchGridModel> GetSearchApplyList(string TabType, SearchApplyModel FilterModel)
        {
            // 準備參數
            Hashtable Parmas = new Hashtable();
            Parmas["user_idno"] = FilterModel.user_idno;
            
            // 根據頁籤類型設定篩選月份
            switch (TabType)
            {
                case "1": Parmas["FilterMonth"] = FilterModel.Search1Filter; break;
                case "2": Parmas["FilterMonth"] = FilterModel.Search2Filter; break;
                case "3": Parmas["FilterMonth"] = FilterModel.Search3Filter; break;
                default: Parmas["FilterMonth"] = ""; break;
            }
            
            // 呼叫 IBatis SQL Map
            // 格式：命名空間.SQL ID
            return base.QueryForListAll<SearchGridModel>("Front.getSearchApplyList" + TabType, Parmas);
        }

        /// <summary>
        /// 取得首頁最新消息（使用 Entity 查詢）
        /// </summary>
        /// <returns>最新消息清單</returns>
        public IList<TblENEWS> GetHomeNews()
        {
            // 建立查詢條件
            TblENEWS where = new TblENEWS();
            where.publish_mk = "Y";  // 已發布
            where.del_mk = "N";      // 未刪除
            
            // 使用 BaseDAO 的 GetRowList 方法
            return base.GetRowList(where);
        }

        /// <summary>
        /// 取得申請主檔（使用 Entity 查詢）
        /// </summary>
        /// <param name="apply_no">申請單號</param>
        /// <returns>申請主檔</returns>
        public TblEEC_Apply GetApply(string apply_no)
        {
            TblEEC_Apply where = new TblEEC_Apply();
            where.apply_no = apply_no;
            
            // 使用 BaseDAO 的 GetRow 方法（取得單筆）
            return base.GetRow(where);
        }

        /// <summary>
        /// 新增申請主檔（使用 Entity 新增）
        /// </summary>
        /// <param name="apply">申請主檔</param>
        /// <returns>新增後的資料（含自動編號）</returns>
        public TblEEC_Apply InsertApply(TblEEC_Apply apply)
        {
            // 使用 BaseDAO 的 Insert 方法
            return base.Insert(apply);
        }

        /// <summary>
        /// 更新申請明細（使用 Entity 更新）
        /// </summary>
        /// <param name="apply_no_sub">申請單號（子檔）</param>
        /// <param name="payed">是否已繳費</param>
        /// <param name="payed_datetime">繳費時間</param>
        public void UpdateApplyDetailPayed(string apply_no_sub, string payed, string payed_datetime)
        {
            // 建立 WHERE 條件
            TblEEC_ApplyDetail where = new TblEEC_ApplyDetail();
            where.apply_no_sub = apply_no_sub;
            
            // 建立要更新的資料
            TblEEC_ApplyDetail row = new TblEEC_ApplyDetail();
            row.payed = payed;
            row.payed_datetime = payed_datetime;
            
            // 使用 BaseDAO 的 Update 方法
            base.Update(row, where);
        }

        /// <summary>
        /// 刪除申請明細（使用 Entity 刪除）
        /// </summary>
        /// <param name="apply_no_sub">申請單號（子檔）</param>
        public void DeleteApplyDetail(string apply_no_sub)
        {
            // 建立 WHERE 條件
            TblEEC_ApplyDetail where = new TblEEC_ApplyDetail();
            where.apply_no_sub = apply_no_sub;
            
            // 使用 BaseDAO 的 Delete 方法
            base.Delete(where);
        }
    }
}
```

## 6. IBatis 與 Direct SQL 對應範例

### 6.1 查詢操作（SELECT）

#### 6.1.1 IBatis 方式

**SQL Map (Front.xml):**
```xml
<select id="getSearchApplyList1" 
        resultClass="EECOnline.Models.SearchGridModel" 
        parameterClass="Hashtable">
    <![CDATA[
    SELECT 
        a.keyid, a.apply_no, a.apply_no_sub, a.user_idno,
        a.hospital_code, a.hospital_name
    FROM EEC_ApplyDetail a
    WHERE a.user_idno=#user_idno#
    ORDER BY a.keyid
    ]]>
</select>
```

**C# 程式碼:**
```csharp
Hashtable param = new Hashtable();
param["user_idno"] = "A123456789";
IList<SearchGridModel> result = dao.QueryForListAll<SearchGridModel>("Front.getSearchApplyList1", param);
```

#### 6.1.2 Direct SQL 方式（Dapper）

```csharp
using (var conn = new SqlConnection(connectionString))
{
    string sql = @"
        SELECT 
            a.keyid, a.apply_no, a.apply_no_sub, a.user_idno,
            a.hospital_code, a.hospital_name
        FROM EEC_ApplyDetail a
        WHERE a.user_idno=@user_idno
        ORDER BY a.keyid";
    
    var result = conn.Query<SearchGridModel>(sql, new { user_idno = "A123456789" }).ToList();
}
```

### 6.2 新增操作（INSERT）

#### 6.2.1 IBatis 方式（使用 Entity）

```csharp
TblEEC_Apply apply = new TblEEC_Apply();
apply.apply_no = "20240101120000000";
apply.user_idno = "A123456789";
apply.user_name = "王小明";
apply.login_type = "1";
apply.createdatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

FrontDAO dao = new FrontDAO();
TblEEC_Apply result = dao.Insert(apply);  // result 會包含自動產生的 keyid
```

#### 6.2.2 Direct SQL 方式（Dapper）

```csharp
using (var conn = new SqlConnection(connectionString))
{
    string sql = @"
        INSERT INTO EEC_Apply (apply_no, user_idno, user_name, login_type, createdatetime)
        VALUES (@apply_no, @user_idno, @user_name, @login_type, @createdatetime);
        SELECT CAST(SCOPE_IDENTITY() as bigint)";
    
    long keyid = conn.ExecuteScalar<long>(sql, new
    {
        apply_no = "20240101120000000",
        user_idno = "A123456789",
        user_name = "王小明",
        login_type = "1",
        createdatetime = DateTime.Now
    });
}
```

### 6.3 更新操作（UPDATE）

#### 6.3.1 IBatis 方式（使用 Entity）

```csharp
// WHERE 條件
TblEEC_ApplyDetail where = new TblEEC_ApplyDetail();
where.apply_no_sub = "20240101120000000A12345678900001";

// 要更新的資料
TblEEC_ApplyDetail row = new TblEEC_ApplyDetail();
row.payed = "Y";
row.payed_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

FrontDAO dao = new FrontDAO();
dao.Update(row, where);
```

#### 6.3.2 Direct SQL 方式（Dapper）

```csharp
using (var conn = new SqlConnection(connectionString))
{
    string sql = @"
        UPDATE EEC_ApplyDetail
        SET payed = @payed, payed_datetime = @payed_datetime
        WHERE apply_no_sub = @apply_no_sub";
    
    conn.Execute(sql, new
    {
        payed = "Y",
        payed_datetime = DateTime.Now,
        apply_no_sub = "20240101120000000A12345678900001"
    });
}
```

### 6.4 刪除操作（DELETE）

#### 6.4.1 IBatis 方式（使用 Entity）

```csharp
TblEEC_ApplyDetail where = new TblEEC_ApplyDetail();
where.apply_no_sub = "20240101120000000A12345678900001";

FrontDAO dao = new FrontDAO();
dao.Delete(where);
```

#### 6.4.2 Direct SQL 方式（Dapper）

```csharp
using (var conn = new SqlConnection(connectionString))
{
    string sql = "DELETE FROM EEC_ApplyDetail WHERE apply_no_sub = @apply_no_sub";
    conn.Execute(sql, new { apply_no_sub = "20240101120000000A12345678900001" });
}
```

## 7. 動態 SQL 範例

### 7.1 條件式查詢

**SQL Map (Front.xml):**
```xml
<select id="searchApply" resultClass="EECOnline.Models.SearchGridModel" parameterClass="Hashtable">
    <![CDATA[
    SELECT * FROM EEC_ApplyDetail
    WHERE 1=1
    ]]>
    <dynamic>
        <!-- 如果有提供 user_idno 參數 -->
        <isNotEmpty prepend="AND" property="user_idno">
            user_idno = #user_idno#
        </isNotEmpty>
        
        <!-- 如果有提供 hospital_code 參數 -->
        <isNotEmpty prepend="AND" property="hospital_code">
            hospital_code = #hospital_code#
        </isNotEmpty>
        
        <!-- 如果有提供 payed 參數 -->
        <isNotEmpty prepend="AND" property="payed">
            payed = #payed#
        </isNotEmpty>
        
        <!-- 如果有提供日期範圍 -->
        <isNotEmpty prepend="AND" property="date_start">
            <![CDATA[
            payed_datetime >= #date_start#
            ]]>
        </isNotEmpty>
        <isNotEmpty prepend="AND" property="date_end">
            <![CDATA[
            payed_datetime <= #date_end#
            ]]>
        </isNotEmpty>
    </dynamic>
    <![CDATA[
    ORDER BY keyid DESC
    ]]>
</select>
```

**C# 程式碼:**
```csharp
Hashtable param = new Hashtable();
param["user_idno"] = "A123456789";  // 必填
param["payed"] = "Y";               // 選填
// hospital_code 未提供，不會加入 WHERE 條件

IList<SearchGridModel> result = dao.QueryForListAll<SearchGridModel>("Front.searchApply", param);
```

**產生的 SQL:**
```sql
SELECT * FROM EEC_ApplyDetail
WHERE 1=1
  AND user_idno = @user_idno
  AND payed = @payed
ORDER BY keyid DESC
```

## 8. 總結

### 8.1 IBatis.NET 優點

1. **SQL 集中管理**：所有 SQL 語句集中在 XML 檔案中，易於維護
2. **動態 SQL 支援**：透過 XML 標籤實現複雜的條件式 SQL
3. **快取機制**：內建查詢結果快取，提升效能
4. **參數化查詢**：自動防止 SQL Injection
5. **結果映射**：自動將查詢結果映射到物件

### 8.2 使用建議

1. **簡單 CRUD**：使用 BaseDAO 的 Entity 方法（GetRow, Insert, Update, Delete）
2. **複雜查詢**：使用 SQL Map XML 定義查詢語句
3. **動態條件**：使用 IBatis 的動態 SQL 標籤
4. **效能優化**：善用 cacheModel 快取常用查詢結果

### 8.3 與 109_e-service (Dapper) 的差異

| 項目 | EECOnline (IBatis.NET) | 109_e-service (Dapper) |
|------|----------------------|----------------------|
| **SQL 位置** | XML 檔案 | C# 程式碼 |
| **動態 SQL** | XML 標籤 | 字串拼接 |
| **學習成本** | 較高 | 較低 |
| **維護性** | SQL 集中管理 | SQL 分散在程式碼中 |
| **適用場景** | 複雜查詢、動態條件 | 簡單查詢、高效能需求 |

兩種方式各有優缺點，IBatis.NET 適合需要集中管理 SQL 和複雜動態查詢的場景，而 Dapper 則適合追求簡潔和高效能的場景。

