using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using ES.Commons;
using System.ComponentModel.DataAnnotations;
using ES.Action;
using System.Data.SqlClient;
using ES.Utils;
using Dapper;

namespace ES.Models
{
    /// <summary>
    /// 貨品資料-動態表格
    /// </summary>
    public class GoodsDynamicGrid<T> : BaseAction

    {
        /// <summary>
        /// 原Bindding模組名稱(Goods)
        /// </summary>
        public string SourceModelName { get; set; }

        /// <summary>
        /// 原Bindding模組名稱(Goods.)[多一個 . ]
        /// </summary>
        public string SourceModelNameFull
        {
            get
            {
                return SourceModelName + ".";
            }
            set
            {
                SourceModelName = value.ToLeft(SourceModelName.ToCount() - 1);
            }
        }
        /// <summary>
        /// 原Bindding模組名稱(Goods.)[多一個 . ]
        /// </summary>
        public string SourceModelIdFull
        {
            get
            {
                return SourceModelName + "_";
            }
            set
            {
                SourceModelName = value.ToLeft(SourceModelName.ToCount() - 1);
            }
        }

        /// <summary>
        ///  唯讀
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        ///  新增按鈕
        /// </summary>
        public bool IsNewOpen { get; set; }

        /// <summary>
        ///  刪除按鈕
        /// </summary>
        public bool IsDeleteOpen { get; set; }

        public string APP_ID { get; set; }

        public T model { get; set; }

        public List<T> GoodsList { get; set; }

        public void GetGoodsList()
        {

            if (APP_ID.TONotNullString() != "")
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    var TableName = model.GetType().Name;
                    if (model.GetType().BaseType != null)
                    {
                        if (model.GetType().BaseType.Name != "Object")
                        { TableName = model.GetType().BaseType.Name; }
                    }
                    if (TableName.StartsWith("Tbl"))
                    {
                        int startIndex = 3;
                        int endIndex = TableName.Length - 3;
                        TableName = TableName.Substring(startIndex, endIndex);
                    }
                    else if (TableName.EndsWith("Model"))
                    {
                        int startIndex = 0;
                        int endIndex = TableName.Length - 5;
                        TableName = TableName.Substring(startIndex, endIndex);
                    }

                    string _sql =
                        @"SELECT * FROM " + TableName + " WHERE 1=1 ";

                    _sql += "and app_id = '" + APP_ID + "'";

                    try
                    {
                        GoodsList = conn.Query<T>(_sql).ToList();
                        if (GoodsList.ToCount() == 0)
                        {
                            GoodsList = new List<T>();
                            GoodsList.Add(model);
                        }

                    }
                    catch (Exception ex)
                    {
                        GoodsList = null;
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            else
            {
                GoodsList = new List<T>();
                GoodsList.Add(model);
            }
        }

        public void SaveGoodsList(string GoodsNO = null)
        {
            conn = DataUtils.GetConnection();
            conn.Open();

            if (this.APP_ID.TONotNullString() != "")
            {
                var ModelPi_AppId = model.GetType().GetProperties().Where(m => m.Name == "APP_ID").FirstOrDefault();
                ModelPi_AppId.SetValue(model, this.APP_ID);
                base.Delete(model);
                var i = 0;
                foreach (var item in GoodsList)
                {
                    i++;
                    if (GoodsNO != null)
                    {
                        var ModelPiNo = model.GetType().GetProperties().Where(m => m.Name == GoodsNO).FirstOrDefault();
                        ModelPiNo.SetValue(model, i);
                    }
                    foreach (var pi in item.GetType().GetProperties())
                    {

                        var piName = pi.Name;
                        var piValue = pi.GetValue(item);
                        // 005001,005003 成分內容 數量
                        if ((piName == "DI_CONT" || piName == "F11_QUANTITY") && piValue.TONotNullString() == "")
                        {
                            piValue = "0";
                        }
                        // 005001,005003 成分內容 單位 成分內容(中文)
                        if((piName == "DI_UNIT" || piName == "F11_UNIT" || piName == "F11_SCI_NAME") && piValue.TONotNullString() == "")
                        {
                            piValue = "";
                        }
                        if (piValue != null)
                        {
                            var ModelPi = model.GetType().GetProperties().Where(m => m.Name == piName).FirstOrDefault();
                            ModelPi.SetValue(model, piValue);
                        }
                    }
                    base.Insert(model);
                }
            }
        }
    }

}