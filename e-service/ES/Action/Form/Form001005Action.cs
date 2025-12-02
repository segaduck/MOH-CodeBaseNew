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
    public class Form001005Action : FormBaseAction
    {
        public Form001005Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form001005Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.SRV_ID, A.APP_ID, A.APP_TIME, A.IDN, A.NAME, A.ADDR, A.TEL, B.ISSUE_DATE, B.LIC_NUM,
                    B.ACTION_TYPE, B.ISSUE_DEPT, B.ACTION_RES, B.OTHER_RES,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ACTION_TYPE AND CODE_KIND = 'F1_ACTION_TYPE_1') AS ACTION_TYPE_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ISSUE_DEPT AND CODE_KIND = 'F1_ISSUE_DEPT_1') AS ISSUE_DEPT_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_TYPE AND CODE_KIND = 'F1_LICENSE_CD_1') AS LIC_TYPE_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_CD AND CODE_KIND = 'F1_LICENSE_CD_1') AS LIC_CD_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ACTION_RES AND CODE_KIND = 'F1_ACTION_RES_1') AS ACTION_RES_DESC
                FROM APPLY A, APPLY_001005 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.APP_ID = @APP_ID
            ";

            args.Add("APP_ID", applyId);

            return GetData(querySQL, args);
        }

        public byte[] GetApplyPDF(Dictionary<string, object> data)
        {
            MemoryStream ms = new MemoryStream();

            try
            {
                PdfReader reader = new PdfReader(HttpContext.Current.Server.MapPath("~/Sample/001005醫事人員證書補換領申請書1.pdf"));
                if (!Convert.IsDBNull(data["OTHER_RES"]))
                {
                    reader = new PdfReader(HttpContext.Current.Server.MapPath("~/Sample/001005醫事人員證書補換領申請書.pdf"));
                }

                PdfStamper stamp = new PdfStamper(reader, ms);

                BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font12 = new iTextSharp.text.Font(bf, 12);
                iTextSharp.text.Font font16 = new iTextSharp.text.Font(bf, 16);
                iTextSharp.text.Font font18 = new iTextSharp.text.Font(bf, 18);

                iTextSharp.text.Font font11s = new iTextSharp.text.Font(bf, 11, Font.STRIKETHRU);
                iTextSharp.text.Font font16s = new iTextSharp.text.Font(bf, 16, Font.STRIKETHRU);

                iTextSharp.text.Font font16u = new iTextSharp.text.Font(bf, 16, Font.UNDERLINE);

                #region P1
                PdfContentByte cb1 = stamp.GetOverContent(1);

                SetApplyId(cb1, font12, GetData(data["APP_ID"]));

                if (data["ACTION_TYPE"].Equals("1")) // 補發
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font18), 402, 738, 0);
                }
                else // 換發
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font18), 402, 754, 0);
                }

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font16), 150, 698, 0);

                // ISSUE_DEPT
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ISSUE_DEPT_DESC"]), font12), 252, 698, 0);

                //  ((DateTime)data["ISSUE_DATE"]).Year - 1911
                if (!Convert.IsDBNull(data["ISSUE_DATE"]))
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Year - 1911).ToString(), font16), 410, 698, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Month).ToString("D2"), font16), 455, 698, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Day).ToString("D2"), font16), 503, 698, 0);
                }

                // LIC_CD_DESC
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_CD_DESC"]), font16), 110, 654, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_NUM"]), font16), 200, 654, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_TYPE_DESC"]), font12), 304, 654, 0);

                // data["ACTION_RES_DESC"]
                if (!Convert.IsDBNull(data["OTHER_RES"]))
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ACTION_RES_DESC"]), font16), 84, 610, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["OTHER_RES"]), font16u), 124, 610, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 260, 345, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["IDN"]), font16), 278, 313, 0);
                    //ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font16), 242, 280, 0);

                    ColumnText text1 = new ColumnText(cb1);
                    if (Encoding.GetEncoding("big5").GetBytes(GetData(data["ADDR"])).Length > 40)
                    {
                        text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font12), 242, 303, 550, 200, 18, Element.ALIGN_LEFT);
                        text1.SetLeading(0f, 1f);
                    }
                    else
                    {
                        text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font16), 242, 300, 550, 200, 18, Element.ALIGN_LEFT);
                    }
                    text1.Go();

                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font16), 242, 251, 0);

                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 226, 219, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 295, 219, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 355, 219, 0);
                }
                else
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ACTION_RES_DESC"]), font16), 420, 654, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["OTHER_RES"]), font16u), 460, 654, 0);

                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 260, 387, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["IDN"]), font16), 278, 355, 0);
                    //ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font16), 242, 280, 0);

                    ColumnText text1 = new ColumnText(cb1);
                    if (Encoding.GetEncoding("big5").GetBytes(GetData(data["ADDR"])).Length > 40)
                    {
                        text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font12), 242, 345, 550, 200, 18, Element.ALIGN_LEFT);
                        text1.SetLeading(0f, 1f);
                    }
                    else
                    {
                        text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font16), 242, 342, 550, 200, 18, Element.ALIGN_LEFT);
                    }
                    text1.Go();

                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font16), 242, 293, 0);

                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 226, 261, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 295, 261, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 355, 261, 0);
                }


                #endregion

                #region P2
                PdfContentByte cb2 = stamp.GetOverContent(2);

                SetApplyId(cb2, font12, GetData(data["APP_ID"]));

                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font16), 140, 666, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["ISSUE_DEPT_DESC"]), font12), 232, 666, 0);
                if (!Convert.IsDBNull(data["ISSUE_DATE"]))
                {
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Year - 1911).ToString(), font16), 386, 666, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Month).ToString("D2"), font16), 431, 666, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Day).ToString("D2"), font16), 476, 666, 0);
                }
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_CD_DESC"]), font16), 126, 629, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_NUM"]), font16), 225, 629, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_TYPE_DESC"]), font12), 325, 629, 0);
                if (!Convert.IsDBNull(data["OTHER_RES"]))
                {
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["ACTION_RES_DESC"]), font16), 84, 593, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["OTHER_RES"]), font16u), 124, 593, 0);

                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 238, 322, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["IDN"]), font16), 268, 292, 0);
                    //ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font16), 232, 274, 0);

                    ColumnText text2 = new ColumnText(cb2);
                    if (Encoding.GetEncoding("big5").GetBytes(GetData(data["ADDR"])).Length > 40)
                    {
                        text2.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font12), 232, 279, 550, 200, 18, Element.ALIGN_LEFT);
                        text2.SetLeading(0f, 1f);
                    }
                    else
                    {
                        text2.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font16), 232, 276, 550, 200, 18, Element.ALIGN_LEFT);
                    }
                    text2.Go();

                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font16), 232, 227, 0);

                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 270, 127, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 346, 127, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 420, 127, 0);
                }
                else
                {
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["ACTION_RES_DESC"]), font16), 435, 630, 0);

                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 238, 321, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["IDN"]), font16), 268, 289, 0);
                    //ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font16), 232, 274, 0);

                    ColumnText text2 = new ColumnText(cb2);
                    if (Encoding.GetEncoding("big5").GetBytes(GetData(data["ADDR"])).Length > 40)
                    {
                        text2.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font12), 232, 279, 550, 200, 18, Element.ALIGN_LEFT);
                        text2.SetLeading(0f, 1f);
                    }
                    else
                    {
                        text2.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font16), 232, 276, 550, 200, 18, Element.ALIGN_LEFT);
                    }
                    text2.Go();

                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font16), 232, 227, 0);

                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 270, 125, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 346, 125, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 420, 125, 0);
                }
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