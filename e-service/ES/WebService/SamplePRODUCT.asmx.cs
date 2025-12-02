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
    ///SamplePRODUCT 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    // [System.Web.Script.Services.ScriptService]
    public class SamplePRODUCT : System.Web.Services.WebService
    {
        [WebMethod]
        public List<SAMPLE_PRODUCTModel.Res> GetData(SAMPLE_PRODUCTModel.Req model)
        {
            ApplyDAO dao = new ApplyDAO();
            var RsModel = new List<SAMPLE_PRODUCTModel.Res>();
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
                RsModel.Add(new SAMPLE_PRODUCTModel.Res()
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
                    if (applyData.SRV_ID == "005013")
                    {
                        // APPLY_005013 申辦案件
                        APPLY_005013_ITEM1 where_1 = new APPLY_005013_ITEM1();
                        where_1.APP_ID = applyData.APP_ID;
                        var listItem1 = dao.GetRowList<APPLY_005013_ITEM1>(where_1);
                        if (listItem1 != null)
                        {
                            foreach (var item1 in listItem1)
                            {
                                SAMPLE_PRODUCTModel.Res InsertData = new SAMPLE_PRODUCTModel.Res();
                                InsertData.E_APPID = applyData.APP_ID;
                                InsertData.SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO;
                                InsertData.SAMPLE_ITEM = Convert.ToInt64(item1.ITEMNUM);
                                InsertData.PRODUCT_CH_NAME = item1.COMMODITIES;
                                InsertData.PRODUCT_EN_NAME = item1.SPEC;
                                InsertData.SAMPLE_QTY = Convert.ToInt64(item1.QTY);
                                InsertData.SAMPLE_QTY_UNIT_CODE = item1.UNIT;
                                InsertData.Description = "0000成功";
                                RsModel.Add(InsertData);
                            }
                        }
                        else
                        {
                            // 貨品沒資料
                            RsModel.Add(new SAMPLE_PRODUCTModel.Res()
                            {
                                SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                                Description = "0001資料庫錯誤"
                            });
                        }
                    }
                    else if (applyData.SRV_ID == "005014")
                    {
                        // APPLY_005014 申辦案件
                        Apply_005014_Item where_005014 = new Apply_005014_Item();
                        where_005014.APP_ID = applyData.APP_ID;
                        var app005014 = dao.GetRowList<Apply_005014_Item>(where_005014);
                        if (app005014 != null)
                        {
                            foreach (var item1 in app005014)
                            {
                                SAMPLE_PRODUCTModel.Res InsertData = new SAMPLE_PRODUCTModel.Res();
                                InsertData.E_APPID = applyData.APP_ID;
                                InsertData.SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO;
                                InsertData.SAMPLE_ITEM = item1.ITEM.Value + 1;
                                InsertData.SAMPLE_PORC_TYPE_CODE = item1.ITEM_TYPE == 1 ? "00" : "01";
                                InsertData.PRODUCT_CH_NAME = item1.COMMODITIES;
                                InsertData.PRODUCT_EN_NAME = item1.COMMODITIES_E;
                                InsertData.SAMPLE_QTY = Convert.ToInt64(item1.QTY);
                                InsertData.SAMPLE_QTY_UNIT_CODE = item1.UNIT;
                                InsertData.Description = "0000成功";
                                RsModel.Add(InsertData);
                            }
                        }
                        else
                        {
                            // 該公文號碼非005013,005014案件
                            RsModel.Add(new SAMPLE_PRODUCTModel.Res()
                            {
                                SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                                Description = "0001資料庫錯誤"
                            });
                        }
                    }
                    else
                    {
                        // 查無該案件
                        RsModel.Add(new SAMPLE_PRODUCTModel.Res()
                        {
                            SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                            Description = "0001資料庫錯誤"
                        });
                    }
                }
                else
                {
                    RsModel.Add(new SAMPLE_PRODUCTModel.Res()
                    {
                        SAMPLE_RDOCUT_NO = model.SAMPLE_RDOCUT_NO,
                        Description = "0001資料庫錯誤"
                    });
                }
            }

            //if (model != null && model.SAMPLE_RDOCUT_NO == "1090023516")
            //{
            //    RsModel.Add(new SAMPLE_PRODUCTModel.Res
            //    {
            //        E_APPID = "NULL",
            //        SAMPLE_RDOCUT_NO = "1090023516",
            //        SAMPLE_ITEM = 1,
            //        PRODUCT_CH_NAME = "測試品項1",
            //        PRODUCT_EN_NAME = "TestItem1",
            //        SAMPLE_PORC_TYPE_CODE = "01",
            //        SAMPLE_DRUG_FORM = "27",
            //        SAMPLE_QTY = 1.00000,
            //        SAMPLE_QTY_UNIT_CODE = "36",
            //        SAMPLE_UNIT_QTY = 150.00000,
            //        SAMPLE_UNIT_UNIT_CODE = "01"
            //    });
            //}
            //else if (model != null && model.SAMPLE_RDOCUT_NO == "1090085431")
            //{
            //    RsModel.Add(new SAMPLE_PRODUCTModel.Res
            //    {
            //        E_APPID = "NULL",
            //        SAMPLE_RDOCUT_NO = "1090085431",
            //        SAMPLE_ITEM = 1,
            //        PRODUCT_CH_NAME = "申請項目1",
            //        PRODUCT_EN_NAME = "OrderItem1",
            //        SAMPLE_PORC_TYPE_CODE = "02",
            //        SAMPLE_DRUG_FORM = "13",
            //        SAMPLE_QTY = 5.00000,
            //        SAMPLE_QTY_UNIT_CODE = "36",
            //        SAMPLE_UNIT_QTY = 300.00000,
            //        SAMPLE_UNIT_UNIT_CODE = "01"
            //    });
            //    RsModel.Add(new SAMPLE_PRODUCTModel.Res
            //    {
            //        E_APPID = "NULL",// APP_ID
            //        SAMPLE_RDOCUT_NO = "1090085431",//MOHE
            //        SAMPLE_ITEM = 2,//序號
            //        PRODUCT_CH_NAME = "申請項目2",//貨品名稱
            //        PRODUCT_EN_NAME = "OrderItem2",
            //        SAMPLE_PORC_TYPE_CODE = "01",
            //        SAMPLE_DRUG_FORM = "01",
            //        SAMPLE_QTY = 1.00000,
            //        SAMPLE_QTY_UNIT_CODE = "41",
            //        SAMPLE_UNIT_QTY = 250.00000,
            //        SAMPLE_UNIT_UNIT_CODE = "11"
            //    });
            //}
            return RsModel;
        }
    }
}
