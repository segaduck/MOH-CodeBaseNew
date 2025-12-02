using ES.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_011010Model
    {
        //Apply_011010 ES.Models.Entities

        /// <summary>
        /// 案件號碼
        /// </summary>
        public string APP_ID { get; set; }
        /// <summary>
        /// 單位類型：1.公部門2.私部門3.公私立醫事機構
        /// </summary>
        [Display(Name = "單位類型")]
        public string UNIT_TYPE { get; set; }
        /// <summary>
        /// 單位名稱(私部門)
        /// 1.身心障礙福利機構/團體
        /// 2.老人及長期照顧福利機構/團體
        /// 3.兒少、婦女及家庭福利機構/團體
        /// 4.學校
        /// 5.矯正機關
        /// 6.社工師事務所及公會/協會/學會
        /// 7.其他(非屬上述類別)
        /// </summary>
        [Display(Name = "單位類型(私部門)")]
        public string UNIT_SUBTYPE { get; set; }
        /// <summary>
        /// 單位名稱
        /// </summary>
        [Display(Name ="單位名稱")]
        public string UNIT_NAME { get; set; }
        /// <summary>
        /// 單位聯絡人局處/部門
        /// </summary>
        [Display(Name ="單位聯絡人局處/部門")]
        public string UNIT_DEPART { get; set; }
        /// <summary>
        /// 單位聯絡人職稱
        /// </summary>
        [Display(Name = "單位聯絡人職稱")]
        public string UNIT_TITLE { get; set; }
        /// <summary>
        /// 單位聯絡人姓名
        /// </summary>
        [Display(Name = "單位聯絡人姓名")]
        public string UNIT_CNAME { get; set; }
        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        public string UNIT_TEL { get; set; }
        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string UNIT_EMAIL { get; set; }
        /// <summary>
        /// 單位社工人員總數類型
        /// 1.由總會/事務所統一推薦單位無分會/分部/分事務所
        /// 2.單位類型非協會(事務所)
        /// </summary>
        [Display(Name = "單位社工人員總數類型")]
        public string CNT_TYPE { get; set; }
        /// <summary>
        /// 總會人數 type='1'
        /// </summary>
        [Display(Name ="總會人數")]
        public string CNT_A { get; set; }
        /// <summary>
        /// 分會人數 type='1'
        /// </summary>
        [Display(Name ="分會人數")]
        public string CNT_B { get; set; }
        /// <summary>
        /// 單位社工人員總數 CNT_A+CNT_B
        /// 填寫說明：
        /// 1.依「全國社會工作專業人員選拔及表揚要點」第五點第一款規定之推薦單位社工人員總數。
        /// 2.推薦單位所屬分會、分事務所等，請由總會統一推薦，並分別填報總、分會之社工人員總數。
        /// update to
        /// 1.依「全國社會工作專業人員選拔及表揚要點」第五點第一款規定之推薦單位社工人員總數（含總會、分會、分事務所人數）。
        /// 2.推薦單位如有所屬分會、分事務所等，請統一由總會推薦。
        /// </summary>
        [Display(Name ="單位社工人員總數")]
        public string CNT_C { get; set; }
        /// <summary>
        /// 單位社工人員總數
        /// </summary>
        [Display(Name ="單位社工人員總數(非協會/事務所)")]
        public string CNT_D { get; set; }
        /// <summary>
        /// 績優社工獎推薦人數
        /// </summary>
        [Display(Name="績優社工獎推薦人數")]
        public string CNT_E { get; set; }
        /// <summary>
        /// 績優社工督導獎推薦人數
        /// </summary>
        [Display(Name ="績優社工督導獎推薦人數")]
        public string CNT_F { get; set; }
        /// <summary>
        /// 資深敬業獎推薦人數
        /// </summary>
        [Display(Name ="資深敬業獎推薦人數")]
        public string CNT_G { get; set; }
        /// <summary>
        /// 特殊貢獻獎推薦人數
        /// </summary>
        [Display(Name ="特殊貢獻獎推薦人數")]
        public string CNT_H { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        public string MERGEYN { get; set; }
        
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

    }
}