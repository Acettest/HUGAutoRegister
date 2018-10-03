using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace YWTWebServiceTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataTable NewDt = new DataTable();
            NewDt.Columns.Add("Id", typeof(Int32));
            NewDt.Columns.Add("Name", typeof(string));

            DataRow row = NewDt.NewRow();

            row["Id"] = 1;
            row["Name"] = "香蕉";
            NewDt.Rows.Add(row);

            row = NewDt.NewRow();
            row["Id"] = 2;
            row["Name"] = "葡萄";
            NewDt.Rows.Add(row);

            row = NewDt.NewRow();
            row["Id"] = 3;
            row["Name"] = "西瓜";
            NewDt.Rows.Add(row);

            row = NewDt.NewRow();
            row["Id"] = 4;
            row["Name"] = "苹果";
            NewDt.Rows.Add(row);

            row = NewDt.NewRow();
            row["Id"] = 5;
            row["Name"] = "榴莲";
            NewDt.Rows.Add(row);

            this.checkedListBox1.DataSource = NewDt;
            this.checkedListBox1.DisplayMember = "Name";
            this.checkedListBox1.ValueMember = "Id";


        }

        //取出Value
        private void button1_Click(object sender, EventArgs e)
        {
            string JD = "";
            if (checkedListBox1.CheckedItems.Count != 0)
            {
                for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
                {
                    JD += ((System.Data.DataRowView)checkedListBox1.CheckedItems[i])[0].ToString() + ",";
                }
            }
            this.textBox1.Text = JD.TrimEnd(',');
        }

        //取出Name
        private void button2_Click(object sender, EventArgs e)
        {
            string JD = "";
            string checkedText = "";
            foreach (System.Data.DataRowView item in this.checkedListBox1.CheckedItems)
            {

                JD += item.Row["Id"].ToString() + ",";
                checkedText += item.Row["Name"].ToString() + ",";
            }
            this.textBox1.Text = JD.TrimEnd(',');
            this.textBox2.Text = checkedText.TrimEnd(',');
        }
    }
}

