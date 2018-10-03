#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: IdlUtil
// 作    者：d.w
// 创建时间：2014/6/12 16:16:40
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
using System.Data;

namespace HGU.Idl
{
    public class IdlUtil
    {
        #region SMethods
        public static string ParseTaskTypeToString(TaskType type)
        {
            switch (type)
            {
                case TaskType.NewBroadband:
                    return "宽带新装";
                case TaskType.NewIMS:
                    return "IMS新装";
                case TaskType.NewIPTV:
                    return "IPTV新装";
                case TaskType.AddBroadband:
                    return "宽带加装";
                case TaskType.AddIMS:
                    return "IMS加装";
                case TaskType.AddIPTV:
                    return "IPTV加装";
                case TaskType.DelONU:
                    return "ONU拆机";
                case TaskType.DelBroadband:
                    return "宽带拆机";
                case TaskType.DelIMS:
                    return "IMS拆机";
                case TaskType.DelIPTV:
                    return "IPTV拆机";
                case TaskType.RelocateBroadband:
                    return "宽带移机";
                case TaskType.RelocateIMS:
                    return "IMS移机";
                case TaskType.RelocateIPTV:
                    return "IPTV移机";
                case TaskType.SameInstall:
                    return "同装";
                default:
                    return "";
            }
        }

        public static TaskType ParseStringToTaskType(string type)
        {
            switch (type)
            {
                case "宽带新装":
                    return TaskType.NewBroadband;
                case "IMS新装":
                    return TaskType.NewIMS;
                case "IPTV新装":
                    return TaskType.NewIPTV;
                case "宽带加装":
                    return TaskType.AddBroadband;
                case "IMS加装":
                    return TaskType.AddIMS;
                case "IPTV加装":
                    return TaskType.AddIPTV;
                case "ONU拆机":
                    return TaskType.DelONU;
                case "宽带拆机":
                    return TaskType.DelBroadband;
                case "IMS拆机":
                    return TaskType.DelIMS;
                case "IPTV拆机":
                    return TaskType.DelIPTV;
                case "宽带移机":
                    return TaskType.RelocateBroadband;
                case "IMS移机":
                    return TaskType.RelocateIMS;
                case "IPTV移机":
                    return TaskType.RelocateIPTV;
                case "同装":
                    return TaskType.SameInstall;
                default:
                    throw new Exception("工单类型错误");
            }
        }

        public static TaskType ParseCodeToTaskType(string type)
        {
            switch (type)
            {
                case "NewBroadband":
                    return TaskType.NewBroadband;
                case "NewIMS":
                    return TaskType.NewIMS;
                case "NewIPTV":
                    return TaskType.NewIPTV;
                case "AddBroadband":
                    return TaskType.AddBroadband;
                case "AddIMS":
                    return TaskType.AddIMS;
                case "AddIPTV":
                    return TaskType.AddIPTV;
                case "DelONU":
                    return TaskType.DelONU;
                case "DelBroadband":
                    return TaskType.DelBroadband;
                case "DelIMS":
                    return TaskType.DelIMS;
                case "DelIPTV":
                    return TaskType.DelIPTV;
                case "RelocateBroadband":
                    return TaskType.RelocateBroadband;
                case "RelocateIMS":
                    return TaskType.RelocateIMS;
                case "RelocateIPTV":
                    return TaskType.RelocateIPTV;
                case "SameInstall":
                    return TaskType.SameInstall;
                default:
                    return TaskType.None;
            }
        }

        public static string ParseTaskStatusToString(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.NotStart:
                    return "待执行";
                case TaskStatus.Executing:
                    return "执行中";
                case TaskStatus.Succeed:
                    return "成功";
                case TaskStatus.Fail:
                    return "失败";
                default:
                    return "";
            }
        }

        public static TaskStatus ParseStringToTaskStatus(string status)
        {
            switch (status)
            {
                case "待执行":
                    return TaskStatus.NotStart;
                case "执行中":
                    return TaskStatus.Executing;
                case "成功":
                    return TaskStatus.Succeed;
                case "失败":
                    return TaskStatus.Fail;
                default:
                    return TaskStatus.NotStart;
            }
        }

        public static string ParseErrorDescToString(ErrorDesc desc, Task task, string error, string conn)
        {
            string oltName = SQLUtil.GetOlt(conn, task.OltID);
            if (oltName == null || oltName == "")
                oltName = task.OltID;
            switch (desc)
            {
                //            BbAlreadyExist,                        //宽带业务已存在
                //BbImsSvlanOrCvlanUnlike,               //宽带业务与现网不一致
                //SvlanOrCvlanException,                 //svlan or cvlan 业务流 异常
                //ImsSvlanOrCvlanUnlike,                 //语音业务与现网不一致
                //ImsPortAlreadyExist,                   //IMS端口被占用
                case ErrorDesc.OltNotExist:
                    return "数据错误：Olt " + oltName + " IP录入错误";
                case ErrorDesc.OltOffline:
                    return "数据错误：Olt " + oltName + " 与OMC网络中断";
                case ErrorDesc.PonNotExist:
                    return "数据错误：Olt " + oltName + " Pon口 " + task.PonID + " 不存在";
                case ErrorDesc.OnuAlreadyExist:
                    return "数据错误：OnuID " + task.OnuID + " 已存在";
                case ErrorDesc.OnuNotExist:
                    return "数据错误：Onu " + task.OnuID + " 不存在";
                case ErrorDesc.OnuTypeNotExist:
                    return "数据错误：Onu型号 " + task.OnuType + " 网管数据配置模型未建";
                case ErrorDesc.OnuTypeError:
                    return "数据错误：Onu型号 " + task.OnuType + " 错误";
                case ErrorDesc.SvlanError:
                    //                    
                    return "数据错误：Olt " + oltName + " Pon口 " +
                         task.PonID + " Svlan " + task.Svlan.ToString() +
                         " 或 Cvlan " + task.Cvlan.ToString() + " 不在olt vlan透传范围内，请核查";
                case ErrorDesc.BbAlreadyExist:
                    return "数据错误：ONU " + task.OnuID + " 中已绑定宽带业务，请核查网管数据" + error;
                case ErrorDesc.BbNotExist:
                    return "数据错误：ONU " + task.OnuID + " 中未绑定宽带业务，请核查资源";
                case ErrorDesc.BbSvlanOrCvlanUnlike:
                    return "数据错误：宽带业务流与现网不一致，请核查资源" + error;
                case ErrorDesc.SvlanOrCvlanException:
                    return "数据错误：ONU " + task.OnuID + " 中存在异常业务流，请核查网管数据" + error;
                case ErrorDesc.ImsSvlanOrCvlanUnlike:
                    return "数据错误：语音业务流与现网不一致，请核查资源" + error;
                case ErrorDesc.ImsPortAlreadyExist:
                    return "数据错误：ONU端口被占用" + error;
                case ErrorDesc.ImsPortUnlike:
                    return "数据错误：语音端口信息与现网不一致，请核查资源" + error;
                case ErrorDesc.ImsNotExist:
                    return "数据错误：ONU " + task.OnuID + " 中未绑定语音业务，请核查资源";
                case ErrorDesc.InformationException:
                    return "数据异常：" + error;
                case ErrorDesc.Unknown:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        public static Task ParseTaskFromDataRow(DataRow dr, string conn)
        {
            string manufacturer = dr["manufacturer"].ToString();
            string oltID = dr["oltID"].ToString();
            string oltName = string.Empty;
            //if (manufacturer == "贝尔" && oltID.Contains("255"))//贝尔已升级，不需要验证255的验证了
            //{
            //    oltName = SQLUtil.GetOlt(conn, oltID);
            //}
            Task task = new Task(
                    dr["taskID"].ToString(),
                    ParseStringToTaskType(dr["taskType"].ToString()),
                    dr["city"].ToString(),
                    dr["manufacturer"].ToString(),
                    dr["omcName"].ToString(),
                    oltID, oltName,
                    (dr["ponID"] == DBNull.Value ? null : dr["ponID"].ToString()),
                    (dr["onuID"] == DBNull.Value ? null : dr["onuID"].ToString()),
                    (dr["onuType"] == DBNull.Value ? null : dr["onuType"].ToString()),
                    (dr["svlan"] == DBNull.Value ? -1 : int.Parse(dr["svlan"].ToString())),
                    (dr["cvlan"] == DBNull.Value ? -1 : int.Parse(dr["cvlan"].ToString())),
                    dr["phone"].ToString(), DateTime.Parse(dr["receiveTime"].ToString()),
                    (dr["muvlan"] == DBNull.Value ? -1 : int.Parse(dr["muvlan"].ToString())),
                    (dr["ywvlan"] == DBNull.Value ? -1 : int.Parse(dr["ywvlan"].ToString())),
                    (dr["mvlan"] == DBNull.Value ? -1 : int.Parse(dr["mvlan"].ToString())),
                    (dr["feNumber"] == DBNull.Value ? -1 : int.Parse(dr["feNumber"].ToString())),
                    (dr["potsNumber"] == DBNull.Value ? -1 : int.Parse(dr["potsNumber"].ToString())),
                    (dr["IsContainIMS"] == DBNull.Value ? null : dr["IsContainIMS"].ToString()),
                    (dr["imsSvlan"] == DBNull.Value ? -1 : int.Parse(dr["imsSvlan"].ToString())),
                    (dr["imsCvlan"] == DBNull.Value ? -1 : int.Parse(dr["imsCvlan"].ToString())),
                    (dr["imsUV"] == DBNull.Value ? -1 : int.Parse(dr["imsUV"].ToString())),
                    (dr["IsContainIPTV"] == DBNull.Value ? null : dr["IsContainIPTV"].ToString()),
                    (dr["iptvSvlan"] == DBNull.Value ? -1 : int.Parse(dr["iptvSvlan"].ToString())),
                    (dr["iptvCvlan"] == DBNull.Value ? -1 : int.Parse(dr["iptvCvlan"].ToString())),
                    (dr["iptvUV"] == DBNull.Value ? -1 : int.Parse(dr["iptvUV"].ToString()))
                );
            if (dr["id"] != DBNull.Value)
                task.Id = long.Parse(dr["id"].ToString());
            if (dr["netInterrupt"] != DBNull.Value)
                task.NetInterrupt = bool.Parse(dr["netInterrupt"].ToString());
            if (dr.Table.Columns.Contains("oltOffline") && dr["oltOffline"] != DBNull.Value)
                task.OltOffline = bool.Parse(dr["oltOffline"].ToString());
            if (dr.Table.Columns.Contains("isRollbackHisTask") && dr["isRollbackHisTask"] != DBNull.Value)
            {
                task.IsRollbackHisTask = bool.Parse(dr["isRollbackHisTask"].ToString());
                task.OldTaskType = ParseStringToTaskType(dr["oldTaskType"].ToString());
            }
            else
                task.OldTaskType = TaskType.None;
            return task;
        }

        public static ReplyBossMsg ParseReplyFromDataRow(DataRow dr)
        {
            return new ReplyBossMsg(
                dr["taskID"].ToString(),
                ParseStringToTaskStatus(dr["taskStatus"].ToString()),
                dr["responseMsg"].ToString(),
                dr["netDelay"] == DBNull.Value ? false : bool.Parse(dr["netDelay"].ToString()),
                DateTime.Parse(dr["receiveTime"].ToString()));
        }

        public static string ConvertToReslutXML(int status, string msg)
        {
            return string.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<Body>\n<Status>{0}</Status>\n<Message>{1}</Message>\n</Body>", status, msg);
        }
        #endregion
    }
}
