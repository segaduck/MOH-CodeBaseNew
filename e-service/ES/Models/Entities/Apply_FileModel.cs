using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 申請案件附檔
    /// </summary>
    public class Apply_FileModel
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public string APP_ID { get; set; }
        /// <summary>
        /// 檔案序號
        /// </summary>
        public int? FILE_NO { get; set; }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FILENAME { get; set; }
        /// <summary>
        /// 檔案來源
        /// </summary>
        public string SRC_FILENAME { get; set; }
        /// <summary>
        /// 檔案來源序號
        /// </summary>
        public int? SRC_NO { get; set; }
        /// <summary>
        /// 檔案批次序號
        /// </summary>
        public int? BATCH_INDEX { get; set; }
        
        /// <summary>
        /// 檔案含補件上傳次數
        /// 新收案件時第一次上傳請寫入1
        /// 後續依據補件次數填寫，因後台顯示檔案歷程時需顯示該檔為第幾次補件時上傳
        /// </summary>
        public int? NOTICE_NO { get; set; }

        /// <summary>
        /// 刪除註記
        /// </summary>
        public string DEL_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DEL_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_ACC { get; set; }

    }
}