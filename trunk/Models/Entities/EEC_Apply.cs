using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_Apply : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }

        /// <summary>申請案號 [yyyyMMddHHmmssfff]</summary>
        public string apply_no { get; set; }

        /// <summary>使用者 身分證字號</summary>
        public string user_idno { get; set; }

        /// <summary>使用者 自然人憑證 PIN 碼</summary>
        public string user_pincode { get; set; }

        /// <summary>使用者 健保卡註冊密碼</summary>
        public string user_cardpwd { get; set; }

        /// <summary>使用者 名稱</summary>
        public string user_name { get; set; }

        /// <summary>使用者 出生年月日</summary>
        public string user_birthday { get; set; }

        /// <summary>使用何種方式登入<br/>
        /// 1: 自然人憑證登入<br/>
        /// 2: 行動自然人憑證登入<br/>
        /// 3: 身分證字號＋註冊健保卡插卡<br/>
        /// </summary>
        public string login_type { get; set; }

        /// <summary>申請時間</summary>
        public string createdatetime { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_Apply;
        }
    }
}