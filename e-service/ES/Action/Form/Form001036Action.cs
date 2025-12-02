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
    public class Form001036Action : FormBaseAction
    {
        public Form001036Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form001036Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.SRV_ID, A.APP_ID, A.APP_TIME, A.IDN, A.NAME, A.ADDR, A.TEL, B.ISSUE_DATE, B.LIC_NUM,
                    B.ACTION_TYPE, B.ACTION_RES, B.OTHER_RES,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ACTION_TYPE AND CODE_KIND = 'F1_ACTION_TYPE_1') AS ACTION_TYPE_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_TYPE AND CODE_KIND = 'F1_LICENSE_CD_3') AS LIC_TYPE_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_CD AND CODE_KIND = 'F1_LICENSE_CD_3') AS LIC_CD_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ACTION_RES AND CODE_KIND = 'F1_ACTION_RES_2') +
                    (CASE WHEN B.OTHER_RES <> '' THEN '[' + B.OTHER_RES + ']' ELSE '' END) AS ACTION_RES_DESC
                FROM APPLY A, APPLY_001036 B
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
                PdfReader reader = new PdfReader(DataUtils.GetConfig("FOLDER_TEMPLATE") + data["SRV_ID"] + "/1029專科護理師證書補換領申請書.pdf");
                PdfStamper stamp = new PdfStamper(reader, ms);

                BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font10 = new iTextSharp.text.Font(bf, 10);
                iTextSharp.text.Font font12 = new iTextSharp.text.Font(bf, 12);
                iTextSharp.text.Font font16 = new iTextSharp.text.Font(bf, 16);
                iTextSharp.text.Font font18 = new iTextSharp.text.Font(bf, 18);

                iTextSharp.text.Font font11s = new iTextSharp.text.Font(bf, 11, Font.STRIKETHRU);
                iTextSharp.text.Font font16s = new iTextSharp.text.Font(bf, 16, Font.STRIKETHRU);

                iTextSharp.text.Font font16u = new iTextSharp.text.Font(bf, 16, Font.UNDERLINE);

                #region P1
                PdfContentByte cb1 = stamp.GetOverContent(1);

                //ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("申辦編號：" + GetData(data["APP_ID"]), font12), 10, 820, 0);

                SetApplyId(cb1, font12, GetData(data["APP_ID"]));

                if (data["ACTION_TYPE"].Equals("1")) // 補發
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 332, 741, 0);
                }
                else // 換發
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 332, 759, 0);
                }

                if (((string)data["NAME"]).Length < 5)
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font16), 142, 702, 0);
                }
                else if (((string)data["NAME"]).Length < 7)
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font12), 142, 702, 0);
                }
                else
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font10), 142, 702, 0);
                }


                // ISSUE_DEPT
                //ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase((string)data["ISSUE_DEPT_DESC"], font12), 252, 678, 0);

                //  ((DateTime)data["ISSUE_DATE"]).Year - 1911
                if (!Convert.IsDBNull(data["ISSUE_DATE"]))
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Year - 1911).ToString(), font16), 392, 702, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Month).ToString("D2"), font16), 435, 702, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Day).ToString("D2"), font16), 476, 702, 0);
                }
                // LIC_CD_DESC
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_CD_DESC"]), font16), 100, 672, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_NUM"]), font16), 225, 672, 0);
                var replacePart = GetData(data["LIC_TYPE_DESC"]).Replace("科", "");
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(replacePart, font16), 308, 672, 0);

                // data["ACTION_RES_DESC"]
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ACTION_RES_DESC"]), font16), 84, 629, 0);
                if (!Convert.IsDBNull(data["OTHER_RES"])) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["OTHER_RES"]), font16u), 124, 629, 0);

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 230, 392, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["IDN"]), font16), 262, 366, 0);
                //ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font16), 231, 365, 0);

                ColumnText text1 = new ColumnText(cb1);
                if (Encoding.GetEncoding("big5").GetBytes(GetData(data["ADDR"])).Length > 40)
                {
                    text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font12), 231, 355, 550, 340, 18, Element.ALIGN_LEFT);
                    text1.SetLeading(0f, 1f);
                }
                else
                {
                    text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font16), 231, 352, 550, 340, 18, Element.ALIGN_LEFT);
                }
                text1.Go();

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font16), 236, 304, 0);

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 270, 242, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 344, 242, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 420, 242, 0);
                #endregion

                #region P2
                PdfContentByte cb2 = stamp.GetOverContent(2);

                SetApplyId(cb2, font12, GetData(data["APP_ID"]));

                if (((string)data["NAME"]).Length < 5)
                {
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font16), 140, 684, 0);
                }
                else if (((string)data["NAME"]).Length < 7)
                {
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font12), 140, 684, 0);
                }
                else
                {
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["NAME"]), font10), 140, 684, 0);
                }

                
                //ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase((string)data["ISSUE_DEPT_DESC"], font12), 232, 666, 0);
                if (!Convert.IsDBNull(data["ISSUE_DATE"]))
                {
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Year - 1911).ToString(), font16), 386, 684, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Month).ToString("D2"), font16), 431, 684, 0);
                    ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["ISSUE_DATE"]).Day).ToString("D2"), font16), 476, 684, 0);
                }
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_CD_DESC"]), font16), 110, 648, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_NUM"]), font16), 230, 648, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase(replacePart, font16), 316, 648, 0);

                ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["ACTION_RES_DESC"]), font16), 84, 612, 0);
                if (!Convert.IsDBNull(data["OTHER_RES"])) ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["OTHER_RES"]), font16u), 124, 612, 0);

                ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 238, 326, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["IDN"]), font16), 268, 294, 0);
                //ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font16), 232, 300, 0);

                ColumnText text2 = new ColumnText(cb2);
                if (Encoding.GetEncoding("big5").GetBytes(GetData(data["ADDR"])).Length > 40)
                {
                    text2.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font12), 232, 284, 550, 200, 18, Element.ALIGN_LEFT);
                    text2.SetLeading(0f, 1f);
                }
                else
                {
                    text2.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font16), 232, 281, 550, 200, 18, Element.ALIGN_LEFT);
                }
                text2.Go();

                ColumnText.ShowTextAligned(cb2, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font16), 232, 232, 0);

                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 270, 132, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 346, 132, 0);
                ColumnText.ShowTextAligned(cb2, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 420, 132, 0);
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