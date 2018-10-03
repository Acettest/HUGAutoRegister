using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    public delegate void LogHandler(string log);
    public interface ILogHandler
    {
        event LogHandler onLog;
    }
}
