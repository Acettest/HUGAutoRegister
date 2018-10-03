#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: CommandAndResponsePair
// 作    者：d.w
// 创建时间：2014/6/12 16:15:01
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
    public enum TaskCommandType
    {
        SameInstall,         //同装业务
        DelManageBusiness,  //删除管理业务流
        ManageBusiness,     //管理业务流
        Business,           //业务
        AddOnu,             //添加ONU
        DelOnu,             //删除ONU
        VerifyOnu,          //ONU验证
        VerifyVlan,         //验证VLAN
        VerifyIMS,          //验证语音
        AddPonVlan,         //中兴语音PONVLAN
        DelPonVlan,
        UPBandwidth,        //ONU上行口带宽限速 add 2015-07-29
        None
    }

    public class Command
    {
        #region IFields
        private string m_commandText;
        private DateTime m_generateTime;
        private bool m_execute;
        private DateTime m_executeTime;
        private TaskCommandType m_taskCommandType;
        #endregion

        #region IProperties
        public string CommandText
        {
            get { return m_commandText; }
        }

        public DateTime GenerateTime
        {
            get { return m_generateTime; }
        }

        public DateTime ExecuteTime
        {
            get { return m_executeTime; }
            set { m_executeTime = value; }
        }

        public bool Execute
        {
            get { return m_execute; }
            set { m_execute = value; }
        }

        public TaskCommandType TaskCommandType
        {
            get { return m_taskCommandType; }
        }
        #endregion

        #region IConstructors
        public Command(string commandText)
        {
            m_commandText = commandText;
            m_generateTime = DateTime.Now;
            m_execute = false;
            m_taskCommandType = TaskCommandType.Business;
        }

        public Command(string commandText, TaskCommandType taskCommandType)
        {
            m_commandText = commandText;
            m_generateTime = DateTime.Now;
            m_execute = false;
            m_taskCommandType = taskCommandType;
        }
        #endregion
    }

    public class CommandAndResponsePair
    {
        #region IFields
        private Command m_command;
        private CommandResponseMsg m_msg;
        private int m_step;
        #endregion

        #region IProperties
        public Command Command
        {
            get { return m_command; }
        }

        public CommandResponseMsg Msg
        {
            get { return m_msg; }
            set { m_msg = value; }
        }
        public int Step
        {
            get { return m_step; }
        }
        #endregion

        #region IConstructors
        public CommandAndResponsePair(Command command)
        {
            m_command = command;
        }
        public CommandAndResponsePair(Command command,int step)
        {
            m_command = command;
            m_step = step;
        }
        #endregion
    }
}
