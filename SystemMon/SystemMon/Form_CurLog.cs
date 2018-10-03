using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SystemMon
{
    public partial class Form_CurLog : Form
    {
        public Form_CurLog()
        {
            InitializeComponent();

            FormClosing += new FormClosingEventHandler(Form_CurLog_FormClosing);
        }

        void Form_CurLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                TextEditor.Text = "";
                Hide();

                e.Cancel = true;
            }
        }

        public Infragistics.Win.UltraWinEditors.UltraTextEditor TextLog
        {
            get { return TextEditor; }
        }
    }
}