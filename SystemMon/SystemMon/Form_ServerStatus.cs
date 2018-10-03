using System;
using System.Data;
using System.Windows.Forms;

namespace SystemMon
{
    public partial class Form_ServerStatus : Form
    {
        private DataTable m_Status = new DataTable();
        public Form_ServerStatus()
        {
            InitializeComponent();

            m_Status.Columns.AddRange(new DataColumn[]{
                new DataColumn("属性", typeof(string)),
                new DataColumn("数值", typeof(string))});

            m_Status.Rows.Add(new object[] { "名称", "" });
            m_Status.Rows.Add(new object[] { "进程号", "" });
            m_Status.Rows.Add(new object[] { "线程数", "" });
            m_Status.Rows.Add(new object[] { "物理内存", "" });
            m_Status.Rows.Add(new object[] { "启动时间", "" });
            m_Status.Rows.Add(new object[] { "CPU时间(分钟)", "" });
            m_Status.Rows.Add(new object[] { "状态", "" });
            m_Status.Rows.Add(new object[] { "告警客户端数量", "" });
            m_Status.Rows.Add(new object[] { "采集器数量", "" });
            m_Status.Rows.Add(new object[] { "活动告警数", "" });

            m_Status.AcceptChanges();

            Grid_Status.SetDataBinding(m_Status, null);
            Grid_Status.DisplayLayout.Bands[0].Columns[0].Width = 120;

            FormClosing += new FormClosingEventHandler(Form_ServerStatus_FormClosing);
        }

        void Form_ServerStatus_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Hide();
                e.Cancel = true;
            }
        }

        public void RefreshStatus(string name, int pid, int threadcount, long phymem,
            DateTime starttime, long elaspedtime,
            int status, int authorized_alarmclientnum, int nonauthorized_alarmclientnum, int adapterclientnum, int activealarmnum)
        {
            m_Status.Rows[0][1] = name;
            m_Status.Rows[1][1] = (pid == -1) ? "未知" : pid.ToString();
            m_Status.Rows[2][1] = (threadcount == -1) ? "未知" : threadcount.ToString();
            m_Status.Rows[3][1] = (phymem == -1) ? "未知" : phymem.ToString() + " (字节)";
            m_Status.Rows[4][1] = (starttime == System.DateTime.MinValue) ? "未知" : starttime.ToString();
            m_Status.Rows[5][1] = (elaspedtime == -1) ? "未知" : elaspedtime.ToString();

            switch (status)
            {
                case -1:
                    m_Status.Rows[6][1] = "未知";
                    break;
                case 0:
                    m_Status.Rows[6][1] = "停止";
                    break;
                case 1:
                    m_Status.Rows[6][1] = "运行";
                    break;
            }

            m_Status.Rows[7][1] = (authorized_alarmclientnum == -1) ? "未知" : (authorized_alarmclientnum.ToString() + "(" + nonauthorized_alarmclientnum.ToString() + ")");
            m_Status.Rows[8][1] = (adapterclientnum == -1) ? "未知" : adapterclientnum.ToString();
            m_Status.Rows[9][1] = (activealarmnum == -1) ? "未知" : activealarmnum.ToString();
        }
    }
}