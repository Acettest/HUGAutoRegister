#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: YWT_Bell_Adapter
// 作    者：d.w
// 创建时间：2014/7/1 15:34:27
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
using System.Threading;
using HGU.Tl1Wrapper;
using HGU.CommandsBuilder;
using HGUAdapterBase;

namespace RemotableHGU_Bell_Adapter
{
    class HGU_Bell_Adapter : HGUAsynAdapter
    {
        protected override void InitCommandBuilder()
        {
            m_commBuilder = new BellCommandsBuilder();
        }
        protected override void InitTl1Client()
        {
            Encoding encoding = (m_encodingStr == "" ? null : Encoding.GetEncoding(m_encodingStr));
            //多线程处理业务
            m_tl1Clients[0] = new BellTl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr,
                TELNET_TIMEOUT, encoding, (EventWaitHandle)m_idleHandles[0], m_commBuilder,
                m_dog);

            m_tl1Clients[1] = new BellTl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr,
                TELNET_TIMEOUT, encoding, (EventWaitHandle)m_idleHandles[1], m_commBuilder,
                m_dog);

            m_tl1Clients[2] = new BellTl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr,
               TELNET_TIMEOUT, encoding, (EventWaitHandle)m_idleHandles[2], m_commBuilder,
               m_dog);

            m_tl1Clients[3] = new BellTl1Wrapper(m_omc.OmcIP, m_omc.OmcPort, m_omc.User, m_omc.Pwd, m_Interval, m_Connstr,
               TELNET_TIMEOUT, encoding, (EventWaitHandle)m_idleHandles[3], m_commBuilder,
               m_dog);
        }
    }
}
