using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace TK_AlarmManagement
{
	/// <summary>
	/// 封装每一个集中客户端的信息
	/// </summary>
    /// 
    [Serializable]
	public class AlarmClient
	{
		private long id = 0;
		private string name = "";
		private string filter = "";
        private bool m_authorized = false;
        private List<CommandMsgV2> m_Messages = new List<CommandMsgV2>();

		/// <summary>
		/// 每个客户端的标识
		/// </summary>
		public long ClientID
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		/// <summary>
		/// 客户端名称
		/// </summary>
		public string ClientName
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		/// <summary>
		/// 告警过滤条件
		/// </summary>
		public string AlarmFilter
		{
			get
			{
				return filter;
			}
			set
			{
				filter = value;
			}
		}

        public bool Authorized
        {
            get { return m_authorized; }
            set { m_authorized = value; }
        }

		public AlarmClient()
		{
		}

		/// <summary>
		/// 重载的构造函数
		/// </summary>
		/// <param name="nid">客户端标识</param>
		public AlarmClient(long nid)
		{
			id = nid;
		}

		/// <summary>
		/// 重载的构造函数
		/// </summary>
		/// <param name="nid">客户端标识</param>
		/// <param name="sname">客户端名称</param>
		public AlarmClient(int nid, string sname)
		{
			id = nid;
			name = sname;
		}

		/// <summary>
		/// 向待发送队列中插入一条告警, 如果前一报文是告警，则增加到其AlarmList当中，否则新建一个CommandMsg
        /// 如此实现告警发生、恢复事件与告警改变事件的串行化
		/// </summary>
		/// <param name="alarm"></param>
		public void FillAlarm(TKAlarm alarm)
		{
            if (AlarmFilter != "")
                if (AlarmFilter.IndexOf(alarm.BusinessType) == -1)
                    return;

            lock (m_Messages)
            {
                CommandMsgV2 alarm_msg = null;
                if (m_Messages.Count == 0 || m_Messages[m_Messages.Count - 1].TK_CommandType != Constants.TK_CommandType.ALARM_REPORT)
                {
                    alarm_msg = new CommandMsgV2();
                    alarm_msg.TK_CommandType = Constants.TK_CommandType.ALARM_REPORT;
                    alarm_msg.SetValue("ClientID", ClientID);
                    alarm_msg.SetValue("AlarmList", new ArrayList());

                    m_Messages.Add(alarm_msg);
                }
                else
                    alarm_msg = m_Messages[m_Messages.Count - 1];

                ArrayList t = alarm_msg.GetValue("AlarmList") as ArrayList;
                t.Add(new TKAlarm(alarm));
            }
		}

		/// <summary>
		/// 向待发送队列中插入一组告警
		/// </summary>
		/// <param name="alarmlist"></param>
		public void FillAlarm(ArrayList alarmlist)
		{
			foreach(object obj in alarmlist)
			{
				TKAlarm alarm = (TKAlarm)obj;
					FillAlarm(alarm);
			}
		}

		public void FillAlarm(SortedList<ulong, TKAlarm>alarmlist)
		{
			foreach(TKAlarm alarm in alarmlist.Values)
			{
                FillAlarm(alarm);
			}
		}

        public void AddMessage(CommandMsgV2 msg)
        {
            lock (m_Messages)
                m_Messages.Add(msg);
        }

        public void SetMessages(CommandMsgV2[] msgs)
        {
            lock (m_Messages)
            {
                m_Messages.Clear();
                m_Messages.AddRange(msgs);
            }
        }

        public CommandMsgV2[] DequeueMessages()
        {
            CommandMsgV2[] r = null;
            lock (m_Messages)
            {
                r = m_Messages.ToArray();
                m_Messages.Clear();
            }

            return r;
        }
	}
}
