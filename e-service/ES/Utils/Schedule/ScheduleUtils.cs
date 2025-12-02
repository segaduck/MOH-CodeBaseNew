using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using log4net;

namespace ES.Utils.Schedule
{
    public class ScheduleUtils
    {
        private static readonly ILog logger = LogUtils.GetLogger();
        private Timer timer;

        public void Run()
        {
            timer = new Timer(new TimerCallback(Test), null, 10000, 20000);
        }

        public void Test(Object state)
        {
            logger.Debug("Schedule Test");
        }
    }
}