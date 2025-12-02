using System.IO;
using System.Linq;
using System.Web.Mvc;
using EECOnline.Commons.Filter;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Services;
using EECOnline.Commons;
using System.Collections;
using System.Collections.Generic;

namespace EECOnline.Controllers
{
    /// <summary>
    /// 系統靜態頁
    /// </summary>
    public class WebLockController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Maintenance()
        {
            FrontDAO dao = new FrontDAO();
            var IsLock = SHAREDAO.GetSetup("WebLock") == "1" ? true : false;
            if (!IsLock)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View("Maintenance");
        }
    }
}