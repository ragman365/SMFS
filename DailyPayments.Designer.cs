using MyXtraGrid;

namespace SMFS
{
    partial class DailyPayments
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DailyPayments));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn28 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn44 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn17 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn35 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn22 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn18 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn23 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn19 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn15 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn16 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn51 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn14 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn41 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemComboBox2 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.panelTop = new System.Windows.Forms.Panel();
            this.chkGroupBank = new System.Windows.Forms.CheckBox();
            this.chkGroupFound = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.btnGetDeposits = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox2)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 30);
            this.panelAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(1354, 401);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 55);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1354, 346);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.MainView = this.gridMain;
            this.dgv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBox2});
            this.dgv.Size = new System.Drawing.Size(1354, 346);
            this.dgv.TabIndex = 6;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain});
            // 
            // gridMain
            // 
            this.gridMain.Appearance.BandPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.BandPanel.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(199)))), ((int)(((byte)(146)))));
            this.gridMain.Appearance.BandPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.BandPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.BandPanel.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.gridMain.Appearance.BandPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseFont = true;
            this.gridMain.Appearance.BandPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.BandPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.ColumnFilterButton.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(199)))), ((int)(((byte)(146)))));
            this.gridMain.Appearance.ColumnFilterButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.ColumnFilterButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.ColumnFilterButton.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseForeColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(255)))), ((int)(((byte)(244)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(222)))), ((int)(((byte)(183)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(255)))), ((int)(((byte)(244)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.ColumnFilterButtonActive.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseForeColor = true;
            this.gridMain.Appearance.Empty.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.Empty.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.EvenRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterCloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.FilterCloseButton.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(199)))), ((int)(((byte)(146)))));
            this.gridMain.Appearance.FilterCloseButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.FilterCloseButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FilterCloseButton.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(158)))), ((int)(((byte)(126)))));
            this.gridMain.Appearance.FilterPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FilterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.FixedLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(128)))), ((int)(((byte)(88)))));
            this.gridMain.Appearance.FixedLine.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedCell.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FocusedCell.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FocusedCell.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedCell.Options.UseForeColor = true;
            this.gridMain.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(160)))), ((int)(((byte)(112)))));
            this.gridMain.Appearance.FocusedRow.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FooterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.FooterPanel.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(199)))), ((int)(((byte)(146)))));
            this.gridMain.Appearance.FooterPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.FooterPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FooterPanel.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.gridMain.Appearance.FooterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(230)))), ((int)(((byte)(195)))));
            this.gridMain.Appearance.GroupButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(230)))), ((int)(((byte)(195)))));
            this.gridMain.Appearance.GroupButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupButton.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupButton.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(230)))), ((int)(((byte)(195)))));
            this.gridMain.Appearance.GroupFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(230)))), ((int)(((byte)(195)))));
            this.gridMain.Appearance.GroupFooter.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.GroupFooter.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupFooter.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseFont = true;
            this.gridMain.Appearance.GroupFooter.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(158)))), ((int)(((byte)(126)))));
            this.gridMain.Appearance.GroupPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.GroupPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(230)))), ((int)(((byte)(195)))));
            this.gridMain.Appearance.GroupRow.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(230)))), ((int)(((byte)(195)))));
            this.gridMain.Appearance.GroupRow.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.GroupRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupRow.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupRow.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupRow.Options.UseFont = true;
            this.gridMain.Appearance.GroupRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.HeaderPanel.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(199)))), ((int)(((byte)(146)))));
            this.gridMain.Appearance.HeaderPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(222)))));
            this.gridMain.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.HeaderPanel.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.gridMain.Appearance.HeaderPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanelBackground.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.HeaderPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(207)))), ((int)(((byte)(170)))));
            this.gridMain.Appearance.HideSelectionRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(160)))), ((int)(((byte)(112)))));
            this.gridMain.Appearance.HideSelectionRow.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HorzLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(168)))), ((int)(((byte)(128)))));
            this.gridMain.Appearance.HorzLine.Options.UseBackColor = true;
            this.gridMain.Appearance.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(253)))), ((int)(((byte)(247)))));
            this.gridMain.Appearance.Preview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(160)))), ((int)(((byte)(112)))));
            this.gridMain.Appearance.Preview.Options.UseBackColor = true;
            this.gridMain.Appearance.Preview.Options.UseForeColor = true;
            this.gridMain.Appearance.Row.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.Row.Options.UseBackColor = true;
            this.gridMain.Appearance.Row.Options.UseForeColor = true;
            this.gridMain.Appearance.RowSeparator.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.RowSeparator.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(178)))), ((int)(((byte)(133)))));
            this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.SelectedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.TopNewRow.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.TopNewRow.Options.UseBackColor = true;
            this.gridMain.Appearance.VertLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(160)))), ((int)(((byte)(188)))));
            this.gridMain.Appearance.VertLine.Options.UseBackColor = true;
            this.gridMain.AppearancePrint.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain.AppearancePrint.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand2});
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn28,
            this.bandedGridColumn41,
            this.bandedGridColumn44,
            this.bandedGridColumn17,
            this.bandedGridColumn35,
            this.bandedGridColumn22,
            this.bandedGridColumn18,
            this.bandedGridColumn51,
            this.bandedGridColumn14,
            this.bandedGridColumn15,
            this.bandedGridColumn16,
            this.bandedGridColumn19,
            this.bandedGridColumn23});
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "amount", this.bandedGridColumn35, "{0:N2}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "inSystem", this.bandedGridColumn18, "{0:N2}")});
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Flat";
            this.gridMain.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridMain_CustomColumnDisplayText);
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
            // 
            // gridBand2
            // 
            this.gridBand2.Columns.Add(this.bandedGridColumn28);
            this.gridBand2.Columns.Add(this.bandedGridColumn44);
            this.gridBand2.Columns.Add(this.bandedGridColumn17);
            this.gridBand2.Columns.Add(this.bandedGridColumn35);
            this.gridBand2.Columns.Add(this.bandedGridColumn22);
            this.gridBand2.Columns.Add(this.bandedGridColumn18);
            this.gridBand2.Columns.Add(this.bandedGridColumn23);
            this.gridBand2.Columns.Add(this.bandedGridColumn19);
            this.gridBand2.Columns.Add(this.bandedGridColumn15);
            this.gridBand2.Columns.Add(this.bandedGridColumn16);
            this.gridBand2.Columns.Add(this.bandedGridColumn51);
            this.gridBand2.Columns.Add(this.bandedGridColumn14);
            this.gridBand2.Columns.Add(this.bandedGridColumn41);
            this.gridBand2.MinWidth = 104;
            this.gridBand2.Name = "gridBand2";
            this.gridBand2.VisibleIndex = 0;
            this.gridBand2.Width = 1127;
            // 
            // bandedGridColumn28
            // 
            this.bandedGridColumn28.AppearanceHeader.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn28.AppearanceHeader.Options.UseForeColor = true;
            this.bandedGridColumn28.Caption = "Num";
            this.bandedGridColumn28.FieldName = "num";
            this.bandedGridColumn28.MinWidth = 55;
            this.bandedGridColumn28.Name = "bandedGridColumn28";
            this.bandedGridColumn28.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn28.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn28.Visible = true;
            this.bandedGridColumn28.Width = 55;
            // 
            // bandedGridColumn44
            // 
            this.bandedGridColumn44.Caption = "Date";
            this.bandedGridColumn44.DisplayFormat.FormatString = "yyyy-MM-dd";
            this.bandedGridColumn44.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.bandedGridColumn44.FieldName = "date";
            this.bandedGridColumn44.MinWidth = 55;
            this.bandedGridColumn44.Name = "bandedGridColumn44";
            this.bandedGridColumn44.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn44.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn44.Visible = true;
            this.bandedGridColumn44.Width = 101;
            // 
            // bandedGridColumn17
            // 
            this.bandedGridColumn17.Caption = "sDate";
            this.bandedGridColumn17.FieldName = "sDate";
            this.bandedGridColumn17.MinWidth = 55;
            this.bandedGridColumn17.Name = "bandedGridColumn17";
            this.bandedGridColumn17.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn17.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn17.Width = 174;
            // 
            // bandedGridColumn35
            // 
            this.bandedGridColumn35.Caption = "Amount in System";
            this.bandedGridColumn35.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn35.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn35.FieldName = "amount";
            this.bandedGridColumn35.MinWidth = 55;
            this.bandedGridColumn35.Name = "bandedGridColumn35";
            this.bandedGridColumn35.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn35.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn35.Visible = true;
            this.bandedGridColumn35.Width = 91;
            // 
            // bandedGridColumn22
            // 
            this.bandedGridColumn22.Caption = "Debit in Bank";
            this.bandedGridColumn22.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn22.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn22.FieldName = "debit";
            this.bandedGridColumn22.MinWidth = 29;
            this.bandedGridColumn22.Name = "bandedGridColumn22";
            this.bandedGridColumn22.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn22.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn22.Width = 78;
            // 
            // bandedGridColumn18
            // 
            this.bandedGridColumn18.Caption = "Amount in System";
            this.bandedGridColumn18.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn18.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn18.FieldName = "inSystem";
            this.bandedGridColumn18.MinWidth = 55;
            this.bandedGridColumn18.Name = "bandedGridColumn18";
            this.bandedGridColumn18.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn18.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn18.Width = 87;
            // 
            // bandedGridColumn23
            // 
            this.bandedGridColumn23.Caption = "Difference";
            this.bandedGridColumn23.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn23.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn23.FieldName = "diff";
            this.bandedGridColumn23.MinWidth = 25;
            this.bandedGridColumn23.Name = "bandedGridColumn23";
            this.bandedGridColumn23.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn23.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn23.Width = 74;
            // 
            // bandedGridColumn19
            // 
            this.bandedGridColumn19.Caption = "Deposit Number";
            this.bandedGridColumn19.FieldName = "depositNumber";
            this.bandedGridColumn19.MinWidth = 34;
            this.bandedGridColumn19.Name = "bandedGridColumn19";
            this.bandedGridColumn19.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn19.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn19.Visible = true;
            this.bandedGridColumn19.Width = 94;
            // 
            // bandedGridColumn15
            // 
            this.bandedGridColumn15.Caption = "Found";
            this.bandedGridColumn15.FieldName = "found";
            this.bandedGridColumn15.MinWidth = 55;
            this.bandedGridColumn15.Name = "bandedGridColumn15";
            this.bandedGridColumn15.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn15.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn15.Visible = true;
            this.bandedGridColumn15.Width = 96;
            // 
            // bandedGridColumn16
            // 
            this.bandedGridColumn16.Caption = "ID";
            this.bandedGridColumn16.FieldName = "ID";
            this.bandedGridColumn16.MinWidth = 55;
            this.bandedGridColumn16.Name = "bandedGridColumn16";
            this.bandedGridColumn16.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn16.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn16.Visible = true;
            this.bandedGridColumn16.Width = 93;
            // 
            // bandedGridColumn51
            // 
            this.bandedGridColumn51.Caption = "Bank Account";
            this.bandedGridColumn51.FieldName = "bankAccount";
            this.bandedGridColumn51.MinWidth = 55;
            this.bandedGridColumn51.Name = "bandedGridColumn51";
            this.bandedGridColumn51.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn51.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn51.Visible = true;
            this.bandedGridColumn51.Width = 120;
            // 
            // bandedGridColumn14
            // 
            this.bandedGridColumn14.Caption = "Bank Details";
            this.bandedGridColumn14.FieldName = "bankDetails";
            this.bandedGridColumn14.MinWidth = 55;
            this.bandedGridColumn14.Name = "bandedGridColumn14";
            this.bandedGridColumn14.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn14.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn14.Visible = true;
            this.bandedGridColumn14.Width = 477;
            // 
            // bandedGridColumn41
            // 
            this.bandedGridColumn41.Caption = "record";
            this.bandedGridColumn41.FieldName = "record";
            this.bandedGridColumn41.MinWidth = 55;
            this.bandedGridColumn41.Name = "bandedGridColumn41";
            this.bandedGridColumn41.Width = 747;
            // 
            // repositoryItemComboBox2
            // 
            this.repositoryItemComboBox2.AutoHeight = false;
            this.repositoryItemComboBox2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox2.Items.AddRange(new object[] {
            "Trust",
            "Funeral",
            "Insurance",
            "DownPmt"});
            this.repositoryItemComboBox2.Name = "repositoryItemComboBox2";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.Ivory;
            this.panelTop.Controls.Add(this.chkGroupBank);
            this.panelTop.Controls.Add(this.chkGroupFound);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.txtScale);
            this.panelTop.Controls.Add(this.btnGetDeposits);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Controls.Add(this.btnRight);
            this.panelTop.Controls.Add(this.btnLeft);
            this.panelTop.Controls.Add(this.dateTimePicker1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1354, 55);
            this.panelTop.TabIndex = 1;
            // 
            // chkGroupBank
            // 
            this.chkGroupBank.AutoSize = true;
            this.chkGroupBank.Location = new System.Drawing.Point(743, 18);
            this.chkGroupBank.Name = "chkGroupBank";
            this.chkGroupBank.Size = new System.Drawing.Size(177, 21);
            this.chkGroupBank.TabIndex = 166;
            this.chkGroupBank.Text = "Group by Bank Account";
            this.chkGroupBank.UseVisualStyleBackColor = true;
            this.chkGroupBank.CheckedChanged += new System.EventHandler(this.chkGroupBank_CheckedChanged);
            // 
            // chkGroupFound
            // 
            this.chkGroupFound.AutoSize = true;
            this.chkGroupFound.Location = new System.Drawing.Point(592, 18);
            this.chkGroupFound.Name = "chkGroupFound";
            this.chkGroupFound.Size = new System.Drawing.Size(131, 21);
            this.chkGroupFound.TabIndex = 165;
            this.chkGroupFound.Text = "Group by Found";
            this.chkGroupFound.UseVisualStyleBackColor = true;
            this.chkGroupFound.CheckedChanged += new System.EventHandler(this.chkGroupFound_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1054, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 17);
            this.label3.TabIndex = 47;
            this.label3.Text = "Scale";
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(987, 15);
            this.txtScale.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(63, 23);
            this.txtScale.TabIndex = 46;
            this.txtScale.Text = "110.00";
            this.txtScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtScale.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtScale_KeyUp);
            // 
            // btnGetDeposits
            // 
            this.btnGetDeposits.BackColor = System.Drawing.Color.SandyBrown;
            this.btnGetDeposits.Font = new System.Drawing.Font("Tahoma", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnGetDeposits.Location = new System.Drawing.Point(385, 11);
            this.btnGetDeposits.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnGetDeposits.Name = "btnGetDeposits";
            this.btnGetDeposits.Size = new System.Drawing.Size(173, 31);
            this.btnGetDeposits.TabIndex = 29;
            this.btnGetDeposits.Text = "Get Payments";
            this.btnGetDeposits.UseVisualStyleBackColor = false;
            this.btnGetDeposits.Click += new System.EventHandler(this.btnGetDeposits_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(14, 15);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(33, 27);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 27;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.PeachPuff;
            this.btnRight.Image = ((System.Drawing.Image)(resources.GetObject("btnRight.Image")));
            this.btnRight.Location = new System.Drawing.Point(329, 14);
            this.btnRight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(33, 28);
            this.btnRight.TabIndex = 26;
            this.btnRight.UseVisualStyleBackColor = false;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.PeachPuff;
            this.btnLeft.Image = ((System.Drawing.Image)(resources.GetObject("btnLeft.Image")));
            this.btnLeft.Location = new System.Drawing.Point(58, 14);
            this.btnLeft.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(33, 28);
            this.btnLeft.TabIndex = 25;
            this.btnLeft.UseVisualStyleBackColor = false;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(93, 15);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(233, 23);
            this.dateTimePicker1.TabIndex = 22;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1354, 30);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPrint,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 26);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuPrint
            // 
            this.menuPrint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printPreviewToolStripMenuItem,
            this.printToolStripMenuItem});
            this.menuPrint.Name = "menuPrint";
            this.menuPrint.Size = new System.Drawing.Size(122, 26);
            this.menuPrint.Text = "Print";
            // 
            // printPreviewToolStripMenuItem
            // 
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(177, 26);
            this.printPreviewToolStripMenuItem.Text = "Print Preview";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.printPreviewToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(177, 26);
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(122, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // DailyPayments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1354, 431);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "DailyPayments";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Daily Payments All Sources";
            this.Load += new System.EventHandler(this.DailyPayments_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox2)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button btnGetDeposits;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtScale;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn28;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn44;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn17;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn35;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn22;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn18;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn23;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn19;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn15;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn16;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn51;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn14;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn41;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox2;
        private System.Windows.Forms.CheckBox chkGroupFound;
        private System.Windows.Forms.CheckBox chkGroupBank;
    }
}