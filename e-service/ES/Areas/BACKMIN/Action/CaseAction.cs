using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using log4net;
using ES.Areas.Admin.Models;
using System.Data.Objects.SqlClient;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Configuration;
using ES.Utils;
using System.Data;
using System.Web.Mvc;
using ES.Services;
using ES.Models.Entities;
using ES.Models;

namespace ES.Areas.Admin.Action
{
    public class CaseAction : BaseAction
    {
        public List<CaseQueryModel> GetAPPLY(CaseModel model, AccountModel account)
        {
            List<CaseQueryModel> li = new List<CaseQueryModel>();
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand com = conn.CreateCommand();
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                     select a.APP_ID,a.APP_TIME,a.FLOW_CD,a.NAME,a.PRO_ACC,a.SRV_ID,s.name as SRV_NAME,isnull(ad.name,a.PRO_ACC) as ACC_NAME, a.APP_EXT_DATE, s.PRO_DEADLINE
                        , case when (
                            SELECT CODE_DESC
                            FROM CODE_CD
                            WHERE DEL_MK = 'N'
                                AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD)
                                AND CODE_CD = A.FLOW_CD
                                AND CODE_KIND = 'F_CASE_STATUS'
                        )='結案(歉難同意)'and A.SRV_ID in ('005014','005013') then '-' 
                        when (
                            SELECT CODE_DESC
                            FROM CODE_CD
                            WHERE DEL_MK = 'N'
                                AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD)
                                AND CODE_CD = A.FLOW_CD
                                AND CODE_KIND = 'F_CASE_STATUS'
                        )='結案(回函核准)' then '結案(回函)' 
                        when (
                            SELECT CODE_DESC
                            FROM CODE_CD
                            WHERE DEL_MK = 'N'
                                AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD)
                                AND CODE_CD = A.FLOW_CD
                                AND CODE_KIND = 'F_CASE_STATUS'
                        )='結案(歉難同意)' then '結案(回函)' 
                        else (
                            SELECT CODE_DESC
                            FROM CODE_CD
                            WHERE DEL_MK = 'N'
                                AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD)
                                AND CODE_CD = A.FLOW_CD
                                AND CODE_KIND = 'F_CASE_STATUS'
                        ) end AS CASE_STATUS
                        from apply a
                        left join service s on a.srv_id = s.srv_id
                        left join admin ad on a.PRO_ACC = ad.ACC_NO
                        where  a.app_disp_mk = 'Y' and a.DEL_MK = 'N'
                ");
                //DataUtils.AddParameters(com, "FLOW_UNIT_CD", model.UNIT_CD);

                //if (model.IS_HOME_PAGE == "T")
                //{
                //    sb.Append(" AND DATEDIFF(DAY,GETDATE(),a.APP_EXT_DATE)<0 ");
                //}

                if (account.Scope == 1) // 所有單位
                {

                }
                else if (account.Scope == 0) // 所屬單位
                {
                    sb.Append("and a.PRO_UNIT_CD in (select unit_cd from unit where (unit_pcd = @SCOPEUNITCODE or unit_cd = @SCOPEUNITCODE)) ");
                    DataUtils.AddParameters(com, "SCOPEUNITCODE", account.ScopeUnitCode);

                    //if (model.IS_HOME_PAGE == "Y" || model.IS_HOME_PAGE == "T")
                    //{
                    sb.Append("and a.PRO_ACC = @PRO_ACC ");
                    DataUtils.AddParameters(com, "PRO_ACC", account.Account);
                    //}
                }
                else // 隸屬單位
                {
                    // 自己承辦的案件
                    sb.Append("and a.PRO_ACC = @PRO_ACC ");
                    DataUtils.AddParameters(com, "PRO_ACC", account.Account);
                }

                // 查詢申請人姓名
                if (model.AP_NAME != null)
                {
                    sb.Append("and a.NAME LIKE '%' + @AP_NAME + '%' ");
                    DataUtils.AddParameters(com, "AP_NAME", model.AP_NAME);
                }

                // 查詢承辦人姓名
                if (model.PRO_ACC != null)
                {
                    sb.Append("and ad.NAME in (SELECT NAME FROM ADMIN WHERE NAME = @NAME) ");
                    DataUtils.AddParameters(com, "NAME", model.PRO_ACC);
                }

                //// 結案狀態
                //if (model.CLOSE_MK == "Y")
                //{
                //    sb.Append(" and a.flow_cd = '0' ");
                //}
                //else
                //{
                //    sb.Append(" and a.flow_cd <> '0' ");
                //}
                //DataUtils.AddParameters(com, "close_mk", model.CLOSE_MK);

                // 申辦編號起
                if (!String.IsNullOrEmpty(model.APP_ID_BEGIN))
                {
                    sb.Append(" and a.APP_ID >= @APP_ID_BEGIN ");
                    DataUtils.AddParameters(com, "APP_ID_BEGIN", model.APP_ID_BEGIN);
                }
                // 申辦編號迄
                if (!String.IsNullOrEmpty(model.APP_ID_END))
                {
                    sb.Append(" and a.APP_ID <= @APP_ID_END ");
                    DataUtils.AddParameters(com, "APP_ID_END", model.APP_ID_END);
                }
                // 申辦日期起
                if (model.APP_TIME_BEGIN.HasValue)
                {
                    sb.Append(" and a.APP_TIME >= @APP_TIME_BEGIN ");
                    DataUtils.AddParameters(com, "APP_TIME_BEGIN", model.APP_TIME_BEGIN.Value);
                }
                // 申辦日期迄
                if (model.APP_TIME_END.HasValue)
                {
                    sb.Append(" and a.APP_TIME <= @APP_TIME_END ");
                    DataUtils.AddParameters(com, "APP_TIME_END", model.APP_TIME_END.Value.AddDays(1));
                }
                // 公文文號
                if (!String.IsNullOrEmpty(model.MOHW_CASE_NO))
                {
                    sb.Append(" and a.MOHW_CASE_NO like '%' + @MOHW_CASE_NO + '%' ");
                    DataUtils.AddParameters(com, "MOHW_CASE_NO", model.MOHW_CASE_NO);
                }

                // 處理進度
                if (!string.IsNullOrEmpty(model.UNIT_CD))
                {
                    sb.Append(" and a.UNIT_CD = @UNIT_CD ");
                    DataUtils.AddParameters(com, "UNIT_CD", model.UNIT_CD);
                }
                if (!String.IsNullOrEmpty(model.FLOW_CD) && Convert.ToString(model.FLOW_CD)!="-1")
                {
                    //if (model.FLOW_CD != "-1")
                    //{
                    //    sb.Append(" AND DATEDIFF(DAY, GETDATE(), A.APP_EXT_DATE)>= 0 ");
                    //}
                    sb.Append(" and a.FLOW_CD = @FLOW_CD ");
                    DataUtils.AddParameters(com, "FLOW_CD", model.FLOW_CD);
                }
                // 申請人身份字號/統編
                if (!String.IsNullOrEmpty(model.IDN))
                {
                    sb.Append(" and a.IDN = @IDN ");
                    DataUtils.AddParameters(com, "IDN", model.IDN);
                }
                // 申請項目
                if (!String.IsNullOrEmpty(model.Apply_Item))
                {
                    sb.Append(" and a.SRV_ID = @SRV_ID ");
                    DataUtils.AddParameters(com, "SRV_ID", model.Apply_Item);
                }
                else
                {
                    sb.Append("and a.SRV_ID NOT LIKE '0070__' ");
                }

                // 中醫藥司
                // 製造廠名稱
                if (!String.IsNullOrEmpty(model.MF_NAME))
                {
                    sb.Append("and a.NAME like '%' + @MF_NAME + '%' ");
                    DataUtils.AddParameters(com, "MF_NAME", model.MF_NAME);
                }
                // 許可證字號
                if (!String.IsNullOrEmpty(model.LIC_CD) || !String.IsNullOrEmpty(model.LIC_NUM) || !String.IsNullOrEmpty(model.MF_CD)
                    || !String.IsNullOrEmpty(model.DRUG_NAME) || !String.IsNullOrEmpty(model.INGR_NAME) || !String.IsNullOrEmpty(model.EFFICACY)
                    || !String.IsNullOrEmpty(model.INDIOCATION) || !String.IsNullOrEmpty(model.DOSAGE_FORM))
                {
                    sb.Append("and a.APP_ID IN (");

                    bool first = true;

                    if (!String.IsNullOrEmpty(model.LIC_CD) || !String.IsNullOrEmpty(model.LIC_NUM) || !String.IsNullOrEmpty(model.DRUG_NAME)
                        || !String.IsNullOrEmpty(model.EFFICACY) || !String.IsNullOrEmpty(model.INDIOCATION) || !String.IsNullOrEmpty(model.DOSAGE_FORM))
                    {
                        first = false;
                        sb.Append(@"select APP_ID from APPLY_005001 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.LIC_CD)) sb.Append("and LIC_CD = @LIC_CD ");                    //許可證字號(字)
                        if (!String.IsNullOrEmpty(model.LIC_NUM)) sb.Append("and LIC_NUM like '%' + @LIC_NUM + '%' ");  //許可證字號(號)
                        if (!String.IsNullOrEmpty(model.DRUG_NAME)) sb.Append("and (DRUG_NAME like '%' + @DRUG_NAME + '%' or DRUG_NAME_E like '%' + @DRUG_NAME + '%') ");   // 品名(中英文)
                        if (!String.IsNullOrEmpty(model.EFFICACY)) sb.Append("and (EFFICACY like '%' + @EFFICACY + '%' or EFFICACY_E like '%' + @EFFICACY + '%') ");        // 效能
                        if (!String.IsNullOrEmpty(model.INDIOCATION)) sb.Append("and (INDIOCATION like '%' + @INDIOCATION + '%' or INDIOCATION_E like '%' + @INDIOCATION + '%') "); // 適應症
                        if (!String.IsNullOrEmpty(model.DOSAGE_FORM)) sb.Append("and (DOSAGE_FORM like '%' + @DOSAGE_FORM + '%' or DOSAGE_FORM_E like '%' + @DOSAGE_FORM + '%') "); // 劑型

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005002 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.LIC_CD)) sb.Append("and LIC_CD = @LIC_CD ");                    //許可證字號(字)
                        if (!String.IsNullOrEmpty(model.LIC_NUM)) sb.Append("and LIC_NUM like '%' + @LIC_NUM + '%' ");  //許可證字號(號)
                        if (!String.IsNullOrEmpty(model.DRUG_NAME)) sb.Append("and (DRUG_NAME like '%' + @DRUG_NAME + '%' or DRUG_NAME_E like '%' + @DRUG_NAME + '%') ");   // 品名(中英文)
                        if (!String.IsNullOrEmpty(model.EFFICACY)) sb.Append("and (EFFICACY like '%' + @EFFICACY + '%' or EFFICACY_E like '%' + @EFFICACY + '%') ");        // 效能
                        if (!String.IsNullOrEmpty(model.INDIOCATION)) sb.Append("and (INDIOCATION like '%' + @INDIOCATION + '%' or INDIOCATION_E like '%' + @INDIOCATION + '%') "); // 適應症
                        if (!String.IsNullOrEmpty(model.DOSAGE_FORM)) sb.Append("and (DOSAGE_FORM like '%' + @DOSAGE_FORM + '%' or DOSAGE_FORM_E like '%' + @DOSAGE_FORM + '%') "); // 劑型
                    }

                    if (!String.IsNullOrEmpty(model.MF_CD))
                    {
                        if (!first) sb.Append("union all ");
                        first = false;

                        sb.Append(@"select APP_ID from APPLY_005004 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.MF_CD)) sb.Append("and LIC_NUM like '%' + @MF_CD + '%' ");   //衛部中藥廠證號

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005005 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.MF_CD)) sb.Append("and LIC_NUM like '%' + @MF_CD + '%' ");   //衛部中藥廠證號
                    }

                    //成分
                    if (!String.IsNullOrEmpty(model.INGR_NAME))
                    {
                        if (!first) sb.Append("union all ");
                        first = false;

                        sb.Append(@"select APP_ID from APPLY_005001_PC where 1=1 ");
                        sb.Append("and (PC_NAME like '%' + @INGR_NAME + '%' or PC_ENAME like '%' + @INGR_NAME + '%') ");

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005001_DI where 1=1 ");
                        sb.Append("and (DI_NAME like '%' + @INGR_NAME + '%' or DI_ENAME like '%' + @INGR_NAME + '%') ");

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005002_PC where 1=1 ");
                        sb.Append("and (PC_NAME like '%' + @INGR_NAME + '%' or PC_ENAME like '%' + @INGR_NAME + '%') ");

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005002_DI where 1=1 ");
                        sb.Append("and (DI_NAME like '%' + @INGR_NAME + '%' or DI_ENAME like '%' + @INGR_NAME + '%') ");
                    }


                    sb.Append(")");

                    if (!String.IsNullOrEmpty(model.LIC_CD)) DataUtils.AddParameters(com, "LIC_CD", model.LIC_CD);           //許可證字號(字)
                    if (!String.IsNullOrEmpty(model.LIC_NUM)) DataUtils.AddParameters(com, "LIC_NUM", model.LIC_NUM);         //許可證字號(號)
                    if (!String.IsNullOrEmpty(model.MF_CD)) DataUtils.AddParameters(com, "MF_CD", model.MF_CD);             //衛部中藥廠證號
                    if (!String.IsNullOrEmpty(model.DRUG_NAME)) DataUtils.AddParameters(com, "DRUG_NAME", model.DRUG_NAME);     //品名(中英文)
                    if (!String.IsNullOrEmpty(model.INGR_NAME)) DataUtils.AddParameters(com, "INGR_NAME", model.INGR_NAME);     //成分
                    if (!String.IsNullOrEmpty(model.EFFICACY)) DataUtils.AddParameters(com, "EFFICACY", model.EFFICACY);       //效能
                    if (!String.IsNullOrEmpty(model.INDIOCATION)) DataUtils.AddParameters(com, "INDIOCATION", model.INDIOCATION); //適應症
                    if (!String.IsNullOrEmpty(model.DOSAGE_FORM)) DataUtils.AddParameters(com, "DOSAGE_FORM", model.DOSAGE_FORM); //劑型
                }

                // 結案狀態
                var statusName = "'完成申請','結案(回函)','結案(回函核准)','收案','結案(歉難同意)','-','收件不收案','逾期未補件而予結案','核可(發文歸檔)','退件通知','自請撤銷','退件','退件且結案','結案','收件成功'";
                // 醫事司案件狀態調整 增加以下舊有資料已結案狀態
                statusName += ",'核可(發文續辦)','駁回','送審通知','審查結果回函','其他','併結公文','存查(退稿)','結案後合併歸檔','展期申請發函','展期申請回函'";
                statusName += ",'函覆審查結果','其他開會通知','審查會議通知','初審會議通知'";
                if (model.CLOSE_MK == "Y")
                {
                    com.CommandText = "SELECT * FROM ( " + sb.ToString() + " ) T " +
                        "where 1=1 and T.CASE_STATUS in ( " + statusName + " )" +
                        " order by " + GetOrderColumn(model.OderByCol) + " " + GetOrderBy(model.SortAZ);
                }
                else
                {
                    com.CommandText = "SELECT * FROM ( " + sb.ToString() + " ) T " +
                        "where 1=1 and T.CASE_STATUS not in ( " + statusName + " )" +
                        " order by " + GetOrderColumn(model.OderByCol) + " " + GetOrderBy(model.SortAZ);
                }

                //string s_log1 = "";
                //s_log1 += "\n##GetAPPLY ";
                //s_log1 += string.Format("\n /*com.CommandText*/ \n{0}", com.CommandText);
                //s_log1 += "\n /*com.Parameters*/";
                //foreach (SqlParameter param in com.Parameters) { s_log1 += string.Format("\n and {0}='{1}'", param.ParameterName, param.Value); }
                //logger.Debug(s_log1);
                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        CaseQueryModel cqm = new CaseQueryModel();
                        cqm.APP_ID = sr["APP_ID"].ToString();
                        cqm.APP_TIME = Convert.ToDateTime(sr["APP_TIME"].ToString());
                        cqm.FLOW_CD = sr["FLOW_CD"].ToString();
                        cqm.NAME = sr["NAME"].ToString();
                        cqm.PRO_ACC = sr["PRO_ACC"].ToString();
                        cqm.SRV_ID = sr["SRV_ID"].ToString();
                        cqm.SRV_NAME = sr["SRV_NAME"].ToString();
                        cqm.ACC_NAME = sr["ACC_NAME"].ToString();
                        cqm.APP_EXT_DATE = DataUtils.GetDBDateTime(sr, 8);

                        if (!cqm.APP_EXT_DATE.HasValue)
                        {
                            cqm.APP_EXT_DATE = cqm.APP_TIME.Value.AddDays(DataUtils.GetDBInt(sr, 9));
                        }
                        cqm.CASE_STATUS = sr["CASE_STATUS"].ToString();

                        // 民眾少量 共用中醫藥司 CODE_CD，但狀態名稱不同
                        if (sr["SRV_ID"].ToString() == "005013")
                        {
                            var case_sta = string.Empty;
                            var flow = sr["FLOW_CD"].ToString();
                            TblCODE_CD where = new TblCODE_CD();
                            where.CODE_KIND = "F_CASE_STATUS";
                            where.CODE_PCD = "9";
                            where.CODE_CD = flow;
                            var rdata = this.GetRow(where);
                            if (rdata != null && !string.IsNullOrEmpty(rdata.CODE_MEMO))
                            {
                                cqm.CASE_STATUS = rdata.CODE_MEMO;
                            }
                        }

                        // 貨品進口專案申請 共用中醫藥司 CODE_CD，但狀態名稱不同
                        if (sr["SRV_ID"].ToString() == "005014")
                        {
                            var case_sta = string.Empty;
                            var flow = sr["FLOW_CD"].ToString();
                            TblCODE_CD where = new TblCODE_CD();
                            where.CODE_KIND = "F_CASE_STATUS";
                            where.CODE_PCD = "9";
                            where.CODE_CD = flow;
                            var rdata = this.GetRow(where);
                            if (rdata != null && !string.IsNullOrEmpty(rdata.CODE_MEMO))
                            {
                                cqm.CASE_STATUS = rdata.CODE_MEMO;
                            }
                        }

                        li.Add(cqm);
                    }
                    sr.Close();
                }
                if (li != null)
                {
                    this.totalCount = li.Count();
                    li = li.Skip((model.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                com.Dispose();
            }

            return li == null ? new List<CaseQueryModel>() : li;
        }

        public List<CaseQueryModel> GetAPPLYByToDoList(CaseModel model, AccountModel account)
        {
            List<CaseQueryModel> li = new List<CaseQueryModel>();
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand com = conn.CreateCommand();
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                    select a.APP_ID,a.APP_TIME,a.FLOW_CD,a.NAME,a.PRO_ACC,a.SRV_ID,s.name as SRV_NAME,ad.name as ACC_NAME, a.APP_EXT_DATE, s.PRO_DEADLINE
                    , (
                            SELECT CODE_DESC
                            FROM CODE_CD
                            WHERE DEL_MK = 'N'
                              AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_PCD=@FLOW_UNIT_CD OR UNIT_CD=@FLOW_UNIT_CD)
                              AND CODE_CD = A.FLOW_CD
                              AND CODE_KIND = 'F_CASE_STATUS'
                        ) AS CASE_STATUS
                    from apply a
                    left join service s on a.srv_id = s.srv_id
                    left join admin ad on a.PRO_ACC = ad.ACC_NO
                    where  a.app_disp_mk = 'Y' and a.DEL_MK = 'N'
                ");

                //where a.close_mk =@close_mk and a.app_disp_mk = 'Y' and a.DEL_MK = 'N'
                //DataUtils.AddParameters(com, "FLOW_UNIT_CD", model.UNIT_CD);
                DataUtils.AddParameters(com, "FLOW_UNIT_CD", account.ServiceUnitCode);

                if (model.IS_HOME_PAGE == "T")
                {
                    sb.Append(" AND DATEDIFF(DAY,GETDATE(),a.APP_EXT_DATE)<0 ");
                }

                if (account.Scope == 1) // 所有單位
                {

                }
                else if (account.Scope == 0) // 所屬單位
                {
                    sb.Append("and a.PRO_UNIT_CD in (select unit_cd from unit where (unit_pcd = @UNIT_CD or unit_cd = @UNIT_CD)) ");
                    DataUtils.AddParameters(com, "UNIT_CD", account.ScopeUnitCode);

                    if (model.IS_HOME_PAGE == "Y" || model.IS_HOME_PAGE == "T")
                    {
                        sb.Append("and a.PRO_ACC = @PRO_ACC ");
                        DataUtils.AddParameters(com, "PRO_ACC", account.Account);
                    }
                }
                else // 隸屬單位
                {
                    sb.Append("and a.PRO_ACC = @PRO_ACC ");
                    DataUtils.AddParameters(com, "PRO_ACC", account.Account);
                }

                if (model.PRO_ACC != null)
                {
                    sb.Append("and ad.NAME in (SELECT NAME FROM ADMIN WHERE NAME =  @NAME) ");
                    DataUtils.AddParameters(com, "NAME", model.PRO_ACC);
                }

                if (model.CLOSE_MK == "Y")
                {
                    sb.Append(" and a.flow_cd = '0' ");
                }
                else
                {
                    sb.Append(" and a.flow_cd != '0' ");
                }
                DataUtils.AddParameters(com, "close_mk", model.CLOSE_MK);

                // 愛
                //com.Parameters.Add("@close_mk", model.CLOSE_MK);
                if (!String.IsNullOrEmpty(model.APP_ID_BEGIN))
                {
                    sb.Append(" and a.APP_ID >= @APP_ID_BEGIN ");
                    DataUtils.AddParameters(com, "APP_ID_BEGIN", model.APP_ID_BEGIN);
                    //com.Parameters.Add("@APP_ID_BEGIN", model.APP_ID_BEGIN);
                }

                if (!String.IsNullOrEmpty(model.APP_ID_END))
                {
                    sb.Append(" and a.APP_ID <= @APP_ID_END ");
                    DataUtils.AddParameters(com, "APP_ID_END", model.APP_ID_END);
                    //com.Parameters.Add("@APP_ID_END", model.APP_ID_END);
                }
                if (model.APP_TIME_BEGIN.HasValue)
                {
                    sb.Append(" and a.APP_TIME >= @APP_TIME_BEGIN ");
                    DataUtils.AddParameters(com, "APP_TIME_BEGIN", model.APP_TIME_BEGIN.Value);
                    //com.Parameters.Add("@APP_TIME_BEGIN", model.APP_TIME_BEGIN.Value);
                }
                if (model.APP_TIME_END.HasValue)
                {
                    sb.Append(" and a.APP_TIME <= @APP_TIME_END ");
                    DataUtils.AddParameters(com, "APP_TIME_END", model.APP_TIME_END.Value.AddDays(1));
                    //com.Parameters.Add("@APP_TIME_END", model.APP_TIME_END.Value.AddDays(1));
                }
                if (!String.IsNullOrEmpty(model.MOHW_CASE_NO))
                {
                    sb.Append(" and a.MOHW_CASE_NO like '%' + @MOHW_CASE_NO + '%' ");
                    DataUtils.AddParameters(com, "MOHW_CASE_NO", model.MOHW_CASE_NO);
                    //com.Parameters.Add("@MOHW_CASE_NO", model.MOHW_CASE_NO);
                }
                if (!String.IsNullOrEmpty(model.FLOW_CD))
                {
                    if (model.FLOW_CD != "-1")
                    {
                        sb.Append(" AND DATEDIFF(DAY, GETDATE(), A.APP_EXT_DATE)>= 0 ");
                        sb.Append(" and a.FLOW_CD = @FLOW_CD ");
                        DataUtils.AddParameters(com, "FLOW_CD", model.FLOW_CD);
                    }

                    //com.Parameters.Add("@FLOW_CD", model.FLOW_CD);
                }

                if (!String.IsNullOrEmpty(model.IDN))
                {
                    sb.Append(" and a.IDN = @IDN ");
                    DataUtils.AddParameters(com, "IDN", model.IDN);
                    //com.Parameters.Add("@IDN", model.IDN);
                }

                //Add by Alyssa 20150703
                if (!String.IsNullOrEmpty(model.Apply_Item))
                {
                    sb.Append(" and a.SRV_ID = @SRV_ID ");
                    DataUtils.AddParameters(com, "SRV_ID", model.Apply_Item);
                }
                else
                {
                    sb.Append("and a.SRV_ID NOT LIKE '0070__' ");
                }

                // 中醫藥司

                // 製造廠名稱
                if (!String.IsNullOrEmpty(model.MF_NAME))
                {
                    sb.Append("and a.NAME like '%' + @MF_NAME + '%' ");
                    DataUtils.AddParameters(com, "MF_NAME", model.MF_NAME);
                }

                // 許可證字號
                if (!String.IsNullOrEmpty(model.LIC_CD) || !String.IsNullOrEmpty(model.LIC_NUM) || !String.IsNullOrEmpty(model.MF_CD)
                    || !String.IsNullOrEmpty(model.DRUG_NAME) || !String.IsNullOrEmpty(model.INGR_NAME) || !String.IsNullOrEmpty(model.EFFICACY)
                    || !String.IsNullOrEmpty(model.INDIOCATION) || !String.IsNullOrEmpty(model.DOSAGE_FORM))
                {
                    sb.Append("and a.APP_ID IN (");

                    bool first = true;

                    if (!String.IsNullOrEmpty(model.LIC_CD) || !String.IsNullOrEmpty(model.LIC_NUM) || !String.IsNullOrEmpty(model.DRUG_NAME)
                        || !String.IsNullOrEmpty(model.EFFICACY) || !String.IsNullOrEmpty(model.INDIOCATION) || !String.IsNullOrEmpty(model.DOSAGE_FORM))
                    {
                        first = false;
                        sb.Append(@"select APP_ID from APPLY_005001 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.LIC_CD)) sb.Append("and LIC_CD = @LIC_CD ");                    //許可證字號(字)
                        if (!String.IsNullOrEmpty(model.LIC_NUM)) sb.Append("and LIC_NUM like '%' + @LIC_NUM + '%' ");  //許可證字號(號)
                        if (!String.IsNullOrEmpty(model.DRUG_NAME)) sb.Append("and (DRUG_NAME like '%' + @DRUG_NAME + '%' or DRUG_NAME_E like '%' + @DRUG_NAME + '%') ");   // 品名(中英文)
                        if (!String.IsNullOrEmpty(model.EFFICACY)) sb.Append("and (EFFICACY like '%' + @EFFICACY + '%' or EFFICACY_E like '%' + @EFFICACY + '%') ");        // 效能
                        if (!String.IsNullOrEmpty(model.INDIOCATION)) sb.Append("and (INDIOCATION like '%' + @INDIOCATION + '%' or INDIOCATION_E like '%' + @INDIOCATION + '%') "); // 適應症
                        if (!String.IsNullOrEmpty(model.DOSAGE_FORM)) sb.Append("and (DOSAGE_FORM like '%' + @DOSAGE_FORM + '%' or DOSAGE_FORM_E like '%' + @DOSAGE_FORM + '%') "); // 劑型

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005002 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.LIC_CD)) sb.Append("and LIC_CD = @LIC_CD ");                    //許可證字號(字)
                        if (!String.IsNullOrEmpty(model.LIC_NUM)) sb.Append("and LIC_NUM like '%' + @LIC_NUM + '%' ");  //許可證字號(號)
                        if (!String.IsNullOrEmpty(model.DRUG_NAME)) sb.Append("and (DRUG_NAME like '%' + @DRUG_NAME + '%' or DRUG_NAME_E like '%' + @DRUG_NAME + '%') ");   // 品名(中英文)
                        if (!String.IsNullOrEmpty(model.EFFICACY)) sb.Append("and (EFFICACY like '%' + @EFFICACY + '%' or EFFICACY_E like '%' + @EFFICACY + '%') ");        // 效能
                        if (!String.IsNullOrEmpty(model.INDIOCATION)) sb.Append("and (INDIOCATION like '%' + @INDIOCATION + '%' or INDIOCATION_E like '%' + @INDIOCATION + '%') "); // 適應症
                        if (!String.IsNullOrEmpty(model.DOSAGE_FORM)) sb.Append("and (DOSAGE_FORM like '%' + @DOSAGE_FORM + '%' or DOSAGE_FORM_E like '%' + @DOSAGE_FORM + '%') "); // 劑型
                    }

                    if (!String.IsNullOrEmpty(model.MF_CD))
                    {
                        if (!first) sb.Append("union all ");
                        first = false;

                        sb.Append(@"select APP_ID from APPLY_005004 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.MF_CD)) sb.Append("and LIC_NUM like '%' + @MF_CD + '%' ");   //衛部中藥廠證號

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005005 where 1=1 ");
                        if (!String.IsNullOrEmpty(model.MF_CD)) sb.Append("and LIC_NUM like '%' + @MF_CD + '%' ");   //衛部中藥廠證號
                    }

                    //成分
                    if (!String.IsNullOrEmpty(model.INGR_NAME))
                    {
                        if (!first) sb.Append("union all ");
                        first = false;

                        sb.Append(@"select APP_ID from APPLY_005001_PC where 1=1 ");
                        sb.Append("and (PC_NAME like '%' + @INGR_NAME + '%' or PC_ENAME like '%' + @INGR_NAME + '%') ");

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005001_DI where 1=1 ");
                        sb.Append("and (DI_NAME like '%' + @INGR_NAME + '%' or DI_ENAME like '%' + @INGR_NAME + '%') ");

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005002_PC where 1=1 ");
                        sb.Append("and (PC_NAME like '%' + @INGR_NAME + '%' or PC_ENAME like '%' + @INGR_NAME + '%') ");

                        sb.Append("union all ");

                        sb.Append(@"select APP_ID from APPLY_005002_DI where 1=1 ");
                        sb.Append("and (DI_NAME like '%' + @INGR_NAME + '%' or DI_ENAME like '%' + @INGR_NAME + '%') ");
                    }


                    sb.Append(")");

                    if (!String.IsNullOrEmpty(model.LIC_CD)) DataUtils.AddParameters(com, "LIC_CD", model.LIC_CD);           //許可證字號(字)
                    if (!String.IsNullOrEmpty(model.LIC_NUM)) DataUtils.AddParameters(com, "LIC_NUM", model.LIC_NUM);         //許可證字號(號)
                    if (!String.IsNullOrEmpty(model.MF_CD)) DataUtils.AddParameters(com, "MF_CD", model.MF_CD);             //衛部中藥廠證號
                    if (!String.IsNullOrEmpty(model.DRUG_NAME)) DataUtils.AddParameters(com, "DRUG_NAME", model.DRUG_NAME);     //品名(中英文)
                    if (!String.IsNullOrEmpty(model.INGR_NAME)) DataUtils.AddParameters(com, "INGR_NAME", model.INGR_NAME);     //成分
                    if (!String.IsNullOrEmpty(model.EFFICACY)) DataUtils.AddParameters(com, "EFFICACY", model.EFFICACY);       //效能
                    if (!String.IsNullOrEmpty(model.INDIOCATION)) DataUtils.AddParameters(com, "INDIOCATION", model.INDIOCATION); //適應症
                    if (!String.IsNullOrEmpty(model.DOSAGE_FORM)) DataUtils.AddParameters(com, "DOSAGE_FORM", model.DOSAGE_FORM); //劑型
                }

                //com.CommandText = "SELECT * FROM ( " + sb.ToString() + " ) T WHERE CASE_STATUS IS NOT NULL " + " order by " + GetOrderColumn(model.OderByCol) + " " + GetOrderBy(model.SortAZ);
                com.CommandText = "SELECT * FROM ( " + sb.ToString() + " ) T  " + " order by " + GetOrderColumn(model.OderByCol) + " " + GetOrderBy(model.SortAZ);

                //string s_log1 = "";
                //s_log1 += "\n##GetAPPLY ";
                //s_log1 += string.Format("\n /*com.CommandText*/ \n{0}", com.CommandText);
                //s_log1 += "\n /*com.Parameters*/";
                //foreach (SqlParameter param in com.Parameters) { s_log1 += string.Format("\n and {0}='{1}'", param.ParameterName, param.Value); }
                //logger.Debug(s_log1);
                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        CaseQueryModel cqm = new CaseQueryModel();
                        cqm.APP_ID = sr["APP_ID"].ToString();
                        cqm.APP_TIME = Convert.ToDateTime(sr["APP_TIME"].ToString());
                        cqm.FLOW_CD = sr["FLOW_CD"].ToString();
                        cqm.NAME = sr["NAME"].ToString();
                        cqm.PRO_ACC = sr["PRO_ACC"].ToString();
                        cqm.SRV_ID = sr["SRV_ID"].ToString();
                        cqm.SRV_NAME = sr["SRV_NAME"].ToString();
                        cqm.ACC_NAME = sr["ACC_NAME"].ToString();
                        cqm.APP_EXT_DATE = DataUtils.GetDBDateTime(sr, 8);

                        if (!cqm.APP_EXT_DATE.HasValue)
                        {
                            cqm.APP_EXT_DATE = cqm.APP_TIME.Value.AddDays(DataUtils.GetDBInt(sr, 9));
                        }
                        cqm.CASE_STATUS = sr["CASE_STATUS"].ToString();

                        // 民眾少量 共用中醫藥司 CODE_CD，但狀態名稱不同
                        if (sr["SRV_ID"].ToString() == "005013")
                        {
                            var case_sta = string.Empty;
                            var flow = sr["FLOW_CD"].ToString();
                            TblCODE_CD where = new TblCODE_CD();
                            where.CODE_KIND = "F_CASE_STATUS";
                            where.CODE_PCD = "9";
                            where.CODE_CD = flow;
                            var rdata = this.GetRow(where);
                            if (rdata != null && !string.IsNullOrEmpty(rdata.CODE_MEMO))
                            {
                                cqm.CASE_STATUS = rdata.CODE_MEMO;
                            }
                        }

                        // 貨品進口專案申請 共用中醫藥司 CODE_CD，但狀態名稱不同
                        if (sr["SRV_ID"].ToString() == "005014")
                        {
                            var case_sta = string.Empty;
                            var flow = sr["FLOW_CD"].ToString();
                            TblCODE_CD where = new TblCODE_CD();
                            where.CODE_KIND = "F_CASE_STATUS";
                            where.CODE_PCD = "9";
                            where.CODE_CD = flow;
                            var rdata = this.GetRow(where);
                            if (rdata != null && !string.IsNullOrEmpty(rdata.CODE_MEMO))
                            {
                                cqm.CASE_STATUS = rdata.CODE_MEMO;
                            }
                        }

                        li.Add(cqm);
                    }
                    sr.Close();
                }
                if (li != null)
                {
                    this.totalCount = li.Count();
                    li = li.Skip((model.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                com.Dispose();
            }

            return li == null ? new List<CaseQueryModel>() : li;
        }


        public DataTable GetAPPLYEXCEL(CaseModel model, AccountModel account)
        {
            DataTable dt = new DataTable();
            List<CaseQueryExcelModel> li = new List<CaseQueryExcelModel>();
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand com = conn.CreateCommand(); ;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                    SELECT a.app_id as '申請編號',convert(varchar,a.APP_TIME,111) as '申辦日期', (
                            SELECT CODE_DESC
                            FROM CODE_CD
                            WHERE DEL_MK = 'N'
                              AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD )
                              AND CODE_CD = A.FLOW_CD
                              AND CODE_KIND = 'F_CASE_STATUS'
                        ) as '處理進度',av.LIC_CD as '許可證字號(字)',av.LIC_NUM as '許可證字號(號)',av.MF_CD as '藥廠編號',a.NAME as '製造廠名稱',av.DRUG_NAME as '中文品名',av.DRUG_NAME_E as '英文品名',av.PC_NAME + av.DI_NAME as '成分',av.EFFICACY as '效能',av.INDIOCATION as '適應症',av.DOSAGE_FORM as '劑型',ad.NAME as '承辦人'
						--,a.PRO_UNIT_CD,a.PRO_ACC,a.close_mk,a.MOHW_CASE_NO,a.FLOW_CD,a.IDN,a.SRV_ID,a.name,av.*
                    FROM APPLY a LEFT OUTER JOIN Apply_Query_View av on a.app_id=av.app_id and a.srv_id = av.SRV_ID left join admin ad on a.PRO_ACC = ad.ACC_NO
                    WHERE a.close_mk = @close_mk and a.app_disp_mk = 'Y' 
                ");

                if (account.Scope == 1) // 所有單位
                {

                }
                else if (account.Scope == 0) // 所屬單位
                {
                    sb.Append("and a.PRO_UNIT_CD in (select unit_cd from unit where (unit_pcd = @UNIT_CD or unit_cd = @UNIT_CD)) ");
                    DataUtils.AddParameters(com, "UNIT_CD", account.ScopeUnitCode);
                }
                else // 隸屬單位
                {
                    sb.Append("and a.PRO_ACC = @PRO_ACC ");
                    DataUtils.AddParameters(com, "PRO_ACC", account.Account);
                }



                DataUtils.AddParameters(com, "close_mk", model.CLOSE_MK);
                //com.Parameters.Add("@close_mk", model.CLOSE_MK);
                if (!String.IsNullOrEmpty(model.APP_ID_BEGIN))
                {
                    sb.Append(" and a.APP_ID >= @APP_ID_BEGIN ");
                    DataUtils.AddParameters(com, "APP_ID_BEGIN", model.APP_ID_BEGIN);
                    //com.Parameters.Add("@APP_ID_BEGIN", model.APP_ID_BEGIN);
                }

                if (!String.IsNullOrEmpty(model.APP_ID_END))
                {
                    sb.Append(" and a.APP_ID <= @APP_ID_END ");
                    DataUtils.AddParameters(com, "APP_ID_END", model.APP_ID_END);
                    //com.Parameters.Add("@APP_ID_END", model.APP_ID_END);
                }
                if (model.APP_TIME_BEGIN.HasValue)
                {
                    sb.Append(" and a.APP_TIME >= @APP_TIME_BEGIN ");
                    DataUtils.AddParameters(com, "APP_TIME_BEGIN", model.APP_TIME_BEGIN.Value);
                }
                if (model.APP_TIME_END.HasValue)
                {
                    sb.Append(" and a.APP_TIME <= @APP_TIME_END ");
                    DataUtils.AddParameters(com, "APP_TIME_END", model.APP_TIME_END.Value.AddDays(1));
                }
                if (!String.IsNullOrEmpty(model.MOHW_CASE_NO))
                {
                    sb.Append(" and a.MOHW_CASE_NO like '%' + @MOHW_CASE_NO + '%' ");
                    DataUtils.AddParameters(com, "MOHW_CASE_NO", model.MOHW_CASE_NO);
                }
                if (!String.IsNullOrEmpty(model.FLOW_CD))
                {
                    sb.Append(" and a.FLOW_CD = @FLOW_CD ");
                    DataUtils.AddParameters(com, "FLOW_CD", model.FLOW_CD);
                }

                if (!String.IsNullOrEmpty(model.IDN))
                {
                    sb.Append(" and a.IDN = @IDN ");
                    DataUtils.AddParameters(com, "IDN", model.IDN);
                }

                //Add by Alyssa 20150703
                if (!String.IsNullOrEmpty(model.Apply_Item))
                {
                    sb.Append(" and a.SRV_ID = @SRV_ID ");
                    DataUtils.AddParameters(com, "SRV_ID", model.Apply_Item);
                }
                else
                {
                    sb.Append("and a.SRV_ID NOT LIKE '0070__' ");
                }

                ///中醫藥師欄位
                // 製造廠名稱
                if (!String.IsNullOrEmpty(model.MF_NAME))
                {
                    sb.Append(" and a.NAME like '%' + @MF_NAME + '%' ");
                    DataUtils.AddParameters(com, "MF_NAME", model.MF_NAME);
                }

                if (!String.IsNullOrEmpty(model.LIC_CD) || !String.IsNullOrEmpty(model.LIC_NUM) || !String.IsNullOrEmpty(model.MF_CD)
                    || !String.IsNullOrEmpty(model.DRUG_NAME) || !String.IsNullOrEmpty(model.INGR_NAME) || !String.IsNullOrEmpty(model.EFFICACY)
                    || !String.IsNullOrEmpty(model.INDIOCATION) || !String.IsNullOrEmpty(model.DOSAGE_FORM))
                {
                    int i = 0;
                    sb.Append(" and (");

                    //許可證字
                    if (!String.IsNullOrEmpty(model.LIC_CD))
                    {
                        i++;
                        sb.Append(" (av.SRV_ID in ('005001','005002') and LIC_CD=@LIC_CD) ");
                        DataUtils.AddParameters(com, "LIC_CD", model.LIC_CD);
                    }


                    //許可證號
                    if (!String.IsNullOrEmpty(model.LIC_CD))
                    {
                        if (i != 0) sb.Append(" OR ");
                        i++;
                        sb.Append(" (av.SRV_ID in ('005001','005002') and LIC_NUM like '%' + @LIC_NUM + '%')");
                        DataUtils.AddParameters(com, "LIC_NUM", model.LIC_NUM);
                    }



                    //中/ 英文品名
                    if (!String.IsNullOrEmpty(model.DRUG_NAME))
                    {
                        if (i != 0) sb.Append(" OR ");
                        i++;
                        sb.Append(" (av.SRV_ID in ('005001','005002') and (DRUG_NAME like '%' +@DRUG_NAME +'%' OR DRUG_NAME_E like '%' +@DRUG_NAME +'%' ))");
                        DataUtils.AddParameters(com, "DRUG_NAME", model.DRUG_NAME);
                    }


                    //效能
                    if (!String.IsNullOrEmpty(model.EFFICACY))
                    {
                        if (i != 0) sb.Append(" OR ");
                        i++;
                        sb.Append(" (av.SRV_ID in ('005001','005002') and (EFFICACY like '%' +@EFFICACY +'%' OR EFFICACY_E like '%' +@EFFICACY +'%' ))");
                        DataUtils.AddParameters(com, "EFFICACY", model.EFFICACY);
                    }

                    //適應症
                    if (!String.IsNullOrEmpty(model.INDIOCATION))
                    {
                        if (i != 0) sb.Append(" OR ");
                        i++;
                        sb.Append(" (av.SRV_ID in ('005001','005002') and (INDIOCATION like '%' +@INDIOCATION +'%' OR INDIOCATION_E like '%' +@INDIOCATION +'%' ))");
                        DataUtils.AddParameters(com, "INDIOCATION", model.EFFICACY);
                    }

                    //劑型
                    if (!String.IsNullOrEmpty(model.DOSAGE_FORM))
                    {
                        if (i != 0) sb.Append(" OR ");
                        i++;
                        sb.Append(" (av.SRV_ID in ('005001','005002') and (DOSAGE_FORM like '%' +@DOSAGE_FORM +'%' OR DOSAGE_FORM_E like '%' +@DOSAGE_FORM +'%' ))");
                        DataUtils.AddParameters(com, "DOSAGE_FORM", model.DOSAGE_FORM);
                    }

                    //藥廠編號
                    if (!String.IsNullOrEmpty(model.MF_CD))
                    {
                        if (i != 0) sb.Append(" OR ");
                        i++;
                        sb.Append(" (av.SRV_ID in ('005004','005005') and MF_CD like '%' +@MF_CD +'%')");
                        DataUtils.AddParameters(com, "MF_CD", model.MF_CD);
                    }

                    //成份
                    if (!String.IsNullOrEmpty(model.INGR_NAME))
                    {
                        if (i != 0) sb.Append(" OR ");
                        i++;
                        sb.Append(" (av.SRV_ID in ('005001','005002') and (PC_NAME  like '%' + @INGR_NAME  +'%' OR PC_ENAME  like '%' +@INGR_NAME +'%' OR DI_NAME  like '%' + @INGR_NAME  +'%' OR DI_ENAME  like '%' +@INGR_NAME +'%' ))");
                        DataUtils.AddParameters(com, "INGR_NAME", model.INGR_NAME);
                    }
                    sb.Append(" ) ");

                }

                com.CommandText = sb.ToString() + " order by a." + GetOrderColumn(model.OderByCol) + " " + GetOrderBy(model.SortAZ);

                SqlDataAdapter sda = new SqlDataAdapter(com);
                sda.Fill(dt);
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                com.Dispose();
            }

            return dt;
            //return li == null ? new List<CaseQueryExcelModel>() : li;
        }

        public List<SelectListItem> GetFLOW_CD(string account, Boolean isPower)
        {
            List<SelectListItem> li = new List<SelectListItem>();
            SelectListItem mapfirst = new SelectListItem();
            mapfirst.Value = "";
            mapfirst.Text = "請選擇";
            li.Add(mapfirst);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    /*
                    String sql = @"with pro_unit_cd as(
                                        select DISTINCT pro_unit_cd from APPLY 
                                        where 1=1 {0} 
                                        ),unit_scd as
                                        (
	                                        select DISTINCT unit_scd from unit where unit_cd in (select * from pro_unit_cd)
                                        )
                                        select FLOW_CD,FLOW_NAME from M_CASE_STATUS where case_cat=4 and close_mk='N' and unit_scd in (select * from unit_scd)
                                        ORDER BY flow_cd,unit_scd
                                        ";
                    */

                    String sql = @"
                        WITH PRO_UNIT_CD AS (
                            SELECT DISTINCT PRO_UNIT_CD FROM APPLY
                            WHERE 1=1 {0} 
                            and PRO_UNIT_CD <> ''
                        ), UNIT_SCD AS (
                            SELECT DISTINCT SUBSTRING(UNIT_SCD, 1, 2) AS UNIT_SCD FROM UNIT WHERE UNIT_CD IN (SELECT * FROM PRO_UNIT_CD)
                        )
                        SELECT DISTINCT CODE_CD AS FLOW_CD, CODE_DESC AS FLOW_NAME
                        FROM CODE_CD
                        WHERE DEL_MK = 'N'
                          AND CODE_KIND = 'F_CASE_STATUS'
                          AND CODE_PCD IN (SELECT * FROM UNIT_SCD)
                        --ORDER BY CODE_PCD, SEQ_NO
                    ";

                    dbc.Parameters.Clear();
                    if (isPower)
                    {
                        sql =
                            @" SELECT DISTINCT  CODE_CD AS FLOW_CD, CODE_DESC AS FLOW_NAME ,CASE CODE_CD WHEN '--' THEN -1 ELSE CONVERT(INT,CODE_CD) END AS ORD
                                FROM CODE_CD 
                                WHERE DEL_MK = 'N' AND CODE_KIND = 'F_CASE_STATUS'  
                                ORDER BY ORD ";
                        sql = string.Format(sql, "");
                    }
                    else
                    {
                        sql = string.Format(sql, "AND PRO_ACC = @PRO_ACC");
                        DataUtils.AddParameters(dbc, "PRO_ACC", account);
                        //dbc.Parameters.Add("@PRO_ACC", account);
                    }
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            //Map map = new Map();
                            //map.Add("FLOW_CD", sda["FLOW_CD"].ToString());
                            //map.Add("FLOW_NAME", sda["FLOW_NAME"].ToString());
                            SelectListItem map = new SelectListItem();
                            map.Value = sda["FLOW_CD"].ToString();
                            map.Text = sda["FLOW_NAME"].ToString();
                            li.Add(map);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<SelectListItem> GetFLOW_CD_BY_ACCOUNT(string account, Boolean isPower, AccountModel accountModel)
        {
            List<SelectListItem> li = new List<SelectListItem>();
            SelectListItem mapfirst = new SelectListItem();
            mapfirst.Value = "";
            mapfirst.Text = "請選擇";
            li.Add(mapfirst);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"
                        WITH PRO_UNIT_CD AS (
                            SELECT DISTINCT PRO_UNIT_CD FROM APPLY
                            WHERE 1=1 {0} 
                            and PRO_UNIT_CD <> ''
                        ), UNIT_SCD AS (
                            SELECT DISTINCT SUBSTRING(UNIT_SCD, 1, 2) AS UNIT_SCD FROM UNIT WHERE UNIT_CD IN (SELECT * FROM PRO_UNIT_CD)
                        )
                        SELECT DISTINCT CODE_CD AS FLOW_CD, CODE_DESC AS FLOW_NAME
                        FROM CODE_CD
                        WHERE DEL_MK = 'N'
                          AND CODE_KIND = 'F_CASE_STATUS'
                          AND CODE_PCD IN (SELECT * FROM UNIT_SCD)
                        --ORDER BY CODE_PCD, SEQ_NO
                    ";

                    dbc.Parameters.Clear();
                    if (isPower)
                    {
                        sql =
                            @" SELECT DISTINCT  CODE_CD AS FLOW_CD, CODE_DESC AS FLOW_NAME ,CASE CODE_CD WHEN '--' THEN -1 ELSE CONVERT(INT,CODE_CD) END AS ORD
                                FROM CODE_CD 
                                WHERE DEL_MK = 'N' AND CODE_KIND = 'F_CASE_STATUS'  
                                AND CODE_PCD IN (SELECT UNIT_SCD FROM UNIT WHERE UNIT_PCD=@UNIT_CD OR UNIT_CD=@UNIT_CD)
                                ORDER BY ORD ";
                        DataUtils.AddParameters(dbc, "UNIT_CD", accountModel.ScopeUnitCode);
                    }
                    else
                    {
                        sql = string.Format(sql, "AND PRO_ACC = @PRO_ACC");
                        DataUtils.AddParameters(dbc, "PRO_ACC", account);
                        //dbc.Parameters.Add("@PRO_ACC", account);
                    }
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            //Map map = new Map();
                            //map.Add("FLOW_CD", sda["FLOW_CD"].ToString());
                            //map.Add("FLOW_NAME", sda["FLOW_NAME"].ToString());
                            SelectListItem map = new SelectListItem();
                            map.Value = sda["FLOW_CD"].ToString();
                            map.Text = sda["FLOW_NAME"].ToString();
                            li.Add(map);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        //Add By Alyssa 2015.07.03
        public List<Map> GetApply_Item(string UNIT_CD)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {

                    String sql = @"
                        SELECT DISTINCT SRV_ID,NAME
                        FROM [SERVICE]
						JOIN UNIT ON UNIT.UNIT_CD = SERVICE.FIX_UNIT_CD OR UNIT_PCD = SERVICE.FIX_UNIT_CD
                        WHERE ONLINE_S_MK = 'Y'
                    ";
                    // 非資訊處
                    if (UNIT_CD != "31")
                    {
                        sql += " AND (UNIT_CD = @UNIT_CD OR UNIT_PCD = @UNIT_CD)";
                        DataUtils.AddParameters(dbc, "UNIT_CD", UNIT_CD);
                    }

                    dbc.CommandText = sql;

                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("SRV_ID", sda["SRV_ID"].ToString());
                            map.Add("NAME", sda["NAME"].ToString());
                            li.Add(map);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public CaseQueryModel GetAPPLY(string app_id)
        {
            CaseQueryModel cqm = new CaseQueryModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        //FIX 須加上登入者單位分類判斷
                        String sql = @"
    select a.APP_ID,a.APP_STR_DATE,a.APP_TIME,a.FLOW_CD,a.IDN,a.MOHW_CASE_NO,
        a.NAME,a.PAY_METHOD,a.PAY_POINT,a.PRO_ACC,a.PRO_UNIT_CD,a.SRV_ID,a.PAY_A_FEE,a.PAY_A_PAID,a.PAY_C_FEE,
        a.UNIT_CD,ad.NAME as ACC_NAME,s.NAME as SRV_NAME,a.CASE_BACK_MK,a.PAY_BACK_MK, 
        (SELECT CODE_DESC
        FROM CODE_CD
        WHERE DEL_MK = 'N'
        AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD )
        AND CODE_CD = A.FLOW_CD
        AND CODE_KIND = 'F_CASE_STATUS') AS CASE_STATUS
    from APPLY a 
    LEFT JOIN service s on s.SRV_ID = a.SRV_ID
    LEFT JOIN ADMIN ad on ad.ACC_NO = a.PRO_ACC
    where a.APP_ID=@APP_ID
                                    ";
                        dbc.Parameters.Clear();
                        dbc.CommandText = sql;
                        DataUtils.AddParameters(dbc, "@APP_ID", app_id);
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                cqm.APP_ID = DataUtils.GetDBString(sda, 0);
                                cqm.APP_STR_DATE = DataUtils.GetDBDateTime(sda, 1);
                                cqm.APP_TIME = DataUtils.GetDBDateTime(sda, 2);
                                cqm.FLOW_CD = DataUtils.GetDBString(sda, 3);
                                cqm.IDN = DataUtils.GetDBString(sda, 4);
                                cqm.MOHW_CASE_NO = DataUtils.GetDBString(sda, 5);
                                cqm.NAME = DataUtils.GetDBString(sda, 6);
                                cqm.PAY_METHOD = DataUtils.GetDBString(sda, 7);
                                cqm.PAY_POINT = DataUtils.GetDBString(sda, 8);
                                cqm.PRO_ACC = DataUtils.GetDBString(sda, 9);
                                cqm.PRO_UNIT_CD = DataUtils.GetDBInt(sda, 10);
                                cqm.SRV_ID = DataUtils.GetDBString(sda, 11);
                                cqm.PAY_A_FEE = DataUtils.GetDBInt(sda, 12);
                                cqm.PAY_A_PAID = DataUtils.GetDBInt(sda, 13);
                                cqm.PAY_C_FEE = DataUtils.GetDBInt(sda, 14);
                                cqm.UNIT_CD = DataUtils.GetDBInt(sda, 15);
                                cqm.ACC_NAME = DataUtils.GetDBString(sda, 16);
                                cqm.SRV_NAME = DataUtils.GetDBString(sda, 17);
                                cqm.CASE_BACK_MK = DataUtils.GetDBString(sda, 18).Equals("Y");
                                cqm.PAY_BACK_MK = DataUtils.GetDBString(sda, 19).Equals("Y");
                                cqm.CASE_STATUS = DataUtils.GetDBString(sda, 20);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return cqm;
        }

        private int GetPRO_DEADLINE(string srv_id)
        {
            int d = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"
                                    select PRO_DEADLINE
                                    from service
                                    where SRV_ID=@SRV_ID
                                    ";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    DataUtils.AddParameters(dbc, "SRV_ID", srv_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            d = DataUtils.GetDBInt(sda, 0);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return d;
        }

        public Boolean UpdateAPPLYEdit1(CaseQueryModel model)
        {
            Boolean result = false;
            int d = GetPRO_DEADLINE(model.SRV_ID);
            if (!model.APP_STR_DATE.HasValue)
            {
                model.APP_STR_DATE = DateTime.Now;
            }
            DateTime APP_EXT_DATE = model.APP_STR_DATE.Value.AddDays(d);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    dbc.CommandText = @"update APPLY set
                                        SRV_ID=@SRV_ID,
                                        PAY_POINT=@PAY_POINT,
                                        PAY_A_FEE=@PAY_A_FEE,
                                        PAY_C_FEE=@PAY_C_FEE,
                                        APP_EXT_DATE=@APP_EXT_DATE,
                                        PAY_BACK_MK=@PAY_BACK_MK,
                                        MOHW_CASE_NO=@MOHW_CASE_NO
                                        where APP_ID=@APP_ID";

                    dbc.Parameters.Clear();
                    DataUtils.AddParameters(dbc, "SRV_ID", model.SRV_ID);
                    DataUtils.AddParameters(dbc, "PAY_POINT", model.PAY_POINT);
                    DataUtils.AddParameters(dbc, "PAY_A_FEE", model.PAY_A_FEE);
                    DataUtils.AddParameters(dbc, "PAY_C_FEE", model.PAY_C_FEE);
                    DataUtils.AddParameters(dbc, "APP_EXT_DATE", APP_EXT_DATE);
                    DataUtils.AddParameters(dbc, "PAY_BACK_MK", model.PAY_A_FEE >= model.PAY_A_PAID ? "N" : "Y");
                    DataUtils.AddParameters(dbc, "MOHW_CASE_NO", model.MOHW_CASE_NO);
                    DataUtils.AddParameters(dbc, "APP_ID", model.APP_ID);
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public string GetFLOWCDNAME(string app_id, string flow_cd)
        {
            string temp = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    /*
                    String sql = @"
                                    select m.FLOW_NAME 
                                    from M_CASE_STATUS m
                                    where FLOW_CD=@FLOW_CD 
                                    and unit_scd in (
                                        select unit_scd from unit where unit_cd in (
                                            select unit_cd from apply where APP_ID = @APP_ID
                                        )
                                    )
                                    ";
                    */

                    string sql = @"
                        SELECT CODE_DESC
                        FROM CODE_CD
                        WHERE CODE_KIND = 'F_CASE_STATUS'
                          AND CODE_CD = @FLOW_CD
                          AND CODE_PCD IN (
                            SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD IN (
                                SELECT UNIT_CD FROM APPLY WHERE APP_ID = @APP_ID
                            )
                          )
                    ";

                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    DataUtils.AddParameters(dbc, "FLOW_CD", flow_cd);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            temp = DataUtils.GetDBString(sda, 0);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return temp;
        }

        //帶出機關
        //帶出原始進度
        //判斷新的狀態是否結案
        //判斷新的狀態是否存在於PairCode中
        public Map GetCONFIRM_DATA(string app_id, string unit_cd, string flow_cd)
        {
            Map map = new Map();
            int APP_DEFER_TIMES = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"select FLOW_CD,APP_DEFER_TIMES,APP_DEFER_TIME_S
                                    from APPLY
                                    where APP_ID=@APP_ID 
                                    ";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            map.Add("ORG_FLOW_CD", DataUtils.GetDBString(sda, 0));
                            APP_DEFER_TIMES = DataUtils.GetDBInt(sda, 1);
                            map.Add("APP_DEFER_TIME_S", DataUtils.GetDBDateTime(sda, 2));
                        }
                        sda.Close();
                    }

                    sql = @"select CLOSE_MK,PAIR_CD,UNIT_SCD FROM M_CASE_STATUS 
                            WHERE CASE_CAT='4' and UNIT_SCD in (
                                select UNIT_SCD from UNIT where UNIT_CD=@UNIT_CD
                            ) and FLOW_CD = @FLOW_CD";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    DataUtils.AddParameters(dbc, "UNIT_CD", unit_cd);
                    DataUtils.AddParameters(dbc, "FLOW_CD", flow_cd);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            map.Add("CLOSE_MK", DataUtils.GetDBString(sda, 0));
                            map.Add("PAIR_CD", DataUtils.GetDBString(sda, 1));
                            map.Add("UNIT_SCD", DataUtils.GetDBString(sda, 2));
                        }
                        sda.Close();
                    }

                    sql = @"select FLOW_CD FROM M_CASE_STATUS 
                            WHERE CASE_CAT='4' 
                            and UNIT_SCD = @UNIT_SCD
                            and PAIR_CD = @PAIR_CD";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    DataUtils.AddParameters(dbc, "UNIT_SCD", map.GetString("UNIT_SCD"));
                    DataUtils.AddParameters(dbc, "PAIR_CD", map.GetString("PAIR_CD"));
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            APP_DEFER_TIMES++;
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(map.GetString("PAIR_CD")))
                                APP_DEFER_TIMES--;
                        }
                        if (APP_DEFER_TIMES < 0)
                            APP_DEFER_TIMES = 0;
                        sda.Close();
                    }
                    map.Add("APP_DEFER_TIMES", APP_DEFER_TIMES);
                }
                conn.Close();
                conn.Dispose();
            }
            return map;
        }

        public Boolean UpdateAPPLYEdit2(CaseQueryModel model, Map dataMap)
        {
            Boolean result = false;
            int d = GetPRO_DEADLINE(model.SRV_ID);
            if (!model.APP_STR_DATE.HasValue)
            {
                model.APP_STR_DATE = DateTime.Now;
            }
            DateTime APP_EXT_DATE = model.APP_STR_DATE.Value.AddDays(d);
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"update APPLY set 
                                        FLOW_CD = @FLOW_CD,
                                        CLOSE_MK = @CLOSE_MK,
                                        ";
                    dbc.Parameters.Clear();
                    if (dataMap.GetString("ORG_FLOW_CD").Equals("00"))
                    {
                        sql += " APP_STR_DATE = @APP_STR_DATE ,";
                        sql += " APP_EXT_DATE = @APP_EXT_DATE ,";
                        //dbc.Parameters.Add("@APP_STR_DATE", DateTime.Now);
                        //dbc.Parameters.Add("@APP_EXT_DATE", APP_EXT_DATE);
                        DataUtils.AddParameters(dbc, "APP_STR_DATE", DateTime.Now);
                        DataUtils.AddParameters(dbc, "APP_EXT_DATE", APP_EXT_DATE);
                    }

                    if (dataMap.GetString("CLOSE_MK").Equals("Y"))
                    {
                        sql += " APP_ACT_DATE = @APP_ACT_DATE ,";
                        DataUtils.AddParameters(dbc, "APP_ACT_DATE", DateTime.Now);
                        //判斷是否為退件
                        if (model.CASE_BACK_MK)
                        {
                            sql += " CASE_BACK_MK = 'Y' ,";
                            sql += " CASE_BACK_TIME = @CASE_BACK_TIME ,";
                            DataUtils.AddParameters(dbc, "CASE_BACK_TIME", DateTime.Now);
                            if (model.PAY_BACK_MK)
                            {
                                sql += " PAY_A_FEE = 0 ,";
                                //Edit by janet 090413
                                if (model.PAY_A_PAID > 0)
                                {
                                    sql += " PAY_BACK_MK = 'Y' ,";
                                }
                                else
                                {
                                    sql += " PAY_BACK_MK = 'N' ,";      //不用退款
                                }
                            }
                            else
                            {
                                sql += " PAY_A_FEE = @PAY_A_FEE ,";
                                DataUtils.AddParameters(dbc, "PAY_A_FEE", model.PAY_A_FEE);
                                sql += " PAY_BACK_MK = 'N' ,";
                            }
                        }
                        else
                        {
                            sql += " CASE_BACK_MK = 'N' ,";
                            sql += " CASE_BACK_TIME = null ,";
                            sql += " PAY_A_FEE = @PAY_A_FEE ,";
                            DataUtils.AddParameters(dbc, "PAY_A_FEE", model.PAY_A_FEE);

                            if (model.PAY_A_PAID > model.PAY_A_FEE)
                                sql += " PAY_BACK_MK = 'Y' ,";
                            else
                                sql += " PAY_BACK_MK = 'N' ,";
                        }
                    }
                    else
                    {
                        sql += " APP_ACT_DATE = null,";
                        sql += " CASE_BACK_MK = 'N',";
                        sql += " CASE_BACK_TIME = null ,";
                        sql += " PAY_A_FEE = @PAY_A_FEE ,";
                        DataUtils.AddParameters(dbc, "PAY_A_FEE", model.PAY_A_FEE);

                        if (model.PAY_A_PAID > model.PAY_A_FEE)
                            sql += " PAY_BACK_MK = 'Y' ,";
                        else
                            sql += " PAY_BACK_MK = 'N' ,";
                    }

                    if (dataMap.GetInt("APP_DEFER_TIMES") == 1)
                    {	//若補件為1 時, 要異動補件起始日
                        sql += " APP_DEFER_TIME_S = @APP_DEFER_TIME_S,";
                        DataUtils.AddParameters(dbc, "APP_DEFER_TIME_S", DateTime.Now);
                        sql += " APP_DEFER_MK = 'Y',";
                    }

                    if ((dataMap.GetInt("APP_DEFER_TIMES") == 0) && (dataMap.GetDateTime("APP_DEFER_TIME_S").HasValue))
                    {
                        //若補件為0時, 要異動補件完成日及計算補件天數
                        TimeSpan ts = DateTime.Now - dataMap.GetDateTime("APP_DEFER_TIME_S").Value;
                        sql += " APP_DEFER_TIME_E = @APP_DEFER_TIME_E,";
                        DataUtils.AddParameters(dbc, "APP_DEFER_TIME_E", DateTime.Now);
                        sql += " APP_DEFER_DAYS = @APP_DEFER_DAYS,";
                        DataUtils.AddParameters(dbc, "APP_DEFER_DAYS", ts.Days);
                        sql += " APP_DEFER_MK = 'N',";
                    }
                    sql += " APP_DEFER_TIMES = @APP_DEFER_TIMES where APP_ID=@APP_ID ";
                    DataUtils.AddParameters(dbc, "FLOW_CD", model.FLOW_CD);
                    DataUtils.AddParameters(dbc, "CLOSE_MK", dataMap.GetString("CLOSE_MK"));
                    DataUtils.AddParameters(dbc, "APP_DEFER_TIMES", dataMap.GetString("APP_DEFER_TIMES"));
                    DataUtils.AddParameters(dbc, "APP_ID", model.APP_ID);

                    //dbc.Parameters.Add("@FLOW_CD", model.FLOW_CD);
                    //dbc.Parameters.Add("@CLOSE_MK", dataMap.GetString("CLOSE_MK"));
                    //dbc.Parameters.Add("@APP_DEFER_TIMES", dataMap.GetString("APP_DEFER_TIMES"));
                    //dbc.Parameters.Add("@APP_ID", model.APP_ID);
                    dbc.CommandText = sql;
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public Boolean UpdateAPPLYRePatch(string app_id)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    dbc.CommandText = @"update APPLY set 
                                        APP_DISP_MK = 'N'
                                        where APP_ID = @APP_ID ";

                    dbc.Parameters.Clear();
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        private string GetOrderColumn(string column)
        {
            switch (column)
            {
                case "APP_ID":
                    return "APP_ID";
                case "SRV_ID":
                    return "SRV_ID";
                case "APP_TIME":
                    return "APP_TIME";
            }

            return "";
        }

        private string GetOrderBy(string orderBy)
        {
            switch (orderBy)
            {
                case "ASC":
                    return "ASC";
                case "DESC":
                    return "DESC";
            }
            return "";
        }


        public ES.Models.LeadModel.DocumentFormat GetDocumentData(string serId, string applyId)
        {
            ES.Models.LeadModel.DocumentFormat model = new ES.Models.LeadModel.DocumentFormat();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    dbc.CommandText = @"
SELECT ad.SRV_NO,ad.APP_ID,UNIT_NAME,ADDRESS,NAME,TEL,FAX,MAIL,'申請'+isnull(DRUG_NAME,'')+isnull(LIC_NUM,'')+SUBJECT as SUBJECTTEXT,CAPTION1,CAPTION2+convert(varchar,APP_COUNT)+'份' as caption2,CAPTION3+convert(varchar,AMOUNT)+'元整'+case when PAYMETHOD=1 then '支票乙紙' when PAYMETHOD=2 then '匯票乙紙' when PAYMETHOD=3 then '現金' end as caption3,OTHER1 as caption4,OTHER2 as caption5,OTHER3 as caption6,OTHER4 as caption7,OTHER5 as caption8
FROM APPLY_DOCUMENT ad inner join DOCUMENT_BASE db on ad.SRV_NO=db.SRV_ID 
where ad.SRV_NO = @SRV_NO and ad.APP_ID = @APP_ID";

                    dbc.Parameters.Clear();
                    DataUtils.AddParameters(dbc, "APP_ID", applyId);
                    DataUtils.AddParameters(dbc, "SRV_NO", serId);
                    using (SqlDataReader sr = dbc.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            model.APP_ID = sr["APP_ID"].ToString();
                            model.SRV_NO = sr["SRV_NO"].ToString();
                            model.Title = sr["UNIT_NAME"].ToString();
                            model.Address = sr["ADDRESS"].ToString();
                            model.Name = sr["NAME"].ToString();
                            model.Tel = sr["TEL"].ToString();
                            model.Fax = sr["FAX"].ToString();
                            model.EMail = sr["MAIL"].ToString();
                            model.SubjectText = sr["SUBJECTTEXT"].ToString();
                            model.Caption1 = sr["CAPTION1"].ToString();
                            model.Caption2 = sr["CAPTION2"].ToString();
                            model.Caption3 = sr["CAPTION3"].ToString();
                            model.Caption4 = sr["CAPTION4"].ToString();
                            model.Caption5 = sr["CAPTION5"].ToString();
                            model.Caption6 = sr["CAPTION6"].ToString();
                            model.Caption7 = sr["CAPTION7"].ToString();
                            model.Caption8 = sr["CAPTION8"].ToString();
                        }
                        sr.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return model == null ? new ES.Models.LeadModel.DocumentFormat() : model;
        }
        public List<ApplyDonateDetailGridModel> GetAPPLYDonateDetail(ApplyDonateDetailViewModel model, AccountModel account)
        {
            List<ApplyDonateDetailGridModel> li = new List<ApplyDonateDetailGridModel>();
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand com = conn.CreateCommand();
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                    select a.APP_ID,c.NAME_CH, b.NAME, case b.PAY_METHOD when 'C' then '信用卡線上刷卡' when 'S' then '超商條碼捐款' when 'T' then 'WebATM' when 'L' then 'LinePay' end as PAY_METHOD_NAME,
						b.PAY_A_FEE, case d.PAY_STATUS_MK when 'N' then '否' else '是' end as PAY_STATUS_MK, case when isnull(a.DONOR_IDN,'') ='' then '否' else '是' end as ISDONOR, isnull(a.DONOR_IDN,'') as DONOR_IDN
						from APPLY_007001 a
						 join apply b on a.APP_ID = b.APP_ID
						 join APPLY_DONATE c on b.SRV_ID = c.SRV_ID_DONATE
						 join apply_pay d on a.APP_ID = d.APP_ID
                        where b.DEL_MK = 'N'
                ");
                // 捐款人姓名
                if (!string.IsNullOrWhiteSpace(model.Form.NAME))
                {
                    sb.Append(@" and b.NAME like '%' + @NAME + '%'");
                }
                // 賑災專案
                if (!string.IsNullOrWhiteSpace(model.Form.SRV_ID_DONATE))
                {
                    sb.Append(@" and SRV_ID like '%' + @SRV_ID");
                }
                // 捐款方式
                if (!string.IsNullOrWhiteSpace(model.Form.PAYWAY))
                {
                    sb.Append(@" and b.PAY_METHOD = @PAYWAY");
                }
                // 是否已捐款
                if (!string.IsNullOrWhiteSpace(model.Form.ISPAY))
                {
                    sb.Append(@" and d.PAY_STATUS_MK = @ISPAY");
                }
                // 是否同意上傳國稅局
                if (!string.IsNullOrWhiteSpace(model.Form.ISAGREE))
                {
                    if (model.Form.ISAGREE == "Y")
                    {
                        sb.Append(@" and isnull(a.DONOR_IDN,'') <> '' ");
                    }
                    if (model.Form.ISAGREE == "N")
                    {
                        sb.Append(@" and isnull(a.DONOR_IDN,'') = '' ");
                    }
                }
                // CVS
                if (!string.IsNullOrEmpty(model.Form.CSV_YEAR))
                {
                    sb.Append(@" and ISNULL(YEAR(b.APP_TIME),0)-1911 = @CSV_YEAR");
                }
                com.CommandText = sb.ToString();
                com.Parameters.Clear();
                if (!string.IsNullOrWhiteSpace(model.Form.NAME))
                {
                    DataUtils.AddParameters(com, "NAME", model.Form.NAME);
                }
                if (!string.IsNullOrWhiteSpace(model.Form.SRV_ID_DONATE))
                {
                    DataUtils.AddParameters(com, "SRV_ID", model.Form.SRV_ID_DONATE);
                }
                if (!string.IsNullOrWhiteSpace(model.Form.PAYWAY))
                {
                    DataUtils.AddParameters(com, "PAYWAY", model.Form.PAYWAY);
                }
                if (!string.IsNullOrWhiteSpace(model.Form.ISPAY))
                {
                    DataUtils.AddParameters(com, "ISPAY", model.Form.ISPAY);
                }
                if (!string.IsNullOrWhiteSpace(model.Form.CSV_YEAR))
                {
                    DataUtils.AddParameters(com, "CSV_YEAR", model.Form.CSV_YEAR);
                }
                //查詢條件
                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        ApplyDonateDetailGridModel cqm = new ApplyDonateDetailGridModel();
                        cqm.APP_ID = sr["APP_ID"].TONotNullString();
                        cqm.NAME_CH = sr["NAME_CH"].TONotNullString();
                        cqm.NAME = sr["NAME"].TONotNullString();
                        cqm.PAY_METHOD_NAME = sr["PAY_METHOD_NAME"].TONotNullString();
                        cqm.PAY_A_FEE = sr["PAY_A_FEE"].TOInt32();
                        cqm.PAY_STATUS_MK = sr["PAY_STATUS_MK"].TONotNullString();
                        cqm.ISDONOR = sr["ISDONOR"].TONotNullString();
                        cqm.DONOR_IDN = sr["DONOR_IDN"].TONotNullString();
                        li.Add(cqm);
                    }
                    sr.Close();
                }
                if (li != null)
                {
                    this.totalCount = li.Count();
                    li = li.Skip((model.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                com.Dispose();
            }

            return li == null ? new List<ApplyDonateDetailGridModel>() : li;
        }
        public List<ApplyDonateCSVModel> GetAPPLYDonateCSV(ApplyDonateDetailViewModel model)
        {
            List<ApplyDonateCSVModel> li = new List<ApplyDonateCSVModel>();
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand com = conn.CreateCommand();
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                    select ISNULL(YEAR(b.APP_TIME),0)-1911 as DON_YR,ISNULL(a.DONOR_IDN,'') as DON_IDN,
ISNULL(a.DONOR_NAME,'') as DON_NM,ISNULL(a.AMOUNT,'') as DON_AMT,'' as DON_BAN,'22' as DON_KD,'衛生福利部' as DONEE_NM
from APPLY_007001 a
	join apply b on a.APP_ID = b.APP_ID
	join APPLY_DONATE c on b.SRV_ID = c.SRV_ID_DONATE
	join apply_pay d on a.APP_ID = d.APP_ID
where b.DEL_MK = 'N'
--and d.PAY_STATUS_MK = 'Y'
                ");
                if (!string.IsNullOrWhiteSpace(model.Form.CSV_YEAR))
                {
                    sb.Append(@" and ISNULL(YEAR(b.APP_TIME),0)-1911 = " + model.Form.CSV_YEAR);
                }
                com.CommandText = sb.ToString();
                //查詢條件
                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        ApplyDonateCSVModel cqm = new ApplyDonateCSVModel();
                        cqm.DON_YR = sr["DON_YR"].TONotNullString();
                        cqm.DON_IDN = sr["DON_IDN"].TONotNullString();
                        cqm.DON_NM = sr["DON_NM"].TONotNullString();
                        cqm.DON_AMT = sr["DON_AMT"].TONotNullString();
                        cqm.DON_BAN = sr["DON_BAN"].TONotNullString();
                        cqm.DON_KD = sr["DON_KD"].TONotNullString();
                        cqm.DONEE_NM = sr["DONEE_NM"].TONotNullString();
                        li.Add(cqm);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                com.Dispose();
            }

            return li == null ? new List<ApplyDonateCSVModel>() : li;
        }

        public List<SelectListItem> GetAccountUnit(string UNIT_CD)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var dao = new ShareCodeListModel();
            var t = dao.GetUnitList;
            var item = t.Where(x => x.Value == UNIT_CD).FirstOrDefault();
            // 資訊處
            if (UNIT_CD == "31")
            {
                // 可選清單
                result.AddRange(t);
            }
            else
            {
                result.Add(item);
            }
            return result;
        }
    }
}