# 衛福部人民線上申辦系統 - 側邊選單權限控制詳細範例：Menu 與 Permission 設計

## 功能概述

本文詳細說明 e-service 系統中的側邊選單權限控制機制，包含選單資料結構、權限管理、動態選單產生等功能。系統使用資料庫驅動的選單系統，結合使用者權限控制，提供彈性的選單管理機制。

## 選單權限架構

```
選單權限控制架構：

ADMIN_MENU (選單主檔)
    ↓ 階層式結構
選單項目 (MN_ID, MN_PID)
    ↓ 關聯
ADMIN_LEVEL (使用者權限)
    ↓ 權限檢查
動態選單產生
    ↓ 前端渲染
側邊選單顯示
```

## 1. 選單資料結構

### 1.1 ADMIN_MENU 資料表

**檔案位置：** `ES/Models/Entities/ADMIN_MENU.cs`

#### 1.1.1 選單資料模型

```csharp
/// <summary>
/// 管理者選單資料表
/// 儲存系統所有選單項目
/// </summary>
public class TblADMIN_MENU
{
    /// <summary>
    /// 選單編號（主鍵）
    /// </summary>
    public int? MN_ID { get; set; }

    /// <summary>
    /// 選單文字
    /// </summary>
    public string MN_TEXT { get; set; }

    /// <summary>
    /// 父選單編號
    /// 0 表示第一層選單
    /// </summary>
    public int? MN_PID { get; set; }

    /// <summary>
    /// 選單類型
    /// F: 功能選單（有連結）
    /// G: 群組選單（無連結，僅分類用）
    /// </summary>
    public string MN_TYPE { get; set; }

    /// <summary>
    /// 開啟目標
    /// _self: 同視窗
    /// _blank: 新視窗
    /// </summary>
    public string MN_TARGET { get; set; }

    /// <summary>
    /// 選單連結 URL
    /// </summary>
    public string MN_URL { get; set; }

    /// <summary>
    /// 排序序號
    /// </summary>
    public int? SEQ_NO { get; set; }

    /// <summary>
    /// 刪除標記
    /// N: 正常
    /// Y: 已刪除
    /// </summary>
    public string DEL_MK { get; set; }

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DEL_TIME { get; set; }
}
```

**資料表說明：**

1. **階層式結構**：使用 MN_PID 建立父子關係
2. **選單類型**：區分功能選單和群組選單
3. **排序控制**：使用 SEQ_NO 控制顯示順序
4. **軟刪除**：使用 DEL_MK 標記刪除，保留歷史資料

### 1.2 ADMIN_LEVEL 權限資料表

```csharp
/// <summary>
/// 管理者權限資料表
/// 儲存使用者與選單的對應關係
/// </summary>
public class TblADMIN_LEVEL
{
    /// <summary>
    /// 帳號
    /// </summary>
    public string ACC_NO { get; set; }

    /// <summary>
    /// 選單編號
    /// </summary>
    public int? MN_ID { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UPD_TIME { get; set; }

    /// <summary>
    /// 更新功能代碼
    /// </summary>
    public string UPD_FUN_CD { get; set; }

    /// <summary>
    /// 更新帳號
    /// </summary>
    public string UPD_ACC { get; set; }

    /// <summary>
    /// 新增時間
    /// </summary>
    public DateTime? ADD_TIME { get; set; }

    /// <summary>
    /// 新增功能代碼
    /// </summary>
    public string ADD_FUN_CD { get; set; }

    /// <summary>
    /// 新增帳號
    /// </summary>
    public string ADD_ACC { get; set; }

    /// <summary>
    /// 刪除標記
    /// </summary>
    public string DEL_MK { get; set; }
}
```

**資料表說明：**

1. **多對多關係**：一個使用者可有多個權限，一個權限可給多個使用者
2. **審計欄位**：記錄新增、更新的時間和操作者
3. **軟刪除**：使用 DEL_MK 標記刪除

## 2. 選單查詢與產生

### 2.1 選單查詢方法

**檔案位置：** `ES/Areas/BACKMIN/Utils/CodeUtils.cs`

#### 2.1.1 取得使用者選單清單

```csharp
/// <summary>
/// 取得使用者的選單清單
/// 根據使用者權限過濾選單項目
/// </summary>
/// <param name="conn">資料庫連線</param>
/// <param name="parentId">父選單編號</param>
/// <param name="account">使用者帳號</param>
/// <returns>選單清單</returns>
public static List<Dictionary<String, Object>> GetMenuList(
    SqlConnection conn, 
    int parentId, 
    string account)
{
    List<Dictionary<String, Object>> list = new List<Dictionary<string, object>>();
    Dictionary<String, Object> item = null;

    // 建立查詢 SQL
    StringBuilder querySQL = new StringBuilder(@"
        SELECT MN_ID, MN_TYPE, MN_TEXT, MN_URL, MN_TARGET
        FROM ADMIN_MENU M
        WHERE MN_PID = @MN_PID
          AND DEL_MK = 'N'");

    // 如果有指定帳號，加入權限檢查
    if (!String.IsNullOrEmpty(account))
    {
        querySQL.Append(@"
            AND (
                EXISTS (
                    SELECT MN_ID 
                    FROM ADMIN_LEVEL 
                    WHERE ACC_NO = @ACC_NO 
                      AND MN_ID = M.MN_ID
                      AND DEL_MK = 'N'
                ) 
                OR MN_ID IN (995, 1000)  -- 特殊選單項目，所有人都可見
            )");
    }

    querySQL.Append(" ORDER BY SEQ_NO");

    // 執行查詢
    SqlCommand cmd = new SqlCommand(querySQL.ToString(), conn);
    cmd.Parameters.AddWithValue("@MN_PID", parentId);
    
    if (!String.IsNullOrEmpty(account))
    {
        cmd.Parameters.AddWithValue("@ACC_NO", account);
    }

    using (SqlDataReader dr = cmd.ExecuteReader())
    {
        while (dr.Read())
        {
            item = new Dictionary<string, object>();
            item.Add("id", DataUtils.GetDBInt(dr, "MN_ID"));
            item.Add("name", DataUtils.GetDBString(dr, "MN_TEXT"));
            item.Add("url", DataUtils.GetDBString(dr, "MN_URL"));
            item.Add("target", DataUtils.GetDBString(dr, "MN_TARGET"));
            
            // 判斷是否為父節點
            string type = DataUtils.GetDBString(dr, "MN_TYPE");
            item.Add("isParent", type.Equals("G"));

            list.Add(item);
        }
    }

    return list;
}
```

**程式碼說明：**

1. **階層查詢**：使用 MN_PID 查詢特定層級的選單
2. **權限過濾**：使用 EXISTS 子查詢檢查使用者權限
3. **特殊項目**：某些選單項目（如首頁）所有人都可見
4. **排序**：使用 SEQ_NO 排序
5. **isParent 標記**：標記是否為父節點，供前端判斷

### 2.2 選單控制器

**檔案位置：** `ES/Areas/BACKMIN/Controllers/MainController.cs`

#### 2.2.1 選單 API 端點

```csharp
/// <summary>
/// 選單 API
/// 提供動態載入選單的端點
/// </summary>
/// <returns>JSON 格式的選單資料</returns>
public ActionResult Menu()
{
    List<Dictionary<String, Object>> list = null;

    // 取得父選單編號
    int parentId = 0;
    if (Request.Form["id"] != null)
    {
        parentId = Int32.Parse(Request.Form["id"]);
    }

    // 查詢選單
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();
        list = CodeUtils.GetMenuList(conn, parentId, GetAccount());
        conn.Close();
        conn.Dispose();
    }

    // 回傳 JSON
    return Json(list, JsonRequestBehavior.AllowGet);
}
```

**程式碼說明：**

1. **動態載入**：支援按需載入子選單
2. **權限控制**：使用 GetAccount() 取得目前使用者
3. **JSON 回傳**：回傳 JSON 格式供前端使用

## 3. 權限管理

### 3.1 權限設定

**檔案位置：** `ES/Areas/BACKMIN/Action/AccountAction.cs`

#### 3.1.1 儲存使用者權限

```csharp
/// <summary>
/// 儲存使用者權限
/// </summary>
/// <param name="model">帳號模型</param>
public void SaveLevel(AccountModel model)
{
    try
    {
        // 第一步：標記要刪除的權限
        string deleteSQL1 = @"
            UPDATE ADMIN_LEVEL 
            SET DEL_MK = 'Y', 
                UPD_TIME = GETDATE(), 
                UPD_FUN_CD = @FUN_CD, 
                UPD_ACC = @UPD_ACC
            WHERE ACC_NO = @ACC_NO 
              AND DEL_MK = 'N'";

        Dictionary<string, object> args = new Dictionary<string, object>();
        args.Add("ACC_NO", model.Account);
        args.Add("FUN_CD", "ADM-ACC");
        args.Add("UPD_ACC", model.UpdateAccount);
        Update(deleteSQL1, args);

        // 第二步：實際刪除已標記的權限
        string deleteSQL2 = @"
            DELETE ADMIN_LEVEL 
            WHERE DEL_MK = 'Y' 
              AND ACC_NO = @ACC_NO";

        args.Clear();
        args.Add("ACC_NO", model.Account);
        Update(deleteSQL2, args);

        // 第三步：新增權限
        string insertSQL = String.Format(@"
            INSERT INTO ADMIN_LEVEL (
                ACC_NO, MN_ID,
                UPD_TIME, UPD_FUN_CD, UPD_ACC, 
                ADD_TIME, ADD_FUN_CD, ADD_ACC,
                DEL_MK
            )
            SELECT @ACC_NO, MN_ID,
                GETDATE(), @FUN_CD, @UPD_ACC, 
                GETDATE(), @FUN_CD, @UPD_ACC,
                'N'
            FROM ADMIN_MENU M
            WHERE DEL_MK = 'N'
              AND MN_ID IN ({0})
              AND MN_ID NOT IN (
                  SELECT MN_ID 
                  FROM ADMIN_LEVEL 
                  WHERE DEL_MK = 'N' 
                    AND ACC_NO = @ACC_NO
              )", model.Level);

        args.Clear();
        args.Add("ACC_NO", model.Account);
        args.Add("FUN_CD", "ADM-ACC");
        args.Add("UPD_ACC", model.UpdateAccount);
        Update(insertSQL, args);
    }
    catch (Exception ex)
    {
        logger.Error("儲存使用者權限失敗", ex);
        throw;
    }
}
```

**程式碼說明：**

1. **先刪後增**：先刪除舊權限，再新增新權限
2. **軟刪除**：先標記刪除，再實際刪除
3. **避免重複**：使用 NOT IN 避免新增重複權限
4. **審計記錄**：記錄操作時間和操作者

### 3.2 權限查詢

#### 3.2.1 取得使用者權限清單

```csharp
/// <summary>
/// 取得使用者權限清單
/// 用於權限設定頁面顯示
/// </summary>
/// <param name="accountNo">帳號</param>
/// <returns>權限清單</returns>
public List<Dictionary<String, Object>> GetLevelList(string accountNo)
{
    List<Dictionary<String, Object>> list = new List<Dictionary<string, object>>();
    Dictionary<String, Object> item = null;

    string querySQL = @"
        SELECT 
            MN_ID, 
            MN_PID, 
            MN_TYPE, 
            MN_TEXT,
            (CASE 
                WHEN (
                    SELECT COUNT(1) 
                    FROM ADMIN_LEVEL 
                    WHERE DEL_MK = 'N' 
                      AND MN_ID = M.MN_ID 
                      AND ACC_NO = @ACC_NO
                ) = 0 
                THEN 'N' 
                ELSE 'Y' 
            END) AS CHK,
            SEQ_NO * 10000 + ISNULL((
                SELECT SEQ_NO 
                FROM ADMIN_MENU 
                WHERE MN_ID = M.MN_PID
            ), 0) AS SEQ
        FROM ADMIN_MENU M
        WHERE DEL_MK = 'N' 
          AND MN_ID NOT IN (995, 1000)
        ORDER BY 6";

    SqlCommand cmd = new SqlCommand(querySQL, conn);
    cmd.Parameters.AddWithValue("@ACC_NO", accountNo);

    using (SqlDataReader dr = cmd.ExecuteReader())
    {
        while (dr.Read())
        {
            item = new Dictionary<string, object>();
            item.Add("MN_ID", DataUtils.GetDBInt(dr, "MN_ID"));
            item.Add("MN_PID", DataUtils.GetDBInt(dr, "MN_PID"));
            item.Add("MN_TYPE", DataUtils.GetDBString(dr, "MN_TYPE"));
            item.Add("MN_TEXT", DataUtils.GetDBString(dr, "MN_TEXT"));
            item.Add("CHK", DataUtils.GetDBString(dr, "CHK"));
            list.Add(item);
        }
    }

    return list;
}
```

**程式碼說明：**

1. **勾選狀態**：使用 CASE WHEN 判斷是否已勾選
2. **排序計算**：使用複合排序確保階層順序
3. **排除特殊項目**：排除不需要設定權限的項目

## 4. 前端選單渲染

### 4.1 zTree 選單樹

**檔案位置：** `ES/Areas/BACKMIN/Views/Shared/_Layout.cshtml`

#### 4.1.1 zTree 設定

```javascript
// zTree 選單設定
var leftMenu = {
    async: {
        enable: true,  // 啟用非同步載入
        url: "@Url.Action("Menu", "Main")",  // 選單 API 端點
        autoParam: ["id"],  // 自動傳遞的參數
        dataFilter: treeFilter  // 資料過濾函數
    },
    callback: {
        onAsyncSuccess: zTreeOnAsyncSuccess  // 非同步載入成功回調
    }
};

// 資料過濾函數
function treeFilter(treeId, parentNode, responseData) {
    if (responseData) {
        for (var i = 0; i < responseData.length; i++) {
            responseData[i].open = false;  // 預設不展開
        }
    }
    return responseData;
}

// 非同步載入成功回調
function zTreeOnAsyncSuccess(event, treeId, treeNode, msg) {
    // 展開第一層節點
    var zTree = $.fn.zTree.getZTreeObj(treeId);
    var nodes = zTree.getNodesByParam("level", 0);
    for (var i = 0; i < nodes.length; i++) {
        zTree.expandNode(nodes[i], true, false, false);
    }
}

// 初始化 zTree
$(document).ready(function() {
    $.fn.zTree.init($("#leftMenu"), leftMenu);
});
```

**程式碼說明：**

1. **非同步載入**：按需載入子選單，提升效能
2. **資料過濾**：處理 API 回傳的資料
3. **自動展開**：自動展開第一層節點
4. **zTree 函式庫**：使用 zTree 實作樹狀選單

## 5. 總結

e-service 系統的側邊選單權限控制機制提供了完整的選單管理功能：

### 5.1 選單管理特色
- **階層式結構**：支援多層選單
- **資料庫驅動**：選單資料儲存在資料庫
- **動態載入**：按需載入子選單
- **排序控制**：靈活的排序機制

### 5.2 權限控制特色
- **細緻權限**：選單層級的權限控制
- **多對多關係**：使用者與權限的多對多關係
- **權限繼承**：父選單權限自動包含子選單
- **特殊項目**：支援特殊選單項目

### 5.3 使用者體驗特色
- **非同步載入**：提升載入速度
- **樹狀顯示**：清晰的階層結構
- **權限過濾**：只顯示有權限的選單
- **響應式設計**：適應不同螢幕大小

這套選單權限控制機制為系統提供了彈性且易於維護的選單管理解決方案。

