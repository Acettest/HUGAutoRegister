#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: ZTETl1Wrapper
// 作    者：d.w
// 创建时间：2014/6/25 17:21:41
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
    public class ZTETl1Wrapper : Tl1Wrapper
    {

        public ZTETl1Wrapper(string ipaddress, int port, string username, string password, int interval, string connstr,
            int telnetTimeout, Encoding omcEncoding, EventWaitHandle handle, ManuCommandsBuilder commBuilder,
            WatchDog.IWatchDog dog)
            : base(ipaddress, port, username, password, interval, connstr,telnetTimeout, omcEncoding, handle,
            commBuilder, dog)
        {

        }

        #region 获取工单对应的命令集
        /// <summary>
        /// 获取工单对应的命令集
        /// </summary>
        /// <param name="task"></param>
        protected override void GetCommandList(Task task)
        {
            GetVerifyOnuCommand(task, 1);  //1.核查ONU   

            if (task.Type == TaskType.NewBroadband)
            {
                #region 宽带新装
                GetAddOnuCommand(task, 2);  //2.根据需要添加ONU     
                GetAddManagBuinss(task, 3);//3.管理业务流配置 
                GetTaskCommandList(task, 4);//4.业务配置
                GetUPBandwidthCommand(task, 5);//5.带宽限速配置
                #endregion
            }
            if (task.Type == TaskType.NewIMS)
            {
                #region IMS新装
                GetAddOnuCommand(task, 2);  //2.根据需要添加ONU     
                GetAddManagBuinss(task, 3);//3.管理业务流配置 
                GetTaskCommandList(task, 4);//4.业务配置
                #endregion
            }
            if (task.Type == TaskType.AddBroadband)
            {
                #region 宽带加装
                GetTaskCommandList(task, 2);  //2.业务配置
                GetUPBandwidthCommand(task, 3);//4.带宽限速配置
                #endregion
            }
            if (task.Type == TaskType.AddIMS)
            {
                #region IMS加装
                GetTaskCommandList(task, 2);//2.业务配置
                #endregion
            }
            if (task.Type == TaskType.DelBroadband)
            {
                #region 宽带拆机
                GetVerifyVlanCommand(task, 2);//2.核查资源一致性 
                GetTaskCommandList(task, 3);//3.删除业务
                //GetVerifyVlanCommand(task, 4);//4.判断是否存在其他业务
                GetDelManagBuinss(task, 4);//删除管理业务流
                GetDelOnuCommand(task, 5);//5.无其他业务删除ONU
                #endregion
            }
            if (task.Type == TaskType.DelIMS)
            {
                #region IMS拆机
                if (task.OldTaskType == TaskType.NewIMS)
                {
                    #region 新装拆机
                    GetVerifyVlanCommand(task, 2);//2.核查资源一致性
                    GetTaskCommandList(task, 3);//3.删除业务
                    GetDelManagBuinss(task, 4);//删除管理业务流
                    GetDelOnuCommand(task, 5); //5.无其他业务删除ONU
                    #endregion
                }
                else
                {
                    #region 加装拆机
                    GetTaskCommandList(task, 2);//2.删除业务
                    #endregion
                }
                #endregion
            }
            if (task.Type == TaskType.DelONU)
            {
                #region ONU拆机
                GetTaskCommandList(task, 2);//2.删除业务
                #endregion
            }
            if (task.Type == TaskType.SameInstall)
            {
                #region 同装业务
                GetTaskCommandList(task, 2); //2.业务配置
                #endregion
            }
            if (task.Type == TaskType.NewIPTV)
            {
                #region IPTV新装
                GetAddOnuCommand(task, 2);  //2.根据需要添加ONU     
                GetAddManagBuinss(task, 3);//3.管理业务流配置 
                GetTaskCommandList(task, 4);//4.业务配置
                #endregion
            }
            if (task.Type == TaskType.AddIPTV)
            {
                #region IPTV加装
                GetTaskCommandList(task, 2);//2.业务配置
                #endregion
            }
            if (task.Type == TaskType.DelIPTV)
            {
                #region IPTV拆机
                if (task.OldTaskType == TaskType.NewIPTV)
                {
                    #region 新装拆机
                    GetVerifyVlanCommand(task, 2);//2.核查资源一致性
                    GetDelManagBuinss(task, 3);//3.删除管理业务流
                    GetTaskCommandList(task, 4);//4.删除业务
                    GetDelOnuCommand(task, 5); //5.无其他业务删除ONU
                    #endregion
                }
                else
                {
                    #region 加装拆机
                    GetTaskCommandList(task, 2);//2.删除业务
                    #endregion
                }
                #endregion
            }
        }
        #endregion

        #region 查询业务流
        /// <summary>
        /// 查询业务流
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        private void GetVerifyVlanCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.VerifyVlan);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取中兴新增PON口VLAN命令
        /// <summary>
        /// 获取中兴新增PON口VLAN命令
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetAddPonVlanCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.AddPonVlan);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region 获取中兴删除PON口VLAN命令
        /// <summary>
        /// 获取中兴删除PON口VLAN命令
        /// </summary>
        /// <param name="task"></param>
        /// <param name="step"></param>
        protected void GetDelPonVlanCommand(Task task, int step)
        {
            List<Command> list = m_commBuilder.ParseTask2Commands(task, TaskCommandType.DelPonVlan);
            foreach (Command c in list)
            {
                CommandAndResponsePair pair = new CommandAndResponsePair(c, step);
                m_commands.Add(pair);
            }
        }
        #endregion

        #region IMS（语音）新装业务
        /// <summary>
        /// IMS（语音）新装业务
        /// </summary>
        /// <param name="task"></param>
        protected override void ExecuteNewIMS(Task task)
        {
            int step = 1;//1.核查ONU 2.根据需要添加ONU 3.管理业务流配置 4.业务配置     
     
            #region 流程
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 2 || step 3
                //添加ONU、管理业务流配置都需要配置ONU是否存在
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

                        #region step 1 验证ONU处理，判定命令执行成功
                        if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                        {
                            ///因是新装，ONU不存在，判断命令执行为成功
                            pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
                            isAddOnu = true;
                            continue;
                        }
                        #endregion

                        break;
                    }
                }
                catch { }

                step++;
            }
            #endregion
        }
        #endregion

        #region IPTV新装业务
        /// <summary>
        /// IPTV新装业务
        /// </summary>
        /// <param name="task"></param>
        protected override void ExecuteNewIPTV(Task task)
        {
            int step = 1;//1.核查ONU 2.根据需要添加ONU 3.管理业务流配置 4.业务配置     

            #region 流程
            isAddOnu = false;
            foreach (CommandAndResponsePair pair in m_commands)
            {
                #region step 2 || step 3
                //添加ONU、管理业务流配置都需要配置ONU是否存在
                if (pair.Step == 2 || pair.Step == 3)
                {
                    if (!isAddOnu)
                        continue;
                }
                #endregion

                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);

                    #region step 1 验证ONU处理，判定命令执行成功
                    if (pair.Step == 1 && pair.Msg.ErrorDesc == ErrorDesc.OnuNotExist)
                    {
                        ///因是新装，ONU不存在，判断命令执行为成功
                        pair.Msg.CompletionCode = CommandResponseStatus.COMPLD;
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

        #region IMS（语音)加装业务
        /// <summary>
        ///  IMS（语音)加装业务
        /// </summary>
        /// <param name="task"></param>
        protected override void ExecuteAddIMS(Task task)
        {
            int step = 1;//1.ONU核查 3.业务配置
          
            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
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

                #region step 2  注释了 宽带语音业务流核查（SvlanOrCvlanException,ImsSvlanOrCvlanUnlike，ImsPortAlreadyExist
                //if (pair.Step == 2 && pair.Msg.Information.Count > 0)
                //{
                //    foreach (Dictionary<string, string> item in pair.Msg.Information)
                //    {
                //        if (item.ContainsKey("SVLAN") && item.ContainsKey("CVLAN"))
                //        {
                //            int svlan = int.Parse(item["SVLAN"]);
                //            int cvlan = int.Parse(item["CVLAN"]);
                //            //判断是否属于语音
                //            if (svlan >= IMS_MIN_SVLAN && svlan <= IMS_MAX_SVLAN && cvlan == IMS_CVLAN)
                //            {//是
                //                //判断是否一致
                //                if (svlan != task.Svlan || cvlan != task.Cvlan)
                //                {//否
                //                    pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                    pair.Msg.ErrorDesc = ErrorDesc.ImsSvlanOrCvlanUnlike;
                //                    m_error = string.Format(" svlan：{0} cvlan：{1} 现网svlan：{2} 现网cvlan：{3} ", task.Svlan, task.Cvlan, svlan, cvlan);
                //                    return;
                //                }
                //                isAddPonVlanZTE = false;
                //                continue;
                //            }
                //            //判断是否属于宽带
                //            if (svlan < BB_MIN_SVLAN || svlan > BB_MAX_SVLAN || cvlan < BB_MIN_CVLAN || cvlan > BB_MAX_CVLAN)
                //            {//否
                //                pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                //                pair.Msg.ErrorDesc = ErrorDesc.SvlanOrCvlanException;
                //                m_error = string.Format(" svlan：{0} cvlan：{1}", svlan, cvlan);
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
        /// <summary>
        ///  IPTV加装业务
        /// </summary>
        /// <param name="task"></param>
        protected override void ExecuteAddIPTV(Task task)
        {
            int step = 1;//1.ONU核查 2.业务配置

            #region 流程
            foreach (CommandAndResponsePair pair in m_commands)
            {
                pair.Msg = ExecuteComm(pair.Command);
                if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                {
                    pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                    break;
                }
                
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
                bool isDeleteIMS = true;
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    #region step 4 删除业务
                    if (pair.Step == 4)
                    {
                        if (!isDeleteIMS)
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
                                        isDeleteIMS = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (!isDeleteIMS)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                            pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                            m_error = string.Format(" Svlan：{0} Cvlan：{1} UV:{2} 现网Svlan：{3} 现网Cvlan：{4} 现网UV：{5} ", task.Svlan, task.Cvlan, task.Uvlan, svlan, cvlan, uv);
                            break;
                        }
                    }
                    else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                    {
                        isDeleteIMS = false;
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
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        break;
                    }
                    step++;
                }
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
                bool isDeleteIMS = true;
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    #region step 4 删除业务
                    if (pair.Step == 4)
                    {
                        if (!isDeleteIMS)
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
                                        isDeleteIMS = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                        }
                        if (!isDeleteIMS)
                        {
                            pair.Msg.CompletionCode = CommandResponseStatus.DENY;
                            pair.Msg.ErrorDesc = ErrorDesc.ImsNotExist;
                            m_error = string.Format(" Svlan：{0} Cvlan：{1} UV:{2} 现网Svlan：{3} 现网Cvlan：{4} 现网UV：{5} ", task.Svlan, task.Cvlan, task.Uvlan, svlan, cvlan, uv);
                            break;
                        }
                    }
                    else if (pair.Step == 2 && pair.Msg.Information.Count <= 0)
                    {
                        isDeleteIMS = false;
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
                foreach (CommandAndResponsePair pair in m_commands)
                {
                    pair.Msg = ExecuteComm(pair.Command);
                    if (pair.Msg.CompletionCode == CommandResponseStatus.DENY)
                    {
                        pair.Msg.ParseErrorDesc(m_commBuilder, pair.Command.CommandText);
                        break;
                    }
                    step++;
                }
                #endregion
            }
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
