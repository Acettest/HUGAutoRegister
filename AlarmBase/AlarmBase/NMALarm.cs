using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    [Serializable]
    public class NMAlarm : ISourceAlarm
    {
        string m_TKSn = "";
        public string SourceOMC = ""; //监控源，一般以OMC名称来设值
        public string Object = "";
        public string AlarmName = "";
        public string OccurTime = "";
        public string ClearTime = "";
        public string LastOccurTime = "";
        public string Severity = "";
        public string Detail = "";
        public string Location = "";
        public const string OMCName = "TK监控器";
        public const string BusinessType = "TK告警";
        public const string Manufacturer = "TK";

        private string m_ActiveTable = "";
        private string m_ResumeTable = "";

        #region ISourceAlarm 成员
        public NMAlarm()
        {
        }

        public NMAlarm(string active, string resume)
        {
            m_ActiveTable = active;
            m_ResumeTable = resume;
        }

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
            get
            {
                return m_ActiveTable;
            }
            set
            {
                m_ActiveTable = value;
            }
        }

        public string ResumeTable
        {
            get
            {
                return m_ResumeTable;
            }
            set
            {
                m_ResumeTable = value;
            }
        }

        public void BuildFromSource(System.Data.DataRow r, string omcname)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void BuildFromDB(System.Data.DataRow r)
        {
            TKSn = r["TKSn"].ToString();
            SourceOMC = r["SourceOMC"].ToString();
            Object = r["Object"].ToString();
            AlarmName = r["AlarmName"].ToString();
            OccurTime = r["OccurTime"].ToString();
            LastOccurTime = r["LastOccurTime"].ToString();
            ClearTime = r["ClearTime"].ToString();
            Severity = r["Severity"].ToString();
            Detail = r["Detail"].ToString();
            Location = r["Location"].ToString();
            //OMCName = r["OMCName"].ToString();
            //BusinessType = r["BusinessType"].ToString();
            //Manufacturer = r["Manufacturer"].ToString();
        }

        public void Garbage(System.Data.DataRow dr, string omcname)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetSql()
        {
            string sql;

            if (ClearTime == "")
                sql = GetInsertSql();
            else
                sql = GetUpdateSql();

            return sql;
        }

        public string GetInsertSql()
        {
            string detail = Detail.Replace("'", "''");
            if (detail.Length > 512)
                detail = detail.Substring(0, 512);

            string sql = "";

            sql = "insert into " + ActiveTable + " (TKSn";
            sql += (SourceOMC == "" ? "" : ",SourceOMC");
            sql += (Object == "" ? "" : ",Object");
            sql += (AlarmName == "" ? "" : ",AlarmName");
            sql += (Detail == "" ? "" : ",Detail");
            sql += (Location == "" ? "" : ",Location");
            sql += (Severity == "" ? "" : ",Severity");
            sql += (OccurTime == "" ? "" : ",OccurTime");
            sql += (LastOccurTime == "" ? "" : ",LastOccurTime");
            sql += (ClearTime == "" ? "" : ",ClearTime");
            sql += (Manufacturer == "" ? "" : ",Manufacturer");
            sql += (BusinessType == "" ? "" : ",BusinessType");
            sql += (OMCName == "" ? "" : ",OMCName");

            sql += ")";

            sql += " values(" + TKSn;
            sql += (SourceOMC == "" ? "" : (", '" + SourceOMC + "'"));
            sql += (Object == "" ? "" : (", '" + Object + "'"));
            sql += (AlarmName == "" ? "" : (", '" + AlarmName + "'"));
            sql += (Detail == "" ? "" : (", '" + detail + "'"));
            sql += (Location == "" ? "" : (", '" + Location + "'"));
            sql += (Severity == "" ? "" : (", '" + Severity + "'"));
            sql += (OccurTime == "" ? "" : (", '" + OccurTime + "'"));
            sql += (LastOccurTime == "" ? "" : (", '" + LastOccurTime + "'"));
            sql += (ClearTime == "" ? "" : (", '" + ClearTime + "'"));
            sql += (Manufacturer == "" ? "" : (", '" + Manufacturer + "'"));
            sql += (BusinessType == "" ? "" : (", '" + BusinessType + "'"));
            sql += (OMCName == "" ? "" : (", '" + OMCName + "'"));

            sql += ")";

            return sql;
        }

        public string GetUpdateSql()
        {
            string sql = "";

            //sql = "Begin transaction";
            sql += " Update " + ActiveTable + " Set ClearTime = '" + ClearTime + "' where TKSn=" + TKSn;
            sql += " Insert into " + ResumeTable + " Select * from " + ActiveTable + " where TKSn=" + TKSn;
            sql += " Delete from " + ActiveTable + " where " + "TKSn = " + TKSn;
            //sql += " Commit";

            return sql;
        }

        #endregion
    }
    
}
