using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A2.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;

namespace EECOnline.Areas.A2.Controllers
{
    public class C102MController : BaseController
    {
        private static new readonly ILog LOG = LogManager.GetLogger(typeof(C102MController));

        public ActionResult Index(C102MFormModel Form)
        {
            try
            {
                SessionModel sm = SessionModel.Get();
                A2DAO dao = new A2DAO();
                ActionResult rtn = View(Form);
                if (ModelState.IsValid)
                {
                    ModelState.Clear();
                    // 醫院登入時，只能看得到自己的資料
                    Form.HospCode = null;
                    if (sm.UserInfo.LoginTab == "2" && sm.UserInfo.HospitalCode.TONotNullString() != "")
                    {
                        Form.HospCode = sm.UserInfo.HospitalCode;
                    }
                    Form.Grid = dao.QueryC102M_Grid(Form);
                    Form.LogGrid = dao.QueryC102M_LogGrid(Form);
                }
                return rtn;
            }
            catch (Exception ex)
            {
                LOG.Error("A2/C102M Index Error: " + ex.Message, ex);
                throw;
            }
        }

        public ActionResult Upload(string apply_no_sub, string his_type)
        {
            SessionModel sm = SessionModel.Get();
            C102MUploadModel model = new C102MUploadModel();
            model.apply_no_sub = apply_no_sub;
            model.his_type = his_type;
            return View("Upload", model);
        }

        private string isFileCheckOK(HttpPostedFileBase YourFile)
        {
            string Result = "";
            const string acceptType = ".PDF,.JPG,.BMP,.PNG,.GIF,.TIF";
            if (YourFile == null || YourFile.ContentLength.TOInt32() <= 0) return "請選擇上傳檔案！<br/ >";
            if (!acceptType.Contains(Path.GetExtension(YourFile.FileName).ToUpper())) Result = Result + "請選擇正確的檔案格式！ (" + acceptType + ")<br/ >";
            if (YourFile.ContentLength.TOInt32() > (20 * 1024 * 1024)) Result = Result + "檔案大小以 20MB 為限！<br/ >";
            return Result;
        }

        [HttpPost]
        public ActionResult SaveUpload(C102MUploadModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            var ErrMsg = this.isFileCheckOK(model.UploadFILE);
            if (ErrMsg != "") sm.LastErrorMessage = ErrMsg;
            else
            {
                var dao = new A2DAO();
                var logData = new TblEEC_ApplyDetailUploadLog();

                var data1 = dao.GetRow(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.apply_no_sub, his_type = model.his_type });
                if (data1 == null) return new HttpNotFoundResult();
                logData.InjectFrom(data1);

                var data2 = dao.GetRow(new TblEEC_ApplyDetail() { apply_no_sub = model.apply_no_sub });
                if (data2 == null) return new HttpNotFoundResult();
                logData.InjectFrom(data2);

                var data3 = dao.GetRow(new TblEEC_Apply() { apply_no = data2.apply_no });
                if (data3 == null) return new HttpNotFoundResult();
                logData.InjectFrom(data3);

                logData.keyid = null;
                logData.provide_bin = this.TransFileToBase64(model.UploadFILE);
                logData.provide_ext = System.IO.Path.GetExtension(model.UploadFILE.FileName);
                logData.provide_datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                logData.provide_user_no = sm.UserInfo.User.userno;
                logData.provide_user_name = sm.UserInfo.User.username;

                int res1 = dao.Insert(logData);
                int res2 = dao.Update(
                    new TblEEC_ApplyDetailPrice()
                    {
                        provide_bin = logData.provide_bin,
                        provide_ext = logData.provide_ext,
                        provide_datetime = logData.provide_datetime
                    },
                    new TblEEC_ApplyDetailPrice()
                    {
                        apply_no_sub = model.apply_no_sub,
                        his_type = model.his_type
                    }
                );

                sm.LastResultMessage = "資料已儲存";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "A2" });
            }
            return View("Upload", model);
        }

        private string TransFileToBase64(HttpPostedFileBase YourFile)
        {
            //string theFileName = Path.GetFileName(YourFile.FileName);
            byte[] thePictureAsBytes = new byte[YourFile.ContentLength];
            using (BinaryReader theReader = new BinaryReader(YourFile.InputStream))
            {
                thePictureAsBytes = theReader.ReadBytes(YourFile.ContentLength);
            }
            return Convert.ToBase64String(thePictureAsBytes);
        }
    }
}