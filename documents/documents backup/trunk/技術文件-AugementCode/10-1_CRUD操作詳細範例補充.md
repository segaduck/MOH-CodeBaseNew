# 電子病歷申請系統 (EECOnline) - CRUD 操作詳細範例補充

> 本文件為「10_資料庫架構與CRUD操作詳細範例.md」的補充文件，提供完整的 CRUD 操作程式碼範例。

## 1. CREATE（新增）操作範例

### 1.1 新增申請主檔

```csharp
using EECOnline.DataLayers;
using EECOnline.Models.Entities;

public class ApplyService
{
    /// <summary>
    /// 建立新的病歷申請
    /// </summary>
    public TblEEC_Apply CreateApply(string user_idno, string user_name, string login_type)
    {
        FrontDAO dao = new FrontDAO();
        
        // 產生申請單號（格式：yyyyMMddHHmmssfff）
        string apply_no = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        
        // 建立申請主檔
        TblEEC_Apply apply = new TblEEC_Apply();
        apply.apply_no = apply_no;
        apply.user_idno = user_idno;
        apply.user_name = user_name;
        apply.login_type = login_type;  // 1:自然人憑證 2:行動自然人憑證 3:身分證+健保卡
        apply.createdatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 執行新增（會自動產生 keyid）
        TblEEC_Apply result = dao.Insert(apply);
        
        return result;
    }
}
```

### 1.2 新增申請明細

```csharp
/// <summary>
/// 新增申請明細（醫院）
/// </summary>
public TblEEC_ApplyDetail CreateApplyDetail(
    string apply_no, 
    string user_idno, 
    string hospital_code,
    string hospital_name,
    string his_range1,
    string his_range2,
    string his_types)
{
    FrontDAO dao = new FrontDAO();
    
    // 產生申請單號（子檔）
    // 格式：yyyyMMddHHmmssfff + 身分證後9碼 + 流水號(001)
    string apply_no_sub = apply_no + user_idno.Substring(1, 9) + "001";
    
    // 建立申請明細
    TblEEC_ApplyDetail detail = new TblEEC_ApplyDetail();
    detail.apply_no = apply_no;
    detail.apply_no_sub = apply_no_sub;
    detail.user_idno = user_idno;
    detail.hospital_code = hospital_code;
    detail.hospital_name = hospital_name;
    detail.his_range1 = his_range1;  // 病歷時間範圍（起）
    detail.his_range2 = his_range2;  // 病歷時間範圍（迄）
    detail.his_types = his_types;    // 病歷類型（多筆以逗號分隔）
    detail.pay_deadline = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");  // 繳費期限7天
    detail.payed = "N";  // 未繳費
    
    // 執行新增
    TblEEC_ApplyDetail result = dao.Insert(detail);
    
    return result;
}
```

### 1.3 批次新增申請價格明細

```csharp
/// <summary>
/// 批次新增申請價格明細
/// </summary>
public void CreateApplyDetailPrices(
    string apply_no,
    string apply_no_sub,
    string user_idno,
    string hospital_code,
    string hospital_name,
    List<HisTypePrice> hisTypePrices)
{
    FrontDAO dao = new FrontDAO();
    
    foreach (var item in hisTypePrices)
    {
        TblEEC_ApplyDetailPrice price = new TblEEC_ApplyDetailPrice();
        price.apply_no = apply_no;
        price.apply_no_sub = apply_no_sub;
        price.user_idno = user_idno;
        price.hospital_code = hospital_code;
        price.hospital_name = hospital_name;
        price.his_type = item.his_type;          // 病歷類型代碼
        price.his_type_name = item.his_type_name; // 病歷類型名稱
        price.price = item.price;                 // 價格
        price.pay_deadline = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
        price.payed = "N";
        price.provide_status = "0";  // 0:預設值（醫院尚未提供）
        price.download_count = 0;
        
        dao.Insert(price);
    }
}

// 輔助類別
public class HisTypePrice
{
    public string his_type { get; set; }
    public string his_type_name { get; set; }
    public int price { get; set; }
}
```

## 2. READ（查詢）操作範例

### 2.1 查詢單筆申請主檔

```csharp
/// <summary>
/// 根據申請單號查詢申請主檔
/// </summary>
public TblEEC_Apply GetApply(string apply_no)
{
    FrontDAO dao = new FrontDAO();
    
    TblEEC_Apply where = new TblEEC_Apply();
    where.apply_no = apply_no;
    
    // 使用 GetRow 取得單筆資料
    TblEEC_Apply result = dao.GetRow(where);
    
    return result;
}
```

### 2.2 查詢使用者的所有申請

```csharp
/// <summary>
/// 查詢使用者的所有申請明細
/// </summary>
public IList<TblEEC_ApplyDetail> GetUserApplyDetails(string user_idno)
{
    FrontDAO dao = new FrontDAO();
    
    TblEEC_ApplyDetail where = new TblEEC_ApplyDetail();
    where.user_idno = user_idno;
    
    // 使用 GetRowList 取得多筆資料
    IList<TblEEC_ApplyDetail> result = dao.GetRowList(where);
    
    return result;
}
```

### 2.3 使用 IBatis SQL Map 進行複雜查詢

```csharp
/// <summary>
/// 查詢申請案件清單（含價格加總）
/// </summary>
public IList<SearchGridModel> GetSearchApplyList(string user_idno, string filterMonth)
{
    FrontDAO dao = new FrontDAO();
    
    Hashtable param = new Hashtable();
    param["user_idno"] = user_idno;
    param["FilterMonth"] = filterMonth;  // 篩選月份（3, 6, 12）
    
    // 呼叫 IBatis SQL Map（Front.xml 中的 getSearchApplyList1）
    IList<SearchGridModel> result = dao.QueryForListAll<SearchGridModel>(
        "Front.getSearchApplyList1", param);
    
    return result;
}
```

**對應的 SQL Map (Front.xml):**
```xml
<select id="getSearchApplyList1" 
        resultClass="EECOnline.Models.SearchGridModel" 
        parameterClass="Hashtable">
    <![CDATA[
    SELECT 
        a.keyid, a.apply_no, a.apply_no_sub, a.user_idno,
        a.hospital_code, a.hospital_name,
        a.his_range1, a.his_range2, a.his_types, a.pay_deadline,
        a.payed, a.payed_datetime,
        (SELECT SUM(b.price) FROM EEC_ApplyDetailPrice b 
         WHERE b.apply_no=a.apply_no AND b.apply_no_sub=a.apply_no_sub) AS price_sum,
        (SELECT createdatetime FROM EEC_Apply c WHERE c.apply_no=a.apply_no) AS createdatetime,
        b.user_birthday
    FROM EEC_ApplyDetail a
    LEFT JOIN EEC_Apply b ON a.apply_no=b.apply_no
    WHERE a.user_idno=#user_idno#
      AND DATEDIFF(MONTH, (SELECT createdatetime FROM EEC_Apply c WHERE c.apply_no=a.apply_no), GETDATE()) <= $FilterMonth$
    ORDER BY a.pay_deadline, a.keyid
    ]]>
</select>
```

### 2.4 查詢醫院的價格設定

```csharp
/// <summary>
/// 查詢醫院的病歷類型價格設定
/// </summary>
public IList<TblEEC_Hospital_SetPrice> GetHospitalPrices(string hospital_code)
{
    FrontDAO dao = new FrontDAO();
    
    TblEEC_Hospital_SetPrice where = new TblEEC_Hospital_SetPrice();
    where.hospital_code = hospital_code;
    
    IList<TblEEC_Hospital_SetPrice> result = dao.GetRowList(where);
    
    // 依排序欄位排序
    return result.OrderBy(x => x.orderby).ToList();
}
```

## 3. UPDATE（更新）操作範例

### 3.1 更新繳費狀態

```csharp
/// <summary>
/// 更新申請明細的繳費狀態
/// </summary>
public void UpdatePayedStatus(
    string apply_no_sub, 
    string payed_orderid,
    string payed_sessionkey,
    string payed_transdate,
    string payed_approvecode)
{
    FrontDAO dao = new FrontDAO();
    
    // WHERE 條件
    TblEEC_ApplyDetail where = new TblEEC_ApplyDetail();
    where.apply_no_sub = apply_no_sub;
    
    // 要更新的資料
    TblEEC_ApplyDetail row = new TblEEC_ApplyDetail();
    row.payed = "Y";  // 已繳費
    row.payed_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    row.payed_orderid = payed_orderid;
    row.payed_sessionkey = payed_sessionkey;
    row.payed_transdate = payed_transdate;
    row.payed_approvecode = payed_approvecode;
    
    // 執行更新
    dao.Update(row, where);
    
    // 同時更新價格明細的繳費狀態
    TblEEC_ApplyDetailPrice priceWhere = new TblEEC_ApplyDetailPrice();
    priceWhere.apply_no_sub = apply_no_sub;
    
    TblEEC_ApplyDetailPrice priceRow = new TblEEC_ApplyDetailPrice();
    priceRow.payed = "Y";
    priceRow.payed_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    
    dao.Update(priceRow, priceWhere);
}
```

### 3.2 更新病歷提供狀態

```csharp
/// <summary>
/// 更新病歷提供狀態（醫院已提供病歷）
/// </summary>
public void UpdateProvideStatus(
    long keyid,
    string provide_bin,
    string provide_ext,
    string provide_status)
{
    FrontDAO dao = new FrontDAO();
    
    // WHERE 條件
    TblEEC_ApplyDetailPrice where = new TblEEC_ApplyDetailPrice();
    where.keyid = keyid;
    
    // 要更新的資料
    TblEEC_ApplyDetailPrice row = new TblEEC_ApplyDetailPrice();
    row.provide_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    row.provide_bin = provide_bin;  // Base64 編碼的病歷檔案
    row.provide_ext = provide_ext;  // 副檔名（pdf, xml, html）
    row.provide_status = provide_status;  // 1:已透過XML轉換 2:已透過XML無轉換
    
    // 執行更新
    dao.Update(row, where);
}
```

### 3.3 更新醫院密碼

```csharp
/// <summary>
/// 更新醫院密碼（含記錄變更歷程）
/// </summary>
public void UpdateHospitalPassword(string hospital_code, string new_pwd, string moduser, string modusername, string modip)
{
    FrontDAO dao = new FrontDAO();
    
    // 1. 更新醫院主檔密碼
    TblEEC_Hospital where = new TblEEC_Hospital();
    where.code = hospital_code;
    
    TblEEC_Hospital row = new TblEEC_Hospital();
    row.PWD = new_pwd;  // 應該先加密
    
    dao.Update(row, where);
    
    // 2. 記錄密碼變更歷程
    TblEEC_Hospital_PWDLOG log = new TblEEC_Hospital_PWDLOG();
    log.hospital_code = hospital_code;
    log.status = "A";  // 啟用
    log.pwd = new_pwd;
    log.moduserid = moduser;
    log.modusername = modusername;
    log.modtime = DateTime.Now;
    log.modip = modip;
    
    dao.Insert(log);
}
```

### 3.4 更新下載次數

```csharp
/// <summary>
/// 更新病歷下載次數
/// </summary>
public void UpdateDownloadCount(long keyid)
{
    FrontDAO dao = new FrontDAO();
    
    // 先查詢目前的下載次數
    TblEEC_ApplyDetailPrice where = new TblEEC_ApplyDetailPrice();
    where.keyid = keyid;
    TblEEC_ApplyDetailPrice current = dao.GetRow(where);
    
    // 更新下載次數 +1
    TblEEC_ApplyDetailPrice row = new TblEEC_ApplyDetailPrice();
    row.download_count = (current.download_count ?? 0) + 1;
    
    dao.Update(row, where);
}
```

## 4. DELETE（刪除）操作範例

### 4.1 實體刪除（不建議）

```csharp
/// <summary>
/// 實體刪除申請明細（不建議使用）
/// </summary>
public void DeleteApplyDetail(string apply_no_sub)
{
    FrontDAO dao = new FrontDAO();
    
    // 先刪除價格明細
    TblEEC_ApplyDetailPrice priceWhere = new TblEEC_ApplyDetailPrice();
    priceWhere.apply_no_sub = apply_no_sub;
    dao.Delete(priceWhere);
    
    // 再刪除申請明細
    TblEEC_ApplyDetail where = new TblEEC_ApplyDetail();
    where.apply_no_sub = apply_no_sub;
    dao.Delete(where);
}
```

### 4.2 軟刪除（建議）

```csharp
/// <summary>
/// 軟刪除最新消息（建議使用）
/// </summary>
public void SoftDeleteNews(int news_id, string moduser, string modusername, string modip)
{
    FrontDAO dao = new FrontDAO();
    
    // WHERE 條件
    TblENEWS where = new TblENEWS();
    where.news_id = news_id;
    
    // 要更新的資料（標記為已刪除）
    TblENEWS row = new TblENEWS();
    row.del_mk = "Y";  // 標記為已刪除
    row.moduser = moduser;
    row.modusername = modusername;
    row.modtime = DateTime.Now;
    row.modip = modip;
    
    // 執行更新（而非刪除）
    dao.Update(row, where);
}
```

## 5. 交易（Transaction）操作範例

### 5.1 完整的申請流程（含交易）

```csharp
/// <summary>
/// 完整的病歷申請流程（使用交易確保資料一致性）
/// </summary>
public string CreateCompleteApply(
    string user_idno,
    string user_name,
    string login_type,
    string hospital_code,
    string hospital_name,
    string his_range1,
    string his_range2,
    List<HisTypePrice> hisTypePrices)
{
    FrontDAO dao = new FrontDAO();
    
    try
    {
        // 開始交易
        dao.BeginTransaction();
        
        // 1. 新增申請主檔
        string apply_no = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        TblEEC_Apply apply = new TblEEC_Apply();
        apply.apply_no = apply_no;
        apply.user_idno = user_idno;
        apply.user_name = user_name;
        apply.login_type = login_type;
        apply.createdatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        dao.Insert(apply);
        
        // 2. 新增申請明細
        string apply_no_sub = apply_no + user_idno.Substring(1, 9) + "001";
        TblEEC_ApplyDetail detail = new TblEEC_ApplyDetail();
        detail.apply_no = apply_no;
        detail.apply_no_sub = apply_no_sub;
        detail.user_idno = user_idno;
        detail.hospital_code = hospital_code;
        detail.hospital_name = hospital_name;
        detail.his_range1 = his_range1;
        detail.his_range2 = his_range2;
        detail.his_types = string.Join(",", hisTypePrices.Select(x => x.his_type));
        detail.pay_deadline = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
        detail.payed = "N";
        dao.Insert(detail);
        
        // 3. 批次新增價格明細
        foreach (var item in hisTypePrices)
        {
            TblEEC_ApplyDetailPrice price = new TblEEC_ApplyDetailPrice();
            price.apply_no = apply_no;
            price.apply_no_sub = apply_no_sub;
            price.user_idno = user_idno;
            price.hospital_code = hospital_code;
            price.hospital_name = hospital_name;
            price.his_type = item.his_type;
            price.his_type_name = item.his_type_name;
            price.price = item.price;
            price.pay_deadline = detail.pay_deadline;
            price.payed = "N";
            price.provide_status = "0";
            price.download_count = 0;
            dao.Insert(price);
        }
        
        // 提交交易
        dao.CommitTransaction();
        
        return apply_no;
    }
    catch (Exception ex)
    {
        // 發生錯誤，回滾交易
        dao.RollbackTransaction();
        throw new Exception("建立申請失敗：" + ex.Message, ex);
    }
}
```

### 5.2 繳費流程（含交易）

```csharp
/// <summary>
/// 繳費流程（更新多個資料表）
/// </summary>
public void ProcessPayment(
    string apply_no_sub,
    string payed_orderid,
    string payed_sessionkey,
    string payed_transdate,
    string payed_approvecode)
{
    FrontDAO dao = new FrontDAO();
    
    try
    {
        dao.BeginTransaction();
        
        // 1. 更新申請明細的繳費狀態
        TblEEC_ApplyDetail detailWhere = new TblEEC_ApplyDetail();
        detailWhere.apply_no_sub = apply_no_sub;
        
        TblEEC_ApplyDetail detailRow = new TblEEC_ApplyDetail();
        detailRow.payed = "Y";
        detailRow.payed_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        detailRow.payed_orderid = payed_orderid;
        detailRow.payed_sessionkey = payed_sessionkey;
        detailRow.payed_transdate = payed_transdate;
        detailRow.payed_approvecode = payed_approvecode;
        
        dao.Update(detailRow, detailWhere);
        
        // 2. 更新所有價格明細的繳費狀態
        TblEEC_ApplyDetailPrice priceWhere = new TblEEC_ApplyDetailPrice();
        priceWhere.apply_no_sub = apply_no_sub;
        
        TblEEC_ApplyDetailPrice priceRow = new TblEEC_ApplyDetailPrice();
        priceRow.payed = "Y";
        priceRow.payed_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        dao.Update(priceRow, priceWhere);
        
        // 3. 記錄訪問記錄
        TblVISIT_RECORD visit = new TblVISIT_RECORD();
        visit.user_idno = detailWhere.user_idno;
        visit.visit_url = "/Payment/Success";
        visit.visit_time = DateTime.Now;
        visit.visit_ip = HttpContext.Current.Request.UserHostAddress;
        dao.Insert(visit);
        
        dao.CommitTransaction();
    }
    catch (Exception ex)
    {
        dao.RollbackTransaction();
        throw new Exception("繳費處理失敗：" + ex.Message, ex);
    }
}
```

## 6. 最佳實踐建議

### 6.1 使用交易確保資料一致性

```csharp
// ✅ 正確：使用交易
dao.BeginTransaction();
try
{
    dao.Insert(apply);
    dao.Insert(detail);
    dao.Insert(price);
    dao.CommitTransaction();
}
catch
{
    dao.RollbackTransaction();
    throw;
}

// ❌ 錯誤：不使用交易（可能導致資料不一致）
dao.Insert(apply);
dao.Insert(detail);  // 如果這裡失敗，apply 已經新增但 detail 沒有
dao.Insert(price);
```

### 6.2 使用軟刪除而非實體刪除

```csharp
// ✅ 正確：軟刪除（保留歷史資料）
TblENEWS row = new TblENEWS();
row.del_mk = "Y";
dao.Update(row, where);

// ❌ 錯誤：實體刪除（資料永久遺失）
dao.Delete(where);
```

### 6.3 記錄審計欄位

```csharp
// ✅ 正確：記錄修改者資訊
TblEEC_Hospital row = new TblEEC_Hospital();
row.PWD = new_pwd;
row.moduser = "admin";
row.modusername = "系統管理員";
row.modtime = DateTime.Now;
row.modip = "192.168.1.1";
dao.Update(row, where);

// ❌ 錯誤：未記錄修改者資訊
TblEEC_Hospital row = new TblEEC_Hospital();
row.PWD = new_pwd;
dao.Update(row, where);
```

### 6.4 使用參數化查詢防止 SQL Injection

```csharp
// ✅ 正確：使用 IBatis 參數化查詢
Hashtable param = new Hashtable();
param["user_idno"] = user_idno;
dao.QueryForList<TblEEC_Apply>("Front.getApply", param);

// ❌ 錯誤：字串拼接（容易受到 SQL Injection 攻擊）
string sql = "SELECT * FROM EEC_Apply WHERE user_idno='" + user_idno + "'";
```

## 7. 總結

本文件提供了 EECOnline 系統的完整 CRUD 操作範例，包括：

1. **CREATE**：新增申請主檔、明細、價格明細
2. **READ**：查詢單筆、多筆、複雜查詢
3. **UPDATE**：更新繳費狀態、病歷提供狀態、密碼
4. **DELETE**：實體刪除、軟刪除
5. **TRANSACTION**：完整的申請流程、繳費流程

### 關鍵要點

- **使用交易**：涉及多個資料表的操作必須使用交易
- **軟刪除優先**：重要資料使用軟刪除而非實體刪除
- **審計追蹤**：記錄所有異動的使用者、時間、IP
- **參數化查詢**：使用 IBatis 的參數化查詢防止 SQL Injection
- **錯誤處理**：使用 try-catch 捕捉例外並回滾交易

這些範例可以直接應用於實際開發中，確保系統的資料一致性、安全性和可追溯性。

