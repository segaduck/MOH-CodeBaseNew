using Dapper;
using ES.Action;
using ES.Action.Form;
using ES.Areas.Admin.Models;
using ES.Areas.BACKMIN.Models;
using ES.Commons;
using ES.Models;
using ES.Models.Entities;
using ES.Models.Share;
using ES.Services;
using ES.Utils;
using log4net;
using Omu.ValueInjecter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Presentation;

namespace ES.DataLayers
{
    public class BackApplyDAO : BaseAction
    {
        protected static readonly ILog logger = ES.Utils.LogUtils.GetLogger();

        #region Tran

        /// <summary>
        /// 資料庫連線
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        public void Tran(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        #endregion

        #region 取得案件進度

        /// <summary>
        /// 取得案件進度
        /// </summary>
        /// <param name="APP_ID">案號</param>
        /// <param name="PCD">單位-進度</param>
        /// <returns></returns>
        public string GetSchedule(string APP_ID, string PCD)
        {
            string Msg = "";
            var result = new List<TblCODE_CD>();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }, { "@CODE_PCD", PCD }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = "";
                var srvID = APP_ID.Substring(8, 6);
                if (srvID == "005013" || srvID == "005014")
                {
                    _sql =
                        @"SELECT cd.CODE_MEMO AS CODE_DESC
                          FROM Apply_Log al JOIN CODE_CD cd on al.FLOW_CD = cd.CODE_CD and cd.CODE_KIND ='F_CASE_STATUS' and CODE_PCD = @CODE_PCD
                          WHERE 1=1 and al.APP_ID = @APP_ID 
                          ORDER BY al.MODTIME ";
                }
                else
                {
                    _sql =
                        @"SELECT cd.CODE_DESC
                          FROM Apply_Log al JOIN CODE_CD cd on al.FLOW_CD = cd.CODE_CD and cd.CODE_KIND ='F_CASE_STATUS' and CODE_PCD = @CODE_PCD
                          WHERE 1=1 and al.APP_ID = @APP_ID 
                          ORDER BY al.MODTIME ";
                }

                try
                {
                    result = conn.Query<TblCODE_CD>(_sql, parameters).ToList();

                    if (result.ToCount() > 0)
                    {
                        foreach (var item in result)
                        {
                            Msg += item.CODE_DESC + ">";
                        }
                        Msg = Msg.Substring(0, Msg.Length - 1);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        #endregion

        #region 補件寄信通知

        /// <summary>
        /// 補件寄信通知        
        /// </summary>
        /// <param name="MainBody">補件欄位</param>
        /// <param name="ACC_NAM">申請人姓名</param>
        /// <param name="CountStr">需補件項目件數</param>
        /// <param name="Mail">信箱</param>
        /// <param name="APP_ID">案件編號</param>
        /// <param name="Srv_Nam">申請案件名稱</param>
        /// <param name="Srv_ID">申請案件編號</param>
        /// <param name="Deadline">截止日期</param>
        /// <param name="ProjectStr">項目</param>
        public void SendMail_Notice(
            string MainBody, string ACC_NAM,
            int Count, string Mail, string APP_ID,
            string Srv_Nam, string Srv_ID,
            string CustomMailBody = "",
            DateTime? Deadline = null,
            string ProjectStr = "",
            string CustomSubject = "")
        {
            ShareDAO dao = new ShareDAO();

            var Sign = "衛生福利部";
            string notReplyMsg = "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。";

            // 信件標題
            string subject = CustomSubject;
            if (string.IsNullOrEmpty(CustomSubject))
            {
                subject = Srv_Nam + "，案件編號﹕" + APP_ID + " 補件通知";
            }

            // 信件內容
            string MailBody = "";
            MailBody += "<table align=\"left\" style=\"width:90%;\">";
            MailBody += " <tr><th align=\"left\">" + ACC_NAM + "，您好:</th></tr>";
            // 客製化信件內容
            switch (Srv_ID)
            {
                #region 社工司
                case "011002":
                case "011005":
                case "011006":
                case "011008":
                case "011009":
                    Sign = "衛生福利部社會救助及社工司";
                    MailBody += "<tr><td>本部已收到您所提交的" + Srv_Nam + "申請，經本部審查，尚有" + Count.ToString() + "件文件需請您補正。</td> </tr>";
                    MailBody += $"<tr><td>{ProjectStr}</td></tr>";
                    MailBody += "<tr><td>＊紙本文件請郵遞至「衛生福利部社會救助及社工司」(地址：115204 臺北市南港區忠孝東路六段488號)，並於信封左下角註明「申請" + Srv_Nam + "」、申請人姓名及連絡電話。</td> </tr>";
                    MailBody += "<tr><td>請您於5個工作天登入原申請系統補正資料，俾利賡續辦理您的申請案件；如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td> </tr>";
                    break;
                case "011007":
                    Sign = "衛生福利部社會救助及社工司";
                    MailBody += "<tr><td>本部已收到您所提交的" + Srv_Nam + "申請，經本部審查，尚有" + Count.ToString() + "件文件需請您補正。</td> </tr>";
                    MailBody += $"<tr><td>{ProjectStr}</td></tr>";
                    MailBody += "<tr><td>＊紙本文件請郵遞至「衛生福利部社會救助及社工司」(地址：115204 臺北市南港區忠孝東路六段488號)，並於信封左下角註明「申請" + Srv_Nam + "」、申請人姓名及連絡電話。</td> </tr>";
                    MailBody += "<tr><td>請您於5個工作天內登入原申請系統補正資料，俾利賡續辦理您的申請案件；如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td> </tr>";
                    break;
                case "011003":
                    MailBody += " <tr><td>本部已收到您所提交的" + Srv_Nam + "，經本部初審，尚有" + Count.ToString() + "件文件需請您補正。 </td></tr>";
                    MailBody += " <tr><td>需重新上傳之文件為：" + ProjectStr + " </td></tr>";
                    MailBody += " <td>請您於5個工作天登入原申請系統補正資料，俾利賡續辦理您的申請案件；如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝。</td>";
                    break;
                case "011004":
                    Sign = "衛生福利部社會救助及社工司";
                    MailBody += " <tr><td>本部已收到您所提交的" + Srv_Nam + "申請，經本部初審尚有(" + Count.ToString() + "件)項目須請您補正。</td> </tr>";
                    MailBody += " <tr><td>" + ProjectStr + " </td></tr>";
                    MailBody += " <td>請您於5個工作天登入原申請系統補正資料，俾利賡續辦理您的申請案件;如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td>";
                    break;
                case "011010":
                    Sign = "衛生福利部社會救助及社工司";
                    MailBody += "<tr><td>本部已收到您所提交的" + Srv_Nam + "申請，經本部審查，尚有" + Count.ToString() + "件文件需請您補正。</td> </tr>";
                    MailBody += $"<tr><td>{ProjectStr}</td></tr>";
                    MailBody += "<tr><td>請您於5個工作天登入原申請系統補正資料，俾利賡續辦理您的申請案件；如逾期未補正，因資料不全致無法審理，本部將不受理您的申請案件。謝謝</td> </tr>";
                    break;
                #endregion

                #region 醫藥司
                case "001005":
                case "001007":
                case "001009":
                case "001036":
                case "001039":
                    MailBody += " <tr><td>本部已收到您所提交的" + Srv_Nam + "申請，經本部初審尚有補件項目須請您補正。</td> </tr>";
                    if (Deadline == null) MailBody += " <td>請您於近日內補正資料，俾利賡續辦理您的申請案件;如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td>";
                    else MailBody += " <td>請您於 " + HelperUtil.DateTimeToTwString(Deadline) + " 內補正資料，俾利賡續辦理您的申請案件;如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td>";
                    break;
                #endregion

                #region 中醫藥司

                #endregion

                #region 國健署

                case "010001":
                case "012001":
                    Sign = "衛生福利部";
                    MailBody += "<tr><td>本部已收到您所提交的" + Srv_Nam + "申請，經本部審查，尚有" + Count.ToString() + "件文件需請您補正。</td> </tr>";
                    MailBody += $"<tr><td>{ProjectStr.TONotNullString().Replace("</br>", "\n")}</td></tr>";
                    MailBody += "<tr><td>請您於30天內補正資料，俾利賡續辦理您的申請案件；如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td> </tr>";
                    break;
                case "010002":
                    Sign = "國民健康署";
                    MailBody += "<tr><td>本署已收到您所提交的" + Srv_Nam + "申請，經本部審查，尚有(" + Count.ToString() + "份)文件需請您補正：" + ProjectStr.TONotNullString() + "</td> </tr>";
                    MailBody += "<tr><td>若有疑義，請洽本部國民健康署02-25220888 分機642、645。</td> </tr>";
                    MailBody += "<tr><td>「" + CustomMailBody.TONotNullString() + "」</td></tr>";
                    MailBody += "<tr><td>請您盡快補正資料，俾利賡續辦理您的申請案件；如未於60日內補正，本部將退回您的申請案件。謝謝。</td> </tr>";
                    break;
                #endregion

                default:
                    MailBody += " <tr><td>本部已收到您所提交的" + Srv_Nam + "申請，經本部初審尚有(" + Count.ToString() + "件)項目須請您補正。</td> </tr>";
                    if (Deadline == null) MailBody += " <td>請您於近日內補正資料，俾利賡續辦理您的申請案件;如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td>";
                    else MailBody += " <td>請您於 " + HelperUtil.DateTimeToTwString(Deadline) + " 內補正資料，俾利賡續辦理您的申請案件;如逾期未補正，因資料不全致無法審理，本部將退回您的申請案件。謝謝</td>";
                    break;
            }

            MailBody += "</tr>";
            MailBody += "<tr>";
            MailBody += "<td align=\"right\">" + Sign + "</td>";
            MailBody += "</tr>";
            MailBody += " <tr><td><br>" + notReplyMsg + "</td></tr>";
            MailBody += "</table>";

            // 中醫藥司會取代此段
            if (CustomMailBody.TONotNullString() != "" && Srv_ID != "010002")
            {
                MailBody = CustomMailBody;
            }

            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    this.conn = conn;
                    conn.Open();
                    // 寄信LOG
                    TblMAIL_LOG log = new TblMAIL_LOG();
                    log.MAIL = Mail;
                    log.SUBJECT = subject;
                    log.BODY = MailBody;
                    log.SEND_TIME = DateTime.Now;
                    log.SRV_ID = Srv_ID;

                    try
                    {
                        if (ConfigModel.MailRevTest == "1")
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                            if (ConfigModel.MailRevIsTwo == "1")
                            {
                                var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                                foreach (var rec in recList)
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }
                            }
                        }
                        else
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }

                        // 紀錄補件欄位
                        ApplyModel app = new ApplyModel();
                        app.APP_ID = APP_ID;
                        ApplyModel newapp = new ApplyModel();
                        newapp.MAILBODY = MainBody;

                        Update(newapp, app);

                        // 寄信成功
                        log.RESULT_MK = "Y";
                        Insert(log);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        // 寄信失敗
                        log.RESULT_MK = "N";
                        Insert(log);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                // 寄信LOG
                TblMAIL_LOG log = new TblMAIL_LOG();
                log.MAIL = Mail;
                log.SUBJECT = subject;
                log.BODY = MailBody;
                log.SEND_TIME = DateTime.Now;
                log.SRV_ID = Srv_ID;

                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }

                    // 紀錄補件欄位
                    ApplyModel app = new ApplyModel();
                    app.APP_ID = APP_ID;
                    ApplyModel newapp = new ApplyModel();
                    newapp.MAILBODY = MainBody;

                    Update(newapp, app);

                    // 寄信成功
                    log.RESULT_MK = "Y";
                    Insert(log);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    // 寄信失敗
                    log.RESULT_MK = "N";
                    Insert(log);
                }
            }
        }

        #endregion

        #region 通知補件

        /// <summary>
        /// 通知補件        
        /// </summary>
        /// <param name="UserName">申請人姓名</param>
        /// <param name="ServiceName">案件名稱</param>
        /// <param name="ServiceId">案件代碼</param>
        /// <param name="Mail">mail</param>
        /// <param name="ApplyDate">申請日期</param>
        /// <param name="AppId">案件編號</param>
        public void SendMail_Notice(string UserName, string ServiceName, string ServiceId, string Mail, string ApplyDate, string AppId, string note)
        {
            ShareDAO dao = new ShareDAO();

            //查詢網站網址
            string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            Uri uri = new Uri(webUrl);
            string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
            string appDocUrl = string.Format("{0}/Apply_{1}/AppDoc?APP_ID={2}", webDomain, ServiceId, AppId);
            string questUrl = string.Format("{0}/QuestWeb?useCache=2&id={1}", webDomain, AppId);

            // 信件標題
            string subject = ServiceName + "，案件編號﹕" + AppId + " 通知補件";
            // 信件內容
            string MailBody = "";
            MailBody += UserName + "，您好:<br/><br/>";
            MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦之(" + ServiceName + ")案件<br/>";
            //MailBody += "申請編號：<a href='https://e-service.mohw.gov.tw//History/Show/" + AppId + "'>" + AppId + "</a>現在進度為'通知補件'<br/>";
            MailBody += "申請編號：<a href='" + appDocUrl + "'>" + AppId + "</a>現在進度為'通知補件'<br/>";
            MailBody += note + "<br/><br/>";
            MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
            MailBody += "請至<a href='" + questUrl + "'>滿意度問卷</a>填寫滿意度調查表，您的寶貴意見將做為本部改進事項<br/><br/>";
            switch (ServiceId)
            {
                case "001036":
                    MailBody += "衛生福利部護理及健康照護司敬上<br/><br/>";
                    break;
                case "010001":
                    MailBody += "衛生福利部國民健康署敬上<br/><br/>";
                    break;
                case "011001":
                case "011002":
                case "011003":
                case "011004":
                case "011005":
                case "011006":
                    //MailBody += "衛生福利部社會救助及社工司敬上<br/><br/>";
                    MailBody += "衛生福利部社會救助及社工司<br/><br/>";
                    break;
                case "012001":
                    MailBody += "衛生福利部秘書處敬上<br/><br/>";
                    break;
                default:
                    MailBody += "衛生福利部醫事司敬上<br/><br/>";
                    break;
            }
            MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
            switch (AppId)
            {
                case "001034":
                case "001035":
                case "001038":
                    MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                    MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                    MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                    MailBody += "115204 臺北市南港區忠孝東路六段488號";
                    break;
            }


            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    this.conn = conn;
                    conn.Open();
                    // 寄信LOG
                    TblMAIL_LOG log = new TblMAIL_LOG();
                    log.MAIL = Mail;
                    log.SUBJECT = subject;
                    log.BODY = MailBody;
                    log.SEND_TIME = DateTime.Now;
                    log.SRV_ID = ServiceId;

                    try
                    {
                        if (ConfigModel.MailRevTest == "1")
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                            if (ConfigModel.MailRevIsTwo == "1")
                            {
                                var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                                foreach (var rec in recList)
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }
                            }
                        }
                        else
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }

                        // 寄信成功
                        log.RESULT_MK = "Y";
                        Insert(log);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        logger.Error("SendMail_Notice failed:" + ex.TONotNullString());
                        // 寄信失敗
                        log.RESULT_MK = "N";
                        Insert(log);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                // 寄信LOG
                TblMAIL_LOG log = new TblMAIL_LOG();
                log.MAIL = Mail;
                log.SUBJECT = subject;
                log.BODY = MailBody;
                log.SEND_TIME = DateTime.Now;
                log.SRV_ID = ServiceId;

                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }

                    // 寄信成功
                    log.RESULT_MK = "Y";
                    Insert(log);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    logger.Error("SendMail_Notice failed:" + ex.TONotNullString());
                    // 寄信失敗
                    log.RESULT_MK = "N";
                    Insert(log);
                }
            }
        }

        #endregion

        #region 發文歸檔寄信通知

        /// <summary>
        /// 發文歸檔寄信通知        
        /// </summary>
        /// <param name="UserName">申請人姓名</param>
        /// <param name="ServiceName">案件名稱</param>
        /// <param name="ServiceId">案件代碼</param>
        /// <param name="Mail">mail</param>
        /// <param name="ApplyDate">申請日期</param>
        /// <param name="AppId">案件編號</param>
        /// <param name="MailDate">郵寄日期</param>
        /// <param name="MailBarcode">掛號條碼</param> 
        public void SendMail_Archive(string UserName, string ServiceName, string ServiceId, string Mail, string ApplyDate, string AppId, string MailDate, string MailBarcode)
        {
            ShareDAO dao = new ShareDAO();

            //查詢網站網址
            string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            Uri uri = new Uri(webUrl);
            string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
            string appDocUrl = string.Format("{0}/Apply_{1}/AppDoc?APP_ID={2}", webDomain, ServiceId, AppId); //案件明細頁連結
            string questUrl = string.Format("{0}/QuestWeb?useCache=2&id={1}", webDomain, AppId);

            // 信件標題
            string subject = ServiceName + "，案件編號﹕" + AppId + " 發文歸檔通知";
            // 信件內容
            string MailBody = "";

            MailBody += UserName + "，您好:<br/><br/>";
            MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦之(" + ServiceName + ")案件<br/>";
            //MailBody += "申請編號：<a href='https://e-service.mohw.gov.tw//History/Show/" + AppId + "'>" + AppId + "</a>現在進度為'核可(發文歸檔)<br/>";
            MailBody += "申請編號：<a href='" + appDocUrl + "'>" + AppId + "</a>現在進度為'核可(發文歸檔)'<br/>";
            MailBody += "已於" + MailDate.Substring(4, 2) + "月" + MailDate.Substring(6, 2) + "日寄出，掛號條碼：" + MailBarcode + " <br/>";
            MailBody += "請至<a href='" + questUrl + "'>滿意度問卷</a>填寫滿意度調查表，您的寶貴意見將做為本部改進事項<br/><br/>";
            MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
            //MailBody += "請至<a href='https://e-service.mohw.gov.tw//QuestWeb/Index/" + AppId + "'> 滿意度問卷 </a> 填寫滿意度調查表,您的寶貴意見將做為本部改進事項<br/><br/>";
            switch (ServiceId)
            {
                case "001036":
                    MailBody += "衛生福利部護理及健康照護司敬上 <br/>";
                    break;
                default:
                    MailBody += "衛生福利部醫事司敬上 <br/>";
                    break;
            }
            MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。 ";


            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    this.conn = conn;
                    conn.Open();
                    // 寄信LOG
                    TblMAIL_LOG log = new TblMAIL_LOG();
                    log.MAIL = Mail;
                    log.SUBJECT = subject;
                    log.BODY = MailBody;
                    log.SEND_TIME = DateTime.Now;
                    log.SRV_ID = ServiceId;

                    try
                    {
                        if (ConfigModel.MailRevTest == "1")
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                            if (ConfigModel.MailRevIsTwo == "1")
                            {
                                var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                                foreach (var rec in recList)
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }
                            }
                        }
                        else
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }

                        // 寄信成功
                        log.RESULT_MK = "Y";
                        Insert(log);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        // 寄信失敗
                        log.RESULT_MK = "N";
                        Insert(log);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                // 寄信LOG
                TblMAIL_LOG log = new TblMAIL_LOG();
                log.MAIL = Mail;
                log.SUBJECT = subject;
                log.BODY = MailBody;
                log.SEND_TIME = DateTime.Now;
                log.SRV_ID = ServiceId;

                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }

                    // 寄信成功
                    log.RESULT_MK = "Y";
                    Insert(log);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    // 寄信失敗
                    log.RESULT_MK = "N";
                    Insert(log);
                }
            }
        }

        #endregion

        #region 逾期未補件而予結案

        /// <summary>
        /// 逾期未補件而予結案        
        /// </summary>
        /// <param name="UserName">申請人姓名</param>
        /// <param name="ServiceName">案件名稱</param>
        /// <param name="ServiceId">案件代碼</param>
        /// <param name="Mail">mail</param>
        /// <param name="ApplyDate">申請日期</param>
        /// <param name="AppId">案件編號</param>
        public void SendMail_Expired(string UserName, string ServiceName, string ServiceId, string Mail, string ApplyDate, string AppId, string note)
        {
            ShareDAO dao = new ShareDAO();
            //查詢網站網址
            string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            Uri uri = new Uri(webUrl);
            string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
            string appDocUrl = string.Format("{0}/Apply_{1}/AppDoc?APP_ID={2}", webDomain, ServiceId, AppId);
            string questUrl = string.Format("{0}/QuestWeb?useCache=2&id={1}", webDomain, AppId);

            // 信件標題
            string subject = ServiceName + "，案件編號﹕" + AppId + " 逾期未補件而予結案";
            // 信件內容
            string MailBody = "";
            MailBody += UserName + "，您好:<br/><br/>";
            MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦之(" + ServiceName + ")案件<br/>";
            //MailBody += "申請編號：<a href='https://e-service.mohw.gov.tw//History/Show/" + AppId + "'>" + AppId + "</a>現在進度為'逾期未補件而予結案'<br/>";
            MailBody += "申請編號：<a href='" + appDocUrl + "'>" + AppId + "</a>現在進度為'逾期未補件而予結案'<br/>";
            if (!string.IsNullOrWhiteSpace(note))
            {
                MailBody += "通知備註：" + note + "<br/><br/>";
            }
            else
            {
                MailBody += "<br/><br/>";
            }

            switch (ServiceId)
            {
                case "001005":
                case "001007":
                case "001008":
                case "001009":
                case "001034":
                case "001036":
                case "001037":
                case "001039":
                    //20201221 fix [MOHES-952]醫事人員請領英文證明書 - 001008 - (通知信)逾期未補件而予結案 內容調整
                    MailBody += String.Format("依據本部公告之「民眾申請案件處理期限表」{0}申請處理期限<br/>為5日（日曆天，即包含例假日），由於尚未收到台端補件資料(費用)，故先行結案，<br/>俟補件資料(規費)收件後，再予以續辦，造成台端不便，敬請見諒。<br/><br/>", ServiceName);
                    break;
            }


            MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
            MailBody += "請至<a href='" + questUrl + "'>滿意度問卷</a>填寫滿意度調查表，您的寶貴意見將做為本部改進事項<br/><br/>";
            switch (ServiceId)
            {
                case "001036":
                    MailBody += "衛生福利部護理及健康照護司敬上<br/><br/>";
                    break;
                case "010001":
                case "010002":
                    MailBody += "衛生福利部國民健康署敬上<br/><br/>";
                    break;
                case "011001":
                case "011002":
                case "011003":
                case "011004":
                case "011005":
                case "011006":
                case "011007":
                case "011008":
                case "011009":
                case "011010":
                    MailBody += "衛生福利部社會救助及社工司<br/><br/>";
                    break;
                case "012001":
                    MailBody += "衛生福利部秘書處敬上<br/><br/>";
                    break;
                default:
                    MailBody += "衛生福利部醫事司敬上<br/><br/>";
                    break;
            }

            MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
            switch (ServiceId)
            {
                case "001034":
                case "001035":
                case "001038":
                    MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                    MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                    MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                    MailBody += "115204 臺北市南港區忠孝東路六段488號";
                    break;
            }

            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    this.conn = conn;
                    conn.Open();
                    // 寄信LOG
                    TblMAIL_LOG log = new TblMAIL_LOG();
                    log.MAIL = Mail;
                    log.SUBJECT = subject;
                    log.BODY = MailBody;
                    log.SEND_TIME = DateTime.Now;
                    log.SRV_ID = ServiceId;

                    try
                    {
                        if (ConfigModel.MailRevTest == "1")
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                            if (ConfigModel.MailRevIsTwo == "1")
                            {
                                var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                                foreach (var rec in recList)
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }
                            }
                        }
                        else
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }

                        // 寄信成功
                        log.RESULT_MK = "Y";
                        Insert(log);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        // 寄信失敗
                        log.RESULT_MK = "N";
                        Insert(log);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                // 寄信LOG
                TblMAIL_LOG log = new TblMAIL_LOG();
                log.MAIL = Mail;
                log.SUBJECT = subject;
                log.BODY = MailBody;
                log.SEND_TIME = DateTime.Now;
                log.SRV_ID = ServiceId;

                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }

                    // 寄信成功
                    log.RESULT_MK = "Y";
                    Insert(log);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    // 寄信失敗
                    log.RESULT_MK = "N";
                    Insert(log);
                }
            }
        }

        #endregion

        #region 已接收，處理中 

        /// <summary>
        /// 已接收，處理中        
        /// </summary>
        /// <param name="UserName">申請人姓名</param>
        /// <param name="ServiceName">案件名稱</param>
        /// <param name="ServiceId">案件代碼</param>
        /// <param name="Mail">mail</param>
        /// <param name="ApplyDate">申請日期</param>
        /// <param name="AppId">案件編號</param>
        public void SendMail_InPorcess(string UserName, string ServiceName, string ServiceId, string Mail, string ApplyDate, string AppId, string note)
        {
            ShareDAO dao = new ShareDAO();

            //查詢網站網址
            string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            Uri uri = new Uri(webUrl);
            string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
            string appDocUrl = string.Format("{0}/Apply_{1}/AppDoc?APP_ID={2}", webDomain, ServiceId, AppId);
            string questUrl = string.Format("{0}/QuestWeb?useCache=2&id={1}", webDomain, AppId);

            // 信件標題
            string subject = ServiceName + "，案件編號﹕" + AppId + " 已接收，處理中";
            // 信件內容
            string MailBody = "";
            MailBody += UserName + "，您好:<br/><br/>";
            MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦之(" + ServiceName + ")案件<br/>";
            MailBody += "申請編號：<a href='" + appDocUrl + "'>" + AppId + "</a>現在進度為'已接收，處理中'<br/>";
            MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/>";

            if (note.TONotNullString() != "")
            {
                MailBody += "備註:" + note + "<br/><br/>";
            }


            MailBody += "請至<a href='" + questUrl + "'>滿意度問卷</a>填寫滿意度調查表，您的寶貴意見將做為本部改進事項<br/><br/>";
            switch (ServiceId)
            {
                case "001036":
                    MailBody += "衛生福利部護理及健康照護司敬上<br/><br/>";
                    break;
                case "010001":
                case "010002":
                    MailBody += "衛生福利部國民健康署敬上<br/><br/>";
                    break;
                case "011001":
                case "011002":
                case "011003":
                case "011004":
                case "011005":
                case "011006":
                case "011007":
                case "011008":
                case "011009":
                case "011010":
                    MailBody += "衛生福利部社會救助及社工司<br/><br/>";
                    break;
                case "012001":
                    MailBody += "衛生福利部秘書處敬上<br/><br/>";
                    break;
                case "006001":
                    MailBody += "衛生福利部國民年金監理會敬上<br/><br/>";
                    break;
                default:
                    MailBody += "衛生福利部醫事司敬上<br/><br/>";
                    break;
            }

            MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
            switch (ServiceId)
            {
                case "001034":
                case "001035":
                case "001038":
                    MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                    MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                    MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                    MailBody += "115204 臺北市南港區忠孝東路六段488號";
                    break;
            }

            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    this.conn = conn;
                    conn.Open();
                    // 寄信LOG
                    TblMAIL_LOG log = new TblMAIL_LOG();
                    log.MAIL = Mail;
                    log.SUBJECT = subject;
                    log.BODY = MailBody;
                    log.SEND_TIME = DateTime.Now;
                    log.SRV_ID = ServiceId;

                    try
                    {
                        if (ConfigModel.MailRevTest == "1")
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                            if (ConfigModel.MailRevIsTwo == "1")
                            {
                                var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                                foreach (var rec in recList)
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }
                            }
                        }
                        else
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }

                        // 寄信成功
                        log.RESULT_MK = "Y";
                        Insert(log);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        // 寄信失敗
                        log.RESULT_MK = "N";
                        Insert(log);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                // 寄信LOG
                TblMAIL_LOG log = new TblMAIL_LOG();
                log.MAIL = Mail;
                log.SUBJECT = subject;
                log.BODY = MailBody;
                log.SEND_TIME = DateTime.Now;
                log.SRV_ID = ServiceId;

                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }

                    // 寄信成功
                    log.RESULT_MK = "Y";
                    Insert(log);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    // 寄信失敗
                    log.RESULT_MK = "N";
                    Insert(log);
                }
            }
        }


        #endregion

        #region 完成申請

        /// <summary>
        /// 完成申請      
        /// </summary>
        /// <param name="UserName">申請人姓名</param>
        /// <param name="ServiceName">案件名稱</param>
        /// <param name="ServiceId">案件代碼</param>
        /// <param name="Mail">mail</param>
        /// <param name="ApplyDate">申請日期</param>
        /// <param name="AppId">案件編號</param>
        public void SendMail_Success(string UserName, string ServiceName, string ServiceId, string Mail, string ApplyDate, string AppId, string note)
        {
            ShareDAO dao = new ShareDAO();

            //查詢網站網址
            string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            Uri uri = new Uri(webUrl);
            string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
            string appDocUrl = string.Format("{0}/Apply_{1}/AppDoc?APP_ID={2}", webDomain, ServiceId, AppId);
            string questUrl = string.Format("{0}/QuestWeb?useCache=2&id={1}", webDomain, AppId);

            // 信件標題
            string subject = ServiceName + "，案件編號﹕" + AppId + " 完成申請";
            // 信件內容
            string MailBody = "";
            switch (ServiceId)
            {
                case "006001":
                    MailBody += UserName + "，您好:<br/><br/>";
                    MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦「" + ServiceName + "」服務";
                    MailBody += "，案件編號："+ AppId + " 已完成申請程序，特此通知。<br/>";
                    MailBody += "案件將由本部國民年金監理會辦理後續審議程序，感謝您使用本部線上申辦服務。<br/>";
                    break;
                case "041001":
                    MailBody += UserName + "，您好:<br/><br/>";
                    MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦「" + ServiceName + "」服務";
                    MailBody += "，案件編號：" + AppId + " 已完成申請程序，特此通知。<br/>";
                    MailBody += "案件將由本部全民健康保險爭議審議會辦理後續審議程序，感謝您使用本部線上申辦服務。<br/>";
                    break;
                case "040001":
                    MailBody += UserName + "先生/女士，您好：<br/><br/>";
                    MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦之(" + ServiceName + ")案件<br/>";
                    MailBody += "申請編號：" + AppId + " 已完成申請<br/>";
                    MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/>";
                    break;
                default:
                    MailBody += UserName + "，您好:<br/><br/>";
                    MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦之(" + ServiceName + ")案件<br/>";
                    MailBody += "申請編號：<a href='" + appDocUrl + "'>" + AppId + "</a> 已完成申請<br/>";
                    MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/>";
                    break;
            }
            
            if (note.TONotNullString() != "")
            {
                MailBody += "備註:" + note + "<br/><br/>";
            }

            switch (ServiceId)
            {
                case "001036":
                    MailBody += "衛生福利部護理及健康照護司敬上<br/><br/>";
                    break;
                case "010001":
                    MailBody += "衛生福利部國民健康署敬上<br/><br/>";
                    break;
                case "011001":
                case "011002":
                case "011003":
                case "011004":
                case "011005":
                case "011006":
                case "011007":
                case "011008":
                case "011009":
                case "011010":
                    MailBody += "衛生福利部社會救助及社工司<br/><br/>";
                    break;
                case "012001":
                    MailBody += "衛生福利部秘書處敬上<br/><br/>";
                    break;
                case "040001":
                    MailBody += "衛生福利部法規會敬上<br/><br/>";
                    break;
                case "041001":
                    MailBody += "全民健康保險爭議審議會<br/><br/>";
                    break;
                case "006001":
                    MailBody += "衛生福利部國民年金監理會<br/><br/>";
                    break;
                default:
                    MailBody += "衛生福利部醫事司敬上<br/><br/>";
                    break;
            }
           
            switch (ServiceId)
            {
                case "001034":
                case "001035":
                case "001038":
                    MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
                    MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                    MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                    MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                    MailBody += "115204 臺北市南港區忠孝東路六段488號";
                    break;
                case "006001":
                    MailBody += "P.S.本郵件係系統自動發信，請勿直接回信；如有相關疑問，請於平日上午9時至下午5時，逕向本部國民年金監理會洽詢（電話：02-3343-7136）。";
                    break;
                case "041001":
                    MailBody += "P.S.本郵件係系統自動發信，請勿直接回信；如有相關疑問，請於平日上午9時至下午5時，逕向本部全民健康保險爭議審議會洽詢（電話：02-8590-7222）。";
                    break;
                case "040001":
                    MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部法規會洽詢。";
                    break;
                default:
                    MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
                    break;
            }

            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    this.conn = conn;
                    conn.Open();
                    // 寄信LOG
                    TblMAIL_LOG log = new TblMAIL_LOG();
                    log.MAIL = Mail;
                    log.SUBJECT = subject;
                    log.BODY = MailBody;
                    log.SEND_TIME = DateTime.Now;
                    log.SRV_ID = ServiceId;

                    try
                    {
                        if (ConfigModel.MailRevTest == "1")
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                            if (ConfigModel.MailRevIsTwo == "1")
                            {
                                var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                                foreach (var rec in recList)
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }
                            }
                        }
                        else
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }

                        // 寄信成功
                        log.RESULT_MK = "Y";
                        Insert(log);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        // 寄信失敗
                        log.RESULT_MK = "N";
                        Insert(log);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                // 寄信LOG
                TblMAIL_LOG log = new TblMAIL_LOG();
                log.MAIL = Mail;
                log.SUBJECT = subject;
                log.BODY = MailBody;
                log.SEND_TIME = DateTime.Now;
                log.SRV_ID = ServiceId;

                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }

                    // 寄信成功
                    log.RESULT_MK = "Y";
                    Insert(log);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    // 寄信失敗
                    log.RESULT_MK = "N";
                    Insert(log);
                }
            }
        }


        #endregion

        #region 通用寄信功能

        /// <summary>
        /// 通用寄信功能        
        /// </summary>
        /// <param name="Mail">信箱</param>
        /// <param name="Subject">主旨</param>
        /// <param name="Body">內容</param>
        /// 
        public void SendMail(string Mail, string Subject, string Body, string ServiceId = "")
        {
            ShareDAO dao = new ShareDAO();
            // 寄信LOG
            TblMAIL_LOG log = new TblMAIL_LOG();
            log.MAIL = Mail;
            log.SUBJECT = Subject;
            log.BODY = Body;
            log.SEND_TIME = DateTime.Now;
            log.SRV_ID = ServiceId;

            if (this.conn == null)
            {
                this.conn = DataUtils.GetConnection();
                this.conn.Open();
            }

            try
            {
                if (ConfigModel.MailRevTest == "1")
                {
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, Subject, Body);
                    mailMessage.IsBodyHtml = true;
                    CommonsServices.SendMail(mailMessage);
                    if (ConfigModel.MailRevIsTwo == "1")
                    {
                        var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                        foreach (var rec in recList)
                        {
                            mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, Subject, Body);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }
                    }
                }
                else
                {
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, Subject, Body);
                    mailMessage.IsBodyHtml = true;
                    CommonsServices.SendMail(mailMessage);
                }

                // 寄信成功
                log.RESULT_MK = "Y";
                Insert(log);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
                // 寄信失敗
                log.RESULT_MK = "N";
                Insert(log);
            }

        }


        #endregion  

        #region 自請撤銷

        /// <summary>
        /// 自請撤銷        
        /// </summary>
        /// <param name="UserName">申請人姓名</param>
        /// <param name="ServiceName">案件名稱</param>
        /// <param name="ServiceId">案件代碼</param>
        /// <param name="Mail">mail</param>
        /// <param name="ApplyDate">申請日期</param>
        /// <param name="AppId">案件編號</param>
        public void SendMail_Cancel(string UserName, string ServiceName, string ServiceId, string Mail, string ApplyDate, string AppId, string note)
        {
            ShareDAO dao = new ShareDAO();
            //查詢網站網址
            string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            Uri uri = new Uri(webUrl);
            string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
            string appDocUrl = string.Format("{0}/Apply_{1}/AppDoc?APP_ID={2}", webDomain, ServiceId, AppId);
            string questUrl = string.Format("{0}/QuestWeb?useCache=2&id={1}", webDomain, AppId);

            // 信件標題
            string subject = ServiceName + "，案件編號﹕" + AppId + " 自請撤銷";
            // 信件內容
            string MailBody = "";
            MailBody += UserName + "，您好:<br/><br/>";
            MailBody += "您於民國" + (Convert.ToInt32(ApplyDate.Substring(0, 4)) - 1911).ToString() + "年" + ApplyDate.Replace("/", "").Substring(4, 2) + "月" + ApplyDate.Replace("/", "").Substring(6, 2) + "日申辦之(" + ServiceName + ")案件<br/>";
            MailBody += "申請編號：<a href='" + appDocUrl + "'>" + AppId + "</a>現在進度為'自請撤銷'<br/>";
            if (!string.IsNullOrWhiteSpace(note))
            {
                MailBody += "通知備註：" + note + "<br/><br/>";
            }
            else
            {
                MailBody += "<br/><br/>";
            }

            MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";

            switch (ServiceId)
            {
                case "001036":
                    MailBody += "衛生福利部護理及健康照護司敬上<br/><br/>";
                    break;
                case "010001":
                case "010002":
                    MailBody += "衛生福利部國民健康署敬上<br/><br/>";
                    break;
                case "011001":
                case "011002":
                case "011003":
                case "011004":
                case "011005":
                case "011006":
                case "011007":
                case "011008":
                case "011009":
                case "011010":
                    MailBody += "衛生福利部社會救助及社工司<br/><br/>";
                    break;
                case "012001":
                    MailBody += "衛生福利部秘書處敬上<br/><br/>";
                    break;
                default:
                    MailBody += "衛生福利部醫事司敬上<br/><br/>";
                    break;
            }

            MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
            switch (ServiceId)
            {
                case "001034":
                case "001035":
                case "001038":
                    MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                    MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                    MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                    MailBody += "115204 臺北市南港區忠孝東路六段488號";
                    break;
            }

            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    this.conn = conn;
                    conn.Open();
                    // 寄信LOG
                    TblMAIL_LOG log = new TblMAIL_LOG();
                    log.MAIL = Mail;
                    log.SUBJECT = subject;
                    log.BODY = MailBody;
                    log.SEND_TIME = DateTime.Now;
                    log.SRV_ID = ServiceId;

                    try
                    {
                        if (ConfigModel.MailRevTest == "1")
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                            if (ConfigModel.MailRevIsTwo == "1")
                            {
                                var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                                foreach (var rec in recList)
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }
                            }
                        }
                        else
                        {
                            MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }

                        // 寄信成功
                        log.RESULT_MK = "Y";
                        Insert(log);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        // 寄信失敗
                        log.RESULT_MK = "N";
                        Insert(log);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                // 寄信LOG
                TblMAIL_LOG log = new TblMAIL_LOG();
                log.MAIL = Mail;
                log.SUBJECT = subject;
                log.BODY = MailBody;
                log.SEND_TIME = DateTime.Now;
                log.SRV_ID = ServiceId;

                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, subject, MailBody);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }

                    // 寄信成功
                    log.RESULT_MK = "Y";
                    Insert(log);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    // 寄信失敗
                    log.RESULT_MK = "N";
                    Insert(log);
                }
            }
        }

        #endregion

        #region 取得案件歷程

        /// <summary>
        /// 取得案件歷程
        /// </summary>
        /// <param name="APP_ID">案號</param>
        /// <returns></returns>
        public IList<LogModel> GetLog(string APP_ID, IList<string> tableName)
        {
            var result = new List<LogModel>();
            conn = DataUtils.GetConnection();
            conn.Open();

            try
            {
                var colString = "";
                var tableJoinList = new List<string>();
                var srv_id = APP_ID.Substring(8, 6);
                int i = 0;
                foreach (var tableLog in tableName)
                {
                    Dictionary<string, object> argsTable = new Dictionary<string, object>();
                    var _sqlTable = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME =@TN";
                    argsTable.Add("TN", tableLog);
                    var DbTableList = GetList(_sqlTable, argsTable);
                    if (DbTableList.ToCount() > 0)
                    {
                        i++;

                        colString += ",";

                        Dictionary<string, object> argsCol = new Dictionary<string, object>();
                        string _sql_colName = "SELECT TABLE_NAME + '.' + COLUMN_NAME+' ' + TABLE_NAME+COLUMN_NAME COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TN AND COLUMN_NAME NOT IN ('MODTIME','MODUSER','MODTYPE') ";
                        argsCol.Add("TN", tableLog);
                        var ColList = GetList(_sql_colName, argsCol);
                        var ColmnName = ColList.Select(m => m["COLUMN_NAME"].TONotNullString()).ToList();
                        colString += string.Join(",", ColmnName);


                        var _sqlJoin = " LEFT JOIN " + tableLog + " ON Apply_LOG.APP_ID = " + tableLog + ".APP_ID  ";
                        _sqlJoin += " AND Apply_LOG.MODTIME <= " + tableLog + ".MODTIME ";
                        _sqlJoin += " AND ISNULL((SELECT TOP 1 MODTIME FROM Apply_Log B WHERE B.MODTIME > Apply_LOG.MODTIME AND B.APP_ID=Apply_LOG.APP_ID ORDER BY MODTIME),'99999999999999') >= " + tableLog + ".MODTIME ";

                        tableJoinList.Add(_sqlJoin);
                    }
                }

                string _sql = " SELECT Apply_LOG.APP_ID, case when isnull(Apply_LOG.MODTIME,'') = '' then replace(replace(replace(convert(varchar,Apply_LOG.UPD_TIME,120),'-',''),' ',''),':','') else Apply_LOG.MODTIME end as MODTIME ";
                _sql += " ,Apply_File_Log.MODTIME FMODTIME,Apply_File_Log.FILENAME,APPLY_FILE_LOG.SRC_FILENAME ";
                _sql += " ,Apply_LOG.MODTIME SMODTIME ";
                _sql += " ,ISNULL((SELECT TOP 1 MODTIME FROM Apply_Log B WHERE B.MODTIME > Apply_LOG.MODTIME AND B.APP_ID=Apply_LOG.APP_ID ORDER BY MODTIME),'99999999999999') EMODTIME ";
                _sql += " ,APPLY.UNIT_CD,Apply_Log.FLOW_CD";
                // 民眾少量、專案進口
                if (srv_id == "005013" || srv_id == "005014")
                {
                    _sql += ",CODE_CD.CODE_MEMO AS CODE_DESC ";
                }
                else
                {
                    _sql += ",CODE_CD.CODE_DESC ";
                }
                _sql += " ,Apply_LOG.IDN,Apply_LOG.BIRTHDAY,Apply_LOG.MOBILE,Apply_LOG.TEL,Apply_LOG.ADDR,Apply_LOG.ENAME,Apply_LOG.SEX_CD,Apply_LOG.NAME,Apply_LOG.PAY_POINT ";
                _sql += colString;

                _sql += " FROM Apply_LOG ";
                _sql += " LEFT JOIN APPLY ON APPLY.APP_ID = Apply_LOG.APP_ID ";
                _sql += " LEFT JOIN Apply_File_LOG ON Apply_LOG.APP_ID = Apply_File_LOG.APP_ID  ";
                _sql += " AND Apply_LOG.MODTIME <= Apply_File_LOG.MODTIME ";
                _sql += " AND ISNULL((SELECT TOP 1 MODTIME FROM Apply_Log B WHERE B.MODTIME > Apply_LOG.MODTIME AND B.APP_ID=Apply_LOG.APP_ID ORDER BY MODTIME),'99999999999999') >= Apply_File_LOG.MODTIME ";
                _sql += " LEFT JOIN UNIT ON UNIT.UNIT_CD = APPLY.UNIT_CD ";
                _sql += " LEFT JOIN CODE_CD ON CODE_CD.CODE_KIND = 'F_CASE_STATUS' AND CODE_CD.CODE_PCD = UNIT.UNIT_SCD AND CODE_CD.CODE_CD = Apply_LOG.FLOW_CD AND CODE_CD.DEL_MK = 'N' ";

                _sql += string.Join(" ", tableJoinList);

                _sql += " WHERE Apply_LOG.APP_ID = @APPID ";
                _sql += " ORDER BY Apply_LOG.UPD_TIME DESC ";

                Dictionary<string, object> args = new Dictionary<string, object>();
                args.Add("APPID", APP_ID);
                var DbList = GetList(_sql, args);

                //string s_log1 = "";
                //s_log1 += string.Format("\n ##GetLog \n _sql:{0}", _sql);
                //s_log1 += string.Format("\n ##GetLog \n args.ToCount:{0}\n", args.ToCount());
                //foreach (var x in args)
                //{
                //    s_log1 += string.Format("\n {0}='{1}'", x.Key, x.Value);
                //}
                //logger.Debug(s_log1);

                var DbModGroup = DbList.GroupBy(m => m["MODTIME"]).Select(m => m.ToList()).ToList();


                foreach (var item in DbModGroup)
                {
                    var FLOW_CD = item.FirstOrDefault()["FLOW_CD"].TONotNullString();
                    if (FLOW_CD != "")
                    {
                        LogModel log = new LogModel();
                        log.CODE_DESC = item.FirstOrDefault()["CODE_DESC"].TONotNullString();
                        log.MODTIME = item.FirstOrDefault()["MODTIME"].TONotNullString();
                        if (FLOW_CD == "3")
                        {
                            log.DIFFTABLE = new List<Hashtable>();
                        }
                        result.Add(log);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        //GetTransLog
        public IList<TransLogModel> GetTransLog(string APP_ID)
        {
            //DataLayers\BackApplyDAO.cs SYS_TRANS_LOG
            //BaseAction.cs SYS_TRANS_LOG
            string cst_split1 = "<|s|>";
            string cst_right1 = "<|=|>";
            IList<TransLogModel> result = new List<TransLogModel>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                //conn = DataUtils.GetConnection();
                conn.Open();
                Dictionary<string, object> args1 = new Dictionary<string, object>();
                string s_Sql = @"
 SELECT SEQ , TRANSTIME
 , APP_ID    , SRV_ID
 , TRANSTYPE    , TARGETTABLE
 , CONDITIONS    , BEFOREVALUES    , AFTERVALUES
 , COLUMNAME    , COLUMNAMEC
 , UPD_TIME    , UPD_FUN_CD    , UPD_ACC
 , ADD_TIME    , ADD_FUN_CD    , ADD_ACC
 , MODTIME    , MODUSER    , MODTYPE 
 FROM SYS_TRANS_LOG 
 WHERE 1=1 AND APP_ID =@APP_ID 
 ORDER BY MODTIME,UPD_TIME";
                SqlCommand sCmd = new SqlCommand(s_Sql, conn);
                DataTable dt = new DataTable();
                sCmd.Parameters.Clear();
                sCmd.Parameters.Add("APP_ID", SqlDbType.VarChar).Value = APP_ID;
                dt.Load(sCmd.ExecuteReader());

                string s_log1 = "";
                int iRow = 0;
                string s_MODTIME = "";
                foreach (DataRow dr in dt.Rows)
                {
                    TransLogModel itme = new TransLogModel();
                    itme.MODTIME = dr["MODTIME"].ToString();
                    if (!s_MODTIME.Equals("") && !s_MODTIME.Equals(itme.MODTIME)) { iRow = 0; s_MODTIME = itme.MODTIME; } //更換
                    if (s_MODTIME.Equals("")) { s_MODTIME = itme.MODTIME; } //第1次

                    string _BEFOREVALUES = dr["BEFOREVALUES"].ToString();
                    string _AFTERVALUES = dr["AFTERVALUES"].ToString();
                    string _COLUMNAME = dr["COLUMNAME"].ToString();
                    string _COLUMNAMEC = dr["COLUMNAMEC"].ToString();
                    //_BEFOREVALUES = _BEFOREVALUES.Replace(", ", ",");
                    IList<string> bVal = _BEFOREVALUES.ToSplit(cst_split1);
                    IList<string> aVal = _AFTERVALUES.ToSplit(cst_split1);
                    IList<string> cVal = _COLUMNAMEC.ToSplit(cst_split1);
                    IList<string> ceVal = _COLUMNAME.ToSplit(cst_split1);
                    if (bVal.ToCount() == 0) { return result; } //logger.Warn("沒有東西!" + s_log1);
                    if (bVal.ToCount() != aVal.ToCount())
                    {
                        s_log1 = string.Format("/APP_ID:{0} ,b:{1},a:{2},c:{3}", APP_ID, bVal.ToCount(), aVal.ToCount(), cVal.ToCount());
                        logger.Warn("數量有誤1:" + s_log1); continue;
                        //return result;
                    } //數量有誤
                    if (bVal.ToCount() != cVal.ToCount())
                    {
                        s_log1 = string.Format("/APP_ID:{0} ,b:{1},a:{2},c:{3}", APP_ID, bVal.ToCount(), aVal.ToCount(), cVal.ToCount());
                        logger.Warn("數量有誤2:" + s_log1); continue;
                        //return result;
                    } //數量有誤
                    string s_DESC1 = "";
                    string _NoInput1 = "UPD_ACC,UPD_FUN_CD,UPD_TIME,APP_ID,Nullable`1"; //排除字串
                    string[] NoInput1 = _NoInput1.Split(',');
                    for (int ix = 0; ix < bVal.Count; ix++)
                    {
                        // 比對值的內容，不相等才show
                        IList<string> BVal = bVal[ix].ToSplit(cst_right1);
                        IList<string> AVal = aVal[ix].ToSplit(cst_right1);
                        bool flag_can_use1 = false; //防呆
                        if (BVal.ToCount() > 1 && AVal.ToCount() > 1) { flag_can_use1 = true; }
                        if (!flag_can_use1)
                        {
                            s_log1 = string.Format("/APP_ID:{0} ,BVal:{1},AVal:{2} ", APP_ID, BVal.ToCount(), AVal.ToCount());
                            logger.Warn("數量有誤3:" + s_log1);
                        }
                        if (flag_can_use1 && !BVal[1].Equals(AVal[1]))
                        {
                            //比對key的內容，排除不要的key
                            var xKEY1 = (cVal.Count > ix ? cVal[ix] : BVal[0]);
                            if (NoInput1.Contains(xKEY1)) { continue; }
                            var xKEY2 = (ceVal.Count > ix ? ceVal[ix] : BVal[0]);

                            //組合字串中
                            iRow += 1;
                            s_DESC1 += string.Format("({0}){1}", iRow, xKEY1);
                            s_DESC1 += string.Format(":{0}=>{1}　", BVal[1], AVal[1]);
                        }

                    }
                    itme.DESC1 = s_DESC1;
                    result.Add(itme);
                }
                conn.Close();
                conn.Dispose();
            }
            //conn.Dispose();
            return result;
        }

        //GetNoticeLog
        public IList<TransLogModel> GetNoticeLog(string APP_ID)
        {
            //DataLayers\BackApplyDAO.cs SYS_TRANS_LOG
            //BaseAction.cs SYS_TRANS_LOG
            string cst_split1 = "<|s|>";
            string cst_right1 = "<|=|>";
            IList<TransLogModel> result = new List<TransLogModel>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                //conn = DataUtils.GetConnection();
                conn.Open();
                Dictionary<string, object> args1 = new Dictionary<string, object>();
                string s_Sql = @"
WITH WC1 AS (
 SELECT DISTINCT APP_ID,MODTIME
 FROM APPLY_NOTICE_LOG 
 WHERE APP_ID =@APP_ID 
)
,WC2 AS (
SELECT MODTIME,dbo.FN_GET_NOTICE_LOG(APP_ID,MODTIME) NOTICE_LOG
FROM WC1
)
SELECT MODTIME,NOTICE_LOG DESC1
FROM WC2
WHERE NOTICE_LOG IS NOT NULL
ORDER BY MODTIME ";
                SqlCommand sCmd = new SqlCommand(s_Sql, conn);
                DataTable dt = new DataTable();
                sCmd.Parameters.Clear();
                sCmd.Parameters.Add("APP_ID", SqlDbType.VarChar).Value = APP_ID;
                dt.Load(sCmd.ExecuteReader());

                foreach (DataRow dr in dt.Rows)
                {
                    TransLogModel itme = new TransLogModel();
                    itme.MODTIME = dr["MODTIME"].ToString();
                    itme.DESC1 = dr["DESC1"].ToString();
                    result.Add(itme);
                }
                conn.Close();
                conn.Dispose();
            }
            //conn.Dispose();
            return result;
        }


        //IList<TransLogModel> NoticeLog = dao.GetNoticeLog(APP_ID);
        #endregion

        #region 最近使用功能

        public List<VISIT_RECORDModel> GetVidsitRecord(string accNo)
        {
            List<VISIT_RECORDModel> result = new List<VISIT_RECORDModel>();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql =
                    @"SELECT TAG,CONTROL_NAME,ACTION_NAME,APP_NAME,ACC_NO,IS_EXPIRED
                       FROM VISIT_RECORD ";
                _sql += " WHERE ACC_NO= @ACC_NO ";
                _sql += " ORDER BY TAG DESC";

                var dictionary = new Dictionary<string, object> { { "@ACC_NO", accNo } };
                var parameters = new DynamicParameters(dictionary);

                try
                {

                    result = conn.Query<VISIT_RECORDModel>(_sql, parameters).ToList();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public void VisitRecordDelete(VISIT_RECORDModel record)
        {
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    VISIT_RECORDModel where = new VISIT_RECORDModel();
                    where.TAG = record.TAG;
                    where.ACC_NO = record.ACC_NO;

                    this.Delete(where);

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
        public void VisitRecordUpdate(List<VISIT_RECORDModel> logList, string accNo)
        {

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    VISIT_RECORDModel where = new VISIT_RECORDModel();
                    where.ACC_NO = accNo;
                    this.Delete(where);

                    foreach (VISIT_RECORDModel log in logList)
                    {
                        log.IS_EXPIRED = true;
                        this.Insert(log);
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }


        #endregion

        #region 公文取號

        public DataTable QueryOfficial(string app_id, string MOHW_CASE_NO)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select MOHW_CASE_NO, CONVERT(varchar,INSERTDATE,111) MOHW_CASE_DATE
                                    from OFFICIAl_DOC
                                    where APP_ID='" + app_id + "' AND MOHW_CASE_NO='" + MOHW_CASE_NO + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        #endregion

        #region FLOW_CD NOTE

        public string GetNote(Dictionary<string, string> contents)
        {
            string result = "";
            foreach (var item in contents)
            {
                result += "<div class=\"form-group\">";
                result += "<label class=\"step-label col-sm-2\" for=\"\">" + item.Key + "</label>";
                result += "<div class=\"col-sm-10\">";

                result += "<p class=\"form-control-static\">" + item.Value + "</p>";
                result += "</div>";
                result += "</div>";
            }

            return result;

        }

        #endregion

        #region Main

        /// <summary>
        /// 取得最新公告
        /// </summary>
        /// <returns></returns>
        public MainModel GetLatestMessages()
        {
            var result = new MainModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {

                    string _sql = @"SELECT * FROM (SELECT TOP 10 MSG_ID, UNIT_CD, CATEGORY, TITLE, CONTENT, TIME_S, TIME_E, FILENAME_1, FILENAME_2, FILENAME_3, SEND_MAIL_MK, SEND_MAIL_TIME, 
                       CLS_SUB_CD, CLS_ADM_CD, CLS_SRV_CD, KEYWORD, SEQ_NO, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC, MESSAGE_TYPE 
                       FROM MESSAGEBACK 
                       WHERE DEL_MK = 'N'
                       AND TIME_S<=GETDATE()
	                   AND TIME_E>=GETDATE()	
                       ORDER BY　TIME_S DESC) as T
                       ORDER By TIME_S DESC";
                    result.LatestMessages = conn.Query<TblMESSAGEBack>(_sql, null).ToList();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public string GetBarChartData(string unit_cd)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    string _sql = "";
                    string year = DateTime.Now.Year.ToString();

                    #region SQL

                    _sql += " SELECT '一月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "01' ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '二月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "02' ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '三月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "03'  ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '四月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "04'  ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '五月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "05'  ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '六月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "06'  ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '七月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "07'  ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '八月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "08' ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '九月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "09' ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '十月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "10' ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '十一月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "11' ";
                    _sql += " AND DEL_MK = 'N' ";

                    _sql += " UNION ";

                    _sql += " SELECT '十二月' STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";
                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }
                    _sql += " AND SUBSTRING(APP_ID,1,6)= '" + year + "12' ";
                    _sql += " AND DEL_MK = 'N' ";

                    #endregion

                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return JsonConvert.SerializeObject(result);
        }

        public string GetPieChartData(string unit_cd)
        {

            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    string _sql = "";
                    string year = DateTime.Now.Year.ToString();

                    #region SQL

                    _sql += " SELECT SERVICE_NAME, CASE_COUNT, SERVICE_STATUS ";
                    _sql += " FROM( ";
                    _sql += " SELECT TOP 100 A.SRV_ID, A.NAME AS SERVICE_NAME, COUNT(B.SRV_ID) CASE_COUNT, 'true' SERVICE_STATUS ";
                    _sql += " FROM SERVICE AS A ";
                    if (unit_cd != "31")
                    {
                        _sql += " LEFT JOIN APPLY AS B ON(A.SRV_ID = B.SRV_ID) AND UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') AND SUBSTRING(APP_ID,1,4)= '" + year + "' AND B.DEL_MK = 'N' ";
                        _sql += " WHERE A.DEL_MK = 'N' ";
                        _sql += " AND A.SC_PID in (SELECT DISTINCT SC_ID FROM SERVICE_CATE WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "'))  ";

                    }
                    else
                    {
                        _sql += " LEFT JOIN APPLY AS B ON(A.SRV_ID = B.SRV_ID) AND  SUBSTRING(APP_ID,1,4)= '" + year + "' AND B.DEL_MK = 'N' ";
                        _sql += " WHERE A.DEL_MK = 'N' ";

                    }
                    _sql += " AND A.ONLINE_N_MK = 'Y' ";
                    _sql += " GROUP BY A.SRV_ID,A.NAME ";
                    _sql += " ORDER BY A.SRV_ID ";
                    _sql += " ) T ";

                    #endregion

                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return JsonConvert.SerializeObject(result);

        }

        public DataTable GetCaseList(string unit_cd, string star_date, string end_date)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    string _sql = "";
                    string year = DateTime.Now.Year.ToString();

                    #region SQL

                    _sql += " SELECT CONVERT(VARCHAR, (CONVERT(INT, SUBSTRING(APP_ID,1,4))-1911)) +'/' + SUBSTRING(APP_ID, 5, 2) STAT_MONTH,COUNT(*) CASE_COUNT ";
                    _sql += " FROM APPLY ";

                    if (unit_cd != "31")
                    {
                        _sql += " WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = '" + unit_cd + "') ";
                    }
                    else
                    {
                        _sql += " WHERE 1=1 ";
                    }

                    _sql += " AND SUBSTRING(APP_ID,1,8) BETWEEN '" + star_date + "' AND '" + end_date + "' ";
                    _sql += " AND DEL_MK = 'N' ";
                    _sql += " GROUP BY CONVERT(VARCHAR, (CONVERT(INT, SUBSTRING(APP_ID, 1, 4)) - 1911)) + '/' + SUBSTRING(APP_ID, 5, 2) ";
                    _sql += " ORDER BY STAT_MONTH ";

                    #endregion

                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public DataTable GetCaseDetailList(string unit_cd, string apply_ym)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    string _sql = "";
                    string year = DateTime.Now.Year.ToString();

                    #region SQL

                    _sql += " SELECT* FROM( ";
                    _sql += "             SELECT A.SRV_ID, A.UNIT_CD, UPPER(IDN) PID, A.NAME APPLY_NAME, B.NAME SERVICE_NAME, APP_TIME, ";
                    _sql += "                   (SELECT DISTINCT CODE_DESC FROM CODE_CD WHERE CODE_KIND = 'F_CASE_STATUS' AND CODE_CD = A.FLOW_CD ";
                    _sql += "                   AND CODE_PCD IN (SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD) ";
                    _sql += "                   ) AS FLOW_CD_NAME ";
                    _sql += "             FROM APPLY A ";
                    _sql += "             JOIN SERVICE B ON A.SRV_ID = B.SRV_ID ";

                    if (unit_cd != "31")
                    {
                        _sql += " WHERE A.UNIT_CD = ( SELECT UNIT_CD FROM UNIT WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD='" + unit_cd + "')) AND A.DEL_MK = 'N' ";
                    }
                    else
                    {
                        _sql += " WHERE A.DEL_MK = 'N'  ";
                    }

                    _sql += " AND SUBSTRING(A.APP_ID,1,6)='" + apply_ym + "' ";
                    _sql += "  ) T ";
                    _sql += " WHERE FLOW_CD_NAME IS NOT NULL ";
                    _sql += "  ORDER BY SRV_ID,APP_TIME ";

                    #endregion

                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply011001

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011001FormModel QueryApply_011001(Apply_011001FormModel parm)
        {
            Apply_011001FormModel result = new Apply_011001FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql =
                    @"select app.SRV_ID ,(convert(varchar, app.APP_TIME, 111))as APP_TIME ,(convert(varchar, app.APP_EXT_DATE, 111)) as APP_EXT_DATE,
                                ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM,app.APP_ID,app.ACC_NO,app.TEL,app.ADDR_CODE,ADDR,a01.ADM_NAM,app.MOBILE as ADM_MOBILE,a01.ADM_MAIL,
                                a01.ACC_SDATE ,a01.ACC_NUM,a01.MERGEYN,app.NAME as ACC_NAM,app.FLOW_CD,app.MOHW_CASE_NO,
                                a01.ACC_NUM
                                from apply app
                                left join ADMIN ad on app.PRO_ACC = ad.ACC_NO
                                left join APPLY_011001 a01 on app.APP_ID = a01.APP_ID
                                where 1 = 1";
                _sql += "and app.app_id = '" + parm.APP_ID + "'";

                try
                {
                    result = conn.QueryFirst<Apply_011001FormModel>(_sql);
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011001FileModel GetFileList_011001(string APP_ID)
        {
            var result = new Apply_011001FileModel();
            ShareDAO dao = new ShareDAO();
            result.FILENAM = dao.GetFileGridList(APP_ID);
            result.APP_ID = APP_ID;

            return result;
        }

        /// <summary>
        /// 檢核存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011001(Apply_011001FormModel model)
        {
            string Msg = "";

            if (model.FLOW_CD == "2" && model.FileCheck.TONotNullString() == "")
            {
                Msg = "請至少選擇一種補件項目 !";
            }
            if (model.FLOW_CD == "2")
            {
                if (model.NOTE.TONotNullString() == "" && model.FileCheck.TONotNullString() != "")
                {
                    Msg = "請填寫補件內容 !";
                }
            }
            if (string.IsNullOrEmpty(model.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
            }

            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011001(Apply_011001FormModel model)
        {
            string mainproject = "";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "011001");
            dict2.Add("LastMODTIME", LastMODTIME);
            SessionModel sm = SessionModel.Get();
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.FLOW_CD == "2" || model.FLOW_CD == "4")
                    {
                        #region 補件內容

                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();


                        if (!string.IsNullOrEmpty(model.FileCheck))
                        {

                            var needchk = model.FileCheck.ToSplit(',');
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";
                            foreach (var item in needchk)
                            {
                                var Field_NAME = "";
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 1:
                                        Field_NAME = "志願服務運用計畫";
                                        mainproject += mainproject == "" ? "志願服務運用計畫" : "、志願服務運用計畫";
                                        anwhere.Field = "FILE_" + "1";
                                        break;
                                    case 2:
                                        Field_NAME = "運用單位組織章程";
                                        mainproject += mainproject == "" ? "運用單位組織章程" : "、運用單位組織章程";
                                        anwhere.Field = "FILE_" + "2";
                                        break;
                                    case 3:
                                        Field_NAME = "單位立案登記證書影本";
                                        mainproject += mainproject == "" ? "單位立案登記證書影本" : "、單位立案登記證書影本";
                                        anwhere.Field = "FILE_" + "3";
                                        break;
                                    case 4:
                                        Field_NAME = "志工基本資料清冊";
                                        mainproject += mainproject == "" ? "志工基本資料清冊" : "、志工基本資料清冊";
                                        anwhere.Field = "FILE_" + "4";
                                        break;
                                    case 5:
                                        Field_NAME = "全部";
                                        mainproject += mainproject == "" ? "全部" : "、全部";
                                        anwhere.Field = "ALL_" + "5";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = model.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                if (model.FLOW_CD == "2")
                                {
                                    Insert(anwhere);
                                }

                                count++;
                                savestatus = true;
                            }
                            MainBody += mainproject;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                        }
                        #endregion
                    }
                    // 更新案件狀態
                    ApplyModel appwhere = new ApplyModel();
                    appwhere.APP_ID = model.APP_ID;

                    ApplyModel appdata = new ApplyModel();
                    appdata.InjectFrom(model);
                    appdata.MOHW_CASE_NO = model.MOHW_CASE_NO;
                    appdata.FLOW_CD = model.FLOW_CD;
                    appdata.UPD_TIME = DateTime.Now;
                    appdata.UPD_ACC = sm.UserInfo.UserNo;
                    appdata.UPD_FUN_CD = "ADM-STORE";

                    if ((model.FLOW_CD == "2" || model.FLOW_CD == "4") && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else
                    {
                        //Update(appdata, appwhere);
                        base.Update2(appdata, appwhere, dict2, true);
                        string MailBody = "";
                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            switch (model.FLOW_CD)
                            {
                                case "2":
                                    SendMail_Notice(MainBody, model.ACC_NAM, count, model.ADM_MAIL, model.APP_ID, "志願服務計畫核備", "011001");
                                    break;
                                // 補正確認完成
                                case "4":
                                    MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                    MailBody += " <tr><th align=\"left\">" + model.ACC_NAM + "，您好:</th></tr>";
                                    MailBody += " <tr><td>您所提交的志願服務計畫核備申請，已完成資料補件共" + count.ToString() + "件（包括" + mainproject + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                    MailBody += " <tr><td align=\"right\">衛生福利部</td></tr></table>";
                                    SendMail(model.ADM_MAIL, $"志願服務計畫核備，案件編號{model.APP_ID}狀態通知", MailBody, "011001");
                                    break;
                                default:
                                    break;
                            }

                        }
                        switch (model.FLOW_CD)
                        {
                            case "0":
                                SendMail_Success(model.ACC_NAM, "志願服務計畫核備", "011001", model.ADM_MAIL, model.APP_TIME, model.APP_ID, "");
                                break;
                            case "5":
                                SendMail_InPorcess(model.ACC_NAM, "志願服務計畫核備", "011001", model.ADM_MAIL, model.APP_TIME, model.APP_ID, "");
                                break;
                            case "9":
                                SendMail_Expired(model.ACC_NAM, "志願服務計畫核備", "011001", model.ADM_MAIL, model.APP_TIME, model.APP_ID, model.NOTE);
                                break;
                            default:
                                break;
                        }
                        tran.Commit();
                    }

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_011001FormModel GetApplyNotice_011001(string app_id)
        {
            Apply_011001FormModel result = new Apply_011001FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011001FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply001005 醫事人員證書補(換)發

        /// <summary>
        /// 取得醫事人員證書補(換)發"
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001005ViewModel QueryApply_001005(Apply_001005ViewModel parm)
        {
            Apply_001005ViewModel result = new Apply_001005ViewModel();
            result.Apply = new ApplyModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {

                    string _sql = @"SELECT APP_ID,ACTION_TYPE,ISSUE_DEPT,ISSUE_DATE,LIC_TYPE,LIC_CD,LIC_NUM,ACTION_RES,OTHER_RES,DEL_MK,DEL_TIME,DEL_FUN_CD,
		                                    DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,EMAIL,DIVISION,MAIL_DATE,MAIL_BARCODE
                             FROM APPLY_001005
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001005ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        // 查詢承辦人姓名 20210513 改寫
                        AdminModel where_admin = new AdminModel();
                        where_admin.ACC_NO = proAcc.TONotNullString();
                        result.Admin = GetRow(where_admin);
                        if (string.IsNullOrEmpty(result.Admin.NAME))
                        {
                            result.Admin.NAME = proAcc.TONotNullString();
                        }
                        //dictionary = new Dictionary<string, object> { { "@ACC_NO", proAcc } };
                        //parameters = new DynamicParameters(dictionary);

                        //_sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                        //        DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        //        FROM ADMIN
                        //    WHERE 1=1";
                        //_sql += " AND ACC_NO = @ACC_NO";
                        //result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }
                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);
                    _sql = @" SELECT APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_ACT_TIME, PAY_EXT_TIME, PAY_EXT_TIME AS PAY_EXT_TIME_AC2, PAY_INC_TIME, PAY_METHOD, PAY_STATUS_MK, PAY_RET_CD,
                            PAY_RET_MSG, BATCH_NO, APPROVAL_CD, PAY_RET_NO, INVOICE_NO, PAY_DESC, CARD_NO, HOST_TIME, TRANS_RET, SESSION_KEY, AUTH_DATE,
                            AUTH_NO, SETTLE_DATE, OTHER, ROC_ID, CLIENT_IP, OID, SID, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            FROM APPLY_PAY
                            WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.ApplyPay = conn.QueryFirst<APPLY_PAY>(_sql, parameters);

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.File = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>().FirstOrDefault();

                    _sql = @"SELECT APP_ID, Field, ISADDYN, FREQUENCY, ADD_TIME, DEADLINE, NOTE, SRC_NO, BATCH_INDEX
                             FROM APPLY_NOTICE AS A
                             WHERE ISADDYN='Y' AND FREQUENCY = (SELECT MAX(FREQUENCY) FROM APPLY_NOTICE WHERE APP_ID=A.APP_ID) ";
                    _sql += " AND APP_ID = @APP_ID";

                    result.Notices = conn.Query<TblAPPLY_NOTICE>(_sql, parameters).ToList<TblAPPLY_NOTICE>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);

                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);

                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);

                //核發日期
                result.ISSUE_DATE_AC = HelperUtil.DateTimeToString(result.ISSUE_DATE);

                //郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                result.PAY_MONEY = result.ApplyPay.PAY_MONEY;

                result.Note = result.Apply.NOTICE_NOTE;

                // 電話
                if (result.Apply.TEL.TONotNullString() != "")
                {
                    string[] tel = result.Apply.TEL.Split('-');
                    if (result.Apply.TEL.TONotNullString().Trim() != "" && tel.ToCount() > 1)
                    {
                        result.TEL_SEC = tel[0];
                        if (tel.ToCount() > 1)
                        {
                            result.TEL_NO = tel[1];

                            if (result.TEL_NO.Contains("#"))
                            {
                                result.TEL_NO = result.TEL_NO.Split('#')[0];
                            }

                            if (result.Apply.TEL.IndexOf('#') > 0)
                            {
                                result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                            }
                        }
                    }
                }

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }

                result.PAY_ACT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_ACT_TIME).TONotNullString();
                result.PAY_EXT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_EXT_TIME).TONotNullString();
                result.PAY_INC_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_INC_TIME).TONotNullString();

                if (result.ApplyPay.PAY_STATUS_MK == "Y")
                {
                    result.IsPay = true;
                }
                else
                {
                    result.IsPay = false;
                }

                switch (result.ApplyPay.PAY_METHOD)
                {
                    case "C":
                        result.PAY_METHOD_NAME = "信用卡";
                        break;
                    case "D":
                        result.PAY_METHOD_NAME = "匯票";
                        break;
                    case "T":
                        result.PAY_METHOD_NAME = "劃撥";
                        break;
                    case "B":
                        result.PAY_METHOD_NAME = "臨櫃";
                        break;
                    case "S":
                        result.PAY_METHOD_NAME = "超商";
                        break;
                    default:
                        result.PAY_METHOD_NAME = "";
                        break;
                }

                ShareDAO shareDAO = new ShareDAO();
                if (shareDAO.CalculationDocDate("001005", parm.APP_ID) && result.Apply.FLOW_CD == "2")
                {
                    result.IsNotice = "N";
                }
                else
                {
                    result.IsNotice = "Y";
                }
            }

            return result;
        }

        /// <summary>
        /// 取得醫事人員證書補（換）發存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001005(Apply_001005ViewModel model)
        {
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001005");
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001005Model apply001005 = new Apply_001005Model();
            apply001005.APP_ID = model.APP_ID;

            if (model.Apply.FLOW_CD == "12")
            {
                apply001005 = this.GetRow(apply001005);
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.Apply.ISMODIFY.TONotNullString() != "")
                    {
                        #region 補件內容
                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        if (model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "其他" + "</p>";
                        }
                        else if (model.Apply.ISMODIFY.TONotNullString() == "Z")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "繳費紀錄照片或pdf檔案、其他" + "</p>";
                        }
                        else
                        {

                            MainBody += "<p class=\"form-control-static\">" + "繳費紀錄照片或pdf檔案" + "</p>";
                        }

                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Note + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        savestatus = true;
                        #endregion
                    }
                    // 9:逾期未補件而予結案// 15:自請撤銷// 8:退件通知
                    if (model.Apply.FLOW_CD == "9" || model.Apply.FLOW_CD == "15" || model.Apply.FLOW_CD == "8")
                    {
                        noteContent.Add("備註", model.Note.TONotNullString());
                        MainBody = GetNote(noteContent);
                    }
                    // 12:核可(發文歸檔)
                    if (model.Apply.FLOW_CD == "12")
                    {
                        noteContent.Add("郵寄日期", model.MAIL_DATE_AC);
                        noteContent.Add("掛號條碼", model.MAIL_BARCODE);
                        MainBody = GetNote(noteContent);
                    }

                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.Apply.FLOW_CD == "Z" ? "Y" : model.Apply.FLOW_CD;
                    apply.NOTICE_NOTE = model.Note.TONotNullString();

                    if (apply.FLOW_CD == "2")
                    {
                        apply.APPLY_NOTICE_DATE = DateTime.Now;
                        apply.ISMODIFY = model.Apply.ISMODIFY;

                        TblAPPLY_NOTICE applyNotice = null;
                        TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                        where.APP_ID = model.APP_ID;
                        var noticeList = GetRowList(where);

                        var newNoticeList = from notice in noticeList
                                            orderby notice.FREQUENCY descending
                                            select notice;

                        var times = newNoticeList.ToCount() == 0 ? 0 : newNoticeList.FirstOrDefault().FREQUENCY.TOInt32();
                        applyNotice = new TblAPPLY_NOTICE();
                        applyNotice.ADD_TIME = DateTime.Now;
                        applyNotice.APP_ID = model.APP_ID;
                        applyNotice.ISADDYN = "N";
                        applyNotice.Field = "FILE1";
                        applyNotice.FREQUENCY = times + 1;
                        applyNotice.NOTE = "001005補件";
                        applyNotice.Field_NAME = "FILE1";
                        Insert(applyNotice);

                    }
                    else
                    {
                        apply.ISMODIFY = "A";
                    }

                    if (model.IsPay)
                    {
                        apply.PAY_A_PAID = model.PAY_MONEY;
                    }

                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.ISMODIFY = model.Apply.ISMODIFY.TONotNullString().Equals("Z") ? "Y" : model.Apply.ISMODIFY.TONotNullString();
                    apply.NOTIBODY = model.Apply.ISMODIFY.TONotNullString() != "" ? model.Note : "";
                    apply.MAILBODY = MainBody;
                    //this.Update(apply, whereApply);
                    base.Update2(apply, whereApply, dict2, true);

                    if (model.Apply.FLOW_CD == "12")
                    {
                        Apply_001005Model whereApply001005 = new Apply_001005Model();
                        whereApply001005.APP_ID = model.APP_ID;
                        Apply_001005Model apply001005Model = new Apply_001005Model();

                        apply001005Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AC);
                        apply001005Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001005Model.UPD_TIME = DateTime.Now;
                        apply001005Model.UPD_FUN_CD = "ADM-STORE";
                        apply001005Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001005Model.DEL_MK = "N";
                        //this.Update(apply001005Model, whereApply001005);
                        base.Update2(apply001005Model, whereApply001005, dict2, true);
                    }
                    #endregion

                    #region 繳費資訊
                    if (model.IsPay)
                    {
                        APPLY_PAY applyPayWhere = new APPLY_PAY();
                        applyPayWhere.APP_ID = model.APP_ID;
                        applyPayWhere.PAY_STATUS_MK = "N";

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = model.APP_ID;
                        applyPay.PAY_STATUS_MK = model.IsPay ? "Y" : "N";
                        applyPay.PAY_MONEY = model.ApplyPay != null ? model.ApplyPay.PAY_MONEY : model.PAY_MONEY;
                        applyPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.PAY_EXT_TIME_AC);
                        applyPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.PAY_INC_TIME_AC);
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = "ADM-STORE";
                        applyPay.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        //this.Update(applyPay, applyPayWhere);
                        base.Update2(applyPay, applyPayWhere, dict2, true);
                    }
                    #endregion

                    // 判斷是否要寄信
                    #region 依據狀態寄發信件內容
                    if (model.Apply.FLOW_CD == "--")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001005Model apply001005Model = new Apply_001005Model();
                        apply001005Model.APP_ID = model.APP_ID;
                        apply001005Model = this.GetRow(apply001005Model);

                        SendMail_InPorcess(applyModel.NAME, "醫事人員或公共衛生師證書補(換)發", "001005", apply001005Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "2")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001005Model apply001005Model = new Apply_001005Model();
                        apply001005Model.APP_ID = model.APP_ID;
                        apply001005Model = this.GetRow(apply001005Model);
                        string note = "";

                        if (model.Apply.ISMODIFY == "Y")
                        {
                            note = "補件項目﹕其他<br/>";
                        }
                        else if (model.Apply.ISMODIFY == "Z")
                        {
                            note = "補件項目﹕繳費紀錄照片或pdf檔案、其他<br/>";
                        }
                        else
                        {
                            note = "補件項目﹕繳費紀錄照片或pdf檔案<br/>";
                        }

                        note += "補件備註﹕" + model.Note;

                        SendMail_Notice(applyModel.NAME, "醫事人員或公共衛生師證書補(換)發", "001005", apply001005Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, note);
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001005Model apply001005Model = new Apply_001005Model();
                        apply001005Model.APP_ID = model.APP_ID;
                        apply001005Model = this.GetRow(apply001005Model);

                        SendMail_Archive(applyModel.NAME, "醫事人員或公共衛生師證書補(換)發", "001005", apply001005Model.EMAIL,
                                        applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                        (HelperUtil.TransToDateTime(model.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                        model.MAIL_BARCODE);
                        savestatus = false;

                    }

                    if (model.Apply.FLOW_CD == "15")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001005Model apply001005Model = new Apply_001005Model();
                        apply001005Model.APP_ID = model.APP_ID;
                        apply001005Model = this.GetRow(apply001005Model);

                        SendMail_Cancel(applyModel.NAME, "醫事人員或公共衛生師證書補(換)發", "001005", apply001005Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                    }

                    if (model.Apply.FLOW_CD == "9")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001005Model apply001005Model = new Apply_001005Model();
                        apply001005Model.APP_ID = model.APP_ID;
                        apply001005Model = this.GetRow(apply001005Model);

                        SendMail_Expired(applyModel.NAME, "醫事人員或公共衛生師證書補(換)發", "001005", apply001005Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note);
                        savestatus = false;

                    }

                    //if (savestatus)
                    //{
                    //    SendMail_Notice(MainBody, model.APPLY_NAME, -1, model.EMAIL, model.APP_ID, "醫事人員證書補(換)發", "001005");
                    //}
                    #endregion

                    tran.Commit();

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }


        public byte[] PrintPdf001005(string id)
        {
            byte[] result = null;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                Form001005Action action = new Form001005Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                result = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        #endregion

        #region Apply001007 專科醫師證書補（換）發

        /// <summary>
        /// 取得專科醫師證書補（換）發
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001007ViewModel QueryApply_001007(Apply_001007ViewModel parm)
        {
            Apply_001007ViewModel result = new Apply_001007ViewModel();
            result.Apply = new ApplyModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {

                    string _sql = @"SELECT APP_ID,ACTION_TYPE,ISSUE_DATE,LIC_TYPE,LIC_CD,LIC_NUM,ACTION_RES,OTHER_RES,DEL_MK,
                             DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,DIVISION,EMAIL,MAIL_DATE,MAIL_BARCODE
                             FROM APPLY_001007
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001007ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        // 查詢承辦人姓名 20210513 改寫
                        AdminModel where_admin = new AdminModel();
                        where_admin.ACC_NO = proAcc.TONotNullString();
                        result.Admin = GetRow(where_admin);
                        if (string.IsNullOrEmpty(result.Admin.NAME))
                        {
                            result.Admin.NAME = proAcc.TONotNullString();
                        }
                        //dictionary = new Dictionary<string, object>
                        //{
                        //    { "@ACC_NO", proAcc }
                        //};
                        //parameters = new DynamicParameters(dictionary);

                        //_sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                        //        DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        //        FROM ADMIN
                        //    WHERE 1=1";
                        //_sql += " AND ACC_NO = @ACC_NO";
                        //result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }
                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);

                    _sql = @" SELECT APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_ACT_TIME, PAY_EXT_TIME, PAY_INC_TIME, PAY_METHOD, PAY_STATUS_MK, PAY_RET_CD,
                            PAY_RET_MSG, BATCH_NO, APPROVAL_CD, PAY_RET_NO, INVOICE_NO, PAY_DESC, CARD_NO, HOST_TIME, TRANS_RET, SESSION_KEY, AUTH_DATE,
                            AUTH_NO, SETTLE_DATE, OTHER, ROC_ID, CLIENT_IP, OID, SID, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            FROM APPLY_PAY
                            WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.ApplyPay = conn.QueryFirst<APPLY_PAY>(_sql, parameters);

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.File = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>().FirstOrDefault();

                    _sql = @"SELECT APP_ID, Field, ISADDYN, FREQUENCY, ADD_TIME, DEADLINE, NOTE, SRC_NO, BATCH_INDEX
                             FROM APPLY_NOTICE AS A
                             WHERE ISADDYN='Y' AND FREQUENCY = (SELECT MAX(FREQUENCY) FROM APPLY_NOTICE WHERE APP_ID=A.APP_ID) ";
                    _sql += " AND APP_ID = @APP_ID";

                    result.Notices = conn.Query<TblAPPLY_NOTICE>(_sql, parameters).ToList<TblAPPLY_NOTICE>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);

                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);

                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);

                //核發日期
                result.ISSUE_DATE_AC = HelperUtil.DateTimeToString(result.ISSUE_DATE);

                //郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                result.Note = result.Apply.NOTICE_NOTE;

                // 電話

                if (result.Apply.TEL.TONotNullString().Trim() != "")
                {
                    string[] tel = result.Apply.TEL.Split('-');
                    result.TEL_SEC = tel[0];
                    if (tel.ToCount() > 1)
                    {
                        result.TEL_NO = tel[1];

                        if (result.TEL_NO.Contains("#"))
                        {
                            result.TEL_NO = result.TEL_NO.Split('#')[0];
                        }

                        if (result.Apply.TEL.IndexOf('#') > 0)
                        {
                            result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                        }
                    }
                }

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }

                result.PAY_ACT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_ACT_TIME).TONotNullString();
                result.PAY_EXT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_EXT_TIME).TONotNullString();
                result.PAY_INC_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_INC_TIME).TONotNullString();

                if (result.ApplyPay.PAY_STATUS_MK == "Y")
                {
                    result.IsPay = true;
                }
                else
                {
                    result.IsPay = false;
                }

                switch (result.ApplyPay.PAY_METHOD)
                {
                    case "C":
                        result.PAY_METHOD_NAME = "信用卡";
                        break;
                    case "D":
                        result.PAY_METHOD_NAME = "匯票";
                        break;
                    case "T":
                        result.PAY_METHOD_NAME = "劃撥";
                        break;
                    case "B":
                        result.PAY_METHOD_NAME = "臨櫃";
                        break;
                    case "S":
                        result.PAY_METHOD_NAME = "超商";
                        break;
                    default:
                        result.PAY_METHOD_NAME = "";
                        break;
                }

                ShareDAO shareDAO = new ShareDAO();
                if (shareDAO.CalculationDocDate("001007", parm.APP_ID) && result.Apply.FLOW_CD == "2")
                {
                    result.IsNotice = "N";
                }
                else
                {
                    result.IsNotice = "Y";
                }

                result.Note = result.Apply.NOTICE_NOTE;
            }

            return result;
        }

        /// <summary>
        /// 取得專科醫師證書補（換）發存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001007(Apply_001007ViewModel model)
        {
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001007");
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001007Model apply001007 = new Apply_001007Model();
            apply001007.APP_ID = model.APP_ID;

            apply001007 = this.GetRow(apply001007);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.Apply.ISMODIFY.TONotNullString() != "")
                    {
                        #region 補件內容
                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        if (model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "其他" + "</p>";
                        }
                        else if (model.Apply.ISMODIFY.TONotNullString() == "N")
                        {

                            MainBody += "<p class=\"form-control-static\">" + "繳費紀錄照片或pdf檔案" + "</p>";
                        }
                        else
                        {
                            MainBody += "<p class=\"form-control-static\">" + "繳費紀錄照片或pdf檔案、其他" + "</p>";
                        }

                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Note + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        savestatus = true;
                        #endregion
                    }
                    // 9:逾期未補件而予結案// 15:自請撤銷 // 8:退件通知
                    if (model.Apply.FLOW_CD == "9" || model.Apply.FLOW_CD == "15" || model.Apply.FLOW_CD == "8")
                    {
                        noteContent.Add("備註", model.Note.TONotNullString());
                        MainBody = GetNote(noteContent);
                    }
                    // 12:核可(發文歸檔)
                    if (model.Apply.FLOW_CD == "12")
                    {
                        noteContent.Add("郵寄日期", model.MAIL_DATE_AC);
                        noteContent.Add("掛號條碼", model.MAIL_BARCODE);
                        MainBody = GetNote(noteContent);
                    }

                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.Apply.FLOW_CD;
                    apply.NOTICE_NOTE = model.Note;

                    if (apply.FLOW_CD == "2")
                    {
                        apply.APPLY_NOTICE_DATE = DateTime.Now;
                        apply.ISMODIFY = model.Apply.ISMODIFY;

                        TblAPPLY_NOTICE applyNotice = null;
                        TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                        where.APP_ID = model.APP_ID;
                        var noticeList = GetRowList(where);

                        var newNoticeList = from notice in noticeList
                                            orderby notice.FREQUENCY descending
                                            select notice;

                        var times = newNoticeList.ToCount() == 0 ? 0 : newNoticeList.FirstOrDefault().FREQUENCY.TOInt32();
                        applyNotice = new TblAPPLY_NOTICE();
                        applyNotice.ADD_TIME = DateTime.Now;
                        applyNotice.APP_ID = model.APP_ID;
                        applyNotice.ISADDYN = "N";
                        applyNotice.Field = "FILE1";
                        applyNotice.FREQUENCY = times + 1;
                        applyNotice.NOTE = "001007補件";
                        applyNotice.Field_NAME = "FILE1";
                        Insert(applyNotice);

                    }
                    else if (apply.FLOW_CD == "9")
                    {
                        apply.NOTICE_NOTE = model.Note;
                    }
                    else
                    {
                        apply.ISMODIFY = "A";
                    }

                    if (model.IsPay)
                    {
                        apply.PAY_A_PAID = model.ApplyPay != null ? model.ApplyPay.PAY_MONEY : model.PAY_MONEY;
                    }

                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.ISMODIFY = model.Apply.ISMODIFY.TONotNullString().Equals("Z") ? "Y" : model.Apply.ISMODIFY.TONotNullString();
                    apply.NOTIBODY = model.Apply.ISMODIFY.TONotNullString() != "" ? model.Note : "";
                    apply.MAILBODY = MainBody;
                    //this.Update(apply, whereApply);
                    base.Update2(apply, whereApply, dict2, true);

                    if (model.Apply.FLOW_CD == "12")
                    {
                        Apply_001007Model whereApply001007 = new Apply_001007Model();
                        whereApply001007.APP_ID = model.APP_ID;
                        Apply_001007Model apply001007Model = new Apply_001007Model();

                        apply001007Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AC);
                        apply001007Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001007Model.UPD_TIME = DateTime.Now;
                        apply001007Model.UPD_FUN_CD = "ADM-STORE";
                        apply001007Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001007Model.DEL_MK = "N";
                        //this.Update(apply001007Model, whereApply001007);
                        base.Update2(apply001007Model, whereApply001007, dict2, true);
                    }

                    #endregion

                    #region 繳費資訊
                    if (model.IsPay)
                    {
                        APPLY_PAY applyPayWhere = new APPLY_PAY();
                        applyPayWhere.APP_ID = model.APP_ID;
                        applyPayWhere.PAY_STATUS_MK = "N";

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = model.APP_ID;
                        applyPay.PAY_STATUS_MK = model.IsPay ? "Y" : "N";
                        applyPay.PAY_MONEY = model.ApplyPay != null ? model.ApplyPay.PAY_MONEY : model.PAY_MONEY;
                        applyPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.PAY_EXT_TIME_AC);
                        applyPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.PAY_INC_TIME_AC);
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = "ADM-STORE";
                        applyPay.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        //this.Update(applyPay, applyPayWhere);
                        base.Update2(applyPay, applyPayWhere, dict2, true);
                    }
                    #endregion
                    // 判斷是否要寄信
                    #region 依據狀態寄發信件內容
                    if (model.Apply.FLOW_CD == "--")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001007Model apply001007Model = new Apply_001007Model();
                        apply001007Model.APP_ID = model.APP_ID;
                        apply001007Model = this.GetRow(apply001007Model);

                        SendMail_InPorcess(applyModel.NAME, "專科醫師證書補（換）發", "001007", apply001007Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "2")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001007Model apply001007Model = new Apply_001007Model();
                        apply001007Model.APP_ID = model.APP_ID;
                        apply001007Model = this.GetRow(apply001007Model);
                        string note = "";

                        if (model.Apply.ISMODIFY == "Y")
                        {
                            note = "補件項目﹕其他<br/>";
                        }
                        else if (model.Apply.ISMODIFY == "Z")
                        {
                            note = "補件項目﹕繳費紀錄照片或pdf檔案、其他<br/>";
                        }
                        else
                        {
                            note = "補件項目﹕繳費紀錄照片或pdf檔案<br/>";
                        }

                        note += "補件備註﹕" + model.Note;

                        SendMail_Notice(applyModel.NAME, "專科醫師證書補（換）發", "001007", apply001007Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, note);
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "9")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001007Model apply001007Model = new Apply_001007Model();
                        apply001007Model.APP_ID = model.APP_ID;
                        apply001007Model = this.GetRow(apply001007Model);

                        SendMail_Expired(applyModel.NAME, "專科醫師證書補（換）發", "001007", apply001007Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note);
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001007Model apply001007Model = new Apply_001007Model();
                        apply001007Model.APP_ID = model.APP_ID;
                        apply001007Model = this.GetRow(apply001007Model);

                        SendMail_Archive(applyModel.NAME, "專科醫師證書補（換）發", "001007", apply001007Model.EMAIL,
                                        applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                        (HelperUtil.TransToDateTime(model.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                        model.MAIL_BARCODE);
                    }

                    if (model.Apply.FLOW_CD == "15")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001007Model apply001007Model = new Apply_001007Model();
                        apply001007Model.APP_ID = model.APP_ID;
                        apply001007Model = this.GetRow(apply001007Model);

                        SendMail_Cancel(applyModel.NAME, "專科醫師證書補（換）發", "001007", apply001007Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                    }

                    //if (savestatus)
                    //{
                    //    SendMail_Notice(MainBody, model.APPLY_NAME, -1, model.EMAIL, model.APP_ID, "專科醫師證書補（換）發", "001007");
                    //}
                    #endregion
                    tran.Commit();

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public byte[] PrintPdf(string id)
        {
            byte[] result = null;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                Form001007Action action = new Form001007Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                result = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        #endregion

        #region Apply001009 醫事人員資格英文求證

        /// <summary>
        /// 醫事人員資格英文求證
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001009ViewModel QueryApply_001009(Apply_001009ViewModel parm)
        {
            Apply_001009ViewModel result = new Apply_001009ViewModel();
            result.Admin = new AdminModel();
            result.Apply = new ApplyModel();

            List<Apply_FileModel> tempFileList = null;

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"SELECT APP_ID,APPLY_CAUSE,APPLY_CAUSE_TEXT,APPLY_CERT_CATE,LIC_TYPE,LIC_CD,LIC_NUM,CERT_APPROVED_DATE,C_SCHOOL_NAME,
	                                  E_SCHOOL_NAME,STUDY_START_YM,STUDY_END_YM,VERIFY_ADDRESS,IS_MERGE_FILE,EMAIL,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,
	                                  UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MAIL_COUNTRY,RECEIVER,MAIL_DATE,MAIL_BARCODE,IS_FILE1,IS_FILE2,IS_FILE3,IS_FILE4,IS_FILE5,VERIFY_NAME
                             FROM APPLY_001009
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001009ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE,MAILBODY,NOTIBODY,E_ALIAS_NAME
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        // 查詢承辦人姓名 20210513 改寫
                        AdminModel where_admin = new AdminModel();
                        where_admin.ACC_NO = proAcc.TONotNullString();
                        result.Admin = GetRow(where_admin);
                        if (string.IsNullOrEmpty(result.Admin.NAME))
                        {
                            result.Admin.NAME = proAcc.TONotNullString();
                        }
                        //dictionary = new Dictionary<string, object>
                        //{
                        //    { "@ACC_NO", proAcc }
                        //};
                        //parameters = new DynamicParameters(dictionary);

                        //_sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                        //        DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        //        FROM ADMIN
                        //    WHERE 1=1";
                        //_sql += " AND ACC_NO = @ACC_NO";
                        //result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }

                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.FileList = new List<Apply_FileModel>();
                    tempFileList = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);

                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);

                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);

                //核發日期
                result.CERT_APPROVED_DATE_AC = HelperUtil.DateTimeToString(result.CERT_APPROVED_DATE);

                //郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                result.Note = result.Apply.NOTICE_NOTE;

                // 電話

                if (result.Apply.TEL.TONotNullString().Trim() != "")
                {
                    string[] tel = result.Apply.TEL.Split('-');
                    result.TEL_SEC = tel[0];
                    if (tel.ToCount() > 1)
                    {
                        result.TEL_NO = tel[1];

                        if (result.TEL_NO.Contains("#"))
                        {
                            result.TEL_NO = result.TEL_NO.Split('#')[0];
                        }

                        if (result.Apply.TEL.IndexOf('#') > 0)
                        {
                            result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                        }
                    }
                }

                // 傳真
                if (result.Apply.FAX.TONotNullString().Trim() != "")
                {
                    string[] fax = result.Apply.FAX.Split('-');
                    result.FAX_SEC = fax[0];
                    if (fax.ToCount() > 1)
                    {
                        result.FAX_NO = fax[1];

                        if (result.FAX_NO.Contains("#"))
                        {
                            result.FAX_NO = result.FAX_NO.Split('#')[0];
                        }

                        if (result.Apply.FAX.IndexOf('#') > 0)
                        {
                            result.FAX_EXT = result.Apply.FAX.Split('#')[1];
                        }
                    }
                }

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }


                ShareDAO shareDAO = new ShareDAO();
                if (shareDAO.CalculationDocDate("001009", parm.APP_ID) && result.Apply.FLOW_CD == "2")
                {
                    result.IsNotice = "N";
                }
                else
                {
                    result.IsNotice = "Y";
                }

                if (tempFileList != null && tempFileList.Count > 0)
                {
                    foreach (var item in tempFileList)
                    {
                        switch (item.FILE_NO)
                        {
                            case 1:
                                result.FILE1_CHECK = "Y";
                                break;
                            case 2:
                                result.FILE2_CHECK = "Y";
                                break;
                            case 3:
                                result.FILE3_CHECK = "Y";
                                break;
                            case 4:
                                result.FILE4_CHECK = "Y";
                                break;
                            case 5:
                                result.FILE5_CHECK = "Y";
                                break;
                        }
                    }
                }

                for (int i = 1; i <= 5; i++)
                {
                    var item = tempFileList.Where(x => x.FILE_NO == i).FirstOrDefault();

                    if (item != null)
                    {
                        result.FileList.Add(item);
                    }
                    else
                    {
                        result.FileList.Add(new Apply_FileModel() { APP_ID = parm.APP_ID, FILENAME = "", FILE_NO = -1 });
                    }
                }

            }

            return result;
        }

        /// <summary>
        /// 醫事人員資格英文求證 Report
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001009ViewModel QueryApply_001009RPT(Apply_001009ViewModel parm)
        {
            Apply_001009ViewModel result = new Apply_001009ViewModel();
            result.Apply = new ApplyModel();

            List<Apply_FileModel> tempFileList = null;

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"SELECT APP_ID,APPLY_CAUSE,APPLY_CAUSE_TEXT,APPLY_CERT_CATE,LIC_TYPE,LIC_CD,LIC_NUM,CERT_APPROVED_DATE,C_SCHOOL_NAME,
	                                  E_SCHOOL_NAME,STUDY_START_YM,STUDY_END_YM,VERIFY_ADDRESS,IS_MERGE_FILE,EMAIL,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,
	                                  UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MAIL_COUNTRY,RECEIVER,MAIL_DATE,MAIL_BARCODE,IS_FILE1,IS_FILE2,IS_FILE3,IS_FILE4,IS_FILE5
                             FROM APPLY_001009
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001009ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE,MAILBODY,NOTIBODY,E_ALIAS_NAME
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        dictionary = new Dictionary<string, object>
                        {
                            { "@ACC_NO", proAcc }
                        };
                        parameters = new DynamicParameters(dictionary);

                        _sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                                DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                                FROM ADMIN
                            WHERE 1=1";
                        _sql += " AND ACC_NO = @ACC_NO";
                        result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }

                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.FileList = new List<Apply_FileModel>();
                    tempFileList = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);

                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);

                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);

                //核發日期
                result.CERT_APPROVED_DATE_AC = HelperUtil.DateTimeToString(result.CERT_APPROVED_DATE);

                //郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                result.Note = result.Apply.NOTICE_NOTE;

                // 電話
                if (result.Apply.TEL.TONotNullString().Trim() != "")
                {
                    string[] tel = result.Apply.TEL.Split('-');
                    result.TEL_SEC = tel[0];
                    if (tel.ToCount() > 1)
                    {
                        result.TEL_NO = tel[1];

                        if (result.TEL_NO.Contains("#"))
                        {
                            result.TEL_NO = result.TEL_NO.Split('#')[0];
                        }

                        if (result.Apply.TEL.IndexOf('#') > 0)
                        {
                            result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                        }
                    }
                }

                // 傳真

                if (result.Apply.FAX.TONotNullString().Trim() != "")
                {
                    string[] fax = result.Apply.FAX.Split('-');
                    result.FAX_SEC = fax[0];
                    if (fax.ToCount() > 1)
                    {
                        result.FAX_NO = fax[1];

                        if (result.FAX_NO.Contains("#"))
                        {
                            result.FAX_NO = result.FAX_NO.Split('#')[0];
                        }

                        if (result.Apply.FAX.IndexOf('#') > 0)
                        {
                            result.FAX_EXT = result.Apply.FAX.Split('#')[1];
                        }
                    }
                }

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }


                ShareDAO shareDAO = new ShareDAO();
                if (shareDAO.CalculationDocDate("001009", parm.APP_ID) && result.Apply.FLOW_CD == "2")
                {
                    result.IsNotice = "N";
                }
                else
                {
                    result.IsNotice = "Y";
                }

                if (tempFileList != null && tempFileList.Count > 0)
                {
                    foreach (var item in tempFileList)
                    {
                        switch (item.FILE_NO)
                        {
                            case 1:
                                result.FILE1_CHECK = "Y";
                                break;
                            case 2:
                                result.FILE2_CHECK = "Y";
                                break;
                            case 3:
                                result.FILE3_CHECK = "Y";
                                break;
                            case 4:
                                result.FILE4_CHECK = "Y";
                                break;
                            case 5:
                                result.FILE5_CHECK = "Y";
                                break;
                        }
                    }
                }

                for (int i = 1; i <= 5; i++)
                {
                    var item = tempFileList.Where(x => x.FILE_NO == i).FirstOrDefault();

                    if (item != null)
                    {
                        result.FileList.Add(item);
                    }
                    else
                    {
                        result.FileList.Add(new Apply_FileModel() { APP_ID = parm.APP_ID, FILENAME = "", FILE_NO = -1 });
                    }
                }

            }

            return result;
        }


        /// <summary>
        /// 醫事人員資格英文求證存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001009(Apply_001009ViewModel model)
        {
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001009");
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001009Model apply001009 = new Apply_001009Model();
            apply001009.APP_ID = model.APP_ID;

            if (model.Apply.FLOW_CD == "12")
            {
                apply001009 = this.GetRow(apply001009);
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件內容

                    string noticeItem = "";
                    TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                    where.APP_ID = model.APP_ID;
                    var noticeList = GetRowList(where);

                    var newNoticeList = from notice in noticeList
                                        orderby notice.FREQUENCY descending
                                        select notice;

                    var times = newNoticeList.ToCount() == 0 ? 0 : newNoticeList.FirstOrDefault().FREQUENCY.TOInt32();

                    if (model.Apply.FLOW_CD == "2")
                    {
                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        if (model.FILE1_CHECK.TONotNullString() == "Y")
                        {
                            TblAPPLY_NOTICE applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE1";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "護照影本";
                            applyNotice.Field_NAME = "FILE1";
                            Insert(applyNotice);

                            if (noticeItem == "")
                            {
                                noticeItem += "護照影本";
                            }
                            else
                            {
                                noticeItem += "、" + "護照影本";
                            }
                        }

                        if (model.FILE2_CHECK.TONotNullString() == "Y")
                        {
                            TblAPPLY_NOTICE applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE2";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "醫事人員或公共衛生師中文證書影本";
                            applyNotice.Field_NAME = "FILE2";
                            Insert(applyNotice);
                            if (noticeItem == "")
                            {
                                noticeItem += "醫事人員或公共衛生師中文證書影本";
                            }
                            else
                            {
                                noticeItem += "、" + "醫事人員或公共衛生師中文證書影本";
                            }
                        }

                        if (model.FILE3_CHECK.TONotNullString() == "Y")
                        {
                            TblAPPLY_NOTICE applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE3";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "考照時之畢業證書影本";
                            applyNotice.Field_NAME = "FILE3";
                            Insert(applyNotice);
                            if (noticeItem == "")
                            {
                                noticeItem += "考照時之畢業證書影本";
                            }
                            else
                            {
                                noticeItem += "、" + "考照時之畢業證書影本";
                            }
                        }

                        if (model.FILE4_CHECK.TONotNullString() == "Y")
                        {
                            TblAPPLY_NOTICE applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE4";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "考試及格證書影本";
                            applyNotice.Field_NAME = "FILE4";
                            Insert(applyNotice);
                            if (noticeItem == "")
                            {
                                noticeItem += "考試及格證書影本";
                            }
                            else
                            {
                                noticeItem += "、" + "考試及格證書影本";
                            }
                        }

                        if (model.FILE5_CHECK.TONotNullString() == "Y")
                        {
                            TblAPPLY_NOTICE applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE5";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "對方機構求證表格";
                            applyNotice.Field_NAME = "FILE5";
                            Insert(applyNotice);
                            if (noticeItem == "")
                            {
                                noticeItem += "對方機構求證表格";
                            }
                            else
                            {
                                noticeItem += "、" + "對方機構求證表格";
                            }
                        }

                        if (model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            TblAPPLY_NOTICE applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "ALL";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "其他";
                            applyNotice.Field_NAME = "ALL";
                            Insert(applyNotice);
                        }

                        if (noticeItem == "" && model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "其他" + "</p>";
                        }
                        else if (noticeItem != "" && model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + noticeItem + "、其他" + "</p>";
                        }
                        else
                        {
                            MainBody += "<p class=\"form-control-static\">" + noticeItem + "</p>";
                        }

                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Note + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        savestatus = true;
                    }
                    #endregion
                    // 9:逾期未補件而予結案 // 15:自請撤銷 // 8:退件通知
                    if (model.Apply.FLOW_CD == "9" || model.Apply.FLOW_CD == "15" || model.Apply.FLOW_CD == "8")
                    {
                        noteContent.Add("備註", model.Note.TONotNullString());
                        MainBody = GetNote(noteContent);
                    }
                    // 12:核可(發文歸檔)
                    if (model.Apply.FLOW_CD == "12")
                    {
                        noteContent.Add("郵寄日期", model.MAIL_DATE_AC);
                        noteContent.Add("掛號條碼", model.MAIL_BARCODE);
                        MainBody = GetNote(noteContent);
                    }

                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;

                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.Apply.FLOW_CD;
                    apply.NOTICE_NOTE = model.Note.TONotNullString();

                    if (apply.FLOW_CD == "2")
                    {
                        apply.APPLY_NOTICE_DATE = DateTime.Now;
                        apply.ISMODIFY = model.Apply.ISMODIFY;
                        apply.MAILBODY = MainBody;
                    }
                    else
                    {
                        apply.ISMODIFY = "A";
                    }

                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.ISMODIFY = model.Apply.ISMODIFY.TONotNullString().Equals("Z") ? "Y" : model.Apply.ISMODIFY.TONotNullString();
                    apply.NOTIBODY = model.Apply.ISMODIFY.TONotNullString() != "" ? model.Note : "";
                    apply.MAILBODY = MainBody;

                    //base.Update(apply, whereApply);
                    base.Update2(apply, whereApply, dict2, true);

                    if (model.Apply.FLOW_CD == "2")
                    {
                        Apply_001009Model whereApply001009 = new Apply_001009Model();
                        whereApply001009.APP_ID = model.APP_ID;
                        Apply_001009Model apply001009Model = new Apply_001009Model();

                        apply001009Model.IS_FILE1 = model.FILE1_CHECK.TONotNullString();
                        apply001009Model.IS_FILE2 = model.FILE2_CHECK.TONotNullString();
                        apply001009Model.IS_FILE3 = model.FILE3_CHECK.TONotNullString();
                        apply001009Model.IS_FILE4 = model.FILE4_CHECK.TONotNullString();
                        apply001009Model.IS_FILE5 = model.FILE5_CHECK.TONotNullString();
                        apply001009Model.UPD_TIME = DateTime.Now;
                        apply001009Model.UPD_FUN_CD = "ADM-STORE";
                        apply001009Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001009Model.DEL_MK = "N";
                        //this.Update(apply001009Model, whereApply001009);
                        base.Update2(apply001009Model, whereApply001009, dict2, true);
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        Apply_001009Model whereApply001009 = new Apply_001009Model();
                        whereApply001009.APP_ID = model.APP_ID;
                        Apply_001009Model apply001009Model = new Apply_001009Model();

                        apply001009Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AC);
                        apply001009Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001009Model.UPD_TIME = DateTime.Now;
                        apply001009Model.UPD_FUN_CD = "ADM-STORE";
                        apply001009Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001009Model.DEL_MK = "N";
                        //this.Update(apply001009Model, whereApply001009);
                        base.Update2(apply001009Model, whereApply001009, dict2, true);
                    }
                    #endregion

                    #region 依據案件狀態寄發信件內容
                    // 判斷是否要寄信 
                    if (model.Apply.FLOW_CD == "2")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001009Model apply001009Model = new Apply_001009Model();
                        apply001009Model.APP_ID = model.APP_ID;
                        apply001009Model = this.GetRow(apply001009Model);

                        string note = "";

                        note = "補件項目﹕";
                        string item = "";
                        if (model.FILE1_CHECK.TONotNullString() == "Y")
                        {
                            item += "護照影本";
                        }

                        if (model.FILE2_CHECK.TONotNullString() == "Y")
                        {
                            item += (item == "" ? "" : "、") + "醫事人員或公共衛生師中文證書影本";
                        }

                        if (model.FILE3_CHECK.TONotNullString() == "Y")
                        {
                            item += (item == "" ? "" : "、") + "考照時之畢業證書影本";
                        }

                        if (model.FILE3_CHECK.TONotNullString() == "Y")
                        {
                            item += (item == "" ? "" : "、") + "考試及格證書影本";
                        }

                        if (model.FILE3_CHECK.TONotNullString() == "Y")
                        {
                            item += (item == "" ? "" : "、") + "對方機構求證表格";
                        }

                        if (item == "" && model.Apply.ISMODIFY == "Y")
                        {
                            item = "補件項目﹕其他<br/>";
                        }
                        else if (item != "" && model.Apply.ISMODIFY == "Y")
                        {
                            item += (item == "" ? "" : "、") + "其他";
                        }
                        else
                        {

                        }

                        note += item + "<br/>";
                        note += "補件備註﹕" + model.Note;

                        SendMail_Notice(applyModel.NAME, "醫事人員或公共衛生師資格英文求證案件", "001009", apply001009Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, note);
                        savestatus = false;

                    }

                    if (model.Apply.FLOW_CD == "--")
                    {

                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001009Model apply001009Model = new Apply_001009Model();
                        apply001009Model.APP_ID = model.APP_ID;
                        apply001009Model = this.GetRow(apply001009Model);

                        SendMail_InPorcess(applyModel.NAME, "醫事人員或公共衛生師資格英文求證案件", "001009", apply001009Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                        savestatus = false;

                    }

                    if (model.Apply.FLOW_CD == "12")
                    {

                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001009Model apply001009Model = new Apply_001009Model();
                        apply001009Model.APP_ID = model.APP_ID;
                        apply001009Model = this.GetRow(apply001009Model);

                        SendMail_Archive(applyModel.NAME, "醫事人員或公共衛生師資格英文求證案件", "001009", apply001009Model.EMAIL,
                                        applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                        (HelperUtil.TransToDateTime(model.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                        model.MAIL_BARCODE);
                        savestatus = false;

                    }

                    if (model.Apply.FLOW_CD == "9")
                    {

                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001009Model apply001009Model = new Apply_001009Model();
                        apply001009Model.APP_ID = model.APP_ID;
                        apply001009Model = this.GetRow(apply001009Model);

                        SendMail_Expired(applyModel.NAME, "醫事人員或公共衛生師資格英文求證案件", "001009", apply001009Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note);
                        savestatus = false;

                    }

                    if (model.Apply.FLOW_CD == "15")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001009Model apply001009Model = new Apply_001009Model();
                        apply001009Model.APP_ID = model.APP_ID;
                        apply001009Model = this.GetRow(apply001009Model);

                        SendMail_Cancel(applyModel.NAME, "醫事人員或公共衛生師資格英文求證案件", "001009", apply001009Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                    }
                    //if (savestatus)
                    //{
                    //    SendMail_Notice(MainBody, model.APPLY_NAME, -1, model.MAIL, model.APP_ID, "醫事人員資格英文求證案件", "001009");
                    //}
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }


        #endregion

        #region Apply001036 專科護理師證書補(換)發

        /// <summary>
        /// 取得專科護理師證書補(換)發
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001036ViewModel QueryApply_001036(Apply_001036ViewModel parm)
        {
            Apply_001036ViewModel result = new Apply_001036ViewModel();
            result.Apply = new ApplyModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {

                    string _sql = @"SELECT APP_ID,ACTION_TYPE,ISSUE_DATE,LIC_TYPE,LIC_CD,LIC_NUM,ACTION_RES,OTHER_RES,DEL_MK,
                             DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,DIVISION,EMAIL,MAIL_DATE,MAIL_BARCODE
                             FROM APPLY_001036
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001036ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        // 查詢承辦人姓名 20210513 改寫
                        AdminModel where_admin = new AdminModel();
                        where_admin.ACC_NO = proAcc.TONotNullString();
                        result.Admin = GetRow(where_admin);
                        if (string.IsNullOrEmpty(result.Admin.NAME))
                        {
                            result.Admin.NAME = proAcc.TONotNullString();
                        }
                        //dictionary = new Dictionary<string, object>
                        //{
                        //    { "@ACC_NO", proAcc }
                        //};
                        //parameters = new DynamicParameters(dictionary);

                        //_sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                        //        DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        //        FROM ADMIN
                        //    WHERE 1=1";
                        //_sql += " AND ACC_NO = @ACC_NO";
                        //result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }
                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);

                    _sql = @" SELECT APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_ACT_TIME, PAY_EXT_TIME, PAY_INC_TIME, PAY_METHOD, PAY_STATUS_MK, PAY_RET_CD,
                            PAY_RET_MSG, BATCH_NO, APPROVAL_CD, PAY_RET_NO, INVOICE_NO, PAY_DESC, CARD_NO, HOST_TIME, TRANS_RET, SESSION_KEY, AUTH_DATE,
                            AUTH_NO, SETTLE_DATE, OTHER, ROC_ID, CLIENT_IP, OID, SID, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            FROM APPLY_PAY
                            WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.ApplyPay = conn.QueryFirst<APPLY_PAY>(_sql, parameters);

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.File = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>().FirstOrDefault();

                    _sql = @"SELECT APP_ID, Field, ISADDYN, FREQUENCY, ADD_TIME, DEADLINE, NOTE, SRC_NO, BATCH_INDEX
                             FROM APPLY_NOTICE AS A
                             WHERE ISADDYN='Y' AND FREQUENCY = (SELECT MAX(FREQUENCY) FROM APPLY_NOTICE WHERE APP_ID=A.APP_ID) ";
                    _sql += " AND APP_ID = @APP_ID";

                    result.Notices = conn.Query<TblAPPLY_NOTICE>(_sql, parameters).ToList<TblAPPLY_NOTICE>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);

                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);

                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);

                //核發日期
                result.ISSUE_DATE_AC = HelperUtil.DateTimeToString(result.ISSUE_DATE);

                //郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                result.Note = result.Apply.NOTICE_NOTE;

                // 電話
                string[] tel = result.Apply.TEL.Split('-');
                if (result.Apply.TEL.TONotNullString().Trim() != "")
                {
                    result.TEL_SEC = tel[0];
                    if (tel.ToCount() > 1)
                    {
                        result.TEL_NO = tel[1];

                        if (result.TEL_NO.Contains("#"))
                        {
                            result.TEL_NO = result.TEL_NO.Split('#')[0];
                        }

                        if (result.Apply.TEL.IndexOf('#') > 0)
                        {
                            result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                        }
                    }
                }

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }

                result.PAY_ACT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_ACT_TIME).TONotNullString();
                result.PAY_EXT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_EXT_TIME).TONotNullString();
                result.PAY_INC_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_INC_TIME).TONotNullString();

                if (result.ApplyPay.PAY_STATUS_MK == "Y")
                {
                    result.IsPay = true;
                }
                else
                {
                    result.IsPay = false;
                }

                switch (result.ApplyPay.PAY_METHOD)
                {
                    case "C":
                        result.PAY_METHOD_NAME = "信用卡";
                        break;
                    case "D":
                        result.PAY_METHOD_NAME = "匯票";
                        break;
                    case "T":
                        result.PAY_METHOD_NAME = "劃撥";
                        break;
                    case "B":
                        result.PAY_METHOD_NAME = "臨櫃";
                        break;
                    case "S":
                        result.PAY_METHOD_NAME = "超商";
                        break;
                    default:
                        result.PAY_METHOD_NAME = "";
                        break;
                }

                ShareDAO shareDAO = new ShareDAO();
                if (shareDAO.CalculationDocDate("001036", parm.APP_ID) && result.Apply.FLOW_CD == "2")
                {
                    result.IsNotice = "N";
                }
                else
                {
                    result.IsNotice = "Y";
                }

            }

            return result;
        }

        /// <summary>
        /// 取得專科護理師證書補(換)發存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001036(Apply_001036ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001036");
            dict2.Add("LastMODTIME", LastMODTIME);
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001036Model apply001036 = new Apply_001036Model();
            apply001036.APP_ID = model.APP_ID;

            if (model.Apply.FLOW_CD == "12")
            {
                apply001036 = this.GetRow(apply001036);
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件內容
                    if (model.Apply.ISMODIFY.TONotNullString() != "")
                    {
                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        if (model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "其他" + "</p>";
                        }
                        else if (model.Apply.ISMODIFY.TONotNullString() == "Z")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "繳費紀錄照片或pdf檔案、其他" + "</p>";
                        }
                        else
                        {

                            MainBody += "<p class=\"form-control-static\">" + "繳費紀錄照片或pdf檔案" + "</p>";
                        }
                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Note + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        savestatus = true;
                    }
                    #endregion

                    if (model.Apply.FLOW_CD == "9")
                    {
                        noteContent.Add("備註", model.Note.TONotNullString());
                        MainBody = GetNote(noteContent);
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        noteContent.Add("郵寄日期", model.MAIL_DATE_AC);
                        noteContent.Add("掛號條碼", model.MAIL_BARCODE);
                        MainBody = GetNote(noteContent);
                    }

                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.Apply.FLOW_CD;
                    apply.NOTICE_NOTE = model.Note.TONotNullString();
                    if (apply.FLOW_CD == "2")
                    {
                        apply.APPLY_NOTICE_DATE = DateTime.Now;
                        apply.ISMODIFY = model.Apply.ISMODIFY.TONotNullString() == "Z" ? "Y" : model.Apply.ISMODIFY.TONotNullString();

                        TblAPPLY_NOTICE applyNotice = null;
                        TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                        where.APP_ID = model.APP_ID;
                        var noticeList = GetRowList(where);

                        var newNoticeList = from notice in noticeList
                                            orderby notice.FREQUENCY descending
                                            select notice;

                        var times = newNoticeList.ToCount() == 0 ? 0 : newNoticeList.FirstOrDefault().FREQUENCY.TOInt32();
                        applyNotice = new TblAPPLY_NOTICE();
                        applyNotice.ADD_TIME = DateTime.Now;
                        applyNotice.APP_ID = model.APP_ID;
                        applyNotice.ISADDYN = "N";
                        applyNotice.Field = "FILE1";
                        applyNotice.FREQUENCY = times + 1;
                        applyNotice.NOTE = "001036補件";
                        applyNotice.Field_NAME = "FILE1";
                        Insert(applyNotice);
                    }
                    else
                    {
                        apply.ISMODIFY = "A";
                    }

                    if (model.IsPay)
                    {
                        apply.PAY_A_PAID = model.PAY_MONEY;
                    }

                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.ISMODIFY = model.Apply.ISMODIFY.TONotNullString().Equals("Z") ? "Y" : model.Apply.ISMODIFY.TONotNullString();
                    apply.NOTIBODY = model.Apply.ISMODIFY.TONotNullString() != "" ? model.Note : "";
                    apply.MAILBODY = MainBody;
                    base.Update2(apply, whereApply, dict2, true);
                    #endregion

                    #region 繳費資訊
                    if (model.IsPay)
                    {
                        APPLY_PAY applyPayWhere = new APPLY_PAY();
                        applyPayWhere.APP_ID = model.APP_ID;
                        applyPayWhere.PAY_STATUS_MK = "N";

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = model.APP_ID;
                        applyPay.PAY_STATUS_MK = model.IsPay ? "Y" : "N";
                        applyPay.PAY_MONEY = model.ApplyPay != null ? model.ApplyPay.PAY_MONEY : model.PAY_MONEY;
                        applyPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.PAY_EXT_TIME_AC);
                        applyPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.PAY_INC_TIME_AC);
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = "ADM-STORE";
                        applyPay.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        //this.Update(applyPay, applyPayWhere);
                        base.Update2(applyPay, applyPayWhere, dict2, true);
                    }
                    #endregion

                    #region 案件主表
                    if (model.Apply.FLOW_CD == "12")
                    {
                        Apply_001036Model whereApply001036 = new Apply_001036Model();
                        whereApply001036.APP_ID = model.APP_ID;
                        Apply_001036Model apply001036Model = new Apply_001036Model();

                        apply001036Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AC);
                        apply001036Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001036Model.UPD_TIME = DateTime.Now;
                        apply001036Model.UPD_FUN_CD = "ADM-STORE";
                        apply001036Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001036Model.DEL_MK = "N";
                        //this.Update(apply001036Model, whereApply001036);
                        base.Update2(apply001036Model, whereApply001036, dict2, true);
                    }
                    #endregion

                    #region 依據案件狀態寄發信件內容
                    // 判斷是否要寄信

                    if (model.Apply.FLOW_CD == "2")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001036Model apply001036Model = new Apply_001036Model();
                        apply001036Model.APP_ID = model.APP_ID;
                        apply001036Model = this.GetRow(apply001036Model);

                        string note = "";

                        if (model.Apply.ISMODIFY == "Y")
                        {
                            note = "補件項目﹕其他<br/>";
                        }
                        else if (model.Apply.ISMODIFY == "Z")
                        {
                            note = "補件項目﹕繳費紀錄照片或pdf檔案、其他<br/>";
                        }
                        else
                        {
                            note = "補件項目﹕繳費紀錄照片或pdf檔案<br/>";
                        }

                        note += "補件備註﹕" + model.Note;

                        SendMail_Notice(applyModel.NAME, "專科護理師證書補(換)發", "001036", apply001036Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, note);
                        savestatus = false;

                    }

                    if (model.Apply.FLOW_CD == "--")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001036Model apply001036Model = new Apply_001036Model();
                        apply001036Model.APP_ID = model.APP_ID;
                        apply001036Model = this.GetRow(apply001036Model);

                        SendMail_InPorcess(applyModel.NAME, "專科護理師證書補(換)發", "001036", apply001036Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                        savestatus = false;

                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001036Model apply001036Model = new Apply_001036Model();
                        apply001036Model.APP_ID = model.APP_ID;
                        apply001036Model = this.GetRow(apply001036Model);

                        SendMail_Archive(applyModel.NAME, "專科護理師證書補(換)發", "001036", apply001036Model.EMAIL,
                                        applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                        (HelperUtil.TransToDateTime(model.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                        model.MAIL_BARCODE);
                        savestatus = false;
                    }
                    if (model.Apply.FLOW_CD == "9")
                    {

                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001036Model apply001036Model = new Apply_001036Model();
                        apply001036Model.APP_ID = model.APP_ID;
                        apply001036Model = this.GetRow(apply001036Model);

                        SendMail_Expired(applyModel.NAME, "專科護理師證書補(換)發", "001036", apply001036Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note);
                        savestatus = false;

                    }
                    if (model.Apply.FLOW_CD == "15")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001036Model apply001036Model = new Apply_001036Model();
                        apply001036Model.APP_ID = model.APP_ID;
                        apply001036Model = this.GetRow(apply001036Model);

                        SendMail_Cancel(applyModel.NAME, "專科護理師證書補(換)發", "001036", apply001036Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                    }
                    //if (savestatus)
                    //{
                    //    SendMail_Notice(MainBody, model.APPLY_NAME, -1, model.EMAIL, model.APP_ID, "專科護理師證書補(換)發", "001036");
                    //}
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 取得專科護理師證書補(換)發 移轉醫事司
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001036_TO_001005(Apply_001036ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001036");
            dict2.Add("LastMODTIME", LastMODTIME);
            string Msg = "";
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            ApplyDAO applyDao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            var CaseNum = "001005";
            var APP_ID = applyDao.GetApp_ID(CaseNum);

            #region 案件查詢
            ApplyModel apply0010036Where = new ApplyModel();
            apply0010036Where.APP_ID = model.APP_ID;
            ApplyModel applyMoel001036 = new ApplyModel();
            applyMoel001036 = this.GetRow(apply0010036Where);

            Apply_001036Model apply001036Where = new Apply_001036Model();
            apply001036Where.APP_ID = model.APP_ID;
            Apply_001036Model apply001036 = new Apply_001036Model();
            apply001036 = this.GetRow(apply001036Where);

            APPLY_PAY applyPay001036Where = new APPLY_PAY();
            applyPay001036Where.APP_ID = model.APP_ID;
            APPLY_PAY applyPay001036 = new APPLY_PAY();
            applyPay001036 = this.GetRow(applyPay001036Where);

            Apply_FileModel file001036Where = new Apply_FileModel();
            file001036Where.APP_ID = model.APP_ID;
            Apply_FileModel file001036 = new Apply_FileModel();
            file001036 = this.GetRow(file001036Where);
            #endregion
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 案件內容
                    ApplyModel apply = new ApplyModel();
                    apply.InjectFrom(applyMoel001036);
                    apply.PAY_POINT = "A";
                    apply.APP_ID = APP_ID;
                    apply.SRV_ID = CaseNum;
                    apply.SRC_SRV_ID = CaseNum;
                    apply.UNIT_CD = 4;
                    apply.FLOW_CD = "1";
                    apply.PRO_ACC = "";
                    apply.PRO_UNIT_CD = -1;
                    apply.APP_DISP_MK = "N";
                    apply.MOHW_CASE_NO = "";
                    //apply.APP_EXT_DATE = applyDao.GetNTFD(CaseNum);
                    apply.APP_TIME = DateTime.Now;
                    apply.ADD_TIME = DateTime.Now;
                    apply.ADD_FUN_CD = "WEB-APPLY";
                    apply.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "WEB-APPLY";
                    apply.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    this.Insert(apply);

                    Apply_001005Model apply001005 = new Apply_001005Model();
                    apply001005.InjectFrom(apply001036);
                    apply001005.APP_ID = APP_ID;
                    apply001005.ISSUE_DEPT = "3";
                    apply001005.LIC_CD = "護理";
                    apply001005.LIC_TYPE = "F";
                    apply001005.DIVISION = "F";
                    apply001005.LIC_NUM = model.LIC_NUM;
                    apply001005.ISSUE_DATE = HelperUtil.TransToDateTime(model.ISSUE_DATE_AC);
                    apply001005.ADD_TIME = DateTime.Now;
                    apply001005.ADD_FUN_CD = "WEB-APPLY";
                    apply001005.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                    apply001005.UPD_TIME = DateTime.Now;
                    apply001005.UPD_FUN_CD = "WEB-APPLY";
                    apply001005.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                    apply001005.DEL_MK = "N";
                    this.Insert(apply001005);
                    #endregion

                    #region 繳費資訊
                    APPLY_PAY applyPay = new APPLY_PAY();
                    applyPay.InjectFrom(applyPay001036);
                    applyPay.APP_ID = APP_ID;
                    applyPay.ADD_TIME = DateTime.Now;
                    applyPay.ADD_FUN_CD = "WEB-APPLY";
                    applyPay.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                    applyPay.UPD_TIME = DateTime.Now;
                    applyPay.UPD_FUN_CD = "WEB-APPLY";
                    applyPay.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                    applyPay.DEL_MK = "N";
                    this.Insert(applyPay);
                    #endregion

                    #region 檔案上傳
                    if (file001036.FILENAME != null)
                    {
                        Apply_FileModel file = new Apply_FileModel();
                        file.InjectFrom(file001036);
                        file.APP_ID = APP_ID;

                        file.FILENAME = shareDao.PutFile("001005", file001036.FILENAME, "1");

                        file.ADD_TIME = DateTime.Now;
                        file.ADD_FUN_CD = "WEB-APPLY";
                        file.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file.UPD_TIME = DateTime.Now;
                        file.UPD_FUN_CD = "WEB-APPLY";
                        file.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file.DEL_MK = "N";
                        this.Insert(file);
                    }
                    #endregion

                    #region 更新舊有檔案
                    applyMoel001036.FLOW_CD = "0";
                    applyMoel001036.UPD_TIME = DateTime.Now;
                    applyMoel001036.UPD_FUN_CD = "WEB-APPLY";
                    applyMoel001036.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                    //this.Update(applyMoel001036, apply0010036Where);
                    base.Update2(applyMoel001036, apply0010036Where, dict2, true);
                    #endregion

                    #region 信件內容
                    if (model.Apply.FLOW_CD == "51")
                    {
                        IList<SpecialistMailModel> mailList = shareDao.getSpecialist("001005");
                        string receivers = "";

                        foreach (var item in mailList)
                        {
                            if (receivers == "")
                            {
                                receivers = item.EMAIL;
                            }
                            else
                            {
                                receivers += "," + item.EMAIL;
                            }
                        }

                        string MailBody = "";
                        MailBody += "您好﹕<br/>";
                        MailBody += "專科護理師證書補(換)案件編號﹕" + model.APP_ID + "已成功移轉至醫事司。";
                        SendMail(receivers, "專科護理師證書補(換)發案件移轉醫事司通知", MailBody);
                    }
                    #endregion
                    tran.Commit();

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 取得專科護理師證書補(換)發存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001036ToAsign(Apply_001036ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001036");
            dict2.Add("LastMODTIME", LastMODTIME);
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001036Model apply001036 = new Apply_001036Model();
            apply001036.APP_ID = model.APP_ID;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.Apply.FLOW_CD;
                    if (model.IsPay)
                    {
                        apply.PAY_A_PAID = model.PAY_MONEY;
                    }
                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    // 重新分文欄位
                    apply.APP_DISP_MK = "N"; //分文處理
                    apply.PRO_ACC = "";
                    apply.PRO_UNIT_CD = null;
                    base.Update2(apply, whereApply, dict2, true);
                    #endregion

                    #region 繳費資訊
                    if (model.IsPay)
                    {
                        APPLY_PAY applyPayWhere = new APPLY_PAY();
                        applyPayWhere.APP_ID = model.APP_ID;
                        applyPayWhere.PAY_STATUS_MK = "N";

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = model.APP_ID;
                        applyPay.PAY_STATUS_MK = model.IsPay ? "Y" : "N";
                        applyPay.PAY_MONEY = model.PAY_MONEY;
                        applyPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.PAY_EXT_TIME_AC);
                        applyPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.PAY_INC_TIME_AC);
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = "ADM-STORE";
                        applyPay.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        //this.Update(applyPay, applyPayWhere);
                        base.Update2(applyPay, applyPayWhere, dict2, true);
                    }
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public byte[] PrintPdf001036(string id)
        {
            byte[] result = null;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                Form001036Action action = new Form001036Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                result = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        #endregion

        #region Apply001037 醫事人員請領無懲戒紀錄證明申請書

        /// <summary>
        /// 醫事人員請領無懲戒紀錄證明申請書
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001037ViewModel QueryApply_001037(Apply_001037ViewModel parm)
        {
            Apply_001037ViewModel result = new Apply_001037ViewModel();
            result.Apply = new ApplyModel();

            List<Apply_FileModel> tempFileList = null;

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"SELECT APP_ID,LIC_CD,LIC_NUM,ISSUE_DATE,COPIES,TOTAL,MAIL_REC,MAIL_ADDR,MAIL_COUNTRY,
		                            REASON_1,REASON_2,REASON_2_DESC,REASON_3,REASON_3_DESC,REASON_4,REASON_4_DESC,REASON_5,REASON_5_DESC,ATTACH_1,ATTACH_2,
		                            DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,IS_MERGE_FILE,EMAIL,ADDR_CODE,MAIL_DATE,MAIL_BARCODE,
                                    IS_FILE1,IS_FILE2,IS_FILE3
                              FROM APPLY_001037
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001037ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE,MAILBODY,NOTIBODY,E_ALIAS_NAME
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        // 查詢承辦人姓名 20210513 改寫
                        AdminModel where_admin = new AdminModel();
                        where_admin.ACC_NO = proAcc.TONotNullString();
                        result.Admin = GetRow(where_admin);
                        if (string.IsNullOrEmpty(result.Admin.NAME))
                        {
                            result.Admin.NAME = proAcc.TONotNullString();
                        }
                        //dictionary = new Dictionary<string, object>
                        //{
                        //    { "@ACC_NO", proAcc }
                        //};
                        //parameters = new DynamicParameters(dictionary);
                        //_sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                        //        DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        //        FROM ADMIN
                        //    WHERE 1=1";
                        //_sql += " AND ACC_NO = @ACC_NO";
                        //result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }

                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);

                    _sql = @" SELECT APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_ACT_TIME, PAY_EXT_TIME, PAY_INC_TIME, PAY_METHOD, PAY_STATUS_MK, PAY_RET_CD,
                            PAY_RET_MSG, BATCH_NO, APPROVAL_CD, PAY_RET_NO, INVOICE_NO, PAY_DESC, CARD_NO, HOST_TIME, TRANS_RET, SESSION_KEY, AUTH_DATE,
                            AUTH_NO, SETTLE_DATE, OTHER, ROC_ID, CLIENT_IP, OID, SID, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            FROM APPLY_PAY
                            WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.ApplyPay = conn.QueryFirst<APPLY_PAY>(_sql, parameters);

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.FileList = new List<Apply_FileModel>();
                    tempFileList = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                    //result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);

                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);

                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);

                //核發日期
                result.ISSUE_DATE_AC = HelperUtil.DateTimeToString(result.ISSUE_DATE);

                //郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                result.Note = result.Apply.NOTICE_NOTE;

                // 電話
                if (result.Apply.TEL.TONotNullString().Trim() != "")
                {
                    string[] tel = result.Apply.TEL.Split('-');
                    result.TEL_SEC = tel[0];
                    if (tel.ToCount() > 1)
                    {
                        result.TEL_NO = tel[1];

                        if (result.TEL_NO.Contains("#"))
                        {
                            result.TEL_NO = result.TEL_NO.Split('#')[0];
                        }

                        if (result.Apply.TEL.IndexOf('#') > 0)
                        {
                            result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                        }
                    }
                }

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }

                zip = new TblZIPCODE();
                zip.ZIP_CO = result.ADDR_CODE;
                var mailAddress = dao.GetRow(zip);
                result.MAIL_CITY_CODE = result.ADDR_CODE;
                if (mailAddress != null)
                {
                    result.MAIL_CITY_TEXT = mailAddress.TOWNNM;
                    result.MAIL_CITY_DETAIL = result.MAIL_ADDR.TONotNullString().Replace(mailAddress.CITYNM + mailAddress.TOWNNM, "");
                }
                if (result.ApplyPay != null)
                {
                    result.PAY_ACT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_ACT_TIME).TONotNullString();
                    result.PAY_EXT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_EXT_TIME).TONotNullString();
                    result.PAY_INC_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_INC_TIME).TONotNullString();
                }
                else
                {
                    result.PAY_ACT_TIME_AC = null;
                    result.PAY_EXT_TIME_AC = null;
                    result.PAY_INC_TIME_AC = null;
                }

                if (result.ApplyPay != null && result.ApplyPay.PAY_STATUS_MK == "Y")
                {
                    result.IsPay = true;
                }
                else
                {
                    result.IsPay = false;
                }
                if (result.ApplyPay != null)
                {
                    switch (result.ApplyPay.PAY_METHOD)
                    {
                        case "C":
                            result.PAY_METHOD_NAME = "信用卡";
                            break;
                        case "D":
                            result.PAY_METHOD_NAME = "匯票";
                            break;
                        case "T":
                            result.PAY_METHOD_NAME = "劃撥";
                            break;
                        case "B":
                            result.PAY_METHOD_NAME = "臨櫃";
                            break;
                        case "S":
                            result.PAY_METHOD_NAME = "超商";
                            break;
                        default:
                            result.PAY_METHOD_NAME = "";
                            break;
                    }
                }
                else
                {
                    result.PAY_METHOD_NAME = "";
                }


                ShareDAO shareDAO = new ShareDAO();
                if (shareDAO.CalculationDocDate("001037", parm.APP_ID) && result.Apply.FLOW_CD == "2")
                {
                    result.IsNotice = "N";
                }
                else
                {
                    result.IsNotice = "Y";
                }

                if (tempFileList != null && tempFileList.Count > 0)
                {
                    foreach (var item in tempFileList)
                    {
                        switch (item.FILE_NO)
                        {
                            case 1:
                                result.FILE1_CHECK = "Y";
                                break;
                            case 2:
                                result.FILE2_CHECK = "Y";
                                break;
                            case 3:
                                result.FILE3_CHECK = "Y";
                                break;
                        }
                    }

                    for (int i = 1; i <= 3; i++)
                    {
                        var item = tempFileList.Where(x => x.FILE_NO == i).FirstOrDefault();

                        if (item != null)
                        {
                            result.FileList.Add(item);
                        }
                        else
                        {
                            result.FileList.Add(new Apply_FileModel() { APP_ID = parm.APP_ID, FILENAME = "", FILE_NO = -1 });
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 醫事人員請領無懲戒紀錄證明申請書
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001037(Apply_001037ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001037");
            dict2.Add("LastMODTIME", LastMODTIME);
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001037Model apply001037 = new Apply_001037Model();
            apply001037.APP_ID = model.APP_ID;

            if (model.Apply.FLOW_CD == "12")
            {
                apply001037 = this.GetRow(apply001037);
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件內容

                    string noticeItem = "";
                    TblAPPLY_NOTICE applyNotice = null;
                    TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                    where.APP_ID = model.APP_ID;
                    var noticeList = GetRowList(where);

                    var newNoticeList = from notice in noticeList
                                        orderby notice.FREQUENCY descending
                                        select notice;

                    var times = newNoticeList.ToCount() == 0 ? 0 : newNoticeList.FirstOrDefault().FREQUENCY.TOInt32();

                    if (model.Apply.ISMODIFY.TONotNullString() != "")
                    {
                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        if (model.FILE1_CHECK.TONotNullString() == "Y")
                        {
                            applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE1";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "護照影本電子檔";
                            applyNotice.Field_NAME = "FILE1";
                            Insert(applyNotice);
                            if (noticeItem == "")
                            {
                                noticeItem += "護照影本電子檔";
                            }
                            else
                            {
                                noticeItem += "、" + "護照影本電子檔";
                            }

                        }

                        if (model.FILE2_CHECK.TONotNullString() == "Y")
                        {
                            applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE2";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "醫事人員或公共衛生師中文證書電子檔";
                            applyNotice.Field_NAME = "FILE2";
                            Insert(applyNotice);

                            if (noticeItem == "")
                            {
                                noticeItem += "醫事人員或公共衛生師中文證書電子檔";
                            }
                            else
                            {
                                noticeItem += "、" + "醫事人員或公共衛生師中文證書電子檔";
                            }

                        }

                        if (model.FILE3_CHECK.TONotNullString() == "Y")
                        {
                            applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "FILE3";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "繳費紀錄照片或pdf檔案";
                            applyNotice.Field_NAME = "FILE3";
                            Insert(applyNotice);

                            if (noticeItem == "")
                            {
                                noticeItem += "繳費紀錄照片或pdf檔案";
                            }
                            else
                            {
                                noticeItem += "、" + "繳費紀錄照片或pdf檔案";
                            }

                        }

                        if (model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            applyNotice = new TblAPPLY_NOTICE();
                            applyNotice.ADD_TIME = DateTime.Now;
                            applyNotice.APP_ID = model.APP_ID;
                            applyNotice.ISADDYN = "N";
                            applyNotice.Field = "ALL";
                            applyNotice.FREQUENCY = times + 1;
                            applyNotice.NOTE = "其他";
                            applyNotice.Field_NAME = "ALL";
                            Insert(applyNotice);
                        }


                        if (noticeItem == "" && model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "其他" + "</p>";
                        }
                        else if (noticeItem != "" && model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + noticeItem + "、其他" + "</p>";
                        }
                        else
                        {
                            MainBody += "<p class=\"form-control-static\">" + noticeItem + "</p>";
                        }


                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Note + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        savestatus = true;
                    }
                    #endregion
                    // 9:逾期未補件而予結案 // 15:自請撤銷 // 8:退件通知
                    if (model.Apply.FLOW_CD == "9" || model.Apply.FLOW_CD == "15" || model.Apply.FLOW_CD == "8")
                    {
                        noteContent.Add("備註", model.Note.TONotNullString());
                        MainBody = GetNote(noteContent);
                    }
                    // 12:核可(發文歸檔)
                    if (model.Apply.FLOW_CD == "12")
                    {
                        noteContent.Add("郵寄日期", model.MAIL_DATE_AC);
                        noteContent.Add("掛號條碼", model.MAIL_BARCODE);
                        MainBody = GetNote(noteContent);
                    }

                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;

                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.Apply.FLOW_CD;
                    apply.NOTICE_NOTE = model.Note.TONotNullString();

                    if (apply.FLOW_CD == "2")
                    {
                        apply.APPLY_NOTICE_DATE = DateTime.Now;
                        apply.ISMODIFY = model.Apply.ISMODIFY;
                        apply.NOTIBODY = MainBody;
                    }
                    else
                    {
                        apply.ISMODIFY = "A";
                    }

                    if (model.IsPay)
                    {
                        apply.PAY_A_PAID = model.PAY_MONEY;
                    }

                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.ISMODIFY = model.Apply.ISMODIFY.TONotNullString().Equals("Z") ? "Y" : model.Apply.ISMODIFY.TONotNullString();
                    apply.NOTIBODY = model.Apply.ISMODIFY.TONotNullString() != "" ? model.Note : "";
                    apply.MAILBODY = MainBody;

                    //this.Update(apply, whereApply);
                    base.Update2(apply, whereApply, dict2, true);
                    #endregion

                    #region 繳費資訊
                    if (model.IsPay)
                    {
                        APPLY_PAY applyPayWhere = new APPLY_PAY();
                        applyPayWhere.APP_ID = model.APP_ID;
                        applyPayWhere.PAY_STATUS_MK = "N";

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = model.APP_ID;
                        applyPay.PAY_STATUS_MK = model.IsPay ? "Y" : "N";
                        if (model.PAY_METHOD == "D" || model.PAY_METHOD == "T" || model.PAY_METHOD == "B")
                        {
                            //當繳費項目是選匯票、劃撥或臨櫃時，交易金額等同繳費金額
                            applyPay.PAY_MONEY = model.ApplyPay != null ? model.ApplyPay.PAY_MONEY : model.PAY_MONEY;
                        }
                        applyPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.PAY_EXT_TIME_AC);
                        applyPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.PAY_INC_TIME_AC);
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = "ADM-STORE";
                        applyPay.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        //this.Update(applyPay, applyPayWhere);
                        base.Update2(applyPay, applyPayWhere, dict2, true);
                    }
                    #endregion

                    #region 案件主表
                    if (model.Apply.FLOW_CD == "2")
                    {
                        Apply_001037Model whereApply001037 = new Apply_001037Model();
                        whereApply001037.APP_ID = model.APP_ID;
                        Apply_001037Model apply001037Model = new Apply_001037Model();

                        apply001037Model.IS_FILE1 = model.FILE1_CHECK.TONotNullString();
                        apply001037Model.IS_FILE2 = model.FILE2_CHECK.TONotNullString();
                        apply001037Model.IS_FILE3 = model.FILE3_CHECK.TONotNullString();
                        apply001037Model.UPD_TIME = DateTime.Now;
                        apply001037Model.UPD_FUN_CD = "ADM-STORE";
                        apply001037Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001037Model.DEL_MK = "N";
                        //this.Update(apply001037Model, whereApply001037);
                        base.Update2(apply001037Model, whereApply001037, dict2, true);
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        Apply_001037Model whereApply001037 = new Apply_001037Model();
                        whereApply001037.APP_ID = model.APP_ID;
                        Apply_001037Model apply001037Model = new Apply_001037Model();

                        apply001037Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AC);
                        apply001037Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001037Model.UPD_TIME = DateTime.Now;
                        apply001037Model.UPD_FUN_CD = "ADM-STORE";
                        apply001037Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001037Model.DEL_MK = "N";
                        //this.Update(apply001037Model, whereApply001037);
                        base.Update2(apply001037Model, whereApply001037, dict2, true);
                    }
                    #endregion

                    #region 依據案件狀態寄發信件內容
                    // 判斷是否要寄信
                    if (model.Apply.FLOW_CD == "2")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001037Model apply001037Model = new Apply_001037Model();
                        apply001037Model.APP_ID = model.APP_ID;
                        apply001037Model = this.GetRow(apply001037Model);

                        string note = "";
                        note = "補件項目﹕";
                        string item = "";
                        if (model.FILE1_CHECK.TONotNullString() == "Y")
                        {
                            item += "護照影本電子檔";
                        }

                        if (model.FILE2_CHECK.TONotNullString() == "Y")
                        {
                            item += (item == "" ? "" : "、") + "醫事人員或公共衛生師中文證書電子檔";
                        }

                        if (model.FILE3_CHECK.TONotNullString() == "Y")
                        {
                            item += (item == "" ? "" : "、") + "繳費紀錄照片或pdf檔案";
                        }

                        if (item == "" && model.Apply.ISMODIFY == "Y")
                        {
                            item = "補件項目﹕其他<br/>";
                        }
                        else if (item != "" && model.Apply.ISMODIFY == "Y")
                        {
                            item += (item == "" ? "" : "、") + "其他";
                        }
                        else
                        {
                        }

                        note += item + "<br/>";
                        note += "補件備註﹕" + model.Note;

                        SendMail_Notice(applyModel.NAME, "醫事人員或公共衛生師請領無懲戒紀錄證明申請書", "001037", apply001037Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, note);
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "--")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001037Model apply001037Model = new Apply_001037Model();
                        apply001037Model.APP_ID = model.APP_ID;
                        apply001037Model = this.GetRow(apply001037Model);

                        SendMail_InPorcess(applyModel.NAME, "醫事人員或公共衛生師請領無懲戒紀錄證明申請書", "001037", apply001037Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001037Model apply001037Model = new Apply_001037Model();
                        apply001037Model.APP_ID = model.APP_ID;
                        apply001037Model = this.GetRow(apply001037Model);

                        SendMail_Archive(applyModel.NAME, "醫事人員或公共衛生師請領無懲戒紀錄證明申請書", "001037", apply001037Model.EMAIL,
                                        applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                        (HelperUtil.TransToDateTime(model.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                        model.MAIL_BARCODE);
                        savestatus = false;
                    }
                    if (model.Apply.FLOW_CD == "9")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001037Model apply001037Model = new Apply_001037Model();
                        apply001037Model.APP_ID = model.APP_ID;
                        apply001037Model = this.GetRow(apply001037Model);

                        SendMail_Expired(applyModel.NAME, "醫事人員或公共衛生師請領無懲戒紀錄證明申請書", "001037", apply001037Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note);
                        savestatus = false;
                    }
                    if (model.Apply.FLOW_CD == "15")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001037Model apply001037Model = new Apply_001037Model();
                        apply001037Model.APP_ID = model.APP_ID;
                        apply001037Model = this.GetRow(apply001037Model);

                        SendMail_Cancel(applyModel.NAME, "醫事人員或公共衛生師請領無懲戒紀錄證明申請書", "001037", apply001037Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                    }
                    //if (savestatus)
                    //{
                    //    SendMail_Notice(MainBody, model.Apply.NAME, -1, model.MAIL, model.APP_ID, "醫事人員請領無懲戒紀錄證明申請書", "001037");
                    //}
                    #endregion

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        public byte[] PrintPdf001037(string id)
        {
            byte[] result = null;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                Form001037Action action = new Form001037Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                result = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        #endregion

        #region Apply011002

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011002FormModel QueryApply_011002(Apply_011002FormModel parm)
        {
            Apply_011002FormModel result = new Apply_011002FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql =
                    @"select app.SRV_ID ,(convert(varchar, app.APP_TIME, 111))as APP_TIME ,(convert(varchar, app.APP_EXT_DATE, 111)) as APP_EXT_DATE
                                ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM,app.APP_ID,app.ACC_NO,a02.H_TEL,a02.W_TEL,a02.C_ZIPCODE,a02.C_ADDR,a02.H_ZIPCODE,a02.H_ADDR
								,app.MOBILE as MOBILE,a02.EMAIL,a02.PRACTICE_PLACE,a02.SPECIALIST_TYPE,a02.TEST_YEAR,a02.H_EQUAL,app.SEX_CD
								,a02.MERGEYN,app.NAME,app.IDN,app.BIRTHDAY,app.PAY_METHOD,app.PAY_A_FEE,app.FLOW_CD,a02.APPLY_TYPE,app.MOHW_CASE_NO
                                from apply app
                                left join ADMIN ad on app.PRO_ACC = ad.ACC_NO
                                left join APPLY_011002 a02 on app.APP_ID = a02.APP_ID
                                where 1 = 1";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_011002FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011002FileModel GetFileList_011002(string APP_ID)
        {
            var result = new Apply_011002FileModel();
            ShareDAO dao = new ShareDAO();
            result.FILENAM = dao.GetFileGridList(APP_ID);
            result.APP_ID = APP_ID;
            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011002(Apply_011002ViewModel model)
        {
            string Msg = "";
            if (model.Form.FLOW_CD == "2" && model.Form.FileCheck.TONotNullString() == "")
            {
                Msg = "請至少選擇一種補件項目 !";
            }

            if ((model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "5") &&
                model.Form.NOTE.TONotNullString() == "" && model.Form.FileCheck.TONotNullString() != "")
            {
                Msg = "請填寫補件內容 !";
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
            }

            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011002(Apply_011002ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", "011002");
            dict2.Add("LastMODTIME", LastMODTIME);
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = "";
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5")
                    {
                        #region 補件內容
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        // 勾選項目串組html
                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";
                            foreach (var item in needchk)
                            {
                                var Field_NAME = "";
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                #region 補件項目
                                switch (newitem)
                                {
                                    case 0:
                                        Field_NAME = "身分證正面影本";
                                        mainproject += mainproject == "" ? "身分證正面影本" : "、身分證正面影本";
                                        anwhere.Field = "FILE_" + "0";
                                        break;
                                    case 1:
                                        Field_NAME = "身分證反面影本";
                                        mainproject += mainproject == "" ? "身分證反面影本" : "、身分證反面影本";
                                        anwhere.Field = "FILE_" + "1";
                                        break;
                                    case 2:
                                        Field_NAME = "照片(規格應同護照照片)";
                                        mainproject += mainproject == "" ? "照片(規格應同護照照片)" : "、照片(規格應同護照照片)";
                                        anwhere.Field = "FILE_" + "2";
                                        break;
                                    case 3:
                                        Field_NAME = "戶籍謄本或戶口名簿影本";
                                        mainproject += mainproject == "" ? "戶籍謄本或戶口名簿影本" : "、戶籍謄本或戶口名簿影本";
                                        anwhere.Field = "FILE_" + "3";
                                        break;
                                    case 4:
                                        Field_NAME = "其他";
                                        mainproject += mainproject == "" ? "其他" : "、其他";
                                        anwhere.Field = "ALL_" + "4";
                                        break;
                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        ProjectStr1 += ProjectStr1 == "" ? "郵局匯票500元１紙，戶名：衛生福利部" : "、郵局匯票500元１紙，戶名：衛生福利部";
                                        anwhere.Field = "OTHER_" + "5";
                                        break;
                                    case 6:
                                        Field_NAME = "專科社會工作師證書正本";
                                        ProjectStr1 += ProjectStr1 == "" ? "專科社會工作師證書正本" : "、專科社會工作師證書正本";
                                        anwhere.Field = "OTHER_" + "6";
                                        break;
                                }
                                #endregion
                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;

                                if (model.Form.FLOW_CD == "2")
                                {
                                    Insert(anwhere);
                                }

                                //count++;
                                savestatus = true;
                            }
                            ProjectStr = "需重新上傳之文件為：" + mainproject + "<br/>";
                            ProjectStr += "需掛號郵遞紙本之文件為：" + ProjectStr1 + "<br/>";
                            MainBody += ProjectStr;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            ProjectStr += $"補件內容：{model.Form.NOTE}";
                        }
                        else
                        {
                            // 當異動成其他案件狀態時，將通知補件項目調整為已補件
                            TblAPPLY_NOTICE ntcWhere = new TblAPPLY_NOTICE();
                            ntcWhere.APP_ID = model.Form.APP_ID;
                            ntcWhere.ISADDYN = "N";
                            var items = GetRowList(ntcWhere);
                            if (items != null && items.Count() > 0)
                            {
                                var isUpdateNotice = false;
                                foreach (var item in items)
                                {
                                    if (item.Field == "OTHER_5" || item.Field == "OTHER_6")
                                    {
                                        isUpdateNotice = true;
                                    }
                                }
                                if (isUpdateNotice)
                                {
                                    TblAPPLY_NOTICE nUpwhere = new TblAPPLY_NOTICE();
                                    nUpwhere.APP_ID = model.Form.APP_ID;
                                    TblAPPLY_NOTICE upData = new TblAPPLY_NOTICE();
                                    upData.APP_ID = model.Form.APP_ID;
                                    upData.ISADDYN = "Y";
                                    Update(upData, nUpwhere);
                                }
                            }
                        }
                        #endregion
                    }

                    #region 案件狀態
                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);


                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // apply
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;

                        // apply_pay
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_INC_TIME = Convert.ToDateTime(model.Form.PAY_EXT_TIME);
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;

                        //base.Update(newDataPay, wherePay);
                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    // 更新案件狀態
                    newDataApply.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";

                    //base.Update2(newDataApply, whereApply, dict2, true);
                    #endregion

                    #region 寄信內容

                    if ((model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4") && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else if (model.Form.FLOW_CD == "5" && savestatus == false)
                    {
                        Msg = "請選擇項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else
                    {
                        //base.Update(newDataApply, whereApply);
                        base.Update2(newDataApply, whereApply, dict2, true);

                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            // 通知補件
                            if (model.Form.FLOW_CD == "2")
                            {
                                SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, "專科社會工作師證書核發", "011002", ProjectStr: ProjectStr);
                            }
                            // 補正確認完成
                            if (model.Form.FLOW_CD == "4")
                            {
                                string MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">" + model.Form.NAME + "，您好:</th></tr>";
                                var inclueStr = string.IsNullOrEmpty(mainproject) ? ProjectStr1 : string.IsNullOrEmpty(ProjectStr1) ? mainproject : $"{mainproject}、{ProjectStr1}";
                                MailBody += " <tr><td>您所提交的專科社會工作師證書核發申請，已完成資料補件共" + count.ToString() + "件（包括" + inclueStr + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                MailBody += " <tr><td align=\"right\">衛生福利部</td></tr>";
                                MailBody += " <tr><td align=\"right\"> PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";

                                SendMail(model.Form.EMAIL, $"專科社會工作師證書核發，案件編號{model.Form.APP_ID}狀態通知", MailBody, "011002");
                            }
                            // 已接收，處理中
                            if (model.Form.FLOW_CD == "5")
                            {
                                string MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">" + model.Form.NAME + "，您好:</th></tr>";
                                MailBody += " <tr><td>您所提交的專科社會工作師證書核發申請，已完成系統資料填答及上傳程序，本部亦已收到紙本資料共" + count.ToString() + "件（包括" + ProjectStr1 + "）備註:" + model.Form.NOTE + "。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                MailBody += " <tr><td align=\"right\">衛生福利部</td></tr>";
                                MailBody += " <tr><td align=\"right\"> PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";
                                SendMail(model.Form.EMAIL, $"專科社會工作師證書核發，案件編號{model.Form.APP_ID}狀態通知", MailBody, "011002");
                            }

                        }
                        // 已收到紙本，審查中
                        //if (model.Form.FLOW_CD == "5")
                        //{
                        //    SendMail_InPorcess(model.Form.NAME, "專科社會工作師證書核發", "011002", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, "");
                        //}
                        if (model.Form.FLOW_CD == "9")
                        {
                            // 逾期未補件而予結案
                            SendMail_Expired(model.Form.NAME, "專科社會工作師證書核發", "011002", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, model.Form.NOTE);
                        }
                        if (model.Form.FLOW_CD == "0")
                        {
                            // 完成申請
                            SendMail_Success(model.Form.NAME, "專科社會工作師證書核發", "011002", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, "");
                        }

                        tran.Commit();
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 查詢付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public DataTable QueryPayInfo_011002(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select PAY_STATUS_MK,CONVERT(varchar,PAY_INC_TIME ,111) PAY_EXT_TIME
                                    from APPLY_PAY
                                    where APP_ID='" + app_id + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 新增更新付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public string UpdatePayInfo_011002(string app_id, bool IS_PAY_STATUS, DateTime? PayDate, int Pay_A_Fee, string SRV_ID)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", app_id);
            dict2.Add("SRV_ID", SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            var result = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    ApplyModel newDataApply = new ApplyModel();
                    ApplyModel whereApply = new ApplyModel() { APP_ID = app_id };
                    if (IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = Pay_A_Fee;
                        newDataApply.UPD_TIME = DateTime.Now;
                        //base.Update(newDataApply, whereApply);
                        base.Update2(newDataApply, whereApply, dict2, true);

                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = PayDate;
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = app_id;

                        //base.Update(newDataPay, wherePay);
                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    result = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }


        public Apply_011002FormModel GetApplyNotice_011002(string app_id)
        {
            Apply_011002FormModel result = new Apply_011002FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011002FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply011005 專科社會工作師證書換發（更名或污損）

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011005FormModel QueryApply_011005(Apply_011005FormModel parm)
        {
            Apply_011005FormModel result = new Apply_011005FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql =
                    @"select app.SRV_ID ,(convert(varchar, app.APP_TIME, 111))as APP_TIME ,(convert(varchar, app.APP_EXT_DATE, 111)) as APP_EXT_DATE,
                                ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM,app.APP_ID,app.ACC_NO,a05.H_TEL,a05.W_TEL,a05.C_ZIPCODE,a05.C_ADDR,a05.H_ZIPCODE,a05.H_ADDR
								,app.MOBILE as MOBILE,a05.EMAIL,a05.PRACTICE_PLACE,a05.SPECIALIST_TYPE,a05.TEST_YEAR,a05.H_EQUAL,app.SEX_CD
								,a05.MERGEYN,app.NAME,app.IDN,app.BIRTHDAY,app.PAY_METHOD,app.PAY_A_FEE,app.FLOW_CD,a05.APPLY_TYPE,app.MOHW_CASE_NO
                                from apply app
                                left join ADMIN ad on app.PRO_ACC = ad.ACC_NO
                                left join APPLY_011005 a05 on app.APP_ID = a05.APP_ID
                                where 1 = 1";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_011005FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011005FileModel GetFileList_011005(string APP_ID)
        {
            var result = new Apply_011005FileModel();
            ShareDAO dao = new ShareDAO();
            result.FILENAM = dao.GetFileGridList(APP_ID);
            result.APP_ID = APP_ID;
            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011005(Apply_011005ViewModel model)
        {
            string Msg = "";

            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011005(Apply_011005ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", "011005");
            dict2.Add("LastMODTIME", LastMODTIME);
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = "";
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5")
                    {
                        #region 補件內容
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();


                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";
                            #region 補件項目
                            foreach (var item in needchk)
                            {
                                var Field_NAME = "";
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 0:
                                        Field_NAME = "身分證正面影本";
                                        mainproject += mainproject == "" ? "身分證正面影本" : "、身分證正面影本";
                                        anwhere.Field = "FILE_" + "0";
                                        break;
                                    case 1:
                                        Field_NAME = "身分證反面影本";
                                        mainproject += mainproject == "" ? "身分證反面影本" : "、身分證反面影本";
                                        anwhere.Field = "FILE_" + "1";
                                        break;
                                    case 2:
                                        Field_NAME = "照片(規格應同護照照片)";
                                        mainproject += mainproject == "" ? "照片(規格應同護照照片)" : "、照片(規格應同護照照片)";
                                        anwhere.Field = "FILE_" + "2";
                                        break;
                                    case 3:
                                        Field_NAME = "戶籍謄本或戶口名簿影本";
                                        mainproject += mainproject == "" ? "戶籍謄本或戶口名簿影本" : "、戶籍謄本或戶口名簿影本";
                                        anwhere.Field = "FILE_" + "3";
                                        break;
                                    case 4:
                                        Field_NAME = "其他";
                                        mainproject += mainproject == "" ? "其他" : "、其他";
                                        anwhere.Field = "ALL_" + "4";
                                        break;
                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        ProjectStr1 += ProjectStr1 == "" ? "郵局匯票500元１紙，戶名：衛生福利部" : "、郵局匯票500元１紙，戶名：衛生福利部";
                                        anwhere.Field = "OTHER_" + "5";
                                        break;
                                    case 6:
                                        Field_NAME = "專科社會工作師證書正本";
                                        ProjectStr1 += ProjectStr1 == "" ? "專科社會工作師證書正本" : "、專科社會工作師證書正本";
                                        anwhere.Field = "OTHER_" + "6";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                if (model.Form.FLOW_CD == "2")
                                {
                                    Insert(anwhere);
                                }

                                //count++;
                                savestatus = true;
                            }
                            #endregion
                            ProjectStr = "需重新上傳之文件為：" + mainproject + "<br/>";
                            ProjectStr += "需掛號郵遞紙本之文件為：" + ProjectStr1 + "<br/>";
                            MainBody += ProjectStr;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            ProjectStr += $"補件內容：{model.Form.NOTE}";
                        }
                        else
                        {
                            // 當異動成其他案件狀態時，將通知補件項目調整為已補件
                            TblAPPLY_NOTICE ntcWhere = new TblAPPLY_NOTICE();
                            ntcWhere.APP_ID = model.Form.APP_ID;
                            ntcWhere.ISADDYN = "N";
                            var items = GetRowList(ntcWhere);
                            if (items != null && items.Count() > 0)
                            {
                                var isUpdateNotice = false;
                                foreach (var item in items)
                                {
                                    if (item.Field == "OTHER_5" || item.Field == "OTHER_6")
                                    {
                                        isUpdateNotice = true;
                                    }
                                }
                                if (isUpdateNotice)
                                {
                                    TblAPPLY_NOTICE nUpwhere = new TblAPPLY_NOTICE();
                                    nUpwhere.APP_ID = model.Form.APP_ID;
                                    TblAPPLY_NOTICE upData = new TblAPPLY_NOTICE();
                                    upData.APP_ID = model.Form.APP_ID;
                                    upData.ISADDYN = "Y";
                                    Update(upData, nUpwhere);
                                }
                            }
                        }
                        #endregion
                    }

                    #region 案件狀態
                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // apply
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;

                        // apply_pay
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_INC_TIME = Convert.ToDateTime(model.Form.PAY_EXT_TIME);
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;

                        base.Update(newDataPay, wherePay);
                        //base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    // 更新案件狀態
                    newDataApply.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";

                    #endregion

                    #region 寄信內容

                    if ((model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4") && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else if (model.Form.FLOW_CD == "5" && savestatus == false)
                    {
                        Msg = "請選擇項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else
                    {
                        //Update(newDataApply, whereApply);
                        base.Update2(newDataApply, whereApply, dict2, true);
                        string MailBody = "";
                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            switch (model.Form.FLOW_CD)
                            {
                                case "2":
                                    SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, "專科社會工作師證書補發（遺失）", "011005", ProjectStr: ProjectStr);
                                    break;
                                // 補正確認完成
                                case "4":
                                    MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                    MailBody += " <tr><th align=\"left\">" + model.Form.NAME + "，您好:</th></tr>";
                                    var inclueStr = string.IsNullOrEmpty(mainproject) ? ProjectStr1 : string.IsNullOrEmpty(ProjectStr1) ? mainproject : $"{mainproject}、{ProjectStr1}";
                                    MailBody += " <tr><td>您所提交的專科社會工作師證書補發（遺失）申請，已完成資料補件共" + count.ToString() + "件（包括" + inclueStr + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                    MailBody += " <tr><td align=\"right\">衛生福利部</td></tr>";
                                    MailBody += " <tr><td align=\"right\"> PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";
                                    SendMail(model.Form.EMAIL, $"專科社會工作師證書補發（遺失），案件編號{model.Form.APP_ID}狀態通知", MailBody, "011005");
                                    break;
                                //已接收，處理中
                                case "5":
                                    MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                    MailBody += " <tr><th align=\"left\">" + model.Form.NAME + "，您好:</th></tr>";
                                    MailBody += " <tr><td>您所提交的專科社會工作師證書補發（遺失）申請，已完成系統資料填答及上傳程序，本部亦已收到紙本資料共" + count.ToString() + "件（包括" + ProjectStr1 + "）備註:" + model.Form.NOTE + "。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                    MailBody += " <tr><td align=\"right\">衛生福利部</td></tr>";
                                    MailBody += " <tr><td align=\"right\"> PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";
                                    SendMail(model.Form.EMAIL, $"專科社會工作師證書補發（遺失），案件編號{model.Form.APP_ID}狀態通知", MailBody, "011005");
                                    break;
                                default:
                                    break;
                            }

                        }
                        switch (model.Form.FLOW_CD)
                        {
                            //case "5":
                            //    SendMail_InPorcess(model.Form.NAME, "專科社會工作師證書補發（遺失）", "011005", model.Form.ADM_MAIL, model.Form.APP_TIME, model.Form.APP_ID, "");
                            //    break;
                            case "9":
                                // 逾期未補件而予結案
                                SendMail_Expired(model.Form.NAME, "專科社會工作師證書補發（遺失）", "011005", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, model.Form.NOTE);
                                break;
                            case "0":
                                // 完成申請
                                SendMail_Success(model.Form.NAME, "專科社會工作師證書補發（遺失）", "011005", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, "");
                                break;
                            default:
                                break;
                        }
                        tran.Commit();
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 查詢付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public DataTable QueryPayInfo_011005(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select PAY_STATUS_MK,CONVERT(varchar,PAY_INC_TIME,111) PAY_EXT_TIME
                                    from APPLY_PAY
                                    where APP_ID='" + app_id + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }


        public Apply_011005FormModel GetApplyNotice_011005(string app_id)
        {
            Apply_011005FormModel result = new Apply_011005FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011005FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply005001

        public Apply_005001FormModel QueryApply_005001(string app_id)
        {
            Apply_005001FormModel result = new Apply_005001FormModel();
            //附件檔
            DataTable file = QueryApplyFile_005001(app_id);
            //繳費狀態
            DataTable payInfo = QueryPayInfo_005001(app_id);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select APPLY.APP_ID,APPLY.NAME,APPLY.CNT_NAME,APPLY.TEL,APPLY.FAX,APPLY.PAY_A_FEE PAYAMOUNT,
                                           APPLY.FLOW_CD CODE_CD,APPLY.MOHW_CASE_NO,APPLY.APP_TIME,APPLY.APP_EXT_DATE,APPLY.PAY_A_FEE,
                                           APPLY.PAY_METHOD,

                                           APPLY_005001.MF_CNT_NAME,APPLY_005001.MF_CNT_NAME_E,APPLY_005001.MF_CNT_TEL,APPLY_005001.MF_ADDR,
                                           APPLY_005001.MF_ADDR_E,APPLY_005001.DRUG_NAME,APPLY_005001.DRUG_NAME_E,APPLY_005001.DRUG_ABROAD_NAME,
                                           APPLY_005001.DRUG_ABROAD_NAME_E,APPLY_005001.DOSAGE_FORM,APPLY_005001.DOSAGE_FORM_E,APPLY_005001.LIC_CD PL_CD,
                                           APPLY_005001.LIC_NUM PL_NUM,APPLY_005001.LIC_CD_E PL_CD_E,APPLY_005001.LIC_NUM_E PL_NUM_E,
                                           CONVERT(varchar,APPLY_005001.ISSUE_DATE,111) ISSUE_DATE,
                                           CONVERT(varchar,APPLY_005001.EXPIR_DATE,111) EXPIR_DATE,APPLY_005001.MF_CONT,
                                           APPLY_005001.MF_CONT_E,APPLY_005001.PC_SCALE_1,APPLY_005001.PC_SCALE_1E,APPLY_005001.PC_SCALE_21,
                                           APPLY_005001.PC_SCALE_22,APPLY_005001.PC_SCALE_23,APPLY_005001.PC_SCALE_24,APPLY_005001.PC_SCALE_2E,
                                           APPLY_005001.INDIOCATION,APPLY_005001.INDIOCATION_E,APPLY_005001.EFFICACY,APPLY_005001.EFFICACY_E,
                                           APPLY_005001.DRUG_ABROAD_CHECK,APPLY_005001.EXPIR_DATE_CHECK,APPLY_005001.Concentrate_CHECK,
                                           APPLY_005001.INDIOCATION_CHECK,APPLY_005001.EFFICACY_CHECK,APPLY_005001.EMAIL,
                                           APPLY_005001.MF_COPIES PAYCOUNT,

                                           ISNULL(ADMIN.NAME,APPLY.PRO_ACC) AS ADMIN_NAME
                                    from APPLY left join APPLY_005001 on APPLY.APP_ID = APPLY_005001.APP_ID
		                                       left join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where APPLY.APP_ID='" + app_id + "'";
                    result = conn.QueryFirst<Apply_005001FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            result.APPLY_STATUS = GetSchedule(app_id, "09");

            if (file != null)
            {
                if (file.Rows.Count > 0)
                {
                    result.FILE1_TEXT = file.Rows[0][0].ToString();
                    result.FILE2_TEXT = file.Rows[0][1].ToString();
                    result.FILE3_TEXT = file.Rows[0][2].ToString();
                }
            }

            if (payInfo != null)
            {
                if (payInfo.Rows.Count > 0)
                {
                    result.PAY_STATUS = payInfo.Rows[0][0].ToString();
                    if (payInfo.Rows[0][0].ToString() == "Y")
                    {
                        result.PAY_ACT_TIME = payInfo.Rows[0][1].ToString();
                    }
                }
            }
            //公文取號
            DataTable offic = QueryOfficial(app_id, result.MOHW_CASE_NO);
            if (offic != null)
            {
                if (offic.Rows.Count > 0)
                {
                    result.MOHW_CASE_NO = offic.Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(offic.Rows[0][0].ToString()))
                    {
                        result.MOHW_CASE_DATE = offic.Rows[0][1].ToString();
                    }
                }
            }
            return result;
        }

        public DataTable QueryApplyFile_005001(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @file1 nvarchar(max),@file2 nvarchar(max),@file3 nvarchar(max);

                                    select @file1 = isnull(null,SUBSTRING(FILENAME,16,len(FILENAME))) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='1'

                                    select @file2 = isnull(null,SUBSTRING(FILENAME,16,len(FILENAME))) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='2'

                                    select @file3 = isnull(null,SUBSTRING(FILENAME,16,len(FILENAME))) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='3'

                                    select @file1 file1,@file2 file2,@file3 file3";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public DataTable QueryPayInfo_005001(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select PAY_STATUS_MK,CONVERT(varchar,PAY_INC_TIME,111) PAY_INC_TIME
                                    from APPLY_PAY
                                    where APP_ID='" + app_id + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApplyDoc005001(Apply_005001ViewModel model)
        {
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            string Msg = "";
            DataTable UndertakerInfo = new DataTable();
            var GetName = "";
            var GetTel = "";
            var GetEmail = "";
            DataTable CaseNoInfo = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    #region 取得承辦人資訊
                    string _sql = @"select ADMIN.NAME,ADMIN.TEL,ADMIN.MAIL
                                    from APPLY join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where APPLY.APP_ID='" + model.Form.APP_ID + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(UndertakerInfo);
                    if (UndertakerInfo.Rows.Count > 0)
                    {
                        GetName = UndertakerInfo.Rows[0]["NAME"].ToString();
                        GetTel = UndertakerInfo.Rows[0]["TEL"].ToString();
                        GetEmail = UndertakerInfo.Rows[0]["MAIL"].ToString();
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }

                if (model.Form.CODE_CD == "4")
                {
                    try
                    {
                        #region 取得公文資訊
                        string _sql = @"select TOP 1 APP_ID,MOHW_CASE_NO,convert(varchar,INSERTDATE,111) INSERTDATE
                                        from OFFICIAL_DOC
                                        where APP_ID='" + model.Form.APP_ID + "' and MOHW_CASE_NO='" + model.Form.MOHW_CASE_NO + @"'
                                        order by INSERTDATE DESC";
                        SqlCommand cmd = new SqlCommand(_sql, conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(CaseNoInfo);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        Msg = "存檔失敗，請聯絡系統管理員 。";
                    }
                }

                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件欄位紀錄
                    // CODE_CD='2' 申請資料待確認
                    // CODE_CD='4' 申請案補件中
                    if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        // 紀錄補件欄位
                        var count = 0;

                        // 比對是否為空值，若不為空則新增至欄位紀錄
                        foreach (var item in model.Detail.GetType().GetProperties())
                        {
                            if (item.GetValue(model.Detail) != null &&
                                (item.GetValue(model.Detail)).ToString() != "False" &&
                                (item.GetValue(model.Detail)).ToString() != "True" &&
                                (item.GetValue(model.Detail)).ToString() != "0" &&
                                item.Name != "DI" && item.Name != "PC")
                            {
                                anwhere = new TblAPPLY_NOTICE();
                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.Field = item.Name;
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = item.GetValue(model.Detail).TONotNullString();
                                Insert(anwhere);
                                MainBody += "<tr>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.GetValue(model.Detail).TONotNullString() + "</td>";
                                MainBody += "</tr>";

                                count++;
                                savestatus = true;
                            }
                        }

                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            string MailBody = "";
                            if (model.Form.CODE_CD == "2")
                            {
                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司產銷證明書(案號：" + model.Form.APP_ID + ")，經本部審查後，尚有待修正事項，請登入「人民申請案線上申辦服務系統」，依審查建議盡速修正申請表單內容，並於3個工作天內完成;倘未能如期送出修正內容，本部將行文通知補件。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + GetName + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + GetTel + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + GetEmail + "</td></tr>";
                                MailBody += "</table>";
                            }
                            else if (model.Form.CODE_CD == "4")
                            {

                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司產銷證明書(案號：" + model.Form.APP_ID + ")一案，本部業於" + (Convert.ToInt32(CaseNoInfo.Rows[0][2].ToString().Split('/')[0]) - 1911).ToString() + "年" + CaseNoInfo.Rows[0][2].ToString().Split('/')[1] + "月" + CaseNoInfo.Rows[0][2].ToString().Split('/')[2] + "日以衛部中字第" + model.Form.MOHW_CASE_NO + "號函，通知貴公司待補正事項，敬請配合辦理。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + GetName + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + GetTel + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + GetEmail + "</td></tr>";
                                MailBody += "</table>";
                            }

                            SendMail_Notice(MainBody, model.Form.CNT_NAME, count, model.Form.EMAIL, model.Form.APP_ID, "產銷證明書", "005001", MailBody);
                        }
                    }
                    #endregion

                    if (savestatus)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                        Msg = "請選擇補件項目並輸入備註說明。";
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 一般存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApply005001(Apply_005001ViewModel model)
        {
            string Msg = "";
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string s_SRV_ID = "005001";
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", model.Form.APP_ID);
                    dict2.Add("SRV_ID", s_SRV_ID);
                    dict2.Add("LastMODTIME", LastMODTIME);

                    #region 寫回Table
                    Apply_005001Model where = new Apply_005001Model();
                    where.APP_ID = model.Form.APP_ID;
                    Apply_005001Model newData = new Apply_005001Model();
                    newData.InjectFrom(model.Form);
                    newData.PC_SCALE_2E = model.Form.PC_SCALE_1E;
                    newData.APP_ID = model.Form.APP_ID;
                    newData.MF_CNT_TEL = model.Form.TEL;
                    newData.MF_ADDR = model.Form.TAX_ORG_CITY_CODE + model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL;
                    newData.LIC_CD = model.Form.PL_CD;
                    newData.LIC_NUM = model.Form.PL_Num;
                    newData.LIC_CD_E = model.Form.PL_CD_E;
                    newData.LIC_NUM_E = model.Form.PL_Num_E;
                    newData.ISSUE_DATE = Convert.ToDateTime(model.Form.ISSUE_DATE);
                    if (string.IsNullOrEmpty(model.Form.EXPIR_DATE))
                    {
                        newData.EXPIR_DATE = null;
                    }
                    else
                    {
                        newData.EXPIR_DATE = Convert.ToDateTime(model.Form.EXPIR_DATE);
                    }
                    newData.MF_COPIES = model.Form.PAYCOUNT.TOInt32();
                    newData.ADD_TIME = DateTime.Now;
                    newData.ADD_FUN_CD = "WEB-APPLY";
                    newData.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                    newData.UPD_TIME = DateTime.Now;
                    newData.UPD_FUN_CD = "WEB-APPLY";
                    newData.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                    newData.DEL_MK = "N";
                    newData.EMAIL = model.Form.EMAIL;

                    base.Update2(newData, where, dict2, true);

                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);
                    newDataApply.APP_TIME = null;
                    newDataApply.ADDR_CODE = null;
                    newDataApply.ADDR = null;
                    newDataApply.FLOW_CD = model.Form.CODE_CD;

                    base.Update2(newDataApply, whereApply, dict2, true);
                    #endregion

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = model.Form.PAYAMOUNT;
                        newDataApply.UPD_TIME = DateTime.Now;
                        base.Update2(newDataApply, whereApply, dict2, true);

                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_INC_TIME = Convert.ToDateTime(model.Form.PAY_ACT_TIME);
                        newDataPay.UPD_TIME = DateTime.Now;
                        newDataPay.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;

                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    #region apply005001_DI
                    model.DI.APP_ID = model.Form.APP_ID;
                    foreach (var item in model.DI.GoodsList)
                    {
                        item.APP_ID = model.Form.APP_ID;
                        item.ADD_TIME = DateTime.Now;
                        item.ADD_FUN_CD = "WEB-APPLY";
                        item.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                        item.UPD_TIME = DateTime.Now;
                        item.UPD_FUN_CD = "WEB-APPLY";
                        item.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                        item.DEL_MK = "N";
                    }
                    model.DI.SaveGoodsList("SRL_NO");
                    #endregion

                    #region apply005001_PC

                    if (model.Form.IS_Concentrate_CHECK)
                    {
                        model.PC.APP_ID = model.Form.APP_ID;
                        foreach (var item in model.PC.GoodsList)
                        {
                            item.APP_ID = model.Form.APP_ID;
                            item.ADD_TIME = DateTime.Now;
                            item.ADD_FUN_CD = "WEB-APPLY";
                            item.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                            item.UPD_TIME = DateTime.Now;
                            item.UPD_FUN_CD = "WEB-APPLY";
                            item.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                            item.DEL_MK = "N";
                        }
                        model.PC.SaveGoodsList("SRL_NO");
                    }
                    #endregion

                    #region 公文文號
                    if (!string.IsNullOrWhiteSpace(model.Form.MOHW_CASE_NO))
                    {
                        // 是否已有該公文文號
                        OFFICIAL_DOC where_doc = new OFFICIAL_DOC();
                        where_doc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                        where_doc.APP_ID = model.Form.APP_ID;
                        var docOrg = GetRow(where_doc);
                        if (docOrg == null)
                        {
                            // 新增
                            OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                            InsertDoc.APP_ID = model.Form.APP_ID;
                            InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                            InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                            InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                            Insert(InsertDoc);
                        }
                        else
                        {
                            if (docOrg.INSERTDATE >= Convert.ToDateTime(model.Form.MOHW_CASE_DATE) &&
                                Convert.ToDateTime(model.Form.MOHW_CASE_DATE).AddDays(1) >= docOrg.INSERTDATE)
                            {
                                //// 更新
                                //OFFICIAL_DOC where_up = new OFFICIAL_DOC();
                                //where_up.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //where_up.APP_ID = model.Form.APP_ID;
                                //OFFICIAL_DOC UpdateData = new OFFICIAL_DOC();
                                //UpdateData.APP_ID = model.Form.APP_ID;
                                //UpdateData.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //UpdateData.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                //UpdateData.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                //Update2(UpdateData, where_up, dict2, true);
                            }
                            else
                            {
                                // 新增
                                OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                                InsertDoc.APP_ID = model.Form.APP_ID;
                                InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                Insert(InsertDoc);
                            }
                        }
                    }

                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_005001FormModel GetApplyNotice_005001(string app_id)
        {
            Apply_005001FormModel result = new Apply_005001FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_005001FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }


        public void CaseFinishMail_005001(Apply_005001ViewModel model)
        {
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                string MOHDATE = GetMOHCaseDate(model.Form.APP_ID, model.Form.MOHW_CASE_NO);
                System.Data.DataTable UndertakerInfo = GetAdminInfo(model.Form.APP_ID);
                var GetName = "";
                var GetTel = "";
                var GetEmail = "";
                if (UndertakerInfo.Rows.Count > 0)
                {
                    GetName = UndertakerInfo.Rows[0]["NAME"].ToString();
                    GetTel = UndertakerInfo.Rows[0]["TEL"].ToString();
                    GetEmail = UndertakerInfo.Rows[0]["MAIL"].ToString();
                }
                string MailBody = "";

                //結案寄信通知(結案(回函核准)CODE_CD=0;結案(歉難同意)CODE_CD=20)
                if (model.Form.CODE_CD == "0")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>本部業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函，";
                    MailBody += "檢送貴公司申請之產銷證明書(案號:" + model.Form.APP_ID + ")正本，請查照。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + GetName + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + GetTel + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + GetEmail + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "產銷證明書", "005001", MailBody, null, "", "產銷證明書，案件編號:" + model.Form.APP_ID + "，已結案(回函核准)");
                }

                if (model.Form.CODE_CD == "20")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>貴公司申請之產銷證明書(案號:" + model.Form.APP_ID + ")一案，本部歉難同意，";
                    MailBody += "業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函回復貴公司。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + GetName + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + GetTel + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + GetEmail + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "產銷證明書", "005001", MailBody, null, "", "產銷證明書，案件編號:" + model.Form.APP_ID + "，已結案(歉難同意)");
                }

                tran.Commit();
                conn.Close();
                conn.Dispose();
            }
        }
        #endregion

        #region Apply005004

        public Apply_005004FormModel QueryApply_005004(string app_id)
        {
            Apply_005004FormModel result = new Apply_005004FormModel();
            //附件檔
            DataTable file = QueryApplyFile_005004(app_id);
            //繳費狀態
            DataTable payInfo = QueryPayInfo_005004(app_id);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql =
                        @"select APPLY.APP_TIME,APPLY.APP_EXT_DATE,ISNULL(ADMIN.NAME,APPLY.PRO_ACC) AS ADMIN_NAME,APPLY.MOHW_CASE_NO,APPLY.APP_ID,APPLY_005004.COMP_NAME,
                                           APPLY.CNT_NAME,APPLY.TEL,APPLY.FAX,APPLY_005004.EMAIL,APPLY.APP_TIME,APPLY.APP_ID,APPLY_005004.APPLY_TYPE,
                                           APPLY_005004.LIC_NUM,APPLY.NAME,APPLY_005004.PL_CD,APPLY_005004.PL_NUM,APPLY.ADDR_CODE,APPLY.ADDR,
                                           APPLY.CHR_NAME,APPLY.IDN,APPLY_005004.PP_NAME,APPLY_005004.FRC_NUM,APPLY_005004.TRA_CHECK,
                                           APPLY_005004.CON_CHECK,convert(varchar,APPLY_005004.ISSUE_DATE,111) ISSUE_DATE,APPLY.PAY_METHOD,
                                           APPLY.PAY_A_FEE,APPLY_005004.RADIOYN,APPLY.FLOW_CD CODE_CD,APPLY_005004.MOHW_CASE_NO_SELF
                                    from APPLY join APPLY_005004 on APPLY.APP_ID = APPLY_005004.APP_ID
		                                   left join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where APPLY.APP_ID='" + app_id + "'";
                    result = conn.QueryFirst<Apply_005004FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            result.APPLY_STATUS = GetSchedule(app_id, "09");

            if (file != null)
            {
                if (file.Rows.Count > 0)
                {
                    result.FILE2_TEXT = file.Rows[0][1].ToString();
                    result.FILE3_TEXT = file.Rows[0][2].ToString();
                    result.FILE4_TEXT = file.Rows[0][3].ToString();
                }
            }

            if (payInfo != null)
            {
                if (payInfo.Rows.Count > 0)
                {
                    result.PAY_STATUS = payInfo.Rows[0][0].ToString();
                    if (payInfo.Rows[0][0].ToString() == "Y")
                    {
                        result.PAY_ACT_TIME = payInfo.Rows[0][1].ToString();
                    }
                }
            }
            //公文取號
            DataTable offic = QueryOfficial(app_id, result.MOHW_CASE_NO);
            if (offic != null)
            {
                if (offic.Rows.Count > 0)
                {
                    result.MOHW_CASE_NO = offic.Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(offic.Rows[0][0].ToString()))
                    {
                        result.MOHW_CASE_DATE = offic.Rows[0][1].ToString();
                    }
                }
            }
            return result;
        }

        public DataTable QueryApplyFile_005004(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @file1 nvarchar(max),@file2 nvarchar(max),@file3 nvarchar(max),@file4 nvarchar(max);

                                    select @file1 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='1'

                                    select @file2 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='2'

                                    select @file3 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='3'

                                    select @file4 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='4'

                                    select @file1 file1,@file2 file2,@file3 file3,@file4 file4";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public DataTable QueryPayInfo_005004(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select PAY_STATUS_MK,CONVERT(varchar,PAY_INC_TIME,111) PAY_INC_TIME
                                    from APPLY_PAY
                                    where APP_ID='" + app_id + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApplyDoc005004(Apply_005004ViewModel model)
        {
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            string Msg = "";
            DataTable UndertakerInfo = new DataTable();
            DataTable CaseNoInfo = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    #region 取得承辦人資訊
                    UndertakerInfo = GetAdminInfo(model.Form.APP_ID);
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }

                if (model.Form.CODE_CD == "4")
                {
                    try
                    {
                        #region 取得公文資訊
                        string _sql = @"select TOP 1 APP_ID,MOHW_CASE_NO,convert(varchar,INSERTDATE,111) INSERTDATE
                                        from OFFICIAL_DOC
                                        where APP_ID='" + model.Form.APP_ID + "' and MOHW_CASE_NO='" + model.Form.MOHW_CASE_NO + @"'
                                        order by INSERTDATE DESC";
                        SqlCommand cmd = new SqlCommand(_sql, conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(CaseNoInfo);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        Msg = "存檔失敗，請聯絡系統管理員 。";
                    }
                }

                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件欄位紀錄
                    // CODE_CD='2' 申請資料待確認
                    // CODE_CD='4' 申請案補件中
                    if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        // 紀錄補件欄位
                        var count = 0;

                        // 比對是否為空值，若不為空則新增至欄位紀錄
                        foreach (var item in model.Detail.GetType().GetProperties())
                        {
                            if (item.GetValue(model.Detail) != null && (item.GetValue(model.Detail)).ToString() != "False" && (item.GetValue(model.Detail)).ToString() != "True")
                            {
                                anwhere = new TblAPPLY_NOTICE();
                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.Field = item.Name;
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = item.GetValue(model.Detail).TONotNullString();
                                Insert(anwhere);
                                MainBody += "<tr>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.GetValue(model.Detail).TONotNullString() + "</td>";
                                MainBody += "</tr>";

                                count++;
                                savestatus = true;
                            }
                        }

                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            string MailBody = "";
                            if (model.Form.CODE_CD == "2")
                            {
                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司申請中藥GMP廠證明文件(中文)(案號：" + model.Form.APP_ID + ")，經本部審查後，尚有待修正事項，請登入「人民申請案線上申辦服務系統」，依審查建議盡速修正申請表單內容，並於3個工作天內完成;倘未能如期送出修正內容，本部將行文通知補件。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }
                            else if (model.Form.CODE_CD == "4")
                            {

                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司申請中藥GMP廠證明文件(中文)(案號：" + model.Form.APP_ID + ")一案，本部業於" + (Convert.ToInt32(CaseNoInfo.Rows[0][2].ToString().Split('/')[0]) - 1911).ToString() + "年" + CaseNoInfo.Rows[0][2].ToString().Split('/')[1] + "月" + CaseNoInfo.Rows[0][2].ToString().Split('/')[2] + "日以衛部中字第" + model.Form.MOHW_CASE_NO + "號函，通知貴公司待補正事項，敬請配合辦理。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }

                            SendMail_Notice(MainBody, model.Form.CNT_NAME, count, model.Form.EMAIL, model.Form.APP_ID, "中藥GMP廠證明文件(中文)", "005004", MailBody);
                        }
                    }
                    #endregion

                    if (savestatus)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                        Msg = "請選擇補件項目並輸入備註說明。";
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 一般存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApply005004(Apply_005004ViewModel model)
        {
            string Msg = "";
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string s_SRV_ID = "005004";
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", model.Form.APP_ID);
                    dict2.Add("SRV_ID", s_SRV_ID);
                    dict2.Add("LastMODTIME", LastMODTIME);

                    #region 寫回Table
                    Apply_005004Model where = new Apply_005004Model();
                    where.APP_ID = model.Form.APP_ID;
                    Apply_005004Model newData = new Apply_005004Model();
                    newData.InjectFrom(model.Form);
                    newData.ADD_TIME = null;
                    newData.UPD_TIME = DateTime.Now;
                    newData.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                    base.Update2(newData, where, dict2, true);

                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);
                    newDataApply.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                    newDataApply.ADD_TIME = null;
                    newDataApply.APP_TIME = null;
                    newDataApply.FLOW_CD = model.Form.CODE_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                    #endregion

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // applyModel
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        // applyPay
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_INC_TIME = Convert.ToDateTime(model.Form.PAY_ACT_TIME);
                        newDataPay.UPD_TIME = DateTime.Now;
                        newDataPay.ADD_TIME = null;

                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion
                    // update Apply
                    base.Update2(newDataApply, whereApply, dict2, true);

                    #region 公文文號
                    if (!string.IsNullOrWhiteSpace(model.Form.MOHW_CASE_NO))
                    {
                        // 是否已有該公文文號
                        OFFICIAL_DOC where_doc = new OFFICIAL_DOC();
                        where_doc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                        where_doc.APP_ID = model.Form.APP_ID;
                        var docOrg = GetRow(where_doc);
                        if (docOrg == null)
                        {
                            // 新增
                            OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                            InsertDoc.APP_ID = model.Form.APP_ID;
                            InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                            InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                            InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                            Insert(InsertDoc);
                        }
                        else
                        {
                            if (docOrg.INSERTDATE >= Convert.ToDateTime(model.Form.MOHW_CASE_DATE) &&
                                Convert.ToDateTime(model.Form.MOHW_CASE_DATE).AddDays(1) >= docOrg.INSERTDATE)
                            {
                                //// 更新
                                //OFFICIAL_DOC where_up = new OFFICIAL_DOC();
                                //where_up.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //where_up.APP_ID = model.Form.APP_ID;
                                //OFFICIAL_DOC UpdateData = new OFFICIAL_DOC();
                                //UpdateData.APP_ID = model.Form.APP_ID;
                                //UpdateData.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //UpdateData.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                //UpdateData.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                //Update2(UpdateData, where_up, dict2, true);
                            }
                            else
                            {
                                // 新增
                                OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                                InsertDoc.APP_ID = model.Form.APP_ID;
                                InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                Insert(InsertDoc);
                            }
                        }
                    }
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_005004FormModel GetApplyNotice_005004(string app_id)
        {
            Apply_005004FormModel result = new Apply_005004FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_005004FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public string GetMOHCaseDate(string APP_ID, string MOHW_CASE_NO)
        {
            string rst = "";

            string sql = @"
 select TOP 1 concat(year(INSERTDATE)-1911,format(INSERTDATE,'MMdd')) INSERTDATE_TW
 FROM OFFICIAl_DOC
 WHERE 1=1
 AND APP_ID=@APP_ID 
 AND MOHW_CASE_NO=@MOHW_CASE_NO
 ORDER BY INSERTDATE DESC";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                DataTable _dt = new DataTable();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Clear();
                cmd.Parameters.Add("APP_ID", SqlDbType.VarChar).Value = APP_ID;
                cmd.Parameters.Add("MOHW_CASE_NO", SqlDbType.VarChar).Value = MOHW_CASE_NO;
                _dt.Load(cmd.ExecuteReader());
                if (_dt.Rows.Count > 0) { rst = Convert.ToString(_dt.Rows[0]["INSERTDATE_TW"]); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        public DataTable GetAdminInfo(string APP_ID)
        {
            DataTable result = new DataTable();

            string _sql = @"
 select ADMIN.NAME
 ,ADMIN.TEL
 ,ADMIN.MAIL
 from APPLY 
 join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
 where APPLY.APP_ID=@APP_ID ";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(_sql, conn);
                cmd.Parameters.Clear();
                cmd.Parameters.Add("APP_ID", SqlDbType.VarChar).Value = APP_ID;
                result.Load(cmd.ExecuteReader());
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public void CaseFinishMail_005004(Apply_005004ViewModel model)
        {
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                string MOHDATE = GetMOHCaseDate(model.Form.APP_ID, model.Form.MOHW_CASE_NO);
                System.Data.DataTable UndertakerInfo = GetAdminInfo(model.Form.APP_ID);
                string MailBody = "";

                //結案寄信通知(結案(回函核准)CODE_CD=0;結案(歉難同意)CODE_CD=20)
                if (model.Form.CODE_CD == "0")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>本部業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函，";
                    MailBody += "檢送貴公司申請之中藥GMP廠證明文件(中文)(案號:" + model.Form.APP_ID + ")正本，請查照。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "中藥GMP廠證明文件(中文)", "005004", MailBody, null, "", "中藥GMP廠證明文件(中文)，案件編號:" + model.Form.APP_ID + "，已結案(回函核准)");
                }

                if (model.Form.CODE_CD == "20")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>貴公司申請之中藥GMP廠證明文件(中文)(案號:" + model.Form.APP_ID + ")一案，本部歉難同意，";
                    MailBody += "業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函回復貴公司。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "中藥GMP廠證明文件(中文)", "005004", MailBody, null, "", "中藥GMP廠證明文件(中文)，案件編號:" + model.Form.APP_ID + "，已結案(歉難同意)");
                }

                tran.Commit();
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Apply005002
        /// <summary>
        /// 後台承辦人員儲存案件資料
        /// </summary>
        /// <param name="model"></param>
        public void SaveApply_005002(Apply_005002ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            ShareDAO dao = new ShareDAO();
            ClamAdmin UserInfo = sm.UserInfo.Admin;
            string CaseNum = "005002";
            var APP_ID = model.Apply.APP_ID;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    if (string.IsNullOrEmpty(APP_ID))
                    {
                        throw new Exception("案件編號為空");
                    }

                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    this.Tran(conn, tran);
                    DateTime now = DateTime.Now;
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict["APP_ID"] = APP_ID;
                    dict["SRV_ID"] = CaseNum;
                    dict["LastMODTIME"] = now.ToString("yyyyMMddHHmmss");

                    // 申請案件資料
                    var ap = model.Apply;
                    var apply = (ApplyModel)new ApplyModel().InjectFrom(ap);
                    apply.APP_ID = APP_ID;
                    apply.SRV_ID = CaseNum;
                    apply.SRC_SRV_ID = CaseNum;
                    apply.PRO_ACC = UserInfo.ACC_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                    apply.TEL = null;  // 不須更新
                    apply.FAX = null;  // 不須更新

                    if (!model.Apply.PAY_A_FEE.HasValue || model.Apply.PAY_A_FEE.Value == 0)
                    {
                        apply.PAY_A_FEE = model.Detail.MF_COPIES * 1500;
                    }

                    if (model.Pay.PAY_STATUS_MK == "Y")
                    {
                        apply.PAY_A_PAID = model.Detail.MF_COPIES * 1500;
                    }

                    //base.Update(apply, new ApplyModel { APP_ID = APP_ID });
                    base.Update2(apply, new ApplyModel { APP_ID = APP_ID }, dict, true);

                    // 基本資料
                    var dtl = model.Detail;
                    dtl.APP_ID = APP_ID;
                    dtl.DEL_MK = "N";
                    dtl.UPD_TIME = now;
                    dtl.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                    dtl.EMAIL = null;   // 不須更新
                    base.Update2(dtl, new Apply_005002Model { APP_ID = APP_ID }, dict, true);

                    // 清除空值欄位
                    Dictionary<string, object> clearArg = new Dictionary<string, object>();
                    clearArg["APPID"] = APP_ID;
                    List<string> clearList = new List<string>();
                    if (!dtl.EXPIR_DATE.HasValue)
                    {
                        clearList.Add("EXPIR_DATE=NULL");
                    }
                    if (string.IsNullOrEmpty(dtl.EFFICACY))
                    {
                        clearList.Add("EFFICACY=NULL");
                    }
                    if (string.IsNullOrEmpty(dtl.EFFICACY_E))
                    {
                        clearList.Add("EFFICACY_E=NULL");
                    }
                    if (string.IsNullOrEmpty(dtl.INDIOCATION))
                    {
                        clearList.Add("INDIOCATION=NULL");
                    }
                    if (string.IsNullOrEmpty(dtl.INDIOCATION_E))
                    {
                        clearList.Add("INDIOCATION_E=NULL");
                    }

                    if (clearList.Count > 0)
                    {
                        string clearSql = string.Format("UPDATE Apply_005002 SET {0} WHERE APP_ID=@APPID ", string.Join(",", clearList));
                        base.Update(clearSql, clearArg);
                    }

                    if (!string.IsNullOrEmpty(model.Apply.APP_ID) && !string.IsNullOrEmpty(model.Apply.MOHW_CASE_NO))
                    {
                        Dictionary<string, object> caseNoArg = new Dictionary<string, object>();
                        caseNoArg["APPID"] = APP_ID;
                        caseNoArg["CASENO"] = model.Apply.MOHW_CASE_NO;
                        caseNoArg["CASEDATE"] = model.AnoField.MOHW_CASE_DATE;
                        string sql = @" UPDATE OFFICIAL_DOC
                            SET INSERTDATE=convert(datetime, @CASEDATE, 111) 
                            WHERE APP_ID=@APPID AND MOHW_CASE_NO=@CASENO ";
                        int count = base.Update(sql, caseNoArg);

                        if (count == 0)
                        {
                            OFFICIAL_DOC doc = new OFFICIAL_DOC();
                            doc.APP_ID = APP_ID;
                            doc.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO;
                            doc.INSERTDATE = DateTime.ParseExact(model.AnoField.MOHW_CASE_DATE, "yyyy/MM/dd", null);
                            doc.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                            base.Insert(doc);
                        }

                    }

                    // 繳費資料
                    #region 繳費資訊
                    APPLY_PAY pay = new APPLY_PAY();
                    pay.PAY_ID = APP_ID;
                    pay.PAY_EXT_TIME = model.Pay.PAY_EXT_TIME;  // 繳費時間
                    pay.PAY_INC_TIME = now;                     // 異動時間
                    pay.PAY_STATUS_MK = model.Pay.PAY_STATUS_MK;
                    pay.UPD_TIME = DateTime.Now;
                    pay.UPD_FUN_CD = "ADM-STORE";
                    //base.Update(pay, new APPLY_PAY { APP_ID = APP_ID });
                    base.Update2(pay, new APPLY_PAY { APP_ID = APP_ID }, dict, true);
                    #endregion

                    if (APP_ID != null)
                    {
                        if (model.IngredientList != null && model.IngredientList.Count > 0)
                        {
                            int idxIng = 0;
                            base.Delete(new Apply_005002_DiModel { APP_ID = APP_ID });
                            // 成份資料
                            foreach (var item in model.IngredientList)
                            {
                                item.APP_ID = APP_ID;
                                item.SRL_NO = ++idxIng;
                                item.ADD_TIME = now;
                                item.ADD_FUN_CD = "ADM-STORE";
                                item.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                                item.UPD_TIME = now;
                                item.UPD_FUN_CD = "ADM-STORE";
                                item.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                                item.DEL_MK = "N";
                                base.Insert(item);
                            }
                        }

                        if (model.ExcipientList != null && model.ExcipientList.Count > 0)
                        {
                            int idxExc = 0;
                            base.Delete(new Apply_005002_PcModel { APP_ID = APP_ID });
                            // 賦型劑
                            foreach (var item in model.ExcipientList)
                            {
                                item.APP_ID = APP_ID;
                                item.SRL_NO = ++idxExc;
                                item.ADD_TIME = now;
                                item.ADD_FUN_CD = "ADM-STORE";
                                item.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                                item.UPD_TIME = now;
                                item.UPD_FUN_CD = "ADM-STORE";
                                item.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                                item.DEL_MK = "N";
                                base.Insert(item);
                            }
                        }
                    }

                    if (model.ErrataList != null && model.ErrataList.Count > 0)
                    {
                        IList<TblAPPLY_NOTICE> errList = base.GetRowList(new TblAPPLY_NOTICE { APP_ID = APP_ID })
                            .OrderByDescending(x => x.FREQUENCY)
                            .ToList();

                        int maxfreq = 1;
                        if (errList != null && errList.Count > 0)
                        {
                            maxfreq = errList[0].FREQUENCY.Value + 1;
                        }

                        foreach (var item in model.ErrataList.Where(x => x.IsSel).ToList())
                        {
                            TblAPPLY_NOTICE notice = new TblAPPLY_NOTICE();
                            notice.APP_ID = APP_ID;
                            notice.Field = item.Name;
                            notice.Field_NAME = model.FieldNameMap.ContainsKey(item.Name) ? model.FieldNameMap[item.Name] : "";
                            notice.ISADDYN = item.IsSel ? "Y" : "N";
                            notice.NOTE = item.Note;
                            notice.FREQUENCY = maxfreq;
                            notice.ADD_TIME = now;
                            base.Insert(notice);
                        }
                    }

                    if (model.UploadFile_1 != null)
                    {
                        base.Delete(new Apply_FileModel { APP_ID = APP_ID, FILE_NO = 1 });

                        Apply_FileModel file1 = new Apply_FileModel();
                        file1.APP_ID = APP_ID;
                        file1.FILE_NO = 1;
                        file1.FILENAME = dao.PutFile("005002", model.UploadFile_1, "1");
                        file1.UPD_TIME = now;
                        file1.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file1.UPD_FUN_CD = "ADM-STORE";
                        file1.ADD_TIME = now;
                        file1.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file1.ADD_FUN_CD = "ADM-STORE";
                        base.Insert(file1);
                    }

                    if (model.UploadFile_2 != null)
                    {
                        base.Delete(new Apply_FileModel { APP_ID = APP_ID, FILE_NO = 2 });

                        Apply_FileModel file2 = new Apply_FileModel();
                        file2.APP_ID = APP_ID;
                        file2.FILE_NO = 2;
                        file2.FILENAME = dao.PutFile("005002", model.UploadFile_2, "2");
                        file2.UPD_TIME = now;
                        file2.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file2.UPD_FUN_CD = "ADM-STORE";
                        file2.ADD_TIME = now;
                        file2.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file2.ADD_FUN_CD = "ADM-STORE";
                        base.Insert(file2);
                    }


                    if (model.UploadFile_3 != null)
                    {
                        base.Delete(new Apply_FileModel { APP_ID = APP_ID, FILE_NO = 3 });

                        Apply_FileModel file3 = new Apply_FileModel();
                        file3.APP_ID = APP_ID;
                        file3.FILE_NO = 3;
                        file3.FILENAME = dao.PutFile("005002", model.UploadFile_3, "3");
                        file3.UPD_TIME = now;
                        file3.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file3.UPD_FUN_CD = "ADM-STORE";
                        file3.ADD_TIME = now;
                        file3.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file3.ADD_FUN_CD = "ADM-STORE";
                        base.Insert(file3);
                    }

                    if (model.UploadFile_4 != null)
                    {
                        base.Delete(new Apply_FileModel { APP_ID = APP_ID, FILE_NO = 4 });

                        Apply_FileModel file4 = new Apply_FileModel();
                        file4.APP_ID = APP_ID;
                        file4.FILE_NO = 4;
                        file4.FILENAME = dao.PutFile("005002", model.UploadFile_4, "4");
                        file4.UPD_TIME = now;
                        file4.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file4.UPD_FUN_CD = "ADM-STORE";
                        file4.ADD_TIME = now;
                        file4.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file4.ADD_FUN_CD = "ADM-STORE";
                        base.Insert(file4);
                    }

                    if (model.UploadFile_5 != null)
                    {
                        base.Delete(new Apply_FileModel { APP_ID = APP_ID, FILE_NO = 5 });

                        Apply_FileModel file5 = new Apply_FileModel();
                        file5.APP_ID = APP_ID;
                        file5.FILE_NO = 5;
                        file5.FILENAME = dao.PutFile("005002", model.UploadFile_5, "5");
                        file5.UPD_TIME = now;
                        file5.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file5.UPD_FUN_CD = "ADM-STORE";
                        file5.ADD_TIME = now;
                        file5.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                        file5.ADD_FUN_CD = "ADM-STORE";
                        base.Insert(file5);
                    }


                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public string SendMail_005002Notice(Apply_005002ViewModel model)
        {
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            string Msg = "";
            DataTable UndertakerInfo = new DataTable();
            DataTable CaseNoInfo = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    #region 取得承辦人資訊
                    string _sql = @"select ADMIN.NAME,ADMIN.TEL,ADMIN.MAIL
                                    from APPLY join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where APPLY.APP_ID='" + model.Apply.APP_ID + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(UndertakerInfo);
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }

                if (model.Apply.FLOW_CD == "4")
                {
                    try
                    {
                        #region 取得公文資訊
                        string _sql = @"select TOP 1 APP_ID,MOHW_CASE_NO,convert(varchar,INSERTDATE,111) INSERTDATE
                                        from OFFICIAL_DOC
                                        where APP_ID='" + model.Apply.APP_ID + "' and MOHW_CASE_NO='" + model.Apply.MOHW_CASE_NO + @"'
                                        order by INSERTDATE DESC";
                        SqlCommand cmd = new SqlCommand(_sql, conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(CaseNoInfo);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        Msg = "存檔失敗，請聯絡系統管理員 。";
                    }
                }

                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件欄位紀錄
                    // CODE_CD='2' 申請資料待確認
                    // CODE_CD='4' 申請案補件中
                    if (model.Apply.FLOW_CD == "2" || model.Apply.FLOW_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Apply.APP_ID;
                        var andata = GetRowList(anwhere);
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        // 紀錄補件欄位
                        var count = 0;

                        // 比對是否為空值，若不為空則新增至欄位紀錄
                        foreach (var item in andata.Where(x => x.FREQUENCY == times && x.ISADDYN == "Y"))
                        {
                            MainBody += "<tr>";
                            MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.Field_NAME + "</td>";
                            MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.NOTE + "</td>";
                            MainBody += "</tr>";

                            count++;
                            savestatus = true;
                        }

                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            string MailBody = "";
                            if (model.Apply.FLOW_CD == "2")
                            {
                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司外銷證明書(案號：" + model.Apply.APP_ID + ")，經本部審查後，尚有待修正事項，請登入「人民申請案線上申辦服務系統」，依審查建議儘速修正申請表單內容，並於3個工作天內完成；倘未能如期送出修正內容，本部將行文通知補件。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }
                            else if (model.Apply.FLOW_CD == "4")
                            {
                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司外銷證明書(案號：" + model.Apply.APP_ID + ")一案，本部業於" + (Convert.ToInt32(CaseNoInfo.Rows[0][2].ToString().Split('/')[0]) - 1911).ToString() + "年" + CaseNoInfo.Rows[0][2].ToString().Split('/')[1] + "月" + CaseNoInfo.Rows[0][2].ToString().Split('/')[2] + "日以衛部中字第" + model.Apply.MOHW_CASE_NO + "號函，通知貴公司待補正事項，敬請配合辦理。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }

                            var applyModel = base.GetRow(new Apply_005002Model { APP_ID = model.Apply.APP_ID });

                            SendMail_Notice(MainBody, model.Apply.CNT_NAME, count, applyModel.EMAIL.TONotNullString().Replace("@0", "@"), model.Apply.APP_ID, "外銷證明書", "005002", MailBody);
                        }
                    }
                    #endregion

                    if (savestatus)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                        Msg = "請選擇補件項目並輸入備註說明。";
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public void CaseFinishMail_005002(Apply_005002ViewModel model)
        {
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                string MOHDATE = GetMOHCaseDate(model.Apply.APP_ID, model.Apply.MOHW_CASE_NO);
                System.Data.DataTable UndertakerInfo = GetAdminInfo(model.Apply.APP_ID);
                string MailBody = "";

                //結案寄信通知(結案(回函核准)CODE_CD=0;結案(歉難同意)CODE_CD=20)
                if (model.Apply.FLOW_CD == "0")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>本部業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Apply.MOHW_CASE_NO + " 號函，";
                    MailBody += "檢送貴公司申請之外銷證明書(案號:" + model.Apply.APP_ID + ")正本，請查照。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    var applyModel = base.GetRow(new Apply_005002Model { APP_ID = model.Apply.APP_ID });

                    SendMail_Notice("", model.Apply.CNT_NAME, 0, applyModel.EMAIL.TONotNullString().Replace("@0", "@"), model.Detail.APP_ID, "外銷證明書", "005002", MailBody, null, "", "外銷證明書，案件編號:" + model.Apply.APP_ID + "，已結案(回函核准)");
                }

                if (model.Apply.FLOW_CD == "20")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>貴公司申請之外銷證明書(案號:" + model.Apply.APP_ID + ")一案，本部歉難同意，";
                    MailBody += "業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Apply.MOHW_CASE_NO + " 號函回復貴公司。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    var applyModel = base.GetRow(new Apply_005002Model { APP_ID = model.Apply.APP_ID });

                    SendMail_Notice("", model.Apply.CNT_NAME, 0, applyModel.EMAIL.TONotNullString().Replace("@0", "@"), model.Apply.APP_ID, "外銷證明書", "005002", MailBody, null, "", "外銷證明書，案件編號:" + model.Apply.APP_ID + "，已結案(歉難同意)");
                }

                tran.Commit();
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 公文取號&公文日期
        /// </summary>
        /// <param name="model"></param>
        public void GetCaseDate_005002(Apply_005002ViewModel model)
        {
            if (model != null)
            {
                //公文取號
                DataTable offic = QueryOfficial(model.Apply.APP_ID, model.Apply.MOHW_CASE_NO);
                if (offic != null)
                {
                    if (offic.Rows.Count > 0)
                    {
                        model.Apply.MOHW_CASE_NO = offic.Rows[0][0].ToString();
                        if (!string.IsNullOrEmpty(offic.Rows[0][0].ToString()))
                        {
                            model.AnoField.MOHW_CASE_DATE = offic.Rows[0][1].ToString();
                        }
                    }
                }
            }
        }

        #endregion

        #region Apply005005

        public Apply_005005FormModel QueryApply_005005(string app_id)
        {
            Apply_005005FormModel result = new Apply_005005FormModel();
            //附件檔
            DataTable file = QueryApplyFile_005005(app_id);
            //繳費狀態
            DataTable payInfo = QueryPayInfo_005005(app_id);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select distinct STUFF((SELECT ',' + IMP_COUNTRY FROM APPLY_005005_IC WHERE APP_ID='" + app_id + @"' AND DEL_MK='N' FOR XML PATH('')),1, 1, '') IMP_COUNTRY,
										   APPLY.APP_TIME,APPLY.APP_EXT_DATE,APPLY.MOHW_CASE_NO,APPLY.APP_ID,APPLY.CNT_NAME,APPLY.TEL,APPLY.FAX,APPLY_005005.MF_CNT_NAME,
                                           APPLY.APP_TIME,APPLY.NAME,APPLY.PAY_METHOD,APPLY.PAY_A_FEE,APPLY.FLOW_CD CODE_CD,APPLY.EADDR MF_ADDR,
										   ISNULL(ADMIN.NAME,APPLY.PRO_ACC) AS ADMIN_NAME,
										   convert(varchar,APPLY_005005.ISSUE_DATE,111) ISSUE_DATE,APPLY_005005.COMP_NAME,APPLY_005005.LIC_NUM,
                                           convert(varchar,APPLY_005005.EXPIR_DATE,111) EXPIR_DATE, APPLY_005005.RADIOYN,APPLY_005005.EMAIL,APPLY_005005.COPIES
                                    from APPLY left join APPLY_005005 on APPLY.APP_ID = APPLY_005005.APP_ID
		                                       left join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
											   left join APPLY_005005_IC on APPLY.APP_ID = APPLY_005005_IC.APP_ID
                                    where APPLY.APP_ID='" + app_id + "'";
                    result = conn.QueryFirst<Apply_005005FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            result.APPLY_STATUS = GetSchedule(app_id, "09");

            if (file != null)
            {
                if (file.Rows.Count > 0)
                {
                    result.Name_File_1_TEXT = file.Rows[0][0].ToString();
                    result.Name_File_2_TEXT = file.Rows[0][1].ToString();
                }
            }

            if (payInfo != null)
            {
                if (payInfo.Rows.Count > 0)
                {
                    result.PAY_STATUS = payInfo.Rows[0][0].ToString();
                    if (payInfo.Rows[0][0].ToString() == "Y")
                    {
                        result.PAY_ACT_TIME = payInfo.Rows[0][1].ToString();
                    }
                }
            }
            //公文取號
            DataTable offic = QueryOfficial(app_id, result.MOHW_CASE_NO);
            if (offic != null)
            {
                if (offic.Rows.Count > 0)
                {
                    result.MOHW_CASE_NO = offic.Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(offic.Rows[0][0].ToString()))
                    {
                        result.MOHW_CASE_DATE = offic.Rows[0][1].ToString();
                    }
                }
            }
            return result;
        }

        public DataTable QueryApplyFile_005005(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @file1 nvarchar(max),@file2 nvarchar(max),@file3 nvarchar(max),@file4 nvarchar(max);

                                    select @file1 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='1'

                                    select @file2 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='2'

                                    select @file1 file1,@file2 file2";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public DataTable QueryPayInfo_005005(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select PAY_STATUS_MK,CONVERT(varchar,PAY_INC_TIME,111) PAY_INC_TIME
                                    from APPLY_PAY
                                    where APP_ID='" + app_id + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApplyDoc005005(Apply_005005ViewModel model)
        {
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            string Msg = "";
            DataTable UndertakerInfo = new DataTable();
            DataTable CaseNoInfo = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    #region 取得承辦人資訊
                    string _sql = @"select ADMIN.NAME,ADMIN.TEL,ADMIN.MAIL
                                    from APPLY join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where APPLY.APP_ID='" + model.Form.APP_ID + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(UndertakerInfo);
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }

                if (model.Form.CODE_CD == "4")
                {
                    try
                    {
                        #region 取得公文資訊
                        string _sql = @"select TOP 1 APP_ID,MOHW_CASE_NO,convert(varchar,INSERTDATE,111) INSERTDATE
                                        from OFFICIAL_DOC
                                        where APP_ID='" + model.Form.APP_ID + "' and MOHW_CASE_NO='" + model.Form.MOHW_CASE_NO + @"'
                                        order by INSERTDATE DESC";
                        SqlCommand cmd = new SqlCommand(_sql, conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(CaseNoInfo);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        Msg = "存檔失敗，請聯絡系統管理員 。";
                    }
                }

                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件欄位紀錄
                    // CODE_CD='2' 申請資料待確認
                    // CODE_CD='4' 申請案補件中
                    if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        // 紀錄補件欄位
                        var count = 0;

                        // 比對是否為空值，若不為空則新增至欄位紀錄
                        foreach (var item in model.Detail.GetType().GetProperties())
                        {
                            if (item.GetValue(model.Detail) != null && (item.GetValue(model.Detail)).ToString() != "False" && (item.GetValue(model.Detail)).ToString() != "True" && (item.GetValue(model.Detail)).ToString() != "0")
                            {
                                anwhere = new TblAPPLY_NOTICE();
                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.Field = item.Name;
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = item.GetValue(model.Detail).TONotNullString();
                                Insert(anwhere);
                                MainBody += "<tr>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.GetValue(model.Detail).TONotNullString() + "</td>";
                                MainBody += "</tr>";

                                count++;
                                savestatus = true;
                            }
                        }

                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            string MailBody = "";
                            if (model.Form.CODE_CD == "2")
                            {
                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司申請中藥GMP廠證明文件(英文)(案號：" + model.Form.APP_ID + ")，經本部審查後，尚有待修正事項，請登入「人民申請案線上申辦服務系統」，依審查建議盡速修正申請表單內容，並於3個工作天內完成;倘未能如期送出修正內容，本部將行文通知補件。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }
                            else if (model.Form.CODE_CD == "4")
                            {

                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>貴公司申請中藥GMP廠證明文件(英文)(案號：" + model.Form.APP_ID + ")一案，本部業於" + (Convert.ToInt32(CaseNoInfo.Rows[0][2].ToString().Split('/')[0]) - 1911).ToString() + "年" + CaseNoInfo.Rows[0][2].ToString().Split('/')[1] + "月" + CaseNoInfo.Rows[0][2].ToString().Split('/')[2] + "日以衛部中字第" + model.Form.MOHW_CASE_NO + "號函，通知貴公司待補正事項，敬請配合辦理。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }

                            SendMail_Notice(MainBody, model.Form.CNT_NAME, count, model.Form.EMAIL, model.Form.APP_ID, "中藥GMP廠證明文件(英文)", "005005", MailBody);
                        }
                    }
                    #endregion

                    if (savestatus)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                        Msg = "請選擇補件項目並輸入備註說明。";
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 一般存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApply005005(Apply_005005ViewModel model)
        {
            string Msg = "";
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string s_SRV_ID = "005005";
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", model.Form.APP_ID);
                    dict2.Add("SRV_ID", s_SRV_ID);
                    dict2.Add("LastMODTIME", LastMODTIME);

                    #region 寫回Table
                    Apply_005005Model where = new Apply_005005Model();
                    where.APP_ID = model.Form.APP_ID;
                    Apply_005005Model newData = new Apply_005005Model();
                    newData.InjectFrom(model.Form);
                    newData.ADD_TIME = null;
                    newData.UPD_TIME = DateTime.Now;
                    newData.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                    base.Update2(newData, where, dict2, true);

                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);
                    newDataApply.ADD_TIME = null;
                    newDataApply.FLOW_CD = model.Form.CODE_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                    base.Update2(newDataApply, whereApply, dict2, true);

                    Apply_005005_IcModel newApply005005IC = new Apply_005005_IcModel();
                    Apply_005005_IcModel whereApply005005IC = new Apply_005005_IcModel();
                    newApply005005IC.APP_ID = model.Form.APP_ID;
                    newApply005005IC.DEL_MK = "Y";
                    newApply005005IC.DEL_TIME = DateTime.Now;
                    newApply005005IC.DEL_FUN_CD = "WEB-APPLY";
                    //newApply005005IC.DEL_ACC = ;
                    whereApply005005IC.APP_ID = model.Form.APP_ID;

                    base.Update2(newApply005005IC, whereApply005005IC, dict2, true);

                    //建立新資料
                    var impSplit = model.Form.IMP_COUNTRY.Split(',');
                    for (var i = 0; i < impSplit.Length; i++)
                    {
                        newApply005005IC = new Apply_005005_IcModel();
                        newApply005005IC.APP_ID = model.Form.APP_ID;
                        newApply005005IC.SRL_NO = i + 1;
                        newApply005005IC.IMP_COUNTRY = impSplit[i];
                        newApply005005IC.UPD_TIME = DateTime.Now;
                        newApply005005IC.UPD_FUN_CD = "WEB-APPLY";
                        //newApply005005IC.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                        newApply005005IC.ADD_TIME = DateTime.Now;
                        newApply005005IC.ADD_FUN_CD = "WEB-APPLY";
                        //newApply005005IC.ADD_ACC = UserInfo.ACC_NO.TONotNullString();

                        base.Insert(newApply005005IC);
                    }
                    #endregion

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;
                        base.Update2(newDataApply, whereApply, dict2, true);

                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_INC_TIME = Convert.ToDateTime(model.Form.PAY_ACT_TIME);
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;

                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    #region 公文文號
                    if (!string.IsNullOrWhiteSpace(model.Form.MOHW_CASE_NO))
                    {
                        // 是否已有該公文文號
                        OFFICIAL_DOC where_doc = new OFFICIAL_DOC();
                        where_doc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                        where_doc.APP_ID = model.Form.APP_ID;
                        var docOrg = GetRow(where_doc);
                        if (docOrg == null)
                        {
                            // 新增
                            OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                            InsertDoc.APP_ID = model.Form.APP_ID;
                            InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                            InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                            InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                            Insert(InsertDoc);
                        }
                        else
                        {
                            if (docOrg.INSERTDATE >= Convert.ToDateTime(model.Form.MOHW_CASE_DATE) &&
                                Convert.ToDateTime(model.Form.MOHW_CASE_DATE).AddDays(1) >= docOrg.INSERTDATE)
                            {
                                //// 更新
                                //OFFICIAL_DOC where_up = new OFFICIAL_DOC();
                                //where_up.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //where_up.APP_ID = model.Form.APP_ID;
                                //OFFICIAL_DOC UpdateData = new OFFICIAL_DOC();
                                //UpdateData.APP_ID = model.Form.APP_ID;
                                //UpdateData.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //UpdateData.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                //UpdateData.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                //Update2(UpdateData, where_up, dict2, true);
                            }
                            else
                            {
                                // 新增
                                OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                                InsertDoc.APP_ID = model.Form.APP_ID;
                                InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                Insert(InsertDoc);
                            }
                        }
                    }
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_005005FormModel GetApplyNotice_005005(string app_id)
        {
            Apply_005005FormModel result = new Apply_005005FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_005005FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public void CaseFinishMail_005005(Apply_005005ViewModel model)
        {
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                string MOHDATE = GetMOHCaseDate(model.Form.APP_ID, model.Form.MOHW_CASE_NO);
                System.Data.DataTable UndertakerInfo = GetAdminInfo(model.Form.APP_ID);
                string MailBody = "";

                //結案寄信通知(結案(回函核准)CODE_CD=0;結案(歉難同意)CODE_CD=20)
                if (model.Form.CODE_CD == "0")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>本部業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函，";
                    MailBody += "檢送貴公司申請之中藥GMP廠證明文件(英文)(案號:" + model.Form.APP_ID + ")正本，請查照。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "中藥GMP廠證明文件(英文)", "005005", MailBody, null, "", "中藥GMP廠證明文件(英文)，案件編號:" + model.Form.APP_ID + "，已結案(回函核准)");
                }

                if (model.Form.CODE_CD == "20")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>貴公司申請之中藥GMP廠證明文件(英文)(案號:" + model.Form.APP_ID + ")一案，本部歉難同意，";
                    MailBody += "業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函回復貴公司。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "中藥GMP廠證明文件(英文)", "005005", MailBody, null, "", "中藥GMP廠證明文件(英文)，案件編號:" + model.Form.APP_ID + "，已結案(歉難同意)");
                }

                tran.Commit();
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region APPLY_005003 WHO格式之產銷證明書(英文)

        public Apply_005003FormModel QueryApply_005003(string app_id)
        {
            Apply_005003FormModel result = new Apply_005003FormModel();

            Dictionary<string, object> dictionary = new Dictionary<string, object> { { "@APP_ID", app_id } };
            DynamicParameters parameters = new DynamicParameters(dictionary);

            string _sql = @"
    select m.APP_ID,m.NAME,m.CNT_NAME,m.TEL,m.FAX
    ,m.PAY_A_FEE PAYAMOUNT
    ,m.FLOW_CD CODE_CD
    ,m.MOHW_CASE_NO,m.APP_TIME,m.APP_EXT_DATE,m.PAY_A_FEE
    ,m.PAY_METHOD

    ,m2.COPIES PAYCOUNT
    ,m2.EMAIL
    ,m2.MF_ADDR ,m2.F_1,m2.F_1_2,m2.F_1_3 ,m2.F_2A_1_NUM
    ,m2.F_2A_1_DATE 
    ,m2.F_2A_2,m2.F_2A_3,m2.F_2A_4,m2.F_2A_5
    ,m2.F_2A_6_NAME,m2.F_2A_6_ADDR ,m2.F_2B_1_NAME,m2.F_2B_2_ADDR
    ,m2.F_2B_2,m2.F_2B_3 ,m2.F_2A_3_1_NAME ,m2.F_2A_3_2_ADDR
    ,m2.F_2A_3_3_REMARKS ,m2.F_3_0,m2.F_3_1,m2.F_3_2,m2.F_3_3
    ,m2.F_4 ,m2.COPIES,m2.ATTACH_1 
    ,m2.F_1_DF ,m2.F_1_1 ,m2.F_2A_6
    ,m2.F_2B_3_REMARKS ,m2.F_2A_1_WORD ,m2.F_2A_2_COMM,m2.F_2A_2_ADDR
    ,m2.MERGEYN
    /* ,m2.FILE_LICF,m2.FILE_LICB,m2.FILE_CHART */

    ,ISNULL(aa.NAME,m.PRO_ACC) AS ADMIN_NAME
    FROM APPLY m
    JOIN APPLY_005003 m2 on m2.APP_ID =m.APP_ID 
    LEFT JOIN ADMIN aa on aa.ACC_NO = m.PRO_ACC 
    WHERE m.APP_ID= @APP_ID";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_005003FormModel>(_sql, parameters);
                conn.Close();
                conn.Dispose();
            }
            result.APPLY_STATUS = GetSchedule(app_id, "09");

            //附件檔
            Apply_005003FormModel file = QueryApplyFile_005003(app_id);
            if (file != null)
            {
                result.FILE_LICF_TEXT = file.FILE_LICF_TEXT.TONotNullString();
                result.FILE_LICB_TEXT = file.FILE_LICB_TEXT.TONotNullString();
                result.FILE_CHART_TEXT = file.FILE_CHART_TEXT.TONotNullString();
            }

            //繳費狀態
            APPLY_PAY pay = QueryPayInfo_005003(app_id);
            //PAY_STATUS_MK,CONVERT(varchar,PAY_INC_TIME,111) PAY_INC_TIME
            bool flag_Pay_show = false;
            if (pay != null) { if (pay.PAY_STATUS_MK != null) { flag_Pay_show = true; } }
            if (flag_Pay_show)
            {
                result.PAY_STATUS = pay.PAY_STATUS_MK;
                if (pay.PAY_STATUS_MK.Equals("Y") && pay.PAY_INC_TIME.HasValue)
                {
                    result.PAY_ACT_TIME = pay.PAY_INC_TIME.Value.ToString("yyyy/MM/dd");
                }
            }
            //公文取號
            DataTable offic = QueryOfficial(app_id, result.MOHW_CASE_NO);
            if (offic != null)
            {
                if (offic.Rows.Count > 0)
                {
                    result.MOHW_CASE_NO = offic.Rows[0][0].ToString();
                    if (!string.IsNullOrEmpty(offic.Rows[0][0].ToString()))
                    {
                        result.MOHW_CASE_DATE = offic.Rows[0][1].ToString();
                    }
                }
            }
            return result;
        }

        public Apply_005003FormModel QueryApplyFile_005003(string APP_ID)
        {
            Apply_005003FormModel result = new Apply_005003FormModel();

            Dictionary<string, object> dictionary = new Dictionary<string, object> { { "@APP_ID", APP_ID } };
            DynamicParameters parameters = new DynamicParameters(dictionary);

            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_LICF_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'2') FILE_LICB_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'3') FILE_CHART_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_005003FormModel>(_sql, parameters);
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public APPLY_PAY QueryPayInfo_005003(string APP_ID)
        {
            APPLY_PAY result = new APPLY_PAY();

            Dictionary<string, object> dictionary = new Dictionary<string, object> { { "@APP_ID", APP_ID } };
            DynamicParameters parameters = new DynamicParameters(dictionary);

            string _sql = @"
     select PAY_STATUS_MK, PAY_INC_TIME 
    from APPLY_PAY 
    where APP_ID=@APP_ID ";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<APPLY_PAY>(_sql, parameters);
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApplyDoc005003(Apply_005003ViewModel model)
        {

            string APP_ID = model.Form.APP_ID;
            string MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
            string s_SRV_ID = "005003";
            string s_SRV_NAME = "WHO格式之產銷證明書(英文)";

            //string s_log1 = "";
            //s_log1 = "\n ##AppendApplyDoc005003: ";
            //s_log1 = string.Format("\n (model.Form.APP_ID)APP_ID:{0}", APP_ID);
            //s_log1 = string.Format("\n (model.Form.MOHW_CASE_NO)MOHW_CASE_NO:{0}", MOHW_CASE_NO);
            //logger.Debug(s_log1);

            // 紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            string Msg = "";
            DataTable AdmInfo = new DataTable();
            AdmInfo = GetAdminInfo(model.Form.APP_ID);
            string s_AdmName = "";
            string s_AdmTel = "";
            string s_AdmEmail = "";
            if (AdmInfo.Rows.Count > 0)
            {
                s_AdmName = Convert.ToString(AdmInfo.Rows[0]["NAME"]);
                s_AdmTel = Convert.ToString(AdmInfo.Rows[0]["TEL"]);
                s_AdmEmail = Convert.ToString(AdmInfo.Rows[0]["MAIL"]);
            }

            string MOHDATE = "";
            string MOHDATE_TW = "";
            if (model.Form.CODE_CD == "4")
            {
                MOHDATE = GetMOHCaseDate(model.Form.APP_ID, model.Form.MOHW_CASE_NO);
                MOHDATE_TW = string.Format(" {0} 年 {1} 月 {2} 日", MOHDATE.Substring(0, 3), MOHDATE.Substring(3, 2), MOHDATE.Substring(5, 2));
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件欄位紀錄
                    // CODE_CD='2' 申請資料待確認
                    // CODE_CD='4' 申請案補件中
                    if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        IList<TblAPPLY_NOTICE> andata = GetRowList(anwhere);
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;

                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        // 紀錄補件欄位
                        var count = 0;

                        // 比對是否為空值，若不為空則新增至欄位紀錄
                        foreach (var item in model.Detail.GetType().GetProperties())
                        {
                            if (item.GetValue(model.Detail) != null &&
                                (item.GetValue(model.Detail)).ToString() != "False" &&
                                (item.GetValue(model.Detail)).ToString() != "True" &&
                                (item.GetValue(model.Detail)).ToString() != "0" && item.Name != "F11")
                            {
                                //s_log1 = "";
                                //try
                                //{
                                //    s_log1 += string.Format("\n item.Name:{0}", item.Name);
                                //    s_log1 += string.Format("\n item.GetValue:{0}", item.GetValue(model.Detail).TONotNullString());
                                //}
                                //catch (Exception ex)
                                //{
                                //    s_log1 += string.Format("\n MainBody:{0}", MainBody);
                                //    if (s_log1 != "") { logger.Debug(s_log1); }
                                //    logger.Error(ex.Message, ex);
                                //    throw;
                                //}

                                anwhere = new TblAPPLY_NOTICE();
                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.Field = item.Name;
                                anwhere.FREQUENCY = (times + 1);
                                anwhere.NOTE = item.GetValue(model.Detail).TONotNullString();
                                Insert(anwhere);

                                MainBody += "<tr>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.GetValue(model.Detail).TONotNullString() + "</td>";
                                MainBody += "</tr>";

                                count++;
                                savestatus = true;
                            }
                        }

                        //CODE_CD :: 0:結案(回函核准) 1:新收案件 2:申請資料待確認 3:審查中 4:申請案補件中 10:已收案，處理中 20:結案(歉難同意)
                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            string MailBody = "";
                            if (model.Form.CODE_CD == "2")
                            {
                                string msg_T1 = string.Format(" <tr><td>貴公司申請 {0} (案號：{1})", s_SRV_NAME, model.Form.APP_ID);
                                string msg_T2 = "，經本部審查後，尚有待修正事項，請登入「人民申請案線上申辦服務系統」，依審查建議盡速修正申請表單內容，並於3個工作天內完成;倘未能如期送出修正內容，本部將行文通知補件。</td></tr>";

                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += msg_T1;
                                MailBody += msg_T2;
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += string.Format(" <tr><td>承辦人:{0}</td></tr>", s_AdmName);
                                MailBody += string.Format(" <tr><td>連絡電話:{0}</td></tr>", s_AdmTel);
                                MailBody += string.Format(" <tr><td>電子郵件:{0}</td></tr>", s_AdmEmail);
                                MailBody += "</table>";
                            }
                            else if (model.Form.CODE_CD == "4")
                            {

                                string msg_T1 = string.Format(" <tr><td>貴公司申請 {0} (案號：{1})一案", s_SRV_NAME, model.Form.APP_ID);
                                string msg_T2 = string.Format("，本部業於 {0} 以衛部中字第 {1} 號函，通知貴公司待補正事項，敬請配合辦理。</td></tr> ", MOHDATE_TW, model.Form.MOHW_CASE_NO);

                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += msg_T1;
                                MailBody += msg_T2;
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                MailBody += string.Format(" <tr><td>承辦人:{0}</td></tr>", s_AdmName);
                                MailBody += string.Format(" <tr><td>連絡電話:{0}</td></tr>", s_AdmTel);
                                MailBody += string.Format(" <tr><td>電子郵件:{0}</td></tr>", s_AdmEmail);
                                MailBody += "</table>";
                            }

                            SendMail_Notice(MainBody, model.Form.CNT_NAME, count, model.Form.EMAIL, model.Form.APP_ID, s_SRV_NAME, s_SRV_ID, MailBody);
                        }
                    }
                    #endregion

                    if (savestatus)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                        Msg = "請選擇補件項目並輸入備註說明。";
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 一般存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApply005003(Apply_005003ViewModel model)
        {
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", "005003");
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string Msg = "";
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 寫回Table
                    Apply_005003Model where = new Apply_005003Model();
                    where.APP_ID = model.Form.APP_ID;

                    Apply_005003Model newData = new Apply_005003Model();
                    newData.InjectFrom(model.Form);
                    newData.APP_ID = model.Form.APP_ID;

                    //newData.MF_CNT_TEL = model.Form.TEL;
                    //newData.MF_ADDR = model.Form.TAX_ORG_CITY_CODE + model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL;
                    //newData.LIC_CD = model.Form.PL_CD;
                    //newData.LIC_NUM = model.Form.PL_Num;
                    //newData.LIC_CD_E = model.Form.PL_CD_E;
                    //newData.LIC_NUM_E = model.Form.PL_Num_E;
                    //newData.ISSUE_DATE = Convert.ToDateTime(model.Form.ISSUE_DATE);
                    //if (string.IsNullOrEmpty(model.Form.EXPIR_DATE))
                    //{
                    //    newData.EXPIR_DATE = null;
                    //}
                    //else
                    //{
                    //    newData.EXPIR_DATE = Convert.ToDateTime(model.Form.EXPIR_DATE);
                    //}
                    //newData.MF_COPIES = model.Form.PAYCOUNT.TOInt32();

                    newData.ADD_TIME = DateTime.Now;
                    newData.ADD_FUN_CD = "WEB-APPLY";
                    newData.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                    newData.UPD_TIME = DateTime.Now;
                    newData.UPD_FUN_CD = "WEB-APPLY";
                    newData.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                    newData.DEL_MK = "N";
                    //newData.EMAIL = model.Form.EMAIL;
                    // base.Update(newData, where);
                    base.Update2(newData, where, dict2, true);

                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);
                    newDataApply.FLOW_CD = model.Form.CODE_CD;
                    //base.Update(newDataApply, whereApply);
                    base.Update2(newDataApply, whereApply, dict2, true);
                    #endregion

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = model.Form.PAYAMOUNT;
                        newDataApply.UPD_TIME = DateTime.Now;
                        base.Update(newDataApply, whereApply);

                        // apply_pay
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        //newDataPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.Form.PAY_EXT_TIME); //DateTime.Now;
                        newDataPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.Form.PAY_ACT_TIME);
                        newDataPay.UPD_TIME = DateTime.Now;
                        newDataPay.UPD_FUN_CD = "WEB-APPLY";
                        newDataPay.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                        base.Update(newDataPay, wherePay);
                    }
                    #endregion

                    #region APPLY_005003_F11
                    model.F11.APP_ID = model.Form.APP_ID;
                    foreach (var item in model.F11.GoodsList)
                    {
                        item.APP_ID = model.Form.APP_ID;
                        item.ADD_TIME = DateTime.Now;
                        item.ADD_FUN_CD = "WEB-APPLY";
                        item.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                        item.UPD_TIME = DateTime.Now;
                        item.UPD_FUN_CD = "WEB-APPLY";
                        item.UPD_ACC = adminInfo.ACC_NO.TONotNullString();
                        item.DEL_MK = "N";
                    }
                    model.F11.SaveGoodsList("SRL_NO");
                    #endregion

                    #region 公文文號
                    if (!string.IsNullOrWhiteSpace(model.Form.MOHW_CASE_NO))
                    {
                        // 是否已有該公文文號
                        OFFICIAL_DOC where_doc = new OFFICIAL_DOC();
                        where_doc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                        where_doc.APP_ID = model.Form.APP_ID;
                        var docOrg = GetRow(where_doc);
                        if (docOrg == null)
                        {
                            // 新增
                            OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                            InsertDoc.APP_ID = model.Form.APP_ID;
                            InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                            InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                            InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                            Insert(InsertDoc);
                        }
                        else
                        {
                            if (docOrg.INSERTDATE >= Convert.ToDateTime(model.Form.MOHW_CASE_DATE) &&
                                Convert.ToDateTime(model.Form.MOHW_CASE_DATE).AddDays(1) >= docOrg.INSERTDATE)
                            {
                                //// 更新
                                //OFFICIAL_DOC where_up = new OFFICIAL_DOC();
                                //where_up.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //where_up.APP_ID = model.Form.APP_ID;
                                //OFFICIAL_DOC UpdateData = new OFFICIAL_DOC();
                                //UpdateData.APP_ID = model.Form.APP_ID;
                                //UpdateData.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                //UpdateData.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                //UpdateData.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                //Update2(UpdateData, where_up, dict2, true);
                            }
                            else
                            {
                                // 新增
                                OFFICIAL_DOC InsertDoc = new OFFICIAL_DOC();
                                InsertDoc.APP_ID = model.Form.APP_ID;
                                InsertDoc.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                                if (model.Form.MOHW_CASE_DATE != null)
                                {
                                    InsertDoc.INSERTDATE = Convert.ToDateTime(model.Form.MOHW_CASE_DATE);
                                }
                                InsertDoc.ADD_ACC = adminInfo.ACC_NO.TONotNullString();
                                Insert(InsertDoc);
                            }
                        }
                    }
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_005003FormModel GetApplyNotice_005003(string app_id)
        {
            Apply_005003FormModel result = new Apply_005003FormModel();

            string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    result = conn.QueryFirst<Apply_005003FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public void CaseFinishMail_005003(Apply_005003ViewModel model)
        {
            string s_SRV_ID = "005003";
            string s_SRV_NAME = "WHO格式之產銷證明書(英文)";

            string MOHDATE = GetMOHCaseDate(model.Form.APP_ID, model.Form.MOHW_CASE_NO);
            System.Data.DataTable UndertakerInfo = GetAdminInfo(model.Form.APP_ID);

            //DataTable AdmInfo = new DataTable();
            //AdmInfo = GetAdminInfo(model.Form.APP_ID);
            //string s_AdmName = "";
            //string s_AdmTel = "";
            //string s_AdmEmail = "";
            //if (AdmInfo.Rows.Count > 0)
            //{
            //    s_AdmName = Convert.ToString(AdmInfo.Rows[0]["NAME"]);
            //    s_AdmTel = Convert.ToString(AdmInfo.Rows[0]["TEL"]);
            //    s_AdmEmail = Convert.ToString(AdmInfo.Rows[0]["MAIL"]);
            //}

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                string MailBody = "";

                //結案寄信通知(結案(回函核准)CODE_CD=0;結案(歉難同意)CODE_CD=20)
                if (model.Form.CODE_CD == "0")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>本部業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函，";
                    MailBody += "檢送貴公司申請之WHO格式之產銷證明書(英文)(案號:" + model.Form.APP_ID + ")正本，請查照。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, s_SRV_NAME, "005003", MailBody, null, "", "WHO格式之產銷證明書(英文)，案件編號:" + model.Form.APP_ID + "，已結案(回函核准)");
                }

                if (model.Form.CODE_CD == "20")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>貴公司申請之WHO格式之產銷證明書(英文)(案號:" + model.Form.APP_ID + ")一案，本部歉難同意，";
                    MailBody += "業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函回復貴公司。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.CNT_NAME, 0, model.Form.EMAIL, model.Form.APP_ID, s_SRV_NAME, "005003", MailBody, null, "", "WHO格式之產銷證明書(英文)，案件編號:" + model.Form.APP_ID + "，已結案(歉難同意)");
                }

                tran.Commit();

                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 介接
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Apply_005003RespModel SortData005003(Apply_005003RespModel data)
        {
            DataTable result = new DataTable();
            //using (SqlConnection conn = DataUtils.GetConnection())
            //{
            //    try
            //    {
            //        string _sql = @"select DISTINCT (CITYNM + TOWNNM) CITY from ZIPCODE where ZIP_CO like '" + data.郵遞區號 + "%'";
            //        SqlCommand cmd = new SqlCommand(_sql, conn);
            //        SqlDataAdapter da = new SqlDataAdapter(cmd);
            //        da.Fill(result);
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Warn(ex.Message, ex);
            //        result = null;
            //    }
            //}

            //地址轉換
            //data.營業地址 = data.營業地址.Replace(result.Rows[0][0].ToString(), "");

            //發證日期(轉民國)
            if (!string.IsNullOrEmpty(data.發證日期))
            {
                data.發證日期_英文 = data.發證日期;
                data.發證日期 = Convert.ToInt32(data.發證日期.Split('/')[0]) - 1911 + "/" +
                            data.發證日期.Split('/')[1] + "/" +
                            data.發證日期.Split('/')[2];
            }

            //有效日期(轉民國)
            if (!string.IsNullOrEmpty(data.有效日期))
            {
                data.有效日期 = Convert.ToInt32(data.有效日期.Split('/')[0]) - 1911 + "/" +
                            data.有效日期.Split('/')[1] + "/" +
                            data.有效日期.Split('/')[2];
            }

            return data;
        }

        #endregion

        #region Apply001034 危險性醫療儀器進口申請案件

        /// <summary>
        /// 取得危險性醫療儀器進口申請案件
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001034ViewModel QueryApply_001034(Apply_001034ViewModel parm)
        {
            Apply_001034ViewModel result = new Apply_001034ViewModel();
            result.Apply = new ApplyModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {

                    string _sql = @"SELECT APP_ID, TAX_ORG_ID, TAX_ORG_NAME, TAX_ORG_ENAME, TAX_ZIP_CODE, TAX_ORG_ADDR, TAX_ORG_EADDR, TAX_ORG_MAN, TAX_ORG_TEL, TAX_ORG_EMAIL,
                                TAX_ORG_FAX, IM_EXPORT, DATE_S, DATE_E, DEST_STATE_ID, DEST_STATE, SELL_STATE_ID, SELL_STATE, TRN_PORT_ID, TRN_PORT, BEG_PORT_ID, BEG_PORT,
                                SELL_NAME, SELL_ADDR, APP_USE_ID, APP_USE, USE_MARK, CONF_TYPE_ID, CONF_TYPE, COPIES, MDOD_APPNO, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME,
                                UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC,MAIL_DATE,MAIL_BARCODE
                             FROM APPLY_001034
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001034ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        // 查詢承辦人姓名 20210513 改寫
                        AdminModel where_admin = new AdminModel();
                        where_admin.ACC_NO = proAcc.TONotNullString();
                        result.Admin = GetRow(where_admin);
                        if (string.IsNullOrEmpty(result.Admin.NAME))
                        {
                            result.Admin.NAME = proAcc.TONotNullString();
                        }
                        //dictionary = new Dictionary<string, object>
                        //{
                        //    { "@ACC_NO", proAcc }
                        //};
                        //parameters = new DynamicParameters(dictionary);

                        //_sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                        //        DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        //        FROM ADMIN
                        //    WHERE 1=1";
                        //_sql += " AND ACC_NO = @ACC_NO";
                        //result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }

                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);

                    _sql = @"SELECT APP_ID,SRL_NO,GOODS_TYPE_ID,GOODS_TYPE,GOODS_ID,GOODS_NAME,APPLY_CNT,GOODS_UNIT_ID,GOODS_UNIT,GOODS_MODEL,GOODS_SPEC_1,GOODS_SPEC_2,GOODS_BRAND,
	                           GOODS_DESC,DOC_TYP_01,DOC_COD_01,DOC_TXT_01,DOC_TYP_02,DOC_COD_02,DOC_TXT_02,DOC_TYP_03,DOC_COD_03,DOC_TXT_03,DOC_TYP_04,DOC_COD_04,DOC_TXT_04,
	                           DOC_TYP_05,DOC_COD_05,DOC_TXT_05,DOC_TYP_06,DOC_COD_06,DOC_TXT_06,DOC_TYP_07,DOC_COD_07,DOC_TXT_07,DOC_TYP_08,DOC_COD_08,DOC_TXT_08,DOC_TYP_09,
	                           DOC_COD_09,DOC_TXT_09,DOC_TYP_10,DOC_COD_10,DOC_TXT_10,DOC_TYP_11,DOC_COD_11,DOC_TXT_11,DOC_TYP_12,DOC_COD_12,DOC_TXT_12,DOC_TYP_13,DOC_COD_13,
	                           DOC_TXT_13,DOC_TYP_14,DOC_COD_14,DOC_TXT_14,DOC_TYP_15,DOC_COD_15,DOC_TXT_15,DOC_TYP_16,DOC_COD_16,DOC_TXT_16,DOC_TYP_17,DOC_COD_17,DOC_TXT_17,
	                           DOC_TYP_18,DOC_COD_18,DOC_TXT_18,DOC_TYP_19,DOC_COD_19,DOC_TXT_19,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC
                             FROM APPLY_001034_GOODS 
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Goods = conn.Query<Apply_001034_GoodsViewModel>(_sql, parameters).ToList<Apply_001034_GoodsViewModel>();

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Files = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>();

                    _sql = @"SELECT APP_ID, Field, ISADDYN, FREQUENCY, ADD_TIME, DEADLINE, NOTE, SRC_NO, BATCH_INDEX
                             FROM APPLY_NOTICE AS A
                             WHERE ISADDYN='Y' AND FREQUENCY = (SELECT MAX(FREQUENCY) FROM APPLY_NOTICE WHERE APP_ID=A.APP_ID) ";
                    _sql += " AND APP_ID = @APP_ID";

                    result.Notices = conn.Query<TblAPPLY_NOTICE>(_sql, parameters).ToList<TblAPPLY_NOTICE>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);

                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);

                //起始日期
                result.DATE_S_AC = HelperUtil.DateTimeToString(result.DATE_S);

                //終止日期
                result.DATE_E_AC = HelperUtil.DateTimeToString(result.DATE_E);

                //性別
                result.GENDER = result.Apply.SEX_CD == "M" ? "男" : "女";

                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);

                //郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                // 電話
                string[] tel = result.TAX_ORG_TEL.Split('-');
                if (result.TAX_ORG_TEL.TONotNullString().Trim() != "")
                {
                    result.TEL_SEC = tel[0];
                    if (tel.ToCount() > 1)
                    {
                        result.TEL_NO = tel[1];

                        if (result.TAX_ORG_TEL.IndexOf('#') > 0)
                        {
                            result.TEL_EXT = result.TAX_ORG_TEL.Split('#')[1];
                        }
                    }
                }

                // 傳真
                string[] fax = result.TAX_ORG_FAX.Split('-');
                if (result.TAX_ORG_FAX.TONotNullString().Trim() != "")
                {
                    result.FAX_SEC = tel[0];
                    if (tel.ToCount() > 1)
                    {
                        result.FAX_NO = tel[1];

                        if (result.TAX_ORG_FAX.IndexOf('#') > 0)
                        {
                            result.FAX_EXT = result.TAX_ORG_FAX.Split('#')[1];
                        }
                    }
                }

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.TAX_ZIP_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.TAX_ORG_ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }

                // 聯絡地址
                zip = new TblZIPCODE();
                zip.ZIP_CO = result.TAX_ZIP_CODE;
                address = dao.GetRow(zip);
                result.TAX_ORG_CITY_CODE = result.TAX_ZIP_CODE;
                if (address != null)
                {
                    result.TAX_ORG_CITY_TEXT = address.TOWNNM;
                    result.TAX_ORG_CITY_DETAIL = result.TAX_ORG_ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }

                //Mail
                if (result.MAIL.TONotNullString().Trim() != "")
                {
                    result.MAIL_ACCOUNT = result.MAIL.Split('@')[0];
                    result.MAIL_DOMAIN = result.MAIL.Split('@')[1];

                    switch (result.MAIL.Split('@')[1])
                    {
                        case "gmail.com":
                            result.DOMAINList = "1";
                            break;
                        case "yahoo.com.tw":
                            result.DOMAINList = "2";
                            break;
                        case "outlook.com":
                            result.DOMAINList = "3";
                            break;
                        default:
                            result.DOMAINList = "0";
                            break;
                    }
                }

            }

            //result.AuxView = this.GetApplyNotice_001034(parm.APP_ID);
            var goodList = this.GetApplyGoodsNotice_001034(parm.APP_ID);
            int? rowCount = 1;

            foreach (var auxView in goodList)
            {
                result.Goods.Where(x => x.SRL_NO == rowCount).FirstOrDefault().GoodsAuxView = auxView;
                rowCount++;
            }

            return result;
        }

        /// <summary>
        /// 取得危險性醫療儀器進口申請案件補件存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply001034(Apply_001034ViewModel model)
        {
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001034");
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;

            Apply_001034ViewModel apply001034 = new Apply_001034ViewModel();
            apply001034.APP_ID = model.APP_ID;

            if (model.Apply.FLOW_CD == "12")
            {
                apply001034 = this.GetRow(apply001034);
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件內容
                    TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                    where.APP_ID = model.APP_ID;
                    var noticeList = GetRowList(where);

                    var newNoticeList = from notice in noticeList
                                        orderby notice.FREQUENCY descending
                                        select notice;

                    var times = newNoticeList.ToCount() == 0 ? 0 : newNoticeList.FirstOrDefault().FREQUENCY.TOInt32();

                    // 紀錄補件欄位
                    var count = 0;

                    foreach (var item in model.AuxView.GetType().GetProperties())
                    {
                        if (item.GetValue(model.AuxView) != null && (item.GetValue(model.AuxView)).ToString() != "True" && (item.GetValue(model.AuxView)).ToString() != "False")
                        {
                            where = new TblAPPLY_NOTICE();
                            where.ADD_TIME = DateTime.Now;
                            where.APP_ID = model.APP_ID;
                            where.ISADDYN = "N";
                            where.Field = item.Name;
                            where.FREQUENCY = times + 1;
                            where.NOTE = item.GetValue(model.AuxView).TONotNullString();
                            where.Field_NAME = (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value.ToString();
                            Insert(where);

                            MainBody += "<tr>";
                            MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                            MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.GetValue(model.AuxView).TONotNullString() + "</td>";
                            MainBody += "</tr>";

                            count++;
                            savestatus = true;

                        }
                    }

                    for (int i = 0; i < model.Goods.Count; i++)
                    {
                        foreach (var item in model.Goods[i].GoodsAuxView.GetType().GetProperties())
                        {
                            if (item.GetValue(model.Goods[i].GoodsAuxView) != null && (item.GetValue(model.Goods[i].GoodsAuxView)).ToString() != "True" && (item.GetValue(model.Goods[i].GoodsAuxView)).ToString() != "False")
                            {
                                where = new TblAPPLY_NOTICE();
                                where.ADD_TIME = DateTime.Now;
                                where.APP_ID = model.APP_ID;
                                where.ISADDYN = "N";
                                where.Field = item.Name;
                                where.FREQUENCY = times + 1;
                                where.NOTE = item.GetValue(model.Goods[i].GoodsAuxView).TONotNullString();
                                //where.SRC_NO = 
                                where.BATCH_INDEX = model.Goods[i].SRL_NO;
                                where.Field_NAME = (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value.ToString();
                                Insert(where);

                                MainBody += "<tr>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (item.CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.GetValue(model.Goods[i].GoodsAuxView).TONotNullString() + "</td>";
                                MainBody += "</tr>";

                                count++;
                                savestatus = true;
                            }
                        }
                    }
                    #endregion

                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    ApplyModel apply = new ApplyModel();
                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.FLOW_CD = model.Apply.FLOW_CD;
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.NOTICE_NOTE = model.Note.TONotNullString();

                    if (model.Apply.FLOW_CD == "2" && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                    }
                    else
                    {
                        //this.Update(apply, whereApply);
                        base.Update2(apply, whereApply, dict2, true);
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        Apply_001034Model whereApply001034 = new Apply_001034Model();
                        whereApply001034.APP_ID = model.APP_ID;
                        Apply_001034Model apply001034Model = new Apply_001034Model();

                        apply001034Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AC);
                        apply001034Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001034Model.UPD_TIME = DateTime.Now;
                        apply001034Model.UPD_FUN_CD = "ADM-STORE";
                        apply001034Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001034Model.DEL_MK = "N";
                        //this.Update(apply001034Model, whereApply001034);
                        base.Update2(apply001034Model, whereApply001034, dict2, true);
                    }
                    #endregion

                    #region 依據案件狀態寄發信件內容
                    // 判斷是否要寄信
                    if (model.Apply.FLOW_CD == "--")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001034Model apply001034Model = new Apply_001034Model();
                        apply001034Model.APP_ID = model.APP_ID;
                        apply001034Model = this.GetRow(apply001034Model);

                        SendMail_InPorcess(applyModel.NAME, "危險性醫療儀器進口", "001034", apply001034Model.TAX_ORG_EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "9")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001034Model apply001034Model = new Apply_001034Model();
                        apply001034Model.APP_ID = model.APP_ID;
                        apply001034Model = this.GetRow(apply001034Model);

                        SendMail_Expired(applyModel.NAME, "危險性醫療儀器進口", "001034", apply001034Model.TAX_ORG_EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note);
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001034Model apply001034Model = new Apply_001034Model();
                        apply001034Model.APP_ID = model.APP_ID;
                        apply001034Model = this.GetRow(apply001034Model);

                        SendMail_Archive(applyModel.NAME, "危險性醫療儀器進口", "001034", apply001034Model.TAX_ORG_EMAIL,
                                        applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                        (HelperUtil.TransToDateTime(model.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                        model.MAIL_BARCODE);
                    }
                    if (model.Apply.FLOW_CD == "15")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001005Model apply001034Model = new Apply_001005Model();
                        apply001034Model.APP_ID = model.APP_ID;
                        apply001034Model = this.GetRow(apply001034Model);

                        SendMail_Cancel(applyModel.NAME, "危險性醫療儀器進口", "001034", apply001034Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                    }
                    //if (savestatus)
                    //{
                    //    SendMail_Notice(MainBody, model.TAX_ORG_NAME, count, model.TAX_ORG_EMAIL, model.APP_ID, "危險性醫療儀器進口", "001034");
                    //}
                    #endregion

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        public Apply_001034ViewModel GetApplyNotice_001034(string app_id)
        {
            Apply_001034ViewModel result = new Apply_001034ViewModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where ISADDYN='Y' AND APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"') and BATCH_INDEX is null
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where ISADDYN=''Y'' AND APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'') and BATCH_INDEX is null
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_001034ViewModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public List<Apply_001034_GoodsViewModel> GetApplyGoodsNotice_001034(string app_id)
        {
            List<Apply_001034_GoodsViewModel> result = new List<Apply_001034_GoodsViewModel>();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where ISADDYN='Y' AND APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"') and BATCH_INDEX is not null
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where ISADDYN=''Y'' AND APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'') and BATCH_INDEX is not null
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p ORDER BY GRP ;'

                                                            EXEC sp_executesql  @ColumnGroup";

                    result = conn.Query<Apply_001034_GoodsViewModel>(_sql).ToList();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply001039 醫師赴國外訓練英文保證函

        /// <summary>
        /// 醫師赴國外訓練英文保證函
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001039FormModel QueryApply_001039(Apply_001039FormModel parm)
        {
            Apply_001039FormModel result = new Apply_001039FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql =
                    @"select app.SRV_ID, app.APP_TIME as APPLY_DATE, (convert(varchar, app.APP_TIME, 111))as APPLY_DATE_STR, app.APP_EXT_DATE AS APP_EXT,  (convert(varchar, app.APP_EXT_DATE, 111)) as APP_EXT_DATE,
								isnull(ad.NAME,app.PRO_ACC) as PRO_NAME, app.APP_ID, a1039.CNAME, a1039.ENAME, a1039.PID,a1039.BIRTHDAY, case when   a1039.GENDER = 'M' then '男' when a1039.GENDER = 'F' then '女' else '其他' end as GENDER ,a1039.ECFMG,a1039.HOSPITAL_DIVISION,a1039.COUNTRY,
								a1039.CAUSE,a1039.MAIL_ADDRESS,app.CNT_TEL as CONTACT_TEL,a1039.CONTACT_NAME,a1039.CONTACT_MOBILE,a1039.E_MAIL,a1039.CONTACT_FAX,app.FLOW_CD,  a1039.IS_MERGE_FILE, app.MOHW_CASE_NO,
                                a1039.MAIL_DATE,a1039.MAIL_BARCODE,app.ISMODIFY,app.NOTICE_NOTE as NOTE
								from apply app
								 left join ADMIN ad on app.PRO_ACC = ad.ACC_NO
                                left join APPLY_001039 a1039 on app.APP_ID = a1039.APP_ID
                                where 1 = 1";
                _sql += "and app.app_id = '" + parm.APP_ID + "'";

                try
                {
                    result = conn.QueryFirst<Apply_001039FormModel>(_sql);
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                    //郵寄日期
                    result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001039FileModel GetFileList_001039(string APP_ID)
        {
            var result = new Apply_001039FileModel();


            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"select 
                                (convert(nvarchar,SUBSTRING(fl1.FILENAME,16,LEN(fl1.FILENAME)))+','+ convert(varchar,fl1.APP_ID)+','+convert(varchar,fl1.FILE_NO)+','+isnull(convert(varchar,fl1.SRC_NO),'0')) as FILE0_TEXT,
                                (convert(nvarchar,SUBSTRING(fl2.FILENAME,16,LEN(fl2.FILENAME)))+','+ convert(varchar,fl2.APP_ID)+','+convert(varchar,fl2.FILE_NO)+','+isnull(convert(varchar,fl2.SRC_NO),'0')) as FILE1_TEXT,
                                (convert(nvarchar,SUBSTRING(fl3.FILENAME,16,LEN(fl3.FILENAME)))+','+ convert(varchar,fl3.APP_ID)+','+convert(varchar,fl3.FILE_NO)+','+isnull(convert(varchar,fl3.SRC_NO),'0')) as FILE2_TEXT,
                                (convert(nvarchar,SUBSTRING(fl4.FILENAME,16,LEN(fl4.FILENAME)))+','+ convert(varchar,fl4.APP_ID)+','+convert(varchar,fl4.FILE_NO)+','+isnull(convert(varchar,fl4.SRC_NO),'0')) as FILE3_TEXT,     
								(convert(nvarchar,SUBSTRING(fl5.FILENAME,16,LEN(fl5.FILENAME)))+','+ convert(varchar,fl5.APP_ID)+','+convert(varchar,fl5.FILE_NO)+','+isnull(convert(varchar,fl5.SRC_NO),'0')) as FILE4_TEXT,  
								(convert(nvarchar,SUBSTRING(fl6.FILENAME,16,LEN(fl6.FILENAME)))+','+ convert(varchar,fl6.APP_ID)+','+convert(varchar,fl6.FILE_NO)+','+isnull(convert(varchar,fl6.SRC_NO),'0')) as FILE5_TEXT,  
								(convert(nvarchar,SUBSTRING(fl7.FILENAME,16,LEN(fl7.FILENAME)))+','+ convert(varchar,fl7.APP_ID)+','+convert(varchar,fl7.FILE_NO)+','+isnull(convert(varchar,fl7.SRC_NO),'0')) as FILE6_TEXT,  
								(convert(nvarchar,SUBSTRING(fl8.FILENAME,16,LEN(fl8.FILENAME)))+','+ convert(varchar,fl8.APP_ID)+','+convert(varchar,fl8.FILE_NO)+','+isnull(convert(varchar,fl8.SRC_NO),'0')) as FILE7_TEXT,  
								(convert(nvarchar,SUBSTRING(fl9.FILENAME,16,LEN(fl9.FILENAME)))+','+ convert(varchar,fl9.APP_ID)+','+convert(varchar,fl9.FILE_NO)+','+isnull(convert(varchar,fl9.SRC_NO),'0')) as FILE8_TEXT,
                                (convert(nvarchar,SUBSTRING(fl10.FILENAME,16,LEN(fl10.FILENAME)))+','+ convert(varchar,fl10.APP_ID)+','+convert(varchar,fl10.FILE_NO)+','+isnull(convert(varchar,fl10.SRC_NO),'0')) as FILE9_TEXT  
                                from apply app";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '1' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl1 on app.APP_ID = fl1.APP_ID ";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '2' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl2 on app.APP_ID = fl2.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '3' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl3 on app.APP_ID = fl3.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '4' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl4 on app.APP_ID = fl4.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '5' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl5 on app.APP_ID = fl5.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '6' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl6 on app.APP_ID = fl6.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '7' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl7 on app.APP_ID = fl7.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '8' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl8 on app.APP_ID = fl8.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '9' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl9 on app.APP_ID = fl9.APP_ID";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '10' and APP_ID = '" + APP_ID + "' order by ADD_TIME desc)as fl10 on app.APP_ID = fl10.APP_ID";
                _sql += " where 1 = 1";
                _sql += " and app.app_id = '" + APP_ID + "'";

                try
                {
                    result = conn.QueryFirst<Apply_001039FileModel>(_sql);
                    result.APP_ID = APP_ID;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 醫師赴國外訓練英文保證函-存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply001039(Apply_001039ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", "001039");
            dict2.Add("LastMODTIME", LastMODTIME);
            SessionModel sm = SessionModel.Get();
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";

            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001039Model apply001039 = new Apply_001039Model();
            apply001039.APP_ID = model.Form.APP_ID;

            if (model.Form.FLOW_CD == "12")
            {
                apply001039 = this.GetRow(apply001039);
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.Form.FLOW_CD == "2")
                    {
                        #region 補件內容
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);
                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();
                        var ProjectStr1 = string.Empty;
                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">";
                        foreach (var item in model.Form.chkNotice.TONotNullString().ToSplit(','))
                        {
                            bool isOther = false;
                            anwhere = new TblAPPLY_NOTICE();
                            anwhere.ADD_TIME = DateTime.Now;
                            anwhere.APP_ID = model.Form.APP_ID;
                            anwhere.ISADDYN = "N";
                            switch (item.ToString())
                            {
                                case "0":
                                    anwhere.Field = "FILE0";
                                    anwhere.Field_NAME = "我國評鑑合格醫院保證函";
                                    ProjectStr1 += ProjectStr1 == "" ? "我國評鑑合格醫院保證函" : "、我國評鑑合格醫院保證函";
                                    break;
                                case "1":
                                    anwhere.Field = "FILE1";
                                    anwhere.Field_NAME = "申請人自行提出書面保證函";
                                    ProjectStr1 += ProjectStr1 == "" ? "申請人自行提出書面保證函" : "、申請人自行提出書面保證函";
                                    break;
                                case "2":
                                    anwhere.Field = "FILE2";
                                    anwhere.Field_NAME = "醫師證書影本";
                                    ProjectStr1 += ProjectStr1 == "" ? "醫師證書影本" : "、醫師證書影本";
                                    break;
                                case "3":
                                    anwhere.Field = "FILE3";
                                    anwhere.Field_NAME = "國外契約或接受文件";
                                    ProjectStr1 += ProjectStr1 == "" ? "國外契約或接受文件" : "、國外契約或接受文件";
                                    break;
                                case "4":
                                    anwhere.Field = "FILE4";
                                    anwhere.Field_NAME = "E.C.F.M.G.及格證書影本";
                                    ProjectStr1 += ProjectStr1 == "" ? "E.C.F.M.G.及格證書影本" : "、E.C.F.M.G.及格證書影本";
                                    break;
                                case "5":
                                    anwhere.Field = "FILE5";
                                    anwhere.Field_NAME = "國民身分證正面影本";
                                    ProjectStr1 += ProjectStr1 == "" ? "國民身分證正面影本" : "、國民身分證正面影本";
                                    break;
                                case "6":
                                    anwhere.Field = "FILE6";
                                    anwhere.Field_NAME = "國民身分證反面影本";
                                    ProjectStr1 += ProjectStr1 == "" ? "國民身分證反面影本" : "、國民身分證反面影本";
                                    break;
                                case "7":
                                    anwhere.Field = "FILE7";
                                    anwhere.Field_NAME = "護照影本或有關部會許可出國文件影本";
                                    ProjectStr1 += ProjectStr1 == "" ? "護照影本或有關部會許可出國文件影本" : "、護照影本或有關部會許可出國文件影本";
                                    break;
                                case "8":
                                    anwhere.Field = "FILE8";
                                    anwhere.Field_NAME = "個人執業發展規劃書";
                                    ProjectStr1 += ProjectStr1 == "" ? "個人執業發展規劃書" : "、個人執業發展規劃書";
                                    break;
                                case "10":
                                    anwhere.Field = "FILE9";
                                    anwhere.Field_NAME = "醫師赴國外訓練英文保證函申請表";
                                    ProjectStr1 += ProjectStr1 == "" ? "醫師赴國外訓練英文保證函申請表" : "、醫師赴國外訓練英文保證函申請表";
                                    break;
                                default:
                                    anwhere.Field = "OTHER";
                                    anwhere.Field_NAME = "其他";
                                    anwhere.NOTE = model.Form.NOTE;
                                    ProjectStr1 += ProjectStr1 == "" ? "其他" : "、其他";
                                    isOther = true;
                                    break;
                            }
                            anwhere.ADD_TIME = DateTime.Now;
                            anwhere.APP_ID = model.Form.APP_ID;
                            anwhere.ISADDYN = "N";
                            anwhere.FREQUENCY = times + 1;
                            Insert(anwhere);
                            count++;
                        }
                        MainBody += ProjectStr1;
                        MainBody += "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        #endregion

                        #region 案件內容
                        // 更新案件狀態
                        ApplyModel appwhere = new ApplyModel();
                        appwhere.APP_ID = model.Form.APP_ID;

                        ApplyModel appdata = new ApplyModel();
                        appdata.InjectFrom(model.Form);
                        appdata.APP_ID = model.Form.APP_ID;
                        appdata.APP_TIME = null;
                        appdata.FLOW_CD = model.Form.FLOW_CD;
                        appdata.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                        appdata.UPD_TIME = DateTime.Now;
                        appdata.UPD_ACC = sm.UserInfo.UserNo;
                        appdata.UPD_FUN_CD = "ADM-STORE";
                        appdata.NOTICE_NOTE = model.Form.NOTE.TONotNullString();

                        if (model.Form.FLOW_CD == "2")
                        {
                            // 是否要寄信
                            savestatus = true;

                            appdata.APPLY_NOTICE_DATE = DateTime.Now;
                            if (model.Form.chkNotice.TONotNullString() != "")
                            {
                                appdata.ISMODIFY = "Y";
                            }
                            else
                            {
                                appdata.ISMODIFY = "N";
                            }
                        }

                        appdata.NOTIBODY = model.Form.NOTE.TONotNullString();
                        appdata.MAILBODY = MainBody;

                        //Update(appdata, appwhere);
                        base.Update2(appdata, appwhere, dict2, true);
                        #endregion

                        #region 寄信內容
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.Form.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001039Model apply001039Model = new Apply_001039Model();
                        apply001039Model.APP_ID = model.Form.APP_ID;
                        apply001039Model = this.GetRow(apply001039Model);
                        string note = "";

                        note = "補件項目﹕" + ProjectStr1 + "<br/>";
                        note += "補件備註﹕" + model.Form.NOTE.TONotNullString();

                        SendMail_Notice(applyModel.NAME, "醫師赴國外訓練英文保證函", "001039", apply001039Model.E_MAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID, note);

                        //// 判斷是否要寄信
                        //if (savestatus)
                        //{
                        //    SendMail_Notice(MainBody, model.Form.CONTACT_NAME, count, model.Form.E_MAIL, model.Form.APP_ID, "醫師赴國外訓練英文保證函", "001039");
                        //}
                        #endregion

                        tran.Commit();
                    }
                    else
                    {
                        #region 案件內容
                        // 更新案件狀態
                        ApplyModel appwhere = new ApplyModel();
                        appwhere.APP_ID = model.Form.APP_ID;
                        // 9:逾期未補件而予結案 // 15:自請撤銷 // 8:退件通知
                        if (model.Form.FLOW_CD == "9" || model.Form.FLOW_CD == "15" || model.Form.FLOW_CD == "8")
                        {
                            noteContent.Add("備註", model.Form.NOTE.TONotNullString());
                            MainBody = GetNote(noteContent);
                        }
                        // 12:核可(發文歸檔)
                        if (model.Form.FLOW_CD == "12")
                        {
                            noteContent.Add("郵寄日期", model.Form.MAIL_DATE_AC);
                            noteContent.Add("掛號條碼", model.Form.MAIL_BARCODE);
                            MainBody = GetNote(noteContent);
                        }

                        ApplyModel appdata = new ApplyModel();
                        appdata.InjectFrom(model.Form);
                        appdata.APP_ID = model.Form.APP_ID;
                        appdata.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                        appdata.NOTICE_NOTE = model.Form.NOTE.TONotNullString();
                        appdata.MAILBODY = MainBody;
                        appdata.FLOW_CD = model.Form.FLOW_CD;
                        appdata.UPD_TIME = DateTime.Now;
                        appdata.UPD_ACC = sm.UserInfo.UserNo;
                        appdata.UPD_FUN_CD = "ADM-STORE";
                        //Update(appdata, appwhere);
                        base.Update2(appdata, appwhere, dict2, true);
                        #endregion

                        #region 案件主表
                        if (model.Form.FLOW_CD == "12")
                        {
                            Apply_001039Model whereApply001039 = new Apply_001039Model();
                            whereApply001039.APP_ID = model.Form.APP_ID;
                            Apply_001039Model apply001039Model = new Apply_001039Model();

                            apply001039Model.MAIL_DATE = HelperUtil.TransToDateTime(model.Form.MAIL_DATE_AC);
                            apply001039Model.MAIL_BARCODE = model.Form.MAIL_BARCODE;
                            this.Update(apply001039Model, whereApply001039);
                        }
                        #endregion

                        #region 依據案件狀態寄發信件內容
                        if (model.Form.FLOW_CD == "--")
                        {
                            ApplyModel applyModel = new ApplyModel();
                            applyModel.APP_ID = model.Form.APP_ID;
                            applyModel = this.GetRow(applyModel);
                            Apply_001039Model apply001039Model = new Apply_001039Model();
                            apply001039Model.APP_ID = model.Form.APP_ID;
                            apply001039Model = this.GetRow(apply001039Model);

                            SendMail_InPorcess(applyModel.NAME, "醫師赴國外訓練英文保證函", "001039", apply001039Model.E_MAIL,
                                applyModel.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                            savestatus = false;
                        }

                        if (model.Form.FLOW_CD == "12")
                        {
                            ApplyModel applyModel = new ApplyModel();
                            applyModel.APP_ID = model.Form.APP_ID;
                            applyModel = this.GetRow(applyModel);
                            Apply_001039Model apply001039Model = new Apply_001039Model();
                            apply001039Model.APP_ID = model.Form.APP_ID;
                            apply001039Model = this.GetRow(apply001039Model);

                            SendMail_Archive(applyModel.NAME, "醫師赴國外訓練英文保證函", "001039", apply001039Model.E_MAIL,
                                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID,
                                            (HelperUtil.TransToDateTime(model.Form.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                            model.Form.MAIL_BARCODE);
                            savestatus = false;
                        }

                        if (model.Form.FLOW_CD == "9")
                        {
                            ApplyModel applyModel = new ApplyModel();
                            applyModel.APP_ID = model.Form.APP_ID;
                            applyModel = this.GetRow(applyModel);
                            Apply_001039Model apply001039Model = new Apply_001039Model();
                            apply001039Model.APP_ID = model.Form.APP_ID;
                            apply001039Model = this.GetRow(apply001039Model);

                            SendMail_Expired(applyModel.NAME, "醫師赴國外訓練英文保證函", "001039", apply001039Model.E_MAIL,
                                applyModel.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);
                            savestatus = false;
                        }
                        if (model.Form.FLOW_CD == "15")
                        {
                            ApplyModel applyModel = new ApplyModel();
                            applyModel.APP_ID = model.Form.APP_ID;
                            applyModel = this.GetRow(applyModel);
                            Apply_001039Model apply001039Model = new Apply_001039Model();
                            apply001039Model.APP_ID = model.Form.APP_ID;
                            apply001039Model = this.GetRow(apply001039Model);

                            SendMail_Cancel(applyModel.NAME, "醫師赴國外訓練英文保證函", "001039", apply001039Model.E_MAIL,
                                applyModel.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                        }
                        // 判斷是否要寄信
                        //if (savestatus)
                        //{
                        //    SendMail_Notice(MainBody, model.Form.CONTACT_NAME, count, model.Form.E_MAIL, model.Form.APP_ID, "醫師赴國外訓練英文保證函", "001039");
                        //}
                        #endregion
                        tran.Commit();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_001039FormModel GetApplyNotice_001039(string app_id)
        {
            Apply_001039FormModel result = new Apply_001039FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_001039FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply011004 社工師證書核發（英文）

        /// <summary>
        /// 社會工作師證書影本
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011004FormModel QueryApply_011004(string APP_ID)
        {
            Apply_011004FormModel result = new Apply_011004FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql =
                    @"select app.APP_ID, app.SRV_ID, app.APP_TIME, (convert(varchar, app.APP_EXT_DATE, 111)) as APP_EXT_DATE,
								ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAME, app.APP_ID, app.NAME, app.ENAME, app.IDN,app.BIRTHDAY,app.SEX_CD,
								a011004.C_TEL, a011004.H_TEL, a011004.MOBILE, a011004.EMAIL, a011004.C_ADDR, a011004.C_ZIP as C_ZIPCODE, app.FLOW_CD,  a011004.IS_MERGE, a011004.APPLY_NUM, a011004.APPLY_FOR, a011004.YEAR, a011004.TYPE
								from apply app 
								left join ADMIN ad on app.PRO_ACC = ad.ACC_NO
                                left join APPLY_011004 a011004 on app.APP_ID = a011004.APP_ID
                                where 1 = 1";
                _sql += "and app.app_id = '" + APP_ID + "'";

                try
                {
                    result = conn.QueryFirst<Apply_011004FormModel>(_sql);
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(APP_ID, "10");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011004FileModel GetFileList_011004(string APP_ID)
        {
            var result = new Apply_011004FileModel();
            ShareDAO dao = new ShareDAO();
            result.FILENAM = dao.GetFileGridList(APP_ID);
            result.APP_ID = APP_ID;
            return result;
        }

        /// <summary>
        /// 社工師證書核發（英文）-存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011004(Apply_011004FormModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "011004");
            dict2.Add("LastMODTIME", LastMODTIME);
            SessionModel sm = SessionModel.Get();
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            var mainproject = string.Empty;
            //檢核
            Msg = this.CheckApply011004(model);

            if (string.IsNullOrEmpty(Msg))
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    this.Tran(conn, tran);
                    try
                    {
                        if (model.FLOW_CD == "2" || model.FLOW_CD == "4")
                        {
                            #region 補件
                            // 取得補件紀錄
                            TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                            anwhere.APP_ID = model.APP_ID;
                            var andata = GetRowList(anwhere);

                            // 只取回最後一次補件的次數
                            var newandaata = from a in andata
                                             orderby a.FREQUENCY descending
                                             select a;
                            // 已補件次數
                            var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();
                            if (!string.IsNullOrEmpty(model.NGITEM))
                            {
                                var needchk = model.NGITEM.ToSplit(',');

                                count = needchk.Count() - 1;
                                // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                                MainBody = "<div class=\"form-group\">";
                                MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                                MainBody += "<div class=\"col-sm-10\">";
                                // 這邊放入檔案名稱
                                MainBody += "<p class=\"form-control-static\">";
                                foreach (var item in needchk)
                                {
                                    if (string.IsNullOrEmpty(item)) continue;
                                    var Field_NAME = "";
                                    var newitem = item.TOInt32();
                                    anwhere = new TblAPPLY_NOTICE();
                                    switch (newitem)
                                    {
                                        case 0:
                                            Field_NAME = "社會工作師證書影本";
                                            mainproject += mainproject == "" ? "社會工作師證書影本" : "、社會工作師證書影本";
                                            anwhere.Field = "FILE_" + "0";
                                            break;
                                        case 1:
                                            Field_NAME = "其他";
                                            mainproject += mainproject == "" ? "其他" : "、其他";
                                            anwhere.Field = "ALL_" + "1";
                                            break;
                                    }

                                    anwhere.ADD_TIME = DateTime.Now;
                                    anwhere.APP_ID = model.APP_ID;
                                    anwhere.ISADDYN = "N";
                                    anwhere.FREQUENCY = times + 1;
                                    anwhere.NOTE = model.NG_NOTE;
                                    anwhere.Field_NAME = Field_NAME;
                                    if (model.FLOW_CD == "2")
                                    {
                                        Insert(anwhere);
                                    }
                                    //count++;
                                    savestatus = true;
                                }

                                // 新增信件內容需補件欄位
                                MainBody += mainproject;
                                MainBody += "</p>";
                                MainBody += "</div>";
                                MainBody += "</div>";
                                MainBody += "<div class=\"form-group\">";
                                MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                                MainBody += "<div class=\"col-sm-10\">";
                                // 這邊放入檔案名稱
                                MainBody += "<p class=\"form-control-static\">" + model.NG_NOTE + "</p>";
                                MainBody += "</div>";
                                MainBody += "</div>";
                            }
                            #endregion
                        }
                        // 更新案件狀態
                        ApplyModel appwhere = new ApplyModel();
                        appwhere.APP_ID = model.APP_ID;

                        ApplyModel appdata = new ApplyModel();
                        appdata.InjectFrom(model);
                        appdata.FLOW_CD = model.FLOW_CD;
                        appdata.UPD_TIME = DateTime.Now;
                        appdata.UPD_ACC = sm.UserInfo.UserNo;
                        appdata.UPD_FUN_CD = "ADM-STORE";
                        appdata.UNIT_CD = 8; //社會救助及社工司
                                             //Update(appdata, appwhere);

                        if ((model.FLOW_CD == "2" || model.FLOW_CD == "4") && savestatus == false)
                        {
                            Msg = "請選擇補件項目並輸入備註資料!!";
                            tran.Rollback();
                        }
                        else
                        {
                            //base.Update(appdata, appwhere);
                            base.Update2(appdata, appwhere, dict2, true);
                            // 判斷是否要寄信
                            if (savestatus)
                            {
                                if (model.FLOW_CD == "2")
                                {
                                    string note = "";
                                    note = "補件項目﹕" + mainproject + "<br/>";
                                    note += "補件備註﹕" + model.NG_NOTE.TONotNullString().Replace("\\n", "<br/>").Replace(Environment.NewLine, "<br/>");

                                    SendMail_Notice(MainBody, model.NAME, count, model.EMAIL, model.APP_ID, "社工師證書核發（英文）", "011004", "", appdata.APP_TIME, note);
                                }
                                // 補正確認完成
                                if (model.FLOW_CD == "4")
                                {
                                    string MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                    MailBody += " <tr><th align=\"left\">" + model.NAME + "，您好:</th></tr>";
                                    MailBody += " <tr><td>您所提交的社工師證書核發（英文）申請，已完成資料補件共" + count.ToString() + "件（包括" + mainproject + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                    MailBody += " <tr><td align=\"right\">衛生福利部社會救助及社工司</td></tr></table>";
                                    SendMail(model.EMAIL, $"社工師證書核發（英文），案件編號{model.APP_ID}狀態通知", MailBody, "011004");
                                }
                            }
                            // 已收到紙本，審查中
                            if (model.FLOW_CD == "5")
                            {
                                SendMail_InPorcess(model.NAME, "社工師證書核發（英文）", "011004", model.EMAIL, model.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                            }
                            if (model.FLOW_CD == "9")
                            {
                                SendMail_Expired(model.NAME, "社工師證書核發（英文）", "011004", model.EMAIL, model.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.NG_NOTE);
                            }
                            if (model.FLOW_CD == "0")
                            {
                                SendMail_Success(model.NAME, "社工師證書核發（英文）", "011004", model.EMAIL, model.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                            }

                            tran.Commit();
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        tran.Rollback();
                        Msg = "存檔失敗，請聯絡系統管理員 。";
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            return Msg;
        }

        public Apply_011004FormModel GetApplyNotice_011004(string app_id)
        {
            Apply_011004FormModel result = new Apply_011004FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011004FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 社會工作師證書影本-存檔檢核
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011004(Apply_011004FormModel model)
        {
            string Msg = "";

            if (model.FLOW_CD == "2")
            {
                if (string.IsNullOrEmpty(model.NGITEM))
                {
                    Msg = " 請勾選補件項目 !";
                }
                if (string.IsNullOrEmpty(model.NG_NOTE))
                {
                    Msg = "請填寫補件內容!";
                }
            }

            return Msg;
        }

        #endregion

        #region Apply011003

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011003FormModel QueryApply_011003(Apply_011003FormModel parm)
        {
            Apply_011003FormModel result = new Apply_011003FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"select app.APP_TIME,app.APP_EXT_DATE,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM,app.MOHW_CASE_NO,
                                      a03.APP_ID,a03.ISMEET,a03.NAME,convert(varchar,a03.BIRTHDAY, 111)BIRTHDAY,
                                      a03.IDN,a03.SEX_CD,app.TEL,a03.FAX,a03.MOBILE,app.ADDR,app.ADDR_CODE,a03.MAIL,a03.EDUCATION,
                                      a03.SCHOOL,a03.OFFICE,a03.GRADUATION,a03.FILE_FIDC,a03.FILE_BIDC,a03.FILE_PIC,
                                      a03.FILE_GRAD,a03.FILE_NOTI,a03.FILE_GRADOFF,a03.FILE_FONSRV,a03.FILE_SCHOOL,a03.MERGEYN,
                                      a03.FILE_DOMICILE,a03.FILE_OTHER,app.FLOW_CD,a03.NOTICEDAY,a03.FILE_NOTI2,a03.FILE_NOTI3,a03.FILE_NOTI4
                                      from APPLY_011003 a03
                                      join APPLY app on a03.APP_ID = app.APP_ID
                                      left join ADMIN ad on app.PRO_ACC = ad.ACC_NO
                                      where 1 = 1";
                _sql += " and a03.app_id = '" + parm.APP_ID + "'";

                string _srvlistsql = @"select a03.*,convert(varchar,a03.SRV_SYEAR,111) SRV_SYEAR,convert(varchar,a03.SRV_EYEAR,111) SRV_EYEAR,
                                       case SRVLST_DEMAND_1 when '1' then '是' else '否' end as SRVLST_DEMAND_1,
                                       case SRVLST_DEMAND_2 when '1' then '是' else '否' end as SRVLST_DEMAND_2
                                       from APPLY_011003_SRVLST a03
                                       where 1 = 1";
                _srvlistsql += "and a03.app_id = '" + parm.APP_ID + "'";

                string _filesql = @"select af.*,a03.*,case a03.SRVLST_DEMAND_1 when '0' then '否' when '1' then '是' end as SRVLST_DEMAND_1,
                                           case a03.SRVLST_DEMAND_2 when '0' then '否' when '1' then '是' end as SRVLST_DEMAND_2
                                    from APPLY_FILE af
                                    left join APPLY_011003_SRVLST a03 on a03.APP_ID = af.APP_ID
                                    where 1 = 1 ";
                _filesql += " and af.app_id = '" + parm.APP_ID + "'";

                try
                {
                    ShareDAO shareDAO = new ShareDAO();
                    result = conn.Query<Apply_011003FormModel>(_sql).FirstOrDefault();
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");
                    result.FILE = conn.Query<Apply_011003FILEModel>(_filesql).ToList();
                    result.SRVLIST = conn.Query<Apply_011003SRVLSTModel>(_srvlistsql).ToList();
                    if (shareDAO.CalculationDocDate("011003", parm.APP_ID) && result.FLOW_CD == "2") result.IsNotice = "Y";
                    else result.IsNotice = "N";
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 檢核存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011003(Apply_011003ViewModel model)
        {
            string Msg = "";

            if ((model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4") && model.Form.FileCheck.TONotNullString() == "")
            {
                Msg = "請至少選擇一種補件項目 !";
            }
            if (model.Form.FLOW_CD == "2")
            {
                if (model.Form.NOTE.TONotNullString() == "" && model.Form.FileCheck.TONotNullString() != "")
                {
                    Msg = "請填寫補件內容 !";
                }
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                Msg = "存檔失敗，請聯絡系統管理員 !";
            }

            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011003(Apply_011003ViewModel model)
        {
            ShareDAO shareDAO = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string s_SRV_ID = "011003";
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", model.Form.APP_ID);
                    dict2.Add("SRV_ID", s_SRV_ID);
                    dict2.Add("LastMODTIME", LastMODTIME);

                    if (model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var getsrvlist = model.Form.SRVLIST.ToCount();
                            var cnt = getsrvlist + 10;
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";
                            foreach (var item in needchk)
                            {
                                if (Convert.ToInt32(Math.Ceiling(Convert.ToDouble(item) / 5)) <= getsrvlist)
                                {
                                    var SRVTimes = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(item) / 5));
                                    anwhere = new TblAPPLY_NOTICE();
                                    anwhere.ADD_TIME = DateTime.Now;
                                    anwhere.APP_ID = model.Form.APP_ID;
                                    anwhere.ISADDYN = "N";
                                    anwhere.FREQUENCY = times + 1;
                                    anwhere.NOTE = model.Form.NOTE;
                                    if (item.TOInt32() % 5 == 1)
                                    {
                                        anwhere.Field_NAME = "經歷_" + SRVTimes.TONotNullString() + " 服務證明正本彩色檔";
                                        anwhere.Field = "SRVLIST_" + SRVTimes.TONotNullString() + "-1";

                                        // 這邊放入檔案名稱
                                        mainproject += mainproject == ""
                                            ? "經歷_" + SRVTimes.TONotNullString() + " 服務證明正本彩色檔"
                                            : "、經歷_" + SRVTimes.TONotNullString() + " 服務證明正本彩色檔";
                                    }

                                    if (item.TOInt32() % 5 == 2)
                                    {
                                        anwhere.Field_NAME = "經歷_" + SRVTimes.TONotNullString() + " 服務單位認可有案證明文件";
                                        anwhere.Field = "SRVLIST_" + SRVTimes.TONotNullString() + "-2";

                                        // 這邊放入檔案名稱
                                        mainproject += mainproject == ""
                                            ? "經歷_" + SRVTimes.TONotNullString() + " 服務單位認可有案證明文件"
                                            : "、經歷_" + SRVTimes.TONotNullString() + " 服務單位認可有案證明文件";
                                    }

                                    if (item.TOInt32() % 5 == 3)
                                    {
                                        anwhere.Field_NAME = "經歷_" + SRVTimes.TONotNullString() + " 勞工保險被保險人投保明細表影本(服務證明為團體或私立機構開具者需附)";
                                        anwhere.Field = "SRVLIST_" + SRVTimes.TONotNullString() + "-3";

                                        // 這邊放入檔案名稱
                                        mainproject += mainproject == ""
                                            ? "經歷_" + SRVTimes.TONotNullString() + " 勞工保險被保險人投保明細表影本(服務證明為團體或私立機構開具者需附)"
                                            : "、經歷_" + SRVTimes.TONotNullString() + " 勞工保險被保險人投保明細表影本(服務證明為團體或私立機構開具者需附)";
                                    }

                                    if (item.TOInt32() % 5 == 4)
                                    {
                                        anwhere.Field_NAME = "經歷_" + SRVTimes.TONotNullString() + " 服務單位立案或法人登記證書影本(服務證明為團體或私立機構開具者需附)";
                                        anwhere.Field = "SRVLIST_" + SRVTimes.TONotNullString() + "-4";

                                        // 這邊放入檔案名稱
                                        mainproject += mainproject == ""
                                            ? "經歷_" + SRVTimes.TONotNullString() + " 服務單位立案或法人登記證書影本(服務證明為團體或私立機構開具者需附)"
                                            : "、經歷_" + SRVTimes.TONotNullString() + " 服務單位立案或法人登記證書影本(服務證明為團體或私立機構開具者需附)";
                                    }

                                    if (item.TOInt32() % 5 == 0)
                                    {
                                        anwhere.Field_NAME = "經歷_" + SRVTimes.TONotNullString() + " 服務單位章程影本(服務證明為團體或私立機構開具者需附)";
                                        anwhere.Field = "SRVLIST_" + SRVTimes.TONotNullString() + "-5";

                                        // 這邊放入檔案名稱
                                        mainproject += mainproject == ""
                                            ? "經歷_" + SRVTimes.TONotNullString() + " 服務單位章程影本(服務證明為團體或私立機構開具者需附)"
                                            : "、經歷_" + SRVTimes.TONotNullString() + " 服務單位章程影本(服務證明為團體或私立機構開具者需附)";
                                    }

                                    if (model.Form.FLOW_CD == "2")
                                    {
                                        Insert(anwhere);
                                    }
                                    count++;
                                    savestatus = true;
                                }
                                else
                                {
                                    var Field_NAME = "";
                                    var newitem = item.TOInt32() - getsrvlist * 5;
                                    anwhere = new TblAPPLY_NOTICE();
                                    switch (newitem)
                                    {
                                        case 1:
                                            Field_NAME = "國民身分證正面影本";
                                            mainproject += mainproject == "" ? "國民身分證正面影本" : "、國民身分證正面影本";
                                            anwhere.Field = "FILE_" + "1";
                                            break;
                                        case 2:
                                            Field_NAME = "國民身分證背面影本";
                                            mainproject += mainproject == "" ? "國民身分證背面影本" : "、國民身分證背面影本";
                                            anwhere.Field = "FILE_" + "2";
                                            break;
                                        case 3:
                                            Field_NAME = "最近一年內半身脫帽照片一張";
                                            mainproject += mainproject == "" ? "最近一年內半身脫帽照片一張" : "、最近一年內半身脫帽照片一張";
                                            anwhere.Field = "FILE_" + "3";
                                            break;
                                        case 4:
                                            Field_NAME = "中華民國一百零二年起，經考選部依「專技人員社會工作師考試應考資格第五條審議通過並公告名單」所列學校之畢業證書影本";
                                            mainproject += mainproject == "" ? "中華民國一百零二年起，經考選部依「專技人員社會工作師考試應考資格第五條審議通過並公告名單」所列學校之畢業證書影本" : "、中華民國一百零二年起，經考選部依「專技人員社會工作師考試應考資格第五條審議通過並公告名單」所列學校之畢業證書影本";
                                            anwhere.Field = "FILE_" + "4";
                                            break;
                                        case 5:
                                            Field_NAME = "非考選部公告之學校系所(報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本)";
                                            mainproject += mainproject == "" ? "非考選部公告之學校系所(報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本)" : "、非考選部公告之學校系所(報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本)";
                                            anwhere.Field = "FILE_" + "5";
                                            break;
                                        case 6:
                                            Field_NAME = "經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本";
                                            mainproject += mainproject == "" ? "經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本" : "、經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本";
                                            anwhere.Field = "FILE_" + "6";
                                            break;
                                        case 7:
                                            Field_NAME = "國外社會工作師資格證明文件";
                                            mainproject += mainproject == "" ? "國外社會工作師資格證明文件" : "、國外社會工作師資格證明文件";
                                            anwhere.Field = "FILE_" + "7";
                                            break;
                                        case 8:
                                            Field_NAME = "公立或立案之私立專科以上學校教學之證明文件";
                                            mainproject += mainproject == "" ? "公立或立案之私立專科以上學校教學之證明文件" : "、公立或立案之私立專科以上學校教學之證明文件";
                                            anwhere.Field = "FILE_" + "8";
                                            break;
                                        case 9:
                                            Field_NAME = "戶籍謄本或戶口名簿(改過姓名者需附)";
                                            mainproject += mainproject == "" ? "戶籍謄本或戶口名簿(改過姓名者需附)" : "、戶籍謄本或戶口名簿(改過姓名者需附)";
                                            anwhere.Field = "FILE_" + "9";
                                            break;
                                        case 10:
                                            Field_NAME = "其他附件(申請人自行列舉)";
                                            mainproject += mainproject == "" ? "其他(申請人自行列舉)" : "、其他(申請人自行列舉)";
                                            anwhere.Field = "FILE_" + "10";
                                            break;

                                        case 11:
                                            Field_NAME = "非考選部公告之學校系所(畢業證書影本)";
                                            mainproject += mainproject == "" ? "非考選部公告之學校系所(畢業證書影本)" : "、非考選部公告之學校系所(畢業證書影本)";
                                            anwhere.Field = "FILE_" + "12";
                                            break;
                                        case 12:
                                            Field_NAME = "非考選部公告之學校系所(學分證明) (如檢附成績單，請清楚標記15學科(如螢光筆劃記)，以利審查)";
                                            mainproject += mainproject == "" ? "非考選部公告之學校系所(學分證明) (如檢附成績單，請清楚標記15學科(如螢光筆劃記)，以利審查)" : "、非考選部公告之學校系所(學分證明) (如檢附成績單，請清楚標記15學科(如螢光筆劃記)，以利審查)";
                                            anwhere.Field = "FILE_" + "13";
                                            break;
                                        case 13:
                                            Field_NAME = "非考選部公告之學校系所(實習證明)";
                                            mainproject += mainproject == "" ? "非考選部公告之學校系所(實習證明)" : "、非考選部公告之學校系所(實習證明)";
                                            anwhere.Field = "FILE_" + "14";
                                            break;
                                        case 14:
                                            Field_NAME = "其他";
                                            mainproject += mainproject == "" ? "其他" : "、其他";
                                            anwhere.Field = "ALL" + "11";
                                            break;
                                    }

                                    anwhere.ADD_TIME = DateTime.Now;
                                    anwhere.APP_ID = model.Form.APP_ID;
                                    anwhere.ISADDYN = "N";
                                    anwhere.FREQUENCY = times + 1;
                                    anwhere.NOTE = model.Form.NOTE;
                                    anwhere.Field_NAME = Field_NAME;

                                    if (model.Form.FLOW_CD == "2")
                                    {
                                        Insert(anwhere);
                                    }

                                    count++;
                                    savestatus = true;
                                }
                            }
                            MainBody += mainproject;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                        }
                    }

                    // 更新案件狀態
                    ApplyModel appwhere = new ApplyModel();
                    appwhere.APP_ID = model.Form.APP_ID;

                    ApplyModel appdata = new ApplyModel();
                    //appdata.InjectFrom(model.Form);
                    appdata.MAILBODY = MainBody;
                    appdata.APP_ID = model.Form.APP_ID;
                    appdata.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                    appdata.FLOW_CD = model.Form.FLOW_CD;
                    appdata.UPD_TIME = DateTime.Now;
                    appdata.UPD_ACC = sm.UserInfo.UserNo;
                    appdata.UPD_FUN_CD = "ADM-STORE";

                    if ((model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4") && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else
                    {
                        base.Update2(appdata, appwhere, dict2, true);
                        string MailBody = "";
                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            if (model.Form.FLOW_CD == "2")
                            {
                                SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, "社會工作實務經驗年資審查", "011003", "", shareDAO.CalDate("011003", model.Form.APP_ID), mainproject);
                            }
                            // 補正確認完成
                            if (model.Form.FLOW_CD == "4")
                            {
                                MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">" + model.Form.NAME + "，您好:</th></tr>";
                                MailBody += " <tr><td>您所提交的社會工作實務經驗年資審查申請，已完成資料補件共" + count.ToString() + "件（包括" + mainproject + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                MailBody += " <tr><td align=\"right\">衛生福利部</td></tr>";
                                MailBody += " <tr><td align=\"left\">PS.本郵件係系統自動發信，請勿直接回覆本信；如有問題，請逕向本部相關業務單位洽詢。</td></tr>";
                                MailBody += "</table>";
                                SendMail(model.Form.EMAIL, $"社會工作實務經驗年資審查，案件編號{model.Form.APP_ID}狀態通知", MailBody, "011003");
                            }
                        }
                        if (model.Form.FLOW_CD == "9")
                        {
                            SendMail_Expired(model.Form.NAME, "社會工作實務經驗年資審查", "011003", model.Form.EMAIL, model.Form.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);
                        }
                        if (model.Form.FLOW_CD == "5")
                        {
                            SendMail_InPorcess(model.Form.NAME, "社會工作實務經驗年資審查", "011003", model.Form.EMAIL, model.Form.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                        }
                        if (model.Form.FLOW_CD == "0")
                        {
                            SendMail_Success(model.Form.NAME, "社會工作實務經驗年資審查", "011003", model.Form.EMAIL, model.Form.APP_TIME?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                        }

                        tran.Commit();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_011003FormModel GetApplyNotice_011003(string app_id)
        {
            Apply_011003FormModel result = new Apply_011003FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011003FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply005013

        public Apply_005013FormModel QueryApply_005013(string app_id)
        {
            Apply_005013FormModel result = new Apply_005013FormModel();
            DataTable File = QueryApplyFile_005013(app_id);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select APPLY.APP_TIME,APPLY.APP_EXT_DATE,APPLY.APP_ID,APPLY.NAME,APPLY.IDN,APPLY.ADDR_CODE,APPLY.ADDR,
                                           APPLY.TEL,FLOW_CD CODE_CD,APPLY.MOHW_CASE_NO,APPLY.MOBILE,

                                           ISNULL(ADMIN.NAME,APPLY.PRO_ACC) AS ADMIN_NAME,

	                                       APPLY_005013.APP_TYPE CSEE_TYPE,APPLY_005013.ORIGIN ProductionCountry,APPLY_005013.SELLER SellerCountry,
                                           APPLY_005013.SHIPPINGPORT,APPLY_005013.RADIOUSAGE,APPLY_005013.RADIOUSAGE_TEXT,APPLY_005013.RADIOYN,
                                           APPLY_005013.EMAIL

                                    from apply left join APPLY_005013 on apply.APP_ID = apply_005013.APP_ID
	                                           left join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where apply.APP_ID='" + app_id + "'";
                    result = conn.QueryFirst<Apply_005013FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            result.Item_005013 = QueryApply_005013_Item1(app_id);
            result.ApplyItem2 = QueryApply_005013_Item2(app_id);
            if (File != null)
            {
                if (File.Rows.Count > 0)
                {
                    result.File_1_TEXT = File.Rows[0][0].ToString();
                    result.File_2_TEXT = File.Rows[0][1].ToString();
                    result.File_3_TEXT = File.Rows[0][2].ToString();
                    result.File_4_TEXT = File.Rows[0][3].ToString();
                    result.File_5_TEXT = File.Rows[0][4].ToString();
                    result.File_6_TEXT = File.Rows[0][5].ToString();
                }
            }

            result.File_005013 = QueryApplyFileList_005013(app_id);
            if (result.File_005013.ToCount() == 0)
            {
                ApplyFileItem4Model I4 = new ApplyFileItem4Model();
                result.File_005013.Add(I4);
            }

            result.APPLY_STATUS = GetSchedule(app_id, "09");

            return result;
        }

        public IList<ApplyItemModel> QueryApply_005013_Item1(string app_id)
        {
            APPLY_005013_ITEM1 where = new APPLY_005013_ITEM1();
            where.APP_ID = app_id;
            var temp = base.GetRowList<APPLY_005013_ITEM1>(where);
            IList<ApplyItemModel> result = new List<ApplyItemModel>();

            foreach (var item in temp)
            {
                ApplyItemModel newItem = new ApplyItemModel();
                newItem.ItemNum = item.ITEMNUM;
                newItem.Commodities = item.COMMODITIES;
                newItem.CommodMemo = item.COMMODMEMO;
                newItem.Spec = item.SPEC;
                newItem.Qty = item.QTY;
                newItem.Unit = item.UNIT;
                newItem.Unit_TEXT = item.UNIT_TEXT;
                newItem.PorcType = item.PORCTYPE;
                newItem.CommodType = item.COMMODTYPE;
                newItem.SpecQty = item.SPECQTY;
                newItem.SpecUnit = item.SPECUNIT;
                newItem.SpecUnit_TEXT = item.SPECUNIT_TEXT;
                result.Add(newItem);
            }

            return result;
        }

        public IList<ApplyItem2Model> QueryApply_005013_Item2(string app_id)
        {
            ShareDAO dbo = new ShareDAO();
            APPLY_005013_ITEM2 where = new APPLY_005013_ITEM2();
            where.APP_ID = app_id;
            var temp = base.GetRowList<APPLY_005013_ITEM2>(where);
            IList<ApplyItem2Model> result = new List<ApplyItem2Model>();

            foreach (var item in temp)
            {
                ApplyItem2Model newItem = new ApplyItem2Model();
                newItem.ItemNum = item.ITEMNUM;
                newItem.ItemName = item.ITEMNAME;
                newItem.Usage = item.USAGE;
                newItem.AllQty = item.ALLQTY;
                newItem.ItemQty = item.QTY;
                newItem.ItemQtyUnit = dbo.Getvw_PACK(item.UNIT);
                newItem.ItemQtyUnit2 = newItem.ItemQtyUnit;
                newItem.UNIT = item.UNIT;
                newItem.ItemSpecQty = item.SPECQTY;
                newItem.ItemSpecQtyUnit = dbo.Getvw_PACK_UNIT(item.SPECUNIT);
                newItem.SPECUNIT = item.SPECUNIT;
                result.Add(newItem);
            }

            return result;
        }

        public DataTable QueryApplyFile_005013(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @file1 nvarchar(max),@file2 nvarchar(max),@file3 nvarchar(max),@file4 nvarchar(max),@file5 nvarchar(max),@file6 nvarchar(max)

                                    select @file1 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='1'

                                    select @file2 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='2'

									select @file3 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='3'

									select @file4 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='4'

									select @file5 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='5'

									select @file6 = substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO='6'

                                    select @file1 file1,@file2 file2,@file3 file3,@file4 file4,@file5 file5,@file6 file6";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public IList<ApplyFileItem4Model> QueryApplyFileList_005013(string app_id)
        {
            DataTable dt = new DataTable();
            IList<ApplyFileItem4Model> result = new List<ApplyFileItem4Model>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select substring(FILENAME,16,len(FILENAME)) + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')+ ','+isnull(convert(varchar,BATCH_INDEX),'0') file7 
                                    from APPLY_FILE where APP_ID='" + app_id + @"' and FILE_NO > 6
									order by convert(int,BATCH_INDEX)";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    dt = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        ApplyFileItem4Model newItem = new ApplyFileItem4Model();
                        newItem.FileName_TEXT = dt.Rows[i][0].ToString();

                        result.Add(newItem);
                    }
                }
            }

            return result;
        }

        public Apply_005013FormModel GetApplyNotice_005013(string app_id)
        {
            Apply_005013FormModel result = new Apply_005013FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_005013FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApplyDoc005013(Apply_005013ViewModel model)
        {
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            string Msg = "";
            DataTable UndertakerInfo = new DataTable();
            DataTable CaseNoInfo = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    #region 取得承辦人資訊
                    string _sql = @"select ADMIN.NAME,ADMIN.TEL,ADMIN.MAIL
                                    from APPLY join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where APPLY.APP_ID='" + model.Form.APP_ID + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(UndertakerInfo);
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }

                if (model.Form.CODE_CD == "4")
                {
                    try
                    {
                        #region 取得公文資訊
                        string _sql = @"select TOP 1 APP_ID,MOHW_CASE_NO,convert(varchar,INSERTDATE,111) INSERTDATE
                                        from OFFICIAL_DOC
                                        where APP_ID='" + model.Form.APP_ID + "' and MOHW_CASE_NO='" + model.Form.MOHW_CASE_NO + @"'
                                        order by INSERTDATE DESC";
                        SqlCommand cmd = new SqlCommand(_sql, conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(CaseNoInfo);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        Msg = "存檔失敗，請聯絡系統管理員 。";
                    }
                }

                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 補件欄位紀錄
                    // CODE_CD='2' 申請資料待確認
                    // CODE_CD='4' 申請案補件中
                    if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        // 紀錄補件欄位
                        var count = 0;

                        // 比對是否為空值，若不為空則新增至欄位紀錄
                        PropertyInfo[] properties = model.Detail.GetType().GetProperties();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            if (properties[i].PropertyType.IsGenericType &&
                                properties[i].PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
                            {
                                //另外處理
                            }
                            else
                            {
                                if (properties[i].GetValue(model.Detail) != null
                                    && (properties[i].GetValue(model.Detail)).ToString() != "False"
                                    && (properties[i].GetValue(model.Detail)).ToString() != "True")
                                {
                                    anwhere = new TblAPPLY_NOTICE();
                                    anwhere.ADD_TIME = DateTime.Now;
                                    anwhere.APP_ID = model.Form.APP_ID;
                                    anwhere.ISADDYN = "N";
                                    anwhere.Field = properties[i].Name;
                                    anwhere.FREQUENCY = times + 1;
                                    anwhere.NOTE = properties[i].GetValue(model.Detail).TONotNullString();
                                    Insert(anwhere);
                                    MainBody += "<tr>";
                                    MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (properties[i].CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                    MainBody += "<td align=\"left\" style=\"border:1px solid\">" + properties[i].GetValue(model.Detail).TONotNullString() + "</td>";
                                    MainBody += "</tr>";

                                    count++;
                                    savestatus = true;
                                }
                            }
                        }

                        //ApplyItem
                        for (var j = 0; j < model.Detail.Item_005013.Count; j++)
                        {
                            PropertyInfo[] propertiesItem = model.Detail.Item_005013[j].GetType().GetProperties();
                            for (var i = 0; i < propertiesItem.Length; i++)
                            {
                                if (propertiesItem[i].GetValue(model.Detail.Item_005013[j]) != null
                                    && (propertiesItem[i].GetValue(model.Detail.Item_005013[j])).ToString() != "False"
                                    && (propertiesItem[i].GetValue(model.Detail.Item_005013[j])).ToString() != "True")
                                {
                                    anwhere = new TblAPPLY_NOTICE();
                                    anwhere.ADD_TIME = DateTime.Now;
                                    anwhere.APP_ID = model.Form.APP_ID;
                                    anwhere.ISADDYN = "N";
                                    anwhere.Field = propertiesItem[i].Name;
                                    anwhere.FREQUENCY = times + 1;
                                    anwhere.NOTE = propertiesItem[i].GetValue(model.Detail.Item_005013[j]).TONotNullString();
                                    anwhere.BATCH_INDEX = j + 1;
                                    anwhere.SRC_NO = 1;//區別ApplyItem
                                    Insert(anwhere);
                                    MainBody += "<tr>";
                                    MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (propertiesItem[i].CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                    MainBody += "<td align=\"left\" style=\"border:1px solid\">" + propertiesItem[i].GetValue(model.Detail.Item_005013[j]).TONotNullString() + "</td>";
                                    MainBody += "</tr>";

                                    count++;
                                    savestatus = true;
                                }
                            }
                        }

                        //ApplyItem2
                        for (var j = 0; j < model.Detail.ApplyItem2.Count; j++)
                        {
                            PropertyInfo[] propertiesItem = model.Detail.ApplyItem2[j].GetType().GetProperties();
                            for (var i = 0; i < propertiesItem.Length; i++)
                            {
                                if (propertiesItem[i].GetValue(model.Detail.ApplyItem2[j]) != null
                                    && (propertiesItem[i].GetValue(model.Detail.ApplyItem2[j])).ToString() != "False"
                                    && (propertiesItem[i].GetValue(model.Detail.ApplyItem2[j])).ToString() != "True")
                                {
                                    anwhere = new TblAPPLY_NOTICE();
                                    anwhere.ADD_TIME = DateTime.Now;
                                    anwhere.APP_ID = model.Form.APP_ID;
                                    anwhere.ISADDYN = "N";
                                    anwhere.Field = propertiesItem[i].Name;
                                    anwhere.FREQUENCY = times + 1;
                                    anwhere.NOTE = propertiesItem[i].GetValue(model.Detail.ApplyItem2[j]).TONotNullString();
                                    anwhere.BATCH_INDEX = j + 1;
                                    anwhere.SRC_NO = 2;//區別ApplyItem2
                                    Insert(anwhere);
                                    MainBody += "<tr>";
                                    MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (propertiesItem[i].CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                    MainBody += "<td align=\"left\" style=\"border:1px solid\">" + propertiesItem[i].GetValue(model.Detail.ApplyItem2[j]).TONotNullString() + "</td>";
                                    MainBody += "</tr>";

                                    count++;
                                    savestatus = true;
                                }
                            }
                        }

                        //ApplyFile
                        if (model.Detail.File_005013 != null)
                        {
                            for (var j = 0; j < model.Detail.File_005013.Count; j++)
                            {
                                PropertyInfo[] propertiesItem = model.Detail.File_005013[j].GetType().GetProperties();
                                for (var i = 0; i < propertiesItem.Length; i++)
                                {
                                    if (propertiesItem[i].GetValue(model.Detail.File_005013[j]) != null
                                        && (propertiesItem[i].GetValue(model.Detail.File_005013[j])).ToString() != "False"
                                        && (propertiesItem[i].GetValue(model.Detail.File_005013[j])).ToString() != "True")
                                    {
                                        anwhere = new TblAPPLY_NOTICE();
                                        anwhere.ADD_TIME = DateTime.Now;
                                        anwhere.APP_ID = model.Form.APP_ID;
                                        anwhere.ISADDYN = "N";
                                        anwhere.Field = propertiesItem[i].Name;
                                        anwhere.FREQUENCY = times + 1;
                                        anwhere.NOTE = propertiesItem[i].GetValue(model.Detail.File_005013[j]).TONotNullString();
                                        anwhere.BATCH_INDEX = j + 1;
                                        anwhere.SRC_NO = 3;//區別ApplyFile
                                        Insert(anwhere);
                                        MainBody += "<tr>";
                                        MainBody += "<td align=\"left\" style=\"border:1px solid\">" + (propertiesItem[i].CustomAttributes.ToList())[0].NamedArguments[0].TypedValue.Value + "</td>";
                                        MainBody += "<td align=\"left\" style=\"border:1px solid\">" + propertiesItem[i].GetValue(model.Detail.File_005013[j]).TONotNullString() + "</td>";
                                        MainBody += "</tr>";

                                        count++;
                                        savestatus = true;
                                    }
                                }
                            }
                        }

                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            string MailBody = "";
                            if (model.Form.CODE_CD == "2")
                            {
                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>民眾少量自用中藥貨品進口(案號：" + model.Form.APP_ID + ")，經本部審查後，尚有待修正事項，請登入「人民申請案線上申辦服務系統」，依審查建議盡速修正申請表單內容，並於5個工作天內完成。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                //MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                //MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                //MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }
                            else if (model.Form.CODE_CD == "4")
                            {

                                MailBody += "<table align=\"left\" style=\"width:90%;\">";
                                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                                MailBody += " <tr><td>民眾少量自用中藥貨品進口(案號：" + model.Form.APP_ID + ")一案，本部業於" + (Convert.ToInt32(CaseNoInfo.Rows[0][2].ToString().Split('/')[0]) - 1911).ToString() + "年" + CaseNoInfo.Rows[0][2].ToString().Split('/')[1] + "月" + CaseNoInfo.Rows[0][2].ToString().Split('/')[2] + "日以衛部中字第" + model.Form.MOHW_CASE_NO + "號函，通知您待補正事項，敬請配合辦理。</td></tr>";
                                MailBody += "<tr><td>";
                                MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                                MailBody += "<tr>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                                MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                                MailBody += "</tr>";
                                MailBody += MainBody;
                                MailBody += "</table>";
                                MailBody += "</td></tr>";
                                MailBody += "<tr><td></td></tr>";
                                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                                //MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                                //MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                                //MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                                MailBody += "</table>";
                            }

                            SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, "民眾少量自用中藥貨品進口", "005013", MailBody);
                        }
                    }
                    #endregion

                    if (savestatus)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                        Msg = "請選擇補件項目並輸入備註說明。";
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 一般存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AppendApply005013(Apply_005013ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sm = SessionModel.Get();
            sm.LastMODTIME = LastMODTIME;
            string Msg = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {

                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", model.Form.APP_ID);
                    dict2.Add("SRV_ID", "005013");
                    dict2.Add("LastMODTIME", LastMODTIME);

                    #region 寫回Table
                    APPLY_005013 where = new APPLY_005013();
                    where.APP_ID = model.Form.APP_ID;
                    APPLY_005013 newData = new APPLY_005013();
                    newData.InjectFrom(model.Form);
                    newData.APP_ID = model.Form.APP_ID;
                    newData.ORIGIN = model.Form.ProductionCountry;
                    newData.ORIGIN_TEXT = model.Form.ProductionCountry_TEXT;
                    newData.SELLER = model.Form.SellerCountry;
                    newData.SELLER_TEXT = model.Form.SellerCountry_TEXT;
                    newData.SHIPPINGPORT = model.Form.ShippingPort;
                    newData.SHIPPINGPORT_TEXT = model.Form.ShippingPort_TEXT;
                    newData.APP_TYPE = model.Form.CSEE_TYPE;
                    newData.RADIOUSAGE = model.Form.RADIOUSAGE;
                    newData.RADIOUSAGE_TEXT = model.Form.RADIOUSAGE_TEXT;
                    newData.RADIOYN = model.Form.RADIOYN;
                    newData.ADD_TIME = null;

                    //base.Update(newData, where);
                    Update2(newData, where, dict2, true);

                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);
                    newDataApply.TEL = model.Form.TEL_Extension.TONotNullString() != "" ? model.Form.TEL_BEFORE + "-" + model.Form.TEL_AFTER + "#" + model.Form.TEL_Extension : model.Form.TEL_BEFORE + "-" + model.Form.TEL_AFTER;
                    newDataApply.MOBILE = model.Form.MOBILE;
                    newDataApply.ADDR_CODE = model.Form.TAX_ORG_CITY_CODE;
                    newDataApply.ADDR = model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL;
                    newDataApply.FLOW_CD = model.Form.CODE_CD;
                    newDataApply.ADD_TIME = null;
                    newDataApply.APP_TIME = null;

                    //base.Update(newDataApply, whereApply);
                    Update2(newDataApply, whereApply, dict2, true);

                    foreach (var item in model.Form.Item_005013)
                    {
                        APPLY_005013_ITEM1 whereitem1 = new APPLY_005013_ITEM1();
                        whereitem1.APP_ID = model.Form.APP_ID;
                        whereitem1.ITEMNUM = item.ItemNum;
                        APPLY_005013_ITEM1 newitem1 = new APPLY_005013_ITEM1();
                        newitem1.InjectFrom(item);
                        newitem1.ITEMNUM = item.ItemNum;
                        newitem1.COMMODITIES = item.Commodities;
                        newitem1.COMMODMEMO = item.CommodMemo;
                        newitem1.SPEC = item.Spec;
                        newitem1.QTY = item.Qty;
                        newitem1.UNIT = item.Unit;
                        newitem1.UNIT_TEXT = item.Unit_TEXT.TONotNullString().Replace("::", "||"); ;
                        newitem1.SPECQTY = item.SpecQty;
                        newitem1.SPECUNIT = item.SpecUnit;
                        newitem1.SPECUNIT_TEXT = item.SpecUnit_TEXT.TONotNullString().Replace("::", "||");
                        newitem1.PORCTYPE = item.PorcType;
                        newitem1.COMMODTYPE = item.CommodType;

                        //base.Update(newitem1, whereitem1);
                        Update2(newitem1, whereitem1, dict2, true);

                        var item2 = model.Form.ApplyItem2[Convert.ToInt32(item.ItemNum) - 1];
                        APPLY_005013_ITEM2 whereitem2 = new APPLY_005013_ITEM2();
                        whereitem2.APP_ID = model.Form.APP_ID;
                        whereitem2.ITEMNUM = item.ItemNum;
                        APPLY_005013_ITEM2 newitem2 = new APPLY_005013_ITEM2();
                        newitem2.InjectFrom(item);
                        newitem2.ITEMNUM = item.ItemNum;
                        newitem2.ITEMNAME = item2.ItemName;
                        newitem2.USAGE = item2.Usage;
                        newitem2.ALLQTY = $"共{item2.ItemQty}{item2.ItemQtyUnit.Replace("::", "||")}，每{item2.ItemQtyUnit2.Replace("::", "||")}{item2.ItemSpecQty}{item2.ItemSpecQtyUnit.Replace("::", "||")}";
                        newitem2.QTY = item.Qty;
                        newitem2.UNIT = item.Unit;
                        newitem2.SPECQTY = item.SpecQty;
                        newitem2.SPECUNIT = item.SpecUnit;

                        //base.Update(newitem1, whereitem1);
                        Update2(newitem2, whereitem2, dict2, true);
                    }
                    #endregion

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public void CaseFinishMail_005013(Apply_005013ViewModel model)
        {
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                string YYY = "";
                string MMdd = "";
                var dt = DateTime.Now.ToString("yyyy-MM-dd");
                string MOHDATE = GetMOHCaseDate(model.Form.APP_ID, model.Form.MOHW_CASE_NO);
                if (string.IsNullOrEmpty(MOHDATE))
                {
                    YYY = Convert.ToString(Convert.ToInt32(dt.ToSplit("-").FirstOrDefault()) - 1911);
                    MMdd = Convert.ToString(dt.ToSplit("-")[1]) + Convert.ToString(dt.ToSplit("-")[2]);
                    MOHDATE = $"{YYY}{MMdd}";
                }
                System.Data.DataTable UndertakerInfo = GetAdminInfo(model.Form.APP_ID);
                string MailBody = "";
                //結案寄信通知(結案(回函核准)CODE_CD=0;結案(歉難同意)/收件不收案CODE_CD=20)
                if (model.Form.CODE_CD == "0")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>本部業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函，";
                    MailBody += "檢送您申請之民眾少量自用中藥貨品進口(案號:" + model.Form.APP_ID + ")正本，請查照。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    //MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    //MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    //MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "民眾少量自用中藥貨品進口", "005013", MailBody, null, "", "民眾少量自用中藥貨品進口，案件編號:" + model.Form.APP_ID + "，已收案通知");
                }

                if (model.Form.CODE_CD == "20")
                {
                    MailBody += "<table align=\"left\" style=\"width:90%;\">";
                    MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                    MailBody += " <tr><td>您申請之民眾少量自用中藥貨品進口(案號:" + model.Form.APP_ID + ")一案，本部歉難同意，";
                    MailBody += "業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Form.MOHW_CASE_NO + " 號函回復貴公司。</td></tr>";
                    MailBody += "<tr><td></td></tr>";
                    MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                    //MailBody += " <tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                    //MailBody += " <tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                    //MailBody += " <tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                    MailBody += "</table>";

                    SendMail_Notice("", model.Form.NAME, 0, model.Form.EMAIL, model.Form.APP_ID, "民眾少量自用中藥貨品進口", "005013", MailBody, null, "", "民眾少量自用中藥貨品進口，案件編號:" + model.Form.APP_ID + "，已收件不收案");
                }
                tran.Commit();
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Apply005014
        /// <summary>
        /// 前台補件
        /// </summary>
        /// <param name="model"></param>
        public void Update_Apply005014(Apply_005014ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            SessionModel sm = SessionModel.Get();
            var CaseNum = "005014";
            ClamAdmin UserInfo = sm.UserInfo.Admin;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    this.Tran(conn, tran);
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", model.Apply.APP_ID);
                    dict2.Add("SRV_ID", "005014");
                    dict2.Add("LastMODTIME", LastMODTIME);

                    var apy = model.Apply;
                    ApplyModel apply = base.GetRow(new ApplyModel { APP_ID = model.Apply.APP_ID });
                    apply.FLOW_CD = apy.FLOW_CD;
                    apply.UPD_TIME = DateTime.Now;
                    apply.TEL = apy.TEL;
                    apply.CNT_NAME = apy.CNT_NAME;
                    apply.CNT_TEL = apy.CNT_TEL;
                    apply.NAME = apy.NAME;
                    apply.IDN = apy.IDN;
                    apply.MOBILE = apy.MOBILE;
                    apply.MOHW_CASE_NO = apy.MOHW_CASE_NO;
                    apply.ADDR = apy.ADDR;
                    apply.ADDR_CODE = apy.ADDR_CODE;
                    apply.FLOW_CD = model.Apply.FLOW_CD;
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_ACC = UserInfo.ACC_NO;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    //base.Update(apply, new ApplyModel { APP_ID = model.Apply.APP_ID });
                    base.Update2(apply, new ApplyModel { APP_ID = model.Apply.APP_ID }, dict2, true);
                    Apply_005014 apply005014 = new Apply_005014();
                    apply005014.InjectFrom(model.Detail);
                    //base.Update(apply005014, new Apply_005014 { APP_ID = model.Apply.APP_ID });
                    base.Update2(apply005014, new Apply_005014 { APP_ID = model.Apply.APP_ID }, dict2, true);
                    // 申請項目
                    if (model.ApplyItems != null && model.ApplyItems.Count >= 0)
                    {
                        for (int i = 0; i < model.ApplyItems.Count; i++)
                        {
                            var item = model.ApplyItems[i];
                            base.Update(item, new Apply_005014_Item { APP_ID = model.Apply.APP_ID, ITEM = item.ITEM });
                        }
                    }
                    if (model.ApplyItems2 != null && model.ApplyItems2.Count >= 0)
                    {
                        for (int i = 0; i < model.ApplyItems2.Count; i++)
                        {
                            var item = model.ApplyItems2[i];
                            base.Update(item, new Apply_005014_Item { APP_ID = model.Apply.APP_ID, ITEM = item.ITEM });
                        }
                    }
                    // 儲存上傳檔案資料
                    ShareDAO shareDao = new ShareDAO();
                    if (model.FileList != null && model.FileList.Count > 0)
                    {
                        DateTime now = DateTime.Now;
                        for (int i = 0; i < model.FileList.Count; i++)
                        {
                            var item = model.FileList[i];
                            if (item != null && !string.IsNullOrEmpty(item.FILE_E) && item.PostFile != null)  // 須補件
                            {
                                string fileUrl = shareDao.PutFile("005014", item.PostFile, string.Format("{0:00}", i));
                                // 儲存檔案 
                                Apply_005014_FILE file014 = new Apply_005014_FILE();
                                file014.APP_ID = model.Apply.APP_ID;
                                file014.SRV_ID = CaseNum;
                                file014.FILE_NAME = Path.GetFileName(fileUrl);
                                file014.FILE_URL = fileUrl;
                                file014.FILE_ID = (i + 1).ToString();
                                file014.MIME = item.PostFile.ContentType;
                                file014.CREATE_DATE = now;
                                file014.DEL_MK = "N";

                                base.Update(file014, new Apply_005014_FILE { APP_ID = model.Apply.APP_ID, FILE_ID = item.FILE_ID });
                            }
                        }
                    }

                    // 補正資訊
                    var maxfreq = base.GetRowList(new TblAPPLY_NOTICE { APP_ID = model.Apply.APP_ID }).Max(x => x.FREQUENCY);
                    if (!maxfreq.HasValue) { maxfreq = 0; }
                    if (model.ApplyNoticeList != null && model.ApplyNoticeList.Count > 0)
                    {
                        maxfreq++;
                        foreach (var it in model.ApplyNoticeList)
                        {
                            it.FREQUENCY = maxfreq;

                            base.Insert(it);
                        }
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public void SendMail_005014(Apply_005014ViewModel model)
        {
            model.GetApplyData(model.Apply.APP_ID);

            string Msg = string.Empty;
            DataTable UndertakerInfo = new DataTable();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    #region 取得承辦人資訊
                    string _sql = @"select ADMIN.NAME,ADMIN.TEL,ADMIN.MAIL
                                    from APPLY join ADMIN on APPLY.PRO_ACC = ADMIN.ACC_NO
                                    where APPLY.APP_ID='" + model.Apply.APP_ID + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(UndertakerInfo);
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    throw new Exception("存檔失敗，請聯絡系統管理員 。");
                }

                bool savestatus = false;
                string MainBody = string.Empty;
                if (model.ApplyNoticeList != null)
                {
                    foreach (var item in model.ApplyNoticeList)
                    {
                        MainBody += "<tr>";
                        MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.Field_NAME + "</td>";
                        MainBody += "<td align=\"left\" style=\"border:1px solid\">" + item.NOTE + "</td>";
                        MainBody += "</tr>";
                        savestatus = true;
                    }
                }

                try
                {

                    SqlTransaction tran = conn.BeginTransaction();
                    this.Tran(conn, tran);
                    string MailBody = "";

                    // 判斷是否補件
                    if (savestatus)
                    {
                        if (model.Apply.FLOW_CD == "2")
                        {
                            MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += "<tr><th align=\"left\">敬啟者:</th></tr>";
                            MailBody += "<tr><td>貨品進口專案申請(案號：" + model.Apply.APP_ID + ")，經本部審查後，尚有待修正事項，請登入「人民申請案線上申辦服務系統」，依審查建議盡速修正申請表單內容，並於5個工作天內完成。</td></tr>";
                            MailBody += "<tr><td>";
                            MailBody += "<table align=\"center\" style=\"width:95%;border:1px solid\">";
                            MailBody += "<tr>";
                            MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正內容</th>";
                            MailBody += "<th align=\"left\" style=\"width:50%;border:1px solid\">修正說明</th>";
                            MailBody += "</tr>";
                            MailBody += MainBody;
                            MailBody += "</table>";
                            MailBody += "</td></tr>";
                            MailBody += "<tr><td></td></tr>";
                            MailBody += "<tr><td>衛生福利部 中醫藥司</td></tr>";
                            //MailBody += "<tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                            //MailBody += "<tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                            //MailBody += "<tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                            MailBody += "</table>";

                            SendMail_Notice(MainBody, model.Apply.NAME, model.ApplyNoticeList.Count,
                                model.Detail.EMAIL.TONotNullString().Replace("@0", "@"), model.Apply.APP_ID, "貨品進口專案申請", "005014", MailBody);
                        }
                    }
                    else
                    {
                        string MOHDATE = GetMOHCaseDate(model.Apply.APP_ID, model.Apply.MOHW_CASE_NO);

                        if (model.Apply.FLOW_CD == "0")
                        {
                            MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += "<tr><th align=\"left\">敬啟者:</th></tr>";
                            MailBody += "<tr><td>本部業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Apply.MOHW_CASE_NO + " 號函，";
                            MailBody += "檢送您申請之貨品進口專案申請(案號:" + model.Apply.APP_ID + ")正本，請查照。</td></tr>";
                            MailBody += "<tr><td></td></tr>";
                            MailBody += "<tr><td>衛生福利部 中醫藥司</td></tr>";
                            //MailBody += "<tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                            //MailBody += "<tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                            //MailBody += "<tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                            MailBody += "</table>";

                            SendMail_Notice("", model.Apply.NAME, 0, model.Detail.EMAIL.TONotNullString().Replace("@0", "@"), model.Apply.APP_ID,
                                "貨品進口專案申請", "005014", MailBody, null, "", "貨品進口專案申請，案件編號:" + model.Apply.APP_ID + "，已收案通知");
                        }

                        if (model.Apply.FLOW_CD == "20")
                        {
                            MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += "<tr><th align=\"left\">敬啟者:</th></tr>";
                            MailBody += "<tr><td>您申請之貨品進口專案申請(案號:" + model.Apply.APP_ID + ")一案，本部歉難同意，";
                            MailBody += "業於 " + MOHDATE.Substring(0, 3) + " 年 " + MOHDATE.Substring(3, 2) + " 月 " + MOHDATE.Substring(5, 2) + " 日 以衛部中字第 " + model.Apply.MOHW_CASE_NO + " 號函回復貴公司。</td></tr>";
                            MailBody += "<tr><td></td></tr>";
                            MailBody += "<tr><td>衛生福利部 中醫藥司</td></tr>";
                            //MailBody += "<tr><td>承辦人:" + UndertakerInfo.Rows[0][0].ToString() + "</td></tr>";
                            //MailBody += "<tr><td>連絡電話:" + UndertakerInfo.Rows[0][1].ToString() + "</td></tr>";
                            //MailBody += "<tr><td>電子郵件:" + UndertakerInfo.Rows[0][2].ToString() + "</td></tr>";
                            MailBody += "</table>";

                            SendMail_Notice("", model.Apply.NAME, 0, model.Detail.EMAIL.TONotNullString().Replace("@0", "@"), model.Apply.APP_ID,
                                "貨品進口專案申請", "005014", MailBody, null, "", "貨品進口專案申請，案件編號:" + model.Apply.APP_ID + "，已收件不收案");
                        }

                    }

                    tran.Commit();

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    logger.Error(ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        #endregion

        #region Apply011006 專科社會工作師證書換發（更名）

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011006FormModel QueryApply_011006(Apply_011006FormModel parm)
        {
            Apply_011006FormModel result = new Apply_011006FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql =
                    @"select app.SRV_ID ,(convert(varchar, app.APP_TIME, 111))as APP_TIME ,(convert(varchar, app.APP_EXT_DATE, 111)) as APP_EXT_DATE,
                                ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM,app.APP_ID,app.ACC_NO,a06.H_TEL,a06.W_TEL,a06.C_ZIPCODE,a06.C_ADDR,a06.H_ZIPCODE,a06.H_ADDR
								,app.MOBILE as MOBILE,a06.EMAIL,a06.PRACTICE_PLACE,a06.SPECIALIST_TYPE,a06.TEST_YEAR,a06.H_EQUAL,app.SEX_CD
								,a06.MERGEYN,app.NAME,app.IDN,app.BIRTHDAY,app.PAY_METHOD,app.PAY_A_FEE,app.FLOW_CD,a06.APPLY_TYPE,app.MOHW_CASE_NO
                                from apply app
                                left join ADMIN ad on app.PRO_ACC = ad.ACC_NO
                                left join APPLY_011006 a06 on app.APP_ID = a06.APP_ID
                                where 1 = 1";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_011006FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011006FileModel GetFileList_011006(string APP_ID)
        {
            var result = new Apply_011006FileModel();
            ShareDAO dao = new ShareDAO();
            result.FILENAM = dao.GetFileGridList(APP_ID);
            result.APP_ID = APP_ID;
            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011006(Apply_011006ViewModel model)
        {
            string Msg = "";
            if (model.Form.FLOW_CD == "2" && model.Form.FileCheck.TONotNullString() == "")
            {
                Msg = "請至少選擇一種補件項目 !";
            }

            if ((model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "5") &&
                model.Form.NOTE.TONotNullString() == "" && model.Form.FileCheck.TONotNullString() != "")
            {
                Msg = "請填寫補件內容 !";
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
            }

            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011006(Apply_011006ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", "011006");
            dict2.Add("LastMODTIME", LastMODTIME);
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = "";
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5")
                    {
                        #region 補件內容
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            #region 補件信件內容
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";
                            #region 補件項目
                            foreach (var item in needchk)
                            {
                                var Field_NAME = "";
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 0:
                                        Field_NAME = "身分證正面影本";
                                        mainproject += mainproject == "" ? "身分證正面影本" : "、身分證正面影本";
                                        anwhere.Field = "FILE_" + "0";
                                        break;
                                    case 1:
                                        Field_NAME = "身分證反面影本";
                                        mainproject += mainproject == "" ? "身分證反面影本" : "、身分證反面影本";
                                        anwhere.Field = "FILE_" + "1";
                                        break;
                                    case 2:
                                        Field_NAME = "照片(規格應同護照照片)";
                                        mainproject += mainproject == "" ? "照片(規格應同護照照片)" : "、照片(規格應同護照照片)";
                                        anwhere.Field = "FILE_" + "2";
                                        break;
                                    case 3:
                                        Field_NAME = "戶籍謄本或戶口名簿影本";
                                        mainproject += mainproject == "" ? "戶籍謄本或戶口名簿影本" : "、戶籍謄本或戶口名簿影本";
                                        anwhere.Field = "FILE_" + "3";
                                        break;
                                    case 4:
                                        Field_NAME = "其他";
                                        mainproject += mainproject == "" ? "其他" : "、其他";
                                        anwhere.Field = "ALL_" + "4";
                                        break;
                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        ProjectStr1 += ProjectStr1 == "" ? "郵局匯票500元１紙，戶名：衛生福利部" : "、郵局匯票500元１紙，戶名：衛生福利部";
                                        anwhere.Field = "OTHER_" + "5";
                                        break;
                                    case 6:
                                        Field_NAME = "專科社會工作師證書正本";
                                        ProjectStr1 += ProjectStr1 == "" ? "專科社會工作師證書正本" : "、專科社會工作師證書正本";
                                        anwhere.Field = "OTHER_" + "6";
                                        break;
                                }
                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                if (model.Form.FLOW_CD == "2")
                                {
                                    Insert(anwhere);
                                }
                                savestatus = true;
                            }
                            #endregion
                            ProjectStr = "需重新上傳之文件為：" + mainproject + "<br/>";
                            ProjectStr += "需掛號郵遞紙本之文件為：" + ProjectStr1 + "<br/>";
                            MainBody += ProjectStr;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            ProjectStr += $"補件內容：{model.Form.NOTE}";
                            #endregion
                        }
                        else
                        {
                            // 當異動成其他案件狀態時，將通知補件項目調整為已補件
                            #region 補件狀態調整
                            TblAPPLY_NOTICE ntcWhere = new TblAPPLY_NOTICE();
                            ntcWhere.APP_ID = model.Form.APP_ID;
                            ntcWhere.ISADDYN = "N";
                            var items = GetRowList(ntcWhere);
                            if (items != null && items.Count() > 0)
                            {
                                var isUpdateNotice = false;
                                foreach (var item in items)
                                {
                                    if (item.Field == "OTHER_5" || item.Field == "OTHER_6")
                                    {
                                        isUpdateNotice = true;
                                    }
                                }
                                if (isUpdateNotice)
                                {
                                    TblAPPLY_NOTICE nUpwhere = new TblAPPLY_NOTICE();
                                    nUpwhere.APP_ID = model.Form.APP_ID;
                                    TblAPPLY_NOTICE upData = new TblAPPLY_NOTICE();
                                    upData.APP_ID = model.Form.APP_ID;
                                    upData.ISADDYN = "Y";
                                    Update(upData, nUpwhere);
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }

                    #region 案件狀態
                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // apply
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;

                        // apply_pay
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_INC_TIME = Convert.ToDateTime(model.Form.PAY_EXT_TIME);
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;

                        //base.Update(newDataPay, wherePay);
                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    // 更新案件狀態
                    newDataApply.MOHW_CASE_NO = model.Form.MOHW_CASE_NO;
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";

                    //base.Update(newDataApply, whereApply);
                    #endregion

                    #region 寄信內容

                    if ((model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4") && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else if (model.Form.FLOW_CD == "5" && savestatus == false)
                    {
                        Msg = "請選擇項目並輸入備註資料!!";
                        tran.Rollback();
                    }
                    else
                    {
                        //base.Update(newDataApply, whereApply);
                        base.Update2(newDataApply, whereApply, dict2, true);
                        string MailBody = "";
                        // 判斷是否要寄信
                        if (savestatus)
                        {
                            switch (model.Form.FLOW_CD)
                            {
                                case "2":
                                    SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, "專科社會工作師證書換發（更名或污損）", "011006", ProjectStr: ProjectStr);
                                    break;
                                // 補正確認完成
                                case "4":
                                    MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                    MailBody += " <tr><th align=\"left\">" + model.Form.NAME + "，您好:</th></tr>";
                                    var inclueStr = string.IsNullOrEmpty(mainproject) ? ProjectStr1 : string.IsNullOrEmpty(ProjectStr1) ? mainproject : $"{mainproject}、{ProjectStr1}";
                                    MailBody += " <tr><td>您所提交的專科社會工作師證書換發（更名或污損）申請，已完成資料補件共" + count.ToString() + "件（包括" + inclueStr + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                    MailBody += " <tr><td align=\"right\">衛生福利部</td></tr>";
                                    MailBody += " <tr><td align=\"right\"> PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";
                                    SendMail(model.Form.EMAIL, $"專科社會工作師證書換發（更名或污損），案件編號{model.Form.APP_ID}狀態通知", MailBody, "011006");
                                    break;
                                // 已接收，處理中
                                case "5":
                                    MailBody = "<table align=\"left\" style=\"width:90%;\">";
                                    MailBody += " <tr><th align=\"left\">" + model.Form.NAME + "，您好:</th></tr>";
                                    MailBody += " <tr><td>您所提交的專科社會工作師證書換發（更名或污損）申請，已完成系統資料填答及上傳程序，本部亦已收到紙本資料共" + count.ToString() + "件（包括" + ProjectStr1 + "）備註:" + model.Form.NOTE + "。將儘速辦理您的申請案件，謝謝。</td></tr>";
                                    MailBody += " <tr><td align=\"right\">衛生福利部</td></tr>";
                                    MailBody += " <tr><td align=\"right\"> PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";
                                    SendMail(model.Form.EMAIL, $"專科社會工作師證書換發（更名或污損），案件編號{model.Form.APP_ID}狀態通知", MailBody, "011006");
                                    break;
                                default:
                                    break;
                            }

                        }
                        switch (model.Form.FLOW_CD)
                        {
                            //case "5":
                            //    SendMail_InPorcess(model.Form.NAME, "專科社會工作師證書換發（更名或污損）", "011006", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, "");
                            //    break;
                            case "9":
                                // 逾期未補件而予結案
                                SendMail_Expired(model.Form.NAME, "專科社會工作師證書換發（更名或污損）", "011006", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, model.Form.NOTE);
                                break;
                            case "0":
                                // 完成申請
                                SendMail_Success(model.Form.NAME, "專科社會工作師證書換發（更名或污損）", "011006", model.Form.EMAIL, model.Form.APP_TIME, model.Form.APP_ID, "");
                                break;
                            default:
                                break;
                        }

                        #endregion
                        tran.Commit();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 查詢付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public DataTable QueryPayInfo_011006(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"select PAY_STATUS_MK,CONVERT(varchar,PAY_INC_TIME,111) PAY_EXT_TIME
                                    from APPLY_PAY
                                    where APP_ID='" + app_id + "'";
                    SqlCommand cmd = new SqlCommand(_sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(result);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }


        public Apply_011006FormModel GetApplyNotice_011006(string app_id)
        {
            Apply_011006FormModel result = new Apply_011006FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011006FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply011007 社工師證書核發（中文）

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011007FormModel QueryApply_011007(Apply_011007FormModel parm)
        {
            Apply_011007FormModel result = new Apply_011007FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
    select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO
    ,app.APP_TIME,app.APP_EXT_DATE
    ,app.NAME,app.IDN,app.BIRTHDAY,app.SEX_CD
    ,app.PAY_METHOD,app.PAY_A_FEE,app.FLOW_CD
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM
    ,a2.APPLY_DATE
    ,a2.EMAIL
    ,a2.W_TEL,a2.H_TEL,a2.MOBILE
    ,a2.C_ZIPCODE,a2.C_ADDR,a2.H_ZIPCODE,a2.H_ADDR,a2.H_EQUAL
    ,a2.TEST_YEAR,a2.TEST_CATEGORY
    ,a2.MERGEYN
    ,a2.FILE_PASSCOPY,a2.FILE_IDNF,a2.FILE_IDNB,a2.FILE_PHOTO
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_011007 a2 on a2.APP_ID=app.APP_ID
    WHERE 1=1
    and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_011007FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011007FileModel GetFileList_011007(string APP_ID)
        {
            var result = new Apply_011007FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);
            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_IDNF_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'2') FILE_IDNB_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'3') FILE_PHOTO_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'4') FILE_PASSCOPY_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_011007FileModel>(_sql, parameters);
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011007(Apply_011007ViewModel model)
        {
            string Msg = "";
            if (model == null || model.Form == null)
            {
                Msg = "存檔失敗，請聯絡系統管理員 !\n";
                return Msg;
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
                return Msg;
            }
            //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
            if ((model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5") && (model.Form.FileCheck.TONotNullString() == ""))
            {
                Msg = "請勾選補件項目 !";
                return Msg;
            }
            if (model.Form.FLOW_CD == "2" && model.Form.FileCheck.TONotNullString() == "")
            {
                Msg += "請至少選擇一種補件項目 !\n";
                if (model.Form.NOTE.TONotNullString() == "") { Msg += "請填寫補件內容 !\n"; }
            }
            return Msg;
        }

        /// <summary>
        /// 存檔-寄信
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011007(Apply_011007ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "社工師證書核發（中文）";
            string s_SRV_ID = "011007";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            // 紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool flag_savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = ""; //郵局匯票500元１紙

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string[] s_Field = new string[] { "考試院考試及格證書影本或電子證書", "身分證正面影本", "身分證反面影本", "照片(規格應同護照照片)" };
                    var Field_NAME = "";
                    //0::完成申請 1::新收案件 2::通知補件 3::補件收件 4::補正確認完成 5::已收到紙本，審查中
                    if (model.Form.FLOW_CD == "2")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        IList<TblAPPLY_NOTICE> andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        int times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            MainBody += "<p class=\"form-control-static\">";

                            foreach (var item in needchk)
                            {
                                if (string.IsNullOrEmpty(item)) continue;
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                #region 補件項目        
                                switch (newitem)
                                {

                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                        Field_NAME = s_Field[newitem]; //"考試院考試及格證書影本或電子證書";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = string.Format("FILE_{0}", newitem);
                                        break;

                                    case 4:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = "ALL_" + "4";
                                        break;

                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        if (!ProjectStr1.Equals("")) { ProjectStr1 += "、"; }
                                        ProjectStr1 += Field_NAME;
                                        anwhere.Field = "OTHER_" + "5";
                                        break;
                                }
                                #endregion
                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = (times + 1);
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                sm.LastMODTIME = LastMODTIME;
                                this.Insert(anwhere);
                                //count++;
                                flag_savestatus = true;
                            }

                            ProjectStr = "";
                            ProjectStr += "需重新上傳之文件為：" + mainproject + "<br />";
                            ProjectStr += "需掛號郵遞紙本之文件為：" + ProjectStr1 + "<br />";
                            MainBody += ProjectStr;

                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";

                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            ProjectStr += $"補件內容：{model.Form.NOTE}<br />";
                        }
                    }
                    bool flag_needchk = false;
                    if (model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5") { flag_needchk = true; }
                    if (flag_needchk)
                    {
                        // 勾選項目串組html
                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            foreach (var item in needchk)
                            {
                                #region 補件項目
                                var newitem = item.TOInt32();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                        Field_NAME = s_Field[newitem]; //"考試院考試及格證書影本或電子證書";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 4:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                }
                                #endregion
                            }
                        }
                        flag_savestatus = true;

                        #region 案件補件異動
                        // 當異動成其他案件狀態時，將通知補件項目調整為已補件
                        TblAPPLY_NOTICE ntcWhere = new TblAPPLY_NOTICE();
                        ntcWhere.APP_ID = model.Form.APP_ID;
                        ntcWhere.ISADDYN = "N";
                        var items = GetRowList(ntcWhere);
                        var isUpdateNotice = false;
                        if (items != null && items.Count() > 0)
                        {
                            foreach (var item in items)
                            {
                                if (item.Field == "OTHER_5") { isUpdateNotice = true; }
                            }
                        }
                        if (isUpdateNotice)
                        {
                            TblAPPLY_NOTICE nUpwhere = new TblAPPLY_NOTICE();
                            nUpwhere.APP_ID = model.Form.APP_ID;
                            TblAPPLY_NOTICE upData = new TblAPPLY_NOTICE();
                            upData.APP_ID = model.Form.APP_ID;
                            upData.ISADDYN = "Y";
                            base.Update(upData, nUpwhere);
                        }
                        #endregion
                    }

                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // apply
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;

                        // apply_pay
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = model.Form.PAY_EXT_TIME; // HelperUtil.TransToDateTime(); //DateTime.Now;
                        newDataPay.UPD_TIME = DateTime.Now;
                        newDataPay.UPD_FUN_CD = "ADM-STORE";
                        newDataPay.UPD_ACC = sm.UserInfo.UserNo;
                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    // 更新案件狀態
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    model.Form.FLOW_CD = model.Form.FLOW_CD ?? "";
                    // 判斷是否要寄信
                    if (flag_savestatus || model.Form.FLOW_CD == "9")
                    {
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "2")
                        {
                            SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, s_SNV_NAME, s_SRV_ID, ProjectStr: ProjectStr);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "4")
                        {
                            string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                            string s_MSG_T1a = "您所提交的{0}申請，已完成資料補件共 {1}件（包括 {2}）。將儘速辦理您的申請案件，謝謝。";
                            string s_MSG_T1 = string.Format(s_MSG_T1a, s_SNV_NAME, count.ToString(), mainproject);
                            string MailBody = "";
                            MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                            MailBody += string.Format(" <tr><td>　　{0}</td></tr>", s_MSG_T1);
                            MailBody += "<tr><td align=\"right\">衛生福利部社會救助及社工司</td></tr>";
                            MailBody += "</table>";
                            SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD.Equals("5"))
                        {
                            SendMail_InPorcess(model.Form.NAME, s_SNV_NAME, "011007", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);
                        }
                    }

                    // FLOW_CD: 9::逾期未補件而予結案
                    if (model.Form.FLOW_CD.Equals("9"))
                    {
                        SendMail_Expired(model.Form.NAME, s_SNV_NAME, "011007", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    }

                    // FLOW_CD: 0::完成申請
                    if (model.Form.FLOW_CD.Equals("0"))
                    {
                        SendMail_Success(model.Form.NAME, s_SNV_NAME, "011007", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");

                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 新增更新付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public string UpdatePayInfo_011007(string app_id, bool IS_PAY_STATUS, DateTime? PayDate, int Pay_A_Fee)
        {
            var result = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    ApplyModel newDataApply = new ApplyModel();
                    ApplyModel whereApply = new ApplyModel() { APP_ID = app_id };
                    if (IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = Pay_A_Fee;
                        newDataApply.UPD_TIME = DateTime.Now;
                        base.Update(newDataApply, whereApply);

                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = PayDate;
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = app_id;

                        base.Update(newDataPay, wherePay);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    result = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }


        public Apply_011007FormModel GetApplyNotice_011007(string app_id)
        {
            Apply_011007FormModel result = new Apply_011007FormModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011007FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply011008 社工師證書換發（更名或汙損）

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011008FormModel QueryApply_011008(Apply_011008FormModel parm)
        {
            Apply_011008FormModel result = new Apply_011008FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
    select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO
    ,app.APP_TIME,app.APP_EXT_DATE
    ,app.NAME,app.IDN,app.BIRTHDAY,app.SEX_CD
    ,app.PAY_METHOD,app.PAY_A_FEE,app.FLOW_CD
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM
    ,a2.APPLY_DATE ,a2.APPLY_TYPE ,a2.CHG_NAME
    ,a2.EMAIL
    ,a2.W_TEL,a2.H_TEL,a2.MOBILE
    ,a2.C_ZIPCODE,a2.C_ADDR,a2.H_ZIPCODE,a2.H_ADDR,a2.H_EQUAL
    ,a2.TEST_YEAR,a2.TEST_CATEGORY
    ,a2.MERGEYN
    ,a2.FILE_PASSCOPY,a2.FILE_IDNF,a2.FILE_IDNB,a2.FILE_PHOTO
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_011008 a2 on a2.APP_ID=app.APP_ID
    WHERE 1=1
    and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_011008FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    //取案件進度-組合字
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011008FileModel GetFileList_011008(string APP_ID)
        {
            var result = new Apply_011008FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);
            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_IDNF_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'2') FILE_IDNB_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'3') FILE_PHOTO_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'4') FILE_PASSCOPY_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'5') FILE_HOUSEHOLD_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_011008FileModel>(_sql, parameters);
                result.APP_ID = APP_ID;
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011008(Apply_011008ViewModel model)
        {
            string Msg = "";
            if (model == null || model.Form == null)
            {
                Msg = "存檔失敗，請聯絡系統管理員 !\n";
                return Msg;
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
                return Msg;
            }
            //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
            if ((model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5") && (model.Form.FileCheck.TONotNullString() == ""))
            {
                Msg = "請勾選補件項目 !";
                return Msg;
            }
            if (model.Form.FLOW_CD == "2" && model.Form.FileCheck.TONotNullString() == "")
            {
                Msg += "請至少選擇一種補件項目 !\n";
                if (model.Form.NOTE.TONotNullString() == "") { Msg += "請填寫補件內容 !\n"; }
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011008(Apply_011008ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "社工師證書換發（更名或汙損）";
            string s_SRV_ID = "011008";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool flag_savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string[] s_Field = new string[] { "考試院考試及格證書影本或電子證書", "身分證正面影本", "身分證反面影本", "照片(規格應同護照照片)", "戶籍謄本或戶口名簿影本" };
                    var Field_NAME = "";

                    //FLOW_CD: 0:完成申請,1:新收案件,5:已收到紙本，審查中,2:通知補件,3:補件收件,4:補正確認完成
                    if (model.Form.FLOW_CD == "2")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        int times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";

                            foreach (var item in needchk)
                            {
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                        Field_NAME = s_Field[newitem];//"考試院考試及格證書影本或電子證書";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = string.Format("FILE_{0}", newitem);
                                        break;
                                    case 5:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = "ALL_" + "5";
                                        break;
                                    case 6:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        if (!ProjectStr1.Equals("")) { ProjectStr1 += "、"; }
                                        ProjectStr1 += Field_NAME;
                                        anwhere.Field = "OTHER_" + "6";
                                        break;
                                    case 7:
                                        Field_NAME = "社會工作師證書正本";
                                        if (!ProjectStr1.Equals("")) { ProjectStr1 += "、"; }
                                        ProjectStr1 += Field_NAME;
                                        anwhere.Field = "OTHER_" + "7";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = (times + 1);
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                sm.LastMODTIME = LastMODTIME;
                                this.Insert(anwhere);
                                //count++;
                                flag_savestatus = true;
                            }

                            ProjectStr = "";
                            ProjectStr += "需重新上傳之文件為：" + mainproject + "<br />";
                            ProjectStr += "需掛號郵遞紙本之文件為：" + ProjectStr1 + "<br />";
                            MainBody += ProjectStr;

                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            ProjectStr += $"補件內容：{model.Form.NOTE}<br />";
                        }
                    }
                    bool flag_needchk = false;
                    if (model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5") { flag_needchk = true; }
                    if (flag_needchk)
                    {
                        // 勾選項目串組html
                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            foreach (var item in needchk)
                            {
                                #region 補件項目
                                var newitem = item.TOInt32();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                        Field_NAME = s_Field[newitem]; //"考試院考試及格證書影本或電子證書";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 5:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 6:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 7:
                                        Field_NAME = "社會工作師證書正本";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                }
                                #endregion
                            }
                        }
                        flag_savestatus = true;

                        #region 案件補件異動
                        // 當異動成其他案件狀態時，將通知補件項目調整為已補件
                        TblAPPLY_NOTICE ntcWhere = new TblAPPLY_NOTICE();
                        ntcWhere.APP_ID = model.Form.APP_ID;
                        ntcWhere.ISADDYN = "N";
                        var items = GetRowList(ntcWhere);
                        var isUpdateNotice = false;
                        if (items != null && items.Count() > 0)
                        {
                            foreach (var item in items)
                            {
                                if (item.Field == "OTHER_6") { isUpdateNotice = true; }
                                if (item.Field == "OTHER_7") { isUpdateNotice = true; }
                            }
                        }
                        if (isUpdateNotice)
                        {
                            TblAPPLY_NOTICE nUpwhere = new TblAPPLY_NOTICE();
                            nUpwhere.APP_ID = model.Form.APP_ID;
                            TblAPPLY_NOTICE upData = new TblAPPLY_NOTICE();
                            upData.APP_ID = model.Form.APP_ID;
                            upData.ISADDYN = "Y";
                            base.Update(upData, nUpwhere);
                        }
                        #endregion
                    }

                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // apply
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;

                        // apply_pay
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = model.Form.PAY_EXT_TIME; // HelperUtil.TransToDateTime(); //DateTime.Now;
                        newDataPay.UPD_TIME = DateTime.Now;
                        newDataPay.UPD_FUN_CD = "ADM-STORE";
                        newDataPay.UPD_ACC = sm.UserInfo.UserNo;
                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    // 更新案件狀態
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    model.Form.FLOW_CD = model.Form.FLOW_CD ?? "";
                    // 判斷是否要寄信
                    if (flag_savestatus || model.Form.FLOW_CD == "9")
                    {
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "2")
                        {
                            SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, s_SNV_NAME, s_SRV_ID, ProjectStr: ProjectStr);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "4")
                        {
                            string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                            string s_MSG_T1a = "您所提交的{0}申請，已完成資料補件共 {1}件（包括 {2}）。將儘速辦理您的申請案件，謝謝。";
                            string s_MSG_T1 = string.Format(s_MSG_T1a, s_SNV_NAME, count.ToString(), mainproject);
                            string MailBody = "";
                            MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                            MailBody += string.Format(" <tr><td>　　{0}</td></tr>", s_MSG_T1);
                            MailBody += "<tr><td align=\"right\">衛生福利部社會救助及社工司</td></tr>";
                            MailBody += "</table>";
                            SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD.Equals("5"))
                        {
                            SendMail_InPorcess(model.Form.NAME, s_SNV_NAME, "011008", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);

                            //string s_NOTE = "";
                            //if (model.Form.NOTE != null && model.Form.NOTE.Length > 0) s_NOTE = string.Format("備註: {0}", model.Form.NOTE);
                            //string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                            //string s_MSG_T1a = "您所提交的{0}申請，已完成系統資料填答及上傳程序，本部亦已收到紙本資料共{1}件（包括 {2}）{3}。將儘速辦理您的申請案件，謝謝。";
                            //string s_MSG_T1 = string.Format(s_MSG_T1a, s_SNV_NAME, count.ToString(), mainproject, s_NOTE);
                            //string MailBody = "";
                            //MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            //MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                            //MailBody += string.Format(" <tr><td>　　{0}</td></tr>", s_MSG_T1);
                            //MailBody += "<tr><td align=\"right\">衛生福利部社會救助及社工司</td></tr>";
                            //MailBody += "</table>";
                            //SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                        }

                    }
                    // FLOW_CD: 9::逾期未補件而予結案
                    if (model.Form.FLOW_CD.Equals("9"))
                    {
                        SendMail_Expired(model.Form.NAME, s_SNV_NAME, "011008", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");

                        //string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                        //string MailBody = "";
                        //MailBody += "<table align=\"left\" style=\"width:90%;\">";
                        //MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                        //MailBody += string.Format(" <tr><td>您於{0}申辦之{1}案件</td></tr>", model.Form.APPLY_DATE_TW, s_SNV_NAME);
                        //MailBody += string.Format("<tr><td align=\"left\">申請編號：{0} 現在進度為'逾期未補件而予結案'</td></tr>", model.Form.APP_ID);
                        //MailBody += "<tr><td align=\"left\">特此通知。感謝您使用衛生福利部線上申辦系統</td></tr>";
                        //MailBody += "<tr><td align=\"left\">請至滿意度問卷填寫滿意度調查表，您的寶貴意見將做為本部改進事項</td></tr>";
                        //MailBody += "<tr><td align=\"rigth\">衛生福利部社會救助及社工司</td></tr>";
                        //MailBody += "<tr><td align=\"left\">PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr>";
                        //MailBody += "</table>";
                        //SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                    }
                    //FLOW_CD: 0::完成申請
                    if (model.Form.FLOW_CD.Equals("0"))
                    {
                        SendMail_Success(model.Form.NAME, s_SNV_NAME, "011008", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 新增更新付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public string UpdatePayInfo_011008(string app_id, bool IS_PAY_STATUS, DateTime? PayDate, int Pay_A_Fee)
        {
            var result = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    ApplyModel newDataApply = new ApplyModel();
                    ApplyModel whereApply = new ApplyModel() { APP_ID = app_id };
                    if (IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = Pay_A_Fee;
                        newDataApply.UPD_TIME = DateTime.Now;
                        base.Update(newDataApply, whereApply);

                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = PayDate;
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = app_id;
                        base.Update(newDataPay, wherePay);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    result = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public Apply_011008FormModel GetApplyNotice_011008(string app_id)
        {
            Apply_011008FormModel result = new Apply_011008FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011008FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply011009 (衛福部)社工師證書補發（遺失）

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011009FormModel QueryApply_011009(Apply_011009FormModel parm)
        {
            Apply_011009FormModel result = new Apply_011009FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
    select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO
    ,app.APP_TIME,app.APP_EXT_DATE
    ,app.NAME,app.IDN,app.BIRTHDAY,app.SEX_CD
    ,app.PAY_METHOD,app.PAY_A_FEE,app.FLOW_CD
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM
    ,a2.APPLY_DATE
    ,a2.EMAIL
    ,a2.W_TEL,a2.H_TEL,a2.MOBILE
    ,a2.C_ZIPCODE,a2.C_ADDR,a2.H_ZIPCODE,a2.H_ADDR,a2.H_EQUAL
    ,a2.TEST_YEAR,a2.TEST_CATEGORY
    ,a2.MERGEYN
    ,a2.FILE_PASSCOPY,a2.FILE_IDNF,a2.FILE_IDNB,a2.FILE_PHOTO
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_011009 a2 on a2.APP_ID=app.APP_ID
    WHERE 1=1";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_011009FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011009FileModel GetFileList_011009(string APP_ID)
        {
            var result = new Apply_011009FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);
            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_IDNF_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'2') FILE_IDNB_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'3') FILE_PHOTO_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'4') FILE_PASSCOPY_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_011009FileModel>(_sql, parameters);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011009(Apply_011009ViewModel model)
        {
            string Msg = "";
            if (model == null || model.Form == null)
            {
                Msg = "存檔失敗，請聯絡系統管理員 !\n";
                return Msg;
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
                return Msg;
            }
            //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
            if ((model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5") && (model.Form.FileCheck.TONotNullString() == ""))
            {
                Msg = "請勾選補件項目 !";
                return Msg;
            }
            if (model.Form.FLOW_CD == "2" && model.Form.FileCheck.TONotNullString() == "")
            {
                Msg += "請至少選擇一種補件項目 !\n";
                if (model.Form.NOTE.TONotNullString() == "") { Msg += "請填寫補件內容 !\n"; }
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011009(Apply_011009ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "社工師證書核發（遺失）";
            string s_SRV_ID = "011009";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            // 紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool flag_savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string[] s_Field = new string[] { "考試院考試及格證書影本或電子證書", "身分證正面影本", "身分證反面影本", "照片(規格應同護照照片)" };
                    var Field_NAME = "";
                    //FLOW_CD: 0:完成申請,1:新收案件,5:已收到紙本，審查中,2:通知補件,3:補件收件,4:補正確認完成
                    if (model.Form.FLOW_CD == "2")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        int times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";

                            foreach (var item in needchk)
                            {
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                        Field_NAME = s_Field[newitem];//"考試院考試及格證書影本或電子證書";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = string.Format("FILE_{0}", newitem);
                                        break;
                                    case 4:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = "ALL_" + "4";
                                        break;
                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        if (!ProjectStr1.Equals("")) { ProjectStr1 += "、"; }
                                        ProjectStr1 += Field_NAME;
                                        anwhere.Field = "OTHER_" + "5";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = (times + 1);
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                sm.LastMODTIME = LastMODTIME;
                                this.Insert(anwhere);
                                flag_savestatus = true;
                            }

                            ProjectStr = "";
                            ProjectStr += "需重新上傳之文件為：" + mainproject + "<br />";
                            ProjectStr += "需掛號郵遞紙本之文件為：" + ProjectStr1 + "<br />";
                            MainBody += ProjectStr;

                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            ProjectStr += $"補件內容：{model.Form.NOTE}<br />";
                        }
                    }
                    bool flag_needchk = false;
                    if (model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5") { flag_needchk = true; }
                    if (flag_needchk)
                    {
                        // 勾選項目串組html
                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            foreach (var item in needchk)
                            {
                                #region 補件項目
                                var newitem = item.TOInt32();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                        Field_NAME = s_Field[newitem]; //"考試院考試及格證書影本或電子證書";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 4:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                }
                                #endregion
                            }
                        }
                        flag_savestatus = true;

                        #region 案件補件異動
                        // 當異動成其他案件狀態時，將通知補件項目調整為已補件
                        TblAPPLY_NOTICE ntcWhere = new TblAPPLY_NOTICE();
                        ntcWhere.APP_ID = model.Form.APP_ID;
                        ntcWhere.ISADDYN = "N";
                        var items = GetRowList(ntcWhere);
                        var isUpdateNotice = false;
                        if (items != null && items.Count() > 0)
                        {
                            foreach (var item in items)
                            {
                                if (item.Field == "OTHER_5") { isUpdateNotice = true; }
                            }
                        }
                        if (isUpdateNotice)
                        {
                            TblAPPLY_NOTICE nUpwhere = new TblAPPLY_NOTICE();
                            nUpwhere.APP_ID = model.Form.APP_ID;
                            TblAPPLY_NOTICE upData = new TblAPPLY_NOTICE();
                            upData.APP_ID = model.Form.APP_ID;
                            upData.ISADDYN = "Y";
                            base.Update(upData, nUpwhere);
                        }
                        #endregion
                    }

                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // apply
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;

                        // apply_pay
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = model.Form.PAY_EXT_TIME; // HelperUtil.TransToDateTime(); //DateTime.Now;
                        newDataPay.UPD_TIME = DateTime.Now;
                        newDataPay.UPD_FUN_CD = "ADM-STORE";
                        newDataPay.UPD_ACC = sm.UserInfo.UserNo;
                        sm.LastMODTIME = LastMODTIME;
                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    // 更新案件狀態
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    sm.LastMODTIME = LastMODTIME;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    model.Form.FLOW_CD = model.Form.FLOW_CD ?? "";
                    // 判斷是否要寄信
                    if (flag_savestatus || model.Form.FLOW_CD == "9")
                    {
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "2")
                        {
                            SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, s_SNV_NAME, s_SRV_ID, ProjectStr: ProjectStr);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "4")
                        {
                            string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                            string s_MSG_T1a = "您所提交的{0}申請，已完成資料補件共 {1}件（包括 {2}）。將儘速辦理您的申請案件，謝謝。";
                            string s_MSG_T1 = string.Format(s_MSG_T1a, s_SNV_NAME, count.ToString(), mainproject);
                            string MailBody = "";
                            MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                            MailBody += string.Format(" <tr><td>　　{0}</td></tr>", s_MSG_T1);
                            MailBody += "<tr><td align=\"right\">衛生福利部社會救助及社工司</td></tr>";
                            MailBody += "</table>";
                            SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD.Equals("5"))
                        {
                            SendMail_InPorcess(model.Form.NAME, s_SNV_NAME, "011009", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);

                            //string s_NOTE = "";
                            //if (model.Form.NOTE != null && model.Form.NOTE.Length > 0) s_NOTE = string.Format("備註: {0}", model.Form.NOTE);
                            //string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                            //string s_MSG_T1a = "您所提交的{0}申請，已完成系統資料填答及上傳程序，本部亦已收到紙本資料共{1}件（包括 {2}）{3}。將儘速辦理您的申請案件，謝謝。";
                            //string s_MSG_T1 = string.Format(s_MSG_T1a, s_SNV_NAME, count.ToString(), mainproject, s_NOTE);
                            //string MailBody = "";
                            //MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            //MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                            //MailBody += string.Format(" <tr><td>　　{0}</td></tr>", s_MSG_T1);
                            //MailBody += "<tr><td align=\"right\">衛生福利部社會救助及社工司</td></tr>";
                            //MailBody += "</table>";
                            //SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                        }

                    }
                    // FLOW_CD: 9::逾期未補件而予結案
                    if (model.Form.FLOW_CD.Equals("9"))
                    {
                        SendMail_Expired(model.Form.NAME, s_SNV_NAME, "011009", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");

                        //string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                        //string MailBody = "";
                        //MailBody += "<table align=\"left\" style=\"width:90%;\">";
                        //MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                        //MailBody += string.Format(" <tr><td>您於{0}申辦之{1}案件</td></tr>", model.Form.APPLY_DATE_TW, s_SNV_NAME);
                        //MailBody += string.Format("<tr><td align=\"left\">申請編號：{0} 現在進度為'逾期未補件而予結案'</td></tr>", model.Form.APP_ID);
                        //MailBody += "<tr><td align=\"left\">特此通知。感謝您使用衛生福利部線上申辦系統</td></tr>";
                        //MailBody += "<tr><td align=\"left\">請至滿意度問卷填寫滿意度調查表，您的寶貴意見將做為本部改進事項</td></tr>";
                        //MailBody += "<tr><td align=\"rigth\">衛生福利部社會救助及社工司</td></tr>";
                        //MailBody += "<tr><td align=\"left\">PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr>";
                        //MailBody += "</table>";
                        //SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                    }
                    //FLOW_CD: 0::完成申請
                    if (model.Form.FLOW_CD.Equals("0"))
                    {
                        SendMail_Success(model.Form.NAME, s_SNV_NAME, "011009", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }



        /// <summary>
        /// 新增更新付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public string UpdatePayInfo_011009(string app_id, bool IS_PAY_STATUS, DateTime? PayExtDate, int Pay_A_Fee)
        {
            var result = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    ApplyModel newDataApply = new ApplyModel();
                    ApplyModel whereApply = new ApplyModel() { APP_ID = app_id };
                    if (IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = Pay_A_Fee;
                        newDataApply.UPD_TIME = DateTime.Now;
                        base.Update(newDataApply, whereApply);

                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = app_id;
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = PayExtDate;
                        newDataPay.UPD_TIME = DateTime.Now;
                        base.Update(newDataPay, wherePay);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    result = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public Apply_011009FormModel GetApplyNotice_011009(string app_id)
        {
            Apply_011009FormModel result = new Apply_011009FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011009FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply011010 全國社會工作專業人員選拔推薦

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011010FormModel QueryApply_011010(Apply_011010FormModel parm)
        {
            Apply_011010FormModel result = new Apply_011010FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
     select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO,convert(varchar,app.APP_TIME,111) APPLY_DATE
    ,app.APP_TIME,app.APP_EXT_DATE,app.FLOW_CD
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM
    ,A2.UNIT_TYPE,A2.UNIT_SUBTYPE,A2.UNIT_NAME,A2.UNIT_DEPART,A2.UNIT_TITLE,A2.UNIT_CNAME,A2.UNIT_TEL,A2.UNIT_EMAIL,A2.UNIT_EMAIL EMAIL
    ,A2.CNT_TYPE,A2.CNT_A,A2.CNT_B,A2.CNT_C,A2.CNT_D,A2.CNT_E,A2.CNT_F,A2.CNT_G,A2.CNT_H
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_011010 A2 on A2.APP_ID=app.APP_ID
    WHERE 1=1 ";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_011010FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_011010FileModel GetFileList_011010(string APP_ID)
        {
            var result = new Apply_011010FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);
            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_EXCEL_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'2') FILE_PDF_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            string _filelist = @" select * from apply_file where 1=1 and isnull(batch_index,'0')>0 and app_id=@APP_ID ";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_011010FileModel>(_sql, parameters);
                result.FILE = conn.Query<Apply_011010FILEModel>(_filelist, parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply011010(Apply_011010ViewModel model)
        {
            string Msg = "";
            if (model == null || model.Form == null)
            {
                Msg = "存檔失敗，請聯絡系統管理員 !\n";
                return Msg;
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
                return Msg;
            }
            //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
            if (model.Form.FLOW_CD == "2" && model.Form.FileCheck.TONotNullString() == "")
            {
                Msg += "請至少選擇一種補件項目 !\n";
                if (model.Form.NOTE.TONotNullString() == "") { Msg += "請填寫補件內容 !\n"; }
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply011010(Apply_011010ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "全國社會工作專業人員選拔推薦";
            string s_SRV_ID = "011010";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            // 紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool flag_savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    string[] s_Field = new string[] { "彙整表(EXCEL檔)", "檢核表(PDF檔)", "推薦表(PDF檔)" };
                    var Field_NAME = "";
                    //FLOW_CD: 0:完成申請,1:新收案件,5:已收到紙本，審查中,2:通知補件,3:補件收件,4:補正確認完成
                    if (model.Form.FLOW_CD == "2")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        int times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";

                            foreach (var item in needchk)
                            {
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                        Field_NAME = s_Field[newitem];//"彙整表(EXCEL檔)", "檢核表(PDF檔)", "推薦表(PDF檔)";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = string.Format("FILE_{0}", newitem);
                                        break;
                                    case 3:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = "ALL_" + "3";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = (times + 1);
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                sm.LastMODTIME = LastMODTIME;
                                this.Insert(anwhere);
                                flag_savestatus = true;
                            }

                            ProjectStr = "";
                            ProjectStr += "需重新上傳之文件為：" + mainproject + "<br />";
                            MainBody += ProjectStr;

                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            ProjectStr += $"補件內容：{model.Form.NOTE}<br />";
                        }
                    }
                    bool flag_needchk = false;
                    if (model.Form.FLOW_CD == "4" || model.Form.FLOW_CD == "5") { flag_needchk = true; }
                    if (flag_needchk)
                    {
                        // 勾選項目串組html
                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            foreach (var item in needchk)
                            {
                                #region 補件項目
                                var newitem = item.TOInt32();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                        Field_NAME = s_Field[newitem]; //"彙整表(EXCEL檔)", "檢核表(PDF檔)", "推薦表(PDF檔)";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                    case 3:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        break;
                                }
                                #endregion
                            }
                        }
                        flag_savestatus = true;

                        #region 案件補件異動
                        // 當異動成其他案件狀態時，將通知補件項目調整為已補件
                        TblAPPLY_NOTICE ntcWhere = new TblAPPLY_NOTICE();
                        ntcWhere.APP_ID = model.Form.APP_ID;
                        ntcWhere.ISADDYN = "N";
                        var items = GetRowList(ntcWhere);
                        var isUpdateNotice = false;
                        if (items != null && items.Count() > 0)
                        {
                            foreach (var item in items)
                            {
                                if (item.Field == "ALL_3") { isUpdateNotice = true; }
                            }
                        }
                        if (isUpdateNotice)
                        {
                            TblAPPLY_NOTICE nUpwhere = new TblAPPLY_NOTICE();
                            nUpwhere.APP_ID = model.Form.APP_ID;
                            TblAPPLY_NOTICE upData = new TblAPPLY_NOTICE();
                            upData.APP_ID = model.Form.APP_ID;
                            upData.ISADDYN = "Y";
                            base.Update(upData, nUpwhere);
                        }
                        #endregion
                    }

                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);

                    // 更新案件狀態
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    sm.LastMODTIME = LastMODTIME;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    model.Form.FLOW_CD = model.Form.FLOW_CD ?? "";
                    // 判斷是否要寄信
                    if (flag_savestatus || model.Form.FLOW_CD == "9")
                    {
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "2")
                        {
                            SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, s_SNV_NAME, s_SRV_ID, ProjectStr: ProjectStr);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD == "4")
                        {
                            string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                            string s_MSG_T1a = "您所提交的{0}申請，已完成資料補件共 {1}件（包括 {2}）。將儘速辦理您的申請案件，謝謝。";
                            string s_MSG_T1 = string.Format(s_MSG_T1a, s_SNV_NAME, count.ToString(), mainproject);
                            string MailBody = "";
                            MailBody += "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                            MailBody += string.Format(" <tr><td>　　{0}</td></tr>", s_MSG_T1);
                            MailBody += "<tr><td align=\"right\">衛生福利部社會救助及社工司</td></tr>";
                            MailBody += "</table>";
                            SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                        }
                        //FLOW_CD: 0::完成申請1::新收案件2::通知補件3::補件收件4::補正確認完成5::已收到紙本，審查中
                        if (model.Form.FLOW_CD.Equals("5"))
                        {
                            SendMail_InPorcess(model.Form.NAME, s_SNV_NAME, "011010", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);
                        }

                    }
                    // FLOW_CD: 9::逾期未補件而予結案
                    if (model.Form.FLOW_CD.Equals("9"))
                    {
                        SendMail_Expired(model.Form.NAME, s_SNV_NAME, "011010", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    }
                    //FLOW_CD: 0::完成申請
                    if (model.Form.FLOW_CD.Equals("0"))
                    {
                        SendMail_Success(model.Form.NAME, s_SNV_NAME, "011010", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        public Apply_011010FormModel GetApplyNotice_011010(string app_id)
        {
            Apply_011010FormModel result = new Apply_011010FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_011010FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply001038

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply001038(Apply_001038FormModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001038");
            dict2.Add("LastMODTIME", LastMODTIME);
            SessionModel sm = SessionModel.Get();
            string Msg = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    // 更新案件狀態
                    ApplyModel appwhere = new ApplyModel();
                    appwhere.APP_ID = model.APP_ID;

                    ApplyModel appdata = new ApplyModel();
                    appdata.InjectFrom(model);
                    appdata.APP_ID = model.APP_ID;
                    appdata.FLOW_CD = model.FLOW_CD;
                    appdata.UPD_TIME = DateTime.Now;
                    appdata.UPD_ACC = sm.UserInfo.UserNo;
                    appdata.UPD_FUN_CD = "ADM-STORE";

                    //Update(appdata, appwhere);
                    base.Update2(appdata, appwhere, dict2, true);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }
        #endregion

        #region Apply001035

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply001035(Apply_001035FormModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001035");
            dict2.Add("LastMODTIME", LastMODTIME);
            SessionModel sm = SessionModel.Get();
            string Msg = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    // 更新案件狀態
                    ApplyModel appwhere = new ApplyModel();
                    appwhere.APP_ID = model.APP_ID;

                    ApplyModel appdata = new ApplyModel();
                    appdata.InjectFrom(model);
                    appdata.APP_ID = model.APP_ID;
                    appdata.FLOW_CD = model.FLOW_CD;
                    appdata.UPD_TIME = DateTime.Now;
                    appdata.UPD_ACC = sm.UserInfo.UserNo;
                    appdata.UPD_FUN_CD = "ADM-STORE";

                    //Update(appdata, appwhere);
                    base.Update2(appdata, appwhere, dict2, true);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }
        #endregion

        #region Apply010001 檔案應用 國健署
        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply010001(Apply_010001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            string Msg = "";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "010001");
            dict2.Add("LastMODTIME", LastMODTIME);
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            string mainproject = "";
            // 紀錄補件欄位
            var count = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.FLOW_CD == "2")
                    {

                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();


                        if (!string.IsNullOrEmpty(model.FileCheck))
                        {

                            var needchk = model.FileCheck.ToSplit(',');
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";
                            foreach (var item in needchk)
                            {
                                var Field_NAME = "";
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 1:
                                        Field_NAME = "代理上傳委任書電子檔 ";
                                        mainproject += mainproject == "" ? "代理上傳委任書電子檔 " : "、代理上傳委任書電子檔 ";
                                        anwhere.Field = "FILE_" + "1";
                                        break;
                                    case 2:
                                        Field_NAME = "代理<法人>上傳登記證影本 ";
                                        mainproject += mainproject == "" ? "代理<法人>上傳登記證影本 " : "、代理<法人>上傳登記證影本 ";
                                        anwhere.Field = "FILE_" + "2";
                                        break;
                                    case 3:
                                        Field_NAME = "其他";
                                        mainproject += mainproject == "" ? "其他" : "、其他";
                                        anwhere.Field = "ALL_" + "3";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = model.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                base.Insert(anwhere);

                                count++;
                                savestatus = true;
                            }
                            MainBody += mainproject;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                        }
                    }
                    else
                    {
                        // 補件確認完成
                        if (model.FLOW_CD == "4" || model.FLOW_CD == "5")
                        {
                            // 勾選項目串組html
                            if (!string.IsNullOrEmpty(model.FileCheck))
                            {
                                var needchk = model.FileCheck.ToSplit(',');
                                count = needchk.Count();
                                foreach (var item in needchk)
                                {
                                    var newitem = item.TOInt32();
                                    #region 補件項目
                                    switch (newitem)
                                    {
                                        case 1:
                                            mainproject += mainproject == "" ? "代理上傳委任書電子檔 " : "、代理上傳委任書電子檔 ";
                                            break;
                                        case 2:
                                            mainproject += mainproject == "" ? "代理<法人>上傳登記證影本 " : "、代理<法人>上傳登記證影本 ";
                                            break;
                                        case 3:
                                            mainproject += mainproject == "" ? "其他" : "、其他";
                                            break;
                                    }
                                    #endregion
                                }
                            }
                            savestatus = true;
                        }
                    }
                    // 更新案件狀態
                    ApplyModel appwhere = new ApplyModel();
                    appwhere.APP_ID = model.APP_ID;

                    ApplyModel appdata = new ApplyModel();
                    appdata.MOHW_CASE_NO = model.MOHW_CASE_NO;
                    appdata.APP_ID = model.APP_ID;
                    appdata.FLOW_CD = model.FLOW_CD;
                    appdata.UPD_TIME = DateTime.Now;
                    appdata.UPD_ACC = sm.UserInfo.UserNo;
                    appdata.UPD_FUN_CD = "ADM-STORE";
                    appdata.NOTICE_NOTE = model.NOTE.TONotNullString();

                    if (model.FLOW_CD == "2" && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                    }
                    else if (model.FLOW_CD == "4" && string.IsNullOrEmpty(model.FileCheck))
                    {
                        Msg = "請選擇已完成的通知補件項目!!";
                    }
                    else
                    {
                        //base.Update(appdata, appwhere);
                        base.Update2(appdata, appwhere, dict2, true);
                    }

                    // 判斷是否要寄信
                    //if (savestatus)
                    //{
                    //}
                    string MailBody = string.Empty;
                    switch (model.FLOW_CD)
                    {
                        case "5":
                            SendMail_InPorcess(model.NAME, "檔案應用申請", "010001", model.MAIL, model.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                            break;
                        case "2":
                            string txt = "<tr><td>補件項目：" + mainproject + "</td></tr>";
                            txt += "<tr><td>補件備註：" + model.NOTE.TONotNullString() + "</td></tr>";
                            SendMail_Notice(MainBody, model.NAME, count, model.MAIL, model.APP_ID, "檔案應用申請", "010001", null, null, txt);
                            break;
                        case "4":
                            MailBody = "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += " <tr><th align=\"left\">" + model.NAME + "，您好:</th></tr>";
                            MailBody += " <tr><td>您所提交的檔案應用申請，已完成資料補件共" + count.ToString() + "件（包括" + mainproject + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                            MailBody += " <tr><td align=\"right\">國民健康署秘書室敬上</td></tr></table>";
                            SendMail(model.MAIL, $"檔案應用申請，案件編號{model.APP_ID} 案件狀態通知", MailBody, "010001");
                            break;
                        case "9":
                            SendMail_Expired(model.NAME, "檔案應用申請", "010001", model.MAIL, model.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.NOTE);
                            break;
                        case "8":
                            MailBody = "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += " <tr><th align=\"left\">" + model.NAME + "，您好:</th></tr>";
                            MailBody += " <tr><td>您所提交的檔案應用申請，申請編號：" + model.APP_ID + " 現在進度為'退件通知'。</td></tr>";
                            MailBody += " <tr><td>退件原因：" + model.NOTE + "</td></tr>";
                            MailBody += " <tr><td>特此通知。感謝您使用衛生福利部線上申辦系統</td></tr>";
                            MailBody += " <tr><td>國民健康署秘書室敬上</td></tr>";
                            MailBody += " <tr><td>PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";
                            SendMail(model.MAIL, $"檔案應用申請，案件編號{model.APP_ID} 案件狀態通知", MailBody, "010001");
                            break;
                        default:
                            break;
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_010001FileModel GetFileList_010001(string APP_ID)
        {
            var result = new Apply_010001FileModel();
            ShareDAO dao = new ShareDAO();
            var file1 = dao.GetFileGridList(APP_ID, "1", "N").FirstOrDefault();
            var file2 = dao.GetFileGridList(APP_ID, "2", "N").FirstOrDefault();
            result.FILE_1 = file1;
            result.FILE_2 = file2;
            result.APP_ID = APP_ID;

            return result;
        }

        public byte[] PrintPdf010001(string id)
        {
            byte[] result = null;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                Form001005Action action = new Form001005Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                result = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        #endregion

        #region Apply012001 檔案應用 秘書室
        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply012001(Apply_012001FormModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "012001");
            dict2.Add("LastMODTIME", LastMODTIME);
            SessionModel sm = SessionModel.Get();
            string Msg = "";
            string mainproject = "";

            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.FLOW_CD == "2")
                    {
                        #region 補件內容

                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;
                        // 已補件次數
                        var times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();


                        if (!string.IsNullOrEmpty(model.FileCheck))
                        {

                            var needchk = model.FileCheck.ToSplit(',');
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";
                            foreach (var item in needchk)
                            {
                                var Field_NAME = "";
                                var newitem = item.TOInt32();
                                anwhere = new TblAPPLY_NOTICE();
                                switch (newitem)
                                {
                                    case 1:
                                        Field_NAME = "代理上傳委任書電子檔 ";
                                        mainproject += mainproject == "" ? "代理上傳委任書電子檔 " : "、代理上傳委任書電子檔 ";
                                        anwhere.Field = "FILE_" + "1";
                                        break;
                                    case 2:
                                        Field_NAME = "代理<法人>上傳登記證影本 ";
                                        mainproject += mainproject == "" ? "代理<法人>上傳登記證影本 " : "、代理<法人>上傳登記證影本 ";
                                        anwhere.Field = "FILE_" + "2";
                                        break;
                                    case 3:
                                        Field_NAME = "其他";
                                        mainproject += mainproject == "" ? "其他" : "、其他";
                                        anwhere.Field = "ALL_" + "3";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = times + 1;
                                anwhere.NOTE = model.NOTE;
                                anwhere.Field_NAME = Field_NAME;
                                Insert(anwhere);

                                count++;
                                savestatus = true;
                            }
                            MainBody += mainproject;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                        }
                        #endregion
                    }
                    else
                    {
                        // 補件確認完成
                        if (model.FLOW_CD == "4" || model.FLOW_CD == "5")
                        {
                            // 勾選項目串組html
                            if (!string.IsNullOrEmpty(model.FileCheck))
                            {
                                var needchk = model.FileCheck.ToSplit(',');
                                count = needchk.Count();
                                foreach (var item in needchk)
                                {
                                    var newitem = item.TOInt32();
                                    #region 補件項目
                                    switch (newitem)
                                    {
                                        case 1:
                                            mainproject += mainproject == "" ? "代理上傳委任書電子檔 " : "、代理上傳委任書電子檔 ";
                                            break;
                                        case 2:
                                            mainproject += mainproject == "" ? "代理<法人>上傳登記證影本 " : "、代理<法人>上傳登記證影本 ";
                                            break;
                                        case 3:
                                            mainproject += mainproject == "" ? "其他" : "、其他";
                                            break;
                                    }
                                    #endregion
                                }
                            }
                            savestatus = true;
                        }
                    }
                    // 更新案件狀態
                    ApplyModel appwhere = new ApplyModel();
                    appwhere.APP_ID = model.APP_ID;

                    ApplyModel appdata = new ApplyModel();
                    appdata.APP_ID = model.APP_ID;
                    appdata.MOHW_CASE_NO = model.MOHW_CASE_NO;
                    appdata.FLOW_CD = model.FLOW_CD;
                    appdata.UPD_TIME = DateTime.Now;
                    appdata.UPD_ACC = sm.UserInfo.UserNo;
                    appdata.UPD_FUN_CD = "ADM-STORE";
                    appdata.NOTICE_NOTE = model.NOTE.TONotNullString();

                    if (model.FLOW_CD == "2" && savestatus == false)
                    {
                        Msg = "請選擇補件項目並輸入備註資料!!";
                    }
                    else if (model.FLOW_CD == "4" && string.IsNullOrEmpty(model.FileCheck))
                    {
                        Msg = "請選擇已完成的通知補件項目!!";
                    }
                    else
                    {
                        base.Update2(appdata, appwhere, dict2, true);
                    }

                    // 判斷是否要寄信
                    //if (savestatus)
                    //{
                    //}
                    string MailBody = string.Empty;
                    switch (model.FLOW_CD)
                    {
                        case "5":
                            SendMail_InPorcess(model.NAME, "檔案應用申請", "012001", model.MAIL, model.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                            break;
                        case "2":

                            string txt = "<tr><td>補件項目：" + mainproject + "</td></tr>";
                            txt += "<tr><td>補件備註：" + model.NOTE.TONotNullString() + "</td></tr>";
                            SendMail_Notice(MainBody, model.NAME, count, model.MAIL, model.APP_ID, "檔案應用申請", "012001", null, null, txt);
                            break;
                        case "4":
                            MailBody = "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += " <tr><th align=\"left\">" + model.NAME + "，您好:</th></tr>";
                            MailBody += " <tr><td>您所提交的檔案應用申請，已完成資料補件共" + count.ToString() + "件（包括" + mainproject + "）。將儘速辦理您的申請案件，謝謝。</td></tr>";
                            MailBody += " <tr><td align=\"right\">衛生福利部秘書處敬上</td></tr></table>";
                            SendMail(model.MAIL, $"檔案應用申請，案件編號{model.APP_ID} 案件狀態通知", MailBody, "010001");
                            break;
                        case "9":
                            SendMail_Expired(model.NAME, "檔案應用申請", "012001", model.MAIL, model.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.NOTE);
                            break;
                        case "8":
                            MailBody = "<table align=\"left\" style=\"width:90%;\">";
                            MailBody += " <tr><th align=\"left\">" + model.NAME + "，您好:</th></tr>";
                            MailBody += " <tr><td>您所提交的檔案應用申請，申請編號：" + model.APP_ID + " 現在進度為'退件通知'。</td></tr>";
                            MailBody += " <tr><td>退件原因：" + model.NOTE + "</td></tr>";
                            MailBody += " <tr><td>特此通知。感謝您使用衛生福利部線上申辦系統</td></tr>";
                            MailBody += " <tr><td>衛生福利部秘書處敬上</td></tr>";
                            MailBody += " <tr><td>PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr></table>";
                            SendMail(model.MAIL, $"檔案應用申請，案件編號{model.APP_ID} 案件狀態通知", MailBody, "012001");
                            break;
                        default:
                            break;
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        public Apply_012001FileModel GetFileList_012001(string APP_ID)
        {
            var result = new Apply_012001FileModel();
            ShareDAO dao = new ShareDAO();
            var file1 = dao.GetFileGridList(APP_ID, "1", "N").FirstOrDefault();
            var file2 = dao.GetFileGridList(APP_ID, "2", "N").FirstOrDefault();
            result.FILE_1 = file1;
            result.FILE_2 = file2;
            result.APP_ID = APP_ID;

            return result;
        }
        #endregion

        #region Apply001008 醫事人員請領英文證明書
        /// <summary>
        /// 查詢醫事人員請領英文證明書申請資料
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public Apply_001008FormModel QueryApply_001008(string APP_ID)
        {
            Apply_001008FormModel result = new Apply_001008FormModel(APP_ID);
            result.Apply = new ApplyModel();
            result.FileList = new Apply_001008FileModel();
            string srvID = "001008";

            // 案件基本資訊
            #region 案件基本資訊
            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var applydata = base.GetRow(aly);
            result.APP_ID_TEXT = APP_ID;
            result.SRV_ID = srvID;
            result.APPLY_DATE = HelperUtil.DateTimeToString(applydata.APP_TIME);
            result.APP_EXT_DATE = applydata.APP_EXT_DATE;
            result.PRO_ACC = applydata.PRO_ACC;
            result.FLOW_CD = applydata.FLOW_CD;
            result.Apply.PAY_A_FEE = applydata.PAY_A_FEE;
            result.Apply.PAY_A_PAID = applydata.PAY_A_PAID;
            #endregion

            // 基本資料
            Apply_001008Model app = new Apply_001008Model();
            app.APP_ID = APP_ID;
            var appdata = base.GetRow(app);

            // 載入表件資料
            #region 申辦表件填寫資料
            result.IDN = applydata.IDN;
            result.NAME = applydata.NAME;
            result.ENAME = applydata.ENAME;
            result.ENAME_ALIAS = appdata.ENAME_ALIAS;
            result.REMARK = appdata.REMARK;
            result.TOTAL_MEM = Convert.ToString(appdata.TOTAL_MEM);

            //地址
            TblZIPCODE zip = new TblZIPCODE();
            zip.ZIP_CO = applydata.ADDR_CODE;
            var address = base.GetRow(zip);

            result.ADDR_ZIP = applydata.ADDR_CODE;
            if (address != null)
            {
                result.ADDR_ZIP_ADDR = address.CITYNM + address.TOWNNM;
            }
            result.ADDR = applydata.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");

            result.TEL = applydata.TEL;
            result.MOBILE = applydata.MOBILE;
            result.EMAIL = appdata.EMAIL;
            result.MERGEYN = appdata.MERGEYN;
            result.Note = applydata.NOTICE_NOTE;
            #endregion

            #region 醫事人員證書grid
            if (result.ME != null && result.ME.GoodsList != null)
            {
                foreach (var item in result.ME.GoodsList)
                {
                    item.ME_ISSUE_DATE_AD = HelperUtil.DateTimeToString(item.ME_ISSUE_DATE);
                }
            }
            #endregion

            #region 專科證書grid
            if (result.PR != null && result.PR.GoodsList != null)
            {
                foreach (var item in result.PR.GoodsList)
                {
                    item.PR_ISSUE_DATE_AD = HelperUtil.DateTimeToString(item.PR_ISSUE_DATE);
                    item.PR_EF_DATE_S_AD = HelperUtil.DateTimeToString(item.PR_EF_DATE_S);
                    item.PR_EF_DATE_E_AD = HelperUtil.DateTimeToString(item.PR_EF_DATE_E);
                }
            }
            #endregion

            #region 佐證附件(只先撈各項附件最新一筆,不查歷程） 
            //繳費紀錄照片或pdf檔案 & 護照影本電子檔 (各項最後一次上傳)
            result.FileList = this.GetFile_001008(APP_ID);

            //醫事人員/專科中文證書電子檔(各筆最後一次上傳)
            result.ATHs = this.GetAthFile_001008(APP_ID);
            #endregion

            #region 繳費資料
            APPLY_PAY pay = this.QueryApplyPay(APP_ID);

            result.IsPay = false;
            if (pay != null)
            {
                result.PAY_MONEY = pay.PAY_STATUS_MK == "Y" ? applydata.PAY_A_PAID : 0;
                result.PAY_ACT_TIME_AC = HelperUtil.DateTimeToString(pay.PAY_ACT_TIME).TONotNullString();
                result.PAY_EXT_TIME_AC = HelperUtil.DateTimeToString(pay.PAY_EXT_TIME).TONotNullString();
                result.PAY_INC_TIME_AC = HelperUtil.DateTimeToString(pay.PAY_INC_TIME).TONotNullString();
                result.IsPay = pay.PAY_STATUS_MK == "Y" ? true : false;

                result.PAY_METHOD = pay.PAY_METHOD;
                switch (pay.PAY_METHOD)
                {
                    case "C":
                        result.PAY_METHOD_NAME = "信用卡";
                        break;
                    case "D":
                        result.PAY_METHOD_NAME = "匯票";
                        break;
                    case "T":
                        result.PAY_METHOD_NAME = "劃撥";
                        break;
                    case "B":
                        result.PAY_METHOD_NAME = "臨櫃";
                        break;
                    case "S":
                        result.PAY_METHOD_NAME = "超商";
                        break;
                    default:
                        result.PAY_METHOD_NAME = "";
                        break;
                }
            }

            ShareDAO shareDAO = new ShareDAO();
            if (shareDAO.CalculationDocDate(srvID, APP_ID) && applydata.FLOW_CD == "2")
            {
                //補件通知已逾期
                result.IsNotice = "N";
            }
            else
            {
                result.IsNotice = "Y";
            }
            #endregion

            #region 案件歷程
            switch (applydata.FLOW_CD)
            {
                case "2":
                    var dictionary = new Dictionary<string, object>
                    {
                        { "@APP_ID", APP_ID }
                    };
                    var parameters = new DynamicParameters(dictionary);

                    using (SqlConnection conn = DataUtils.GetConnection())
                    {
                        try
                        {
                            string sql = @"SELECT APP_ID,
                                    STUFF((
	                                    SELECT  ','+FIELD 
	                                    FROM APPLY_NOTICE X
	                                    WHERE 1=1
	                                    AND A.APP_ID=X.APP_ID
	                                    AND X.ISADDYN='N'
	                                    FOR XML PATH('')
                                    ),1,1,'') Field
                                    /*,(SELECT DISTINCT NOTE FROM APPLY_NOTICE X WHERE X.APP_ID=A.APP_ID AND X.ISADDYN='N') NOTE*/
                                    FROM APPLY A
                                    WHERE 1=1
                                    AND A.APP_ID=@APP_ID
                                    ";
                            TblAPPLY_NOTICE notice = conn.QueryFirst<TblAPPLY_NOTICE>(sql, parameters);
                            if (notice != null)
                            {
                                result.NoticeCheck = notice.Field;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.Message, ex);
                            //result = null;
                        }
                        finally
                        {
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                    TblAPPLY_NOTICE notice_where = new TblAPPLY_NOTICE();
                    notice_where.APP_ID = APP_ID;
                    notice_where.ISADDYN = "N";
                    var notelist = GetRowList(notice_where);
                    if (notelist != null && notelist.Count > 0)
                    {
                        result.Note = notelist.OrderByDescending(x => x.ADD_TIME).FirstOrDefault().NOTE.TONotNullString();
                    }
                    break;
                case "9":
                    //逾期未補件而予結案
                    if (applydata != null)
                    {
                        result.Note = applydata.NOTICE_NOTE;
                    }
                    break;
                case "12":
                    //核可(發文歸檔)
                    if (appdata != null)
                    {
                        result.MAIL_DATE_AD = HelperUtil.DateTimeToString(appdata.MAIL_DATE);
                        result.MAIL_BARCODE = appdata.MAIL_BARCODE;
                    }
                    break;
                case "15":
                    //自請撤銷
                    if (applydata != null)
                    {
                        result.Note = applydata.NOTICE_NOTE;
                    }
                    break;
                case "8":
                    //退件通知
                    if (applydata != null)
                    {
                        result.Note = applydata.NOTICE_NOTE;
                    }
                    break;
                default:
                    break;
            }

            #endregion
            return result;
        }

        /// <summary>
        /// 查詢繳費資訊
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public APPLY_PAY QueryApplyPay(string APP_ID)
        {
            APPLY_PAY result = null;

            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "@APP_ID",APP_ID }
            };
            DynamicParameters parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
                select APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_ACT_TIME, PAY_EXT_TIME, PAY_INC_TIME, PAY_METHOD, PAY_STATUS_MK, PAY_RET_CD,
                PAY_RET_MSG, BATCH_NO, APPROVAL_CD, PAY_RET_NO, INVOICE_NO, PAY_DESC, CARD_NO, HOST_TIME, TRANS_RET, SESSION_KEY, AUTH_DATE,
                AUTH_NO, SETTLE_DATE, OTHER, ROC_ID, CLIENT_IP, OID, SID, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                from apply_pay 
                where app_id=@APP_ID
                ";

                try
                {
                    result = conn.QueryFirst<APPLY_PAY>(_sql, parameters);
                }
                catch (Exception ex)
                {
                    logger.Warn("QueryApplyPay ex:" + ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取上傳附件檔(apply_file)
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public Apply_001008FileModel GetFile_001008(string APP_ID)
        {
            var result = new Apply_001008FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                string _sql = @"select
                concat(isnull(reverse(substring(reverse(fl1.filename),1,charindex('\',reverse(fl1.filename)) - 1)),''),',',fl1.APP_ID,',',cast(fl1.FILE_NO as varchar),',',isnull(cast(fl1.SRC_NO as varchar),'0')) as FILE_PAYRECORD_TEXT,
                concat(isnull(reverse(substring(reverse(fl2.filename),1,charindex('\',reverse(fl2.filename)) - 1)),''),',',fl2.APP_ID,',',cast(fl2.FILE_NO as varchar),',',isnull(cast(fl2.SRC_NO as varchar),'0')) as FILE_PASSPORT_TEXT
                from apply app";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '1' and APP_ID = @APP_ID order by ADD_TIME desc)as fl1 on app.APP_ID = fl1.APP_ID ";
                _sql += " left join (select top 1 * from APPLY_FILE where FILE_NO = '2' and APP_ID = @APP_ID order by ADD_TIME desc)as fl2 on app.APP_ID = fl2.APP_ID ";
                _sql += "where 1 = 1";
                _sql += "and app.APP_ID = @APP_ID ";

                try
                {
                    result = conn.QueryFirst<Apply_001008FileModel>(_sql, parameters);
                    result.APP_ID = APP_ID;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取得醫事人員/專科中文證書電子檔各項電子檔最後一次上傳資料
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public IList<ES.Models.ViewModels.Apply_001008_AthViewModel> GetAthFile_001008(string APP_ID)
        {
            var result = new List<ES.Models.ViewModels.Apply_001008_AthViewModel>();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
                            WITH MAXATH AS (
	                            SELECT APP_ID,SRL_NO,MAX(NOTICE_NO) MAX_NOTICE_NO
	                            FROM APPLY_001008_ATH 
	                            WHERE APP_ID=@APP_ID
	                            GROUP BY APP_ID,SRL_NO 
                            )
                            SELECT A.SRL_NO,
                            CASE WHEN ISNULL(A.ATH_UP,'')='' THEN 'N' ELSE 'Y' END HAS_OLDFILE,
                            concat(ISNULL(REVERSE(SUBSTRING(REVERSE(A.ATH_UP),1,CHARINDEX('\',REVERSE(A.ATH_UP)) - 1)),''),',',ISNULL(A.APP_ID,'') ,',' , ISNULL(CAST(A.SRL_NO AS VARCHAR),'') ,',',A.NOTICE_NO) AS FILE_1_TEXT
                            FROM APPLY_001008_ATH A
                            JOIN MAXATH B ON A.APP_ID=B.APP_ID AND A.SRL_NO=B.SRL_NO AND A.NOTICE_NO=B.MAX_NOTICE_NO
                            WHERE 1=1 
                            ORDER BY A.APP_ID,A.SRL_NO  
                            ";
                try
                {
                    result = conn.Query<ES.Models.ViewModels.Apply_001008_AthViewModel>(_sql, parameters).ToList<ES.Models.ViewModels.Apply_001008_AthViewModel>();
                }
                catch (Exception ex)
                {
                    logger.Warn("GetAthFile_001008 ex:" + ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 取得上傳附件歷程
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <param name="FILE_NO"></param>
        /// <returns></returns>
        public IList<Apply_001008FileHisModel> GetFileHis_001008(string APP_ID, string FILE_NO)
        {
            IList<Apply_001008FileHisModel> result = null;

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }, { "@FILE_NO" , FILE_NO }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                string _sql = @"SELECT
                        B.FILENAME,B.FILE_NO,B.ADD_TIME
                        ,CONCAT(ISNULL(REVERSE(SUBSTRING(REVERSE(B.FILENAME),1,CHARINDEX('\',REVERSE(B.FILENAME)) - 1)),''),',',B.APP_ID,',',CAST(B.FILE_NO AS VARCHAR),',',ISNULL(CAST(B.SRC_NO AS VARCHAR),'0')) AS FILE_TEXT
                        FROM APPLY A 
                        JOIN APPLY_FILE B ON A.APP_ID = B.APP_ID 
                        WHERE 1 = 1
                        AND A.APP_ID=@APP_ID
                        AND B.FILE_NO=@FILE_NO 
                        ORDER BY B.ADD_TIME DESC ";
                try
                {
                    result = conn.Query<Apply_001008FileHisModel>(_sql, parameters).ToList();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取得醫事人員/專科中文證書電子檔已上傳的所有流水號資料
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public IList<Apply_001008_AthModel> GetAthSRLNo_001008(string APP_ID)
        {
            IList<Apply_001008_AthModel> result = new List<Apply_001008_AthModel>();

            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "@APP_ID",APP_ID }
            };
            DynamicParameters parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
                            SELECT DISTINCT APP_ID,SRL_NO 
                            FROM APPLY_001008_ATH WHERE APP_ID=@APP_ID 
                            ORDER BY SRL_NO
                            ";
                try
                {
                    result = conn.Query<Apply_001008_AthModel>(_sql, parameters).ToList<Apply_001008_AthModel>();
                }
                catch (Exception ex)
                {
                    logger.Warn("GetFileSRLNo_001008 ex:" + ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取得醫事人員/專科證明書電子檔資料
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <param name="SRL_NO"></param>
        /// <returns></returns>
        public IList<ES.Models.ViewModels.Apply_001008_AthViewModel> GetAthFileHis_001008(string APP_ID, string SRL_NO)
        {
            IList<ES.Models.ViewModels.Apply_001008_AthViewModel> result = new List<ES.Models.ViewModels.Apply_001008_AthViewModel>();

            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "@APP_ID",APP_ID }, { "@SRL_NO", SRL_NO }
            };
            DynamicParameters parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
                            SELECT
                            B.NOTICE_NO,B.ATH_UP,B.SRL_NO,B.ADD_TIME
                            ,CONCAT(ISNULL(REVERSE(SUBSTRING(REVERSE(B.ATH_UP),1,CHARINDEX('\',REVERSE(B.ATH_UP)) - 1)),''),',',B.APP_ID,',',CAST(B.SRL_NO AS VARCHAR),',',ISNULL(CAST(B.SRC_NO AS VARCHAR),'0')) AS FILE_1_TEXT
                            FROM APPLY A 
                            JOIN APPLY_001008_ATH B ON A.APP_ID = B.APP_ID 
                            WHERE 1 = 1
                            AND A.APP_ID=@APP_ID
                            AND B.SRL_NO=@SRL_NO
                            ORDER BY B.ADD_TIME DESC
                            ";
                try
                {
                    result = conn.Query<ES.Models.ViewModels.Apply_001008_AthViewModel>(_sql, parameters).ToList<ES.Models.ViewModels.Apply_001008_AthViewModel>();
                }
                catch (Exception ex)
                {
                    logger.Warn("GetAthFileHis_001008 ex:" + ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 醫事人員請領英文證明書(審核)存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveApply001008(Apply_001008FormModel model)
        {
            string s_SRV_ID = "001008";
            string s_SRV_NAME = "醫事人員或公共衛生師請領英文證明書";
            string s_FUN_CD = "ADM-STORE";
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string msg = "";

            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo.Admin;

            Apply_001008Model apply001008where = new Apply_001008Model();
            apply001008where.APP_ID = model.APP_ID;
            Apply_001008Model apply001008 = null;

            apply001008 = this.GetRow(apply001008where);

            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = "";

            //前台案件狀態訊息通知內容設定
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                try
                {
                    #region 補件通知(apply_notice)
                    // 取得補件紀錄
                    TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                    anwhere.APP_ID = model.APP_ID;
                    var andata = GetRowList(anwhere);

                    // 只取回最後一次補件的次數
                    var newandaata = from a in andata
                                     orderby a.FREQUENCY descending
                                     select a;
                    // 目前補件次數
                    int times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                    if (model.FLOW_CD == "2" && !string.IsNullOrEmpty(model.NoticeCheck))
                    {
                        var needchk = model.NoticeCheck.ToSplit(',');
                        count = needchk.Count();

                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">";

                        foreach (var item in needchk)
                        {
                            var Field_NAME = "";
                            var newitem = item.TOInt32();
                            anwhere = new TblAPPLY_NOTICE();

                            switch (newitem)
                            {
                                case 0:
                                    Field_NAME = "繳費紀錄照片或pdf檔案";
                                    if (!mainproject.Equals("")) { mainproject += "、"; }
                                    mainproject += Field_NAME;
                                    anwhere.Field = "FILE_" + "0";

                                    break;
                                case 1:
                                    Field_NAME = "護照影本電子檔";
                                    if (!mainproject.Equals("")) { mainproject += "、"; }
                                    mainproject += Field_NAME;
                                    anwhere.Field = "FILE_" + "1";
                                    break;
                                case 2:
                                    Field_NAME = "醫事人員或公共衛生師/專科中文證書電子檔";
                                    if (!mainproject.Equals("")) { mainproject += "、"; }
                                    mainproject += Field_NAME;
                                    anwhere.Field = "FILE_" + "2";
                                    break;
                                case 3:
                                    Field_NAME = "其他";
                                    if (!mainproject.Equals("")) { mainproject += "、"; }
                                    mainproject += Field_NAME;
                                    anwhere.Field = "ALL_" + "3";
                                    break;
                            }

                            anwhere.ADD_TIME = DateTime.Now;
                            anwhere.APP_ID = model.APP_ID;
                            anwhere.ISADDYN = "N";
                            anwhere.FREQUENCY = (times + 1);
                            anwhere.NOTE = model.Note;
                            anwhere.Field_NAME = Field_NAME;

                            this.Insert(anwhere);

                            //有執行補件
                            savestatus = true;
                        }

                        MainBody += mainproject;

                        MainBody += "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Note + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                    }
                    else if (model.FLOW_CD == "9" || model.FLOW_CD == "15" || model.FLOW_CD == "8")
                    {
                        //9:逾期未補件而予結案 //15:自請撤銷 // 8:退件通知
                        noteContent.Add("備註", model.Note.TONotNullString());
                        MainBody = GetNote(noteContent);
                    }
                    else if (model.FLOW_CD == "12")
                    {
                        // 12:核可(發文歸檔)
                        noteContent.Add("郵寄日期", model.MAIL_DATE_AD);
                        noteContent.Add("掛號條碼", model.MAIL_BARCODE);
                        MainBody = GetNote(noteContent);
                    }
                    #endregion

                    #region 更新案件主檔(apply)
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.FLOW_CD;
                    apply.NOTICE_NOTE = model.Note;

                    if (apply.FLOW_CD == "2")
                    {
                        //補件通知
                        apply.APPLY_NOTICE_DATE = DateTime.Now;
                        //apply.ISMODIFY = model.Apply.ISMODIFY;
                        apply.MAILBODY = MainBody;
                    }
                    else if (apply.FLOW_CD == "9")
                    {
                        //逾期未補件而予結案
                        apply.MAILBODY = MainBody;
                    }
                    else if (apply.FLOW_CD == "12")
                    {
                        //核可(發文歸檔)
                        apply.MAILBODY = MainBody;
                    }

                    //apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = s_FUN_CD;
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.NOTIBODY = string.IsNullOrWhiteSpace(model.NoticeCheck) ? "" : model.Note;

                    if (model.IsPay)
                    {
                        //回寫 apply(申請時已繳交費用 pay_a_paid) 註記成已繳費
                        apply.PAY_A_PAID = model.PAY_MONEY;
                    }

                    //base.Update(apply, whereApply);
                    base.Update2(apply, whereApply, dict2, true);
                    #endregion

                    #region 更新醫事人員請領英文證明書（apply_001008)
                    if (model.FLOW_CD == "12")
                    {
                        Apply_001008Model whereApply001008 = new Apply_001008Model();
                        whereApply001008.APP_ID = model.APP_ID;
                        Apply_001008Model apply001008Model = new Apply_001008Model();

                        apply001008Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AD);
                        apply001008Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001008Model.UPD_TIME = DateTime.Now;
                        apply001008Model.UPD_FUN_CD = s_FUN_CD;
                        apply001008Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001008Model.DEL_MK = "N";
                        //this.Update(apply001008Model, whereApply001008);
                        base.Update2(apply001008Model, whereApply001008, dict2, true);
                    }
                    #endregion

                    #region 更新繳費記錄(apply_pay)
                    if (model.IsPay)
                    {
                        APPLY_PAY applyPayWhere = new APPLY_PAY();
                        applyPayWhere.APP_ID = model.APP_ID;
                        applyPayWhere.PAY_STATUS_MK = "N";

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = model.APP_ID;
                        applyPay.PAY_STATUS_MK = model.IsPay ? "Y" : "N";

                        if (model.PAY_METHOD == "D" || model.PAY_METHOD == "T" || model.PAY_METHOD == "B")
                        {
                            //當繳費項目是選匯票、劃撥或臨櫃時，交易金額等同繳費金額
                            applyPay.PAY_MONEY = model.PAY_MONEY;
                        }

                        applyPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.PAY_EXT_TIME_AC); //交易時間
                        applyPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.PAY_INC_TIME_AC); //異動時間
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = s_FUN_CD;
                        applyPay.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        //base.Update(applyPay, applyPayWhere);
                        base.Update2(applyPay, applyPayWhere, dict2, true);
                    }
                    #endregion

                    ApplyModel srcApplyWhere = new ApplyModel();
                    srcApplyWhere.APP_ID = model.APP_ID;
                    ApplyModel applydata = base.GetRow(srcApplyWhere);

                    string appDate = "";
                    string appDateY = "";
                    string appDateM = "";
                    string appDateD = "";
                    if (applydata != null)
                    {
                        appDate = HelperUtil.DateTimeToTwString(applydata.APP_TIME);

                        if (!string.IsNullOrEmpty(appDate))
                        {
                            appDateY = appDate.Substring(0, 3);
                            appDateM = appDate.Substring(4, 2);
                            appDateD = appDate.Substring(7, 2);
                        }
                    }

                    #region 寄信作業
                    string subject = "";
                    string MailBody = "";

                    //查詢網站網址
                    string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                    Uri uri = new Uri(webUrl);
                    string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
                    string appDocUrl = string.Format("{0}/Apply_{1}/AppDoc?APP_ID={2}", webDomain, s_SRV_ID, model.APP_ID);

                    // 判斷是否要寄信
                    switch (model.FLOW_CD)
                    {
                        case "--"://已接收，處理中
                            //subject = string.Format("{0}申請作業，案件編號﹕{1} 申請通知", s_SRV_NAME, model.APP_ID);

                            //MailBody = model.NAME + "，您好:<br/><br/>";
                            //MailBody += string.Format("您於民國{0}年{1}月{2}日申辦之{3}案件<br/>", appDateY, appDateM, appDateD, s_SRV_NAME);
                            //MailBody += "申請編號：<a href='https://e-service.mohw.gov.tw/History/Show/" + model.APP_ID + "'>" + model.APP_ID + "</a>現在進度為'已接收，處理中'<br/>";
                            //MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
                            //MailBody += "衛生福利部醫事司敬上<br/><br/>";
                            //MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
                            //MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                            //MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                            //MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                            //MailBody += "115204 臺北市南港區忠孝東路六段488號";

                            //MailBody = string.Format(@"{0}，您好:<br/><br/>
                            //            您於民國{1}年{2}月{3}日申辦之{4}案件<br/>
                            //            申請編號：<a href='{5}'>{6}</a>現在進度為'已接收，處理中'<br/>
                            //            特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>
                            //            衛生福利部醫事司敬上<br/><br/>
                            //            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>
                            //            ※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>
                            //            115209 臺北市南港區昆陽街161-2號<br><br>
                            //            ※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>
                            //            115204 臺北市南港區忠孝東路六段488號",
                            //            model.NAME, appDateY, appDateM, appDateD,s_SRV_NAME,
                            //            appDocUrl, model.APP_ID);

                            //SendMail(apply001008.EMAIL, subject, MailBody);
                            SendMail_InPorcess(model.NAME, s_SRV_NAME, s_SRV_ID, apply001008.EMAIL,
                            applydata.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");

                            break;
                        case "2"://通知補件
                            subject = string.Format("{0}申請作業，案件編號﹕{1} 補件通知", s_SRV_NAME, model.APP_ID);

                            //MailBody = applydata.NAME + "，您好:<br/><br/>";
                            //MailBody += string.Format("您於民國{0}年{1}月{2}日申辦之{3}案件<br/>", appDateY, appDateM, appDateD, s_SRV_NAME);
                            //MailBody += "申請編號：<a href='https://e-service.mohw.gov.tw//History/Show/" + model.APP_ID + "'>" + model.APP_ID + "</a>現在進度為'通知補件'<br/>";
                            //MailBody += "補件項目：" + mainproject + "<br/><br>";
                            //MailBody += "補件備註：" + model.Note.Replace("\\n", "<br/>").Replace(Environment.NewLine,"<br/>") + "<br/><br>";
                            //MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
                            //MailBody += "衛生福利部醫事司敬上<br/><br/>";
                            //MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
                            //MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                            //MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                            //MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                            //MailBody += "115204 臺北市南港區忠孝東路六段488號";

                            //MailBody = string.Format(@"{0}，您好:<br/><br/>
                            //            您於民國{1}年{2}月{3}日申辦之{4}案件<br/>
                            //            申請編號：<a href='{5}'>{6}</a>現在進度為'通知補件'<br/>
                            //            補件項目：{7}<br/><br>
                            //            補件備註：{8}<br/><br>
                            //            特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>
                            //            衛生福利部醫事司敬上<br/><br/>
                            //            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>
                            //            ※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>
                            //            115209 臺北市南港區昆陽街161-2號<br><br>
                            //            ※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>
                            //            115204 臺北市南港區忠孝東路六段488號",
                            //            applydata.NAME, appDateY, appDateM, appDateD, s_SRV_NAME,
                            //            appDocUrl, model.APP_ID, mainproject,
                            //            model.Note.Replace("\\n", "<br/>").Replace(Environment.NewLine, "<br/>"));


                            //SendMail(apply001008.EMAIL, subject, MailBody);

                            string note = "";
                            note = "補件項目﹕" + mainproject + "<br/>";
                            note += "補件備註﹕" + model.Note.TONotNullString().Replace("\\n", "<br/>").Replace(Environment.NewLine, "<br/>");

                            SendMail_Notice(applydata.NAME, s_SRV_NAME, s_SRV_ID, apply001008.EMAIL,
                            applydata.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, note);
                            break;
                        case "9": //逾期未補件而予結案
                                  //subject = string.Format("{0}申請作業，案件編號﹕{1} 申請通知", s_SRV_NAME, model.APP_ID);

                            //MailBody += applydata.NAME + "，您好:<br/><br/>";
                            //MailBody += string.Format("您於民國{0}年{1}月{2}日申辦之{3}案件<br/>", appDate.Substring(0, 3), appDate.Substring(4, 2), appDate.Substring(7, 2), s_SRV_NAME);
                            //MailBody += "申請編號：<a href='https://e-service.mohw.gov.tw//History/Show/" + model.APP_ID + "'>" + model.APP_ID + "</a>現在進度為'逾期未補件而予結案'<br/>";
                            //MailBody += "通知備註：" + model.Note.Replace("\\n", "<br>") + "<br/><br>";
                            //MailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
                            //MailBody += "衛生福利部醫事司敬上<br/><br/>";
                            //MailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
                            //MailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                            //MailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                            //MailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                            //MailBody += "115204 臺北市南港區忠孝東路六段488號";

                            //SendMail(apply001008.EMAIL, subject, MailBody);

                            SendMail_Expired(applydata.NAME, s_SRV_NAME, s_SRV_ID, apply001008.EMAIL,
                            applydata.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note.TONotNullString().Replace("\\n", "<br>"));
                            savestatus = false;
                            break;
                        case "12": //核可(發文歸檔)
                            //ApplyModel applyModel = new ApplyModel();
                            //applyModel.APP_ID = model.APP_ID;
                            //applyModel = this.GetRow(applyModel);
                            //Apply_001008Model apply001008Model = new Apply_001008Model();
                            //apply001008Model.APP_ID = model.APP_ID;
                            //apply001008Model = this.GetRow(apply001008Model);

                            SendMail_Archive(applydata.NAME, s_SRV_NAME, s_SRV_ID, apply001008.EMAIL,
                                            applydata.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                            (HelperUtil.TransToDateTime(model.MAIL_DATE_AD))?.ToString("yyyyMMdd"),
                                            model.MAIL_BARCODE);
                            savestatus = false;
                            break;
                        case "15": //核可(發文歸檔)
                            SendMail_Cancel(applydata.NAME, "醫事人員或公共衛生師請領英文證明書", "001008", apply001008.EMAIL,
                                  applydata.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                            savestatus = false;
                            break;
                    }

                    // 判斷是否要寄信
                    if (savestatus)
                    {
                        if (model.FLOW_CD == "2")
                        {
                            //SendMail_Notice(MainBody, model.NAME, count, model.EMAIL, model.APP_ID, s_SRV_NAME, s_SRV_ID, ProjectStr: ProjectStr);
                        }
                    }
                    #endregion

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    msg = "存檔失敗，請聯絡系統管理員。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return msg;
        }

        /// <summary>
        /// 查詢當次補件通知設定資訊
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public TblAPPLY_NOTICE GetApplyNotice_001008(string APP_ID)
        {
            TblAPPLY_NOTICE result = null;
            var dictionary = new Dictionary<string, object>
                    {
                        { "@APP_ID", APP_ID }
                    };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string sql = @"SELECT APP_ID,
                                    STUFF((
	                                    SELECT  '、'+FIELD 
	                                    FROM APPLY_NOTICE X
	                                    WHERE 1=1
	                                    AND A.APP_ID=X.APP_ID
	                                    AND X.ISADDYN='N'
	                                    FOR XML PATH('')
                                    ),1,1,'') Field
                                    ,(SELECT DISTINCT NOTE FROM APPLY_NOTICE X WHERE X.APP_ID=A.APP_ID AND X.ISADDYN='N') NOTE
                                    FROM APPLY A
                                    WHERE 1=1
                                    AND A.APP_ID=@APP_ID
                                    ";
                    result = conn.QueryFirst<TblAPPLY_NOTICE>(sql, parameters);
                }
                catch (Exception ex)
                {
                    logger.Warn("GetApplyNotice_001008 ex:" + ex.Message, ex);
                    result = null;
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// 套印申請書
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte[] PrintPdf001008(string id)
        {
            byte[] result = null;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                Form001008Action action = new Form001008Action(conn);
                Dictionary<string, object> data = action.GetData(id);

                data.Add("LIST_ME", action.GetListME(id));
                data.Add("LIST_PR", action.GetListPR(id));

                result = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }
            return result;
        }
        #endregion

        #region Apply_010002 低收入戶及中低收入戶之體外受精（俗稱試管嬰兒）補助方案

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_010002FormModel QueryApply_010002(Apply_010002FormModel parm)
        {
            Apply_010002FormModel result = new Apply_010002FormModel();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("SRV_ID", parm.SRV_ID);
            parameters.Add("APP_ID", parm.APP_ID);

            string _sql = @"
     select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO
    ,app.APP_TIME,app.APP_EXT_DATE
    ,app.NAME,app.SEX_CD
    /* ,app.IDN,app.BIRTHDAY */ 
    ,app.PAY_METHOD,app.PAY_A_FEE,app.FLOW_CD
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM

    ,a2.APPLY_DATE
    ,a2.ORG_NAME
    ,a2.APNAME,a2.IDN,a2.BIRTHDAY,a2.TEL,a2.MOBILE,a2.EMAIL
    ,a2.SPNAME,a2.SPIDN,a2.SPBIRTHDAY,a2.SPTEL,a2.SPMOBILE,a2.SPEMAIL
    ,a2.C_ZIPCODE,a2.C_ADDR,a2.H_ZIPCODE,a2.H_ADDR,a2.H_EQUAL
    ,a2.MYDATA_GET2,a2.MYDATA_GET1
    ,a2.MERGEYN
    ,a2.FILE_IDCNF,a2.FILE_IDCNB,a2.FILE_DISEASE,a2.FILE_LOWREC
    ,a2.MYDATA_RTN_CODE,a2.MYDATA_RTN_CODE_DESC,a2.MYDATA_TX_TIME
    ,a2.MYDATA_IDCN,a2.MYDATA_LOWREC,a2.MYDATA_TX_RESULT_MSG
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_010002 a2 on a2.APP_ID=app.APP_ID
    WHERE 1=1
    and app.SRV_ID = @SRV_ID
    and app.APP_ID = @APP_ID";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_010002FormModel>(_sql, parameters);
                conn.Close();
                conn.Dispose();
            }
            // 取案件進度
            if (result != null) { result.APP_STATUS = this.GetSchedule(parm.APP_ID, "10"); }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_010002FileModel GetFileList_010002(string APP_ID)
        {
            var result = new Apply_010002FileModel();
            //var dictionary = new Dictionary<string, object> { { "@APP_ID", APP_ID } };
            var parameters = new DynamicParameters(); //動態參數
            parameters.Add("APP_ID", APP_ID);

            string _sql = @"
                            select app.APP_ID
                            ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_IDCNF_TEXT
                            ,dbo.FN_FILE_TEXT(app.APP_ID ,'2') FILE_IDCNB_TEXT
                            ,dbo.FN_FILE_TEXT(app.APP_ID ,'3') FILE_DISEASE_TEXT
                            ,dbo.FN_FILE_TEXT(app.APP_ID ,'4') FILE_LOWREC_TEXT
                            FROM APPLY app 
                            where 1=1 
                            and app.app_id = @APP_ID";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_010002FileModel>(_sql, parameters);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply010002(Apply_010002ViewModel model)
        {
            string Msg = "";
            // 通知補件
            if (model.Form.FLOW_CD == "2")
            {
                if (model.Form.FileCheck.TONotNullString() == "")
                {
                    Msg = "請至少選擇一種補件項目 !";
                }

                if (model.Form.NOTE.TONotNullString() == "" && model.Form.FileCheck.TONotNullString() != "")
                {
                    Msg = "請填寫補件內容 !";
                }
            }
            // 確認補正完成
            if (model.Form.FLOW_CD == "4")
            {
                if (model.Form.FileCheck.TONotNullString() == "")
                {
                    Msg = "請至少選擇一種補件項目 !";
                }
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply010002(Apply_010002ViewModel model)
        {
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", "010002");
            dict2.Add("LastMODTIME", LastMODTIME);
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "低收入戶及中低收入戶之體外受精(俗稱試管嬰兒)補助方案";
            string s_SRV_ID = "010002";
            // 紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            // 紀錄補件欄位
            var count = 0;
            string Msg = "";
            string mainproject = "";
            string ProjectStr = "";
            string ProjectStr1 = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    // 通知補件 || 確認補正完成
                    if (model.Form.FLOW_CD == "2" || model.Form.FLOW_CD == "4")
                    {
                        // 取得補件紀錄
                        TblAPPLY_NOTICE anwhere = new TblAPPLY_NOTICE();
                        anwhere.APP_ID = model.Form.APP_ID;
                        var andata = GetRowList(anwhere);

                        // 只取回最後一次補件的次數
                        var newandaata = from a in andata
                                         orderby a.FREQUENCY descending
                                         select a;

                        // 已補件次數
                        int times = newandaata.ToCount() == 0 ? 0 : newandaata.FirstOrDefault().FREQUENCY.TOInt32();

                        if (!string.IsNullOrEmpty(model.Form.FileCheck))
                        {
                            var needchk = model.Form.FileCheck.ToSplit(',');
                            count = needchk.Count();
                            // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                            MainBody = "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">";

                            string[] s_Field = new string[] { "國民身分證影印本(正面)", "國民身分證影印本(反面)", "人工生殖機構開立之不孕症診斷證明書", "中低收入戶或低收入戶證明文件" };
                            foreach (var item in needchk)
                            {
                                anwhere = new TblAPPLY_NOTICE();
                                var Field_NAME = "";
                                var newitem = item.TOInt32();
                                switch (newitem)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                        Field_NAME = s_Field[newitem];
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = string.Format("FILE_{0}", newitem);
                                        break;

                                    case 4:
                                        Field_NAME = "其他";
                                        if (!mainproject.Equals("")) { mainproject += "、"; }
                                        mainproject += Field_NAME;
                                        anwhere.Field = "ALL_" + "4";
                                        break;

                                    case 5:
                                        Field_NAME = "郵局匯票500元１紙，戶名：衛生福利部";
                                        if (!ProjectStr1.Equals("")) { ProjectStr1 += "、"; }
                                        ProjectStr1 += Field_NAME;
                                        anwhere.Field = "OTHER_" + "5";
                                        break;
                                }

                                anwhere.ADD_TIME = DateTime.Now;
                                anwhere.APP_ID = model.Form.APP_ID;
                                anwhere.ISADDYN = "N";
                                anwhere.FREQUENCY = (times + 1);
                                anwhere.NOTE = model.Form.NOTE;
                                anwhere.Field_NAME = Field_NAME;

                                if (model.Form.FLOW_CD == "2")
                                {
                                    // 通知補件
                                    this.Insert(anwhere);
                                }

                                //count++;
                                savestatus = true;
                            }

                            ProjectStr = "";
                            //ProjectStr += "需重新上傳之文件為：" + mainproject + "<br />";
                            //ProjectStr += "需掛號郵遞紙本之文件為：" + ProjectStr1 + "<br />";
                            ProjectStr += ProjectStr1.TONotNullString() != "" ? mainproject + "、" + ProjectStr1 : mainproject;
                            MainBody += ProjectStr;
                            MainBody += "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";

                            MainBody += "<div class=\"form-group\">";
                            MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                            MainBody += "<div class=\"col-sm-10\">";
                            // 這邊放入檔案名稱
                            MainBody += "<p class=\"form-control-static\">" + model.Form.NOTE + "</p>";
                            MainBody += "</div>";
                            MainBody += "</div>";
                            //ProjectStr += $"補件內容：{model.Form.NOTE}<br />";
                        }
                    }

                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    newDataApply.InjectFrom(model.Form);

                    #region 繳費資訊
                    if (model.Form.IS_PAY_STATUS)
                    {
                        // apply
                        newDataApply.PAY_A_PAID = model.Form.PAY_A_FEE;
                        newDataApply.UPD_TIME = DateTime.Now;

                        // apply_pay
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = model.Form.APP_ID;
                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = model.Form.PAY_EXT_TIME;// HelperUtil.TransToDateTime(); //DateTime.Now;
                        newDataPay.UPD_TIME = DateTime.Now;
                        newDataPay.UPD_FUN_CD = "ADM-STORE";
                        newDataPay.UPD_ACC = sm.UserInfo.UserNo;
                        base.Update2(newDataPay, wherePay, dict2, true);
                    }
                    #endregion

                    // 更新案件狀態
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    ApplyModel srcApplyWhere = new ApplyModel();
                    srcApplyWhere.APP_ID = model.Form.APP_ID;
                    ApplyModel applydata = base.GetRow(srcApplyWhere);

                    string appDate = "";
                    string appDateY = "";
                    string appDateM = "";
                    string appDateD = "";
                    if (applydata != null)
                    {
                        appDate = HelperUtil.DateTimeToTwString(applydata.APP_TIME);

                        if (!string.IsNullOrEmpty(appDate))
                        {
                            appDateY = appDate.Substring(0, 3);
                            appDateM = appDate.Substring(4, 2);
                            appDateD = appDate.Substring(7, 2);
                        }
                    }

                    // 通知補件
                    if (model.Form.FLOW_CD == "2")
                    {
                        SendMail_Notice(MainBody, model.Form.NAME, count, model.Form.EMAIL, model.Form.APP_ID, s_SNV_NAME, s_SRV_ID, ProjectStr: ProjectStr, CustomMailBody: model.Form.NOTE);
                    }
                    // 已接收，處理中
                    if (model.Form.FLOW_CD == "5")
                    {
                        SendMail_InPorcess(model.Form.NAME, s_SNV_NAME, s_SRV_ID, model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);
                    }
                    // 逾期未補件而予結案
                    if (model.Form.FLOW_CD == "9")
                    {
                        SendMail_Expired(model.Form.NAME, s_SNV_NAME, s_SRV_ID, model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, model.Form.NOTE);
                    }
                    // 確認補正完成
                    if (model.Form.FLOW_CD == "4")
                    {
                        string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);
                        string s_MSG_T1a = "您所提交的{0}申請，已完成資料補件共 {1}件（包括 {2}）。將儘速辦理您的申請案件，謝謝。";
                        string s_MSG_T1 = string.Format(s_MSG_T1a, s_SNV_NAME, count.ToString(), mainproject);
                        string MailBody = "";
                        MailBody += "<table align=\"left\" style=\"width:90%;\">";
                        MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                        MailBody += string.Format(" <tr><td>　　{0}</td></tr>", s_MSG_T1);
                        MailBody += "<tr><td align=\"right\">衛生福利部</td></tr>";
                        MailBody += "</table>";
                        SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                    }
                    // 申請延遲補件
                    if (model.Form.FLOW_CD == "10")
                    {
                        string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);

                        string MailBody = "";
                        MailBody += "<table align=\"left\" style=\"width:90%;\">";
                        MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                        MailBody += string.Format(" <tr><td>您於民國{0}年{1}月{2}日申辦之低收入戶及中低收入戶之體外受精(俗稱試管嬰兒)補助方案</td></tr>", appDateY, appDateM, appDateD);
                        MailBody += string.Format(" <tr><th align=\"left\">申請編號：{0} 現在進度為'申請延遲補件'</th></tr>", model.Form.APP_ID);
                        MailBody += string.Format(" <tr><th align=\"left\">特此通知。感謝您使用衛生福利部線上申辦系統</th></tr>", model.Form.NAME);
                        MailBody += "<tr><td align=\"left\">衛生福利部國民健康署敬上</td></tr><br/><br/>";
                        MailBody += "<tr><td align=\"left\">PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr>";
                        MailBody += "</table>";
                        SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                    }
                    // 退件
                    if (model.Form.FLOW_CD == "8")
                    {
                        string subject = string.Format("{0}，案件編號﹕{1} 通知", s_SNV_NAME, model.Form.APP_ID);

                        string MailBody = "";
                        MailBody += "<table align=\"left\" style=\"width:90%;\">";
                        MailBody += string.Format(" <tr><th align=\"left\">{0}，您好:</th></tr>", model.Form.NAME);
                        MailBody += string.Format(" <tr><td>您申請低收入戶及中低收入戶之體外受精(俗稱試管嬰兒)補助方案，因資料不全且未於通知補件後60日內補正，故退回您的申請案件。</td></tr>", appDateY, appDateM, appDateD);
                        MailBody += string.Format(" <tr><th align=\"left\">若有疑義，請洽本部國民健康署02-25220888 分機642、645</th></tr>", model.Form.APP_ID);
                        MailBody += string.Format(" <tr><th align=\"left\"></th></tr>", model.Form.NAME);
                        MailBody += "<tr><td align=\"left\">衛生福利部國民健康署敬上</td></tr><br/><br/>";
                        MailBody += "<tr><td align=\"left\">PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。</td></tr>";
                        MailBody += "</table>";
                        SendMail(model.Form.EMAIL, subject, MailBody, s_SRV_ID);
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    logger.Error(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return Msg;
        }

        /// <summary>
        /// 查詢付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public DataTable QueryPayInfo(string app_id)
        {
            DataTable result = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                string _sql = @"
 select PAY_STATUS_MK 
 ,CONVERT(varchar,PAY_EXT_TIME,111) PAY_EXT_TIME
 ,CONVERT(varchar,PAY_ACT_TIME,111) PAY_ACT_TIME
 from APPLY_PAY
 where APP_ID=@APP_ID";
                SqlCommand cmd = new SqlCommand(_sql, conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@APP_ID", app_id);
                //SqlDataAdapter da = new SqlDataAdapter(cmd);
                result.Load(cmd.ExecuteReader());
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        /// <summary>
        /// 新增更新付款資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public string UpdatePayInfo_010002(string app_id, bool IS_PAY_STATUS, DateTime? PayDate, int Pay_A_Fee)
        {
            var result = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    ApplyModel newDataApply = new ApplyModel();
                    ApplyModel whereApply = new ApplyModel() { APP_ID = app_id };
                    if (IS_PAY_STATUS)
                    {
                        newDataApply.PAY_A_PAID = Pay_A_Fee;
                        newDataApply.UPD_TIME = DateTime.Now;
                        base.Update(newDataApply, whereApply);

                        APPLY_PAY newDataPay = new APPLY_PAY();
                        newDataPay.PAY_STATUS_MK = "Y";
                        newDataPay.PAY_EXT_TIME = PayDate;
                        newDataPay.UPD_TIME = DateTime.Now;
                        APPLY_PAY wherePay = new APPLY_PAY();
                        wherePay.APP_ID = app_id;

                        base.Update(newDataPay, wherePay);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    result = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        public Apply_010002FormModel GetApplyNotice_010002(string app_id)
        {
            Apply_010002FormModel result = new Apply_010002FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    string _sql = @"DECLARE @ColumnGroup NVARCHAR(MAX), @PivotSQL NVARCHAR(MAX) 

                                    SELECT  @ColumnGroup=COALESCE(@ColumnGroup + ',' ,'' ) + QUOTENAME(Field) 
                                    FROM (
	                                        select Field,NOTE
	                                        from APPLY_NOTICE 
	                                        where APP_ID='" + app_id + @"' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID='" + app_id + @"')
	                                     ) T
                                    GROUP BY QUOTENAME(Field) 

                                    select @ColumnGroup =N'
                                                            SELECT *
                                                            FROM (
	                                                                select isnull(BATCH_INDEX,1) grp,Field,NOTE
	                                                                from APPLY_NOTICE 
	                                                                where APP_ID=''" + app_id + @"'' and FREQUENCY = (select max(FREQUENCY) from APPLY_NOTICE where APP_ID=''" + app_id + @"'')
                                                                 ) t 
                                                            PIVOT (
	                                                                MAX(NOTE) 
	                                                                FOR Field IN (' + @ColumnGroup + N')
                                                                   ) p;'

                                                            EXEC sp_executesql  @ColumnGroup";
                    result = conn.QueryFirst<Apply_010002FormModel>(_sql);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        #endregion

        #region Apply_001010 
        /// <summary>
        /// 取得醫事人員或公共衛生師證書影本申請書"
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001010ViewModel QueryApply_001010(Apply_001010ViewModel parm)
        {
            Apply_001010ViewModel result = new Apply_001010ViewModel();
            result.Apply = new ApplyModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {

                    string _sql = @"SELECT APP_ID,ISSUE_DEPT,ISSUE_DATE,LIC_TYPE,LIC_CD,LIC_NUM,DEL_MK,DEL_TIME,DEL_FUN_CD,COPIES,TOTAL_MEM,
		                                    DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,EMAIL,DIVISION,MAIL_DATE,MAIL_BARCODE
                             FROM APPLY_001010
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001010ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                    var proAcc = string.Empty;
                    if (result.Apply != null && result.Apply.PRO_ACC != null)
                    {
                        proAcc = result.Apply.PRO_ACC.TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(proAcc))
                    {
                        // 查詢承辦人姓名 20210513 改寫
                        AdminModel where_admin = new AdminModel();
                        where_admin.ACC_NO = proAcc.TONotNullString();
                        result.Admin = GetRow(where_admin);
                        if (string.IsNullOrEmpty(result.Admin.NAME))
                        {
                            result.Admin.NAME = proAcc.TONotNullString();
                        }
                        //dictionary = new Dictionary<string, object> { { "@ACC_NO", proAcc } };
                        //parameters = new DynamicParameters(dictionary);

                        //_sql = @"SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, ADMIN_LEVEL, NAME, TEL, MAIL, AD_OU, SSO_KEY, IDN, LEVEL_UPD_TIME, DEL_MK, DEL_TIME, DEL_FUN_CD,
                        //        DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        //        FROM ADMIN
                        //    WHERE 1=1";
                        //_sql += " AND ACC_NO = @ACC_NO";
                        //result.Admin = conn.QueryFirst<AdminModel>(_sql, parameters);
                    }
                    else
                    {
                        result.Admin = new AdminModel();
                        // 分文處理檢視案件，無法取得承辦人。
                    }
                    dictionary = new Dictionary<string, object>
                    {
                         { "@APP_ID", parm.APP_ID }
                    };
                    parameters = new DynamicParameters(dictionary);
                    _sql = @" SELECT APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_ACT_TIME, PAY_EXT_TIME, PAY_EXT_TIME AS PAY_EXT_TIME_AC2, PAY_INC_TIME, PAY_METHOD, PAY_STATUS_MK, PAY_RET_CD,
                            PAY_RET_MSG, BATCH_NO, APPROVAL_CD, PAY_RET_NO, INVOICE_NO, PAY_DESC, CARD_NO, HOST_TIME, TRANS_RET, SESSION_KEY, AUTH_DATE,
                            AUTH_NO, SETTLE_DATE, OTHER, ROC_ID, CLIENT_IP, OID, SID, DEL_MK, DEL_TIME, DEL_FUN_CD, DEL_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            FROM APPLY_PAY
                            WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.ApplyPay = conn.QueryFirst<APPLY_PAY>(_sql, parameters);

                    _sql = @"SELECT APP_ID,FILE_NO,SUBSTRING(FILENAME,16,LEN(FILENAME)) FILENAME,SRC_FILENAME,DEL_MK,DEL_TIME,DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,SRC_NO,BATCH_INDEX
                              FROM APPLY_FILE
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.File = conn.Query<Apply_FileModel>(_sql, parameters).ToList<Apply_FileModel>().FirstOrDefault();

                    _sql = @"SELECT APP_ID, Field, ISADDYN, FREQUENCY, ADD_TIME, DEADLINE, NOTE, SRC_NO, BATCH_INDEX
                             FROM APPLY_NOTICE AS A
                             WHERE ISADDYN='Y' AND FREQUENCY = (SELECT MAX(FREQUENCY) FROM APPLY_NOTICE WHERE APP_ID=A.APP_ID) ";
                    _sql += " AND APP_ID = @APP_ID";

                    result.Notices = conn.Query<TblAPPLY_NOTICE>(_sql, parameters).ToList<TblAPPLY_NOTICE>();

                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "02");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();
                //申請日期
                result.APPLY_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_TIME);
                //預計完成日期
                result.APP_EXT_DATE = HelperUtil.DateTimeToTwString(result.Apply.APP_EXT_DATE);
                //生日
                result.BIRTHDAY_AC = HelperUtil.DateTimeToString(result.Apply.BIRTHDAY);
                //核發日期
                result.ISSUE_DATE_AC = HelperUtil.DateTimeToString(result.ISSUE_DATE);
                ////郵寄日期
                result.MAIL_DATE_AC = HelperUtil.DateTimeToString(result.MAIL_DATE);
                result.PAY_MONEY = result.ApplyPay.PAY_MONEY;
                result.Note = result.Apply.NOTICE_NOTE;

                // 電話
                if (result.Apply.TEL.TONotNullString() != "")
                {
                    string[] tel = result.Apply.TEL.Split('-');
                    if (result.Apply.TEL.TONotNullString().Trim() != "" && tel.ToCount() > 1)
                    {
                        result.TEL_SEC = tel[0];
                        if (tel.ToCount() > 1)
                        {
                            result.TEL_NO = tel[1];

                            if (result.TEL_NO.Contains("#"))
                            {
                                result.TEL_NO = result.TEL_NO.Split('#')[0];
                            }

                            if (result.Apply.TEL.IndexOf('#') > 0)
                            {
                                result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                            }
                        }
                    }
                }
                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }
                result.PAY_ACT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_ACT_TIME).TONotNullString();
                result.PAY_EXT_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_EXT_TIME).TONotNullString();
                result.PAY_INC_TIME_AC = HelperUtil.DateTimeToString(result.ApplyPay.PAY_INC_TIME).TONotNullString();

                if (result.ApplyPay.PAY_STATUS_MK == "Y")
                {
                    result.IsPay = true;
                }
                else
                {
                    result.IsPay = false;
                }

                switch (result.ApplyPay.PAY_METHOD)
                {
                    case "D":
                        result.PAY_METHOD_NAME = "匯票";
                        break;
                    case "B":
                        result.PAY_METHOD_NAME = "臨櫃";
                        break;
                    default:
                        result.PAY_METHOD_NAME = "";
                        break;
                }

                ShareDAO shareDAO = new ShareDAO();
                if (shareDAO.CalculationDocDate("001010", parm.APP_ID) && result.Apply.FLOW_CD == "2")
                {
                    result.IsNotice = "N";
                }
                else
                {
                    result.IsNotice = "Y";
                }
            }
            return result;
        }


        /// <summary>
        /// 取得醫事人員或公共衛生師證書影本申請書存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string SaveApply_001010(Apply_001010ViewModel model)
        {
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", "001010");
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string Msg = "";
            //紀錄欄位(信件用)
            string MainBody = "";
            // 記錄存檔狀況
            bool savestatus = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo?.Admin;
            Dictionary<string, string> noteContent = new Dictionary<string, string>();

            Apply_001010Model apply001010 = new Apply_001010Model();
            apply001010.APP_ID = model.APP_ID;

            if (model.Apply.FLOW_CD == "12")
            {
                apply001010 = this.GetRow(apply001010);
            }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    if (model.Apply.ISMODIFY.TONotNullString() != "")
                    {
                        #region 補件內容
                        // 傳回項目及內容(如果是多筆項目整段可用迴圈跑)
                        MainBody = "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">項目</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        if (model.Apply.ISMODIFY.TONotNullString() == "Y")
                        {
                            MainBody += "<p class=\"form-control-static\">" + "其他" + "</p>";
                        }
                        MainBody += "</div>";
                        MainBody += "</div>";
                        MainBody += "<div class=\"form-group\">";
                        MainBody += "<label class=\"step-label col-sm-2\" for=\"\">內容</label>";
                        MainBody += "<div class=\"col-sm-10\">";
                        // 這邊放入檔案名稱
                        MainBody += "<p class=\"form-control-static\">" + model.Note + "</p>";
                        MainBody += "</div>";
                        MainBody += "</div>";
                        savestatus = true;
                        #endregion
                    }
                    // 9:逾期未補件而予結案// 15:自請撤銷// 8:退件通知
                    if (model.Apply.FLOW_CD == "9" || model.Apply.FLOW_CD == "15" || model.Apply.FLOW_CD == "8")
                    {
                        noteContent.Add("備註", model.Note.TONotNullString());
                        MainBody = GetNote(noteContent);
                    }
                    // 12:核可(發文歸檔)
                    if (model.Apply.FLOW_CD == "12")
                    {
                        noteContent.Add("郵寄日期", model.MAIL_DATE_AC);
                        noteContent.Add("掛號條碼", model.MAIL_BARCODE);
                        MainBody = GetNote(noteContent);
                    }

                    #region 案件內容
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.APP_ID;
                    apply.FLOW_CD = model.Apply.FLOW_CD == "Z" ? "Y" : model.Apply.FLOW_CD;
                    apply.NOTICE_NOTE = model.Note.TONotNullString();

                    if (apply.FLOW_CD == "2")
                    {
                        apply.APPLY_NOTICE_DATE = DateTime.Now;
                        apply.ISMODIFY = model.Apply.ISMODIFY;

                        TblAPPLY_NOTICE applyNotice = null;
                        TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                        where.APP_ID = model.APP_ID;
                        var noticeList = GetRowList(where);

                        var newNoticeList = from notice in noticeList
                                            orderby notice.FREQUENCY descending
                                            select notice;

                        var times = newNoticeList.ToCount() == 0 ? 0 : newNoticeList.FirstOrDefault().FREQUENCY.TOInt32();
                        applyNotice = new TblAPPLY_NOTICE();
                        applyNotice.ADD_TIME = DateTime.Now;
                        applyNotice.APP_ID = model.APP_ID;
                        applyNotice.ISADDYN = "N";
                        applyNotice.Field = "FILE1";
                        applyNotice.FREQUENCY = times + 1;
                        applyNotice.NOTE = "001010補件";
                        applyNotice.Field_NAME = "FILE1";
                        Insert(applyNotice);
                    }
                    else
                    {
                        apply.ISMODIFY = "A";
                    }

                    if (model.IsPay)
                    {
                        apply.PAY_A_PAID = model.PAY_MONEY;
                    }
                    apply.MOHW_CASE_NO = model.Apply.MOHW_CASE_NO.TONotNullString();
                    apply.UPD_TIME = DateTime.Now;
                    apply.UPD_FUN_CD = "ADM-STORE";
                    apply.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                    apply.DEL_MK = "N";
                    apply.ISMODIFY = model.Apply.ISMODIFY.TONotNullString().Equals("Z") ? "Y" : model.Apply.ISMODIFY.TONotNullString();
                    apply.NOTIBODY = model.Apply.ISMODIFY.TONotNullString() != "" ? model.Note : "";
                    apply.MAILBODY = MainBody;
                    base.Update2(apply, whereApply, dict2, true);

                    if (model.Apply.FLOW_CD == "12")
                    {
                        Apply_001010Model whereApply001010 = new Apply_001010Model();
                        whereApply001010.APP_ID = model.APP_ID;
                        Apply_001010Model apply001010Model = new Apply_001010Model();
                        apply001010Model.MAIL_DATE = HelperUtil.TransToDateTime(model.MAIL_DATE_AC);
                        apply001010Model.MAIL_BARCODE = model.MAIL_BARCODE;
                        apply001010Model.UPD_TIME = DateTime.Now;
                        apply001010Model.UPD_FUN_CD = "ADM-STORE";
                        apply001010Model.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        apply001010Model.DEL_MK = "N";
                        base.Update2(apply001010Model, whereApply001010, dict2, true);
                    }
                    #endregion

                    #region 繳費資訊
                    if (model.IsPay)
                    {
                        APPLY_PAY applyPayWhere = new APPLY_PAY();
                        applyPayWhere.APP_ID = model.APP_ID;
                        applyPayWhere.PAY_STATUS_MK = "N";

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = model.APP_ID;
                        applyPay.PAY_STATUS_MK = model.IsPay ? "Y" : "N";
                        applyPay.PAY_MONEY = model.ApplyPay != null ? model.ApplyPay.PAY_MONEY : model.PAY_MONEY;
                        applyPay.PAY_EXT_TIME = HelperUtil.TransToDateTime(model.PAY_EXT_TIME_AC);
                        applyPay.PAY_INC_TIME = HelperUtil.TransToDateTime(model.PAY_INC_TIME_AC);
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = "ADM-STORE";
                        applyPay.UPD_ACC = UserInfo == null ? "Admin" : UserInfo.ACC_NO.TONotNullString();
                        base.Update2(applyPay, applyPayWhere, dict2, true);
                    }
                    #endregion

                    // 判斷是否要寄信
                    #region 依據狀態寄發信件內容
                    if (model.Apply.FLOW_CD == "--")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001010Model apply001010Model = new Apply_001010Model();
                        apply001010Model.APP_ID = model.APP_ID;
                        apply001010Model = this.GetRow(apply001010Model);
                        SendMail_InPorcess(applyModel.NAME, "醫事人員或公共衛生師證書影本申請書", "001010", apply001010Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "2")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001010Model apply001010Model = new Apply_001010Model();
                        apply001010Model.APP_ID = model.APP_ID;
                        apply001010Model = this.GetRow(apply001010Model);
                        string note = "";

                        if (model.Apply.ISMODIFY == "Y")
                        {
                            note = "補件項目﹕其他<br/>";
                        }
                        note += "補件備註﹕" + model.Note;

                        SendMail_Notice(applyModel.NAME, "醫事人員或公共衛生師證書影本申請書", "001010", apply001010Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, note);
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "12")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001010Model apply001010Model = new Apply_001010Model();
                        apply001010Model.APP_ID = model.APP_ID;
                        apply001010Model = this.GetRow(apply001010Model);

                        SendMail_Archive(applyModel.NAME, "醫事人員或公共衛生師證書影本申請書", "001010", apply001010Model.EMAIL,
                                        applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID,
                                        (HelperUtil.TransToDateTime(model.MAIL_DATE_AC))?.ToString("yyyyMMdd"),
                                        model.MAIL_BARCODE);
                        savestatus = false;
                    }

                    if (model.Apply.FLOW_CD == "15")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001010Model apply001010Model = new Apply_001010Model();
                        apply001010Model.APP_ID = model.APP_ID;
                        apply001010Model = this.GetRow(apply001010Model);

                        SendMail_Cancel(applyModel.NAME, "醫事人員或公共衛生師證書影本申請書", "001010", apply001010Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, "");
                    }

                    if (model.Apply.FLOW_CD == "9")
                    {
                        ApplyModel applyModel = new ApplyModel();
                        applyModel.APP_ID = model.APP_ID;
                        applyModel = this.GetRow(applyModel);
                        Apply_001010Model apply001010Model = new Apply_001010Model();
                        apply001010Model.APP_ID = model.APP_ID;
                        apply001010Model = this.GetRow(apply001010Model);

                        SendMail_Expired(applyModel.NAME, "醫事人員或公共衛生師證書影本申請書", "001010", apply001010Model.EMAIL,
                            applyModel.APP_TIME?.ToString("yyyyMMdd"), model.APP_ID, model.Note);
                        savestatus = false;

                    }
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        /// <summary>
        /// 套印\醫事人員或公共衛生師證書影本申請書"
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_001010ViewModel PrintApply_001010(Apply_001010ViewModel parm)
        {
            Apply_001010ViewModel result = new Apply_001010ViewModel();
            result.Apply = new ApplyModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", parm.APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {

                    string _sql = @"SELECT APP_ID,ISSUE_DEPT,ISSUE_DATE,LIC_TYPE,LIC_CD,LIC_NUM,DEL_MK,DEL_TIME,DEL_FUN_CD,COPIES,TOTAL_MEM,
		                                    DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,EMAIL,DIVISION,MAIL_DATE,MAIL_BARCODE
                             FROM APPLY_001010
                             WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result = conn.QueryFirst<Apply_001010ViewModel>(_sql, parameters);

                    _sql = @"SELECT APP_ID,SRV_ID,SRC_SRV_ID,UNIT_CD,ACC_NO,IDN,SEX_CD,BIRTHDAY,NAME,ENAME,CNT_NAME,CNT_ENAME,CHR_NAME,CHR_ENAME,TEL,FAX,CNT_TEL,
	                              ADDR_CODE,ADDR,EADDR,CARD_IDN,APP_TIME,PAY_POINT,PAY_METHOD,PAY_BACK_MK,PAY_BACK_DATE,PAY_A_FEE,PAY_A_FEEBK,PAY_A_PAID,PAY_C_FEE,
	                              PAY_C_FEEBK,PAY_C_PAID,CHK_MK,ATM_VNO,API_MK,PRINT_MK,TRANS_ID,MOHW_CASE_NO,FLOW_CD,TO_MIS_MK,TO_ARCHIVE_MK,APP_STR_DATE,APP_EXT_DATE,
	                              APP_ACT_DATE,APP_DEFER_MK,APP_DEFER_TIME_S,APP_DEFER_TIME_E,APP_DEFER_DAYS,APP_DEFER_TIMES,APP_DISP_ACC,APP_DISP_MK,PRO_ACC,PRO_UNIT_CD,
	                              CLOSE_MK,APP_GRADE,APP_GRADE_TIME,APP_GRADE_LOG,NOTIFY_COUNT,NOTIFY_TYPE,CASE_BACK_MK,CASE_BACK_TIME,DIGITAL,LOGIN_TYPE,DEL_MK,DEL_TIME,
	                              DEL_FUN_CD,DEL_ACC,UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME,ADD_FUN_CD,ADD_ACC,MARITAL_CD,CERT_SN,MOBILE,ISMODIFY,NOTICE_NOTE
                              FROM APPLY
                              WHERE 1 = 1";
                    _sql += " AND APP_ID = @APP_ID";
                    result.Apply = conn.QueryFirst<ApplyModel>(_sql, parameters);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (result != null)
            {
                BackApplyDAO dao = new BackApplyDAO();

                // 電話
                if (result.Apply.TEL.TONotNullString() != "")
                {
                    string[] tel = result.Apply.TEL.Split('-');
                    if (result.Apply.TEL.TONotNullString().Trim() != "" && tel.ToCount() > 1)
                    {
                        result.TEL_SEC = tel[0];
                        if (tel.ToCount() > 1)
                        {
                            result.TEL_NO = tel[1];

                            if (result.TEL_NO.Contains("#"))
                            {
                                result.TEL_NO = result.TEL_NO.Split('#')[0];
                            }

                            if (result.Apply.TEL.IndexOf('#') > 0)
                            {
                                result.TEL_EXT = result.Apply.TEL.Split('#')[1];
                            }
                        }
                    }
                }
                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = result.Apply.ADDR_CODE;
                var address = dao.GetRow(zip);
                result.CITY_CODE = result.Apply.ADDR_CODE;
                if (address != null)
                {
                    result.CITY_TEXT = address.TOWNNM;
                    result.CITY_DETAIL = result.Apply.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }
            }
            return result;
        }
        #endregion

        #region Apply040001 衛生福利部訴願案件

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_040001FormModel QueryApply_040001(Apply_040001FormModel parm)
        {
            Apply_040001FormModel result = new Apply_040001FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
     select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO,convert(varchar,app.APP_TIME,111) APPLY_DATE
    ,app.APP_TIME,app.APP_EXT_DATE,app.FLOW_CD,app.NOTICE_NOTE NOTE
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM
    ,A2.EMAIL,app.NAME,convert(varchar,app.BIRTHDAY,111) BIRTHDAY
	,app.IDN,app.CNT_TEL,app.MOBILE,app.ADDR_CODE,app.ADDR
	,A2.CHR_NAME,convert(varchar,A2.CHR_BIRTH,111) CHR_BIRTH
	,A2.CHR_IDN,A2.CHR_TEL,A2.CHR_MOBILE,A2.CHR_ADDR_CODE,A2.CHR_ADDR
	,A2.R_NAME,convert(varchar,A2.R_BIRTH,111) R_BIRTH
	,A2.R_IDN,A2.R_TEL,A2.R_MOBILE,A2.R_ADDR_CODE,A2.R_ADDR
	,A2.ORG_NAME,convert(varchar,A2.ORG_DATE,111) ORG_DATE
	,A2.ORG_MEMO,A2.ORG_FACT
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_040001 A2 on A2.APP_ID=app.APP_ID
    WHERE 1=1 ";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_040001FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "13");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_040001FileModel GetFileList_040001(string APP_ID)
        {
            var result = new Apply_040001FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);
            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_1_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            string _filelist = @" select * from apply_file where 1=1 and app_id=@APP_ID ";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_040001FileModel>(_sql, parameters);
                result.FILE = conn.Query<Apply_040001FILEModel>(_filelist, parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply040001(Apply_040001ViewModel model)
        {
            string Msg = "";
            if (model == null || model.Form == null)
            {
                Msg = "存檔失敗，請聯絡系統管理員 !\n";
                return Msg;
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
                return Msg;
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply040001(Apply_040001ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "衛生福利部訴願案件";
            string s_SRV_ID = "040001";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
                    string Msg = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    //FLOW_CD: 0:完成申請,1:新收案件
                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    //newDataApply.InjectFrom(model.Form);
                    newDataApply.APP_ID = model.Form.APP_ID;
                    // 更新案件狀態
                    newDataApply.NOTICE_NOTE= model.Form.NOTE;
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    sm.LastMODTIME = LastMODTIME;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    model.Form.FLOW_CD = model.Form.FLOW_CD ?? "";
                    //FLOW_CD: 0::完成申請
                    if (model.Form.FLOW_CD.Equals("0"))
                    {
                        SendMail_Success(model.Form.NAME, s_SNV_NAME, "040001", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        #endregion

        #region Apply041001 全民健康保險爭議審議申請書

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_041001FormModel QueryApply_041001(Apply_041001FormModel parm)
        {
            Apply_041001FormModel result = new Apply_041001FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
     select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO,convert(varchar,app.APP_TIME,111) APPLY_DATE
    ,app.APP_TIME,app.APP_EXT_DATE,app.FLOW_CD,app.NOTICE_NOTE NOTE
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM
    ,A2.EMAIL,app.NAME,app.IDN,app.CNT_TEL,app.MOBILE,app.ADDR_CODE,app.ADDR
	,A2.R_NAME,A2.R_ADDR_CODE,A2.R_ADDR,A2.R_TEL,A2.R_MOBILE,A2.R_IDN
	,A2.KIND1,A2.KIND2,A2.KIND3,Convert(varchar,A2.LIC_DATE,111) LIC_DATE
	,A2.LIC_CD,A2.LIC_NUM,Convert(varchar,A2.KNOW_DATE,111) KNOW_DATE
	,A2.KNOW_MEMO,A2.KNOW_FACT
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_041001 A2 on A2.APP_ID=app.APP_ID
    WHERE 1=1 ";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_041001FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "14");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_041001FileModel GetFileList_041001(string APP_ID)
        {
            var result = new Apply_041001FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);
            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_1_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'2') FILE_2_TEXT 
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'3') FILE_3_TEXT
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'4') FILE_4_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            string _filelist = @" select * from apply_file where 1=1 and app_id=@APP_ID ";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_041001FileModel>(_sql, parameters);
                result.FILE = conn.Query<Apply_041001FILEModel>(_filelist, parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply041001(Apply_041001ViewModel model)
        {
            string Msg = "";
            if (model == null || model.Form == null)
            {
                Msg = "存檔失敗，請聯絡系統管理員 !\n";
                return Msg;
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
                return Msg;
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply041001(Apply_041001ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "全民健康保險爭議案件(權益案件及特約管理案件)線上申辦";
            string s_SRV_ID = "041001";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            string Msg = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    //FLOW_CD: 0:完成申請,1:新收案件
                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    //newDataApply.InjectFrom(model.Form);
                    newDataApply.APP_ID = model.Form.APP_ID;
                    // 更新案件狀態
                    newDataApply.NOTICE_NOTE = model.Form.NOTE;
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    sm.LastMODTIME = LastMODTIME;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    model.Form.FLOW_CD = model.Form.FLOW_CD ?? "";
                    ////FLOW_CD: 0::完成申請
                    //if (model.Form.FLOW_CD.Equals("0"))
                    //{
                    //    SendMail_Success(model.Form.NAME, s_SNV_NAME, "041001", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    //}

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        #endregion

        #region Apply006001 國民年金爭議審議線上申辦

        /// <summary>
        /// 取得案件詳細資料
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_006001FormModel QueryApply_006001(Apply_006001FormModel parm)
        {
            Apply_006001FormModel result = new Apply_006001FormModel();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                string _sql = @"
    select app.SRV_ID 
    ,app.APP_ID,app.ACC_NO,convert(varchar,app.APP_TIME,111) APPLY_DATE
    ,app.APP_TIME,app.APP_EXT_DATE,app.FLOW_CD,app.NOTICE_NOTE NOTE
    ,ISNULL(ad.NAME,app.PRO_ACC) as PRO_NAM
    ,A2.EMAIL,app.NAME,convert(varchar,app.BIRTHDAY,111) BIRTHDAY
	,app.IDN,app.CNT_TEL,app.MOBILE,app.ADDR_CODE,app.ADDR
	,A2.ISSAME,A2.R_NAME,convert(varchar,A2.R_BIRTH,111) R_BIRTH
	,A2.R_IDN,A2.R_ADDR_CODE,A2.R_ADDR,A2.R_MOBILE,A2.R_TEL,A2.KINDTYPE
	,convert(varchar,A2.LIC_DATE,111) LIC_DATE,A2.LIC_CD,A2.LIC_NUM
	,A2.PAY_YEAR,A2.PAY_MONTH,A2.PAY_NUM,convert(varchar,A2.KNOW_DATE,111) KNOW_DATE
	,A2.KNOW_MEMO,A2.KNOW_FACT
    FROM APPLY app
    LEFT JOIN ADMIN ad on ad.ACC_NO=app.PRO_ACC
    LEFT JOIN APPLY_006001 A2 on A2.APP_ID=app.APP_ID
    WHERE 1=1 ";
                _sql += "and app.app_id = @APP_ID";

                try
                {
                    result = conn.QueryFirst<Apply_006001FormModel>(_sql, new { APP_ID = parm.APP_ID });
                    // 取案件進度
                    result.APP_STATUS = this.GetSchedule(parm.APP_ID, "15");

                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 取檔案
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public Apply_006001FileModel GetFileList_006001(string APP_ID)
        {
            var result = new Apply_006001FileModel();

            var dictionary = new Dictionary<string, object>
            {
                { "@APP_ID", APP_ID }
            };
            var parameters = new DynamicParameters(dictionary);
            string _sql = @"
    select app.APP_ID
    ,dbo.FN_FILE_TEXT(app.APP_ID ,'1') FILE_1_TEXT
    FROM APPLY app 
    where 1=1 
    and app.APP_ID =@APP_ID";

            string _filelist = @" select * from apply_file where 1=1 and app_id=@APP_ID ";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.QueryFirst<Apply_006001FileModel>(_sql, parameters);
                result.FILE = conn.Query<Apply_006001FILEModel>(_filelist, parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 存檔用檢核邏輯
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckApply006001(Apply_006001ViewModel model)
        {
            string Msg = "";
            if (model == null || model.Form == null)
            {
                Msg = "存檔失敗，請聯絡系統管理員 !\n";
                return Msg;
            }
            if (string.IsNullOrEmpty(model.Form.APP_ID))
            {
                // 防止hidden沒有藏到案件編號導致大量更新
                Msg = "存檔失敗，請聯絡系統管理員 !";
                return Msg;
            }
            return Msg;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string AppendApply006001(Apply_006001ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var adminInfo = sm.UserInfo.Admin; //var UserInfo = sm.UserInfo?.Admin;
            string s_SNV_NAME = "國民年金爭議審議線上申辦";
            string s_SRV_ID = "006001";
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.Form.APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            string Msg = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    //FLOW_CD: 0:完成申請,1:新收案件
                    // 更新案件狀態
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.Form.APP_ID;
                    ApplyModel newDataApply = new ApplyModel();
                    //newDataApply.InjectFrom(model.Form);
                    newDataApply.APP_ID = model.Form.APP_ID;
                    // 更新案件狀態
                    newDataApply.NOTICE_NOTE = model.Form.NOTE;
                    newDataApply.FLOW_CD = model.Form.FLOW_CD;
                    newDataApply.UPD_TIME = DateTime.Now;
                    newDataApply.UPD_FUN_CD = "ADM-STORE";
                    newDataApply.UPD_ACC = sm.UserInfo.UserNo;
                    sm.LastMODTIME = LastMODTIME;
                    base.Update2(newDataApply, whereApply, dict2, true);

                    model.Form.FLOW_CD = model.Form.FLOW_CD ?? "";
                    //FLOW_CD: 0::完成申請
                    if (model.Form.FLOW_CD.Equals("0"))
                    {
                        SendMail_Success(model.Form.NAME, s_SNV_NAME, "006001", model.Form.EMAIL, model.Form.APPLY_DATE?.ToString("yyyyMMdd"), model.Form.APP_ID, "");
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = "存檔失敗，請聯絡系統管理員 。";
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        #endregion
    }
}
