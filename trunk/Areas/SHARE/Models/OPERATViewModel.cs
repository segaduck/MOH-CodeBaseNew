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
    public class OPERATViewModel
    {
        public OPERATViewModel()
        {
            this.Form = new OPERATFormModel();
        }

        /// <summary>
        /// 查詢條件 FromModel
        /// </summary>
        public OPERATFormModel Form { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class OPERATFormModel: PagingResultsViewModel
    {
        /// <summary>
        /// 承辦人帳號
        /// </summary>
        [Display(Name = "承辦人帳號")]
        [Control(Mode = Control.TextBox, size = "10", maxlength = "3")]
        public string OPERAT_cd { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        [Display(Name = "承辦人姓名")]
        [Control(Mode = Control.TextBox, size = "30")]
        public string OPERAT_nm { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<OPERATGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OPERATGridModel : TblAMDBURM
    {
       
    }
    
}