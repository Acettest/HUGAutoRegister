using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HGU.Idl;
using System.Xml;
using System.Data.SqlClient;

using RemotableHGU_PBoss_Adapter.WebReference;

namespace RemotableHGU_PBoss_Adapter
{
    public delegate void LogHandler(string log);
    public delegate bool IsAdapterRunningHandler();
    public delegate void EnqueueReplyHandler(ReplyBossMsgExtend replay);

    public class KBossWebWrapper
    {
        private const int SLEEP_INTERVAL = 100;                    //millionsecond
        private const int ResponseDelay = 15;                      //秒 kboss延时

        NMExternalOtherUtilService m_service;
        private string m_Connstr;
        private EventWaitHandle m_idleHandle;                          //主程序分配task用
        private ReplyBossMsgExtend m_currentReply;
        private long m_replyExist = 0L;


        public ReplyBossMsgExtend CurrentReply
        {
            get { return m_currentReply; }
        }
        public event LogHandler SendLog;
        public event IsAdapterRunningHandler IsAdapterRunning;
        public event EnqueueReplyHandler EnqueueReply;

        public KBossWebWrapper(EventWaitHandle handle, string connstr)
        {
            m_service = new NMExternalOtherUtilService();
            m_service.Timeout = 300000;
            m_Connstr = connstr;
            m_idleHandle = handle;
        }

        public void Retriever()
        {
            UnlockDispatch();
            while (IsAdapterRunning())
            {
                DoWork();
                while (true)
                {
                    if (Interlocked.Read(ref m_replyExist) == 1L || !IsAdapterRunning())
                        break;
                    Thread.Sleep(SLEEP_INTERVAL);
                }
            }
        }

        private void DoWork()
        {
            try
            {
                while (Interlocked.Read(ref m_replyExist) == 1L)
                {
                    try
                    {
                        ResponseTask();
                        //todo
                    }
                    catch (Exception e)
                    {
                        SendLog(e.ToString());
                    }
                    finally
                    {
                        UnlockReply();
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
            }

        }

        #region 同步处理
        public void SetReply(ReplyBossMsgExtend reply)
        {
            m_currentReply = reply;
            Interlocked.Exchange(ref m_replyExist, 1L);
        }

        private void UnlockReply()
        {
            m_currentReply = null;
            Interlocked.Exchange(ref m_replyExist, 0L);
            UnlockDispatch();
        }

        private void UnlockDispatch()
        {
            m_idleHandle.Set();
        }
        #endregion

        #region
        private void ResponseTask()
        {
            if (!CheckDelay())
            {
                EnqueueReply(m_currentReply);
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            string resXML = string.Empty;
            switch (m_currentReply.Type)
            {
                case ReplyType.NormalTask:
                    if (m_currentReply.ReplyBoss.NetDelay)
                        ClearTask("spClearTask");
                    else if (ResponseTask(xmlDoc, false, out resXML))
                    {
                        UpdateTask(xmlDoc, "task", resXML);
                        ClearTask("spClearTask");
                    }
                    return;
                case ReplyType.RollbackTask:
                    if (ResponseTask(xmlDoc, true, out resXML))
                    {
                        UpdateTask(xmlDoc, "RollbackHisTask", resXML);
                        ClearTask("spClearRollbackTask");
                    }
                    return;
                case ReplyType.NetInterrupt:
                    if (ResponseTask(xmlDoc, false, out resXML))
                    {
                        UpdateTask(xmlDoc, "Task", resXML);
                    }
                    return;
                default:
                    return;
            }
        }

        /// <summary>
        /// 反馈工单
        /// </summary>
        private bool ResponseTask(XmlDocument xmlDoc, bool rollback, out string resXML)
        {
            resXML = GetResFormat(rollback);
            string result = string.Empty;
            try
            {
                //test
                DateTime test1 = DateTime.Now;
                result = m_service.feedBackResult(resXML);
                //result = m_service.returnMakeData(resXML);
                SendLog(m_currentReply.ReplyBoss.TaskID + " returnMakeData spend " + (DateTime.Now - test1).TotalSeconds.ToString() + " s");
                xmlDoc.LoadXml(result);
                return true;
            }
            catch (System.Net.WebException ex)
            {
                //捕获webservice的异常
                //System.Net.WebException: 无法连接到远程服务器 ---> System.Net.Sockets.SocketException: 由于连接方在一段时间后没有正确答复或连接的主机没有反应，连接尝试失败。 10.151.131.21:7001
                //告警
                SendLog(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                SendLog(ex.ToString());
                return false;
            }
        }

        private void UpdateTask(XmlDocument xmlDoc, string tableName, string resXML)
        {
            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("//Status");
            string status = node.InnerText == "null" ? "0" : node.InnerText;
            SQLUtil.UpdateTaskReplyInfo(m_Connstr, m_currentReply.ReplyBoss.TaskID, xmlDoc.InnerXml,
                DateTime.Now, int.Parse(status) + 1, resXML, tableName);
        }

        private void ClearTask(string procName)
        {
            SqlParameter spTaskID = new SqlParameter("@taskID", m_currentReply.ReplyBoss.TaskID);
            SQLUtil.ExecProc(m_Connstr, procName, spTaskID);
        }

        /// <summary>
        /// kboss要求必须要延时15s后才上报
        /// </summary>
        private bool CheckDelay()
        {
            return DateTime.Now > m_currentReply.ReplyBoss.ReceiveTime.AddSeconds(ResponseDelay);
        }

        /// <summary>
        /// boss接口规范
        /// </summary>
        /// <returns></returns>
        private string GetResFormat(bool rollback)
        {
            return string.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<ROOT>\n<WorkCode>{0}</WorkCode>\n<Status>{1}</Status>\n<Desc>{2}</Desc>\n<Rollback>{3}</Rollback>\n</ROOT>\n",
                m_currentReply.ReplyBoss.TaskID,
                (m_currentReply.ReplyBoss.NetDelay ? "1" : (rollback ? "" : (m_currentReply.ReplyBoss.Status == TaskStatus.Succeed ? "0" : "-1"))),
                //(m_currentReply.ReplyBoss.Status == TaskStatus.Succeed ? "" : m_currentReply.ReplyBoss.ResponseMsg),
                m_currentReply.ReplyBoss.ResponseMsg,
                (!rollback ? "" : (m_currentReply.ReplyBoss.Status == TaskStatus.Succeed ? "0" : "-1")));   
        }
        #endregion
    }
}
