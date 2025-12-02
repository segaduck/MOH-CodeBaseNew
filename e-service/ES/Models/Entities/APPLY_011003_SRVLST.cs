using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class APPLY_011003_SRVLST
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 服務經歷-服務單位名稱
        /// </summary>
        [Display(Name = "服務經歷-服務單位名稱")]
        public string SRV_NAM { get; set; }

        /// <summary>
        /// 服務經歷-職稱
        /// </summary>
        [Display(Name = "服務經歷-職稱")]
        public string SRV_TITLE { get; set; }

        /// <summary>
        /// 服務經歷-服務年資(起)
        /// </summary>
        [Display(Name = "服務經歷-服務年資(起)")]
        public DateTime? SRV_SYEAR { get; set; }

        /// <summary>
        /// 服務經歷-服務年資(迄)
        /// </summary>
        [Display(Name = "服務經歷-服務年資(迄)")]
        public DateTime? SRV_EYEAR { get; set; }

        /// <summary>
        /// 服務證明正本彩色檔
        /// </summary>
        [Display(Name = "服務證明正本彩色檔")]
        public string FILE_PICRGB { get; set; }

        /// <summary>
        /// 服務單位認可有案證明文件
        /// </summary>
        [Display(Name = "服務單位認可有案證明文件")]
        public string FILE_SRVPROVE { get; set; }

        /// <summary> 
        /// 勞工保險被保險人投保明細表影本(服務證明為團體或私立機構開具者需附)
        /// </summary>
        [Display(Name = "勞工保險被保險人投保明細表影本(服務證明為團體或私立機構開具者需附)")]
        public string FILE_LABOR { get; set; }

        /// <summary>
        /// 特殊需求1
        /// </summary>
        [Display(Name = "特殊需求1(勾選)")]
        public string SRVLST_DEMAND_1 { get; set; }

        /// <summary>
        /// 特殊需求2
        /// </summary>
        [Display(Name = "特殊需求1說明")]
        public string SRVLST_DEMAND_1_TEXT { get; set; }

        /// <summary>
        /// 服務單位立案或法人登記證書影本(服務證明為團體或私立機構開具者需附)
        /// </summary>
        [Display(Name = "服務單位立案或法人登記證書影本(服務證明為團體或私立機構開具者需附)")]
        public string FILE_LEGAL { get; set; }

        /// <summary>
        /// 特殊需求2
        /// </summary>
        [Display(Name = "特殊需求2(勾選)")]
        public string SRVLST_DEMAND_2 { get; set; }

        /// <summary>
        /// 特殊需求2
        /// </summary>
        [Display(Name = "特殊需求2說明")]
        public string SRVLST_DEMAND_2_TEXT { get; set; }

        /// <summary>
        /// 服務單位章程影本(服務證明為團體或私立機構開具者否需附)
        /// </summary>
        [Display(Name = "服務單位章程影本(服務證明為團體或私立機構開具者否需附)")]
        public string FILE_CHARTER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_MK { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? DEL_TIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_FUN_CD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_ACC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPD_TIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UPD_FUN_CD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UPD_ACC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADD_TIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ADD_FUN_CD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ADD_ACC { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        public string SEQ_NO { get; set; }
        
    }
}