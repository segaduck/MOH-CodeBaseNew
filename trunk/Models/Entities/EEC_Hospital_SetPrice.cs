using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_Hospital_SetPrice : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }

        /// <summary>醫院代號</summary>
        public string hospital_code { get; set; }

        /// <summary>醫院名稱</summary>
        public string hospital_name { get; set; }

        /// <summary>病歷類型代號</summary>
        public string his_type { get; set; }

        /// <summary>病歷類型名稱</summary>
        public string his_type_name { get; set; }

        /// <summary>病歷類型單價</summary>
        public int? price { get; set; }

        /// <summary>開放申請期間(起)</summary>
        public string show_date1 { get; set; }

        /// <summary>開放申請期間(迄)</summary>
        public string show_date2 { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_Hospital_SetPrice;
        }
    }
}