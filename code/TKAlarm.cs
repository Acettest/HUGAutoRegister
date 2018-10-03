using System;
using System.Data;
using System.Threading;

namespace TK_AlarmManagement
{
	/// <summary>
	/// TKAlarm 的摘要说明。
	/// </summary>
    /// 
    [Serializable]
	public class TKAlarm
	{
		public ulong TKSn				= 0;						//集中告警流水号
		public string ManuSn			= "";						//厂商告警流水号
		public string City				= "";						//告警城市
		public string Manufacturer		= "";						//设备厂商
        public string BusinessType      = "";                       //业务类型
		public string NeName			= "";						//网元名称
		public string NeType			= "";						//网元类型
		public string ObjName			= "";						//对象名称
		public string ObjType			= "";						//对象类型
		public string AlarmName			= "";						//告警名称
		public string Redefinition		= "";						//重定义的告警名称
		public string Category			= "";						//告警种类
		public string Severity			= "";						//告警级别
		public string OccurTime			= "";						//告警发生时间
		public string AckTimeLV1		= "";						//告警确认时间
		public string AckAgainTimeLV1	= "";
		public string AckTimeLV2		= "";						//再次确认时间
		public string AckAgainTimeLV2	= "";
		public string ClearTime			= "";						//告警恢复时间
		public string Location			= "";						//告警定位信息
		public string OperatorLV11		= "";						//操作员信息
		public string OperatorLV12		= "";						//操作员信息
		public string OperatorLV21		= "";
		public string OperatorLV22		= "";
		public string ProjectInfo		= "";						//工程信息
		public string OrderOperatorLV1	= "";						//派单人LV1
		public string OrderIDLV1		= "";						//派单idLV1
		public string OrderTimeLV1		= "";						//派单时间LV1
		public string OrderOperatorLV2	= "";						//派单人LV2
		public string OrderIDLV2		= "";						//派单idLV2
		public string OrderTimeLV2		= "";						//派单时间LV2

        public string ReceiveTime       = "";                       //告警接受时间
        public string ProjectEndTime = "";						    //工程告警的工程结束时间，用于判断是否为超时工程告警，不发往客户端
        public string ProjectTimeOut = "0";						    //工程告警超时标识

        public string OMCName = ""; 
        public string Reserved2 = ""; //保留字段
        public string Reserved3 = ""; //保留字段

        //public static object g_lockTKSN = new int();
		//public static ulong lTKSn;

        public System.Threading.ReaderWriterLock SyncRoot = new ReaderWriterLock();
		public TKAlarm()
		{
		}

        public TKAlarm(TKAlarm alm)
        {
            try
            {
                alm.SyncRoot.AcquireReaderLock(-1);
                TKSn = alm.TKSn;
                ManuSn = alm.ManuSn;
                City = alm.City;
                Manufacturer = alm.Manufacturer;
                BusinessType = alm.BusinessType;
                NeName = alm.NeName;
                NeType = alm.NeType;
                ObjName = alm.ObjName;
                ObjType = alm.ObjType;
                AlarmName = alm.AlarmName;
                Redefinition = alm.Redefinition;
                Category = alm.Category;
                Severity = alm.Severity;
                OccurTime = alm.OccurTime;
                AckTimeLV1 = alm.AckTimeLV1;
                AckAgainTimeLV1 = alm.AckAgainTimeLV1;
                AckTimeLV2 = alm.AckTimeLV2;
                AckAgainTimeLV2 = alm.AckAgainTimeLV2;
                ClearTime = alm.ClearTime;
                Location = alm.Location;
                OperatorLV11 = alm.OperatorLV11;
                OperatorLV12 = alm.OperatorLV12;
                OperatorLV21 = alm.OperatorLV21;
                OperatorLV22 = alm.OperatorLV22;
                ProjectInfo = alm.ProjectInfo;
                OrderOperatorLV1 = alm.OrderOperatorLV1;
                OrderIDLV1 = alm.OrderIDLV1;
                OrderTimeLV1 = alm.OrderTimeLV1;
                OrderOperatorLV2 = alm.OrderOperatorLV2;
                OrderIDLV2 = alm.OrderIDLV2;
                OrderTimeLV2 = alm.OrderTimeLV2;
                OMCName = alm.OMCName;
                Reserved2 = alm.Reserved2;
                Reserved3 = alm.Reserved3;
                ProjectTimeOut = alm.ProjectTimeOut;
                ProjectEndTime = alm.ProjectEndTime;
                ReceiveTime = alm.ReceiveTime;
            }
            finally
            {
                alm.SyncRoot.ReleaseReaderLock();
            }
        }

		#region 废弃-获取告警字符串
		/*
        public string GetAlarmString()
		{
            try
            {
                SyncRoot.AcquireReaderLock(-1);

                System.Text.StringBuilder alarmstring = new System.Text.StringBuilder();
                alarmstring.Append("<++++>\r\n");
                alarmstring.Append("COMMAND = ALARM_REPORT\r\n");
                alarmstring.Append("集中告警流水号 = ");
                alarmstring.Append(TKSn.ToString());
                alarmstring.Append("\r\n");
                alarmstring.Append("厂商告警流水号 = ");
                alarmstring.Append(ManuSn);
                alarmstring.Append("\r\n");
                alarmstring.Append("告警城市 = ");
                alarmstring.Append(City);
                alarmstring.Append("\r\n");
                alarmstring.Append("设备厂商 = ");
                alarmstring.Append(Manufacturer);
                alarmstring.Append("\r\n");
                alarmstring.Append("业务类型 = ");
                alarmstring.Append(BusinessType);
                alarmstring.Append("\r\n");
                alarmstring.Append("网元名称 = ");
                alarmstring.Append(NeName);
                alarmstring.Append("\r\n");
                alarmstring.Append("网元类型 = ");
                alarmstring.Append(NeType);
                alarmstring.Append("\r\n");
                alarmstring.Append("对象名称 = ");
                alarmstring.Append(ObjName);
                alarmstring.Append("\r\n");
                alarmstring.Append("对象类型 = ");
                alarmstring.Append(ObjType);
                alarmstring.Append("\r\n");
                alarmstring.Append("告警名称 = ");
                alarmstring.Append(AlarmName);
                alarmstring.Append("\r\n");
                alarmstring.Append("重定义告警名称 = ");
                alarmstring.Append(Redefinition);
                alarmstring.Append("\r\n");
                //alarmstring.Append("告警种类 = ");
                //alarmstring.Append(Category);
                //alarmstring.Append("\r\n");
                alarmstring.Append("告警级别 = ");
                alarmstring.Append(Severity);
                alarmstring.Append("\r\n");
                alarmstring.Append("告警发生时间 = ");
                alarmstring.Append(OccurTime);
                alarmstring.Append("\r\n");
                alarmstring.Append("告警确认时间LV1 = ");
                alarmstring.Append(AckTimeLV1);
                alarmstring.Append("\r\n");
                alarmstring.Append("再次确认时间LV1 = ");
                alarmstring.Append(AckAgainTimeLV1);
                alarmstring.Append("\r\n");
                alarmstring.Append("告警确认时间LV2 = ");
                alarmstring.Append(AckTimeLV2);
                alarmstring.Append("\r\n");
                alarmstring.Append("再次确认时间LV2 = ");
                alarmstring.Append(AckAgainTimeLV2);
                alarmstring.Append("\r\n");
                alarmstring.Append("告警恢复时间 = ");
                alarmstring.Append(ClearTime);
                alarmstring.Append("\r\n");
                alarmstring.Append("告警定位信息 = ");
                alarmstring.Append(Location);
                alarmstring.Append("\r\n");
                alarmstring.Append("操作员信息LV11 = ");
                alarmstring.Append(OperatorLV11);
                alarmstring.Append("\r\n");
                alarmstring.Append("操作员信息LV12 = ");
                alarmstring.Append(OperatorLV12);
                alarmstring.Append("\r\n");
                alarmstring.Append("操作员信息LV21 = ");
                alarmstring.Append(OperatorLV21);
                alarmstring.Append("\r\n");
                alarmstring.Append("操作员信息LV22 = ");
                alarmstring.Append(OperatorLV22);
                alarmstring.Append("\r\n");
                alarmstring.Append("工程上报信息 = ");
                alarmstring.Append(ProjectInfo);
                alarmstring.Append("\r\n");
                alarmstring.Append("派单人LV1 = ");
                alarmstring.Append(OrderOperatorLV1);
                alarmstring.Append("\r\n");
                alarmstring.Append("派单号LV1 = ");
                alarmstring.Append(OrderIDLV1);
                alarmstring.Append("\r\n");
                alarmstring.Append("派单时间LV1 = ");
                alarmstring.Append(OrderTimeLV1);
                alarmstring.Append("\r\n");
                alarmstring.Append("派单人LV2 = ");
                alarmstring.Append(OrderOperatorLV2);
                alarmstring.Append("\r\n");
                alarmstring.Append("派单号LV2 = ");
                alarmstring.Append(OrderIDLV2);
                alarmstring.Append("\r\n");
                alarmstring.Append("派单时间LV2 = ");
                alarmstring.Append(OrderTimeLV2);
                alarmstring.Append("\r\n");
                alarmstring.Append("OMCName = ");
                alarmstring.Append(OMCName);
                alarmstring.Append("\r\n");
                alarmstring.Append("Reserved2 = ");
                alarmstring.Append(Reserved2);
                alarmstring.Append("\r\n");
                alarmstring.Append("Reserved3 = ");
                alarmstring.Append(Reserved3);
                alarmstring.Append("\r\n");
                alarmstring.Append("接收时间 = ");
                alarmstring.Append(ReceiveTime);
                alarmstring.Append("\r\n");
                alarmstring.Append("<---->\r\n");

                return alarmstring.ToString();
            }
            finally
            {
                SyncRoot.ReleaseReaderLock();
            }
        }
         * */
        #endregion

        public void ConvertFromDB(DataRow dr)
        {
            try
            {
                SyncRoot.AcquireWriterLock(-1);

                TKSn = Convert.ToUInt64(dr["TKsn"].ToString());
                ManuSn = dr["ManuSn"].ToString().Trim();
                City = dr["City"].ToString().Trim();
                Manufacturer = dr["Manufacturer"].ToString().Trim();
                BusinessType = dr["BusinessType"].ToString().Trim();
                NeName = dr["NeName"].ToString().Trim();
                NeType = dr["NeType"].ToString().Trim();
                ObjName = dr["ObjName"].ToString().Trim();
                ObjType = dr["ObjType"].ToString().Trim();
                AlarmName = dr["AlarmName"].ToString().Trim();
                Redefinition = dr["Redefinition"].ToString().Trim();
                Severity = dr["Severity"].ToString().Trim();
                OccurTime = dr["OccurTime"].ToString().Trim();
                AckTimeLV1 = dr["AckTimeLV1"].ToString().Trim();
                AckAgainTimeLV1 = dr["AckAgainTimeLV1"].ToString().Trim();
                AckTimeLV2 = dr["AckTimeLV2"].ToString().Trim();
                AckAgainTimeLV2 = dr["AckAgainTimeLV2"].ToString().Trim();
                ClearTime = dr["ClearTime"].ToString().Trim();
                Location = dr["Location"].ToString().Trim();
                OperatorLV11 = dr["OperatorLV11"].ToString().Trim();
                OperatorLV12 = dr["OperatorLV12"].ToString().Trim();
                OperatorLV21 = dr["OperatorLV21"].ToString().Trim();
                OperatorLV22 = dr["OperatorLV22"].ToString().Trim();
                ProjectInfo = dr["ProjectInfo"].ToString().Trim();
                OrderOperatorLV1 = dr["OrderOperatorLV1"].ToString().Trim();
                OrderIDLV1 = dr["OrderIDLV1"].ToString().Trim();
                OrderTimeLV1 = dr["OrderTimeLV1"].ToString().Trim();
                OrderOperatorLV2 = dr["OrderOperatorLV2"].ToString().Trim();
                OrderIDLV2 = dr["OrderIDLV2"].ToString().Trim();
                OrderTimeLV2 = dr["OrderTimeLV2"].ToString().Trim();
                OMCName = dr["OMCName"].ToString().Trim();
                Reserved2 = dr["Reserved2"].ToString().Trim();
                Reserved3 = dr["Reserved3"].ToString().Trim();
                ReceiveTime = dr["ReceiveTime"].ToString().Trim();
            }
            finally
            {
                SyncRoot.ReleaseWriterLock();
            }
        }

        //获取告警字符串
        public CommandMsgV2 ConvertToMsg()
        {
            try
            {
                SyncRoot.AcquireReaderLock(-1);

                CommandMsgV2 msg = new CommandMsgV2();
                msg.TK_CommandType = Constants.TK_CommandType.ALARM_REPORT;
                msg.SeqID = CommandProcessor.AllocateID();

                msg.SetValue("集中告警流水号", TKSn.ToString());
                msg.SetValue("厂商告警流水号", ManuSn);
                msg.SetValue("告警城市", City);
                msg.SetValue("设备厂商", Manufacturer);
                msg.SetValue("业务类型", BusinessType);
                msg.SetValue("网元名称", NeName);
                msg.SetValue("网元类型", NeType);
                msg.SetValue("对象名称", ObjName);
                msg.SetValue("对象类型", ObjType);
                msg.SetValue("告警名称", AlarmName);
                msg.SetValue("重定义告警名称", Redefinition);
                //msg.SetValue("告警种类", Category);
                msg.SetValue("告警级别", Severity);
                msg.SetValue("告警发生时间", OccurTime);
                msg.SetValue("告警确认时间LV1", AckTimeLV1);
                msg.SetValue("再次确认时间LV1", AckAgainTimeLV1);
                msg.SetValue("告警确认时间LV2", AckTimeLV2);
                msg.SetValue("再次确认时间LV2", AckAgainTimeLV2);
                msg.SetValue("告警恢复时间", ClearTime);
                msg.SetValue("告警定位信息", Location);
                msg.SetValue("操作员信息LV11", OperatorLV11);
                msg.SetValue("操作员信息LV12", OperatorLV12);
                msg.SetValue("操作员信息LV21", OperatorLV21);
                msg.SetValue("操作员信息LV22", OperatorLV22);
                msg.SetValue("工程上报信息", ProjectInfo);
                msg.SetValue("派单人LV1", OrderOperatorLV1);
                msg.SetValue("派单号LV1", OrderIDLV1);
                msg.SetValue("派单时间LV1", OrderTimeLV1);
                msg.SetValue("派单人LV2", OrderOperatorLV2);
                msg.SetValue("派单号LV2", OrderIDLV2);
                msg.SetValue("派单时间LV2", OrderTimeLV2);
                msg.SetValue("OMCName", OMCName);
                msg.SetValue("Reserved2", Reserved2);
                msg.SetValue("Reserved3", Reserved3);
                msg.SetValue("接收时间", ReceiveTime);
                msg.SetValue("工程超时", ProjectTimeOut);

                return msg;
            }
            finally
            {
                SyncRoot.ReleaseReaderLock();
            }
        }

        public void ConvertFromMsg(ICommunicationMessage msg)
        {
            try
            {
                if (msg.TK_CommandType != Constants.TK_CommandType.ALARM_REPORT
                    && msg.TK_CommandType != Constants.TK_CommandType.ADAPTER_ALARM_REPORT
                    && msg.TK_CommandType != Constants.TK_CommandType.ALARM_ORDER_CHANGE)
                    throw new Exception("无效告警报文.");

                SyncRoot.AcquireWriterLock(-1);

                    if (!msg.Contains("集中告警流水号"))
                        throw new Exception("告警报文没有流水号.");

                    foreach (string key in msg.GetKeys())
                    {
                        switch (key)
                        {
                            case "集中告警流水号":
                                TKSn = Convert.ToUInt64(msg.GetValue(key).ToString());
                                break;
                            case "厂商告警流水号":
                                ManuSn = msg.GetValue(key).ToString();
                                break;
                            case "告警城市":
                                City = msg.GetValue(key).ToString();
                                break;
                            case "设备厂商":
                                Manufacturer = msg.GetValue(key).ToString();
                                break;
                            case "业务类型":
                                BusinessType = msg.GetValue(key).ToString();
                                break;
                            case "网元名称":
                                NeName = msg.GetValue(key).ToString();
                                break;
                            case "网元类型":
                                NeType = msg.GetValue(key).ToString();
                                break;
                            case "对象名称":
                                ObjName = msg.GetValue(key).ToString();
                                break;
                            case "对象类型":
                                ObjType = msg.GetValue(key).ToString();
                                break;
                            case "告警名称":
                                AlarmName = msg.GetValue(key).ToString();
                                break;
                            case "重定义告警名称":
                                Redefinition = msg.GetValue(key).ToString();
                                break;
                            //case "告警种类":
                            //    Category = msg.GetValue(key).ToString();
                            //    break;
                            case "告警级别":
                                Severity = msg.GetValue(key).ToString();
                                break;
                            case "告警发生时间":
                                OccurTime = msg.GetValue(key).ToString();
                                break;
                            case "告警确认时间LV1":
                                AckTimeLV1 = msg.GetValue(key).ToString();
                                break;
                            case "再次确认时间LV1":
                                AckAgainTimeLV1 = msg.GetValue(key).ToString();
                                break;
                            case "告警确认时间LV2":
                                AckTimeLV2 = msg.GetValue(key).ToString();
                                break;
                            case "再次确认时间LV2":
                                AckAgainTimeLV2 = msg.GetValue(key).ToString();
                                break;
                            case "告警恢复时间":
                                ClearTime = msg.GetValue(key).ToString();
                                break;
                            case "告警定位信息":
                                Location = msg.GetValue(key).ToString();
                                break;
                            case "操作员信息LV11":
                                OperatorLV11 = msg.GetValue(key).ToString();
                                break;
                            case "操作员信息LV12":
                                OperatorLV12 = msg.GetValue(key).ToString();
                                break;
                            case "操作员信息LV21":
                                OperatorLV21 = msg.GetValue(key).ToString();
                                break;
                            case "操作员信息LV22":
                                OperatorLV22 = msg.GetValue(key).ToString();
                                break;
                            case "工程上报信息":
                                ProjectInfo = msg.GetValue(key).ToString();
                                break;
                            case "工程超时":
                                ProjectTimeOut = msg.GetValue(key).ToString();
                                break;
                            case "派单人LV1":
                                OrderOperatorLV1 = msg.GetValue(key).ToString();
                                break;
                            case "派单号LV1":
                                OrderIDLV1 = msg.GetValue(key).ToString();
                                break;
                            case "派单时间LV1":
                                OrderTimeLV1 = msg.GetValue(key).ToString();
                                break;
                            case "派单人LV2":
                                OrderOperatorLV2 = msg.GetValue(key).ToString();
                                break;
                            case "派单号LV2":
                                OrderIDLV2 = msg.GetValue(key).ToString();
                                break;
                            case "派单时间LV2":
                                OrderTimeLV2 = msg.GetValue(key).ToString();
                                break;
                            case "OMCName":
                                OMCName = msg.GetValue(key).ToString();
                                break;
                            case "Reserved2":
                                Reserved2 = msg.GetValue(key).ToString();
                                break;
                            case "Reserved3":
                                Reserved3 = msg.GetValue(key).ToString();
                                break;
                            case "接收时间":
                                ReceiveTime = msg.GetValue(key).ToString();
                                break;
                            default:
                                break;
                        }
                    }
            }
            finally
            {
                SyncRoot.ReleaseWriterLock();
            }
        }

		//将字符串转换成一条告警
		public void StringToAlarm(string sAlarm)
		{
            if (sAlarm == "")
                return;

            try
            {
                SyncRoot.AcquireWriterLock(-1);

                int nBegin = 0;
                int nEnd = 0;
                string sLeft = "";
                string sRight = "";

                try
                {
                    nBegin = sAlarm.IndexOf("\r\n", nEnd);
                    if (-1 == nBegin)
                        return;
                    nBegin += 2;

                    while (true)
                    {
                        nEnd = sAlarm.IndexOf("=", nBegin);
                        if (-1 == nEnd)
                            break;

                        sLeft = sAlarm.Substring(nBegin, nEnd - nBegin);

                        nBegin = nEnd;

                        nEnd = sAlarm.IndexOf("\r\n", nBegin);

                        if (-1 == nEnd)
                            break;

                        sRight = sAlarm.Substring(nBegin + 1, nEnd - nBegin - 1);

                        switch (sLeft)
                        {
                            case "COMMAND":
                                if (sRight.Trim() != "ALARM" && sRight.Trim() != Constants.MSG_TYPE_ADAPTER_ALARM_REPORT && sRight.Trim() != Constants.MSG_TYPE_ALARM_REPORT)
                                    return;
                                break;
                            case "集中告警流水号":
                                TKSn = Convert.ToUInt64(sRight);
                                break;
                            case "厂商告警流水号":
                                ManuSn = sRight.Trim();
                                break;
                            case "告警城市":
                                City = sRight.Trim();
                                break;
                            case "设备厂商":
                                Manufacturer = sRight.Trim();
                                break;
                            case "业务类型":
                                BusinessType = sRight.Trim();
                                break;
                            case "网元名称":
                                NeName = sRight.Trim();
                                break;
                            case "网元类型":
                                NeType = sRight.Trim();
                                break;
                            case "对象名称":
                                ObjName = sRight.Trim();
                                break;
                            case "对象类型":
                                ObjType = sRight.Trim();
                                break;
                            case "告警名称":
                                AlarmName = sRight.Trim();
                                break;
                            case "重定义告警名称":
                                Redefinition = sRight.Trim();
                                break;
                            //case "告警种类":
                            //    Category = sRight.Trim();
                            //    break;
                            case "告警级别":
                                Severity = sRight.Trim();
                                break;
                            case "告警发生时间":
                                OccurTime = sRight.Trim();
                                break;
                            case "告警确认时间LV1":
                                AckTimeLV1 = sRight.Trim();
                                break;
                            case "再次确认时间LV1":
                                AckAgainTimeLV1 = sRight.Trim();
                                break;
                            case "告警确认时间LV2":
                                AckTimeLV2 = sRight.Trim();
                                break;
                            case "再次确认时间LV2":
                                AckAgainTimeLV2 = sRight.Trim();
                                break;
                            case "告警恢复时间":
                                ClearTime = sRight.Trim();
                                break;
                            case "告警定位信息":
                                Location = sRight.Trim();
                                break;
                            case "操作员信息LV11":
                                OperatorLV11 = sRight.Trim();
                                break;
                            case "操作员信息LV12":
                                OperatorLV12 = sRight.Trim();
                                break;
                            case "操作员信息LV21":
                                OperatorLV21 = sRight.Trim();
                                break;
                            case "操作员信息LV22":
                                OperatorLV22 = sRight.Trim();
                                break;
                            case "工程上报信息":
                                ProjectInfo = sRight.Trim();
                                break;
                            case "派单人LV1":
                                OrderOperatorLV1 = sRight.Trim();
                                break;
                            case "派单号LV1":
                                OrderIDLV1 = sRight.Trim();
                                break;
                            case "派单时间LV1":
                                OrderTimeLV1 = sRight.Trim();
                                break;
                            case "派单人LV2":
                                OrderOperatorLV2 = sRight.Trim();
                                break;
                            case "派单号LV2":
                                OrderIDLV2 = sRight.Trim();
                                break;
                            case "派单时间LV2":
                                OrderTimeLV2 = sRight.Trim();
                                break;
                            case "OMCName":
                                OMCName = sRight.Trim();
                                break;
                            case "Reserved2":
                                Reserved2 = sRight.Trim();
                                break;
                            case "Reserved3":
                                Reserved3 = sRight.Trim();
                                break;
                            case "接收时间":
                                ReceiveTime = sRight.Trim();
                                break;
                            default:
                                break;

                        }

                        nBegin = nEnd + 2;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            finally
            {
                SyncRoot.ReleaseWriterLock();
            }
		}

		//
		public string GetSql()
		{
			if(ClearTime != "")
				return GetResumeAlarmSql();
			if(OccurTime != "")
				return GetActiveAlarmSql();
			return "";
        }

        //生成告警事件的SQL语句
        public string GetEventSql()
        {
            try
            {
                SyncRoot.AcquireReaderLock(-1);

                string sql = "";

                string[] empty = Location.Split(new string[] { "热点区域:","热点名称:", "中继器IP:" },StringSplitOptions.RemoveEmptyEntries);
                sql = "insert into AlarmEvent (EventState,TKSn,fetchTime,clearTime,nename,terminal_name,server_ip,alarmName,Severity,Location,description,OMCName,TaskID)";
                sql += " values(" + (ClearTime == "" ? "'0'" : "'1'");
                sql += ", '" + TKSn + "'";
                sql += (OccurTime == "" ? ",null" : (",'" + OccurTime + "'"));
                sql += (ClearTime == "" ? ",null" : (",'" + ClearTime + "'"));
                sql += (NeName == "" ? ",''" : (",'" + NeName + "'"));
                sql += (empty.Length != 3 ? ",''" : (",'" + empty[1].ToString().Trim() + "'"));
                sql += (empty.Length != 3 ? ",''" : (",'" + empty[2].ToString().Trim() + "'"));
                sql += (AlarmName == "" ? ",''" : (",'" + AlarmName.Replace("'", "''") + "'"));
                sql += (Severity == "" ? ",''" : (",'" + Severity + "'"));
                sql += (Location == "" ? ",''" : (",'" + Location.Replace("'", "''") + "'"));
                sql += (Reserved2 == "" ? ",''" : (",'" + Reserved2.Replace("'", "''") + "'"));
                sql += (OMCName == "" ? ",''" : (",'" + OMCName + "'"));
                sql += (Reserved3 == "" ? ",''" : (",'" + Reserved3 + "'"));

                sql += ")";

                return sql;
            }
            finally
            {
                SyncRoot.ReleaseReaderLock();
            }
        }

		private string GetActiveAlarmSql()
		{
            try
            {
                SyncRoot.AcquireReaderLock(-1);

                string sql = "";

                sql = "insert into ActiveAlarm (TKSn, ManuSn ";
                sql += (City == "" ? "" : ",City");
                sql += (Manufacturer == "" ? "" : ",Manufacturer");
                sql += (BusinessType == "" ? "" : ",BusinessType");
                sql += (NeName == "" ? "" : ",NeName");
                sql += (NeType == "" ? "" : ",NeType");
                sql += (ObjName == "" ? "" : ",ObjName");
                sql += (ObjType == "" ? "" : ",ObjType");
                sql += (AlarmName == "" ? "" : ",AlarmName");
                sql += (Redefinition == "" ? "" : ",Redefinition");
                //sql += (Category == "" ? "" : ",Category");
                sql += (Severity == "" ? "" : ",Severity");
                sql += (OccurTime == "" ? "" : ",OccurTime");
                sql += (AckTimeLV1 == "" ? "" : ",AckTimeLV1");
                sql += (AckTimeLV2 == "" ? "" : ",AckTimeLV2");
                sql += (AckAgainTimeLV1 == "" ? "" : ",AckAgainTimeLV1");
                sql += (AckAgainTimeLV2 == "" ? "" : ",AckAgainTimeLV2");
                sql += (ClearTime == "" ? "" : ",ClearTime");
                sql += (Location == "" ? "" : ",Location");
                sql += (OperatorLV11 == "" ? "" : ",OperatorLV11");
                sql += (OperatorLV12 == "" ? "" : ",OperatorLV12");
                sql += (OperatorLV21 == "" ? "" : ",OperatorLV21");
                sql += (OperatorLV22 == "" ? "" : ",OperatorLV22");
                sql += (ProjectInfo == "" ? "" : ",ProjectInfo");
                sql += (OrderOperatorLV1 == "" ? "" : ",OrderOperatorLV1");
                sql += (OrderIDLV1 == "" ? "" : ",OrderIDLV1");
                sql += (OrderTimeLV1 == "" ? "" : ",OrderTimeLV1");
                sql += (OrderOperatorLV2 == "" ? "" : ",OrderOperatorLV2");
                sql += (OrderIDLV2 == "" ? "" : ",OrderIDLV2");
                sql += (OrderTimeLV2 == "" ? "" : ",OrderTimeLV2");
                sql += (OMCName == "" ? "" : ",OMCName");
                sql += (Reserved2 == "" ? "" : ",Reserved2");
                sql += (Reserved3 == "" ? "" : ",Reserved3");
                sql += (ReceiveTime == "" ? "" : ",ReceiveTime");

                sql += ")";

                sql += " values('" + TKSn.ToString() + "', '" + ManuSn + "'";
                sql += (City == "" ? "" : (", '" + City + "'"));
                sql += (Manufacturer == "" ? "" : (", '" + Manufacturer + "'"));
                sql += (BusinessType == "" ? "" : (", '" + BusinessType + "'"));
                sql += (NeName == "" ? "" : (", '" + NeName + "'"));
                sql += (NeType == "" ? "" : (", '" + NeType + "'"));
                sql += (ObjName == "" ? "" : (", '" + ObjName + "'"));
                sql += (ObjType == "" ? "" : (", '" + ObjType + "'"));
                sql += (AlarmName == "" ? "" : (", '" + AlarmName.Replace("'", "''") + "'"));
                sql += (Redefinition == "" ? "" : (", '" + Redefinition.Replace("'", "''") + "'"));
                //sql += (Category == "" ? "" : (", '" + Category + "'"));
                sql += (Severity == "" ? "" : (", '" + Severity + "'"));
                sql += (OccurTime == "" ? "" : (", '" + OccurTime + "'"));
                sql += (AckTimeLV1 == "" ? "" : (", '" + AckTimeLV1 + "'"));
                sql += (AckTimeLV2 == "" ? "" : (", '" + AckTimeLV2 + "'"));
                sql += (AckAgainTimeLV1 == "" ? "" : (", '" + AckAgainTimeLV1 + "'"));
                sql += (AckAgainTimeLV2 == "" ? "" : (", '" + AckAgainTimeLV2 + "'"));
                sql += (ClearTime == "" ? "" : (", '" + ClearTime + "'"));
                sql += (Location == "" ? "" : (", '" + Location.Replace("'", "''") + "'"));
                sql += (OperatorLV11 == "" ? "" : (", '" + OperatorLV11 + "'"));
                sql += (OperatorLV12 == "" ? "" : (", '" + OperatorLV12 + "'"));
                sql += (OperatorLV21 == "" ? "" : (", '" + OperatorLV21 + "'"));
                sql += (OperatorLV22 == "" ? "" : (", '" + OperatorLV22 + "'"));
                sql += (ProjectInfo == "" ? "" : (", '" + ProjectInfo + "'"));
                sql += (OrderOperatorLV1 == "" ? "" : (", '" + OrderOperatorLV1 + "'"));
                sql += (OrderIDLV1 == "" ? "" : (", '" + OrderIDLV1 + "'"));
                sql += (OrderTimeLV1 == "" ? "" : (", '" + OrderTimeLV1 + "'"));
                sql += (OrderOperatorLV2 == "" ? "" : (", '" + OrderOperatorLV2 + "'"));
                sql += (OrderIDLV2 == "" ? "" : (", '" + OrderIDLV2 + "'"));
                sql += (OrderTimeLV2 == "" ? "" : (", '" + OrderTimeLV2 + "'"));
                sql += (OMCName == "" ? "" : (", '" + OMCName.Replace("'", "''") + "'"));
                sql += (Reserved2 == "" ? "" : (", '" + Reserved2.Replace("'", "''") + "'"));
                sql += (Reserved3 == "" ? "" : (", '" + Reserved3.Replace("'", "''") + "'"));
                sql += (ReceiveTime == "" ? "" : (", '" + ReceiveTime + "'"));

                sql += ")";

                return sql;
            }
            finally
            {
                SyncRoot.ReleaseReaderLock();
            }
		}

		private string GetResumeAlarmSql()
		{
            try
            {
                SyncRoot.AcquireReaderLock(-1);

                string sql = "";

                if (ClearTime == Constants.GARBAGE_CLEARTIME)
                {
#if MYSQL
                    sql = "start transaction";
                    sql += "; update ActiveAlarm set ClearTime = '" + ClearTime + "' where TKSn = " + TKSn.ToString();
                    sql += "; insert into GarbageAlarm SELECT * FROM  ActiveAlarm WHERE  ActiveAlarm.TKSn = " + TKSn.ToString();
                    sql += "; delete from ActiveAlarm WHERE  TKSn = " + TKSn.ToString();
                    sql += "; commit;";
#else
                    sql = "begin transaction";
                    sql += " update ActiveAlarm set ClearTime = '" + ClearTime + "' where TKSn = " + TKSn.ToString();
                    sql += " insert into GarbageAlarm SELECT * FROM  ActiveAlarm WHERE  ActiveAlarm.TKSn = " + TKSn.ToString();
                    sql += " delete from ActiveAlarm WHERE  TKSn = " + TKSn.ToString();
                    sql += " commit;";
#endif
                }
                else
                {
#if MYSQL
                    sql = "start transaction";
                    sql += "; update ActiveAlarm set ClearTime = '" + ClearTime + "' where TKSn = " + TKSn.ToString();
                    sql += "; insert into ResumeAlarm SELECT * FROM  ActiveAlarm WHERE  ActiveAlarm.TKSn = " + TKSn.ToString();
                    sql += "; delete from ActiveAlarm WHERE  TKSn = " + TKSn.ToString();
                    sql += "; commit;";
#else
                    sql = "begin transaction";
                    sql += " update ActiveAlarm set ClearTime = '" + ClearTime + "' where TKSn = " + TKSn.ToString();
                    sql += " insert into ResumeAlarm SELECT * FROM  ActiveAlarm WHERE  ActiveAlarm.TKSn = " + TKSn.ToString();
                    sql += " delete from ActiveAlarm WHERE  TKSn = " + TKSn.ToString();
                    sql += " commit";
#endif
                }

                return sql;
            }
            finally
            {
                SyncRoot.ReleaseReaderLock();
            }
		}
	}
}
