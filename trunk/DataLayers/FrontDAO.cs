using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Services;
using EECOnline.Commons;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using Turbo.Commons;
using System;
using Newtonsoft.Json;
using System.Reflection;
using System.Web;
using System.Net.Mail;
using System.Text.RegularExpressions;
using HppApi;
using EECOnline.Utils;

namespace EECOnline.DataLayers
{
    public class FrontDAO : BaseDAO
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public byte[] sftpDownload(string fullPath, DateTime dt)
        {
            try
            {
                var localPath = "";

                TblSETUP st = new TblSETUP();
                st.setup_cd = "APPLY_FILE";
                var stdata = GetRow(st);

                localPath = stdata.setup_val.Replace('\\', '/') + "/" + dt.Year + "/" + dt.ToString("MM") + "/" + fullPath;
                localPath = localPath.Replace('\\', '/');

                return File.ReadAllBytes(localPath);
            }
            catch (Exception ex)
            {
                LOG.Error("下載檔案失敗(sftpDownload)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("下載檔案失敗，原因：{0}", ex.Message));
            }
        }

        public IList<SearchGridModel> GetSearchApplyList(string TabType, SearchApplyModel FilterModel)
        {
            Hashtable Parmas = new Hashtable();
            Parmas["user_idno"] = FilterModel.user_idno;
            switch (TabType)
            {
                case "1": Parmas["FilterMonth"] = FilterModel.Search1Filter; break;
                case "2": Parmas["FilterMonth"] = FilterModel.Search2Filter; break;
                case "3": Parmas["FilterMonth"] = FilterModel.Search3Filter; break;
                default: Parmas["FilterMonth"] = ""; break;
            }
            return base.QueryForListAll<SearchGridModel>("Front.getSearchApplyList" + TabType, Parmas);
        }

        /// <summary>
        /// 取得明細金額加總
        /// </summary>
        private string GetTotalTransAmt(SearchApplyDetailModel model)
        {
            long tmpSum = 0;
            foreach (var row in model.DetailPrice)
            {
                tmpSum = tmpSum + (row.price ?? 0);
            }
            return tmpSum.ToString();
        }

        private string GetTotalTransAmt_forLogin(LoginApplyModel model)
        {
            long tmpSum = 0;
            foreach (var row in model.ApplyDetail.FirstOrDefault().ApplyDetailPrice)  // 現在一次申請只能一筆醫院 所以這邊直接用 FirstOrDefault()
            {
                tmpSum = tmpSum + (row.price ?? 0);
            }
            return tmpSum.ToString();
        }

        /// <summary>
        /// PAY_EEC_ 相關參數資料集擷取
        /// </summary>
        public string GetDataFromHashtableList(IList<Hashtable> datas, string key)
        {
            //return datas.Where(x => x["setup_cd"].TONotNullString() == key).FirstOrDefault()["setup_val"].TONotNullString();
            try
            {
                var item = datas.Where(x => x["setup_cd"].TONotNullString() == key);
                if (item.ToCount() == 1) return item.FirstOrDefault()["setup_val"].TONotNullString();
                return "";
            }
            catch (Exception ex)
            {
                LOG.Debug("GetDataFromHashtableList() Error: " + ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// 聯合信用卡中心 SessionTransactionKey <br/>
        /// 去取得付款用的 SessionKey
        /// </summary>
        public string Get_ApplyDetail_Pay_SessionTransactionKey_forLogin(LoginApplyModel model, ref string ErrCode, ref string ErrMsg)
        {
            if (model == null) return "";
            var GetSetups = base.QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);
            var tmpMERCHANTID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID");
            var tmpTRMINALID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID");
            if (model.ApplyDetail.FirstOrDefault().hospital_code == "1131010011H")  // 亞東
            {
                tmpMERCHANTID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID_FE");
                tmpTRMINALID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID_FE");
            }
            var ECmodel = new CreditHPPModel();
            ECmodel.EncModel.MerchantID = tmpMERCHANTID;
            ECmodel.EncModel.OrderID = "";
            ECmodel.EncModel.TerminalID = tmpTRMINALID;
            ECmodel.EncModel.TransAmt = GetTotalTransAmt_forLogin(model);
            ECmodel.EncModel.TransMode = "0";
            ECmodel.EncModel.Template = "BOTH";
            ECmodel.EncModel.CardholderName = "";
            ECmodel.EncModel.CardholderEmailAddress = "";
            ECmodel.EncModel.IDNUMBER = Convert.ToString(model.user_idno);  // 持卡人身份證字號
            ECmodel.EncModel.PrivateData = "";  // 自訂資料
            ECmodel.ECConnetModel.DomainName = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_DOMAINNAME");
            ECmodel.ECConnetModel.RequestURL = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_REQUESTURL");
            // 回應網址
            ECmodel.EncModel.NotifyURL = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_NOTIFYURL");
            // 聯合信用卡
            ApiClient resEC = CardUtils.GetTransactionKeyEC(ECmodel, model.ApplyDetail.FirstOrDefault().apply_no_sub);
            if (resEC != null)
            {
                ErrCode = resEC.getRESPONSECODE();
                ErrMsg = resEC.getRESPONSEMSG();
                if (ErrCode.Equals("00"))
                {
                    // 作業執行成功
                    var orderid = resEC.getORDERID();
                    var sessionkey = resEC.getKEY();
                    var transdate = resEC.getTRANSDATE();
                    var approvecode = resEC.getAPPROVECODE();
                    TblEEC_ApplyDetail where = new TblEEC_ApplyDetail()
                    {
                        apply_no = model.apply_no,
                        apply_no_sub = model.ApplyDetail.FirstOrDefault().apply_no_sub,  // 現在一次申請只能一筆醫院 所以這邊直接用 FirstOrDefault()
                    };
                    TblEEC_ApplyDetail update = new TblEEC_ApplyDetail()
                    {
                        payed_orderid = orderid,
                        payed_sessionkey = sessionkey,
                        payed_transdate = transdate,
                        payed_approvecode = approvecode,
                    };
                    int res = base.Update(update, where);
                    return sessionkey;
                }
            }
            else
            {
                ErrCode = "-1";
                ErrMsg = "取得預約交易代碼失敗";
            }
            return "";
        }

        /// <summary>
        /// 聯合信用卡中心 SessionTransactionKey <br/>
        /// 去取得付款用的 SessionKey
        /// </summary>
        public string Get_ApplyDetail_Pay_SessionTransactionKey(SearchApplyDetailModel model, ref string ErrCode, ref string ErrMsg)
        {
            if (model == null) return "";
            var GetSetups = base.QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);
            var tmpMERCHANTID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID");
            var tmpTRMINALID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID");
            if (model.hospital_code == "1131010011H")  // 亞東
            {
                tmpMERCHANTID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID_FE");
                tmpTRMINALID = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID_FE");
            }
            var ECmodel = new CreditHPPModel();
            ECmodel.EncModel.MerchantID = tmpMERCHANTID;
            ECmodel.EncModel.OrderID = "";
            ECmodel.EncModel.TerminalID = tmpTRMINALID;
            ECmodel.EncModel.TransAmt = GetTotalTransAmt(model);
            ECmodel.EncModel.TransMode = "0";
            ECmodel.EncModel.Template = "BOTH";
            ECmodel.EncModel.CardholderName = "";
            ECmodel.EncModel.CardholderEmailAddress = "";
            ECmodel.EncModel.IDNUMBER = Convert.ToString(model.user_idno);  // 持卡人身份證字號
            ECmodel.EncModel.PrivateData = "";  // 自訂資料
            ECmodel.ECConnetModel.DomainName = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_DOMAINNAME");
            ECmodel.ECConnetModel.RequestURL = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_REQUESTURL");
            // 回應網址
            ECmodel.EncModel.NotifyURL = this.GetDataFromHashtableList(GetSetups, "PAY_EEC_NOTIFYURL");
            // 聯合信用卡
            ApiClient resEC = CardUtils.GetTransactionKeyEC(ECmodel, model.apply_no_sub);
            if (resEC != null)
            {
                ErrCode = resEC.getRESPONSECODE();
                ErrMsg = resEC.getRESPONSEMSG();
                if (ErrCode.Equals("00"))
                {
                    // 作業執行成功
                    var orderid = resEC.getORDERID();
                    var sessionkey = resEC.getKEY();
                    var transdate = resEC.getTRANSDATE();
                    var approvecode = resEC.getAPPROVECODE();
                    TblEEC_ApplyDetail where = new TblEEC_ApplyDetail()
                    {
                        apply_no = model.apply_no,
                        apply_no_sub = model.apply_no_sub,
                    };
                    TblEEC_ApplyDetail update = new TblEEC_ApplyDetail()
                    {
                        payed_orderid = orderid,
                        payed_sessionkey = sessionkey,
                        payed_transdate = transdate,
                        payed_approvecode = approvecode,
                    };
                    int res = base.Update(update, where);
                    return sessionkey;
                }
            }
            else
            {
                ErrCode = "-1";
                ErrMsg = "取得預約交易代碼失敗";
            }
            return "";
        }

        public IList<NewsGridModel> GetHomeNews()
        {
            return base.QueryForList<NewsGridModel>("Front.getHomeNews", null);
        }

        public IList<Hashtable> Get_EEC_CommonType()
        {
            return base.QueryForListAll<Hashtable>("Front.get_EEC_CommonType", null);
        }

        public IList<Hashtable> Get_EEC_CommonTypeAll()
        {
            return base.QueryForListAll<Hashtable>("Front.get_EEC_CommonTypeAll", null);
        }

        public enum em_lType : int
        {
            None = 0,
            Login1 = 1,
            Login2 = 2,
            Login3 = 3,
            Search1 = 4,
            Search2 = 5,
            Search3 = 6,
        };
        public enum em_lStatus : int
        {
            LoginTry = 0,
            LoginSuccess = 1,
            LoginFailed = 2
        };

        /// <summary>
        /// 查詢前台登入記錄用
        /// </summary>
        /// <param name="idno">操作者 IDNO</param>
        /// <param name="username">操作者 姓名</param>
        /// <param name="lType">登入方式</param>
        /// <param name="lStatus">登入狀態 (是/否成功)</param>
        /// <param name="lIP">IP</param>
        /// <param name="oType">操作功能別(編碼)</param>
        /// <param name="oName">操作功能別(名稱)</param>
        /// <param name="isDL">是否下載病歷</param>
        public static void FrontLOG(string idno, string username, em_lType lType, em_lStatus lStatus,
            string lIP, string oType, string oName, bool isDL = false)
        {
            try
            {
                var InsLog = new TblEEC_UserOperation();
                InsLog.keyid = null;
                InsLog.idno = idno;
                InsLog.name = username;
                InsLog.login_type = Convert.ToInt32(lType).ToString();
                InsLog.login_status = Convert.ToInt32(lStatus).ToString();
                InsLog.ip = lIP;
                InsLog.opra_time = DateTime.Now.ToString("yyyyMMddHHmmss");
                InsLog.opra_type = oType;
                InsLog.opra_type_name = oName;
                InsLog.is_download = (isDL ? "Y" : "N");
                InsLog.download_time = (isDL ? DateTime.Now.ToString("yyyyMMddHHmmss") : null);
                new FrontDAO().Insert(InsLog);
            }
            catch (Exception ex)
            {
                LOG.Debug("FrontDAO.FrontLOG() Error: " + ex.TONotNullString());
            }
        }
    }
}