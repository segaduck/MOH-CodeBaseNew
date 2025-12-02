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
    public class Apply_001039ViewModel
    {
        public Apply_001039ViewModel()
        {
        }

        /// <summary>
        /// Form
        /// </summary>
        public Apply_001039FormModel Form { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Apply_001039FormModel Detail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Apply_001039PreviewModel Preview { get; set; }
    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_001039FormModel : Apply_001039Model
    {
        /// <summary>
        /// 補件內容
        /// </summary>
        [Display(Name = "補件內容")]
        public string NOTE { get; set; }
        /// <summary>
        /// 補件勾選內容
        /// </summary>
        [Display(Name = "補件勾選內容")]
        public string chkNotice { get; set; }
        /// <summary>
        /// 申請人中文姓名(與護照相同)
        /// </summary>
        [Display(Name = "申請人中文姓名")]
        [Required]
        public string CNAME { get; set; }


        /// <summary>
        /// 申請人身分證字號
        /// </summary>
        [Display(Name = "身分證編號/居留證號")]
        [Required]
        public string PID { get; set; }

        /// <summary>
        /// 申請人英文姓名(與護照相同)
        /// </summary>
        [Display(Name = "申請人英文姓名")]
        [Required]
        public string ENAME { get; set; }

        /// <summary>
        /// 申請人性別
        /// </summary>
        [Display(Name = "申請人性別")]
        [Required]
        public string GENDER { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Display(Name = "申請人出生年月日")]
         [Required]
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Display(Name = "申請人出生年月日")]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// 申請人出生年月日-西元年
        /// </summary>
        [Display(Name = "申請人出生年月日")]
        public string BIRTHDAY_AD
        {
            get
            {
                if (!BIRTHDAY.HasValue)
                    return null;
                else
                {
                    return HelperUtil.DateTimeToString(BIRTHDAY);
                }
            }
            set
            {
                if (BIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToString(BIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    BIRTHDAY = HelperUtil.TransToDateTime(value);
            }
        }

        /// <summary>
        /// E.C.F.M.G.及格證書字號
        /// </summary>
        [Display(Name = "E.C.F.M.G.及格證書字號")]
        [Required]
        public string ECFMG { get; set; }

        /// <summary>
        /// 訓練醫院及科別(請用英文填寫)
        /// </summary>
        [Display(Name = "訓練醫院及科別")]
        [Required]
        public string HOSPITAL_DIVISION { get; set; }

        /// <summary>
        /// 前往國家(中、英文)
        /// </summary>
        [Display(Name = "前往國家")]
        [Required]
        public string COUNTRY { get; set; }

        /// <summary>
        /// 事由
        /// </summary>
        [Display(Name = "事由")]
        [Required]
        public string CAUSE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        [Required]
        public string MAIL_ADDRESS { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Display(Name = "聯絡人姓名")]
        [Required]
        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 聯絡人電話
        /// </summary>
        [Display(Name = "聯絡人電話")]
        public string CONTACT_TEL { get; set; }

        /// <summary>
        /// 聯絡人行動電話
        /// </summary>
        [Display(Name = "聯絡人行動電話")]
        [Required]
        public string CONTACT_MOBILE { get; set; }

        /// <summary>
        /// 申辦日期_字串
        /// </summary>
        [Display(Name = "申辦日期")]
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
        [Display(Name = "聯絡人E_MAIL")]
        public string E_MAIL { get; set; }

        /// <summary>
        /// 聯絡人E_MAIL_1
        /// </summary>
        [Display(Name = "聯絡人E_MAIL")]
        [Required]
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
        [Display(Name = "聯絡人傳真")]
        public string CONTACT_FAX { get; set; }

        /// <summary>
        /// 聯絡人傳真_區碼
        /// </summary>
        [Display(Name = "聯絡人傳真")]
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
        [Display(Name = "我國評鑑合格醫院保證函")]
        public string File1 { get; set; }
        public string File1_TEXT { get; set; }

        /// <summary>
        /// 申請人自行提出書面保證函（保證學業完成如期返國）
        /// </summary>
        [Display(Name = "申請人自行提出書面保證函")]
        public string File2 { get; set; }
        public string File2_TEXT { get; set; }

        /// <summary>
        /// 醫師證書影本
        /// </summary>
        [Display(Name = "醫師證書影本")]
        public string File3 { get; set; }
        public string File3_TEXT { get; set; }

        /// <summary>
        /// 國外契約或接受文件（正本或影本）
        /// </summary>
        [Display(Name = "國外契約或接受文件")]
        public string File4 { get; set; }
        public string File4_TEXT { get; set; }

        /// <summary>
        /// E.C.F.M.G.及格證書影本
        /// </summary>
        [Display(Name = "E.C.F.M.G.及格證書影本")]
        public string File5 { get; set; }
        public string File5_TEXT { get; set; }

        /// <summary>
        /// 國民身分證正面影本
        /// </summary>
        [Display(Name = "國民身分證正面影本")]
        public string File6 { get; set; }
        public string File6_TEXT { get; set; }

        /// <summary>
        /// 國民身分證反面影本
        /// </summary>
        [Display(Name = "國民身分證反面影本")]
        public string File7 { get; set; }
        public string File7_TEXT { get; set; }

        /// <summary>
        /// 護照影本或有關部會許可出國文件影本
        /// </summary>
        [Display(Name = "護照影本或有關部會許可出國文件影本")]
        public string File8 { get; set; }
        public string File8_TEXT { get; set; }

        /// <summary>
        /// 個人執業發展規劃書
        /// </summary>
        [Display(Name = "個人執業發展規劃書")]
        public string File9 { get; set; }
        public string File9_TEXT { get; set; }

        /// <summary>
        /// 醫師赴國外訓練英文保證函申請表
        /// </summary>
        [Display(Name = "醫師赴國外訓練英文保證函申請表")]
        public string File10 { get; set; }
        public string File10_TEXT { get; set; }

        /// <summary>
        /// 預計完成日
        /// </summary>
        public DateTime? APP_EXT { get; set; }

        /// <summary>
        /// 預計完成日
        /// </summary>
        public string APP_EXT_DATE { get; set; }

        /// <summary>
        /// 承辦人員姓名
        /// </summary>
        public string PRO_NAME { get; set; }

        /// <summary>
        /// 系統狀態
        /// </summary>
        public string APP_STATUS { get; set; }


        public Apply_001039FileModel FileList { get; set; }

        /// <summary>
        /// 案件狀態修改
        /// </summary>
        public string FLOW_CD { get; set; }

        /// <summary>
        /// 申辦項目
        /// </summary>
        [Display(Name = "申辦項目")]
        public string APPTYPE { get; set; }

        /// <summary>
        /// 公文文號
        /// </summary>
        public string MOHW_CASE_NO { get; set; }

        public string MAIL_DATE_AC { get; set; }

        public string ISMODIFY { get; set; }
        public string NOTICE_NOTE { get; set; }
    }

    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_001039FileModel
    {
        public string APP_ID { get; set; }

        /// <summary>
        /// 我國評鑑合格醫院保證函
        /// </summary>
        [Display(Name = "我國評鑑合格醫院保證函")]
        public string FILE0 { get; set; }
        public string FILE0_TEXT { get; set; }

        /// <summary>
        /// 申請人自行提出書面保證函
        /// </summary>
        [Display(Name = "申請人自行提出書面保證函")]
        public string FILE1 { get; set; }
        public string FILE1_TEXT { get; set; }

        /// <summary>
        /// 醫師證書影本
        /// </summary>
        [Display(Name = "醫師證書影本")]
        public string FILE2 { get; set; }
        public string FILE2_TEXT { get; set; }

        /// <summary>
        /// 國外契約或接受文件
        /// </summary
        [Display(Name = "國外契約或接受文件")]
        public string FILE3 { get; set; }
        public string FILE3_TEXT { get; set; }

        /// <summary>
        /// E.C.F.M.G.及格證書影本
        /// </summary
        [Display(Name = "E.C.F.M.G.及格證書影本")]
        public string FILE4 { get; set; }
        public string FILE4_TEXT { get; set; }

        /// <summary>
        /// 國民身分證正面影本
        /// </summary
        [Display(Name = "國民身分證正面影本")]
        public string FILE5 { get; set; }
        public string FILE5_TEXT { get; set; }

        /// <summary>
        /// 國民身分證反面影本
        /// </summary
        [Display(Name = "國民身分證反面影本")]
        public string FILE6 { get; set; }
        public string FILE6_TEXT { get; set; }

        /// <summary>
        /// 護照影本或有關部會許可出國文件影本
        /// </summary
        [Display(Name = "護照影本或有關部會許可出國文件影本")]
        public string FILE7 { get; set; }
        public string FILE7_TEXT { get; set; }

        /// <summary>
        /// 個人執業發展規劃書
        /// </summary
        [Display(Name = "個人執業發展規劃書")]
        public string FILE8 { get; set; }
        public string FILE8_TEXT { get; set; }

        /// <summary>
        /// 醫師赴國外訓練英文保證函申請表
        /// </summary
        [Display(Name = "醫師赴國外訓練英文保證函申請表")]
        public string FILE9 { get; set; }
        public string FILE9_TEXT { get; set; }

    }

    /// <summary>
    /// 檔案預覽
    /// </summary>
    public class Apply_001039PreviewModel
    {
        /// <summary>
        /// 志工基本資料清冊
        /// </summary>
        [Display(Name = "志工基本資料清冊")]
        public string FILENAME { get; set; }

        /// <summary>
        /// 運用單位組織章程
        /// </summary>
        [Display(Name = "運用單位組織章程")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 單位立案登記證書影本
        /// </summary>
        [Display(Name = "單位立案登記證書影本")]
        public string FILE_NO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "志工基本資料清冊")]
        public string SRC_NO { get; set; }
    }
}