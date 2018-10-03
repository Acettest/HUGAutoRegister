using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TK_AlarmManagement;

namespace RemotableHGU_Fiber_Adapter
{
    class Program
    {
        public static AdapterController<HGU_Fiber_Adapter> s_AdapterController = null;

        static void Main(string[] args)
        {

            s_AdapterController = new AdapterController<HGU_Fiber_Adapter>();
            s_AdapterController.Prepare();
           // s_AdapterController.Start();
            Console.WriteLine(s_AdapterController.Adapter.Name + " 已经启动, 等待控制器命令...");

            while (true)
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
