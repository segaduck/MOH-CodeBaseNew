using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_Hospital_Api : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }

        /// <summary>醫院代號</summary>
        public string hospital_code { get; set; }

        /// <summary>醫院名稱</summary>
        public string hospital_name { get; set; }

        /// <summary>醫院的 API 的 別名</summary>
        public string hospital_apikey { get; set; }

        /// <summary>醫院的 API Domain Name</summary>
        public string hospital_domain { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_Hospital_Api;
        }
    }
}