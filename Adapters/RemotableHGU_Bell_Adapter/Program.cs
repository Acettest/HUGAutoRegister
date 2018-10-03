using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TK_AlarmManagement;

namespace RemotableHGU_Bell_Adapter
{
    class Program
    {
        public static AdapterController<HGU_Bell_Adapter> s_AdapterController = null;

        static void Main(string[] args)
        {
            
			s_AdapterController = new AdapterController<HGU_Bell_Adapter>();//为采集器赋值
            s_AdapterController.Prepare();
            //正式运行时，是通过其他进程来发送命令的方式来启动采集进程的，而调试时，必须主动启动采集进程才可能出现窗口
            //s_AdapterController.Start();//调试时取消注释，显示程序运行窗口，正式运行时应将其注释

            Console.WriteLine(s_AdapterController.Adapter.Name + " 已经启动, 等待控制器命令...");

            while (true)
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
