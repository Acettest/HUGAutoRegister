using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace TK_AlarmManagement
{
    public class TKMessageV2Encoder : IMessageEncoder
    {
        #region IMessageEncoder 成员
        public byte[] encodeMessages(List<ICommunicationMessage> msgs, bool compress)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, msgs);
                return _MakeHeader(ms.GetBuffer(), compress);
            }
        }

        public byte[] encodeMessage(ICommunicationMessage msg, bool compress)
        {
            return _MakeHeader(((CommandMsgV2)msg).ToNetBuf(), compress);
        }

        public byte[] encodeMessage(string message, Encoding encode, bool compress)
        {
            byte[] buf = encode.GetBytes(message);

            return _MakeHeader(buf, compress);
        }

        byte[] _MakeHeader(byte[] buf, bool compress)
        {
            byte[] newbuf;

            if (compress)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    GZipStream gs = new GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true);
                    gs.Write(buf, 0, buf.Length);
                    gs.Close();

                    newbuf = new byte[ms.Length + 4];
                    byte[] len = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(ms.Length)));
                    len.CopyTo(newbuf, 0);
                    Array.Copy(ms.GetBuffer(), 0, newbuf, 4, ms.Length);
                }
            }
            else
            {
                int l = buf.Length;
                newbuf = new byte[buf.Length + 4];
                byte[] len = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(l));
                len.CopyTo(newbuf, 0);
                buf.CopyTo(newbuf, 4);
            }

            return newbuf;
        }
        #endregion
    }
}
