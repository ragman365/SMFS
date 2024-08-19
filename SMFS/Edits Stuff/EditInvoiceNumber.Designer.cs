namespace SMFS
{
    partial class EditInvoiceNumber
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
            this.txtPart2 = new System.Windows.Forms.TextBox();
            this.lblContract = new System.Windows.Forms.Label();
            this.btnAccept = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtInvoiceNumber = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPart1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtPart2
            // 
            this.txtPart2.Location = new System.Drawing.Point(136, 114);
            this.txtPart2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPart2.Name = "txtPart2";
            this.txtPart2.Size = new System.Drawing.Size(221, 23);
            this.txtPart2.TabIndex = 1;
            this.txtPart2.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // lblContract
            // 
            this.lblContract.AutoSize = true;
            this.lblContract.Location = new System.Drawing.Point(73, 117);
            this.lblContract.Name = "lblContract";
            this.lblContract.Size = new System.Drawing.Size(50, 17);
            this.lblContract.TabIndex = 2;
            this.lblContract.Text = "Part 2:";
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(16, 160);
            this.btnAccept.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(199, 69);
            this.btnAccept.TabIndex = 5;
            this.btnAccept.Text = "Accept Edit";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Invoice Number :";
            // 
            // txtInvoiceNumber
            // 
            this.txtInvoiceNumber.Location = new System.Drawing.Point(136, 17);
            this.txtInvoiceNumber.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtInvoiceNumber.Name = "txtInvoiceNumber";
            this.txtInvoiceNumber.Size = new System.Drawing.Size(221, 23);
            this.txtInvoiceNumber.TabIndex = 6;
            this.txtInvoiceNumber.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(77, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Part 1:";
            // 
            // txtPart1
            // 
            this.txtPart1.Location = new System.Drawing.Point(136, 76);
            this.txtPart1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPart1.Name = "txtPart1";
            this.txtPart1.Size = new System.Drawing.Size(221, 23);
            this.txtPart1.TabIndex = 8;
            this.txtPart1.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // EditInvoiceNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 246);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPart1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtInvoiceNumber);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.lblContract);
            this.Controls.Add(this.txtPart2);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "EditInvoiceNumber";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit Invoice Number";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditACHLine_FormClosing);
            this.Load += new System.EventHandler(this.EditInvoiceNumber_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtPart2;
        private System.Windows.Forms.Label lblContract;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInvoiceNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPart1;
    }
}