using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace ES.Utils
{
    public class LogUtils
    {
        private static Dictionary<string, ILog> item = new Dictionary<string, ILog>();

        public static ILog GetLogger()
        {
            return GetLogger("WebLogger");
        }

        public static ILog GetLogger(string loggerName)
        {
            if (!item.ContainsKey(loggerName))
            {
                item.Add(loggerName, LogManager.GetLogger(loggerName));
            }

            return item[loggerName];
        }
    }
}