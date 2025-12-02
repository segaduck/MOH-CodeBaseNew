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
    public class Form001037Action : FormBaseAction
    {
        public Form001037Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form001037Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.SRV_ID, A.APP_ID, A.APP_TIME, A.IDN, A.NAME, A.ENAME, A.BIRTHDAY, A.ADDR, A.TEL,
                    B.LIC_NUM, B.ISSUE_DATE, B.COPIES, B.TOTAL, B.MAIL_REC, B.MAIL_ADDR,
                    B.REASON_1, B.REASON_2, B.REASON_3, B.REASON_4, B.REASON_5,
                    B.REASON_2_DESC, B.REASON_3_DESC, B.REASON_4_DESC, B.REASON_5_DESC,
                    B.ATTACH_1, B.ATTACH_2,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = (SELECT CODE_PCD FROM CODE_CD WHERE CODE_CD = B.LIC_CD AND CODE_KIND = 'F1_LICENSE_CD_1') AND CODE_KIND = 'F1_LICENSE_CD_1') AS LIC_TYPE_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_CD AND CODE_KIND = 'F1_LICENSE_CD_1') AS LIC_CD_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.MAIL_COUNTRY AND CODE_KIND = 'F1_COUNTRY_1') AS MAIL_COUNTRY_DESC
                FROM APPLY A, APPLY_001037 B
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
                PdfReader reader = new PdfReader(HttpContext.Current.Server.MapPath("~/Sample/001037醫事人員請領無懲戒紀錄證明申請書.pdf"));
                //PdfReader reader = new PdfReader(DataUtils.GetConfig("FOLDER_TEMPLATE") + data["SRV_ID"] + "\\醫事人員請領無懲戒紀錄證明申請書 110809.pdf");
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

                SetApplyId(cb1, font12, GetData(data["APP_ID"]));

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["NAME"]), font16), 240, 698, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ENAME"]), font16), 240, 674, 0);

                if (!Convert.IsDBNull(data["BIRTHDAY"]))
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(((DateTime)data["BIRTHDAY"]).Year.ToString(), font16), 306, 650, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(((DateTime)data["BIRTHDAY"]).Month.ToString("D2"), font16), 370, 650, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(((DateTime)data["BIRTHDAY"]).Day.ToString("D2"), font16), 434, 650, 0);
                }

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["LIC_TYPE_DESC"]), font16), 220, 626, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_CD_DESC"]), font16), 380, 602, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["LIC_NUM"]), font16), 460, 602, 0);

                if (!Convert.IsDBNull(data["ISSUE_DATE"]))
                {
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(((DateTime)data["ISSUE_DATE"]).Year.ToString(), font16), 278, 578, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(((DateTime)data["ISSUE_DATE"]).Month.ToString("D2"), font16), 342, 578, 0);
                    ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(((DateTime)data["ISSUE_DATE"]).Day.ToString("D2"), font16), 406, 578, 0);
                }

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["COPIES"]), font16), 216, 554, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase(GetData(data["TOTAL"]), font16), 426, 554, 0);

                ColumnText text1 = new ColumnText(cb1);
                if (Encoding.GetEncoding("big5").GetBytes(GetData(data["ADDR"])).Length > 40)
                {
                    text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font12), 204, 552, 550, 520, 18, Element.ALIGN_LEFT);
                    text1.SetLeading(0f, 1f);
                }
                else
                {
                    text1.SetSimpleColumn(new Phrase(GetData(data["ADDR"]), font16), 204, 550, 550, 520, 18, Element.ALIGN_LEFT);
                }
                text1.Go();
                //ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["ADDR"]), font16), 204, 530, 0);

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["TEL"]), font16), 204, 506, 0);

                ColumnText text2 = new ColumnText(cb1);
                if (Encoding.GetEncoding("big5").GetBytes(GetData(data["MAIL_ADDR"])).Length > 64)
                {
                    text2.SetSimpleColumn(new Phrase(GetData(data["MAIL_ADDR"]), font10), 110, 477, 550, 445, 18, Element.ALIGN_LEFT);
                    text2.SetLeading(0f, 1f);
                }
                else
                {
                    text2.SetSimpleColumn(new Phrase(GetData(data["MAIL_ADDR"]), font16), 110, 477, 550, 445, 18, Element.ALIGN_LEFT);
                }
                text2.Go();

                //ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["MAIL_ADDR"]), font16), 110, 458, 0);

                if (data["REASON_1"].Equals("1"))
                {
                    if (data["REASON_2"].Equals("3")) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 126, 371, 0); // 出國執業
                    if (data["REASON_2"].Equals("4")) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 206, 371, 0); // 出國進修

                    if(GetData(data["REASON_3"]).IndexOf("11") >= 0) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 214, 347, 0); // 美國、加拿大
                    if (GetData(data["REASON_3"]).IndexOf("12") >= 0) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 334, 347, 0); // 紐西蘭、澳洲
                    if (GetData(data["REASON_3"]).IndexOf("13") >= 0) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 454, 347, 0); // 歐洲地區

                    if (GetData(data["REASON_3"]).IndexOf("14") >= 0) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 230, 323, 0); // 大陸地區
                    if (GetData(data["REASON_3"]).IndexOf("15") >= 0)
                    {
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 318, 323, 0); // 其他國家
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["REASON_3_DESC"]), font16), 398, 323, 0); // 其他國家
                    }

                    if (data["REASON_2"].Equals("5"))
                    {
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 126, 299, 0); // 其他原因
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["REASON_2_DESC"]), font16), 224, 299, 0); // 其他原因
                    }
                }
                if (data["REASON_1"].Equals("2"))
                {
                    if (data["REASON_4"].Equals("6"))
                    {
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 126, 244, 0); // 機關要求
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["REASON_4_DESC"]), font16), 224, 244, 0); // 機關要求
                    }

                    if (data["REASON_5"].Equals("7"))
                    {
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 126, 220, 0); // 其他原因
                        ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase(GetData(data["REASON_5_DESC"]), font16), 224, 220, 0); // 其他原因
                    }
                }
                if (!String.IsNullOrEmpty(GetData(data["ATTACH_1"]))) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 106, 174, 0); // 中文證書影本
                if (!String.IsNullOrEmpty(GetData(data["ATTACH_2"]))) ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 106, 154, 0); // 護照影本
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_LEFT, new Phrase("■", font16), 106, 134, 0); // 證明書規費：每份 200 元整

                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Year - 1911).ToString(), font16), 283, 102, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Month).ToString("D2"), font16), 355, 102, 0);
                ColumnText.ShowTextAligned(cb1, Element.ALIGN_CENTER, new Phrase((((DateTime)data["APP_TIME"]).Day).ToString("D2"), font16), 427, 102, 0);

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