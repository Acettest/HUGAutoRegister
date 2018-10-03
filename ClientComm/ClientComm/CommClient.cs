using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace TK_AlarmManagement
{
    public class CommClient<MSGTYPE> : ICommClient, ICommandHandler
        where MSGTYPE : ICommunicationMessage, new()
    {
        #region 私有成员定义
        private Socket m_Client = null;

        private long m_ClientID = 0;

        private ICommunicator m_Comm = null;

        private IMessageExtractor m_Extractor = null;
        private IMessageEncoder m_Encoder = null;

        private int m_SendTimeOut = 30; // 秒为单位

        private bool m_bCompress = false;

        private long m_Started = 0;

        private Dictionary<long, CommonPair<ICommunicationMessage, ManualResetEvent>> m_MessagesWaitForResponse = new Dictionary<long, CommonPair<ICommunicationMessage, ManualResetEvent>>();

        /// <summary>
        /// 保活定时器
        /// </summary>
        /// 
        private bool m_bKeepAlive = false;
        private System.Timers.Timer m_TimerKeepAlive = new System.Timers.Timer();
        private System.DateTime m_LastActiveTime = System.DateTime.MinValue;
        private object m_LockActiveCounter = new int();
        private ManualResetEvent m_KeepAliveClearEvent = new ManualResetEvent(true);

        private System.Threading.ManualResetEvent m_ClearEvent = new ManualResetEvent(true);

        private string m_Name = "";
        private IPAddress m_RemoteIP = IPAddress.Any;
        private int m_RemotePort = 0;
        private IPAddress m_LocalIP = IPAddress.Any;
        private int m_LocalPort = 0;
        #endregion

        internal CommClient()
        {
            m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			this.m_TimerKeepAlive.Interval = 30000; // 每30秒钟保活一次
			this.m_TimerKeepAlive.Elapsed += new System.Timers.ElapsedEventHandler(m_TimerKeepAlive_Elapsed);

        }

        #region Dummy 事件处理
        void CommClient_LogReceived(string sLog)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void CommClient_onConnectionBroken(ICommClient ptr, string sLog)
        {
            //this.Close();
            // do nothing
        }
        #endregion

        #region Communicator事件侦听
        void m_Comm_onConnectionBroken(ICommunicator ptr, string broken_info)
        {
            this.Close();
            InvokeConnectionBroken(this, broken_info);
        }

        void m_Comm_onLog(string sLog)
        {
            SendLog(sLog);
        }
        #endregion

        #region 发送日志信息
        void SendLog(string sLog)
        {
            LogHandler temp;
            lock (m_LockLogHandler)
            {
                temp = _onLog;
            }

            if (temp != null)
                temp(sLog);
        }
        #endregion

        #region 发送连接中断
        void InvokeConnectionBroken(ICommClient ptr, string reason)
        {
            ClientConnectionBrokenHandler temp;
            lock (m_LockClientConnectionBrokenHandler)
            {
                temp = _onConnectionBroken;
            }

            if (temp != null)
                temp(ptr, reason);
        }
        #endregion

        #region ICommClient 成员
        public void Init(string name, string ip, int port, int send_timeout, bool keepalive, 
            IMessageExtractor extractor, IMessageEncoder encoder, bool compress)
        {
            m_Name = name;
            m_RemoteIP = IPAddress.Parse(ip);
            m_RemotePort = port;
            m_SendTimeOut = send_timeout;
            m_bCompress = compress;

            m_bKeepAlive = keepalive;

            m_Extractor = extractor;
            m_Encoder = encoder;
        }

        #region 公有属性
        public string Name
        {
            get { return m_Name; }
        }

        public long ClientID
        {
            get { return m_ClientID; }
        }

        public IPAddress LocalIP
        {
            get { return m_LocalIP; }
        }

        public int LocalPort
        {
            get { return m_LocalPort; }
        }

        public IPAddress RemoteIP
        {
            get { return m_RemoteIP; }
        }

        public int RemotePort
        {
            get { return m_RemotePort; }
        }
        #endregion

        public long RunFlag
        {
            get { return Interlocked.Read(ref m_Started); }
        }

        public bool Start()
        {
            lock (this)
            {
                if (Interlocked.Exchange(ref m_Started, 1) == 1)
                    return true;

                if (!connectServer())
                {
                    Interlocked.Exchange(ref m_Started, 0);
                    return false;
                }

                if (m_Comm == null)
                {
                    m_ClientID = CommManager.AllocateClientID();
                    m_Comm = new AsyncCommunicator(this, m_ClientID, m_Client, null, m_Extractor, m_Encoder, m_bCompress);
                    m_Comm.onLog += new LogHandler(m_Comm_onLog);
                    m_Comm.onConnectionBroken += new ConnectionBrokenHandler(m_Comm_onConnectionBroken);
                }

                m_Comm.startWork();

                if (m_bKeepAlive)
                {
                    m_NotAliveCounter = 0;
                    this.m_TimerKeepAlive.Start();
                }

                //CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.RESPONSE, this, new Dictionary<Constants.TK_CommandType, byte>());
            }

            return true;
        }

        public bool Close()
        {
            lock (this)
            {
                try
                {
                    if (Interlocked.Exchange(ref m_Started, 0) == 0)
                        return true;

                    this.m_TimerKeepAlive.Stop();

                    // 取消所有响应包的等待
                    lock (this.m_MessagesWaitForResponse)
                    {
                        foreach (KeyValuePair<long, CommonPair<ICommunicationMessage, ManualResetEvent>> de in this.m_MessagesWaitForResponse)
                        {
                            CommonPair<ICommunicationMessage, ManualResetEvent> elem = de.Value;
                            ManualResetEvent mutex = elem.Second;
                            mutex.Set();
                        }

                        m_MessagesWaitForResponse.Clear();
                    }

                    m_KeepAliveClearEvent.WaitOne();

                    if (m_Comm != null)
                    {
                        m_Comm.endWork();
                        m_Comm.onLog -= new LogHandler(m_Comm_onLog);
                        m_Comm.onConnectionBroken -= new ConnectionBrokenHandler(m_Comm_onConnectionBroken);
                        m_Comm = null;
                    }

                    this.disconnectServer();

                    //CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.RESPONSE, this);
                    SendLog("通讯管理器已经停止.");
                }
                catch (Exception ex)
                {
                    SendLog("通讯管理器关闭与应用服务器的连接时发生异常:\n" + ex.ToString());
                }
            }

            return true;
        }

        private object m_LockLogHandler = new object();
        private LogHandler _onLog;
        public event LogHandler onLog
        {
            add
            {
                lock (m_LockLogHandler)
                    _onLog += value;
            }
            remove
            {
                lock (m_LockLogHandler)
                    _onLog -= value;
            }
        }

        private object m_LockClientConnectionBrokenHandler = new object();
        private ClientConnectionBrokenHandler _onConnectionBroken;
        public event ClientConnectionBrokenHandler onConnectionBroken
        {
            add
            {
                lock (m_LockClientConnectionBrokenHandler)
                    _onConnectionBroken += value;
            }
            remove
            {
                lock (m_LockClientConnectionBrokenHandler)
                    _onConnectionBroken -= value;
            }
        }

        public ICommunicationMessage SendCommand(ICommunicationMessage command)
        {
            return SendCommand(command, m_SendTimeOut);
        }

        public ICommunicationMessage SendCommand(ICommunicationMessage command, int timeout)
        {
            // TODO:  添加 Communicator.sendCommand 实现
            command.SeqID = CommandProcessor.AllocateID();
            try
            {
                ICommunicationMessage response = null;
                ManualResetEvent mutex = new ManualResetEvent(false);
                lock (this)
                {
                    if (Interlocked.Read(ref m_Started) == 0)
                        throw new Exception("与服务器的连接未建立.");

                    lock (this.m_MessagesWaitForResponse)
                    {
                        m_MessagesWaitForResponse[command.SeqID] = new CommonPair<ICommunicationMessage, ManualResetEvent>(null, mutex);
                    }

                    m_Comm.enqueueMessage(command);
                }

                // 等待响应包的回填
                if (!mutex.WaitOne(timeout * 1000, false))
                    throw new Exception(RemoteIP + ":" + RemotePort.ToString() + "服务器通讯超时");

                if (Interlocked.Read(ref m_Started) == 0)
                    throw new Exception(RemoteIP + ":" + RemotePort.ToString() + "服务器通讯中断");

                lock (m_MessagesWaitForResponse)
                {
                    response = m_MessagesWaitForResponse[command.SeqID].First;
                    m_MessagesWaitForResponse.Remove(command.SeqID);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                lock (m_MessagesWaitForResponse)
                {
                    m_MessagesWaitForResponse.Remove(command.SeqID);
                }
            }
        }

        public void PostCommand(ICommunicationMessage command)
        {
            // TODO:  添加 Communicator.sendCommand 实现
            command.SeqID = CommandProcessor.AllocateID();

            lock (this)
            {
                if (Interlocked.Read(ref m_Started) == 0)
                    throw new Exception("与服务器的连接未建立.");
                m_Comm.enqueueMessage(command);
            }
        }

        public int FilterResponse(List<ICommunicationMessage> income_msgs)
        {
            List<ICommunicationMessage> not_filtered = new List<ICommunicationMessage>();

            foreach (ICommunicationMessage message in income_msgs)
            {
                if (message.TK_CommandType != Constants.TK_CommandType.RESPONSE
                    || !message.Contains(Constants.MSG_PARANAME_RESPONSE_TO)
                    || !message.Contains("ClientID")
                    || Convert.ToInt64(message.GetValue("ClientID").ToString()) != m_ClientID)
                {
                    not_filtered.Add(message);
                    continue;
                }

                long id = 0;
                try
                {
                    id = Convert.ToInt64(message.GetValue(Constants.MSG_PARANAME_RESPONSE_TO).ToString());

                    lock (m_MessagesWaitForResponse)
                    {
                        if (m_MessagesWaitForResponse.ContainsKey(id))
                        {
                            // 是自身正在等待的响应包
                            CommonPair<ICommunicationMessage, ManualResetEvent> de = m_MessagesWaitForResponse[id];
                            ManualResetEvent mutex = de.Second;

                            de.First = message.clone(); // 复制信息
                            mutex.Set();
                        }
                        else
                            not_filtered.Add(message);
                    }

                }
                catch
                {
                    SendLog("报文缺少RESPONSE_TO关键字");
                    not_filtered.Add(message);
                }
            }

            int filtered_count = income_msgs.Count - not_filtered.Count;
            income_msgs.Clear();
            income_msgs.AddRange(not_filtered);
            return filtered_count;
        }
        #endregion

        #region ICommandHandler 成员, 用于处理下发消息的响应包

        public void handleCommand(ICommunicationMessage message)
        {
        }

        #endregion

        #region 连接服务器
        public bool connectServer()
        {
            // TODO:  添加 Communicator.connectServer 实现
            try
            {   
                lock (m_Client)
                {
                    m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint ep = new IPEndPoint(RemoteIP, RemotePort);
                    m_Client.Connect(ep);

                    string[] subs = m_Client.LocalEndPoint.ToString().Split(':');
                    m_LocalIP = (m_Client.LocalEndPoint as IPEndPoint).Address;
                    m_LocalPort = (m_Client.LocalEndPoint as IPEndPoint).Port;
                }

                SendLog("通讯管理器已连接到应用服务器:" + m_Client.RemoteEndPoint.ToString());
                return true;
            }
            catch (Exception ex)
            {
                SendLog("通讯管理器连接到应用服务器时发生异常:\n" + ex.ToString());

                lock (m_Client)
                {
                    try
                    {
                        m_Client.Close();
                    }
                    catch { }
                }
                return false;
            }
        }

        public void disconnectServer()
        {
            try
            {
                lock (m_Client)
                {
                    if (m_Client.Connected)
                    {
                        m_Client.Shutdown(SocketShutdown.Both);
                        m_Client.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                SendLog("通讯管理器关闭与应用服务器的连接时发生异常:\n" + ex.ToString());
            }
        }
        #endregion
 
		#region 与服务器的连接保活定时器
		private int m_NotAliveCounter = 0;
		private bool m_bInKeepAlive = false;
		private object m_bLockKeepAlive = new int();
		private void m_TimerKeepAlive_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{			
			lock (m_bLockKeepAlive)
			{
				if (Interlocked.Read(ref m_Started) == 0)
					return;

				if (m_bInKeepAlive)
					return;
				else
					m_bInKeepAlive = true;
			}

			try
			{
                m_KeepAliveClearEvent.Reset();

				lock (this.m_LockActiveCounter)
				{
					System.TimeSpan span = System.DateTime.Now - this.m_LastActiveTime;
					if (span.TotalMilliseconds < this.m_TimerKeepAlive.Interval)
						return; // 距离上次Socket活动还不到一个检测周期，则返回
				}

                MSGTYPE cm = new MSGTYPE();
                cm.TK_CommandType = Constants.TK_CommandType.KEEPALIVE;

                ICommunicationMessage resp = null;

                bool alive = false;
                try
                {
                    resp = SendCommand(cm);
                    if (resp == null)
                        alive = false;
                    else
                        alive = true;
                }
                catch (Exception ex)
                {
                    SendLog(ex.ToString());
                    alive = false;
                }

                if (alive)
                {
                    this.m_NotAliveCounter = 0;
                }
                else
                {
                    ++this.m_NotAliveCounter;
                    if (this.m_NotAliveCounter >= 3) // 连续五次
                    {
                        this.m_NotAliveCounter = 0;

                        m_KeepAliveClearEvent.Set();
                        InvokeConnectionBroken(this, "保活失败");
                    }
                }

			}
			catch (Exception ex)
			{
                SendLog(ex.ToString());
			}
			finally
			{
				lock (m_bLockKeepAlive)
					m_bInKeepAlive = false;

                m_KeepAliveClearEvent.Set();
			}

		}
		#endregion

    }
}
