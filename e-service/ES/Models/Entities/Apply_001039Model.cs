using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001039 醫師赴國外訓練英文保證函
    /// </summary>
    public class Apply_001039Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public DateTime? APPLY_DATE { get; set; }

        /// <summary>
        /// 申請人中文姓名(與護照相同)
        /// </summary>
        [Display(Name = "申請人中文姓名(與護照相同)")]
        public string CNAME { get; set; }

        /// <summary>
        /// 申請人身分證字號
        /// </summary>
        [Display(Name = "申請人身分證字號")]
        public string PID { get; set; }
        /// <summary>
        /// 申請人英文姓名(與護照相同)
        /// </summary>
        [Display(Name = "申請人英文姓名(與護照相同)")]
        public string ENAME { get; set; }
        /// <summary>
        /// 申請人性別
        /// </summary>
        [Display(Name = "申請人性別")]
        public string GENDER { get; set; }
        /// <summary>
        /// 申請人出生年月日
        /// </summary>
        [Display(Name = "申請人出生年月日")]
        public DateTime? BIRTHDAY { get; set; }
        /// <summary>
        /// E.C.F.M.G.及格證書字號
        /// </summary>
        [Display(Name = "E.C.F.M.G.及格證書字號")]
        public string ECFMG { get; set; }
        /// <summary>
        /// 訓練醫院及科別(請用英文填寫)
        /// </summary>
        [Display(Name = "訓練醫院及科別(請用英文填寫)")]
        public string HOSPITAL_DIVISION { get; set; }
        /// <summary>
        /// 前往國家(中、英文)
        /// </summary>
        [Display(Name = "前往國家(中、英文)")]
        public string COUNTRY { get; set; }
        /// <summary>
        /// 事由
        /// </summary>
        [Display(Name = "事由")]
        public string CAUSE { get; set; }
        /// <summary>
        /// 本求證表郵寄地址(含國別及收件者)
        /// </summary>
        [Display(Name = "本求證表郵寄地址(含國別及收件者)")]
        public string MAIL_ADDRESS { get; set; }
        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Display(Name = "聯絡人姓名")]
        public string CONTACT_NAME { get; set; }
        /// <summary>
        /// 聯絡人電話
        /// </summary>
        [Display(Name = "聯絡人電話")]
        public string CONTACT_TEL { get; set; }
        /// <summary>
        /// 分機
        /// </summary>
        [Display(Name = "電話分機")]
        public string CONTACT_TEL_EXT { get; set; }
        /// <summary>
        /// 聯絡人行動電話
        /// </summary>
        [Display(Name = "聯絡人行動電話")]
        public string CONTACT_MOBILE { get; set; }
        /// <summary>
        /// 聯絡人E-MAIL
        /// </summary>
        [Display(Name = "聯絡人E-MAIL")]
        public string E_MAIL { get; set; }
        /// <summary>
        /// 聯絡人傳真
        /// </summary>
        [Display(Name = "聯絡人傳真")]
        public string CONTACT_FAX { get; set; }
        /// <summary>
        /// 分機
        /// </summary>
        [Display(Name = "傳真分機")]
        public string CONTACT_FAX_EXT { get; set; }
        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE_FILE { get; set; }

        /// <summary>
        /// 我國評鑑合格醫院保證函（向本部保證申請人如期學成返國時，將聘僱為該科相當職位之醫師）
        /// </summary>
        [Display(Name = "我國評鑑合格醫院保證函（向本部保證申請人如期學成返國時，將聘僱為該科相當職位之醫師）")]
        public string ATTH1 { get; set; }

        /// <summary>
        /// 申請人自行提出書面保證函（保證學業完成如期返國）
        /// </summary>
        [Display(Name = "申請人自行提出書面保證函（保證學業完成如期返國）")]
        public string ATTH2 { get; set; }

        /// <summary>
        /// 醫師證書影本
        /// </summary>
        [Display(Name = "醫師證書影本")]
        public string ATTH3 { get; set; }

        /// <summary>
        /// 國外契約或接受文件（正本或影本）
        /// </summary>
        [Display(Name = "國外契約或接受文件（正本或影本）")]
        public string ATTH4 { get; set; }

        /// <summary>
        /// E.C.F.M.G.及格證書影本
        /// </summary>
        [Display(Name = "E.C.F.M.G.及格證書影本")]
        public string ATTH5 { get; set; }

        /// <summary>
        /// 國民身分證正面影本
        /// </summary>
        [Display(Name = "國民身分證正面影本")]
        public string ATTH6 { get; set; }

        /// <summary>
        /// 國民身分證反面影本
        /// </summary>
        [Display(Name = "國民身分證反面影本")]
        public string ATTH7 { get; set; }

        /// <summary>
        /// 護照影本或有關部會許可出國文件影本
        /// </summary>
        [Display(Name = "護照影本或有關部會許可出國文件影本")]
        public string ATTH8 { get; set; }

        /// <summary>
        /// 個人執業發展規劃書
        /// </summary>
        [Display(Name = "個人執業發展規劃書")]
        public string ATTH9 { get; set; }

        /// <summary>
        /// 醫師赴國外訓練英文保證函申請表
        /// </summary>
        [Display(Name = "醫師赴國外訓練英文保證函申請表")]
        public string ATTH10 { get; set; }

        /// <summary>
        /// 郵寄日期
        /// </summary>
        [Display(Name = "郵寄日期")]
        public DateTime? MAIL_DATE { get; set; }
        /// <summary>
        /// 掛號條碼
        /// </summary>
        [Display(Name = "掛號條碼")]
        public string MAIL_BARCODE { get; set; }

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