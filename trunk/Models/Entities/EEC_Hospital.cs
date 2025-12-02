using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_Hospital : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }

        /// <summary>醫院代號</summary>
        public string code { get; set; }

        /// <summary>醫院名稱</summary>
        public string text { get; set; }

        /// <summary>城市代號</summary>
        public string city { get; set; }

        /// <summary>城市名稱</summary>
        public string cityName { get; set; }

        /// <summary>排序用</summary>
        public string orderby { get; set; }

        /// <summary>醫院登入用信箱 (帳號)</summary>
        public string Email { get; set; }

        /// <summary>醫院授權碼</summary>
        public string AuthCode { get; set; }

        /// <summary>醫院登入用密碼</summary>
        public string PWD { get; set; }

        /// <summary>醫院授權碼 產生時間</summary>
        public string AuthDate { get; set; }

        /// <summary>醫院帳號狀態</summary>
        public string AuthStatus { get; set; }

        /// <summary>錯誤次數</summary>
        public int? errct { get; set; }

        /// <summary>滿意度問卷網址</summary>
        public string ResponsePage { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_Hospital;
        }
    }
}