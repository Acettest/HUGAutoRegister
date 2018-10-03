#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: YWTAlarm
// 作    者：d.w
// 创建时间：2014/6/19 14:19:28
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
using TK_AlarmManagement;

namespace RemotableHGU_PBoss_Adapter
{
   public class HUGAlarm: ISourceAlarm
    {
        public string m_TKSn = "";//集中告警流水号
        public string ManuSn = "";
        public string city = "";
        public string fetchTime = "";
        public string clearTime = "";
        public string nename = "";
        public string terminal_name = "";
        public string server_ip = "";
        public string alarmName = "";
        public string Severity = "";
        public string location = "";
        public string description = "";
        public string TaskId = "";
        public string OMCName = "";

        public string m_ActiveTable = "";
        public string m_ResumeTable = "";

        public HUGAlarm()
        {
        }

        public HUGAlarm(string activetable, string resumetable)
        {
            ActiveTable = activetable;
            ResumeTable = resumetable;
        }

        public HUGAlarm(HUGAlarm alarm)
        {
            m_TKSn = alarm.TKSn;
            ManuSn = alarm.ManuSn;
            city = alarm.city;
            fetchTime = alarm.fetchTime;
            clearTime = alarm.clearTime;
            nename = alarm.nename;
            terminal_name = alarm.terminal_name;
            server_ip = alarm.server_ip;
            alarmName = alarm.alarmName;
            Severity = alarm.Severity;
            location = alarm.location;
            description = alarm.description;
            TaskId = alarm.TaskId;
            OMCName = alarm.OMCName;

            ActiveTable = alarm.ActiveTable;
            ResumeTable = alarm.ResumeTable;
        }

        #region ISourceAlarm 成员

        public string TKSn
        {
            get
            {
                return m_TKSn;
            }
            set
            {
                m_TKSn = value;
            }
        }

        public string ActiveTable
        {
            get { return m_ActiveTable; }
            set { m_ActiveTable = value; }
        }

        public string ResumeTable
        {
            get { return m_ResumeTable; }
            set { m_ResumeTable = value; }
        }

        public void BuildFromSource(System.Data.DataRow r, string omcname)
        {
            this.ManuSn = r["ManuSn"].ToString();
            this.city = r["city"].ToString();
            this.fetchTime = r["fetchTime"].ToString();
            this.clearTime = r["clearTime"].ToString();								//对象类型
            this.nename = r["nename"].ToString();								        //网元名称
            this.terminal_name = r["terminal_name"].ToString();                                  //测试机IP
            this.server_ip = r["server_ip"].ToString();								//网元类型
            this.alarmName = r["alarmName"].ToString();
            this.Severity = r["Severity"].ToString();
            this.location = r["location"].ToString();
            this.description = r["description"].ToString();
            this.TaskId = r["TaskId"].ToString();
            this.OMCName = omcname;
        }

        public void BuildFromDB(System.Data.DataRow r)
        {
            m_TKSn = r["TKSn"].ToString();
            BuildFromSource(r, r["OMCName"].ToString());
        }

        public void Garbage(System.Data.DataRow dr, string omcname)
        {
            this.BuildFromSource(dr, omcname);
            this.clearTime = Constants.GARBAGE_CLEARTIME;
        }

        public string GetSql()
        {
            string sql;

            if (ActiveTable == "")
                throw new Exception("未指定活动告警表");

            if (clearTime == "")
                sql = GetInsertSql();
            else
                sql = GetUpdateSql();

            return sql;
        }

        public string GetInsertSql()
        {
            string sql = "";

            sql = "insert into " + ActiveTable + " (TKSn";
            sql += (city == "" ? "" : ",city");
            sql += (ManuSn == "" ? "" : ",ManuSn");
            sql += (fetchTime == "" ? "" : ",fetchTime");
            sql += (clearTime == "" ? "" : ",clearTime");
            sql += (nename == "" ? "" : ",nename");
            sql += (terminal_name == "" ? "" : ",terminal_name");
            sql += (server_ip == "" ? "" : ",server_ip");
            sql += (alarmName == "" ? "" : ",alarmName");
            sql += (Severity == "" ? "" : ",Severity");
            sql += (location == "" ? "" : ",location");
            sql += (description == "" ? "" : ",description");
            sql += (OMCName == "" ? "" : ",OMCName");
            sql += (TaskId == "" ? "" : ",TaskId");

            sql += ")";

            sql += " values(" + m_TKSn;
            sql += (city == "" ? "" : (", '" + city + "'"));
            sql += (ManuSn == "" ? "" : (", '" + ManuSn + "'"));
            sql += (fetchTime == "" ? "" : (", '" + fetchTime + "'"));
            sql += (clearTime == "" ? "" : (", '" + clearTime + "'"));
            sql += (nename == "" ? "" : (", '" + nename + "'"));
            sql += (terminal_name == "" ? "" : (", '" + terminal_name + "'"));
            sql += (server_ip == "" ? "" : (", '" + server_ip + "'"));
            sql += (alarmName == "" ? "" : (", '" + alarmName.Replace("'", "''") + "'"));
            sql += (Severity == "" ? "" : (", '" + Severity + "'"));
            sql += (location == "" ? "" : (", '" + location.Replace("'", "''") + "'"));
            sql += (description == "" ? "" : (", '" + description.Replace("'", "''") + "'"));
            sql += (OMCName == "" ? "" : (", '" + OMCName + "'"));
            sql += (TaskId == "" ? "" : (", '" + TaskId + "'"));

            sql += ")";

            return sql;
        }

        public string GetUpdateSql()
        {
            string sql = "";

            sql = "Update " + ActiveTable + " Set  clearTime = '" + clearTime;
            sql += "' where TKSn = " + m_TKSn;
            sql += ";Insert into " + ResumeTable + " Select * from " + ActiveTable + " where " + "TKSn = " + m_TKSn;
            sql += ";Delete from " + ActiveTable + " where " + "TKSn = " + m_TKSn + ";";

            return sql;
        }

        #endregion
    }
}
