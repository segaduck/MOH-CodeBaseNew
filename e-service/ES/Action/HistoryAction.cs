using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Data.SqlClient;
using ES.Models;
using System.Text;
using ES.Utils;
using ES.Services;

namespace ES.Action
{
    public class HistoryAction : BaseAction
    {
        /// <summary>
        /// 案件紀錄查詢
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public HistoryAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 案件紀錄查詢
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public HistoryAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        ///// <summary>
        ///// 查詢線上申辦案件
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public List<Dictionary<string, object>> GetOnlineList(HistoryActionModel model)
        //{
        //    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        //    Dictionary<string, object> args = new Dictionary<string, object>();

        //    StringBuilder querySQL = new StringBuilder(@"
        //             SELECT S.SRV_ID, A.APP_ID, S.NAME AS SRV_NAME, A.APP_TIME, A.APP_ACT_DATE, (CASE
        //                    WHEN A.PAY_POINT = 'A' THEN '不需繳費'
        //                    WHEN A.PAY_POINT = 'D' THEN '審核後繳費'
        //                    WHEN A.PAY_A_FEE = A.PAY_A_PAID THEN '已繳費'
        //                    WHEN A.PAY_A_FEE = (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) THEN '繳費未確認'
        //                    WHEN A.PAY_A_FEE > (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) AND A.CLOSE_MK = 'N' THEN '補繳'
        //                    WHEN A.PAY_A_FEE > (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) AND A.CLOSE_MK = 'Y' THEN '補繳未確認'
        //                    WHEN A.PAY_BACK_MK = 'Y' THEN '退費' ELSE '不退費'
        //                END) AS PAY_STATUS,
        //                /*(SELECT F.FLOW_NAME FROM UNIT U, M_CASE_STATUS F WHERE U.UNIT_SCD = F.UNIT_SCD AND UNIT_CD = A.UNIT_CD AND F.FLOW_CD = A.FLOW_CD) AS FLOW_NAME,*/
        //                (SELECT CODE_DESC FROM UNIT U, CODE_CD F WHERE U.UNIT_SCD = F.CODE_PCD AND U.UNIT_CD = A.UNIT_CD AND F.CODE_CD = A.FLOW_CD AND F.CODE_KIND = 'F_CASE_STATUS') AS FLOW_NAME,
        //                (CASE
        //                    WHEN A.SRV_ID = '001035' THEN 'F'
        //                    WHEN SUBSTRING(A.SRV_ID, 1, 3) <> '001' THEN 'F'
        //                    WHEN (SELECT COUNT(1) FROM SURVEY_ONLINE WHERE APPLY_ID = A.APP_ID) > 0 THEN 'Y'
        //                    ELSE 'N'
        //                END) AS SURVEY_MK,
        //                (SELECT MAX(PAY_ACT_TIME) FROM APPLY_PAY WHERE DEL_MK = 'N' AND PAY_STATUS_MK = 'Y' AND APP_ID = A.APP_ID) AS PAY_ACT_TIME,
        //                (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE DEL_MK = 'N' AND PAY_STATUS_MK = 'Y' AND APP_ID = A.APP_ID) AS PAY_MONEY,
        //                ROW_NUMBER() OVER (ORDER BY A.APP_TIME DESC) AS _ROW_NO
        //            FROM APPLY A, SERVICE S 
        //            WHERE A.DEL_MK = 'N'
        //                AND A.SRV_ID = S.SRV_ID 
        //                AND A.ACC_NO = @ACC_NO ");

        //    // 申請人帳號
        //    args.Add("ACC_NO", model.Account);

        //    // 申請編號
        //    if (!String.IsNullOrEmpty(model.ApplyId))
        //    {
        //        querySQL.Append("AND A.APP_ID = @APP_ID ");
        //        args.Add("APP_ID", model.ApplyId);
        //    }

        //    // 分類
        //    if (!String.IsNullOrEmpty(model.ServiceId))
        //    {
        //        querySQL.Append("AND A.SRV_ID = @SRV_ID ");
        //        args.Add("SRV_ID", model.ServiceId);
        //    }

        //    // 申請日期 (起)
        //    if (model.ApplyDateS != null)
        //    {
        //        querySQL.Append("AND A.APP_TIME >= @APP_TIME_S ");
        //        args.Add("APP_TIME_S", model.ApplyDateS);
        //    }

        //    // 申請日期 (迄)
        //    if (model.ApplyDateE != null)
        //    {
        //        querySQL.Append("AND A.APP_TIME < DATEADD(D, 1, @APP_TIME_E) ");
        //        args.Add("APP_TIME_E", model.ApplyDateE);
        //    }

        //    list = GetList(querySQL.ToString(), model.NowPage, args);

        //    //logger.Debug("AA: " + querySQL);
        //    //logger.Debug("AA: " + list.Count);

        //    return list;
        //}

        ///// <summary>
        ///// 非線上申辦案件查詢
        ///// </summary>
        ///// <returns></returns>
        //public List<Dictionary<string, object>> GetNonWebList(HistoryActionModel model)
        //{
        //    List<Dictionary<string, object>> list = null;
        //    Dictionary<string, object> args = new Dictionary<string, object>();

        //    StringBuilder querySQL = new StringBuilder(@"
        //        SELECT F.CASE_NO, F.DOC_ID, F.CASE_ENT_DATE, F.CASE_ENT_PER,
        //            (SELECT TOP 1 NAME FROM ADMIN WHERE ACC_NO = F.CASE_ENT_PER ) AS ENT_NAME,
        //            (SELECT TEL FROM ADMIN WHERE ACC_NO = F.CASE_ENT_PER) AS ENT_EXT,
        //            (SELECT FLOW_NAME FROM M_CASE_STATUS WHERE FLOW_CD = F.FLOW_CD AND UNIT_SCD = F.CASE_ENT_UNIT) AS FLOW_NAME,
        //            (SELECT UNIT_NAME FROM UNIT WHERE UNIT_SCD = F.CASE_ENT_UNIT) AS UNIT_NAME,
        //            (SELECT TOP 1 CASE_SUBJECT FROM M_CASE_DATA WHERE CASE_NO = F.CASE_NO) AS CASE_SUBJECT
        //        FROM M_CASE_FLOW F
        //        WHERE 1=1 
        //    ");

        //    // 案號
        //    if (!String.IsNullOrEmpty(model.CaseNo))
        //    {
        //        querySQL.Append("AND F.CASE_NO = @CASE_NO ");
        //        args.Add("CASE_NO", model.CaseNo);
        //    }

        //    // 文號
        //    if (!String.IsNullOrEmpty(model.DocNo))
        //    {
        //        querySQL.Append("AND F.DOC_ID = @DOC_ID ");
        //        args.Add("DOC_ID", model.DocNo);
        //    }

        //    querySQL.Append("ORDER BY CASE_ENT_DATE, DOC_ID");

        //    list = GetList(querySQL.ToString(), args);

        //    return list;
        //}

        ///// <summary>
        ///// 非線上申辦案件查詢
        ///// </summary>
        ///// <returns></returns>
        //public List<Dictionary<string, object>> GetBatchList(HistoryActionModel model)
        //{
        //    List<Dictionary<string, object>> list = null;
        //    Dictionary<string, object> args = new Dictionary<string, object>();

        //    StringBuilder querySQL = new StringBuilder(@"
        //        SELECT F.CASE_NO, F.DOC_ID, F.CASE_ENT_PER, F.CASE_DUE_TIME, F.CASE_ENT_TIME, D.CASE_SUBJECT,
        //            (SELECT TOP 1 NAME FROM ADMIN WHERE ACC_NO = F.CASE_ENT_PER ) AS ENT_NAME,
        //            (SELECT TEL FROM ADMIN WHERE ACC_NO = F.CASE_ENT_PER) AS ENT_EXT,
        //            (SELECT FLOW_NAME FROM M_CASE_STATUS WHERE FLOW_CD = F.FLOW_CD AND UNIT_SCD = F.CASE_ENT_UNIT) AS FLOW_NAME,
        //            (SELECT UNIT_NAME FROM UNIT WHERE UNIT_SCD = F.CASE_ENT_UNIT) AS UNIT_NAME
        //        FROM M_CASE_BATCH_FLOW F, M_CASE_BATCH_DATA D
        //        WHERE F.CASE_NO = D.CASE_NO
        //            AND D.CASE_FRM_NAME = @CASE_FRM_NAME
        //    ");

        //    args.Add("CASE_FRM_NAME", model.Account);

        //    // 案號
        //    if (!String.IsNullOrEmpty(model.BatchNo))
        //    {
        //        querySQL.Append("AND F.CASE_NO = @CASE_NO ");
        //        args.Add("CASE_NO", model.BatchNo);
        //    }

        //    // 文號
        //    if (model.BatchDate != null)
        //    {
        //        querySQL.Append("AND D.SC_DATE = @SC_DATE ");
        //        args.Add("SC_DATE", model.BatchDate);
        //    }

        //    querySQL.Append("ORDER BY CASE_ENT_TIME, DOC_ID");

        //    list = GetList(querySQL.ToString(), args);

        //    return list;
        //}

        ///// <summary>
        ///// 案件申請紀錄
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public HistoryOnlineModel GetOnlineData(HistoryOnlineModel model)
        //{
        //    string sql = @" 
        //        SELECT A.APP_ID, S.SRV_ID, S.NAME AS SERVICE_NAME,
        //            (CASE 
        //                WHEN S.FIX_UNIT_CD < 10 THEN (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = S.FIX_UNIT_CD)
        //                ELSE (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = S.FIX_UNIT_CD))
        //            END) AS PRO_UNIT_NAME,
        //            ISNULL((SELECT NAME FROM ADMIN WHERE ACC_NO = A.PRO_ACC), '尚未分案') AS PRO_ACC_NAME,
        //            A.APP_TIME, S.PRO_DEADLINE, S.RPT_NAME, A.PAY_METHOD,
        //            (CASE WHEN A.PAY_A_FEE = A.PAY_A_PAID THEN 'Y' ELSE 'N' END) PAY_MK, PAY_A_FEE, A.FLOW_CD, S.REUPD_MK
        //        FROM APPLY A, SERVICE S
        //        WHERE A.DEL_MK = 'N'
        //            AND A.SRV_ID = S.SRV_ID
        //            AND A.ACC_NO = @ACC_NO
        //            AND A.APP_ID = @APP_ID
        //    ";

        //    SqlCommand cmd = new SqlCommand(sql, conn);
        //    DataUtils.AddParameters(cmd, "ACC_NO", model.Account);
        //    DataUtils.AddParameters(cmd, "APP_ID", model.ApplyId);

        //    using (SqlDataReader dr = cmd.ExecuteReader())
        //    {
        //        if (dr.Read())
        //        {
        //            int n = 0;
        //            model.ApplyId = DataUtils.GetDBString(dr, n++);
        //            model.ServiceId = DataUtils.GetDBString(dr, n++);
        //            model.ServiceName = DataUtils.GetDBString(dr, n++);
        //            model.ProUnitName = DataUtils.GetDBString(dr, n++);
        //            model.ProAccount = DataUtils.GetDBString(dr, n++);
        //            model.ApplyDate = DataUtils.GetDBDateTime(dr, n++);
        //            model.ProDeadline = DataUtils.GetDBInt(dr, n++);
        //            model.ReportName = DataUtils.GetDBString(dr, n++);
        //            model.PayMethod = DataUtils.GetDBString(dr, n++);
        //            model.PayStatus = DataUtils.GetDBString(dr, n++);
        //            model.PayFee = DataUtils.GetDBInt(dr, n++);
        //            model.FlowStatus = DataUtils.GetDBString(dr, n++);
        //            model.ReuploadMark = DataUtils.GetDBString(dr, n++).Equals("Y");
        //        }
        //    }

        //    return model;
        //}

        ///// <summary>
        ///// 案件繳費紀錄
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public List<ApplyPayModel> GetApplyPayList(string applyId)
        //{
        //    List<ApplyPayModel> list = new List<ApplyPayModel>();

        //    string sql = @" SELECT P.PAY_ID, P.PAY_MONEY, P.PAY_ACT_TIME, P.PAY_DESC,
        //                        (SELECT CODE_DESC FROM CODE_CD WHERE CODE_KIND = 'PAY_METHOD' AND CODE_CD = P.PAY_METHOD) AS PAY_METHOD
        //                    FROM APPLY_PAY P
        //                    WHERE P.DEL_MK = 'N'
        //                        AND P.APP_ID = @APP_ID 
        //                        AND P.PAY_STATUS_MK = 'Y'";

        //    SqlCommand cmd = new SqlCommand(sql, conn);
        //    DataUtils.AddParameters(cmd, "APP_ID", applyId);

        //    using (SqlDataReader dr = cmd.ExecuteReader())
        //    {
        //        while (dr.Read())
        //        {
        //            int n = 0;
        //            ApplyPayModel item = new ApplyPayModel();
        //            item.PayId = DataUtils.GetDBString(dr, n++);
        //            item.PayMoney = DataUtils.GetDBInt(dr, n++);
        //            item.PayDate = DataUtils.GetDBDateTime(dr, n++);
        //            item.PayDesc = DataUtils.GetDBString(dr, n++);
        //            item.PayMethod = DataUtils.GetDBString(dr, n++);

        //            list.Add(item);
        //        }
        //    }

        //    return list;
        //}

        //public List<ApplyLogModel> GetApplyLogList(string applyId)
        //{
        //    List<ApplyLogModel> list = new List<ApplyLogModel>();

        //    string sql = @" SELECT TRNS_TIME, TRNS_TYPE, OLD_DESC, NEW_DESC
        //                    FROM APPLY_TRANS_LOG
        //                    WHERE APP_ID = @APP_ID 
        //                    ORDER BY TRNS_TIME ";

        //    SqlCommand cmd = new SqlCommand(sql, conn);
        //    DataUtils.AddParameters(cmd, "APP_ID", applyId);

        //    using (SqlDataReader dr = cmd.ExecuteReader())
        //    {
        //        while (dr.Read())
        //        {
        //            int n = 0;
        //            ApplyLogModel item = new ApplyLogModel();
        //            item.TransTime = DataUtils.GetDBDateTime(dr, n++);
        //            item.Type = DataUtils.GetDBString(dr, n++);
        //            item.Status1 = DataUtils.GetDBString(dr, n++);
        //            item.Status2 = DataUtils.GetDBString(dr, n++);

        //            list.Add(item);
        //        }
        //    }

        //    return list;
        //}

        public bool AddFile(List<Dictionary<string, object>> list)
        {
            try
            {
                string insertSQL = @"
                INSERT INTO APPLY_FILE (
                    APP_ID, FILE_NO, FILENAME, SRC_FILENAME,
                    UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                ) VALUES (
                    @APP_ID, ISNULL((SELECT MAX(FILE_NO)+1 FROM APPLY_FILE WHERE APP_ID = @APP_ID), 1), @FILENAME, @SRC_FILENAME,
                    GETDATE(), 'WEB-REUPD', @UPD_ACC, GETDATE(), 'WEB-REUPD', @UPD_ACC
                )
            ";

                foreach (Dictionary<string, object> args in list)
                {
                    Update(insertSQL, args);
                }
            }
            catch (Exception ex)
            {
                logger.Debug("history_AddFile failed:" + ex.TONotNullString());
                return false;
            }

            return true;
        }
    }
}