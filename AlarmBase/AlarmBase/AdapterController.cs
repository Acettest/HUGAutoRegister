using AlarmBase;
using DefLib.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace TK_AlarmManagement
{
    public class AdapterController<T> : IRemoteAdapter, ICommandHandler
        where T : IAlarmAdapter, new()
    {
        protected string m_Name = "";

        #region 重启定时器私有成员
        private System.Timers.Timer m_TimerReconnect;
        private bool m_InReconnectTimer = false;
        private ManualResetEvent m_ClearReconnectEvent = new ManualResetEvent(true);
        #endregion

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public AdapterController()
        {
            m_Logs = new List<string>();
            m_SysPara = new Dictionary<string, string>();
            m_State = new Dictionary<string, string>();

            m_TimerReconnect = new System.Timers.Timer(30000);

            //这个重连机制应该是有问题的
            m_TimerReconnect.Elapsed += new System.Timers.ElapsedEventHandler(m_TimerReconnect_Elapsed);
        }

        public T Adapter
        {
            get { return m_Adapter; }
        }

        protected T m_Adapter = default(T);
        protected long m_Run = 0;
        protected List<string> m_Logs = null;

        protected ICommServer m_ControllerServer = null;
        protected Dictionary<string, string> m_SysPara = null;

        protected ICommClient m_CommClient = null;

        #region 初始化
        protected void Init()
        {
            try
            {
                //获取应用程序运行路径
                string path = AppDomain.CurrentDomain.BaseDirectory;
                DataSet DBds = new DataSet();

                //读入数据库连接参数
                DBds = MD5Encrypt.DES.instance().DecryptXML2DS(path + "conf.xml", 1);

                m_SysPara.Clear();
                foreach (DataRow r in DBds.Tables["Parameters"].Rows)
                {
                    m_SysPara.Add(r["name"].ToString(), r["value"].ToString());
                }

                m_CommClient = CommManager.instance().CreateCommClient<CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(
                    m_SysPara["Adapter Name"].ToString(),
                    m_SysPara["Server IP"].ToString(),
                    Convert.ToInt32(m_SysPara["Server Port"]),
                    Convert.ToInt32(m_SysPara["Comm Timeout"]), true, false);

                m_CommClient.onLog += new TK_AlarmManagement.LogHandler(LogReceiver);

                #region Declear Adapter
                m_Adapter = new T();
                m_Adapter.ControllerPort = Convert.ToInt32(m_SysPara["Controller Port"]);
                m_Adapter.Name = m_SysPara["Adapter Name"].ToString();
                m_Adapter.Interval = Convert.ToInt32(m_SysPara["Retrieve Interval"]);
                m_Adapter.SvrID = Convert.ToInt32(m_SysPara["SvrID"]);
                m_Adapter.EncodingStr = m_SysPara["Encoding"];
                m_Adapter.Init(m_CommClient);
                #endregion

                DefLib.Util.Logger.Instance().SubscibeLog("", new DefLib.Util.Logger.LogFunction(LogReceiver));
                //m_Adapter.LogReceived += new TK_AlarmManagement.LogHandler(LogReceiver);
                m_Adapter.StateChanged += new StateChangeHandler(m_Adapter_StateChanged);

                m_Name = m_Adapter.Name;

                // 配置监控终端服务器
                List<Constants.TK_CommandType> acceptedCommands = new List<Constants.TK_CommandType>();
                acceptedCommands.Add(Constants.TK_CommandType.RESPONSE);

                List<Constants.TK_CommandType> superCommands = new List<Constants.TK_CommandType>();
                superCommands.Add(Constants.TK_CommandType.RESPONSE);

                m_ControllerServer = CommManager.instance().CreateCommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>("监控服务器",
                    acceptedCommands, superCommands,
                    Convert.ToInt32(m_SysPara["Controller Port"]),
                    Convert.ToInt32(m_SysPara["MaxController"]), 30, true, false);

                m_ControllerServer.onLog += new LogHandler(LogReceiver);

                acceptedCommands.Clear();
                superCommands.Clear();
                Dictionary<Constants.TK_CommandType, byte> empty = new Dictionary<Constants.TK_CommandType, byte>();
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_START, this, empty);
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_STOP, this, empty);
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_GETRUNTIMEINFO, this, empty);
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_GETOMCLIST, this, empty);
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_GETCURLOG, this, empty);
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_GETLOGFILES, this, empty);
                CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_SHUTDOWN, this, empty);
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("AdapterController", ex.ToString());
                throw ex;
            }
        }
        #endregion

        Dictionary<string, string> m_State;
        void m_Adapter_StateChanged(string name, string state)
        {
            lock (m_State)
                m_State[name] = state;
        }

        void LogReceiver(string category, string sLog)
        {
            LogReceiver("[" + category + "]: " + sLog);
        }


        void LogReceiver(string sLog)
        {
            lock (m_Logs)
            {
                m_Logs.Add(System.DateTime.Now.ToString("HH:mm:ss") + " " + sLog);

                if (m_Logs.Count > 1000)
                    m_Logs.RemoveRange(0, 200);
            }

            try
            {
                string sPath = AppDomain.CurrentDomain.BaseDirectory + "log" + Path.DirectorySeparatorChar;
                if (!Directory.Exists(sPath))
                    Directory.CreateDirectory(sPath);

                sPath += DateTime.Today.ToString("yy_MM_dd") + "_log.txt";
                string log = DateTime.Now.ToString("HH:mm:ss");
                log += " " + sLog;

                //日志性能有问题
                using (StreamWriter sw = File.AppendText(sPath))
                {
                    sw.WriteLine(log);
                }
            }
            catch { }
        }

        #region IRemoteAdapteController 成员

        public void Prepare()
        {
            try
            {
                Logger.Instance().SendLog("AdapterController", "正在初始化参数...");
                Init();

                CommandProcessor.instance().Start(true);
                m_ControllerServer.Start();//通讯服务器启动

                Logger.Instance().SendLog("AdapterController", "初始化完毕.");
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("AdapterController", ex.ToString());
            }
        }

        public bool Start()
        {
            lock (this)
            {
                Stop();

                try
                {
                    if (Interlocked.Exchange(ref m_Run, 1) == 1)
                        return true;

                    Logger.Instance().SendLog("AdapterController", "正在启动...");

                    m_Adapter.Start();//采集服务器启动 核心代码

                    m_TimerReconnect.Start();

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Instance().SendLog("AdapterController", ex.ToString());

                    try
                    {
                        Stop();
                        //m_TimerReconnect.Stop();
                        //m_ClearReconnectEvent.WaitOne();

                        //m_Adapter.Stop();
                        //m_Adapter.LogReceived -= new TK_AlarmManagement.LogHandler(LogReceiver);
                    }
                    catch { }
                    finally
                    {
                        Interlocked.Exchange(ref m_Run, 0);
                    }
                    return false;
                }
            }
        }

        public bool Stop()
        {
            lock (this)
            {
                try
                {
                    if (Interlocked.Exchange(ref m_Run, 0) == 0)
                        return true;

                    m_TimerReconnect.Stop();
                    m_ClearReconnectEvent.WaitOne();

                    m_Adapter.Stop();
                    Logger.Instance().SendLog("AdapterController", "已经停止.");
                }
                catch (Exception ex)
                {
                    Logger.Instance().SendLog("AdapterController", ex.ToString());
                    return false;
                }
                finally
                {
                    //m_CommClient.onLog -= new TK_AlarmManagement.LogHandler(LogReceiver);
                }

                return true;
            }
        }

        /// <summary>
        /// 此方法包含两个部分：1:通讯进程终止，2:程序退出
        /// </summary>
        public void Shutdown()
        {
            try
            {
                Stop();
            }
            catch { }

            try
            {
                m_ControllerServer.Close();
                //CommandProcessor.instance().Stop(); 在handlecommand内，不能去停止commandprocessor
            }
            catch { }

            System.Environment.Exit(1);
        }

        public List<string> GetOMCList()
        {
            List<string> omcs = new List<string>();
            foreach (KeyValuePair<string, string> pair in m_Adapter.OMCInfo)
            {
                omcs.Add(pair.Key);
            }

            return omcs;
        }

        public int GetStatus()
        {
            return (int)m_Adapter.CompleteRunFlag;// Interlocked.Read(ref m_Run);
        }

        public List<string> GetCurrentLog()
        {
            StringBuilder sb = new StringBuilder();
            lock (m_Logs)
            {
                return new List<string>(m_Logs.ToArray());
            }

            //return sb.ToString();
        }
        /// <summary>
        /// 获取日志文件夹下的所有文件名称
        /// </summary>
        /// <returns></returns>
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
                Logger.Instance().SendLog("AdapterController", ex.ToString());
                return new List<string>();
            }
        }

        #endregion

        /// <summary>
        /// 重连机制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_TimerReconnect_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                m_ClearReconnectEvent.WaitOne();
                m_ClearReconnectEvent.Reset();

                if (m_InReconnectTimer)
                    return;
                else
                    m_InReconnectTimer = true;

                if (m_TimerReconnect.Enabled && Interlocked.Read(ref m_Run) == 1 && m_Adapter.CompleteRunFlag == 0)
                {
                    Logger.Instance().SendLog("AdapterController", "自动重新启动" + Name + "采集程序.");

                    m_Adapter.Stop();

                    m_Adapter.Start();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    m_Adapter.Stop();
                    Logger.Instance().SendLog("AdapterController", "自动重启" + Name + "采集程序时出现异常:" + Environment.NewLine + ex.ToString());

                    for (int i = 0; i < 180; i += 1000)
                    {
                        if (Interlocked.Read(ref m_Run) == 0)
                            break;
                        Thread.Sleep(1000);
                    }
                }
                catch { }
            }
            finally
            {
                m_InReconnectTimer = false;

                m_ClearReconnectEvent.Set();
            }
        }

        #region ICommandHandler 成员
        /// <summary>
        /// 命令处理程序
        /// </summary>
        /// <param name="message"></param>
        public void handleCommand(ICommunicationMessage message)
        {
            if (message.Contains(Constants.MSG_PARANAME_ADAPTER_NAME))
            {
                if (message.GetValue(Constants.MSG_PARANAME_ADAPTER_NAME).ToString().Trim() != Name)
                    return;
            }
            else
                throw new Exception("Incoming package's name mismatched.");

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
                //对应几个命令：启动、停止、退出、
                switch (message.TK_CommandType)
                {
                    case Constants.TK_CommandType.ADAPTER_START:
                        if (Start())
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        else
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");

                        break;
                    case Constants.TK_CommandType.ADAPTER_STOP:
                        if (Stop())
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        else
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");

                        break;
                    case Constants.TK_CommandType.ADAPTER_SHUTDOWN:
                        Shutdown();
                        break;
                    case Constants.TK_CommandType.ADAPTER_GETRUNTIMEINFO:
                        {
                            Process p = Process.GetCurrentProcess();
                            resp.SetValue("PROCESSID", p.Id.ToString());
                            resp.SetValue("THREADCOUNT", p.Threads.Count.ToString());
                            resp.SetValue("PHYMEMORY", p.WorkingSet64.ToString());
                            resp.SetValue("STATUS", GetStatus().ToString());
                            resp.SetValue("STARTTIME", p.StartTime.ToString());
                            resp.SetValue("CPUTIME", ((long)p.TotalProcessorTime.TotalMinutes).ToString());
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        }
                        break;
                    case Constants.TK_CommandType.ADAPTER_GETOMCLIST://重点
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in GetOMCList())
                            {
                                sb.Append(s);
                                sb.Append(",");
                            }

                            if (sb.Length > 0)
                                sb.Remove(sb.Length - 1, 1);

                            resp.SetValue("OMCLIST", sb.ToString());
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        }
                        break;
                    case Constants.TK_CommandType.ADAPTER_GETCURLOG:
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (string s in GetCurrentLog())
                            {
                                sb.Append(s);
                                sb.Append(Environment.NewLine);
                            }

                            resp.SetValue("CURLOG", sb.ToString());
                            resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        }
                        break;
                    case Constants.TK_CommandType.ADAPTER_GETLOGFILES:
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
                    default:
                        break;
                }

                CommandProcessor.instance().DispatchCommand(resp);
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("AdapterController", ex.ToString());
            }
        }

        #endregion
    }
}
