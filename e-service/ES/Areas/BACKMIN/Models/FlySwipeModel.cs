using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class FlySwipeModel
    {
        public FlySwipeModel()
        {
            this.detail = new ExpFlySwipeModel();
            this.es = new EsunSearchModel();
            this.esu = new EsunUSearchModel();
        }

        /// 授權碼
        /// </summary>
        [Display(Name = "授權碼")]
        public virtual string QRY_TRACENO { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        [Display(Name = "交易日期")]
        public virtual DateTime? QRY_PAYDATE { get; set; }

        public List<FlySwipeSearchGridModel> grids { get; set; }

        public ExpFlySwipeModel detail { get; set; }

        /// <summary>
        /// 訂單編號
        /// </summary>
        [Display(Name ="訂單編號")]
        public string ONO { get; set; }
        public EsunSearchModel es { get; set; }
        public EsunUSearchModel esu { get; set; }
    }

    public class FlySwipeSearchGridModel : ExpFlySwipeModel { }

    public class ExpFlySwipeModel : ImpFlySwipeModel { }

    /// <summary>
    /// 匯入資料-FlySwipe
    /// </summary>
    public class ImpFlySwipeModel
    {
        /// <summary>
        /// 商店代號
        /// </summary>
        [Display(Name = "商店代號")]
        public string STORECODE { get; set; }
        /// <summary>
        /// 授權碼
        /// </summary>
        [Display(Name = "授權碼")]
        public string TRACENO { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        [Display(Name = "交易日期")]
        public DateTime? PAYDATE { get; set; }
        /// <summary>
        /// 交易金額
        /// </summary>
        [Display(Name = "交易金額")]
        public string PAYMONEY { get; set; }
        /// <summary>
        /// 帳務日期
        /// </summary>
        [Display(Name = "帳務日期")]
        public DateTime? BILLINGDATE { get; set; }
        /// <summary>
        /// 撥款日期
        /// </summary>
        [Display(Name = "撥款日期")]
        public DateTime? GRANTDATE { get; set; }
        public string DEL_MK { get; set; }
        public DateTime? DEL_TIME { get; set; }
        public string DEL_FUN_CD { get; set; }
        public string DEL_ACC { get; set; }
        public DateTime? UPD_TIME { get; set; }
        public string UPD_FUN_CD { get; set; }
        public string UPD_ACC { get; set; }
        public DateTime? ADD_TIME { get; set; }
        public string ADD_FUN_CD { get; set; }
        public string ADD_ACC { get; set; }

        /// <summary>
        /// 銀聯卡追蹤碼
        /// </summary>
        [Display(Name = "銀聯卡追蹤碼")]
        public string TRACENO_QID { get; set; }
    }

    public class EsunSearchModel
    {
        public EsunSearchModel()
        {

        }
        public string postUrl { get; set; }
        public string data { get; set; }
        public string mac { get; set; }
        public string ksn { get; set; }
    }
  
    public class EsunUSearchModel
    {
        public EsunUSearchModel()
        {

        }
        public string postUrl { get; set; }
        public string MID { get; set; }
        public string CID { get; set; }
        public string ONO { get; set; }
        public string TA { get; set; }
        public string TT { get; set; }
        public string U { get; set; }
        public string TXNNO { get; set; }
        public string M { get; set; }

    }
}
