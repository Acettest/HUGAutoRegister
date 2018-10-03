using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using DefLib.Util;
using DefLib;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace TK_AlarmManagement
{
    /// <summary>
    /// 服务器
    /// </summary>
    public class UNAlarmServer :  ICommandHandler
    {

        #region 私有成员
        private Queue m_Logs = new Queue();
        private List<string> m_Log4Controller = new List<string>();
		private System.Timers.Timer Timer_Log = new System.Timers.Timer ();
        private string m_StartArg = string.Empty;
        #endregion

        #region 公共属性
        public string StartArg
        {
            get { return m_StartArg; }
            set { m_StartArg = value; }
        }

        /// <summary>
        /// 记录当前用户操作
        /// </summary>
        public bool bRun = false;
        #endregion

        #region 初始化
        public UNAlarmServer()
        {
            Logger.Instance().SubscibeLog("", new Logger.LogFunction(Main_LogReceived));
			
			Timer_Log.Elapsed +=	new System.Timers.ElapsedEventHandler(Timer_Log_ElapsedEvent);
			Timer_Log.Interval = 5000;
			
            Dictionary<Constants.TK_CommandType, byte> empty = new Dictionary<Constants.TK_CommandType, byte>();
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.SERVER_GETRUNTIMEINFO, this, empty);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.SERVER_GETCURLOG, this, empty);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.SERVER_GETLOGFILES, this, empty);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.MON_GETTERMINALSINFO, this, empty);
        }

        public void UNAlarmServer_Init()
        {
            try
            {
                Timer_Log.Start();
                //Utilities.ReadNorthServerConf();
                CommandProcessor.instance().Start(true); // as server
                AlarmManager.instance().Prepare();
                Utilities.ReadAlarmConf();
                Run();
            }
            catch (Exception ex)
            {
                receiveLog("初始化告警管理服务时发生异常: " + ex.ToString());
            }
        }
        #endregion

        #region 启停服务器
        /// <summary>
        /// 启动服务器
        /// </summary>
        private void Run()
        {
            bRun = true;
            AlarmManager.instance().Start();
            UserManager.instance().Start();
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Stop()
        {
            UserManager.instance().Stop();
            AlarmManager.instance().Stop();
            bRun = false;
        }
        #endregion

        #region 日志相关
		bool b_InTime =false;
		object m_LockLogger = new object ();
		void Timer_Log_ElapsedEvent(Object sender,System.Timers.ElapsedEventArgs e)
		{
			lock(m_LockLogger)
			{
				if(b_InTime)
					return;
				else
					b_InTime =true;
			}
			
			try
			{
				
				string[] ar = null;
            lock (m_Logs.SyncRoot)
            {
                if (m_Logs.Count > 0)
                {
                    ar = new string[m_Logs.Count];
                    m_Logs.CopyTo(ar, 0);
                    m_Logs.Clear();
                }
            }

            if (ar != null)
            {
                foreach (string s in ar)
                {
                    receiveLog(s);
                }
            }
			}
			finally
			{
				lock(m_LockLogger)
				 b_InTime =false;
			}
		}
		
        private delegate void DelegateLog(string log);
        private void Main_LogReceived(string category, string sLog)
        {
            lock (m_Logs.SyncRoot)
                m_Logs.Enqueue("[" + category + "]: " + sLog);
        }

        private void receiveLog(string sLog)
        {
            try
            {
                string sPath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "log" + Path.DirectorySeparatorChar;
                sPath += DateTime.Today.ToString("yy_MM_dd") + "_log.txt";
                string log = DateTime.Now.ToString("HH:mm:ss");
                log += "\t" + sLog + Environment.NewLine;

                using (StreamWriter sw = File.AppendText(sPath))
                {
                    sw.Write(log);
                }

                Console.Write(log);

                lock (m_Log4Controller)
                {
                    if (m_Log4Controller.Count == 1000)
                    {
                        for (int i = 0; i < 100; ++i)
                            m_Log4Controller.RemoveAt(0);
                    }

                    m_Log4Controller.Add(log);
                }
            }
            catch {}
        }
        #endregion

        #region ICommandHandler 成员

        public void handleCommand(ICommunicationMessage message)
        {
            CommandMsgV2 resp = new CommandMsgV2();
            resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
            resp.SeqID = CommandProcessor.AllocateID();
            resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
            try
            {
                if (message.Contains("ClientID"))
                    resp.SetValue("ClientID", message.GetValue("ClientID"));
                else
                    throw new Exception("No ClientID in incoming package.");

                switch (message.TK_CommandType)
                {
                    case Constants.TK_CommandType.SERVER_GETRUNTIMEINFO:
                        {
                            Process p = Process.GetCurrentProcess();
                            resp.SetValue("PROCESSID", p.Id.ToString());
                            resp.SetValue("THREADCOUNT", p.Threads.Count.ToString());
                            resp.SetValue("MAX_THREADCOUNT", 30 + (AlarmManager.instance().MaxAdapters + AlarmManager.instance().MaxClients + AlarmManager.instance().MaxControllers) * 2);
                            resp.SetValue("PHYMEMORY", p.WorkingSet64.ToString());

                            /// SystemInfo使用了性能计数器，有可能构造不出来
                            /// 
                            resp.SetValue("AVAIL_PHYMEMORY", 0);
                            resp.SetValue("MAX_PHYMEMORY", 0);
                            resp.SetValue("CPUUSAGE", 0);

                            try
                            {
                                resp.SetValue("AVAIL_PHYMEMORY", SystemInfo.Instance.MemoryAvailable);
                                resp.SetValue("MAX_PHYMEMORY", SystemInfo.Instance.PhysicalMemory);
                                resp.SetValue("CPUUSAGE", (int)SystemInfo.Instance.CpuLoad);
                            }
                            catch { }

                            resp.SetValue("STATUS", AlarmManager.instance().GetStatus().ToString());
                            resp.SetValue("STARTTIME", p.StartTime.ToString());
                            resp.SetValue("CPUTIME", ((long)p.TotalProcessorTime.TotalMinutes).ToString());
                            resp.SetValue("ALARMCLIENTS", AlarmManager.instance().GetAlarmClientsNum().ToString());
                            resp.SetValue("MAX_ALARMCLIENTS", AlarmManager.instance().MaxClients);
                            resp.SetValue("ADAPTERCLIENTS", AlarmManager.instance().GetAdapterClientsNum().ToString());
                            resp.SetValue("MAX_ADAPTERCLIENTS", AlarmManager.instance().MaxAdapters);
                            resp.SetValue("ACTIVEALARMNUM", AlarmManager.instance().GetActiveAlarmsNum().ToString());
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        }
                        break;
                    case Constants.TK_CommandType.SERVER_GETCURLOG:
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in GetCurrentLog())
                            {
                                sb.Append(s);
                            }

                            resp.SetValue("CURLOG", sb.ToString());
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        }
                        break;
                    case Constants.TK_CommandType.SERVER_GETLOGFILES:
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in GetLogFiles())
                            {
                                sb.Append(s);
                                sb.Append(",");
                            }

                            if (sb.Length > 0)
                                sb.Remove(sb.Length - 1, 1);

                            resp.SetValue("LOGFILES", sb.ToString());
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        }
                        break;
                    case Constants.TK_CommandType.MON_GETTERMINALSINFO:
                        {
                            C5.HashDictionary<long, AdapterInfo> ads = new C5.HashDictionary<long, AdapterInfo>();
                            AlarmManager.instance().GetAdaptersInfo(ads);

                            resp.SetValue(Constants.MSG_PARANAME_TERMINALS_INFO, ads);
                        }
                        break;
                    default:
                        break;
                }

                CommandProcessor.instance().DispatchCommand(resp);
            }
            catch (Exception ex)
            {
                Main_LogReceived("", ex.ToString());
            }
        }

        #endregion

        #region IRemoteController 成员
        public string[] GetCurrentLog()
        {
            lock (m_Log4Controller)
            {
                string[] logs = m_Log4Controller.ToArray();
                m_Log4Controller.Clear();
                return logs;
            }
        }

        public List<string> GetLogFiles()
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                path += "log" + Path.DirectorySeparatorChar;

                List<string> files = new List<string>(System.IO.Directory.GetFiles(path));
                return files;
            }
            catch (Exception ex)
            {
                Main_LogReceived("", ex.ToString());
                return new List<string>();
            }
        }

        #endregion

        #region 主函数入口
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "log"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "log");

            UNAlarmServer f = new UNAlarmServer();
            if (args.Length > 0)
                f.StartArg = args[0];

            f.UNAlarmServer_Init();
        }
        #endregion

    }

    public class LoggerAgent : System.MarshalByRefObject, ILoggerAgent
    {
        #region ILoggerAgent 成员

        public void SendLog(string log)
        {
            Logger.Instance().SendLog(log);
        }

        #endregion
    }
}
