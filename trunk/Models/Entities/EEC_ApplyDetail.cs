using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_ApplyDetail : IDBRow
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

        /// <summary>醫院代號</summary>
        public string hospital_code { get; set; }

        /// <summary>醫院名稱</summary>
        public string hospital_name { get; set; }

        /// <summary>病歷時間區間 (起)</summary>
        public string his_range1 { get; set; }

        /// <summary>病歷時間區間 (迄)</summary>
        public string his_range2 { get; set; }

        /// <summary>病歷類型 (多筆，以逗號分隔代號)</summary>
        public string his_types { get; set; }

        /// <summary>付款截止日</summary>
        public string pay_deadline { get; set; }

        /// <summary>是否已付款 (Y = 已付)</summary>
        public string payed { get; set; }

        /// <summary>付款日期時間</summary>
        public string payed_datetime { get; set; }

        /// <summary>信用卡付款的 orderid</summary>
        public string payed_orderid { get; set; }

        /// <summary>信用卡付款的 sessionkey</summary>
        public string payed_sessionkey { get; set; }

        /// <summary>信用卡付款的 transdate</summary>
        public string payed_transdate { get; set; }

        /// <summary>信用卡付款的 approvecode</summary>
        public string payed_approvecode { get; set; }

        /// <summary>信用卡付款的 - 是否請款成功</summary>
        public string is_request_payment { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_ApplyDetail;
        }
    }
}