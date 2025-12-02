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
    public class DynamicOtherFileGrid : TblAPPLY_FILE
    {
        //private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();

        ///// <summary>
        ///// static contrustor
        ///// 用來定義 acceptFileTypes
        ///// </summary>
        //static DynamicOtherFileGrid()
        //{
        //    acceptFileTypes.Add(AcceptFileType.BMP);
        //    acceptFileTypes.Add(AcceptFileType.CSV);
        //    acceptFileTypes.Add(AcceptFileType.DAT);
        //    acceptFileTypes.Add(AcceptFileType.DOC);
        //    acceptFileTypes.Add(AcceptFileType.DOCX);
        //    acceptFileTypes.Add(AcceptFileType.GIF);
        //    acceptFileTypes.Add(AcceptFileType.JPG);
        //    acceptFileTypes.Add(AcceptFileType.JPGE);
        //    acceptFileTypes.Add(AcceptFileType.ODG);
        //    acceptFileTypes.Add(AcceptFileType.ODP);
        //    acceptFileTypes.Add(AcceptFileType.ODS);
        //    acceptFileTypes.Add(AcceptFileType.ODT);
        //    acceptFileTypes.Add(AcceptFileType.PDF);
        //    acceptFileTypes.Add(AcceptFileType.PNG);
        //    acceptFileTypes.Add(AcceptFileType.PPT);
        //    acceptFileTypes.Add(AcceptFileType.PPTX);
        //    acceptFileTypes.Add(AcceptFileType.TIFF);
        //    acceptFileTypes.Add(AcceptFileType.TXT);
        //    acceptFileTypes.Add(AcceptFileType.XLS);
        //    acceptFileTypes.Add(AcceptFileType.XLSX);
        //}

        public IList<OtherFile> fileGrid { get; set; }

    }

    public class OtherFile : TblAPPLY_FILE
    {
        public HttpPostedFileBase File { get; set; }

        public string FileName { get; set; }

        public string FileSize { get; set; }

        public string FilePath { get; set; }
    }
}