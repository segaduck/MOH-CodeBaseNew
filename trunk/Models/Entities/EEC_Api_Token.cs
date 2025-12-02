using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_Api_Token : IDBRow
    {
        [IdentityDBField]
        public long? keyid { get; set; }
        public string user_name { get; set; }
        public string token { get; set; }
        public string createtime { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_Api_Token;
        }
    }
}