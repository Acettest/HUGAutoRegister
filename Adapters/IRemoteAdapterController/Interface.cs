using System;
using System.Collections.Generic;
using System.Text;

namespace AdapterRemoting
{
    public interface IRemoteController
    {
        void Prepare();
        bool Start();
        bool Stop();
        void Shutdown();
        List<string> GetOMCList();
        int GetStatus(); // 0 stopped, 1 running
        List<string> GetCurrentLog();
        List<string> GetLogFiles();
    }
}
