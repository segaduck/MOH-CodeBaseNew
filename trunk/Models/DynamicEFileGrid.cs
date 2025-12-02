using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Commons;
using EECOnline.DataLayers;
using EECOnline.Models.Entities;
using EECOnline.Services;
using Turbo.DataLayer;

namespace EECOnline.Models
{
    /// <summary>
    /// 檔案上傳類, 可用來上傳
    /// </summary>
    public class DynamicEFileGrid : UploadFile
    {
        private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();

        /// <summary>
        /// static contrustor
        /// 用來定義 acceptFileTypes
        /// </summary>
        static DynamicEFileGrid()
        {
            acceptFileTypes.Add(AcceptFileType.BMP);
            acceptFileTypes.Add(AcceptFileType.CSV);
            acceptFileTypes.Add(AcceptFileType.DAT);
            acceptFileTypes.Add(AcceptFileType.DOC);
            acceptFileTypes.Add(AcceptFileType.DOCX);
            acceptFileTypes.Add(AcceptFileType.GIF);
            acceptFileTypes.Add(AcceptFileType.JPG);
            acceptFileTypes.Add(AcceptFileType.JPGE);
            acceptFileTypes.Add(AcceptFileType.ODG);
            acceptFileTypes.Add(AcceptFileType.ODP);
            acceptFileTypes.Add(AcceptFileType.ODS);
            acceptFileTypes.Add(AcceptFileType.ODT);
            acceptFileTypes.Add(AcceptFileType.PDF);
            acceptFileTypes.Add(AcceptFileType.PNG);
            acceptFileTypes.Add(AcceptFileType.PPT);
            acceptFileTypes.Add(AcceptFileType.PPTX);
            acceptFileTypes.Add(AcceptFileType.TIFF);
            acceptFileTypes.Add(AcceptFileType.TXT);
            acceptFileTypes.Add(AcceptFileType.XLS);
            acceptFileTypes.Add(AcceptFileType.XLSX);
        }

        /// <summary>
        /// 預設 SystemUploadFile 建構子
        /// </summary>
        public DynamicEFileGrid()
        {
        }

        /// <summary>
        /// 指定上傳檔案儲存路徑, 建構 ExcelUploadFile
        /// </summary>
        /// <param name="locationPath">相對於 ContextRoot 的路徑</param>
        public DynamicEFileGrid(string locationPath) : base(locationPath)
        {
        }

        /// <summary>
        /// 取得可接受的上傳檔案類型
        /// </summary>
        /// <returns></returns>
        public override IList<AcceptFileType> GetAcceptFileTypes()
        {
            return acceptFileTypes;
        }

        /// <summary>
        /// 外層ID(例如:Deatil等...)
        /// </summary>
        public bool ShowFileUpload { get; set; }

        /// <summary>
        /// 刪除按鈕是否顯示
        /// </summary>
        public bool ShowDelete { get; set; }

        /// <summary>
        /// 限制筆數
        /// </summary>
        public int? limitRow { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 程式路徑(屬性)
        /// </summary>
        public string peky1 { get; set; }

        /// <summary>
        /// PKEY1(Id)
        /// </summary>
        public string peky2 { get; set; }

        /// <summary>
        /// PKEY2
        /// </summary>
        public string peky3 { get; set; }

        /// <summary>
        /// PKEY3
        /// </summary>
        public string peky4 { get; set; }

        /// <summary>
        /// 登打檔案名稱
        /// </summary>
        public string pfilename { get; set; }

        public IList<TblEFILE> fileGrid { get; set; }

        /// <summary>
        /// 取得初始列表
        /// </summary>
        public void GetFileGrid()
        {
            // 若有檔案列表，且FILEPKEY1(程式路徑)跟FILEPKEY1(基本PKEY)沒有資料則不進入
            if (fileGrid.ToCount() == 0 && this.peky1 != null && this.peky2 != null)
            {
                BaseDAO dao = new BaseDAO();

                TblEFILE where = new TblEFILE();
                where.peky1 = this.peky1;
                where.peky2 = this.peky2;
                where.peky3 = this.peky3;
                where.peky4 = this.peky4;

                fileGrid = dao.GetRowList(where);
            }
        }

        /// <summary>
        /// 儲存檔案列表
        /// </summary>
        public void SaveFileGrid()
        {
            BaseDAO dao = new BaseDAO();
            dao.BeginTransaction();
            try
            {
                // 刪除舊有檔案資料(必須有FILEPKEY1與FILEPKEY2以上條件才能刪除)
                if (this.peky2.TONotNullString() != "")
                {
                    TblEFILE fileWhere = new TblEFILE();
                    fileWhere.peky1 = this.peky1;
                    fileWhere.peky2 = this.peky2;
                    fileWhere.peky3 = this.peky3;
                    fileWhere.peky4 = this.peky4;

                    dao.Delete(fileWhere);

                    // 若有檔案列表，且FILEPKEY1(程式路徑)跟FILEPKEY1(基本PKEY)沒有資料則不進入
                    if (fileGrid.ToCount() != 0)
                    {
                        // 新增檔案資料
                        foreach (var file in fileGrid)
                        {
                            file.peky2 = this.peky2;
                            file.peky3 = this.peky3;
                            file.peky4 = this.peky4;
                            dao.Insert(file);
                        }
                    }

                    dao.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                dao.RollBackTransaction();
            }
        }
    }


}