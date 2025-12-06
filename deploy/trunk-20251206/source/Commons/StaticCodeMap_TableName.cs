using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;

namespace EECOnline.Commons
{
    /// <summary>
    /// 系統代碼及表格名稱列舉
    /// </summary>
    public partial class StaticCodeMap
    {
        /// <summary>
        /// 系統表格名稱列舉
        /// </summary>
        public class TableName
        {
            public static DBRowTableName AMCHANGEPWD_GUID = DBRowTableName.Instance("AMCHANGEPWD_GUID");
            public static DBRowTableName AMCHANGEPWD_LOG = DBRowTableName.Instance("AMCHANGEPWD_LOG");
            public static DBRowTableName AMDBURM = DBRowTableName.Instance("AMDBURM");
            public static DBRowTableName AMDBURMTEMP = DBRowTableName.Instance("AMDBURMTEMP");
            public static DBRowTableName AMEMAILLOG_EMAIL = DBRowTableName.Instance("AMEMAILLOG_EMAIL");
            public static DBRowTableName AMFUNCM = DBRowTableName.Instance("AMFUNCM");
            public static DBRowTableName AMFUNLOG = DBRowTableName.Instance("AMFUNLOG");
            public static DBRowTableName AMGMAPM = DBRowTableName.Instance("AMGMAPM");
            public static DBRowTableName AMGRP = DBRowTableName.Instance("AMGRP");
            public static DBRowTableName AMURLOG = DBRowTableName.Instance("AMURLOG");
            public static DBRowTableName AMUROLE = DBRowTableName.Instance("AMUROLE");
            public static DBRowTableName APPLY_FILE = DBRowTableName.Instance("APPLY_FILE");
            public static DBRowTableName CODE = DBRowTableName.Instance("CODE");
            public static DBRowTableName CODE1 = DBRowTableName.Instance("CODE1");
            public static DBRowTableName CONTACT = DBRowTableName.Instance("CONTACT");
            public static DBRowTableName EFILE = DBRowTableName.Instance("EFILE");
            public static DBRowTableName LOGINLOG = DBRowTableName.Instance("LOGINLOG");
            public static DBRowTableName MAILLOG = DBRowTableName.Instance("MAILLOG");
            public static DBRowTableName SETUP = DBRowTableName.Instance("SETUP");
            public static DBRowTableName UNIT = DBRowTableName.Instance("UNIT");
            public static DBRowTableName VISIT_RECORD = DBRowTableName.Instance("VISIT_RECORD");
            public static DBRowTableName ZIPCODE = DBRowTableName.Instance("ZIPCODE");
            public static DBRowTableName ZIPCODE6 = DBRowTableName.Instance("ZIPCODE6");

            public static DBRowTableName EFAQ = DBRowTableName.Instance("EFAQ");
            public static DBRowTableName ELINKS = DBRowTableName.Instance("ELINKS");
            public static DBRowTableName ENEWS = DBRowTableName.Instance("ENEWS");
            public static DBRowTableName NEWS = DBRowTableName.Instance("NEWS");

            public static DBRowTableName ESERVICE_STATUS_MAIL = DBRowTableName.Instance("ESERVICE_STATUS_MAIL");
            public static DBRowTableName APPLY = DBRowTableName.Instance("APPLY");

            public static DBRowTableName ESERVICE_TOP_GRP = DBRowTableName.Instance("ESERVICE_TOP_GRP");
            public static DBRowTableName ESERVICE_STATUS = DBRowTableName.Instance("ESERVICE_STATUS");
            public static DBRowTableName ESERVICE_SOLUTION = DBRowTableName.Instance("ESERVICE_SOLUTION");
            public static DBRowTableName ESERVICE_SEND_GRP = DBRowTableName.Instance("ESERVICE_SEND_GRP");
            public static DBRowTableName ESERVICE_PRO_GRP = DBRowTableName.Instance("ESERVICE_PRO_GRP");
            public static DBRowTableName ESERVICE_PAY = DBRowTableName.Instance("ESERVICE_PAY");
            public static DBRowTableName ESERVICE_GET = DBRowTableName.Instance("ESERVICE_GET");
            public static DBRowTableName ESERVICE_FILE_ITEM = DBRowTableName.Instance("ESERVICE_FILE_ITEM");
            public static DBRowTableName ESERVICE = DBRowTableName.Instance("ESERVICE");
            public static DBRowTableName APPLY_001008_BE = DBRowTableName.Instance("APPLY_001008_BE");
            public static DBRowTableName APPLY_001008_AF = DBRowTableName.Instance("APPLY_001008_AF");

            // EEC_
            public static DBRowTableName EEC_Apply = DBRowTableName.Instance("EEC_Apply");
            public static DBRowTableName EEC_ApplyDetail = DBRowTableName.Instance("EEC_ApplyDetail");
            public static DBRowTableName EEC_ApplyDetailPrice = DBRowTableName.Instance("EEC_ApplyDetailPrice");
            public static DBRowTableName EEC_ApplyDetailPrice_ApiData = DBRowTableName.Instance("EEC_ApplyDetailPrice_ApiData");
            public static DBRowTableName EEC_ApplyDetailUploadLog = DBRowTableName.Instance("EEC_ApplyDetailUploadLog");
            public static DBRowTableName EEC_User = DBRowTableName.Instance("EEC_User");
            public static DBRowTableName EEC_UserOperation = DBRowTableName.Instance("EEC_UserOperation");
            public static DBRowTableName EEC_Hospital = DBRowTableName.Instance("EEC_Hospital");
            public static DBRowTableName EEC_Hospital_Api = DBRowTableName.Instance("EEC_Hospital_Api");
            public static DBRowTableName EEC_Hospital_CHANGEPWD_GUID = DBRowTableName.Instance("EEC_Hospital_CHANGEPWD_GUID");
            public static DBRowTableName EEC_Hospital_PWDLOG = DBRowTableName.Instance("EEC_Hospital_PWDLOG");
            public static DBRowTableName EEC_Hospital_SetPrice = DBRowTableName.Instance("EEC_Hospital_SetPrice");
            public static DBRowTableName EEC_NHICardVerify = DBRowTableName.Instance("EEC_NHICardVerify");
            public static DBRowTableName EEC_SPAPIWEB01 = DBRowTableName.Instance("EEC_SPAPIWEB01");
            public static DBRowTableName EEC_CommonType = DBRowTableName.Instance("EEC_CommonType");
            public static DBRowTableName ContactUs = DBRowTableName.Instance("ContactUs");
            // EEC_Hospital 權限
            public static DBRowTableName AMGMAPM_Hosp = DBRowTableName.Instance("AMGMAPM_Hosp");
            public static DBRowTableName AMGRP_Hosp = DBRowTableName.Instance("AMGRP_Hosp");
            public static DBRowTableName AMUROLE_Hosp = DBRowTableName.Instance("AMUROLE_Hosp");

            // EEC_Api
            public static DBRowTableName EEC_Api_User = DBRowTableName.Instance("EEC_Api_User");
            public static DBRowTableName EEC_Api_Token = DBRowTableName.Instance("EEC_Api_Token");
        }
    }
}