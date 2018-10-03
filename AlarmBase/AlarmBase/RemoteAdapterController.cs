using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public class RemoteAdapterController<T> : MarshalByRefObject, AdapterRemoting.IRemoteAdapteController
        where T : IAlarmAdapter
    {
        protected IAlarmAdapter m_Adapter = null;

        public RemoteAdapterController(T adapter)
        {
            m_Adapter = adapter;
        }

        #region IRemoteAdapteController 成员

        public bool Start()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Stop()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Shutdown()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetStatus()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetCurrentLog()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetLogFile()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
