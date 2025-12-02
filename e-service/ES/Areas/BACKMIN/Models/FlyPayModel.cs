using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class FlyPayModel
    {
        public FlyPayModel()
        {
            this.detail = new ExportModel();
        }
        
        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        [Display(Name = "航班日期/抵達日期")]
        public virtual DateTime? QRY_FLIGHTDATE { get; set; }
        public virtual DateTime? QRY_FLIGHTDATE_END { get; set; }
        [Display(Name = "航班號碼")]
        public virtual string QRY_FLIGHTNO { get; set; }
        [Display(Name = "證件號碼")]
        public virtual string QRY_MAINDOCNO { get; set; }
        [Display(Name = "防疫所別")]
        public virtual string QRY_PLACE { get; set; }
        [Display(Name = "繳費編號")]
        public virtual string QRY_PAYRESULT { get; set; }
        [Display(Name = "繳費狀態")]
        public virtual string QRY_STATUS { get; set; }
        [Display(Name = "逾時註記")]
        public virtual string QRY_NEEDBACK { get; set; }
        /// <summary>
        /// 授權碼
        /// </summary>
        [Display(Name = "授權碼")]
        public virtual string QRY_TRACENO { get; set; }
        /// <summary>
        /// 撥款日期區間
        /// </summary>
        [Display(Name = "撥款日期區間")]
        public virtual DateTime? QRY_GRANTDATE1 { get; set; }
        /// <summary>
        /// 撥款日期
        /// </summary>
        [Display(Name = "迄止撥款日期")]
        public virtual DateTime? QRY_GRANTDATE2 { get; set; }
        [Display(Name = "新增日期起")]
        public virtual DateTime? QRY_ADDTIMES { get; set; }
        [Display(Name = "新增日期迄")]
        public virtual DateTime? QRY_ADDTIMEE { get; set; }
        public List<FlySearchGridModel> grids { get; set; }

        /// <summary>
        /// 繳費狀態下拉清單
        /// </summary>
        public List<Map> QRY_PAY_STATUSList { get; set; }
        /// <summary>
        /// 是否需要退費
        /// </summary>
        public List<Map> QRY_NEED_BACKList { get; set; }
        public ExportModel detail { get; set; }

        public virtual bool QRY_CanEdit { get; set; }
        /// <summary>
        /// 1:菲律賓專案 flypay; 2:防疫旅館專案 flypaybasic
        /// </summary>
        public string ProjectType { get; set; }

        /// <summary>
        /// 出境國家
        /// </summary>
        [Display(Name = "出境國家")]
        public virtual string QRY_FLYCONTRY { get; set; }

        /// <summary>
        /// null 中信 ; 2 玉山 銀行
        /// </summary>
        [Display(Name = "銀行別")]
        public virtual string QRY_BANKTYPE { get; set; }
        /// <summary>
        /// 銀行別下拉清單
        /// </summary>
        public List<Map> QRY_BANKTYPEList { get; set; }

        /// <summary>
        /// 春節專案流水號
        /// </summary>
        [Display(Name = "春節專案流水號")]
        public virtual string QRY_SPRCODE { get; set; }

        /// <summary>
        /// 識別碼
        /// </summary>
        [Display(Name = "訂房/繳費成功識別碼")]
        public virtual string QRY_GUID { get; set; }

        /// <summary>
        /// 1 北 ; 2 中; 3 南
        /// </summary>
        [Display(Name = "地區(春節專案)")]
        public virtual string QRY_SECTION { get; set; }
        /// <summary>
        /// 地區(春節專案)下拉清單
        /// </summary>
        public List<Map> QRY_SECTIONList { get; set; }

        /// <summary>
        /// 價錢(春節專案)
        /// </summary>
        [Display(Name = "價錢(春節專案)")]
        public virtual string QRY_PLEVEL { get; set; }
    }

    public class FlySearchGridModel : ExportModel
    {
    }

    public class ImportModel
    {
        // FLYPAYBASIC 專用欄位
        public string FLY_ID { get; set; }
        // FLYPAYBASIC 專用欄位
        public string GUID { get; set; }

        public string FIRSTNAME { get; set; }
        public string MIDDLENAME { get; set; }
        public string LASTNAME { get; set; }
        public string BIRTH { get; set; }
        public string GENDER { get; set; }
        public string MAINDOCNO { get; set; }
        public string NATIONALITY { get; set; }
        public string FLIGHTDATE { get; set; }
        public string FLIGHTNO { get; set; }
        public string DEPARTAIRPORT { get; set; }
        public string PASSENGERTYPE { get; set; }

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
        // 出境國家
        public string FLYCONTRY { get; set; }
        // 轉機國家
        public string FLYCONTRYTR { get; set; }
        // 春節專案 地區 北1 中2 南3
        public string SECTION { get; set; }
        // 春節專案 價錢 2000 A 1500 B
        public string PLEVEL { get; set; }
        // 春節專案流水號
        public string SPRCODE { get; set; }

        public string FIRSTNAME2 { get; set; }
        public string MIDDLENAME2 { get; set; }
        public string LASTNAME2 { get; set; }
        public string BIRTH2 { get; set; }
        public string MAINDOCNO2 { get; set; }
        public string FIRSTNAME3 { get; set; }
        public string MIDDLENAME3 { get; set; }
        public string LASTNAME3 { get; set; }
        public string BIRTH3 { get; set; }
        public string MAINDOCNO3 { get; set; }
        public string FIRSTNAME4 { get; set; }
        public string MIDDLENAME4 { get; set; }
        public string LASTNAME4 { get; set; }
        public string BIRTH4 { get; set; }
        public string MAINDOCNO4 { get; set; }
    }

    public class ExportModel : ImportModel
    {
        /// <summary>
        /// 交易日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [Display(Name = "刷卡交易日期")]
        public DateTime? PAYDATE { get; set; }
        public string PAYMONEY { get; set; }
        public string PAYRESULT { get; set; }
        public string PLACE { get; set; }
        public string ISPAY { get; set; }
        public string STATUS { get; set; }
        public string PAYRETURN { get; set; }
        public string XID { get; set; }
        /// <summary>
        /// 授權碼
        /// </summary>
        public string TRACENO { get; set; }
        public string QID { get; set; }
        public string AUTHCODE { get; set; }
        /// <summary>
        /// 撥款日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? GRANTDATE { get; set; }

        public string BANKTYPE_TEXT { get; set; }
        public string SPRCODE_TEXT { get; set; }
        public string SECTION_TEXT { get; set; }
        /// <summary>
        /// 新增時間
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
        public DateTime? ADDTIMEG { get; set; }

    }
}
