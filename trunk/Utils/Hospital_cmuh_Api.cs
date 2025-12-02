using System;
using System.Collections.Generic;
using Turbo.Commons;
using EECOnline.Services;
using log4net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.IO;
using System.Collections;
using Omu.ValueInjecter;
using System.Linq;

namespace EECOnline.Utils
{
    /// <summary>
    /// 中國醫藥<br/>
    /// 備註：
    /// </summary>
    public class Hospital_cmuh_Api
    {
        private static readonly ILog LOG = LogUtils.GetLogger();

        public class Api_A2_2_ParamsModel
        {
            public string token { get; set; }
            public string caseNo { get; set; }
            public IList<Api_A2_2_ParamsSubModel> data { get; set; }
        }

        public class Api_A2_2_ParamsSubModel
        {
            public string ec_no { get; set; }
            public string ec_name { get; set; }
            public string ec_price { get; set; }
            public string ec_date { get; set; }
            public string ec_success { get; set; }
            public string ec_reason { get; set; }

            public string ec_fileBase64 { get; set; }
        }

        public class Api_A2_2_ResultModel
        {
            public string ec_no { get; set; }
            public string ec_name { get; set; }
            public string ec_price { get; set; }
            public string ec_date { get; set; }
            public string ec_success { get; set; }
            public string ec_reason { get; set; }
        }
    }
}