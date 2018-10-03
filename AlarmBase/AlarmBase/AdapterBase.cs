using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using System.IO;
using System.Runtime.Serialization;
using TK_AlarmManagement;

namespace TK_AlarmManagement
{
    /// <summary>
    /// 通用的具备网管告警生成功能的适配器模块，支持服务器通讯、告警ID分配、网管告警、日志维护，不包含厂家的采集能力
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AdapterBase<T> : INMAlarmGenerator, IAlarmAdapter
        where T : ISourceAlarm, new()
    {
        #region 保护成员
        protected string m_Name = "";

        protected System.Threading.ReaderWriterLock m_StopPrivilege = null;

        protected Dictionary<string, string> m_OMCAddresses;
        protected Dictionary<string, string> m_OMCInfo;

        protected string m_Connstr = "";
        protected string m_ServerConnStr = "";

        protected long m_PendingRun = 0;
        protected long m_Run = 0;

        protected int m_Interval = 10000;

        protected string m_BusinessType = "";
        protected string m_Manufacturer = "";
        protected string m_ActiveTable = "";
        protected string m_ResumeTable = "";
        protected string m_MaxIDTable = "";

        protected string m_UniteActiveTable = "";
        protected string m_UniteResumeTable = "";
        protected string m_UniteGarbageTable = "";

        protected string m_UniteTKActiveTable = "";
        protected string m_UniteTKResumeTable = "";

        protected string m_TempTableCreateSql = "";

        protected int m_ControllerPort = 0;

        protected ICommClient m_CommClient = null;

        /// <summary>
        /// 维护定时器，每天3点清理日志
        /// </summary>
        protected long m_InMaintenanceTimer = 0;
        protected System.Timers.Timer m_TimerMaintenance = null;
        //add FTTH
        protected int m_SvrID;
        protected string m_encodingStr;
        #endregion

        #region 公有属性
        //add FTTH
        public int SvrID
        {
            get { return m_SvrID; }
            set { m_SvrID = value; }
        }

        public string EncodingStr
        {
            get { return m_encodingStr; }
            set { m_encodingStr = value; }
        }

        /// <summary>
        /// 采集器的名称
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// AdapterController的端口
        /// </summary>
        public int ControllerPort
        {
            get { return m_ControllerPort; }
            set { m_ControllerPort = value; }
        }

        /// <summary>
        /// 获取采集线程是否在运行，为1则正在运行
        /// </summary>
        public long PendingRunFlag
        {
            get { return Interlocked.Read(ref m_PendingRun); }
        }

        /// <summary>
        /// 0: stopped 1: running
        /// </summary>
        public long CompleteRunFlag
        {
            get { return Interlocked.Read(ref m_Run); }
        }

        /// <summary>
        /// 获取采集器的采集周期，单位毫秒
        /// </summary>
        public int Interval
        {
            get { return m_Interval; }
            set { m_Interval = value; }
        }

        /// <summary>
        /// 存放OMC的名称与数据库连接串。不保证多线程安全，访问时应当加锁
        /// </summary>
        public Dictionary<string, string> OMCAddresses
        {
            get { return m_OMCAddresses; }
        }

        /// <summary>
        /// 存放OMC的名称与数据源的名称，可能是IP地址、ODBC源
        /// </summary>
        public Dictionary<string, string> OMCInfo
        {
            get { return m_OMCInfo; }
        }

        /// <summary>
        /// 获取采集器与系统数据库的连接字符串
        /// </summary>
        public string Connstr
        {
            get { return m_Connstr; }
        }

        /// <summary>
        /// 完整信息数据库的连接字符串
        /// </summary>
        public string ServerConnStr
        {
            get { return m_ServerConnStr; }
        }

        /// <summary>
        /// 获取业务类型
        /// </summary>
        public string BusinessType
        {
            get { return m_BusinessType; }
        }

        /// <summary>
        /// 获取告警厂商名称
        /// </summary>
        public string Manufacturer
        {
            get { return m_Manufacturer; }
        }

        /// <summary>
        /// 获取原始活动告警表名称
        /// </summary>
        public string ActiveTable
        {
            get { return m_ActiveTable; }
        }

        /// <summary>
        /// 获取原始历史告警表名称
        /// </summary>
        public string ResumeTable
        {
            get { return m_ResumeTable; }
        }

        public string MaxIDTable
        {
            get { return m_MaxIDTable; }
        }

        public string UniteActiveTable
        {
            get { return m_UniteActiveTable; }
        }

        public string UniteResumeTable
        {
            get { return m_UniteResumeTable; }
        }

        public string UniteGarbageTable
        {
            get { return m_UniteGarbageTable; }
        }

        public string UniteTKActiveTable
        {
            get { return m_UniteTKActiveTable; }
        }

        public string UniteTKResumeTable
        {
            get { return m_UniteTKResumeTable; }
        }

        public string TempTableCreateSql
        {
            get { return m_TempTableCreateSql; }
        }

        public string TempTableName
        {
            get { return "#" + Name + "_TempTable"; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 数据库采集器构造函数
        /// </summary>
        /// <param name="name">采集器的名称，用于日志显示</param>
        /// <param name="interval">采集轮询周期，单位毫秒</param>
        public AdapterBase(string name, int interval)
        {
            Name = name;
            m_Interval = interval;

            InitMember();
        }

        public AdapterBase()
        {
            InitMember();
        }

        virtual protected void InitMember()
        {
            m_StopPrivilege = new System.Threading.ReaderWriterLock();
            m_OMCAddresses = new Dictionary<string, string>();
            m_OMCInfo = new Dictionary<string, string>();
            LogReceived += new LogHandler(DBAdapterBase_LogReceived);
            StateChanged += new StateChangeHandler(DBAdapterBase_StateChanged);

            m_TimerMaintenance = new System.Timers.Timer(3540 * 1000); // 59分钟运行一次的维护定时器
            m_TimerMaintenance.Elapsed += new System.Timers.ElapsedEventHandler(m_TimerMaintenance_Elapsed);
        }
        #endregion

        #region 维护定时器-已含告警生成
        void m_TimerMaintenance_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref m_InMaintenanceTimer, 1) == 1)
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
                        if (span.TotalDays > 7)
                            System.IO.File.Delete(file);

                        ClearNMAlarm(Name, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                        RaiseNMAlarm(Name, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, ex.Message, DateTime.Now);
                        SendLog(ex.ToString());
                    }

                    if (PendingRunFlag == 0)
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

        #region 其它-不必生成告警
        void DBAdapterBase_LogReceived(string sLog)
        {
            // do nothing
        }

        void DBAdapterBase_StateChanged(string name, string state)
        {
            // do nothing
        }
        #endregion

        #region 读取采集配置-不必生成告警
        virtual protected void ReadConfig()
        {
            //获取当前路径
            string sPath = System.AppDomain.CurrentDomain.BaseDirectory;
            sPath = sPath.Substring(0, sPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            //读取保存在dbconnection.xml中的登陆连接信息
            DataSet xmlds = new DataSet();
            xmlds = MD5Encrypt.DES.instance().DecryptXML2DS(sPath + "DBConf.xml", 1);
            //xmlds.ReadXml(sPath + Name + Path.DirectorySeparatorChar + "conf.xml");

            if (xmlds.Tables["SysDB"].Rows.Count == 0)
            {
                SendLog("未定义系统数据库连接");
                return;
            }

            //将xml文件中的连接信息转换成连接字符串
            m_Connstr = "Persist Security Info=False;";
            m_Connstr = m_Connstr + "User ID=" + xmlds.Tables["SysDB"].Rows[0]["username"].ToString() + ";";
            m_Connstr = m_Connstr + "pwd =" + xmlds.Tables["SysDB"].Rows[0]["userpass"].ToString() + ";";
            m_Connstr = m_Connstr + "Initial Catalog=" + xmlds.Tables["SysDB"].Rows[0]["database"].ToString() + ";";
            m_Connstr = m_Connstr + "Data Source=" + xmlds.Tables["SysDB"].Rows[0]["hostname"].ToString();

            try
            {
                //m_ShortcutConnStr = "Persist Security Info=False;";
                //m_ShortcutConnStr = m_ShortcutConnStr + "User ID=" + xmlds.Tables["SysDB"].Rows[0]["shortcut_username"].ToString() + ";";
                //m_ShortcutConnStr = m_ShortcutConnStr + "pwd =" + xmlds.Tables["SysDB"].Rows[0]["shortcut_userpass"].ToString() + ";";
                //m_ShortcutConnStr = m_ShortcutConnStr + "Initial Catalog=" + xmlds.Tables["SysDB"].Rows[0]["shortcut_database"].ToString() + ";";
                //m_ShortcutConnStr = m_ShortcutConnStr + "Data Source=" + xmlds.Tables["SysDB"].Rows[0]["shortcut_hostname"].ToString();

                m_ServerConnStr = "Persist Security Info=False;";
                m_ServerConnStr = m_ServerConnStr + "User ID=" + xmlds.Tables["SysDB"].Rows[0]["server_username"].ToString() + ";";
                m_ServerConnStr = m_ServerConnStr + "pwd =" + xmlds.Tables["SysDB"].Rows[0]["server_userpass"].ToString() + ";";
                m_ServerConnStr = m_ServerConnStr + "Initial Catalog=" + xmlds.Tables["SysDB"].Rows[0]["server_database"].ToString() + ";";
                m_ServerConnStr = m_ServerConnStr + "Data Source=" + xmlds.Tables["SysDB"].Rows[0]["server_hostname"].ToString();
            }
            catch
            {
                // 为保持版本兼容性过度，无上述信息也可启动
            }

            m_BusinessType = xmlds.Tables["SysDB"].Rows[0]["businesstype"].ToString();
            m_Manufacturer = xmlds.Tables["SysDB"].Rows[0]["manufacturer"].ToString();
            m_ActiveTable = xmlds.Tables["SysDB"].Rows[0]["activetable"].ToString();
            m_ResumeTable = xmlds.Tables["SysDB"].Rows[0]["resumetable"].ToString();
            m_MaxIDTable = xmlds.Tables["SysDB"].Rows[0]["maxidtable"].ToString();
            m_UniteActiveTable = xmlds.Tables["SysDB"].Rows[0]["uniteactivetable"].ToString();
            m_UniteResumeTable = xmlds.Tables["SysDB"].Rows[0]["uniteresumetable"].ToString();
            m_UniteGarbageTable = xmlds.Tables["SysDB"].Rows[0]["unitegarbagetable"].ToString();
            m_UniteTKActiveTable = xmlds.Tables["SysDB"].Rows[0]["unitetkactivetable"].ToString();
            m_UniteTKResumeTable = xmlds.Tables["SysDB"].Rows[0]["unitetkresumetable"].ToString();

            m_OMCAddresses.Clear();
            m_OMCInfo.Clear();

            //读取OMC配置
            foreach (DataRow dr in xmlds.Tables["OMCInfo"].Rows)
            {
                string connstr;

                if (xmlds.Tables["OMCInfo"].Columns.Contains("connstr")) // for compatibility
                    connstr = dr["connstr"].ToString().Trim();
                else
                {
                    connstr = "Persist Security Info=False;";
                    connstr = connstr + "User ID=" + dr["UserName"].ToString() + ";";
                    connstr = connstr + "pwd =" + dr["Password"].ToString() + ";";
                    connstr = connstr + "Initial Catalog=" + dr["Database"].ToString() + ";";
                    connstr = connstr + "Data Source=" + dr["Hostname"].ToString();
                }

                m_OMCAddresses[dr["Name"].ToString().Trim()] = connstr;
                m_OMCInfo[dr["Name"].ToString().Trim()] = dr["Hostname"].ToString();

                DeclareNMMonitorObjects(dr["Name"].ToString().Trim(), Constants.MO_SOURCEOMC, Constants.TKALM_OMCALM, "重要告警");
                DeclareNMMonitorObjects(dr["Name"].ToString().Trim(), Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, "重要告警");
                DeclareNMMonitorObjects(dr["Name"].ToString().Trim(), Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, "重要告警");
            }

            DeclareNMMonitorObjects(Name, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, "重要告警");

            WatchDog.WatchDogFactory.Instance().CreateWatchDogsFromFile("watchdog.xml");

            /// 读取创建临时表的SQL脚本
            string temptable_createfile = sPath + "TempTable.sql";
            if (System.IO.File.Exists(temptable_createfile))
            {
                using (StreamReader sr = new StreamReader(temptable_createfile, Encoding.GetEncoding("GB2312")))
                {
                    m_TempTableCreateSql = sr.ReadToEnd();
                }
            }
        }
        #endregion

        #region 网管告警一致性检查的基本实现，派生类如有特殊要求，应重写此方法-已含告警生成
        virtual protected void _checkNMActiveConsistency(string omcName, string omcConn)
        {

            // A - B
            string q = "select * from " + UniteTKActiveTable + " where SourceOMC='" + omcName + "'";
            q += " and TKSn not in (select TKSn from " + UniteActiveTable + " where OMCName='" + NMAlarm.OMCName + "')"; // OMCName也是全局唯一，不追加使用Manufacturer

            DataSet ds = new DataSet();
            try
            {
                SqlHelper.FillDataset(Connstr, CommandType.Text, q, ds, new string[] { "temp" });

                ClearNMAlarm(omcName, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, DateTime.Now);
            }
            catch (SqlException ex)
            {
                RaiseNMAlarm(omcName, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, ex.Message, DateTime.Now);
                SendLog(ex.ToString());
            }

            List<TKAlarm> pending = new List<TKAlarm>();
            foreach (DataRow r in ds.Tables["temp"].Rows)
            {
                NMAlarm sourcealarm = new NMAlarm();
                sourcealarm.ActiveTable = UniteTKActiveTable;
                sourcealarm.ResumeTable = UniteTKResumeTable;
                sourcealarm.BuildFromDB(r);

                TKAlarm tkalarm = ConvertNMToTKAlarm(sourcealarm);
                pending.Add(tkalarm);

                if (PendingRunFlag == 0)
                    return;
            }

            // (B-A)
            q = "select * from " + UniteActiveTable + " where OMCName='" + NMAlarm.OMCName + "' and NEName='" + omcName + "'";
            q += " and TKSn not in (select TKSn from " + UniteTKActiveTable + " where SourceOMC='" + omcName + "')";

            ds.Tables.Clear();
            try
            {
                SqlHelper.FillDataset(Connstr, CommandType.Text, q, ds, new string[] { "temp" });

                ClearNMAlarm(omcName, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, DateTime.Now);
            }
            catch (Exception ex)
            {
                RaiseNMAlarm(omcName, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, ex.Message, DateTime.Now);
                SendLog(ex.ToString());
            }

            foreach (DataRow r in ds.Tables["temp"].Rows)
            {
                int tksn = Convert.ToInt32(r["TKSn"]);

                q = "select * from " + UniteTKResumeTable + " where tksn=" + tksn;

                try
                {
                    DataSet temp = new DataSet();
                    SqlHelper.FillDataset(Connstr, CommandType.Text, q, temp, new string[] { "temp" });

                    ClearNMAlarm(omcName, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, DateTime.Now);

                    if (temp.Tables["temp"].Rows.Count == 0)
                    {
                        // 作为垃圾告警处理
                        TKAlarm tkalarm_garbage = new TKAlarm();
                        tkalarm_garbage.ConvertFromDB(r);
                        tkalarm_garbage.ClearTime = Constants.GARBAGE_CLEARTIME;

                        SendAlarm(tkalarm_garbage);
                    }
                    else
                    {
                        NMAlarm sourcealarm_r = new NMAlarm(UniteActiveTable, UniteResumeTable);
                        //sourcealarm_r.ActiveTable = UniteTKActiveTable;
                        //sourcealarm_r.ResumeTable = UniteTKResumeTable;
                        sourcealarm_r.BuildFromDB(temp.Tables["temp"].Rows[0]);

                        TKAlarm alarm = ConvertNMToTKAlarm(sourcealarm_r);
                        pending.Add(alarm);
                    }

                }
                catch (SqlException ex)
                {
                    RaiseNMAlarm(omcName, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, ex.Message, DateTime.Now);
                    SendLog(ex.ToString());
                }

                if (PendingRunFlag == 0)
                    return;
            }

            SendAlarms(pending);
        }
        #endregion

        #region 分配TKSN，仅用于独立进程的采集-不必生成告警
        private object m_LockAllocation = new int();

        protected ulong m_CurTKSN = 0;
        protected ulong m_TKSN_Begin = 0;
        protected ulong m_TKSN_End = 0;
        virtual public ulong AllocateTKSN()
        {
            lock (m_LockAllocation)
            {
                if (m_CurTKSN == m_TKSN_End)
                {
                    // allocate from server
                    if (!AllocateTKSNFromServer())
                        throw new Exception("无法从服务器分配告警流水号.");

                    m_CurTKSN = m_TKSN_Begin;
                }
                else
                    ++m_CurTKSN;

                return m_CurTKSN;
            }
        }
        #endregion

        #region 事件桥接接口-不必生成告警
        protected void SendLog(string s)
        {
            try
            {
                DefLib.Util.Logger.Instance().SendLog("Adapter", s);
            }
            catch { }
        }

        protected void SendAlarms(List<TKAlarm> ar)
        {
            foreach (TKAlarm alarm in ar)
            {
                if (PendingRunFlag == 0)
                    return;

                SendAlarm(alarm);
            }
        }

        protected void SendAlarm(TKAlarm alarm)
        {
            AdapterAlarmReport(alarm);
        }

        protected void SendStateChange(string omcName, string s)
        {
            try
            {
                StateChanged(omcName, s);
            }
            catch { }
        }
        #endregion

        #region IAlarmAdapter 成员-不必生成告警

        public event LogHandler LogReceived;
        public event StateChangeHandler StateChanged;

        virtual public void Init(ICommClient comm)
        {
            m_CommClient = comm;
        }

        virtual public void Start()
        {
            lock (this)
            {
                if (Interlocked.Read(ref m_Run) == 1)
                    return;

                if (Interlocked.Exchange(ref m_PendingRun, 1) == 1)
                    return;

                try
                {
                    /// 允许派生类在启动开始阶段插入动作
                    //_PreStartStage();

                    ReadConfig();

                    TempStorageHelper.Instance().Init();

                    if (m_CommClient == null)
                    {
                        Interlocked.Exchange(ref m_PendingRun, 0);
                        return;
                    }

                    if (!m_CommClient.Start())
                    {
                        Interlocked.Exchange(ref m_PendingRun, 0);
                        throw new Exception("连接告警服务器失败, 适配器无法启动.");
                    }

                    m_CommClient.onConnectionBroken += new ClientConnectionBrokenHandler(DBAdapterBase_onConnectionBroken);

                    SendLog("正在启动" + Name + "告警适配器(采集周期:" + Interval + "[毫秒])...");
                    if (!AdapterLogin())
                    {
                        Interlocked.Exchange(ref m_PendingRun, 0);
                        throw new Exception("向告警服务器注册失败, 适配器无法启动.");
                    }

                    AdapterStateReport(AdapterStatus.Running, null);

                    m_TimerMaintenance.Start();

                    /// 允许派生类在启动加载告警前插入动作
                    //_DeclareCustomAlarm();

                    /// 检查临时存储尚未入库的数据
                    _checkTempStorage();

                    ClearAllNMAlarms();
                    LoadNMAlarms();
                    ClearAllNMAlarms();

                    /// 允许派生类在启动最后插入动作
                    //_PostStartStage();

                    Interlocked.Exchange(ref m_Run, 1);

                    SendLog("启动成功：" + Name + "告警适配器(采集周期:" + Interval + "[毫秒])...");
                }
                catch (Exception ex)
                {
                    Interlocked.Exchange(ref m_PendingRun, 0);
                    Interlocked.Exchange(ref m_Run, 0);
                    throw ex;
                }
            }
        }

        abstract protected void _PreStartStage();
        abstract protected void _PostStartStage();
        abstract protected void _DeclareCustomAlarm();

        virtual public void Stop()
        {
            lock (this)
            {
                if (Interlocked.Read(ref m_Run) == 0)
                    return;

                if (Interlocked.Exchange(ref m_PendingRun, 0) == 0)
                    return;

                SendLog("正在关闭" + Name + "告警适配器...");

                _PreStopStage();

                AdapterStateReport(AdapterStatus.Stop, null);

                if (!AdapterLogout())
                {
                    SendLog("从告警服务器注销失败, 服务器中信息可能不正确.");
                }

                try
                {
                    if (m_CommClient != null)
                    {
                        m_CommClient.onConnectionBroken -= new ClientConnectionBrokenHandler(DBAdapterBase_onConnectionBroken);
                        m_CommClient.Close();
                    }
                }
                catch { }

                m_TimerMaintenance.Stop();

                _PostStopStage();

                try
                {
                    m_StopPrivilege.AcquireWriterLock(-1);
                }
                finally
                {
                    m_StopPrivilege.ReleaseLock();
                }

                Interlocked.Exchange(ref m_Run, 0);

                SendLog(Name + "告警适配器已结束.");
            }
        }

        /// <summary>
        /// 在结束刚开始阶段插入动作
        /// </summary>
        abstract protected void _PreStopStage();

        /// <summary>
        /// 在结束等待读写锁之前插入动作
        /// </summary>
        abstract protected void _PostStopStage();

        #endregion

        #region 告警时间戳类，用于强制清除告警时判断延迟时间-不必生成告警
        protected class AlarmTimeStamp<A>
        {
            public A Alarm;
            public System.DateTime TimeStamp;
            public bool NeedRemove;

            public static bool IsNeedRemove(AlarmTimeStamp<A> p)
            {
                return p.NeedRemove;
            }
        }
        #endregion

        #region 各项服务器通讯命令-不必生成告警
        protected bool AdapterLogin()
        {
            CommandMsgV2 cm = new CommandMsgV2();
            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_LOGIN;

            cm.SetValue(Constants.MSG_PARANAME_ADAPTER_NAME, Name);
            cm.SetValue(Constants.MSG_PARANAME_ADAPTER_ADDRESS, m_CommClient.LocalIP.ToString());
            cm.SetValue(Constants.MSG_PARANAME_ADAPTER_CONTROLLER_PORT, ControllerPort);

            try
            {
                CommandMsgV2 resp = m_CommClient.SendCommand(cm) as CommandMsgV2;
                if (resp == null)
                    return false;

                if (resp.Contains(Constants.MSG_PARANAME_RESULT))
                    switch (resp.GetValue(Constants.MSG_PARANAME_RESULT).ToString())
                    {
                        case "OK":
                            return true;
                        default:
                            return false;
                    }
                else
                    return false;
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
                return false;
            }
        }

        protected bool AdapterLogout()
        {
            CommandMsgV2 cm = new CommandMsgV2();
            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_LOGOUT;

            try
            {
                CommandMsgV2 resp = m_CommClient.SendCommand(cm) as CommandMsgV2;
                if (resp == null)
                    return false;

                if (resp.Contains(Constants.MSG_PARANAME_RESULT))
                    switch (resp.GetValue(Constants.MSG_PARANAME_RESULT).ToString())
                    {
                        case "OK":
                            return true;
                        default:
                            return false;
                    }
                else
                    return false;
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
                return false;
            }
        }

        protected void AdapterStateReport(AdapterStatus state, object extrainfo)
        {
            CommandMsgV2 cm = new CommandMsgV2();
            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_STATE_REPORT;

            cm.SetValue(Constants.MSG_PARANAME_ADAPTER_STATE, state);
            cm.SetValue(Constants.MSG_PARANAME_ADAPTER_EXTRAINFO, extrainfo);

            try
            {
                m_CommClient.PostCommand(cm);
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
        }

        protected void AdapterAlarmReport(TKAlarm alarm)
        {
            try
            {
                CommandMsgV2 msg = alarm.ConvertToMsg();
                m_CommClient.PostCommand(msg);
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
        }

        protected bool AllocateTKSNFromServer()
        {
            CommandMsgV2 cm = new CommandMsgV2();
            cm.TK_CommandType = Constants.TK_CommandType.ALLOCATE_TKSN;

            cm.SetValue(Constants.MSG_PARANAME_TKSN_NUM, 10000);

            try
            {
                CommandMsgV2 resp = m_CommClient.SendCommand(cm, 120) as CommandMsgV2;
                if (resp == null)
                    return false;

                if (resp.GetValue(Constants.MSG_PARANAME_RESULT).ToString() != "OK")
                    return false;

                lock (m_LockAllocation)
                {
                    m_TKSN_Begin = Convert.ToUInt64(resp.GetValue(Constants.MSG_PARANAME_TKSN_START));
                    m_TKSN_End = Convert.ToUInt64(resp.GetValue(Constants.MSG_PARANAME_TKSN_END));
                }

                return true;
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
                return false;
            }
        }
        #endregion

        protected void DBAdapterBase_onConnectionBroken(ICommClient ptr, string reason)
        {
            try
            {
                Thread th = new Thread(new ThreadStart(Stop));
                th.Name = "Stop Invoked By Connection Broken";
                th.Start();
            }
            catch { }
        }

        #region 自身网管告警管理
        Dictionary<string, Dictionary<string, Dictionary<string, NMAlarm>>> m_NMAlarms =
            new Dictionary<string, Dictionary<string, Dictionary<string, NMAlarm>>>();
        
        public List<NMAlarm> GetNMAlarms()
        {
            List<NMAlarm> ret = new List<NMAlarm>();
            lock (m_NMAlarms)
            {
                foreach (Dictionary<string, Dictionary<string, NMAlarm>> omcalarms in m_NMAlarms.Values)
                    foreach (Dictionary<string, NMAlarm> objalarms in omcalarms.Values)
                        foreach (NMAlarm a in objalarms.Values)
                            ret.Add(a);
                return ret;
            }
        }

        /// <summary>
        /// 更新界面状态
        /// </summary>
        /// <param name="sourceomc"></param>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="detail"></param>
        /// <param name="occurtime"></param>
        /// <returns></returns>
        public bool RaiseNMAlarm(string sourceomc, string obj, string name, string detail, DateTime occurtime)
        {
            TKAlarm tkalarm = null;
            try
            {
                lock (m_NMAlarms)
                {
                    if (!m_NMAlarms.ContainsKey(sourceomc))
                        return false;

                    if (!m_NMAlarms[sourceomc].ContainsKey(obj))
                        return false;

                    if (!m_NMAlarms[sourceomc][obj].ContainsKey(name))
                        return false;

                    NMAlarm a = m_NMAlarms[sourceomc][obj][name];
                    if (a.TKSn != "" && a.ClearTime == "")
                    {
                        a.LastOccurTime = occurtime.ToString();
                        return false; // 告警已经发生过，尚未恢复
                    }

                    a.SourceOMC = sourceomc;
                    a.Object = obj;
                    a.AlarmName = name;
                    a.OccurTime = occurtime.ToString();
                    a.LastOccurTime = occurtime.ToString();
                    a.ClearTime = "";
                    a.Detail = detail;
                    a.TKSn = "";
                    a.ActiveTable = UniteTKActiveTable;
                    a.ResumeTable = UniteTKResumeTable;

                    tkalarm = ConvertNMToTKAlarm(a);

                    try
                    {
                        TempStorageHelper.Instance().Store<NMAlarm>(Convert.ToInt64(a.TKSn), a);
                        SqlHelper.ExecuteNonQuery(Connstr, CommandType.Text, a.GetSql());
                        TempStorageHelper.Instance().Clear<NMAlarm>(Convert.ToInt64(a.TKSn));
                    }
                    catch (Exception ex)
                    {
                        // 存储网管告警出异常不再发出告警
                        // 不继续抛出告警, 允许继续往服务器发送告警 
                        SendLog(ex.ToString());
                    }
                }

                try
                {
                    if (tkalarm != null)
                        SendAlarm(tkalarm);
                }
                catch (Exception ex)
                {
                    // 向服务器发送告警失败，只记录日志，不影响程序运行
                    SendLog(ex.ToString());
                    return false;
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// 清楚告警，此处没用到
        /// </summary>
        /// <param name="sourceomc"></param>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="cleartime"></param>
        /// <returns></returns>
        public bool ClearNMAlarm(string sourceomc, string obj, string name, DateTime cleartime)
        {
            try
            {
                TKAlarm tkalarm = null;
                lock (m_NMAlarms)
                {
                    if (!m_NMAlarms.ContainsKey(sourceomc))
                        return false;

                    if (!m_NMAlarms[sourceomc].ContainsKey(obj))
                        return false;

                    if (!m_NMAlarms[sourceomc][obj].ContainsKey(name))
                        return false;

                    NMAlarm a = m_NMAlarms[sourceomc][obj][name];
                    if (a.TKSn == "" || a.ClearTime != "")
                        return false; //告警已经恢复，恢复动作无效

                    a.ClearTime = cleartime.ToString();
                    a.ActiveTable = UniteTKActiveTable;
                    a.ResumeTable = UniteTKResumeTable;

                    tkalarm = ConvertNMToTKAlarm(a);

                    try
                    {
                        SqlHelper.ExecuteNonQuery(Connstr, CommandType.Text, a.GetSql());
                    }
                    catch (Exception ex)
                    {
                        // 存储网管告警出异常不再发出告警
                        // 不继续抛出告警, 允许继续往服务器发送告警
                        SendLog(ex.ToString());
                    }
                }

                try
                {
                    if (tkalarm != null)
                        SendAlarm(tkalarm);
                }
                catch (Exception ex)
                {
                    SendLog(ex.ToString());
                    return false;
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
                return false;
            }

            return true;
        }

        public void ClearAllNMAlarms()
        {
            lock (m_NMAlarms)
            {
                foreach (Dictionary<string, Dictionary<string, NMAlarm>> omcalarms in m_NMAlarms.Values)
                    foreach (Dictionary<string, NMAlarm> objalarms in omcalarms.Values)
                    {
                        try
                        {
                            foreach (NMAlarm a in objalarms.Values)
                            {
                                if (a.TKSn != "" && a.ClearTime == "") //未恢复的告警应发至服务器恢复之
                                {
                                    //a.ClearTime = DateTime.Now.ToString();
                                    ClearNMAlarm(a.SourceOMC, a.Object, a.AlarmName, DateTime.Now);
                                    //TKAlarm tkalarm = ConvertNMToTKAlarm(a);
                                    //SendAlarm(tkalarm);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SendLog(ex.ToString()); // 监控模块产生的异常无法再作为告警处理
                        }

                        string[] keys = new string[objalarms.Count];
                        objalarms.Keys.CopyTo(keys, 0);
                        foreach (string alarmname in keys)
                        {
                            NMAlarm alarm = new NMAlarm(UniteTKActiveTable, UniteTKResumeTable);
                            alarm.Severity = objalarms[alarmname].Severity;
                            objalarms[alarmname] = alarm;
                        }
                    }
            } // endlock
        }

        /// <summary>
        /// 在m_NMAlarms中放一个空告警，其告警级别设置为sev
        /// </summary>
        /// <param name="omcname"></param>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="sev"></param>
        public void DeclareNMMonitorObjects(string omcname, string obj, string name, string sev)
        {
            lock (m_NMAlarms)
            {
                if (!m_NMAlarms.ContainsKey(omcname))
                    m_NMAlarms.Add(omcname, new Dictionary<string, Dictionary<string, NMAlarm>>());

                if (!m_NMAlarms[omcname].ContainsKey(obj))
                    m_NMAlarms[omcname].Add(obj, new Dictionary<string, NMAlarm>());

                NMAlarm alarm = new NMAlarm(UniteTKActiveTable, UniteTKResumeTable);
                alarm.Severity = sev;

                m_NMAlarms[omcname][obj][name] = alarm;
            }
        }

        public void LoadNMAlarms()
        {
            lock (m_NMAlarms)
            {
                foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, NMAlarm>>> pairomc in m_NMAlarms)
                {
                    string omcname = pairomc.Key;
                    foreach (KeyValuePair<string, Dictionary<string, NMAlarm>> pairobj in pairomc.Value)
                    {
                        string obj = pairobj.Key;

                        foreach (KeyValuePair<string, NMAlarm> pair in pairobj.Value)
                        {
                            string name = pair.Key;

                            try
                            {
                                string q;
                                q = "select * from " + UniteTKActiveTable + " where sourceomc='" + omcname + "'";
                                q += " and Object='" + obj + "'";
                                q += " and AlarmName='" + name + "'";

                                DataSet ds = new DataSet();
                                ds.Tables.Add("temp");
                                try
                                {
                                    SqlHelper.FillDataset(Connstr, CommandType.Text, q, ds, new string[] { "temp" });

                                    ClearNMAlarm(omcname, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, DateTime.Now);
                                }
                                catch (Exception ex)
                                {
                                    RaiseNMAlarm(omcname, Constants.MO_SYSDB, Constants.TKALM_SYSDBALM, ex.Message, DateTime.Now);
                                    SendLog(ex.ToString());
                                }

                                if (ds.Tables["temp"].Rows.Count == 1)
                                { // load alarm
                                    pair.Value.BuildFromDB(ds.Tables["temp"].Rows[0]);
                                }

                                ClearNMAlarm(omcname, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
                            }
                            catch (Exception ex)
                            {
                                RaiseNMAlarm(omcname, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, ex.Message, DateTime.Now);
                                SendLog(ex.ToString());
                            }
                        } // end foreach name->alarm
                    } // end foreach obj->name pair
                } // end foreach omc->obj pair
            } // end lock
        }

        public TKAlarm ConvertNMToTKAlarm(NMAlarm a)
        {
            TKAlarm tkalarm = new TKAlarm();
            tkalarm.OMCName = NMAlarm.OMCName;
            tkalarm.Manufacturer = NMAlarm.Manufacturer;
            tkalarm.BusinessType = NMAlarm.BusinessType;

            tkalarm.NeName = a.SourceOMC;
            tkalarm.ObjName = a.Object;
            tkalarm.AlarmName = tkalarm.Redefinition = a.AlarmName;
            tkalarm.OccurTime = a.OccurTime;
            tkalarm.Reserved2 = a.LastOccurTime;
            tkalarm.ClearTime = a.ClearTime;
            tkalarm.Location = tkalarm.NeName + "//" + tkalarm.ObjName;

            if (a.Detail.Length > 120)
                tkalarm.Reserved3 = a.Detail.Substring(0, 120);
            else
                tkalarm.Reserved3 = a.Detail;

            tkalarm.Severity = a.Severity;

            if (a.TKSn == "")
                a.TKSn = AllocateTKSN().ToString();

            tkalarm.TKSn = a.TKSn;

            return tkalarm;
        }
        #endregion

        #region 重新加载临时存储未成功入库的数据
        virtual protected void _checkTempStorage()
        {
            List<T> tempstorage = TempStorageHelper.Instance().RestoreAllAndClear<T>();

            foreach (T alarm in tempstorage)
            {
                try
                {
                    TempStorageHelper.Instance().Store<T>(Convert.ToInt64(alarm.TKSn), alarm);
                    SqlHelper.ExecuteNonQuery(Connstr, CommandType.Text, alarm.GetSql());
                    TempStorageHelper.Instance().Clear<T>(Convert.ToInt64(alarm.TKSn));
                }
                catch (Exception ex)
                {
                    SendLog(ex.ToString());
                }
            }
        }
        #endregion
    }

}
