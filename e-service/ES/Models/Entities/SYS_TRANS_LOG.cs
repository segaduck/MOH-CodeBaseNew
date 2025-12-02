using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 歷程TABLE1
    /// </summary>
    public class SYS_TRANS_LOG
    {
        /// <summary>
        /// 序號
        /// </summary>
        public int? SEQ { get; set; }
        /// <summary>
        ///  GETDATE() 2018-07-23 12:37:32.000
        /// </summary>
        public string TRANSTIME { get; set; }
        /// <summary>
        /// 案號-案件編號 
        /// </summary>
        public string APP_ID { get; set; } // varchar 20     
        /// <summary>
        /// 申請項目代碼
        /// </summary>
        public string SRV_ID { get; set; } // varchar 6     
        /// <summary>
        /// TRANSTYPE: Insert/Update/Delete
        /// </summary>
        public string TRANSTYPE { get; set; } // varchar 10 允許
        /// <summary>
        /// 目標TABLE
        /// </summary>
        public string TARGETTABLE { get; set; } // varchar 30 允許
        /// <summary>
        /// WHERE 條件
        /// </summary>
        public string CONDITIONS { get; set; } // varchar 2000 允許
        /// <summary>
        /// 異動後欄位值 GID=848, GDISTID=001, GTYPE=1, GNAME=TTT5
        /// </summary>
        public string BEFOREVALUES { get; set; } // varchar -1 允許
        /// <summary>
        /// 異動後欄位值 GID=848, GDISTID=001, GTYPE=1, GNAME=TTT5
        /// </summary>
        public string AFTERVALUES { get; set; } // varchar -1 允許

        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string COLUMNAME { get; set; }

        /// <summary>
        /// 欄位名稱 中文
        /// </summary>
        public string COLUMNAMEC { get; set; }


        public DateTime? UPD_TIME { get; set; }

        public string UPD_FUN_CD { get; set; }

        /// <summary>
        /// 新增時間
        /// </summary>
        public DateTime? ADD_TIME { get; set; } // datetime
        /// <summary>
        /// 登入群組
        /// </summary>
        public string ADD_FUN_CD { get; set; } // varchar 20     
        /// <summary>
        /// 登入者 
        /// </summary>
        public string ADD_ACC { get; set; } // varchar 30 允許  

        public string MODTIME { get; set; }

        public string MODUSER { get; set; }

        public string MODTYPE { get; set; }
    }
}