using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblFLYPAYROOMS_DE
    {
        /// <summary>
        /// 
        /// </summary>
        public int? DE_ID { get; set; }
        /// <summary>
        /// 梯次
        /// </summary>
        public int? CD_ID { get; set; }
        /// <summary>
        /// 選擇地區 北1 中2 南3
        /// </summary>
        public string SEC { get; set; }
        /// <summary>
        /// 九天一周期 D0~D8
        /// </summary>
        public string SE_DAY { get; set; }
        /// <summary>
        /// 可申請房間數
        /// </summary>
        public int? ROOMS { get; set; }
        /// <summary>
        /// 集檢所代號
        /// </summary>
        public string SE_CODE { get; set; }
        /// <summary>
        /// 集檢所名稱
        /// </summary>
        public string SE_VAL { get; set; }
    }
}