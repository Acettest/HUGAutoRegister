using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Compression;
using TK_AlarmManagement;

namespace TK_AlarmManagement
{
    public class Communicator : ICommunicator
    {
        #region 构造函数
        /// <summary>
        /// ClientCommunicator构造函数
        /// </summary>
        /// <param name="clientid">客户端ID</param>
        /// <param name="client">客户端的TCP实体</param>
        /// <param name="filter">报文过滤器</param>
        public Communicator(ICommer parent_commer, long clientid, Socket client, ICommandInterpreter intepreter, 
            IMessageExtractor extractor, IMessageEncoder encoder, bool compress)
        {
            m_ParentCommer = parent_commer;
            m_ClientID = clientid;
            m_Client = client;
            m_ClientInfo = client.RemoteEndPoint as IPEndPoint;

            m_Interpreter = intepreter;

            if (m_Interpreter != null)
                m_Interpreter.CommunicatorObj = this;

            m_bCompress = compress;

            m_Extractor = extractor;
            m_Extractor.Compressed = compress;

            m_Encoder = encoder;
            onConnectionBroken += new ConnectionBrokenHandler(ClientCommunicator_onConnectionBroken);
        }
        #endregion

        #region 私有成员
        private ICommer m_ParentCommer = null;
        private Socket m_Client = null;
        private long m_ClientID = -1;
        private IPEndPoint m_ClientInfo = null;

        private ICommandInterpreter m_Interpreter = null;

        private Queue<ICommunicationMessage> m_Messages = new Queue<ICommunicationMessage>(16);
        private Queue<ICommunicationMessage> m_DelayedMessages = new Queue<ICommunicationMessage>();

        private AutoResetEvent m_SignalNewMessage = new AutoResetEvent(false);
        private ManualResetEvent m_ClearEvent = new ManualResetEvent(true);
        private ManualResetEvent m_ListenerClearEvent = new ManualResetEvent(true);

        private long m_Run = 0;
        private bool m_bCompress = false;

        private IMessageExtractor m_Extractor = null;

        private IMessageEncoder m_Encoder = null;
        #endregion

        #region 公共属性
        public long ClientID
        {
            get { return m_ClientID; }
        }

        public IPEndPoint RemoteEP
        {
            get { return m_ClientInfo; }
        }
        #endregion

        #region 事件接口
        ConnectionBrokenHandler _onConnectionBroken;
        object m_LockConnectionBroken = new object();
        public event ConnectionBrokenHandler onConnectionBroken
        {
            add
            {
                lock (m_LockConnectionBroken)
                    _onConnectionBroken += value;
            }
            remove
            {
                lock (m_LockConnectionBroken)
                    _onConnectionBroken -= value;
            }
        }


        LogHandler _onLog;
        object m_LockLogHandler = new object();
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
        #endregion

        #region 启停接口
        public void startWork()
        {
            Interlocked.Exchange(ref m_Run, 1);

            ThreadPool.QueueUserWorkItem(new WaitCallback(_worker));
            ThreadPool.QueueUserWorkItem(new WaitCallback(_listener));
            //Thread thr = new Thread(new ThreadStart(_worker));
            //thr.Start();

            //Thread thr2 = new Thread(new ThreadStart(_listener));
            //thr2.Start();

            if (m_Interpreter != null)
                m_Interpreter.Start();
        }

        public void endWork()
        {
            if (Interlocked.Exchange(ref m_Run, 0) == 0)
                return;

            m_SignalNewMessage.Set();
            Thread.Sleep(0);

            m_ClearEvent.WaitOne();

            m_ListenerClearEvent.WaitOne();

            if (m_Interpreter != null)
                m_Interpreter.Stop();

            try
            {
                if (m_Client != null)
                {
                    lock (m_Client)
                    {
                        //if (m_Client.Connected)
                        //{
                        //    NetworkStream ns = m_Client.GetStream();
                        //    if (ns != null)
                        //        ns.Close();
                        //}

                        //if (m_Client.Connected)
                        //    m_Client.GetStream().Close();
                        m_Client.Shutdown(SocketShutdown.Both);
                        m_Client.Close();
                    }
                    //m_Client = null;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    SendLog(ex.Message);
                }
                catch { }
            }

            lock (m_Messages)
                m_Messages.Clear();
        }
        #endregion

        #region 入队列操作
        public void enqueueMessage(ICommunicationMessage msg)
        {
            ICommunicationMessage parsed = null;
            if (m_Interpreter != null)
            {
                parsed = m_Interpreter.ImmediatelyInterpret(msg);

                if (parsed == null)
                {
                    if (m_Interpreter.DelayedInterpret(msg) == true)
                        return;
                    else
                    {
                        lock (m_Messages)
                            m_Messages.Enqueue(msg);
                    }
                }
                else
                {
                    lock (m_Messages)
                        m_Messages.Enqueue(parsed);
                }
            }
            else
            {
                lock (m_Messages)
                    m_Messages.Enqueue(msg);
            }

            m_SignalNewMessage.Set();
        }

        public void enqueueDelayedMessages(ICommunicationMessage msg)
        {
            lock (m_DelayedMessages)
                m_DelayedMessages.Enqueue(msg);

            m_SignalNewMessage.Set();
        }

        #endregion

        #region 发送与侦听
        private long m_last_seq_id = 0;
        private void _worker(object state)
        {
            try
            {
                m_ClearEvent.Reset();

                while (Interlocked.Read(ref m_Run) == 1)
                {
                    m_SignalNewMessage.WaitOne();
                    if (Interlocked.Read(ref m_Run) == 0)
                        return;

                    // 一次命令激活发送完队列中所有数据
                    while (Interlocked.Read(ref m_Run) == 1)
                    {
                        List<ICommunicationMessage> tobesent = new List<ICommunicationMessage>();
                        lock (m_Messages)
                        {
                            while (m_Messages.Count > 0)
                            {
                                tobesent.Add(m_Messages.Dequeue());
                            } // endif

                            Thread.Sleep(0);
                        }

                        foreach (ICommunicationMessage msg in tobesent)
                        {
                                msg.SetValue("LAST_SEQ_ID", m_last_seq_id);
                                m_last_seq_id = msg.SeqID;

                                //msg.RemoveKey("ClientID");
                                _sendMessage(msg);
                        }
                        tobesent.Clear();

                        List<ICommunicationMessage> alarms = new List<ICommunicationMessage>();
                        lock (m_DelayedMessages)
                        {
                            if (m_DelayedMessages.Count == 0)
                                break;

                            int i = 0;
                            while (m_DelayedMessages.Count > 0 && ++i <= 50)
                            {
                                ICommunicationMessage msg = m_DelayedMessages.Dequeue();

                                msg.SetValue("LAST_SEQ_ID", m_last_seq_id);
                                m_last_seq_id = msg.SeqID;

                                //msg.RemoveKey("ClientID");
                                alarms.Add(msg);
                            }
                        }

                        if (alarms.Count > 0)
                            _sendMessage(alarms);

                        Thread.Sleep(0);

                        lock (m_DelayedMessages)
                        {
                            if (m_DelayedMessages.Count == 0)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_ClearEvent.Set();

                try
                {
                    SendLog(ex.Message);
                    InvokeConnectionBroken(this, m_ClientInfo.ToString());
                }
                catch { }
            }
            finally
            {
                m_ClearEvent.Set();
            }
        }

        //private object m_LockCommunication = new int();
        private void _listener(object state)
        {
            int buflen = 0;
            byte[] buf = new byte[8192];
            //C5.LinkedList<byte> totalBuf = new C5.LinkedList<byte>();
            DefLib.Util.ArrayBuilder<byte> totalBuf = new DefLib.Util.ArrayBuilder<byte>();

            byte[] rest = null;

            try
            {
                //NetworkStream stream = m_Client.GetStream();

                m_ListenerClearEvent.Reset();

                while (Interlocked.Read(ref m_Run) == 1)
                {
                    if (Interlocked.Read(ref m_Run) == 0)
                        break;

                    try
                    {
                        totalBuf.Clear();

                        /// 读取Socket缓冲区
                        /// 
                        while (true)
                        {
                            lock (m_Client)
                            {
                                if (!m_Client.Poll(100, SelectMode.SelectRead))
                                    break;
                            }

                            if (Interlocked.Read(ref m_Run) == 0)
                            {
                                totalBuf.Clear();
                                break;
                            }

                            buflen = 0;
                            lock (m_Client)
                            {
                                if (m_Client.Connected)
                                {
                                    buflen = m_Client.Receive(buf, buf.Length, SocketFlags.None);
                                }
                            }

                            if (buflen > 0)
                            {
                                totalBuf.Append(buf, buflen);
                                //for (int i = 0; i < buflen; ++i)
                                //    totalBuf.Add(buf[i]);
                            }
                            else
                            {
                                throw new CommException("读到零字节数据.");
                            }

                            if (totalBuf.Length > 102400)
                                break;
                        }

                        if (totalBuf.Length > 0)
                        {
                            if (rest != null)
                            {
                                totalBuf.Insert(0, rest, rest.Length);
                                //totalBuf.InsertAll(0, rest);
                                rest = null;
                            }

                            if (m_Extractor == null)
                            {
                                totalBuf.Clear();
                                continue;
                            }

                            //SendLog(totalBuf.Count + " Bytes recved: " + DumpByte2Str(totalBuf.ToArray()));

                            // 解析收到的数据
                            List<ICommunicationMessage> messages = m_Extractor.extractMessages(totalBuf.ToArray(), ref rest);

                            // 派发给侦听者
                            if (messages.Count > 0)
                            {
                                List<ICommunicationMessage> queue = new List<ICommunicationMessage>();
                                foreach (ICommunicationMessage command in messages)
                                {
                                    try
                                    {
                                        command.SetValue("ClientID", m_ClientID);

                                        if (command.TK_CommandType == Constants.TK_CommandType.KEEPALIVE)
                                        {
                                            command.TK_CommandType = Constants.TK_CommandType.RESPONSE;
                                            command.SetValue("RESPONSE_TO", command.SeqID);
                                            command.SetValue("RESULT", "OK");
                                            command.SeqID = CommandProcessor.AllocateID();

                                            enqueueMessage(command);
                                            continue;
                                        }

                                        queue.Add(command);
                                    } // end try
                                    catch
                                    { }
                                } // end for

                                if (queue.Count > 0)
                                {
                                    m_ParentCommer.FilterResponse(queue);
                                }

                                if (queue.Count > 0)
                                    CommandProcessor.instance().DispatchCommands(queue);
                            } // end if messages.Count
                        }
                    }
                    catch (CommException commex)
                    {
                        throw new CommException(commex.ToString());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("通讯异常:\n" + ex.ToString());
                    }

                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //SendLog(m_ClientInfo.ToString() + "侦听线程异常退出:\n" + ex.ToString());
                    m_ListenerClearEvent.Set();
                }
                catch { }

                try
                {
                    InvokeConnectionBroken(this, ex.Message);
                    return;
                }
                catch { }
            }
            finally
            {
                try
                {
                    m_ListenerClearEvent.Set();
                }
                catch { }
            }
        }

        private bool _sendMessage(ICommunicationMessage msg)
        {
            byte[] newbuf = m_Encoder.encodeMessage(msg, m_bCompress);
            return _sendMessage(newbuf);
        }

        private bool _sendMessage(List<ICommunicationMessage> msgs)
        {
            byte[] newbuf = m_Encoder.encodeMessages(msgs, m_bCompress);
            return _sendMessage(newbuf);
        }

        private bool _sendMessage(string msgstr, Encoding encode)
        {
            byte[] newbuf = m_Encoder.encodeMessage(msgstr, encode, m_bCompress);
            return _sendMessage(newbuf);
        }

        private bool _sendMessage(byte[] newbuf)
        {
            try
            {
                lock (m_Client)
                {
                    if (m_Client.Connected)
                    {
                        m_Client.Send(newbuf);
                    }
                }
            }
            catch (SocketException ex)
            {
                if (-1 != ex.ToString().IndexOf("没有正确答复或连接的主机没有反应"))
                {
                    int i = ex.ErrorCode;
                }

                throw ex;
            }
            catch (IOException ex)
            {
                if (-1 != ex.ToString().IndexOf("没有正确答复或连接的主机没有反应"))
                {
                    Exception ee = ex.InnerException;
                    if (ee is SocketException)
                    {
                        int i = (ee as SocketException).ErrorCode;
                    }
                }

                throw ex;
            }

            return true;
        }
        #endregion

        //#region 网络报文解析
        //List<ICommunicationMessage> extractMessages(byte[] package, ref byte[] rest)
        //{
        //    List<ICommunicationMessage> ar = new List<CommandMsg>();

        //    rest = new byte[0];
        //    int packagePos = 0;
        //    while (true)
        //    {
        //        if (packagePos == package.Length)
        //            return ar;

        //        int content_len;
        //        if (package.Length - packagePos < sizeof(int))
        //        {
        //            rest = new byte[package.Length - packagePos];
        //            for (int i = packagePos; i < package.Length; ++i)
        //                rest[i - packagePos] = package[i];
        //            return ar;
        //        }
        //        else
        //            content_len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(package, packagePos));

        //        if (content_len > 100000000)
        //            throw new Exception("报文过长.");

        //        if (package.Length < packagePos + content_len + sizeof(int))
        //        { // 包不完整, 等待下次读取
        //            //if (package.Length > packagePos + sizeof(int))
        //            //{
        //            //    // 检查报文的有效性
        //            //    string s = Encoding.Default.GetString(package, packagePos + sizeof(int), package.Length - packagePos - sizeof(int));
        //            //    for (int i = 0; i < s.Length && i < Constants.MSG_START_ID.Length; ++i)
        //            //        if (s[i] != Constants.MSG_START_ID[i])
        //            //            throw new Exception("无效报文头.");
        //            //}

        //            rest = new byte[package.Length - packagePos];
        //            for (int i = packagePos; i < package.Length; ++i)
        //                rest[i - packagePos] = package[i];
        //            return ar;
        //        }

        //        string sTemp;
        //        if (m_bCompress)
        //        {
        //            using (MemoryStream ms = new MemoryStream(package, packagePos + sizeof(int), content_len))
        //            {
        //                using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
        //                {
        //                    using (MemoryStream outstream = new MemoryStream())
        //                    {
        //                        byte[] tempbuf = new byte[2048];
        //                        while (true)
        //                        {
        //                            int read = gs.Read(tempbuf, 0, 2048);
        //                            if (read == 0)
        //                                break;

        //                            outstream.Write(tempbuf, 0, read);
        //                        }

        //                        sTemp = Encoding.Default.GetString(outstream.GetBuffer(), 0, Convert.ToInt32(outstream.Length));
        //                    }
        //                }
        //            }
        //        }
        //        else
        //            sTemp = System.Text.Encoding.Default.GetString(package, packagePos + sizeof(int), content_len);

        //        packagePos += sizeof(int) + content_len;

        //        if (!sTemp.StartsWith(Constants.MSG_START_ID) || !sTemp.EndsWith(Constants.MSG_END_ID))
        //            throw new Exception("无效报文标识.");

        //        int indexBegin, indexEnd;

        //        int startPos = 0;
        //        while (true)
        //        {
        //            if (sTemp.Length == startPos)
        //                break;

        //            while (true)
        //            { // 如果内容含有标识头，服务器侧会用双标识头替换
        //                indexBegin = sTemp.IndexOf(Constants.MSG_START_ID, startPos);
        //                if (indexBegin == -1)
        //                    throw new Exception("无效报文.");

        //                if (sTemp.Length - indexBegin < Constants.MSG_DOUBLE_START_ID.Length)
        //                    break; // 长度不足容纳双标识头

        //                if (sTemp.Substring(indexBegin + Constants.MSG_START_ID.Length, Constants.MSG_START_ID.Length) == Constants.MSG_START_ID)
        //                    startPos = indexBegin + 2 * Constants.MSG_START_ID.Length;
        //                else
        //                    break;
        //            }

        //            while (true)
        //            {
        //                indexEnd = sTemp.IndexOf(Constants.MSG_END_ID, startPos);
        //                if (indexEnd == -1)
        //                    break;

        //                if (sTemp.Length - indexEnd < Constants.MSG_DOUBLE_END_ID.Length)
        //                    break; // 长度不足容纳双标识尾

        //                if (sTemp.Substring(indexEnd + Constants.MSG_END_ID.Length, Constants.MSG_END_ID.Length) == Constants.MSG_END_ID)
        //                    startPos = indexEnd + 2 * Constants.MSG_END_ID.Length;
        //                else
        //                    break;
        //            }

        //            if (-1 != indexBegin && -1 != indexEnd && indexBegin > indexEnd)
        //            {
        //                startPos = indexEnd + Constants.MSG_END_ID.Length;
        //                continue;
        //            }

        //            if (-1 != indexBegin && -1 != indexEnd && indexBegin < indexEnd)
        //            {
        //                CommandMsg msg = new CommandMsg();

        //                // 仅把内容传如包解码
        //                msg.decode(sTemp.Substring(indexBegin, indexEnd - indexBegin + Constants.MSG_END_ID.Length));
        //                ar.Add(msg);

        //                startPos = indexEnd + Constants.MSG_END_ID.Length;
        //            }
        //            else
        //            {
        //                int peeked = System.Text.Encoding.Default.GetByteCount(sTemp.Substring(0, startPos));

        //                if (peeked < package.Length)
        //                {
        //                    throw new Exception("报文含有无效内容.");
        //                }
        //            }
        //        }
        //    } // end while

        //}
        //#endregion

        void ClientCommunicator_onConnectionBroken(ICommunicator ptr, string broken_info)
        {
            this.endWork();
        }

        void SendLog(string sLog)
        {
            LogHandler temp;
            lock (m_LockLogHandler)
                temp = _onLog;

            if (temp != null)
                temp(sLog);
        }

        void InvokeConnectionBroken(ICommunicator ptr, string reason)
        {
            ConnectionBrokenHandler temp;
            lock (m_LockConnectionBroken)
                temp = _onConnectionBroken;

            if (temp != null)
                temp(ptr, reason);
        }

        string DumpByte2Str(byte[] buf)
        {
            string s = "";
            for (int i = 0; i < buf.Length; ++i)
            {
                s += string.Format("{0:X2} ", buf[i]);
            }

            return s;
        }
    }
}
