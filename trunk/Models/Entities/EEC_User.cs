using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_User : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }

        /// <summary>使用者 名稱</summary>
        public string user_name { get; set; }

        /// <summary>使用者 身分證字號</summary>
        public string user_idno { get; set; }

        /// <summary>使用者 出生年月日</summary>
        public string user_birthday { get; set; }

        /// <summary>使用者 自然人憑證 PIN 碼</summary>
        public string user_pincode { get; set; }

        /// <summary>使用者 健保卡註冊密碼</summary>
        public string user_cardpwd { get; set; }

        /// <summary>使用者 電子郵件Email</summary>
        public string user_email { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_User;
        }
    }
}