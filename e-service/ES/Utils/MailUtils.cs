using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Net.Mail;
using System.Web.Configuration;
using System.Net;
using System.Data.SqlClient;
using System.Threading;
using ES.Utils;
using ES.Models;
using ES.Services;

namespace ES.Utils
{
    public class MailUtils
    {
        private static readonly ILog logger = LogUtils.GetLogger();

        /// <summary>
        /// 寄送信件
        /// </summary>
        /// <param name="sendTo">收件者</param>
        /// <param name="subject">信件標題</param>
        /// <param name="body">信件內容</param>
        /// <returns></returns>
        public static bool SendMail(string sendTo, string subject, string body)
        {
            //logger.Debug("sendTo: " + sendTo);
            //logger.Debug("subject: " + subject);
            //logger.Debug("body: " + body);

            return SendMail(sendTo, subject, body, null, false);
        }

        public static bool SendMail(string sendTo, string subject, string body, bool isThread)
        {
            return SendMail(sendTo, subject, body, null, isThread);
        }

        public static bool SendMail(string sendTo, string subject, string body, string serviceId)
        {
            return SendMail(sendTo, subject, body, serviceId, false);
        }

        /// <summary>
        /// 寄送信件
        /// </summary>
        /// <param name="sendTo">收件者</param>
        /// <param name="subject">信件標題</param>
        /// <param name="body">信件內容</param>
        /// <param name="serviceId">案件編號</param>
        /// <returns></returns>
        public static bool SendMail(string sendTo, string subject, string body, string serviceId, bool isThread)
        {
            try
            {
                Mail mail = new Mail(sendTo, subject, body, serviceId);

                if (isThread || sendTo.Split(';').Count() > 3)
                {
                    Thread t = new Thread(new ThreadStart(mail.Start));
                    t.Start();
                }
                else
                {
                    mail.Start();
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                return false;
            }

            return true;
        }

        public static bool SendMailNoLog(string sendTo, string subject, string body)
        {
            try
            {
                Mail mail = new Mail(sendTo, subject, body, null);
                mail.SendMail();
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                return false;
            }

            return true;
        }
    }

    class Mail
    {
        private static readonly ILog logger = LogUtils.GetLogger();
        private static int count = 0;

        private string sendTo;
        private string subject;
        private string body;
        private string serviceId;

        public Mail(string sendTo, string subject, string body, string serviceId)
        {
            this.sendTo = sendTo;
            this.subject = subject;
            this.body = body;
            this.serviceId = serviceId;
            count++;
        }

        public void Start()
        {
            String resultMark = SendMail() ? "Y" : "N";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();

                string insertSQL = @"
                    INSERT INTO MAIL_LOG (
                        SUBJECT, BODY, SEND_TIME, MAIL, RESULT_MK, SRV_ID
                    ) VALUES (
                        @SUBJECT, @BODY, GETDATE(), @MAIL, @RESULT_MK, @SRV_ID
                    )
                ";

                SqlCommand cmd = new SqlCommand(insertSQL, conn);
                DataUtils.AddParameters(cmd, "SUBJECT", subject);
                DataUtils.AddParameters(cmd, "BODY", body.Trim());
                DataUtils.AddParameters(cmd, "MAIL", sendTo);
                DataUtils.AddParameters(cmd, "RESULT_MK", resultMark);
                DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

                int flag = cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 發送信件
        /// </summary>
        /// <returns></returns>
        public bool SendMail()
        {
            try
            {
                var isTest = ConfigModel.MailRevTest ?? "";
                string s_MailServerPort = ConfigModel.MailServerPort ?? "";
                string s_EnableSsl = ConfigModel.MailEnableSsl ?? "";

                var mailFrom = isTest == "1" ? ConfigModel.MailSenderAddr : DataUtils.GetConfig("MAIL_FROM");
                var mailServer = isTest == "1" ? ConfigModel.MailServer : DataUtils.GetConfig("MAIL_SERVER");
                var mailAcc = isTest == "1" ? ConfigModel.MailSenderAcc : DataUtils.GetConfig("MAIL_ACCOUNT");
                var mailPws = isTest == "1" ? ConfigModel.MailSenderPwd : DataUtils.GetConfig("MAIL_PASSWORD");
                var mailPort = isTest == "1" ? s_MailServerPort : DataUtils.GetConfig("MAIL_SERVER_PORT");
                var mailSSL = isTest == "1" ? s_EnableSsl : DataUtils.GetConfig("MAIL_ENABLESSL");

                int now = count;

                //logger.Debug("[" + now + "] sendTo: " + sendTo);
                //logger.Debug("[" + now + "] subject: " + subject);
                //logger.Debug("[" + now + "] body: " + body);

                string[] mails = sendTo.Split(';');

                // 寄件人
                MailAddress from = new MailAddress(mailFrom);
                MailMessage mail = new MailMessage();
                mail.From = from;
                // Webconfig 測試mail
                if (isTest == "1")
                {
                    mail.To.Add(new MailAddress(ConfigModel.MailRevAddr1));
                    if (ConfigModel.MailRevIsTwo == "1")
                    {
                        var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                        foreach (var rec in recList)
                        {
                            mail.To.Add(new MailAddress(rec));
                        }
                    }
                }
                else
                {
                    foreach (string mailto in mails)
                    {
                        if (!String.IsNullOrEmpty(mailto))
                        {
                            if (DataUtils.GetConfig("TEST_ALLOW_MAIL_DOMAIN").Equals("") || mailto.ToLower().EndsWith(DataUtils.GetConfig("TEST_ALLOW_MAIL_DOMAIN")))
                            {
                                mail.To.Add(new MailAddress(mailto));
                            }
                            //mail.To.Add(new MailAddress("jay@thinkon.com.tw"));
                            //mail.To.Add(new MailAddress(mailto));
                        }
                    }
                }


                if (mail.To.Count == 0) return true;

                // 寄件人
                mail.From = from;

                // 信件主旨
                mail.Subject = subject;

                // 信件內容
                mail.Body = body;

                // 是否採用HTML
                mail.IsBodyHtml = true;

                // logger.Debug("Mail: " + DataUtils.GetConfig("MAIL_SERVER") + " / " + DataUtils.GetConfig("MAIL_ACCOUNT") + " / " + DataUtils.GetConfig("MAIL_PASSWORD"));

                SmtpClient client = new SmtpClient(mailServer);
                client.Credentials = new NetworkCredential(mailAcc, mailPws);
                client.Timeout = (1000 * 60); // 毫秒

                if (!string.IsNullOrEmpty(mailPort)) { client.Port = Convert.ToInt32(mailPort); }
                if (mailSSL.Equals("Y")) { client.EnableSsl = true; }
                //logger.Debug("[" + now + "] SendMail Start");
                client.Send(mail);
                //logger.Debug("[" + now + "] SendMail End");

                return true;
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }

            return false;
        }
    }
}