using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Models.Share;
using Omu.ValueInjecter;
using System.Collections;
using ES.Models;
using System.Configuration;
using System.Data.SqlClient;
using ES.Action;
using ES.Utils;
using System.Text;
using System.Data;
using System.Web.Mvc;
using ES.Commons;
using Dapper;
using ES.Services;

namespace ES.DataLayers
{
    public class MyKeyMapDAO : BaseAction
    {
        /// <summary>
        /// 國家清單
        /// </summary>
        public IList<KeyMapModel> GetCountryMapList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F1_COUNTRY_1' ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        /// <summary>
        /// 國家清單 優先順序
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> GetCountry2MapList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F1_COUNTRY_2' ORDER BY SEQ_NO");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 港口
        /// </summary>
        public IList<KeyMapModel> GetPortMapList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F1_PORT_1' AND CODE_PCD='' ");
            sql.Append(" ORDER BY CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 港口 優先順序
        /// </summary>
        public IList<KeyMapModel> GetPort2MapList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F1_PORT_2' AND CODE_PCD='' ");
            sql.Append(" ORDER BY SEQ_NO ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 岸口
        /// </summary>
        public IList<KeyMapModel> GetHarborMapList(string port)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@port", port }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F1_PORT_1' AND CODE_PCD=@port ");
            sql.Append(" ORDER BY CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 岸口
        /// </summary>
        public IList<KeyMapModel> GetHarborMapList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F1_PORT_1' AND CODE_PCD<>'' ");
            sql.Append(" ORDER BY CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 進口關 捨棄 db為主
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> GetLocalPortList()
        {
            //List<KeyMapModel> result = new List<KeyMapModel>();
            //result.Add(new KeyMapModel { CODE = "A", TEXT = "基隆關" });
            //result.Add(new KeyMapModel { CODE = "B", TEXT = "高雄關" });
            //result.Add(new KeyMapModel { CODE = "D", TEXT = "臺北關(如桃園機場)" });
            //result.Add(new KeyMapModel { CODE = "E", TEXT = "臺中關" });

            //return result;

            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE, CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F5_vw_IMPORT' ");
            sql.Append(" ORDER BY CODE_CD ");
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
            }
            return result;
        }

        /// <summary>
        /// 公用清單
        /// </summary>
        public IList<KeyMapModel> GetCodeList(string CODE_KIND)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", CODE_KIND }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");

            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        /// <summary>
        /// 公用清單 code_memo
        /// </summary>
        public IList<KeyMapModel> GetCodeMemoList(string CODE_KIND)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", CODE_KIND }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_MEMO AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");

            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        /// <summary>
        /// 案件狀態(for 醫事司用)
        /// </summary>
        public IList<KeyMapModel> GetDEPCaseStatusList(string UNIT_CD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@UNIT_CD", UNIT_CD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE DEL_MK = 'N' ");
            sql.Append(" AND CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = @UNIT_CD) ");
            sql.Append(" AND CODE_KIND = 'F_CASE_STATUS' ");
            sql.Append(" AND CODE_CD IN ('1','--','2','3','0','8','9','12','15') ");
            sql.Append(" ORDER BY SEQ_NO ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        ///查詢 幼婦組專科護理師案件狀態
        /// </summary>
        public IList<KeyMapModel> GetStatuListForNurse(string UNIT_CD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@UNIT_CD", UNIT_CD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE DEL_MK = 'N' ");
            sql.Append(" AND CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = @UNIT_CD) ");
            sql.Append(" AND CODE_KIND = 'F_CASE_STATUS' ");
            sql.Append(" AND CODE_CD IN ('1','--','2','3','0','9','12','51','52') ");
            sql.Append(" ORDER BY SEQ_NO ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }


        /// <summary>
        /// 案件狀態
        /// </summary>
        public IList<KeyMapModel> GetCaseStatusList(string UNIT_CD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@UNIT_CD", UNIT_CD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE DEL_MK = 'N' ");
            sql.Append(" AND CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = @UNIT_CD) ");
            sql.Append(" AND CODE_KIND = 'F_CASE_STATUS' ");
            sql.Append(" ORDER BY SEQ_NO ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 案件狀態(取CODE_MEMO)
        /// </summary>
        public IList<KeyMapModel> GetCaseMemoStatusList(string UNIT_CD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@UNIT_CD", UNIT_CD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT CODE_CD AS CODE,CODE_MEMO AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE DEL_MK = 'N' ");
            sql.Append(" AND CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = @UNIT_CD) ");
            sql.Append(" AND CODE_KIND = 'F_CASE_STATUS' AND CODE_CD <> '4' ");
            sql.Append(" ORDER BY SEQ_NO ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 案件狀態(以CODE_PCD取列表)
        /// </summary>
        public IList<KeyMapModel> GetCaseStatusPCDList(string CODE_PCD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_PCD", CODE_PCD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE DEL_MK = 'N' ");
            sql.Append(" AND CODE_PCD IN(@CODE_PCD) ");
            sql.Append(" AND CODE_KIND = 'F_CASE_STATUS' ");
            sql.Append(" ORDER BY SEQ_NO ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 科別
        /// </summary>
        public IList<KeyMapModel> GetDivisionList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_2" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" AND CODE_PCD='' ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 科別 001007
        /// </summary>
        public IList<KeyMapModel> GetDivisionList001007()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_2" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" AND CODE_PCD = '' ");
            //sql.Append(" AND CODE_CD NOT IN ('C0700','C0701','C0900') ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 證書類別
        /// </summary>
        public IList<KeyMapModel> GetDivisionCertList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_3" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" AND CODE_PCD='' ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 證書(專科醫師證書補（換）發)
        /// </summary>
        public IList<KeyMapModel> GetLicNumList(string CODE_PCD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_2" },
                { "@CODE_PCD", CODE_PCD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" AND CODE_PCD =@CODE_PCD ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 證書類別
        /// </summary>
        public IList<KeyMapModel> GetLicNumCertList(string CODE_PCD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_3" },
                { "@CODE_PCD", CODE_PCD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" AND CODE_PCD =@CODE_PCD ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetLicNumCertList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_3" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetLicNumCertMDList(string CODE_PCD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_1" },
                { "@CODE_PCD", CODE_PCD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" AND CODE_PCD =@CODE_PCD ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
            }

            return result;
        }

        public IList<KeyMapModel> GetLicNumCertMDList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_1" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND = @CODE_KIND AND CODE_PCD='' ");
            sql.Append(" AND ISNULL(CODE_PCD,'')='' ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetLicNumCertMDByPCDList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_1" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT CODE_PCD AS CODE, CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND AND CODE_PCD<>'' ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetLicIssueDeptList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_ISSUE_DEPT_1" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 證書(專科醫師證書補（換）發)
        /// </summary>
        public IList<KeyMapModel> GetLicNumList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_KIND", "F1_LICENSE_CD_2" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND =@CODE_KIND ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 英文藥商許可執照字號
        /// </summary>
        public IList<KeyMapModel> GetPLList_EN()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_CD AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F5_LIC_CD' ");
            sql.Append(" ORDER BY SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetDonateCodeList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var parameters = new DynamicParameters();
            sql.Append(" SELECT  SRV_ID_DONATE AS CODE,NAME_CH AS TEXT ");
            sql.Append(" FROM APPLY_DONATE ");
            sql.Append(" WHERE DEL_MK = 'N' ");
            sql.Append(" ORDER BY SRV_ID_DONATE ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetUnitList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var parameters = new DynamicParameters();
            sql.Append(" SELECT  UNIT_CD AS CODE,UNIT_NAME AS TEXT ");
            sql.Append(" FROM UNIT ");
            sql.Append(" WHERE UNIT_SCD IN(SELECT CODE_PCD FROM CODE_CD WHERE DEL_MK = 'N' AND CODE_KIND = 'F_CASE_STATUS') ");
            sql.Append(" ORDER BY UNIT_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetFlowCDListByUnit(string unit_cd)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@UNIT_CD", unit_cd }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE DEL_MK = 'N' AND CODE_KIND = 'F_CASE_STATUS'");

            // 醫事司過濾狀態選單
            if (unit_cd == "4" || unit_cd == "02")
            {
                sql.Append(" AND CODE_CD IN ('--','1','2','3','8','9','12','15','0')");
            }

            sql.Append(" AND CODE_PCD IN(SELECT UNIT_SCD FROM UNIT WHERE UNIT_PCD = @UNIT_CD OR UNIT_CD = @UNIT_CD) ");
            sql.Append(" ORDER BY CASE CODE_CD WHEN '--' THEN - 1 ELSE CONVERT(INT, CODE_CD) END ");


            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 國家清單
        /// </summary>
        public IList<KeyMapModel> GetFlyPayCountryMapList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT CONCAT([NAME],' (',[ENAME],')') AS TEXT, CONCAT([CODE1],'-',[CODE2],'-',[CODE3]) AS CODE");
            sql.Append(" FROM [FLYCOUNTRY]");
            sql.Append(" ORDER BY [NAME]");
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 進口關清單
        /// </summary>
        public IList<KeyMapModel> GetImportMapList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND ='F5_vw_IMPORT' ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString()).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetCERTCATEList001009()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_PCD", "" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND in ('F1_LICENSE_CD_1','F1_LICENSE_CD_2') ");
            sql.Append(" AND CODE_PCD = @CODE_PCD ");
            sql.Append(" ORDER BY CODE_KIND,SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetCERTCATEByPCDList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_PCD", "" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND in ('F1_LICENSE_CD_1','F1_LICENSE_CD_2') ");
            sql.Append(" AND CODE_PCD <> @CODE_PCD ");
            sql.Append(" ORDER BY CODE_KIND,SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetCERTCATEMDList(string CODE_PCD)
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_PCD", CODE_PCD }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND in ('F1_LICENSE_CD_1','F1_LICENSE_CD_2') ");
            sql.Append(" AND CODE_PCD = @CODE_PCD ");
            sql.Append(" ORDER BY CODE_KIND,SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
            }

            return result;
        }

        public IList<KeyMapModel> GetCERTCATEMDList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_PCD", "" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT  CODE_CD AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND in ('F1_LICENSE_CD_1','F1_LICENSE_CD_2') ");
            sql.Append(" AND CODE_PCD = @CODE_PCD ");
            sql.Append(" ORDER BY CODE_KIND,SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        public IList<KeyMapModel> GetCERTCATEMDByPCDList()
        {
            List<KeyMapModel> result = null;
            StringBuilder sql = new StringBuilder();
            var dictionary = new Dictionary<string, object>
            {
                { "@CODE_PCD", "" }
            };
            var parameters = new DynamicParameters(dictionary);
            sql.Append(" SELECT CODE_DESC AS CODE,CODE_DESC AS TEXT ");
            sql.Append(" FROM CODE_CD ");
            sql.Append(" WHERE CODE_KIND in ('F1_LICENSE_CD_1','F1_LICENSE_CD_2') ");
            sql.Append(" AND CODE_PCD <> @CODE_PCD ");
            sql.Append(" ORDER BY CODE_KIND,SEQ_NO,CODE_CD ");

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<KeyMapModel>(sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        #region Helper 控件
        /// <summary>
        /// 根據郵遞區號取得縣市鄉鎮名稱
        /// 支援 5 碼 (查 ZIPCODE 表) 及 6 碼 (查 ZIPCODE6 表)
        /// </summary>
        /// <param name="CODE">郵遞區號 (5碼或6碼)</param>
        /// <returns>縣市鄉鎮名稱</returns>
        public KeyMapModel GetCityTownName(string CODE)
        {
            KeyMapModel result = null;
            var parameters = new DynamicParameters();
            string _sql;

            if (!string.IsNullOrEmpty(CODE))
            {
                // 根據輸入長度選擇查詢來源
                if (CODE.Length == 6)
                {
                    // 6碼：優先查詢 ZIPCODE6 表
                    _sql = @"SELECT ZIP_CO AS CODE, (CITYNM + TOWNNM) AS TEXT
                             FROM ZIPCODE6 
                             WHERE ZIP_CO = @ZIP_CO";
                }
                else
                {
                    // 5碼或其他：查詢原 ZIPCODE 表
                    _sql = @"SELECT ZIP_CO AS CODE, (CITYNM + TOWNNM) AS TEXT
                             FROM ZIPCODE 
                             WHERE ZIP_CO = @ZIP_CO";
                }

                parameters.Add("ZIP_CO", CODE);

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    result = conn.Query<KeyMapModel>(_sql, parameters).FirstOrDefault();
                    conn.Close();
                    conn.Dispose();
                }
            }

            return result;
        }

        public KeyMapModel GetCityTownZIP(string Address)
        {
            KeyMapModel result = null;
            //var dictionary = new Dictionary<string, object> { { "@ZIP_CO", CODE } };
            var parameters = new DynamicParameters(); //動態參數
            string _sql = @"SELECT ZIP_CO AS CODE,(CITYNM + TOWNNM) AS TEXT
                            FROM ZIPCODE 
                            WHERE 1 = 1 ";
            if (!string.IsNullOrEmpty(Address))
            {
                parameters.Add("Address", Address);
                _sql += " and @Address LIKE '%' + (CITYNM + TOWNNM) + '%'";
             //   _sql += " and (CITYNM + TOWNNM) LIKE '%' + @Address + '%'";
            }
            _sql += " order by ZIP_CO";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
               var result_temp = conn.Query<KeyMapModel>(_sql, parameters).ToList();
                if (result_temp.ToCount() > 0)
                {
                    result = result_temp.FirstOrDefault();
                    result.TEXT = Address.Replace(result.TEXT, "");
                }
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        #endregion
    }
}
