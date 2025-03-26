namespace SMFS
{
    partial class ServiceReport
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
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn15 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemCheckEdit3 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.bandedGridColumn17 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.price22 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.data3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.bandedGridColumn21 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 24);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(857, 330);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 70);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(857, 260);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "iMaginary";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckEdit3,
            this.repositoryItemComboBox1,
            this.repositoryItemCheckEdit1});
            this.dgv.Size = new System.Drawing.Size(857, 260);
            this.dgv.TabIndex = 7;
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
            this.gridBand3});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn15,
            this.bandedGridColumn21,
            this.bandedGridColumn17,
            this.price22,
            this.bandedGridColumn1,
            this.bandedGridColumn2,
            this.data3,
            this.bandedGridColumn4,
            this.bandedGridColumn5,
            this.bandedGridColumn3});
            this.gridMain.GridControl = this.dgv;
            this.gridMain.GroupCount = 1;
            this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "price", this.price22, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "data", this.data3, "${0:0,0.00}")});
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.AutoPopulateColumns = false;
            this.gridMain.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            this.gridMain.OptionsBehavior.ReadOnly = true;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Flat";
            this.gridMain.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.bandedGridColumn3, DevExpress.Data.ColumnSortOrder.Ascending)});
            // 
            // gridBand3
            // 
            this.gridBand3.Caption = "gridBand1";
            this.gridBand3.Columns.Add(this.bandedGridColumn15);
            this.gridBand3.Columns.Add(this.bandedGridColumn4);
            this.gridBand3.Columns.Add(this.bandedGridColumn1);
            this.gridBand3.Columns.Add(this.bandedGridColumn17);
            this.gridBand3.Columns.Add(this.price22);
            this.gridBand3.Columns.Add(this.data3);
            this.gridBand3.Columns.Add(this.bandedGridColumn2);
            this.gridBand3.Columns.Add(this.bandedGridColumn5);
            this.gridBand3.Name = "gridBand3";
            this.gridBand3.VisibleIndex = 0;
            this.gridBand3.Width = 490;
            // 
            // bandedGridColumn15
            // 
            this.bandedGridColumn15.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn15.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn15.Caption = "Num";
            this.bandedGridColumn15.FieldName = "num";
            this.bandedGridColumn15.Name = "bandedGridColumn15";
            this.bandedGridColumn15.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn15.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn15.Width = 37;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn4.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn4.Caption = "Type";
            this.bandedGridColumn4.ColumnEdit = this.repositoryItemComboBox1;
            this.bandedGridColumn4.FieldName = "type";
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            // 
            // repositoryItemComboBox1
            // 
            this.repositoryItemComboBox1.AutoHeight = false;
            this.repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox1.Items.AddRange(new object[] {
            "Service",
            "Merchandise",
            "Cash Advance",
            "Other"});
            this.repositoryItemComboBox1.Name = "repositoryItemComboBox1";
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn1.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn1.Caption = "Select";
            this.bandedGridColumn1.ColumnEdit = this.repositoryItemCheckEdit3;
            this.bandedGridColumn1.FieldName = "select";
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            // 
            // repositoryItemCheckEdit3
            // 
            this.repositoryItemCheckEdit3.AutoHeight = false;
            this.repositoryItemCheckEdit3.Name = "repositoryItemCheckEdit3";
            // 
            // bandedGridColumn17
            // 
            this.bandedGridColumn17.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn17.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn17.Caption = "Service";
            this.bandedGridColumn17.FieldName = "service";
            this.bandedGridColumn17.Name = "bandedGridColumn17";
            this.bandedGridColumn17.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn17.Visible = true;
            this.bandedGridColumn17.Width = 400;
            // 
            // price22
            // 
            this.price22.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.price22.AppearanceHeader.Options.UseForeColor = true;
            this.price22.Caption = "Price";
            this.price22.DisplayFormat.FormatString = "N2";
            this.price22.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.price22.FieldName = "price";
            this.price22.Name = "price22";
            this.price22.OptionsColumn.FixedWidth = true;
            this.price22.Visible = true;
            this.price22.Width = 90;
            // 
            // data3
            // 
            this.data3.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.data3.AppearanceHeader.Options.UseForeColor = true;
            this.data3.Caption = "Cost";
            this.data3.DisplayFormat.FormatString = "N2";
            this.data3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.data3.FieldName = "data";
            this.data3.Name = "data3";
            this.data3.OptionsColumn.FixedWidth = true;
            this.data3.Width = 90;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn2.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn2.Caption = "Total";
            this.bandedGridColumn2.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn2.FieldName = "total";
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Width = 90;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn5.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn5.Caption = "Same as Master";
            this.bandedGridColumn5.ColumnEdit = this.repositoryItemCheckEdit1;
            this.bandedGridColumn5.FieldName = "SameAsMaster";
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            // 
            // repositoryItemCheckEdit1
            // 
            this.repositoryItemCheckEdit1.AutoHeight = false;
            this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
            // 
            // bandedGridColumn21
            // 
            this.bandedGridColumn21.Caption = "record";
            this.bandedGridColumn21.FieldName = "record";
            this.bandedGridColumn21.Name = "bandedGridColumn21";
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "Package Name";
            this.bandedGridColumn3.FieldName = "PackageName";
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            // 
            // panelTop
            // 
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(857, 70);
            this.panelTop.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(857, 24);
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
            this.toolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem1.Text = "Print";
            // 
            // printPreviewToolStripMenuItem
            // 
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.printPreviewToolStripMenuItem.Text = "Print Preview";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.printPreviewToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // ServiceReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(857, 354);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Name = "ServiceReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ServiceReport";
            this.Load += new System.EventHandler(this.ServiceReport_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn15;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn17;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn price22;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn data3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn21;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}