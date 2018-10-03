using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DefLib.Util
{
    public class ThreadPool
    {
        #region Singleton Implementation
        protected static object m_LockSingleton = new object();
        protected static ThreadPool m_ThreadPool = new ThreadPool();
        protected ThreadPool()
        {
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
        List<Thread> m_Pool = new List<Thread>();
        int m_PoolSize = 0;

        public ItemStatus QueueWorkItem(WaitCallback item, object param)
        {
            if (m_PendingStop)
                throw new Exception("ThreadPool is about to stop.");

            // 检查线程池
            lock (m_Pool)
            {
                if (m_Pool.Count < m_PoolSize)
                {
                    Thread th = new Thread(new ThreadStart(_Worker));
                    th.Start();

                    lock (m_Pool)
                        m_Pool.Add(th);
                }
            }

            PoolQueueItem queueitem = new PoolQueueItem();
            queueitem.Item = item;
            queueitem.Parameter = param;
            queueitem.Status = new ItemStatus();
            queueitem.Status.Status = ThreadState.Unstarted;
            lock (m_ItemQueue)
            {
                m_ItemQueue.Enqueue(queueitem);

                m_NewItemSignal.Set();
            }

            return queueitem.Status;
        }

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
                th.Start();

                lock (m_Pool)
                    m_Pool.Add(th);
            }

            m_PoolSize = poolsize;
            return true;
        }

        public bool Stop()
        {
            if (m_Stopped)
                return true;

            m_PendingStop = true;

            lock (m_Pool)
            {
                for (int i = 0; i < m_Pool.Count; ++i)
                {
                    m_NewItemSignal.Set();
                    Thread.Sleep(10);
                }

                for (int i = 0; i < m_Pool.Count; ++i)
                {
                    bool r = m_Pool[i].Join(30000);
                    if (!r)
                    {
                        m_Pool[i].Abort();
                        Logger.Instance().SendLog("ThreadPool", "等待工作线程终止超时, 强制Abort");
                    }
                }

                for (int i = 0; i < m_Pool.Count; ++i)
                {
                    m_Pool[i].Join();
                }

                m_Pool.Clear();
            }

            m_Stopped = true;
            return true;
        }

        void _Worker()
        {
            try
            {
                while (!m_PendingStop)
                {
                    m_NewItemSignal.WaitOne();

                    PoolQueueItem item = null;
                    lock (m_ItemQueue)
                    {
                        if (m_ItemQueue.Count > 0)
                            item = m_ItemQueue.Dequeue();

                        // 如果队列中还有其它工作项，则再触发之
                        if (m_ItemQueue.Count > 0)
                            m_NewItemSignal.Set();
                    }

                    if (m_PendingStop)
                        break;

                    if (item != null)
                    {
                        item.Status.Status = ThreadState.Running;

                        try
                        {
                            item.Item(item.Parameter);
                            item.Status.Status = ThreadState.Stopped;
                        }
                        catch (ThreadAbortException ex)
                        {
                            item.Status.Status = ThreadState.Aborted;
							Util.Logger.Instance().SendLog("ThreadPool", "Somebody try to abort current thread.");
                        }
                        catch (Exception ex)
                        {
                            Util.Logger.Instance().SendLog("ThreadPool", "Unhandled exception occurred: " + Environment.NewLine + ex.ToString());
                            item.Status.Status = ThreadState.Stopped;
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
                Util.Logger.Instance().SendLog("ThreadPool", string.Format("Thread[{0}] ended.", Thread.CurrentThread.ManagedThreadId));
            }
        }

        public class ItemStatus
        {
            public ThreadState Status;
        }
    }


    internal class PoolQueueItem
    {
        public WaitCallback Item;
        public object Parameter;
        public ThreadPool.ItemStatus Status;
    }
}
