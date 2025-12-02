using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Models.Entities
{
    public class Apply_005014_FILE_Log
    {
        public string APP_ID { get; set; }
        public string FILE_ID { get; set; }
        public string MIME { get; set; }
        public string FILE_URL { get; set; }
        public string MODTIME { get; set; }
        public string MODUSER { get; set; }
        public string MODTYPE { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string DEL_MK { get; set; }
        public string SRV_ID { get; set; }
        public string FILE_NAME { get; set; }
    }
}