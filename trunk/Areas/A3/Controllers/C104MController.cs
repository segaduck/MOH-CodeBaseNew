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
    public class C104MController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string Idx = "1")
        {
            return (Idx == "2") ?
                Index2(new C104MForm2Model()) :
                Index1(new C104MForm1Model());
        }

        [HttpPost]
        public ActionResult Index1(C104MForm1Model model)
        {
            ModelState.Clear();
            ActionResult rtn = View("Index1", model);
            A3DAO dao = new A3DAO();
            // 設定查詢分頁資訊
            dao.SetPageInfo(model.rid, model.p);
            // 查詢結果
            model.Form1Grid = dao.QueryC104M_1(model);
            // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
            if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows1", model);
            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model, dao, "Index1");
            return rtn;
        }

        [HttpPost]
        public ActionResult Index2(C104MForm2Model model)
        {
            ModelState.Clear();
            ActionResult rtn = View("Index2", model);
            A3DAO dao = new A3DAO();
            // 設定查詢分頁資訊
            dao.SetPageInfo(model.rid, model.p);
            // 查詢結果
            model.Form2Grid = dao.QueryC104M_2(model);
            // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
            if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows2", model);
            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model, dao, "Index2");
            return rtn;
        }

        [HttpPost]
        public ActionResult Export1(C104MForm1Model model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("Index1", model);
            A3DAO dao = new A3DAO();
            model.Form1Grid = dao.QueryC104M_1All(model);
            if (model.Form1Grid.ToCount() <= 0) { sm.LastResultMessage = "查無資料"; }
            else
            {
                #region 固定欄位
                string[] DisplayArr = {
                    "醫院別",
                    "申請人",
                    "申請類型",
                    "系統受理時間",
                    "是否完成繳費",
                    "繳費金額",
                    "是否需補上傳",
                };
                string[] FieldArr = {
                    "hospital_name",
                    "user_name",
                    "his_types_Text",
                    "createdatetime_Text",
                    "payed_Text",
                    "price_Text",
                    "NeedUploadNum_Text",
                };
                #endregion
                // 建立 Excel 物件
                ExcelPackage objExcel = new ExcelPackage();
                ExcelWorksheet objSheet = objExcel.Workbook.Worksheets.Add("申請帳務明細列表");
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
                foreach (var row in model.Form1Grid)
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
                LOG.Debug("A3C104M Export1() Called. Export record: " + model.Form1Grid.ToCount().ToString() + " rows.");
                return File(objExcel.GetAsByteArray(), HelperUtil.XLSXContentType, "申請帳務明細列表.xlsx");
            }
            return rtn;
        }

        [HttpPost]
        public ActionResult Export2(C104MForm2Model model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("Index2", model);
            A3DAO dao = new A3DAO();
            model.Form2Grid = dao.QueryC104M_2All(model);
            if (model.Form2Grid.ToCount() <= 0) { sm.LastResultMessage = "查無資料"; }
            else
            {
                #region 固定欄位
                string[] DisplayArr = {
                    "醫院別",
                    //"刷卡手續費",
                    //"平台手續費",
                    "病歷產製費",
                    "小計",
                    "申請案件總量",
                };
                string[] FieldArr = {
                    "hospital_name",
                    //"Other1_Text",
                    //"Other2_Text",
                    "HisType_Text",
                    "SubSum_Text",
                    "ApplyNum",
                };
                var commonTypes = new FrontDAO().Get_EEC_CommonTypeAll();
                var addArr1 = new List<string>();
                var addArr2 = new List<string>();
                var Idx = 0;
                foreach (var row in commonTypes)
                {
                    if (row["type_price"].TONotNullString() == "0") continue;
                    addArr1.Add(row["type_name"].ToString());
                    addArr2.Add("EEC_CommonType_" + Idx.ToString());
                    Idx++;
                }
                var newArr1 = DisplayArr.ToList();
                var newArr2 = FieldArr.ToList();
                newArr1.InsertRange(1, addArr1);
                newArr2.InsertRange(1, addArr2);
                DisplayArr = newArr1.ToArray();
                FieldArr = newArr2.ToArray();
                #endregion
                // 建立 Excel 物件
                ExcelPackage objExcel = new ExcelPackage();
                ExcelWorksheet objSheet = objExcel.Workbook.Worksheets.Add("帳務細項列表");
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
                foreach (var row in model.Form2Grid)
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
                LOG.Debug("A3C104M Export2() Called. Export record: " + model.Form2Grid.ToCount().ToString() + " rows.");
                return File(objExcel.GetAsByteArray(), HelperUtil.XLSXContentType, "帳務細項列表.xlsx");
            }
            return rtn;
        }
    }
}