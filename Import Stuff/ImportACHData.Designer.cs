namespace SMFS
{
    partial class ImportACHData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportACHData));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn12 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn13 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn10 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn11 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn14 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn15 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnPullACH = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1315, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printPreviewToolStripMenuItem,
            this.printToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(99, 22);
            this.toolStripMenuItem1.Text = "Print";
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
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 24);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(1315, 288);
            this.panelAll.TabIndex = 6;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.tabControl1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 38);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1315, 250);
            this.panelBottom.TabIndex = 8;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1315, 250);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgv);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1307, 224);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ACH Import Data";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(3, 3);
            this.dgv.MainView = this.gridMain;
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(1301, 218);
            this.dgv.TabIndex = 4;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain});
            // 
            // gridMain
            // 
            this.gridMain.Appearance.BandPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.BandPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.BandPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.BandPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseFont = true;
            this.gridMain.Appearance.BandPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.BandPanelBackground.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.BandPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.ColumnFilterButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.ColumnFilterButton.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseForeColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(189)))), ((int)(((byte)(125)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(189)))), ((int)(((byte)(125)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseForeColor = true;
            this.gridMain.Appearance.Empty.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(247)))), ((int)(((byte)(219)))));
            this.gridMain.Appearance.Empty.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.Empty.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(247)))), ((int)(((byte)(219)))));
            this.gridMain.Appearance.EvenRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterCloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.FilterCloseButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.FilterCloseButton.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(166)))), ((int)(((byte)(93)))));
            this.gridMain.Appearance.FilterPanel.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.FilterPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FilterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.FixedLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(120)))), ((int)(((byte)(88)))));
            this.gridMain.Appearance.FixedLine.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedCell.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FocusedCell.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FocusedCell.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedCell.Options.UseForeColor = true;
            this.gridMain.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(136)))), ((int)(((byte)(91)))));
            this.gridMain.Appearance.FocusedRow.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FooterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.FooterPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.FooterPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FooterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.GroupButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(143)))), ((int)(((byte)(62)))));
            this.gridMain.Appearance.GroupButton.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(189)))), ((int)(((byte)(125)))));
            this.gridMain.Appearance.GroupFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(189)))), ((int)(((byte)(125)))));
            this.gridMain.Appearance.GroupFooter.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupFooter.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(166)))), ((int)(((byte)(93)))));
            this.gridMain.Appearance.GroupPanel.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.GroupPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.GroupPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(189)))), ((int)(((byte)(125)))));
            this.gridMain.Appearance.GroupRow.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(189)))), ((int)(((byte)(125)))));
            this.gridMain.Appearance.GroupRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupRow.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupRow.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(166)))), ((int)(((byte)(93)))));
            this.gridMain.Appearance.HeaderPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(166)))), ((int)(((byte)(93)))));
            this.gridMain.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.HeaderPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Appearance.HeaderPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(166)))), ((int)(((byte)(93)))));
            this.gridMain.Appearance.HeaderPanelBackground.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.HeaderPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(172)))), ((int)(((byte)(134)))));
            this.gridMain.Appearance.HideSelectionRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(234)))), ((int)(((byte)(216)))));
            this.gridMain.Appearance.HideSelectionRow.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HorzLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(169)))), ((int)(((byte)(107)))));
            this.gridMain.Appearance.HorzLine.Options.UseBackColor = true;
            this.gridMain.Appearance.OddRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(224)))), ((int)(((byte)(190)))));
            this.gridMain.Appearance.OddRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            this.gridMain.Appearance.OddRow.Options.UseForeColor = true;
            this.gridMain.Appearance.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(252)))), ((int)(((byte)(237)))));
            this.gridMain.Appearance.Preview.Font = new System.Drawing.Font("Verdana", 7.5F);
            this.gridMain.Appearance.Preview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(120)))), ((int)(((byte)(88)))));
            this.gridMain.Appearance.Preview.Options.UseBackColor = true;
            this.gridMain.Appearance.Preview.Options.UseFont = true;
            this.gridMain.Appearance.Preview.Options.UseForeColor = true;
            this.gridMain.Appearance.Row.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(247)))), ((int)(((byte)(219)))));
            this.gridMain.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.Row.Options.UseBackColor = true;
            this.gridMain.Appearance.Row.Options.UseForeColor = true;
            this.gridMain.Appearance.RowSeparator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(247)))), ((int)(((byte)(219)))));
            this.gridMain.Appearance.RowSeparator.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.RowSeparator.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(159)))), ((int)(((byte)(114)))));
            this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.SelectedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.TopNewRow.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.TopNewRow.Options.UseBackColor = true;
            this.gridMain.Appearance.VertLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(169)))), ((int)(((byte)(107)))));
            this.gridMain.Appearance.VertLine.Options.UseBackColor = true;
            this.gridMain.AppearancePrint.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain.AppearancePrint.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand5});
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn1,
            this.bandedGridColumn6,
            this.bandedGridColumn12,
            this.bandedGridColumn8,
            this.bandedGridColumn3,
            this.bandedGridColumn4,
            this.bandedGridColumn5,
            this.bandedGridColumn13,
            this.bandedGridColumn10,
            this.bandedGridColumn11,
            this.bandedGridColumn2,
            this.bandedGridColumn7,
            this.bandedGridColumn9,
            this.bandedGridColumn14,
            this.bandedGridColumn15});
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridMain_CustomDrawCell);
            this.gridMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridMain_CellValueChanged);
            this.gridMain.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridMain_CustomColumnDisplayText);
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
            // 
            // gridBand5
            // 
            this.gridBand5.Columns.Add(this.bandedGridColumn1);
            this.gridBand5.Columns.Add(this.bandedGridColumn6);
            this.gridBand5.Columns.Add(this.bandedGridColumn8);
            this.gridBand5.Columns.Add(this.bandedGridColumn12);
            this.gridBand5.Columns.Add(this.bandedGridColumn4);
            this.gridBand5.Columns.Add(this.bandedGridColumn5);
            this.gridBand5.Columns.Add(this.bandedGridColumn13);
            this.gridBand5.Columns.Add(this.bandedGridColumn3);
            this.gridBand5.Columns.Add(this.bandedGridColumn10);
            this.gridBand5.Columns.Add(this.bandedGridColumn11);
            this.gridBand5.Columns.Add(this.bandedGridColumn2);
            this.gridBand5.Columns.Add(this.bandedGridColumn7);
            this.gridBand5.Columns.Add(this.bandedGridColumn9);
            this.gridBand5.Columns.Add(this.bandedGridColumn14);
            this.gridBand5.Columns.Add(this.bandedGridColumn15);
            this.gridBand5.Name = "gridBand5";
            this.gridBand5.VisibleIndex = 0;
            this.gridBand5.Width = 1331;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn1.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn1.Caption = "Num";
            this.bandedGridColumn1.FieldName = "Num";
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Visible = true;
            this.bandedGridColumn1.Width = 50;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn6.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn6.Caption = "Status";
            this.bandedGridColumn6.FieldName = "status";
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Visible = true;
            // 
            // bandedGridColumn8
            // 
            this.bandedGridColumn8.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn8.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn8.Caption = "Customer Number";
            this.bandedGridColumn8.FieldName = "Customer Number";
            this.bandedGridColumn8.Name = "bandedGridColumn8";
            this.bandedGridColumn8.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn8.Visible = true;
            this.bandedGridColumn8.Width = 107;
            // 
            // bandedGridColumn12
            // 
            this.bandedGridColumn12.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn12.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn12.Caption = "Name";
            this.bandedGridColumn12.FieldName = "Name";
            this.bandedGridColumn12.Name = "bandedGridColumn12";
            this.bandedGridColumn12.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn12.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn12.Visible = true;
            this.bandedGridColumn12.Width = 119;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn4.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn4.Caption = "Routing";
            this.bandedGridColumn4.FieldName = "Routing";
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            this.bandedGridColumn4.Width = 130;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn5.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn5.Caption = "Account Number";
            this.bandedGridColumn5.FieldName = "Account Number";
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 125;
            // 
            // bandedGridColumn13
            // 
            this.bandedGridColumn13.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn13.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn13.Caption = "Type";
            this.bandedGridColumn13.FieldName = "Type";
            this.bandedGridColumn13.Name = "bandedGridColumn13";
            this.bandedGridColumn13.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn13.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn13.Visible = true;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn3.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn3.Caption = "Amount";
            this.bandedGridColumn3.FieldName = "Amount";
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 100;
            // 
            // bandedGridColumn10
            // 
            this.bandedGridColumn10.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn10.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn10.Caption = "Day of Month";
            this.bandedGridColumn10.FieldName = "COL 12";
            this.bandedGridColumn10.Name = "bandedGridColumn10";
            this.bandedGridColumn10.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn10.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn10.Visible = true;
            // 
            // bandedGridColumn11
            // 
            this.bandedGridColumn11.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn11.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn11.Caption = "Frequency in Months";
            this.bandedGridColumn11.DisplayFormat.FormatString = "N0";
            this.bandedGridColumn11.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn11.FieldName = "frequency";
            this.bandedGridColumn11.Name = "bandedGridColumn11";
            this.bandedGridColumn11.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn11.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn11.Visible = true;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn2.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn2.AppearanceHeader.Options.UseTextOptions = true;
            this.bandedGridColumn2.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn2.Caption = "Frequency Text";
            this.bandedGridColumn2.FieldName = "COL 11";
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 100;
            // 
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn7.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn7.Caption = "Start Date";
            this.bandedGridColumn7.DisplayFormat.FormatString = "MM/dd/yyyy";
            this.bandedGridColumn7.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn7.FieldName = "startDate";
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Visible = true;
            // 
            // bandedGridColumn9
            // 
            this.bandedGridColumn9.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn9.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn9.Caption = "Number Payments";
            this.bandedGridColumn9.DisplayFormat.FormatString = "N0";
            this.bandedGridColumn9.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn9.FieldName = "numPayments";
            this.bandedGridColumn9.Name = "bandedGridColumn9";
            this.bandedGridColumn9.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn9.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn9.Visible = true;
            // 
            // bandedGridColumn14
            // 
            this.bandedGridColumn14.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn14.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn14.Caption = "Payments Left";
            this.bandedGridColumn14.FieldName = "leftPayments";
            this.bandedGridColumn14.Name = "bandedGridColumn14";
            this.bandedGridColumn14.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn14.Visible = true;
            // 
            // bandedGridColumn15
            // 
            this.bandedGridColumn15.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn15.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn15.Caption = "Calculated";
            this.bandedGridColumn15.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn15.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn15.FieldName = "difference";
            this.bandedGridColumn15.Name = "bandedGridColumn15";
            this.bandedGridColumn15.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn15.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn15.Visible = true;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnPullACH);
            this.panelTop.Controls.Add(this.btnRun);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1315, 38);
            this.panelTop.TabIndex = 7;
            // 
            // btnPullACH
            // 
            this.btnPullACH.Location = new System.Drawing.Point(60, 8);
            this.btnPullACH.Name = "btnPullACH";
            this.btnPullACH.Size = new System.Drawing.Size(111, 23);
            this.btnPullACH.TabIndex = 150;
            this.btnPullACH.Text = "Pull ACH CSV File";
            this.btnPullACH.UseVisualStyleBackColor = true;
            this.btnPullACH.Click += new System.EventHandler(this.btnPullACH_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(344, 8);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(111, 23);
            this.btnRun.TabIndex = 149;
            this.btnRun.Text = "Import ACH Data";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(28, 22);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 146;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // ImportACHData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 312);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Name = "ImportACHData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import ACH Bank Information";
            this.Load += new System.EventHandler(this.ImportACHData_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            this.panelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn8;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn10;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn11;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn12;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn13;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private System.Windows.Forms.Button btnPullACH;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn9;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn14;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn15;
    }
}