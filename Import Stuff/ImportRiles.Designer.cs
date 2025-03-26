namespace SMFS
{
    partial class ImportRiles
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportRiles));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.mainGrid = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
            this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.panelTop = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.labelMaximum = new System.Windows.Forms.Label();
            this.barImport = new System.Windows.Forms.ProgressBar();
            this.picLoader = new System.Windows.Forms.PictureBox();
            this.btnImportRiles = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLoader)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(752, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printPreviewToolStripMenuItem,
            this.printToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(99, 22);
            this.toolStripMenuItem1.Text = "Print";
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
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 24);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(752, 244);
            this.panelAll.TabIndex = 5;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 61);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(752, 183);
            this.panelBottom.TabIndex = 7;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.MainView = this.mainGrid;
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(752, 183);
            this.dgv.TabIndex = 1;
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
            this.gridBand2,
            this.gridBand1});
            this.mainGrid.GridControl = this.dgv;
            this.mainGrid.Name = "mainGrid";
            this.mainGrid.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.mainGrid.OptionsPrint.PrintBandHeader = false;
            this.mainGrid.OptionsView.EnableAppearanceEvenRow = true;
            this.mainGrid.OptionsView.EnableAppearanceOddRow = true;
            this.mainGrid.OptionsView.ShowGroupPanel = false;
            // 
            // gridBand2
            // 
            this.gridBand2.Name = "gridBand2";
            this.gridBand2.VisibleIndex = 0;
            // 
            // gridBand1
            // 
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 1;
            this.gridBand1.Width = 279;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.pictureBox1);
            this.panelTop.Controls.Add(this.lblTotal);
            this.panelTop.Controls.Add(this.labelMaximum);
            this.panelTop.Controls.Add(this.barImport);
            this.panelTop.Controls.Add(this.picLoader);
            this.panelTop.Controls.Add(this.btnImportRiles);
            this.panelTop.Controls.Add(this.btnSelectFile);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(752, 61);
            this.panelTop.TabIndex = 6;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(472, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(27, 23);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(469, 41);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(35, 13);
            this.lblTotal.TabIndex = 14;
            this.lblTotal.Text = "label1";
            // 
            // labelMaximum
            // 
            this.labelMaximum.AutoSize = true;
            this.labelMaximum.Location = new System.Drawing.Point(393, 41);
            this.labelMaximum.Name = "labelMaximum";
            this.labelMaximum.Size = new System.Drawing.Size(35, 13);
            this.labelMaximum.TabIndex = 13;
            this.labelMaximum.Text = "label1";
            // 
            // barImport
            // 
            this.barImport.BackColor = System.Drawing.Color.Lime;
            this.barImport.Location = new System.Drawing.Point(9, 41);
            this.barImport.Name = "barImport";
            this.barImport.Size = new System.Drawing.Size(364, 12);
            this.barImport.TabIndex = 12;
            // 
            // picLoader
            // 
            this.picLoader.Image = ((System.Drawing.Image)(resources.GetObject("picLoader.Image")));
            this.picLoader.Location = new System.Drawing.Point(390, 2);
            this.picLoader.Name = "picLoader";
            this.picLoader.Size = new System.Drawing.Size(38, 34);
            this.picLoader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLoader.TabIndex = 11;
            this.picLoader.TabStop = false;
            // 
            // btnImportRiles
            // 
            this.btnImportRiles.Location = new System.Drawing.Point(238, 8);
            this.btnImportRiles.Name = "btnImportRiles";
            this.btnImportRiles.Size = new System.Drawing.Size(135, 23);
            this.btnImportRiles.TabIndex = 10;
            this.btnImportRiles.Text = "Import Riles Data";
            this.btnImportRiles.UseVisualStyleBackColor = true;
            this.btnImportRiles.Click += new System.EventHandler(this.btnImportRiles_Click);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(9, 6);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFile.TabIndex = 9;
            this.btnSelectFile.Text = "Select File";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // ImportRiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 268);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.menuStrip1);
            this.Name = "ImportRiles";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Riles Data";
            this.Load += new System.EventHandler(this.ImportRiles_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLoader)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView mainGrid;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand2;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label labelMaximum;
        private System.Windows.Forms.ProgressBar barImport;
        private System.Windows.Forms.PictureBox picLoader;
        private System.Windows.Forms.Button btnImportRiles;
        private System.Windows.Forms.Button btnSelectFile;
    }
}