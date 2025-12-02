using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Scope;
using IBatisNet.DataMapper.Configuration.ParameterMapping;
using log4net;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;
using EECOnline.Models.Entities;
using EECOnline.Models;
using Turbo.DataLayer;
using System.Text.RegularExpressions;
using System.Reflection;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using EECOnline.Commons;

namespace EECOnline.DataLayers
{
    /// <summary>
    /// 用來配合 SqlMapper 在原本的 Insert/Update/Delete 異動資料作業前後, 
    /// 植入客制化資料異動記錄的功能
    /// <para>
    /// 注意事項:
    /// 一般功能的 DAO 要繼承(或直接使用) EECOnline.DataLayers.BaseDAO,
    /// 在執行 Insert/Update/Delete 時才會觸發 AfterInsert/AfterUpdate/BeforeDelete
    /// </para>
    /// </summary>
    public class TransLogDAO : ISqlMapperExecuteTracer
    {
        private ILog logger = LogManager.GetLogger(typeof(TransLogDAO));

        #region ISqlMapperExecuteTracer 介面實作

        public void BeforeInsert(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            // 資料新增前, 不需植入任何處理, 直接返回
        }

        public void AfterInsert(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            //logger.Debug("AfterInsert: statementId=" + requestScope.Statement.Id + ", sql: " + requestScope.IDbCommand.CommandText);
            try
            {
                // 抓取新增的資料, 寫入 _LOG 表格
                LogAfterInsert(sqlMapper, requestScope);
            }
            catch (Exception ex)
            {
                throw new Exception("[新增]異動記錄失敗: " + ex.Message, ex);
            }
        }

        public void BeforeUpdate(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            //logger.Debug("BeforeUpdate: statementId=" + requestScope.Statement.Id + ", sql: " + requestScope.IDbCommand.CommandText);

            // 抓取異動前的資料, 再搭配 RequestScope 中將更新的資料, 寫入 _LOG 表格
            // 因為可能存在 Update 動作同時更新 where 條件欄位值的情況
            // 同一組 where 條件在異動後, 會有抓不到資料的可能
            // 所以, 不能等異動後再抓資料寫 _LOG

            try
            {
                LogBeforeUpdate(sqlMapper, requestScope);
            }
            catch (Exception ex)
            {
                throw new Exception("[修改]異動記錄失敗: " + ex.Message, ex);
            }
        }

        public void AfterUpdate(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            // 異動作業後, 已不需要再寫入 _LOG 
        }

        public void BeforeDelete(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            //logger.Debug("BeforeDelete: statementId=" + requestScope.Statement.Id + ", sql: " + requestScope.IDbCommand.CommandText);

            // 抓取刪除前的資料, 寫入 _LOG 表格

            try
            {
                LogBeforeDelete(sqlMapper, requestScope);
            }
            catch (Exception ex)
            {
                throw new Exception("[刪除]異動記錄失敗: " + ex.Message, ex);
            }
        }

        public void AfterDelete(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            // 資料刪除後, 不需植入任何處理, 直接返回
        }

        #endregion

        private void LogAfterInsert(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            TCustomSqlStatement stmt = ParseSql(requestScope.IDbCommand.CommandText);
            TInsertSqlStatement insStmt;
            if (stmt == null)
            {
                // ParseSql 失敗(不支援的語法/非 Insert/Update/Delete), 忽略返回
                return;
            }

            if (stmt is TInsertSqlStatement)
            {
                insStmt = (TInsertSqlStatement)stmt;

                if (insStmt.Values == null
                    || insStmt.Values.Count != 1)
                {
                    throw new Exception("尚未支援含有複雜 Insert Values 字句的異動 SQL 指令\n" + insStmt.Values);
                }
            }
            else
            {
                throw new Exception("LogAfterInsert: Sql 指令 Parse 結果類型不符, statementType=" + stmt.sqlstatementtype);
            }

            // 檢查 xxxxxx_LOG 表格 Entity 是否有定義
            IDBRow logTableEntity = CheckEntity(insStmt.TargetTable.Name);
            if (logTableEntity == null)
            {
                // xxxxxx_LOG 表格 Entity 沒有定義, 略過返回
                return;
            }

            RowBaseDAO daoLog = new RowBaseDAO(sqlMapper);
            Hashtable param = new Hashtable();
            param["tableName"] = insStmt.TargetTable.Name;
            var list = daoLog.QueryForListAll<Hashtable>(daoLog.CommonSqlMapNamespace + ".isTargetTableExistInDB", param);
            if (list.Count() == 0)
            {
                daoLog.Create(logTableEntity, null, true);
            }


            foreach (var pi in logTableEntity.GetType().GetProperties())
            {
                RowBaseDAO daoColumnLog = new RowBaseDAO(sqlMapper);
                Hashtable paramC = new Hashtable();
                paramC["tableName"] = insStmt.TargetTable.Name;
                paramC["columnName"] = pi.Name;
                var listC = daoColumnLog.QueryForListAll<Hashtable>(daoLog.CommonSqlMapNamespace + ".isTargetColumnExistInTable", paramC);
                if (listC.Count() == 0)
                {
                    daoColumnLog.Alter(logTableEntity, null, true, pi.Name);
                }
            }


            // 新增一筆資料到 XXXXXX_LOG 表格
            logger.Info("LogAfterInsert(" + insStmt.TargetTable.Name + ") => " + logTableEntity.GetTableName() + " : " + logTableEntity.GetType().Name);

            // Insert 的欄位清單
            IList<string> columns = new List<string>();
            for (int i = 0; i < insStmt.ColumnList.Count; i++)
            {
                TObjectName colName = insStmt.ColumnList.getObjectName(i);
                columns.Add(colName.ToScript());
            }
            logger.Debug("Columns: " + string.Join(", ", columns));

            // Insert Values 處理
            Regex regex = new Regex(@":\d+");
            IList<object> values = new List<object>();
            var valueList = insStmt.Values.getMultiTarget(0).ColumnList;
            for (int i = 0; i < valueList.Count; i++)
            {
                var item = valueList.getResultColumn(i).ToString();

                values.Add(GetParameterValue(requestScope.IDbCommand.Parameters, item));

                logger.Debug("values(" + i + ")= " + values[i]);
            }

            if (columns.Count != values.Count)
            {
                throw new Exception("LogAfterInsert(" + insStmt.TargetTable.Name + ") Insert Columns 跟 Values 個數不符");
            }

            // 用來寫入 XXXXXX_LOG 的 DAO 要直接使用 Turbo.DataLayer.RowBaseDAO
            RowBaseDAO dao = new RowBaseDAO(sqlMapper);

            for (int i = 0; i < columns.Count; i++)
            {
                SetProperty(logTableEntity, columns[i], values[i]);
            }

            SetMODInfo(dao, logTableEntity, "I");

            // 為避免觸發遞迴 AfterInsert 事件, 先清掉 ISqlMapper.ExecuteTracer, 
            // 等 Insert() _LOG 表格後再設回去
            ISqlMapperExecuteTracer keep = sqlMapper.ExecuteTracer;
            sqlMapper.ExecuteTracer = null;

            dao.Insert(logTableEntity.GetType(), logTableEntity, false, true, "I");

            sqlMapper.ExecuteTracer = keep;
        }

        private void LogBeforeUpdate(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            TCustomSqlStatement stmt = ParseSql(requestScope.IDbCommand.CommandText);
            TUpdateSqlStatement updStmt;
            if (stmt == null)
            {
                // ParseSql 失敗(不支援的語法/非 Insert/Update/Delete), 忽略返回
                return;
            }

            if (stmt is TUpdateSqlStatement)
            {
                updStmt = (TUpdateSqlStatement)stmt;

                if (updStmt.WhereClause == null)
                {
                    throw new Exception("LogBeforeUpdate: Sql 指令 Parse 結果 WhereClause 為空值 \n" + requestScope.IDbCommand.CommandText);
                }
            }
            else
            {
                throw new Exception("LogBeforeUpdate: Sql 指令 Parse 結果類型不符, statementType=" + stmt.sqlstatementtype);
            }

            // 檢查 xxxxxx_LOG 表格 Entity 是否有定義
            IDBRow logTableEntity = CheckEntity(updStmt.TargetTable.Name);
            if (logTableEntity == null)
            {
                // xxxxxx_LOG 表格 Entity 沒有定義, 略過返回
                return;
            }

            RowBaseDAO daoLog = new RowBaseDAO(sqlMapper);
            Hashtable param = new Hashtable();
            param["tableName"] = updStmt.TargetTable.Name;
            var list = daoLog.QueryForListAll<Hashtable>(daoLog.CommonSqlMapNamespace + ".isTargetTableExistInDB", param);
            if (list.Count() == 0)
            {
                daoLog.Create(logTableEntity, null, true);
            }


            foreach (var pi in logTableEntity.GetType().GetProperties())
            {
                RowBaseDAO daoColumnLog = new RowBaseDAO(sqlMapper);
                Hashtable paramC = new Hashtable();
                paramC["tableName"] = updStmt.TargetTable.Name;
                paramC["columnName"] = pi.Name;
                var listC = daoColumnLog.QueryForListAll<Hashtable>(daoLog.CommonSqlMapNamespace + ".isTargetColumnExistInTable", paramC);
                if (listC.Count() == 0)
                {
                    daoColumnLog.Alter(logTableEntity, null, true, pi.Name);
                }
            }

            // 抓取資料庫表格中的(異動後)資料, 新增一筆資料到 XXXXXX_LOG 表格
            logger.Info("LogBeforeUpdate(" + updStmt.TargetTable.Name + ") => " + logTableEntity.GetTableName() + " : " + logTableEntity.GetType().Name);

            // 檢查 xxxxxx 表格 Entity 是否有定義
            IDBRow tblEntity = CheckEntity(updStmt.TargetTable.Name);
            if (tblEntity == null)
            {
                // xxxxxx 表格 Entity 沒有定義
                throw new Exception("LogBeforeUpdate: 找不到表格 " + updStmt.TargetTable.Name + " 對應的 Entity Model");
            }

            // 由 parse 結果取得 where condition 作為讀取資料的條件
            logger.Debug("LogBeforeUpdate: Where " + updStmt.WhereClause.Condition);

            /*
             * 2018.11.05,
             * 為了支援 Where 條件中的 < > <= >= 這類運算, 
             * 不再使用 RowOP 去抓來源表格資料列
             */

            string whereCondition = updStmt.WhereClause.Condition.String;
            Hashtable parmObject = new Hashtable();
            GetWhereColumns(updStmt.WhereClause.Condition, requestScope.IDbCommand.Parameters, parmObject, ref whereCondition);

            RowBaseDAO dao = new RowBaseDAO(sqlMapper);

            string sql = "Select * From " + tblEntity.GetTableName() +
                    " Where " + whereCondition;

            //IList<IDBRow> tblRows = dao.GetRowList(tblEntity.GetType(), tblEntity);
            IList tblRows = dao.QuerySqlForListAll(tblEntity.GetType(), null, sql, parmObject);
            if (tblRows == null || tblRows.Count == 0)
            {
                logger.Warn("LogBeforeUpdate: 找不到表格 " + updStmt.TargetTable.Name + " 修改前的資料 " + JsonConvert.SerializeObject(parmObject));
                return;
            }

            // 從 update statement 中取出欄位更新資訊
            logger.Debug("LogBeforeUpdate: SET ");

            Dictionary<string, object> updateColumns = new Dictionary<string, object>();
            int setCols = updStmt.ResultColumnList.Count;
            for (int i = 0; i < setCols; i++)
            {
                TResultColumn col = updStmt.ResultColumnList.getResultColumn(i);
                string colName = col.Expr.LeftOperand.String;
                string item = col.Expr.RightOperand.String;

                updateColumns[colName] = GetParameterValue(requestScope.IDbCommand.Parameters, item);
                logger.Debug("[" + colName + " = " + updateColumns[colName] + "]");
            }


            // 為避免觸發遞迴 AfterInsert 事件, 先清掉 ISqlMapper.ExecuteTracer, 
            // 等 Insert() _LOG 表格後再設回去
            ISqlMapperExecuteTracer keep = sqlMapper.ExecuteTracer;
            sqlMapper.ExecuteTracer = null;

            // 將修改後資料(有可能會有多筆) 寫入 XXXXX_LOG 
            foreach (var tblRow in tblRows)
            {
                IDBRow logEntity = (IDBRow)Activator.CreateInstance(logTableEntity.GetType());
                logEntity.InjectFrom(tblRow);

                // 套用 Update Statement 中即將更新的資料
                foreach (var key in updateColumns.Keys)
                {
                    SetProperty(logEntity, key.ToUpper(), updateColumns[key]);
                }

                SetMODInfo(dao, logEntity, "U");

                //dao.Insert(logEntity.GetType(), logEntity);
                dao.Insert(logEntity.GetType(), logEntity, false, true, "U");

            }

            sqlMapper.ExecuteTracer = keep;

        }


        private void LogBeforeDelete(ISqlMapper sqlMapper, RequestScope requestScope)
        {
            TCustomSqlStatement stmt = ParseSql(requestScope.IDbCommand.CommandText);
            TDeleteSqlStatement delStmt;
            if (stmt == null)
            {
                // ParseSql 失敗(不支援的語法/非 Insert/Update/Delete), 忽略返回
                return;
            }

            if (stmt is TDeleteSqlStatement)
            {
                delStmt = (TDeleteSqlStatement)stmt;

                if (delStmt.WhereClause == null)
                {
                    throw new Exception("LogBeforeDelete: Sql 指令 Parse 結果 WhereClause 為空值 \n" + requestScope.IDbCommand.CommandText);
                }
            }
            else
            {
                throw new Exception("LogBeforeDelete: Sql 指令 Parse 結果類型不符, statementType=" + stmt.sqlstatementtype);
            }

            // 檢查 xxxxxx_LOG 表格 Entity 是否有定義
            IDBRow logTableEntity = CheckEntity(delStmt.TargetTable.Name);
            if (logTableEntity == null)
            {
                // xxxxxx_LOG 表格 Entity 沒有定義, 略過返回
                return;
            }

            RowBaseDAO daoLog = new RowBaseDAO(sqlMapper);
            Hashtable param = new Hashtable();
            param["tableName"] = delStmt.TargetTable.Name;
            var list = daoLog.QueryForListAll<Hashtable>(daoLog.CommonSqlMapNamespace + ".isTargetTableExistInDB", param);
            if (list.Count() == 0)
            {
                daoLog.Create(logTableEntity, null, true);
            }


            foreach (var pi in logTableEntity.GetType().GetProperties())
            {
                RowBaseDAO daoColumnLog = new RowBaseDAO(sqlMapper);
                Hashtable paramC = new Hashtable();
                paramC["tableName"] = delStmt.TargetTable.Name;
                paramC["columnName"] = pi.Name;
                var listC = daoColumnLog.QueryForListAll<Hashtable>(daoLog.CommonSqlMapNamespace + ".isTargetColumnExistInTable", paramC);
                if (listC.Count() == 0)
                {
                    daoColumnLog.Alter(logTableEntity, null, true, pi.Name);
                }
            }

            // 抓取資料庫表格中的(異動前)資料, 新增一筆資料到 XXXXXX_LOG 表格
            logger.Info("LogBeforeDelete(" + delStmt.TargetTable.Name + ") => " + logTableEntity.GetTableName() + " : " + logTableEntity.GetType().Name);

            // 檢查 xxxxxx 表格 Entity 是否有定義
            IDBRow tblEntity = CheckEntity(delStmt.TargetTable.Name);
            if (tblEntity == null)
            {
                // xxxxxx 表格 Entity 沒有定義
                throw new Exception("LogBeforeDelete: 找不到表格 " + delStmt.TargetTable.Name + " 對應的 Entity Model");
            }

            // 由 parse 結果取得 where condition 作為讀取資料的條件
            logger.Debug("LogBeforeDelete: Where " + delStmt.WhereClause.Condition);

            /*
             * 2018.11.05,
             * 為了支援 Where 條件中的 < > <= >= 這類運算, 
             * 不再使用 RowOP 去抓來源表格資料列
            */

            string whereCondition = delStmt.WhereClause.Condition.String;
            Hashtable parmObject = new Hashtable();
            GetWhereColumns(delStmt.WhereClause.Condition, requestScope.IDbCommand.Parameters, parmObject, ref whereCondition);

            RowBaseDAO dao = new RowBaseDAO(sqlMapper);

            string sql = "Select * From " + tblEntity.GetTableName() +
                    " Where " + whereCondition;

            //IList<IDBRow> tblRows = dao.GetRowList(tblEntity.GetType(), tblEntity);
            IList tblRows = dao.QuerySqlForListAll(tblEntity.GetType(), null, sql, parmObject);

            if (tblRows == null || tblRows.Count == 0)
            {
                logger.Warn("LogBeforeDelete: 找不到表格 " + delStmt.TargetTable.Name + " 刪除前的資料 " + JsonConvert.SerializeObject(parmObject));
                return;
            }

            // 為避免觸發遞迴 AfterInsert 事件, 先清掉 ISqlMapper.ExecuteTracer, 
            // 等 Insert() _LOG 表格後再設回去
            ISqlMapperExecuteTracer keep = sqlMapper.ExecuteTracer;
            sqlMapper.ExecuteTracer = null;

            // 將刪除前資料(有可能會有多筆) 寫入 XXXXX_LOG 
            foreach (var tblRow in tblRows)
            {
                IDBRow logEntity = (IDBRow)Activator.CreateInstance(logTableEntity.GetType());
                logTableEntity.InjectFrom(tblRow);

                SetMODInfo(dao, logTableEntity, "D");
                dao.Insert(logTableEntity.GetType(), logTableEntity, false, true, "D");
            }

            sqlMapper.ExecuteTracer = keep;
        }

        /// <summary>
        /// 設定 XXXXXX_LOG 表格的: MODSEQ, MODPRGID 及 MODTYPE
        /// <para>MODTYPE: I.新增, U.修改, D.刪除</para>
        /// </summary>
        /// <param name="logEntity"></param>
        /// <param name="modType"></param>
        private void SetMODInfo(RowBaseDAO dao, IDBRow logEntity, string modType)
        {
            SetProperty(logEntity, "MODTYPE", modType);
            SetProperty(logEntity, "MODTIME", MyHelperUtil.DateTimeToLongTwString(DateTime.Now));
        }

        /// <summary>
        /// 判斷 item 是否為參數 place holder  :NN
        /// 若是參數則由 parameters 取出對應的值, 並置換 whereCondition 中的參數格式為 SqlMap 格式
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="parmItem"></param>
        /// <param name="propName"></param>
        /// <param name="whereCondition"></param>
        /// <returns></returns>
        private object GetParameterValue(IDataParameterCollection parameters, string parmItem, ref string propName, ref string whereCondition)
        {
            object value = null;
            // 判斷 item 是否為參數 place holder  :NN
            Match match = Regex.Match(parmItem, @"@[a-z]{5}\d+");
            if (match.Success)
            {
                // 以參數值置換
                int pIdx = Convert.ToInt32(match.Groups[0].Value.Replace("@param", ""));
                if (string.IsNullOrEmpty(propName))
                {
                    propName = "PARAMS" + pIdx;
                }
                IDbDataParameter parm = (IDbDataParameter)parameters[pIdx];
                if (parm.Value != DBNull.Value)
                {
                    value = parm.Value;

                    whereCondition = (whereCondition + " ").Replace(parmItem + " ", "#" + propName + "# ");
                }
            }

            return value;
        }

        private object GetParameterValue(IDataParameterCollection parameters, string parmItem)
        {
            object value = null;
            // 判斷 item 是否為參數 place holder  :NN
            Match match = Regex.Match(parmItem, @"@[a-z]{5}\d+");
            if (match.Success)
            {
                // 以參數值置換
                int pIdx = Convert.ToInt32(match.Groups[0].Value.Replace("@param", ""));
                IDbDataParameter parm = (IDbDataParameter)parameters[pIdx];
                if (parm.Value != DBNull.Value)
                {
                    value = parm.Value;
                }
            }
            else
            {
                // 常數字串值, 移除前後單引號
                //value = Regex.Replace(parmItem, "^'|'$", "");
                value = parmItem.TrimStart('\'').TrimEnd('\'');
            }

            return value;
        }


        /// <summary>
        /// 遞迴解析 whereExpression 以得到 where 條件欄位列表,
        /// 據以重組符合 SqlMap 參數格式的 where 字串, 並將條件值寫入 parameter entity
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="parameters"></param>
        /// <param name="parmObject"></param>
        /// <param name="whereCondition"></param>
        private void GetWhereColumns(TExpression whereExpression, IDataParameterCollection parameters, Hashtable parmObject, ref string whereCondition)
        {
            if (whereExpression.OperatorToken.String == "="
                || whereExpression.OperatorToken.String == "<="
                || whereExpression.OperatorToken.String == ">="
                || whereExpression.OperatorToken.String == "<"
                || whereExpression.OperatorToken.String == ">"
                || whereExpression.OperatorToken.String == "<>"
                )
            {
                // 找到一組: Column [= | <= | >= | < | > | <>] Value
                if (!whereExpression.LeftOperand.Leaf || !whereExpression.RightOperand.Leaf)
                {
                    throw new Exception("異動記錄不支援 WHERE 條件中的語法: " + whereExpression.LeftOperand + " " + whereExpression.OperatorToken + " " + whereExpression.RightOperand);
                }

                string colName = whereExpression.LeftOperand.String;
                string colValue = whereExpression.RightOperand.String;

                // 檢查 parmObject 是否已存在同名的 colName 條件, 若存在則名稱字尾加上 [_序數]
                object chkVal = parmObject[colName];
                int seq = 1;
                while (chkVal != null)
                {
                    seq++;
                    colName += "_" + seq;
                    chkVal = parmObject[colName];
                }

                object propValue = GetParameterValue(parameters, colValue, ref colName, ref whereCondition);
                parmObject[colName] = propValue;

                logger.Debug("GetWhereColumns: " + colName + " => " + colValue + " = " + propValue);

                return;
            }
            else if ("||".Equals(whereExpression.OperatorToken.String, StringComparison.OrdinalIgnoreCase))
            {
                // 找到字串相加運算元 ||
                // 判斷 LeftOperand 及 RightOperand 是否為 leaf 節點
                // 非 leaf 節點不用處理(在遞迴中會處理到)

                if (whereExpression.LeftOperand.Leaf)
                {
                    // 判斷是否為參數
                    string colValue = whereExpression.LeftOperand.String;
                    string colName = string.Empty;
                    object propValue = GetParameterValue(parameters, colValue, ref colName, ref whereCondition);
                    if (propValue != null)
                    {
                        parmObject[colName] = propValue;
                        logger.Debug("GetWhereColumns: " + colName + " => " + colValue + " = " + propValue);
                    }
                }
                if (whereExpression.RightOperand.Leaf)
                {
                    // 判斷是否為參數
                    string colValue = whereExpression.RightOperand.String;
                    string colName = string.Empty;
                    object propValue = GetParameterValue(parameters, colValue, ref colName, ref whereCondition);
                    if (propValue != null)
                    {
                        parmObject[colName] = propValue;
                        logger.Debug("GetWhereColumns: " + colName + " => " + colValue + " = " + propValue);
                    }
                }
            }
            else if ("IN".Equals(whereExpression.OperatorToken.String, StringComparison.OrdinalIgnoreCase))
            {
                // 找到 IN Subquery
                // whereExpression.LeftOperand: 先視為不存在參數
                // whereExpression.RightOperand: 為獨立的 Subquery, 往下遞迴處理
                if (whereExpression.RightOperand.SubQuery != null)
                {
                    TExpression subWhereCondition = whereExpression.RightOperand.SubQuery.WhereClause.Condition;
                    GetWhereColumns(subWhereCondition, parameters, parmObject, ref whereCondition);
                }
            }
            else if ("BETWEEN".Equals(whereExpression.OperatorToken.String, StringComparison.OrdinalIgnoreCase))
            {
                // 找到一組: Column BETWEEN Value1 AND Value2
                // BetweenOperand: Column Name

                string colName = whereExpression.BetweenOperand.String;
                string colName2 = colName + "_2";

                string colValue = whereExpression.LeftOperand.String;
                string colValue2 = whereExpression.RightOperand.String;

                object propValue = GetParameterValue(parameters, colValue, ref colName, ref whereCondition);
                parmObject[colName] = propValue;
                object propValue2 = GetParameterValue(parameters, colValue2, ref colName2, ref whereCondition);
                parmObject[colName2] = propValue2;

                logger.Debug("GetWhereColumns: " + colName + " BETWEEN " + colValue + " AND " + colValue2 + " => [" + propValue + " AND " + propValue2 + "]");
            }
            else if (!"AND".Equals(whereExpression.OperatorToken.String, StringComparison.OrdinalIgnoreCase)
                        && !"OR".Equals(whereExpression.OperatorToken.String, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: 待處理 IS, IS NOT 
                throw new Exception("異動記錄不支援 WHERE 條件中的 " + whereExpression.OperatorToken.String + " 運算元");
            }

            if (whereExpression.LeftOperand != null && !whereExpression.LeftOperand.Leaf)
            {
                GetWhereColumns(whereExpression.LeftOperand, parameters, parmObject, ref whereCondition);
            }
            if (whereExpression.RightOperand != null && !whereExpression.RightOperand.Leaf)
            {
                GetWhereColumns(whereExpression.RightOperand, parameters, parmObject, ref whereCondition);
            }
        }


        private TCustomSqlStatement ParseSql(string sql)
        {
            TGSqlParser parser = new TGSqlParser(EDbVendor.dbvmssql);
            parser.sqltext = sql;
            int rtn = parser.parse();
            if (rtn == 0)
            {
                // Parse Success
                if (parser.sqlstatements.Count == 1)
                {
                    return parser.sqlstatements[0];
                }
                else
                {
                    throw new Exception("尚未支援含有 subquery 的異動 SQL 指令\n" + sql);
                }
            }
            else
            {
                // Parse Syntax Error
                logger.Error("ParseAndLog: Syntax Error, " + parser.Errormessage);
                foreach (var item in parser.SyntaxErrors)
                {
                    logger.Error("ParseAndLog: (" + item.errorno + "" + item.errortype + ") " + item.hint + " at " + item.tokentext);
                }
                //throw new Exception("ParseAndLog: Syntax Error, " + parser.Errormessage);
                //不丟出 exception, 返回 null, 避免不支援SQL功能(如 Store Procedure 呼叫)掛掉
                return null;
            }

        }


        /// <summary>
        /// 檢查 tableName 對應的 Tbl[tableName] 表格 Entity 是否有定義,
        /// 若存在則回傳 Entity Instance, 若不存在則回傳 null.
        /// <para>若 logEntity = true, 則檢查 Tbl[tableName]_LOG 表格 Entity</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private IDBRow CheckEntity(string tableName, bool logEntity = false)
        {
            IDBRow entity = null;
            string strFullyQualifiedName = "EECOnline.Models.Entities.Tbl" + tableName.ToUpper() + (logEntity ? "_LOG" : "");
            try
            {
                Type t = Type.GetType(strFullyQualifiedName);
                if (t != null)
                {
                    // Entity Class 有定義, 建立並檢查 Instance
                    object obj = Activator.CreateInstance(t);
                    if (obj is IDBRow)
                    {
                        entity = (IDBRow)obj;
                    }
                }
                else
                {
                    // Entity Class 沒有定義
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CheckEntity " + strFullyQualifiedName + ": " + ex.Message, ex);
            }
            return entity;
        }

        private void SetProperty(IDBRow obj, string propertyName, object value)
        {
            PropertyInfo prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (null != prop && prop.CanWrite)
            {
                prop.SetValue(obj, value, null);
            }
        }

    }

    class WhereColumn
    {
        public string ColumnName { get; set; }

        public string OperatorToken { get; set; }

        public string Value { get; set; }
    }

}