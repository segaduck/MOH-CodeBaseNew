using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_ApplyDetailUploadLog : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }

        /// <summary>申請案號 (表頭的) [yyyyMMddHHmmssfff]</summary>
        public string apply_no { get; set; }

        /// <summary>申請案號 (明細的) [yyyyMMddHHmmssfff + 身分證後9碼 + 流水編號(001)]</summary>
        public string apply_no_sub { get; set; }

        /// <summary>使用者 身分證字號</summary>
        public string user_idno { get; set; }

        /// <summary>使用者 名稱</summary>
        public string user_name { get; set; }

        /// <summary>醫院代號</summary>
        public string hospital_code { get; set; }

        /// <summary>醫院名稱</summary>
        public string hospital_name { get; set; }

        /// <summary>病歷類型代號 (單筆)</summary>
        public string his_type { get; set; }

        /// <summary>病歷類型名稱 (單筆)</summary>
        public string his_type_name { get; set; }

        /// <summary>病歷時間區間 (起)</summary>
        public string his_range1 { get; set; }

        /// <summary>病歷時間區間 (迄)</summary>
        public string his_range2 { get; set; }

        /// <summary>申請時間</summary>
        public string createdatetime { get; set; }

        /// <summary>提供日期時間</summary>
        public string provide_datetime { get; set; }

        /// <summary>提供的檔案 base64 碼</summary>
        public string provide_bin { get; set; }

        /// <summary>提供的檔案的副檔名</summary>
        public string provide_ext { get; set; }

        /// <summary>提供的登入者帳號</summary>
        public string provide_user_no { get; set; }

        /// <summary>提供的登入者姓名</summary>
        public string provide_user_name { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_ApplyDetailUploadLog;
        }
    }
}