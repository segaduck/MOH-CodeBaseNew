using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.MemoryStorage;
using EECOnline.Controllers;
using System.Linq;
using Hangfire.Dashboard;

[assembly: OwinStartup(typeof(EECOnline.Startup))]

namespace EECOnline
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();
            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire",
                new DashboardOptions
                {
                    Authorization = new[] { new DashboardAccessAuthFilter() },
                    //IsReadOnlyFunc = (context) =>
                    //{
                    //    // 由 DashboardContext 可識別使用者帳號、IP等
                    //    // 此處設定一律唯讀
                    //    return true;
                    //}
                });

            //ScheduledController sche = new ScheduledController();

            AjaxController aj = new AjaxController();
            RecurringJob.AddOrUpdate(() => aj.CheckProvideDataStatus_Or_SendMailToEECHosp(), "00 04 * * *", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => aj.CheckDataIsGot_Or_SendEmailTo_FarEastern(), "00 04 * * *", TimeZoneInfo.Local);

            // 請款檔
            // 亞東
            RecurringJob.AddOrUpdate(() => aj.ExportDat_GO("1131010011H"), "20 00 * * *", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => aj.ProcessRequestFileDownloadSchedule("1131010011H"), "40 7 * * *", TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => aj.ImportDat_Go("1131010011H"), "50 7 * * *", TimeZoneInfo.Local);
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
            if (ipAddr == "::1" || ipAddr == "211.23.49.110" || ipAddr == "210.68.37.161" || clientIp == "127.0.0.1" || clientIp == "211.23.49.110" || clientIp == "210.68.37.161")
            {
                isLogin = true;
            }
            var loginUser = owinCtx.Request.User.Identity.Name.Split('\\').Last();

            //依據來源IP、登入帳號決定可否存取
            //例如：已登入者可存取
            return isLogin;
        }
    }
}
