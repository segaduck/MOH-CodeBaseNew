using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申請案件
    /// </summary>
    public class TblAPPLY : IDBRow
    {
        /// <summary>
        /// 案件編碼
        /// </summary>
        [Display(Name = "案件編碼")]
        public string apy_id { get; set; }

        /// <summary>
        /// 申請項目編碼
        /// </summary>
        [Display(Name = "申請項目編碼")]
        public string srv_id { get; set; }

        /// <summary>
        /// 申辦項目縣市       
        /// </summary>
        [Display(Name = "申辦項目縣市")]
        public string apy_city { get; set; }

        /// <summary>
        /// 申辦項目縣市轄區       
        /// </summary>
        [Display(Name = "申辦項目縣市轄區")]
        public string apy_city_s { get; set; }

        /// <summary>
        /// 申辦方式
        /// 1 個人申請
        /// 2 批次申請
        /// </summary>
        [Display(Name = "申辦方式")]
        public string apy_type { get; set; }

        /// <summary>
        /// 案件申請人姓名
        /// </summary>
        [Display(Name = "案件申請人姓名")]
        public string doc_name { get; set; }

        /// <summary>
        /// 案件申請人身分證 / 案件申請機構統一編號
        /// </summary>
        [Display(Name = "案件申請人身分證 / 案件申請機構統一編號")]
        public string doc_id_no { get; set; }

        /// <summary>
        /// 性別
        /// 1 男 
        /// 2 女
        /// 3 不透漏
        /// </summary>
        [Display(Name = "性別")]
        public string doc_sex { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        public string doc_birth { get; set; }

        /// <summary>
        /// 是否為公費生(public_expense)
        /// </summary>
        [Display(Name = "是否為公費生")]
        public string pe_yn { get; set; }

        /// <summary>
        /// 公費機構代碼
        /// </summary>
        [Display(Name = "公費機構代碼")]
        public string pe_bas_id { get; set; }

        /// <summary>
        /// 公費機構名稱
        /// </summary>
        [Display(Name = "公費機構名稱")]
        public string pe_bas_nm { get; set; }

        /// <summary>
        /// 公費機構地址郵遞區號
        /// </summary>
        [Display(Name = "公費機構地址郵遞區號")]
        public string pe_bas_addr_code { get; set; }

        /// <summary>
        /// 公費機構地址
        /// </summary>
        [Display(Name = "公費機構地址")]
        public string pe_bas_addr { get; set; }

        /// <summary>
        /// 公費機構電話
        /// </summary>
        [Display(Name = "公費機構電話")]
        public string pe_bas_tel_no { get; set; }

        /// <summary>
        /// 人員狀態
        /// </summary>
        [Display(Name = "人員狀態")]
        public string doc_status { get; set; }

        /// <summary>
        /// 醫事人員/機構 類別代碼
        /// </summary>
        [Display(Name = "醫事人員/機構 類別代碼")]
        public string cer_ref_id { get; set; }

        /// <summary>
        /// 醫事人員/機構 類別
        /// </summary>
        [Display(Name = "醫事人員/機構 類別")]
        public string ref_name { get; set; }

        /// <summary>
        /// 是否為首次執登
        /// </summary>
        [Display(Name = "是否為首次執登")]
        public string prt_apply_m { get; set; }

        /// <summary>
        /// 醫事證書 (字號)
        /// </summary>
        [Display(Name = "醫事證書 (字號)")]
        public string cer_doc_id { get; set; }

        /// <summary>
        /// 醫事證書 (第號)
        /// </summary>
        [Display(Name = "醫事證書 (第號)")]
        public string cer_doc_no { get; set; }

        /// <summary>
        /// 專科證書 (第號)
        /// </summary>
        [Display(Name = "專科證書 (第號)")]
        public string spc_sdw_no { get; set; }

        /// <summary>
        /// 專科證書有效期限起
        /// </summary>
        [Display(Name = "專科證書有效期限起")]
        public string spc_lic_bdate { get; set; }

        /// <summary>
        /// 專科證書有效期限迄
        /// </summary>
        [Display(Name = "專科證書有效期限迄")]
        public string spc_lic_edate { get; set; }

        /// <summary>
        /// 執業科別
        /// </summary>
        [Display(Name = "執業科別")]
        public string prt_dept_1 { get; set; }

        /// <summary>
        /// 執業執照 (字號)
        /// </summary>
        [Display(Name = "執業執照 (字號)")]
        public string prt_lic_id { get; set; }

        /// <summary>
        /// 執業執照 (第號)
        /// </summary>
        [Display(Name = "執業執照 (第號)")]
        public string prt_lic_no { get; set; }

        /// <summary>
        /// 執業執照起日
        /// </summary>
        [Display(Name = "執業執照起日")]
        public string prt_lic_bdate { get; set; }

        /// <summary>
        /// 執業執照迄日
        /// </summary>
        [Display(Name = "執業執照迄日")]
        public string prt_lic_edate { get; set; }

        /// <summary>
        /// 執業機構名稱
        /// </summary>
        [Display(Name = "執業機構名稱")]
        public string bas_name { get; set; }

        /// <summary>
        /// 機構地址郵遞區號
        /// </summary>
        [Display(Name = "機構地址郵遞區號")]
        public string zone_zip_code { get; set; }

        /// <summary>
        /// 機構地址
        /// </summary>
        [Display(Name = "機構地址")]
        public string bas_addr { get; set; }

        /// <summary>
        /// 執業機構電話
        /// </summary>
        [Display(Name = "執業機構電話")]
        public string bas_tel_no { get; set; }

        /// <summary>
        /// (醫事機構)負責人身分證字號
        /// </summary>
        [Display(Name = "負責人身分證字號")]
        public string bas_had_id_no { get; set; }

        /// <summary>
        /// (醫事機構)負責人姓名
        /// </summary>
        [Display(Name = "負責人姓名")]
        public string bas_had_name { get; set; }

        /// <summary>
        /// (醫事機構)負責人醫事證書(字)(代碼)
        /// </summary>
        [Display(Name = "負責人醫事證書(字)(代碼)")]
        public string bas_had_cer_id { get; set; }

        /// <summary>
        /// (醫事機構)負責人醫事證書(字)
        /// </summary>
        [Display(Name = "負責人醫事證書(字)")]
        public string bas_had_cer_name { get; set; }

        /// <summary>
        /// (醫事機構)負責人醫事證書(號)
        /// </summary>
        [Display(Name = "負責人醫事證書(號)")]
        public string bas_had_cer_no { get; set; }

        /// <summary> 
        /// 醫事機構
        /// 開業日期(起)
        /// </summary>
        [Display(Name = "開業日期(起)")]
        public string bas_lic_bdate { get; set; }

        /// <summary>
        /// 醫事機構
        /// 開業日期(迄)
        /// </summary>
        [Display(Name = "開業日期(迄)")]
        public string bas_lic_edate { get; set; }

        /// <summary>
        /// 醫事機構
        /// 停業日期(起)
        /// </summary>
        [Display(Name = "停業日期(起)")]
        public string bas_end_bdate { get; set; }

        /// <summary>
        /// 醫事機構
        /// 停業日期(迄)
        /// </summary>
        [Display(Name = "停業日期(迄)")]
        public string bas_end_edate { get; set; }

        /// <summary>
        /// (醫事機構)機構類別(代碼)(權屬別)
        /// </summary>
        public string author_id { get; set; }

        /// <summary>
        /// (醫事機構)機構類別(權屬別)
        /// </summary>
        public string author_name { get; set; }

        /// <summary>
        /// (醫事機構)機構類別(代碼)(型態別)
        /// </summary>
        [Display(Name = "機構類別(代碼)(型態別)")]
        public string type_id { get; set; }

        /// <summary>
        /// (醫事機構)機構類別(型態別)
        /// </summary>
        [Display(Name = "機構類別(型態別)")]
        public string type_name { get; set; }

        /// <summary>
        /// (醫事機構)機構代碼
        /// </summary>
        [Display(Name = "機構代碼")]
        public string bas_agency_id { get; set; }

        /// <summary>
        /// 停業日期(起)
        /// </summary>
        [Display(Name = "停業日期(起)")]
        public string upd_start_date { get; set; }

        /// <summary>
        /// 停業日期(迄)
        /// </summary>
        [Display(Name = "停業日期(迄)")]
        public string upd_end_date { get; set; }

        /// <summary> 
        /// 機構狀態(代碼)
        /// </summary>
        [Display(Name = "機構狀態(代碼)")]
        public string bas_status { get; set; }

        /// <summary>
        /// 機構狀態
        /// </summary>
        [Display(Name = "機構狀態")]
        public string bas_status_name { get; set; }

        /// <summary>
        /// (醫事機構)是否具管制藥品使用執照
        /// </summary>
        [Display(Name = "是否具管制藥品使用執照")]
        public string med_lic_status { get; set; }

        /// <summary>
        /// 案件申請時間
        /// </summary>
        [Display(Name = "案件申請時間")]
        public string apy_time { get; set; }

        /// <summary>
        /// 公會會員證字號
        /// </summary>
        [Display(Name = "公會會員證字號")]
        public string apy_member_id { get; set; }

        /// <summary>
        /// 案件申請人聯絡電話
        /// </summary>
        [Display(Name = "案件申請人聯絡電話")]
        public string apy_tel { get; set; }

        /// <summary>
        /// 案件申請人電子郵件
        /// </summary>
        [Display(Name = "案件申請人電子郵件")]
        public string apy_mail { get; set; }

        /// <summary>
        /// 登入方式
        /// </summary>
        [Display(Name = "登入方式")]
        public string apy_logintype { get; set; }

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
        /// 是否為代辦
        /// </summary>
        [Display(Name = "是否為代辦")]
        public string apy_agent { get; set; }

        /// <summary>
        /// 案件申請狀態
        /// </summary>
        [Display(Name = "案件申請狀態")]
        public string apy_status { get; set; }

        /// <summary>
        /// 案件申請狀態
        /// </summary>
        [Display(Name = "案件申請狀態")]
        public string apy_status_nm { get; set; }

        /// <summary>
        /// 案件處理有效日數
        /// </summary>
        [Display(Name = "案件處理有效日數")]
        public string apy_day { get; set; }

        /// <summary>
        /// 案件承辦人
        /// </summary>
        [Display(Name = "案件承辦人")]
        public string apy_undertaker { get; set; }

        /// <summary>
        /// 退件原因
        /// </summary>
        [Display(Name = "退件原因")]
        public string returndesc { get; set; }

        //目前得分總分
        [Display(Name = "目前得分總分")]
        public string POINT_SUM { get; set; }

        //是否符合換照資格
        [Display(Name = "是否符合換照資格")]
        public string IS_TAKE { get; set; }

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
            return StaticCodeMap.TableName.APPLY;
        }
    }
}