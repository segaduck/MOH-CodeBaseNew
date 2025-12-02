using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// 案件申辦主檔
    /// </summary>
    public class ApplyModel
    {
        /// <summary>
        /// 案件號碼
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請項目
        /// </summary>
        public string SRV_ID { get; set; }

        /// <summary>
        /// 原申請項目
        /// </summary>
        public string SRC_SRV_ID { get; set; }

        /// <summary>
        /// 單位代碼
        /// </summary>
        public int? UNIT_CD { get; set; }

        /// <summary>
        /// 會員帳號
        /// </summary>
        public string ACC_NO { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Display(Name = "國民身分證統一編號")]
        public string IDN { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [Display(Name = "性別")]
        public string SEX_CD { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 申請人姓名/機構名稱
        /// </summary>
        [Display(Name = "申請人姓名/機構名稱")]
        public string NAME { get; set; }

        /// <summary>
        /// 申請人英文姓名/機構英文名稱
        /// </summary>
        [Display(Name = "申請人英文姓名")]
        public string ENAME { get; set; }

        /// <summary>
        /// 申請人英文別名
        /// </summary>
        [Display(Name = "申請人英文別名")]
        public string E_ALIAS_NAME { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Display(Name = "聯絡人姓名")]
        public string CNT_NAME { get; set; }

        /// <summary>
        /// 聯絡人英文姓名
        /// </summary>
        [Display(Name = "聯絡人姓名(英文)")]
        public string CNT_ENAME { get; set; }

        /// <summary>
        /// 負責人姓名
        /// </summary>
        [Display(Name = "負責人姓名")]
        public string CHR_NAME { get; set; }

        /// <summary>
        /// 負責人英文姓名
        /// </summary>
        [Display(Name = "負責人姓名(英文)")]
        public string CHR_ENAME { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "電話")]
        public string TEL { get; set; }

        /// <summary>
        /// 傳真
        /// </summary>
        [Display(Name = "傳真")]
        public string FAX { get; set; }

        /// <summary>
        /// 聯絡人電話
        /// </summary>
        [Display(Name = "聯絡人電話")]
        public string CNT_TEL { get; set; }

        /// <summary>
        /// 通訊地址(郵遞區號)
        /// </summary>
        [Display(Name = "通訊地址(郵遞區號)")]
        public string ADDR_CODE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string ADDR { get; set; }

        /// <summary>
        /// 英文通訊地址
        /// </summary>
        [Display(Name = "通訊地址(英文)")]
        public string EADDR { get; set; }

        /// <summary>
        /// 信用卡身分證字號
        /// </summary>
        public string CARD_IDN { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 繳費方式
        /// </summary>
        public string PAY_POINT { get; set; }
        /// <summary>
        /// 繳費方式-繳費金額
        /// </summary>
        [Display(Name = "繳費方式")]
        public string PAY_METHOD { get; set; }
        /// <summary>
        /// 退款
        /// </summary>
        public string PAY_BACK_MK { get; set; }

        /// <summary>
        /// 退款日期
        /// </summary>
        public DateTime? PAY_BACK_DATE { get; set; }

        /// <summary>
        /// 匯支票應付金額-繳費金額
        /// </summary>
        [Display(Name = "繳費金額")]
        public int? PAY_A_FEE { get; set; }

        /// <summary>
        /// 匯支票手續費
        /// </summary>
        public int? PAY_A_FEEBK { get; set; }

        /// <summary>
        /// 匯支票已繳費金額
        /// </summary>
        public int? PAY_A_PAID { get; set; }

        /// <summary>
        /// 信用卡應付金額
        /// </summary>
        public int? PAY_C_FEE { get; set; }

        /// <summary>
        /// 信用卡手續費
        /// </summary>
        public int? PAY_C_FEEBK { get; set; }

        /// <summary>
        /// 信用卡已繳費金額
        /// </summary>
        public int? PAY_C_PAID { get; set; }

        /// <summary>
        /// 已入帳
        /// </summary>
        public string CHK_MK { get; set; }

        /// <summary>
        /// 轉帳帳號
        /// </summary>
        public string ATM_VNO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string API_MK { get; set; }
        /// <summary>
        /// 是否列印
        /// </summary>
        public string PRINT_MK { get; set; }

        /// <summary>
        /// 移轉案件編號
        /// </summary>
        public string TRANS_ID { get; set; }

        /// <summary>
        /// 公文文號
        /// </summary>
        [Display(Name = "公文文號")]
        public string MOHW_CASE_NO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FLOW_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TO_MIS_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TO_ARCHIVE_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? APP_STR_DATE { get; set; }
        /// <summary>
        /// 預計完成日
        /// </summary>
        public DateTime? APP_EXT_DATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? APP_ACT_DATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string APP_DEFER_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? APP_DEFER_TIME_S { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? APP_DEFER_TIME_E { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? APP_DEFER_DAYS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? APP_DEFER_TIMES { get; set; }
        /// <summary>
        /// 分文帳號
        /// </summary>
        public string APP_DISP_ACC { get; set; }

        /// <summary>
        /// 分文註記
        /// </summary>
        public string APP_DISP_MK { get; set; }

        /// <summary>
        /// 承辦帳號
        /// </summary>
        public string PRO_ACC { get; set; }

        /// <summary>
        /// 承辦單位
        /// </summary>
        public int? PRO_UNIT_CD { get; set; }

        /// <summary>
        /// 是否補繳
        /// </summary>
        public string CLOSE_MK { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? APP_GRADE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? APP_GRADE_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string APP_GRADE_LOG { get; set; }
        /// <summary>
        /// 通知次數
        /// </summary>
        public int? NOTIFY_COUNT { get; set; }

        /// <summary>
        /// 通知類型
        /// </summary>
        public string NOTIFY_TYPE { get; set; }

        /// <summary>
        /// 是否退件
        /// </summary>
        public string CASE_BACK_MK { get; set; }

        /// <summary>
        /// 退件時間
        /// </summary>
        public DateTime? CASE_BACK_TIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DIGITAL { get; set; }
        /// <summary>
        /// 登入方式
        /// </summary>
        public string LOGIN_TYPE { get; set; }

        /// <summary>
        /// 刪除註記
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
        /// 
        /// </summary>
        public string MARITAL_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CERT_SN { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "行動電話")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 是否全部補件
        /// </summary>
        public string ISMODIFY { get; set; }

        /// <summary>
        /// 補件日期
        /// </summary>
        public DateTime? APPLY_NOTICE_DATE { get; set; }

        /// <summary>
        /// 補件說明
        /// </summary>
        [Display(Name = "備註")]
        public string NOTICE_NOTE { get; set; }

        /// <summary>
        /// 新增補件內容(含html)
        /// </summary>
        public string MAILBODY { get; set; }

        /// <summary>
        /// 補件內容
        /// </summary>
        public string NOTIBODY { get; set; }
        /// <summary>
        /// 公文介接
        /// </summary>
        public string TOKEN { get; set; }
    }
}