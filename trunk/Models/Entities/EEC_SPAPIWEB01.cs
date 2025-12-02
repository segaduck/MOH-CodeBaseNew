using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class EEC_SPAPIWEB01 : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? SPAPIWEB01ID { get; set; }

        public string transaction_id { get; set; }
        public string op_code { get; set; }
        public string sp_service_id { get; set; }
        public string sp_checksum { get; set; }
        public string hint { get; set; }

        //[NotDBField] public string id_num { get; set; }
        //[NotDBField] public string op_mode { get; set; }
        //[NotDBField] public string sign_info { get; set; }

        public string sign_type { get; set; }
        public string sign_data { get; set; }
        public string tbs_encoding { get; set; }
        public string hash_algorithm { get; set; }

        public int? time_limit { get; set; }

        public string IDNO { get; set; }
        public string BIRTHDAY { get; set; }

        public DateTime? CREATEDATE { get; set; }
        public DateTime? UPDATADATE { get; set; }
        /// <summary>
        /// 1:申請/2:查詢
        /// </summary>
        public string fType { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_SPAPIWEB01;
        }
    }
}