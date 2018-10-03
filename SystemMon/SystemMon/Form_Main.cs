using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using TK_AlarmManagement;

namespace SystemMon
{
    public partial class Form_Main : Form
    {
        #region 私有成员
        DataTable m_RunTimeInfo = new DataTable();
        DataTable m_InfoMirror = null;
        System.Timers.Timer m_RefreshTimer = new System.Timers.Timer();
        int m_CommTimeout = 300;

        private ManualResetEvent m_TimerFinEvent = new ManualResetEvent(true);
        Dictionary<string, IPEndPoint> m_URIs = new Dictionary<string, IPEndPoint>();
        Dictionary<string, string> m_ExecutePaths = new Dictionary<string, string>();
        Dictionary<string, string> m_SysPara = new Dictionary<string, string>();
        Dictionary<string, ICommClient> m_Comms = new Dictionary<string, ICommClient>();
        ArrayList m_AdapterProcess = new ArrayList();

        Form_ServerStatus m_FormServer = null;
        Form_CurLog m_FormCurLog = null;
        #endregion

        #region 构造函数
        public Form_Main()
        {
            InitializeComponent();

            m_RunTimeInfo.Columns.AddRange(new DataColumn[]{
                new DataColumn("进程号", typeof(string)),
                new DataColumn("线程数", typeof(string)),
                new DataColumn("物理内存", typeof(string)),
                new DataColumn("启动时间", typeof(string)),
                new DataColumn("CPU时间(分钟)", typeof(string)),
                new DataColumn("状态", typeof(string)),
                new DataColumn("类型", typeof(string)),
                new DataColumn("名称", typeof(string)),
                new DataColumn("时间戳", typeof(DateTime)),
                });

            Grid_RunTimeInfo.SetDataBinding(m_RunTimeInfo, null);

            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["进程号"].Width = 50;
            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["线程数"].Width = 50;
            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["物理内存"].Width = 90;
            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["状态"].Width = 50;
            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["类型"].Width = 50;
            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["启动时间"].Width = 130;
            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["CPU时间(分钟)"].Hidden = true;
            Grid_RunTimeInfo.DisplayLayout.Bands[0].Columns["时间戳"].Hidden = true;

            Load += new EventHandler(Form_Main_Load);
            FormClosing += new FormClosingEventHandler(Form_Main_FormClosing);

            m_RefreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_RefreshTimer_Elapsed);

            ToolbarsManager.BeforeToolDropdown += new Infragistics.Win.UltraWinToolbars.BeforeToolDropdownEventHandler(ToolbarsManager_BeforeToolDropdown);

            // UltraGrid会默认激活一行，但是并未选中，造成视觉上的误解，这里通过afteractive事件去掉第一次的激活
            Grid_RunTimeInfo.AfterRowActivate += new EventHandler(Grid_RunTimeInfo_AfterRowActivate);

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Log"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Log");
        }

        private bool m_bLoaded = false;
        void Grid_RunTimeInfo_AfterRowActivate(object sender, EventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            if (!m_bLoaded)
            {
                grid.ActiveRow = null;
                m_bLoaded = true;
            }
        }
        #endregion

        #region 右键菜单状态设置
        void ToolbarsManager_BeforeToolDropdown(object sender, Infragistics.Win.UltraWinToolbars.BeforeToolDropdownEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "Context_Grid":
                    if (Grid_RunTimeInfo.Selected.Rows.Count > 1)
                    {
                        ToolbarsManager.Tools["Menu_ServerInfo"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_CurrentLog"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_AllLog"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_OMCList"].SharedProps.Enabled = false;
                    }
                    else if (Grid_RunTimeInfo.Selected.Rows.Count == 0)
                    {
                        ToolbarsManager.Tools["Menu_ServerInfo"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_OMCList"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_CurrentLog"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_AllLog"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_RunAdapter"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_StopAdapter"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_ShutdownAdapter"].SharedProps.Enabled = false;
                        ToolbarsManager.Tools["Context_SelectFile"].SharedProps.Enabled = false;
                    }
                    else
                    {
                        if (Grid_RunTimeInfo.Selected.Rows[0].Cells["类型"].Value.ToString() == "服务器")
                        {
                            ToolbarsManager.Tools["Menu_ServerInfo"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_OMCList"].SharedProps.Enabled = false;
                            ToolbarsManager.Tools["Context_CurrentLog"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_AllLog"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_RunAdapter"].SharedProps.Enabled = false;
                            ToolbarsManager.Tools["Context_StopAdapter"].SharedProps.Enabled = false;
                            ToolbarsManager.Tools["Context_ShutdownAdapter"].SharedProps.Enabled = false;
                            ToolbarsManager.Tools["Context_SelectFile"].SharedProps.Enabled = false;
                        }
                        else
                        {
                            ToolbarsManager.Tools["Menu_ServerInfo"].SharedProps.Enabled = false;
                            ToolbarsManager.Tools["Context_OMCList"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_CurrentLog"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_AllLog"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_RunAdapter"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_StopAdapter"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_ShutdownAdapter"].SharedProps.Enabled = true;
                            ToolbarsManager.Tools["Context_SelectFile"].SharedProps.Enabled = true;
                        }
                    }

                    ToolbarsManager.Tools["Context_Export"].SharedProps.Enabled = true;
                    break;
                case "Menu_System":
                    if (Grid_RunTimeInfo.Selected.Rows.Count == 0 || Grid_RunTimeInfo.Selected.Rows.Count > 1)
                    {
                        ToolbarsManager.Tools["Menu_ServerInfo"].SharedProps.Enabled = false;
                    }
                    else if (Grid_RunTimeInfo.Selected.Rows.Count == 1)
                    {
                        if (Grid_RunTimeInfo.Selected.Rows[0].Cells["类型"].Value.ToString() == "服务器")
                        {
                            ToolbarsManager.Tools["Menu_ServerInfo"].SharedProps.Enabled = true;
                        }
                        else
                        {
                            ToolbarsManager.Tools["Menu_ServerInfo"].SharedProps.Enabled = false;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 定时刷新状态信息
        private long m_InTimer = 0;
        void m_RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref m_InTimer, 1) == 1)
                return;

            try
            {
                m_TimerFinEvent.Reset();

                refreshStatus();
            }
            catch
            { }
            finally
            {
                Interlocked.Exchange(ref m_InTimer, 0);
                m_TimerFinEvent.Set();
            }
        }

        void refreshStatus()
        {
            try
            {
                if (m_InfoMirror == null)
                    return;

                Dictionary<string, Dictionary<string, string>> statuses = new Dictionary<string, Dictionary<string, string>>(16);
                Dictionary<string, Dictionary<string, string>> serverstatuses = new Dictionary<string, Dictionary<string, string>>(16);

                lock (m_InfoMirror)
                    foreach (DataRow r in m_InfoMirror.Rows)
                    {
                        if (r["类型"].ToString() == "服务器")
                        {
                            serverstatuses.Add(r["名称"].ToString(), new Dictionary<string, string>());
                            continue;
                        }

                        statuses.Add(r["名称"].ToString(), new Dictionary<string, string>(10));
                    }

                foreach (KeyValuePair<string, Dictionary<string, string>> pair in serverstatuses)
                {
                    if (!m_RefreshTimer.Enabled)
                        return;

                    #region 刷新服务器状态
                    string name = pair.Key;

                    BeginInvoke(new StringParaFunc(_setStatusBar), new object[] { "检查:" + name + " 的状态...." });

                    int pid, threadcount, status, authorized_alarmclientnum, nonauthorized_alarmclientnum;
                    int adapterclientnum, activealarmnum;
                    long phymem, elaspedtime;
                    DateTime starttime;
                    Dictionary<string, string> s = pair.Value;

                    ICommClient comm = m_Comms[name];
                    if (comm.Start())
                    {
                        s["状态"] = "在线";
                        s["时间戳"] = DateTime.Now.ToString();

                        CommandMsgV2 cm = new CommandMsgV2();
                        cm.TK_CommandType = Constants.TK_CommandType.SERVER_GETRUNTIMEINFO;

                        CommandMsgV2 resp = (CommandMsgV2)comm.SendCommand(cm);
                        if (resp == null)
                        {
                            s["状态"] = "未知";
                            s["进程号"] = "未知";
                            s["线程数"] = "未知";
                            s["物理内存"] = "未知";
                            s["启动时间"] = "未知";
                            s["CPU时间(分钟)"] = "未知";

                            pid = -1;
                            threadcount = -1;
                            phymem = -1;
                            starttime = DateTime.MinValue;
                            elaspedtime = -1;
                            status = -1;
                            authorized_alarmclientnum = -1;
                            nonauthorized_alarmclientnum = -1;
                            adapterclientnum = -1;
                            activealarmnum = -1;
                        }
                        else
                        {
                            pid = Convert.ToInt32(resp.GetValue("PROCESSID"));
                            s["进程号"] = pid.ToString();

                            threadcount = Convert.ToInt32(resp.GetValue("THREADCOUNT"));
                            s["线程数"] = threadcount.ToString();

                            starttime = Convert.ToDateTime(resp.GetValue("STARTTIME"));
                            s["启动时间"] = resp.GetValue("STARTTIME").ToString();

                            elaspedtime = Convert.ToInt64(resp.GetValue("CPUTIME"));
                            s["CPU时间(分钟)"] = resp.GetValue("CPUTIME").ToString();

                            phymem = Convert.ToInt64(resp.GetValue("PHYMEMORY"));
                            if (phymem > 1024)
                            {
                                long kb = phymem / 1024;
                                s["物理内存"] = kb.ToString() + " KB";
                            }
                            else
                                s["物理内存"] = phymem.ToString() + " B";

                            status = Convert.ToInt32(resp.GetValue("STATUS"));
                            switch (status)
                            {
                                case 0:
                                    s["状态"] = "停止";
                                    break;
                                case 1:
                                    s["状态"] = "运行";
                                    break;
                                default:
                                    s["状态"] = "未知";
                                    break;
                            }

                            string[] subs = resp.GetValue("ALARMCLIENTS").ToString().Split(',');
                            authorized_alarmclientnum = Convert.ToInt32(subs[0]);
                            nonauthorized_alarmclientnum = Convert.ToInt32(subs[1]);
                            adapterclientnum = Convert.ToInt32(resp.GetValue("ADAPTERCLIENTS"));
                            activealarmnum = Convert.ToInt32(resp.GetValue("ACTIVEALARMNUM"));
                        }
                    }
                    else
                    {
                        s["状态"] = "不在线";
                        s["时间戳"] = DateTime.Now.ToString();
                        s["进程号"] = "未知";
                        s["线程数"] = "未知";
                        s["物理内存"] = "未知";
                        s["启动时间"] = "未知";
                        s["CPU时间(分钟)"] = "未知";

                        pid = -1;
                        threadcount = -1;
                        phymem = -1;
                        starttime = DateTime.MinValue;
                        elaspedtime = -1;
                        status = -1;
                        authorized_alarmclientnum = -1;
                        nonauthorized_alarmclientnum = -1;
                        adapterclientnum = -1;
                        activealarmnum = -1;
                    }

                    try
                    {
                        if (m_FormServer != null && m_FormServer.Visible)
                            m_FormServer.BeginInvoke(new ServerInfo(m_FormServer.RefreshStatus),
                                new object[]{name, pid, threadcount, phymem, starttime, elaspedtime,
                                    status, authorized_alarmclientnum, nonauthorized_alarmclientnum, adapterclientnum,
                                    activealarmnum});
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }

                    BeginInvoke(new StringParaFunc(_setStatusBar), new object[] { name + "状态检查完毕." });

                    #endregion
                }

                foreach (KeyValuePair<string, Dictionary<string, string>> pair in statuses)
                {
                    if (!m_RefreshTimer.Enabled)
                        return;

                    string name = pair.Key;

                    if (!m_Comms.ContainsKey(name))
                    {
                        continue;
                    }

                    BeginInvoke(new StringParaFunc(_setStatusBar), new object[] { "检查:" + name + " 的状态...." });

                    Dictionary<string, string> status = pair.Value;

                    ICommClient comm = m_Comms[name];

                    if (!comm.Start())
                    {
                        status["时间戳"] = DateTime.Now.ToString();
                        status["状态"] = "不在线";
                        status["进程号"] = "未知";
                        status["线程数"] = "未知";
                        status["物理内存"] = "未知";
                        status["启动时间"] = "未知";
                        status["CPU时间(分钟)"] = "未知";
                        continue;
                    }

                    status["时间戳"] = DateTime.Now.ToString();

                    try
                    {
                        //if (!m_Process.ContainsKey(m_ExecutePaths[name]))
                        //{
                        //    status["状态"] = "未知";
                        //    continue;
                        //}
                        //Process p = Process.GetProcessById(m_Process[m_ExecutePaths[name]]);

                        //status["进程号"] = p.Id.ToString();

                        //status["线程数"] = p.Threads.Count.ToString();

                        //status["启动时间"] = p.StartTime.ToString("yyyy-MM-dd HH:mm:ss");

                        //status["CPU时间(分钟)"] = ((long)p.TotalProcessorTime.TotalMinutes).ToString();

                        //long mem = Convert.ToInt64(p.WorkingSet64);
                        //if (mem > 1024)
                        //{
                        //    mem /= 1024;
                        //    status["物理内存"] = mem.ToString() + " KB";
                        //}
                        //else
                        //    status["物理内存"] = mem.ToString() + " B";

                        //int s = 1;

                        CommandMsgV2 cm = new CommandMsgV2();
                        cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_GETRUNTIMEINFO;
                        cm.SetValue("NAME", name);

                        CommandMsgV2 resp = (CommandMsgV2)comm.SendCommand(cm, 5);
                        if (resp == null)
                        {
                            status["状态"] = "未知";
                            continue;
                        }

                        status["进程号"] = resp.GetValue("PROCESSID").ToString();

                        status["线程数"] = resp.GetValue("THREADCOUNT").ToString();

                        status["启动时间"] = Convert.ToDateTime(resp.GetValue("STARTTIME")).ToString("yyyy-MM-dd HH:mm:ss");

                        status["CPU时间(分钟)"] = resp.GetValue("CPUTIME").ToString();

                        long mem = Convert.ToInt64(resp.GetValue("PHYMEMORY"));
                        if (mem > 1024)
                        {
                            mem /= 1024;
                            status["物理内存"] = mem.ToString() + " KB";
                        }
                        else
                            status["物理内存"] = mem.ToString() + " B";

                        int s = Convert.ToInt32(resp.GetValue("STATUS"));
                        switch (s)
                        {
                            case 0:
                                status["状态"] = "停止";
                                break;
                            case 1:
                                status["状态"] = "运行";
                                break;
                            case 2:
                                status["状态"] = "空闲";
                                break;
                            case 3:
                                status["状态"] = "异常";
                                break;
                            case 4:
                                status["状态"] = "错误";
                                break;
                            default:
                                status["状态"] = "未知";
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        status["状态"] = "未知";

                        status["进程号"] = "未知";
                        status["线程数"] = "未知";
                        status["物理内存"] = "未知";
                        status["启动时间"] = "未知";
                        status["CPU时间(分钟)"] = "未知";
                    }

                    BeginInvoke(new StringParaFunc(_setStatusBar), new object[] { name + "状态检查完毕." });
                } //end foreach

                if (serverstatuses.Count > 0)
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> p in serverstatuses)
                        statuses.Add(p.Key, p.Value);
                }

                lock (m_InfoMirror)
                {
                    foreach (DataRow r in m_InfoMirror.Rows)
                    {
                        if (!statuses.ContainsKey(r["名称"].ToString()))
                            continue;

                        Dictionary<string, string> status = statuses[r["名称"].ToString()];
                        foreach (KeyValuePair<string, string> pair in status)
                        {
                            r[pair.Key] = pair.Value;
                        }
                    }
                } // endlock
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            try
            {
                BeginInvoke(new NonParaFun(_updateGUI));
            }
            catch { }
        }

        delegate void ServerInfo(string name, int pid, int threadcount, long phymem,
            DateTime starttime, long elaspedtime,
            int status, int authorized_alarmclientnum, int nonauthorized_alarmclientnum, int adapterclientnum, int activealarmnum);
        delegate void NonParaFun();
        void _updateGUI()
        {
            lock (m_InfoMirror)
            {
                //bool b_err = false;//选中行是否包含错误行
                for (int i = 0; i < m_RunTimeInfo.Rows.Count; ++i)
                {
                    DataRow r = m_RunTimeInfo.Rows[i];
                    DateTime t1 = Convert.ToDateTime(r["时间戳"]);
                    DataRow m = m_InfoMirror.Rows.Find(r["名称"]);
                    if (m != null)
                    {
                        if (m["状态"].ToString() == "错误" || m["状态"].ToString() == "异常")
                        {
                            //if (Grid_RunTimeInfo.Rows[i].IsActiveRow)
                            //    b_err = true;
                            Grid_RunTimeInfo.Rows[i].Appearance.BackColor = Color.Red;
                        }
                        else
                        {
                            Grid_RunTimeInfo.Rows[i].Appearance.BackColor = Color.White;
                        }
                        DateTime t2 = Convert.ToDateTime(m["时间戳"]);
                        if (t2 < t1)
                            continue;

                        foreach (DataColumn col in m_InfoMirror.Columns)
                        {
                            r[col.ColumnName] = m[col];
                        }
                    }
                }
                //if (Grid_RunTimeInfo.ActiveRow != null)
                //{
                //    if (b_err)
                //        Grid_RunTimeInfo.DisplayLayout.ActiveRow.Appearance.BackColor = Color.Red;
                //    else
                //        Grid_RunTimeInfo.DisplayLayout.ActiveRow.Appearance.BackColor = Color.White;
                //}

                Grid_RunTimeInfo.Refresh();
            }
        }
        #endregion

        void Form_Main_Load(object sender, EventArgs e)
        {
            try
            {
                ReadConfig();
                this.Text = m_SysPara["Adapter Name"];
                //if (m_SysPara.ContainsKey("Process Monitor") && m_SysPara["Process Monitor"] == "1")
                //{
                //    m_Monitor = new Thread(Monitor);
                //    m_Monitor.Start();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_Stoped = true;
            m_RefreshTimer.Stop();
            m_TimerFinEvent.WaitOne();
            try { m_Monitor.Abort(); } catch { }
            CommManager.instance().Stop();
        }

        #region 监控采集进程
        private System.Threading.Thread m_Monitor;
        private int m_Interval = 300;
        private bool m_Stoped = false;
        private Dictionary<string, int> m_Process = new Dictionary<string, int>();
        private void Monitor()
        {
            while (true)
            {
                if (m_Stoped)
                    break;
                string fname = "";
                try
                {
                    Process[] ps = Process.GetProcesses();
                    foreach (string str in m_ExecutePaths.Keys)
                    {
                        fname = m_ExecutePaths[str];
                        bool b_Find = false;
                        foreach (Process p in ps)
                        {
                            if (!p.ProcessName.StartsWith("Tran"))
                                continue;
                            if (fname == p.MainModule.FileName)
                            {
                                b_Find = true;
                                if (m_Process.ContainsKey(fname))
                                    m_Process[fname] = p.Id;
                                else
                                    m_Process.Add(fname, p.Id);
                                break;
                            }
                        }

                        if (!b_Find)
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo(fname);
                            ProcessStartInfo info = new ProcessStartInfo(fi.FullName);
                            // info.Arguments = "-start";
                            info.ErrorDialog = true;
                            info.CreateNoWindow = false;
                            info.WindowStyle = ProcessWindowStyle.Normal;
                            info.WorkingDirectory = fi.DirectoryName;

                            Process p = Process.Start(info);
                            if (m_Process.ContainsKey(fname))
                                m_Process[fname] = p.Id;
                            else
                                m_Process.Add(fname, p.Id);
                            receiveLog(fi.Name + "已经启动.");
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    receiveLog("启动失败:" + fname + " " + ex.Message);
                }
                finally
                {
                    int count = m_Interval;
                    while (count-- > 0)
                    {
                        if (m_Stoped)
                            break;
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
        }
        #endregion

        #region 读取配置信息
        private bool ReadConfig()
        {
            //获取当前路径
            string sPath = System.AppDomain.CurrentDomain.BaseDirectory;
            sPath = sPath.Substring(0, sPath.LastIndexOf("\\") + 1);

            //读取保存在dbconnection.xml中的登陆连接信息
            DataSet xmlds = new DataSet();

            xmlds.ReadXml(sPath + "\\Conf.xml");

            if (!xmlds.Tables.Contains("Adapters"))
            {
                MessageBox.Show("未配置采集器信息, 请检查配置文件CONF.XML");
                return false;
            }

            if (!xmlds.Tables.Contains("Server"))
            {
                MessageBox.Show("未配置服务器信息, 请检查配置文件CONF.XML");
                return false;
            }

            if (!xmlds.Tables.Contains("Parameters"))
            {
                MessageBox.Show("参数表丢失, 请检查配置文件CONF.XML");
                return false;
            }

            for (int i = 0; i < xmlds.Tables["Parameters"].Rows.Count; ++i)
            {
                DataRow raw = xmlds.Tables["Parameters"].Rows[i];

                m_SysPara.Add(raw["paraname"].ToString(), raw["value"].ToString());
            }

            m_CommTimeout = Convert.ToInt32(m_SysPara["Comm Timeout"]);

            // 只取第一个服务器
            for (int i = 0; i < 1 && i < xmlds.Tables["Server"].Rows.Count; ++i)
            {
                DataRow raw = xmlds.Tables["Server"].Rows[i];
                m_URIs.Add(raw["servername"].ToString(), new IPEndPoint(IPAddress.Parse(raw["ip"].ToString()), Convert.ToInt32(raw["port"])));
                m_Comms.Add(raw["servername"].ToString(),
                    CommManager.instance().CreateCommClient<CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(raw["servername"].ToString() + "监控",
                    raw["ip"].ToString(), Convert.ToInt32(raw["port"]), m_CommTimeout, false, false));

                m_ExecutePaths.Add(raw["servername"].ToString(), raw["executepath"].ToString());


                DataRow r = m_RunTimeInfo.NewRow();
                r["名称"] = raw["servername"].ToString();
                r["类型"] = "服务器";
                r["状态"] = "未知";
                r["进程号"] = "未知";
                r["线程数"] = "未知";
                r["物理内存"] = "未知";
                r["启动时间"] = "未知";
                r["CPU时间(分钟)"] = "未知";
                r["时间戳"] = DateTime.Now;

                m_RunTimeInfo.Rows.Add(r);
            }
            string akey = "";
            for (int i = 0; i < xmlds.Tables["Adapters"].Rows.Count; ++i)
            {
                DataRow raw = xmlds.Tables["Adapters"].Rows[i];

                try
                {
                    m_URIs.Add(raw["adaptername"].ToString(), new IPEndPoint(IPAddress.Parse(raw["ip"].ToString()), Convert.ToInt32(raw["port"])));
                    m_Comms.Add(raw["adaptername"].ToString(),
                        CommManager.instance().CreateCommClient<CommandMsgV2, TKMessageV2Extractor, TKMessageV2Encoder>(raw["adaptername"].ToString() + "监控",
                        raw["ip"].ToString(), Convert.ToInt32(raw["port"]), m_CommTimeout, false, false));
                    m_ExecutePaths.Add(raw["adaptername"].ToString(), raw["executepath"].ToString());
                    akey = raw["executepath"].ToString();
                    akey = akey.Substring(akey.LastIndexOf('\\') + 1).Replace(".exe", "");
                    if (!m_AdapterProcess.Contains(akey))
                        m_AdapterProcess.Add(akey);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException)
                        MessageBox.Show("不允许重复的服务名称或采集名称");
                    return false;
                }

                DataRow r = m_RunTimeInfo.NewRow();
                r["名称"] = raw["adaptername"].ToString();
                r["类型"] = "采集";
                r["状态"] = "未知";
                r["进程号"] = "未知";
                r["线程数"] = "未知";
                r["物理内存"] = "未知";
                r["启动时间"] = "未知";
                r["CPU时间(分钟)"] = "未知";
                r["时间戳"] = DateTime.Now;

                m_RunTimeInfo.Rows.Add(r);
            }

            m_RunTimeInfo.AcceptChanges();

            m_InfoMirror = m_RunTimeInfo.Copy();
            m_InfoMirror.PrimaryKey = new DataColumn[] { m_InfoMirror.Columns["名称"] };

            return true;
        }
        #endregion

        private void ToolbarsManager_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "Menu_About":    // ButtonTool
                    // Place code here
                    //appStylistRuntime1.ShowRuntimeApplicationStylingEditor(this, "hello");
                    break;

                case "Menu_RunAdapters":    // ButtonTool
                    #region 启动采集
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        foreach (UltraGridRow row in Grid_RunTimeInfo.Rows)
                        {
                            if (row.Cells["类型"].Value.ToString() != "采集")
                                continue;

                            _runAdapter(row);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;

                case "Menu_StopAdapters":    // ButtonTool
                    #region 结束所有采集
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        _closeAdapter();
                        //foreach (UltraGridRow row in Grid_RunTimeInfo.Rows)
                        //{
                        //    if (row.Cells["类型"].Value.ToString() != "采集")
                        //        continue;

                        //    _stopAdapter(row);
                        //}

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion                    // Place code here
                    break;

                case "Menu_ServerInfo":    // ButtonTool
                    #region 获取服务器信息
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        UltraGridRow row = Grid_RunTimeInfo.Selected.Rows[0];
                        if (Grid_RunTimeInfo.Selected.Rows[0].Cells["类型"].Value.ToString() != "服务器")
                            return;

                        string name = row.Cells["名称"].Value.ToString();

                        if (m_FormServer == null)
                        {
                            m_FormServer = new Form_ServerStatus();
                            Point p = new Point();
                            p.X = Location.X + Width;
                            p.Y = Location.Y;
                            m_FormServer.Location = p;
                            m_FormServer.Owner = this;
                        }

                        m_FormServer.Show();
                        m_FormServer.Activate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;

                case "Menu_Exit":    // ButtonTool
                    // Place code here
                    {
                        if (DialogResult.Yes == MessageBox.Show("是否退出监控平台?", "请确认",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                            Application.ExitThread();
                        break;
                    }
                case "Menu_Refresh":
                    {
                        #region 启停状态定时刷新
                        try
                        {
                            Infragistics.Win.UltraWinToolbars.StateButtonTool state = (Infragistics.Win.UltraWinToolbars.StateButtonTool)e.Tool;
                            if (state.Checked)
                            {
                                Cursor = Cursors.AppStarting;

                                m_RefreshTimer.Interval = Convert.ToInt32(m_SysPara["Refresh Interval"]) * 1000;
                                m_RefreshTimer.Start();

                                _setStatusBar("启动状态刷新及监控告警采集");
                                m_RefreshTimer_Elapsed(null, null);
                            }
                            else
                            {
                                Cursor = Cursors.WaitCursor;

                                _setStatusBar("停止状态刷新及监控告警采集");
                                m_RefreshTimer.Stop();


                                m_TimerFinEvent.WaitOne();
                            }
                        }
                        finally
                        {
                            Cursor = Cursors.Default;
                        }
                        #endregion
                        break;
                    }
                case "Context_RunAdapter":
                    #region 启动采集
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        Infragistics.Win.UltraWinGrid.UltraGridRow[] rows = new Infragistics.Win.UltraWinGrid.UltraGridRow[Grid_RunTimeInfo.Selected.Rows.Count];
                        Grid_RunTimeInfo.Selected.Rows.CopyTo(rows, 0);

                        foreach (UltraGridRow row in rows)
                        {
                            if (row.Cells["类型"].Value.ToString() != "采集")
                                continue;

                            _runAdapter(row);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;
                case "Context_StopAdapter":
                    #region 停止采集
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        Infragistics.Win.UltraWinGrid.UltraGridRow[] rows = new Infragistics.Win.UltraWinGrid.UltraGridRow[Grid_RunTimeInfo.Selected.Rows.Count];
                        Grid_RunTimeInfo.Selected.Rows.CopyTo(rows, 0);

                        foreach (UltraGridRow row in rows)
                        {
                            if (row.Cells["类型"].Value.ToString() != "采集")
                                continue;

                            _stopAdapter(row);
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;
                case "Context_ShutdownAdapter":
                    #region 结束采集进程
                    try
                    {
                        if (DialogResult.No == MessageBox.Show("结束所选采集进程的执行, 如果不在本地运行, 将无法通过监控平台启动. 是否继续?",
                            "请确认", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2))
                            return;

                        Cursor = Cursors.WaitCursor;

                        Infragistics.Win.UltraWinGrid.UltraGridRow[] rows = new Infragistics.Win.UltraWinGrid.UltraGridRow[Grid_RunTimeInfo.Selected.Rows.Count];
                        Grid_RunTimeInfo.Selected.Rows.CopyTo(rows, 0);

                        foreach (UltraGridRow row in rows)
                        {
                            if (row.Cells["类型"].Value.ToString() != "采集")
                                continue;

                            _shutdownAdapter(row);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;
                case "Context_CurrentLog":
                    #region 获取当前日志
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        if (m_FormCurLog == null)
                        {
                            m_FormCurLog = new Form_CurLog();
                            m_FormCurLog.Owner = this;
                            Point p = new Point();
                            p.X = Location.X + Width;
                            p.Y = Location.Y;
                            m_FormCurLog.Location = p;
                        }

                        UltraGridRow row = Grid_RunTimeInfo.Selected.Rows[0];
                        CommandMsgV2 cm = new CommandMsgV2();

                        if (row.Cells["类型"].Value.ToString() == "服务器")
                            cm.TK_CommandType = Constants.TK_CommandType.SERVER_GETCURLOG;
                        else
                            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_GETCURLOG;
                        cm.SetValue("NAME", row.Cells["名称"].Value.ToString());

                        ICommClient comm = m_Comms[cm.GetValue("NAME").ToString()];
                        if (!comm.Start())
                        {
                            _setStatus(row, "不在线");
                            MessageBox.Show("无法连接监控对象: " + cm.GetValue("NAME").ToString());
                            return;
                        }

                        if (row.Cells["状态"].Value.ToString() == "未知"
                            || row.Cells["状态"].Value.ToString() == "不在线")
                            _setStatus(row, "在线");
                        Grid_RunTimeInfo.Refresh();

                        CommandMsg resp = (CommandMsg)comm.SendCommand(cm);
                        if (resp == null)
                        {
                            MessageBox.Show("获取当前日志失败.");
                            return;
                        }

                        m_FormCurLog.TextLog.Text = "";
                        m_FormCurLog.TextLog.AppendText(resp.GetValue("CURLOG").ToString());
                        m_FormCurLog.Show();
                        m_FormCurLog.Activate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;
                case "Context_AllLog":
                    #region 获取全部日志文件
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        UltraGridRow row = Grid_RunTimeInfo.Selected.Rows[0];
                        string name = row.Cells["名称"].Value.ToString();
                        ICommClient comm = m_Comms[name];
                        if (!comm.Start())
                        {
                            _setStatus(row, "不在线");
                            MessageBox.Show("无法连接监控对象: " + name);
                            return;
                        }

                        if (row.Cells["状态"].Value.ToString() == "未知"
                            || row.Cells["状态"].Value.ToString() == "不在线")
                            _setStatus(row, "在线");
                        Grid_RunTimeInfo.Refresh();

                        if (comm.LocalIP != comm.RemoteIP)
                        {
                            MessageBox.Show("监控对象:" + name + "未在本地运行, 无法获取日志文件.");
                            return;
                        }

                        Form_AllLog form = new Form_AllLog();
                        Point p = new Point();
                        p.X = Location.X + Width;
                        p.Y = Location.Y;
                        form.Location = p;
                        form.Owner = this;

                        CommandMsgV2 cm = new CommandMsgV2();

                        if (row.Cells["类型"].Value.ToString() == "服务器")
                            cm.TK_CommandType = Constants.TK_CommandType.SERVER_GETLOGFILES;
                        else
                            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_GETLOGFILES;
                        cm.SetValue("NAME", name);

                        CommandMsgV2 resp = (CommandMsgV2)comm.SendCommand(cm);
                        if (resp == null)
                        {
                            MessageBox.Show("获取日志文件清单失败.");
                            return;
                        }

                        string[] subs = resp.GetValue("LOGFILES").ToString().Split(',');
                        foreach (string s in subs)
                        {
                            form.Files.Add(s.Substring(s.LastIndexOf("\\") + 1));

                            if (form.WorkingDir == "")
                                form.WorkingDir = s.Substring(0, s.LastIndexOf("\\") + 1);
                        }

                        form.Show();
                        form.Activate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;
                case "Context_OMCList":
                    #region 获取OMC清单
                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        UltraGridRow row = Grid_RunTimeInfo.Selected.Rows[0];
                        string name = row.Cells["名称"].Value.ToString();
                        ICommClient comm = m_Comms[name];
                        if (!comm.Start())
                        {
                            _setStatus(row, "不在线");
                            MessageBox.Show("无法连接监控对象: " + name);
                            return;
                        }

                        if (row.Cells["状态"].Value.ToString() == "未知"
                            || row.Cells["状态"].Value.ToString() == "不在线")
                            _setStatus(row, "在线");
                        Grid_RunTimeInfo.Refresh();

                        Form_OMCList form = new Form_OMCList();
                        Point p = new Point();
                        p.X = Location.X + Width;
                        p.Y = Location.Y;
                        form.Location = p;
                        form.Owner = this;

                        CommandMsgV2 cm = new CommandMsgV2();
                        cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_GETOMCLIST;
                        cm.SetValue("NAME", name);

                        CommandMsgV2 resp = (CommandMsgV2)comm.SendCommand(cm);
                        if (resp == null)
                        {
                            MessageBox.Show("获取采集源OMC清单失败.");
                            return;
                        }

                        string[] subs = resp.GetValue("OMCLIST").ToString().Split(',');
                        foreach (string s in subs)
                        {
                            form.OMCList.Add(s);
                        }

                        form.Show();
                        form.Activate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;
                case "Context_Export":
                    #region 输出Excel文件
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.DefaultExt = "xls";
                    saveFileDialog.Filter = "Excel文档(*.xls)|*.xls";
                    if (DialogResult.OK == saveFileDialog.ShowDialog())
                    {
                        try
                        {
                            GridExcelExporter.Export(Grid_RunTimeInfo, saveFileDialog.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    #endregion
                    break;
                case "Context_SelectFile":
                    #region 定位文件
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        UltraGridRow row = Grid_RunTimeInfo.Selected.Rows[0];
                        selectFile(m_ExecutePaths[row.Cells["名称"].Value.ToString()]);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                    #endregion
                    break;
            }

        }

        #region 右键菜单关联动作
        private void _shutdownAdapter(UltraGridRow row)
        {
            string name = row.Cells["名称"].Value.ToString();
            StatusBar.Panels["Information"].Text = "连接" + name + "...";
            StatusBar.Refresh();

            ICommClient comm = m_Comms[name];
            if (!comm.Start())
            {
                _setStatus(row, "不在线");
                StatusBar.Panels["Information"].Text = "无法连接到" + name + "...";
                return;
            }

            //if (comm.LocalIP != comm.RemoteIP)
            //{
            //    if (DialogResult.Cancel == MessageBox.Show("监控对象: " + name + "未在本地运行, 是否继续?", "请确认",
            //        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
            //    {
            //        StatusBar.Panels["Information"].Text = "已取消对" + name + "的结束操作.";
            //        return;
            //    }
            //}

            StatusBar.Panels["Information"].Text = "结束" + name + "...";
            StatusBar.Refresh();

            CommandMsgV2 cm = new CommandMsgV2();
            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_SHUTDOWN;
            cm.SetValue("NAME", name);
            comm.PostCommand(cm);

            //等待结束
            int i = 0;
            for (; i < 5; ++i)
            {
                if (!comm.Start())
                    break;
                System.Threading.Thread.Sleep(1000);
                comm.Close();
            }

            if (i == 5) //结束失败
            {
                _setStatus(row, "在线");

                StatusBar.Panels["Information"].Text = "未能结束" + name + ".";
            }
            else
            {
                _setStatus(row, "不在线");

                StatusBar.Panels["Information"].Text = name + "已结束.";
            }

            Grid_RunTimeInfo.Refresh();
        }

        private void _stopAdapter(UltraGridRow row)
        {
            string name = row.Cells["名称"].Value.ToString();

            CommandMsgV2 cm = new CommandMsgV2();
            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_STOP;
            cm.SetValue("NAME", name);

            ICommClient comm = m_Comms[name];
            if (!comm.Start())
            {
                Grid_RunTimeInfo.Selected.Rows[0].Cells["状态"].Value = "不在线";
                StatusBar.Panels["Information"].Text = "无法连接监控对象: " + name;
                return;
            }

            _setStatus(row, "在线");

            CommandMsgV2 resp = (CommandMsgV2)comm.SendCommand(cm, 60);
            if (resp == null)
            {
                StatusBar.Panels["Information"].Text = "停止" + name + "采集失败, 通讯超时.";
                return;
            }
            else if (resp.GetValue(Constants.MSG_PARANAME_RESULT).ToString() != "OK")
            {
                MessageBox.Show(name + "采集服务连接正常, 但是停止采集失败, 请检查日志.");
                StatusBar.Panels["Information"].Text = name + "采集服务连接正常, 但是停止采集失败.";
                return;
            }

            StatusBar.Panels["Information"].Text = name + "已停止.";
            _setStatus(row, "停止");
            Grid_RunTimeInfo.Refresh();
            StatusBar.Refresh();
        }

        private void _runAdapter(UltraGridRow row)
        {
            string name = row.Cells["名称"].Value.ToString();
            CommandMsgV2 cm = new CommandMsgV2();
            cm.TK_CommandType = Constants.TK_CommandType.ADAPTER_START;
            cm.SetValue("NAME", name);

            Form_Information m_inform = new Form_Information();
            StatusBar.Panels["Information"].Text = "正在连接" + name + "...";
            StatusBar.Refresh();

            ICommClient comm = m_Comms[name];
            if (!comm.Start())
            {
                StatusBar.Panels["Information"].Text = "连接" + name + "失败, 尝试启动进程...";
                StatusBar.Refresh();

                string file = m_ExecutePaths[name];
                if (System.IO.File.Exists(file))
                {
                    System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                    info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    info.FileName = file;
                    info.WorkingDirectory = file.Substring(0, file.LastIndexOf("\\") + 1);
                    System.Diagnostics.Process.Start(info);
                }
                else
                {
                    StatusBar.Panels["Information"].Text = "无法找到监控对象: " + name + "可执行程序.";
                    _setStatus(row, "不在线");
                    return;
                }
            }

            StatusBar.Panels["Information"].Text = "启动采集...";
            StatusBar.Refresh();

            int i = 0;
            while (!comm.Start() && i < 10)
            {
                ++i;
                System.Threading.Thread.Sleep(1000);
            }

            _setStatus(row, "停止");
            Grid_RunTimeInfo.Refresh();

            if (i == 10)
            {
                StatusBar.Panels["Information"].Text = "无法连接采集进程.";
                _setStatus(row, "未知");
                Grid_RunTimeInfo.Refresh();
                return;
            }

            CommandMsgV2 resp = (CommandMsgV2)comm.SendCommand(cm);
            if (resp == null)
            {
                MessageBox.Show("启动" + name + "采集失败, 通讯超时.");
                _setStatus(row, "未知");
                StatusBar.Panels["Information"].Text = "无法连接" + name + "采集进程.";
                return;
            }
            else if (resp.GetValue(Constants.MSG_PARANAME_RESULT).ToString() != "OK")
            {
                MessageBox.Show(name + "采集进程正常, 但是启动失败, 请检查日志.");
                StatusBar.Panels["Information"].Text = name + "采集进程已运行, 但无法启动采集.";
                return;
            }

            _setStatus(row, "运行");
            StatusBar.Panels["Information"].Text = name + "采集进程成功启动.";
            Grid_RunTimeInfo.Refresh();
        }
        #endregion

        #region 状态信息设置
        private void _setStatus(UltraGridRow row, string status)
        {
            row.Cells["状态"].Value = status;
            row.Cells["时间戳"].Value = System.DateTime.Now;
        }

        delegate void StringParaFunc(string s);
        private void _setStatusBar(string status)
        {
            StatusBar.Panels["Information"].Text = status;
            StatusBar.Refresh();
        }
        #endregion

        private void _closeAdapter()
        {
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (m_AdapterProcess.Contains(p.ProcessName))
                {
                    try
                    {
                        p.Kill();
                    }
                    catch { }
                }
            }
            m_RefreshTimer_Elapsed(null, null);
        }
        private void receiveLog(string sLog)
        {
            try
            {
                string fname = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                string log = DateTime.Now.ToString("HH:mm:ss");
                log += "\t" + sLog + "\r\n";

                if (!File.Exists(fname))
                {
                    using (StreamWriter sw = File.CreateText(fname))
                    {
                        sw.Write(log);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(fname))
                    {
                        sw.Write(log);
                    }
                }
            }
            catch
            { }
        }

        private void selectFile(string fname)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + fname);
            }
            catch (Exception ex)
            {
                receiveLog("定位文件失败:" + fname + " " + ex.Message);
            }
        }
    }
}