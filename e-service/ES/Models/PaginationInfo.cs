using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Omu.ValueInjecter;

namespace ES.Models
{
    public class PaginationInfo
    {
        public virtual int NowPage { get; set; }
        public int TotalPage { get; set; }
        public int TotalCount { get; set; }

    }
}