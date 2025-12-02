using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Utils;
using System.Data.SqlClient;
using log4net;
using ES.Areas.Admin.Models;
using ES.Action;

namespace ES.Areas.Admin.Action
{
    public class UnitAction : BaseAction
    {
        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public UnitAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public UnitAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得列表
        /// </summary>
        /// <returns></returns>
        public List<UnitModel> GetList()
        {
            List<UnitModel> list = new List<UnitModel>();

            string sql = @"SELECT * FROM (
                            SELECT (CASE
                                    WHEN UNIT_LEVEL = 0 THEN SEQ_NO * 1000000
                                    WHEN UNIT_LEVEL = 1 THEN (SELECT SEQ_NO * 1000000 FROM UNIT WHERE UNIT_CD = U.UNIT_PCD) + SEQ_NO * 1000
                                    WHEN UNIT_LEVEL = 2 THEN (SELECT (SELECT SEQ_NO * 1000000 FROM UNIT WHERE UNIT_CD = PU.UNIT_PCD) + SEQ_NO * 1000 FROM UNIT AS PU WHERE UNIT_CD = U.UNIT_PCD) + SEQ_NO
                                END) AS SEQ, UNIT_CD, UNIT_NAME, UNIT_PCD, UNIT_LEVEL, SEQ_NO,
                                (SELECT COUNT(1) FROM UNIT WHERE UNIT_PCD = U.UNIT_CD AND DEL_MK = 'N') AS CHILD_COUNT
                            FROM UNIT AS U WHERE DEL_MK = 'N'
                        ) T ORDER BY SEQ";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    UnitModel model = new UnitModel();
                    model.UnitCode = DataUtils.GetDBInt(dr, 1);
                    model.Name = DataUtils.GetDBString(dr, 2);
                    model.ParentCode = DataUtils.GetDBInt(dr, 3);
                    model.Level = DataUtils.GetDBInt(dr, 4);
                    model.Seq = DataUtils.GetDBInt(dr, 5);
                    model.HaveChild = DataUtils.GetDBInt(dr, 6) > 0;

                    list.Add(model);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得根路徑單位資料
        /// </summary>
        /// <returns></returns>
        public UnitEditModel GetRootUnit() {
            UnitEditModel model = new UnitEditModel();

            string sql = @"SELECT (MAX(SEQ_NO)/10)*10+10 FROM UNIT WHERE UNIT_PCD = 0";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    model.Seq = DataUtils.GetDBInt(dr, 0);
                }
                else
                {
                    model.Seq = 10;
                }
                dr.Close();
            }

            return model;
        }

        /// <summary>
        /// 取得上層單位資料
        /// </summary>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        public UnitEditModel GetParentUnit(int parentCode)
        {
            UnitEditModel model = new UnitEditModel();

            string sql = @"SELECT (CASE
                                WHEN UNIT_LEVEL = 1 THEN (SELECT UNIT_NAME + '／' FROM UNIT WHERE UNIT_CD = U.UNIT_PCD) + UNIT_NAME + '／'
                                WHEN UNIT_LEVEL = 2 THEN (SELECT (SELECT UNIT_NAME + '／'  FROM UNIT WHERE UNIT_CD = PU.UNIT_PCD) + UNIT_NAME + '／' FROM UNIT AS PU WHERE UNIT_CD = U.UNIT_PCD) + UNIT_NAME + '／'
                            END) AS UNIT_PNAME, UNIT_CD, UNIT_LEVEL + 1,
                            ISNULL((SELECT (MAX(SEQ_NO)/10)*10+10 FROM UNIT WHERE DEL_MK = 'N' AND UNIT_PCD = U.UNIT_CD), 10) AS SEQ_NO
                        FROM UNIT U WHERE UNIT_CD = @UNIT_CD";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", parentCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    model.ParentUnitName = DataUtils.GetDBString(dr, 0);
                    model.ParentCode = DataUtils.GetDBInt(dr, 1);
                    model.Level = DataUtils.GetDBInt(dr, 2);
                    model.Seq = DataUtils.GetDBInt(dr, 3);
                    model.UnitCode = 0;
                }
                dr.Close();
            }

            return model;
        }

        /// <summary>
        /// 取得單位資料
        /// </summary>
        /// <param name="unitCode"></param>
        /// <returns></returns>
        public UnitEditModel GetUnit(int unitCode)
        {
            UnitEditModel model = new UnitEditModel();

            string sql = @"SELECT (CASE
                                WHEN UNIT_LEVEL = 0 THEN ''
                                WHEN UNIT_LEVEL = 1 THEN (SELECT UNIT_NAME + '／' FROM UNIT WHERE UNIT_CD = U.UNIT_PCD)
                                WHEN UNIT_LEVEL = 2 THEN (SELECT (SELECT UNIT_NAME + '／'  FROM UNIT WHERE UNIT_CD = PU.UNIT_PCD) + UNIT_NAME + '／' FROM UNIT AS PU WHERE UNIT_CD = U.UNIT_PCD)
                            END) AS UNIT_PNAME, UNIT_CD, UNIT_NAME, UNIT_ADDR, UNIT_PCD, UNIT_LEVEL, UNIT_SCD, SEQ_NO
                        FROM UNIT U WHERE UNIT_CD = @UNIT_CD";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", unitCode);

            logger.Debug("SQL: " + sql);
            logger.Debug("UNIT_CD: " + unitCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    model.ParentUnitName = DataUtils.GetDBString(dr, 0);
                    model.UnitCode = DataUtils.GetDBInt(dr, 1);
                    model.Name = DataUtils.GetDBString(dr, 2);
                    model.Address = DataUtils.GetDBString(dr, 3);
                    model.ParentCode = DataUtils.GetDBInt(dr, 4);
                    model.Level = DataUtils.GetDBInt(dr, 5);
                    model.UnitSCode = DataUtils.GetDBString(dr, 6);
                    model.Seq = DataUtils.GetDBInt(dr, 7);
                }
                dr.Close();
            }

            return model;
        }

        /// <summary>
        /// 新增單位資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(UnitEditModel model)
        {
            string sql = @"INSERT INTO UNIT (
                            UNIT_CD, UNIT_NAME, UNIT_ADDR, UNIT_PCD, UNIT_LEVEL, UNIT_SCD, SEQ_NO, 
                            UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        ) VALUES (
                            ISNULL((SELECT MAX(UNIT_CD)+1 FROM UNIT),1), @UNIT_NAME, @UNIT_ADDR, @UNIT_PCD, @UNIT_LEVEL, @UNIT_SCD, @SEQ_NO,
                            GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                        )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "UNIT_NAME", model.Name);
            DataUtils.AddParameters(cmd, "UNIT_ADDR", model.Address);
            DataUtils.AddParameters(cmd, "UNIT_PCD", model.ParentCode);
            DataUtils.AddParameters(cmd, "UNIT_LEVEL", model.Level);
            DataUtils.AddParameters(cmd, "UNIT_SCD", model.UnitSCode);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-UNIT");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 修改單位資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(UnitEditModel model)
        {
            string sql = @"UPDATE UNIT SET 
                            UNIT_NAME = @UNIT_NAME,
                            UNIT_ADDR = @UNIT_ADDR,
                            UNIT_SCD = @UNIT_SCD,
                            SEQ_NO = @SEQ_NO,
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC
                        WHERE UNIT_CD = @UNIT_CD";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "UNIT_NAME", model.Name);
            DataUtils.AddParameters(cmd, "UNIT_ADDR", model.Address);
            DataUtils.AddParameters(cmd, "UNIT_SCD", model.UnitSCode);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-UNIT");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 刪除單位資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(UnitModel model)
        {
            string sql = @"UPDATE UNIT SET
                            DEL_MK = 'Y',
                            DEL_TIME = GETDATE(),
                            DEL_FUN_CD = @FUN_CD,
                            DEL_ACC = @UPD_ACC,
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC
                        WHERE UNIT_CD = @UNIT_CD";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-UNIT");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}