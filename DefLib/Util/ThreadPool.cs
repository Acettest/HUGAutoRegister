using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DefLib.Util
{
    public class ThreadInfo
    {
        public Thread _Thr = null;
        public int _ThrID = -1;
        public Stopwatch _TimeMeter = new Stopwatch();
        public int _Timeout = Timeout.Infinite; // in s

        public ThreadInfo(Thread th, int timeout)
        {
            _Thr = th;
            _ThrID = th.ManagedThreadId;
            _Timeout = timeout;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class ThreadPool
    {
        #region Singleton Implementation
        protected static object m_LockSingleton = new object();
        protected static ThreadPool m_ThreadPool = new ThreadPool();
        protected ThreadPool()
        {
            m_CheckTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_CheckTimer_Elapsed);
        }

        public static ThreadPool Instance()
        {
            lock (m_LockSingleton)
            {
                if (m_ThreadPool == null)
                    m_ThreadPool = new ThreadPool();

                return m_ThreadPool;
            }
        }
        #endregion

        AutoResetEvent m_NewItemSignal = new AutoResetEvent(false);
        bool m_PendingStop = false;
        bool m_Stopped = true;
        Queue<PoolQueueItem> m_ItemQueue = new Queue<PoolQueueItem>();
        List<ThreadInfo> m_Pool = new List<ThreadInfo>();
        int m_PoolSize = 0;
        System.Timers.Timer m_CheckTimer = new System.Timers.Timer(1000);

        public int WorkerCount
        {
            get { lock (m_Pool) return m_Pool.Count; }
        }

        public int ItemCount
        {
            get
            {
                lock (m_ItemQueue) return m_ItemQueue.Count;
            }
        }

        /// <summary>
        /// 向线程池中提交任务，将放入线程池调度队列中
        /// </summary>
        /// <param name="item">任务函数的回调封装</param>
        /// <param name="param">需传递给任务的参数</param>
        /// <param name="timeout">任务的超时设置，单位为秒，超过该时间，任务将强行终止</param>
        /// <returns></returns>
        public ItemStatus QueueWorkItem(WaitCallback item, object param, int timeout)
        {

            if (m_PendingStop)
                throw new Exception("ThreadPool is about to stop.");

            // 
            lock (m_Pool)
            {
                if (m_Pool.Count < m_PoolSize)
                {
                    Thread th = new Thread(new ThreadStart(_Worker));
                    m_Pool.Add(new ThreadInfo(th, timeout));

                    th.Start();
                }
            }

            PoolQueueItem queueitem = new PoolQueueItem();
            queueitem.Item = item;
            queueitem.Parameter = param;
            queueitem.Status = new ItemStatus();
            queueitem.Status.Status = System.Threading.ThreadState.Unstarted;
            queueitem.Timeout = timeout;
            lock (m_ItemQueue)
            {
                m_ItemQueue.Enqueue(queueitem);

                m_NewItemSignal.Set();
            }

            return queueitem.Status;
        }

        /// <summary>
        /// 启动线程池
        /// </summary>
        /// <param name="poolsize">线程池可以并发执行的任务数，即线程池内部的线程数量</param>
        /// <returns></returns>
        public bool Start(int poolsize)
        {
            if (!m_Stopped)
                return false;

            m_PendingStop = false;
            m_Stopped = false;
            m_Pool.Clear();

            for (int i = 0; i < poolsize; ++i)
            {
                Thread th = new Thread(new ThreadStart(_Worker));
                lock (m_Pool)
                    m_Pool.Add(new ThreadInfo(th, -1));

                th.Start();
            }

            m_PoolSize = poolsize;

            m_CheckTimer.Start();
            return true;
        }

        public bool Stop()
        {
            if (m_Stopped)
                return true;

            m_PendingStop = true;
            m_CheckTimer.Stop();

            lock (m_Pool)
            {
                for (int i = 0; i < m_Pool.Count; ++i)
                {
                    m_NewItemSignal.Set();
                    Thread.Sleep(10);
                }

                for (int i = 0; i < m_Pool.Count; ++i)
                {
                    try
                    {
                        bool r = m_Pool[i]._Thr.Join(30000);
                        if (!r)
                        {
                            m_Pool[i]._Thr.Abort();
                            Logger.Instance().SendLog("ThreadPool", ", Abort");
                        }
                    }
                    catch { }
                }

                for (int i = 0; i < m_Pool.Count; ++i)
                {
                    m_Pool[i]._Thr.Join();
                }

                m_Pool.Clear();
            }

            m_Stopped = true;
            return true;
        }

        void _Worker()
        {
            int current_thr_id = Thread.CurrentThread.ManagedThreadId;

            try
            {
                while (!m_PendingStop)
                {
                    m_NewItemSignal.WaitOne();

                    // 从任务队列中取出等待的任务，放入本线程执行
                    PoolQueueItem item = null;
                    lock (m_ItemQueue)
                    {
                        if (m_ItemQueue.Count > 0)
                            item = m_ItemQueue.Dequeue();

                        // 
                        if (m_ItemQueue.Count > 0)
                            m_NewItemSignal.Set();
                    }

                    if (m_PendingStop)
                        break;

                    if (item != null)
                    {
                        // 查找线程池中本线程的相关配置信息
                        ThreadInfo info = null;
                        lock (m_Pool)
                        {
                            info = m_Pool.Find(
                                delegate(ThreadInfo t)
                                {
                                    return t._ThrID == current_thr_id;
                                });
                        }

                        if (info != null)
                        {
                            info._Timeout = item.Timeout;
                            info._TimeMeter.Reset();
                        }
                        else
                            throw new Exception("Cannot find thread info.");

                        item.Status.Status = System.Threading.ThreadState.Running;

                        try
                        {
                            // 开始计时，执行任务
                            info._TimeMeter.Start();
                            item.Item(item.Parameter);
                            item.Status.Status = System.Threading.ThreadState.Stopped;
                            info._TimeMeter.Stop();

                            Util.Logger.Instance().SendLog("ThreadPool", string.Format("Thread[{0}] finish job normally.{1}", Thread.CurrentThread.ManagedThreadId, Environment.NewLine));
                        }
                        catch (ThreadAbortException)
                        {
                            item.Status.Status = System.Threading.ThreadState.Aborted;

                            Util.Logger.Instance().SendLog("ThreadPool", string.Format("Someone abort the work thread[{0}].{1}", Thread.CurrentThread.ManagedThreadId, Environment.NewLine));

                            // 当调用 Abort 以终止线程时，系统将引发 ThreadAbortException。
                            // ThreadAbortException 是一个可由应用程序代码捕获的特殊异常，但在 catch 块的结尾将被再次引发，
                            // 除非调用 ResetAbort
                            // 如此之后，本线程仍可继续使用
                            Thread.ResetAbort();
                        }
                        catch (Exception ex)
                        {
                            Util.Logger.Instance().SendLog("ThreadPool", string.Format("Unhandled exception occurred[{0}]:{1}{2}", Thread.CurrentThread.ManagedThreadId, Environment.NewLine, ex.ToString()));
                            item.Status.Status = System.Threading.ThreadState.Stopped;
                        }
                        finally
                        {
                            lock (m_Pool)
                            {
                                if (info != null)
                                {
                                    info._Timeout = Timeout.Infinite;
                                    info._TimeMeter.Reset();
                                }
                            }
                        }
                    }
                } // end while
            }
            catch (ThreadAbortException ex)
            {
                #region 此部分已经不会触发
                try
                {
                    // 收到线程强制退出异常，处理线程池，删除相关信息
                    Util.Logger.Instance().SendLog("ThreadPool", string.Format("Someone abort the work thread[{0}].{1}", Thread.CurrentThread.ManagedThreadId, Environment.NewLine));

                    lock (m_Pool)
                    {
                        ThreadInfo abort = m_Pool.Find(
                        delegate(ThreadInfo t)
                        {
                            return t._ThrID == current_thr_id;
                        });

                        if (!m_Pool.Remove(abort))
                            Util.Logger.Instance().SendLog("ThreadPool", string.Format("Thread {0} removation failed.", abort._ThrID));

                        // 重新加入一个活动线程
                        Thread th = new Thread(new ThreadStart(_Worker));
                        m_Pool.Add(new ThreadInfo(th, -1));

                        th.Start();

                        // 当调用 Abort 以终止线程时，系统将引发 ThreadAbortException。
                        // ThreadAbortException 是一个可由应用程序代码捕获的特殊异常，但在 catch 块的结尾将被再次引发，
                        // 除非调用 ResetAbort
                        Thread.ResetAbort();
                    }
                }
                catch (Exception e)
                {
                    Util.Logger.Instance().SendLog("ThreadPool", string.Format("Unhandled exception occurred[{0}] when dealing abortion:{1}{2}", Thread.CurrentThread.ManagedThreadId, Environment.NewLine, e.ToString()));
                }
                finally
                {
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.Logger.Instance().SendLog("ThreadPool", string.Format("Unhandled exception occurred[{0}]:{1}{2}", Thread.CurrentThread.ManagedThreadId, Environment.NewLine, ex.ToString()));
            }
            finally
            {
                Util.Logger.Instance().SendLog("ThreadPool", string.Format("Thread[{0}] ended.", current_thr_id));
            }
        }

        object m_LockTimer = new object();
        bool m_bInCheckTimer = false;
        void m_CheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_LockTimer)
            {
                if (m_bInCheckTimer)
                    return;
                else
                    m_bInCheckTimer = true;
            }

            try
            {
                if (m_PendingStop)
                    return;

                List<ThreadInfo> pool = new List<ThreadInfo>();
                lock (m_Pool)
                {
                    pool.AddRange(m_Pool);
                }

                foreach (ThreadInfo info in pool)
                {
                    // 检查每个线程的超时设置，如果超时，则强行终止
                    if (info._Timeout != Timeout.Infinite)
                    {
                        if (info._TimeMeter.ElapsedMilliseconds / 1000 > info._Timeout)
                        {
                            // abort the work thread
                            info._Thr.Abort();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logger.Instance().SendLog("ThreadPool", ex.ToString());
            }
            finally
            {
                lock (m_LockTimer)
                    m_bInCheckTimer = false;
            }
        }

        public class ItemStatus
        {
            public System.Threading.ThreadState Status;
        }
    }


    internal class PoolQueueItem
    {
        public WaitCallback Item;
        public object Parameter;
        public ThreadPool.ItemStatus Status;
        public int Timeout;
    }
}
