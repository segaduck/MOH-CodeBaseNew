using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// 地址
    /// </summary>
    public class TblZIPCODE
    {
        /// <summary>
        /// 郵遞區號
        /// </summary>
        public string ZIP_CO { get; set; }

        /// <summary>
        /// 縣市
        /// </summary>
        public string CITYNM { get; set; }

        /// <summary>
        /// 區
        /// </summary>
        public string TOWNNM { get; set; }

        /// <summary>
        /// 路段
        /// </summary>
        public string ROADNM { get; set; }

        /// <summary>
        /// 範圍
        /// </summary>
        public string ROUND { get; set; }

    }
}