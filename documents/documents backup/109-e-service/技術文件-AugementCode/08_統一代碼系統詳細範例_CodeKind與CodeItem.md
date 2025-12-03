# 衛福部人民線上申辦系統 - 統一代碼系統詳細範例：CodeKind 與 CodeItem

## 功能概述

本文詳細說明 e-service 系統中的統一代碼管理系統。系統透過 `CODE_CD` 資料表，建立了一套完整的代碼種類（Code Kind）和代碼項目（Code Item）管理機制，用於統一管理整個專案中的所有短代碼和對應名稱，包含下拉選單選項、狀態代碼、分類代碼等。

## 統一代碼系統架構

```
統一代碼系統架構：

CODE_CD (統一代碼表)
    ↓ 階層式結構
代碼種類 (CODE_KIND)
    ↓ 一對多關係
代碼項目 (CODE_CD, CODE_PCD)
    ↓ 應用層面
CodeUtils 工具類別
    ↓ 前端渲染
下拉選單 / CheckBox
    ↓ 使用者介面
統一的使用者體驗
```

## 1. 資料庫代碼管理架構

### 1.1 CODE_CD 統一代碼資料表

**檔案位置：** `ES/Models/Entities/CODE_CD.cs`

#### 1.1.1 核心代碼資料結構

```csharp
/// <summary>
/// 統一代碼資料表
/// 儲存系統所有代碼資料
/// </summary>
public class TblCODE_CD
{
    /// <summary>
    /// 代碼種類
    /// 例如：APP_STATUS（申請狀態）、LOGIN_TYPE（登入類型）
    /// </summary>
    public string CODE_KIND { get; set; }

    /// <summary>
    /// 代碼值
    /// 例如：D（草稿）、P（送審中）、A（核准）
    /// </summary>
    public string CODE_CD { get; set; }

    /// <summary>
    /// 父代碼值
    /// 用於建立階層式代碼結構
    /// 空字串表示第一層代碼
    /// </summary>
    public string CODE_PCD { get; set; }

    /// <summary>
    /// 代碼說明
    /// 顯示給使用者看的文字
    /// </summary>
    public string CODE_DESC { get; set; }

    /// <summary>
    /// 代碼備註
    /// 額外的說明資訊
    /// </summary>
    public string CODE_MEMO { get; set; }

    /// <summary>
    /// 排序序號
    /// 控制顯示順序
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

    /// <summary>
    /// 刪除功能代碼
    /// </summary>
    public string DEL_FUN_CD { get; set; }
}
```

**資料表說明：**

1. **複合主鍵**：CODE_KIND + CODE_CD + CODE_PCD
2. **階層結構**：使用 CODE_PCD 建立父子關係
3. **排序控制**：使用 SEQ_NO 控制顯示順序
4. **軟刪除**：使用 DEL_MK 標記刪除，保留歷史資料

### 1.2 常用代碼種類範例

#### 1.2.1 申請狀態代碼（APP_STATUS）

```sql
-- 申請狀態代碼
INSERT INTO CODE_CD (CODE_KIND, CODE_CD, CODE_PCD, CODE_DESC, SEQ_NO, DEL_MK)
VALUES
    ('APP_STATUS', 'D', '', '草稿', 1, 'N'),
    ('APP_STATUS', 'P', '', '送審中', 2, 'N'),
    ('APP_STATUS', 'R', '', '退件', 3, 'N'),
    ('APP_STATUS', 'S', '', '補件', 4, 'N'),
    ('APP_STATUS', 'A', '', '核准', 5, 'N'),
    ('APP_STATUS', 'C', '', '撤銷', 6, 'N');
```

#### 1.2.2 登入類型代碼（LOGIN_TYPE）

```sql
-- 登入類型代碼
INSERT INTO CODE_CD (CODE_KIND, CODE_CD, CODE_PCD, CODE_DESC, SEQ_NO, DEL_MK)
VALUES
    ('LOGIN_TYPE', 'MEMBER', '', '帳號密碼', 1, 'N'),
    ('LOGIN_TYPE', 'MOICA', '', '自然人憑證', 2, 'N'),
    ('LOGIN_TYPE', 'MOEACA', '', '工商憑證', 3, 'N'),
    ('LOGIN_TYPE', 'HCA', '', '醫事人員憑證', 4, 'N'),
    ('LOGIN_TYPE', 'NEWEID', '', '數位身分證', 5, 'N');
```

#### 1.2.3 階層式代碼範例（服務分類）

```sql
-- 第一層：大分類
INSERT INTO CODE_CD (CODE_KIND, CODE_CD, CODE_PCD, CODE_DESC, SEQ_NO, DEL_MK)
VALUES
    ('SRV_CATE', '001', '', '醫事人員', 1, 'N'),
    ('SRV_CATE', '002', '', '醫療機構', 2, 'N'),
    ('SRV_CATE', '003', '', '藥物食品', 3, 'N');

-- 第二層：子分類
INSERT INTO CODE_CD (CODE_KIND, CODE_CD, CODE_PCD, CODE_DESC, SEQ_NO, DEL_MK)
VALUES
    ('SRV_CATE', '001001', '001', '執業登記', 1, 'N'),
    ('SRV_CATE', '001002', '001', '證書申請', 2, 'N'),
    ('SRV_CATE', '002001', '002', '開業登記', 1, 'N'),
    ('SRV_CATE', '002002', '002', '變更登記', 2, 'N');
```

## 2. CodeUtils 工具類別

### 2.1 取得代碼說明

**檔案位置：** `ES/Utils/CodeUtils.cs`

#### 2.1.1 根據代碼值取得說明

```csharp
/// <summary>
/// 取得代碼說明
/// </summary>
/// <param name="conn">資料庫連線</param>
/// <param name="kind">代碼種類</param>
/// <param name="parentCode">父代碼</param>
/// <param name="value">代碼值</param>
/// <returns>代碼說明</returns>
public static string GetCodeDesc(
    SqlConnection conn,
    string kind,
    string parentCode,
    string value)
{
    // 建立查詢 SQL
    StringBuilder sb = new StringBuilder("SELECT ");
    sb.Append("CODE_DESC FROM CODE_CD ");
    sb.Append("WHERE CODE_KIND = @CODE_KIND ");

    // 如果有父代碼，加入條件
    if (parentCode != "")
    {
        sb.Append("AND CODE_PCD = @CODE_PCD ");
    }

    sb.Append("AND CODE_CD = @CODE_CD ");

    // 執行查詢
    SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
    cmd.Parameters.AddWithValue("CODE_KIND", kind);

    if (parentCode != "")
    {
        cmd.Parameters.AddWithValue("CODE_PCD", parentCode);
    }

    cmd.Parameters.AddWithValue("CODE_CD", value);

    var result = string.Empty;
    using (SqlDataReader dr = cmd.ExecuteReader())
    {
        if (dr.Read())
        {
            result = dr.GetString(0);
        }
        dr.Close();
    }

    return result;
}
```

**程式碼說明：**

1. **參數化查詢**：防止 SQL Injection
2. **彈性查詢**：支援有無父代碼的查詢
3. **安全關閉**：使用 using 確保資源釋放

**使用範例：**

```csharp
using (SqlConnection conn = DataUtils.GetConnection())
{
    conn.Open();

    // 取得申請狀態說明
    string statusDesc = CodeUtils.GetCodeDesc(conn, "APP_STATUS", "", "P");
    // 結果：送審中

    // 取得服務分類說明
    string cateDesc = CodeUtils.GetCodeDesc(conn, "SRV_CATE", "001", "001001");
    // 結果：執業登記

    conn.Close();
}
```

### 2.2 產生下拉選單

#### 2.2.1 基本下拉選單

```csharp
/// <summary>
/// 取得代碼下拉列表
/// </summary>
/// <param name="conn">資料庫連線</param>
/// <param name="kind">代碼種類</param>
/// <param name="parentCode">父代碼</param>
/// <param name="value">預設選項</param>
/// <param name="empty">是否產生空項目</param>
/// <returns>下拉選單項目清單</returns>
public static List<SelectListItem> GetCodeSelectList(
    SqlConnection conn,
    string kind,
    string parentCode,
    string value,
    bool empty)
{
    List<SelectListItem> list = new List<SelectListItem>();

    // 如果需要空項目，先加入
    if (empty)
    {
        SelectListItem item = new SelectListItem()
        {
            Text = "--請選擇--",
            Value = ""
        };
        list.Add(item);
    }

    // 查詢代碼資料
    string sql = @"
        SELECT CODE_CD, CODE_DESC
        FROM CODE_CD
        WHERE DEL_MK = 'N'
          AND CODE_KIND = @CODE_KIND
          AND CODE_PCD = @CODE_PCD
        ORDER BY SEQ_NO";

    SqlCommand cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("CODE_KIND", kind);
    cmd.Parameters.AddWithValue("CODE_PCD", parentCode);

    using (SqlDataReader dr = cmd.ExecuteReader())
    {
        while (dr.Read())
        {
            string s_Val = dr.GetString(0);
            string s_Txt = dr.GetString(1);

            SelectListItem item = new SelectListItem()
            {
                Text = s_Txt,
                Value = s_Val
            };

            // 設定預設選項
            if (value != null && value.Equals(s_Val))
            {
                item.Selected = true;
            }

            list.Add(item);
        }
        dr.Close();
    }

    return list;
}
```

**程式碼說明：**

1. **空項目選項**：支援產生「請選擇」空項目
2. **排序**：使用 SEQ_NO 排序
3. **預設選項**：支援設定預設選中的項目
4. **軟刪除過濾**：只查詢未刪除的代碼

**使用範例：**

```csharp
using (SqlConnection conn = DataUtils.GetConnection())
{
    conn.Open();

    // 產生申請狀態下拉選單
    List<SelectListItem> statusList = CodeUtils.GetCodeSelectList(
        conn,
        "APP_STATUS",  // 代碼種類
        "",            // 父代碼（空字串表示第一層）
        "P",           // 預設選項
        true           // 包含空項目
    );

    ViewBag.StatusList = statusList;

    conn.Close();
}
```

#### 2.2.2 多選下拉選單

```csharp
/// <summary>
/// 取得代碼下拉列表（支援多選）
/// </summary>
/// <param name="conn">資料庫連線</param>
/// <param name="kind">代碼種類</param>
/// <param name="parentCode">父代碼</param>
/// <param name="values">預設選項陣列</param>
/// <returns>下拉選單項目清單</returns>
public static List<SelectListItem> GetCodeSelectList(
    SqlConnection conn,
    string kind,
    string parentCode,
    string[] values)
{
    List<SelectListItem> list = new List<SelectListItem>();

    string sql = @"
        SELECT CODE_CD, CODE_DESC
        FROM CODE_CD
        WHERE DEL_MK = 'N'
          AND CODE_KIND = @CODE_KIND
          AND CODE_PCD = @CODE_PCD
        ORDER BY SEQ_NO";

    SqlCommand cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("CODE_KIND", kind);
    cmd.Parameters.AddWithValue("CODE_PCD", parentCode);

    using (SqlDataReader dr = cmd.ExecuteReader())
    {
        while (dr.Read())
        {
            string s_Val = dr.GetString(0);
            string s_Txt = dr.GetString(1);

            SelectListItem item = new SelectListItem()
            {
                Text = s_Txt,
                Value = s_Val
            };

            // 檢查是否在預設選項陣列中
            if (values != null && values.Contains(s_Val))
            {
                item.Selected = true;
            }

            list.Add(item);
        }
        dr.Close();
    }

    return list;
}
```

### 2.3 產生 CheckBox 清單

```csharp
/// <summary>
/// 取得代碼 CheckBox 列表
/// </summary>
/// <param name="conn">資料庫連線</param>
/// <param name="kind">代碼種類</param>
/// <param name="parentCode">父代碼</param>
/// <param name="values">預設選項陣列</param>
/// <returns>CheckBox 項目清單</returns>
public static List<CheckBoxListItem> GetCodeCheckBoxList(
    SqlConnection conn,
    string kind,
    string parentCode,
    string[] values)
{
    List<CheckBoxListItem> list = new List<CheckBoxListItem>();

    string sql = @"
        SELECT CODE_CD, CODE_DESC
        FROM CODE_CD
        WHERE DEL_MK = 'N'
          AND CODE_KIND = @CODE_KIND
          AND CODE_PCD = @CODE_PCD
        ORDER BY SEQ_NO";

    SqlCommand cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("CODE_KIND", kind);
    cmd.Parameters.AddWithValue("CODE_PCD", parentCode);

    using (SqlDataReader dr = cmd.ExecuteReader())
    {
        while (dr.Read())
        {
            string s_Val = dr.GetString(0);
            string s_Txt = dr.GetString(1);

            CheckBoxListItem item = new CheckBoxListItem()
            {
                Text = s_Txt,
                Value = s_Val
            };

            // 檢查是否在預設選項陣列中
            if (values != null && values.Contains(s_Val))
            {
                item.Checked = true;
            }

            list.Add(item);
        }
        dr.Close();
    }

    return list;
}
```

## 3. 前端使用範例

### 3.1 控制器中準備資料

**檔案位置：** `ES/Controllers/Apply_001008Controller.cs`

```csharp
/// <summary>
/// 申請頁面
/// </summary>
[HttpGet]
public ActionResult Apply()
{
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();

        // 產生申請狀態下拉選單
        ViewBag.StatusList = CodeUtils.GetCodeSelectList(
            conn, "APP_STATUS", "", "", true);

        // 產生登入類型下拉選單
        ViewBag.LoginTypeList = CodeUtils.GetCodeSelectList(
            conn, "LOGIN_TYPE", "", "", false);

        // 產生服務分類下拉選單（第一層）
        ViewBag.CateList = CodeUtils.GetCodeSelectList(
            conn, "SRV_CATE", "", "", true);

        conn.Close();
    }

    return View();
}
```

### 3.2 View 中使用下拉選單

**檔案位置：** `ES/Views/Apply_001008/Apply.cshtml`

```html
<!-- 申請狀態下拉選單 -->
<div class="form-group">
    <label>申請狀態</label>
    @Html.DropDownList("Status",
        (List<SelectListItem>)ViewBag.StatusList,
        new { @class = "form-control" })
</div>

<!-- 登入類型下拉選單 -->
<div class="form-group">
    <label>登入類型</label>
    @Html.DropDownList("LoginType",
        (List<SelectListItem>)ViewBag.LoginTypeList,
        new { @class = "form-control" })
</div>

<!-- 服務分類下拉選單 -->
<div class="form-group">
    <label>服務分類</label>
    @Html.DropDownList("Category",
        (List<SelectListItem>)ViewBag.CateList,
        new { @class = "form-control", @id = "ddlCategory" })
</div>
```

### 3.3 階層式下拉選單（父子聯動）

```javascript
// 服務分類聯動
$(document).ready(function () {
  // 第一層分類變更時
  $("#ddlCategory").change(function () {
    var parentCode = $(this).val();

    // 清空第二層選單
    $("#ddlSubCategory").empty();
    $("#ddlSubCategory").append('<option value="">--請選擇--</option>');

    if (parentCode != "") {
      // 載入第二層分類
      $.ajax({
        url: '@Url.Action("GetSubCategory", "Code")',
        type: "POST",
        data: { parentCode: parentCode },
        success: function (data) {
          $.each(data, function (index, item) {
            $("#ddlSubCategory").append(
              $("<option></option>").val(item.Value).text(item.Text)
            );
          });
        }
      });
    }
  });
});
```

**控制器端點：**

```csharp
/// <summary>
/// 取得子分類
/// </summary>
/// <param name="parentCode">父代碼</param>
/// <returns>JSON 格式的下拉選單項目</returns>
[HttpPost]
public ActionResult GetSubCategory(string parentCode)
{
    List<SelectListItem> list = null;

    using (SqlConnection conn = GetConnection())
    {
        conn.Open();
        list = CodeUtils.GetCodeSelectList(
            conn, "SRV_CATE", parentCode, "", false);
        conn.Close();
    }

    return Json(list, JsonRequestBehavior.AllowGet);
}
```

## 4. 使用 Dapper 查詢代碼

### 4.1 使用 Dapper 取得代碼清單

**檔案位置：** `ES/DataLayers/MyKeyMapDAO.cs`

```csharp
/// <summary>
/// 使用 Dapper 取得代碼清單
/// </summary>
/// <param name="CODE_KIND">代碼種類</param>
/// <returns>代碼清單</returns>
public IList<KeyMapModel> GetCodeList(string CODE_KIND)
{
    List<KeyMapModel> result = null;
    StringBuilder sql = new StringBuilder();

    // 準備參數
    var dictionary = new Dictionary<string, object>
    {
        { "@CODE_KIND", CODE_KIND }
    };
    var parameters = new DynamicParameters(dictionary);

    // 建立查詢 SQL
    sql.Append(" SELECT CODE_CD AS CODE, CODE_DESC AS TEXT ");
    sql.Append(" FROM CODE_CD ");
    sql.Append(" WHERE CODE_KIND = @CODE_KIND ");
    sql.Append("   AND DEL_MK = 'N' ");
    sql.Append(" ORDER BY SEQ_NO, CODE_CD ");

    // 執行查詢
    using (SqlConnection conn = DataUtils.GetConnection())
    {
        conn.Open();
        result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
        conn.Close();
        conn.Dispose();
    }

    return result;
}
```

**KeyMapModel 定義：**

```csharp
/// <summary>
/// 鍵值對模型
/// 用於下拉選單資料綁定
/// </summary>
public class KeyMapModel
{
    /// <summary>
    /// 代碼值
    /// </summary>
    public string CODE { get; set; }

    /// <summary>
    /// 代碼說明
    /// </summary>
    public string TEXT { get; set; }
}
```

## 5. 代碼管理最佳實務

### 5.1 代碼命名規範

```
代碼種類命名規範：

1. 使用大寫英文字母和底線
2. 使用有意義的名稱
3. 避免過長的名稱

範例：
✓ APP_STATUS（申請狀態）
✓ LOGIN_TYPE（登入類型）
✓ SRV_CATE（服務分類）
✗ STATUS（太簡短，不明確）
✗ APPLICATION_STATUS_CODE（太長）
```

### 5.2 代碼值設計原則

```
代碼值設計原則：

1. 使用簡短的代碼值（1-10 個字元）
2. 使用有意義的縮寫
3. 保持一致性

範例：
✓ D（Draft - 草稿）
✓ P（Pending - 送審中）
✓ A（Approved - 核准）
✗ DRAFT（太長）
✗ 1（無意義）
```

### 5.3 階層式代碼設計

```
階層式代碼設計：

1. 使用固定長度的代碼值
2. 父代碼包含在子代碼中
3. 最多 3-4 層

範例：
第一層：001（醫事人員）
第二層：001001（執業登記）
第三層：001001001（新申請）

✓ 清晰的階層關係
✓ 易於查詢和維護
✗ 避免過深的階層（超過 4 層）
```

### 5.4 代碼維護注意事項

```csharp
/// <summary>
/// 代碼維護注意事項
/// </summary>
public class CodeMaintenanceGuidelines
{
    // 1. 不要刪除已使用的代碼，使用軟刪除
    public void SoftDelete(string codeKind, string codeValue)
    {
        string sql = @"
            UPDATE CODE_CD
            SET DEL_MK = 'Y',
                DEL_TIME = GETDATE()
            WHERE CODE_KIND = @CODE_KIND
              AND CODE_CD = @CODE_CD";

        // 執行更新...
    }

    // 2. 新增代碼時設定適當的排序序號
    public void AddCode(string codeKind, string codeValue, string codeDesc)
    {
        // 取得目前最大序號
        string maxSeqSql = @"
            SELECT ISNULL(MAX(SEQ_NO), 0) + 1
            FROM CODE_CD
            WHERE CODE_KIND = @CODE_KIND";

        // 新增代碼...
    }

    // 3. 修改代碼說明時保留歷史記錄
    public void UpdateCodeDesc(string codeKind, string codeValue, string newDesc)
    {
        // 記錄修改歷史...

        string sql = @"
            UPDATE CODE_CD
            SET CODE_DESC = @CODE_DESC
            WHERE CODE_KIND = @CODE_KIND
              AND CODE_CD = @CODE_CD";

        // 執行更新...
    }
}
```

## 6. 總結

e-service 系統的統一代碼管理機制提供了完整且高效的解決方案：

### 6.1 核心特色

- **統一架構**：透過 CODE_CD 表建立完整的代碼管理體系
- **階層支援**：支援多層級的代碼階層結構
- **彈性查詢**：提供多種查詢方法滿足不同需求
- **前後端整合**：統一的 API 和元件設計

### 6.2 技術價值

- **可維護性**：集中式管理降低維護成本
- **可擴展性**：靈活的架構設計支援未來擴展
- **可重用性**：統一的工具類別可在整個系統中重用
- **一致性**：確保整個系統的代碼使用標準

### 6.3 使用者體驗

- **介面統一**：所有下拉選單具有一致的外觀
- **操作直觀**：階層選擇提供直觀的操作體驗
- **載入快速**：優化的查詢機制提供快速的資料載入
- **錯誤提示**：完整的錯誤處理和使用者提示

這套統一代碼管理機制為系統提供了彈性且易於維護的代碼管理解決方案，確保整個系統的代碼使用一致性和可維護性。
