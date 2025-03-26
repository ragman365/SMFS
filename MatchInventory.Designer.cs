namespace SMFS
{
    partial class MatchInventory
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
            this.gridBand9 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn180 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn185 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn188 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn189 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn10 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repositoryItemCheckEdit4 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnMatch = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
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
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit4)).BeginInit();
            this.panelTop.SuspendLayout();
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
            this.panelAll.Size = new System.Drawing.Size(1697, 300);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 47);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1697, 253);
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
            this.repositoryItemCheckEdit4});
            this.dgv.Size = new System.Drawing.Size(1697, 253);
            this.dgv.TabIndex = 7;
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
            this.gridBand9});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn180,
            this.bandedGridColumn6,
            this.bandedGridColumn185,
            this.bandedGridColumn188,
            this.bandedGridColumn189,
            this.bandedGridColumn1,
            this.bandedGridColumn2,
            this.bandedGridColumn3,
            this.bandedGridColumn4,
            this.bandedGridColumn5,
            this.bandedGridColumn10,
            this.bandedGridColumn8,
            this.bandedGridColumn7,
            this.bandedGridColumn9});
            this.gridMain.DetailHeight = 431;
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.gridMain.OptionsPrint.AllowMultilineHeaders = true;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsSelection.MultiSelect = true;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            // 
            // gridBand9
            // 
            this.gridBand9.Caption = "gridBand1";
            this.gridBand9.Columns.Add(this.bandedGridColumn180);
            this.gridBand9.Columns.Add(this.bandedGridColumn6);
            this.gridBand9.Columns.Add(this.bandedGridColumn185);
            this.gridBand9.Columns.Add(this.bandedGridColumn188);
            this.gridBand9.Columns.Add(this.bandedGridColumn189);
            this.gridBand9.Columns.Add(this.bandedGridColumn1);
            this.gridBand9.Columns.Add(this.bandedGridColumn2);
            this.gridBand9.Columns.Add(this.bandedGridColumn3);
            this.gridBand9.Columns.Add(this.bandedGridColumn4);
            this.gridBand9.Columns.Add(this.bandedGridColumn5);
            this.gridBand9.Columns.Add(this.bandedGridColumn10);
            this.gridBand9.Columns.Add(this.bandedGridColumn8);
            this.gridBand9.Columns.Add(this.bandedGridColumn7);
            this.gridBand9.Columns.Add(this.bandedGridColumn9);
            this.gridBand9.MinWidth = 26;
            this.gridBand9.Name = "gridBand9";
            this.gridBand9.VisibleIndex = 0;
            this.gridBand9.Width = 1189;
            // 
            // bandedGridColumn180
            // 
            this.bandedGridColumn180.Caption = "Num";
            this.bandedGridColumn180.FieldName = "num";
            this.bandedGridColumn180.MinWidth = 49;
            this.bandedGridColumn180.Name = "bandedGridColumn180";
            this.bandedGridColumn180.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn180.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn180.Visible = true;
            this.bandedGridColumn180.Width = 60;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.Caption = "Found";
            this.bandedGridColumn6.FieldName = "found";
            this.bandedGridColumn6.MinWidth = 40;
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Visible = true;
            this.bandedGridColumn6.Width = 80;
            // 
            // bandedGridColumn185
            // 
            this.bandedGridColumn185.Caption = "Location";
            this.bandedGridColumn185.FieldName = "location";
            this.bandedGridColumn185.MinWidth = 49;
            this.bandedGridColumn185.Name = "bandedGridColumn185";
            this.bandedGridColumn185.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn185.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn185.Width = 188;
            // 
            // bandedGridColumn188
            // 
            this.bandedGridColumn188.Caption = "Case Id";
            this.bandedGridColumn188.FieldName = "caseid";
            this.bandedGridColumn188.MinWidth = 49;
            this.bandedGridColumn188.Name = "bandedGridColumn188";
            this.bandedGridColumn188.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn188.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn188.Visible = true;
            this.bandedGridColumn188.Width = 100;
            // 
            // bandedGridColumn189
            // 
            this.bandedGridColumn189.Caption = "Decedent Full Name";
            this.bandedGridColumn189.FieldName = "DecedentFullName";
            this.bandedGridColumn189.MinWidth = 49;
            this.bandedGridColumn189.Name = "bandedGridColumn189";
            this.bandedGridColumn189.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn189.Width = 188;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "Service Date ";
            this.bandedGridColumn1.DisplayFormat.FormatString = "MM/dd/yyyy";
            this.bandedGridColumn1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.bandedGridColumn1.FieldName = "date";
            this.bandedGridColumn1.MinWidth = 55;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Visible = true;
            this.bandedGridColumn1.Width = 100;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.Caption = "Item Name";
            this.bandedGridColumn2.FieldName = "ItemName";
            this.bandedGridColumn2.MinWidth = 55;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 161;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "Serial Number";
            this.bandedGridColumn3.FieldName = "SerialNumber";
            this.bandedGridColumn3.MinWidth = 55;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 100;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "Secondary Arranger";
            this.bandedGridColumn4.FieldName = "SecondaryArranger";
            this.bandedGridColumn4.MinWidth = 55;
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.Width = 188;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.Caption = "Decedent Date of Death";
            this.bandedGridColumn5.FieldName = "DecedentDateofDeath";
            this.bandedGridColumn5.MinWidth = 55;
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 100;
            // 
            // bandedGridColumn10
            // 
            this.bandedGridColumn10.Caption = "SMFS ServiceId";
            this.bandedGridColumn10.FieldName = "SMFSServiceId";
            this.bandedGridColumn10.MinWidth = 29;
            this.bandedGridColumn10.Name = "bandedGridColumn10";
            this.bandedGridColumn10.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn10.Visible = true;
            this.bandedGridColumn10.Width = 100;
            // 
            // bandedGridColumn8
            // 
            this.bandedGridColumn8.Caption = "SMFS Service Date";
            this.bandedGridColumn8.DisplayFormat.FormatString = "MM/dd/yyyy";
            this.bandedGridColumn8.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.bandedGridColumn8.FieldName = "SMFSdate";
            this.bandedGridColumn8.MinWidth = 34;
            this.bandedGridColumn8.Name = "bandedGridColumn8";
            this.bandedGridColumn8.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn8.Visible = true;
            this.bandedGridColumn8.Width = 100;
            // 
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.Caption = "SMFS Item Name";
            this.bandedGridColumn7.FieldName = "SMFSItemName";
            this.bandedGridColumn7.MinWidth = 34;
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Visible = true;
            this.bandedGridColumn7.Width = 188;
            // 
            // bandedGridColumn9
            // 
            this.bandedGridColumn9.Caption = "SMFS DOD";
            this.bandedGridColumn9.DisplayFormat.FormatString = "MM/dd/yyyy";
            this.bandedGridColumn9.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.bandedGridColumn9.FieldName = "SMFSdod";
            this.bandedGridColumn9.MinWidth = 34;
            this.bandedGridColumn9.Name = "bandedGridColumn9";
            this.bandedGridColumn9.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn9.Visible = true;
            this.bandedGridColumn9.Width = 100;
            // 
            // repositoryItemCheckEdit4
            // 
            this.repositoryItemCheckEdit4.AutoHeight = false;
            this.repositoryItemCheckEdit4.Name = "repositoryItemCheckEdit4";
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnMatch);
            this.panelTop.Controls.Add(this.btnSelectFile);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1697, 47);
            this.panelTop.TabIndex = 1;
            // 
            // btnMatch
            // 
            this.btnMatch.Location = new System.Drawing.Point(625, 9);
            this.btnMatch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMatch.Name = "btnMatch";
            this.btnMatch.Size = new System.Drawing.Size(87, 28);
            this.btnMatch.TabIndex = 2;
            this.btnMatch.Text = "Match";
            this.btnMatch.UseVisualStyleBackColor = true;
            this.btnMatch.Click += new System.EventHandler(this.btnMatch_Click);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(12, 11);
            this.btnSelectFile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(87, 28);
            this.btnSelectFile.TabIndex = 1;
            this.btnSelectFile.Text = "Select File";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1697, 30);
            this.menuStrip1.TabIndex = 6;
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
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // MatchInventory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1697, 330);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MatchInventory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Match Inventory";
            this.Load += new System.EventHandler(this.MatchInventory_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit4)).EndInit();
            this.panelTop.ResumeLayout(false);
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
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn180;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn185;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn188;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn189;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit4;
        private System.Windows.Forms.Button btnSelectFile;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button btnMatch;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn8;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn9;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand9;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn10;
    }
}