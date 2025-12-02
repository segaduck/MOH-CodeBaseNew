using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models
{
    public class SAMPLE_PRODUCTModel
    {
        public class Req
        {
            public string SAMPLE_RDOCUT_NO { get; set; }
        }

        public class Res
        {
            public string E_APPID { get; set; }
            public string SAMPLE_RDOCUT_NO { get; set; }
            public double SAMPLE_ITEM { get; set; }
            public string PRODUCT_CH_NAME { get; set; }
            public string PRODUCT_EN_NAME { get; set; }
            public string SAMPLE_PORC_TYPE_CODE { get; set; }
            public string SAMPLE_DRUG_FORM { get; set; }
            public double SAMPLE_QTY { get; set; }
            public string SAMPLE_QTY_UNIT_CODE { get; set; }
            public double SAMPLE_UNIT_QTY { get; set; }
            public string SAMPLE_UNIT_UNIT_CODE { get; set; }

            public string Description { get; set; }
        }


    }
}