using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using EECOnline.Areas.SHARE.Models;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using EECOnline.Services;
using System.Collections;
using EECOnline.Models;
using System.IO;
using Turbo.Commons;
using System.Net.Mail;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace EECOnline.DataLayers
{
    public class SHAREDAO : BaseDAO
    {
        #region ZIP_CO

        /// <summary>
        /// 查詢 ZIP_CO
        /// </summary>
        /// <param name="detail"></param>
        public IList<ZIP_COGridModel> QueryZIP_CO(ZIP_COFormModel parms)
        {
            return base.QueryForList<ZIP_COGridModel>("SHARE.queryZIP_CO", parms);
        }

        /// <summary>
        /// 查詢 ZIP_CO
        /// </summary>
        /// <param name="detail"></param>
        public Hashtable GetZip3Detail(Hashtable parms)
        {
            return base.QueryForObject<Hashtable>("SHARE.getZip3All", parms);
        }

        /// <summary>
        /// 查詢 ZIP_CO
        /// </summary>
        /// <param name="detail"></param>
        public Hashtable GetZipDetail(Hashtable parms)
        {
            return base.QueryForObject<Hashtable>("SHARE.getZipAll", parms);
        }

        /// <summary>
        /// 查詢 ZIP_CO
        /// </summary>
        /// <param name="detail"></param>
        public Hashtable QueryZIP_CO_Three(Hashtable parms)
        {
            return base.QueryForObject<Hashtable>("SHARE.queryZIP_CO_Three", parms);
        }
        #endregion

        #region UNIT
        /// <summary>
        /// 查詢 UNIT
        /// </summary>
        /// <param name="parms"></param>
        public IList<UNITGridModel> QueryUNIT(UNITFormModel parms)
        {
            return base.QueryForList<UNITGridModel>("SHARE.queryUNIT", parms);
        }

        #endregion

        #region OPERAT
        /// <summary>
        /// 查詢 OPERAT
        /// </summary>
        /// <param name="parms"></param>
        public IList<OPERATGridModel> QueryOPERAT(OPERATFormModel parms)
        {
            return base.QueryForList<OPERATGridModel>("SHARE.queryOPERAT", parms);
        }

        #endregion

        #region GRP
        /// <summary>
        /// 查詢 GRP
        /// </summary>
        /// <param name="parms"></param>
        public IList<GRPGridModel> QueryGRP(GRPFormModel parms)
        {
            return base.QueryForList<GRPGridModel>("SHARE.queryGRP", parms);
        }

        #endregion

        #region 上傳檔案

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="srv_id"></param>
        /// <param name="uploadfile"></param>
        /// <param name="serNum"></param>
        /// <returns></returns>
        public string PutFile(string srv_id, HttpPostedFileBase uploadfile, string serNum, string other = null)
        {
            try
            {
                // 取得亂數 避免重複檔名
                var fileName = "";
                var getGUID = Guid.NewGuid();
                var guidItem = getGUID.ToString().ToSplit('-');
                if (other != null)
                {
                    fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + "_" + other + Path.GetExtension(uploadfile.FileName);
                }
                else
                {
                    fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + Path.GetExtension(uploadfile.FileName);
                }


                // 檔案路徑
                var localPath = GetServerLocalPath("APPLY_FILE"); ;
                TblSETUP st = new TblSETUP();
                st.setup_cd = "APPLY_FILE";
                var stdata = GetRow(st);

                DirectoryInfo dir = new DirectoryInfo(localPath);
                if (!dir.Exists) { dir.Create(); }
                FileStream fs = File.Open(localPath + fileName, FileMode.Append);
                MemoryStream target = new MemoryStream();
                uploadfile.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Close();

                return stdata.setup_val + "\\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("MM") + "\\" + fileName;
            }
            catch (Exception ex)
            {
                LOG.Error("申辦案件檔案上傳失敗(PutFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("申辦案件檔案上傳失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="srv_id"></param>
        /// <param name="uploadfile"></param>
        /// <param name="serNum"></param>
        /// <returns></returns>
        public string PutFile(string srv_id, HttpPostedFileBase uploadfile, string serNum, int key, string other = null)
        {
            try
            {
                var fileName = "";
                // 取得亂數 避免重複檔名
                var getGUID = Guid.NewGuid();
                var guidItem = getGUID.ToString().ToSplit('-');
                if (other != null)
                {
                    fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + "-" + key.TONotNullString() + "_" + other + Path.GetExtension(uploadfile.FileName);
                }
                else
                {
                    fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + "-" + key.TONotNullString() + Path.GetExtension(uploadfile.FileName);
                }

                // 檔案路徑
                var localPath = GetServerLocalPath("APPLY_FILE");
                TblSETUP st = new TblSETUP();
                st.setup_cd = "APPLY_FILE";
                var stdata = GetRow(st);

                DirectoryInfo dir = new DirectoryInfo(localPath);
                if (!dir.Exists) { dir.Create(); }
                FileStream fs = File.Open(localPath + fileName, FileMode.Append);
                MemoryStream target = new MemoryStream();
                uploadfile.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Close();

                return stdata.setup_val + "\\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("MM") + "\\" + fileName;
            }
            catch (Exception ex)
            {
                LOG.Error("申辦案件檔案上傳失敗(PutFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("申辦案件檔案上傳失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 取得前台案件申辦主機檔案儲存位置
        /// </summary>
        public string GetServerLocalPath(string setup_cd)
        {
            var localPath = "";
            try
            {
                TblSETUP st = new TblSETUP();
                st.setup_cd = setup_cd;
                var stdata = GetRow(st);

                switch (setup_cd)
                {
                    case "APPLY_FILE":
                        localPath += stdata.setup_val + "/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM") + "/";
                        break;
                    case "OnlineHelp":
                        localPath += stdata.setup_val;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LOG.Error("取得前台案件申辦主機檔案儲存位置，原因：" + ex.Message, ex);
                throw new Exception(string.Format("取得前台案件申辦主機檔案儲存位置，原因：{0}", ex.Message));
            }
            return localPath;
        }

        #endregion

        #region 取得SETUP共用代碼
        /// <summary>
        /// 取得SETUP共用代碼
        /// </summary>
        /// <param name="parms"></param>
        public static string GetSetup(string setup_cd)
        {
            SHAREDAO dao = new SHAREDAO();
            TblSETUP where = new TblSETUP();
            where.setup_cd = setup_cd;
            var setup = dao.GetRow(where);
            var setupVal = (setup == null) ? "" : setup.setup_val;
            return setupVal;
        }

        #endregion

        #region API
        public void ModelSetLOG_Info1(Dictionary<string, string> model, string Check = "")
        {
            var msg = "Info[" + Check + "]";

            msg += "pi>[";
            foreach (var p in model)
            {
                msg += p.Key + ":" + p.Value + ",";
            }
            msg += "]";

            LOG.Info(msg);
        }
        #endregion
    }
}