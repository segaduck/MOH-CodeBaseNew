using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A4.Models;
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

namespace EECOnline.Areas.A4.Controllers
{
    public class C101MController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Index(new C101MFormModel());
        }

        [HttpPost]
        public ActionResult Index(C101MFormModel model)
        {
            ModelState.Clear();
            A4DAO dao = new A4DAO();
            model.Grid = dao.QueryC101M_All(model);
            return View("Index", model);
        }

        [HttpPost]
        public ActionResult Export(C101MFormModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("Index", model);
            A4DAO dao = new A4DAO();
            model.Grid = dao.QueryC101M_All(model);
            if (model.Grid.ToCount() <= 0) { sm.LastResultMessage = "查無資料"; }
            else
            {
                #region 固定欄位
                string[] DisplayArr = {
                    "病歷類別",
                    "申請數量",
                };
                string[] FieldArr = {
                    "TypeName",
                    "SubNoCount",
                };
                #endregion
                // 建立 Excel 物件
                ExcelPackage objExcel = new ExcelPackage();
                ExcelWorksheet objSheet = objExcel.Workbook.Worksheets.Add("病歷申請數量統計表");
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
                LOG.Debug("A3C104M Export1() Called. Export record: " + model.Grid.ToCount().ToString() + " rows.");
                return File(objExcel.GetAsByteArray(), HelperUtil.XLSXContentType, "病歷申請數量統計表.xlsx");
            }
            return rtn;
        }
    }
}