using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A3.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;
using OfficeOpenXml;

namespace EECOnline.Areas.A3.Controllers
{
    public class C102MController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Index(new C102MFormModel());
        }

        [HttpPost]
        public ActionResult Index(C102MFormModel model)
        {
            ModelState.Clear();
            ActionResult rtn = View("Index", model);
            A3DAO dao = new A3DAO();
            // 設定查詢分頁資訊
            dao.SetPageInfo(model.rid, model.p);
            // 查詢結果
            model.Grid = dao.QueryC102M(model);
            // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
            if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows", model);
            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model, dao, "Index");
            return rtn;
        }

        [HttpPost]
        public ActionResult Export(C102MFormModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("Index1", model);
            A3DAO dao = new A3DAO();
            model.Grid = dao.QueryC102MAll(model);
            if (model.Grid.ToCount() <= 0) { sm.LastResultMessage = "查無資料"; }
            else
            {
                #region 固定欄位
                string[] DisplayArr = {
                    "帳務月份",
                    "醫院名稱",
                    "金額",
                };
                string[] FieldArr = {
                    "AccountingYM_Text",
                    "hospital_name",
                    "price_Text",
                };
                #endregion
                // 建立 Excel 物件
                ExcelPackage objExcel = new ExcelPackage();
                ExcelWorksheet objSheet = objExcel.Workbook.Worksheets.Add("月結帳務請款表");
                // 一些暫存變數
                var iRow = 1;
                var iCol = 1;
                // 產生 標題列 (固定欄位)
                foreach (var col in DisplayArr)
                {
                    objSheet.Cells[iRow, iCol].Value = col;
                    iCol++;
                }
                // 產生 標題列 (動態欄位)
                iRow = 2;
                iCol = 1;
                foreach (var row in model.Grid)
                {
                    foreach (var col in FieldArr)
                    {
                        objSheet.Cells[iRow, iCol].Value = row.GetPiValue(col).TONotNullString();
                        iCol++;
                    }
                    iRow++;
                    iCol = 1;
                }
                // 後續處理
                objSheet.Cells.AutoFitColumns(10, 35);
                objSheet.Cells.Style.Font.Name = "新細明體";
                objSheet.Cells.Style.Font.Size = 12;
                //sm.LastResultMessage = "已匯出";
                LOG.Debug("A3C102M Export1() Called. Export record: " + model.Grid.ToCount().ToString() + " rows.");
                return File(objExcel.GetAsByteArray(), HelperUtil.XLSXContentType, "月結帳務請款表.xlsx");
            }
            return rtn;
        }
    }
}