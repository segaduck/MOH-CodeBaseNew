using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using ES.Action;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.Models;
using ES.Models.Entities;
using ES.Models.Share;
using ES.Models.ViewModels;
using ES.Services;
using ES.Utils;
using Omu.ValueInjecter;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Xml;

namespace ES.DataLayers
{
    public class ShareDAO : BaseAction
    {
        #region ZIP_CO
        /// <summary>
        /// 查詢 ZIP_CO
        /// 支援 5碼 (ZIPCODE表) 和 6碼 (ZIPCODE6表) 格式
        /// </summary>
        /// <param name="parms">查詢參數，包含 ZIP_FORMAT (5=5碼, 6=6碼)</param>
        public IList<ZIP_COGridModel> QueryZIP_CO(ZIP_COFormModel parms)
        {
            ZIP_COGridModel model = new ZIP_COGridModel();
            IList<ZIP_COGridModel> li = new List<ZIP_COGridModel>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    var _da = new SqlDataAdapter();
                    DataTable _dt = new DataTable();
                    
                    // 根據 ZIP_FORMAT 選擇查詢來源表
                    // 預設使用6碼表 ZIPCODE6
                    string tableName = (parms.ZIP_FORMAT == "5") ? "ZIPCODE" : "ZIPCODE6";
                    
                    // 根據格式選擇不同的 SQL 查詢
                    string _sql;
                    if (parms.ZIP_FORMAT == "5")
                    {
                        // 5碼表：ZIPCODE 有 ROADNM 和 ROUND 欄位
                        _sql = @" SELECT DISTINCT ZIP_CO AS CODE,(CITYNM + TOWNNM) AS TEXT,
                                        ROADNM, ROUND
                                    FROM ZIPCODE 
                                    WHERE 1 = 1";
                    }
                    else
                    {
                        // 6碼表：ZIPCODE6 有 ROADNM 和 SCOOP 欄位 (SCOOP 對應 ROUND)
                        _sql = @" SELECT DISTINCT ZIP_CO AS CODE,(CITYNM + TOWNNM) AS TEXT,
                                        ROADNM, SCOOP AS ROUND
                                    FROM ZIPCODE6 
                                    WHERE 1 = 1";
                    }

                    if (!string.IsNullOrEmpty(parms.ZIP_CO))
                    {
                        _sql += " and ZIP_CO like '%" + parms.ZIP_CO.ToTrim() + "%'";
                    }
                    if (!string.IsNullOrEmpty(parms.CITYNM))
                    {
                        _sql += " and CITYNM like '%" + parms.CITYNM.ToTrim() + "%'";
                    }
                    if (!string.IsNullOrEmpty(parms.TOWNNM))
                    {
                        _sql += " and TOWNNM like '%" + parms.TOWNNM.ToTrim() + "%'";
                    }
                    // ROADNM 查詢條件 (5碼和6碼都支援)
                    if (!string.IsNullOrWhiteSpace(parms.ROADNM))
                    {
                        _sql += " and ROADNM like '%" + parms.ROADNM.ToTrim() + "%'";
                    }

                    _sql += " order by ZIP_CO";


                    _da.SelectCommand = new SqlCommand(_sql, conn);
                    _da.Fill(_dt);
                    for (var i = 0; i < _dt.Rows.Count; i++)
                    {
                        var _rows = _dt.Rows[i];
                        ZIP_COGridModel t = (ZIP_COGridModel)Activator.CreateInstance(typeof(ZIP_COGridModel));
                        foreach (var pi in t.GetType().GetProperties())
                        {
                            var piName = pi.Name;
                            if (_dt.Columns.Contains(piName))
                            {
                                var rowValue = Convert.ChangeType(_rows[piName], _rows[piName].GetType());
                                if (rowValue.ToString() != "")
                                {
                                    pi.SetValue(t, rowValue);
                                }
                            }
                        }
                        li.Add(t);
                    }
                }
                if (li != null)
                {
                    this.totalCount = li.Count();
                    li = li.Skip((parms.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }
        #endregion

        #region 共用欄位驗證(Apply)

        public string checkApply<T>(T TModel)
        {
            string ErrMsg = "";
            ApplyModel form = new ApplyModel();
            form.InjectFrom(TModel);

            if (!string.IsNullOrEmpty(form.IDN))
            {
                if (!CheckUtils.IsIdentity(form.IDN))
                {
                    ErrMsg += "身分證/統一編號輸入錯誤\r\n";
                }

                //if (form.IDN.Length == 10)
                //{
                //    //身分證格式驗證(第一碼為英文，第二碼只能為:1,2,A,B,C,D)
                //    if (!System.Text.RegularExpressions.Regex.IsMatch(form.IDN.Substring(0, 1), @"^[A-Z]$"))
                //    {
                //        //第一碼為A-Z
                //        ErrMsg += "身分證格式輸入錯誤\r\n";
                //    }
                //    else if (!System.Text.RegularExpressions.Regex.IsMatch(form.IDN.Substring(1, 1), @"^[A-D,1,2]$"))
                //    {
                //        //第二碼只能為:1,2,A,B,C,D
                //        ErrMsg += "身分證格式輸入錯誤\r\n";
                //    }
                //    else if (form.IDN.Substring(0, 1) == "Y")
                //    {
                //        //特殊判斷開頭為Y，第二碼不可為A,B,C,D
                //        if(System.Text.RegularExpressions.Regex.IsMatch(form.IDN.Substring(1, 1), @"^[A-D]$"))
                //        {
                //            ErrMsg += "身分證格式輸入錯誤\r\n";
                //        }
                //    }
                //}
                //else if (form.IDN.Length == 8)
                //{
                //    //全數字
                //    for (int i = 0; i < form.IDN.Length; i++)
                //    {
                //        if (!Char.IsNumber(form.IDN, i))
                //        {
                //            ErrMsg += "統一編號格式輸入錯誤!\r\n";
                //            break;
                //        }
                //    }
                //}
                //else
                //{
                //    //error
                //    ErrMsg += "身分證號/統一編號長度輸入錯誤\r\n";
                //}
            }

            if (!string.IsNullOrEmpty(form.TEL))
            {
                //電話數字驗證(-,#,數字)
                if (!System.Text.RegularExpressions.Regex.IsMatch(form.TEL, @"^(\d{2,4}-)(\d{6,8})?(#\d{1,6})?$"))
                {
                    ErrMsg += "電話格式輸入錯誤\r\n";
                }
            }

            if (!string.IsNullOrEmpty(form.FAX))
            {
                //傳真數字驗證(-,#,數字)
                if (!System.Text.RegularExpressions.Regex.IsMatch(form.TEL, @"^(\d{2,4}-)(\d{6,8})?(#\d{1,6})?$"))
                {
                    ErrMsg += "傳真格式輸入錯誤\r\n";
                }
            }

            if (!string.IsNullOrEmpty(form.CNT_TEL))
            {
                //聯絡人電話數字驗證(-,#,數字)
                if (!System.Text.RegularExpressions.Regex.IsMatch(form.TEL, @"^(\d{2,4}-)(\d{6,8})?(#\d{1,6})?$"))
                {
                    ErrMsg += "聯絡人電話格式輸入錯誤\r\n";
                }
            }

            if (!string.IsNullOrEmpty(form.MOBILE))
            {
                //手機數字驗證:09開頭，後面接著8個0~9的數字，@是避免跳脫字元
                if (!System.Text.RegularExpressions.Regex.IsMatch(form.MOBILE, @"^09[0-9]{8}$"))
                {
                    ErrMsg += "手機格式輸入錯誤\r\n";
                }
            }

            return ErrMsg;
        }


        #endregion

        #region 繳費頁面資訊查詢

        public DataTable getPayInfo(string Service_ID)
        {
            string _sql = @" select APP_FEE,PAY_METHOD,PAY_POINT
                                 from service 
                                 where SRV_ID=@SRV_ID ";
            DataTable _dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    conn.Open();
                    dbc.Parameters.Clear();
                    dbc.Parameters.Add("SRV_ID", SqlDbType.VarChar).Value = Service_ID;
                    _dt.Load(dbc.ExecuteReader());
                    //_da.SelectCommand = dbc;
                    //_da.Fill(_dt);
                }
                conn.Close();
                conn.Dispose();
            }
            return _dt;
        }

        #endregion

        #region SFTP上傳下載

        // SFTP-建立資料夾
        public void AddDirectory(string srv_id)
        {
            // SFTP-可以使用才繼續,不可使用退出
            if (!ConfigModel.FtpNasCanUse.Equals("Y"))
            {
                logger.Info("SFTP檔案上傳 不可使用!");
                return;
            }

            try
            {
                using (var sftp = new SftpClient(ConfigModel.FtpNasServer, 22, ConfigModel.FtpNasUser, ConfigModel.FtpNasPassword))
                {
                    //檔案路徑原則:SRV_ID/YEAR/MONTH/file
                    sftp.Connect();
                    int FolderCount_srv = 0, FolderCount_year = 0, FolderCount_month = 0;
                    var DirectorySrvList = sftp.ListDirectory(ConfigModel.FtpNasPath).ToList();
                    foreach (var item in DirectorySrvList)
                    {
                        if (item.Name == srv_id)
                        {
                            FolderCount_srv++;
                        }
                    }

                    if (FolderCount_srv == 0)
                    {
                        sftp.CreateDirectory(ConfigModel.FtpNasPath + srv_id);
                    }

                    var DirectoryYearList = sftp.ListDirectory(ConfigModel.FtpNasPath + srv_id).ToList();
                    foreach (var itemYear in DirectoryYearList)
                    {
                        if (itemYear.Name == DateTime.Now.ToString("yyyy"))
                        {
                            FolderCount_year++;
                        }
                    }

                    if (FolderCount_year == 0)
                    {
                        sftp.CreateDirectory(ConfigModel.FtpNasPath + srv_id + "/" + DateTime.Now.ToString("yyyy"));
                    }

                    var DirectoryMonthList = sftp.ListDirectory(ConfigModel.FtpNasPath + srv_id + "/" + DateTime.Now.ToString("yyyy")).ToList();
                    foreach (var itemMonth in DirectoryMonthList)
                    {
                        if (itemMonth.Name == DateTime.Now.ToString("MM"))
                        {
                            FolderCount_month++;
                        }
                    }

                    if (FolderCount_month == 0)
                    {
                        sftp.CreateDirectory(ConfigModel.FtpNasPath + srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM"));
                    }

                    sftp.Disconnect();
                }

            }
            catch (Exception ex)
            {
                logger.Error("SFTP建立資料夾失敗，原因：" + ex.Message, ex);
                throw new Exception(string.Format("SFTP建立資料夾失敗，原因：{0}", ex.Message));
            }
        }

        //上傳檔案
        public string PutFile(string srv_id, HttpPostedFileBase uploadfile, string serNum)
        {
            try
            {
                // 取得亂數 避免重複檔名
                var getGUID = Guid.NewGuid();
                var guidItem = getGUID.ToString().ToSplit('-');
                // 檢核副檔名是否合法
                var extension = System.IO.Path.GetExtension(uploadfile.FileName)?.ToUpper().ToTrim().ToSplit('.').LastOrDefault();
                var allowedExtensions = new[] { "DOC", "DOCX", "ODT", "ODF", "ODS", "XLS", "XLSX", "PDF", "PPT", "PPTX", "JPG", "JPEG", "BMP", "PNG", "GIF", "TIF", "ZIP" };

                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("不允許的檔案類型：" + extension);
                }

                var fullPath = ConfigModel.FtpNasPath + srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" +
                               DateTime.Now.ToString("MM") + "/";
                var fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + System.IO.Path.GetExtension(uploadfile.FileName);
                var localPath = "";

                //儲存至本機
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    var _da = new SqlDataAdapter();
                    DataTable _dt = new DataTable();
                    string _sql = @"select SETUP_VAL from setup where SETUP_CD='FOLDER_APPLY_FILE'";
                    using (SqlCommand dbc = new SqlCommand(_sql, conn))
                    {
                        _da.SelectCommand = dbc;
                        _da.Fill(_dt);
                    }

                    localPath = _dt.Rows[0][0].ToString().Replace('\\', '/');
                    localPath += srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM") + "/";
                    conn.Close();
                    conn.Dispose();
                }
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(localPath);
                if (!dir.Exists) { dir.Create(); }
                FileStream fs = System.IO.File.Open(localPath + fileName, FileMode.Append);
                MemoryStream target = new MemoryStream();
                uploadfile.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Close();

                // SFTP-可以使用才繼續,不可使用退出
                if (!ConfigModel.FtpNasCanUse.Equals("Y"))
                {
                    logger.Info("SFTP檔案上傳 不可使用!");
                }
                else
                {
                    if (srv_id == "001034" || srv_id == "001035" || srv_id == "001038")
                    {
                        // 20210111 因SFTP需為後續醫審排程處理，非即時性丟檔案，故修改
                        //AddDirectory(srv_id);

                        //using (var sftp = new SftpClient(ConfigModel.FtpNasServer, 22, ConfigModel.FtpNasUser, ConfigModel.FtpNasPassword))
                        //{
                        //    sftp.Connect();

                        //    using (Stream fileStream = new MemoryStream(File.ReadAllBytes(localPath + fileName)))
                        //    {
                        //        sftp.UploadFile(fileStream, fullPath + fileName, true);
                        //    }
                        //    sftp.Disconnect();
                        //}
                    }
                }

                return srv_id + "\\" + DateTime.Now.ToString("yyyy") + "\\" +
                       DateTime.Now.ToString("MM") + "\\" + fileName;

            }
            catch (Exception ex)
            {
                logger.Error("SFTP檔案上傳失敗(PutFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("SFTP檔案上傳失敗，原因：{0}", ex.Message));
            }
        }

        public string PutFile(string srv_id, string srcFileName, string serNum)
        {
            try
            {
                // 取得亂數 避免重複檔名
                var getGUID = Guid.NewGuid();
                var guidItem = getGUID.ToString().ToSplit('-');
                // 檢核副檔名是否合法
                var extension = System.IO.Path.GetExtension(srcFileName)?.ToUpper().ToTrim().ToSplit('.').LastOrDefault();
                var allowedExtensions = new[] { "DOC", "DOCX", "ODT", "ODF", "ODS", "XLS", "XLSX", "PDF", "PPT", "PPTX", "JPG", "JPEG", "BMP", "PNG", "GIF", "TIF", "ZIP" };

                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("不允許的檔案類型：" + extension);
                }

                var fullPath = ConfigModel.FtpNasPath + srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" +
                               DateTime.Now.ToString("MM") + "/";
                var fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + srcFileName.Substring(srcFileName.IndexOf('.'));
                var localPath = "";
                var srcLocalPath = "";

                //儲存至本機
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    var _da = new SqlDataAdapter();
                    DataTable _dt = new DataTable();
                    string _sql = @"select SETUP_VAL from setup where SETUP_CD='FOLDER_APPLY_FILE'";
                    using (SqlCommand dbc = new SqlCommand(_sql, conn))
                    {
                        _da.SelectCommand = dbc;
                        _da.Fill(_dt);
                    }

                    localPath = _dt.Rows[0][0].ToString().Replace('\\', '/');
                    srcLocalPath = _dt.Rows[0][0].ToString().Replace('\\', '/');

                    localPath += srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM") + "/";
                    srcLocalPath += "/" + srcFileName.Replace('\\', '/');
                    conn.Close();
                    conn.Dispose();
                }

                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(localPath);
                if (!dir.Exists)
                {
                    dir.Create();
                }

                FileStream fs = System.IO.File.Open(localPath + fileName, FileMode.Append);
                MemoryStream target = new MemoryStream();
                FileStream srcFs = System.IO.File.Open(srcLocalPath, FileMode.Open);
                srcFs.CopyTo(target);
                byte[] data = target.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Close();

                return srv_id + "\\" + DateTime.Now.ToString("yyyy") + "\\" +
                       DateTime.Now.ToString("MM") + "\\" + fileName;

            }
            catch (Exception ex)
            {
                logger.Error("SFTP檔案上傳失敗(PutFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("SFTP檔案上傳失敗，原因：{0}", ex.Message));
            }
        }


        //上傳檔案(醫事人員請領英文證明書)
        public string PutFile(string srv_id, string app_id, HttpPostedFileBase uploadfile, string serNum, string fileAlias = "_FILE_")
        {
            try
            {
                // 檢核副檔名是否合法
                var extension = System.IO.Path.GetExtension(uploadfile.FileName)?.ToUpper().ToTrim().ToSplit('.').LastOrDefault();
                var allowedExtensions = new[] { "DOC", "DOCX", "ODT", "ODF", "ODS", "XLS", "XLSX", "PDF", "PPT", "PPTX", "JPG", "JPEG", "BMP", "PNG", "GIF", "TIF", "ZIP" };

                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("不允許的檔案類型：" + extension);
                }
                var fullPath = ConfigModel.FtpNasPath + srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" +
                               DateTime.Now.ToString("MM") + "/" + (string.IsNullOrWhiteSpace(app_id) ? "" : app_id + "/");
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + fileAlias + serNum + System.IO.Path.GetExtension(uploadfile.FileName);
                var localPath = "";

                //儲存至本機
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    var _da = new SqlDataAdapter();
                    DataTable _dt = new DataTable();
                    string _sql = @"select SETUP_VAL from setup where SETUP_CD='FOLDER_APPLY_FILE'";
                    using (SqlCommand dbc = new SqlCommand(_sql, conn))
                    {
                        _da.SelectCommand = dbc;
                        _da.Fill(_dt);
                    }

                    localPath = _dt.Rows[0][0].ToString().Replace('\\', '/');
                    localPath += srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM") + "/" + app_id + "/";
                    conn.Close();
                    conn.Dispose();
                }
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(localPath);
                if (!dir.Exists)
                {
                    dir.Create();
                }
                FileStream fs = System.IO.File.Open(localPath + fileName, FileMode.Append);
                MemoryStream target = new MemoryStream();
                uploadfile.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Close();

                return srv_id + "\\" + DateTime.Now.ToString("yyyy") + "\\" +
                      DateTime.Now.ToString("MM") + "\\" + (string.IsNullOrWhiteSpace(app_id) ? "" : app_id + "\\") + fileName;

            }
            catch (Exception ex)
            {
                logger.Error("檔案上傳失敗_醫事人員或公共衛生師請領英文證明書(PutFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("檔案上傳失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 檔案上傳(最新消息)
        /// </summary>
        /// <param name="srv_id"></param>
        /// <param name="uploadfile"></param>
        /// <param name="serNum"></param>
        /// <returns></returns>
        public string MSGPutFile(string id, HttpPostedFileBase uploadfile, string serNum, string msg)
        {
            try
            {
                // 取得亂數 避免重複檔名
                var getGUID = Guid.NewGuid();
                var guidItem = getGUID.ToString().ToSplit('-');
                var fullPath = ConfigModel.FtpNasPath + id + "/" + DateTime.Now.ToString("yyyy") + "/" +
                               DateTime.Now.ToString("MM") + "/";
                var fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + System.IO.Path.GetExtension(uploadfile.FileName);
                var localPath = "";


                localPath = (@"C:\e-service\File\Message\").Replace('\\', '/');
                localPath += id + "/";
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(localPath);
                if (!dir.Exists) { dir.Create(); }
                FileStream fs = System.IO.File.Open(localPath + fileName, FileMode.Append);
                MemoryStream target = new MemoryStream();
                uploadfile.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Close();

                return id + "\\" + fileName;

            }
            catch (Exception ex)
            {
                logger.Error("SFTP檔案上傳失敗(MSGPutFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("SFTP檔案上傳失敗，原因：{0}", ex.Message));
            }
        }

        /// <summary>
        /// 檔案上傳(書表下載)
        /// </summary>
        /// <param name="srv_id"></param>
        /// <param name="uploadfile"></param>
        /// <param name="serNum"></param>
        /// <returns></returns>
        public string SFPutFile(string id, HttpPostedFileBase uploadfile, string serNum, string msg)
        {
            try
            {
                // 取得亂數 避免重複檔名
                var getGUID = Guid.NewGuid();
                var guidItem = getGUID.ToString().ToSplit('-');
                var fullPath = ConfigModel.FtpNasPath + id + "/" + DateTime.Now.ToString("yyyy") + "/" +
                               DateTime.Now.ToString("MM") + "/";
                var fileName = DateTime.Now.ToString("yyyyMMddHHmm") + guidItem[1] + "_FILE_" + serNum + System.IO.Path.GetExtension(uploadfile.FileName);
                var localPath = "";


                localPath = (@"C:\e-service\File\ServiceFile\").Replace('\\', '/');
                localPath += id + "/";
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(localPath);
                if (!dir.Exists) { dir.Create(); }
                FileStream fs = System.IO.File.Open(localPath + fileName, FileMode.Append);
                MemoryStream target = new MemoryStream();
                uploadfile.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Close();

                return id + "\\" + fileName;

            }
            catch (Exception ex)
            {
                logger.Error("SFTP檔案上傳失敗(SFPutFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("SFTP檔案上傳失敗，原因：{0}", ex.Message));
            }
        }

        //刪除資料夾
        public void DelDirectory(string app_id)
        {
            // SFTP-可以使用才繼續,不可使用退出
            if (!ConfigModel.FtpNasCanUse.Equals("Y"))
            {
                logger.Info("SFTP檔案上傳 不可使用!");
                return;
            }

            try
            {
                using (var sftp = new SftpClient(ConfigModel.FtpNasServer, 22, ConfigModel.FtpNasUser, ConfigModel.FtpNasPassword))
                {
                    sftp.Connect();

                    foreach (SftpFile file in sftp.ListDirectory(ConfigModel.FtpNasPath + app_id))
                    {
                        if ((file.Name != ".") && (file.Name != ".."))
                        {
                            sftp.DeleteFile(file.FullName);
                        }
                    }

                    sftp.DeleteDirectory(ConfigModel.FtpNasPath + app_id);
                    sftp.Disconnect();
                }

            }
            catch (Exception ex)
            {
                logger.Error("SFTP刪除資料夾失敗(DelDirectory)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("SFTP刪除資料夾失敗，原因：{0}", ex.Message));
            }
        }

        //刪除檔案
        public void DelFile(string srv_id, string fileName)
        {
            // SFTP-可以使用才繼續,不可使用退出
            if (!ConfigModel.FtpNasCanUse.Equals("Y"))
            {
                logger.Info("SFTP檔案上傳 不可使用!");
                return;
            }

            try
            {
                using (var sftp = new SftpClient(ConfigModel.FtpNasServer, 22, ConfigModel.FtpNasUser, ConfigModel.FtpNasPassword))
                {
                    var fullPath = ConfigModel.FtpNasPath + srv_id + "/" + DateTime.Now.ToString("yyyy") + "/" +
                                   DateTime.Now.ToString("MM") + "/";
                    sftp.Connect();
                    sftp.DeleteFile(fullPath + fileName);
                    sftp.Disconnect();
                }

            }
            catch (Exception ex)
            {
                logger.Error("SFTP刪除檔案失敗(DelFile)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("SFTP刪除檔案失敗，原因：{0}", ex.Message));
            }
        }

        //下載
        public byte[] sftpDownload(string fullPath)
        {
            try
            {
                //using (var sftp = new SftpClient(ConfigModel.FtpNasServer, 22, ConfigModel.FtpNasUser, ConfigModel.FtpNasPassword))
                //{
                //    sftp.Connect();
                //    using (MemoryStream fileStream = new MemoryStream())
                //    {
                //        fullPath = ConfigModel.FtpNasPath + fullPath;

                //        sftp.DownloadFile(fullPath.Replace("\\", "/"), fileStream);

                //        return fileStream.ToArray();
                //    }
                //    sftp.Disconnect();
                //}

                var localPath = "";
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    var _da = new SqlDataAdapter();
                    DataTable _dt = new DataTable();
                    string _sql = @"select SETUP_VAL from setup where SETUP_CD='FOLDER_APPLY_FILE'";
                    using (SqlCommand dbc = new SqlCommand(_sql, conn))
                    {
                        _da.SelectCommand = dbc;
                        _da.Fill(_dt);
                    }

                    localPath = _dt.Rows[0][0].ToString().Replace('\\', '/') + fullPath;
                    localPath = localPath.Replace('\\', '/');
                    conn.Close();
                    conn.Dispose();
                }

                return File.ReadAllBytes(localPath);
            }
            catch (Exception ex)
            {
                logger.Error("下載檔案失敗(sftpDownload)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("下載檔案失敗，原因：{0}", ex.Message));
            }
        }
        public string getApplyFileRoute(string fullPath)
        {
            try
            {
                var localPath = "";
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    var _da = new SqlDataAdapter();
                    DataTable _dt = new DataTable();
                    string _sql = @"select SETUP_VAL from setup where SETUP_CD='FOLDER_APPLY_FILE'";
                    using (SqlCommand dbc = new SqlCommand(_sql, conn))
                    {
                        _da.SelectCommand = dbc;
                        _da.Fill(_dt);
                    }

                    localPath = _dt.Rows[0][0].ToString().Replace('\\', '/') + fullPath;
                    localPath = localPath.Replace('\\', '/');
                    conn.Close();
                    conn.Dispose();
                }

                return localPath;
            }
            catch (Exception ex)
            {
                logger.Error("下載檔案失敗(getApplyFileRoute)，原因：" + ex.Message, ex);
                throw new Exception(string.Format("下載檔案失敗，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 取得分案承辦信箱

        public IList<SpecialistMailModel> getSpecialist(string srv_id)
        {
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = "";
            IList<SpecialistMailModel> result = new List<SpecialistMailModel>();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                // 不走分文流程
                if (srv_id.Substring(0, 3) == "011" || srv_id.Substring(0, 3) == "010" || srv_id.Substring(0, 3) == "012" || srv_id.Substring(0,3)=="006" || srv_id.Substring(0,3)=="040" || srv_id.Substring(0,3)=="041")
                {
                    _sql = @" select ad.NAME,MAIL 
                              from   SERVICE sv
                              join   ADMIN ad on sv.PAGE_MAKER_ID = ad.ACC_NO
                              where  SRV_ID = '" + srv_id + "'";

                    // 此段為信件通知 110.01.27 Alan新增
                    _sql += @"union  all
                              select NAME, MAIL
                              from   MAILACC mc
                              where  SRV_ID = '" + srv_id + "'";
                }
                else
                {
                    _sql = @" select NAME,MAIL 
                                 from ADMIN 
                                 where UNIT_CD in (select UNIT_CD from SERVICE_UNIT where SRV_ID='" + srv_id + @"' union select distinct UNIT_PCD from UNIT where UNIT_CD in (select UNIT_CD from SERVICE_UNIT where SRV_ID='" + srv_id + @"'))
                                   and ACC_NO in (select ACC_NO from ADMIN_LEVEL where MN_ID='151') and MAIL is not null ";
                }

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }

                for (int i = 0; i < _dt.Rows.Count; i++)
                {
                    SpecialistMailModel item = new SpecialistMailModel();
                    item.NAME = _dt.Rows[i][0].ToString();
                    item.EMAIL = _dt.Rows[i][1].ToString();
                    result.Add(item);
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        #endregion

        #region 公文文號紀錄

        public void InsertCaseNo(string APP_ID, string MohwCaseNo)
        {
            SessionModel sm = SessionModel.Get();

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                this.conn = conn;
                OFFICIAL_DOC newData = new OFFICIAL_DOC();
                newData.APP_ID = APP_ID;
                newData.MOHW_CASE_NO = MohwCaseNo;
                newData.ADD_ACC = sm.UserInfo.Admin.ACC_NO;
                newData.INSERTDATE = DateTime.Now;

                base.Insert(newData);
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 紀錄傳送DI資料
        /// </summary>
        public void InsertOFFICIAL_DOC_DI(DOC_DISaveModel model)
        {
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                this.conn = conn;

                OFFICIAL_DOC_DI newData = new OFFICIAL_DOC_DI();
                newData.di_filename = model.di_filename;
                //newData.di_content_b64 = di_content_b64;
                newData.di_status = model.di_status;
                newData.add_user = sm.UserInfo.Admin.ACC_NO;
                newData.add_time = DateTime.Now;
                newData.APP_ID = model.APP_ID;
                newData.xml_content = model.xml_content;
                // 新公文介接欄位
                newData.SysID = model.SysID;
                newData.VerifyCode = model.VerifyCode;
                newData.UnitCode = model.UnitCode;
                newData.EmployeeCode = model.EmployeeCode;
                newData.FileContent = model.FileContent;

                base.Insert(newData);
                conn.Close();
                conn.Dispose();
            }
        }
        /// <summary>
        /// 紀錄傳送DI資料
        /// </summary>
        public void UpdateOFFICIAL_DOC_DI(DOC_DISaveModel model)
        {
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                this.conn = conn;
                OFFICIAL_DOC_DI where = new OFFICIAL_DOC_DI();
                where.APP_ID = model.APP_ID;
                where.SysID = model.SysID;
                OFFICIAL_DOC_DI newData = new OFFICIAL_DOC_DI();
                // 新公文介接欄位
                newData.Success = model.Success;
                newData.Token = model.Token;
                newData.ObjectId = model.ObjectId;
                newData.ErrorMsg = model.ErrorMsg;
                base.Update(newData, where);
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 紀錄傳送DI資料
        /// </summary>
        public void UpdateTOKEN(string APP_ID, string TOKEN)
        {
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                this.conn = conn;
                ApplyModel where = new ApplyModel();
                where.APP_ID = APP_ID;
                ApplyModel newData = new ApplyModel();
                // 新公文介接欄位
                newData.TOKEN = TOKEN;
                base.Update(newData, where);
                conn.Close();
                conn.Dispose();
            }
        }
        /// <summary>
        /// 更新中醫藥司流水號
        /// </summary>
        /// <param name="seq"></param>
        public void UpdateDraftByCreateSeqA10(string seq)
        {
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                this.conn = conn;
                var newSeq = seq.TOInt32() + 1;
                TblSETUP where = new TblSETUP();
                where.SETUP_CD = "DraftByCreateSeqA10";
                TblSETUP data = new TblSETUP();
                data.SETUP_VAL = newSeq.TONotNullString();
                data.UPD_ACC = sm.UserInfo.UserNo;
                data.UPD_TIME = DateTime.Now;
                data.UPD_FUN_CD = "ADM-DI";
                base.Update<TblSETUP>(data, where);
                conn.Close();
                conn.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cntNameAndTel">聯絡人及電話</param>
        /// <param name="cntFax">傳　　真</param>
        /// <param name="cntMail">電子郵件信箱</param>
        /// <param name="subject">主旨:案件項目名稱+線上申請+案件編號</param>
        public string CreateDI_PostIncoming(string cntName, string cntTel, string cntFax, string cntMail, string subject)
        {
            // 中醫藥司 A10
            XmlElement xe1, xe2, xe3;
            StringBuilder sb = new StringBuilder();
            //sb.Append("< !NOTATION DI SYSTEM \"\">");
            XmlDocument doc = new XmlDocument();
            doc.CreateComment("<!--  A10_1050000001.DI  104_1_utf8.dtd 令 2008.10.1 修改日期: 2015.05.01-- >");
            doc.XmlResolver = null; // 忽略DTD驗證
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            XmlDocumentType type = doc.CreateDocumentType("函", null, "104_1_utf8.dtd", sb.ToString());
            doc.AppendChild(type);

            // 建立根結點物件
            XmlElement root = doc.CreateElement("函");
            doc.AppendChild(root);

            // 發文機關
            xe1 = doc.CreateElement("發文機關");
            xe1.InnerText = "";

            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "衛生福利部";     // 固定寫入 "行政院衛生福利部"
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("機關代碼");
            xe2.InnerText = "A21000000I";

            root.AppendChild(xe1);

            // 函類別
            xe1 = doc.CreateElement("函類別");     // 固定文字
            xe1.SetAttribute("代碼", "函");
            root.AppendChild(xe1);

            // 地址
            xe1 = doc.CreateElement("地址");      // 固定文字
            xe1.InnerText = "115204 台北市南港區忠孝東路6段488號";
            root.AppendChild(xe1);

            // 聯絡方式
            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = $"聯絡人：{cntName.TONotNullString()}";
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = $"聯絡電話：{cntTel.TONotNullString()}";
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = $"傳真：{cntFax.TONotNullString()}";
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = $"電子郵件：{cntMail.TONotNullString()}";
            root.AppendChild(xe1);

            // 受文者
            xe1 = doc.CreateElement("受文者");
            xe1.InnerText = "";

            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "如正、副本行文單位";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("機關代碼");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 發文日期
            xe1 = doc.CreateElement("發文日期");
            xe1.InnerText = "";

            xe2 = doc.CreateElement("年月日");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 發文字號
            xe1 = doc.CreateElement("發文字號");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("字");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("文號");
            xe2.InnerText = "";

            xe3 = doc.CreateElement("年度");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);

            xe3 = doc.CreateElement("流水號");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);

            xe3 = doc.CreateElement("支號");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);

            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            // 速別
            xe1 = doc.CreateElement("速別");     // 固定文字
            xe1.InnerText = $"";
            xe1.SetAttribute("代碼", "普通件");
            root.AppendChild(xe1);

            // 密等及解密條件或保密期限
            xe1 = doc.CreateElement("密等及解密條件或保密期限");    // 密等固定文字
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("密等");
            xe2.InnerText = $"";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("解密條件或保密期限");
            xe2.InnerText = $"";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 附件
            xe1 = doc.CreateElement("附件");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("文字");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);
            // 主旨
            xe1 = doc.CreateElement("主旨");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("文字");      // 寫入申辦表單名稱，例如：產銷證明書
            xe2.InnerText = $"{subject.TONotNullString()}"; //案件項目名稱+線上申請+案件編號
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 段落
            xe1 = doc.CreateElement("段落");
            xe1.SetAttribute("代碼", "普通件");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("文字");
            xe2.InnerText = $"";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("條例");
            xe2.InnerText = $"";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 正本
            xe1 = doc.CreateElement("正本");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            //署名
            xe1 = doc.CreateElement("署名");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            var str = doc.OuterXml;

            // doc.Save($"C:\\e-service\\test.xml");

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cntNameAndTel">聯絡人及電話</param>
        /// <param name="cntFax">傳　　真</param>
        /// <param name="cntMail">電子郵件信箱</param>
        /// <param name="subject">主旨:案件項目名稱+線上申請+案件編號</param>
        public string CreateDI(string cntNameAndTel, string cntFax, string cntMail, string subject)
        {
            // 中醫藥司 A10
            XmlElement xe1, xe2, xe3;
            StringBuilder sb = new StringBuilder();
            //sb.Append("< !NOTATION DI SYSTEM \"\">");
            XmlDocument doc = new XmlDocument();
            doc.CreateComment("<!--  A10_1050000001.DI  104_1_big5.dtd 令 2008.10.1 修改日期: 2015.05.01-- >");
            doc.XmlResolver = null; // 忽略DTD驗證
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "BIG5", null));

            XmlDocumentType type = doc.CreateDocumentType("函", null, "104_1_big5.dtd", sb.ToString());
            doc.AppendChild(type);

            // 建立根結點物件
            XmlElement root = doc.CreateElement("函");
            doc.AppendChild(root);

            // 發文機關
            xe1 = doc.CreateElement("發文機關");
            xe1.InnerText = "";

            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "";     // 固定寫入 "行政院衛生福利部"
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 函類別
            xe1 = doc.CreateElement("函類別");     // 固定文字
            xe1.SetAttribute("代碼", "函");
            root.AppendChild(xe1);

            // 地址
            xe1 = doc.CreateElement("地址");      // 固定文字
            xe1.InnerText = "";
            root.AppendChild(xe1);

            // 聯絡方式
            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = $"聯絡人及電話：{cntNameAndTel.TONotNullString()}";
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = $"傳　　真：{cntFax.TONotNullString()}";
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = $"電子郵件信箱：{cntMail.TONotNullString()}";
            root.AppendChild(xe1);

            // 受文者
            xe1 = doc.CreateElement("受文者");     // 固定寫入 "如正本行文單位"
            xe1.InnerText = "如正本行文單位";
            root.AppendChild(xe1);

            // 發文日期
            xe1 = doc.CreateElement("發文日期");
            xe1.InnerText = "";

            xe2 = doc.CreateElement("年月日");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 發文字號
            xe1 = doc.CreateElement("發文字號");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("字");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("文號");
            xe2.InnerText = "";

            xe3 = doc.CreateElement("年度");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);

            xe3 = doc.CreateElement("流水號");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);

            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            // 速別
            xe1 = doc.CreateElement("速別");     // 固定文字
            xe1.InnerText = $"";
            xe1.SetAttribute("代碼", "普通件");
            root.AppendChild(xe1);

            // 密等及解密條件或保密期限
            xe1 = doc.CreateElement("密等及解密條件或保密期限");    // 密等固定文字
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("密等");
            xe2.InnerText = $"";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("解密條件或保密期限");
            xe2.InnerText = $"";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 附件
            xe1 = doc.CreateElement("附件");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("文字");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);
            // 主旨
            xe1 = doc.CreateElement("主旨");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("文字");      // 寫入申辦表單名稱，例如：產銷證明書
            xe2.InnerText = $"{subject.TONotNullString()}"; //案件項目名稱+線上申請+案件編號
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 正本
            xe1 = doc.CreateElement("正本");
            xe1.InnerText = $"";

            xe2 = doc.CreateElement("全銜");     // 單位
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("全銜");     // 部門
            xe2.InnerText = "";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            //署名
            xe1 = doc.CreateElement("署名");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            //承辦單位
            xe1 = doc.CreateElement("承辦單位");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            //年度/分類號
            xe1 = doc.CreateElement("年度分類號");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            //保存年限
            xe1 = doc.CreateElement("保存年限");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            //分層負責代碼
            xe1 = doc.CreateElement("分層負責代碼");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            //決行層次
            xe1 = doc.CreateElement("決行層次");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            //發文方式
            xe1 = doc.CreateElement("發文方式");
            xe1.InnerText = $"";
            root.AppendChild(xe1);

            var str = doc.OuterXml;

            // doc.Save($"C:\\e-service\\test.xml");

            return str;
        }
        #endregion

        #region 取得截止日期

        /// <summary>
        /// 取得截止日期(true=>鎖定，false=>不鎖定)
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <param name="APP_ID"></param>
        public DateTime CalDate(string SRV_ID, string APP_ID)
        {
            DateTime noticeTime = new DateTime();//通知補件日期
            DateTime noticeEndTime = new DateTime();//截止日期
            int docDay = 0;//補件天數
            string docType = "";//補件型態(工作天/日曆天)

            if (SRV_ID == null)
            {
                logger.Warn("##CalDate, SRV_ID is null !");
                return noticeEndTime;
            }
            if (APP_ID == null)
            {
                logger.Warn(string.Format("##CalDate, SRV_ID: {0} ", SRV_ID));
                logger.Warn("##CalDate, APP_ID is null !");
                return noticeEndTime;
            }

            #region 取得補件資訊
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    //取得補件天數
                    string sql = "";
                    if (SRV_ID.Substring(0, 3) == "005")
                    {
                        sql = @"select TOP 1 convert(varchar,DATEADD(day, 1, convert(varchar,ADD_TIME,112)),111) ADD_TIME
                                from APPLY_NOTICE 
                                where APP_ID=@APP_ID
                                order by ADD_TIME desc";
                    }
                    else
                    {
                        sql = @"select convert(varchar,DATEADD(day, 1, convert(varchar,APPLY_NOTICE_DATE,112)),111) APPLY_NOTICE_DATE 
                                from APPLY
                                where APP_ID=@APP_ID";
                    }

                    DataTable temp = new DataTable();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("APP_ID", SqlDbType.VarChar).Value = APP_ID;
                    temp.Load(cmd.ExecuteReader());
                    if (temp.Rows.Count > 0) { noticeTime = Convert.ToDateTime(temp.Rows[0][0].ToString()); }
                    temp.Reset();

                    string s_CODEKIND = string.Format("F_NOTICE_{0}", SRV_ID);
                    //取得補件資訊
                    sql = @"select CODE_CD,SEQ_NO
                            from CODE_CD
                            where CODE_KIND=@CODE_KIND";
                    DataTable temp2 = new DataTable();
                    SqlCommand cmd2 = new SqlCommand(sql, conn);
                    cmd2.Parameters.Clear();
                    cmd2.Parameters.Add("CODE_KIND", SqlDbType.VarChar).Value = s_CODEKIND;
                    temp2.Load(cmd2.ExecuteReader());
                    if (temp2.Rows.Count > 0)
                    {
                        docDay = Convert.ToInt32(temp2.Rows[0]["CODE_CD"].ToString());
                        docType = temp2.Rows[0]["SEQ_NO"].ToString();
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
            #endregion

            #region 截止日期計算
            noticeEndTime = noticeTime.AddDays(docDay);
            //docType == "10" : 工作天
            if (docType == "10")
            {
                //計算截止期限
                for (var i = 0; i < docDay; i++)
                {
                    if (IsHoliday(noticeTime.AddDays(i).ToString("yyyyMMdd")))
                    {
                        noticeEndTime = noticeEndTime.AddDays(1);
                    }
                }
            }
            #endregion

            return noticeEndTime;
        }
        #endregion

        #region 補件案件是否鎖定

        /// <summary>
        /// 補件案件是否鎖定(true=>鎖定，false=>不鎖定)
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <param name="APP_ID"></param>
        public bool CalculationDocDate(string SRV_ID, string APP_ID)
        {

            DateTime? NULLnoticeTime = null;
            DateTime noticeTime = new DateTime();//通知補件日期
            DateTime noticeEndTime = DateTime.Today;//截止日期
            int docDay = 0;//補件天數
            string docType = "";//補件型態(工作天/日曆天)

            #region 取得補件資訊
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    //取得補件天數
                    string sql = "";
                    if (SRV_ID.Substring(0, 3) == "005")
                    {
                        //datetime null
                        sql = @"
 select TOP 1 convert(date, DATEADD(day, 1, ADD_TIME)) ADD_TIME  
 from APPLY_NOTICE 
 where ADD_TIME is not null
 and APP_ID= @APP_ID
 order by ADD_TIME desc";
                    }
                    else
                    {
                        //datetime null
                        sql = @"
 select TOP 1 convert(date, DATEADD(day, 1, APPLY_NOTICE_DATE)) APPLY_NOTICE_DATE 
 from APPLY
 where APPLY_NOTICE_DATE is not null
 and APP_ID= @APP_ID 
 order by APPLY_NOTICE_DATE desc";
                    }
                    DataTable temp = new DataTable();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("APP_ID", SqlDbType.VarChar).Value = APP_ID;
                    temp.Load(cmd.ExecuteReader());
                    if (temp.Rows.Count > 0) { NULLnoticeTime = HelperUtil.ConvertDateTime(temp.Rows[0][0]); }
                    temp.Reset();

                    //取得補件資訊
                    docDay = this.GetFNoticeDays(SRV_ID);
                    docType = this.GetFNoticeDaysType(SRV_ID);
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
            #endregion

            #region 比較截止日期與今日
            //截止日期計算
            noticeEndTime = this.GetFNoticeDateLine(docDay, docType, NULLnoticeTime);

            int T1 = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd").ToString());
            int T2 = Convert.ToInt32(noticeEndTime.AddDays(1).ToString("yyyyMMdd").ToString());

            if (T1 - T2 >= 0)
            {
                return true;//需鎖定，不可補件
            }
            else
            {
                return false;//不鎖定，可補件
            }
            #endregion
        }

        public bool IsHoliday(string someDay)
        {
            DataTable temp = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                //取得補件天數
                string sql = @"
                select * from HOLIDAY where format(StartDate,'yyyyMMdd')=@StartDate";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Clear();
                cmd.Parameters.Add("StartDate", SqlDbType.VarChar).Value = someDay;
                temp.Load(cmd.ExecuteReader());
                //SqlDataAdapter da = new SqlDataAdapter(cmd);
                //da.Fill(temp);
                //try { } catch (Exception ex) { logger.Warn(ex.Message, ex); }
                conn.Close();
                conn.Dispose();
            }
            return (temp != null && temp.Rows.Count > 0) ? true : false;
        }

        /// <summary>
        /// 取得補件天數
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <param name="APP_ID"></param>
        public int GetFNoticeDays(string SRV_ID)
        {
            int docDay = 0;//補件天數
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    //取得補件資訊
                    string code_kind = string.Format("F_NOTICE_{0}", SRV_ID);
                    string sql2 = @" SELECT CODE_CD,SEQ_NO FROM CODE_CD where CODE_KIND=@CODE_KIND ";
                    SqlCommand cmd2 = new SqlCommand(sql2, conn);
                    cmd2.Parameters.Clear();
                    cmd2.Parameters.Add("CODE_KIND", SqlDbType.VarChar).Value = code_kind;
                    DataTable temp2 = new DataTable();
                    temp2.Load(cmd2.ExecuteReader());
                    if (temp2.Rows.Count > 0)
                    {
                        bool rst = int.TryParse(temp2.Rows[0]["CODE_CD"].TONotNullString(), out docDay);
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
            return docDay;
        }

        /// <summary>
        /// 取得補件天數類型
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <param name="APP_ID"></param>
        public string GetFNoticeDaysType(string SRV_ID)
        {
            string docType = "";//補件型態(工作天/日曆天)
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    //取得補件資訊
                    string code_kind = string.Format("F_NOTICE_{0}", SRV_ID);
                    string sql2 = @" SELECT CODE_CD,SEQ_NO FROM CODE_CD where CODE_KIND=@CODE_KIND ";
                    SqlCommand cmd2 = new SqlCommand(sql2, conn);
                    cmd2.Parameters.Clear();
                    cmd2.Parameters.Add("CODE_KIND", SqlDbType.VarChar).Value = code_kind;
                    DataTable temp2 = new DataTable();
                    temp2.Load(cmd2.ExecuteReader());
                    if (temp2.Rows.Count > 0)
                    {
                        docType = temp2.Rows[0]["SEQ_NO"].TONotNullString();
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
            return docType;
        }

        /// <summary>
        /// 取得補件截止日期
        /// </summary>
        /// <param name="docDay">補件天數</param>
        /// <param name="docType">補件類型(工作天(10)/日曆天(20))</param>
        public DateTime GetFNoticeDateLine(int docDay, string docType, DateTime? NULLnoticeTime)
        {
            DateTime noticeTime = new DateTime();//通知補件日期
            DateTime noticeEndTime = DateTime.Today;//截止日期
            if (NULLnoticeTime.HasValue)
            {
                noticeTime = NULLnoticeTime.Value;
                noticeEndTime = noticeTime.AddDays(docDay);
            }

            //docType == "10" : 工作天
            if (docType == "10")
            {
                //計算截止期限
                for (var i = 0; i < docDay; i++)
                {
                    if (IsHoliday(noticeTime.AddDays(i).ToString("yyyyMMdd")))
                    {
                        noticeEndTime = noticeEndTime.AddDays(1);
                    }
                }
            }
            return noticeEndTime;
        }

        /// <summary>
        /// 取得中醫藥司需補件案件
        /// </summary>
        /// <returns></returns>
        public List<string> GetFNoticeAPPID005()
        {
            var result = new List<string>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    //取得補件案件
                    string sql = "";
                    sql = @"
                          SELECT DISTINCT APP_ID+','+ convert(varchar,convert(date, DATEADD(day, 1, ADD_TIME))) APP_ID
                         FROM APPLY_NOTICE 
                         WHERE ADD_TIME IS NOT NULL
                         AND ISADDYN = 'N'
                         AND (APP_ID LIKE '%005001%' OR APP_ID LIKE '%005002%' OR APP_ID LIKE '%005003%'
						 OR APP_ID LIKE '%005004%' OR APP_ID LIKE '005005' OR APP_ID LIKE '005013' 
						 OR APP_ID LIKE '%005014%')
                         AND ADD_TIME > '2021-08-01'";

                    DataTable DT = new DataTable();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Clear();
                    DT.Load(cmd.ExecuteReader());
                    if (DT.Rows.Count > 0)
                    {
                        for (var i = 0; i < DT.Rows.Count; i++)
                        {
                            result.Add(DT.Rows[i][0].TONotNullString());
                        }
                    }
                    DT.Reset();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
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

        /// <summary>
        /// 取得申辦項目代號中文
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public string GetServiceName(string SRV_ID)
        {
            var rst = "";
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select NAME
                                 from service 
                                 where SRV_ID='" + SRV_ID + "' ";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        /// <summary>
        /// 取得申辦項目單位中文
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public string GetServiceUnit(string SRV_ID)
        {
            var rst = "";
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" SELECT B.UNIT_NAME FROM SERVICE A
LEFT JOIN UNIT B ON A.FIX_UNIT_CD = B.UNIT_CD
WHERE SRV_ID = '" + SRV_ID + @"'
ORDER BY SRV_ID ";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        /// <summary>
        /// 取得申辦項目單位代號
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public string GetServiceUnitCD(string SRV_ID)
        {
            string rst = "";
            DataTable _dt = new DataTable();
            string _sql = @"
 SELECT B.UNIT_CD FROM SERVICE A
 LEFT JOIN UNIT B ON A.FIX_UNIT_CD = B.UNIT_CD
 WHERE SRV_ID = @SRV_ID 
 ORDER BY SRV_ID ";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    dbc.Parameters.Clear();
                    dbc.Parameters.Add("SRV_ID", SqlDbType.VarChar).Value = SRV_ID;
                    _dt.Load(dbc.ExecuteReader());
                    //_da.SelectCommand = dbc;
                    //_da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        /// <summary>
        /// 取得後台使用者名稱
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public string GetAdmin(string ACC_NO)
        {
            var rst = ACC_NO.TONotNullString();
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" SELECT A.NAME FROM ADMIN A
 WHERE 1=1 AND A.ACC_NO ='" + ACC_NO + "'";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        /// <summary>
        /// 取得主機檔案儲存位置
        /// </summary>
        public string GetServerLocalPath()
        {
            var rst = string.Empty;
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @"select SETUP_VAL from setup where SETUP_CD='FOLDER_APPLY_FILE'";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].ToString().Replace('\\', '/'); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        /// <summary>
        /// 取的該案件的所有檔案
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public List<FileGroupModel> GetFileGridList(string APP_ID)
        {
            var result = new List<FileGroupModel>();
            Apply_FileModel where = new Apply_FileModel();
            where.APP_ID = APP_ID;
            var grids = this.GetRowList<Apply_FileModel>(where);
            if (grids != null && grids.Count > 0)
            {
                try
                {
                    foreach (var item in grids.OrderByDescending(x => x.UPD_TIME))
                    {
                        var itemRp = item.FILENAME.Replace('/', '\\');
                        var itemNum = itemRp.LastIndexOf('\\');
                        var filename = itemRp.Substring(itemNum + 1, item.FILENAME.Length - itemNum - 1);
                        FileGroupModel data = new FileGroupModel();
                        data.FILE_NAME = filename;
                        data.FILE_NAME_TEXT = $"{filename},{item.APP_ID},{item.FILE_NO},{item.SRC_NO}";
                        data.SEQ = item.FILE_NO.ToString();
                        result.Add(data);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            }
            return result;
        }

        public List<FileGroupModel> GetFileGridList(string APP_ID, string FILE_NO, string isBackmin = "Y")
        {
            var result = new List<FileGroupModel>();
            Apply_File_LogModel where = new Apply_File_LogModel();
            where.APP_ID = APP_ID;
            where.FILE_NO = FILE_NO.TOInt32();
            var grids = new List<Apply_File_LogModel>();
            if (isBackmin == "Y")
            {
                // 後台取得多檔
                grids = this.GetRowList<Apply_File_LogModel>(where).ToList();
            }
            else
            {
                // 前台顯示單檔
                var alllist = this.GetRowList<Apply_File_LogModel>(where).ToList().Where(x => x.BATCH_INDEX.TOInt32() == 0 || x.BATCH_INDEX == null).ToList();
                if (alllist != null && alllist.Count() > 0)
                {
                    grids.Add(alllist.OrderByDescending(x => x.MODTIME).FirstOrDefault());
                }
            }
            var noticeDates = this.GetRowList<TblAPPLY_NOTICE>(new TblAPPLY_NOTICE() { APP_ID = APP_ID });
            if (noticeDates != null)
            {
                // 曾經的補件通知
                var dates = noticeDates.OrderBy(x => x.FREQUENCY).OrderBy(x => x.ADD_TIME).ToList();

                if (grids != null && grids.Count > 0)
                {
                    try
                    {
                        foreach (var item in grids.OrderByDescending(x => x.MODTIME).ToList())
                        {
                            if (string.IsNullOrEmpty(item.FILENAME)) continue;
                            if (item.BATCH_INDEX.TOInt32() == 0)
                            {
                                var itemRp = item.FILENAME.Replace('/', '\\');
                                var itemNum = itemRp.LastIndexOf('\\');
                                var filename = itemRp.Substring(itemNum + 1, item.FILENAME.Length - itemNum - 1);
                                FileGroupModel data = new FileGroupModel();
                                data.FILE_NAME = filename;
                                data.FILE_NAME_TEXT = $"{filename},{item.APP_ID},{item.FILE_NO},{item.SRC_NO}";
                                data.SEQ = item.FILE_NO.ToString();
                                data.SRC = item.FILENAME;
                                data.SRC_FILENAME = item.SRC_FILENAME;
                                data.NOTICE_NO = "0";
                                var NoticeNum = dates.Select(x => x.FREQUENCY).Distinct().Count();
                                for (var i = 0; i < NoticeNum; i++)
                                {
                                    var num = i + 1;
                                    var nextNum = i + 2;
                                    // FREQUENCY NUM
                                    var dateItem = dates.Where(x => x.FREQUENCY == num).Select(x => x.ADD_TIME).OrderByDescending(x => x);
                                    var betweenDate = dateItem.FirstOrDefault();
                                    var nextDataItem = dates.Where(x => x.FREQUENCY == nextNum).Select(x => x.ADD_TIME).OrderByDescending(x => x);
                                    var andDate = nextDataItem.FirstOrDefault();
                                    if (item.UPD_TIME < betweenDate)
                                    {
                                        // 第一次上傳檔案
                                        data.NOTICE_NO = "0";
                                        break;
                                    }
                                    else if (i == NoticeNum - 1 && betweenDate < item.UPD_TIME)
                                    {
                                        // 最後一次補件時間
                                        data.NOTICE_NO = (num).ToString();
                                        break;
                                    }
                                    else if (betweenDate < item.UPD_TIME && item.UPD_TIME < andDate)
                                    {
                                        // 中間補件時間
                                        data.NOTICE_NO = (num).ToString();
                                        break;
                                    }
                                }
                                result.Add(data);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message, ex);
                    }
                }
            }
            else
            {
                // 沒有apply_notice資料
                if (grids != null && grids.Count > 0)
                {
                    try
                    {
                        foreach (var item in grids.OrderByDescending(x => x.MODTIME).ToList())
                        {
                            if (string.IsNullOrEmpty(item.FILENAME)) continue;
                            if (item.BATCH_INDEX.TOInt32() == 0)
                            {
                                var itemRp = item.FILENAME.Replace('/', '\\');
                                var itemNum = itemRp.LastIndexOf('\\');
                                var filename = itemRp.Substring(itemNum + 1, item.FILENAME.Length - itemNum - 1);
                                FileGroupModel data = new FileGroupModel();
                                data.FILE_NAME = filename;
                                data.FILE_NAME_TEXT = $"{filename},{item.APP_ID},{item.FILE_NO},{item.SRC_NO}";
                                data.SEQ = item.FILE_NO.ToString();
                                data.SRC = item.FILENAME;
                                data.SRC_FILENAME = item.SRC_FILENAME;
                                data.NOTICE_NO = "0";
                                result.Add(data);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message, ex);
                    }
                }
            }
            return result;
        }
        public List<FileGroupModel> GetFileGridList(string APP_ID, string FILE_NO, string SRC_NO, string BATCH_INDEX, string isBackmin = "Y")
        {
            var result = new List<FileGroupModel>();
            Apply_File_LogModel where = new Apply_File_LogModel();
            where.APP_ID = APP_ID;
            where.FILE_NO = FILE_NO.TOInt32();
            where.SRC_NO = SRC_NO.TOInt32();
            where.BATCH_INDEX = BATCH_INDEX.TOInt32();
            var grids = new List<Apply_File_LogModel>();
            if (isBackmin == "Y")
            {
                // 後台取得多檔
                grids = this.GetRowList<Apply_File_LogModel>(where).ToList();
            }
            else
            {
                // 前台顯示單檔
                var alllist = this.GetRowList<Apply_File_LogModel>(where).ToList();
                if (alllist != null && alllist.Count() > 0)
                {
                    grids.Add(alllist.OrderByDescending(x => x.MODTIME).FirstOrDefault());
                }
            }
            var noticeDates = this.GetRowList<TblAPPLY_NOTICE>(new TblAPPLY_NOTICE() { APP_ID = APP_ID });
            if (noticeDates != null)
            {
                // 曾經的補件通知
                var dates = noticeDates.OrderBy(x => x.FREQUENCY).OrderBy(x => x.ADD_TIME).ToList();

                if (grids != null && grids.Count > 0)
                {
                    try
                    {
                        foreach (var item in grids.OrderByDescending(x => x.MODTIME).ToList())
                        {
                            if (string.IsNullOrEmpty(item.FILENAME)) continue;
                            var itemRp = item.FILENAME.Replace('/', '\\');
                            var itemNum = itemRp.LastIndexOf('\\');
                            var filename = itemRp.Substring(itemNum + 1, item.FILENAME.Length - itemNum - 1);
                            FileGroupModel data = new FileGroupModel();
                            data.FILE_NAME = filename;
                            data.FILE_NAME_TEXT = $"{filename},{item.APP_ID},{item.FILE_NO},{item.SRC_NO},{item.BATCH_INDEX}";
                            data.SEQ = item.FILE_NO.ToString();
                            data.SRC = item.FILENAME;
                            data.SRC_FILENAME = item.SRC_FILENAME;
                            data.BATCH_INDEX = item.BATCH_INDEX.TONotNullString();
                            data.NOTICE_NO = "0";
                            var NoticeNum = dates.Select(x => x.FREQUENCY).Distinct().Count();
                            for (var i = 0; i < NoticeNum; i++)
                            {
                                var num = i + 1;
                                var nextNum = i + 2;
                                // FREQUENCY NUM
                                var dateItem = dates.Where(x => x.FREQUENCY == num).Select(x => x.ADD_TIME).OrderByDescending(x => x);
                                var betweenDate = dateItem.FirstOrDefault();
                                var nextDataItem = dates.Where(x => x.FREQUENCY == nextNum).Select(x => x.ADD_TIME).OrderByDescending(x => x);
                                var andDate = nextDataItem.FirstOrDefault();
                                if (item.UPD_TIME < betweenDate)
                                {
                                    // 第一次上傳檔案
                                    data.NOTICE_NO = "0";
                                    break;
                                }
                                else if (i == NoticeNum - 1 && betweenDate < item.UPD_TIME)
                                {
                                    // 最後一次補件時間
                                    data.NOTICE_NO = (num).ToString();
                                    break;
                                }
                                else if (betweenDate < item.UPD_TIME && item.UPD_TIME < andDate)
                                {
                                    // 中間補件時間
                                    data.NOTICE_NO = (num).ToString();
                                    break;
                                }
                            }
                            result.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message, ex);
                    }
                }
            }
            else
            {
                // 沒有apply_notice資料
                if (grids != null && grids.Count > 0)
                {
                    try
                    {
                        foreach (var item in grids.OrderByDescending(x => x.MODTIME).ToList())
                        {
                            if (string.IsNullOrEmpty(item.FILENAME)) continue;
                            var itemRp = item.FILENAME.Replace('/', '\\');
                            var itemNum = itemRp.LastIndexOf('\\');
                            var filename = itemRp.Substring(itemNum + 1, item.FILENAME.Length - itemNum - 1);
                            FileGroupModel data = new FileGroupModel();
                            data.FILE_NAME = filename;
                            data.FILE_NAME_TEXT = $"{filename},{item.APP_ID},{item.FILE_NO},{item.SRC_NO},{item.BATCH_INDEX}";
                            data.SEQ = item.FILE_NO.ToString();
                            data.SRC = item.FILENAME;
                            data.SRC_FILENAME = item.SRC_FILENAME;
                            data.BATCH_INDEX = item.BATCH_INDEX.TONotNullString();
                            data.NOTICE_NO = "0";
                            result.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message, ex);
                    }
                }
            }
            return result;
        }
        public List<FileGroupModel> GetFileGridListTop(string APP_ID)
        {
            var result = new List<FileGroupModel>();
            Apply_File_LogModel where = new Apply_File_LogModel();
            where.APP_ID = APP_ID;
            var grids = this.GetRowList<Apply_File_LogModel>(where);
            var items = new List<string>();
            if (grids != null && grids.Count > 0)
            {
                try
                {
                    foreach (var item in grids.OrderByDescending(x => x.UPD_TIME))
                    {
                        if (!string.IsNullOrEmpty(item.FILENAME))
                        {
                            if (items.Contains(item.FILENAME.ToSplit("_").LastOrDefault()))
                            {
                                // next
                            }
                            else
                            {
                                var itemRp = item.FILENAME.Replace('/', '\\');
                                var itemNum = itemRp.LastIndexOf('\\');
                                var filename = itemRp.Substring(itemNum + 1, item.FILENAME.Length - itemNum - 1);
                                FileGroupModel data = new FileGroupModel();
                                data.FILE_NAME = filename;
                                data.FILE_NAME_TEXT = $"{filename},{item.APP_ID},{item.FILE_NO},{item.SRC_NO}";
                                data.SEQ = item.FILE_NO.ToString();
                                data.SRC = item.FILENAME.Replace('/', '\\');
                                result.Add(data);
                                items.Add(item.FILENAME.ToSplit("_").LastOrDefault());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            }
            return result;
        }
        /// <summary>
        /// 取得附件清單資料(for 005014 用)
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public List<FileGroupModel> GetFileGridListTop_005014(string APP_ID)
        {
            var result = new List<FileGroupModel>();
            Apply_005014_FILE where = new Apply_005014_FILE();
            where.APP_ID = APP_ID;
            var grids = this.GetRowList<Apply_005014_FILE>(where);
            var items = new List<string>();
            if (grids != null && grids.Count > 0)
            {
                try
                {
                    foreach (var item in grids.OrderByDescending(x => x.CREATE_DATE))
                    {
                        var itemRp = item.FILE_NAME.Replace('/', '\\');
                        var itemNum = item.FILE_ID.ToString();
                        var filename = item.FILE_NAME.ToSplit('/').LastOrDefault();
                        FileGroupModel data = new FileGroupModel();
                        data.FILE_NAME = filename;
                        data.FILE_NAME_TEXT = $"{APP_ID}-{itemNum}-{filename}";
                        data.SEQ = item.FILE_ID.ToString();
                        data.SRC = item.FILE_URL.Replace('/', '\\');
                        result.Add(data);
                        items.Add(item.FILE_NAME.ToSplit("_").LastOrDefault());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            }
            return result;
        }

        /// <summary>
        /// 取得附件清單資料(for 001008 醫事人員請領英文證明書-醫事人員中文證書電子檔用)
        /// </summary>
        /// <param name="APP_ID">案件編號</param>
        /// <param name="FILE_NO">檔案序號</param>
        /// <param name="isBackmin">前後台判斷Y:後台,N:前台</param>
        /// <returns></returns>
        public List<FileGroupModel> GetAthFileGridList(string APP_ID, string SRL_NO, string isBackmin = "Y")
        {
            List<FileGroupModel> result = new List<FileGroupModel>();
            Apply_001008_Ath_Log athLogWhere = new Apply_001008_Ath_Log();
            athLogWhere.APP_ID = APP_ID;
            athLogWhere.SRL_NO = SRL_NO.TOInt32();

            IList<Apply_001008_Ath_Log> grids = new List<Apply_001008_Ath_Log>();

            int iPos = -1;
            string fileName = "";

            if (isBackmin == "Y")
            {
                //從 log table 取該檔案序號的所有歷程檔案資訊
                grids = this.GetRowList<Apply_001008_Ath_Log>(athLogWhere).ToList();
            }
            else
            {
                //從 log table 取該檔案序號最後一次上傳的檔案資訊
                var ath = this.GetRowList<Apply_001008_Ath_Log>(athLogWhere).ToList().OrderByDescending(x => x.NOTICE_NO).FirstOrDefault();
                grids.Add(ath);
            }

            if (grids != null)
            {
                try
                {
                    foreach (var item in grids.OrderByDescending(x => x.NOTICE_NO))
                    {
                        iPos = item.ATH_UP.LastIndexOf("\\");
                        fileName = item.ATH_UP.Substring(iPos + 1, item.ATH_UP.Length - iPos - 1);

                        FileGroupModel data = new FileGroupModel();
                        data.FILE_NAME = fileName;
                        data.FILE_NAME_TEXT = $"{fileName},{item.APP_ID},{item.SRL_NO},{Convert.ToString(item.NOTICE_NO)}";
                        data.SEQ = Convert.ToString(item.SRL_NO);
                        data.SRC = item.ATH_UP;
                        data.SRC_FILENAME = item.SRC_FILENAME;
                        data.NOTICE_NO = (item.NOTICE_NO - 1).ToString();
                        result.Add(data);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            }

            return result;
        }

        /// <summary>
        /// 查詢通知補件項目
        /// </summary>
        /// <param name="APP_ID">案件編號</param>
        /// <param name="FILE_NOs">用逗號分隔</param>
        /// <returns></returns>
        public string GetApplyNoticePaper(string APP_ID, string FILE_NOs)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(FILE_NOs))
            {
                var items = FILE_NOs.ToSplit(',');
                TblAPPLY_NOTICE where = new TblAPPLY_NOTICE();
                where.APP_ID = APP_ID;
                where.ISADDYN = "N";
                var data = this.GetRowList(where);
                if (data != null && data.Count > 0)
                {
                    // 若有其他補件項目，通知補件 狀態欄位不能編修
                    var other5 = data.Where(x => x.Field == "OTHER_5").FirstOrDefault();
                    var other6 = data.Where(x => x.Field == "OTHER_6").FirstOrDefault();
                    var cnt = 0;
                    if (other5 != null && !string.IsNullOrWhiteSpace(other5.Field))
                    {
                        cnt++;
                    }
                    if (other6 != null && !string.IsNullOrWhiteSpace(other6.Field))
                    {
                        cnt++;
                    }
                    result = (data.Count > cnt) ? "N" : "Y";
                }
            }
            return result;
        }

        #region 繳費紀錄檔案上傳

        public ApplyModel GetApplyData(ApplyModel model)
        {
            ApplyModel result = new ApplyModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                this.conn = conn;
                try
                {
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    result = GetRow(whereApply);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    throw new Exception("GetApplyData failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }
        public void SavePayFile(ULPayFileFormModel form)
        {
            SessionModel sm = SessionModel.Get();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.conn = conn;
                this.tran = tran;
                try
                {
                    Apply_FileModel file = new Apply_FileModel();
                    file.APP_ID = form.APP_ID;
                    file.FILE_NO = form.FILE_NO.TOInt32();
                    file.FILENAME = this.PutFile(form.SRV_ID, form.ATTACH_FILE, form.FILE_NO);
                    file.SRC_FILENAME = form.ATTACH_FILE_NAME;
                    file.ADD_TIME = DateTime.Now;
                    file.ADD_FUN_CD = "WEB-APPLY";
                    file.ADD_ACC = sm.UserInfo.Member.ACC_NO.TONotNullString();
                    file.UPD_TIME = DateTime.Now;
                    file.UPD_FUN_CD = "WEB-APPLY";
                    file.UPD_ACC = sm.UserInfo.Member.ACC_NO.TONotNullString();
                    file.DEL_MK = "N";
                    base.Insert(file);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("ULPayFileOnHistory failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public void UpdateApplyNotice(string APP_ID, string SRV_ID)
        {
            SessionModel sm = SessionModel.Get();
            string s_SRV_ID = SRV_ID;
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            string FUN_CD = "WEB-APPLY";
            ClamMember UserInfo = sm.UserInfo.Member;

            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", APP_ID);
            dict2.Add("SRV_ID", s_SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.conn = conn;
                this.tran = tran;
                try
                {

                    //申請案主檔
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = APP_ID;
                    ApplyModel newApply = new ApplyModel();
                    newApply.APP_ID = APP_ID;
                    newApply.UPD_TIME = DateTime.Now;
                    newApply.UPD_ACC = UserInfo.ACC_NO.TONotNullString();
                    newApply.UPD_FUN_CD = FUN_CD;
                    newApply.FLOW_CD = "3"; //註記補件收件
                    base.Update2(newApply, whereApply, dict2); //for log歷程顯示前置作業用

                    TblAPPLY_NOTICE applyNotice = new TblAPPLY_NOTICE();
                    applyNotice.APP_ID = APP_ID;
                    applyNotice.ISADDYN = "Y";
                    TblAPPLY_NOTICE whereApplyNotice = new TblAPPLY_NOTICE();
                    whereApplyNotice.APP_ID = APP_ID;
                    whereApplyNotice.ISADDYN = "N";

                    //base.Update(applyNotice, whereApplyNotice);
                    base.Update2(applyNotice, whereApplyNotice, dict2); //for log歷程顯示前置作業用

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("ULPayFileOnHistory failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public void SendMail_Update(string ACC_NAM, string Mail, string APP_ID, string Srv_ID, string Count, string CustomMailBody = "", bool ISSEND = true, bool ISBACK = false)
        {
            ShareDAO dao = new ShareDAO();
            MailMessage mailMessage;
            string notReplyMsg = "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。";

            #region 申請人

            // 信件標題
            string subject = "衛生福利部人民申請案件線上申辦系統，案件編號﹕" + APP_ID + " 補件完成通知";
            // 信件內容
            string MailBody = "<table align=\"left\" style=\"width:90%;\">";
            MailBody += " <tr><th align=\"left\">" + ACC_NAM + "，您好:</th></tr>";
            // 客製化信件內容
            switch (Srv_ID)
            {
                #region 社工司
                case "011002":
                case "011005":
                case "011006":
                    MailBody += " <tr><td>您申請" + APP_ID + "，已完成資料補件，將盡速辦理您的申辦案件，謝謝。</td> </tr>";
                    break;
                #endregion

                #region 醫藥司
                case "001005":
                case "001007":
                case "001009":
                case "001010":
                case "001036":
                    MailBody += " <tr><td>您申請" + APP_ID + "，已完成資料補件，將盡速辦理您的申辦案件，謝謝。</td> </tr>";
                    break;
                #endregion

                default:
                    MailBody += " <tr><td>您申請" + APP_ID + "，已完成資料補件，將盡速辦理您的申辦案件，謝謝。</td> </tr>";
                    break;
            }
            switch (Srv_ID)
            {
                case "001036":
                    MailBody += "<tr><td>衛生福利部護理及健康照護司敬上<br/><br/></td> </tr>";
                    break;
                case "005001":
                case "005002":
                case "005003":
                case "005004":
                case "005005":
                case "005013":
                case "005014":
                    MailBody += "<tr><td>衛生福利部中醫藥司敬上<br/><br/></td></tr>";
                    break;
                case "010001":
                case "010002":
                    MailBody += "<tr><td>衛生福利部國民健康署敬上<br/><br/></td> </tr>";
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
                    MailBody += "<tr><td>衛生福利部社會救助及社工司<br/><br/></td></tr>";
                    break;
                case "012001":
                    MailBody += "<tr><td>衛生福利部秘書處敬上<br/><br/></td></tr>";
                    break;
                default:
                    MailBody += "<tr><td>衛生福利部醫事司敬上<br/><br/></td></tr>";
                    break;
            }
            MailBody += " <tr><td><br>" + notReplyMsg + "</td></tr>";
            MailBody += "</table>";

            // 中醫藥司會取代此段
            if (CustomMailBody != "")
            {
                MailBody = CustomMailBody;
            }

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
                        mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
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
                        mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, subject, MailBody);
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

                #endregion

                if (ISSEND)
                {
                    #region 承辦人
                    IList<SpecialistMailModel> mailList = new List<SpecialistMailModel>();
                    if (ISBACK)
                    {
                        // 中醫藥司 補件收件返回新收案件，重新取得分文承辦人
                        mailList = dao.getSpecialist(Srv_ID);
                        if (mailList == null) { return; }
                    }
                    else
                    {
                        if (Srv_ID.Substring(0, 3) == "011")
                        {
                            mailList = dao.getSpecialist(Srv_ID);
                        }
                        else
                        {
                            ApplyModel app = new ApplyModel();
                            app.APP_ID = APP_ID;
                            var getapp = dao.GetRow(app);

                            AdminModel adm = new AdminModel();
                            adm.ACC_NO = getapp.PRO_ACC.TONotNullString();
                            var getadm = dao.GetRow(adm);
                            mailList.Add(new SpecialistMailModel()
                            {
                                NAME = getadm.NAME,
                                EMAIL = getadm.MAIL
                            });
                        }
                    }
                    if (mailList != null && mailList.Count > 0)
                    {
                        foreach (var item in mailList)
                        {
                            subject = "衛生福利部人民申請案件線上申辦系統，案件編號﹕" + APP_ID + " 補件完成通知";
                            MailBody = item.NAME + " 您好：<br/>";
                            // 客製化信件內容
                            switch (Srv_ID)
                            {
                                #region 社工司
                                case "011002":
                                case "011005":
                                case "011006":
                                    MailBody += "您有案件編號﹕" + APP_ID + "，待處理，已完成資料補件!!<br/>";
                                    break;
                                #endregion

                                default:
                                    MailBody += "您有案件編號﹕" + APP_ID + "，待處理，已完成資料補件!!<br/>";
                                    break;
                            }

                            // 中醫藥司會取代此段
                            if (CustomMailBody != "")
                            {
                                MailBody = CustomMailBody;
                            }

                            MailBody += "<br>" + notReplyMsg + "<br/>";

                            // 寄信LOG
                            TblMAIL_LOG log2 = new TblMAIL_LOG();
                            log2.MAIL = item.EMAIL;
                            log2.SUBJECT = subject;
                            log2.BODY = MailBody;
                            log2.SEND_TIME = DateTime.Now;
                            log2.SRV_ID = Srv_ID;
                            try
                            {
                                if (ConfigModel.MailRevTest == "1")
                                {
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, subject, MailBody);
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
                                    mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, item.EMAIL, subject, MailBody);
                                    mailMessage.IsBodyHtml = true;
                                    CommonsServices.SendMail(mailMessage);
                                }

                                // 寄信成功
                                log2.RESULT_MK = "Y";
                                Insert(log2);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("SendMail_Update failed:" + ex.TONotNullString());
                                // 寄信失敗
                                log2.RESULT_MK = "N";
                                Insert(log2);
                            }
                        }
                    }
                    #endregion
                }
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion 繳費紀錄檔案上傳      

        public string GetPRO_ACC(string APP_ID)
        {
            var rst = string.Empty;
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select PRO_ACC from apply 
                WHERE 1=1 AND APP_ID ='" + APP_ID + "'";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }
        public string GetEPNO(string EPID)
        {
            var rst = string.Empty;
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select EPNO from ADMIN_EPNO 
                WHERE 1=1 AND EPID ='" + EPID + "'";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }
        public string GetPRO_ACCMail(string PRO_ACC)
        {
            var rst = string.Empty;
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select mail from [admin] 
                WHERE 1=1 AND acc_no ='" + PRO_ACC + "'";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }


        /// <summary>
        /// 新收案件寄信通知        
        /// </summary>
        /// <param name="ACC_NAM">申請人姓名</param>
        /// <param name="Mail">信箱</param>
        /// <param name="APP_ID">案件編號</param>
        /// <param name="Srv_Nam">申請案件名稱</param>
        /// <param name="Srv_ID">申請案件編號</param>
        /// <param name="ISSEND">預設true，是否通知承辦人</param>
        public void SendMail_Overdue(string subject, string mailbody, string email, string srv_id)
        {
            SessionModel sm = SessionModel.Get();
            MailMessage mailMessage;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                this.conn = conn;

                // 寄信LOG
                TblMAIL_LOG log2 = new TblMAIL_LOG();
                log2.MAIL = email;
                log2.SUBJECT = subject;
                log2.BODY = mailbody;
                log2.SEND_TIME = DateTime.Now;
                log2.SRV_ID = srv_id;
                try
                {
                    if (ConfigModel.MailRevTest == "1")
                    {
                        // 測試環境
                        mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, log2.SUBJECT, log2.BODY);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                        if (ConfigModel.MailRevIsTwo == "1")
                        {
                            var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                            foreach (var rec in recList)
                            {
                                mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, log2.SUBJECT, log2.BODY);
                                mailMessage.IsBodyHtml = true;
                                CommonsServices.SendMail(mailMessage);
                            }
                        }
                    }
                    else
                    {
                        // 承辦mail
                        mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, email, log2.SUBJECT, log2.BODY);
                        mailMessage.IsBodyHtml = true;
                        CommonsServices.SendMail(mailMessage);
                    }
                    logger.Debug("寄信成功");
                    // 寄信成功
                    log2.RESULT_MK = "Y";
                    Insert(log2);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    // 寄信失敗
                    logger.Error("寄信失敗");
                    log2.RESULT_MK = "N";
                    Insert(log2);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// 取得申辦項目代號中文
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public string GetESUN_ERR_CODE(string code_cd)
        {
            var rst = code_cd.TONotNullString();
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select CODE_DESC
                                 from code_cd 
                                 where 1 = 1 and code_kind ='ESUN_ERR_CODE' and code_cd='" + code_cd + "' ";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }
                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }
        /// <summary>
        /// 取得申辦項目代號中文
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public string GetESUN_ERR_CODE_U(string code_cd)
        {
            var rst = code_cd.TONotNullString();
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select CODE_DESC
                                 from code_cd 
                                 where 1 = 1 and code_kind ='ESUN_ERR_CODE_U' and code_cd='" + code_cd + "' ";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }

                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        public string Getvw_PACK_UNIT(string CODE)
        {
            var rst = string.Empty;
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select CODE_DESC from CODE_CD 
                WHERE 1=1 AND CODE_KIND='F5_vw_PACK_UNIT' AND CODE_CD ='" + CODE + "'";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }

                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        public string Getvw_PACK(string CODE)
        {
            var rst = string.Empty;
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = @" select CODE_MEMO from CODE_CD 
                WHERE 1=1 AND CODE_KIND='F5_vw_PACK' AND CODE_CD ='" + CODE + "'";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }

                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        public string GetApplyColumn(string APP_ID, string column)
        {
            var rst = string.Empty;
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                switch (column.ToUpper())
                {
                    case "SRV_ID":
                        _sql += @" select SRV_ID from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "UNIT_NAME":
                        _sql += @" select NAME from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "ADDRESS":
                        _sql += @" select ADDR_CODE + ' ' + ADDR from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "FAX":
                        _sql += @" select FAX from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "MAIL":
                        _sql += @" select MEMBER.MAIL from apply
                JOIN MEMBER ON MEMBER.ACC_NO = apply.ACC_NO 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "CNT_NAME":
                        _sql += @" select CNT_NAME from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "TEL":
                        _sql += @" select TEL from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "TOKEN":
                        _sql += @" select TOKEN from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "001009_APPLY_CERT_CATE_NAME":
                        _sql += @" select code_desc from APPLY_001009
join CODE_CD on CODE_CD.CODE_KIND = 'F1_LICENSE_CD_1' and CODE_CD.CODE_CD = APPLY_001009.APPLY_CERT_CATE
where 1 = 1 and APPLY_001009.APP_ID ='" + APP_ID + "'";
                        break;
                    case "001037_APPLY_CERT_CATE_NAME":
                        _sql += @" select cd2.CODE_DESC from apply_001037 a
join CODE_CD cd1 on cd1.CODE_CD = a.LIC_CD and cd1.CODE_KIND='F1_LICENSE_CD_1'
join CODE_CD cd2 on cd2.CODE_CD = cd1.CODE_PCD and cd2.CODE_KIND='F1_LICENSE_CD_1'
where 1 = 1 and a.APP_ID = '" + APP_ID + "'";
                        break;
                    case "001039_APPLY_CERT_CATE_NAME":
                        _sql += @" select '醫師' code_desc from apply_001039 where 1=1 and app_id='" + APP_ID + "'";
                        break;
                    case "APP_TIME":
                        _sql += @" SELECT CONVERT(VARCHAR(3),CONVERT(VARCHAR(4),APP_TIME,20) - 1911) + '年' +
           SUBSTRING(CONVERT(VARCHAR(10),APP_TIME,20),6,2) + '月' +
           SUBSTRING(CONVERT(VARCHAR(10),APP_TIME,20),9,2) + '日' FROM apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                    case "NAME":
                        _sql += @" select NAME from apply where 1=1 and app_id='" + APP_ID + "'";
                        break;
                    default:
                        _sql += @" select APP_ID from apply 
                WHERE 1=1 and app_id ='" + APP_ID + "'";
                        break;
                }

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }

                if (_dt.Rows.Count > 0) { rst = _dt.Rows[0][0].TONotNullString(); }
                conn.Close();
                conn.Dispose();
            }
            return rst;
        }

        public DocumentExportModel.DocumentModel GetDIColumn005013(string APP_ID)
        {
            IList<DocumentExportModel.DocumentModel> result = new List<DocumentExportModel.DocumentModel>();
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                _sql = "select srv_id, apply.name as unit_name, apply.ADDR_CODE +' '+ apply.ADDR as address, apply.fax ";
                _sql += ",member.mail,apply.name,apply.tel as cnt_tel,apply.token,apply.app_id ";
                _sql += ",'申請「'+SUBSTRING(item3,1,LEN(item3)-1)+'」少量自用中藥貨品進口('+ case a13.APP_TYPE when 'Y' then '事前申請' when 'N' then '補辦案件' end  +')，請惠准辦理。' as subject ";
                _sql += ",apply.name as apply_name, apply.IDN as apply_idn, apply.ADDR_CODE +' '+ apply.ADDR as apply_addr,apply.TEL ";
                _sql += ", a13item1.item1 as productname ";
                _sql += ", a13item1.item2 as productunit ";
                _sql += ",a13.ORIGIN_TEXT as PRODUCTION_COUNTRY ";
                _sql += ",a13.SELLER_TEXT as SELL_COUNTRY ";
                _sql += ",a13.SHIPPINGPORT_TEXT as TRANSFER_COUNTRY ";
                _sql += ",apply.ADDR_CODE,apply.ADDR,zip.CITYNM,zip.TOWNNM ";
                _sql += "from apply ";
                _sql += "join member on MEMBER.ACC_NO = apply.ACC_NO ";
                _sql += "join apply_005013 a13 on a13.app_id = apply.APP_ID ";
                _sql += "left join ( ";
                _sql += "select app_id, (select ITEMNUM +'-'+case PORCTYPE when '0' then '中藥材' else '中藥製劑' end +'-'+ COMMODITIES + '、' ";
                _sql += "from APPLY_005013_ITEM1 where APPLY_005013_ITEM1.app_id = b.APP_ID for xml path('')) as item1 ";
                _sql += ",(select ITEMNUM +'-'+qty +'-'+UNIT_TEXT +'-規格數量，'+SPECQTY+'-'+SPECUNIT_TEXT+ '、' ";
                _sql += "from APPLY_005013_ITEM1 where APPLY_005013_ITEM1.app_id = b.APP_ID for xml path('')) as item2 ";
                _sql += ", (select COMMODITIES +'、' from APPLY_005013_ITEM1 where APPLY_005013_ITEM1.app_id = b.APP_ID for xml path('')) as item3 ";
                _sql += "from APPLY_005013_ITEM1 b ";
                _sql += ") a13item1 on a13item1.APP_ID = apply.APP_ID ";
                _sql += "left join ZIPCODE zip on zip.ZIP_CO = apply.ADDR_CODE ";
                _sql += "where apply.app_id = '" + APP_ID + "'";

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }

                for (int i = 0; i < _dt.Rows.Count; i++)
                {
                    DocumentExportModel.DocumentModel item = new DocumentExportModel.DocumentModel();
                    item.applyData = new Document005();
                    item.SRV_ID = _dt.Rows[i][0].TONotNullString();
                    item.UNIT_NAME = _dt.Rows[i][1].TONotNullString();
                    // addr_code+addr 2 
                    item.ADDRESS = _dt.Rows[i][2].TONotNullString().Contains(_dt.Rows[i][21].TONotNullString()) ? _dt.Rows[i][2].TONotNullString()
                        : _dt.Rows[i][2].TONotNullString().Contains(_dt.Rows[i][22].TONotNullString()) ? _dt.Rows[i][19].TONotNullString() + " " + _dt.Rows[i][21].TONotNullString() + _dt.Rows[i][20].TONotNullString()
                        : _dt.Rows[i][19].TONotNullString() + " " + _dt.Rows[i][21].TONotNullString() + _dt.Rows[i][22].TONotNullString() + _dt.Rows[i][20].TONotNullString();
                    item.FAX = _dt.Rows[i][3].TONotNullString();
                    item.MAIL = _dt.Rows[i][4].TONotNullString();
                    item.NAME = _dt.Rows[i][5].TONotNullString();
                    item.TEL = _dt.Rows[i][6].TONotNullString();
                    // token 7
                    item.APP_ID = _dt.Rows[i][8].TONotNullString();
                    item.SUBJECT = _dt.Rows[i][9].TONotNullString();
                    item.applyData.Apply_NAME = _dt.Rows[i][10].TONotNullString();
                    item.applyData.Apply_IDN = _dt.Rows[i][11].TONotNullString();
                    // addr_code + addr 12
                    item.applyData.Apply_ADDR = item.ADDRESS;
                    // cnt_tel 13
                    item.applyData.CNT_TEL = item.TEL;
                    item.applyData.PRODUCTNAME = _dt.Rows[i][14].TONotNullString();
                    item.applyData.PRODUCTUNIT = _dt.Rows[i][15].TONotNullString();
                    item.applyData.PRODUCTION_COUNTRY = _dt.Rows[i][16].TONotNullString();
                    item.applyData.SELL_COUNTRY = _dt.Rows[i][17].TONotNullString();
                    item.applyData.TRANSFER_COUNTRY = _dt.Rows[i][18].TONotNullString();
                    // addr_code,addr,citynm,townnm 19,20,21,22

                    result.Add(item);
                }
                conn.Close();
                conn.Dispose();
            }
            return result.FirstOrDefault();
        }
        public DocumentExportModel.DocumentModel GetDIColumn005014(string APP_ID)
        {
            IList<DocumentExportModel.DocumentModel> result = new List<DocumentExportModel.DocumentModel>();
            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();
            string _sql = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                _sql = "select distinct ";
                _sql += "srv_id, apply.name as unit_name, apply.ADDR_CODE +' '+ apply.ADDR as address, apply.fax ";
                _sql += ",member.mail,apply.cnt_name,apply.cnt_tel,apply.token,apply.app_id,'本公司申請「'+SUBSTRING(item3,1,LEN(item3)-1)+'」專案進口，請惠准辦理。' as subject ";
                _sql += ",apply.name as apply_name,apply.IDN as apply_idn, apply.ADDR_CODE +' '+ apply.ADDR as apply_addr,apply.CNT_TEL ";
                _sql += ",a14item.item1 as PRODUCTNAME ";
                _sql += ",a14item.item2 as PRODUCTUNIT ";
                _sql += ",port2.CODE_DESC as PRODUCTION_COUNTRY ";
                _sql += ",port3.CODE_DESC as TRANSFER_COUNTRY ";
                _sql += ",port4.CODE_DESC as SELL_COUNTRY ";
                _sql += ",apply.ADDR_CODE,apply.ADDR,zip.CITYNM,zip.TOWNNM ";
                _sql += "from apply ";
                _sql += "join member on MEMBER.ACC_NO = apply.ACC_NO ";
                _sql += "join Apply_005014 a14 on a14.APP_ID = apply.APP_ID ";
                _sql += "left join ( ";
                _sql += "select app_id, (select convert(varchar,ITEM) +'-'+ case PORCTYPE when '0' then '中藥材' else '中藥製劑' end +'-'+ COMMODITIES +'、' ";
                _sql += "from APPLY_005014_ITEM where APPLY_005014_ITEM.app_id = b.APP_ID for xml path('')) as item1 ";
                _sql += ",(select convert(varchar,ITEM) +'-'+ convert(varchar,qty) +'-'+ code_cd.CODE_MEMO +'-規格數量，'+SPECQTY+'-'+specunit.CODE_DESC+ '、' ";
                _sql += "from APPLY_005014_ITEM ";
                _sql += "left join code_cd on code_cd.CODE_CD = APPLY_005014_ITEM.UNIT and code_cd.CODE_KIND = 'F5_vw_PACK' ";
                _sql += "left join code_cd specunit on specunit.CODE_CD = apply_005014_item.SPECUNIT and specunit.CODE_KIND='F5_vw_PACK_UNIT' ";
                _sql += "where APPLY_005014_ITEM.app_id = b.APP_ID for xml path('')) as item2 ";
                _sql += ",(select COMMODITIES +'、' from APPLY_005014_ITEM where APPLY_005014_ITEM.app_id = b.APP_ID for xml path('')) as item3 ";
                _sql += "from APPLY_005014_ITEM b ";
                _sql += ") a14item on a14item.APP_ID = apply.APP_ID ";
                _sql += "left join code_cd port2 on port2.CODE_CD = a14.PRODUCTION_COUNTRY and port2.CODE_KIND = 'F1_PORT_2' ";
                _sql += "left join code_cd port3 on port3.CODE_CD = a14.TRANSFER_COUNTRY and port3.CODE_KIND='F1_PORT_2' ";
                _sql += "left join code_cd port4 on port4.CODE_CD = a14.SELL_COUNTRY and port4.CODE_KIND='F1_PORT_2' ";
                _sql += "left join ZIPCODE zip on zip.ZIP_CO = apply.ADDR_CODE ";
                _sql += "where apply.app_id = '" + APP_ID + "'";

                using (SqlCommand dbc = new SqlCommand(_sql, conn))
                {
                    _da.SelectCommand = dbc;
                    _da.Fill(_dt);
                }

                for (int i = 0; i < _dt.Rows.Count; i++)
                {
                    DocumentExportModel.DocumentModel item = new DocumentExportModel.DocumentModel();
                    item.applyData = new Document005();
                    item.SRV_ID = _dt.Rows[i][0].TONotNullString();
                    item.UNIT_NAME = _dt.Rows[i][1].TONotNullString();
                    // addr_code+ addr 2
                    item.ADDRESS = _dt.Rows[i][2].TONotNullString().Contains(_dt.Rows[i][21].TONotNullString()) ? _dt.Rows[i][2].TONotNullString()
                        : _dt.Rows[i][2].TONotNullString().Contains(_dt.Rows[i][22].TONotNullString()) ? _dt.Rows[i][19].TONotNullString() + " " + _dt.Rows[i][21].TONotNullString() + _dt.Rows[i][20].TONotNullString()
                        : _dt.Rows[i][19].TONotNullString() + " " + _dt.Rows[i][21].TONotNullString() + _dt.Rows[i][22].TONotNullString() + _dt.Rows[i][20].TONotNullString();
                    item.FAX = _dt.Rows[i][3].TONotNullString();
                    item.MAIL = _dt.Rows[i][4].TONotNullString();
                    item.NAME = _dt.Rows[i][5].TONotNullString();
                    item.TEL = _dt.Rows[i][6].TONotNullString();
                    // token 7
                    item.APP_ID = _dt.Rows[i][8].TONotNullString();
                    item.SUBJECT = _dt.Rows[i][9].TONotNullString();
                    item.applyData.Apply_NAME = _dt.Rows[i][10].TONotNullString();
                    item.applyData.Apply_IDN = _dt.Rows[i][11].TONotNullString();
                    // addr_code + addr 12
                    item.applyData.Apply_ADDR = item.ADDRESS;
                    // cnt_tel 13
                    item.applyData.CNT_TEL = item.TEL;
                    item.applyData.PRODUCTNAME = _dt.Rows[i][14].TONotNullString();
                    item.applyData.PRODUCTUNIT = _dt.Rows[i][15].TONotNullString();
                    item.applyData.PRODUCTION_COUNTRY = _dt.Rows[i][16].TONotNullString();
                    item.applyData.TRANSFER_COUNTRY = _dt.Rows[i][17].TONotNullString();
                    item.applyData.SELL_COUNTRY = _dt.Rows[i][18].TONotNullString();
                    // addr_code,addr,citynm,townnm 19,20,21,22

                    result.Add(item);
                }
                conn.Close();
                conn.Dispose();
            }
            return result.FirstOrDefault();
        }
    }
}
