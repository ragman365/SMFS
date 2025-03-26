namespace SMFS
{
    partial class AutoDrafts
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
            DevExpress.Utils.ContextButton contextButton2 = new DevExpress.Utils.ContextButton();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoDrafts));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.Num = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.contractNumber = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.serviceTotal = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repButton = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.repositoryItemPictureEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnProcess = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.excludeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.includeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkExcludeLapsed = new System.Windows.Forms.CheckBox();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 24);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(630, 237);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 51);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(630, 186);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.ContextMenuStrip = this.contextMenuStrip1;
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "Caramel";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repButton,
            this.repCheckEdit1,
            this.repositoryItemPictureEdit1});
            this.dgv.Size = new System.Drawing.Size(630, 186);
            this.dgv.TabIndex = 3;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain});
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
            this.gridMain.ColumnPanelRowHeight = 5;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.Num,
            this.bandedGridColumn1,
            this.contractNumber,
            this.bandedGridColumn9,
            this.serviceTotal,
            this.bandedGridColumn2,
            this.bandedGridColumn3,
            this.bandedGridColumn4});
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            this.gridMain.OptionsBehavior.ReadOnly = true;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.RowHeight = 1;
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick_2);
            // 
            // Num
            // 
            this.Num.AppearanceHeader.Options.UseTextOptions = true;
            this.Num.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
            this.Num.Caption = "Num";
            this.Num.FieldName = "num";
            this.Num.Name = "Num";
            this.Num.OptionsColumn.AllowEdit = false;
            this.Num.OptionsColumn.FixedWidth = true;
            this.Num.Visible = true;
            this.Num.Width = 50;
            // 
            // contractNumber
            // 
            this.contractNumber.Caption = "Contract";
            this.contractNumber.FieldName = "contractNumber";
            this.contractNumber.Name = "contractNumber";
            this.contractNumber.OptionsColumn.AllowEdit = false;
            this.contractNumber.OptionsColumn.FixedWidth = true;
            this.contractNumber.Visible = true;
            this.contractNumber.Width = 80;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.Caption = "Customer";
            this.bandedGridColumn2.FieldName = "customer";
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 150;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "Lapsed";
            this.bandedGridColumn3.FieldName = "lapsed";
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            // 
            // bandedGridColumn9
            // 
            this.bandedGridColumn9.Caption = "Draft DOM";
            this.bandedGridColumn9.DisplayFormat.FormatString = "N0";
            this.bandedGridColumn9.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn9.FieldName = "autodraftDay";
            this.bandedGridColumn9.Name = "bandedGridColumn9";
            this.bandedGridColumn9.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn9.Visible = true;
            // 
            // serviceTotal
            // 
            this.serviceTotal.Caption = "Draft Amount";
            this.serviceTotal.DisplayFormat.FormatString = "N2";
            this.serviceTotal.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.serviceTotal.FieldName = "autodraftAmount";
            this.serviceTotal.MinWidth = 10;
            this.serviceTotal.Name = "serviceTotal";
            this.serviceTotal.OptionsColumn.AllowEdit = false;
            this.serviceTotal.OptionsColumn.FixedWidth = true;
            this.serviceTotal.Visible = true;
            this.serviceTotal.Width = 100;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "record";
            this.bandedGridColumn1.FieldName = "record";
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            // 
            // repButton
            // 
            this.repButton.AutoHeight = false;
            this.repButton.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.repButton.Name = "repButton";
            // 
            // repCheckEdit1
            // 
            this.repCheckEdit1.AutoHeight = false;
            this.repCheckEdit1.Name = "repCheckEdit1";
            // 
            // repositoryItemPictureEdit1
            // 
            contextButton2.Caption = "contextButton1";
            contextButton2.Id = new System.Guid("7020ad83-ad48-4296-8d4d-5a08a0a3f273");
            contextButton2.Name = "contextButton1";
            this.repositoryItemPictureEdit1.ContextButtons.Add(contextButton2);
            this.repositoryItemPictureEdit1.Name = "repositoryItemPictureEdit1";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelTop.Controls.Add(this.chkExcludeLapsed);
            this.panelTop.Controls.Add(this.btnProcess);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(630, 51);
            this.panelTop.TabIndex = 1;
            // 
            // btnProcess
            // 
            this.btnProcess.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnProcess.Location = new System.Drawing.Point(56, 6);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(75, 39);
            this.btnProcess.TabIndex = 5;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = false;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(31, 33);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(630, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPrint,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuPrint
            // 
            this.menuPrint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printPreviewToolStripMenuItem,
            this.printToolStripMenuItem});
            this.menuPrint.Name = "menuPrint";
            this.menuPrint.Size = new System.Drawing.Size(99, 22);
            this.menuPrint.Text = "Print";
            // 
            // printPreviewToolStripMenuItem
            // 
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.printPreviewToolStripMenuItem.Text = "Print Preview";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.printPreviewToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "Exclude";
            this.bandedGridColumn4.FieldName = "exclude";
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Columns.Add(this.Num);
            this.gridBand1.Columns.Add(this.bandedGridColumn4);
            this.gridBand1.Columns.Add(this.contractNumber);
            this.gridBand1.Columns.Add(this.bandedGridColumn2);
            this.gridBand1.Columns.Add(this.bandedGridColumn3);
            this.gridBand1.Columns.Add(this.bandedGridColumn9);
            this.gridBand1.Columns.Add(this.serviceTotal);
            this.gridBand1.Columns.Add(this.bandedGridColumn1);
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 605;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.excludeToolStripMenuItem,
            this.includeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(115, 48);
            // 
            // excludeToolStripMenuItem
            // 
            this.excludeToolStripMenuItem.Name = "excludeToolStripMenuItem";
            this.excludeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.excludeToolStripMenuItem.Text = "Exclude";
            this.excludeToolStripMenuItem.Click += new System.EventHandler(this.excludeToolStripMenuItem_Click);
            // 
            // includeToolStripMenuItem
            // 
            this.includeToolStripMenuItem.Name = "includeToolStripMenuItem";
            this.includeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.includeToolStripMenuItem.Text = "Include";
            this.includeToolStripMenuItem.Click += new System.EventHandler(this.includeToolStripMenuItem_Click);
            // 
            // chkExcludeLapsed
            // 
            this.chkExcludeLapsed.AutoSize = true;
            this.chkExcludeLapsed.Checked = true;
            this.chkExcludeLapsed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExcludeLapsed.Location = new System.Drawing.Point(166, 18);
            this.chkExcludeLapsed.Name = "chkExcludeLapsed";
            this.chkExcludeLapsed.Size = new System.Drawing.Size(191, 17);
            this.chkExcludeLapsed.TabIndex = 6;
            this.chkExcludeLapsed.Text = "Exclude All Lapsed unless Included";
            this.chkExcludeLapsed.UseVisualStyleBackColor = true;
            // 
            // AutoDrafts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 261);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.LookAndFeel.SkinName = "Coffee";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "AutoDrafts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoDrafts";
            this.Load += new System.EventHandler(this.AutoDrafts_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private DevExpress.XtraGrid.GridControl dgv;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repButton;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repCheckEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryItemPictureEdit1;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn Num;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn contractNumber;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn serviceTotal;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn9;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem excludeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem includeToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkExcludeLapsed;
    }
}