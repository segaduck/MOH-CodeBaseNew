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
using NPOI.XSSF.UserModel;
using log4net;
using ES.Utils;

namespace WebUI.CustomClass
{
    /// <summary>
    /// reportLabs 的摘要描述
    /// </summary>
    public class FlyPayUtils
    {
        protected static readonly ILog logger = LogUtils.GetLogger();

        private static String[] col_name = {
            "FirstName",
            "MiddleName",
            "LastName",
            "Birth",
            "Gender",
            "MainDocNo",
            "Nationality",
            "FlightDate",
            "FlightNo",
            "DepartAirport",
            "PassengerType"
        };

        public FlyPayUtils()
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //
        }

        public static List<Map> readEXCEL(XSSFWorkbook ms)
        {
            ISheet sheet = ms.GetSheetAt(0);
            List<Map> li = new List<Map>();
            for (int i = 1; i < sheet.LastRowNum + 1; i++)
            {
                Map map = new Map();
                for (int j = 0; j < sheet.GetRow(i).Cells.Count(); j++)
                {
                    // 空值寫入
                    ICell ic = sheet.GetRow(i).GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                    if (j > col_name.Length - 1) { break; }

                    //object obj_temp = new object();
                    string s_temp = "";
                    if (ic != null)
                    {
                        ic.SetCellType(CellType.String);
                        //obj_temp = ic.StringCellValue;
                        s_temp = ic.StringCellValue;
                    }
                    map.Add(col_name[j], s_temp);
                }

                li.Add(map);
            }
            return li;
        }

        public static List<Map> readEXCEL_backmin(HSSFWorkbook ms)
        {
            ISheet sheet = ms.GetSheetAt(0);

            List<Map> li = new List<Map>();

            //欄位數-全部
            //int col_length = 21;
            //int tt_row = 1;
            //string s_name2 = "";
            //for (int j = 0; j < sheet.GetRow(tt_row).Cells.Count(); j++)
            //{
            //    if (j > col_length - 1) { break; }
            //    ICell ic = sheet.GetRow(tt_row).GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);
            //    object obj_temp = new object();
            //    if (ic == null) { obj_temp = ""; }
            //    if (ic != null) {
            //        ic.SetCellType(CellType.String);
            //        obj_temp = ic.StringCellValue;
            //    }
            //    if (!s_name2.Equals("")) { s_name2 += ","; }
            //    s_name2 += obj_temp;
            //}

            string s_colname2 = "FisrtName,MiddleName,LastName,Birth,Gender,MainDocNo,Nationality,FlightDate,FlightNo,DepartAirport,PassengerType,防疫所別,為應繳費人員,繳費狀態,繳費日期,繳費金額,撥款日期,授權碼,繳費編號,中信繳費編號,銀聯繳費編號,識別碼,驗證碼,資料行";
            //欄位名-全部
            string[] a_colname2 = s_colname2.Split(',');

            string s_log1 = "";
            s_log1 = string.Format("s_colname2:{0}", s_colname2);
            logger.Debug(s_log1);

            for (int i = 1; i < sheet.LastRowNum + 1; i++)
            {
                Map map = new Map();
                for (int j = 0; j < sheet.GetRow(i).Cells.Count(); j++)
                {
                    // 空值寫入
                    ICell ic = sheet.GetRow(i).GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                    if (j > a_colname2.Length - 1) { break; }

                    //object obj_temp = new object();
                    string s_temp = "";
                    //if (ic == null) { obj_temp = ""; }
                    if (ic != null)
                    {
                        ic.SetCellType(CellType.String);
                        //obj_temp = ic.StringCellValue;
                        s_temp = ic.StringCellValue;
                    }
                    map.Add(a_colname2[j], s_temp); //obj_temp);
                }

                li.Add(map);
            }
            return li;
        }
    }
}