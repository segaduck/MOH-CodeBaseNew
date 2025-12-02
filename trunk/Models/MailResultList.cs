using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace EECOnline.Models
{
    #region 系統郵件傳送處理結果
    /// <summary>系統郵件傳送處理結果</summary>
    public class MailSentResult
    {
        /// <summary>郵件訊息</summary>
        MailMessage _Message = null;

        /// <summary>預設建構子</summary>
        public MailSentResult()
        {
            _Message = new MailMessage();
        }

        /// <summary>預設建構子</summary>
        /// <param name="message">處理的郵件</param>
        public MailSentResult(MailMessage message)
        {
            _Message = message;
        }

        /// <summary>處理的郵件</summary>
        public MailMessage Message
        {
            get { return _Message; }
        }

        /// <summary>郵件主旨</summary>
        public string Subject
        {
            get { return (_Message == null) ? null : _Message.Subject; }
        }

        /// <summary>郵件訊息主體</summary>
        public string Body
        {
            get { return (_Message == null) ? null : _Message.Body; }
        }

        /// <summary>寄件者電子郵件地址</summary>
        public MailAddress Sender
        {
            get { return (_Message == null) ? null : _Message.Sender; }
        }

        /// <summary>處理是否成功。（true: 處理成功，false: 處理失敗）</summary>
        public bool IsSuccess { get; set; } = false;

        /// <summary>處理結果代碼。（null: 處理成功，"-1": 系統預設錯誤代碼，其他值： 自訂錯誤代碼）</summary>
        public string ResultCode { get; set; }

        /// <summary>處理結果訊息</summary>
        public string ResultText { get; set; }

        /// <summary>處理開始日期時間</summary>
        public DateTime Start { get; set; } = DateTime.MinValue;

        /// <summary>處理結束日期時間</summary>
        public DateTime End { get; set; } = DateTime.MinValue;

        /// <summary>已處理次數</summary>
        public int TriedTimes { get; set; }

        /// <summary>設定處理結果為「處理成功」</summary>
        /// <param name="resultText">（非必要）在處理成功時的結果訊息</param>
        public void SetSuccessResult(string resultText = "處理完畢")
        {
            IsSuccess = true;
            ResultCode = null;
            ResultText = resultText;
            End = DateTime.Now;
        }

        /// <summary>設定處理結果為「處理失敗」</summary>
        /// <param name="resultText">失敗結果訊息</param>
        /// <param name="resultCode">（非必要）失敗結果代碼</param>
        public void SetFailureResult(string resultText, string resultCode = "-1")
        {
            if (string.IsNullOrEmpty(resultText)) throw new ArgumentNullException("resultText");
            IsSuccess = false;
            ResultCode = string.IsNullOrEmpty(resultCode) ? "-1" : resultCode;
            ResultText = resultText;
            End = DateTime.Now;
        }

        /// <summary>設定處理結果為「處理失敗」</summary>
        /// <param name="ex">異常例外</param>
        /// <param name="resultCode">（非必要）失敗結果代碼</param>
        public void SetFailureResult(Exception ex, string resultCode = "-1")
        {
            if (ex == null) throw new ArgumentNullException("ex");
            SetFailureResult(ex.Message, resultCode);
        }
    }
    #endregion

    #region 電子郵件傳送處理結果項目
    /// <summary>電子郵件傳送處理結果項目</summary>
    public class MailResultItem
    {
        /// <summary>預設建構子</summary>
        public MailResultItem() { }

        /// <summary>預設建構子</summary>
        /// <param name="itemNo">項目流水編號</param>
        public MailResultItem(int itemNo)
        {
            ItemNo = itemNo;
        }

        /// <summary>項目流水編號</summary>
        public int ItemNo { get; set; }

        /// <summary>電子郵件傳送處理是否成功。（true: 處理成功，false: 處理失敗）</summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>收件者電子郵件位址</summary>
        public MailAddressCollection ToMailAddr { get; set; }

        /// <summary>處理開始日期時間</summary>
        public DateTime Start { get; set; } = DateTime.MinValue;

        /// <summary>處理結束日期時間</summary>
        public DateTime End { get; set; } = DateTime.MinValue;

        /// <summary>電子郵件傳送處理結果訊息</summary>
        public string Result { get; set; }
    }
    #endregion

    #region 電子郵件傳送處理結果集合
    /// <summary>電子郵件傳送處理結果集合</summary>
    public class MailResultList
    {
        /// <summary>電子郵件傳送結果彙總訊息</summary>
        MailResultItem _Summary = null;
        
        /// <summary>電子郵件傳送結果訊息</summary>
        List<MailResultItem> _Results = null;

        /// <summary>預設建構子</summary>
        public MailResultList()
        {
            _Summary = new MailResultItem();
            _Results = new List<MailResultItem>();
        }

        /// <summary>電子郵件傳送結果彙總訊息</summary>
        public MailResultItem Summary
        {
            get { return _Summary; }
        }

        /// <summary>應傳送郵件數</summary>
        public int Total { get; set; }

        /// <summary>已處理郵件數</summary>
        public int Success { get; set; }

        /// <summary>失敗郵件數</summary>
        public int Failure { get; set; }

        /// <summary>電子郵件傳送處理是否全部成功。（true: 全部處理成功，false: 部份處理失敗）</summary>
        public bool IsAllSuccess {
            get { return (_Summary.IsSuccess && Failure == 0); }
        }

        /// <summary>電子郵件傳送處理結果集合</summary>
        public IList<MailResultItem> Results
        {
            get { return _Results; }
        }
    }
    #endregion
}