using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 最近使用
    /// </summary>
    public class TblVISIT_RECORD : IDBRow
    {
        /// <summary>
        /// 控制器名稱
        /// </summary>
        public string control_name { get; set; }

        /// <summary>
        /// 動作名稱
        /// </summary>
        public string action_name { get; set; }

        /// <summary>
        /// 功能名稱
        /// </summary>
        public string funcm_name { get; set; }

        /// <summary>
        /// 功能連結
        /// </summary>
        public string funcm_link { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        public string userno { get; set; }

        /// <summary>
        /// 狀態
        /// 1 成功
        /// 0 失敗
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 使用方式
        /// 0 查詢
        /// 1 新增
        /// 2 修改
        /// 3 刪除
        /// </summary>
        public string use_type { get; set; }

        /// <summary>
        /// 0 前台
        /// 1 後台
        /// </summary>
        public string urlwhere { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string note { get; set; }
        
        /// <summary>
        /// 編輯時間
        /// </summary>
        public DateTime? modtime { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.VISIT_RECORD;
        }
    }
}