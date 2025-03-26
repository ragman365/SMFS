namespace SMFS
{
    partial class AddNewContract
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
            DevExpress.XtraGrid.GridLevelNode gridLevelNode1 = new DevExpress.XtraGrid.GridLevelNode();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelBottomRight = new System.Windows.Forms.Panel();
            this.btnClearRemoval = new System.Windows.Forms.Button();
            this.btnRefundRemCurrMonth = new System.Windows.Forms.Button();
            this.btnRefundRemYTDprevious = new System.Windows.Forms.Button();
            this.btnDeathRemCurrMonth = new System.Windows.Forms.Button();
            this.btnDeathRemYTDprevious = new System.Windows.Forms.Button();
            this.btnMoveBalance = new System.Windows.Forms.Button();
            this.panelBottomLeft = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.Num = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn11 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn12 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnAddUpdate = new System.Windows.Forms.Button();
            this.txtWorkDate = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelBottomRight.SuspendLayout();
            this.panelBottomLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).BeginInit();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(636, 412);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.panelBottomRight);
            this.panelBottom.Controls.Add(this.panelBottomLeft);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 37);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(636, 375);
            this.panelBottom.TabIndex = 2;
            // 
            // panelBottomRight
            // 
            this.panelBottomRight.Controls.Add(this.btnClearRemoval);
            this.panelBottomRight.Controls.Add(this.btnRefundRemCurrMonth);
            this.panelBottomRight.Controls.Add(this.btnRefundRemYTDprevious);
            this.panelBottomRight.Controls.Add(this.btnDeathRemCurrMonth);
            this.panelBottomRight.Controls.Add(this.btnDeathRemYTDprevious);
            this.panelBottomRight.Controls.Add(this.btnMoveBalance);
            this.panelBottomRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottomRight.Location = new System.Drawing.Point(451, 0);
            this.panelBottomRight.Name = "panelBottomRight";
            this.panelBottomRight.Size = new System.Drawing.Size(185, 375);
            this.panelBottomRight.TabIndex = 8;
            // 
            // btnClearRemoval
            // 
            this.btnClearRemoval.BackColor = System.Drawing.Color.Transparent;
            this.btnClearRemoval.ForeColor = System.Drawing.Color.Black;
            this.btnClearRemoval.Location = new System.Drawing.Point(7, 258);
            this.btnClearRemoval.Name = "btnClearRemoval";
            this.btnClearRemoval.Size = new System.Drawing.Size(167, 23);
            this.btnClearRemoval.TabIndex = 10;
            this.btnClearRemoval.Text = "Clear Removal";
            this.btnClearRemoval.UseVisualStyleBackColor = false;
            this.btnClearRemoval.Click += new System.EventHandler(this.btnClearRemoval_Click);
            // 
            // btnRefundRemCurrMonth
            // 
            this.btnRefundRemCurrMonth.BackColor = System.Drawing.Color.Transparent;
            this.btnRefundRemCurrMonth.ForeColor = System.Drawing.Color.Black;
            this.btnRefundRemCurrMonth.Location = new System.Drawing.Point(7, 192);
            this.btnRefundRemCurrMonth.Name = "btnRefundRemCurrMonth";
            this.btnRefundRemCurrMonth.Size = new System.Drawing.Size(167, 23);
            this.btnRefundRemCurrMonth.TabIndex = 9;
            this.btnRefundRemCurrMonth.Text = "Refund Rem Current Month";
            this.btnRefundRemCurrMonth.UseVisualStyleBackColor = false;
            this.btnRefundRemCurrMonth.Click += new System.EventHandler(this.btnRefundRemCurrMonth_Click);
            // 
            // btnRefundRemYTDprevious
            // 
            this.btnRefundRemYTDprevious.BackColor = System.Drawing.Color.Transparent;
            this.btnRefundRemYTDprevious.ForeColor = System.Drawing.Color.Black;
            this.btnRefundRemYTDprevious.Location = new System.Drawing.Point(7, 151);
            this.btnRefundRemYTDprevious.Name = "btnRefundRemYTDprevious";
            this.btnRefundRemYTDprevious.Size = new System.Drawing.Size(167, 23);
            this.btnRefundRemYTDprevious.TabIndex = 8;
            this.btnRefundRemYTDprevious.Text = "Refund Rem YTD Previous";
            this.btnRefundRemYTDprevious.UseVisualStyleBackColor = false;
            this.btnRefundRemYTDprevious.Click += new System.EventHandler(this.btnRefundRemYTDprevious_Click);
            // 
            // btnDeathRemCurrMonth
            // 
            this.btnDeathRemCurrMonth.BackColor = System.Drawing.Color.Transparent;
            this.btnDeathRemCurrMonth.ForeColor = System.Drawing.Color.Black;
            this.btnDeathRemCurrMonth.Location = new System.Drawing.Point(6, 97);
            this.btnDeathRemCurrMonth.Name = "btnDeathRemCurrMonth";
            this.btnDeathRemCurrMonth.Size = new System.Drawing.Size(167, 23);
            this.btnDeathRemCurrMonth.TabIndex = 7;
            this.btnDeathRemCurrMonth.Text = "Death Rem Current Month";
            this.btnDeathRemCurrMonth.UseVisualStyleBackColor = false;
            this.btnDeathRemCurrMonth.Click += new System.EventHandler(this.btnDeathRemCurrMonth_Click);
            // 
            // btnDeathRemYTDprevious
            // 
            this.btnDeathRemYTDprevious.BackColor = System.Drawing.Color.Transparent;
            this.btnDeathRemYTDprevious.ForeColor = System.Drawing.Color.Black;
            this.btnDeathRemYTDprevious.Location = new System.Drawing.Point(6, 56);
            this.btnDeathRemYTDprevious.Name = "btnDeathRemYTDprevious";
            this.btnDeathRemYTDprevious.Size = new System.Drawing.Size(167, 23);
            this.btnDeathRemYTDprevious.TabIndex = 6;
            this.btnDeathRemYTDprevious.Text = "Death Rem YTD Previous";
            this.btnDeathRemYTDprevious.UseVisualStyleBackColor = false;
            this.btnDeathRemYTDprevious.Click += new System.EventHandler(this.btnDeathRemYTDprevious_Click);
            // 
            // btnMoveBalance
            // 
            this.btnMoveBalance.BackColor = System.Drawing.Color.Transparent;
            this.btnMoveBalance.ForeColor = System.Drawing.Color.Black;
            this.btnMoveBalance.Location = new System.Drawing.Point(6, 3);
            this.btnMoveBalance.Name = "btnMoveBalance";
            this.btnMoveBalance.Size = new System.Drawing.Size(167, 23);
            this.btnMoveBalance.TabIndex = 5;
            this.btnMoveBalance.Text = "Move Balance to";
            this.btnMoveBalance.UseVisualStyleBackColor = false;
            this.btnMoveBalance.Click += new System.EventHandler(this.btnMoveBalance_Click);
            // 
            // panelBottomLeft
            // 
            this.panelBottomLeft.Controls.Add(this.dgv);
            this.panelBottomLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelBottomLeft.Location = new System.Drawing.Point(0, 0);
            this.panelBottomLeft.Name = "panelBottomLeft";
            this.panelBottomLeft.Size = new System.Drawing.Size(451, 375);
            this.panelBottomLeft.TabIndex = 7;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            gridLevelNode1.RelationName = "Level1";
            this.dgv.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] {
            gridLevelNode1});
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "iMaginary";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBox1});
            this.dgv.Size = new System.Drawing.Size(451, 375);
            this.dgv.TabIndex = 6;
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
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand1});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.Num,
            this.bandedGridColumn11,
            this.bandedGridColumn12});
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain.OptionsNavigation.UseAdvHorzNavigation = false;
            this.gridMain.OptionsPrint.AutoWidth = false;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsView.ColumnAutoWidth = true;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.gridMain_ShowingEditor);
            this.gridMain.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridMain_CellValueChanged);
            this.gridMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridMain_KeyDown);
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Columns.Add(this.Num);
            this.gridBand1.Columns.Add(this.bandedGridColumn11);
            this.gridBand1.Columns.Add(this.bandedGridColumn12);
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 300;
            // 
            // Num
            // 
            this.Num.Caption = "Num";
            this.Num.FieldName = "num";
            this.Num.Name = "Num";
            this.Num.OptionsColumn.AllowEdit = false;
            this.Num.OptionsColumn.FixedWidth = true;
            this.Num.OptionsColumn.ReadOnly = true;
            this.Num.Visible = true;
            this.Num.Width = 50;
            // 
            // bandedGridColumn11
            // 
            this.bandedGridColumn11.Caption = "Description";
            this.bandedGridColumn11.FieldName = "description";
            this.bandedGridColumn11.Name = "bandedGridColumn11";
            this.bandedGridColumn11.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn11.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn11.OptionsColumn.ReadOnly = true;
            this.bandedGridColumn11.Visible = true;
            this.bandedGridColumn11.Width = 150;
            // 
            // bandedGridColumn12
            // 
            this.bandedGridColumn12.Caption = "Data";
            this.bandedGridColumn12.FieldName = "data";
            this.bandedGridColumn12.Name = "bandedGridColumn12";
            this.bandedGridColumn12.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn12.Visible = true;
            this.bandedGridColumn12.Width = 100;
            // 
            // repositoryItemComboBox1
            // 
            this.repositoryItemComboBox1.AutoHeight = false;
            this.repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox1.Items.AddRange(new object[] {
            "ACH",
            "CC",
            "LKBX",
            "NONE"});
            this.repositoryItemComboBox1.Name = "repositoryItemComboBox1";
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnAddUpdate);
            this.panelTop.Controls.Add(this.txtWorkDate);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(636, 37);
            this.panelTop.TabIndex = 1;
            // 
            // btnAddUpdate
            // 
            this.btnAddUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAddUpdate.ForeColor = System.Drawing.Color.Black;
            this.btnAddUpdate.Location = new System.Drawing.Point(186, 6);
            this.btnAddUpdate.Name = "btnAddUpdate";
            this.btnAddUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnAddUpdate.TabIndex = 4;
            this.btnAddUpdate.Text = "Update";
            this.btnAddUpdate.UseVisualStyleBackColor = false;
            this.btnAddUpdate.Click += new System.EventHandler(this.btnAddUpdate_Click);
            // 
            // txtWorkDate
            // 
            this.txtWorkDate.Enabled = false;
            this.txtWorkDate.Location = new System.Drawing.Point(66, 6);
            this.txtWorkDate.Name = "txtWorkDate";
            this.txtWorkDate.Size = new System.Drawing.Size(100, 21);
            this.txtWorkDate.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Work Date :";
            // 
            // AddNewContract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 412);
            this.Controls.Add(this.panelAll);
            this.Name = "AddNewContract";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AddNewContract";
            this.Load += new System.EventHandler(this.AddNewContract_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelBottomRight.ResumeLayout(false);
            this.panelBottomLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox1)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox txtWorkDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddUpdate;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn Num;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn11;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn12;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox1;
        private System.Windows.Forms.Panel panelBottomRight;
        private System.Windows.Forms.Button btnDeathRemYTDprevious;
        private System.Windows.Forms.Button btnMoveBalance;
        private System.Windows.Forms.Panel panelBottomLeft;
        private System.Windows.Forms.Button btnDeathRemCurrMonth;
        private System.Windows.Forms.Button btnRefundRemCurrMonth;
        private System.Windows.Forms.Button btnRefundRemYTDprevious;
        private System.Windows.Forms.Button btnClearRemoval;
    }
}