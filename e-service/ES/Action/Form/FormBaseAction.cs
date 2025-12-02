using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Data.SqlClient;
using log4net;
using ES.Utils;

namespace ES.Action.Form
{
    public class FormBaseAction : BaseAction
    {
        protected PdfPCell GetCellHeader(string body, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(body, font)) { HorizontalAlignment = PdfPCell.ALIGN_RIGHT, BorderWidth = 0.5f, PaddingBottom = 7 };
        }

        protected PdfPCell GetCellFooterR(string body, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(body, font)) { BorderWidth = 0, HorizontalAlignment = PdfPCell.ALIGN_RIGHT, VerticalAlignment = PdfPCell.ALIGN_BOTTOM };
        }

        protected PdfPCell GetCellFooter(string body, iTextSharp.text.Font font)
        {

            PdfPTable tb = new PdfPTable(1);
            tb.WidthPercentage = 100;
            string[] bodys = body.Split('\n');

            for (int i = 0; i < bodys.Length; i++)
            {
                tb.AddCell(GetCellL(bodys[i], font));
            }

            return new PdfPCell(tb) { HorizontalAlignment = PdfPCell.ALIGN_LEFT, BorderWidth = 0.5f, PaddingBottom = 10 };
        }

        protected PdfPCell GetCellC(string body, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(body, font)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfPCell.ALIGN_MIDDLE, BorderWidth = 0, PaddingBottom = 5 };
        }

        protected PdfPCell GetCellC(string[] body, iTextSharp.text.Font[] font)
        {
            Phrase p = new Phrase();
            for (int i = 0; i < body.Length; i++)
            {
                p.Add(new Chunk(body[i], font[i]));
            }
            return new PdfPCell(p) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, VerticalAlignment = PdfPCell.ALIGN_MIDDLE, BorderWidth = 0, PaddingBottom = 5 };
        }

        protected PdfPCell GetCellR(string body, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(body, font)) { HorizontalAlignment = PdfPCell.ALIGN_RIGHT, VerticalAlignment = PdfPCell.ALIGN_MIDDLE, BorderWidth = 0, PaddingBottom = 5 };
        }

        protected PdfPCell GetCellL(string body, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(body, font)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT, VerticalAlignment = PdfPCell.ALIGN_MIDDLE, BorderWidth = 0, PaddingBottom = 5 };
        }

        protected PdfPCell GetCellL(string[] body, iTextSharp.text.Font font)
        {
            Phrase p = new Phrase();
            for (int i = 0; i < body.Length; i++)
            {
                p.Add(new Chunk(body[i], font));
            }
            return new PdfPCell(p) { HorizontalAlignment = PdfPCell.ALIGN_LEFT, VerticalAlignment = PdfPCell.ALIGN_MIDDLE, BorderWidth = 0, PaddingBottom = 5 };
        }

        protected PdfPCell GetCellL(string[] body, iTextSharp.text.Font[] font)
        {
            Phrase p = new Phrase();
            for (int i = 0; i < body.Length; i++)
            {
                p.Add(new Chunk(body[i], font[i]));
            }
            return new PdfPCell(p) { HorizontalAlignment = PdfPCell.ALIGN_LEFT, VerticalAlignment = PdfPCell.ALIGN_MIDDLE, BorderWidth = 0, PaddingBottom = 5 };
        }

        protected PdfPCell GetCellCB(string body, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(body, font)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, BorderWidth = 1, PaddingBottom = 10 };
        }

        protected PdfPCell GetCellLB(string body, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(body, font)) { HorizontalAlignment = PdfPCell.ALIGN_LEFT, BorderWidth = 1, PaddingBottom = 10 };
        }

        protected string GetData(object o)
        {
            return (Convert.IsDBNull(o)) ? "" : o.ToString();
        }

        protected void SetApplyId(PdfContentByte cb, iTextSharp.text.Font font, string applyId)
        {
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("申辦編號：" + applyId, font), 10, 820, 0);
        }
    }
}