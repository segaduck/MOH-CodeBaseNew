using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Controllers;

namespace ES.Areas.Schedule.Controllers
{
    public class TestController : BaseController
    {
        //
        // GET: /Schedule/Test/

        public ActionResult Index()
        {
            logger.Debug("TEST...");

            return View("Message");
        }
    }
}
