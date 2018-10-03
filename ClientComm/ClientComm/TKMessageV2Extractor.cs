using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace TK_AlarmManagement
{
    public class TKMessageV2Extractor : IMessageExtractor
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

                object content;
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

                                outstream.Position = 0;
                                BinaryFormatter bf = new BinaryFormatter();
                                content = bf.Deserialize(outstream);
                            }
                        }
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream(package, packagePos + sizeof(int), content_len))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        content = bf.Deserialize(ms);
                    }
                }

                packagePos += sizeof(int) + content_len;

                if (content is List<ICommunicationMessage>)
                {
                    ar.AddRange(content as List<ICommunicationMessage>);
                }
                else if (content is CommandMsgV2)
                {
                    ar.Add(content as CommandMsgV2);
                }
                else
                {
                    throw new Exception("无法识别的报文内容.");
                }
            } // end while

        }
    }
}
