using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using log4net;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class MdodAction : BaseAction
    {
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleMdodLogger");

        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public MdodAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public MdodAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 危險性醫療儀器進口申請作業 (主檔)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001034Data(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, B.TAX_ORG_ID, B.TAX_ORG_NAME, B.TAX_ORG_ENAME, B.TAX_ORG_ADDR,
                    B.TAX_ORG_EADDR, B.TAX_ORG_MAN, B.TAX_ORG_TEL, B.TAX_ORG_EMAIL, B.TAX_ORG_FAX,
                    B.IM_EXPORT, B.DATE_S, B.DATE_E, '2' AS LIN_TYPE, B.DEST_STATE_ID,
                    B.SELL_STATE_ID, B.TRN_PORT_ID, B.BEG_PORT_ID, B.SELL_NAME, B.SELL_ADDR,
                    B.APP_USE_ID, B.USE_MARK, B.CONF_TYPE_ID, A.APP_TIME
                FROM APPLY A, APPLY_001034 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID = '001034'
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("APPNO", DataUtils.GetDBString(dr, n++)); // 申請文件編碼
                    item.Add("ORGAN", DataUtils.GetDBString(dr, n++)); // 統一編號
                    item.Add("ORGCHNNAM", DataUtils.GetDBString(dr, n++)); // 機構中文名稱
                    item.Add("EnglishName", DataUtils.GetDBString(dr, n++)); // 機構英文名稱
                    item.Add("ORGADD", DataUtils.GetDBString(dr, n++)); // 公司地址

                    item.Add("ORGEADD", DataUtils.GetDBString(dr, n++)); // 公司英文地址
                    item.Add("ORGNAM", DataUtils.GetDBString(dr, n++)); // 連絡人
                    item.Add("ORGTEL", DataUtils.GetDBString(dr, n++)); // 連絡人電話
                    item.Add("ORGMAIL", DataUtils.GetDBString(dr, n++)); // 連絡人E-MAIL
                    item.Add("ContactFaxNo", DataUtils.GetDBString(dr, n++)); // 聯絡人傳真號碼

                    item.Add("GETTYP", DataUtils.GetDBString(dr, n++)); // 進出口別

                    item.Add("BEGDTE", (DataUtils.GetDBDateTime(dr, n) == null) ? "" : ((DateTime)DataUtils.GetDBDateTime(dr, n)).ToString("yyyyMMdd")); // 起始日期
                    n++;
                    item.Add("ENDDTE", (DataUtils.GetDBDateTime(dr, n) == null) ? "" : ((DateTime)DataUtils.GetDBDateTime(dr, n)).ToString("yyyyMMdd")); // 終止日期
                    n++;
                    item.Add("LINTYP", DataUtils.GetDBString(dr, n++)); // 業務類別
                    item.Add("COUCOD", DataUtils.GetDBString(dr, n++)); // 目的國家代碼

                    item.Add("BUYCOU", DataUtils.GetDBString(dr, n++)); // 買/賣方國家代碼
                    item.Add("TRNPORT", DataUtils.GetDBString(dr, n++)); // 轉口港代碼
                    item.Add("BEGPORT", DataUtils.GetDBString(dr, n++)); // 起運口岸
                    item.Add("BUYNAM", DataUtils.GetDBString(dr, n++)); // 買/賣方英文名稱
                    item.Add("BUYADD", DataUtils.GetDBString(dr, n++)); // 買/賣方英文地址

                    item.Add("CHGCOD", DataUtils.GetDBString(dr, n++)); // 用途
                    item.Add("CHGREMARK", DataUtils.GetDBString(dr, n++)); // 用途說明
                    item.Add("ConfirmType", DataUtils.GetDBString(dr, n++)); // 核發方式

                    DateTime date = (DateTime)DataUtils.GetDBDateTime(dr, n++); // 申請時間
                    item.Add("APPDTE", date.ToString("MM dd yyyy")); // 申請日期
                    item.Add("APPTIME", date.ToString("HH:mm:ss")); // 申請時間

                    item.Add("APPTYP", "2");
                    item.Add("CHECKTYP", "0");

                    list.Add(item);
                    #endregion
                }
                dr.Close();
            }

            return list;
        }

        public bool UpdateFlag(List<Dictionary<string, string>> list)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("APP_ID", "");

            string updateSQL = @"
                UPDATE APPLY SET
                    TO_MIS_MK = 'Y',
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = 'SCH_MDOD',
                    UPD_ACC = ''
                WHERE APP_ID = @APP_ID;
            ";

            for (int i = 0; i < list.Count; i++)
            {
                args["APP_ID"] = list[i]["APPNO"];

                logger.Debug("APP_ID: " + list[i]["APPNO"]);

                Update(updateSQL, args);
            }
            return true;
        }

        /// <summary>
        /// 危險性醫療儀器進口申請作業 (檢附文件)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001034Doc(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, B.DOC_TYP_01, B.DOC_COD_01, B.DOC_TXT_01,
                    B.DOC_TYP_02, B.DOC_COD_02, B.DOC_TXT_02, B.DOC_TYP_03, B.DOC_COD_03, B.DOC_TXT_03,
                    B.DOC_TYP_04, B.DOC_COD_04, B.DOC_TXT_04, B.DOC_TYP_05, B.DOC_COD_05, B.DOC_TXT_05,
                    B.DOC_TYP_06, B.DOC_COD_06, B.DOC_TXT_06, B.DOC_TYP_07, B.DOC_COD_07, B.DOC_TXT_07,
                    B.DOC_TYP_08, B.DOC_COD_08, B.DOC_TXT_08, B.DOC_TYP_09, B.DOC_COD_09, B.DOC_TXT_09,
                    B.DOC_TYP_10, B.DOC_COD_10, B.DOC_TXT_10, B.DOC_TYP_11, B.DOC_COD_11, B.DOC_TXT_11,
                    B.DOC_TYP_12, B.DOC_COD_12, B.DOC_TXT_12, B.DOC_TYP_13, B.DOC_COD_13, B.DOC_TXT_13,
                    B.DOC_TYP_14, B.DOC_COD_14, B.DOC_TXT_14, B.DOC_TYP_15, B.DOC_COD_15, B.DOC_TXT_15,
                    B.DOC_TYP_16, B.DOC_COD_16, B.DOC_TXT_16, B.DOC_TYP_17, B.DOC_COD_17, B.DOC_TXT_17,
                    B.DOC_TYP_18, B.DOC_COD_18, B.DOC_TXT_18, B.DOC_TYP_19, B.DOC_COD_19, B.DOC_TXT_19
                FROM APPLY A, APPLY_001034_GOODS B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID = '001034'
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                int n = 1;
                string applyId = "";
                while (dr.Read())
                {
                    if (!applyId.Equals(DataUtils.GetDBString(dr, 0)))
                    {
                        applyId = DataUtils.GetDBString(dr, 0);
                        n = 1;
                    }

                    #region 取資料
                    for (int i = 0; i < 19; i++)
                    {
                        if (!String.IsNullOrEmpty(DataUtils.GetDBString(dr, (3 * i + 1))))
                        {
                            item = new Dictionary<string, string>();
                            item.Add("APPNO", DataUtils.GetDBString(dr, 0)); // 申請文件編碼
                            item.Add("SEQNO", (n++).ToString()); // 流水號
                            item.Add("DOCTYP", DataUtils.GetDBString(dr, (3 * i + 1))); // 檢附文件代碼檔
                            item.Add("DOCCOD", DataUtils.GetDBString(dr, (3 * i + 2))); // 檢附文件字號
                            item.Add("DOCTXT", DataUtils.GetDBString(dr, (3 * i + 3))); // 檢附文件說明
                            list.Add(item);
                        }
                    }
                    #endregion
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 危險性醫療儀器進口申請作業 (貨品資料)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001034Goods(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                  SELECT A.APP_ID, B.SRL_NO, '' as GOODS_SID, B.APPLY_CNT, B.GOODS_BRAND,
                    B.GOODS_NAME, B.GOODS_UNIT_ID, B.GOODS_SPEC_1 + '::' + B.GOODS_SPEC_2 AS GOODS_SPEC, B.GOODS_DESC, B.GOODS_MODEL
                FROM APPLY A, APPLY_001034_GOODS B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID in ('001034')
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("APPNO", DataUtils.GetDBString(dr, n++)); // 申請文件編碼
                    item.Add("SEQNO", DataUtils.GetDBInt(dr, n++).ToString()); // 流水號  
                    item.Add("ITEMNO", DataUtils.GetDBString(dr, n++)); // 貨品編碼
                    item.Add("APPNUM", DataUtils.GetDBInt(dr, n++).ToString()); // 數量[18,4]
                    item.Add("BRAND", DataUtils.GetDBString(dr, n++)); // 牌名

                    item.Add("GOODNAME", DataUtils.GetDBString(dr, n++)); // 貨品名稱
                    item.Add("UNIT", DataUtils.GetDBString(dr, n++)); // 單位
                    item.Add("Spec", DataUtils.GetDBString(dr, n++)); // 規格
                    item.Add("AdditionalGoodsDesc", DataUtils.GetDBString(dr, n++)); // 貨名輔助描述
                    item.Add("Model", DataUtils.GetDBString(dr, n++)); // 型號

                    list.Add(item);
                    #endregion
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 非感染性人體器官、組織及細胞進出口申請作業 (主檔)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001035Data(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, B.TAX_ORG_ID, B.TAX_ORG_NAME, B.TAX_ORG_ENAME, B.TAX_ORG_ADDR,
                    B.TAX_ORG_EADDR, B.TAX_ORG_MAN, B.TAX_ORG_TEL, B.TAX_ORG_EMAIL, B.TAX_ORG_FAX,
                    case B.IM_EXPORT when '0' then '1' else '2' end as GETTYP, B.DATE_S, B.DATE_E, '1' AS LIN_TYPE, B.DEST_STATE_ID,
                    B.SELL_STATE_ID, B.TRN_PORT_ID, B.BEG_PORT_ID, B.SELL_NAME, B.SELL_ADDR,
                    B.APP_USE_ID, B.USE_MARK, B.CONF_TYPE_ID, A.APP_TIME
                FROM APPLY A, APPLY_001035 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID in ('001035')
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            "; 
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("APPNO", DataUtils.GetDBString(dr, n++)); // 申請文件編碼
                    item.Add("ORGAN", DataUtils.GetDBString(dr, n++)); // 統一編號
                    item.Add("ORGCHNNAM", DataUtils.GetDBString(dr, n++)); // 機構中文名稱
                    item.Add("EnglishName", DataUtils.GetDBString(dr, n++)); // 機構英文名稱
                    item.Add("ORGADD", DataUtils.GetDBString(dr, n++)); // 公司地址

                    item.Add("ORGEADD", DataUtils.GetDBString(dr, n++)); // 公司英文地址
                    item.Add("ORGNAM", DataUtils.GetDBString(dr, n++)); // 連絡人
                    item.Add("ORGTEL", DataUtils.GetDBString(dr, n++)); // 連絡人電話
                    item.Add("ORGMAIL", DataUtils.GetDBString(dr, n++)); // 連絡人E-MAIL
                    item.Add("ContactFaxNo", DataUtils.GetDBString(dr, n++)); // 聯絡人傳真號碼

                    item.Add("GETTYP", DataUtils.GetDBString(dr, n++)); // 進出口別 系統0輸入,1輸出；食藥署1輸入,2輸出
                    item.Add("BEGDTE", ((DateTime)DataUtils.GetDBDateTime(dr, n++)).ToString("yyyyMMdd")); // 起始日期
                    item.Add("ENDDTE", ((DateTime)DataUtils.GetDBDateTime(dr, n++)).ToString("yyyyMMdd")); // 終止日期
                    item.Add("LINTYP", DataUtils.GetDBString(dr, n++)); // 業務類別
                    item.Add("COUCOD", DataUtils.GetDBString(dr, n++)); // 目的國家代碼

                    item.Add("BUYCOU", DataUtils.GetDBString(dr, n++)); // 買/賣方國家代碼
                    item.Add("TRNPORT", DataUtils.GetDBString(dr, n++)); // 轉口港代碼
                    item.Add("BEGPORT", DataUtils.GetDBString(dr, n++)); // 起運口岸
                    item.Add("BUYNAM", DataUtils.GetDBString(dr, n++)); // 買/賣方英文名稱
                    item.Add("BUYADD", DataUtils.GetDBString(dr, n++)); // 買/賣方英文地址

                    item.Add("CHGCOD", DataUtils.GetDBString(dr, n++)); // 用途
                    item.Add("CHGREMARK", DataUtils.GetDBString(dr, n++)); // 用途說明
                    item.Add("ConfirmType", DataUtils.GetDBString(dr, n++)); // 核發方式

                    DateTime date = (DateTime)DataUtils.GetDBDateTime(dr, n++); // 申請時間
                    item.Add("APPDTE", date.ToString("MM dd yyyy")); // 申請日期
                    item.Add("APPTIME", date.ToString("HH:mm:ss")); // 申請時間

                    item.Add("APPTYP", "2");
                    item.Add("CHECKTYP", "0");

                    list.Add(item);
                    #endregion
                }
                dr.Close();
            }

            return list;
        }
        /// <summary>
        /// 生殖細胞及胚胎輸入輸出申請作業申請作業 (主檔)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001038Data(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
            SELECT 
                A.APP_ID AS APPNO,
		        A.IDN AS ORGAN,
		        CASE B.IM_EXPORT WHEN '0' THEN ISNULL(E.TAI_UNITNAME,'') ELSE ISNULL(A.NAME,'') END AS ORGCHNNAM,
		        '' AS EnglishName,
                ISNULL(A.ADDR_CODE,'')+ISNULL(A.ADDR,'') AS ORGADD,
                A.EADDR AS ORGEADD,
		        CASE B.IM_EXPORT WHEN '0' THEN ISNULL(E.TAI_NAME,'') ELSE ISNULL(D.ORG_NAME,'') END AS ORGNAM,
		        CASE WHEN ISNULL(D.ORG_TEL,'') = '' THEN ISNULL(E.TAI_TEL,'') ELSE ISNULL(D.ORG_TEL,'') END AS ORGTEL,
		        CASE B.IM_EXPORT WHEN '0' THEN ISNULL(E.TAI_EMAIL,'') ELSE ISNULL(D.ORG_EMAIL,'') END AS ORGMAIL,
		        A.FAX AS ContactFaxNo,
                CASE B.IM_EXPORT WHEN '0' THEN '1' ELSE '2' END AS GETTYP,
		        B.DATE_S AS BEGDTE,
		        B.DATE_E AS ENDDTE,
		        '1' AS LINTYP,
                CASE B.IM_EXPORT WHEN '0' THEN 'TW' ELSE ISNULL(B.DEST_STATE_ID,'') END AS COUCOD,
		        CASE B.IM_EXPORT WHEN '0' THEN ISNULL(B.DEST_STATE_ID,'') ELSE ISNULL(B.SELL_STATE_ID,'') END AS BOUCOD,
		        B.TRN_PORT_ID AS TRNPORT,
                B.BEG_PORT_ID AS BEGPROT,
		        (CASE WHEN ISNULL(D.OTH_UNITNAME,'') = '' THEN ISNULL(E.ORG_UNITNAME,'') ELSE ISNULL(D.OTH_UNITNAME,'') END) + ' '+ (CASE WHEN ISNULL(D.OTH_TEL,'') = '' THEN ISNULL(E.ORG_TEL,'') ELSE ISNULL(D.OTH_TEL,'') END) AS BUYNAM,
		        CASE WHEN ISNULL(D.OTH_ADDR,'') = '' THEN ISNULL(E.TAI_ADDR,'') ELSE ISNULL(D.OTH_ADDR,'') END AS BUYADD,
		        B.APP_USE_ID AS CHGCOD,
                B.USE_MARK AS CHGREMARK,
		        B.CONF_TYPE_ID AS ConfirmType,
		        A.APP_TIME,
                B.TAX_ORG_ID AS AgentID,
		        B.TAX_ORG_NAME AS AgentChineseName,
		        B.TAX_ORG_ENAME AS AgentEnglishName, 
                B.TAX_ORG_ADDR AS AgentChineseAddr,
                B.TAX_ORG_EADDR AS AgentEnglishAddr, 
		        B.TAX_ORG_MAN AS AgentContactPerson,
		        B.TAX_ORG_TEL AS AgentTelNo,
                B.TAX_ORG_EMAIL AS AgentEmail, 
		        B.TAX_ORG_FAX AS AgentFaxNo
                FROM APPLY A
		        LEFT JOIN APPLY_001038 B ON A.APP_ID = B.APP_ID
		        LEFT JOIN MEMBER C ON A.ACC_NO = C.ACC_NO
		        LEFT JOIN APPLY_001038_SELL D ON A.APP_ID = D.APP_ID
		        LEFT JOIN APPLY_001038_DEST E ON A.APP_ID = E.APP_ID
                WHERE 1=1
                AND A.SRV_ID IN ('001038')
                AND A.APP_TIME < @APP_TIME
                AND A.TO_MIS_MK = 'N'
                AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("APPNO", DataUtils.GetDBString(dr, n++)); // 申請文件編碼
                    item.Add("ORGAN", DataUtils.GetDBString(dr, n++)); // 統一編號
                    item.Add("ORGCHNNAM", DataUtils.GetDBString(dr, n++)); // 機構中文名稱
                    item.Add("EnglishName", DataUtils.GetDBString(dr, n++)); // 機構英文名稱
                    item.Add("ORGADD", DataUtils.GetDBString(dr, n++)); // 公司地址

                    item.Add("ORGEADD", DataUtils.GetDBString(dr, n++)); // 公司英文地址
                    item.Add("ORGNAM", DataUtils.GetDBString(dr, n++)); // 連絡人
                    item.Add("ORGTEL", DataUtils.GetDBString(dr, n++)); // 連絡人電話
                    item.Add("ORGMAIL", DataUtils.GetDBString(dr, n++)); // 連絡人E-MAIL
                    item.Add("ContactFaxNo", DataUtils.GetDBString(dr, n++)); // 聯絡人傳真號碼

                    item.Add("GETTYP", DataUtils.GetDBString(dr, n++)); // 進出口別 系統0輸入,1輸出；食藥署1輸入,2輸出
                    item.Add("BEGDTE", ((DateTime)DataUtils.GetDBDateTime(dr, n++)).ToString("yyyyMMdd")); // 起始日期
                    item.Add("ENDDTE", ((DateTime)DataUtils.GetDBDateTime(dr, n++)).ToString("yyyyMMdd")); // 終止日期
                    item.Add("LINTYP", DataUtils.GetDBString(dr, n++)); // 業務類別
                    item.Add("COUCOD", DataUtils.GetDBString(dr, n++)); // 目的國家代碼

                    item.Add("BUYCOU", DataUtils.GetDBString(dr, n++)); // 買/賣方國家代碼
                    item.Add("TRNPORT", DataUtils.GetDBString(dr, n++)); // 轉口港代碼
                    item.Add("BEGPORT", DataUtils.GetDBString(dr, n++)); // 起運口岸
                    item.Add("BUYNAM", DataUtils.GetDBString(dr, n++)); // 買/賣方英文名稱
                    item.Add("BUYADD", DataUtils.GetDBString(dr, n++)); // 買/賣方英文地址

                    item.Add("CHGCOD", DataUtils.GetDBString(dr, n++)); // 用途
                    item.Add("CHGREMARK", DataUtils.GetDBString(dr, n++)); // 用途說明
                    item.Add("ConfirmType", DataUtils.GetDBString(dr, n++)); // 核發方式

                    DateTime date = (DateTime)DataUtils.GetDBDateTime(dr, n++); // 申請時間
                    item.Add("APPDTE", date.ToString("MM dd yyyy")); // 申請日期
                    item.Add("APPTIME", date.ToString("HH:mm:ss")); // 申請時間

                    item.Add("AgentID", DataUtils.GetDBString(dr, n++)); // 申辦代理人身分證或統編
                    item.Add("AgentChineseName", DataUtils.GetDBString(dr, n++)); // 申辦代理人姓名或公司名稱(中文
                    item.Add("AgentEnglishName", DataUtils.GetDBString(dr, n++)); // 申辦代理人姓名或公司名稱(英文
                    item.Add("AgentChineseAddr", DataUtils.GetDBString(dr, n++)); // 申辦代理人聯絡地址(中文
                    item.Add("AgentEnglishAddr", DataUtils.GetDBString(dr, n++)); // 申辦代理人聯絡地址(英文
                    item.Add("AgentContactPerson", DataUtils.GetDBString(dr, n++)); // 申辦代理人聯絡者姓名
                    item.Add("AgentTelNo", DataUtils.GetDBString(dr, n++)); // 申辦代理人聯絡人電話
                    item.Add("AgentEmail", DataUtils.GetDBString(dr, n++)); // 申辦代理人聯絡人傳真
                    item.Add("AgentFaxNo", DataUtils.GetDBString(dr, n++)); // 申辦代理人email

                    item.Add("APPTYP", "2");
                    item.Add("CHECKTYP", "0");

                    list.Add(item);
                    #endregion
                }
                dr.Close();
            }

            return list;
        }
        /// <summary>
        /// 非感染性人體器官、組織及細胞進出口申請作業 (檢附文件)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001035Doc(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, 
                	(CASE WHEN ISNULL(B.DOC_COD_01, '') <> '' OR ISNULL(B.DOC_TXT_01, '') <> '' THEN 'B11' ELSE B.DOC_TYP_01 END ) AS DOC_TYP_01,
                	B.DOC_COD_01, B.DOC_TXT_01,
                	(CASE WHEN ISNULL(B.DOC_COD_02, '') <> '' OR ISNULL(B.DOC_TXT_02, '') <> '' THEN 'B12' ELSE B.DOC_TYP_02 END ) AS DOC_TYP_02,
                    B.DOC_COD_02, B.DOC_TXT_02, 
                    (CASE WHEN ISNULL(B.DOC_COD_03, '') <> '' OR ISNULL(B.DOC_TXT_03, '') <> '' THEN 'B13' ELSE B.DOC_TYP_03 END ) AS DOC_TYP_03,
                    B.DOC_COD_03, B.DOC_TXT_03,
                    (CASE WHEN ISNULL(B.DOC_COD_04, '') <> '' OR ISNULL(B.DOC_TXT_04, '') <> '' THEN 'B14' ELSE B.DOC_TYP_04 END ) AS DOC_TYP_04,
                    B.DOC_COD_04, B.DOC_TXT_04, 
                    (CASE WHEN ISNULL(B.DOC_COD_05, '') <> '' OR ISNULL(B.DOC_TXT_05, '') <> '' THEN 'B15' ELSE B.DOC_TYP_05 END ) AS DOC_TYP_05,
                    B.DOC_COD_05, B.DOC_TXT_05,
                    (CASE WHEN ISNULL(B.DOC_COD_06, '') <> '' OR ISNULL(B.DOC_TXT_06, '') <> '' THEN 'B16' ELSE B.DOC_TYP_06 END ) AS DOC_TYP_06,
                    B.DOC_COD_06, B.DOC_TXT_06, 
                    (CASE WHEN ISNULL(B.DOC_COD_07, '') <> '' OR ISNULL(B.DOC_TXT_07, '') <> '' THEN 'B17' ELSE B.DOC_TYP_07 END ) AS DOC_TYP_07,
                    B.DOC_COD_07, B.DOC_TXT_07,
                    (CASE WHEN ISNULL(B.DOC_COD_08, '') <> '' OR ISNULL(B.DOC_TXT_08, '') <> '' THEN 'B18' ELSE B.DOC_TYP_08 END ) AS DOC_TYP_08,
                    B.DOC_COD_08, B.DOC_TXT_08
                FROM APPLY A, APPLY_001035 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID in  ('001035','001038')
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n=1;
                    for (int i = 0; i < 8; i++)
                    {
                        if (!String.IsNullOrEmpty(DataUtils.GetDBString(dr, (3 * i + 1))))
                        {
                            item = new Dictionary<string, string>();
                            item.Add("APPNO", DataUtils.GetDBString(dr, 0)); // 申請文件編碼
                            item.Add("SEQNO", (n++).ToString()); // 流水號
                            item.Add("DOCTYP", DataUtils.GetDBString(dr, (3 * i + 1))); // 檢附文件代碼檔
                            item.Add("DOCCOD", DataUtils.GetDBString(dr, (3 * i + 2))); // 檢附文件字號
                            item.Add("DOCTXT", DataUtils.GetDBString(dr, (3 * i + 3))); // 檢附文件說明
                            list.Add(item);
                        }
                    }
                    #endregion
                }
                dr.Close();
            }

            return list;
        }
        /// <summary>
        /// 非感染性人體器官、組織及細胞進出口申請作業 (檢附文件)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001038Doc(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, 
                	(CASE WHEN ISNULL(B.DOC_COD_01, '') <> '' OR ISNULL(B.DOC_TXT_01, '') <> '' THEN 'B11' ELSE B.DOC_TYP_01 END ) AS DOC_TYP_01,
                	B.DOC_COD_01, B.DOC_TXT_01,
                	(CASE WHEN ISNULL(B.DOC_COD_02, '') <> '' OR ISNULL(B.DOC_TXT_02, '') <> '' THEN 'B12' ELSE B.DOC_TYP_02 END ) AS DOC_TYP_02,
                    B.DOC_COD_02, B.DOC_TXT_02, 
                    (CASE WHEN ISNULL(B.DOC_COD_03, '') <> '' OR ISNULL(B.DOC_TXT_03, '') <> '' THEN 'B13' ELSE B.DOC_TYP_03 END ) AS DOC_TYP_03,
                    B.DOC_COD_03, B.DOC_TXT_03,
                    (CASE WHEN ISNULL(B.DOC_COD_04, '') <> '' OR ISNULL(B.DOC_TXT_04, '') <> '' THEN 'B14' ELSE B.DOC_TYP_04 END ) AS DOC_TYP_04,
                    B.DOC_COD_04, B.DOC_TXT_04, 
                    (CASE WHEN ISNULL(B.DOC_COD_05, '') <> '' OR ISNULL(B.DOC_TXT_05, '') <> '' THEN 'B15' ELSE B.DOC_TYP_05 END ) AS DOC_TYP_05,
                    B.DOC_COD_05, B.DOC_TXT_05,
                    (CASE WHEN ISNULL(B.DOC_COD_06, '') <> '' OR ISNULL(B.DOC_TXT_06, '') <> '' THEN 'B16' ELSE B.DOC_TYP_06 END ) AS DOC_TYP_06,
                    B.DOC_COD_06, B.DOC_TXT_06, 
                    (CASE WHEN ISNULL(B.DOC_COD_07, '') <> '' OR ISNULL(B.DOC_TXT_07, '') <> '' THEN 'B17' ELSE B.DOC_TYP_07 END ) AS DOC_TYP_07,
                    B.DOC_COD_07, B.DOC_TXT_07,
                    (CASE WHEN ISNULL(B.DOC_COD_08, '') <> '' OR ISNULL(B.DOC_TXT_08, '') <> '' THEN 'B18' ELSE B.DOC_TYP_08 END ) AS DOC_TYP_08,
                    B.DOC_COD_08, B.DOC_TXT_08
                FROM APPLY A, APPLY_001038 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID in  ('001035','001038')
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 1;
                    for (int i = 0; i < 8; i++)
                    {
                        if (!String.IsNullOrEmpty(DataUtils.GetDBString(dr, (3 * i + 1))))
                        {
                            item = new Dictionary<string, string>();
                            item.Add("APPNO", DataUtils.GetDBString(dr, 0)); // 申請文件編碼
                            item.Add("SEQNO", (n++).ToString()); // 流水號
                            item.Add("DOCTYP", DataUtils.GetDBString(dr, (3 * i + 1))); // 檢附文件代碼檔
                            item.Add("DOCCOD", DataUtils.GetDBString(dr, (3 * i + 2))); // 檢附文件字號
                            item.Add("DOCTXT", DataUtils.GetDBString(dr, (3 * i + 3))); // 檢附文件說明
                            list.Add(item);
                        }
                    }
                    #endregion
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 非感染性人體器官、組織及細胞進出口申請作業 (貨品資料)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001035Goods(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, B.SRL_NO, B.GOODS_SID, B.APPLY_CNT, B.GOODS_BRAND,
                    B.GOODS_NAME, B.GOODS_UNIT_ID, B.GOODS_SPEC_1 + '::' + B.GOODS_SPEC_2 AS GOODS_SPEC, B.GOODS_DESC, B.GOODS_MODEL
                FROM APPLY A, APPLY_001035_GOODS B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID in ('001035','001038')
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            "; 
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("APPNO", DataUtils.GetDBString(dr, n++)); // 申請文件編碼
                    item.Add("SEQNO", DataUtils.GetDBInt(dr, n++).ToString()); // 流水號
                    item.Add("ITEMNO", DataUtils.GetDBString(dr, n++)); // 貨品編碼
                    item.Add("APPNUM", DataUtils.GetDBInt(dr, n++).ToString()); // 數量[18,4]
                    item.Add("BRAND", DataUtils.GetDBString(dr, n++)); // 牌名

                    item.Add("GOODNAME", DataUtils.GetDBString(dr, n++)); // 貨品名稱
                    item.Add("UNIT", DataUtils.GetDBString(dr, n++)); // 單位
                    item.Add("Spec", DataUtils.GetDBString(dr, n++)); // 規格
                    item.Add("AdditionalGoodsDesc", DataUtils.GetDBString(dr, n++)); // 貨名輔助描述
                    item.Add("Model", DataUtils.GetDBString(dr, n++)); // 型號

                    list.Add(item); 
                    #endregion
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 非感染性人體器官、組織及細胞進出口申請作業 (貨品資料)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001038Goods(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, B.SRL_NO, B.GOODS_SID, B.APPLY_CNT, B.GOODS_BRAND,
                    B.GOODS_NAME, B.GOODS_SPEC_1 AS UNIT, ISNULL(B.GOODS_TYPE_ID,'')+'::'+ISNULL(B.GOODS_SPEC_2,'') AS GOODS_SPEC, B.GOODS_DESC, B.GOODS_MODEL
                FROM APPLY A, APPLY_001038_GOODS B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID in ('001035','001038')
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("APPNO", DataUtils.GetDBString(dr, n++)); // 申請文件編碼
                    item.Add("SEQNO", DataUtils.GetDBInt(dr, n++).ToString()); // 流水號
                    item.Add("ITEMNO", DataUtils.GetDBString(dr, n++)); // 貨品編碼
                    item.Add("APPNUM", DataUtils.GetDBInt(dr, n++).ToString()); // 數量[18,4]
                    item.Add("BRAND", DataUtils.GetDBString(dr, n++)); // 牌名

                    item.Add("GOODNAME", DataUtils.GetDBString(dr, n++)); // 貨品名稱
                    item.Add("UNIT", DataUtils.GetDBString(dr, n++)); // 單位
                    item.Add("Spec", DataUtils.GetDBString(dr, n++)); // 規格
                    item.Add("AdditionalGoodsDesc", DataUtils.GetDBString(dr, n++)); // 貨名輔助描述
                    item.Add("Model", DataUtils.GetDBString(dr, n++)); // 型號

                    list.Add(item);
                    #endregion
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 非感染性人體器官、組織及細胞進出口申請作業 (上傳附件)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> Get001035File(DateTime dt)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                SELECT A.APP_ID, B.FILE_NO, B.FILENAME
                FROM APPLY A, APPLY_FILE B
                WHERE A.APP_ID = B.APP_ID
                    AND A.SRV_ID in ('001035','001038','0001034')
                    AND A.APP_TIME < @APP_TIME
                    AND A.TO_MIS_MK = 'N'
                    AND A.CLOSE_MK = 'N'
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("APPNO", DataUtils.GetDBString(dr, n++)); // 申請文件編碼
                    item.Add("SEQNO", DataUtils.GetDBInt(dr, n++).ToString()); // 流水號
                    item.Add("FILE", DataUtils.GetDBString(dr, n++)); // 檔案名稱

                    list.Add(item);
                    #endregion
                }
                dr.Close();
            }

            return list;
        }
    }
}