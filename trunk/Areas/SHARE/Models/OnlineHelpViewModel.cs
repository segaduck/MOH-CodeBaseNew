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
    public class OnlineHelpViewModel
    {
        public OnlineHelpViewModel()
        {
            this.Form = new OnlineHelpFormModel();
        }

        /// <summary>
        /// 查詢條件 FromModel
        /// </summary>
        public OnlineHelpFormModel Form { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class OnlineHelpFormModel
    {
        public string Help_Url { get; set; }
    }
        
}