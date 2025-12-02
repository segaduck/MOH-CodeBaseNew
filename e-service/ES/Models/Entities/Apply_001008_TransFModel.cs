using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models.Entities
{
    public class Apply_001008_TransFModel
    {
        ///<summary>
        /// 案件編號
        ///</summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        ///<summary>
        /// 項目序號
        ///</summary>
        [Display(Name = "項目序號")]
        public int? SRL_NO { get; set; }

        ///<summary>
        /// 寄送地址郵遞區號
        ///</summary>
        [Display(Name = "寄送地址郵遞區號")]
        public string TRANSF_ZIP { get; set; }

        ///<summary>
        /// 寄送地址
        ///</summary>
        [Display(Name = "寄送地址")]
        public string TRANSF_ADDR { get; set; }

        /// <summary>
        /// 機構名稱
        /// </summary>
        [Display(Name = "機構名稱")]
        public string TRANSF_UNITNAME { get; set; }

        ///<summary>
        /// 寄送份數
        ///</summary>
        [Display(Name = "寄送份數")]
        public int? TRANSF_COPIES { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string DEL_MK { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public DateTime? DEL_TIME { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string DEL_FUN_CD { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string DEL_ACC { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public DateTime? UPD_TIME { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string UPD_FUN_CD { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string UPD_ACC { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public DateTime? ADD_TIME { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string ADD_FUN_CD { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string ADD_ACC { get; set; }
    }
}