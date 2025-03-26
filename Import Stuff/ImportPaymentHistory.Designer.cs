namespace SMFS
{
    partial class ImportPaymentHistory
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportPaymentHistory));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.mainGrid = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.panelTop = new System.Windows.Forms.Panel();
            this.picLoader = new System.Windows.Forms.PictureBox();
            this.btnImportFile = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.barImport = new System.Windows.Forms.ProgressBar();
            this.labelMaximum = new System.Windows.Forms.Label();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).BeginInit();
            this.panelTop.SuspendLayout();
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
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(539, 306);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.dgv);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 88);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(539, 218);
            this.panelBottom.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.MainView = this.mainGrid;
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(539, 218);
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
            this.mainGrid.GridControl = this.dgv;
            this.mainGrid.Name = "mainGrid";
            this.mainGrid.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.mainGrid.OptionsView.ColumnAutoWidth = false;
            this.mainGrid.OptionsView.EnableAppearanceEvenRow = true;
            this.mainGrid.OptionsView.EnableAppearanceOddRow = true;
            this.mainGrid.OptionsView.ShowBands = false;
            this.mainGrid.OptionsView.ShowGroupPanel = false;
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 279;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.labelMaximum);
            this.panelTop.Controls.Add(this.barImport);
            this.panelTop.Controls.Add(this.picLoader);
            this.panelTop.Controls.Add(this.btnImportFile);
            this.panelTop.Controls.Add(this.btnSelectFile);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 24);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(539, 64);
            this.panelTop.TabIndex = 1;
            // 
            // picLoader
            // 
            this.picLoader.Image = ((System.Drawing.Image)(resources.GetObject("picLoader.Image")));
            this.picLoader.Location = new System.Drawing.Point(384, 3);
            this.picLoader.Name = "picLoader";
            this.picLoader.Size = new System.Drawing.Size(38, 34);
            this.picLoader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLoader.TabIndex = 2;
            this.picLoader.TabStop = false;
            // 
            // btnImportFile
            // 
            this.btnImportFile.Location = new System.Drawing.Point(232, 9);
            this.btnImportFile.Name = "btnImportFile";
            this.btnImportFile.Size = new System.Drawing.Size(135, 23);
            this.btnImportFile.TabIndex = 1;
            this.btnImportFile.Text = "Import FIle to Database";
            this.btnImportFile.UseVisualStyleBackColor = true;
            this.btnImportFile.Click += new System.EventHandler(this.btnImportFile_Click);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(3, 7);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFile.TabIndex = 0;
            this.btnSelectFile.Text = "Select File";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(539, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // barImport
            // 
            this.barImport.BackColor = System.Drawing.Color.Lime;
            this.barImport.Location = new System.Drawing.Point(3, 41);
            this.barImport.Name = "barImport";
            this.barImport.Size = new System.Drawing.Size(364, 12);
            this.barImport.TabIndex = 3;
            // 
            // labelMaximum
            // 
            this.labelMaximum.AutoSize = true;
            this.labelMaximum.Location = new System.Drawing.Point(387, 41);
            this.labelMaximum.Name = "labelMaximum";
            this.labelMaximum.Size = new System.Drawing.Size(35, 13);
            this.labelMaximum.TabIndex = 4;
            this.labelMaximum.Text = "label1";
            // 
            // ImportPaymentHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 306);
            this.Controls.Add(this.panelAll);
            this.Name = "ImportPaymentHistory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Payment History";
            this.Load += new System.EventHandler(this.Import_Load);
            this.panelAll.ResumeLayout(false);
            this.panelAll.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mainGrid)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
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
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.PictureBox picLoader;
        private System.Windows.Forms.ProgressBar barImport;
        private System.Windows.Forms.Label labelMaximum;
    }
}