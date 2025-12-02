using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Areas.A7.Models;
using EECOnline.Services;
using EECOnline.Commons;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using System;
using Turbo.Commons;

namespace EECOnline.DataLayers
{
    public class A7DAO : BaseDAO
    {
        #region C101M

        /// <summary>
        /// 查詢最新消息維護
        /// </summary>
        public IList<C101MGridModel> QueryC101MGrid(C101MFormModel parm)
        {
            var list = base.QueryForList<C101MGridModel>("A7.queryC101MGrid", parm);
            for (int i = 0; i < list.ToCount(); i++)
            {
                var today = DateTime.Today.ToString("yyyyMMdd").ToString();
                var start = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(list[i].showdates, ""), "/");
                var end = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(list[i].showdatee, ""), "/");
                list[i].showdate = start + "~" + end;
                list[i].showdates = start;
                list[i].showdatee = end;

            }

            return list;
        }

        /// <summary>
        /// 新增最新消息檢核
        /// </summary>
        /// <param name="parm"></param>
        public string CheckC101M(C101MDetailModel model)
        {
            string msg = "";

            //// 啟用狀態(防止F12)
            //if (string.IsNullOrEmpty(model.grp_status))
            //{
            //    msg = "請選擇類別狀態 !";
            //}

            // 名稱檢核
            //TblENEWS en = new TblENEWS();
            //en.enews_id = model.enews_id;
            //var endata = GetRow(en);

            //if (endata != null)
            //{
            //    if (endata.enews_id != model.enews_id)
            //    {
            //        msg = "公告名稱重複，請重新輸入 !";
            //    }
            //}

            if (HelperUtil.TransToDateTime(model.showdates, "") > HelperUtil.TransToDateTime(model.showdatee, ""))
            {
                msg +=  "上架日期起不得大於上架日期迄 ！\n";
            }


            return msg;
        }

        /// <summary>
        /// 新增公告維護
        /// </summary>
        /// <param name="detail"></param>
        public void AppendC101MDetail(C101MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                //新增 ENEWS
                TblENEWS en = new TblENEWS();
                en.InjectFrom(model);
                en.modip = sm.UserInfo.LoginIP.TONotNullString();
                en.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                en.modusername = sm.UserInfo.User.username.TONotNullString();
                en.moduser = sm.UserInfo.UserNo.TONotNullString();
                en.del_mk = "N";
                en.newstype = model.enews;
                model.enews_id = Insert(en).TONotNullString();

                //更新 檔案列表
                TblEFILE filedata = new TblEFILE();
                model.Upload.peky1 = "ENEWS";
                model.Upload.peky2 = model.enews_id;
                model.Upload.SaveFileGrid();

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_AppendC101MDetail failed :" + ex.TONotNullString());
                throw new Exception("AppendC101MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 公告
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateC101MDetail(C101MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            base.BeginTransaction();
            try
            {
                //更新 ENEWS 資料表
                TblENEWS where = new TblENEWS();
                where.enews_id = model.enews_id.TOInt32();
                TblENEWS newdata = new TblENEWS();
                newdata.InjectFrom(model);
                newdata.modip = sm.UserInfo.LoginIP.TONotNullString();
                newdata.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                newdata.modusername = sm.UserInfo.User.username.TONotNullString();
                newdata.moduser = sm.UserInfo.UserNo.TONotNullString();

                base.Update(newdata, where);

                //更新 檔案列表
                TblEFILE filedata = new TblEFILE();
                model.Upload.peky1 = "ENEWS";
                model.Upload.peky2 = model.enews_id;
                model.Upload.SaveFileGrid();

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_UpdateC101MDetail failed :" + ex.TONotNullString());
                throw new Exception("UpdateC101MDetail failed:" + ex.Message, ex);
            }
        }


        /// <summary>
        /// 刪除/ 公告
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteC101MDetail(C101MDetailModel model)
        {
            //整批交易管理
            BeginTransaction();
            try
            {
                TblENEWS where = new TblENEWS();
                where.enews_id = model.enews_id.TOInt32();
                Delete(where);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_DeleteC101MDetail failed :" + ex.TONotNullString());
                throw new Exception("DeleteC101MDetail failed:" + ex.Message, ex);
            }
        }

        #endregion C101M

        #region C102M

        /// <summary>
        /// 查詢友善連結類別
        /// </summary>
        public IList<C102MGridModel> QueryC102MGrid(C102MFormModel parm)
        {
            return base.QueryForList<C102MGridModel>("A7.queryC102MGrid", parm);
        }

        /// <summary>
        /// 新增友善類別檢核
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckC102M(C102MDetailModel model)
        {
            string msg = "";

            if (model.Upload.fileGrid.ToCount()<1)
            {
                msg = "請上傳檔案 ! \n";
            }

            // 名稱檢核
            TblELINKS el = new TblELINKS();
            el.title = model.title;
            var eldata = GetRow(el);

            if (eldata != null)
            {
                if (eldata.title != model.title)
                {
                    msg = "連結標題名稱重複，請重新輸入 !";
                }
            }

            return msg;
        }

        /// <summary>
        /// 新增友善連結類別
        /// </summary>
        /// <param name="detail"></param>
        public void AppendC102MDetail(C102MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                //新增 ELINKS
                TblELINKS el = new TblELINKS();
                el.InjectFrom(model);
                el.moduser = sm.UserInfo.UserNo.TONotNullString();
                el.modip = sm.UserInfo.LoginIP.TONotNullString();
                el.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                el.modusername = sm.UserInfo.User.username.TONotNullString();
                el.del_mk = "N";
                model.elinks_id = Insert(el).TONotNullString();

                //更新 檔案列表
                TblEFILE filedata = new TblEFILE();
                model.Upload.peky1 = "ELINKS";
                model.Upload.peky2 = model.elinks_id;
                model.Upload.SaveFileGrid();

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_AppendC102MDetail failed :" + ex.TONotNullString());
                throw new Exception("AppendC102MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 編輯友善連結類別
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateC102MDetail(C102MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            base.BeginTransaction();
            try
            {
                //更新 ELINKS 資料表
                TblELINKS where = new TblELINKS();
                where.elinks_id = model.elinks_id.TOInt32();
                TblELINKS newdata = new TblELINKS();
                newdata.InjectFrom(model);
                newdata.modip = sm.UserInfo.LoginIP.TONotNullString();
                newdata.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                newdata.modusername = sm.UserInfo.User.username.TONotNullString();
                newdata.moduser = sm.UserInfo.UserNo.TONotNullString();

                base.Update(newdata, where);

                //更新 檔案列表
                TblEFILE filedata = new TblEFILE();
                model.Upload.peky1 = "ELINKS";
                model.Upload.peky2 = model.elinks_id;
                model.Upload.SaveFileGrid();

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_UpdateC102MDetail failed :" + ex.TONotNullString());
                throw new Exception("UpdateC102MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 刪除/ 公告
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteC102MDetail(C102MDetailModel model)
        {
            //整批交易管理
            BeginTransaction();
            try
            {
                TblELINKS where = new TblELINKS();
                where.elinks_id = model.elinks_id.TOInt32();
                Delete(where);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_DeleteC102MDetail failed :" + ex.TONotNullString());
                throw new Exception("DeleteC102MDetail failed:" + ex.Message, ex);
            }
        }

        #endregion C102M

        #region C103M

        /// <summary>
        /// 查詢聯絡我們
        /// </summary>
        public IList<C103MGridModel> QueryC103MGrid(C103MFormModel parm)
        {
            return base.QueryForList<C103MGridModel>("A7.queryC103MGrid", parm);
        }

        /// <summary>
        /// 新增聯絡我們
        /// </summary>
        /// <param name="detail"></param>
        public void AppendC103MDetail(C103MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                //新增 
                TblCONTACT con = new TblCONTACT();
                con.InjectFrom(model);
                con.con_cd = con.con_name;
                con.con_name = model.con_name_list.Where(m=>m.Value == model.con_name).FirstOrDefault().Text;
                con.moduser = sm.UserInfo.UserNo.TONotNullString();
                con.modip = sm.UserInfo.LoginIP.TONotNullString();
                con.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                con.modusername = sm.UserInfo.User.username.TONotNullString();
                con.del_mk = "N";
                

                Insert(con);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_AppendC103MDetail failed :" + ex.TONotNullString());
                throw new Exception("AppendC103MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 聯絡檢核
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckC103M(C103MDetailModel model)
        {
            string msg = "";

            // 防止PK遺失整批更新狀況
            if (!model.IsNew)
            {
                if (string.IsNullOrEmpty(model.con_id.ToString()))
                {
                    msg = "更新失敗，請聯絡系統管理員 !";
                }
            }
            return msg;
        }

        /// <summary>
        /// 更新聯絡我們
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateC103MDetail(C103MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            base.BeginTransaction();
            try
            {
                //更新 CONTACT
                TblCONTACT where = new TblCONTACT();
                where.con_id = model.con_id.TOInt32();
                TblCONTACT newdata = new TblCONTACT();
                newdata.InjectFrom(model);
                newdata.con_name = model.con_name_list.Where(m => m.Value == model.con_name).FirstOrDefault().Text;
                newdata.modip = sm.UserInfo.LoginIP.TONotNullString();
                newdata.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                newdata.modusername = sm.UserInfo.User.username.TONotNullString();
                newdata.moduser = sm.UserInfo.UserNo.TONotNullString();

                base.Update(newdata, where);
                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_UpdateC103MDetail failed :" + ex.TONotNullString());
                throw new Exception("UpdateC103MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 刪除 常見問題
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteC103MDetail(C103MDetailModel model)
        {
            //整批交易管理
            BeginTransaction();
            try
            {
                TblCONTACT where = new TblCONTACT();
                where.con_id = model.con_id.TOInt32();
                Delete(where);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_DeleteC103MDetail failed :" + ex.TONotNullString());
                throw new Exception("DeleteC103MDetail failed:" + ex.Message, ex);
            }
        }
        #endregion C103M

        #region C104M

        /// <summary>
        /// 查詢常見問題
        /// </summary>
        public IList<C104MGridModel> QueryC104MGrid(C104MFormModel parm)
        {
            return base.QueryForList<C104MGridModel>("A7.queryC104MGrid", parm);
        }

        /// <summary>
        /// 新增常見問題檢核
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string CheckC104M(C104MDetailModel model)
        {
            string msg = "";

            //// 啟用狀態(防止F12)
            //if (string.IsNullOrEmpty(model.grp_status))
            //{
            //    msg = "請選擇類別狀態 !";
            //}

            // 名稱檢核
            TblEFAQ faq = new TblEFAQ();
            faq.question = model.question;
            var faqdata = GetRow(faq);

            if (faqdata != null)
            {
                if (faqdata.question != model.question)
                {
                    msg = "問題主旨名稱重複，請重新輸入 !";
                }
            }

            return msg;
        }

        /// <summary>
        /// 新增常見問題
        /// </summary>
        /// <param name="detail"></param>
        public void AppendC104MDetail(C104MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                //新增 EFAQ 資料表
                TblEFAQ faq = new TblEFAQ();
                faq.InjectFrom(model);
                faq.modip = sm.UserInfo.LoginIP.TONotNullString();
                faq.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                faq.modusername = sm.UserInfo.User.username.TONotNullString();
                faq.moduser = sm.UserInfo.UserNo.TONotNullString();
                faq.del_mk = "N";
                faq.faqtype = model.code_name;
                Insert(faq);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_AppendC104MDetail failed :" + ex.TONotNullString());
                throw new Exception("AppendC104MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 編輯常見問題
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateC104MDetail(C104MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            base.BeginTransaction();
            try
            {
                //更新 EFAQ 資料表
                TblEFAQ where = new TblEFAQ();
                where.efaq_id = model.efaq_id.TOInt32();
                TblEFAQ newdata = new TblEFAQ();
                newdata.InjectFrom(model);
                newdata.faqtype = model.code_name;
                newdata.modip = sm.UserInfo.LoginIP.TONotNullString();
                newdata.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                newdata.modusername = sm.UserInfo.User.username.TONotNullString();
                newdata.moduser = sm.UserInfo.UserNo.TONotNullString();

                base.Update(newdata, where);
                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_UpdateC104MDetail failed :" + ex.TONotNullString());
                throw new Exception("UpdateC104MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 刪除 常見問題
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteC104MDetail(C104MDetailModel model)
        {
            //整批交易管理
            BeginTransaction();
            try
            {
                TblEFAQ where = new TblEFAQ();
                where.efaq_id = model.efaq_id.TOInt32();
                Delete(where);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("A7_DeleteC104MDetail failed :" + ex.TONotNullString());
                throw new Exception("DeleteC104MDetail failed:" + ex.Message, ex);
            }
        }

        #endregion C104M
    }
}