namespace SMFS
{
    partial class Merchandise
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Merchandise));
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.importClipBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAssign = new System.Windows.Forms.Panel();
            this.btnSelectCustomer = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.deceasedDate = new DevExpress.XtraEditors.DateEdit();
            this.txtServiceID = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.usedDate = new DevExpress.XtraEditors.DateEdit();
            this.panelMiddle = new System.Windows.Forms.Panel();
            this.cmbOwner = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.receivedDate = new DevExpress.XtraEditors.DateEdit();
            this.txtSerialNumber = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbLocation = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnChange = new System.Windows.Forms.Button();
            this.txtBatesville = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.btnDetach = new System.Windows.Forms.Button();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtType = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtGuage = new System.Windows.Forms.TextBox();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.rtbPrice = new System.Windows.Forms.RichTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.rtbDesc = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameCasketDescriptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.panelAssign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.deceasedDate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deceasedDate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.usedDate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.usedDate.Properties)).BeginInit();
            this.panelMiddle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.receivedDate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.receivedDate.Properties)).BeginInit();
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
            this.panelAll.Size = new System.Drawing.Size(586, 686);
            this.panelAll.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.pictureBox1);
            this.panelBottom.Controls.Add(this.panelAssign);
            this.panelBottom.Controls.Add(this.panelMiddle);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 188);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(586, 498);
            this.panelBottom.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 144);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(586, 354);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.PictureBox1_DragDrop);
            this.pictureBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.PictureBox1_DragEnter);
            this.pictureBox1.DragOver += new System.Windows.Forms.DragEventHandler(this.pictureBox1_DragOver);
            this.pictureBox1.DragLeave += new System.EventHandler(this.pictureBox1_DragLeave);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importClipBoardToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(198, 28);
            // 
            // importClipBoardToolStripMenuItem
            // 
            this.importClipBoardToolStripMenuItem.Name = "importClipBoardToolStripMenuItem";
            this.importClipBoardToolStripMenuItem.Size = new System.Drawing.Size(197, 24);
            this.importClipBoardToolStripMenuItem.Text = "Import Clip Board";
            this.importClipBoardToolStripMenuItem.Click += new System.EventHandler(this.importClipBoardToolStripMenuItem_Click);
            // 
            // panelAssign
            // 
            this.panelAssign.Controls.Add(this.btnSelectCustomer);
            this.panelAssign.Controls.Add(this.label12);
            this.panelAssign.Controls.Add(this.deceasedDate);
            this.panelAssign.Controls.Add(this.txtServiceID);
            this.panelAssign.Controls.Add(this.label11);
            this.panelAssign.Controls.Add(this.label8);
            this.panelAssign.Controls.Add(this.usedDate);
            this.panelAssign.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAssign.Location = new System.Drawing.Point(0, 68);
            this.panelAssign.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelAssign.Name = "panelAssign";
            this.panelAssign.Size = new System.Drawing.Size(586, 76);
            this.panelAssign.TabIndex = 2;
            // 
            // btnSelectCustomer
            // 
            this.btnSelectCustomer.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectCustomer.Location = new System.Drawing.Point(309, 39);
            this.btnSelectCustomer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectCustomer.Name = "btnSelectCustomer";
            this.btnSelectCustomer.Size = new System.Drawing.Size(145, 30);
            this.btnSelectCustomer.TabIndex = 97;
            this.btnSelectCustomer.Text = "Select Customer";
            this.btnSelectCustomer.UseVisualStyleBackColor = true;
            this.btnSelectCustomer.Click += new System.EventHandler(this.btnSelectCustomer_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(14, 46);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(109, 17);
            this.label12.TabIndex = 96;
            this.label12.Text = "Date Deceased :";
            // 
            // deceasedDate
            // 
            this.deceasedDate.EditValue = null;
            this.deceasedDate.Location = new System.Drawing.Point(122, 41);
            this.deceasedDate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.deceasedDate.Name = "deceasedDate";
            this.deceasedDate.Properties.Appearance.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deceasedDate.Properties.Appearance.Options.UseFont = true;
            this.deceasedDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.deceasedDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.deceasedDate.Size = new System.Drawing.Size(161, 26);
            this.deceasedDate.TabIndex = 95;
            this.deceasedDate.EditValueChanged += new System.EventHandler(this.deceasedDate_EditValueChanged);
            this.deceasedDate.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // txtServiceID
            // 
            this.txtServiceID.Location = new System.Drawing.Point(383, 6);
            this.txtServiceID.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtServiceID.Name = "txtServiceID";
            this.txtServiceID.Size = new System.Drawing.Size(135, 23);
            this.txtServiceID.TabIndex = 94;
            this.txtServiceID.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(288, 11);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(79, 17);
            this.label11.TabIndex = 93;
            this.label11.Text = "Service ID :";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 17);
            this.label8.TabIndex = 92;
            this.label8.Text = "Date Used :";
            // 
            // usedDate
            // 
            this.usedDate.EditValue = null;
            this.usedDate.Location = new System.Drawing.Point(106, 6);
            this.usedDate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.usedDate.Name = "usedDate";
            this.usedDate.Properties.Appearance.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.usedDate.Properties.Appearance.Options.UseFont = true;
            this.usedDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.usedDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.usedDate.Size = new System.Drawing.Size(161, 26);
            this.usedDate.TabIndex = 91;
            this.usedDate.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // panelMiddle
            // 
            this.panelMiddle.Controls.Add(this.cmbOwner);
            this.panelMiddle.Controls.Add(this.label10);
            this.panelMiddle.Controls.Add(this.label9);
            this.panelMiddle.Controls.Add(this.receivedDate);
            this.panelMiddle.Controls.Add(this.txtSerialNumber);
            this.panelMiddle.Controls.Add(this.label7);
            this.panelMiddle.Controls.Add(this.cmbLocation);
            this.panelMiddle.Controls.Add(this.label6);
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMiddle.Location = new System.Drawing.Point(0, 0);
            this.panelMiddle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelMiddle.Name = "panelMiddle";
            this.panelMiddle.Size = new System.Drawing.Size(586, 68);
            this.panelMiddle.TabIndex = 1;
            // 
            // cmbOwner
            // 
            this.cmbOwner.DisplayMember = "Ownership";
            this.cmbOwner.FormattingEnabled = true;
            this.cmbOwner.Location = new System.Drawing.Point(383, 38);
            this.cmbOwner.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(135, 24);
            this.cmbOwner.TabIndex = 96;
            this.cmbOwner.ValueMember = "LocationCode";
            this.cmbOwner.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(306, 43);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(81, 17);
            this.label10.TabIndex = 95;
            this.label10.Text = "Ownership :";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 42);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(105, 17);
            this.label9.TabIndex = 94;
            this.label9.Text = "Date Received :";
            // 
            // receivedDate
            // 
            this.receivedDate.EditValue = null;
            this.receivedDate.Location = new System.Drawing.Point(106, 38);
            this.receivedDate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.receivedDate.Name = "receivedDate";
            this.receivedDate.Properties.Appearance.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.receivedDate.Properties.Appearance.Options.UseFont = true;
            this.receivedDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.receivedDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.receivedDate.Size = new System.Drawing.Size(161, 26);
            this.receivedDate.TabIndex = 93;
            this.receivedDate.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // txtSerialNumber
            // 
            this.txtSerialNumber.Location = new System.Drawing.Point(383, 5);
            this.txtSerialNumber.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSerialNumber.Name = "txtSerialNumber";
            this.txtSerialNumber.Size = new System.Drawing.Size(135, 23);
            this.txtSerialNumber.TabIndex = 10;
            this.txtSerialNumber.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(288, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 17);
            this.label7.TabIndex = 9;
            this.label7.Text = "Serial Number :";
            // 
            // cmbLocation
            // 
            this.cmbLocation.DisplayMember = "LocationCode";
            this.cmbLocation.FormattingEnabled = true;
            this.cmbLocation.Location = new System.Drawing.Point(106, 5);
            this.cmbLocation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbLocation.Name = "cmbLocation";
            this.cmbLocation.Size = new System.Drawing.Size(160, 24);
            this.cmbLocation.TabIndex = 8;
            this.cmbLocation.ValueMember = "LocationCode";
            this.cmbLocation.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 17);
            this.label6.TabIndex = 7;
            this.label6.Text = "Location :";
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnChange);
            this.panelTop.Controls.Add(this.txtBatesville);
            this.panelTop.Controls.Add(this.label13);
            this.panelTop.Controls.Add(this.btnDetach);
            this.panelTop.Controls.Add(this.btnAttach);
            this.panelTop.Controls.Add(this.btnSave);
            this.panelTop.Controls.Add(this.txtType);
            this.panelTop.Controls.Add(this.label5);
            this.panelTop.Controls.Add(this.txtGuage);
            this.panelTop.Controls.Add(this.txtCode);
            this.panelTop.Controls.Add(this.rtbPrice);
            this.panelTop.Controls.Add(this.label4);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.rtbDesc);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(586, 188);
            this.panelTop.TabIndex = 1;
            // 
            // btnChange
            // 
            this.btnChange.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChange.Location = new System.Drawing.Point(553, 154);
            this.btnChange.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new System.Drawing.Size(19, 30);
            this.btnChange.TabIndex = 15;
            this.btnChange.Text = ">";
            this.btnChange.UseVisualStyleBackColor = true;
            this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
            // 
            // txtBatesville
            // 
            this.txtBatesville.Location = new System.Drawing.Point(173, 86);
            this.txtBatesville.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBatesville.Name = "txtBatesville";
            this.txtBatesville.Size = new System.Drawing.Size(116, 23);
            this.txtBatesville.TabIndex = 14;
            this.txtBatesville.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(21, 90);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(156, 17);
            this.label13.TabIndex = 13;
            this.label13.Text = "Batesville Item Number :";
            // 
            // btnDetach
            // 
            this.btnDetach.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetach.Location = new System.Drawing.Point(309, 150);
            this.btnDetach.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDetach.Name = "btnDetach";
            this.btnDetach.Size = new System.Drawing.Size(121, 30);
            this.btnDetach.TabIndex = 12;
            this.btnDetach.Text = "Detach Image";
            this.btnDetach.UseVisualStyleBackColor = true;
            this.btnDetach.Click += new System.EventHandler(this.btnDetach_Click);
            // 
            // btnAttach
            // 
            this.btnAttach.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAttach.Location = new System.Drawing.Point(309, 118);
            this.btnAttach.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(121, 30);
            this.btnAttach.TabIndex = 11;
            this.btnAttach.Text = "Attach Image";
            this.btnAttach.UseVisualStyleBackColor = true;
            this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(448, 108);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(87, 70);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtType
            // 
            this.txtType.Location = new System.Drawing.Point(173, 118);
            this.txtType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtType.Name = "txtType";
            this.txtType.Size = new System.Drawing.Size(116, 23);
            this.txtType.TabIndex = 9;
            this.txtType.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(50, 122);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "Merchandise Type:";
            // 
            // txtGuage
            // 
            this.txtGuage.Location = new System.Drawing.Point(173, 149);
            this.txtGuage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGuage.Name = "txtGuage";
            this.txtGuage.Size = new System.Drawing.Size(116, 23);
            this.txtGuage.TabIndex = 7;
            this.txtGuage.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(173, 4);
            this.txtCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(116, 23);
            this.txtCode.TabIndex = 6;
            this.txtCode.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // rtbPrice
            // 
            this.rtbPrice.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbPrice.Location = new System.Drawing.Point(448, 31);
            this.rtbPrice.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rtbPrice.Name = "rtbPrice";
            this.rtbPrice.Size = new System.Drawing.Size(112, 30);
            this.rtbPrice.TabIndex = 5;
            this.rtbPrice.Text = "";
            this.rtbPrice.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(45, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 17);
            this.label4.TabIndex = 4;
            this.label4.Text = "Merchandise Guage:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(444, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Merchandise Cost :";
            // 
            // rtbDesc
            // 
            this.rtbDesc.Location = new System.Drawing.Point(173, 37);
            this.rtbDesc.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rtbDesc.Name = "rtbDesc";
            this.rtbDesc.Size = new System.Drawing.Size(245, 45);
            this.rtbDesc.TabIndex = 2;
            this.rtbDesc.Text = "";
            this.rtbDesc.TextChanged += new System.EventHandler(this.something_Changed);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(164, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Merchandise Description :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Merchandise Code :";
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
            this.menuStrip1.Size = new System.Drawing.Size(586, 30);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 26);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // miscToolStripMenuItem
            // 
            this.miscToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameCasketDescriptionToolStripMenuItem});
            this.miscToolStripMenuItem.Name = "miscToolStripMenuItem";
            this.miscToolStripMenuItem.Size = new System.Drawing.Size(53, 26);
            this.miscToolStripMenuItem.Text = "Misc";
            // 
            // renameCasketDescriptionToolStripMenuItem
            // 
            this.renameCasketDescriptionToolStripMenuItem.Name = "renameCasketDescriptionToolStripMenuItem";
            this.renameCasketDescriptionToolStripMenuItem.Size = new System.Drawing.Size(273, 26);
            this.renameCasketDescriptionToolStripMenuItem.Text = "Rename Casket Description";
            this.renameCasketDescriptionToolStripMenuItem.Click += new System.EventHandler(this.renameCasketDescriptionToolStripMenuItem_Click);
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.PeachPuff;
            this.btnRight.Image = ((System.Drawing.Image)(resources.GetObject("btnRight.Image")));
            this.btnRight.Location = new System.Drawing.Point(181, 0);
            this.btnRight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(33, 28);
            this.btnRight.TabIndex = 22;
            this.btnRight.UseVisualStyleBackColor = false;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.PeachPuff;
            this.btnLeft.Image = ((System.Drawing.Image)(resources.GetObject("btnLeft.Image")));
            this.btnLeft.Location = new System.Drawing.Point(146, 0);
            this.btnLeft.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(33, 28);
            this.btnLeft.TabIndex = 18;
            this.btnLeft.UseVisualStyleBackColor = false;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // Merchandise
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 716);
            this.Controls.Add(this.btnRight);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Merchandise";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Merchandise";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Merchandise_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Merchandise_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Merchandise_DragEnter);
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panelAssign.ResumeLayout(false);
            this.panelAssign.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.deceasedDate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deceasedDate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.usedDate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.usedDate.Properties)).EndInit();
            this.panelMiddle.ResumeLayout(false);
            this.panelMiddle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.receivedDate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.receivedDate.Properties)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtGuage;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.RichTextBox rtbPrice;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox rtbDesc;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtType;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel panelMiddle;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbLocation;
        private System.Windows.Forms.TextBox txtSerialNumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private DevExpress.XtraEditors.DateEdit usedDate;
        private System.Windows.Forms.Panel panelAssign;
        private System.Windows.Forms.Label label9;
        private DevExpress.XtraEditors.DateEdit receivedDate;
        private System.Windows.Forms.ComboBox cmbOwner;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtServiceID;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnDetach;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.Label label12;
        private DevExpress.XtraEditors.DateEdit deceasedDate;
        private System.Windows.Forms.Button btnSelectCustomer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miscToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameCasketDescriptionToolStripMenuItem;
        private System.Windows.Forms.TextBox txtBatesville;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnChange;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem importClipBoardToolStripMenuItem;
    }
}