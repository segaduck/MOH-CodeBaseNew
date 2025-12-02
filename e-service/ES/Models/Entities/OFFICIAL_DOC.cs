using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class OFFICIAL_DOC
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public string APP_ID { get; set; }

        /// <summary>
        /// 公文文號
        /// </summary>
        public string MOHW_CASE_NO { get; set; }

        /// <summary>
        /// 取得時間
        /// </summary>
        public DateTime? INSERTDATE { get; set; }

        /// <summary>
        /// 新增人員
        /// </summary>
        public string ADD_ACC { get; set; }
    }
}