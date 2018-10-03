namespace SystemMon
{
    partial class Form_AllLog
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.Button_Browse = new Infragistics.Win.Misc.UltraButton();
            this.Combo_FileList = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.Text_FileContent = new Infragistics.Win.FormattedLinkLabel.UltraFormattedTextEditor();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Combo_FileList)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Button_Browse);
            this.panel1.Controls.Add(this.Combo_FileList);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(418, 32);
            this.panel1.TabIndex = 0;
            // 
            // Button_Browse
            // 
            this.Button_Browse.Location = new System.Drawing.Point(153, 2);
            this.Button_Browse.Name = "Button_Browse";
            this.Button_Browse.Size = new System.Drawing.Size(75, 24);
            this.Button_Browse.TabIndex = 1;
            this.Button_Browse.Text = "查看(&B)";
            this.Button_Browse.Click += new System.EventHandler(this.Button_Browse_Click);
            // 
            // Combo_FileList
            // 
            this.Combo_FileList.Location = new System.Drawing.Point(3, 3);
            this.Combo_FileList.Name = "Combo_FileList";
            this.Combo_FileList.Size = new System.Drawing.Size(144, 21);
            this.Combo_FileList.TabIndex = 0;
            // 
            // Text_FileContent
            // 
            this.Text_FileContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Text_FileContent.Location = new System.Drawing.Point(0, 32);
            this.Text_FileContent.Name = "Text_FileContent";
            this.Text_FileContent.Size = new System.Drawing.Size(418, 234);
            this.Text_FileContent.TabIndex = 1;
            this.Text_FileContent.Value = "";
            // 
            // Form_AllLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 266);
            this.Controls.Add(this.Text_FileContent);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form_AllLog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "查看日志文件";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Combo_FileList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private Infragistics.Win.Misc.UltraButton Button_Browse;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor Combo_FileList;
        private Infragistics.Win.FormattedLinkLabel.UltraFormattedTextEditor Text_FileContent;
    }
}