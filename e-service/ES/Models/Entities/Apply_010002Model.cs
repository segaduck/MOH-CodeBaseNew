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
    public class Apply_010002Model
    {
        /// <summary>
        /// 案件號碼
        /// </summary>
        public string APP_ID { get; set; } //varchar 20     
        /// <summary>
        /// 申辦日期
        /// </summary>
        public DateTime? APPLY_DATE { get; set; } //datetime
        /// <summary>
        /// 承辦單位
        /// </summary>
        public string  ORG_NAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人
        /// </summary>
        public string  APNAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人身份證統一編號/外籍統一證號或護照號碼
        /// </summary>
        public string  IDN { get; set; } //varchar 20     
        /// <summary>
        /// *出生年月日BIRTH 年 /月/日
        /// </summary>
        public DateTime? BIRTHDAY { get; set; } //date   允許
        /// <summary>
        /// *申請人電話 TEL
        /// </summary>
        public string  TEL { get; set; } //varchar 30 允許
        /// <summary>
        /// *申請人手機 MOBILE
        /// </summary>
        public string  MOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 申請人E-MAIL EMAIL
        /// </summary>
        public string  EMAIL { get; set; } //varchar 60 允許
        /// <summary>
        /// *配偶姓名 SPNAME
        /// </summary>
        public string  SPNAME { get; set; } //nvarchar 60    
        /// <summary>
        /// *配偶身份證統一編號/外籍統一證號或護照號碼 SPIDN/SPIDNS
        /// </summary>
        public string  SPIDN { get; set; } //varchar 20     
        /// <summary>
        /// *配偶出生年月日 SPBIRTH
        /// </summary>
        public DateTime? SPBIRTHDAY { get; set; } //date   允許
        /// <summary>
        /// *配偶電話 SPTEL
        /// </summary>
        public string  SPTEL { get; set; } //varchar 30 允許
        /// <summary>
        /// *配偶手機 SPMOBILE
        /// </summary>
        public string  SPMOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 配偶E-MAIL SPEMAIL
        /// </summary>
        public string  SPEMAIL { get; set; } //varchar 60 允許
        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string  C_ZIPCODE { get; set; } //varchar 5 允許
        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string  C_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string  H_ZIPCODE { get; set; } //varchar 5 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string  H_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 同戶籍地址
        /// </summary>
        public string  H_EQUAL { get; set; } //varchar 1 允許

        /// <summary>
        /// 國民身分證影印本正反面-同意自Mydata平台取得資料
        /// </summary>
        public string MYDATA_GET1 { get; set; }

        /// <summary>
        /// 中低收入戶或低收入戶證明文件-同意自Mydata平台取得資料
        /// </summary>
        public string MYDATA_GET2 { get; set; }

        /// <summary>
        /// 佐證文件檔案上傳--佐證文件採合併檔案是 否
        /// </summary>
        public string  MERGEYN { get; set; } //varchar 1 允許
        /// <summary>
        /// 國民身分證影印本(正面) 
        /// </summary>
        public string  FILE_IDCNF { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        public string  FILE_IDCNB { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        public string  FILE_DISEASE { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        public string  FILE_LOWREC { get; set; } //varchar 20 允許

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
        /// MyData tx_id
        /// </summary>
        public string MYDATA_TX_ID { get; set; }
        /// <summary>
        /// MyData ReturnUrl 帶回狀態碼
        /// </summary>
        public string MYDATA_RTN_CODE { get; set; }
        /// <summary>
        /// MyData ReturnUrl 帶回狀態碼說明文字
        /// </summary>
        public string MYDATA_RTN_CODE_DESC { get; set; }

        /// <summary>
        /// MyData ApiSP 交易回應時間
        /// </summary>
        public DateTime? MYDATA_TX_TIME { get; set; }
        /// <summary>
        /// MyData 個人戶籍資料 (json)
        /// </summary>
        public string MYDATA_IDCN { get; set; }
        /// <summary>
        /// MyData 低收入戶及中低收入戶證明 (json)
        /// </summary>
        public string MYDATA_LOWREC { get; set; }
        /// <summary>
        /// MyData 交易異常訊息(成功時為空白)
        /// </summary>
        public string MYDATA_TX_RESULT_MSG { get; set; }
    }
}