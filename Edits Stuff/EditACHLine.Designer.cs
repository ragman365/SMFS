namespace SMFS
{
    partial class EditACHLine
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
            this.txtContract = new System.Windows.Forms.TextBox();
            this.lblContract = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPayment = new System.Windows.Forms.TextBox();
            this.btnAccept = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDate = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtExpected = new System.Windows.Forms.TextBox();
            this.lblPayer = new System.Windows.Forms.Label();
            this.txtPayer = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtContract
            // 
            this.txtContract.Location = new System.Drawing.Point(86, 65);
            this.txtContract.Name = "txtContract";
            this.txtContract.Size = new System.Drawing.Size(100, 21);
            this.txtContract.TabIndex = 1;
            this.txtContract.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // lblContract
            // 
            this.lblContract.AutoSize = true;
            this.lblContract.Location = new System.Drawing.Point(24, 68);
            this.lblContract.Name = "lblContract";
            this.lblContract.Size = new System.Drawing.Size(56, 13);
            this.lblContract.TabIndex = 2;
            this.lblContract.Text = "Contract :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Payment :";
            // 
            // txtPayment
            // 
            this.txtPayment.Location = new System.Drawing.Point(85, 127);
            this.txtPayment.Name = "txtPayment";
            this.txtPayment.Size = new System.Drawing.Size(100, 21);
            this.txtPayment.TabIndex = 3;
            this.txtPayment.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(25, 181);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(171, 56);
            this.btnAccept.TabIndex = 5;
            this.btnAccept.Text = "Accept Edit";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(42, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Code :";
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(86, 11);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(100, 21);
            this.txtCode.TabIndex = 6;
            this.txtCode.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Location :";
            // 
            // txtLocation
            // 
            this.txtLocation.Location = new System.Drawing.Point(86, 38);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.Size = new System.Drawing.Size(100, 21);
            this.txtLocation.TabIndex = 8;
            this.txtLocation.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Date :";
            // 
            // txtDate
            // 
            this.txtDate.Location = new System.Drawing.Point(85, 154);
            this.txtDate.Name = "txtDate";
            this.txtDate.Size = new System.Drawing.Size(100, 21);
            this.txtDate.TabIndex = 10;
            this.txtDate.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 98);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Expected :";
            // 
            // txtExpected
            // 
            this.txtExpected.Location = new System.Drawing.Point(86, 95);
            this.txtExpected.Name = "txtExpected";
            this.txtExpected.Size = new System.Drawing.Size(100, 21);
            this.txtExpected.TabIndex = 15;
            this.txtExpected.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // lblPayer
            // 
            this.lblPayer.AutoSize = true;
            this.lblPayer.Location = new System.Drawing.Point(208, 68);
            this.lblPayer.Name = "lblPayer";
            this.lblPayer.Size = new System.Drawing.Size(42, 13);
            this.lblPayer.TabIndex = 18;
            this.lblPayer.Text = "Payer :";
            // 
            // txtPayer
            // 
            this.txtPayer.Location = new System.Drawing.Point(253, 65);
            this.txtPayer.Name = "txtPayer";
            this.txtPayer.Size = new System.Drawing.Size(100, 21);
            this.txtPayer.TabIndex = 17;
            // 
            // EditACHLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 251);
            this.Controls.Add(this.lblPayer);
            this.Controls.Add(this.txtPayer);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtExpected);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtLocation);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPayment);
            this.Controls.Add(this.lblContract);
            this.Controls.Add(this.txtContract);
            this.Name = "EditACHLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit ACH Line";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditACHLine_FormClosing);
            this.Load += new System.EventHandler(this.EditACHLine_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtContract;
        private System.Windows.Forms.Label lblContract;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPayment;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDate;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtExpected;
        private System.Windows.Forms.Label lblPayer;
        private System.Windows.Forms.TextBox txtPayer;
    }
}