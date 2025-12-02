using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using System.Drawing;
using System.Drawing.Imaging;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.IO;
using System.Net;
using System.Data;
using System.Web.UI.WebControls;
using ES.Areas.Admin.Models;
using System.Text;

namespace WebUI.CustomClass
{
    /// <summary>
    /// reportLabs 的摘要描述
    /// </summary>
    public class ReportUtils
    {
        private static String[] name = { "SRL_NO", "CLIENT_NO", "BILLING_CYCLE", "PAYMENT_DATE", "COLLECTION_AMOUNT", "FCC", "POST_ANOUNT", "POST_DATE", "CHANNEL" };
        public ReportUtils()
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //
        }

        public static List<Map> readEXCEL(MemoryStream ms)
        {
            ms.Position = 0; // 增加这句
            var workbook = new HSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(0);
            List<Map> li = new List<Map>();
            int idx;
            string val;
            for (int i = 13; i < sheet.LastRowNum - 1; i++)
            {
                Map map = new Map();
                int j = 0;
                foreach (ICell ic in sheet.GetRow(i).Cells)
                {
                    if (j > name.Length - 1)
                    {
                        break;
                    }
                    object obj_temp = new object();
                    if (ic.CellType == CellType.String)
                    {
                        if (name[j].Equals("CLIENT_NO"))
                        {
                            val = ic.StringCellValue;
                            idx = val.IndexOf("49465");
                            if (idx < 0) idx = 0;
                            if (val.Length - idx >= 14)
                            {
                                obj_temp = val.Substring(idx, 14);
                            }
                            else
                            {
                                obj_temp = ic.StringCellValue;
                            }
                        }
                        else
                        {
                            obj_temp = ic.StringCellValue;
                        }
                    }
                    else if (ic.CellType == CellType.Numeric)
                    {
                        obj_temp = ic.NumericCellValue;
                    }
                    else
                    {
                        ic.SetCellType(CellType.String);
                        obj_temp = ic.StringCellValue;
                    }
                    map.Add(name[j], obj_temp);
                    j++;
                }
                li.Add(map);
            }
            return li;
        }

        public static MemoryStream RenderDataTableToExcel(DataTable SourceTable)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();//建立文件檔
            MemoryStream ms = new MemoryStream();//建立記憶體空間
            ISheet sheet = workbook.CreateSheet("sheet1");//開啟EXCEL分頁
            int rowIndex = BuildSheet(workbook, sheet, SourceTable);//建立TABLE資料

            #region 插入圖片  空的
            //String server = System.Configuration.ConfigurationSettings.AppSettings["ReportService"];
            //String defualtConfig = "&rs:Command=Render&rs:Format=IMAGE&rc:PageWidth=21cm&rc:PageHeight=12cm&rc:MarginTop=0cm&rc:MarginBottom=0cm&rc:MarginLeft=0cm&rc:MarginRight=0cm&rc:OutputFormat=JPEG";
            //HSSFPatriarch patriarch = (HSSFPatriarch)sheet.CreateDrawingPatriarch();
            //String[] urls = new String[3];
            //urls[0] = server + "D_ALL_PC_KGC_Chart01" + defualtConfig + ReportParameter;
            //urls[1] = server + "D_ALL_PC_KGC_Chart02" + defualtConfig + ReportParameter;
            //urls[2] = server + "D_ALL_PC_KGC_Chart03" + defualtConfig + ReportParameter;
            //reportLabs.ExcelAddImg(urls, rowIndex, patriarch, workbook);
            #endregion

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }

        public static MemoryStream RenderDataTableToExcel(DataTable SourceTable, string Title)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();//建立文件檔
            MemoryStream ms = new MemoryStream();//建立記憶體空間
            ISheet sheet = workbook.CreateSheet("sheet1");//開啟EXCEL分頁
            int rowIndex = BuildSheet1(workbook, sheet, SourceTable, Title);//建立TABLE資料

            #region 插入圖片  空的
            //String server = System.Configuration.ConfigurationSettings.AppSettings["ReportService"];
            //String defualtConfig = "&rs:Command=Render&rs:Format=IMAGE&rc:PageWidth=21cm&rc:PageHeight=12cm&rc:MarginTop=0cm&rc:MarginBottom=0cm&rc:MarginLeft=0cm&rc:MarginRight=0cm&rc:OutputFormat=JPEG";
            //HSSFPatriarch patriarch = (HSSFPatriarch)sheet.CreateDrawingPatriarch();
            //String[] urls = new String[3];
            //urls[0] = server + "D_ALL_PC_KGC_Chart01" + defualtConfig + ReportParameter;
            //urls[1] = server + "D_ALL_PC_KGC_Chart02" + defualtConfig + ReportParameter;
            //urls[2] = server + "D_ALL_PC_KGC_Chart03" + defualtConfig + ReportParameter;
            //reportLabs.ExcelAddImg(urls, rowIndex, patriarch, workbook);
            #endregion

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }

        /// <summary> 2019擴充功能 匯出ODS  RefineWorkSheet
        /// 
        /// </summary>
        /// <param name="SourceTable"></param>
        /// <returns></returns>
        public static MemoryStream RenderDataTableToODS(DataTable SourceTable)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();//建立文件檔
            MemoryStream ms = new MemoryStream();//建立記憶體空間
            ISheet sheet = workbook.CreateSheet("sheet1");//開啟EXCEL分頁
            int rowIndex = RefineWorkSheet(workbook, sheet, SourceTable);//建立TABLE資料

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }

        public static void ExcelAddImg(String[] urls, int rowIndex, HSSFPatriarch patriarch, HSSFWorkbook workbook)
        {
            WebClient wc = new WebClient();
            int pictureIndex = 0;
            foreach (String url in urls)
            {
                MemoryStream memoryStream = new MemoryStream();
                var image = System.Drawing.Image.FromStream(wc.OpenRead(new Uri(url)));
                var thumbStream = image.GetThumbnailImage(image.Width, image.Height, () => false, IntPtr.Zero);
                thumbStream.Save(memoryStream, ImageFormat.Jpeg);
                pictureIndex = workbook.AddPicture(memoryStream.ToArray(), PictureType.JPEG);
                HSSFClientAnchor anchor = new HSSFClientAnchor(0, 0, 0, 0, 0, rowIndex + 2, 0, 0);
                anchor.AnchorType = AnchorType.MoveDontResize;
                HSSFPicture picture = (HSSFPicture)patriarch.CreatePicture(anchor, pictureIndex);
                picture.Resize();
                //picture.LineStyle = .LINEWIDTH_DEFAULT;
                memoryStream.Close();
                memoryStream.Dispose();
                rowIndex += 30;
            }
        }

        public static DataTable GVtoTable(GridView Chartarr)
        {
            DataTable dt = new DataTable();
            String b = "";
            List<String> ls = new List<string>();
            for (int i = 1; i <= Chartarr.HeaderRow.Cells.Count; i++)
            {
                b = (Chartarr.HeaderRow.FindControl("Label" + i) as Label).Text;
                dt.Columns.Add(b, System.Type.GetType("System.String"));
            }

            foreach (GridViewRow row in Chartarr.Rows)
            {
                ls.Clear();
                int i = 1;
                foreach (TableCell tc in row.Cells)
                {
                    ls.Add((tc.FindControl("Label" + i) as Label).Text);
                    i++;
                }
                dt.Rows.Add(ls.ToArray());
            }
            return dt;
        }

        /// <summary> 2019 擴充ODS 版面校正
        /// 
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheet"></param>
        /// <param name="SourceTable"></param>
        /// <returns></returns>
        public static int RefineWorkSheet(HSSFWorkbook workbook, ISheet sheet, DataTable SourceTable)
        {
            //版面配置
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, SourceTable.Columns.Count - 1));//合併欄位

            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, SourceTable.Columns.Count - 1));//合併欄位

            #region CS格式
            //抬頭文字格式
            HSSFFont font = null;
            font = (HSSFFont)workbook.CreateFont();
            font.FontHeightInPoints = 16;
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            font.FontName = "標楷體";

            //資料抬頭格式
            ICellStyle style_title = workbook.CreateCellStyle();
            style_title.Alignment = HorizontalAlignment.Center;
            style_title.VerticalAlignment = VerticalAlignment.Center;
            style_title.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkBlue.Index2;
            style_title.FillPattern = FillPattern.SolidForeground;
            style_title.SetFont(font);
            //內容文字格式
            HSSFFont font_data = null;
            font_data = (HSSFFont)workbook.CreateFont();
            font_data.FontHeightInPoints = 16;
            //資料內容格式(淡藍)
            ICellStyle style1 = workbook.CreateCellStyle();
            style1.Alignment = HorizontalAlignment.Left;
            style1.VerticalAlignment = VerticalAlignment.Center;
            style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.CornflowerBlue.Index;
            style1.FillPattern = FillPattern.SolidForeground;
            style1.SetFont(font_data);
            //資料內容格式(白色)
            ICellStyle style2 = workbook.CreateCellStyle();
            style2.Alignment = HorizontalAlignment.Left;
            style2.VerticalAlignment = VerticalAlignment.Center;
            style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.SetFont(font_data);
            //列印日期
            ICellStyle style3 = workbook.CreateCellStyle();
            style3.Alignment = HorizontalAlignment.Right;
            style3.VerticalAlignment = VerticalAlignment.Center;
            style3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style3.FillPattern = FillPattern.SolidForeground;
            style3.SetFont(font_data);
            //表頭 2019擴充  文件抬頭
            ICellStyle cs = workbook.CreateCellStyle();
            cs.VerticalAlignment = VerticalAlignment.Center;
            cs.Alignment = HorizontalAlignment.Center;
            cs.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightOrange.Index;
            cs.SetFont(font_data);
            #endregion


            #region DataTable資料塞入
            //文件抬頭
            HSSFRow headerRow0 = (HSSFRow)sheet.CreateRow(0);//創造第0排
            ICell headice0 = headerRow0.CreateCell(0);//創造第0格位
            headice0.SetCellValue("衛生福利部人民申請線上申辦服務系統使用者帳號清單");
            headice0.CellStyle = cs;
            sheet.GetRow(0).HeightInPoints = 25;


            //sheet.CreateRow(0).CreateCell(0).SetCellValue("衛生福利部人民申請線上申辦服務系統使用者帳號清單");


            HSSFRow headerRow1 = (HSSFRow)sheet.CreateRow(1);//創造第1排
            ICell headice1 = headerRow1.CreateCell(0);//創造第0格位
            headice1.SetCellValue("匯出日期:" + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
            sheet.GetRow(1).HeightInPoints = 25;
            headice1.CellStyle = style3;

            //資料抬頭
            HSSFRow titleRow = (HSSFRow)sheet.CreateRow(2);
            int colIndex = 0;
            foreach (DataColumn column in SourceTable.Columns)
            {
                ICell ice = titleRow.CreateCell(column.Ordinal);
                ice.SetCellValue(column.ColumnName);
                ice.CellStyle = style_title;
                sheet.SetColumnWidth(colIndex, (column.ColumnName.Length + 2) * 1024);
                colIndex++;
            }

            //資料內容
            int rowIndex = 3;
            foreach (DataRow row in SourceTable.Rows)
            {
                HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);

                foreach (DataColumn column in SourceTable.Columns)
                {
                    ICell rowice = dataRow.CreateCell(column.Ordinal);
                    rowice.SetCellValue(row[column].ToString());
                    if (rowIndex % 2 == 1)
                        rowice.CellStyle = style1;
                    else
                        rowice.CellStyle = style2;
                }
                rowIndex++;
            }
            #endregion
            return rowIndex;

        }


        public static int BuildSheet(HSSFWorkbook workbook, ISheet sheet, DataTable SourceTable)
        {
            //版面配置
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, SourceTable.Columns.Count - 1));//合併欄位

            #region CS格式
            //抬頭文字格式
            HSSFFont font = null;
            font = (HSSFFont)workbook.CreateFont();
            font.FontHeightInPoints = 16;
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            //資料抬頭格式
            ICellStyle style_title = workbook.CreateCellStyle();
            style_title.Alignment = HorizontalAlignment.Center;
            style_title.VerticalAlignment = VerticalAlignment.Center;
            style_title.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkBlue.Index2;
            style_title.FillPattern = FillPattern.SolidForeground;
            style_title.SetFont(font);
            //內容文字格式
            HSSFFont font_data = null;
            font_data = (HSSFFont)workbook.CreateFont();
            font_data.FontHeightInPoints = 16;
            //資料內容格式(淡藍)
            ICellStyle style1 = workbook.CreateCellStyle();
            style1.Alignment = HorizontalAlignment.Left;
            style1.VerticalAlignment = VerticalAlignment.Center;
            style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.CornflowerBlue.Index;
            style1.FillPattern = FillPattern.SolidForeground;
            style1.SetFont(font_data);
            //資料內容格式(白色)
            ICellStyle style2 = workbook.CreateCellStyle();
            style2.Alignment = HorizontalAlignment.Left;
            style2.VerticalAlignment = VerticalAlignment.Center;
            style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.SetFont(font_data);
            //列印日期
            ICellStyle style3 = workbook.CreateCellStyle();
            style3.Alignment = HorizontalAlignment.Right;
            style3.VerticalAlignment = VerticalAlignment.Center;
            style3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style3.FillPattern = FillPattern.SolidForeground;
            style3.SetFont(font_data);
            #endregion

            #region DataTable資料塞入
            //文件抬頭
            HSSFRow headerRow = (HSSFRow)sheet.CreateRow(0);//創造第一排
            ICell headice = headerRow.CreateCell(0);//創造第一格位
            headice.SetCellValue("列印日期:" + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
            //headice.SetCellValue("列印日期:" + (DateTime.Now.Year - 1911) + DateTime.Now.ToString("/MM/dd HH:mm"));
            headice.CellStyle = style3;
            sheet.GetRow(0).HeightInPoints = 20;
            //資料抬頭
            HSSFRow titleRow = (HSSFRow)sheet.CreateRow(1);
            int colIndex = 0;
            foreach (DataColumn column in SourceTable.Columns)
            {
                ICell ice = titleRow.CreateCell(column.Ordinal);
                ice.SetCellValue(column.ColumnName);
                ice.CellStyle = style_title;
                sheet.SetColumnWidth(colIndex, (column.ColumnName.Length + 1) * 1024);
                colIndex++;
            }

            //資料內容
            int rowIndex = 2;
            foreach (DataRow row in SourceTable.Rows)
            {
                HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);

                foreach (DataColumn column in SourceTable.Columns)
                {
                    ICell rowice = dataRow.CreateCell(column.Ordinal);
                    rowice.SetCellValue(row[column].ToString());
                    if (rowIndex % 2 == 1)
                        rowice.CellStyle = style1;
                    else
                        rowice.CellStyle = style2;
                }
                rowIndex++;
            }
            #endregion
            return rowIndex;
        }

        public static int BuildSheet1(HSSFWorkbook workbook, ISheet sheet, DataTable SourceTable, string Title)
        {
            var chr = (char)10;
            //版面配置
            if (string.IsNullOrEmpty(Title))
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, SourceTable.Columns.Count - 1));//合併欄位

            #region CS格式
            //抬頭文字格式
            HSSFFont font = null;
            font = (HSSFFont)workbook.CreateFont();
            font.FontHeightInPoints = 16;
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            //資料抬頭格式
            ICellStyle style_title = workbook.CreateCellStyle();
            style_title.Alignment = HorizontalAlignment.Center;
            style_title.VerticalAlignment = VerticalAlignment.Center;
            style_title.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkBlue.Index2;
            style_title.FillPattern = FillPattern.SolidForeground;
            style_title.SetFont(font);
            //內容文字格式
            HSSFFont font_data = null;
            font_data = (HSSFFont)workbook.CreateFont();
            font_data.FontHeightInPoints = 16;
            //資料內容格式(淡藍)
            ICellStyle style1 = workbook.CreateCellStyle();
            style1.Alignment = HorizontalAlignment.Left;
            style1.VerticalAlignment = VerticalAlignment.Center;
            style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.CornflowerBlue.Index;
            style1.FillPattern = FillPattern.SolidForeground;
            style1.SetFont(font_data);
            //資料內容格式(白色)
            ICellStyle style2 = workbook.CreateCellStyle();
            style2.Alignment = HorizontalAlignment.Left;
            style2.VerticalAlignment = VerticalAlignment.Center;
            style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.SetFont(font_data);
            //列印日期
            ICellStyle style3 = workbook.CreateCellStyle();
            style3.Alignment = HorizontalAlignment.Right;
            style3.VerticalAlignment = VerticalAlignment.Center;
            style3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style3.FillPattern = FillPattern.SolidForeground;
            style3.SetFont(font_data);
            #endregion

            #region DataTable資料塞入
            //文件抬頭
            if (string.IsNullOrEmpty(Title))
            {
                HSSFRow headerRow = (HSSFRow)sheet.CreateRow(0);//創造第一排
                ICell headice = headerRow.CreateCell(0);//創造第一格位
                headice.SetCellValue("列印日期:" + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                //headice.SetCellValue("列印日期:" + (DateTime.Now.Year - 1911) + DateTime.Now.ToString("/MM/dd HH:mm"));
                headice.CellStyle = style3;
                sheet.GetRow(0).HeightInPoints = 20;
            }
            else
            {
                //創造第一排
                sheet.CreateRow(0).CreateCell(0).CellStyle = style3;
                sheet.GetRow(0).GetCell(0).CellStyle.Alignment = HorizontalAlignment.Center;
                sheet.GetRow(0).GetCell(0).SetCellValue(Title);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, SourceTable.Columns.Count - 3));
                sheet.GetRow(0).CreateCell(SourceTable.Columns.Count - 1).CellStyle = style3;
                sheet.GetRow(0).GetCell(SourceTable.Columns.Count - 1).CellStyle.Alignment = HorizontalAlignment.Right;
                sheet.GetRow(0).GetCell(SourceTable.Columns.Count - 1).SetCellValue("列印日期:" + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
            }


            //資料抬頭
            HSSFRow titleRow = (HSSFRow)sheet.CreateRow(1);
            int colIndex = 0;
            foreach (DataColumn column in SourceTable.Columns)
            {
                var wrapTextBool = false;
                var colTitleCnt = 0;
                ICell ice = titleRow.CreateCell(column.Ordinal);
                if (column.ColumnName.ToString().Contains("<br>"))
                {
                    wrapTextBool = true;
                    colTitleCnt = column.ColumnName.ToString().IndexOf("<br>");
                }
                else
                {
                    colTitleCnt = column.ColumnName.Length + 1;
                }
                ice.SetCellValue(column.ColumnName.Replace("<br>", chr.ToString()));
                ice.CellStyle = style_title;
                if (wrapTextBool)
                {
                    ice.CellStyle.WrapText = wrapTextBool;
                }
                sheet.SetColumnWidth(colIndex, (colTitleCnt) * 1024);
                colIndex++;
            }

            //資料內容
            int rowIndex = 2;
            foreach (DataRow row in SourceTable.Rows)
            {
                HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);

                foreach (DataColumn column in SourceTable.Columns)
                {
                    ICell rowice = dataRow.CreateCell(column.Ordinal);
                    var wrapTextBool = false;

                    if (row[column].ToString().Contains("<br>"))
                        wrapTextBool = true;
                    rowice.SetCellValue(row[column].ToString().Replace("<br>", chr.ToString()));
                    int i = (row[column].ToString().Replace("<br>", chr.ToString()).Split(chr).Length);
                    if (rowIndex % 2 == 1)
                        rowice.CellStyle = style1;
                    else
                        rowice.CellStyle = style2;
                    if (wrapTextBool)
                    {
                        rowice.CellStyle.WrapText = wrapTextBool;
                        //調整ROW的高度
                        dataRow.HeightInPoints = i * sheet.DefaultRowHeight / 10;
                    }
                }
                rowIndex++;
            }
            #endregion
            return rowIndex;
        }
        public static MemoryStream RenderDataTableToExcelFlyPay(DataTable SourceTable)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();//建立文件檔
            MemoryStream ms = new MemoryStream();//建立記憶體空間
            ISheet sheet = workbook.CreateSheet("sheet1");//開啟EXCEL分頁
            int rowIndex = BuildSheetFlyPay(workbook, sheet, SourceTable);//建立TABLE資料

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }
        public static MemoryStream RenderDataTableToExcelFlyPaySPR(DataTable SourceTable)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();//建立文件檔
            MemoryStream ms = new MemoryStream();//建立記憶體空間
            ISheet sheet = workbook.CreateSheet("sheet1");//開啟EXCEL分頁
            int rowIndex = BuildSheetFlyPaySPR(workbook, sheet, SourceTable);//建立TABLE資料

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }
        public static int BuildSheetFlyPay(HSSFWorkbook workbook, ISheet sheet, DataTable SourceTable)
        {
            //版面配置
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, SourceTable.Columns.Count - 1));//合併欄位

            #region CS格式
            //抬頭文字格式
            HSSFFont font = null;
            font = (HSSFFont)workbook.CreateFont();
            font.FontHeightInPoints = 16;
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            //資料抬頭格式
            ICellStyle style_title = workbook.CreateCellStyle();
            style_title.Alignment = HorizontalAlignment.Center;
            style_title.VerticalAlignment = VerticalAlignment.Center;
            style_title.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkBlue.Index2;
            style_title.FillPattern = FillPattern.SolidForeground;
            style_title.SetFont(font);
            //內容文字格式
            HSSFFont font_data = null;
            font_data = (HSSFFont)workbook.CreateFont();
            font_data.FontHeightInPoints = 16;
            //資料內容格式(淡藍)
            ICellStyle style1 = workbook.CreateCellStyle();
            style1.Alignment = HorizontalAlignment.Left;
            style1.VerticalAlignment = VerticalAlignment.Center;
            style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.CornflowerBlue.Index;
            style1.FillPattern = FillPattern.SolidForeground;
            style1.SetFont(font_data);
            //資料內容格式(白色)
            ICellStyle style2 = workbook.CreateCellStyle();
            style2.Alignment = HorizontalAlignment.Left;
            style2.VerticalAlignment = VerticalAlignment.Center;
            style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.SetFont(font_data);
            //列印日期
            ICellStyle style3 = workbook.CreateCellStyle();
            style3.Alignment = HorizontalAlignment.Right;
            style3.VerticalAlignment = VerticalAlignment.Center;
            style3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style3.FillPattern = FillPattern.SolidForeground;
            style3.SetFont(font_data);
            #endregion

            #region DataTable資料塞入
            //文件抬頭
            HSSFRow headerRow = (HSSFRow)sheet.CreateRow(0);//創造第一排
            ICell headice = headerRow.CreateCell(0);//創造第一格位
            headice.SetCellValue("列印日期:" + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
            //headice.SetCellValue("列印日期:" + (DateTime.Now.Year - 1911) + DateTime.Now.ToString("/MM/dd HH:mm"));
            headice.CellStyle = style3;
            sheet.GetRow(0).HeightInPoints = 20;
            //資料抬頭
            HSSFRow titleRow = (HSSFRow)sheet.CreateRow(1);
            int colIndex = 0;
            foreach (DataColumn column in SourceTable.Columns)
            {
                ICell ice = titleRow.CreateCell(column.Ordinal);
                ice.SetCellValue(column.ColumnName);
                ice.CellStyle = style_title;
                sheet.SetColumnWidth(colIndex, (column.ColumnName.Length + 1) * 1024);
                colIndex++;
            }

            //資料內容
            int rowIndex = 2;
            foreach (DataRow row in SourceTable.Rows)
            {
                HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);

                foreach (DataColumn column in SourceTable.Columns)
                {
                    ICell rowice = dataRow.CreateCell(column.Ordinal);
                    rowice.SetCellValue(row[column].ToString());

                    rowice.CellStyle = style2;
                }
                rowIndex++;
            }
            #endregion
            return rowIndex;
        }

        public static int BuildSheetFlyPaySPR(HSSFWorkbook workbook, ISheet sheet, DataTable SourceTable)
        {
            #region CS格式
            //抬頭文字格式
            HSSFFont font = null;
            font = (HSSFFont)workbook.CreateFont();
            font.FontHeightInPoints = 12;
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;

            //內容文字格式
            HSSFFont font_data = null;
            font_data = (HSSFFont)workbook.CreateFont();
            font_data.FontHeightInPoints = 12;
            //資料內容格式(橘紅) 表頭
            ICellStyle style1 = workbook.CreateCellStyle();
            style1.Alignment = HorizontalAlignment.Left;
            style1.VerticalAlignment = VerticalAlignment.Center;
            style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Coral.Index;
            style1.FillPattern = FillPattern.SolidForeground;
            style1.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thick;
            style1.BorderTop = NPOI.SS.UserModel.BorderStyle.Thick;
            style1.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thick;
            style1.BorderRight = NPOI.SS.UserModel.BorderStyle.Thick;
            style1.SetFont(font_data);
            //資料內容格式(白)一般數據
            ICellStyle style2 = workbook.CreateCellStyle();
            style2.Alignment = HorizontalAlignment.Left;
            style2.VerticalAlignment = VerticalAlignment.Center;
            style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style2.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style2.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style2.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            style2.SetFont(font_data);
            //資料內容格式(橘)上限數據
            ICellStyle style3 = workbook.CreateCellStyle();
            style3.Alignment = HorizontalAlignment.Left;
            style3.VerticalAlignment = VerticalAlignment.Center;
            style3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Tan.Index;
            style3.FillPattern = FillPattern.SolidForeground;
            style3.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style3.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style3.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style3.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            style3.SetFont(font_data);
            //資料內容格式(黃)小計數據
            ICellStyle style4 = workbook.CreateCellStyle();
            style4.Alignment = HorizontalAlignment.Left;
            style4.VerticalAlignment = VerticalAlignment.Center;
            style4.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            style4.FillPattern = FillPattern.SolidForeground;
            style4.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style4.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style4.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style4.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            style4.SetFont(font_data);
            #endregion

            #region DataTable資料塞入
            //資料抬頭
            HSSFRow titleRow = (HSSFRow)sheet.CreateRow(0);
            int colIndex = 0;
            foreach (DataColumn column in SourceTable.Columns)
            {
                ICell ice = titleRow.CreateCell(column.Ordinal);
                ice.SetCellValue(column.ColumnName);
                ice.CellStyle = style1;
                if (column.ColumnName == "集檢所")
                {
                    sheet.SetColumnWidth(colIndex, (column.ColumnName.Length + 1) * 1024 * 4/3);
                }
                else
                {
                    sheet.SetColumnWidth(colIndex, (column.ColumnName.Length + 1) * 1024/3);
                }
                colIndex++;
            }

            //資料內容
            int rowIndex = 1;
            foreach (DataRow row in SourceTable.Rows)
            {
                HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);
                if (rowIndex == 3 || rowIndex == 4 || rowIndex == 23 || rowIndex == 33)
                {
                    // 上限數據
                    foreach (DataColumn column in SourceTable.Columns)
                    {
                        ICell rowice = dataRow.CreateCell(column.Ordinal);
                        rowice.SetCellValue(row[column].ToString());
                        rowice.CellStyle = style3;
                    }
                }
                else if (rowIndex == 5 || rowIndex == 24 || rowIndex == 34)
                {
                    // 小計數據
                    foreach (DataColumn column in SourceTable.Columns)
                    {
                        ICell rowice = dataRow.CreateCell(column.Ordinal);
                        rowice.SetCellValue(row[column].ToString());
                        rowice.CellStyle = style4;
                    }
                }
                else
                {
                    // 一般數據
                    foreach (DataColumn column in SourceTable.Columns)
                    {
                        ICell rowice = dataRow.CreateCell(column.Ordinal);
                        rowice.SetCellValue(row[column].ToString());
                        rowice.CellStyle = style2;
                    }
                }

                rowIndex++;
            }
            #endregion
            return rowIndex;
        }

        private void SetColumnWidthAuto(ISheet sheet, int colWidth)
        {

        }

        public static MemoryStream RenderDataTableToExcelPayEC(DataTable SourceTable)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();//建立文件檔
            MemoryStream ms = new MemoryStream();//建立記憶體空間
            ISheet sheet = workbook.CreateSheet("sheet1");//開啟EXCEL分頁
            int rowIndex = BuildSheetPayEC(workbook, sheet, SourceTable);//建立TABLE資料

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }

        public static int BuildSheetPayEC(HSSFWorkbook workbook, ISheet sheet, DataTable SourceTable)
        {
            //版面配置
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, SourceTable.Columns.Count - 1));//合併欄位

            #region CS格式
            //抬頭文字格式
            HSSFFont font = null;
            font = (HSSFFont)workbook.CreateFont();
            font.FontHeightInPoints = 16;
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            //資料抬頭格式
            ICellStyle style_title = workbook.CreateCellStyle();
            style_title.Alignment = HorizontalAlignment.Center;
            style_title.VerticalAlignment = VerticalAlignment.Center;
            style_title.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkBlue.Index2;
            style_title.FillPattern = FillPattern.SolidForeground;
            style_title.SetFont(font);
            //內容文字格式
            HSSFFont font_data = null;
            font_data = (HSSFFont)workbook.CreateFont();
            font_data.FontHeightInPoints = 16;
            //資料內容格式(淡藍)
            ICellStyle style1 = workbook.CreateCellStyle();
            style1.Alignment = HorizontalAlignment.Left;
            style1.VerticalAlignment = VerticalAlignment.Center;
            style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.CornflowerBlue.Index;
            style1.FillPattern = FillPattern.SolidForeground;
            style1.SetFont(font_data);
            //資料內容格式(白色)
            ICellStyle style2 = workbook.CreateCellStyle();
            style2.Alignment = HorizontalAlignment.Left;
            style2.VerticalAlignment = VerticalAlignment.Center;
            style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.SetFont(font_data);
            //列印日期
            ICellStyle style3 = workbook.CreateCellStyle();
            style3.Alignment = HorizontalAlignment.Right;
            style3.VerticalAlignment = VerticalAlignment.Center;
            style3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            style3.FillPattern = FillPattern.SolidForeground;
            style3.SetFont(font_data);
            #endregion

            #region DataTable資料塞入
            //文件抬頭
            HSSFRow headerRow = (HSSFRow)sheet.CreateRow(0);//創造第一排
            ICell headice = headerRow.CreateCell(0);//創造第一格位
            headice.SetCellValue("列印日期:" + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
            //headice.SetCellValue("列印日期:" + (DateTime.Now.Year - 1911) + DateTime.Now.ToString("/MM/dd HH:mm"));
            headice.CellStyle = style3;
            sheet.GetRow(0).HeightInPoints = 20;
            //資料抬頭
            HSSFRow titleRow = (HSSFRow)sheet.CreateRow(1);
            int colIndex = 0;
            foreach (DataColumn column in SourceTable.Columns)
            {
                ICell ice = titleRow.CreateCell(column.Ordinal);
                ice.SetCellValue(column.ColumnName);
                ice.CellStyle = style_title;
                sheet.SetColumnWidth(colIndex, (column.ColumnName.Length + 1) * 1024);
                colIndex++;
            }

            //資料內容
            int rowIndex = 2;
            foreach (DataRow row in SourceTable.Rows)
            {
                HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);

                foreach (DataColumn column in SourceTable.Columns)
                {
                    ICell rowice = dataRow.CreateCell(column.Ordinal);
                    rowice.SetCellValue(row[column].ToString());

                    rowice.CellStyle = style2;
                }
                rowIndex++;
            }
            #endregion
            return rowIndex;
        }
    }
}