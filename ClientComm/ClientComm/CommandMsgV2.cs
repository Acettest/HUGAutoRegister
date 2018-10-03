using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TK_AlarmManagement
{
    [Serializable]
    public class CommandMsgV2 : CommandMsg 
    {
        protected string m_EncodeModeV2 = "iso-8859-1";

        public CommandMsgV2()
        {
            EncodeMode = "iso-8859-1";
            TK_CommandType = 0;
        }

        public override string EncodeMode
        {
            get { return m_EncodeModeV2; }
            set { m_EncodeModeV2 = value; }
        }

        public override string encode()
        {
            return Encoding.GetEncoding(EncodeMode).GetString(ToNetBuf());
        }

        public override bool decode(string msg)
        {
            throw new Exception("Obsolete Method");
        }

        /// <summary>
        /// 使用new关键字，允许外部通过基类类型引用调用基类方法
        /// </summary>
        /// <returns></returns>
        public override byte[] ToNetBuf()
        {
            byte[] buf;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, this);

                buf = new byte[ms.Length];
                Array.Copy(ms.GetBuffer(), buf, ms.Length);
            }

            return buf;
        }

        /// <summary>
        /// 使用new关键字，允许外部通过基类类型引用调用基类方法
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public override bool FromNetBuf(byte[] buf)
        {
            throw new Exception("CommandMsgV2不支持FromNetBuf");
        }

        public override ICommunicationMessage clone()
        {
            CommandMsgV2 cm = new CommandMsgV2();
            cm.m_EncodeMode = EncodeMode;
            cm.m_SeqID = SeqID;
            cm.m_Type = TK_CommandType;

            lock (m_SyncRoot)
            {
                foreach (C5.KeyValuePair<string, object> de in m_Content)
                {
                    cm.SetValue(de.Key, de.Value);
                }
            }

            return cm;
        }
    }
}
