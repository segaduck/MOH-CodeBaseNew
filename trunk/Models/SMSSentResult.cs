using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EECOnline.Models
{
    #region 系統簡訊訊息
    /// <summary>系統簡訊訊息</summary>
    public class SMSMessage
    {
        /// <summary>預設建構子</summary>
        public SMSMessage() { }

        /// <summary>預設建構子</summary>
        /// <param name="phone">行動電話號碼</param>
        /// <param name="message">簡訊內容文字</param>
        public SMSMessage(string phone, string text)
        {
            Phone = phone;
            Text = text;
        }

        /// <summary>預設建構子</summary>
        /// <param name="phone">行動電話號碼</param>
        /// <param name="text">簡訊內容文字</param>
        /// <param name="name">簡訊接收者名稱（註：在寫入系統處理日誌時會使用到，用以辨識簡訊發送給誰）</param>
        public SMSMessage(string phone, string text, string name)
        {
            Phone = phone;
            Text = text;
            Name = name;
        }

        /// <summary>簡訊接收者行動電話號碼</summary>
        public string Phone { get; set; }

        /// <summary>簡訊內容文字</summary>
        public string Text { get; set; }

        /// <summary>簡訊接收者名稱（註：在寫入系統處理日誌時會使用到，用以辨識簡訊發送給誰）</summary>
        public string Name { get; set; }
    }
    #endregion

    /// <summary>系統簡訊傳送處理結果</summary>
    public class SMSSentResult
    {
        /// <summary>處理的簡訊</summary>
        SMSMessage _Message = null;

        /// <summary>預設建構子</summary>
        public SMSSentResult()
        {
            _Message = new SMSMessage();
        }

        /// <summary>預設建構子</summary>
        /// <param name="message">處理的簡訊</param>
        public SMSSentResult(SMSMessage message)
        {
            _Message = message;
        }

        /// <summary>預設建構子</summary>
        /// <param name="phone">行動電話號碼</param>
        /// <param name="message">簡訊內容文字</param>
        public SMSSentResult(string phone, string text)
        {
            _Message = new SMSMessage();
            _Message.Phone = phone;
            _Message.Text = text;
        }

        /// <summary>預設建構子</summary>
        /// <param name="phone">行動電話號碼</param>
        /// <param name="text">簡訊內容文字</param>
        /// <param name="name">簡訊接收者名稱（註：在寫入系統處理日誌時會使用到，用以辨識簡訊發送給誰）</param>
        public SMSSentResult(string phone, string text, string name)
        {
            _Message = new SMSMessage();
            _Message.Phone = phone;
            _Message.Text = text;
            _Message.Name = name;
        }

        /// <summary>處理的簡訊</summary>
        public SMSMessage Message
        {
            get { return _Message; }
        }

        /// <summary>行動電話號碼</summary>
        public string Phone
        {
            get { return (_Message == null) ? null : _Message.Phone; }
        }

        /// <summary>簡訊內容文字</summary>
        public string Text
        {
            get { return (_Message == null) ? null : _Message.Text; }
        }

        /// <summary>簡訊接收者名稱</summary>
        public string Name
        {
            get { return (_Message == null) ? null : _Message.Name; }
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
}