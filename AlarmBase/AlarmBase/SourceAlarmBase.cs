using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public abstract class SourceAlarmBase : ISourceAlarm
    {
        protected string m_TKSn = "";
        protected string m_ActiveTable = "";
        protected string m_ResumeTable = "";
        protected string m_OMCName = "";

        protected string m_FN_ClearTime = "";

        virtual public string OMCName
        {
            get { return m_OMCName;}
            set { m_OMCName = value;}
        }

        virtual public string FN_ClearTime
        {
            get { return m_FN_ClearTime; }
            set { m_FN_ClearTime = value; }
        }

        #region 需要派生类实现的公共属性
        abstract public string OccurTime
        {
            get;
            set;
        }

        abstract public string ClearTime
        {
            get;
            set;
        }
        #endregion

        #region 本类中实现的ISourceAlarm 成员

        public string TKSn
        {
            get { return m_TKSn; }
            set { m_TKSn = value; }
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

        virtual public void Garbage(System.Data.DataRow dr, string omcname)
        {
            BuildFromSource(dr, omcname);
            ClearTime = TK_AlarmManagement.Constants.GARBAGE_CLEARTIME;
        }

        virtual public string GetSql()
        {
            string sql = "";

            if (ActiveTable == "")
                throw new Exception("未设置活动告警.");

            if (OccurTime != "" && ClearTime == "") // 活动告警
            {
                sql = GetInsertSql();
            }
            else if (OccurTime != "" && ClearTime != "") //已恢复告警
            {
                sql = GetUpdateSql();
            }

            return sql;
        }

        virtual public string GetUpdateSql()
        {
            string sql = "";

#if MYSQL
            sql = "start transaction";
            sql += "; Update " + ActiveTable + " Set " + FN_ClearTime + " = '" + ClearTime + "' where TKSn=" + TKSn;
            sql += "; Insert into " + ResumeTable + " Select * from " + ActiveTable + " where TKSn=" + TKSn;
            sql += "; Delete from " + ActiveTable + " where " + "TKSn = " + TKSn;
            sql += "; Commit;";
#else
            sql = "Begin transaction";
            sql += " Update " + ActiveTable + " Set " + FN_ClearTime + " = '" + ClearTime + "' where TKSn=" + TKSn;
            sql += " Insert into " + ResumeTable + " Select * from " + ActiveTable + " where TKSn=" + TKSn;
            sql += " Delete from " + ActiveTable + " where " + "TKSn = " + TKSn;
            sql += " Commit";
#endif

            return sql;
        }

        #endregion

        #region 需要派生类实现的ISourceAlarm 成员
        abstract public void BuildFromSource(System.Data.DataRow r, string omcname);

        abstract public void BuildFromDB(System.Data.DataRow r);

        abstract public string GetInsertSql();
        #endregion

        protected string ConvertDBStr(string src)
        {
            return src.Replace("'", "''");
        }
    }
}
