namespace SMFS
{
    partial class Contracts
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Contracts));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.Num = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.contractNumber = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.serviceTotal = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repButton = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repositoryItemPictureEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.chkNewContracts = new System.Windows.Forms.CheckBox();
            this.pictureDelete = new System.Windows.Forms.PictureBox();
            this.pictureAdd = new System.Windows.Forms.PictureBox();
            this.chkActiveOnly = new System.Windows.Forms.CheckBox();
            this.chkIncludePaid = new System.Windows.Forms.CheckBox();
            this.chkShowAll = new System.Windows.Forms.CheckBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.chkLapse = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewContractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).BeginInit();
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
            this.panelAll.Location = new System.Drawing.Point(0, 24);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(838, 237);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 48);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(838, 189);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "Caramel";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repButton,
            this.repCheckEdit1,
            this.repositoryItemPictureEdit1});
            this.dgv.Size = new System.Drawing.Size(838, 189);
            this.dgv.TabIndex = 3;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridMain});
            // 
            // gridMain
            // 
            this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.Wheat;
            this.gridMain.Appearance.FocusedRow.BackColor = System.Drawing.Color.Yellow;
            this.gridMain.Appearance.FocusedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.gridMain.Appearance.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.gridMain.Appearance.OddRow.BackColor = System.Drawing.Color.Orange;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand1});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.ColumnPanelRowHeight = 5;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.Num,
            this.bandedGridColumn1,
            this.contractNumber,
            this.bandedGridColumn9,
            this.serviceTotal,
            this.bandedGridColumn2,
            this.bandedGridColumn3,
            this.bandedGridColumn4,
            this.bandedGridColumn5,
            this.bandedGridColumn6,
            this.bandedGridColumn7,
            this.bandedGridColumn8});
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            this.gridMain.OptionsBehavior.ReadOnly = true;
            this.gridMain.OptionsPrint.PrintBandHeader = false;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.EnableAppearanceOddRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.RowHeight = 1;
            this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick_2);
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Columns.Add(this.Num);
            this.gridBand1.Columns.Add(this.contractNumber);
            this.gridBand1.Columns.Add(this.bandedGridColumn9);
            this.gridBand1.Columns.Add(this.bandedGridColumn8);
            this.gridBand1.Columns.Add(this.serviceTotal);
            this.gridBand1.Columns.Add(this.bandedGridColumn2);
            this.gridBand1.Columns.Add(this.bandedGridColumn3);
            this.gridBand1.Columns.Add(this.bandedGridColumn6);
            this.gridBand1.Columns.Add(this.bandedGridColumn7);
            this.gridBand1.Columns.Add(this.bandedGridColumn4);
            this.gridBand1.Columns.Add(this.bandedGridColumn1);
            this.gridBand1.Columns.Add(this.bandedGridColumn5);
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 600;
            // 
            // Num
            // 
            this.Num.AppearanceHeader.Options.UseTextOptions = true;
            this.Num.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
            this.Num.Caption = "Num";
            this.Num.FieldName = "num";
            this.Num.Name = "Num";
            this.Num.OptionsColumn.AllowEdit = false;
            this.Num.OptionsColumn.FixedWidth = true;
            this.Num.Visible = true;
            this.Num.Width = 50;
            // 
            // contractNumber
            // 
            this.contractNumber.Caption = "Contract";
            this.contractNumber.FieldName = "contractNumber";
            this.contractNumber.Name = "contractNumber";
            this.contractNumber.OptionsColumn.AllowEdit = false;
            this.contractNumber.OptionsColumn.FixedWidth = true;
            this.contractNumber.Visible = true;
            this.contractNumber.Width = 80;
            // 
            // bandedGridColumn9
            // 
            this.bandedGridColumn9.Caption = "Issue Date";
            this.bandedGridColumn9.DisplayFormat.FormatString = "mm/dd/yyyy";
            this.bandedGridColumn9.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.bandedGridColumn9.FieldName = "issueDate8";
            this.bandedGridColumn9.Name = "bandedGridColumn9";
            this.bandedGridColumn9.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn9.Visible = true;
            // 
            // bandedGridColumn8
            // 
            this.bandedGridColumn8.AppearanceCell.Options.UseTextOptions = true;
            this.bandedGridColumn8.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.bandedGridColumn8.Caption = "L";
            this.bandedGridColumn8.FieldName = "lapsed";
            this.bandedGridColumn8.Name = "bandedGridColumn8";
            this.bandedGridColumn8.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn8.Visible = true;
            this.bandedGridColumn8.Width = 20;
            // 
            // serviceTotal
            // 
            this.serviceTotal.Caption = "Service Total";
            this.serviceTotal.DisplayFormat.FormatString = "N2";
            this.serviceTotal.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.serviceTotal.FieldName = "serviceTotal";
            this.serviceTotal.MinWidth = 10;
            this.serviceTotal.Name = "serviceTotal";
            this.serviceTotal.OptionsColumn.AllowEdit = false;
            this.serviceTotal.OptionsColumn.FixedWidth = true;
            this.serviceTotal.Visible = true;
            this.serviceTotal.Width = 100;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.AppearanceHeader.Options.UseTextOptions = true;
            this.bandedGridColumn2.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn2.Caption = "Merchandise Total";
            this.bandedGridColumn2.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn2.FieldName = "merchandiseTotal";
            this.bandedGridColumn2.MinWidth = 10;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 100;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.AppearanceHeader.Options.UseTextOptions = true;
            this.bandedGridColumn3.AppearanceHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.bandedGridColumn3.Caption = "Down Payment";
            this.bandedGridColumn3.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn3.FieldName = "downPayment";
            this.bandedGridColumn3.MinWidth = 10;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 100;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.Caption = "Total Purchase";
            this.bandedGridColumn6.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn6.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn6.FieldName = "purchase";
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Width = 100;
            // 
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.Caption = "Paid";
            this.bandedGridColumn7.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn7.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn7.FieldName = "paid";
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Width = 100;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "Agreement";
            this.bandedGridColumn4.ColumnEdit = this.repCheckEdit1;
            this.bandedGridColumn4.FieldName = "agreement";
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            // 
            // repCheckEdit1
            // 
            this.repCheckEdit1.AutoHeight = false;
            this.repCheckEdit1.Name = "repCheckEdit1";
            this.repCheckEdit1.Click += new System.EventHandler(this.repCheckEdit1_Click);
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "record";
            this.bandedGridColumn1.FieldName = "record";
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.Caption = "Agreement File";
            this.bandedGridColumn5.FieldName = "agreementFile";
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            // 
            // repButton
            // 
            this.repButton.AutoHeight = false;
            this.repButton.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.repButton.Name = "repButton";
            // 
            // repositoryItemPictureEdit1
            // 
            contextButton1.Caption = "contextButton1";
            contextButton1.Id = new System.Guid("7020ad83-ad48-4296-8d4d-5a08a0a3f273");
            contextButton1.Name = "contextButton1";
            this.repositoryItemPictureEdit1.ContextButtons.Add(contextButton1);
            this.repositoryItemPictureEdit1.Name = "repositoryItemPictureEdit1";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelTop.Controls.Add(this.cmbType);
            this.panelTop.Controls.Add(this.btnRight);
            this.panelTop.Controls.Add(this.btnLeft);
            this.panelTop.Controls.Add(this.dateTimePicker2);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.dateTimePicker1);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.chkNewContracts);
            this.panelTop.Controls.Add(this.pictureDelete);
            this.panelTop.Controls.Add(this.pictureAdd);
            this.panelTop.Controls.Add(this.chkActiveOnly);
            this.panelTop.Controls.Add(this.chkIncludePaid);
            this.panelTop.Controls.Add(this.chkShowAll);
            this.panelTop.Controls.Add(this.btnRefresh);
            this.panelTop.Controls.Add(this.chkLapse);
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(838, 48);
            this.panelTop.TabIndex = 1;
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.LightSteelBlue;
            this.btnRight.Image = ((System.Drawing.Image)(resources.GetObject("btnRight.Image")));
            this.btnRight.Location = new System.Drawing.Point(549, 48);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(28, 23);
            this.btnRight.TabIndex = 27;
            this.btnRight.UseVisualStyleBackColor = false;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.LightSteelBlue;
            this.btnLeft.Image = ((System.Drawing.Image)(resources.GetObject("btnLeft.Image")));
            this.btnLeft.Location = new System.Drawing.Point(78, 50);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(28, 23);
            this.btnLeft.TabIndex = 26;
            this.btnLeft.UseVisualStyleBackColor = false;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Location = new System.Drawing.Point(347, 51);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker2.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(314, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "-To-";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(108, 51);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker1.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Date Issued :";
            // 
            // chkNewContracts
            // 
            this.chkNewContracts.AutoSize = true;
            this.chkNewContracts.Location = new System.Drawing.Point(348, 8);
            this.chkNewContracts.Name = "chkNewContracts";
            this.chkNewContracts.Size = new System.Drawing.Size(137, 17);
            this.chkNewContracts.TabIndex = 18;
            this.chkNewContracts.Text = "Show Contracts Issued";
            this.chkNewContracts.UseVisualStyleBackColor = true;
            this.chkNewContracts.CheckedChanged += new System.EventHandler(this.chkNewContracts_CheckedChanged);
            // 
            // pictureDelete
            // 
            this.pictureDelete.Image = ((System.Drawing.Image)(resources.GetObject("pictureDelete.Image")));
            this.pictureDelete.Location = new System.Drawing.Point(92, 13);
            this.pictureDelete.Name = "pictureDelete";
            this.pictureDelete.Size = new System.Drawing.Size(31, 23);
            this.pictureDelete.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureDelete.TabIndex = 17;
            this.pictureDelete.TabStop = false;
            this.pictureDelete.Click += new System.EventHandler(this.pictureDelete_Click);
            // 
            // pictureAdd
            // 
            this.pictureAdd.Image = ((System.Drawing.Image)(resources.GetObject("pictureAdd.Image")));
            this.pictureAdd.Location = new System.Drawing.Point(45, 13);
            this.pictureAdd.Name = "pictureAdd";
            this.pictureAdd.Size = new System.Drawing.Size(31, 23);
            this.pictureAdd.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureAdd.TabIndex = 16;
            this.pictureAdd.TabStop = false;
            this.pictureAdd.Tag = "Add Client";
            this.pictureAdd.Click += new System.EventHandler(this.pictureAdd_Click);
            // 
            // chkActiveOnly
            // 
            this.chkActiveOnly.AutoSize = true;
            this.chkActiveOnly.Location = new System.Drawing.Point(143, 26);
            this.chkActiveOnly.Name = "chkActiveOnly";
            this.chkActiveOnly.Size = new System.Drawing.Size(85, 17);
            this.chkActiveOnly.TabIndex = 15;
            this.chkActiveOnly.Text = "Show Active";
            this.chkActiveOnly.UseVisualStyleBackColor = true;
            this.chkActiveOnly.CheckedChanged += new System.EventHandler(this.chkActiveOnly_CheckedChanged);
            // 
            // chkIncludePaid
            // 
            this.chkIncludePaid.AutoSize = true;
            this.chkIncludePaid.Location = new System.Drawing.Point(236, 8);
            this.chkIncludePaid.Name = "chkIncludePaid";
            this.chkIncludePaid.Size = new System.Drawing.Size(106, 17);
            this.chkIncludePaid.TabIndex = 14;
            this.chkIncludePaid.Text = "Include Paid Amt";
            this.chkIncludePaid.UseVisualStyleBackColor = true;
            this.chkIncludePaid.CheckedChanged += new System.EventHandler(this.chkIncludePaid_CheckedChanged);
            // 
            // chkShowAll
            // 
            this.chkShowAll.AutoSize = true;
            this.chkShowAll.Checked = true;
            this.chkShowAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowAll.Location = new System.Drawing.Point(143, 7);
            this.chkShowAll.Name = "chkShowAll";
            this.chkShowAll.Size = new System.Drawing.Size(66, 17);
            this.chkShowAll.TabIndex = 6;
            this.chkShowAll.Text = "Show All";
            this.chkShowAll.UseVisualStyleBackColor = true;
            this.chkShowAll.CheckedChanged += new System.EventHandler(this.chkShowAll_CheckedChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnRefresh.Location = new System.Drawing.Point(491, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 39);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // chkLapse
            // 
            this.chkLapse.AutoSize = true;
            this.chkLapse.Location = new System.Drawing.Point(236, 27);
            this.chkLapse.Name = "chkLapse";
            this.chkLapse.Size = new System.Drawing.Size(83, 17);
            this.chkLapse.TabIndex = 4;
            this.chkLapse.Text = "Show Lapse";
            this.chkLapse.UseVisualStyleBackColor = true;
            this.chkLapse.CheckedChanged += new System.EventHandler(this.chkLapse_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(31, 33);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.columnsToolStripMenuItem,
            this.miscToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(838, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPrint,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuPrint
            // 
            this.menuPrint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printPreviewToolStripMenuItem,
            this.printToolStripMenuItem});
            this.menuPrint.Name = "menuPrint";
            this.menuPrint.Size = new System.Drawing.Size(99, 22);
            this.menuPrint.Text = "Print";
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
            // columnsToolStripMenuItem
            // 
            this.columnsToolStripMenuItem.Name = "columnsToolStripMenuItem";
            this.columnsToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.columnsToolStripMenuItem.Text = "Columns";
            // 
            // miscToolStripMenuItem
            // 
            this.miscToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewContractToolStripMenuItem});
            this.miscToolStripMenuItem.Name = "miscToolStripMenuItem";
            this.miscToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.miscToolStripMenuItem.Text = "Misc";
            // 
            // addNewContractToolStripMenuItem
            // 
            this.addNewContractToolStripMenuItem.Name = "addNewContractToolStripMenuItem";
            this.addNewContractToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addNewContractToolStripMenuItem.Text = "Add New Contract";
            this.addNewContractToolStripMenuItem.Click += new System.EventHandler(this.addNewContractToolStripMenuItem_Click);
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Trusts",
            "Single Premium",
            "Insurance",
            "All"});
            this.cmbType.Location = new System.Drawing.Point(348, 24);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(106, 21);
            this.cmbType.TabIndex = 28;
            this.cmbType.Text = "Trusts";
            // 
            // Contracts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 261);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.LookAndFeel.SkinName = "Coffee";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "Contracts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Contracts";
            this.Load += new System.EventHandler(this.XtraForm1_Load);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).EndInit();
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
        private DevExpress.XtraGrid.GridControl dgv;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repButton;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repCheckEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryItemPictureEdit1;
        private System.Windows.Forms.CheckBox chkLapse;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox chkShowAll;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPrint;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem columnsToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkIncludePaid;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn Num;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn contractNumber;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn8;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn serviceTotal;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private System.Windows.Forms.CheckBox chkActiveOnly;
        private System.Windows.Forms.PictureBox pictureDelete;
        private System.Windows.Forms.PictureBox pictureAdd;
        private System.Windows.Forms.ToolStripMenuItem miscToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewContractToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkNewContracts;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn9;
        private System.Windows.Forms.ComboBox cmbType;
    }
}