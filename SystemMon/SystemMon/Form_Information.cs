using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SystemMon
{
    public partial class Form_Information : Form
    {
        public Form_Information()
        {
            InitializeComponent();
        }

        public Label InformLabel
        {
            get { return Label_Inform; }
        }
    }
}