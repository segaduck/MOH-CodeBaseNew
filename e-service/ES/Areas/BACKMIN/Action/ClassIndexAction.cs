using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Utils;
using log4net;
using ES.Areas.Admin.Models;
using System.Data.Common;
using System.Data;
using ES.Controllers;
using System.Configuration;

namespace ES.Areas.Admin.Action
{
    public class ClassIndexAction
    {
        private List<Map> detiallist = null;

        /// <summary>
        /// 回傳細項資料
        /// </summary>
        /// <returns></returns>
        public List<Map> GetClassDetailModel()
        {
            return this.detiallist;
        }

        public List<Map> GetCLS_CD(ClassIndexModel model, String TableName)
        {
            detiallist = new List<Map>();
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"with pd as(
                                    select DISTINCT PARENT_CD 
                                    from {0} 
                                    where del_mk='N' and cls_level='2'
                                    {1}
                                    )
                                    select NAME,{2} 
                                    from {3} 
                                    where del_mk='N' and cls_level='1' and {4} in (select * from pd)
                                    ";
                    dbc.Parameters.Clear();
                    if (!String.IsNullOrEmpty(model.ClassName))
                    {
                        sql = String.Format(sql, TableName, " and name like @name ", TableName, TableName, TableName);
                        DataUtils.AddParameters(dbc, "name", "%" + model.ClassName + "%");
                        //dbc.Parameters.Add("@name", "%" + model.ClassName + "%");
                    }
                    else
                    {
                        sql = String.Format(sql, TableName, "", TableName, TableName, TableName);
                    }
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("NAME", sda["NAME"].ToString());
                            map.Add("CODE", sda[TableName].ToString());
                            detiallist.Add(map);
                        }
                        sda.Close();
                    }

                    sql = @"with pd as(
                            select DISTINCT PARENT_CD from {0} 
                            where del_mk='N' and cls_level='2'
                            {1}
                            )
                            select NAME,{2},PARENT_CD from {3} where del_mk='N' and PARENT_CD in (select * from pd)";
                    dbc.Parameters.Clear();
                    if (!String.IsNullOrEmpty(model.ClassName))
                    {
                        sql = String.Format(sql, TableName, " and name like @name ", TableName, TableName);
                        //dbc.Parameters.Add("@name", "%" + model.ClassName + "%");
                        DataUtils.AddParameters(dbc, "name", "%" + model.ClassName + "%");
                    }
                    else
                    {
                        sql = String.Format(sql, TableName, "", TableName, TableName);
                    }
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("NAME", sda["NAME"].ToString());
                            map.Add("CODE", sda[TableName].ToString());
                            map.Add("PARENT_CD", sda["PARENT_CD"].ToString());
                            li.Add(map);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<Map> GetCATE_SEARCH()
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"select TITLE,SRL_NO from CATE_SEARCH where del_mk='N'";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("TITLE", sda["TITLE"].ToString());
                            map.Add("SRL_NO", sda["SRL_NO"].ToString());
                            li.Add(map);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public ClassEditModel SelectCATE_SEARCH(int Pkey)
        {
            ClassEditModel model = new ClassEditModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"select SRL_NO,TITLE,CLS_SUB_CD,CLS_ADM_CD,CLS_SRV_CD,KEYWORD
                                from CATE_SEARCH where SRL_NO=@SRL_NO";
                    dbc.Parameters.Clear();
                    //dbc.Parameters.Add("@SRL_NO", Pkey);
                    DataUtils.AddParameters(dbc, "SRL_NO", Pkey);
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            model.SRL_NO = DataUtils.GetDBInt(sda, 0);
                            model.TITLE = DataUtils.GetDBString(sda, 1);
                            model.CLS_SUB_CD = DataUtils.GetDBString(sda, 2);
                            model.CLS_ADM_CD = DataUtils.GetDBString(sda, 3);
                            model.CLS_SRV_CD = DataUtils.GetDBString(sda, 4);
                            model.KEYWORD = DataUtils.GetDBString(sda, 5);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return model;
        }

        public Boolean UpdateCATE_SEARCH(ClassEditModel model)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    dbc.CommandText = @"update CATE_SEARCH set 
                                        TITLE = @TITLE,
                                        CLS_SUB_CD = @CLS_SUB_CD,
                                        CLS_ADM_CD = @CLS_ADM_CD,
                                        CLS_SRV_CD = @CLS_SRV_CD,
                                        KEYWORD = @KEYWORD,
                                        UPD_TIME = GETDATE(),
                                        UPD_FUN_CD = 'ADM-SEARCH',
                                        UPD_ACC = @UPD_ACC
                                        where SRL_NO = @SRL_NO ";

                    dbc.Parameters.Clear();
                    DataUtils.AddParameters(dbc, "TITLE", model.TITLE);
                    DataUtils.AddParameters(dbc, "CLS_SUB_CD", model.CLS_SUB_CD);
                    DataUtils.AddParameters(dbc, "CLS_ADM_CD", model.CLS_ADM_CD);
                    DataUtils.AddParameters(dbc, "CLS_SRV_CD", model.CLS_SRV_CD);
                    DataUtils.AddParameters(dbc, "KEYWORD", model.KEYWORD);
                    DataUtils.AddParameters(dbc, "SRL_NO", model.SRL_NO);
                    DataUtils.AddParameters(dbc, "UPD_ACC", model.UPD_ACC);
                    //dbc.Parameters.Add("@TITLE", model.TITLE);
                    //dbc.Parameters.Add("@CLS_SUB_CD", model.CLS_SUB_CD);
                    //dbc.Parameters.Add("@CLS_ADM_CD", model.CLS_ADM_CD);
                    //dbc.Parameters.Add("@CLS_SRV_CD", model.CLS_SRV_CD);
                    //dbc.Parameters.Add("@KEYWORD", model.KEYWORD);
                    //dbc.Parameters.Add("@SRL_NO", model.SRL_NO);
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        private void AddNode(List<ClassNodeModel> li, String TableName, String id)
        {
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"select {0},PARENT_CD,NAME,CLS_LEVEL
                                from {1} where DEL_MK='N'";
                    dbc.Parameters.Clear();
                    dbc.CommandText = String.Format(sql, TableName, TableName);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            ClassNodeModel SUB_node = new ClassNodeModel();
                            SUB_node.id = id + sda[TableName].ToString();
                            SUB_node.pId = String.IsNullOrEmpty(sda["PARENT_CD"].ToString()) ? id : id + sda["PARENT_CD"].ToString();
                            SUB_node.name = "[" + sda[TableName].ToString() + "] " + sda["NAME"].ToString();
                            SUB_node.open = false;
                            SUB_node.clevel = int.Parse(sda["CLS_LEVEL"].ToString());
                            SUB_node.cname = sda["NAME"].ToString();
                            SUB_node.ccd = sda[TableName].ToString();
                            SUB_node.cpcd = sda["PARENT_CD"].ToString();
                            SUB_node.clevel = int.Parse(sda["CLS_LEVEL"].ToString());
                            li.Add(SUB_node);
                        }
                        sda.Close();
                    }
                }
            }
        }

        public List<ClassNodeModel> GetAllClass()
        {
            List<ClassNodeModel> li = new List<ClassNodeModel>();
            #region 主題分類
            ClassNodeModel SUB = new ClassNodeModel();
            SUB.id = "CLS_SUB_CD#";
            SUB.name = "主題分類";
            SUB.open = false;
            SUB.pId = "0";
            SUB.clevel = 0;
            li.Add(SUB);

            AddNode(li, "CLS_SUB_CD", SUB.id);
            #endregion

            #region 行政分類
            ClassNodeModel ADM = new ClassNodeModel();
            ADM.id = "CLS_ADM_CD#";
            ADM.name = "行政分類";
            ADM.open = false;
            ADM.pId = "0";
            ADM.clevel = 0;
            li.Add(ADM);

            AddNode(li, "CLS_ADM_CD", ADM.id);
            #endregion

            #region 服務分類
            ClassNodeModel SRV = new ClassNodeModel();
            SRV.id = "CLS_SRV_CD#";
            SRV.name = "服務分類";
            SRV.open = false;
            SRV.pId = "0";
            SRV.clevel = 0;
            li.Add(SRV);

            AddNode(li, "CLS_SRV_CD", SRV.id);
            #endregion
            return li;
        }

        public Boolean AddDateCLS(String TabelName, ClassPageModel model, String Account)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"insert into {0} 
({1},PARENT_CD,NAME,CLS_LEVEL,DEL_MK,UPD_ACC,UPD_FUN_CD,UPD_TIME,ADD_ACC,ADD_FUN_CD,ADD_TIME)
values
(@{2},@PARENT_CD,@NAME,@CLS_LEVEL,@DEL_MK,@UPD_ACC,@UPD_FUN_CD,@UPD_TIME,@ADD_ACC,@ADD_FUN_CD,@ADD_TIME)
                                        ";

                    dbc.Parameters.Clear();
                    DataUtils.AddParameters(dbc, TabelName, model.Classid);
                    DataUtils.AddParameters(dbc, "PARENT_CD", model.ParentClassid == null ? "" : model.ParentClassid);
                    DataUtils.AddParameters(dbc, "NAME", model.Classname);
                    DataUtils.AddParameters(dbc, "CLS_LEVEL", model.Clevel);
                    DataUtils.AddParameters(dbc, "DEL_MK", "N");
                    DataUtils.AddParameters(dbc, "UPD_ACC", Account);
                    DataUtils.AddParameters(dbc, "UPD_FUN_CD", "ADM-CLASS");
                    DataUtils.AddParameters(dbc, "UPD_TIME", DateTime.Now);
                    DataUtils.AddParameters(dbc, "ADD_ACC", Account);
                    DataUtils.AddParameters(dbc, "ADD_FUN_CD", "ADM-CLASS");
                    DataUtils.AddParameters(dbc, "ADD_TIME", DateTime.Now);

                    /*
                    dbc.Parameters.Add("@" + TabelName, model.Classid);
                    dbc.Parameters.Add("@PARENT_CD", model.ParentClassid == null ? "" : model.ParentClassid);
                    dbc.Parameters.Add("@NAME", model.Classname);
                    dbc.Parameters.Add("@CLS_LEVEL", model.Clevel);
                    dbc.Parameters.Add("@DEL_MK", "N");
                    dbc.Parameters.Add("@UPD_ACC", Account);
                    dbc.Parameters.Add("@UPD_FUN_CD", "ADM");
                    dbc.Parameters.Add("@UPD_TIME", DateTime.Now);
                    dbc.Parameters.Add("@ADD_ACC", Account);
                    dbc.Parameters.Add("@ADD_FUN_CD", "ADM");
                    dbc.Parameters.Add("@ADD_TIME", DateTime.Now);
                    */
                    dbc.CommandText = String.Format(sql, TabelName, TabelName, TabelName);
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public Boolean UpdateDateCLS(String TabelName, ClassPageModel model, String Account)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"update {0} set 
                            {1}=@{2},PARENT_CD=@PARENT_CD,NAME=@NAME,CLS_LEVEL=@CLS_LEVEL,
                            UPD_ACC=@UPD_ACC,UPD_FUN_CD=@UPD_FUN_CD,UPD_TIME=@UPD_TIME 
                            where {3}=@{4}_OLD ";
                    dbc.Parameters.Clear();
                    DataUtils.AddParameters(dbc, TabelName, model.Classid);
                    DataUtils.AddParameters(dbc, "PARENT_CD", model.ParentClassid == null ? "" : model.ParentClassid);
                    DataUtils.AddParameters(dbc, "NAME", model.Classname);
                    DataUtils.AddParameters(dbc, "CLS_LEVEL", model.Clevel);
                    DataUtils.AddParameters(dbc, "UPD_ACC", Account);
                    DataUtils.AddParameters(dbc, "UPD_FUN_CD", "ADM-CLASS");
                    DataUtils.AddParameters(dbc, "UPD_TIME", DateTime.Now);
                    DataUtils.AddParameters(dbc, TabelName + "_OLD", model.BeforeClassid);

                    //dbc.Parameters.Add("@" + TabelName, model.Classid);
                    //dbc.Parameters.Add("@PARENT_CD", model.ParentClassid == null ? "" : model.ParentClassid);
                    //dbc.Parameters.Add("@NAME", model.Classname);
                    //dbc.Parameters.Add("@CLS_LEVEL", model.Clevel);
                    //dbc.Parameters.Add("@UPD_ACC", Account);
                    //dbc.Parameters.Add("@UPD_FUN_CD", "ADM");
                    //dbc.Parameters.Add("@UPD_TIME", DateTime.Now);
                   // dbc.Parameters.Add("@" + TabelName + "_OLD", model.BeforeClassid);
                    dbc.CommandText = String.Format(sql, TabelName, TabelName, TabelName, TabelName, TabelName);
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public Boolean DeleteDateCLS(String TabelName, ClassPageModel model, String Account)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"Delete from {0} where {1} = @{2}";
                    dbc.Parameters.Clear();
                    //dbc.Parameters.Add("@" + TabelName, model.Classid);
                    DataUtils.AddParameters(dbc, TabelName, model.Classid);
                    //dbc.Parameters.Add("@DEL_MK", "Y");
                    //dbc.Parameters.Add("@DEL_ACC", Account);
                    //dbc.Parameters.Add("@DEL_FUN_CD", "ADM");
                    //dbc.Parameters.Add("@DEL_TIME", DateTime.Now);
                    dbc.CommandText = String.Format(sql, TabelName, TabelName, TabelName);
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public Boolean IsClassIdExist(String TabelName, String Classid)
        {
            Boolean isExist = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"select * from {0} where {1}=@{2}";
                    dbc.Parameters.Clear();
                    //dbc.Parameters.Add("@" + TabelName, Classid);
                    DataUtils.AddParameters(dbc, TabelName, Classid);
                    dbc.CommandText = String.Format(sql, TabelName, TabelName, TabelName);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if(sda.Read())
                        {
                            isExist = true;
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return isExist;
        }
    }
}