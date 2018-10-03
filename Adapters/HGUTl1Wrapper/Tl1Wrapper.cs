#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: Tl1Wrapper
// 作    者：d.w
// 创建时间：2014/6/19 14:44:05
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
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Threading;
using TelnetLib;
using HGU.Idl;
using HGU.CommandsBuilder;

namespace HGU.Tl1Wrapper
{
    #region 委托
    public delegate void LogHandler(string log);

    public delegate void NetInterruptHandler(string ex);
    public delegate void ConnectedOMCHandler();

    public delegate void EnqueueTaskHandler(Task task);

    public delegate bool IsAdapterRunningHandler();

    public delegate void RaiseOmcAlarmHandler(string msg);
    #endregion

    public class Tl1Wrapper
    {
        #region Constants
        public const string COMMAND_TERMINAL_FLAG = ";";           //终止符
        public const string TASK_TABLE = "Task";
        public const string ROLLBACKHISTASK_TABLE = "RollbackHisTask";
        private const string SP_CLEAR_OLTALARM = "spClearOltAlarm";
        public const int SLEEP_INTERVAL = 100;                    //millionsecond
        const string SHAKEHAND = "SHAKEHAND:::CTAG::;";                                      //握手命令
        const int SHAKEHAND_INTERVAL = 5 * 60 * 1000;                           //millionsecond
        private const int ImsDelay = 60;                      //秒 华为IMS延时
        #endregion

        #region SFields
        private static Encoding s_telnetEncode = Encoding.GetEncoding("latin1");
        #endregion

        #region IFields
        private TelnetHelper m_telconn;                           //
        private string m_ipaddress;                               //Omc ip地址
        private int m_port;                                       //Omc 端口
        private string m_username;                                //Omc 用户
        private string m_password;                                //Omc 密码
        private int m_telnetTimeout;                               //telnet超时 秒
        private Encoding m_omcEncode;
        protected ManuCommandsBuilder m_commBuilder;
        protected List<CommandAndResponsePair> m_commands;
        protected string m_error;
        protected string m_loginStr;
        protected string m_logoutStr;
        private string m_Connstr;

        protected Task m_task;
        private EventWaitHandle m_idleHandle;                          //主程序分配task用
        protected long m_taskExist = 0L;
        protected long m_isOMCAvailable = 0L;                          //omc能否连接
        private int m_Interval;
        protected WatchDog.IWatchDog m_dog;
        protected bool m_isOnuAlreadyExist = false;//ONU是否存在
        protected bool isAddPonVlanZTE = false;//中兴语音时是否添加PON口VLAN
        protected bool isDelOnu = false;//删除业务后是否需要删除ONU
        #endregion

        #region IProperties
        public event LogHandler SendLog;
        public event NetInterruptHandler OnNetInterrupt;
        public event EnqueueTaskHandler EnqueueTask;
        public event IsAdapterRunningHandler IsAdapterRunning;
        public event ConnectedOMCHandler OnConnectedOMC;
        public event RaiseOmcAlarmHandler RaiseOmcAlarm;                //remove?

        public Task Task
        {
            get { return m_task; }
        }

        public bool IsOMCAvailable
        {
            get
            {
                return Interlocked.Equals(m_isOMCAvailable, 1L);
            }
        }
        #endregion

        #region IConstructors
        public Tl1Wrapper(string ipaddress, int port, string username, string password, int interval, string connstr,
            int telnetTimeout, Encoding omcEncoding, EventWaitHandle handle, ManuCommandsBuilder commBuilder,
            WatchDog.IWatchDog dog)
        {
            this.m_ipaddress = ipaddress;
            this.m_port = port;
            this.m_username = username;
            this.m_password = password;
            this.m_Interval = interval * 2;                //
            this.m_Connstr = connstr;
            this.m_telnetTimeout = telnetTimeout;
            this.m_omcEncode = omcEncoding;
            this.m_idleHandle = handle;
            this.m_commBuilder = commBuilder;
            m_commands = new List<CommandAndResponsePair>();
            m_loginStr = m_commBuilder.GetLoginStr(m_username, m_password);
            m_logoutStr = m_commBuilder.GetLogoutStr(username);
            m_dog = dog;
        }
        #endregion

        public virtual void Retriever()//核心业务处理代码
        {
            try
            {
                Connect();
                while (IsAdapterRunning())
                {
                    if (Interlocked.Read(ref m_taskExist) == 1L && Interlocked.Read(ref m_isOMCAvailable) == 1L)
                        DoWork();//business code
                    int i = SHAKEHAND_INTERVAL % SLEEP_INTERVAL;
                    for (; i < SHAKEHAND_INTERVAL; i += SLEEP_INTERVAL)
                    {
                        if ((Interlocked.Read(ref m_taskExist) == 1L && Interlocked.Read(ref m_isOMCAvailable) == 1L) ||
                            !AdapterRunning())
                            break;
                        Thread.Sleep(SLEEP_INTERVAL);
                    }
                    if (i == SHAKEHAND_INTERVAL)
                    {
                       // CheckAccount();
                        if (m_dog != null) m_dog.Feed();
                    }
                }
            }
            catch (Exception ex)
            {
                SendLog("Retriever SOS:" + Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// 处理工单
        /// </summary>
        protected virtual void DoWork()
        {
            try
            {
                while (Interlocked.Read(ref m_taskExist) == 1L &&
                    Interlocked.Read(ref m_isOMCAvailable) == 1L)
                {
                    try
                    {
                        if (m_task.NetInterrupt)
                        {
                            ProcessNetInterrupt(m_task);
                        }
                        else if (m_task.OltOffline)
                        {
                            ProcessOltOffline(m_task);
                        }
                        else
                        {
                            if (m_task.HwIMSDelay && m_task.ExecuteTime.AddSeconds(ImsDelay) > DateTime.Now && m_task.Type == TaskType.AddIMS)//华为IMS延迟
                                EnqueueTask(m_task);
                            else
                                ProcessTask(m_task);//here
                        }
                    }
                    catch (Exception e)
                    {
                        SendLog(e.ToString());
                    }
                    finally
                    {
                        UnlockTask();
                        if (m_dog != null) m_dog.Feed();
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
        }

        #region 同步处理
        public void SetTask(Task task)
        {
            m_task = task;
            Interlocked.Exchange(ref m_taskExist, 1L);
        }

        protected void UnlockTask()
        {
            m_task = null;
            Interlocked.Exchange(ref m_taskExist, 0L);
            UnlockDispatch();
        }

        protected void UnlockDispatch()
        {
            if (Interlocked.Read(ref m_isOMCAvailable) == 1L)
            {
                m_idleHandle.Set();
                SendLog("UnlockDispatch");
            }
        }

        public void SetOmcUnavailable()
        {
            Interlocked.Exchange(ref m_isOMCAvailable, 0L);
        }

        protected void SetOmcAvailable()
        {
            Interlocked.Exchange(ref m_isOMCAvailable, 1L);
            OnConnectedOMC();
        }
        #endregion

        #region 连接Omc

        protected void ConnectEMS()
        {
            m_telconn = new TelnetHelper(m_ipaddress, m_port);
            m_telconn.omcEncoding = m_omcEncode;
        }

        private void DisconnectEMS()
        {
            m_telconn.Close();
            m_telconn = null;
        }

        protected virtual void CheckAccount()
        {
            if (Interlocked.Read(ref m_isOMCAvailable) == 1L)
                ShakeHand();
            else
                Connect();
        }

        /// <summary>
        /// 长连接的方式
        /// </summary>
        private void Connect()
        {
            try
            {
                SendLog("尝试连接OMC");
                ConnectEMS();
                SendLog("尝试登录");
                if (Login(m_loginStr))
                {
                    SendLog("登录成功");
                    if (Interlocked.Read(ref m_isOMCAvailable) == 0L)
                    {
                        SetOmcAvailable();
                        if (m_task == null)
                            UnlockDispatch();
                    }
                }
                else
                {
                    SetOmcUnavailable();
                    OnNetInterrupt("登录失败");
                    SendLog("登录失败");
                }
            }
            catch (Exception ex)
            {
                if (Interlocked.Read(ref m_isOMCAvailable) == 1L)
                {
                    SetOmcUnavailable();
                    NetInterrupt(ex.Message);
                }
                SendLog(ex.ToString());
            }
        }

        /// <summary>
        /// 握手检测连接状态
        /// </summary>
        private void ShakeHand()
        {
            try
            {
                SendLog("开始握手.");
                CommandResponseMsg crm = ExecuteComm(SHAKEHAND);
                if (crm.CompletionCode == CommandResponseStatus.COMPLD)
                    SendLog("握手成功.");
                else
                {
                    SetOmcUnavailable();
                    OnNetInterrupt("握手失败: " + "\r\n" + crm.ReplyContent);
                    SendLog("握手失败: " + "\r\n" + crm.ReplyContent);
                }
            }
            catch (SocketException ex)
            {
                SetOmcUnavailable();
                OnNetInterrupt("握手失败: " + "\r\n" + ex.Message);
                SendLog("握手失败: " + "\r\n" + ex.Message);
            }
            catch (TimeoutException ex)
            {
                SetOmcUnavailable();
                OnNetInterrupt(ex.Message);
                SendLog("握手失败: " + "\r\n" + ex.Message);
            }
            catch (Exception ex)
            {
                SetOmcUnavailable();
                OnNetInterrupt(ex.Message);
                SendLog("握手失败: " + "\r\n" + ex.Message);
                SendLog("ShakeHand SOS:" + Environment.NewLine + ex.ToString());
            }
        }
        #endregion

        #region 工单处理

        #region 获取工单对应的命令集
        /// <summary>
        /// 获取工单对应的命令集 适用于华为、烽火、贝尔
        /// </summary>
        /// <param name="task"></param>
        protected virtual void GetCommandList(Task task)
        {
            GetVerifyOnuCommand(task, 1);  //1.核查ONU

            if (task.Type == TaskType.NewBroadband)
            {
                #region 宽带新装
                GetAddOnuCommand(task, 2);   //2.根据需要添加ONU
                GetAddManagBuinss(task, 3);//3.管理业务流配置 
                GetTaskCommandList(task, 4); //4.业务配置
                GetUPBandwidthCommand(task, 5); //5.带宽限速配置
                #endregion
            }
            if (task.Type == TaskType.NewIMS)
            {
                #region IMS新装
                GetAddOnuCommand(task, 2);  //2.根据需要添加ONU     
                GetAddManagBuinss(task, 3);//3.管理业务流配置 
                GetTaskCommandList(task, 4);//4.业务配置
                #endregion
            }
            if (task.Type == TaskType.AddBroadband)
            {
                #region 宽带加装
                GetTaskCommandList(task, 2);//2.业务配置
                GetUPBandwidthCommand(task, 3);//3.带宽限速配置
                #endregion
            }
            if (task.Type == TaskType.AddIMS)
            {
                #region IMS加装
                GetTaskCommandList(task, 2);//2.业务配置
                #endregion
            }
            if (task.Type == TaskType.DelBroadband)
            {
                #region 宽带拆机
                GetVerifyVlanCommand(task, 2);//2.核查资源一致性 
                GetDelManagBuinss(task, 3);//3.删除管理业务流
                GetTaskCommandList(task, 4);//4.删除业务
                GetDelOnuCommand(task, 5);//5.无其他业务删除ONU
                #endregion
            }
            if (task.Type == TaskType.DelIMS)
            {
                #region IMS拆机
                if (task.OldTaskType == TaskType.NewIMS)
                {
                    #region 新装拆机
                    GetVerifyVlanCommand(task, 2);//2.核查资源一致性
                    GetDelManagBuinss(task, 3);//3.删除管理业务流
                    GetTaskCommandList(task, 4);//4.删除业务
                    GetDelOnuCommand(task, 5); //5.无其他业务删除ONU
                    #endregion
                }
                else
                {
                    #region 加装拆机
                    GetTaskCommandList(task, 2);//2.删除业务
                    #endregion
                }
                #endregion
            }
            if (task.Type == TaskType.DelONU)
            {
                #region ONU拆机
                GetTaskCommandList(task, 2);//2.删除业务
                #endregion
            }
            if (task.Type == TaskType.SameInstall)
            {
                #region 同装业务
                GetTaskCommandList(task, 2); //2.业务配置
                #endregion
            }
            if (task.Type == TaskType.NewIPTV)
            {
                #region IPTV新装
                GetAddOnuCommand(task, 2);  //2.根据需要添加ONU     
                GetAddManagBuinss(task, 3);//3.管理业务流配置 
                GetTaskCommandList(task, 4);//4.业务配置
                #endregion
            }
            if (task.Type == TaskType.AddIPTV)
            {
                #region IPTV加装
                GetTaskCommandList(task, 2);//2.业务配置
                #endregion
            }
            if (task.Type == TaskType.DelIPTV)
            {
                #region IPTV拆机
                if (task.OldTaskType == TaskType.NewIPTV)
                {
                    #region 新装拆机
                    GetVerifyVlanCommand(task, 2);//2.核查资源一致性
                    GetDelManagBuinss(task, 3);//3.删除管理业务流
                    GetTaskCommandList(task, 4);//4.删除业务
                    GetDelOnuCommand(task, 5); //5.无其他业务删除ONU
                    #endregion
                }
                else
                {
                    #region 加装拆机
                    GetTaskCommandList(task, 2);//2.删除业务
                    #endregion
                }
                #endregion
            }
        }
        #endregion

        #region 获取工单业务命令
        /// <summary>
        /// 获取工单业务命令
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetTaskCommandList(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取管理业务流配置
        /// <summary>
        /// 获取管理业务流配置
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetAddManagBuinss(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.ManageBusiness);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取删除管理业务流配置
        /// <summary>
        /// 获取删除管理业务流配置
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetDelManagBuinss(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.DelManageBusiness);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取查询ONU命令
        /// <summary>
        /// 获取查询ONU命令
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetVerifyOnuCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.VerifyOnu);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取添加ONU命令
        /// <summary>
        /// 获取添加ONU命令
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetAddOnuCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.AddOnu);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取删除ONU命令
        /// <summary>
        /// 获取删除ONU命令
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetDelOnuCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.DelOnu);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取查询宽带、语音业务命令[svlan,cvlan：烽火没有]（烽火、贝尔暂时不支持语音业务查询）
        /// <summary>
        /// 获取查询宽带、语音业务命令[svlan,cvlan：烽火没有]（烽火、贝尔暂时不支持语音业务查询）
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetVerifyVlanCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.VerifyVlan);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取语音端口查询命令[onuPort,pn] （贝尔不支持号码查询）
        /// <summary>
        /// 获取语音端口查询命令[onuPort,pn] （贝尔不支持号码查询）
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetVerifyImsCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.VerifyIMS);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region  获取带宽限速命令
        /// <summary>
        /// 获取带宽限速命令
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetUPBandwidthCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.UPBandwidth);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }

        #endregion

        #endregion

        #region 执行工单,网络异常直接入列下次再执行且不判断
        /// <summary>
        /// 执行工单,网络异常直接入列下次再执行且不判断
        /// </summary>
        /// <returns></returns>
        protected virtual void ExecuteTask(Task task)
        {
            try
            {
                task.ExecuteTime = DateTime.Now;
                m_commands.Clear();
                m_error = string.Empty;
                GetCommandList(task);
                isAddOnu = false;
                isAddPonVlanZTE = false;
                isDelOnu = false;

                #region
                try
                {
                    switch (task.Type)
                    {
                        case TaskType.NewBroadband:
                            ExecuteNewBroadband(task);
                            break;
                        case TaskType.NewIMS:
                            ExecuteNewIMS(task);
                            break;
                        case TaskType.NewIPTV:
                            ExecuteNewIPTV(task);
                            break;
                        case TaskType.AddBroadband:
                            ExecuteAddBroadband(task);
                            break;
                        case TaskType.AddIMS:
                            ExecuteAddIMS(task);
                            break;
                        case TaskType.AddIPTV:
                            ExecuteAddIPTV(task);
                            break;
                        case TaskType.DelBroadband:
                            ExecuteDelBroadband(task);
                            break;
                        case TaskType.DelIMS:
                            ExecuteDelIMS(task);
                            break;
                        case TaskType.DelIPTV:
                            ExecuteDelIPTV(task);
                            break;
                        case TaskType.DelONU:
                            ExecuteDelOnu(task);
                            break;
                        case TaskType.SameInstall:
                            ExecuteSameInstall(task);
                            break;
                        default:
                            break;
                    }
                }
                catch (SocketException ex)
                {
                    SetOmcUnavailable();
                    /////////OnNetInterrupt("command:" + pair.Command.CommandText + "\r\n" + ex.Message);
                    SendLog(task.TaskID + "\r\n" + ex.ToString());
                    //网络导致失败回滚、重发
                    //需回滚的就开机工单
                    if (!task.IsRollbackTask)
                    {
                        //工单加一个标识位
                        //todo
                        task.NetInterrupt = true;
                    }
                }
                catch (TimeoutException ex)
                {
                    SetOmcUnavailable();
                    OnNetInterrupt(ex.Message);
                    /////////SendLog(task.TaskID + "\r\n" + "command:" + pair.Command.CommandText + "\r\n" + ex.Message);
                    //网络导致失败回滚、重发
                    //需回滚的就开机工单
                    if (!task.IsRollbackTask)
                    {
                        //工单加一个标识位
                        //todo
                        task.NetInterrupt = true;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                SendLog(task.TaskID + "\r\n" + ex.ToString());
            }
        }
        #endregion

        #region 各种业务

        protected bool isAddOnu = false;//是否添加ONU

        #region 宽带新装业务(适用于华为、中兴、烽火)
        /// <summary>
        /// 宽带新装业务(适用于华为、中兴、烽火)
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteNewBroadband(Task task)
        {
            int step = 1;//1.核查ONU 2.根据需要添加ONU 3.管理业务流配置 4.业务配置

            #region 流程
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 2 || step 3
                //添加ONU、管理业务流配置都需要配置ONU是否存在
                if (pair.Step == 2 || pair.Step == 3)
                {
                    if (!isAddOnu)
                        continue;
                }
                #endregion

                try
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                        #region step 1 验证ONU处理,判定命令执行成功
                        if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                        {
                            //因是新装，ONU不存在，判断命令执行为成功
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;//认为命令执行成功
                            isAddOnu = true;
                            continue;
                        }
                        #endregion

                        break;
                    }
                }
                catch { }

                step++;
            }
            #endregion
        }
        #endregion

        #region IMS（语音）新装业务(适用于华为、烽火)
        /// <summary>
        ///  IMS（语音）新装业务(适用于华为、烽火)
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteNewIMS(Task task)
        {
            int step = 1;//1.核查ONU 2.更具需要添加ONU 3.业务配置

            #region 流程
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 2 || step 3
                //添加ONU、管理业务流配置都需要配置ONU是否存在
                if (pair.Step == 2 || pair.Step == 3)
                {
                    if (!isAddOnu)
                        continue;
                }
                #endregion

                try
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                        #region step 1 验证ONU处理，判定命令执行成功
                        if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                            isAddOnu = true;
                            continue;
                        }
                        #endregion

                        break;
                    }
                }
                catch { }

                step++;
            }
            #endregion
        }
        #endregion

        #region IPTV新装业务(适用于华为、烽火)
        /// <summary>
        ///  IPTV新装业务(适用于华为、烽火)
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteNewIPTV(Task task)
        {
            int step = 1;//1.核查ONU 2.更具需要添加ONU 3.业务配置

            #region 流程
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 2 || step 3
                //添加ONU、管理业务流配置都需要配置ONU是否存在
                if (pair.Step == 2 || pair.Step == 3)
                {
                    if (!isAddOnu)
                        continue;
                }
                #endregion

                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                    #region step 1 验证ONU处理，判定命令执行成功
                    if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                        isAddOnu = true;
                        continue;
                    }
                    #endregion

                    break;
                }
                step++;
            }
            #endregion
        }
        #endregion

        #region 宽带加装业务（适用于华为、中兴）
        /// <summary>
        /// 宽带加装业务（适用于华为、中兴）
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteAddBroadband(Task task)
        {
            int step = 1;//1.ONU核查 2.业务配置

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    break;
                }

                #region step 2 注释了 宽带语音业务流核查LST-PORTVLAN（SvlanOrCvlanException，BbAlreadyExist）
                //if (pair.Step == 2 && pair.Msg.Information.Count > 0) 
                //{
                //    foreach (Dictionary<string, string> item in pair.Msg.Information)
                //    {
                //        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN"))
                //        {
                //            int svlan = int.Parse(item["SVLAN"]);
                //            int cvlan = int.Parse(item["CVLAN"]);
                //            //判断是否属于宽带
                //            if (svlan >= BB_MIN_SVLAN && svlan <= BB_MAX_SVLAN && cvlan >= BB_MIN_CVLAN && cvlan <= BB_MAX_CVLAN)
                //            {//是
                //                pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                pair.Msg.ErrorDesc = ErrorDesc.BbAlreadyExist;
                //                m_error = string.Format(" svlan：{0} cvlan：{1}", svlan, cvlan);
                //                return;
                //            }
                //            //判断是否属于语音
                //            if (svlan < IMS_MIN_SVLAN || svlan > IMS_MAX_SVLAN || cvlan != IMS_CVLAN)
                //            {//否
                //                pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                pair.Msg.ErrorDesc = ErrorDesc.SvlanOrCvlanException;
                //                m_error = string.Format(" svlan：{0} cvlan：{1}", svlan, cvlan);
                //                return;
                //            }
                //        }
                //    }
                //}
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region IMS加装业务（适用于华为）
        /// <summary>
        /// IMS加装业务（适用于华为）
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteAddIMS(Task task)
        {
            int step = 1;//1.ONU核查 2.宽带语音业务流核查LST-PORTVLAN（SvlanOrCvlanException,ImsSvlanOrCvlanUnlike，ImsPortAlreadyExist）3.核查语音端口号 4.业务配置
            foreach (CommandAndResponsePair pair in m_commands)
            {
                try
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        //IMS延迟
                        if (pair.Command.CommandText.IndexOf("LST-POTS") >= 0 && pair.Msg.ErrorDesc == ErrorDesc.HwIMSDelay)
                        {
                            task.HwIMSDelay = true;
                        }
                        break;
                    }
                }
                catch { }

                #region step 2 注释了 宽带语音业务流核查LST-PORTVLAN（SvlanOrCvlanException,ImsSvlanOrCvlanUnlike，ImsPortAlreadyExist）
                //if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                //{
                //    foreach (Dictionary<string, string> item in pair.Msg.Information)
                //    {
                //        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN"))
                //        {
                //            int svlan = int.Parse(item["SVLAN"]);
                //            int cvlan = int.Parse(item["CVLAN"]);
                //            //判断是否属于语音
                //            if (svlan >= IMS_MIN_SVLAN && svlan <= IMS_MAX_SVLAN && cvlan == IMS_CVLAN)
                //            {//是
                //                //判断是否一致
                //                if (svlan != task.Svlan || cvlan != task.Cvlan)
                //                {//否
                //                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                    pair.Msg.ErrorDesc = ErrorDesc.ImsSvlanOrCvlanUnlike;
                //                    m_error = string.Format(" svlan：{0} cvlan：{1} 现网svlan：{2} 现网cvlan：{3} ", task.Svlan, task.Cvlan, svlan, cvlan);
                //                    return;
                //                }
                //                continue;
                //            }
                //            //判断是否属于宽带
                //            if (svlan < BB_MIN_SVLAN || svlan > BB_MAX_SVLAN || cvlan < BB_MIN_CVLAN || cvlan > BB_MAX_CVLAN)
                //            {//否
                //                pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                pair.Msg.ErrorDesc = ErrorDesc.SvlanOrCvlanException;
                //                m_error = string.Format(" svlan：{0} cvlan：{1}", svlan, cvlan);
                //                return;
                //            }
                //        }
                //    }
                //}
                #endregion

                #region step 3 注释了 核查语音端口号
                //if (pair.Step == 3 && pair.Msg.Information.Count > 0)
                //{
                //    if (task.HwIMSDelay)
                //        task.HwIMSDelay = false;
                //    foreach (Dictionary<string, string> item in pair.Msg.Information)
                //    {
                //        if (item.ContainsKey("ONUPORT") && item.ContainsKey("PN"))
                //        {
                //            string pn = item["PN"].ToString();
                //            string port = item["ONUPORT"].ToString();
                //            if (pn != "--")
                //            {
                //                //判断端口号是否被用
                //                if (task.OnuPort == port)
                //                {
                //                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                    pair.Msg.ErrorDesc = ErrorDesc.ImsPortAlreadyExist;
                //                    m_error = string.Format(" port：{0}", task.OnuPort);
                //                    return;
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion

                step++;
            }
        }
        #endregion

        #region IPTV加装业务（适用于华为）
        /// <summary>
        /// IPTV加装业务（适用于华为）
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteAddIPTV(Task task)
        {
            int step = 1;//1.ONU核查 2.业务配置
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    if (pair.Command.CommandText.IndexOf("LST-POTS") >= 0 && pair.Msg.ErrorDesc == ErrorDesc.HwIMSDelay)
                    {
                        task.HwIMSDelay = true;
                    }
                    break;
                }
                step++;
            }
        }
        #endregion

        #region 宽带拆机业务（适用于华为）
        //适用于华为
        protected virtual void ExecuteDelBroadband(Task task)
        {
            int step = 1;//1.ONU核查 2.核查资源一致性(lst-portvlan) 3.删除管理业务流 4.删除业务 5.无其他业务删除ONU

            #region 流程
            bool isDelBroadband = true;
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 4 删除业务
                if (pair.Step == 4)
                {
                    if (!isDelBroadband)
                        continue;
                }
                #endregion

                #region step 5 无其他业务删除ONU
                if (pair.Step == 5)
                {
                    if (!isDelOnu)
                        continue;
                }
                #endregion

                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                    #region step 3 因同拆的时候，都需删除管理业务流，为防止后执行删管理业务流出错，则默认给成功。
                    if (pair.Step == 3 && pair.Command.CommandText.IndexOf("DEL-PONVLAN::") >= 0
                          && pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                        continue;
                    }
                    #endregion

                    if (pair.Step == 5 && task.OldTaskType == TaskType.AddBroadband)
                        m_error = " 该业务为ONU上唯一业务，业务删除成功，ONU删除失败";
                    break;
                }

                #region step 2 核查资源一致性
                if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                {
                    list = pair.Msg.Information;
                    int svlan = 0, cvlan = 0, uv = 0;
                    foreach (Dictionary<string, string> item in pair.Msg.Information)
                    {
                        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                        {
                            try
                            {
                                if (item["SVLAN"] != "")
                                {
                                    svlan = int.Parse(item["SVLAN"]);
                                }
                                if (item["CVLAN"] != "")
                                {
                                    cvlan = int.Parse(item["CVLAN"]);
                                }
                                if (item["UV"] != "")
                                {
                                    uv = int.Parse(item["UV"]);
                                }
                                if (svlan == task.Svlan && cvlan == task.Cvlan && uv == task.Uvlan)
                                {
                                    isDelBroadband = true;
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    if (!isDelBroadband)
                    {
                        #region 数据与现网不一致工单
                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.BbNotExist;
                        m_error = string.Format(" Svlan：{0} Cvlan：{1} 现网Svlan：{2} 现网Cvlan：{3} ", task.Svlan, task.Cvlan, svlan, cvlan);
                        #endregion
                    }
                }
                else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                {
                    isDelBroadband = false;
                    if (task.IsRollbackTask)//回滚
                        continue;

                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                    pair.Msg.ErrorDesc = ErrorDesc.BbNotExist;
                    return;
                }
                #endregion

                #region step 3 删除管理业务流
                if (pair.Step == 3)
                {
                    continue;
                }
                #endregion

                #region step 4 判断是否存在其他业务
                if (pair.Step == 4)
                {
                    int otherCount = 0; //其他业务数量
                    int svlan = 0, cvlan = 0, uv = 0;
                    foreach (Dictionary<string, string> item in list)
                    {
                        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                        {
                            try
                            {
                                if (item["SVLAN"] != "")
                                {
                                    svlan = int.Parse(item["SVLAN"]);
                                }
                                if (item["CVLAN"] != "" && item["CVLAN"] != "--")
                                {
                                    cvlan = int.Parse(item["CVLAN"]);
                                }
                                if (item["UV"] != "" && item["UV"] != "--")
                                {
                                    uv = int.Parse(item["UV"]);
                                }
                                if (svlan != task.Svlan || cvlan != task.Cvlan || uv != task.Uvlan)
                                {
                                    otherCount++;
                                }
                            }
                            catch { }
                        }
                    }
                    if (otherCount > 1)
                        isDelOnu = false;
                    else
                        isDelOnu = true;
                }
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region IMS拆机业务（适用于华为）
        /// <summary>
        /// IMS拆机业务（适用于华为）
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteDelIMS(Task task)
        {
            if (task.OldTaskType == TaskType.NewIMS)
            {
                #region 新装拆机流程
                int step = 1; //1.ONU核查  2.核查资源一致性  3.删除管理业务流   4.删除业务  5.无其他业务删除ONU

                #region 流程
                bool isDeleteIMS = true;
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    #region step 4 删除业务
                    if (pair.Step == 4)
                    {
                        if (!isDeleteIMS)
                            continue;
                    }
                    #endregion

                    #region step 5 无其他业务删除ONU
                    if (pair.Step == 5)
                    {
                        if (!isDelOnu)
                            continue;
                    }
                    #endregion

                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        #region step 3 因同拆的时候，都需删除管理业务流，为防止后执行删管理业务流出错，则默认给成功。
                        if (pair.Step == 3 && pair.Command.CommandText.IndexOf("DEL-PONVLAN::") >= 0
                              && pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                            continue;
                        }
                        #endregion

                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        if (pair.Step == 5)
                            m_error = " 该业务为ONU上唯一业务，业务删除成功，ONU删除失败";
                        break;
                    }

                    #region step 2 核查资源一致性
                    if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                    {
                        list = pair.Msg.Information;
                        int svlan = 0, cvlan = 0, uv = 0;
                        foreach (Dictionary<string, string> item in pair.Msg.Information)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan == task.Svlan && cvlan == task.Cvlan && uv == task.Uvlan)
                                    {
                                        isDeleteIMS = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (!isDeleteIMS)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                            pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                            m_error = string.Format(" Svlan：{0} Cvlan：{1} UV:{2} 现网Svlan：{3} 现网Cvlan：{4} 现网UV：{5} ", task.Svlan, task.Cvlan, task.Uvlan, svlan, cvlan, uv);
                            break;
                        }
                    }
                    else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                    {
                        isDeleteIMS = false;
                        if (task.IsRollbackTask)//回滚
                            continue;

                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                        return;
                    }
                    #endregion

                    #region step 3 删除管理业务流
                    if (pair.Step == 3)
                    {
                        continue;
                    }
                    #endregion

                    #region step 4 判断是否存在宽带业务
                    if (pair.Step == 4)
                    {
                        int otherCount = 0; //其他业务数量
                        int svlan = 0, cvlan = 0, uv = 0;
                        foreach (Dictionary<string, string> item in list)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "" && item["CVLAN"] != "--")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "" && item["UV"] != "--")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan != task.Svlan || cvlan != task.Cvlan || uv != task.Uvlan)
                                    {
                                        otherCount++;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (otherCount > 0)
                            isDelOnu = false;
                        else
                            isDelOnu = true;
                    }
                    #endregion

                    step++;

                }
                #endregion

                #endregion
            }
            else
            {
                #region 加装拆机流程
                int step = 1;//1.ONU核查 2.删除业务
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        break;
                    }
                    step++;
                }
                #endregion
            }
        
        }
        #endregion

        #region IPTV拆机业务（适用于华为）
        /// <summary>
        /// IMS拆机业务（适用于华为）
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteDelIPTV(Task task)
        {
            if (task.OldTaskType == TaskType.NewIPTV)
            {
                #region 新装拆机流程
                int step = 1; //1.ONU核查  2.核查资源一致性  3.删除管理业务流   4.删除业务  5.无其他业务删除ONU

                #region 流程
                bool isDeleteIMS = true;
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    #region step 4 删除业务
                    if (pair.Step == 4)
                    {
                        if (!isDeleteIMS)
                            continue;
                    }
                    #endregion

                    #region step 5 无其他业务删除ONU
                    if (pair.Step == 5)
                    {
                        if (!isDelOnu)
                            continue;
                    }
                    #endregion

                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        #region step 3 因同拆的时候，都需删除管理业务流，为防止后执行删管理业务流出错，则默认给成功。
                        if (pair.Step == 3 && pair.Command.CommandText.IndexOf("DEL-PONVLAN::") >= 0
                              && pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                            continue;
                        }
                        #endregion

                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        if (pair.Step == 5)
                            m_error = " 该业务为ONU上唯一业务，业务删除成功，ONU删除失败";
                        break;
                    }

                    #region step 2 核查资源一致性
                    if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                    {
                        list = pair.Msg.Information;
                        int svlan = 0, cvlan = 0, uv = 0;
                        foreach (Dictionary<string, string> item in pair.Msg.Information)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan == task.Svlan && cvlan == task.Cvlan && uv == task.Uvlan)
                                    {
                                        isDeleteIMS = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (!isDeleteIMS)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                            pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                            m_error = string.Format(" Svlan：{0} Cvlan：{1} UV:{2} 现网Svlan：{3} 现网Cvlan：{4} 现网UV：{5} ", task.Svlan, task.Cvlan, task.Uvlan, svlan, cvlan, uv);
                            break;
                        }
                    }
                    else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                    {
                        isDeleteIMS = false;
                        if (task.IsRollbackTask)//回滚
                            continue;

                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                        return;
                    }
                    #endregion

                    #region step 3 删除管理业务流
                    if (pair.Step == 3)
                    {
                        continue;
                    }
                    #endregion

                    #region step 4 判断是否存在宽带业务
                    if (pair.Step == 4)
                    {
                        int otherCount = 0; //其他业务数量
                        int svlan = 0, cvlan = 0, uv = 0;
                        foreach (Dictionary<string, string> item in list)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "" && item["CVLAN"] != "--")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "" && item["UV"] != "--")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan != task.Svlan || cvlan != task.Cvlan || uv != task.Uvlan)
                                    {
                                        otherCount++;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (otherCount > 0)
                            isDelOnu = false;
                        else
                            isDelOnu = true;
                    }
                    #endregion

                    step++;
                }
                #endregion

                #endregion
            }
            else
            {
                #region 加装拆机流程
                int step = 1;//1.ONU核查 2.删除业务
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        break;
                    }
                    step++;
                }
                #endregion
            }
        }
        #endregion

        #region ONU拆机业务(适用于华为、中兴、烽火)
        /// <summary>
        ///  ONU拆机业务(适用于华为、中兴、烽火)
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteDelOnu(Task task)
        {
            int step = 1;//1.ONU核查 2.删除ONU
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);//socket登陆网管命令操作
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    break;
                }
                step++;
            }
        }
        #endregion

        #region 业务回滚(适用于中兴)
        /// <summary>
        /// 业务回滚
        /// </summary>
        /// <param name="task"></param>
        protected virtual void ExecuteRollBack(Task task)
        {
            int step = 1;//1.ONU核查 2.删除业务 3.判断是否存在其他业务 4.无其他业务删除ONU
            bool isDelYW = true;   //删除业务

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 2 删除业务
                if (pair.Step == 2)
                {
                    if (!isDelYW)
                        continue;
                }
                #endregion

                #region step 4 无其他业务删除ONU
                if (pair.Step == 4)
                {
                    if (!isDelOnu)
                        continue;
                }
                #endregion

                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    if (pair.Step == 4)
                        m_error = " 该业务为ONU上唯一业务，业务删除成功，ONU删除失败";
                    break;
                }

                #region step 3 判断是否存在其他业务
                if (pair.Step == 3 && pair.Msg.Information.Count > 0)
                {
                    isDelOnu = true;
                    foreach (Dictionary<string, string> item in pair.Msg.Information)
                    {
                        if (item.ContainsKey("UV"))
                        {
                            string pn = item["UV"].ToString();
                            if (pn != "--")
                            {
                                isDelOnu = false;
                            }
                        }
                        else
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                            pair.Msg.ErrorDesc = ErrorDesc.InformationException;
                            m_error = " 删除宽带业务成功，核查其他业务失败（UV不存在）";
                            return;
                        }
                    }
                }
                #endregion

                step++;
            }
            #endregion
        }

        #endregion

        #region 同装业务
        protected virtual void ExecuteSameInstall(Task task)
        {
            int step = 1;//1.核查ONU 2.根据需要添加ONU 3.管理业务流配置 4.宽带业务配置 5.带宽限速配置 6.IMS业务配置

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    #region step 1 验证ONU处理,判定命令执行成功
                    if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                    {
                        //因是新装，ONU不存在，判断命令执行为成功
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;//认为命令执行成功
                        isAddOnu = true;
                        continue;
                    }
                    #endregion

                    break;
                }
                step++;
            }
            #endregion
        }
        #endregion

        #endregion

        #region 命令结果入库
        /// <summary>
        /// 命令结果入库，需要改
        /// </summary>
        /// <param name="isRollbackCommands">是否回滚命令</param>
        private void LogCommands(Task task)
        {
            foreach (CommandAndResponsePair pair in m_commands)
            {
                SQLUtil.InsertCommandAndRes(m_Connstr, task.TaskID, pair, task.Rollback);
            }
            m_commands.Clear();
        }
        private void LogCommands(Task task, CommandAndResponsePair pair)
        {
            SQLUtil.InsertCommandAndRes(m_Connstr, task.TaskID, pair, task.Rollback);
        }
        #endregion

        #region 检查工单执行结果,如果失败则网络异常
        /// <summary>
        /// 检查工单执行结果,如果失败则网络异常
        /// </summary>
        /// <param name="task"></param>
        private void CheckTask(Task task)
        {
            string errorMsg = string.Empty;
            task.CompleteTime = DateTime.Now;
            int i = 0;

            #region 网络异常
            if (task.NetInterrupt && !task.IsRollbackTask)
            {
                EnqueueTask(task);
                LogCommands(task);
                SendLog("NetInterrupt:" + task.TaskID);
                return;
            }
            #endregion

            #region 华为IMS延迟
            if (task.HwIMSDelay && task.Type == TaskType.AddIMS)
            {
                EnqueueTask(task);
                LogCommands(task);
                SendLog("IMSDelay:" + task.TaskID);
                return;
            }
            #endregion

            for (; i < m_commands.Count; i++)
            {
                if (!m_commands[i].Command.Execute)
                    continue;

                if (m_commands[i].Msg == null)
                {
                    //errorMsg = "工单执行失败，命令：" + m_commands[i].Command.CommandText + " OMC未响应";
                    errorMsg = "工单执行失败，OMC未响应";
                    i = -1;
                    break;
                }
                if (m_commands[i].Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    //errorMsg = "工单执行失败，命令：" + m_commands[i].Command.CommandText + " " +
                    errorMsg = "工单执行失败，" +
                        (m_commands[i].Msg.ErrorDesc == ErrorDesc.Unknown ?
                        "OMC错误，请与本地市" + task.Manufacturer + "OLT维护人员联系 " + m_commands[i].Msg.GetAddInfo() + m_error : IdlUtil.ParseErrorDescToString(m_commands[i].Msg.ErrorDesc, task, m_error, m_Connstr));
                    task.Error = m_commands[i].Msg.ErrorDesc;
                    i = -1;
                    break;
                }
            }
            LogCommands(task);
            #region 失败
            if (i == -1)
            {
                task.Status = TaskStatus.Fail;
                task.ResponseMsg = errorMsg;
                AdditionalRules(task);
                if (task.Status == TaskStatus.Fail)
                {
                    if (!task.IsRollbackTask && task.NeedRollback() &&
                        task.Error != ErrorDesc.OltOffline &&
                        //task.Error != ErrorDesc.OnuAlreadyExist &&
                        task.Manufacturer != "华为")//华为无法进行LST-POTS查询，需要用户端激活才可以查询
                    {
                        RollbackTask(task);
                    }
                }
            }
            #endregion

            #region 成功
            else
            {
                SendLog("CheckTask Succeed:" + task.TaskID);
                if (task.Type != TaskType.DelBroadband && task.Type != TaskType.DelIMS && task.Type != TaskType.DelONU)
                    task.ResponseMsg = "工单执行成功，数据已下发至 " + task.OltID + " Olt " + task.PonID + " Pon口";
                else
                    task.ResponseMsg = "工单执行成功";
                task.Status = TaskStatus.Succeed;
            }
            #endregion
        }
        #endregion

        #region 工单错误的特殊处理
        /// <summary>
        /// 工单错误的特殊处理
        /// </summary>
        /// <param name="task"></param>
        private void AdditionalRules(Task task)
        {
            SendLog("AdditionalRules:" + task.TaskID);
            //拆机工单onu不存在判定成功
            //if (task.Type == TaskType.DelONU && task.Error == ErrorDesc.OnuNotExist)
            //{
            //    task.Status = TaskStatus.Succeed;
            //    task.Error = ErrorDesc.None;
            //    task.ResponseMsg = "工单执行成功";
            //}

            //开机回滚工单失败，如果是ONU不存在则判断成功 2014-09-29 by d.w
            if (task.IsRollbackTask == true && task.Status == TaskStatus.Fail && task.Error == ErrorDesc.OnuNotExist)
            {
                task.Status = TaskStatus.Succeed;
                task.Error = ErrorDesc.None;
                return;
            }
            //开机宽带回滚工单失败，如果宽带业务不存在则判断成功 2014-09-29 by d.w
            if (task.IsRollbackTask == true && task.Status == TaskStatus.Fail && task.Type == TaskType.DelBroadband && task.Error == ErrorDesc.BbNotExist)
            {
                task.Status = TaskStatus.Succeed;
                task.Error = ErrorDesc.None;
                return;
            }
            //开机IMS回滚工单失败，如果IMS业务不存在则判断成功 2014-09-29 by d.w
            if (task.IsRollbackTask == true && task.Status == TaskStatus.Fail && task.Type == TaskType.DelIMS && task.Error == ErrorDesc.ImsNotExist)
            {
                task.Status = TaskStatus.Succeed;
                task.Error = ErrorDesc.None;
                return;
            }

            //此处
            if (task.Error == ErrorDesc.OltNotExist && (SQLUtil.GetOlt(m_Connstr, task.OltID) != null))
            {
                task.Error = ErrorDesc.OltOffline;
                if (!task.OltOffline)
                {
                    task.OltOffline = true;
                    //SQLUtil.UpdateOltOfflineTask(m_Connstr, task, GetTaskTableName(task));//OLT掉线处理，暂时不用，数据库字段{oltOffline}还没加
                }
                //OLT掉线处理，暂时不用
                //非回滚工单 olt 离线 则等待下次执行
                //if (!task.IsRollbackTask)
                //{
                //    EnqueueTask(task);
                //    SendLog("OltOffline:" + task.TaskID);
                //}
            }
        }
        #endregion

        #region 回滚告警
        /// <summary>
        /// 回滚告警
        /// </summary>
        /// <param name="task"></param>
        private void RollbackTask(Task task)
        {
            SendLog("RollbackTask:" + task.TaskID);


            Task rollbackTask = task.GetRollbackTask();

            //add 2013-06-27 移机工单装机失败需回滚(错误PON向正在使用PON口移机，不能使用回滚)
            //if (rollbackTask.IsRollbackTask && task.IsRelocateTask)
            //{
            //    //Task delTask = new Task(task.TaskID, TaskType.Delete, task.City, task.Manufacturer, task.OmcName, task.OltID, task.PonID, task.OnuID, task.ReceiveTime, task.Account);
            //    task.IsRelocateTask = false;
            //    Task delTask = task.GetRollbackTask();
            //    task.IsRelocateTask = true;
            //    ExecuteTask(delTask);
            //    //有一个递归
            //    CheckTask(delTask);
            //}

            ExecuteTask(rollbackTask);
            //有一个递归
            CheckTask(rollbackTask);
            if (rollbackTask.Status == TaskStatus.Succeed)
                task.Rollback = true;
            else
                task.Rollback = false;

            //如果是新装工单失败回滚onu不存在判定为已回滚
            if (rollbackTask.Status == TaskStatus.Fail && rollbackTask.Error == ErrorDesc.OnuNotExist)
                task.Rollback = true;
        }
        #endregion

        #region 工单处理
        /// <summary>
        /// 工单处理
        /// </summary>
        /// <param name="task"></param>
        protected void ProcessTask(Task task)
        {
            try
            {
                SendLog("ExecuteTask:" + task.TaskID);
                ExecuteTask(task);//business code processing in right environment
            }
            catch { }

            try
            {
                SendLog("CheckTask:" + task.TaskID);
                CheckTask(task);
            }
            catch { }

            //if (task.OltOffline)//OLT离线工单下次执行//OLT掉线处理，暂时不用
            //{
            //    SendLog(string.Format("task {0} is oltOffLine", task.TaskID));
            //    return;
            //}
            if (!m_task.IsRollbackHisTask)
            {
                SQLUtil.UpdateTask(m_Connstr, m_task, GetTaskTableName(task), false);
                SendLog(string.Format("UpdateTask:{0}", task.TaskID));
            }
            else
            {
                SQLUtil.UpdateTask(m_Connstr, m_task, GetTaskTableName(task), true);
                SendLog(string.Format("UpdateTask:{0}", task.TaskID));
            }
        }
        #endregion

        #region 网络异常的工单的处理
        /// <summary>
        /// 网络异常的工单的处理
        /// 开机工单回滚，其他工单再执行一次，但是判断工单是否成功要变
        /// 如拆机工单第一次断网但成功执行，再执行会有onu不存在的错误
        /// </summary>
        /// <param name="task"></param>
        protected void ProcessNetInterrupt(Task task)
        {
            switch (task.Type)
            {
                case TaskType.NewBroadband:
                case TaskType.NewIMS:
                case TaskType.NewIPTV:
                    Task preRollbackTask = task.GetRollbackTask();
                    ExecuteTask(preRollbackTask);
                    CheckTask(preRollbackTask);
                    if (preRollbackTask.Status != TaskStatus.Succeed)
                    {
                        //失败，不再处理
                        EnqueueTask(task);
                        break;
                    }
                    task.NetInterrupt = false;
                    EnqueueTask(task);
                    break;
                case TaskType.AddBroadband:
                case TaskType.AddIMS:
                case TaskType.AddIPTV:
                case TaskType.DelBroadband:
                case TaskType.DelIMS:
                case TaskType.DelONU:
                case TaskType.DelIPTV:
                    task.NetInterrupt = false;
                    EnqueueTask(task);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region olt掉线的工单再处理
        /// <summary>
        /// olt掉线的工单再处理
        /// </summary>
        /// <param name="task"></param>
        protected void ProcessOltOffline(Task task)
        {
            switch (task.Type)
            {
                case TaskType.NewBroadband:
                case TaskType.NewIMS:
                case TaskType.NewIPTV:
                    Task preRollbackTask = task.GetRollbackTask();
                    ExecuteTask(preRollbackTask);
                    CheckTask(preRollbackTask);
                    if (preRollbackTask.Status != TaskStatus.Succeed)
                    {
                        //失败，不再处理
                        EnqueueTask(task);
                        break;
                    }
                    task.OltOffline = false;
                    ClearOltAlarm(task);
                    EnqueueTask(task);
                    break;
                case TaskType.AddBroadband:
                case TaskType.AddIMS:
                case TaskType.AddIPTV:
                case TaskType.DelBroadband:
                case TaskType.DelIMS:
                case TaskType.DelONU:
                case TaskType.DelIPTV:
                    task.OltOffline = false;
                    ClearOltAlarm(task);
                    EnqueueTask(task);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 命令
        protected CommandResponseMsg ExecuteComm(string command)
        {
            //CommandResponseMsg crm = new CommandResponseMsg();
            //crm.CompletionCode = CommandResponseStatus.COMPLD;
            //return crm;

            string respons = string.Empty;
            if (m_telconn == null)
                ConnectEMS();
            m_telconn.Write(command);
            respons = m_telconn.WaitFor(COMMAND_TERMINAL_FLAG, m_telnetTimeout);
            if (respons == "")
                throw new TimeoutException("EMS未响应");
            return CommandResponseMsg.ConvertFromString(m_commBuilder, ConvertEncoding(respons));
        }

        protected CommandResponseMsg ExecuteComm(Command command)
        {
            //CommandResponseMsg crm = new CommandResponseMsg();
            //crm.CompletionCode = CommandResponseStatus.COMPLD;
            //command.Execute = true;
            //command.ExecuteTime = DateTime.Now;
            //return crm;

            string respons = string.Empty;
            try
            {
                SendLog("尝试连接OMC");
                ConnectEMS();
                SendLog("尝试登录");
                if (Login(m_loginStr))
                {
                    SendLog("登录成功");
                    command.Execute = true;
                    command.ExecuteTime = DateTime.Now;
                    m_telconn.Write(command.CommandText);
                    respons = m_telconn.WaitFor(COMMAND_TERMINAL_FLAG, m_telnetTimeout);
                    if (respons == "")
                        throw new TimeoutException("EMS未响应");
                }
            }
            catch { }
            finally
            {
                Logout(m_logoutStr);
            }
            return CommandResponseMsg.ConvertFromString(m_commBuilder, ConvertEncoding(respons));
        } 

        protected string ExecuteCommString(string command)
        {
            string respons = string.Empty;
            if (m_telconn == null)
                ConnectEMS();
            m_telconn.Write(command);
            respons = m_telconn.WaitFor(COMMAND_TERMINAL_FLAG, m_telnetTimeout);
            if (respons == "")
                throw new TimeoutException("EMS未响应");
            return ConvertEncoding(respons);
        }

        private string ConvertEncoding(string msg)
        {
            if (m_omcEncode == null)
                return msg;
            byte[] bytes = s_telnetEncode.GetBytes(msg);
            return m_omcEncode.GetString(bytes);
        }

        protected bool Login(string command)
        {
            bool bSuc = false;
            string msg = string.Empty;
            CommandResponseMsg crm = ExecuteComm(command);
            if (crm.CompletionCode == CommandResponseStatus.COMPLD)
                bSuc = true;
            else
            {
                msg = crm.ToString();
                SendLog(msg);
            }
            return bSuc;
        }

        private void Logout(string command)
        {
            try
            {
                if (m_telconn == null)
                {
                    SetOmcUnavailable();
                    return;
                }
                if (command != "")
                {
                    ExecuteComm(command);
                }
                m_telconn.Close();
                m_telconn = null;
            }
            catch (Exception ex)
            {
                try
                {

                    m_telconn.Close();
                    m_telconn = null;
                }
                catch { }
                throw new Exception("Logout发生异常", ex);
            }
        }
        #endregion

        #region 获取表名
        private string GetTaskTableName(Task task)
        {
            if (!task.IsRollbackHisTask)
                return TASK_TABLE;
            else
                return ROLLBACKHISTASK_TABLE;
        }
        #endregion

        #region 清空Olt告警
        /// <summary>
        /// 清空Olt告警
        /// </summary>
        /// <param name="task"></param>
        private void ClearOltAlarm(Task task)
        {
            SendLog(string.Format("ClearOltAlarm:{0}", task.TaskID));
            SqlParameter spTaskID = new SqlParameter("@ftthTaskID", task.TaskID);
            SQLUtil.ExecProc(m_Connstr, SP_CLEAR_OLTALARM, spTaskID);
        }
        #endregion

        #region 子类事件
        protected void WriteLog(string message)
        {
            SendLog(message);
        }

        protected void NetInterrupt(string message)
        {
            OnNetInterrupt(message);
        }

        protected bool AdapterRunning()
        {
            return IsAdapterRunning();
        }
        #endregion

    }
}
