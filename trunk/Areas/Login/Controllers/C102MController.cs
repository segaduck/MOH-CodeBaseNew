using EECOnline.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Models;
using EECOnline.Areas.Login.Models;
using EECOnline.Services;
using EECOnline.Commons;
using EECOnline.DataLayers;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;

namespace EECOnline.Areas.Login.Controllers
{
    public class C102MController : BaseController
    {
        public ActionResult Index()
        {
            LoginDAO dao = new LoginDAO();
            var model = dao.queryHomeInfo();
            return View("Index", model);
        }

        public ActionResult News(string NewsID)
        {
            SessionModel sm = SessionModel.Get();
            C102MNewsDetailModel model = new C102MNewsDetailModel();
            LoginDAO dao = new LoginDAO();
            var TheNewsID = -1;
            int.TryParse(NewsID, out TheNewsID);
            var findData = dao.GetRow(new TblENEWS() { enews_id = TheNewsID });
            if (findData == null)
                sm.LastErrorMessage = "查無資料";
            else
            {
                model.InjectFrom(findData);
                model.Files = dao.GetRowList(new TblEFILE()
                {
                    peky1 = "ENEWS",
                    peky2 = model.enews_id.TONotNullString()
                });
            }
            return View("NewsDetail", model);
        }
    }
}