namespace xabg.GroundScaleSimulator
{
    partial class FormMessageBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMessageBox));
            this.RxtDataOut = new System.Windows.Forms.RichTextBox();
            this.TspTools = new System.Windows.Forms.ToolStrip();
            this.TSBtnClear = new System.Windows.Forms.ToolStripButton();
            this.metroStyleManager1 = new MetroFramework.Components.MetroStyleManager(this.components);
            this.TspTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // RxtDataOut
            // 
            this.RxtDataOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RxtDataOut.Location = new System.Drawing.Point(10, 85);
            this.RxtDataOut.Name = "RxtDataOut";
            this.RxtDataOut.Size = new System.Drawing.Size(364, 555);
            this.RxtDataOut.TabIndex = 0;
            this.RxtDataOut.Text = "";
            // 
            // TspTools
            // 
            this.TspTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSBtnClear});
            this.TspTools.Location = new System.Drawing.Point(10, 60);
            this.TspTools.Name = "TspTools";
            this.TspTools.Size = new System.Drawing.Size(364, 25);
            this.TspTools.TabIndex = 1;
            this.TspTools.Text = "toolStrip1";
            // 
            // TSBtnClear
            // 
            this.TSBtnClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TSBtnClear.Image = ((System.Drawing.Image)(resources.GetObject("TSBtnClear.Image")));
            this.TSBtnClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TSBtnClear.Name = "TSBtnClear";
            this.TSBtnClear.Size = new System.Drawing.Size(23, 22);
            this.TSBtnClear.Text = "toolStripButton1";
            this.TSBtnClear.Click += new System.EventHandler(this.TSBtnClear_Click);
            // 
            // metroStyleManager1
            // 
            this.metroStyleManager1.Owner = null;
            // 
            // FormMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 650);
            this.Controls.Add(this.RxtDataOut);
            this.Controls.Add(this.TspTools);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMessageBox";
            this.Padding = new System.Windows.Forms.Padding(10, 60, 10, 10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Style = MetroFramework.MetroColorStyle.Brown;
            this.Text = "数据输出";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.SteelBlue;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMessageBox_FormClosing);
            this.TspTools.ResumeLayout(false);
            this.TspTools.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox RxtDataOut;
        private System.Windows.Forms.ToolStrip TspTools;
        private System.Windows.Forms.ToolStripButton TSBtnClear;
        private MetroFramework.Components.MetroStyleManager metroStyleManager1;
    }
}