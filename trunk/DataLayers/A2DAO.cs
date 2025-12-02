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
using EECOnline.Areas.A2.Models;

namespace EECOnline.DataLayers
{
    public class A2DAO : BaseDAO
    {
        #region C101M

        public IList<C101MGridModel> QueryC101M(C101MFormModel Form)
        {
            return base.QueryForList<C101MGridModel>("A2.queryC101M", Form);
        }

        public C101MDetailModel DetailC101M(string apply_no_sub)
        {
            Hashtable parmas = new Hashtable();
            parmas["apply_no_sub"] = apply_no_sub;
            return base.QueryForObject<C101MDetailModel>("A2.detailC101M", parmas);
        }

        public IList<C101MDetailGridModel> DetailC101MGrid(string apply_no_sub)
        {
            Hashtable parmas = new Hashtable();
            parmas["apply_no_sub"] = apply_no_sub;
            return base.QueryForListAll<C101MDetailGridModel>("A2.detailC101MGrid", parmas);
        }

        #endregion

        #region C102M

        public IList<C102MGridModel> QueryC102M_Grid(C102MFormModel Form)
        {
            return base.QueryForListAll<C102MGridModel>("A2.queryC102M_Grid", Form);
        }

        public IList<C102MLogGridModel> QueryC102M_LogGrid(C102MFormModel Form)
        {
            return base.QueryForListAll<C102MLogGridModel>("A2.queryC102M_LogGrid", Form);
        }

        #endregion
    }
}