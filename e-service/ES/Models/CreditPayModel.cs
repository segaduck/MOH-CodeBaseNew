using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    public class CreditPayModel
    {
        public CreditPayModel()
        {
            this.EncModel = new CTCB_EncryptModel();
            this.DecModel = new CTCB_DecryptModel();
        }
        public CTCB_EncryptModel EncModel { get; set; }
        public CTCB_DecryptModel DecModel { get; set; }
    }

    /// <summary>
    /// ctcbMac.dll 加解密元件
    /// CTCB.Crypto.Encrypt 產生刷卡機的輸入資料壓碼
    /// </summary>
    public class CTCB_EncryptModel
    {
        /// <summary>
        /// 銀行所授予的特店代號，純數字，固定13碼
        /// </summary>
        public string MerchantID { get; set; }
        /// <summary>
        /// 銀行所授予的終端機代號，純數字，固定8碼
        /// </summary>
        public string TerminalID { get; set; }
        /// <summary>
        /// 為電子商場的應用程式所給予此筆交易的訂單編號，資料型態為最長 19 個字元的文字串。 
        /// 訂單編號字串之字元僅接受一般英文字母、數字及底線’ ’__’的組合，不可出現其餘符號字元。
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 為消費者此筆交易所購買商品欲授權總金額，正整數格式的字串
        /// </summary>
        public string AuthAmt { get; set; }
        /// <summary>
        /// 交易方式Enum{一般交易:0,分期交易:1,紅利折抵一般交易:2,紅利折抵分期交易:4}
        /// </summary>
        public string txType { get; set; }
        /// <summary>
        /// 此為貴特店在URL 帳務管理後台登錄的壓碼字串。
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 特店所要顯示的商店名稱，中文請填BIG5碼
        /// </summary>
        public string MerchantName { get; set; }
        /// <summary>
        /// 從收單行端取得授權碼後，要導回的網址，請勿填入特殊字元@#%?&等
        /// </summary>
        public string AuthResURL { get; set; }
        /// <summary>
        /// 訂單描述，中文請填BIG5碼
        /// </summary>
        public string OrderDetail { get; set; }
        /// <summary>
        /// 不自動請款:0,自動請款:1
        /// </summary>
        public string AutoCap { get; set; }
        /// <summary>
        /// 設定刷卡頁顯示特定語系或客制化頁面。
        /// 繁體中文:1,簡體中文:2,英文:3,客製化頁面:5
        /// </summary>
        public string Customize { get; set; }
        /// <summary>
        /// 紅利折抵一般或紅利折抵分期的產品代碼，純數字，長度為 0 ~ 2 。 
        /// 紅利折抵一般特店或紅利折抵分期特店必填 ，其餘特店免填
        /// </summary>
        public string ProdCode { get; set; }
        /// <summary>
        /// 分期的付款期數，純數字，長度為 0 ~ 2 。 
        /// 分期特店或紅利折抵分期特 店必填 ，其餘特店免填
        /// </summary>
        public string NumberOfPay { get; set; }
        /// <summary>
        /// 以指定的Key 將資料加密後的密文 唯讀
        /// </summary>
        public string EncodeData { get; set; }
        /// <summary>
        /// 產生密文 時的錯誤代碼 唯讀
        /// </summary>
        public string LastError { get; set; }
        /// <summary>
        /// (選填)字元的編碼， Encoding 型態，預設是 Big5 。
        /// </summary>
        public string EncodingType { get; set; }

        //函式ClearData 除MerchantID,TerminalID,Key欄位部清除，會將所有的屬性資料清空。
    }

    /// <summary>
    /// ctcbMac.dll 加解密元件
    /// CTCB.Crypto.Decrypt 解開網路收單伺服器所回傳的 資料 密文
    /// </summary>
    public class CTCB_DecryptModel
    {
        /// <summary>
        /// 伺服器所回傳的密文
        /// </summary>
        public string EncRes { get; set; }
        /// <summary>
        /// 交易執行狀態，純數字, 長度為 1 或 2
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 交易錯誤代碼，文數字, 長度為 0 或 2
        /// </summary>
        public string ErrCode { get; set; }
        /// <summary>
        /// 交易授權碼，最大長度為6 的文字串。
        /// </summary>
        public string AuthCode { get; set; }
        /// <summary>
        /// 授權金額，長度 1~7 位 正整數 格式的字串
        /// </summary>
        public string AuthAmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MerID { get; set; }
        /// <summary>
        /// 為電子商場的應用程式所給予此筆交易的訂單編號，資料型態為最長 19 個字元的文字串。 
        /// 訂單編號字串之字元僅接受一般英文字母、數字及底線’ ’__’的組合，不可出現其餘符號字元。
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 折抵金額。純數字 , 長度 1~7 位的字串
        /// </summary>
        public string OffsetAmt { get; set; }
        /// <summary>
        /// 原始訂單金額。純數字格式，長度1~7位的字串
        /// </summary>
        public string OriginalAmt { get; set; }
        /// <summary>
        /// 本次兌換點數，正整數格式長度1~7位的字串
        /// </summary>
        public string UtilizedPoint { get; set; }
        /// <summary>
        /// 純數字欄位，依交易方式不同填入不同的資料，說明如下：
        /// 若txType = 0 ，資料請填 「 1 」 。
        /// 若txType=1，資料資料為一到兩碼的分期期數。
        /// 若txType=2，資料為固定兩碼的產品代碼。
        /// 若txType=4，資料資料為三到四碼，前兩碼固定為產品代產品代碼，後一碼或兩碼為分期期數。
        /// </summary>
        public string Option { get; set; }
        /// <summary>
        /// 卡號末四碼純數字, 長度為 0 或 4
        /// </summary>
        public string Last4digitPAN { get; set; }
        /// <summary>
        /// 此為貴特店在URL 帳務管理後台登錄的壓碼字串。
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 交易錯誤訊息 ，文數字
        /// </summary>
        public string ErrDesc { get; set; }
        /// <summary>
        /// 從收單行端取得授權碼後，要導回的網址
        /// </summary>
        public string AuthResURL { get; set; }
        /// <summary>
        /// 此次授權之交易序號，最長為40 個位元文字串 此值為系統的內定的 Unquie 值
        /// </summary>
        public string XID { get; set; }
        /// <summary>
        /// 此次紅利交易的賺取點數
        /// </summary>
        public string AwardedPoint { get; set; }
        /// <summary>
        /// 為此次紅利交易的點數餘額
        /// </summary>
        public string PointBalance { get; set; }
        /// <summary>
        /// 紅利折抵的產品代碼 ，純數字 長度為 0 ~ 2 。 
        /// 紅利折抵一般特店與紅利折抵分期特店必填 ，其餘特店免填
        /// </summary>
        public string ProdCode { get; set; }
        /// <summary>
        /// 分期的付款期數，純數字，長度為 0 ~ 2。分期特店與紅利折抵分期特店必填 ，其餘特店免填
        /// </summary>
        public string NumberOfPay { get; set; }
        /// <summary>
        /// 產生壓碼時的錯誤代碼(唯讀)
        /// </summary>
        public string LastError { get; set; }
        /// <summary>
        /// 隱碼卡號(654321******)
        /// </summary>
        public string CardNumber { get; set; }
        /// <summary>
        /// 身分證字號核對
        /// </summary>
        public string PidResult { get; set; }
        /// <summary>
        /// 取得經 SHA 256 加密後的卡號(僅供優化環境使用)
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 是否設定電子發票信用卡載具(僅供優化環境使用) 否:0,是:1
        /// </summary>
        public string EInvoice { get; set; }

        ///函式ClearData，除Key欄位職不清除外，會將所有屬性資料清空。

    }
}