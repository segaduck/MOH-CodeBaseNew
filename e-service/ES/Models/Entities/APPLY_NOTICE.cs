using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 補件備註
    /// </summary>
    public class TblAPPLY_NOTICE
    {
        /// <summary>
        /// 案號
        /// </summary>
        public string APP_ID { get; set; }

        /// <summary>
        /// 欄位ID
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string Field_NAME { get; set; }

        /// <summary>
        /// 是否補件
        /// </summary>
        public string ISADDYN { get; set; }

        /// <summary>
        /// 次數(第幾次)
        /// </summary>
        public int? FREQUENCY { get; set; }

        /// <summary>
        /// 新增日期
        /// </summary>
        public DateTime? ADD_TIME { get; set; }

        /// <summary>
        /// 最後補件日期
        /// </summary>
        public DateTime? DEADLINE { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string NOTE { get; set; }

        /// <summary>
        /// 檔案序號
        /// </summary>
        public int? SRC_NO { get; set; }

        /// <summary>
        /// 批次檔案序號
        /// </summary>
        public int? BATCH_INDEX { get; set; }
    }
}