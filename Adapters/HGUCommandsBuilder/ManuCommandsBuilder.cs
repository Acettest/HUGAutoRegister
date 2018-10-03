#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: CommandsBuilder
// 作    者：d.w
// 创建时间：2014/6/18 9:14:32
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
using HGU.Idl;

namespace HGU.CommandsBuilder
{
    public abstract class ManuCommandsBuilder : ICommandsBuilder
    {
        public List<Command> ParseTask2Commands(Task task)
        {
            switch (task.Type)
            {
                case TaskType.NewBroadband:       
                case TaskType.AddBroadband:
                case TaskType.NewIMS:
                case TaskType.AddIMS:
                case TaskType.NewIPTV:
                case TaskType.AddIPTV:
                    return ParseAddTask(task);
                case TaskType.DelONU:
                    return ParseDelOnu(task);
                case TaskType.DelBroadband:
                case TaskType.DelIMS:
                case TaskType.DelIPTV:
                    return ParseDelTask(task);
                case TaskType.SameInstall:
                    return ParseSameInstall(task);
                default:
                    return null;
            }
        }

        public List<Command> ParseTask2Commands(Task task, TaskCommandType taskCommandType)
        {
            switch (taskCommandType)
            {
                case TaskCommandType.Business:
                    switch (task.Type)
                    {
                        case TaskType.NewBroadband:
                        case TaskType.AddBroadband:
                        case TaskType.NewIMS:
                        case TaskType.AddIMS:
                        case TaskType.NewIPTV:
                        case TaskType.AddIPTV:
                            return ParseAddTask(task);
                        case TaskType.DelONU:
                            return ParseDelOnu(task);
                        case TaskType.DelBroadband:
                        case TaskType.DelIMS:
                        case TaskType.DelIPTV:
                            return ParseDelTask(task);
                        case TaskType.SameInstall:
                            return ParseSameInstall(task);
                        default:
                            return null;
                    }
                case TaskCommandType.AddOnu:
                    return ParseAddOnu(task);
                case TaskCommandType.DelOnu:
                    return ParseDelOnu(task);
                case TaskCommandType.VerifyOnu:
                    return ParseLstOnu(task);
                case TaskCommandType.VerifyVlan:
                    return ParseLstPortVlan(task);
                case TaskCommandType.UPBandwidth:
                    return ParseUPBandwidth(task);
                case TaskCommandType.ManageBusiness:
                    return ParseAddManagBuinss(task);
                case TaskCommandType.DelManageBusiness:
                    return ParseDelManagBuinss(task);
                case TaskCommandType.SameInstall:
                    return ParseSameInstall(task);
                default:
                    return null;
            }
        }

        public virtual string GetLoginStr(string userName, string password)
        {
            return "LOGIN:::CTAG::UN=" + userName + ",PWD=" + password + ";";
        }

        public virtual string GetLogoutStr()
        {
            return "LOGOUT:::CTAG::;";
        }

        public virtual string GetLogoutStr(string userName)
        {
            return GetLogoutStr();
        }

        public abstract ErrorDesc ParseErrorDesc(Dictionary<string, string> additionalInfo, string command);

        protected string m_block_records = "block_records=";
        protected char[] m_separator = new char[] { '\t' };
        public virtual List<Dictionary<string, string>> ParseInformation(string replyContent)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            string[] array = replyContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int rows = 0;
            int columns = 0;
            bool b_over = false;
            int i = 0;
            int index = 0;
            int lastindex = 0;
            string block_records = m_block_records;//"block_records=";
            string[] t_columns = null;
            foreach (string s in array)
            {
                if (s.Contains(block_records))
                {
                    rows = Convert.ToInt32(s.Substring(s.IndexOf(block_records) + block_records.Length));
                }
                if (!b_over && array[i].Contains("------------------"))
                {
                    t_columns = array[i + 1].Split(m_separator, StringSplitOptions.RemoveEmptyEntries);
                    columns = t_columns.Length;
                    index = i + 1 + 1;
                    lastindex = index + rows - 1;
                }
                if (i != 0 && i >= index && i <= lastindex)
                {
                    string[] t_values = s.Split(m_separator, StringSplitOptions.RemoveEmptyEntries);
                    if (t_values.Length != columns)
                        continue;
                    Dictionary<string, string> item = new Dictionary<string, string>();
                    for (int j = 0; j < t_values.Length; j++)
                    {
                        item.Add(t_columns[j].ToString(), t_values[j].ToString());
                    }
                    list.Add(item);
                    b_over = true;
                }
                ++i;
            }

            return list;
        }

        /// <summary>
        /// 查询ONU
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseLstOnu(Task task);

        /// <summary>
        /// 装ONU
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseAddOnu(Task task);

        /// <summary>
        /// 拆ONU
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseDelOnu(Task task);

        /// <summary>
        /// 装业务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseAddTask(Task task);

        /// <summary>
        /// 拆业务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseDelTask(Task task);

        /// <summary>
        /// 查询业务VLAN信息
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseLstPortVlan(Task task);

        /// <summary>
        /// 配置ONU上行口带宽限速
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseUPBandwidth(Task task);

        /// <summary>
        /// 新增管理业务流配置
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseAddManagBuinss(Task task);

        /// <summary>
        /// 删除管理业务流配置
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseDelManagBuinss(Task task);

        /// <summary>
        /// 同装业务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected abstract List<Command> ParseSameInstall(Task task);


    }
}
