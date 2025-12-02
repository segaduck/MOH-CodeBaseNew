using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models;

namespace ES.Controllers
{
    public class MessageBoxController : Controller
    {
        /// <summary>
        /// 顯示訊息
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            MessageBoxModel model = (MessageBoxModel)TempData["MessageBoxModel"];

            return View(model);
        }

    }
}
