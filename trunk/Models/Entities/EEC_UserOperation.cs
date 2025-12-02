using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_UserOperation : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }

        /// <summary>操作者 IDNO</summary>
        public string idno { get; set; }

        /// <summary>操作者 姓名</summary>
        public string name { get; set; }

        /// <summary>登入方式</summary>
        public string login_type { get; set; }

        /// <summary>登入狀態 (是/否成功)</summary>
        public string login_status { get; set; }

        /// <summary>IP</summary>
        public string ip { get; set; }

        /// <summary>操作時間</summary>
        public string opra_time { get; set; }

        /// <summary>操作功能別(編碼)</summary>
        public string opra_type { get; set; }

        /// <summary>操作功能別(名稱)</summary>
        public string opra_type_name { get; set; }

        /// <summary>是否下載病歷</summary>
        public string is_download { get; set; }

        /// <summary>下載病歷時間</summary>
        public string download_time { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_UserOperation;
        }
    }
}