using ES.Action;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ES.Utils;
using log4net;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ES.DataLayers
{
    public class ApplyDonateDAO : BaseAction
    {
        protected static readonly ILog logger = LogUtils.GetLogger();

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

        public List<ApplyDonateGridModel> GetApplyDonateGrid()
        {
            var result = new List<ApplyDonateGridModel>();
            // 所有賑災專戶(未刪除)
            TblAPPLY_DONATE where = new TblAPPLY_DONATE();
            where.DEL_MK = "N";
            var list = this.GetRowList<TblAPPLY_DONATE>(where);
            foreach (var item in list)
            {
                var data = new ApplyDonateGridModel();
                data.InjectFrom(item);
                var DonateAmt = 0;
                // 申請案件
                ApplyModel where07 = new ApplyModel();
                where07.SRV_ID = item.SRV_ID_DONATE;
                var appList07 = this.GetRowList<ApplyModel>(where07);
                if (appList07 != null)
                {
                    foreach (var app in appList07)
                    {
                        // 繳費資料
                        APPLY_PAY where07_pay = new APPLY_PAY();
                        where07_pay.APP_ID = app.APP_ID;
                        where07_pay.PAY_STATUS_MK = "Y";
                        var appPay = this.GetRow<APPLY_PAY>(where07_pay);
                        if (appPay != null)
                        {
                            DonateAmt += appPay.PAY_MONEY.TOInt32();
                        }
                    }
                }
                data.DonateAmt = DonateAmt.ToString();
                result.Add(data);
            }

            return result;
        }
        public List<ApplyDonateGridDetailModel> GetApplyDonateGridDetail(string srv_id)
        {
            List<ApplyDonateGridDetailModel> result = new List<ApplyDonateGridDetailModel>();
            // 取得捐款專案下所有申請案件
            ApplyModel where = new ApplyModel();
            where.SRV_ID = srv_id;
            var data = this.GetRowList<ApplyModel>(where);
            if (data != null && data.Count > 0)
            {
                var i = 1;
                // 取得案件編號
                foreach (var item in data)
                {
                    ApplyDonateGridDetailModel insertData = new ApplyDonateGridDetailModel();
                    // 取得繳費資料
                    APPLY_PAY wherePay = new APPLY_PAY();
                    wherePay.APP_ID = item.APP_ID;
                    wherePay.PAY_STATUS_MK = "Y";
                    var dataPay = this.GetRow<APPLY_PAY>(wherePay);
                    if (dataPay != null)
                    {
                        insertData.InjectFrom(dataPay);
                    }
                    else
                    {
                        insertData.PAY_MONEY = 0;
                    }
                    // 基本資料
                    insertData.APP_ID = item.APP_ID;
                    insertData.D_NAME = item.NAME;
                    insertData.REC_NO = "";
                    insertData.SEQ = i.ToString();
                    insertData.D_USE = "";
                    insertData.D_DUSE = "";
                    insertData.D_REMARK = "";

                    result.Add(insertData);
                    i++;
                }
            }

            return result;
        }
        /// <summary>
        /// 檢核捐款專案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckDonate(ApplyDonateViewModel model)
        {
            var ErrorMsg = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(model.Detail.NAME_CH))
                {
                    ErrorMsg += "請輸入賑災專戶名稱(中文)\n";
                }
                if (string.IsNullOrWhiteSpace(model.Detail.NAME_ENG))
                {
                    ErrorMsg += "請輸入賑災專戶名稱(英文)\n";
                }
                if (model.Detail.START_DATE == null)
                {
                    ErrorMsg += "請輸入起始日\n";
                }
                if (model.Detail.END_DATE == null)
                {
                    ErrorMsg += "請輸入結束日\n";
                }
                if (model.Detail.START_DATE != null && model.Detail.END_DATE != null)
                {
                    if (Convert.ToDateTime(model.Detail.START_DATE) > Convert.ToDateTime(model.Detail.END_DATE))
                    {
                        ErrorMsg += "起始日請勿大於結束日\n";
                    }
                }
                if (model.Detail.PAY_WAY_C == false && model.Detail.PAY_WAY_S == false && model.Detail.PAY_WAY_T == false)
                {
                    ErrorMsg += "請輸入捐款方式\n";
                }
                if (string.IsNullOrWhiteSpace(model.Detail.BANK_NAME))
                {
                    ErrorMsg += "請輸入戶頭名稱\n";
                }
                if (string.IsNullOrWhiteSpace(model.Detail.BANK_CODE))
                {
                    ErrorMsg += "請輸入銀行代碼\n";
                }
                if (string.IsNullOrWhiteSpace(model.Detail.BANK_ACCOUNT))
                {
                    ErrorMsg += "請輸入銀行帳號\n";
                }
                if (string.IsNullOrWhiteSpace(model.Detail.DESC_CH))
                {
                    ErrorMsg += "請輸入專戶說明(中文)\n";
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
                throw new Exception("CheckDonate failed:" + ex.Message, ex);
            }
            return ErrorMsg;
        }
        /// <summary>
        /// 新增捐款專案
        /// </summary>
        /// <param name="model"></param>
        /// <param name="max_id"></param>
        /// <param name="account"></param>
        /// <param name="isDraft"></param>
        /// <returns></returns>
        public string AppendDonate(ApplyDonateViewModel model, string max_id, string account, bool isDraft = false)
        {
            var ErrorMsg = string.Empty;
            var max_int = Convert.ToInt32(max_id);
            var new_srv_id = (max_int + 1).ToString().PadLeft(6, '0');
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 新增 更新
                    if (isDraft)
                    {
                        TblAPPLY_DONATE insertData = new TblAPPLY_DONATE();
                        insertData.SRV_ID_DONATE = new_srv_id;
                        insertData.NAME_CH = model.Detail.NAME_CH;
                        insertData.NAME_ENG = model.Detail.NAME_ENG;
                        insertData.START_DATE = HelperUtil.TransToDateTime(model.Detail.START_DATE);
                        insertData.END_DATE = HelperUtil.TransToDateTime(model.Detail.END_DATE);
                        var payStrA = model.Detail.PAY_WAY_C ? "C" : "";
                        var payStrB = model.Detail.PAY_WAY_S ? "S" : "";
                        var payStrC = model.Detail.PAY_WAY_T ? "T" : "";
                        var payStrD = model.Detail.PAY_WAY_L ? "L" : "";
                        insertData.PAY_WAY = $"{payStrA}{payStrB}{payStrC}{payStrD}";
                        insertData.BANK_CODE = model.Detail.BANK_CODE;
                        insertData.BANK_ACCOUNT = model.Detail.BANK_ACCOUNT;
                        insertData.BANK_NAME = model.Detail.BANK_NAME;
                        insertData.DESC_CH = model.Detail.DESC_CH;
                        insertData.DESC_ENG = model.Detail.DESC_ENG;
                        insertData.ISDRAFT = isDraft ? "Y" : "N";
                        insertData.ISOPEN = "N";
                        var dt = DateTime.Now;
                        insertData.UPD_TIME = dt;
                        insertData.UPD_FUN_CD = "ADM-DONATE";
                        insertData.UPD_ACC = account;
                        insertData.ADD_TIME = dt;
                        insertData.ADD_FUN_CD = "ADM-DONATE";
                        insertData.ADD_ACC = account;
                        insertData.DEL_MK = "N";
                        Insert<TblAPPLY_DONATE>(insertData);
                    }
                    else
                    {
                        if (model.Detail.SRV_ID_DONATE == null)
                        {
                            TblAPPLY_DONATE where = new TblAPPLY_DONATE();
                            where.SRV_ID_DONATE = new_srv_id;
                            where.NAME_CH = model.Detail.NAME_CH;
                            where.NAME_ENG = model.Detail.NAME_ENG;
                            where.START_DATE = HelperUtil.TransToDateTime(model.Detail.START_DATE);
                            where.END_DATE = HelperUtil.TransToDateTime(model.Detail.END_DATE);
                            var payStrA = model.Detail.PAY_WAY_C ? "C" : "";
                            var payStrB = model.Detail.PAY_WAY_S ? "S" : "";
                            var payStrC = model.Detail.PAY_WAY_T ? "T" : "";
                            var payStrD = model.Detail.PAY_WAY_L ? "L" : "";
                            where.PAY_WAY = $"{payStrA}{payStrB}{payStrC}{payStrD}";
                            where.BANK_CODE = model.Detail.BANK_CODE;
                            where.BANK_ACCOUNT = model.Detail.BANK_ACCOUNT;
                            where.BANK_NAME = model.Detail.BANK_NAME;
                            where.DESC_CH = model.Detail.DESC_CH;
                            where.DESC_ENG = model.Detail.DESC_ENG;
                            where.ISDRAFT = isDraft ? "Y" : "N";
                            where.ISOPEN = model.Detail.ISOPEN;
                            var dt = DateTime.Now;
                            where.UPD_TIME = dt;
                            where.UPD_FUN_CD = "ADM-DONATE";
                            where.UPD_ACC = account;
                            where.DEL_MK = "N";
                            Insert<TblAPPLY_DONATE>(where);
                        }
                        else
                        {
                            TblAPPLY_DONATE where = new TblAPPLY_DONATE();
                            where.SRV_ID_DONATE = model.Detail.SRV_ID_DONATE;
                            TblAPPLY_DONATE updateData = new TblAPPLY_DONATE();
                            updateData.NAME_CH = model.Detail.NAME_CH;
                            updateData.NAME_ENG = model.Detail.NAME_ENG;
                            updateData.START_DATE = HelperUtil.TransToDateTime(model.Detail.START_DATE);
                            updateData.END_DATE = HelperUtil.TransToDateTime(model.Detail.END_DATE);
                            var payStrA = model.Detail.PAY_WAY_C ? "C" : "";
                            var payStrB = model.Detail.PAY_WAY_S ? "S" : "";
                            var payStrC = model.Detail.PAY_WAY_T ? "T" : "";
                            var payStrD = model.Detail.PAY_WAY_L ? "L" : "";
                            updateData.PAY_WAY = $"{payStrA}{payStrB}{payStrC}{payStrD}";
                            updateData.BANK_CODE = model.Detail.BANK_CODE;
                            updateData.BANK_ACCOUNT = model.Detail.BANK_ACCOUNT;
                            updateData.BANK_NAME = model.Detail.BANK_NAME;
                            updateData.DESC_CH = model.Detail.DESC_CH;
                            updateData.DESC_ENG = model.Detail.DESC_ENG;
                            updateData.ISDRAFT = isDraft ? "Y" : "N";
                            updateData.ISOPEN = model.Detail.ISOPEN;
                            var dt = DateTime.Now;
                            updateData.UPD_TIME = dt;
                            updateData.UPD_FUN_CD = "ADM-DONATE";
                            updateData.UPD_ACC = account;
                            where.DEL_MK = "N";
                            Update<TblAPPLY_DONATE>(updateData, where);
                        }


                    }
                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("AppendDonate failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return ErrorMsg;
        }
        /// <summary>
        /// 取得單筆捐款專案
        /// </summary>
        /// <param name="srv_id_donate"></param>
        /// <returns></returns>
        public ApplyDonateDetailModel GetApplyDonate(string srv_id_donate)
        {
            ApplyDonateDetailModel model = new ApplyDonateDetailModel();
            TblAPPLY_DONATE where = new TblAPPLY_DONATE();
            where.SRV_ID_DONATE = srv_id_donate;
            var data = this.GetRow<TblAPPLY_DONATE>(where);
            model.SRV_ID_DONATE = data.SRV_ID_DONATE;
            model.NAME_CH = data.NAME_CH;
            model.NAME_ENG = data.NAME_ENG;
            model.START_DATE = HelperUtil.DateTimeToString(data.START_DATE);
            model.END_DATE = HelperUtil.DateTimeToString(data.END_DATE);
            model.PAY_WAY_C = data.PAY_WAY.Contains("C") ? true : false;
            model.PAY_WAY_S = data.PAY_WAY.Contains("S") ? true : false;
            model.PAY_WAY_T = data.PAY_WAY.Contains("T") ? true : false;
            model.PAY_WAY_L = data.PAY_WAY.Contains("L") ? true : false;
            model.BANK_CODE = data.BANK_CODE;
            model.BANK_ACCOUNT = data.BANK_ACCOUNT;
            model.BANK_NAME = data.BANK_NAME;
            model.DESC_CH = data.DESC_CH;
            model.DESC_ENG = data.DESC_ENG;
            model.ISOPEN = data.ISOPEN;
            model.ISDRAFT = data.ISDRAFT;
            return model;
        }
        /// <summary>
        /// 取得捐款專案附件
        /// </summary>
        /// <param name="srv_id_donate"></param>
        /// <returns></returns>
        public List<string> GetApplyDonateFile(string srv_id_donate)
        {
            List<string> model = new List<string>();
            Apply_FileModel where = new Apply_FileModel();
            where.APP_ID = $"00000000{srv_id_donate}0001";
            where.DEL_MK = "N";
            var data = this.GetRowList<Apply_FileModel>(where);
            //  select @file1 = SRC_FILENAME + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
            //  from APPLY_FILE where APP_ID = '" + app_id + @"' and FILE_NO = '1'
            foreach (var item in data)
            {
                var src_no = item.SRC_NO == null ? "0" : Convert.ToString(item.SRC_NO);
                var str = $"{item.SRC_FILENAME},{item.APP_ID},{item.FILE_NO},{src_no}";
                model.Add(str);
            }
            return model;
        }
        /// <summary>
        /// 計算捐款專案附件數量
        /// </summary>
        /// <param name="srv_id_donate"></param>
        /// <returns></returns>
        public Int32 CountApplyDonateFileAll(string srv_id_donate)
        {
            Apply_FileModel where = new Apply_FileModel();
            where.APP_ID = $"00000000{srv_id_donate}0001";
            var data = this.GetRowList<Apply_FileModel>(where);
            var result = data.ToCount();
            return result;
        }
        /// <summary>
        /// 捐款專案上傳附件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="account"></param>
        public void UploadFileDonate(ApplyDonateViewModel model, string account)
        {

            SessionModel sm = SessionModel.Get();
            ShareDAO dao = new ShareDAO();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    this.Tran(conn, tran);
                    if (!model.File_1.TONotNullString().Equals(""))
                    {
                        var src_all = this.CountApplyDonateFileAll(model.Detail.SRV_ID_DONATE);
                        var src_no = (src_all + 1).ToString();
                        Apply_FileModel file = new Apply_FileModel();
                        file.APP_ID = $"00000000{model.Detail.SRV_ID_DONATE}0001";
                        file.FILE_NO = src_all + 1;
                        file.FILENAME = dao.PutFile(model.Detail.SRV_ID_DONATE, model.File_1, src_no);
                        file.SRC_NO = src_all + 1;
                        file.SRC_FILENAME = model.File_1.FileName;
                        file.ADD_TIME = DateTime.Now;
                        file.ADD_FUN_CD = "ADM-DONATE";
                        file.ADD_ACC = account;
                        file.UPD_TIME = DateTime.Now;
                        file.UPD_FUN_CD = "ADM-DONATE";
                        file.UPD_ACC = account;
                        file.DEL_MK = "N";

                        base.Insert(file);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("UploadFileDonate failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// 捐款專案異動專案狀態
        /// </summary>
        /// <param name="srv_id_donate"></param>
        /// <param name="isopen"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public string OpenChangeStatus(string srv_id_donate, string isopen, string account)
        {
            var ErrorMsg = string.Empty;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 更新
                    TblAPPLY_DONATE where = new TblAPPLY_DONATE();
                    where.SRV_ID_DONATE = srv_id_donate;
                    TblAPPLY_DONATE updata = new TblAPPLY_DONATE();
                    updata.ISOPEN = isopen == "Y" ? "N" : "Y";
                    var dt = DateTime.Now;
                    updata.UPD_TIME = dt;
                    updata.UPD_FUN_CD = "ADM-DONATE";
                    updata.UPD_ACC = account;
                    Update<TblAPPLY_DONATE>(updata, where);

                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("OpenChangeStatus failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return ErrorMsg;
        }

        /// <summary>
        /// 刪除捐款專案附件
        /// </summary>
        /// <param name="filestr"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public string DelFileDonate(string filestr, string account)
        {
            var ErrorMsg = string.Empty;
            var values = filestr.ToSplit(',');
            // var str = $"{item.SRC_FILENAME},{item.APP_ID},{item.FILE_NO},{src_no}";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    #region 更新
                    Apply_FileModel where = new Apply_FileModel();
                    where.APP_ID = values[1];
                    where.SRC_FILENAME = values[0];
                    Apply_FileModel updata = new Apply_FileModel();
                    var dt = DateTime.Now;
                    updata.DEL_MK = "Y";
                    updata.DEL_TIME = dt;
                    updata.DEL_FUN_CD = "ADM-DONATE";
                    updata.DEL_ACC = account;
                    updata.UPD_TIME = dt;
                    updata.UPD_FUN_CD = "ADM-DONATE";
                    updata.UPD_ACC = account;
                    Update<Apply_FileModel>(updata, where);

                    #endregion
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("DelFileDonate failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return ErrorMsg;
        }
        /// <summary>
        /// 取得單筆捐款明細
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="srvid"></param>
        /// <returns></returns>
        public ApplyDonateDetailModifyModel GetAPPLYDonateDetail(string appid)
        {
            var result = new ApplyDonateDetailModifyModel();
            ApplyModel where = new ApplyModel();
            where.APP_ID = appid;
            var data = this.GetRow<ApplyModel>(where);
            // 帶入基本資料
            result.USE_MK = true;
            result.APP_ID = appid;
            result.SRV_ID_DONATE = data.SRV_ID;
            result.SRC_SRV_ID = data.SRV_ID;
            result.ACC_NO = data.IDN;
            result.IDN = data.IDN;
            result.BIRTHDAY = data.BIRTHDAY;
            result.MOBILE = data.MOBILE;
            result.ADDR_CODE = data.ADDR_CODE;
            result.ADDR = data.ADDR;
            result.PAY_METHOD = data.PAY_METHOD;

            Apply_007001Model where07 = new Apply_007001Model();
            where07.APP_ID = appid;
            var data07 = this.GetRow<Apply_007001Model>(where07);
            result.REF_MEM_MK = string.IsNullOrWhiteSpace(data07.REF_MEM_MK) ? false : data07.REF_MEM_MK == "N" ? false : true;
            result.DONOR_NAME = data07.DONOR_NAME;
            result.DONOR_IDN = data07.DONOR_IDN;
            result.PUBLIC_MK = data07.PUBLIC_MK;
            result.DONOR_MAIL = data07.DONOR_MAIL;
            result.DONOR_TEL = data07.DONOR_TEL;
            result.AMOUNT = data07.AMOUNT.TONotNullString();

            Apply_007001_DataModel where07D = new Apply_007001_DataModel();
            var data07D = this.GetRow<Apply_007001_DataModel>(where07D);
            result.REC_MK = data07D.REC_MK;
            result.REC_TITLE_CD = data07D.REC_TITLE_CD == "1" ? "1" : "2";
            result.REC_TITLE = data07D.REC_TITLE;
            result.REC_ADDR_1 = data07D.REC_ADDR_1;
            result.REC_ADDR_2 = data07D.REC_ADDR_2;
            //APPLY_PAY wherePay = new APPLY_PAY();
            //wherePay.APP_ID = appid;
            //var dataPay = this.GetRow<APPLY_PAY>(wherePay);
            return result;
        }

        /// <summary>
        /// 更新捐款明細
        /// </summary>
        /// <param name="model"></param>
        public string UpdateDonateDetail(ApplyDonateDetailModifyModel model)
        {
            ShareDAO dao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            var ErrMsg = string.Empty;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    // Update Apply
                    ApplyModel where = new ApplyModel();
                    where.APP_ID = model.APP_ID;
                    ApplyModel data = new ApplyModel();
                    // 帶入基本資料                  
                    data.APP_ID = model.APP_ID;
                    data.IDN = model.IDN;
                    data.SEX_CD = model.IDN.Substring(2, 1) == "1" ? "M" : "F";
                    data.BIRTHDAY = model.BIRTHDAY;
                    data.NAME = model.DONOR_NAME;
                    data.TEL = model.DONOR_TEL;
                    data.MOBILE = model.MOBILE;
                    data.ADDR_CODE = model.ADDR_CODE;
                    data.ADDR = model.ADDR;
                    data.UPD_TIME = DateTime.Now;
                    data.UPD_FUN_CD = "WEB-APPLY";
                    data.UPD_ACC = sm.UserInfo.UserNo;
                    data.PAY_METHOD = model.PAY_METHOD;
                    data.PAY_A_FEE = model.AMOUNT.TOInt32();
                    if (model.PAY_METHOD == "S")
                    {
                        where.PAY_A_FEEBK = Int32.Parse(DataUtils.GetConfig("PAY_STORE_FEE"));
                    }
                    else
                    {
                        where.PAY_A_FEEBK = 0;
                    }
                    base.Update(data, where);

                    // Update 案件
                    Apply_007001Model where07 = new Apply_007001Model();
                    where07.APP_ID = model.APP_ID;
                    Apply_007001Model data07 = new Apply_007001Model();
                    data07.APP_ID = model.APP_ID;
                    data07.REF_MEM_MK = model.REF_MEM_MK == false ? "N" : "Y";
                    data07.DONOR_NAME = model.DONOR_NAME;
                    data07.DONOR_IDN = model.DONOR_IDN;
                    data07.PUBLIC_MK = model.PUBLIC_MK;
                    data07.DONOR_MAIL = model.DONOR_MAIL;
                    data07.DONOR_TEL = model.DONOR_TEL;
                    data07.AMOUNT = model.AMOUNT.TOInt32();
                    data07.UPD_TIME = DateTime.Now;
                    data07.UPD_FUN_CD = "WEB-APPLY";
                    data07.UPD_ACC = sm.UserInfo.UserNo;
                    base.Update(data07, where07);

                    // Update 收據抬頭
                    Apply_007001_DataModel where07D = new Apply_007001_DataModel();
                    where07D.APP_ID = model.APP_ID;
                    Apply_007001_DataModel data07D = new Apply_007001_DataModel();
                    data07D.APP_ID = model.APP_ID;
                    data07D.REC_MK = model.REC_TITLE_CD == "" ? "N" : "Y";
                    data07D.REC_TITLE_CD = model.REC_TITLE_CD == "1" ? "1" : "2";
                    data07D.REC_TITLE = model.REC_TITLE;
                    data07D.REC_ADDR_1 = model.ADDR_CODE;
                    data07D.REC_ADDR_2 = model.ADDR;
                    data07D.UPD_TIME = DateTime.Now;
                    data07D.UPD_FUN_CD = "WEB-APPLY";
                    data07D.UPD_ACC = sm.UserInfo.UserNo;
                    base.Update(data07D, where07D);

                    #region 繳費資訊
                    // Update 繳費資訊
                    APPLY_PAY wherePay = new APPLY_PAY();
                    wherePay.APP_ID = model.APP_ID;
                    APPLY_PAY dataPay = new APPLY_PAY();
                    dataPay.APP_ID = model.APP_ID;
                    dataPay.PAY_MONEY = model.AMOUNT.TOInt32();
                    if (model.PAY_METHOD == "S")
                    {
                        dataPay.PAY_PROFEE = Int32.Parse(DataUtils.GetConfig("PAY_STORE_FEE"));
                    }
                    else
                    {
                        dataPay.PAY_PROFEE = 0;
                    }
                    dataPay.PAY_INC_TIME = DateTime.Now;
                    dataPay.PAY_METHOD = model.PAY_METHOD;
                    dataPay.UPD_TIME = DateTime.Now;
                    dataPay.UPD_FUN_CD = "WEB-APPLY";
                    dataPay.UPD_ACC = sm.UserInfo.UserNo; ;
                    base.Update(dataPay, wherePay);
                    #endregion

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    ErrMsg = ex.Message;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return ErrMsg;
        }
        /// <summary>
        /// 檢核異動捐款明細
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckDonateDetail(ApplyDonateDetailModifyModel model)
        {
            var ErrMsg = string.Empty;

            return ErrMsg;
        }

        public System.Data.DataTable ExportDonateDetail(List<ApplyDonateGridDetailModel> list)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("項次", typeof(string));
            dt.Columns.Add("收據編號", typeof(string));
            dt.Columns.Add("捐贈者名稱或姓名", typeof(string));
            dt.Columns.Add("金額(新臺幣/元)", typeof(string));
            dt.Columns.Add("捐贈日期", typeof(string));
            dt.Columns.Add("捐贈用途", typeof(string));
            dt.Columns.Add("指定用途", typeof(string));
            dt.Columns.Add("說明", typeof(string));

            foreach (var item in list)
            {
                dt.Rows.Add(
                    item.SEQ.TONotNullString(),
                    "",
                    item.D_NAME.TONotNullString(),
                    item.PAY_MONEY.TONotNullString(),
                    item.ADD_TIME.TONotNullString(),
                    item.D_USE.TONotNullString(),
                    item.D_DUSE.TONotNullString(),
                    item.D_REMARK.TONotNullString()
                    );
            }

            return dt;
        }
    }
}
