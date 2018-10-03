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
                new DataColumn("����", typeof(string)),
                new DataColumn("��ֵ", typeof(string))});

            m_Status.Rows.Add(new object[] { "����", "" });
            m_Status.Rows.Add(new object[] { "���̺�", "" });
            m_Status.Rows.Add(new object[] { "�߳���", "" });
            m_Status.Rows.Add(new object[] { "�����ڴ�", "" });
            m_Status.Rows.Add(new object[] { "����ʱ��", "" });
            m_Status.Rows.Add(new object[] { "CPUʱ��(����)", "" });
            m_Status.Rows.Add(new object[] { "״̬", "" });
            m_Status.Rows.Add(new object[] { "�澯�ͻ�������", "" });
            m_Status.Rows.Add(new object[] { "�ɼ�������", "" });
            m_Status.Rows.Add(new object[] { "��澯��", "" });

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
            m_Status.Rows[1][1] = (pid == -1) ? "δ֪" : pid.ToString();
            m_Status.Rows[2][1] = (threadcount == -1) ? "δ֪" : threadcount.ToString();
            m_Status.Rows[3][1] = (phymem == -1) ? "δ֪" : phymem.ToString() + " (�ֽ�)";
            m_Status.Rows[4][1] = (starttime == System.DateTime.MinValue) ? "δ֪" : starttime.ToString();
            m_Status.Rows[5][1] = (elaspedtime == -1) ? "δ֪" : elaspedtime.ToString();

            switch (status)
            {
                case -1:
                    m_Status.Rows[6][1] = "δ֪";
                    break;
                case 0:
                    m_Status.Rows[6][1] = "ֹͣ";
                    break;
                case 1:
                    m_Status.Rows[6][1] = "����";
                    break;
            }

            m_Status.Rows[7][1] = (authorized_alarmclientnum == -1) ? "δ֪" : (authorized_alarmclientnum.ToString() + "(" + nonauthorized_alarmclientnum.ToString() + ")");
            m_Status.Rows[8][1] = (adapterclientnum == -1) ? "δ֪" : adapterclientnum.ToString();
            m_Status.Rows[9][1] = (activealarmnum == -1) ? "δ֪" : activealarmnum.ToString();
        }
    }
}