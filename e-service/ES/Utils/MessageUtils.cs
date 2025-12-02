using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Utils
{
    public class MessageUtils
    {
        #region 最新消息通知 郵件範例 NEWS
        public static string MAIL_NEWS_SUBJECT = "{0}";
        public static string MAIL_NEWS_BODY = @"
            {0}<br><br>
            衛生福利部{1}敬上<br><br>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。
        ";
        #endregion

        #region 分文通知 郵件範例 Dispatch
        public static string MAIL_Dispatch_SUBJECT = "承辦通知";
        public static string MAIL_Dispatch_BODY =  @"
            {0}您好:<br><br>
	        會員({1})於{2}申請一案件-{3}-, 已經由{4}分文指派給您.<br>
	        利用本信, 以咨通知!<br><br>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。
        ";
        #endregion

        #region 進度通知 郵件範例 ApplyQUES
        public static string MAIL_ApplyQUES_SUBJECT = "衛生福利部{0}進度通知";
        public static string MAIL_ApplyQUES_BODY = @"
            '{0}'，您好:<br><br>
	        您於民國{1}年{2}月{3}日申辦之{4}案件<br>
	        申請編號：<a href='{5}/History/Show/{6}'>{7}</a>現在進度為'{8}'<br>
            {9}<br>
	        特此通知。感謝您使用衛生福利部線上申辦系統<br><br>
            請至<a href='{10}/QuestWeb/Index/{11}'>滿意度問卷</a>填寫滿意度調查表,您的寶貴意見將做為本部改進事項<br><br>
	        衛生福利部{12}敬上<br>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。
        ";
        #endregion

        #region 進度通知 郵件範例 ApplyNotice
        public static string MAIL_ApplyNotice_SUBJECT = "衛生福利部{0}進度通知";
        public static string MAIL_ApplyNotice_BODY = @"
            '{0}'，您好:<br><br>
	        您於民國{1}年{2}月{3}日申辦之{4}案件<br>
	        申請編號：<a href='{5}/History/Show/{6}'>{7}</a>現在進度為'{8}'<br>
            補件內容：{9}<br><br>
	        特此通知。感謝您使用衛生福利部線上申辦系統<br><br>
	        衛生福利部{10}敬上<br>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>
            ※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>
            115209 臺北市南港區昆陽街161-2號<br><br>
            ※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>
            115204 臺北市南港區忠孝東路六段488號

        ";
        #endregion

        #region 進度通知 郵件範例 ApplyNoticeClose
        public static string MAIL_ApplyNoticeClose_SUBJECT = "衛生福利部{0}進度通知";
        public static string MAIL_ApplyNoticeClose_BODY = @"
            '{0}'，您好:<br><br>
	        您於民國{1}年{2}月{3}日申辦之{4}案件<br>
	        申請編號：<a href='{5}/History/Show/{6}'>{7}</a>現在進度為'{8}'<br>
            通知備註：{9}<br><br>
	        特此通知。感謝您使用衛生福利部線上申辦系統<br><br>
	        衛生福利部{10}敬上<br>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>
            ※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>
            115209 臺北市南港區昆陽街161-2號<br><br>
            ※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>
            115204 臺北市南港區忠孝東路六段488號
        ";
        #endregion

        #region 進度通知 郵件範例 ApplyNotification
        public static string MAIL_ApplyNotification_SUBJECT = "衛生福利部{0}進度通知";
        public static string MAIL_ApplyNotification_BODY = @"
            '{0}'，您好:<br><br>
	        您於民國{1}年{2}月{3}日申辦之{4}案件<br>
	        申請編號：<a href='{5}/History/Show/{6}'>{7}</a>現在進度為'{8}'<br>
	        特此通知。感謝您使用衛生福利部線上申辦系統<br><br>
	        衛生福利部{9}敬上<br>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>
            ※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>
            115209 臺北市南港區昆陽街161-2號<br><br>
            ※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>
            115204 臺北市南港區忠孝東路六段488號
        ";
        #endregion

        #region 繳費方式 郵件範例 MaintainView
        public static string MAIL_MaintainView_SUBJECT = "繳費通知";
        public static string MAIL_MaintainView_BODY = @"
            衛生福利部{0}處理通知'{1}'，您好:<br><br>
	        您於民國{2}年{3}月{4}日申辦之{5}案件<br>
	        申請編號：<a href='{6}/History/Show/{7}'>{8}</a>本部已經收到您寄來的繳費單據<br>
	        特此通知。感謝您使用衛生福利部線上申辦系統<br><br> 
	        衛生福利部{9}敬上
        ";
        #endregion

        #region 忘記密碼 郵件範例
        /// <summary>
        /// 忘記密碼信件標題
        /// </summary>
        public static string MAIL_FORGET_SUBJECT = "會員新密碼通知";
        /// <summary>
        /// 忘記密碼信件內容
        /// </summary>
        public static string MAIL_FORGET_BODY = @"
            {0} 會員您好：<br/><br/>
            此封信為您於衛生福利部線上申辦系統之取得新密碼回覆函。<br/><br/>
            您的帳號：{1}<br/>
            您的密碼：{2}<br/><br/>
            若您想變更密碼，請至<a href=""{3}"">衛生福利部線上申辦系統</a>「會員專區」裡的會員基本資料進行「變更密碼」修改作業。<br/><br/>
            提醒您，您的會員資料非常重要，請您務必妥善保管並牢記，以免外洩。<br/><br/>
            衛生福利部敬上<br/><br/>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。
        ";
        #endregion

        #region 新案申請通知 郵件範例
        public static string MAIL_NEWCASE_SUBJECT = "新案申請通知";
        public static string MAIL_NEWCASE_BODY_1 = @"
            您好：<br/>
            會員""{0}""於民國{1}年{2}月{3}日申請""{4}"", 申請編號為：""{5}""<br/><br/>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。
        ";
        public static string MAIL_NEWCASE_BODY_2 = @"
            您好：<br/>
            會員""{0}""於民國{1}年{2}月{3}日申請""{4}"", 申請編號為：""{5}""<br/><br/>
            PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。
        ";
        #endregion

        #region 滿意度調查
        public static string MAIL_SURVEY_SUBJECT = "衛生福利部網路申辦案件滿意度調查";
        public static string MAIL_SURVEY_BODY = @"
            申請者Email：{0}<br/><br/>
            申請案號：{1}<br/><br/>
            1.承辦人服務態度親切：{2}<br/>
            2.承辦人了解業務，且能詳細說明：{3}<br/>
            3.承辦機關處理時間尚屬合理，沒有拖延：{4}<br/>
            4.承辦機關處理作業流程規劃合宜，沒有過於繁複：{5}<br/>
            5.若有資料不足情形，承辦機關能一次告知補正：{6}<br/>
            <br/>
            具體建議：<br/>{7}
        ";
        #endregion

        #region 新案申請通知中醫藥師
        public static string MAIL_ToADMIN_NEWCASE_SUBJECT = "新案申請通知";
        public static string MAIL_ToADMIN_NEWCASE_BODY_1 = @"
            您好：<br/>
            會員""{0}""於民國{1}年{2}月{3}日申請""{4}"", 申請編號為：""{5}""<br/><br/>
            PS.可逕至後台匯出電子公文所需相關檔案【案件分文及處理】-> 【中醫藥司公文匯出】。<br/>
               或點選直接該連結<a href='{6}' target=_blank>【公文匯出】</a>直接匯出公文檔案。
        ";
        #endregion

        #region 公文傳送通知中醫藥師
        public static string MAIL_ToADMIN_SEND_DOCUMENTFILE_SUBJECT = "公文介接傳送檔案通知";
        public static string MAIL_ToADMIN_SEND_DOCUMENTFILE_BODY_1 = @"
            您好：<br/>
            檔案傳送成功：一共有【""{0}""】件申請案件，傳送至公文系統共有【""{1}""】個檔案。
        ";
        #endregion

        public static string MAIL_REUPD_NOTICE_SUBJECT = "衛生福利部網路申辦案件補件通知";
        public static string MAIL_REUPD_NOTICE_BODY = @"
            申請案號：{0}<br/>
            已完成補件。
        ";
    }
}