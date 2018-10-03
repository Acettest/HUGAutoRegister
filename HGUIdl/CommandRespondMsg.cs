#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: CommandRespondMsg
// 作    者：d.w
// 创建时间：2014/6/12 16:15:21
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
    public enum CommandResponseStatus
    { COMPLD, DENY };

    public enum ErrorDesc
    {
        OltNotExist,                           //olt不存在
        OltOffline,                            //olt掉线
        PonNotExist,                           //pon板不存在
        OnuNotExist,
        OnuAlreadyExist,                       //已存在
        OnuTypeNotExist,                       //onu型号不存在
        OnuTypeError,                          //
        SvlanError,                            //svlan错误
        BbAlreadyExist,                        //宽带业务已存在
        BbNotExist,                             //宽带业务不存在
        BbSvlanOrCvlanUnlike,               //宽带业务与现网不一致
        SvlanOrCvlanException,                 //svlan or cvlan 异常
        ImsSvlanOrCvlanUnlike,                 //语音业务与现网不一致
        ImsPortAlreadyExist,                   //IMS端口被占用
        ImsPortUnlike,                          //IMS端口与现网不一致
        ImsNotExist,                            //语音业务不存在
        InformationException,                   //数据异常
        HwIMSDelay,                             //华为IMS延迟 2个IMS执行LST-POTS需要间隔一会 新装、加装中有处理
        Unknown,
        None
    }
    /// <summary>
    /// 示例
    ///   FH_10.16.38.20 2012-03-29 16:16:17
    ///M  CTAG DENY
    ///   EN=IRNE   ENDESC=resource does not exist   EADD=onu not exist:ffff0004
    ///;
    /// </summary>
    public class CommandResponseMsg
    {
        #region IFields
        private string m_neID;
        private DateTime m_omcTime;
        private string m_ctag;
        private CommandResponseStatus m_completionCode;
        private DateTime m_localTime;
        private Dictionary<string, string> m_additionalInfo;
        private List<Dictionary<string, string>> m_information;
        private string m_replyContent;
        private ErrorDesc m_errorDesc;
        #endregion

        #region IProperties
        public string NeID
        {
            get { return m_neID; }
            set { m_neID = value; }
        }

        public DateTime OmcTime
        {
            get { return m_omcTime; }
            set { m_omcTime = value; }
        }

        public string Ctag
        {
            get { return m_ctag; }
            set { m_ctag = value; }
        }

        public CommandResponseStatus CompletionCode
        {
            get { return m_completionCode; }
            set { m_completionCode = value; }
        }

        public Dictionary<string, string> AdditionalInfo
        {
            get { return m_additionalInfo; }
            set { m_additionalInfo = value; }
        }

        public List<Dictionary<string, string>> Information
        {
            get { return m_information; }
            set { m_information = value; }
        }

        public DateTime LocalTime
        {
            get { return m_localTime; }
        }

        public string ReplyContent
        {
            get { return m_replyContent; }
            set { m_replyContent = value; }
        }

        public ErrorDesc ErrorDesc
        {
            get { return m_errorDesc; }
            set { m_errorDesc = value; }
        }
        #endregion

        #region IConstructors
        public CommandResponseMsg()
        {
            m_localTime = DateTime.Now;
            m_errorDesc = ErrorDesc.None;
        }
        #endregion

        #region IMethods
        public override string ToString()
        {
            //todo
            StringBuilder msg = new StringBuilder();
            msg.Append("\r\n\n   ");
            msg.Append(m_neID + " ");
            msg.Append(m_omcTime.ToString("yyyy-MM-dd HH:mm:ss"));
            msg.Append("\r\nM  ");
            msg.Append(m_ctag + " ");
            msg.Append(Enum.GetName(typeof(CommandResponseStatus), m_completionCode));
            if (m_additionalInfo != null)
            {
                msg.Append("\r\n");
                if (m_additionalInfo.Count != 0)
                    foreach (KeyValuePair<string, string> pair in m_additionalInfo)
                    {
                        msg.Append("   " + pair.Key + "=" + pair.Value);
                    }
            }
            msg.Append("\r\n;\n");
            return msg.ToString();
        }

        public string GetAddInfo()
        {
            string info = "";
            if (m_additionalInfo != null)
            {
                foreach (KeyValuePair<string, string> pair in m_additionalInfo)
                {

                    info += pair.Key + "=" + pair.Value + ";";

                }
            }
            return info;
        }

        public void ParseErrorDesc(ICommandsBuilder builder, string command)
        {
            if (m_completionCode == CommandResponseStatus.COMPLD)
                return;
            m_errorDesc = builder.ParseErrorDesc(m_additionalInfo, command);
        }
        #endregion

        #region SMethods
        /// <summary>
        /// 格式见用户指南
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static CommandResponseMsg ConvertFromString(ICommandsBuilder builder, string msg)
        {
            if (msg == null || msg == "")
                return null;
            try
            {
                CommandResponseMsg rm = new CommandResponseMsg();
                rm.m_replyContent = msg;
                //todo
                string[] list = msg.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                //header
                string header = list[0];
                if (header.Substring(3, 1) != " ")
                    rm.m_neID = header.Substring(3, header.IndexOf(' ', 3) - 3);
                else
                    rm.m_neID = "";
                rm.OmcTime = Convert.ToDateTime(header.Substring(header.IndexOf(' ', 3) + 1));
                //response identification
                string identification = list[1];
                if (identification.Substring(3, 1) != " ")
                    rm.m_ctag = identification.Substring(3, identification.IndexOf(' ', 3) - 3);
                else
                    rm.m_ctag = "";
                string completionCode = identification.Substring(identification.IndexOf(' ', 3) + 1);
                if (completionCode == "COMPLD")
                    rm.m_completionCode = CommandResponseStatus.COMPLD;
                else
                    rm.m_completionCode = CommandResponseStatus.DENY;
                //text block
                Dictionary<string, string> info = new Dictionary<string, string>();
                string text = list[2];
                if (text != ";" && text.Contains("EN="))
                {
                    text = list[2];
                    string[] textAttr = text.Split(new string[] { "   " }, StringSplitOptions.RemoveEmptyEntries);
                    if (textAttr.Length > 0)
                    {
                        foreach (string s in textAttr)
                        {
                            string[] attr = s.Split('=');
                            info.Add(attr[0], attr[1]);
                        }
                    }
                }
                rm.m_additionalInfo = info;

                #region
                if (info.ContainsKey("EN"))
                {
                    if (info["EN"].Trim() == "0")
                        rm.m_completionCode = CommandResponseStatus.COMPLD;
                    else
                        rm.m_completionCode = CommandResponseStatus.DENY;
                }
                #endregion
                if (list.Length <= 4)
                    rm.m_information = null;
                else
                    rm.m_information = builder.ParseInformation(msg);

                return rm;
            }
            catch (Exception ex)
            {
                throw new Exception("转化命令响应错误:\r\n" + ex.ToString());
            }
        }


        #endregion
    }
}
