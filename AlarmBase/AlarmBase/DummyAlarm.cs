using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public class DummyAlarm : ISourceAlarm
    {
        #region 构造器
        public DummyAlarm()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public DummyAlarm(string activetable, string resumetable)
        {
        }

        public DummyAlarm(DummyAlarm alarm)
        {
        }
        #endregion

        #region ISourceAlarm 成员

        public string TKSn
        {
            get { return ""; }
            set { }
        }

        public string ActiveTable
        {
            get { return ""; }
            set { }
        }

        public string ResumeTable
        {
            get { return ""; }
            set { }
        }

        public void BuildFromSource(System.Data.DataRow r, string omcname)
        {
        }

        public void BuildFromDB(System.Data.DataRow r)
        {
        }

        public void Garbage(System.Data.DataRow dr, string omcname)
        {
        }

        public string GetSql()
        {
            return "";
        }

        public string GetInsertSql()
        {
            return "";
        }

        public string GetUpdateSql()
        {
            return "";
        }
        #endregion
    }
}
