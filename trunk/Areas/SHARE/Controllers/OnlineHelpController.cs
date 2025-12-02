using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.SHARE.Models;
using EECOnline.Models;
using EECOnline.DataLayers;
using Turbo.Commons;
using EECOnline.Controllers;

namespace EECOnline.Areas.SHARE.Controllers
{
    public class OnlineHelpController : BaseController
    {
        public ActionResult Index(string lp)
        {
            SHAREDAO dao = new SHAREDAO();
            OnlineHelpFormModel form = new OnlineHelpFormModel();

            // 檔案路徑
            var localPath = dao.GetServerLocalPath("OnlineHelp");
            form.Help_Url = localPath + lp + ".htm";
            //form.Help_Url = "/Template/Application.htm";

            return View(form);
        }

    }
}