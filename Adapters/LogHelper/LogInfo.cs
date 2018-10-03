using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace LogHelper
{
    public class LogInfo
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 有点画蛇添足的懒加载单例模式
        static private readonly Lazy<LogInfo> lazy = new Lazy<LogInfo>(() => new LogInfo());
        public static LogInfo Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private LogInfo()
        {
            
        }
        #endregion


        public void WriteLog(string message)
        {
            log.Debug(message);
        }

        private AutoResetEvent autoReset = new AutoResetEvent(true);
        public List<string> GetCurrentLog()
        {
            autoReset.WaitOne();
            List<string> currentLogsList = new List<string>();
            string path = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "log";
            string logfile = path + Path.DirectorySeparatorChar + DateTime.Now.ToString("yy_MM_dd") + @"_log.txt";
            try
            {
                using (StreamReader sw = new StreamReader(logfile, System.Text.Encoding.Default))//编码格式要设置，否则会乱码
                {
                    while (!sw.EndOfStream)
                    {
                        if (currentLogsList.Count > 1000)
                        {
                            currentLogsList.RemoveAt(0);
                        }
                        currentLogsList.Add(sw.ReadLine());
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
            autoReset.Set();
            return currentLogsList;
        }
    }
}
