using System;
using System.Reflection;
using System.Windows.Forms;
using TK_AlarmManagement;

namespace SystemMon
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //捕获线程未处理异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            CommandProcessor.instance().Start(false);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string[] f = Assembly.GetEntryAssembly().GetManifestResourceNames();
            Infragistics.Win.AppStyling.StyleManager.Load(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SystemMon.Skin.MyElectricBlue.isl"));

            Application.Run(new Form_Main());

            CommManager.instance().Stop();
            CommandProcessor.instance().Stop();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("发生不可恢复线程的异常，请重启监控终端.\n" + e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show("发生不可恢复UI的异常，请重启监控终端.\n" + e.Exception.ToString());
            Application.Exit();
        }
    }
}