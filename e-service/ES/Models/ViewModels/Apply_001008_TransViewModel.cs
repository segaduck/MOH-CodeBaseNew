using ES.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models.ViewModels
{
    public class Apply_001008_TransViewModel : Apply_001008_TransModel
    {
        public string TRANS_ZIP { get; set; }

        public string TRANS_ZIP_ADDR { get; set; }


        public string TRANS_ZIP_DETAIL
        {
            get
            {
                return TRANS_ADDR;
            }
            set
            {
                TRANS_ADDR = value;
            }
        }

        /// <summary>
        /// 專科人員證書類別
        /// </summary>
        [Display(Name = "郵寄地址")]
        public string TRANS_ADDR { get; set; }

        /// <summary>
        /// 專科人員證書類別
        /// </summary>
        [Display(Name = "郵寄份數")]
        public int? TRANS_COPIES { get; set; }
    }
}