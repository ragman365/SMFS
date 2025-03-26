using MyXtraGrid;

namespace SMFS
{
    partial class FuneralActivityReport
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
            DevExpress.Utils.ContextButton contextButton1 = new DevExpress.Utils.ContextButton();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FuneralActivityReport));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.Num = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn16 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.gridColumn15 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridColumn16 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.contractNumber = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridColumn12 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridColumn14 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.repButton = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.repositoryItemPictureEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
            this.repPaidCheckEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnSendEmail = new System.Windows.Forms.Button();
            this.chkComboLocation = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.lblLocation = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtContract = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbSearch = new System.Windows.Forms.ComboBox();
            this.chkPageBreaks = new System.Windows.Forms.CheckBox();
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
            this.miscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNewContactApptsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repPaidCheckEdit)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkComboLocation.Properties)).BeginInit();
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
            this.panelAll.Size = new System.Drawing.Size(1700, 389);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 76);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1700, 313);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "Caramel";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repButton,
            this.repCheckEdit1,
            this.repositoryItemPictureEdit1,
            this.repositoryItemMemoEdit1,
            this.repPaidCheckEdit});
            this.dgv.Size = new System.Drawing.Size(1700, 313);
            this.dgv.TabIndex = 4;
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
            this.gridMain.Appearance.Row.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridMain.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.Row.Options.UseBackColor = true;
            this.gridMain.Appearance.Row.Options.UseFont = true;
            this.gridMain.Appearance.Row.Options.UseForeColor = true;
            this.gridMain.Appearance.Row.Options.UseTextOptions = true;
            this.gridMain.Appearance.Row.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
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
            this.gridMain.AppearancePrint.Row.Options.UseTextOptions = true;
            this.gridMain.AppearancePrint.Row.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand3,
            this.gridBand1,
            this.gridBand2});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.ColumnPanelRowHeight = 6;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.Num,
            this.bandedGridColumn1,
            this.bandedGridColumn16,
            this.gridColumn15,
            this.gridColumn16,
            this.gridColumn14,
            this.contractNumber,
            this.bandedGridColumn3,
            this.bandedGridColumn8,
            this.gridColumn12});
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.AutoPopulateColumns = false;
            this.gridMain.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsPrint.PrintHeader = false;
            this.gridMain.OptionsSelection.MultiSelect = true;
            this.gridMain.OptionsView.BestFitMaxRowCount = 5;
            this.gridMain.OptionsView.BestFitMode = DevExpress.XtraGrid.Views.Grid.GridBestFitMode.Full;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowColumnHeaders = false;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.RowHeight = 1;
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            this.gridMain.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gridMain_RowCellStyle_1);
            this.gridMain.CalcRowHeight += new DevExpress.XtraGrid.Views.Grid.RowHeightEventHandler(this.gridMain_CalcRowHeight);
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
            // 
            // gridBand3
            // 
            this.gridBand3.Caption = "gridBand3";
            this.gridBand3.Columns.Add(this.Num);
            this.gridBand3.Columns.Add(this.bandedGridColumn16);
            this.gridBand3.Columns.Add(this.gridColumn15);
            this.gridBand3.Columns.Add(this.gridColumn16);
            this.gridBand3.Columns.Add(this.contractNumber);
            this.gridBand3.Columns.Add(this.bandedGridColumn3);
            this.gridBand3.Columns.Add(this.bandedGridColumn8);
            this.gridBand3.Columns.Add(this.gridColumn12);
            this.gridBand3.Columns.Add(this.gridColumn14);
            this.gridBand3.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            this.gridBand3.MinWidth = 48;
            this.gridBand3.Name = "gridBand3";
            this.gridBand3.OptionsBand.AllowSize = false;
            this.gridBand3.OptionsBand.FixedWidth = true;
            this.gridBand3.VisibleIndex = 0;
            this.gridBand3.Width = 1250;
            // 
            // Num
            // 
            this.Num.AppearanceHeader.Options.UseTextOptions = true;
            this.Num.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
            this.Num.Caption = "Num";
            this.Num.FieldName = "num";
            this.Num.MinWidth = 101;
            this.Num.Name = "Num";
            this.Num.OptionsColumn.AllowEdit = false;
            this.Num.OptionsColumn.FixedWidth = true;
            this.Num.Width = 101;
            // 
            // bandedGridColumn16
            // 
            this.bandedGridColumn16.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bandedGridColumn16.AppearanceCell.Options.UseFont = true;
            this.bandedGridColumn16.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn16.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn16.Caption = "C1";
            this.bandedGridColumn16.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn16.FieldName = "C1";
            this.bandedGridColumn16.MinWidth = 101;
            this.bandedGridColumn16.Name = "bandedGridColumn16";
            this.bandedGridColumn16.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn16.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn16.Visible = true;
            this.bandedGridColumn16.Width = 250;
            // 
            // repositoryItemMemoEdit1
            // 
            this.repositoryItemMemoEdit1.Appearance.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.repositoryItemMemoEdit1.Appearance.Options.UseFont = true;
            this.repositoryItemMemoEdit1.Appearance.Options.UseTextOptions = true;
            this.repositoryItemMemoEdit1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.repositoryItemMemoEdit1.LinesCount = 5;
            this.repositoryItemMemoEdit1.MaxLength = 5;
            this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
            // 
            // gridColumn15
            // 
            this.gridColumn15.Caption = "Loc";
            this.gridColumn15.FieldName = "loc";
            this.gridColumn15.MinWidth = 101;
            this.gridColumn15.Name = "gridColumn15";
            this.gridColumn15.OptionsColumn.AllowEdit = false;
            this.gridColumn15.OptionsColumn.FixedWidth = true;
            this.gridColumn15.Width = 125;
            // 
            // gridColumn16
            // 
            this.gridColumn16.Caption = "Location Name";
            this.gridColumn16.FieldName = "Location Name";
            this.gridColumn16.MinWidth = 101;
            this.gridColumn16.Name = "gridColumn16";
            this.gridColumn16.OptionsColumn.AllowEdit = false;
            this.gridColumn16.OptionsColumn.FixedWidth = true;
            this.gridColumn16.Width = 252;
            // 
            // contractNumber
            // 
            this.contractNumber.Caption = "Service ID";
            this.contractNumber.FieldName = "serviceId";
            this.contractNumber.MinWidth = 101;
            this.contractNumber.Name = "contractNumber";
            this.contractNumber.OptionsColumn.AllowEdit = false;
            this.contractNumber.OptionsColumn.FixedWidth = true;
            this.contractNumber.Width = 199;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bandedGridColumn3.AppearanceCell.Options.UseFont = true;
            this.bandedGridColumn3.Caption = "C2";
            this.bandedGridColumn3.FieldName = "C2";
            this.bandedGridColumn3.MinWidth = 101;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 300;
            // 
            // bandedGridColumn8
            // 
            this.bandedGridColumn8.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bandedGridColumn8.AppearanceCell.Options.UseFont = true;
            this.bandedGridColumn8.Caption = "C3";
            this.bandedGridColumn8.FieldName = "C3";
            this.bandedGridColumn8.MinWidth = 101;
            this.bandedGridColumn8.Name = "bandedGridColumn8";
            this.bandedGridColumn8.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn8.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn8.Visible = true;
            this.bandedGridColumn8.Width = 400;
            // 
            // gridColumn12
            // 
            this.gridColumn12.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridColumn12.AppearanceCell.Options.UseFont = true;
            this.gridColumn12.Caption = "C4";
            this.gridColumn12.FieldName = "C4";
            this.gridColumn12.MinWidth = 101;
            this.gridColumn12.Name = "gridColumn12";
            this.gridColumn12.OptionsColumn.AllowEdit = false;
            this.gridColumn12.OptionsColumn.FixedWidth = true;
            this.gridColumn12.Visible = true;
            this.gridColumn12.Width = 300;
            // 
            // gridColumn14
            // 
            this.gridColumn14.Caption = "Contract Number";
            this.gridColumn14.FieldName = "contractNumber";
            this.gridColumn14.MinWidth = 101;
            this.gridColumn14.Name = "gridColumn14";
            this.gridColumn14.OptionsColumn.AllowEdit = false;
            this.gridColumn14.OptionsColumn.FixedWidth = true;
            this.gridColumn14.Width = 226;
            // 
            // gridBand1
            // 
            this.gridBand1.AutoFillDown = false;
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Columns.Add(this.bandedGridColumn1);
            this.gridBand1.MinWidth = 252;
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.OptionsBand.AllowSize = false;
            this.gridBand1.OptionsBand.FixedWidth = true;
            this.gridBand1.Visible = false;
            this.gridBand1.VisibleIndex = -1;
            this.gridBand1.Width = 252;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "record";
            this.bandedGridColumn1.FieldName = "record";
            this.bandedGridColumn1.MinWidth = 138;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Width = 47900;
            // 
            // gridBand2
            // 
            this.gridBand2.Caption = "gridBand2";
            this.gridBand2.MinWidth = 4849;
            this.gridBand2.Name = "gridBand2";
            this.gridBand2.Visible = false;
            this.gridBand2.VisibleIndex = -1;
            this.gridBand2.Width = 4849;
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
            contextButton1.Caption = "contextButton1";
            contextButton1.Id = new System.Guid("7020ad83-ad48-4296-8d4d-5a08a0a3f273");
            contextButton1.Name = "contextButton1";
            this.repositoryItemPictureEdit1.ContextButtons.Add(contextButton1);
            this.repositoryItemPictureEdit1.Name = "repositoryItemPictureEdit1";
            // 
            // repPaidCheckEdit
            // 
            this.repPaidCheckEdit.AutoHeight = false;
            this.repPaidCheckEdit.Name = "repPaidCheckEdit";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.Beige;
            this.panelTop.Controls.Add(this.btnSendEmail);
            this.panelTop.Controls.Add(this.chkComboLocation);
            this.panelTop.Controls.Add(this.lblLocation);
            this.panelTop.Controls.Add(this.label4);
            this.panelTop.Controls.Add(this.txtContract);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.cmbSearch);
            this.panelTop.Controls.Add(this.chkPageBreaks);
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
            this.panelTop.Size = new System.Drawing.Size(1700, 76);
            this.panelTop.TabIndex = 1;
            // 
            // btnSendEmail
            // 
            this.btnSendEmail.Location = new System.Drawing.Point(1502, 12);
            this.btnSendEmail.Name = "btnSendEmail";
            this.btnSendEmail.Size = new System.Drawing.Size(140, 28);
            this.btnSendEmail.TabIndex = 142;
            this.btnSendEmail.Text = "Send Email";
            this.btnSendEmail.UseVisualStyleBackColor = true;
            this.btnSendEmail.Click += new System.EventHandler(this.btnSendEmail_Click);
            // 
            // chkComboLocation
            // 
            this.chkComboLocation.EditValue = "";
            this.chkComboLocation.Location = new System.Drawing.Point(159, 44);
            this.chkComboLocation.Margin = new System.Windows.Forms.Padding(4);
            this.chkComboLocation.Name = "chkComboLocation";
            this.chkComboLocation.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.chkComboLocation.Properties.DisplayMember = "LocationCode";
            this.chkComboLocation.Properties.SeparatorChar = '|';
            this.chkComboLocation.Size = new System.Drawing.Size(157, 22);
            this.chkComboLocation.TabIndex = 140;
            // 
            // lblLocation
            // 
            this.lblLocation.AutoSize = true;
            this.lblLocation.Location = new System.Drawing.Point(88, 46);
            this.lblLocation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(69, 17);
            this.lblLocation.TabIndex = 141;
            this.lblLocation.Text = "Location :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1414, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 17);
            this.label4.TabIndex = 47;
            this.label4.Text = "ServiceId";
            // 
            // txtContract
            // 
            this.txtContract.Location = new System.Drawing.Point(1312, 16);
            this.txtContract.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtContract.Name = "txtContract";
            this.txtContract.Size = new System.Drawing.Size(96, 23);
            this.txtContract.TabIndex = 46;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(851, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 17);
            this.label3.TabIndex = 41;
            this.label3.Text = "Search By :";
            // 
            // cmbSearch
            // 
            this.cmbSearch.FormattingEnabled = true;
            this.cmbSearch.Items.AddRange(new object[] {
            "Deceased Date",
            "Create Date",
            "Service Date"});
            this.cmbSearch.Location = new System.Drawing.Point(936, 15);
            this.cmbSearch.Name = "cmbSearch";
            this.cmbSearch.Size = new System.Drawing.Size(139, 24);
            this.cmbSearch.TabIndex = 40;
            this.cmbSearch.Text = "Deceased Date";
            // 
            // chkPageBreaks
            // 
            this.chkPageBreaks.AutoSize = true;
            this.chkPageBreaks.Location = new System.Drawing.Point(936, 46);
            this.chkPageBreaks.Name = "chkPageBreaks";
            this.chkPageBreaks.Size = new System.Drawing.Size(176, 21);
            this.chkPageBreaks.TabIndex = 39;
            this.chkPageBreaks.Text = "Page Break at Locations";
            this.chkPageBreaks.UseVisualStyleBackColor = true;
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
            this.btnRun.BackColor = System.Drawing.Color.DarkKhaki;
            this.btnRun.Font = new System.Drawing.Font("Tahoma", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.btnRun.Location = new System.Drawing.Point(657, 9);
            this.btnRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(126, 58);
            this.btnRun.TabIndex = 28;
            this.btnRun.Text = "Run";
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
            this.btnRight.BackColor = System.Drawing.Color.DarkKhaki;
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
            this.btnLeft.BackColor = System.Drawing.Color.DarkKhaki;
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
            this.fileToolStripMenuItem,
            this.miscToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1700, 30);
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
            // miscToolStripMenuItem
            // 
            this.miscToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewContactApptsToolStripMenuItem});
            this.miscToolStripMenuItem.Name = "miscToolStripMenuItem";
            this.miscToolStripMenuItem.Size = new System.Drawing.Size(53, 26);
            this.miscToolStripMenuItem.Text = "Misc";
            // 
            // createNewContactApptsToolStripMenuItem
            // 
            this.createNewContactApptsToolStripMenuItem.Name = "createNewContactApptsToolStripMenuItem";
            this.createNewContactApptsToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.createNewContactApptsToolStripMenuItem.Text = "Create New Contact Appts";
            this.createNewContactApptsToolStripMenuItem.Click += new System.EventHandler(this.createNewContactApptsToolStripMenuItem_Click);
            // 
            // FuneralActivityReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1700, 419);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FuneralActivityReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Daily Schedule of Services Report";
            this.Load += new System.EventHandler(this.FuneralActivityReport_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repPaidCheckEdit)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkComboLocation.Properties)).EndInit();
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
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtScale;
        private System.Windows.Forms.CheckBox chkPageBreaks;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbSearch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtContract;
        private DevExpress.XtraEditors.CheckedComboBoxEdit chkComboLocation;
        private System.Windows.Forms.Label lblLocation;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn Num;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repPaidCheckEdit;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn gridColumn15;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn gridColumn16;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn contractNumber;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn8;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn gridColumn14;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn gridColumn12;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn16;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repCheckEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repButton;
        private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryItemPictureEdit1;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand3;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand2;
        private System.Windows.Forms.Button btnSendEmail;
        private System.Windows.Forms.ToolStripMenuItem miscToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createNewContactApptsToolStripMenuItem;
    }
}