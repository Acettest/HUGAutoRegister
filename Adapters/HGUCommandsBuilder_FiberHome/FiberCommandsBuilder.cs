#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: FiberHomeCommandsBuilder
// 作    者：d.w
// 创建时间：2014/6/18 17:41:07
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
    public class FiberCommandsBuilder : ManuCommandsBuilder
    {
        #region 错误描述
        /// <summary>
        /// 错误描述
        /// </summary>
        /// <param name="additionalInfo"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public override ErrorDesc ParseErrorDesc(Dictionary<string, string> additionalInfo, string command)
        {
            if (additionalInfo.ContainsKey("EADD"))
            {
                switch (additionalInfo["EADD"])
                {
                    case "olt ip not exist:ffff0021":
                        return ErrorDesc.OltNotExist;
                    case "input parameter error:ffff0020":
                        return ErrorDesc.PonNotExist;
                    case "onu already existed:ffff0035":
                        return ErrorDesc.OnuAlreadyExist;
                    case "onu not exist:ffff0004":
                    case "resource does not exist":
                        return ErrorDesc.OnuNotExist;
                    case "PON port authentication mode wrong or can't obtain:ffff002f":
                        return ErrorDesc.PonNotExist;
                    case "service not exist:ffff0011":
                        if (command.IndexOf("DEL-LANPORTVLAN") >= 0 && additionalInfo["ENDESC"] == "resource does not exist")
                            return ErrorDesc.BbNotExist;
                        else
                            return ErrorDesc.Unknown;
                    case "object not exist, please check"://UNM2000
                            return ErrorDesc.OnuNotExist;
                    default:
                        return ErrorDesc.Unknown;
                }
            }
            else
            {
                switch (additionalInfo["ENDESC"])
                {
                    case "resource does not exist":
                        return ErrorDesc.OnuNotExist;
                    case "device operation failed":
                        if (command.IndexOf("LST-POTS") >= 0)
                            return ErrorDesc.ImsNotExist;
                        else
                            return ErrorDesc.Unknown;
                    default:
                        return ErrorDesc.Unknown;
                }
            }
        }
        #endregion

        #region 查询ONU
        /// <summary>
        /// 查询ONU
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseLstOnu(Task task)
        {
            List<Command> commands = new List<Command>();                                           
            string lstOnu = string.Format("LST-ONU::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::;",
                task.OltID, task.PonID, task.OnuID);
            commands.Add(new Command(lstOnu, TaskCommandType.VerifyOnu));
            return commands;
        }
        #endregion

        #region 增加ONU
        /// <summary>
        /// 增加ONU
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseAddOnu(Task task)
        {
            List<Command> commands = new List<Command>();
            string addOnu = string.Format("ADD-ONU::OLTID={0},PONID={1}:CTAG::ONUIDTYPE=LOID,ONUID={2},ONUTYPE={3};",
                task.OltID, task.PonID, task.OnuID, task.OnuType);
            commands.Add(new Command(addOnu, TaskCommandType.AddOnu));
            return commands;
        }
        #endregion

        #region 删除ONU
        /// <summary>
        /// 删除ONU
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseDelOnu(Task task)
        {
            List<Command> commands = new List<Command>();
            string delOnu = string.Format("DEL-ONU::OLTID={0},PONID={1}:CTAG::ONUIDTYPE=LOID,ONUID={2};",
                task.OltID, task.PonID, task.OnuID);
            commands.Add(new Command(delOnu, TaskCommandType.DelOnu));
            return commands;
        }
        #endregion

        #region 业务配置
        /// <summary>
        /// 业务配置
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseAddTask(Task task)
        {
            List<Command> commands = new List<Command>();
            switch (task.Type)
            {
                case TaskType.NewBroadband:
                case TaskType.AddBroadband:
                    string addBroadband = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=0,CCOS=0;",
                       task.OltID, task.PonID, task.OnuID, task.Svlan.ToString(), task.Cvlan.ToString(), task.Uvlan);
                    commands.Add(new Command(addBroadband));
                    break;
                case TaskType.NewIMS:
                case TaskType.AddIMS:
                    string addIMS = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=6,CCOS=6;",
                       task.OltID, task.PonID, task.OnuID, task.Svlan.ToString(), task.Cvlan.ToString(), task.Uvlan);
                    commands.Add(new Command(addIMS));
                    break;
                case TaskType.NewIPTV:
                case TaskType.AddIPTV:
                    string addIPTV_DianBo = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=5,CCOS=5;",
                       task.OltID, task.PonID, task.OnuID, task.Svlan.ToString(), task.Cvlan.ToString(), task.Uvlan);
                    commands.Add(new Command(addIPTV_DianBo));

                    string addIPTV_ZuBo = string.Format("ADD-LANIPTVPORT::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::UV={3},MVLAN={4};",
                        task.OltID, task.PonID, task.OnuID, task.Uvlan, task.Mvlan);
                    commands.Add(new Command(addIPTV_ZuBo));
                    break;
                default:
                    return null;
            }
            return commands;
        }
        #endregion

        #region 删除业务
        /// <summary>
        /// 删除业务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseDelTask(Task task)
        {
            List<Command> commands = new List<Command>();
            switch (task.Type)
            {
                case TaskType.DelBroadband:          
                    string delBroadband = string.Format("DEL-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=0,CCOS=0;",
                   task.OltID, task.PonID, task.OnuID, task.Svlan, task.Cvlan, task.Uvlan);
                    commands.Add(new Command(delBroadband));
                    break;
                case TaskType.DelIMS:            
                    string delIMS = string.Format("DEL-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=6,CCOS=6;",
					   task.OltID, task.PonID, task.OnuID,task.Svlan, task.Cvlan, task.Uvlan);
                    commands.Add(new Command(delIMS));
                    break;
                case TaskType.DelIPTV:                     
                    string delIPTV_DianBo = string.Format("DEL-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=5,CCOS=5;",
                       task.OltID, task.PonID, task.OnuID, task.Svlan, task.Cvlan, task.Uvlan);
                    commands.Add(new Command(delIPTV_DianBo));
                                                    
                     string delIPTV_ZuBo = string.Format("DEL-LANIPTVPORT::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::UV={3},MVLAN={4};",
                        task.OltID, task.PonID, task.OnuID, task.Uvlan, task.Muvlan);
                    commands.Add(new Command(delIPTV_ZuBo));
                    break;
                default:
                    return null;
            }
            return commands;
        }
        #endregion

        #region 查询业务流
        /// <summary>
        /// 查询业务流
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseLstPortVlan(Task task)
        {
            List<Command> commands = new List<Command>();
            string lstPortVlan = string.Format("LST-PORTVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::;",
                task.OltID, task.PonID, task.OnuID);
            commands.Add(new Command(lstPortVlan, TaskCommandType.VerifyVlan));
            return commands;
        }
        #endregion

        #region 配置ONU上行口带宽限速
        /// <summary>
        /// 配置ONU上行口带宽限速
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseUPBandwidth(Task task)
        {
            List<Command> commands = new List<Command>();
            string upBandwidth = string.Format("CFG-ONUBW::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::UPBW={3};",
                task.OltID, task.PonID, task.OnuID, "FTTHUP100M");
            commands.Add(new Command(upBandwidth, TaskCommandType.UPBandwidth));
            return commands;
        }
        #endregion

        #region 新增管理业务流
        /// <summary>
        /// 新增管理业务流
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseAddManagBuinss(Task task)
        {
            List<Command> commands = new List<Command>();
            string addPonVlan = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::CVLAN={3},UV={4},CCOS=7;",
				task.OltID, task.PonID, task.OnuID,task.Mvlan,task.Muvlan);
            commands.Add(new Command(addPonVlan, TaskCommandType.ManageBusiness));
            return commands;
        }
        #endregion

        #region 删除管理业务流配置
        /// <summary>
        /// 删除管理业务流配置
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Command> ParseDelManagBuinss(Task task)
        {
            List<Command> commands = new List<Command>();
            string delBuinssVlan = string.Format("DEL-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::CVLAN={3},UV={4},CCOS=7;",
                     task.OltID, task.PonID, task.OnuID, task.Mvlan, task.Muvlan);
            commands.Add(new Command(delBuinssVlan, TaskCommandType.ManageBusiness));
            return commands;
        }
        #endregion

        #region 同装业务
        protected override List<Command> ParseSameInstall(Task task)
        {
            List<Command> commands = new List<Command>();
            //2.根据需要添加ONU
            string addOnu = string.Format("ADD-ONU::OLTID={0},PONID={1}:CTAG::ONUIDTYPE=LOID,ONUID={2},ONUTYPE={3};",
               task.OltID, task.PonID, task.OnuID, task.OnuType);
            commands.Add(new Command(addOnu, TaskCommandType.AddOnu));

            //3.新增管理业务流
            string addPonVlan = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::CVLAN={3},UV={4},CCOS=7;",
               task.OltID, task.PonID, task.OnuID, task.Mvlan, task.Muvlan);
            commands.Add(new Command(addPonVlan, TaskCommandType.ManageBusiness));

            //4.宽带业务配置
            string addBroadband = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=0,CCOS=0;",
                      task.OltID, task.PonID, task.OnuID, task.Svlan.ToString(), task.Cvlan.ToString(), task.Uvlan);
            commands.Add(new Command(addBroadband));

            //5.带宽限速配置
            string upBandwidth = string.Format("CFG-ONUBW::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::UPBW={3};",
               task.OltID, task.PonID, task.OnuID, "FTTHUP100M");
            commands.Add(new Command(upBandwidth, TaskCommandType.UPBandwidth));

            if (task.IsContainIMS == "Y")
            {
                //如果含有IMS业务，则添加ims指令
                string addIMS = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=6,CCOS=6;",
                             task.OltID, task.PonID, task.OnuID, task.IMSSvlan.ToString(), task.IMSCvlan.ToString(), task.IMSUV.ToString());
                commands.Add(new Command(addIMS));
            }

            if (task.IsContainIPTV == "Y")
            {
                //如果含有IPTV业务，则添加IPTV指令
                string addIPTV_DianBo = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=5,CCOS=5;",
                          task.OltID, task.PonID, task.OnuID, task.Svlan.ToString(), task.Cvlan.ToString(), task.Uvlan);
                commands.Add(new Command(addIPTV_DianBo));

                string addIPTV_ZuBo = string.Format("ADD-LANIPTVPORT::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::UV={3},MVLAN={4};",
                    task.OltID, task.PonID, task.OnuID, task.Uvlan, task.Mvlan);
                commands.Add(new Command(addIPTV_ZuBo));
            }
            return commands;
        }
        #endregion
    }
}
