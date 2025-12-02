using ES.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_011008Model
    {
        //Apply_011008 ES.Models.Entities

        /// <summary>
        /// 案件號碼
        /// </summary>
        public string APP_ID { get; set; } //varchar 20     

        /// <summary>
        /// [申請類別]（1:更名或2:汙損）
        /// </summary>
        /// 
        [Display(Name = "申請類別")]
        [Required]
        public string APPLY_TYPE { get; set; } //varchar 1  

        /// <summary>
        /// 申辦日期
        /// </summary>
        public DateTime? APPLY_DATE { get; set; } //datetime

        /// <summary>
        /// 姓名(更正前)
        /// </summary>
        [Display(Name = "姓名(更正前)")]
        public string CHG_NAME { get; set; } //varchar 20    

        /// <summary>
        /// EMAIL 
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string EMAIL { get; set; } //varchar 100 允許
        /// <summary>
        ///  電話(公)
        /// </summary>
        [Display(Name = "電話(公)")]
        public string W_TEL { get; set; } //varchar 30 允許
        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL { get; set; } //varchar 30 允許
        /// <summary>
        /// 行動電話	
        /// </summary>
        [Display(Name = "行動電話")]
        public string MOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 通訊地址(含郵遞區號)	
        /// </summary>
        [Display(Name = "通訊地址(含郵遞區號)")]
        public string C_ZIPCODE { get; set; } //varchar 5 允許
        /// <summary>
        /// 通訊地址(含郵遞區號)	
        /// </summary>
        [Display(Name = "通訊地址(含郵遞區號)")]
        public string C_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 戶籍地址郵遞
        /// </summary>
        [Display(Name = "戶籍地址郵遞")]
        public string H_ZIPCODE { get; set; } //varchar 5 允許
        /// <summary>
        /// 戶籍地址郵遞
        /// </summary>
        [Display(Name = "戶籍地址郵遞")]
        public string H_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 同通訊地址(戶籍地)
        /// </summary>
        public string H_EQUAL { get; set; } //varchar 1 允許

        /// <summary>
        /// 考試年度	
        /// </summary>
        [Display(Name = "考試年度")]
        public string TEST_YEAR { get; set; } //varchar 3 允許
        /// <summary>
        /// 考試名稱類別
        /// </summary>
        [Display(Name = "考試名稱類別")]
        public string TEST_CATEGORY { get; set; } //nvarchar 200 允許

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        public string MERGEYN { get; set; } //varchar 1 允許
        /// <summary>
        /// 身分證正面影本
        /// </summary>
        public string FILE_IDNF { get; set; } //varchar 20 允許
        /// <summary>
        /// 身分證反面影本
        /// </summary>
        public string FILE_IDNB { get; set; } //varchar 20 允許
        /// <summary>
        /// 一年內2吋正面脫帽半身照片
        /// </summary>
        public string FILE_PHOTO { get; set; } //varchar 20 允許
        /// <summary>
        /// 考試院考試及格證書影本或電子證書
        /// </summary>
        public string FILE_PASSCOPY { get; set; } //varchar 20 允許
        /// <summary>
        /// 戶籍謄本或戶口名簿影本
        /// </summary>
        public string FILE_HOUSEHOLD { get; set; } //varchar 20 允許

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