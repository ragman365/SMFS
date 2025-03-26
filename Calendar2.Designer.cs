namespace SMFS
{
    partial class Calendar2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Calendar2));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand4 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn70 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemMemoEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn71 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridView5 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnPrintAll = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.btnSaveAll = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hRGroupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView5)).BeginInit();
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
            this.panelAll.Size = new System.Drawing.Size(1757, 901);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 54);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1757, 847);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "Foggy";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemMemoEdit1});
            this.dgv.Size = new System.Drawing.Size(1757, 847);
            this.dgv.TabIndex = 12;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain,
            this.gridView5});
            // 
            // gridMain
            // 
            this.gridMain.Appearance.BandPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.BandPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridMain.Appearance.BandPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.BandPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.BandPanel.Options.UseFont = true;
            this.gridMain.Appearance.BandPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.BandPanelBackground.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.BandPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.ColumnFilterButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.ColumnFilterButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButton.Options.UseForeColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(226)))), ((int)(((byte)(216)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(226)))), ((int)(((byte)(216)))));
            this.gridMain.Appearance.ColumnFilterButtonActive.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBackColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseBorderColor = true;
            this.gridMain.Appearance.ColumnFilterButtonActive.Options.UseForeColor = true;
            this.gridMain.Appearance.Empty.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.Empty.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.Empty.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(227)))), ((int)(((byte)(245)))));
            this.gridMain.Appearance.EvenRow.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(227)))), ((int)(((byte)(245)))));
            this.gridMain.Appearance.EvenRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
            this.gridMain.Appearance.EvenRow.Options.UseBorderColor = true;
            this.gridMain.Appearance.EvenRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterCloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.FilterCloseButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.FilterCloseButton.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.FilterCloseButton.Options.UseForeColor = true;
            this.gridMain.Appearance.FilterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.FilterPanel.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.FilterPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FilterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FilterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.FixedLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(136)))), ((int)(((byte)(122)))));
            this.gridMain.Appearance.FixedLine.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedCell.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FocusedCell.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FocusedCell.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedCell.Options.UseForeColor = true;
            this.gridMain.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(171)))), ((int)(((byte)(177)))));
            this.gridMain.Appearance.FocusedRow.ForeColor = System.Drawing.Color.White;
            this.gridMain.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.FooterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.FooterPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(202)))), ((int)(((byte)(221)))), ((int)(((byte)(208)))));
            this.gridMain.Appearance.FooterPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FooterPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.FooterPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(209)))), ((int)(((byte)(188)))));
            this.gridMain.Appearance.GroupButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(209)))), ((int)(((byte)(188)))));
            this.gridMain.Appearance.GroupButton.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupButton.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(234)))), ((int)(((byte)(221)))));
            this.gridMain.Appearance.GroupFooter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(234)))), ((int)(((byte)(221)))));
            this.gridMain.Appearance.GroupFooter.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupFooter.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupFooter.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.GroupPanel.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.GroupPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.GroupRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(234)))), ((int)(((byte)(221)))));
            this.gridMain.Appearance.GroupRow.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(234)))), ((int)(((byte)(221)))));
            this.gridMain.Appearance.GroupRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.GroupRow.Options.UseBackColor = true;
            this.gridMain.Appearance.GroupRow.Options.UseBorderColor = true;
            this.gridMain.Appearance.GroupRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(226)))), ((int)(((byte)(216)))));
            this.gridMain.Appearance.HeaderPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(226)))), ((int)(((byte)(216)))));
            this.gridMain.Appearance.HeaderPanel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridMain.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.HeaderPanel.Options.UseBackColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseBorderColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.HeaderPanelBackground.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.HeaderPanelBackground.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(211)))), ((int)(((byte)(215)))));
            this.gridMain.Appearance.HideSelectionRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(130)))), ((int)(((byte)(134)))));
            this.gridMain.Appearance.HideSelectionRow.Options.UseBackColor = true;
            this.gridMain.Appearance.HideSelectionRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HorzLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(197)))), ((int)(((byte)(180)))));
            this.gridMain.Appearance.HorzLine.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(136)))), ((int)(((byte)(122)))));
            this.gridMain.Appearance.HorzLine.Options.UseBackColor = true;
            this.gridMain.Appearance.HorzLine.Options.UseBorderColor = true;
            this.gridMain.Appearance.OddRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.OddRow.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.OddRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            this.gridMain.Appearance.OddRow.Options.UseBorderColor = true;
            this.gridMain.Appearance.OddRow.Options.UseForeColor = true;
            this.gridMain.Appearance.Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.gridMain.Appearance.Preview.Font = new System.Drawing.Font("Verdana", 7.5F);
            this.gridMain.Appearance.Preview.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.Preview.Options.UseBackColor = true;
            this.gridMain.Appearance.Preview.Options.UseFont = true;
            this.gridMain.Appearance.Preview.Options.UseForeColor = true;
            this.gridMain.Appearance.Row.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.RowSeparator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridMain.Appearance.RowSeparator.BackColor2 = System.Drawing.Color.White;
            this.gridMain.Appearance.RowSeparator.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Transparent;
            this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.SelectedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.TopNewRow.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.VertLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(197)))), ((int)(((byte)(180)))));
            this.gridMain.Appearance.VertLine.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(136)))), ((int)(((byte)(122)))));
            this.gridMain.Appearance.VertLine.Options.UseBackColor = true;
            this.gridMain.Appearance.VertLine.Options.UseBorderColor = true;
            this.gridMain.AppearancePrint.BandPanel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridMain.AppearancePrint.BandPanel.Options.UseFont = true;
            this.gridMain.AppearancePrint.HeaderPanel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridMain.AppearancePrint.HeaderPanel.Options.UseFont = true;
            this.gridMain.AppearancePrint.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain.AppearancePrint.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand4});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn70,
            this.bandedGridColumn71,
            this.bandedGridColumn1,
            this.bandedGridColumn2,
            this.bandedGridColumn3,
            this.bandedGridColumn4,
            this.bandedGridColumn5,
            this.bandedGridColumn6,
            this.bandedGridColumn7});
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.AutoPopulateColumns = false;
            this.gridMain.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            this.gridMain.OptionsNavigation.EnterMoveNextColumn = true;
            this.gridMain.OptionsPrint.AutoWidth = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridMain_CustomDrawCell);
            this.gridMain.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gridMain_RowCellStyle);
            this.gridMain.CalcRowHeight += new DevExpress.XtraGrid.Views.Grid.RowHeightEventHandler(this.gridMain_CalcRowHeight);
            this.gridMain.ShownEditor += new System.EventHandler(this.gridMain_ShownEditor);
            this.gridMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridMain_CellValueChanged);
            this.gridMain.CellValueChanging += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridMain_CellValueChanging);
            this.gridMain.CustomRowFilter += new DevExpress.XtraGrid.Views.Base.RowFilterEventHandler(this.gridMain_CustomRowFilter);
            this.gridMain.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridMain_CustomColumnDisplayText);
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
            this.gridMain.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridMain_ValidatingEditor);
            // 
            // gridBand4
            // 
            this.gridBand4.AppearanceHeader.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridBand4.AppearanceHeader.Options.UseFont = true;
            this.gridBand4.Caption = "gridBand1";
            this.gridBand4.Columns.Add(this.bandedGridColumn70);
            this.gridBand4.Columns.Add(this.bandedGridColumn1);
            this.gridBand4.Columns.Add(this.bandedGridColumn2);
            this.gridBand4.Columns.Add(this.bandedGridColumn3);
            this.gridBand4.Columns.Add(this.bandedGridColumn4);
            this.gridBand4.Columns.Add(this.bandedGridColumn5);
            this.gridBand4.Columns.Add(this.bandedGridColumn6);
            this.gridBand4.Columns.Add(this.bandedGridColumn7);
            this.gridBand4.Columns.Add(this.bandedGridColumn71);
            this.gridBand4.MinWidth = 30;
            this.gridBand4.Name = "gridBand4";
            this.gridBand4.VisibleIndex = 0;
            this.gridBand4.Width = 1631;
            // 
            // bandedGridColumn70
            // 
            this.bandedGridColumn70.Caption = "Num";
            this.bandedGridColumn70.FieldName = "num";
            this.bandedGridColumn70.MinWidth = 57;
            this.bandedGridColumn70.Name = "bandedGridColumn70";
            this.bandedGridColumn70.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn70.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn70.Width = 66;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.AppearanceCell.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn1.AppearanceCell.Options.UseForeColor = true;
            this.bandedGridColumn1.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn1.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.bandedGridColumn1.Caption = "Sunday";
            this.bandedGridColumn1.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn1.FieldName = "sunday";
            this.bandedGridColumn1.MinWidth = 64;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Visible = true;
            this.bandedGridColumn1.Width = 233;
            // 
            // repositoryItemMemoEdit1
            // 
            this.repositoryItemMemoEdit1.Appearance.Options.UseTextOptions = true;
            this.repositoryItemMemoEdit1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.repositoryItemMemoEdit1.Name = "repositoryItemMemoEdit1";
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.AppearanceCell.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn2.AppearanceCell.Options.UseForeColor = true;
            this.bandedGridColumn2.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn2.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.bandedGridColumn2.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn2.Caption = "Monday";
            this.bandedGridColumn2.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn2.FieldName = "monday";
            this.bandedGridColumn2.MinWidth = 64;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 233;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.AppearanceCell.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn3.AppearanceCell.Options.UseForeColor = true;
            this.bandedGridColumn3.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn3.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.bandedGridColumn3.Caption = "Tuesday";
            this.bandedGridColumn3.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn3.FieldName = "tuesday";
            this.bandedGridColumn3.MinWidth = 34;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 233;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.AppearanceCell.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn4.AppearanceCell.Options.UseForeColor = true;
            this.bandedGridColumn4.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn4.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.bandedGridColumn4.Caption = "Wednesday";
            this.bandedGridColumn4.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn4.FieldName = "wednesday";
            this.bandedGridColumn4.MinWidth = 34;
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            this.bandedGridColumn4.Width = 233;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.AppearanceCell.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn5.AppearanceCell.Options.UseForeColor = true;
            this.bandedGridColumn5.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn5.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.bandedGridColumn5.Caption = "Thursday";
            this.bandedGridColumn5.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn5.FieldName = "thursday";
            this.bandedGridColumn5.MinWidth = 34;
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 233;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.AppearanceCell.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn6.AppearanceCell.Options.UseForeColor = true;
            this.bandedGridColumn6.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn6.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.bandedGridColumn6.Caption = "Friday";
            this.bandedGridColumn6.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn6.FieldName = "friday";
            this.bandedGridColumn6.MinWidth = 34;
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Visible = true;
            this.bandedGridColumn6.Width = 233;
            // 
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.AppearanceCell.ForeColor = System.Drawing.Color.Black;
            this.bandedGridColumn7.AppearanceCell.Options.UseForeColor = true;
            this.bandedGridColumn7.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn7.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.bandedGridColumn7.Caption = "Saturday";
            this.bandedGridColumn7.ColumnEdit = this.repositoryItemMemoEdit1;
            this.bandedGridColumn7.FieldName = "saturday";
            this.bandedGridColumn7.MinWidth = 34;
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Visible = true;
            this.bandedGridColumn7.Width = 233;
            // 
            // bandedGridColumn71
            // 
            this.bandedGridColumn71.Caption = "record";
            this.bandedGridColumn71.FieldName = "record";
            this.bandedGridColumn71.MinWidth = 57;
            this.bandedGridColumn71.Name = "bandedGridColumn71";
            this.bandedGridColumn71.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn71.Width = 219;
            // 
            // gridView5
            // 
            this.gridView5.DetailHeight = 431;
            this.gridView5.GridControl = this.dgv;
            this.gridView5.Name = "gridView5";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.Aquamarine;
            this.panelTop.Controls.Add(this.btnPrintAll);
            this.panelTop.Controls.Add(this.btnRefresh);
            this.panelTop.Controls.Add(this.btnRight);
            this.panelTop.Controls.Add(this.btnLeft);
            this.panelTop.Controls.Add(this.dateTimePicker1);
            this.panelTop.Controls.Add(this.btnSaveAll);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1757, 54);
            this.panelTop.TabIndex = 1;
            // 
            // btnPrintAll
            // 
            this.btnPrintAll.BackColor = System.Drawing.Color.Lime;
            this.btnPrintAll.Location = new System.Drawing.Point(75, 13);
            this.btnPrintAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPrintAll.Name = "btnPrintAll";
            this.btnPrintAll.Size = new System.Drawing.Size(116, 28);
            this.btnPrintAll.TabIndex = 143;
            this.btnPrintAll.Text = "Print All Groups";
            this.btnPrintAll.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.Lime;
            this.btnRefresh.Location = new System.Drawing.Point(941, 12);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(87, 28);
            this.btnRefresh.TabIndex = 142;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.SeaShell;
            this.btnRight.Image = ((System.Drawing.Image)(resources.GetObject("btnRight.Image")));
            this.btnRight.Location = new System.Drawing.Point(712, 15);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(41, 26);
            this.btnRight.TabIndex = 141;
            this.btnRight.UseVisualStyleBackColor = false;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.SeaShell;
            this.btnLeft.Image = ((System.Drawing.Image)(resources.GetObject("btnLeft.Image")));
            this.btnLeft.Location = new System.Drawing.Point(382, 14);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(40, 27);
            this.btnLeft.TabIndex = 140;
            this.btnLeft.UseVisualStyleBackColor = false;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(426, 15);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(280, 23);
            this.dateTimePicker1.TabIndex = 139;
            // 
            // btnSaveAll
            // 
            this.btnSaveAll.BackColor = System.Drawing.Color.Lime;
            this.btnSaveAll.Location = new System.Drawing.Point(248, 12);
            this.btnSaveAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSaveAll.Name = "btnSaveAll";
            this.btnSaveAll.Size = new System.Drawing.Size(87, 28);
            this.btnSaveAll.TabIndex = 138;
            this.btnSaveAll.Text = "Save All Members";
            this.btnSaveAll.UseVisualStyleBackColor = false;
            this.btnSaveAll.Click += new System.EventHandler(this.btnSaveAll_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(21, 11);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(33, 27);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 31;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.hRGroupsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1757, 30);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 26);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printPreviewToolStripMenuItem,
            this.printToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(122, 26);
            this.toolStripMenuItem1.Text = "Print";
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
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // hRGroupsToolStripMenuItem
            // 
            this.hRGroupsToolStripMenuItem.Name = "hRGroupsToolStripMenuItem";
            this.hRGroupsToolStripMenuItem.Size = new System.Drawing.Size(94, 26);
            this.hRGroupsToolStripMenuItem.Text = "HR Groups";
            // 
            // Calendar2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1757, 931);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Calendar2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Calendar";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditTable_FormClosing);
            this.Load += new System.EventHandler(this.Calendar2_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemMemoEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView5)).EndInit();
            this.panelTop.ResumeLayout(false);
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
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn70;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn71;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button btnSaveAll;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button btnRight;
        private DevExpress.XtraEditors.Repository.RepositoryItemMemoEdit repositoryItemMemoEdit1;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand4;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ToolStripMenuItem hRGroupsToolStripMenuItem;
        private System.Windows.Forms.Button btnPrintAll;
    }
}