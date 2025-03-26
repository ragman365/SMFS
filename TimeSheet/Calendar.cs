using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using GeneralLib;
/***************************************************************************************/
namespace SMFS
{
	/***************************************************************************************/
	public class Calendar : DevExpress.XtraEditors.XtraForm
	{
		private int original_width = 0;
		private string work_doctors = "";
		private string workSupervisor = "";
		private string workUsername = "";
		private DateTime work_date = new DateTime();
		private DateTime clear_date = new DateTime();
		/***************************************************************************************/
		private bool work_call = false;
		private string specialty = "";
		private string work_custom = "";
		/***************************************************************************************/
		private string popup_doctor = "";
		private string popup_call = "";
		private string popup_cover = "";
		private int work_row = 0;
		private int work_col = 0;
		private bool allow_edit = false;
		/***************************************************************************************/
		//		private System.Windows.Forms.DataGrid dg1;
		//		this.dg1 = new System.Windows.Forms.DataGrid();
		/***************************************************************************************/
		private System.Windows.Forms.DataGrid dgv;
		private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;
		private System.Windows.Forms.DateTimePicker dateTimePicker1;
		private System.Windows.Forms.MenuStrip mainMenu1;
		private System.Windows.Forms.ToolStripMenuItem menuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuItem3;
		private System.Windows.Forms.PrintDialog printDialog1;
		private System.Drawing.Printing.PrintDocument printDocument1;
		private System.Windows.Forms.ContextMenuStrip contextMenu1;
		private System.Windows.Forms.ToolStripMenuItem menuItem4;
		private System.Windows.Forms.ToolStripMenuItem menuItem5;
		private IContainer components;

		private string m_fax_company = "", m_fax_to_who = "", m_fax_num = "", m_fax_reason = "", m_fax_type = "";
		private SplitContainer splitContainer1;
		private Button btnLeft;
		private Button btnRight;
		private ToolStripMenuItem toolStripMenuItem1;
		private ToolStripMenuItem printPreviewToolStripMenuItem;
		private ToolStripMenuItem printToolStripMenuItem;
		private ToolStripMenuItem setVacationDayToolStripMenuItem;
		private ToolStripMenuItem removeVacationDayToolStripMenuItem;
		private ToolStripMenuItem hoursToolStripMenuItem;
		private ToolStripMenuItem hoursToolStripMenuItem1;
		private ToolStripMenuItem hoursToolStripMenuItem2;
		private ToolStripMenuItem toolStripMenuItem2;
		private ToolStripMenuItem toolStripMenuItem3;
		private Button btnAccept;
		private PrintPreviewDialog printPreviewDialog1;
		private DataTable vacationDt = null;

		//private string calendar_temp { get { return GlobalVariables.ApplicationDataDirectory_LocalData.FullName + "\\calendar.jpg"; } }

		/***************************************************************************************/
		public Calendar(string username, string supervisor, DateTime new_date)
		{
			work_doctors = "";
			work_date = new_date;
			workSupervisor = supervisor;
			workUsername = username;
			InitializeComponent();
		}
		/***************************************************************************************/
		public Calendar(bool call, string special, DateTime new_date)
		{
			work_doctors = "";
			work_date = new_date;
			work_call = true;
			specialty = special;
			InitializeComponent();
		}
		/***************************************************************************************/
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		/***************************************************************************************/
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Calendar));
            this.dgv = new System.Windows.Forms.DataGrid();
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setVacationDayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.hoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hoursToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.hoursToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.removeVacationDayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.mainMenu1 = new System.Windows.Forms.MenuStrip();
            this.menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.contextMenu1.SuspendLayout();
            this.mainMenu1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv
            // 
            this.dgv.BackgroundColor = System.Drawing.Color.Azure;
            this.dgv.CaptionVisible = false;
            this.dgv.ContextMenuStrip = this.contextMenu1;
            this.dgv.DataMember = "";
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(940, 541);
            this.dgv.TabIndex = 0;
            this.dgv.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            this.dataGridTableStyle1});
            this.dgv.Paint += new System.Windows.Forms.PaintEventHandler(this.dgv_Paint);
            this.dgv.DoubleClick += new System.EventHandler(this.dg1_DoubleClick);
            this.dgv.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dg1_MouseDown);
            // 
            // contextMenu1
            // 
            this.contextMenu1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setVacationDayToolStripMenuItem,
            this.removeVacationDayToolStripMenuItem});
            this.contextMenu1.Name = "contextMenu1";
            this.contextMenu1.Size = new System.Drawing.Size(224, 52);
            // 
            // setVacationDayToolStripMenuItem
            // 
            this.setVacationDayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.hoursToolStripMenuItem,
            this.hoursToolStripMenuItem1,
            this.hoursToolStripMenuItem2});
            this.setVacationDayToolStripMenuItem.Name = "setVacationDayToolStripMenuItem";
            this.setVacationDayToolStripMenuItem.Size = new System.Drawing.Size(223, 24);
            this.setVacationDayToolStripMenuItem.Text = "Set Vacation Day";
            this.setVacationDayToolStripMenuItem.Click += new System.EventHandler(this.setVacationDayToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(209, 26);
            this.toolStripMenuItem2.Text = "Set Start Vacation";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(209, 26);
            this.toolStripMenuItem3.Text = "Set Stop Vacation";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // hoursToolStripMenuItem
            // 
            this.hoursToolStripMenuItem.Name = "hoursToolStripMenuItem";
            this.hoursToolStripMenuItem.Size = new System.Drawing.Size(209, 26);
            this.hoursToolStripMenuItem.Text = "8 Hours";
            this.hoursToolStripMenuItem.Click += new System.EventHandler(this.hoursToolStripMenuItem_Click);
            // 
            // hoursToolStripMenuItem1
            // 
            this.hoursToolStripMenuItem1.Name = "hoursToolStripMenuItem1";
            this.hoursToolStripMenuItem1.Size = new System.Drawing.Size(209, 26);
            this.hoursToolStripMenuItem1.Text = "4 Hours";
            this.hoursToolStripMenuItem1.Click += new System.EventHandler(this.hoursToolStripMenuItem1_Click);
            // 
            // hoursToolStripMenuItem2
            // 
            this.hoursToolStripMenuItem2.Name = "hoursToolStripMenuItem2";
            this.hoursToolStripMenuItem2.Size = new System.Drawing.Size(209, 26);
            this.hoursToolStripMenuItem2.Text = "2 Hours";
            this.hoursToolStripMenuItem2.Click += new System.EventHandler(this.hoursToolStripMenuItem2_Click);
            // 
            // removeVacationDayToolStripMenuItem
            // 
            this.removeVacationDayToolStripMenuItem.Name = "removeVacationDayToolStripMenuItem";
            this.removeVacationDayToolStripMenuItem.Size = new System.Drawing.Size(223, 24);
            this.removeVacationDayToolStripMenuItem.Text = "Remove Vacation Day";
            this.removeVacationDayToolStripMenuItem.Click += new System.EventHandler(this.removeVacationDayToolStripMenuItem_Click);
            // 
            // dataGridTableStyle1
            // 
            this.dataGridTableStyle1.DataGrid = this.dgv;
            this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(73, 6);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(280, 23);
            this.dateTimePicker1.TabIndex = 1;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // mainMenu1
            // 
            this.mainMenu1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem1});
            this.mainMenu1.Location = new System.Drawing.Point(0, 0);
            this.mainMenu1.Name = "mainMenu1";
            this.mainMenu1.Size = new System.Drawing.Size(940, 30);
            this.mainMenu1.TabIndex = 1;
            // 
            // menuItem1
            // 
            this.menuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.menuItem4,
            this.menuItem3});
            this.menuItem1.Name = "menuItem1";
            this.menuItem1.Size = new System.Drawing.Size(46, 26);
            this.menuItem1.Text = "File";
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
            // menuItem4
            // 
            this.menuItem4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem5});
            this.menuItem4.Name = "menuItem4";
            this.menuItem4.Size = new System.Drawing.Size(122, 26);
            this.menuItem4.Text = "Fax";
            // 
            // menuItem5
            // 
            this.menuItem5.Name = "menuItem5";
            this.menuItem5.Size = new System.Drawing.Size(173, 26);
            this.menuItem5.Text = "SomeWhere";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Name = "menuItem3";
            this.menuItem3.Size = new System.Drawing.Size(122, 26);
            this.menuItem3.Text = "Exit";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // printDocument1
            // 
            this.printDocument1.BeginPrint += new System.Drawing.Printing.PrintEventHandler(this.printDocument1_BeginPrint);
            this.printDocument1.EndPrint += new System.Drawing.Printing.PrintEventHandler(this.printDocument1_EndPrint);
            this.printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument1_PrintPage);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 30);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnAccept);
            this.splitContainer1.Panel1.Controls.Add(this.btnRight);
            this.splitContainer1.Panel1.Controls.Add(this.btnLeft);
            this.splitContainer1.Panel1.Controls.Add(this.dateTimePicker1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgv);
            this.splitContainer1.Size = new System.Drawing.Size(940, 575);
            this.splitContainer1.SplitterDistance = 30;
            this.splitContainer1.TabIndex = 7;
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(456, 5);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 29;
            this.btnAccept.Text = "Accept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.PeachPuff;
            this.btnRight.Image = ((System.Drawing.Image)(resources.GetObject("btnRight.Image")));
            this.btnRight.Location = new System.Drawing.Point(357, 6);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(41, 26);
            this.btnRight.TabIndex = 28;
            this.btnRight.UseVisualStyleBackColor = false;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.PeachPuff;
            this.btnLeft.Image = ((System.Drawing.Image)(resources.GetObject("btnLeft.Image")));
            this.btnLeft.Location = new System.Drawing.Point(29, 5);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(40, 27);
            this.btnLeft.TabIndex = 27;
            this.btnLeft.UseVisualStyleBackColor = false;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // printPreviewDialog1
            // 
            this.printPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog1.Document = this.printDocument1;
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog1.Icon")));
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.Visible = false;
            // 
            // Calendar
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
            this.ClientSize = new System.Drawing.Size(940, 605);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.mainMenu1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "Calendar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Calendar";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Calendar_Closing);
            this.Load += new System.EventHandler(this.Calendar_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.contextMenu1.ResumeLayout(false);
            this.mainMenu1.ResumeLayout(false);
            this.mainMenu1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		/***************************************************************************************/
		private void Calendar_Load(object sender, System.EventArgs e)
		{
			vacationDt = new DataTable();
			vacationDt.Columns.Add("what");

			allow_edit = false;
			//string preference = User.getInstance.UserPreferences.AdministrativePreference.GetPreference("Doctor Calendar", "Allow Edit");
			//         if (preference.Trim().ToUpper() == "YES")
			//             allow_edit = true;
			this.dateTimePicker1.Value = work_date;
			DataTable dt = new DataTable("dt");
			this.dgv.DataSource = dt;
			this.dgv.TableStyles[0].MappingName = "dt";
			this.dgv.Tag = "CALENDAR";
			create_datacolumn(dt, dgv, "CUSTOM", "Num", 0);
			original_width = 141;
			for (int i = 0; i < 7; i++)
			{
				int dow = i;
				string type = "";
				if (dow == 0)
					type += "SUNDAY";
				else if (dow == 1)
					type += "MONDAY";
				else if (dow == 2)
					type += "TUESDAY";
				else if (dow == 3)
					type += "WEDNESDAY";
				else if (dow == 4)
					type += "THURSDAY";
				else if (dow == 5)
					type += "FRIDAY";
				else if (dow == 6)
					type += "SATURDAY";
				create_datacolumn(dt, dgv, "CUSTOM", type, original_width);
			}
			add_slots();
			dg_cleanup();
			fill_calendar(work_date);
			remove_slots();
			dg_cleanup();
			cleanup_file_menu();
			if (work_call)
			{
				create_menu();
				this.dgv.Tag = "CALL";
			}
			else
			{
				create_menu();
				this.dgv.Tag = "CALL";
			}


			//			dg1.Hide();
			//			Graphics graphics      = Graphics.FromHwnd(this.dg1.Handle); 
			//			Graphics graphics      = Graphics.FromHwnd(this.Handle); 
		}
		/****************************************************************************************/
		private void fill_doctors(DateTime new_date, int row, int col, DataTable timeOffDt)
		{
			string name = "";
			for (int i = 0; i < timeOffDt.Rows.Count; i++)
			{
				name = timeOffDt.Rows[i]["name"].ObjToString();
				load_appts(name, new_date, row, col);
			}
		}
		/****************************************************************************************/
		private void load_appts(string name, DateTime newdate, int row, int col)
		{
			//int i_doc            = convert_doctor ( name );
			//string mydate        = newdate.Year.ToString("D4");
			//mydate              += "-" + newdate.Month.ToString("D2" );
			//mydate              += "-" + newdate.Day.ToString("D2" );
			//string command     =  "select emr, apptdate, appttime, stat, acctnum, nam, doc, place, apptkey, typ, slots ";
			//command             += " from appointments ";
			//if ( i_doc > 0 )
			//{
			//	string str       = i_doc.ToString();
			//	if ( str.Length == 1 )
			//		command     += " where ( place = '" + i_doc.ToString() + "' || place = '" + i_doc.ToString("D2") + "' ) ";
			//	else
			//		command     += " where place = '" + i_doc.ToString() + "' ";
			//}
			//else
			//	command         += " where place = '" + name + "' ";
			//command             += " and apptdate = '" + mydate + "' ";
			//command             += "order by apptdate, appttime, acctnum ";
			//         DataTable dx        = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx = sqlCommand.ExecuteFill();
			//int newrows          = dx.Rows.Count;
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			//DataRowCollection rx = dx.Rows;
			//int newappts         = 0;
			//int rechecks         = 0;
			//for ( int i=0; i<newrows; i++ )
			//{
			//	string chart     = rx[i]["acctnum"].ToString();
			//	if ( chart.Trim().ToUpper() == "45551" )
			//	{
			//		chart += " ";
			//		chart  = chart.Trim();
			//	}
			//	string status    = rx[i]["stat"].ToString();
			//	//status           = G1.code_to_status ( null, status ).Trim().ToUpper();
			//	if ( status == "CANCELLED" || status == "CANCELED" || status == "RESCHED" )
			//		continue;
			//	int type         = G1.myint(dx.Rows[i]["typ"].ToString());
			//	if ( type == 1 || type == 2 )
			//		newappts++;
			//	else
			//		rechecks++;
			//}
			string newname = name;
			//string call      = get_doctor_status ( i_doc, newdate );
			//if ( call.Trim().Length > 0 )
			//	newname     += " " + call;
			//if ( newappts == 0 && rechecks == 0 && call.Trim().Length == 0 )
			//	return;
			string text = rc[row][col].ToString();
			text += "\n" + newname;
			rc[row][col] = text;
			dt.AcceptChanges();
		}
		/***************************************************************************************/
		private string convert_idoctor(int i_doc)
		{
			string command = "select * from doctors ";
			command += "where doctor = '" + i_doc.ToString() + "' ";
			command += "order by doctor ";
			DataTable dx = G1.get_db_data(command);
			if (dx == null)
				return "";
			int newrows = dx.Rows.Count;
			if (newrows <= 0)
				return "";
			DataRowCollection rx = dx.Rows;
			string fname = rx[0]["FName"].ToString();
			string lname = rx[0]["LName"].ToString();
			string name = fname + " " + lname;
			return name;
		}
		/***************************************************************************************/
		private int convert_doctor(string name)
		{
			int i_doc = 0;
			string command = "select * from doctors order by doc ";
			DataTable dx = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx         = new DataTable();
			//dx = sqlCommand.ExecuteFill();
			int newrows = dx.Rows.Count;
			DataRowCollection rc = dx.Rows;
			for (int i = 0; i < newrows; i++)
			{
				string doctor = rc[i]["doc"].ToString();
				if (doctor.Trim().Length == 0)
					continue;
				if (G1.validate_numeric(doctor) == true)
				{
					string fname = rc[i]["FName"].ToString();
					string lname = rc[i]["LName"].ToString();
					string doc = fname.Substring(0, 1) + " " + lname;
					if (doc.ToUpper() == name.ToUpper())
					{
						i_doc = G1.myint(doctor);
						break;
					}
				}
			}
			return i_doc;
		}
		/****************************************************************************************/
		private void fill_calendar(DateTime new_date)
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			int count = rc.Count;
			if (count <= 0)
				return;
			int mm = new_date.Month;
			int dd = new_date.Day;
			int yy = new_date.Year;
			string sdate = mm.ToString("D2") + "/" + dd.ToString("D2") + "/" + yy.ToString("D4");
			long cdate = G1.date_to_days(sdate);
			//int days             = G1.days_in_month ( yy, mm );
			int days = DateTime.DaysInMonth(yy, mm);
			DateTime ldate = new_date;
			ldate = ldate.AddDays((double)(-(dd) + 1));
			for (int j = 0; j < 6; j++)
			{
				for (int i = 0; i < 7; i++)
					rc[j][i + 1] = "";
			}
			dt.AcceptChanges();

			DateTime firstDate = new DateTime(new_date.Year, new_date.Month, 1);
			DateTime lastDate = new DateTime(new_date.Year, new_date.Month, days);

			//string cmd = "Select * from `tc_timerequest` where `supervisor` = '" + workSupervisor + "' AND (( `fromdate` >= '" + firstDate.ToString("yyyyMMdd") + "' AND `fromdate` <= '" + lastDate.ToString("yyyyMMdd") + "' ) ";

			string cmd = "Select * from `tc_timerequest` where `supervisor` = '" + workUsername + "' AND (( `fromdate` >= '" + firstDate.ToString("yyyyMMdd") + "' AND `fromdate` <= '" + lastDate.ToString("yyyyMMdd") + "' ) ";
			cmd += " OR ( `todate` >= '" + lastDate.ToString("yyyyMMdd") + "' AND `todate` <= '" + lastDate.ToString("yyyyMMdd") + "') ) ";
			//if (cmbMyProc.Text.ToUpper() == "APPROVED")
			//	cmd += " and `approved` = 'Y' ";
			//else if (cmbMyProc.Text.ToUpper() == "UNAPPROVED")
			//	cmd += " and `approved` <> 'Y' ";
			cmd += " order by `fromdate` DESC; ";
			DataTable dx = G1.get_db_data(cmd);

			dx.Columns.Add("sFrom");
			dx.Columns.Add("sTo");
			DateTime date = DateTime.Now;

			string sfDate = "";
			string slDate = "";
			for (int i = 0; i < dx.Rows.Count; i++)
			{
				date = dx.Rows[i]["fromdate"].ObjToDateTime();
				sfDate = date.ToString("yyyyMMdd");
				date = dx.Rows[i]["todate"].ObjToDateTime();
				slDate = date.ToString("yyyyMMdd");
				dx.Rows[i]["sFrom"] = sfDate;
				dx.Rows[i]["sTo"] = slDate;
			}

			DataRow[] dRows = null;

			DataTable timeOffDt = null;

			int row = 0;
			slDate = "";
			for (int i = 1; i <= days; i++)
			{
				int dow = (int)ldate.DayOfWeek;
				rc[row][dow + 1] = i.ToString();

				slDate = ldate.ToString("yyyyMMdd");

				dRows = dx.Select("'" + slDate + "' >= sFrom AND '" + slDate + "'<= sTo");

				//                dRows = dx.Select("sFrom >='" + slDate + "'");
				if (dRows.Length > 0)
				{
					timeOffDt = dRows.CopyToDataTable();
					dRows = timeOffDt.Select("sTo >='" + slDate + "'");
					if (dRows.Length > 0)
					{
						timeOffDt = dRows.CopyToDataTable();
						fill_doctors(ldate, row, dow + 1, timeOffDt);
					}
				}

				if (dow == 6)
					row = row + 1;
				ldate = ldate.AddDays((double)1.0);
			}
		}
		/****************************************************************************************/
		private void create_datacolumn(DataTable dt, DataGrid dg, string type, string name, int width)
		{
			if (type.ToUpper() == "CUSTOM" && 1 != 1)
			{
				DataColumn c1 = new DataColumn(name, Type.GetType("System.String"));
				c1.ColumnName = name;
				dt.Columns.Add(c1);
				int cols = dg.TableStyles[0].GridColumnStyles.Count;
				cols = cols - 1;
				if (cols < 0)
					cols = 0;
				//DataGridTextBoxColumn1 TextCol = new DataGridTextBoxColumn1();
				//dg.TableStyles[0].GridColumnStyles.Add( TextCol);
				dg.TableStyles[0].GridColumnStyles[cols].MappingName = name;
				dg.TableStyles[0].GridColumnStyles[cols].HeaderText = name;
				dg.TableStyles[0].GridColumnStyles[cols].Width = width;
			}
			else
			{
				DataColumn c1 = new DataColumn(name, Type.GetType("System.String"));
				//				DataColumn c1 = new DataColumn(name,Type.GetType(type));
				c1.ColumnName = name;
				dt.Columns.Add(c1);
				int cols = dg.TableStyles[0].GridColumnStyles.Count;
				cols = cols - 1;
				if (cols < 0)
					cols = 0;
				DataGridColumnStyle TextCol = new DataGridTextBoxColumn();
				dg.TableStyles[0].GridColumnStyles.Add(TextCol);
				dg.TableStyles[0].GridColumnStyles[cols].MappingName = name;
				dg.TableStyles[0].GridColumnStyles[cols].HeaderText = name;
				dg.TableStyles[0].GridColumnStyles[cols].Width = width;
			}
		}
		/*****************************************************************************************/
		//  COMMENTED BY CODEIT.RIGHT
		//		private void set_alternate_row_color ( DataGrid dg, Color color )
		//		{
		//			//			dg.TableStyles[0].BackColor = System.Drawing.Color.Blue;
		//			dg.TableStyles[0].AlternatingBackColor=color;
		//			dg.TableStyles[0].SelectionBackColor=System.Drawing.Color.Yellow;
		//			dg.TableStyles[0].SelectionForeColor=System.Drawing.Color.Black;
		//			DataTable dt = G1.GridtoTable ( dg );
		//			dt.AcceptChanges ();
		//			Appointments.AutoSizeGrid ( 18, dg );
		//		}
		/***********************************************************************************************/
		private void dg_cleanup()
		{
			DataTable dt = G1.GridtoTable(dgv);
			dt.AcceptChanges();
			//G1.remove_endrow ( this, dg1 );
			dgv.TableStyles[0].RowHeaderWidth = 0;
			AutoSizeGrid(18, dgv);
			this.dgv.Refresh();
		}
		/***************************************************************************************/
		private void create_fax_menu()
		{
			if (!allow_edit)
				return;
			int menucount = this.mainMenu1.Items.Count;
			for (int i = (menucount - 1); i >= 0; i--)
			{
				string name = this.mainMenu1.Items[i].Text;
				if (name.Trim().ToUpper() == "FILE")
				{
					int mcount = ((ToolStripMenuItem)this.mainMenu1.Items[i]).DropDownItems.Count;
					for (int j = 0; j < mcount; j++)
					{
						string newname = ((ToolStripMenuItem)this.mainMenu1.Items[i]).DropDownItems[j].Text;
						if (newname.Trim().ToUpper() == "FAX")
							addto_faxmenu(((ToolStripMenuItem)((ToolStripMenuItem)this.mainMenu1.Items[i]).DropDownItems[j]));
					}
					break;
				}
			}
		}
		/***************************************************************************************/
		private void addto_faxmenu(ToolStripMenuItem menu)
		{
			ToolStripMenuItem emenu = new ToolStripMenuItem();
			emenu.Text = "Edit Fax List";
			emenu.Click += new System.EventHandler(edit_Click);
			menu.DropDownItems.Add(emenu);
			string command = "select * from callfaxes ";
			command += "where specialty = '" + specialty + "' ";
			command += "order by Company ";
			DataTable dx = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx         = sqlCommand.ExecuteFill();
			int newrows = dx.Rows.Count;
			if (newrows <= 0)
				return;
			emenu = new ToolStripMenuItem();
			emenu.Text = specialty + " Call List";
			emenu.Click += new System.EventHandler(faxcall_Click);
			menu.DropDownItems.Add(emenu);
			for (int i = 0; i < newrows; i++)
			{
				string company = dx.Rows[i]["company"].ToString();
				string person = dx.Rows[i]["person"].ToString();
				string fax = dx.Rows[i]["phone"].ToString();
				emenu = new ToolStripMenuItem();
				emenu.Text = "Fax to : " + company + ":" + person + ":" + fax;
				emenu.Click += new System.EventHandler(faxcall_Click);
				menu.DropDownItems.Add(emenu);
			}
		}
		/***************************************************************************************/
		private void cleanup_file_menu()
		{
			//if ( G1.isSupervisor() || G1.isAdmin() )
			//	return;
			//int menucount = this.mainMenu1.Items.Count;
			//for ( int i=(menucount-1); i>=0; i-- )
			//{
			//	string name = this.mainMenu1.Items[i].Text;
			//	if ( name.ToUpper().IndexOf ( "FILE") >= 0 )
			//	{ // Found it, now find the Edit Call Options Menu
			//		int count = ((ToolStripMenuItem)this.mainMenu1.Items[i]).DropDownItems.Count;
			//		for ( int j=(count-1); j>=0; j-- )
			//		{
			//			name = ((ToolStripMenuItem)this.mainMenu1.Items[i]).DropDownItems[j].Text;
			//			if ( name.Trim().ToUpper().IndexOf ( "EDIT CALL OPTIONS" ) >= 0 )
			//			{
			//				((ToolStripMenuItem)this.mainMenu1.Items[i]).DropDownItems.RemoveAt(j);
			//				break;
			//			}
			//		}
			//		break;
			//	}
			//}
		}
		/***************************************************************************************/
		private void create_menu()
		{
			//if ( !G1.is_supervisor() && !G1.is_admin() )
			//    return;
			if (!allow_edit)
				return;
			create_fax_menu();
			string command = "select * from doctors ";
			command += "where cal = '" + specialty + "' ";
			command += "order by practitioner, doc ";
			DataTable dx = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx         = sqlCommand.ExecuteFill();
			int newrows = dx.Rows.Count;
			DataRowCollection rc = dx.Rows;
			for (int i = 0; i < newrows; i++)
			{
				string doctor = rc[i]["doc"].ToString();
				if (doctor.Trim().Length == 0)
					continue;
				string call = rc[i]["cal"].ToString();
				if (call.Trim().Length == 0)
					continue;
				doctor = rc[i]["Fname"].ToString() + " " + rc[i]["Lname"].ToString();
				ToolStripMenuItem menu = new ToolStripMenuItem();
				menu.Text = doctor;
				menu.DropDownOpening += new EventHandler(doctor_Popup);
				add_to_options(doctor);
				locate_template_call(menu, rc);
				//				fill_call_menu   ( menu, "Off", rc );
				//				fill_call_menu   ( menu, "Out", rc );
				//				fill_call_menu   ( menu, "Call", rc );
				//				fill_call_menu   ( menu, "Con", rc );
				//				fill_call_menu   ( menu, "DC", rc );
				//				fill_call_menu   ( menu, "Hosp", rc );
				//				fill_call_menu   ( menu, "Clinic", rc );
				//				fill_call_menu   ( menu, "Clear", rc );
				this.contextMenu1.Items.Add(menu);
			}
			ToolStripMenuItem hmenu = new ToolStripMenuItem();
			hmenu.Text = "Closed";
			hmenu.DropDownOpening += new EventHandler(doctor_Popup);
			this.contextMenu1.Items.Add(hmenu);
			closed_template(hmenu, "New Years");
			closed_template(hmenu, "Christmas");
			closed_template(hmenu, "Memorial Day");
			closed_template(hmenu, "Labor Day");
			closed_template(hmenu, "Thanksgiving");
			closed_template(hmenu, "Holiday");
			closed_template(hmenu, "Half Day");
			closed_template(hmenu, "Clear");
			ToolStripMenuItem xmenu = new ToolStripMenuItem();
			xmenu.Text = "Enter Something";
			xmenu.DropDownOpening += new EventHandler(doctor_Popup);
			this.contextMenu1.Items.Add(xmenu);
			closed_template(xmenu, "Enter Something");
			closed_template(xmenu, "Clear Something");
		}
		/***************************************************************************************/
		private void add_to_options(string doctor)
		{
			int count = this.mainMenu1.Items.Count;
			for (int i = 0; i < count; i++)
			{
				string name = this.mainMenu1.Items[i].Text.ToUpper();
				if (name == "OPTIONS")
				{
					ToolStripMenuItem omenu = (ToolStripMenuItem)this.mainMenu1.Items[i];
					for (int j = 0; j < omenu.DropDownItems.Count; j++)
					{
						string str = omenu.DropDownItems[j].Text.ToUpper();
						if (str == "ANALYSIS")
						{
							ToolStripMenuItem menu = (ToolStripMenuItem)omenu.DropDownItems[j];
							ToolStripMenuItem newmenu = new ToolStripMenuItem();
							newmenu.Click += new EventHandler(newmenu_Click);
							newmenu.Text = doctor;
							menu.DropDownItems.Add(newmenu);
							break;
						}
					}
				}
			}
		}
		/***************************************************************************************/
		void newmenu_Click(object sender, EventArgs e)
		{ // Run Special Analysis on Doctor
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string doctor = jj.Text;
			string str = doctor;
			string fname = "";
			string lname = "";
			string consult = "";
			string call = "";
			bool er = false;
			parse_name(str, ref fname, ref lname, ref consult, ref call, ref er);
			string initials = str;
			if (fname.Length > 0 && lname.Length > 0)
				initials = fname.ToUpper().Substring(0, 1) + lname.ToUpper().Substring(0, 1);

			string command = "select * from doctors ";
			command += "where fname = '" + fname + "' ";
			command += "and lname = '" + lname + "' ";
			command += "order by doc ";
			DataTable dx = G1.get_db_data(command);
			DataTable dt = G1.GridtoTable(dgv);
			DateTime date = this.dateTimePicker1.Value;
			int iday = 1;
			long t_wait = 0L;
			long t_wait_count = 0L;
			long t_all = 0L;
			long t_all_count = 0L;
			for (int i = 0; i < dx.Rows.Count; i++)
			{
				string doc = dx.Rows[i]["doc"].ToString();
				for (int j = 0; j < dt.Rows.Count; j++)
				{
					for (int k = 0; k <= 7; k++)
					{
						str = dt.Rows[j][k].ToString();
						str += "\n";
						string[] Lines = str.Split('\n');
						if (Lines.Length > 0)
							str = Lines[0];
						else
							str = "";
						if (str.Length > 0)
						{
							string day = str;
							dt.Rows[j][k] = day;
							if (G1.validate_numeric(day))
							{
								iday = G1.myint(day);
								string newdate = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + iday.ToString("D2");
								command = "select emr, apptdate, appttime, stat, acctnum, nam, doc, place, apptkey, typ, slots, appout, checkin, wait ";
								command += " from appointments ";
								command += " where place = '" + doc.ToString() + "' ";
								command += " and apptdate = '" + newdate + "' ";
								command += "order by apptdate, appttime, acctnum ";
								DataTable xx = G1.get_db_data(command);
								int newrows = xx.Rows.Count;
								DataRowCollection rc = dt.Rows;
								DataRowCollection rx = xx.Rows;
								int newappts = 0;
								int rechecks = 0;
								int resch = 0;
								string last_time = "";
								long total_wait = 0L;
								long total_wait_count = 0L;
								long total_all = 0L;
								long total_all_count = 0L;

								for (int l = 0; l < newrows; l++)
								{
									string chart = rx[l]["acctnum"].ToString();
									if (chart.Trim().ToUpper() == "45551")
									{
										chart += " ";
										chart = chart.Trim();
									}
									string status = rx[l]["stat"].ToString();
									//status = G1.code_to_status(null, status).Trim().ToUpper();
									if (status == "CANCELLED" || status == "CANCELED" || status == "RESCHED")
									{
										if (status == "RESCHED")
											resch++;
										continue;
									}
									int type = G1.myint(xx.Rows[l]["typ"].ToString());
									if (type == 1 || type == 2)
										newappts++;
									else
										rechecks++;
									last_time = xx.Rows[l]["appttime"].ToString();

									string key = rx[l]["apptkey"].ToString();
									string checkin = "";
									string checkback = "";
									string checkout = "";
									if (iday == 20)
										checkout = "";
									//if (doc.ToUpper() != "LAB")
									//{
									//    checkout  = G1.get_db_time(key, "appout");
									//    checkback = G1.get_db_time(key, "wait");
									//    checkin   = G1.get_db_time(key, "checkin");
									//    if (checkout.Length > 0)
									//    {
									//        long t_in = 0L;
									//        long t_back = 0L;
									//        long t_out = 0L;
									//        if ( checkin.Trim().Length > 0 )
									//            t_in      = G1.date_to_seconds(checkin);
									//        if ( checkback.Trim().Length > 0 )
									//            t_back    = G1.date_to_seconds(checkback);
									//        if ( checkout.Trim().Length > 0 )
									//        t_out     = G1.date_to_seconds(checkout);
									//        if (t_in > 0L && t_out > 0L )
									//        {
									//            total_all += t_out - t_in;
									//            total_all_count += 1L;
									//        }
									//        if (t_in > 0L && t_back > 0L)
									//        {
									//            total_wait += t_back - t_in;
									//            total_wait_count += 1L;
									//        }
									//    }
									//}
								}
								if (last_time.Trim().Length > 0)
								{
									int xdx = last_time.LastIndexOf(":00");
									if (xdx > 0)
										last_time = last_time.Substring(0, xdx);
								}
								string newname = "";
								if (last_time.Trim().Length > 0)
									newname = doc + " ( " + newappts.ToString() + "/" + rechecks.ToString() + "/" + resch.ToString() + "/" + last_time + " )";
								else
									newname = doc + " ( " + newappts.ToString() + "/" + rechecks.ToString() + "/" + resch.ToString() + " )";
								string back = "";
								string wait = "";
								if (total_all_count > 0)
								{
									long total_wait_avg = total_all / total_all_count / 60L;
									wait = "Wait=" + total_wait_avg.ToString();
									if (total_wait_count > 0)
									{
										long total_back_avg = total_wait / total_wait_count / 60L;
										back = "Back=" + total_back_avg.ToString();
									}
								}
								t_wait += total_wait;
								t_wait_count += total_wait_count;
								t_all += total_all;
								t_all_count += total_all_count;
								string text = rc[j][k].ToString();
								text = day;
								text += "\n" + newname;
								if (wait.Trim().Length > 0)
									text += "\n" + wait;
								if (back.Trim().Length > 0)
									text += "\n" + back;
								rc[j][k] = text;
								dt.AcceptChanges();
							}
						}
					}
				}
			}
			if (t_all_count > 0)
			{
				long total_wait_avg = t_all / t_all_count / 60L;
				string wait = "InClinic=" + total_wait_avg.ToString();
				string back = "";
				if (t_wait_count > 0)
				{
					long total_back_avg = t_wait / t_wait_count / 60L;
					back = "Back=" + total_back_avg.ToString();
				}
				int max = dt.Rows.Count;
				dt.Rows[max - 1][7] = "\nAverages " + t_all_count.ToString() + "\n" + wait + "\n" + back;
			}
			dg_cleanup();
		}
		/***************************************************************************************/
		private void closed_template(ToolStripMenuItem menu, string why)
		{
			ToolStripMenuItem cmenu = new ToolStripMenuItem();
			cmenu.Text = why;
			menu.DropDownItems.Add(cmenu);
			cmenu.Click += new EventHandler(closed_Click);
		}
		/***************************************************************************************/
		private void locate_template_call(ToolStripMenuItem menu, DataRowCollection rc)
		{
			string subkey = "";
			string lookup = specialty + " CALL";
			//string key           = G1.locate_template ( lookup, ref subkey );
			string key = "";
			if (key.Trim().Length == 0)
				return;
			string command = "select * from templatedata ";
			command += "where keyval = '" + subkey + "' ";
			command += "order by rw ";
			DataTable dx = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx         = sqlCommand.ExecuteFill();
			int newrows = dx.Rows.Count;
			DataRowCollection rx = dx.Rows;
			for (int i = 0; i < newrows; i++)
			{
				string selection = rx[i]["selection"].ToString();
				if (selection.Trim().Length == 0)
					continue;
				string type = rx[i]["typ"].ToString();
				subkey = rx[i]["subkey"].ToString();
				fill_call_menu(menu, selection, rc, subkey);
			}
		}
		/***************************************************************************************/
		private void fill_call_menu(ToolStripMenuItem callmenu, string text, DataRowCollection rc, string key)
		{
			string doctor = callmenu.Text;
			ToolStripMenuItem menu = new ToolStripMenuItem();
			menu.Text = text;
			menu.Click += new System.EventHandler(call_Click);
			menu.DropDownOpening += new EventHandler(call_Popup);
			callmenu.DropDownItems.Add(menu);
			if (key.Trim().Length > 0)
			{
				if (!G1.validate_numeric(key))
					return;
				int ikey = G1.myint(key);
				if (ikey <= 0)
					return;
				string command = "select * from templatedata ";
				command += "where keyval = '" + key + "' ";
				command += "order by rw ";
				DataTable dx = G1.get_db_data(command);
				//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
				//DataTable dx         = sqlCommand.ExecuteFill();
				int newrows = dx.Rows.Count;
				DataRowCollection rx = dx.Rows;
				for (int i = 0; i < newrows; i++)
				{
					string selection = rx[i]["selection"].ToString();
					if (selection.Trim().Length == 0)
						continue;
					if (selection.Trim().ToUpper() == "$DOCTORS")
					{
						fill_call_doctors(menu, doctor, rc);
					}
					else
					{
						string type = rx[i]["typ"].ToString();
						string subkey = rx[i]["subkey"].ToString();
						fill_call_menu(menu, selection, rc, subkey);
					}
				}
			}
		}
		/***************************************************************************************/
		private void fill_call_doctors(ToolStripMenuItem callmenu, string doctor, DataRowCollection rc)
		{
			ToolStripMenuItem xmenu = new ToolStripMenuItem();
			xmenu.Text = callmenu.Text;
			xmenu.Click += new System.EventHandler(call_Click);
			xmenu.DropDownOpening += new EventHandler(cover_Popup);
			callmenu.DropDownItems.Add(xmenu);
			int rows = rc.Count;
			for (int i = 0; i < rows; i++)
			{
				string str = rc[i]["Fname"].ToString() + " " + rc[i]["Lname"].ToString();
				if (str.Trim().ToUpper() == doctor.Trim().ToUpper())
					continue;
				ToolStripMenuItem nmenu = new ToolStripMenuItem();
				nmenu.Text = str;
				nmenu.Click += new System.EventHandler(cover_Click);
				nmenu.DropDownOpening += new EventHandler(cover_Popup);
				callmenu.DropDownItems.Add(nmenu);
			}
		}
		/*****************************************************************************************/
		private void edit_Click(object sender, System.EventArgs e)
		{
			//ToolStripMenuItem jj  = (ToolStripMenuItem) sender;
			//string call  = jj.Text;
			//CallFaxes myform   = new CallFaxes ( specialty );
			//myform.Text        = "Edit " + specialty + " Calendar Fax List";
			//myform.Show ();
		}
		/*****************************************************************************************/
		private void faxcall_Click(object sender, System.EventArgs e)
		{
			CleanupFaxProperties();
			string faxFile = null;
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string call = jj.Text;
			if (m_fax_reason.Length == 0)
				m_fax_reason = call;
			if (call.ToUpper().IndexOf("FAX TO") == 0)
			{
				call += ":";
				int idx = call.IndexOf(":");
				if (idx <= 0)
					return;
				if ((idx + 1) > call.Length)
					return;
				call = call.Substring((idx + 1));
				idx = call.IndexOf(":");
				if (idx <= 0)
					return;
				m_fax_company = call.Substring(0, idx).Trim();
				if ((idx + 1) > call.Length)
					return;
				call = call.Substring((idx + 1));
				idx = call.IndexOf(":");
				if (idx <= 0)
					return;
				m_fax_to_who = call.Substring(0, idx).Trim();
				if ((idx + 1) > call.Length)
					return;
				call = call.Substring((idx + 1));
				idx = call.IndexOf(":");
				if (idx <= 0)
					return;
				m_fax_num = call.Substring(0, idx).Trim();
				//				if ( ChartForm.fax_company.Trim().Length == 0 || ChartForm.fax_to_who.Trim().Length == 0 ||
				//					ChartForm.fax_phone.Trim().Length == 0 )
				//					return;
				string save_custom = work_custom;
				string command = "select * from callfaxes ";
				command += "where specialty = '" + specialty + "' ";
				command += " and company = '" + m_fax_company + "'";
				command += " and person = '" + m_fax_to_who + "'";
				command += " and phone = '" + m_fax_num + "'";
				command += "order by Company ";
				DataTable dx = G1.get_db_data(command);
				//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
				//DataTable dx         = sqlCommand.ExecuteFill();
				int newrows = dx.Rows.Count;
				if (newrows <= 0)
					return;
				string note = dx.Rows[0]["note"].ToString();
				work_custom = dx.Rows[0]["customize"].ToString();
				work_custom = work_custom.Replace(" ", ""); // Remove Blanks
				if (work_custom == "NORMAL")
					work_custom = "";
				if (work_custom != save_custom)
				{
					add_slots();
					fill_calendar(work_date);
					remove_slots();
					dg_cleanup();
				}
				faxFile = save_calendar();
				fax_calendar(faxFile, note);
				if (work_custom != save_custom)
				{
					work_custom = save_custom;
					add_slots();
					fill_calendar(work_date);
					remove_slots();
					dg_cleanup();
				}
			}
			else if (call.ToUpper().IndexOf("CALL LIST") > 0)
			{ // Pull Specialty List and Fax Calendar
				string str = "Do you really want to fax the calendar to this entire list? ";
				DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show(str, "Fax List Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (result == DialogResult.No)
					return;
				string save_custom = work_custom;
				string command = "select * from callfaxes ";
				command += "where specialty = '" + specialty + "' ";
				command += "order by Company ";
				DataTable dx = G1.get_db_data(command);
				//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
				//DataTable dx         = sqlCommand.ExecuteFill();
				int newrows = dx.Rows.Count;
				if (newrows <= 0)
					return;
				faxFile = save_calendar(); // Print the Calendar to a file
				string old_custom = "";
				for (int i = 0; i < newrows; i++)
				{
					string company = dx.Rows[i]["company"].ToString();
					string person = dx.Rows[i]["person"].ToString();
					string fax = dx.Rows[i]["phone"].ToString();
					string customize = dx.Rows[i]["customize"].ToString();
					string note = dx.Rows[i]["note"].ToString();
					m_fax_company = company;
					m_fax_to_who = person;
					m_fax_num = fax;
					if (customize != old_custom)
					{
						work_custom = customize;
						add_slots();
						fill_calendar(work_date);
						remove_slots();
						dg_cleanup();
						faxFile = save_calendar(); // Print the Calendar to a file
						old_custom = customize;
					}
					fax_calendar(faxFile, note);
				}
				if (work_custom != save_custom)
				{
					work_custom = save_custom;
					add_slots();
					fill_calendar(work_date);
					remove_slots();
					dg_cleanup();
				}
			}
		}
		/*****************************************************************************************/
		private void call_Click(object sender, System.EventArgs e)
		{
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string call = jj.Text;
			setup_call("CALL", call);
		}
		/*****************************************************************************************/
		private void closed_Click(object sender, System.EventArgs e)
		{
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string call = jj.Text;
			if (call.Trim().ToUpper() == "ENTER SOMETHING")
			{
				using (Ask askform = new Ask("What would you like to put here? "))
				{
					askform.Text = "";
					askform.ShowDialog();
					string answer = askform.Answer;
					if (answer.Trim().Length > 0)
						update_call_day(answer, "");
				}
			}
			else if (call.Trim().ToUpper() == "CLEAR SOMETHING")
			{
				clear_something();
			}
			else
				setup_call("CLOSED", call);
		}
		/*****************************************************************************************/
		private void cover_Click(object sender, System.EventArgs e)
		{
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string call = jj.Text;
			setup_call("COVER", call);
		}
		/***************************************************************************************/
		private void setup_call(string where, string what)
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			int row = dgv.CurrentRowIndex;
			int col = dgv.CurrentCell.ColumnNumber;
			string str = popup_doctor;
			string fname = "";
			string lname = "";
			string consult = "";
			string call = "";
			bool er = false;
			parse_name(str, ref fname, ref lname, ref consult, ref call, ref er);
			string initials = str;
			if (fname.Length > 0 && lname.Length > 0)
				initials = fname.ToUpper().Substring(0, 1) + lname.ToUpper().Substring(0, 1);
			if (where.Trim().ToUpper() == "CALL")
			{
				if (what.Trim().ToUpper() == "CON")
					str = consult;
				else if (what.Trim().ToUpper() == "CALL")
					str = call;
				else if (what.Trim().ToUpper() == "HOSP")
					str = initials;
				else
					str = call;
				str += "," + what;
			}
			else if (where.ToUpper() == "CLOSED")
				str = "CLOSED," + what;
			else
			{
				str = lname;
				parse_name(what, ref fname, ref lname, ref consult, ref call, ref er);
				str += "," + popup_call + "," + consult;
			}
			string er_name = "";
			if (what.Trim().ToUpper() == "CON")
				er_name = call;
			else if (er)
				er_name = call;
			update_call_day(str, er_name);
			popup_doctor = "";
			popup_call = "";
			popup_cover = "";
			dg_cleanup();
		}
		/***************************************************************************************/
		private void update_call_day(string str, string er_name)
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			int row = dgv.CurrentRowIndex;
			int col = dgv.CurrentCell.ColumnNumber;
			string who = "";
			string what = "";
			string cover = "";
			str += ",";
			int idx = str.IndexOf(",");
			if (idx > 0)
			{
				who = str.Substring(0, idx);
				if ((idx + 1) < str.Length)
				{
					str = str.Substring((idx + 1));
					idx = str.IndexOf(",");
					if (idx > 0)
					{
						what = str.Substring(0, idx);
						if ((idx + 1) < str.Length)
						{
							str = str.Substring((idx + 1));
							cover = str.Replace(",", "");
						}
					}
				}
			}
			int iday = 0;
			string text = rc[row][col].ToString().Trim();
			if (text.Trim().Length == 0)
				return;
			text += "\n";
			idx = text.IndexOf("\n");
			if (idx > 0)
			{
				string day = text.Substring(0, idx);
				if (G1.validate_numeric(day))
					iday = G1.myint(day);
			}
			if (iday <= 0)
				return;
			DateTime date = this.dateTimePicker1.Value;
			string newdate = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + iday.ToString("D2");
			if (what.Trim().ToUpper() == "CLEAR")
				clear_call(newdate, popup_doctor);
			else
			{
				string record = "";
				//string record  = G1.create_call_record ();
				if (record.Trim().Length > 0)
				{
					//G1.update_call ( record, "specialty", specialty );
					//G1.update_call ( record, "dat", newdate );
					//G1.update_call ( record, "doctor", popup_doctor );
					//G1.update_call ( record, "who", who );
					//G1.update_call ( record, "what", what );
					//G1.update_call ( record, "cover", cover );
					//G1.update_call ( record, "er",    er_name );
				}
			}
			setup_call_day(iday);
		}
		/***************************************************************************************/
		private void clear_something()
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			int row = dgv.CurrentRowIndex;
			int col = dgv.CurrentCell.ColumnNumber;
			int iday = 0;
			string text = rc[row][col].ToString().Trim();
			if (text.Trim().Length == 0)
				return;
			text += "\n";
			int idx = text.IndexOf("\n");
			if (idx > 0)
			{
				string day = text.Substring(0, idx);
				if (G1.validate_numeric(day))
					iday = G1.myint(day);
			}
			if (iday <= 0)
				return;
			DateTime date = this.dateTimePicker1.Value;
			string newdate = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + iday.ToString("D2");
			clear_date = new DateTime(date.Year, date.Month, iday);
			string[] HeaderLines = text.Split('\n');
			int LineCount = HeaderLines.Length;
			using (ListSelect form = new ListSelect(text, false))
			{
				form.Text = "List";
				form.ListDone += Form_ListDone;
				form.ShowDialog();
			}
		}
		public delegate void d_void_eventdone_string(string s);
		public event d_void_eventdone_string listDone;
		/***************************************************************************************/
		private void Form_ListDone(string s)
		{
			s = s.TrimEnd(',');
			string newdate = clear_date.Year.ToString("D4") + "-" + clear_date.Month.ToString("D2") + "-" + clear_date.Day;
			string command = "select * from oncall ";
			command += "where specialty = '" + specialty + "' ";
			command += "and dat = '" + newdate + "' ";
			command += "and location <> 'CLINIC' ";
			command += "order by doctor ";
			DataTable dx = G1.get_db_data(command);
			int newrows = dx.Rows.Count;
			DataRowCollection rx = dx.Rows;
			for (int i = 0; i < dx.Rows.Count; i++)
			{
				string who = rx[i]["who"].ToString();
				if (who.Trim().ToUpper() == s.Trim().ToUpper())
				{
					string record = rx[i]["callkey"].ToString();
					if (record.Trim().Length > 0)
					{
						//G1.delete_call(record);
						setup_call_day(clear_date.Day);
						dg_cleanup();
						break;
					}
				}
			}
			return;
		}

		/***************************************************************************************/
		private void clear_call(string newdate, string doctor)
		{
			string command = "select * from oncall ";
			command += "where specialty = '" + specialty + "' ";
			command += "and dat = '" + newdate + "' ";
			command += "and doctor = '" + doctor + "' ";
			command += "and location <> 'CLINIC' ";
			command += "order by doctor ";
			DataTable dx = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx = sqlCommand.ExecuteFill();
			int newrows = dx.Rows.Count;
			DataRowCollection rx = dx.Rows;
			for (int i = 0; i < newrows; i++)
			{
				string record = rx[i]["callkey"].ToString();
				//if ( record.Trim().Length > 0 )
				//	G1.delete_call ( record );
			}
		}
		/***************************************************************************************/
		private void setup_call_day(int iday)
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			DateTime date = this.dateTimePicker1.Value;
			int dd = date.Day;
			DateTime ldate = date.AddDays((double)(-(dd) + 1));
			int ddow = (int)ldate.DayOfWeek;
			ldate = ldate.AddDays((double)(iday - 1));
			int dow = (int)ldate.DayOfWeek;
			int row = ((iday + ddow - 1) / 7);
			if (row < 0)
				row = 0;
			string newdate = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + iday.ToString("D2");
			string command = "select * from oncall ";
			command += "where specialty = '" + specialty + "' ";
			command += "and dat = '" + newdate + "' ";
			command += "and location <> 'CLINIC' ";
			command += "order by practitioner, doctor ";
			DataTable dx = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx = sqlCommand.ExecuteFill();
			int newrows = dx.Rows.Count;
			DataRowCollection rx = dx.Rows;
			string text = iday.ToString();
			if (newrows > 0)
			{
				for (int i = 0; i < newrows; i++)
				{
					string who = rx[i]["who"].ToString();
					string what = rx[i]["what"].ToString().Trim().ToUpper();
					string er = rx[i]["er"].ToString().Trim().ToUpper();
					if (work_custom == "ER")
					{
						if (er.Trim().Length == 0)
						{
							if (what.Trim().ToUpper() != "CALL" && what.Trim().ToUpper() != "HOSP")
								continue;
						}
						if (what.Trim().ToUpper() == "CON")
							who = "(" + rx[i]["er"].ToString() + ")";
					}
					else if (work_custom == "ANSWERCALL")
					{
						if (what.Trim().ToUpper() != "CALL" && what.Trim().ToUpper() != "7 PM - 7 AM")
							continue;
					}
					else if (work_custom == "FLOORS")
					{
						if (er.Trim().Length == 0)
						{
							if (what.Trim().ToUpper() != "CALL" && what.Trim().ToUpper() != "HOSP")
								continue;
						}
						if (what.Trim().ToUpper() == "CON")
							continue;
					}
					string cover = rx[i]["cover"].ToString();
					text += "\n" + who + "," + what;
					if (cover.Trim().Length > 0)
						text += "," + cover;
				}
			}
			rc[row][dow + 1] = text;
			dg_cleanup();
		}
		/***************************************************************************************/
		private void parse_name(string who, ref string fname, ref string lname, ref string consult, ref string call, ref bool er)
		{
			fname = "";
			lname = who;
			consult = "X";
			call = who;
			er = false;
			if (who.Trim().Length == 0)
				return;

			int idx = who.IndexOf(" ");
			if (idx > 0)
			{
				fname = who.Substring(0, idx);
				if ((idx + 1) < who.Length)
					lname = who.Substring((idx + 1)).Trim();
			}

			if (lname.Trim().Length == 0)
				return;
			consult = lname.Substring(0, 1);
			call = lname;

			string command = "select * from doctors ";
			command += "where fname = '" + fname + "' ";
			command += "and lname = '" + lname + "' ";
			command += "order by doc ";
			DataTable dx = G1.get_db_data(command);
			//SqlCommand sqlCommand = new SqlCommand(command, EMRConfig.WebServer);
			//DataTable dx = sqlCommand.ExecuteFill();
			int newrows = dx.Rows.Count;
			DataRowCollection rc = dx.Rows;
			if (newrows <= 0)
				return;
			//if ( rc[0]["er"].ToString() == "1" )
			if (rc[0]["er"].ObjToBool())
				er = true;
			string con = rc[0]["con"].ToString();
			if (con.Trim().Length > 0)
			{
				consult = con;
				call = fname.Substring(0, 1) + " " + lname;
			}
		}
		/***************************************************************************************/
		private void doctor_Popup(object sender, EventArgs e)
		{
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string str = jj.Text.Trim();
			if (str.Trim().ToUpper() == "ENTER SOMETHING")
			{
				str += "";
			}
			else
				popup_doctor = str;
		}
		/***************************************************************************************/
		private void call_Popup(object sender, EventArgs e)
		{
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string str = jj.Text.Trim();
			popup_call = str;
		}
		/***************************************************************************************/
		private void cover_Popup(object sender, EventArgs e)
		{
			ToolStripMenuItem jj = (ToolStripMenuItem)sender;
			string str = jj.Text.Trim();
			popup_cover = str;
		}
		/***************************************************************************************/
		private void remove_slots()
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			if (dt.Columns.Count <= 0)
				return;
			int maxrow = rc.Count;
			bool found = false;
			for (int i = 0; i < 6; i++)
			{
				string text = rc[maxrow - 1][i + 1].ToString();
				if (text.Trim().Length > 0)
					found = true;
			}
			if (!found)
				rc.RemoveAt(maxrow - 1);
			dg_cleanup();
		}
		/***************************************************************************************/
		private void add_slots()
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			int rows = dt.Rows.Count;
			int columns = dt.Columns.Count;
			if (columns <= 0)
				return;
			for (int i = (rows - 1); i >= 0; i--)
				dt.Rows.RemoveAt(i);

			int col = G1.get_column_number(dgv, "Slots");
			for (int i = 0; i < 6; i++)
			{
				DataRow newRow = dt.NewRow();
				newRow["Sunday"] = "";
				newRow["Monday"] = "";
				newRow["Tuesday"] = "";
				newRow["Wednesday"] = "";
				newRow["Thursday"] = "";
				newRow["Friday"] = "";
				newRow["Saturday"] = "";
				dt.Rows.Add(newRow);
			}
			dg_cleanup();
		}
		/***********************************************************************************************/
		private void AutoSizeGrid(int hin, DataGrid dg1)
		{
			// DataGrid should be bound to a DataTable for this part to 
			// work. 
			DataTable dt = G1.GridtoTable(dg1);
			int numRows = dt.Rows.Count;
			Graphics g = Graphics.FromHwnd(dg1.Handle);
			StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
			SizeF size;
			calculate_widths(g);

			// Since DataGridRows[] is not exposed directly by the DataGrid 
			// we use reflection to hack internally to it.. There is actually 
			// a method get_DataGridRows that returns the collection of rows 
			// that is what we are doing here, and casting it to a System.Array 
			MethodInfo mi = dg1.GetType().BaseType.GetMethod("get_DataGridRows",
				BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Instance
				| BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

			if (mi == null)
				mi = dg1.GetType().GetMethod("get_DataGridRows",
					BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Instance
					| BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);


			System.Array dgra = (System.Array)mi.Invoke(dg1, null);

			// Convert this to an ArrayList, little bit easier to deal with 
			// that way, plus we can strip out the newrow row. 
			ArrayList DataGridRows = new ArrayList();
			foreach (object dgrr in dgra)
			{
				if (dgrr.ToString().EndsWith("DataGridRelationshipRow") == true)
					DataGridRows.Add(dgrr);
			}

			// Now loop through all the rows in the grid 
			DataRowCollection rc = dt.Rows;

			string cellData = "";
			for (int i = 0; i < numRows; ++i)
			{
				// Here we are telling it that the column width is set to 
				// 400.. so size will contain the Height it needs to be. 
				//				size = g.MeasureString(dg1[i,1].ToString(),dg1.Font,400,sf); 

				size = g.MeasureString(dg1[i, 0].ToString(), dg1.Font, 400, sf);
				int h = Convert.ToInt32(size.Height);
				// Little extra cellpadding space 
				h = h + 8;
				h = h - 2;
				if (hin > 0)
					h = hin;
				h = calculate_height(i, h, size, g);
				// Now we pick that row out of the DataGridRows[] Array 
				// that we have and set it's Height property to what we 
				// think it should be. 
				PropertyInfo pi = DataGridRows[i].GetType().GetProperty("Height");
				pi.SetValue(DataGridRows[i], h, null);

				for ( int j=0; j<7; j++)
                {
					cellData = dg1[i, j].ObjToString();
					if (cellData.ToUpper().IndexOf("COLOR") > 0)
					{
					}
				}

				// I have read here that after you set the Height in this manner that you should 
				// Call the DataGrid Invalidate() method, but I haven't seen any prob with not calling 
			}
			//			Invalidate();
			g.Dispose();
		}
		/****************************************************************************************/
		private void calculate_widths(Graphics g)
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			for (int j = 0; j < dt.Columns.Count; j++)
			{
				int width = dgv.TableStyles[0].GridColumnStyles[j].Width;
				if (width <= 0)
					continue;
				string str = dgv.TableStyles[0].GridColumnStyles[j].HeaderText;
				if (str.Trim().Length == 0)
					continue;
				SizeF stringSize = new SizeF();
				stringSize = g.MeasureString(str, dgv.Font);
				int new_width = (int)stringSize.Width;
				int csize = new_width / str.Length;
				new_width += csize;
				if (new_width > width)
					dgv.TableStyles[0].GridColumnStyles[j].Width = new_width;
			}
			int t_width = 50;  // Start with row header width
			for (int j = 0; j < dt.Columns.Count; j++)
			{
				int width = dgv.TableStyles[0].GridColumnStyles[j].Width;
				//int width = 50;
				if (width <= 0)
					continue;
				t_width += width;
			}
			int x = this.Left;
			int y = this.Top;
			int height = this.Height;
			if (t_width < original_width)
				t_width = original_width;
			this.SetBounds(x, y, t_width, height);
		}
		/****************************************************************************************/
		private int calculate_height(int row, int h, SizeF size, Graphics g)
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			int height = h * 5;
			return height;
		}
		/***********************************************************************************************/
		private string get_doctor_status(int i_doc, DateTime today)
		{
			if (i_doc <= 0 || i_doc > 99)
				return "";
			string doctor = convert_idoctor(i_doc);
			string newdate = today.Year.ToString("D4") + "-" + today.Month.ToString("D2") + "-" + today.Day.ToString("D2");
			string command = "select * from oncall ";
			command += "where dat = '" + newdate + "' ";
			command += "and doctor = '" + doctor + "' ";
			command += "order by doctor, location ";
			DataTable dx = G1.get_db_data(command);
			DataRowCollection rx = dx.Rows;
			string call = "";
			for (int j = 0; j < rx.Count; j++)
			{
				string now = "";
				string what = rx[j]["what"].ToString().Trim().ToUpper();
				if (now.ToUpper() == "CALL")
				{
					if (what.Trim().ToUpper() == "CON")
						now = "C&C";
				}
				else if (now.ToUpper() == "CON")
				{
					if (what.Trim().ToUpper() == "CALL")
						now = "C&C";
				}
				else
				{
					if (what == "CALL" || what == "CON" || what == "OFF" || what == "OUT" ||
						what == "DC" || what == "C&C" || what == "HOSP")
						now = what;
				}
				call = now.ToUpper();
			}
			return call;
		}
		/***************************************************************************************/
		private void dateTimePicker1_ValueChanged(object sender, System.EventArgs e)
		{
			work_date = this.dateTimePicker1.Value;
			add_slots();
			fill_calendar(work_date);
			remove_slots();
			dg_cleanup();
		}
		/***************************************************************************************/
		private void menuItem3_Click(object sender, System.EventArgs e)
		{ // Exit Calendar
			this.Close();
		}
		/***************************************************************************************/
		private int pageNumber;
		private int poshold;
		/***************************************************************************************/
		private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			pageNumber = 0;
			poshold = 0;
		}
		/***************************************************************************************/
		private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			if (work_call)
			{
				string file = save_calendar(e);
				DevExpress.XtraEditors.XtraMessageBox.Show("Calendar file saved to file:\n" + file + ".");
			}
			else
				print_calendar(e);
		}
		/***************************************************************************************/
		private void print_calendar(System.Drawing.Printing.PrintPageEventArgs printArgs)
		{
			int sx = printArgs.PageBounds.Left;
			int sy = printArgs.PageBounds.Top;
			int sizexx = printArgs.PageBounds.Width;
			int sizeyy = printArgs.PageBounds.Height;
			Bitmap bmp = new Bitmap(sizexx, sizeyy);
			Graphics gg = Graphics.FromImage(bmp);
			gg.FillRectangle(Brushes.White, new Rectangle((int)sx, (int)sy, sizexx, sizeyy));
			pageNumber++;
			int pagepos = 0, listpos = poshold;
			DataGrid dg1 = this.dgv;
			DataTable dt = G1.GridtoTable(dg1);
			DataRowCollection rx = dt.Rows;
			string str;
			int crows = dg1.CurrentRowIndex;
			int rows = dt.Rows.Count;
			int columns = dt.Columns.Count;
			int j; //i, 
			Brush myBrush = Brushes.Black;
			myBrush = Brushes.Black;
			Font drawFont = new Font("Times New Roman", 9);
			PointF drawPoint = new PointF(50.0F, 50.0F);
			float startx = (float)50.0;
			float y = (float)50.0;
			float x = (float)startx;

			int height = drawFont.Height;
			str = "Vacation Calendar";
			DateTime newdate = dateTimePicker1.Value;
			str += " for the Month of " + newdate.ToString("MM/dd/yyyy");
			drawPoint = new PointF(x, y);
			Font topFont = new Font("Times New Roman", 12);
			printArgs.Graphics.DrawString(str, topFont, Brushes.Black, drawPoint);
			gg.DrawString(str, topFont, Brushes.Black, drawPoint);
			y = y + height + height;
			pagepos = pagepos + 2;
			for (j = 0; j < columns; j++)
			{
				int width = dg1.TableStyles[0].GridColumnStyles[j].Width;
				if (width <= 0)
					continue;
				x = x + width;
			}
			printArgs.Graphics.FillRectangle(Brushes.LightGray, new Rectangle((int)startx, (int)y, (int)x, height));
			gg.FillRectangle(Brushes.LightGray, new Rectangle((int)startx, (int)y, (int)x, height));
			x = (float)startx;
			for (j = 0; j < columns; j++)
			{
				int width = dg1.TableStyles[0].GridColumnStyles[j].Width;
				if (width <= 0)
					continue;
				str = dg1.TableStyles[0].GridColumnStyles[j].HeaderText;
				drawPoint = new PointF(x, y);
				printArgs.Graphics.DrawString(str, drawFont, myBrush, drawPoint);
				gg.DrawString(str, drawFont, myBrush, drawPoint);
				x = x + width;
			}
			y = y + height;
			pagepos = pagepos + 1;

			Graphics g = Graphics.FromHwnd(dg1.Handle);
			int linesperpage = (int)(((printArgs.PageBounds.Height) / drawFont.GetHeight(printArgs.Graphics)) - 5);
			Pen p = new Pen(new SolidBrush(Color.Gray));
			while (pagepos < linesperpage && listpos < rows)
			{
				SizeF size = g.MeasureString(dg1[listpos, 0].ToString(), dg1.Font);
				int h = Convert.ToInt32(size.Height);
				h = calculate_dg_height(listpos, height, size, g);
				h = 100;
				x = (float)startx;
				for (j = 0; j < columns; j++)
				{
					str = rx[listpos][j].ToString();
					string day = "";
					int idx = str.IndexOf("\n");
					if (idx > 0)
					{
						day = str.Substring(0, idx);
						if ((idx + 1) < str.Length)
							str = str.Substring((idx + 1));
						else
							str = "";
					}
					else
					{
						day = str;
						str = "";
					}
					int width = dg1.TableStyles[0].GridColumnStyles[j].Width;
					if (width <= 0)
						continue;
					drawPoint = new PointF(x, y);
					printArgs.Graphics.DrawString(str, drawFont, myBrush, drawPoint);
					gg.DrawString(str, drawFont, myBrush, drawPoint);
					printArgs.Graphics.DrawRectangle(p, new Rectangle((int)x, (int)y, (int)width, (int)h));
					gg.DrawRectangle(p, new Rectangle((int)x, (int)y, (int)width, (int)h));
					x = x + width;

					Font myfont = new Font("Times New Roman", 14f, FontStyle.Bold);
					SizeF mysize = g.MeasureString(day, myfont);
					drawPoint = new PointF((x - mysize.Width - 2), y);
					printArgs.Graphics.DrawString(day, myfont, myBrush, drawPoint);
					gg.DrawString(day, myfont, myBrush, drawPoint);
				}
				y = y + h;
				pagepos++;
				listpos++;
			}
			if (listpos < rows)
			{
				poshold = listpos;
				printArgs.HasMorePages = true;
			}
			else
				printArgs.HasMorePages = false;
			bmp.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
			//bmp.Save       ( calendar_temp,System.Drawing.Imaging.ImageFormat.Jpeg );
		}

		/***************************************************************************************/
		private string save_calendar(System.Drawing.Printing.PrintPageEventArgs printArgs = null)
		{
			int sx = 0;
			int sy = 180;
			int sizexx = 850;
			int sizeyy = 850;
			if (printArgs != null)
			{
				sx = printArgs.PageBounds.Left;
				sy = printArgs.PageBounds.Top;
				sizexx = printArgs.PageBounds.Width;
				sizeyy = printArgs.PageBounds.Height;
			}
			Bitmap bmp = new Bitmap(sizexx, sizeyy);
			Graphics gg = Graphics.FromImage(bmp);
			Rectangle rect = new Rectangle(sx, sy, sizexx, sizeyy);
			gg.SetClip(rect);
			gg.FillRectangle(Brushes.White, new Rectangle((int)sx, (int)sy, sizexx, sizeyy));

			pageNumber++;
			int pagepos = 0, listpos = poshold;
			DataGrid dg1 = this.dgv;
			DataTable dt = G1.GridtoTable(dg1);
			DataRowCollection rx = dt.Rows;
			string str;
			int crows = dg1.CurrentRowIndex;
			int rows = dt.Rows.Count;
			int columns = 0;

			for (int j = 0; j < dt.Columns.Count; j++)
			{
				int width = dg1.TableStyles[0].GridColumnStyles[j].Width;
				if (width > 0)
					columns = columns + 1;
			}
			Brush myBrush = Brushes.Black;
			myBrush = Brushes.Black;
			Font drawFont = new Font("Times New Roman", 9);
			float startx = (float)50.0;
			float y = (float)50.0;
			float x = (float)startx;

			if (printArgs == null)
			{
				startx = (float)0.0;
				y = (float)180.0;
				x = (float)startx;
			}

			int height = drawFont.Height;
			DateTime newdate = dateTimePicker1.Value;
			string year = newdate.Year.ToString();
			int mm = newdate.Month;
			string month = G1.decode_month(mm);
			str = month + " - " + specialty;
			string company = "SMFS";
			str += " (" + company + ")";
			Font topFont = new Font("Times New Roman", 24, FontStyle.Bold | FontStyle.Italic);
			SizeF xsize = gg.MeasureString(str, topFont);
			if (printArgs != null)
				xsize = printArgs.Graphics.MeasureString(str, topFont);
			float new_x = sx + (sizexx / 2) - (xsize.Width / 2);
			float new_y = y;
			PointF drawPoint = new PointF(new_x, new_y);
			if (printArgs != null)
				printArgs.Graphics.DrawString(str, topFont, Brushes.Black, drawPoint);
			gg.DrawString(str, topFont, Brushes.Black, drawPoint);
			y = y + xsize.Height;


			xsize = gg.MeasureString(year, topFont);
			if (printArgs != null)
				xsize = printArgs.Graphics.MeasureString(year, topFont);
			new_x = x + xsize.Width;
			string leftside = year;
			topFont = new Font("Times New Roman", 18, FontStyle.Bold);
			drawPoint = new PointF(new_x, (new_y + 6));
			if (printArgs != null)
				printArgs.Graphics.DrawString(leftside, topFont, Brushes.Black, drawPoint);
			gg.DrawString(leftside, topFont, Brushes.Black, drawPoint);

			xsize = gg.MeasureString(year, topFont);
			if (printArgs != null)
				xsize = printArgs.Graphics.MeasureString(year, topFont);
			new_x = (sx + sizexx) - (xsize.Width * 2);
			drawPoint = new PointF(new_x, (new_y + 6));
			if (printArgs != null)
				printArgs.Graphics.DrawString(year, topFont, Brushes.Black, drawPoint);
			gg.DrawString(year, topFont, Brushes.Black, drawPoint);

			pagepos = pagepos + 1;
			for (int j = 0; j < dt.Columns.Count; j++)
			{
				int width = dg1.TableStyles[0].GridColumnStyles[j].Width;
				if (width <= 0)
					continue;
				if (printArgs == null)
					width = sizexx / columns;
				x = x + width;
			}
			if (printArgs != null)
				printArgs.Graphics.FillRectangle(Brushes.Black, new Rectangle((int)startx, (int)y, (int)x, height));
			gg.FillRectangle(Brushes.Black, new Rectangle((int)startx, (int)y, (int)x, height));
			drawFont = new Font("Times New Roman", 9, FontStyle.Bold);
			x = (float)startx;
			for (int j = 0; j < dt.Columns.Count; j++)
			{
				int width = dg1.TableStyles[0].GridColumnStyles[j].Width;
				if (width <= 0)
					continue;
				if (printArgs == null)
					width = sizexx / columns;
				str = dg1.TableStyles[0].GridColumnStyles[j].HeaderText;
				drawPoint = new PointF(x, y);
				if (printArgs != null)
					printArgs.Graphics.DrawString(str, drawFont, Brushes.White, drawPoint);
				gg.DrawString(str, drawFont, Brushes.White, drawPoint);
				x = x + width;
			}
			y = y + height;
			pagepos = pagepos + 1;

			drawFont = new Font("Times New Roman", 9);
			Graphics g = Graphics.FromHwnd(dg1.Handle);
			int linesperpage = 66;
			if (printArgs != null)
				linesperpage = (int)(((printArgs.PageBounds.Height) / drawFont.GetHeight(printArgs.Graphics)) - 5);
			Pen p = new Pen(new SolidBrush(Color.Gray));
			while (pagepos < linesperpage && listpos < rows)
			{
				SizeF size = g.MeasureString(dg1[listpos, 1].ToString(), dg1.Font);
				int h = Convert.ToInt32(size.Height);
				h = calculate_box_height(listpos, height, size, g);
				x = (float)startx;
				for (int j = 0; j < dt.Columns.Count; j++)
				{
					str = rx[listpos][j].ToString();
					string day = "";
					int idx = str.IndexOf("\n");
					if (idx > 0)
					{
						day = str.Substring(0, idx);
						if ((idx + 1) < str.Length)
							str = str.Substring((idx + 1));
						else
							str = "";
					}
					else
					{
						day = str;
						str = "";
					}
					int width = dg1.TableStyles[0].GridColumnStyles[j].Width;
					if (width <= 0)
						continue;

					if (printArgs == null)
						width = sizexx / columns;

					print_box(str, x, y, width, h, printArgs, gg);

					if (printArgs != null)
						printArgs.Graphics.DrawRectangle(p, new Rectangle((int)x, (int)y, (int)width, (int)h));
					gg.DrawRectangle(p, new Rectangle((int)x, (int)y, (int)width, (int)h));
					x = x + width;

					Font myfontf = new Font("Times New Roman", 14f, FontStyle.Bold);
					SizeF mysize = g.MeasureString(day, myfontf);
					drawPoint = new PointF((x - mysize.Width - 2), y);
					if (printArgs != null)
						printArgs.Graphics.DrawString(day, myfontf, myBrush, drawPoint);
					gg.DrawString(day, myfontf, myBrush, drawPoint);
				}
				y = y + h;
				pagepos++;
				listpos++;
			}
			if (printArgs != null)
			{
				if (listpos < rows)
				{
					poshold = listpos;
					printArgs.HasMorePages = true;
				}
				else
					printArgs.HasMorePages = false;
			}
			bmp.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
			string calendarFile = GetTemporaryFilePath(".jpg");
			bmp.Save(calendarFile, System.Drawing.Imaging.ImageFormat.Jpeg);
			return calendarFile;
		}
		/***************************************************************************************/
		public static string GetTemporaryFilePath(string filename = null)
		{
			if (String.IsNullOrEmpty(filename))//Fully Random Filename+Extension
				return "C:/SMFS_DATA" + @"\" + Path.GetRandomFileName();

			if (!String.IsNullOrEmpty(filename) && filename.StartsWith(".")) //Random Filename with specific extension
				return
					Path.ChangeExtension(
						"C:/SMFS_DATA" + @"\" + Path.GetRandomFileName(), filename);

			return "C:/SMFS_DATA" + @"\" + filename;
		}
		/***************************************************************************************/
		private void print_box(string str, float x, float y, int width, int h, System.Drawing.Printing.PrintPageEventArgs e, Graphics gg)
		{
			Graphics g = Graphics.FromHwnd(dgv.Handle);
			Rectangle rect = new Rectangle((int)x, (int)y, (int)(width), (int)(h));
			string[] HeaderLines = str.Split('\n');
			int out_count = 0;
			int hosp_count = 0;
			int icount = HeaderLines.Length;
			for (int j = 0; j < icount; j++)
			{
				string hstr = HeaderLines[j].ToString();
				int idx = hstr.ToUpper().IndexOf(",AM ");
				if (idx > 0)
				{
					string who1 = hstr.Substring(0, idx);
					if ((j + 1) < icount)
					{
						for (int k = (j + 1); k < icount; k++)
						{
							string strr = HeaderLines[k].ToString();
							int ddx = strr.ToUpper().IndexOf(",PM ");
							if (ddx > 0)
							{
								string who2 = strr.Substring(0, ddx);
								if (who1.Trim().ToUpper() == who2.Trim().ToUpper())
								{
									string xstr = combine_ampm(hstr, strr);
									if (xstr.Trim().Length > 0)
									{
										HeaderLines[j] = xstr;
										HeaderLines[k] = "";
										break;
									}
								}
							}
						}
					}
				}
				else if (hstr.ToUpper().IndexOf(",PM ") > 0)
				{
					idx = hstr.ToUpper().IndexOf(",PM ");
					string who1 = hstr.Substring(0, idx);
					if ((j + 1) < icount)
					{
						for (int k = (j + 1); k < icount; k++)
						{
							string strr = HeaderLines[k].ToString();
							int ddx = strr.ToUpper().IndexOf(",AM ");
							if (ddx > 0)
							{
								string who2 = strr.Substring(0, ddx);
								if (who1.Trim().ToUpper() == who2.Trim().ToUpper())
								{
									string xstr = combine_ampm(strr, hstr);
									if (xstr.Trim().Length > 0)
									{
										HeaderLines[j] = xstr;
										HeaderLines[k] = "";
										break;
									}
								}
							}
						}
					}
				}
			}
			string consults = "";
			string closed = "";
			foreach (string xstr in HeaderLines)
			{
				string hstr = xstr.ToUpper();
				hstr += ",";
				string who = "";
				string what = "";
				string cover = "";
				int idx = hstr.IndexOf(",");
				if (idx >= 0)
				{
					who = hstr.Substring(0, idx);
					if ((idx + 1) < hstr.Length)
						hstr = hstr.Substring((idx + 1));
					else
						hstr = "";
				}
				idx = hstr.IndexOf(",");
				if (idx >= 0)
				{
					what = hstr.Substring(0, idx);
					if ((idx + 1) < hstr.Length)
						hstr = hstr.Substring((idx + 1));
					else
						hstr = "";
				}
				idx = hstr.IndexOf(",");
				if (idx >= 0)
				{
					cover = hstr.Substring(0, idx);
					if ((idx + 1) < hstr.Length)
						hstr = hstr.Substring((idx + 1));
					else
						hstr = "";
				}
				if (who.Trim().ToUpper() == "CLOSED")
				{
					closed = what;
					continue;
				}
				if (what.Trim().ToUpper() == "CON")
					consults = who;
				else if (what.Trim().ToUpper() == "CALL")
				{
					float fx = (float)rect.Left + 2;
					float fy = (float)rect.Top + 2;
					Font myfont = new Font("Times New Roman", 8f);
					if (e != null)
						e.Graphics.DrawString(who, myfont, Brushes.Black, fx, fy);
					gg.DrawString(who, myfont, Brushes.Black, fx, fy);
				}
				else if (what.Trim().ToUpper() == "HOSP")
				{
					if (who.Trim().Length > 0)
					{
						Font myfont = new Font("Times New Roman", 8f);
						SizeF ssize = gg.MeasureString(who, myfont);
						if (e != null)
							ssize = e.Graphics.MeasureString(who, myfont);
						float fx = (float)rect.Right - ssize.Width - 2;
						float fy = (float)rect.Bottom;
						fy = fy - ((hosp_count + 1) * (ssize.Height + 2));
						if (e != null)
							e.Graphics.DrawString(who, myfont, Brushes.Black, fx, fy);
						gg.DrawString(who, myfont, Brushes.Black, fx, fy);
						hosp_count = hosp_count + 1;
					}
				}
				else
				{
					if (who.Trim().Length > 0)
					{
						who += " " + what;
						if (cover.Trim().Length > 0)
							who += " (" + cover + ")";
						Font myfont = new Font("Times New Roman", 8f);
						SizeF ssize = gg.MeasureString(who, myfont);
						if (e != null)
							ssize = e.Graphics.MeasureString(who, myfont);
						float fx = (float)rect.Left + 2;
						float fy = (float)rect.Bottom;
						fy = fy - ((out_count + 1) * (ssize.Height + 2));
						if (e != null)
							e.Graphics.DrawString(who, myfont, Brushes.Black, fx, fy);
						gg.DrawString(who, myfont, Brushes.Black, fx, fy);
						out_count = out_count + 1;
					}
				}
			}
			if (closed.Trim().Length > 0)
			{
				string cc = "CLOSED";
				Font myfont = new Font("Times New Roman", 12f, FontStyle.Bold);
				SizeF ssize = gg.MeasureString(cc, myfont);
				if (e != null)
					ssize = e.Graphics.MeasureString(cc, myfont);
				float fx = (float)(((rect.Left + rect.Right) / 2) - (ssize.Width / 2));
				float fy = (float)(((rect.Top + rect.Bottom) / 2) - (ssize.Height));
				if (e != null)
					e.Graphics.DrawString(cc, myfont, Brushes.Black, fx, fy);
				gg.DrawString(cc, myfont, Brushes.Black, fx, fy);

				cc = closed;
				myfont = new Font("Times New Roman", 12f, FontStyle.Bold);
				ssize = gg.MeasureString(cc, myfont);
				if (e != null)
					ssize = e.Graphics.MeasureString(cc, myfont);
				fx = (float)(((rect.Left + rect.Right) / 2) - (ssize.Width / 2));
				fy = (float)(((rect.Top + rect.Bottom) / 2));
				if (e != null)
					e.Graphics.DrawString(cc, myfont, Brushes.Black, fx, fy);
				gg.DrawString(cc, myfont, Brushes.Black, fx, fy);
			}
			if (consults.Trim().Length > 0)
			{
				Font myfont = new Font("Times New Roman", 12f, FontStyle.Bold);
				SizeF ssize = gg.MeasureString(consults, myfont);
				if (e != null)
					ssize = e.Graphics.MeasureString(consults, myfont);
				float fx = (float)(((rect.Left + rect.Right) / 2) - (ssize.Width / 2));
				float fy = (float)(((rect.Top + rect.Bottom) / 2) - (ssize.Height / 2));
				if (out_count >= 4) // Move Consults to the right if left side too full
				{
					fx += ssize.Width;
					fy -= ssize.Height;
				}
				if (e != null)
					e.Graphics.DrawString(consults, myfont, Brushes.Black, fx, fy);
				gg.DrawString(consults, myfont, Brushes.Black, fx, fy);
			}
		}
		/****************************************************************************************/
		private int calculate_box_height(int row, int h, SizeF size, Graphics g)
		{
			DataTable dt = G1.GridtoTable(dgv);
			DataRowCollection rc = dt.Rows;
			int height = h * 8;
			if (rc.Count > 5) // Allow for larger Calendar
				height = (int)((float)h * 7.5f);
			return height;
		}
		/****************************************************************************************/
		private int calculate_dg_height(int row, int h, SizeF size, Graphics g)
		{
			DataTable dt = G1.GridtoTable(this.dgv);
			DataRowCollection rc = dt.Rows;
			int height = 0;
			for (int j = 0; j < dt.Columns.Count; j++)
			{
				int width = this.dgv.TableStyles[0].GridColumnStyles[j].Width;
				if (width <= 0)
					continue;
				string text = rc[row][j].ToString();
				string[] Lines = text.Split('\n');
				int Count = Lines.Length;
				int local = 0;
				foreach (string str in Lines)
				{
					local = local + h;
					SizeF stringSize = new SizeF();
					stringSize = g.MeasureString(str, dgv.Font);
					//					if ( stringSize.Width > size.Width )
					//						local = local + h;
					if (str.Trim().Length > 17)
						local = local + h;
				}
				if (local > height)
					height = local;
			}
			if (height <= 0)
				height = h;
			return height;
		}
		/************************************************************************************/
		private string combine_ampm(string am, string pm)
		{
			int idx = am.Trim().IndexOf(",");
			if (idx <= 0)
				return "";
			string doctor1 = am.Substring(0, idx).Trim().ToUpper();
			string str = am.Substring(0, 1);
			if ((idx + 1) >= am.Length)
				return "";
			str += " - " + am.Substring((idx + 1));

			idx = pm.Trim().IndexOf(",");
			if (idx <= 0)
				return "";
			string doctor2 = pm.Substring(0, idx).Trim().ToUpper();
			if (doctor1 != doctor2)
				return "";
			if ((idx + 1) >= pm.Length)
				return "";
			str += "/" + pm.Substring((idx + 1));
			return str;
		}
		/***************************************************************************************/
		private void printDocument1_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
		{ // Finish Print
		}
		/****************************************************************************************/
		private void fax_calendar(string faxFile, string additionNote)
		{  // Fax Calendar	
			if (String.IsNullOrWhiteSpace(faxFile))
				return;

			//Contact toContact = new Contact();
			//toContact.StoreName = m_fax_company;
			//toContact.StoreContact = m_fax_to_who;
			//toContact.Fax = m_fax_num;

			//FaxInfo info = new FaxInfo();
			//info.To_Contact = toContact;
			//info.Reason = m_fax_reason;
			//info.FaxType = m_fax_type;
			//info.CoverNote = additionNote;

			//Fax newFax = new Fax(info);
			//newFax.AddAttachment(faxFile);
			//newFax.SendFax();
			//CleanupFaxProperties();
		}
		/***************************************************************************************/
		private void CleanupFaxProperties()
		{
			m_fax_company = "";
			m_fax_to_who = "";
			m_fax_num = "";
			m_fax_reason = "";
			m_fax_type = "";
		}
		/***************************************************************************************/
		private void menuItem5_Click(object sender, System.EventArgs e)
		{ // Must Fax Calendar somewhere
		  //string calendarFile = save_calendar();
		  //FaxInfo faxInfo = LocalMethods.GetFaxInfo();
		  //if (faxInfo == null)
		  //	return;

			//Fax newFax = new Fax(faxInfo);
			//newFax.AddAttachment(calendarFile);
			//newFax.SendFax();
			//File.Delete(calendarFile);
		}
		/********************************************************************************************/
		private void DisposeCustom()
		{
			this.contextMenu1 = null;
			this.mainMenu1 = null;
			if (this.dgv != null)
				this.dgv.Dispose();
			this.dgv = null;
			if (this.printDocument1 != null)
				this.printDocument1.Dispose();
			this.printDocument1 = null;
		}
		/***************************************************************************************/
		private void Calendar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{ // Close Calendar
			DisposeCustom();
		}
		/****************************************************************************************/
		private void dg1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{ // Try to Highlight the current day rectangle
			if (e.Button == MouseButtons.Right)
			{
				DataGrid.HitTestInfo hi = dgv.HitTest(e.X, e.Y);
				dgv.CurrentCell = new DataGridCell(hi.Row, hi.Column);
				work_row = dgv.CurrentCell.RowNumber;
				work_col = dgv.CurrentCell.ColumnNumber;
			}
			else if (e.Button == MouseButtons.Left)
			{
				DataGrid.HitTestInfo hi = dgv.HitTest(e.X, e.Y);
				work_row = e.Y;
				work_col = e.X;
				dgv.CurrentCell = new DataGridCell(hi.Row, hi.Column);
				work_row = dgv.CurrentCell.RowNumber;
				work_col = dgv.CurrentCell.ColumnNumber;
			}
		}
		/***************************************************************************************/
		private void dg1_DoubleClick(object sender, System.EventArgs e)
		{ // Day Selected
		  //int row = work_row;
		  //int col = work_col;
		  //if ( row >= 0 && col >= 0 )
		  //{
		  //	DataTable dt         = G1.GridtoTable ( this.dgv );
		  //	DataRowCollection rc = dt.Rows;
		  //	string data          = rc[row][col].ToString();
		  //	data                += "\n";
		  //	int idx              = data.IndexOf ( "\n" );
		  //	if ( idx <= 0 )
		  //		return;
		  //	data                 = data.Substring ( 0, idx ).Trim();
		  //	if ( !G1.validate_numeric ( data ) )
		  //		return;
		  //	int day              = G1.myint ( data );
		  //	DateTime work_date   = this.dateTimePicker1.Value;
		  //	int month            = work_date.Month;
		  //	int year             = work_date.Year;
		  //	string newdate       = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
		  //	if ( CalendarDone != null )
		  //	{
		  //		CalendarDone ( dt );
		  //		this.Close   ();
		  //	}
		  //}
		}
		/***************************************************************************************/
		public delegate void d_string_eventdone_string(DataTable dt);
		public event d_string_eventdone_string CalendarDone;
		/***************************************************************************************/
		private void btnLeft_Click(object sender, EventArgs e)
		{
			DateTime date = this.dateTimePicker1.Value;
			date = date.AddMonths(-1);
			int days = DateTime.DaysInMonth(date.Year, date.Month);
			date = new DateTime(date.Year, date.Month, days);
			this.dateTimePicker1.Value = date;
		}
		/***************************************************************************************/
		private void btnRight_Click(object sender, EventArgs e)
		{
			DateTime date = this.dateTimePicker1.Value;
			date = date.AddMonths(1);
			int days = DateTime.DaysInMonth(date.Year, date.Month);
			date = new DateTime(date.Year, date.Month, days);
			this.dateTimePicker1.Value = date;
		}
		/***************************************************************************************/
		private void setVacationDayToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//int row = work_row;
			//int col = work_col;
			//if (row >= 0 && col >= 0)
			//{
			//	DataTable dt = G1.GridtoTable(this.dgv);
			//	DataRowCollection rc = dt.Rows;
			//	string data = rc[row][col].ToString();
			//	data += "\n";
			//	int idx = data.IndexOf("\n");
			//	if (idx <= 0)
			//		return;
			//	data = data.Substring(0, idx).Trim();
			//	if (!G1.validate_numeric(data))
			//		return;
			//	int day = G1.myint(data);
			//	DateTime work_date = this.dateTimePicker1.Value;
			//	int month = work_date.Month;
			//	int year = work_date.Year;
			//	string newdate = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
			//	//if (CalendarDone != null)
			//	//{
			//	//	CalendarDone(newdate);
			//	//	this.Close();
			//	//}
			//}
		}
		/***************************************************************************************/
		private void hoursToolStripMenuItem_Click(object sender, EventArgs e)
		{ // 8 Hours
			int row = work_row;
			int col = work_col;
			if (row >= 0 && col >= 0)
			{
				DataTable dt = G1.GridtoTable(this.dgv);
				DataRowCollection rc = dt.Rows;
				string data = rc[row][col].ToString();
				data += "\n";
				int idx = data.IndexOf("\n");
				if (idx <= 0)
					return;
				data = data.Substring(0, idx).Trim();
				if (!G1.validate_numeric(data))
					return;
				int day = G1.myint(data);
				DateTime work_date = this.dateTimePicker1.Value;
				int month = work_date.Month;
				int year = work_date.Year;
				string newdate = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
				rc[row][col] = data + "\n    8 Hours Vacation";

				data = data + "~" + newdate + " 8 Hours Vacation";

				AddVacationRow(data);
			}
		}
		/***************************************************************************************/
		private void hoursToolStripMenuItem1_Click(object sender, EventArgs e)
		{ // 4 Hours
			int row = work_row;
			int col = work_col;
			if (row >= 0 && col >= 0)
			{
				DataTable dt = G1.GridtoTable(this.dgv);
				DataRowCollection rc = dt.Rows;
				string data = rc[row][col].ToString();
				data += "\n";
				int idx = data.IndexOf("\n");
				if (idx <= 0)
					return;
				data = data.Substring(0, idx).Trim();
				if (!G1.validate_numeric(data))
					return;
				int day = G1.myint(data);
				DateTime work_date = this.dateTimePicker1.Value;
				int month = work_date.Month;
				int year = work_date.Year;
				string newdate = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
				rc[row][col] = data + "\n    4 Hours Vacation";

				data = data + "~" + newdate + " 4 Hours Vacation";

				AddVacationRow(data);
			}
		}
		/***************************************************************************************/
		private void hoursToolStripMenuItem2_Click(object sender, EventArgs e)
		{ // 2 Hours
			int row = work_row;
			int col = work_col;
			if (row >= 0 && col >= 0)
			{
				DataTable dt = G1.GridtoTable(this.dgv);
				DataRowCollection rc = dt.Rows;
				string data = rc[row][col].ToString();
				data += "\n";
				int idx = data.IndexOf("\n");
				if (idx <= 0)
					return;
				data = data.Substring(0, idx).Trim();
				if (!G1.validate_numeric(data))
					return;
				int day = G1.myint(data);
				DateTime work_date = this.dateTimePicker1.Value;
				int month = work_date.Month;
				int year = work_date.Year;
				string newdate = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
				rc[row][col] = data + "\n    2 Hours Vacation";

				data = data + "~" + newdate + " 2 Hours Vacation";

				AddVacationRow(data);
			}
		}
		/***************************************************************************************/
		private void removeVacationDayToolStripMenuItem_Click(object sender, EventArgs e)
		{ // Remove Vacation
			int row = work_row;
			int col = work_col;
			if (row >= 0 && col >= 0)
			{
				DataTable dt = G1.GridtoTable(this.dgv);
				DataRowCollection rc = dt.Rows;
				string data = rc[row][col].ToString();
				data += "\n";
				int idx = data.IndexOf("\n");
				if (idx <= 0)
					return;
				data = data.Substring(0, idx).Trim();
				if (!G1.validate_numeric(data))
					return;
				int day = G1.myint(data);
				DateTime work_date = this.dateTimePicker1.Value;
				int month = work_date.Month;
				int year = work_date.Year;
				string newdate = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
				rc[row][col] = data + "\n    Remove Vacation";

				data = data + "~" + newdate + " Remove Vacation";

				AddVacationRow(data);
			}
		}
		/***************************************************************************************/
		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			int row = work_row;
			int col = work_col;
			if (row >= 0 && col >= 0)
			{
				DataTable dt = G1.GridtoTable(this.dgv);
				DataRowCollection rc = dt.Rows;
				string data = rc[row][col].ToString();
				data += "\n";
				int idx = data.IndexOf("\n");
				if (idx <= 0)
					return;
				data = data.Substring(0, idx).Trim();
				if (!G1.validate_numeric(data))
					return;
				int day = G1.myint(data);
				DateTime work_date = this.dateTimePicker1.Value;
				int month = work_date.Month;
				int year = work_date.Year;
				string newdate = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
				rc[row][col] = data + "\n    Start Vacation";

				data = data + "~" + newdate + " Start Vacation";

				AddVacationRow(data);
			}
		}
		/***************************************************************************************/
		private void AddVacationRow(string data)
		{
			DataRow dRow = vacationDt.NewRow();
			dRow["what"] = data;
			vacationDt.Rows.Add(dRow);

			if (data.ToUpper().IndexOf("STOP VACATION") > 0)
				SetColors();
		}
		/***************************************************************************************/
		private void SetColors ()
        {
			string data = "";
			string[] Lines = null;

			int calStartDay = -1;
			int calStopDay = -1;
			int testRow = -1;
			int col = 0;

			DataTable dt = (DataTable)dgv.DataSource;

			for ( int i=0; i<vacationDt.Rows.Count; i++)
            {
				try
				{
					data = vacationDt.Rows[i]["what"].ObjToString();
					if (data.ToUpper().IndexOf("START VACATION") > 0)
					{
						Lines = data.Split('~');
						calStartDay = Lines[0].ObjToInt32();
					}
					else if (data.ToUpper().IndexOf("STOP VACATION") > 0)
					{
						if (calStartDay < 0)
							continue;
						Lines = data.Split('~');
						calStopDay = Lines[0].ObjToInt32();

						for ( int j=0; j<dt.Columns.Count; j++)
                        {
							for ( int k=0; k<dt.Rows.Count; k++)
                            {
								data = dt.Rows[k][j].ObjToString();
								if (String.IsNullOrWhiteSpace(data))
									continue;
								Lines = data.Split('~');
								testRow = Lines[0].ObjToInt32();
								if ( testRow >= calStartDay && testRow <= calStopDay )
                                {
									data += "~COLOR";
									dt.Rows[k][j] = data;
                                }
                            }
                        }

					}
				}
				catch ( Exception ex )
                {
                }
            }
			dg_cleanup();
		}
		/***************************************************************************************/
		private void toolStripMenuItem3_Click(object sender, EventArgs e)
		{
			int row = work_row;
			int col = work_col;
			if (row >= 0 && col >= 0)
			{
				DataTable dt = G1.GridtoTable(this.dgv);
				DataRowCollection rc = dt.Rows;
				string data = rc[row][col].ToString();
				data += "\n";
				int idx = data.IndexOf("\n");
				if (idx <= 0)
					return;
				data = data.Substring(0, idx).Trim();
				if (!G1.validate_numeric(data))
					return;
				int day = G1.myint(data);
				DateTime work_date = this.dateTimePicker1.Value;
				int month = work_date.Month;
				int year = work_date.Year;
				string newdate = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
				rc[row][col] = data + "\n    Stop Vacation";

				data = data + "~" + newdate + " Stop Vacation";

				AddVacationRow(data);
			}
		}
		/***************************************************************************************/
		private void btnAccept_Click(object sender, EventArgs e)
        {
			OnCalendarDone();
			this.Close();
        }

        private void dgv_Paint(object sender, PaintEventArgs e)
        {

        }

        /***************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        { // Print the Calendar
			using (PrintDialog pd = new PrintDialog())
			{
				//			pd.AllowPrintToFile = true;
				pd.Document = printDocument1;
				if (pd.ShowDialog() == DialogResult.OK)
				{
					printDocument1.DefaultPageSettings.Landscape = true;

					printDocument1.Print();
				}
			}
		}
		/***************************************************************************************/
		private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {  // Print Preview the Calendar
			printDocument1.DefaultPageSettings.Landscape = true;

			printPreviewDialog1.WindowState = FormWindowState.Maximized;
			printPreviewDialog1.PrintPreviewControl.Zoom = 1.0;
			printPreviewDialog1.ShowDialog();
        }
		/***************************************************************************************/
		protected void OnCalendarDone()
		{
			if (CalendarDone != null)
			{
				//DataTable dt = G1.GridtoTable(this.dgv);
				CalendarDone ( vacationDt );
			}
		}
 /***************************************************************************************/
        string fmrmyform_RegDone(string who, DataGrid dg)
        {
            //if (TemplateForm.draw_rtb != null)
            //    TemplateForm.draw_rtb.Dispose();
            //TemplateForm.draw_rtb = null;
            //int count = this.contextMenu1.Items.Count;
            //for (int i = (count - 1); i >= 0; i--)
            //{
            //    this.contextMenu1.Items[i].Dispose();
            //}
            //create_menu();
            return "Calendar";
        }
/***************************************************************************************/
	}
}
