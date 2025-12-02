using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;

namespace ES.Models
{
    public class X509CertsHelper
    {
        X509Certificate x509 = null;

        /// <summary>
        /// 傳入 Base64 編碼的 X509 憑證資料建構 X509CertsHelper
        /// </summary>
        /// <param name="sCert"></param>
        public X509CertsHelper(string sCert)
        {
            byte[] certData = Convert.FromBase64String(sCert);
            x509 = new X509Certificate(certData);
        }

        /// <summary>
        /// 憑證的主旨辨別名稱
        /// </summary>
        public string Subject { get { return (x509 != null) ? x509.Subject : null; } }
        /// <summary>
        /// 憑證內部序號
        /// </summary>
        public string SerialNumber { get { return (x509 != null) ? x509.GetSerialNumberString() : null; } }
        /// <summary>
        /// 憑證授權(簽發)單位名稱
        /// </summary>
        public string Issuer { get { return (x509 != null) ? x509.Issuer : null; } }
        /// <summary>
        /// 憑證有效日期(起)
        /// </summary>
        public string NotBefore { get { return (x509 != null) ? x509.GetEffectiveDateString() : null; } }
        /// <summary>
        /// 憑證有效日期(迄)
        /// </summary>
        public string NotAfter { get { return (x509 != null) ? x509.GetExpirationDateString() : null; } }

        /// <summary>
        /// 憑證簽發資訊 C=TW, CN=ｏｏｏ, SERIALNUMBER=000000000000000
        /// </summary>
        public string GNameAll { get { return (x509 != null) ? x509.GetName() : null; } }

        /// <summary>
        /// 憑證核發對象名稱
        /// </summary>
        public string Name
        {
            get
            {
                if (x509 == null) { return null; }
                string str = GNameAll;
                if (str == null) { return null; }
                string[] tk = Regex.Split(str, ", ", RegexOptions.IgnoreCase);
                if (tk == null) { return null; }
                if (tk.Length < 3) { return null; }
                string[] parts = tk[1].Split('=');
                if (parts == null) { return null; }
                if (parts.Length < 2) { return null; }
                return parts[1];
            }
        }

        /// <summary>
        /// 憑證簽發卡號
        /// </summary>
        public string CardID
        {
            get
            {
                if (x509 == null) { return null; }
                string str = GNameAll;
                if (str == null) { return null; }
                string[] tk = Regex.Split(str, ", ", RegexOptions.IgnoreCase);
                if (tk == null) { return null; }
                if (tk.Length < 3) { return null; }
                string[] parts = tk[2].Split('=');
                if (parts == null) { return null; }
                if (parts.Length < 2) { return null; }
                return parts[1];
            }
        }

        /// <summary>
        /// 此憑證是否有效
        /// </summary>
        public bool IsExpired
        {
            get
            {
                bool expired = true;
                DateTime dStart = Convert.ToDateTime(this.NotBefore);
                DateTime dEnd = Convert.ToDateTime(this.NotAfter);
                DateTime now = DateTime.Now;
                if ((now.CompareTo(dStart) >= 0) && (now.CompareTo(dEnd) <= 0)) { expired = false; }
                return expired;
            }
        }

        /// <summary>
        /// 是否為廢止憑證(CRL)
        /// </summary>
        public bool IsCRL { get { return false; } }

        public override string ToString()
        {
            return string.Format("[Subject] {0}\n[Issuer] {1}\n[NotBefore] {2}\n[NotAfter] {3}\n[SerialNumber] {4}\n[Name] {5}\n[CardID] {6}\n[Expired] {7}\n[GNameAll] {8}",
                this.Subject, this.Issuer, this.NotBefore, this.NotAfter,
                this.SerialNumber, this.Name, this.CardID, this.IsExpired, this.GNameAll);
        }
    }
}