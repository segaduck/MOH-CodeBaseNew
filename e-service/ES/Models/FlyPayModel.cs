using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CTCB.Crypto;
using CTCBAPI;
using System.Web.Configuration;
using System.Web.Mvc;

namespace ES.Models
{
    public class FlyPayModel
    {
        public FlyPayModel()
        {
            this.enc = new Encrypt();
            this.uc = new UnionPayMac();
            this.paylist = new List<UserInfoPayModel>();
            this.es = new EsunACQModel();
        }
        public bool isEsun { get; set; }
        public Encrypt enc { get; set; }
        //public Decrypt dec { get; set; }
        public UnionPayMac uc { get; set; }

        public List<UserInfoPayModel> paylist { get; set; }

        public UserInfoPayBasicModel payBasic { get; set; }
        public Int32? amountall { get; set; }

        public string crdtype { get; set; }
        public string language { get; set; }
        public string ErrMsg { get; set; }
        /// <summary>
        /// 繳費成功日期
        /// </summary>
        public string succDate { get; set; }
        /// <summary>
        /// 銀行回傳值 xid
        /// </summary>
        public string payOrder { get; set; }
        /// <summary>
        /// post用
        /// </summary>
        public string UrlmerId { get; set; }
        /// <summary>
        /// 授權碼-traceNumber
        /// </summary>
        public string authCode { get; set; }

        /// <summary>
        /// 訂房編號
        /// </summary>
        public string Guid { get; set; }

        public string IsWebTestEnvir1
        {
            get
            {
                return WebConfigurationManager.AppSettings["WebTestEnvir"];
            }
        }


        public string ValidateCode { get; set; }

        /// <summary>
        /// 專案驗證碼
        /// </summary>
        public string flytype { get; set; }
        /// <summary>
        /// 玉山model
        /// </summary>
        public EsunACQModel es { get; set; }

    }
    /// <summary>
    /// eservice繳費輸入Model
    /// </summary>
    public class UserInfoPayModel
    {
        public string mainno { get; set; }
        public string birth { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }
    }
    /// <summary>
    /// 單一護照號碼取得所有航班繳費資訊
    /// </summary>
    public class FlightModel
    {
        public string mainno { get; set; }
        public DateTime? flightDate { get; set; }
        public string flightNo { get; set; }
        public string status { get; set; }
    }

    /// <summary>
    /// 防疫旅館進入1/12-1/26限1500名額
    /// </summary>
    public class UserInfoPayBasicModel
    {
        public string isUsing { get; set; }
        public string mainno { get; set; }
        public string birth { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }
        /// <summary>
        /// 抵達日期
        /// </summary>
        public string ArDate { get; set; }
        /// <summary>
        /// 驗證碼
        /// </summary>
        public string RAN { get; set; }
        /// <summary>
        /// 出境國家
        /// </summary>
        public string dirct { get; set; }
        /// <summary>
        /// 轉機國家
        /// </summary>
        public string dirct_tr { get; set; }

        /// <summary>
        /// 轉機國家(複製用)
        /// </summary>
        public string dirct_tr_copy { get; set; }


        /// <summary>
        /// 轉機國家(新增項目)
        /// </summary>
        public List<string> dirct_tr_list { get; set; }
        public IList<SelectListItem> dirct_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.FlyPayCountryList;
            }
        }
        /// <summary>
        /// 春節專案 選擇地區
        /// </summary>
        public string section { get; set; }
        public IList<SelectListItem> section_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.SprSection_list;
            }
        }
        public string sprlevel { get; set; }

        /// <summary>
        /// 居住天數 春節專案(10,14)
        /// </summary>
        public string livedays { get; set; }
    }

    public class EsunACQModel
    {
        public EsunACQModel()
        {

        }
        // 系統自用
        public string postUrl { get; set; }
        // JSON傳送
        public string data { get; set; }
        // 壓碼值 SHA-256(data + MacKey)
        // SHA-256({"MID":"8089000016"}WEGSC0Q7BAJGTQYL8BV8KRQRZXH6VK0B)
        // 單筆查詢 post https://acqtest.esunbank.com.tw/ACQQuery/esuncard/txnf0180
        // 取消授權 post https://acqtest.esunbank.com.tw/ACQTrans/esuncard/txnf0150
        // type4 請求授權 post PC https://acqtest.esunbank.com.tw/ACQTrans/esuncard/txnf014s
        // type4 請求授權 post mobile https://acqtest.esunbank.com.tw/ACQTrans/esuncard/txnf014m
        // 沖正交易 請求授權 post https://acqtest.esunbank.com.tw/ACQTrans/esuncard/txnf0160

        public string mac { get; set; }
        // MacKey序號，固定1
        public string ksn { get; set; }
        // 特店代碼
        public string MID { get; set; }
        // 訂單編號
        public string ONO { get; set; }

        // type4授權
        // 終端機代碼
        public string TID { get; set; }
        // 交易金額
        public string TA { get; set; }
        // 回覆位址 url 不可包含#?&字元
        public string U { get; set; }
        // 分期代碼
        public string IC { get; set; }
        // 銀行紅利折抵 Y/N
        public string BPF { get; set; }

        // 沖正交易
        // 交易類別 05:授權, 51:取消授權, 71:退貨授權
        public string TYP { get; set; }
        // 專案資訊 1. 依「專案資訊代號」定義存放 2. 明文內容為120位元長
        public string INFO { get; set; }

        // 銀聯授權
        // 子特店代號 空白
        public string CID { get; set; }
        // 01 消費 04 退貨 31 取消(限消費當日23:00前交易) 00 查詢
        public string TT { get; set; }
        //1.【01、04、31】交易 : 請放空白。 2.【00】:若有值，則依該序號查詢; 若為空值，則查詢該筆「訂單編號」最後新狀態之交易。
        public string TXNNO { get; set; }
        // 由【輸入字串】與【押碼KEY】(銀行端提供)所產生。(供銀行端驗證交易資料)
        public string M { get; set; }

    }

    public class FlyPayEchelonViewModel
    {
        public FlyPayEchelonViewModel()
        {

        }
        public string echelon { get; set; }
        public string section { get; set; }
        public string daycode { get; set; }
        public string room { get; set; }
        public string dayval { get; set; }
        public string se_code { get; set; }
        public string rooms { get; set; }
        public string sec { get; set; }
    }
    public class FlyPaySPRViewModel
    {
        public FlyPaySPRViewModel()
        {

        }
        public string SPRCODE { get; set; }
        public int? FLY_ID { get; set; }
        public string ISUSE { get; set; }
        public string STATUS { get; set; }
        public string FLIGHTDATE { get; set; }
        public int? CD_ID { get; set; }
        public string ECHELON { get; set; }
        public string SECTION { get; set; }
        public string DAYCODE { get; set; }
        public string ROOM { get; set; }
        public string DAYVAL { get; set; }
    }
    //public class type4ReturnModel
    //{
    //    public string RC { get; set; }
    //    // 特店代碼
    //    public string MID { get; set; }
    //    // 訂單編號
    //    public string ONO { get; set; }
    //    // 收單交易日期
    //    public string LTD { get; set; }
    //    // 收單交易時間
    //    public string LTT { get; set; }
    //    // 簽帳單序號
    //    public string RRN { get; set; }
    //    // 授權碼
    //    public string AIR { get; set; }
    //    // 卡號(前六後四)
    //    public string AN { get; set; }
    //}
    //public class EsunACQReturnModel
    //{
    //    // post back json { returnCode, txnData, version = 2}
    //    public EsunACQReturnModel()
    //    {

    //    }
    //    // 回覆碼
    //    public string RC { get; set; }
    //    // 特店代碼
    //    public string MID { get; set; }
    //    // 訂單編號
    //    public string ONO { get; set; }
    //    // 收單交易日期
    //    public string LTD { get; set; }
    //    // 收單交易時間
    //    public string LTT { get; set; }
    //    // 簽帳單序號
    //    public string RRN { get; set; }
    //    // 授權碼
    //    public string AIR { get; set; }
    //    // 交易金額
    //    public string TXNAMOUNT { get; set; }
    //    // 剩餘消費金額
    //    public string SETTLEAMOUNT { get; set; }
    //    // 訂單狀態
    //    /// <summary>
    //    /// 10:授權中,11:授權失敗,19:授權成功(可請款),40:授權取消中,
    //    /// 41:授權取消失敗(可請款),49:授權取消成功,50:請款中
    //    /// 51:請款失敗(可請款),59:請款成功(可退貨),60:退貨中
    //    /// 61:退貨失敗(可退貨),69:退貨成功,70:退貨授權中
    //    /// 71:退貨授權失敗,79:退貨授權成功
    //    /// </summary>
    //    public string SETTLESTATUS { get; set; }
    //    // 卡號(前六後四)
    //    public string AN { get; set; }
    //    // 分期付款資料欄位 若有值才會回覆此欄位
    //    // 分期總金額
    //    public string ITA { get; set; }
    //    // 分期期數
    //    public string IP { get; set; }
    //    // 每期金額
    //    public string IPA { get; set; }
    //    // 頭期款金額
    //    public string IFPA { get; set; }
    //    // 銀行紅利藍衛 若有值才會回覆此欄位
    //    // 剩餘點數
    //    public string BB { get; set; }
    //    // 折抵點數
    //    public string BRP { get; set; }
    //    // 折抵金額
    //    public string BRA { get; set; }
    //}
}