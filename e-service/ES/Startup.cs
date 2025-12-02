using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using System.Web.Hosting;
using Hangfire.MemoryStorage;
using Hangfire.Annotations;
using System.Collections.Generic;
using Hangfire.Dashboard;
using System.Linq;
using ES.Controllers;
using ES.Utils;
using ES.Areas.Admin.Controllers;
using Hangfire.Server;
using Hangfire.SqlServer;

[assembly: OwinStartup(typeof(ES.Startup))]

namespace ES
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            var options = new BackgroundJobServerOptions
            {
                ServerCheckInterval = TimeSpan.FromMinutes(20),
                HeartbeatInterval= TimeSpan.FromMinutes(20),
                CancellationCheckInterval= TimeSpan.FromMinutes(20),
            };
            app.UseHangfireServer(options);
            //  app.UseHangfireDashboard();
            app.UseHangfireDashboard("/EserviceHangFire",
                new DashboardOptions
                {
                    Authorization = new[] { new DashboardAccessAuthFilter() },
                });

            MdodController Mdod = new MdodController();
            // 醫審系統 案件上傳至Sftp 每小時執行 
            RecurringJob.AddOrUpdate(() => Mdod.CreateXML(), "0 * * * *", TimeZoneInfo.Local);
            ESAPIController Esc = new ESAPIController();
            // 我的E政府上稿 API管理案件 每2小時執行
            RecurringJob.AddOrUpdate(() => Esc.ScheService(), "0 21 * * *", TimeZoneInfo.Local);
            // 防疫旅館 菲律賓專案
            //FlyPaySftpController FlyPay = new FlyPaySftpController();
            //RecurringJob.AddOrUpdate(() => FlyPay.Run(), "0 4,6,8,10,12,14,16,18,20,22 * * *", TimeZoneInfo.Local);
            // 中醫藥司(三科)，3天未補件，發送EMIAL通知承辦人此功能。
            CaseController CaseC = new CaseController();
            RecurringJob.AddOrUpdate(() => CaseC.SendMailOverdue(), "0 1 * * *", TimeZoneInfo.Local);
            // 玉山銀行 單筆查詢 交易結果
            //FlySwipeController SwipeC = new FlySwipeController();
            //RecurringJob.AddOrUpdate(() => SwipeC.ConditionSchedual(), "0,10,20,30,40,50 * * * *", TimeZoneInfo.Local);
            // 防疫旅館 春節專案 釋放名額
            //RecurringJob.AddOrUpdate(() => SwipeC.ApplySPRProject(), "0 * * * *", TimeZoneInfo.Local);
            // 防疫旅館 春節專案流水號
            //RecurringJob.AddOrUpdate(() => SwipeC.SetSPRNum(), "10 0 * * *", TimeZoneInfo.Local);
            // 防疫旅館 春節專案流水號+每日統計報表
            //Areas.Admin.Controllers.FlyPayController FlyPayAdm = new Areas.Admin.Controllers.FlyPayController();
            //RecurringJob.AddOrUpdate(() => FlyPayAdm.ExportSPR1(), "50 8 * * *", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate(() => FlyPayAdm.ExportSPR2(), "50 12 * * *", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate(() => FlyPayAdm.ExportSPR3(), "50 20 * * *", TimeZoneInfo.Local);
            // 聯合信用中心 請款交易
            PayECController payEC = new PayECController();
            // 產生請款檔排程
            RecurringJob.AddOrUpdate(() => payEC.RequestFileUploadSchedule(), "20 0 * * *", TimeZoneInfo.Local);
            // 處理請款回覆檔排程
            RecurringJob.AddOrUpdate(() => payEC.ProcessRequestFileDownloadSchedule(), "40 7 * * *", TimeZoneInfo.Local);
            // 請款回附檔排程回寫資料表
            RecurringJob.AddOrUpdate(() => payEC.ECFileReadLineSchedule(), "50 7 * * *", TimeZoneInfo.Local);
        }

    }
    public class DashboardAccessAuthFilter : IDashboardAuthorizationFilter
    {

        public bool Authorize(DashboardContext context)
        {
            //DashboardContext.Request 提供 Method、Path、LocalIpAddress、RemoteIpAddress 等基本屬性
            var clientIp = context.Request.RemoteIpAddress;
            //不足的話，可以轉成 OwinContext
            var owinCtx = new OwinContext(context.GetOwinEnvironment());
            var ipAddr = owinCtx.Request.LocalIpAddress;
            var isLogin = false;

            if (ipAddr == "::1" || clientIp == "::1")
            {
                isLogin = true;
            }
            else if (!string.IsNullOrEmpty(ipAddr) || !string.IsNullOrEmpty(clientIp))
            {
                var hangs = DataUtils.GetConfig("HANGFIREIP");
                // SETUP 控制開放IP 
                if (hangs.Contains(ipAddr) || hangs.Contains(clientIp))
                {
                    isLogin = true;
                }
            }

            var loginUser = owinCtx.Request.User.Identity.Name.Split('\\').Last();
            //依據來源IP、登入帳號決定可否存取
            //例如：已登入者可存取
            return isLogin;
        }
    }

}
