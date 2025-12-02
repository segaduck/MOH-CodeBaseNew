using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 信件通知表
    /// </summary>
    public class TblMAILACC
    {
        /// <summary>
        /// 通知人姓名
        /// </summary>
        public string NAME { get; set; }

        /// <summary>
        /// 通知人信箱
        /// </summary>
        public string MAIL { get; set; }

        /// <summary>
        /// 申辦案件
        /// </summary>
        public string SRV_ID { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string NOTE { get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        public DateTime? UPD_TIME { get; set; }


        /// <summary>
        /// 新增日期
        /// </summary>
        public DateTime? ADD_TIME { get; set; }

    
        /// <summary>
        /// 新增人員
        /// </summary>
        public string ADD_ACC { get; set; }
    }
}