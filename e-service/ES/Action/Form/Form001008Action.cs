using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using iTextSharp.text;
using log4net;
using iTextSharp.text.pdf;
using System.Data.SqlClient;
using ES.Utils;
using System.Text;

namespace ES.Action.Form
{
    public class Form001008Action : FormBaseAction
    {
        public Form001008Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form001008Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            /*string querySQL = @"
                SELECT A.SRV_ID, A.APP_ID, A.APP_TIME, A.IDN, A.NAME, A.ENAME, A.BIRTHDAY, A.ADDR, A.TEL,
                    B.ENAME_ALIAS, B.MAIL_ADDR, B.COPIES, B.REMARK,
                    ISNULL(PAY_A_FEE, 0) + ISNULL(PAY_A_FEEBK, 0) + ISNULL(PAY_C_FEE, 0) + ISNULL(PAY_C_FEEBK, 0) AS PAY_TOTAL,
                    (SELECT COUNT(1) FROM APPLY_001008_ME WHERE ME_TYPE_CD = '1' AND APP_ID = A.APP_ID) AS ME_TYPE_1_COUNT,
                    (SELECT COUNT(1) FROM APPLY_001008_ME WHERE ME_TYPE_CD = '2' AND APP_ID = A.APP_ID) AS ME_TYPE_2_COUNT,
                    (SELECT COUNT(1) FROM APPLY_001008_PR WHERE PR_TYPE_CD = '1' AND APP_ID = A.APP_ID) AS PR_TYPE_1_COUNT,
                    (SELECT COUNT(1) FROM APPLY_001008_PR WHERE PR_TYPE_CD = '2' AND APP_ID = A.APP_ID) AS PR_TYPE_2_COUNT
                FROM APPLY A, APPLY_001008 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.APP_ID = @APP_ID
            ";*/
            //20201031 國內外寄送地址改成多筆建檔
            string querySQL = @"
                SELECT A.SRV_ID, A.APP_ID, A.APP_TIME, A.IDN, A.NAME, A.ENAME, A.BIRTHDAY, 
                    CONCAT(ISNULL(A.ADDR_CODE,''),ISNULL(CONCAT(ISNULL(X1.CITYNM,''), ISNULL(X1.TOWNNM,'')),'') ,REPLACE(A.ADDR,CONCAT(ISNULL(X1.CITYNM,''), ISNULL(X1.TOWNNM,'')),'')) ADDR,
                    A.TEL,B.ENAME_ALIAS, 
                    (STUFF((
	                    SELECT '、' + X.ADDR 
	                    FROM (
		                    SELECT DISTINCT '1' ITYPE,A.APP_ID,
                            CONCAT(ISNULL(A.TRANS_ZIP,''),' ', CONCAT(ISNULL(B.CITYNM,''),ISNULL(B.TOWNNM,'')), REPLACE(A.TRANS_ADDR,CONCAT(ISNULL(B.CITYNM,''),ISNULL(B.TOWNNM,'')),'')) AS ADDR
		                    FROM APPLY_001008_TRANS A
		                    LEFT JOIN ZIPCODE B ON A.TRANS_ZIP=B.ZIP_CO
		                    WHERE A.APP_ID=@APP_ID

		                    UNION

		                    SELECT '2' ITYPE,APP_ID, TRANSF_ADDR ADDR
		                    FROM APPLY_001008_TRANSF 
		                    WHERE APP_ID=@APP_ID
	                    ) X
	                    WHERE 1=1
	                    AND A.APP_ID=X.APP_ID
	                    FOR XML PATH('')
	                    ),1,1,'') 
                    ) MAIL_ADDR,
                    B.COPIES, B.REMARK,
                    ISNULL(PAY_A_FEE, 0) + ISNULL(PAY_A_FEEBK, 0) + ISNULL(PAY_C_FEE, 0) + ISNULL(PAY_C_FEEBK, 0) AS PAY_TOTAL,
                    (SELECT COUNT(1) FROM APPLY_001008_ME WHERE ME_TYPE_CD = '1' AND APP_ID = A.APP_ID) AS ME_TYPE_1_COUNT,
                    (SELECT COUNT(1) FROM APPLY_001008_ME WHERE ME_TYPE_CD = '2' AND APP_ID = A.APP_ID) AS ME_TYPE_2_COUNT,
                    (SELECT COUNT(1) FROM APPLY_001008_PR WHERE PR_TYPE_CD = '1' AND APP_ID = A.APP_ID) AS PR_TYPE_1_COUNT,
                    (SELECT COUNT(1) FROM APPLY_001008_PR WHERE PR_TYPE_CD = '2' AND APP_ID = A.APP_ID) AS PR_TYPE_2_COUNT
                FROM APPLY A
                JOIN APPLY_001008 B ON A.APP_ID=B.APP_ID
                LEFT JOIN ZIPCODE X1 ON A.ADDR_CODE=X1.ZIP_CO
                WHERE 1=1
                    AND A.APP_ID = @APP_ID
            ";

            args.Add("APP_ID", applyId);

            return GetData(querySQL, args);
        }

        public List<Dictionary<string, object>> GetListME(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT T.*,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = T.ME_LIC_TYPE AND CODE_KIND = 'F1_LICENSE_CD_1') AS ME_LIC_TYPE_DESC
                FROM APPLY_001008_ME T WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", applyId);

            return GetList(querySQL, args);
        }

        public List<Dictionary<string, object>> GetListPR(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT T.*,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = T.PR_LIC_TYPE AND CODE_KIND = 'F1_LICENSE_CD_2') AS PR_LIC_TYPE_DESC
                FROM APPLY_001008_PR T WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", applyId);

            return GetList(querySQL, args);
        }

        public byte[] GetApplyPDF(Dictionary<string, object> data)
        {
            MemoryStream ms = new MemoryStream();

            try
            {
                List<Dictionary<string, object>> listME = (List<Dictionary<string, object>>)data["LIST_ME"];
                List<Dictionary<string, object>> listPR = (List<Dictionary<string, object>>)data["LIST_PR"];
                PdfReader reader = new PdfReader(HttpContext.Current.Server.MapPath("~/Sample/001008醫事人員請領英文證明申請書1.pdf"));
                //PdfReader reader = new PdfReader(DataUtils.GetConfig("FOLDER_TEMPLATE") + data["SRV_ID"] + "\\醫事人員請領英文證明申請書.pdf");
                PdfStamper stamp = new PdfStamper(reader, ms);

                BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font8 = new iTextSharp.text.Font(bf, 8);
                iTextSharp.text.Font font10 = new iTextSharp.text.Font(bf, 10);
                iTextSharp.text.Font font12 = new iTextSharp.text.Font(bf, 12);
                iTextSharp.text.Font font14 = new iTextSharp.text.Font(bf, 14);
                iTextSharp.text.Font font16 = new iTextSharp.text.Font(bf, 16);
                iTextSharp.text.Font font18 = new iTextSharp.text.Font(bf, 18);

                iTextSharp.text.Font font11s = new iTextSharp.text.Font(bf, 11, Font.STRIKETHRU);
                iTextSharp.text.Font font16s = new iTextSharp.text.Font(bf, 16, Font.STRIKETHRU);

                iTextSharp.text.Font font16u = new iTextSharp.text.Font(bf, 16, Font.UNDERLINE);

                #region P1
                PdfContentByte cb1 = stamp.GetOverContent(1);

                SetApplyId(cb1, font12, GetData(data["APP_ID"]));

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 240, 728, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ENAME"]), font16), 240, 703, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ENAME_ALIAS"]), font16), 270, 679, 0);

                string t = "";
                foreach (Dictionary<string, object> item in listME)
                {
                    t += GetData(item["ME_LIC_TYPE_DESC"]) + GetData(item["ME_COPIES"])  + "份、";
                }
                if (t.EndsWith("、")) t = t.Substring(0, t.Length - 1);

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(t, font12), 96, 597, 0);

                t = "";
                foreach (Dictionary<string, object> item in listPR)
                {
                    t += GetData(item["PR_LIC_TYPE_DESC"]) + GetData(item["PR_COPIES"]) + "份、";
                }
                if (t.EndsWith("、")) t = t.Substring(0, t.Length - 1);

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(t, font12), 96, 538, 0);

                if ((int)data["ME_TYPE_2_COUNT"] == 0 && (int)data["PR_TYPE_2_COUNT"] == 0)
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font14), 89, 521, 0); // 以上申請之各類（科）別證明資料，請分別開立。
                }
                if ((int)data["ME_TYPE_1_COUNT"] == 0 && (int)data["PR_TYPE_1_COUNT"] == 0)
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font14), 89, 505, 0); // 以上申請之各類（科）別證明資料，請整併於一張證明書。
                }
                if ((int)data["ME_TYPE_1_COUNT"] + (int)data["PR_TYPE_1_COUNT"] > 0 && (int)data["ME_TYPE_2_COUNT"] + (int)data["PR_TYPE_2_COUNT"] > 0)
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font14), 89, 482, 0);

                    string s1 = "", s2 = "";

                    foreach (Dictionary<string, object> item in listME)
                    {
                        if (item["ME_TYPE_CD"].Equals("1"))
                        {
                            s1 += GetData(item["ME_LIC_TYPE_DESC"]) + GetData(item["ME_COPIES"]) + "份、";
                        }

                        if (item["ME_TYPE_CD"].Equals("2"))
                        {
                            s2 += GetData(item["ME_LIC_TYPE_DESC"]) + GetData(item["ME_COPIES"]) + "份、";
                        }
                    }
                    foreach (Dictionary<string, object> item in listPR)
                    {
                        if (item["PR_TYPE_CD"].Equals("1"))
                        {
                            s1 += GetData(item["PR_LIC_TYPE_DESC"]) + GetData(item["PR_COPIES"]) + "份、";
                        }

                        if (item["PR_TYPE_CD"].Equals("2"))
                        {
                            s2 += GetData(item["PR_LIC_TYPE_DESC"]) + GetData(item["PR_COPIES"]) + "份、";
                        }
                    }

                    if (s1.EndsWith("、"))
                    {
                        s1 = s1.Substring(0, s1.Length - 1);
                    }
                    if (s2.EndsWith("、"))
                    {
                        s2 = s2.Substring(0, s2.Length - 1);
                    }

                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(s1, font12), 110, 481, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(s2, font12), 110, 463, 0);
                }

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font14), 198, 453, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font14), 198, 433, 0);

                /*
                if (GetData(data["MAIL_ADDR"]).Length > 65)
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["MAIL_ADDR"]), font10), 94, 380, 0);
                }
                else
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["MAIL_ADDR"]), font14), 94, 380, 0);
                }
                 */
                ColumnText text1 = new ColumnText(cb1);
                if (Encoding.GetEncoding("big5").GetBytes(GetData(data["MAIL_ADDR"])).Length > 64)
                {
                    text1.SetSimpleColumn(new Phrase(GetData(data["MAIL_ADDR"]), font10), 94, 176, 555, 415, 18, Element.ALIGN_LEFT);
                    text1.SetLeading(0f, 1f);
                }
                else
                {
                    text1.SetSimpleColumn(new Phrase(GetData(data["MAIL_ADDR"]), font14), 94, 100, 555, 415, 18, Element.ALIGN_LEFT);
                }
                text1.Go();

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["COPIES"]), font16), 240, 381, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["PAY_TOTAL"]), font16), 330, 381, 0);

                ColumnText text2 = new ColumnText(cb1);
                text2.SetSimpleColumn(new Phrase(GetData(data["REMARK"]).Replace("\r\n", " "), font12), 130, 335, 550, 383, 18, Element.ALIGN_LEFT);
                text2.Go();

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 248, 85, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 320, 85, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 392, 85, 0);

                #endregion

                stamp.Close();
                reader.Close();
                return ms.ToArray();
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }

            return ms.ToArray();
        }
    }
}