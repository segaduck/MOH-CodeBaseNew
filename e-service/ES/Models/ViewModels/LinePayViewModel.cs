using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using ES.DataLayers;
using System.Web.Mvc;
using System.Web.Configuration;
using CTCB.Crypto;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_007001 線上捐款
    /// </summary>
    public class LinePayViewModel
    {
        /// <summary>
        /// 參考網址
        /// 技術文件https://pay.line.me/tw/developers/apis/onlineApis?locale=zh_TW
        /// </summary>
        public LinePayViewModel()
        {
            this.currency = "TWD";
            this.packages = new List<LinePayPackages>();
            this.redirectUrls = new LinePayRedirectUrls();
            this.options = new LinePayOptions();
        }
        public Int32? amount { get; set; }
        public string currency { get; set; }
        public string orderId { get; set; }
        public List<LinePayPackages> packages { get; set; }
        public LinePayRedirectUrls redirectUrls { get; set; }
        public LinePayOptions options { get; set; }
    }

    public class LinePayPackages
    {
        public LinePayPackages()
        {
            this.products = new List<LinePayProducts>();
        }
        public string id { get; set; }
        public Int32? amount { get; set; }
        //public Int32? userFee { get; set; }
        public string name { get; set; }
        public List<LinePayProducts> products { get; set; }
    }
    public class LinePayProducts
    {
        public LinePayProducts()
        {
            //this.imageUrl = "https://e-service.mohw.gov.tw/Images/default/banner-covid19Payment.jpg";
        }
        public string id { get; set; }
        public string name { get; set; }
        //public string imageUrl { get; set; }
        public Int32? quantity { get; set; }
        public Int32? price { get; set; }
        //public Int32? originalPrice { get; set; }

    }
    public class LinePayRedirectUrls
    {
        public LinePayRedirectUrls()
        {
            this.cancelUrl = "http://210.68.37.161:4155/ApplyDonate/DonateResult";
            this.confirmUrl = "http://210.68.37.161:4155/ApplyDonate/DonateResult";
        }
        //public string appPackageName { get; set; }
        public string confirmUrl { get; set; }
        //public string confirmUrlType { get; set; }
        public string cancelUrl { get; set; }

    }
    public class LinePayOptions
    {
        public LinePayOptions()
        {
            this.payment = new LinePayPayment();
            //this.display = new LinePayDisplay();
            //this.shipping = new LinePayShipping();
            //this.familyService = new LinePayFamilyService();
            //this.extra = new LinePayExtra();
        }
        public LinePayPayment payment { get; set; }
        //public LinePayDisplay display { get; set; }
        //public LinePayShipping shipping { get; set; }
        //public LinePayFamilyService familyService { get; set; }
        //public LinePayExtra extra { get; set; }
    }

    public class LinePayPayment
    {
        public LinePayPayment()
        {
            //this.capture = true;
            this.payType = "PREAPPROVED";
        }
        public bool? capture { get; set; }
        public string payType { get; set; }
    }
    public class LinePayDisplay
    {
        public LinePayDisplay()
        {
            this.locale = "zh";
            this.checkConfirmUrlBrowser = false;
        }
        public string locale { get; set; }
        public bool? checkConfirmUrlBrowser { get; set; }
    }
    public class LinePayShipping
    {
        public LinePayShipping()
        {
            this.address = new LinePayShippingAddress();
            this.type = "NO_SHIPPING";
        }
        public string type { get; set; }
        public string feeAmount { get; set; }
        public string feeInquiryUrl { get; set; }
        public string feeInquiryType { get; set; }
        public LinePayShippingAddress address { get; set; }
    }
    public class LinePayShippingAddress
    {
        public LinePayShippingAddress()
        {
            this.recipient = new LinePayShippingAddressRecipient();
        }
        public string country { get; set; }
        public string postalCode { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string detail { get; set; }
        public string optional { get; set; }
        public LinePayShippingAddressRecipient recipient { get; set; }
    }
    public class LinePayShippingAddressRecipient
    {
        public LinePayShippingAddressRecipient()
        {

        }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string firstNameOptional { get; set; }
        public string lastNameOptional { get; set; }
        public string email { get; set; }
        public string phoneNo { get; set; }
    }
    public class LinePayFamilyService
    {
        public LinePayFamilyService()
        {
            this.addFriends = new List<LinePayFamilyServiceAddFriends>();
        }
        public List<LinePayFamilyServiceAddFriends> addFriends { get; set; }
    }
    public class LinePayFamilyServiceAddFriends
    {
        public LinePayFamilyServiceAddFriends()
        {
            this.ids = new List<string>();
        }
        public string type { get; set; }
        public List<string> ids { get; set; }
    }
    public class LinePayExtra
    {
        public LinePayExtra()
        {
            this.promotionRestriction = new LinePayPromotionRestriction();
        }
        public string branchName { get; set; }
        public string branchId { get; set; }
        public object promotionRestriction { get; set; }
    }
    public class LinePayPromotionRestriction
    {
        public LinePayPromotionRestriction()
        {
            this.useLimit = 0;
            this.rewardLimit = 0;
        }
        public Int32? useLimit { get; set; }
        public Int32? rewardLimit { get; set; }
    }
    public class LinePayResponseViewModel
    {
        public LinePayResponseViewModel()
        {
            this.info = new LinePayResponseInfo();
            /*
0000	成功
1104	此商家不存在
1105	此商家無法使用LINE Pay
1106	標頭(Header)資訊錯誤
1124	金額有誤（scale）
1145	正在進行付款
1172	該訂單編號(orderId)的交易記錄已經存在
1178	商家不支援該貨幣
1183	付款金額不能小於 0
1194	此商家無法使用自動付款
2101	參數錯誤
2102	JSON資料格式錯誤
9000	內部錯誤
             */
        }
        public string returnCode { get; set; }
        public string returnMessage { get; set; }
        public LinePayResponseInfo info { get; set; }
    }
    public class LinePayResponseInfo
    {
        public LinePayResponseInfo()
        {
            this.paymentUrl = new LinePayResponsePaymentUrl();
        }
        public Int32? transactionId { get; set; }
        public string paymentAccessToken { get; set; }
        public LinePayResponsePaymentUrl paymentUrl { get; set; }
    }
    public class LinePayResponsePaymentUrl
    {
        public LinePayResponsePaymentUrl()
        {

        }
        public string app { get; set; }
        public string web { get; set; }
    }
}