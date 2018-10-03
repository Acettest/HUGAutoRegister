using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TK_AlarmManagement
{
	/// <summary>
	/// 命令处理器类
	/// </summary>
	public class CommandProcessor : TK_AlarmManagement.ICommandProcessor
    {
        #region 私有成员
        private ReaderWriterLock m_RWHandlers = new ReaderWriterLock();
        private Dictionary<Constants.TK_CommandType, List<ICommandHandler>> m_ReportHandlers = new Dictionary<Constants.TK_CommandType, List<ICommandHandler>>();

        // 一个handler侦听多种消息，只需要+=事件一次，这个变量辅助完成此操作
        private Hashtable m_CommandSender = new Hashtable();

        private Hashtable m_Dispatcher = new Hashtable();

        private ArrayList lstCommand;

		private object m_lockRun = new int();
		private long bRun = 0;

        #endregion
        
        #region Singleton Implementation
        private static object _locksingleton = new int();
		private static CommandProcessor _commandProcessor = null;
        /// <summary>
        /// 获取唯一CommandProcessor实例
        /// </summary>
        /// <returns></returns>
        public static CommandProcessor instance()
        {
            lock (_locksingleton)
            {
                if (_commandProcessor == null)
                    _commandProcessor = new CommandProcessor();
            }

            return _commandProcessor;
        }

		protected CommandProcessor()
		{
			lstCommand = new ArrayList();

            ThreadPool.SetMinThreads(4, 4);
            ThreadPool.SetMaxThreads(64, 32);
        }
        #endregion

        #region 序列号分配
        private static long m_SeqID = 0x0000000000000000;
        public static long AllocateID()
        {
            return Interlocked.Increment(ref m_SeqID);
        }
        #endregion

        #region 启动、停止 CommandProcessor
        public void Start(bool isServer)
		{
			lock(m_lockRun)
			{
                if (Interlocked.Read(ref bRun) == 1)
                    return;
                else
                {
                    if (isServer && Interlocked.Read(ref m_SeqID) == 0)
                        Interlocked.Add(ref m_SeqID, 0x4000000000000000);
                    Interlocked.Exchange(ref bRun, 1);
                }
			}
		}

		public void Stop()
		{
			lock (m_lockRun)
			{
                Interlocked.Exchange(ref bRun, 0);

                try
                {
                    m_RWHandlers.AcquireWriterLock(-1);

                    // stop all worker
                    foreach (object o in m_Dispatcher.Values)
                    {
                        CommandDispatcher dispatcher = o as CommandDispatcher;
                        dispatcher.EndWork();
                    }

                    m_Dispatcher.Clear();

                    m_ReportHandlers.Clear();

                    m_CommandSender.Clear();
                }
                finally
                {
                    m_RWHandlers.ReleaseWriterLock();
                }
            }
        }
        #endregion

        #region 侦听句柄的注册与去注册
        /// <summary>
        /// 注册一个ICommandHandler
        /// </summary>
        /// <param name="type">支持的命令类型</param>
        /// <param name="handler"></param>
        /// 
        public void registerReportHandler(Constants.TK_CommandType type, ICommandHandler handler, Dictionary<Constants.TK_CommandType, byte> supercommands)
		{
            try
            {

                m_RWHandlers.AcquireWriterLock(-1);

                if (!this.m_CommandSender.ContainsKey(handler))
                {
                    this.m_CommandSender.Add(handler, 1);
                }

                List<ICommandHandler> handlers;
                if (this.m_ReportHandlers.ContainsKey(type))
                    handlers = this.m_ReportHandlers[type];
                else
                    handlers = (this.m_ReportHandlers[type] = new List<ICommandHandler>());

                handlers.Add(handler);

                // 为每一个命令句柄构建一个派发实例
                // 一个句柄可能侦听多种命令类型，派发实例是唯一的，只是在RefCount上累加
                // 这样可以做到每个命令句柄在多线程环境下独立处理命令，互不影响
                if (!m_Dispatcher.ContainsKey(handler))
                {
                    CommandDispatcher dispatcher = new CommandDispatcher(handler, supercommands);
                    dispatcher.TypeCount = 1;
                    m_Dispatcher[handler] = dispatcher;

                    dispatcher.StartWork();
                }
                else
                {
                    CommandDispatcher dispatcher = (CommandDispatcher)m_Dispatcher[handler];
                    ++dispatcher.TypeCount;
                }
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }
            finally
            {
                m_RWHandlers.ReleaseWriterLock();
            }
		}

		public void unregisterReportHandler(Constants.TK_CommandType type, ICommandHandler handler)
        {
            try
            {
                m_RWHandlers.AcquireWriterLock(-1);

                if (!this.m_ReportHandlers.ContainsKey(type))
                    return; // 无此类型处理器注册，直接返回

                List<ICommandHandler> handlers = this.m_ReportHandlers[type];
                handlers.Remove(handler);

                // 在注销派发实例时，必须在RefCount为零的情况下，才可以彻底删除
                if (m_Dispatcher.ContainsKey(handler))
                {
                    CommandDispatcher dispatcher = (CommandDispatcher)m_Dispatcher[handler];
                    --dispatcher.TypeCount;

                    if (dispatcher.TypeCount == 0)
                    {
                        dispatcher.EndWork();
                        m_Dispatcher.Remove(handler);

                        m_CommandSender.Remove(handler);
                    }
                }
            }
            catch
			{
			}
			finally
			{
                m_RWHandlers.ReleaseWriterLock();
			}
        }
        #endregion

        #region 派发命令接口
        public void DispatchCommands(List<ICommunicationMessage> msgs)
        {
            try
            {
                if (Interlocked.Read(ref bRun) == 0)
                    return;

                Dictionary<CommandDispatcher, List<ICommunicationMessage>> queue = new Dictionary<CommandDispatcher, List<ICommunicationMessage>>();
                foreach (ICommunicationMessage cm in msgs)
                {
                    if (cm == null)
                        continue;

                    try
                    {
                        m_RWHandlers.AcquireReaderLock(-1);

                        if (!m_ReportHandlers.ContainsKey(cm.TK_CommandType))
                            continue;

                        List<ICommandHandler> handlers = this.m_ReportHandlers[cm.TK_CommandType];
                        foreach (object h in handlers)
                        {
                            ICommandHandler handler = (ICommandHandler)h;

                            // 使用派发实例来派发各种命令
                            if (m_Dispatcher.ContainsKey(handler))
                            {
                                CommandDispatcher dispatcher = (CommandDispatcher)m_Dispatcher[handler];

                                if (queue.ContainsKey(dispatcher))
                                    queue[dispatcher].Add(cm);
                                else
                                {
                                    queue[dispatcher] = new List<ICommunicationMessage>();
                                    queue[dispatcher].Add(cm);
                                }
                            }
                        }
                    }
                    finally
                    {
                        m_RWHandlers.ReleaseReaderLock();
                    }
                } // end for msgs

                // 批量派发
                foreach (KeyValuePair<CommandDispatcher, List<ICommunicationMessage>> pair in queue)
                    pair.Key.EnqueueMsgs(pair.Value);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }
        }

        public void DispatchCommand(ICommunicationMessage cm)
		{
            try
            {
                m_RWHandlers.AcquireReaderLock(-1);

                if (Interlocked.Read(ref bRun) == 0)
                    return;

                if (cm == null)
                    return;

                if (!m_ReportHandlers.ContainsKey(cm.TK_CommandType))
                    return;

                List<ICommandHandler> handlers = m_ReportHandlers[cm.TK_CommandType];
                foreach (object h in handlers)
                {
                    ICommandHandler handler = (ICommandHandler)h;

                    // 使用派发实例来派发各种命令
                    if (m_Dispatcher.ContainsKey(handler))
                    {
                        CommandDispatcher dispatcher = (CommandDispatcher)m_Dispatcher[handler];
                        dispatcher.EnqueueMsg(cm);
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }
            finally
            {
                m_RWHandlers.ReleaseReaderLock();
            }
        }
        #endregion

    }

    #region CommandDispatcher
    public class CommandDispatcher
    {
        private ICommandHandler m_Handler = null;
        private List<ICommunicationMessage> m_MsgQueue = new List<ICommunicationMessage>();
        private Dictionary<Constants.TK_CommandType, byte> m_SuperCommands = new Dictionary<Constants.TK_CommandType, byte>();
        private Thread m_Worker = null;
        private long m_Stop = 0;
        private AutoResetEvent m_SignalNewCommand = new AutoResetEvent(false);
        private ManualResetEvent m_ClearEvent = new ManualResetEvent(true);

        public int TypeCount = 0;
        public CommandDispatcher(ICommandHandler handler, Dictionary<Constants.TK_CommandType, byte> super_commands)
        {
            m_Handler = handler;

            m_Worker = new Thread(new ThreadStart(worker));
            m_Worker.Name = handler.GetType().Name;

            if (super_commands != null)
            {
                foreach (KeyValuePair<Constants.TK_CommandType, byte> de in super_commands)
                    m_SuperCommands.Add(de.Key, de.Value);
            }
        }

        public void EnqueueMsg(ICommunicationMessage cm)
        {
            lock (m_MsgQueue)
                m_MsgQueue.Add(cm);

            m_SignalNewCommand.Set();
        }

        public void EnqueueMsgs(List<ICommunicationMessage> msgs)
        {
            lock (m_MsgQueue)
                m_MsgQueue.AddRange(msgs);

            m_SignalNewCommand.Set();
        }

        public void StartWork()
        {
            Interlocked.Exchange(ref m_Stop, 0);

            m_Worker.Start();
        }

        public void EndWork()
        {
            Interlocked.Exchange(ref m_Stop, 1);

            m_SignalNewCommand.Set();

            Thread.Sleep(10);
            m_ClearEvent.WaitOne();
        }

        private void worker()
        {
            m_ClearEvent.Reset();

            while (Interlocked.Read(ref m_Stop) == 0)
            {
                m_SignalNewCommand.WaitOne(); // 等待命令到达

                try
                {
                    if (Interlocked.Read(ref m_Stop) == 1)
                        break;

                    ICommunicationMessage[] temp = null;
                    lock (m_MsgQueue)
                    {
                        if (m_MsgQueue.Count > 0)
                        {
                            temp = new ICommunicationMessage[m_MsgQueue.Count];
                            m_MsgQueue.CopyTo(temp);
                            m_MsgQueue.Clear();
                        }
                        else
                            continue;
                    }

                    // 先处理优先命令
                    List<ICommunicationMessage> secondcmds = new List<ICommunicationMessage>();
                    foreach (ICommunicationMessage cm in temp)
                    {
                        if (m_SuperCommands.ContainsKey(cm.TK_CommandType))
                            m_Handler.handleCommand(cm);
                        else
                            secondcmds.Add(cm);
                    }

                    int i = 0;
                    foreach (ICommunicationMessage cm in secondcmds)
                    {
                        m_Handler.handleCommand(cm);

                        if (++i % 10 == 0 && Interlocked.Read(ref m_Stop) == 1)
                            break;
                    
                    }
                }
                catch
                {
                }
                finally
                {
                    System.Threading.Thread.Sleep(10);
                }
            }

            m_ClearEvent.Set();
        }
    }
    #endregion
}
