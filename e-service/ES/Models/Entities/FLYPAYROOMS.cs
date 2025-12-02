using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblFLYPAYROOMS
    {
        /// <summary>
        /// 
        /// </summary>
        public int? CD_ID { get; set; }
        /// <summary>
        /// 梯次
        /// </summary>
        public string ECHELON { get; set; }
        /// <summary>
        /// 選擇地區 北1 中2 南3
        /// </summary>
        public string SECTION { get; set; }
        /// <summary>
        /// 九天一周期 D0~D8
        /// </summary>
        public string DAYCODE { get; set; }
        /// <summary>
        /// 可申請房間數
        /// </summary>
        public string ROOM { get; set; }
        /// <summary>
        /// 梯次 日期
        /// </summary>
        public string DAYVAL { get; set; }
    }
}