#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: ReplyBossMsg
// 作    者：d.w
// 创建时间：2014/6/12 16:18:01
// 描    述：
// 版    本：1.0.0.0
//-----------------------------------------------------------------------------
// 历史更新纪录
//-----------------------------------------------------------------------------
// 版    本：           修改时间：           修改人：           
// 修改内容：
//-----------------------------------------------------------------------------
// Copyright (C) 2009-2014 www.chaselwang.com . All Rights Reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace HGU.Idl
{
    public class ReplyBossMsg
    {
        #region IFields
        private string m_taskID;
        private TaskStatus m_status;
        private string m_responseMsg;
        private DateTime m_receiveTime;
        //private int m_rollback;                       //-1,0,1
        private bool m_netDelay;

        #endregion

        #region IProperties
        public string TaskID
        {
            get { return m_taskID; }
        }

        public TaskStatus Status
        {
            get { return m_status; }
        }

        public string ResponseMsg
        {
            get { return m_responseMsg; }
        }

        public DateTime ReceiveTime
        {
            get { return m_receiveTime; }
        }

        public bool NetDelay
        {
            get { return m_netDelay; }
        }
        #endregion

        #region IConstructors
        public ReplyBossMsg(string taskID, TaskStatus status, string responseMsg, bool netInterrupt, DateTime receiveTime)
        {
            m_taskID = taskID;
            m_status = status;
            m_responseMsg = responseMsg;
            m_netDelay = netInterrupt;
            m_receiveTime = receiveTime;
        }
        #endregion

        #region IMethods
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            return CompareMembers(this, obj as ReplyBossMsg);
        }

        public static bool CompareMembers(ReplyBossMsg left, ReplyBossMsg right)
        {
            if (left == null || right == null)
                return false;
            if (left.m_taskID != right.m_taskID)
                return false;
            return true;
        }
        #endregion
    }
}
