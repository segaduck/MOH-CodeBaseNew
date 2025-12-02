using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblAMUROLE : IDBRow
    {
        [IdentityDBField]
        public int? role_id { get; set; }

        public string userno { get; set; }

        public int? grp_id { get; set; }

        public string moduser { get; set; }

        public string modusername { get; set; }

        public string modtime { get; set; }

        public string modip { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.AMUROLE;
        }
    }
}