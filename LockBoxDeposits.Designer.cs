using MyXtraGrid;

namespace SMFS
{
    partial class LockBoxDeposits
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LockBoxDeposits));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn48 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.TDA2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.IDA3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.NOX4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.Transfers5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.balance6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.bandedGridColumn57 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnSelectColumns = new System.Windows.Forms.Button();
            this.cmbSelectColumns = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBeginningBalance = new System.Windows.Forms.TextBox();
            this.pictureDelete = new System.Windows.Forms.PictureBox();
            this.pictureAdd = new System.Windows.Forms.PictureBox();
            this.cmbLockbox = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
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
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.lockScreenDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unlockScreenDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDelete)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureAdd)).BeginInit();
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
            this.panelAll.Size = new System.Drawing.Size(1761, 401);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 117);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1761, 284);
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
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemMemoEdit1});
            this.dgv.Size = new System.Drawing.Size(1761, 284);
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
            this.gridMain.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.Row.Options.UseBackColor = true;
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
            this.gridBand5});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn48,
            this.bandedGridColumn57,
            this.bandedGridColumn1,
            this.bandedGridColumn4,
            this.bandedGridColumn7,
            this.TDA2,
            this.IDA3,
            this.NOX4,
            this.bandedGridColumn5,
            this.Transfers5,
            this.bandedGridColumn6,
            this.bandedGridColumn3,
            this.balance6,
            this.bandedGridColumn2});
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "TDA", this.TDA2, "${0:0,0.00}")});
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleAlways;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            this.gridMain.CustomDrawFooterCell += new DevExpress.XtraGrid.Views.Grid.FooterCellCustomDrawEventHandler(this.gridMain_CustomDrawFooterCell);
            this.gridMain.CustomDrawGroupRow += new DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventHandler(this.gridMain_CustomDrawGroupRow);
            this.gridMain.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gridMain_RowCellStyle);
            this.gridMain.CalcRowHeight += new DevExpress.XtraGrid.Views.Grid.RowHeightEventHandler(this.gridMain_CalcRowHeight);
            this.gridMain.CustomSummaryCalculate += new DevExpress.Data.CustomSummaryEventHandler(this.gridMain_CustomSummaryCalculate);
            this.gridMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridMain_CellValueChanged);
            this.gridMain.CustomRowFilter += new DevExpress.XtraGrid.Views.Base.RowFilterEventHandler(this.gridMain_CustomRowFilter);
            this.gridMain.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridMain_CustomColumnDisplayText);
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
            this.gridMain.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridMain_ValidatingEditor);
            // 
            // gridBand5
            // 
            this.gridBand5.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand5.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.gridBand5.Caption = "Current Information";
            this.gridBand5.Columns.Add(this.bandedGridColumn48);
            this.gridBand5.Columns.Add(this.bandedGridColumn1);
            this.gridBand5.Columns.Add(this.bandedGridColumn4);
            this.gridBand5.Columns.Add(this.bandedGridColumn7);
            this.gridBand5.Columns.Add(this.TDA2);
            this.gridBand5.Columns.Add(this.IDA3);
            this.gridBand5.Columns.Add(this.NOX4);
            this.gridBand5.Columns.Add(this.bandedGridColumn3);
            this.gridBand5.Columns.Add(this.bandedGridColumn6);
            this.gridBand5.Columns.Add(this.bandedGridColumn5);
            this.gridBand5.Columns.Add(this.Transfers5);
            this.gridBand5.Columns.Add(this.balance6);
            this.gridBand5.Columns.Add(this.bandedGridColumn2);
            this.gridBand5.Columns.Add(this.bandedGridColumn57);
            this.gridBand5.MinWidth = 26;
            this.gridBand5.Name = "gridBand5";
            this.gridBand5.VisibleIndex = 0;
            this.gridBand5.Width = 1472;
            // 
            // bandedGridColumn48
            // 
            this.bandedGridColumn48.Caption = "Num";
            this.bandedGridColumn48.FieldName = "num";
            this.bandedGridColumn48.MinWidth = 49;
            this.bandedGridColumn48.Name = "bandedGridColumn48";
            this.bandedGridColumn48.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn48.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn48.Visible = true;
            this.bandedGridColumn48.Width = 49;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "Date";
            this.bandedGridColumn1.FieldName = "date";
            this.bandedGridColumn1.MinWidth = 49;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Visible = true;
            this.bandedGridColumn1.Width = 85;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "DOW";
            this.bandedGridColumn4.FieldName = "dow";
            this.bandedGridColumn4.MinWidth = 55;
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            this.bandedGridColumn4.Width = 90;
            // 
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.Caption = "Trust Down Payment";
            this.bandedGridColumn7.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn7.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn7.FieldName = "NDA";
            this.bandedGridColumn7.MinWidth = 25;
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Visible = true;
            this.bandedGridColumn7.Width = 94;
            // 
            // TDA2
            // 
            this.TDA2.Caption = "Trust Deposit Amount";
            this.TDA2.DisplayFormat.FormatString = "N2";
            this.TDA2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.TDA2.FieldName = "TDA";
            this.TDA2.MinWidth = 49;
            this.TDA2.Name = "TDA2";
            this.TDA2.OptionsColumn.AllowEdit = false;
            this.TDA2.OptionsColumn.FixedWidth = true;
            this.TDA2.Visible = true;
            this.TDA2.Width = 117;
            // 
            // IDA3
            // 
            this.IDA3.Caption = "Insurance Deposit Amount";
            this.IDA3.DisplayFormat.FormatString = "N2";
            this.IDA3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.IDA3.FieldName = "IDA";
            this.IDA3.MinWidth = 49;
            this.IDA3.Name = "IDA3";
            this.IDA3.OptionsColumn.AllowEdit = false;
            this.IDA3.OptionsColumn.FixedWidth = true;
            this.IDA3.Visible = true;
            this.IDA3.Width = 133;
            // 
            // NOX4
            // 
            this.NOX4.Caption = "noxapater Deposit Amount";
            this.NOX4.DisplayFormat.FormatString = "N2";
            this.NOX4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.NOX4.FieldName = "NOX";
            this.NOX4.MinWidth = 49;
            this.NOX4.Name = "NOX4";
            this.NOX4.OptionsColumn.FixedWidth = true;
            this.NOX4.Visible = true;
            this.NOX4.Width = 134;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "Daily Deposits";
            this.bandedGridColumn3.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn3.FieldName = "dailyTotals";
            this.bandedGridColumn3.MinWidth = 40;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 108;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.Caption = "Misc";
            this.bandedGridColumn6.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn6.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn6.FieldName = "misc";
            this.bandedGridColumn6.MinWidth = 25;
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Visible = true;
            this.bandedGridColumn6.Width = 94;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.Caption = "Returns";
            this.bandedGridColumn5.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn5.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn5.FieldName = "returns";
            this.bandedGridColumn5.MinWidth = 47;
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 103;
            // 
            // Transfers5
            // 
            this.Transfers5.Caption = "Transfers";
            this.Transfers5.DisplayFormat.FormatString = "N2";
            this.Transfers5.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.Transfers5.FieldName = "transfers";
            this.Transfers5.MinWidth = 49;
            this.Transfers5.Name = "Transfers5";
            this.Transfers5.OptionsColumn.FixedWidth = true;
            this.Transfers5.Visible = true;
            this.Transfers5.Width = 110;
            // 
            // balance6
            // 
            this.balance6.Caption = "Balance";
            this.balance6.DisplayFormat.FormatString = "N2";
            this.balance6.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.balance6.FieldName = "balance";
            this.balance6.MinWidth = 49;
            this.balance6.Name = "balance6";
            this.balance6.OptionsColumn.FixedWidth = true;
            this.balance6.Visible = true;
            this.balance6.Width = 105;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn2.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn2.Caption = "Comment";
            this.bandedGridColumn2.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn2.FieldName = "comment";
            this.bandedGridColumn2.MinWidth = 49;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 250;
            // 
            // repositoryItemMemoEdit1
            // 
            this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
            // 
            // bandedGridColumn57
            // 
            this.bandedGridColumn57.Caption = "record";
            this.bandedGridColumn57.FieldName = "record";
            this.bandedGridColumn57.MinWidth = 49;
            this.bandedGridColumn57.Name = "bandedGridColumn57";
            this.bandedGridColumn57.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn57.RowCount = 2;
            this.bandedGridColumn57.Width = 188;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.PeachPuff;
            this.panelTop.Controls.Add(this.btnSelectColumns);
            this.panelTop.Controls.Add(this.cmbSelectColumns);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.txtScale);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.txtBeginningBalance);
            this.panelTop.Controls.Add(this.pictureDelete);
            this.panelTop.Controls.Add(this.pictureAdd);
            this.panelTop.Controls.Add(this.cmbLockbox);
            this.panelTop.Controls.Add(this.btnSave);
            this.panelTop.Controls.Add(this.btnGetDeposits);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Controls.Add(this.btnRight);
            this.panelTop.Controls.Add(this.btnLeft);
            this.panelTop.Controls.Add(this.dateTimePicker1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1761, 117);
            this.panelTop.TabIndex = 1;
            // 
            // btnSelectColumns
            // 
            this.btnSelectColumns.BackColor = System.Drawing.Color.SandyBrown;
            this.btnSelectColumns.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectColumns.Location = new System.Drawing.Point(895, 26);
            this.btnSelectColumns.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectColumns.Name = "btnSelectColumns";
            this.btnSelectColumns.Size = new System.Drawing.Size(134, 30);
            this.btnSelectColumns.TabIndex = 97;
            this.btnSelectColumns.Text = "Select Columns";
            this.btnSelectColumns.UseVisualStyleBackColor = false;
            this.btnSelectColumns.Click += new System.EventHandler(this.btnSelectColumns_Click);
            // 
            // cmbSelectColumns
            // 
            this.cmbSelectColumns.FormattingEnabled = true;
            this.cmbSelectColumns.Location = new System.Drawing.Point(710, 29);
            this.cmbSelectColumns.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbSelectColumns.Name = "cmbSelectColumns";
            this.cmbSelectColumns.Size = new System.Drawing.Size(179, 24);
            this.cmbSelectColumns.TabIndex = 96;
            this.cmbSelectColumns.SelectedIndexChanged += new System.EventHandler(this.cmbSelectColumns_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(608, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 17);
            this.label3.TabIndex = 45;
            this.label3.Text = "Scale";
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(541, 75);
            this.txtScale.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(63, 23);
            this.txtScale.TabIndex = 44;
            this.txtScale.Text = "90.00";
            this.txtScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtScale.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtScale_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(855, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 17);
            this.label1.TabIndex = 37;
            this.label1.Text = "Beginning Balance :";
            // 
            // txtBeginningBalance
            // 
            this.txtBeginningBalance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBeginningBalance.Location = new System.Drawing.Point(987, 78);
            this.txtBeginningBalance.Name = "txtBeginningBalance";
            this.txtBeginningBalance.Size = new System.Drawing.Size(135, 23);
            this.txtBeginningBalance.TabIndex = 36;
            this.txtBeginningBalance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBeginningBalance.WordWrap = false;
            // 
            // pictureDelete
            // 
            this.pictureDelete.Image = ((System.Drawing.Image)(resources.GetObject("pictureDelete.Image")));
            this.pictureDelete.Location = new System.Drawing.Point(93, 78);
            this.pictureDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureDelete.Name = "pictureDelete";
            this.pictureDelete.Size = new System.Drawing.Size(36, 28);
            this.pictureDelete.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureDelete.TabIndex = 35;
            this.pictureDelete.TabStop = false;
            this.pictureDelete.Click += new System.EventHandler(this.pictureDelete_Click);
            // 
            // pictureAdd
            // 
            this.pictureAdd.Image = ((System.Drawing.Image)(resources.GetObject("pictureAdd.Image")));
            this.pictureAdd.Location = new System.Drawing.Point(14, 78);
            this.pictureAdd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureAdd.Name = "pictureAdd";
            this.pictureAdd.Size = new System.Drawing.Size(36, 28);
            this.pictureAdd.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureAdd.TabIndex = 33;
            this.pictureAdd.TabStop = false;
            this.pictureAdd.Tag = "Add Client";
            this.pictureAdd.Click += new System.EventHandler(this.pictureAdd_Click);
            // 
            // cmbLockbox
            // 
            this.cmbLockbox.FormattingEnabled = true;
            this.cmbLockbox.Items.AddRange(new object[] {
            "LKBX",
            "TFBX"});
            this.cmbLockbox.Location = new System.Drawing.Point(93, 47);
            this.cmbLockbox.Name = "cmbLockbox";
            this.cmbLockbox.Size = new System.Drawing.Size(81, 24);
            this.cmbLockbox.TabIndex = 31;
            this.cmbLockbox.Text = "LKBX";
            this.cmbLockbox.SelectedIndexChanged += new System.EventHandler(this.cmbLockbox_SelectedIndexChanged);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.Lime;
            this.btnSave.Font = new System.Drawing.Font("Tahoma", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnSave.Location = new System.Drawing.Point(541, 11);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(126, 58);
            this.btnSave.TabIndex = 30;
            this.btnSave.Text = "Save Data";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnGetDeposits
            // 
            this.btnGetDeposits.BackColor = System.Drawing.Color.SandyBrown;
            this.btnGetDeposits.Font = new System.Drawing.Font("Tahoma", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnGetDeposits.Location = new System.Drawing.Point(385, 11);
            this.btnGetDeposits.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnGetDeposits.Name = "btnGetDeposits";
            this.btnGetDeposits.Size = new System.Drawing.Size(126, 58);
            this.btnGetDeposits.TabIndex = 29;
            this.btnGetDeposits.Text = "Get Deposits";
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
            this.dateTimePicker1.CloseUp += new System.EventHandler(this.dateTimePicker1_CloseUp);
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
            this.menuStrip1.Size = new System.Drawing.Size(1761, 30);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPrint,
            this.toolStripMenuItem1,
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
            this.menuPrint.Size = new System.Drawing.Size(186, 26);
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
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lockScreenDetailsToolStripMenuItem,
            this.unlockScreenDetailsToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(186, 26);
            this.toolStripMenuItem1.Text = "Screen Details";
            // 
            // lockScreenDetailsToolStripMenuItem
            // 
            this.lockScreenDetailsToolStripMenuItem.Name = "lockScreenDetailsToolStripMenuItem";
            this.lockScreenDetailsToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.lockScreenDetailsToolStripMenuItem.Text = "Lock Screen Details";
            this.lockScreenDetailsToolStripMenuItem.Click += new System.EventHandler(this.lockScreenDetailsToolStripMenuItem_Click);
            // 
            // unlockScreenDetailsToolStripMenuItem
            // 
            this.unlockScreenDetailsToolStripMenuItem.Name = "unlockScreenDetailsToolStripMenuItem";
            this.unlockScreenDetailsToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.unlockScreenDetailsToolStripMenuItem.Text = "Unlock Screen Details";
            this.unlockScreenDetailsToolStripMenuItem.Click += new System.EventHandler(this.unLockScreenDetailsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(186, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // LockBoxDeposits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1761, 431);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "LockBoxDeposits";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lockbox Deposit Report";
            this.Load += new System.EventHandler(this.LockBoxDeposits_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDelete)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureAdd)).EndInit();
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
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn48;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn57;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn TDA2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn IDA3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn NOX4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn Transfers5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn balance6;
        private System.Windows.Forms.Button btnGetDeposits;
        private System.Windows.Forms.Button btnSave;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private System.Windows.Forms.ComboBox cmbLockbox;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private System.Windows.Forms.PictureBox pictureAdd;
        private System.Windows.Forms.PictureBox pictureDelete;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBeginningBalance;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtScale;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private System.Windows.Forms.Button btnSelectColumns;
        private System.Windows.Forms.ComboBox cmbSelectColumns;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem lockScreenDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unlockScreenDetailsToolStripMenuItem;
    }
}