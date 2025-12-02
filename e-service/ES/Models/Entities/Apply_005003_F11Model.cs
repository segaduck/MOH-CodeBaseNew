using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// WHO格式之產銷證明書(英文) 申請品項-1.勾選藥品名稱
    /// </summary>
    public class Apply_005003_F11Model
    {
        /// <summary>
        /// 案件號碼
        /// </summary>
        //[Required]
        public string APP_ID { get; set; }
        /// <summary>
        /// 同案件號碼-序號
        /// </summary>
        //[Required]
        public int? SRL_NO { get; set; }
        /// <summary>
        /// 生藥名
        /// </summary>
        //[Required]
        public string F11_SCI_NM { get; set; }
        /// <summary>
        /// 成分內容(中文)
        /// </summary>
        public string F11_SCI_NAME { get; set; }
        /// <summary>
        /// 數量-份量
        /// </summary>
        public string F11_QUANTITY { get; set; }
        /// <summary>
        /// 數量-單位
        /// </summary>
        public string F11_UNIT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DEL_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_ACC { get; set; }

    }
}