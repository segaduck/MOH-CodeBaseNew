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
using EECOnline.Areas.A4.Models;

namespace EECOnline.DataLayers
{
    public class A4DAO : BaseDAO
    {
        public IList<C101MGridModel> QueryC101M_All(C101MFormModel parm)
        {
            return base.QueryForListAll<C101MGridModel>("A4.queryC101M", parm);
        }

        public IList<C103MGridModel> QueryC103M_All(C103MFormModel parm)
        {
            return base.QueryForListAll<C103MGridModel>("A4.queryC103M", parm);
        }
    }
}