using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Areas.A8.Models;
using EECOnline.Services;
using EECOnline.Commons;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using System;
using Turbo.Commons;

namespace EECOnline.DataLayers
{
    public class A8DAO : BaseDAO
    {
        #region 共用

        /// <summary>
        /// 傳入使用者輸入的密碼明文, 加密後回傳
        /// </summary>
        /// <param name="usePwd"></param>
        /// <returns></returns>
        private string EncPassword(string userPwd)
        {
            if (string.IsNullOrWhiteSpace(userPwd))
            {
                throw new ArgumentNullException("userPwd");
            }
            //TODO: 置換 RSACSP 改成不可逆的 Hash 方法
            RSACSP.RSACSP rsa = new RSACSP.RSACSP();
            return rsa.Utl_Encrypt(userPwd);
        }

        #endregion


        #region C101M

        /// <summary>
        /// 查詢群組
        /// </summary>
        public IList<C101MGridModel> QueryC101MGrid(C101MFormModel parm)
        {
            return base.QueryForList<C101MGridModel>("A8.queryC101MGrid", parm);
        }

        #endregion C101M

        #region C102M

        /// <summary>
        /// 查詢群組
        /// </summary>
        public IList<C102MGridModel> QueryC102MGrid(C102MFormModel parm)
        {
            return base.QueryForList<C102MGridModel>("A8.queryC102MGrid", parm);
        }

        #endregion C102M

        #region C103M

        /// <summary>
        /// 查詢群組
        /// </summary>
        public IList<C103MGridModel> QueryC103MGrid(C103MFormModel parm)
        {
            SessionModel sm = SessionModel.Get();
            var unit_cd = sm.UserInfo.User.unit_cd;
            parm.unit_cd = unit_cd;

            return base.QueryForList<C103MGridModel>("A8.queryC103MGrid", parm);
        }

        #endregion C103M
    }
}