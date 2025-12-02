using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAMURLOG : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? role_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sysid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string modules { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string submodules { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string prgid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string exception { get; set; }
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
            return StaticCodeMap.TableName.AMURLOG;
        }
    }
}