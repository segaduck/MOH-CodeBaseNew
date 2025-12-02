using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using ES.Utils;
using ES.Areas.Admin.Models;
using System.Text;
using log4net;
using Dapper;
using System.Collections;

namespace ES.Areas.Admin.Action
{
    public class ServiceAction : BaseAction
    {
        /// <summary>
        /// 案件管理
        /// </summary>
        protected ServiceAction()
        {
        }

        /// <summary>
        /// 案件管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 案件管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得分類列表
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetCategoryList(int parentId, int unitCode)
        {
            List<Dictionary<String, Object>> list = new List<Dictionary<string, object>>();
            Dictionary<String, Object> item = null;

            string querySQL = @"
                SELECT SC_ID, NAME,
                    (CASE WHEN (SELECT COUNT(1) FROM SERVICE_CATE WHERE DEL_MK = 'N' AND SC_PID = C.SC_ID) = 0 THEN 'N' ELSE 'Y' END) AS PARENT_MK
                FROM SERVICE_CATE C
                WHERE DEL_MK = 'N' AND SC_PID = @SC_PID
            ";

            if (unitCode != -1)
            {
                querySQL += @"AND UNIT_CD = @UNIT_CD ";
            }

            querySQL += "ORDER BY SEQ_NO";

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "SC_PID", parentId);
            if (unitCode != -1)
            {
                DataUtils.AddParameters(cmd, "UNIT_CD", unitCode);
            }

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    item = new Dictionary<string, object>();
                    item.Add("id", DataUtils.GetDBInt(dr, n++));
                    item.Add("name", DataUtils.GetDBString(dr, n++));
                    item.Add("isParent", DataUtils.GetDBString(dr, n++).Equals("Y"));

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 依分類ID取得服務列表
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetServiceList(int categoryId)
        {
            List<Dictionary<String, Object>> list = null;
            Dictionary<String, Object> args = new Dictionary<string, object>();

            string querySQL = @"
    SELECT SRV_ID, NAME,
    ONLINE_S_MK, ONLINE_N_MK
    FROM SERVICE
    WHERE DEL_MK = 'N'
    AND SC_ID = @SC_ID
    ORDER BY SEQ_NO
            ";

            args.Add("SC_ID", categoryId);

            list = GetList(querySQL, args);

            return list;
        }

        /// <summary>
        /// 依分類ID取得分類資訊
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetCategoryById(int categoryId)
        {
            Dictionary<String, Object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT SC_ID, SC_PID, LEVEL,
                    ISNULL((SELECT NAME + ' 》' FROM SERVICE_CATE WHERE SC_ID = C.SC_PID), '') + NAME AS NAME,
                    (SELECT COUNT(1) FROM SERVICE_CATE WHERE DEL_MK = 'N' AND SC_PID = C.SC_ID) AS CATE_COUNT,
                    (SELECT COUNT(1) FROM SERVICE WHERE DEL_MK = 'N' AND SC_ID = C.SC_ID) AS SRV_COUNT
                FROM SERVICE_CATE C
                WHERE DEL_MK = 'N'
                    AND SC_ID = @SC_ID
            ";

            args.Add("SC_ID", categoryId);

            return GetData(querySQL, args);
        }

        /// <summary>
        /// 取得根分類流水號
        /// </summary>
        /// <returns></returns>
        public ServiceCategoryModel GetRootCategoryName()
        {
            ServiceCategoryModel item = new ServiceCategoryModel();

            string sql = @"SELECT ISNULL((MAX(SEQ_NO)/10)*10,0)+10 FROM SERVICE_CATE WHERE SC_PID = 0";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item.Seq = DataUtils.GetDBInt(dr, 0);
                    item.Level = 1;
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 取得上層分類名稱
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public ServiceCategoryModel GetParentCategoryName(int parentId)
        {

            ServiceCategoryModel item = new ServiceCategoryModel();

            string sql = @"SELECT SC_ID, (CASE
                                WHEN LEVEL = 1 THEN NAME + '／'
                                WHEN LEVEL = 2 THEN (SELECT NAME + '／' FROM SERVICE_CATE WHERE SC_ID = C.SC_PID) + NAME + '／'
                            END) AS PARENT_NAME, UNIT_CD, LEVEL+1 AS LEVEL,
                            ISNULL((SELECT (MAX(SEQ_NO)/10)*10+10 FROM SERVICE_CATE WHERE DEL_MK = 'N' AND SC_PID = C.SC_ID), 10) AS SEQ_NO
                        FROM SERVICE_CATE C WHERE SC_ID = @SC_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SC_ID", parentId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item.ParentId = DataUtils.GetDBInt(dr, 0);
                    item.ParentName = DataUtils.GetDBString(dr, 1);
                    item.UnitCode = DataUtils.GetDBInt(dr, 2);
                    item.Level = DataUtils.GetDBInt(dr, 3);
                    item.Seq = DataUtils.GetDBInt(dr, 4);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 取得分類資料
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public ServiceCategoryModel GetCategory(int categoryId)
        {
            ServiceCategoryModel item = new ServiceCategoryModel();

            string sql = @"SELECT SC_ID, SC_PID, UNIT_CD, NAME, LEVEL, SEQ_NO, (CASE
                                    WHEN LEVEL = 1 THEN (SELECT NAME + '／' FROM SERVICE_CATE WHERE SC_ID = C.SC_PID)
                                    WHEN LEVEL = 2 THEN (SELECT (SELECT NAME + '／'  FROM SERVICE_CATE WHERE SC_ID = PC.SC_PID) + NAME + '／' FROM SERVICE_CATE AS PC WHERE SC_ID = C.SC_PID)
                                END) AS PARENT_NAME
                           FROM SERVICE_CATE C WHERE SC_ID = @SC_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SC_ID", categoryId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item.CategoryId = DataUtils.GetDBInt(dr, 0);
                    item.ParentId = DataUtils.GetDBInt(dr, 1);
                    item.UnitCode = DataUtils.GetDBInt(dr, 2);
                    item.Name = DataUtils.GetDBString(dr, 3);
                    item.Level = DataUtils.GetDBInt(dr, 4);
                    item.Seq = DataUtils.GetDBInt(dr, 5);
                    item.ParentName = DataUtils.GetDBString(dr, 6);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 新增分類
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool InsertCategory(ServiceCategoryModel model)
        {
            string sql = @"
                INSERT INTO SERVICE_CATE (
                        SC_ID, SC_PID, UNIT_CD, NAME, LEVEL, SEQ_NO,
                        UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC 
                    ) VALUES (
                        ISNULL((SELECT MAX(SC_ID)+1 FROM SERVICE_CATE),1),
                        @SC_PID, @UNIT_CD, @NAME, @LEVEL, @SEQ_NO,
                        GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                )";

            if (model.ParentId == 0)
            {
                sql = sql.Replace("(SELECT MAX(SC_ID)+1 FROM SERVICE_CATE)", "(SELECT MAX(SC_ID)+1 FROM SERVICE_CATE WHERE SC_ID <= 100)");
            }

            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "SC_PID", model.ParentId);
            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);
            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "LEVEL", model.Level);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SRV");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 修改分類
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateCategory(ServiceCategoryModel model)
        {
            string sql = @"UPDATE SERVICE_CATE SET 
                            NAME = @NAME,
                            UNIT_CD = @UNIT_CD,
                            SEQ_NO = @SEQ_NO,
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC
                        WHERE SC_ID = @SC_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SRV");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "SC_ID", model.CategoryId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 刪除分類
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool DeleteCategory(ServiceCategoryModel model)
        {
            string sql = @"UPDATE SERVICE_CATE SET 
                            DEL_MK = 'Y',
                            DEL_TIME = GETDATE(),
                            DEL_FUN_CD = @FUN_CD,
                            DEL_ACC = @UPD_ACC,
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC
                        WHERE SC_ID = @SC_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SRV");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "SC_ID", model.CategoryId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 取得上層分類名稱
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public ServiceEditModel GetParentServiceName(int parentId)
        {

            ServiceEditModel item = new ServiceEditModel();

            string sql = @"SELECT SC_ID, (CASE WHEN SC_PID > 0 THEN SC_PID ELSE SC_ID END) AS SC_PID, (CASE
                                WHEN LEVEL = 1 THEN NAME
                                WHEN LEVEL = 2 THEN (SELECT NAME + '／' FROM SERVICE_CATE WHERE SC_ID = C.SC_PID) + NAME
                                WHEN LEVEL = 3 THEN (SELECT (SELECT NAME + '／'  FROM SERVICE_CATE WHERE SC_ID = PC.SC_PID) + NAME + '／' FROM SERVICE_CATE AS PC WHERE SC_ID = C.SC_PID) + NAME
                            END) AS PARENT_NAME, UNIT_CD,
                            (SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = C.UNIT_CD) AS UNIT_SCD,
                            ISNULL((SELECT (MAX(SEQ_NO)/10)*10+10 FROM SERVICE WHERE DEL_MK = 'N' AND SC_ID = C.SC_ID), 10) AS SEQ_NO
                        FROM SERVICE_CATE C WHERE SC_ID = @SC_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SC_ID", parentId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item.CategoryId = DataUtils.GetDBInt(dr, 0);
                    item.CategoryParentId = DataUtils.GetDBInt(dr, 1);
                    item.CategoryName = DataUtils.GetDBString(dr, 2);
                    item.UnitCode = DataUtils.GetDBInt(dr, 3);
                    item.UnitSCode = DataUtils.GetDBString(dr, 4);
                    item.Seq = DataUtils.GetDBInt(dr, 5);
                }
                dr.Close();
            }

            return item;
        }

        public ServiceEditModel GetService(string serviceId)
        {
            ServiceEditModel item = new ServiceEditModel();

            string sql = @"SELECT S.SRV_ID, S.SC_ID, S.SEQ_NO, S.NAME, S.SRV_DESC, S.FIX_UNIT_CD,
	                            S.PAGE_MAKER_ID, S.DESIGN_FILE_MK, S.FILE_MAKER_ID, S.ACCOUNT_NAME, S.ACCOUNT_CD, 
                                S.PAY_POINT, S.PAY_UNIT, S.APP_FEE, S.BASE_NUM, S.FEE_EXTRA,
                                S.CHK_PAY_UNIT, S.CHK_FEE, S.CHK_BASE_NUM, S.CHK_FEE_EXTRA, S.PAY_METHOD,
                                S.PAY_DEADLINE, S.PRO_DEADLINE, S.TRAN_MIS_MK, S.TRAN_ARCHIVE_MK, S.MOHW_ARCHIVE_CD,
                                S.RDEC_CD, S.CA_TYPE, S.APP_TARGET, S.CLS_SUB_CD, S.CLS_ADM_CD,
                                S.CLS_SRV_CD, S.KEYWORD,
                                (SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = C.UNIT_CD) AS UNIT_SCD, C.UNIT_CD, (CASE
									WHEN C.LEVEL = 1 THEN C.NAME
                                    WHEN C.LEVEL = 2 THEN (SELECT NAME + '／' FROM SERVICE_CATE WHERE SC_ID = C.SC_PID) + C.NAME
                                    WHEN C.LEVEL = 3 THEN (SELECT (SELECT NAME + '／'  FROM SERVICE_CATE WHERE SC_ID = PC.SC_PID) + NAME + '／' FROM SERVICE_CATE AS PC WHERE SC_ID = C.SC_PID) + C.NAME
                                END) AS CATEGORY_NAME, (
                                    SELECT SUBSTRING(UNIT_CD, 1, LEN(UNIT_CD)-1)
                                    FROM (
	                                    SELECT (
		                                    SELECT CAST ( UNIT_CD AS VARCHAR ) + ','
		                                    FROM SERVICE_UNIT
		                                    WHERE SRV_ID = @SRV_ID
		                                    FOR XML PATH('')
	                                    ) AS UNIT_CD
                                    ) T
                                ) AS ASSIGN_UNIT_CD,
                                CONVERT(CHAR(10), S.UPD_TIME, 111) + ' ' +   CONVERT(CHAR(8), S.UPD_TIME, 108) AS UPD_TIME, S.UPD_ACC, S.PAY_RULE_FIELD, 
                                S.PAY_ACCOUNT, S.SHARED_MK, S.FORM_ID, S.REUPD_MK,
                                (
                                    SELECT SUBSTRING(ACC_NO, 1, LEN(ACC_NO)-1)
                                    FROM (
	                                    SELECT (
		                                    SELECT CAST ( ACC_NO AS NVARCHAR ) + ','
		                                    FROM SERVICE_CASE
		                                    WHERE SRV_ID = @SRV_ID
		                                    FOR XML PATH('')
	                                    ) AS ACC_NO
                                    ) TS
                                ) AS CASE_MAKER_IDS
							FROM SERVICE S
							join SERVICE_CATE C  on S.SC_ID = C.SC_ID
							left join SERVICE_CASE D on S.SRV_ID = D.SRV_ID 
    WHERE 1=1 AND S.SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.CategoryId = DataUtils.GetDBInt(dr, n++);
                    item.Seq = DataUtils.GetDBInt(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.Desc = DataUtils.GetDBString(dr, n++);
                    item.FixUnitCode = DataUtils.GetDBInt(dr, n++);

                    item.PageMakerId = DataUtils.GetDBString(dr, n++);
                    item.DesignFileMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.FileMakerId = DataUtils.GetDBString(dr, n++);
                    item.AccountName = DataUtils.GetDBString(dr, n++);
                    item.AccountCode = DataUtils.GetDBString(dr, n++);

                    item.PayPoint = DataUtils.GetDBString(dr, n++);
                    item.PayUnit = DataUtils.GetDBString(dr, n++);
                    item.ApplyFee = DataUtils.GetDBInt(dr, n++);
                    item.BaseNum = DataUtils.GetDBInt(dr, n++);
                    item.FeeExtra = DataUtils.GetDBInt(dr, n++);

                    item.ChkPayUnit = DataUtils.GetDBString(dr, n++);
                    item.ChkApplyFee = DataUtils.GetDBInt(dr, n++);
                    item.ChkBaseNum = DataUtils.GetDBInt(dr, n++);
                    item.ChkFeeExtra = DataUtils.GetDBInt(dr, n++);
                    item.PayMethod = DataUtils.GetDBStringArray(dr, n++);

                    item.PayDeadline = DataUtils.GetDBInt(dr, n++).ToString();
                    item.ProDeadline = DataUtils.GetDBInt(dr, n++).ToString();
                    item.TranMisMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.TranArchiveMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.ArchiveCode = DataUtils.GetDBString(dr, n++);

                    item.RdecCode = DataUtils.GetDBString(dr, n++);
                    item.LoginType = DataUtils.GetDBStringArray(dr, n++, ", ");
                    item.ApplyTarget = DataUtils.GetDBStringArray(dr, n++);
                    item.ClassSubCode = DataUtils.GetDBString(dr, n++);
                    item.ClassAdmCode = DataUtils.GetDBString(dr, n++);

                    item.ClassSrvCode = DataUtils.GetDBString(dr, n++);
                    item.KeyWord = DataUtils.GetDBString(dr, n++);

                    item.UnitSCode = DataUtils.GetDBString(dr, n++);
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.CategoryName = DataUtils.GetDBString(dr, n++);
                    item.AssignUnitCode = DataUtils.GetDBString(dr, n++).Split(',');

                    item.UpdateTime = DataUtils.GetDBString(dr, n++);
                    item.UpdateAccount = DataUtils.GetDBString(dr, n++);
                    item.ApplyPayField = DataUtils.GetDBString(dr, n++);
                    item.PayAccount = DataUtils.GetDBInt(dr, n++).ToString();
                    item.SharedMark = DataUtils.GetDBString(dr, n++).Equals("Y");

                    item.FormID = DataUtils.GetDBString(dr, n++);
                    item.ReUpdateMark = DataUtils.GetDBString(dr, n++).Equals("Y");

                    item.CaseMakerIds = DataUtils.GetDBString(dr, n++).Split(',');
                    item.Init();
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 新增案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool InsertService(ServiceEditModel model)
        {
            // 取srvid
            Hashtable hash = new Hashtable();
            hash["ServiceId"] = "";
            var dictionary = new Dictionary<string, object> { { "@SRV_CATE", model.CategoryParentId.ToString("D3") } };
            var parameters = new DynamicParameters(dictionary);

            var srvsql = @"select ISNULL((
                            SELECT REPLICATE('0', 6-LEN(CONVERT(VARCHAR, CONVERT(INTEGER, MAX(SRV_ID))+1))) + CONVERT(VARCHAR, CONVERT(INTEGER, MAX(SRV_ID))+1)
                            FROM SERVICE
                            WHERE SRV_ID LIKE @SRV_CATE + '%'
                        ), REPLICATE('0', 3-LEN(@SRV_CATE)) + @SRV_CATE + '001') as ServiceId";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                model.ServiceId = conn.QuerySingle<string>(srvsql, parameters);
                conn.Close();
                conn.Dispose();
            }

            string sql = @"INSERT INTO SERVICE (
                        SRV_ID, SC_ID, SC_PID, NAME, SRV_DESC, FIX_UNIT_CD, PAGE_MAKER_ID,
                        DESIGN_FILE_MK, FILE_MAKER_ID, ACCOUNT_NAME, ACCOUNT_CD, PAY_POINT,
                        PAY_UNIT, APP_FEE, BASE_NUM, FEE_EXTRA, CHK_PAY_UNIT,
                        CHK_FEE, CHK_BASE_NUM, CHK_FEE_EXTRA, PAY_METHOD, PAY_DEADLINE,
                        PRO_DEADLINE, TRAN_MIS_MK, TRAN_ARCHIVE_MK, MOHW_ARCHIVE_CD, RDEC_CD,
                        CA_TYPE, APP_TARGET, CLS_SUB_CD, CLS_ADM_CD, CLS_SRV_CD,
                        KEYWORD, SEQ_NO, ONLINE_S_MK, ONLINE_N_MK, PAY_RULE_FIELD,
                        PAY_ACCOUNT, SHARED_MK, FORM_ID, REUPD_MK,
                        UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                    ) VALUES (
                        @SRV_ID,
                        @SC_ID, @SC_PID, @NAME, @SRV_DESC, @FIX_UNIT_CD, @PAGE_MAKER_ID,
                        @DESIGN_FILE_MK, @FILE_MAKER_ID, @ACCOUNT_NAME, @ACCOUNT_CD, @PAY_POINT,
                        @PAY_UNIT, @APP_FEE, @BASE_NUM, @FEE_EXTRA, @CHK_PAY_UNIT,
                        @CHK_FEE, @CHK_BASE_NUM, @CHK_FEE_EXTRA, @PAY_METHOD, @PAY_DEADLINE,
                        @PRO_DEADLINE, @TRAN_MIS_MK, @TRAN_ARCHIVE_MK, @MOHW_ARCHIVE_CD, @RDEC_CD,
                        @CA_TYPE, @APP_TARGET, @CLS_SUB_CD, @CLS_ADM_CD, @CLS_SRV_CD,
                        @KEYWORD, @SEQ_NO, 'N', 'N', @PAY_RULE_FIELD,
                        @PAY_ACCOUNT, @SHARED_MK, @FORM_ID, @REUPD_MK,
                        GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                    )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "SRV_CATE", model.CategoryParentId.ToString("D3"));
            DataUtils.AddParameters(cmd, "SC_ID", model.CategoryId);
            DataUtils.AddParameters(cmd, "SC_PID", model.CategoryParentId);
            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "SRV_DESC", model.Desc);
            DataUtils.AddParameters(cmd, "FIX_UNIT_CD", model.FixUnitCode);
            DataUtils.AddParameters(cmd, "PAGE_MAKER_ID", model.PageMakerId);

            DataUtils.AddParameters(cmd, "DESIGN_FILE_MK", model.DesignFileMark);
            DataUtils.AddParameters(cmd, "FILE_MAKER_ID", model.FileMakerId);
            DataUtils.AddParameters(cmd, "ACCOUNT_NAME", model.AccountName);
            DataUtils.AddParameters(cmd, "ACCOUNT_CD", model.AccountCode);
            DataUtils.AddParameters(cmd, "PAY_POINT", model.PayPoint);

            DataUtils.AddParameters(cmd, "PAY_UNIT", model.PayUnit);
            DataUtils.AddParameters(cmd, "APP_FEE", model.ApplyFee);
            DataUtils.AddParameters(cmd, "BASE_NUM", model.BaseNum);
            DataUtils.AddParameters(cmd, "FEE_EXTRA", model.FeeExtra);
            DataUtils.AddParameters(cmd, "CHK_PAY_UNIT", model.ChkPayUnit);

            DataUtils.AddParameters(cmd, "CHK_FEE", model.ChkApplyFee);
            DataUtils.AddParameters(cmd, "CHK_BASE_NUM", model.ChkBaseNum);
            DataUtils.AddParameters(cmd, "CHK_FEE_EXTRA", model.ChkFeeExtra);
            DataUtils.AddParameters(cmd, "PAY_METHOD", DataUtils.StringArrayToString(model.PayMethod));
            DataUtils.AddParameters(cmd, "PAY_DEADLINE", model.PayDeadline);

            DataUtils.AddParameters(cmd, "PRO_DEADLINE", model.ProDeadline);
            DataUtils.AddParameters(cmd, "TRAN_MIS_MK", model.TranMisMark);
            DataUtils.AddParameters(cmd, "TRAN_ARCHIVE_MK", model.TranArchiveMark);
            DataUtils.AddParameters(cmd, "MOHW_ARCHIVE_CD", model.ArchiveCode);
            DataUtils.AddParameters(cmd, "RDEC_CD", model.RdecCode);

            DataUtils.AddParameters(cmd, "CA_TYPE", DataUtils.StringArrayToString(model.LoginType, ", "));
            DataUtils.AddParameters(cmd, "APP_TARGET", DataUtils.StringArrayToString(model.ApplyTarget));
            DataUtils.AddParameters(cmd, "CLS_SUB_CD", model.ClassSubCode);
            DataUtils.AddParameters(cmd, "CLS_ADM_CD", model.ClassAdmCode);
            DataUtils.AddParameters(cmd, "CLS_SRV_CD", model.ClassSrvCode);

            DataUtils.AddParameters(cmd, "KEYWORD", model.KeyWord);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);
            DataUtils.AddParameters(cmd, "PAY_RULE_FIELD", model.ApplyPayField);

            DataUtils.AddParameters(cmd, "PAY_ACCOUNT", model.PayAccount);
            DataUtils.AddParameters(cmd, "SHARED_MK", model.SharedMark);
            DataUtils.AddParameters(cmd, "FORM_ID", model.FormID);
            DataUtils.AddParameters(cmd, "REUPD_MK", model.ReUpdateMark);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SRV");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1 && UpdateServiceUnit(model))
                if (flag == 1 && UpdateServiceCASE(model)) return true;
            return false;
        }

        /// <summary>
        /// 修改案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateService(ServiceEditModel model)
        {
            string sql = @"
                UPDATE SERVICE SET
                    NAME = @NAME,
                    SRV_DESC = @SRV_DESC,
                    FIX_UNIT_CD = @FIX_UNIT_CD,
                    PAGE_MAKER_ID = @PAGE_MAKER_ID,
                    DESIGN_FILE_MK = @DESIGN_FILE_MK,

                    FILE_MAKER_ID = @FILE_MAKER_ID,
                    ACCOUNT_NAME = @ACCOUNT_NAME,
                    ACCOUNT_CD = @ACCOUNT_CD,
                    PAY_POINT = @PAY_POINT,
                    PAY_UNIT = @PAY_UNIT,

                    APP_FEE = @APP_FEE,
                    BASE_NUM = @BASE_NUM,
                    FEE_EXTRA = @FEE_EXTRA,
                    CHK_PAY_UNIT = @CHK_PAY_UNIT,
                    CHK_FEE = @CHK_FEE,

                    CHK_BASE_NUM = @CHK_BASE_NUM,
                    CHK_FEE_EXTRA = @CHK_FEE_EXTRA,
                    PAY_METHOD = @PAY_METHOD,
                    PAY_DEADLINE = @PAY_DEADLINE,
                    PRO_DEADLINE = @PRO_DEADLINE,

                    TRAN_MIS_MK = @TRAN_MIS_MK,
                    TRAN_ARCHIVE_MK = @TRAN_ARCHIVE_MK,
                    MOHW_ARCHIVE_CD = @MOHW_ARCHIVE_CD,
                    RDEC_CD = @RDEC_CD,
                    CA_TYPE = @CA_TYPE,

                    APP_TARGET = @APP_TARGET,
                    CLS_SUB_CD = @CLS_SUB_CD,
                    CLS_ADM_CD = @CLS_ADM_CD,
                    CLS_SRV_CD = @CLS_SRV_CD,
                    KEYWORD = @KEYWORD,

                    SEQ_NO = @SEQ_NO,
                    PAY_RULE_FIELD = @PAY_RULE_FIELD,
                    PAY_ACCOUNT = @PAY_ACCOUNT,
                    SHARED_MK = @SHARED_MK,
                    FORM_ID = @FORM_ID,

                    REUPD_MK = @REUPD_MK,

                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE SRV_ID = @SRV_ID
            ";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "SRV_DESC", model.Desc);
            DataUtils.AddParameters(cmd, "FIX_UNIT_CD", model.FixUnitCode);
            DataUtils.AddParameters(cmd, "PAGE_MAKER_ID", model.PageMakerId);
            DataUtils.AddParameters(cmd, "DESIGN_FILE_MK", model.DesignFileMark);

            DataUtils.AddParameters(cmd, "FILE_MAKER_ID", model.FileMakerId);
            DataUtils.AddParameters(cmd, "ACCOUNT_NAME", model.AccountName);
            DataUtils.AddParameters(cmd, "ACCOUNT_CD", model.AccountCode);
            DataUtils.AddParameters(cmd, "PAY_POINT", model.PayPoint);
            DataUtils.AddParameters(cmd, "PAY_UNIT", model.PayUnit);

            DataUtils.AddParameters(cmd, "APP_FEE", model.ApplyFee);
            DataUtils.AddParameters(cmd, "BASE_NUM", model.BaseNum);
            DataUtils.AddParameters(cmd, "FEE_EXTRA", model.FeeExtra);
            DataUtils.AddParameters(cmd, "CHK_PAY_UNIT", model.ChkPayUnit);
            DataUtils.AddParameters(cmd, "CHK_FEE", model.ChkApplyFee);

            DataUtils.AddParameters(cmd, "CHK_BASE_NUM", model.ChkBaseNum);
            DataUtils.AddParameters(cmd, "CHK_FEE_EXTRA", model.ChkFeeExtra);
            DataUtils.AddParameters(cmd, "PAY_METHOD", DataUtils.StringArrayToString(model.PayMethod));
            DataUtils.AddParameters(cmd, "PAY_DEADLINE", model.PayDeadline);
            DataUtils.AddParameters(cmd, "PRO_DEADLINE", model.ProDeadline);

            DataUtils.AddParameters(cmd, "TRAN_MIS_MK", model.TranMisMark);
            DataUtils.AddParameters(cmd, "TRAN_ARCHIVE_MK", model.TranArchiveMark);
            DataUtils.AddParameters(cmd, "MOHW_ARCHIVE_CD", model.ArchiveCode);
            DataUtils.AddParameters(cmd, "RDEC_CD", model.RdecCode);
            DataUtils.AddParameters(cmd, "CA_TYPE", DataUtils.StringArrayToString(model.LoginType, ", "));

            DataUtils.AddParameters(cmd, "APP_TARGET", DataUtils.StringArrayToString(model.ApplyTarget));
            DataUtils.AddParameters(cmd, "CLS_SUB_CD", model.ClassSubCode);
            DataUtils.AddParameters(cmd, "CLS_ADM_CD", model.ClassAdmCode);
            DataUtils.AddParameters(cmd, "CLS_SRV_CD", model.ClassSrvCode);
            DataUtils.AddParameters(cmd, "KEYWORD", model.KeyWord);

            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);
            DataUtils.AddParameters(cmd, "PAY_RULE_FIELD", model.ApplyPayField);
            DataUtils.AddParameters(cmd, "PAY_ACCOUNT", model.PayAccount);
            DataUtils.AddParameters(cmd, "SHARED_MK", model.SharedMark);
            DataUtils.AddParameters(cmd, "FORM_ID", model.FormID);

            DataUtils.AddParameters(cmd, "REUPD_MK", model.ReUpdateMark);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SRV");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1 && UpdateServiceUnit(model))
                if (flag == 1 && UpdateServiceCASE(model)) return true;

            return false;
        }

        public bool UpdateServiceUnit(ServiceEditModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            //StringBuilder deleteSQL1 = new StringBuilder(@"
            //    UPDATE SERVICE_UNIT SET
            //        DEL_MK = 'Y',
            //        DEL_TIME = GETDATE(),
            //        DEL_FUN_CD = @FUN_CD,
            //        DEL_ACC = @UPD_ACC,
            //        UPD_TIME = GETDATE(),
            //        UPD_FUN_CD = @FUN_CD,
            //        UPD_ACC = @UPD_ACC
            //    WHERE SRV_ID = @SRV_ID
            //");

            //args.Add("FUN_CD", "ADM-SRV");
            //args.Add("UPD_ACC", model.UpdateAccount);
            //args.Add("SRV_ID", model.ServiceId);

            //if (model.AssignUnitCode != null && model.AssignUnitCode.Length > 0)
            //{
            //    deleteSQL1.Append("AND UNIT_CD NOT IN (");

            //    for (int i = 0; i < model.AssignUnitCode.Length; i++)
            //    {
            //        deleteSQL1.Append("@UNIT_CD_" + i);

            //        if (i < model.AssignUnitCode.Length - 1)
            //        {
            //            deleteSQL1.Append(", ");
            //        }

            //        args.Add("UNIT_CD_" + i, model.AssignUnitCode[i]);
            //    }

            //    deleteSQL1.Append(") ");
            //}

            //Update(deleteSQL1.ToString(), args);

            args.Clear();
            args.Add("SRV_ID", model.ServiceId);
            string deleteSQL2 = @"
                DELETE SERVICE_UNIT WHERE SRV_ID = @SRV_ID
            ";

            Update(deleteSQL2, args);

            args.Clear();
            args.Add("FUN_CD", "ADM-SRV");
            args.Add("UPD_ACC", model.UpdateAccount);
            args.Add("SRV_ID", model.ServiceId);
            args.Add("UNIT_CD", "");

            if (model.AssignUnitCode != null && model.AssignUnitCode.Length > 0)
            {
                for (int i = 0; i < model.AssignUnitCode.Length; i++)
                {
                    string insertSQL = @"
                    INSERT INTO SERVICE_UNIT (
                        SRV_ID, UNIT_CD,
                        UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                    )
                    SELECT @SRV_ID, UNIT_CD,
                        GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                    FROM UNIT
                    WHERE UNIT_CD = @UNIT_CD
                      AND NOT EXISTS (
                        SELECT UNIT_CD 
                        FROM SERVICE_UNIT 
                        WHERE SRV_ID = @SRV_ID
                          AND UNIT_CD = @UNIT_CD
                    )
                ";
                    args["UNIT_CD"] = model.AssignUnitCode[i];

                    Update(insertSQL, args);
                }
            }

            return true;
        }

        public bool UpdateServiceCASE(ServiceEditModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            StringBuilder deleteSQL1 = new StringBuilder(@"
                UPDATE SERVICE_CASE SET
                    DEL_MK = 'Y',
                    DEL_TIME = GETDATE(),
                    DEL_FUN_CD = @FUN_CD,
                    DEL_ACC = @UPD_ACC,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE SRV_ID = @SRV_ID
            ");

            args.Add("FUN_CD", "ADM-SRV");
            args.Add("UPD_ACC", model.UpdateAccount);
            args.Add("SRV_ID", model.ServiceId);

            if (model.CaseMakerIds != null && model.CaseMakerIds.Length > 0&& model.AssignUnitCode != null)
            {
                deleteSQL1.Append("AND ACC_NO NOT IN (");

                for (int i = 0; i < model.AssignUnitCode.Length; i++)
                {
                    deleteSQL1.Append("@ACC_NO_" + i);

                    if (i < model.AssignUnitCode.Length - 1)
                    {
                        deleteSQL1.Append(", ");
                    }

                    args.Add("ACC_NO_" + i, model.AssignUnitCode[i]);
                }

                deleteSQL1.Append(") ");
            }

            Update(deleteSQL1.ToString(), args);

            args.Clear();
            args.Add("SRV_ID", model.ServiceId);
            string deleteSQL2 = @"
                DELETE SERVICE_CASE WHERE SRV_ID = @SRV_ID AND DEL_MK = 'Y'
            ";

            Update(deleteSQL2, args);

            args.Clear();
            args.Add("FUN_CD", "ADM-SRV");
            args.Add("UPD_ACC", model.UpdateAccount);
            args.Add("SRV_ID", model.ServiceId);
            args.Add("ACC_NO", "");

            if (model.CaseMakerIds != null && model.CaseMakerIds.Length > 0)
            {
                for (int i = 0; i < model.CaseMakerIds.Length; i++)
                {
                    string insertSQL = @"
                    INSERT INTO SERVICE_CASE (
                        SRV_ID, ACC_NO,
                        UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                    )
                    SELECT @SRV_ID, ACC_NO,
                        GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                    FROM ADMIN
                    WHERE ACC_NO = @ACC_NO
                      AND NOT EXISTS (
                        SELECT ACC_NO 
                        FROM SERVICE_CASE 
                        WHERE SRV_ID = @SRV_ID
                          AND ACC_NO = @ACC_NO
                    )
                ";
                    args["ACC_NO"] = model.CaseMakerIds[i];

                    Update(insertSQL, args);
                }
            }

            return true;
        }

        /// <summary>
        /// 案件批次更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool BatchUpdateService(ServiceBatchModel model)
        {
            StringBuilder updateSQL = new StringBuilder(@"UPDATE SERVICE SET ");

            switch (model.Type)
            {
                case "D": // 案件刪除
                    updateSQL.Append(@"
                        DEL_MK = 'Y',
                        DEL_TIME = GETDATE(),
                        DEL_FUN_CD = @FUN_CD,
                        DEL_ACC = @UPD_ACC,
                    ");
                    break;

                case "U": // 案件上線
                    updateSQL.Append("ONLINE_S_MK = 'Y', ");
                    break;

                case "L": // 案件下線
                    updateSQL.Append("ONLINE_S_MK = 'N', ");
                    break;

                case "O": // 開放申辦
                    updateSQL.Append("ONLINE_N_MK = 'Y', ");
                    break;

                case "C": // 關閉申辦
                    updateSQL.Append("ONLINE_N_MK = 'N', ");
                    break;
            }

            updateSQL.Append(@"
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE DEL_MK = 'N' AND SRV_ID IN (
            ");

            for (int i = 0; i < model.ActionId.Count(); i++)
            {
                updateSQL.Append("@SRV_ID_").Append(i);

                if (i < model.ActionId.Count() - 1)
                {
                    updateSQL.Append(", ");
                }
            }

            updateSQL.Append(")");

            SqlCommand cmd = new SqlCommand(updateSQL.ToString(), conn, tran);
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SRVBTH");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            for (int i = 0; i < model.ActionId.Count(); i++)
            {
                DataUtils.AddParameters(cmd, "SRV_ID_" + i, model.ActionId[i].Split(',')[0]);
            }

            int flag = cmd.ExecuteNonQuery();


            if (flag == model.ActionId.Count())
            {
                if (model.Type.Equals("D"))
                {
                    return BatchDeleteService(model);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 刪除其他案件相關資料表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool BatchDeleteService(ServiceBatchModel model)
        {

            string[] tables = new string[] { "SERVICE_AGREE", "SERVICE_FILE", "SERVICE_HELP", "SERVICE_NOTICE" };

            for (int i = 0; i < tables.Count(); i++)
            {
                StringBuilder updateSQL = new StringBuilder(@"UPDATE ").Append(tables[i]).Append(" SET ");

                updateSQL.Append(@"
                        UPD_TIME = GETDATE(),
                        UPD_FUN_CD = @FUN_CD,
                        UPD_ACC = @UPD_ACC
                    WHERE DEL_MK = 'N' AND SRV_ID IN (
                ");

                for (int j = 0; j < model.ActionId.Count(); j++)
                {
                    updateSQL.Append("@SRV_ID_").Append(j);

                    if (j < model.ActionId.Count() - 1)
                    {
                        updateSQL.Append(", ");
                    }
                }

                updateSQL.Append(")");

                SqlCommand cmd = new SqlCommand(updateSQL.ToString(), conn, tran);

                DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SRVBTH");
                DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

                for (int j = 0; j < model.ActionId.Count(); j++)
                {
                    DataUtils.AddParameters(cmd, "SRV_ID_" + j, model.ActionId[j].Split(',')[0]);
                }

                cmd.ExecuteNonQuery();
            }

            return true;
        }

        /// <summary>
        /// 依案件代碼取得案件名稱
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetServiceName(string serviceId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string sql = @"
                SELECT SRV_ID, SC_ID, NAME
                FROM SERVICE
                WHERE DEL_MK = 'N' AND SRV_ID = @SRV_ID
            ";

            args.Add("SRV_ID", serviceId);

            return GetData(sql, args);
        }
    }
}