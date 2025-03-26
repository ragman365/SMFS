namespace SMFS
{
    partial class NewInsurance
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
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.contractNumber = new System.Windows.Forms.TextBox();
            this.firstName = new System.Windows.Forms.TextBox();
            this.lastName = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.txtPayer = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtServiceID = new System.Windows.Forms.TextBox();
            this.middleName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panelAll.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(55, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Payer :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "Contract # :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "First Name :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 144);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 31;
            this.label3.Text = "Last Name :";
            // 
            // contractNumber
            // 
            this.contractNumber.Enabled = false;
            this.contractNumber.Location = new System.Drawing.Point(104, 30);
            this.contractNumber.Name = "contractNumber";
            this.contractNumber.Size = new System.Drawing.Size(91, 21);
            this.contractNumber.TabIndex = 34;
            // 
            // firstName
            // 
            this.firstName.Location = new System.Drawing.Point(104, 84);
            this.firstName.Name = "firstName";
            this.firstName.Size = new System.Drawing.Size(133, 21);
            this.firstName.TabIndex = 35;
            // 
            // lastName
            // 
            this.lastName.Location = new System.Drawing.Point(104, 139);
            this.lastName.Name = "lastName";
            this.lastName.Size = new System.Drawing.Size(109, 21);
            this.lastName.TabIndex = 36;
            // 
            // btnAdd
            // 
            this.btnAdd.Font = new System.Drawing.Font("Tahoma", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnAdd.Location = new System.Drawing.Point(23, 178);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 42);
            this.btnAdd.TabIndex = 37;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Tahoma", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnCancel.Location = new System.Drawing.Point(120, 178);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 42);
            this.btnCancel.TabIndex = 38;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(256, 235);
            this.panelAll.TabIndex = 40;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.txtPayer);
            this.panelTop.Controls.Add(this.label7);
            this.panelTop.Controls.Add(this.txtServiceID);
            this.panelTop.Controls.Add(this.middleName);
            this.panelTop.Controls.Add(this.label4);
            this.panelTop.Controls.Add(this.firstName);
            this.panelTop.Controls.Add(this.label5);
            this.panelTop.Controls.Add(this.btnCancel);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.btnAdd);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.lastName);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.contractNumber);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(256, 235);
            this.panelTop.TabIndex = 41;
            // 
            // txtPayer
            // 
            this.txtPayer.Location = new System.Drawing.Point(104, 4);
            this.txtPayer.Name = "txtPayer";
            this.txtPayer.Size = new System.Drawing.Size(100, 21);
            this.txtPayer.TabIndex = 44;
            this.txtPayer.TextChanged += new System.EventHandler(this.txtPayer_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(37, 63);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 42;
            this.label7.Text = "Service ID :";
            // 
            // txtServiceID
            // 
            this.txtServiceID.Location = new System.Drawing.Point(104, 58);
            this.txtServiceID.Name = "txtServiceID";
            this.txtServiceID.Size = new System.Drawing.Size(91, 21);
            this.txtServiceID.TabIndex = 43;
            // 
            // middleName
            // 
            this.middleName.Location = new System.Drawing.Point(104, 112);
            this.middleName.Name = "middleName";
            this.middleName.Size = new System.Drawing.Size(100, 21);
            this.middleName.TabIndex = 41;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 40;
            this.label4.Text = "Middle Name :";
            // 
            // NewInsurance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(256, 235);
            this.Controls.Add(this.panelAll);
            this.Name = "NewInsurance";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create New Insurance";
            this.Load += new System.EventHandler(this.NewInsurance_Load);
            this.panelAll.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox contractNumber;
        private System.Windows.Forms.TextBox firstName;
        private System.Windows.Forms.TextBox lastName;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox middleName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtServiceID;
        private System.Windows.Forms.TextBox txtPayer;
    }
}