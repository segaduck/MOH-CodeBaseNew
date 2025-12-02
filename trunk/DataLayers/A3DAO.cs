using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Services;
using EECOnline.Commons;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using System;
using Turbo.Commons;
using EECOnline.Areas.A3.Models;

namespace EECOnline.DataLayers
{
    public class A3DAO : BaseDAO
    {
        public IList<C101MGridModel> QueryC101M(C101MFormModel parmas)
        {
            return base.QueryForList<C101MGridModel>("A3.queryC101M", parmas);
        }

        public IList<C101MGridModel> QueryC101MAll(C101MFormModel parmas)
        {
            return base.QueryForListAll<C101MGridModel>("A3.queryC101M", parmas);
        }

        public IList<C102MGridModel> QueryC102M(C102MFormModel parmas)
        {
            return base.QueryForList<C102MGridModel>("A3.queryC102M", parmas);
        }

        public IList<C102MGridModel> QueryC102MAll(C102MFormModel parmas)
        {
            return base.QueryForListAll<C102MGridModel>("A3.queryC102M", parmas);
        }

        public IList<C103MGridModel> QueryC103M(C103MFormModel parmas)
        {
            return base.QueryForList<C103MGridModel>("A3.queryC103M", parmas);
        }

        public IList<C103MGridModel> QueryC103MAll(C103MFormModel parmas)
        {
            return base.QueryForListAll<C103MGridModel>("A3.queryC103M", parmas);
        }

        public IList<C104MForm1GridModel> QueryC104M_1(C104MForm1Model parmas)
        {
            return base.QueryForList<C104MForm1GridModel>("A3.queryC104M_1", parmas);
        }

        public IList<C104MForm1GridModel> QueryC104M_1All(C104MForm1Model parmas)
        {
            return base.QueryForListAll<C104MForm1GridModel>("A3.queryC104M_1", parmas);
        }

        public IList<C104MForm2GridModel> QueryC104M_2(C104MForm2Model parmas)
        {
            return base.QueryForList<C104MForm2GridModel>("A3.queryC104M_2", parmas);
        }

        public IList<C104MForm2GridModel> QueryC104M_2All(C104MForm2Model parmas)
        {
            return base.QueryForListAll<C104MForm2GridModel>("A3.queryC104M_2", parmas);
        }

        public void C101M_SaveRspStatus(string orderid, bool rsp_status)
        {
            var getData = base.GetRow(new TblEEC_ApplyDetail() { payed_orderid = orderid, payed = "Y" });
            if (getData == null) return;
            if (rsp_status)
            {
                // 請款成功
                base.Update(
                    new TblEEC_ApplyDetail() { is_request_payment = "Y" },
                    new TblEEC_ApplyDetail() { keyid = getData.keyid }
                );
            }
            else
            {
                // 未請款成功
                // 先不做事
            }
        }
    }
}