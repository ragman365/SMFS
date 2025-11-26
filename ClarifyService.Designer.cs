namespace SMFS
{
    partial class ClarifyService
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
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.cmbCasketType = new System.Windows.Forms.ComboBox();
            this.txtCasketGauge = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.txtCasketCost = new System.Windows.Forms.TextBox();
            this.txtCasketDesc = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCasketCode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbAsCash = new System.Windows.Forms.ComboBox();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(31, 17);
            this.btnAccept.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(87, 28);
            this.btnAccept.TabIndex = 12;
            this.btnAccept.Text = "Accept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(163, 17);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 28);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(608, 290);
            this.panelAll.TabIndex = 16;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.cmbAsCash);
            this.panelBottom.Controls.Add(this.label7);
            this.panelBottom.Controls.Add(this.cmbCasketType);
            this.panelBottom.Controls.Add(this.txtCasketGauge);
            this.panelBottom.Controls.Add(this.label5);
            this.panelBottom.Controls.Add(this.label4);
            this.panelBottom.Controls.Add(this.cmbType);
            this.panelBottom.Controls.Add(this.txtCasketCost);
            this.panelBottom.Controls.Add(this.txtCasketDesc);
            this.panelBottom.Controls.Add(this.label1);
            this.panelBottom.Controls.Add(this.txtCasketCode);
            this.panelBottom.Controls.Add(this.label2);
            this.panelBottom.Controls.Add(this.label3);
            this.panelBottom.Controls.Add(this.label6);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 71);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(608, 219);
            this.panelBottom.TabIndex = 18;
            // 
            // cmbCasketType
            // 
            this.cmbCasketType.FormattingEnabled = true;
            this.cmbCasketType.Items.AddRange(new object[] {
            "Casket",
            "Vault",
            "Urn"});
            this.cmbCasketType.Location = new System.Drawing.Point(169, 71);
            this.cmbCasketType.Name = "cmbCasketType";
            this.cmbCasketType.Size = new System.Drawing.Size(357, 24);
            this.cmbCasketType.TabIndex = 18;
            // 
            // txtCasketGauge
            // 
            this.txtCasketGauge.Location = new System.Drawing.Point(168, 102);
            this.txtCasketGauge.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCasketGauge.Name = "txtCasketGauge";
            this.txtCasketGauge.Size = new System.Drawing.Size(357, 23);
            this.txtCasketGauge.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(68, 104);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 17);
            this.label5.TabIndex = 16;
            this.label5.Text = "Casket Gauge :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(69, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "Casket Type :";
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Casket",
            "Vault",
            "Urn"});
            this.cmbType.Location = new System.Drawing.Point(169, 155);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(90, 24);
            this.cmbType.TabIndex = 13;
            this.cmbType.Text = "Casket";
            // 
            // txtCasketCost
            // 
            this.txtCasketCost.Location = new System.Drawing.Point(169, 130);
            this.txtCasketCost.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCasketCost.Name = "txtCasketCost";
            this.txtCasketCost.Size = new System.Drawing.Size(357, 23);
            this.txtCasketCost.TabIndex = 7;
            this.txtCasketCost.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCasketCost_KeyUp);
            // 
            // txtCasketDesc
            // 
            this.txtCasketDesc.Location = new System.Drawing.Point(169, 41);
            this.txtCasketDesc.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCasketDesc.Name = "txtCasketDesc";
            this.txtCasketDesc.Size = new System.Drawing.Size(357, 23);
            this.txtCasketDesc.TabIndex = 6;
            this.txtCasketDesc.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCasketDesc_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Casket Code :";
            // 
            // txtCasketCode
            // 
            this.txtCasketCode.Location = new System.Drawing.Point(169, 12);
            this.txtCasketCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCasketCode.Name = "txtCasketCode";
            this.txtCasketCode.Size = new System.Drawing.Size(357, 23);
            this.txtCasketCode.TabIndex = 5;
            this.txtCasketCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCasketCode_KeyUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Casket Description :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(115, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Cost :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(115, 158);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 17);
            this.label6.TabIndex = 12;
            this.label6.Text = "Type :";
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnAccept);
            this.panelTop.Controls.Add(this.btnCancel);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(608, 71);
            this.panelTop.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(97, 186);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 17);
            this.label7.TabIndex = 19;
            this.label7.Text = "As Cash :";
            // 
            // cmbAsCash
            // 
            this.cmbAsCash.FormattingEnabled = true;
            this.cmbAsCash.Items.AddRange(new object[] {
            "Yes",
            "No"});
            this.cmbAsCash.Location = new System.Drawing.Point(169, 183);
            this.cmbAsCash.Name = "cmbAsCash";
            this.cmbAsCash.Size = new System.Drawing.Size(66, 24);
            this.cmbAsCash.TabIndex = 20;
            this.cmbAsCash.Text = "No";
            // 
            // ClarifyService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 290);
            this.Controls.Add(this.panelAll);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ClarifyService";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clarify Item";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Clarify_FormClosed);
            this.Load += new System.EventHandler(this.ClarifyService_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ComboBox cmbCasketType;
        private System.Windows.Forms.TextBox txtCasketGauge;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.TextBox txtCasketCost;
        private System.Windows.Forms.TextBox txtCasketDesc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCasketCode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbAsCash;
        private System.Windows.Forms.Label label7;
    }
}