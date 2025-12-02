using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_001039醫師赴國外訓練英文保證函
    /// </summary>
    public class Apply_001039ViewModel : Apply_001039Model
    {
        public Apply_001039ViewModel()
        {
        }

        /// <summary>
        /// 申請表單
        /// </summary>
        public Apply_001039FormModel Form { get; set; }

        /// <summary>
        /// 補件表單
        /// </summary>
        public Apply_001039AppDocModel AppDoc { get; set; }
    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_001039FormModel : Apply_001039Model
    {

        /// <summary>
        /// 申請人中文姓名(與護照相同)
        /// </summary>
        [Required]
        [Display(Name = "申請人中文姓名")]
        public string CNAME { get; set; }


        /// <summary>
        /// 申請人身分證字號
        /// </summary>
        [Required]
        [Display(Name = "身分證編號/居留證號")]
        public string PID { get; set; }

        /// <summary>
        /// 申請人英文姓名(與護照相同)
        /// </summary>
        [Required]
        [Display(Name = "申請人英文姓名")]
        public string ENAME { get; set; }

        /// <summary>
        /// 申請人性別
        /// </summary>
        [Required]
        [Display(Name = "申請人性別")]
        public string GENDER { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Required]
        [Display(Name = "申請人出生年月日")]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// E.C.F.M.G.及格證書字號
        /// </summary>
        [Required]
        [Display(Name = "E.C.F.M.G.及格證書字號")]
        public string ECFMG { get; set; }

        /// <summary>
        /// 訓練醫院及科別(請用英文填寫)
        /// </summary>
        [Required]
        [Display(Name = "訓練醫院及科別")]
        public string HOSPITAL_DIVISION { get; set; }

        /// <summary>
        /// 前往國家(中、英文)
        /// </summary>
        [Required]
        [Display(Name = "前往國家")]
        public string COUNTRY { get; set; }

        /// <summary>
        /// 事由
        /// </summary>
        [Required]
        [Display(Name = "事由")]
        public string CAUSE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Required]
        [Display(Name = "通訊地址")]
        public string MAIL_ADDRESS { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Required]
        [Display(Name = "聯絡人姓名")]
        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 聯絡人行動電話
        /// </summary>
        [Required]
        [Display(Name = "聯絡人行動電話")]
        public string CONTACT_MOBILE { get; set; }

        /// <summary>
        /// 申辦日期_字串
        /// </summary>
        public string APPLY_DATE_STR { get; set; }

        /// <summary>
        /// 聯絡人電話_區碼
        /// </summary>
        [Display(Name = "聯絡人電話")]
        public string CONTACT_TEL_0 { get; set; }

        /// <summary>
        /// 聯絡人電話_號碼
        /// </summary>
        [Display(Name = "聯絡人電話")]
        public string CONTACT_TEL_1 { get; set; }

        /// <summary>
        /// 聯絡人電話_分機
        /// </summary>
        public string CONTACT_TEL_2 { get; set; }

        /// <summary>
        /// 聯絡人E_MAIL_1
        /// </summary>
        [Required]
        [Display(Name = "聯絡人E_MAIL")]
        public string E_MAIL_1 { get; set; }

        /// <summary>
        /// 聯絡人E_MAIL_2
        /// </summary>
        [Display(Name = "聯絡人E_MAIL")]
        public string E_MAIL_2 { get; set; }

        /// <summary>
        /// 聯絡人E_MAIL_3
        /// </summary>
        public string E_MAIL_3 { get; set; }

        /// <summary>
        /// 聯絡人傳真_區碼
        /// </summary>
        public string CONTACT_FAX_0 { get; set; }

        /// <summary>
        /// 聯絡人傳真_號碼
        /// </summary>
        public string CONTACT_FAX_1 { get; set; }

        /// <summary>
        /// 聯絡人傳真_分機
        /// </summary>
        public string CONTACT_FAX_2 { get; set; }

        /// <summary>
        /// 我國評鑑合格醫院保證函（向本部保證申請人如期學成返國時，將聘僱為該科相當職位之醫師）
        /// </summary>
        public HttpPostedFileBase FILE0 { get; set; }
        public string FILE0_TEXT { get; set; }

        /// <summary>
        /// 申請人自行提出書面保證函（保證學業完成如期返國）
        /// </summary>
        public HttpPostedFileBase FILE1 { get; set; }
        public string FILE1_TEXT { get; set; }

        /// <summary>
        /// 醫師證書影本
        /// </summary>
        public HttpPostedFileBase FILE2 { get; set; }
        public string FILE2_TEXT { get; set; }

        /// <summary>
        /// 國外契約或接受文件（正本或影本）
        /// </summary>
        public HttpPostedFileBase FILE3 { get; set; }
        public string FILE3_TEXT { get; set; }

        /// <summary>
        /// E.C.F.M.G.及格證書影本
        /// </summary>
        public HttpPostedFileBase FILE4 { get; set; }
        public string FILE4_TEXT { get; set; }

        /// <summary>
        /// 國民身分證正面影本
        /// </summary>
        public HttpPostedFileBase FILE5 { get; set; }
        public string FILE5_TEXT { get; set; }

        /// <summary>
        /// 國民身分證反面影本
        /// </summary>
        public HttpPostedFileBase FILE6 { get; set; }
        public string FILE6_TEXT { get; set; }

        /// <summary>
        /// 護照影本或有關部會許可出國文件影本
        /// </summary>
        public HttpPostedFileBase FILE7 { get; set; }
        public string FILE7_TEXT { get; set; }

        /// <summary>
        /// 個人執業發展規劃書
        /// </summary>
        public HttpPostedFileBase FILE8 { get; set; }
        public string FILE8_TEXT { get; set; }

        /// <summary>
        /// 醫師赴國外訓練英文保證函申請表
        /// </summary>
        public HttpPostedFileBase FILE9 { get; set; }
        public string FILE9_TEXT { get; set; }

    }

    /// <summary>
    /// 補件表單
    /// </summary>
    public class Apply_001039AppDocModel : Apply_001039Model
    {
        /// <summary>
        /// 補件欄位字串
        /// </summary>
        [Display(Name = "補件欄位字串")]
        public string FieldStr { get; set; }

        /// <summary>
        /// 申請人中文姓名(與護照相同)
        /// </summary>
        [Required]
        [Display(Name = "申請人中文姓名")]
        public string CNAME { get; set; }


        /// <summary>
        /// 申請人身分證字號
        /// </summary>
        [Required]
        [Display(Name = "申請人身分證字號")]
        public string PID { get; set; }

        /// <summary>
        /// 申請人英文姓名(與護照相同)
        /// </summary>
        [Required]
        [Display(Name = "申請人英文姓名")]
        public string ENAME { get; set; }

        /// <summary>
        /// 申請人性別
        /// </summary>
        [Required]
        [Display(Name = "申請人性別")]
        public string GENDER { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Required]
        [Display(Name = "申請人出生年月日")]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// E.C.F.M.G.及格證書字號
        /// </summary>
        [Required]
        [Display(Name = "E.C.F.M.G.及格證書字號")]
        public string ECFMG { get; set; }

        /// <summary>
        /// 訓練醫院及科別(請用英文填寫)
        /// </summary>
        [Required]
        [Display(Name = "訓練醫院及科別")]
        public string HOSPITAL_DIVISION { get; set; }

        /// <summary>
        /// 前往國家(中、英文)
        /// </summary>
        [Required]
        [Display(Name = "前往國家")]
        public string COUNTRY { get; set; }

        /// <summary>
        /// 事由
        /// </summary>
        [Required]
        [Display(Name = "事由")]
        public string CAUSE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Required]
        [Display(Name = "通訊地址")]
        public string MAIL_ADDRESS { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Required]
        [Display(Name = "聯絡人姓名")]
        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 聯絡人行動電話
        /// </summary>
        [Required]
        [Display(Name = "聯絡人行動電話")]
        public string CONTACT_MOBILE { get; set; }

        /// <summary>
        /// 申辦日期_字串
        /// </summary>
        public string APPLY_DATE_STR { get; set; }

        /// <summary>
        /// 聯絡人電話_區碼
        /// </summary>
        [Display(Name = "聯絡人電話")]
        public string CONTACT_TEL_0 { get; set; }

        /// <summary>
        /// 聯絡人電話_號碼
        /// </summary>
        public string CONTACT_TEL_1 { get; set; }

        /// <summary>
        /// 聯絡人電話_分機
        /// </summary>
        public string CONTACT_TEL_2 { get; set; }

        /// <summary>
        /// 聯絡人E_MAIL_1
        /// </summary>
        [Required]
        [Display(Name = "聯絡人E_MAIL")]
        public string E_MAIL_1 { get; set; }

        /// <summary>
        /// 聯絡人E_MAIL_2
        /// </summary>
        [Display(Name = "聯絡人E_MAIL")]
        public string E_MAIL_2 { get; set; }

        /// <summary>
        /// 聯絡人E_MAIL_3
        /// </summary>
        public string E_MAIL_3 { get; set; }

        /// <summary>
        /// 聯絡人傳真_區碼
        /// </summary>
        public string CONTACT_FAX_0 { get; set; }

        /// <summary>
        /// 聯絡人傳真_號碼
        /// </summary>
        public string CONTACT_FAX_1 { get; set; }

        /// <summary>
        /// 聯絡人傳真_分機
        /// </summary>
        public string CONTACT_FAX_2 { get; set; }

        /// <summary>
        /// 我國評鑑合格醫院保證函（向本部保證申請人如期學成返國時，將聘僱為該科相當職位之醫師）
        /// </summary>
        public HttpPostedFileBase FILE0 { get; set; }
        public string FILE0_TEXT { get; set; }

        /// <summary>
        /// 申請人自行提出書面保證函（保證學業完成如期返國）
        /// </summary>
        public HttpPostedFileBase FILE1 { get; set; }
        public string FILE1_TEXT { get; set; }

        /// <summary>
        /// 醫師證書影本
        /// </summary>
        public HttpPostedFileBase FILE2 { get; set; }
        public string FILE2_TEXT { get; set; }

        /// <summary>
        /// 國外契約或接受文件（正本或影本）
        /// </summary>
        public HttpPostedFileBase FILE3 { get; set; }
        public string FILE3_TEXT { get; set; }

        /// <summary>
        /// E.C.F.M.G.及格證書影本
        /// </summary>
        public HttpPostedFileBase FILE4 { get; set; }
        public string FILE4_TEXT { get; set; }

        /// <summary>
        /// 國民身分證正面影本
        /// </summary>
        public HttpPostedFileBase FILE5 { get; set; }
        public string FILE5_TEXT { get; set; }

        /// <summary>
        /// 國民身分證反面影本
        /// </summary>
        public HttpPostedFileBase FILE6 { get; set; }
        public string FILE6_TEXT { get; set; }

        /// <summary>
        /// 護照影本或有關部會許可出國文件影本
        /// </summary>
        public HttpPostedFileBase FILE7 { get; set; }
        public string FILE7_TEXT { get; set; }

        /// <summary>
        /// 個人執業發展規劃書
        /// </summary>
        public HttpPostedFileBase FILE8 { get; set; }
        public string FILE8_TEXT { get; set; }

        /// <summary>
        /// 醫師赴國外訓練英文保證函申請表
        /// </summary>
        public HttpPostedFileBase FILE9 { get; set; }
        public string FILE9_TEXT { get; set; }

        /// <summary>
        /// 審查狀態
        /// </summary>
        public string FLOW_CD { get; set; }

        /// <summary>
        /// 通知補件
        /// </summary>
        public string APPSTATUS { get; set; }
        public string MAILBODY { get; set; }

    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_001039DoneModel
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 件數
        /// </summary>
        public string Count { get; set; }
    }
}