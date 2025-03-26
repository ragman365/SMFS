namespace SMFS
{
    partial class CasketPackageView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CasketPackageView));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblWhich = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtBasicService = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCasketCost = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            this.panelAll.Size = new System.Drawing.Size(956, 514);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.pictureBox1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 38);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(956, 476);
            this.panelBottom.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(956, 476);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.lblWhich);
            this.panelTop.Controls.Add(this.cmbType);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.btnSelect);
            this.panelTop.Controls.Add(this.txtBasicService);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.txtCasketCost);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.btnRight);
            this.panelTop.Controls.Add(this.btnLeft);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(956, 38);
            this.panelTop.TabIndex = 1;
            // 
            // lblWhich
            // 
            this.lblWhich.AutoSize = true;
            this.lblWhich.Location = new System.Drawing.Point(467, 14);
            this.lblWhich.Name = "lblWhich";
            this.lblWhich.Size = new System.Drawing.Size(46, 13);
            this.lblWhich.TabIndex = 31;
            this.lblWhich.Text = "lblWhich";
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(359, 10);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(102, 21);
            this.cmbType.TabIndex = 30;
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(321, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Type :";
            // 
            // btnSelect
            // 
            this.btnSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnSelect.Location = new System.Drawing.Point(100, 9);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(209, 23);
            this.btnSelect.TabIndex = 28;
            this.btnSelect.Text = "Select this Merchandise";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtBasicService
            // 
            this.txtBasicService.Location = new System.Drawing.Point(872, 9);
            this.txtBasicService.Name = "txtBasicService";
            this.txtBasicService.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtBasicService.Size = new System.Drawing.Size(68, 21);
            this.txtBasicService.TabIndex = 27;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(739, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Basic Service W/O Vault :";
            // 
            // txtCasketCost
            // 
            this.txtCasketCost.Location = new System.Drawing.Point(665, 9);
            this.txtCasketCost.Name = "txtCasketCost";
            this.txtCasketCost.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtCasketCost.Size = new System.Drawing.Size(68, 21);
            this.txtCasketCost.TabIndex = 25;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(593, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Casket Cost :";
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.PeachPuff;
            this.btnRight.Image = ((System.Drawing.Image)(resources.GetObject("btnRight.Image")));
            this.btnRight.Location = new System.Drawing.Point(43, 9);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(28, 23);
            this.btnRight.TabIndex = 23;
            this.btnRight.UseVisualStyleBackColor = false;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.PeachPuff;
            this.btnLeft.Image = ((System.Drawing.Image)(resources.GetObject("btnLeft.Image")));
            this.btnLeft.Location = new System.Drawing.Point(9, 9);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(28, 23);
            this.btnLeft.TabIndex = 19;
            this.btnLeft.UseVisualStyleBackColor = false;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // CasketPackageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 514);
            this.Controls.Add(this.panelAll);
            this.Name = "CasketPackageView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "CasketPackageView";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CasketPackageView_FormClosing);
            this.Load += new System.EventHandler(this.CasketPackageView_Load);
            this.Resize += new System.EventHandler(this.CasketPackageView_Resize);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.TextBox txtBasicService;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCasketCost;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblWhich;
    }
}