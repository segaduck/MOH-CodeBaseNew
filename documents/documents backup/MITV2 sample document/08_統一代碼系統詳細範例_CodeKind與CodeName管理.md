# MIT 微笑標章管理系統 - 統一代碼系統詳細範例：Code Kind 與 Code Name 管理

## 功能概述

本文詳細說明 MITAP2024 系統中的統一代碼管理系統（Unified Code Management System）。系統透過 `SysCode` 和 `SysCodeLink` 資料表，建立了一套完整的代碼種類（Code Kind）和代碼名稱（Code Name）管理機制，用於統一管理整個專案中的所有短代碼和對應名稱，包含下拉選單選項、狀態代碼、分類代碼等。

## 統一代碼系統架構概覽

```
MITAP2024 統一代碼系統架構：

代碼種類定義 (Code Kind)
    ↓ 一對多關係
代碼項目 (Code Items)
    ↓ 階層關係
父子代碼結構
    ↓ 關聯關係
代碼連結系統 (SysCodeLink)
    ↓ 應用層面
前端下拉選單
    ↓ 使用者介面
統一的使用者體驗
```

## 1. 資料庫代碼管理架構

### 1.1 SysCode 統一代碼資料模型

**檔案位置：** `MITAP2024/MITAP2024.Server/Models/SysSet/SysCode.cs`

#### 1.1.1 核心代碼資料結構

```csharp
/// <summary>
/// 統一代碼管理資料模型
/// 用於管理整個系統的所有代碼種類和代碼項目
/// </summary>
[Table("SysCode")]
public partial class SysCode
{
    /// <summary>
    /// 主鍵 - 代碼項目唯一識別碼
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required Guid Guid { get; set; }

    /// <summary>
    /// 代碼類別 - 定義代碼的分類
    /// 例如：DEPTTYPE（單位類型）、DELSTATUS（停用狀態）、ACTIVESTATUS（啟用狀態）
    /// </summary>
    [Required, StringLength(50)]
    public required string Kind { get; set; }

    /// <summary>
    /// 代碼 - 實際的代碼值
    /// 例如：01（審查）、02（檢驗）、0（啟用）、-1（停用）
    /// </summary>
    [Required, StringLength(50)]
    public required string Code { get; set; }

    /// <summary>
    /// 代碼名稱 - 顯示給使用者的友善名稱
    /// 例如：「審查」、「檢驗」、「啟用」、「停用」
    /// </summary>
    [Required, StringLength(50)]
    public required string CodeName { get; set; }

    /// <summary>
    /// 代碼說明 - 詳細的代碼描述
    /// 提供更詳細的代碼用途說明
    /// </summary>
    [StringLength(100)]
    public string? CodeDesc { get; set; }

    /// <summary>
    /// 排序順序 - 控制代碼在下拉選單中的顯示順序
    /// 數字越小越優先顯示
    /// </summary>
    public int? Ordinal { get; set; }

    /// <summary>
    /// 上層代碼類別 - 建立階層關係的父類別
    /// 用於建立代碼的階層結構
    /// </summary>
    [StringLength(50)]
    public string? ParentCodeKind { get; set; }

    /// <summary>
    /// 上層代碼 - 建立階層關係的父代碼
    /// 與 ParentCodeKind 配合使用
    /// </summary>
    [StringLength(50)]
    public string? ParentCode { get; set; }

    /// <summary>
    /// 自訂屬性 - 擴充用途的自訂欄位
    /// 可用於儲存特殊的業務邏輯標記
    /// </summary>
    [StringLength(50)]
    public string? Flag1 { get; set; }

    /// <summary>
    /// 自訂屬性說明 - Flag1 的說明文字
    /// 解釋 Flag1 的用途和意義
    /// </summary>
    [StringLength(50)]
    public string? Flag1Desc { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    public required DateTime CreateDate { get; set; }

    /// <summary>
    /// 建立者
    /// </summary>
    [Required, StringLength(50)]
    public string CreateUser { get; set; }

    /// <summary>
    /// 最後修改時間
    /// </summary>
    public DateTime? ModifyDate { get; set; }

    /// <summary>
    /// 最後修改者
    /// </summary>
    [StringLength(128)]
    public string? ModifyUser { get; set; }
}
```

**程式碼說明：**

1. **雙重索引設計**：`Kind` + `Code` 組成唯一索引，確保代碼不重複
2. **階層支援**：透過 `ParentCodeKind` 和 `ParentCode` 建立父子關係
3. **排序機制**：`Ordinal` 欄位控制顯示順序，提升使用者體驗
4. **擴充性設計**：`Flag1` 和 `Flag1Desc` 提供業務邏輯擴充能力
5. **完整審計**：包含建立和修改的時間與使用者記錄

### 1.2 SysCodeLink 代碼關聯資料模型

**檔案位置：** `MITAP2024/MITAP2024.Server/Models/SysSet/SysCodeLink.cs`

#### 1.2.1 代碼關聯結構

```csharp
/// <summary>
/// 代碼關聯資料模型
/// 用於建立不同代碼之間的關聯關係
/// </summary>
[Table("SysCodeLink")]
public partial class SysCodeLink
{
    /// <summary>
    /// 主鍵 - 關聯記錄唯一識別碼
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required Guid Guid { get; set; }

    /// <summary>
    /// 關聯代碼類別1 - 來源代碼的類別
    /// 例如：DEPTTYPE（單位類型）
    /// </summary>
    [Required, StringLength(50)]
    public required string Kind1 { get; set; }

    /// <summary>
    /// 關聯代碼1 - 來源代碼
    /// 例如：01（審查單位）
    /// </summary>
    [Required, StringLength(50)]
    public required string Code1 { get; set; }

    /// <summary>
    /// 關聯代碼類別2 - 目標代碼的類別
    /// 例如：REVIEWTYPE（審查類型）
    /// </summary>
    [Required, StringLength(50)]
    public required string Kind2 { get; set; }

    /// <summary>
    /// 關聯代碼2 - 目標代碼
    /// 例如：TECH（技術審查）
    /// </summary>
    [Required, StringLength(50)]
    public required string Code2 { get; set; }

    /// <summary>
    /// 關聯備註說明 - 描述關聯關係的用途
    /// 例如：「審查單位對應的審查類型」
    /// </summary>
    [StringLength(100)]
    public string? LinkNote { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public required DateTime CreateDate { get; set; }

    /// <summary>
    /// 建立者
    /// </summary>
    [Required, StringLength(50)]
    public required string CreateUser { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? ModifyDate { get; set; }

    /// <summary>
    /// 修改者
    /// </summary>
    [StringLength(50)]
    public string? ModifyUser { get; set; }
}
```

**程式碼說明：**

1. **多對多關聯**：支援任意兩種代碼類別之間的關聯
2. **雙向關聯**：可以建立 A→B 和 B→A 的雙向關聯關係
3. **關聯說明**：`LinkNote` 欄位記錄關聯的業務意義
4. **靈活配置**：支援動態建立和修改代碼關聯關係
5. **審計追蹤**：完整記錄關聯的建立和修改歷程

## 2. 代碼初始化與種子資料

### 2.1 系統預設代碼定義

**檔案位置：** `MITAP2024/MITAP2024.Server/Models/InitDataModals.cs`

#### 2.1.1 核心代碼種類初始化

```csharp
/// <summary>
/// 系統代碼初始化方法
/// 定義系統啟動時的預設代碼資料
/// </summary>
private static void InitSysCode(ModelBuilder modelBuilder)
{
    // 代碼資料格式：[Guid, Kind, Code, CodeName, CodeDesc]
    string[][] syscodedatas = [
        // === 代碼類別定義 ===
        ["247E1493-B445-4C83-BADF-9097614B5A35", "kind", "DEPTTYPE", "單位類型", ""],

        // === 單位類型代碼 ===
        ["4E7427D8-438E-471A-A8A8-6F62E3C22445", "DEPTTYPE", "01", "審查", ""],
        ["2C6373D2-28F4-4609-9A42-FB700DD6D423", "DEPTTYPE", "02", "檢驗", ""],
        ["7500D6A3-F425-47CB-A136-8C4BF0D51B98", "DEPTTYPE", "03", "秘書處", ""],
        ["9D350116-74FA-4593-BB6D-F0539A8BA916", "DEPTTYPE", "00", "未設定", ""],

        // === 停用/不停用狀態 ===
        ["23FDDC8B-63FB-41B5-8614-2540ED7F047E", "kind", "DELSTATUS", "停用/不停用(0/-1)", ""],
        ["4D2B6392-D598-4910-9C55-1CECE75E6FA6", "DELSTATUS", "0", "啟用", ""],
        ["B84D3DEE-1FC7-4802-AEFC-BDE06E76CCE4", "DELSTATUS", "-1", "停用", ""],

        // === 啟用/不啟用狀態 ===
        ["D13B575A-04FD-4006-A07D-C24CB0E34D10", "kind", "ACTIVESTATUS", "啟用/不啟用(1/0)", ""],
        ["8F2A1B3C-5D6E-4F7A-8B9C-1D2E3F4A5B6C", "ACTIVESTATUS", "1", "啟用", ""],
        ["7E1D2C3B-4A5F-6E7D-8C9B-0A1F2E3D4C5B", "ACTIVESTATUS", "0", "不啟用", ""],

        // === 申請狀態代碼 ===
        ["A1B2C3D4-E5F6-7890-ABCD-EF1234567890", "kind", "APPLYSTATUS", "申請狀態", ""],
        ["B2C3D4E5-F6A7-8901-BCDE-F23456789012", "APPLYSTATUS", "DRAFT", "草稿", ""],
        ["C3D4E5F6-A7B8-9012-CDEF-345678901234", "APPLYSTATUS", "SUBMIT", "已送出", ""],
        ["D4E5F6A7-B8C9-0123-DEF4-456789012345", "APPLYSTATUS", "REVIEW", "審查中", ""],
        ["E5F6A7B8-C9D0-1234-EF56-567890123456", "APPLYSTATUS", "APPROVE", "已核准", ""],
        ["F6A7B8C9-D0E1-2345-F678-678901234567", "APPLYSTATUS", "REJECT", "已駁回", ""],

        // === 審查結果代碼 ===
        ["12345678-9ABC-DEF0-1234-56789ABCDEF0", "kind", "REVIEWRESULT", "審查結果", ""],
        ["23456789-ABCD-EF01-2345-6789ABCDEF01", "REVIEWRESULT", "PASS", "通過", ""],
        ["3456789A-BCDE-F012-3456-789ABCDEF012", "REVIEWRESULT", "FAIL", "不通過", ""],
        ["456789AB-CDEF-0123-4567-89ABCDEF0123", "REVIEWRESULT", "PENDING", "待補件", ""],
    ];

    // 批次建立代碼資料
    foreach (var syscodedata in syscodedatas)
    {
        modelBuilder.Entity<SysCode>().HasData(
            new SysCode
            {
                Guid = Guid.Parse(syscodedata[0]),
                Kind = syscodedata[1],
                Code = syscodedata[2],
                CodeName = syscodedata[3],
                CodeDesc = syscodedata[4],
                ParentCodeKind = null,  // 基礎代碼無父層關係
                ParentCode = null,
                Flag1 = null,
                Flag1Desc = null,
                Ordinal = 0,
                CreateDate = DateTime.Parse("2024-01-01 00:00:00"),
                CreateUser = "SYSTEM",
                ModifyDate = DateTime.Parse("2024-01-01 00:00:00"),
                ModifyUser = "SYSTEM"
            }
        );
    }
}
```

**程式碼說明：**

1. **分類組織**：代碼按功能分類組織，便於管理和維護
2. **標準化命名**：使用一致的命名規則，提升可讀性
3. **完整覆蓋**：涵蓋系統中所有常用的狀態和分類代碼
4. **擴充準備**：預留空間供未來新增更多代碼類別
5. **系統初始化**：確保系統啟動時就有完整的基礎代碼

### 2.2 資料庫索引與約束設計

**檔案位置：** `MITAP2024/MITAP2024.Server/Models/MainDbContext.cs`

#### 2.2.1 代碼唯一性約束

```csharp
/// <summary>
/// 資料庫模型建立時的約束設定
/// 確保代碼系統的資料完整性
/// </summary>
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // SysCode 複合唯一索引：Kind + Code
    // 確保同一類別下的代碼不會重複
    modelBuilder.Entity<SysCode>()
        .HasIndex(p => new { p.Kind, p.Code })
        .IsUnique();

    // 其他相關索引設定
    modelBuilder.Entity<SysApp>()
        .HasIndex(p => new { p.SysCode })
        .IsUnique();

    modelBuilder.Entity<Roles>()
        .HasIndex(p => new { p.RoleCode })
        .IsUnique();

    // 初始化系統代碼資料
    InitSysCode(modelBuilder);
}
```

**程式碼說明：**

1. **複合唯一索引**：`Kind` + `Code` 確保代碼在同類別下的唯一性
2. **效能優化**：索引提升代碼查詢的效能
3. **資料完整性**：防止重複代碼造成的業務邏輯錯誤
4. **系統一致性**：統一的索引設計模式
5. **自動初始化**：模型建立時自動載入預設代碼

## 3. 後端代碼查詢服務

### 3.1 統一代碼查詢服務

**檔案位置：** `MITAP2024/MITAP2024.Server/Common/Service/COM0101Service.cs`

#### 3.1.1 代碼查詢核心邏輯

```csharp
/// <summary>
/// 統一代碼查詢服務介面
/// 提供系統中所有代碼查詢的統一入口
/// </summary>
public interface ICOM0101Service
{
    /// <summary>
    /// 依代碼類別查詢代碼清單
    /// 支援階層查詢和條件篩選
    /// </summary>
    /// <param name="queryModel">查詢條件模型</param>
    /// <returns>代碼清單查詢結果</returns>
    Task<ActionResult<PagedQueryResult>> QuerySysCode(COM0101QueryModel queryModel);

    /// <summary>
    /// 查詢單一代碼項目
    /// 用於取得特定代碼的詳細資訊
    /// </summary>
    /// <param name="queryModel">查詢條件模型</param>
    /// <returns>單一代碼查詢結果</returns>
    Task<ActionResult<PagedQueryResult>> QueryOneSysCode(COM0101QueryModel1 queryModel);
}

/// <summary>
/// 統一代碼查詢服務實作
/// 提供高效能的代碼查詢功能
/// </summary>
public class COM0101Service : BaseService, ICOM0101Service
{
    private readonly MainDbContext _context;
    private ILog logger;

    public COM0101Service(MainDbContext context)
    {
        _context = context;
        logger = LogManager.GetLogger("MITAP2024");
    }

    /// <summary>
    /// 查詢代碼清單的核心實作
    /// 支援多種查詢模式和條件篩選
    /// </summary>
    /// <param name="queryModel">查詢條件</param>
    /// <returns>代碼查詢結果</returns>
    public async Task<ActionResult<PagedQueryResult>> QuerySysCode(COM0101QueryModel queryModel)
    {
        return await Task.Run(() =>
        {
            PagedQueryResult result = new PagedQueryResult()
            {
                TotalCount = 0,
                DataList = new List<COM0101Model>(),
                IsSuccess = false
            };

            try
            {
                List<COM0101Model> retrieveDataList = new List<COM0101Model>();

                // 基礎查詢 SQL - 支援階層查詢和排序
                string QueryCmd = @"
                    SELECT sc.Guid, sc.Kind, sc.Code, sc.CodeName,
                           sc.ParentCodeKind, sc.ParentCode, sc.Ordinal,
                           sc.Flag1, sc.Flag1Desc
                    FROM SysCode sc
                    WHERE 1=1
                        AND sc.Kind NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET')
                        AND sc.Code NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET')
                        AND (sc.Kind ='SYSPARAM' OR sc.Code ='SYSPARAM')
                        AND (sc.Kind ='MITPARAMS' OR sc.Code ='MITPARAMS')
                        AND (sc.Kind ='SYSTEMSET' OR sc.Code ='SYSTEMSET')
                        AND sc.Kind=@kind
                        AND sc.ParentCodeKind =@parentkind
                        AND sc.ParentCode =@parentcode
                    ORDER BY sc.Ordinal, sc.Code";

                // 根據代碼類型動態調整查詢條件
                switch (queryModel.q_codetype)
                {
                    case 0: // 一般代碼
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSPARAM' OR sc.Code ='SYSPARAM')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='MITPARAMS' OR sc.Code ='MITPARAMS')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSTEMSET' OR sc.Code ='SYSTEMSET')", "");
                        break;
                    case 1: // 系統參數
                        QueryCmd = QueryCmd.Replace("AND sc.Kind NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET') AND sc.Code NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='MITPARAMS' OR sc.Code ='MITPARAMS')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSTEMSET' OR sc.Code ='SYSTEMSET')", "");
                        break;
                    case 2: // MIT 參數
                        QueryCmd = QueryCmd.Replace("AND sc.Kind NOT IN ('SYSPARAM','MITPARAMS') AND sc.Code NOT IN ('SYSPARAM','MITPARAMS') ", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSPARAM' OR sc.Code ='SYSPARAM')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSTEMSET' OR sc.Code ='SYSTEMSET')", "");
                        break;
                    case 3: // 系統設定
                        QueryCmd = QueryCmd.Replace("AND sc.Kind NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET') AND sc.Code NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='MITPARAMS' OR sc.Code ='MITPARAMS')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSPARAM' OR sc.Code ='SYSPARAM')", "");
                        break;
                    default: // 全部代碼
                        QueryCmd = QueryCmd.Replace("AND sc.Kind NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET') AND sc.Code NOT IN ('SYSPARAM','MITPARAMS','SYSTEMSET')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='MITPARAMS' OR sc.Code ='MITPARAMS')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSPARAM' OR sc.Code ='SYSPARAM')", "");
                        QueryCmd = QueryCmd.Replace("AND (sc.Kind ='SYSTEMSET' OR sc.Code ='SYSTEMSET')", "");
                        break;
                }

                // 動態處理階層查詢條件
                if (string.IsNullOrEmpty(queryModel.q_parentKind))
                {
                    QueryCmd = QueryCmd.Replace("AND sc.ParentCodeKind =@parentkind", "");
                }
                if (string.IsNullOrEmpty(queryModel.q_parentCode))
                {
                    QueryCmd = QueryCmd.Replace("AND sc.ParentCode =@parentcode", "");
                }

                // 執行資料庫查詢
                using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
                {
                    conn.Open();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = QueryCmd;

                    // 設定查詢參數
                    cmd.Parameters.Add(new SqlParameter("@kind", queryModel.q_kind));
                    if (!string.IsNullOrEmpty(queryModel.q_parentKind))
                    {
                        cmd.Parameters.Add(new SqlParameter("@parentkind", queryModel.q_parentKind));
                    }
                    if (!string.IsNullOrEmpty(queryModel.q_parentCode))
                    {
                        cmd.Parameters.Add(new SqlParameter("@parentcode", queryModel.q_parentCode));
                    }

                    // 讀取查詢結果
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retrieveDataList.Add(new COM0101Model
                            {
                                guid = reader["Guid"].ToString(),
                                kind = reader["Kind"].ToString(),
                                code = reader["Code"].ToString(),
                                codeName = reader["CodeName"].ToString(),
                                parentCodeKind = reader["ParentCodeKind"]?.ToString(),
                                parentCode = reader["ParentCode"]?.ToString(),
                                ordinal = Convert.ToInt32(reader["Ordinal"] ?? 0),
                                flag1 = reader["Flag1"]?.ToString(),
                                flag1Desc = reader["Flag1Desc"]?.ToString()
                            });
                        }
                    }
                }

                result.DataList = retrieveDataList;
                result.TotalCount = retrieveDataList.Count;
                result.IsSuccess = true;
            }
            catch (Exception e)
            {
                result.message = TextUtils.GenErrmsgWithNum(logger, "查詢系統代碼失敗", e);
                result.IsSuccess = false;
            }

            return result;
        });
    }
}
```

**程式碼說明：**

1. **動態 SQL 建構**：根據查詢條件動態調整 SQL 語句
2. **多種查詢模式**：支援一般代碼、系統參數、MIT 參數等不同類型
3. **階層查詢支援**：可查詢特定父代碼下的子代碼
4. **效能優化**：使用參數化查詢防止 SQL 注入
5. **錯誤處理**：完整的異常處理和日誌記錄

### 3.2 代碼查詢控制器

**檔案位置：** `MITAP2024/MITAP2024.Server/Common/Controller/COM0101Controller.cs`

#### 3.2.1 統一代碼 API 端點

```csharp
/// <summary>
/// 系統代碼查詢控制器
/// 提供前端統一的代碼查詢 API 端點
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class COM0101Controller : ControllerBase
{
    private ICOM0101Service _service;

    public COM0101Controller(ICOM0101Service service)
    {
        _service = service;
    }

    /// <summary>
    /// 取得代碼清單 API
    /// 前端下拉選單的主要資料來源
    /// </summary>
    /// <param name="queryModel">查詢條件模型</param>
    /// <returns>代碼清單 JSON 回應</returns>
    [HttpPost("GetCode")]
    public async Task<ActionResult<PagedQueryResult>> GetCode([FromBody] COM0101QueryModel queryModel)
    {
        var rdata = await _service.QuerySysCode(queryModel);
        return rdata;
    }

    /// <summary>
    /// 取得單一代碼 API
    /// 用於代碼詳細資訊查詢和驗證
    /// </summary>
    /// <param name="queryModel">單一代碼查詢條件</param>
    /// <returns>單一代碼 JSON 回應</returns>
    [HttpPost("GetOneCode")]
    public async Task<ActionResult<PagedQueryResult>> GetOneCode([FromBody] COM0101QueryModel1 queryModel)
    {
        var rdata = await _service.QueryOneSysCode(queryModel);
        return rdata;
    }
}
```

**程式碼說明：**

1. **RESTful API 設計**：遵循 REST 原則的 API 端點設計
2. **統一回應格式**：使用 `PagedQueryResult` 統一回應格式
3. **非同步處理**：使用 `async/await` 提升 API 效能
4. **清晰的職責分離**：控制器專注於 HTTP 請求處理
5. **標準化命名**：API 端點命名遵循系統慣例

## 4. 前端代碼選擇元件

### 4.1 統一代碼選擇元件

**檔案位置：** `MITAP2024/mitap2024.client/src/components/SelectSysCode.vue`

#### 4.1.1 Vue.js 代碼選擇元件

```vue
<template>
  <!-- 統一代碼選擇下拉選單 -->
  <InputSelect
    :title="title"
    :isRequire="isRequire"
    :isDisabled="isDisabled"
    :data="code"
    :dataText="codename"
    :options="getOptions()"
    :invalid_class="invalid_class"
    :invalid_msg="invalid_msg"
    @update:data="updateData"
    @update:dataText="updateDataText"
    @update:invalid="updateInvalid" />
</template>

<script setup lang="ts">
import { reactive, watch } from "vue";
import InputSelect from "@/components/InputSelect.vue";
import type { OptionModels } from "@/components/InputSelect.vue";
import TextUtilApi from "@/composables/TextUtilApi";
import type { SysCode } from "@/composables/CommonDataType";
import type { QueryResultBase } from "@/composables/PageClassApi";
import CommonUtilApi from "@/composables/CommonUtilApi";

//=============================
// 資料型態宣告
//=============================
interface DataModel extends SysCode {
  // 擴充 SysCode 介面以支援額外屬性
}
type DataModels = DataModel[];

interface QueryResult extends QueryResultBase {
  dataList: DataModels;
  data: null | DataModel;
}

//=============================
// 元件屬性定義
//=============================
interface Props {
  title: string; // 欄位標題
  isRequire?: boolean; // 是否必填
  isDisabled?: boolean; // 是否停用
  kind: string; // 代碼類別（必要）
  parentKind?: string; // 父代碼類別（階層查詢用）
  parentCode?: string; // 父代碼（階層查詢用）
  code?: string; // 目前選中的代碼值
  codename?: string; // 目前選中的代碼名稱
  flag1desc?: string; // 自訂屬性說明
  kindparentkind?: string; // 代碼類別的父類別
  invalid_class?: string; // 驗證錯誤樣式
  invalid_msg?: string; // 驗證錯誤訊息
}

const props = withDefaults(defineProps<Props>(), {
  isRequire: false,
  isDisabled: false,
  parentKind: "",
  parentCode: "",
  code: "",
  codename: "",
  flag1desc: "",
  kindparentkind: "",
  invalid_class: "",
  invalid_msg: ""
});

//=============================
// 事件發射定義
//=============================
interface Emits {
  "update:code": [value: string]; // 代碼值變更事件
  "update:codename": [value: string]; // 代碼名稱變更事件
  "update:flag1desc": [value: string]; // 自訂屬性說明變更事件
  "update:kindparentkind": [value: string]; // 父類別變更事件
  "update:invalid": [value: string]; // 驗證狀態變更事件
}

const emit = defineEmits<Emits>();

//=============================
// 響應式資料
//=============================
let query_result: QueryResult = reactive({
  jwtkey: "",
  isSuccess: false,
  message: "",
  dataList: [],
  data: null,
  totalCount: 0
});

//=============================
// 計算屬性和方法
//=============================

/// <summary>
/// 將查詢結果轉換為下拉選單選項格式
/// </summary>
const getOptions = (): OptionModels => {
  return query_result.dataList.map((item) => ({
    value: item.code || "",
    text: item.codeName || "",
    selected: item.code === props.code
  }));
};

/// <summary>
/// 初始化查詢結果物件
/// </summary>
const initQueryResult = (): void => {
  Object.assign(query_result, {
    jwtkey: "",
    isSuccess: false,
    message: "",
    dataList: [],
    data: null,
    totalCount: 0
  });
};

/// <summary>
/// 從後端 API 取得代碼資料
/// 支援階層查詢和條件篩選
/// </summary>
const fetchCode = (): void => {
  // 檢查必要參數
  if (TextUtilApi().isNullOrEmpty(props.kind)) {
    console.warn("SelectSysCode: kind 參數為必要項目");
    return;
  }

  // 準備查詢參數
  let formdata = {
    q_kind: props.kind, // 代碼類別
    q_parentKind: props.parentKind || "", // 父代碼類別
    q_parentCode: props.parentCode || "", // 父代碼
    q_codetype: 0 // 代碼類型（0=一般代碼）
  };

  // 初始化查詢結果
  initQueryResult();

  // 檢查是否可以執行查詢
  let canquery = true;
  if (
    !TextUtilApi().isNullOrEmpty(props.parentKind) &&
    TextUtilApi().isNullOrEmpty(props.parentCode)
  ) {
    canquery = false; // 有父類別但無父代碼時不查詢
  }

  if (canquery) {
    // 發送 API 請求
    fetch(CommonUtilApi().rootrul() + "api/COM0101/GetCode/", {
      method: "post",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(formdata)
    })
      .then((r) => r.json())
      .then((json) => {
        Object.assign(query_result, json as QueryResult);
        return;
      })
      .catch((error) => {
        console.error("SelectSysCode 查詢失敗:", error);
        query_result.isSuccess = false;
        query_result.message = "查詢代碼失敗";
      });
  }
};

/// <summary>
/// 處理代碼值變更事件
/// 同時更新相關的附加資訊
/// </summary>
const updateData = (val: string) => {
  emit("update:code", val);

  // 查找選中項目的附加資訊
  const selectedItem = query_result.dataList.find((x) => x.code === val);
  if (selectedItem) {
    emit("update:flag1desc", selectedItem.flag1Desc || "");
    emit("update:kindparentkind", selectedItem.parentCodeKind || "");
  }
};

/// <summary>
/// 處理代碼名稱變更事件
/// </summary>
const updateDataText = (val: string) => {
  emit("update:codename", val);
};

/// <summary>
/// 處理驗證狀態變更事件
/// </summary>
const updateInvalid = (val: string) => {
  emit("update:invalid", val);
};

//=============================
// 監聽器設定
//=============================

// 監聽 kind 變更，重新載入代碼資料
watch(
  () => props.kind,
  () => {
    if (props.kind) {
      fetchCode();
    }
  }
);

// 監聽父代碼變更，重新載入子代碼資料
watch(
  () => [props.parentKind, props.parentCode],
  () => {
    fetchCode();
  }
);

//=============================
// 元件初始化
//=============================
fetchCode(); // 元件載入時執行初始查詢
</script>
```

**程式碼說明：**

1. **泛用性設計**：支援任意代碼類別的選擇功能
2. **階層查詢支援**：可根據父代碼動態載入子代碼
3. **響應式更新**：監聽屬性變更自動重新載入資料
4. **事件驅動**：透過事件機制與父元件通訊
5. **錯誤處理**：包含完整的錯誤處理和使用者提示

### 4.2 靜態代碼顯示元件

**檔案位置：** `MITAP2024/mitap2024.client/src/components/InputStaticSyscode.vue`

#### 4.2.1 唯讀代碼顯示元件

```vue
<template>
  <!-- 靜態代碼顯示（唯讀模式） -->
  <Inputlabel :title="title" :isRequire="isRequire" />
  <div :class="dataRowClass('')">
    <div class="col-12">
      <input
        type="text"
        :class="inputTextClass('')"
        :value="displayText"
        readonly
        disabled />
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, computed } from "vue";
import Inputlabel from "@/components/Inputlabel.vue";
import TextUtilApi from "@/composables/TextUtilApi";
import type { SysCode } from "@/composables/CommonDataType";
import type { QueryResultBase } from "@/composables/PageClassApi";
import CommonUtilApi from "@/composables/CommonUtilApi";
import { dataRowClass, inputTextClass } from "@/composables/CssClassApi";

//=============================
// 資料型態宣告
//=============================
interface DataModel extends SysCode {}
type DataModels = DataModel[];

interface QueryResult extends QueryResultBase {
  dataList: DataModels;
  data: null | DataModel;
}

//=============================
// 元件屬性定義
//=============================
interface Props {
  title: string; // 欄位標題
  isRequire?: boolean; // 是否必填（顯示用）
  kind: string; // 代碼類別
  code?: string; // 要顯示的代碼值
}

const props = withDefaults(defineProps<Props>(), {
  isRequire: false,
  code: ""
});

//=============================
// 響應式資料
//=============================
let query_result: QueryResult = reactive({
  jwtkey: "",
  isSuccess: false,
  message: "",
  dataList: [],
  data: null,
  totalCount: 0
});

//=============================
// 計算屬性
//=============================

/// <summary>
/// 計算要顯示的文字內容
/// 格式：代碼名稱 (代碼值)
/// </summary>
const displayText = computed(() => {
  if (query_result.data) {
    const codeName = query_result.data.codeName || "";
    const code = query_result.data.code || "";
    return code ? `${codeName} (${code})` : codeName;
  }
  return props.code || "";
});

//=============================
// 方法定義
//=============================

/// <summary>
/// 初始化查詢結果物件
/// </summary>
const initQueryResult = (): void => {
  Object.assign(query_result, {
    jwtkey: "",
    isSuccess: false,
    message: "",
    dataList: [],
    data: null,
    totalCount: 0
  });
};

/// <summary>
/// 從後端 API 取得單一代碼資料
/// 用於顯示代碼的完整資訊
/// </summary>
const fetchCode = (): void => {
  if (
    TextUtilApi().isNullOrEmpty(props.kind) ||
    TextUtilApi().isNullOrEmpty(props.code)
  ) {
    return;
  }

  let formdata = {
    q_kind: props.kind,
    q_code: props.code?.toString()
  };

  initQueryResult();

  fetch(CommonUtilApi().rootrul() + "api/COM0101/GetOneCode/", {
    method: "post",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(formdata)
  })
    .then((r) => r.json())
    .then((json) => {
      Object.assign(query_result, json as QueryResult);
      return;
    })
    .catch((error) => {
      console.error("InputStaticSyscode 查詢失敗:", error);
    });
};

//=============================
// 元件初始化
//=============================
fetchCode(); // 元件載入時執行查詢
</script>
```

**程式碼說明：**

1. **唯讀顯示**：專門用於顯示代碼資訊，不可編輯
2. **格式化顯示**：以「代碼名稱 (代碼值)」格式顯示
3. **單一代碼查詢**：使用 `GetOneCode` API 取得特定代碼資訊
4. **計算屬性**：使用 Vue 3 的 `computed` 實現響應式顯示
5. **輕量化設計**：專注於顯示功能，減少不必要的複雜性

## 5. 代碼管理功能

### 5.1 系統代碼維護服務

**檔案位置：** `MITAP2024/MITAP2024.Server/Main/SYS11/Service/SYS1102Service.cs`

#### 5.1.1 代碼新增功能

```csharp
/// <summary>
/// 系統代碼新增服務方法
/// 支援代碼關聯和階層結構建立
/// </summary>
/// <param name="model">代碼資料模型</param>
/// <param name="sessiondata">使用者會話資料</param>
/// <returns>新增結果</returns>
public async Task<ActionResult<PagedQueryResult>> Create(SYS1102Model model, SessionDataModel sessiondata)
{
    return await Task.Run(() =>
    {
        PagedQueryResult result = new PagedQueryResult()
        {
            IsSuccess = false,
            message = ""
        };

        try
        {
            // 第一步：資料驗證
            if (CheckModelDataHasError(model, ref result, true))
            {
                result.IsSuccess = false;
                return result;
            }

            // 第二步：檢查重複代碼
            if (_context.SysCode.Where(x => x.Kind == model.Kind && x.Code == model.Code).Count() > 0)
            {
                result.message = "已有重覆的系統代碼";
                result.IsSuccess = false;
                return result;
            }

            // 第三步：建立新代碼實體
            SysCode entity = new SysCode()
            {
                Guid = Guid.NewGuid(),
                Kind = model.Kind,
                Code = model.Code,
                CodeName = model.CodeName,
                CodeDesc = model.CodeDesc,
                ParentCodeKind = model.ParentCodeKind,
                ParentCode = model.ParentCode,
                Flag1 = model.Flag1,
                Flag1Desc = model.Flag1Desc,
                Ordinal = model.Ordinal.Value,
                CreateDate = DateTime.Now,
                CreateUser = sessiondata.Name,
                ModifyDate = DateTime.Now,
                ModifyUser = sessiondata.Name
            };
            _context.SysCode.Add(entity);

            // 第四步：處理代碼關聯（如果有的話）
            if (model.LinkSysCodes != null && model.LinkSysCodes.Count > 0)
            {
                foreach (var slink in model.LinkSysCodes)
                {
                    if (slink.IsDelete != "1") // 非刪除狀態的關聯
                    {
                        SysCodeLink link = new SysCodeLink()
                        {
                            Guid = Guid.NewGuid(),
                            Kind1 = slink.Kind1,
                            Code1 = slink.Code1,
                            Kind2 = slink.Kind2,
                            Code2 = slink.Code2,
                            LinkNote = slink.LinkNote,
                            CreateDate = DateTime.Now,
                            CreateUser = sessiondata.Name,
                            ModifyDate = DateTime.Now,
                            ModifyUser = sessiondata.Name
                        };
                        _context.SysCodeLink.Add(link);
                    }
                }
            }

            // 第五步：儲存變更
            _context.SaveChanges();
            result.Data = ToViewModel(entity);
            result.IsSuccess = true;
            result.message = "新增系統代碼成功";
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "新增系統代碼失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **完整驗證**：包含資料格式驗證和重複檢查
2. **關聯支援**：支援建立代碼間的關聯關係
3. **審計記錄**：記錄建立者和建立時間
4. **交易處理**：使用資料庫交易確保資料一致性
5. **錯誤處理**：完整的異常處理和錯誤訊息

### 5.2 代碼關聯查詢功能

**檔案位置：** `MITAP2024/MITAP2024.Server/Main/SYS11/Service/SYS1102Service.cs`

#### 5.2.1 代碼關聯資料查詢

```csharp
/// <summary>
/// 查詢代碼關聯資料的 SQL 查詢
/// 取得特定代碼的所有關聯項目
/// </summary>
private void LoadCodeLinks(SYS1102Model data, string kind, string code)
{
    // 查詢代碼關聯的 SQL 語句
    var QueryLinkCmd = @"
        SELECT scl.Guid as Guid, scl.Kind1 as Kind1, scl.Code1 as Code1,
               scl.Kind2 as Kind2, scl.Code2 as Code2, sc.CodeName as CodeName,
               sc.CodeDesc as CodeDesc, scl.LinkNote as LinkNote
        FROM SysCode sc
            INNER JOIN SysCodeLink scl ON sc.Kind = scl.Kind2 AND sc.Code = scl.Code2
        WHERE scl.Kind1=@kind AND scl.Code1=@code";

    using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
    {
        conn.Open();
        DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = QueryLinkCmd;
        cmd.Parameters.Add(new SqlParameter("@kind", kind));
        cmd.Parameters.Add(new SqlParameter("@code", code));

        using (DbDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                data.LinkSysCodes.Add(new SYS1101ModelSub
                {
                    Guid = reader["Guid"].ToString(),
                    Kind1 = reader["Kind1"].ToString(),
                    Code1 = reader["Code1"].ToString(),
                    Kind2 = reader["Kind2"].ToString(),
                    Code2 = reader["Code2"].ToString(),
                    CodeName = reader["CodeName"].ToString(),
                    CodeDesc = reader["CodeDesc"].ToString(),
                    LinkNote = reader["LinkNote"].ToString(),
                    IsDelete = "0" // 預設為未刪除狀態
                });
            }
        }
    }
}
```

**程式碼說明：**

1. **關聯查詢**：透過 JOIN 查詢取得代碼關聯和目標代碼資訊
2. **參數化查詢**：使用參數化查詢防止 SQL 注入攻擊
3. **完整資訊**：同時取得關聯資訊和目標代碼的名稱描述
4. **資料封裝**：將查詢結果封裝為業務模型物件
5. **連線管理**：適當的資料庫連線管理和資源釋放

## 6. 實際應用範例

### 6.1 申請表單中的代碼應用

#### 6.1.1 單位類型選擇範例

```vue
<template>
  <!-- 申請表單中的單位類型選擇 -->
  <div class="form-group">
    <SelectSysCode
      title="申請單位類型"
      :isRequire="true"
      kind="DEPTTYPE"
      v-model:code="formData.deptTypeCode"
      v-model:codename="formData.deptTypeName"
      :invalid_class="validation.deptTypeCode.class"
      :invalid_msg="validation.deptTypeCode.message"
      @update:invalid="updateValidation('deptTypeCode', $event)" />
  </div>

  <!-- 申請狀態顯示 -->
  <div class="form-group">
    <InputStaticSyscode
      title="目前狀態"
      kind="APPLYSTATUS"
      :code="formData.statusCode" />
  </div>

  <!-- 審查結果選擇（條件顯示） -->
  <div class="form-group" v-if="showReviewResult">
    <SelectSysCode
      title="審查結果"
      :isRequire="true"
      kind="REVIEWRESULT"
      v-model:code="formData.reviewResultCode"
      v-model:codename="formData.reviewResultName"
      :invalid_class="validation.reviewResultCode.class"
      :invalid_msg="validation.reviewResultCode.message"
      @update:invalid="updateValidation('reviewResultCode', $event)" />
  </div>
</template>

<script setup lang="ts">
import { reactive, computed } from "vue";
import SelectSysCode from "@/components/SelectSysCode.vue";
import InputStaticSyscode from "@/components/InputStaticSyscode.vue";

//=============================
// 表單資料模型
//=============================
interface FormData {
  deptTypeCode: string; // 單位類型代碼
  deptTypeName: string; // 單位類型名稱
  statusCode: string; // 申請狀態代碼
  reviewResultCode: string; // 審查結果代碼
  reviewResultName: string; // 審查結果名稱
}

const formData = reactive<FormData>({
  deptTypeCode: "",
  deptTypeName: "",
  statusCode: "DRAFT", // 預設為草稿狀態
  reviewResultCode: "",
  reviewResultName: ""
});

//=============================
// 驗證狀態管理
//=============================
interface ValidationState {
  class: string;
  message: string;
}

const validation = reactive<Record<string, ValidationState>>({
  deptTypeCode: { class: "", message: "" },
  reviewResultCode: { class: "", message: "" }
});

//=============================
// 計算屬性
//=============================

/// <summary>
/// 計算是否顯示審查結果選擇
/// 只有在審查中或已完成審查時才顯示
/// </summary>
const showReviewResult = computed(() => {
  return ["REVIEW", "APPROVE", "REJECT"].includes(formData.statusCode);
});

//=============================
// 方法定義
//=============================

/// <summary>
/// 更新驗證狀態
/// </summary>
const updateValidation = (field: string, errorMessage: string) => {
  validation[field].class = errorMessage ? "is-invalid" : "";
  validation[field].message = errorMessage;
};

/// <summary>
/// 表單提交前的驗證
/// </summary>
const validateForm = (): boolean => {
  let isValid = true;

  // 驗證單位類型
  if (!formData.deptTypeCode) {
    updateValidation("deptTypeCode", "請選擇申請單位類型");
    isValid = false;
  } else {
    updateValidation("deptTypeCode", "");
  }

  // 驗證審查結果（如果需要的話）
  if (showReviewResult.value && !formData.reviewResultCode) {
    updateValidation("reviewResultCode", "請選擇審查結果");
    isValid = false;
  } else {
    updateValidation("reviewResultCode", "");
  }

  return isValid;
};
</script>
```

**程式碼說明：**

1. **統一介面**：使用統一的代碼選擇元件，確保一致的使用者體驗
2. **動態顯示**：根據申請狀態動態顯示相關的代碼選擇項目
3. **雙向綁定**：同時綁定代碼值和代碼名稱，便於後續處理
4. **驗證整合**：整合表單驗證機制，提供即時的錯誤提示
5. **業務邏輯**：體現實際業務流程中的代碼使用邏輯

### 6.2 階層代碼應用範例

#### 6.2.1 地區城市選擇範例

```vue
<template>
  <!-- 階層代碼選擇：縣市 → 鄉鎮市區 -->
  <div class="row">
    <div class="col-md-6">
      <SelectSysCode
        title="縣市"
        :isRequire="true"
        kind="CITY"
        v-model:code="addressData.cityCode"
        v-model:codename="addressData.cityName"
        @update:code="onCityChange" />
    </div>
    <div class="col-md-6">
      <SelectSysCode
        title="鄉鎮市區"
        :isRequire="true"
        kind="DISTRICT"
        :parentKind="'CITY'"
        :parentCode="addressData.cityCode"
        v-model:code="addressData.districtCode"
        v-model:codename="addressData.districtName" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive } from "vue";
import SelectSysCode from "@/components/SelectSysCode.vue";

//=============================
// 地址資料模型
//=============================
interface AddressData {
  cityCode: string; // 縣市代碼
  cityName: string; // 縣市名稱
  districtCode: string; // 鄉鎮市區代碼
  districtName: string; // 鄉鎮市區名稱
}

const addressData = reactive<AddressData>({
  cityCode: "",
  cityName: "",
  districtCode: "",
  districtName: ""
});

//=============================
// 事件處理
//=============================

/// <summary>
/// 縣市變更時清空鄉鎮市區選擇
/// 確保階層關係的正確性
/// </summary>
const onCityChange = (newCityCode: string) => {
  // 縣市變更時清空下層選擇
  addressData.districtCode = "";
  addressData.districtName = "";
};
</script>
```

**程式碼說明：**

1. **階層關係**：透過 `parentKind` 和 `parentCode` 建立父子關係
2. **動態載入**：子層代碼會根據父層選擇動態載入
3. **資料一致性**：父層變更時自動清空子層選擇
4. **使用者體驗**：提供直觀的階層選擇介面
5. **擴充性**：可輕鬆擴展到更多層級的階層結構

## 7. 統一代碼系統優勢

### 7.1 系統架構優勢

1. **統一管理**：所有代碼集中在 `SysCode` 表中統一管理
2. **靈活擴充**：可輕鬆新增新的代碼類別和代碼項目
3. **階層支援**：支援無限層級的代碼階層結構
4. **關聯機制**：透過 `SysCodeLink` 建立代碼間的關聯關係
5. **效能優化**：透過索引和快取提升查詢效能

### 7.2 開發效率優勢

1. **元件重用**：統一的前端選擇元件可在整個系統中重用
2. **API 統一**：統一的後端 API 減少重複開發
3. **維護簡化**：集中式的代碼管理簡化系統維護
4. **擴展容易**：新增代碼類別無需修改現有程式碼
5. **一致性保證**：確保整個系統的代碼使用一致性

### 7.3 使用者體驗優勢

1. **介面統一**：所有下拉選單具有一致的外觀和行為
2. **載入快速**：優化的查詢機制提供快速的資料載入
3. **操作直觀**：階層選擇和關聯選擇提供直觀的操作體驗
4. **錯誤提示**：完整的錯誤處理和使用者提示
5. **響應式設計**：支援各種裝置和螢幕尺寸

## 8. 總結

MITAP2024 系統的統一代碼管理機制提供了完整且高效的解決方案：

### 8.1 核心特色

1. **統一架構**：透過 `SysCode` 和 `SysCodeLink` 建立完整的代碼管理體系
2. **階層支援**：支援多層級的代碼階層結構，適應複雜業務需求
3. **關聯機制**：靈活的代碼關聯功能，支援複雜的業務邏輯
4. **前後端整合**：統一的 API 和元件設計，確保系統一致性
5. **效能優化**：透過索引、快取和優化查詢提升系統效能

### 8.2 技術價值

1. **可維護性**：集中式管理降低維護成本和複雜度
2. **可擴展性**：靈活的架構設計支援未來功能擴展
3. **可重用性**：統一的元件和服務可在整個系統中重用
4. **一致性**：確保整個系統的代碼使用標準和一致性
5. **效率性**：提升開發效率和系統執行效能

### 8.3 業務價值

透過這套統一代碼管理系統，MITAP2024 能夠：

- 提供一致且直觀的使用者介面體驗
- 支援複雜的業務邏輯和流程需求
- 簡化系統維護和代碼管理工作
- 提升開發效率和程式碼品質
- 為未來的系統擴展奠定堅實基礎

這套機制不僅滿足了當前的業務需求，也為未來的系統發展和整合提供了強大的支撐。
