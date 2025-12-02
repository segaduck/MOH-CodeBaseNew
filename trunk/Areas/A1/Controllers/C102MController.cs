using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A1.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;
using OfficeOpenXml;
using Turbo.DataLayer;

namespace EECOnline.Areas.A1.Controllers
{
    public class C102MController : BaseController
    {
        private C102MFormModel ResetModel(string AuthCode)
        {
            A1DAO dao = new A1DAO();
            var loginHosp = dao.GetRowList(new TblEEC_Hospital() { AuthCode = AuthCode });
            var form = new C102MFormModel();
            form.hospital_code = loginHosp.FirstOrDefault().code;
            form.AuthDate = loginHosp.FirstOrDefault().AuthDate;
            form.Email = loginHosp.FirstOrDefault().Email;
            form.Grid = dao.QueryC102M(form);
            return form;
        }

        public ActionResult Index(C102MFormModel form)
        {
            ModelState.Clear();
            A1DAO dao = new A1DAO();
            SessionModel sm = SessionModel.Get();
            if (form.hospital_code.TONotNullString() == "")
                form.hospital_code = form.hospital_code_list.FirstOrDefault().Value;
            if (form.AuthCode.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請先取得醫院授權碼！";
                if (sm.UserInfo.LoginTab == "1")
                    return Redirect(Url.Action("Index", "C101M", new { area = "A1", code = form.hospital_code }));
                else
                    return View("Index", form);
            }
            // 檢查 此醫院是否在 EEC_Hospital_SetPrice 有資料
            var hospPriList = dao.GetRowList(new TblEEC_Hospital_SetPrice() { hospital_code = form.hospital_code });
            if (hospPriList.ToCount() <= 0)
            {
                // 如果沒有 就新增預設資料進去
                var typeList = new ShareCodeListModel().Get_HIS_Type_AllList();
                foreach (var row in typeList)
                {
                    dao.Insert(new TblEEC_Hospital_SetPrice()
                    {
                        hospital_code = form.hospital_code,
                        hospital_name = form.hospital_name,
                        his_type = row.Value,
                        his_type_name = row.Text,
                        price = 0,
                    });
                }
            }
            return View("Index", this.ResetModel(form.AuthCode));
        }

        public ActionResult Save(C102MFormModel form)
        {
            ModelState.Clear();
            A1DAO dao = new A1DAO();
            SessionModel sm = SessionModel.Get();
            // 先檢查一下
            if (form.Grid.ToCount() <= 0) sm.LastErrorMessage = "查無資料！";
            else
            {
                // 儲存程序
                foreach (var row in form.Grid)
                {
                    TblEEC_Hospital_SetPrice where = new TblEEC_Hospital_SetPrice() { keyid = row.keyid };
                    TblEEC_Hospital_SetPrice update = new TblEEC_Hospital_SetPrice()
                    {
                        price = row.price ?? 0,
                        show_date1 = row.show_date1,
                        show_date2 = row.show_date2,
                    };
                    ClearFieldMap cfmModel = new ClearFieldMap();
                    cfmModel.Add((TblEEC_Hospital_SetPrice x) => x.price);
                    cfmModel.Add((TblEEC_Hospital_SetPrice x) => x.show_date1);
                    cfmModel.Add((TblEEC_Hospital_SetPrice x) => x.show_date2);
                    int res = dao.Update(update, where, cfmModel);
                }
                sm.LastResultMessage = "已儲存";
            }
            return View("Index", this.ResetModel(form.AuthCode));
        }

        public ActionResult Cancel(C102MFormModel form)
        {
            ModelState.Clear();
            A1DAO dao = new A1DAO();
            SessionModel sm = SessionModel.Get();
            // 先檢查一下
            int ckKey = -1;
            if (!int.TryParse(form.DoClearKeyid, out ckKey)) sm.LastErrorMessage = "操作失敗！";
            else if (form.Grid.ToCount() <= 0) sm.LastErrorMessage = "查無資料！";
            else if (form.Grid.Where(x => x.keyid == ckKey).ToCount() <= 0) sm.LastErrorMessage = "查無資料！";
            else
            {
                // 清除程序
                TblEEC_Hospital_SetPrice where = new TblEEC_Hospital_SetPrice() { keyid = ckKey };
                TblEEC_Hospital_SetPrice update = new TblEEC_Hospital_SetPrice() { price = 0 };
                ClearFieldMap cfmModel = new ClearFieldMap();
                cfmModel.Add((TblEEC_Hospital_SetPrice x) => x.price);
                cfmModel.Add((TblEEC_Hospital_SetPrice x) => x.show_date1);
                cfmModel.Add((TblEEC_Hospital_SetPrice x) => x.show_date2);
                int res = dao.Update(update, where, cfmModel);
                sm.LastResultMessage = "已清除";
            }
            return View("Index", this.ResetModel(form.AuthCode));
        }
    }
}