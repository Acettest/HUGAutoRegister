using System;
using System.Collections.Generic;
using System.Text;

namespace AlarmBase
{
    class AlarmObject : IComparable<AlarmObject>
    {
        /// <summary>
        /// 互斥量，多线程下，外部使用AlarmObject属性，应lock (SyncRoot)
        /// </summary>
        public object SyncRoot = null;

        AlarmObject()
        {
            SyncRoot = new int();
        }

        #region 私有成员
        private string m_Position = "";
        private string m_AlarmName = "";
        private string m_AlarmDetail = "";
        private string m_RecentAlarmReason = "";
        private string m_FirstAlarmReason = "";
        private DateTime m_RecentOccurTime = DateTime.MinValue;
        private DateTime m_FirstOccurTime = DateTime.MinValue;
        private DateTime m_ClearTime = new DateTime();
        private bool m_Alarmed = false;
        #endregion

        public string Position
        {
            get { lock (SyncRoot) return m_Position; }
            set { lock (SyncRoot) m_Position = value; }
        }

        public string AlarmName
        {
            get { lock (SyncRoot) return m_AlarmName; }
            set { lock (SyncRoot) m_AlarmName = value; }
        }

        public string AlarmDetail
        {
            get { lock (SyncRoot) return m_AlarmDetail; }
            set { lock (SyncRoot) m_AlarmDetail = value; }
        }

        public string RecentAlarmReason
        {
            get { lock (SyncRoot) return m_RecentAlarmReason; }
            set { lock (SyncRoot) m_RecentAlarmReason = value; }
        }

        public string FirstAlarmReason
        {
            get { lock (SyncRoot) return m_FirstAlarmReason; }
            set { lock (SyncRoot) m_FirstAlarmReason = value; }
        }

        public DateTime RecentOccurTime
        {
            get { lock (SyncRoot) return m_RecentOccurTime; }
            set { lock (SyncRoot) m_RecentOccurTime = value; }
        }

        public DateTime FirstOccurTime
        {
            get { lock (SyncRoot) return m_FirstOccurTime; }
            set { lock (SyncRoot) m_FirstOccurTime = value; }
        }

        public DateTime ClearTime
        {
            get { lock (SyncRoot) return m_ClearTime; }
            set { lock (SyncRoot) m_ClearTime = value; }
        }

        public bool Alarmed
        {
            get { lock (SyncRoot) return m_Alarmed; }
            set { lock (SyncRoot) m_Alarmed = value; }
        }

        public bool RaiseAlarm(DateTime occurtime, string reason)
        {
            lock (SyncRoot)
            {
                if (m_Alarmed)
                {
                    m_RecentOccurTime = occurtime;
                    m_RecentAlarmReason = reason;

                    return false;
                }
                else
                {
                    m_FirstOccurTime = occurtime;
                    m_FirstAlarmReason = reason;

                    m_RecentOccurTime = DateTime.MinValue;
                    m_RecentAlarmReason = "";

                    m_Alarmed = true;
                    return true;
                }
            }
        }

        public bool ClearAlarm(DateTime cleartime)
        {
            lock (SyncRoot)
            {
                if (m_Alarmed)
                {
                    m_ClearTime = cleartime;
                    m_Alarmed = false;

                    return true;
                }
                else
                    return false;
            }
        }

        #region IComparable<AlarmObject> 成员

        public int CompareTo(AlarmObject other)
        {
            if (m_Position == other.m_Position && m_AlarmName == other.m_AlarmName)
                return 0;

            if (m_Position.CompareTo(other.m_Position) < 0 || (m_Position == other.m_Position && m_AlarmName.CompareTo(other.m_AlarmName) < 0))
                return -1;
            else
                return 1;
        }

        #endregion
    }
}
