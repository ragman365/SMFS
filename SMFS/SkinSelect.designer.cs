namespace SMFS
{
    partial class SkinSelect
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
            this.dgv2 = new DevExpress.XtraGrid.GridControl();
            this.skinView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.panelSkinTop = new System.Windows.Forms.Panel();
            this.lblNoColor = new System.Windows.Forms.Label();
            this.chkNoColor = new System.Windows.Forms.CheckBox();
            this.radioColor = new System.Windows.Forms.RadioButton();
            this.radioSkin = new System.Windows.Forms.RadioButton();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.skinView)).BeginInit();
            this.panelSkinTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelSkinTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(198, 332);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv2);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 54);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(198, 278);
            this.panelBottom.TabIndex = 6;
            // 
            // dgv2
            // 
            this.dgv2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv2.Location = new System.Drawing.Point(0, 0);
            this.dgv2.MainView = this.skinView;
            this.dgv2.Name = "dgv2";
            this.dgv2.Size = new System.Drawing.Size(198, 278);
            this.dgv2.TabIndex = 4;
            this.dgv2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.skinView});
            // 
            // skinView
            // 
            this.skinView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.gridColumn2,
            this.gridColumn3});
            this.skinView.CustomizationFormBounds = new System.Drawing.Rectangle(776, 537, 216, 178);
            this.skinView.GridControl = this.dgv2;
            this.skinView.Name = "skinView";
            this.skinView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            this.skinView.OptionsBehavior.AutoPopulateColumns = false;
            this.skinView.OptionsBehavior.Editable = false;
            this.skinView.OptionsBehavior.ReadOnly = true;
            this.skinView.OptionsFind.AllowFindPanel = false;
            this.skinView.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CellSelect;
            this.skinView.OptionsView.ShowGroupPanel = false;
            this.skinView.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.skinView_CustomDrawCell);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "Skin";
            this.gridColumn1.FieldName = "skin";
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 0;
            this.gridColumn1.Width = 60;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "Even";
            this.gridColumn2.FieldName = "even";
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 1;
            this.gridColumn2.Width = 33;
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "Color";
            this.gridColumn3.FieldName = "color";
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Visible = true;
            this.gridColumn3.VisibleIndex = 2;
            this.gridColumn3.Width = 89;
            // 
            // panelSkinTop
            // 
            this.panelSkinTop.BackColor = System.Drawing.Color.Transparent;
            this.panelSkinTop.Controls.Add(this.lblNoColor);
            this.panelSkinTop.Controls.Add(this.chkNoColor);
            this.panelSkinTop.Controls.Add(this.radioColor);
            this.panelSkinTop.Controls.Add(this.radioSkin);
            this.panelSkinTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSkinTop.Location = new System.Drawing.Point(0, 0);
            this.panelSkinTop.Name = "panelSkinTop";
            this.panelSkinTop.Size = new System.Drawing.Size(198, 54);
            this.panelSkinTop.TabIndex = 5;
            // 
            // lblNoColor
            // 
            this.lblNoColor.AutoSize = true;
            this.lblNoColor.Location = new System.Drawing.Point(14, 33);
            this.lblNoColor.Name = "lblNoColor";
            this.lblNoColor.Size = new System.Drawing.Size(48, 13);
            this.lblNoColor.TabIndex = 63;
            this.lblNoColor.Text = "No Color";
            // 
            // chkNoColor
            // 
            this.chkNoColor.AutoSize = true;
            this.chkNoColor.Location = new System.Drawing.Point(64, 33);
            this.chkNoColor.Name = "chkNoColor";
            this.chkNoColor.Size = new System.Drawing.Size(15, 14);
            this.chkNoColor.TabIndex = 62;
            this.chkNoColor.UseVisualStyleBackColor = true;
            this.chkNoColor.CheckedChanged += new System.EventHandler(this.chkNoColor_CheckedChanged);
            // 
            // radioColor
            // 
            this.radioColor.AutoSize = true;
            this.radioColor.Location = new System.Drawing.Point(85, 30);
            this.radioColor.Name = "radioColor";
            this.radioColor.Size = new System.Drawing.Size(82, 17);
            this.radioColor.TabIndex = 61;
            this.radioColor.TabStop = true;
            this.radioColor.Text = "Select Color";
            this.radioColor.UseVisualStyleBackColor = true;
            this.radioColor.CheckedChanged += new System.EventHandler(this.radioColor_CheckedChanged);
            // 
            // radioSkin
            // 
            this.radioSkin.AutoSize = true;
            this.radioSkin.Location = new System.Drawing.Point(85, 7);
            this.radioSkin.Name = "radioSkin";
            this.radioSkin.Size = new System.Drawing.Size(76, 17);
            this.radioSkin.TabIndex = 60;
            this.radioSkin.TabStop = true;
            this.radioSkin.Text = "Select Skin";
            this.radioSkin.UseVisualStyleBackColor = true;
            this.radioSkin.CheckedChanged += new System.EventHandler(this.radioSkin_CheckedChanged);
            // 
            // SkinSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 332);
            this.Controls.Add(this.panelAll);
            this.Name = "SkinSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SkinSelect";
            this.Load += new System.EventHandler(this.SkinSelect_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.skinView)).EndInit();
            this.panelSkinTop.ResumeLayout(false);
            this.panelSkinTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelSkinTop;
        private System.Windows.Forms.Label lblNoColor;
        private System.Windows.Forms.CheckBox chkNoColor;
        private System.Windows.Forms.RadioButton radioColor;
        private System.Windows.Forms.RadioButton radioSkin;
        private DevExpress.XtraGrid.GridControl dgv2;
        private DevExpress.XtraGrid.Views.Grid.GridView skinView;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
    }
}