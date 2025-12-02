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
    public class GRPViewModel
    {
        public GRPViewModel()
        {
            this.Form = new GRPFormModel();
        }

        /// <summary>
        /// 查詢條件 FromModel
        /// </summary>
        public GRPFormModel Form { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class GRPFormModel: PagingResultsViewModel
    {
        /// <summary>
        /// 群組編號
        /// </summary>
        [Display(Name = " 群組編號")]
        [Control(Mode = Control.TextBox, size = "10", maxlength = "3")]
        public string grp_id { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [Display(Name = "群組名稱")]
        [Control(Mode = Control.TextBox, size = "30")]
        public string grpname { get; set; }

        public string srv_city { get; set; }
        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<GRPGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GRPGridModel : TblAMGRP
    {
       
    }
    
}