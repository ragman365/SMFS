namespace SMFS
{
    partial class PriceSingle
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
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.rtb2 = new EMRControlLib.EMRRichTextBox();
            this.rtb1 = new EMRControlLib.EMRRichTextBox();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(826, 395);
            this.panelAll.TabIndex = 15;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.rtb2);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 165);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(826, 230);
            this.panelBottom.TabIndex = 17;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.Transparent;
            this.panelTop.Controls.Add(this.rtb1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(826, 165);
            this.panelTop.TabIndex = 16;
            // 
            // rtb2
            // 
            this.rtb2.DetectURLs = false;
            this.rtb2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb2.Location = new System.Drawing.Point(0, 0);
            this.rtb2.Name = "rtb2";
            this.rtb2.ShowBold = true;
            this.rtb2.ShowCenterJustify = true;
            this.rtb2.ShowColors = true;
            this.rtb2.ShowFont = true;
            this.rtb2.ShowFontSize = true;
            this.rtb2.ShowItalic = true;
            this.rtb2.ShowLeftJustify = true;
            this.rtb2.ShowOpen = true;
            this.rtb2.ShowRedo = true;
            this.rtb2.ShowRightJustify = true;
            this.rtb2.ShowSave = true;
            this.rtb2.ShowStamp = true;
            this.rtb2.ShowStrikeout = true;
            this.rtb2.ShowUnderline = true;
            this.rtb2.ShowUndo = true;
            this.rtb2.Size = new System.Drawing.Size(826, 230);
            this.rtb2.StampAction = EMRControlLib.StampActions.EditedBy;
            this.rtb2.StampColor = System.Drawing.Color.Blue;
            this.rtb2.TabIndex = 0;
            // 
            // rtb1
            // 
            this.rtb1.DetectURLs = false;
            this.rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb1.Location = new System.Drawing.Point(0, 0);
            this.rtb1.Name = "rtb1";
            this.rtb1.ShowBold = true;
            this.rtb1.ShowCenterJustify = true;
            this.rtb1.ShowColors = true;
            this.rtb1.ShowFont = true;
            this.rtb1.ShowFontSize = true;
            this.rtb1.ShowItalic = true;
            this.rtb1.ShowLeftJustify = true;
            this.rtb1.ShowOpen = true;
            this.rtb1.ShowRedo = true;
            this.rtb1.ShowRightJustify = true;
            this.rtb1.ShowSave = true;
            this.rtb1.ShowStamp = true;
            this.rtb1.ShowStrikeout = true;
            this.rtb1.ShowUnderline = true;
            this.rtb1.ShowUndo = true;
            this.rtb1.Size = new System.Drawing.Size(826, 165);
            this.rtb1.StampAction = EMRControlLib.StampActions.EditedBy;
            this.rtb1.StampColor = System.Drawing.Color.Blue;
            this.rtb1.TabIndex = 1;
            // 
            // PriceSingle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(826, 395);
            this.Controls.Add(this.panelAll);
            this.Name = "PriceSingle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Price Top & Bottom Line Descriptions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PriceLists_FormClosing);
            this.Load += new System.EventHandler(this.PriceSingle_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private EMRControlLib.EMRRichTextBox rtb2;
        private EMRControlLib.EMRRichTextBox rtb1;
    }
}