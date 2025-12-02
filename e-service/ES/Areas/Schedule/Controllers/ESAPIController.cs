using ES.Areas.Admin.Controllers;
using ES.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ESAPI;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Collections;
using System.Net.Mail;
using ES.Models;
using ES.Services;
using ES.Controllers;

namespace ES.Areas.Admin.Controllers
{
    public class ESAPIController : BaseNoMemberController
    {
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleMdodLogger");
        private static bool isLock;
        static string s_log1 = "";
        static int iTimeout1 = (600 * 1000);
        static string MainMail = string.Empty;
        static ESAPIController()
        {
            ESAPIController.logger = LogUtils.GetLogger("ScheduleMdodLogger");
            ESAPIController.isLock = false;
        }

        public ESAPIController()
        {

        }

        public ActionResult ScheService()
        {
            ActionResult actionResult;
            if (!ESAPIController.isLock)
            {
                try
                {
                    ESAPIController.isLock = true;
                    ESAPIController.logger.Debug("我的E政府上稿排程開始");
                    MainMail = string.Empty;

                    #region 新增
                    s_log1 = "新增資料...." + DateTime.Now;
                    MainMail += s_log1 + "<br />";
                    ESAPIController.logger.Debug(s_log1);
                    try
                    {
                        var iList = PostService();
                        if (iList == null)
                        {
                            s_log1 = "(資料不存在)" + DateTime.Now;
                            MainMail += s_log1 + "<br />";
                            ESAPIController.logger.Debug(s_log1);
                        }
                        if (iList != null)
                        {
                            s_log1 = string.Format("資料筆數：{0}..", iList.Count) + DateTime.Now;
                            MainMail += s_log1 + "<br />";
                            ESAPIController.logger.Debug(s_log1);
                            foreach (var item in iList)
                            {
                                MainMail += $"{item.Key}:{item.Value}<br />";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MainMail += comUtil.ChgHtml(ex.Message + "<br />" + ex.ToString() + "<br />");
                        ESAPIController.logger.Debug(ex.Message);
                        ESAPIController.logger.Debug(ex.StackTrace);
                        ESAPIController.logger.Debug(ex.ToString());
                    }
                    #endregion

                    #region 更新
                    s_log1 = "更新資料...." + DateTime.Now;
                    MainMail += s_log1 + "<br />";
                    ESAPIController.logger.Debug(s_log1);
                    try
                    {
                        var iList = PutService();
                        if (iList == null)
                        {
                            s_log1 = "(資料不存在)" + DateTime.Now;
                            MainMail += s_log1 + "<br />";
                            ESAPIController.logger.Debug(s_log1);
                        }
                        if (iList != null)
                        {
                            s_log1 = string.Format("資料筆數：{0}..", iList.Count) + DateTime.Now;
                            MainMail += s_log1 + "<br />";
                            ESAPIController.logger.Debug(s_log1);
                            foreach (var item in iList)
                            {
                                MainMail += $"{item.Key}:{item.Value}<br />";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MainMail += comUtil.ChgHtml(ex.Message + "<br />" + ex.ToString() + "<br />");
                        ESAPIController.logger.Debug(ex.Message);
                        ESAPIController.logger.Debug(ex.StackTrace);
                        ESAPIController.logger.Debug(ex.ToString());
                    }
                    #endregion

                    #region 刪除(下架)
                    s_log1 = "刪除(下架)資料...." + DateTime.Now;
                    MainMail += s_log1 + "<br />";
                    ESAPIController.logger.Debug(s_log1);
                    try
                    {
                        DeleteService();
                    }
                    catch (Exception ex)
                    {
                        MainMail += comUtil.ChgHtml(ex.Message + "<br />" + ex.ToString() + "<br />");
                        ESAPIController.logger.Debug(ex.Message);
                        ESAPIController.logger.Debug(ex.StackTrace);
                        ESAPIController.logger.Debug(ex.ToString());
                    }
                    #endregion

                    #region 寄信
                    s_log1 = "執行結束:" + DateTime.Now;
                    MainMail += s_log1 + "<br />";
                    ESAPIController.logger.Debug(s_log1);

                    LogSend(MainMail);

                    ESAPIController.logger.Debug("操作紀錄信件已寄出:" + DateTime.Now + "..");
                    ESAPIController.logger.Debug("我的E政府上稿排程結束");
                    ESAPIController.isLock = false;
                    #endregion
                    ((dynamic)base.ViewBag).Message = "上稿執行完成。";
                }
                catch (Exception e)
                {
                    ((dynamic)base.ViewBag).Message = string.Concat("執行失敗，錯誤訊息：", e);
                }
            }
            else
            {
                ESAPIController.logger.Debug("程式重複執行");
            }
            actionResult = base.View("Message");
            return actionResult;
        }

        /// <summary>
        /// 抓取API POST 介接資料
        /// </summary>
        static Dictionary<string, object> PostService()
        {
            try
            {
                string APIURL = comUtil.APIURL;
                List<APIResult> contributors = new List<APIResult>();
                // 異動案件，紀錄結果
                Dictionary<string, object> dict = new Dictionary<string, object>();
                var newList = new List<serviceJson2>();

                // 取得當日新增資料
                #region 查詢
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn); //conn.Open();
                    //2.16.886.101.20003.20065.20005 衛生福利部oid
                    string qry = @"
                   SELECT CASE
                             WHEN unit.unit_name LIKE '福利部' THEN '2.16.886.101.20003.20065'
                             WHEN unit.unit_name LIKE '社會救助及社工司' THEN '2.16.886.101.20003.20065.20004'
                             WHEN unit.unit_name LIKE '醫事司' THEN '2.16.886.101.20003.20065.20005'
                             WHEN unit.unit_name LIKE '中醫藥司' THEN '2.16.886.101.20003.20065.20006'
                             WHEN unit.unit_name LIKE '社會及家庭署' THEN '2.16.886.101.20003.20065.20025'
                             WHEN unit.unit_name LIKE '國民健康署' THEN '2.16.886.101.20003.20065.20026'
                             WHEN unit.unit_name LIKE '食品藥物管理署' THEN '2.16.886.101.20003.20065.20065'
                             WHEN unit.unit_name LIKE '護理及健康照護司' THEN '2.16.886.101.20003.20065.20066'
                             WHEN unit.unit_name LIKE '秘書處' THEN '2.16.886.101.20003.20065.20010'
                             WHEN unit.unit_name LIKE '國民年金監理會' THEN '2.16.886.101.20003.20065.20014'
                             ELSE ''
                           END                                   AS OID,
                           Concat('327220000I', 'SRV', S.srv_id) AS IDENTIFIER,
                           Isnull(S.cls_sub_cd, '')              AS CATEGORYTHEME,
                           Isnull(S.cls_adm_cd, '')              AS CATEGORYCAKE,
                           Isnull(S.cls_srv_cd, '')              AS CATEGORYSERVICE,
                           Isnull(S.keyword, '')                 AS KEYWORDS,
                           Isnull(S.NAME, '')                    AS TITLE,
                           Isnull(S.srv_desc, S.NAME)            AS DESCRIPTION,
                           Isnull(N.content_1, '')               AS 'PROCEDURE',
                           Isnull(N.content_4, '')               AS CONTACTPERSON,
                           Isnull(N.content_2, '')               AS DOCUMENTARY,
                           Isnull(S.pro_deadline, '')            AS WORKINGDAYS,
                           Isnull(N.content_3, '')               AS REMARKS,
                           CASE
                             WHEN(SELECT srv_id
                                  FROM   service_notice
                                  WHERE  service_notice.srv_id = S.srv_id
                                  GROUP  BY srv_id) IS NOT NULL THEN Concat(
                             N'https://eservice.mohw.gov.tw/Service/Notice/', S.srv_id)
                             ELSE ''
                           END                                   AS ONLINELINK,
                           CASE
                             WHEN(SELECT srv_id
                                  FROM   service_file
                                  WHERE  service_file.srv_id = S.srv_id
                                  GROUP  BY srv_id) IS NOT NULL THEN Concat(N'https://eservice.mohw.gov.tw/Service/File/', S.srv_id)
                             ELSE ''
                           END                                   AS DOCUMENTLINK,
                           Concat(N'https://eservices.taichung.gov.tw/AdvSearch/FormDownload/160/Download/', S.srv_id)                           AS DOCUMENTLINK2,
                           '#同臨櫃#'                         AS DOCUMENTLINK3,
                           ''                                    AS STATUSREASON,
                           'eservice@turbotech.com.tw;CCBOBO1@mohw.gov.tw;t92267@gmail.com'           AS NOTIFICATIONEMAILS
                    FROM   service S
                           LEFT JOIN service_notice N
                                  ON N.srv_id = S.srv_id
                           LEFT JOIN (SELECT ut.unit_name,
                                             ut.unit_cd
                                      FROM   (SELECT CASE
                                                       WHEN s2ut.unit_level = 1 THEN unit_name
                                                       ELSE (SELECT s3ut.unit_name
                                                             FROM   unit s3ut
                                                             WHERE  s3ut.unit_cd = s2ut.unit_pcd)
                                                     END AS UNIT_NAME,
                                                     s2ut.unit_cd
                                              FROM   unit s2ut) ut) unit
                                  ON unit.unit_cd = S.fix_unit_cd 
                ";
                    // 當日新增(異動)
                    qry += @" WHERE CAST(S.ADD_TIME AS DATE) = CAST(GETDATE() AS DATE)";

                    var cmd = new SqlCommand(qry, conn);
                    cmd.CommandType = CommandType.Text;                    

                    using (SqlDataReader objReader = cmd.ExecuteReader())
                    {
                        if (objReader.HasRows)
                        {
                            while (objReader.Read())
                            {
                                var model = new serviceJson2();
                                model.oid = objReader.GetString(objReader.GetOrdinal("OID"));
                                model.identifier = objReader.GetString(objReader.GetOrdinal("IDENTIFIER"));
                                model.categorytheme = objReader.GetString(objReader.GetOrdinal("CATEGORYTHEME"));
                                model.categorycake = objReader.GetString(objReader.GetOrdinal("CATEGORYCAKE"));
                                model.categoryservice = objReader.GetString(objReader.GetOrdinal("CATEGORYSERVICE"));
                                model.keywords = objReader.GetString(objReader.GetOrdinal("KEYWORDS"));
                                model.notificationemails = objReader.GetString(objReader.GetOrdinal("NOTIFICATIONEMAILS"));
                                model.title = objReader.GetString(objReader.GetOrdinal("TITLE"));
                                model.statusreason = objReader.GetString(objReader.GetOrdinal("STATUSREASON"));
                                model.replacecontent = string.Empty;
                                switch (objReader.GetString(objReader.GetOrdinal("IDENTIFIER")))
                                {
                                    case "327220000ISRV005014":
                                        model.function = "1,3";
                                        break;
                                    case "327220000ISRV005013":
                                        model.function = "1,3";
                                        break;
                                    case "327220000ISRV010002":
                                        model.function = "2";
                                        break;
                                    case "327220000ISRV012001":
                                        model.function = "1,2,3";
                                        break;
                                    case "327220000ISRV001039":
                                        model.function = "1,2,3";
                                        break;
                                    default:
                                        model.function = "1";
                                        break;
                                }
                                model.servicecontent1 = objReader.GetString(objReader.GetOrdinal("DESCRIPTION"));
                                model.criteria1 = string.Empty;
                                model.procedure1 = objReader.GetString(objReader.GetOrdinal("PROCEDURE"));
                                model.documentary1 = objReader.GetString(objReader.GetOrdinal("DOCUMENTARY"));
                                model.mydataresourceid1 = false;
                                model.workingdays1 = objReader.GetInt32(objReader.GetOrdinal("WORKINGDAYS")).ToString();
                                model.contactperson1 = objReader.GetString(objReader.GetOrdinal("CONTACTPERSON"));
                                model.remarks1 = objReader.GetString(objReader.GetOrdinal("REMARKS"));
                                model.reference1 = objReader.GetString(objReader.GetOrdinal("DOCUMENTLINK"));
                                model.onlinelink = objReader.GetString(objReader.GetOrdinal("ONLINELINK"));
                                var onlineWay = "#同線上申辦#";
                                model.servicecontent2 = onlineWay;
                                model.criteria2 = onlineWay;
                                model.procedure2 = onlineWay;
                                model.documentary2 = onlineWay;
                                model.mydataresourceid2 = false;
                                model.workingdays2 = onlineWay;
                                model.contactperson2 = onlineWay;
                                model.remarks2 = onlineWay;
                                model.reference2 = onlineWay;
                                model.documentlink2 = model.function.Contains("2") ? objReader.GetString(objReader.GetOrdinal("DOCUMENTLINK2")): null;
                                model.servicecontent3 = onlineWay;
                                model.criteria3 = onlineWay;
                                model.procedure3 = onlineWay;
                                model.documentary3 = onlineWay;
                                model.workingdays3 = onlineWay;
                                model.contactperson3 = onlineWay;
                                model.remarks3 = onlineWay;
                                model.reference3 = onlineWay;
                                model.documentlink3 = model.function.Contains("3") ? objReader.GetString(objReader.GetOrdinal("DOCUMENTLINK")): null;
                                //I would also check for DB.Null here before reading the value.
                                newList.Add(model);
                                dict.Add(model.identifier, "");
                            }
                        }
                        objReader.Close();
                    }
                    DataUtils.CloseDbConn(conn);
                }
                #endregion 查詢

                if (newList != null && newList.Count > 0)
                {
                    s_log1 = string.Format("newList筆數：{0}..", newList.Count) + DateTime.Now;
                    MainMail += s_log1 + "<br />";
                    ESAPIController.logger.Debug(s_log1);

                    foreach (var item in newList)
                    {
                        ESAPIController.logger.Debug("ErrorData_TItle:" + item.title);

                        #region 介接欄位
                        NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                        postParams.Add("oid", item.oid);//機關物件識別碼
                        postParams.Add("identifier", item.identifier);//服務代碼(機關代碼 服務編號))
                        postParams.Add("categorytheme", item.categorytheme);//主題分類代碼
                        postParams.Add("categorycake", item.categorycake);//施政分類代碼
                        postParams.Add("categoryservice", item.categoryservice);//服務分類代碼
                        postParams.Add("keywords", item.keywords);//關鍵字
                        postParams.Add("notificationemails", item.notificationemails);//郵件通知信箱
                        postParams.Add("language", "");//機關服務語系 0中文(預設),1英文
                        postParams.Add("title", item.title+"*");//標題
                        postParams.Add("statusreason", item.statusreason);//狀態原因
                        postParams.Add("replacecontent", item.replacecontent);//智能取代
                        postParams.Add("function", item.function);//辦理方式 線上申辦1(預設)、臨櫃2、郵寄3，如有2種以上辦理方式，請用半形逗號區隔
                                                                  // 線上申辦欄位
                        postParams.Add("servicecontent1", item.servicecontent1);//線上申辦服務內容
                        postParams.Add("criteria1", item.criteria1);//線上申辦申辦資格
                        postParams.Add("procedure1", item.procedure1);//線上申辦申辦流程
                        postParams.Add("documentary1", item.documentary1);//線上申辦應備物品
                        postParams.Add("mydataresourceid1", item.mydataresourceid1.ToString());//線上申辦是否使用MyData
                        postParams.Add("workingdays1", item.workingdays1.ToString());//線上申辦作業天數
                        postParams.Add("contactperson1", item.contactperson1);//線上申辦聯絡窗口
                        postParams.Add("remarks1", item.remarks1);//線上申辦備註
                        postParams.Add("reference1", item.reference1);//線上申辦參考資料
                        postParams.Add("onlinelink", item.onlinelink);//線上申辦連結
                                                                      // 臨櫃欄位
                        postParams.Add("servicecontent2", item.servicecontent2);// 臨櫃服務內容
                        postParams.Add("criteria2", item.criteria2);// 臨櫃申辦資格
                        postParams.Add("procedure2", item.procedure2);// 臨櫃申辦流程
                        postParams.Add("documentary2", item.documentary2);// 臨櫃應備物品
                        postParams.Add("mydataresourceid2", item.mydataresourceid2.ToString()); // 臨櫃是否使用MyData
                        postParams.Add("workingdays2", item.workingdays2);// 臨櫃作業天數
                        postParams.Add("contactperson2", item.contactperson2);// 臨櫃聯絡窗口
                        postParams.Add("remarks2", item.remarks2); // 臨櫃備註
                        postParams.Add("reference2", item.reference2);// 臨櫃參考資料
                        postParams.Add("documentlink2", item.documentlink2);// 臨櫃檔案下載
                                                                            // 郵寄欄位
                        postParams.Add("servicecontent3", item.servicecontent3);// 郵寄服務內容
                        postParams.Add("criteria3", item.criteria3); // 郵寄申辦資格
                        postParams.Add("procedure3", item.procedure3); // 郵寄申辦流程
                        postParams.Add("documentary3", item.documentary3); //郵寄應備物品
                        postParams.Add("workingdays3", item.workingdays3); //郵寄作業天數
                        postParams.Add("contactperson3", item.contactperson3); //郵寄聯絡窗口
                        postParams.Add("remarks3", item.remarks3);//郵寄備註
                        postParams.Add("reference3", item.reference3);//郵寄參考資料
                        postParams.Add("documentlink3", item.documentlink3);//郵寄檔案下載

                        #endregion

                        #region 介接資訊
                        string byteArray = JsonConvert.SerializeObject(postParams.AllKeys.ToDictionary(k => k, k => postParams[k]));

                        var webRequest = WebRequest.Create(APIURL) as HttpWebRequest;
                        //webRequest.Credentials = new System.Net.NetworkCredential("userName", "password");
                        webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                        webRequest.ContentType = "application/json";
                        webRequest.UserAgent = "Nothing";
                        webRequest.Method = "POST";
                        webRequest.Timeout = iTimeout1;
                        webRequest.PreAuthenticate = true;
                        webRequest.UseDefaultCredentials = false;
                        // 正式機/測試機金鑰
                        webRequest.Headers.Add("Authorization", comUtil.APIURL_key1);
                        webRequest.Accept = "application/json";
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        //System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                        using (var reqStream = new StreamWriter(webRequest.GetRequestStream()))
                        {
                            var temp = "[" + byteArray + "]";
                            reqStream.Write(temp);
                        }
                        #endregion 介接資訊

                        #region 介接回傳
                        using (var s = webRequest.GetResponse().GetResponseStream())
                        {
                            using (var sr = new StreamReader(s))
                            {
                                var contributorsAsJson = sr.ReadToEnd();
                                var temp = JsonConvert.DeserializeObject<objData1>(contributorsAsJson);
                                //if (temp.Code.ToString() != "-999")
                                {
                                    var model = new APIResult();
                                    model.identifier = item.identifier;
                                    model.code = temp.Code;
                                    model.datetime = temp.DataTime;
                                    model.message = temp.Message;
                                    model.success = temp.Success;
                                    model.method = "POST";
                                    model.servicejson = byteArray;
                                    var obj2 = JsonConvert.SerializeObject(temp.ResultData);
                                    model.resultdata = obj2;
                                    contributors.Add(model);
                                }
                                // 寄信用當日異動案件介接是否成功
                                dict[item.identifier] = temp.Code.ToString();
                            }
                        }
                        #endregion 介接回傳 
                        InsertDB(contributors);
                        contributors.Clear();
                    }
                }
                return dict;
            }         
            catch(Exception ex)
            {
                ESAPIController.logger.Debug("POST_ERROR: " + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 抓取API PUT 介接資料
        /// </summary>
        static Dictionary<string, object> PutService()
        {
            try
            {
                string APIURL = comUtil.APIURL;
                List<APIResult> contributors = new List<APIResult>();
                // 異動案件，紀錄結果
                Dictionary<string, object> dict = new Dictionary<string, object>();
                var newList = new List<serviceJson2>();
                // 取得當日修改資料

                #region 查詢
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn);
                    //2.16.886.101.20003.20065.20005 
                    string qry = @"
                SELECT CASE
                         WHEN unit.unit_name LIKE '福利部' THEN '2.16.886.101.20003.20065'
		                 WHEN unit.UNIT_NAME LIKE '社會救助及社工司' THEN '2.16.886.101.20003.20065.20004'
		                 WHEN unit.UNIT_NAME LIKE '醫事司' THEN '2.16.886.101.20003.20065.20005'
		                 WHEN unit.UNIT_NAME LIKE '中醫藥司' THEN '2.16.886.101.20003.20065.20006'
		                 WHEN unit.UNIT_NAME LIKE '社會及家庭署' THEN '2.16.886.101.20003.20065.20025'
		                 WHEN unit.UNIT_NAME LIKE '國民健康署' THEN '2.16.886.101.20003.20065.20026'
		                 WHEN unit.UNIT_NAME LIKE '食品藥物管理署' THEN '2.16.886.101.20003.20065.20065'
		                 WHEN unit.UNIT_NAME LIKE '護理及健康照護司' THEN '2.16.886.101.20003.20065.20066'
                         WHEN unit.unit_name LIKE '秘書處' THEN '2.16.886.101.20003.20065.20010'
                         WHEN unit.unit_name LIKE '國民年金監理會' THEN '2.16.886.101.20003.20065.20014'
                         WHEN unit.unit_name LIKE '全民健康保險爭議審議會' THEN '2.16.886.101.20003.20065.20018'
                         WHEN unit.unit_name LIKE '衛生福利部法規會' THEN '2.16.886.101.20003.20065.20069'
                         WHEN unit.unit_name LIKE '國民年金監理會' THEN '2.16.886.101.20003.20065.20014'
                         ELSE ''
                       END                                   AS OID,
                       Concat('327220000I', 'SRV', S.srv_id) AS IDENTIFIER,
                       Isnull(S.cls_sub_cd, '')              AS CATEGORYTHEME,
                       Isnull(S.cls_adm_cd, '')              AS CATEGORYCAKE,
                       Isnull(S.cls_srv_cd, '')              AS CATEGORYSERVICE,
                       Isnull(S.keyword, '')                 AS KEYWORDS,
                       Isnull(S.NAME, '')                    AS TITLE,
                       Isnull(S.srv_desc, S.NAME)            AS DESCRIPTION,
                       Isnull(N.content_1, '')               AS 'PROCEDURE',
                       Isnull(N.content_4, '')               AS CONTACTPERSON,
                       Isnull(N.content_2, '')               AS DOCUMENTARY,
                       Isnull(S.pro_deadline, '')            AS WORKINGDAYS,
                       Isnull(N.content_3, '')               AS REMARKS,
                       CASE
                         WHEN(SELECT srv_id
                              FROM   service_notice
                              WHERE  service_notice.srv_id = S.srv_id
                              GROUP  BY srv_id) IS NOT NULL THEN Concat(
                         N'https://eservice.mohw.gov.tw/Service/Notice/', S.srv_id)
                         ELSE ''
                       END                                   AS ONLINELINK,
                       CASE
                         WHEN(SELECT srv_id
                              FROM   service_file
                              WHERE  service_file.srv_id = S.srv_id
                              GROUP  BY srv_id) IS NOT NULL THEN Concat(
                         N'https://eservice.mohw.gov.tw/Service/File/', S.srv_id)
                         ELSE ''
                       END                                   AS DOCUMENTLINK,
                       Concat(N'https://eservices.taichung.gov.tw/AdvSearch/FormDownload/160/Download/', S.srv_id)                           AS DOCUMENTLINK2,
                       '#同臨櫃#'                         AS DOCUMENTLINK3,
                       ''                                    AS STATUSREASON,
                       'eservice@turbotech.com.tw;CCBOBO1@mohw.gov.tw;t92267@gmail.com'           AS NOTIFICATIONEMAILS
                FROM   service S
                       LEFT JOIN service_notice N
                              ON N.srv_id = S.srv_id
                       LEFT JOIN (SELECT ut.unit_name,
                                         ut.unit_cd
                                  FROM   (SELECT CASE
                                                   WHEN s2ut.unit_level = 1 THEN unit_name
                                                   ELSE (SELECT s3ut.unit_name
                                                         FROM   unit s3ut
                                                         WHERE  s3ut.unit_cd = s2ut.unit_pcd)
                                                 END AS UNIT_NAME,
                                                 s2ut.unit_cd
                                          FROM   unit s2ut) ut) unit
                              ON unit.unit_cd = S.fix_unit_cd 
            ";
                    // 當日新增(異動)
                    qry += @" WHERE CAST(S.UPD_TIME AS DATE) = CAST(GETDATE() AS DATE)";

                    var cmd = new SqlCommand(qry, conn);
                    cmd.CommandType = CommandType.Text;
                    //con.Open();
                    using (SqlDataReader objReader = cmd.ExecuteReader())
                    {
                        if (objReader.HasRows)
                        {
                            while (objReader.Read())
                            {
                                var model = new serviceJson2();
                                model.oid = objReader.GetString(objReader.GetOrdinal("OID"));
                                model.identifier = objReader.GetString(objReader.GetOrdinal("IDENTIFIER"));
                                model.categorytheme = objReader.GetString(objReader.GetOrdinal("CATEGORYTHEME"));
                                model.categorycake = objReader.GetString(objReader.GetOrdinal("CATEGORYCAKE"));
                                model.categoryservice = objReader.GetString(objReader.GetOrdinal("CATEGORYSERVICE"));
                                model.keywords = objReader.GetString(objReader.GetOrdinal("KEYWORDS"));
                                model.notificationemails = objReader.GetString(objReader.GetOrdinal("NOTIFICATIONEMAILS"));
                                model.title = objReader.GetString(objReader.GetOrdinal("TITLE"));
                                model.statusreason = objReader.GetString(objReader.GetOrdinal("STATUSREASON"));
                                model.replacecontent = string.Empty;
                                switch (objReader.GetString(objReader.GetOrdinal("IDENTIFIER")))
                                {
                                    case "327220000ISRV005014":
                                        model.function = "1,3";
                                        break;
                                    case "327220000ISRV005013":
                                        model.function = "1,3";
                                        break;
                                    case "327220000ISRV010002":
                                        model.function = "2";
                                        break;
                                    case "327220000ISRV012001":
                                        model.function = "1,2,3";
                                        break;
                                    case "327220000ISRV001039":
                                        model.function = "1,2,3";
                                        break;
                                    case "327220000ISRV041001": /*全民健康保險爭議案件(權益案件及特約管理案件)線上申辦*/
                                        model.function = "1,2,3";
                                        break;
                                    default:
                                        model.function = "1";
                                        break;
                                }
                                model.servicecontent1 = objReader.GetString(objReader.GetOrdinal("DESCRIPTION"));
                                model.criteria1 = string.Empty;
                                model.procedure1 = objReader.GetString(objReader.GetOrdinal("PROCEDURE"));
                                model.documentary1 = objReader.GetString(objReader.GetOrdinal("DOCUMENTARY"));
                                model.mydataresourceid1 = false;
                                model.workingdays1 = objReader.GetInt32(objReader.GetOrdinal("WORKINGDAYS")).ToString();
                                model.contactperson1 = objReader.GetString(objReader.GetOrdinal("CONTACTPERSON"));
                                model.remarks1 = objReader.GetString(objReader.GetOrdinal("REMARKS"));
                                model.reference1 = objReader.GetString(objReader.GetOrdinal("DOCUMENTLINK"));
                                model.onlinelink = objReader.GetString(objReader.GetOrdinal("ONLINELINK"));
                                var onlineWay = "#同線上申辦#";
                                model.servicecontent2 = onlineWay;
                                model.criteria2 = onlineWay;
                                model.procedure2 = onlineWay;
                                model.documentary2 = onlineWay;
                                model.mydataresourceid2 = false;
                                model.workingdays2 = onlineWay;
                                model.contactperson2 = onlineWay;
                                model.remarks2 = onlineWay;
                                model.reference2 = onlineWay;
                                model.documentlink2 = model.function.Contains("2") ? objReader.GetString(objReader.GetOrdinal("DOCUMENTLINK2")) : null;
                                model.servicecontent3 = onlineWay;
                                model.criteria3 = onlineWay;
                                model.procedure3 = onlineWay;
                                model.documentary3 = onlineWay;
                                model.workingdays3 = onlineWay;
                                model.contactperson3 = onlineWay;
                                model.remarks3 = onlineWay;
                                model.reference3 = onlineWay;
                                model.documentlink3 = model.function.Contains("3") ? objReader.GetString(objReader.GetOrdinal("DOCUMENTLINK")) : null;
                                //I would also check for DB.Null here before reading the value.
                                newList.Add(model);
                                dict.Add(model.identifier, "");
                            }
                        }
                        objReader.Close();
                    }
                    DataUtils.CloseDbConn(conn);
                }
                #endregion 查詢

                if (newList != null && newList.Count > 0)
                {
                    s_log1 = string.Format("newList筆數：{0}..", newList.Count) + DateTime.Now;
                    MainMail += s_log1 + "<br />";
                    ESAPIController.logger.Debug(s_log1);

                    foreach (var item in newList)
                    {
                        ESAPIController.logger.Debug("ErrorData_TItle:" + item.title);

                        #region 介接欄位
                        NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                        postParams.Add("oid", item.oid);//機關物件識別碼
                        postParams.Add("identifier", item.identifier);//服務代碼(機關代碼 服務編號))
                        postParams.Add("categorytheme", item.categorytheme);//主題分類代碼
                        postParams.Add("categorycake", item.categorycake);//施政分類代碼
                        postParams.Add("categoryservice", item.categoryservice);//服務分類代碼
                        postParams.Add("keywords", item.keywords);//關鍵字
                        postParams.Add("notificationemails", item.notificationemails);//郵件通知信箱
                        postParams.Add("language", "");//機關服務語系 0中文(預設),1英文
                        postParams.Add("title", item.title+"*");//標題
                        postParams.Add("statusreason", item.statusreason);//狀態原因
                        postParams.Add("replacecontent", item.replacecontent);//智能取代
                        postParams.Add("function", item.function);//辦理方式 線上申辦1(預設)、臨櫃2、郵寄3，如有2種以上辦理方式，請用半形逗號區隔
                                                                  // 線上申辦欄位
                        postParams.Add("servicecontent1", item.servicecontent1);//線上申辦服務內容
                        postParams.Add("criteria1", item.criteria1);//線上申辦申辦資格
                        postParams.Add("procedure1", item.procedure1);//線上申辦申辦流程
                        postParams.Add("documentary1", item.documentary1);//線上申辦應備物品
                        postParams.Add("mydataresourceid1", item.mydataresourceid1.ToString());//線上申辦是否使用MyData
                        postParams.Add("workingdays1", item.workingdays1.ToString());//線上申辦作業天數
                        postParams.Add("contactperson1", item.contactperson1);//線上申辦聯絡窗口
                        postParams.Add("remarks1", item.remarks1);//線上申辦備註
                        postParams.Add("reference1", item.reference1);//線上申辦參考資料
                        postParams.Add("onlinelink", item.onlinelink);//線上申辦連結
                                                                      // 臨櫃欄位
                        postParams.Add("servicecontent2", item.servicecontent2);// 臨櫃服務內容
                        postParams.Add("criteria2", item.criteria2);// 臨櫃申辦資格
                        postParams.Add("procedure2", item.procedure2);// 臨櫃申辦流程
                        postParams.Add("documentary2", item.documentary2);// 臨櫃應備物品
                        postParams.Add("mydataresourceid2", item.mydataresourceid2.ToString()); // 臨櫃是否使用MyData
                        postParams.Add("workingdays2", item.workingdays2);// 臨櫃作業天數
                        postParams.Add("contactperson2", item.contactperson2);// 臨櫃聯絡窗口
                        postParams.Add("remarks2", item.remarks2); // 臨櫃備註
                        postParams.Add("reference2", item.reference2);// 臨櫃參考資料
                        postParams.Add("documentlink2", item.documentlink2);// 臨櫃檔案下載
                                                                            // 郵寄欄位
                        postParams.Add("servicecontent3", item.servicecontent3);// 郵寄服務內容
                        postParams.Add("criteria3", item.criteria3); // 郵寄申辦資格
                        postParams.Add("procedure3", item.procedure3); // 郵寄申辦流程
                        postParams.Add("documentary3", item.documentary3); //郵寄應備物品
                        postParams.Add("workingdays3", item.workingdays3); //郵寄作業天數
                        postParams.Add("contactperson3", item.contactperson3); //郵寄聯絡窗口
                        postParams.Add("remarks3", item.remarks3);//郵寄備註
                        postParams.Add("reference3", item.reference3);//郵寄參考資料
                        postParams.Add("documentlink3", item.documentlink3);//郵寄檔案下載
                        #endregion 介接欄位

                        #region 介接資訊
                        string byteArray = JsonConvert.SerializeObject(postParams.AllKeys.ToDictionary(k => k, k => postParams[k]));

                        var webRequest = WebRequest.Create(APIURL) as HttpWebRequest;
                        //webRequest.Credentials = new System.Net.NetworkCredential("userName", "password");
                        webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                        webRequest.ContentType = "application/json";
                        webRequest.UserAgent = "Nothing";
                        webRequest.Method = "PUT";
                        webRequest.Timeout = iTimeout1;
                        webRequest.PreAuthenticate = true;
                        webRequest.UseDefaultCredentials = false;
                        // 正式機/測試機金鑰
                        webRequest.Headers.Add("Authorization", comUtil.APIURL_key1);
                        webRequest.Accept = "application/json";
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                        using (var reqStream = new StreamWriter(webRequest.GetRequestStream()))
                        {
                            var temp = "[" + byteArray + "]";
                            reqStream.Write(temp);
                        }
                        #endregion 介接資訊

                        #region 介接回傳
                        using (var s = webRequest.GetResponse().GetResponseStream())
                        {
                            using (var sr = new StreamReader(s))
                            {
                                var contributorsAsJson = sr.ReadToEnd();
                                var temp = JsonConvert.DeserializeObject<objData1>(contributorsAsJson);
                                //if (temp.Code.ToString() != "-999")
                                {
                                    var model = new APIResult();
                                    model.identifier = item.identifier;
                                    model.code = temp.Code;
                                    model.datetime = temp.DataTime;
                                    model.message = temp.Message;
                                    model.success = temp.Success;
                                    model.method = "PUT";
                                    model.servicejson = byteArray;
                                    var obj2 = JsonConvert.SerializeObject(temp.ResultData);
                                    model.resultdata = obj2;
                                    contributors.Add(model);
                                    // 寄信用當日異動案件介接是否成功
                                    dict[item.identifier] = temp.Code.ToString();
                                }
                            }
                        }
                        #endregion 介接回傳

                        InsertDB(contributors);
                        contributors.Clear();
                    }
                }
                return dict;
            }
            catch(Exception ex)
            {
                ESAPIController.logger.Debug("PUT_ERROR: " + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// 抓取API DELETE 介接資料
        /// </summary>
        static Dictionary<string, object> DeleteService()
        {
            List<APIResult> contributors = new List<APIResult>();
            // 異動案件，紀錄結果
            Dictionary<string, object> dict = new Dictionary<string, object>();
            var newList = new List<string>();
            // 取得當日刪除(下架)資料

            #region 查詢
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn);
                string qry = @" SELECT concat('327220000I','SRV',S.SRV_ID) AS IDENTIFIER from SERVICE S ";
                // 當日刪除
                qry += @" WHERE CAST(S.DEL_TIME AS DATE) = CAST(GETDATE() AS DATE) AND S.DEL_MK = 'Y'";

                var cmd = new SqlCommand(qry, conn);
                cmd.CommandType = CommandType.Text;

                using (SqlDataReader objReader = cmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            var identifier = objReader.GetString(objReader.GetOrdinal("IDENTIFIER"));
                            //I would also check for DB.Null here before reading the value.
                            newList.Add(identifier);
                            dict.Add(identifier, "");
                        }
                    }
                    objReader.Close();
                }
                DataUtils.CloseDbConn(conn);
            }
            #endregion 查詢

            bool flag_no_data = false;
            if (newList == null) { flag_no_data = true; }
            else if (newList.Count == 0) { flag_no_data = true; }
            if (flag_no_data)
            {
                s_log1 = "newList筆數：0.." + DateTime.Now;
                MainMail += s_log1 + "<br />";
                ESAPIController.logger.Debug(s_log1);
            }
            if (newList != null && newList.Count > 0)
            {
                s_log1 = string.Format("newList筆數：{0}..", newList.Count) + DateTime.Now;
                MainMail += s_log1 + "<br />";
                ESAPIController.logger.Debug(s_log1);

                foreach (var item in newList)
                {
                    #region 介接欄位

                    string APIURL = comUtil.APIURL + item;
                    NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                    postParams.Add("oid", "2.16.886.101.20003.20065.20005");
                    postParams.Add("identifier", item);//服務代碼(機關代碼 服務編號))
                    postParams.Add("statusreason", "申辦項目下架");
                    #endregion 介接欄位

                    #region 介接資訊
                    string byteArray = JsonConvert.SerializeObject(postParams.AllKeys.ToDictionary(k => k, k => postParams[k]));

                    var webRequest = WebRequest.Create(APIURL) as HttpWebRequest;
                    //webRequest.Credentials = new System.Net.NetworkCredential("userName", "password");
                    webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "Nothing";
                    webRequest.Method = "DELETE";
                    webRequest.Timeout = iTimeout1;
                    webRequest.PreAuthenticate = true;
                    // 正式機/測試機金鑰
                    webRequest.Headers.Add("Authorization", comUtil.APIURL_key1);
                    webRequest.Accept = "application/json";
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                    using (var reqStream = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        var temp = "[" + byteArray + "]";
                        reqStream.Write(temp);
                    }
                    #endregion 介接資訊

                    #region 介接回傳
                    using (var s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var contributorsAsJson = sr.ReadToEnd();
                            var temp = JsonConvert.DeserializeObject<objData1>(contributorsAsJson);
                            //if (temp.Code.ToString() != "-999")
                            //{
                            var model = new APIResult();
                            model.identifier = item;
                            model.code = temp.Code;
                            model.datetime = temp.DataTime;
                            model.message = temp.Message;
                            model.success = temp.Success;
                            model.method = "DELETE";
                            model.servicejson = byteArray;
                            var obj2 = JsonConvert.SerializeObject(temp.ResultData);
                            model.resultdata = obj2;
                            contributors.Add(model);
                            // 寄信用當日異動案件介接是否成功
                            dict[item] = temp.Code.ToString();
                            //}
                        }
                    }
                    #endregion 介接回傳

                    InsertDB(contributors);
                    contributors.Clear();
                }
            }
            return dict;
        }

        /// <summary>
        /// 寄出log
        /// </summary>
        /// <param name="MailBody"></param>
        public static void LogSend(string MailBody)
        {
            var mails = DataUtils.GetConfig("ESAPI_LogSend");
            if (string.IsNullOrEmpty(mails))
            {
                mails = "eservice@turbotech.com.tw";
            }
            var subject = "衛福部我的E政府上稿排程紀錄" + DateTime.Now;
            foreach( var item in mails.ToSplit(','))
            {
                MailMessage mailMessage = NewMail(ConfigModel.MailSenderAddr, item, subject, MailBody);
                mailMessage.IsBodyHtml = true;
                SendMail(mailMessage);
            }
        }
        
        /// <summary>
        /// 寫入DB
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        static void InsertDB(List<APIResult> dt)
        {
            if (dt != null && dt.Count > 0)
            {
                foreach (var item in dt)
                {
                    Insert("APIRESULT", ClassToHashtable(item));
                }
            }
        }

        /// <summary>
        /// Let Class became Hashtable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static Hashtable ClassToHashtable<T>(T Model)
        {
            Hashtable Parameter = new Hashtable();
            try
            {
                foreach (var pi in Model.GetType().GetProperties())
                {
                    var piName = pi.Name;
                    var piType = pi.PropertyType.Name;
                    if (pi.PropertyType.Name.Contains("Nullable"))
                    {
                        piType = pi.PropertyType.GenericTypeArguments.FirstOrDefault().Name;
                        ESAPIController.logger.Debug(piType);
                    }

                    var piValue = pi.GetValue(Model);
                    if (piValue != null)
                    {
                        Parameter[piName] = piType + "^" + piValue != null ? piValue.ToString() : "";
                        ESAPIController.logger.Debug(Parameter[piName]);
                    }
                }
            }
            catch (Exception ex)
            {
                ESAPIController.logger.Debug(ex.Message);
                ESAPIController.logger.Debug(ex.StackTrace);
                return null;
            }
            return Parameter;
        }

        /// <summary>
        /// Using Table-Name and Parameter-Hashtable to Insert Data,return int that success row count.
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public static void Insert(string Table, Hashtable Parameter)
        {

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn); //conn.Open();
                ESAPIController.logger.Debug("SqlConnection Open()");
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    ESAPIController.logger.Debug("BaseAction_Insert(" + Table + "," + Parameter + ")");

                    IList<string> _keyString = new List<string>();
                    IList<string> _valueString = new List<string>();
                    foreach (var para in Parameter.Keys)
                    {
                        var _key = para != null ? para.ToString() : "";
                        var _paramkey = "@" + _key;
                        _keyString.Add(_key);
                        _valueString.Add(_paramkey);
                    }
                    string _sql_key = string.Join(" , ", _keyString);
                    string _sql_value = string.Join(" , ", _valueString);
                    string _sql = "INSERT INTO " + Table + " ( " + _sql_key + ") VALUES (" + _sql_value + ")";
                    SqlCommand _cmd = new SqlCommand(_sql, conn, tran);
                    foreach (var key in _keyString)
                    {
                        _cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = Parameter[key].TONotNullString();
                        ESAPIController.logger.Debug("MSSQL Insert Command Parameter:['@" + key + "','" + Parameter[key].TONotNullString() + "']");
                    }
                    _cmd.ExecuteNonQuery();
                    tran.Commit();
                    ESAPIController.logger.Debug("SqlTransaction Commit()");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ESAPIController.logger.Debug("SqlTransaction Rollback()");
                    ESAPIController.logger.Debug(ex.Message);
                    ESAPIController.logger.Debug(ex.StackTrace);
                }
                finally
                {
                    DataUtils.CloseDbConn(conn); //conn.Close();
                }
                ESAPIController.logger.Debug("SqlConnection Close()");
            }
        }
        
        /// <summary>
        /// 寄信
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachmentPaths"></param>
        /// <param name="isBodyHtml"></param>
        /// <returns></returns>
        public static MailMessage NewMail(string from, string to, string subject, string body,
                                        string[] attachmentPaths = null, bool isBodyHtml = false)
        {
            if (string.IsNullOrEmpty(from)) from = ConfigModel.MailSenderAddr;

            if (string.IsNullOrEmpty(from)) throw new ArgumentNullException("寄件者電子郵件位址不可為空。");
            if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("收件者電子郵件位址不可為空。");
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException("郵件主旨不可為空。");

            MailMessage message = new MailMessage(from, to, subject, body);
            message.IsBodyHtml = isBodyHtml;
            //AddAttachment(message, attachmentPaths);
            return message;
        }
        
        /// <summary>執行電子郵件傳送</summary>
        /// <param name="message">要處理的電子郵件</param>
        /// <param name="credential">（非必要）電子郵件寄件者帳號與密碼。輸入 null 表示使用系統預設值。</param>
        /// <param name="mailServer">（非必要）電子郵件服務主機 IP。輸入 null 表示使用系統預設值。</param>
        /// <returns>電子郵件傳送處理結果</returns>
        public static void SendMail(MailMessage message, NetworkCredential credential = null, string mailServer = null)
        {
            SmtpClient client = null;
            try
            {
                if (message == null) throw new ArgumentNullException("電子郵件內容不可為空。");
                if (message.From == null) throw new ArgumentNullException("寄件者電子郵件位址不可為空。");
                if (message.To == null || message.To.Count == 0) throw new ArgumentNullException("收件者電子郵件位址不可為空。");
                if (string.IsNullOrEmpty(message.Subject)) throw new ArgumentNullException("郵件主旨不可為空。");

                //取得系統郵件服務主機 IP
                if (string.IsNullOrEmpty(mailServer))
                {
                    mailServer = ConfigModel.MailServer;
                }

                //取得系統寄件者電子郵件地址與密碼
                if (credential == null)
                {
                    credential = new NetworkCredential(ConfigModel.MailSenderAcc, ConfigModel.MailSenderPwd);
                }

                //使用 SMTP 協定傳送電子郵件
                client = new SmtpClient(mailServer);
                client.Credentials = (client.Credentials == null) ? credential : CredentialCache.DefaultNetworkCredentials;

                string s_MailServerPort = ConfigModel.MailServerPort ?? "";
                if (!string.IsNullOrEmpty(s_MailServerPort)) { client.Port = Convert.ToInt32(s_MailServerPort); }

                string s_EnableSsl = ConfigModel.MailEnableSsl ?? "";
                if (s_EnableSsl.Equals("Y")) { client.EnableSsl = true; }

                client.Send(message);
            }
            catch (SmtpFailedRecipientsException ex)
            {
                ESAPIController.logger.Debug(ex.Message);
                ESAPIController.logger.Debug(ex.StackTrace);
                string errText = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
                int errCode = (ex.InnerException == null) ? ex.HResult : ex.InnerException.HResult;
                ESAPIController.logger.Debug(errText);
                ESAPIController.logger.Debug(errCode);
            }
            catch (Exception ex)
            {
                ESAPIController.logger.Debug(ex.Message);
                ESAPIController.logger.Debug(ex.StackTrace);
                string errText = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
                int errCode = (ex.InnerException == null) ? ex.HResult : ex.InnerException.HResult;
                errText = (errCode == -2146233079) ? "系統郵件服務主機無法連線。" : errText;
                ESAPIController.logger.Debug(errText);
                ESAPIController.logger.Debug(errCode);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
        }
    }
}