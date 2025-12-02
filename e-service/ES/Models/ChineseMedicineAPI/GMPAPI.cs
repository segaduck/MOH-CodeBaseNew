using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.ChineseMedicineAPI
{
    /// <summary>
    /// GMP API介接
    /// </summary>
    public class GMPAPI
    {
        public GMPAPI()
        {
            this.GMPResp = new GMPAPI_Resp();
        }

        /// <summary>
        /// GMP證明書
        /// </summary>
        public GMPAPI_Resp GMPResp { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string STATUS { get; set; } 
    }

    /// <summary>
    /// GMP證明書
    /// </summary>
    public class GMPAPI_Resp
    {
        /// <summary>
        /// 線上申辦   --->  製造廠名稱
        /// 許可證系統 --->  名稱
        /// </summary>
        public string 名稱 { get; set; }

        /// <summary>
        /// 線上申辦   --->  藥商許可執照號碼
        /// 許可證系統 --->  醫事機構代碼
        /// </summary>
        public string 醫事機構代碼 { get; set; }

        /// <summary>
        /// 線上申辦   --->  製造廠郵遞區號
        /// 許可證系統 --->  郵遞區號
        /// </summary>
        public string 郵遞區號 { get; set; }

        /// <summary>
        /// 線上申辦   --->  製造廠地址
        /// 許可證系統 --->  營業地址
        /// </summary>
        public string 營業地址 { get; set; }

        /// <summary>
        /// 線上申辦   --->  負責人姓名
        /// 許可證系統 --->  負責人
        /// </summary>
        public string 負責人 { get; set; }

        /// <summary>
        /// 線上申辦   --->  最近一次GMP查廠日期
        /// 許可證系統 --->  查廠日期
        /// </summary>
        public string 查廠日期 { get; set; }

        /// <summary>
        /// 線上申辦   --->  GMP有效期限
        /// 許可證系統 --->  有效期限    
        /// </summary>
        public string 有效期限 { get; set; }
    }
}