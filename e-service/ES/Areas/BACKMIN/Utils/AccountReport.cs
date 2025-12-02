using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Utils;
using WebUI.CustomClass;

namespace ES.Areas.BACKMIN.Utils
{
    public class AccountReport : GemboxGenerator
    {
        private AccountQueryModel Qry { get; set; }

        private DataTable Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AccountReport(AccountQueryModel inQry)
        {
            this.Qry = inQry;
        }

        public override void GetData()
        {
            ReportAction temp = new ReportAction();

            this.Data = temp.AccountReport(this.Qry.Permission);
        }

        public override bool DataCheck()
        {
            return this.Data.Rows.Count != 0;
        }

        public override void ExportXls()
        {

            base.FilePath = DataUtils.GetConfig("FOLDER_TEMPLATE") + "/Temp";

            if (!Directory.Exists(base.FilePath))
            {
                Directory.CreateDirectory(base.FilePath);
            }

            base.FilePath = base.FilePath + "/" + DateTime.Now.ToString("yyyyMMddHHmmss");

            FileStream file = new FileStream(base.FilePath + ".xls", FileMode.Create, FileAccess.Write);

            //ReportUtils.RenderDataTableToExcel(this.Data).WriteTo(file);

            ReportUtils.RenderDataTableToODS(this.Data).WriteTo(file);

            file.Close();            
        }
        /// <summary>ReportAction.AccountReport  裡面已經有了  不實做
        /// 
        /// </summary>
        public override void CreateTitle()
        {
            throw new NotImplementedException();
        }
        /// <summary>ReportAction.AccountReport  裡面已經有了  不實做
        /// 
        /// </summary>
        public override void AddData()
        {
            throw new NotImplementedException();
        }


        /// <summary>  Initial   processing 
        /// /
        /// </summary>
        /// <returns></returns>
        public bool Processing()
        {
            this.GetData();

            if (!this.DataCheck())
            {
                this.Error = "查無資料";
                return false;
            }

            this.ExportXls();

            base.GemboxGeneratorFile();

            if (!File.Exists(this.FilePath + ".ods"))
            {
                this.Error = "ODS產出檔案失敗";
                return false;
            }
            else
            {
                return true;
            }


        }
        
    }
}