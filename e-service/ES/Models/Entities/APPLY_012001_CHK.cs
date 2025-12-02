using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPPLY_012001_CHK
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        [Display(Name = "序號")]
        public int? SEQ_NO { get; set; }

        /// <summary>
        /// 種類 
        /// 0:申請檔案 1:申請事由
        /// </summary>
        [Display(Name = "種類")]
        public string TYPE { get; set; }

        /// <summary>
        /// 申請項目
        /// </summary>
        [Display(Name = "申請項目")]
        public string CHECKNO { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Display(Name = "備註")]
        public string NOTE { get; set; }
       
    }
}