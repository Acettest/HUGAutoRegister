using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;

namespace TK_AlarmManagement
{
    class ServerAlarmMsgInterpreter : ICommandInterpreter
    {
        ICommunicator m_Communicator = null;

        private Queue<ICommunicationMessage> m_DelayedMsgs = new Queue<ICommunicationMessage>();

        private long m_Run = 0;
        private AutoResetEvent m_SignalNewMessage = new AutoResetEvent(false);
        private ManualResetEvent m_ClearEvent = new ManualResetEvent(true);
        private void _dealing(object state)
        {
            m_ClearEvent.Reset();

            try
            {
                while (Interlocked.Read(ref m_Run) == 1)
                {
                    m_SignalNewMessage.WaitOne();
                    if (Interlocked.Read(ref m_Run) == 0)
                        return;

                    // 每次得到信号都将队列中的报文处理完
                    while (Interlocked.Read(ref m_Run) == 1)
                    {
                        ICommunicationMessage msg = null;

                        lock (m_DelayedMsgs)
                        {
                            if (m_DelayedMsgs.Count == 0)
                                break;
                            msg = m_DelayedMsgs.Dequeue();
                        }

                        switch (msg.TK_CommandType)
                        {
                            case Constants.TK_CommandType.ALARM_REPORT:
                                    _processAlarm(msg.GetValue("AlarmList") as ArrayList);
                                break;
                            default:
                                m_Communicator.enqueueDelayedMessages(msg);
                                break;
                        }

                        Thread.Sleep(10);
                    }
                }
            }
            finally
            {
                m_ClearEvent.Set();
            }
        }

        private void _processAlarm(ArrayList alarms)
        {
            lock (alarms.SyncRoot)
            {
                if (alarms.Count == 0)
                    return;

                for (int i = 0; i < alarms.Count; ++i)
                {
                    TKAlarm alarm = (TKAlarm)alarms[i];
                    CommandMsgV2 msg = alarm.ConvertToMsg();

                    m_Communicator.enqueueDelayedMessages(msg);

                    Thread.Sleep(0);

                    if (Interlocked.Read(ref m_Run) == 0)
                        break;
                }
            }
        }

        #region ICommandInterpreter 成员

        public ICommunicator CommunicatorObj
        {
            get
            {
                return m_Communicator;
            }
            set
            {
                m_Communicator = value;
            }
        }

        public bool Start()
        {
            if (Interlocked.Exchange(ref m_Run, 1) == 1)
                return true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(_dealing));

            return true;
        }

        public bool Stop()
        {
            if (Interlocked.Exchange(ref m_Run, 0) == 0)
                return true;

            m_SignalNewMessage.Set();
            Thread.Sleep(0);

            m_ClearEvent.WaitOne();
            return true;
        }

        public ICommunicationMessage ImmediatelyInterpret(ICommunicationMessage cm)
        {
            return null; // no immediately translation
        }

        public bool DelayedInterpret(ICommunicationMessage cm)
        {
            switch (cm.TK_CommandType)
            {
                case Constants.TK_CommandType.ALARM_REPORT:
                case Constants.TK_CommandType.ALARM_ACK_CHANGE:
                    lock (m_DelayedMsgs)
                        m_DelayedMsgs.Enqueue(cm);
                    m_SignalNewMessage.Set();
                    break;
                default:
                    return false;
            }

            return true;
        }

        #endregion
    }
}
