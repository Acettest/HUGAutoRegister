using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public enum AdapterStatus
    {
        Unknown = -1,
        Stop = 0,
        Running = 1,
        Working = 2,
    }

    [Serializable]
    public class AdapterInfo
    {
        public long ID = 0;
        public string Name = "";
        public string Address = "";
        public int ControllerPort = 0;
        public AdapterStatus State = AdapterStatus.Stop;
        public string OMCName = "";
        public object ExtraInfo = null;
    }

    [Serializable]
    public class WLANTestLineInfo
    {
        public string NICName;
        public string NICAddr;
        public string RepeaterAddr;
        public string APName;
        public string APMac;
        public string APSSID;
        public string APBSSID;
        public string TestMode;
        public bool IsActive = false;
    }
}
