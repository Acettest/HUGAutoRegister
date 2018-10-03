using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public interface ILoggerAgent
    {
        void SendLog(string log);
    }
}
