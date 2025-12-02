using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Globalization;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace ES.Utils
{
    public class PayUtils
    {
        private static readonly ILog logger = LogUtils.GetLogger();
        // 衛生福利部 企業識別碼
        private static readonly string identifier = "49465";
        // 超商代收代號
        private static readonly string storeCode = "69A";
        // 郵局劃撥帳戶
        private static readonly string postCode = "50013985";
        // 檢核規則
        private static readonly int[] plus = new int[] { 3, 7, 1 };

        /// <summary>
        /// 取得虛擬帳號
        /// </summary>
        /// <param name="no">繳款人識別碼</param>
        /// <param name="fee">費用</param>
        /// <returns></returns>
        public static string GetVirtualAccount(string no, int fee)
        {
            char[] c1 = (identifier + no).ToCharArray();
            char[] c2 = fee.ToString("D8").ToCharArray();

            int n1, n2 = 0, n3 = 0;

            for (int i = 0; i < c1.Count(); i++)
            {
                n1 = Int32.Parse(c1[i].ToString());
                n2 += n1 * plus[(i % 3)] % 10;

                //logger.Debug("n2: " + Int32.Parse(c1[i].ToString()) + " * " + plus[(i % 3)] +  " % 10 = " + (n1 * plus[(i % 3)] % 10) + " / " + n2);
            }
             
            for (int i = 0; i < c2.Count(); i++)
            {
                n1 = Int32.Parse(c2[i].ToString());
                //logger.Debug(c2[i] + " / " + plus[(i % 3)]);
                n3 += n1 * plus[(i % 3)] % 10;

                //logger.Debug("n3: " + Int32.Parse(c2[i].ToString()) + " * " + plus[(i % 3)] + " % 10 = " + (n1 * plus[(i % 3)] % 10) + " / " + n3);
            }

            //logger.Debug("n2: " + n2 + " / n3: " + n3);

            //logger.Debug("no: " + identifier + no + ((n2 + n3) % 10));

            int chk = 10 - (n2 + n3) % 10;

            return identifier + no + (chk % 10).ToString();
        }

        /// <summary>
        /// 取得超商條碼
        /// </summary>
        /// <param name="dt">代收期限</param>
        /// <param name="no">虛擬帳號</param>
        /// <param name="fee">應繳金額</param>
        /// <returns></returns>
        public static string[] GetStore(DateTime dt, string no, int fee)
        {
            string[] barcode = new string[] { "", "", "" };

            barcode[0] = GetStore1(dt);
            barcode[1] = GetStore2(no);
            barcode[2] = GetStore3(dt, fee);

            barcode[2] = barcode[2].Substring(0, 4) + GetStoreCheckCode(barcode) + barcode[2].Substring(4);

            return barcode;
        }

        /// <summary>
        /// 取得郵局條碼
        /// </summary>
        /// <param name="dt">代收期限</param>
        /// <param name="no">虛擬帳號</param>
        /// <param name="fee">應繳金額</param>
        /// <returns></returns>
        public static string[] GetPost(DateTime dt, string no, int fee)
        {
            string[] barcode = new string[] { "", "", "" };

            barcode[0] = postCode;              // 郵局 劃撥專戶帳號(8碼)
            barcode[1] = GetPost2(dt, no);      // 寄款人代號(24碼)
            barcode[2] = fee.ToString("D11");   // 繳款金額(11碼)

            barcode[1] += GetPostCheckNumber(barcode);

            return barcode;
        }

        /// <summary>
        /// 超商條碼第一段
        /// 代收期限yymmdd (6) + 69A(3)
        /// </summary>
        /// <param name="dt">代收期限</param>
        /// <returns></returns>
        private static string GetStore1(DateTime dt)
        {
            TaiwanCalendar tc = new TaiwanCalendar();
            string yy = (tc.GetYear(dt) % 100).ToString("D2");
            string mmdd = dt.ToString("MMdd");

            return yy + mmdd + storeCode;
        }

        /// <summary>
        /// 超商條碼第二段
        /// ATM/跨行匯款之虛擬帳號(16)
        /// </summary>
        /// <param name="no">虛擬帳號</param>
        /// <returns></returns>
        private static string GetStore2(string no)
        {
            return "00" + no;
        }

        /// <summary>
        /// 超商條碼第三段
        /// 應繳月日mmdd (4) + 檢碼(2) + 應繳金額(9)
        /// </summary>
        /// <param name="dt">代收期限</param>
        /// <param name="fee">應繳金額</param>
        /// <returns></returns>
        private static string GetStore3(DateTime dt, int fee)
        {
            string mmdd = dt.ToString("MMdd");

            return mmdd + fee.ToString("D9");
        }

        /// <summary>
        /// 取得超商檢核碼
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        private static string GetStoreCheckCode(string[] barcode)
        {
            string checkCode = "";
            int n = 0;

            // 第一碼
            for (int i = 0; i < barcode.Count(); i++)
            {
                for (int j = 0; j < barcode[i].Length; j = j + 2)
                {
                    n += GetStoreNumber(barcode[i], j);
                }
            }

            n = n % 11;

            if (n == 0)
            {
                checkCode += "A";
            }
            else if (n == 10)
            {
                checkCode += "B";
            }
            else
            {
                checkCode += n.ToString();
            }

            // 第二碼
            n = 0;
            for (int i = 0; i < barcode.Count(); i++)
            {
                for (int j = 1; j < barcode[i].Length; j = j + 2)
                {
                    n += GetStoreNumber(barcode[i], j);
                }
            }

            n = n % 11;

            if (n == 0)
            {
                checkCode += "X";
            }
            else if (n == 10)
            {
                checkCode += "Y";
            }
            else
            {
                checkCode += n.ToString();
            }

            return checkCode;
        }

        /// <summary>
        /// 取得條碼數值
        /// </summary>
        /// <param name="code">條碼</param>
        /// <param name="idx">位置</param>
        /// <returns></returns>
        private static int GetStoreNumber(string code, int idx)
        {
            string s = code.Substring(idx, 1);

            switch (s)
            {
                case "A":
                case "J":
                    return 1;
                case "B":
                case "K":
                case "S":
                    return 2;
                case "C":
                case "L":
                case "T":
                    return 3;
                case "D":
                case "M":
                case "U":
                    return 4;
                case "E":
                case "N":
                case "V":
                    return 5;
                case "F":
                case "O":
                case "W":
                    return 6;
                case "G":
                case "P":
                case "X":
                    return 7;
                case "H":
                case "Q":
                case "Y":
                    return 8;
                case "I":
                case "R":
                case "Z":
                    return 9;
                default:
                    return Int32.Parse(s);
            }
        }

        /// <summary>
        /// 郵局條碼第二段
        /// 繳款截止日判斷碼(7)+金額檢核判斷碼(1)+ATM虛擬帳號(15)+檢查碼(1)
        /// </summary>
        /// <param name="dt">繳款截止日</param>
        /// <param name="no">ATM虛擬帳號</param>
        /// <returns></returns>
        private static string GetPost2(DateTime dt, string no)
        {
            TaiwanCalendar tc = new TaiwanCalendar();
            string yyy = tc.GetYear(dt).ToString("D3");
            string mmdd = dt.ToString("MMdd");

            return yyy + mmdd + "10" + no;
        }

        /// <summary>
        /// 取得郵局檢查碼
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        private static string GetPostCheckNumber(string[] barcode)
        {
            int[] chk = new int[3];
            string no = barcode[0] + barcode[1];
            char[] c = no.ToCharArray();
            int n = 0;

            // A
            for (int i = 0; i < c.Count(); i++)
            {
                n += (Int32.Parse(c[i].ToString()) * plus[(i % 3)]) % 10;
                //logger.Debug(i + ": " + ((Int32.Parse(c[i].ToString()) * plus[(i % 3)]) % 10));
            }

            chk[0] = (n % 10);
            //logger.Debug("chk[0]: " + chk[0]);

            // B
            c = barcode[2].Substring(3).ToCharArray();
            n = 0;

            for (int i = 0; i < c.Count(); i++)
            {
                n += (Int32.Parse(c[i].ToString()) * (c.Count() - i)) % 10;
            }
            chk[1] = (n % 10);
            //logger.Debug("chk[1]: " + chk[1]);

            // C
            chk[2] = (chk[0] + chk[1]) % 10;

            return ((10 - chk[2])%10).ToString();
        }

        /// <summary>
        /// 產生 Code39 條碼
        /// </summary>
        /// <param name="strSource"></param>
        /// <returns></returns>
        public static Bitmap GetCode39(string strSource)
        {
            int x = 5; //左邊界
            int y = 0; //上邊界
            int WidLength = 2; //粗BarCode長度
            int NarrowLength = 1; //細BarCode長度
            int BarCodeHeight = 30; //BarCode高度
            int intSourceLength = strSource.Length;
            string strEncode = "010010100"; //編碼字串 初值為 起始符號 *

            string AlphaBet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%*"; //Code39的字母

            string[] Code39 = //Code39的各字母對應碼
            {
                 /**//* 0 */ "000110100",  
                 /**//* 1 */ "100100001",  
                 /**//* 2 */ "001100001",  
                 /**//* 3 */ "101100000",
                 /**//* 4 */ "000110001",  
                 /**//* 5 */ "100110000",  
                 /**//* 6 */ "001110000",  
                 /**//* 7 */ "000100101",
                 /**//* 8 */ "100100100",  
                 /**//* 9 */ "001100100",  
                 /**//* A */ "100001001",  
                 /**//* B */ "001001001",
                 /**//* C */ "101001000",  
                 /**//* D */ "000011001",  
                 /**//* E */ "100011000",  
                 /**//* F */ "001011000",
                 /**//* G */ "000001101",  
                 /**//* H */ "100001100",  
                 /**//* I */ "001001100",  
                 /**//* J */ "000011100",
                 /**//* K */ "100000011",  
                 /**//* L */ "001000011",  
                 /**//* M */ "101000010",  
                 /**//* N */ "000010011",
                 /**//* O */ "100010010",  
                 /**//* P */ "001010010",  
                 /**//* Q */ "000000111",  
                 /**//* R */ "100000110",
                 /**//* S */ "001000110",  
                 /**//* T */ "000010110",  
                 /**//* U */ "110000001",  
                 /**//* V */ "011000001",
                 /**//* W */ "111000000",  
                 /**//* X */ "010010001",  
                 /**//* Y */ "110010000",  
                 /**//* Z */ "011010000",
                 /**//* - */ "010000101",  
                 /**//* . */ "110000100",  
                 /**//*' '*/ "011000100",
                 /**//* $ */ "010101000",
                 /**//* / */ "010100010",  
                 /**//* + */ "010001010",  
                 /**//* % */ "000101010",  
                 /**//* * */ "010010100"  
            };
            strSource = strSource.ToUpper();
            //實作圖片
            Bitmap objBitmap = new Bitmap(
              ((WidLength * 3 + NarrowLength * 7) * (intSourceLength + 2)) + (x * 2),
              BarCodeHeight + (y * 2));
            Graphics objGraphics = Graphics.FromImage(objBitmap); //宣告GDI+繪圖介面
            //填上底色
            objGraphics.FillRectangle(Brushes.White, 0, 0, objBitmap.Width, objBitmap.Height);

            for (int i = 0; i < intSourceLength; i++)
            {
                //檢查是否有非法字元
                if (AlphaBet.IndexOf(strSource[i]) == -1 || strSource[i] == '*')
                {
                    objGraphics.DrawString("含有非法字元",
                      SystemFonts.DefaultFont, Brushes.Red, x, y);
                    return objBitmap;
                }
                //查表編碼
                strEncode = string.Format("{0}0{1}", strEncode,
                 Code39[AlphaBet.IndexOf(strSource[i])]);
            }

            strEncode = string.Format("{0}0010010100", strEncode); //補上結束符號 *

            int intEncodeLength = strEncode.Length; //編碼後長度
            int intBarWidth;

            for (int i = 0; i < intEncodeLength; i++) //依碼畫出Code39 BarCode
            {
                intBarWidth = strEncode[i] == '1' ? WidLength : NarrowLength;
                objGraphics.FillRectangle(i % 2 == 0 ? Brushes.Black : Brushes.White,
                 x, y, intBarWidth, BarCodeHeight);
                x += intBarWidth;
            }
            return objBitmap;
        }

        public static byte[] GetCode39Image(string code)
        {
            Bitmap bmp = PayUtils.GetCode39(code);
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        public static byte[] GetPDF(Dictionary<string, string> dict)
        {
            MemoryStream ms = new MemoryStream();

            try
            {
                PdfReader reader = new PdfReader(DataUtils.GetConfig("FOLDER_TEMPLATE") + "Pay\\衛生福利部繳款單.pdf");
                PdfStamper stamp = new PdfStamper(reader, ms);

                BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font12 = new iTextSharp.text.Font(bf, 12);
                iTextSharp.text.Font font14 = new iTextSharp.text.Font(bf, 14);

                PdfContentByte cb = stamp.GetOverContent(1);

                // 第一聯
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["PayDeadline"], font14), 480, 754, 0); // 繳款期限
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["Name"], font14), 132, 734, 0); // 繳款人姓名
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["VirtualAccount"], font14), 132, 712, 0); // 帳單號碼
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["ServiceName"], font14), 132, 692, 0); // 繳費類別
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["ApplyId"], font14), 132, 672, 0); // 申請編號
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("NT$" + dict["Fee"], font14), 132, 650, 0); // 應繳金額

                // 第二聯
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["PayDeadline"], font14), 480, 408, 0); // 繳款期限
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["Fee"], font14), 420, 387, 0); // 應繳總金額

                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["VirtualAccount"], font14), 130, 326, 0); // 輸入帳號
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["Fee"], font14), 130, 306, 0); // 輸入金額

                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["Post1"], font14), 130, 168, 0); // 收款專戶
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["Post2"], font14), 60, 110, 0); // 帳單編號
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(dict["Post3"], font14), 60, 70, 0); // 應繳金額

                // 條碼
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("*" + dict["Store1"] + "*", font12), 364, 328, 0); // 條碼一
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("*" + dict["Store2"] + "*", font12), 364, 268, 0); // 條碼二
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("*" + dict["Store3"] + "*", font12), 364, 208, 0); // 條碼三

                iTextSharp.text.Image store1 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Store1"]));
                iTextSharp.text.Image store2 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Store2"]));
                iTextSharp.text.Image store3 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Store3"]));

                store1.ScalePercent(80f);
                store2.ScalePercent(80f);
                store3.ScalePercent(80f);

                store1.SetAbsolutePosition(360, 340);
                store2.SetAbsolutePosition(360, 280);
                store3.SetAbsolutePosition(360, 220);

                cb.AddImage(store1);
                cb.AddImage(store2);
                cb.AddImage(store3);

                iTextSharp.text.Image post1 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Post1"]));
                iTextSharp.text.Image post2 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Post2"]));
                iTextSharp.text.Image post3 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Post3"]));

                post1.ScalePercent(80f);
                post2.ScalePercent(80f);
                post3.ScalePercent(80f);

                post1.SetAbsolutePosition(250, 150);
                post2.SetAbsolutePosition(250, 110);
                post3.SetAbsolutePosition(250, 70);

                cb.AddImage(post1);
                cb.AddImage(post2);
                cb.AddImage(post3);

                stamp.Close();
                reader.Close();
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }

            return ms.ToArray();
        }

        public static MemoryStream GetPDF2(Dictionary<string, string> dict)
        {
            Document doc = new Document(PageSize.A4);
            MemoryStream ms = new MemoryStream();

            try
            {
                // 寫實例
                PdfWriter pw = PdfWriter.GetInstance(doc, ms);
                pw.CloseStream = false;
                doc.Open();

                BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

                iTextSharp.text.Font font12 = new iTextSharp.text.Font(bf, 12);
                iTextSharp.text.Font font10 = new iTextSharp.text.Font(bf, 10);
                iTextSharp.text.Font font7 = new iTextSharp.text.Font(bf, 7);
                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bf, 12);

                PdfPTable title = new PdfPTable(1);
                title.DefaultCell.BorderWidth = 0;
                title.DefaultCell.BackgroundColor = new BaseColor(210, 210, 210);
                title.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                title.WidthPercentage = 19.5f;
                title.AddCell(new Phrase("衛生福利部繳款單", fontTitle));
                doc.Add(title);
                doc.Add(new Paragraph(" ", font10));

                doc.Add(GetPayTable1(dict));
                doc.Add(new Paragraph("---------------------------------------------------------------------------------------", font12));
                doc.Add(new Paragraph(" ", font10));
                doc.Add(GetPayTable2(dict));

                doc.Close();

                //logger.Debug("TTTTTTTTTEST");
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }

            //logger.Debug("ms: " + ms.Length);

            return ms;
        }

        private static PdfPTable GetPayTable1(Dictionary<string, string> dict)
        {
            BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            iTextSharp.text.Font font12 = new iTextSharp.text.Font(bf, 12);
            iTextSharp.text.Font font10 = new iTextSharp.text.Font(bf, 10);
            iTextSharp.text.Font font7 = new iTextSharp.text.Font(bf, 7);
            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bf, 12);

            PdfPTable table = new PdfPTable(3);
            table.SetWidths(new float[] { 1.5f, 7, 1.5f });
            table.WidthPercentage = 95;

            PdfPCell[] rc0 = new PdfPCell[3];
            rc0[0] = new PdfPCell(new Phrase("繳費期限：" + dict["PayDeadline"], font7)) { BorderWidth = 0 };
            rc0[0].Colspan = 3;
            rc0[0].HorizontalAlignment = PdfPCell.ALIGN_RIGHT;

            PdfPCell[] rc1 = new PdfPCell[3];
            rc1[0] = new PdfPCell(new Phrase("繳款人姓名", font10)) { BorderWidth = 1 };
            rc1[1] = new PdfPCell(new Phrase(dict["Name"], font10)) { BorderWidth = 1 };
            rc1[2] = new PdfPCell(new Phrase("收　　訖　　章", font7)) { BorderWidth = 1 };
            rc1[2].Rowspan = 5;
            rc1[2].VerticalAlignment = PdfPCell.ALIGN_BOTTOM;
            rc1[2].HorizontalAlignment = PdfPCell.ALIGN_CENTER;

            PdfPCell[] rc2 = new PdfPCell[3];
            rc2[0] = new PdfPCell(new Phrase("帳單號碼", font10)) { BorderWidth = 1 };
            rc2[1] = new PdfPCell(new Phrase(dict["VirtualAccount"], font10)) { BorderWidth = 1 };

            PdfPCell[] rc3 = new PdfPCell[3];
            rc3[0] = new PdfPCell(new Phrase("繳費類別", font10)) { BorderWidth = 1 };
            rc3[1] = new PdfPCell(new Phrase(dict["ServiceName"], font10)) { BorderWidth = 1 };

            PdfPCell[] rc4 = new PdfPCell[3];
            rc4[0] = new PdfPCell(new Phrase("申請編號", font10)) { BorderWidth = 1 };
            rc4[1] = new PdfPCell(new Phrase(dict["ApplyId"], font10)) { BorderWidth = 1 };

            PdfPCell[] rc5 = new PdfPCell[3];
            rc5[0] = new PdfPCell(new Phrase("應繳金額", font10)) { BorderWidth = 1 };
            rc5[1] = new PdfPCell(new Phrase("NT$" + dict["Fee"], font10)) { BorderWidth = 1 };

            PdfPRow row0 = new PdfPRow(rc0);
            PdfPRow row1 = new PdfPRow(rc1);
            PdfPRow row2 = new PdfPRow(rc2);
            PdfPRow row3 = new PdfPRow(rc3);
            PdfPRow row4 = new PdfPRow(rc4);
            PdfPRow row5 = new PdfPRow(rc5);

            table.Rows.Add(row0);
            table.Rows.Add(row1);
            table.Rows.Add(row2);
            table.Rows.Add(row3);
            table.Rows.Add(row4);
            table.Rows.Add(row5);

            return table;
        }

        private static PdfPTable GetPayTable2(Dictionary<string, string> dict)
        {
            BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            iTextSharp.text.Font font12 = new iTextSharp.text.Font(bf, 12);
            iTextSharp.text.Font font10 = new iTextSharp.text.Font(bf, 10);
            iTextSharp.text.Font font7 = new iTextSharp.text.Font(bf, 7);
            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bf, 12);

            PdfPTable table = new PdfPTable(5);
            table.SetWidths(new float[] { 0.5f, 4f, 2, 0.5f, 4.7f });
            table.WidthPercentage = 95;

            PdfPCell[] rc1 = new PdfPCell[5];
            rc1[0] = new PdfPCell(new Phrase("全行代收專戶", font10)) { BorderWidth = 1 };
            rc1[0].Rowspan = 2;
            rc1[0].VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            rc1[0].HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            rc1[1] = new PdfPCell(new Phrase("戶名：衛生福利部", font10)) { FixedHeight = 24, BorderWidth = 1 };
            rc1[1].Colspan = 3;
            rc1[1].VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            rc1[4] = new PdfPCell(new Phrase("應繳總金額：" + dict["Fee"], font10)) { BorderWidth = 1 };
            rc1[4].VerticalAlignment = PdfPCell.ALIGN_MIDDLE;

            PdfPCell[] rc2 = new PdfPCell[5];
            rc2[1] = new PdfPCell() { BorderWidth = 1 };
            rc2[1].AddElement(new Phrase("ATM轉帳/跨行匯款：", font10));
            rc2[1].AddElement(new Phrase("中國信託商業銀行：822", font10));
            rc2[1].AddElement(new Phrase("輸入帳號：" + dict["VirtualAccount"], font10));
            rc2[1].AddElement(new Phrase("輸入金額：" + dict["Fee"], font10));
            rc2[1].AddElement(new Phrase(" ", font10));
            rc2[1].AddElement(new Phrase("郵局、7-11、全家、萊爾富、OK超商、農漁會信用部", font7));
            rc2[2] = new PdfPCell(new Phrase("收　　訖　　章", font7)) { BorderWidth = 1 };
            rc2[2].VerticalAlignment = PdfPCell.ALIGN_BOTTOM;
            rc2[2].HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            rc2[3] = new PdfPCell(new Phrase("便利商店及農漁會信用部", font10)) { BorderWidth = 1 };
            rc2[3].Rowspan = 2;
            rc2[3].VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            rc2[3].HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            rc2[4] = new PdfPCell() { BorderWidth = 1 };
            rc2[4].Rowspan = 2;

            iTextSharp.text.Image store1 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Store1"]));
            iTextSharp.text.Image store2 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Store2"]));
            iTextSharp.text.Image store3 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Store3"]));

            store1.ScalePercent(80f);
            store2.ScalePercent(80f);
            store3.ScalePercent(80f);

            rc2[4].AddElement(store1);
            rc2[4].AddElement(new Phrase("*" + dict["Store1"] + "*", font7));
            rc2[4].AddElement(new Phrase(" ", font7));
            rc2[4].AddElement(store2);
            rc2[4].AddElement(new Phrase("*" + dict["Store2"] + "*", font7));
            rc2[4].AddElement(new Phrase(" ", font7));
            rc2[4].AddElement(store3);
            rc2[4].AddElement(new Phrase("*" + dict["Store3"] + "*", font7));

            PdfPCell[] rc3 = new PdfPCell[5];
            rc3[0] = new PdfPCell(new Phrase("認證欄", font10)) { FixedHeight = 52, BorderWidth = 1 };
            rc3[0].VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            rc3[0].HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            rc3[1] = new PdfPCell() { BorderWidth = 1 };
            rc3[1].Colspan = 2;

            PdfPCell[] rc4 = new PdfPCell[5];
            rc4[0] = new PdfPCell(new Phrase("郵政專用", font10)) { BorderWidth = 1 };
            rc4[0].VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            rc4[0].HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            rc4[1] = new PdfPCell() { BorderWidth = 1 };
            rc4[1].Colspan = 4;

            iTextSharp.text.Image post1 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Post1"]));
            iTextSharp.text.Image post2 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Post2"]));
            iTextSharp.text.Image post3 = iTextSharp.text.Image.GetInstance(GetCode39Image(dict["Post3"]));

            post1.ScalePercent(80f);
            post2.ScalePercent(80f);
            post3.ScalePercent(80f);

            PdfPTable table2 = new PdfPTable(2);
            table2.WidthPercentage = 100;
            table2.DefaultCell.BorderWidth = 0;
            table2.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            table2.SetWidths(new float[] { 3, 7 });
            table2.AddCell(new Phrase("收款專戶：\n" + dict["Post1"], font10));
            table2.AddCell(new PdfPCell(post1) { BorderWidth = 0 });
            table2.AddCell(new PdfPCell(new Phrase("", font7)) { Colspan = 2, BorderWidth = 0, FixedHeight = 3 });
            table2.AddCell(new Phrase("帳單編號：\n" + dict["Post2"], font10));
            table2.AddCell(new PdfPCell(post2) { BorderWidth = 0 });
            table2.AddCell(new PdfPCell(new Phrase("", font7)) { Colspan = 2, BorderWidth = 0, FixedHeight = 3 });
            table2.AddCell(new Phrase("應繳金額：\n" + dict["Post3"], font10));
            table2.AddCell(new PdfPCell(post3) { BorderWidth = 0 });
            table2.AddCell(new PdfPCell(new Phrase("", font7)) { Colspan = 2, BorderWidth = 0, FixedHeight = 3 });

            rc4[1].AddElement(table2);

            PdfPRow row1 = new PdfPRow(rc1);
            PdfPRow row2 = new PdfPRow(rc2);
            PdfPRow row3 = new PdfPRow(rc3);
            PdfPRow row4 = new PdfPRow(rc4);

            table.Rows.Add(row1);
            table.Rows.Add(row2);
            table.Rows.Add(row3);
            table.Rows.Add(row4);

            return table;
        }
    }
}