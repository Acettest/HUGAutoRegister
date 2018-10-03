using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TK_AlarmManagement;

namespace RemotableHGU_PBoss_Adapter
{
    class Program
    {
        public static AdapterController<HGUPBossAsynAdapter> s_AdapterController = null;

        static void Main(string[] args)
        {

            s_AdapterController = new AdapterController<HGUPBossAsynAdapter>();
            s_AdapterController.Prepare();

            Console.WriteLine(s_AdapterController.Adapter.Name + " 已经启动, 等待控制器命令...");

            while (true)
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
