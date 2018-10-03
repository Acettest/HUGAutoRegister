using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

#if MYSQL
using MySql.Data.MySqlClient;
#else
using System.Data.SqlClient;
#endif
using Microsoft.ApplicationBlocks.Data;


using System.Data;
using System.Text;
using System.IO;
using DefLib.Util;
using System.Diagnostics;

namespace TK_AlarmManagement
{
    /// <summary>
    /// AlarmManager 的摘要说明。
    /// </summary>
    public class AlarmManager : ICommandHandler
    {
        #region 私有成员
        private Dictionary<string, string> m_SysPara = new Dictionary<string, string>();

        private long bRun = 0;

        protected System.Threading.ReaderWriterLock m_StopPrivilege = null;

        /// <summary>
        /// 派发告警线程
        /// </summary>
        /// 
        private ManualResetEvent m_SendAlarmClearEvent = new ManualResetEvent(true);

        /// <summary>
        /// 维护告警线程
        /// </summary>
        private ManualResetEvent m_UpdateAlarmClearEvent = new ManualResetEvent(true);
        private AutoResetEvent m_SignalNewAlarm = new AutoResetEvent(false);

        /// <summary>
        /// 定时查询派单信息
        /// </summary>
        private System.Timers.Timer timerGetOrderInfo;

        /// <summary>
        /// 定时查询告警是否超出工程区间
        /// </summary>
        private System.Timers.Timer timerQueryProjectInfo;

        /// <summary>
        /// 长操作线程
        /// </summary>
        private List<CommandMsgV2> m_LongOperQueue = new List<CommandMsgV2>();
        private AutoResetEvent m_SignalNewLongOper = new AutoResetEvent(true);
        private ManualResetEvent m_ClearLongOperEvent = new ManualResetEvent(true);

        /// <summary>
        /// 厂商适配器组
        /// </summary>
        private ArrayList lstAdapter = new ArrayList();
        private Dictionary<long, AdapterInfo> m_AdapterInfo = new Dictionary<long, AdapterInfo>();

        /// <summary>
        /// 适配器线程组
        /// </summary>
        private ArrayList lstAdapterThread = new ArrayList();

        /// <summary>
        /// 已注册连接的客户端列表
        /// </summary>
        private Dictionary<long, AlarmClient> m_Clients = new Dictionary<long, AlarmClient>();

        /// <summary>
        /// 工程信息内存表
        /// </summary>
        private DataSet m_ProjectInfo = new DataSet();

        //工程告警过滤器组
        private FilterGroupEx m_FilterGroup = new FilterGroupEx();

        /// <summary>
        /// 集中告警数据库连接
        /// </summary>
        private string m_Connstr = "";

        /// <summary>
        /// 告警临时缓存表
        /// </summary>
        private List<TKAlarm> m_AlarmQueue = new List<TKAlarm>();

        /// <summary>
        /// 集中告警组
        /// </summary>
        private SortedList<ulong, TKAlarm> m_AlarmTable = new SortedList<ulong, TKAlarm>(Constants.ACTIVEALARM_MAXLENGTH);

        /// <summary>
        /// 维护定时器，每天3点清理日志
        /// </summary>
        private long m_InMaintenanceTimer = 0;
        private System.Timers.Timer m_TimerMaintenance = null;

        /// <summary>
        /// 告警拷贝定时器
        /// </summary>
        private System.Timers.Timer m_TimerAlarmCopy = null;

        private AdapterControlHelper m_AdapterControlHelper = null;
        private TableOperHelper m_ConfHelper = null;

        int m_MaxClients, m_MaxAdapters, m_MaxControllers;
        #endregion

        #region 公有属性
        /// <summary>
        /// 厂商适配器组
        /// </summary>
        public ArrayList AdapterList
        {
            get
            {
                lock (lstAdapter.SyncRoot)
                    return lstAdapter.Clone() as ArrayList;
            }
        }


        /// <summary>
        /// 只读，已注册连接的客户端列表
        /// </summary>
        public AlarmClient[] ClientsList
        {
            get
            {
                lock (m_Clients)
                {
                    AlarmClient[] c = new AlarmClient[m_Clients.Count];
                    m_Clients.Values.CopyTo(c, 0);
                    return c;
                }
            }
        }

        public string ConnString
        {
            get
            {
                return m_Connstr;
            }
        }

        public int MaxClients
        {
            get
            {
                return m_MaxClients;
            }
        }

        public int MaxAdapters
        {
            get
            {
                return m_MaxAdapters;
            }
        }

        public int MaxControllers
        {
            get { return m_MaxControllers; }
        }
        #endregion

        #region Singleton Implementation
        private static object _locksingleton = new int();
        private static AlarmManager _alarmManager = null;
        public static AlarmManager instance()
        {
            lock (_locksingleton)
                if (_alarmManager == null)
                    _alarmManager = new AlarmManager();

            return _alarmManager;
        }

        protected AlarmManager()
        {
            m_StopPrivilege = new ReaderWriterLock();

            timerGetOrderInfo = new System.Timers.Timer(300000);
            timerGetOrderInfo.Elapsed += new System.Timers.ElapsedEventHandler(timerGetOrderInfo_Elapsed);

            timerQueryProjectInfo = new System.Timers.Timer(600000);
            timerQueryProjectInfo.Elapsed += new System.Timers.ElapsedEventHandler(timerQueryProjectInfo_Elapsed);

            m_TimerMaintenance = new System.Timers.Timer(3540 * 1000); // 59分钟运行一次
            m_TimerMaintenance.Elapsed += new System.Timers.ElapsedEventHandler(m_TimerMaintenance_Elapsed);

            m_TimerAlarmCopy = new System.Timers.Timer(5000);
            m_TimerAlarmCopy.Elapsed += new System.Timers.ElapsedEventHandler(m_TimerAlarmCopy_Elapsed);
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化AlarmManager
        /// </summary>
        public void Prepare()
        {
            ReadConfig();
            InitProjectInfo();

            // 读取ID分配信息
            DataSet ds = new DataSet();
#if MYSQL
            string sql = "select * from MaxTKSN limit 0,1";
#else
            string sql = "select top 1 * from MaxTKSN";
#endif
            SqlHelper.FillDataset(m_Connstr, CommandType.Text, sql, ds, new string[] { "maxid" });
            if (ds.Tables["maxid"].Rows.Count == 1)
            {
                lock (m_LockTKSN)
                {
                    m_CurTKSN = Convert.ToUInt64(ds.Tables["maxid"].Rows[0]["AllocatedTKSN"]);
                    m_AllocationStep = Convert.ToUInt64(ds.Tables["maxid"].Rows[0]["AllocateStep"]);
                    m_MaxTKSN = m_CurTKSN;
                }
            }
            else
                throw new Exception("无法读取集中告警序列号信息, 请检查数据库配置.");

            CreateControllerServer();
            m_ControllerServer.Start();

            //启动北向接口服务器
            CreateNorthServer();
            m_NorthServer.Start();
        }

        /// <summary>
        /// 获取系统参数值
        /// </summary>
        /// <param name="name">系统参数的名称</param>
        /// <returns></returns>
        public string GetSysPara(string name)
        {
            lock (m_SysPara)
            {
                if (m_SysPara.ContainsKey(name))
                    return m_SysPara[name];
                else
                    throw new Exception(string.Format("系统参数中不包含:{0}.", name));
            }
        }

        /// <summary>
        /// 读取数据库配置文件
        /// </summary>
        private void ReadConfig()
        {
            //获取当前路径
            string sPath = AppDomain.CurrentDomain.BaseDirectory;// Application.ExecutablePath;
//            sPath = sPath.Substring(0, sPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            //读取保存在dbconnection.xml中的登陆连接信息
            DataSet xmlds = new DataSet();

            xmlds.ReadXml(sPath + "dbconnection.xml");

            if (xmlds.Tables["SysDB"].Rows.Count == 0)
            {
                throw new Exception("未定义登陆数据库连接");
            }

            //将xml文件中的连接信息转换成连接字符串
            m_Connstr = "Persist Security Info=False;";
            m_Connstr = m_Connstr + "User ID=" + xmlds.Tables["SysDB"].Rows[0]["username"].ToString() + ";";
            m_Connstr = m_Connstr + "pwd =" + xmlds.Tables["SysDB"].Rows[0]["userpass"].ToString() + ";";
            m_Connstr = m_Connstr + "Initial Catalog=" + xmlds.Tables["SysDB"].Rows[0]["database"].ToString() + ";";
            m_Connstr = m_Connstr + "Data Source=" + xmlds.Tables["SysDB"].Rows[0]["hostname"].ToString();
#if MYSQL
            m_Connstr += "; character set=utf8";
#endif

            foreach (DataRow r in xmlds.Tables["Parameters"].Rows)
            {
                m_SysPara.Add(r["name"].ToString(), r["value"].ToString());
            }

            m_MaxClients = Convert.ToInt32(m_SysPara["MaxClients"]);
            m_MaxAdapters = Convert.ToInt32(m_SysPara["MaxAdapters"]);
            m_MaxControllers = Convert.ToInt32(m_SysPara["MaxControllers"]);
        }

        /// <summary>
        /// 初始化工程信息内存表
        /// </summary>
        private void InitProjectInfo()
        {
            //try
            //{
            //    lock (m_ProjectInfo)
            //    {
            //        string sql = "select id, type, start_time, end_time, ne_name, obj_name, create_date, operator, phone_no, department, report_msg from Maintenance";
            //        SqlHelper.FillDataset(m_Connstr, CommandType.Text, sql, m_ProjectInfo, new string[] { "ProjectInfo" });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.Instance().SendLog("BL", ex.ToString());
            //}
            //finally
            //{
            //}

            try
            {
                lock (m_ProjectInfo)
                {
                    //string sql = "select id, type, start_time, end_time, ne_name, obj_name, create_date, operator, phone_no, department, report_msg from Maintenance";

                    string sql = "select id, type, start_time, end_time, ne_name, redefinition, create_date, operator, phone_no, department, report_msg ";
                    sql += " from Maintenance where end_time >= '" + DateTime.Now.ToShortDateString() + "'";
                    SqlHelper.FillDataset(m_Connstr, CommandType.Text, sql, m_ProjectInfo, new string[] { "ProjectInfo" });

                    if (m_FilterGroup != null)
                    {
                        lock (m_FilterGroup.AllFilters.SyncRoot)
                            m_FilterGroup.clearFilters();
                    }

                    lock (m_FilterGroup.AllFilters.SyncRoot)
                        m_FilterGroup.loadFilters(m_ProjectInfo.Tables["ProjectInfo"]);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
            }
            finally
            {
            }
        }
        #endregion

        #region 启停告警管理器
        /// <summary>
        /// 开启AlarmManager
        /// </summary>
        /// 
        CommServer<ServerAlarmMsgInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> m_AlarmServer = null;
        CommServer<ServerAlarmMsgInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> m_CompressedAlarmServer = null;
        CommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> m_AdapterServer = null;
        CommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> m_ControllerServer = null;
        CommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> m_NorthServer = null;
        private object m_lockrun = new int();
        public void Start()
        {
            lock (m_lockrun)
            {
                if (Interlocked.Read(ref bRun) == 1)
                    return;
                else
                    Interlocked.Exchange(ref bRun, 1);
            }

            Register();

            // 启动服务器
            CreateCompressedAlarmServer();
            CreateAlarmServer();
            CreateAdapterServer();

            m_CompressedAlarmServer.Start();
            m_AlarmServer.Start();
            m_AdapterServer.Start();

            m_AdapterControlHelper = new AdapterControlHelper(m_AdapterServer);
            m_AdapterControlHelper.RegisterCommand();

            m_ConfHelper = new TableOperHelper();
            m_ConfHelper.RegisterCommand();

            //TODO: 取出数据库中的活动告警
            GetExistAlarm();

            StartAdapters();

            Thread thr = new Thread(new ThreadStart(UpdateAlarm));
            thr.Start();

            thr = new Thread(new ThreadStart(SendAlarm));
            thr.Start();

            thr = new Thread(new ThreadStart(_dealingLongOper));
            thr.Start();

            timerGetOrderInfo.Start();

            timerQueryProjectInfo.Start();

            m_TimerMaintenance.Start();

            m_TimerAlarmCopy.Start();

            Logger.Instance().SendLog("BL", "告警服务器已经启动.");
        }

        /// <summary>
        /// 关闭AlarmManager
        /// </summary>
        public void Stop()
        {
            lock (m_lockrun)
            {
                if (Interlocked.Read(ref bRun) == 1)
                    Interlocked.Exchange(ref bRun, 0);
                else
                    return;
            }

            if (m_AdapterControlHelper != null)
            {
                m_AdapterControlHelper.UnregisterCommand();
                m_AdapterControlHelper = null;
            }

            if (m_ConfHelper != null)
            {
                m_ConfHelper.UnregisterCommand();
                m_ConfHelper = null;
            }

            StopAdapters();

            m_AdapterServer.Close();
            m_AlarmServer.Close();
            m_CompressedAlarmServer.Close();

            UnRegister();

            m_TimerMaintenance.Stop();
            m_TimerAlarmCopy.Stop();
            timerGetOrderInfo.Stop();

            timerQueryProjectInfo.Stop();

            m_SendAlarmClearEvent.WaitOne();

            m_SignalNewAlarm.Set();
            m_UpdateAlarmClearEvent.WaitOne();

            m_SignalNewLongOper.Set();
            m_ClearLongOperEvent.WaitOne();

            lstAdapter.Clear();
            lstAdapterThread.Clear();

            lock (m_AdapterInfo)
                m_AdapterInfo.Clear();

            lock (m_Clients)
                m_Clients.Clear();

            lock (m_AlarmQueue)
                m_AlarmQueue.Clear();

            lock (m_AlarmTable)
                m_AlarmTable.Clear();

            m_FilterGroup.clearFilters();

            try
            {
                m_StopPrivilege.AcquireWriterLock(-1);
            }
            catch
            {
                m_StopPrivilege.ReleaseLock();
            }

            Logger.Instance().SendLog("BL", "告警服务器已经停止.");
        }

        private CommServer<ServerAlarmMsgInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> CreateCompressedAlarmServer()
        {
            List<Constants.TK_CommandType> super_commands = new List<Constants.TK_CommandType>();
            super_commands.Add(Constants.TK_CommandType.RESPONSE);
            super_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            List<Constants.TK_CommandType> accepted_commands = new List<Constants.TK_CommandType>();
            accepted_commands.Add(Constants.TK_CommandType.ALARM_REPORT);
            accepted_commands.Add(Constants.TK_CommandType.ALARM_ACK_CHANGE);
            accepted_commands.Add(Constants.TK_CommandType.ALARM_ORDER_CHANGE);
            accepted_commands.Add(Constants.TK_CommandType.ALARM_PROJECT_CHANGE);
            accepted_commands.Add(Constants.TK_CommandType.RESPONSE);
            accepted_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            // 启动服务器
            if (m_CompressedAlarmServer == null)
            {
                m_CompressedAlarmServer = CommManager.instance().CreateCommServer<ServerAlarmMsgInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(
                    Constants.COMPRERSSED_ALARM_SERVERNAME, accepted_commands, super_commands,
                    Convert.ToInt32(m_SysPara["Compressed Alarm Server Port"]),
                    Convert.ToInt32(m_SysPara["MaxClients"]), 30, false, true);
                m_CompressedAlarmServer.onLog += new LogHandler(LogReceiver);
            }

            return m_CompressedAlarmServer;
        }

        private CommServer<ServerAlarmMsgInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> CreateAlarmServer()
        {
            List<Constants.TK_CommandType> super_commands = new List<Constants.TK_CommandType>();
            super_commands.Add(Constants.TK_CommandType.RESPONSE);
            super_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            List<Constants.TK_CommandType> accepted_commands = new List<Constants.TK_CommandType>();
            accepted_commands.Add(Constants.TK_CommandType.ALARM_REPORT);
            accepted_commands.Add(Constants.TK_CommandType.ALARM_ACK_CHANGE);
            accepted_commands.Add(Constants.TK_CommandType.ALARM_ORDER_CHANGE);
            accepted_commands.Add(Constants.TK_CommandType.ALARM_PROJECT_CHANGE);
            accepted_commands.Add(Constants.TK_CommandType.RESPONSE);
            accepted_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            // 启动服务器
            if (m_AlarmServer == null)
            {
                m_AlarmServer = CommManager.instance().CreateCommServer<ServerAlarmMsgInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(
                    Constants.ALARM_SERVERNAME, accepted_commands, super_commands,
                    Convert.ToInt32(m_SysPara["Alarm Server Port"]),
                    Convert.ToInt32(m_SysPara["MaxClients"]), 30, true, false);
                m_AlarmServer.onLog += new LogHandler(LogReceiver);
            }

            return m_AlarmServer;
        }

        private CommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> CreateAdapterServer()
        {
            List<Constants.TK_CommandType> super_commands = new List<Constants.TK_CommandType>();
            super_commands.Add(Constants.TK_CommandType.RESPONSE);
            super_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            List<Constants.TK_CommandType> accepted_commands = new List<Constants.TK_CommandType>();
            accepted_commands.Add(Constants.TK_CommandType.RESPONSE);
            accepted_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            // 启动服务器
            if (m_AdapterServer == null)
            {
                m_AdapterServer = CommManager.instance().CreateCommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(
                    Constants.ADAPTER_SERVERNAME, accepted_commands, super_commands, 
                    Convert.ToInt32(m_SysPara["Adapter Server Port"]),
                    Convert.ToInt32(m_SysPara["MaxAdapters"]), 30, false, false);
                m_AdapterServer.onLog += new LogHandler(LogReceiver);
            }

            return m_AdapterServer;
        }

        private CommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> CreateControllerServer()
        {
            List<Constants.TK_CommandType> super_commands = new List<Constants.TK_CommandType>();
            super_commands.Add(Constants.TK_CommandType.RESPONSE);
            super_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            List<Constants.TK_CommandType> accepted_commands = new List<Constants.TK_CommandType>();
            accepted_commands.Add(Constants.TK_CommandType.RESPONSE);
            accepted_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            // 启动服务器
            if (m_ControllerServer == null)
            {
                m_ControllerServer = CommManager.instance().CreateCommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(
                    Constants.MONITOR_SERVERNAME, accepted_commands, super_commands,
                    Convert.ToInt32(m_SysPara["Controller Server Port"]),
                    Convert.ToInt32(m_SysPara["MaxControllers"]), 30, false, false);
                m_ControllerServer.onLog += new LogHandler(LogReceiver);
            }

            return m_ControllerServer;
        }

        private CommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder> CreateNorthServer()
        {
            List<Constants.TK_CommandType> super_commands = new List<Constants.TK_CommandType>();
            super_commands.Add(Constants.TK_CommandType.RESPONSE);
            super_commands.Add(Constants.TK_CommandType.KEEPALIVE);

            List<Constants.TK_CommandType> accepted_commands = new List<Constants.TK_CommandType>();
            accepted_commands.Add(Constants.TK_CommandType.RESPONSE);
            accepted_commands.Add(Constants.TK_CommandType.KEEPALIVE);
            accepted_commands.Add(Constants.TK_CommandType.NI_ALARM_SYNC_REPORTER);
            accepted_commands.Add(Constants.TK_CommandType.NI_LOG_SYNC_REPORTER);

            // 启动服务器
            if (m_NorthServer == null)
            {
                m_NorthServer = CommManager.instance().CreateCommServer<DefaultInterpreter, CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(
                    Constants.NORTH_SERVERNAME, accepted_commands, super_commands,
                    Convert.ToInt32(m_SysPara["North Server Port"]),
                    Convert.ToInt32(m_SysPara["MaxControllers"]), 30, false, false);
                m_NorthServer.onLog += new LogHandler(LogReceiver);
            }

            return m_NorthServer;
        }

        void LogReceiver(string log)
        {
            Logger.Instance().SendLog("BL", log);
        }
        #endregion

        #region AlarmAdapter操作，增删、启停
        /// <summary>
        /// 添加一个适配器
        /// </summary>
        public void AddAdapter(IAlarmAdapter adapter)
        {
            lock (lstAdapter.SyncRoot)
                lstAdapter.Add(adapter);
        }

        /// <summary>
        /// 删除一个适配器
        /// </summary>
        /// <param name="adapter"></param>
        public void DeleteAdapter(IAlarmAdapter adapter)
        {
            lock (lstAdapter.SyncRoot)
                lstAdapter.Remove(adapter);
        }

        /// <summary>
        /// 启动所有适配器
        /// </summary>
        public void StartAdapters()
        {
            Logger.Instance().SendLog("BL", "正在启动所有告警适配器...");

            lock (lstAdapter.SyncRoot)
            {
                for (int i = 0; i < lstAdapter.Count; i++)
                {
                    IAlarmAdapter alarmAdapter = (IAlarmAdapter)AdapterList[i];

                    alarmAdapter.AlarmReceived += new AlarmHandler(alarmAdapter_AlarmReceived);
                    alarmAdapter.LogReceived += new LogHandler(alarmAdapter_LogReceived);

                    Thread threadAdapter = new Thread(new ThreadStart(alarmAdapter.Start));
                    threadAdapter.Start();

                    lock (lstAdapterThread.SyncRoot)
                        lstAdapterThread.Add(threadAdapter);
                }
            }
        }

        /// <summary>
        /// 关闭所有的适配器
        /// </summary>
        public void StopAdapters()
        {
            Logger.Instance().SendLog("BL", "正在关闭所有告警适配器...");

            lock (lstAdapter.SyncRoot)
            {
                for (int i = 0; i < lstAdapter.Count; i++)
                {
                    IAlarmAdapter alarmAdapter = (IAlarmAdapter)AdapterList[i];
                    alarmAdapter.Stop();
                }
            }

            lock (lstAdapterThread.SyncRoot)
                lstAdapterThread.Clear();
        }
        #endregion

        #region 告警数据库操作
        /// <summary>
        /// 执行数据库操作（删除、插入、更新）
        /// </summary>
        /// <param name="tkalarm"></param>
        private void SaveToDB(string sql)
        {
            try
            {
                if (sql != "")
                {
                    SqlHelper.ExecuteNonQuery(m_Connstr, CommandType.Text, sql);

                }
                return;
            }
#if MYSQL
            catch (MySqlException ex)
            {
                if (ex.Number != 1062/*MySQL dup entry*/ )//MSSQL dup error: 2601 && ex.Number != 2627)
#else
            catch (SqlException ex)
            {
                if (ex.Number != 2601 && ex.Number != 2627)
#endif
                {
                    Logger.Instance().SendLog("BL", ex.Number.ToString() + " " + ex.ToString() + sql);

                    throw ex;
                }
                return;
            }
            finally
            {
            }
        }
        #endregion

        #region 客户端管理
        /// <summary>
        /// 注册一个客户端
        /// </summary>
        /// <param name="client">已登陆的客户端实例</param>
        public void RegisterClient(AlarmClient client)
        {
            lock (m_Clients)
            {
                if (!m_Clients.ContainsKey(client.ClientID))
                    m_Clients.Add(client.ClientID, client);
            }
        }

        /// <summary>
        /// 从已注册的客户端列表中移除指定ID的客户端
        /// </summary>
        /// <param name="clientid"></param>
        /// 
        public bool UnRegisterClient(long clientid)
        {
            lock (m_Clients)
            {
                return m_Clients.Remove(clientid);
            } // end lock clients
        }
        #endregion

        #region 向CP的命令注册与去注册
        /// <summary>
        /// 将Manager注册到CommandProcessor中
        /// </summary>
        public void Register()
        {
            Dictionary<Constants.TK_CommandType, byte> super_commands = new Dictionary<Constants.TK_CommandType, byte>();
            super_commands.Add(Constants.TK_CommandType.REGISTERCLIENT, 1);
            super_commands.Add(Constants.TK_CommandType.UNREGISTERCLIENT, 1);
            super_commands.Add(Constants.TK_CommandType.ADAPTER_LOGIN, 1);
            super_commands.Add(Constants.TK_CommandType.ADAPTER_LOGOUT, 1);
            super_commands.Add(Constants.TK_CommandType.ADAPTER_STATE_REPORT, 1);
            super_commands.Add(Constants.TK_CommandType.UNREGISTERADAPTER, 1);
            super_commands.Add(Constants.TK_CommandType.ALLOCATE_TKSN, 1);
            super_commands.Add(Constants.TK_CommandType.ALARM_ACK, 1);
            super_commands.Add(Constants.TK_CommandType.ALARM_REACK, 1);
            super_commands.Add(Constants.TK_CommandType.PROJECT_ADD, 1);
            super_commands.Add(Constants.TK_CommandType.PROJECT_MODIFY, 1);
            super_commands.Add(Constants.TK_CommandType.PROJECT_REMOVE, 1);

            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ALARM_ACK, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ALARM_REACK, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.PROJECT_ADD, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.PROJECT_MODIFY, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.PROJECT_REMOVE, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.REGISTERCLIENT, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.UNREGISTERCLIENT, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.SENDORDER, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ALARM_PROJECT_CHANGE, this, super_commands);

            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_ALARM_REPORT, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_LOGIN, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_LOGOUT, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_STATE_REPORT, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.UNREGISTERADAPTER, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ALLOCATE_TKSN, this, super_commands);
            CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.ADAPTER_SHUTDOWN, this, super_commands);
        }

        public void UnRegister()
        {
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ALARM_ACK, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ALARM_REACK, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.PROJECT_ADD, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.PROJECT_MODIFY, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.PROJECT_REMOVE, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.REGISTERCLIENT, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.UNREGISTERCLIENT, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.SENDORDER, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ALARM_PROJECT_CHANGE, this);

            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ADAPTER_ALARM_REPORT, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ADAPTER_LOGIN, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ADAPTER_LOGOUT, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ADAPTER_STATE_REPORT, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.UNREGISTERADAPTER, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ALLOCATE_TKSN, this);
            CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.ADAPTER_SHUTDOWN, this);
        }
        #endregion

        #region ICommandHandler 成员

        public void handleCommand(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = null;// new CommandMsgV2();

            switch (message.TK_CommandType)
            {
                case Constants.TK_CommandType.REGISTERCLIENT:
                    /*responseMsg = */
                    RegisterClient(message);
                    break;
                case Constants.TK_CommandType.UNREGISTERCLIENT:
                    /*responseMsg = */
                    UnRegisterClient(message);
                    break;
                case Constants.TK_CommandType.ALARM_ACK:
                    enqueueLongOper(message as CommandMsgV2);
                    break;
                case Constants.TK_CommandType.ALARM_REACK:
                    enqueueLongOper(message as CommandMsgV2);
                    break;
                case Constants.TK_CommandType.PROJECT_ADD:
                    responseMsg = ProjectAdd(message as CommandMsgV2);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.PROJECT_MODIFY:
                    responseMsg = ProjectModify(message as CommandMsgV2);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.PROJECT_REMOVE:
                    responseMsg = ProjectRemove(message as CommandMsgV2);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.SENDORDER:
                    responseMsg = SendOrder(message);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.ADAPTER_ALARM_REPORT:
                    AlarmReceived(message);
                    break;
                case Constants.TK_CommandType.ADAPTER_LOGIN:
                    responseMsg = RegisterAdapter(message);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.ADAPTER_LOGOUT:
                    responseMsg = UnRegisterAdapter(message);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.UNREGISTERADAPTER:
                    responseMsg = UnRegisterAdapter(message);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.ADAPTER_STATE_REPORT:
                    AdapterStateChange(message);
                    break;
                case Constants.TK_CommandType.ALLOCATE_TKSN:
                    responseMsg = AdapterAllocateTKSN(message);
                    CommandProcessor.instance().DispatchCommand(responseMsg);
                    break;
                case Constants.TK_CommandType.ADAPTER_SHUTDOWN:
                    {
                        try
                        {
                            m_AdapterServer.Close();
                            m_AlarmServer.Close();
                            m_CompressedAlarmServer.Close();
                            m_ControllerServer.Close();
                            m_NorthServer.Close();
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance().SendLog("BL", ex.ToString());
                        }

                        System.Environment.Exit(1);
                    }
                    break;
                default:
                    break;

            }
        }
        #endregion

        #region 采集适配器相关
        public void GetAdaptersInfo(C5.HashDictionary<long, AdapterInfo> ads)
        {
            ads.Clear();

            lock (m_AdapterInfo)
            {
                foreach (KeyValuePair<long, AdapterInfo> p in m_AdapterInfo)
                    ads.Add(p.Key, p.Value);
            }
        }

        public void RemoveAdapterInfo(long id)
        {
            lock (m_AdapterInfo)
                m_AdapterInfo.Remove(id);
        }

        private CommandMsgV2 AdapterAllocateTKSN(ICommunicationMessage message)
        {
            long adapterid = 0;
            ulong num = 0;

            CommandMsgV2 resp = null;

            try
            {
                adapterid = Convert.ToInt64(message.GetValue("ClientID"));
                num = Convert.ToUInt64(message.GetValue(Constants.MSG_PARANAME_TKSN_NUM));

                ulong start = 0, end = 0;
                AllocateTKSN(num, ref start, ref end);

                resp = new CommandMsgV2();
                resp.SeqID = CommandProcessor.AllocateID();
                resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                resp.SetValue("ClientID", adapterid);
                resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                resp.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                resp.SetValue(Constants.MSG_PARANAME_TKSN_START, start);
                resp.SetValue(Constants.MSG_PARANAME_TKSN_END, end);
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());

                resp = new CommandMsgV2();
                resp.SeqID = CommandProcessor.AllocateID();
                resp.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                resp.SetValue("ClientID", adapterid);
                resp.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                resp.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
            }

            return resp;
        }

        private void AdapterStateChange(ICommunicationMessage message)
        {
            long adapterid = 0;

            try
            {
                adapterid = Convert.ToInt64(message.GetValue("ClientID"));

                AdapterInfo info = null;
                lock (m_AdapterInfo)
                {
                    if (m_AdapterInfo.ContainsKey(adapterid))
                    {
                        info = m_AdapterInfo[adapterid];
                    }
                }

                if (info != null)
                {
                    AdapterStatus s = (AdapterStatus)message.GetValue(Constants.MSG_PARANAME_ADAPTER_STATE);
                    if (info.State != s)
                    {
                        switch (info.State = s)
                        {
                            case AdapterStatus.Running:
                                Logger.Instance().SendLog("BL", "适配器: " + info.Name + " " + info.Address + " 状态已更改为运行(空闲).");
                                break;
                            case AdapterStatus.Stop:
                                Logger.Instance().SendLog("BL", "适配器: " + info.Name + " " + info.Address + " 状态已更改为停止.");
                                break;
                            case AdapterStatus.Working:
                                Logger.Instance().SendLog("BL", "适配器: " + info.Name + " " + info.Address + " 正在执行任务.");
                                break;
                            default:
                                Logger.Instance().SendLog("BL", "适配器: " + info.Name + " " + info.Address + " 状态未知: " + message.GetValue("ADAPTER_STATE").ToString());
                                break;
                        }
                    }

                    object o = message.GetValue(Constants.MSG_PARANAME_ADAPTER_EXTRAINFO);
                    if (o != null)
                        info.ExtraInfo = o;
                }

                //CommandMsgV2 tomonitor = (CommandMsgV2)message.clone();
                //tomonitor.TK_CommandType = Constants.TK_CommandType.MON_TERMINALSTATECHANGE;
                //tomonitor.SeqID = CommandProcessor.AllocateID();
                //CommandProcessor.instance().DispatchCommand(tomonitor);
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
            }
        }

        private CommandMsgV2 UnRegisterAdapter(ICommunicationMessage message)
        {
            long adapterid = 0;
            CommandMsgV2 msg = new CommandMsgV2();

            try
            {
                adapterid = Convert.ToInt64(message.GetValue("ClientID"));

                AdapterInfo info = null;
                lock (m_AdapterInfo)
                {
                    if (m_AdapterInfo.ContainsKey(adapterid))
                    {
                        info = m_AdapterInfo[adapterid];
                        info.State = AdapterStatus.Stop;

                        if (info.ControllerPort == 0)
                            m_AdapterInfo.Remove(adapterid); // 删除不可控制的采集，其它的不删除，待主动校验时删除
                    }
                }

                if (info != null)
                    Logger.Instance().SendLog("BL", "适配器: " + info.Name + " " + info.Address + " 已从系统注销.");

                msg.SeqID = CommandProcessor.AllocateID();
                msg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                msg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                msg.SetValue("ClientID", adapterid);
                msg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());

                msg.SeqID = CommandProcessor.AllocateID();
                msg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                msg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                msg.SetValue("ClientID", adapterid);
                msg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
            }

            return msg;
        }

        private CommandMsgV2 RegisterAdapter(ICommunicationMessage message)
        {
            long adapterid = 0;
            CommandMsgV2 msg = null;

            try
            {
                AdapterInfo info = new AdapterInfo();
                adapterid = Convert.ToInt64(message.GetValue("ClientID"));

                info.ID = adapterid;
                info.Name = message.GetValue(Constants.MSG_PARANAME_ADAPTER_NAME).ToString();
                info.Address = message.GetValue(Constants.MSG_PARANAME_ADAPTER_ADDRESS).ToString();
                info.ControllerPort = Convert.ToInt32(message.GetValue(Constants.MSG_PARANAME_ADAPTER_CONTROLLER_PORT));
                info.State = 0;

                if (message.Contains(Constants.MSG_PARANAME_ADAPTER_EXTRAINFO))
                    info.ExtraInfo = message.GetValue(Constants.MSG_PARANAME_ADAPTER_EXTRAINFO);
                else
                    info.ExtraInfo = null;

                lock (m_AdapterInfo)
                {
                    List<long> toberemove = new List<long>();
                    foreach (KeyValuePair<long, AdapterInfo> p in m_AdapterInfo)
                    {
                        AdapterInfo exist = p.Value;
                        if (exist.Name == info.Name && exist.Address == info.Address && exist.ControllerPort == info.ControllerPort)
                        {
                            // 该采集已有记录
                            toberemove.Add(p.Key);
                        }
                    }

                    foreach (long id in toberemove)
                        m_AdapterInfo.Remove(id);

                    m_AdapterInfo[adapterid] = info;
                }

                Logger.Instance().SendLog("BL", "适配器: " + info.Name + " " + info.Address + " 已登录到系统.");

                msg = new CommandMsgV2();
                msg.SeqID = CommandProcessor.AllocateID();
                msg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                msg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                msg.SetValue("ClientID", adapterid);
                msg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");

            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());

                msg = new CommandMsgV2();
                msg.SeqID = CommandProcessor.AllocateID();
                msg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                msg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                msg.SetValue("ClientID", adapterid);
                msg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
            }

            return msg;
        }

        /// <summary>
        /// 收到adapter发送的集中告警事件响应
        /// </summary>
        /// <param name="tkalarm">集中告警</param>
        private void alarmAdapter_AlarmReceived(List<TKAlarm> lstAlarm)
        {
            try
            {
                //将新告警放入内存中
                lock (m_AlarmQueue)
                {
                    m_AlarmQueue.AddRange(lstAlarm);
                }

                m_SignalNewAlarm.Set();
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
            }
            finally
            {
            }
        }

        /// <summary>
        /// 将adapter的日志信息转发到界面线程
        /// </summary>
        /// <param name="sLog"></param>
        private void alarmAdapter_LogReceived(string sLog)
        {
            Logger.Instance().SendLog("DRV", sLog);
        }

        private void AlarmReceived(ICommunicationMessage message)
        {
            TKAlarm alarm = new TKAlarm();
            try
            {
                alarm.ConvertFromMsg(message);
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", "适配器发送无效告警: " + ex.ToString());
                return;
            }

            lock (m_AlarmQueue)
                m_AlarmQueue.Add(alarm);

            m_SignalNewAlarm.Set();
        }

        #endregion

        #region 监控相关

        #endregion

        #region 长操作处理
        private void enqueueLongOper(CommandMsgV2 msg)
        {
            lock (m_LongOperQueue)
                m_LongOperQueue.Add(msg);

            m_SignalNewLongOper.Set();
        }

        private void _dealingLongOper()
        {
            m_ClearLongOperEvent.Reset();

            try
            {
                while (Interlocked.Read(ref bRun) == 1)
                {
                    m_SignalNewLongOper.WaitOne();

                    ICommunicationMessage[] opers = null;
                    lock (m_LongOperQueue)
                    {
                        opers = m_LongOperQueue.ToArray();
                        m_LongOperQueue.Clear();
                    }

                    foreach (ICommunicationMessage msg in opers)
                    {
                        try
                        {
                            ICommunicationMessage responseMsg = null;
                            switch (msg.TK_CommandType)
                            {
                                case Constants.TK_CommandType.ALARM_ACK:
                                    responseMsg = AlarmAck(msg);
                                    CommandProcessor.instance().DispatchCommand(responseMsg);
                                    break;
                                case Constants.TK_CommandType.ALARM_REACK:
                                    responseMsg = AlarmReAck(msg);
                                    CommandProcessor.instance().DispatchCommand(responseMsg);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance().SendLog("BL", ex.ToString());
                        }
                    }

                    Thread.Sleep(100);
                }
            }
            finally
            {
                m_ClearLongOperEvent.Set();
            }
        }

        #endregion

        #region 定时上报告警
        private void SendAlarm()
        {
            m_SendAlarmClearEvent.Reset();

            while (Interlocked.Read(ref bRun) == 1)
            {
                try
                {
                    List<AlarmClient> lstTemp = new List<AlarmClient>();
                    lock (m_Clients)
                    {
                        foreach (AlarmClient c in m_Clients.Values)
                        {
                            if (c.Authorized)
                            {
                                AlarmClient n = new AlarmClient();
                                n.ClientID = c.ClientID;
                                n.AlarmFilter = c.AlarmFilter;

                                CommandMsgV2[] t = c.DequeueMessages();
                                if (t.Length == 0)
                                    continue;
                                else
                                    n.SetMessages(t);

                                lstTemp.Add(n);
                            }
                        }
                    }

                    foreach (AlarmClient client in lstTemp)
                    {
                        List<ICommunicationMessage> msgs = new List<ICommunicationMessage>();
                        msgs.AddRange(client.DequeueMessages());

                        CommandProcessor.instance().DispatchCommands(msgs);
                    }

                    Thread.Sleep(3000);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Logger.Instance().SendLog("BL", ex.ToString());
                    }
                    catch { }
                }
                finally
                {
                }
            }

            m_SendAlarmClearEvent.Set();
        }
        #endregion

        #region 定时维护活动告警表

        //		private object m_lockUpdate = new int();
        //		private bool bUpdate = false;
        /// <summary>
        /// 定时通过缓存表lstAlarmTemp中告警维护活动内存中所有的告警表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateAlarm()
        {
            m_UpdateAlarmClearEvent.Reset();

            while (Interlocked.Read(ref bRun) == 1)
            {
                m_SignalNewAlarm.WaitOne();
                if (Interlocked.Read(ref bRun) == 0)
                    break;

                try
                {
                    if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0)
                    {
                        lock (m_FilterGroup.AllFilters.SyncRoot)
                        {
                            ArrayList lstRemoveFlt = new ArrayList();

                            foreach (DictionaryEntry de in m_FilterGroup.AllFilters)
                            {
                                FilterEx filter = (FilterEx)de.Value;

                                if (DateTime.Compare(filter.EndTime, DateTime.Now) < 0)
                                {
                                    lstRemoveFlt.Add(de.Key.ToString());
                                }
                            }

                            foreach (string key in lstRemoveFlt)
                            {
                                m_FilterGroup.deleteFilter(key);
                            }
                        }
                    }

                    TKAlarm[] lstTemp;

                    // 取队列数据
                    lock (m_AlarmQueue)
                    {
                        lstTemp = m_AlarmQueue.ToArray();
                        m_AlarmQueue.Clear();
                    }

                    for (int i = 0; i < lstTemp.Length; i++)
                    {
                        if (Interlocked.Read(ref bRun) == 0)
                            break;

                        TKAlarm tempalarm = lstTemp[i];
                        ulong tksn = 0;

                        try
                        {
                            tksn = tempalarm.TKSn;
                        }
                        catch
                        {
                            Logger.Instance().SendLog("BL", "告警管理器无法转换TKSn");
                            continue;
                        }

                        try
                        {
                            try
                            {
                                tempalarm.SyncRoot.AcquireWriterLock(-1);
                                lock (m_FilterGroup.AllFilters.SyncRoot)
                                    m_FilterGroup.isMatched(ref tempalarm);
                            }
                            finally
                            {
                                tempalarm.SyncRoot.ReleaseWriterLock();
                            }

                            tempalarm.SyncRoot.AcquireReaderLock(-1);

                            // step 1. 更新内存表
                            //若为活动告警，直接拷贝到stlstTKAlarm中；若为清除告警，则删除stlstTKAlarm中对应的活动告警
                            lock (m_AlarmTable) // 内存表锁
                            {
                                if (tempalarm.OccurTime != "" && tempalarm.ClearTime == "")
                                {
                                    if (m_AlarmTable.Count > Constants.ACTIVEALARM_MAXLENGTH)
                                    {
                                        for (int m = 0; m < Constants.ACTIVEALARM_MAXLENGTH / 5; m++)
                                        {
                                            m_AlarmTable.RemoveAt(m);
                                        }
                                    }

                                    try
                                    {
                                        //若内存中已存在此活动告警，尝试重新入库，但不发送
                                        if (m_AlarmTable.ContainsKey(tempalarm.TKSn))
                                        {
                                            try
                                            {
                                                SaveToDB(m_AlarmTable[tempalarm.TKSn].GetSql());
                                            }
                                            catch { }
                                            continue; 
                                        }

                                        //采集自身告警不保存在内存中
                                        //if (tempalarm.Manufacturer != "TK")
										tempalarm.ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            m_AlarmTable.Add(tksn, tempalarm);
                                        //}
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Instance().SendLog("BL", ex.ToString());
                                        Logger.Instance().SendLog("BL", tempalarm.ConvertToMsg().encode());
                                    }

                                }
                                else if (tempalarm.ClearTime != "")
                                {
                                    m_AlarmTable.Remove(tksn);
                                }
                            } // end lock stlstTKAlarm

                            // step 2: 保存采集到的告警
                            try
                            {
                                SaveToDB(tempalarm.GetSql());

                                //插一份告警至AlarmEvent
                                if (tempalarm.OMCName == "Adapter_Web")
                                {
                                    SaveToDB(tempalarm.GetEventSql());
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance().SendLog("BL", ex.ToString());
                            }

                            // step 3. 发给客户端。因是最后一步，不单独try...catch 
                            //将告警派发给每个注册的客户端
                            lock (m_Clients)
                            {
                                foreach (AlarmClient c in m_Clients.Values)
                                {
                                    if (c.Authorized)
                                        c.FillAlarm(tempalarm);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance().SendLog("BL", ex.ToString());
                            Logger.Instance().SendLog("BL", tempalarm.ConvertToMsg().encode());
                        }
                        finally
                        {
                            tempalarm.SyncRoot.ReleaseLock();
                        }
                    } // end for
                }
                catch (Exception ex)
                {
                    Logger.Instance().SendLog("BL", ex.ToString());
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }

            m_UpdateAlarmClearEvent.Set();
        }
        #endregion

        #region 维护定时器-删除两周前的日志
        void m_TimerMaintenance_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref m_InMaintenanceTimer, 1) == 1)
                return;

            if (!m_TimerMaintenance.Enabled)
                return;

            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);

                DateTime now = DateTime.Now;
                if (now.Hour != 3)
                    return;

                string[] logfiles = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "log" + Path.DirectorySeparatorChar, "*_log.txt");
                foreach (string file in logfiles)
                {
                    try
                    {
                        DateTime t = System.IO.File.GetCreationTime(file);
                        TimeSpan span = now - t;
                        if (span.TotalDays > 14)
                            System.IO.File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance().SendLog("BL", ex.ToString());
                    }

                    if (Interlocked.Read(ref bRun) == 0)
                        return;
                }
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();
                Interlocked.Exchange(ref m_InMaintenanceTimer, 0);
            }
        }
        #endregion

        #region 定时拷贝内存告警

        void m_TimerAlarmCopy_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                lock (Utilities.m_AlarmDataTable)
                {
                    Utilities.m_AlarmDataTable.Rows.Clear();
                    lock (m_AlarmTable)
                    {
                        foreach (KeyValuePair<ulong, TKAlarm> u in m_AlarmTable)
                        {
                            string apHotArea="";
                            try
                            {
                                apHotArea = u.Value.Location.Substring(5, u.Value.Location.IndexOf("热点名称") - 5).Trim();
                            }
                            catch {
                                apHotArea = "";
                            }
                            Utilities.m_AlarmDataTable.Rows.Add(new object[]{u.Value.TKSn, u.Value.ManuSn, u.Value.City, u.Value.Manufacturer, u.Value.BusinessType, u.Value.NeName, u.Value.NeType,
                                u.Value.ObjName, u.Value.ObjType, u.Value.AlarmName, u.Value.Redefinition, u.Value.Category, u.Value.Severity, u.Value.OccurTime, u.Value.AckTimeLV1,
                                u.Value.AckAgainTimeLV1, u.Value.AckTimeLV2, u.Value.AckAgainTimeLV2, u.Value.ClearTime, u.Value.Location, u.Value.OperatorLV11, u.Value.OperatorLV12,
                                u.Value.OperatorLV21, u.Value.OperatorLV22, u.Value.ProjectInfo, u.Value.OrderOperatorLV1, u.Value.OrderIDLV1, u.Value.OrderTimeLV1, u.Value.OrderOperatorLV2,
                                u.Value.OrderIDLV2, u.Value.OrderTimeLV2, u.Value.ReceiveTime, u.Value.ProjectEndTime, u.Value.ProjectTimeOut, u.Value.OMCName, u.Value.Reserved2, u.Value.Reserved3,apHotArea});

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
            }
        }
        

        #endregion

        #region 客户端注册与去注册命令包
        private CommandMsgV2 RegisterClient(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();
            try
            {
                if (!message.Contains("SERVERNAME"))
                    return null;

                if (message.GetValue("SERVERNAME").ToString() != Constants.ALARM_SERVERNAME
                    && message.GetValue("SERVERNAME").ToString() != Constants.COMPRERSSED_ALARM_SERVERNAME)
                    return null;

                if (message.Contains("ClientID"))
                {
                    long clientid = Convert.ToInt64(message.GetValue("ClientID"));

                    AlarmClient newclient = new AlarmClient(clientid);

                    newclient.Authorized = Convert.ToBoolean(message.GetValue(Constants.MSG_PARANAME_AUTHORIZED));

                    if (message.Contains("Filter"))
                        newclient.AlarmFilter = message.GetValue("Filter").ToString().Trim();

                    int lv = 0;
                    if (message.Contains("COMPANY"))
                    {
                        if (message.GetValue("COMPANY").ToString() != Constants.COMPANY_LV1)
                            lv = 2;
                        else
                            lv = 1;
                    }

                    lock (m_AlarmTable)
                    {
                        lock (m_Clients)
                        {
                            if (m_Clients.ContainsKey(clientid))
                            {
                                if (!m_Clients[clientid].Authorized)
                                {
                                    m_Clients[clientid].Authorized = newclient.Authorized;
                                    m_Clients[clientid].AlarmFilter = newclient.AlarmFilter;

                                    foreach (KeyValuePair<ulong, TKAlarm> pair in m_AlarmTable)
                                    {
                                        if (lv == 1 && pair.Value.AckAgainTimeLV1 != "")
                                            continue;
                                        else if (lv == 2 && pair.Value.AckAgainTimeLV2 != "")
                                            continue;

                                        m_Clients[clientid].FillAlarm(pair.Value);
                                    }
                                    //m_Clients[clientid].FillAlarm(m_AlarmTable);
                                }
                            }
                            else
                            {
                                RegisterClient(newclient);
                            }
                        }
                    }

                    responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                    responseMsg.SeqID = CommandProcessor.AllocateID();
                    responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                    responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                    responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                    responseMsg.SetValue("CITY", newclient.AlarmFilter);

                    if (message.Contains("RIGHT"))
                        responseMsg.SetValue("RIGHT", message.GetValue("RIGHT"));

                }
                else
                {
                    return null;
                }
            }
            catch
            {
                responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
            }
            finally
            {
            }
            return responseMsg;
        }

        private CommandMsgV2 UnRegisterClient(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();

            try
            {
                if (message.Contains("ClientID"))
                {
                    long clientID = (long)message.GetValue("ClientID");
                    if (!UnRegisterClient(clientID))
                        UnRegisterAdapter(message);

                    responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                    responseMsg.SeqID = CommandProcessor.AllocateID();
                    responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                    responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                    responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
            }
            finally
            {
            }
            return responseMsg;
        }
        #endregion

        #region 告警确认
        private CommandMsgV2 AlarmAck(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();
            bool bSucess = false;

            try
            {
                    if (message.Contains("ClientID"))
                    {
                        long income_clientid = Convert.ToInt64(message.GetValue("ClientID"));

                        List<ulong> lstSuccessID = new List<ulong>();
                        Dictionary<ulong, string> pendingFalse = new Dictionary<ulong, string>(); 

                        string sSuccessIDs = "";
                        string sFalseIDs = "";
                        string sAckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        try
                        {
                            string sTKSNs = message.GetValue("告警ID").ToString().Trim();
                            string sOperatorID = message.GetValue("操作员ID").ToString().Trim();
                            string sOperatorLV = message.GetValue("操作员级别").ToString().Trim();

                            string[] Alarms = sTKSNs.Split(new char[] { ';' });

                            List<ulong> active_queue = new List<ulong>();
                            List<ulong> resume_queue = new List<ulong>();

                            foreach (string str in Alarms)
                            {
                                string sTKSn = "";
                                string sAlarmState = "";

                                if (str.Split(new char[] { ',' }).Length == 2)
                                {
                                    sTKSn = str.Split(new char[] { ',' })[0];
                                    sAlarmState = str.Split(new char[] { ',' })[1];
                                }

                                ulong nTKSn = Convert.ToUInt64(sTKSn);

                                if (sAlarmState == "0")
                                    active_queue.Add(nTKSn);
                                else if (sAlarmState == "1")
                                    resume_queue.Add(nTKSn);
                                
                                #region 废弃-逐条处理告警
                                /*
                                if (sAlarmState == "0")
                                {
                                    #region 处理活动告警
                                    bool bChangeMemory = false;				//是否修改内存成功
                                    bool bExist = false;				//是否已被其他用户确认

                                    // strp 1. 锁内存告警表，修改数据
                                    lock (m_AlarmTable)
                                    {
                                        if (m_AlarmTable.ContainsKey(nTKSn))
                                        {
                                            TKAlarm tkAlarm = (TKAlarm)m_AlarmTable[nTKSn];

                                            try
                                            {
                                                tkAlarm.SyncRoot.AcquireWriterLock(-1);

                                                if (sOperatorLV == "1")
                                                {
                                                    if (tkAlarm.OperatorLV11 == "")
                                                    {
                                                        tkAlarm.OperatorLV11 = sOperatorID;
                                                        tkAlarm.AckTimeLV1 = sAckTime;
                                                        bChangeMemory = true;
                                                    }
                                                    else
                                                    {
                                                        bExist = true;
                                                    }
                                                }
                                                else if (sOperatorLV == "2")
                                                {
                                                    if (tkAlarm.OperatorLV21 == "")
                                                    {
                                                        tkAlarm.OperatorLV21 = sOperatorID;
                                                        tkAlarm.AckTimeLV2 = sAckTime;
                                                        bChangeMemory = true;
                                                    }
                                                    else
                                                    {
                                                        bExist = true;
                                                    }
                                                }
                                            }
                                            finally
                                            {
                                                tkAlarm.SyncRoot.ReleaseWriterLock();
                                            }
                                        }
                                        else
                                        {
                                            bSucess = true;
                                        }
                                    } // end lock stlstTKAlarm

                                    // step 2. 修改数据库
                                    if (bChangeMemory)
                                    {
                                        //修改数据库
                                        string sql = "";
                                        if (sOperatorLV == "1")
                                        {
                                            sql = "begin transaction";
                                            sql += " update ActiveAlarm with (rowlock,holdlock) set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + sTKSn;
                                            sql += " update ResumeAlarm with (rowlock,holdlock) set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + sTKSn;
                                            sql += " commit";
                                        }
                                        else if (sOperatorLV == "2")
                                        {
                                            sql = "begin transaction";
                                            sql += " update ActiveAlarm with (rowlock,holdlock) set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + sTKSn;
                                            sql += " update ResumeAlarm with (rowlock,holdlock) set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + sTKSn;
                                            sql += " commit";
                                        }
                                        try
                                        {
                                            SaveToDB(sql);
                                            bSucess = true;
                                        }
                                        catch
                                        {
                                            //修改数据库失败，回滚操作，将内存中的修改恢复
                                            lock (m_AlarmTable)
                                            {
                                                if (m_AlarmTable.ContainsKey(nTKSn))
                                                {
                                                    TKAlarm tkAlarm = (TKAlarm)m_AlarmTable[nTKSn];

                                                    try
                                                    {
                                                        tkAlarm.SyncRoot.AcquireWriterLock(-1);
                                                        if (tkAlarm.TKSn == sTKSn)
                                                        {
                                                            if (sOperatorLV == "1")
                                                            {
                                                                tkAlarm.OperatorLV11 = "";
                                                                tkAlarm.AckTimeLV1 = "";
                                                            }
                                                            else if (sOperatorLV == "2")
                                                            {
                                                                tkAlarm.OperatorLV21 = "";
                                                                tkAlarm.AckTimeLV2 = "";
                                                            }
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        tkAlarm.SyncRoot.ReleaseWriterLock();
                                                    }

                                                }
                                            } // end lock stlstTKAlarm
                                            //TODO: 要考虑回滚时告警已恢复的情况
                                        }
                                    }
                                    else if (bExist)
                                    {
                                        //该告警已被其他用户确认
                                        sReason = "告警已被其他用户确认";
                                    }
                                    else
                                    {
                                        //该告警已经恢复
                                        sReason = "告警已恢复";
                                    }
                                    #endregion
                                }
                                //处理已恢复告警
                                else if (sAlarmState == "1")
                                {
                                    #region 处理历史告警
                                    bool bExist = false;

                                    string sql = "";
                                    if (sOperatorLV == "1")
                                    {
                                        sql = "select OperatorLV11, AckTimeLV1 from ResumeAlarm where TKSn = " + sTKSn;
                                    }
                                    else if (sOperatorLV == "2")
                                    {
                                        sql = "select OperatorLV21, AckTimeLV2 from ResumeAlarm where TKSn = " + sTKSn;
                                    }

                                    try
                                    {
                                        using (MySqlDataReader dr = SqlHelper.ExecuteReader(m_Connstr, CommandType.Text, sql))
                                        {

                                            sql = "";
                                            while (dr.Read())
                                            {
                                                if (sOperatorLV == "1")
                                                {
                                                    if (dr["OperatorLV11"] == System.DBNull.Value)
                                                    {
                                                        sql = "update ResumeAlarm with (rowlock) set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + sTKSn;
                                                    }
                                                    else
                                                    {
                                                        bExist = true;
                                                    }
                                                }
                                                else if (sOperatorLV == "2")
                                                {
                                                    if (dr["OperatorLV21"] == System.DBNull.Value)
                                                    {
                                                        sql = "update ResumeAlarm with (rowlock) set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + sTKSn;
                                                    }
                                                    else
                                                    {
                                                        bExist = true;
                                                    }
                                                }

                                                break;
                                            }
                                            dr.Close();
                                        }

                                        if (bExist)
                                        {
                                            //该告警已被其他用户确认
                                            sReason = "告警已被其他用户确认";
                                        }
                                        else
                                        {
                                            try
                                            {
                                                SaveToDB(sql);
                                                bSucess = true;
                                            }
                                            catch
                                            {
                                                //确认操作失败
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //执行失败
                                        Logger.Instance().SendLog("BL", ex.ToString());
                                    }
                                    finally
                                    {
                                    }
                                    #endregion
                                }

                                if (bSucess)
                                {
                                    lstSuccessID.Add(sTKSn);
                                }
                                else
                                {
                                    lstFalseID.Add(sTKSn + "," + sReason);
                                }
                                */
                                #endregion
                            }

                            /// 活动告警确认步骤：
                            /// 首先在内存表中查找，执行确认操作
                            /// 如果内存表中找不到（可能内存数据已经绕接），那么在数据库中查找
                            #region 活动告警逐条处理
                            foreach (ulong nTKSn in active_queue)
                            {
                                string sReason = "";

                                bool bChangeMemory = false;				//是否修改内存成功
                                bool bExist = false;				//是否已被其他用户确认

                                #region step 1. 锁内存告警表，修改数据
                                lock (m_AlarmTable)
                                {
                                    if (m_AlarmTable.ContainsKey(nTKSn))
                                    {
                                        TKAlarm tkAlarm = (TKAlarm)m_AlarmTable[nTKSn];

                                        try
                                        {
                                            tkAlarm.SyncRoot.AcquireWriterLock(-1);

                                            if (sOperatorLV == "1")
                                            {
                                                if (tkAlarm.OperatorLV11 == "")
                                                {
                                                    tkAlarm.OperatorLV11 = sOperatorID;
                                                    tkAlarm.AckTimeLV1 = sAckTime;
                                                    bChangeMemory = true;
                                                    bExist = false;
                                                }
                                                else
                                                {
                                                    bExist = true;
                                                }
                                            }
                                            else if (sOperatorLV == "2")
                                            {
                                                if (tkAlarm.OperatorLV21 == "")
                                                {
                                                    tkAlarm.OperatorLV21 = sOperatorID;
                                                    tkAlarm.AckTimeLV2 = sAckTime;
                                                    bChangeMemory = true;
                                                    bExist = false;
                                                }
                                                else
                                                {
                                                    bExist = true;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            tkAlarm.SyncRoot.ReleaseWriterLock();
                                        }
                                    }
                                    else
                                    {
                                        bExist = false;
                                        bSucess = false;
                                    }
                                } // end lock stlstTKAlarm
                                #endregion

                                #region step 2. 修改数据库
                                if (bChangeMemory)
                                {
                                    #region 修改数据库
                                    string sql = "";
                                    if (sOperatorLV == "1")
                                    {
#if MYSQL
                                        sql = "start transaction";
                                        sql += "; update ActiveAlarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; update ResumeAlarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; commit;";
#else
                                        sql = "begin transaction";
                                        sql += " update ActiveAlarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " update ResumeAlarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " commit";
#endif

                                    }
                                    else if (sOperatorLV == "2")
                                    {
#if MYSQL
                                        sql = "start transaction";
                                        sql += "; update ActiveAlarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; update ResumeAlarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; commit;";
#else
                                        sql = "begin transaction";
                                        sql += " update ActiveAlarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " update ResumeAlarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " commit";
#endif
                                    }

                                    try
                                    {
                                        SaveToDB(sql);
                                        bSucess = true;
                                    }
                                    catch
                                    {
                                        //修改数据库失败，回滚操作，将内存中的修改恢复
                                        lock (m_AlarmTable)
                                        {
                                            if (m_AlarmTable.ContainsKey(nTKSn))
                                            {
                                                TKAlarm tkAlarm = (TKAlarm)m_AlarmTable[nTKSn];

                                                try
                                                {
                                                    tkAlarm.SyncRoot.AcquireWriterLock(-1);
                                                    if (sOperatorLV == "1")
                                                    {
                                                        tkAlarm.OperatorLV11 = "";
                                                        tkAlarm.AckTimeLV1 = "";
                                                    }
                                                    else if (sOperatorLV == "2")
                                                    {
                                                        tkAlarm.OperatorLV21 = "";
                                                        tkAlarm.AckTimeLV2 = "";
                                                    }
                                                }
                                                finally
                                                {
                                                    tkAlarm.SyncRoot.ReleaseWriterLock();
                                                }

                                            }
                                        } // end lock stlstTKAlarm
                                    }
                                    #endregion
                                }
                                else if (bExist)
                                {
                                    //该告警已被其他用户确认
                                    sReason = "告警已被其他用户确认";
                                }
                                else
                                {
                                    //该告警已经恢复或者在内存中不存在
                                    #region 如果数据库活动表中存在, 则尝试更新之
                                    bool bExistInDB = false;
                                    string q;

                                    if (sOperatorLV == "1")
                                    {
                                        q = "select * from activealarm where tksn=" + nTKSn.ToString();
                                        q += " and operatorlv11 is null";
                                    }
                                    else
                                    {
                                        q = "select * from activealarm where tksn=" + nTKSn.ToString();
                                        q += " and operatorlv21 is null";
                                    }

                                    try
                                    {
#if MYSQL
                                        using (MySqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.Text, q))
#else
                                        using (SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.Text, q))
#endif
                                        {
                                            if (dr.HasRows)
                                                bExistInDB = true;
                                            dr.Close();
                                        }
                                    }
                                    catch { }

                                    if (bExistInDB)
                                    {
                                        if (sOperatorLV == "1")
                                        {
#if MYSQL
                                            q = "start transaction";
                                            q += "; update ActiveAlarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += "; commit;";
#else
                                            q = "begin transaction";
                                            q += " update ActiveAlarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += " commit";
#endif
                                        }
                                        else if (sOperatorLV == "2")
                                        {
#if MYSQL
                                            q = "start transaction";
                                            q += "; update ActiveAlarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += "; commit;";
#else
                                            q = "begin transaction";
                                            q += " update ActiveAlarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += " commit";
#endif
                                        }

                                        try
                                        {
                                            SqlHelper.ExecuteNonQuery(ConnString, CommandType.Text, q);
                                            bSucess = true;
                                        }
                                        catch
                                        {
                                            sReason = "内存中不存在, 数据库中存在, 但更新失败.";
                                        }
                                    }
                                    else
                                        sReason = "告警已恢复";
                                    #endregion
                                }
                                #endregion

                                if (bSucess)
                                {
                                    lstSuccessID.Add(nTKSn);
                                }
                                else
                                {
                                    pendingFalse.Add(nTKSn, sReason);
                                }
                            }
                            #endregion

                            /// 历史告警确认步骤：
                            /// 在历史告警表中查找，执行确认操作
                            #region 历史告警批量处理
                            if (resume_queue.Count > 0)
                            {
                                List<StringBuilder> pending = new List<StringBuilder>();
                                StringBuilder sb = new StringBuilder();
                                for (int i = 0; i < resume_queue.Count; ++i)
                                {
                                    sb.Append(resume_queue[i].ToString());
                                    sb.Append(",");

                                    if ((i + 1) % 300 == 0 || i == resume_queue.Count - 1)
                                    {
                                        sb.Remove(sb.Length - 1, 1);
                                        pending.Add(sb);
                                        sb = new StringBuilder();
                                    }
                                }

                                foreach (StringBuilder tksns in pending)
                                {
                                    string q = "";

                                    if (sOperatorLV == "1")
                                    {
#if MYSQL
                                        q = "start transaction";
                                        q += "; select tksn into #tt from ";
                                        q += " ((select tksn from resumealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null) ";
                                        q += " union (select tksn from garbagealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null)) as t"; 
                                        q += " update resumealarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null";
                                        q += " update garbagealarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null";
                                        q += "; select tksn from #tt";
                                        q += "; commit;";
#else
                                        q = "begin transaction";
                                        q += " select tksn into #tt from ";
                                        q += " ((select tksn from resumealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null) ";
                                        q += " union (select tksn from garbagealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null)) as t";
                                        q += " update resumealarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null";
                                        q += " update garbagealarm set OperatorLV11 = '" + sOperatorID + "', AckTimeLV1 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv11 is null";
                                        q += " select tksn from #tt";
                                        q += " commit";
#endif
                                    }
                                    else if (sOperatorLV == "2")
                                    {
#if MYSQL
                                        q = "start transaction";
                                        q += "; select tksn into #tt from ";
                                        q += " ((select tksn from resumealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null)";
                                        q += " union (select tksn from garbagealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null)) as t";
                                        q += " update resumealarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null";
                                        q += " update garbagealarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null";
                                        q += "; select tksn from #tt";
                                        q += "; commit;";
#else
                                        q = "begin transaction";
                                        q += " select tksn into #tt from ";
                                        q += " ((select tksn from resumealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null)";
                                        q += " union (select tksn from garbagealarm where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null)) as t";
                                        q += " update resumealarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null";
                                        q += " update garbagealarm set OperatorLV21 = '" + sOperatorID + "', AckTimeLV2 = '" + sAckTime + "' where tksn in (" + tksns.ToString() + ")";
                                        q += " and operatorlv21 is null";
                                        q += " select tksn from #tt";
                                        q += " commit";
#endif
                                    }
                                    else
                                        continue; // 不正确的操作者级别

                                    DataSet ds = new DataSet();
                                    ds.Tables.Add("temp");

                                    try
                                    {
                                        SqlHelper.FillDataset(m_Connstr, CommandType.Text, q, ds, new string[] { "temp" });
                                    }
                                    catch { }

                                    resume_queue.Sort();
                                    foreach (DataRow r in ds.Tables["temp"].Rows)
                                    {
                                        ulong tksn = Convert.ToUInt64(r["tksn"]);
                                        int idx = resume_queue.BinarySearch(tksn);
                                        resume_queue.RemoveAt(idx);

                                        lstSuccessID.Add(tksn);
                                    }
                                }

                                foreach (ulong tksn in resume_queue)
                                {
                                    pendingFalse.Add(tksn, "告警已被确认.");
                                    //lstFalseID.Add(tksn.ToString() + ", 告警已被确认或不存在.");
                                }
                            }
                            #endregion

                            /// 对于在活动、历史告警表中都不存在的告警，直接按照成功处理
                            #region 不存在的告警返回成功
                            if (pendingFalse.Count > 0)
                            {
                                try
                                {
#if MYSQL
                                    using (MySqlConnection conn = new MySqlConnection(ConnString))
#else
                                    using (SqlConnection conn = new SqlConnection(ConnString))
#endif
                                    {
                                        conn.Open();

                                        DataTable temp = new DataTable();
                                        temp.Columns.Add(new DataColumn("TKSn", typeof(ulong)));

                                        foreach (ulong tksn in pendingFalse.Keys)
                                        {
                                            DataRow r = temp.NewRow();
                                            r["TKSn"] = tksn;
                                            temp.Rows.Add(r);
                                        }
                                        temp.AcceptChanges();

                                        string q = "CREATE temporary TABLE tt (tksn BIGINT PRIMARY KEY)";

                                        SqlHelper.ExecuteNonQuery(conn, CommandType.Text, q);
#if MYSQL
                                        SqlHelper.UpdateDataTable(conn, "select * from tt limit 0,1", temp);
#else
                                        SqlHelper.UpdateDataTable(conn, "select top 1 * from tt", temp);
#endif

                                        DataSet ds = new DataSet();
                                        q = "select tksn from tt where tksn not in (select tksn from activealarm)";
                                        q += " and tksn not in (select tksn from resumealarm)";
                                        q += " and tksn not in (select tksn from garbagealarm)";
                                        SqlHelper.FillDataset(conn, CommandType.Text, q, ds, new string[] { "temp" });

                                        conn.Close();

                                        foreach (DataRow r in ds.Tables["temp"].Rows)
                                        {
                                            ulong tksn = Convert.ToUInt64(r["tksn"]);
                                            lstSuccessID.Add(tksn);

                                            pendingFalse.Remove(tksn);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Instance().SendLog("BL", ex.ToString());
                                }
                            }
                            #endregion

                            //构造广播包					
                            for (int i = 0; i < lstSuccessID.Count; i++)
                            {
                                if (i != lstSuccessID.Count - 1)
                                    sSuccessIDs += lstSuccessID[i].ToString() + ";";
                                else
                                    sSuccessIDs += lstSuccessID[i].ToString();
                            }

                            foreach (KeyValuePair<ulong, string> pair in pendingFalse)
                            {
                                sFalseIDs += (pair.Key.ToString() + "," + pair.Value + ";");
                            }

                            if (sFalseIDs.Length > 1)
                                sFalseIDs = sFalseIDs.Remove(sFalseIDs.Length - 1);

                            lock (m_Clients)
                            {
                                foreach (AlarmClient client in m_Clients.Values)
                                {
                                    if (!client.Authorized)
                                        continue;

                                    CommandMsgV2 broadcastMsg = new CommandMsgV2();
                                    broadcastMsg.TK_CommandType = Constants.TK_CommandType.ALARM_ACK_CHANGE;
                                    broadcastMsg.SeqID = CommandProcessor.AllocateID();
                                    broadcastMsg.SetValue("ClientID", client.ClientID);
                                    broadcastMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                                    broadcastMsg.SetValue("操作员ID", sOperatorID);
                                    broadcastMsg.SetValue("操作员级别", sOperatorLV);
                                    broadcastMsg.SetValue("SUCCESS_ID", sSuccessIDs);
                                    broadcastMsg.SetValue("FALSE_ID", sFalseIDs);
                                    broadcastMsg.SetValue("ACKTIME", sAckTime);

                                    if (client.ClientID == income_clientid)
                                        broadcastMsg.SetValue(Constants.MSG_PARANAME_IMMEDIATE_ID, client.ClientID);
                                    client.AddMessage(broadcastMsg);
                                }
                            }

                            //构造响应包
                            responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                            responseMsg.SeqID = CommandProcessor.AllocateID();
                            responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                            responseMsg.SetValue("SUCCESS_ID", sSuccessIDs);
                            responseMsg.SetValue("FALSE_ID", sFalseIDs);
                            responseMsg.SetValue("ACKTIME", sAckTime);
                        }
                        catch (Exception ex)
                        {
                            responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                            responseMsg.SeqID = CommandProcessor.AllocateID();
                            responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                            responseMsg.SetValue("SUCCESS_ID", sSuccessIDs);
                            responseMsg.SetValue("FALSE_ID", sFalseIDs);
                            responseMsg.SetValue("ACKTIME", sAckTime);
                            responseMsg.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);

                            Logger.Instance().SendLog("BL", ex.ToString());
                        }
                        finally
                        {
                        }
                    }
                    else
                    {
                        return null;
                    }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
                return null;
            }
            finally
            {
            }
            return responseMsg;
        }

        private CommandMsgV2 AlarmReAck(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();

            try
            {
                    if (message.Contains("ClientID"))
                    {
                        long income_clientid = Convert.ToInt64(message.GetValue("ClientID"));

                        List<ulong> lstSuccessID = new List<ulong>();
                        List<string> lstFalseID = new List<string>();
                        //Dictionary<ulong, string> pendingFalse = new Dictionary<ulong, string>();
                        string sSuccessIDs = "";
                        string sFalseIDs = "";
                        string sAckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        try
                        {
                            string sTKSNs = message.GetValue("告警ID").ToString().Trim();
                            string sOperatorID = message.GetValue("操作员ID").ToString().Trim();
                            string sOperatorLV = message.GetValue("操作员级别").ToString().Trim();

                            string[] Alarms = sTKSNs.Split(new char[] { ';' });

                            foreach (string sTKSn in Alarms)
                            {
                                bool bSucess = false;
                                ulong nTKSn = Convert.ToUInt64(sTKSn);

                                #region 活动告警逐条处理
                                string sReason = "";

                                bool bChangeMemory = false;				//是否修改内存成功
                                bool bExist = false;				//是否已被其他用户确认

                                /// 对内存活动告警进行二次确认
                                /// 如果内存中没有，则在数据库中的活动告警表进行二次确认
                                /// 如果仍然找不到，则认为二次确认失败
                                #region step 1. 锁内存告警表，修改数据
                                lock (m_AlarmTable)
                                {
                                    if (m_AlarmTable.ContainsKey(nTKSn))
                                    {
                                        TKAlarm tkAlarm = (TKAlarm)m_AlarmTable[nTKSn];

                                        try
                                        {
                                            tkAlarm.SyncRoot.AcquireWriterLock(-1);

                                            if (sOperatorLV == "1")
                                            {
                                                if (tkAlarm.OperatorLV12 == "")
                                                {
                                                    tkAlarm.OperatorLV12 = sOperatorID;
                                                    tkAlarm.AckAgainTimeLV1 = sAckTime;
                                                    bChangeMemory = true;
                                                }
                                                else
                                                {
                                                    bExist = true;
                                                }
                                            }
                                            else if (sOperatorLV == "2")
                                            {
                                                if (tkAlarm.OperatorLV22 == "")
                                                {
                                                    tkAlarm.OperatorLV22 = sOperatorID;
                                                    tkAlarm.AckAgainTimeLV2 = sAckTime;
                                                    bChangeMemory = true;
                                                }
                                                else
                                                {
                                                    bExist = true;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            tkAlarm.SyncRoot.ReleaseWriterLock();
                                        }
                                    }
                                    else
                                    {
                                        bSucess = false;
                                    }
                                } // end lock stlstTKAlarm
                                #endregion

                                // step 2. 修改数据库
                                if (bChangeMemory)
                                {
                                    //修改数据库
                                    string sql = "";
                                    if (sOperatorLV == "1")
                                    {
#if MYSQL
                                        sql = "start transaction";
                                        sql += "; update ActiveAlarm set OperatorLV12 = '" + sOperatorID + "', AckAgainTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; update ResumeAlarm set OperatorLV12 = '" + sOperatorID + "', AckAgainTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; commit;";
#else
                                        sql = "begin transaction";
                                        sql += " update ActiveAlarm set OperatorLV12 = '" + sOperatorID + "', AckAgainTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " update ResumeAlarm set OperatorLV12 = '" + sOperatorID + "', AckAgainTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " commit";
#endif
                                    }
                                    else if (sOperatorLV == "2")
                                    {
#if MYSQL
                                        sql = "start transaction";
                                        sql += "; update ActiveAlarm set OperatorLV22 = '" + sOperatorID + "', AckAgainTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; update ResumeAlarm set OperatorLV22 = '" + sOperatorID + "', AckAgainTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += "; commit;";
#else
                                        sql = "begin transaction";
                                        sql += " update ActiveAlarm set OperatorLV22 = '" + sOperatorID + "', AckAgainTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " update ResumeAlarm set OperatorLV22 = '" + sOperatorID + "', AckAgainTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                        sql += " commit";
#endif
                                    }
                                    try
                                    {
                                        SaveToDB(sql);
                                        bSucess = true;
                                    }
                                    catch
                                    {
                                        //修改数据库失败，回滚操作，将内存中的修改恢复
                                        lock (m_AlarmTable)
                                        {
                                            if (m_AlarmTable.ContainsKey(nTKSn))
                                            {
                                                TKAlarm tkAlarm = m_AlarmTable[nTKSn];

                                                try
                                                {
                                                    tkAlarm.SyncRoot.AcquireWriterLock(-1);
                                                    if (sOperatorLV == "1")
                                                    {
                                                        tkAlarm.OperatorLV12 = "";
                                                        tkAlarm.AckAgainTimeLV1 = "";
                                                    }
                                                    else if (sOperatorLV == "2")
                                                    {
                                                        tkAlarm.OperatorLV22 = "";
                                                        tkAlarm.AckAgainTimeLV2 = "";
                                                    }
                                                }
                                                finally
                                                {
                                                    tkAlarm.SyncRoot.ReleaseWriterLock();
                                                }

                                            }
                                        } // end lock stlstTKAlarm
                                        //TODO: 要考虑回滚时告警已恢复的情况
                                    }
                                }
                                else if (bExist)
                                {
                                    //该告警已被其他用户确认
                                    sReason = "告警已被其他用户二次确认";
                                }
                                else
                                {
                                    //该告警已经恢复或者在内存中不存在
                                    #region 如果数据库活动表中存在, 则尝试更新之
                                    bool bExistInDB = false;
                                    string q;

                                    if (sOperatorLV == "1")
                                    {
                                        q = "select * from activealarm where tksn=" + nTKSn.ToString();
                                        q += " and operatorlv12 is null";
                                    }
                                    else
                                    {
                                        q = "select * from activealarm where tksn=" + nTKSn.ToString();
                                        q += " and operatorlv22 is null";
                                    }

                                    try
                                    {
#if MYSQL
                                        using (MySqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.Text, q))
#else
                                        using (SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.Text, q))
#endif
                                        {
                                            if (dr.HasRows)
                                                bExistInDB = true;
                                            dr.Close();
                                        }
                                    }
                                    catch { }

                                    if (bExistInDB)
                                    {
                                        if (sOperatorLV == "1")
                                        {
#if MYSQL
                                            q = "start transaction";
                                            q += "; update ActiveAlarm set OperatorLV12 = '" + sOperatorID + "', AckAgainTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += "; commit;";
#else
                                            q = "begin transaction";
                                            q += " update ActiveAlarm set OperatorLV12 = '" + sOperatorID + "', AckAgainTimeLV1 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += " commit";
#endif
                                        }
                                        else if (sOperatorLV == "2")
                                        {
#if MYSQL
                                            q = "start transaction";
                                            q += "; update ActiveAlarm set OperatorLV22 = '" + sOperatorID + "', AckAgainTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += "; commit;";
#else
                                            q = "being transaction";
                                            q += " update ActiveAlarm set OperatorLV22 = '" + sOperatorID + "', AckAgainTimeLV2 = '" + sAckTime + "' where TKSn = " + nTKSn.ToString();
                                            q += " commit";
#endif
                                        }

                                        try
                                        {
                                            SqlHelper.ExecuteNonQuery(ConnString, CommandType.Text, q);
                                            bSucess = true;
                                        }
                                        catch
                                        {
                                            sReason = "内存中不存在, 数据库中存在, 但更新失败.";
                                        }
                                    }
                                    else
                                        sReason = "告警已恢复";
                                    #endregion
                                }
                                #endregion

                                if (bSucess)
                                {
                                    lstSuccessID.Add(nTKSn);
                                }
                                else
                                {
                                    //pendingFalse.Add(nTKSn, sReason);
                                    lstFalseID.Add(nTKSn.ToString() + "," + sReason);
                                }
                            }

                            //构造广播包					
                            for (int i = 0; i < lstSuccessID.Count; i++)
                            {
                                if (i != lstSuccessID.Count - 1)
                                    sSuccessIDs += lstSuccessID[i].ToString() + ";";
                                else
                                    sSuccessIDs += lstSuccessID[i].ToString();
                            }

                            for (int j = 0; j < lstFalseID.Count; j++)
                            {
                                if (j != lstFalseID.Count - 1)
                                    sFalseIDs += lstFalseID[j] + ";";
                                else
                                    sFalseIDs += lstFalseID[j];
                            }

                            lock (m_Clients)
                            {
                                foreach (AlarmClient client in m_Clients.Values)
                                {
                                    if (!client.Authorized)
                                        continue;

                                    CommandMsgV2 broadcastMsg = new CommandMsgV2();
                                    broadcastMsg.TK_CommandType = Constants.TK_CommandType.ALARM_ACK_CHANGE;
                                    broadcastMsg.SeqID = CommandProcessor.AllocateID();
                                    broadcastMsg.SetValue("ClientID", client.ClientID);
                                    broadcastMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                                    broadcastMsg.SetValue("操作员ID", sOperatorID);
                                    broadcastMsg.SetValue("操作员级别", sOperatorLV);
                                    broadcastMsg.SetValue("SUCCESS_ID", sSuccessIDs);
                                    broadcastMsg.SetValue("FALSE_ID", sFalseIDs);
                                    broadcastMsg.SetValue("REACKTIME", sAckTime);

                                    if (client.ClientID == income_clientid)
                                        broadcastMsg.SetValue(Constants.MSG_PARANAME_IMMEDIATE_ID, client.ClientID);
                                    client.AddMessage(broadcastMsg);
                                }
                            }

                            //构造响应包
                            responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                            responseMsg.SeqID = CommandProcessor.AllocateID();
                            responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                            responseMsg.SetValue("SUCCESS_ID", sSuccessIDs);
                            responseMsg.SetValue("FALSE_ID", sFalseIDs);
                            responseMsg.SetValue("REACKTIME", sAckTime);
                        }
                        catch (Exception ex)
                        {
                            responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                            responseMsg.SeqID = CommandProcessor.AllocateID();
                            responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                            responseMsg.SetValue("SUCCESS_ID", sSuccessIDs);
                            responseMsg.SetValue("FALSE_ID", sFalseIDs);
                            responseMsg.SetValue("REACKTIME", sAckTime);
                            responseMsg.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);

                            Logger.Instance().SendLog("BL", ex.ToString());
                        }
                        finally
                        {
                        }
                    }
                    else
                    {
                        return null;
                    }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
                return null;
            }
            finally
            {
            }
            return responseMsg;
        }
        #endregion

        #region 工程管理命令
        private CommandMsgV2 ProjectAdd(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();

            try
            {
                if (message.Contains("ClientID"))
                {
                    string sID = "";

                    lock (m_ProjectInfo)
                    {
                        try
                        {
                            string sProjectType = ((string)message.GetValue("工程类型")).Trim();
                            string sStartTime = ((string)message.GetValue("开始时间")).Trim();
                            string sEndTime = ((string)message.GetValue("结束时间")).Trim();
                            string sNeName = ((string)message.GetValue("网元名称")).Trim();
                            string sRedefinition = ((string)message.GetValue("重定义告警名称")).Trim();
                            string sOperator = ((string)message.GetValue("操作员")).Trim();
                            string sPhoneNo = ((string)message.GetValue("联系电话")).Trim();
                            string sDepartment = ((string)message.GetValue("部门")).Trim();
                            string sReportMsg = ((string)message.GetValue("工程上报信息")).Trim();

                            string sCreatDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            //获取当前数据库中工程ID的最大值
                            string sql = "select max(id) from Maintenance";

                            SqlHelper.FillDataset(m_Connstr, CommandType.Text, sql, m_ProjectInfo, new string[] { "MaxID" });

                            if (m_ProjectInfo.Tables["MaxID"].Rows.Count > 0)
                            {
                                object[] objs = m_ProjectInfo.Tables["MaxID"].Rows[0].ItemArray;

                                if (objs[0] == System.DBNull.Value)
                                    sID = "1";
                                else
                                {
                                    int nID = (int)objs[0] + 1;
                                    sID = nID.ToString();
                                }
                            }
                            else
                                sID = "1";

                            //m_ProjectInfo.Tables.Remove("MaxID");

                            sql = "insert into Maintenance (type, start_time, end_time, ne_name, create_date, operator, phone_no, department, report_msg, redefinition)";
                            sql += " values('" + sProjectType + "', '" + sStartTime + "', '" + sEndTime + "', '" + sNeName.Replace("'", "''") + "', '" + sCreatDate + "', '" + sOperator + "', '" + sPhoneNo + "', '" + sDepartment + "', '" + sReportMsg + "', '" + sRedefinition.Replace("'", "''") + "')";

                            SaveToDB(sql);

                            //将新工程添加到内存表中
                            //object[] newProject = { sID, sProjectType, sStartTime, sEndTime, sNeName, sObjName, sCreatDate, sOperator, sPhoneNo, sDepartment, sReportMsg };

                            object[] newProject = { sID, sProjectType, sStartTime, sEndTime, sNeName, sRedefinition, sCreatDate, sOperator, sPhoneNo, sDepartment, sReportMsg };

                            lock (m_ProjectInfo.Tables["ProjectInfo"])
                            {
                                m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();
                                m_ProjectInfo.Tables["ProjectInfo"].Rows.Add(newProject);
                                m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();
                            }

                            //将一组工程信息添加到工程过滤器组中
                            FilterEx filter = new FilterEx(sID);
                            filter.BSCes = Utilities.ConvertString2StrArr(sNeName);
                            filter.Redefinition = Utilities.ConvertString2StrArr(sRedefinition);
                            filter.StartTime = DateTime.Parse(sStartTime);
                            filter.EndTime = DateTime.Parse(sEndTime);
                            filter.AddInfo = sReportMsg;

                            lock (m_FilterGroup.AllFilters.SyncRoot)
                                m_FilterGroup.addFilter(filter);

                            responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance().SendLog("BL", ex.ToString());
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                            responseMsg.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);

                            //添加失败，删除记录
                            string sql = "delete from Maintenance where id = " + sID;
                            try
                            {
                                SaveToDB(sql);
                            }
                            catch (Exception ex1)
                            {
                                Logger.Instance().SendLog("BL", ex1.ToString());
                            }
                        }
                        finally
                        {
                            m_ProjectInfo.Tables.Remove("MaxID");
                            long clientID = (long)message.GetValue("ClientID");

                            responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                            responseMsg.SeqID = CommandProcessor.AllocateID();
                            responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());

                responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                responseMsg.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
            }
            finally
            {
            }
            return responseMsg;
        }

        private CommandMsgV2 ProjectModify(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();

            bool bModifySucess = false;				//是否再内存中修改成功

            try
            {
                if (message.Contains("ClientID"))
                {
                    if (message.Contains("工程编号"))
                    {
                        string sID = "";

                        lock (m_ProjectInfo)
                        {
                            try
                            {
                                sID = ((string)message.GetValue("工程编号")).Trim();

                                string sProjectType = ((string)message.GetValue("工程类型")).Trim();
                                string sStartTime = ((string)message.GetValue("开始时间")).Trim();
                                string sEndTime = ((string)message.GetValue("结束时间")).Trim();
                                string sNeName = ((string)message.GetValue("网元名称")).Trim();
                                string sRedefinition = ((string)message.GetValue("重定义告警名称")).Trim();
                                string sOperator = ((string)message.GetValue("操作员")).Trim();
                                string sPhoneNo = ((string)message.GetValue("联系电话")).Trim();
                                string sDepartment = ((string)message.GetValue("部门")).Trim();
                                string sReportMsg = ((string)message.GetValue("工程上报信息")).Trim();

                                //更新内存表中的工程信息
                                lock (m_ProjectInfo.Tables["ProjectInfo"])
                                {
                                    m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();
                                    DataRow[] drs = m_ProjectInfo.Tables["ProjectInfo"].Select("id=" + sID);
                                    foreach (DataRow dr in drs)
                                    {
                                        dr["type"] = sProjectType;
                                        dr["start_time"] = sStartTime;
                                        dr["end_time"] = sEndTime;
                                        dr["ne_name"] = sNeName;
                                        dr["redefinition"] = sRedefinition;
                                        dr["operator"] = sOperator;
                                        dr["phone_no"] = sPhoneNo;
                                        dr["department"] = sDepartment;
                                        dr["report_msg"] = sReportMsg;
                                    }

                                    m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();
                                }

                                bModifySucess = true;

                                //更新数据库中的告警信息
                                string sql = "update Maintenance set ";
                                sql += "type = '" + sProjectType + "',";
                                sql += "start_time = '" + sStartTime + "',";
                                sql += "end_time = '" + sEndTime + "',";
                                sql += "ne_name = '" + sNeName + "',";
                                sql += "redefinition = '" + sRedefinition + "',";
                                sql += "operator = '" + sOperator + "',";
                                sql += "phone_no = '" + sPhoneNo + "',";
                                sql += "department = '" + sDepartment + "',";
                                sql += "report_msg = '" + sReportMsg + "'";
                                sql += " where id =" + sID;

                                SaveToDB(sql);

                                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                            }
                            catch
                            {
                                //如果内存更新成功、数据库更新失败，则重新到数据库中读取记录，更新内存
                                if (bModifySucess)
                                {
                                    //string sql = "select id, type, start_time, end_time, ne_name, obj_name, create_date, operator, phone_no, department, report_msg from Maintenance where id = " + sID;
                                    string sql = "select id, type, start_time, end_time, ne_name, redefinition, create_date, operator, phone_no, department, report_msg from Maintenance where id = " + sID;

                                    SqlHelper.FillDataset(m_Connstr, CommandType.Text, sql, m_ProjectInfo, new string[] { "Project" });

                                    if (m_ProjectInfo.Tables["Project"].Rows.Count > 0)
                                    {
                                        lock (m_ProjectInfo.Tables["ProjectInfo"])
                                        {

                                            DataRow[] drs = m_ProjectInfo.Tables["ProjectInfo"].Select("id=" + sID);
                                            foreach (DataRow dr in drs)
                                            {
                                                dr.Delete();
                                            }

                                            DataRow drNew = m_ProjectInfo.Tables["Project"].NewRow();
                                            drNew = m_ProjectInfo.Tables["Project"].Rows[0];

                                            m_ProjectInfo.Tables["ProjectInfo"].Rows.Add(drNew);
                                            m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();

                                        }

                                        m_ProjectInfo.Tables.Remove("Project");
                                    }
                                }

                                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");

                            }
                            finally
                            {
                            }
                        } // end lock
                    }
                    else
                    {
                        responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                    }

                    responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                    responseMsg.SeqID = CommandProcessor.AllocateID();
                    responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                    responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());

                responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                responseMsg.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
            }
            finally
            {
            }
            return responseMsg;
        }

        private CommandMsgV2 ProjectRemove(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();

            bool deleteSucess = false;			//是否成功从内存中删除
            string sID = "";

            lock (m_ProjectInfo)
            {
                try
                {
                    if (message.Contains("ClientID"))
                    {
                        sID = ((string)message.GetValue("工程编号")).Trim();

                        //从内存中删除记录
                        lock (m_ProjectInfo.Tables["ProjectInfo"])
                        {
                            m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();

                            DataRow[] drs = m_ProjectInfo.Tables["ProjectInfo"].Select("id=" + sID);

                            foreach (DataRow dr in drs)
                            {
                                dr.Delete();
                            }

                            m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();
                        }

                        lock (m_FilterGroup.AllFilters.SyncRoot)
                            m_FilterGroup.deleteFilter(sID);

                        deleteSucess = true;


                        string sql = "delete from Maintenance where id = " + sID;
                        SaveToDB(sql);

                        responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                        responseMsg.SeqID = CommandProcessor.AllocateID();
                        responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                        responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                        responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    if (deleteSucess)
                    {
                        try
                        {
                            //若已从内存中删除，再添加进来
                            //string sql = "select id, type, start_time, end_time, ne_name, obj_name, create_date, operator, phone_no, department, report_msg from Maintenance where id = " + sID;
                            string sql = "select id, type, start_time, end_time, ne_name, redefinition, create_date, operator, phone_no, department, report_msg from Maintenance where id = " + sID;

                            SqlHelper.FillDataset(m_Connstr, CommandType.Text, sql, m_ProjectInfo, new string[] { "Project" });

                            if (m_ProjectInfo.Tables["Project"].Rows.Count > 0)
                            {
                                lock (m_ProjectInfo.Tables["ProjectInfo"])
                                {
                                    m_ProjectInfo.Tables["ProjectInfo"].Rows.Add(m_ProjectInfo.Tables["Project"].Rows[0]);
                                    m_ProjectInfo.Tables["ProjectInfo"].AcceptChanges();
                                }

                                m_ProjectInfo.Tables.Remove("Project");

                                DataRow dr = m_ProjectInfo.Tables["Project"].Rows[0];
                                FilterEx filter = new FilterEx(dr["id"].ToString().Trim());
                                filter.BSCes = Utilities.ConvertString2StrArr(dr["ne_name"].ToString().Trim());
                                filter.Redefinition = Utilities.ConvertString2StrArr(dr["Redefinition"].ToString().Trim());
                                filter.StartTime = DateTime.Parse(dr["start_time"].ToString());
                                filter.EndTime = DateTime.Parse(dr["end_time"].ToString());

                                lock (m_FilterGroup.AllFilters.SyncRoot)
                                    m_FilterGroup.addFilter(filter);
                            }
                        }
                        catch (Exception ex1)
                        {
                            Logger.Instance().SendLog("BL", "工程信息恢复失败:" + ex1.ToString());
                        }
                    }

                    Logger.Instance().SendLog("BL", ex.ToString());

                    responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                    responseMsg.SeqID = CommandProcessor.AllocateID();
                    responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                    responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                    responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                    responseMsg.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);
                }
                finally
                {
                }
            } // end lock projeci info

            return responseMsg;
        }

        private object m_lockProject = new Int32();
        private bool m_bProject = false;
        private void timerQueryProjectInfo_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_lockProject)
            {
                if (m_bProject)
                    return;
                else
                    m_bProject = true;
            }

            if (!timerQueryProjectInfo.Enabled)
                return;

            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);

                //存放超出工程区间告警的流水号
                List<ulong> lstTKSN = new List<ulong>();

                //遍历内存中的活动告警表，求出超出工程区间告警
                lock (m_AlarmTable)
                {
                    foreach (TKAlarm tkAlarm in m_AlarmTable.Values)
                    {
                        try
                        {
                            tkAlarm.SyncRoot.AcquireWriterLock(-1);

                            if (tkAlarm.ProjectInfo != "" && tkAlarm.ProjectEndTime != "")
                                this.GetType();

                            if (tkAlarm.ProjectInfo == "" || tkAlarm.ProjectTimeOut == "1" || tkAlarm.ProjectEndTime == "")
                                continue;

                            try
                            {
                                if (DateTime.Compare(DateTime.Now, DateTime.Parse(tkAlarm.ProjectEndTime)) > 0)
                                {
                                    tkAlarm.ProjectTimeOut = "1";
                                    lstTKSN.Add(tkAlarm.TKSn);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance().SendLog("BL", ex.ToString() + tkAlarm.ProjectEndTime);
                            }
                        }
                        finally
                        {
                            tkAlarm.SyncRoot.ReleaseWriterLock();
                        }
                    }
                }

                lock (m_Clients)
                {
                    foreach (AlarmClient client in m_Clients.Values)
                    {
                        if (!client.Authorized)
                            continue;

                        foreach (ulong tksn in lstTKSN)
                        {
                            string sTKSn = tksn.ToString();

                            CommandMsgV2 boardcaseMsg = new CommandMsgV2();
                            boardcaseMsg.TK_CommandType = Constants.TK_CommandType.ALARM_PROJECT_CHANGE;
                            boardcaseMsg.SeqID = CommandProcessor.AllocateID();
                            boardcaseMsg.SetValue("ClientID", client.ClientID);
                            boardcaseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                            boardcaseMsg.SetValue("集中告警流水号", sTKSn);

                            client.AddMessage(boardcaseMsg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();

                lock (m_lockProject)
                {
                    m_bProject = false;
                }
            }
        }
        #endregion

        #region 派单相关

        /// <summary>
        /// 客户端派单命令处理——已经废弃
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private CommandMsgV2 SendOrder(ICommunicationMessage message)
        {
            CommandMsgV2 responseMsg = new CommandMsgV2();
            bool bSucess = false;

            try
            {
                    if (message.Contains("ClientID"))
                    {
                        long income_clientid = Convert.ToInt64(message.GetValue("ClientID"));

                        try
                        {
                            string sTKSn = ((string)message.GetValue("告警ID")).Trim();
                            string sAlarmState = ((string)message.GetValue("告警状态")).Trim();
                            string sOrderOperator = ((string)message.GetValue("派单人")).Trim();
                            string sOrderLevel = ((string)message.GetValue("派单人级别")).Trim();
                            string sOrderID = ((string)message.GetValue("派单号")).Trim();
                            string sOrderTime = ((string)message.GetValue("派单时间")).Trim();

                            ulong nTKSn = Convert.ToUInt64(sTKSn);

                            bool bChangMemory = false;
                            bool bFound = false;

                            if (sAlarmState == "0")
                            {
                                lock (m_AlarmTable)
                                {
                                    if (m_AlarmTable.ContainsKey(nTKSn))
                                    {
                                        bFound = true;

                                        TKAlarm tkAlarm = (TKAlarm)m_AlarmTable[nTKSn];

                                        try
                                        {
                                            tkAlarm.SyncRoot.AcquireWriterLock(-1);
                                            if (sOrderLevel == "1")
                                            {
                                                //if(tkAlarm.OrderIDLV1 == "")
                                                //{
                                                tkAlarm.OrderIDLV1 = sOrderID;
                                                tkAlarm.OrderOperatorLV1 = sOrderOperator;
                                                tkAlarm.OrderTimeLV1 = sOrderTime;
                                                bChangMemory = true;
                                                //}
                                            }
                                            else if (sOrderLevel == "2")
                                            {
                                                //if(tkAlarm.OrderIDLV2 == "")
                                                //{
                                                tkAlarm.OrderIDLV2 = sOrderID;
                                                tkAlarm.OrderOperatorLV2 = sOrderOperator;
                                                tkAlarm.OrderTimeLV2 = sOrderTime;
                                                bChangMemory = true;
                                                //}
                                            }
                                        }
                                        finally
                                        {
                                            tkAlarm.SyncRoot.ReleaseWriterLock();
                                        }
                                    }
                                }

                                if (!bFound)
                                {
                                    //内存中没找到该告警，将告警状态改为恢复告警
                                    sAlarmState = "1";
                                }
                                else if (bChangMemory)
                                {
                                    string sql = "";
                                    if (sOrderLevel == "1")
                                    {
#if MYSQL
                                        sql = "start transaction";
                                        sql += "; update ActiveAlarm set OrderOperatorLV1 = '" + sOrderOperator + "', OrderIDLV1 = '" + sOrderID + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += "; update ResumeAlarm set OrderOperatorLV1 = '" + sOrderOperator + "', OrderIDLV1 = '" + sOrderID + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += "; update GarbageAlarm set OrderOperatorLV1 = '" + sOrderOperator + "', OrderIDLV1 = '" + sOrderID + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += "; commit;";
#else
                                        sql = "being transaction";
                                        sql += " update ActiveAlarm set OrderOperatorLV1 = '" + sOrderOperator + "', OrderIDLV1 = '" + sOrderID + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += " update ResumeAlarm set OrderOperatorLV1 = '" + sOrderOperator + "', OrderIDLV1 = '" + sOrderID + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += " update GarbageAlarm set OrderOperatorLV1 = '" + sOrderOperator + "', OrderIDLV1 = '" + sOrderID + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += " commit";
#endif
                                    }
                                    else if (sOrderLevel == "2")
                                    {
#if MYSQL
                                        sql = "start transaction";
                                        sql += "; update ActiveAlarm set OrderOperatorLV2 = '" + sOrderOperator + "', OrderIDLV2 = '" + sOrderID + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += "; update ResumeAlarm set OrderOperatorLV2 = '" + sOrderOperator + "', OrderIDLV2 = '" + sOrderID + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += "; update GarbageAlarm set OrderOperatorLV2 = '" + sOrderOperator + "', OrderIDLV2 = '" + sOrderID + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += "; commit;";
#else
                                        sql = "begin transaction";
                                        sql += " update ActiveAlarm set OrderOperatorLV2 = '" + sOrderOperator + "', OrderIDLV2 = '" + sOrderID + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += " update ResumeAlarm set OrderOperatorLV2 = '" + sOrderOperator + "', OrderIDLV2 = '" + sOrderID + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += " update GarbageAlarm set OrderOperatorLV2 = '" + sOrderOperator + "', OrderIDLV2 = '" + sOrderID + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sql += " commit";
#endif

                                    }
                                    try
                                    {
                                        SaveToDB(sql);
                                        bSucess = true;
                                    }
                                    catch
                                    {
                                        bFound = false;

                                        //更新数据库出错，恢复内存数据
                                        lock (m_AlarmTable)
                                        {
                                            if (m_AlarmTable.ContainsKey(nTKSn))
                                            {
                                                TKAlarm tkAlarm = m_AlarmTable[nTKSn];
                                                bFound = true;

                                                try
                                                {
                                                    tkAlarm.SyncRoot.AcquireWriterLock(-1);

                                                    if (sOrderLevel == "1")
                                                    {
                                                        tkAlarm.OrderIDLV1 = "";
                                                        tkAlarm.OrderOperatorLV1 = "";
                                                        tkAlarm.OrderTimeLV1 = "";
                                                    }
                                                    else if (sOrderLevel == "2")
                                                    {
                                                        tkAlarm.OrderIDLV2 = "";
                                                        tkAlarm.OrderOperatorLV2 = "";
                                                        tkAlarm.OrderTimeLV2 = "";
                                                    }
                                                }
                                                finally
                                                {
                                                    tkAlarm.SyncRoot.ReleaseWriterLock();
                                                }
                                            }
                                        } // end lock table

                                        if (!bFound)
                                        {
                                            //恢复内存数据时，该告警已恢复，已从内存中移除，需更新数据库
                                            string sqla = "";
                                            if (sOrderLevel == "1")
                                            {
#if MYSQL
                                                sqla = "start transaction";
                                                sqla += "; update ResumeAlarm  set OrderIDLV1 = null, OrderOperatorLV1 = null, OrderTimeLV1 = null where TKSn = " + sTKSn;
                                                sqla += "; update GarbageAlarm  set OrderIDLV1 = null, OrderOperatorLV1 = null, OrderTimeLV1 = null where TKSn = " + sTKSn;
                                                sqla += "; commit;";
#else
                                                sqla = "begin transaction";
                                                sqla += " update ResumeAlarm  set OrderIDLV1 = null, OrderOperatorLV1 = null, OrderTimeLV1 = null where TKSn = " + sTKSn;
                                                sqla += " update GarbageAlarm  set OrderIDLV1 = null, OrderOperatorLV1 = null, OrderTimeLV1 = null where TKSn = " + sTKSn;
                                                sqla += " commit";
#endif
                                            }
                                            else if (sOrderLevel == "2")
                                            {
#if MYSQL
                                                sqla = "start transaction";
                                                sqla += "; update ResumeAlarm set OrderIDLV2 = null, OrderOperatorLV2 = null, OrderTimeLV2 = null where TKSn = " + sTKSn;
                                                sqla += "; update GarbageAlarm set OrderIDLV2 = null, OrderOperatorLV2 = null, OrderTimeLV2 = null where TKSn = " + sTKSn;
                                                sqla += "; commit;";
#else
                                                sqla = "begin transaction";
                                                sqla += " update ResumeAlarm set OrderIDLV2 = null, OrderOperatorLV2 = null, OrderTimeLV2 = null where TKSn = " + sTKSn;
                                                sqla += " update GarbageAlarm set OrderIDLV2 = null, OrderOperatorLV2 = null, OrderTimeLV2 = null where TKSn = " + sTKSn;
                                                sqla += " commit";
#endif
                                            }

                                            try
                                            {
                                                SaveToDB(sqla);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Instance().SendLog("BL", ex.ToString());
                                            }
                                        }

                                        responseMsg.SetValue("REASON", "");
                                    }
                                }

                            }

                            //上面代码会改变sAlarmState的值，故写成2个if语句，而不用if...else...
                            if (sAlarmState == "1")
                            {
                                //string sql = "select OrderIDLV1, OrderIDLV2 from ResumeAlarm where TKSn = " + sTKSn;

                                try
                                {
                                    string sqla = "";

                                    if (sOrderLevel == "1")
                                    {
#if MYSQL
                                        sqla = "start transaction";
                                        sqla += "; update ResumeAlarm set OrderIDLV1 = '" + sOrderID + "', OrderOperatorLV1 = '" + sOrderOperator + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += "; update GarbageAlarm set OrderIDLV1 = '" + sOrderID + "', OrderOperatorLV1 = '" + sOrderOperator + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += "; commit;";
#else
                                        sqla = "begin transaction";
                                        sqla += " update ResumeAlarm set OrderIDLV1 = '" + sOrderID + "', OrderOperatorLV1 = '" + sOrderOperator + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += " update GarbageAlarm set OrderIDLV1 = '" + sOrderID + "', OrderOperatorLV1 = '" + sOrderOperator + "', OrderTimeLV1 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += " commit;";
#endif
                                    }
                                    else if (sOrderLevel == "2")
                                    {
#if MYSQL
                                        sqla = "start transaction";
                                        sqla += "; update ResumeAlarm set OrderIDLV2 = '" + sOrderID + "', OrderOperatorLV2 = '" + sOrderOperator + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += "; update GarbageAlarm set OrderIDLV2 = '" + sOrderID + "', OrderOperatorLV2 = '" + sOrderOperator + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += "; commit;";
#else
                                        sqla = "begin transaction";
                                        sqla += " update ResumeAlarm set OrderIDLV2 = '" + sOrderID + "', OrderOperatorLV2 = '" + sOrderOperator + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += " update GarbageAlarm set OrderIDLV2 = '" + sOrderID + "', OrderOperatorLV2 = '" + sOrderOperator + "', OrderTimeLV2 = '" + sOrderTime + "' where TKSn = " + sTKSn;
                                        sqla += " commit";
#endif
                                    }

                                    //if(bFound)
                                    //{
                                    try
                                    {
                                        SaveToDB(sqla);
                                        bSucess = true;
                                    }
                                    catch (Exception ex2)
                                    {
                                        Logger.Instance().SendLog("BL", ex2.ToString());
                                        responseMsg.SetValue("REASON", "");
                                    }
                                    //}
                                }
                                catch (Exception ex)
                                {
                                    Logger.Instance().SendLog("BL", ex.ToString());
                                }
                            }

                            if (bSucess)
                            {
                                lock (m_Clients)
                                {
                                    foreach (AlarmClient client in m_Clients.Values)
                                    {
                                        if (!client.Authorized)
                                            continue;

                                        //构造一条广播命令，通知所有客户端确认告警
                                        CommandMsgV2 broadcastMsg = new CommandMsgV2();
                                        broadcastMsg.TK_CommandType = Constants.TK_CommandType.ALARM_ORDER_CHANGE;
                                        broadcastMsg.SeqID = CommandProcessor.AllocateID();
                                        broadcastMsg.SetValue("ClientID", client.ClientID);
                                        broadcastMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                                        broadcastMsg.SetValue("集中告警流水号", sTKSn);

                                        if (income_clientid == client.ClientID)
                                            broadcastMsg.SetValue(Constants.MSG_PARANAME_IMMEDIATE_ID, client.ClientID);

                                        if (sOrderLevel == "1")
                                        {
                                            broadcastMsg.SetValue("派单人LV1", sOrderOperator);
                                            broadcastMsg.SetValue("派单号LV1", sOrderID);
                                            broadcastMsg.SetValue("派单时间LV1", sOrderTime);
                                        }
                                        else if (sOrderLevel == "2")
                                        {
                                            broadcastMsg.SetValue("派单人LV2", sOrderOperator);
                                            broadcastMsg.SetValue("派单号LV2", sOrderID);
                                            broadcastMsg.SetValue("派单时间LV2", sOrderTime);
                                        }

                                        client.AddMessage(broadcastMsg);
                                    }
                                }

                                responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                                responseMsg.SeqID = CommandProcessor.AllocateID();
                                responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                                responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                            }
                            else
                            {
                                responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                                responseMsg.SeqID = CommandProcessor.AllocateID();
                                responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                                responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                                responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                            }
                        }
                        catch (Exception ex)
                        {
                            responseMsg.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                            responseMsg.SeqID = CommandProcessor.AllocateID();
                            responseMsg.SetValue("ClientID", message.GetValue("ClientID"));
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESPONSE_TO, message.SeqID);
                            responseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "NOK");
                            responseMsg.SetValue(Constants.MSG_PARANAME_REASON, ex.Message);

                            Logger.Instance().SendLog("BL", ex.ToString());
                        }
                    }
                    else
                    {
                        return null;
                    }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
                return null;
            }
            finally
            {
            }

            return responseMsg;
        }

        private object m_lockOrder = new Int32();
        private bool m_bOrder = false;
        private void timerGetOrderInfo_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_lockOrder)
            {
                if (m_bOrder)
                    return;
                else
                    m_bOrder = true;
            }

            if (!timerGetOrderInfo.Enabled)
                return;

            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);

                string sql = "select id, tksn, operator, operatorlevel, taskid, alarmCreateTime from sendorder_senddata";

                DataSet dsOrder;
                dsOrder = SqlHelper.ExecuteDataset(m_Connstr, CommandType.Text, sql);

                if (dsOrder.Tables[0].Rows.Count > 0)
                {
                    string maxid = dsOrder.Tables[0].Rows[0][0].ToString().Trim();

                    foreach (DataRow dr in dsOrder.Tables[0].Rows)
                    {
                        string sID = Convert.ToString(dr["id"]).Trim();
                        string sTksn = Convert.ToString(dr["tksn"]).Trim();
                        string sOperator = Convert.ToString(dr["operator"]).Trim();
                        string sOperatorLevel = Convert.ToString(dr["operatorlevel"]).Trim();
                        string sTaskid = Convert.ToString(dr["taskid"]).Trim();
                        string sAlarmCreateTime = Convert.ToString(dr["alarmCreateTime"]).Trim();

                        if (sTksn == "")
                            continue;

                        if (sOperatorLevel == "1")
                        {
                            sql = "update Activealarm set OrderOperatorLV1 = '" + sOperator + "', OrderIDLV1 = '" + sTaskid + "', OrderTimeLV1 = '" + sAlarmCreateTime + "' where tksn = " + sTksn;
                            sql += " update Resumealarm set OrderOperatorLV1 = '" + sOperator + "', OrderIDLV1 = '" + sTaskid + "', OrderTimeLV1 = '" + sAlarmCreateTime + "' where tksn = " + sTksn;
                            sql += " update Garbagealarm set OrderOperatorLV1 = '" + sOperator + "', OrderIDLV1 = '" + sTaskid + "', OrderTimeLV1 = '" + sAlarmCreateTime + "' where tksn = " + sTksn;
                        }
                        else
                        {
                            sql = "update Activealarm set OrderOperatorLV2 = '" + sOperator + "', OrderIDLV2 = '" + sTaskid + "', OrderTimeLV2 = '" + sAlarmCreateTime + "' where tksn = " + sTksn;
                            sql = " update Resumealarm set OrderOperatorLV2 = '" + sOperator + "', OrderIDLV2 = '" + sTaskid + "', OrderTimeLV2 = '" + sAlarmCreateTime + "' where tksn = " + sTksn;
                            sql = " update Garbagealarm set OrderOperatorLV2 = '" + sOperator + "', OrderIDLV2 = '" + sTaskid + "', OrderTimeLV2 = '" + sAlarmCreateTime + "' where tksn = " + sTksn;
                        }

                        //更新数据库
                        SqlHelper.ExecuteNonQuery(m_Connstr, CommandType.Text, sql);

                        //更新内存
                        lock (m_AlarmTable)
                        {
                            if (m_AlarmTable.ContainsKey(Convert.ToUInt64(sTksn)))
                            {
                                TKAlarm tkAlarm = m_AlarmTable[Convert.ToUInt64(sTksn)];

                                try
                                {
                                    tkAlarm.SyncRoot.AcquireWriterLock(-1);

                                    if (sOperatorLevel == "1")
                                    {
                                        tkAlarm.OrderOperatorLV1 = sOperator;
                                        tkAlarm.OrderIDLV1 = sTaskid;
                                        tkAlarm.OrderTimeLV1 = sAlarmCreateTime;
                                    }
                                    else
                                    {
                                        tkAlarm.OrderOperatorLV2 = sOperator;
                                        tkAlarm.OrderIDLV2 = sTaskid;
                                        tkAlarm.OrderTimeLV2 = sAlarmCreateTime;
                                    }
                                }
                                finally
                                {
                                    tkAlarm.SyncRoot.ReleaseWriterLock();
                                }
                            }
                        }

                        //发广播包，同步客户端数据
                        lock (m_Clients)
                        {
                            foreach (AlarmClient client in m_Clients.Values)
                            {
                                if (!client.Authorized)
                                    continue;

                                CommandMsgV2 boardcaseMsg = new CommandMsgV2();
                                boardcaseMsg.TK_CommandType = Constants.TK_CommandType.ALARM_ORDER_CHANGE;
                                boardcaseMsg.SeqID = CommandProcessor.AllocateID();
                                boardcaseMsg.SetValue("ClientID", client.ClientID);
                                boardcaseMsg.SetValue(Constants.MSG_PARANAME_RESULT, "OK");
                                boardcaseMsg.SetValue("集中告警流水号", sTksn);

                                if (sOperatorLevel == "1")
                                {
                                    boardcaseMsg.SetValue("派单人LV1", sOperator);
                                    boardcaseMsg.SetValue("派单号LV1", sTaskid);
                                    boardcaseMsg.SetValue("派单时间LV1", sAlarmCreateTime);
                                }
                                else if (sOperatorLevel == "2")
                                {
                                    boardcaseMsg.SetValue("派单人LV2", sOperator);
                                    boardcaseMsg.SetValue("派单号LV2", sTaskid);
                                    boardcaseMsg.SetValue("派单时间LV2", sAlarmCreateTime);
                                }

                                client.AddMessage(boardcaseMsg);
                            }
                        }

                        //删除派单记录
                        sql = "delete from sendorder_senddata where id = " + sID;
                        SqlHelper.ExecuteNonQuery(m_Connstr, CommandType.Text, sql);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();
                m_bOrder = false;
            }
        }
        #endregion

        #region 启动时，获取库中所有活动告警
        /// <summary>
        /// 读取数据库中已存在的活动告警
        /// </summary>
        private void GetExistAlarm()
        {
            //return;
            string sql = "select * from activealarm";

            try
            {
#if MYSQL
                using (MySqlDataReader dr = SqlHelper.ExecuteReader(m_Connstr, CommandType.Text, sql))
#else
                using (SqlDataReader dr = SqlHelper.ExecuteReader(m_Connstr, CommandType.Text, sql))
#endif
                {
                    while (dr.Read())
                    {
                        TKAlarm tkAlarm = new TKAlarm();
                        tkAlarm.TKSn = Convert.ToUInt64(dr["TKsn"].ToString());
                        tkAlarm.ManuSn = dr["ManuSn"].ToString().Trim();
                        tkAlarm.City = dr["City"].ToString().Trim();
                        tkAlarm.Manufacturer = dr["Manufacturer"].ToString().Trim();
                        tkAlarm.BusinessType = dr["BusinessType"].ToString().Trim();
                        tkAlarm.NeName = dr["NeName"].ToString().Trim();
                        tkAlarm.NeType = dr["NeType"].ToString().Trim();
                        tkAlarm.ObjName = dr["ObjName"].ToString().Trim();
                        tkAlarm.ObjType = dr["ObjType"].ToString().Trim();
                        tkAlarm.AlarmName = dr["AlarmName"].ToString().Trim();
                        tkAlarm.Redefinition = dr["Redefinition"].ToString().Trim();
                        tkAlarm.Severity = dr["Severity"].ToString().Trim();
                        tkAlarm.OccurTime = Convert.ToDateTime(dr["OccurTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                        tkAlarm.AckTimeLV1 = dr["AckTimeLV1"].ToString().Trim();
                        tkAlarm.AckAgainTimeLV1 = dr["AckAgainTimeLV1"].ToString().Trim();
                        tkAlarm.AckTimeLV2 = dr["AckTimeLV2"].ToString().Trim();
                        tkAlarm.AckAgainTimeLV2 = dr["AckAgainTimeLV2"].ToString().Trim();
                        tkAlarm.ClearTime = dr["ClearTime"].ToString() != "" ? Convert.ToDateTime(dr["ClearTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "";						
                        tkAlarm.Location = dr["Location"].ToString().Trim();
                        tkAlarm.OperatorLV11 = dr["OperatorLV11"].ToString().Trim();
                        tkAlarm.OperatorLV12 = dr["OperatorLV12"].ToString().Trim();
                        tkAlarm.OperatorLV21 = dr["OperatorLV21"].ToString().Trim();
                        tkAlarm.OperatorLV22 = dr["OperatorLV22"].ToString().Trim();
                        tkAlarm.ProjectInfo = dr["ProjectInfo"].ToString().Trim();
                        tkAlarm.OrderOperatorLV1 = dr["OrderOperatorLV1"].ToString().Trim();
                        tkAlarm.OrderIDLV1 = dr["OrderIDLV1"].ToString().Trim();
                        tkAlarm.OrderTimeLV1 = dr["OrderTimeLV1"].ToString().Trim();
                        tkAlarm.OrderOperatorLV2 = dr["OrderOperatorLV2"].ToString().Trim();
                        tkAlarm.OrderIDLV2 = dr["OrderIDLV2"].ToString().Trim();
                        tkAlarm.OrderTimeLV2 = dr["OrderTimeLV2"].ToString().Trim();
                        tkAlarm.OMCName = dr["OMCName"].ToString().Trim();
                        tkAlarm.Reserved2 = dr["Reserved2"].ToString().Trim();
                        tkAlarm.Reserved3 = dr["Reserved3"].ToString().Trim();
                        tkAlarm.ReceiveTime = dr["ReceiveTime"].ToString() != "" ? Convert.ToDateTime(dr["ReceiveTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "";

                        lock (m_FilterGroup.AllFilters.SyncRoot)
                            m_FilterGroup.isMatched(ref tkAlarm);  //新版本区别老版本,从数据库中取出告警需要判断工程超时
                        if (tkAlarm.ProjectInfo != "" && tkAlarm.ProjectEndTime != "")
                        {
                            try
                            {
                                if (DateTime.Compare(DateTime.Now, DateTime.Parse(tkAlarm.ProjectEndTime)) > 0)
                                {
                                    tkAlarm.ProjectTimeOut = "1";
                                }
                            }
                            catch
                            { }
                        }
                        lock (m_AlarmTable)
                        {
                            ////采集自身告警不保存在内存中
                            //if (tkAlarm.Manufacturer != "TK")
                            m_AlarmTable.Add(tkAlarm.TKSn, tkAlarm);
                        }
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance().SendLog("BL", ex.ToString());
            }
            finally
            {
            }
        }
        #endregion

        #region 分配TKSN
        private static ulong m_CurTKSN = 0;
        private static ulong m_MaxTKSN = 0;
        private static ulong m_AllocationStep = 0;
        private static object m_LockTKSN = new int();
        public ulong AllocateTKSN()
        {
            lock (m_LockTKSN)
            {
                if (m_CurTKSN < m_MaxTKSN)
                    return ++m_CurTKSN;
                else // 从数据库中分配TKSN
                {
                    ulong newmax = m_MaxTKSN + m_AllocationStep;
                    string sql = "update MaxTKSN set AllocatedTKSN=" + newmax.ToString();

                    SqlHelper.ExecuteNonQuery(m_Connstr, CommandType.Text, sql);

                    m_MaxTKSN += m_AllocationStep;
                    return ++m_CurTKSN;
                }
            }
        }

        public void AllocateTKSN(ulong num, ref ulong start, ref ulong end)
        {
            lock (m_LockTKSN)
            {
                if (m_CurTKSN + num < m_MaxTKSN)
                {
                    start = m_CurTKSN + 1;
                    end = m_CurTKSN + num;
                    m_CurTKSN = end;
                }
                else // 从数据库中分配
                {
                    while (m_MaxTKSN < m_CurTKSN + num)
                    {
                        ulong newmax = m_MaxTKSN + m_AllocationStep;
                        string sql = "update MaxTKSN set AllocatedTKSN=" + newmax.ToString();

                        SqlHelper.ExecuteNonQuery(m_Connstr, CommandType.Text, sql);

                        m_MaxTKSN += m_AllocationStep;
                    }

                    start = m_CurTKSN + 1;
                    end = m_CurTKSN + num;
                    m_CurTKSN = end;
                }
            }
        }
        #endregion

        #region 监控平台相关
        public long GetStatus()
        {
            return Interlocked.Read(ref bRun);
        }

        public string GetAlarmClientsNum()
        {
            int authorized = 0, nonauthorized = 0;
            lock (m_Clients)
            {
                foreach (AlarmClient c in m_Clients.Values)
                    if (c.Authorized)
                        ++authorized;
                    else
                        ++nonauthorized;
            }
            return authorized.ToString() + "," + nonauthorized;
        }

        public int GetAdapterClientsNum()
        {
            lock (m_AdapterInfo)
                return m_AdapterInfo.Count;
        }

        public int GetActiveAlarmsNum()
        {
            lock (m_AlarmTable)
                return m_AlarmTable.Count;
        }
        #endregion
    }
}
