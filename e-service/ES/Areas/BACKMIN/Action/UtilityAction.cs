using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Utils;
using log4net;
using System.Data.SqlClient;
using System.Text;
using ES.Areas.Admin.Models;
using ES.Action;
using ES.Controllers;

namespace ES.Areas.Admin.Action
{
    public class UtilityAction : BaseAction
    {
        /// <summary>
        /// LOG紀錄
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public UtilityAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// LOG紀錄
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public UtilityAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }
        /// <summary>
        /// 取得案件名稱
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public String getServiceName(String SRV_ID)
        {
            string sql = @"SELECT NAME FROM SERVICE WHERE
                           SRV_ID =@SRV_ID ";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "SRV_ID", SRV_ID);

            return cmd.ExecuteScalar().ToString();
           
        }
        
        /// <summary>
        /// 取得最後一次登入成功與否
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public String getLastLoginStatus(String LoginID)
        {
            string sql = @"SELECT TOP 1 STATUS FROM LOGIN_LOG WHERE
                           LOGIN_ID =@LOGIN_ID ORDER BY LOGIN_TIME DESC ";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "LOGIN_ID", LoginID);

            var info = cmd.ExecuteScalar();

            if (info==null)//沒有登入紀錄
            {
                return "S";
            }
            else
            {
                return cmd.ExecuteScalar().ToString();
            }
        }
        /// <summary>
        /// 取得最後一次失敗次數
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public String getLastLoginFailTotal(String LoginID)
        {
            string sql = @"SELECT TOP 1 FAIL_TOTAL FROM LOGIN_LOG WHERE
                           LOGIN_ID =@LOGIN_ID AND STATUS IN ('A','H') ORDER BY LOGIN_TIME DESC ";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "LOGIN_ID", LoginID);

            var info = cmd.ExecuteScalar();

            if (info == null)//沒有失敗次數
            {
                return "0";
            }
            else
            {
                return cmd.ExecuteScalar().ToString();
            }

           
        }

        /// <summary>
        /// 取得總失敗次數
        /// </summary>
        /// <param name="SRV_ID"></param>
        /// <returns></returns>
        public String getMaxLoginFailCount(String LoginID)
        {
            string sql = @"SELECT  MAX(FAIL_COUNT) FROM LOGIN_LOG WHERE
                           LOGIN_ID =@LOGIN_ID GROUP BY LOGIN_ID ";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "LOGIN_ID", LoginID);

            var info = cmd.ExecuteScalar();

            if (info == null)//沒有失敗次數
            {
                return "0";
            }
            else
            {
                return cmd.ExecuteScalar().ToString();
            }
        }

        /// <summary>
        /// 新增登出登入LOG
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool InsertLoginLog(String LoginID, String Name, String UNIT_CD, String IP_ADDR, String STATUS, int FAIL_TOTAL, int FAIL_COUNT)
        {
            string sql = @"INSERT INTO LOGIN_LOG (
                            LOGIN_ID, LOGIN_TIME, NAME, UNIT_CD, IP_ADDR, STATUS, FAIL_TOTAL,FAIL_COUNT
                        ) VALUES (
                            @LOGIN_ID, GETDATE(), @NAME, @UNIT_CD, @IP_ADDR, @STATUS,@FAIL_TOTAL,@FAIL_COUNT
                        )";       


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "LOGIN_ID", LoginID);
            DataUtils.AddParameters(cmd, "NAME", Name);
            DataUtils.AddParameters(cmd, "UNIT_CD", UNIT_CD);
            DataUtils.AddParameters(cmd, "STATUS", STATUS);
            DataUtils.AddParameters(cmd, "IP_ADDR", IP_ADDR);
            if (FAIL_TOTAL == -1)
            {
                DataUtils.AddParameters(cmd, "FAIL_TOTAL", DBNull.Value);
            }
            else
            {
                DataUtils.AddParameters(cmd, "FAIL_TOTAL", FAIL_TOTAL);
            }
            if (FAIL_COUNT == -1)
            {
                DataUtils.AddParameters(cmd, "FAIL_COUNT", DBNull.Value);
            }
            else
            {
                DataUtils.AddParameters(cmd, "FAIL_COUNT", FAIL_COUNT);
            }
            
            

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 新增異動LOG
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(UtilityModel model,AccountModel upd_Model)
        {
            string sql = @"INSERT INTO TX_LOG (
                            TX_TIME, TX_CATE_CD, TX_LOGIN_ID, TX_LOGIN_NAME, TX_UNIT_CD, TX_TYPE, TX_DESC
                        ) VALUES (
                            GETDATE(), @TX_CATE_CD, @TX_LOGIN_ID, @TX_LOGIN_NAME, @TX_UNIT_CD, @TX_TYPE,@TX_DESC
                        )";     
            

            model.TX_LOGIN_ID = upd_Model.Account;
            model.TX_LOGIN_NAME = upd_Model.Name;
            model.TX_UNIT_CD = int.Parse(upd_Model.UnitCode);
            

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "TX_CATE_CD", model.TX_CATE_CD);
            DataUtils.AddParameters(cmd, "TX_LOGIN_ID", model.TX_LOGIN_ID);
            DataUtils.AddParameters(cmd, "TX_LOGIN_NAME", model.TX_LOGIN_NAME);
            DataUtils.AddParameters(cmd, "TX_UNIT_CD", model.TX_UNIT_CD);
            DataUtils.AddParameters(cmd, "TX_TYPE", model.TX_TYPE);
            DataUtils.AddParameters(cmd, "TX_DESC", model.TX_DESC); 

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
        /// <summary>
        /// 新增 案件管理批次LOG
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool InsertServiceBatch(UtilityModel model, AccountModel upd_Model,string[] ActionId)
        {
            string sql = @"INSERT INTO TX_LOG (
                            TX_TIME, TX_CATE_CD, TX_LOGIN_ID, TX_LOGIN_NAME, TX_UNIT_CD, TX_TYPE, TX_DESC
                        ) SELECT GETDATE(), @TX_CATE_CD, @TX_LOGIN_ID, @TX_LOGIN_NAME, @TX_UNIT_CD, @TX_TYPE,NAME FROM SERVICE WHERE
                           SRV_ID IN (";
           

            for (int i = 0; i < ActionId.Count(); i++)
            {
                sql+="@SRV_ID_"+i.ToString();

                if (i < ActionId.Count() - 1)
                {
                    sql+=", ";
                }
            }

            sql +=")";                

            model.TX_LOGIN_ID = upd_Model.Account;
            model.TX_LOGIN_NAME = upd_Model.Name;
            model.TX_UNIT_CD = int.Parse(upd_Model.UnitCode);


            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            for (int i = 0; i < ActionId.Count(); i++)
            {
                DataUtils.AddParameters(cmd, "SRV_ID_" + i, ActionId[i].Split(',')[0]);
            }
            DataUtils.AddParameters(cmd, "TX_CATE_CD", model.TX_CATE_CD);
            DataUtils.AddParameters(cmd, "TX_LOGIN_ID", model.TX_LOGIN_ID);
            DataUtils.AddParameters(cmd, "TX_LOGIN_NAME", model.TX_LOGIN_NAME);
            DataUtils.AddParameters(cmd, "TX_UNIT_CD", model.TX_UNIT_CD);
            DataUtils.AddParameters(cmd, "TX_TYPE", model.TX_TYPE);            

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
        /// <summary>
        /// 新增 申請須知管理LOG
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool InsertServiceNotice(UtilityModel model, AccountModel upd_Model, String SRV_ID)
        {
            string sql = @"INSERT INTO TX_LOG (
                            TX_TIME, TX_CATE_CD, TX_LOGIN_ID, TX_LOGIN_NAME, TX_UNIT_CD, TX_TYPE, TX_DESC
                        ) SELECT GETDATE(), @TX_CATE_CD, @TX_LOGIN_ID, @TX_LOGIN_NAME, @TX_UNIT_CD, @TX_TYPE,NAME FROM SERVICE WHERE
                           SRV_ID =@SRV_ID ";


            

            model.TX_LOGIN_ID = upd_Model.Account;
            model.TX_LOGIN_NAME = upd_Model.Name;
            model.TX_UNIT_CD = int.Parse(upd_Model.UnitCode);


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "SRV_ID", SRV_ID);
            
            DataUtils.AddParameters(cmd, "TX_CATE_CD", model.TX_CATE_CD);
            DataUtils.AddParameters(cmd, "TX_LOGIN_ID", model.TX_LOGIN_ID);
            DataUtils.AddParameters(cmd, "TX_LOGIN_NAME", model.TX_LOGIN_NAME);
            DataUtils.AddParameters(cmd, "TX_UNIT_CD", model.TX_UNIT_CD);
            DataUtils.AddParameters(cmd, "TX_TYPE", model.TX_TYPE);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }


    }
}