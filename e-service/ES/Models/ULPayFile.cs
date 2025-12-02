using System;
using System.Collections.Generic;
using ES.Models.Entities;
using System.Web;

namespace ES.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ULPayFileModel
    {
        /// <summary>
        /// 
        /// </summary>
        public ULPayFileFormModel Form { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ULPayFileFormModel : ApplyModel
    {
        /// <summary>
        /// APP_ID-SRV_ID-FILE_NO
        /// </summary>
        public string Upload_PayFile { get; set; }

        /// <summary>
        /// 該申辦案件的FILE_NO
        /// </summary>
        public string FILE_NO { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案
        /// </summary>
        public HttpPostedFileBase ATTACH_FILE { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf
        /// </summary>
        public string ATTACH_FILE_NAME { get; set; }

        /// <summary>
        /// 是否成功上傳狀態 Y/N
        /// </summary>
        public string STATUS { get; set; }
        /// <summary>
        /// 檢核錯誤訊息內容
        /// </summary>
        public string ERRMSG { get; set; }

        public string EMAIL { get; set; }
    }
}