#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: YWTPBossAsynAdapter
// 作    者：d.w
// 创建时间：2014/7/4 14:48:11
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
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.Collections;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using DefLib;
using TK_AlarmManagement;
using HGU.Idl;

namespace RemotableHGU_PBoss_Adapter
{
   public class HGUPBossAsynAdapter : DBAdapterBase<HUGAlarm>, ICommandHandler, ICommandRegister
    {
        private const int RESPONSE_THREADS = 10;
        private const int RESPONSE_DELAY = 15;                      //秒 kboss回单间隔时间

        #region IFields
        private Queue<ReplyBossMsgExtend> m_ReplyBossMsgs;           //普通工单
        //private Queue<ReplyBossMsg> m_rollbackTaskReplyBossMsgs;   //回滚历史工单的回复
        //private Queue<ReplyBossMsg> m_netInterruptReplyBossMsgs;   //网络异常的工单回复
        private KBossWebWrapper[] m_webWrappers = new KBossWebWrapper[RESPONSE_THREADS];
        private WaitHandle[] m_idleHandles = new WaitHandle[RESPONSE_THREADS];

        string omcName = "";
        ArrayList m_Files = new ArrayList();

        bool m_InTimer = false;
        object m_LockTimer = new object();
        System.Timers.Timer m_Timer_ClearLogger = new System.Timers.Timer(); //每隔30天清理日志
        #endregion

        #region IConstructors
        public HGUPBossAsynAdapter()
        {
            m_Timer_ClearLogger.Interval = 1000 * 60 * 60 * 3; //3小时检查一次
            m_Timer_ClearLogger.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_ClearLogger_Elapsed);
        }

        public HGUPBossAsynAdapter(string name, int interval)
            : this()
        {
            m_Name = name;
            m_Interval = interval;
        }
        #endregion

        #region IMethods
        #region 初始化
        protected void Init()
        {
            m_ReplyBossMsgs = new Queue<ReplyBossMsgExtend>();

            for (int i = 0; i < RESPONSE_THREADS; i++)
            {
                m_idleHandles[i] = new AutoResetEvent(false);
                m_webWrappers[i] = new KBossWebWrapper((EventWaitHandle)m_idleHandles[i], m_Connstr);
                m_webWrappers[i].SendLog += SendLog;
                m_webWrappers[i].EnqueueReply += EnqueueReply;
                m_webWrappers[i].IsAdapterRunning += IsAdapterRunning;
            }
        }

        protected override void ReadConfig()
        {
            base.ReadConfig();
            Init();
        }
        #endregion

        #region 重载基类方法
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
                WatchDog.IWatchDog dog = WatchDog.WatchDogFactory.Instance().GetWatchDog(Name, omcName);

                while (PendingRunFlag == 1)
                {
                    try
                    {
                        DoWork(dog);
                        if (dog != null) dog.Feed();
                        if (PendingRunFlag == 0)
                        {
                            ClearNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
                            break;
                        }
                        else
                            Thread.Sleep(0);
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

        public override void Start()
        {            
            base.Start();
            try
            {
                m_Timer_ClearLogger.Start();
                //
                for (int i = 0; i < RESPONSE_THREADS; i++)
                {
                    new Thread(new ThreadStart(m_webWrappers[i].Retriever)).Start();
                }
                Thread thread = new Thread(new ThreadStart(DispatchRetriever));
                thread.Start();
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
        }

        private void DispatchRetriever()
        {
            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);

                while (PendingRunFlag == 1)
                {
                    try
                    {
                        DispatchReply();
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

        protected override TKAlarm ConvertToTKAlarm(HUGAlarm source_alarm)
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
        #endregion

        #region
        private void DoWork(WatchDog.IWatchDog dog)
        {
            GetReplyBossMsg("vTaskReplyBossMsg", ReplyType.NormalTask);
            GetReplyBossMsg("vRollbackTaskReplyBossMsg", ReplyType.RollbackTask);
            GetReplyBossMsg("vNetDelayReplyBossMsg", ReplyType.NetInterrupt);
            if (dog != null) dog.Feed();
        }

        private void GetReplyBossMsg(string view,ReplyType type)
        {
            DataTable dt = SQLUtil.GetReplyBossMsg(m_Connstr, view);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    ReplyBossMsg reply = IdlUtil.ParseReplyFromDataRow(dr);
                    ReplyBossMsgExtend replyExtend = new ReplyBossMsgExtend(reply, type);
                    if (!m_ReplyBossMsgs.Contains(replyExtend)&&!CheckAlreadyWork(replyExtend))
                    lock (m_ReplyBossMsgs)
                    {
                        m_ReplyBossMsgs.Enqueue(replyExtend);
                    }
                }
            }
        }

        public bool CheckAlreadyWork(ReplyBossMsgExtend replyExtend)
        {
            bool bAlready = false;
            for (int i = 0; i < RESPONSE_THREADS; i++)
            {
                if (replyExtend.Equals(m_webWrappers[i].CurrentReply))
                {
                    bAlready = true;
                    break;
                }
            }
            return bAlready;
        }

        private void DispatchReply()
        {
            ReplyBossMsgExtend currentReply;
            //debug
            bool b = false;
            DateTime dt1 = DateTime.Now;
            if (m_ReplyBossMsgs.Count > 0)
            {
                b = true;
                SendLog("count " + m_ReplyBossMsgs.Count);
                SendLog(dt1.ToString());
            }
            while (m_ReplyBossMsgs.Count > 0 &&
                Interlocked.Read(ref m_PendingRun) == 1)
            {
                int index = WaitHandle.WaitAny(m_idleHandles);
                lock (m_ReplyBossMsgs)
                {
                    currentReply = m_ReplyBossMsgs.Dequeue();
                }
                m_webWrappers[index].SetReply(currentReply);
                Thread.Sleep(1);
                SendLog("thread :" + index.ToString() + " Task " + currentReply.ReplyBoss.TaskID);                
            }
            //debug
            if (b)
            {
                SendLog(DateTime.Now.ToString());
                SendLog("totalSeconds: "+(DateTime.Now - dt1).TotalSeconds.ToString());
            }
        }

        private void EnqueueReply(ReplyBossMsgExtend reply)
        {
            lock (m_ReplyBossMsgs)
            {
                m_ReplyBossMsgs.Enqueue(reply);
            }
        }

        private bool IsAdapterRunning()
        {
            if (Interlocked.Read(ref m_PendingRun) == 0L)
                return false;
            return true;
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
