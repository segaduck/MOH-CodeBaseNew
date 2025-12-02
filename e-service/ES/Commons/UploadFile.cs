using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Web;
using log4net;

namespace ES.Commons
{
    /// <summary>
    /// 上傳檔案允許的檔案類型
    /// </summary>
    public class AcceptFileType
    {
        /// <summary>
        /// PDF文件
        /// </summary>
        public static AcceptFileType PDF = new AcceptFileType("pdf", "PDF文件");

        /// <summary>
        /// DAT文件
        /// </summary>
        public static AcceptFileType DAT = new AcceptFileType("dat", "DAT文件");

        /// <summary>
        /// Excel 97-2003 文件
        /// </summary>
        public static AcceptFileType XLS = new AcceptFileType("xls", "Excel 97-2003 文件(.xls)");

        /// <summary>
        /// Word 97-2003 文件
        /// </summary>
        public static AcceptFileType DOC = new AcceptFileType("doc", "Word 97-2003 文件(.doc)");

        /// <summary>
        /// Excel 2007/2010 文件
        /// </summary>
        public static AcceptFileType XLSX = new AcceptFileType("xlsx", "Excel 2007/2010 文件(.xlsx)");

        /// <summary>
        /// Word 2007/2010 文件
        /// </summary>
        public static AcceptFileType DOCX = new AcceptFileType("docx", "Word 2007/2010 文件(.docx)");

        /// <summary>
        /// PowerPoint 97-2003 文件(.ppt)
        /// </summary>
        public static AcceptFileType PPT = new AcceptFileType("ppt", "PowerPoint 97-2003 文件(.ppt)");

        /// <summary>
        /// PowerPoint 2007/2010 文件(.pptx)
        /// </summary>
        public static AcceptFileType PPTX = new AcceptFileType("pptx", "PowerPoint 2007/2010 文件(.pptx)");

        /// <summary>
        /// JPG圖片
        /// </summary>
        public static AcceptFileType JPG = new AcceptFileType("jpg", "JPG圖片");
        public static AcceptFileType JPEG = new AcceptFileType("jpeg", "JPEG圖片");


        /// <summary>
        /// PNG圖片
        /// </summary>
        public static AcceptFileType PNG = new AcceptFileType("png", "PNG圖片");

        /// <summary>
        /// GIF圖片
        /// </summary>
        public static AcceptFileType GIF = new AcceptFileType("gif", "GIF圖片");

        /// <summary>
        /// BMP圖片
        /// </summary>
        public static AcceptFileType BMP = new AcceptFileType("bmp", "GIF圖片");

        /// <summary>
        /// TIFF圖片
        /// </summary>
        public static AcceptFileType TIFF = new AcceptFileType("tiff", "TIFF圖片");

        /// <summary>
        /// Text文件
        /// </summary>
        public static AcceptFileType TXT = new AcceptFileType("txt", "TEXT文件(.txt)");

        /// <summary>
        /// CSV文件
        /// </summary>
        public static AcceptFileType CSV = new AcceptFileType("csv", "CSV文件(.csv)");

        /// <summary>
        /// 壓縮檔案(.ZIP)
        /// </summary>
        public static AcceptFileType ZIP = new AcceptFileType("zip", "壓縮檔案(.ZIP)");

        /// <summary>
        /// MDB文件(.mdb)
        /// </summary>
        public static AcceptFileType MDB = new AcceptFileType("mdb", "MDB文件(.mdb)");

        /// <summary>
        /// 壓縮檔案(.RAR)
        /// </summary>
        public static AcceptFileType RAR = new AcceptFileType("rar", "壓縮檔案(.RAR)");

        /// <summary>
        /// 壓縮檔案(.7Z)
        /// </summary>
        public static AcceptFileType ZIP_7Z = new AcceptFileType("7z", "壓縮檔案(.7Z)");

        /// <summary>
        /// OpenDocument 文字檔案(.odt)
        /// </summary>
        public static AcceptFileType ODT = new AcceptFileType("odt", "OpenDocument 文字檔案(.odt)");

        /// <summary>
        /// OpenDocument 試算表(.ods)
        /// </summary>
        public static AcceptFileType ODS = new AcceptFileType("ods", "OpenDocument 試算表(.ods)");

        /// <summary>
        /// OpenDocument 簡報(.odp)
        /// </summary>
        public static AcceptFileType ODP = new AcceptFileType("odp", "OpenDocument 簡報(.odp)");

        /// <summary>
        /// OpenDocument 繪圖(.odg)
        /// </summary>
        public static AcceptFileType ODG = new AcceptFileType("odg", "OpenDocument 繪圖(.odg)");


        #region AcceptFileType實作內容
        private string ext;
        private string extName;

        private AcceptFileType() { }

        /// <summary>
        /// 指定檔案類型(副檔名)建構 AcceptFileType
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="extName"></param>
        private AcceptFileType(string ext, string extName)
        {
            if (string.IsNullOrEmpty(ext))
            {
                throw new ArgumentNullException("ext");
            }
            this.ext = ext.ToLower();
            this.extName = string.IsNullOrEmpty(extName) ? ext : extName;
        }

        /// <summary>
        /// 檔案類型名稱
        /// </summary>
        public string ExtName
        {
            get
            {
                return this.extName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ext;
        }

        /// <summary>
        /// 隱藏預設 Equals(object)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private new bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// 傳入副檔名字串, 判斷是否為相同檔案類型
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public bool Equals(string ext)
        {
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }
            else
            {
                return ext.ToLower().Equals(this.ext);
            }
        }

        /// <summary>
        /// 是否為相同 AcceptFileType 類型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Equals(AcceptFileType type)
        {
            if(type == null)
            {
                return false;
            }
            else
            {
                return type.ext.Equals(this.ext);
            }
        }
        #endregion
    }

    /// <summary>
    /// 上傳檔案處理的基礎類型
    /// </summary>
    /// 
    [Serializable]
    public abstract class UploadFile
    {
        private ILog logger = LogManager.GetLogger(typeof(UploadFile));

        //private string physicalPath;
        private string fileName;

        /// <summary>
        /// UploadFile 建構子
        /// </summary>
        public UploadFile() { }

        /// <summary>
        /// 指定上傳檔案儲存路徑, 建構 UploadFile
        /// </summary>
        /// <param name="locationPath">相對於 ContextRoot 的路徑</param>
        public UploadFile(string locationPath)
        {
            if(locationPath == null)
            {
                throw new ArgumentNullException("沒有指定 locationPath");
            }
            this.LOCATION_PATH = locationPath;
        }

        /// <summary>
        /// 上傳檔案binding欄位(input type=file)
        /// </summary>
        public HttpPostedFileWrapper POSTED_FILE { get; set; }

        /// <summary>
        /// 上傳檔案原始檔名
        /// </summary>
        public string NAME {
            get {
                if(this.IsUploaded)
                {
                    fileName = this.POSTED_FILE.FileName;
                    if (fileName.Contains("\\"))
                    {
                        var fileNameSplit = fileName.Split('\\');
                        var fileNameCount = fileNameSplit.Length-1;
                        fileName = fileNameSplit[fileNameCount];
                    }
                }
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        /// <summary>
        /// 儲存時指定的檔案名稱
        /// </summary>
        public string SAVE_NAME { get; set; }

        /// <summary>
        /// 存放上傳檔案的路徑(相對於 ContextRoot, 不含上傳檔名)
        /// </summary>
        public string LOCATION_PATH { get; set; }

        /// <summary>
        /// 上傳檔案連結完整路徑 URI (含上傳後存檔檔名)
        /// </summary>
        public string LOCATION_URI
        {
            get;
            set;
        }

        /// <summary>
        /// 取得檔案儲存的完整實體路徑 (不含存檔檔名)
        /// </summary>
        public string PHYSICAL_NFPATH
        {
            get
            {
                if (this.LOCATION_PATH != null)
                {
                    return HttpContext.Current.Server.MapPath(this.LOCATION_PATH);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 取得檔案儲存的完整實體路徑 (含存檔檔名)
        /// </summary>
        public string PHYSICAL_PATH
        {
            get
            {
                if(this.LOCATION_PATH != null)
                {
                    return HttpContext.Current.Server.MapPath(this.LOCATION_URI);
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 回傳 可上傳的檔案類型列表, 
        /// 繼承的子類別實作這個 Method 以明確定義可接受的 AcceptFileType 清單
        /// </summary>
        public abstract IList<AcceptFileType> GetAcceptFileTypes();

        /// <summary>
        /// 回傳已設定的可上傳的檔案類型名稱字串(以逗號分格)
        /// </summary>
        public string AcceptFileTypeNames
        {
            get
            {
                IList<AcceptFileType> acceptTypes = GetAcceptFileTypes();
                List<string> names = new List<string>();
                if(acceptTypes != null)
                {
                    foreach (AcceptFileType type in acceptTypes)
                    {
                        names.Add(type.ExtName);
                    }
                }
                return string.Join(", ", names.ToArray());
            }
        }

        /// <summary>
        /// 判斷上傳的檔案類型是否在可接受的範圍(根據 addAcceptFileType 設定)
        /// </summary>
        public bool IsAcceptedType
        {
            get
            {
                bool accepted = false;
                if(!this.IsUploaded)
                {
                    accepted = true;
                }
                else
                {
                    IList<AcceptFileType> acceptTypes = GetAcceptFileTypes();
                    if (acceptTypes != null)
                    {
                        string ext = this.Extension;
                        foreach (AcceptFileType type in acceptTypes)
                        {
                            if (type.Equals(ext))
                            {
                                accepted = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        throw new System.NullReferenceException("GetAcceptFileTypes 沒有設定");
                    }
                }
                return accepted;
            }
        }


        /// <summary>
        /// 檢核檔案是否有上傳
        /// </summary>
        /// <returns></returns>
        public bool IsUploaded
        {
            get
            {
                return (this.POSTED_FILE != null 
                    && this.POSTED_FILE.ContentLength > 0
                    && !string.IsNullOrWhiteSpace(this.POSTED_FILE.FileName) );
            }
        }

        /// <summary>
        /// 取得上傳檔案的類型(副檔名字串, 不包括 '.')
        /// </summary>
        /// <returns></returns>
        public string Extension
        {
            get
            {
                if (string.IsNullOrEmpty(this.NAME))
                {
                    return string.Empty;
                }
                string extensions = string.Empty;
                int p = this.NAME.LastIndexOf(".", StringComparison.Ordinal);
                if (p > -1)
                {
                    extensions = this.NAME.Substring(p+1).ToLower();
                }
                return extensions;
            }
        }

        /// <summary>
        /// 取得上傳檔案的 InputStream 供讀取上傳內容
        /// <para>若檔案沒有上傳, 則回傳 null</para>
        /// <para>會檢查上傳檔案類型是否為 IsAcceptedType, 若否則會丟出 UnAcceptableFileTypeException</para>
        /// </summary>
        /// <returns></returns>
        private Stream GetHttpPostFileInputStream()
        {
            //===================================
            // 未上傳檔案時忽略
            //===================================
            if (!this.IsUploaded)
            {
                return null;
            }

            // 上傳檔案類型檢核
            if (!this.IsAcceptedType)
            {
                string msg = string.Format("上傳檔案類型不合法(可接受:{0})", this.AcceptFileTypeNames);
                throw new UnAcceptableFileTypeException(msg);
            }

            return this.POSTED_FILE.InputStream;
        }


        /// <summary>
        /// 取得上傳檔案的內容(byte[]), 注意: 一旦讀取後, 即無法再次讀取!
        /// <para>若檔案沒有上傳, 則回傳 null</para>
        /// <para>會檢查上傳檔案類型是否為 IsAcceptedType, 若否則會丟出 UnAcceptableFileTypeException</para>
        /// </summary>
        /// <returns></returns>
        public byte[] GetHttpPostFileContent()
        {
            byte[] data = null;
            using (Stream s = GetHttpPostFileInputStream())
            {
                if(s == null)
                {
                    return null;
                }

                data = new byte[this.POSTED_FILE.ContentLength];
                s.Read(data, 0, data.Length);
            }
            return data;
        }

        /// <summary>
        /// 根據指定的 LOCATION_PATH 及指定的 newFileName, 儲存上傳的檔案
        /// <para>若檔案沒有上傳, 則不作任何處理</para>
        /// <para>儲存前會檢查上傳檔案類型是否為 IsAcceptedType, 若否則會丟出 UnAcceptableFileTypeException</para>
        /// </summary>
        /// <param name="newFileName">儲存實體檔案名稱</param>
        /// <returns></returns>
        public void SaveHttpPostFile(string newFileName)
        {

            //===================================
            // 未上傳檔案時忽略
            //===================================
            if (!this.IsUploaded){
                return ;
            }

            // 上傳檔案類型檢核
            if(!this.IsAcceptedType)
            {
                string msg = string.Format("上傳檔案類型不合法(可接受:{0})", this.AcceptFileTypeNames);
                throw new UnAcceptableFileTypeException(msg);
            }

            if(string.IsNullOrEmpty(this.LOCATION_PATH))
            {
                throw new ArgumentNullException("LOCATION_PATH 沒有設定");
            }

            //===================================
            //設置屬性
            //===================================

            //原始檔名
            NAME = this.POSTED_FILE.FileName;
            // 儲存檔名
            SAVE_NAME = newFileName;
            if(!newFileName.EndsWith("." + this.Extension, StringComparison.OrdinalIgnoreCase))
            {
                // 如果傳入的 newFileName 沒有包含副檔名, 才自動加上去
                SAVE_NAME += "." + this.Extension;
            }

            //生成資料夾路徑
            if (!Directory.Exists(PHYSICAL_NFPATH))
            {
                Directory.CreateDirectory(PHYSICAL_NFPATH);
            }

            //存檔檔案路徑
            LOCATION_URI = this.LOCATION_PATH + "/" + SAVE_NAME;

            // PHYSICAL_PATH 自動將 LOCATION 對應為 實際路徑
            logger.Info("SaveHttpPostFile: " + LOCATION_URI + " => " + this.PHYSICAL_PATH);

            //===================================
            //儲存檔案
            //===================================
            this.POSTED_FILE.SaveAs(PHYSICAL_PATH);

        }

        /// <summary>
        /// 刪除已儲存的實體檔案
        /// </summary>
        public void DeleteSavedFile()
        {
            string physicalPath = PHYSICAL_PATH;
            if(File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                logger.Info(string.Format("File '{0}' has been deleted", physicalPath));
            }
        }
    }
}
