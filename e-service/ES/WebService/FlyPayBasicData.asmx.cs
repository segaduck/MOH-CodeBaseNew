using ES.Action;
using ES.Controllers;
using ES.DataLayers;
using ES.Models;
using ES.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ES.WebService
{
    /// <summary>
    ///FlyPayBasicData 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    // [System.Web.Script.Services.ScriptService]
    public class FlyPayBasicData : System.Web.Services.WebService
    {

        //[WebMethod]
        //public Dictionary<string, string> SetData(string FIRSTNAME, string MIDDLENAME, string LASTNAME, string BIRTH, string GENDER
        //    , string MAINDOCNO, string NATIONALITY, string FLIGHTDATE, string FLIGHTNO, string DEPARTAIRPORT
        //    , string PASSENGERTYPE, string VCode)
        //{
        //    Dictionary<string, string> result = new Dictionary<string, string>();
        //    bool isFailure = false;
        //    var ErrorMsg = string.Empty;
        //    var GUID = string.Empty;
        //    if (string.IsNullOrWhiteSpace(VCode))
        //    {
        //        ErrorMsg += "請輸入驗證碼\n";
        //        isFailure = true;
        //    }
        //    else if (VCode != ConfigModel.FlyPayBasicVCode)
        //    {
        //        ErrorMsg += "無效驗證碼\n";
        //        isFailure = true;
        //    }
        //    if (string.IsNullOrEmpty(BIRTH))
        //    {
        //        ErrorMsg += "請輸入出生年月日\n";
        //        isFailure = true;
        //    }
        //    else if (BIRTH.Length != 10 || BIRTH.Replace("-", "").Length != 8)
        //    {
        //        ErrorMsg += "出生年月日格式不正確\n";
        //        isFailure = true;
        //    }
        //    if (string.IsNullOrEmpty(MAINDOCNO))
        //    {
        //        ErrorMsg += "請輸入護照號碼或身分證字號\n";
        //        isFailure = true;
        //    }
        //    if (string.IsNullOrEmpty(FLIGHTDATE))
        //    {
        //        ErrorMsg += "請輸入航班日期時間\n";
        //        isFailure = true;
        //    }
        //    if (string.IsNullOrEmpty(FLIGHTNO))
        //    {
        //        ErrorMsg += "請輸入航班代碼\n";
        //        isFailure = true;
        //    }
        //    // 驗證不通過
        //    if (isFailure)
        //    {
        //        result.Add("Message", ErrorMsg);
        //        return result;
        //    }
        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        SqlTransaction tran = conn.BeginTransaction();
        //        try
        //        {
        //            FlyPayAction dao = new FlyPayAction(conn, tran);
        //            GUID = dao.InsertFlyPayBasicData(FIRSTNAME, MIDDLENAME, LASTNAME, BIRTH, GENDER
        //            , MAINDOCNO, NATIONALITY, FLIGHTDATE, FLIGHTNO, DEPARTAIRPORT, PASSENGERTYPE);
        //            tran.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            // 儲存失敗
        //            tran.Rollback();
        //            isFailure = true;
        //            result.Add("Message", ex.Message);
        //            return result;
        //        }
        //        result.Add("Message", "成功");
        //        if(ConfigModel.WebTestEnvir == "Y")
        //        {
        //            // 測試環境
        //            result.Add("UrlRedirct", $"http://210.68.37.161:4155/FlyPay?GUID={GUID}");
        //        }
        //        else
        //        {
        //            // 正式環境
        //            result.Add("UrlRedirct", $"https://eservice.mohw.gov.tw/FlyPay?GUID={GUID}");
        //        }
        //    }
        //    return result;
        //}

        [WebMethod]
        public List<DictionaryEntry[]> GetData(List<FlyPayBasicModel> param)
        {
            var rst = new List<DictionaryEntry[]>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn);//conn.Open();
                FlyPayAction dao = new FlyPayAction(conn);
                rst = dao.GetAllGuidDataList(param);
                DataUtils.CloseDbConn(conn);
            }
            return rst;
        }

        [WebMethod]
        public List<DictionaryEntry[]> GetDataAll()
        {
            var rst = new List<DictionaryEntry[]>();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn);//conn.Open();
                FlyPayAction dao = new FlyPayAction(conn);
                rst = dao.GetAllGuidDataListAll();
                DataUtils.CloseDbConn(conn);
            }
            return rst;
        }

        // 取得資料庫連線
        //protected SqlConnection GetConnection()
        //{
        //    string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //    SqlConnection conn = new SqlConnection(connectionString);
        //    return conn;
        //}
    }
}
