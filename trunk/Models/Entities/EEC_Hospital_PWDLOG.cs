using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_Hospital_PWDLOG : IDBRow
    {
        [IdentityDBField]
        public int? amlog_id { get; set; }
        public string hospital_code { get; set; }
        public string status { get; set; }
        public string pwd { get; set; }
        public string moduserid { get; set; }
        public string modusername { get; set; }
        public string modtime { get; set; }
        public string modip { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_Hospital_PWDLOG;
        }
    }
}