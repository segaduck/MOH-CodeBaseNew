# EECOnline 電子病歷申請系統 - 統一代碼系統詳細範例：CodeKind 與 CodeName 管理

## 功能概述

本文詳細說明 EECOnline 系統中的統一代碼管理系統（Unified Code Management System）。系統透過 `CODE` 和 `CODE1` 資料表，建立了一套完整的代碼種類（Code Kind）和代碼名稱（Code Name）管理機制，用於統一管理整個專案中的所有短代碼和對應名稱，包含下拉選單選項、狀態代碼、分類代碼等。

## 統一代碼系統架構概覽

```
EECOnline 統一代碼系統架構：

CODE / CODE1 (統一代碼表)
    ↓ 代碼種類定義
StaticCodeMap.CodeMap (代碼類別列舉)
    ↓ IBatis SQL 映射
KeyMap.xml (SQL 查詢定義)
    ↓ 資料存取層
MyKeyMapDAO (代碼查詢 DAO)
    ↓ 業務邏輯層
ShareCodeListModel (下拉選單模型)
    ↓ 控制器層
AjaxController (動態載入)
    ↓ 前端視圖
Razor Views (下拉選單渲染)
    ↓ 使用者介面
統一的使用者體驗
```

## 1. 資料庫代碼管理架構

### 1.1 CODE 統一代碼資料表

**檔案位置：** `trunk/Models/Entities/CODE.cs`

#### 1.1.1 核心代碼資料結構

```csharp
/// <summary>
/// 統一代碼資料表
/// 儲存系統所有代碼資料
/// </summary>
public class TblCODE : IDBRow
{
    /// <summary>
    /// 主鍵 - 代碼項目唯一識別碼
    /// </summary>
    [IdentityDBField]
    public long? keyid { get; set; }

    /// <summary>
    /// 代碼類別 - 定義代碼的分類
    /// 例如：APPLY（申辦項目）、APPLYSTATUS（申請狀態）、BEDTYPE（病床類型）
    /// </summary>
    public string code_type { get; set; }

    /// <summary>
    /// 代碼值 - 實際的代碼值
    /// 例如：001008（申辦項目代碼）、D（草稿）、A（核准）
    /// </summary>
    public string code_cd { get; set; }

    /// <summary>
    /// 代碼名稱 - 顯示給使用者的友善名稱
    /// 例如：「醫事人員執業登記」、「草稿」、「核准」
    /// </summary>
    public string code_name { get; set; }

    /// <summary>
    /// 代碼說明 - 詳細的代碼描述
    /// 提供更詳細的代碼用途說明
    /// </summary>
    public string code_desc { get; set; }

    /// <summary>
    /// 狀態 - 代碼啟用狀態
    /// 1: 啟用, 0: 停用
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// 排序序號 - 控制代碼在下拉選單中的顯示順序
    /// 數字越小越優先顯示
    /// </summary>
    public int? orderby { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime? created { get; set; }

    /// <summary>
    /// 建立者
    /// </summary>
    public string creater { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? updated { get; set; }

    /// <summary>
    /// 修改者
    /// </summary>
    public string updater { get; set; }

    /// <summary>
    /// 取得資料表名稱
    /// </summary>
    public DBRowTableName GetTableName()
    {
        return StaticCodeMap.TableName.CODE;
    }
}
```

**程式碼說明：**

1. **雙重索引設計**：`code_type` + `code_cd` 組成唯一索引，確保代碼不重複
2. **狀態控制**：透過 `status` 欄位控制代碼的啟用/停用狀態
3. **排序機制**：`orderby` 欄位控制顯示順序，提升使用者體驗
4. **完整審計**：包含建立和修改的時間與使用者記錄
5. **IDBRow 介面**：實作 Turbo 框架的資料列介面，支援 ORM 操作

### 1.2 CODE1 擴充代碼資料表

**檔案位置：** `trunk/Models/Entities/CODE1.cs`

#### 1.2.1 擴充代碼資料結構

```csharp
/// <summary>
/// 擴充代碼資料表
/// 用於儲存特殊用途的代碼資料
/// </summary>
public class TblCODE1 : IDBRow
{
    /// <summary>
    /// 主鍵 - 代碼項目唯一識別碼
    /// </summary>
    [IdentityDBField]
    public long? keyid { get; set; }

    /// <summary>
    /// 項目類別 - 代碼項目的分類
    /// 例如：LoginType（登入類型）
    /// </summary>
    public string ITEM { get; set; }

    /// <summary>
    /// 代碼值 - 實際的代碼值
    /// 例如：1（自然人憑證）、2（行動自然人憑證）、3（健保卡）
    /// </summary>
    public string CODE { get; set; }

    /// <summary>
    /// 代碼名稱 - 顯示給使用者的友善名稱
    /// 例如：「自然人憑證登入」、「行動自然人憑證登入」、「身分證字號 + 健保卡」
    /// </summary>
    public string TEXT { get; set; }

    /// <summary>
    /// 代碼說明 - 詳細的代碼描述
    /// </summary>
    public string MEMO { get; set; }

    /// <summary>
    /// 排序序號 - 控制顯示順序
    /// </summary>
    public int? ORDERBY { get; set; }

    /// <summary>
    /// 取得資料表名稱
    /// </summary>
    public DBRowTableName GetTableName()
    {
        return StaticCodeMap.TableName.CODE1;
    }
}
```

**程式碼說明：**

1. **特殊用途**：CODE1 表用於儲存特殊用途的代碼，與 CODE 表分開管理
2. **簡化結構**：相較於 CODE 表，CODE1 結構更簡化，適合簡單的代碼管理
3. **靈活應用**：可用於系統設定、登入類型等特殊代碼需求
4. **獨立管理**：與 CODE 表分開，避免代碼混淆

### 1.3 StaticCodeMap 代碼類別列舉

**檔案位置：** `trunk/Commons/StaticCodeMap.cs`

#### 1.3.1 代碼類別定義

```csharp
/// <summary>
/// 系統代碼及表格名稱列舉
/// </summary>
public partial class StaticCodeMap
{
    /// <summary>
    /// 代碼表類別列舉清單, 叫用 KeyMapDAO.GetCodeMapList() 所需的參數
    /// </summary>
    public class CodeMap : CodeMapType
    {
        #region 私有(隱藏) CodeMap 建構式

        private CodeMap(string codeName) : base(codeName)
        { }

        /// <summary>
        /// 建構式 - 指定代碼名稱和 SQL 語句 ID
        /// </summary>
        /// <param name="codeName">代碼名稱</param>
        /// <param name="sqlStatementId">SQL 語句 ID（對應 KeyMap.xml 中的 select id）</param>
        private CodeMap(string codeName, string sqlStatementId) :
            base(codeName, sqlStatementId)
        { }

        #endregion 私有(隱藏) CodeMap 建構式

        /// <summary>
        /// 前台申辦案件縣市別清單
        /// </summary>
        public static CodeMapType E_SRV_CITY = new CodeMapType("E_SRV_CITY", "KeyMap.getE_SRV_CITY");

        /// <summary>
        /// 系統名稱清單
        /// </summary>
        public static CodeMapType sysid = new CodeMapType("sysid", "KeyMap.getSYSID");

        /// <summary>
        /// 模組名稱清單
        /// </summary>
        public static CodeMapType modules = new CodeMapType("modules", "KeyMap.getMODULES");

        /// <summary>
        /// 單位資料
        /// </summary>
        public static CodeMapType unit = new CodeMapType("unit", "KeyMap.getUNIT");

        /// <summary>
        /// 功能群組
        /// </summary>
        public static CodeMapType amgrp = new CodeMapType("amgrp", "KeyMap.getAMGRP");

        /// <summary>
        /// 申辦項目
        /// </summary>
        public static CodeMapType apply = new CodeMapType("apply", "KeyMap.getApply");

        /// <summary>
        /// 處理狀態
        /// </summary>
        public static CodeMapType srv_status = new CodeMapType("srv_status", "KeyMap.getSrv_status");

        /// <summary>
        /// 公告類型
        /// </summary>
        public static CodeMapType enews = new CodeMapType("enews", "KeyMap.getEnews");

        /// <summary>
        /// 常見問題類型
        /// </summary>
        public static CodeMapType code_name = new CodeMapType("code_name", "KeyMap.getfaq");

        /// <summary>
        /// 機構類別
        /// </summary>
        public static CodeMapType Type_id = new CodeMapType("Type_id", "KeyMap.getType_id");

        /// <summary>
        /// 登記事項
        /// </summary>
        public static CodeMapType Apy_change = new CodeMapType("Apy_change", "KeyMap.getApy_change");

        /// <summary>
        /// 病床類型
        /// </summary>
        public static CodeMapType Bed_type = new CodeMapType("Bed_type", "KeyMap.getBed_type");

        /// <summary>
        /// 執業科別(全部)
        /// </summary>
        public static CodeMapType DcdDept = new CodeMapType("DcdDept", "KeyMap.getDcdDept");

        /// <summary>
        /// 權屬別(全部)
        /// </summary>
        public static CodeMapType AUTHOR = new CodeMapType("AUTHOR", "KeyMap.getAUTHOR");

        /// <summary>
        /// 縣市清單
        /// </summary>
        public static CodeMapType Zip_City = new CodeMapType("Zip_City", "KeyMap.getZip_City");

        /// <summary>
        /// 鄉鎮區清單
        /// </summary>
        public static CodeMapType Zip_Town = new CodeMapType("Zip_Town", "KeyMap.getZip_Town");

        /// <summary>
        /// 街道清單
        /// </summary>
        public static CodeMapType Zip_Road = new CodeMapType("Zip_Road", "KeyMap.getZip_Road");

        /// <summary>
        /// 醫院清單
        /// </summary>
        public static CodeMapType Get_Hospital = new CodeMapType("Get_Hospital", "KeyMap.get_Hospital");

        /// <summary>
        /// 病歷類型 (全顯示)
        /// </summary>
        public static CodeMapType Get_HIS_Type_All = new CodeMapType("Get_HIS_Type_All", "KeyMap.get_HIS_Type_All");

        /// <summary>
        /// 病歷類型 (僅顯示有效期內，須傳入醫院代號)
        /// </summary>
        public static CodeMapType Get_HIS_Type_Valid = new CodeMapType("Get_HIS_Type_Valid", "KeyMap.get_HIS_Type_Valid");

        /// <summary>
        /// 登入類型
        /// 1: 自然人憑證登入
        /// 2: 行動自然人憑證登入 (TW FidO)
        /// 3: 身分證字號 + 健保卡
        /// </summary>
        public static CodeMapType Get_login_type = new CodeMapType("Get_login_type", "KeyMap.get_login_type");

        /// <summary>
        /// 帳務月份列表 - 依照申請訂單 createdatetime 為主
        /// </summary>
        public static CodeMapType Get_AccountingYM = new CodeMapType("Get_AccountingYM", "KeyMap.get_AccountingYM");
    }
}
```

**程式碼說明：**

1. **靜態列舉設計**：使用靜態屬性定義所有代碼類別，便於全域存取
2. **雙參數建構**：每個代碼類別包含代碼名稱和對應的 SQL 語句 ID
3. **命名規範**：代碼類別名稱與業務功能對應，提升可讀性
4. **SQL 映射**：每個代碼類別對應 KeyMap.xml 中的一個 SQL 查詢
5. **擴充性**：新增代碼類別只需新增一個靜態屬性即可

## 2. IBatis SQL 映射定義

### 2.1 KeyMap.xml SQL 查詢定義

**檔案位置：** `trunk/SqlMaps/KeyMap.xml`

#### 2.1.1 SQL Map 檔案結構

```xml
<?xml version="1.0" encoding="utf-8" ?>
<sqlMap namespace="KeyMap"
xmlns="http://ibatis.apache.org/mapping"
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <resultMaps>
  </resultMaps>

  <!-- 快取模型定義 -->
  <cacheModel id="EECOnline-cache" implementation="MEMORY">
    <!-- 快取有效期：1 小時 -->
    <flushInterval hours="1" />
    <!-- 快取類型：弱引用 -->
    <property name="Type" value="WEAK" />
  </cacheModel>

  <!--
    這個檔案專門用來定義 代碼DDL清單內容，資料來源可以是 DB 代碼表格,
    也可以是用 Union 直接定義在這裡, 而不要寫在程式中.
    SQL 返回結果欄位, 一律 Alias 成「CODE」與「TEXT」
    -->
  <statements>
    <!-- SQL 查詢定義 -->
  </statements>
</sqlMap>
```

**程式碼說明：**

1. **命名空間**：`KeyMap` 命名空間用於組織所有代碼查詢 SQL
2. **快取機制**：使用 MEMORY 快取模型，快取有效期 1 小時
3. **弱引用快取**：使用 WEAK 類型，允許 GC 回收記憶體
4. **統一欄位名稱**：所有查詢結果必須使用 `CODE` 和 `TEXT` 欄位名稱
5. **集中管理**：所有代碼查詢集中在一個檔案，便於維護

#### 2.1.2 系統功能清單查詢

```xml
<!-- 取得系統功能清單 -->
<select id="getSYSID" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="TBKC-cache">
  <![CDATA[
    SELECT sysid CODE,prgname TEXT
    FROM   amfuncm
    WHERE  modules = ''
    ORDER BY prgorder
  ]]>
</select>
```

**程式碼說明：**

1. **查詢 ID**：`getSYSID` 對應 `StaticCodeMap.CodeMap.sysid`
2. **結果類別**：返回 `KeyMapModel` 物件列表
3. **參數類別**：使用 `Hashtable` 接收參數
4. **快取模型**：使用 `TBKC-cache` 快取查詢結果
5. **查詢邏輯**：從 `AMFUNCM` 表查詢第一層系統功能（`modules = ''`）
6. **排序**：按 `prgorder` 排序

#### 2.1.3 模組功能清單查詢

```xml
<!-- 取得模組功能清單 -->
<select id="getMODULES" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="TBKC-cache">
  <![CDATA[
    SELECT prgid CODE,prgname TEXT
    FROM   amfuncm
    WHERE  1 = 1
    AND    modules <> ''
    AND    submodules = ''
   ]]>
  <dynamic>
    <isParameterPresent>
      <isNotEmpty prepend="" property="sysid">
        <![CDATA[
            AND sysid = #sysid#
         ]]>
      </isNotEmpty>
    </isParameterPresent>
  </dynamic>
  <![CDATA[
  ORDER BY prgorder
  ]]>
</select>
```

**程式碼說明：**

1. **動態 SQL**：使用 `<dynamic>` 標籤支援動態查詢條件
2. **參數檢查**：`<isParameterPresent>` 檢查參數是否存在
3. **非空檢查**：`<isNotEmpty>` 檢查參數是否非空
4. **參數綁定**：使用 `#sysid#` 綁定參數，防止 SQL Injection
5. **查詢邏輯**：查詢第二層模組功能（`modules <> ''` 且 `submodules = ''`）
6. **條件過濾**：可選擇性傳入 `sysid` 參數過濾特定系統的模組

#### 2.1.4 單位清單查詢

```xml
<!-- 取得單位清單 -->
<select id="getUNIT" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="TBKC-cache">
  <![CDATA[
    SELECT unit_cd CODE,unit_nm TEXT
    FROM   UNIT
    WHERE  1 = 1
    AND status = '1'
    ORDER BY unit_cd
  ]]>
</select>
```

**程式碼說明：**

1. **狀態過濾**：只查詢啟用狀態的單位（`status = '1'`）
2. **簡單查詢**：不需要動態參數，直接查詢所有啟用單位
3. **排序**：按單位代碼排序

#### 2.1.5 功能群組查詢

```xml
<!-- 取得功能群組 -->
<select id="getAMGRP"
       resultClass="Turbo.DataLayer.KeyMapModel"
       parameterClass="Hashtable"
       cacheModel="TBKC-cache">
  <![CDATA[
    SELECT convert(nvarchar,grp_id) CODE,grpname TEXT
    FROM   AMGRP
    WHERE  1 = 1
    ORDER BY grp_id
  ]]>
</select>
```

**程式碼說明：**

1. **型別轉換**：使用 `convert(nvarchar,grp_id)` 將群組 ID 轉換為字串
2. **統一格式**：確保 CODE 欄位為字串類型，符合 KeyMapModel 定義
3. **查詢來源**：從 `AMGRP` 表查詢所有功能群組

#### 2.1.6 申辦項目查詢

```xml
<!-- 取得申辦項目 -->
<select id="getApply" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="TBKC-cache">
  <![CDATA[
    SELECT code_cd CODE,code_name TEXT
    FROM   CODE
    WHERE  1 = 1
    AND    code_type = 'APPLY'
    AND    status = '1'
      AND    substring(code_cd,1,3) = substring(#srv_cd#,1,3)
    ORDER BY code_cd
   ]]>
</select>
```

**程式碼說明：**

1. **代碼類別過濾**：只查詢 `code_type = 'APPLY'` 的申辦項目
2. **狀態過濾**：只查詢啟用狀態的項目（`status = '1'`）
3. **前綴匹配**：使用 `substring` 函數匹配服務代碼前三碼
4. **參數綁定**：使用 `#srv_cd#` 綁定服務代碼參數
5. **業務邏輯**：根據服務代碼前綴過濾相關的申辦項目

#### 2.1.7 登入類型查詢

```xml
<!-- 取得登入類型 -->
<select id="get_login_type" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="EECOnline-cache">
  <![CDATA[
    SELECT CODE, TEXT
    FROM CODE1
    WHERE ITEM='LoginType'
    ORDER BY CAST(CODE AS INT)
  ]]>
</select>
```

**程式碼說明：**

1. **資料來源**：從 `CODE1` 表查詢登入類型
2. **項目過濾**：只查詢 `ITEM='LoginType'` 的項目
3. **型別轉換排序**：使用 `CAST(CODE AS INT)` 將代碼轉換為整數後排序
4. **特殊用途**：CODE1 表用於儲存特殊用途的代碼

#### 2.1.8 帳務月份查詢

```xml
<!-- 取得帳務月份列表 -->
<select id="get_AccountingYM" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="EECOnline-cache">
  <![CDATA[
    SELECT DISTINCT
      CAST(YEAR(a.createdatetime)      AS NVARCHAR) + '/' + CAST(MONTH(a.createdatetime) AS NVARCHAR) AS CODE,
      CAST(YEAR(a.createdatetime)-1911 AS NVARCHAR) + '/' + CAST(MONTH(a.createdatetime) AS NVARCHAR) AS TEXT
    FROM EEC_Apply a
    WHERE 1=1
    ORDER BY CAST(YEAR(a.createdatetime) AS NVARCHAR) + '/' + CAST(MONTH(a.createdatetime) AS NVARCHAR) DESC
  ]]>
</select>
```

**程式碼說明：**

1. **動態產生**：從 `EEC_Apply` 表動態產生帳務月份列表
2. **西元年份**：CODE 欄位使用西元年份（例如：2024/10）
3. **民國年份**：TEXT 欄位使用民國年份（例如：113/10）
4. **去重**：使用 `DISTINCT` 去除重複的月份
5. **降序排序**：最新的月份排在最前面
6. **業務邏輯**：根據申請訂單的建立時間產生月份選項

## 3. 資料存取層實作

### 3.1 MyKeyMapDAO 代碼查詢 DAO

**檔案位置：** `trunk/DataLayers/MyKeyMapDAO.cs`

#### 3.1.1 繼承 KeyMapDAO 基礎類別

```csharp
/// <summary>
/// 代碼查詢資料存取物件
/// 繼承自 Turbo 框架的 KeyMapDAO 基礎類別
/// </summary>
public class MyKeyMapDAO : KeyMapDAO
{
    private new static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    // 代碼查詢方法實作
}
```

**程式碼說明：**

1. **繼承設計**：繼承 `KeyMapDAO` 基礎類別，獲得基本的代碼查詢功能
2. **日誌記錄**：使用 log4net 記錄日誌
3. **擴充功能**：在基礎類別之上擴充 EECOnline 特有的代碼查詢功能

#### 3.1.2 Session 快取代碼查詢

```csharp
/// <summary>
/// 傳入指定的代碼類別(CodeMapType), 以 IList KeyMapModel 的格式回傳系統代碼清單,
/// 會自動檢查 Session 中是否已有 Cache 資料, 有則直接回傳,
/// 若沒有cache資料, 則轉呼叫 KeyMapDao.GetCodeMapList() 取得後 cache 到 Session 中
/// </summary>
/// <param name="codeType">代碼類別</param>
/// <returns>代碼清單</returns>
public IList<KeyMapModel> GetCachedCodeMapList(CodeMapType codeType)
{
    // 第一步：宣告代碼清單變數
    IList<KeyMapModel> list;

    // 第二步：從 Session 中取得快取資料
    object obj = HttpContext.Current.Session["CodeMap_" + codeType.ToString()];

    // 第三步：檢查快取是否存在
    if (obj != null && obj is IList<KeyMapModel>)
    {
        // 快取命中，直接使用快取資料
        list = (IList<KeyMapModel>)obj;
    }
    else
    {
        // 第四步：快取未命中，從資料庫查詢
        var dao = new MyKeyMapDAO();
        list = dao.GetCodeMapList(codeType);

        // 第五步：將查詢結果儲存到 Session 快取
        HttpContext.Current.Session["CodeMap_" + codeType.ToString()] = list;
    }

    // 第六步：建立新的清單，避免修改快取資料
    IList<KeyMapModel> listNew = new List<KeyMapModel>();
    foreach (var item in list)
    {
        KeyMapModel keyMap = new KeyMapModel()
        {
            CODE = item.CODE,
            TEXT = item.TEXT,
            Selected = item.Selected
        };
        listNew.Add(keyMap);
    }

    // 第七步：返回新的清單
    return listNew;
}
```

**程式碼說明：**

1. **雙層快取**：IBatis 快取（1 小時） + Session 快取（使用者 Session 期間）
2. **快取鍵值**：使用 `"CodeMap_" + codeType.ToString()` 作為快取鍵值
3. **快取檢查**：先檢查 Session 快取，命中則直接返回
4. **資料庫查詢**：快取未命中時，呼叫 `GetCodeMapList()` 從資料庫查詢
5. **快取儲存**：將查詢結果儲存到 Session 快取
6. **深度複製**：建立新的清單物件，避免修改快取資料
7. **效能優化**：減少資料庫查詢次數，提升系統效能

#### 3.1.3 帶參數的代碼查詢

```csharp
/// <summary>
/// 取得「代碼-名稱」項目清單
/// </summary>
/// <param name="type">代碼類別（CodeMapType）</param>
/// <param name="parms">（非必要參數）在 SqlMap SQL 內的查詢條件參數。</param>
/// <param name="excludeCodes">（非必要參數）要排除的項目代碼陣列（或是項目代碼集合）</param>
/// <returns>代碼清單</returns>
public IList<KeyMapModel> GetCodeMapList2(CodeMapType type, object parms = null, IEnumerable<string> excludeCodes = null)
{
    // 第一步：檢查 SQL 語句 ID 是否為空
    if (string.IsNullOrEmpty(type.SQL))
    {
        throw new ArgumentNullException("GetCodeMapList2 error: The CodeMapType.SQL value can not be empty.");
    }
    else
    {
        // 第二步：呼叫 IBatis 查詢
        var list = base.QueryForListAll<KeyMapModel>(type.SQL, parms);

        // 第三步：排除指定的代碼項目
        if (excludeCodes != null) this.RemoveListItem(list, excludeCodes);

        // 第四步：返回查詢結果
        return list;
    }
}
```

**程式碼說明：**

1. **參數支援**：支援傳入查詢參數（例如：`sysid`、`srv_cd` 等）
2. **SQL 檢查**：檢查 SQL 語句 ID 是否為空，避免錯誤
3. **IBatis 查詢**：呼叫 `QueryForListAll()` 執行 IBatis 查詢
4. **排除功能**：支援排除指定的代碼項目
5. **靈活應用**：適用於需要動態參數的代碼查詢場景

#### 3.1.4 代碼項目移除方法

```csharp
/// <summary>
/// 從 KeyMapModel 型別項目集合內移除指定項目
/// </summary>
/// <param name="list">項目清單</param>
/// <param name="excludeCodes">要排除的項目代碼陣列（或是項目代碼集合）</param>
public void RemoveListItem(IList<KeyMapModel> list, IEnumerable<string> excludeCodes)
{
    // 第一步：檢查清單是否為空
    if (list != null && list.Count > 0)
    {
        // 第二步：建立待刪除項目清單
        var del = new List<KeyMapModel>();

        // 第三步：遍歷要排除的代碼
        foreach (var code in excludeCodes)
        {
            // 第四步：找出符合的項目
            foreach (var n in list)
            {
                if (n.CODE == code) del.Add(n);
            }
        }

        // 第五步：從清單中移除項目
        foreach (var n in del)
        {
            list.Remove(n);
        }
    }
}
```

**程式碼說明：**

1. **安全檢查**：先檢查清單是否為空，避免 NullReferenceException
2. **待刪除清單**：建立臨時清單儲存待刪除項目，避免在遍歷時修改集合
3. **雙層遍歷**：外層遍歷要排除的代碼，內層遍歷清單項目
4. **代碼比對**：比對 CODE 欄位，找出符合的項目
5. **批次移除**：最後統一移除所有符合的項目

#### 3.1.5 取得代碼名稱方法

```csharp
/// <summary>
/// 傳回郵遞區號中文名稱
/// </summary>
/// <param name="zip_co">郵遞區號代碼</param>
/// <returns>郵遞區號中文名稱</returns>
public string GetZIP_COName(string zip_co)
{
    // 第一步：建立查詢條件
    TblZIPCODE where = new TblZIPCODE();
    where.ZIP_CO = zip_co;

    // 第二步：查詢郵遞區號資料
    BaseDAO dao = new BaseDAO();
    var ZIP_CO_list = dao.GetRowList(where);

    // 第三步：檢查查詢結果
    if (ZIP_CO_list.ToCount() > 0)
    {
        // 第四步：組合中文名稱（縣市 + 鄉鎮 + 街道）
        return ZIP_CO_list.FirstOrDefault().CITYNM +
               ZIP_CO_list.FirstOrDefault().TOWNNM +
               ZIP_CO_list.FirstOrDefault().ROADNM;
    }

    // 第五步：查無資料，返回空字串
    return "";
}
```

**程式碼說明：**

1. **查詢條件**：使用 `TblZIPCODE` 實體建立查詢條件
2. **資料查詢**：呼叫 `BaseDAO.GetRowList()` 查詢郵遞區號資料
3. **結果檢查**：使用 `ToCount()` 檢查查詢結果是否有資料
4. **名稱組合**：將縣市、鄉鎮、街道名稱組合成完整地址
5. **預設值**：查無資料時返回空字串

## 4. 業務邏輯層實作

### 4.1 ShareCodeListModel 下拉選單模型

**檔案位置：** `trunk/Models/ShareCodeListModel.cs`

#### 4.1.1 共用代碼統一模型

```csharp
/// <summary>
/// 共用代碼統一模型(所有下拉選單放置)
/// </summary>
public class ShareCodeListModel
{
    // 下拉選單屬性定義
}
```

**程式碼說明：**

1. **統一管理**：集中管理所有下拉選單的代碼清單
2. **屬性設計**：每個下拉選單對應一個屬性
3. **延遲載入**：使用 `get` 存取子實作延遲載入
4. **可重用性**：在整個系統中重複使用

#### 4.1.2 常用代碼清單

```csharp
#region 常用(是否、停/使用、....)

/// <summary>
/// 否 / 是
/// </summary>
public IList<SelectListItem> YorN_list
{
    get
    {
        // 第一步：定義代碼字典
        var dictionary = new Dictionary<string, string> {
            {"1", "是"},
            {"0", "否"}
        };

        // 第二步：轉換為下拉選單項目
        return MyCommonUtil.ConvertSelItems(dictionary);
    }
}

/// <summary>
/// 停 / 使用
/// </summary>
public IList<SelectListItem> StoporStart_list
{
    get
    {
        // 第一步：定義代碼字典
        var dictionary = new Dictionary<string, string> {
            {"1", "使用"},
            {"0", "停用"},
            {"2", "帳號須變更密碼"},
        };

        // 第二步：轉換為下拉選單項目
        return MyCommonUtil.ConvertSelItems(dictionary);
    }
}

/// <summary>
/// 性別
/// </summary>
public IList<SelectListItem> sex_list
{
    get
    {
        // 第一步：定義代碼字典
        var dictionary = new Dictionary<string, string> {
            {"1", "男"},
            {"2", "女"},
            {"3", "不透漏"}
        };

        // 第二步：轉換為下拉選單項目
        return MyCommonUtil.ConvertSelItems(dictionary);
    }
}

#endregion
```

**程式碼說明：**

1. **字典定義**：使用 `Dictionary<string, string>` 定義代碼和名稱
2. **工具轉換**：呼叫 `MyCommonUtil.ConvertSelItems()` 轉換為下拉選單項目
3. **硬編碼**：常用的固定代碼直接硬編碼在程式中，不需查詢資料庫
4. **效能優化**：避免頻繁查詢資料庫，提升效能
5. **易於維護**：代碼集中在一處，便於維護

#### 4.1.3 資料庫代碼清單

```csharp
/// <summary>
/// 系統名稱清單
/// </summary>
public IList<SelectListItem> sysid_list
{
    get
    {
        // 第一步：建立 DAO 物件
        MyKeyMapDAO dao = new MyKeyMapDAO();

        // 第二步：查詢系統名稱清單
        IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.sysid);

        // 第三步：轉換為下拉選單項目
        return MyCommonUtil.ConvertSelItems(list);
    }
}

/// <summary>
/// 模組名稱清單(根據SYSID)
/// </summary>
public IList<SelectListItem> modules_list(object parms)
{
    // 第一步：建立 DAO 物件
    MyKeyMapDAO dao = new MyKeyMapDAO();

    // 第二步：查詢模組名稱清單（傳入 sysid 參數）
    IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.modules, parms);

    // 第三步：轉換為下拉選單項目
    return MyCommonUtil.ConvertSelItems(list);
}

/// <summary>
/// 取得單位資料
/// </summary>
public IList<SelectListItem> unit_list
{
    get
    {
        // 第一步：建立 DAO 物件
        MyKeyMapDAO dao = new MyKeyMapDAO();

        // 第二步：查詢單位清單
        IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.unit);

        // 第三步：建立新清單並加入「請選擇」選項
        KeyMapModel item = new KeyMapModel();
        var newlist = list;
        item.TEXT = "請選擇";
        item.CODE = "";
        newlist.Add(item);

        // 第四步：按代碼排序
        newlist = newlist.OrderBy(m => m.CODE).ToList();

        // 第五步：轉換為下拉選單項目
        return MyCommonUtil.ConvertSelItems(newlist);
    }
}
```

**程式碼說明：**

1. **DAO 查詢**：呼叫 `MyKeyMapDAO.GetCodeMapList()` 查詢代碼清單
2. **參數傳遞**：`modules_list()` 方法支援傳入參數（例如：`sysid`）
3. **空項目**：部分清單會加入「請選擇」空項目，提升使用者體驗
4. **排序**：使用 LINQ 的 `OrderBy()` 方法排序
5. **統一轉換**：最後統一呼叫 `MyCommonUtil.ConvertSelItems()` 轉換

## 5. 控制器層實作

### 5.1 AjaxController 動態載入

**檔案位置：** `trunk/Controllers/AjaxController.cs`

#### 5.1.1 Ajax 控制器定義

```csharp
/// <summary>
/// 這個類集中放置一些 Ajax 動作會用的的下拉代碼清單控制 action
/// </summary>
[BypassAuthorize]
public class AjaxController : EBaseController
{
    protected static readonly ILog LOG = LogManager.GetLogger(typeof(MvcApplication));

    // Ajax Action 方法定義
}
```

**程式碼說明：**

1. **略過授權**：使用 `[BypassAuthorize]` 屬性略過權限檢查
2. **繼承基礎控制器**：繼承 `EBaseController` 獲得基本功能
3. **日誌記錄**：使用 log4net 記錄日誌
4. **集中管理**：所有 Ajax 代碼查詢集中在一個控制器

#### 5.1.2 取得模組清單

```csharp
/// <summary>
/// Ajax 取得系統對應的模組清單
/// </summary>
/// <param name="sysid">系統代號</param>
/// <returns>HTML Option 字串</returns>
[HttpPost]
public ActionResult GetModuleList(string sysid)
{
    // 第一步：建立共用代碼模型
    ShareCodeListModel model = new ShareCodeListModel();

    // 第二步：準備查詢參數
    var parms = new { sysid = sysid };

    // 第三步：查詢模組清單
    var list = model.modules_list(parms);

    // 第四步：建立 HTML Option 字串並返回
    return MyCommonUtil.BuildOptionHtmlAjaxResult(list, "", "", "請選擇");
}
```

**程式碼說明：**

1. **HTTP POST**：使用 `[HttpPost]` 屬性限制只接受 POST 請求
2. **參數接收**：接收前端傳入的 `sysid` 參數
3. **匿名物件**：使用匿名物件 `new { sysid = sysid }` 傳遞參數
4. **HTML 產生**：呼叫 `BuildOptionHtmlAjaxResult()` 產生 HTML Option 字串
5. **預設選項**：加入「請選擇」預設選項

#### 5.1.3 取得城市清單

```csharp
/// <summary>
/// 申辦項目
/// </summary>
[HttpPost]
public ActionResult GetCity_list(string srv_cd)
{
    // 第一步：建立 Ajax 結果結構
    var result = new AjaxResultStruct();

    // 第二步：建立 DAO 物件
    var dao = new MyKeyMapDAO();

    // 第三步：準備查詢參數
    Hashtable parms = new Hashtable();
    parms["srv_cd"] = srv_cd;

    // 第四步：查詢城市清單
    IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_city_parm, parms);

    // 第五步：移除空項目
    list.Remove(list.Where(m => m.TEXT == null).FirstOrDefault());

    // 第六步：設定結果資料
    result.data = list;

    // 第七步：返回 JSON 結果
    return Content(result.Serialize(), "application/json");
}
```

**程式碼說明：**

1. **結果結構**：使用 `AjaxResultStruct` 封裝返回結果
2. **Hashtable 參數**：使用 `Hashtable` 傳遞參數給 IBatis
3. **LINQ 過濾**：使用 LINQ 移除 TEXT 為 null 的項目
4. **JSON 序列化**：呼叫 `Serialize()` 方法序列化為 JSON
5. **Content-Type**：設定 Content-Type 為 `application/json`

#### 5.1.4 取得郵遞區號名稱

```csharp
/// <summary>
/// Ajax 傳回郵遞區號中文名稱(單筆)
/// </summary>
/// <param name="CODE">郵遞區號代碼</param>
/// <returns>JSON 結果</returns>
[HttpPost]
public ActionResult GetZIP_CO(string CODE)
{
    // 第一步：建立 Ajax 結果結構
    var result = new AjaxResultStruct();

    // 第二步：建立 DAO 物件
    var dao = new MyKeyMapDAO();

    // 第三步：查詢郵遞區號名稱
    result.data = dao.GetZIP_COName(CODE);

    // 第四步：返回 JSON 結果
    return Content(result.Serialize(), "application/json");
}
```

**程式碼說明：**

1. **單筆查詢**：查詢單一郵遞區號的中文名稱
2. **直接賦值**：將查詢結果直接賦值給 `result.data`
3. **簡潔實作**：方法實作簡潔明瞭，易於維護

## 6. 前端視圖應用

### 6.1 Razor 視圖下拉選單

#### 6.1.1 靜態下拉選單

```csharp
@{
    // 第一步：建立共用代碼模型
    ShareCodeListModel codeModel = new ShareCodeListModel();
}

<!-- 第二步：渲染性別下拉選單 -->
@Html.DropDownListFor(m => m.sex, codeModel.sex_list, new { @class = "form-control" })

<!-- 第三步：渲染狀態下拉選單 -->
@Html.DropDownListFor(m => m.status, codeModel.StoporStart_list, new { @class = "form-control" })

<!-- 第四步：渲染是否下拉選單 -->
@Html.DropDownListFor(m => m.enabled, codeModel.YorN_list, new { @class = "form-control" })
```

**程式碼說明：**

1. **模型建立**：在 Razor 視圖中建立 `ShareCodeListModel` 實例
2. **DropDownListFor**：使用 `Html.DropDownListFor()` 輔助方法渲染下拉選單
3. **Lambda 表達式**：使用 Lambda 表達式綁定模型屬性
4. **HTML 屬性**：使用匿名物件設定 HTML 屬性（例如：`class`）
5. **Bootstrap 樣式**：使用 `form-control` 類別套用 Bootstrap 樣式

#### 6.1.2 動態下拉選單（Ajax 聯動）

```html
<!-- 第一步：系統下拉選單 -->
<select id="sysid" name="sysid" class="form-control">
  <option value="">請選擇</option>
  @foreach (var item in codeModel.sysid_list) {
  <option value="@item.Value">@item.Text</option>
  }
</select>

<!-- 第二步：模組下拉選單（根據系統動態載入） -->
<select id="modules" name="modules" class="form-control">
  <option value="">請選擇</option>
</select>

<!-- 第三步：JavaScript 聯動邏輯 -->
<script type="text/javascript">
  $(document).ready(function () {
    // 第四步：監聽系統下拉選單變更事件
    $("#sysid").change(function () {
      // 第五步：取得選中的系統代號
      var sysid = $(this).val();

      // 第六步：清空模組下拉選單
      $("#modules").empty();
      $("#modules").append('<option value="">請選擇</option>');

      // 第七步：如果有選擇系統，則載入模組清單
      if (sysid) {
        // 第八步：發送 Ajax 請求
        $.ajax({
          url: '@Url.Action("GetModuleList", "Ajax")',
          type: "POST",
          data: { sysid: sysid },
          success: function (data) {
            // 第九步：將返回的 HTML Option 插入模組下拉選單
            $("#modules").html(data);
          },
          error: function () {
            alert("載入模組清單失敗");
          }
        });
      }
    });
  });
</script>
```

**程式碼說明：**

1. **靜態系統清單**：系統下拉選單在頁面載入時就產生
2. **動態模組清單**：模組下拉選單根據選中的系統動態載入
3. **jQuery 事件**：使用 jQuery 的 `change()` 事件監聽下拉選單變更
4. **Ajax 請求**：使用 `$.ajax()` 發送非同步請求
5. **HTML 更新**：將返回的 HTML Option 字串直接插入下拉選單
6. **錯誤處理**：提供錯誤處理機制，提升使用者體驗

## 7. 代碼管理最佳實踐

### 7.1 新增代碼種類

#### 7.1.1 資料庫新增代碼

```sql
-- 新增代碼種類範例
INSERT INTO CODE (
    code_type, code_cd, code_name, code_desc,
    status, orderby, created, creater
) VALUES (
    'HOSPITAL_TYPE',    -- 代碼類別
    '01',               -- 代碼值
    '醫學中心',         -- 代碼名稱
    '醫學中心等級醫院', -- 代碼說明
    '1',                -- 啟用狀態
    1,                  -- 排序序號
    GETDATE(),          -- 建立時間
    'admin'             -- 建立者
);

-- 新增更多代碼項目
INSERT INTO CODE (code_type, code_cd, code_name, status, orderby, created, creater)
VALUES ('HOSPITAL_TYPE', '02', '區域醫院', '1', 2, GETDATE(), 'admin');

INSERT INTO CODE (code_type, code_cd, code_name, status, orderby, created, creater)
VALUES ('HOSPITAL_TYPE', '03', '地區醫院', '1', 3, GETDATE(), 'admin');

INSERT INTO CODE (code_type, code_cd, code_name, status, orderby, created, creater)
VALUES ('HOSPITAL_TYPE', '04', '診所', '1', 4, GETDATE(), 'admin');
```

#### 7.1.2 StaticCodeMap 新增代碼類別

```csharp
/// <summary>
/// 醫院類型
/// </summary>
public static CodeMapType Hospital_Type = new CodeMapType("Hospital_Type", "KeyMap.getHospital_Type");
```

#### 7.1.3 KeyMap.xml 新增 SQL 查詢

```xml
<!-- 取得醫院類型 -->
<select id="getHospital_Type" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="EECOnline-cache">
  <![CDATA[
    SELECT code_cd CODE, code_name TEXT
    FROM   CODE
    WHERE  1 = 1
    AND    code_type = 'HOSPITAL_TYPE'
    AND    status = '1'
    ORDER BY orderby, code_cd
  ]]>
</select>
```

#### 7.1.4 ShareCodeListModel 新增屬性

```csharp
/// <summary>
/// 醫院類型清單
/// </summary>
public IList<SelectListItem> hospital_type_list
{
    get
    {
        MyKeyMapDAO dao = new MyKeyMapDAO();
        IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Hospital_Type);
        return MyCommonUtil.ConvertSelItems(list);
    }
}
```

### 7.2 代碼維護注意事項

#### 7.2.1 軟刪除機制

```sql
-- ✓ 正確：使用軟刪除（修改 status 為 '0'）
UPDATE CODE
SET status = '0',
    updated = GETDATE(),
    updater = 'admin'
WHERE code_type = 'HOSPITAL_TYPE'
  AND code_cd = '04';

-- ✗ 錯誤：直接刪除資料
DELETE FROM CODE
WHERE code_type = 'HOSPITAL_TYPE'
  AND code_cd = '04';
```

**說明：**

1. **保留歷史資料**：軟刪除保留歷史資料，避免資料遺失
2. **關聯資料完整性**：避免刪除後導致關聯資料錯誤
3. **可恢復性**：軟刪除的資料可以恢復
4. **審計追蹤**：保留完整的資料變更記錄

#### 7.2.2 快取清除

```csharp
/// <summary>
/// 清除代碼快取
/// 當代碼資料有異動時呼叫
/// </summary>
public static void ClearCodeCache()
{
    // 第一步：清除 Session 快取
    HttpContext.Current.Session.Clear();

    // 第二步：IBatis 快取會在 1 小時後自動過期
    // 如需立即清除，需要重啟應用程式或等待快取過期
}
```

**說明：**

1. **Session 快取**：呼叫 `Session.Clear()` 清除所有 Session 快取
2. **IBatis 快取**：IBatis 快取有效期 1 小時，會自動過期
3. **即時更新**：如需立即生效，建議重啟應用程式
4. **使用時機**：新增、修改、刪除代碼後呼叫

### 7.3 安全性考量

#### 7.3.1 防止 SQL Injection

```csharp
// ✓ 正確：使用參數化查詢
Hashtable parms = new Hashtable();
parms["code_type"] = codeType;
return base.QueryForListAll<KeyMapModel>("KeyMap.getCodeList", parms);

// ✗ 錯誤：字串拼接
string sql = "SELECT * FROM CODE WHERE code_type = '" + codeType + "'";
```

#### 7.3.2 輸入驗證

```csharp
// ✓ 正確：驗證輸入參數
[HttpPost]
public ActionResult GetModuleList(string sysid)
{
    // 第一步：驗證參數
    if (string.IsNullOrWhiteSpace(sysid))
    {
        return Content("", "text/html");
    }

    // 第二步：查詢代碼清單
    ShareCodeListModel model = new ShareCodeListModel();
    var parms = new { sysid = sysid };
    var list = model.modules_list(parms);

    // 第三步：返回結果
    return MyCommonUtil.BuildOptionHtmlAjaxResult(list, "", "", "請選擇");
}
```

## 8. 總結

EECOnline 系統的統一代碼管理機制提供了完整且高效的解決方案：

### 8.1 核心特色

1. **雙表設計**：CODE 和 CODE1 表分別管理一般代碼和特殊代碼
2. **靜態列舉**：StaticCodeMap.CodeMap 提供類型安全的代碼類別定義
3. **IBatis 映射**：KeyMap.xml 集中管理所有代碼查詢 SQL
4. **雙層快取**：IBatis 快取（1 小時） + Session 快取（使用者 Session 期間）
5. **統一模型**：ShareCodeListModel 集中管理所有下拉選單
6. **Ajax 支援**：AjaxController 提供動態載入功能

### 8.2 技術優勢

1. **安全性**：

   - 參數化查詢防止 SQL Injection
   - 輸入驗證確保資料安全
   - 軟刪除保留歷史資料

2. **效能**：

   - 雙層快取減少資料庫查詢
   - IBatis 快取機制
   - Session 快取提升使用者體驗

3. **可維護性**：

   - 集中式管理降低維護成本
   - 清晰的分層架構
   - 統一的命名規範

4. **擴充性**：
   - 靈活的架構設計支援未來擴展
   - 新增代碼種類只需四個步驟
   - 支援動態參數查詢

### 8.3 應用價值

透過這套統一代碼管理機制，EECOnline 系統能夠：

- 為所有下拉選單提供統一的資料來源
- 確保代碼使用的一致性和標準化
- 支援複雜的業務邏輯和代碼關聯
- 提供良好的使用者體驗和系統效能
- 便於系統管理員進行代碼管理和維護

這套機制不僅滿足了當前的業務需求，也為未來的功能擴展和系統整合奠定了堅實的基礎。
