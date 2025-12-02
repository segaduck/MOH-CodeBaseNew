using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申請案件-醫事人員-歇業後執業-執業
    /// </summary>
    public class TblAPPLY_001008_AF : IDBRow
    {
        /// <summary>
        /// 案件編碼
        /// </summary>
        [Display(Name = "案件編碼")]
        public string apy_not_deta_id { get; set; }

        /// <summary>
        /// 申請項目編碼
        /// </summary>
        [Display(Name = "申請項目編碼")]
        public string srv_id { get; set; }

        /// <summary>
        /// 申辦縣市
        /// </summary>
        [Display(Name = "申請執業縣市")]
        public string apy_city { get; set; }

        /// <summary>
        /// 申請歇業縣市轄區
        /// </summary>
        [Display(Name = "申請歇業縣市轄區")]
        public string apy_city_s { get; set; }

        /// <summary>
        /// 案件申請人電話
        /// </summary>
        [Display(Name = "案件申請人電話")]
        public string apy_tel_af { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        public string apy_mail_af { get; set; }

        /// <summary>
        /// 公會會員證字號
        /// </summary>
        [Display(Name = "公會會員證字號")]
        public string apy_member_id_af { get; set; }

        /// <summary>
        /// 執業機構名稱代碼
        /// </summary>
        [Display(Name = "執業機構名稱代碼")]
        public string apy_bas_agency_id_af { get; set; }

        /// <summary>
        /// 執業機構院別代碼
        /// </summary>
        [Display(Name = "執業機構院別代碼")]
        public string apy_zone_code_af { get; set; }
        
        /// <summary>
        /// 執業機構名稱
        /// </summary>
        [Display(Name = "執業機構名稱")]
        public string apy_bas_name_af { get; set; }

        /// <summary>
        /// 機構地址郵遞區號
        /// </summary>
        [Display(Name = "機構地址郵遞區號")]
        public string apy_bas_addr_zip_af { get; set; }

        /// <summary>
        /// 機構地址
        /// </summary>
        [Display(Name = "機構地址")]
        public string apy_bas_addr_af { get; set; }

        /// <summary>
        /// 執業機構電話
        /// </summary>
        [Display(Name = "執業機構電話")]
        public string apy_bas_tel_no_af { get; set; }

        /// <summary>
        /// 執業科別
        /// </summary>
        [Display(Name = "執業科別")]
        public string apy_prt_dept_1_af { get; set; }

        /// <summary>
        /// 執業日期
        /// </summary>
        [Display(Name = "執業日期")]
        public string apy_bas_date { get; set; }

        /// <summary>
        /// 領取項目編碼
        /// </summary>
        [Display(Name = "領取項目編碼")]
        public string apy_get_id { get; set; }

        /// <summary>
        /// 領取郵遞區號
        /// </summary>
        [Display(Name = "領取郵遞區號")]
        public string apy_get_addr { get; set; }

        /// <summary>
        /// 領取地址
        /// </summary>
        [Display(Name = "領取地址")]
        public string apy_get_addr_ADDR { get; set; }

        /// <summary>
        /// 案件完成狀態
        /// </summary>
        [Display(Name = "案件完成狀態")]
        public string apy_status { get; set; }

        /// <summary>
        /// 分文承辦人員
        /// </summary>
        [Display(Name = "分文承辦人員")]
        public string apy_undertaker { get; set; }

        /// <summary>
        /// 案件狀態說明
        /// </summary>
        [Display(Name = "案件狀態說明")]
        public string returndesc_af { get; set; }

        /// <summary>
        /// 編輯者帳號
        /// </summary>
        [Display(Name = "編輯者帳號")]
        public string moduser { get; set; }

        /// <summary>
        /// 編輯者姓名
        /// </summary>
        [Display(Name = "編輯者姓名")]
        public string modusername { get; set; }

        /// <summary>
        /// 編輯時間
        /// </summary>
        [Display(Name = "編輯時間")]
        public string modtime { get; set; }

        /// <summary>
        /// 編輯IP
        /// </summary>
        [Display(Name = "編輯IP")]
        public string modip { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.APPLY_001008_AF;
        }
    }
}