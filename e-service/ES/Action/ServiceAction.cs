using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Models;
using System.Data.SqlClient;
using ES.Utils;

namespace ES.Action
{
    public class ServiceAction : BaseAction
    {
        /// <summary>
        /// <summary>
        /// 服務項目
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 服務項目
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 列表-表件下載
        /// </summary>
        /// <returns></returns>
        public List<ServiceModel> GetList()
        {

            List<ServiceModel> list = new List<ServiceModel>();

            string sql = @" 
    SELECT S.SRV_ID, S.NAME, S.ONLINE_N_MK, S.ONLINE_S_MK, C.UNIT_CD,
	    (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = C.UNIT_CD) AS UNIT_NAME,
        (SELECT COUNT(1) FROM SERVICE_FILE WHERE DEL_MK = 'N' AND SRV_ID = S.SRV_ID) AS FILE_COUNT,
        (SELECT COUNT(1) FROM SERVICE_HELP WHERE DEL_MK = 'N' AND SRV_ID = S.SRV_ID) AS HELP_COUNT,
        (SELECT COUNT(1) FROM SERVICE_NORM WHERE DEL_MK = 'N' AND SRV_ID = S.SRV_ID) AS NORM_COUNT
    FROM SERVICE S, SERVICE_CATE C
    WHERE S.SC_ID = C.SC_ID
	    AND S.DEL_MK = 'N'
	    AND S.ONLINE_S_MK = 'Y'
    ORDER BY C.UNIT_CD,s.seq_no";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    ServiceModel item = new ServiceModel();
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.OnlineNMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.OnlineSMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.UnitName = DataUtils.GetDBString(dr, n++);
                    item.FileMark = DataUtils.GetDBInt(dr, n++) > 0;
                    item.HelpMark = DataUtils.GetDBInt(dr, n++) > 0;
                    item.NormMark = DataUtils.GetDBInt(dr, n++) > 0;
                    item.LeadMark = (item.ServiceId == "005001" || item.ServiceId == "005002" || item.ServiceId == "005003" || item.ServiceId == "005004" || item.ServiceId == "005005") ? true : false;
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 列表 (首頁用)
        /// </summary>
        /// <returns></returns>
        public List<ServiceModel> GetList(int unitCode)
        {

            List<ServiceModel> list = new List<ServiceModel>();

            string sql = @" SELECT S.SRV_ID, S.NAME, S.ONLINE_N_MK, S.ONLINE_S_MK, C.UNIT_CD,
	                            (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = C.UNIT_CD) AS UNIT_NAME,
                                (SELECT COUNT(1) FROM SERVICE_FILE WHERE DEL_MK = 'N' AND SRV_ID = S.SRV_ID) AS FILE_COUNT
                            FROM SERVICE S, SERVICE_CATE C
                            WHERE S.SC_ID = C.SC_ID
	                            AND S.DEL_MK = 'N'
	                            AND S.ONLINE_S_MK = 'Y'
                                AND S.ONLINE_N_MK = 'Y'
                                AND C.UNIT_CD = @UNIT_CD
                            ORDER BY C.UNIT_CD";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", unitCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    ServiceModel item = new ServiceModel();
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.OnlineNMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.OnlineSMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.UnitName = DataUtils.GetDBString(dr, n++);
                    item.FileMark = DataUtils.GetDBInt(dr, n++) > 0;


                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 申請須知
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceNoticeModel GetNotice(string serviceId)
        {
            ServiceNoticeModel item = new ServiceNoticeModel();

            string sql = @" SELECT S.SRV_ID, S.NAME, S.ONLINE_N_MK,
                                (SELECT COUNT(1) FROM SERVICE_FILE WHERE DEL_MK = 'N' AND SRV_ID = S.SRV_ID) AS FILE_COUNT,
                                (SELECT COUNT(1) FROM SERVICE_HELP WHERE DEL_MK = 'N' AND SRV_ID = S.SRV_ID) AS HELP_COUNT,
                                (SELECT COUNT(1) FROM SERVICE_NORM WHERE DEL_MK = 'N' AND SRV_ID = S.SRV_ID) AS NORM_COUNT,
                                U.UNIT_NAME, U.UNIT_ADDR,
                                S.PAY_UNIT, S.APP_FEE, S.PRO_DEADLINE, S.CA_TYPE,
                                N.TITLE_1, N.CONTENT_1, N.TITLE_2, N.CONTENT_2, N.TITLE_3, N.CONTENT_3,
                                N.TITLE_4, N.CONTENT_4, N.TITLE_5, N.CONTENT_5, N.TITLE_6, N.CONTENT_6
                            FROM SERVICE S
                                LEFT OUTER JOIN SERVICE_NOTICE N ON S.SRV_ID = N.SRV_ID
                                LEFT JOIN SERVICE_CATE C ON C.SC_ID = S.SC_ID
                                LEFT JOIN UNIT U ON U.UNIT_CD = C.UNIT_CD
                            WHERE S.SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;

                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.OnlineNMark = DataUtils.GetDBString(dr, n++).Equals("Y");

                    item.FileMark = DataUtils.GetDBInt(dr, n++) > 0;
                    item.HelpMark = DataUtils.GetDBInt(dr, n++) > 0;
                    item.NormMark = DataUtils.GetDBInt(dr, n++) > 0;
                    item.UnitName = DataUtils.GetDBString(dr, n++);
                    item.UnitAddress = DataUtils.GetDBString(dr, n++);

                    item.PayUnit = DataUtils.GetDBString(dr, n++);
                    item.ApplyFee = DataUtils.GetDBInt(dr, n++);
                    item.ProDeadline = DataUtils.GetDBInt(dr, n++);
                    item.CAType = DataUtils.GetDBString(dr, n++);

                    item.Title1 = DataUtils.GetDBString(dr, n++);
                    item.Content1 = DataUtils.GetDBString(dr, n++);
                    item.Title2 = DataUtils.GetDBString(dr, n++);
                    item.Content2 = DataUtils.GetDBString(dr, n++);
                    item.Title3 = DataUtils.GetDBString(dr, n++);
                    item.Content3 = DataUtils.GetDBString(dr, n++);
                    item.Title4 = DataUtils.GetDBString(dr, n++);
                    item.Content4 = DataUtils.GetDBString(dr, n++);
                    item.Title5 = DataUtils.GetDBString(dr, n++);
                    item.Content5 = DataUtils.GetDBString(dr, n++);
                    item.Title6 = DataUtils.GetDBString(dr, n++);
                    item.Content6 = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 書表下載
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<ServiceFileModel> GetFileList(string serviceId)
        {

            List<ServiceFileModel> list = new List<ServiceFileModel>();

            string sql = @"
                SELECT S.SRV_ID, S.NAME, F.FILE_ID, F.TITLE, F.FILENAME,
                    F.FILE_TYPE_CD, F.FILE_URL
                FROM SERVICE S, SERVICE_FILE F
                WHERE S.SRV_ID = F.SRV_ID
	                AND S.DEL_MK = 'N'
                    AND F.DEL_MK = 'N'
                    AND S.SRV_ID = @SRV_ID
                ORDER BY F.SEQ_NO
            ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    ServiceFileModel item = new ServiceFileModel();
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.FileId = DataUtils.GetDBInt(dr, n++);
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.FileName = DataUtils.GetDBString(dr, n++);

                    item.FileType = DataUtils.GetDBString(dr, n++);
                    item.FileUrl = DataUtils.GetDBString(dr, n++);

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 操作輔助說明
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<ServiceFileModel> GetHelpList(string serviceId)
        {

            List<ServiceFileModel> list = new List<ServiceFileModel>();

            string sql = @" SELECT S.SRV_ID, S.NAME, F.FILE_ID, F.TITLE, F.FILENAME
                            FROM SERVICE S, SERVICE_HELP F
                            WHERE S.SRV_ID = F.SRV_ID
	                            AND S.DEL_MK = 'N'
                                AND F.DEL_MK = 'N'
                                AND S.SRV_ID = @SRV_ID
                            ORDER BY F.SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    ServiceFileModel item = new ServiceFileModel();
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.FileId = DataUtils.GetDBInt(dr, n++);
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.FileName = DataUtils.GetDBString(dr, n++);

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 相關規範
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<ServiceFileModel> GetNormList(string serviceId)
        {

            List<ServiceFileModel> list = new List<ServiceFileModel>();

            string sql = @" SELECT S.SRV_ID, S.NAME, F.FILE_ID, F.TITLE, F.FILENAME
                            FROM SERVICE S, SERVICE_NORM F
                            WHERE S.SRV_ID = F.SRV_ID
	                            AND S.DEL_MK = 'N'
                                AND F.DEL_MK = 'N'
                                AND S.SRV_ID = @SRV_ID
                            ORDER BY F.SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    ServiceFileModel item = new ServiceFileModel();
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.FileId = DataUtils.GetDBInt(dr, n++);
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.FileName = DataUtils.GetDBString(dr, n++);

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        public bool UpdateFileCount(string serviceId, int fileId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string updateSQL = @"
                if exists (select * from service_file_count where count_date = convert(varchar(10),getdate(),120) and srv_id = @SRV_ID and file_id = @FILE_ID)
                begin
                    update service_file_count set counter = counter+1 where count_date = convert(varchar(10),getdate(),120) and srv_id = @SRV_ID and file_id = @FILE_ID
                end
                else
                begin
                    insert into service_file_count (count_date, srv_id, file_id, counter) values (getdate(), @SRV_ID, @FILE_ID, 1)
                end
            ";

            logger.Debug("SRV_ID: " + serviceId + " / FILE_ID: " + fileId);

            args.Add("SRV_ID", serviceId);
            args.Add("FILE_ID", fileId);

            int flag = Update(updateSQL, args);

            if (flag == 1)
            {
                return true;
            }

            return false;
        }
    }
}