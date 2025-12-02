using EECOnline.Models;
using EECOnline.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Turbo.DataLayer;

namespace EECOnline.DataLayers
{
    /// <summary>
    /// 原本 BaseDAO 的內容, 已拉到 Turbo.DataLayer.RowBaseDAO,
    /// 保留這個class以維持原有程式相容性
    /// </summary>
    public class BaseDAO : RowBaseDAO
    {
        /// <summary>
        /// 以預設的 SqlMap config 連接資料庫
        /// </summary>
        public BaseDAO() : base()
        {
            base.PageSize = ConfigModel.DefaultPageSize;

            // 植入客制化資料異動記錄的功能
            base.SetExecuteTracert(new TransLogDAO());
        }

        /// <summary>
        /// 以指定的 SqlMap config 連接資料庫
        /// </summary>
        /// <param name="sqlMapConfig"></param>
        public BaseDAO(string sqlMapConfig)
            : base(sqlMapConfig)
        {
            base.PageSize = ConfigModel.DefaultPageSize;

            // 植入客制化資料異動記錄的功能
            base.SetExecuteTracert(new TransLogDAO());
        }
       
    }
}