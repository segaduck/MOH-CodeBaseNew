using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Areas.A6.Models;
using EECOnline.Services;
using EECOnline.Commons;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using System;
using Turbo.Commons;
using System.Web;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace EECOnline.DataLayers
{
    public class A6DAO : BaseDAO
    {
        #region 共用

        /// <summary>
        /// 傳入使用者輸入的密碼明文, 加密後回傳
        /// </summary>
        /// <param name="usePwd"></param>
        /// <returns></returns>
        private string EncPassword(string userPwd)
        {
            if (string.IsNullOrWhiteSpace(userPwd))
            {
                throw new ArgumentNullException("userPwd");
            }
            //TODO: 置換 RSACSP 改成不可逆的 Hash 方法
            RSACSP.RSACSP rsa = new RSACSP.RSACSP();
            return rsa.Utl_Encrypt(userPwd);
        }

        /// <summary>
        /// SHA512加密法
        /// </summary>
        /// <param name="originText">加密前字串（必須是明文密碼，請勿傳入加密之後的密碼）</param>
        /// <returns></returns>
        public string CypherText(string originText)
        {
            string strRtn = "";
            using (System.Security.Cryptography.SHA512CryptoServiceProvider sha512 = new System.Security.Cryptography.SHA512CryptoServiceProvider())
            {
                byte[] dataToHash = System.Text.Encoding.UTF8.GetBytes(originText);
                byte[] hashvalue = sha512.ComputeHash(dataToHash);
                strRtn = Convert.ToBase64String(hashvalue);
            }
            return strRtn;
        }
        #endregion

        #region C101M

        /// <summary>
        /// 查詢群組
        /// </summary>
        public IList<C101MGridModel> QueryC101MGrid(C101MFormModel parm)
        {
            var list = base.QueryForList<C101MGridModel>("A6.queryC101MGrid", parm);
            return list;
        }

        /// <summary>
        /// 新增群組檢核
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckC101M(C101MDetailModel model)
        {
            string msg = "";

            // 啟用狀態(防止F12)
            if (string.IsNullOrEmpty(model.grp_status))
            {
                msg = "請選擇群組狀態 !";
            }

            // (防止F12)
            if (string.IsNullOrEmpty(model.unit_cd))
            {
                msg = "請選擇單位 !";
            }

            if (model.grpname != model.hdgrpname)
            {
                // 名稱檢核
                TblAMGRP gp = new TblAMGRP();
                gp.grpname = model.grpname;
                var gpdata = GetRow(gp);

                if (gpdata != null)
                {
                    msg = "群組名稱重複，請重新輸入 !";
                }
            }

            return msg;
        }

        /// <summary>
        /// 新增群組
        /// </summary>
        /// <param name="detail"></param>
        public void AppendC101MDetail(C101MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                //新增 AMGRP
                model.modip = sm.UserInfo.LoginIP.TONotNullString();
                model.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                model.moduser = sm.UserInfo.UserNo.TONotNullString();
                model.modusername = sm.UserInfo.User.username.TONotNullString();
                model.del_mk = "N";

                TblAMGRP grp = new TblAMGRP();
                grp.InjectFrom(model);

                base.Insert(grp);

                //Insert<C101MDetailModel>(model);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("AppendC101MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 更新群組
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateC101MDetail(C101MDetailModel model)
        {
            //整批交易管理
            base.BeginTransaction();
            try
            {
                //更新 AMGRP 資料表
                TblAMGRP where = new TblAMGRP();
                where.grp_id = model.grp_id.TOInt32();
                TblAMGRP newdata = new TblAMGRP();
                newdata.InjectFrom(model);

                base.Update(newdata, where);
                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("UpdateC101MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 刪除群組
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteC101MDetail(C101MDetailModel model)
        {
            //整批交易管理
            BeginTransaction();
            try
            {
                TblAMGRP where = new TblAMGRP();
                where.grp_id = model.grp_id.TOInt32();
                Delete(where);
                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("DeleteC101MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 查詢群組
        /// </summary>
        public IList<C101MFuncmGridModel> QueryC101MSetGrid(C101MFuncmModel parm)
        {
            return base.QueryForListAll<C101MFuncmGridModel>("A6.queryC101MSetFuncm", parm);
        }

        /// <summary>
        /// 群組程式設定
        /// </summary>
        public void UpdateORAppendC101MSetGrid(IList<C101MFuncmGridModel> Grid)
        {
            base.BeginTransaction();
            try
            {
                foreach (C101MFuncmGridModel item in Grid)
                {
                    C101MFuncmGridModel where = new C101MFuncmGridModel
                    {
                        grp_id = item.grp_id,
                        sysid = item.sysid,
                        modules = item.modules,
                        submodules = item.submodules,
                        prgid = item.prgid
                    };
                    base.InsertOrUpdate<C101MFuncmGridModel>(item, where);
                }

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("UpdateORAppendC101SetGrid failed: " + ex.Message, ex);
            }
        }
        #endregion C101M

        //#region C102M

        ///// <summary>
        ///// 查詢帳號
        ///// </summary>
        //public IList<C102MGridModel> QueryC102MGrid(C102MFormModel parm)
        //{
        //    return base.QueryForList<C102MGridModel>("A6.queryC102MGrid", parm);
        //}

        ///// <summary>
        ///// 查詢帳號_匯出Excel
        ///// </summary>
        //public IList<C102MGridAllModel> QueryC102MGridAll(C102MFormModel parm)
        //{
        //    return base.QueryForListAll<C102MGridAllModel>("A6.queryC102MGridAll", parm);
        //}

        ///// <summary>
        ///// 查詢群組權限一覽_匯出Excel
        ///// </summary>
        //public IList<C102MGmapmGridModel> QueryC102MGmapmGrid(C102MFormModel parm)
        //{
        //    return base.QueryForListAll<C102MGmapmGridModel>("A6.queryC102MGmapmGrid", parm);
        //}

        ///// <summary>
        ///// 查詢帳號明細
        ///// </summary>
        //public C102MDetailModel QueryC102MDetail(string userno)
        //{
        //    Hashtable parm = new Hashtable();
        //    parm["userno"] = userno;
        //    return base.QueryForObject<C102MDetailModel>("A6.queryC102MDetail", parm);
        //}

        ///// <summary>
        ///// 查詢功能群組
        ///// </summary>
        //public IList<C102MGrpGridModel> QueryC102MGrp(C102MGrpModel parm)
        //{
        //    return base.QueryForListAll<C102MGrpGridModel>("A6.queryC102MGrp", parm);
        //}

        ///// <summary>
        ///// 檢查函數
        ///// </summary>
        ///// <param name="Grid"></param>
        ///// <returns></returns>
        //public string CheckC102M(C102MGrpModel model)
        //{
        //    // 全部都要檢核項目
        //    var GridY = model.Grid.Where(m => m.Is_Check).ToList();
        //    if (GridY.ToCount() == 0)
        //    {
        //        return "請至少勾選一筆群組資料後離開";
        //    }
        //    else
        //    {
        //        TblAMDBURM ad = new TblAMDBURM();
        //        ad.userno = model.userno;
        //        var addata = GetRow(ad);

        //        if (addata != null)
        //        {
        //            if (addata.unit_cd != "00" && addata.unit_cd != "01")
        //            {
        //                foreach (var item in GridY)
        //                {
        //                    TblAMGRP gp = new TblAMGRP();
        //                    gp.grp_id = item.grp_id;
        //                    var gpdata = GetRow(gp);

        //                    if (gpdata != null)
        //                    {
        //                        if (gpdata.unit_cd != addata.unit_cd)
        //                        {
        //                            return "選擇的群組必須與單位相符。";
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return "";
        //}

        ///// <summary>
        ///// 新增/修改 帳號檢核
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public string CheckC102M(C102MDetailModel model)
        //{
        //    string msg = "";

        //    // 帳號重複
        //    TblAMDBURM ad = new TblAMDBURM();
        //    ad.userno = model.userno;
        //    var addata = GetRow(ad);
        //    if (model.IsNew)
        //    {
        //        if (addata != null)
        //        {
        //            msg += "帳號重複，請重新輸入 ! \n";
        //        }

        //        if (model.userno.TONotNullString() == "")
        //        {
        //            msg += "請輸入帳號 ! \n";
        //        }
        //        else
        //        {
        //            if (model.userno.Length != model.userno.ToTrim().Length)
        //            {
        //                msg += "帳號不可填入空格，請重新輸入  ! \n";
        //            }
        //        }

        //    }

        //    // 20230217 信箱檢核收到回報 @mail.XXXXX.gov.tw 無法通過檢核，故將上限調整為5
        //    // 20230217 信箱檢核收到回報 @mail.XXXXXXX.gov.tw 無法通過檢核，故將上限調整為10
        //    //Regex mailreg = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        //    Regex mailreg = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
        //    if (!mailreg.IsMatch(model.email))
        //    {
        //        msg += "請填入正確的信箱格式  ! \n";
        //    }

        //    if (model.authdates.TOInt32() < model.modtime.TOInt32())
        //    {
        //        msg += "啟用日期不得小於申請日期  ! \n";
        //    }

        //    return msg;
        //}

        ///// <summary>
        ///// 新增帳號
        ///// </summary>
        ///// <param name="model"></param>
        //public void AppendC102MDetail(C102MDetailModel model)
        //{
        //    SessionModel sm = SessionModel.Get();
        //    //整批交易管理
        //    BeginTransaction();
        //    try
        //    {
        //        //新增 AMDBURM
        //        TblAMDBURM ad = new TblAMDBURM();
        //        ad.InjectFrom(model);
        //        ad.pwd = this.CypherText(model.userno);   //ad.pwd = this.EncPassword(model.userno);
        //        ad.del_mk = "N";
        //        ad.errct = 0;
        //        ad.modip = sm.UserInfo.LoginIP.TONotNullString();
        //        ad.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
        //        ad.moduserid = sm.UserInfo.UserNo.TONotNullString();
        //        ad.modusername = sm.UserInfo.User.username.TONotNullString();
        //        ad.del_mk = "N";
        //        //ad.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
        //        //ad.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
        //        //ad.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";
        //        Insert(ad);

        //        SendResetMail2(model, sm.UserInfo.LoginIP);

        //        CommitTransaction();
        //    }
        //    catch (Exception ex)
        //    {
        //        base.RollBackTransaction();
        //        throw new Exception("AppendC102MDetail failed:" + ex.Message, ex);
        //    }
        //}

        ///// <summary>
        ///// 更新帳號
        ///// </summary>
        ///// <param name="model"></param>
        //public void UpdateC102MDetail(C102MDetailModel model)
        //{
        //    //整批交易管理
        //    base.BeginTransaction();
        //    try
        //    {
        //        //更新 AMDBURM 資料表
        //        TblAMDBURM where = new TblAMDBURM();
        //        where.userno = model.userno;
        //        var getdata = GetRow(where);
        //        TblAMDBURM newdata = new TblAMDBURM();
        //        newdata.InjectFrom(model);
        //        newdata.authstatus = getdata.authstatus == "2" ? "2" : model.authstatus;
        //        //newdata.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
        //        //newdata.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
        //        //newdata.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";

        //        base.Update(newdata, where);
        //        base.CommitTransaction();
        //    }
        //    catch (Exception ex)
        //    {
        //        base.RollBackTransaction();
        //        throw new Exception("UpdateC102MDetail failed:" + ex.Message, ex);
        //    }
        //}

        ///// <summary>
        ///// 刪除帳號
        ///// </summary>
        ///// <param name="detail"></param>
        //public void DeleteC102MDetail(C102MDetailModel model)
        //{
        //    //整批交易管理
        //    BeginTransaction();
        //    try
        //    {
        //        TblAMDBURM where = new TblAMDBURM();
        //        where.userno = model.userno;
        //        Delete(where);
        //        CommitTransaction();
        //    }
        //    catch (Exception ex)
        //    {
        //        base.RollBackTransaction();
        //        throw new Exception("DeleteC102MDetail failed:" + ex.Message, ex);
        //    }
        //}

        ///// <summary>
        ///// 新增帳號
        ///// </summary>
        ///// <param name="model"></param>
        //public void SaveC102MGrp(C102MGrpModel model)
        //{
        //    SessionModel sm = SessionModel.Get();
        //    //整批交易管理
        //    BeginTransaction();
        //    try
        //    {
        //        // 刪除該帳號所有群組資料
        //        TblAMUROLE where = new TblAMUROLE();
        //        where.userno = model.userno;
        //        base.Delete(where);

        //        // 將勾選群組資料新增進入AMUROLE
        //        var GridY = model.Grid.Where(m => m.IsCheck == "1").ToList();
        //        foreach (var item in GridY)
        //        {
        //            TblAMUROLE newar = new TblAMUROLE();
        //            newar.userno = model.userno;
        //            newar.grp_id = item.grp_id.TOInt32();
        //            newar.modip = sm.UserInfo.LoginIP.TONotNullString();
        //            newar.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
        //            newar.moduser = sm.UserInfo.UserNo.TONotNullString();
        //            newar.modusername = sm.UserInfo.User.username.TONotNullString();

        //            base.Insert(newar);
        //        }

        //        CommitTransaction();
        //    }
        //    catch (Exception ex)
        //    {
        //        base.RollBackTransaction();
        //        throw new Exception("SaveC102MGrp failed:" + ex.Message, ex);
        //    }
        //}

        //#endregion

        #region C103M

        /// <summary>
        /// 查詢帳號
        /// </summary>
        public IList<C103MGridModel> QueryC103MGrid(C103MFormModel parm)
        {
            return base.QueryForList<C103MGridModel>("A6.queryC103MGrid", parm);
        }

        /// <summary>
        /// 查詢帳號_匯出Excel
        /// </summary>
        public IList<C103MGridAllModel> QueryC103MGridAll(C103MFormModel parm)
        {
            return base.QueryForListAll<C103MGridAllModel>("A6.queryC103MGridAll", parm);
        }

        /// <summary>
        /// 查詢群組權限一覽_匯出Excel
        /// </summary>
        public IList<C103MGmapmGridModel> QueryC103MGmapmGrid(C103MFormModel parm)
        {
            return base.QueryForListAll<C103MGmapmGridModel>("A6.queryC103MGmapmGrid", parm);
        }

        /// <summary>
        /// 查詢帳號明細
        /// </summary>
        public C103MDetailModel QueryC103MDetail(string userno)
        {
            Hashtable parm = new Hashtable();
            parm["userno"] = userno;
            return base.QueryForObject<C103MDetailModel>("A6.queryC103MDetail", parm);
        }

        /// <summary>
        /// 查詢功能群組
        /// </summary>
        public IList<C103MGrpGridModel> QueryC103MGrp(C103MGrpModel parm)
        {
            return base.QueryForListAll<C103MGrpGridModel>("A6.queryC103MGrp", parm);
        }

        /// <summary>
        /// 檢查函數
        /// </summary>
        /// <param name="Grid"></param>
        /// <returns></returns>
        public string CheckC103M(C103MGrpModel model)
        {
            // 全部都要檢核項目
            var GridY = model.Grid.Where(m => m.Is_Check).ToList();
            if (GridY.ToCount() == 0)
            {
                return "請至少勾選一筆群組資料後離開";
            }
            else
            {
                TblAMDBURM ad = new TblAMDBURM();
                ad.userno = model.userno;
                var addata = GetRow(ad);

                if (addata != null)
                {
                    if (addata.unit_cd != "00" && addata.unit_cd != "01")
                    {
                        foreach (var item in GridY)
                        {
                            TblAMGRP gp = new TblAMGRP();
                            gp.grp_id = item.grp_id;
                            var gpdata = GetRow(gp);

                            if (gpdata != null)
                            {
                                if (gpdata.unit_cd != addata.unit_cd)
                                {
                                    return "選擇的群組必須與單位相符。";
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 新增/修改 帳號檢核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckC103M(C103MDetailModel model)
        {
            string msg = "";

            // 帳號重複
            TblAMDBURM ad = new TblAMDBURM();
            ad.userno = model.userno;
            var addata = GetRow(ad);
            if (model.IsNew)
            {
                if (addata != null)
                {
                    msg += "帳號重複，請重新輸入 ! \n";
                }

                if (model.userno.TONotNullString() == "")
                {
                    msg += "請輸入帳號 ! \n";
                }
                else
                {
                    if (model.userno.Length != model.userno.ToTrim().Length)
                    {
                        msg += "帳號不可填入空格，請重新輸入  ! \n";
                    }
                }

            }

            // 20230217 信箱檢核收到回報 @mail.XXXXX.gov.tw 無法通過檢核，故將上限調整為5
            // 20230217 信箱檢核收到回報 @mail.XXXXXXX.gov.tw 無法通過檢核，故將上限調整為10
            //Regex mailreg = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Regex mailreg = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            if (!mailreg.IsMatch(model.email))
            {
                msg += "請填入正確的信箱格式  ! \n";
            }

            if (model.authdates.TOInt32() < model.modtime.TOInt32())
            {
                msg += "啟用日期不得小於申請日期  ! \n";
            }

            return msg;
        }

        /// <summary>
        /// 新增帳號
        /// </summary>
        /// <param name="model"></param>
        public void AppendC103MDetail(C103MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                //新增 AMDBURM
                TblAMDBURM ad = new TblAMDBURM();
                ad.InjectFrom(model);
                ad.pwd = this.CypherText(model.userno);   //ad.pwd = this.EncPassword(model.userno);
                ad.del_mk = "N";
                ad.errct = 0;
                ad.modip = sm.UserInfo.LoginIP.TONotNullString();
                ad.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                ad.moduserid = sm.UserInfo.UserNo.TONotNullString();
                ad.modusername = sm.UserInfo.User.username.TONotNullString();
                ad.del_mk = "N";
                //ad.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
                //ad.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
                //ad.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";
                Insert(ad);

                SendResetMail(model, sm.UserInfo.LoginIP);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("AppendC103MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 更新帳號
        /// </summary>
        /// <param name="model"></param>
        public void UpdateC103MDetail(C103MDetailModel model)
        {
            //整批交易管理
            base.BeginTransaction();
            try
            {
                //更新 AMDBURM 資料表
                TblAMDBURM where = new TblAMDBURM();
                where.userno = model.userno;
                var getdata = GetRow(where);
                TblAMDBURM newdata = new TblAMDBURM();
                newdata.InjectFrom(model);
                newdata.authstatus = getdata.authstatus == "2" ? "2" : model.authstatus;
                //newdata.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
                //newdata.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
                //newdata.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";

                base.Update(newdata, where);
                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("UpdateC103MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 刪除帳號
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteC103MDetail(C103MDetailModel model)
        {
            //整批交易管理
            BeginTransaction();
            try
            {
                TblAMDBURM where = new TblAMDBURM();
                where.userno = model.userno;
                Delete(where);
                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("DeleteC103MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 新增帳號
        /// </summary>
        /// <param name="model"></param>
        public void SaveC103MGrp(C103MGrpModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                // 刪除該帳號所有群組資料
                TblAMUROLE where = new TblAMUROLE();
                where.userno = model.userno;
                base.Delete(where);

                // 將勾選群組資料新增進入AMUROLE
                var GridY = model.Grid.Where(m => m.IsCheck == "1").ToList();
                foreach (var item in GridY)
                {
                    TblAMUROLE newar = new TblAMUROLE();
                    newar.userno = model.userno;
                    newar.grp_id = item.grp_id.TOInt32();
                    newar.modip = sm.UserInfo.LoginIP.TONotNullString();
                    newar.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                    newar.moduser = sm.UserInfo.UserNo.TONotNullString();
                    newar.modusername = sm.UserInfo.User.username.TONotNullString();

                    base.Insert(newar);
                }

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("SaveC103MGrp failed:" + ex.Message, ex);
            }
        }

        #endregion

        /// <summary>
        /// 寄出驗證信
        /// </summary>
        /// <param name="detail"></param>
        public void SendResetMail(C103MDetailModel model, string IP)
        {
            SessionModel sm = SessionModel.Get();
            BeginTransaction();
            try
            {
                // 帳號基本資料
                TblAMDBURM ad = new TblAMDBURM();
                ad.userno = model.userno;
                var addata = GetRow(ad);

                // 產出驗證碼
                var gd = Guid.NewGuid().ToString("N");

                // 紀錄驗證碼至資料表比對
                TblAMCHANGEPWD_GUID ag = new TblAMCHANGEPWD_GUID();
                ag.userno = model.userno;
                ag.guid = gd;
                ag.guidyn = "N";
                ag.modip = IP;
                ag.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                ag.moduserid = addata.userno.TONotNullString();
                ag.modusername = addata.username.TONotNullString();
                Insert(ag);

                //查詢網站網址
                string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                Uri uri = new Uri(webUrl);
                string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
                string appDocUrl = string.Format("{0}/Login/C101M/PasswordChange?Guid={1}", webDomain, gd);

                // 信件內容
                var body = addata.username + "您好：<br>";
                body += "我們已收到您衛生福利部民眾線上申請電子病歷服務平台管理系統的帳號申請。<br>";
                body += "開啟以下密碼重設連結，並依照螢幕上的指示重設您的密碼。<br>";
                body += "密碼重設連結：<br>";
                body += "<a target='_blank' href='" + appDocUrl + "'>" + appDocUrl + "</a><br><br>";
                body += "PS.這封電子郵件是由伺服器自動發送。請勿回覆。<br>";
                body += "如果您不是此電子郵件的指定收件人，請刪除此訊息。<br>";
                body += "<br><br><br><br>";
                body += "衛福部民眾線上申請電子病歷服務平台管理系統";

                // 寄信
                MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, model.email, "衛福部民眾線上申請電子病歷服務平台管理系統-申辦帳號通知信件", body);
                mailMessage.IsBodyHtml = true;
                var t = CommonsServices.SendMail(mailMessage);

                // 寄信LOG
                TblAMEMAILLOG_EMAIL maillog = new TblAMEMAILLOG_EMAIL();
                maillog.eservice_id = "Login";
                maillog.subject = "衛福部民眾線上申請電子病歷服務平台管理系統-申辦帳號通知信件";
                maillog.body = t.IsSuccess == true ? body : t.ResultText.TONotNullString();
                maillog.send_time = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                maillog.mail = model.email;
                maillog.mail_type = "1";
                maillog.modip = IP;
                maillog.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                maillog.moduser = addata.userno.TONotNullString();
                maillog.modusername = addata.username.TONotNullString();
                maillog.status = t.IsSuccess == true ? "1" : "0";

                Insert(maillog);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("SendResetMail failed:" + ex.Message, ex);
            }
        }

        #region C102M

        /// <summary>
        /// 查詢群組
        /// </summary>
        public IList<C102MGridModel> QueryC102MGrid(C102MFormModel parm)
        {
            return base.QueryForList<C102MGridModel>("A6.queryC102MGrid", parm);
        }

        #endregion C102M

        #region C104M

        public IList<C104MGridModel> QueryC104M(C104MFormModel parm)
        {
            return base.QueryForListAll<C104MGridModel>("A6.queryC104M", parm);
        }

        public C104MDetailModel QueryC104MDetail(string keyid)
        {
            Hashtable parm = new Hashtable();
            parm["keyid"] = keyid;
            return base.QueryForObject<C104MDetailModel>("A6.queryC104MDetail", parm);
        }

        #endregion C104M
    }
}