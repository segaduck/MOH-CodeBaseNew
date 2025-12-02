using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class EEC_NHICardVerify : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? NHIID { get; set; }

        public string successURL { get; set; }
        public string toVerify { get; set; }
        public string IDNO { get; set; }
        public string USERNAME { get; set; }
        public string BIRTHDAY { get; set; }
        public string EMAIL { get; set; }
        public string UUID { get; set; }
        public string result { get; set; }

        public DateTime? CREATEDATE { get; set; }
        public DateTime? UPDATADATE { get; set; }
        /// <summary>
        /// 1:申請/2:查詢
        /// </summary>
        public string fType { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_NHICardVerify;
        }
    }
}