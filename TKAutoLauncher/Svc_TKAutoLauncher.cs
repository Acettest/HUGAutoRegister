using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace TKAutoLauncher
{
    public partial class TKAutoLauncher : ServiceBase
    {
        EventLog m_EventLog = null;

        public TKAutoLauncher()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            try
            {
                m_EventLog = new EventLog();
                m_EventLog.Source = "TKAutoLauncher";

                string domainpath = AppDomain.CurrentDomain.BaseDirectory;

                DataSet conf = new DataSet();
                conf.ReadXml(domainpath + AppDomain.CurrentDomain.FriendlyName + ".conf.xml");

                if (conf.Tables.Contains("Paras"))
                {
                    foreach (DataRow r in conf.Tables["Paras"].Rows)
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(r["startprog"].ToString());
                        ProcessStartInfo info = new ProcessStartInfo(fi.FullName);
                       // info.Arguments = "-start";
                        info.ErrorDialog = true;
                        info.CreateNoWindow = false;
                        info.WindowStyle = ProcessWindowStyle.Normal;
                        info.WorkingDirectory = fi.DirectoryName;

                        Process p = Process.Start(info);
                        m_EventLog.WriteEntry(fi.Name + "已经启动.", EventLogEntryType.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                m_EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
        }
    }
}
