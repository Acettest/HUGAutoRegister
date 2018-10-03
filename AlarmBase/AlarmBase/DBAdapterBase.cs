using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace TK_AlarmManagement
{
    /// <summary>
    /// 基于数据库的告警采集适配器，相比AdapterBase，增加了OMC数据库操作模板过程，以及强制清除定时器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DBAdapterBase<T> : AdapterBase<T>
        where T : ISourceAlarm, new()
    {
        #region 保护成员
        protected List<AlarmTimeStamp<T>> m_PendingClearForcely;

        protected System.Timers.Timer m_TimerClearForcely;

        #endregion



        virtual protected void _Retriever(object para)
        {
        }

        #region 私有成员
        /// <summary>
        /// 是否正在运行强制清除定时线程
        /// </summary>
        private long m_InTimer = 0;
        #endregion

        #region 构造函数
        /// <summary>
        /// 数据库采集器构造函数
        /// </summary>
        /// <param name="name">采集器的名称，用于日志显示</param>
        /// <param name="interval">采集轮询周期，单位毫秒</param>
        public DBAdapterBase(string name, int interval) : base(name, interval)
        {
        }

        public DBAdapterBase() : base()
        {
        }

        protected override void InitMember()
        {
            base.InitMember();

            m_PendingClearForcely = new List<AlarmTimeStamp<T>>();

            m_TimerClearForcely = new System.Timers.Timer(30000);
            m_TimerClearForcely.Elapsed += new System.Timers.ElapsedEventHandler(m_TimerClearForcely_Elapsed);
        }
        #endregion

        #region 强制清除定时器-已含告警生成
        void m_TimerClearForcely_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref m_InTimer, 1) == 1)
                return;

            if (PendingRunFlag == 0)
                return;

            try
            {
                m_StopPrivilege.AcquireReaderLock(-1);

                lock (m_PendingClearForcely)
                    if (m_PendingClearForcely.Count == 0)
                        return;

                _clearForcely();

                ClearNMAlarm(Name, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
            }
            catch (Exception ex)
            {
                RaiseNMAlarm(Name, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, ex.Message, DateTime.Now);
                SendLog(ex.ToString());
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();
                Interlocked.Exchange(ref m_InTimer, 0);
            }
        }
        #endregion

        #region 入强制清除队列-不必生成告警
        protected void enqueuePendingClearForcely(T alarm)
        {
            AlarmTimeStamp<T> stamp = new AlarmTimeStamp<T>();
            stamp.Alarm = alarm;
            stamp.TimeStamp = System.DateTime.Now;
            stamp.NeedRemove = false;

            lock (m_PendingClearForcely)
                m_PendingClearForcely.Add(stamp);
        }
        #endregion

        #region 告警一致性检查的基本实现，派生类如有特殊要求，应重写此方法-已含告警生成
        virtual protected void _checkActiveConsistency(string omcName, string omcConn)
        {
            // A - B
            string q = "select * from " + ActiveTable + " where OMCName='" + omcName + "'";
            q += " and TKSn not in (select TKSn from " + UniteActiveTable + " where OMCName='" + omcName + "')"; // OMCName也是全局唯一，不追加使用Manufacturer

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
                T sourcealarm = new T();
                sourcealarm.ActiveTable = ActiveTable;
                sourcealarm.ResumeTable = ResumeTable;
                sourcealarm.BuildFromDB(r);

                TKAlarm tkalarm = ConvertToTKAlarm(sourcealarm);
                pending.Add(tkalarm);

                if (PendingRunFlag == 0)
                    return;
            }

            // (B-A)
            q = "select * from " + UniteActiveTable + " where OMCName='" + omcName + "'";
            q += " and TKSn not in (select TKSn from " + ActiveTable + " where OMCName='" + omcName + "')";

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

                q = "select * from " + ResumeTable + " where tksn=" + tksn;

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
                        T sourcealarm_r = new T();
                        sourcealarm_r.ActiveTable = ActiveTable;
                        sourcealarm_r.ResumeTable = ResumeTable;
                        sourcealarm_r.BuildFromDB(temp.Tables["temp"].Rows[0]);

                        TKAlarm alarm = ConvertToTKAlarm(sourcealarm_r);
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

        virtual protected void _checkResumeConsistency(string omcName, string omcConn)
        {
            // A - B
            string q = "select * from " + ResumeTable + " where OMCName='" + omcName + "'";
            q += " and TKSn not in (select TKSn from " + UniteResumeTable + " where OMCName='" + omcName + "')";
            q += " and TKSn not in (select TKSn from " + UniteGarbageTable + " where OMCName='" + omcName + "')";

            DataSet ds = new DataSet();
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

            List<TKAlarm> pending = new List<TKAlarm>();
            foreach (DataRow r in ds.Tables["temp"].Rows)
            {
                T sourcealarm_r = new T();
                sourcealarm_r.BuildFromDB(r);

                TKAlarm tkalarm = ConvertToTKAlarm(sourcealarm_r);
                tkalarm.ClearTime = "";
                TKAlarm tkalarm_r = ConvertToTKAlarm(sourcealarm_r);
                pending.Add(tkalarm);
                pending.Add(tkalarm_r);
            }

            #region (B-A)的操作暂时不做
            /*q = "select * from resumealarm where manufacturer='" + Manufacturer + "' and OMCName='" + omcName + "'";
            q += " and TKSn not in (select TKSn from " + ResumeTable + " where OMCName='" + omcName + "')";

            ds.Tables.Clear();
            try
            {
                SqlHelper.FillDataset(Connstr, CommandType.Text, q, ds, new string[] { "temp" });
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }

            foreach (DataRow r in ds.Tables["temp"].Rows)
            {
                T sourcealarm_r = new T();
                sourcealarm_r.ActiveTable = ActiveTable;
                sourcealarm_r.ResumeTable = ResumeTable;
                sourcealarm_r.ConvertToAlarm(r, omcName);

                AlarmTimeStamp<T> stamp = new AlarmTimeStamp<T>();
                stamp.Alarm = sourcealarm_r;
                stamp.TimeStamp = System.DateTime.Now;
                stamp.OMCName = omcName;
                stamp.OMCConn = omcConn;

                lock (PendingClearForcely)
                    PendingClearForcely.Add(stamp);
            }*/
            #endregion

            SendAlarms(pending);
        }
        #endregion

        #region 轮询告警函数，被采集线程调用-已含告警生成
        virtual protected void _OMCDBRetriever(object para)
        {
            string omcName = para as string;
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

                try
                {
                    SendStateChange(omcName, "运行");
                }
                catch { }

                SendLog("正在检查:" + Name + "的" + omcName + "的告警数据一致性...");

                try
                {
                    /// 检查活动告警的一致性
                    _checkActiveConsistency(omcName, omcConn);
                    if (dog != null) dog.Feed();

                    /// 检查TK网管活动告警的一致性
                    _checkNMActiveConsistency(omcName, omcConn);

                    /// 检查历史告警的一致性
                    _checkResumeConsistency(omcName, omcConn);
                    if (dog != null) dog.Feed();

                    /// 清除所有网管告警
                    ClearNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
                }
                catch (Exception ex)
                {
                    RaiseNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, ex.Message, DateTime.Now);
                    SendLog(ex.ToString());
                }
                finally
                {
                    SendLog("检查完毕:" + Name + "的" + omcName + "的告警数据一致性.");
                }

                while (PendingRunFlag == 1)
                {
                    try
                    {
                        //AdapterStateReport(AdapterStatus.Working, null);

                        SendLog("开始获取" + omcName + "的新增告警");
                        _getNewActiveAlarms(omcName, omcConn);
                        if (dog != null) dog.Feed();

                        if (PendingRunFlag == 0)
                        {
                            ClearNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
                            break;
                        }
                        else
                            Thread.Sleep(0);

                        SendLog("开始检查" + omcName + "的活动告警状态");
                        _clearStalledActiveAlarms(omcName, omcConn);
                        if (dog != null) dog.Feed();

                        if (PendingRunFlag == 0)
                        {
                            ClearNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
                            break;
                        }
                        else
                            Thread.Sleep(0);

                        SendLog("开始获取" + omcName + "的瞬断告警");
                        _fetchTransientAlarms(omcName, omcConn);
                        if (dog != null) dog.Feed();

                        if (PendingRunFlag == 0)
                        {
                            ClearNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, DateTime.Now);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        RaiseNMAlarm(omcName, Constants.MO_PROCESS, Constants.TKALM_PROCESSALM, ex.Message, DateTime.Now);
                        SendLog(ex.ToString());
                    }

                    //AdapterStateReport(AdapterStatus.Running, null);

                    int i = m_Interval % 1000;
                    for (; i < m_Interval; i += 1000)
                    {
                        if (Interlocked.Read(ref m_PendingRun) == 0)
                            break;
                        Thread.Sleep(1000);

                        try
                        {
                            if (i % 60 == 0)
                                if (dog != null) dog.Feed();
                        }
                        catch { }
                    }
                }
            }
            finally
            {
                m_StopPrivilege.ReleaseReaderLock();

                try
                {
                    SendStateChange(omcName, "停止");
                }
                catch { }
            }
        }
        #endregion

        #region 抽象接口，需要派生类实现
        /// <summary>
        /// 用于实现者填写各个告警业务实际处理动作
        /// </summary>
        abstract protected void _getNewActiveAlarms(string omcName, string omcConn);
        abstract protected void _clearStalledActiveAlarms(string omcName, string omcConn);
        abstract protected void _fetchTransientAlarms(string omcName, string omcConn);
        abstract protected void _clearForcely();

        abstract protected TKAlarm ConvertToTKAlarm(T source_alarm);
        #endregion

        #region IAlarmAdapter 成员-不必生成告警
        public override void Start()
        {
            lock (this)
            {
                base.Start();

                lock (m_OMCAddresses)
                {
                    foreach (string o in m_OMCAddresses.Keys)
                    {
                        //_OMCDBRetriever在HGUAsynAdapter中被重写
                        Thread thr = new Thread(new ParameterizedThreadStart(_OMCDBRetriever));//获取任务并执行核心代码
                        thr.Start(o as string);

                        SendLog(o as string + "采集服务已经启动.");
                    }
                }

                m_TimerClearForcely.Start();
            }
        }

        public override void Stop()
        {
            lock (this)
            {
                m_TimerClearForcely.Stop();

                base.Stop();
            }
        }

        #endregion      

    }
}
