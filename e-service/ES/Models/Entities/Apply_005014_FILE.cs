using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Models.Entities
{
    public class Apply_005014_FILE
    {
        public string APP_ID { get; set; }

        public string SRV_ID { get; set; }

        public string FILE_ID { get; set; }

        public string FILE_NAME { get; set; }

        public string FILE_URL { get; set; }

        public string MIME { get; set; }

        public DateTime? CREATE_DATE { get; set; }

        public string DEL_MK { get; set; }

        public string FILE_E { get; set; }

    }

    public class Apply_005014_FILExt : Apply_005014_FILE
    {
        public System.IO.Stream Stream { get; set; }
        public string Base64 { get; set; }
        public HttpPostedFileBase PostFile { get; set; }
    }

}