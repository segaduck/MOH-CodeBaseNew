using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Omu.ValueInjecter;
using ES.Models;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_001038Controller : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="srvid"></param>
        /// <returns></returns>
        public ActionResult Index(string appid, string srvid)
        {
            var APP_ID = appid;
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001038FormModel model = new Apply_001038FormModel(APP_ID);
            ActionResult rtn = View(model);

            // 案件基本資訊
            #region 案件內容
            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);
            model.SRV_ID = "001038";
            model.APP_TIME = alydata.APP_TIME;
            model.APP_EXT_DATE = alydata.APP_EXT_DATE;
            model.PRO_ACC = alydata.PRO_ACC;
            model.FLOW_CD = alydata.FLOW_CD;
            model.IDN = alydata.IDN;
            #endregion

            #region 基本資料
            // 基本資料
            Apply_001038Model app = new Apply_001038Model();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);
            model.IM_EXPORT = appdata.IM_EXPORT;
            // 委託人資料
            model.TAX_ORG_ID = appdata.TAX_ORG_ID;
            model.TAX_ORG_NAME = appdata.TAX_ORG_NAME;
            model.TAX_ORG_ENAME = appdata.TAX_ORG_ENAME;
            model.TAX_ORG_TID = appdata.TAX_ORG_TID;
            //model.TAX_ORG_ZIP = appdata.TAX_ORG_ZIP; 沒有TAX_ORG_ZIP這個欄位
            model.TAX_ORG_ADDR = appdata.TAX_ORG_ADDR;
            model.TAX_ORG_EADDR = appdata.TAX_ORG_EADDR;
            model.TAX_ORG_MAN = appdata.TAX_ORG_MAN;
            model.TAX_ORG_TEL = appdata.TAX_ORG_TEL;
            model.TAX_ORG_EMAIL = appdata.TAX_ORG_EMAIL;
            model.TAX_ORG_FAX = appdata.TAX_ORG_FAX;
            model.DATE_S = appdata.DATE_S;
            model.DATE_E = appdata.DATE_E;
            model.TRN_COUNTRY_ID = appdata.TRN_COUNTRY_ID;
            model.TRN_PORT_ID = appdata.TRN_PORT_ID;
            model.BEG_COUNTRY_ID = appdata.BEG_COUNTRY_ID;
            model.BEG_PORT_ID = appdata.BEG_PORT_ID;
            #endregion

            #region 其他資料
            // 其他資料
            model.APP_USE_ID = appdata.APP_USE_ID;
            model.CONF_TYPE_ID = appdata.CONF_TYPE_ID;
            for (var k = 1; k <= 6; k++)
            {
                var kNo = k.TONotNullString().PadLeft(2, '0');
                // 檢附文件類型
                var MOD_DOC_TYP = model.GetType().GetProperties().Where(m => m.Name == "DOC_TYP_" + kNo);
                var APP_DOC_TYP = appdata.GetType().GetProperties().Where(m => m.Name == "DOC_TYP_" + kNo);
                if (MOD_DOC_TYP.ToCount() > 0 && APP_DOC_TYP.ToCount() > 0)
                {
                    var ModObj = MOD_DOC_TYP.FirstOrDefault();
                    var AppObj = APP_DOC_TYP.FirstOrDefault();
                    var AppVal = AppObj.GetValue(appdata);
                    ModObj.SetValue(model, AppVal);
                }
                // 檢附文件字號
                var MOD_DOC_COD = model.GetType().GetProperties().Where(m => m.Name == "DOC_COD_" + kNo);
                var APP_DOC_COD = appdata.GetType().GetProperties().Where(m => m.Name == "DOC_COD_" + kNo);
                if (MOD_DOC_COD.ToCount() > 0 && APP_DOC_COD.ToCount() > 0)
                {
                    var ModObj = MOD_DOC_COD.FirstOrDefault();
                    var AppObj = APP_DOC_COD.FirstOrDefault();
                    var AppVal = AppObj.GetValue(appdata);
                    ModObj.SetValue(model, AppVal);
                }
                // 檢附文件說明
                var MOD_DOC_TXT = model.GetType().GetProperties().Where(m => m.Name == "DOC_TXT_" + kNo);
                var APP_DOC_TXT = appdata.GetType().GetProperties().Where(m => m.Name == "DOC_TXT_" + kNo);
                if (MOD_DOC_TXT.ToCount() > 0 && APP_DOC_TXT.ToCount() > 0)
                {
                    var ModObj = MOD_DOC_TXT.FirstOrDefault();
                    var AppObj = APP_DOC_TXT.FirstOrDefault();
                    var AppVal = AppObj.GetValue(appdata);
                    ModObj.SetValue(model, AppVal);
                }
            }

            // 檔案
            for (var k = 1; k <= 6; k++)
            {
                Apply_FileModel fileWhere = new Apply_FileModel();
                fileWhere.APP_ID = APP_ID;
                fileWhere.FILE_NO = k;
                var filedata = dao.GetRow(fileWhere);


                var kNo = k.TONotNullString().PadLeft(2, '0');
                var fileNo = model.GetType().GetProperties().Where(m => m.Name == "DOC_FILE_" + kNo + "_FILENAME");
                if (fileNo.ToCount() > 0)
                {
                    var fileObj = fileNo.FirstOrDefault();
                    fileObj.SetValue(model, filedata.FILENAME);
                }
            }
            #endregion

            #region 輸入
            // 輸入
            Apply_001038_DEST dest = new Apply_001038_DEST();
            dest.APP_ID = APP_ID;
            var destdata = dao.GetRow(dest);
            model.Dest_ORG_UNITNAME = destdata.ORG_UNITNAME;
            model.Dest_DEST_STATE_ID = appdata.DEST_STATE_ID;
            model.Dest_ORG_NAME = destdata.ORG_NAME;
            model.Dest_ORG_TEL = destdata.ORG_TEL;
            model.Dest_ORG_EMAIL = destdata.ORG_EMAIL;
            model.Dest_TAI_UNITNAME = destdata.TAI_UNITNAME;
            model.Dest_LIC_NUM = destdata.LIC_NUM;
            model.Dest_TAI_NAME = destdata.TAI_NAME;
            model.Dest_TAI_TEL = destdata.TAI_TEL;
            model.Dest_TAI_EMAIL = destdata.TAI_EMAIL;
            model.Dest_TAI_ADDR = destdata.TAI_ADDR;
            model.Dest_USE_MARK = appdata.USE_MARK;
            model.Dest_A_NAME = destdata.A_NAME;
            model.Dest_A_NUM1 = destdata.A_NUM1;
            model.Dest_A_DATE = destdata.A_DATE;
            model.Dest_B_NAME = destdata.B_NAME;
            model.Dest_B_NUM1 = destdata.B_NUM1;
            model.Dest_B_NUM2 = destdata.B_NUM2;
            model.Dest_B_DATE = destdata.B_DATE;
            model.Dest_C_NAME1 = destdata.C_NAME1;
            model.Dest_C_NAME2 = destdata.C_NAME2;
            model.Dest_C_NUM1 = destdata.C_NUM1;
            model.Dest_C_DATE = destdata.C_DATE;
            model.Dest_C_DAY = destdata.C_DAY;
            #endregion

            #region 輸出
            // 輸出
            Apply_001038_SELL sell = new Apply_001038_SELL();
            sell.APP_ID = APP_ID;
            var selldata = dao.GetRow(sell);
            model.Sell_ORG_UNITNAME = selldata.ORG_UNITNAME;
            model.Sell_ORG_NAME = selldata.ORG_NAME;
            model.Sell_ORG_LIC_NUM = selldata.ORG_LIC_NUM;
            model.Sell_ORG_TEL = selldata.ORG_TEL;
            model.Sell_ORG_EMAIL = selldata.ORG_EMAIL;
            model.Sell_SELL_STATE_ID = appdata.SELL_STATE_ID;
            model.Sell_OTH_UNITNAME = selldata.OTH_UNITNAME;
            model.Sell_OTH_TEL = selldata.OTH_TEL;
            model.Sell_OTH_EMAIL = selldata.OTH_EMAIL;
            model.Sell_OTH_ZIP = selldata.OTH_ZIP;
            model.Sell_OTH_ADDR = selldata.OTH_ADDR;
            model.Sell_USE_MARK = appdata.USE_MARK;
            model.Sell_A_NAME = selldata.A_NAME;
            model.Sell_A_NUM1 = selldata.A_NUM1;
            model.Sell_A_DATE = selldata.A_DATE;
            model.Sell_B_NAME = selldata.B_NAME;
            model.Sell_B_NUM1 = selldata.B_NUM1;
            model.Sell_B_NUM2 = selldata.B_NUM2;
            model.Sell_B_DATE = selldata.B_DATE;
            model.Sell_C_NAME1 = selldata.C_NAME1;
            model.Sell_C_NAME2 = selldata.C_NAME2;
            model.Sell_C_NUM1 = selldata.C_NUM1;
            model.Sell_C_DATE = selldata.C_DATE;
            model.Sell_C_DAY = selldata.C_DAY;
            #endregion
            model.IsNew = false;
            model.GOODS.IsReadOnly = true;
            rtn = View("Index", model);
            return rtn;
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001038FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            var ErrorMsg = "";

            // 存檔
            ErrorMsg = dao.AppendApply001038(model);
            if (ErrorMsg == "")
            {
                result.status = true;
                result.message = "存檔成功 !";
            }
            else { result.message = ErrorMsg; }


            return Content(result.Serialize(), "application/json");
        }
    }
}
