namespace SMFS
{
    partial class AddEditReferenceTable
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmbReference = new System.Windows.Forms.ComboBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.chkProtect = new System.Windows.Forms.CheckBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select Reference Table :";
            // 
            // cmbReference
            // 
            this.cmbReference.FormattingEnabled = true;
            this.cmbReference.Location = new System.Drawing.Point(208, 35);
            this.cmbReference.Name = "cmbReference";
            this.cmbReference.Size = new System.Drawing.Size(226, 24);
            this.cmbReference.TabIndex = 2;
            this.cmbReference.SelectedIndexChanged += new System.EventHandler(this.cmbReference_SelectedIndexChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(12, 119);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 43);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Update Reference";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // chkProtect
            // 
            this.chkProtect.AutoSize = true;
            this.chkProtect.Location = new System.Drawing.Point(50, 83);
            this.chkProtect.Name = "chkProtect";
            this.chkProtect.Size = new System.Drawing.Size(217, 21);
            this.chkProtect.TabIndex = 4;
            this.chkProtect.Tag = "If checked, the user can only selection from the Dropdown";
            this.chkProtect.Text = "Protect Field from being Edited";
            this.chkProtect.UseVisualStyleBackColor = true;
            this.chkProtect.CheckedChanged += new System.EventHandler(this.chkProtect_CheckedChanged);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(334, 119);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(100, 43);
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "Remove Reference";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(178, 119);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 43);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // AddEditReferenceTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 182);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.chkProtect);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.cmbReference);
            this.Controls.Add(this.label1);
            this.Name = "AddEditReferenceTable";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add/Edit Reference Table";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddEditReferenceTable_FormClosing);
            this.Load += new System.EventHandler(this.AddEditReferenceTable_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbReference;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.CheckBox chkProtect;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnCancel;
    }
}