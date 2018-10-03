using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Management;
using System.ServiceProcess;

namespace TKAutoLauncher
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.AfterInstall += new InstallEventHandler(ProjectInstaller_AfterInstall);
        }

        void ProjectInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            ConnectionOptions coOptions = new ConnectionOptions();

            coOptions.Impersonation = ImpersonationLevel.Impersonate;

            ManagementScope mgmtScope = new System.Management.ManagementScope(@"root\CIMV2", coOptions);

            mgmtScope.Connect();

            ManagementObject wmiService;

            wmiService = new ManagementObject("Win32_Service.Name='" + serviceInstaller1.ServiceName + "'");

            ManagementBaseObject InParam = wmiService.GetMethodParameters("Change");

            InParam["DesktopInteract"] = true;

            ManagementBaseObject OutParam = wmiService.InvokeMethod("Change", InParam, null);

            //ServiceController.Start();
        }
    }
}