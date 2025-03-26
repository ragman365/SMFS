namespace SMFS
{
    partial class ImportCasketCosts
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportCasketCosts));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.mainGrid = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.chkAllowDuplicates = new System.Windows.Forms.CheckBox();
            this.btnTie = new System.Windows.Forms.Button();
            this.lblTotal = new System.Windows.Forms.Label();
            this.labelMaximum = new System.Windows.Forms.Label();
            this.barImport = new System.Windows.Forms.ProgressBar();
            this.picLoader = new System.Windows.Forms.PictureBox();
            this.btnImportFile = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnImportBatesville = new System.Windows.Forms.Button();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timCostsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLoader)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Controls.Add(this.menuStrip1);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 0);
            this.panelAll.Margin = new System.Windows.Forms.Padding(4);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(1154, 321);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 107);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1154, 214);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4);
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.MainView = this.mainGrid;
            this.dgv.Margin = new System.Windows.Forms.Padding(4);
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(1154, 214);
            this.dgv.TabIndex = 0;
            this.dgv.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.mainGrid});
            // 
            // mainGrid
            // 
            this.mainGrid.Appearance.EvenRow.BackColor = System.Drawing.Color.Cyan;
            this.mainGrid.Appearance.EvenRow.Options.UseBackColor = true;
            this.mainGrid.Appearance.OddRow.BackColor = System.Drawing.Color.Azure;
            this.mainGrid.Appearance.OddRow.Options.UseBackColor = true;
            this.mainGrid.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand1});
            this.mainGrid.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.bandedGridColumn1,
            this.bandedGridColumn6,
            this.bandedGridColumn2,
            this.bandedGridColumn3,
            this.bandedGridColumn4,
            this.bandedGridColumn7,
            this.bandedGridColumn5});
            this.mainGrid.DetailHeight = 431;
            this.mainGrid.GridControl = this.dgv;
            this.mainGrid.Name = "mainGrid";
            this.mainGrid.OptionsBehavior.AutoPopulateColumns = false;
            this.mainGrid.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.mainGrid.OptionsSelection.MultiSelect = true;
            this.mainGrid.OptionsView.ColumnAutoWidth = false;
            this.mainGrid.OptionsView.EnableAppearanceEvenRow = true;
            this.mainGrid.OptionsView.EnableAppearanceOddRow = true;
            this.mainGrid.OptionsView.ShowBands = false;
            this.mainGrid.OptionsView.ShowGroupPanel = false;
            this.mainGrid.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.mainGrid_CustomDrawCell);
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Columns.Add(this.bandedGridColumn1);
            this.gridBand1.Columns.Add(this.bandedGridColumn6);
            this.gridBand1.Columns.Add(this.bandedGridColumn2);
            this.gridBand1.Columns.Add(this.bandedGridColumn3);
            this.gridBand1.Columns.Add(this.bandedGridColumn4);
            this.gridBand1.Columns.Add(this.bandedGridColumn7);
            this.gridBand1.Columns.Add(this.bandedGridColumn5);
            this.gridBand1.MinWidth = 20;
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 1114;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "Num";
            this.bandedGridColumn1.FieldName = "num";
            this.bandedGridColumn1.MinWidth = 29;
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            this.bandedGridColumn1.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn1.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn1.Visible = true;
            this.bandedGridColumn1.Width = 58;
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.Caption = "Item Number";
            this.bandedGridColumn6.FieldName = "itemNumber";
            this.bandedGridColumn6.MinWidth = 29;
            this.bandedGridColumn6.Name = "bandedGridColumn6";
            this.bandedGridColumn6.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn6.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn6.Visible = true;
            this.bandedGridColumn6.Width = 110;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.Caption = "Casket Code";
            this.bandedGridColumn2.FieldName = "casketCode";
            this.bandedGridColumn2.MinWidth = 34;
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 110;
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "Casket Description";
            this.bandedGridColumn3.FieldName = "casketDescription";
            this.bandedGridColumn3.MinWidth = 34;
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            this.bandedGridColumn3.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn3.Visible = true;
            this.bandedGridColumn3.Width = 462;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "Casket Cost";
            this.bandedGridColumn4.DisplayFormat.FormatString = "N2";
            this.bandedGridColumn4.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.bandedGridColumn4.FieldName = "casketCost";
            this.bandedGridColumn4.MinWidth = 34;
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            this.bandedGridColumn4.Width = 136;
            // 
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.Caption = "Old Casket Cost";
            this.bandedGridColumn7.FieldName = "oldCasketCost";
            this.bandedGridColumn7.MinWidth = 29;
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Visible = true;
            this.bandedGridColumn7.Width = 110;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.Caption = "Status";
            this.bandedGridColumn5.FieldName = "status";
            this.bandedGridColumn5.MinWidth = 34;
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 128;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnImportBatesville);
            this.panelTop.Controls.Add(this.pictureBox4);
            this.panelTop.Controls.Add(this.chkAllowDuplicates);
            this.panelTop.Controls.Add(this.btnTie);
            this.panelTop.Controls.Add(this.lblTotal);
            this.panelTop.Controls.Add(this.labelMaximum);
            this.panelTop.Controls.Add(this.barImport);
            this.panelTop.Controls.Add(this.picLoader);
            this.panelTop.Controls.Add(this.btnImportFile);
            this.panelTop.Controls.Add(this.btnSelectFile);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 28);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1154, 79);
            this.panelTop.TabIndex = 1;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pictureBox4.Location = new System.Drawing.Point(126, 10);
            this.pictureBox4.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(41, 28);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 10;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.pictureBox4_Click);
            // 
            // chkAllowDuplicates
            // 
            this.chkAllowDuplicates.AutoSize = true;
            this.chkAllowDuplicates.Location = new System.Drawing.Point(928, 45);
            this.chkAllowDuplicates.Margin = new System.Windows.Forms.Padding(4);
            this.chkAllowDuplicates.Name = "chkAllowDuplicates";
            this.chkAllowDuplicates.Size = new System.Drawing.Size(132, 21);
            this.chkAllowDuplicates.TabIndex = 9;
            this.chkAllowDuplicates.Text = "Allow Duplicates";
            this.chkAllowDuplicates.UseVisualStyleBackColor = true;
            this.chkAllowDuplicates.Visible = false;
            // 
            // btnTie
            // 
            this.btnTie.Location = new System.Drawing.Point(931, 9);
            this.btnTie.Margin = new System.Windows.Forms.Padding(4);
            this.btnTie.Name = "btnTie";
            this.btnTie.Size = new System.Drawing.Size(100, 28);
            this.btnTie.TabIndex = 8;
            this.btnTie.Text = "Tie to DB";
            this.btnTie.UseVisualStyleBackColor = true;
            this.btnTie.Visible = false;
            this.btnTie.Click += new System.EventHandler(this.btnTie_Click);
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(617, 52);
            this.lblTotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(46, 17);
            this.lblTotal.TabIndex = 7;
            this.lblTotal.Text = "label1";
            // 
            // labelMaximum
            // 
            this.labelMaximum.AutoSize = true;
            this.labelMaximum.Location = new System.Drawing.Point(516, 52);
            this.labelMaximum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMaximum.Name = "labelMaximum";
            this.labelMaximum.Size = new System.Drawing.Size(46, 17);
            this.labelMaximum.TabIndex = 6;
            this.labelMaximum.Text = "label1";
            // 
            // barImport
            // 
            this.barImport.BackColor = System.Drawing.Color.Lime;
            this.barImport.Location = new System.Drawing.Point(4, 52);
            this.barImport.Margin = new System.Windows.Forms.Padding(4);
            this.barImport.Name = "barImport";
            this.barImport.Size = new System.Drawing.Size(485, 15);
            this.barImport.TabIndex = 5;
            // 
            // picLoader
            // 
            this.picLoader.Image = ((System.Drawing.Image)(resources.GetObject("picLoader.Image")));
            this.picLoader.Location = new System.Drawing.Point(853, 9);
            this.picLoader.Margin = new System.Windows.Forms.Padding(4);
            this.picLoader.Name = "picLoader";
            this.picLoader.Size = new System.Drawing.Size(51, 42);
            this.picLoader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLoader.TabIndex = 2;
            this.picLoader.TabStop = false;
            // 
            // btnImportFile
            // 
            this.btnImportFile.Location = new System.Drawing.Point(189, 10);
            this.btnImportFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnImportFile.Name = "btnImportFile";
            this.btnImportFile.Size = new System.Drawing.Size(215, 28);
            this.btnImportFile.TabIndex = 1;
            this.btnImportFile.Text = "Import FIle to Casket Master";
            this.btnImportFile.UseVisualStyleBackColor = true;
            this.btnImportFile.Click += new System.EventHandler(this.btnImportFile_Click);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(4, 9);
            this.btnSelectFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(100, 28);
            this.btnSelectFile.TabIndex = 0;
            this.btnSelectFile.Text = "Select File";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.importToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1154, 28);
            this.menuStrip1.TabIndex = 3;
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
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnImportBatesville
            // 
            this.btnImportBatesville.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnImportBatesville.Location = new System.Drawing.Point(448, 10);
            this.btnImportBatesville.Margin = new System.Windows.Forms.Padding(4);
            this.btnImportBatesville.Name = "btnImportBatesville";
            this.btnImportBatesville.Size = new System.Drawing.Size(254, 28);
            this.btnImportBatesville.TabIndex = 11;
            this.btnImportBatesville.Text = "Import All to Batesville Table";
            this.btnImportBatesville.UseVisualStyleBackColor = false;
            this.btnImportBatesville.Click += new System.EventHandler(this.btnImportBatesville_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.timCostsToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            this.importToolStripMenuItem.Text = "Import";
            // 
            // timCostsToolStripMenuItem
            // 
            this.timCostsToolStripMenuItem.Name = "timCostsToolStripMenuItem";
            this.timCostsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.timCostsToolStripMenuItem.Text = "Tim Costs";
            this.timCostsToolStripMenuItem.Click += new System.EventHandler(this.timCostsToolStripMenuItem_Click);
            // 
            // ImportCasketCosts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 321);
            this.Controls.Add(this.panelAll);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ImportCasketCosts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Casket Costs";
            this.Load += new System.EventHandler(this.ImportCasketCosts_Load);
            this.panelAll.ResumeLayout(false);
            this.panelAll.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLoader)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private DevExpress.XtraGrid.GridControl dgv;
        private System.Windows.Forms.Button btnImportFile;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView mainGrid;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.PictureBox picLoader;
        private System.Windows.Forms.Label labelMaximum;
        private System.Windows.Forms.ProgressBar barImport;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnTie;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkAllowDuplicates;
        private System.Windows.Forms.PictureBox pictureBox4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private System.Windows.Forms.Button btnImportBatesville;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timCostsToolStripMenuItem;
    }
}