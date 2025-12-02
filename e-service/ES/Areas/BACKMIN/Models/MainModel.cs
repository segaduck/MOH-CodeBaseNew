using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using System.Data;
using ES.Commons;

namespace ES.Areas.Admin.Models
{
    public class MainModel
    {
        public List<TblMESSAGEBack> LatestMessages { get; set; }

        public List<VISIT_RECORDModel> VisitRecords { get; set; }

        public string barChartData { get; set; }

        public string pieChartData { get; set; }

        public DataTable CaseList { get; set; } 

        public string START_DATE_AC { get; set; }

        public string START_DATE_AC_TW
        {
            get
            {
                if (string.IsNullOrEmpty(START_DATE_AC))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(START_DATE_AC));
                }
            }
            set
            {
                START_DATE_AC = HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value));
            }
        }

        public string END_DATE_AC { get; set; }

        public string END_DATE_AC_TW
        {
            get
            {
                if (string.IsNullOrEmpty(END_DATE_AC))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(END_DATE_AC));
                }
            }
            set
            {
                END_DATE_AC = HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value));
            }
        }

    }

    public class LoginModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        [Display(Name = "帳號")]
        public string Account { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [Required(ErrorMessage = "請輸入驗證碼")]
        [Display(Name ="驗證碼")]
        public string ValidateCode { get; set; }
    }

    public class MenuModel
    {
        /// <summary>
        /// 項目編號
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 父項目編號
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 型態
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// 連結目標
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// 控制項
        /// </summary>
        public string Control { get; set; }

        /// <summary>
        /// 動作項
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 項目代碼
        /// </summary>
        public string Code { get; set; }
    }
}