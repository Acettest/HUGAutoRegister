﻿#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: BellTl1Wrapper
// 作    者：d.w
// 创建时间：2014/6/30 9:16:21
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
using System.Net.Sockets;
using HGU.CommandsBuilder;
using HGU.Idl;

namespace HGU.Tl1Wrapper
{
    public class BellTl1Wrapper : Tl1Wrapper
    {
        public BellTl1Wrapper(string ipaddress, int port, string username, string password, int interval, string connstr,
            int telnetTimeout, Encoding omcEncoding, EventWaitHandle handle, ManuCommandsBuilder commBuilder,
            WatchDog.IWatchDog dog)
            : base(ipaddress, port, username, password, interval, connstr,
            telnetTimeout, omcEncoding, handle, commBuilder, dog)
        { }

        #region 获取工单对应的命令集
        protected override void GetCommandList(Task task)
        {
            if (task.Type == TaskType.AddIMS)
            {
                #region IMS加装
                GetVerifyOnuCommand(task, 1);  //1.核查ONU   
                GetTaskCommandList(task, 2);//2.业务配置
                #endregion
            }
            else
                base.GetCommandList(task);
        }
        #endregion

        #region 宽带新装业务
        protected override void ExecuteNewBroadband(Task task)
        {
            int step = 1;//1.核查ONU 2.根据需要添加ONU 3.管理业务流配置 4.业务配置

            #region 流程
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 2 || step 3
                if (pair.Step == 2 || pair.Step == 3)
                {
                    if (!isAddOnu)
                        continue;
                }
                #endregion

                try
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                        #region step 1 验证ONU处理,判定命令执行成功
                        if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                        {
                            //因是新装，ONU不存在，判断命令执行为成功
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;//认为命令执行成功
                            isAddOnu = true;
                            continue;
                        }
                        #endregion

                        break;

                    }
                }
                catch { }

                #region step 2 ADD-ONU后睡几秒，防止IMS装失败：“generic.mo.duplicate.entry”
                if (pair.Step == 2)
                    Thread.Sleep(1000 * 25);
                #endregion

                #region step 1 验证ONU处理
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据
                {
                    isAddOnu = true;
                }
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region IMS（语音）新装业务
        protected override void ExecuteNewIMS(Task task)
        {
            int step = 1;//1.核查ONU 2.更具需要添加ONU 3.业务配置

            #region
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                TaskCommandType cmt = pair.Command.TaskCommandType;

                #region  step 2 || step 3
                if (pair.Step == 2 || pair.Step == 3)
                {
                    if (!isAddOnu)
                        continue;
                }
                #endregion

                try
                {
                    pair.Msg = ExecuteComm(pair.Command);
                }
                catch { }

                #region 贝尔 OLT从password从修改到LOID认证方式,需执行CFG-ONU，不需要关注执行成功与失败 2016-06-24 by d.w
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                    #region step 1 验证ONU处理，判定命令执行成功
                    if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                        isAddOnu = true;
                        continue;
                    }
                    #endregion

                    break;
                }
                #endregion

                #region step 2 ADD-ONU后睡几秒，防止IMS装失败：“generic.mo.duplicate.entry”
                if (pair.Step == 2)
                    Thread.Sleep(1000 * 25);
                #endregion

                #region step 1 验证ONU处理
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据
                {
                    isAddOnu = true;
                }
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region ITPV新装业务
        protected override void ExecuteNewIPTV(Task task)
        {
            int step = 1;//1.核查ONU 2.更具需要添加ONU 3.业务配置

            #region
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                TaskCommandType cmt = pair.Command.TaskCommandType;

                #region  step 2 || step 3
                if (pair.Step == 2 || pair.Step == 3)
                {
                    if (!isAddOnu)
                        continue;
                }
                #endregion

                pair.Msg = ExecuteComm(pair.Command);
                #region 贝尔 OLT从password从修改到LOID认证方式,需执行CFG-ONU，不需要关注执行成功与失败 2016-06-24 by d.w
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                    #region step 1 验证ONU处理，判定命令执行成功
                    if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                        isAddOnu = true;
                        continue;
                    }
                    #endregion

                    break;
                }
                #endregion

                #region step 2 ADD-ONU后睡几秒，防止IMS装失败：“generic.mo.duplicate.entry”
                if (pair.Step == 2)
                    Thread.Sleep(1000 * 25);
                #endregion

                #region step 1 验证ONU处理
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据
                {
                    isAddOnu = true;
                }
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region 宽带加装业务
        protected override void ExecuteAddBroadband(Task task)
        {
            int step = 1;//1.ONU核查 2.宽带语音业务流核查（SvlanOrCvlanException，BbAlreadyExist）—不需要了 3.业务配置

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    break;
                }

                #region 验证ONU贝尔处理 step 1
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                {
                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                    pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                    break;
                }
                #endregion

                #region step 2 注释了 宽带语音业务流核查
                //if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                //{
                //    foreach (Dictionary<string, string> item in pair.Msg.Information)
                //    {
                //        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN"))
                //        {
                //            int svlan = int.Parse(item["SVLAN"]);
                //            int cvlan = int.Parse(item["CVLAN"]);
                //        }
                //    }
                //}
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region IMS（语音)加装业务
        protected override void ExecuteAddIMS(Task task)
        {
            int step = 1;//1.ONU核查 2.核查语音端口号(不需要了) 3.业务配置

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                TaskCommandType cmt = pair.Command.TaskCommandType;

                try
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        break;
                    }
                }
                catch { }

                #region 验证ONU贝尔处理 step 1
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                {
                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                    pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                    break;
                }
                #endregion

                #region step 2 注释了 核查语音端口号
                //if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                //{
                //    foreach (Dictionary<string, string> item in pair.Msg.Information)
                //    {
                //        if (item.ContainsKey("ONUPORT"))
                //        {
                //            string port = item["ONUPORT"].ToString().Replace("NA", "1");
                //            //判断端口号是否被用
                //            if (task.OnuPort == port)
                //            {
                //                pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                pair.Msg.ErrorDesc = ErrorDesc.ImsPortAlreadyExist;
                //                m_error = string.Format(" port：{0}", task.OnuPort);
                //                return;
                //            }
                //        }
                //    }
                //}
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region IPTV加装业务
        protected override void ExecuteAddIPTV(Task task)
        {
            int step = 1;//1.ONU核查 2.业务配置

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                TaskCommandType cmt = pair.Command.TaskCommandType;

                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    break;
                }

                #region 验证ONU贝尔处理 step 1
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                {
                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                    pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                    break;
                }
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region 宽带拆机业务
        protected override void ExecuteDelBroadband(Task task)
        {
            int step = 1;//1.ONU核查 2.核查资源一致性(lst-portvlan) 3.删除管理业务流 4.删除业务 5.无其他业务删除ONU

            #region 流程
            bool isDelBroadband = true;
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 4 删除业务
                if (pair.Step == 4)
                {
                    if (!isDelBroadband)
                        continue;
                }
                #endregion

                #region step 5 无其他业务删除ONU
                if (pair.Step == 5)
                {
                    if (!isDelOnu)
                        continue;
                }
                #endregion

                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    #region step 3 因同拆的时候，都需删除管理业务流，为防止后执行删管理业务流出错，则默认给成功。
                    if (pair.Step == 3 && pair.Command.CommandText.IndexOf("DEL-PONVLAN::") >= 0
                          && pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                        continue;
                    }
                    #endregion

                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    if (pair.Step == 5)
                        m_error = " 该业务为ONU上唯一业务，业务删除成功，ONU删除失败";
                    break;
                }

                #region 验证ONU贝尔处理 step 1
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                {
                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                    pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                    break;
                }
                #endregion

                #region step 2 核查资源一致性
                if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                {
                    list = pair.Msg.Information;
                    int svlan = 0, cvlan = 0, uv = 0;
                    foreach (Dictionary<string, string> item in pair.Msg.Information)
                    {
                        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                        {
                            try
                            {
                                if (item["SVLAN"] != "")
                                {
                                    svlan = int.Parse(item["SVLAN"]);
                                }
                                if (item["CVLAN"] != "")
                                {
                                    cvlan = int.Parse(item["CVLAN"]);
                                }
                                if (item["UV"] != "")
                                {
                                    uv = int.Parse(item["UV"]);
                                }
                                if (svlan == task.Svlan && cvlan == task.Cvlan && uv == task.Uvlan)
                                {
                                    isDelBroadband = true;
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    if (!isDelBroadband)
                    {
                        #region 数据与现网不一致工单
                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.BbNotExist;
                        m_error = string.Format(" Svlan：{0} Cvlan：{1} 现网Svlan：{2} 现网Cvlan：{3} ", task.Svlan, task.Cvlan, svlan, cvlan);
                        return;
                        #endregion
                    }
                }
                else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                {
                    isDelBroadband = false;
                    if (task.IsRollbackTask)//回滚
                        continue;

                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                    pair.Msg.ErrorDesc = ErrorDesc.BbNotExist;
                    return;

                }
                #endregion

                #region step 3 删除管理业务流
                if (pair.Step == 3)
                {
                    continue;
                }
                #endregion

                #region step 4 判断是否存在其他业务
                if (pair.Step == 4)
                {
                    int otherCount = 0; //其他业务数量
                    int svlan = 0, cvlan = 0, uv = 0;
                    foreach (Dictionary<string, string> item in list)
                    {
                        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                        {
                            try
                            {
                                if (item["SVLAN"] != "")
                                {
                                    svlan = int.Parse(item["SVLAN"]);
                                }
                                if (item["CVLAN"] != "" && item["CVLAN"] != "--")
                                {
                                    cvlan = int.Parse(item["CVLAN"]);
                                }
                                if (item["UV"] != "" && item["UV"] != "--")
                                {
                                    uv = int.Parse(item["UV"]);
                                }
                                if (svlan != task.Svlan || cvlan != task.Cvlan || uv != task.Uvlan)
                                {
                                    otherCount++;
                                }
                            }
                            catch { }
                        }
                    }
                    if (otherCount > 1)
                        isDelOnu = false;
                    else
                        isDelOnu = true;
                }
                #endregion

                step++;
            }
            #endregion
        }
        #endregion

        #region IMS拆机业务
        protected override void ExecuteDelIMS(Task task)
        {
            if (task.OldTaskType == TaskType.NewIMS)
            {
                #region 新装拆机流程
                int step = 1; //1.ONU核查  2.核查资源一致性  3.删除管理业务流   4.删除业务  5.无其他业务删除ONU

                #region 流程
                bool isDelIMS = true;
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    #region step 4 删除业务
                    if (pair.Step == 4)
                    {
                        if (!isDelIMS)
                            continue;
                    }
                    #endregion

                    #region step 5 无其他业务删除ONU
                    if (pair.Step == 5)
                    {
                        if (!isDelOnu)
                            continue;
                    }
                    #endregion

                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        #region step3 因同拆的时候，都需删除管理业务流，为防止后执行删管理业务流出错，则默认给成功。
                        if (pair.Step == 3 && pair.Command.CommandText.IndexOf("DEL-PONVLAN::") >= 0
                              && pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                            continue;
                        }
                        #endregion

                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        if (pair.Step == 5)
                            m_error = " 该业务为ONU上唯一业务，业务删除成功，ONU删除失败";
                        break;
                    }

                    #region 验证ONU贝尔处理 step 1
                    if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                        break;
                    }
                    #endregion

                    #region step 2 核查资源一致性
                    if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                    {
                        int svlan = 0;
                        int cvlan = 0;
                        int uv = 0;
                        foreach (Dictionary<string, string> item in pair.Msg.Information)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan == task.Svlan && cvlan == task.Cvlan && uv == task.Uvlan)
                                    {
                                        isDelIMS = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (!isDelIMS)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                            pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                            m_error = string.Format(" Svlan：{0} Cvlan：{1} UV:{2} 现网Svlan：{3} 现网Cvlan：{4} 现网UV：{5} ", task.Svlan, task.Cvlan, task.Uvlan, svlan, cvlan, uv);
                            break;
                        }
                    }
                    else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                    {
                        isDelIMS = false;
                        if (task.IsRollbackTask)//回滚
                            continue;

                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                        return;

                    }
                    #endregion

                    #region step 3 删除管理业务流
                    if (pair.Step == 3)
                    {
                        continue;
                    }
                    #endregion

                    #region step 4 判断是否存在其他业务
                    if (pair.Step == 4)
                    {
                        int otherCount = 0; //其他业务数量
                        int svlan = 0, cvlan = 0, uv = 0;
                        foreach (Dictionary<string, string> item in list)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "" && item["CVLAN"] != "--")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "" && item["UV"] != "--")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan != task.Svlan || cvlan != task.Cvlan || uv != task.Uvlan)
                                    {
                                        otherCount++;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (otherCount > 0)
                            isDelOnu = false;
                        else
                            isDelOnu = true;
                    }
                    #endregion

                    step++;
                }
                #endregion

                #endregion
            }
            else
            {
                #region 加装拆机流程
                int step = 1;//1.ONU核查 2..删除业务

                #region 流程
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        break;
                    }

                    #region 验证ONU贝尔处理 step 1
                    if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                        break;
                    }
                    #endregion
                    step++;
                }
                #endregion

                #endregion
            }
        }
        #endregion

        #region IPTV拆机业务
        protected override void ExecuteDelIPTV(Task task)
        {
            if (task.OldTaskType == TaskType.NewIPTV)
            {
                #region 新装拆机流程
                int step = 1; //1.ONU核查  2.核查资源一致性  3.删除管理业务流   4.删除业务  5.无其他业务删除ONU

                #region 流程
                bool isDelIMS = true;
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    #region step 4 删除业务
                    if (pair.Step == 4)
                    {
                        if (!isDelIMS)
                            continue;
                    }
                    #endregion

                    #region step 5 无其他业务删除ONU
                    if (pair.Step == 5)
                    {
                        if (!isDelOnu)
                            continue;
                    }
                    #endregion

                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        #region step 3 因同拆的时候，都需删除管理业务流，为防止后执行删管理业务流出错，则默认给成功。
                        if (pair.Step == 3 && pair.Command.CommandText.IndexOf("DEL-PONVLAN::") >= 0
                              && pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                            continue;
                        }
                        #endregion

                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        if (pair.Step == 5)
                            m_error = " 该业务为ONU上唯一业务，业务删除成功，ONU删除失败";
                        break;
                    }

                    #region 验证ONU贝尔处理 step 1
                    if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                        break;
                    }
                    #endregion

                    #region step 2 核查资源一致性
                    if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                    {
                        int svlan = 0;
                        int cvlan = 0;
                        int uv = 0;
                        foreach (Dictionary<string, string> item in pair.Msg.Information)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan == task.Svlan && cvlan == task.Cvlan && uv == task.Uvlan)
                                    {
                                        isDelIMS = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (!isDelIMS)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                            pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                            m_error = string.Format(" Svlan：{0} Cvlan：{1} UV:{2} 现网Svlan：{3} 现网Cvlan：{4} 现网UV：{5} ", task.Svlan, task.Cvlan, task.Uvlan, svlan, cvlan, uv);
                            break;
                        }
                    }
                    else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                    {
                        isDelIMS = false;
                        if (task.IsRollbackTask)//回滚
                            continue;

                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                        return;

                    }
                    #endregion

                    #region step 3 删除管理业务流
                    if (pair.Step == 3)
                    {
                        continue;
                    }
                    #endregion

                    #region step 4 判断是否存在其他业务
                    if (pair.Step == 4)
                    {
                        int otherCount = 0; //其他业务数量
                        int svlan = 0, cvlan = 0, uv = 0;
                        foreach (Dictionary<string, string> item in list)
                        {
                            if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN") && item.ContainsKey("UV"))
                            {
                                try
                                {
                                    if (item["SVLAN"] != "")
                                    {
                                        svlan = int.Parse(item["SVLAN"]);
                                    }
                                    if (item["CVLAN"] != "" && item["CVLAN"] != "--")
                                    {
                                        cvlan = int.Parse(item["CVLAN"]);
                                    }
                                    if (item["UV"] != "" && item["UV"] != "--")
                                    {
                                        uv = int.Parse(item["UV"]);
                                    }
                                    if (svlan != task.Svlan || cvlan != task.Cvlan || uv != task.Uvlan)
                                    {
                                        otherCount++;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (otherCount > 0)
                            isDelOnu = false;
                        else
                            isDelOnu = true;
                    }
                    #endregion

                    step++;
                }
                #endregion

                #endregion
            }
            else
            {
                #region 加装拆机流程
                int step = 1;//1.ONU核查 2.删除业务

                #region 流程
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        break;
                    }

                    #region 验证ONU贝尔处理 step 1
                    if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                    {
                        pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                        pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                        break;
                    }
                    #endregion
                    step++;
                }
                #endregion

                #endregion
            }
        }
        #endregion

        #region ONU拆机业务
        protected override void ExecuteDelOnu(Task task)
        {
            int step = 1;//1.ONU核查 2.删除ONU

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    break;
                }

                #region 验证ONU贝尔处理 step 1
                if (pair.Step == 1 && pair.Msg.Information.Count <= 0)//贝尔 执行LST-ONU成功，实际无ONU数据，命令执行失败，跳出
                {
                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                    pair.Msg.ErrorDesc = ErrorDesc.OnuNotExist;
                    break;
                }
                #endregion
                step++;
            }
            #endregion
        }
        #endregion

        #region 同装业务
        protected virtual void ExecuteSameInstall(Task task)
        {
            int step = 1;//1.核查ONU 2.根据需要添加ONU 3.管理业务流配置 4.宽带业务配置 5.带宽限速配置 6.IMS业务配置

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    #region step 1 验证ONU处理,判定命令执行成功
                    if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                    {
                        //因是新装，ONU不存在，判断命令执行为成功
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;//认为命令执行成功
                        isAddOnu = true;
                        continue;
                    }
                    #endregion

                    break;
                }
                step++;
            }
            #endregion
        }
        #endregion
    }
}
