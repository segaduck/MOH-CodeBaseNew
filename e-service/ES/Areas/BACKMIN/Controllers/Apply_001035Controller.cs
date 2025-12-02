using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using System.Linq;
using System.Web.Mvc;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_001035Controller : BaseController
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
            Apply_001035FormModel model = new Apply_001035FormModel(APP_ID);
            ActionResult rtn = View(model);

            #region 案件內容
            // 案件基本資訊
            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);
            model.SRV_ID = "001035";
            model.APP_TIME = alydata.APP_TIME;
            model.NAME = alydata.NAME;
            model.IDN = alydata.IDN;
            model.APP_EXT_DATE = alydata.APP_EXT_DATE;
            model.PRO_ACC = alydata.PRO_ACC;
            model.FLOW_CD = alydata.FLOW_CD;
            #endregion

            #region 基本資料
            // 基本資料
            Apply_001035Model app = new Apply_001035Model();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            // 納稅義務人資料
            model.TAX_ORG_ID = appdata.TAX_ORG_ID;
            model.TAX_ORG_NAME = appdata.TAX_ORG_NAME;
            model.TAX_ORG_ENAME = appdata.TAX_ORG_ENAME;
            //model.TAX_ORG_ZIP = appdata.TAX_ORG_ZIP; 沒有TAX_ORG_ZIP這個欄位
            model.TAX_ORG_ADDR = appdata.TAX_ORG_ADDR;
            model.TAX_ORG_EADDR = appdata.TAX_ORG_EADDR;
            model.TAX_ORG_MAN = appdata.TAX_ORG_MAN;
            model.TAX_ORG_TEL = appdata.TAX_ORG_TEL;
            model.TAX_ORG_EMAIL = appdata.TAX_ORG_EMAIL;
            model.TAX_ORG_FAX = appdata.TAX_ORG_FAX;
            model.IM_EXPORT = appdata.IM_EXPORT;
            model.DATE_S = appdata.DATE_S;
            model.DATE_E = appdata.DATE_E;
            if (model.IM_EXPORT == "0")
            {
                model.Dest_DEST_STATE_ID = appdata.DEST_STATE_ID;
                model.Dest_SELL_STATE_ID = appdata.SELL_STATE_ID;
                model.Dest_BEG_COUNTRY_ID = appdata.BEG_COUNTRY_ID;
                model.Dest_BEG_PORT_ID = appdata.BEG_PORT_ID;
                model.Dest_SELL_NAME = appdata.SELL_NAME;
                model.Dest_SELL_ADDR = appdata.SELL_ADDR;
            }
            if (model.IM_EXPORT == "1")
            {
                model.Sell_DEST_STATE_ID = appdata.DEST_STATE_ID;
                model.Sell_SELL_STATE_ID = appdata.SELL_STATE_ID;
                model.Sell_TRN_COUNTRY_ID = appdata.TRN_COUNTRY_ID;
                model.Sell_TRN_PORT_ID = appdata.TRN_PORT_ID;
                model.Sell_BEG_COUNTRY_ID = appdata.BEG_COUNTRY_ID;
                model.Sell_BEG_PORT_ID = appdata.BEG_PORT_ID;
                model.Sell_SELL_NAME = appdata.SELL_NAME;
                model.Sell_SELL_ADDR = appdata.SELL_ADDR;
            }
            #endregion

            #region 其他資料
            // 其他資料
            model.APP_USE_ID = appdata.APP_USE_ID;
            model.CONF_TYPE_ID = appdata.CONF_TYPE_ID;
            for (var k = 1; k <= 23; k++)
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
            for (var k = 1; k <= 23; k++)
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

            model.IsNew = false;
            model.GOODS.IsReadOnly = true;
            return View("Index", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001035FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            var ErrorMsg = "";

            // 存檔
            ErrorMsg = dao.AppendApply001035(model);
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
