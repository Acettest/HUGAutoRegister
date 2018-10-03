using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace TK_AlarmManagement
{
    //public delegate void LogHandler(string sLog);
    public delegate void ClientConnectionBrokenHandler(ICommClient ptr, string reason);

    public interface ICommer : ILogHandler
    {
        string Name { get; }
        IPAddress LocalIP { get;}
        int LocalPort { get; }

        long RunFlag
        {
            get;
        }

        /// <summary>
        /// 开始通讯侦听
        /// </summary>
        bool Start();

        /// <summary>
        /// 结束通讯
        /// </summary>
        bool Close();

        int FilterResponse(List<ICommunicationMessage> income_msgs);
    }

    public interface ICommServer : ICommer
    {
        event ConnectionBrokenHandler onClientConnectionBroken;

        /// <summary>
        /// 初始化，传入服务器地址
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void Init(string name, List<Constants.TK_CommandType> accepted_msgs, List<Constants.TK_CommandType> super_cmd,
            int port, int maxclients, int send_timeout, bool keepalive, bool compress);

        /// <summary>
        /// 向服务器发送信息，返回参数为响应包
        /// </summary>
        /// <param name="clientid">客户端编号</param>
        /// <param name="command"></param>
        /// <param timeout="timeout">以秒为单位</param>
        /// <returns></returns>
        ICommunicationMessage SendCommand(long clientid, ICommunicationMessage command);
        ICommunicationMessage SendCommand(long clientid, ICommunicationMessage command, int timeout);

        /// <summary>
        /// 向客户端发送命令，不需要响应
        /// </summary>
        /// <param name="command"></param>
        void PostCommand(long clientid, ICommunicationMessage command);    

    }

    public interface ICommClient : ICommer
    {
        /// <summary>
        /// 初始化，传入服务器地址
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void Init(string name, string ip, int port, int send_timeout, bool keepalive, 
            IMessageExtractor extractor, IMessageEncoder encoder,
            bool compress);

        long ClientID { get; }

        IPAddress RemoteIP
        {
            get;
        }

        int RemotePort
        {
            get;
        }

        event ClientConnectionBrokenHandler onConnectionBroken;

        /// <summary>
        /// 向服务器发送信息，返回参数为响应包
        /// </summary>
        /// <param name="command"></param>
        /// <param timeout="timeout">以秒为单位</param>
        /// <returns></returns>
        ICommunicationMessage SendCommand(ICommunicationMessage command);
        ICommunicationMessage SendCommand(ICommunicationMessage command, int timeout);

        /// <summary>
        /// 向服务器发送命令，不需要响应
        /// </summary>
        /// <param name="command"></param>
        void PostCommand(ICommunicationMessage command);    

    }
}
