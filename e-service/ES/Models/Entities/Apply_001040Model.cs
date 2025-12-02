using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;



namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001039醫事人員資格英文求證
    /// </summary>
    public class Apply_001040Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public string APP_ID { get; set; }
        /// <summary>
        /// 申辦日期
        /// </summary>
        public DateTime? APPLY_DATE { get; set; }
        /// <summary>
        /// 申請人中文姓名(與護照相同)
        /// </summary>
        public string CNAME { get; set; }
        /// <summary>
        /// 申請人身分證字號
        /// </summary>
        public string PID { get; set; }
        /// <summary>
        /// 申請人英文姓名(與護照相同)
        /// </summary>
        public string ENAME { get; set; }
        /// <summary>
        /// 申請人英文別名(與護照相同)
        /// </summary>
        public string ENAME_ALIAS { get; set; }
        /// <summary>
        /// 申請人出生年月日
        /// </summary>
        public DateTime? BIRTHDAY { get; set; }
        /// <summary>
        /// 申請事由
        /// </summary>
        public string APPLY_REASON { get; set; }
        /// <summary>
        /// 申請事由說明
        /// </summary>
        public string APPLY_REASON_DESC { get; set; }
        /// <summary>
        /// 申請證明類別
        /// </summary>
        public string LIC_TYPE { get; set; }
        /// <summary>
        /// 醫事人員證書字號
        /// </summary>
        public string LIC_NUM { get; set; }
        /// <summary>
        /// 醫事人員證書日期
        /// </summary>
        public DateTime? ISSUE_DATE { get; set; }
        /// <summary>
        /// 畢業學校名稱及所在縣市(醫事證照核發時之畢業學校名稱)中文
        /// </summary>
        public string SCHOOL_C { get; set; }
        /// <summary>
        /// 畢業學校名稱及所在縣市(醫事證照核發時之畢業學校名稱)英文
        /// </summary>
        public string SCHOOL_E { get; set; }
        /// <summary>
        /// 修業年限起
        /// </summary>
        public DateTime? EDU_START_DATE { get; set; }
        /// <summary>
        /// 修業年限迄
        /// </summary>
        public DateTime? EDU_END_DATE { get; set; }
        /// <summary>
        /// 通訊地址
        /// </summary>
        public string CONTACT_ADDRESS { get; set; }
        /// <summary>
        /// 本求證表郵寄地址(含國別及收件者)
        /// </summary>
        public string MAIL_ADDRESS { get; set; }
        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        public string CONTACT_NAME { get; set; }
        /// <summary>
        /// 聯絡人電話
        /// </summary>
        public string CONTACT_TEL { get; set; }
        /// <summary>
        /// 分機
        /// </summary>
        public string CONTACT_TEL_EXT { get; set; }
        /// <summary>
        /// 聯絡人行動電話
        /// </summary>
        public string CONTACT_MOBILE { get; set; }
        /// <summary>
        /// 聯絡人E-MAIL
        /// </summary>
        public string E_MAIL { get; set; }
        /// <summary>
        /// 聯絡人傳真
        /// </summary>
        public string CONTACT_FAX { get; set; }
        /// <summary>
        /// 分機
        /// </summary>
        public string CONTACT_FAX_EXT { get; set; }
        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        public bool IS_MERGE_FILE { get; set; }
        /// <summary>
        /// 護照影本
        /// </summary>
        public string PASSPORT_COPY_ATTH { get; set; }
        /// <summary>
        /// 醫事人員中/英文證書影本
        /// </summary>
        public string ME_CERT_COPY_ATTH { get; set; }
        /// <summary>
        /// 畢業證書影本
        /// </summary>
        public string BACHELOR_COPY_ATTH { get; set; }
        /// <summary>
        /// 考試及格證書影本
        /// </summary>
        public string CERT_COPY_ATTH { get; set; }
        /// <summary>
        /// 對方機構求證表格
        /// </summary>
        public string APPROVE_COPY_ATTH { get; set; }

    }
}