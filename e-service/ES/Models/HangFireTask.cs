using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Hangfire.SQLite;
namespace ES.Models
{
    public class HangFireTask : IRegisteredObject
    {
        public static readonly HangFireTask Instance = new HangFireTask();

        private readonly object _lockObject = new object();
        private bool _started;
        private BackgroundJobServer _backgroundJobServer;

        private HangFireTask()
        {
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_started) return;
                _started = true;
                HostingEnvironment.RegisterObject(this);
                //使用SQLite 儲存 排程資料
                GlobalConfiguration.Configuration
               .UseSQLiteStorage("Data Source=" + HttpContext.Current.Server.MapPath("~/" + System.Configuration.ConfigurationManager.AppSettings["connectionHangFire"] + "") + ";");

                // 建立Background JobSserver 來處理 Job
                _backgroundJobServer = new BackgroundJobServer();


            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_backgroundJobServer != null)
                {
                    _backgroundJobServer.Dispose();
                }
                HostingEnvironment.UnregisterObject(this);
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }
    }
}