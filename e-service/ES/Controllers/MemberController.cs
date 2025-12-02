using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ES.Utils;
using System.Data.SqlClient;
using log4net;
using ES.Models;
using ES.Action;
using System.Web.Configuration;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;

namespace ES.Controllers
{
    public class MemberController : BaseController
    {

        /// <summary>
        /// 登入頁面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            string s_log1 = "";
            s_log1 += string.Format("\n ##MemberController.Index");
            logger.Debug(s_log1);

            //LoginViewModel viewModel = new LoginViewModel();
            //ActionResult rtn = View("Index", viewModel);
            //return rtn;

            //sm.LastErrorMessage = "登入頁面!";
            return RedirectToAction("Index", "Login");

        }

    }
}
