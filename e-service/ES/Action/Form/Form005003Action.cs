using ES.Areas.Admin.Controllers;
using ES.Models;
using ES.Models.ViewModels;
using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Xceed.Words.NET;
using ES.Services;
using System.Web.Hosting;

namespace ES.Action.Form
{
    public class Form005003Action : FormBaseWordAction
    {
        public Form005003Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form005003Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(String id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
    SELECT A.SRV_ID, A.APP_TIME, A.NAME AS MF_NAME, A.ENAME AS MF_ENAME
    ,B.*
    ,CONVERT(VARCHAR, F_2A_1_DATE, 102) AS F_2A_1_DATE_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_1_2 AND CODE_KIND = 'F_YN_1') AS F_1_2_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_1_3 AND CODE_KIND = 'F_YN_1') AS F_1_3_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_2A_3 AND CODE_KIND = 'F5_WHO_STATUS') AS F_2A_3_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_2A_4 AND CODE_KIND = 'F_YN_1') AS F_2A_4_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_2A_5 AND CODE_KIND = 'F_YN_1') AS F_2A_5_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_2B_2 AND CODE_KIND = 'F5_WHO_STATUS') AS F_2B_2_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_3_0 AND CODE_KIND = 'F_YN_1') AS F_3_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_3_2 AND CODE_KIND = 'F_YN_1') AS F_3_2_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_3_3 AND CODE_KIND = 'F_YN_1') AS F_3_3_F
    ,(SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.F_4 AND CODE_KIND = 'F_YN_1') AS F_4_F
    FROM APPLY A
    JOIN APPLY_005003 B ON A.APP_ID = B.APP_ID 
    WHERE 1=1
    AND A.APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetData(querySQL, args);
        }

        /// <summary>
        /// APPLY_005003_IC
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetList_IC(string id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"SELECT * FROM APPLY_005003_IC WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetList(querySQL, args);
        }

        /// <summary>
        /// APPLY_005003_F11
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetList_F11(string id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"SELECT * FROM APPLY_005003_F11 WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetList(querySQL, args);
        }

        /// <summary>
        /// 005003_申請單套表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte[] GetApplyWord(string id)
        {
            //Dictionary<string, object> data = GetData(id);
            //List<Dictionary<string, object>> listIC = GetList_IC(id);
            //List<Dictionary<string, object>> listF11 = GetList_F11(id);

            ShareCodeListModel sc = new ShareCodeListModel();
            DataLayers.ApplyDAO dao = new DataLayers.ApplyDAO();
            Apply_005003AppDocModel fm = dao.QueryApply_005003(id);

            //string path = Server.MapPath("~/Sample/apply005003.docx");
            string path = DataUtils.GetConfig("FOLDER_TEMPLATE") + "005003";//data["SRV_ID"];
            //string templateFile = path + "/apply005003.docx";
            //string path = System.Web.HttpContext.Current.Server.MapPath("~/Sample/apply005003.docx");
            string templateFile = HostingEnvironment.MapPath("~/Sample/apply005003.docx");

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(templateFile))
                {
                    string F_1_2_YN = sc.Get_YesNo_TEXT(fm.F_1_2);
                    string F_1_3_YN = sc.Get_YesNo_TEXT(fm.F_1_3);
                    //申請日期
                    //doc.ReplaceText("$APP_TIME_TW", "中華民國" + Form.APP_TIME_TW.Split('/')[0] + "年" + Form.APP_TIME_TW.Split('/')[1] + "月" + Form.APP_TIME_TW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IMP_COUNTRY$]", fm.MF_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1$]", fm.F_1.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_1$]", fm.F_1_1.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_2_F$]", F_1_2_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_3_F$]", F_1_3_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);


                    string F_2A_1_NUM_WN = string.Format("License number:{0}-{1}", fm.F_2A_1_WORD, fm.F_2A_1_NUM);
                    string F_2A_1_DATE_US = sc.Get_DATE_US_TEXT(fm.F_2A_1_DATE);
                    doc.ReplaceText("[$F_2A_1_NUM$]", F_2A_1_NUM_WN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_2A_1_DATE_F$]", F_2A_1_DATE_US.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    if (fm.F_1_2.TONotNullString().Equals("Y"))
                    {
                        string F_2A_4_YN = sc.Get_YesNo_TEXT(fm.F_2A_4);
                        string F_2A_5_YN = sc.Get_YesNo_TEXT(fm.F_2A_5);
                        string F_2A_6_NAME = fm.F_2A_6_NAME.TONotNullString();
                        string F_2A_6_ADDR = fm.F_2A_6_ADDR.TONotNullString();
                        if (fm.F_2A_6.TONotNullString().Equals("Y"))
                        {
                            F_2A_6_NAME = "The same as the license holder.";
                            F_2A_6_ADDR = "";
                        }
                        doc.ReplaceText("[$F_2A_2$]", fm.F_2A_2.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3$]", fm.F_2A_3.TONotNullString() + ".", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_4_F$]", F_2A_4_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_5_F$]", F_2A_5_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_NAME$]", F_2A_6_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_ADDR$]", F_2A_6_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                        doc.ReplaceText("[$F_2B_1_NAME$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_2_ADDR$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_2$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_3$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_1_NAME$]", fm.F_2A_3_1_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_2_ADDR$]", fm.F_2A_3_2_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_3_REMARKS$]", fm.F_2B_3_REMARKS.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else
                    {
                        doc.ReplaceText("[$F_2A_2$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_4_F$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_5_F$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_NAME$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_ADDR$]", "", false, System.Text.RegularExpressions.RegexOptions.None);

                        fm.F_2B_1_NAME = fm.F_2A_6_NAME.TONotNullString();
                        fm.F_2B_2_ADDR = fm.F_2A_6_ADDR.TONotNullString();
                        //*2B.3僅供外銷專用之原因
                        string F_2B_3_Eng = sc.Get_PList2B3_ENG(fm.F_2B_3);
                        doc.ReplaceText("[$F_2B_1_NAME$]", fm.F_2B_1_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_2_ADDR$]", fm.F_2B_2_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_2$]", fm.F_2B_2.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_3$]", F_2B_3_Eng.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                        doc.ReplaceText("[$F_2A_3_1_NAME$]", fm.F_2A_3_1_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_2_ADDR$]", fm.F_2A_3_2_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_3_REMARKS$]", fm.F_2B_3_REMARKS.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    logger.Debug("##fm.MF_ADDR.TONotNullString():" + fm.MF_ADDR.TONotNullString());
                    logger.Debug("##fm.F_2B_1_NAME.TONotNullString():" + fm.F_2B_1_NAME.TONotNullString());

                    string F_3_0_YN = sc.Get_YesNo_TEXT(fm.F_3_0);
                    string F_3_1 = string.Format("{0} years.", fm.F_3_1.TONotNullString());
                    string F_3_2_YN = sc.Get_YesNo_TEXT(fm.F_3_2);
                    string F_3_3_YN = sc.Get_YesNo_TEXT(fm.F_3_3);
                    string F_4_YN = sc.Get_YesNo_TEXT(fm.F_4);
                    doc.ReplaceText("[$F_3_F$]", F_3_0_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_3_1$]", F_3_1.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_3_2_F$]", F_3_2_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_3_3_F$]", F_3_3_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_4_F$]", F_4_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }
            return buffer;

            //string IMP_COUNTRY = "";
            //string F_1_1 = "";
            //for (int i = 0; i < listIC.Count; i++)
            //{
            //    IMP_COUNTRY += listIC[i]["IMP_COUNTRY"];
            //    if (i < listIC.Count - 1) IMP_COUNTRY += ", ";
            //}

            //for (int i = 0; i < listF11.Count; i++)
            //{
            //    string F11_SCI_NAME = listF11[i]["F11_SCI_NAME"].ToString().Replace("&", "&amp;");
            //    string F11_QUANTITY = listF11[i]["F11_QUANTITY"].ToString();
            //    string F11_UNIT = listF11[i]["F11_UNIT"].ToString();
            //    string str_F11 = string.Format("{0}.....{1}{2}", F11_SCI_NAME, F11_QUANTITY, F11_UNIT);
            //    F_1_1 += str_F11;
            //    //F_1_1 += list2[i]["F11_SCI_NAME"];
            //    if (i < listF11.Count - 1) F_1_1 += ", ";
            //}

            //data.Add("IMP_COUNTRY", IMP_COUNTRY);
            //data.Add("F_1_1", F_1_1);

            //string path = DataUtils.GetConfig("FOLDER_TEMPLATE") + data["SRV_ID"];
            //string templateFile = path + "/藥品產銷證明(WHO_format)_0000914001.docx";
            //string tempFile = path + "/" + new DateTime().ToString("yyyyMMddHHmmssfff") + ".docx";
            //logger.Debug(string.Format("##GetApplyWord(string id) tempFile:{0}", tempFile));
            //return MarkDocx(tempFile, templateFile, data);
        }
    }
}