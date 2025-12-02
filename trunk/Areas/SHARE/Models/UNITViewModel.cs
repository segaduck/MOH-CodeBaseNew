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
    public class UNITViewModel
    {
        public UNITViewModel()
        {
            this.Form = new UNITFormModel();
        }

        /// <summary>
        /// 查詢條件 FromModel
        /// </summary>
        public UNITFormModel Form { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class UNITFormModel: PagingResultsViewModel
    {
        /// <summary>
        /// 單位代碼
        /// </summary>
        [Display(Name = "單位代碼")]
        [Control(Mode = Control.TextBox, size = "10", maxlength = "3")]
        public string unit_cd { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        [Display(Name = "單位名稱")]
        [Control(Mode = Control.TextBox, size = "30")]
        public string unit_nm { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<UNITGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UNITGridModel : TblUNIT
    {
       
    }
    
}