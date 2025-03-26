using MyXtraGrid;

namespace SMFS
{
    partial class BankDetailsByLocation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BankDetailsByLocation));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv2 = new DevExpress.XtraGrid.GridControl();
            this.gridMain2 = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemDateEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.repositoryItemDateEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblType = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.btnExitLocation = new System.Windows.Forms.Button();
            this.chkGroup = new System.Windows.Forms.CheckBox();
            this.cmbLocation = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1.CalendarTimeProperties)).BeginInit();
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
            this.panelAll.Size = new System.Drawing.Size(1761, 401);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv2);
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 62);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1761, 339);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv2
            // 
            this.dgv2.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv2.Location = new System.Drawing.Point(1051, 4);
            this.dgv2.LookAndFeel.SkinName = "iMaginary";
            this.dgv2.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv2.MainView = this.gridMain2;
            this.dgv2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv2.Name = "dgv2";
            this.dgv2.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemDateEdit2});
            this.dgv2.Size = new System.Drawing.Size(541, 226);
            this.dgv2.TabIndex = 9;
            this.dgv2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain2});
            // 
            // gridMain2
            // 
            this.gridMain2.Appearance.BandPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(206)))), ((int)(((byte)(164)))));
            this.gridMain2.Appearance.BandPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(206)))), ((int)(((byte)(164)))));
            this.gridMain2.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain2.Appearance.BandPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.BandPanel.Options.UseBackColor = true;
            this.gridMain2.Appearance.BandPanel.Options.UseBorderColor = true;
            this.gridMain2.Appearance.BandPanel.Options.UseFont = true;
            this.gridMain2.Appearance.BandPanel.Options.UseForeColor = true;
            this.gridMain2.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.gridMain2.Appearance.BandPanelBackground.Options.UseBackColor = true;
            this.gridMain2.Appearance.ColumnFilterButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.ColumnFilterButton.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain2.Appearance.ColumnFilterButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.ColumnFilterButton.ForeColor = System.Drawing.Color.Gray;
            this.gridMain2.Appearance.ColumnFilterButton.Options.UseBackColor = true;
            this.gridMain2.Appearance.ColumnFilterButton.Options.UseBorderColor = true;
            this.gridMain2.Appearance.ColumnFilterButton.Options.UseForeColor = true;
            this.gridMain2.Appearance.ColumnFilterButtonActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain2.Appearance.ColumnFilterButtonActive.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(241)))), ((int)(((byte)(209)))));
            this.gridMain2.Appearance.ColumnFilterButtonActive.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain2.Appearance.ColumnFilterButtonActive.ForeColor = System.Drawing.Color.Blue;
            this.gridMain2.Appearance.ColumnFilterButtonActive.Options.UseBackColor = true;
            this.gridMain2.Appearance.ColumnFilterButtonActive.Options.UseBorderColor = true;
            this.gridMain2.Appearance.ColumnFilterButtonActive.Options.UseForeColor = true;
            this.gridMain2.Appearance.Empty.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain2.Appearance.Empty.Options.UseBackColor = true;
            this.gridMain2.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.EvenRow.BackColor2 = System.Drawing.Color.GhostWhite;
            this.gridMain2.Appearance.EvenRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.EvenRow.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal;
            this.gridMain2.Appearance.EvenRow.Options.UseBackColor = true;
            this.gridMain2.Appearance.EvenRow.Options.UseForeColor = true;
            this.gridMain2.Appearance.FilterCloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain2.Appearance.FilterCloseButton.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.gridMain2.Appearance.FilterCloseButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain2.Appearance.FilterCloseButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.FilterCloseButton.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal;
            this.gridMain2.Appearance.FilterCloseButton.Options.UseBackColor = true;
            this.gridMain2.Appearance.FilterCloseButton.Options.UseBorderColor = true;
            this.gridMain2.Appearance.FilterCloseButton.Options.UseForeColor = true;
            this.gridMain2.Appearance.FilterPanel.BackColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.FilterPanel.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain2.Appearance.FilterPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain2.Appearance.FilterPanel.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal;
            this.gridMain2.Appearance.FilterPanel.Options.UseBackColor = true;
            this.gridMain2.Appearance.FilterPanel.Options.UseForeColor = true;
            this.gridMain2.Appearance.FixedLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(96)))), ((int)(((byte)(0)))));
            this.gridMain2.Appearance.FixedLine.Options.UseBackColor = true;
            this.gridMain2.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(166)))), ((int)(((byte)(70)))));
            this.gridMain2.Appearance.FocusedRow.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(216)))), ((int)(((byte)(120)))));
            this.gridMain2.Appearance.FocusedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridMain2.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridMain2.Appearance.FooterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.FooterPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.FooterPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.FooterPanel.Options.UseBackColor = true;
            this.gridMain2.Appearance.FooterPanel.Options.UseBorderColor = true;
            this.gridMain2.Appearance.FooterPanel.Options.UseForeColor = true;
            this.gridMain2.Appearance.GroupButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.GroupButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.GroupButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.GroupButton.Options.UseBackColor = true;
            this.gridMain2.Appearance.GroupButton.Options.UseBorderColor = true;
            this.gridMain2.Appearance.GroupButton.Options.UseForeColor = true;
            this.gridMain2.Appearance.GroupFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(226)))), ((int)(((byte)(184)))));
            this.gridMain2.Appearance.GroupFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(226)))), ((int)(((byte)(184)))));
            this.gridMain2.Appearance.GroupFooter.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.GroupFooter.Options.UseBackColor = true;
            this.gridMain2.Appearance.GroupFooter.Options.UseBorderColor = true;
            this.gridMain2.Appearance.GroupFooter.Options.UseForeColor = true;
            this.gridMain2.Appearance.GroupPanel.BackColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.GroupPanel.BackColor2 = System.Drawing.Color.White;
            this.gridMain2.Appearance.GroupPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain2.Appearance.GroupPanel.ForeColor = System.Drawing.Color.White;
            this.gridMain2.Appearance.GroupPanel.Options.UseBackColor = true;
            this.gridMain2.Appearance.GroupPanel.Options.UseFont = true;
            this.gridMain2.Appearance.GroupPanel.Options.UseForeColor = true;
            this.gridMain2.Appearance.GroupRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(166)))), ((int)(((byte)(70)))));
            this.gridMain2.Appearance.GroupRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(236)))), ((int)(((byte)(215)))));
            this.gridMain2.Appearance.GroupRow.Options.UseBackColor = true;
            this.gridMain2.Appearance.GroupRow.Options.UseForeColor = true;
            this.gridMain2.Appearance.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.HeaderPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.HeaderPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain2.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.HeaderPanel.Options.UseBackColor = true;
            this.gridMain2.Appearance.HeaderPanel.Options.UseBorderColor = true;
            this.gridMain2.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridMain2.Appearance.HeaderPanel.Options.UseForeColor = true;
            this.gridMain2.Appearance.HeaderPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.gridMain2.Appearance.HeaderPanelBackground.Options.UseBackColor = true;
            this.gridMain2.Appearance.HideSelectionRow.BackColor = System.Drawing.Color.Gray;
            this.gridMain2.Appearance.HideSelectionRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(208)))), ((int)(((byte)(200)))));
            this.gridMain2.Appearance.HideSelectionRow.Options.UseBackColor = true;
            this.gridMain2.Appearance.HideSelectionRow.Options.UseForeColor = true;
            this.gridMain2.Appearance.HorzLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.HorzLine.Options.UseBackColor = true;
            this.gridMain2.Appearance.OddRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(236)))), ((int)(((byte)(215)))));
            this.gridMain2.Appearance.OddRow.BackColor2 = System.Drawing.Color.White;
            this.gridMain2.Appearance.OddRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.OddRow.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.BackwardDiagonal;
            this.gridMain2.Appearance.OddRow.Options.UseBackColor = true;
            this.gridMain2.Appearance.OddRow.Options.UseForeColor = true;
            this.gridMain2.Appearance.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(234)))));
            this.gridMain2.Appearance.Preview.BackColor2 = System.Drawing.Color.White;
            this.gridMain2.Appearance.Preview.ForeColor = System.Drawing.Color.Maroon;
            this.gridMain2.Appearance.Preview.Options.UseBackColor = true;
            this.gridMain2.Appearance.Preview.Options.UseForeColor = true;
            this.gridMain2.Appearance.Row.BackColor = System.Drawing.Color.White;
            this.gridMain2.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.Row.Options.UseBackColor = true;
            this.gridMain2.Appearance.Row.Options.UseForeColor = true;
            this.gridMain2.Appearance.RowSeparator.BackColor = System.Drawing.Color.White;
            this.gridMain2.Appearance.RowSeparator.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(194)))));
            this.gridMain2.Appearance.RowSeparator.Options.UseBackColor = true;
            this.gridMain2.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(176)))), ((int)(((byte)(80)))));
            this.gridMain2.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain2.Appearance.SelectedRow.Options.UseBackColor = true;
            this.gridMain2.Appearance.SelectedRow.Options.UseForeColor = true;
            this.gridMain2.Appearance.VertLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(216)))), ((int)(((byte)(174)))));
            this.gridMain2.Appearance.VertLine.Options.UseBackColor = true;
            this.gridMain2.AppearancePrint.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain2.AppearancePrint.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain2.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand1});
            this.gridMain2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain2.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn1,
            this.bandedGridColumn2,
            this.bandedGridColumn5,
            this.bandedGridColumn6,
            this.bandedGridColumn3,
            this.bandedGridColumn4});
            this.gridMain2.DetailHeight = 431;
            this.gridMain2.GridControl = this.dgv2;
            this.gridMain2.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "Payment", this.bandedGridColumn3, "${0:0,0.00}"),
            new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "Debit", this.bandedGridColumn4, "${0:0,0.00}")});
            this.gridMain2.Name = "gridMain2";
            this.gridMain2.OptionsBehavior.AutoPopulateColumns = false;
            this.gridMain2.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain2.OptionsPrint.PrintBandHeader = false;
            this.gridMain2.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain2.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain2.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain2.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleAlways;
            this.gridMain2.OptionsView.ShowBands = false;
            this.gridMain2.OptionsView.ShowFooter = true;
            this.gridMain2.OptionsView.ShowGroupPanel = false;
            this.gridMain2.PaintStyleName = "Style3D";
            this.gridMain2.CustomRowFilter += new DevExpress.XtraGrid.Views.Base.RowFilterEventHandler(this.gridMain2_CustomRowFilter);
            this.gridMain2.DoubleClick += new System.EventHandler(this.gridMain2_DoubleClick);
            // 
            // gridBand1
            // 
            this.gridBand1.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand1.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.gridBand1.Caption = "Current Information";
            this.gridBand1.Columns.Add(this.bandedGridColumn1);
            this.gridBand1.Columns.Add(this.bandedGridColumn2);
            this.gridBand1.Columns.Add(this.bandedGridColumn6);
            this.gridBand1.Columns.Add(this.bandedGridColumn5);
            this.gridBand1.Columns.Add(this.bandedGridColumn3);
            this.gridBand1.Columns.Add(this.bandedGridColumn4);
            this.gridBand1.MinWidth = 56;
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 809;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "Num";
            this.bandedGridColumn1.FieldName = "num";
            this.bandedGridColumn1.MinWidth = 40;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Visible = true;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.Caption = "Date";
            this.bandedGridColumn2.FieldName = "Date";
            this.bandedGridColumn2.MinWidth = 40;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 149;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.Caption = "What";
            this.bandedGridColumn6.FieldName = "What";
            this.bandedGridColumn6.MinWidth = 29;
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Visible = true;
            this.bandedGridColumn6.Width = 89;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.Caption = "Contract";
            this.bandedGridColumn5.FieldName = "Contract";
            this.bandedGridColumn5.MinWidth = 34;
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 128;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "Payment";
            this.bandedGridColumn3.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn3.FieldName = "Payment";
            this.bandedGridColumn3.MinWidth = 40;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 183;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "Debit";
            this.bandedGridColumn4.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn4.FieldName = "Debit";
            this.bandedGridColumn4.MinWidth = 40;
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            this.bandedGridColumn4.Width = 185;
            // 
            // repositoryItemDateEdit2
            // 
            this.repositoryItemDateEdit2.AutoHeight = false;
            this.repositoryItemDateEdit2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemDateEdit2.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemDateEdit2.Name = "repositoryItemDateEdit2";
            // 
            // dgv
            // 
            this.dgv.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Location = new System.Drawing.Point(14, 8);
            this.dgv.LookAndFeel.SkinName = "iMaginary";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemDateEdit1});
            this.dgv.Size = new System.Drawing.Size(541, 226);
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
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleAlways;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            this.gridMain.CustomDrawGroupRow += new DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventHandler(this.gridMain_CustomDrawGroupRow);
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
            this.gridBand5.MinWidth = 35;
            this.gridBand5.Name = "gridBand5";
            this.gridBand5.VisibleIndex = 0;
            this.gridBand5.Width = 92;
            // 
            // repositoryItemDateEdit1
            // 
            this.repositoryItemDateEdit1.AutoHeight = false;
            this.repositoryItemDateEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemDateEdit1.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemDateEdit1.Name = "repositoryItemDateEdit1";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.PeachPuff;
            this.panelTop.Controls.Add(this.lblType);
            this.panelTop.Controls.Add(this.cmbType);
            this.panelTop.Controls.Add(this.btnExitLocation);
            this.panelTop.Controls.Add(this.chkGroup);
            this.panelTop.Controls.Add(this.cmbLocation);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.txtScale);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1761, 62);
            this.panelTop.TabIndex = 1;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(881, 23);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(39, 17);
            this.lblType.TabIndex = 50;
            this.lblType.Text = "Type";
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "All",
            "Trust",
            "Ins",
            "Funeral"});
            this.cmbType.Location = new System.Drawing.Point(759, 19);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(119, 24);
            this.cmbType.TabIndex = 49;
            this.cmbType.Text = "All";
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // btnExitLocation
            // 
            this.btnExitLocation.BackColor = System.Drawing.Color.LightSalmon;
            this.btnExitLocation.Location = new System.Drawing.Point(615, 5);
            this.btnExitLocation.Name = "btnExitLocation";
            this.btnExitLocation.Size = new System.Drawing.Size(93, 52);
            this.btnExitLocation.TabIndex = 48;
            this.btnExitLocation.Text = "Exit Location";
            this.btnExitLocation.UseVisualStyleBackColor = false;
            this.btnExitLocation.Click += new System.EventHandler(this.btnExitLocation_Click);
            // 
            // chkGroup
            // 
            this.chkGroup.AutoSize = true;
            this.chkGroup.Location = new System.Drawing.Point(436, 17);
            this.chkGroup.Name = "chkGroup";
            this.chkGroup.Size = new System.Drawing.Size(121, 21);
            this.chkGroup.TabIndex = 47;
            this.chkGroup.Text = "Group by Date";
            this.chkGroup.UseVisualStyleBackColor = true;
            this.chkGroup.CheckedChanged += new System.EventHandler(this.chkGroup_CheckedChanged);
            // 
            // cmbLocation
            // 
            this.cmbLocation.FormattingEnabled = true;
            this.cmbLocation.Location = new System.Drawing.Point(72, 15);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(243, 24);
            this.cmbLocation.TabIndex = 46;
            this.cmbLocation.SelectedIndexChanged += new System.EventHandler(this.cmbLocation_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1154, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 17);
            this.label3.TabIndex = 45;
            this.label3.Text = "Scale";
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(1087, 19);
            this.txtScale.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(63, 23);
            this.txtScale.TabIndex = 44;
            this.txtScale.Text = "90.00";
            this.txtScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtScale.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtScale_KeyUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(323, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 17);
            this.label2.TabIndex = 39;
            this.label2.Text = "Locations";
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
            // BankDetailsByLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1761, 431);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "BankDetailsByLocation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bank Details by Location";
            this.Load += new System.EventHandler(this.BankDetailsByLocation_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemDateEdit1)).EndInit();
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
        private System.Windows.Forms.PictureBox pictureBox1;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repositoryItemDateEdit1;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtScale;
        private System.Windows.Forms.ComboBox cmbLocation;
        private DevExpress.XtraGrid.GridControl dgv2;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain2;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repositoryItemDateEdit2;
        private System.Windows.Forms.CheckBox chkGroup;
        private System.Windows.Forms.Button btnExitLocation;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cmbType;
    }
}