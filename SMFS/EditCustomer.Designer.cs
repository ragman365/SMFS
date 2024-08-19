using DevExpress.XtraBars.Docking;

namespace SMFS
{
    partial class EditCustomer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditCustomer));
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.toolCustomerDemographics = new System.Windows.Forms.ToolStripButton();
            this.toolFamily = new System.Windows.Forms.ToolStripButton();
            this.toolServices = new System.Windows.Forms.ToolStripButton();
            this.toolPayments = new System.Windows.Forms.ToolStripButton();
            this.tblLeft = new System.Windows.Forms.ToolStrip();
            this.toolLegal = new System.Windows.Forms.ToolStripButton();
            this.toolForms = new System.Windows.Forms.ToolStripButton();
            this.panelDesign = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.panelCustomer = new System.Windows.Forms.Panel();
            this.panelFamily = new System.Windows.Forms.Panel();
            this.panelServices = new System.Windows.Forms.Panel();
            this.panelPayments = new System.Windows.Forms.Panel();
            this.panelForms = new System.Windows.Forms.Panel();
            this.panelAll = new EditCustomer.CustomPanel();
            this.panelLegal = new System.Windows.Forms.Panel();
            this.btnAdmin = new System.Windows.Forms.ToolStripButton();
            this.btnContracts = new System.Windows.Forms.ToolStripButton();
            this.btnCustomer = new System.Windows.Forms.ToolStripButton();
            this.tlbMain = new System.Windows.Forms.ToolStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.funeralDirectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.funeralArrangerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.funeralHomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtFuneralHome = new System.Windows.Forms.TextBox();
            this.txtGPLGroup = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCasketGroup = new System.Windows.Forms.TextBox();
            this.lblFuneralDirector = new System.Windows.Forms.Label();
            this.lblFuneralArranger = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.tblLeft.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.panelAll.SuspendLayout();
            this.tlbMain.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockManager1
            // 
            this.dockManager1.Form = this;
            this.dockManager1.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "System.Windows.Forms.MenuStrip",
            "System.Windows.Forms.StatusStrip",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl",
            "DevExpress.XtraBars.Navigation.OfficeNavigationBar",
            "DevExpress.XtraBars.Navigation.TileNavPane",
            "DevExpress.XtraBars.TabFormControl",
            "DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl"});
            // 
            // toolCustomerDemographics
            // 
            this.toolCustomerDemographics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolCustomerDemographics.Image = ((System.Drawing.Image)(resources.GetObject("toolCustomerDemographics.Image")));
            this.toolCustomerDemographics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolCustomerDemographics.Name = "toolCustomerDemographics";
            this.toolCustomerDemographics.Size = new System.Drawing.Size(21, 20);
            this.toolCustomerDemographics.Text = "toolCustomerDemographics";
            this.toolCustomerDemographics.ToolTipText = "Customer Demographics";
            this.toolCustomerDemographics.Click += new System.EventHandler(this.toolCustomerDemographics_Click);
            // 
            // toolFamily
            // 
            this.toolFamily.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolFamily.Image = ((System.Drawing.Image)(resources.GetObject("toolFamily.Image")));
            this.toolFamily.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolFamily.Name = "toolFamily";
            this.toolFamily.Size = new System.Drawing.Size(21, 20);
            this.toolFamily.Text = "Customer Family";
            this.toolFamily.Click += new System.EventHandler(this.toolFamily_Click);
            // 
            // toolServices
            // 
            this.toolServices.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolServices.Image = ((System.Drawing.Image)(resources.GetObject("toolServices.Image")));
            this.toolServices.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolServices.Name = "toolServices";
            this.toolServices.Size = new System.Drawing.Size(21, 20);
            this.toolServices.Text = "Services";
            this.toolServices.Click += new System.EventHandler(this.toolServices_Click);
            // 
            // toolPayments
            // 
            this.toolPayments.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPayments.Image = ((System.Drawing.Image)(resources.GetObject("toolPayments.Image")));
            this.toolPayments.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPayments.Name = "toolPayments";
            this.toolPayments.Size = new System.Drawing.Size(21, 20);
            this.toolPayments.Text = "Payments";
            this.toolPayments.ToolTipText = "Payments";
            this.toolPayments.Click += new System.EventHandler(this.toolPayments_Click);
            // 
            // tblLeft
            // 
            this.tblLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.tblLeft.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolCustomerDemographics,
            this.toolFamily,
            this.toolServices,
            this.toolPayments,
            this.toolLegal,
            this.toolForms});
            this.tblLeft.Location = new System.Drawing.Point(0, 63);
            this.tblLeft.MaximumSize = new System.Drawing.Size(50, 397);
            this.tblLeft.Name = "tblLeft";
            this.tblLeft.Size = new System.Drawing.Size(24, 329);
            this.tblLeft.Stretch = true;
            this.tblLeft.TabIndex = 0;
            this.tblLeft.Text = "toolStrip1";
            // 
            // toolLegal
            // 
            this.toolLegal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolLegal.Image = ((System.Drawing.Image)(resources.GetObject("toolLegal.Image")));
            this.toolLegal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolLegal.Name = "toolLegal";
            this.toolLegal.Size = new System.Drawing.Size(21, 20);
            this.toolLegal.Text = "Funeral Information";
            this.toolLegal.Click += new System.EventHandler(this.toolLegal_Click);
            // 
            // toolForms
            // 
            this.toolForms.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolForms.Image = ((System.Drawing.Image)(resources.GetObject("toolForms.Image")));
            this.toolForms.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolForms.Name = "toolForms";
            this.toolForms.Size = new System.Drawing.Size(21, 20);
            this.toolForms.Tag = "Forms";
            this.toolForms.Text = "Forms";
            this.toolForms.ToolTipText = "Forms";
            this.toolForms.Click += new System.EventHandler(this.toolForms_Click);
            // 
            // panelDesign
            // 
            this.panelDesign.Location = new System.Drawing.Point(3, 3);
            this.panelDesign.Name = "panelDesign";
            this.panelDesign.Size = new System.Drawing.Size(934, 357);
            this.panelDesign.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(1110, 63);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(63, 155);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(55, 129);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(55, 129);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(55, 129);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(55, 129);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(55, 129);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "tabPage5";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // panelCustomer
            // 
            this.panelCustomer.Location = new System.Drawing.Point(960, 6);
            this.panelCustomer.Name = "panelCustomer";
            this.panelCustomer.Size = new System.Drawing.Size(17, 354);
            this.panelCustomer.TabIndex = 1;
            this.panelCustomer.Tag = "myPanel";
            // 
            // panelFamily
            // 
            this.panelFamily.Location = new System.Drawing.Point(1007, 6);
            this.panelFamily.Name = "panelFamily";
            this.panelFamily.Size = new System.Drawing.Size(18, 276);
            this.panelFamily.TabIndex = 0;
            this.panelFamily.Tag = "myPanel";
            // 
            // panelServices
            // 
            this.panelServices.Location = new System.Drawing.Point(1031, 6);
            this.panelServices.Name = "panelServices";
            this.panelServices.Size = new System.Drawing.Size(18, 276);
            this.panelServices.TabIndex = 0;
            this.panelServices.Tag = "myPanel";
            // 
            // panelPayments
            // 
            this.panelPayments.Location = new System.Drawing.Point(1055, 6);
            this.panelPayments.Name = "panelPayments";
            this.panelPayments.Size = new System.Drawing.Size(16, 276);
            this.panelPayments.TabIndex = 0;
            this.panelPayments.Tag = "myPanel";
            // 
            // panelForms
            // 
            this.panelForms.Location = new System.Drawing.Point(1077, 6);
            this.panelForms.Name = "panelForms";
            this.panelForms.Size = new System.Drawing.Size(14, 467);
            this.panelForms.TabIndex = 0;
            this.panelForms.Tag = "myPanel";
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelLegal);
            this.panelAll.Controls.Add(this.panelForms);
            this.panelAll.Controls.Add(this.panelCustomer);
            this.panelAll.Controls.Add(this.panelFamily);
            this.panelAll.Controls.Add(this.panelPayments);
            this.panelAll.Controls.Add(this.panelDesign);
            this.panelAll.Controls.Add(this.panelServices);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(24, 63);
            this.panelAll.MaximumSize = new System.Drawing.Size(0, 5000);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(1161, 329);
            this.panelAll.TabIndex = 6;
            this.panelAll.Paint += new System.Windows.Forms.PaintEventHandler(this.panelAll_Paint);
            // 
            // panelLegal
            // 
            this.panelLegal.Location = new System.Drawing.Point(1106, 6);
            this.panelLegal.Name = "panelLegal";
            this.panelLegal.Size = new System.Drawing.Size(18, 276);
            this.panelLegal.TabIndex = 3;
            this.panelLegal.Tag = "myPanel";
            // 
            // btnAdmin
            // 
            this.btnAdmin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAdmin.Image = ((System.Drawing.Image)(resources.GetObject("btnAdmin.Image")));
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(36, 36);
            this.btnAdmin.Tag = "ADMIN";
            this.btnAdmin.ToolTipText = "Administrative Tasks";
            // 
            // btnContracts
            // 
            this.btnContracts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnContracts.Image = ((System.Drawing.Image)(resources.GetObject("btnContracts.Image")));
            this.btnContracts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnContracts.Name = "btnContracts";
            this.btnContracts.Size = new System.Drawing.Size(36, 36);
            this.btnContracts.Tag = "New Contract";
            this.btnContracts.Text = "New Contract";
            this.btnContracts.Click += new System.EventHandler(this.btnContracts_Click);
            // 
            // btnCustomer
            // 
            this.btnCustomer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCustomer.Image = ((System.Drawing.Image)(resources.GetObject("btnCustomer.Image")));
            this.btnCustomer.Name = "btnCustomer";
            this.btnCustomer.Size = new System.Drawing.Size(36, 36);
            this.btnCustomer.Tag = "Find Customer";
            this.btnCustomer.Text = "Find Customer";
            this.btnCustomer.ToolTipText = "Find Existing Customer";
            this.btnCustomer.Click += new System.EventHandler(this.btnCustomer_Click);
            // 
            // tlbMain
            // 
            this.tlbMain.BackColor = System.Drawing.SystemColors.Control;
            this.tlbMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tlbMain.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.tlbMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAdmin,
            this.btnContracts,
            this.btnCustomer});
            this.tlbMain.Location = new System.Drawing.Point(0, 24);
            this.tlbMain.Name = "tlbMain";
            this.tlbMain.Size = new System.Drawing.Size(1185, 39);
            this.tlbMain.TabIndex = 5;
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
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.funeralDirectorToolStripMenuItem,
            this.funeralArrangerToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // funeralDirectorToolStripMenuItem
            // 
            this.funeralDirectorToolStripMenuItem.Name = "funeralDirectorToolStripMenuItem";
            this.funeralDirectorToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.funeralDirectorToolStripMenuItem.Text = "Funeral Director";
            this.funeralDirectorToolStripMenuItem.Click += new System.EventHandler(this.funeralDirectorToolStripMenuItem_Click);
            // 
            // funeralArrangerToolStripMenuItem
            // 
            this.funeralArrangerToolStripMenuItem.Name = "funeralArrangerToolStripMenuItem";
            this.funeralArrangerToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.funeralArrangerToolStripMenuItem.Text = "Funeral Arranger";
            this.funeralArrangerToolStripMenuItem.Click += new System.EventHandler(this.funeralArrangerToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.selectToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1185, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.funeralHomeToolStripMenuItem});
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.selectToolStripMenuItem.Text = "Select";
            // 
            // funeralHomeToolStripMenuItem
            // 
            this.funeralHomeToolStripMenuItem.Name = "funeralHomeToolStripMenuItem";
            this.funeralHomeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.funeralHomeToolStripMenuItem.Text = "Funeral Home";
            this.funeralHomeToolStripMenuItem.Click += new System.EventHandler(this.funeralHomeToolStripMenuItem_Click);
            // 
            // txtFuneralHome
            // 
            this.txtFuneralHome.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFuneralHome.Enabled = false;
            this.txtFuneralHome.Location = new System.Drawing.Point(508, 35);
            this.txtFuneralHome.Name = "txtFuneralHome";
            this.txtFuneralHome.Size = new System.Drawing.Size(173, 21);
            this.txtFuneralHome.TabIndex = 7;
            // 
            // txtGPLGroup
            // 
            this.txtGPLGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGPLGroup.Enabled = false;
            this.txtGPLGroup.Location = new System.Drawing.Point(760, 34);
            this.txtGPLGroup.Name = "txtGPLGroup";
            this.txtGPLGroup.Size = new System.Drawing.Size(98, 21);
            this.txtGPLGroup.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(428, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Funeral Home :";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(694, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "GPL Group :";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(868, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Casket Group :";
            // 
            // txtCasketGroup
            // 
            this.txtCasketGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCasketGroup.Enabled = false;
            this.txtCasketGroup.Location = new System.Drawing.Point(947, 34);
            this.txtCasketGroup.Name = "txtCasketGroup";
            this.txtCasketGroup.Size = new System.Drawing.Size(133, 21);
            this.txtCasketGroup.TabIndex = 12;
            // 
            // lblFuneralDirector
            // 
            this.lblFuneralDirector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFuneralDirector.AutoSize = true;
            this.lblFuneralDirector.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFuneralDirector.Location = new System.Drawing.Point(163, 33);
            this.lblFuneralDirector.Name = "lblFuneralDirector";
            this.lblFuneralDirector.Size = new System.Drawing.Size(71, 11);
            this.lblFuneralDirector.TabIndex = 13;
            this.lblFuneralDirector.Text = "Funeral Director";
            // 
            // lblFuneralArranger
            // 
            this.lblFuneralArranger.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFuneralArranger.AutoSize = true;
            this.lblFuneralArranger.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFuneralArranger.Location = new System.Drawing.Point(163, 45);
            this.lblFuneralArranger.Name = "lblFuneralArranger";
            this.lblFuneralArranger.Size = new System.Drawing.Size(75, 11);
            this.lblFuneralArranger.TabIndex = 14;
            this.lblFuneralArranger.Text = "Funeral Arranger";
            // 
            // EditCustomer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1185, 392);
            this.Controls.Add(this.lblFuneralArranger);
            this.Controls.Add(this.lblFuneralDirector);
            this.Controls.Add(this.txtCasketGroup);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtGPLGroup);
            this.Controls.Add(this.txtFuneralHome);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.tblLeft);
            this.Controls.Add(this.tlbMain);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.tabControl1);
            this.Name = "EditCustomer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EditCustomer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditCustomer_FormClosing);
            this.Load += new System.EventHandler(this.EditCustomer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.tblLeft.ResumeLayout(false);
            this.tblLeft.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.panelAll.ResumeLayout(false);
            this.tlbMain.ResumeLayout(false);
            this.tlbMain.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DockManager dockManager1;
        private CustomPanel panelAll;
//        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelDesign;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panelCustomer;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panelFamily;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel panelServices;
        private System.Windows.Forms.ToolStrip tblLeft;
        private System.Windows.Forms.ToolStripButton toolCustomerDemographics;
        private System.Windows.Forms.ToolStripButton toolFamily;
        private System.Windows.Forms.ToolStripButton toolServices;
        private System.Windows.Forms.ToolStripButton toolPayments;
        private System.Windows.Forms.ToolStrip tlbMain;
        private System.Windows.Forms.ToolStripButton btnAdmin;
        private System.Windows.Forms.ToolStripButton btnContracts;
        private System.Windows.Forms.ToolStripButton btnCustomer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolLegal;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Panel panelPayments;
        private System.Windows.Forms.ToolStripButton toolForms;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Panel panelForms;
        private System.Windows.Forms.TextBox txtFuneralHome;
        private System.Windows.Forms.TextBox txtGPLGroup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCasketGroup;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelLegal;
        private System.Windows.Forms.ToolStripMenuItem funeralDirectorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem funeralArrangerToolStripMenuItem;
        private System.Windows.Forms.Label lblFuneralArranger;
        private System.Windows.Forms.Label lblFuneralDirector;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem funeralHomeToolStripMenuItem;
    }
}