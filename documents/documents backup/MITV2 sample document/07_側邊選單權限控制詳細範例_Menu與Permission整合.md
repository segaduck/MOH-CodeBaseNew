# MIT 微笑標章管理系統 - 側邊選單權限控制詳細範例：Menu 與 Permission 整合

## 功能概述

本文詳細說明 MITAP2024 系統中的側邊選單（Sidebar Menu）和選單項目控制機制。系統採用基於角色的選單權限控制（Role-Based Menu Access Control），透過多層級的樹狀選單結構，結合使用者角色和功能權限，動態產生個人化的選單介面。

## 選單權限控制架構概覽

```
MITAP2024 選單權限控制架構：

使用者登入
    ↓ 身份驗證通過
選單權限查詢
    ↓ 多表關聯查詢
角色權限過濾
    ↓ 基於使用者角色
功能選單篩選
    ↓ 根據 RoleFuncs 關聯
樹狀結構建構
    ↓ 父子關係組織
前端選單渲染
    ↓ 動態顯示/隱藏
使用者個人化選單
```

## 1. 資料庫選單權限架構

### 1.1 選單權限相關資料表結構

```
選單權限資料表關聯圖：

SysApp (系統別)
    ↓ 一對多
SysFunc (功能選單)
    ↓ 自關聯 (ParentGuid)
    ↓ 多對多
RoleFuncs (角色功能關聯)
    ↓ 關聯到
Roles (角色表)
    ↓ 多對多
RoleUsers (使用者角色關聯)
    ↓ 關聯到
Users/ComMan (使用者表)
```

### 1.2 SysFunc 功能選單資料模型

**檔案位置：** `MITAP2024/MITAP2024.Server/Models/Sys/SysFunc.cs`

#### 1.2.1 功能選單資料結構

```csharp
/// <summary>
/// 功能選單資料模型
/// 支援多層級樹狀結構的選單系統
/// </summary>
[Table("SysFunc")]
public partial class SysFunc
{
    /// <summary>
    /// 主鍵 - 功能選單唯一識別碼
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required Guid Guid { get; set; }

    /// <summary>
    /// 系統別主鍵 - 關聯到 SysApp.Guid
    /// 用於區分不同系統的選單（如前台 MITF、後台 DEFAULT）
    /// </summary>
    public required Guid SysAppGuid { get; set; }

    /// <summary>
    /// 選單類型
    /// 0: 目錄（資料夾，可包含子選單）
    /// 1: 功能項目（實際的功能頁面）
    /// </summary>
    public required int DataType { get; set; }

    /// <summary>
    /// 功能代碼 - 用於權限檢查和路由識別
    /// 例如：SYS10（系統管理）、MIT0101（申請案件管理）
    /// </summary>
    [Required, StringLength(10)]
    public required string FuncCode { get; set; }

    /// <summary>
    /// 功能名稱 - 顯示在選單上的文字
    /// 例如：「系統管理」、「申請案件管理」
    /// </summary>
    [Required, StringLength(100)]
    public required string FuncName { get; set; }

    /// <summary>
    /// 功能 URL - 點擊選單項目時導向的路徑
    /// 目錄類型的選單此欄位為 null
    /// </summary>
    [StringLength(100)]
    public string? FuncUrl { get; set; }

    /// <summary>
    /// 上層選單主鍵 - 建立父子關係的關鍵欄位
    /// null 表示為根層級選單
    /// </summary>
    public Guid? ParentGuid { get; set; }

    /// <summary>
    /// 階層深度 - 從 0 開始計算
    /// 0: 根層級選單
    /// 1: 第二層選單
    /// 2: 第三層選單，以此類推
    /// </summary>
    [Required]
    public required int Level { get; set; } = 0;

    /// <summary>
    /// 排序順序 - 同層級選單的顯示順序
    /// 數字越小越優先顯示
    /// </summary>
    [Required]
    public required int Order { get; set; } = 0;

    /// <summary>
    /// 擴充旗標 - 用於特殊功能標記
    /// 例如：是否允許跨單位查詢等
    /// </summary>
    public int? Flag1 { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    public required DateTime CreateDate { get; set; }

    /// <summary>
    /// 建立者姓名
    /// </summary>
    [Required, StringLength(50)]
    public required string CreateUser { get; set; }

    /// <summary>
    /// 最後修改時間
    /// </summary>
    public required DateTime? ModifyDate { get; set; }

    /// <summary>
    /// 最後修改者
    /// </summary>
    [StringLength(50)]
    public string? ModifyUser { get; set; }
}
```

**程式碼說明：**

1. **階層式設計**：透過 `ParentGuid` 和 `Level` 欄位建立多層級選單結構
2. **類型區分**：`DataType` 區分目錄和功能項目，支援不同的顯示邏輯
3. **排序機制**：`Level` 和 `Order` 欄位確保選單按正確順序顯示
4. **系統區分**：`SysAppGuid` 支援多系統共用同一套選單管理機制
5. **權限整合**：`FuncCode` 作為權限檢查的關鍵識別碼

### 1.3 選單權限關聯資料模型

#### 1.3.1 角色功能關聯表

```csharp
/// <summary>
/// 角色授權功能關聯表
/// 定義哪些角色可以存取哪些功能選單
/// </summary>
[Table("RoleFuncs")]
public partial class RoleFuncs
{
    /// <summary>
    /// 主鍵
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required Guid Guid { get; set; }

    /// <summary>
    /// 系統別主鍵 - 關聯到 SysApp.Guid
    /// </summary>
    public required Guid SysAppGuid { get; set; }

    /// <summary>
    /// 角色主鍵 - 關聯到 Roles.Guid
    /// </summary>
    public required Guid RoleGuid { get; set; }

    /// <summary>
    /// 功能主鍵 - 關聯到 SysFunc.Guid
    /// 建立角色與功能選單的多對多關聯
    /// </summary>
    public required Guid FuncGuid { get; set; }

    /// <summary>
    /// 啟用狀態
    /// 0: 啟用（允許存取）
    /// 1: 不啟用（禁止存取）
    /// </summary>
    public int? IsDelete { get; set; } = 0;

    /// <summary>
    /// 是否允許跨單位查詢
    /// 0: 否（僅限本單位資料）
    /// 1: 是（可查詢其他單位資料）
    /// </summary>
    public int? Flag1 { get; set; } = 0;
}
```

#### 1.3.2 使用者角色關聯表

```csharp
/// <summary>
/// 使用者角色關聯表
/// 定義使用者被指派的角色
/// </summary>
[Table("RoleUsers")]
public partial class RoleUsers
{
    /// <summary>
    /// 主鍵
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required Guid Guid { get; set; }

    /// <summary>
    /// 系統別主鍵 - 關聯到 SysApp.Guid
    /// </summary>
    public required Guid SysAppGuid { get; set; }

    /// <summary>
    /// 角色主鍵 - 關聯到 Roles.Guid
    /// </summary>
    public required Guid RoleGuid { get; set; }

    /// <summary>
    /// 使用者主鍵
    /// 前台系統：關聯到 ComMan.Guid
    /// 後台系統：關聯到 Users.Guid
    /// </summary>
    public required Guid UserGuid { get; set; }

    /// <summary>
    /// 啟用狀態
    /// 0: 啟用（角色有效）
    /// 1: 不啟用（角色無效）
    /// </summary>
    public int? IsDelete { get; set; } = 0;
}
```

**程式碼說明：**

1. **多對多關聯**：透過 `RoleFuncs` 和 `RoleUsers` 建立使用者與功能的多對多關聯
2. **系統區分**：每個關聯都包含 `SysAppGuid`，支援多系統獨立權限管理
3. **彈性控制**：`IsDelete` 和 `Flag1` 提供細緻的權限控制選項
4. **雙重使用者系統**：支援前台（ComMan）和後台（Users）兩套使用者系統

## 2. 後端選單權限查詢機制

### 2.1 統一選單查詢服務

**檔案位置：** `MITAP2024/MITAP2024.Server/Common/BaseCls/BasePageHeaderService.cs`

#### 2.1.1 核心選單權限查詢方法

```csharp
/// <summary>
/// 統一的選單權限查詢方法
/// 根據使用者角色動態產生可存取的功能選單
/// </summary>
/// <param name="_context">資料庫上下文</param>
/// <param name="logger">日誌記錄器</param>
/// <param name="jwtkey">JWT Token</param>
/// <param name="q_sysAppCode">系統別代碼（MITF/DEFAULT）</param>
/// <param name="account">使用者帳號</param>
/// <returns>選單查詢結果</returns>
public static PagedQueryResult QueryMenu(MainDbContext _context, ILog logger, string jwtkey, string q_sysAppCode, string account)
{
    PagedQueryResult result = new PagedQueryResult()
    {
        TotalCount = 0,
        DataList = new List<SYS1004Model>(),
        IsSuccess = false
    };

    try
    {
        result.jwtkey = jwtkey;

        // 第一步：驗證系統別代碼
        if (string.IsNullOrEmpty(q_sysAppCode))
        {
            result.IsSuccess = false;
            result.message = "未傳入系統別代碼";
            return result;
        }

        // 第二步：建立權限查詢 SQL
        // 前台系統（MITF）使用 ComMan 表
        string QueryCmd = @"
SELECT f.Guid, f.SysAppGuid, f.DataType, f.FuncCode, f.FuncName, f.FuncUrl,
       f.ParentGuid, f.[Level], f.[Order], f.Flag1, f.CreateDate, f.CreateUser,
       f.ModifyDate, f.ModifyUser, sa.SysCode, sa.[SysName]
FROM SysFunc f
    INNER JOIN SysApp sa ON f.SysAppGuid = sa.Guid
WHERE f.Guid IN (
    SELECT rf.FuncGuid
    FROM RoleFuncs rf
        INNER JOIN Roles r ON r.Guid = rf.RoleGuid
        INNER JOIN RoleUsers ru ON ru.RoleGuid = r.Guid
        INNER JOIN ComMan cm ON cm.Guid = ru.UserGuid
    WHERE cm.ManMail = @useract AND sa.SysCode = @syscode
)
ORDER BY f.[Level], f.[Order], f.FuncCode";

        // 第三步：後台系統（DEFAULT）使用 Users 表
        if (q_sysAppCode == "DEFAULT")
        {
            QueryCmd = @"
SELECT f.Guid, f.SysAppGuid, f.DataType, f.FuncCode, f.FuncName, f.FuncUrl,
       f.ParentGuid, f.[Level], f.[Order], f.Flag1, f.CreateDate, f.CreateUser,
       f.ModifyDate, f.ModifyUser, sa.SysCode, sa.[SysName]
FROM SysFunc f
    INNER JOIN SysApp sa ON f.SysAppGuid = sa.Guid
WHERE f.Guid IN (
    SELECT rf.FuncGuid
    FROM RoleFuncs rf
        INNER JOIN Roles r ON r.Guid = rf.RoleGuid
        INNER JOIN RoleUsers ru ON ru.RoleGuid = r.Guid
        INNER JOIN Users u ON u.Guid = ru.UserGuid
    WHERE u.UserAct = @useract AND sa.SysCode = @syscode
)
ORDER BY f.[Level], f.[Order], f.FuncCode";
        }

        // 第四步：執行資料庫查詢
        List<SYS1004Model> tmpDataList = new List<SYS1004Model>();
        using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
        {
            conn.Open();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = QueryCmd;

            // 設定查詢參數
            cmd.Parameters.Add(new SqlParameter("@useract", account));
            cmd.Parameters.Add(new SqlParameter("@syscode", q_sysAppCode));

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // 第五步：建立選單項目模型
                    SYS1004Model item = new SYS1004Model()
                    {
                        Guid = TextUtils.GuidToString(TextUtils.GetDbDataNotNullGuid(reader, "Guid")),
                        SysAppGuid = TextUtils.GuidToString(TextUtils.GetDbDataNotNullGuid(reader, "SysAppGuid")),
                        ParentGuid = TextUtils.GuidToString(TextUtils.GetDbDataGuid(reader, "ParentGuid")),
                        DataType = TextUtils.GetDbDataNotNullInt(reader, "DataType"),
                        FuncCode = TextUtils.GetDbDataString(reader, "FuncCode"),
                        FuncName = TextUtils.GetDbDataString(reader, "FuncName"),
                        FuncUrl = TextUtils.GetDbDataString(reader, "FuncUrl"),
                        Level = TextUtils.GetDbDataInt(reader, "Level"),
                        Order = TextUtils.GetDbDataInt(reader, "Order"),
                        SysCode = TextUtils.GetDbDataString(reader, "SysCode"),
                        SysName = TextUtils.GetDbDataString(reader, "SysName"),
                        CreateDate = TextUtils.GetDbDataDatetimeToString(reader, "CreateDate"),
                        CreateUser = TextUtils.GetDbDataString(reader, "CreateUser"),
                        ModifyDate = TextUtils.GetDbDataDatetimeToString(reader, "ModifyDate"),
                        ModifyUser = TextUtils.GetDbDataString(reader, "ModifyUser")
                    };
                    tmpDataList.Add(item);
                }
            }
            conn.Close();
        }

        // 第六步：建構樹狀選單結構
        List<SYS1004Model> retrieveFolderDataList = new List<SYS1004Model>();
        List<SYS1004Model> retrieveDataList = tmpDataList.Where(x => x.Level == 0).ToList();

        for (int i = 0; i < retrieveDataList.Count; i++)
        {
            SYS1004Model item = retrieveDataList[i];
            string? funcPath = item.FuncName;
            item.FuncPath = funcPath;

            // 第七步：處理目錄類型的選單項目
            if (item.DataType == 0)  // 0 = 目錄
            {
                retrieveFolderDataList.Add(item);
                // 遞迴建構子選單
                FuncUtils.findChildItem(ref item, tmpDataList, ref retrieveFolderDataList);
            }
            retrieveDataList[i] = item;
        }

        result.TotalCount = tmpDataList.Count;
        result.DataList = retrieveDataList;
        result.IsSuccess = true;
    }
    catch (Exception e)
    {
        result.message = TextUtils.GenErrmsgWithNum(logger, "查詢功能選單失敗", e);
        result.IsSuccess = false;
    }

    return result;
}
```

**程式碼說明：**

1. **多表關聯查詢**：透過 5 個表的 INNER JOIN 確保只取得有權限的選單
2. **系統別區分**：根據 `q_sysAppCode` 決定使用前台或後台的使用者表
3. **階層排序**：按 `Level`、`Order`、`FuncCode` 順序排列，確保選單正確顯示
4. **權限過濾**：只查詢使用者有權限存取的功能選單
5. **樹狀結構準備**：為後續的階層建構做準備

### 2.2 樹狀選單結構建構

**檔案位置：** `MITAP2024/MITAP2024.Server/Main/SYS10/Utils/FuncUtils.cs`

#### 2.2.1 遞迴子選單建構方法

```csharp
/// <summary>
/// 遞迴建構子選單項目
/// 建立完整的多層級樹狀選單結構
/// </summary>
/// <param name="item">當前選單項目（參考參數）</param>
/// <param name="tmpDataList">所有選單項目的平面清單</param>
/// <param name="retrieveFolderDataList">目錄類型選單清單（參考參數）</param>
public static void findChildItem(ref SYS1004Model item, List<SYS1004Model> tmpDataList, ref List<SYS1004Model> retrieveFolderDataList)
{
    // 第一步：檢查當前項目是否有效
    if (string.IsNullOrEmpty(item.Guid) == false)
    {
        string guid = item.Guid;

        // 第二步：查找所有子選單項目
        List<SYS1004Model> childList = tmpDataList.Where(x => x.ParentGuid == guid).ToList();

        // 第三步：處理每個子選單項目
        for (int i = 0; i < childList.Count; i++)
        {
            SYS1004Model child = childList[i];

            // 第四步：建構選單路徑（麵包屑導航）
            string funcPath = item.FuncPath + " > " + child.FuncName;
            child.FuncPath = funcPath;

            // 第五步：如果子項目是目錄，遞迴處理其子項目
            if (child.DataType == 0)  // 0 = 目錄
            {
                retrieveFolderDataList.Add(child);
                // 遞迴呼叫，建構更深層的子選單
                findChildItem(ref child, tmpDataList, ref retrieveFolderDataList);
            }

            childList[i] = child;
        }

        // 第六步：將子選單清單指派給當前項目
        item.Childs = childList;
    }
}
```

**程式碼說明：**

1. **遞迴設計**：透過遞迴呼叫處理任意深度的選單階層
2. **路徑建構**：自動產生選單的完整路徑，便於使用者了解位置
3. **類型區分**：只對目錄類型的選單進行遞迴處理
4. **效能優化**：使用 LINQ 快速查找子項目，避免多次資料庫查詢
5. **參考傳遞**：使用 `ref` 關鍵字確保物件修改生效

## 3. 前端選單渲染機制

### 3.1 Vue.js 選單元件實作

**檔案位置：** `MITAP2024/mitap2024.client/src/components/MenuItem.vue`

#### 3.1.1 遞迴選單項目元件

```vue
<template>
  <!-- 功能項目選單（有 URL 的選單項目） -->
  <li class="nav-item" v-if="!isFolder()">
    <a
      class="nav-link text-dark"
      :href="model?.funcUrl != null ? model?.funcUrl : ''">
      <FontAwesomeIcon :icon="icon" />{{ model?.funcName }}
    </a>
  </li>

  <!-- 目錄選單（下拉式選單） -->
  <li v-if="isFolder()" class="nav-item dropdown">
    <a
      class="nav-link dropdown-toggle text-light"
      data-bs-toggle="dropdown"
      href="#">
      <FontAwesomeIcon :icon="icon" />{{ model?.funcName }}
    </a>
    <!-- 遞迴渲染子選單 -->
    <ul class="dropdown-menu">
      <MenuItem
        v-for="child in model?.childs"
        :key="child.guid"
        :model="child"
        :icon="icon" />
    </ul>
  </li>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { PropType } from "vue";
import { faTag } from "@fortawesome/free-solid-svg-icons";
import type { IconDefinition } from "@fortawesome/free-solid-svg-icons";
import type { SysFunc } from "../composables/CommonDataType";

// 元件屬性定義
const props = defineProps({
  model: { type: Object as PropType<SysFunc> },
  icon: { type: Object as PropType<IconDefinition>, default: faTag }
});

/// <summary>
/// 判斷是否為目錄類型的選單項目
/// 目錄類型：DataType = 0，顯示為下拉選單
/// 功能類型：DataType = 1，顯示為連結項目
/// </summary>
/// <returns>是否為目錄</returns>
const isFolder = (): boolean => {
  return props.model?.dataType === 0; // 0 = 目錄，1 = 功能項目
};
</script>
```

**程式碼說明：**

1. **條件渲染**：根據 `DataType` 決定渲染為連結或下拉選單
2. **遞迴元件**：元件內部呼叫自己，支援無限層級的選單結構
3. **圖示整合**：使用 FontAwesome 圖示增強視覺效果
4. **Bootstrap 整合**：使用 Bootstrap 的下拉選單樣式
5. **TypeScript 支援**：完整的型別定義確保開發時的型別安全

### 3.2 頁面標頭選單整合

**檔案位置：** `MITAP2024/mitap2024.client/src/mitf/PageHeader.vue`

#### 3.2.1 前台選單載入與顯示

```vue
<template>
  <div class="container-fluid bg-light">
    <div>
      <img id="dept" class="rounded" src="../assets/images/dept.png" />
      <span id="title" class="align-middle">{{ sysName }}</span>
    </div>
  </div>

  <!-- 導航選單列 -->
  <nav class="navbar navbar-expand-sm bg-light sticky-top">
    <div class="container-fluid bg-danger text-light sticky-top">
      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#collapsibleNavbar">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="collapsibleNavbar">
        <ul class="navbar-nav">
          <!-- 固定選單項目 -->
          <li class="nav-item" v-if="isLogin">
            <router-link class="nav-link text-light" :to="'/mitf'">
              <FontAwesomeIcon :icon="faHome" />首頁
            </router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link text-light" :to="'/mitf'">
              <FontAwesomeIcon :icon="faNewspaper" />系統公告
            </router-link>
          </li>

          <!-- 動態權限選單項目 -->
          <MenuItem
            :model="item"
            v-for="item in query_result.dataList"
            :key="item.guid" />
        </ul>
      </div>

      <!-- 使用者資訊下拉選單 -->
      <div class="justify-content-end" v-if="isLogin">
        <ul class="navbar-nav">
          <li class="nav-item dropdown">
            <a
              class="nav-link dropdown-toggle text-warning"
              data-bs-toggle="dropdown"
              href="#">
              <FontAwesomeIcon :icon="faUser" />單位：{{
                sdata.comName
              }}&nbsp;使用者：{{ sdata.manName }}
            </a>
            <ul class="dropdown-menu">
              <li>
                <a class="dropdown-item" href="#" @click="doLogout()">登出</a>
              </li>
              <li>
                <a class="dropdown-item" href="#" @click="personInfo()"
                  >個人資料維護</a
                >
              </li>
            </ul>
          </li>
        </ul>
      </div>
    </div>
  </nav>

  <AlertDlg :data="alert_message" />
  <ProcessLoading :loading="loading" />
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { router } from "@/router/routers";
import AlertDlg from "@/components/AlertDlg.vue";
import ProcessLoading from "@/components/ProcessLoading.vue";
import SessionApi from "@/composables/SessionApi";
import { faHome, faUser, faNewspaper } from "@fortawesome/free-solid-svg-icons";
import CommonUtilApi from "@/composables/CommonUtilApi";
import MenuItem from "@/components/MenuItem.vue";
import type {
  JwtData,
  QueryPageData,
  QueryResultBase
} from "@/composables/PageClassApi";
import type { SysFunc } from "@/composables/CommonDataType";

// 響應式資料定義
const loading = ref(false);
const alert_message = ref("");
const sysName = ref("MIT 微笑標章管理系統");
const isLogin = ref(false);
const sdata = ref({
  comName: "",
  manName: ""
});

// 選單查詢結果
const query_result = reactive({
  dataList: [] as SysFunc[],
  isSuccess: false,
  message: ""
});

/// <summary>
/// 查詢使用者可存取的選單項目
/// 根據使用者角色動態載入選單
/// </summary>
const doQueryMenu = (): void => {
  // 第一步：檢查登入狀態
  if (CommonUtilApi().mitf_CheckIsLogin() == false) {
    return;
  }

  alert_message.value = "";
  loading.value = true;

  // 第二步：準備查詢參數
  let formdata = {
    jwtkey: SessionApi().getToken(),
    q_sysAppCode: "MITF", // 前台系統代碼
    q_readRow: 0,
    q_startRow: 0,
    sortFieldNumbers: ""
  };

  // 第三步：發送 API 請求
  CommonUtilApi().mitf_doFetch(
    CommonUtilApi().rootrul() + "api/mitf/PageHeader/QueryDatas",
    formdata,
    afterDoQueryMenu,
    setAlert
  );
};

/// <summary>
/// 選單查詢完成回調
/// 處理伺服器回傳的選單資料
/// </summary>
const afterDoQueryMenu = (json: any): void => {
  let rval = json as QueryResult;

  // 將查詢結果指派給響應式物件
  Object.assign(query_result, rval);
  loading.value = false;

  // 除錯：輸出選單結構到控制台
  console.log("載入的選單項目:", query_result.dataList);
};

/// <summary>
/// 錯誤訊息設定
/// </summary>
const setAlert = (message: string): void => {
  alert_message.value = message;
  loading.value = false;
};

/// <summary>
/// 檢查登入狀態並載入使用者資訊
/// </summary>
const checkLoginStatus = (): void => {
  isLogin.value = CommonUtilApi().mitf_CheckIsLogin();

  if (isLogin.value) {
    // 載入使用者 Session 資料
    const sessionData = SessionApi().getSessionData();
    if (sessionData.fSessionData) {
      sdata.value.comName = sessionData.fSessionData.comName || "";
      sdata.value.manName = sessionData.fSessionData.manName || "";
    }

    // 載入選單
    doQueryMenu();
  }
};

// 元件掛載時初始化
onMounted(() => {
  checkLoginStatus();
});
</script>
```

**程式碼說明：**

1. **條件顯示**：使用 `v-if` 根據登入狀態顯示不同的選單項目
2. **動態載入**：透過 API 呼叫動態載入使用者有權限的選單
3. **響應式更新**：使用 Vue 3 的響應式系統自動更新選單顯示
4. **錯誤處理**：完整的錯誤處理和載入狀態管理
5. **使用者體驗**：顯示載入動畫和使用者資訊

### 3.3 選單資料型別定義

**檔案位置：** `MITAP2024/mitap2024.client/src/composables/CommonDataType.ts`

#### 3.3.1 前端選單資料結構

```typescript
/// <summary>
/// 系統功能選單介面定義
/// 對應後端 SysFunc 資料模型
/// </summary>
export interface SysFunc {
  guid: string; // 選單項目唯一識別碼
  sysAppGuid: string; // 系統別 GUID
  dataType: number; // 選單類型：0=目錄, 1=功能項目
  funcCode: string; // 功能代碼
  funcName: string; // 功能名稱（顯示文字）
  funcUrl: string; // 功能 URL
  funcPath: string; // 選單路徑（麵包屑）
  parentGuid: string; // 父選單 GUID
  level: number; // 階層深度
  order: number; // 排序順序
  childs?: SysFunc[]; // 子選單陣列（遞迴結構）
}

/// <summary>
/// 角色介面定義
/// 用於權限管理
/// </summary>
export interface Role {
  guid: string; // 角色唯一識別碼
  roleCode: string; // 角色代碼
  roleName: string; // 角色名稱
  isDefault: number; // 是否為預設角色
}

/// <summary>
/// 系統別介面定義
/// 用於區分不同系統的選單
/// </summary>
export interface SysApp {
  guid: string; // 系統別唯一識別碼
  code: string; // 系統代碼（MITF/DEFAULT）
  name: string; // 系統名稱
}
```

**程式碼說明：**

1. **型別安全**：完整的 TypeScript 介面定義確保編譯時型別檢查
2. **遞迴結構**：`childs` 屬性支援無限層級的選單結構
3. **可選屬性**：使用 `?` 標記可選屬性，提升程式碼彈性
4. **一致性**：前後端使用相同的資料結構，減少轉換錯誤
5. **擴充性**：預留擴充欄位，便於未來功能增強

## 4. 多角色與多選單權限處理

### 4.1 使用者多角色指派機制

在 MITAP2024 系統中，一個使用者可以被指派多個角色，每個角色都有不同的功能權限。系統會自動合併所有角色的權限，產生使用者的完整選單。

#### 4.1.1 多角色權限合併邏輯

```sql
-- 多角色權限查詢 SQL（前台系統）
SELECT DISTINCT f.Guid, f.SysAppGuid, f.DataType, f.FuncCode, f.FuncName, f.FuncUrl,
       f.ParentGuid, f.[Level], f.[Order], f.Flag1, sa.SysCode, sa.[SysName]
FROM SysFunc f
    INNER JOIN SysApp sa ON f.SysAppGuid = sa.Guid
WHERE f.Guid IN (
    -- 子查詢：取得使用者所有角色的功能權限
    SELECT rf.FuncGuid
    FROM RoleFuncs rf
        INNER JOIN Roles r ON r.Guid = rf.RoleGuid
        INNER JOIN RoleUsers ru ON ru.RoleGuid = r.Guid
        INNER JOIN ComMan cm ON cm.Guid = ru.UserGuid
    WHERE cm.ManMail = @useract           -- 使用者帳號
      AND sa.SysCode = @syscode           -- 系統別
      AND rf.IsDelete = 0                 -- 權限啟用
      AND ru.IsDelete = 0                 -- 角色指派啟用
)
ORDER BY f.[Level], f.[Order], f.FuncCode
```

**權限合併說明：**

1. **DISTINCT 關鍵字**：確保相同功能不會因為多個角色而重複出現
2. **多表關聯**：透過 `RoleUsers` 表取得使用者的所有角色
3. **權限聯集**：使用者擁有所有指派角色的功能權限聯集
4. **狀態檢查**：只考慮啟用狀態的角色和權限

### 4.2 實際多角色應用案例

#### 4.2.1 案例：申請者兼審查委員

```
使用者：張三（email: zhang@example.com）
指派角色：
1. 申請者角色（APPLICANT）
   - 可存取：申請案件管理、申請進度查詢、申請資料維護
2. 審查委員角色（REVIEWER）
   - 可存取：案件審查、審查意見填寫、審查進度管理

最終選單權限：
├── 申請管理
│   ├── 申請案件管理      ✓ (來自申請者角色)
│   ├── 申請進度查詢      ✓ (來自申請者角色)
│   └── 申請資料維護      ✓ (來自申請者角色)
└── 審查管理
    ├── 案件審查          ✓ (來自審查委員角色)
    ├── 審查意見填寫      ✓ (來自審查委員角色)
    └── 審查進度管理      ✓ (來自審查委員角色)
```

#### 4.2.2 角色權限衝突處理

```csharp
/// <summary>
/// 處理角色權限衝突的邏輯
/// 當使用者有多個角色時，採用「最大權限原則」
/// </summary>
public class RoleConflictResolver
{
    /// <summary>
    /// 解決跨單位查詢權限衝突
    /// 如果任一角色允許跨單位查詢，則使用者具備此權限
    /// </summary>
    /// <param name="userRoles">使用者的所有角色</param>
    /// <param name="funcGuid">功能 GUID</param>
    /// <returns>是否允許跨單位查詢</returns>
    public static bool ResolveCrossDeptPermission(List<RoleFuncs> userRoles, Guid funcGuid)
    {
        // 查找該功能在所有角色中的權限設定
        var funcPermissions = userRoles.Where(rf => rf.FuncGuid == funcGuid).ToList();

        // 只要有任一角色允許跨單位查詢，就給予權限
        return funcPermissions.Any(fp => fp.Flag1 == 1);
    }

    /// <summary>
    /// 解決功能存取權限衝突
    /// 只要任一角色有權限，使用者就可以存取
    /// </summary>
    /// <param name="userRoles">使用者的所有角色</param>
    /// <param name="funcGuid">功能 GUID</param>
    /// <returns>是否有存取權限</returns>
    public static bool ResolveFunctionAccess(List<RoleFuncs> userRoles, Guid funcGuid)
    {
        // 查找該功能在所有角色中的權限設定
        var funcPermissions = userRoles.Where(rf => rf.FuncGuid == funcGuid).ToList();

        // 只要有任一角色允許存取且未被停用，就給予權限
        return funcPermissions.Any(fp => fp.IsDelete == 0);
    }
}
```

### 4.3 多層級樹狀選單處理

#### 4.3.1 父子選單權限繼承邏輯

```csharp
/// <summary>
/// 處理目錄類型選單的權限繼承
/// 當子選單有權限時，父選單自動顯示
/// </summary>
/// <param name="_context">資料庫上下文</param>
/// <param name="roles">角色資訊</param>
/// <param name="sessiondata">使用者 Session 資料</param>
public static void DealFuncAuth(MainDbContext _context, Roles roles, SessionDataModel sessiondata)
{
    // 第一步：取得完整的功能選單樹狀結構
    Tuple<List<SYS1004Model>, List<SYS1004Model>, int> funcs = ReadFuncTree(_context, roles.SysAppGuid);
    List<SYS1004Model> firstLevelItems = funcs.Item1;

    // 第二步：遞迴處理每個第一層選單項目
    foreach (SYS1004Model item in firstLevelItems)
    {
        dealEachFunc(item, _context, sessiondata);
    }
}

/// <summary>
/// 遞迴處理每個選單項目的權限
/// </summary>
/// <param name="func">選單項目</param>
/// <param name="_context">資料庫上下文</param>
/// <param name="sessiondata">使用者 Session 資料</param>
private static void dealEachFunc(SYS1004Model func, MainDbContext _context, SessionDataModel sessiondata)
{
    ILog logger = LogManager.GetLogger(AppSettingReader.GetLoggerName());

    // 第一步：如果是目錄類型，處理子選單權限
    if (func.DataType == 0)  // 0 = 目錄
    {
        // 取得所有子功能
        List<SysFunc> childFunc = _context.SysFunc
            .Where(x => x.ParentGuid == Guid.Parse(func.Guid))
            .ToList();

        // 取得目前目錄的角色權限
        List<RoleFuncs> roleFuncs = _context.RoleFuncs
            .Where(x => x.FuncGuid == Guid.Parse(func.Guid))
            .ToList();

        // 第二步：遞迴處理所有子選單
        foreach (SYS1004Model child in func.Childs)
        {
            dealEachFunc(child, _context, sessiondata);
        }

        // 第三步：檢查子功能的權限，決定父目錄是否顯示
        var childAuthRoles = (from r in _context.Roles.ToList()
                              join rf in _context.RoleFuncs.ToList() on r.Guid equals rf.RoleGuid
                              join f in childFunc on rf.FuncGuid equals f.Guid
                              select r).Distinct();

        // 第四步：如果有子功能有權限，確保父目錄也有對應的角色權限
        if (childAuthRoles.Count() > 0)
        {
            foreach (RoleFuncs rolefunc in roleFuncs)
            {
                // 如果父目錄的角色沒有對應的子功能權限，移除該角色權限
                if (childAuthRoles.Count(x => x.Guid == rolefunc.RoleGuid) == 0)
                {
                    _context.RoleFuncs.Remove(rolefunc);
                }
            }

            // 第五步：為有子功能權限的角色自動新增父目錄權限
            foreach (var childAuthRole in childAuthRoles)
            {
                if (roleFuncs.Count(x => x.RoleGuid == childAuthRole.Guid) == 0)
                {
                    // 自動新增父目錄權限
                    RoleFuncs newRoleFunc = new RoleFuncs()
                    {
                        Guid = Guid.NewGuid(),
                        SysAppGuid = childAuthRole.SysAppGuid,
                        RoleGuid = childAuthRole.Guid,
                        FuncGuid = Guid.Parse(func.Guid),
                        IsDelete = 0,
                        Flag1 = 0,
                        CreateDate = DateTime.Now,
                        CreateUser = sessiondata.Name,
                        ModifyDate = DateTime.Now,
                        ModifyUser = sessiondata.Name
                    };
                    _context.RoleFuncs.Add(newRoleFunc);
                }
            }
        }
        else
        {
            // 第六步：如果沒有子功能權限，移除所有父目錄權限
            foreach (RoleFuncs rolefunc in roleFuncs)
            {
                _context.RoleFuncs.Remove(rolefunc);
            }
        }
    }
}
```

**權限繼承說明：**

1. **向上繼承**：子選單有權限時，父目錄自動顯示
2. **權限清理**：子選單無權限時，父目錄自動隱藏
3. **動態調整**：系統自動維護目錄與功能項目的權限一致性
4. **效能優化**：只在權限變更時執行，避免每次查詢都計算

### 4.4 選單快取與效能優化

#### 4.4.1 Redis 選單快取機制

```csharp
/// <summary>
/// 選單快取管理服務
/// 使用 Redis 快取使用者選單，提升載入效能
/// </summary>
public class MenuCacheService
{
    private static readonly string MENU_CACHE_PREFIX = "menu:";
    private static readonly int CACHE_EXPIRE_MINUTES = 60;  // 快取 1 小時

    /// <summary>
    /// 取得使用者選單（優先從快取取得）
    /// </summary>
    /// <param name="userAccount">使用者帳號</param>
    /// <param name="sysAppCode">系統別代碼</param>
    /// <returns>選單資料</returns>
    public static async Task<List<SYS1004Model>> GetUserMenu(string userAccount, string sysAppCode)
    {
        // 第一步：建立快取鍵值
        string cacheKey = $"{MENU_CACHE_PREFIX}{sysAppCode}:{userAccount}";

        // 第二步：嘗試從 Redis 取得快取資料
        IDatabase cache = RedisConnection.Instance.GetDatabase();
        string cachedMenu = await cache.StringGetAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedMenu))
        {
            // 第三步：快取命中，反序列化並回傳
            return JsonSerializer.Deserialize<List<SYS1004Model>>(cachedMenu);
        }

        // 第四步：快取未命中，從資料庫查詢
        using (var context = new MainDbContext())
        {
            var logger = LogManager.GetLogger(AppSettingReader.GetLoggerName());
            var result = BasePageHeaderService.QueryMenu(context, logger, "", sysAppCode, userAccount);

            if (result.IsSuccess && result.DataList != null)
            {
                var menuList = result.DataList.Cast<SYS1004Model>().ToList();

                // 第五步：將查詢結果存入快取
                string menuJson = JsonSerializer.Serialize(menuList);
                await cache.StringSetAsync(cacheKey, menuJson, TimeSpan.FromMinutes(CACHE_EXPIRE_MINUTES));

                return menuList;
            }
        }

        return new List<SYS1004Model>();
    }

    /// <summary>
    /// 清除使用者選單快取
    /// 當使用者角色變更時呼叫
    /// </summary>
    /// <param name="userAccount">使用者帳號</param>
    /// <param name="sysAppCode">系統別代碼</param>
    public static async Task ClearUserMenuCache(string userAccount, string sysAppCode)
    {
        string cacheKey = $"{MENU_CACHE_PREFIX}{sysAppCode}:{userAccount}";
        IDatabase cache = RedisConnection.Instance.GetDatabase();
        await cache.KeyDeleteAsync(cacheKey);
    }

    /// <summary>
    /// 清除所有選單快取
    /// 當選單結構變更時呼叫
    /// </summary>
    public static async Task ClearAllMenuCache()
    {
        IDatabase cache = RedisConnection.Instance.GetDatabase();
        var server = RedisConnection.Instance.GetServer(RedisConnection.Instance.GetEndPoints().First());

        // 取得所有選單快取鍵值
        var keys = server.Keys(pattern: $"{MENU_CACHE_PREFIX}*");

        // 批次刪除
        if (keys.Any())
        {
            await cache.KeyDeleteAsync(keys.ToArray());
        }
    }
}
```

**快取策略說明：**

1. **分層快取**：按使用者和系統別分別快取，提升命中率
2. **自動過期**：設定 1 小時過期時間，平衡效能和資料新鮮度
3. **主動清理**：權限變更時主動清除相關快取
4. **批次操作**：支援批次清除，提升管理效率

## 5. 選單權限控制最佳實務

### 5.1 安全性最佳實務

#### 5.1.1 前後端雙重驗證

```typescript
// 前端：選單項目顯示前的權限檢查
const checkMenuItemPermission = (menuItem: SysFunc): boolean => {
    // 第一步：檢查使用者是否已登入
    if (!CommonUtilApi().mitf_CheckIsLogin()) {
        return false;
    }

    // 第二步：檢查選單項目是否在使用者權限清單中
    const userMenus = query_result.dataList;
    const hasPermission = userMenus.some(menu =>
        menu.funcCode === menuItem.funcCode &&
        menu.dataType === menuItem.dataType
    );

    return hasPermission;
};

// 後端：API 端點的權限檢查
[HttpPost("AccessProtectedFunction")]
public async Task<ActionResult<PagedQueryResult>> AccessProtectedFunction([FromBody] RequestModel model)
{
    MitfSessionDataModel? sessiondata = null;

    // 第一步：身份驗證與授權檢查
    string authResult = _service.CheckIsLoginAndAuth(
        model.jwtkey,
        ref sessiondata,
        "MIT0101"  // 對應的功能代碼
    );

    if (authResult != ConstMsg.SUC_CODE_00001)
    {
        // 權限檢查失敗，記錄安全性事件
        SecurityLogger.LogAuthorizationFailure(
            sessiondata?.ManMail ?? "Unknown",
            "MIT0101",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
        );

        return new PagedQueryResult()
        {
            jwtkey = model.jwtkey,
            IsSuccess = false,
            message = "權限不足或登入已過期"
        };
    }

    // 第二步：執行業務邏輯
    return await _service.ExecuteBusinessLogic(model, sessiondata);
}
```

#### 5.1.2 選單注入攻擊防護

```csharp
/// <summary>
/// 選單資料安全性驗證
/// 防止惡意選單項目注入
/// </summary>
public class MenuSecurityValidator
{
    /// <summary>
    /// 驗證選單項目的安全性
    /// </summary>
    /// <param name="menuItem">選單項目</param>
    /// <returns>是否安全</returns>
    public static bool ValidateMenuItem(SYS1004Model menuItem)
    {
        // 第一步：檢查必要欄位
        if (string.IsNullOrEmpty(menuItem.FuncCode) ||
            string.IsNullOrEmpty(menuItem.FuncName))
        {
            return false;
        }

        // 第二步：檢查功能代碼格式
        if (!Regex.IsMatch(menuItem.FuncCode, @"^[A-Z0-9]{3,10}$"))
        {
            return false;
        }

        // 第三步：檢查 URL 安全性
        if (!string.IsNullOrEmpty(menuItem.FuncUrl))
        {
            // 只允許相對路徑，防止重定向攻擊
            if (menuItem.FuncUrl.StartsWith("http://") ||
                menuItem.FuncUrl.StartsWith("https://") ||
                menuItem.FuncUrl.Contains("javascript:"))
            {
                return false;
            }
        }

        // 第四步：檢查 XSS 攻擊
        if (ContainsXssPattern(menuItem.FuncName))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 檢查是否包含 XSS 攻擊模式
    /// </summary>
    private static bool ContainsXssPattern(string input)
    {
        string[] xssPatterns = {
            "<script", "</script>", "javascript:", "onload=",
            "onerror=", "onclick=", "onmouseover=", "eval("
        };

        return xssPatterns.Any(pattern =>
            input.ToLower().Contains(pattern.ToLower()));
    }
}
```

### 5.2 效能優化最佳實務

#### 5.2.1 選單載入優化策略

```typescript
// 前端：選單懶載入機制
class MenuLazyLoader {
  private static menuCache = new Map<string, SysFunc[]>();
  private static loadingPromises = new Map<string, Promise<SysFunc[]>>();

  /// <summary>
  /// 懶載入選單資料
  /// 避免重複載入相同的選單
  /// </summary>
  static async loadMenu(sysAppCode: string): Promise<SysFunc[]> {
    const cacheKey = `${sysAppCode}_${SessionApi().getToken()}`;

    // 第一步：檢查記憶體快取
    if (this.menuCache.has(cacheKey)) {
      return this.menuCache.get(cacheKey)!;
    }

    // 第二步：檢查是否正在載入中
    if (this.loadingPromises.has(cacheKey)) {
      return await this.loadingPromises.get(cacheKey)!;
    }

    // 第三步：發起載入請求
    const loadingPromise = this.fetchMenuFromServer(sysAppCode);
    this.loadingPromises.set(cacheKey, loadingPromise);

    try {
      const menuData = await loadingPromise;

      // 第四步：儲存到快取
      this.menuCache.set(cacheKey, menuData);

      return menuData;
    } finally {
      // 第五步：清理載入狀態
      this.loadingPromises.delete(cacheKey);
    }
  }

  /// <summary>
  /// 從伺服器取得選單資料
  /// </summary>
  private static async fetchMenuFromServer(
    sysAppCode: string
  ): Promise<SysFunc[]> {
    const formdata = {
      jwtkey: SessionApi().getToken(),
      q_sysAppCode: sysAppCode,
      q_readRow: 0,
      q_startRow: 0,
      sortFieldNumbers: ""
    };

    return new Promise((resolve, reject) => {
      CommonUtilApi().mitf_doFetch(
        CommonUtilApi().rootrul() + "api/mitf/PageHeader/QueryDatas",
        formdata,
        (result: any) => {
          if (result.isSuccess) {
            resolve(result.dataList || []);
          } else {
            reject(new Error(result.message || "載入選單失敗"));
          }
        },
        (error: string) => {
          reject(new Error(error));
        }
      );
    });
  }

  /// <summary>
  /// 清除快取
  /// 當使用者登出或權限變更時呼叫
  /// </summary>
  static clearCache(): void {
    this.menuCache.clear();
    this.loadingPromises.clear();
  }
}
```

#### 5.2.2 資料庫查詢優化

```sql
-- 優化後的選單權限查詢 SQL
-- 使用索引提升查詢效能
CREATE INDEX IX_RoleFuncs_RoleGuid_FuncGuid ON RoleFuncs (RoleGuid, FuncGuid)
    INCLUDE (IsDelete, Flag1);

CREATE INDEX IX_RoleUsers_UserGuid_RoleGuid ON RoleUsers (UserGuid, RoleGuid)
    INCLUDE (IsDelete);

CREATE INDEX IX_SysFunc_Level_Order ON SysFunc ([Level], [Order])
    INCLUDE (FuncCode, FuncName, FuncUrl, ParentGuid, DataType);

-- 優化的選單查詢（使用 CTE 提升可讀性和效能）
WITH UserRoles AS (
    -- 第一步：取得使用者的所有有效角色
    SELECT DISTINCT ru.RoleGuid
    FROM RoleUsers ru
        INNER JOIN ComMan cm ON cm.Guid = ru.UserGuid
        INNER JOIN SysApp sa ON sa.Guid = ru.SysAppGuid
    WHERE cm.ManMail = @useract
      AND sa.SysCode = @syscode
      AND ru.IsDelete = 0
),
UserFunctions AS (
    -- 第二步：取得角色對應的所有功能權限
    SELECT DISTINCT rf.FuncGuid, MAX(rf.Flag1) as MaxFlag1
    FROM RoleFuncs rf
        INNER JOIN UserRoles ur ON ur.RoleGuid = rf.RoleGuid
    WHERE rf.IsDelete = 0
    GROUP BY rf.FuncGuid
)
-- 第三步：取得最終的選單資料
SELECT f.Guid, f.SysAppGuid, f.DataType, f.FuncCode, f.FuncName, f.FuncUrl,
       f.ParentGuid, f.[Level], f.[Order], uf.MaxFlag1 as Flag1,
       sa.SysCode, sa.[SysName]
FROM SysFunc f
    INNER JOIN UserFunctions uf ON uf.FuncGuid = f.Guid
    INNER JOIN SysApp sa ON sa.Guid = f.SysAppGuid
WHERE sa.SysCode = @syscode
ORDER BY f.[Level], f.[Order], f.FuncCode;
```

### 5.3 維護性最佳實務

#### 5.3.1 選單配置管理

```csharp
/// <summary>
/// 選單配置管理服務
/// 提供選單的批次匯入匯出功能
/// </summary>
public class MenuConfigurationService
{
    /// <summary>
    /// 匯出選單配置到 JSON 檔案
    /// </summary>
    /// <param name="sysAppCode">系統別代碼</param>
    /// <returns>JSON 配置字串</returns>
    public static string ExportMenuConfiguration(string sysAppCode)
    {
        using (var context = new MainDbContext())
        {
            var sysApp = context.SysApp.FirstOrDefault(x => x.SysCode == sysAppCode);
            if (sysApp == null) return "{}";

            var menuItems = context.SysFunc
                .Where(x => x.SysAppGuid == sysApp.Guid)
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Order)
                .Select(x => new
                {
                    FuncCode = x.FuncCode,
                    FuncName = x.FuncName,
                    FuncUrl = x.FuncUrl,
                    ParentFuncCode = context.SysFunc
                        .Where(p => p.Guid == x.ParentGuid)
                        .Select(p => p.FuncCode)
                        .FirstOrDefault(),
                    DataType = x.DataType,
                    Level = x.Level,
                    Order = x.Order,
                    Flag1 = x.Flag1
                })
                .ToList();

            return JsonSerializer.Serialize(menuItems, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }

    /// <summary>
    /// 從 JSON 檔案匯入選單配置
    /// </summary>
    /// <param name="sysAppCode">系統別代碼</param>
    /// <param name="jsonConfig">JSON 配置字串</param>
    /// <param name="operatorName">操作者姓名</param>
    /// <returns>匯入結果</returns>
    public static bool ImportMenuConfiguration(string sysAppCode, string jsonConfig, string operatorName)
    {
        try
        {
            using (var context = new MainDbContext())
            {
                var sysApp = context.SysApp.FirstOrDefault(x => x.SysCode == sysAppCode);
                if (sysApp == null) return false;

                // 解析 JSON 配置
                var menuConfigs = JsonSerializer.Deserialize<List<MenuConfigItem>>(jsonConfig);
                if (menuConfigs == null) return false;

                // 建立功能代碼對應表
                var funcCodeMap = new Dictionary<string, Guid>();

                // 第一輪：建立所有選單項目
                foreach (var config in menuConfigs.OrderBy(x => x.Level))
                {
                    var funcGuid = Guid.NewGuid();
                    funcCodeMap[config.FuncCode] = funcGuid;

                    var sysFunc = new SysFunc
                    {
                        Guid = funcGuid,
                        SysAppGuid = sysApp.Guid,
                        FuncCode = config.FuncCode,
                        FuncName = config.FuncName,
                        FuncUrl = config.FuncUrl,
                        DataType = config.DataType,
                        Level = config.Level,
                        Order = config.Order,
                        Flag1 = config.Flag1,
                        CreateDate = DateTime.Now,
                        CreateUser = operatorName,
                        ModifyDate = DateTime.Now,
                        ModifyUser = operatorName
                    };

                    context.SysFunc.Add(sysFunc);
                }

                context.SaveChanges();

                // 第二輪：設定父子關係
                foreach (var config in menuConfigs.Where(x => !string.IsNullOrEmpty(x.ParentFuncCode)))
                {
                    if (funcCodeMap.ContainsKey(config.FuncCode) &&
                        funcCodeMap.ContainsKey(config.ParentFuncCode))
                    {
                        var childFunc = context.SysFunc.Find(funcCodeMap[config.FuncCode]);
                        if (childFunc != null)
                        {
                            childFunc.ParentGuid = funcCodeMap[config.ParentFuncCode];
                        }
                    }
                }

                context.SaveChanges();
                return true;
            }
        }
        catch (Exception ex)
        {
            // 記錄錯誤日誌
            var logger = LogManager.GetLogger(AppSettingReader.GetLoggerName());
            logger.Error($"匯入選單配置失敗: {ex.Message}", ex);
            return false;
        }
    }
}

/// <summary>
/// 選單配置項目資料模型
/// </summary>
public class MenuConfigItem
{
    public string FuncCode { get; set; }
    public string FuncName { get; set; }
    public string? FuncUrl { get; set; }
    public string? ParentFuncCode { get; set; }
    public int DataType { get; set; }
    public int Level { get; set; }
    public int Order { get; set; }
    public int? Flag1 { get; set; }
}
```

## 6. 總結

MITAP2024 系統的側邊選單權限控制機制提供了完整且靈活的解決方案：

### 6.1 核心特色

1. **多層級樹狀結構**：支援無限層級的選單階層，適應複雜的業務需求
2. **基於角色的權限控制**：透過 RBAC 模型實現細緻的權限管理
3. **多角色支援**：使用者可擁有多個角色，系統自動合併權限
4. **前後端整合**：統一的權限檢查機制，確保安全性
5. **效能優化**：多層次快取機制，提升使用者體驗

### 6.2 技術優勢

1. **安全性**：雙重驗證、XSS 防護、SQL 注入防護
2. **可維護性**：模組化設計、配置管理、批次操作
3. **擴充性**：支援多系統、多角色、多權限層級
4. **效能**：資料庫索引優化、Redis 快取、懶載入
5. **使用者體驗**：響應式設計、載入動畫、錯誤處理

### 6.3 應用價值

透過這套選單權限控制機制，MITAP2024 系統能夠：

- 為不同角色的使用者提供個人化的操作介面
- 確保敏感功能只有授權使用者才能存取
- 支援複雜的組織架構和權限需求
- 提供良好的使用者體驗和系統效能
- 便於系統管理員進行權限管理和維護

這套機制不僅滿足了當前的業務需求，也為未來的功能擴展和系統整合奠定了堅實的基礎。
