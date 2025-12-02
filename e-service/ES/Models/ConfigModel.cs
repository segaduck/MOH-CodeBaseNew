using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace ES.Models
{
    // 這個檔案集中定義系統組態設定變數(舊系統 DefineVar 有用到的值也納入這裡)
    public class ConfigModel
    {
        /// <summary>
        /// 系統預設最高管理帳號
        /// </summary>
        public const string Admin = "superadmin";

        /// <summary>
        /// 預設分頁筆數(常數定義), 如果 web.config 中有設定 DefaultPageSize 則為以 web.config 中設定的為主
        /// </summary>
        private const int _DefaultPageSize = 20;


        /// <summary>
        /// 是否啟用壓力測試模式(AppSettings StressTestMode=Y), 
        /// 若啟用則 LoginRequired 會以設定的StressTestUserInfo
        /// 測試帳號覆寫 LoginUserInfo
        /// </summary>
        public static bool StressTestMode
        {
            get
            {
                string strTestModel = ConfigurationManager.AppSettings["StressTestMode"];
                bool testModel = "Y".Equals(strTestModel);
                return testModel;
            }
        }


        /// <summary>
        /// 主機環境角色設定: 1.內網環境, 2.外網環境
        /// </summary>
        public static string NetID
        {
            get
            {
                string netId = ConfigurationManager.AppSettings["NetID"];
                if (string.IsNullOrEmpty(netId))
                {
                    netId = "1";
                }
                return netId;
            }
        }

        /// <summary>
        /// 預設分頁筆數
        /// </summary>
        public static int DefaultPageSize
        {
            get
            {
                int iPageSize;
                string pageSize = ConfigurationManager.AppSettings["DefaultPageSize"];
                if (int.TryParse(pageSize, out iPageSize))
                {
                    return iPageSize;
                }
                else
                {
                    return _DefaultPageSize;
                }
            }
        }           

        /// <summary>
        /// 系統電子郵件服務主機 IP
        /// </summary>
        public static string MailServer
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailServer"];
                return (string.IsNullOrEmpty(value)) ? "127.0.0.1" : value;
            }
        }

        public static string MailServerPort
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailServerPort"];
                return (string.IsNullOrEmpty(value)) ? "" : value;
            }
        }

        public static string MailEnableSsl
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailEnableSsl"];
                return (string.IsNullOrEmpty(value)) ? "" : value;
            }
        }

        /// <summary>
        /// 系統寄件者電子郵件地址
        /// </summary>
        public static string MailSenderAddr
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailSenderAddr"];
                return (string.IsNullOrEmpty(value)) ? "預設信箱" : value;
            }
        }

        /// <summary>
        /// 系統寄件者電子郵件使用者帳號
        /// </summary>
        public static string MailSenderAcc
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailSenderAcc"];
                return (string.IsNullOrEmpty(value)) ? "service" : value;
            }
        }

        /// <summary>
        /// 系統寄件者電子郵件密碼
        /// </summary>
        public static string MailSenderPwd
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailSenderPWD"];
                return (string.IsNullOrEmpty(value)) ? "service!QAZ2wsx" : value;
            }
        }

        /// <summary>
        /// SFTP-CanUse 可以使用Y/N/""
        /// </summary>
        public static string FtpNasCanUse
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FtpNasCanUse"];
                return (string.IsNullOrEmpty(value)) ? "" : value;
            }
        }

        /// <summary>
        /// SFTP-FtpNasServer
        /// </summary>
        public static string FtpNasServer
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FtpNasServer"];
                return (string.IsNullOrEmpty(value)) ? "" : value;
            }
        }

        /// <summary>
        /// SFTP-User
        /// </summary>
        public static string FtpNasUser
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FtpNasUser"];
                return (string.IsNullOrEmpty(value)) ? "labor" : value;
            }
        }

        /// <summary>
        /// SFTP-密碼
        /// </summary>
        public static string FtpNasPassword
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FtpNasPassword"];
                return (string.IsNullOrEmpty(value)) ? "labor22595700" : value;
            }
        }

        /// <summary>
        /// SFTP-Path
        /// </summary>
        public static string FtpNasPath
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FtpNasPath"];
                return (string.IsNullOrEmpty(value)) ? "" : value;
            }
        }

        /// <summary>
        /// 系統名稱(忘記密碼通知信會用)
        /// </summary>
        public static string SYSNAME
        {
            get
            {
                return "衛服部";
            }
        }

        /// <summary>
        /// 系統收件者電子郵件地址(測試收信信箱)是否啟用
        /// </summary>
        public static string MailRevTest
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailRevTest"];
                return (string.IsNullOrEmpty(value)) ? "0" : value;
            }
        }
        /// <summary>
        /// 系統收件者電子郵件地址(測試收信信箱)是否啟用
        /// </summary>
        public static string MailRevIsTwo
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailRevIsTwo"];
                return (string.IsNullOrEmpty(value)) ? "0" : value;
            }
        }
        /// <summary>
        /// 系統收件者電子郵件地址(測試收信信箱)
        /// </summary>
        public static string MailRevAddr1
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailRevAddr1"];
                return (string.IsNullOrEmpty(value)) ? "預設信箱" : value;
            }
        }
        /// <summary>
        /// 系統收件者電子郵件地址(測試收信信箱)
        /// </summary>
        public static string MailRevAddr2
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MailRevAddr2"];
                return (string.IsNullOrEmpty(value)) ? "預設信箱" : value;
            }
        }

        /// <summary>
        /// WebTestEnvir 測試環境
        /// </summary>
        public static string WebTestEnvir
        {
            get
            {
                string value = ConfigurationManager.AppSettings["WebTestEnvir"];
                return value ?? "N";
            }
        }

        /// <summary>
        /// 案件通知轉址主機位址
        /// </summary>
        public static string NoticeMailUrl
        {
            get
            {
                string value = ConfigurationManager.AppSettings["NoticeMailUrl"];
                return (string.IsNullOrEmpty(value)) ? "" : value;
            }
        }

        /// <summary>
        /// 防疫旅館FlyPayBasicData驗證碼
        /// </summary>
        public static string FlyPayBasicVCode
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FlyPayBasicVCode"];
                return (string.IsNullOrEmpty(value)) ? "" : value;
            }
        }
    }
}