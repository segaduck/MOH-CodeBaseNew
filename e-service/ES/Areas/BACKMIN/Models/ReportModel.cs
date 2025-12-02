using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class DateModel
    {
        [Display(Name = "起始日期")]
        [Required]
        public virtual string Sdate { get; set; }

        [Display(Name = "結束日期")]
        [Remote("CheckDateIsOver", "Report", AdditionalFields = "Sdate", ErrorMessage = "結束日期不可大於起始日期!")]
        [Required]
        public virtual string Fdate { get; set; }
    }

    public class ReportModel
    {
        [Display(Name = "帳號")]
        public virtual string Account { get; set; }

        [Display(Name = "姓名")]
        public virtual string Name { get; set; }

        [Display(Name = "單位名稱")]
        public virtual string UnitName { get; set; }

        [Display(Name = "異動者帳號")]
        public virtual string UpdateAccount { get; set; }
    }

    public class SatisfactionScoresModel : DateModel
    {
        [Display(Name = "資料類型")]
        public virtual string Table_Type { get; set; }

        [Display(Name = "資料表")]
        public virtual string Rbl_Table { get; set; }
    }

    public class CaseSumModel : DateModel
    {
    }

    public class StatisticsLoginModel : DateModel
    {
    }

    public class StatisticsHotModel : DateModel
    {
    }

    public class StatisticsFileModel : DateModel
    {
    }

    public class StatisticsPaytypeModel : DateModel
    {
    }

    public class SocialWorker : DateModel
    {
        public SocialWorker()
        {
            this.queryModel = new SocialGridModel();
        }
        public SocialGridModel queryModel { get; set; }
    }
    public class SocialGridModel
    {
        public string viewAPP_ID { get; set; }
        public virtual int NowPage { get; set; }

    }
    public class CaseSumResultModel
    {
        public virtual string name { get; set; }
        public virtual int newcase { get; set; }
        public virtual int procase { get; set; }
        public virtual int okcase { get; set; }
        public virtual int ok_delay { get; set; }
        public virtual int unokcase { get; set; }
        public virtual int unok_delay { get; set; }

        public virtual int moica { get; set; }
        public virtual int moeaca { get; set; }
        public virtual int hca0 { get; set; }
        public virtual int hca1 { get; set; }
        public virtual int member { get; set; }
        public virtual int NEWEID { get; set; }

        public virtual int sumCase { get; set; }
        public virtual int sumOk { get; set; }
        public virtual int sumUnok { get; set; }

    }

    public static class LongWithSql
    {
        public static string GetCaseSumResultSql()
        {
            string CaseSumResultSql = @"with tblTmp as (
                            Select UNIT_CD,APP_ID,APP_STR_DATE,APP_EXT_DATE,APP_ACT_DATE,CLOSE_MK,LOGIN_TYPE,1 kind 
                            From APPLY
                            Where APP_TIME>=CONVERT(date,@Sdate) And APP_TIME<=CONVERT(date,@Fdate) 
                            and UNIT_CD in (select UNIT_CD from UNIT where UNIT_LEVEL=1)
                            UNION
                            Select UNIT_CD,APP_ID,APP_STR_DATE,APP_EXT_DATE,APP_ACT_DATE,CLOSE_MK,LOGIN_TYPE,2 kind 
                            From APPLY 
                            Where APP_TIME < CONVERT(date,@Sdate) And CLOSE_MK = 'N'  
                            and UNIT_CD in (select UNIT_CD from UNIT where UNIT_LEVEL=1)
                            )
                            ,
                            newcase as (
	                            Select UNIT_CD,count(*) as COUNT From tblTmp
	                            Where kind=1
	                            Group By UNIT_CD 
                            ),
                            moica as (
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where kind=1 and LOGIN_TYPE='MOICA'
                            Group By UNIT_CD
                            ),
                            moeaca as(
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where kind=1 and LOGIN_TYPE='MOEACA'
                            Group By UNIT_CD
                            ),
                            hca0 as (
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where kind=1 and LOGIN_TYPE='HCA0' 
                            Group By UNIT_CD
                            ),
                            hca1 as(
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where kind=1 and LOGIN_TYPE='HCA1'
                            Group By UNIT_CD
                            ),
                            NEWEID as(
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where kind=1 and LOGIN_TYPE='NEWEID'
                            Group By UNIT_CD
                            ),
                            procase as (
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where kind=2
                            Group By UNIT_CD
                            ),
                            okcase as (
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where CLOSE_MK = 'Y' And APP_ACT_DATE<=APP_EXT_DATE
                            Group By UNIT_CD
                            ),
                            ok_delay as (
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where CLOSE_MK = 'Y' And APP_ACT_DATE>APP_EXT_DATE 
                            Group By UNIT_CD
                            ),
                            unokcase as(
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where CLOSE_MK = 'N' And (isnull(APP_STR_DATE,0)=0 or APP_EXT_DATE>= GETDATE()
                            or APP_ID in (
                            select a.APP_ID from APPLY a, APPLY_PAUSE_TIME b 
                            where a.APP_ID = b.APP_ID and b.PAUSE_E = 0
                            ))
                            Group By UNIT_CD
                            ),
                            unok_delay as (
                            Select UNIT_CD,count(*) as COUNT From tblTmp
                            Where CLOSE_MK = 'N' And isnull(APP_STR_DATE,0)>0 and (APP_EXT_DATE < GETDATE()
                            and APP_ID not in (
                            select a.APP_ID from APPLY a, APPLY_PAUSE_TIME b 
                            where a.APP_ID = b.APP_ID and b.PAUSE_E = 0
                            ))
                            Group By UNIT_CD
                            ),
                            tblName as (
                            Select u.UNIT_NAME,u.UNIT_CD,u.SEQ_NO,n.COUNT as newcase,m.COUNT as moica,me.COUNT as moeaca,
                            h0.COUNT as hca0,h1.COUNT as hca1,new1.COUNT as NEWEID,p.COUNT as procase,oc.COUNT as okcase,od.COUNT as ok_delay,
                            ukc.COUNT as unokcase,ukd.COUNT as unok_delay
                            From UNIT u
                            LEFT JOIN newcase n on n.UNIT_CD = u.UNIT_CD
                            LEFT JOIN moica m on m.UNIT_CD = u.UNIT_CD
                            LEFT JOIN moeaca me on me.UNIT_CD = u.UNIT_CD
                            LEFT JOIN hca0 h0 on h0.UNIT_CD = u.UNIT_CD
                            LEFT JOIN hca1 h1 on h1.UNIT_CD = u.UNIT_CD
                            LEFT JOIN NEWEID new1 on new1.UNIT_CD = u.UNIT_CD
                            LEFT JOIN procase p on p.UNIT_CD = u.UNIT_CD
                            LEFT JOIN okcase oc on oc.UNIT_CD = u.UNIT_CD
                            LEFT JOIN ok_delay od on od.UNIT_CD = u.UNIT_CD
                            LEFT JOIN unokcase ukc on ukc.UNIT_CD = u.UNIT_CD
                            LEFT JOIN unok_delay ukd on ukd.UNIT_CD = u.UNIT_CD
                            Where u.UNIT_LEVEL=1
                            )
                            select * from (
                            Select UNIT_CD, UNIT_NAME, 
                            newcase, procase, okcase, ok_delay, unokcase, unok_delay, moica, moeaca, hca0, hca1,NEWEID
                            From tblName 
                            UNION
                            Select '99999' as UNIT_CD, '本署小計' as UNIT_NAME, 
                            sum(newcase) as newcase, sum(procase) as procase, sum(okcase) as okcase, sum(ok_delay) as ok_delay, sum(unokcase) as unokcase, 
                            sum(unok_delay) as unok_delay, sum(moica) as moica, sum(moeaca) as moeaca, sum(hca0) as hca0, sum(hca1) as hca1, sum(NEWEID) as NEWEID
                            From tblName 
                            ) a order by a.UNIT_CD";
            return CaseSumResultSql;
        }

        public static string GetHotSql()
        {
            string HotSql = @"with tblName as (
                                        select sc.NAME SCNAME,s.SRV_ID,s.NAME,count(a.APP_ID) as TIMES from APPLY a
                                        LEFT JOIN SERVICE s on s.SRV_ID = a.SRV_ID
                                        LEFT JOIN SERVICE_CATE sc on s.SC_ID = sc.SC_ID
                                        where a.APP_TIME>=Convert(date,@Sdate) and a.APP_TIME<=Convert(date,@Fdate)
                                        and a.UNIT_CD in (select UNIT_CD from Unit where UNIT_LEVEL=1)
                                        group by sc.NAME,sc.SEQ_NO,s.SRV_ID,s.NAME,s.SEQ_NO
                                        ),
                                        detail as(
                                        SELECT SRV_ID, 
                                        [MEMBER], [MOICA], [MOEACA], [HCA0], [HCA1], [NEWEID]
                                        FROM
                                        (
                                        select SRV_ID,LOGIN_TYPE,count(*) as val from APPLY 
                                        where APP_TIME>=Convert(date,@Sdate) and APP_TIME<=Convert(date,@Fdate)
                                        and UNIT_CD in (select UNIT_CD from Unit where UNIT_LEVEL=1)
                                         group by SRV_ID,LOGIN_TYPE
                                        ) AS SourceTable
                                        PIVOT(
                                        sum(val)
                                        FOR LOGIN_TYPE IN ([MEMBER], [MOICA], [MOEACA], [HCA0], [HCA1], [NEWEID])
                                        ) AS PivotTable
                                        )
                                        select t.SCNAME,t.SRV_ID,t.NAME,t.TIMES,/*d.MEMBER*/(t.TIMES-ISNULL(d.MOICA,0)-ISNULL(d.MOEACA,0)-ISNULL(d.HCA0,0)-ISNULL(d.HCA1,0)-ISNULL(d.NEWEID,0)) MEMBER,d.MOICA,d.MOEACA,d.HCA0,d.HCA1,d.NEWEID from tblName t
                                        LEFT JOIN detail d on d.SRV_ID = t.SRV_ID
                                        order by TIMES desc";
            return HotSql;
        }

        public static string GetFileSql()
        {
            /*
            string FileSql = @"with tmpFaculty as(
                                select UNIT_NAME,UNIT_CD,SEQ_NO
                                from UNIT
                                where UNIT_LEVEL=1
                                )
                                , tmpFile as(
                                select sc.SC_ID,sc.NAME sname,u.UNIT_NAME uname,sum(ISNULL(sfc.COUNTER,0)) as counter
                                from SERVICE_CATE sc
                                LEFT JOIN UNIT u on sc.UNIT_PCD = u.UNIT_CD
                                LEFT JOIN SERVICE s on s.SC_ID = sc.SC_ID
                                LEFT JOIN SERVICE_FILE_COUNT sfc on sfc.SRV_ID = s.SRV_ID
                                where sc.SC_PID = 0 and u.UNIT_CD in (select UNIT_CD from tmpFaculty)
                                and sfc.COUNT_DATE>=CONVERT(date,@Sdate) and sfc.COUNT_DATE<=CONVERT(date,@Fdate)
                                group by sc.SC_ID,sc.NAME,u.UNIT_NAME
                                )
                                select SC_ID,sname,uname,counter from tmpFile order by counter desc";
             */
            string FileSql = @"
                WITH T AS (
	                SELECT SC.SC_ID, SC.UNIT_CD, S.SRV_ID, S.NAME, SF.TITLE, SUM(SFC.COUNTER) AS TOTAL_COUNT 
	                FROM SERVICE_FILE_COUNT SFC, SERVICE S, SERVICE_CATE SC, SERVICE_FILE SF
	                WHERE SFC.SRV_ID = S.SRV_ID
	                AND S.SC_PID = SC.SC_ID
                    AND SFC.SRV_ID = SF.SRV_ID
	                AND SFC.FILE_ID = SF.FILE_ID
	                AND SFC.COUNT_DATE >= CONVERT(DATE, @Sdate)
	                AND SFC.COUNT_DATE <= CONVERT(DATE, @Fdate)
	                GROUP BY SC.SC_ID, SC.UNIT_CD, S.SRV_ID, S.NAME, SF.TITLE
                )
                SELECT T.SC_ID, T.NAME AS SNAME, U.UNIT_NAME AS UNAME, SUM(T.TOTAL_COUNT) AS COUNTER
                FROM T, UNIT U
                WHERE T.UNIT_CD = U.UNIT_CD
                GROUP BY T.SC_ID, T.NAME, U.UNIT_NAME
                ORDER BY 4 DESC
            ";


            return FileSql;
        }

        public static string GetFileDetailSql()
        {
            /*
            string FileDetailSql = @"select s.SRV_ID,s.NAME,sf.TITLE,ISNULL(sum(sfc.COUNTER),0) as total_count
                                from SERVICE s
                                LEFT JOIN SERVICE_FILE sf on s.SRV_ID = sf.SRV_ID
                                LEFT JOIN SERVICE_FILE_COUNT sfc on sf.SRV_ID=sfc.SRV_ID and sf.FILE_ID = sfc.FILE_ID
                                where s.SC_ID= @SC_ID
                                and sfc.COUNT_DATE>=CONVERT(date,@Sdate) and sfc.COUNT_DATE<=CONVERT(date,@Fdate)
                                group by s.SRV_ID,s.NAME,sf.TITLE order by total_count desc";
            */

            string FileDetailSql = @"
                SELECT S.SRV_ID, S.NAME, SF.TITLE, SUM(SFC.COUNTER) AS TOTAL_COUNT 
                FROM SERVICE_FILE_COUNT SFC, SERVICE S, SERVICE_CATE SC, SERVICE_FILE SF
                WHERE SFC.SRV_ID = S.SRV_ID
                AND S.SC_PID = SC.SC_ID
                AND S.SC_PID = @SC_ID
                AND SFC.SRV_ID = SF.SRV_ID
	            AND SFC.FILE_ID = SF.FILE_ID
                AND SFC.COUNT_DATE >= CONVERT(DATE, @Sdate)
                AND SFC.COUNT_DATE <= CONVERT(DATE, @Fdate)
                GROUP BY S.SRV_ID, S.NAME, SF.TITLE
                ORDER BY 4 DESC
            ";

            return FileDetailSql;
        }

        public static string GetPaytypeSql()
        {
            string PaytypeSql = @"with tmpFaculty as(
                                    select UNIT_NAME,UNIT_CD,SEQ_NO
                                    from UNIT
                                    where UNIT_LEVEL=1
                                    ), 
                                    Addall as(
                                    SELECT UNIT_CD, 
                                    [A], [C], [D], [T], [B], [S]
                                    FROM
                                    (
                                    Select a.UNIT_CD, ap.PAY_METHOD,count(*) as val
                                    From APPLY a
                                    LEFT JOIN APPLY_PAY ap on ap.APP_ID = a.APP_ID
                                    Where a.app_time>=CONVERT(date,@Sdate) And a.app_time<=CONVERT(date,@Fdate)
                                    And ap.PAY_MONEY > 0 
                                    and a.UNIT_CD in (select UNIT_CD from tmpFaculty)
                                    Group By a.UNIT_CD ,ap.PAY_METHOD
                                    ) AS SourceTable
                                    PIVOT(
                                    sum(val)
                                    FOR PAY_METHOD IN ([A], [C], [D], [T], [B], [S])
                                    ) AS PivotTable
                                    )
                                    ,tmpPayType as (
                                    Select s.NAME, s.SC_ID, s.UNIT_CD, s.SEQ_NO,
                                          ISNULL(a.A,0) as ATM, ISNULL(a.C,0) as CreditCard, ISNULL(a.D,0) as draft,
			                                    ISNULL(a.T,0) as transfer, ISNULL(a.B,0) as counter, ISNULL(a.S,0) as store 
                                     From SERVICE_CATE s
                                    LEFT JOIN Addall a on a.UNIT_CD = s.UNIT_CD
                                     Where s.SC_PID = 0 
                                     and s.UNIT_CD in (select UNIT_CD from tmpFaculty)
                                    )
                                    Select SC_ID, NAME, ATM, CreditCard, draft, transfer, counter, store,
                                           (ATM+CreditCard+draft+transfer+counter+store) as tatol
                                     From tmpPayType
                                     Order By SEQ_NO";
            return PaytypeSql;
        }

        public static string GetPaytypeDetailSql()
        {
            string PaytypeDetailSql = @" with tmpFaculty as(
                                            select UNIT_NAME,UNIT_CD,SEQ_NO
                                            from UNIT
                                            where UNIT_LEVEL=1
                                            )
                                            , Addall as(
                                            SELECT SRV_ID, 
                                            [A], [C], [D], [T], [B], [S]
                                            FROM
                                            (
                                            Select a.SRV_ID, ap.PAY_METHOD,count(*) as val
                                            From APPLY a
                                            LEFT JOIN APPLY_PAY ap on ap.APP_ID = a.APP_ID
                                            Where a.app_time>=CONVERT(date,'2006/09/09') And a.app_time<=CONVERT(date,'2013/09/09')
                                            And ap.PAY_MONEY > 0 
                                            and a.UNIT_CD in (select UNIT_CD from tmpFaculty)
                                            Group By a.SRV_ID ,ap.PAY_METHOD
                                            ) AS SourceTable
                                            PIVOT(
                                            sum(val)
                                            FOR PAY_METHOD IN ([A], [C], [D], [T], [B], [S])
                                            ) AS PivotTable
                                            )

                                            ,tmpPayType as (
                                            Select Distinct s.SC_ID as SC_ID, s.NAME, u.SEQ_NO as fseq, s.SEQ_NO as sseq,
                                                  ISNULL(ad.A,0) as ATM, ISNULL(ad.C,0) as CreditCard, ISNULL(ad.D,0) as draft,
			                                            ISNULL(ad.T,0) as transfer, ISNULL(ad.B,0) as counter, ISNULL(ad.S,0) as store 
                                            From APPLY a
                                            LEFT JOIN SERVICE s ON s.SRV_ID = a.SRV_ID
                                            LEFT JOIN SERVICE_CATE sc on sc.SC_ID = s.SC_ID
                                            LEFT JOIN UNIT u ON u.UNIT_CD = a.UNIT_CD
                                            LEFT JOIN Addall ad on ad.SRV_ID = s.SRV_ID
                                             Where sc.SC_PID = @SC_ID and
                                            a.app_time>=CONVERT(date, @Sdate) And a.app_time<=CONVERT(date, @Fdate)
                                             and a.UNIT_CD in (select UNIT_CD from tmpFaculty)
                                            )
                                            Select SC_ID, NAME, ATM, CreditCard, draft, transfer, counter, store,
                                                   (ATM+CreditCard+draft+transfer+counter+store) as tatol
                                             From tmpPayType
                                            Order By fseq, sseq";
            return PaytypeDetailSql;
        }
    }

}