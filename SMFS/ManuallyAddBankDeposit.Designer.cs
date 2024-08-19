namespace SMFS
{
    partial class ManuallyAddBankDeposit
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
            this.lblCurrentPrice = new System.Windows.Forms.Label();
            this.txtBankAccount = new System.Windows.Forms.TextBox();
            this.txtDebit = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnSelectBank = new System.Windows.Forms.Button();
            this.txtDate = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtCredit = new System.Windows.Forms.TextBox();
            this.panelAll.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(97, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 17);
            this.label5.TabIndex = 26;
            this.label5.Text = "Date :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 17);
            this.label1.TabIndex = 29;
            this.label1.Text = "Bank Account :";
            // 
            // lblCurrentPrice
            // 
            this.lblCurrentPrice.AutoSize = true;
            this.lblCurrentPrice.Location = new System.Drawing.Point(94, 74);
            this.lblCurrentPrice.Name = "lblCurrentPrice";
            this.lblCurrentPrice.Size = new System.Drawing.Size(49, 17);
            this.lblCurrentPrice.TabIndex = 30;
            this.lblCurrentPrice.Text = "Debit :";
            // 
            // txtBankAccount
            // 
            this.txtBankAccount.Location = new System.Drawing.Point(151, 102);
            this.txtBankAccount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBankAccount.Name = "txtBankAccount";
            this.txtBankAccount.Size = new System.Drawing.Size(116, 23);
            this.txtBankAccount.TabIndex = 2;
            this.txtBankAccount.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtService_KeyUp);
            // 
            // txtDebit
            // 
            this.txtDebit.Location = new System.Drawing.Point(151, 71);
            this.txtDebit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDebit.Name = "txtDebit";
            this.txtDebit.Size = new System.Drawing.Size(116, 23);
            this.txtDebit.TabIndex = 4;
            this.txtDebit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCurrentPrice_KeyDown);
            this.txtDebit.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCurrentPrice_KeyUp);
            // 
            // btnAdd
            // 
            this.btnAdd.Font = new System.Drawing.Font("Tahoma", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnAdd.Location = new System.Drawing.Point(21, 170);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(87, 52);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "Accept";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Tahoma", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnCancel.Location = new System.Drawing.Point(409, 170);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 52);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(578, 239);
            this.panelAll.TabIndex = 40;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.txtDescription);
            this.panelTop.Controls.Add(this.btnSelectBank);
            this.panelTop.Controls.Add(this.txtDate);
            this.panelTop.Controls.Add(this.label7);
            this.panelTop.Controls.Add(this.txtCredit);
            this.panelTop.Controls.Add(this.txtDebit);
            this.panelTop.Controls.Add(this.label5);
            this.panelTop.Controls.Add(this.btnCancel);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.btnAdd);
            this.panelTop.Controls.Add(this.lblCurrentPrice);
            this.panelTop.Controls.Add(this.txtBankAccount);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(578, 239);
            this.panelTop.TabIndex = 41;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(58, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 17);
            this.label2.TabIndex = 45;
            this.label2.Text = "Description :";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(151, 136);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(345, 23);
            this.txtDescription.TabIndex = 44;
            this.txtDescription.TextChanged += new System.EventHandler(this.txtDescription_TextChanged);
            // 
            // btnSelectBank
            // 
            this.btnSelectBank.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectBank.Location = new System.Drawing.Point(273, 94);
            this.btnSelectBank.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectBank.Name = "btnSelectBank";
            this.btnSelectBank.Size = new System.Drawing.Size(182, 35);
            this.btnSelectBank.TabIndex = 43;
            this.btnSelectBank.Text = "Select Bank Account";
            this.btnSelectBank.UseVisualStyleBackColor = true;
            this.btnSelectBank.Click += new System.EventHandler(this.btnSelectBank_Click);
            // 
            // txtDate
            // 
            this.txtDate.Location = new System.Drawing.Point(151, 9);
            this.txtDate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDate.Name = "txtDate";
            this.txtDate.Size = new System.Drawing.Size(116, 23);
            this.txtDate.TabIndex = 1;
            this.txtDate.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtType_KeyUp);
            this.txtDate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtDate_MouseDown);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(94, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 17);
            this.label7.TabIndex = 42;
            this.label7.Text = "Credit:";
            // 
            // txtCredit
            // 
            this.txtCredit.Location = new System.Drawing.Point(151, 40);
            this.txtCredit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCredit.Name = "txtCredit";
            this.txtCredit.Size = new System.Drawing.Size(116, 23);
            this.txtCredit.TabIndex = 3;
            this.txtCredit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCustomerPrice_KeyDown);
            this.txtCredit.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCustomerPrice_KeyUp);
            // 
            // ManuallyAddBankDeposit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 239);
            this.Controls.Add(this.panelAll);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ManuallyAddBankDeposit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manually Edit Service";
            this.Load += new System.EventHandler(this.ManuallyAddBankDeposit_Load);
            this.panelAll.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCurrentPrice;
        private System.Windows.Forms.TextBox txtBankAccount;
        private System.Windows.Forms.TextBox txtDebit;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtCredit;
        private System.Windows.Forms.TextBox txtDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button btnSelectBank;
    }
}