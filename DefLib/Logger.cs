using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public class Logger : ILogHandler
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

        public void SendLog(string log)
        {
            _SendLog(log);
        }

        void _SendLog(string sLog)
        {
            LogHandler temp;
            lock (m_LockLogHandler)
            {
                temp = _onLog;
            }

            if (temp != null)
                temp(sLog);
        }  
  
        #region ILogHandler 成员

        private object m_LockLogHandler = new object();
        private LogHandler _onLog;
        public event LogHandler onLog
        {
            add
            {
                lock (m_LockLogHandler)
                    _onLog += value;
            }
            remove
            {
                lock (m_LockLogHandler)
                    _onLog -= value;
            }
        }

        #endregion
    }
}
