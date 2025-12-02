using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Commons;

namespace EECOnline.Models
{
    public class TxtUploadFile : UploadFile
    {
        private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();
        
        /// <summary>
        /// static contrustor 
        /// 用來定義 acceptFileTypes
        /// </summary>
        static TxtUploadFile()
        {
            acceptFileTypes.Add(AcceptFileType.TXT);
            acceptFileTypes.Add(AcceptFileType.DAT);
        }

        /// <summary>
        /// 預設 TxtUploadFile 建構子
        /// </summary>
        public TxtUploadFile()
        {

        }

        /// <summary>
        /// 指定上傳檔案儲存路徑, 建構 TxtUploadFile
        /// </summary>
        /// <param name="locationPath">相對於 ContextRoot 的路徑</param>
        public TxtUploadFile(string locationPath) : base(locationPath)
        {

        }

        public override IList<AcceptFileType> GetAcceptFileTypes()
        {
            return acceptFileTypes;
        }
    }
}