using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblLOGINLOG : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? loginlog_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string moduser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string modusername { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string modtime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string modip { get; set; }
        
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.LOGINLOG;
        }
    }
}