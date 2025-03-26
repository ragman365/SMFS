namespace SMFS
{
    partial class Messages
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Messages));
            DevExpress.Utils.ContextButton contextButton1 = new DevExpress.Utils.ContextButton();
            this.tsMessages = new System.Windows.Forms.ToolStrip();
            this.tsbNewMessage = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbReply = new System.Windows.Forms.ToolStripButton();
            this.tsbForward = new System.Windows.Forms.ToolStripButton();
            this.tsbDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsbPrint = new System.Windows.Forms.ToolStripButton();
            this.tsbTickle = new System.Windows.Forms.ToolStripButton();
            this.lblTimeSent = new System.Windows.Forms.Label();
            this.panelAll = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelTop = new System.Windows.Forms.Panel();
            this.dgv = new DevExpress.XtraGrid.GridControl();
            this.gridMain = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
            this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.Num = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.attachment = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.repButton = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            this.repCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.repositoryItemPictureEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.rtb2 = new DevExpress.XtraRichEdit.RichEditControl();
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.panelMiddle = new System.Windows.Forms.Panel();
            this.btnDiscard = new System.Windows.Forms.Button();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.chkComboLocNames = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.FromLbl = new System.Windows.Forms.Label();
            this.sentoLbl = new System.Windows.Forms.Label();
            this.lblFrom = new System.Windows.Forms.Label();
            this.subjectLbl = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbBoxes = new System.Windows.Forms.ComboBox();
            this.cmbUsers = new System.Windows.Forms.ComboBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.picPaperClip = new System.Windows.Forms.PictureBox();
            this.tsMessages.SuspendLayout();
            this.panelAll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.panelMiddle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkComboLocNames.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPaperClip)).BeginInit();
            this.SuspendLayout();
            // 
            // tsMessages
            // 
            this.tsMessages.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNewMessage,
            this.toolStripButton1,
            this.tsbReply,
            this.tsbForward,
            this.tsbDelete,
            this.toolStripSeparator1,
            this.tsbRefresh,
            this.tsbPrint,
            this.tsbTickle});
            this.tsMessages.Location = new System.Drawing.Point(0, 0);
            this.tsMessages.Name = "tsMessages";
            this.tsMessages.Size = new System.Drawing.Size(933, 31);
            this.tsMessages.TabIndex = 31;
            this.tsMessages.Text = "toolStrip1";
            // 
            // tsbNewMessage
            // 
            this.tsbNewMessage.Image = ((System.Drawing.Image)(resources.GetObject("tsbNewMessage.Image")));
            this.tsbNewMessage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbNewMessage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNewMessage.Name = "tsbNewMessage";
            this.tsbNewMessage.Size = new System.Drawing.Size(108, 28);
            this.tsbNewMessage.Text = "New Message";
            this.tsbNewMessage.Click += new System.EventHandler(this.tsbNewMessage_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbReply
            // 
            this.tsbReply.Image = ((System.Drawing.Image)(resources.GetObject("tsbReply.Image")));
            this.tsbReply.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbReply.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbReply.Name = "tsbReply";
            this.tsbReply.Size = new System.Drawing.Size(64, 28);
            this.tsbReply.Text = "Reply";
            this.tsbReply.Click += new System.EventHandler(this.tsbReply_Click);
            // 
            // tsbForward
            // 
            this.tsbForward.Image = ((System.Drawing.Image)(resources.GetObject("tsbForward.Image")));
            this.tsbForward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbForward.Name = "tsbForward";
            this.tsbForward.Size = new System.Drawing.Size(78, 28);
            this.tsbForward.Text = "Forward";
            // 
            // tsbDelete
            // 
            this.tsbDelete.Image = ((System.Drawing.Image)(resources.GetObject("tsbDelete.Image")));
            this.tsbDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDelete.Name = "tsbDelete";
            this.tsbDelete.Size = new System.Drawing.Size(68, 28);
            this.tsbDelete.Text = "Delete";
            this.tsbDelete.Click += new System.EventHandler(this.tsbDelete_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.Image = ((System.Drawing.Image)(resources.GetObject("tsbRefresh.Image")));
            this.tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Size = new System.Drawing.Size(66, 28);
            this.tsbRefresh.Text = "Refresh";
            this.tsbRefresh.Click += new System.EventHandler(this.tsbRefresh_Click);
            // 
            // tsbPrint
            // 
            this.tsbPrint.Image = ((System.Drawing.Image)(resources.GetObject("tsbPrint.Image")));
            this.tsbPrint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPrint.Name = "tsbPrint";
            this.tsbPrint.Size = new System.Drawing.Size(52, 28);
            this.tsbPrint.Text = "Print";
            this.tsbPrint.Click += new System.EventHandler(this.tsbPrint_Click);
            // 
            // tsbTickle
            // 
            this.tsbTickle.Image = ((System.Drawing.Image)(resources.GetObject("tsbTickle.Image")));
            this.tsbTickle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbTickle.Name = "tsbTickle";
            this.tsbTickle.Size = new System.Drawing.Size(57, 28);
            this.tsbTickle.Text = "Tickle";
            this.tsbTickle.Click += new System.EventHandler(this.tsbTickle_Click);
            // 
            // lblTimeSent
            // 
            this.lblTimeSent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTimeSent.Location = new System.Drawing.Point(833, 142);
            this.lblTimeSent.Name = "lblTimeSent";
            this.lblTimeSent.Size = new System.Drawing.Size(160, 13);
            this.lblTimeSent.TabIndex = 40;
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.splitContainer1);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 31);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(933, 475);
            this.panelAll.TabIndex = 41;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelTop);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelBottom);
            this.splitContainer1.Panel2.Controls.Add(this.panelMiddle);
            this.splitContainer1.Size = new System.Drawing.Size(933, 475);
            this.splitContainer1.SplitterDistance = 224;
            this.splitContainer1.TabIndex = 3;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.dgv);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(933, 224);
            this.panelTop.TabIndex = 0;
            // 
            // dgv
            // 
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.LookAndFeel.SkinName = "Glass Oceans";
            this.dgv.LookAndFeel.UseDefaultLookAndFeel = false;
            this.dgv.MainView = this.gridMain;
            this.dgv.Name = "dgv";
            this.dgv.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repButton,
            this.repCheckEdit1,
            this.repositoryItemPictureEdit1});
            this.dgv.Size = new System.Drawing.Size(933, 224);
            this.dgv.TabIndex = 5;
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
            this.gridMain.Appearance.FocusedRow.BackColor = System.Drawing.Color.Yellow;
            this.gridMain.Appearance.FocusedRow.ForeColor = System.Drawing.Color.Black;
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
            this.gridMain.Appearance.Row.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.Row.Options.UseBackColor = true;
            this.gridMain.Appearance.Row.Options.UseForeColor = true;
            this.gridMain.Appearance.RowSeparator.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.RowSeparator.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
            this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
            this.gridMain.Appearance.SelectedRow.Options.UseBackColor = true;
            this.gridMain.Appearance.SelectedRow.Options.UseForeColor = true;
            this.gridMain.Appearance.TopNewRow.BackColor = System.Drawing.Color.White;
            this.gridMain.Appearance.TopNewRow.Options.UseBackColor = true;
            this.gridMain.Appearance.VertLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(160)))), ((int)(((byte)(188)))));
            this.gridMain.Appearance.VertLine.Options.UseBackColor = true;
            this.gridMain.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand1});
            this.gridMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.gridMain.ColumnPanelRowHeight = 5;
            this.gridMain.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.Num,
            this.bandedGridColumn1,
            this.bandedGridColumn5,
            this.bandedGridColumn4,
            this.bandedGridColumn7,
            this.bandedGridColumn2,
            this.bandedGridColumn3,
            this.bandedGridColumn6,
            this.attachment});
            this.gridMain.GridControl = this.dgv;
            this.gridMain.Name = "gridMain";
            this.gridMain.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            this.gridMain.OptionsNavigation.EnterMoveNextColumn = true;
            this.gridMain.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            this.gridMain.OptionsView.EnableAppearanceEvenRow = true;
            this.gridMain.OptionsView.ShowBands = false;
            this.gridMain.OptionsView.ShowFooter = true;
            this.gridMain.OptionsView.ShowGroupPanel = false;
            this.gridMain.PaintStyleName = "Style3D";
            this.gridMain.RowHeight = 1;
            this.gridMain.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridMain_RowClick);
            this.gridMain.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridMain_CustomDrawCell);
            this.gridMain.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridMain_FocusedRowChanged);
            // 
            // gridBand1
            // 
            this.gridBand1.Caption = "gridBand1";
            this.gridBand1.Columns.Add(this.Num);
            this.gridBand1.Columns.Add(this.bandedGridColumn7);
            this.gridBand1.Columns.Add(this.bandedGridColumn5);
            this.gridBand1.Columns.Add(this.bandedGridColumn4);
            this.gridBand1.Columns.Add(this.bandedGridColumn2);
            this.gridBand1.Columns.Add(this.attachment);
            this.gridBand1.Columns.Add(this.bandedGridColumn1);
            this.gridBand1.Name = "gridBand1";
            this.gridBand1.VisibleIndex = 0;
            this.gridBand1.Width = 750;
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
            // bandedGridColumn7
            // 
            this.bandedGridColumn7.Caption = "Date";
            this.bandedGridColumn7.FieldName = "senddate";
            this.bandedGridColumn7.Name = "bandedGridColumn7";
            this.bandedGridColumn7.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn7.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn7.Visible = true;
            // 
            // bandedGridColumn5
            // 
            this.bandedGridColumn5.Caption = "From";
            this.bandedGridColumn5.FieldName = "fromUser";
            this.bandedGridColumn5.Name = "bandedGridColumn5";
            this.bandedGridColumn5.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn5.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn5.Visible = true;
            this.bandedGridColumn5.Width = 100;
            // 
            // bandedGridColumn4
            // 
            this.bandedGridColumn4.Caption = "To";
            this.bandedGridColumn4.FieldName = "toUser";
            this.bandedGridColumn4.Name = "bandedGridColumn4";
            this.bandedGridColumn4.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn4.OptionsColumn.FixedWidth = true;
            this.bandedGridColumn4.Visible = true;
            this.bandedGridColumn4.Width = 150;
            // 
            // bandedGridColumn2
            // 
            this.bandedGridColumn2.Caption = "Subject";
            this.bandedGridColumn2.FieldName = "subject";
            this.bandedGridColumn2.Name = "bandedGridColumn2";
            this.bandedGridColumn2.OptionsColumn.AllowEdit = false;
            this.bandedGridColumn2.Visible = true;
            this.bandedGridColumn2.Width = 300;
            // 
            // attachment
            // 
            this.attachment.Caption = "Attachment";
            this.attachment.FieldName = "attachment";
            this.attachment.Name = "attachment";
            this.attachment.Visible = true;
            // 
            // bandedGridColumn1
            // 
            this.bandedGridColumn1.Caption = "record";
            this.bandedGridColumn1.FieldName = "record";
            this.bandedGridColumn1.Name = "bandedGridColumn1";
            // 
            // bandedGridColumn3
            // 
            this.bandedGridColumn3.Caption = "From Record";
            this.bandedGridColumn3.FieldName = "fromRecord";
            this.bandedGridColumn3.Name = "bandedGridColumn3";
            // 
            // bandedGridColumn6
            // 
            this.bandedGridColumn6.Caption = "To Record";
            this.bandedGridColumn6.FieldName = "toRecord";
            this.bandedGridColumn6.Name = "bandedGridColumn6";
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
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.rtb2);
            this.panelBottom.Controls.Add(this.rtb);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 79);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(933, 168);
            this.panelBottom.TabIndex = 2;
            // 
            // rtb2
            // 
            this.rtb2.ActiveViewType = DevExpress.XtraRichEdit.RichEditViewType.Simple;
            this.rtb2.Appearance.Text.Font = new System.Drawing.Font("Times New Roman", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb2.Appearance.Text.Options.UseFont = true;
            this.rtb2.EnableToolTips = false;
            this.rtb2.LayoutUnit = DevExpress.XtraRichEdit.DocumentLayoutUnit.Pixel;
            this.rtb2.Location = new System.Drawing.Point(443, 10);
            this.rtb2.Name = "rtb2";
            this.rtb2.Size = new System.Drawing.Size(315, 119);
            this.rtb2.TabIndex = 2;
            this.rtb2.Views.PrintLayoutView.AllowDisplayLineNumbers = false;
            this.rtb2.Visible = false;
            // 
            // rtb
            // 
            this.rtb.Location = new System.Drawing.Point(67, 34);
            this.rtb.Name = "rtb";
            this.rtb.Size = new System.Drawing.Size(168, 75);
            this.rtb.TabIndex = 0;
            this.rtb.Text = "";
            this.rtb.DoubleClick += new System.EventHandler(this.rtb_DoubleClick);
            // 
            // panelMiddle
            // 
            this.panelMiddle.Controls.Add(this.picPaperClip);
            this.panelMiddle.Controls.Add(this.btnDiscard);
            this.panelMiddle.Controls.Add(this.txtSubject);
            this.panelMiddle.Controls.Add(this.btnSend);
            this.panelMiddle.Controls.Add(this.chkComboLocNames);
            this.panelMiddle.Controls.Add(this.FromLbl);
            this.panelMiddle.Controls.Add(this.sentoLbl);
            this.panelMiddle.Controls.Add(this.lblFrom);
            this.panelMiddle.Controls.Add(this.subjectLbl);
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMiddle.Location = new System.Drawing.Point(0, 0);
            this.panelMiddle.Name = "panelMiddle";
            this.panelMiddle.Size = new System.Drawing.Size(933, 79);
            this.panelMiddle.TabIndex = 1;
            // 
            // btnDiscard
            // 
            this.btnDiscard.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDiscard.Location = new System.Drawing.Point(683, 23);
            this.btnDiscard.Name = "btnDiscard";
            this.btnDiscard.Size = new System.Drawing.Size(75, 42);
            this.btnDiscard.TabIndex = 39;
            this.btnDiscard.Text = "Discard";
            this.btnDiscard.UseVisualStyleBackColor = true;
            this.btnDiscard.Click += new System.EventHandler(this.btnDiscard_Click);
            // 
            // txtSubject
            // 
            this.txtSubject.Location = new System.Drawing.Point(61, 44);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(477, 21);
            this.txtSubject.TabIndex = 38;
            // 
            // btnSend
            // 
            this.btnSend.Font = new System.Drawing.Font("Tahoma", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(544, 23);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 42);
            this.btnSend.TabIndex = 37;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // chkComboLocNames
            // 
            this.chkComboLocNames.EditValue = "";
            this.chkComboLocNames.Location = new System.Drawing.Point(61, 25);
            this.chkComboLocNames.Name = "chkComboLocNames";
            this.chkComboLocNames.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.chkComboLocNames.Properties.DisplayMember = "name";
            this.chkComboLocNames.Properties.SeparatorChar = '|';
            this.chkComboLocNames.Size = new System.Drawing.Size(477, 20);
            this.chkComboLocNames.TabIndex = 36;
            this.chkComboLocNames.EditValueChanged += new System.EventHandler(this.chkComboLocNames_EditValueChanged);
            // 
            // FromLbl
            // 
            this.FromLbl.AutoSize = true;
            this.FromLbl.Location = new System.Drawing.Point(19, 7);
            this.FromLbl.Name = "FromLbl";
            this.FromLbl.Size = new System.Drawing.Size(38, 13);
            this.FromLbl.TabIndex = 32;
            this.FromLbl.Text = "From :";
            // 
            // sentoLbl
            // 
            this.sentoLbl.AutoSize = true;
            this.sentoLbl.Location = new System.Drawing.Point(29, 27);
            this.sentoLbl.Name = "sentoLbl";
            this.sentoLbl.Size = new System.Drawing.Size(26, 13);
            this.sentoLbl.TabIndex = 31;
            this.sentoLbl.Text = "To :";
            // 
            // lblFrom
            // 
            this.lblFrom.Location = new System.Drawing.Point(64, 9);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(297, 13);
            this.lblFrom.TabIndex = 33;
            // 
            // subjectLbl
            // 
            this.subjectLbl.AutoSize = true;
            this.subjectLbl.Location = new System.Drawing.Point(6, 47);
            this.subjectLbl.Name = "subjectLbl";
            this.subjectLbl.Size = new System.Drawing.Size(50, 13);
            this.subjectLbl.TabIndex = 30;
            this.subjectLbl.Text = "Subject :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(548, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 42;
            this.label1.Text = "Box :";
            // 
            // cmbBoxes
            // 
            this.cmbBoxes.FormattingEnabled = true;
            this.cmbBoxes.Items.AddRange(new object[] {
            "Messages Received",
            "Sent Messages",
            "Deleted Messages",
            "Tickles"});
            this.cmbBoxes.Location = new System.Drawing.Point(580, 6);
            this.cmbBoxes.Name = "cmbBoxes";
            this.cmbBoxes.Size = new System.Drawing.Size(121, 21);
            this.cmbBoxes.TabIndex = 43;
            this.cmbBoxes.Text = "Messages Received";
            this.cmbBoxes.SelectedIndexChanged += new System.EventHandler(this.cmbBoxes_SelectedIndexChanged);
            // 
            // cmbUsers
            // 
            this.cmbUsers.DisplayMember = "name";
            this.cmbUsers.FormattingEnabled = true;
            this.cmbUsers.Location = new System.Drawing.Point(710, 6);
            this.cmbUsers.Name = "cmbUsers";
            this.cmbUsers.Size = new System.Drawing.Size(121, 21);
            this.cmbUsers.TabIndex = 44;
            this.cmbUsers.SelectedIndexChanged += new System.EventHandler(this.cmbUsers_SelectedIndexChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 60000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // picPaperClip
            // 
            this.picPaperClip.Image = ((System.Drawing.Image)(resources.GetObject("picPaperClip.Image")));
            this.picPaperClip.InitialImage = ((System.Drawing.Image)(resources.GetObject("picPaperClip.InitialImage")));
            this.picPaperClip.Location = new System.Drawing.Point(783, 5);
            this.picPaperClip.Name = "picPaperClip";
            this.picPaperClip.Size = new System.Drawing.Size(72, 68);
            this.picPaperClip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPaperClip.TabIndex = 40;
            this.picPaperClip.TabStop = false;
            this.picPaperClip.Click += new System.EventHandler(this.picPaperClip_Click);
            // 
            // Messages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 506);
            this.Controls.Add(this.cmbUsers);
            this.Controls.Add(this.cmbBoxes);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.lblTimeSent);
            this.Controls.Add(this.tsMessages);
            this.Name = "Messages";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Messages";
            this.Load += new System.EventHandler(this.Messages_Load);
            this.tsMessages.ResumeLayout(false);
            this.tsMessages.PerformLayout();
            this.panelAll.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPictureEdit1)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelMiddle.ResumeLayout(false);
            this.panelMiddle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkComboLocNames.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPaperClip)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsMessages;
        private System.Windows.Forms.ToolStripButton tsbNewMessage;
        private System.Windows.Forms.ToolStripSeparator toolStripButton1;
        private System.Windows.Forms.ToolStripButton tsbReply;
        private System.Windows.Forms.ToolStripButton tsbForward;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbPrint;
        private System.Windows.Forms.ToolStripButton tsbTickle;
        private System.Windows.Forms.Label lblTimeSent;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.RichTextBox rtb;
        private System.Windows.Forms.Panel panelMiddle;
        private System.Windows.Forms.Label FromLbl;
        private System.Windows.Forms.Label sentoLbl;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label subjectLbl;
        private System.Windows.Forms.Panel panelTop;
        private DevExpress.XtraGrid.GridControl dgv;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gridMain;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn Num;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit repButton;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repCheckEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryItemPictureEdit1;
        private DevExpress.XtraEditors.CheckedComboBoxEdit chkComboLocNames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbBoxes;
        private System.Windows.Forms.Button btnSend;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private System.Windows.Forms.TextBox txtSubject;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private System.Windows.Forms.Button btnDiscard;
        private System.Windows.Forms.ComboBox cmbUsers;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private DevExpress.XtraRichEdit.RichEditControl rtb2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn attachment;
        private System.Windows.Forms.PictureBox picPaperClip;
    }
}