using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace TK_AlarmManagement
{
    public class CommServer<INTERPRETER, MSGTYPE, EXTRACTOR, ENCODER> : ICommServer, ICommandHandler
        where INTERPRETER : ICommandInterpreter, new()
        where MSGTYPE : ICommunicationMessage, new()
        where EXTRACTOR : IMessageExtractor, new()
        where ENCODER : IMessageEncoder, new()
    {
        #region 私有成员
        private string m_Name = "Default Server";
        private int m_Port = 0;
        private int m_MaxClients = 0;
        private int m_SyncCmdTimeout = 0;
        private Dictionary<Constants.TK_CommandType, byte> m_AcceptedMsgs = null;
        private Dictionary<Constants.TK_CommandType, byte> m_SuperCmds = null;
        private bool m_bKeepAlive = true;
        private bool m_bCompress = false;

        private long m_Run = 0;

        private Socket m_Listener = null;

        private ManualResetEvent m_AccepterStopEvent = new ManualResetEvent(true);

        private Dictionary<long, ICommunicator> m_ClientCommunicators = null;

        private Dictionary<long, CommonPair<ICommunicationMessage, ManualResetEvent>> m_MessagesWaitForResponse = new Dictionary<long, CommonPair<ICommunicationMessage, ManualResetEvent>>();

        System.Timers.Timer m_TimerKeepAlive = new System.Timers.Timer(30000);
        System.Timers.Timer m_TimerCheckBroken = new System.Timers.Timer(1000);
        ManualResetEvent m_KeepAliveClearEvent = new ManualResetEvent(true);
        ManualResetEvent m_CheckBrokenClearEvent = new ManualResetEvent(true);
        Dictionary<long, int> m_ClientActiveCounter = null;
        #endregion

        #region 构造函数
        internal CommServer()
        {
            m_AcceptedMsgs = new Dictionary<Constants.TK_CommandType, byte>();
            m_SuperCmds = new Dictionary<Constants.TK_CommandType, byte>();

            m_ClientCommunicators = new Dictionary<long, ICommunicator>(32);
            m_ClientActiveCounter = new Dictionary<long, int>(32);

            m_TimerKeepAlive.Elapsed += m_TimerKeepAlive_Elapsed;
            m_TimerCheckBroken.Elapsed += m_TimerCheckBroken_Elapsed;
        }
        #endregion

        private object m_LockClientConnectionBrokenHandler = new object();
        private ConnectionBrokenHandler _onConnectionBroken;
        public event ConnectionBrokenHandler onClientConnectionBroken
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

        #region 保活定时器
        object m_LockKeepAlive = new object();
        bool m_bInKeepAlive = false;
        void m_TimerKeepAlive_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                m_KeepAliveClearEvent.Reset();

                if (Interlocked.Read(ref m_Run) == 0)
                return;

                if (!m_TimerKeepAlive.Enabled)
                    return;

                lock (m_LockKeepAlive)
                {
                    if (m_bInKeepAlive)
                        return;
                    else
                        m_bInKeepAlive = true;
                }

                Dictionary<long, int> t = new Dictionary<long, int>();
                lock (m_ClientActiveCounter)
                {
                    foreach (KeyValuePair<long, int> pair in m_ClientActiveCounter)
                        t.Add(pair.Key, pair.Value);
                }

                foreach (KeyValuePair<long, int> pair in t)
                {
                    if (Interlocked.Read(ref m_Run) == 0)
                        break;

                    MSGTYPE cm = new MSGTYPE();
                    cm.TK_CommandType = Constants.TK_CommandType.KEEPALIVE;

                    bool commfail = false;
                    ICommunicationMessage resp;
                    try
                    {
                        resp = SendCommand(pair.Key, cm);
                        if (resp == null)
                            commfail = true;
                    }
                    catch
                    {
                        commfail = true;
                    }

                    if (commfail)
                    {
                        bool fail = false;
                        lock (m_ClientActiveCounter)
                        {
                            if (m_ClientActiveCounter.ContainsKey(pair.Key))
                            {
                                ++m_ClientActiveCounter[pair.Key];
                                if (m_ClientActiveCounter[pair.Key] > 3)
                                    fail = true;
                            }
                        }

                        if (fail)
                        {
                            m_KeepAliveClearEvent.Set();

                            ICommunicator commer = null;
                            lock (m_ClientCommunicators)
                            {
                                if (m_ClientCommunicators.ContainsKey(pair.Key))
                                {
                                    commer = m_ClientCommunicators[pair.Key];
                                    commer.endWork();
                                }
                            }

                            if (commer != null)
                            {
                                SendLog("与客户端: " + commer.RemoteEP + " 保活失败, 剔除客户端数据.");

                                _HandleConnectionBroken(commer, "保活失败，被服务器剔除");
                            }
                        }
                    }
                    else
                        lock (m_ClientActiveCounter)
                        {
                            if (m_ClientActiveCounter.ContainsKey(pair.Key))
                                m_ClientActiveCounter[pair.Key] = 0;
                        }
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
            finally
            {
                m_bInKeepAlive = false;

                m_KeepAliveClearEvent.Set();
            }
        }
        #endregion

        #region 接受连接接入
        private void _asyncAccepter()
        {
            m_Listener.BeginAccept(_accepterCallback, null);
        }

        private void _accepterCallback(IAsyncResult ar)
        {
            Socket newclient = m_Listener.EndAccept(ar);

            lock (m_ClientCommunicators)
            {
                if (m_ClientCommunicators.Count == m_MaxClients)
                {
                    byte[] buf = System.Text.Encoding.Default.GetBytes("Client connection exceeds. New request is closing.");

                    try
                    {
                        newclient.Send(buf);
                        newclient.Shutdown(SocketShutdown.Both);
                        newclient.Close();
                    }
                    catch (Exception ex)
                    {
                        SendLog(ex.ToString());
                    }
                }
                else
                {
                    try
                    {
                        long id = CommManager.AllocateClientID();
                        ICommunicator comm = new AsyncCommunicator(this, id, newclient, new INTERPRETER(), new EXTRACTOR(), new ENCODER(), m_bCompress);

                        lock (m_ClientCommunicators)
                            m_ClientCommunicators[id] = comm;

                        lock (m_ClientActiveCounter)
                            m_ClientActiveCounter[id] = 0;

                        comm.onLog += new LogHandler(comm_onLog);
                        comm.onConnectionBroken += new ConnectionBrokenHandler(_HandleConnectionBroken);
                        comm.startWork();

                        // 发送报文注册客户端
                        MSGTYPE msg = new MSGTYPE();
                        msg.SeqID = CommandProcessor.AllocateID();
                        msg.TK_CommandType = Constants.TK_CommandType.REGISTERCLIENT;
                        msg.SetValue("ClientID", id);
                        msg.SetValue("SERVERNAME", Name);
                        msg.SetValue(Constants.MSG_PARANAME_AUTHORIZED, false);
                        CommandProcessor.instance().DispatchCommand(msg);

                        SendLog("客户端 " + newclient.RemoteEndPoint.ToString() + " 连接到:" + Name);

                        SendLog(Name + "当前客户端的数量:" + m_ClientCommunicators.Count);

                        //int wt, iot;
                        //ThreadPool.GetAvailableThreads(out wt, out iot);
                        //SendLog("可用工作线程数: " + wt + " I/O线程数: " + iot);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            newclient.Close();
                            SendLog(ex.ToString());
                        }
                        catch { }
                    }
                }
            } // endlock

            if (Interlocked.Read(ref m_Run) == 1)
                _asyncAccepter();
        }

        private void _accepter()
        {
            try
            {
                m_AccepterStopEvent.Reset();

                while (Interlocked.Read(ref m_Run) == 1)
                {
                    Socket newclient = m_Listener.Accept();//.AcceptTcpClient();

                    lock (m_ClientCommunicators)
                    {
                        if (m_ClientCommunicators.Count == m_MaxClients)
                        {
                            byte[] buf = System.Text.Encoding.Default.GetBytes("Client connection exceeds. New request is closing.");

                            try
                            {
                                newclient.Send(buf);
                                newclient.Shutdown(SocketShutdown.Both);
                                newclient.Close();
                            }
                            catch (Exception ex)
                            {
                                SendLog(ex.ToString());
                            }

                            continue;
                        }
                        else
                        {
                            try
                            {
                                //IPEndPoint ep = newclient.Client.RemoteEndPoint as IPEndPoint;
                                //byte[] addr = ep.Address.GetAddressBytes();
                                //long id = addr[3] * 0x10000000000L +
                                //    addr[2] * 0x100000000L +
                                //    addr[1] * 0x1000000L +
                                //    addr[0] * 0x10000L + ep.Port;
                                long id = CommManager.AllocateClientID();
                                ICommunicator comm = new AsyncCommunicator(this, id, newclient, new INTERPRETER(), new EXTRACTOR(), new ENCODER(), m_bCompress);

                                lock (m_ClientCommunicators)
                                    m_ClientCommunicators[id] = comm;

                                lock (m_ClientActiveCounter)
                                    m_ClientActiveCounter[id] = 0;

                                //newclient.NoDelay = true;
                                //newclient.LingerState = new LingerOption(false, 0);
                                //newclient.SendBufferSize = 2048;
                                //newclient.SendTimeout = 1000;

                                comm.onLog += new LogHandler(comm_onLog);
                                comm.onConnectionBroken += new ConnectionBrokenHandler(_HandleConnectionBroken);
                                comm.startWork();

                                // 发送报文注册客户端
                                MSGTYPE msg = new MSGTYPE();
                                msg.SeqID = CommandProcessor.AllocateID();
                                msg.TK_CommandType = Constants.TK_CommandType.REGISTERCLIENT;
                                msg.SetValue("ClientID", id);
                                msg.SetValue("SERVERNAME", Name);
                                msg.SetValue(Constants.MSG_PARANAME_AUTHORIZED, false);
                                CommandProcessor.instance().DispatchCommand(msg);

                                SendLog("客户端 " + newclient.RemoteEndPoint.ToString() + " 连接到:" + Name);

                                SendLog(Name + "当前客户端的数量:" + m_ClientCommunicators.Count);

                                //int wt, iot;
                                //ThreadPool.GetAvailableThreads(out wt, out iot);
                                //SendLog("可用工作线程数: " + wt + " I/O线程数: " + iot);
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    //NetworkStream ns = newclient.GetStream();
                                    //if (ns != null)
                                    //    ns.Close();

                                    newclient.Close();
                                    SendLog(ex.ToString());
                                }
                                catch { }
                            }
                        }
                    } // endlock
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
            finally
            {
                m_AccepterStopEvent.Set();
            }
        }
        #endregion

        #region ICommServer 成员
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="accepted_msgs"></param>
        /// <param name="super_cmd"></param>
        /// <param name="port"></param>
        /// <param name="maxclients"></param>
        /// <param name="send_timeout">主动同步发送的命令的超时时间, 单位为秒</param>
        public void Init(string name, List<Constants.TK_CommandType> accepted_msgs, List<Constants.TK_CommandType> super_cmd, int port, int maxclients, int send_timeout, bool keepalive, bool compress)
        {
            m_Name = name;
            m_Port = port;
            m_MaxClients = maxclients;
            m_SyncCmdTimeout = send_timeout;
            m_bKeepAlive = keepalive;
            m_bCompress = compress;

            if (accepted_msgs != null)
            {
                foreach (Constants.TK_CommandType type in accepted_msgs)
                    m_AcceptedMsgs.Add(type, 0);
            }

            if (super_cmd != null)
            {
                foreach (Constants.TK_CommandType type in super_cmd)
                    m_SuperCmds.Add(type, 0);
            }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public IPAddress LocalIP
        {
            get { return IPAddress.Any; }
        }

        public int LocalPort
        {
            get { return m_Port; }
        }

        public long RunFlag
        {
            get { return Interlocked.Read(ref m_Run); }
        }

        public bool Start()
        {
            lock (this)
            {
                try
                {
                    if (Interlocked.Exchange(ref m_Run, 1) == 1)
                        return true;

                    m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//(IPAddress.Any, m_Port);
                    m_Listener.Bind(new IPEndPoint(IPAddress.Any, m_Port));
                    m_Listener.Listen(100);
                    //m_Listener.Start();

                    //Thread thr = new Thread(new ThreadStart(_accepter));
                    //thr.Start();

                    SendLog(Name + " started at " + m_Listener.LocalEndPoint.ToString());

                    foreach (KeyValuePair<Constants.TK_CommandType, byte> pair in m_AcceptedMsgs)
                    {
                        CommandProcessor.instance().registerReportHandler(pair.Key, this, m_SuperCmds);
                    }

                    if (m_bKeepAlive)
                        m_TimerKeepAlive.Start();

                    m_TimerCheckBroken.Start();

                    _asyncAccepter();
                    //CommandProcessor.instance().registerReportHandler(Constants.TK_CommandType.RESPONSE, this, new Dictionary<Constants.TK_CommandType, byte>());

                    SendLog(Name + (m_bCompress ? "(压缩)" : "") + "已启动,最大客户端数量:" + m_MaxClients);
                }
                catch (Exception ex)
                {
                    Interlocked.Exchange(ref m_Run, 0);
                    throw ex;
                }
            }

            return true;
        }

        public bool Close()
        {
            lock (this)
            {
                if (Interlocked.Exchange(ref m_Run, 0) == 0)
                    return true;

                //m_Listener.Shutdown(SocketShutdown.Both);
                m_Listener.Close();

                m_AccepterStopEvent.WaitOne();

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

                m_TimerCheckBroken.Stop();
                m_CheckBrokenClearEvent.WaitOne();



                lock (m_ClientCommunicators)
                {
                    foreach (Communicator comm in m_ClientCommunicators.Values)
                    {
                        comm.endWork();
                    }

                    m_ClientCommunicators.Clear();
                }

                lock (m_ClientActiveCounter)
                    m_ClientActiveCounter.Clear();

                foreach (KeyValuePair<Constants.TK_CommandType, byte> pair in m_AcceptedMsgs)
                {
                    CommandProcessor.instance().unregisterReportHandler(pair.Key, this);
                }

                //CommandProcessor.instance().unregisterReportHandler(Constants.TK_CommandType.RESPONSE, this);
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

        public ICommunicationMessage SendCommand(long clientid, ICommunicationMessage command)
        {
            return SendCommand(clientid, command, m_SyncCmdTimeout);
        }

        public ICommunicationMessage SendCommand(long clientid, ICommunicationMessage command, int timeout)
        {
            // TODO:  添加 Communicator.sendCommand 实现
            command.SeqID = CommandProcessor.AllocateID();
            try
            {
                ICommunicator comm = null;
                lock (m_ClientCommunicators)
                {
                    if (!m_ClientCommunicators.ContainsKey(clientid))
                        return null;

                    comm = m_ClientCommunicators[clientid];
                }

                ICommunicationMessage response = null;
                ManualResetEvent mutex = new ManualResetEvent(false);

                lock (this.m_MessagesWaitForResponse)
                {
                    m_MessagesWaitForResponse[command.SeqID] = new CommonPair<ICommunicationMessage, ManualResetEvent>(null, mutex);
                }

                comm.enqueueMessage(command);

                // 等待响应包的回填
                if (!mutex.WaitOne(timeout * 1000, false))
                    throw new Exception(comm.RemoteEP.ToString() + "服务器通讯超时");

                lock (m_ClientCommunicators)
                {
                    if (!m_ClientCommunicators.ContainsKey(clientid))
                        throw new Exception(comm.RemoteEP.ToString() + "服务器通讯中断");
                }

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

        public void PostCommand(long clientid, ICommunicationMessage command)
        {
            command.SeqID = CommandProcessor.AllocateID();
            ICommunicator comm = null;
            lock (m_ClientCommunicators)
            {
                if (!m_ClientCommunicators.ContainsKey(clientid))
                    throw new Exception(string.Format("客户端[{0}]不存在.", clientid));

                comm = m_ClientCommunicators[clientid];
            }

            comm.enqueueMessage(command);
        }

        public int FilterResponse(List<ICommunicationMessage> income_msgs)
        {
            List<ICommunicationMessage> not_filtered = new List<ICommunicationMessage>();

            foreach (ICommunicationMessage message in income_msgs)
            {
                if (message.TK_CommandType != Constants.TK_CommandType.RESPONSE
                    || !message.Contains(Constants.MSG_PARANAME_RESPONSE_TO)
                    || !message.Contains("ClientID"))
                {
                    not_filtered.Add(message);
                    continue;
                }

                long clientid = Convert.ToInt64(message.GetValue("ClientID").ToString());
                lock (m_ClientCommunicators)
                {
                    if (!m_ClientCommunicators.ContainsKey(clientid))
                    {
                        not_filtered.Add(message);
                        continue; // 非来自自身管理的client的响应包, 不处理
                    }
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

        #region ICommandHandler 成员

        public void handleCommand(ICommunicationMessage message)
        {
            long clientid = 0, immediateid = 0;

            try
            {
                clientid = Convert.ToInt64(message.GetValue("ClientID").ToString());
                lock (m_ClientCommunicators)
                {
                    if (!m_ClientCommunicators.ContainsKey(clientid))
                        return; // 非来自自身管理的client的响应包, 不处理
                }

            }
            catch
            {
                return; // 没有clientid的报文不处理
            }

            try
            {
                // 非自身发包的响应以及向客户端的主动发包都发送到客户端
                if (message.Contains(Constants.MSG_PARANAME_IMMEDIATE_ID))
                    immediateid = Convert.ToInt64(message.GetValue(Constants.MSG_PARANAME_IMMEDIATE_ID).ToString());

                if (clientid == Constants.BOARDCAST_CLIENT_ID)
                {
                    lock (m_ClientCommunicators)
                    {
                        foreach (Communicator disp in m_ClientCommunicators.Values)
                        {
                            disp.enqueueMessage(message.clone());
                        }
                    }
                }
                else
                {
                    ICommunicator dispatcher = null;
                    lock (m_ClientCommunicators)
                    {
                        if (!m_ClientCommunicators.ContainsKey(clientid))
                            return;

                        dispatcher = m_ClientCommunicators[clientid];
                    }

                    dispatcher.enqueueMessage(message);
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
            finally
            {
            }
        }

        #endregion

        #region ClientCommunicator的事件处理：连接中断、信息日志
        object m_LockCheckBroken = new object();
        bool m_bInCheckBroken = false;
        List<BrokenInfo> m_BrokenClient = new List<BrokenInfo>(128);
        void m_TimerCheckBroken_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                m_CheckBrokenClearEvent.Reset();

                if (Interlocked.Read(ref m_Run) == 0)
                    return;

                if (!m_TimerCheckBroken.Enabled)
                    return;

                lock (m_LockCheckBroken)
                {
                    if (m_bInCheckBroken)
                        return;
                    else
                        m_bInCheckBroken = true;
                }


                List<BrokenInfo> temp = null;
                lock (m_BrokenClient)
                {
                    if (m_BrokenClient.Count > 0)
                    {
                        temp = m_BrokenClient;
                        m_BrokenClient = new List<BrokenInfo>();
                    }
                }

                if (temp == null)
                    return;

                int live_count = 0;
                foreach (BrokenInfo info in temp)
                {
                    bool isclient = false;
                    ICommunicator client = null;
                    lock (m_ClientCommunicators)
                    {
                        isclient = m_ClientCommunicators.TryGetValue(info.Commer.ClientID, out client);
                        if (isclient) m_ClientCommunicators.Remove(info.Commer.ClientID);
                        live_count = m_ClientCommunicators.Count;
                    }

                    lock (m_ClientActiveCounter)
                        m_ClientActiveCounter.Remove(info.Commer.ClientID);

                    if (isclient)
                    {
                        MSGTYPE command = new MSGTYPE();
                        command.TK_CommandType = Constants.TK_CommandType.UNREGISTERCLIENT;
                        command.SetValue("ClientID", info.Commer.ClientID);
                        command.SeqID = CommandProcessor.AllocateID();
                        CommandProcessor.instance().DispatchCommand(command);

                        SendLog("客户端: " + info.Info + " 已经从" + Name + "断开.");
                    }

                    InvokeConnectionBroken(info.Commer, info.Info);
                }

                SendLog(Name + "当前客户端的数量:" + live_count);
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
            finally
            {
                m_bInCheckBroken = false;

                m_CheckBrokenClearEvent.Set();
            }
        }

        class BrokenInfo
        {
            public ICommunicator Commer;
            public string Info;
        }

        void _HandleConnectionBroken(ICommunicator ptr, string broken_info)
        {
            BrokenInfo info = new BrokenInfo();
            info.Commer = ptr;
            info.Info = broken_info;

            lock (m_BrokenClient)
                m_BrokenClient.Add(info);
            //bool isclient = false;
            //int count = 0;
            //lock (m_ClientCommunicators)
            //{
            //    isclient = m_ClientCommunicators.Remove(id);
            //    count = m_ClientCommunicators.Count;
            //}

            //lock (m_ClientActiveCounter)
            //    m_ClientActiveCounter.Remove(id);

            //if (isclient)
            //{
            //    MSGTYPE command = new MSGTYPE();
            //    command.TK_CommandType = Constants.TK_CommandType.UNREGISTERCLIENT;
            //    command.SetValue("ClientID", id);
            //    command.SeqID = CommandProcessor.AllocateID();
            //    CommandProcessor.instance().DispatchCommand(command);

            //    SendLog("客户端: " + clientinfo + " 已经从" + Name + "断开.");

            //    SendLog(Name + "当前客户端的数量:" + count);
            //}
        }

        void comm_onLog(string sLog)
        {
            SendLog(sLog);
        }
        #endregion

        #region 发送连接中断
        void InvokeConnectionBroken(ICommunicator ptr, string reason)
        {
            try
            {
                ConnectionBrokenHandler temp;
                lock (m_LockClientConnectionBrokenHandler)
                {
                    temp = _onConnectionBroken;
                }

                if (temp != null)
                    temp(ptr, reason);
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }
        }
        #endregion

    }
}
