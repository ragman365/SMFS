namespace SMFS
{
    partial class ManuallyEditService
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
            this.txtService = new System.Windows.Forms.TextBox();
            this.txtCurrentPrice = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.chkSalesTax = new System.Windows.Forms.CheckBox();
            this.txtType = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtCustomerPrice = new System.Windows.Forms.TextBox();
            this.panelAll.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(97, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 17);
            this.label5.TabIndex = 26;
            this.label5.Text = "Type :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(84, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 17);
            this.label1.TabIndex = 29;
            this.label1.Text = "Service :";
            // 
            // lblCurrentPrice
            // 
            this.lblCurrentPrice.AutoSize = true;
            this.lblCurrentPrice.Location = new System.Drawing.Point(52, 110);
            this.lblCurrentPrice.Name = "lblCurrentPrice";
            this.lblCurrentPrice.Size = new System.Drawing.Size(97, 17);
            this.lblCurrentPrice.TabIndex = 30;
            this.lblCurrentPrice.Text = "Current Price :";
            // 
            // txtService
            // 
            this.txtService.Location = new System.Drawing.Point(151, 41);
            this.txtService.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtService.Name = "txtService";
            this.txtService.Size = new System.Drawing.Size(388, 23);
            this.txtService.TabIndex = 2;
            this.txtService.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtService_KeyUp);
            // 
            // txtCurrentPrice
            // 
            this.txtCurrentPrice.Location = new System.Drawing.Point(151, 107);
            this.txtCurrentPrice.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCurrentPrice.Name = "txtCurrentPrice";
            this.txtCurrentPrice.Size = new System.Drawing.Size(154, 23);
            this.txtCurrentPrice.TabIndex = 4;
            this.txtCurrentPrice.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCurrentPrice_KeyDown);
            this.txtCurrentPrice.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCurrentPrice_KeyUp);
            // 
            // btnAdd
            // 
            this.btnAdd.Font = new System.Drawing.Font("Tahoma", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnAdd.Location = new System.Drawing.Point(17, 181);
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
            this.btnCancel.Location = new System.Drawing.Point(405, 181);
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
            this.panelAll.Size = new System.Drawing.Size(556, 244);
            this.panelAll.TabIndex = 40;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.chkSalesTax);
            this.panelTop.Controls.Add(this.txtType);
            this.panelTop.Controls.Add(this.label7);
            this.panelTop.Controls.Add(this.txtCustomerPrice);
            this.panelTop.Controls.Add(this.txtCurrentPrice);
            this.panelTop.Controls.Add(this.label5);
            this.panelTop.Controls.Add(this.btnCancel);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.btnAdd);
            this.panelTop.Controls.Add(this.lblCurrentPrice);
            this.panelTop.Controls.Add(this.txtService);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(556, 244);
            this.panelTop.TabIndex = 41;
            // 
            // chkSalesTax
            // 
            this.chkSalesTax.AutoSize = true;
            this.chkSalesTax.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkSalesTax.Location = new System.Drawing.Point(45, 137);
            this.chkSalesTax.Name = "chkSalesTax";
            this.chkSalesTax.Size = new System.Drawing.Size(124, 21);
            this.chkSalesTax.TabIndex = 5;
            this.chkSalesTax.Text = "Add Sales Tax :";
            this.chkSalesTax.UseVisualStyleBackColor = true;
            this.chkSalesTax.CheckedChanged += new System.EventHandler(this.chkSalesTax_CheckedChanged);
            // 
            // txtType
            // 
            this.txtType.Enabled = false;
            this.txtType.Location = new System.Drawing.Point(151, 9);
            this.txtType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtType.Name = "txtType";
            this.txtType.Size = new System.Drawing.Size(116, 23);
            this.txtType.TabIndex = 1;
            this.txtType.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtType_KeyUp);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(40, 79);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 17);
            this.label7.TabIndex = 42;
            this.label7.Text = "Customer Price:";
            // 
            // txtCustomerPrice
            // 
            this.txtCustomerPrice.Location = new System.Drawing.Point(152, 76);
            this.txtCustomerPrice.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCustomerPrice.Name = "txtCustomerPrice";
            this.txtCustomerPrice.Size = new System.Drawing.Size(105, 23);
            this.txtCustomerPrice.TabIndex = 3;
            this.txtCustomerPrice.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCustomerPrice_KeyDown);
            this.txtCustomerPrice.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCustomerPrice_KeyUp);
            // 
            // ManuallyEditService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 244);
            this.Controls.Add(this.panelAll);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ManuallyEditService";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manually Edit Service";
            this.Load += new System.EventHandler(this.ManuallyEditService_Load);
            this.panelAll.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCurrentPrice;
        private System.Windows.Forms.TextBox txtService;
        private System.Windows.Forms.TextBox txtCurrentPrice;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtCustomerPrice;
        private System.Windows.Forms.TextBox txtType;
        private System.Windows.Forms.CheckBox chkSalesTax;
    }
}