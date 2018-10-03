using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TK_AlarmManagement;

namespace RemotableYWT_HuaWei_Adapter
{
    class Program
    {
        public static AdapterController<HGU_HuaWei_Adapter> controller = null;

        static void Main(string[] args)
        {

            controller = new AdapterController<HGU_HuaWei_Adapter>();
            controller.Prepare();//核心入口
            controller.Start();
            //Console.WriteLine(controller.Adapter.Name + " 已经启动, 等待控制器命令...");

            while (true)
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
