using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Models.Entities;
using ES.Commons;
using ES.DataLayers;

namespace ES.Models
{
    /// <summary>
    /// 前台會員
    /// </summary>
    public class ClamMember : TblMEMBER
    {
        public string BIRTHDAY_TW
        {
            get
            {
                //twYYYMMDD 回傳給系統
                return (BIRTHDAY.HasValue ? HelperUtil.TransToTwYear(BIRTHDAY.Value) : "");
            }
        }

        public string BIRTHDAY_AD
        {
            get
            {
                //AD-YYYYMMDD 回傳給系統
                return (BIRTHDAY.HasValue ? HelperUtil.DateTimeToString(BIRTHDAY.Value) : "");
            }
        }

        public string ADDR_TEXT
        {
            get
            {
                var dao = new MyKeyMapDAO();
                KeyMapModel kmm = dao.GetCityTownName(CITY_CD);
                return (kmm != null ? kmm.TEXT : "");
            }
        }

    }
}