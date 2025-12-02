using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Models.Share;
using Omu.ValueInjecter;
using System.Collections;
using ES.Models;
using System.Configuration;
using System.Data.SqlClient;
using ES.Action;
using ES.Utils;
using System.Text;
using System.Data;
using ES.Models.Entities;
using ES.Services;
using Dapper;

namespace ES.DataLayers
{
    public class WebDAO : BaseAction
    {
        #region ServiceLst
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<ServiceLstGridModel> GetServiceLst(ServiceLstViewModel model)
        {
            IList<ServiceLstGridModel> rst = null;

            var dictionary = new Dictionary<string, object> { { "@ACT_TYPE", model.ACT_TYPE }, { "@LST_ID", model.LST_ID } };
            var parameters = new DynamicParameters(dictionary);

            string _sql = @"
    SELECT m.SRV_ID
    ,ISNULL(m.LSTNAME ,m.NAME) SRV_ID_NAME, m.SEQ_NO
    FROM SERVICE m
    JOIN SERVICE_LST lt on lt.SRV_ID=m.SRV_ID
    WHERE 1=1 
    AND m.DEL_MK = 'N'
    AND lt.DEL_MK = 'N'
    AND lt.ACT_TYPE=@ACT_TYPE 
    AND lt.LST_ID=@LST_ID
    ORDER BY SEQ_NO";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                rst = conn.Query<ServiceLstGridModel>(_sql, parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return rst;

        }
        #endregion

        #region History

        public IList<HistoryGridModel> GetCase(string UserNo)
        {
            var result = new List<HistoryGridModel>();
            var dictionary = new Dictionary<string, object> { { "@ACC_NO", UserNo } };
            var parameters = new DynamicParameters(dictionary);

            string _sql = @"
    SELECT S.SRV_ID, A.APP_ID, S.NAME AS SRV_ID_NAM,A.NAME, convert(varchar,A.APP_TIME) as APP_TIME, A.APP_ACT_DATE, (CASE
    WHEN A.PAY_POINT = 'A' THEN '不需繳費'
    WHEN A.PAY_POINT = 'D' THEN '審核後繳費'
    WHEN A.PAY_A_FEE = A.PAY_A_PAID THEN '已繳費'
    WHEN A.PAY_A_FEE = (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) THEN '繳費未確認'
    WHEN A.PAY_A_FEE > (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) AND A.CLOSE_MK = 'N' THEN '補繳'
    WHEN A.PAY_A_FEE > (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) AND A.CLOSE_MK = 'Y' THEN '補繳未確認'
    WHEN A.PAY_BACK_MK = 'Y' THEN '退費' ELSE '不退費' END) AS PAY_STATUS,
    '' AS Upload_PayFile, '' AS Download_PayFile, P.PAY_METHOD,
    /*(SELECT F.FLOW_NAME FROM UNIT U, M_CASE_STATUS F WHERE U.UNIT_SCD = F.UNIT_SCD AND UNIT_CD = A.UNIT_CD AND F.FLOW_CD = A.FLOW_CD) AS FLOW_NAME,*/
    case (SELECT (CASE WHEN (A.SRV_ID='005013' OR A.SRV_ID='005014') THEN F.CODE_MEMO ELSE F.CODE_DESC END) CODE_DESC FROM UNIT U, CODE_CD F WHERE U.UNIT_SCD = F.CODE_PCD AND U.UNIT_CD = A.UNIT_CD AND F.CODE_CD = A.FLOW_CD AND F.CODE_KIND = 'F_CASE_STATUS') 
    when '結案(回函核准)' then '結案(回函)' 
	when '結案(歉難同意)' then '結案(回函)'
    when '收件不收案' then '-' 
    else (SELECT (CASE WHEN (A.SRV_ID='005013' OR A.SRV_ID='005014') THEN F.CODE_MEMO ELSE F.CODE_DESC END) CODE_DESC FROM UNIT U, CODE_CD F WHERE U.UNIT_SCD = F.CODE_PCD AND U.UNIT_CD = A.UNIT_CD AND F.CODE_CD = A.FLOW_CD AND F.CODE_KIND = 'F_CASE_STATUS') end AS FLOW_NAME,
    (CASE WHEN A.SRV_ID IN ('001035','001038') THEN 'F'
    WHEN SUBSTRING(A.SRV_ID, 1, 3) <> '001' THEN 'F'
    WHEN (SELECT COUNT(1) FROM SURVEY_ONLINE WHERE APPLY_ID = A.APP_ID) > 0 THEN 'Y'
    ELSE 'N' END) AS SURVEY_MK,
    (SELECT MAX(PAY_ACT_TIME) FROM APPLY_PAY WHERE DEL_MK = 'N' AND PAY_STATUS_MK = 'Y' AND APP_ID = A.APP_ID) AS PAY_ACT_TIME,
    (SELECT SUM(PAY_MONEY) FROM APPLY_PAY WHERE DEL_MK = 'N' AND PAY_STATUS_MK = 'Y' AND APP_ID = A.APP_ID) AS PAY_MONEY,
    ROW_NUMBER() OVER (ORDER BY A.APP_TIME DESC) AS _ROW_NO
    FROM APPLY A
    LEFT JOIN SERVICE S ON S.SRV_ID = A.SRV_ID
    LEFT JOIN APPLY_PAY P ON P.APP_ID = A.APP_ID
    WHERE A.DEL_MK = 'N'
    AND A.ACC_NO = @ACC_NO";

            var hisGrid = new List<HistoryGridModel>();
            var chkNeedPayFileStr = "001005,001007,001008,001037,005004";
            var NoULChkStr = "不需繳費,已繳費,退費,不退費";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                hisGrid = conn.Query<HistoryGridModel>(_sql, parameters).ToList();
                foreach (var item in hisGrid)
                {
                    if (chkNeedPayFileStr.Contains(item.SRV_ID) && !NoULChkStr.Contains(item.PAY_STATUS) && item.FLOW_NAME != "逾期未補件而予結案")
                    {
                        //APPLY_ apply_ Apply_ 有修改申辦案件上傳繳費紀錄檔案的FILE_NO時，需CHECK FILE_NO 與申請案件代碼的相對應關係
                        Dictionary<string, string> SRV_IDAndFileNoDic = new Dictionary<string, string>
                        {
                            {"001005","1" },
                            {"001007","1" },
                            {"001008","1" },
                            {"001037","5" },
                            {"005004","2" }
                        };
                        string _sqlFile = _sqlFile = string.Format(@"SELECT FILENAME FROM Apply_File where APP_ID='{0}' AND FILE_NO='{1}'", item.APP_ID, SRV_IDAndFileNoDic[item.SRV_ID]);
                        string resultFile = conn.QueryFirstOrDefault<string>(_sqlFile);
                        if (string.IsNullOrEmpty(resultFile))
                        {
                            item.Upload_PayFile = string.Format("{0}-{1}-{2}", item.APP_ID, item.SRV_ID, SRV_IDAndFileNoDic[item.SRV_ID]);
                        }
                    }
                    // 當尚未繳費 並且繳費方式為 超商(或劃撥) 允予列印超商繳費單
                    if (item.PAY_STATUS == "繳費未確認" && (item.PAY_METHOD == "S" /*|| item.PAY_METHOD == "T"*/))
                    {
                        item.Download_PayFile = "Y";
                    }
                    // 當尚未繳費 必且繳費方式為 信用卡 允予信用卡重新繳費
                    if (item.PAY_STATUS == "繳費未確認" && item.PAY_METHOD == "C")
                    {
                        item.RePayCard = "Y";
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return hisGrid;
        }

        #endregion


    }
}
