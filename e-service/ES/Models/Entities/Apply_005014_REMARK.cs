using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Models.Entities
{
    /// <summary>
    /// 貨品進口專案申請資料表-申請項目
    /// </summary>
    public class Apply_005014_REMARK
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public string APP_ID { get; set; }
        
        public string DEL_MK { get; set; }
        public string DEL_TIME { get; set; }
        public string DEL_FUN_CD { get; set; }
        public string DEL_ACC { get; set; }
        public string UPD_TIME { get; set; }
        public string UPD_FUN_CD { get; set; }
        public string UPD_ACC { get; set; }
        public string ADD_TIME { get; set; }
        public string ADD_FUN_CD { get; set; }
        public string ADD_ACC { get; set; }

        /// <summary>
        /// 備註1  Y|N
        /// </summary>
        public string REMARK1 { get; set; }

        /// <summary>
        /// 備註1_ITEM1_COMMENT  
        /// </summary>
        public string REMARK1_ITEM1_COMMENT { get; set; }

        /// <summary>
        /// 備註1_item2 中藥用 | 非中藥材用途 1,2 
        /// </summary>
        public string REMARK1_ITEM2 { get; set; }

        /// <summary>
        /// 備註1_item2_COMMENT  
        /// </summary>
        public string REMARK1_ITEM2_COMMENT { get; set; }

        /// <summary>
        /// 備註1 補正
        /// </summary>
        public string REMARK1_E { get; set; }

        /// <summary>
        /// 備註2 bool Y|N
        /// </summary>
        public string REMARK2 { get; set; }

        /// <summary>
        /// 備註2 補正
        /// </summary>
        public string REMARK2_E { get; set; }

        /// <summary>
        /// 備註3_1 bool Y|N
        /// </summary>
        public string REMARK3_1 { get; set; }

        /// <summary>
        /// 備註3_2 bool
        /// </summary>
        public string REMARK3_2 { get; set; }

        /// <summary>
        /// 備註3_2_COMMENT
        /// </summary>
        public string REMARK3_2_COMMENT { get; set; }

        /// <summary>
        /// 備註3_3 bool
        /// </summary>
        public string REMARK3_3 { get; set; }

        /// <summary>
        /// 備註3_3_comment
        /// </summary>
        public string REMARK3_3_COMMENT { get; set; }

        /// <summary>
        /// 備註3_4 bool
        /// </summary>
        public string REMARK3_4 { get; set; }

        /// <summary>
        /// 備註3_4_comment
        /// </summary>
        public string REMARK3_4_COMMENT { get; set; }

        /// <summary>
        /// 備註3_5 bool
        /// </summary>
        public string REMARK3_5 { get; set; }

        /// <summary>
        /// 備註3_5_comment
        /// </summary>
        public string REMARK3_5_COMMENT { get; set; }

        /// <summary>
        /// 備註3 備正
        /// </summary>
        public string REMARK3_E { get; set; }

    }
}