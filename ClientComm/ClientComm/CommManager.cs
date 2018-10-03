using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace TK_AlarmManagement
{
    public class CommManager : TK_AlarmManagement.ICommManager
    {
        #region Factory&&Singleton Implementation
        private static object m_singltonlock = new int();
        private static CommManager m_instance = null;

        public static CommManager instance()
        {
            lock (m_singltonlock)
            {
                if (m_instance == null)
                    m_instance = new CommManager();
            }

            return m_instance;
        }

        static private long m_NextClientID = 0;
        private SortedList<string, ICommServer> m_CommServers = null;
        private List<ICommClient> m_CommClients = null;
        protected CommManager()
        {
            m_CommServers = new SortedList<string, ICommServer>();
            m_CommClients = new List<ICommClient>();

            LogReceived += new LogHandler(CommManager_LogReceived);
        }

        public CommServer<T, MSGTYPE, EXTRACTOR, ENCODER> CreateCommServer<T, MSGTYPE, EXTRACTOR, ENCODER>(string name, 
            List<Constants.TK_CommandType> acceptedcommands, 
            List<Constants.TK_CommandType> supercmds, 
            int port, int maxclients, int synccmd_timeout, bool keepalive, bool compress)
            where T : ICommandInterpreter, new()
            where MSGTYPE : ICommunicationMessage, new()
            where EXTRACTOR : IMessageExtractor, new()
            where ENCODER : IMessageEncoder, new()
        {
            CommServer<T, MSGTYPE, EXTRACTOR, ENCODER> server = new CommServer<T, MSGTYPE, EXTRACTOR, ENCODER>();
            server.Init(name, acceptedcommands, supercmds, port, maxclients, synccmd_timeout, keepalive, compress);

            lock (m_CommServers)
            {
                if (m_CommServers.ContainsKey(name))
                    throw new Exception(string.Format("CommServer:{{{0}}} already exists.", name));

                m_CommServers.Add(name, server);
            }

            return server;
        }

        public CommClient<MSGTYPE> CreateCommClient<MSGTYPE, EXTRACTOR, ENCODER>(string name, string ip, int port, 
            int synccmd_timeout, bool keepalive, bool compress)
            where MSGTYPE : ICommunicationMessage, new()
            where EXTRACTOR : IMessageExtractor, new()
            where ENCODER : IMessageEncoder, new()
        {
            CommClient<MSGTYPE> client = new CommClient<MSGTYPE>();
            IMessageExtractor extractor = new EXTRACTOR();
            extractor.Compressed = compress;

            IMessageEncoder encoder = new ENCODER();
            client.Init(name, ip, port, synccmd_timeout, keepalive, extractor, encoder, compress);

            lock (m_CommClients)
                m_CommClients.Add(client);

            return client;
        }

        public void DisposeServer(string name)
        {
            try
            {
                lock (m_CommServers)
                {
                    if (m_CommServers.ContainsKey(name))
                    {
                        m_CommServers[name].Close();
                        m_CommServers.Remove(name);
                    }
                }
            }
            catch (Exception ex)
            {
                LogReceived(ex.ToString());
            }
        }

        public void DisposeClient(ICommClient client)
        {
            try
            {
                lock (m_CommClients)
                {
                    if (m_CommClients.Contains(client))
                    {
                        client.Close();
                        m_CommClients.Remove(client);
                    }
                }
            }
            catch (Exception ex)
            {
                LogReceived(ex.ToString());
            }
        }
        #endregion

        #region 公共事件
        public event LogHandler LogReceived;
        #endregion

        #region 私有方法
        void CommManager_LogReceived(string sLog)
        {
            // do nothing
        }
        #endregion

        #region 公共接口，Start、Stop
        /// <summary>
        /// 启动TCP服务器，并建立和CommandProcessor的订阅关系
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            lock (this)
            {
                try
                {
                    lock (m_CommServers)
                    {
                        foreach (KeyValuePair<string, ICommServer> server in m_CommServers)
                            server.Value.Start();
                    }

                    lock (m_CommClients)
                    {
                        foreach (ICommClient client in m_CommClients)
                            client.Start();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return true;
        }

        public bool Stop()
        {
            lock (this)
            {
                try
                {
                    lock (m_CommClients)
                    {
                        foreach (ICommClient client in m_CommClients)
                            client.Close();
                    }

                    lock (m_CommServers)
                    {
                        foreach (KeyValuePair<string, ICommServer> server in m_CommServers)
                            server.Value.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return true;
        }

        static public long AllocateClientID()
        {
            return Interlocked.Increment(ref m_NextClientID);
        }
        #endregion

        public ICommServer GetServer(string name)
        {
            lock (m_CommServers)
            {
                if (m_CommServers.ContainsKey(name))
                    return m_CommServers[name];
                else
                    return null;
            }
        }
    }
}
