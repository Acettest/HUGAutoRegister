using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SystemMon
{
    public partial class Form_AllLog : Form
    {
        public Form_AllLog()
        {
            InitializeComponent();

            Load += new EventHandler(Form_AllLog_Load);
        }

        void Form_AllLog_Load(object sender, EventArgs e)
        {
            foreach (string s in Files)
            {
                Combo_FileList.Items.Add(s);
            }

            if (Combo_FileList.Items.Count > 0)
                Combo_FileList.SelectedIndex = 0;
        }

        public List<string> Files = new List<string>();
        public string WorkingDir = "";

        private void Button_Browse_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (Files.Count == 0)
                    return;

                if (Combo_FileList.SelectedIndex != -1)
                {
                    string fn = WorkingDir + Combo_FileList.SelectedItem.ToString();
                    if (!File.Exists(fn))
                    {
                        MessageBox.Show("找不到" + fn + ", 请检查目录.");
                        return;
                    }

                    using (StreamReader fs = new StreamReader(fn))
                    {
                        Text_FileContent.Text = fs.ReadToEnd();
                    }
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
        }
    }
}