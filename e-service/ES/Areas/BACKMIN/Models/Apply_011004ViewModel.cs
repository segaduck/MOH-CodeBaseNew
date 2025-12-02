using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel
    /// </summary>
    public class Apply_011004ViewModel
    {
        public Apply_011004ViewModel()
        {
            this.Form = new Apply_011004FormModel();
        }

        /// <summary>
        /// Form
        /// </summary>
        public Apply_011004FormModel Form { get; set; }
    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011004FormModel : ApplyModel
    {
        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_DATE_STR { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string EMAIL_1 { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string EMAIL_2 { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string EMAIL_3 { get; set; }

        /// <summary>
        /// 中文姓名
        /// </summary>
        [Display(Name = "中文姓名")]
        public string NAME { get; set; }

        /// <summary>
        /// 英文姓名
        /// </summary>
        [Display(Name = "英文姓名")]
        public string ENAME { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Display(Name = "申請人出生年月日")]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// 電話(公司)
        /// </summary>
        [Display(Name = "電話(公司)")]
        public string C_TEL { get; set; }

        /// <summary>
        /// 電話(公司)
        /// </summary>
        [Display(Name = "電話(公司)-區碼")]
        public string C_TEL_1 { get; set; }

        /// <summary>
        /// 電話(公司)
        /// </summary>
        [Display(Name = "電話(公司)")]
        public string C_TEL_2 { get; set; }

        /// <summary>
        /// 電話(公司)
        /// </summary>
        [Display(Name = "電話(公司)")]
        public string C_TEL_3 { get; set; }

        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL { get; set; }

        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL_1 { get; set; }

        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL_2 { get; set; }
        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL_3 { get; set; }

        /// <summary>
        /// 手機號碼
        /// </summary>
        [Display(Name = "手機號碼")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        public string H_ZIPCODE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string H_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        public string H_ADDR { get; set; }

        /// <summary>
        /// 申請用途
        /// </summary>
        [Display(Name = "申請用途")]
        public string APPLY_FOR { get; set; }

        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        public string APPLY_NUM { get; set; }

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        public string YEAR { get; set; }

        /// <summary>
        /// 考試類別
        /// </summary>
        [Display(Name = "考試類別")]
        public string TYPE { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE { get; set; }

        /// <summary>
        /// 社會工作師證書影本
        /// </summary>
        [Display(Name = "社會工作師證書影本")]
        public HttpPostedFileBase File_Copy { get; set; }
        public string File_Copy_TEXT { get; set; }

        /// <summary>
        /// 社會工作師證書影本
        /// </summary>
        [Display(Name = "社會工作師證書影本")]
        public string FILE_NAME { get; set; }

        /// <summary>
        /// 系統狀態
        /// </summary>
        [Display(Name = "系統狀態")]
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 補正原因
        /// </summary>
        [Display(Name = "補正原因")]
        public string NG_NOTE { get; set; }

         /// <summary>
        /// 預計結束日期
        /// </summary>
        [Display(Name = "預計結束日期")]
        public string APP_EXT { get; set; }

        /// <summary>
        /// 案件承辦人姓名
        /// </summary>
        [Display(Name = "案件承辦人姓名")]
        public string PRO_NAME { get; set; }

        /// <summary>
        /// 補件項目
        /// </summary>
        [Display(Name = "補件項目")]
        public string NGITEM { get; set; }

        /// <summary>
        /// 檔案下載
        /// </summary>
        public Apply_011004FileModel FileList { get; set; }

        /// <summary>
        /// 歷程
        /// </summary>
        public IList<ES.Models.LogModel> Grid { get; set; }

    }

    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_011004FileModel
    {
        public Apply_011004FileModel()
        {
            this.FILENAM = new List<FileGroupModel>();
        }
        public string APP_ID { get; set; }

        /// <summary>
        /// 志願服務運用計畫
        /// </summary>
        [Display(Name = "社會工作師證書影本")]
        public string FILE_COPY { get; set; }
        public string FILE_COPY_TEXT { get; set; }

        public List<FileGroupModel> FILENAM { get; set; }
    }

}