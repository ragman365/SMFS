namespace SMFS
{
    partial class AddEditUsers
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddEditUsers));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toggleActiveStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.Num = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.bandedGridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn10 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemCheckedComboBoxEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
            this.bandedGridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnPreferences = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(1312, 321);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 38);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1312, 283);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.ContextMenuStrip = this.contextMenuStrip1;
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "Glass Oceans";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBox1,
            this.repositoryItemCheckedComboBoxEdit1});
            this.dgv.Size = new System.Drawing.Size(1312, 283);
            this.dgv.TabIndex = 4;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain});
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleActiveStatusToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(214, 28);
            // 
            // toggleActiveStatusToolStripMenuItem
            // 
            this.toggleActiveStatusToolStripMenuItem.Name = "toggleActiveStatusToolStripMenuItem";
            this.toggleActiveStatusToolStripMenuItem.Size = new System.Drawing.Size(213, 24);
            this.toggleActiveStatusToolStripMenuItem.Text = "Toggle Active Status";
            this.toggleActiveStatusToolStripMenuItem.Click += new System.EventHandler(this.toggleActiveStatusToolStripMenuItem_Click);
            // 
            // gridMain
            // 
            this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.Wheat;
            this.gridMain.Appearance.FocusedRow.BackColor = System.Drawing.Color.Yellow;
            this.gridMain.Appearance.FocusedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Appearance.OddRow.BackColor = System.Drawing.Color.Orange;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand1});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.ColumnPanelRowHeight = 6;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.Num,
            this.bandedGridColumn1,
            this.bandedGridColumn6,
            this.bandedGridColumn5,
            this.bandedGridColumn4,
            this.bandedGridColumn2,
            this.bandedGridColumn7,
            this.bandedGridColumn3,
            this.bandedGridColumn8,
            this.bandedGridColumn9,
            this.bandedGridColumn10});
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            this.gridMain.OptionsView.ColumnAutoWidth = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Skin";
            this.gridMain.RowHeight = 1;
            this.gridMain.ShownEditor += new System.EventHandler(this.gridMain_ShownEditor);
            this.gridMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridMain_CellValueChanged);
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
            this.gridMain.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridMain_ValidatingEditor);
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Columns.Add(this.Num);
            this.gridBand1.Columns.Add(this.bandedGridColumn6);
            this.gridBand1.Columns.Add(this.bandedGridColumn5);
            this.gridBand1.Columns.Add(this.bandedGridColumn4);
            this.gridBand1.Columns.Add(this.bandedGridColumn2);
            this.gridBand1.Columns.Add(this.bandedGridColumn7);
            this.gridBand1.Columns.Add(this.bandedGridColumn3);
            this.gridBand1.Columns.Add(this.bandedGridColumn9);
            this.gridBand1.Columns.Add(this.bandedGridColumn10);
            this.gridBand1.Columns.Add(this.bandedGridColumn8);
            this.gridBand1.Columns.Add(this.bandedGridColumn1);
            this.gridBand1.MinWidth = 14;
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 1246;
            // 
            // Num
            // 
            this.Num.AppearanceHeader.Options.UseTextOptions = true;
            this.Num.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
            this.Num.Caption = "Num";
            this.Num.FieldName = "num";
            this.Num.MinWidth = 27;
            this.Num.Name = "Num";
            this.Num.OptionsColumn.AllowEdit = false;
            this.Num.OptionsColumn.FixedWidth = true;
            this.Num.Visible = true;
            this.Num.Width = 68;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.Caption = "User";
            this.bandedGridColumn6.FieldName = "userName";
            this.bandedGridColumn6.MinWidth = 27;
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Visible = true;
            this.bandedGridColumn6.Width = 101;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.Caption = "First Name";
            this.bandedGridColumn5.FieldName = "firstName";
            this.bandedGridColumn5.MinWidth = 27;
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 101;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "Last Name";
            this.bandedGridColumn4.FieldName = "lastName";
            this.bandedGridColumn4.MinWidth = 27;
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            this.bandedGridColumn4.Width = 101;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.AppearanceHeader.Options.UseTextOptions = true;
            this.bandedGridColumn2.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn2.Caption = "Password";
            this.bandedGridColumn2.FieldName = "Password";
            this.bandedGridColumn2.MinWidth = 14;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 101;
            // 
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.Caption = "Status";
            this.bandedGridColumn7.FieldName = "status";
            this.bandedGridColumn7.MinWidth = 27;
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Visible = true;
            this.bandedGridColumn7.Width = 101;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.AppearanceHeader.Options.UseTextOptions = true;
            this.bandedGridColumn3.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn3.Caption = "Permissions";
            this.bandedGridColumn3.ColumnEdit = this.repositoryItemComboBox1;
            this.bandedGridColumn3.FieldName = "permissions";
            this.bandedGridColumn3.MinWidth = 14;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 122;
            // 
            // repositoryItemComboBox1
            // 
            this.repositoryItemComboBox1.AutoHeight = false;
            this.repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox1.Name = "repositoryItemComboBox1";
            this.repositoryItemComboBox1.SelectedIndexChanged += new System.EventHandler(this.repositoryItemComboBox1_SelectedIndexChanged);
            this.repositoryItemComboBox1.Validating += new System.ComponentModel.CancelEventHandler(this.repositoryItemComboBox1_Validating);
            // 
            // bandedGridColumn9
            // 
            this.bandedGridColumn9.Caption = "Agent Code";
            this.bandedGridColumn9.FieldName = "agentCode";
            this.bandedGridColumn9.MinWidth = 27;
            this.bandedGridColumn9.Name = "bandedGridColumn9";
            this.bandedGridColumn9.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn9.Visible = true;
            this.bandedGridColumn9.Width = 101;
            // 
            // bandedGridColumn10
            // 
            this.bandedGridColumn10.Caption = "Assigned Locations";
            this.bandedGridColumn10.ColumnEdit = this.repositoryItemCheckedComboBoxEdit1;
            this.bandedGridColumn10.FieldName = "assignedLocations";
            this.bandedGridColumn10.MinWidth = 29;
            this.bandedGridColumn10.Name = "bandedGridColumn10";
            this.bandedGridColumn10.Visible = true;
            this.bandedGridColumn10.Width = 178;
            // 
            // repositoryItemCheckedComboBoxEdit1
            // 
            this.repositoryItemCheckedComboBoxEdit1.AutoHeight = false;
            this.repositoryItemCheckedComboBoxEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemCheckedComboBoxEdit1.Name = "repositoryItemCheckedComboBoxEdit1";
            this.repositoryItemCheckedComboBoxEdit1.SeparatorChar = '~';
            // 
            // bandedGridColumn8
            // 
            this.bandedGridColumn8.Caption = "Email";
            this.bandedGridColumn8.FieldName = "email";
            this.bandedGridColumn8.MinWidth = 27;
            this.bandedGridColumn8.Name = "bandedGridColumn8";
            this.bandedGridColumn8.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn8.Visible = true;
            this.bandedGridColumn8.Width = 272;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "record";
            this.bandedGridColumn1.FieldName = "record";
            this.bandedGridColumn1.MinWidth = 27;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.Width = 101;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.pictureBox2);
            this.panelTop.Controls.Add(this.btnDelete);
            this.panelTop.Controls.Add(this.btnPreferences);
            this.panelTop.Controls.Add(this.btnAdd);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1312, 38);
            this.panelTop.TabIndex = 1;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(8, 6);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(33, 27);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 6;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(126, 6);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(26, 28);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Text = "-";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnPreferences
            // 
            this.btnPreferences.Location = new System.Drawing.Point(512, 5);
            this.btnPreferences.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPreferences.Name = "btnPreferences";
            this.btnPreferences.Size = new System.Drawing.Size(108, 28);
            this.btnPreferences.TabIndex = 4;
            this.btnPreferences.Text = "Preferences";
            this.btnPreferences.UseVisualStyleBackColor = true;
            this.btnPreferences.Click += new System.EventHandler(this.btnPreferences_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(61, 6);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(26, 28);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "+";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // AddEditUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1312, 321);
            this.Controls.Add(this.panelAll);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "AddEditUsers";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add/Edit Users";
            this.Load += new System.EventHandler(this.AddEditUsers_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).EndInit();
            this.panelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn Num;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toggleActiveStatusToolStripMenuItem;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnPreferences;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn8;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn9;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn10;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit repositoryItemCheckedComboBoxEdit1;
    }
}