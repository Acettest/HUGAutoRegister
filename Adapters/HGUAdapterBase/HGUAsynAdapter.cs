#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: YWTAsynAdapter
// 作    者：d.w
// 创建时间：2014/6/19 14:17:53
// 描    述：
// 版    本：1.0.0.0
//-----------------------------------------------------------------------------
// 历史更新纪录
//-----------------------------------------------------------------------------
// 版    本：           修改时间：           修改人：           
// 修改内容：
//-----------------------------------------------------------------------------
// Copyright (C) 2009-2014 www.chaselwang.com . All Rights Reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Data;
using Microsoft.ApplicationBlocks.Data;
using TK_AlarmManagement;
using DefLib;
using HGU.Idl;
using HGU.CommandsBuilder;
using HGU.Tl1Wrapper;

namespace HGUAdapterBase
{
    public class HGUAsynAdapter : DBAdapterBase<HGUAlarm>, ICommandHandler, ICommandRegister
    {
        #region Constants
        public const string TASK_TABLE = "Task";
        public const string ROLLBACKHISTASK_TABLE = "RollbackHisTask";
        public const string TASK_VIEW_NAME = "vNormalTask";                      //普通工单视图
        public const string RELOCATETASK_VIEW_NAME = "vRelocateTask";            //移机工单
        public const string ROLLBACKHISTASK_VIEW_NAME = "vRollbackHisTask";      //回滚工单视图
        public const int NETDELAY_MINUTE = 14;                                   //工单网络异常超时
        public const int TELNET_TIMEOUT = 100;                                   //telnet超时，秒
        public const int OLTOFFLINE_INTERVAL = 15 * 60;                          //
        #endregion

        #region IFields
        protected OMCAddInfo m_omc;
        /// <summary>
        /// 从数据库中所取工单均入队列
        /// </summary>
        private Queue<Task> m_tasks;

        private Queue<Task> m_oltOfflineTasks;                                    //需重新执行的olt掉线工单

        /// <summary>
        /// 每个客户端单独登陆网管处理工单，设置多个客户端是为了加快处理速度
        /// </summary>
        protected Tl1Wrapper[] m_tl1Clients = new Tl1Wrapper[4];

        protected WaitHandle[] m_idleHandles = new WaitHandle[4];
        private string m_loginStr;
        private string m_logoutStr;
        protected ManuCommandsBuilder m_commBuilder;

        private long m_isOMCAvailable = 0L;
        private string m_condition;                                        //sql where
        private long m_maxTaskID;
        private long m_maxRollTaskID;
        private object m_connectOMCLocker = new object();
        protected WatchDog.IWatchDog m_dog;

        string omcName = "";
        ArrayList m_Files = new ArrayList();

        bool m_InTimer = false;
        object m_LockTimer = new object();
        System.Timers.Timer m_Timer_ClearLogger = new System.Timers.Timer(); //每隔30天清理日志

        #endregion

        #region IConstructors
        public HGUAsynAdapter()
        {
            m_Timer_ClearLogger.Interval = 1000 * 60 * 60 * 3; //3小时检查一次
            m_Timer_ClearLogger.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_ClearLogger_Elapsed);
        }

        public HGUAsynAdapter(string name, int interval)
            : this()
        {
            m_Name = name;
            m_Interval = interval;
        }
        #endregion

        #region IMethods
        #region 初始化
        protected virtual void InitCommandBuilder()
        { }

        protected override void ReadConfig()//此方法只在AdapterBase中进行了调用，由于此处重写了，调用进入此处
        {
            base.ReadConfig();
            Init();
        }

        protected void Init()
        {
            SQLUtil.s_conn = m_Connstr;
            m_omc = SQLUtil.GetOMCInfo(m_Connstr, m_SvrID);
            m_condition = string.Format("where city='{0}' and manufacturer='{1}' and omcName='{2}'",
                m_omc.Omc.City, m_omc.Omc.Manufacturer, m_omc.Omc.OmcName);
            InitCommandBuilder();//对bell，实际调用的是HGU_Bell_Adapter.InitCommandBuilder
            m_tasks = new Queue<Task>();
            m_oltOfflineTasks = new Queue<Task>();

            WatchDog.WatchDogFactory.Instance().CreateWatchDogsFromFile("watchdog.xml");
            m_dog = WatchDog.WatchDogFactory.Instance().GetWatchDog(Name, Name);

            m_loginStr = m_commBuilder.GetLoginStr(m_omc.User, m_omc.Pwd);
            m_logoutStr = m_commBuilder.GetLogoutStr(m_omc.User);
            m_idleHandles[0] = new AutoResetEvent(false);
            m_idleHandles[1] = new AutoResetEvent(false);

            m_idleHandles[2] = new AutoResetEvent(false);
            m_idleHandles[3] = new AutoResetEvent(false);

            InitTl1Client();//对bell而言，实际调用的是HGU_Bell_Adapter.InitTl1Client

            SubscribeTl1(m_tl1Clients[0]);
            SubscribeTl1(m_tl1Clients[1]);

            SubscribeTl1(m_tl1Clients[2]);
            SubscribeTl1(m_tl1Clients[3]);


        }

        protected virtual void InitTl1Client()
        {
            Encoding encoding = (m_encodingStr == "" ? null : Encoding.GetEncoding(m_encodingStr));

            m_tl1Clients[0] = new Tl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr,TELNET_TIMEOUT,
                encoding, (EventWaitHandle)m_idleHandles[0], m_commBuilder,m_dog);

            m_tl1Clients[1] = new Tl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr,TELNET_TIMEOUT,
                encoding, (EventWaitHandle)m_idleHandles[1], m_commBuilder,m_dog);


            m_tl1Clients[2] = new Tl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr, TELNET_TIMEOUT,
              encoding, (EventWaitHandle)m_idleHandles[2], m_commBuilder, m_dog);

            m_tl1Clients[3] = new Tl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr, TELNET_TIMEOUT,
              encoding, (EventWaitHandle)m_idleHandles[3], m_commBuilder, m_dog);

        }

        private void SubscribeTl1(Tl1Wrapper tl1)
        {
            tl1.SendLog += SendLog;
            tl1.EnqueueTask += EnqueueTask;
            tl1.IsAdapterRunning += IsAdapterRunning;
            tl1.OnNetInterrupt += OnNetInterruptHandler;
            tl1.OnConnectedOMC += OnConnectedHandler;
            tl1.RaiseOmcAlarm += RaiseOmcAlarm;
        }
        #endregion

        #region 重载基类逻辑控制方法
        //重载了DBAdapterBase中的_OMCDBRetriever，在DBAdapterBase中实际调用的是此处的DBAdapterBase
        /// <summary>
        /// 从数据库取工单
        /// </summary>
        /// <param name="para"></param>
        protected override void _OMCDBRetriever(object para)
        {
            omcName = para as string;
            string omcConn = "";

            lock (m_OMCAddresses)
            {
                if (m_OMCAddresses.ContainsKey(omcName))
                {
                    omcConn = m_OMCAddresses[omcName];
                }
                else
                {
                    return;
                }
            }

            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);

                while (PendingRunFlag == 1)
                {
                    try
                    {
                        //test
                        //throw new Exception("Test");
                        /// 检查TK网管活动告警的一致性
                        _checkNMActiveConsistency(omcName, omcConn);
                        DoWork();//从数据库取工单
                        //if (m_dog != null) m_dog.Feed();

                        ClearNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);

                    }
                    catch (Exception ex)
                    {
                        RaiseNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, ex.Message, DateTime.Now);
                        SendLog(ex.ToString());
                    }

                    int i = m_Interval % 1000;
                    for (; i < m_Interval; i += 1000)
                    {
                        if (Interlocked.Read(ref m_PendingRun) == 0)
                            break;
                        Thread.Sleep(1000);
                    }
                }
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 业务处理核心
        /// </summary>
        public override void Start()
        {
            m_maxTaskID = 0L;
            m_maxRollTaskID = 0L;
            //DBAdapterBase中调用获取工单代码
            base.Start();//此处为一个调用链，从基类开始，且最终会调用到最底层Adapter中的重写方法
            try
            {
                m_Timer_ClearLogger.Start();
                Thread thread = new Thread(new ThreadStart(TaskRetriever));//处理工单，所取工单排队处理
                thread.Start();

                //Thread thread3 = new Thread(new ThreadStart(OltOfflineTaskRetriever));
                //thread3.Start();
                //

                Thread thread0 = new Thread(new ThreadStart(m_tl1Clients[0].Retriever));
                thread0.Start();
                Thread thread1 = new Thread(new ThreadStart(m_tl1Clients[1].Retriever));
                thread1.Start();

                Thread thread2 = new Thread(new ThreadStart(m_tl1Clients[2].Retriever));
                thread2.Start();
                Thread thread3 = new Thread(new ThreadStart(m_tl1Clients[3].Retriever));
                thread3.Start();


            }
            catch (Exception ex)
            {
                SendLog("初始化数据失败:" + ex.ToString());
            }
        }

        public override void Stop()
        {
            m_Timer_ClearLogger.Stop();
            UnRegisterCommand();
            base.Stop();
            m_tl1Clients[0].SetOmcUnavailable();
            m_tl1Clients[1].SetOmcUnavailable();

            m_tl1Clients[2].SetOmcUnavailable();
            m_tl1Clients[3].SetOmcUnavailable();

            SetOMCUnavailable();
            SQLUtil.UpdateSvrStatus(m_Connstr, "0", "服务器连接断开！", m_SvrID.ToString());
        }
        #endregion

        #region
        /// <summary>
        /// 从数据库中取工单，有两个DoWork，一个取工单，一个处理工单
        /// </summary>
        private void DoWork()
        {
            string msg = string.Empty;
            try
            {
                if (Interlocked.Equals(m_isOMCAvailable, 0L))
                {
                    CheckNetDelayTask();
                }
                GetActiveTask();//获取任务-从数据库查询
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
        }

        /// <summary>
        /// 只有开机工单才有网络延时回复
        /// </summary>
        private void CheckNetDelayTask()
        {
            DateTime stamp = DateTime.Now.AddMinutes(-NETDELAY_MINUTE);
            string tableName = string.Empty;
            foreach (Task task in m_tasks)
            {
                if (!task.NetDelay && (task.Type == TaskType.NewBroadband || task.Type == TaskType.NewIMS) && task.ReceiveTime < stamp)
                {
                    task.NetDelay = true;
                    //task.ResponseMsg = "网络延时";
                    if (task.IsRollbackHisTask)//暂时没回滚工单是开机的
                        tableName = ROLLBACKHISTASK_TABLE;
                    else
                        tableName = TASK_TABLE;
                    SQLUtil.UpdateNetDelayTask(m_Connstr, task, tableName);
                }
            }
        }

        private void TaskRetriever()
        {
            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);
                SendLog(DateTime.Now.ToString() + " " + "PendingRunFlag=" + PendingRunFlag);
                while (PendingRunFlag == 1)
                {
                    try
                    {
                        //派单
                        DispatchTask();
                    }
                    catch (Exception ex)
                    {
                        RaiseNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, ex.Message, DateTime.Now);
                        SendLog(ex.ToString());
                    }
                    int i = m_Interval % 1000;
                    for (; i < m_Interval; i += 1000)
                    {
                        if (Interlocked.Read(ref m_PendingRun) == 0)
                            break;
                        Thread.Sleep(1000);
                    }
                }
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();
            }
        }

        private void OltOfflineTaskRetriever()
        {
            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);

                while (PendingRunFlag == 1)
                {
                    try
                    {
                        DispatchOltOfflineTask();
                    }
                    catch (Exception ex)
                    {
                        SendLog(ex.ToString());
                    }
                    for (int i = 0; i < OLTOFFLINE_INTERVAL; i += 1)
                    {
                        if (Interlocked.Read(ref m_PendingRun) == 0)
                            break;
                        Thread.Sleep(1000);
                    }
                }
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();
            }
        }
        #region 派单
        private void DispatchTask()
        {
            Task currentTask;
            while (m_tasks.Count > 0 &&
                Interlocked.Read(ref m_PendingRun) == 1 &&
                Interlocked.Equals(m_isOMCAvailable, 1L))
            {
                int index = WaitHandle.WaitAny(m_idleHandles);
                lock (m_tasks)
                {
                    currentTask = m_tasks.Dequeue();
                    if (CheckAlreadyWork(currentTask.TaskID, currentTask.OnuID))
                    {
                        m_tasks.Enqueue(currentTask);
                        continue;
                    }
                }
                SendLog(string.Format("Thread {0} get task {1}", index.ToString(), currentTask.TaskID));
                m_tl1Clients[index].SetTask(currentTask);
            }
        }

        private void DispatchOltOfflineTask()
        {
            Task currentTask;
            int i = m_oltOfflineTasks.Count;
            for (; i > 0; i--)
            {
                if (Interlocked.Read(ref m_PendingRun) == 1 &&
                Interlocked.Equals(m_isOMCAvailable, 1L))
                {
                    int index = WaitHandle.WaitAny(m_idleHandles);
                    lock (m_oltOfflineTasks)
                    {
                        currentTask = m_oltOfflineTasks.Dequeue();
                    }
                    SendLog(string.Format("Thread {0} get oltoffline task {1}", index.ToString(), currentTask.TaskID));
                    m_tl1Clients[index].SetTask(currentTask);
                }
                else
                    break;
            }
        }
        #endregion

        #region 取工单
        /// <summary>
        /// 可以考虑先判断队列中还有未完成的工单则不取
        /// </summary>
        private void GetActiveTask()
        {
            GetNormalTask();//取正向开通工单
            GetRollbackhisTask();//回滚工单
            GetRelocateTask();//移机工单
        }

        private void GetNormalTask()
        {
            string condition = m_condition + " and id>" + m_maxTaskID;
            DataTable dt = SQLUtil.GetTask(m_Connstr, TASK_VIEW_NAME, condition);
            EnqueueTask(dt, false, ref m_maxTaskID);
        }

        private void GetRollbackhisTask()
        {
            string condition = m_condition + " and id>" + m_maxRollTaskID;
            DataTable dt = SQLUtil.GetTask(m_Connstr, ROLLBACKHISTASK_VIEW_NAME, condition);
            EnqueueTask(dt, false, ref m_maxRollTaskID);
        }

        private void GetRelocateTask()
        {
            long id = 0L;
            DataTable dt = SQLUtil.GetTask(m_Connstr, RELOCATETASK_VIEW_NAME, m_condition);
            EnqueueTask(dt, true, ref id);
        }

        private void EnqueueTask(DataTable tasks, bool isRelocate, ref long maxID)
        {
            if (tasks.Rows.Count > 0)
            {
                foreach (DataRow dr in tasks.Rows)
                {
                    Task task = IdlUtil.ParseTaskFromDataRow(dr, m_Connstr);
                    task.IsRelocateTask = isRelocate;
                    lock (m_tasks)
                    {
                        //要检查出列代执行的工单
                        if (!m_tasks.Contains(task) && !CheckAlreadyWork(task))
                        {
                            m_tasks.Enqueue(task);

                            if (task.Id > maxID)
                                maxID = task.Id;
                        }
                    }
                }
            }
        }

        //
        private bool CheckAlreadyWork(Task task)
        {
           // return (task.Equals(m_tl1Clients[0].Task)) || (task.Equals(m_tl1Clients[1].Task));

            return (task.Equals(m_tl1Clients[0].Task)) || (task.Equals(m_tl1Clients[1].Task)) || (task.Equals(m_tl1Clients[2].Task)) || (task.Equals(m_tl1Clients[3].Task));
        }

        /// <summary>
        /// 检测当前loid工单是否在执行
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="onuID"></param>
        /// <returns></returns>
        private bool CheckAlreadyWork(string taskID, string onuID)//需要确认跨OLT跨PON口是否会有重复LOID
        {
            if (m_tl1Clients[0].Task != null)
            {
                if (m_tl1Clients[0].Task.OnuID == onuID)
                {
                    SendLog(string.Format("Thread 0 loid {0} 正被task {1} 使用", onuID, taskID));
                    return true;
                }
            }
            if (m_tl1Clients[1].Task != null)
            {
                if (m_tl1Clients[1].Task.OnuID == onuID)
                {
                    SendLog(string.Format("Thread 1 loid {0} 正被task {1} 使用", onuID, taskID));
                    return true;
                }
            }

            if (m_tl1Clients[2].Task != null)
            {
                if (m_tl1Clients[2].Task.OnuID == onuID)
                {
                    SendLog(string.Format("Thread 2 loid {0} 正被task {1} 使用", onuID, taskID));
                    return true;
                }
            }
            if (m_tl1Clients[3].Task != null)
            {
                if (m_tl1Clients[3].Task.OnuID == onuID)
                {
                    SendLog(string.Format("Thread 3 loid {0} 正被task {1} 使用", onuID, taskID));
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 事件和同步
        private void SetOMCAvailable()
        {
            Interlocked.Exchange(ref m_isOMCAvailable, 1L);
        }

        private void SetOMCUnavailable()
        {
            Interlocked.Exchange(ref m_isOMCAvailable, 0L);
        }

        private bool IsAdapterRunning()
        {
            if (Interlocked.Read(ref m_PendingRun) == 0L)
                return false;
            return true;
        }

        private void RaiseOmcAlarm(string msg)
        {
            try
            {
                RaiseNMAlarm(omcName, Constants.MO_SOURCEOMC, Constants.TKALM_OMCALM, msg, DateTime.Now);
                SQLUtil.RaiseOMCAlarm(m_Connstr, m_omc.Omc);
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
        }

        //error login handler
        //broken connection handler
        /// <summary>
        /// msg生成告警用
        /// </summary>
        /// <param name="msg"></param>
        private void OnNetInterruptHandler(string msg)
        {
            lock (m_connectOMCLocker)
            {
                if (Interlocked.Equals(m_isOMCAvailable, 1L) &&
                    CheckAllThreadNet())
                {
                    SetOMCUnavailable();
                    SQLUtil.UpdateSvrStatus(m_Connstr, "0", "服务器连接断开！", m_SvrID.ToString());
                    RaiseOmcAlarm(msg);
                }
            }
        }

        private bool CheckAllThreadNet()
        {
           // return !m_tl1Clients[0].IsOMCAvailable && !m_tl1Clients[1].IsOMCAvailable;
            return !m_tl1Clients[0].IsOMCAvailable && !m_tl1Clients[1].IsOMCAvailable && !m_tl1Clients[2].IsOMCAvailable && !m_tl1Clients[3].IsOMCAvailable;
        }

        private void OnConnectedHandler()
        {
            lock (m_connectOMCLocker)
            {
                if (Interlocked.Equals(m_isOMCAvailable, 0L))
                {
                    SetOMCAvailable();
                    SQLUtil.UpdateSvrStatus(m_Connstr, "1", "服务器连接成功！", m_SvrID.ToString());
                    ClearNMAlarm(omcName, Constants.MO_SOURCEOMC, Constants.TKALM_OMCALM, DateTime.Now);
                    SQLUtil.ClearOMCAlarm(m_Connstr, m_omc.Omc);
                }
            }
        }

        /// <summary>
        /// 回收网络失败工单和相关处理
        /// </summary>
        /// <param name="task"></param>
        private void EnqueueTask(Task task)
        {
            //olt offline 告警
            if (task.OltOffline)
            {
                RaiseOltAlarm(task);
                lock (m_oltOfflineTasks)
                {
                    if (!m_oltOfflineTasks.Contains(task))
                        m_oltOfflineTasks.Enqueue(task);
                    else
                        SendLog("工单冲突：" + task.TaskID);
                }
            }
            else
            {
                lock (m_tasks)
                {
                    m_tasks.Enqueue(task);
                }
            }
        }

        private void RaiseOltAlarm(Task task)
        {
            string sql = "select * from ftth_activealarm where taskID='" + task.TaskID + "'";
            DataSet ds = new DataSet();
            try
            {
                SqlHelper.FillDataset(m_Connstr, CommandType.Text, sql, ds, new string[] { "OltAlarm" });
                if (ds.Tables["OltAlarm"].Rows.Count == 0)
                {
                    SendLog(string.Format("RaiseOltAlarm:{0}", task.TaskID));
                    string msg = string.Format("OLT:{0}掉线", task.OltID);
                    SQLUtil.RaiseOltAlarm(m_Connstr, m_omc.Omc, task);
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
        }
        #endregion
        #endregion

        #region null
        protected override void _PreStartStage()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void _PostStartStage()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void _DeclareCustomAlarm()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void _PreStopStage()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void _PostStopStage()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void _getNewActiveAlarms(string omcName, string omcConn)
        {
        }

        protected override void _clearStalledActiveAlarms(string omcName, string omcConn)
        {
        }

        protected override void _fetchTransientAlarms(string omcName, string omcConn)
        {
        }

        protected override void _clearForcely()
        {
        }

        protected override TKAlarm ConvertToTKAlarm(HGUAlarm source_alarm)
        {
            TKAlarm tkAlarm = new TKAlarm();
            tkAlarm.BusinessType = BusinessType;
            tkAlarm.City = source_alarm.city;
            tkAlarm.Manufacturer = Manufacturer;

            tkAlarm.NeName = source_alarm.nename;
            tkAlarm.AlarmName = source_alarm.alarmName;
            tkAlarm.Redefinition = source_alarm.alarmName;
            tkAlarm.Severity = source_alarm.Severity;
            tkAlarm.OccurTime = source_alarm.fetchTime;
            tkAlarm.ClearTime = source_alarm.clearTime;
            tkAlarm.Location = source_alarm.location;
            tkAlarm.OMCName = source_alarm.OMCName;
            tkAlarm.ReceiveTime = DateTime.Now.ToString();
            tkAlarm.Reserved2 = source_alarm.description;
            tkAlarm.Reserved3 = source_alarm.TaskId;

            if (source_alarm.TKSn != "")
                tkAlarm.TKSn = source_alarm.TKSn;
            else if (tkAlarm.OccurTime != "" && tkAlarm.ClearTime == "")
            {
                tkAlarm.TKSn = AllocateTKSN().ToString();
            }
            else if (tkAlarm.ClearTime != "")
            {
                string sql = "select TKSn from " + ActiveTable + " where alarmName = '" + source_alarm.alarmName + "' and OMCName='" + source_alarm.OMCName + "'";
                using (System.Data.SqlClient.SqlDataReader sqldr = SqlHelper.ExecuteReader(m_Connstr, CommandType.Text, sql))
                {
                    while (sqldr.Read())
                    {
                        tkAlarm.TKSn = sqldr["TKSn"].ToString();
                        break;
                    }
                    sqldr.Close();
                }
            }

            if (source_alarm.TKSn == "")
                source_alarm.TKSn = tkAlarm.TKSn;
            source_alarm.ManuSn = source_alarm.TKSn;

            tkAlarm.ManuSn = source_alarm.ManuSn;
            return tkAlarm;
        }
        #endregion

        #region 每隔15天清理日志
        void m_Timer_ClearLogger_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_LockTimer)
            {
                if (m_InTimer)
                    return;
                else
                    m_InTimer = true;
            }

            try
            {
                string[] logfiles = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "log" + Path.DirectorySeparatorChar, "*_log.txt");
                foreach (string file in logfiles)
                {
                    try
                    {
                        DateTime t = System.IO.File.GetCreationTime(file);
                        TimeSpan span = DateTime.Now - t;
                        if (span.TotalDays > 30)
                            System.IO.File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        SendLog("清除日志失败:" + ex.Message);
                    }
                }
            }
            finally
            {
                lock (m_LockTimer)
                    m_InTimer = false;
            }
        }
        #endregion

        #region ICommandHandler 成员

        public void handleCommand(ICommunicationMessage message)
        {
            switch (message.TK_CommandType)
            {
                default:
                    break;
            }
        }
        #endregion

        #region ICommandRegister 成员

        public void RegisterCommand()
        {
        }

        public void UnRegisterCommand()
        {
        }

        #endregion
        #endregion

    }
}
