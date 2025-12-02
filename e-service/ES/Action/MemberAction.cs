using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Utils;
using ES.Models;
using ES.Services;
using log4net;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using ES.Commons;

namespace ES.Action
{
    public class MemberAction : BaseAction
    {
        /// <summary>
        /// 會員管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public MemberAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 會員管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public MemberAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 新增會員資料 (個人)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(MemberRPModel model)
        {
            string sql = @"INSERT INTO MEMBER (
                            ACC_NO, IDN, SEX_CD, BIRTHDAY, NAME,
                            ENAME, TEL, MAIL, CITY_CD, TOWN_CD, ADDR,
                            PSWD,
                            UPD_TIME, UPD_FUN_CD, ADD_TIME, ADD_FUN_CD
                        ) VALUES (
                            @ACC_NO, @IDN, @SEX_CD, CONVERT(DATETIME, @BIRTHDAY, 111), @NAME,
                            @ENAME, @TEL, @MAIL, @CITY_CD, @TOWN_CD, @ADDR,
                            @PSWD,
                            GETDATE(), @FUN_CD, GETDATE(), @FUN_CD
                        )";

            SqlCommand cmd = new SqlCommand(sql, conn);

            DataUtils.AddParameters(cmd, "ACC_NO", model.Account);
            DataUtils.AddParameters(cmd, "IDN", model.Identity.ToUpper());
            DataUtils.AddParameters(cmd, "SEX_CD", model.SexCode);
            DataUtils.AddParameters(cmd, "BIRTHDAY", model.Birthday);
            DataUtils.AddParameters(cmd, "NAME", model.Name);

            DataUtils.AddParameters(cmd, "ENAME", model.NameEng);
            DataUtils.AddParameters(cmd, "TEL", model.Tel);
            DataUtils.AddParameters(cmd, "MAIL", model.Mail);
            DataUtils.AddParameters(cmd, "CITY_CD", model.CityCode);
            DataUtils.AddParameters(cmd, "TOWN_CD", model.TownCode);
            DataUtils.AddParameters(cmd, "ADDR", model.Address);

            DataUtils.AddParameters(cmd, "PSWD", DataUtils.Crypt256(model.Password));
            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-REG");


            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 新增會員資料 (公司)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(MemberRCModel model)
        {
            string sql = @"INSERT INTO MEMBER (
                            ACC_NO, IDN, NAME, ENAME, MAIL,
                            TEL, FAX, CITY_CD, TOWN_CD, ADDR, EADDR,
                            MEDICO, CHR_NAME, CHR_ENAME, CNT_NAME, CNT_ENAME,
                            CNT_TEL, PSWD,
                            UPD_TIME, UPD_FUN_CD, ADD_TIME, ADD_FUN_CD
                        ) VALUES (
                            @ACC_NO, @IDN, @NAME, @ENAME, @MAIL,
                            @TEL, @FAX, @CITY_CD, @TOWN_CD, @ADDR, @EADDR,
                            @MEDICO, @CHR_NAME, @CHR_ENAME, @CNT_NAME, @CNT_ENAME,
                            @CNT_TEL, @PSWD,
                            GETDATE(), @FUN_CD, GETDATE(), @FUN_CD 
                        )";

            SqlCommand cmd = new SqlCommand(sql, conn);

            DataUtils.AddParameters(cmd, "ACC_NO", model.Account);
            DataUtils.AddParameters(cmd, "IDN", model.Identity.ToUpper());
            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "ENAME", model.NameEng);
            DataUtils.AddParameters(cmd, "MAIL", model.Mail);

            DataUtils.AddParameters(cmd, "TEL", model.Tel);
            DataUtils.AddParameters(cmd, "FAX", model.Fax);
            DataUtils.AddParameters(cmd, "CITY_CD", model.CityCode);
            DataUtils.AddParameters(cmd, "TOWN_CD", model.TownCode);
            DataUtils.AddParameters(cmd, "ADDR", model.Address);
            DataUtils.AddParameters(cmd, "EADDR", model.AddressEng);

            DataUtils.AddParameters(cmd, "MEDICO", model.Medico);
            DataUtils.AddParameters(cmd, "CHR_NAME", model.ChargeName);
            DataUtils.AddParameters(cmd, "CHR_ENAME", model.ChargeNameEng);
            DataUtils.AddParameters(cmd, "CNT_NAME", model.ContactName);
            DataUtils.AddParameters(cmd, "CNT_ENAME", model.ContactNameEng);

            DataUtils.AddParameters(cmd, "CNT_TEL", model.ContactTel);

            DataUtils.AddParameters(cmd, "PSWD", DataUtils.Crypt256(model.Password));
            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-REG");


            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 修改會員資料 (個人)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(MemberPModel model)
        {
            string sql = @"UPDATE MEMBER SET 
                            SEX_CD = @SEX_CD,
                            BIRTHDAY = @BIRTHDAY, 
                            NAME = @NAME,
                            ENAME = @ENAME,
                            TEL = @TEL,
                            MOBILE = @MOBILE,
                            MAIL = @MAIL,
                            MAIL_MK = @MAIL_MK,
                            CITY_CD = @CITY_CD,
                            TOWN_CD = @TOWN_CD,
                            ADDR = @ADDR,
                            PSWD = ISNULL(@PSWD, PSWD),
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @ACC_NO
                        WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);


            DataUtils.AddParameters(cmd, "SEX_CD", model.SexCode);
            DataUtils.AddParameters(cmd, "BIRTHDAY", model.Birthday);
            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "ENAME", model.NameEng);
            DataUtils.AddParameters(cmd, "TEL", model.Tel);
            DataUtils.AddParameters(cmd, "MOBILE", model.Mobile);

            DataUtils.AddParameters(cmd, "MAIL", model.Mail);
            DataUtils.AddParameters(cmd, "MAIL_MK", model.MailMark);
            DataUtils.AddParameters(cmd, "CITY_CD", model.CityCode);
            DataUtils.AddParameters(cmd, "TOWN_CD", model.TownCode);
            DataUtils.AddParameters(cmd, "ADDR", model.Address);

            if (model.Password != null)
            {
                DataUtils.AddParameters(cmd, "PSWD", DataUtils.Crypt256(model.Password));
            }
            else
            {
                DataUtils.AddParameters(cmd, "PSWD", model.Password);
            }

            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-INFO");
            DataUtils.AddParameters(cmd, "ACC_NO", model.Account);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 修改會員資料 (公司)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(MemberCModel model)
        {
            string sql = @"UPDATE MEMBER SET 
                            NAME = @NAME,
                            ENAME = @ENAME,
                            TEL = @TEL,
                            FAX = @FAX,
                            MAIL = @MAIL,
                            MAIL_MK = @MAIL_MK,
                            CITY_CD = @CITY_CD,
                            TOWN_CD = @TOWN_CD,
                            ADDR = @ADDR,
                            EADDR = @EADDR,
                            MEDICO = @MEDICO,
                            CHR_NAME = @CHR_NAME,
                            CHR_ENAME = @CHR_ENAME,
                            CNT_NAME = @CNT_NAME,
                            CNT_ENAME = @CNT_ENAME,
                            CNT_TEL = @CNT_TEL,
                            PSWD = ISNULL(@PSWD, PSWD),
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @ACC_NO
                        WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);


            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "ENAME", model.NameEng);
            DataUtils.AddParameters(cmd, "TEL", model.Tel);
            DataUtils.AddParameters(cmd, "FAX", model.Fax);
            DataUtils.AddParameters(cmd, "MAIL", model.Mail);

            DataUtils.AddParameters(cmd, "MAIL_MK", model.MailMark);
            DataUtils.AddParameters(cmd, "CITY_CD", model.CityCode);
            DataUtils.AddParameters(cmd, "TOWN_CD", model.TownCode);
            DataUtils.AddParameters(cmd, "ADDR", model.Address);
            DataUtils.AddParameters(cmd, "EADDR", model.AddressEng);
            DataUtils.AddParameters(cmd, "MEDICO", model.Medico);

            DataUtils.AddParameters(cmd, "CHR_NAME", model.ChargeName);
            DataUtils.AddParameters(cmd, "CHR_ENAME", model.ChargeNameEng);
            DataUtils.AddParameters(cmd, "CNT_NAME", model.ContactName);
            DataUtils.AddParameters(cmd, "CNT_ENAME", model.ContactNameEng);
            DataUtils.AddParameters(cmd, "CNT_TEL", model.ContactTel);

            if (model.Password != null)
            {
                DataUtils.AddParameters(cmd, "PSWD", DataUtils.Crypt256(model.Password));
            }
            else
            {
                DataUtils.AddParameters(cmd, "PSWD", model.Password);
            }
            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-INFO");
            DataUtils.AddParameters(cmd, "ACC_NO", model.Account);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 修改會員資料 (憑證資訊)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateC(String identity, string cardinfo, string cardtype, string cardidx)
        {
            string sql = @"UPDATE MEMBER SET 
                            CARD_TYPE = @CARD_TYPE,
                            CARD_INFO = @CARD_INFO,
                            CARD_IDX = @CARD_IDX,
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD
                        WHERE IDN = @IDN";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "CARD_INFO", cardinfo);
            DataUtils.AddParameters(cmd, "CARD_TYPE", cardtype);
            DataUtils.AddParameters(cmd, "CARD_IDX", cardidx);
            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-INFO");
            DataUtils.AddParameters(cmd, "IDN", identity);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }


        /// <summary>
        /// 取得會員資料(憑證)
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public MemberModel GetMemberC(string cardinfo, string cardtype)
        {
            MemberModel member = null;

            string sql = @"SELECT ACC_NO, PSWD, IDN, NAME FROM MEMBER WHERE CARD_INFO = @CARD_INFO AND CARD_TYPE = @CARD_TYPE";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("CARD_INFO", cardinfo);
            cmd.Parameters.AddWithValue("CARD_TYPE", cardtype);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int idx = 0;
                    member = new MemberModel();

                    member.Account = DataUtils.GetDBString(dr, idx++);
                    member.Password = DataUtils.GetDBString(dr, idx++);
                    member.Identity = DataUtils.GetDBString(dr, idx++);
                    member.Name = DataUtils.GetDBString(dr, idx++);
                    member.IsPerson = CheckUtils.IsIdentity(member.Identity);
                }
                dr.Close();
            }

            return member;
        }

        /// <summary>
        /// 取得會員資料
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public MemberModel GetMember(string account)
        {

            string sql = @"SELECT ACC_NO, PSWD, IDN, NAME, MAIL FROM MEMBER WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ACC_NO", account);
            MemberModel member = new MemberModel();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int idx = 0;
                    member.Account = DataUtils.GetDBString(dr, idx++);
                    member.Password = DataUtils.GetDBString(dr, idx++);
                    member.Identity = DataUtils.GetDBString(dr, idx++);
                    member.Name = DataUtils.GetDBString(dr, idx++);
                    member.Mail = DataUtils.GetDBString(dr, idx++);
                    member.IsPerson = (member.Identity.Length == 10);
                }
                dr.Close();
            }

            return member;
        }

        /// <summary>
        /// 取得會員資料 (個人)
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public MemberPModel GetMemberP(string account)
        {
            string sql = @"SELECT ACC_NO, IDN, SEX_CD, NAME, ENAME,
                                CNT_NAME, CNT_ENAME, CNT_TEL, CHR_NAME, CHR_ENAME,
                                TEL, FAX, MAIL, CITY_CD, TOWN_CD, ADDR,
                                EADDR, MAIL_MK, BIRTHDAY, MOBILE
                            FROM MEMBER WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ACC_NO", account);
            MemberPModel member = new MemberPModel();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;

                    member.Account = DataUtils.GetDBString(dr, n++);
                    member.Identity = DataUtils.GetDBString(dr, n++);
                    member.SexCode = DataUtils.GetDBString(dr, n++);
                    member.Name = DataUtils.GetDBString(dr, n++);
                    member.NameEng = DataUtils.GetDBString(dr, n++);

                    member.ContactName = DataUtils.GetDBString(dr, n++);
                    member.ContactNameEng = DataUtils.GetDBString(dr, n++);
                    member.ContactTel = DataUtils.GetDBString(dr, n++);
                    member.ChargeName = DataUtils.GetDBString(dr, n++);
                    member.ChargeNameEng = DataUtils.GetDBString(dr, n++);

                    member.Tel = DataUtils.GetDBString(dr, n++);
                    member.Fax = DataUtils.GetDBString(dr, n++);
                    member.Mail = DataUtils.GetDBString(dr, n++);
                    member.CityCode = DataUtils.GetDBString(dr, n++);
                    member.TownCode = DataUtils.GetDBString(dr, n++);

                    member.Address = DataUtils.GetDBString(dr, n++);
                    member.AddressEng = DataUtils.GetDBString(dr, n++);
                    member.MailMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    member.Birthday = DataUtils.GetDBDateTime(dr, n++);

                    member.Mobile = DataUtils.GetDBString(dr, n++);

                    /*
                    if (member.TownCode != null && member.TownCode.Length == 4)
                    {
                        member.CityCode = member.TownCode.Substring(0, 2);
                    }
                    */
                }
                dr.Close();
            }

            return member;
        }

        /// <summary>
        /// 取得會員資料 (公司)
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public MemberCModel GetMemberC(string account)
        {
            string sql = @"
    SELECT ACC_NO, IDN, SEX_CD, NAME, ENAME,
    CNT_NAME, CNT_ENAME, CNT_TEL, CHR_NAME, CHR_ENAME,
    TEL, FAX, MAIL, CITY_CD, TOWN_CD,
    ADDR, EADDR, MEDICO
    FROM MEMBER WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ACC_NO", account);
            MemberCModel member = new MemberCModel();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;

                    member.Account = DataUtils.GetDBString(dr, n++);
                    member.Identity = DataUtils.GetDBString(dr, n++);
                    member.SexCode = DataUtils.GetDBString(dr, n++);
                    member.Name = DataUtils.GetDBString(dr, n++);
                    member.NameEng = DataUtils.GetDBString(dr, n++);

                    member.ContactName = DataUtils.GetDBString(dr, n++);
                    member.ContactNameEng = DataUtils.GetDBString(dr, n++);
                    member.ContactTel = DataUtils.GetDBString(dr, n++);
                    member.ChargeName = DataUtils.GetDBString(dr, n++);
                    member.ChargeNameEng = DataUtils.GetDBString(dr, n++);

                    member.Tel = DataUtils.GetDBString(dr, n++);
                    member.Fax = DataUtils.GetDBString(dr, n++);
                    member.Mail = DataUtils.GetDBString(dr, n++);
                    member.CityCode = DataUtils.GetDBString(dr, n++);
                    member.TownCode = DataUtils.GetDBString(dr, n++);

                    member.Address = DataUtils.GetDBString(dr, n++);
                    member.AddressEng = DataUtils.GetDBString(dr, n++);
                    member.Medico = DataUtils.GetDBString(dr, n++);

                    /*
                    if (member.TownCode != null && member.TownCode.Length == 4)
                    {
                        member.CityCode = member.TownCode.Substring(0, n++);
                    }
                    */

                }
                dr.Close();
            }

            return member;
        }

        /// <summary>
        /// 依照身分證號 / 統一編號撈出帳號
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public String getMemberIDN(string identity)
        {
            string sql = @"SELECT ACC_NO FROM MEMBER WHERE IDN = @IDN";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("IDN", identity);

            return cmd.ExecuteScalar().ToString();
        }

        /// <summary>
        /// 檢查帳號是否存在
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool CheckAccountExists(string account)
        {
            string sql = @"SELECT ACC_NO FROM MEMBER WHERE ACC_NO = @ACC_NO";
            var rst = false;
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ACC_NO", account);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    rst = true;
                }
                dr.Close();
            }

            return rst;
        }

        /// <summary>
        /// 檢查身分證號 / 統一編號是否存在
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public bool CheckIdentityExists(string identity)
        {
            string sql = @"SELECT ACC_NO FROM MEMBER WHERE IDN = @IDN";
            var rst = false;
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("IDN", identity);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    rst = true;
                }
                dr.Close();
            }

            return rst;
        }

        /// <summary>
        /// 忘記密碼會員資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetForgetMember(MemberForgetModel model)
        {
            Dictionary<string, object> data = null;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT ACC_NO, IDN, NAME, CNT_NAME, MAIL
                FROM MEMBER
                WHERE DEL_MK = 'N'
                    AND ACC_NO = @ACC_NO
                    AND IDN = @IDN
                    AND MAIL = @MAIL
            ";

            args.Add("ACC_NO", model.Account);
            args.Add("IDN", model.Identity);
            args.Add("MAIL", model.Mail);

            data = GetData(querySQL, args);

            return data;
        }

        public bool updateLoginStatistics(string loginType)
        {
            //logger.Debug("loginType: " + loginType);

            if (loginType == null)
            {
                return false;
            }

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("MEMBER", loginType.Equals("MEMBER") ? 1 : 0);
            args.Add("MOICA", loginType.Equals("MOICA") ? 1 : 0);
            args.Add("MOEACA", loginType.Equals("MOEACA") ? 1 : 0);
            args.Add("HCA0", loginType.Equals("HCA0") ? 1 : 0);
            args.Add("HCA1", loginType.Equals("HCA1") ? 1 : 0);
            args.Add("NEWEID", loginType.Equals("NEWEID") ? 1 : 0);

            string updateSQL = @"
                if exists (select * from login_statistics where login_date = convert(varchar(10),getdate(),120))
                begin
                    update login_statistics set 
                        member = member + @MEMBER,
                        moica = moica + @MOICA,
                        moeaca = moeaca + @MOEACA,
                        hca0 = hca0 + @HCA0,
                        hca1 = hca1 + @HCA1,
                        NEWEID = NEWEID + @NEWEID
                    where login_date = convert(varchar(10),getdate(),120)
                end
                else
                begin
                    insert into login_statistics (
                        login_date, member, moica, moeaca, hca0, hca1,NEWEID
                    ) values (
                        getdate(), @MEMBER, @MOICA, @MOEACA, @HCA0, @HCA1, @NEWEID
                    )
                end
            ";

            int flag = Update(updateSQL, args);

            logger.Debug(loginType + ": " + flag);

            return ((flag == 1) ? true : false);
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool UpdatePassword(Dictionary<string, object> data)
        {
            string updateSQL = @"
                UPDATE MEMBER SET
                    PSWD = @PSWD,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = ACC_NO
                WHERE ACC_NO = @ACC_NO
            ";

            SqlCommand cmd = new SqlCommand(updateSQL, conn, tran);
            DataUtils.AddParameters(cmd, "PSWD", DataUtils.Crypt256((string)data["NEW_PSWD"]));
            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-FORGET");
            DataUtils.AddParameters(cmd, "ACC_NO", data["ACC_NO"]);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 自然人憑證登入檢核
        /// </summary>
        /// <param name="sCAData"></param>
        /// <param name="sEncCert"></param>
        /// <returns></returns>
        public MemberModel CheckCAData(Dictionary<string, object> args, ref Dictionary<string, object> item)
        {
            string s_errmsg = "";
            // 3-array / 4-array
            string sCAData = args["CAData"] != null ? args["CAData"].ToString() : "";
            string sEncCert = args["EncCert"] != null ? args["EncCert"].ToString() : "";
            string sLoginType = args["loginType"] != null ? args["loginType"].ToString() : "";

            item = new Dictionary<string, object>();
            MemberModel rst = null;
            //string rst = string.Empty;
            SessionModel sm = SessionModel.Get();
            string[] sArrData = Regex.Split(sCAData, "~~", RegexOptions.IgnoreCase);

            string sUTCExpiredDate = "";
            string cardtype = "";
            DateTime? UTCExpDate = null;
            if (sLoginType != "3")
            {
                if (sArrData.Length == 3) { cardtype = ""; }
                else if (sArrData.Length == 4)
                {
                    //"UTCExpiredDate": "Fri Dec 27 13:41:07 2024\n"
                    sUTCExpiredDate = sArrData[3];
                    UTCExpDate = MyCommonUtil.TransUTCDateToDateTime(sUTCExpiredDate);
                    cardtype = "";
                    if (UTCExpDate.HasValue && DateTime.Compare(DateTime.Today, UTCExpDate.Value) > 0)
                    {
                        s_errmsg = "憑證已過期";
                        sm.LastErrorMessage = s_errmsg;
                        logger.Warn(string.Format("{0},{1}", sCAData, s_errmsg));
                        return rst;
                    }
                }
                else
                {
                    s_errmsg = "憑證資料有誤!!";
                    sm.LastErrorMessage = s_errmsg;
                    logger.Warn(string.Format("{0},{1}", sCAData, s_errmsg));
                    return rst;
                }
            }
            string sName = sArrData[0];//sName
            string sCardId = sArrData[1];//CardId
            string sLastIDNO = sArrData[2];//LatIDNO4
            string s_infomsg = string.Format("##CheckCAData Name={0}, CardId={1}, LatIDNO4={2}", sName, sCardId, sLastIDNO);
            logger.Info(s_infomsg);
            if (sLoginType.Equals("1") && sLastIDNO.Length == 4) { cardtype = "MOICA"; } /*自然人憑證*/
            if (sLoginType.Equals("2") && sLastIDNO.Length == 8) { cardtype = "MOEACA"; } /*工商憑證*/
            if (sLoginType.Equals("3") && sLastIDNO.Length == 10) { cardtype = "HCA1"; } /*醫事憑證-人員*/
            if (sLoginType.Equals("4")) { cardtype = "NEWEID"; } /*NEWEID-人員*/
            logger.Info(string.Format("cardtype:{0},{1}", sLoginType, cardtype));

            if (string.IsNullOrEmpty(cardtype) || cardtype.Length < 1)
            {
                s_errmsg = "憑證類別選擇有誤!!";
                sm.LastErrorMessage = s_errmsg;
                logger.Warn(string.Format("{0},{1}", s_infomsg, s_errmsg));
                return rst;
            }

            X509CertsHelper x509Helper = new X509CertsHelper(sEncCert);
            logger.Info(string.Format("##CheckCAData {0}", x509Helper.ToString()));
            if (x509Helper.IsExpired)
            {
                s_errmsg = "憑證已過期!!";
                sm.LastErrorMessage = s_errmsg;
                logger.Warn(string.Format("{0},{1}", s_infomsg, s_errmsg));
                return rst;
            }
            if (x509Helper.IsCRL)
            {
                s_errmsg = "憑證已廢止(無效)";
                sm.LastErrorMessage = s_errmsg;
                logger.Warn(string.Format("{0},{1}", s_infomsg, s_errmsg));
                return rst;
            }
            if (string.IsNullOrEmpty(sCardId) && string.IsNullOrEmpty(x509Helper.CardID))
            {
                s_errmsg = "憑證簽發卡號資訊有誤";
                sm.LastErrorMessage = s_errmsg;
                logger.Warn(string.Format("{0},{1}", s_infomsg, s_errmsg));
                return rst;
            }
            if (sLoginType != "3")
            {
                if (!string.IsNullOrEmpty(x509Helper.CardID) && !string.IsNullOrEmpty(sCardId) && !x509Helper.CardID.Equals(sCardId))
                {
                    s_errmsg = "憑證登入序號不一致";
                    sm.LastErrorMessage = s_errmsg;
                    logger.Warn(string.Format("{0},{1}", s_infomsg, s_errmsg));
                    return rst;
                }
            }
               

            item.Add("Subject", x509Helper.Subject);
            item.Add("Name", x509Helper.Name ?? sName);
            item.Add("SerialNumber", x509Helper.SerialNumber);
            item.Add("CardID", x509Helper.CardID ?? sCardId);
            //if (!txtIDNO.EndsWith(sLastIDNO)) { sm.LastErrorMessage = "憑證簽發對象身分證號不符合"; return rst; }
            rst = CheckCAUser(sLastIDNO, x509Helper.SerialNumber, cardtype);
            //if (rst == null)
            //{
            //    rst = new MemberModel();
            //    rst.x509 = x509Helper;
            //}
            return rst;
        }

        /// <summary>
        /// 取得會員資料
        /// </summary>
        /// <param name="sLastIDNO"></param>
        /// <param name="SerialNumber"></param>
        /// <param name="cardtype"></param>
        /// <returns></returns>
        public MemberModel CheckCAUser(string sLastIDNO, string SerialNumber, string cardtype)
        {

            //string s_cardtype = "MOICA";
            if (!string.IsNullOrEmpty(SerialNumber)) { SerialNumber = SerialNumber.Trim(); }
            string sql = @"
    SELECT ACC_NO, IDN, SEX_CD
    ,NAME, ENAME
    ,CNT_NAME, CNT_ENAME
    ,CNT_TEL, CHR_NAME, CHR_ENAME
    ,TEL, FAX, MAIL, CITY_CD, TOWN_CD
    ,ADDR ,EADDR, MAIL_MK, BIRTHDAY, MOBILE
    /*,CARD_TYPE,CARD_INFO,SERIALNO ,LAST_LOGINDATE*/
    FROM MEMBER ";
            MemberModel member = null;
            if (cardtype == "HCA1")
            {
                sql += @"
    WHERE IDN = @IDN ";
            }
            else
            {
                sql += @"
    WHERE SERIALNO = @SERIALNO AND CARD_TYPE = @CARD_TYPE";

            }
            SqlCommand cmd = new SqlCommand(sql, conn);
            if (cardtype == "HCA1")
            {
                cmd.Parameters.AddWithValue("IDN", sLastIDNO);
            }
            else
            {
                cmd.Parameters.AddWithValue("SERIALNO", SerialNumber);
                cmd.Parameters.AddWithValue("CARD_TYPE", cardtype);
                //cmd.Parameters.AddWithValue("CARD_INFO", cardinfo);
                //logger.Debug(string.Format("#CheckCAUser.SERIALNO:{0}", SerialNumber));
                //logger.Debug(string.Format("#CheckCAUser.CARD_TYPE:{0}", cardtype));

            }




            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    member = new MemberModel();

                    member.Account = DataUtils.GetDBString(dr, n++); //ACC_NO
                    member.Identity = DataUtils.GetDBString(dr, n++);
                    member.SexCode = DataUtils.GetDBString(dr, n++);
                    member.Name = DataUtils.GetDBString(dr, n++);
                    member.NameEng = DataUtils.GetDBString(dr, n++);

                    //連絡人
                    member.ContactName = DataUtils.GetDBString(dr, n++);
                    member.ContactNameEng = DataUtils.GetDBString(dr, n++);
                    member.ContactTel = DataUtils.GetDBString(dr, n++);
                    member.ChargeName = DataUtils.GetDBString(dr, n++);
                    member.ChargeNameEng = DataUtils.GetDBString(dr, n++);

                    member.Tel = DataUtils.GetDBString(dr, n++);
                    member.Fax = DataUtils.GetDBString(dr, n++);
                    member.Mail = DataUtils.GetDBString(dr, n++);
                    member.CityCode = DataUtils.GetDBString(dr, n++);
                    member.TownCode = DataUtils.GetDBString(dr, n++);

                    member.Address = DataUtils.GetDBString(dr, n++);
                    member.AddressEng = DataUtils.GetDBString(dr, n++);
                    member.MailMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    member.Birthday = DataUtils.GetDBDateTime(dr, n++);
                    member.Mobile = DataUtils.GetDBString(dr, n++);

                    member.IsPerson = CheckUtils.IsIdentity(member.Identity);

                    //return member;
                }
                dr.Close();
            }

            return member;

        }
    }
}