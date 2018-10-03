#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: BellCommandsBuilder
// 作    者：d.w
// 创建时间：2014/6/18 17:22:28
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
    public class BellCommandsBuilder : ManuCommandsBuilder
    {
        #region 错误信息描述
        public override ErrorDesc ParseErrorDesc(Dictionary<string, string> additionalInfo, string command)
        {
            #region
            //ADD-ONU::OLTID=10.215.255.6,PONID=1-1-11-4:CTAG::AUTHTYPE=LOID,ONUID=34557407876130000000,ONUTYPE=I-240E-Q;
            //EN=IRC;ENDESC="resource conflict";EADD="resource conflict";
            if (additionalInfo["EADD"].IndexOf("Timed out") >= 0)
                return ErrorDesc.OltNotExist;
            //add by d.w 2014-04-24
            if (command.IndexOf("DEL-ONU") >= 0 && additionalInfo["EN"] == "SEOF" && additionalInfo["ENDESC"] == "\"EMS operation failed\"" && additionalInfo["EADD"] == "\"Could not find data\"")
            {
                return ErrorDesc.OnuNotExist;
            }
            if (additionalInfo["EN"] == "SEOF" && additionalInfo["ENDESC"] == "\"EMS operation failed\"" && additionalInfo["EADD"] == "\"generate ONU Port error:Could not find data.\"")
            {
                if (command.IndexOf("DEL-ONU") >= 0)
                    return ErrorDesc.OnuNotExist;
                return ErrorDesc.OltNotExist;
            }
            //宿州一网通开通sip语音业务，北向自动激活模块做了升级。
            if (additionalInfo["EN"] == "SEOF" && additionalInfo["ENDESC"] == "\"EMS operation failed\"" && additionalInfo["EADD"] == "\"generate ONU Port error:Could not find data. failed to get onu AID, please check oltip, ponAID, MAC or LOID\"")
            {
                if (command.IndexOf("DEL-ONU") >= 0)
                    return ErrorDesc.OnuNotExist;
                return ErrorDesc.OltNotExist;
            }
            //Pon板
            if (additionalInfo["EN"] == "SEOF" && additionalInfo["ENDESC"] == "\"EMS operation failed\"" && additionalInfo["EADD"] == "\"generate ONU Port error:invalid boardType:EMPTY\"")
                return ErrorDesc.PonNotExist;
            if (additionalInfo["EN"] == "IIPE" && additionalInfo["ENDESC"] == "\"input parameter error\"" && additionalInfo["EADD"] == "\"The underlying facility has not been provisioned\"")
                return ErrorDesc.PonNotExist;
            if (additionalInfo["EN"] == "IIPE" && additionalInfo["ENDESC"] == "\"input parameter error\"" && additionalInfo["EADD"] == "\"Cannot configure ONT/ONT Services on non-PLT cards \"")
                return ErrorDesc.PonNotExist;
            //Pon口
            if (additionalInfo["EN"] == "IIPE" && additionalInfo["ENDESC"] == "\"input parameter error\"" && additionalInfo["EADD"] == "\"Value is not valid for the parameter.\"")
                return ErrorDesc.PonNotExist;
            if (additionalInfo["EN"] == "DDOF" && additionalInfo["ENDESC"] == "\"device operation failed" && additionalInfo["EADD"] == "\"Status, Stopped\"")
                return ErrorDesc.OnuTypeError;
            if (additionalInfo["ENDESC"] == "\"resource already exist\"" && additionalInfo["EADD"] == "\"Input, entity already exists\"")
                return ErrorDesc.OnuAlreadyExist;
            if (additionalInfo["EN"] == "DDOF" && additionalInfo["ENDESC"] == "\"device operation failed\"" && additionalInfo["EADD"] == "\"SNMP operation failed or returned an error\"")
                return ErrorDesc.SvlanError;
            if (additionalInfo["EN"] == "IIPE" && additionalInfo["ENDESC"] == "\"input parameter error\"" && additionalInfo["EADD"] == "\"Vlan ID does not exist\"")
                return ErrorDesc.SvlanError;
            return ErrorDesc.Unknown;
            #endregion
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
            string addOnu = string.Format("ADD-ONU::OLTID={0},PONID={1}:CTAG::AUTHTYPE=LOID,ONUID={2},ONUTYPE={3};",
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
            commands.Add(new Command(delOnu,TaskCommandType.DelOnu));
            return commands;
        }
        #endregion

        #region 业务配置
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
                    string delIPTV_DianBo = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=5,CCOS=5;",
                       task.OltID, task.PonID, task.OnuID, task.Svlan.ToString(), task.Cvlan.ToString(), task.Uvlan);
                    commands.Add(new Command(delIPTV_DianBo));
                                                         
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
                    task.OltID, task.PonID, task.OnuID, task.Svlan, task.Cvlan, task.Uvlan);
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
        protected override List<Command> ParseLstPortVlan(Task task)
        {
            List<Command> commands = new List<Command>();
            string lstPortVlan = string.Format("LST-PORTVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::;",
                task.OltID.Contains("255") ? task.OltName : task.OltID, task.PonID, task.OnuID);
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
            string addPonVlan = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},UV={4},SCOS=7,CCOS=7;",
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
            string delBuinssVlan = string.Format("DEL-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},UV={4},SCOS=7,CCOS=7;",
                     task.OltID, task.PonID, task.OnuID, task.Mvlan, task.Muvlan);
            commands.Add(new Command(delBuinssVlan, TaskCommandType.ManageBusiness));
            return commands;
        }
        #endregion

        #region 同装业务
        protected override List<Command> ParseSameInstall(Task task)
        {
            List<Command> commands = new List<Command>();
            //2.添加onu
            string addOnu = string.Format("ADD-ONU::OLTID={0},PONID={1}:CTAG::AUTHTYPE=LOID,ONUID={2},ONUTYPE={3};",
               task.OltID, task.PonID, task.OnuID, task.OnuType);
            commands.Add(new Command(addOnu, TaskCommandType.AddOnu));

            //3.新增管理业务流
            string addPonVlan = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},UV={4},SCOS=7,CCOS=7;",
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
                string delIPTV_DianBo = string.Format("ADD-PONVLAN::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::SVLAN={3},CVLAN={4},UV={5},SCOS=5,CCOS=5;",
                      task.OltID, task.PonID, task.OnuID, task.Svlan.ToString(), task.Cvlan.ToString(), task.Uvlan);
                commands.Add(new Command(delIPTV_DianBo));

                string addIPTV_ZuBo = string.Format("ADD-LANIPTVPORT::OLTID={0},PONID={1},ONUIDTYPE=LOID,ONUID={2}:CTAG::UV={3},MVLAN={4};",
                   task.OltID, task.PonID, task.OnuID, task.Uvlan, task.Mvlan);
                commands.Add(new Command(addIPTV_ZuBo));
            }
            return commands;

        }
        #endregion
    }
}
