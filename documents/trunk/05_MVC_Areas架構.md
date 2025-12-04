# EECOnline MVC Areas 架構

## 1. Areas 概述

ASP.NET MVC Areas 允許將大型應用程式劃分為較小的功能區塊，每個 Area 都有自己的 Controllers、Models 和 Views。EECOnline 使用 Areas 來組織不同的功能模組。

### 1.1 Areas 總覽

```mermaid
graph TB
    subgraph "EECOnline Application"
        ROOT[根目錄]
        
        subgraph "後台管理 Areas"
            A1[A1<br/>醫院管理]
            A2[A2<br/>案件管理]
            A3[A3<br/>帳務管理]
            A4[A4<br/>報表統計]
            A5[A5<br/>帳號管理]
            A6[A6<br/>權限管理]
            A7[A7<br/>病歷類型]
            A8[A8<br/>系統日誌]
        end
        
        subgraph "登入模組 Area"
            LOGIN[Login<br/>登入/儀表板]
        end
        
        subgraph "共用模組 Area"
            SHARE[SHARE<br/>共用元件]
        end
        
        subgraph "後台申請 Area"
            BACKAPPLY[BackApply<br/>後台申請]
        end
    end
```

### 1.2 Areas 功能對照表

| Area | 功能模組 | 說明 | 主要 Controller |
|------|----------|------|-----------------|
| A1 | 醫院管理 | 醫院資料、收費標準設定 | C101M, C102M |
| A2 | 案件管理 | 申請案件處理與追蹤 | C101M, C102M |
| A3 | 帳務管理 | 繳費記錄、對帳報表 | C101M-C104M |
| A4 | 報表統計 | 統計報表、資料匯出 | C101M-C103M |
| A5 | 帳號管理 | 使用者帳號維護 | C101M, C102M |
| A6 | 權限管理 | 群組、角色、功能權限 | C101M-C104M |
| A7 | 病歷類型 | 電子病歷項目設定 | C101M-C104M |
| A8 | 系統日誌 | 操作記錄、稽核軌跡 | C101M-C103M |
| Login | 登入模組 | 身份驗證、後台儀表板 | C101M, C102M |
| SHARE | 共用元件 | 郵遞區號、單位選擇等 | GRP, OPERAT, UNIT, ZIP_CO |
| BackApply | 後台申請 | 後台代申請功能 | - |

---

## 2. Area 目錄結構

### 2.1 標準 Area 結構

```
Areas/
├── A1/                           # 醫院管理
│   ├── A1AreaRegistration.cs     # Area 註冊
│   ├── Controllers/              # 控制器
│   │   ├── C101MController.cs    # 醫院資料管理
│   │   └── C102MController.cs    # 收費標準設定
│   ├── Models/                   # 視圖模型
│   │   ├── C101MFormModel.cs     # 查詢表單
│   │   ├── C101MGridModel.cs     # 列表資料
│   │   └── C101MDetailModel.cs   # 詳細資料
│   └── Views/                    # 視圖
│       ├── C101M/                # 醫院資料視圖
│       │   ├── Index.cshtml
│       │   ├── Detail.cshtml
│       │   └── _Grid.cshtml
│       └── C102M/                # 收費標準視圖
│           ├── Index.cshtml
│           └── Detail.cshtml
├── A2/                           # 案件管理
│   └── ...
├── Login/                        # 登入模組
│   └── ...
└── SHARE/                        # 共用元件
    └── ...
```

### 2.2 目錄結構圖

```mermaid
graph TB
    AREAS[Areas/] --> A1[A1/]
    AREAS --> A2[A2/]
    AREAS --> LOGIN[Login/]
    AREAS --> SHARE[SHARE/]
    
    A1 --> A1_REG[A1AreaRegistration.cs]
    A1 --> A1_CTRL[Controllers/]
    A1 --> A1_MODEL[Models/]
    A1 --> A1_VIEW[Views/]
    
    A1_CTRL --> C101M[C101MController.cs]
    A1_CTRL --> C102M[C102MController.cs]
    
    A1_MODEL --> FORM[C101MFormModel.cs]
    A1_MODEL --> GRID[C101MGridModel.cs]
    A1_MODEL --> DETAIL[C101MDetailModel.cs]
    
    A1_VIEW --> V_C101M[C101M/]
    A1_VIEW --> V_C102M[C102M/]
    
    V_C101M --> INDEX[Index.cshtml]
    V_C101M --> DETAIL_V[Detail.cshtml]
    V_C101M --> GRID_V[_Grid.cshtml]
```

---

## 3. Area 註冊

### 3.1 AreaRegistration 類別

每個 Area 都有一個 `AreaRegistration` 類別負責註冊路由：

```csharp
// Areas/A1/A1AreaRegistration.cs
namespace EECOnline.Areas.A1
{
    public class A1AreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "A1"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // 預設路由
            context.MapRoute(
                "A1_default",
                "A1/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
```

### 3.2 全域 Area 註冊

```csharp
// Global.asax.cs
protected void Application_Start()
{
    // 註冊所有 Areas
    AreaRegistration.RegisterAllAreas();
    
    // 其他初始化...
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);
}
```

### 3.3 路由對應

```mermaid
graph LR
    subgraph "URL 路由對應"
        URL1[/A1/C101M/Index] --> CTRL1[A1.C101MController.Index]
        URL2[/A1/C101M/Detail/1] --> CTRL2[A1.C101MController.Detail]
        URL3[/Login/C101M/Login] --> CTRL3[Login.C101MController.Login]
        URL4[/SHARE/ZIP_CO/GetList] --> CTRL4[SHARE.ZIP_COController.GetList]
    end
```

---

## 4. 各 Area 詳細說明

### 4.1 A1 - 醫院管理

```mermaid
graph TB
    subgraph "A1 醫院管理"
        A1_ROOT[A1 Area]
        
        subgraph "C101M 醫院資料管理"
            C101M_INDEX[Index - 列表查詢]
            C101M_NEW[New - 新增]
            C101M_MODIFY[Modify - 修改]
            C101M_SAVE[Save - 儲存]
            C101M_DELETE[Delete - 刪除]
        end
        
        subgraph "C102M 收費標準設定"
            C102M_INDEX[Index - 列表]
            C102M_DETAIL[Detail - 明細]
            C102M_SAVE[Save - 儲存]
        end
        
        A1_ROOT --> C101M_INDEX
        A1_ROOT --> C102M_INDEX
    end
```

**功能說明：**
- **C101M**: 醫院基本資料維護（院所代碼、名稱、地址、聯絡人等）
- **C102M**: 各醫院電子病歷收費標準設定

### 4.2 A2 - 案件管理

```mermaid
graph TB
    subgraph "A2 案件管理"
        A2_ROOT[A2 Area]
        
        subgraph "C101M 案件查詢"
            C101M_QUERY[查詢條件]
            C101M_LIST[案件列表]
            C101M_DETAIL[案件詳情]
        end
        
        subgraph "C102M 案件處理"
            C102M_PROCESS[處理作業]
            C102M_UPLOAD[上傳病歷]
            C102M_STATUS[狀態更新]
        end
        
        A2_ROOT --> C101M_QUERY
        A2_ROOT --> C102M_PROCESS
    end
```

**功能說明：**
- **C101M**: 申請案件查詢與檢視
- **C102M**: 案件處理、病歷上傳、狀態更新

### 4.3 A3 - 帳務管理

```mermaid
graph TB
    subgraph "A3 帳務管理"
        A3_ROOT[A3 Area]
        
        C101M[C101M<br/>繳費記錄查詢]
        C102M[C102M<br/>對帳報表]
        C103M[C103M<br/>收款統計]
        C104M[C104M<br/>匯出作業]
        
        A3_ROOT --> C101M
        A3_ROOT --> C102M
        A3_ROOT --> C103M
        A3_ROOT --> C104M
    end
```

**功能說明：**
- **C101M**: 繳費記錄查詢
- **C102M**: 對帳報表產製
- **C103M**: 收款統計分析
- **C104M**: 資料匯出功能

### 4.4 A4 - 報表統計

```mermaid
graph TB
    subgraph "A4 報表統計"
        A4_ROOT[A4 Area]
        
        C101M[C101M<br/>月報表]
        C102M[C102M<br/>季報表]
        C103M[C103M<br/>年報表]
        
        A4_ROOT --> C101M
        A4_ROOT --> C102M
        A4_ROOT --> C103M
    end
```

**功能說明：**
- **C101M**: 月度統計報表
- **C102M**: 季度統計報表
- **C103M**: 年度統計報表

### 4.5 A5 - 帳號管理

```mermaid
graph TB
    subgraph "A5 帳號管理"
        A5_ROOT[A5 Area]
        
        subgraph "C101M 管理者帳號"
            ADMIN_LIST[帳號列表]
            ADMIN_NEW[新增帳號]
            ADMIN_EDIT[編輯帳號]
            ADMIN_RESET[重設密碼]
        end
        
        subgraph "C102M 醫院帳號"
            HOSP_LIST[帳號列表]
            HOSP_AUTH[授權管理]
        end
        
        A5_ROOT --> ADMIN_LIST
        A5_ROOT --> HOSP_LIST
    end
```

**功能說明：**
- **C101M**: 系統管理員帳號維護
- **C102M**: 醫院管理員帳號管理

### 4.6 A6 - 權限管理

```mermaid
graph TB
    subgraph "A6 權限管理"
        A6_ROOT[A6 Area]
        
        C101M[C101M<br/>群組管理]
        C102M[C102M<br/>角色管理]
        C103M[C103M<br/>功能管理]
        C104M[C104M<br/>權限設定]
        
        A6_ROOT --> C101M
        A6_ROOT --> C102M
        A6_ROOT --> C103M
        A6_ROOT --> C104M
        
        C101M --> C104M
        C102M --> C104M
        C103M --> C104M
    end
```

**功能說明：**
- **C101M**: 群組（Group）管理
- **C102M**: 角色（Role）管理
- **C103M**: 功能（Function）管理
- **C104M**: 權限對應設定

### 4.7 A7 - 病歷類型設定

```mermaid
graph TB
    subgraph "A7 病歷類型"
        A7_ROOT[A7 Area]
        
        C101M[C101M<br/>病歷大類]
        C102M[C102M<br/>病歷項目]
        C103M[C103M<br/>收費項目]
        C104M[C104M<br/>項目對應]
        
        A7_ROOT --> C101M
        A7_ROOT --> C102M
        C101M --> C102M
        C102M --> C103M
        C103M --> C104M
    end
```

**功能說明：**
- **C101M**: 電子病歷大分類管理
- **C102M**: 病歷項目細項設定
- **C103M**: 收費項目設定
- **C104M**: 項目與收費對應

### 4.8 A8 - 系統日誌

```mermaid
graph TB
    subgraph "A8 系統日誌"
        A8_ROOT[A8 Area]
        
        C101M[C101M<br/>操作日誌]
        C102M[C102M<br/>登入日誌]
        C103M[C103M<br/>錯誤日誌]
        
        A8_ROOT --> C101M
        A8_ROOT --> C102M
        A8_ROOT --> C103M
    end
```

**功能說明：**
- **C101M**: 使用者操作記錄
- **C102M**: 登入/登出記錄
- **C103M**: 系統錯誤記錄

### 4.9 Login - 登入模組

```mermaid
graph TB
    subgraph "Login 登入模組"
        LOGIN_ROOT[Login Area]
        
        subgraph "C101M 登入"
            LOGIN_PAGE[登入頁面]
            LOGIN_PROCESS[登入處理]
            LOGOUT[登出]
        end
        
        subgraph "C102M 儀表板"
            DASHBOARD[儀表板首頁]
            QUICK_STAT[快速統計]
            NOTICE[公告訊息]
        end
        
        LOGIN_ROOT --> LOGIN_PAGE
        LOGIN_ROOT --> DASHBOARD
        
        LOGIN_PROCESS --> DASHBOARD
    end
```

**功能說明：**
- **C101M**: 後台登入頁面與驗證
- **C102M**: 後台儀表板與首頁

### 4.10 SHARE - 共用元件

```mermaid
graph TB
    subgraph "SHARE 共用元件"
        SHARE_ROOT[SHARE Area]
        
        GRP[GRPController<br/>群組選擇器]
        OPERAT[OPERATController<br/>操作員選擇器]
        UNIT[UNITController<br/>單位選擇器]
        ZIP_CO[ZIP_COController<br/>郵遞區號選擇器]
        
        SHARE_ROOT --> GRP
        SHARE_ROOT --> OPERAT
        SHARE_ROOT --> UNIT
        SHARE_ROOT --> ZIP_CO
    end
```

**功能說明：**
- **GRP**: 群組下拉選單
- **OPERAT**: 操作人員選擇
- **UNIT**: 單位/機關選擇
- **ZIP_CO**: 郵遞區號（縣市-區鄉鎮）選擇

---

## 5. Controller 標準模式

### 5.1 Controller 繼承結構

```mermaid
classDiagram
    class Controller {
        <<ASP.NET MVC>>
    }
    
    class BaseController {
        #SessionModel Session
        #LoginUserInfo CurrentUser
        #PaginationInfo PageInfo
        +SetPageInfo()
        +GetCurrentUser()
    }
    
    class C101MController {
        -A1DAO _dao
        +Index()
        +Query()
        +New()
        +Modify()
        +Save()
        +Delete()
    }
    
    Controller <|-- BaseController
    BaseController <|-- C101MController
```

### 5.2 標準 CRUD Action

```csharp
// Areas/A1/Controllers/C101MController.cs
namespace EECOnline.Areas.A1.Controllers
{
    [LoginRequired]
    public class C101MController : BaseController
    {
        private A1DAO _dao = new A1DAO();
        
        /// <summary>
        /// 列表頁面
        /// </summary>
        public ActionResult Index()
        {
            var model = new C101MFormModel();
            return View(model);
        }
        
        /// <summary>
        /// 查詢 (AJAX)
        /// </summary>
        [HttpPost]
        public ActionResult Query(C101MFormModel form)
        {
            _dao.SetPageInfo(form.PageSize, form.PageIndex);
            var list = _dao.QueryC101MGrid(form);
            
            return Json(new {
                success = true,
                rows = list,
                total = _dao.PageInfo.TotalCount
            });
        }
        
        /// <summary>
        /// 新增頁面
        /// </summary>
        public ActionResult New()
        {
            var model = new TblEEC_Hospital();
            ViewBag.IsNew = true;
            return View("Detail", model);
        }
        
        /// <summary>
        /// 修改頁面
        /// </summary>
        public ActionResult Modify(string id)
        {
            var model = _dao.GetC101MDetail(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.IsNew = false;
            return View("Detail", model);
        }
        
        /// <summary>
        /// 儲存
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(TblEEC_Hospital model, bool isNew)
        {
            try
            {
                // 驗證
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "資料驗證失敗" });
                }
                
                // 設定異動資訊
                if (isNew)
                {
                    model.CREATE_USER = CurrentUser.UserID;
                    model.CREATE_DATE = DateTime.Now;
                }
                else
                {
                    model.UPDATE_USER = CurrentUser.UserID;
                    model.UPDATE_DATE = DateTime.Now;
                }
                
                // 儲存
                _dao.SaveC101M(model, isNew);
                
                return Json(new { success = true, message = "儲存成功" });
            }
            catch (Exception ex)
            {
                LogUtils.Error("Save Error", ex);
                return Json(new { success = false, message = "儲存失敗：" + ex.Message });
            }
        }
        
        /// <summary>
        /// 刪除
        /// </summary>
        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                _dao.DeleteC101M(id);
                return Json(new { success = true, message = "刪除成功" });
            }
            catch (Exception ex)
            {
                LogUtils.Error("Delete Error", ex);
                return Json(new { success = false, message = "刪除失敗：" + ex.Message });
            }
        }
    }
}
```

---

## 6. View 標準結構

### 6.1 Index.cshtml (列表頁)

```html
@model EECOnline.Areas.A1.Models.C101MFormModel
@{
    ViewBag.Title = "醫院資料管理";
    Layout = "~/Views/Shared/_MainLayout.cshtml";
}

<div class="content-wrapper">
    <!-- 查詢表單 -->
    <div class="box box-primary">
        <div class="box-header">
            <h3 class="box-title">查詢條件</h3>
        </div>
        <div class="box-body">
            @using (Html.BeginForm("Query", "C101M", FormMethod.Post, new { id = "queryForm" }))
            {
                <div class="row">
                    <div class="col-md-4">
                        @Html.LabelFor(m => m.HospitalCode)
                        @Html.TextBoxFor(m => m.HospitalCode, new { @class = "form-control" })
                    </div>
                    <div class="col-md-4">
                        @Html.LabelFor(m => m.HospitalName)
                        @Html.TextBoxFor(m => m.HospitalName, new { @class = "form-control" })
                    </div>
                    <div class="col-md-4">
                        <button type="button" class="btn btn-primary" onclick="doQuery()">
                            <i class="fa fa-search"></i> 查詢
                        </button>
                        <button type="button" class="btn btn-success" onclick="doNew()">
                            <i class="fa fa-plus"></i> 新增
                        </button>
                    </div>
                </div>
            }
        </div>
    </div>
    
    <!-- 資料列表 -->
    <div class="box">
        <div class="box-body">
            <table id="dataGrid" class="table table-bordered table-hover">
                <thead>
                    <tr>
                        <th>院所代碼</th>
                        <th>院所名稱</th>
                        <th>地址</th>
                        <th>操作</th>
                    </tr>
                </thead>
                <tbody id="gridBody">
                </tbody>
            </table>
            <!-- 分頁 -->
            <div id="paging"></div>
        </div>
    </div>
</div>

@section scripts {
    <script>
        function doQuery() {
            $.ajax({
                url: '@Url.Action("Query")',
                type: 'POST',
                data: $('#queryForm').serialize(),
                success: function(response) {
                    if (response.success) {
                        renderGrid(response.rows);
                        renderPaging(response.total);
                    }
                }
            });
        }
        
        function doNew() {
            window.location.href = '@Url.Action("New")';
        }
        
        function doModify(id) {
            window.location.href = '@Url.Action("Modify")' + '?id=' + id;
        }
        
        function doDelete(id) {
            if (confirm('確定要刪除嗎？')) {
                $.post('@Url.Action("Delete")', { id: id }, function(response) {
                    if (response.success) {
                        doQuery();
                    } else {
                        alert(response.message);
                    }
                });
            }
        }
        
        $(function() {
            doQuery();
        });
    </script>
}
```

### 6.2 Detail.cshtml (詳細頁)

```html
@model EECOnline.Models.Entities.TblEEC_Hospital
@{
    ViewBag.Title = ViewBag.IsNew ? "新增醫院" : "修改醫院";
    Layout = "~/Views/Shared/_MainLayout.cshtml";
}

<div class="content-wrapper">
    <div class="box box-primary">
        <div class="box-header">
            <h3 class="box-title">@ViewBag.Title</h3>
        </div>
        @using (Html.BeginForm("Save", "C101M", FormMethod.Post, new { id = "detailForm" }))
        {
            @Html.AntiForgeryToken()
            @Html.Hidden("isNew", ViewBag.IsNew)
            
            <div class="box-body">
                <div class="row">
                    <div class="col-md-6">
                        <div class="form-group">
                            @Html.LabelFor(m => m.HOSPITAL_CODE, "院所代碼")
                            @Html.TextBoxFor(m => m.HOSPITAL_CODE, new { 
                                @class = "form-control", 
                                @readonly = !ViewBag.IsNew 
                            })
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="form-group">
                            @Html.LabelFor(m => m.HOSPITAL_NAME, "院所名稱")
                            @Html.TextBoxFor(m => m.HOSPITAL_NAME, new { @class = "form-control" })
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <div class="form-group">
                            @Html.LabelFor(m => m.HOSPITAL_ADDR, "地址")
                            @Html.TextBoxFor(m => m.HOSPITAL_ADDR, new { @class = "form-control" })
                        </div>
                    </div>
                </div>
            </div>
            
            <div class="box-footer">
                <button type="button" class="btn btn-primary" onclick="doSave()">
                    <i class="fa fa-save"></i> 儲存
                </button>
                <button type="button" class="btn btn-default" onclick="doBack()">
                    <i class="fa fa-arrow-left"></i> 返回
                </button>
            </div>
        }
    </div>
</div>

@section scripts {
    <script>
        function doSave() {
            $.ajax({
                url: '@Url.Action("Save")',
                type: 'POST',
                data: $('#detailForm').serialize(),
                success: function(response) {
                    if (response.success) {
                        alert(response.message);
                        doBack();
                    } else {
                        alert(response.message);
                    }
                }
            });
        }
        
        function doBack() {
            window.location.href = '@Url.Action("Index")';
        }
    </script>
}
```

---

## 7. Model 標準結構

### 7.1 FormModel (查詢表單)

```csharp
// Areas/A1/Models/C101MFormModel.cs
public class C101MFormModel : PagingFormModel
{
    [Display(Name = "院所代碼")]
    public string HospitalCode { get; set; }
    
    [Display(Name = "院所名稱")]
    public string HospitalName { get; set; }
    
    [Display(Name = "狀態")]
    public string Status { get; set; }
}

// 分頁基礎模型
public class PagingFormModel
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

### 7.2 GridModel (列表資料)

```csharp
// Areas/A1/Models/C101MGridModel.cs
public class C101MGridModel
{
    public string HOSPITAL_CODE { get; set; }
    public string HOSPITAL_NAME { get; set; }
    public string HOSPITAL_ADDR { get; set; }
    public string STATUS { get; set; }
    public string STATUS_NAME { get; set; }
    public DateTime? CREATE_DATE { get; set; }
}
```

### 7.3 DetailModel (詳細資料)

```csharp
// Areas/A1/Models/C101MDetailModel.cs
public class C101MDetailModel
{
    [Required(ErrorMessage = "院所代碼為必填")]
    [Display(Name = "院所代碼")]
    [StringLength(10)]
    public string HOSPITAL_CODE { get; set; }
    
    [Required(ErrorMessage = "院所名稱為必填")]
    [Display(Name = "院所名稱")]
    [StringLength(100)]
    public string HOSPITAL_NAME { get; set; }
    
    [Display(Name = "地址")]
    [StringLength(200)]
    public string HOSPITAL_ADDR { get; set; }
    
    [Display(Name = "電話")]
    [Phone(ErrorMessage = "電話格式不正確")]
    public string PHONE { get; set; }
    
    [Display(Name = "電子郵件")]
    [EmailAddress(ErrorMessage = "Email格式不正確")]
    public string EMAIL { get; set; }
}
```

---

## 8. Area 間的互動

### 8.1 跨 Area 呼叫

```mermaid
sequenceDiagram
    participant A1 as A1 醫院管理
    participant SHARE as SHARE 共用元件
    participant A6 as A6 權限管理
    
    A1->>SHARE: 取得郵遞區號清單
    SHARE-->>A1: 郵遞區號資料
    
    A1->>A6: 檢查操作權限
    A6-->>A1: 權限結果
```

### 8.2 共用元件呼叫範例

```javascript
// 在 A1 的 View 中使用 SHARE 的郵遞區號選擇器
$.ajax({
    url: '/SHARE/ZIP_CO/GetCityList',
    type: 'GET',
    success: function(data) {
        $('#citySelect').html(data);
    }
});

// 選擇縣市後取得區域
$('#citySelect').change(function() {
    var cityCode = $(this).val();
    $.ajax({
        url: '/SHARE/ZIP_CO/GetDistrictList',
        type: 'GET',
        data: { cityCode: cityCode },
        success: function(data) {
            $('#districtSelect').html(data);
        }
    });
});
```

---

## 9. 命名規範

### 9.1 Controller 命名

| 類型 | 格式 | 範例 |
|------|------|------|
| 主要功能 | C{序號}MController | C101MController |
| 次要功能 | C{序號}MController | C102MController |
| 共用元件 | {功能名}Controller | ZIP_COController |

### 9.2 Action 命名

| 操作 | Action 名稱 | HTTP 方法 |
|------|------------|-----------|
| 列表頁 | Index | GET |
| 查詢 | Query | POST |
| 新增頁 | New | GET |
| 修改頁 | Modify | GET |
| 儲存 | Save | POST |
| 刪除 | Delete | POST |
| 詳細 | Detail | GET |
| 匯出 | Export | GET/POST |

### 9.3 View 命名

| 類型 | 命名 | 說明 |
|------|------|------|
| 列表頁 | Index.cshtml | 主要列表頁面 |
| 詳細頁 | Detail.cshtml | 新增/修改共用 |
| 部分視圖 | _Grid.cshtml | 列表區塊 |
| 部分視圖 | _Form.cshtml | 表單區塊 |

---

本文件說明 EECOnline 的 MVC Areas 架構，包含各 Area 的功能說明、目錄結構、Controller/View/Model 的標準模式與命名規範。
