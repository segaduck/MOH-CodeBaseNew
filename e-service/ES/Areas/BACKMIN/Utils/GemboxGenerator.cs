using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using GemBox.Spreadsheet;
using System.IO;
using log4net;

namespace ES.Areas.BACKMIN.Utils
{
    public abstract class GemboxGenerator
    {
        public ExcelFile EF { get; set; }

        public string FilePath { get; set; }

        public string Error { get; set; }

        public abstract void GetData();

        public abstract bool DataCheck();

        public abstract void ExportXls();

        public abstract void CreateTitle();

        public abstract void AddData();

        public void GemboxGeneratorFile()
        {
            //設定序號
            string GemBoxKey = "EAAN-YCD0-1FIF-U4PG";
            //SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            SpreadsheetInfo.SetLicense(GemBoxKey);

            if (!File.Exists(this.FilePath + ".xls"))
            {
                return;
            }
            try
            {
                this.EF = ExcelFile.Load(this.FilePath + ".xls");

                this.EF.Save(this.FilePath + ".ods");
            }
            catch (Exception ex)
            {
                //todo 寫GEMBOX錯誤訊息
              
            }

        }

        

    }
}