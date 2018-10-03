namespace SystemMon
{
    partial class Form_OMCList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_OMCList));
            this.List_OMCList = new Infragistics.Win.UltraWinListView.UltraListView();
            ((System.ComponentModel.ISupportInitialize)(this.List_OMCList)).BeginInit();
            this.SuspendLayout();
            // 
            // List_OMCList
            // 
            this.List_OMCList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.List_OMCList.ItemSettings.DefaultImage = ((System.Drawing.Image)(resources.GetObject("List_OMCList.ItemSettings.DefaultImage")));
            this.List_OMCList.Location = new System.Drawing.Point(0, 0);
            this.List_OMCList.Name = "List_OMCList";
            this.List_OMCList.Size = new System.Drawing.Size(218, 266);
            this.List_OMCList.TabIndex = 0;
            this.List_OMCList.Text = "ultraListView1";
            this.List_OMCList.View = Infragistics.Win.UltraWinListView.UltraListViewStyle.List;
            // 
            // Form_OMCList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(218, 266);
            this.Controls.Add(this.List_OMCList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form_OMCList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "采集源清单";
            ((System.ComponentModel.ISupportInitialize)(this.List_OMCList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Infragistics.Win.UltraWinListView.UltraListView List_OMCList;

    }
}