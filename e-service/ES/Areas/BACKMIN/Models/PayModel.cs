using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class PayModel
    {
        public PayModel()
        {
            this.queryModel = new MaintainModel();
            this.viewModel = new MaintainViewModel();
        }
        public MaintainModel queryModel { get; set; }
        public MaintainViewModel viewModel { get; set; }
        public string APP_ID { get; set; }
        public string ActionType { get; set; }
    }

    public class MaintainModel
    {
        public string viewAPP_ID { get; set; }
        public virtual int NowPage { get; set; }

        [Display(Name = "申辦編號")]
        public virtual string APP_ID_BEGIN { get; set; }

        [Display(Name = "申辦編號")]
        public virtual string APP_ID_END { get; set; }

        [Display(Name = "申辦日期")]
        public virtual DateTime? APP_TIME_BEGIN { get; set; }

        [Display(Name = "申辦日期")]
        public virtual DateTime? APP_TIME_END { get; set; }

        [Display(Name = "承辦單位")]
        public virtual string UNIT_TYPE { get; set; }

        [Display(Name = "案件類別")]
        public virtual string SC_PID { get; set; }

        [Display(Name = "案件類別")]
        public virtual string SRV_ID { get; set; }

        [Display(Name = "處理進度")]
        public virtual string CLOSE_MK { get; set; }

        [Display(Name = "繳費狀態")]
        public virtual string PAY_STATUS { get; set; }

        [Display(Name = "身份字號/統編")]
        public virtual string IDN { get; set; }

        [Display(Name = "繳費方式")]
        public virtual string PAY_WAY { get; set; }

        [Display(Name = "排序欄位")]
        public virtual string OderByCol { get; set; }

        [Display(Name = "排序方法")]
        public virtual string SortAZ { get; set; }
    }

    public class MaintainViewModel
    {
        [Display(Name = "案件編號")]
        public virtual string SRV_ID { get; set; }

        [Display(Name = "會員ID/統編")]
        public virtual string IDN { get; set; }

        public virtual string CARD_IDN { get; set; }

        [Display(Name = "申請時間")]
        public virtual string APP_TIME { get; set; }

        [Display(Name = "繳費時機")]
        public virtual string PAY_POINT { get; set; }

        public virtual int? PAY_A_FEE { get; set; }

        public virtual int? PAY_A_PAID { get; set; }

        public virtual int? PAY_C_FEE { get; set; }

        public virtual int? PAY_C_PAID { get; set; }

        [Display(Name = "案件名稱")]
        public virtual string SRV_NAME { get; set; }

        [Display(Name = "以案案號")]
        public virtual string MOHW_CASE_NO { get; set; }

        [Display(Name = "案件進度")]
        public virtual string FLOW_NAME { get; set; }
        [Display(Name = "以案案號")]
        public virtual string SESSION_KEY { get; set; }

        public virtual int? UNIT_CD { get; set; }

        public virtual string FLOW_CD { get; set; }

        [Display(Name = "會員名稱")]
        public virtual string NAME { get; set; }

        public virtual string SRC_SRV_ID { get; set; }

        [Display(Name = "處理起算日")]
        public virtual string APP_STR_DATE { get; set; }

        public virtual string CASE_BACK_MK { get; set; }

        public virtual string PAY_BACK_MK { get; set; }

        [Display(Name = "結案狀態")]
        public virtual string CLOSE_MK { get; set; }

        [Display(Name = "繳費方式")]
        public virtual string PAY_WAY { get; set; }

        public virtual DateTime? SETTLE_DATE { get; set; }

        public virtual string APP_ID { get; set; }
        /// <summary>
        /// 繳費狀態
        /// </summary>
        public virtual string PAY_STATUS { get; set; }
    }

    public class FormatReportModel
    {
        public virtual string DefaultDay { get; set; }
    }

    public class OptionModel
    {
        public virtual string NAME { get; set; }
        public virtual string VALUE { get; set; }
    }

    public static class SqlModel
    {
        public static String GetMaintainSQL(String PAY_METHOD, String PAY_STATUS)
        {
            String temp = @"SELECT
                            a.APP_ID,
                            s.NAME as SRV_NAME,
                            a.APP_TIME,
                            msc.FLOW_NAME,
                            msc.CLOSE_MK as MSC_CLOSE_MK,
                            a.APP_EXT_DATE,
                            a.NAME,
                            a.PAY_POINT,
                            a.PAY_A_FEE,
                            a.PAY_A_PAID,
                            a.PAY_C_FEE,
                            a.PAY_C_PAID,
                            a.CLOSE_MK,
                            a.PAY_BACK_MK,
                            a.CASE_BACK_MK,
                            a.PAY_METHOD,
                            /*ap.SETTLE_DATE*/
                            (SELECT MAX(SETTLE_DATE) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) AS SETTLE_DATE,
                            isnull(ap.PAY_STATUS_MK,'N') as PAY_STATUS_MK
                            ,ap.PAY_BANK
                            from APPLY a
                            LEFT JOIN APPLY_PAY ap on ap.APP_ID = a.APP_ID
                            LEFT JOIN SERVICE s on a.SRV_ID = s.SRV_ID
                            LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
                            LEFT JOIN M_CASE_STATUS msc on msc.UNIT_SCD = u.UNIT_SCD and a.FLOW_CD = msc.FLOW_CD
                            where /*msc.CASE_CAT = 4 and*/ a.PAY_POINT in ('B','C','D') ";

            if (!String.IsNullOrEmpty(PAY_METHOD))
            {
                temp += "and a.PAY_METHOD in(" + PAY_METHOD + ") ";
            }
            switch (PAY_STATUS)
            {
                case "":
                    break;
                case "1":
                    temp += "and A.PAY_A_FEE <= A.PAY_A_PAID AND (SELECT MAX(SETTLE_DATE) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) IS NULL ";
                    //temp += "and ap.PAY_STATUS_MK = 'Y' and ap.SETTLE_DATE is null ";
                    break;
                case "2":
                    temp += "and A.PAY_A_FEE <= A.PAY_A_PAID AND (SELECT MAX(SETTLE_DATE) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) IS NOT NULL ";
                    //temp += "and ap.PAY_STATUS_MK = 'Y' and ap.SETTLE_DATE is not null ";
                    break;
                case "0":
                    temp += "and A.PAY_A_FEE > A.PAY_A_PAID ";
                    //temp += "and ap.PAY_STATUS_MK = 'N' ";
                    break;
            }
            return temp;
        }

        public static String GetMaintainViewSQL()
        {
            String temp = @"select 
                            a.SRV_ID,
                            a.IDN,
                            a.CARD_IDN,
                            a.APP_TIME,
                            a.PAY_POINT,
                            a.PAY_A_FEE,
                            a.PAY_A_PAID,
                            a.PAY_C_FEE,
                            a.PAY_C_PAID,
                            s.NAME as SRV_NAME,
                            a.MOHW_CASE_NO,
                            msc.code_desc as FLOW_NAME,
                            a.UNIT_CD,
                            a.FLOW_CD,
                            a.NAME,
                            a.SRC_SRV_ID,
                            a.APP_TIME as APP_STR_DATE,
                            a.CASE_BACK_MK,
                            a.PAY_BACK_MK,
                            a.CLOSE_MK,
                            a.PAY_METHOD,
                            ap.SETTLE_DATE,
                            a.APP_ID,
                            ap.SESSION_KEY,
                            ap.PAY_STATUS_MK
                            from APPLY a
                            LEFT JOIN SERVICE s on a.SRV_ID = s.SRV_ID
                            LEFT JOIN APPLY_PAY ap on ap.APP_ID = a.APP_ID
                            LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
                            LEFT JOIN CODE_CD msc on msc.CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = u.UNIT_CD) and msc.CODE_KIND = 'F_CASE_STATUS'  and a.FLOW_CD = msc.CODE_CD
                            where a.app_id=@APP_ID /*and msc.CASE_CAT=4*/ ";
            return temp;
        }

        public static String GetFormatReportOKSQL()
        {
            String temp = @"select a.APP_ID,s.NAME as SRV_NAME,a.APP_TIME,a.NAME,a.PAY_POINT,a.CLOSE_MK
                            ,a.PAY_BACK_MK,a.CASE_BACK_MK,u.UNIT_NAME
                            ,ap.PAY_METHOD,IsNull(ap.PAY_MONEY,0),IsNull(ap.PAY_PROFEE,0),ap.PAY_DESC,ap.PAY_INC_TIME,ap.PAY_ID,ap.SESSION_KEY
                            ,s.ACCOUNT_NAME,s.ACCOUNT_CD,ap.AUTH_NO,ap.AUTH_DATE,
                            ap.PAY_ACT_TIME, ap.SETTLE_DATE, a.ENAME
                            from APPLY a
                            LEFT JOIN service s on a.SRV_ID = s.SRV_ID
                            LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
                            LEFT JOIN APPLY_PAY ap on a.APP_ID = ap.APP_ID
                            where a.PAY_POINT!='A' and a.PAY_A_FEE=a.PAY_A_PAID ";
            return temp;
        }
        public static String GetFormatReportERRSQL()
        {
            String temp = @" select a.APP_ID,s.NAME as SRV_NAME,a.APP_TIME,a.NAME,a.PAY_POINT,IsNull(a.PAY_A_FEE,0)
                            ,IsNull(a.PAY_A_PAID,0),IsNull(a.PAY_C_FEE,0),IsNull(a.PAY_C_PAID,0)
                            ,a.CLOSE_MK,a.PAY_BACK_MK,a.CASE_BACK_MK,a.PAY_METHOD,u.UNIT_NAME, a.ENAME
                            from APPLY a
                            LEFT JOIN service s on a.SRV_ID = s.SRV_ID
                            LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
                            where 1=1";
            return temp;
        }

        public static String GetMaintainECSQL()
        {
            String temp = @"
                            SELECT distinct
                            a.APP_ID,
                            s.NAME as SRV_NAME,
                            a.APP_TIME,
                            msc.FLOW_NAME,
                            msc.CLOSE_MK as MSC_CLOSE_MK,
                            a.APP_EXT_DATE,
                            a.NAME,
                            a.PAY_POINT,
                            a.PAY_A_FEE,
                            a.PAY_A_PAID,
                            a.PAY_C_FEE,
                            a.PAY_C_PAID,
                            a.CLOSE_MK,
                            a.PAY_BACK_MK,
                            a.CASE_BACK_MK,
                            a.PAY_METHOD,
                            /*ap.SETTLE_DATE*/
                            (SELECT MAX(SETTLE_DATE) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) AS SETTLE_DATE,
                            isnull(ap.PAY_STATUS_MK,'N') as PAY_STATUS_MK,ap.PAY_BANK,
                            ISNULL((select top 1 case WHEN L_RESPCODE IS null THEN '尚未請款' when L_RESPCODE ='' then '請款中' WHEN L_RESPCODE ='00' THEN '成功' when L_RESPCODE ='A39' then '非檔案請款' ELSE '失敗' END AS RSP_NAME from PAY_ECRSP where PAY_ECRSP.C_ECID = ap.ECORDERID order by PAY_ECRSP.ADD_TIME desc),'尚未請款') AS RSP_NAME
                            ,ap.ECORDERID,(select top 1 L_RESPCODE from PAY_ECRSP where PAY_ECRSP.C_ECID = ap.ECORDERID order by PAY_ECRSP.ADD_TIME desc) AS L_RESPCODE
                            from APPLY a
                            LEFT JOIN APPLY_PAY ap on ap.APP_ID = a.APP_ID
                            LEFT JOIN SERVICE s on a.SRV_ID = s.SRV_ID
                            LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
                            LEFT JOIN M_CASE_STATUS msc on msc.UNIT_SCD = u.UNIT_SCD and a.FLOW_CD = msc.FLOW_CD
                            where /*msc.CASE_CAT = 4 and*/ a.PAY_POINT in ('B','C','D') and a.PAY_METHOD ='C' 
                            and A.PAY_A_FEE <= A.PAY_A_PAID AND (SELECT MAX(SETTLE_DATE) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) IS NULL 
                            and PAY_BANK='EC' 
                            /*AND ECORDERID IS NOT NULL AND AUTH_NO IS NOT NULL AND AUTH_DATE IS NOT NULL*/ ";

            return temp;
        }

        /// <summary>
        /// 聯合信用卡中心 請款檔
        /// </summary>
        /// <returns></returns>

        public static string exportECDetailListSQL()
        {
            String temp = @"select 
                    LEFT(CAST(ECORDERID as NVARCHAR) + REPLICATE(' ', 40), 40) as ECorderID_40 
                    ,LEFT(REPLICATE(' ',19),19) as empty_19 
                    ,RIGHT(REPLICATE('0', 8) + CAST(PAY_MONEY as NVARCHAR), 8) as ECmoney_8 
                    ,LEFT(CAST(AUTH_NO AS nvarchar) + REPLICATE(' ',8),8) AS AUTHNO_8 
                    ,'02' AS AUTHCODE_2 /*01:退貨,02:請款*/ 
                    ,CONVERT(VARCHAR,AUTH_DATE,112) AS AUTHDATE_8 
                    ,LEFT(CAST(a.APP_ID AS NVARCHAR) + REPLICATE(' ',16),16) AS PRIVATE_16 
                    ,LEFT(REPLICATE('　',20),20) AS empty_40 ,PAY_MONEY
                    ,LEFT(REPLICATE(' ',111),111) as empty_return
                    from apply a
                    join apply_pay on apply_pay.app_id = a.app_id 
                    where PAY_BANK='EC' 
                    AND AUTH_NO IS NOT NULL
                    /*AND ECORDERID IS NOT NULL 
                    AND AUTH_DATE IS NOT NULL*/
                    ";

            return temp;
        }
    }

}