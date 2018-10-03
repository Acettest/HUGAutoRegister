using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Runtime.Serialization;

namespace TK_AlarmManagement
{
	/// <summary>
	/// 命令包类
	/// </summary>
    /// 
    [Serializable]
	public class CommandMsg: ICommunicationMessage
	{
		protected long m_SeqID = 0;
        protected Constants.TK_CommandType m_Type;
        protected C5.HashDictionary<string, object> m_Content = new C5.HashDictionary<string, object>();
        protected string m_EncodeMode = "GB2312";

        protected object m_SyncRoot = new object();

		public CommandMsg()
		{
		}

        #region ICommunicationMessage 成员

        virtual public string EncodeMode
        {
            get { return m_EncodeMode; }
            set { m_EncodeMode = value; }
        }

        virtual public TK_AlarmManagement.Constants.TK_CommandType TK_CommandType
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

        virtual public long SeqID
		{
			get
			{
				return m_SeqID;
			}
			set
			{
				m_SeqID = value;
			}
		}

        virtual public string encode()
		{
            StringBuilder package = new StringBuilder(4096);
            package.Append(Constants.MSG_START_ID);

            StringBuilder content = new StringBuilder(4096);

            // 首先是报文流水号
            content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_SEQUENCE_ID, Constants.MSG_NV_TERMINATOR, m_SeqID.ToString(), Constants.MSG_LINE_TERMINATOR);

			// 报文类型
			switch (m_Type)
			{
					//TODO: dpwang 07/08/10 添加新类型
				case Constants.TK_CommandType.ALARM_REPORT:
					content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_REPORT, Constants.MSG_LINE_TERMINATOR);
					break;
				case Constants.TK_CommandType.RESPONSE:
					content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_RESPONSE, Constants.MSG_LINE_TERMINATOR);
					break;
                case Constants.TK_CommandType.LOGIN:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_LOGIN, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ALARM_ACK:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_ACK, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ALARM_REACK:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_REACK, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ALARM_ACK_CHANGE:
					content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_ACK_CHANGE, Constants.MSG_LINE_TERMINATOR);
					break;
				case Constants.TK_CommandType.ALARM_ORDER_CHANGE:
					content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_ORDER_CHANGE, Constants.MSG_LINE_TERMINATOR);
					break;
                case Constants.TK_CommandType.KEEPALIVE:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_KEEPALIVE, Constants.MSG_LINE_TERMINATOR); 
                    break;
                case Constants.TK_CommandType.ADAPTER_LOGIN:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_LOGIN, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_LOGOUT:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_LOGOUT, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_STATE_REPORT:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_STATE_REPORT, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_ALARM_REPORT:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_ALARM_REPORT, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_START:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_START, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_STOP:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_STOP, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ALLOCATE_TKSN:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALLOCATE_TKSN, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_GETRUNTIMEINFO:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_GETRUNTIMEINFO, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_GETOMCLIST:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_GETOMCLIST, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_GETLOGFILES:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_GETLOGFILES, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_GETCURLOG:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_GETCURLOG, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ADAPTER_SHUTDOWN:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ADAPTER_SHUTDOWN, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.SERVER_GETRUNTIMEINFO:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_SERVER_GETRUNTIMEINFO, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.SERVER_GETLOGFILES:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_SERVER_GETLOGFILES, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.SERVER_GETCURLOG:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_SERVER_GETCURLOG, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.PROJECT_ADD:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_PROJECT_ADD, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.PROJECT_MODIFY:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_PROJECT_MODIFY, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.PROJECT_REMOVE:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_PROJECT_REMOVE, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ALARM_PROJECT_CHANGE:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_PROJECT_CHANGE, Constants.MSG_LINE_TERMINATOR);
                    break;

                case Constants.TK_CommandType.ALARM_HANG_UP:          //告警挂起
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_HANG_UP, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ALARM_HANG_DOWN:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_HANG_DOWN, Constants.MSG_LINE_TERMINATOR);
                    break;
                case Constants.TK_CommandType.ALARM_HANG_CHANGE:
                    content.AppendFormat("{0}{1}{2}{3}", Constants.MSG_PARANAME_COMMAND, Constants.MSG_NV_TERMINATOR, Constants.MSG_TYPE_ALARM_HANG_CHANGE, Constants.MSG_LINE_TERMINATOR);
                    break;
                default:
                    throw new Exception("封包时遇到未知的命令类型:" + m_Type);
			}

            try
            {
                lock (m_SyncRoot)
                {
                    foreach (C5.KeyValuePair<string, object> de in m_Content)
                    {
                        if (de.Key.ToString() == Constants.MSG_PARANAME_COMMAND ||
                            de.Key.ToString() == Constants.MSG_PARANAME_SEQUENCE_ID)
                            continue;

                        content.AppendFormat("{0}{1}{2}{3}", de.Key.ToString(),
                            Constants.MSG_NV_TERMINATOR, 
                            de.Value.ToString().Replace(Constants.MSG_LINE_TERMINATOR, Constants.MSG_DOUBLE_LINE_TERMINATOR),
                            Constants.MSG_LINE_TERMINATOR);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            content = content.Replace(Constants.MSG_START_ID, Constants.MSG_DOUBLE_START_ID);
            content = content.Replace(Constants.MSG_END_ID, Constants.MSG_DOUBLE_END_ID);

            package.Append(content);
			package.Append(Constants.MSG_END_ID);

			return package.ToString();
		}

        virtual public bool decode(string msg)
		{
            string s = msg.Substring(Constants.MSG_START_ID.Length, msg.Length - Constants.MSG_START_ID.Length - Constants.MSG_END_ID.Length);

            s = msg.Replace(Constants.MSG_DOUBLE_START_ID, Constants.MSG_START_ID);
            s = s.Replace(Constants.MSG_DOUBLE_END_ID, Constants.MSG_END_ID);

			bool cont = true;

            lock (m_SyncRoot)
            {
                m_Content.Clear();
                while (cont)
                {
                    string ln;

                    int i = 0;

                    int startpos = 0;
                    while (true)
                    {
                        i = s.IndexOf(Constants.MSG_LINE_TERMINATOR, startpos);
                        if (i == -1)
                            break;

                        if (s.Length - i < Constants.MSG_DOUBLE_LINE_TERMINATOR.Length)
                            break;

                        // 如果是双行结束符，表明是做过转义
                        if (s.Substring(i + Constants.MSG_LINE_TERMINATOR.Length, Constants.MSG_LINE_TERMINATOR.Length) == Constants.MSG_LINE_TERMINATOR)
                            startpos = i + Constants.MSG_DOUBLE_LINE_TERMINATOR.Length;
                        else
                            break;
                    }

                    if (i == -1)
                    {
                        ln = s;
                        cont = false;
                        break;
                    }
                    else
                    {
                        ln = s.Substring(0, i);
                        s = s.Substring(i + Constants.MSG_LINE_TERMINATOR.Length);
                    }

                    int index = ln.IndexOf(Constants.MSG_NV_TERMINATOR);
                    //				if (index == -1)
                    //					return false; // 每一行都必须由 name = value的形式构成，找不到分隔符则中断解析
                    if (index == -1)
                        continue;		//解析失败，继续解析下一行

                    string name = ln.Substring(0, index).Trim();
                    StringBuilder val = new StringBuilder(ln.Substring(index + Constants.MSG_NV_TERMINATOR.Length));
                    //string val = ln.Substring(index + Constants.MSG_NV_TERMINATOR.Length);
                    m_Content[name] = val.Replace(Constants.MSG_DOUBLE_LINE_TERMINATOR, Constants.MSG_LINE_TERMINATOR);

                    if (name == Constants.MSG_PARANAME_COMMAND)
                    {
                        string value = val.ToString();
                        switch (value)
                        {
                            case Constants.MSG_TYPE_LOGIN:
                                this.m_Type = Constants.TK_CommandType.LOGIN;
                                break;
                            case Constants.MSG_TYPE_LOGOUT:
                                this.m_Type = Constants.TK_CommandType.LOGOUT;
                                break;
                            case Constants.MSG_TYPE_PROJECT_ADD:
                                this.m_Type = Constants.TK_CommandType.PROJECT_ADD;
                                break;
                            case Constants.MSG_TYPE_PROJECT_MODIFY:
                                this.m_Type = Constants.TK_CommandType.PROJECT_MODIFY;
                                break;
                            case Constants.MSG_TYPE_PROJECT_REMOVE:
                                this.m_Type = Constants.TK_CommandType.PROJECT_REMOVE;
                                break;
                            case Constants.MSG_TYPE_ALARM_REPORT:
                                this.m_Type = Constants.TK_CommandType.ALARM_REPORT;
                                break;
                            case Constants.MSG_TYPE_RESPONSE:
                                this.m_Type = Constants.TK_CommandType.RESPONSE;
                                break;
                            case Constants.MSG_TYPE_KEEPALIVE:
                                this.m_Type = Constants.TK_CommandType.KEEPALIVE;
                                break;
                            case Constants.MSG_TYPE_ALARM_ACK:
                                this.m_Type = Constants.TK_CommandType.ALARM_ACK;
                                break;
                            case Constants.MSG_TYPE_ALARM_REACK:
                                this.m_Type = Constants.TK_CommandType.ALARM_REACK;
                                break;
                            case Constants.MSG_TYPE_SENDORDER:
                                this.m_Type = Constants.TK_CommandType.SENDORDER;
                                break;
                            case Constants.MSG_TYPE_ALARM_ACK_CHANGE:
                                m_Type = Constants.TK_CommandType.ALARM_ACK_CHANGE;
                                break;
                            case Constants.MSG_TYPE_ALARM_ORDER_CHANGE:
                                m_Type = Constants.TK_CommandType.ALARM_ORDER_CHANGE;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_LOGIN:
                                m_Type = Constants.TK_CommandType.ADAPTER_LOGIN;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_LOGOUT:
                                m_Type = Constants.TK_CommandType.ADAPTER_LOGOUT;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_STATE_REPORT:
                                m_Type = Constants.TK_CommandType.ADAPTER_STATE_REPORT;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_ALARM_REPORT:
                                m_Type = Constants.TK_CommandType.ADAPTER_ALARM_REPORT;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_START:
                                m_Type = Constants.TK_CommandType.ADAPTER_START;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_STOP:
                                m_Type = Constants.TK_CommandType.ADAPTER_STOP;
                                break;
                            case Constants.MSG_TYPE_ALLOCATE_TKSN:
                                m_Type = Constants.TK_CommandType.ALLOCATE_TKSN;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_GETCURLOG:
                                m_Type = Constants.TK_CommandType.ADAPTER_GETCURLOG;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_GETLOGFILES:
                                m_Type = Constants.TK_CommandType.ADAPTER_GETLOGFILES;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_GETOMCLIST:
                                m_Type = Constants.TK_CommandType.ADAPTER_GETOMCLIST;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_GETRUNTIMEINFO:
                                m_Type = Constants.TK_CommandType.ADAPTER_GETRUNTIMEINFO;
                                break;
                            case Constants.MSG_TYPE_ADAPTER_SHUTDOWN:
                                m_Type = Constants.TK_CommandType.ADAPTER_SHUTDOWN;
                                break;
                            case Constants.MSG_TYPE_SERVER_GETCURLOG:
                                m_Type = Constants.TK_CommandType.SERVER_GETCURLOG;
                                break;
                            case Constants.MSG_TYPE_SERVER_GETLOGFILES:
                                m_Type = Constants.TK_CommandType.SERVER_GETLOGFILES;
                                break;
                            case Constants.MSG_TYPE_SERVER_GETRUNTIMEINFO:
                                m_Type = Constants.TK_CommandType.SERVER_GETRUNTIMEINFO;
                                break;
                            case Constants.MSG_TYPE_ALARM_PROJECT_CHANGE:
                                m_Type = Constants.TK_CommandType.ALARM_PROJECT_CHANGE;
                                break;

                            case Constants.MSG_TYPE_ALARM_HANG_UP:            //告警挂起
                                m_Type = Constants.TK_CommandType.ALARM_HANG_UP;
                                break;
                            case Constants.MSG_TYPE_ALARM_HANG_DOWN:
                                m_Type = Constants.TK_CommandType.ALARM_HANG_DOWN;
                                break;
                            case Constants.MSG_TYPE_ALARM_HANG_CHANGE:
                                m_Type = Constants.TK_CommandType.ALARM_HANG_CHANGE;
                                break;
                            default:
                                throw new Exception("解包时遇到未知的命令类型:" + value);
                        }
                        //this.m_Type = (Constants.TK_CommandType)(Int32.Parse(val));
                    }
                    else if (name == Constants.MSG_PARANAME_SEQUENCE_ID)
                    {
                        m_SeqID = Int64.Parse(val.ToString());
                    }
                } //  end while
            }

			return true;
		}

        virtual public byte[] ToNetBuf()
        {
            return Encoding.GetEncoding(EncodeMode).GetBytes(encode());
        }

        virtual public bool FromNetBuf(byte[] buf)
        {
            return decode(Encoding.GetEncoding(EncodeMode).GetString(buf));
        }

        virtual public ICommunicationMessage clone()
        {
            CommandMsg cm = new CommandMsg();
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
		#endregion

        #region ICommunicationMessage 成员


        public void SetValue(string key, object value)
        {
            lock (m_SyncRoot)
                m_Content[key] = value;
        }

        public object GetValue(string key)
        {
            lock (m_SyncRoot)
            {
                if (!m_Content.Contains(key))
                    throw new Exception("CommandMsg中不包含指定键: " + key);
                else
                    return m_Content[key];
            }
        }

        public bool RemoveKey(string key)
        {
            lock (m_SyncRoot)
                return m_Content.Remove(key);
        }

        public bool Contains(string key)
        {
            lock (m_SyncRoot)
                return m_Content.Contains(key);
        }

        public C5.ICollectionValue<string> GetKeys()
        {
            lock (m_SyncRoot)
                return m_Content.Keys;
        }
        #endregion
    }
}
