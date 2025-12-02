using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_ApplyDetailPrice : IDBRow
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

        /// <summary>病歷類型代號 (單筆)<br />如果是特殊醫院代號，則紀錄 ec_no</summary>
        public string his_type { get; set; }

        /// <summary>病歷類型名稱 (單筆)<br />如果是特殊醫院代號，則紀錄 ec_name</summary>
        public string his_type_name { get; set; }

        /// <summary>病歷類型單價 (單筆)</summary>
        public int? price { get; set; }

        /// <summary>付款截止日</summary>
        public string pay_deadline { get; set; }

        /// <summary>是否已付款 (Y = 已付)</summary>
        public string payed { get; set; }

        /// <summary>付款日期時間</summary>
        public string payed_datetime { get; set; }

        /// <summary>///// 目前已取消使用此欄，改以 provide_status 為主 /////<br />有無提供 (Y/N)</summary>
        public string isprovide { get; set; }

        /// <summary>提供日期時間</summary>
        public string provide_datetime { get; set; }

        /// <summary>提供的檔案 base64 碼</summary>
        public string provide_bin { get; set; }

        /// <summary>提供的檔案的副檔名</summary>
        public string provide_ext { get; set; }

        /// <summary>是否有取得病歷資料的狀態<br />
        /// 0: 預設值 尚未跑排程 <br />
        /// 1: 跑過排程 XML 有資料 <br />
        /// 2: 跑過排程 XML 無資料 <br />
        /// </summary>
        public string provide_status { get; set; }

        /// <summary>for 特殊醫院代號 的 病歷日期(對方給的 ec_date)</summary>
        public string ec_date { get; set; }

        /// <summary>for 特殊醫院代號 的 病歷日期說明(對方給的 ec_dateText)</summary>
        public string ec_dateText { get; set; }

        /// <summary>for 特殊醫院代號 的 病歷備註(對方給的 ec_note)</summary>
        public string ec_note { get; set; }

        /// <summary>for 特殊醫院代號 的 病歷科別(對方給的 ec_dept)</summary>
        public string ec_dept { get; set; }

        /// <summary>for 特殊醫院代號 的 病歷醫師(對方給的 ec_doctor)</summary>
        public string ec_doctor { get; set; }

        /// <summary>for 特殊醫院代號 的 表單名稱(對方給的 ec_docType)</summary>
        public string ec_docType { get; set; }

        /// <summary>for 特殊醫院代號 的 系統名稱(對方給的 ec_system)</summary>
        public string ec_system { get; set; }

        /// <summary>for 特殊醫院代號<br />匯出至SFTP檔案名稱</summary>
        public string ec_fileName { get; set; }

        /// <summary>for 特殊醫院代號<br />API: A2-1 回傳是否成功值</summary>
        public string ec_success { get; set; }

        /// <summary>for 特殊醫院代號<br />API: A2-2 回傳是否成功值</summary>
        public string ec_success_yn { get; set; }

        /// <summary>for 特殊醫院代號<br />API: A2-2 回傳原因說明</summary>
        public string ec_reason { get; set; }

        /// <summary>記錄病歷下載次數</summary>
        public int? download_count { get; set; }

        /// <summary>傳輸 API 用的 caseNo <br />
        /// 依醫院需要之格式存放 (目前僅中山醫使用)</summary>
        public string caseNo { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_ApplyDetailPrice;
        }
    }
}