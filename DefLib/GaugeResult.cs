using System;
using System.Collections.Generic;
using System.Text;

namespace TK_AlarmManagement
{
    [Serializable]
    public class GaugeResult
    {
        public int Total = 0;
        public double Max = 0;
        public double Min = 0;
        public double Avg = 0;
    }
}
