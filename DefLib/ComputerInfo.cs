using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;

namespace DefLib
{
    ///  
    /// 系统信息类 - 获取CPU、内存、磁盘、进程信息 
    ///  
    public class SystemInfo
    {
        private int m_ProcessorCount = 0;   //CPU个数 
        private PerformanceCounter m_CpuLoad;   //CPU计数器 
        private PerformanceCounter m_MemAvail; // 可用内存计数器
        private long m_PhysicalMemory = 0;   //物理内存 

        private static SystemInfo m_Instance = new SystemInfo();
        private static object m_LockSingleton = new object();
        public static SystemInfo Instance
        {
            get
            {
                lock (m_LockSingleton)
                {
                    if (m_Instance == null)
                        m_Instance = new SystemInfo();
                    return m_Instance;
                }
            }
        }

        #region 构造函数
        ///  
        /// 构造函数，初始化计数器等 
        ///  
        protected SystemInfo()
        {
            //初始化CPU计数器 
            m_CpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            m_CpuLoad.MachineName = ".";
            m_CpuLoad.NextValue();

            //CPU个数 
            m_ProcessorCount = Environment.ProcessorCount;

            //获得物理内存 
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    m_PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
        }
        #endregion

        #region CPU个数
        ///  
        /// 获取CPU个数 
        ///  
        public int ProcessorCount
        {
            get
            {
                return m_ProcessorCount;
            }
        }
        #endregion

        #region CPU占用率
        ///  
        /// 获取CPU占用率 
        ///  
        public float CpuLoad
        {
            get
            {
                return m_CpuLoad.NextValue();
            }
        }
        #endregion

        #region 可用内存
        ///  
        /// 获取可用内存 
        ///  
        public long MemoryAvailable
        {
            get
            {
                long availablebytes = 0;
                //ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_PerfOS_Memory"); 
                //foreach (ManagementObject mo in mos.Get()) 
                //{ 
                //    availablebytes = long.Parse(mo["Availablebytes"].ToString()); 
                //} 
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return availablebytes;
            }
        }
        #endregion

        #region 物理内存
        ///  
        /// 获取物理内存 
        ///  
        public long PhysicalMemory
        {
            get
            {
                return m_PhysicalMemory;
            }
        }
        #endregion
    }
}
