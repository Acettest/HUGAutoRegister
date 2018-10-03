using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace TK_AlarmManagement
{
    public class TKMessageEncoder : IMessageEncoder
    {
        #region IMessageEncoder 成员
        public byte[] encodeMessages(List<ICommunicationMessage> msgs, bool compress)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (ICommunicationMessage msg in msgs)
                {
                    byte[] buf = ((CommandMsg)msg).ToNetBuf();
                    ms.Write(buf, 0, buf.Length);
                }

                byte[] newbuf = new byte[ms.Length];
                Array.Copy(ms.GetBuffer(), newbuf, ms.Length);

                return _MakeHeader(newbuf, compress);
            }
        }

        public byte[] encodeMessage(ICommunicationMessage msg, bool compress)
        {
            return _MakeHeader(((CommandMsg)msg).ToNetBuf(), compress);
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
