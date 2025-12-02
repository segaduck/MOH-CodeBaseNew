using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_Api_User : IDBRow
    {
        [IdentityDBField]
        public long? keyid { get; set; }
        public string user_name { get; set; }
        public string user_pwd { get; set; }
        public string use_dates { get; set; }
        public string use_datee { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_Api_User;
        }
    }
}