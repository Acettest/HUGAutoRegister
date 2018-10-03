using System;
using System.Collections.Generic;
using System.Text;
using TK_AlarmManagement;

namespace TK_AlarmManagement
{
    public interface INMAlarmGenerator
    {
        ulong AllocateTKSN();
        void ClearAllNMAlarms();
        bool ClearNMAlarm(string omcname, string obj, string name, DateTime cleartime);
        void DeclareNMMonitorObjects(string omcname, string obj, string name, string sev);
        List<NMAlarm> GetNMAlarms();
        void LoadNMAlarms();
        bool RaiseNMAlarm(string sourceomc, string obj, string name, string detail, DateTime occurtime);
        TKAlarm ConvertNMToTKAlarm(NMAlarm a);

        string UniteTKActiveTable { get; }
        string UniteTKResumeTable { get; }
    }
}
