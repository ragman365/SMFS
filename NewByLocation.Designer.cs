using MyXtraGrid;

namespace SMFS
{
    partial class NewByLocation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewByLocation));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn48 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn50 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn51 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn52 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn49 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.ibtrust75 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.sptrust85 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn109 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.total86 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.lapses4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.reinstates5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn57 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ibtrustytd4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.sptrustytd4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.totalytd5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.lapsesytd4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.reinstatesytd5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.chkExcludeBlankLine = new System.Windows.Forms.CheckBox();
            this.chkInclude = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
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
            this.panelAll.Location = new System.Drawing.Point(0, 28);
            this.panelAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(1302, 403);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 75);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1302, 328);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "iMaginary";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(1302, 328);
            this.dgv.TabIndex = 8;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain});
            // 
            // gridMain
            // 
            this.gridMain.Appearance.BandPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(206)))), ((int)(((byte)(164)))));
            this.gridMain.Appearance.BandPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(206)))), ((int)(((byte)(164)))));
            this.gridMain.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.BandPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.BandPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseFont = true;
            this.gridMain.Appearance.BandPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.gridMain.Appearance.BandPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.ColumnFilterButton.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain.Appearance.ColumnFilterButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.ColumnFilterButton.ForeColor = System.Drawing.Color.Gray;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseForeColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(241)))), ((int)(((byte)(209)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.ForeColor = System.Drawing.Color.Blue;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseForeColor = true;
            this.gridMain.Appearance.Empty.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain.Appearance.Empty.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.GhostWhite;
            this.gridMain.Appearance.EvenRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.EvenRow.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal;
            this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterCloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain.Appearance.FilterCloseButton.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.gridMain.Appearance.FilterCloseButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain.Appearance.FilterCloseButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FilterCloseButton.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterPanel.BackColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FilterPanel.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain.Appearance.FilterPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FilterPanel.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal;
            this.gridMain.Appearance.FilterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.FixedLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(96)))), ((int)(((byte)(0)))));
            this.gridMain.Appearance.FixedLine.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(166)))), ((int)(((byte)(70)))));
            this.gridMain.Appearance.FocusedRow.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(216)))), ((int)(((byte)(120)))));
            this.gridMain.Appearance.FocusedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FooterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.FooterPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.FooterPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FooterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.GroupButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.GroupButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupButton.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupButton.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(226)))), ((int)(((byte)(184)))));
            this.gridMain.Appearance.GroupFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(226)))), ((int)(((byte)(184)))));
            this.gridMain.Appearance.GroupFooter.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupFooter.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupPanel.BackColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupPanel.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.GroupPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.GroupPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.GroupPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupPanel.Options.UseFont = true;
            this.gridMain.Appearance.GroupPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(166)))), ((int)(((byte)(70)))));
            this.gridMain.Appearance.GroupRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(236)))), ((int)(((byte)(215)))));
            this.gridMain.Appearance.GroupRow.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.HeaderPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.HeaderPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.HeaderPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.gridMain.Appearance.HeaderPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.BackColor = System.Drawing.Color.Gray;
            this.gridMain.Appearance.HideSelectionRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain.Appearance.HideSelectionRow.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HorzLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.HorzLine.Options.UseBackColor = true;
            this.gridMain.Appearance.OddRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(236)))), ((int)(((byte)(215)))));
            this.gridMain.Appearance.OddRow.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.OddRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.OddRow.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.BackwardDiagonal;
            this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            this.gridMain.Appearance.OddRow.Options.UseForeColor = true;
            this.gridMain.Appearance.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(234)))));
            this.gridMain.Appearance.Preview.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.Preview.ForeColor = System.Drawing.Color.Maroon;
            this.gridMain.Appearance.Preview.Options.UseBackColor = true;
            this.gridMain.Appearance.Preview.Options.UseForeColor = true;
            this.gridMain.Appearance.Row.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.Row.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridMain.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.Row.Options.UseBackColor = true;
            this.gridMain.Appearance.Row.Options.UseFont = true;
            this.gridMain.Appearance.Row.Options.UseForeColor = true;
            this.gridMain.Appearance.RowSeparator.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.RowSeparator.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain.Appearance.RowSeparator.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(176)))), ((int)(((byte)(80)))));
            this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.SelectedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.VertLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain.Appearance.VertLine.Options.UseBackColor = true;
            this.gridMain.AppearancePrint.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain.AppearancePrint.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand5,
            this.gridBand1});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn48,
            this.bandedGridColumn57,
            this.bandedGridColumn49,
            this.bandedGridColumn51,
            this.bandedGridColumn50,
            this.bandedGridColumn52,
            this.bandedGridColumn3,
            this.bandedGridColumn1,
            this.bandedGridColumn2,
            this.ibtrust75,
            this.sptrust85,
            this.bandedGridColumn109,
            this.total86,
            this.lapses4,
            this.reinstates5,
            this.ibtrustytd4,
            this.sptrustytd4,
            this.totalytd5,
            this.lapsesytd4,
            this.reinstatesytd5});
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.GroupCount = 1;
            this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "ibtrust", this.ibtrust75, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "sptrust", this.sptrust85, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "total", this.total86, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "lapses", this.lapses4, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "reinstates", this.reinstates5, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "ibtrustytd", this.ibtrustytd4, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "sptrustytd", this.sptrustytd4, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "totalytd", this.totalytd5, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "lapsesytd", this.lapsesytd4, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "reinstatesytd", this.reinstatesytd5, "${0:0,0.00}")});
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.Editable = false;
            this.gridMain.OptionsBehavior.ReadOnly = true;
            this.gridMain.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain.OptionsView.ColumnAutoWidth = true;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleAlways;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.bandedGridColumn52, DevExpress.Data.ColumnSortOrder.Ascending)});
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            this.gridMain.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridMain_CustomDrawCell);
            this.gridMain.CustomDrawGroupRow += new DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventHandler(this.gridMain_CustomDrawGroupRow);
            this.gridMain.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gridMain_RowCellStyle);
            this.gridMain.CustomRowFilter += new DevExpress.XtraGrid.Views.Base.RowFilterEventHandler(this.gridMain_CustomRowFilter);
            this.gridMain.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridMain_CustomColumnDisplayText);
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
            // 
            // gridBand5
            // 
            this.gridBand5.Caption = "Current Information";
            this.gridBand5.Columns.Add(this.bandedGridColumn48);
            this.gridBand5.Columns.Add(this.bandedGridColumn50);
            this.gridBand5.Columns.Add(this.bandedGridColumn51);
            this.gridBand5.Columns.Add(this.bandedGridColumn52);
            this.gridBand5.Columns.Add(this.bandedGridColumn3);
            this.gridBand5.Columns.Add(this.bandedGridColumn49);
            this.gridBand5.Columns.Add(this.bandedGridColumn1);
            this.gridBand5.Columns.Add(this.bandedGridColumn2);
            this.gridBand5.Columns.Add(this.ibtrust75);
            this.gridBand5.Columns.Add(this.sptrust85);
            this.gridBand5.Columns.Add(this.bandedGridColumn109);
            this.gridBand5.Columns.Add(this.total86);
            this.gridBand5.Columns.Add(this.lapses4);
            this.gridBand5.Columns.Add(this.reinstates5);
            this.gridBand5.Columns.Add(this.bandedGridColumn57);
            this.gridBand5.MinWidth = 12;
            this.gridBand5.Name = "gridBand5";
            this.gridBand5.VisibleIndex = 0;
            this.gridBand5.Width = 1040;
            // 
            // bandedGridColumn48
            // 
            this.bandedGridColumn48.Caption = "Num";
            this.bandedGridColumn48.FieldName = "num";
            this.bandedGridColumn48.MinWidth = 23;
            this.bandedGridColumn48.Name = "bandedGridColumn48";
            this.bandedGridColumn48.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn48.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn48.Visible = true;
            this.bandedGridColumn48.Width = 58;
            // 
            // bandedGridColumn50
            // 
            this.bandedGridColumn50.Caption = "Agent Name";
            this.bandedGridColumn50.FieldName = "agentName";
            this.bandedGridColumn50.MinWidth = 23;
            this.bandedGridColumn50.Name = "bandedGridColumn50";
            this.bandedGridColumn50.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn50.Width = 175;
            // 
            // bandedGridColumn51
            // 
            this.bandedGridColumn51.Caption = "Loc";
            this.bandedGridColumn51.FieldName = "loc";
            this.bandedGridColumn51.MinWidth = 23;
            this.bandedGridColumn51.Name = "bandedGridColumn51";
            this.bandedGridColumn51.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn51.Visible = true;
            this.bandedGridColumn51.Width = 47;
            // 
            // bandedGridColumn52
            // 
            this.bandedGridColumn52.Caption = "Location Name";
            this.bandedGridColumn52.FieldName = "Location Name";
            this.bandedGridColumn52.MinWidth = 23;
            this.bandedGridColumn52.Name = "bandedGridColumn52";
            this.bandedGridColumn52.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn52.Visible = true;
            this.bandedGridColumn52.Width = 175;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "Contracts";
            this.bandedGridColumn3.FieldName = "contracts";
            this.bandedGridColumn3.MinWidth = 23;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 146;
            // 
            // bandedGridColumn49
            // 
            this.bandedGridColumn49.Caption = "Agent";
            this.bandedGridColumn49.FieldName = "agentNumber";
            this.bandedGridColumn49.MinWidth = 23;
            this.bandedGridColumn49.Name = "bandedGridColumn49";
            this.bandedGridColumn49.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn49.Width = 70;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "Contract  Number";
            this.bandedGridColumn1.FieldName = "contractNumber";
            this.bandedGridColumn1.MinWidth = 23;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Width = 87;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.Caption = "Contract Value";
            this.bandedGridColumn2.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn2.FieldName = "contractValue";
            this.bandedGridColumn2.MinWidth = 23;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Width = 87;
            // 
            // ibtrust75
            // 
            this.ibtrust75.Caption = "IB Trust";
            this.ibtrust75.DisplayFormat.FormatString = "N2";
            this.ibtrust75.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.ibtrust75.FieldName = "ibtrust";
            this.ibtrust75.MinWidth = 23;
            this.ibtrust75.Name = "ibtrust75";
            this.ibtrust75.OptionsColumn.FixedWidth = true;
            this.ibtrust75.Visible = true;
            this.ibtrust75.Width = 117;
            // 
            // sptrust85
            // 
            this.sptrust85.Caption = "SP Trust";
            this.sptrust85.DisplayFormat.FormatString = "N2";
            this.sptrust85.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.sptrust85.FieldName = "sptrust";
            this.sptrust85.MinWidth = 23;
            this.sptrust85.Name = "sptrust85";
            this.sptrust85.OptionsColumn.FixedWidth = true;
            this.sptrust85.Visible = true;
            this.sptrust85.Width = 117;
            // 
            // bandedGridColumn109
            // 
            this.bandedGridColumn109.Caption = "XX Trust";
            this.bandedGridColumn109.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn109.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn109.FieldName = "xxtrust";
            this.bandedGridColumn109.MinWidth = 23;
            this.bandedGridColumn109.Name = "bandedGridColumn109";
            this.bandedGridColumn109.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn109.Width = 87;
            // 
            // total86
            // 
            this.total86.Caption = "Total";
            this.total86.DisplayFormat.FormatString = "N2";
            this.total86.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.total86.FieldName = "total";
            this.total86.MinWidth = 23;
            this.total86.Name = "total86";
            this.total86.OptionsColumn.FixedWidth = true;
            this.total86.Visible = true;
            this.total86.Width = 146;
            // 
            // lapses4
            // 
            this.lapses4.Caption = "Lapses";
            this.lapses4.DisplayFormat.FormatString = "N2";
            this.lapses4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.lapses4.FieldName = "lapses";
            this.lapses4.MinWidth = 23;
            this.lapses4.Name = "lapses4";
            this.lapses4.OptionsColumn.FixedWidth = true;
            this.lapses4.Visible = true;
            this.lapses4.Width = 117;
            // 
            // reinstates5
            // 
            this.reinstates5.Caption = "Reinstates";
            this.reinstates5.DisplayFormat.FormatString = "N2";
            this.reinstates5.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.reinstates5.FieldName = "reinstates";
            this.reinstates5.MinWidth = 23;
            this.reinstates5.Name = "reinstates5";
            this.reinstates5.OptionsColumn.FixedWidth = true;
            this.reinstates5.Visible = true;
            this.reinstates5.Width = 117;
            // 
            // bandedGridColumn57
            // 
            this.bandedGridColumn57.Caption = "record";
            this.bandedGridColumn57.FieldName = "record";
            this.bandedGridColumn57.MinWidth = 23;
            this.bandedGridColumn57.Name = "bandedGridColumn57";
            this.bandedGridColumn57.RowCount = 2;
            this.bandedGridColumn57.Width = 87;
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "YTD Information";
            this.gridBand1.Columns.Add(this.ibtrustytd4);
            this.gridBand1.Columns.Add(this.sptrustytd4);
            this.gridBand1.Columns.Add(this.totalytd5);
            this.gridBand1.Columns.Add(this.lapsesytd4);
            this.gridBand1.Columns.Add(this.reinstatesytd5);
            this.gridBand1.MinWidth = 12;
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 1;
            this.gridBand1.Width = 579;
            // 
            // ibtrustytd4
            // 
            this.ibtrustytd4.Caption = "IB Trust YTD";
            this.ibtrustytd4.DisplayFormat.FormatString = "N2";
            this.ibtrustytd4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.ibtrustytd4.FieldName = "ibtrustytd";
            this.ibtrustytd4.MinWidth = 23;
            this.ibtrustytd4.Name = "ibtrustytd4";
            this.ibtrustytd4.OptionsColumn.FixedWidth = true;
            this.ibtrustytd4.Visible = true;
            this.ibtrustytd4.Width = 142;
            // 
            // sptrustytd4
            // 
            this.sptrustytd4.Caption = "SP Trust YTD";
            this.sptrustytd4.DisplayFormat.FormatString = "N2";
            this.sptrustytd4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.sptrustytd4.FieldName = "sptrustytd";
            this.sptrustytd4.MinWidth = 23;
            this.sptrustytd4.Name = "sptrustytd4";
            this.sptrustytd4.OptionsColumn.FixedWidth = true;
            this.sptrustytd4.Visible = true;
            this.sptrustytd4.Width = 117;
            // 
            // totalytd5
            // 
            this.totalytd5.Caption = "Total YTD";
            this.totalytd5.DisplayFormat.FormatString = "N2";
            this.totalytd5.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.totalytd5.FieldName = "totalytd";
            this.totalytd5.MinWidth = 23;
            this.totalytd5.Name = "totalytd5";
            this.totalytd5.OptionsColumn.FixedWidth = true;
            this.totalytd5.Visible = true;
            this.totalytd5.Width = 130;
            // 
            // lapsesytd4
            // 
            this.lapsesytd4.Caption = "Lapses YTD";
            this.lapsesytd4.DisplayFormat.FormatString = "N2";
            this.lapsesytd4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.lapsesytd4.FieldName = "lapsesytd";
            this.lapsesytd4.MinWidth = 23;
            this.lapsesytd4.Name = "lapsesytd4";
            this.lapsesytd4.OptionsColumn.FixedWidth = true;
            this.lapsesytd4.Visible = true;
            this.lapsesytd4.Width = 94;
            // 
            // reinstatesytd5
            // 
            this.reinstatesytd5.Caption = "Reinstates YTD";
            this.reinstatesytd5.DisplayFormat.FormatString = "N2";
            this.reinstatesytd5.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.reinstatesytd5.FieldName = "reinstatesytd";
            this.reinstatesytd5.MinWidth = 23;
            this.reinstatesytd5.Name = "reinstatesytd5";
            this.reinstatesytd5.OptionsColumn.FixedWidth = true;
            this.reinstatesytd5.Visible = true;
            this.reinstatesytd5.Width = 96;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.PeachPuff;
            this.panelTop.Controls.Add(this.chkExcludeBlankLine);
            this.panelTop.Controls.Add(this.chkInclude);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.txtScale);
            this.panelTop.Controls.Add(this.btnRun);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Controls.Add(this.btnRight);
            this.panelTop.Controls.Add(this.btnLeft);
            this.panelTop.Controls.Add(this.dateTimePicker2);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.dateTimePicker1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1302, 75);
            this.panelTop.TabIndex = 1;
            // 
            // chkExcludeBlankLine
            // 
            this.chkExcludeBlankLine.AutoSize = true;
            this.chkExcludeBlankLine.Location = new System.Drawing.Point(296, 45);
            this.chkExcludeBlankLine.Name = "chkExcludeBlankLine";
            this.chkExcludeBlankLine.Size = new System.Drawing.Size(148, 21);
            this.chkExcludeBlankLine.TabIndex = 38;
            this.chkExcludeBlankLine.Text = "Exclude Blank Lines";
            this.chkExcludeBlankLine.UseVisualStyleBackColor = true;
            // 
            // chkInclude
            // 
            this.chkInclude.AutoSize = true;
            this.chkInclude.Location = new System.Drawing.Point(93, 45);
            this.chkInclude.Name = "chkInclude";
            this.chkInclude.Size = new System.Drawing.Size(181, 21);
            this.chkInclude.TabIndex = 37;
            this.chkInclude.Text = "Include Empty Locations";
            this.chkInclude.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1189, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 17);
            this.label1.TabIndex = 36;
            this.label1.Text = "Scale";
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(1122, 14);
            this.txtScale.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(63, 23);
            this.txtScale.TabIndex = 35;
            this.txtScale.Text = "100.00";
            this.txtScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtScale.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtScale_KeyUp);
            // 
            // btnRun
            // 
            this.btnRun.BackColor = System.Drawing.Color.SandyBrown;
            this.btnRun.Font = new System.Drawing.Font("Tahoma", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnRun.Location = new System.Drawing.Point(657, 9);
            this.btnRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(126, 58);
            this.btnRun.TabIndex = 28;
            this.btnRun.Text = "Run by Location";
            this.btnRun.UseVisualStyleBackColor = false;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
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
            this.btnRight.Location = new System.Drawing.Point(608, 14);
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
            // dateTimePicker2
            // 
            this.dateTimePicker2.Location = new System.Drawing.Point(372, 15);
            this.dateTimePicker2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(233, 23);
            this.dateTimePicker2.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(334, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 17);
            this.label2.TabIndex = 23;
            this.label2.Text = "-To-";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(93, 15);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(233, 23);
            this.dateTimePicker1.TabIndex = 22;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1302, 28);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPrint,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
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
            // NewByLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1302, 431);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "NewByLocation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Business by Location";
            this.Load += new System.EventHandler(this.NewByLocation_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
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
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnRun;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn48;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn49;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn50;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn51;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn52;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ibtrust75;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn sptrust85;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn109;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn total86;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn57;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn lapses4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn reinstates5;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ibtrustytd4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn sptrustytd4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn totalytd5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn lapsesytd4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn reinstatesytd5;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand5;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtScale;
        private System.Windows.Forms.CheckBox chkInclude;
        private System.Windows.Forms.CheckBox chkExcludeBlankLine;
    }
}