using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.ChineseMedicineAPI
{
    /// <summary>
    /// 產銷證明、外銷明書、WHO格式之產銷證明書介接
    /// </summary>
    public class SALEAPI
    {
        public SALEAPI()
        {
            this.SALEResp = new SALEAPI_Resp();
        }

        /// <summary>
        /// 產銷證明、外銷明書、WHO格式之產銷證明書
        /// </summary>
        public SALEAPI_Resp SALEResp { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string STATUS { get; set; }
    }

    /// <summary>
    /// 產銷證明、外銷明書、WHO格式之產銷證明書
    /// </summary>
    public class SALEAPI_Resp
    {
        /// <summary>
        /// 線上申辦   --->  製造廠名稱
        /// 許可證系統 --->  名稱
        /// </summary>
        public string 製造廠名稱 { get; set; }

        /// <summary>
        /// 線上申辦   --->  製造廠名稱
        /// 許可證系統 --->  名稱
        /// </summary>
        public string 製造廠地址 { get; set; }

        /// <summary>
        /// 線上申辦   --->  中文品名
        /// 許可證系統 --->  中文品名
        /// </summary>
        public string 中文品名 { get; set; }

        /// <summary>
        /// 線上申辦   --->  英文品名
        /// 許可證系統 --->  英文品名
        /// </summary>
        public string 英文品名 { get; set; }

        /// <summary>
        /// 線上申辦   --->  劑型
        /// 許可證系統 --->  藥品劑型
        /// </summary>
        public string 藥品劑型 { get; set; }

        /// <summary>
        /// 線上申辦   --->  核准日期
        /// 許可證系統 --->  發證日期
        /// </summary>
        public string 發證日期 { get; set; }

        /// <summary>
        /// 線上申辦   --->  有效日期
        /// 許可證系統 --->  有效日期
        /// </summary>
        public string 有效日期 { get; set; }

        /// <summary>
        /// 線上申辦   --->  處方說明
        /// 許可證系統 --->  處方說明
        /// </summary>
        public string 處方說明 { get; set; }

        /// <summary>
        /// 線上申辦   --->  效能
        /// 許可證系統 --->  效能
        /// </summary>
        public string 效能 { get; set; }

        /// <summary>
        /// 線上申辦   --->  適應症
        /// 許可證系統 --->  適應症
        /// </summary>
        public string 適應症 { get; set; }

        /// <summary>
        /// 線上申辦   --->  限制1
        /// 許可證系統 --->  限制1
        /// </summary>
        public string 限制1 { get; set; }

        /// <summary>
        /// 線上申辦   --->  限制2
        /// 許可證系統 --->  限制2
        /// </summary>
        public string 限制2 { get; set; }

        /// <summary>
        /// 線上申辦   --->  限制3
        /// 許可證系統 --->  限制3
        /// </summary>
        public string 限制3 { get; set; }

        /// <summary>
        /// 線上申辦   --->  限制4
        /// 許可證系統 --->  限制4
        /// </summary>
        public string 限制4 { get; set; }

        /// <summary>
        /// 外銷品名
        /// </summary>
        public List<SALEAPI_DRUGNAME> 外銷品名 { get; set; }

        /// <summary>
        /// 成分說明
        /// </summary>
        public List<SALEAPI_DI> 成分說明 { get; set; }
    }

    /// <summary>
    /// 外銷品名
    /// </summary>
    public class SALEAPI_DRUGNAME
    {
        /// <summary>
        /// 線上申辦   --->  外銷中文品名
        /// 許可證系統 --->  外銷中文品名
        /// </summary>
        public string 外銷中文品名 { get; set; }

        /// <summary>
        /// 線上申辦   --->  外銷英文品名
        /// 許可證系統 --->  外銷英文品名
        /// </summary>
        public string 外銷英文品名 { get; set; }
    }

    /// <summary>
    /// 成分說明
    /// </summary>
    public class SALEAPI_DI
    {
        /// <summary>
        /// 線上申辦   --->  成分內容
        /// 許可證系統 --->  成分內容
        /// </summary>
        public string 成分內容 { get; set; }

        /// <summary>
        /// 線上申辦   --->  份量
        /// 許可證系統 --->  份量
        /// </summary>
        public string 份量 { get; set; }

        /// <summary>
        /// 線上申辦   --->  單位
        /// 許可證系統 --->  單位
        /// </summary>
        public string 單位 { get; set; }
    }
}