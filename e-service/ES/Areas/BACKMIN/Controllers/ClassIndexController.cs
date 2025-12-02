using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;
using System.Web.Script.Serialization;
using System.Data.Objects.DataClasses;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClassIndexController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 2;
        }

        //
        // GET: /Admin/ClassIndex/Query
        [HttpGet]
        public ActionResult Query()
        {
            int PageType = String.IsNullOrEmpty(Request["type"]) ? 99 : Convert.ToInt32(Request["type"].ToString());
            String target = String.IsNullOrEmpty(Request["target"]) ? "" : Request["target"].ToString();
            ClassIndexModel cim = new ClassIndexModel();
            cim.PageType = PageType;
            switch (PageType)
            {
                case 0:
                    cim.PageName = "主題分類代碼";
                    cim.ShowWay = "0";
                    break;
                case 1:
                    cim.PageName = "施政分類代碼";
                    cim.ShowWay = "0";
                    break;
                case 2:
                    cim.PageName = "服務分類代碼";
                    cim.ShowWay = "0";
                    break;
                default:
                    cim.PageName = "主題分類代碼";
                    cim.ShowWay = "0";
                    cim.PageType = 0;
                    break;
            }
            cim.Target = target;
            ViewBag.List = new List<Map>();
            ViewBag.DetailList = new List<Map>();
            ViewBag.SelectValue = "";
            return View(cim);
        }

        [HttpPost]
        public ActionResult Query(ClassIndexModel cim)
        {
            ClassIndexAction cia = new ClassIndexAction();
            if (!String.IsNullOrEmpty(cim.ClassName))
                cim.ClassName = cim.ClassName.Trim();
            switch (cim.PageType)
            {
                case 0:
                    //ViewBag.List = cia.GetCLS_SUB_CD(cim);
                    ViewBag.List = cia.GetCLS_CD(cim,"CLS_SUB_CD");
                    break;
                case 1:
                    //ViewBag.List = cia.GetCLS_ADM_CD(cim);
                    ViewBag.List = cia.GetCLS_CD(cim, "CLS_ADM_CD");
                    break;
                case 2:
                    //ViewBag.List = cia.GetCLS_SRV_CD(cim);
                    ViewBag.List = cia.GetCLS_CD(cim, "CLS_SRV_CD");
                    break;
            }
            ViewBag.DetailList = cia.GetClassDetailModel();
            ViewBag.SelectValue = String.IsNullOrEmpty(cim.ClassName) ? "" : cim.ClassName;
            return View(cim);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            ClassIndexAction cia = new ClassIndexAction();
            ViewBag.List = cia.GetCATE_SEARCH();

            this.SetVisitRecord("ClassIndex", "Edit", "分類檢索維護");

            return View();
        }

        [HttpPost]
        public ActionResult EditPage()
        {
            int Pkey = Convert.ToInt32(Request.Form["Pkey"].ToString());
            ClassEditModel model = new ClassEditModel();
            ClassIndexAction cia = new ClassIndexAction();
            //CATE_SEARCH cs = cia.SelectCATE_SEARCH(Pkey);
            model = cia.SelectCATE_SEARCH(Pkey);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditPageSave(ClassEditModel model)
        {
            ClassIndexAction cia = new ClassIndexAction();

            model.UPD_ACC = GetAccount();

            if (cia.UpdateCATE_SEARCH(model))
            {
                TempData["tempMessage"] = "修改完成!";
            }else
            {
                TempData["tempMessage"] = "修改失敗!";
            }
            ViewBag.List = cia.GetCATE_SEARCH();

            return RedirectToAction("Edit", "ClassIndex");
        }

        public ActionResult TreeView()
        {
            ClassPageModel cpm = null;
            if (TempData["Model"] != null)
            {
                cpm = TempData["Model"] as ClassPageModel;
            }
            else
            {
                cpm = new ClassPageModel();
                cpm.Classid = "";
                cpm.Classname = "";
            }
                
           
            ClassIndexAction cia = new ClassIndexAction();
            var jsonSerialiser = new JavaScriptSerializer();

            ViewBag.Json = jsonSerialiser.Serialize(cia.GetAllClass());
            ViewBag.tempMessage = TempData["tempMessage"];

            this.SetVisitRecord("ClassIndex", "TreeView", "分類檢索管理");

            return View(cpm);
        }

        public ActionResult CheckIsExist(ClassPageModel cpm)
        {
            //String TargetTable, Classid = "";
            //TargetTable = String.IsNullOrEmpty(Request.Form["TargetTable"]) ? "" : Request.Form["TargetTable"].ToString();
            //Classid = String.IsNullOrEmpty(Request.Form["Classid"]) ? "" : Request.Form["Classid"].ToString();
            Boolean isExist = false;
            if (cpm.ActionModel.Equals("Add"))
            {
                String tablename = cpm.TargetTable.Split('#')[0];

                ClassIndexAction cia = new ClassIndexAction();

                isExist = cia.IsClassIdExist(tablename, cpm.Classid);
            }
            return Json( !isExist , JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TreeView_EDIT(ClassPageModel cpm)
        {
            //String ActionModel = String.IsNullOrEmpty(Request.Form["ActModel"]) ? "" : Request.Form["ActModel"].ToString();
            //String TargetTable,Parentid,Classid,Classname,Clevel,BeforeClassid ="";
            //TargetTable = String.IsNullOrEmpty(Request.Form["TargetTable"]) ? "" : Request.Form["TargetTable"].ToString();
            //Parentid = String.IsNullOrEmpty(Request.Form["Parentid"]) ? "" : Request.Form["Parentid"].ToString();
            //Classid = String.IsNullOrEmpty(Request.Form["Classid"]) ? "" : Request.Form["Classid"].ToString();
            //Classname = String.IsNullOrEmpty(Request.Form["Classname"]) ? "" : Request.Form["Classname"].ToString();
            //Clevel = String.IsNullOrEmpty(Request.Form["Clevel"]) ? "" : Request.Form["Clevel"].ToString();
            //BeforeClassid = String.IsNullOrEmpty(Request.Form["BeforeClassid"]) ? "" : Request.Form["BeforeClassid"].ToString();

            //String tablename = TargetTable.Split('_')[0];
            String tablename = cpm.TargetTable.Split('#')[0];
            ClassIndexAction cia = new ClassIndexAction();
            switch (cpm.ActionModel)
            {
                case "Add":
                    if (cia.AddDateCLS(tablename, cpm, this.GetAccount()))
                    {
                        TempData["tempMessage"] = "新增完成!";
                    }
                    else
                    {
                        TempData["tempMessage"] = "新增失敗!";
                    }
                    break;
                case "Update":
                    if (cia.UpdateDateCLS(tablename, cpm , this.GetAccount()))
                    {
                        TempData["tempMessage"] = "修改完成!";
                    }
                    else
                    {
                        TempData["tempMessage"] = "修改失敗!";
                    }
                    
                    break;
                case "Delete":
                    if (cia.DeleteDateCLS(tablename, cpm, this.GetAccount()))
                    {
                        TempData["tempMessage"] = "刪除完成!";
                    }
                    else
                    {
                        TempData["tempMessage"] = "刪除失敗!";
                    }
                    break;
            }
            TempData["Model"] = cpm;
            return RedirectToAction("TreeView", "ClassIndex");
        }

        
    }
}
