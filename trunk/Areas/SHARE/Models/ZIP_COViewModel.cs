using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EECOnline.Commons;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Omu.ValueInjecter;
using Turbo.DataLayer;
using Turbo.Commons;
using EECOnline.Models;
using EECOnline.Models.Entities;

namespace EECOnline.Areas.SHARE.Models
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
    public class ZIP_COFormModel: PagingResultsViewModel
    {
        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Display(Name = "郵遞區號")]
        [Control(Mode = Control.TextBox, size = "10", maxlength = "5")]
        public string zip_co { get; set; }

        /// <summary>
        /// 郵遞區號名稱
        /// </summary>
        [Display(Name = "郵遞區號名稱")]
        [Control(Mode = Control.TextBox, size = "30")]
        public string zip_nm { get; set; }

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
        public string ZIP_NM { get; set; }
    }
    
}