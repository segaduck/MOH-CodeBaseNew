using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Commons;

namespace EECOnline.Commons
{
    /// <summary>
    /// 系統代碼及表格名稱列舉
    /// </summary>
    public partial class StaticCodeMap
    {
        /// <summary>
        /// 代碼表類別列舉清單, 叫用 KeyMapDAO.GetCodeMapList() 所需的參數
        /// </summary>
        public class CodeMap : CodeMapType
        {
            #region 私有(隱藏) CodeMap 建構式

            private CodeMap(string codeName) : base(codeName)
            { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="codeName"></param>
            /// <param name="sqlStatementId"></param>
            private CodeMap(string codeName, string sqlStatementId) :
                base(codeName, sqlStatementId)
            { }

            #endregion 私有(隱藏) CodeMap 建構式

            /// <summary>
            /// 前台申辦案件縣市別清單
            /// </summary>
            public static CodeMapType E_SRV_CITY = new CodeMapType("E_SRV_CITY", "KeyMap.getE_SRV_CITY");

            /// <summary>
            /// 系統名稱清單
            /// </summary>
            public static CodeMapType sysid = new CodeMapType("sysid", "KeyMap.getSYSID");

            /// <summary>
            /// 模組名稱清單
            /// </summary>
            public static CodeMapType modules = new CodeMapType("modules", "KeyMap.getMODULES");

            /// <summary>
            /// 單位資料
            /// </summary>
            public static CodeMapType unit = new CodeMapType("unit", "KeyMap.getUNIT");

            /// <summary>
            /// 單位資料
            /// </summary>
            public static CodeMapType unit1 = new CodeMapType("unit1", "KeyMap.getUNIT1");

            /// <summary>
            /// 功能群組
            /// </summary>
            public static CodeMapType amgrp = new CodeMapType("amgrp", "KeyMap.getAMGRP");

            /// <summary>
            /// 申辦項目
            /// </summary>
            public static CodeMapType apply = new CodeMapType("apply", "KeyMap.getApply");

            /// <summary>
            /// 申辦縣市
            /// </summary>
            public static CodeMapType srv_city = new CodeMapType("srv_city", "KeyMap.getSrv_City");

            /// <summary>
            /// 申辦縣市
            /// </summary>
            public static CodeMapType srv_city_unit = new CodeMapType("srv_city_unit", "KeyMap.getSrv_City_Unit");

            /// <summary>
            /// 申辦縣市
            /// </summary>
            public static CodeMapType srv_city_parm = new CodeMapType("srv_city_parm", "KeyMap.getSrv_City_Parm");

            /// <summary>
            /// 申辦縣市轄區
            /// </summary>
            public static CodeMapType srv_city_s_parm = new CodeMapType("srv_city_s_parm", "KeyMap.getsrv_city_s_parm");

            /// <summary>
            /// 狀態所屬種類
            /// </summary>
            public static CodeMapType applyhardtype = new CodeMapType("applyhardtype", "KeyMap.getApplyhardtype");

            /// <summary>
            /// 處理狀態
            /// </summary>
            public static CodeMapType srv_status = new CodeMapType("srv_status", "KeyMap.getSrv_status");

            /// <summary>
            /// 處理狀態
            /// </summary>
            public static CodeMapType srv_status1 = new CodeMapType("srv_status1", "KeyMap.getSrv_status1");

            /// <summary>
            /// 承辦人
            /// </summary>
            public static CodeMapType apy_undertaker = new CodeMapType("apy_undertaker", "KeyMap.getApy_undertaker");

            /// <summary>
            /// 公告類型
            /// </summary>
            public static CodeMapType enews = new CodeMapType("enews", "KeyMap.getEnews");

            /// <summary>
            /// 常見問題類型
            /// </summary>
            public static CodeMapType code_name = new CodeMapType("code_name", "KeyMap.getfaq");

            /// <summary>
            /// 常見問題類型(不顯示停用)
            /// </summary>
            public static CodeMapType code_name1 = new CodeMapType("code_name1", "KeyMap.getfaq1");

            /// <summary>
            /// 最新公告上架狀態
            /// </summary>
            public static CodeMapType status = new CodeMapType("status", "KeyMap.getEnews");

            /// <summary>
            /// 機構類別
            /// </summary>
            public static CodeMapType Type_id = new CodeMapType("Type_id", "KeyMap.getType_id");

            /// <summary>
            /// 機構類別
            /// </summary>
            public static CodeMapType StatuList = new CodeMapType("StatuList", "KeyMap.getStatuList");

            /// <summary>
            /// 登記事項
            /// </summary>
            public static CodeMapType Apy_change = new CodeMapType("Apy_change", "KeyMap.getApy_change");

            /// <summary>
            /// 病床類型
            /// </summary>
            public static CodeMapType Bed_type = new CodeMapType("Bed_type", "KeyMap.getBed_type");

            /// <summary>
            /// 病床類型
            /// </summary>
            public static CodeMapType Bed_kind = new CodeMapType("Bed_kind", "KeyMap.getBed_kind");

            /// <summary>
            /// 執業科別(限院所)
            /// </summary>
            public static CodeMapType PrtDept = new CodeMapType("PrtDept", "KeyMap.getPrtDept");

            /// <summary>
            /// 執業科別(全部)
            /// </summary>
            public static CodeMapType DcdDept = new CodeMapType("DcdDept", "KeyMap.getDcdDept");

            /// <summary>
            /// 執業科別(全部)
            /// </summary>
            public static CodeMapType NDcdDept = new CodeMapType("NDcdDept", "KeyMap.getNDcdDept");

            /// <summary>
            /// 權屬別(全部)
            /// </summary>
            public static CodeMapType AUTHOR = new CodeMapType("AUTHOR", "KeyMap.getAUTHOR");

            /// <summary>
            /// 縣市清單
            /// </summary>
            public static CodeMapType Zip_City = new CodeMapType("Zip_City", "KeyMap.getZip_City");

            /// <summary>
            /// 鄉鎮區清單
            /// </summary>
            public static CodeMapType Zip_Town = new CodeMapType("Zip_Town", "KeyMap.getZip_Town");

            /// <summary>
            /// 街道清單
            /// </summary>
            public static CodeMapType Zip_Road = new CodeMapType("Zip_Road", "KeyMap.getZip_Road");

            /// <summary>
            /// 領取地址清單(申辦查詢)
            /// </summary>
            public static CodeMapType Get_Body_p = new CodeMapType("Get_Body_p", "KeyMap.getBody_p");

            /// <summary>
            /// 領取地址清單(填寫申辦)
            /// </summary>
            public static CodeMapType Get_Body_w = new CodeMapType("Get_Body_w", "KeyMap.getBody_w");

            /// <summary>
            /// 畢業證書
            /// </summary>
            public static CodeMapType Get_QUAL = new CodeMapType("Get_QUAL", "KeyMap.get_QUAL");

            /// <summary>
            /// 申請資格
            /// </summary>
            public static CodeMapType Get_APPLYQUAL = new CodeMapType("Get_APPLYQUAL", "KeyMap.get_APPLYQUAL");

            /// <summary>
            /// 學校
            /// </summary>
            public static CodeMapType Get_SCHOOLID = new CodeMapType("Get_SCHOOLID", "KeyMap.get_SCHOOLID");

            /// <summary>
            /// 科系
            /// </summary>
            public static CodeMapType Get_UCODE = new CodeMapType("Get_UCODE", "KeyMap.get_UCODE");

            /// <summary>
            /// 課程代碼
            /// </summary>
            public static CodeMapType Get_COURSEID = new CodeMapType("Get_COURSEID", "KeyMap.get_COURSEID");

            /// <summary>
            /// 職類機構代碼
            /// </summary>
            public static CodeMapType Get_CODE_WORKHISTORY = new CodeMapType("Get_CODE_WORKHISTORY", "KeyMap.get_CODE_WORKHISTORY");

            /// <summary>
            /// 職稱或職務代碼
            /// </summary>
            public static CodeMapType Get_CODE_WORKHISTORYDET = new CodeMapType("Get_CODE_WORKHISTORYDET", "KeyMap.get_CODE_WORKHISTORYDET");

            /// <summary>
            /// 實際工作內容
            /// </summary>
            public static CodeMapType Get_CODE_WORKHISTORYPART = new CodeMapType("Get_CODE_WORKHISTORYPART", "KeyMap.get_CODE_WORKHISTORYPART");

            /// <summary>
            /// 醫院清單
            /// </summary>
            public static CodeMapType Get_Hospital = new CodeMapType("Get_Hospital", "KeyMap.get_Hospital");

            /// <summary>
            /// 病歷類型 (全顯示)
            /// </summary>
            public static CodeMapType Get_HIS_Type_All = new CodeMapType("Get_HIS_Type_All", "KeyMap.get_HIS_Type_All");

            /// <summary>
            /// 病歷類型 (僅顯示有效期內，須傳入醫院代號)
            /// </summary>
            public static CodeMapType Get_HIS_Type_Valid = new CodeMapType("Get_HIS_Type_Valid", "KeyMap.get_HIS_Type_Valid");

            /// <summary>
            /// 病歷類型 取單價 (僅顯示有效期內，須傳入醫院代號)
            /// </summary>
            public static CodeMapType Get_HIS_Type_Valid_Price = new CodeMapType("Get_HIS_Type_Valid_Price", "KeyMap.get_HIS_Type_Valid_Price");

            /// <summary>
            /// 1: 自然人憑證登入<br />
            /// 2: 行動自然人憑證登入 (TW FidO)<br />
            /// 3: 身分證字號 + 健保卡
            /// </summary>
            public static CodeMapType Get_login_type = new CodeMapType("Get_login_type", "KeyMap.get_login_type");

            /// <summary>
            /// 帳務月份列表 - 依照申請訂單 createdatetime 為主
            /// </summary>
            public static CodeMapType Get_AccountingYM = new CodeMapType("Get_AccountingYM", "KeyMap.get_AccountingYM");
        }
    }
}