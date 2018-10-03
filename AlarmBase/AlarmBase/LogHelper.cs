using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace AlarmBase
{
    public class LoggerHelper
    {
        private long m_LogRun = 0L;//0停止 1运行
        private readonly string m_BasePath = string.Empty;
        private static LoggerHelper m_Logger = null;
        private LoggerHelper()
        {
            m_BasePath = AppDomain.CurrentDomain.BaseDirectory + "Log\\";
            if (!Directory.Exists(m_BasePath))
                Directory.CreateDirectory(m_BasePath);
        }

        public static LoggerHelper CreateInstance()
        {
            if (m_Logger == null)
                m_Logger = new LoggerHelper();
            return m_Logger;
        }
        /// <summary>
        /// 提供最新的200条日志记录
        /// </summary>
        public List<string> m_Logs = new List<string>();
        private List<string> m_InfoLogs = new List<string>();
        private List<string> m_ErrorLogs = new List<string>();

        /// <summary>
        /// 发送日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="msg">日志</param>
        public void SendLog(LogType logType, string msg)
        {
            lock (m_Logs)
            {
                m_Logs.Add("[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg);

                if (m_Logs.Count > 200)
                    m_Logs.RemoveRange(0, 50);
            }
            Console.WriteLine(msg);
            switch (logType)
            {
                case LogType.Info:
                    {
                        lock (m_InfoLogs)
                            m_InfoLogs.Add(msg);
                        break;
                    }
                case LogType.Error:
                    {
                        lock (m_ErrorLogs)
                            m_ErrorLogs.Add(msg);
                        break;
                    }
                case LogType.None:
                    break;
                case LogType.Notice:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

        /// <summary>
        /// 启动日志记录线程
        /// </summary>
        public void Start()
        {
            Interlocked.Exchange(ref m_LogRun, 1L);
            Thread log = new Thread(WriteLogger);
            log.Start();
        }

        /// <summary>
        /// 停止日志记录线程
        /// </summary>
        public void Stop()
        {
            Interlocked.Exchange(ref m_LogRun, 0L);
        }

        /// <summary>
        /// 日志线程
        /// </summary>
        private void WriteLogger()
        {
            string fname = string.Empty;
            var logsInfo = new List<string>();
            var logsError = new List<string>();
            while (Interlocked.Read(ref m_LogRun) == 1L)
            {
                try
                {
                    lock (m_InfoLogs)
                    {
                        if (m_InfoLogs.Count > 0)
                        {
                            m_InfoLogs.ForEach(s => logsInfo.Add(s));
                            fname = m_BasePath + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                            m_InfoLogs.Clear();
                        }
                    }
                    if (logsInfo.Count > 0)
                    {
                        WriteLoggerToFile(fname, logsInfo);
                        logsInfo.Clear();
                    }

                    lock (m_ErrorLogs)
                    {
                        if (m_ErrorLogs.Count > 0)
                        {
                            m_ErrorLogs.ForEach(s => logsError.Add(s));
                            fname = m_BasePath + DateTime.Now.ToString("yyyy_MM_dd") + "_Error.txt";
                            m_ErrorLogs.Clear();
                        }
                    }
                    if (logsError.Count > 0)
                    {
                        WriteLoggerToFile(fname, logsError);
                        logsError.Clear();
                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }

        /// <summary>
        /// 日志写入文件
        /// </summary>
        /// <param name="fname">文件名</param>
        /// <param name="logs">日志</param>
        private void WriteLoggerToFile(string fname, List<string> logs)
        {
            StringBuilder sb = new StringBuilder();
            logs.ForEach(s => sb.AppendLine(DateTime.Now.ToString("HH:mm:ss") + "\t" + s));
            if (!File.Exists(fname))
            {
                using (StreamWriter sw = File.CreateText(fname))
                {
                    sw.Write(sb.ToString());
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(fname))
                {
                    sw.Write(sb.ToString());
                }
            }
        }
    }

    /// <summary>
    /// 日志操作类型:Info写入日志文件，Error写入错误文件,Notice只显示不计入文件
    /// </summary>
    public enum LogType
    {
        None = 0,
        Info = 1,
        Error = 2,
        Notice = 3
    }
}
