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
    public class AsyncCommunicator : ICommunicator
    {
        #region 构造函数
        /// <summary>
        /// ClientCommunicator构造函数
        /// </summary>
        /// <param name="clientid">客户端ID</param>
        /// <param name="client">客户端的TCP实体</param>
        /// <param name="filter">报文过滤器</param>
        public AsyncCommunicator(ICommer parent_commer, long clientid, Socket client, ICommandInterpreter intepreter, 
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
            onConnectionBroken += new ConnectionBrokenHandler(_HandleSelfConnectionBroken);
        }
        #endregion

        #region 私有成员
        private ICommer m_ParentCommer = null;
        private Socket m_Client = null;
        private long m_ClientID = -1;
        private IPEndPoint m_ClientInfo = null;

        private ICommandInterpreter m_Interpreter = null;

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

            //ThreadPool.QueueUserWorkItem(new WaitCallback(_worker));
            //ThreadPool.QueueUserWorkItem(new WaitCallback(_listener));
            //Thread thr = new Thread(new ThreadStart(_worker));
            //thr.Start();

            //Thread thr2 = new Thread(new ThreadStart(_listener));
            //thr2.Start();

            if (m_Interpreter != null)
                m_Interpreter.Start();

            _recvAsync();
        }

        public void endWork()
        {
            if (Interlocked.Exchange(ref m_Run, 0) == 0)
                return;

            if (m_Interpreter != null)
                m_Interpreter.Stop();

            try
            {
                if (m_Client != null)
                {
                    lock (m_Client)
                    {
                        m_Client.Shutdown(SocketShutdown.Both);
                        m_Client.Close();
                    }
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
        }
        #endregion

        #region 入队列操作
        public void enqueueMessage(ICommunicationMessage msg)
        {
            ICommunicationMessage parsed = null;
            ICommunicationMessage tobesent = null;
            if (m_Interpreter != null)
            {
                parsed = m_Interpreter.ImmediatelyInterpret(msg);

                if (parsed == null)
                {
                    if (m_Interpreter.DelayedInterpret(msg) == true)
                        return;
                    else
                    {
                        tobesent = msg;
                        //lock (m_Messages)
                        //    m_Messages.Enqueue(msg);
                    }
                }
                else
                {
                    tobesent = parsed;
                    //lock (m_Messages)
                    //    m_Messages.Enqueue(parsed);
                }
            }
            else
            {
                tobesent = msg;
                //lock (m_Messages)
                //    m_Messages.Enqueue(msg);
            }

            if (tobesent != null)
            {
                tobesent.SetValue("LAST_SEQ_ID", m_last_seq_id);
                m_last_seq_id = tobesent.SeqID;

                _sendMessage(tobesent);
            }

            //m_SignalNewMessage.Set();
        }

        public void enqueueDelayedMessages(ICommunicationMessage msg)
        {
            msg.SetValue("LAST_SEQ_ID", m_last_seq_id);
            m_last_seq_id = msg.SeqID;

            _sendMessage(msg);
            //lock (m_DelayedMessages)
            //    m_DelayedMessages.Enqueue(msg);

            //m_SignalNewMessage.Set();
        }

        #endregion

        #region 发送与侦听
        private long m_last_seq_id = 0;

        byte[] buf = new byte[8192];
        byte[] rest = null;
        DefLib.Util.ArrayBuilder<byte> totalBuf = new DefLib.Util.ArrayBuilder<byte>();

        ManualResetEvent m_RecvDone = new ManualResetEvent(false);
        private void _recvAsync()
        {
            try
            {
                m_RecvDone.Reset();
                IAsyncResult ar = m_Client.BeginReceive(buf, 0, buf.Length, SocketFlags.None, _recvCallback, null);
                //m_RecvDone.WaitOne();
            }
            catch (Exception ex)
            {
                InvokeConnectionBroken(this, ex.Message);
            }
        }

        volatile static int m_RecvCount = 0;
        private void _recvCallback(IAsyncResult ar)
        {
            SocketError err_code;

            try
            {
                int buflen = m_Client.EndReceive(ar, out err_code);
                m_RecvDone.Set();

                if (err_code != SocketError.Success)
                {
                    InvokeConnectionBroken(this, new SocketException((int)err_code).Message);
                    return;
                }

                if (buflen == 0)
                {
                    InvokeConnectionBroken(this, "收到零字节数据.");
                    return;
                }

                totalBuf.Clear();
                totalBuf.Append(buf, buflen);
                if (rest != null)
                {
                    totalBuf.Insert(0, rest, rest.Length);
                    rest = null;
                }

                if (m_Extractor == null)
                {
                    totalBuf.Clear();
                    return;
                }

                //SendLog(totalBuf.Count + " Bytes recved: " + DumpByte2Str(totalBuf.ToArray()));

                // 解析收到的数据
                List<ICommunicationMessage> messages = m_Extractor.extractMessages(totalBuf.ToArray(), ref rest);

                // 派发给侦听者
                if (messages.Count > 0)
                {
                    ++m_RecvCount;
                    //DefLib.Util.Logger.Instance().SendLog("AsyncComm", "Received[" + ClientID + "]: " + m_RecvCount);

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
                    {
                        CommandProcessor.instance().DispatchCommands(queue);
                    }
                } // end if messages.Count

                if (Interlocked.Read(ref m_Run) != 0)
                {
                    _recvAsync();
                }
            }
            catch (Exception ex)
            {
                m_RecvDone.Set();
                InvokeConnectionBroken(this, ex.ToString());
            }
            finally
            {
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

        ManualResetEvent m_SendDone = new ManualResetEvent(false);
        private bool _sendMessage(byte[] newbuf)
        {
            try
            {
                lock (m_Client)
                {
                    if (m_Client.Connected)
                    {
                        m_SendDone.Reset();
                        IAsyncResult ar = m_Client.BeginSend(newbuf, 0, newbuf.Length, SocketFlags.None, _sendCallback, m_Client);
                        //m_SendDone.WaitOne();
                    }
                }
            }
            catch (Exception ex)
            {
                InvokeConnectionBroken(this, ex.Message);
                return false;
            }

            return true;
        }

        private void _sendCallback(IAsyncResult ar)
        {
            SocketError err_code = SocketError.Success;

            try
            {
                Socket client = ar.AsyncState as Socket;
                int bytesSent = client.EndSend(ar, out err_code);
                m_SendDone.Set();

                if (err_code != SocketError.Success)
                {
                    InvokeConnectionBroken(this, new SocketException((int)err_code).ToString());
                }
            }
            catch (Exception ex)
            {
                DefLib.Util.Logger.Instance().SendLog("AsyncComm", ex.ToString());
            }
        }
        #endregion

        void _HandleSelfConnectionBroken(ICommunicator ptr, string broken_info)
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
            try
            {
                ConnectionBrokenHandler temp;
                lock (m_LockConnectionBroken)
                    temp = _onConnectionBroken;

                if (temp != null)
                    temp(ptr, reason);
            }
            catch { }
        }

        public static string DumpByte2Str(byte[] buf)
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
