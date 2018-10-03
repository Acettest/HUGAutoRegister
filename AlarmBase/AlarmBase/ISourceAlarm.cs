using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace TK_AlarmManagement
{
    public interface ISourceAlarm
    {
        string TKSn
        {
            get;
            set;
        }

        string ActiveTable
        {
            get;
            set;
        }

        string ResumeTable
        {
            get;
            set;
        }

        void BuildFromSource(DataRow r, string omcname);
        void BuildFromDB(DataRow r);
        void Garbage(DataRow dr, string omcname);
        string GetSql();
        string GetInsertSql();
        string GetUpdateSql();
   }
}
