using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_CommonType : IDBRow
    {
        [IdentityDBField]
        public long? keyid { get; set; }
        public string type_name { get; set; }
        public string type_price { get; set; }
        public string type_date1 { get; set; }
        public string type_date2 { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_CommonType;
        }
    }
}