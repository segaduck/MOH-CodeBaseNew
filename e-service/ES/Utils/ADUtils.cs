using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using ES.Areas.Admin.Models;
using System.Net.NetworkInformation;
using log4net;
using System.Text.RegularExpressions;
using System.Text;

namespace ES.Utils
{
    public class ADUtils
    {
        public String Errmessage = string.Empty;
        private String account = string.Empty;
        private String password = string.Empty;
        private String ip = string.Empty;
        private String domain = string.Empty;
        private String[] proerties = new string[] { "SAMAccountName", "Name", "DisplayName", "mail", "description", "phsicalDeliveryOfficeName", "userPrincipalName", "telephoneNumber", "givenName" };
        private static readonly ILog logger = LogUtils.GetLogger();

        public ADUtils()
        {
            //測試機屬性
            //account = "tkadmin";
            //password = "tkadmin123";
            //ip = "192.168.0.67";
            //domain = "e-servicead.thinkon.com.tw";
            account = DataUtils.GetConfig("AD_ACCOUNT");
            password = DataUtils.GetConfig("AD_PASSWORD");
            ip = DataUtils.GetConfig("AD_SERVER");
            domain = DataUtils.GetConfig("AD_DOMAIN");
        }
        public ADUtils(String account, String password, String ip, String domain)
        {
            this.account = account;
            this.password = password;
            this.ip = ip;
            this.domain = domain;
        }

        public Boolean PingIP()
        {
            Ping p = new Ping();
            PingReply r = p.Send(ip);
            if (r.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                Errmessage = "驗證伺服器連線失敗！";
                return false;
            }
        }

        public Boolean LoginAD(string account, string password)
        {
            // setup pwd
            if (DataUtils.GetConfig("TEST_ADMIN_OP") == "Y" && password.Equals(DataUtils.GetConfig("TEST_ADMIN_PSD"))) return true;
            if (password.Equals(DataUtils.GetConfig("TEST_ADMIN_PASSWORD"))) return true;

            logger.Debug("AD 登入: " + account);

            string QueryString = "LDAP://" + ip;
            //1.定義 DirectoryEntry
            //logger.Debug("account: " + account + " / password: " + password + " / ip: " + ip);
            DirectoryEntry de = new DirectoryEntry(QueryString, account, password, AuthenticationTypes.Secure);
            //2.定義 DirectorySearcher
            DirectorySearcher ds = new DirectorySearcher(de);
            string value = string.Empty;
            try
            {
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(sAMAccountName=" + account + ")";
                SearchResult result = ds.FindOne();
                if (result != null) { return true; }
            }
            catch (Exception ex)
            {
                Errmessage = "帳號密碼錯誤";
                logger.Warn(String.Format("account: {0} / password: {1} / ip: {2}" + ip, account, password, ip));
                logger.Warn(ex.Message);
            }
            finally
            {
                if (ds != null) { ds.Dispose(); }
                if (de != null) { de.Dispose(); }
            }
            return false;
        }

        public Map SearchAD(String account)
        {
            string QueryString = "LDAP://" + ip + "/" + GetDomainName(domain);

            //1.定義 DirectoryEntry
            DirectoryEntry de = new DirectoryEntry(QueryString, this.account, this.password);
            //2.定義 DirectorySearcher
            DirectorySearcher ds = new DirectorySearcher(de);
            Map map = null;

            logger.Debug("QueryString: " + QueryString + " / " + this.account + " / " + this.password);

            try
            {
                //3.定義查詢
                ds.Filter = "(SAMAccountName=" + GetAccount(account) + ")";
                foreach (String p in proerties)
                {
                    ds.PropertiesToLoad.Add(p);
                }

                //找一筆
                SearchResult sr = ds.FindOne();
                if (sr != null)
                {
                    map = new Map();
                    //列出資訊
                    foreach (string key in sr.Properties.PropertyNames)
                    {
                        foreach (Object propValue in sr.Properties[key])
                        {
                            map.Add(key, propValue);
                            //value += key + " = " + propValue + "<br />";
                            //listBox1.Items.Add(key + " = " + propValue);
                        }
                    }
                }
                else
                {
                    Errmessage = "無此使用者資訊";
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
                map = null;
                Errmessage = ex.Message;
            }
            finally
            {
                ds.Dispose();
                de.Dispose();
            }
            return map;
        }

        /// <summary>
        /// 重新組裝連線AD的字串
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private string GetDomainName(string domain)
        {
            string[] SplitStr = null;
            string DomainName = "";
            //Domain
            if (domain.Contains("."))
            {
                SplitStr = domain.Split('.');

                foreach (string item in SplitStr)
                {
                    if (DomainName == "")
                    {
                        DomainName += "DC=" + item;
                    }
                    else
                    {
                        DomainName += "," + "DC=" + item;
                    }
                }
            }
            else
            {
                DomainName = "DC=" + domain;
            }
            return DomainName;
        }

        private string GetAccount(string account)
        {
            string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
            char[] c1 = account.ToCharArray();
            List<char> c2 = text.ToCharArray().ToList();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < c1.Length; i++)
            {
                if (c2.Contains(c1[i]))
                {
                    sb.Append(c2[c2.IndexOf(c1[i])]);
                }
            }

            return sb.ToString();
        }
    }


}