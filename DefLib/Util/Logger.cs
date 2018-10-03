using System;
using System.Collections.Generic;
using System.Text;

namespace DefLib.Util
{
    public class Logger
    {
        #region Singleton Implementation
        protected static object m_LockSingleton = new object();
        protected static Logger m_Logger = new Logger();
        protected Logger()
        {
        }

        public static Logger Instance()
        {
            lock (m_LockSingleton)
            {
                if (m_Logger == null)
                    m_Logger = new Logger();

                return m_Logger;
            }
        }
        #endregion

        private Dictionary<string, List<LogFunction>> m_Subscriber = new Dictionary<string, List<LogFunction>>();

        public delegate void LogFunction(string category, string message);
        [Obsolete]
        public void SubscibeLog(string category, LogFunction callback)
        {
            SubscribeLog(category, callback);
        }

        public void SubscribeLog(string category, LogFunction callback)
        {
            if (callback == null)
                return;

            lock (m_Subscriber)
            {
                if (m_Subscriber.ContainsKey(category))
                {
                    // 已经注册过的函数回调，只允许添加一次
                    if (m_Subscriber[category].Contains(callback))
                        return;

                    m_Subscriber[category].Add(callback);
                }
                else
                {
                    (m_Subscriber[category] = new List<LogFunction>()).Add(callback);
                }
            }
        }

        public void SendLog(string category, string message)
        {
            if (category == null)
                return;

            if (category == "")
            { // send log to all subscriber
                SendLog(message);
            }
            else
            {
                lock (m_Subscriber)
                {
                    if (m_Subscriber.ContainsKey(category))
                    {
                        foreach (LogFunction f in m_Subscriber[category])
                        {
                            f(category, message);
                        }
                    }

                    if (m_Subscriber.ContainsKey(""))
                    {
                        foreach (LogFunction f in m_Subscriber[""])
                        {
                            f(category, message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Send log message to all subscriber
        /// </summary>
        /// <param name="message"></param>
        public void SendLog(string message)
        {
            lock (m_Subscriber)
            {
                foreach (KeyValuePair<string, List<LogFunction>> p in m_Subscriber)
                {
                    foreach (LogFunction f in p.Value)
                        f("", message);
                }
            }
        }
    }
}
