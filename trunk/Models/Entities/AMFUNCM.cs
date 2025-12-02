using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 程式功能
    /// </summary>
    public class TblAMFUNCM : IDBRow
    {
        /// <summary>
        /// IDKEY
        /// </summary>
        [IdentityDBField]
        public int? amlfun_id { get; set; }

        /// <summary>
        /// 清單第一層
        /// </summary>
        public string sysid { get; set; }

        /// <summary>
        /// 清單第二層
        /// </summary>
        public string modules { get; set; }

        /// <summary>
        /// 清單第三層
        /// </summary>
        public string submodules { get; set; }

        /// <summary>
        /// 功能編號
        /// </summary>
        public string prgid { get; set; }

        /// <summary>
        /// 功能名稱
        /// </summary>
        public string prgname { get; set; }

        /// <summary>
        /// 功能排序
        /// </summary>
        public string prgorder { get; set; }

        /// <summary>
        /// 功能是否顯示於清單
        /// </summary>
        public string showmenu { get; set; }

        /// <summary>
        /// 附加觸發條件
        /// </summary>
        public string querystring { get; set; }

        /// <summary>
        /// 編輯者帳號
        /// </summary>
        public string moduser { get; set; }

        /// <summary>
        /// 編輯者姓名
        /// </summary>
        public string modusername { get; set; }

        /// <summary>
        /// 編輯時間
        /// </summary>
        public string modtime { get; set; }

        /// <summary>
        /// 編輯IP
        /// </summary>
        public string modip { get; set; }

        /// <summary>
        /// 刪除註記
        /// </summary>
        public string del_mk { get; set; }
        
        /// <summary>
        /// 標頭用
        /// </summary>
        public string showlist { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.AMFUNCM;
        }
    }
}