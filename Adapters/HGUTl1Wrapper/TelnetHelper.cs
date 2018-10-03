using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace TelnetLib
{
    class TelnetHelper
    {
        #region 枚举参数
        enum Verbs
        {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }

        enum Options
        {
            SGA = 3
        }

        #endregion

        #region IFields
        private TcpClient tcpSocket;

        private int timeOutMs = 100;

        public Encoding omcEncoding;
        #endregion

        #region IConstructors
        /// <summary>
        /// 新建Telnet连接(默认端口:23)
        /// </summary>
        /// <param name="ip">IP地址</param>
        public TelnetHelper(string ip)
        {
            tcpSocket = new TcpClient(ip, 23);
        }

        /// <summary>
        /// 新建Telnet连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="Port">端口号</param>
        public TelnetHelper(string Hostname, int Port)
        {
            tcpSocket = new TcpClient(Hostname, Port);
        }
        #endregion

        #region Telnet方法
        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        public void Close()
        {
            tcpSocket.Client.Shutdown(SocketShutdown.Both);
            tcpSocket.Client.Close();
            tcpSocket.Close();
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="command">命令文本</param>
        public void Write(string command)
        {
            if (!tcpSocket.Connected) return;
            byte[] buf = omcEncoding != null ? omcEncoding.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF")) : System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="command">命令文本</param>
        public void WriteLine(string command)
        {
            command += "\n";
            if (!tcpSocket.Connected) return;
            byte[] buf = omcEncoding != null ? omcEncoding.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF")) : System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        private string Read()
        {
            if (!tcpSocket.Connected) return null;
            StringBuilder sb = new StringBuilder();
            do
            {
                ParseTelnet(sb);
                System.Threading.Thread.Sleep(timeOutMs);
            } while (tcpSocket.Available > 0);
            return sb.ToString();
        }

        private void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }

        /// <summary>
        /// 等待回应
        /// </summary>
        /// <param name="flag">命令行参数</param>
        /// <param name="timeout">等待时延(秒)</param>
        /// <returns></returns>
        public string WaitFor(string flag, int timeout)
        {
            string result = "";
            int i = 0;
            while (i++ < timeout)
            {
                result += Read();
                if (result.IndexOf(flag) != -1)
                    break;
                System.Threading.Thread.Sleep(1000);
            }
            return result;
        }
        #endregion
    }
}
