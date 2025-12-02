using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Controllers;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Utils;
using System.IO;
using log4net;

namespace ES.Areas.Admin.Controllers
{
    public class DeleteController : BaseController
    {
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleSystemLogger");

        public ActionResult Index()
        {
            try
            {
                logger.Debug("Client IP: " + GetClientIP());
                logger.Debug("刪除申請單暫存資料表開始");
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    DeleteAction action = new DeleteAction(conn);
                    action.DeleteApplyTemp();
                    conn.Close();
                    conn.Dispose();
                }
                logger.Debug("刪除申請單暫存資料表結束");

                logger.Debug("刪除申請單暫存檔案開始");

                int date = Int32.Parse(DateTime.Now.AddYears(-1).ToString("yyyyMMdd"));
                int t;

                string path = DataUtils.GetConfig("FOLDER_APPLY_FILE") + "TEMP\\";
                logger.Debug("申請單暫存檔案路徑：" + path);

                DirectoryInfo dir = new DirectoryInfo(path);

                if (dir.Exists)
                {
                    foreach (DirectoryInfo dirInfo in dir.GetDirectories())
                    {
                        //logger.Debug("dirInfo: " + dirInfo.Name);

                        foreach (FileInfo fileInfo in dirInfo.GetFiles())
                        {
                            //logger.Debug("fileInfo: " + fileInfo.Name);

                            if (fileInfo.Name.Length >= 8 && Int32.TryParse(fileInfo.Name.Substring(0, 8), out t))
                            {
                                if (t <= date)
                                {
                                    logger.Debug("DELETE: " + fileInfo.FullName);
                                    fileInfo.Delete();
                                }
                            }
                        }
                    }
                }

                logger.Debug("刪除申請單暫存檔案結束");

                ViewBag.Message = "完成";
            }
            catch (Exception e)
            {
                ViewBag.Message = "發生錯誤：" + e;
                logger.Warn("發生錯誤：" + e.Message, e);
            }

            return View("Message");
        }

    }
}
