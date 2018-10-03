using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace TK_AlarmManagement
{
    public class TKMessageExtractor : IMessageExtractor
    {
        bool m_bCompressed = false;

        public bool Compressed
        {
            get { return m_bCompressed; }
            set { m_bCompressed = value; }
        }

        public List<ICommunicationMessage> extractMessages(byte[] package, ref byte[] rest)
        {
            List<ICommunicationMessage> ar = new List<ICommunicationMessage>();

            rest = new byte[0];
            int packagePos = 0;
            while (true)
            {
                if (packagePos == package.Length)
                    return ar;

                int content_len;
                if (package.Length - packagePos < sizeof(int))
                {
                    rest = new byte[package.Length - packagePos];
                    for (int i = packagePos; i < package.Length; ++i)
                        rest[i - packagePos] = package[i];
                    return ar;
                }
                else
                    content_len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(package, packagePos));

                if (content_len > 100000000)
                    throw new Exception("报文过长.");

                if (package.Length < packagePos + content_len + sizeof(int))
                { // 包不完整, 等待下次读取
                    //if (package.Length > packagePos + sizeof(int))
                    //{
                    //    // 检查报文的有效性
                    //    string s = Encoding.Default.GetString(package, packagePos + sizeof(int), package.Length - packagePos - sizeof(int));
                    //    for (int i = 0; i < s.Length && i < Constants.MSG_START_ID.Length; ++i)
                    //        if (s[i] != Constants.MSG_START_ID[i])
                    //            throw new Exception("无效报文头.");
                    //}

                    rest = new byte[package.Length - packagePos];
                    for (int i = packagePos; i < package.Length; ++i)
                        rest[i - packagePos] = package[i];
                    return ar;
                }

                string sTemp;
                if (m_bCompressed)
                {
                    using (MemoryStream ms = new MemoryStream(package, packagePos + sizeof(int), content_len))
                    {
                        using (GZipStream gs = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            using (MemoryStream outstream = new MemoryStream())
                            {
                                byte[] tempbuf = new byte[2048];
                                while (true)
                                {
                                    int read = gs.Read(tempbuf, 0, 2048);
                                    if (read == 0)
                                        break;

                                    outstream.Write(tempbuf, 0, read);
                                }

                                sTemp = Encoding.Default.GetString(outstream.GetBuffer(), 0, Convert.ToInt32(outstream.Length));
                            }
                        }
                    }
                }
                else
                    sTemp = System.Text.Encoding.Default.GetString(package, packagePos + sizeof(int), content_len);

                packagePos += sizeof(int) + content_len;

                if (!sTemp.StartsWith(Constants.MSG_START_ID) || !sTemp.EndsWith(Constants.MSG_END_ID))
                    throw new Exception("无效报文标识.");

                int indexBegin, indexEnd;

                int startPos = 0;
                while (true)
                {
                    if (sTemp.Length == startPos)
                        break;

                    while (true)
                    { // 如果内容含有标识头，服务器侧会用双标识头替换
                        indexBegin = sTemp.IndexOf(Constants.MSG_START_ID, startPos);
                        if (indexBegin == -1)
                            throw new Exception("无效报文.");

                        if (sTemp.Length - indexBegin < Constants.MSG_DOUBLE_START_ID.Length)
                            break; // 长度不足容纳双标识头

                        if (sTemp.Substring(indexBegin + Constants.MSG_START_ID.Length, Constants.MSG_START_ID.Length) == Constants.MSG_START_ID)
                            startPos = indexBegin + 2 * Constants.MSG_START_ID.Length;
                        else
                            break;
                    }

                    while (true)
                    {
                        indexEnd = sTemp.IndexOf(Constants.MSG_END_ID, startPos);
                        if (indexEnd == -1)
                            break;

                        if (sTemp.Length - indexEnd < Constants.MSG_DOUBLE_END_ID.Length)
                            break; // 长度不足容纳双标识尾

                        if (sTemp.Substring(indexEnd + Constants.MSG_END_ID.Length, Constants.MSG_END_ID.Length) == Constants.MSG_END_ID)
                            startPos = indexEnd + 2 * Constants.MSG_END_ID.Length;
                        else
                            break;
                    }

                    if (-1 != indexBegin && -1 != indexEnd && indexBegin > indexEnd)
                    {
                        startPos = indexEnd + Constants.MSG_END_ID.Length;
                        continue;
                    }

                    if (-1 != indexBegin && -1 != indexEnd && indexBegin < indexEnd)
                    {
                        CommandMsg msg = new CommandMsg();

                        // 仅把内容传如包解码
                        msg.decode(sTemp.Substring(indexBegin, indexEnd - indexBegin + Constants.MSG_END_ID.Length));
                        ar.Add(msg);

                        startPos = indexEnd + Constants.MSG_END_ID.Length;
                    }
                    else
                    {
                        int peeked = System.Text.Encoding.Default.GetByteCount(sTemp.Substring(0, startPos));

                        if (peeked < package.Length)
                        {
                            throw new Exception("报文含有无效内容.");
                        }
                    }
                }
            } // end while

        }
    }
}
