using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SystemMon
{
    public partial class Form_OMCList : Form
    {
        public Form_OMCList()
        {
            InitializeComponent();

            Load += new EventHandler(Form_OMCList_Load);
        }

        void Form_OMCList_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < OMCList.Count; ++i)
            {
                List_OMCList.Items.Add(i.ToString(), OMCList[i]);
            }
        }

        public List<string> OMCList = new List<string>();
    }
}