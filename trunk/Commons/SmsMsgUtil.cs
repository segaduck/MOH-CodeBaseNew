using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Turbo.Commons;
using EECOnline.Models;

namespace EECOnline.Commons
{
    /// <summary>
    /// 【EVERY8D】簡訊發送工具
    /// </summary>
    public class SmsMsgUtil
    {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///  查詢剩餘點數
        /// </summary>
        /// <returns></returns>
        public string GetPoint()
        {
            using (WebClient webClient = new WebClient())
            {
                ServicePointManager.SecurityProtocol =
                //SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                return webClient.DownloadString("https://oms.every8d.com/vac/API21/HTTP/getCredit.ashx?UID=eisapi&PWD=eisapi7654321");
            }
        }

        /// <summary>
        /// 發送簡訊
        /// </summary>
        /// <param name="smsModel"></param>
        /// <returns></returns>
        public string SendSms(SmsMsgModel smsModel)
        {
            ////開發者測試電話號碼 (存在時, 所有發送簡訊轉到這個號碼)
            //if (!string.IsNullOrWhiteSpace(SystemParameter.DevRecvMobile))
            //{
            //    LOG.Info("DevRecvMobile 設定, 改為發送到  =[" + SystemParameter.DevRecvMobile + "]");
            //    smsModel.dstaddr = TesnUtil.SafeTrim((string)SystemParameter.DevRecvMobile.Replace("-", ""));
            //}

            var Url = "https://oms.every8d.com/vac/API21/HTTP/sendSMS.ashx";
            Url += smsModel.GetParamsUriString();
            var Url_point = "https://oms.every8d.com/vac/API21/HTTP/getCredit.ashx";
            Url_point += smsModel.GetParamsUriString2();
            try
            {
                var returnPonint = "";
                using (WebClient webClient = new WebClient())
                {
                    ServicePointManager.SecurityProtocol =
                       SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    returnPonint = webClient.DownloadString(Url_point);
                }

                using (WebClient webClient = new WebClient())
                {
                    ServicePointManager.SecurityProtocol =
                       SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                    //kmsgid=184143638

                    if (Convert.ToInt32(returnPonint) > 0)
                    {
                        var returnStr = webClient.DownloadString(Url);
                        //var returnStrList = returnStr.Split(',');

                        //    if (Int64.Parse(returnStr) < 0)
                        //    {
                        //        var msg = "";
                        //        switch (Int64.Parse(returnStr))
                        //        {
                        //            case -1:
                        //                msg = "CGI string error ，系統維護中或其他錯誤 ,帶入的參數異常,伺服器異常";
                        //                break;

                        //            case -2:
                        //                msg = "授權錯誤(帳號 / 密碼錯誤)";
                        //                break;

                        //            case -4:
                        //                msg = "A Number違反規則 發送端 870短碼VCSN 設定異常";
                        //                break;

                        //            case -5:
                        //                msg = "B Number違反規則 接收端 門號錯誤";
                        //                break;

                        //            case -6:
                        //                msg = "Closed User 接收端的門號停話異常090 094 099 付費代號等";
                        //                break;

                        //            case -20:
                        //                msg = "Schedule Time錯誤 預約時間錯誤 或時間已過";
                        //                break;

                        //            case -21:
                        //                msg = "Valid Time錯誤 有效時間錯誤";
                        //                break;

                        //            case -59999:
                        //                msg = "帳務系統異常 簡訊無法扣款送出";
                        //                break;

                        //            case -60002:
                        //                msg = "您帳戶中的點數不足";
                        //                break;

                        //            case -60014:
                        //                msg = "該用戶已申請 拒收簡訊平台之簡訊 (2010 NCC新規)";
                        //                break;

                        //            case -999959999:
                        //                msg = "在12 小時內，相同容錯機制碼";
                        //                break;

                        //            case -999969999:
                        //                msg = "同秒, 同門號, 同內容簡訊";
                        //                break;

                        //            case -999979999:
                        //                msg = "簡訊為空";
                        //                break;

                        //            default:
                        //                msg = returnStr;
                        //                break;
                        //        }
                        //        LOG.Error("發送簡訊失敗! => 平台回應錯誤資訊");
                        //        LOG.Error("回應訊息:[kmsgid=" + returnStr + "]");
                        //        return msg;
                        //    }
                    }
                    else
                    {
                        LOG.Error("發送簡訊失敗! => 簡訊餘額不足！目前餘額為：" + returnPonint + "");

                        return "發送簡訊失敗! => 簡訊餘額不足！目前餘額為：" + returnPonint + "";
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Error("發送簡訊失敗! => Exception");
                LOG.Error(JsonConvert.SerializeObject(smsModel));
                LOG.Error(ex.Message);
                LOG.Error(ex.StackTrace);
                return ex.Message;
            }
            return "";
        }
    }
}