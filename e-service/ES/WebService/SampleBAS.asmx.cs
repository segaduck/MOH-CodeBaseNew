using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ES.WebService
{
    /// <summary>
    ///SampleBAS 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    // [System.Web.Script.Services.ScriptService]
    public class SampleBAS : System.Web.Services.WebService
    {

        [WebMethod]
        public List<SAMPLE_BASModel.Res> GetData(SAMPLE_BASModel.Req model)
        {
            ApplyDAO dao = new ApplyDAO();
            var RsModel = new List<SAMPLE_BASModel.Res>();
            var userIP = HttpContext.Current.Request.UserHostAddress;
            //判斷client端是否有設定代理伺服器
            if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] == null)
            {
                userIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
            }
            else
            {
                userIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            var whiteIP = DataUtils.GetConfig("SampleBasWhiteIPs");
            // 白名單設定
            if (!whiteIP.Contains(userIP))
            {
                RsModel.Add(new SAMPLE_BASModel.Res()
                {
                    SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                    Description = "0099非授權IP"
                });
                return RsModel;
            }

            // 公文文號查詢
            if (model != null && !string.IsNullOrWhiteSpace(model.SAMPLE_RDOCUT_NO))
            {
                // Apply 主檔
                ApplyModel where = new ApplyModel();
                where.MOHW_CASE_NO = model.SAMPLE_RDOCUT_NO;
                var applyData = dao.GetRow<ApplyModel>(where);
                if (applyData != null)
                {
                    // OFFICIAl_DOC 公文文號關聯檔
                    OFFICIAL_DOC where_doc = new OFFICIAL_DOC();
                    where_doc.MOHW_CASE_NO = model.SAMPLE_RDOCUT_NO;
                    var docData = dao.GetRow<OFFICIAL_DOC>(where_doc);
                    string DocInsDate = string.Empty;
                    if (docData != null)
                    {
                        DocInsDate = HelperUtil.DateTimeToTwString(docData.INSERTDATE.Value);
                    }
                    if(applyData.SRV_ID == "005013")
                    {
                        // APPLY_005013 申辦案件
                        APPLY_005013 where_005013 = new APPLY_005013();
                        where_005013.APP_ID = applyData.APP_ID;
                        var app005013 = dao.GetRow<APPLY_005013>(where_005013);

                        SAMPLE_BASModel.Res InsertData = new SAMPLE_BASModel.Res();
                        InsertData.E_AppID = applyData.APP_ID;
                        InsertData.SAMPLE_RDOCUT_NO = applyData.MOHW_CASE_NO;
                        InsertData.SAMPLE_RDATE = DocInsDate;
                        InsertData.ORIGIN_CN_CODE = app005013.ORIGIN;
                        InsertData.SHIP_PORT_CODE = app005013.SHIPPINGPORT;
                        InsertData.SELL_CN_CODE = app005013.SELLER;
                        InsertData.APPLY_IDN_BAN = applyData.IDN;
                        InsertData.APPLY_IDN_NAME = applyData.NAME;
                        InsertData.APPLY_IDN_PHONE = applyData.TEL;
                        InsertData.APPLY_IDN_ADD = applyData.ADDR;
                        InsertData.CaseType = "0";//0:個人自用/1:專案進口

                        InsertData.Description = "0000成功";
                        RsModel.Add(InsertData);
                    }
                    else if(applyData.SRV_ID == "005014")
                    {
                        // APPLY_005014 申辦案件
                        Apply_005014 where_005014 = new Apply_005014();
                        where_005014.APP_ID = applyData.APP_ID;
                        var app005014 = dao.GetRow<Apply_005014>(where_005014);

                        SAMPLE_BASModel.Res InsertData = new SAMPLE_BASModel.Res();
                        InsertData.E_AppID = applyData.APP_ID;
                        InsertData.SAMPLE_RDOCUT_NO = applyData.MOHW_CASE_NO;
                        InsertData.SAMPLE_RDATE = DocInsDate;
                        InsertData.ORIGIN_CN_CODE = app005014.PRODUCTION_COUNTRY;
                        InsertData.SHIP_PORT_CODE = app005014.TRANSFER_COUNTRY;
                        InsertData.SELL_CN_CODE = app005014.SELL_COUNTRY;
                        InsertData.APPLY_IDN_BAN = applyData.IDN;
                        InsertData.APPLY_IDN_NAME = applyData.NAME;
                        InsertData.APPLY_IDN_PHONE = applyData.TEL;
                        InsertData.APPLY_IDN_ADD = applyData.ADDR;
                        InsertData.CaseType = "1";//0:個人自用/1:專案進口

                        InsertData.Description = "0000成功";
                        RsModel.Add(InsertData);
                    }
                    else
                    {
                        // 該公文號碼非005013,005014案件
                        RsModel.Add(new SAMPLE_BASModel.Res()
                        {
                            SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                            Description = "0001資料庫錯誤"
                        });
                    }
                }
                else
                {
                    // 查無該案件
                    RsModel.Add(new SAMPLE_BASModel.Res()
                    {
                        SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                        Description = "0001資料庫錯誤"
                    });
                }
            }
            else
            {
                // 未傳入參數
                RsModel.Add(new SAMPLE_BASModel.Res()
                {
                    SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                    Description = "0013未填入數值"
                });
            }
            //if (model != null && model.SAMPLE_RDOCUT_NO == "1090023516")
            //{
            //    RsModel.Add(new SAMPLE_BASModel.Res
            //    {
            //        E_AppID = "NULL",
            //        SAMPLE_RDOCUT_NO = "1090023516",
            //        SAMPLE_RDATE = "109/06/01",
            //        ORIGIN_CN_CODE = "DE",
            //        SHIP_PORT_CODE = "AE",
            //        SELL_CN_CODE = "EC",
            //        APPLY_IDN_BAN = "A127360506",
            //        APPLY_TRUST_NAME = "李浩民",
            //        APPLY_TRUST_TEL = "02-7986-5654",
            //        APPLY_TRUST_ADD = "台北市復興東路85號",
            //        CaseType = "1"
            //    });
            //}
            //else if (model != null && model.SAMPLE_RDOCUT_NO == "1090085431")
            //{
            //    RsModel.Add(new SAMPLE_BASModel.Res
            //    {
            //        E_AppID = "NULL",
            //        SAMPLE_RDOCUT_NO = "1090085431",
            //        SAMPLE_RDATE = "109/08/11",
            //        ORIGIN_CN_CODE = "CN",
            //        SHIP_PORT_CODE = "HK",
            //        SELL_CN_CODE = "JP",
            //        APPLY_IDN_BAN = "A123456789",
            //        APPLY_TRUST_NAME = "王曉明",
            //        APPLY_TRUST_TEL = "02-0000-1111",
            //        APPLY_TRUST_ADD = "台北市市府路1號",
            //        CaseType = "0"
            //    });
            //}
            return RsModel;
        }

    }
}
