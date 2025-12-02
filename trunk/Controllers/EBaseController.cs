using Turbo.DataLayer;
using EECOnline.DataLayers;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Turbo.Commons;
using EECOnline.Commons.Filter;
using Omu.ValueInjecter;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;

namespace EECOnline.Controllers
{

    /// <summary>
    /// 無權限控管的共用 Controller 基底類, 前台無需 登入/授權 使用的功能, 一律繼承這個基底類 
    /// </summary>
    /// <remarks>
    /// 請注意！若有修改到 SetPagingParams()、ComputePagingParams() 兩個方法程式碼時，
    /// 請務必同步修改在 BaseGuestController.cs 內的 SetPagingParams()、ComputePagingParams() 兩個方法程式碼。
    /// 否則會導致系統分頁處理不一致問題。
    /// </remarks>
    public class EBaseController : Controller
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string CACHE_FORM_ACTION = "Index";

        /// <summary>
        /// 預設系統靜態頁
        /// </summary>
        public static string Web_LOCK_PAGE = "~/WebLock/Maintenance";

        /// <summary>
        /// 設定分頁元件所用到的參數:
        /// pagingModel: 查詢用的 FormModel, 
        /// dao: 執行查詢的DAO, 
        /// action: 分頁連結的 action 名稱
        /// </summary>
        /// <param name="pagingModel"></param>
        /// <param name="dao"></param>
        /// <param name="action"></param>
        protected void SetPagingParams(PagingResultsViewModel pagingModel, BaseDAO dao, string action)
        {
            SetPagingParams(pagingModel, dao, action, pagingModel.ajaxLoadPageCallback);
        }

        /// <summary>
        /// 依據每頁筆數重新計笡總頁數，與適當調整當前頁次值
        /// </summary>
        /// <param name="info">分頁資訊</param>
        private void ComputePagingParams(PaginationInfo info)
        {
            if (info.PageSize == 0)
            {
                info.TotalPages = 1;
            }
            else
            {
                double v = (info.Total + (info.PageSize - 1)) / info.PageSize;
                info.TotalPages = (info.PageSize <= 0) ? 1 : Convert.ToInt32(Math.Floor(v));
            }

            if (info.PageIdx > info.TotalPages) info.PageIdx = info.TotalPages;
        }

        /// <summary>
        /// 設定分頁元件所用到的參數(指定客制化的分頁資料顯示 callback function):
        /// pagingModel: 查詢用的 FormModel, 
        /// dao: 執行查詢的DAO, 
        /// action: 分頁連結的 action 名稱, 
        /// callback: 分頁資料AJAX載入後會呼叫的 js callback function
        /// </summary>
        /// <param name="pagingModel">查詢用的 FormModel</param>
        /// <param name="dao">執行查詢的DAO</param>
        /// <param name="action">分頁連結的 action</param>
        /// <param name="callback">分頁資料AJAX載入後會呼叫的 js callback function</param>
        protected void SetPagingParams(PagingResultsViewModel pagingModel, BaseDAO dao, string action, string callback)
        {
            //20180608 設定使用者指定的每頁筆數
            // dao.PaginationInfo.PageSize = pagingModel.PagingInfo.PageSize;
            // 2018.09.05, PagingInfo 要改用 Clone() 方式取得
            // 在多個分頁查詢共用同一個 dao 時才會正確 
            pagingModel.PagingInfo = dao.PaginationInfo.Clone();

            //20180608 依據每頁筆數重新計笡總頁數，與適當調整當前頁次值
            ComputePagingParams(pagingModel.PagingInfo);

            pagingModel.statementId = dao.StatementID;
            pagingModel.rid = dao.ResultID;
            pagingModel.action = Url.Action(action);
            pagingModel.ajaxLoadPageCallback = callback;
        }

        /// <summary>設定分頁元件所用到的參數</summary>
        /// <param name="pagingModel">分頁結果 ViewModel</param>
        /// <param name="rows">已取得的查詢結果資料集合</param>
        /// <param name="action">View 名稱</param>
        protected void SetPagingParams<TRowModel>(PagingResultsViewModel pagingModel, IList<TRowModel> rows, string action)
        {
            if (pagingModel == null) throw new ArgumentNullException("pagingModel");
            else
            {
                //建立一個虛假 DAO 物件，用來表示 rows 是從這個 DAO 執行得到的
                BaseDAO dao = new BaseDAO();
                dao.SetPageInfo(pagingModel.rid, pagingModel.p);
                if (string.IsNullOrEmpty(dao.ResultID)) dao.ResultID = System.Guid.NewGuid().ToString();

                var daoPaging = dao.PaginationInfo;
                daoPaging.Total = (rows == null) ? 0 : rows.Count;
                daoPaging.ResultExists = (daoPaging.Total > 0);
                daoPaging.PageSize = 20;

                //設定必要的分頁參數
                var viewPaging = pagingModel.PagingInfo;
                viewPaging.Total = daoPaging.Total;
                viewPaging.ResultExists = daoPaging.ResultExists;
                viewPaging.PageSize = daoPaging.PageSize;

                SetPagingParams(pagingModel, dao, action, pagingModel.ajaxLoadPageCallback);
            }
        }


        /// <summary>
        /// 透過分頁勾選狀態資訊, 導出已選取的 Grid 內的已勾選項目
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="form"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        protected IList<T> GetSelectedList<T>(PagingResultsViewModel form, IList<T> grid)
        {
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }

            IList<T> selectedList = new List<T>();

            if (form.SelectCheckedIdx == null)
            {
                form.SelectCheckedIdx = new string[0];
            }
            if (form.SelectExcludeIdx == null)
            {
                form.SelectExcludeIdx = new string[0];
            }

            if (form.SelectAllChecked == "Y")
            {
                // 全選
                for (int i = 0; i < grid.Count(); i++)
                {
                    // 判斷排除項目
                    bool exclude = false;
                    foreach (var idx in form.SelectExcludeIdx)
                    {
                        if (Convert.ToString(i) == idx)
                        {
                            exclude = true;
                            break;
                        }
                    }
                    if (!exclude)
                    {
                        selectedList.Add(grid[i]);
                    }
                }
            }
            else
            {
                // 從 Form.SendCheckedIdx 取出已勾選項目索引
                foreach (var item in form.SelectCheckedIdx)
                {
                    int idx = -1;
                    if (!int.TryParse(item, out idx))
                    {
                        throw new Exception("分頁勾選索引值 [" + item + "] 不是整數值！");
                    }
                    if (idx >= 0 && idx < grid.Count())
                    {
                        // 判斷排除項目
                        bool exclude = false;
                        foreach (var ic in form.SelectExcludeIdx)
                        {
                            if (Convert.ToString(idx) == ic)
                            {
                                exclude = true;
                                break;
                            }
                        }
                        if (!exclude)
                        {
                            selectedList.Add(grid[idx]);
                        }
                    }
                    else
                    {
                        throw new Exception("分頁勾選勾選索引值 [" + idx + "] 超出資料範圍！");
                    }
                }
            }

            LOG.Info(
                string.Format("GetSelectedList: SelectAllChecked={0}, Total={1}, SelectedCount={2}, \n\tSelectCheckedIdx: [{3}], \n\tSelectExcludeIdx: [{4}]",
                    form.SelectAllChecked,
                    grid.Count(),
                    selectedList.Count(),
                    string.Join(",", form.SelectCheckedIdx),
                    string.Join(",", form.SelectExcludeIdx)
                ));
            return selectedList;
        }


        /// <summary>
        /// 每個 action 被執行前會觸發這個 event
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string action = ControllerContextHelper.GetAction(filterContext);

            // 將任何的 Model Binding Exception 加到一般的 ModelError 中
            // 以便讓這些 Exception 有機會顯示出來
            var modelState = filterContext.Controller.ViewData.ModelState;
            if (!modelState.IsValid)
            {
                var modelStateErrors = modelState.Values.Where(E => E.Errors.Count > 0)
                    .SelectMany(E => E.Errors);

                List<string> exceptions = new List<string>();
                foreach (var item in modelStateErrors)
                {
                    if (item.Exception != null)
                    {
                        LOG.Error(action + ": ModelError: " + item.Exception.Message);
                        exceptions.Add(item.Exception.Message);
                    }
                }
                if (exceptions.Count > 0)
                {
                    modelState.AddModelError("Exception", string.Join("<br/>\n", exceptions.ToArray()));
                }
            }

            // 系統切換靜態頁面
            var IsLock = SHAREDAO.GetSetup("WebLock") == "1" ? true : false;
            if (IsLock)
            {
                // 根據 WebLock.Roles 決定, 顯示頁面
                string Page = Web_LOCK_PAGE;
                LOG.Info("OnWebLock: redirect to page: " + Page);
                filterContext.Result = new RedirectResult(Page);
            }
            else
            {
                // FormModelCacheFilter OnActionExecuting 
                if (CACHE_FORM_ACTION.Equals(action))
                {
                    FormModelCacheFilter cacheFilter = new FormModelCacheFilter();
                    cacheFilter.OnActionExecuting(filterContext);
                    if (cacheFilter.CacheFormModelInvalid)
                    {
                        // Cached Form Model 無效, 導向 GET Index()
                        LOG.Info("CacheFormModel Invalid, redirect with GET: " + action);
                        filterContext.Result = RedirectToAction(action);
                    }
                }
            }
          
            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// 每個 action 被執行後會觸發這個 event
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string action = ControllerContextHelper.GetAction(filterContext);
            string ActionPath = ControllerContextHelper.GetActionPath(filterContext);
            LoginDAO dao = new LoginDAO();
            var use_type = "0";
            var ip = HttpContext.Request.UserHostAddress;

            // 新增最近使用
            if (filterContext.Exception != null)
            {
                if (filterContext.Exception.Message.ToUpper().Contains("INSERT")) { use_type = "1"; }
                if (filterContext.Exception.Message.ToUpper().Contains("UPDATE")) { use_type = "2"; }
                if (filterContext.Exception.Message.ToUpper().Contains("DELETE")) { use_type = "3"; }

                TblVISIT_RECORD vr = new TblVISIT_RECORD();
                vr.control_name = action;
                vr.action_name = action;
                vr.funcm_name = ActionPath;
                vr.funcm_link = ActionPath;
                vr.modtime = DateTime.Now;
                vr.userno = ip;
                vr.use_type = use_type;
                vr.status = "0";
                vr.urlwhere = "0";
                vr.note = filterContext.Exception.Message;

                dao.Insert(vr);
            }
            else
            {
                //if (filterContext.Exception.Message.Contains("找不到路徑"))
                //{
                //    TblVISIT_RECORD vr = new TblVISIT_RECORD();
                //    vr.modtime = DateTime.Now;
                //    vr.use_type = "-1";
                //    vr.status = "0";
                //    vr.note = filterContext.Exception.Message;
                //    vr.userno = ip;

                //    dao.Insert(vr);
                //}
                //else if (filterContext.Exception.Message.Contains("找不到公用動作"))
                //{
                //    TblVISIT_RECORD vr = new TblVISIT_RECORD();
                //    vr.modtime = DateTime.Now;
                //    vr.use_type = "-1";
                //    vr.status = "0";
                //    vr.note = filterContext.Exception.Message;
                //    vr.userno = ip;

                //    dao.Insert(vr);
                //}
                //else
                //{
                    TblVISIT_RECORD vr = new TblVISIT_RECORD();
                    vr.control_name = action;
                    vr.action_name = action;
                    vr.funcm_name = ActionPath;
                    vr.funcm_link = ActionPath;
                    vr.modtime = DateTime.Now;
                    vr.userno = ip;
                    vr.use_type = use_type;
                    vr.urlwhere = "0";
                    vr.status = "1";

                    dao.Insert(vr);
                //}
            }

            if (CACHE_FORM_ACTION.Equals(action))
            {
                (new FormModelCacheFilter()).OnActionExecuted(filterContext);
            }
            else
            {
                base.OnActionExecuted(filterContext);
            }
        }


        /// <summary>
        /// 載入當前 Controller 主查詢(Index) Cached Form Model,
        /// 可用來在 Controller 中直接呼叫 Index(form) 以返回查詢結果,
        /// 並包含其查詢條件及分頁結果狀態.
        /// <para>若找到 cached form 則回傳 true, 若找不到則回傳 false</para>
        /// </summary>
        /// <returns></returns>
        protected bool LoadCachedFormModel(PagingResultsViewModel form)
        {
            PagingResultsViewModel savedForm = null;
            string actionPath = ControllerContextHelper.GetActionPath(this.ControllerContext);
            string action = ControllerContextHelper.GetAction(this.ControllerContext);

            if (!string.IsNullOrEmpty(action))
            {
                // 去掉 context 取得 actionPath 中的 "action name"
                // 並置換為 CACHE_FORM_ACTION (Index)
                int p = actionPath.LastIndexOf(action);
                if (p > -1)
                {
                    actionPath = actionPath.Substring(0, p);
                    actionPath += CACHE_FORM_ACTION;
                }

                savedForm = (PagingResultsViewModel)ControllerContext.HttpContext.Session[actionPath];
                if (savedForm != null)
                {
                    LOG.Info("LoadCachedFormModel: load formModel for '" + actionPath + "', rid=" + form.rid);
                    if (form.useCache == 2)
                    {
                        savedForm.rid = "";
                    }
                    savedForm.useCache = form.useCache;
                    form.InjectFrom(savedForm);
                }
            }

            return (savedForm != null);
        }

        /// <summary>依據「網頁對話視窗類型」傳回對話視窗頁面的 HTTP 回應內容（即傳回 PartialView、View 兩者其中一個）</summary>
        /// <param name="dialog">網頁對話視窗類型。 ("": popupDialog，"W": popupWindow)</param>
        /// <param name="viewName">View 名稱</param>
        /// <param name="model">資料 Model</param>
        /// <returns></returns>
        protected virtual ActionResult BuildDialogViewResult(string dialog, string viewName, object model = null)
        {
            if (string.IsNullOrEmpty(dialog)) return PartialView(viewName, model);
            else
            {
                var masterName = "~/Views/Shared/_MainLayout_NoHeader_NoBg.cshtml";
                return View(viewName, masterName, model);
            }
        }
    }
}