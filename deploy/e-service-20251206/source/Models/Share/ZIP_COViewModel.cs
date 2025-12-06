using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Omu.ValueInjecter;
using ES.Models.Share;
using ES.Models.Entities;

namespace ES.Models.Share
{
    /// <summary>
    /// 
    /// </summary>
    public class ZIP_COViewModel
    {
        public ZIP_COViewModel()
        {
            this.Form = new ZIP_COFormModel();
        }

        /// <summary>
        /// 查詢條件 FromModel
        /// </summary>
        public ZIP_COFormModel Form { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ZIP_COFormModel:PaginationInfo
    {
        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Display(Name = "郵遞區號")]
        public string ZIP_CO { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        [Display(Name = "縣市名稱")]
        public string CITYNM { get; set; }

        /// <summary>
        /// 鄉鎮市區名稱
        /// </summary>
        [Display(Name = "鄉鎮市區名稱")]
        public string TOWNNM { get; set; }

        /// <summary>
        /// 街道名稱
        /// </summary>
        [Display(Name ="街道名稱")]
        public string ROADNM { get; set; }

        /// <summary>
        /// 郵遞區號格式 (5=5碼, 6=6碼)
        /// 預設為6碼
        /// </summary>
        [Display(Name = "郵遞區號格式")]
        public string ZIP_FORMAT { get; set; } = "6";

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<ZIP_COGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ZIP_COGridModel : TblZIPCODE
    {
        public string CODE { get; set; }

        public string TEXT { get; set; }
    }

}