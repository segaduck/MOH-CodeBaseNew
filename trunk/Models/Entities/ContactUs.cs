using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblContactUs : IDBRow
    {
        [IdentityDBField]
        public long? keyid { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string Created { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.ContactUs;
        }
    }
}