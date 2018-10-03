#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: ICommandsBuilder
// 作    者：d.w
// 创建时间：2014/6/12 16:15:52
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
    public interface ICommandsBuilder
    {
        List<Command> ParseTask2Commands(Task task);
        List<Command> ParseTask2Commands(Task task,TaskCommandType taskCommandType);
        string GetLoginStr(string userName, string password);
        string GetLogoutStr();
        string GetLogoutStr(string userName);
        ErrorDesc ParseErrorDesc(Dictionary<string, string> additionalInfo, string command);
        List<Dictionary<string, string>> ParseInformation(string replyContent);
    }
}
