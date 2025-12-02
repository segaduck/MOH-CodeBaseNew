using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using log4net;
using System.Text;
using ES.Utils;
using System.Collections;
using ES.Services;
using Dapper;
using System.Data;
using System.Reflection;
using ES.Models;
using System.ComponentModel.DataAnnotations;
using ES.Commons;

namespace ES.Action
{
    public class BaseAction
    {
        protected static readonly ILog logger = LogUtils.GetLogger();
        protected SqlConnection conn = null;
        protected SqlTransaction tran = null;

        protected int pageSize = 20;
        protected int totalCount = 0;

        private bool isEnableProfile = true;

        public void ToggleProfiling(bool isProfile)
        {
            this.isEnableProfile = isProfile;
        }

        public void setPageSize(int pageSize)
        {
            this.pageSize = pageSize;
        }

        public int GetTotalCount()
        {
            return totalCount;
        }

        public int GetPageSize()
        {
            return pageSize;
        }

        protected string GetPageSize(int nowPage)
        {
            if (this.totalCount < this.pageSize * nowPage)
            {
                return (this.totalCount % this.pageSize).ToString();
            }
            else
            {
                return this.pageSize.ToString();
            }
        }

        protected string GetEndCount(int nowPage)
        {
            if (this.totalCount < this.pageSize * nowPage)
            {
                return this.totalCount.ToString();
            }
            else
            {
                return (this.pageSize * nowPage).ToString();
            }
        }

        private string GetCountSQL(string sql)
        {
            StringBuilder sb = new StringBuilder("SELECT COUNT(1) FROM (");
            sb.Append(sql);
            sb.Append(") T ");

            return sb.ToString();
        }

        private string GetPageSQL(string sql, int nowPage)
        {
            StringBuilder sb = new StringBuilder("SELECT * FROM (");
            sb.Append(sql);
            sb.Append(") T ");
            sb.Append("WHERE 1=1 ");
            sb.Append("AND _ROW_NO <= ").Append(this.pageSize * nowPage).Append(" ");
            sb.Append("AND _ROW_NO > ").Append(this.pageSize * (nowPage - 1)).Append(" ");

            return sb.ToString();
        }


        protected string GetSingleValue(string querySQL, Dictionary<string, object> args)
        {
            string Result = "";
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            // 計算筆數
            SqlCommand cmd1 = new SqlCommand(querySQL, conn, tran);

            for (int i = 0; i < args.Keys.ToArray().Count(); i++)
            {
                DataUtils.AddParameters(cmd1, args.Keys.ToArray()[i], args[args.Keys.ToArray()[i]]);
            }

            using (SqlDataReader dr = cmd1.ExecuteReader())
            {
                if (dr.Read())
                {
                    Result = DataUtils.GetDBString(dr, 0);//DataUtils.GetDBInt(dr, 0);
                }
                dr.Close();
            }

            return Result;
        }

        /// <summary>
        /// 取得分頁查詢結果
        /// SQL中須有 ROW_NUMBER() OVER (ORDER BY XXX) AS _ROW_NO
        /// </summary>
        /// <param name="querySQL">查詢語法</param>
        /// <param name="nowPage">目前頁數</param>
        /// <param name="args">SQL參數</param>
        /// <returns></returns>
        protected List<Dictionary<string, object>> GetList(string querySQL, int nowPage, Dictionary<string, object> args)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;

            // 計算筆數
            SqlCommand cmd1 = new SqlCommand(GetCountSQL(querySQL), conn, tran);

            for (int i = 0; i < args.Keys.ToArray().Count(); i++)
            {
                DataUtils.AddParameters(cmd1, args.Keys.ToArray()[i], args[args.Keys.ToArray()[i]]);
            }

            using (SqlDataReader dr = cmd1.ExecuteReader())
            {
                if (dr.Read())
                {
                    this.totalCount = DataUtils.GetDBInt(dr, 0);
                }
                dr.Close();
            }

            // 查詢資料
            SqlCommand cmd2 = new SqlCommand(GetPageSQL(querySQL, nowPage), conn, tran);

            for (int i = 0; i < args.Keys.ToArray().Count(); i++)
            {
                DataUtils.AddParameters(cmd2, args.Keys.ToArray()[i], args[args.Keys.ToArray()[i]]);
            }

            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    item = new Dictionary<string, object>();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        item.Add(dr.GetName(i), DataUtils.GetDBValue(dr, i));
                    }
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="querySQL"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected List<Dictionary<string, object>> GetList(string querySQL, Dictionary<string, object> args)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;

            SqlCommand cmd = new SqlCommand(querySQL, conn, tran);

            if (args != null)
            {
                for (int i = 0; i < args.Keys.ToArray().Count(); i++)
                {
                    DataUtils.AddParameters(cmd, args.Keys.ToArray()[i], args[args.Keys.ToArray()[i]]);
                }
            }

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    item = new Dictionary<string, object>();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        item.Add(dr.GetName(i), DataUtils.GetDBValue(dr, i));
                    }
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得單筆結果
        /// </summary>
        /// <param name="querySQL"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected Dictionary<string, object> GetData(string querySQL, Dictionary<string, object> args)
        {
            Dictionary<string, object> item = null;

            SqlCommand cmd = new SqlCommand(querySQL, conn, tran);

            for (int i = 0; i < args.Keys.ToArray().Count(); i++)
            {
                DataUtils.AddParameters(cmd, args.Keys.ToArray()[i], args[args.Keys.ToArray()[i]]);
            }

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item = new Dictionary<string, object>();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        item.Add(dr.GetName(i), DataUtils.GetDBValue(dr, i));
                    }
                }
                dr.Close();
            }

            return item;
        }



        /// <summary>
        /// 執行異動資料庫語法
        /// </summary>
        /// <param name="updateSQL"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected int Update(string updateSQL, Dictionary<string, object> args)
        {
            SqlCommand cmd = new SqlCommand(updateSQL, conn, tran);

            for (int i = 0; i < args.Keys.ToArray().Count(); i++)
            {
                DataUtils.AddParameters(cmd, args.Keys.ToArray()[i], args[args.Keys.ToArray()[i]]);
            }

            return cmd.ExecuteNonQuery();
        }

        #region Insert

        /// <summary>
        /// Using Table-Name and Parameter-Hashtable to Insert Data,return int that success row count.
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public int Insert(string Table, Hashtable Parameter)
        {
            //logger.Debug("BaseAction_Insert(" + Table + "," + Parameter + ")");
            var _dbCount = 0;
            var _dbCountLog = 0;
            IList<string> _keyString = new List<string>();
            IList<string> _valueString = new List<string>();
            foreach (var para in Parameter.Keys)
            {
                var _key = para.TONotNullString();
                var _paramkey = "@" + _key;
                _keyString.Add(_key);
                _valueString.Add(_paramkey);
            }
            string _sql_key = string.Join(" , ", _keyString);
            string _sql_value = string.Join(" , ", _valueString);
            string _sql = "INSERT INTO " + Table + " ( " + _sql_key + ") VALUES (" + _sql_value + ")";
            //logger.Debug("_sql:\n" + _sql);
            SqlCommand cmd = new SqlCommand(_sql, conn, tran);

            string s_log1 = "";
            int iCnt = 0;
            foreach (var key in _keyString)
            {
                iCnt += 1;
                var val = Parameter[key].TONotNullString().ToSplit("::");
                var valtype = val.FirstOrDefault().ToLower();
                switch (valtype)
                {
                    case "int":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int16":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int32":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int64":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "string":
                        cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "varchar":
                        cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "bool":
                        cmd.Parameters.Add("@" + key, SqlDbType.Bit).Value = val[1];
                        break;
                    case "boolean":
                        cmd.Parameters.Add("@" + key, SqlDbType.Bit).Value = val[1];
                        break;
                    case "datetime":
                        cmd.Parameters.Add("@" + key, SqlDbType.DateTime).Value = val[1];
                        break;
                    case "decimal":
                        cmd.Parameters.Add("@" + key, SqlDbType.Decimal).Value = val[1];
                        break;
                    case "double":
                        cmd.Parameters.Add("@" + key, SqlDbType.Float).Value = val[1];
                        break;
                    case "byte[]":
                        cmd.Parameters.Add("@" + key, SqlDbType.Binary).Value = "";
                        logger.Debug($"DI_binary:{key},{val[1]}");
                        break;
                }
                s_log1 += string.Format("{0}[{1}]={2}.{3}\n", iCnt, key, valtype, val[1]);
                //logger.Debug(string.Format("{0}[{1}]={2}.{3}\n", iCnt, key, valtype, val[1]));
            }

            try
            {
                //BaseAction-amu-test
                _dbCount = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                string s_errMsg = "";
                s_errMsg += string.Format("##_sql:{0}\n", _sql);
                s_errMsg += string.Format("_log1:{0}\n", s_log1);
                logger.Error(s_errMsg);
                logger.Error(ex.Message, ex);
                throw ex;
            }

            if (this.isEnableProfile)
            {
                _dbCountLog = DbLogGerner(Table, Parameter, "I");
            }

            return _dbCount;
        }

        /// <summary>
        /// Using Class to Insert Data,return int that success row count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Table"></param>
        /// <returns></returns>
        public int Insert<T>(T Table)
        {
            //logger.Debug("BaseAction_Insert(" + Table + ")");
            //string TableName = Table.GetType().Name;
            string TableName = GetTableName(Table);
            //if (Table.GetType().BaseType != null)
            //{
            //    if (Table.GetType().BaseType.Name != "Object")
            //    { TableName = Table.GetType().BaseType.Name; }
            //}
            // 因應欄位名稱與資料表名稱相同而須做處理
            //if (TableName.StartsWith("Tbl"))
            //{
            //    int startIndex = 3;
            //    int endIndex = TableName.Length - 3;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}
            //else if (TableName.EndsWith("Model"))
            //{
            //    int startIndex = 0;
            //    int endIndex = TableName.Length - 5;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}

            Hashtable Parameter = new Hashtable();
            Parameter = Service.ClassToHashtable(Table);

            return Insert(TableName, Parameter);
        }

        /// <summary>
        /// Using Table-Name and Parameter-Hashtable to Insert Data,return int that success row count.
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public int InsertLog(string Table, Hashtable Parameter)
        {
            //logger.Debug("BaseAction_InsertLog(" + Table + "," + Parameter + ")");
            IList<string> _keyString = new List<string>();
            IList<string> _valueString = new List<string>();
            foreach (var para in Parameter.Keys)
            {
                var _key = para.TONotNullString();
                var _paramkey = "@" + _key;
                _keyString.Add(_key);
                _valueString.Add(_paramkey);
            }
            string _sql_key = string.Join(" , ", _keyString);
            string _sql_value = string.Join(" , ", _valueString);

            string _sql = "INSERT INTO " + Table + " ( " + _sql_key + ") VALUES (" + _sql_value + ")";

            SqlCommand cmd = new SqlCommand(_sql, conn, tran);

            foreach (var key in _keyString)
            {
                var val = Parameter[key].TONotNullString().ToSplit("::");
                switch (val.FirstOrDefault().ToLower())
                {
                    case "int":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int16":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int32":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int64":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "string":
                        cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "varchar":
                        cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "nvarchar":
                        cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "bool":
                        cmd.Parameters.Add("@" + key, SqlDbType.Bit).Value = val[1];
                        break;
                    case "boolean":
                        cmd.Parameters.Add("@" + key, SqlDbType.Bit).Value = val[1];
                        break;
                    case "datetime":
                        cmd.Parameters.Add("@" + key, SqlDbType.DateTime).Value = val[1];
                        break;
                    case "decimal":
                        cmd.Parameters.Add("@" + key, SqlDbType.Decimal).Value = val[1];
                        break;
                    case "double":
                        cmd.Parameters.Add("@" + key, SqlDbType.Float).Value = val[1];
                        break;
                    case "byte[]":
                        cmd.Parameters.Add("@" + key, SqlDbType.Image).Value = val[1];
                        break;
                }

            }
            //logger.Debug("cmd.ExecuteNonQuery:" + _sql);

            return cmd.ExecuteNonQuery();
        }

        #endregion

        #region Update

        /// <summary>
        /// Using Table-Name and Parameter-Hashtable to Update Data,return int that effect row count.
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Parameter"></param>
        /// <param name="Where"></param>
        /// <returns></returns>
        public int Update(string Table, Hashtable Parameter, Hashtable Where)
        {
            //logger.Debug("BaseAction_Update(" + Table + "," + Parameter + "," + Where + ")");
            if (Parameter.Count == 0) { return -1; }

            var _dbCount = 0;
            var _dbCountLog = 0;
            // Update Block
            IList<string> _keyString = new List<string>();
            IList<string> _updateString = new List<string>();
            foreach (var para in Parameter.Keys)
            {
                var _key = para.TONotNullString();
                var _paramkey = "@" + _key;
                _keyString.Add(_key);
                _updateString.Add(_key + "=" + _paramkey);
            }
            string _sql_update = string.Join(" , ", _updateString);

            // Where Block
            IList<string> _wherekeyString = new List<string>();
            // IList<string> _whereString = new List<string>();
            string _sql_where = "";
            foreach (var where in Where.Keys)
            {
                var _key = where.TONotNullString();
                var _paramkey = "@where" + _key;
                _wherekeyString.Add(_key);
                var s_item = string.Format("{0}={1}", _key, _paramkey);
                // _whereString.Add(_key + "=" + _paramkey);
                if (_sql_where.TONotNullString() != "") { _sql_where += " and "; }
                _sql_where += s_item;
            }

            // Bindding String
            string _sql = "UPDATE " + Table + " SET " + _sql_update + " WHERE " + _sql_where;
            //logger.Debug(_sql);

            // Execute Command
            SqlCommand cmd = new SqlCommand(_sql, conn, tran);
            string s_log1 = "";
            int iCnt = 0;
            foreach (var key in _keyString)
            {
                iCnt += 1;
                var val = Parameter[key].TONotNullString().ToSplit("::");
                var valtype = val.FirstOrDefault().ToLower();
                switch (valtype)
                {
                    case "int":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int16":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int32":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int64":
                        cmd.Parameters.Add("@" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "string":
                        cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "varchar":
                        cmd.Parameters.Add("@" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "bool":
                        cmd.Parameters.Add("@" + key, SqlDbType.Bit).Value = val[1];
                        break;
                    case "datetime":
                        cmd.Parameters.Add("@" + key, SqlDbType.DateTime).Value = val[1];
                        break;
                    case "decimal":
                        cmd.Parameters.Add("@" + key, SqlDbType.Decimal).Value = val[1];
                        break;
                    case "double":
                        cmd.Parameters.Add("@" + key, SqlDbType.Float).Value = val[1];
                        break;
                    case "byte[]":
                        cmd.Parameters.Add("@" + key, SqlDbType.Image).Value = val[1];
                        break;
                }
                s_log1 += string.Format("{0}[{1}]={2}.{3}\n", iCnt, key, valtype, val[1]);
            }

            string s_log2 = "";
            iCnt = 0;
            foreach (var key in _wherekeyString)
            {
                iCnt += 1;
                var val = Where[key].TONotNullString().ToSplit("::");
                var valtype = val.FirstOrDefault().ToLower();
                switch (valtype)
                {
                    case "int":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int16":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int32":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int64":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "string":
                        cmd.Parameters.Add("@where" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "varchar":
                        cmd.Parameters.Add("@where" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "bool":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Bit).Value = val[1];
                        break;
                    case "datetime":
                        cmd.Parameters.Add("@where" + key, SqlDbType.DateTime).Value = val[1];
                        break;
                    case "decimal":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Decimal).Value = val[1];
                        break;
                    case "double":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Float).Value = val[1];
                        break;
                    case "byte[]":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Image).Value = val[1];
                        break;
                }
                s_log2 += string.Format("{0}[{1}]={2}.{3}\n", iCnt, key, valtype, val[1]);
            }
            try
            {
                //BaseAction-amu-test
                _dbCount = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                string s_errMsg = "";
                s_errMsg += string.Format("##_sql:{0}\n", _sql);
                s_errMsg += string.Format("_log1:{0}\n", s_log1);
                s_errMsg += string.Format("_log2:{0}\n", s_log2);
                logger.Error(s_errMsg);
                logger.Error(ex.Message, ex);
                throw ex;
            }

            _dbCountLog = DbLogGerner(Table, Parameter, "U");
            return _dbCount;
        }

        /// <summary>
        /// 依物件命，取得TABLE名稱(可能)(TABLE命名時，要避免下列字串 Tbl/ViewModel/View/Model)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Table"></param>
        /// <returns></returns>
        public string GetTableName<T>(T Table)
        {
            //logger.Debug("BaseAction_Update(" + Table + ")");
            string TableName = Table.GetType().Name;
            if (Table.GetType().BaseType != null)
            {
                if (Table.GetType().BaseType.Name != "Object")
                { TableName = Table.GetType().BaseType.Name; }
            }
            //因應欄位名稱與資料表名稱相同而須做處理
            //if (TableName.StartsWith("Tbl"))
            //{
            //    int startIndex = 3;
            //    int endIndex = TableName.Length - 3;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}
            //else if (TableName.EndsWith("Model"))
            //{
            //    int startIndex = 0;
            //    int endIndex = TableName.Length - 5;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}

            if (TableName.StartsWith("Tbl"))
            {
                int startIndex = 3;
                int endIndex = TableName.Length - 3;
                TableName = TableName.Substring(startIndex, endIndex);
            }
            else if (TableName.EndsWith("ViewModel"))
            {
                int startIndex = 0;
                int endIndex = TableName.Length - 9;
                TableName = TableName.Substring(startIndex, endIndex);
            }
            else if (TableName.EndsWith("Model"))
            {
                int startIndex = 0;
                int endIndex = TableName.Length - 5;
                TableName = TableName.Substring(startIndex, endIndex);
            }
            else if (TableName.EndsWith("View"))
            {
                int startIndex = 0;
                int endIndex = TableName.Length - 4;
                TableName = TableName.Substring(startIndex, endIndex);
            }

            return TableName;
        }

        /// <summary>
        /// 取得資料內容 (string)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string GetValue1(IList<string> val)
        {
            //logger.Debug("BaseAction_GetValue1(" + val + ")");
            string rst = "";
            if (val == null) { return rst; }
            if (val.ToCount() < 2) { return rst; }
            switch (val.FirstOrDefault().ToLower())
            {
                case "int":
                case "int16":
                case "int32":
                case "int64":
                case "string":
                case "varchar":
                case "bool":
                case "boolean":
                case "decimal":
                case "double":
                case "byte[]":
                    rst = val[1];
                    break;
                case "datetime":
                    //rst = Convert.ToDateTime(val[1]).ToString("yyyy-MM-dd HH:mm:ss");
                    //.ToString("yyyy-MM-dd HH:mm:ss") //日期欄位
                    rst = HelperUtil.DateTimeToTwString(HelperUtil.ConvertDateTime(val[1]));
                    break;
                default:
                    string s_log1 = "";
                    s_log1 = string.Format("## public string GetValue1(IList<string> val) val.FirstOrDefault().ToLower():{0}", val.FirstOrDefault().ToLower());
                    logger.Warn(s_log1);
                    rst = val[1];
                    break;
            }
            return rst;
        }

        /// <summary>
        /// 儲存異動前後-前台登入使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Table"></param>
        /// <param name="Where"></param>
        /// <param name="dict2"></param>
        public void Update2<T>(T Table, T Where, Dictionary<string, object> dict2)
        {
            //logger.Debug("BaseAction_Update2(" + Table + "," + Where + "," + dict2 + ")");
            Update2(Table, Where, dict2, false);
        }

        /// <summary>
        /// 儲存異動前後-後台登入使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Table"></param>
        /// <param name="Where"></param>
        public void Update2<T>(T Table, T Where, Dictionary<string, object> dict2, bool isBackmin)
        {
            //logger.Debug("BaseAction_Update2(" + Table + "," + Where + "," + dict2 + "," + isBackmin + ")");
            //isBackmin=false; true:為 前台登入使用 false: 前台登入使用
            //DataLayers\BackApplyDAO.cs SYS_TRANS_LOG
            //BaseAction.cs SYS_TRANS_LOG
            //dict2["APP_ID"] = dict2["APP_ID"] ?? "";
            //dict2["SRV_ID"] = dict2["SRV_ID"] ?? "";
            //dict2["LastMODTIME"] = dict2["LastMODTIME"] ?? "";
            if (dict2["APP_ID"] == null) { return; }
            if (string.IsNullOrEmpty(dict2["APP_ID"].TONotNullString())) { return; }
            if (dict2["SRV_ID"] == null) { return; }
            if (string.IsNullOrEmpty(dict2["SRV_ID"].TONotNullString())) { return; }
            if (dict2["LastMODTIME"] == null) { return; }
            if (string.IsNullOrEmpty(dict2["LastMODTIME"].TONotNullString())) { return; }
            string APP_ID = dict2["APP_ID"].TONotNullString();
            string SRV_ID = dict2["SRV_ID"].TONotNullString();
            string LastMODTIME = dict2["LastMODTIME"].TONotNullString();
            //logger.Debug("LastMODTIME:" + LastMODTIME);

            string cst_split1 = "<|s|>";
            string cst_right1 = "<|=|>";
            string memUserNo = "NoLogin";
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return; }

            // 前台 後台
            if (isBackmin) { if (sm.UserInfo.Admin == null) { return; } }
            else { if (sm.UserInfo.Member == null) { return; } }

            //memUserNo = mem.ACC_NO;
            memUserNo = sm.UserInfo.UserNo.TONotNullString();

            string TableName = GetTableName(Table);
            logger.Debug("TableName:" + TableName);

            T TableB = GetRow(Where); //依查詢條件 
            Hashtable ParameterB = Service.ClassToHashtable(TableB); //未修改前資料值
            Hashtable Parameter = Service.ClassToHashtable2(Table); //修改值
            string _BEFOREVAL = ""; //未修改前資料值
            string _AFTERVAL = ""; //修改值
            string _COLUMNAME = "";//欄位值
            string _COLUMNAMEC = "";//欄位值-中文
            string _NoInput1 = "UPD_TIME,UPD_FUN_CD,UPD_ACC,ADD_TIME, ADD_FUN_CD,ADD_ACC,APP_ID,MODTIME"; //排除字串
            string[] NoInput1 = _NoInput1.Split(',');
            foreach (var para in Parameter.Keys)
            {
                var _key = para.TONotNullString();
                var val = GetValue1(Parameter[_key].TONotNullString().ToSplit("::"));
                var _dpName = Parameter[_key].TONotNullString().ToSplit("::")[2];
                string keyValue = string.Format("{0}{2}{1}", _key, val, cst_right1);
                logger.Debug("keyValue:" + keyValue);
                //(略過) 1:排除 2:沒有命名同字串 3.空字串
                if (NoInput1.Contains(_key) || _key.Equals(_dpName) || _dpName.Length < 1) { continue; }

                if (_AFTERVAL.TONotNullString() != "") { _AFTERVAL += cst_split1; }
                _AFTERVAL += keyValue; //變動後
                if (_COLUMNAME.TONotNullString() != "") { _COLUMNAME += cst_split1; }
                _COLUMNAME += _key;
                if (_COLUMNAMEC.TONotNullString() != "") { _COLUMNAMEC += cst_split1; }
                _COLUMNAMEC += _dpName; //中文欄名

                var _keyB = para.TONotNullString();
                var valB = GetValue1(ParameterB[_key].TONotNullString().ToSplit("::"));
                string keyValueB = string.Format("{0}{2}{1}", _keyB, valB, cst_right1);
                logger.Debug("keyValueB:" + keyValueB);
                if (_BEFOREVAL.TONotNullString() != "") { _BEFOREVAL += cst_split1; }
                _BEFOREVAL += keyValueB; //變動前
            }

            Hashtable WhereParameter = Service.ClassToHashtable(Where); //查詢條件值
            string _sql_where = "";
            foreach (var para in WhereParameter.Keys)
            {
                var _key = para.TONotNullString();
                var val = GetValue1(WhereParameter[_key].TONotNullString().ToSplit("::"));
                string keyValue = string.Format("{0}={1}", _key, val); //條件值
                if (_sql_where.TONotNullString() != "") { _sql_where += " and "; }
                _sql_where += keyValue;
            }

            if (string.IsNullOrEmpty(sm.LastMODTIME))
            {
                sm.LastMODTIME = LastMODTIME;
            }
            Update(Table, Where);

            if (_BEFOREVAL.Length < 1 || _AFTERVAL.Length < 1) { return; }
            //string _sql_key = string.Join(" , ", _keyString);
            //string _sql_value = string.Join(" , ", _valueString);
            string _sql = @"
 INSERT INTO SYS_TRANS_LOG ( TRANSTIME
 ,APP_ID,SRV_ID,TRANSTYPE,TARGETTABLE    
 ,CONDITIONS,BEFOREVALUES,AFTERVALUES,COLUMNAME,COLUMNAMEC
 ,UPD_TIME,UPD_FUN_CD,UPD_ACC
 ,ADD_TIME,ADD_FUN_CD,ADD_ACC
 ,MODTIME,MODUSER,MODTYPE)
 VALUES ( format(getdate(),'yyyy-MM-dd HH:mm:ss.fff')
 ,@APP_ID,@SRV_ID,'Update',@TARGETTABLE   
 ,@CONDITIONS,@BEFOREVALUES,@AFTERVALUES,@COLUMNAME,@COLUMNAMEC
 ,GETDATE(),'WEB-APPLY',@UPD_ACC
 ,GETDATE(),'WEB-APPLY',@ADD_ACC
 ,@MODTIME,@MODUSER,@MODTYPE)";

            SqlCommand iCmd = new SqlCommand(_sql, conn, tran);
            iCmd.Parameters.Clear();
            iCmd.Parameters.Add("APP_ID", SqlDbType.VarChar).Value = APP_ID;
            iCmd.Parameters.Add("SRV_ID", SqlDbType.VarChar).Value = SRV_ID;
            iCmd.Parameters.Add("TARGETTABLE", SqlDbType.VarChar).Value = TableName;

            iCmd.Parameters.Add("CONDITIONS", SqlDbType.VarChar).Value = _sql_where;
            iCmd.Parameters.Add("BEFOREVALUES", SqlDbType.VarChar).Value = _BEFOREVAL;
            iCmd.Parameters.Add("AFTERVALUES", SqlDbType.VarChar).Value = _AFTERVAL;

            iCmd.Parameters.Add("COLUMNAME", SqlDbType.VarChar).Value = _COLUMNAME;
            iCmd.Parameters.Add("COLUMNAMEC", SqlDbType.VarChar).Value = _COLUMNAMEC;

            iCmd.Parameters.Add("MODTIME", SqlDbType.VarChar).Value = LastMODTIME;
            iCmd.Parameters.Add("MODUSER", SqlDbType.VarChar).Value = memUserNo;// sm.UserInfo.UserNo.TONotNullString();
            iCmd.Parameters.Add("MODTYPE", SqlDbType.VarChar).Value = "U"; //只能記修改值

            iCmd.Parameters.Add("UPD_ACC", SqlDbType.VarChar).Value = memUserNo;
            iCmd.Parameters.Add("ADD_ACC", SqlDbType.VarChar).Value = memUserNo;
            iCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Using Class to Update Data,return int that effect row count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Table">更新</param>
        /// <param name="Where">條件</param>
        /// <returns></returns>
        public int Update<T>(T Table, T Where)
        {
            //logger.Debug("BaseAction_Update(" + Table + "," + Where + ")");

            string TableName = GetTableName(Table); //Table.GetType().Name;
            //if (Table.GetType().BaseType != null)
            //{
            //    if (Table.GetType().BaseType.Name != "Object")
            //    { TableName = Table.GetType().BaseType.Name; }
            //}
            //// 因應欄位名稱與資料表名稱相同而須做處理
            //if (TableName.StartsWith("Tbl"))
            //{
            //    int startIndex = 3;
            //    int endIndex = TableName.Length - 3;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}
            //else if (TableName.EndsWith("Model"))
            //{
            //    int startIndex = 0;
            //    int endIndex = TableName.Length - 5;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}

            //string WhereName = Where.GetType().Name;
            //if (Where.GetType().BaseType != null)
            //{
            //    if (Where.GetType().BaseType.Name == "Object")
            //    { WhereName = Where.GetType().BaseType.Name; }
            //}
            //// 因應欄位名稱與資料表名稱相同而須做處理
            //if (WhereName.StartsWith("Tbl"))
            //{
            //    int startIndex = 3;
            //    int endIndex = WhereName.Length - 3;
            //    WhereName = WhereName.Substring(startIndex, endIndex);
            //}
            //else if (WhereName.EndsWith("Model"))
            //{
            //    int startIndex = 0;
            //    int endIndex = WhereName.Length - 5;
            //    WhereName = WhereName.Substring(startIndex, endIndex);
            //}

            Hashtable Parameter = new Hashtable();
            Parameter = Service.ClassToHashtable(Table);

            Hashtable WhereParameter = new Hashtable();
            WhereParameter = Service.ClassToHashtable(Where);

            return Update(TableName, Parameter, WhereParameter);
        }



        #endregion Update

        #region Delete

        /// <summary>
        /// Using Table-Name and Parameter-Hashtable to Update Data,return int that effect row count.
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Parameter"></param>
        /// <param name="Where"></param>
        /// <returns></returns>
        public int Delete(string Table, Hashtable Where)
        {
            //logger.Debug("BaseAction_Delete(" + Table + "," + Where + ")");
            var _dbCount = 0;
            var _dbCountLog = 0;

            // Where Block
            IList<string> _wherekeyString = new List<string>();
            IList<string> _whereString = new List<string>();
            string _sql_where = "";
            foreach (var where in Where.Keys)
            {
                var _key = where.TONotNullString();
                var _paramkey = "@where" + _key;
                _wherekeyString.Add(_key);
                _whereString.Add(_key + "=" + _paramkey);
            }
            foreach (var item in _whereString)
            {

                if (_sql_where.TONotNullString() == "")
                {
                    _sql_where = item;
                }
                else
                {
                    _sql_where += " and " + item;
                }

            }

            // Bindding String
            string _sql = "DELETE " + Table + " WHERE " + _sql_where;

            // Execute Command
            SqlCommand cmd = new SqlCommand(_sql, conn, tran);
            foreach (var key in _wherekeyString)
            {
                var val = Where[key].TONotNullString().ToSplit("::");
                switch (val.FirstOrDefault().ToLower())
                {
                    case "int":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int16":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int32":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "int64":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Int).Value = val[1];
                        break;
                    case "string":
                        cmd.Parameters.Add("@where" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "varchar":
                        cmd.Parameters.Add("@where" + key, SqlDbType.NVarChar).Value = val[1];
                        break;
                    case "bool":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Bit).Value = val[1];
                        break;
                    case "datetime":
                        cmd.Parameters.Add("@where" + key, SqlDbType.DateTime).Value = val[1];
                        break;
                    case "decimal":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Decimal).Value = val[1];
                        break;
                    case "double":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Float).Value = val[1];
                        break;
                    case "byte[]":
                        cmd.Parameters.Add("@where" + key, SqlDbType.Image).Value = val[1];
                        break;
                }
            }
            _dbCount = cmd.ExecuteNonQuery();
            _dbCountLog = DbLogGerner(Table, Where, "D");
            return _dbCount;
        }

        /// <summary>
        /// Using Class to Update Data,return int that effect row count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Table">更新</param>
        /// <param name="Where">條件</param>
        /// <returns></returns>
        public int Delete<T>(T Where)
        {
            //logger.Debug("BaseAction_Delete(" + Where + ")");
            string WhereName = GetTableName(Where);
            //string WhereName = Where.GetType().Name;
            //if (Where.GetType().BaseType != null)
            //{
            //    if (Where.GetType().BaseType.Name != "Object")
            //    { WhereName = Where.GetType().BaseType.Name; }
            //}
            //因應欄位名稱與資料表名稱相同而須做處理
            //if (WhereName.StartsWith("Tbl"))
            //{
            //    int startIndex = 3;
            //    int endIndex = WhereName.Length - 3;
            //    WhereName = WhereName.Substring(startIndex, endIndex);
            //}
            //else if (WhereName.EndsWith("Model"))
            //{
            //    int startIndex = 0;
            //    int endIndex = WhereName.Length - 5;
            //    WhereName = WhereName.Substring(startIndex, endIndex);
            //}

            Hashtable WhereParameter = new Hashtable();
            WhereParameter = Service.ClassToHashtable(Where);

            return Delete(WhereName, WhereParameter);
        }



        #endregion Delete

        #region Log
        /// <summary>
        /// 紀錄LOG
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Parameter"></param>
        /// <param name="TYPE"></param>
        /// <returns></returns>
        public int DbLogGerner(string Table, Hashtable Parameter, string TYPE)
        {
            int _dbCount = 0;
            SessionModel sm = SessionModel.Get();
            if ((sm == null || sm.UserInfo == null) && (Table.TONotNullString() != "MEMBER") && (Table.TONotNullString() != "LOGIN_LOG")) { return _dbCount; } //(前後台都要登入才可使用)
            if ((sm.UserInfo != null && sm.UserInfo.UserNo.TONotNullString() == "") && (Table.TONotNullString() != "MEMBER") && (Table.TONotNullString() != "LOGIN_LOG")) { return _dbCount; } //(前後台都要登入才可使用)
            var TableLog = Table + "_Log";
            //var SerialNum = DateTime.Now.ToString("yyyyMMdd");
            string LastMODTIME = sm.LastMODTIME == null || string.IsNullOrEmpty(sm.LastMODTIME) ? DateTime.Now.ToString("yyyyMMddHHmmss") : sm.LastMODTIME;
            //string LastMODUSER = (sm.UserInfo.UserNo.TONotNullString() == "" ? "---" : sm.UserInfo.UserNo.TONotNullString());
            string LastMODUSER = sm.UserInfo != null ? sm.UserInfo.UserNo.TONotNullString() : "user";

            //logger.Debug("LastMODTIME:" + LastMODTIME);

            // LOG沒有這張表
            Dictionary<string, object> args = new Dictionary<string, object>();
            string _sql_tbName = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where 1 = 1 AND TABLE_NAME = @TN ";
            args.Add("TN", TableLog);
            var tb_list = GetList(_sql_tbName, args);
            if (tb_list.ToCount() == 0)
            {
                var _sql = "CREATE TABLE " + TableLog + " ( ";
                foreach (var para in Parameter.Keys)
                {
                    var _key = para.TONotNullString();
                    var _keyT = "";
                    var _keyVal = Parameter[_key].TONotNullString().ToSplit("::");
                    switch (_keyVal.FirstOrDefault().ToLower())
                    {
                        case "int":
                        case "int16":
                        case "int32":
                        case "int64":
                            _keyT = "int";
                            break;
                        case "string":
                        case "varchar":
                            _keyT = "nvarchar(MAX)";
                            break;
                        case "bool":
                        case "boolean":
                            _keyT = "bit";
                            break;
                        case "datetime":
                            _keyT = "datetime";
                            break;
                        case "decimal":
                            _keyT = "decimal(18,0)";
                            break;
                        case "double":
                            _keyT = "float";
                            break;
                        case "byte[]":
                            _keyT = "image";
                            break;
                    }
                    _sql += " " + _key + " " + _keyT + " NULL, ";
                }
                _sql += " MODTIME nvarchar(30) NULL,"; //yyyyMMddHHmmss
                _sql += " MODUSER nvarchar(60) NULL,";
                _sql += " MODTYPE nvarchar(10) NULL,";
                _sql += " )";

                // Execute Command
                SqlCommand cmd = new SqlCommand(_sql, conn, tran);
                cmd.ExecuteNonQuery();
            }

            //LOG沒有這個欄位
            foreach (var para in Parameter.Keys)
            {
                var _key = para.TONotNullString();
                Dictionary<string, object> argsCol = new Dictionary<string, object>();
                string _sql_colName = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TN AND COLUMN_NAME = @CN ";
                argsCol.Add("TN", TableLog);
                argsCol.Add("CN", _key);
                var col_list = GetList(_sql_colName, argsCol);
                if (col_list.ToCount() == 0)
                {
                    var _keyT = "";
                    var _keyVal = Parameter[_key].TONotNullString().ToSplit("::");
                    switch (_keyVal.FirstOrDefault().ToLower())
                    {
                        case "int":
                        case "int16":
                        case "int32":
                        case "int64":
                            _keyT = "int";
                            break;
                        case "string":
                        case "varchar":
                            _keyT = "nvarchar(MAX)";
                            break;
                        case "bool":
                        case "boolean":
                            _keyT = "bit";
                            break;
                        case "datetime":
                            _keyT = "datetime";
                            break;
                        case "decimal":
                            _keyT = "decimal(18,0)";
                            break;
                        case "double":
                            _keyT = "float";
                            break;
                        case "byte[]":
                            _keyT = "image";
                            break;
                    }
                    var _sql = "ALTER TABLE " + TableLog + " ADD " + _key + " " + _keyT;
                    // Execute Command
                    SqlCommand cmd = new SqlCommand(_sql, conn, tran);
                    cmd.ExecuteNonQuery();
                }
            }

            //SessionModel sm = SessionModel.Get();
            Parameter["MODTIME"] = "String::" + LastMODTIME;
            Parameter["MODUSER"] = "String::" + LastMODUSER;
            Parameter["MODTYPE"] = "String::" + TYPE;
            //logger.Debug("Parameter[\"MODTIME\"]:" + LastMODTIME);
            _dbCount = InsertLog(TableLog, Parameter);
            return _dbCount;
        }

        public void DbLog(string Table, Hashtable Parameter)
        {

        }
        #endregion

        #region GetRow

        /// <summary>
        /// 取單筆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        public T GetRow<T>(T param)
        {
            //logger.Debug("BaseAction_GetRow(" + param + ")");
            // 初始化
            T model = (T)Activator.CreateInstance(typeof(T));
            Hashtable hash = new Hashtable();

            // 塞入Hash
            foreach (var pi in param.GetType().GetProperties())
            {
                var piName = pi.Name;
                var piValue = pi.GetValue(param);
                if (piValue != null)
                {
                    hash[piName] = piValue.TONotNullString();
                }
            }

            // 取Model名稱
            var TableName = GetTableName(param);
            //var TableName = param.GetType().Name;
            //if (TableName.StartsWith("Tbl"))
            //{
            //    int startIndex = 3;
            //    int endIndex = TableName.Length - 3;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}
            //else if (TableName.EndsWith("ViewModel"))
            //{
            //    int startIndex = 0;
            //    int endIndex = TableName.Length - 9;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}
            //else if (TableName.EndsWith("Model"))
            //{
            //    int startIndex = 0;
            //    int endIndex = TableName.Length - 5;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}
            //else if (TableName.EndsWith("View"))
            //{
            //    int startIndex = 0;
            //    int endIndex = TableName.Length - 4;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}

            string _keyString = "";
            foreach (var para in hash.Keys)
            {
                _keyString += " and " + para + "= '" + hash[para] + "'";
            }

            //var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();

            string _sql = "select top 1 * from " + TableName + "  with (nolock)  where 1 = 1";
            _sql += _keyString;
            _sql += "";

            //SqlCommand Cmd = null;
            if (this.conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlCommand Cmd = new SqlCommand(_sql, conn);
                    _dt.Load(Cmd.ExecuteReader());
                    //_da.SelectCommand = new SqlCommand(_sql, conn);
                    //_da.Fill(_dt);
                    for (var i = 0; i < _dt.Rows.Count; i++)
                    {
                        var _rows = _dt.Rows[i];
                        model = (T)Activator.CreateInstance(typeof(T));
                        foreach (var pi in model.GetType().GetProperties())
                        {
                            var piName = pi.Name;
                            if (_dt.Columns.Contains(piName))
                            {
                                var rowValue = Convert.ChangeType(_rows[piName], _rows[piName].GetType());
                                if (rowValue.ToString() != "")
                                {
                                    pi.SetValue(model, rowValue);
                                }
                            }
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                SqlCommand Cmd = new SqlCommand(_sql, this.conn, this.tran);
                _dt.Load(Cmd.ExecuteReader());
                //_da.SelectCommand = new SqlCommand(_sql, conn);
                //_da.Fill(_dt);
                for (var i = 0; i < _dt.Rows.Count; i++)
                {
                    var _rows = _dt.Rows[i];
                    model = (T)Activator.CreateInstance(typeof(T));
                    foreach (var pi in model.GetType().GetProperties())
                    {
                        var piName = pi.Name;
                        if (_dt.Columns.Contains(piName))
                        {
                            var rowValue = Convert.ChangeType(_rows[piName], _rows[piName].GetType());
                            if (rowValue.ToString() != "")
                            {
                                pi.SetValue(model, rowValue);
                            }
                        }
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <returns></returns>     
        public IList<T> GetRowList<T>(T param)
        {
            //logger.Debug("BaseAction_GetRowList(" + param + ")");
            // 初始化
            T model = (T)Activator.CreateInstance(typeof(T));
            IList<T> list = new List<T>();
            Hashtable hash = new Hashtable();

            // 塞入Hash
            foreach (var pi in param.GetType().GetProperties())
            {
                var piName = pi.Name;
                var piValue = pi.GetValue(param);
                if (piValue != null)
                {
                    hash[piName] = piValue.TONotNullString();
                }
            }

            // 取Model名稱
            var TableName = GetTableName(param);
            //var TableName = param.GetType().Name;            
            //if (TableName.StartsWith("Tbl"))
            //{
            //    int startIndex = 3;
            //    int endIndex = TableName.Length - 3;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}
            //else if (TableName.EndsWith("Model"))
            //{
            //    int startIndex = 0;
            //    int endIndex = TableName.Length - 5;
            //    TableName = TableName.Substring(startIndex, endIndex);
            //}

            string _keyString = "";
            foreach (var para in hash.Keys)
            {
                _keyString += " and " + para + "= '" + hash[para] + "'";
            }

            var _da = new SqlDataAdapter();
            DataTable _dt = new DataTable();

            string _sql = "select * from " + TableName + " where 1 = 1 ";
            _sql += _keyString;

            if (conn == null)
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    _da.SelectCommand = new SqlCommand(_sql, conn);
                    _da.Fill(_dt);

                    for (var i = 0; i < _dt.Rows.Count; i++)
                    {
                        var _rows = _dt.Rows[i];
                        model = (T)Activator.CreateInstance(typeof(T));
                        foreach (var pi in model.GetType().GetProperties())
                        {
                            var piName = pi.Name;
                            if (_dt.Columns.Contains(piName))
                            {
                                var rowValue = Convert.ChangeType(_rows[piName], _rows[piName].GetType());
                                if (rowValue.ToString() != "")
                                {
                                    pi.SetValue(model, rowValue);
                                }
                            }
                        }
                        list.Add(model);
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                _da.SelectCommand = new SqlCommand(_sql, conn, tran);
                _da.Fill(_dt);

                for (var i = 0; i < _dt.Rows.Count; i++)
                {
                    var _rows = _dt.Rows[i];
                    model = (T)Activator.CreateInstance(typeof(T));
                    foreach (var pi in model.GetType().GetProperties())
                    {
                        var piName = pi.Name;
                        if (_dt.Columns.Contains(piName))
                        {
                            var rowValue = Convert.ChangeType(_rows[piName], _rows[piName].GetType());
                            if (rowValue.ToString() != "")
                            {
                                pi.SetValue(model, rowValue);
                            }
                        }
                    }
                    list.Add(model);
                }
            }

            return list;
        }

        #endregion

        /// <summary>
        /// Service Method
        /// </summary>
        public static class Service
        {
            private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            /// <summary>
            /// Let Class became Hashtable
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="Model"></param>
            /// <returns></returns>
            public static Hashtable ClassToHashtable<T>(T Model)
            {

                Hashtable Parameter = new Hashtable();
                try
                {
                    foreach (var pi in Model.GetType().GetProperties())
                    {
                        var piName = pi.Name;
                        var piType = pi.PropertyType.Name;
                        if (pi.PropertyType.Name.Contains("Nullable"))
                        {
                            piType = pi.PropertyType.GenericTypeArguments.FirstOrDefault().Name;
                        }

                        var piValue = pi.GetValue(Model);
                        if (piValue != null)
                        {
                            Parameter[piName] = piType + "::" + piValue.TONotNullString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.Warn("ClassToHashtable('" + Model.GetType().Name + "'): " + ex.Message);
                    return null;
                }
                return Parameter;
            }

            /// <summary>
            /// 組合 Hashtable [0][1][2]
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="Model"></param>
            /// <returns></returns>
            public static Hashtable ClassToHashtable2<T>(T Model)
            {
                Hashtable Parameter = new Hashtable();
                foreach (var pi in Model.GetType().GetProperties())
                {
                    var piName = pi.Name;
                    var piType = pi.PropertyType.Name;

                    if (pi.PropertyType.Name.Contains("Nullable"))
                    {
                        piType = pi.PropertyType.GenericTypeArguments.FirstOrDefault().Name;
                    }
                    var piValue = pi.GetValue(Model);

                    var piDispName = piName;//piValue.TONotNullString();
                    var dd = pi.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                    if (dd != null) { piDispName = dd.Name; }

                    if (piValue != null)
                    {
                        Parameter[piName] = piType + "::" + piValue.TONotNullString() + "::" + piDispName.TONotNullString();
                    }
                }
                return Parameter;
            }

        }

    }
}