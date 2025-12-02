using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using log4net;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using NPOI.SS.Formula.Functions;
using System.Web.UI.WebControls;

namespace ES.Areas.Admin.Action
{
    public class MainAction : BaseAction
    {
        /// <summary>
        /// 主頁面
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public MainAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 主頁面
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public MainAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(string accountNo)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT MAX(LOGIN_TIME) AS LAST_LOGIN_TIME
                FROM LOGIN_LOG
                WHERE LOGIN_ID = @ACC_NO
                    AND STATUS = 'O'
                    AND LOGIN_TIME < (SELECT MAX(LOGIN_TIME) FROM LOGIN_LOG WHERE LOGIN_ID = @ACC_NO AND STATUS = 'O')
            ";

            args.Add("ACC_NO", accountNo);

            return GetData(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList(int scope, int srvUnitCode, int unitCode)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT SRV_ID, NAME, UNIT_CD, (
                 SELECT COUNT(1)
                 FROM APPLY A, SERVICE S
                 WHERE (SC_ID = C.SC_ID OR S.SC_PID = C.SC_ID)
                  AND A.SRV_ID = S.SRV_ID
                  AND A.APP_DISP_MK = 'N'
                  AND A.TO_ARCHIVE_MK = 'N'
                        AND A.CLOSE_MK='N'
            ";

            if (scope != 1)
            {
                querySQL += @" AND S.SRV_ID IN (SELECT SRV_ID FROM SERVICE_UNIT WHERE UNIT_CD = @UNIT_CD) ";
                args.Add("UNIT_CD", unitCode);
            }

            querySQL += @"
                ) AS TOTAL, (
                 SELECT COUNT(1)
                 FROM APPLY A, SERVICE S
                 WHERE (SC_ID = C.SC_ID OR S.SC_PID = C.SC_ID)
                  AND A.SRV_ID = S.SRV_ID
                  AND A.APP_DISP_MK = 'Y'
                        AND A.FLOW_CD = '01'
                  AND A.TO_ARCHIVE_MK = 'N'
                        AND A.CLOSE_MK='N'
            ";

            if (scope != 1)
            {
                querySQL += @" AND S.SRV_ID IN (SELECT SRV_ID FROM SERVICE_UNIT WHERE UNIT_CD = @UNIT_CD) ";
                //args.Add("UNIT_CD", unitCode);
            }

            querySQL += @"
                ) AS TOTAL2
                FROM SERVICE_CATE C
                WHERE SC_PID = 0
            ";

            if (scope != 1)
            {
                querySQL += "AND C.UNIT_CD = @SRV_UNIT_CD ";
                args.Add("SRV_UNIT_CD", srvUnitCode);
            }

            querySQL += "ORDER BY C.SEQ_NO";

            logger.Debug("SQL: " + querySQL);

            return GetList(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList(int scope, int srvUnitCode, int unitCode, string accNo)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = "";


            if (srvUnitCode == 8)
            {
                //社工司
                return null;
            }
            else
            {

                querySQL += @"SELECT* FROM( ";
                querySQL += " SELECT 1 ORD,S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD,'未分案' AS FLOW_CD_NAME, ";
                querySQL += " COUNT(*) TOTAL,  ";
                querySQL += " COUNT(*) TOTAL2 ";
                querySQL += " FROM SERVICE S ";
                querySQL += " JOIN APPLY AS A ON S.SRV_ID = A.SRV_ID ";
                querySQL += " WHERE 1 = 1 ";
                //if (scope != 1)
                //{
                //    querySQL += " AND A.PRO_UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD = @UNIT_CD OR UNIT_CD = @UNIT_CD)) ";
                //}
                querySQL += " AND A.FLOW_CD = '1' AND A.DEL_MK = 'N' AND A.CLOSE_MK = 'N' AND A.APP_DISP_MK = 'N' ";
                querySQL += " AND EXISTS(SELECT ACC_NO FROM SERVICE_CASE WHERE SRV_ID = A.SRV_ID AND ACC_NO = @ACC_NO) ";
                querySQL += " AND A.FLOW_CD IN(SELECT CODE_CD FROM CODE_CD WHERE UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD =@UNIT_CD OR UNIT_CD = @UNIT_CD))) ";
                querySQL += " AND @ACC_NO IN(SELECT ACC_NO FROM ADMIN_LEVEL WHERE MN_ID = '151') ";
                querySQL += " GROUP BY S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD ";

                querySQL += " UNION ALL ";

                querySQL += " SELECT 2 ORD,S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD,( ";
                querySQL += " SELECT DISTINCT CODE_DESC FROM CODE_CD WHERE CODE_KIND = 'F_CASE_STATUS' AND CODE_CD = A.FLOW_CD ";
                querySQL += " AND CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = '4') ";
                querySQL += "     ) AS FLOW_CD_NAME, ";
                querySQL += "     SUM(CASE WHEN PRO_ACC = @ACC_NO THEN 1 ELSE 0 END) TOTAL,  ";
                querySQL += "     SUM(CASE WHEN PRO_ACC = null THEN 1 ELSE 0 END) TOTAL2 ";
                querySQL += "     FROM SERVICE S ";
                querySQL += " JOIN APPLY AS A ON S.SRV_ID = A.SRV_ID ";
                querySQL += " WHERE 1 = 1 ";
                if (scope != 1)
                {
                    querySQL += " AND A.PRO_UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD = @UNIT_CD OR UNIT_CD = @UNIT_CD)) ";
                }

                querySQL += " AND A.FLOW_CD = '1' AND A.APP_DISP_MK = 'Y' AND A.DEL_MK='N'  AND DATEDIFF(DAY, GETDATE(), A.APP_EXT_DATE)>= 0 ";
                querySQL += " AND A.FLOW_CD IN(SELECT CODE_CD FROM CODE_CD WHERE UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD =@UNIT_CD OR UNIT_CD = @UNIT_CD))) ";
                querySQL += " AND S.SRV_ID IN(SELECT SRV_ID FROM SERVICE_CASE WHERE ACC_NO = @ACC_NO) ";
                querySQL += " GROUP BY S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD ";

                querySQL += " UNION ALL ";

                querySQL += " SELECT 3 ORD,S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD,( ";
                querySQL += " SELECT DISTINCT CODE_DESC FROM CODE_CD WHERE CODE_KIND = 'F_CASE_STATUS' AND CODE_CD = A.FLOW_CD ";
                querySQL += " AND CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = '4') ";
                querySQL += "     ) AS FLOW_CD_NAME, ";
                querySQL += "     SUM(CASE WHEN PRO_ACC = @ACC_NO THEN 1 ELSE 0 END) TOTAL,  ";
                querySQL += "     SUM(CASE WHEN PRO_ACC = null THEN 1 ELSE 0 END) TOTAL2 ";
                querySQL += "     FROM SERVICE S ";
                querySQL += " JOIN APPLY AS A ON S.SRV_ID = A.SRV_ID ";
                querySQL += " WHERE 1 = 1 ";
                if (scope != 1)
                {
                    querySQL += " AND A.PRO_UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD = @UNIT_CD OR UNIT_CD = @UNIT_CD)) ";
                }

                querySQL += " AND A.FLOW_CD = '3' AND A.APP_DISP_MK = 'Y' AND A.DEL_MK='N'  AND DATEDIFF(DAY, GETDATE(), A.APP_EXT_DATE)>= 0 ";
                querySQL += " AND A.FLOW_CD IN(SELECT CODE_CD FROM CODE_CD WHERE UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD =@UNIT_CD OR UNIT_CD = @UNIT_CD))) ";
                querySQL += " AND S.SRV_ID IN(SELECT SRV_ID FROM SERVICE_CASE WHERE ACC_NO = @ACC_NO) ";
                querySQL += " GROUP BY S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD ";

                querySQL += " UNION ALL ";

                querySQL += " SELECT 4 ORD,SRV_ID,NAME,UNIT_CD,FLOW_CD,FLOW_CD_NAME, ";
                querySQL += " SUM(CASE WHEN PRO_ACC = @ACC_NO THEN 1 ELSE 0 END) TOTAL, ";
                querySQL += " SUM(CASE WHEN PRO_ACC = null THEN 1 ELSE 0 END) TOTAL2 ";
                querySQL += " FROM (";
                querySQL += " SELECT S.SRV_ID,S.NAME,A.UNIT_CD,'-1' FLOW_CD,'逾期' AS FLOW_CD_NAME,A.PRO_ACC,A.APP_ID ";
                querySQL += " FROM SERVICE S ";
                querySQL += " JOIN APPLY AS A ON S.SRV_ID = A.SRV_ID ";
                querySQL += " JOIN (SELECT * FROM CODE_CD JOIN (SELECT UNIT_SCD,UNIT_CD FROM UNIT) A ON A.UNIT_SCD=CODE_PCD AND CODE_KIND ='F_CASE_STATUS') AS B ON B.CODE_CD=A.FLOW_CD ";
                querySQL += " WHERE 1 = 1 ";
                if (scope != 1)
                {
                    querySQL += " AND A.PRO_UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD = @UNIT_CD OR UNIT_CD = @UNIT_CD)) ";
                }
                if (srvUnitCode == 4)
                {
                    //醫事司
                    querySQL += "AND ISNULL(A.FLOW_CD,'') NOT IN ('','0','8','9','12','13','14','15','16','17','18','19','20','50','52','99','98') ";
                }
                else
                {
                    querySQL += " AND ISNULL(A.FLOW_CD,'') NOT IN ('','0') ";
                }
                querySQL += " AND A.APP_DISP_MK = 'Y' AND A.DEL_MK='N'  AND DATEDIFF(DAY, GETDATE(), A.APP_EXT_DATE)< 0 ";
                querySQL += " AND A.FLOW_CD IN(SELECT CODE_CD FROM CODE_CD WHERE A.UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD =@UNIT_CD OR UNIT_CD = @UNIT_CD))) ";
                querySQL += " AND S.SRV_ID IN(SELECT SRV_ID FROM SERVICE_CASE WHERE ACC_NO = @ACC_NO) ";
                querySQL += " GROUP BY S.SRV_ID,S.NAME,A.UNIT_CD,A.PRO_ACC,A.APP_ID ";

                querySQL += " ) T GROUP BY SRV_ID,NAME,UNIT_CD,FLOW_CD,FLOW_CD_NAME ";

                querySQL += " UNION ALL ";

                querySQL += " SELECT 52 ORD,S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD,'返回重新分文' AS FLOW_CD_NAME, ";
                querySQL += " COUNT(*) TOTAL,  ";
                querySQL += " COUNT(*) TOTAL2 ";
                querySQL += " FROM SERVICE S ";
                querySQL += " JOIN APPLY AS A ON S.SRV_ID = A.SRV_ID ";
                querySQL += " WHERE 1 = 1 ";
                //if (scope != 1)
                //{
                //    querySQL += " AND A.PRO_UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD = @UNIT_CD OR UNIT_CD = @UNIT_CD)) ";
                //}
                querySQL += " AND A.FLOW_CD = '52' AND A.DEL_MK = 'N' AND A.CLOSE_MK = 'N' AND A.APP_DISP_MK = 'N' ";
                querySQL += " AND EXISTS(SELECT ACC_NO FROM SERVICE_CASE WHERE SRV_ID = A.SRV_ID AND ACC_NO = @ACC_NO) ";
                querySQL += " AND A.FLOW_CD IN(SELECT CODE_CD FROM CODE_CD WHERE UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE (UNIT_PCD =@UNIT_CD OR UNIT_CD = @UNIT_CD))) ";
                querySQL += " AND @ACC_NO IN(SELECT ACC_NO FROM ADMIN_LEVEL WHERE MN_ID = '151') ";
                querySQL += " GROUP BY S.SRV_ID,S.NAME,A.UNIT_CD,FLOW_CD ";

                querySQL += "     ) T WHERE ";
                querySQL += " FLOW_CD_NAME IS NOT NULL";
                querySQL += " ORDER BY ORD,SRV_ID,NAME,UNIT_CD,FLOW_CD ";

            }


            args.Add("ACC_NO", accNo);
            args.Add("UNIT_CD", srvUnitCode);


            logger.Debug("SQL: " + querySQL);

            return GetList(querySQL, args);
        }
    }
}