using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Models.Entities
{
    public class Apply_005002_DOSAGE_FORM
    {
        public string APPLY_ID { get; set; }

        public int DF_ID { get; set; }

        /// <summary>
        /// 外銷品名
        /// </summary>
        public string DOSAGE_FROM { get; set; }

        /// <summary>
        /// 外銷品名 英文
        /// </summary>
        public string DOSAGE_FROM_E { get; set; }

        public int SORT { get; set; }

        public string DEL_MK { get; set; }

        public DateTime? DEL_TIME { get; set; }

        public string DEL_FUNC_CD { get; set; }

        public DateTime? UPD_TIME { get; set; }

        public string UPD_FUN_CD { get; set; }

        public string UPD_ACC { get; set; }

        public DateTime? ADD_TIME { get; set; }

        public string ADD_FUN_CD { get; set; }

        public string ADD_ACC { get; set; }

    }
}