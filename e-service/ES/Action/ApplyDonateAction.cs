using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using log4net;
using ES.Models;
using System.Text;
using ES.Utils;
using ES.Models.ViewModels;
using ES.Models.Entities;

namespace ES.Action
{
    public class ApplyDonateAction : BaseAction
    {
        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ApplyDonateAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ApplyDonateAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<ApplyDonateGridModel> GetApplyDonateList()
        {
            List<ApplyDonateGridModel> list = new List<ApplyDonateGridModel>();
            string _sql = @"SELECT SRV_ID_DONATE, NAME_CH, NAME_ENG, START_DATE, 
END_DATE, PAY_WAY, DESC_CH, DESC_ENG FROM APPLY_DONATE 
WHERE DEL_MK = 'N' AND ISOPEN = 'Y' AND ISDRAFT = 'N' 
AND START_DATE <= GETDATE() AND DATEADD(DAY,1,END_DATE) >= GETDATE()";

            SqlCommand cmd = new SqlCommand(_sql, conn);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    var item = new ApplyDonateGridModel();
                    item.SRV_ID_DONATE = Convert.ToString(dr["SRV_ID_DONATE"]);
                    item.NAME_CH = Convert.ToString(dr["NAME_CH"]);
                    item.NAME_ENG = Convert.ToString(dr["NAME_ENG"]);
                    item.START_DATE = Convert.ToDateTime(dr["START_DATE"]);
                    item.END_DATE = Convert.ToDateTime(dr["END_DATE"]);
                    item.PAY_WAY = Convert.ToString(dr["PAY_WAY"]);
                    item.DESC_CH = Convert.ToString(dr["DESC_CH"]);
                    item.DESC_ENG = Convert.ToString(dr["DESC_ENG"]);
                    list.Add(item);
                }
                dr.Close();
            }
            return list;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public ApplyDonateDetailModel GetApplyDonate(string srv_id_donate)
        {
            ApplyDonateDetailModel item = new ApplyDonateDetailModel();

            List<ApplyDonateGridModel> list = new List<ApplyDonateGridModel>();
            string _sql = @"SELECT SRV_ID_DONATE, NAME_CH, NAME_ENG, START_DATE, 
                            END_DATE, PAY_WAY, DESC_CH, DESC_ENG FROM APPLY_DONATE 
                            WHERE DEL_MK = 'N' AND ISOPEN = 'Y' AND ISDRAFT = 'N' 
                            AND START_DATE <= GETDATE() AND DATEADD(DAY,1,END_DATE) >= GETDATE()
                            AND SRV_ID_DONATE = '"+ srv_id_donate + "'";

            SqlCommand cmd = new SqlCommand(_sql, conn);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item.SRV_ID_DONATE = Convert.ToString(dr["SRV_ID_DONATE"]);
                    item.NAME_CH = Convert.ToString(dr["NAME_CH"]);
                    item.NAME_ENG = Convert.ToString(dr["NAME_ENG"]);
                    item.START_DATE = Convert.ToDateTime(dr["START_DATE"]);
                    item.END_DATE = Convert.ToDateTime(dr["END_DATE"]);
                    item.PAY_WAY = Convert.ToString(dr["PAY_WAY"]);
                    item.DESC_CH = Convert.ToString(dr["DESC_CH"]);
                    item.DESC_ENG = Convert.ToString(dr["DESC_ENG"]);
                }
                dr.Close();
            }
            return item;
        }
        public List<string> GetApplyDonateFile(string srv_id_donate)
        {
            List<string> model = new List<string>();
            Apply_FileModel where = new Apply_FileModel();
            where.APP_ID = $"00000000{srv_id_donate}0001";
            where.DEL_MK = "N";
            var data = this.GetRowList<Apply_FileModel>(where);
            //  select @file1 = SRC_FILENAME + ',' + convert(varchar,APP_ID) + ',' + convert(varchar,FILE_NO) + ',' + isnull(convert(varchar,SRC_NO),'0')
            //  from APPLY_FILE where APP_ID = '" + app_id + @"' and FILE_NO = '1'
            foreach (var item in data)
            {
                var src_no = item.SRC_NO == null ? "0" : Convert.ToString(item.SRC_NO);
                var str = $"{item.SRC_FILENAME},{item.APP_ID},{item.FILE_NO},{src_no}";
                model.Add(str);
            }
            return model;
        }


        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public Apply_007001ViewModel GetDonateAppId(string app_id)
        {
            Apply_007001ViewModel item = new Apply_007001ViewModel();

            string _sql = @"SELECT PAY_A_FEE as AMOUNT,APP_ID FROM APPLY WHERE APP_ID = " + app_id;

            SqlCommand cmd = new SqlCommand(_sql, conn);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item.AMOUNT = Convert.ToInt32(dr["AMOUNT"]);
                    item.APP_ID = Convert.ToString(dr["APP_ID"]);
                }
                dr.Close();
            }
            return item;
        }
    }
}