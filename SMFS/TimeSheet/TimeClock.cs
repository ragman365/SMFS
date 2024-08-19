using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrintingLinks;
using GeneralLib;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TimeClock : DevExpress.XtraEditors.XtraForm
    {
        //public static User cUser = null;
        DateTime lastposting;
        /***********************************************************************************************/
        private bool allowTimeOffRequests = false;
        private string superList = "";
        private bool TimeClockSupervisor = false;
        private bool loading = true;
        private bool workGroup = false;
        private string workDate = "";
        private bool allEmployees = false;
        private string empno = "";
        private string empName = "";
        private string workMyName = "";
        private string workUserName = "";
        private string workEmpNo = "";
        private string workLocation = "";
        private string actualUsername = "";
        private string workMethod = "";
        private string workMyTimeKeeper = "";
        private bool workingUnapproved = false;
        private bool isSupervisor = false;
        private decimal workRate = 0;
        private DateTime workHireDate = DateTime.Now;
        private double workVacationOverride = 0D;
        /***********************************************************************************************/
        private string workEmpSalaried = "";
        private string workEmpStatus = "";
        private string workEmpType = "";
        private DateTime workStartTime = DateTime.Now;
        private DateTime workStopTime = DateTime.Now;
        private DataRow workDataRow = null;
        private bool timeProvided = false;
        private bool workPrintOnly = false;
        private DataTable printDt = null;
        /***********************************************************************************************/
        private long saveReportLdate = 0L;
        private long saveReportEdate = 0L;
        /***********************************************************************************************/
        private bool timeSheetOtherModified = false;
        private bool timeSheetContracModified = false;
        private bool timeSheetModified = false;
        private bool timeSheetSaved = false;
        private bool contractCheetModified = false;
        private bool employeeApproved = false;
        private bool managerApproved = false;
        private bool employeeApprovedIn = false;
        private bool managerApprovedIn = false;
        private string workOldRecord = "";
        private string printingWhat = "";
        private string saveManagerApproved = "";
        private string saveEmployeeApproved = "";
        /***********************************************************************************************/
        public TimeClock(bool group)
        {
            workGroup = group;
            allEmployees = true;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public TimeClock(string emp = "")
        {
            empno = emp;
            workEmpNo = emp;
            allEmployees = false;
            if (string.IsNullOrWhiteSpace(empno))
                allEmployees = true;
            InitializeComponent();
            timeSheetSaved = false;
            timeSheetModified = false;
        }
        /***********************************************************************************************/
        public TimeClock(string emp = "", string userName = "", string name = "")
        {
            empno = emp;
            workEmpNo = emp;
            empName = name;
            workUserName = userName;
            allEmployees = false;
            if (string.IsNullOrWhiteSpace(empno))
                allEmployees = true;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public TimeClock(DateTime startTime, DateTime stopTime, string emp = "", string userName = "", string name = "", bool printOnly = false, string oldRecord = "", DataRow dr = null )
        {
            empno = emp;
            workEmpNo = emp;
            empName = name;
            workUserName = userName;
            allEmployees = false;
            if (string.IsNullOrWhiteSpace(empno))
                allEmployees = true;
            workStartTime = startTime;
            workStopTime = stopTime;
            timeProvided = true;
            workPrintOnly = printOnly;
            workOldRecord = oldRecord;
            workDataRow = dr;
            InitializeComponent();
        }
        ///***********************************************************************************************/
        //public TimeClock(string emp, string indate, string method = "")
        //{
        //    empno = emp;
        //    workEmpNo = emp;
        //    workDate = indate;
        //    workMethod = method;
        //    allEmployees = false;
        //    InitializeComponent();
        //}
        /***********************************************************************************************/
        private bool is_supervisor = false;
        private bool is_timekeeper = false;
        /***********************************************************************************************/
        private void TimeClock_Load(object sender, System.EventArgs e)
        {
            //if (1 == 1)
            //    return;

            lblPostingAsOf.Hide();
            label1.Hide();
            label2.Hide();
            txtAvailablePTO.Hide();
            txtDecemberPTO.Hide();
            chkUnapproved.Hide();
            lblCycleNote.Hide();
            txtCycleNote.Hide();

            menuSupervisors.Dispose();
            menuOptions.Dispose();
            menuHelp.Dispose();
            menuEditHelp.Dispose();
            barSubItem9.Dispose();
            menuHelpItem.Dispose();


            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            DateTime newDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime testDate = DateTime.Now;
            DateTime beginDate = new DateTime(2022, 12, 23);
            try
            {
                for (; ; )
                {
                    newDate = beginDate;
                    testDate = newDate.AddDays(13);
                    if (now >= newDate && now <= newDate.AddDays(13))
                    {
                        startDate = newDate;
                        stopDate = startDate.AddDays(14);
                        break;
                    }
                    beginDate = beginDate.AddDays(14);
                }
            }
            catch (Exception ex)
            {
                startDate = DateTime.Now;
            }


            //DateTime startDate = DateTime.Now;
            //DateTime stopDate = DateTime.Now;
            //DateTime now = DateTime.Now;
            //DateTime newDate = DateTime.Now;
            //DateTime beginDate = new DateTime(2022, 12, 23);
            //for (; ; )
            //{
            //    newDate = beginDate;
            //    if (now >= newDate && now <= newDate.AddDays(13))
            //    {
            //        startDate = newDate;
            //        break;
            //    }
            //    beginDate = beginDate.AddDays(14);
            //}
            int count = 0;
            //for (; ;)
            //{
            //    DayOfWeek dow = now.DayOfWeek;
            //    if ( dow == DayOfWeek.Friday )
            //    {
            //        count++;
            //        if ( count >= 2 )
            //        {
            //            startDate = now;
            //            break;
            //        }
            //        now = now.AddDays(-1);
            //        continue;
            //    }
            //    now = now.AddDays(-1);
            //}


            if (timeProvided)
            {
                this.dateTimePicker1.Value = workStartTime;
                this.dateTimePicker2.Value = workStopTime;
            }
            else
            {
                this.dateTimePicker1.Value = startDate;
                this.dateTimePicker2.Value = stopDate;
            }

            stopDate = this.dateTimePicker2.Value;
            DateTime checkDate = stopDate.AddDays(-14);
            DateTime date1 = new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, 17, 0, 0);
            //if ( DateTime.Now <= date1 )
            //{
            //    this.dateTimePicker1.Value = this.dateTimePicker1.Value.AddDays(-14);
            //    this.dateTimePicker2.Value = this.dateTimePicker2.Value.AddDays(-14);
            //}

            SetupTotalsSummary();

            btnAddNextPunch.Hide();
            btnAddNextPunch.Refresh();

            Rectangle rect = btnClock.Bounds;
            btnDecimal.SetBounds(rect.Left, rect.Top, rect.Width, rect.Height);
            btnDecimal.Hide();
            btnClock.Hide();
            rect = Screen.FromControl(this).Bounds;
            if (rect.Width < this.Width)
            {
                int top = this.Top;
                int left = this.Left;
                int height = this.Height;
                int width = this.Width;
                width = rect.Width;
                this.SetBounds(left, top+25, width, height);
            }
            else
            {
                int top = this.Top;
                int left = this.Left;
                int height = this.Height;
                int width = this.Width;
                this.SetBounds(left, top + 25, width, height);
            }
            this.LookAndFeel.UseDefaultLookAndFeel = false;

            //if (!G1.is_supervisor())
            //    menuAdmin.Visibility = BarItemVisibility.Never;

            string answer = "";


            this.dgv3.Hide();
            this.rtb.Show();
            this.rtb.Dock = DockStyle.Fill;

            btnError.Visible = false;

            string cmd = "Select * from `users` u LEFT JOIN `tc_er` t ON u.`username` = t.`username` WHERE u.`username` = '" + workUserName + "'";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            workMyTimeKeeper = dx.Rows[0]["TimeKeeper"].ObjToString();
            workHireDate = dx.Rows[0]["hireDate"].ObjToDateTime();
            workVacationOverride = dx.Rows[0]["vacationOverride"].ObjToDouble();

            is_supervisor = false;
            is_timekeeper = false;
            workEmpSalaried = dx.Rows[0]["salaried"].ObjToString();
            workEmpStatus = dx.Rows[0]["EmpStatus"].ObjToString();
            if (String.IsNullOrWhiteSpace(workEmpStatus))
                workEmpStatus = "FullTime";
            workEmpType = dx.Rows[0]["EmpType"].ObjToString();
            workMyName = dx.Rows[0]["lastName"].ObjToString() + ", " + dx.Rows[0]["firstName"].ObjToString();
            workLocation = dx.Rows[0]["Location"].ObjToString();

            if ( workDataRow != null )
            {
                workEmpStatus = workDataRow ["EmpStatus"].ObjToString();
                if (String.IsNullOrWhiteSpace(workEmpStatus))
                    workEmpStatus = "FullTime";
                workEmpType = workDataRow["EmpType"].ObjToString();
            }
            else if ( !String.IsNullOrWhiteSpace ( workOldRecord ))
            {
                cmd = "Select * from `tc_pay` WHERE `record` = '" + workOldRecord + "';";
                DataTable ddd = G1.get_db_data(cmd);
                if ( ddd.Rows.Count > 0 )
                {
                    workEmpStatus = ddd.Rows[0]["EmpStatus"].ObjToString();
                    if (String.IsNullOrWhiteSpace(workEmpStatus))
                        workEmpStatus = "FullTime";
                    workEmpType = ddd.Rows[0]["EmpType"].ObjToString();
                }
            }

            workRate = dx.Rows[0]["rate"].ObjToDecimal();

            if (dx.Rows[0]["isSupervisor"].ObjToString().ToUpper() == "Y")
                is_supervisor = true;
            if (dx.Rows[0]["isTimeKeeper"].ObjToString().ToUpper() == "Y")
                is_timekeeper = true;

            if (!is_supervisor && !is_timekeeper)
            {
                menuFormats.Visibility = BarItemVisibility.Never;
            }

            allowTimeOffRequests = false;
            string preference = G1.getPreference(LoginForm.username, "TimeOff Requests", "Allow Access");
            if (preference == "YES")
                allowTimeOffRequests = true;

            if (G1.RobbyServer)
                allowTimeOffRequests = true; // Back Door for ME!



            btnAddNextPunch.Hide();
            btnAddPunch.Hide();
            btnAddPunch.Refresh();

            tabControl1.TabPages.Remove(tabMain);
            tabControl1.TabPages.Remove(tabMyTimeOff);
            tabControl1.TabPages.Remove(tabPTO);
            tabControl1.TabPages.Remove(tabReport);
            tabControl1.TabPages.Remove(tabDetail);
            tabControl1.TabPages.Remove(tabTimeOffProc);
            tabControl1.TabPages.Remove(tabContractLabor);
            tabControl1.TabPages.Remove(tabPageOther);
            if ( G1.isHR() || G1.isAdmin() )
            {
                AddTabPage(tabMain);
                AddTabPage(tabContractLabor);
            }
            if (workEmpStatus.ToUpper().IndexOf("PARTTIME") >= 0)
            {
                if (workEmpStatus.ToUpper().IndexOf("&") > 0)
                {
                    AddTabPage(tabMain);
                    //tabControl1.TabPages.Add(tabMain);
                    btnAddPunch.Show();
                    btnAddPunch.Refresh();
                }
                else if (workEmpSalaried == "Y" && workEmpType.ToUpper() == "EXEMPT")
                {
                    AddTabPage(tabMain);
                    //tabControl1.TabPages.Add(tabMain);
                    if (G1.isAdmin() || G1.isHR())
                    {
                        AddTabPage(tabContractLabor);
                        //tabControl1.TabPages.Add(tabContractLabor);
                    }
                    btnAddPunch.Show();
                    btnAddPunch.Refresh();
                }
                else
                {
                    AddTabPage(tabContractLabor);
                    //tabControl1.TabPages.Add(tabContractLabor);
                }
                //tabControl1.TabPages.Add(tabMyTimeOff);
                //if ( is_timekeeper )
                //    tabControl1.TabPages.Add(tabTimeOffProc);
            }
            else
            {
                btnAddPunch.Show();
                btnAddPunch.Refresh();
                AddTabPage(tabMain);
                //tabControl1.TabPages.Add(tabMain);
                btnAddPunch.Show();
                btnAddPunch.Refresh();
                //tabControl1.TabPages.Add(tabMyTimeOff);
                //if (is_timekeeper)
                //    tabControl1.TabPages.Add(tabTimeOffProc);
            }

            tabControl1.TabPages.Add(tabPageOther);
            if (workEmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
            {
                if (allowTimeOffRequests)
                {
                    tabControl1.TabPages.Add(tabMyTimeOff);
                    //tabControl1.TabPages.Add(tabTimeOffProc);
                }
            }
            else if ( allowTimeOffRequests )
                tabControl1.TabPages.Add(tabMyTimeOff);

            //if (workEmpType.ToUpper() == "NON-EXEMPT")
            //    bandSalary.Visible = false;

            if (!is_timekeeper)
            {
                gridMain.Columns["approve"].Visible = false;
                gridNotes.Visible = false;
                menuAddHolidays.Visibility = BarItemVisibility.Never;
                menuEditPreferences.Visibility = BarItemVisibility.Never;
                menuEditHourStatus.Visibility = BarItemVisibility.Never;
                //                menuOptions.Visibility = BarItemVisibility.Never;
            }
            else
                LoadTimeOffRequests();

            if (!G1.isHR())
            {
                gridMain7.Columns["paymentAmount"].Visible = false;
                gridMain7.Columns["rate"].Visible = false;
                gridMain8.Columns["paymentAmount"].Visible = false;
                gridMain8.Columns["rate"].Visible = false;
            }

            //answer = G1.GetPreference("TimeClock", "Allow Add Employee");
            answer = "NO";
            if (answer.Trim().ToUpper() != "YES")
            {
                menuAddNewEmployee.Visibility = BarItemVisibility.Never;
                menuExportPTO.Visibility = BarItemVisibility.Never;
                editEmployeeToolStripMenuItem.Visible = false;
            }

            if (is_supervisor && allEmployees)
                tabControl1.TabPages.Remove(tabMyTimeOff);
            if ( !allowTimeOffRequests )
                tabControl1.TabPages.Remove(tabMyTimeOff); // For Now

            //else if (is_supervisor && !allEmployees)
            //    tabControl1.TabPages.Remove(tabTimeOffProc);

            //tabControl1.TabPages.Add(tabTimeOffProc);

            LoadTimePeriod();
            SetupPunchButtons();

            ResetWindow(workGroup, empno);

            GetEmployeePunches(empno);

            if (!btnPunchIn2.Visible)
                btnAddPunch_Click(null, null);
            if (!btnPunchIn3.Visible)
                btnAddPunch_Click(null, null);
            if (!btnPunchIn4.Visible)
                btnAddPunch_Click(null, null);

            if (workPrintOnly)
            {
                gridMain.Bands["gridNotes"].Visible = false;
                //gridMain.Columns["notes"].Visible = false;
                string timeFile = @"C:/rag/pdfTime.pdf";
                if (File.Exists(timeFile))
                {
                    File.SetAttributes(timeFile, FileAttributes.Normal);
                    File.Delete(timeFile);
                }

                DataTable dd = (DataTable)dgv.DataSource;

                PerformGrouping();

                gridMain.ExpandAllGroups();

                //if (gotTime)
                //{
                if (workEmpStatus.ToUpper().IndexOf("FULLTIME") >= 0)
                {
                    printingWhat = "TIMESHEET";
                    SetupPrintPage(dgv);
                    printableComponentLink1.ExportToPdf(timeFile);
                }
                //}
                string contractFile = @"C:/rag/pdfContract.pdf";
                if (File.Exists(contractFile))
                {
                    File.SetAttributes(contractFile, FileAttributes.Normal);
                    File.Delete(contractFile);
                }
                //if (gotContract)
                //{
                if (workEmpStatus.ToUpper().IndexOf("PARTTIME") >= 0)
                {
                    printingWhat = "CONTRACT";
                    dd = (DataTable)dgv7.DataSource;
                    SetupPrintPage(dgv7);
                    printableComponentLink1.ExportToPdf(contractFile);
                }
                string otherFile = @"C:/rag/pdfOther.pdf";
                if (File.Exists(otherFile))
                {
                    File.SetAttributes(otherFile, FileAttributes.Normal);
                    File.Delete(otherFile);
                }
                dd = (DataTable)dgv8.DataSource;
                if (dd.Rows.Count > 0)
                {
                    printingWhat = "OTHER";
                    SetupPrintPage(dgv8);
                    printableComponentLink1.ExportToPdf(otherFile);
                }
                //}
                this.Close();
                return;
            }

            LoadPayPeriods();

            if (workGroup)
                dgv.ContextMenuStrip = this.contextMenuStrip1;
            else
                dgv.ContextMenuStrip = this.contextMenuStrip3;


            //if (workGroup && is_supervisor)
            //    CheckForRequests();

            //lblCycleNote.Show(); // For Now
            //txtCycleNote.Show();


            btnClock.Hide();
            btnDecimal.Hide();

            PerformGrouping();

            if (!is_timekeeper && !G1.isHR() && !G1.isAdmin() && !Employees.isManager())
                chkManagerApproved.Enabled = false;
            if ( !Employees.isManager())
                chkManagerApproved.Enabled = false;

            if ( G1.isHR() )
            {
                chkManagerApproved.Enabled = true;
                chkEmployeeApproved.Enabled = true;
            }

            loading = false;

            gridMain.ExpandAllGroups();

            dgv7.RefreshDataSource();
            dgv7.Refresh();
            gridMain7.RefreshData();
            gridMain7.RefreshEditor(true);

            CleanupColumns();

            //LoadMyTimeOffRequests();

            gridMain.OptionsNavigation.EnterMoveNextColumn = true;

            DataTable junkDt = (DataTable)dgv.DataSource;

            OnLoadDone();
        }
        /****************************************************************************************/
        private void AddTabPage ( TabPage page )
        {
            bool found = false;
            for ( int i=0; i<tabControl1.TabPages.Count; i++)
            {
                if ( tabControl1.TabPages[i].Name  == page.Name )
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                tabControl1.TabPages.Add(page);
        }
        /****************************************************************************************/
        private void CleanupColumns()
        {
            if (!G1.isHR())
            {
                gridMain.DestroyCustomization();
                G1.HideGridChooser(gridMain);
                gridMain7.DestroyCustomization();
                G1.HideGridChooser(gridMain7);
                gridMain8.DestroyCustomization();
                G1.HideGridChooser(gridMain8);
            }
        }
        /****************************************************************************************/
        private void PerformGrouping()
        {
            DataTable ddd = (DataTable)dgv.DataSource;

            DataView tempview = ddd.DefaultView;
            tempview.Sort = "week asc";
            ddd = tempview.ToTable();
            dgv.DataSource = ddd;

            gridMain.Columns["week"].GroupIndex = 0;
            gridMain.ExpandAllGroups();
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            //AddSummaryColumn("total", null, "{0:0,0.00}");
            //AddSummaryColumn("week1", null, "{0:0,0.00}");
            //AddSummaryColumn("week2", null, "{0:0,0.00}");
            //AddSummaryColumn("overtime", null, "{0:0,0.00}");
            //AddSummaryColumn("hours", null, "{0:0,0.00}");

            AddSummaryColumn("total", "{0:0,0.00}");
            AddSummaryColumn("vacation", "{0:0,0.00}");
            AddSummaryColumn("holiday", "{0:0,0.00}");
            AddSummaryColumn("sick", "{0:0,0.00}");
            if (G1.isHR())
            {
                AddSummaryColumn("other", "{0:0,0.00}");
                GridGroupSummaryItem item = new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "other", this.bandedGridColumn66, "{0:0,0.00}");
                this.gridMain.GroupSummary.Add(item);
            }
            else
            {
                gridMain.Columns["other"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.None;
            }
            AddSummaryColumn("week1", "{0:0,0.00}");
            AddSummaryColumn("week2", "{0:0,0.00}");
            AddSummaryColumn("overtime", "{0:0,0.00}");
            AddSummaryColumn("hours", "{0:0,0.00}");

            AddSummaryColumn("hours", gridMain7, "{0:0,0.00}");
            AddSummaryColumn("paymentAmount", gridMain7, "{0:0,0.00}");

            gridMain7.Columns["week"].GroupIndex = 0;
            gridMain7.ExpandAllGroups();

            AddSummaryColumn("paymentAmount", gridMain8, "{0:0,0.00}");

            gridMain8.Columns["week"].GroupIndex = 0;
            gridMain8.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void CheckForRequests()
        {
            string super = LoginForm.username;
            string cmd = "Select * from `tc_timerequest` where `supervisor` = '" + super + "' ";
            cmd += " and `approved` <> 'Y' ";
            cmd += " order by `fromdate` DESC; ";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int i = 0; i < tabControl1.Controls.Count; i++)
                {
                    Control con = tabControl1.Controls[i];
                    if (con.Text.ToUpper() == "TIMEOFFPROC")
                    {
                        if (con.Text.IndexOf("*") < 0)
                            con.Text += "*";
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < tabControl1.Controls.Count; i++)
                {
                    Control con = tabControl1.Controls[i];
                    if (con.Text.ToUpper() == "TIMEOFFPROC*")
                    {
                        con.Text = "TimeOffProc";
                        break;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void LoadTimeOffRequests()
        {
            //if (1 == 1)
            //    return; // For Now
            if (!is_supervisor)
                return;
            //if (!TimeClockSupervisor)
            //    return;
            //if (!allEmployees)
            //    return;

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit5;
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";

            string record = "";
            string empno = "";
            string supervisor = "";
            string approved = "";
            string approved_by = "";
            string name = "";
            double hours = 0D;
            string comment = "";
            double pto_now = 0D;
            double december = 0D;
            name = LoginForm.username;
            string fullName = name;


            //empno = cUser.UserID.ObjToString();
            empno = LoginForm.workUserRecord.ObjToString();


            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("date");
            dt.Columns.Add("approved");
            dt.Columns.Add("approvedby");
            dt.Columns.Add("fromdate");
            dt.Columns.Add("todate");
            dt.Columns.Add("hours");
            dt.Columns.Add("record");
            dt.Columns.Add("empno");
            dt.Columns.Add("name");
            dt.Columns.Add("comment");
            dt.Columns.Add("pto_taken", Type.GetType("System.Double"));
            dt.Columns.Add("pto_now", Type.GetType("System.Double"));
            dt.Columns.Add("pto_inc", Type.GetType("System.Double"));
            dt.Columns.Add("december", Type.GetType("System.Double"));

            string cmd = "Select * from `tc_timerequest` where `supervisor` = '" + workMyName + "' OR `supervisor` LIKE '%" + fullName + "%' ";
            if (cmbMyProc.Text.ToUpper() == "APPROVED")
                cmd += " and `approved` = 'Y' ";
            else if (cmbMyProc.Text.ToUpper() == "UNAPPROVED")
                cmd += " and `approved` <> 'Y' ";
            cmd += " order by `fromdate` DESC; ";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                empno = dx.Rows[i]["empno"].ObjToString();
                name = dx.Rows[i]["name"].ObjToString();
                pto_now = dx.Rows[i]["pto_now"].ObjToDouble();
                december = dx.Rows[i]["december"].ObjToDouble();

                supervisor = dx.Rows[i]["supervisor"].ObjToString();
                approved_by = dx.Rows[i]["approved_by"].ObjToString();
                approved = dx.Rows[i]["approved"].ObjToString();
                DateTime fromDate = dx.Rows[i]["fromdate"].ObjToDateTime();
                DateTime toDate = dx.Rows[i]["todate"].ObjToDateTime();
                DateTime Date = dx.Rows[i]["date_requested"].ObjToDateTime();
                hours = dx.Rows[i]["requested_hours"].ObjToDouble();
                comment = dx.Rows[i]["OtherInformation"].ObjToString();

                DataRow dRow = dt.NewRow();
                dRow["record"] = record;
                dRow["empno"] = empno;
                dRow["name"] = name;
                dRow["approvedby"] = approved_by;
                dRow["approved"] = approved;
                dRow["fromdate"] = fromDate.Month.ToString("D2") + "/" + fromDate.Day.ToString("D2") + "/" + fromDate.Year.ToString("D4");
                dRow["todate"] = toDate.Month.ToString("D2") + "/" + toDate.Day.ToString("D2") + "/" + toDate.Year.ToString("D4");
                dRow["date"] = Date.Month.ToString("D2") + "/" + Date.Day.ToString("D2") + "/" + Date.Year.ToString("D4");
                dRow["hours"] = hours;
                dRow["comment"] = comment;
                dRow["pto_now"] = pto_now;
                dRow["december"] = december;
                dt.Rows.Add(dRow);
            }
            dgv6.DataSource = dt;
        }
        /***********************************************************************************************/
        private void LoadMyTimeOffRequests()
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit4;
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";

            if ( !G1.isHR() && !G1.isSpecial() )
            {
                txtAvailablePTO.Hide();
                txtDecember.Hide();
                //txtPTOtaken.Hide();
                //label7.Hide();
                label8.Hide();

                //gridView5.Columns["pto_taken"].Visible = false;
                gridView5.Columns["pto_now"].Visible = false;
                gridView5.Columns["december"].Visible = false;
            }

            string work_employee = empno;

            string record = "";
            string myempno = "";
            string supervisor = "";
            string approved = "";
            string approved_by = "";
            string name = "";
            double hours = 0D;
            string comment = "";
            double pto_now = 0D;
            double december = 0D;
            //name = cUser.FName.ObjToString();
            //empno = cUser.UserID.ObjToString();
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("date");
            dt.Columns.Add("approved");
            dt.Columns.Add("approvedby");
            dt.Columns.Add("fromdate");
            dt.Columns.Add("todate");
            dt.Columns.Add("hours");
            dt.Columns.Add("record");
            dt.Columns.Add("empno");
            dt.Columns.Add("name");
            dt.Columns.Add("comment");
            dt.Columns.Add("pto_taken", Type.GetType("System.Double"));
            dt.Columns.Add("pto_now", Type.GetType("System.Double"));
            dt.Columns.Add("pto_inc", Type.GetType("System.Double"));
            dt.Columns.Add("december", Type.GetType("System.Double"));
            dt.Columns.Add("mod");

            double yearlyVacation = 0D;
            double yearlySick = 0D;
            DateTime endDate = DateTime.Now;

            Employees.SetupBenefits(workHireDate, endDate, ref yearlyVacation, ref yearlySick);
            if (workVacationOverride > 0D)
                yearlyVacation = workVacationOverride;
            txtDecember.Text = G1.ReformatMoney ( yearlyVacation );
            txtHireDate.Text = workHireDate.ToString("MM/dd/yyyy");

            if (!allEmployees)
            {
            }
            else
            {
            }

            int year = this.dateTimePicker1.Value.Year;

            DateTime January = new DateTime(year, 1, 1, 0, 0, 0);
            DateTime December = new DateTime(year, 12, 31, 0, 0, 0);

            string cmd = "SELECT * FROM tc_punches_pchs WHERE `date` >= '" + January.ToString("yyyy-MM-dd") + "' AND `date` <= '" + December.ToString("yyyy-MM-dd") + "' AND `empy!AccountingID` = '" + workEmpNo + "' AND vacation > '0';";
            DataTable vDt = G1.get_db_data(cmd);

            cmd = "Select * from `tc_timerequest` where `empno` = '" + workUserName + "' order by `fromdate`;";
            DataTable dx = G1.get_db_data(cmd);

            double pto_taken = 0D;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                myempno = dx.Rows[i]["empno"].ObjToString();
                name = dx.Rows[i]["name"].ObjToString();
                pto_now = dx.Rows[i]["pto_now"].ObjToDouble();
                december = dx.Rows[i]["december"].ObjToDouble();

                supervisor = dx.Rows[i]["supervisor"].ObjToString();
                approved_by = dx.Rows[i]["approved_by"].ObjToString();
                approved = dx.Rows[i]["approved"].ObjToString();
                DateTime fromDate = dx.Rows[i]["fromdate"].ObjToDateTime();
                DateTime toDate = dx.Rows[i]["todate"].ObjToDateTime();
                DateTime Date = dx.Rows[i]["date_requested"].ObjToDateTime();
                hours = dx.Rows[i]["requested_hours"].ObjToDouble();
                comment = dx.Rows[i]["comment"].ObjToString();

                Employees.SetupBenefits(workHireDate, toDate, ref yearlyVacation, ref yearlySick);
                if (workVacationOverride > 0D)
                    yearlyVacation = workVacationOverride;
                december = yearlyVacation;

                pto_now = CalcNowPTO(workHireDate, vDt, december, fromDate, toDate, ref pto_taken );

                DataRow dRow = dt.NewRow();
                dRow["record"] = record;
                dRow["empno"] = myempno;
                dRow["name"] = name;
                dRow["approved"] = approved;
                dRow["approvedby"] = approved_by;
                dRow["fromdate"] = fromDate.Month.ToString("D2") + "/" + fromDate.Day.ToString("D2") + "/" + fromDate.Year.ToString("D4");
                dRow["todate"] = toDate.Month.ToString("D2") + "/" + toDate.Day.ToString("D2") + "/" + toDate.Year.ToString("D4");
                dRow["date"] = Date.Month.ToString("D2") + "/" + Date.Day.ToString("D2") + "/" + Date.Year.ToString("D4");
                dRow["hours"] = hours;
                dRow["comment"] = comment;
                dRow["pto_now"] = pto_now;
                dRow["december"] = december;
                dRow["pto_taken"] = pto_taken;
                dt.Rows.Add(dRow);
            }

            //RecalcPTO(dt);

            G1.NumberDataTable(dt);

            pto_taken = 0D;
            for (int i = 0; i < vDt.Rows.Count; i++)
                pto_taken += vDt.Rows[i]["vacation"].ObjToDouble();

            string taken = G1.ReformatMoney(pto_taken);
            txtPTOtaken.Text = taken;

            dgv5.DataSource = dt;
            btnSaveVacation.Hide();
            btnSaveVacation.Refresh();

            LoadVacationHours(dt);
        }
        /***********************************************************************************************/
        private void RecalcPTO ( DataTable dx )
        {
            double yearlyVacation = 0D;
            double yearlySick = 0D;
            DateTime endDate = DateTime.Now;
            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;
            double hours = 0D;
            double december = 0D;
            double pto_now = 0D;


            int year = this.dateTimePicker1.Value.Year;

            DateTime January = new DateTime(year, 1, 1, 0, 0, 0);
            DateTime December = new DateTime(year, 12, 31, 0, 0, 0);

            string cmd = "SELECT * FROM tc_punches_pchs WHERE `date` >= '" + January.ToString("yyyy-MM-dd") + "' AND `date` <= '" + December.ToString("yyyy-MM-dd") + "' AND `empy!AccountingID` = '" + workEmpNo + "' AND vacation > '0';";
            DataTable vDt = G1.get_db_data(cmd);

            Employees.SetupBenefits(workHireDate, endDate, ref yearlyVacation, ref yearlySick);
            if (workVacationOverride > 0D)
                yearlyVacation = workVacationOverride;
            double pto_taken = 0D;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                try
                {
                    fromDate = dx.Rows[i]["fromdate"].ObjToDateTime();
                    toDate = dx.Rows[i]["todate"].ObjToDateTime();
                    //DateTime Date = dx.Rows[i]["date_requested"].ObjToDateTime();
                    hours = dx.Rows[i]["hours"].ObjToDouble();

                    Employees.SetupBenefits(workHireDate, toDate, ref yearlyVacation, ref yearlySick);
                    if (workVacationOverride > 0D)
                        yearlyVacation = workVacationOverride;
                    december = yearlyVacation;

                    pto_now = CalcNowPTO(workHireDate, vDt, december, fromDate, toDate, ref pto_taken);

                    dx.Rows[i]["pto_now"] = pto_now;
                    dx.Rows[i]["december"] = december;
                    dx.Rows[i]["pto_taken"] = pto_taken;
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        public static double CalcPTOupto ( string workEmpNo, DateTime hireDate, DateTime fromDate, DateTime toDate, ref double pto_now )
        {
            double pto_taken = 0D;
            int year = toDate.Year;
            if (fromDate.Year < year)
                year = fromDate.Year;

            DateTime January = new DateTime(year, 1, 1, 0, 0, 0);
            DateTime December = new DateTime(year, 12, 31, 0, 0, 0);

            string cmd = "SELECT * FROM tc_punches_pchs WHERE `date` >= '" + January.ToString("yyyy-MM-dd") + "' AND `date` <= '" + December.ToString("yyyy-MM-dd") + "' AND `empy!AccountingID` = '" + workEmpNo + "' AND vacation > '0';";
            DataTable vDt = G1.get_db_data(cmd);

            double yearlyVacation = 0D;
            double yearlySick = 0D;

            Employees.SetupBenefits(hireDate, fromDate, ref yearlyVacation, ref yearlySick);

            pto_now = CalcNowPTO(hireDate, vDt, yearlyVacation, fromDate, toDate, ref pto_taken);
            return pto_taken;
        }
        /***********************************************************************************************/
        public static double CalcNowPTO ( DateTime hireDate, DataTable vDt, double yearlyVacation, DateTime fromDate, DateTime toDate, ref double pto_taken )
        {
            double ptoNow = yearlyVacation;
            DateTime date = DateTime.Now;
            double vacation = 0D;
            pto_taken = 0D;
            for ( int i=0; i<vDt.Rows.Count; i++)
            {
                date = vDt.Rows[i]["date"].ObjToDateTime();
                vacation = vDt.Rows[i]["vacation"].ObjToDouble();
                if (date < fromDate)
                {
                    ptoNow -= vacation;
                    pto_taken += vacation;
                }
                else if (date >= fromDate && date <= toDate)
                {
                    ptoNow -= vacation;
                    pto_taken += vacation;
                }
            }
            return ptoNow;
        }
        /***********************************************************************************************/
        private void LoadVacationHours ( DataTable dt )
        {
            DataTable timDt = (DataTable)dgv.DataSource;

            string approved = "";
            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;
            DateTime testDate = DateTime.Now;
            DateTime myDate = DateTime.Now;

            double days = 0D;
            double hours = 0D;
            TimeSpan ts;
            bool gotVacation = false;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                approved = dt.Rows[i]["approved"].ObjToString().ToUpper();
                if ( approved == "Y" )
                {
                    fromDate = dt.Rows[i]["fromDate"].ObjToDateTime();
                    fromDate = fromDate.AddDays(-1);
                    toDate = dt.Rows[i]["toDate"].ObjToDateTime();
                    toDate = toDate.AddDays(-1);
                    hours = dt.Rows[i]["hours"].ObjToDouble();

                    ts = toDate - fromDate;
                    days = ts.TotalDays + 1;

                    hours = hours / days;

                    for ( int j=0; j<days; j++)
                    {
                        testDate = fromDate.AddDays(j);
                        for ( int k=0; k<timDt.Rows.Count; k++)
                        {
                            myDate = timDt.Rows[k]["date"].ObjToDateTime();
                            if ( myDate == testDate )
                            {
                                timDt.Rows[k]["vacation"] = hours;
                                gotVacation = true;
                            }
                        }
                    }
                }
            }
            if (gotVacation)
                dgv.DataSource = timDt;
        }
        /***********************************************************************************************/
        private void SetupHelpMenus()
        {
            if (is_supervisor)
                return; // Allow two (2) Help menu options for supervisors so they can see both sets of documented help
        }
        /***********************************************************************************************/
        private void ResetWindow(bool workGroup, string empno)
        {
            if (workGroup)
            {
                //if (!workPrintOnly)
                //    StrechLastColumn("cyclenotes");
                int top = panelTop.Top;
                int left = panelTop.Left;
                int width = panelTop.Width;
                int height = panelTop.Height - 28;
                //panelTop.SetBounds(left, top, width, height);
                gridMain_Click(null, null);
            }
            else
            {
                //if (!workPrintOnly)
                //    StrechLastColumn("notes");

                bool super = false;
                //string answer = G1.GetPreference("TimeClock", "Supervisor", empno);
                string answer = "NO";
                if (answer == "YES")
                    super = true;
                if (is_supervisor)
                    super = true;

                if (!bandSalary.Visible && !super)
                {
                    int top = panelTop.Top;
                    int left = panelTop.Left;
                    int width = panelTop.Width;
                    int height = panelTop.Height - 28;
                    //panelTop.SetBounds(left, top, width, height);
                }
            }
        }
        /***********************************************************************************************/
        private void StrechLastColumn(string column = "")
        {
            //if (1 == 1)
            //    return;
            if (string.IsNullOrWhiteSpace(column))
                column = "notes";
            try
            {
                GridColumn col = (GridColumn)gridMain.Columns[column];
                col.MinWidth = 1000;
            }
            catch (Exception ex)
            {
            }
            //            gridMain.OptionsView.ColumnAutoWidth = true;
        }
        /***********************************************************************************************/
        private void CheckPreferences()
        {
            string answer = G1.getPreference(LoginForm.username, "TimeClock", "Supervisor");
            if (answer == "YES")
                TimeClockSupervisor = true;
            else
                menuEditHourStatus.Dispose();

            answer = G1.getPreference(LoginForm.username, "Preference Menu", "Allow Access");
            if (answer != "YES")
                menuEditPreferences.Dispose();

            answer = G1.getPreference(LoginForm.username, "TimeClock", "Allow Edit Help");
            if (answer != "YES")
                menuEditHelp.Visibility = BarItemVisibility.Never;
        }
        /***********************************************************************************************/
        private void SetupAllEmployees()
        {
            if (!workGroup)
            {
                dateTimePicker2.Visible = false;
                lblTo.Visible = false;
                int top = DateControl_Forward.Top;
                int left = lblTo.Left;
                int width = DateControl_Forward.Width;
                int height = DateControl_Forward.Height;
                DateControl_Forward.SetBounds(left, top, width, height);
                dateTimePicker1.Value = DateTime.Now;
            }
        }
        /***********************************************************************************************/
        private void LocateStartingPayPeriod()
        {
            lastposting = GetLastPostingDate();
            DateTime date = lastposting.AddDays(27); // Move a month ahead to start
            DateTime workTime = workDate.ObjToDateTime();
            try
            {
                for (; ; )
                {
                    if (workTime >= date)
                    {
                        dateTimePicker1.Value = date;
                        //dateTimePicker2.Value = date.AddDays(14).AddMinutes(-1);
                        dateTimePicker2.Value = date.AddDays(14);
                        break;
                    }
                    date = date.AddDays(-14); // Go back in time
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void LoadPayPeriods()
        {
            DateTime date1 = dateTimePicker1.Value;
            DateTime date2 = dateTimePicker2.Value;



            DateTime startDate = DateTime.Now;
            DateTime stopDate = DateTime.Now;
            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            DateTime newDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime testDate = DateTime.Now;
            DateTime beginDate = new DateTime(2022, 12, 23);
            try
            {
                for (; ; )
                {
                    newDate = beginDate;
                    testDate = newDate.AddDays(13);
                    if (now >= newDate && now <= newDate.AddDays(13))
                    {
                        startDate = newDate;
                        break;
                    }
                    beginDate = beginDate.AddDays(14);
                }
            }
            catch (Exception ex)
            {
                startDate = DateTime.Now;
            }


            //DateTime startDate = DateTime.Now;
            //DateTime stopDate = DateTime.Now;
            //DateTime now = DateTime.Now;
            //DateTime newDate = DateTime.Now;
            //DateTime beginDate = new DateTime(2022, 12, 23);
            //for (; ; )
            //{
            //    newDate = beginDate;
            //    if (now >= newDate && now <= newDate.AddDays(13))
            //    {
            //        startDate = newDate;
            //        break;
            //    }
            //    beginDate = beginDate.AddDays(14);
            //}


            // DateTime last = GetLastPostingDate();
            DateTime last = beginDate;
            DateTime date = last.AddDays(14); // Move (2) weeks ahead to start
            DateTime workTime = DateTime.Now;
            for (; ; )
            {
                if (workTime > date)
                {
                    date1 = date;
                    date2 = date.AddDays(14);
                    break;
                }
                date = date.AddDays(-14); // Go back in time
            }

            for (int i = 26; i >= 1; i--)
            {
                BarButtonItem menu = new BarButtonItem();
                menu.Caption = date2.Month.ToString("D2") + "/" + date2.Day.ToString("D2") + "/" + date2.Year.ToString("D4");
                menu.ItemClick += Menu_ItemClick;
                menuPayPeriods.AddItem(menu);
                date1 = date1.AddDays(-14);
                date2 = date2.AddDays(-14);
            }
        }
        /***********************************************************************************************/
        private void Menu_ItemClick(object sender, ItemClickEventArgs e)
        {
            string date = e.Item.Caption.Trim();
            DateTime date2 = date.ObjToDateTime();
            DateTime date1 = date2.AddDays(-14);
            this.dateTimePicker1.Value = date1;
            this.dateTimePicker2.Value = date2;
            LoadTimePeriod();
            GetEmployeePunches(empno);

            if (!btnPunchIn2.Visible)
                btnAddPunch_Click(null, null);
            if (!btnPunchIn3.Visible)
                btnAddPunch_Click(null, null);
            if (!btnPunchIn4.Visible)
                btnAddPunch_Click(null, null);

            PerformGrouping();
        }
        /***********************************************************************************************/
        private void LoadSupervisors()
        {
            string cmd = "Select * from `tc_er` WHERE `isSupervisor` = 'Y' order by `username`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot locate employees!");
                return;
            }

            string oldSuper = "";
            string oldCode = "";
            string super = "";
            string jobcode = "";

            BarButtonItem all = new BarButtonItem();
            all.Name = "ALL";
            all.Caption = "ALL Employees";
            all.Tag = "";
            all.ItemClick += Super_ItemClick;
            menuSupervisors.AddItem(all);


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                super = dt.Rows[i]["username"].ObjToString();
                BarButtonItem menu = new BarButtonItem();
                menu.Name = super;
                menu.Caption = LookupSupervisor(super);
                menu.Tag = oldCode;
                menu.ItemClick += Super_ItemClick;
                menuSupervisors.AddItem(menu);
                superList += super + " " + "\n";
            }
            if (!TimeClockSupervisor || !workGroup)
                menuSupervisors.Visibility = BarItemVisibility.Never;
        }
        /***********************************************************************************************/
        private string LookupSupervisor(string empno)
        {
            string name = "";
            string cmd = "Select * from `er` where `empno` = '" + empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                name = dt.Rows[0]["name"].ObjToString();
            return name;
        }
        /***********************************************************************************************/
        DataTable SavedReportDt = null;
        int SavedReportRow = -1;
        DataTable SavedSuperDt = null;
        int SavedSuperRow = -1;
        /***********************************************************************************************/
        private void ReLoadAll()
        {
            string newEmpNo = "";
            string empno = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                newEmpNo = dt.Rows[i]["empno"].ObjToString();
                for (int j = 0; j < SavedSuperDt.Rows.Count; j++)
                {
                    empno = SavedSuperDt.Rows[j]["empno"].ObjToString();
                    if (empno == newEmpNo)
                    {
                        G1.copy_dt_row(dt, i, SavedSuperDt, j);
                        break;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void Super_ItemClick(object sender, ItemClickEventArgs e)
        {
            string super = e.Item.Caption.Trim();
            string jobcodes = e.Item.Tag.ObjToString().Trim();

            if (dgv4.Visible)
            {
                SuperReport(super, jobcodes);
                LoadMainPicture(dgv4);
                return;
            }

            if (super.Trim().ToUpper() == "ALL EMPLOYEES" && SavedSuperDt != null)
            {
                ReLoadAll();
                dgv.DataSource = SavedSuperDt;
                gridMain.FocusedRowHandle = SavedSuperRow;
                if (workingUnapproved)
                    chkUnapproved_CheckedChanged(null, null);
                LoadMainPicture(dgv);
                return;
            }

            if (SavedSuperDt == null)
            {
                SavedSuperDt = (DataTable)dgv.DataSource;
                SavedSuperRow = gridMain.FocusedRowHandle;
            }
            string cmd = jobcodes.TrimEnd(',');

            DataRow[] dRows = SavedSuperDt.Select("`jobcode` IN (" + cmd + ")");
            //            DataRow[] dRows = SavedSuperDt.Select("`supervisor` = '" + super + "'");

            DataTable dt = SavedSuperDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            dt.AcceptChanges();

            LoadOtherEmployees(dt, super);

            VerifySupervisor(dt, super);

            dgv.DataSource = dt;
            if (workingUnapproved)
                chkUnapproved_CheckedChanged(null, null);
            LoadMainPicture(dgv);
        }
        /***********************************************************************************************/
        private void VerifySupervisor(DataTable dt, string supervisor)
        {
            string emp = "";
            string super = "";
            string cmd = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                emp = dt.Rows[i]["empno"].ObjToString();
                cmd = "Select * from `er` where `empno` = '" + emp + "';";
                DataTable empdt = G1.get_db_data(cmd);
                if (empdt.Rows.Count > 0)
                {
                    super = empdt.Rows[0]["preferred_supervisor"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(super))
                    {
                        if (super.ToUpper() != supervisor.ToUpper())
                            dt.Rows.RemoveAt(i);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void LoadOtherEmployees(DataTable dt, string super)
        {
            bool modified = false;
            string cmd = "Select * from `er` where `preferred_supervisor` = '" + super + "';";
            DataTable supertDt = G1.get_db_data(cmd);
            for (int i = 0; i < supertDt.Rows.Count; i++)
            {
                string emp = supertDt.Rows[i]["empno"].ObjToString();
                bool found = false;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    string empno = dt.Rows[j]["empno"].ObjToString();
                    if (empno == emp)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DataRow[] dRows = SavedSuperDt.Select("`empno` = '" + emp + "'");
                    for (int j = 0; j < dRows.Length; j++)
                        dt.ImportRow(dRows[j]);
                    dt.AcceptChanges();
                    modified = true;
                }
            }
            if (modified)
                G1.sortTable(dt, "empno", "ASC");
        }
        /***********************************************************************************************/
        private void LoadMainPicture(DevExpress.XtraGrid.GridControl dgv)
        {
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            ShowPictureRow(dt, 0);
        }
        /***********************************************************************************************/
        private void SuperReport(string super, string jobcodes)
        {
            if (super.Trim().ToUpper() == "ALL EMPLOYEES" && SavedReportDt != null)
            {
                dgv4.DataSource = SavedReportDt;
                gridMain4.FocusedRowHandle = SavedReportRow;
                return;
            }

            if (SavedReportDt == null)
            {
                SavedReportDt = (DataTable)dgv4.DataSource;
                SavedReportRow = gridMain4.FocusedRowHandle;
            }
            string cmd = jobcodes.TrimEnd(',');

            DataRow[] dRows = SavedReportDt.Select("`jobcode` IN (" + cmd + ")");

            DataTable dt = SavedReportDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            dt.AcceptChanges();
            dgv4.DataSource = dt;
        }
        /***********************************************************************************************/
        private void SetupPunchButtons()
        {
            int totalwidth = 0;
            int numWidth = 20;
            for (int i = 0; i < gridMain.VisibleColumns.Count; i++)
            {
                if (!gridMain.VisibleColumns[i].Visible)
                    continue;
                string name = gridMain.VisibleColumns[i].FieldName.ToUpper();
                int width = gridMain.VisibleColumns[i].Width;
                if (name == "IN1")
                    btnPunchIn1.Left = totalwidth + 3 + numWidth;
                else if (name == "IN2")
                    btnPunchIn2.Left = totalwidth + 3 + numWidth;
                else if (name == "IN3")
                    btnPunchIn3.Left = totalwidth + 3 + numWidth;
                else if (name == "IN4")
                    btnPunchIn4.Left = totalwidth + 3 + numWidth;
                else if (name == "IN5")
                    btnPunchIn5.Left = totalwidth + 3 + numWidth;
                else if (name == "OUT1")
                    btnPunchOut1.Left = totalwidth + 3 + numWidth;
                else if (name == "OUT2")
                    btnPunchOut2.Left = totalwidth + 3 + numWidth;
                else if (name == "OUT3")
                    btnPunchOut3.Left = totalwidth + 3 + numWidth;
                else if (name == "OUT4")
                    btnPunchOut4.Left = totalwidth + 3 + numWidth;
                else if (name == "OUT5")
                    btnPunchOut5.Left = totalwidth + 3 + numWidth;
                totalwidth += width;
            }
            dgv.BringToFront();
            btnPunchIn1.BringToFront();
            btnPunchIn2.BringToFront();
            btnPunchIn3.BringToFront();
            btnPunchIn4.BringToFront();
            btnPunchIn5.BringToFront();
            btnPunchOut1.BringToFront();
            btnPunchOut2.BringToFront();
            btnPunchOut3.BringToFront();
            btnPunchOut4.BringToFront();
            btnPunchOut5.BringToFront();
        }
        /***********************************************************************************************/
        private DataTable SetupTimeData()
        {
            DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("date");
            dt.Columns.Add("day");
            dt.Columns.Add("empno");
            dt.Columns.Add("name");
            dt.Columns.Add("in1");
            dt.Columns.Add("out1");
            dt.Columns.Add("in2");
            dt.Columns.Add("out2");
            dt.Columns.Add("in3");
            dt.Columns.Add("out3");
            dt.Columns.Add("in4");
            dt.Columns.Add("out4");
            dt.Columns.Add("in5");
            dt.Columns.Add("out5");
            dt.Columns.Add("overtime");
            dt.Columns.Add("week1");
            dt.Columns.Add("week2");
            dt.Columns.Add("hours");
            dt.Columns.Add("total");
            dt.Columns.Add("pto");
            dt.Columns.Add("qpto");
            dt.Columns.Add("docked");
            dt.Columns.Add("qdocked");
            dt.Columns.Add("paid");
            dt.Columns.Add("notes");
            dt.Columns.Add("cyclenotes");
            dt.Columns.Add("vacation", Type.GetType("System.Double"));
            dt.Columns.Add("holiday", Type.GetType("System.Double"));
            dt.Columns.Add("sick", Type.GetType("System.Double"));
            dt.Columns.Add("other", Type.GetType("System.Double"));

            dt.Columns.Add("approve");
            dt.Columns.Add("holiday1");
            dt.Columns.Add("holiday2");
            dt.Columns.Add("picture", typeof(Bitmap));
            dt.Columns.Add("jobcode");
            dt.Columns.Add("mod");
            return dt;
        }
        /***********************************************************************************************/
        private void SetupSalariedHeaders()
        {
            bandPunch1.Visible = false;
            bandPunch2.Visible = false;
            bandPunch3.Visible = false;
            bandPunch4.Visible = false;
            bandPunch5.Visible = false;
            bandSalary.Visible = true;

            btnPunchIn1.Visible = false;
            btnPunchIn2.Visible = false;
            btnPunchIn3.Visible = false;
            btnPunchIn4.Visible = false;
            btnPunchIn5.Visible = false;
            btnPunchOut1.Visible = false;
            btnPunchOut2.Visible = false;
            btnPunchOut3.Visible = false;
            btnPunchOut4.Visible = false;
            btnPunchOut5.Visible = false;

            gridMain.Columns["date"].Visible = true;
            gridMain.Columns["day"].Visible = true;
            gridMain.Columns["hours"].Visible = true;
            gridMain.Columns["empno"].Visible = false;
            gridMain.Columns["name"].Visible = false;
            gridMain.Columns["picture"].Visible = false;

            //            btnAddPunch.Visible = false;
        }
        /***********************************************************************************************/
        private void LoadTimePeriod()
        {
            DataTable dt = SetupTimeData();
            if (string.IsNullOrWhiteSpace(empno) && !workGroup)
            {
                dgv.DataSource = dt;
                gridMain.Columns["date"].Visible = false;
                gridMain.Columns["day"].Visible = false;
                gridMain.Columns["total"].Visible = false;
                gridMain.Columns["week1"].Visible = false;
                gridMain.Columns["week2"].Visible = false;
                gridMain.Columns["empno"].Visible = true;
                gridMain.Columns["name"].Visible = true;
                gridMain.Columns["picture"].Visible = true;
                bandholiday.Visible = false;
                bandPTO.Visible = false;
                return;
            }
            gridMain.Columns["date"].Visible = true;
            gridMain.Columns["day"].Visible = true;
            gridMain.Columns["total"].Visible = true;
            gridMain.Columns["week1"].Visible = true;
            gridMain.Columns["week2"].Visible = true;
            gridMain.Columns["empno"].Visible = false;
            gridMain.Columns["name"].Visible = false;
            gridMain.Columns["picture"].Visible = false;
            btnSpyGlass.Visible = false;
            double zero = 0D;
            if (!workGroup)
            {
                for (int i = 0; i < 14; i++)
                {
                    DateTime date = dateTimePicker1.Value.AddDays(i);
                    DataRow dRow = dt.NewRow();
                    dRow["date"] = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    dRow["day"] = date.DayOfWeek.ToString();
                    dRow["hours"] = zero.ToString("###,###.00");
                    dRow["week1"] = zero.ToString("###,###.00");
                    dRow["week2"] = zero.ToString("###,###.00");
                    dRow["total"] = zero.ToString("###,###.00");
                    dt.Rows.Add(dRow);
                }
                bandholiday.Visible = false;
                bandPTO.Visible = false;
            }
            //string answer = G1.GetPreference("TimeClock", "Allow Edit PTO");
            string answer = "YES";
            if (answer != "YES")
                bandPTO.Visible = false;
            dgv.DataSource = dt;
            //if (!workGroup)
            CalculateDecember();
            if (workGroup)
            {
                bandPunch1.Visible = false;
                bandPunch2.Visible = false;
                bandPunch3.Visible = false;
                bandPunch4.Visible = false;
                bandPunch5.Visible = false;
                bandSalary.Visible = false;

                btnPunchIn1.Visible = false;
                btnPunchIn2.Visible = false;
                btnPunchIn3.Visible = false;
                btnPunchIn4.Visible = false;
                btnPunchIn5.Visible = false;
                btnPunchOut1.Visible = false;
                btnPunchOut2.Visible = false;
                btnPunchOut3.Visible = false;
                btnPunchOut4.Visible = false;
                btnPunchOut5.Visible = false;

                gridMain.Columns["date"].Visible = false;
                gridMain.Columns["day"].Visible = false;
                gridMain.Columns["hours"].Visible = false;
                gridMain.Columns["empno"].Visible = true;
                gridMain.Columns["name"].Visible = true;
                gridMain.Columns["picture"].Visible = true;

                //                btnSpyGlass.Visible = false;
                btnAddPunch.Visible = false;
                btnSpyGlass.Visible = true;
            }
        }
        /***********************************************************************************************/
        public static DateTime GetLastPostingDate()
        {
            DateTime date = new DateTime(2023, 1, 7);
            string cmd = "Select * from `options` where `option` = 'ptodate';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return date;
            int year = 0;
            int seq = 0;
            string sDate = dt.Rows[0]["answer"].ObjToString();
            if (G1.validate_date(sDate))
                date = sDate.ObjToDateTime();

            return date;
        }
        /***********************************************************************************************/
        private string formatDate(string date)
        {
            string[] Lines = date.Trim().Split(' ');
            date = Lines[0].Trim();
            if (G1.validate_date(date))
            {
                long lvalue = G1.date_to_days(date);
                date = G1.days_to_date(lvalue);
            }
            return date;
        }
        /***********************************************************************************************/
        private string formatTime(DateTime time)
        {
            string rtnTime = time.Hour.ToString("D2") + ":" + time.Minute.ToString("D2") + ":" + time.Second.ToString("D2");
            return rtnTime;
        }
        /***********************************************************************************************/
        private string formatTime(string time)
        {
            if (time.ToUpper() == "MIDNIGHT")
                return time;
            if (time.Trim().IndexOf("/") > 0)
            {
                string[] l = time.Trim().Split(' ');
                if (l.Length > 1)
                    time = l[1].Trim();
            }
            string[] Lines = time.Split(':');
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            if (Lines.Length > 0)
            {
                if (G1.validate_numeric(Lines[0]))
                    hours = Lines[0].ObjToInt32();
            }
            if (Lines.Length > 1)
            {
                if (G1.validate_numeric(Lines[1]))
                    minutes = Lines[1].ObjToInt32();
            }
            if (Lines.Length > 2)
            {
                if (G1.validate_numeric(Lines[2]))
                    seconds = Lines[2].ObjToInt32();
            }
            //            time = hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
            time = hours.ToString("D2") + ":" + minutes.ToString("D2");
            return time;
        }
        /***********************************************************************************************/
        private double RoundValue(double value)
        {
            long lvalue = (long)((value + .005D) * 100.0D);
            value = (double)(lvalue) / 100.0D;
            return value;
        }
        /***********************************************************************************************/
        private void SelectCurrentDay()
        {
            //            DateTime today = G1.GetCurrentDateTime();
            DateTime today = dateTimePicker1.Value;
            string datein = today.Month.ToString("D2") + "/" + today.Day.ToString("D2") + "/" + today.Year.ToString("D4");
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DateTime d = dt.Rows[i]["date"].ObjToDateTime();
                string str = d.Month.ToString("D2") + "/" + d.Day.ToString("D2") + "/" + d.Year.ToString("D4");
                if (str == datein)
                {
                    gridMain.SelectRows(i, i);
                    gridMain.FocusedRowHandle = i;
                    break;
                }
            }
        }
        /***********************************************************************************************/
        private void GetSavedDate(ref long saveldate, ref long saveedate)
        {
            DateTime timePeriod = dateTimePicker1.Value;

            string mydate = timePeriod.Month.ToString("D2") + "/" + timePeriod.Day.ToString("D2") + "/" + timePeriod.Year.ToString("D4");

            timePeriod = mydate.ObjToDateTime();

            long ldate = G1.TimeToUnix(timePeriod);
            long edate = G1.TimeToUnix(timePeriod.AddHours(23));
            if (workGroup)
            {
                DateTime endPeriod = timePeriod.AddDays(14).AddMinutes(-1);
                edate = G1.TimeToUnix(endPeriod);
            }
            saveldate = ldate;
            saveedate = edate;
        }
        /***********************************************************************************************/
        private long ConvertUTS(long ldate)
        {
            DateTime date = ldate.UnixToDateTime();
            DateTime newDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            long newTime = G1.TimeToUnix(newDate);
            return newTime;
        }
        /***********************************************************************************************/
        private void GetAllPunches()
        {
            //btnClock.Hide();
            //btnDecimal.Hide();
            this.Cursor = Cursors.WaitCursor;
            this.lblTimeApprovedBy.Hide();
            DateTime timePeriod = dateTimePicker1.Value;

            string mydate = timePeriod.Month.ToString("D2") + "/" + timePeriod.Day.ToString("D2") + "/" + timePeriod.Year.ToString("D4");

            timePeriod = mydate.ObjToDateTime();

            long ldate = G1.TimeToUnix(timePeriod);
            //            long edate = timePeriod.AddHours(23).ToUnix() + 36000L;
            long edate = G1.TimeToUnix(timePeriod.AddHours(23));
            if (workGroup)
            {
                DateTime endPeriod = timePeriod.AddDays(14).AddMinutes(-1);
                //endPeriod = endPeriod.AddHours(23);
                //endPeriod = endPeriod.AddMinutes(59);
                //                edate = dateTimePicker2.Value.ToUnix();
                //                edate = G1.TimeToUnix(dateTimePicker2.Value);
                edate = G1.TimeToUnix(endPeriod);
            }

            chkUnapproved.Visible = true;
            btnAddNextPunch.Visible = false;

            DataTable timDt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(timDt, "ERROR") < 0)
                timDt.Columns.Add("ERROR");
            if (G1.get_column_number(timDt, "december") < 0)
                timDt.Columns.Add("december", Type.GetType("System.Double"));
            if (G1.get_column_number(timDt, "available") < 0)
                timDt.Columns.Add("available", Type.GetType("System.Double"));
            if (G1.get_column_number(timDt, "overtime") < 0)
                timDt.Columns.Add("overtime", Type.GetType("System.Double"));
            if (G1.get_column_number(timDt, "expectedhours") < 0)
                timDt.Columns.Add("expectedhours", Type.GetType("System.Double"));
            if (G1.get_column_number(timDt, "parttime") < 0)
                timDt.Columns.Add("parttime");
            if (G1.get_column_number(timDt, "method") < 0)
                timDt.Columns.Add("method");
            if (G1.get_column_number(timDt, "notes") < 0)
                timDt.Columns.Add("notes");
            if (G1.get_column_number(timDt, "approve") < 0)
                timDt.Columns.Add("approve");
            if (G1.get_column_number(timDt, "cyclenotes") < 0)
                timDt.Columns.Add("cyclenotes");
            if (G1.get_column_number(timDt, "allowpto") < 0)
                timDt.Columns.Add("allowpto");
            if (G1.get_column_number(timDt, "vacation") < 0)
                timDt.Columns.Add("vacation", Type.GetType("System.Double"));
            if (G1.get_column_number(timDt, "holiday") < 0)
                timDt.Columns.Add("holiday", Type.GetType("System.Double"));
            if (G1.get_column_number(timDt, "sick") < 0)
                timDt.Columns.Add("sick", Type.GetType("System.Double"));
            if (G1.get_column_number(timDt, "other") < 0)
                timDt.Columns.Add("other", Type.GetType("System.Double"));

            dgv.DataSource = null;

            DateTime firstDate = ldate.UnixToDateTime();
            DateTime lastDate = edate.UnixToDateTime();

            firstDate = timePeriod;
            lastDate = timePeriod.AddHours(23);
            if (workGroup)
                lastDate = dateTimePicker2.Value;

            DateTime saveFirstDate = firstDate;
            DateTime saveLastDate = lastDate;

            string cmd = "Select * from `tc_punches_pchs` where `UTS_Added` >= '" + ldate + "' and `UTS_Added` <= '" + edate + "' ";
            cmd += "order by `empy!AccountingID`,`UTS_Added`;";

            long saveLdate = ldate;
            long saveEdate = edate;

            saveReportLdate = ldate;
            saveReportEdate = edate;

            //            DataTable dx = G1.get_db3_data(cmd);
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("date");
            dx.Columns.Add("day");
            dx.Columns.Add("week");
            dx.Columns.Add("hours", Type.GetType("System.Double"));
            dx.Columns.Add("week1", Type.GetType("System.Double"));
            dx.Columns.Add("week2", Type.GetType("System.Double"));
            dx.Columns.Add("total", Type.GetType("System.Double"));
            dx.Columns.Add("overtime", Type.GetType("System.Double"));
            dx.Columns.Add("in1");
            dx.Columns.Add("in2");
            dx.Columns.Add("in3");
            dx.Columns.Add("in4");
            dx.Columns.Add("in5");
            dx.Columns.Add("out1");
            dx.Columns.Add("out2");
            dx.Columns.Add("out3");
            dx.Columns.Add("out4");
            dx.Columns.Add("out5");
            dx.Columns.Add("pto", Type.GetType("System.Double"));
            dx.Columns.Add("qpto", Type.GetType("System.Double"));
            dx.Columns.Add("docked", Type.GetType("System.Double"));
            dx.Columns.Add("qdocked", Type.GetType("System.Double"));
            dx.Columns.Add("paid", Type.GetType("System.Double"));
            dx.Columns.Add("holiday1", Type.GetType("System.Double"));
            dx.Columns.Add("holiday2", Type.GetType("System.Double"));
            dx.Columns.Add("notes");
            dx.Columns.Add("cyclenotes");
            dx.Columns.Add("approve");
            dx.Columns.Add("allowpto");

            gridMain.Columns["overtime"].Visible = true;

            string oldempno = "";
            Bitmap emptyImage = new Bitmap(1, 1);
            double december = 0D;
            double available = 0D;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                bool deleted = dx.Rows[i]["Deleted"].ObjToBool();
                if (deleted)
                    continue;
                bool manual = dx.Rows[i]["ManualEntry"].ObjToBool();
                string emp = dx.Rows[i]["empy!AccountingID"].ObjToString();
                if (emp != oldempno)
                {
                    cmd = "Select * from `er` where `empno` = '" + emp + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        DataRow dRow = timDt.NewRow();
                        dRow["empno"] = emp;
                        dRow["name"] = ddx.Rows[0]["name"].ObjToString();
                        dRow["jobcode"] = ddx.Rows[0]["jobcode"].ObjToString();
                        dRow["picture"] = emptyImage;
                        Byte[] bytes = ddx.Rows[0]["picture"].ObjToBytes();
                        Image myImage = emptyImage;
                        if (bytes != null)
                        {
                            myImage = G1.byteArrayToImage(bytes);
                            dRow["picture"] = (Bitmap)(myImage);
                        }
                        december = 0D;
                        available = 0D;
                        bool allow = CalcDecember(ddx, 0, ref december, ref available, false);
                        dRow["december"] = RoundValue(december);
                        dRow["available"] = RoundValue(available);
                        dRow["expectedhours"] = ddx.Rows[0]["expectedhours"].ObjToDouble();
                        dRow["parttime"] = ddx.Rows[0]["parttime"].ObjToString();
                        if (!allow)
                            dRow["allowpto"] = "NO";
                        timDt.Rows.Add(dRow);
                    }
                    else
                    {
                        DataRow dRow = timDt.NewRow();
                        dRow["empno"] = emp;
                        dRow["name"] = "New Employee/ Pull New List";
                        timDt.Rows.Add(dRow);
                    }
                    oldempno = emp;
                    //if (workGroup )
                    //{
                    //    GetEmployeePunches(emp, timDt );
                    //}
                }
            }

            dx.Columns.Add("Stat");
            long oldtime = 0L;
            long firsttime = 0L;
            long lasttime = 0L;
            int oldday = 0;
            bool newemp = true;
            bool newdate = true;
            string status = "in";
            DateTime lastDtime;
            int count = 0;
            string str = "";
            string dow = "";
            oldempno = "";

            int row = -1;

            //bool holidayAdded = AddHolidayPay(timDt, saveFirstDate, saveLastDate);
            //if ( holidayAdded)
            //    bandholiday.Visible = true;
            //if ( workGroup)
            //{
            //    dx.AcceptChanges();
            //    CleanupAllColumns(timDt);
            //    CheckForErrors(timDt);
            //    dgv.DataSource = timDt;
            //    ShowBands();
            //    SelectCurrentDay();
            //    string emp = timDt.Rows[0]["empno"].ObjToString();
            //    CalculateDecember(emp);
            //    this.Cursor = Cursors.Default;
            //    return;
            //}

            long lastTime = 0L;
            int groupRow = 0;
            double week1 = 0D;
            double week2 = 0D;
            long diff = 0L;

            try
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    bool deleted = dx.Rows[i]["Deleted"].ObjToBool();
                    if (deleted)
                        continue;
                    bool manual = dx.Rows[i]["ManualEntry"].ObjToBool();
                    string emp = dx.Rows[i]["empy!AccountingID"].ObjToString();
                    ldate = dx.Rows[i]["UTS_Added"].ObjToInt64();
                    ldate = ConvertUTS(ldate); // Trim Seconds
                    DateTime date = ldate.UnixToDateTime();
                    dow = date.DayOfWeek.ToString();
                    if (emp != oldempno)
                    {
                        if (emp == "140")
                        {
                        }
                        week1 = 0D;
                        week2 = 0D;
                        status = "in";
                        oldempno = emp;
                        firsttime = ldate;
                        lastDtime = date;
                        firstDate = date;
                        lastTime = ldate;
                        dx.Rows[i]["stat"] = status;
                        //if (dow.ToUpper() == "FRIDAY")
                        //    dow = "Fri/Sat";
                        //else if (dow.ToUpper() == "SATURDAY")
                        //    dow = "Sat/Sun";
                        //else if (dow.ToUpper() == "SUNDAY")
                        //    dow = "Sun/Mon";
                        //else if (dow.ToUpper() == "MONDAY")
                        //    dow = "Mon/Tue";
                        //else if (dow.ToUpper() == "TUESDAY")
                        //    dow = "Tue/Wed";
                        //else if (dow.ToUpper() == "WEDNESDAY")
                        //    dow = "Wed/Thu";
                        //else if (dow.ToUpper() == "THURSDAY")
                        //    dow = "Thu/Fri";
                        dx.Rows[i]["day"] = dow;
                        dx.Rows[i]["in1"] = date;
                        //                        int row = LocateDateRow(timDt, date);
                        row++;
                        groupRow = row;
                        timDt.Rows[row]["in1"] = formatTime(date);
                        timDt.Rows[row]["method"] = "H";
                        //DataRow dRow = timDt.NewRow();
                        //dRow["day"] = dow;
                        //dRow["date"] = date;
                        //dRow["in1"] = date;
                        //timDt.Rows.Add(dRow);
                        count = 1;
                    }
                    else
                    {
                        if (emp == "140")
                        { // An attempt to block out times punched less than 60 seconds. Didn't seem to catch everything
                            diff = ldate - lasttime;
                            if (diff < 60L)
                            {
                                lasttime = ldate;
                                //                                continue;
                            }
                        }
                        lasttime = ldate;
                        long jdate = dt_to_days(firstDate);
                        long kdate = dt_to_days(date);
                        if (kdate != jdate)
                        {
                            status = "in";
                            firsttime = ldate;
                            lastDtime = date;
                            firstDate = date;
                            dx.Rows[i]["stat"] = status;
                            dx.Rows[i]["day"] = dow;
                            count = 1;
                            str = "in" + count.ObjToString();
                            dx.Rows[i][str] = date;
                            //                            int row = LocateDateRow(timDt, date);
                            timDt.Rows[row]["in1"] = formatTime(date);
                            timDt.Rows[row]["ERROR"] = "YES";
                            //DataRow dRow = timDt.NewRow();
                            //dRow["day"] = dow;
                            //dRow["date"] = date;
                            //dRow["in1"] = date;
                            //timDt.Rows.Add(dRow);
                        }
                        else
                        {
                            if (status == "in")
                            {
                                double dHours = (double)(ldate - firsttime) / 3600D;
                                double localHours = dHours;
                                dx.Rows[i]["hours"] = RoundValue(dHours);
                                status = "out";
                                lastDtime = date;
                                dx.Rows[i]["stat"] = status;
                                dx.Rows[i]["day"] = dow;
                                str = "out" + count.ObjToString();
                                dx.Rows[i][str] = date;
                                if (timDt.Rows.Count > 0)
                                {
                                    //int nrow = timDt.Rows.Count - 1;
                                    //nrow = LocateDateRow(timDt, date);
                                    timDt.Rows[row][str] = formatTime(date);
                                    timDt.Rows[row]["ERROR"] = "";
                                    dHours += timDt.Rows[row]["hours"].ObjToDouble();
                                    timDt.Rows[row]["hours"] = RoundValue(dHours);
                                    timDt.Rows[row]["total"] = RoundValue(dHours);
                                    DateTime localTime = ldate.UnixToDateTime();
                                    TimeSpan ts = localTime - saveFirstDate;
                                    if (ts.TotalDays < 7)
                                    {
                                        week1 += RoundValue(localHours);
                                        timDt.Rows[row]["week"] = "1";
                                    }
                                    else
                                    {
                                        week2 += RoundValue(localHours);
                                        timDt.Rows[row]["week"] = "2";
                                    }
                                    timDt.Rows[row]["week1"] = RoundValue(week1);
                                    timDt.Rows[row]["week2"] = RoundValue(week2);
                                }
                            }
                            else
                            {
                                status = "in";
                                lastDtime = date;
                                firstDate = date;
                                firsttime = ldate;
                                //                                if ( !workGroup )
                                count++;
                                if (count < 6)
                                {
                                    dx.Rows[i]["stat"] = status;
                                    dx.Rows[i]["day"] = dow;
                                    str = "in" + count.ObjToString();
                                    dx.Rows[i][str] = date;
                                    if (timDt.Rows.Count > 0)
                                    {
                                        //int nrow = timDt.Rows.Count - 1;
                                        //nrow = LocateDateRow(timDt, date);
                                        timDt.Rows[row][str] = formatTime(date);
                                        timDt.Rows[row]["ERROR"] = "YES";
                                    }
                                }
                                else
                                {
                                    count--;
                                    timDt.Rows[row]["ERROR"] = "YES";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            cmd = "Select * from `tc_salarylog_salg` where `UnixTime` >= '" + saveLdate + "' and `UnixTime` <= '" + saveEdate + "' ";
            cmd += "order by `empy!AccountingID`,`UnixTime`;";

            dx = G1.get_db_data(cmd);

            row = timDt.Rows.Count - 1;

            oldempno = "";
            string testemp = "";
            bool found = false;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                string emp = dx.Rows[i]["empy!AccountingID"].ObjToString();
                if (emp != oldempno)
                {
                    cmd = "Select * from `er` where `empno` = '" + emp + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        if (ddx.Rows[0]["term"].ObjToString().ToUpper() == "Y")
                        {
                            if (ddx.Rows[0]["special"].ObjToString().ToUpper() != "Y")
                                continue;
                        }
                        DataRow dRow = null;
                        found = false;
                        for (int j = 0; j < timDt.Rows.Count; j++)
                        {
                            testemp = timDt.Rows[j]["empno"].ObjToString();
                            if (testemp == emp)
                            {
                                dRow = timDt.Rows[j];
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            dRow = timDt.NewRow();
                        dRow["empno"] = emp;
                        dRow["name"] = ddx.Rows[0]["name"].ObjToString();
                        dRow["jobcode"] = ddx.Rows[0]["jobcode"].ObjToString();
                        dRow["method"] = "S";
                        dRow["expectedhours"] = ddx.Rows[0]["expectedhours"].ObjToDouble();
                        dRow["picture"] = emptyImage;
                        Byte[] bytes = ddx.Rows[0]["picture"].ObjToBytes();
                        Image myImage = emptyImage;
                        if (bytes != null)
                        {
                            myImage = G1.byteArrayToImage(bytes);
                            dRow["picture"] = (Bitmap)(myImage);
                        }
                        december = 0D;
                        available = 0D;
                        bool allow = CalcDecember(ddx, 0, ref december, ref available, false);
                        dRow["december"] = RoundValue(december);
                        dRow["available"] = RoundValue(available);
                        if (!allow)
                            dRow["allowpto"] = "NO";
                        if (!found)
                            timDt.Rows.Add(dRow);
                    }
                    oldempno = emp;
                }
            }

            for (int i = 0; i < timDt.Rows.Count; i++)
                timDt.Rows[i]["num"] = (i + 1).ToString();

            week1 = 0D;
            week2 = 0D;
            double total = 0D;
            double Pto = 0D;
            double Overtime = 0D;
            double Docked = 0D;
            double Worked = 0D;
            double Vacation = 0D;
            double Holiday = 0D;
            double Sick = 0D;
            double Other = 0D;

            double tPto = 0D;
            double tDocked = 0D;
            double tOvertime = 0D;
            double tWorked = 0D;
            double tVacation = 0D;
            double tHoliday = 0D;
            double tSick = 0D;
            double tOther = 0D;
            double hoursExpected = 0D;
            oldempno = "";

            try
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    string emp = "";
                    try
                    {
                        emp = dx.Rows[i]["empy!AccountingID"].ObjToString();
                        ldate = dx.Rows[i]["UnixTime"].ObjToInt64();
                        DateTime date = ldate.UnixToDateTime();
                        dow = date.DayOfWeek.ToString();
                        if (emp != oldempno)
                        {
                            if (emp == "268")
                            {

                            }
                            row = FindDataTableRow(emp, timDt);
                            if (row < 0)
                                continue;
                            //                            row++;
                            total = 0D;
                            tPto = 0L;
                            tDocked = 0D;
                            tOvertime = 0D;
                            tWorked = 0D;
                            tVacation = 0D;
                            tHoliday = 0D;
                            tSick = 0D;
                            tOther = 0D;
                            week1 = 0D;
                            week2 = 0D;
                            hoursExpected = 0D;
                            oldempno = emp;
                            //if ( emp == "130")
                            //{
                            total = timDt.Rows[row]["total"].ObjToDouble();
                            week1 = timDt.Rows[row]["week1"].ObjToDouble();
                            week2 = timDt.Rows[row]["week2"].ObjToDouble();
                            hoursExpected = timDt.Rows[row]["expectedhours"].ObjToDouble();
                            if (hoursExpected >= 84D)
                            {
                            }
                            //}
                        }
                        DecodeSalaryRow(dx, i, hoursExpected, ref Worked, ref Vacation, ref Holiday, ref Sick, ref Other);
                        tWorked += Worked;
                        tVacation += Vacation;
                        tHoliday += Holiday;
                        tSick += Sick;
                        tOther += Other;

                        total = Worked + Other + Holiday;
                        tPto = Vacation + Sick;

                        DateTime localTime = ldate.UnixToDateTime();
                        TimeSpan ts = localTime - saveFirstDate;
                        if (ts.TotalDays < 7)
                        {
                            week1 += RoundValue(total);
                            timDt.Rows[row]["week"] = "1";
                        }
                        else
                        {
                            week2 += RoundValue(total);
                            timDt.Rows[row]["week"] = "2";
                        }
                        timDt.Rows[row]["total"] = RoundValue(week1 + week2);
                        timDt.Rows[row]["week1"] = RoundValue(week1);
                        timDt.Rows[row]["week2"] = RoundValue(week2);
                        timDt.Rows[row]["pto"] = RoundValue(tPto);
                        timDt.Rows[row]["qpto"] = 0D;
                        timDt.Rows[row]["qdocked"] = 0D;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            LoadMissing(timDt);

            bool holidayAdded = AddHolidayPay(timDt, saveFirstDate, saveLastDate);
            //if (holidayAdded)
            //    bandholiday.Visible = true;

            G1.sortTable(timDt, "empno", "ASC");

            available = 0D;
            december = 0D;
            double expectedHours = 0D;
            string parttime = "";
            try
            {
                for (int i = 0; i < timDt.Rows.Count; i++)
                {
                    timDt.Rows[i]["overtime"] = 0D;
                    string emp = timDt.Rows[i]["empno"].ObjToString();
                    if (emp == "130")
                    {
                    }
                    week1 = timDt.Rows[i]["week1"].ObjToDouble();
                    week2 = timDt.Rows[i]["week2"].ObjToDouble();
                    total = timDt.Rows[i]["total"].ObjToDouble();
                    available = timDt.Rows[i]["available"].ObjToDouble();
                    december = timDt.Rows[i]["december"].ObjToDouble();
                    expectedHours = timDt.Rows[i]["expectedhours"].ObjToDouble();
                    parttime = timDt.Rows[i]["parttime"].ObjToString();

                    if (expectedHours >= 84)
                    {
                        CalcPtoDockeOvertime(week1, week2, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                        tPto = Pto;
                        tDocked = Docked;
                        tOvertime = Overtime;
                    }
                    else
                    {
                        CalcPtoDockeOvertime(week1, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                        tPto = Pto;
                        tDocked = Docked;
                        tOvertime = Overtime;

                        CalcPtoDockeOvertime(week2, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                        tPto += Pto;
                        tDocked += Docked;
                        tOvertime += Overtime;
                    }

                    timDt.Rows[i]["overtime"] = RoundValue(tOvertime);
                    timDt.Rows[i]["qpto"] = RoundValue(tPto);
                    timDt.Rows[i]["qdocked"] = RoundValue(tDocked);
                    //                    timDt.Rows[i]["total"] = RoundValue(week1 + week2 + tOvertime + tPto);
                    timDt.Rows[i]["total"] = RoundValue(week1 + week2 + tPto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Loading PTO" + ex.Message.ToString());
            }

            dx.AcceptChanges();
            CleanupAllColumns(timDt);
            CheckForErrors(timDt);
            int week = 0;
            for (int i = 0; i < timDt.Rows.Count; i++)
            {
                dow = timDt.Rows[i]["day"].ObjToString().ToUpper();
                if (dow == "FRIDAY")
                    week++;
                timDt.Rows[i]["week"] = week.ToString();
            }
            dgv.DataSource = timDt;
            ShowBands();
            SetApprovals();
            SetNotes(saveLdate, saveEdate);
            gridMain.Columns["cyclenotes"].Visible = true;
            SelectCurrentDay();
            gridMain.FocusedRowHandle = 0;
            SavedSuperDt = (DataTable)dgv.DataSource;
            ShowPictureRow(SavedSuperDt, 0);

            //            CalcDecember(timDt, 0, ref december, ref available);
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ShowPicture(DataRow dr)
        {
            if (dr["picture"] != null)
            {
                try
                {
                    this.picEmployee.Image = (Bitmap)(dr["picture"]);
                    string emp = dr["empno"].ObjToString();
                    CalculateDecember(emp);
                    txtAvailablePTO.Refresh();
                    txtDecemberPTO.Refresh();
                }
                catch (Exception ex)
                {
                    if (allEmployees)
                    {
                        Bitmap emptyImage = new Bitmap(1, 1);
                        this.picEmployee.Image = emptyImage;
                    }
                }
            }
            else
            {
                Bitmap emptyImage = new Bitmap(1, 1);
                this.picEmployee.Image = emptyImage;
            }
        }
        /***********************************************************************************************/
        private void ShowPictureRow(DataTable dt, int row)
        {
            if (row < 0)
                return;
            DataRow dr = dt.Rows[row];
            ShowPicture(dr);
        }
        /***********************************************************************************************/
        private int FindDataTableRow(string emp, DataTable dt)
        {
            int row = -1;
            string oldemp = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                oldemp = dt.Rows[i]["empno"].ObjToString();
                if (oldemp == emp)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /***********************************************************************************************/
        private void CalcPtoDockeOvertime(double week1, double week2, double ptoAvailable, double DecemberPTO, double expectedHours, string parttime, ref double Pto, ref double Docked, ref double Overtime)
        {
            Pto = 0D;
            Docked = 0D;
            Overtime = 0D;
            if (expectedHours >= 84)
            {
                double hours = 84D;
                if (expectedHours > 0D)
                    hours = expectedHours / 2D;
                double total = week1 + week2;
                if (total < hours && parttime.ToUpper() != "Y")
                {
                    if (DecemberPTO > (hours - total))
                    {
                        double qPto = hours - total;
                        if (qPto > 0D)
                        {
                            Pto = RoundValue(qPto);
                        }
                    }
                    else
                        Docked = RoundValue(hours - total);
                }
            }
        }
        /***********************************************************************************************/
        private void CalcPtoDockeOvertime(double total, double ptoAvailable, double DecemberPTO, double expectedHours, string parttime, ref double Pto, ref double Docked, ref double Overtime)
        {
            Pto = 0D;
            Docked = 0D;
            Overtime = 0D;
            double hours = 40D;
            if (expectedHours > 0D)
                hours = expectedHours / 2D;
            if (total < hours && parttime.ToUpper() != "Y")
            {
                if (DecemberPTO > (hours - total))
                {
                    double qPto = hours - total;
                    if (qPto > 0D)
                    {
                        Pto = RoundValue(qPto);
                    }
                }
                else
                    Docked = RoundValue(hours - total);
            }
            else if (total > 40D) // Even if Expected hours are less than 40, only pay overtime over 40
            {
                double diff = total - 40D;
                Overtime = RoundValue(diff);
            }
        }
        /***********************************************************************************************/
        private void LoadMissing(DataTable timDt)
        {
            string cmd = "Select * from `er` where ( `empno` > '100' and `empno` <> '500' and `status` <> 'deactivate' and `term` <> 'Y' ) or ( `special` = 'Y' );";
            DataTable oldDt = G1.get_db_data(cmd);
            if (oldDt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot locate employees!");
                return;
            }

            Bitmap emptyImage = new Bitmap(1, 1);
            double december = 0D;
            double available = 0D;
            string str = "";
            string parttime = "";
            string emp = "";

            for (int i = 0; i < oldDt.Rows.Count; i++)
            {
                emp = oldDt.Rows[i]["empno"].ObjToString();
                if (emp == "420")
                {
                }
                parttime = oldDt.Rows[i]["parttime"].ObjToString();
                //if (!string.IsNullOrWhiteSpace(parttime))
                //{
                //    str = parttime.Substring(0, 1);
                //    if (str.ToUpper() == "Y")
                //        continue; // Don't load empty Parttime people
                //}
                DataRow[] dRow = timDt.Select("empno='" + emp + "'");
                if (dRow.Length <= 0)
                {
                    DataRow dR = timDt.NewRow();
                    dR["empno"] = emp;
                    dR["name"] = oldDt.Rows[i]["name"].ObjToString();
                    dR["jobcode"] = oldDt.Rows[i]["jobcode"].ObjToString();
                    Byte[] bytes = oldDt.Rows[i]["picture"].ObjToBytes();
                    Image myImage = emptyImage;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        dR["picture"] = (Bitmap)(myImage);
                    }
                    dR["week1"] = 0D;
                    dR["week2"] = 0D;
                    dR["total"] = 0D;
                    dR["hours"] = 0D;
                    december = 0D;
                    available = 0D;
                    bool allow = CalcDecember(oldDt, i, ref december, ref available, false);
                    dR["december"] = RoundValue(december);
                    dR["available"] = RoundValue(available);
                    dR["ERROR"] = "YES";
                    if (!allow)
                        dR["allowpto"] = false;
                    dR["parttime"] = parttime;
                    timDt.Rows.Add(dR);
                }
            }
        }
        /***********************************************************************************************/
        private string GetSalaried(string empno)
        {
            string salaried = "H";
            string cmd = "Select * from `er` where `empno` = '" + empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                salaried = dt.Rows[0]["salaried"].ObjToString();
                if (string.IsNullOrWhiteSpace(salaried))
                    salaried = "H";
            }
            return salaried;
        }
        /***********************************************************************************************/
        private double GetExpectedHours(string empno)
        {
            double expectedHours = 8D;
            string cmd = "Select * from `er` where `empno` = '" + empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                expectedHours = dt.Rows[0]["expectedHours"].ObjToDouble();
            return expectedHours;
        }
        /***********************************************************************************************/
        private void DecodeSalaryRow(DataTable dt, int row, double hoursExpected, ref double Worked, ref double Vacation, ref double Holiday, ref double Sick, ref double Other)
        {
            Worked = 0D;
            Vacation = 0D;
            Holiday = 0D;
            Sick = 0D;
            Other = 0D;

            string text = dt.Rows[row]["hours_work"].ObjToString().ToUpper();
            if (text != "FULL" && text != "HALF")
            {
                Other = ParseOutHoursWorked(dt.Rows[row]["hours_work"].ObjToString(), hoursExpected);
                double hours_other = ParseOutHoursWorked(dt.Rows[row]["hours_other"].ObjToString(), hoursExpected);
                if (hours_other > 0D)
                    Other += hours_other;
                hours_other = ParseOutHoursWorked(dt.Rows[row]["hours_holiday"].ObjToString(), hoursExpected);
                if (hours_other > 0D)
                    Other += hours_other;
            }
            else
            {

                Worked = ParseOutHoursWorked(dt.Rows[row]["hours_work"].ObjToString(), hoursExpected);
                Other = ParseOutHoursWorked(dt.Rows[row]["hours_other"].ObjToString(), hoursExpected);
            }
            Vacation = ParseOutHoursWorked(dt.Rows[row]["hours_vacation"].ObjToString(), hoursExpected);
            Sick = ParseOutHoursWorked(dt.Rows[row]["hours_sick"].ObjToString(), hoursExpected);
        }
        /***********************************************************************************************/
        private double ParseOutHoursWorked(string str, double hoursExpected)
        {
            double worked = 0D;
            string[] Lines = str.Split(' ');
            if (Lines.Length >= 1)
            {
                double dValue = 0D;
                if (Lines[0].Trim().ToUpper() == "FULL")
                {
                    dValue = 8.0D;
                    if (hoursExpected >= 84D)
                        dValue = 12D;
                }
                else if (Lines[0].Trim().ToUpper() == "HALF")
                {
                    dValue = 4.0D;
                    if (hoursExpected >= 84D)
                        dValue = 6D;
                }
                else
                {
                    if (G1.validate_numeric(Lines[0].Trim()))
                        dValue = Lines[0].ObjToDouble();
                    else if (!string.IsNullOrWhiteSpace(Lines[0]))
                    {
                        dValue = 8D;
                        if (hoursExpected >= 84D)
                            dValue = 12D;
                    }
                }
                worked = dValue;
            }
            return worked;
        }
        /***********************************************************************************************/
        private void CopyRow(DataTable dtin, int row, DataTable dtout)
        {
            DataRow dRow = dtout.NewRow();
            dtout.Rows.Add(dRow);
            int r = dtout.Rows.Count - 1;
            G1.copy_dt_row(dtin, row, dtout, r);
        }
        /***********************************************************************************************/
        private bool gotTime = false;
        private bool gotContract = false;
        private bool gotOther = false;
        private DataTable GetEmployeePunches(string employee = "", DataTable groupDt = null, bool reporting = false)
        {
            gotTime = false;
            gotContract = false;
            gotOther = false;
            if (string.IsNullOrEmpty(employee))
            {
                if (allEmployees && groupDt == null)
                {
                    GetAllPunches();
                    return null;
                }
            }

            btnClock.Show();
            btnClock.BringToFront();
            chkUnapproved.Visible = false;
            this.timer1.Enabled = true;

            DateTime date = DateTime.Now;

            bool returning = false;
            DataTable dt;
            string emp = empno;
            if (string.IsNullOrWhiteSpace(emp) && !string.IsNullOrWhiteSpace(employee) && groupDt != null)
            {
                emp = employee;
                returning = true;
            }
            //dt = G1.get_db_data("Select * from `er` where `empno` = '" + emp + "';");
            //if (dt.Rows.Count <= 0)
            //    return null;

            string parttime = "";
            //string parttime = dt.Rows[0]["parttime"].ObjToString();

            //if (dt.Rows[0]["picture"] != null && !reporting)
            //{
            //    Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
            //    if (bytes != null)
            //    {
            //        Image myImage = G1.byteArrayToImage(bytes);
            //        this.picEmployee.Image = (Bitmap)(myImage);
            //    }
            //}
            //bandSalary.Visible = false;
            string salaried = "";
            //string salaried = dt.Rows[0]["salaried"].ObjToString();
            //if (workMethod == "S")
            //    salaried = "S";
            //if (salaried == "S")
            //{
            //    SetupSalariedHeaders();
            //    menuFormats.Visibility = BarItemVisibility.Always;
            //    //gridMain.Columns["worked"].OptionsColumn.ReadOnly = false;
            //}

            //if (!reporting)
            //    CalculateDecember(employee);

            DateTime timePeriod = dateTimePicker1.Value;

            long ldate = G1.TimeToUnix(timePeriod);
            timePeriod = timePeriod.AddDays(7D);
            long hdate = G1.TimeToUnix(timePeriod);
            timePeriod = timePeriod.AddDays(7D);
            //timePeriod = timePeriod.AddMinutes(-1); // This gets the time back to 23:59:00
            long edate = G1.TimeToUnix(timePeriod);
            long adate = 0L;

            DataTable timDt = null;
            if (groupDt != null)
            {
                timDt = groupDt.Clone();
                double zero = 0D;
                for (int i = 0; i < 14; i++)
                {
                    date = dateTimePicker1.Value.AddDays(i);
                    DataRow dRow = timDt.NewRow();
                    dRow["date"] = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    dRow["day"] = date.DayOfWeek.ToString();
                    dRow["hours"] = zero.ToString("###,###.00");
                    dRow["week1"] = zero.ToString("###,###.00");
                    dRow["week2"] = zero.ToString("###,###.00");
                    dRow["total"] = zero.ToString("###,###.00");
                    timDt.Rows.Add(dRow);
                }
            }
            else
                timDt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(timDt, "ERROR") < 0)
                timDt.Columns.Add("ERROR");

            if (G1.get_column_number(timDt, "full") < 0)
            {
                timDt.Columns.Add("full");
                timDt.Columns.Add("part");
                timDt.Columns.Add("worked");
            }
            if (G1.get_column_number(timDt, "notes") < 0)
                timDt.Columns.Add("notes");
            if (G1.get_column_number(timDt, "approve") < 0)
                timDt.Columns.Add("approve");
            if (G1.get_column_number(timDt, "cyclenotes") < 0)
                timDt.Columns.Add("cyclenotes");
            if (G1.get_column_number(timDt, "week") < 0)
                timDt.Columns.Add("week");

            DateTime firstDate = ldate.UnixToDateTime().ToLocalTime();
            if (workEmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
                firstDate = firstDate.AddDays(-1);
            DateTime lastDate = edate.UnixToDateTime().ToLocalTime();
            if (workEmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
                lastDate = lastDate.AddDays(-1);

            DateTime saveFirstDate = firstDate;
            DateTime saveLastDate = lastDate;

            long saveLdate = ldate;
            long saveEdate = edate;

            // 1664561417

            //firstDate = firstDate.AddMinutes(301);
            //lastDate = lastDate.AddHours(5);

            string str = lastDate.ToString("MM/dd/yyyy");
            lastDate = str.ObjToDateTime();

            DateTime firstRealDate = new DateTime(firstDate.Year, firstDate.Month, firstDate.Day, 17, 0, 1 ); // One (1) Second After 5PM
            //if (workEmpStatus.ToUpper().IndexOf("PARTTIME") >= 0)
            //    firstRealDate = firstRealDate.AddDays(1);

            DateTime lastRealDate = new DateTime(lastDate.Year, lastDate.Month, lastDate.Day, 17, 0, 0); // Exactly 5PM
            //if (workEmpStatus.ToUpper().IndexOf("PARTTIME") >= 0)
            //    lastRealDate = lastRealDate.AddDays(1);

            DateTime testDate = DateTime.Now;
            DateTime testDate2 = DateTime.Now;


            //string cmd = "Select * from `tc_punches_pchs` where `UTS_Added` >= '" + ldate + "' and `UTS_Added` <= '" + edate + "' ";
            string cmd = "Select * from `tc_punches_pchs` where `date` >= '" + firstDate.ToString("yyyyMMdd") + "' AND `date` <= '" + lastDate.ToString("yyyyMMdd") + "' ";
            if (!String.IsNullOrWhiteSpace(employee))
                cmd += " and `empy!AccountingID` = '" + employee + "' ";

            cmd += "order by `empy!AccountingID`,`date`;";

            DataTable dx = G1.get_db_data(cmd);
            //dx.Columns.Add("date");
            dx.Columns.Add("day");
            dx.Columns.Add("week");
            dx.Columns.Add("hours", Type.GetType("System.Double"));
            dx.Columns.Add("week1", Type.GetType("System.Double"));
            dx.Columns.Add("week2", Type.GetType("System.Double"));
            dx.Columns.Add("total", Type.GetType("System.Double"));
            dx.Columns.Add("in1");
            dx.Columns.Add("in2");
            dx.Columns.Add("in3");
            dx.Columns.Add("in4");
            dx.Columns.Add("in5");
            dx.Columns.Add("out1");
            dx.Columns.Add("out2");
            dx.Columns.Add("out3");
            dx.Columns.Add("out4");
            dx.Columns.Add("out5");
            dx.Columns.Add("pto", Type.GetType("System.Double"));
            dx.Columns.Add("qpto", Type.GetType("System.Double"));
            dx.Columns.Add("holiday1", Type.GetType("System.Double"));
            dx.Columns.Add("holiday2", Type.GetType("System.Double"));
            //dx.Columns.Add("notes");
            dx.Columns.Add("cyclenotes");
            dx.Columns.Add("mod");
            dx.Columns.Add("timeIn1");
            dx.Columns.Add("timeOut1");

            int slot = 0;

            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    bool deleted = dx.Rows[i]["Deleted"].ObjToBool();
            //    if (deleted)
            //        continue;
            //    bool manual = dx.Rows[i]["ManualEntry"].ObjToBool();
            //    string empno = dx.Rows[i]["empy!AccountingID"].ObjToString();
            //    ldate = dx.Rows[i]["UTS_Added"].ObjToInt64();
            //    ldate = ConvertUTS(ldate); // Trim Seconds
            //    DateTime date = ldate.UnixToDateTime();
            //    dx.Rows[i]["date"] = date.ToString("MM/dd/yyyy hh:mm:ss");
            //}

            dx.Columns.Add("Stat");
            long oldtime = 0L;
            long firsttime = 0L;
            long lasttime = 0L;
            string oldempno = "";
            int oldday = 0;
            bool newemp = true;
            bool newdate = true;
            string status = "in";
            DateTime lastDtime;
            int count = 0;
            string dow = "";
            double vacation = 0D;
            double holiday = 0D;
            double sick = 0D;
            double other = 0D;

            bool holidayAdded = AddHolidayPay(timDt, saveFirstDate, saveLastDate, parttime);
            //if (holidayAdded)
            //    bandholiday.Visible = true;

            long lastTime = 0L;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;

            double week1 = 0D;
            double week2 = 0D;
            string punchType = "";
            double dValue = 0D;
            double hours = 0D;
            string notes = "";

            DataTable contractDt = dx.Clone();
            DataTable dupDt = dx.Clone();
            DataTable otherDt = dx.Clone();

            DateTime newDate = DateTime.Now;
            DateTime cutoffDate = DateTime.Now;
            TimeSpan ts;

            string str1 = "";
            string str2 = "";
            DateTime nextDate = DateTime.Now;

            DateTime midnight = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            for ( int i=0; i<timDt.Rows.Count; i++)
            {
                timDt.Rows[i]["week1"] = 0D;
                timDt.Rows[i]["week2"] = 0D;
            }
            try
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    bool deleted = dx.Rows[i]["Deleted"].ObjToBool();
                    if (deleted)
                        continue;

                    date = dx.Rows[i]["date"].ObjToDateTime();
                    //date = date.AddDays(1);


                    cutoffDate = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0);
                    nextDate = new DateTime(date.Year, date.Month, date.Day, 17, 0, 5);

                    date1 = dx.Rows[i]["timeIn"].ObjToDateTime();
                    date2 = dx.Rows[i]["timeOut"].ObjToDateTime();

                    if ( i == 11 || i == 12 )
                    {
                    }

                    testDate = new DateTime(date.Year, date.Month, date.Day, date1.Hour, date1.Minute, date1.Second);
                    date2 = new DateTime(date.Year, date.Month, date.Day, date2.Hour, date2.Minute, date2.Second);
                    if (workEmpStatus.ToUpper().IndexOf("PARTTIME") >= 0)
                    {
                        if (date2 > lastRealDate)
                            continue;
                    }
                    else
                    {
                        //    testDate = testDate.AddDays(-1);
                        //testDate = testDate.AddDays(-1);
                        if (testDate < firstRealDate)
                            continue;
                        if (testDate > lastRealDate)
                            continue;
                    }

                    testDate2 = new DateTime(date.Year, date.Month, date.Day, date2.Hour, date2.Minute, date2.Second);
                    //if (workEmpStatus.ToUpper().IndexOf("PARTTIME") >= 0)
                    //    testDate2 = testDate2.AddDays(-1);
                    //testDate = testDate.AddDays(-1);
                    if (testDate2 < firstRealDate)
                        continue;
                    if (testDate2 > lastRealDate)
                        continue;

                    vacation = dx.Rows[i]["vacation"].ObjToDouble();
                    holiday = dx.Rows[i]["holiday"].ObjToDouble();
                    if ( vacation != 0D || holiday != 0D )
                    {
                    }
                    //sick = dx.Rows[i]["sick"].ObjToDouble();
                    //other = dx.Rows[i]["other"].ObjToDouble();

                    punchType = dx.Rows[i]["punchType"].ObjToString();
                    if (punchType.ToUpper() == "OTHER")
                    {
                        date1 = date.AddHours(13);
                        date2 = date.AddHours(13);
                    }

                    newDate = new DateTime(date.Year, date.Month, date.Day, date1.Hour, date1.Minute, 0);
                    if (newDate < cutoffDate)
                    {
                        //date = date.AddDays(1);
                    }
                    //else if (testDate >= nextDate)
                    //    date = date.AddDays(1);


                    punchType = dx.Rows[i]["punchType"].ObjToString();
                    if (punchType.Trim().ToUpper() == "CONTRACT")
                    {
                        dupDt.ImportRow(dx.Rows[i]);
                        gotContract = true;
                        continue;
                    }
                    if (punchType.Trim().ToUpper() == "OTHER")
                    {
                        otherDt.ImportRow(dx.Rows[i]);
                        gotOther = true;
                        continue;
                    }

                    vacation = dx.Rows[i]["vacation"].ObjToDouble();
                    holiday = dx.Rows[i]["holiday"].ObjToDouble();
                    sick = dx.Rows[i]["sick"].ObjToDouble();
                    other = dx.Rows[i]["other"].ObjToDouble();
                    notes = dx.Rows[i]["notes"].ObjToString();
                    //str = formatTime(dx.Rows[i]["timeIn"].ObjToDateTime());
                    //if (str == "00:00")
                    //    continue;

                    bool manual = dx.Rows[i]["ManualEntry"].ObjToBool();
                    string empno = dx.Rows[i]["empy!AccountingID"].ObjToString();
                    slot = dx.Rows[i]["slot"].ObjToInt32();
                    if ( slot == 1 )
                    {
                    }
                    //date = dx.Rows[i]["date"].ObjToDateTime();
                    dow = date.DayOfWeek.ToString();


                    //int row = LocateDateRow(timDt, date);
                    int row = LocatePunchRow(timDt, date);
                    if (row < 0)
                        continue;
                    if (row == 0)
                    {
                    }

                    if (vacation != 0D)
                    {
                        dValue = timDt.Rows[row]["vacation"].ObjToDouble();
                        dValue += vacation;
                        timDt.Rows[row]["vacation"] = dValue;
                    }
                    if (holiday != 0D)
                    {
                        dValue = timDt.Rows[row]["holiday"].ObjToDouble();
                        if (dValue <= 0D )
                        {
                            dValue += holiday;
                            timDt.Rows[row]["holiday"] = dValue;
                        }
                    }
                    if (sick != 0D)
                    {
                        dValue = timDt.Rows[row]["sick"].ObjToDouble();
                        dValue += sick;
                        timDt.Rows[row]["sick"] = dValue;
                    }
                    if (other != 0D)
                    {
                        dValue = timDt.Rows[row]["other"].ObjToDouble();
                        dValue += other;
                        //if ( G1.isHR() )
                            timDt.Rows[row]["other"] = dValue;
                    }

                    if ( !String.IsNullOrWhiteSpace ( notes ))
                    {
                        timDt.Rows[row]["notes"] = notes;
                    }

                    str = formatTime(dx.Rows[i]["timeIn"].ObjToDateTime());
                    if (str == "00:00")
                    {
                        if ( vacation == 0D && holiday == 0D && sick == 0D && other == 0D )
                            continue;
                    }

                    gotTime = true;

                    str1 = dx.Rows[i]["timeIn"].ObjToString();
                    str2 = dx.Rows[i]["timeOut"].ObjToString();

                    if (str1 == "00:00:00" && str2 != "00:00:00")
                        str1 = "Midnight";
                    else if (str2 == "00:00:00" && str1 != "00:00:00")
                        str2 = "Midnight";

                    if (str1 == "Midnight")
                        timDt.Rows[row]["in" + slot.ToString()] = str1;
                    else
                        timDt.Rows[row]["in" + slot.ToString()] = formatTime(dx.Rows[i]["timeIn"].ObjToDateTime());
                    if (str2 == "Midnight")
                        timDt.Rows[row]["out" + slot.ToString()] = "Midnight";
                    else
                        timDt.Rows[row]["out" + slot.ToString()] = formatTime(dx.Rows[i]["timeOut"].ObjToDateTime());
                    timDt.Rows[row]["empno"] = empno;
                    date1 = dx.Rows[i]["timeIn"].ObjToDateTime();
                    date2 = dx.Rows[i]["timeOut"].ObjToDateTime();
                    if (date1 == date2)
                        continue;
                    //date1 = ValidateTime(date1);
                    //date2 = ValidateTime(date2);
                    //if ( date2 > date1 )
                    //{
                    if (row <= 6)
                    {
                        if ( row == 0 )
                        {
                        }
                        week1 = timDt.Rows[row]["week1"].ObjToDouble();
                        ts = date2 - date1;
                        hours = CalculateTime(date1, date2);
                        if ((week1+hours) >= 12D)
                        {
                        }
                        week1 += hours;
                        //week1 += ts.TotalHours;
                        timDt.Rows[row]["week1"] = week1;
                        timDt.Rows[row]["hours"] = week1;
                    }
                    else
                    {
                        week2 = timDt.Rows[row]["week2"].ObjToDouble();
                        ts = date2 - date1;
                        week2 += CalculateTime(date1, date2);
                        //week2 += ts.TotalHours;
                        timDt.Rows[row]["week2"] = week2;
                        timDt.Rows[row]["hours"] = week2;
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            dupDt = CalcContractPay(dupDt);

            G1.NumberDataTable(dupDt);
            dgv7.DataSource = dupDt;
            gridMain7.ExpandAllGroups();
            gridMain7.RefreshData();
            dgv7.Refresh();

            otherDt = CalcOtherPay(otherDt);
            G1.NumberDataTable(otherDt);
            dgv8.DataSource = otherDt;

            gridMain8.ExpandAllGroups();
            gridMain8.RefreshData();
            dgv8.Refresh();

            double total = 0D;
            hours = 0D;
            dow = "";
            week1 = 0D;
            week2 = 0D;

            if (salaried == "S")
            {
                cmd = "Select * from `tc_salarylog_salg` where `UnixTime` >= '" + saveLdate + "' and `UnixTime` <= '" + saveEdate + "' ";
                cmd += " and `empy!AccountingID` = '" + employee + "' ";
                cmd += "order by `empy!AccountingID`,`UnixTime`;";
                DataTable dd = G1.get_db_data(cmd);

                total = 0D;
                double punched = 0D;
                double Worked = 0D;
                double Other = 0D;
                double Vacation = 0D;
                double Holiday = 0D;
                double Sick = 0D;
                int row = 0;
                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    emp = "";
                    try
                    {
                        emp = dd.Rows[i]["empy!AccountingID"].ObjToString();
                        ldate = dd.Rows[i]["UnixTime"].ObjToInt64();
                        date = ldate.UnixToDateTime();
                        dow = date.DayOfWeek.ToString();
                        total = 0D;
                        week1 = 0D;
                        week2 = 0D;
                        oldempno = emp;
                        double expectedHours = GetExpectedHours(emp);
                        DecodeSalaryRow(dd, i, expectedHours, ref Worked, ref Vacation, ref Holiday, ref Sick, ref Other);

                        total = Worked + Other;

                        DateTime localTime = ldate.UnixToDateTime();
                        ts = localTime - saveFirstDate;

                        if (ts.TotalDays < 7)
                        {
                            week1 += RoundValue(total);
                            row = ts.TotalDays.ObjToInt32();
                        }
                        else
                        {
                            week2 += RoundValue(total);
                            row = ts.TotalDays.ObjToInt32();
                        }

                        punched = timDt.Rows[row]["hours"].ObjToDouble();
                        if (punched > 0D)
                            total += punched;

                        timDt.Rows[row]["part"] = "";
                        timDt.Rows[row]["worked"] = "";
                        //timDt.Rows[row]["hours"] = RoundValue(total);
                        if (total == 8D)
                            timDt.Rows[row]["full"] = "Y";
                        timDt.Rows[row]["empno"] = emp;
                    }
                    catch (Exception ex)
                    {
                    }
                }

            }

            total = 0D;
            hours = 0D;
            dow = "";
            week1 = 0D;
            week2 = 0D;
            oldempno = "";
            int weekCount = 0;
            double holiday1 = 0D;
            double holiday2 = 0D;
            holiday = 0D;
            vacation = 0D;
            sick = 0D;
            for (int i = 0; i < timDt.Rows.Count; i++)
            {
                dow = timDt.Rows[i]["day"].ObjToString();
                holiday1 = timDt.Rows[i]["holiday1"].ObjToDouble();
                holiday2 = timDt.Rows[i]["holiday2"].ObjToDouble();
                holiday = timDt.Rows[i]["holiday"].ObjToDouble();
                vacation = timDt.Rows[i]["vacation"].ObjToDouble();
                sick = timDt.Rows[i]["sick"].ObjToDouble();
                //hours = timDt.Rows[i]["hours"].ObjToDouble() + holiday + vacation + sick;
                hours = timDt.Rows[i]["hours"].ObjToDouble();
                timDt.Rows[i]["hours"] = RoundValue(hours);
                //total += hours.ObjToDouble();
                total = hours.ObjToDouble();
                timDt.Rows[i]["total"] = RoundValue(total);
                if (dow.Trim().ToUpper() == "FRIDAY")
                    weekCount++;
                if (weekCount == 1)
                {
                    week1 += hours;
                    timDt.Rows[i]["week1"] = RoundValue(week1);
                }
                else if (weekCount > 1)
                {
                    week2 += hours;
                    timDt.Rows[i]["week2"] = RoundValue(week2);
                }
                timDt.Rows[i]["empno"] = employee;
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                bool deleted = dx.Rows[i]["Deleted"].ObjToBool();
                if (deleted)
                    continue;
                bool manual = dx.Rows[i]["ManualEntry"].ObjToBool();
                string empno = dx.Rows[i]["empy!AccountingID"].ObjToString();
                if (empno != oldempno)
                {
                    if (i > 0)
                        dx.Rows[i - 1]["total"] = RoundValue(total);
                    oldempno = empno;
                    total = 0D;
                }
                else
                {
                    hours = dx.Rows[i]["hours"].ObjToDouble();
                    total += hours;
                }
            }

            if (dx.Rows.Count > 0)
                dx.Rows[dx.Rows.Count - 1]["total"] = RoundValue(total);

            oldtime = 0L;
            firsttime = 0L;
            lasttime = 0L;
            oldempno = "";
            oldday = 0;
            newemp = true;
            newdate = true;

            dx.AcceptChanges();
            if (!String.IsNullOrWhiteSpace(employee))
            {
                //                dgv.DataSource = dx;
                //                dgv.DataSource = timDt;
                //                ShowBands();
                //                bandPunch2.Visible = true;
                //                SelectCurrentDay();
                //                return;
            }

            CleanupAllColumns(timDt);
            bool foundErrors = false;
            if (!reporting)
                foundErrors = CheckForErrors(timDt);
            if (groupDt != null && !reporting)
            {
                if (timDt.Rows.Count > 0)
                {
                    int lastRow = groupDt.Rows.Count - 1;
                    if (SavedRow >= 0)
                    {
                        lastRow = SavedRow;
                        lastRow = GetDataTableRow(groupDt, emp);
                    }
                    if (lastRow >= 0)
                    {
                        groupDt.Rows[lastRow]["week1"] = RoundValue(week1);
                        groupDt.Rows[lastRow]["week2"] = RoundValue(week2);
                        groupDt.Rows[lastRow]["total"] = RoundValue(total);
                    }
                    btnError.Visible = false;
                }
                gridMain.Columns["date"].Visible = false;
                gridMain.Columns["day"].Visible = false;
                gridMain.Columns["name"].Visible = true;
                gridMain.Columns["empno"].Visible = true;
                CleanupAllColumns(groupDt);
                if (SavedRow >= 0)
                {
                    int tempRow = GetDataTableRow(groupDt, emp);
                    if (tempRow < 0)
                        tempRow = SavedRow;
                    groupDt.Rows[tempRow]["ERROR"] = "";
                    if (foundErrors)
                    {
                        groupDt.Rows[tempRow]["ERROR"] = "YES";
                        //btnError.Visible = true;
                    }
                }
                dgv.DataSource = groupDt;
                timDt = groupDt;
                return null;
            }
            gridMain.Columns["overtime"].Visible = false;
            //gridMain.Columns["notes"].Visible = false;

            timDt = AddUpOther(timDt);

            if (G1.get_column_number(timDt, "newDate") < 0)
                timDt.Columns.Add("newDate");
            date = DateTime.Now;
            for (int i = 0; i < timDt.Rows.Count; i++)
            {
                date = timDt.Rows[i]["date"].ObjToDateTime();
                date = date.AddDays(1);
                timDt.Rows[i]["newDate"] = date;
            }

            gridMain.Columns["date"].Visible = false;

            LoadNoteDates(timDt);

            dgv.DataSource = timDt;


            SetCheckBoxes(employee, saveLdate, saveEdate, salaried);
            ShowBands();
            SelectCurrentDay();
            DateTime now = DateTime.Now;
            int crow = LocatePunchRow(timDt, now);
            if (crow >= 0 && crow < timDt.Rows.Count)
            {
                gridMain.FocusedRowHandle = crow;
                gridMain.SelectRow(crow);
            }
            if (!is_supervisor || employee == "xyzzy") // This is just to catch Someone else
                gridNotes.Visible = false;
            else
            {
                gridNotes.Visible = true;
                SetNotes(saveLdate, saveEdate, employee);
            }

            gridMain.Columns["approve"].Visible = false;
            gridMain.Columns["cyclenotes"].Visible = false;

            tabControl1.TabPages.Remove(tabReport);
            tabControl1.TabPages.Remove(tabPTO);
            tabControl1.TabPages.Remove(tabDetail);

            //for (int i = 0; i < tabControl1.TabPages.Count; i++)
            //{
            //    if (tabControl1.TabPages[i].Name.ToUpper() == "TABPTO")
            //        tabControl1.TabPages.RemoveAt(i);
            //}

            SetPunchButtonColor(DateTime.Now, salaried);

            //CheckTimeApprovedBy(employee);

            timer1_Tick(null, null);

            gridMain.Columns["cyclenotes"].Visible = false;
            DetermineNotesDisplayWidth();

            int week = 0;
            for (int i = 0; i < timDt.Rows.Count; i++)
            {
                dow = timDt.Rows[i]["day"].ObjToString().ToUpper();
                if (dow == "FRIDAY")
                    week++;
                timDt.Rows[i]["week"] = week.ToString();
            }

            CheckApprovalsIn();

            return timDt;
        }
        /***********************************************************************************************/
        public static double CalculateTime(DateTime timeIn, DateTime timeOut)
        {
            if (timeOut == timeIn)
                return 0D;
            if (timeOut < timeIn)
                timeOut = timeOut.AddDays(1);
            TimeSpan ts = timeOut - timeIn;
            double hours = ts.TotalHours;
            hours = G1.RoundValue(hours);
            return hours;
        }
        /***********************************************************************************************/
        public static DateTime ValidateTime(DateTime timeIn)
        {
            //DateTime newTime = new DateTime(timeIn.Year, timeIn.Month, timeIn.Day);
            //TimeSpan ts = timeIn - newTime;
            //if (ts.TotalHours < 12)
            //    timeIn = timeIn.AddDays(1);
            return timeIn;
        }
        /***********************************************************************************************/
        private DataTable CalcContractPay(DataTable dt)
        {
            if (G1.get_column_number(dt, "paymentAmount") < 0)
                dt.Columns.Add("paymentAmount", Type.GetType("System.Double"));
            if (dt.Rows.Count <= 0)
                return dt;
            double rate = 0D;
            double hours = 0D;
            double totalPay = 0D;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime date = DateTime.Now;
            DateTime firstDate = this.dateTimePicker1.Value;
            DateTime lastDate = this.dateTimePicker2.Value;

            DateTime midDate = firstDate.AddDays(7);

            DateTime cutoffDate = DateTime.Now;
            DateTime newDate = DateTime.Now;

            TimeSpan ts;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["date"].ObjToDateTime();

                cutoffDate = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0);

                date1 = dt.Rows[i]["timeIn"].ObjToDateTime();
                date2 = dt.Rows[i]["timeOut"].ObjToDateTime();

                newDate = new DateTime(date.Year, date.Month, date.Day, date1.Hour, date1.Minute, 0);

                ts = date - firstDate;
                if (newDate <= midDate)
                {
                    if (ts.TotalDays <= 8)
                    {
                        if (ts.TotalDays == 8 && newDate >= cutoffDate)
                            dt.Rows[i]["week"] = "2";
                        else
                            dt.Rows[i]["week"] = "1";
                    }
                }
                else
                {
                    if (ts.TotalDays == 7 && newDate >= cutoffDate)
                        dt.Rows[i]["week"] = "2";
                    else if (ts.TotalDays == 7 && newDate <= cutoffDate)
                        dt.Rows[i]["week"] = "1";
                    else
                        dt.Rows[i]["week"] = "2";
                    //                    dt.Rows[i]["week"] = "2";
                }
                date1 = dt.Rows[i]["timeIn"].ObjToDateTime();
                dt.Rows[i]["timeIn1"] = date1.ToString("HH:mm");
                date2 = dt.Rows[i]["timeOut"].ObjToDateTime();
                dt.Rows[i]["timeOut1"] = date2.ToString("HH:mm");
                rate = dt.Rows[i]["rate"].ObjToDouble();

                ts = date2 - date1;
                hours = ts.TotalHours;
                hours = CalculateHours(date, dt.Rows[i]["timeIn"].ObjToString(), dt.Rows[i]["timeOut"].ObjToString());
                totalPay = hours * rate;
                dt.Rows[i]["paymentAmount"] = totalPay;
                dt.Rows[i]["hours"] = hours;
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable CalcOtherPay(DataTable dt)
        {
            if (G1.get_column_number(dt, "paymentAmount") < 0)
                dt.Columns.Add("paymentAmount", Type.GetType("System.Double"));
            double rate = 0D;
            double hours = 0D;
            double totalPay = 0D;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime date = DateTime.Now;
            DateTime firstDate = this.dateTimePicker1.Value;
            DateTime lastDate = this.dateTimePicker2.Value;

            TimeSpan ts;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["date"].ObjToDateTime();
                ts = date - firstDate;
                if (ts.TotalDays < 7)
                    dt.Rows[i]["week"] = "1";
                else
                    dt.Rows[i]["week"] = "2";
                //date1 = dt.Rows[i]["timeIn"].ObjToDateTime();
                //dt.Rows[i]["timeIn1"] = date1.ToString("HH:mm");
                //date2 = dt.Rows[i]["timeOut"].ObjToDateTime();
                //dt.Rows[i]["timeOut1"] = date2.ToString("HH:mm");
                rate = dt.Rows[i]["rate"].ObjToDouble();

                //ts = date2 - date1;
                //hours = ts.TotalHours;
                //totalPay = hours * rate;
                dt.Rows[i]["paymentAmount"] = rate;
                dt.Rows[i]["hours"] = hours;
            }
            return dt;
        }
        /***********************************************************************************************/
        private void DetermineNotesDisplayWidth()
        {
            int width = 0;
            int totalWidth = 0;
            for (int i = 0; i < gridMain.Bands.Count; i++)
            {
                if (gridMain.Bands[i].Visible)
                {
                    for (int j = 0; j < gridMain.Bands[i].Columns.Count; j++)
                    {
                        if (gridMain.Bands[i].Columns[j].Name.ToUpper() == "NOTES")
                            continue;
                        if (gridMain.Bands[i].Columns[j].Visible)
                            totalWidth += gridMain.Bands[i].Columns[j].VisibleWidth;
                    }
                }
            }
            width = this.Width - totalWidth;
            if (width > 10)
            {
                if (!workPrintOnly)
                {
                    gridMain.Bands["gridNotes"].Width = width;
                    gridMain.Columns["notes"].Width = width;
                }
            }
        }
        /***********************************************************************************************/
        private void CheckTimeApprovedBy(string empno)
        {
            this.lblTimeApprovedBy.Hide();
            if (string.IsNullOrWhiteSpace(empno))
                return;
            this.lblTimeApprovedBy.Text = "TIME NOT APPROVED!";
            this.lblTimeApprovedBy.ForeColor = Color.Red;
            this.lblTimeApprovedBy.Show();
            int CycleOffset = DetermineCycle();
            DateTime timePeriod = dateTimePicker1.Value;
            long ldate = G1.TimeToUnix(timePeriod);
            string cmd = "SELECT * from `tc_signoffs_sgno` WHERE `CycleOffset` = '" + CycleOffset.ToString() + "' and `empy!AccountingID` ='" + empno + "';";
            DataTable dd = G1.get_db_data(cmd);
            if (dd == null)
                return;
            if (dd.Rows.Count <= 0)
                return;
            string managerId = dd.Rows[0]["empy!ID_Manager"].ObjToString();
            cmd = "SELECT * from `er` where empno = '" + managerId + "';";
            dd = G1.get_db_data(cmd);
            if (dd == null)
                return;
            if (dd.Rows.Count <= 0)
                return;
            string approved = "Time Approved by : " + dd.Rows[0]["name"].ObjToString();
            this.lblTimeApprovedBy.ForeColor = Color.Green;
            this.lblTimeApprovedBy.Text = approved;
        }
        /***********************************************************************************************/
        private void ReCalcPto(DataRow dRow)
        {
            double available = 0D;
            double december = 0D;
            double expectedHours = 0D;
            string parttime = "";
            double week1 = 0D;
            double week2 = 0D;
            double total = 0D;
            double Pto = 0D;
            double tPto = 0D;
            double Docked = 0D;
            double tDocked = 0D;
            double Overtime = 0D;
            double tOvertime = 0D;
            string allow = "";

            try
            {
                dRow["overtime"] = 0D;
                string emp = dRow["empno"].ObjToString();
                if (emp == "337")
                {
                }
                week1 = dRow["week1"].ObjToDouble();
                week2 = dRow["week2"].ObjToDouble();
                total = dRow["total"].ObjToDouble();
                available = dRow["available"].ObjToDouble();
                december = dRow["december"].ObjToDouble();
                expectedHours = dRow["expectedhours"].ObjToDouble();
                parttime = dRow["parttime"].ObjToString();
                allow = dRow["allowpto"].ObjToString();

                if (expectedHours >= 84)
                {
                    CalcPtoDockeOvertime(week1, week2, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                    tPto = Pto;
                    tDocked = Docked;
                    tOvertime = Overtime;
                }
                else
                {
                    CalcPtoDockeOvertime(week1, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                    tPto = Pto;
                    tDocked = Docked;
                    tOvertime = Overtime;

                    CalcPtoDockeOvertime(week2, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                    tPto += Pto;
                    tDocked += Docked;
                    tOvertime += Overtime;
                }

                dRow["overtime"] = RoundValue(tOvertime);
                if (allow.ToUpper() == "NO")
                {
                    tDocked += tPto;
                    tPto = 0D;
                }
                dRow["qpto"] = RoundValue(tPto);
                dRow["qdocked"] = RoundValue(tDocked);
                dRow["total"] = RoundValue(week1 + week2 + tPto);
                //                dgv.DataSource = timDt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Loading PTO" + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void ReCalcPto(DataTable timDt, int row = -1)
        {
            double available = 0D;
            double december = 0D;
            double expectedHours = 0D;
            string parttime = "";
            double week1 = 0D;
            double week2 = 0D;
            double total = 0D;
            double Pto = 0D;
            double tPto = 0D;
            double Docked = 0D;
            double tDocked = 0D;
            double Overtime = 0D;
            double tOvertime = 0D;

            try
            {
                int begin = 0;
                int end = timDt.Rows.Count;
                if (row >= 0)
                {
                    begin = row;
                    end = row;
                }
                for (int i = begin; i <= end; i++)
                {
                    timDt.Rows[i]["overtime"] = 0D;
                    string emp = timDt.Rows[i]["empno"].ObjToString();
                    if (emp == "337")
                    {
                    }
                    week1 = timDt.Rows[i]["week1"].ObjToDouble();
                    week2 = timDt.Rows[i]["week2"].ObjToDouble();
                    total = timDt.Rows[i]["total"].ObjToDouble();
                    available = timDt.Rows[i]["available"].ObjToDouble();
                    december = timDt.Rows[i]["december"].ObjToDouble();
                    expectedHours = timDt.Rows[i]["expectedhours"].ObjToDouble();
                    parttime = timDt.Rows[i]["parttime"].ObjToString();
                    string allow = timDt.Rows[i]["allowpto"].ObjToString();

                    if (expectedHours >= 84)
                    {
                        CalcPtoDockeOvertime(week1, week2, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                        tPto = Pto;
                        tDocked = Docked;
                        tOvertime = Overtime;
                    }
                    else
                    {
                        CalcPtoDockeOvertime(week1, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                        tPto = Pto;
                        tDocked = Docked;
                        tOvertime = Overtime;

                        CalcPtoDockeOvertime(week2, available, december, expectedHours, parttime, ref Pto, ref Docked, ref Overtime);
                        tPto += Pto;
                        tDocked += Docked;
                        tOvertime += Overtime;
                    }

                    if (allow.ToUpper() == "NO")
                    {
                        tDocked += tPto;
                        tPto = 0D;
                    }
                    timDt.Rows[i]["overtime"] = RoundValue(tOvertime);
                    timDt.Rows[i]["qpto"] = RoundValue(tPto);
                    timDt.Rows[i]["qdocked"] = RoundValue(tDocked);
                    timDt.Rows[i]["total"] = RoundValue(week1 + week2 + tPto);
                }
                //                dgv.DataSource = timDt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Loading PTO" + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void CleanOutEmployeeTimes(DataTable dt)
        {
            double zero = 0D;
            try
            {
                for (int i = 0; i < 14; i++)
                {
                    dt.Rows[i]["hours"] = zero.ToString("###,###.00");
                    dt.Rows[i]["week1"] = zero.ToString("###,###.00");
                    dt.Rows[i]["week2"] = zero.ToString("###,###.00");
                    dt.Rows[i]["total"] = zero.ToString("###,###.00");
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void ReloadIndividualGrid(DataTable dt, string empno)
        { // Flicker Free Reload for Salary Individuals
            string full = "";
            double expectedHours = GetExpectedHours(empno);

            double dailyHours = 8D;
            if (expectedHours >= 84D)
                dailyHours = 12D;

            double hours = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double worked = 0D;
            double total = 0D;
            double punchedHours = 0D;
            double holiday1 = 0D;
            double holiday2 = 0D;
            int weekCount = 0;
            string dow = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                hours = 0D;
                full = dt.Rows[i]["full"].ObjToString();
                if (full == "Y")
                    hours = dailyHours;
                worked = dt.Rows[i]["worked"].ObjToDouble();
                punchedHours = TotalRow(dt, i);
                hours += worked + punchedHours;
                dt.Rows[i]["hours"] = hours.ToString("###,###.00");

                dow = dt.Rows[i]["day"].ObjToString();

                holiday1 = dt.Rows[i]["holiday1"].ObjToDouble();
                holiday2 = dt.Rows[i]["holiday2"].ObjToDouble();

                total += hours.ObjToDouble();
                dt.Rows[i]["total"] = RoundValue(total);
                if (dow.Trim().ToUpper() == "FRIDAY")
                    weekCount++;
                if (weekCount == 1)
                {
                    week1 += hours;
                    dt.Rows[i]["week1"] = RoundValue(week1);
                }
                else if (weekCount > 1)
                {
                    week2 += hours;
                    dt.Rows[i]["week2"] = RoundValue(week2);
                }
            }
            CleanupAllColumns(dt);
        }
        /***********************************************************************************************/
        private void ReCalcSalary(string employee)
        {
            DataTable timDt = (DataTable)dgv.DataSource;

            double zero = 0D;
            for (int i = 0; i < 14; i++)
            {
                timDt.Rows[i]["hours"] = zero.ToString("###,###.00");
                timDt.Rows[i]["week1"] = zero.ToString("###,###.00");
                timDt.Rows[i]["week2"] = zero.ToString("###,###.00");
                timDt.Rows[i]["total"] = zero.ToString("###,###.00");
            }

            if (!string.IsNullOrWhiteSpace(employee))
            {
                ReloadIndividualGrid(timDt, employee);
                //                GetEmployeePunches(employee);
                //                dgv.DataSource = timDt;
                return;
            }

            DateTime timePeriod = dateTimePicker1.Value;

            long ldate = G1.TimeToUnix(timePeriod);
            timePeriod = timePeriod.AddDays(7D);
            long hdate = G1.TimeToUnix(timePeriod);
            timePeriod = timePeriod.AddDays(7D);
            timePeriod = timePeriod.AddMinutes(-1); // This gets the time back to 23:59:00
            long edate = G1.TimeToUnix(timePeriod);

            DateTime firstDate = ldate.UnixToDateTime().ToLocalTime();
            DateTime lastDate = edate.UnixToDateTime().ToLocalTime(); ;

            DateTime saveFirstDate = firstDate;
            DateTime saveLastDate = lastDate;

            double total = 0D;
            double hours = 0D;
            string dow = "";
            double week1 = 0D;
            double week2 = 0D;

            string cmd = "Select * from `tc_salarylog_salg` where `UnixTime` >= '" + ldate + "' and `UnixTime` <= '" + edate + "' ";
            cmd += " and `empy!AccountingID` = '" + employee + "' ";
            cmd += "order by `empy!AccountingID`,`UnixTime`;";
            DataTable dd = G1.get_db_data(cmd);

            total = 0D;
            string emp = "";
            string oldempno = "";
            double Worked = 0D;
            double Other = 0D;
            double Vacation = 0D;
            double Holiday = 0D;
            double Sick = 0D;
            int row = 0;
            ldate = 0L;
            for (int i = 0; i < dd.Rows.Count; i++)
            {
                emp = "";
                try
                {
                    emp = dd.Rows[i]["empy!AccountingID"].ObjToString();
                    ldate = dd.Rows[i]["UnixTime"].ObjToInt64();
                    DateTime date = ldate.UnixToDateTime();
                    dow = date.DayOfWeek.ToString();
                    total = 0D;
                    week1 = 0D;
                    week2 = 0D;
                    oldempno = emp;
                    double expectedHours = GetExpectedHours(emp);
                    DecodeSalaryRow(dd, i, expectedHours, ref Worked, ref Vacation, ref Holiday, ref Sick, ref Other);

                    total = Worked + Other;

                    DateTime localTime = ldate.UnixToDateTime();
                    TimeSpan ts = localTime - saveFirstDate;

                    if (ts.TotalDays < 7)
                    {
                        week1 += RoundValue(total);
                        row = ts.TotalDays.ObjToInt32();
                    }
                    else
                    {
                        week2 += RoundValue(total);
                        row = ts.TotalDays.ObjToInt32();
                    }
                    timDt.Rows[row]["part"] = "";
                    timDt.Rows[row]["worked"] = "";
                    timDt.Rows[row]["hours"] = RoundValue(total);
                    if (total == 8D)
                        timDt.Rows[row]["full"] = "Y";
                    timDt.Rows[row]["empno"] = emp;
                }
                catch (Exception ex)
                {
                }
            }
            total = 0D;
            hours = 0D;
            dow = "";
            week1 = 0D;
            week2 = 0D;
            oldempno = "";
            int weekCount = 0;
            double holiday1 = 0D;
            double holiday2 = 0D;
            for (int i = 0; i < timDt.Rows.Count; i++)
            {
                dow = timDt.Rows[i]["day"].ObjToString();
                holiday1 = timDt.Rows[i]["holiday1"].ObjToDouble();
                holiday2 = timDt.Rows[i]["holiday2"].ObjToDouble();
                hours = timDt.Rows[i]["hours"].ObjToDouble() + holiday1 + holiday2;
                timDt.Rows[i]["hours"] = RoundValue(hours);
                total += hours.ObjToDouble();
                timDt.Rows[i]["total"] = RoundValue(total);
                if (dow.Trim().ToUpper() == "FRIDAY")
                    weekCount++;
                if (weekCount == 1)
                {
                    week1 += hours;
                    timDt.Rows[i]["week1"] = RoundValue(week1);
                }
                else if (weekCount > 1)
                {
                    week2 += hours;
                    timDt.Rows[i]["week2"] = RoundValue(week2);
                }
                timDt.Rows[i]["empno"] = employee;
            }
            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    bool deleted = dx.Rows[i]["Deleted"].ObjToBool();
            //    if (deleted)
            //        continue;
            //    bool manual = dx.Rows[i]["ManualEntry"].ObjToBool();
            //    string empno = dx.Rows[i]["empy!AccountingID"].ObjToString();
            //    if (empno != oldempno)
            //    {
            //        if (i > 0)
            //            dx.Rows[i - 1]["total"] = RoundValue(total);
            //        oldempno = empno;
            //        total = 0D;
            //    }
            //    else
            //    {
            //        hours = dx.Rows[i]["hours"].ObjToDouble();
            //        total += hours;
            //    }
            //}

            //if (dx.Rows.Count > 0)
            //    dx.Rows[dx.Rows.Count - 1]["total"] = RoundValue(total);
            CleanupAllColumns(timDt);
            CheckForErrors(timDt);
        }
        /***********************************************************************************************/
        private bool CheckForErrors(DataTable dt, bool ignoreErrorColumn = false)
        {
            btnError.Visible = false;
            bool foundError = false;
            int inCount = 0;
            int outCount = 0;
            string str = "";
            double week1 = 0D;
            double week2 = 0D;
            double total = 0D;
            if (G1.get_column_number(dt, "ERROR") < 0)
                dt.Columns.Add("ERROR");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (ignoreErrorColumn)
                    dt.Rows[i]["ERROR"] = "";
                inCount = 0;
                outCount = 0;
                for (int j = 1; j < 5; j++)
                {
                    str = "in" + j.ToString();
                    if (!string.IsNullOrWhiteSpace(dt.Rows[i][str].ObjToString()))
                        inCount++;
                    str = "out" + j.ToString();
                    if (!string.IsNullOrWhiteSpace(dt.Rows[i][str].ObjToString()))
                        outCount++;
                }
                if (inCount != outCount)
                    dt.Rows[i]["ERROR"] = "YES";
                week1 = dt.Rows[i]["week1"].ObjToDouble();
                week2 = dt.Rows[i]["week2"].ObjToDouble();
                total = dt.Rows[i]["total"].ObjToDouble();
                if (week1 == 0D && week2 == 0D && total == 0D)
                {
                    dt.Rows[i]["ERROR"] = "YES";
                    foundError = true;
                }
                if (!ignoreErrorColumn)
                {
                    //if (!string.IsNullOrWhiteSpace(dt.Rows[i]["ERROR"].ObjToString()))
                    //    btnError.Visible = true;
                }
            }
            return foundError;
        }
        /***********************************************************************************************/
        private void AddMyDate(DataTable dt)
        {
            if (G1.get_column_number(dt, "date") < 0)
                return;
            if (G1.get_column_number(dt, "mydate") < 0)
                dt.Columns.Add("mydate", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string date = dt.Rows[i]["date"].ObjToString();
                if (G1.validate_date(date))
                {
                    long days = G1.date_to_days(date);
                    dt.Rows[i]["mydate"] = days;
                }
            }
            G1.sortTable(dt, "mydate", "ASC");
        }
        /***********************************************************************************************/
        private bool AddHolidayPay(DataTable dt, DateTime FirstDate, DateTime LastDate, string parttime = "")
        {
            bool HolidayAdded = false;

            DateTime timePeriod = FirstDate.AddDays(7D);
            //            long hdate = timePeriod.ToUnix();
            long hdate = G1.TimeToUnix(timePeriod);

            //            long ldate = FirstDate.ToUnix();
            long ldate = G1.TimeToUnix(FirstDate);
            //            long edate = LastDate.ToUnix();
            long edate = G1.TimeToUnix(LastDate);

            string date1 = FirstDate.ToShortDateString();
            string date2 = LastDate.ToShortDateString();
            date1 = G1.date_to_sql(date1);
            date2 = G1.date_to_sql(date2);

            //            string cmd = "Select * from `tc_holidays_hday` where `UnixTime` >= '" + ldate + "' and `UnixTime` <= '" + edate + "' order by `UnixTime`;";
            string cmd = "Select * from `holidays` where `date` >= '" + date1 + "' and `date` <= '" + date2 + "' ;";
            DataTable hx = G1.get_db_data(cmd);
            AddMyDate(hx);

            double week1Holiday = 0D;
            double week2Holiday = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double total = 0D;
            double hours = 0D;
            bool foundHoliday = false;
            string part = "";
            try
            {
                for (int i = 0; i < hx.Rows.Count; i++)
                {
                    string isHalf = hx.Rows[i]["IsHalf"].ObjToString().ToUpper();
                    DateTime date = hx.Rows[i]["date"].ObjToDateTime();
                    //                long adate = date.ToUnix();
                    long adate = G1.TimeToUnix(date);
                    double hHours = 8D;
                    if (isHalf == "YES")
                        hHours = 4D;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if (G1.get_column_number(dt, "parttime") >= 0)
                        {
                            part = dt.Rows[j]["parttime"].ObjToString();
                            if (part == "Y")
                                continue;
                        }
                        else
                        {
                            if (parttime == "Y")
                                continue;
                        }
                        foundHoliday = false;
                        if (string.IsNullOrWhiteSpace(dt.Rows[j]["date"].ObjToString()))
                        {
                            if (date >= FirstDate && date <= LastDate)
                                foundHoliday = true;
                        }
                        else
                        {
                            DateTime dTime = dt.Rows[j]["date"].ObjToDateTime();
                            if (date == dTime)
                                foundHoliday = true;
                        }
                        if (foundHoliday)
                        {
                            if (adate > hdate)
                            {
                                week2Holiday += hHours;
                                dt.Rows[j]["holiday2"] = RoundValue(hHours);
                                dt.Rows[j]["holiday"] = RoundValue(hHours);
                                HolidayAdded = true;
                            }
                            else
                            {
                                dt.Rows[j]["holiday1"] = RoundValue(hHours);
                                dt.Rows[j]["holiday"] = RoundValue(hHours);
                                week1Holiday += hHours;
                                HolidayAdded = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            string empno = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                empno = dt.Rows[i]["empno"].ObjToString();
                if (string.IsNullOrWhiteSpace(empno))
                    continue;
                if (empno == "144")
                {
                }
                week1Holiday = dt.Rows[i]["holiday1"].ObjToDouble();
                week2Holiday = dt.Rows[i]["holiday2"].ObjToDouble();
                total = dt.Rows[i]["total"].ObjToDouble();
                hours = dt.Rows[i]["hours"].ObjToDouble();
                week1 = dt.Rows[i]["week1"].ObjToDouble();
                week2 = dt.Rows[i]["week2"].ObjToDouble();
                week1 += week1Holiday;
                week2 += week2Holiday;
                total = week1 + week2;
                hours = week1 + week2;
                dt.Rows[i]["total"] = RoundValue(total);
                dt.Rows[i]["hours"] = RoundValue(hours);
                dt.Rows[i]["week1"] = RoundValue(week1);
                dt.Rows[i]["week2"] = RoundValue(week2);
            }
            dt.AcceptChanges();
            return HolidayAdded;
        }
        /***********************************************************************************************/
        private void CleanupAllColumns(DataTable timDt)
        {
            //            DataTable timDt = (DataTable)dgv.DataSource;
            CleanupColumn(timDt, "total");
            CleanupColumn(timDt, "week1");
            CleanupColumn(timDt, "week2");
            CleanupColumn(timDt, "hours");
            CleanupColumn(timDt, "pto");
            CleanupColumn(timDt, "qpto");
            CleanupColumn(timDt, "holiday1");
            CleanupColumn(timDt, "holiday2");
            CleanupColumn(timDt, "docked");
            CleanupColumn(timDt, "qdocked");
            CleanupColumn(timDt, "paid");
            CleanupColumn(timDt, "worked");
            CleanupColumn(timDt, "part");
        }
        /***********************************************************************************************/
        private void CleanupColumn(DataTable timDt, string columnName)
        {
            if (G1.get_column_number(timDt, columnName) < 0)
                return;
            double dValue = 0D;
            //            DataTable timDt = (DataTable)dgv.DataSource;
            for (int i = 0; i < timDt.Rows.Count; i++)
            {
                dValue = timDt.Rows[i][columnName].ObjToDouble();
                //                timDt.Rows[i][columnName] = RoundValue(dValue);
                timDt.Rows[i][columnName] = RoundValue(dValue).ToString("###,###.00");
            }
        }
        /***********************************************************************************************/
        private long dt_to_days(DateTime date)
        {
            string sdate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
            if (!G1.validate_date(sdate))
                return 0L;
            return G1.date_to_days(sdate);
        }
        /***********************************************************************************************/
        private string GetEmployeeDepartment(string empno)
        {
            string department = "";
            string cmd = "Select * from `tc_employee_empy` where `LoginID` = '" + empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string id = dt.Rows[0]["dpmt!ID"].ObjToString();
                cmd = "Select * from `tc_department_dpmt` where `ID` = '" + id + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    department = dt.Rows[0]["Description"].ObjToString();
            }
            return department;
        }
        /***********************************************************************************************/
        private bool isExpected(DataTable dt, int row, int col, string punch)
        {
            if (workGroup)
                return false;

            DateTime now = DateTime.Now;
            int nrow = LocatePunchRow(dt, now);
            if (nrow < 0)
                return false;
            if (nrow != row)
                return false;

            string str = "";
            string data = "";
            for (int i = 1; i <= 5; i++)
            {
                str = "IN" + i;
                data = dt.Rows[row][str].ObjToString();
                if (string.IsNullOrWhiteSpace(data))
                {
                    if (str == punch)
                        return true;
                }
                str = "OUT" + i;
                data = dt.Rows[row][str].ObjToString();
                if (string.IsNullOrWhiteSpace(data))
                {
                    if (str == punch)
                        return true;
                }
            }
            return false;
        }
        /***********************************************************************************************/
        private void gridView2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            //            DataTable dt = (view.DataSource as DataTable);
            DataTable dt = (DataTable)(dgv.DataSource);
            int row = e.RowHandle;
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                e.DisplayText = (row + 1).ToString();
            }
            else if (e.Column.FieldName.ToString().ToUpper() == "PTO")
            {
                string str = e.DisplayText.Trim();
                double hours = str.ObjToDouble();
                if (hours > 0D)
                {
                    e.DisplayText = hours.ToString("###,###.00");
                    e.Appearance.ForeColor = Color.Red;
                }
            }
            else if (e.Column.FieldName.ToString().ToUpper() == "DOCKED")
            {
                string str = e.DisplayText.Trim();
                double hours = str.ObjToDouble();
                if (hours > 0D)
                {
                    e.DisplayText = hours.ToString("###,###.00");
                    e.Appearance.ForeColor = Color.Blue;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            //            DataTable dt = (view.DataSource as DataTable);
            DataTable dt = (DataTable)(dgv.DataSource);
            int row = e.RowHandle;
            try
            {
                if (e.Column.FieldName.ToUpper() == "NUM")
                {
                    e.DisplayText = (row + 1).ToString();
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "TOTAL")
                {
                    string str = e.DisplayText.Trim();
                    if (str.IndexOf(":") > 0)
                    {
                        double hours = TimeToDecimal(str);
                        e.DisplayText = hours.ToString("###,###.00");
                    }
                    else
                    {
                        double hours = str.ObjToDouble();
                        e.DisplayText = hours.ToString("###,###.00");
                        if (btnDecimal.Visible)
                            e.DisplayText = DecimalToTime(hours);
                    }
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "HOURS")
                {
                    string str = e.DisplayText.Trim();
                    if (str.IndexOf(":") > 0)
                    {
                        double hours = TimeToDecimal(str);
                        e.DisplayText = hours.ToString("###,###.00");
                    }
                    else
                    {
                        double hours = str.ObjToDouble();
                        e.DisplayText = hours.ToString("###,###.00");
                        if (btnDecimal.Visible)
                            e.DisplayText = DecimalToTime(hours);
                    }
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "WEEK1")
                {
                    string str = e.DisplayText.Trim();
                    if (str.IndexOf(":") > 0)
                    {
                        double hours = TimeToDecimal(str);
                        e.DisplayText = hours.ToString("###,###.00");
                    }
                    else
                    {
                        double hours = str.ObjToDouble();
                        e.DisplayText = hours.ToString("###,###.00");
                        if (btnDecimal.Visible)
                            e.DisplayText = DecimalToTime(hours);
                    }
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "WEEK2")
                {
                    string str = e.DisplayText.Trim();
                    if (str.IndexOf(":") > 0)
                    {
                        double hours = TimeToDecimal(str);
                        e.DisplayText = hours.ToString("###,###.00");
                    }
                    else
                    {
                        double hours = str.ObjToDouble();
                        e.DisplayText = hours.ToString("###,###.00");
                        if (btnDecimal.Visible)
                            e.DisplayText = DecimalToTime(hours);
                    }
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "OVERTIME")
                {
                    string str = e.DisplayText.Trim();
                    if (str.IndexOf(":") > 0)
                    {
                        double hours = TimeToDecimal(str);
                        e.DisplayText = hours.ToString("###,###.00");
                    }
                    else
                    {
                        double hours = str.ObjToDouble();
                        e.DisplayText = hours.ToString("###,###.00");
                        if (btnDecimal.Visible)
                            e.DisplayText = DecimalToTime(hours);
                    }
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "DATE")
                {
                    string date = e.DisplayText.Trim();
                    e.DisplayText = formatDate(date);
                }
                else if (e.Column.FieldName.ToString().ToUpper().IndexOf("IN") == 0)
                {
                    string time = e.DisplayText.Trim();
                    e.DisplayText = formatTime(time);
                }
                else if (e.Column.FieldName.ToString().ToUpper().IndexOf("OUT") == 0)
                {
                    string time = e.DisplayText.Trim();
                    e.DisplayText = formatTime(time);
                }
                else if (e.Column.FieldName.ToString().ToUpper().IndexOf("NAME") == 0)
                {
                    string name = e.DisplayText.Trim();
                    string error = dt.Rows[row]["ERROR"].ObjToString();
                    if (!string.IsNullOrWhiteSpace(error))
                        e.Appearance.ForeColor = Color.Red;
                }
                else if (e.Column.FieldName.ToString().ToUpper().IndexOf("DAY") == 0)
                {
                    string name = e.DisplayText.Trim();
                    string error = dt.Rows[row]["ERROR"].ObjToString();
                    if (!string.IsNullOrWhiteSpace(error))
                        e.Appearance.ForeColor = Color.Red;
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "QPTO")
                {
                    string str = e.DisplayText.Trim();
                    double hours = str.ObjToDouble();
                    if (hours > 0D)
                    {
                        e.DisplayText = hours.ToString("###,###.00");
                        e.Appearance.ForeColor = Color.Red;
                    }
                }
                else if (e.Column.FieldName.ToString().ToUpper() == "QDOCKED")
                {
                    string str = e.DisplayText.Trim();
                    double hours = str.ObjToDouble();
                    if (hours > 0D)
                    {
                        e.DisplayText = hours.ToString("###,###.00");
                        e.Appearance.ForeColor = Color.Blue;
                    }
                }
                else if (e.Column.FieldName.ToUpper() == "OTHER")
                {
                    double dValue = dt.Rows[row]["other"].ObjToDouble();
                    if (dValue == 0D)
                        e.DisplayText = "";
                    else
                    {
                        if (!G1.isHR())
                            e.DisplayText = "*.**";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private double TimeToDecimal(string str)
        {
            string[] Lines = str.Split(':');
            double value = 0D;
            int hours = Lines[0].ObjToInt32();
            int minutes = 0;
            if (Lines.Length > 1)
                minutes = Lines[1].ObjToInt32();
            value = (double)(hours) + (double)(minutes) / 60D;
            return value;
        }
        /***********************************************************************************************/
        private string DecimalToTime(double hours)
        {
            int ihours = (int)(hours);
            hours -= (double)ihours;
            int minutes = (int)(60D * hours);
            string time = ihours.ToString() + ":" + minutes.ToString("D2");
            return time;
        }
        /***********************************************************************************************/
        private void menuItemExit_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.timer1.Enabled = false;
            this.Close();
        }
        /***********************************************************************************************/
        private int DetermineRow()
        {
            DateTime now = DateTime.Now;
            DateTime start = dateTimePicker1.Value;
            long jnow = G1.date_to_days(now.Month.ToString("D2") + "/" + now.Day.ToString("D2") + "/" + now.Year.ToString("D4"));
            long jstart = G1.date_to_days(start.Month.ToString("D2") + "/" + start.Day.ToString("D2") + "/" + start.Year.ToString("D4"));
            int row = (int)(jnow - jstart);
            return row;
        }
        /***********************************************************************************************/
        private DataRow GetCurrentGridRow()
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            return dr;
        }
        /***********************************************************************************************/
        private double GetCellHours(DataRow dRow, string cell)
        {
            double value = 0D;
            try
            {
                string time = dRow[cell].ObjToString().Trim();
                if (time.ToUpper() == "MIDNIGHT")
                    time = "00:00:00";
                string[] Lines = time.Split(':');
                int hours = 0;
                int minutes = 0;
                int seconds = 0;
                if (Lines.Length > 0)
                    hours = Lines[0].Trim().ObjToInt32();
                if (Lines.Length > 1)
                    minutes = Lines[1].Trim().ObjToInt32() * 60;
                //if ( Lines.Length > 2 )
                //    seconds = Lines[2].Trim().ObjToInt32();
                value = (double)(hours) + (double)(minutes + seconds) / 3600D;
            }
            catch
            {
            }
            return value;
        }
        /***********************************************************************************************/
        private double TotalRow(DataTable dx, int row)
        {
            DataRow dRow = dx.Rows[row];
            double hours = 0D;
            for (int i = 1; i <= 5; i++)
            {
                string inName = "IN" + i.ToString();
                string outName = "OUT" + i.ToString();
                double clock_in = GetCellHours(dRow, inName);
                double clock_out = GetCellHours(dRow, outName);
                if (clock_out > 0D && clock_in > 0D)
                    hours += clock_out - clock_in;
            }
            hours = RoundValue(hours);
            return hours;
        }
        /***********************************************************************************************/
        private void TotalRow()
        {
            DataRow dRow = GetCurrentGridRow();
            double hours = 0D;
            for (int i = 1; i <= 5; i++)
            {
                GridBand bandPunch = getGridBand(i);
                if (bandPunch.Visible)
                {
                    string inName = "IN" + i.ToString();
                    string outName = "OUT" + i.ToString();
                    double clock_in = GetCellHours(dRow, inName);
                    double clock_out = GetCellHours(dRow, outName);
                    if (clock_out > 0D && clock_in > 0D)
                        hours += clock_out - clock_in;
                }
            }
            hours = RoundValue(hours);
            dRow["hours"] = hours.ToString("###,###.00");
            TotalAll();
        }
        /***********************************************************************************************/
        private void TotalAll()
        {
            double total = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double hours = 0D;
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < 14; i++)
            {
                hours = dt.Rows[i]["hours"].ObjToDouble();
                total += hours;
                //dt.Rows[i]["total"] = total.ToString("###,###.00");
                dt.Rows[i]["total"] = hours.ToString("###,###.00");
                if (i < 7)
                {
                    week1 += hours;
                    dt.Rows[i]["week1"] = week1.ToString("###,###.00");
                }
                else
                {
                    week2 += hours;
                    dt.Rows[i]["week2"] = week2.ToString("###,###.00");
                }
            }
        }
        /***********************************************************************************************/
        private string GetCurrentTime()
        {
            DateTime now = DateTime.Now;
            //            string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2") + ":" + now.Second.ToString("D2");
            string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
            return time;
        }
        /***********************************************************************************************/
        private int LocateDateRow(DataTable dt, DateTime date)
        {
            int row = 0;
            //date = date.AddDays(1);
            string datein = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
            string dateout = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DateTime d = dt.Rows[i]["date"].ObjToDateTime();
                dateout = d.Month.ToString("D2") + "/" + d.Day.ToString("D2") + "/" + d.Year.ToString("D4");
                if (datein == dateout)
                {
                    row = i;
                    break;
                }
            }
            if (row == 0)
            {

            }
            return row;
        }
        /***********************************************************************************************/
        private int LocatePunchRow(DataTable dt, DateTime now)
        {
            int row = -1;
            string sDate = now.Month.ToString("D2") + "/" + now.Day.ToString("D2") + "/" + now.Year.ToString("D4");
            string date = "";
            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime date3 = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date3 = dt.Rows[i]["date"].ObjToDateTime();
                //date3 = date3.AddDays(-1);
                date = date3.ToString("MM/dd/yyyy");
                if (date == sDate)
                {
                    row = i;
                    break;
                }
                if ( i == dt.Rows.Count - 1 )
                {
                    date1 = now;
                    date2 = dt.Rows[i]["date"].ObjToDateTime();
                    if ( date1 > date2 )
                    {
                        //row = 0;
                        break;
                    }
                }
            }
            return row;
        }
        /***********************************************************************************************/
        private bool CheckPreviousPunch(DataTable dt, int row, string ButtonName)
        {
            string punch = ButtonName.Trim().ToUpper();
            bool punchOkay = true;
            int punchCount = -1;
            bool punchIn = false;
            try
            {
                if (punch.ToUpper().IndexOf("BTNPUNCHIN") >= 0)
                {
                    punchCount = (punch.ToUpper().Replace("BTNPUNCHIN", "")).ObjToInt32();
                    punchIn = true;
                }
                else
                {
                    punchCount = (punch.ToUpper().Replace("BTNPUNCHOUT", "")).ObjToInt32();
                    punchIn = false;
                }
                if (punchIn && punchCount == 1)
                    return punchOkay;
                else if (punchIn)
                    punchCount--; // Check Previous Punch Out
                for (; ; )
                {
                    if (!punchIn)
                    {
                        string name = "IN" + punchCount.ObjToString();
                        string data = dt.Rows[row][name].ObjToString();
                        if (string.IsNullOrWhiteSpace(data))
                        {
                            if (DevExpress.XtraEditors.XtraMessageBox.Show("***WARNING*** Previous Punch has not been made!!\nDo you STILL want to Punch Out?", "Punch Out Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                                return false;
                            return true;
                        }
                        punchCount--;
                        if (punchCount <= 0)
                            break;
                        punchIn = true;
                    }
                    else
                    {
                        string name = "OUT" + punchCount.ObjToString();
                        string data = dt.Rows[row][name].ObjToString();
                        if (string.IsNullOrWhiteSpace(data))
                        {
                            if (DevExpress.XtraEditors.XtraMessageBox.Show("***WARNING*** Previous Punch has not been made!!\nDo you STILL want to Punch In?", "Punch In Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                                return false;
                            return true;
                        }
                        punchIn = false;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return punchOkay;
        }
        /***********************************************************************************************/
        private void btnPunch_Click(object sender, System.EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DateTime now = DateTime.Now;
            long ldate = G1.TimeToUnix(now);
            DateTime nnow = ldate.UnixToDateTime();
            //if (1 == 1)
            //    return;
            int row = LocatePunchRow(dt, now);
            if (row < 0)
            {
                MessageBox.Show("***ERROR*** Cannot Determine Punch Row! Call IT!");
                return;
            }
            gridMain.FocusedRowHandle = row;
            DataRow dRow = GetCurrentGridRow();
            string empno = dRow["empno"].ObjToString();
            Button button = (Button)(sender);
            string ButtonName = button.Name.ToUpper();
            bool punchOkay = CheckPreviousPunch(dt, row, ButtonName);
            if (!punchOkay)
                return;
            if (button.Name.ToUpper().IndexOf("PUNCHIN") > 0)
            {
                AddPunchNow(empno, now);
                string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
                string name = "IN" + button.Name.ToUpper().Replace("BTNPUNCHIN", "");
                if (string.IsNullOrWhiteSpace(dRow[name].ObjToString()))
                {
                    dRow[name] = time;
                    CheckForErrors(dt, true);
                }
                else
                {
                    MessageBox.Show("***ERROR*** Punch has already been make!");
                }
            }
            else if (button.Name.ToUpper().IndexOf("PUNCHOUT") > 0)
            {
                AddPunchNow(empno, now);
                string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
                string name = "OUT" + button.Name.ToUpper().Replace("BTNPUNCHOUT", "");
                if (string.IsNullOrWhiteSpace(dRow[name].ObjToString()))
                {
                    dRow[name] = time;
                    TotalRow();
                    CheckForErrors(dt, true);
                }
                else
                {
                    MessageBox.Show("***ERROR*** Punch has already been make!");
                }
            }
        }
        /***********************************************************************************************/
        private void AddPunchNow(string empno, DateTime time)
        {
            //if (1 == 1)
            //    return;
            string machine = System.Environment.MachineName.ObjToString();

            long ldate = G1.TimeToUnix(time);
            bool alreadyExists = IndividualPunches.VerifyPunchEntry(ldate, empno);
            //if (alreadyExists)
            //{
            //    MessageBox.Show("***ERROR*** Added Punch Time for Employee Already Exists! Add or Subtract a second! PUNCH NOT ADDED!");
            //    return; ;
            //}
            //            string cmd = "INSERT INTO `tc_punches_pchs` (`UTS_Added`, `empy!AccountingID`, `ManualEntry`) VALUES ('" + ldate.ToString() + "', '" + empno + "', '0' );";
            //string cmd = "INSERT INTO `tc_punches_pchs` (`UTS_Added`, `empy!AccountingID`, `ManualEntry`, `user`, `computer`) VALUES ('" + ldate.ToString() + "', '" + empno + "', '0', '" + LoginForm.username + "', '" + machine + "' );";
            //try
            //{
            //    G1.update_db_data(cmd);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            //}
        }
        /***********************************************************************************************/
        private void CalculateDecember(string empno = "")
        {
            DateTime lastposting = this.dateTimePicker1.Value;
            DateTime yearend = new DateTime(lastposting.Year, 12, 31);
            bool CalcNewYear = false;
            if (CalcNewYear)
            {
                //                string newyear = DateTime.Now.Year.ToString();
                string newyear = DateTime.Now.Year.ToString();
                if (G1.validate_numeric(newyear))
                {
                    int iyear = newyear.ObjToInt32();
                    if (iyear >= lastposting.Year)
                        yearend = new DateTime(iyear, 12, 31);
                }
            }

            DataTable dt = G1.get_db_data("Select * from `tc_er` where `username` = '" + workUserName + "';");
            if (dt.Rows.Count <= 0)
                return;

            DataTable dx = G1.get_db_data("Select * from `users` where `username` = '" + workUserName + "';");
            if (dx.Rows.Count <= 0)
                return;
            if (1 == 1)
                return;

            string name = workMyName;
            empName = name;
            string hiredate = dx.Rows[0]["hiredate"].ObjToString();

            this.Text = name + " ( Hire Date : " + hiredate + " )";

            DateTime hdate = dx.Rows[0]["hiredate"].ObjToDateTime();
            double increment = dt.Rows[0]["pto_inc"].ObjToDouble();
            string status = dt.Rows[0]["status"].ObjToString().ToUpper();
            if (status.ToUpper().IndexOf("HOLD") < 0)
            {
                double available = dt.Rows[0]["pto_now"].ObjToDouble();
                this.txtAvailablePTO.Text = "  " + available.ToString("###,###.00");
                lastposting = this.dateTimePicker1.Value;
                lastposting = GetLastPostingDate();
                for (; ; )
                {
                    lastposting = lastposting.AddDays(14D);
                    if (lastposting >= yearend)
                    {
                        lastposting = lastposting.AddDays(-14D);
                        increment = MonthlyPto(hdate, lastposting);
                        TimeSpan ts = yearend - lastposting;
                        available += (increment / 14D) * (ts.Days + 1);
                        this.txtDecemberPTO.Text = "  " + available.ToString("###,###.00");
                        break;
                    }
                    else
                    {
                        increment = MonthlyPto(hdate, lastposting);
                        available += increment;
                        this.txtDecemberPTO.Text = "  " + available.ToString("###,###.00");
                    }
                }
            }
            else
            {
                txtDecemberPTO.Text = "HOLD";
                txtAvailablePTO.Text = "HOLD";
            }
        }
        /***********************************************************************************************/
        private double CalculateYearEndDecember(string empno)
        {
            DateTime lastposting = this.dateTimePicker1.Value;
            DateTime yearend = new DateTime(lastposting.Year, 12, 31);
            bool CalcNewYear = true;
            if (CalcNewYear)
            {
                //                string newyear = DateTime.Now.Year.ToString();
                string newyear = DateTime.Now.Year.ToString();
                if (G1.validate_numeric(newyear))
                {
                    int iyear = newyear.ObjToInt32();
                    if (iyear >= lastposting.Year)
                        yearend = new DateTime(iyear, 12, 31);
                }
            }

            DataTable dt = G1.get_db_data("Select * from `users` u JOIN `tc_er` t ON u.`username` = t.`username` where u.`username` = '" + empno + "';");
            if (dt.Rows.Count <= 0)
                return 0D;

            string name = dt.Rows[0]["lastName"].ObjToString() + "," + dt.Rows[0]["firstName"].ObjToString();
            empName = name;
            string hiredate = dt.Rows[0]["hiredate"].ObjToString();

            this.Text = name + " ( Hire Date : " + hiredate + " )";

            DateTime hdate = dt.Rows[0]["hiredate"].ObjToDateTime();
            //double increment = dt.Rows[0]["pto_inc"].ObjToDouble();
            double increment = 0D;
            string status = dt.Rows[0]["status"].ObjToString().ToUpper();
            double available = 0D;
            if (status.ToUpper().IndexOf("HOLD") < 0)
            {
                //available = dt.Rows[0]["pto_now"].ObjToDouble();
                //this.txtAvailablePTO.Text = "  " + available.ToString("###,###.00");
                //lastposting = this.dateTimePicker1.Value;
                //lastposting = GetLastPostingDate();
                //for (;;)
                //{
                //    lastposting = lastposting.AddDays(14D);
                //    if (lastposting >= yearend)
                //    {
                //        lastposting = lastposting.AddDays(-14D);
                //        increment = MonthlyPto(hdate, lastposting);
                //        TimeSpan ts = yearend - lastposting;
                //        available += (increment / 14D) * (ts.Days + 1);
                //        break;
                //    }
                //    else
                //    {
                //        increment = MonthlyPto(hdate, lastposting);
                //        available += increment;
                //    }
                //}
            }
            else
            {
                //txtDecemberPTO.Text = "HOLD";
                //txtAvailablePTO.Text = "HOLD";
            }
            return available;
        }
        /***********************************************************************************************/
        private bool CalcDecember(DataTable dt, int row, ref double december, ref double available, bool calcTitle = true)
        {
            DateTime lastposting = GetLastPostingDate();
            //            DateTime lastposting = this.dateTimePicker1.Value;
            DateTime yearend = new DateTime(lastposting.Year, 12, 31);

            string empno = dt.Rows[row]["empno"].ObjToString();
            if (empno == "103")
            {
            }
            string name = dt.Rows[row]["name"].ObjToString();
            empName = name;
            string hiredate = dt.Rows[row]["hiredate"].ObjToString();

            if (calcTitle)
                this.Text = name + " ( Hire Date : " + hiredate + " )";

            DateTime hdate = dt.Rows[row]["hiredate"].ObjToDateTime();
            DateTime now = DateTime.Now;
            TimeSpan ts = now - hdate;
            if (ts.Days < 90)
                return false;
            double increment = dt.Rows[row]["pto_inc"].ObjToDouble();
            string status = dt.Rows[row]["status"].ObjToString().ToUpper();
            if (status.ToUpper().IndexOf("HOLD") < 0)
            {
                available = dt.Rows[row]["pto_now"].ObjToDouble();
                december = available;
                lastposting = this.dateTimePicker1.Value;
                lastposting = GetLastPostingDate();
                for (; ; )
                {
                    lastposting = lastposting.AddDays(14D);
                    if (lastposting >= yearend)
                    {
                        lastposting = lastposting.AddDays(-14D);
                        increment = MonthlyPto(hdate, lastposting);
                        ts = yearend - lastposting;
                        december += (increment / 14D) * (ts.Days + 1);
                        break;
                    }
                    else
                    {
                        increment = MonthlyPto(hdate, lastposting);
                        december += increment;
                    }
                }
            }
            return true;
        }
        /***********************************************************************************************/
        private double MonthlyPto(DateTime hdate, DateTime actualdate)
        {
            double increment = 0D;
            TimeSpan ts = actualdate - hdate;

            double years = (double)(ts.Days) / 365D;

            if (years <= 1D)
                increment = 10D;
            else if (years <= 5D)
                increment = 15D;
            else if (years <= 6D)
                increment = 17D;
            else if (years <= 7D)
                increment = 19D;
            else if (years <= 8D)
                increment = 21D;
            else if (years <= 9D)
                increment = 23D;
            else
            {
                increment = 25D;
                //                return 7.7D;
            }

            increment = increment * 8D / 26D + 0.005D; // hours per pay period
            increment = Math.Round(increment, 2);
            //int inc = (int) ((increment+0.005D) * 100D);
            //increment = (double)(inc) / 100D;
            return increment;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            int rowHandle = -1;
            DevExpress.XtraGrid.Columns.GridColumn column = null;
            string ColumnName = "";
            GridHitInfo hitInfo = gridMain.CalcHitInfo(savePoint);
            if (hitInfo.InRowCell)
            {
                rowHandle = hitInfo.RowHandle;
                column = hitInfo.Column;
                ColumnName = column.FieldName.Trim().ToUpper();
                if (ColumnName.Trim().ToUpper() == "FULL")
                    return; // Just in case someone double clicks on the checkbox
                if (ColumnName.Trim().ToUpper() == "APPROVE")
                    return; // Just in case someone double clicks on the checkbox
            }

            DataRow dr = gridMain.GetFocusedDataRow();
            string emp = dr["empno"].ObjToString();
            string name = dr["name"].ObjToString();
            DateTime day = dateTimePicker1.Value;
            string date = dateTimePicker1.Value.ToShortDateString();
            if (!string.IsNullOrWhiteSpace(empno))
            { // This is for correcting time punches
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.FocusedRowHandle;
                day = day.AddDays(row);
                date = day.ToShortDateString();
                emp = empno;
                SavedPunchRow = gridMain.FocusedRowHandle;
                IndividualPunches indForm = new IndividualPunches(emp, empName, date, is_supervisor);
                indForm.EditPunchesDone += IndForm_EditPunchesDone;
                indForm.Show();
                return;
            }
            string method = dr["method"].ObjToString();
            //            SavedRow = gridMain.FocusedRowHandle; // Don't use because it could be filtered
            SavedRow = dr["num"].ObjToInt32() - 1; // Actual Row In DataTable
            SavedDt = (DataTable)dgv.DataSource;
            SavedDRow = dr;
            SavedEmp = dr["empno"].ObjToString();
            TimeClock timeForm = new TimeClock(emp, date, method);
            timeForm.TimeClockDone += TimeForm_TimeClockDone;
            timeForm.ShowDialog();
            //btnClock.Hide();
        }
        /***********************************************************************************************/
        private int GetDataTableRow(DataTable dt, string empno)
        {
            int row = -1;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["empno"].ObjToString() == empno)
                    {
                        row = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return row;
        }
        /***********************************************************************************************/
        DataRow SavedDRow = null;
        DataTable SavedDt = null;
        int SavedRow = -1;
        string SavedEmp = "";
        private void TimeForm_TimeClockDone(string empNo, string workUserName)
        {
            if (!dateTimePicker2.Visible)
            {
                LoadTimePeriod();
                GetEmployeePunches(empno);
            }
            else
            {
                LoadTimePeriod();
                if (!workGroup)
                    GetEmployeePunches(empno);
                else if (SavedRow >= 0)
                {
                    DataTable dt = SavedDt;
                    string emp = SavedDRow["empno"].ObjToString();
                    SavedDRow["ERROR"] = "";
                    GetEmployeePunches(emp, dt);
                    int tempRow = GetDataTableRow(dt, emp);
                    if (tempRow < 0)
                        tempRow = SavedRow;
                    ReCalcPto(dt, tempRow);
                    DetermineErrors(dt);
                    dgv.DataSource = dt;
                    gridMain.Columns["hours"].Visible = false;
                    gridMain.Columns["picture"].Visible = true;
                    int cycle = DetermineCycle();
                    string cycleNote = GetCycleNote(emp, cycle);
                    long saveLdate = -1L;
                    long saveEdate = -1L;
                    GetSavedDate(ref saveLdate, ref saveEdate);
                    string notes = GetNoteChanges(saveLdate, saveEdate, emp);
                    if (SavedDRow != null)
                    {
                        SavedDRow["cyclenotes"] = cycleNote;
                        SavedDRow["notes"] = notes;
                    }
                    bandSalary.Visible = false;
                }
            }
            if (SavedRow >= 0)
            {
                int tempRow = SavedRow;
                if (!string.IsNullOrWhiteSpace(SavedEmp))
                {
                    tempRow = GetDataTableRow(SavedDt, SavedEmp);
                    if (tempRow < 0)
                        tempRow = SavedRow;
                }
                gridMain.FocusedRowHandle = tempRow;
                SavedRow = -1;
                SavedDt = null;
                SavedDRow = null;
            }
            //btnClock.Hide();
        }
        /***********************************************************************************************/
        private void DetermineErrors(DataTable dt)
        {
            btnError.Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["ERROR"].ObjToString().ToUpper() == "YES")
                {
                    //btnError.Visible = true;
                    break;
                }
            }
        }
        /***********************************************************************************************/
        int SavedPunchRow = -1;
        private void IndForm_EditPunchesDone()
        {
            DataTable saveDt = null;
            if (dgv.Visible)
            {
                saveDt = (DataTable)dgv.DataSource;
                dgv.DataSource = null;
            }
            LoadTimePeriod();
            GetEmployeePunches(empno);
            //if (saveDt != null)
            //    dgv.DataSource = saveDt;
            if (SavedPunchRow >= 0)
            {
                gridMain.FocusedRowHandle = SavedPunchRow;
                SavedPunchRow = -1;
            }
        }
        /***********************************************************************************************/
        private int GetCurrentRow()
        {
            int rowHandle = -1;
            DevExpress.XtraGrid.Columns.GridColumn column = null;
            string ColumnName = "";
            GridHitInfo hitInfo = gridMain.CalcHitInfo(savePoint);
            if (hitInfo.InRowCell)
            {
                rowHandle = hitInfo.RowHandle;
                column = hitInfo.Column;
                ColumnName = column.FieldName.Trim().ToUpper();
            }
            return rowHandle;
        }
        /***********************************************************************************************/
        private string GetCurrentColumn()
        {
            int rowHandle = -1;
            DevExpress.XtraGrid.Columns.GridColumn column = null;
            string ColumnName = "";
            GridHitInfo hitInfo = gridMain.CalcHitInfo(savePoint);
            if (hitInfo.InRowCell)
            {
                rowHandle = hitInfo.RowHandle;
                column = hitInfo.Column;
                ColumnName = column.FieldName.Trim().ToUpper();
            }
            return ColumnName;
        }
        /***********************************************************************************************/
        private void SetBandVisible(int band, bool visible = false)
        {
            if (workGroup)
                return;
            if (band == 1)
            {
                bandPunch1.Visible = visible;
                btnPunchIn1.Visible = visible;
                btnPunchOut1.Visible = visible;
            }
            else if (band == 2)
            {
                bandPunch2.Visible = visible;
                btnPunchIn2.Visible = visible;
                btnPunchOut2.Visible = visible;
            }
            else if (band == 3)
            {
                bandPunch3.Visible = visible;
                btnPunchIn3.Visible = visible;
                btnPunchOut3.Visible = visible;
            }
            else if (band == 4)
            {
                bandPunch4.Visible = visible;
                btnPunchIn4.Visible = visible;
                btnPunchOut4.Visible = visible;
            }
            else if (band == 5)
            {
                bandPunch5.Visible = visible;
                btnPunchIn5.Visible = visible;
                btnPunchOut5.Visible = visible;
            }
        }
        /***********************************************************************************************/
        private void ShowBands()
        {
            string str = "";
            string time = "";
            string timeout = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 2; i <= 5; i++)
                SetBandVisible(i);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    time = "in" + j.ObjToString();
                    str = dt.Rows[i][time].ObjToString();
                    if (!string.IsNullOrWhiteSpace(str))
                        SetBandVisible(j, true);
                    time = "out" + j.ObjToString();
                    timeout = dt.Rows[i][time].ObjToString();
                    if (!string.IsNullOrWhiteSpace(timeout))
                        SetBandVisible(j, true);
                }
            }
            //            gridMain.RefreshData();
            dgv.Refresh();
            this.ForceRefresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void btnAddPunch_Click(object sender, EventArgs e)
        {
            for (int i = 1; i <= 5; i++)
            {
                GridBand bandPunch = getGridBand(i);
                if (!bandPunch.Visible)
                {
                    bandPunch.Visible = true;
                    if (i == 1)
                    {
                        btnPunchIn1.Visible = true;
                        btnPunchOut1.Visible = true;
                    }
                    else if (i == 2)
                    {
                        btnPunchIn2.Visible = true;
                        btnPunchOut2.Visible = true;
                    }
                    else if (i == 3)
                    {
                        btnPunchIn3.Visible = true;
                        btnPunchOut3.Visible = true;
                    }
                    else if (i == 4)
                    {
                        btnPunchIn4.Visible = true;
                        btnPunchOut4.Visible = true;
                    }
                    else
                    {
                        btnPunchIn5.Visible = true;
                        btnPunchOut5.Visible = true;
                    }
                    break;
                }
            }
        }
        /***********************************************************************************************/
        private void ForceBtnVisible(int band)
        {
        }
        /***********************************************************************************************/
        private GridBand getGridBand(int band)
        {
            string bandName = "bandPunch" + band.ToString();
            for (int i = 0; i < gridMain.Bands.Count; i++)
            {
                GridBand bandPunch = (GridBand)(gridMain.Bands[i]);
                if (bandPunch.Name.ToUpper() == bandName.ToUpper())
                {
                    return bandPunch;
                }
            }
            return null;
        }
        /***********************************************************************************************/
        private void gridMain_RowStyle(object sender, RowStyleEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.RowHandle > 0)
            {
                string day = view.GetRowCellDisplayText(e.RowHandle, view.Columns["day"]);
                if (day != null)
                {
                    //if (day.ToUpper() == "SATURDAY" || day.ToUpper() == "SUNDAY")
                    //{
                    //    e.Appearance.BackColor = Color.Pink;
                    //    e.Appearance.BackColor2 = Color.Pink;
                    //    e.HighPriority = true;
                    //}
                }
            }
        }
        /***********************************************************************************************/
        bool dgvNormalPrinting = true;
        private void SetupPrintPage(DevExpress.XtraGrid.GridControl ddgv = null )
        {
            dgvNormalPrinting = true;
            //Printer.setupPrinterMargins(10, 10, 80, 100);
            Printer.setupPrinterMargins(10, 10, 80, 200);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            if (ddgv == null)
            {
                if (dgv.Visible)
                {
                    printableComponentLink1.Component = dgv;
                    printableComponentLink1.Landscape = true;
                    printableComponentLink1.Margins.Left = 10;
                    printableComponentLink1.Margins.Right = 10;
                    printableComponentLink1.Margins.Top = 135;
                    printableComponentLink1.Margins.Bottom = 50;

                    printableComponentLink1.CreateDocument();
                }
                else if (dgv2.Visible)
                {
                    printableComponentLink1.Component = dgv2;
                    printableComponentLink1.Landscape = true;
                    printableComponentLink1.Margins.Left = 50;
                    printableComponentLink1.Margins.Right = 50;
                    printableComponentLink1.Margins.Top = 135;
                    printableComponentLink1.Margins.Bottom = 50;

                    printableComponentLink1.CreateDocument();
                }
                else if (dgv4.Visible)
                {
                    printableComponentLink1.Component = dgv4;
                    printableComponentLink1.Landscape = false;
                    printableComponentLink1.Margins.Left = 50;
                    printableComponentLink1.Margins.Right = 50;
                    printableComponentLink1.Margins.Top = 135;
                    printableComponentLink1.Margins.Bottom = 50;

                    printableComponentLink1.CreateDocument();
                }
                else if (dgv5.Visible)
                {
                    printableComponentLink1.Component = dgv5;
                    printableComponentLink1.Landscape = true;
                    printableComponentLink1.Margins.Left = 50;
                    printableComponentLink1.Margins.Right = 50;
                    printableComponentLink1.Margins.Top = 135;
                    printableComponentLink1.Margins.Bottom = 50;

                    printableComponentLink1.CreateDocument();
                }
                else if (dgv7.Visible)
                {
                    printableComponentLink1.Component = dgv7;
                    printableComponentLink1.Landscape = true;
                    printableComponentLink1.Margins.Left = 10;
                    printableComponentLink1.Margins.Right = 10;
                    printableComponentLink1.Margins.Top = 135;
                    printableComponentLink1.Margins.Bottom = 10;

                    printingSystem1.Document.AutoFitToPagesWidth = 1;

                    printableComponentLink1.CreateDocument();
                }
                else if (dgv8.Visible)
                {
                    printableComponentLink1.Component = dgv8;
                    printableComponentLink1.Landscape = true;
                    printableComponentLink1.Margins.Left = 10;
                    printableComponentLink1.Margins.Right = 10;
                    printableComponentLink1.Margins.Top = 135;
                    printableComponentLink1.Margins.Bottom = 10;

                    printingSystem1.Document.AutoFitToPagesWidth = 1;

                    printableComponentLink1.CreateDocument();
                }
            }
            else
            {
                dgvNormalPrinting = false;
                printableComponentLink1.Component = ddgv;
                printableComponentLink1.Landscape = true;
                printableComponentLink1.Margins.Left = 50;
                printableComponentLink1.Margins.Right = 50;
                printableComponentLink1.Margins.Top = 135;
                printableComponentLink1.Margins.Bottom = 50;
                if (ddgv == dgv7 || ddgv == dgv || ddgv == dgv8 || ddgv == dgv5 )
                {
                    printableComponentLink1.Margins.Left = 10;
                    printableComponentLink1.Margins.Right = 10;
                    printableComponentLink1.Margins.Top = 135;
                    printableComponentLink1.Margins.Bottom = 10;

                    printableComponentLink1.Landscape = true;
                    printingSystem1.Document.AutoFitToPagesWidth = 1;
                }
                else
                {
                    if (rtb.Visible)
                        printableComponentLink1.Landscape = false;
                }

                printableComponentLink1.CreateDocument();
            }
        }
        /***********************************************************************************************/
        private void SetupPrintPageAll(DevExpress.XtraGrid.GridControl ddgv, ref DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1)
        {
            dgvNormalPrinting = true;
            dgvNormalPrinting = false;

            printableComponentLink1.CreateDocument();

            printableComponentLink1.Component = ddgv;
            printableComponentLink1.Landscape = true;
            printableComponentLink1.Margins.Left = 50;
            printableComponentLink1.Margins.Right = 50;
            printableComponentLink1.Margins.Top = 135;
            printableComponentLink1.Margins.Bottom = 50;
            printableComponentLink1.Landscape = false;

            printableComponentLink1.CreateDocument();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        bool printUserName = false;
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);
            //Printer.DrawQuadBorder(0, 0, 12, 12, BorderSide.All, 1, Color.Red);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);

            string title = "";
            DateTime date1 = this.dateTimePicker1.Value;

            if (dgv.Visible)
                title = date1.Year.ToString() + " Employee Timesheet";
            else if (dgv7.Visible)
                title = date1.Year.ToString() + " Employee PartTime Hours";
            else if (dgv8.Visible)
                title = date1.Year.ToString() + " Employee Other Services Worked";
            else if (dgv4.Visible)
                title = date1.Year.ToString() + " Employee Timesheet Report";
            else if (dgv5.Visible)
                title = date1.Year.ToString() + " Employee Vacation Requests Report";
            else if (rtb.Visible)
                title = date1.Year.ToString() + " Individual Employee Timesheet Punch Report";

            if (workPrintOnly)
            {
                if (printingWhat == "TIMESHEET")
                    title = date1.Year.ToString() + " Employee Timesheet";
                else if (printingWhat == "CONTRACT")
                    title = date1.Year.ToString() + " Employee PartTime Hours";
                else if (printingWhat == "OTHER")
                    title = date1.Year.ToString() + " Employee Other Services Worked";
                else if (printingWhat == "TIMEOFF")
                    title = date1.Year.ToString() + " Employee Vacation Requests Report";
            }
            else
            {
                if (printableComponentLink1.Component == dgv5)
                    title = date1.Year.ToString() + " Employee Vacation Requests Report";
                else if (printableComponentLink1.Component == dgv)
                    title = date1.Year.ToString() + " Employee Timesheet";
                else if (printableComponentLink1.Component == dgv7)
                    title = date1.Year.ToString() + " Employee PartTime Hours";
                else if (printableComponentLink1.Component == dgv8)
                    title = date1.Year.ToString() + " Employee Other Services Worked";
                else if (printableComponentLink1.Component == dgv4)
                    title = date1.Year.ToString() + " Employee Timesheet Report";
            }

            Printer.DrawQuad(6, 8, 8, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //if (dgv.Visible)
            //    Printer.DrawQuad(6, 8, 4, 4, "Employee Timesheet", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //else if (dgv7.Visible)
            //    Printer.DrawQuad(6, 8, 4, 4, "Employee Contract Hours Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.ToString("MM/dd/yyyy") + " - ";
            date = this.dateTimePicker2.Value;
            //date = date.AddDays(1);
            workDate += date.ToString("MM/dd/yyyy");

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            if (dgv2.Visible)
                Printer.DrawQuad(18, 8, 10, 4, "Pay Period:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);

            // File Date Brick
            TextBrick fileIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            fileIDLabel.Text = "TimeSheet Date";
            //fileIDLabel.Rect = new RectangleF(0, 80, 144, 18);
            fileIDLabel.Rect = new RectangleF(0, 80, 144, 14);
            title = this.dateTimePicker1.Text;
            date = this.dateTimePicker2.Value;
            //date = date.AddDays(1);
            string str = G1.DayOfWeekText(date) + ", " + date.ToString("MMMMMMMMM dd, yyyy");
            title += " -to- " + str;
            bool doit = true;
            if (doit)
            {
                TextBrick fileBrick = e.Graph.DrawString(title, Color.Black, new RectangleF(0, 80, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Left);
                fileBrick.BorderWidth = 2;
                fileBrick.Font = new Font("Arial", 16);
                fileBrick.HorzAlignment = HorzAlignment.Center;
                fileBrick.VertAlignment = VertAlignment.Top;
            }

            title = "XYZZY";
            if (dgv.Visible)
                title = "Employee (" + empno + ") " + empName;
            else if (dgv7.Visible)
                title = "Employee (" + empno + ") " + empName;
            else if (dgv8.Visible)
                title = "Employee (" + empno + ") " + empName;
            else if (dgv4.Visible)
                title = "Employee Timesheet Report";
            else if (rtb.Visible)
                title = "Individual Employee Timesheet Punch Report";

            TextBrick fBrick = e.Graph.DrawString(title, Color.Black, new RectangleF(0, 63, e.Graph.ClientPageSize.Width, 15), DevExpress.XtraPrinting.BorderSide.None);
            fBrick.BorderWidth = 2;
            fBrick.Font = new Font("Arial", 10);
            fBrick.HorzAlignment = HorzAlignment.Center;
            fBrick.VertAlignment = VertAlignment.Bottom;

            Printer.SetQuadSize(24, 12);
            Printer.DrawQuadBorder(1, 1, 24, 24, BorderSide.All, 2, Color.Black);
            Printer.DrawQuadBorder(1, 23, 24, 24, BorderSide.All, 2, Color.Black);
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderAreayy(object sender, CreateAreaEventArgs e)
        {
            // Date in upper left corner
            PageInfoBrick printDate = e.Graph.DrawPageInfo(PageInfo.DateTime, "{0:MM/dd/yyyy HH:mm}", Color.Black, new RectangleF(0, 0, 200, 18), BorderSide.None);

            // Create and Draw the Report Title, Include Thick bottom border
            DateTime date1 = this.dateTimePicker1.Value;
            string title = "";
            if (dgvNormalPrinting || 1 == 1)
            {
                if (dgv.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Timesheet for (" + empno + ") " + empName;
                else if (dgv7.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Contract Hours for (" + empno + ") " + empName;
                else if (dgv8.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Other Services Worked for (" + empno + ") " + empName;
                else if (dgv4.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Timesheet Report";
                else if (rtb.Visible)
                    title = date1.Year.ToString() + " SMFS Individual Employee Timesheet Punch Report";
            }
            else
            {
                title = date1.Year.ToString() + " SMFS Employee Timesheet Report";
                if (dgv2.Visible)
                    title = date1.Year.ToString() + " SMFS Employee PTO Timesheet Report";
                else if (dgv7.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Contract Report";
                else if (dgv8.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Other Services Worked for (" + empno + ") " + empName;
                else if (rtb.Visible)
                    title = date1.Year.ToString() + " SMFS Individual Employee Timesheet Punch Report";
            }
            TextBrick textBrick = e.Graph.DrawString(title, Color.Black, new RectangleF(0, 18, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
            textBrick.BorderWidth = 2;
            textBrick.Font = new Font("Arial", 16);
            textBrick.HorzAlignment = HorzAlignment.Center;
            textBrick.VertAlignment = VertAlignment.Bottom;

            // RightTopPanel
            // Page Number Brick
            //TextBrick pageNumberLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            //pageNumberLabel.Text = "PAGE NO.";
            //pageNumberLabel.Rect = new RectangleF(0, 0, 144, 18);
            //PageInfoBrick pageNumberInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            //pageNumberInfo.PageInfo = PageInfo.Number;
            //pageNumberInfo.Rect = new RectangleF(60, 0, 84, 18);
            //pageNumberInfo.HorzAlignment = HorzAlignment.Far;

            TextBrick pageNumberLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            pageNumberLabel.Text = "PAGE NO.";
            pageNumberLabel.Rect = new RectangleF(500, 0, 144, 18);
            PageInfoBrick pageNumberInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            pageNumberInfo.PageInfo = PageInfo.Number;
            pageNumberInfo.Rect = new RectangleF(60, 0, 84, 18);
            pageNumberInfo.HorzAlignment = HorzAlignment.Far;

            bool doit = true;

            // UserName Brick
            TextBrick userIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            string str = this.Text;
            int idx = str.IndexOf("Current Time");
            if (idx > 0)
                str = str.Substring(0, idx);
            userIDLabel.Text = str;
            userIDLabel.Rect = new RectangleF(0, 18, 1000, 18);
            //PageInfoBrick userIDInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            //userIDInfo.PageInfo = PageInfo.UserName;
            //userIDInfo.Rect = new RectangleF(60, 18, 84, 18);

            // Create RightTopPanel and Paint
            PanelBrick rightTopPanel = new PanelBrick();
            rightTopPanel.BorderWidth = 0;
            rightTopPanel.Bricks.Add(pageNumberLabel);
            rightTopPanel.Bricks.Add(pageNumberInfo);
            //if (doit)
            //{
            //    if (printUserName)
            //        rightTopPanel.Bricks.Add(userIDLabel);
            //}
            //            rightTopPanel.Bricks.Add(userIDInfo);
            //            e.Graph.DrawBrick(rightTopPanel, new RectangleF(816, 0, 144, 36));
            e.Graph.DrawBrick(rightTopPanel, new RectangleF(0, 45, 1000, 36));

            // File Date Brick
            TextBrick fileIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            fileIDLabel.Text = "TimeSheet Date";
            //fileIDLabel.Rect = new RectangleF(0, 80, 144, 18);
            fileIDLabel.Rect = new RectangleF(0, 80, 144, 14);
            title = this.dateTimePicker1.Text;
            title += " -to- " + this.dateTimePicker2.Text;
            if (doit)
            {
                TextBrick fileBrick = e.Graph.DrawString(title, Color.Black, new RectangleF(0, 80, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
                fileBrick.BorderWidth = 2;
                fileBrick.Font = new Font("Arial", 16);
                fileBrick.HorzAlignment = HorzAlignment.Center;
                fileBrick.VertAlignment = VertAlignment.Top;
            }
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderAreaxx(object sender, CreateAreaEventArgs e)
        {
            // Date in upper left corner
            PageInfoBrick printDate = e.Graph.DrawPageInfo(PageInfo.DateTime, "{0:MM/dd/yyyy HH:mm}", Color.DarkBlue, new RectangleF(0, 0, 200, 18), BorderSide.None);

            // Create and Draw the Report Title, Include Thick bottom border
            DateTime date1 = this.dateTimePicker1.Value;
            string title = "";
            if (dgvNormalPrinting)
            {
                if (dgv.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Timesheet for (" + empno + ") " + empName;
                else if (dgv4.Visible)
                    title = date1.Year.ToString() + " SMFS Employee Timesheet Report";
                else if (rtb.Visible)
                    title = date1.Year.ToString() + " SMFS Individual Employee Timesheet Punch Report";
            }
            else
            {
                title = date1.Year.ToString() + " SMFS Employee Timesheet Report";
                if (dgv2.Visible)
                    title = date1.Year.ToString() + " SMFS Employee PTO Timesheet Report";
                else if (rtb.Visible)
                    title = date1.Year.ToString() + " SMFS Individual Employee Timesheet Punch Report";
            }
            TextBrick textBrick = e.Graph.DrawString(title, Color.Navy, new RectangleF(0, 18, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
            textBrick.BorderWidth = 2;
            textBrick.Font = new Font("Arial", 16);
            textBrick.HorzAlignment = HorzAlignment.Center;
            textBrick.VertAlignment = VertAlignment.Top;

            // RightTopPanel
            // Page Number Brick
            TextBrick pageNumberLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            pageNumberLabel.Text = "PAGE NO.";
            pageNumberLabel.Rect = new RectangleF(0, 0, 144, 18);
            PageInfoBrick pageNumberInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            pageNumberInfo.PageInfo = PageInfo.Number;
            pageNumberInfo.Rect = new RectangleF(60, 0, 84, 18);
            pageNumberInfo.HorzAlignment = HorzAlignment.Far;

            bool doit = true;

            // UserName Brick
            TextBrick userIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            string str = this.Text;
            int idx = str.IndexOf("Current Time");
            if (idx > 0)
                str = str.Substring(0, idx);
            userIDLabel.Text = str;
            userIDLabel.Rect = new RectangleF(0, 18, 1000, 18);
            //PageInfoBrick userIDInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            //userIDInfo.PageInfo = PageInfo.UserName;
            //userIDInfo.Rect = new RectangleF(60, 18, 84, 18);

            // Create RightTopPanel and Paint
            PanelBrick rightTopPanel = new PanelBrick();
            rightTopPanel.BorderWidth = 0;
            rightTopPanel.Bricks.Add(pageNumberLabel);
            rightTopPanel.Bricks.Add(pageNumberInfo);
            if (doit)
            {
                if (printUserName)
                    rightTopPanel.Bricks.Add(userIDLabel);
            }
            //            rightTopPanel.Bricks.Add(userIDInfo);
            //            e.Graph.DrawBrick(rightTopPanel, new RectangleF(816, 0, 144, 36));
            e.Graph.DrawBrick(rightTopPanel, new RectangleF(0, 45, 1000, 36));

            // File Date Brick
            TextBrick fileIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            fileIDLabel.Text = "TimeSheet Date";
            fileIDLabel.Rect = new RectangleF(0, 80, 144, 18);
            title = this.dateTimePicker1.Text;
            title += " -to- " + this.dateTimePicker2.Text;
            if (doit)
            {
                TextBrick fileBrick = e.Graph.DrawString(title, Color.Navy, new RectangleF(0, 80, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
                fileBrick.BorderWidth = 2;
                fileBrick.Font = new Font("Arial", 16);
                fileBrick.HorzAlignment = HorzAlignment.Center;
                fileBrick.VertAlignment = VertAlignment.Top;
            }
        }
        /***********************************************************************************************/
        private void menuItemPrintPreview_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["TABPTO"])
            {
                SetupPrintPage(dgv2);
                printableComponentLink1.ShowPreview();
                return;
            }
            else if (tabControl1.SelectedTab == tabControl1.TabPages["TABDETAIL"])
            {
                SetupPrintPage(dgv3);
                printableComponentLink1.ShowPreview();
                return;
            }
            else if (tabControl1.SelectedTab == tabControl1.TabPages["TABMYTIMEOFF"])
            {
                SetupPrintPage(dgv5);
                printableComponentLink1.ShowPreview();
                return;
            }
            else if (tabControl1.SelectedTab == tabControl1.TabPages["TABCONTRACTLABOR"])
            {
                SetupPrintPage(dgv7);
                printableComponentLink1.ShowPreview();
                return;
            }
            else if (tabControl1.SelectedTab == tabControl1.TabPages["TABPAGEOTHER"])
            {
                SetupPrintPage(dgv8);
                printableComponentLink1.ShowPreview();
                return;
            }
            if (!workGroup)
            {
                gridMain.Columns["notes"].MinWidth = 0;
                int saveWidth = gridMain.Columns["notes"].Width;
                int saveDateWidth = gridMain.Columns["newDate"].Width;
                //gridMain.Columns["notes"].OptionsColumn.FixedWidth = true; // This is not working
                gridMain.Columns["notes"].Width = 100;
                gridMain.Columns["newDate"].Width = saveDateWidth + 10;
                //gridMain.Columns["notes"].Visible = true;
                SetupPrintPage(dgv);
                printableComponentLink1.Landscape = false;
                printableComponentLink1.ShowPreview();
                //gridMain.Columns["notes"].OptionsColumn.FixedWidth = false;
                gridMain.Columns["notes"].Width = saveWidth;
                gridMain.Columns["newDate"].Width = saveDateWidth;
                return;
            }
            DataTable dt = (DataTable)(dgv.DataSource);
            DataTable dx = dt.Copy();
            NumberDataTable(dx);
            SetReportApprovals(dx);
            LoadNoteDates(dx);
            LoadEmployeeSupervisors(dx);
            SavedReportDt = dx;
            SavedReportRow = -1;
            dgv4.DataSource = dx;
            SetupPrintPage(dgv4);
            printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void menuItemPrint_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["TABPTO"])
            {
                SetupPrintPage(dgv2);
                printableComponentLink1.PrintDlg();
                return;
            }
            else if (tabControl1.SelectedTab == tabControl1.TabPages["TABDETAIL"])
            {
                SetupPrintPage(dgv3);
                printableComponentLink1.PrintDlg();
                return;
            }
            else if (tabControl1.SelectedTab == tabControl1.TabPages["TABCONTRACTLABOR"])
            {
                SetupPrintPage(dgv7);
                printableComponentLink1.ShowPreview();
                return;
            }
            if (!workGroup)
            {
                gridMain.Columns["notes"].MinWidth = 0;
                int saveWidth = gridMain.Columns["notes"].Width;
                int saveDateWidth = gridMain.Columns["newDate"].Width;
                //gridMain.Columns["notes"].OptionsColumn.FixedWidth = true; // This is not working
                gridMain.Columns["notes"].Width = 100;
                gridMain.Columns["newDate"].Width = saveDateWidth + 10;
                //gridMain.Columns["notes"].Visible = true;
                SetupPrintPage(dgv);
                printableComponentLink1.Landscape = false;
                printableComponentLink1.ShowPreview();
                //gridMain.Columns["notes"].OptionsColumn.FixedWidth = false;
                gridMain.Columns["notes"].Width = saveWidth;
                gridMain.Columns["newDate"].Width = saveDateWidth;
                return;
            }
            DataTable dt = (DataTable)(dgv.DataSource);
            DataTable dx = dt.Copy();
            NumberDataTable(dx);
            SetReportApprovals(dx);
            LoadNoteDates(dx);
            LoadEmployeeSupervisors(dx);
            SavedReportDt = dx;
            SavedReportRow = -1;
            dgv4.DataSource = dx;
            SetupPrintPage(dgv4);
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
            gridMain.Columns["num"].Visible = false;
            gridMain.Columns["picture"].Visible = false;
            //if (workGroup)
            //    gridMain.Columns["notes"].Visible = false;
            gridMain.Columns["cyclenotes"].Visible = false;
            if (!workGroup)
                printUserName = true;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
            gridMain.Columns["num"].Visible = true;
            if (workGroup)
                gridMain.Columns["picture"].Visible = true;
            if (workPrintOnly)
                return;
            gridMain.Columns["notes"].Visible = true;
            if (workGroup)
                gridMain.Columns["cyclenotes"].Visible = true;
            printUserName = false;
        }
        /***********************************************************************************************/
        private void DateControl_Backward_Click(object sender, EventArgs e)
        {
            chkUnapproved.Checked = false;

            if (!CheckForSave())
                return;

            if (dgv4 != null)
            {
                if (dgv4.DataSource != null)
                {
                    DataTable dd = (DataTable)dgv4.DataSource;
                    dd.Dispose();
                }
            }
            dgv4.DataSource = null;
            if (!dateTimePicker2.Visible)
            {
                DateTime date = this.dateTimePicker1.Value;
                date = date.AddDays(-1);
                loading = true;
                this.dateTimePicker1.Value = date;
                loading = false;
                LoadTimePeriod();
                GetEmployeePunches(empno);
                LoadMyTimeOffRequests();
            }
            else
            {
                DateTime date = this.dateTimePicker1.Value;
                date = date.AddDays(-14);
                loading = true;
                this.dateTimePicker1.Value = date;
                this.dateTimePicker2.Value = date.AddDays(14);
                loading = false;
                LoadTimePeriod();
                GetEmployeePunches(empno);
                LoadMyTimeOffRequests();
            }
            if (!btnPunchIn2.Visible)
                btnAddPunch_Click(null, null);
            if (!btnPunchIn3.Visible)
                btnAddPunch_Click(null, null);
            if (!btnPunchIn4.Visible)
                btnAddPunch_Click(null, null);

            PerformGrouping();
        }
        /***********************************************************************************************/
        private bool ValidateMoveForward()
        {
            bool move = true;
            DateTime date = this.dateTimePicker2.Value;
            return move;
        }
        /***********************************************************************************************/
        private void DateControl_Forward_Click(object sender, EventArgs e)
        {
            if (!ValidateMoveForward())
            {
                MessageBox.Show("***WARNING*** Move More Than One Pay Period Beyond Current Pay Period Not Permitted!");
                return;
            }

            if (!CheckForSave())
                return;

            chkUnapproved.Checked = false;
            if (dgv4 != null)
            {
                if (dgv4.DataSource != null)
                {
                    DataTable dd = (DataTable)dgv4.DataSource;
                    dd.Dispose();
                }
            }
            dgv4.DataSource = null;
            try
            {
                if (!dateTimePicker2.Visible)
                {
                    DateTime date = this.dateTimePicker1.Value;
                    date = date.AddDays(1);
                    loading = true;
                    this.dateTimePicker1.Value = date;
                    loading = false;
                    LoadTimePeriod();
                    GetEmployeePunches(empno);
                    LoadMyTimeOffRequests();
                }
                else
                {
                    DateTime date = this.dateTimePicker1.Value;
                    date = date.AddDays(14);
                    loading = true;
                    this.dateTimePicker1.Value = date;
                    this.dateTimePicker2.Value = date.AddDays(14);
                    loading = false;
                    LoadTimePeriod();
                    GetEmployeePunches(empno);
                    LoadMyTimeOffRequests();
                }
            }
            catch (Exception ex)
            {
            }
            try
            {
                if (!btnPunchIn2.Visible)
                    btnAddPunch_Click(null, null);
                if (!btnPunchIn3.Visible)
                    btnAddPunch_Click(null, null);
                if (!btnPunchIn4.Visible)
                    btnAddPunch_Click(null, null);

                PerformGrouping();
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void menuMatch_ItemClick(object sender, ItemClickEventArgs e)
        {
            //EmrEmpXRef emrForm = new EmrEmpXRef();
            //emrForm.Show();
        }
        /***********************************************************************************************/
        private void DateControl_Backward_Click_1(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddDays(14);
            this.dateTimePicker1.Value = date;
            this.dateTimePicker2.Value = date.AddDays(-13);
            LoadTimePeriod();
            GetEmployeePunches(empno);
        }
        /***********************************************************************************************/
        private void btnSpyGlass_Click(object sender, EventArgs e)
        {
            int addHeight = 50;
            if (dgv.Visible)
            {
                if (!gridMain.OptionsFind.AlwaysVisible)
                    gridMain.OptionsFind.AlwaysVisible = true;
                else
                {
                    gridMain.FindFilterText = "";
                    gridMain.OptionsFind.AlwaysVisible = false;
                    gridMain.FindFilterText = "";
                }
            }
            else if (dgv4.Visible)
            {
                if (!gridMain4.OptionsFind.AlwaysVisible)
                    gridMain4.OptionsFind.AlwaysVisible = true;
                else
                {
                    gridMain4.FindFilterText = "";
                    gridMain4.OptionsFind.AlwaysVisible = false;
                    gridMain4.FindFilterText = "";
                }
            }
            else if (dgv2.Visible)
            {
                if (!gridView2.OptionsFind.AlwaysVisible)
                    gridView2.OptionsFind.AlwaysVisible = true;
                else
                {
                    gridView2.FindFilterText = "";
                    gridView2.OptionsFind.AlwaysVisible = false;
                    gridView2.FindFilterText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void ShowPicture(Bitmap map)
        {
            //Bitmap emptyImage = map;
            //Image image = emptyImage;
            //using (PatientPic picForm = new PatientPic(image, false))
            //{
            //    picForm.ShowDialog();
            //}
        }
        /***********************************************************************************************/
        private void gridMain_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            int rowHandle = -1;
            DevExpress.XtraGrid.Columns.GridColumn column = null;
            string ColumnName = "";
            GridHitInfo hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                rowHandle = hitInfo.RowHandle;
                column = hitInfo.Column;
                ColumnName = column.FieldName.Trim().ToUpper();
            }
        }
        /***********************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!allEmployees)
                return;
            LoadTimePeriod();
            GetEmployeePunches(empno);
        }
        /***************************************************************************************/
        public delegate void d_TimeClockDone(string empNo, string username);
        public event d_TimeClockDone TimeClockDone;
        protected void OnDone()
        {
            if (TimeClockDone != null)
            {
                if (timeSheetSaved || timeSheetContracModified || timeSheetOtherModified)
                    TimeClockDone(workEmpNo, workUserName);
            }
        }
        /***************************************************************************************/
        public delegate void d_TimeClockLoadDone();
        public event d_TimeClockLoadDone TimeClockLoadDone;
        protected void OnLoadDone()
        {
            if (TimeClockLoadDone != null)
                TimeClockLoadDone();
        }
        /***********************************************************************************************/
        private bool CheckForSave()
        {
            if (timeSheetModified || timeSheetContracModified || timeSheetOtherModified)
            {
                DialogResult result = MessageBox.Show("***Question***\nTimeSheet has been modified!\nWould you like to SAVE your Changes?", "TimeSheet Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Cancel)
                {
                    return false;
                }
                else if (result == DialogResult.Yes)
                {
                    if (timeSheetModified)
                        SaveTimeSheet();
                    if (timeSheetContracModified)
                        SaveContractTimeSheet();
                    if (timeSheetOtherModified)
                        SaveOtherTimeSheet();

                    CheckPossibleTimeLimits();
                }
                else
                {
                    timeSheetModified = false;
                    timeSheetContracModified = false;
                    timeSheetOtherModified = false;
                }
            }
            return true;
        }
        /***********************************************************************************************/
        private void TimeClock_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (!workGroup)
            //    UpdateCycleNote();
            if (timeSheetModified || timeSheetContracModified || timeSheetOtherModified)
            {
                DialogResult result = MessageBox.Show("***Question***\nTimeSheet has been modified!\nWould you like to SAVE your Changes?", "TimeSheet Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == DialogResult.Yes)
                {
                    if (timeSheetModified)
                        SaveTimeSheet();
                    if (timeSheetContracModified)
                        SaveContractTimeSheet();
                    if (timeSheetOtherModified)
                        SaveOtherTimeSheet();

                    CheckPossibleTimeLimits();
                }
            }
            OnDone();
        }
        /***********************************************************************************************/
        private void SaveContractTimeSheet()
        {
            DateTime timePeriod1 = dateTimePicker1.Value;
            DateTime midDate = timePeriod1.AddDays(6);

            DataTable dt = (DataTable)dgv7.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr;

            string cmd = "Delete from `tc_punches_pchs` WHERE `CALLEDBY` = '-1';";
            G1.get_db_data(cmd);

            string record = "";
            string deceasedName = "";
            string calledBy = "";
            string funeralNo = "";
            DateTime date = DateTime.Now;
            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            double rate = 0D;

            TimeSpan ts;
            double hours = 0D;
            double totalPay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            string machine = System.Environment.MachineName.ObjToString();
            string sTimeIn = "";
            string sTimeOut = "";
            string service = "";
            double baseRate = 0D;

            bool sendNotice = false;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dr = dt.Rows[i];
                    record = dr["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("tc_punches_pchs", "CALLEDBY", "-1");
                        if (G1.BadRecord("tc_punches_pchs", record))
                            break;
                    }

                    deceasedName = dr["deceasedName"].ObjToString();
                    calledBy = dr["calledBy"].ObjToString();
                    funeralNo = dr["funeralNo"].ObjToString();
                    service = dr["service"].ObjToString();

                    date = dr["date"].ObjToDateTime();

                    date1 = dr["timeIn1"].ObjToDateTime();
                    date2 = dr["timeOut1"].ObjToDateTime();

                    sTimeIn = date1.Hour.ToString("D2") + ":" + date1.Minute.ToString("D2");
                    sTimeOut = date2.Hour.ToString("D2") + ":" + date2.Minute.ToString("D2");

                    rate = dr["rate"].ObjToDouble();

                    ts = date2 - date1;
                    hours = ts.TotalHours;
                    totalPay = hours * rate;
                    if (hours <= 0D)
                    {
                        dr["paymentAmount"] = 0D;
                        dr["hours"] = 0D;
                    }
                    else
                    {
                        if (date1 <= midDate)
                            week1 += hours;
                        else
                            week2 += hours;

                        dr["paymentAmount"] = totalPay;
                        dr["hours"] = hours;

                        if (date.DayOfWeek == DayOfWeek.Monday)
                        {
                            if (date1 <= midDate && week1 > 20D)
                                sendNotice = true;
                            else if (week2 > 32D)
                                sendNotice = true;
                        }
                    }
                    try
                    {
                        G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "punchType", "CONTRACT", "empy!AccountingID", workEmpNo, "service", service, "rate", rate.ToString(), "date", date.ToString("MM/dd/yyyy"), "calledBy", calledBy, "deceasedName", deceasedName, "funeralNo", funeralNo, "timeIn", sTimeIn, "timeOut", sTimeOut, "user", LoginForm.username });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void SaveOtherTimeSheet()
        {
            DateTime timePeriod1 = dateTimePicker1.Value;
            DateTime midDate = timePeriod1.AddDays(6);

            DataTable dt = (DataTable)dgv8.DataSource;
            if (dt.Rows.Count <= 0)
                return;

            DataRow dr;

            string cmd = "Delete from `tc_punches_pchs` WHERE `CALLEDBY` = '-1';";
            G1.get_db_data(cmd);

            string record = "";
            string deceasedName = "";
            string calledBy = "";
            string funeralNo = "";
            DateTime date = DateTime.Now;
            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            double rate = 0D;

            TimeSpan ts;
            double hours = 0D;
            double totalPay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            string machine = System.Environment.MachineName.ObjToString();
            string sTimeIn = "";
            string sTimeOut = "";
            string service = "";
            double baseRate = 0D;

            bool sendNotice = false;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dr = dt.Rows[i];
                    record = dr["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("tc_punches_pchs", "CALLEDBY", "-1");
                        if (G1.BadRecord("tc_punches_pchs", record))
                            break;
                    }

                    deceasedName = dr["deceasedName"].ObjToString();
                    calledBy = dr["calledBy"].ObjToString();
                    funeralNo = dr["funeralNo"].ObjToString();
                    service = dr["service"].ObjToString();

                    date = dr["date"].ObjToDateTime();

                    date1 = dr["timeIn1"].ObjToDateTime();
                    date2 = dr["timeOut1"].ObjToDateTime();

                    sTimeIn = date1.Hour.ToString("D2") + ":" + date1.Minute.ToString("D2");
                    sTimeOut = date2.Hour.ToString("D2") + ":" + date2.Minute.ToString("D2");

                    rate = dr["rate"].ObjToDouble();

                    ts = date2 - date1;
                    hours = ts.TotalHours;
                    totalPay = hours * rate;
                    if (hours <= 0D)
                    {
                        dr["paymentAmount"] = 0D;
                        dr["hours"] = 0D;
                    }
                    else
                    {
                        if (date1 <= midDate)
                            week1 += hours;
                        else
                            week2 += hours;

                        dr["paymentAmount"] = totalPay;
                        dr["hours"] = hours;

                        if (date.DayOfWeek == DayOfWeek.Monday)
                        {
                            if (date1 <= midDate && week1 > 20D)
                                sendNotice = true;
                            else if (week2 > 32D)
                                sendNotice = true;
                        }
                    }
                    try
                    {
                        G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "punchType", "OTHER", "empy!AccountingID", workEmpNo, "service", service, "rate", rate.ToString(), "date", date.ToString("MM/dd/yyyy"), "calledBy", calledBy, "deceasedName", deceasedName, "funeralNo", funeralNo, "timeIn", sTimeIn, "timeOut", sTimeOut, "user", LoginForm.username });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void SaveTimeSheet()
        {
            timeSheetModified = false;
            DataTable dt = (DataTable)dgv.DataSource;
            DateTime date = DateTime.Now;
            DateTime time = DateTime.Now;
            string timeIn = "";
            string timeOut = "";
            string sTimeIn = "";
            string sTimeOut = "";
            string str = "";
            string cmd = "";
            int slot = 0;
            double totalVacation = 0D;
            double totalSick = 0D;
            double totalHoliday = 0D;
            double vacation = 0D;
            double holiday = 0D;
            double sick = 0;
            double other = 0;
            string notes = "";
            DataTable tx = null;
            string record = "";
            string machine = System.Environment.MachineName.ObjToString();
            string mod = "";
            DateTime midnight = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                vacation = dt.Rows[i]["vacation"].ObjToDouble();
                holiday = dt.Rows[i]["holiday"].ObjToDouble();
                sick = dt.Rows[i]["sick"].ObjToDouble();
                other = dt.Rows[i]["other"].ObjToDouble();
                date = dt.Rows[i]["date"].ObjToDateTime();
                notes = dt.Rows[i]["notes"].ObjToString();

                mod = dt.Rows[i]["mod"].ObjToString().ToUpper();
                if (vacation > 0D || holiday > 0D || sick > 0D || other > 0D || !String.IsNullOrWhiteSpace(notes) )
                    mod = "Y";
                if ( mod != "Y")
                    continue;
                for (int j = 1; j < 5; j++)
                {
                    str = dt.Rows[i]["IN" + j.ToString()].ObjToString();
                    if (str.ToUpper() == "MIDNIGHT")
                        time = midnight;
                    else
                        time = dt.Rows[i]["IN" + j.ToString()].ObjToDateTime();
                    sTimeIn = time.Hour.ToString("D2") + ":" + time.Minute.ToString("D2");



                    str = dt.Rows[i]["OUT" + j.ToString()].ObjToString();
                    if (str.ToUpper() == "MIDNIGHT")
                        time = midnight;
                    else
                        time = dt.Rows[i]["OUT" + j.ToString()].ObjToDateTime();
                    sTimeOut = time.Hour.ToString("D2") + ":" + time.Minute.ToString("D2");
                    if (sTimeIn == "00:00")
                    {
                        if (vacation == 0D && holiday == 0D && sick == 0D && j == 1 && String.IsNullOrWhiteSpace(notes) )
                        {
                            if (sTimeIn == "00:00" && sTimeOut == "00:00")
                            {
                                cmd = "Select * from `tc_punches_pchs` WHERE `empy!AccountingID` = '" + empno + "' and `slot` = '" + j.ToString() + "' AND `date` = '" + date.ToString("yyyyMMdd") + "';";
                                tx = G1.get_db_data(cmd);
                                if (tx.Rows.Count > 0)
                                {
                                    record = tx.Rows[0]["record"].ObjToString();
                                    G1.delete_db_table("tc_punches_pchs", "record", record);
                                }
                            }
                            if ( sTimeOut == "00:00")
                                continue;
                        }
                        else if (j != 1)
                        {
                            if (sTimeIn == "00:00" && sTimeOut == "00:00")
                            {
                                cmd = "Select * from `tc_punches_pchs` WHERE `empy!AccountingID` = '" + empno + "' and `slot` = '" + j.ToString() + "' AND `date` = '" + date.ToString("yyyyMMdd") + "';";
                                tx = G1.get_db_data(cmd);
                                if (tx.Rows.Count > 0)
                                {
                                    record = tx.Rows[0]["record"].ObjToString();
                                    G1.delete_db_table("tc_punches_pchs", "record", record);
                                }
                            }
                            if (sTimeOut == "00:00")
                                continue;
                        }
                    }

                    //timeOut = dt.Rows[i]["OUT" + j.ToString()].ObjToString();
                    str = dt.Rows[i]["OUT" + j.ToString()].ObjToString();
                    if (str.ToUpper() == "MIDNIGHT")
                        time = midnight;
                    else
                        time = dt.Rows[i]["OUT" + j.ToString()].ObjToDateTime();
                    sTimeOut = time.Hour.ToString("D2") + ":" + time.Minute.ToString("D2");

                    cmd = "Select * from `tc_punches_pchs` WHERE `empy!AccountingID` = '" + empno + "' and `slot` = '" + j.ToString() + "' AND `date` = '" + date.ToString("yyyyMMdd") + "';";
                    tx = G1.get_db_data(cmd);
                    if (tx.Rows.Count <= 0)
                    {
                        record = G1.create_record("tc_punches_pchs", "punchType", "None");
                        //cmd = "INSERT INTO `tc_punches_pchs` (`date`, `empy!AccountingID`, `ManualEntry`, `user`, `computer`, `slot`, `timeIn`, `timeOut` ) VALUES ('" + date.ToString("yyyyMMdd") + "', '" + empno + "', '0', '" + LoginForm.username + "', '" + machine + "', '" + j.ToString() + "', '" + sTimeIn + "', '" + sTimeOut + "' );";
                        cmd = "UPDATE `tc_punches_pchs` (`date`, `empy!AccountingID`, `ManualEntry`, `user`, `computer`, `slot`, `timeIn`, `timeOut` ) VALUES ('" + date.ToString("yyyyMMdd") + "', '" + empno + "', '0', '" + LoginForm.username + "', '" + machine + "', '" + j.ToString() + "', '" + sTimeIn + "', '" + sTimeOut + "' ) WHERE `record`= '" + record + "';";
                        try
                        {
                            G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "date", date.ToString("yyyyMMdd"), "empy!AccountingID", empno, "ManualEntry", "0", "user", LoginForm.username, "computer", machine, "slot", j.ToString(), "timeIn", sTimeIn, "timeOut", sTimeOut, "username", workUserName });
                            //G1.update_db_data(cmd);
                            if (j == 1)
                            {
                                totalVacation += vacation;
                                totalSick += sick;
                                totalHoliday += holiday;
                                G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "vacation", vacation.ToString(), "holiday", holiday.ToString(), "sick", sick.ToString(), "other", other.ToString(), "username", workUserName, "notes", notes });
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            record = tx.Rows[0]["record"].ObjToString();
                            G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "timeIn", sTimeIn, "timeout", sTimeOut, "vacation", "0.00", "holiday", "0.00", "sick", "0.00", "other", "0.00", "notes", notes  });
                            if (j == 1)
                            {
                                totalVacation += vacation;
                                totalSick += sick;
                                totalHoliday += holiday;
                                G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "vacation", vacation.ToString(), "holiday", holiday.ToString(), "sick", sick.ToString(), "other", other.ToString(), "username", workUserName });
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }

            CheckForApproval(totalVacation, totalSick, totalHoliday);

            timeSheetSaved = true;
        }
        /***********************************************************************************************/
        private void CheckPossibleTimeLimits()
        {
            DateTime timePeriod1 = dateTimePicker1.Value;
            long ldate = G1.TimeToUnix(timePeriod1);

            DateTime timePeriod = dateTimePicker2.Value;
            timePeriod = timePeriod.AddMinutes(-1); // This gets the time back to 23:59:00
            long edate = G1.TimeToUnix(timePeriod);

            TimeSpan ts = timePeriod - timePeriod1;

            bool showSave = true;
            if ((ts.Days + 1) > 14)
                showSave = false;

            long adate = 0L;

            DateTime firstDate = ldate.UnixToDateTime().ToLocalTime();
            DateTime lastDate = edate.UnixToDateTime().ToLocalTime();

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime date = DateTime.Now;
            double rate = 0D;
            double hours = 0D;
            double pay = 0D;
            double othours = 0D;
            double otpay = 0D;
            double total = 0D;
            double contractHours = 0D;
            double contractPay = 0D;
            double totalPay = 0D;
            double midHours = 0D;
            double vacationhours = 0D;
            double holidayhours = 0D;
            double sickhours = 0D;
            double vacationpay = 0D;
            double holidaypay = 0D;
            double sickpay = 0D;

            double vacation = 0D;
            double holiday = 0D;
            double sick = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double cweek1 = 0D;
            double cweek2 = 0D;
            bool sendNotice = false;
            bool sendContractNotice = false;

            string id = "";
            string punchType = "";
            DataRow[] dRows = null;
            DateTime midDate = firstDate.AddDays(6);

            try
            {
                string cmd = "Select * from `tc_punches_pchs` where `date` >= '" + firstDate.ToString("yyyyMMdd") + "' and `date` <= '" + lastDate.ToString("yyyyMMdd") + "' ";
                cmd += " AND `empy!AccountingID` = '" + empno + "' ";
                cmd += "order by `date`;";
                DataTable dx = G1.get_db_data(cmd);

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["date"].ObjToDateTime();
                    date1 = dx.Rows[i]["timeIn"].ObjToDateTime();
                    date2 = dx.Rows[i]["timeOut"].ObjToDateTime();
                    punchType = dx.Rows[i]["punchType"].ObjToString().Trim().ToUpper();

                    vacationhours = dx.Rows[i]["vacation"].ObjToDouble();
                    holidayhours = dx.Rows[i]["holiday"].ObjToDouble();
                    sickhours = dx.Rows[i]["sick"].ObjToDouble();

                    ts = date2 - date1;
                    hours = ts.TotalHours;
                    if (vacationhours > 0D)
                        vacation += vacationhours;
                    if (holidayhours > 0D)
                        holiday += holidayhours;
                    if (sickhours > 0D)
                        sick += sickhours;
                    if (punchType == "CONTRACT")
                    {
                        total += hours;
                        if (date <= midDate)
                            cweek1 += hours;
                        else
                            cweek2 += hours;
                    }
                    else
                    {
                        total += hours;
                        if (date <= midDate)
                            week1 += hours;
                        else
                            week2 += hours;
                    }
                    if (date.DayOfWeek == DayOfWeek.Monday)
                    {
                        if (date <= midDate && week1 > 32D)
                            sendNotice = true;
                        else if (week2 > 32D)
                            sendNotice = true;

                        if (date <= midDate && cweek1 > 20D)
                            sendContractNotice = true;
                        else if (cweek2 > 20D)
                            sendContractNotice = true;
                    }
                }
                if (sendNotice || sendContractNotice)
                {
                    string da = "hranncwgetlvkxoi";
                    string email = "";
                    string body = "";
                    cmd = "Select * from `tc_er` where `isSupervisor` = 'Y';";
                    dx = G1.get_db_data(cmd);
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        email = dx.Rows[i]["emailAddress"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(email))
                        {
                            if (sendNotice)
                            {
                                body = "Employee " + empName + " has already reached hour limit or 32 hours!<br />";
                                body += "Week1 has " + G1.ReformatMoney(week1) + " hours.<br />";
                                body += "Week2 has " + G1.ReformatMoney(week2) + " hours.";
                            }
                            if (sendContractNotice)
                            {
                                if (!string.IsNullOrWhiteSpace(body))
                                    body += "<br />Employee " + empName + " also has already reached PartTime hour limit of 20 hours!<br />";
                                else
                                    body = "Employee " + empName + " has already reached PartTime hour limit 20 hours!<br />";
                                body += "Week1 has " + G1.ReformatMoney(cweek1) + " hours.<br />";
                                body += "Week2 has " + G1.ReformatMoney(cweek2) + " hours.";
                            }
                            RemoteProcessing.SendEmailToSomewhere("Early Hours Reached for " + empName, "", email, da, body);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void CheckForApproval(double totalVacation, double totalSick, double totalHoliday)
        {
            if (!employeeApproved && !managerApproved)
            {
                if ( saveManagerApproved.ToUpper() != "Y" && saveEmployeeApproved.ToUpper() != "Y" )
                    return;
                if (saveManagerApproved.ToUpper() == "Y")
                    managerApproved = true;
                if (saveEmployeeApproved.ToUpper() == "Y")
                    employeeApproved = true;
            }

            DateTime startdate = this.dateTimePicker1.Value;
            string startDate = startdate.ToString("yyyyMMdd");
            DateTime stopdate = this.dateTimePicker2.Value;
            string endDate = stopdate.ToString("yyyyMMdd");

            string record = "";

            string cmd = "DELETE from `tc_approvals` WHERE `username` = '-1'";
            G1.get_db_data(cmd);

            cmd = "Select * from `tc_approvals` where `startdate` = '" + startDate + "' AND `enddate` = '" + endDate + "' AND `username` = '" + workUserName + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                record = G1.create_record("tc_approvals", "username", "-1");
                if (G1.BadRecord("tc_approvals", record))
                    return;
            }
            else
                record = dx.Rows[0]["record"].ObjToString();

            string employeeApproval = "";
            string managerApproval = "";
            if (chkEmployeeApproved.Checked || saveEmployeeApproved == "Y" )
                employeeApproval = "Y";
            if (chkManagerApproved.Checked || saveManagerApproved == "Y" )
                managerApproval = "Y";

            G1.update_db_table("tc_approvals", "record", record, new string[] { "user", LoginForm.username, "username", workUserName, "startdate", startDate, "enddate", endDate, "employeeApproved", employeeApproval, "managerApproved", managerApproval, "vacationHours", totalVacation.ToString(), "sickHours", totalSick.ToString(), "holidayHours", totalHoliday.ToString() });
        }
        /***********************************************************************************************/
        private void CheckApprovalsIn()
        {
            DateTime startdate = this.dateTimePicker1.Value;
            string startDate = startdate.ToString("yyyyMMdd");
            DateTime stopdate = this.dateTimePicker2.Value;
            string endDate = stopdate.ToString("yyyyMMdd");

            string record = "";
            employeeApprovedIn = false;
            managerApprovedIn = false;

            LookupActualUsername();
            if (String.IsNullOrWhiteSpace(actualUsername))
                return;

            string cmd = "Select * from `tc_approvals` where `startdate` = '" + startDate + "' AND `enddate` = '" + endDate + "' AND `username` = '" + actualUsername + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                loading = true;
                chkEmployeeApproved.Checked = false;
                chkManagerApproved.Checked = false;
                loading = false;
                return;
            }
            string str = dx.Rows[0]["employeeApproved"].ObjToString().ToUpper();
            if (str == "Y")
            {
                loading = true;
                chkEmployeeApproved.Checked = true;
                employeeApprovedIn = true;
                if ( G1.isHR() )
                    saveEmployeeApproved = "Y";
                loading = false;
            }
            str = dx.Rows[0]["managerApproved"].ObjToString().ToUpper();
            if (str == "Y")
            {
                loading = true;
                chkManagerApproved.Checked = true;
                managerApprovedIn = true;
                if (G1.isHR())
                    saveManagerApproved = "Y";
                loading = false;
            }
        }
        /***********************************************************************************************/
        private void btnError_Click(object sender, EventArgs e)
        {
            bool found = false;
            int pass = 0;
            int row = gridMain.FocusedRowHandle;
            DataTable dt = (DataTable)dgv.DataSource;
            for (; ; )
            {
                pass++;
                if (pass >= 2)
                    break;
                for (int i = (row + 1); i < dt.Rows.Count; i++)
                {
                    string error = dt.Rows[i]["ERROR"].ObjToString();
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        gridMain.FocusedRowHandle = i;
                        string emp = dt.Rows[i]["empno"].ObjToString();
                        CalculateDecember(emp);
                        ShowPictureRow(dt, i);
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
                gridMain.FocusedRowHandle = 0;
                row = 0;
            }
        }
        /***********************************************************************************************/
        private int LastPTORow = -1;
        private void gridMain_Click(object sender, EventArgs e)
        {
            if (workGroup)
            {
                int row = gridMain.FocusedRowHandle;
                if (row != LastPTORow)
                {
                    DataTable dd = (DataTable)dgv.DataSource;
                    DataRow dRow = gridMain.GetFocusedDataRow();
                    string emp = dRow["empno"].ObjToString();
                    CalculateDecember(emp);
                    txtAvailablePTO.Refresh();
                    txtDecemberPTO.Refresh();
                    LastPTORow = -1;
                    if (dRow["picture"] != null)
                    {
                        try
                        {
                            this.picEmployee.Image = (Bitmap)(dRow["picture"]);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain4_Click(object sender, EventArgs e)
        {
            int row = gridMain4.FocusedRowHandle;
            DataTable dd = (DataTable)dgv4.DataSource;
            DataRow dRow = gridMain4.GetFocusedDataRow();
            string emp = dRow["empno"].ObjToString();
            CalculateDecember(emp);
            txtAvailablePTO.Refresh();
            txtDecemberPTO.Refresh();
            LastPTORow = -1;
            if (dRow["picture"] != null)
            {
                try
                {
                    this.picEmployee.Image = (Bitmap)(dRow["picture"]);
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void SetCheckBoxes(string employee, long ldate, long edate, string salaried)
        {
            if (salaried != "S")
                return;

            DataTable dt = (DataTable)(dgv.DataSource);

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["part"] = "";
                dt.Rows[i]["worked"] = "";
                dt.Rows[i]["full"] = "";
            }
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";

            string cmd = "Select * from `tc_salarylog_salg` WHERE `UnixTime` >= '" + ldate.ToString() + "' and `UnixTime` <= '" + edate.ToString() + "' and `empy!AccountingID` = '" + employee + "';";

            DataTable dd = G1.get_db_data(cmd);

            double Worked = 0D;
            double Vacation = 0D;
            double Holiday = 0D;
            double Sick = 0D;
            double Other = 0D;
            double expectedHours = 0D;

            for (int i = 0; i < dd.Rows.Count; i++)
            {
                long mDate = dd.Rows[i]["UnixTime"].ObjToInt64();
                DecodeSalaryRow(dd, i, expectedHours, ref Worked, ref Vacation, ref Holiday, ref Sick, ref Other);
                string hours = dd.Rows[i]["hours_work"].ObjToString().ToUpper();
                DateTime firstDate = mDate.UnixToDateTime();

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    DateTime workDate = dt.Rows[j]["date"].ObjToDateTime();
                    if (workDate == firstDate)
                    {
                        if (hours == "FULL")
                            dt.Rows[j]["full"] = "Y";
                        dt.Rows[j]["Worked"] = RoundValue(Holiday + Other).ObjToString();
                        dt.Rows[j]["Part"] = RoundValue(Vacation + Sick).ObjToString();
                        break;
                    }
                }
            }
            CleanupAllColumns(dt);
        }
        /***********************************************************************************************/
        private void ClearApprovals()
        {
            DataTable dt = (DataTable)(dgv.DataSource);

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["approve"] = "";
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";
        }
        /***********************************************************************************************/
        private int DetermineCycle()
        {
            DateTime startTime = dateTimePicker1.Value;
            int CycleOffset = 104;
            DateTime beginningDate = new DateTime(2016, 7, 26);
            TimeSpan ts = startTime - beginningDate;

            int span = ts.Days / 14;

            CycleOffset += span;
            return CycleOffset;
        }
        /***********************************************************************************************/
        private void SetApprovals()
        {
            int CycleOffset = DetermineCycle();

            DataTable dt = (DataTable)(dgv.DataSource);

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["approve"] = "";
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";

            DateTime timePeriod = dateTimePicker1.Value;
            long ldate = G1.TimeToUnix(timePeriod);

            string cmd = "Select * from `tc_signoffs_sgno` WHERE `CycleOffset` = '" + CycleOffset + "';";

            DataTable dd = G1.get_db_data(cmd);

            string empno = "";

            for (int i = 0; i < dd.Rows.Count; i++)
            {
                empno = dd.Rows[i]["empy!AccountingID"].ObjToString();
                DataRow[] dRow = dt.Select("empno='" + empno + "'");
                if (dRow.Length > 0)
                    dRow[0]["approve"] = "Y";
            }

        }
        /***********************************************************************************************/
        private string GetNoteChanges(long saveLdate, long saveEdate, string employee = "")
        {
            bool grouped = true;
            if (!string.IsNullOrWhiteSpace(employee))
                grouped = false;

            DataTable dt = (DataTable)(dgv.DataSource);
            string cmd = "Select * from `tc_notesday_dnot` where `UnixTime` >= '" + saveLdate + "' and `UnixTime` <= '" + saveEdate + "' ";
            if (!grouped)
                cmd += " and `empy!AccountingID` = '" + employee + "' ";
            cmd += "order by `empy!AccountingID`,`UnixTime`;";

            DataTable dd = G1.get_db_data(cmd);

            string empno = "";
            string oldemp = "";
            string notes = "";
            string note = "";
            for (int i = 0; i < dd.Rows.Count; i++)
            {
                empno = dd.Rows[i]["empy!AccountingID"].ObjToString();
                if (empno != oldemp)
                    notes = "";
                oldemp = empno;
                DataRow[] dRow = dt.Select("empno='" + empno + "'");
                if (dRow.Length > 0)
                {
                    note = dd.Rows[i]["Note"].ObjToString();
                    if (string.IsNullOrWhiteSpace(note))
                        continue;
                    if (!string.IsNullOrWhiteSpace(notes))
                        notes += "/";
                    notes += note;
                    dRow[0]["notes"] = notes;
                }
            }
            return notes;
        }
        /***********************************************************************************************/
        private void SetNotes(long saveLdate, long saveEdate, string employee = "")
        {
            bool grouped = true;
            if (!string.IsNullOrWhiteSpace(employee))
                grouped = false;

            DataTable dt = (DataTable)(dgv.DataSource);
            string cmd = "Select * from `tc_notesday_dnot` where `UnixTime` >= '" + saveLdate + "' and `UnixTime` <= '" + saveEdate + "' ";
            if (!grouped)
                cmd += " and `empy!AccountingID` = '" + employee + "' ";
            cmd += "order by `empy!AccountingID`,`UnixTime`;";

            DataTable dd = G1.get_db_data(cmd);

            string empno = "";
            string oldemp = "";
            string notes = "";
            string note = "";

            if (grouped)
            {
                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    empno = dd.Rows[i]["empy!AccountingID"].ObjToString();
                    if (empno != oldemp)
                        notes = "";
                    oldemp = empno;
                    DataRow[] dRow = dt.Select("empno='" + empno + "'");
                    if (dRow.Length > 0)
                    {
                        note = dd.Rows[i]["Note"].ObjToString();
                        if (string.IsNullOrWhiteSpace(note))
                            continue;
                        if (!string.IsNullOrWhiteSpace(notes))
                            notes += "/";
                        notes += note;
                        dRow[0]["notes"] = notes;
                    }
                }
            }
            else
            {
                long ldate = 0L;
                DateTime dayDate = DateTime.Now;
                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    ldate = dd.Rows[i]["UnixTime"].ObjToInt64();
                    DateTime date = ldate.UnixToDateTime();
                    notes = dd.Rows[i]["Note"].ObjToString();
                    if (string.IsNullOrWhiteSpace(notes))
                        continue;

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dayDate = dt.Rows[j]["date"].ObjToDateTime();
                        if (dayDate == date)
                            dt.Rows[j]["notes"] = notes;
                    }
                }
            }
            int CycleOffset = DetermineCycle();
            cmd = "SELECT * from `tc_cyclenotes_cnot` WHERE `CycleOffset` = '" + CycleOffset.ToString() + "' ";
            if (!grouped)
                cmd += " and `empy!AccountingID` = '" + employee + "' ";
            cmd += "order by `empy!AccountingID`,`CycleOffset`;";
            dd = G1.get_db_data(cmd);
            empno = "";
            oldemp = "";
            notes = "";
            bool newemp = true;
            for (int i = 0; i < dd.Rows.Count; i++)
            {
                empno = dd.Rows[i]["empy!AccountingID"].ObjToString();
                if (empno != oldemp)
                    newemp = true;
                oldemp = empno;
                DataRow[] dRow = dt.Select("empno='" + empno + "'");
                if (dRow.Length > 0)
                {
                    if (newemp)
                        notes = "";
                    if (!string.IsNullOrWhiteSpace(notes))
                        notes += "/";
                    notes += dd.Rows[i]["Note"].ObjToString();
                    dRow[0]["cyclenotes"] = notes;
                    if (!grouped)
                        txtCycleNote.Text = notes;
                }
                newemp = false;
            }
        }
        /***********************************************************************************************/
        private void ClearCheckBoxes()
        {
            DataTable dt = (DataTable)(dgv.DataSource);

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["full"] = "";
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            //            dr["full"] = "1";
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        Point savePoint = new Point();
        Point savePoint4 = new Point();
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            savePoint = e.Location;
            GridHitInfo hitInfo = gridMain.CalcHitInfo(savePoint);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                DevExpress.XtraGrid.Columns.GridColumn column = hitInfo.Column;
                string ColumnName = column.FieldName.Trim().ToUpper();
                if (ColumnName == "PICTURE")
                {
                    DataRow dr = gridMain.GetDataRow(rowHandle);
                    string empno = dr["empno"].ObjToString();
                    //                    if (dr["picture"] != null)
                    if (!string.IsNullOrWhiteSpace(dr["picture"].ObjToString()))
                    {
                        Bitmap map = (Bitmap)(dr["picture"]);
                        ShowPicture(map);
                    }
                }
                else if (ColumnName == "OTHER")
                {
                }
                else
                {
                    DataRow dr = gridMain.GetDataRow(rowHandle);
                    ShowPicture(dr);
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain4_MouseDown(object sender, MouseEventArgs e)
        {
            savePoint4 = e.Location;
            GridHitInfo hitInfo = gridMain4.CalcHitInfo(savePoint4);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                DevExpress.XtraGrid.Columns.GridColumn column = hitInfo.Column;
                string ColumnName = column.FieldName.Trim().ToUpper();
                if (ColumnName == "PICTURE")
                {
                    DataRow dr = gridMain.GetDataRow(rowHandle);
                    string empno = dr["empno"].ObjToString();
                    if (!string.IsNullOrWhiteSpace(dr["picture"].ObjToString()))
                    {
                        Bitmap map = (Bitmap)(dr["picture"]);
                        ShowPicture(map);
                    }
                }
                else
                {
                    if (rowHandle >= 0)
                    {
                        DataRow dr = gridMain.GetDataRow(rowHandle);
                        //                    DataRow dr = gridMain4.GetFocusedDataRow();
                        ShowPicture(dr);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void approveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataView view = dt.DefaultView;

            DataTable newDt = view.ToTable();
            for (int i = 0; i < newDt.Rows.Count; i++)
            {
                string empno = newDt.Rows[i]["empno"].ObjToString();
                DataRow[] dRow = dt.Select("empno='" + empno + "'");
                if (dRow.Length > 0)
                    dRow[0]["approve"] = "Y";
                if (SavedSuperDt != null)
                {
                    dRow = SavedSuperDt.Select("empno='" + empno + "'");
                    if (dRow.Length > 0)
                        dRow[0]["approve"] = "Y";
                }
            }
            dgv.RefreshDataSource();
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string myCol = gridMain.FocusedColumn.FieldName.ToUpper();
            DataRow dRow = GetCurrentGridRow();
            string column = GetCurrentColumn();
            column = myCol;
            if (String.IsNullOrWhiteSpace(column))
                column = gridMain.FocusedColumn.FieldName.ToUpper();
            //if ( column.ToUpper().IndexOf ( "IN" ) == 0 )
            //{
            //    if (cellColumn.ToUpper() == "OUT")
            //        column = column.Replace("IN", "OUT");
            //}
            //else if (column.ToUpper().IndexOf("OUT") == 0)
            //{
            //    if (cellColumn.ToUpper() == "IN")
            //        column = column.Replace("OUT", "IN");
            //}
            if (column == "NOTES")
            {
                if (!workGroup)
                { // Individual Employee Notes
                    row = e.RowHandle;
                    //DataTable dt = (DataTable)dgv.DataSource;
                    string empno = dRow["empno"].ObjToString();
                    DateTime date = dRow["date"].ObjToDateTime();
                    long ldate = G1.TimeToUnix(date);
                    string note = dRow["notes"].ObjToString();
                    note = G1.protect_data(note);

                    string cmd = "Select * from `tc_notesday_dnot` where `UnixTime` = '" + ldate + "' ";
                    cmd += " and `empy!AccountingID` = '" + empno + "' ";
                    cmd += "order by `empy!AccountingID`,`UnixTime`;";
                    DataTable dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count <= 0)
                    { // Might want to add user who did this but wait unil fully operational after eliminating Dusty's code. Same on update
                        cmd = "INSERT INTO `tc_notesday_dnot` (`UnixTime`, `empy!AccountingID`, `Note`) VALUES ('" + ldate.ToString() + "', '" + empno + "', '" + note + "' );";
                        DataTable ddd = G1.get_db_data(cmd);
                    }
                    else
                    {
                        cmd = "Update `tc_notesday_dnot` Set `Note` = '" + note + "' where `UnixTime` = '" + ldate.ToString() + "' and `empy!AccountingID` = '" + empno + "';";
                        DataTable ddd = G1.get_db_data(cmd);
                    }
                    cellRowHandle = -1;
                    cellColumn = "";
                    cellChanging = "";

                    timeSheetModified = true;
                    dRow["mod"] = "Y";
                }
            }
            else if (column == "CYCLENOTES")
            {
                int cycle = DetermineCycle();
                //DataTable dt = (DataTable)dgv.DataSource;
                string empno = dRow["empno"].ObjToString();
                string note = dRow["cyclenotes"].ObjToString();
                note = G1.protect_data(note);
                string cmd = "Select * from `tc_cyclenotes_cnot` where `CycleOffset` = '" + cycle + "' ";
                cmd += " and `empy!AccountingID` = '" + empno + "' ";
                cmd += "order by `empy!AccountingID`,`CycleOffset`;";
                DataTable dd = G1.get_db_data(cmd);
                if (dd.Rows.Count <= 0)
                {
                    cmd = "INSERT INTO `tc_cyclenotes_cnot` (`CycleOffset`, `empy!AccountingID`, `Note`) VALUES ('" + cycle.ToString() + "', '" + empno + "', '" + note + "' );";
                    DataTable ddd = G1.get_db_data(cmd);
                }
                else
                {
                    cmd = "Update `tc_cyclenotes_cnot` Set `Note` = '" + note + "' where `CycleOffset` = '" + cycle.ToString() + "' and `empy!AccountingID` = '" + empno + "';";
                    DataTable ddd = G1.get_db_data(cmd);
                }
            }
            else if (column == "FULL")
            {
                //                repositoryItemCheckEdit1_CheckedChanged_1(sender, null);
            }
            else if (column == "VACATION")
            {
                timeSheetModified = true;
                dRow["mod"] = "Y";
                CalcHours(dt, row);
            }
            else if (column == "HOLIDAY")
            {
                timeSheetModified = true;
                dRow["mod"] = "Y";
                CalcHours(dt, row);
            }
            else if (column == "SICK")
            {
                timeSheetModified = true;
                dRow["mod"] = "Y";
                CalcHours(dt, row);
            }
            else if (column == "OTHER")
            {
                timeSheetModified = true;
                dRow["mod"] = "Y";
                CalcHours(dt, row);
            }
            else if (column == "WORKED")
            {
                if (!workGroup)
                { // Individual Employee Notes
                    //DataTable dt = (DataTable)dgv.DataSource;
                    string empno = dRow["empno"].ObjToString();
                    if (dgv.Visible)
                    {
                        ReCalcSalary(empno);
                        row = e.RowHandle;
                        DateTime date = dRow["date"].ObjToDateTime();
                        double worked = dRow["worked"].ObjToDouble();
                        long ldate = G1.TimeToUnix(date);

                        string salaried = GetSalaried(empno);
                        if (salaried == "S")
                        {
                            string cmd = "Select * from `tc_salarylog_salg` where `UnixTime` = '" + ldate + "' ";
                            cmd += " and `empy!AccountingID` = '" + empno + "' ";
                            cmd += "order by `empy!AccountingID`,`UnixTime`;";
                            DataTable dd = G1.get_db_data(cmd);
                            if (dd.Rows.Count <= 0)
                            { // Might want to add user who did this but wait unil fully operational after eliminating Dusty's code. Same on update
                                cmd = "INSERT INTO `tc_salarylog_salg` (`UnixTime`, `empy!AccountingID`, `hours_other`) VALUES ('" + ldate.ToString() + "', '" + empno + "', '" + worked.ToString() + "' );";
                                DataTable ddd = G1.get_db_data(cmd);
                            }
                            else
                            {
                                cmd = "Update `tc_salarylog_salg` Set `hours_other` = '" + worked.ToString() + "' where `UnixTime` = '" + ldate.ToString() + "' and `empy!AccountingID` = '" + empno + "';";
                                DataTable ddd = G1.get_db_data(cmd);
                            }
                        }
                        else
                        {
                            IndForm_EditPunchesDone();
                        }
                    }
                    cellRowHandle = -1;
                    cellColumn = "";
                    cellChanging = "";
                }
            }
            else if (column.ToUpper().IndexOf("IN") == 0)
            {
                Button button = this.btnAddNextPunch;
                DateTime now = DateTime.Now;
                now = dRow["date"].ObjToDateTime();
                string str = e.Value.ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                {
                    timeSheetModified = true;
                    dRow["mod"] = "Y";
                    gridMain.RefreshData();
                    str = "00:00:00";
                    //return;
                }
                str = reformatToProperTime(now, str);
                if ( str.ToUpper() == "MIDNIGHT")
                {
                    timeSheetModified = true;
                    dRow["mod"] = "Y";
                    dRow[column] = str;
                    gridMain.RefreshData();
                    return;
                }
                string[] Lines = str.Split(':');
                string sHour = "00";
                string sMinute = "00";
                string sSeconds = "00";
                if (Lines.Length >= 1)
                    sHour = Lines[0].Trim();
                if (Lines.Length >= 2)
                    sMinute = Lines[1].Trim();
                if (!G1.validate_numeric(sHour) || !G1.validate_numeric(sMinute))
                    return;
                int iHour = sHour.ObjToInt32();
                if (iHour <= 0 || iHour >= 24)
                    iHour = 0;
                int iMinute = sMinute.ObjToInt32();
                if (iMinute <= 0 || iMinute >= 60)
                    iMinute = 0;
                if (iHour == 17 && iMinute == 0)
                    sSeconds = "01";
                DateTime ClockTime = new DateTime(now.Year, now.Month, now.Day, iHour, iMinute, sSeconds.ObjToInt32());
                AddNextManualPunch(button, ClockTime);
                button = this.btnAddNextPunch;
                timeSheetModified = true;
                dRow["mod"] = "Y";
                gridMain.RefreshData();
            }
            else if (column.ToUpper().IndexOf("OUT") == 0)
            {
                Button button = this.btnAddNextPunch;
                DateTime now = DateTime.Now;
                now = dRow["date"].ObjToDateTime();
                string str = e.Value.ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                {
                    timeSheetModified = true;
                    dRow["mod"] = "Y";
                    str = "00:00:00";
                    //return;
                }
                str = reformatToProperTime(now, str);
                if (str.ToUpper() == "MIDNIGHT")
                {
                    timeSheetModified = true;
                    dRow["mod"] = "Y";
                    dRow[column] = str;
                    //gridMain.RefreshData();
                    str = "00:00:00";
                    //return;
                }
                string[] Lines = str.Split(':');
                string sHour = "00";
                string sMinute = "00";
                string sSeconds = "00";
                if (Lines.Length >= 1)
                    sHour = Lines[0].Trim();
                if (Lines.Length >= 2)
                    sMinute = Lines[1].Trim();
                if (!G1.validate_numeric(sHour) || !G1.validate_numeric(sMinute))
                    return;
                int iHour = sHour.ObjToInt32();
                if (iHour <= 0 || iHour >= 24)
                    iHour = 0;
                int iMinute = sMinute.ObjToInt32();
                if (iMinute <= 0 || iMinute >= 60)
                    iMinute = 0;
                DateTime ClockTime = new DateTime(now.Year, now.Month, now.Day, iHour, iMinute, sSeconds.ObjToInt32());
                AddNextManualPunch(button, ClockTime);
                button = this.btnAddNextPunch;
                timeSheetModified = true;
                dRow["mod"] = "Y";
                //gridMain.RefreshEditor(true);
                gridMain.RefreshData();
            }
        }
        /***********************************************************************************************/
        private string reformatToProperTime(DateTime now, string str)
        {
            bool gotPM = false;
            bool gotAM = false;
            str = str.ToUpper();
            if (str.ToUpper().IndexOf("PM") > 0)
            {
                gotPM = true;
                str = str.Replace("PM", "");
                str = str.Trim();
            }
            else if (str.ToUpper().IndexOf("AM") > 0)
            {
                gotAM = true;
                str = str.Replace("AM", "");
                str = str.Trim();
            }
            string[] Lines = str.Split(':');
            string sHour = "00";
            string sMinute = "00";
            string sSeconds = "00";
            if (Lines.Length >= 1)
                sHour = Lines[0].Trim();
            if (Lines.Length >= 2)
                sMinute = Lines[1].Trim();

            if (!G1.validate_numeric(sHour) || !G1.validate_numeric(sMinute))
                return str;

            int iHour = sHour.ObjToInt32();
            if (iHour <= 0 || iHour >= 24)
                iHour = 0;
            int iMinute = sMinute.ObjToInt32();
            if (iMinute <= 0 || iMinute >= 60)
                iMinute = 0;

            DateTime ClockTime = new DateTime(now.Year, now.Month, now.Day, iHour, iMinute, sSeconds.ObjToInt32());

            if (gotPM)
            {
                if (iHour != 12)
                    ClockTime = ClockTime.AddHours(12);
                str = ClockTime.ToString("HH:mm:ss");
            }
            else if ( gotAM )
            {
                if (iHour == 12 && iMinute == 0)
                    str = "Midnight";
                else if (iHour == 0 && iMinute == 0)
                    str = "Midnight";
            }
            return str;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged_1(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string empno = dr["empno"].ObjToString();
            GridHitInfo hitInfo = gridMain.CalcHitInfo(savePoint);

            bool tchecked = ApproveDisApproveTime(sender, dr, hitInfo);

            if (dgv4 != null)
            {
                if (dgv4.DataSource != null)
                {
                    DataTable dt = (DataTable)dgv4.DataSource;
                    CheckOtherGrid(dt, empno, tchecked);
                }
            }

            if (dgv.Visible)
            {
                if (!workGroup && !allEmployees)
                    ReCalcSalary(empno);
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit3_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string empno = dr["empno"].ObjToString();
            GridHitInfo hitInfo = gridMain4.CalcHitInfo(savePoint4);

            bool tchecked = ApproveDisApproveTime(sender, dr, hitInfo);

            DataTable dt = (DataTable)dgv.DataSource;
            CheckOtherGrid(dt, empno, tchecked);

            gridMain4.RefreshData();
            dgv4.Refresh();
        }
        /***********************************************************************************************/
        private bool ApproveDisApproveTime(object sender, DataRow dr, GridHitInfo hitInfo)
        {
            string empno = dr["empno"].ObjToString();
            int rowHandle = -1;
            DevExpress.XtraGrid.Columns.GridColumn column = null;
            string ColumnName = "";

            ShowPicture(dr);

            bool tchecked = false;

            if (hitInfo.InRowCell)
            {
                CheckEdit checkbox = (CheckEdit)sender;
                if (checkbox.Checked)
                    tchecked = true;
                rowHandle = hitInfo.RowHandle;
                column = hitInfo.Column;
                ColumnName = column.FieldName.Trim().ToUpper();
                if (ColumnName.Trim().ToUpper() == "FULL")
                {
                    DateTime timePeriod = dateTimePicker1.Value;
                    if (rowHandle > 0)
                        timePeriod = timePeriod.AddDays(rowHandle);
                    long ldate = G1.TimeToUnix(timePeriod);
                    string cmd = "";
                    if (tchecked)
                    {
                        cmd = "SELECT * from `tc_salarylog_salg` WHERE `UnixTime` = '" + ldate.ToString() + "' and `empy!AccountingID` ='" + empno + "';";
                        DataTable dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count > 0)
                        {
                            cmd = "DELETE from `tc_salarylog_salg` WHERE `UnixTime` = '" + ldate.ToString() + "' and `empy!AccountingID` ='" + empno + "';";
                            G1.get_db_data(cmd);
                        }
                        dr["full"] = "Y";
                        cmd = "INSERT INTO `tc_salarylog_salg` (`UnixTime`, `empy!AccountingID`, `hours_work`) VALUES ('" + ldate.ToString() + "', '" + empno + "', 'full' );";
                    }
                    else
                    {
                        dr["full"] = "";
                        cmd = "SELECT * from `tc_salarylog_salg` WHERE `UnixTime` = '" + ldate.ToString() + "' and `empy!AccountingID` ='" + empno + "';";
                        DataTable dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count > 0)
                            cmd = "DELETE from `tc_salarylog_salg` WHERE `UnixTime` = '" + ldate.ToString() + "' and `empy!AccountingID` ='" + empno + "';";
                    }

                    try
                    {
                        G1.update_db_data(cmd);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
                if (ColumnName.Trim().ToUpper() == "APPROVE")
                {
                    bool canDo = CheckCanDo();
                    if ( !canDo )
                    {
                        MessageBox.Show("***WARNING*** You are not setup for Approving Time Cards!!!");
                        return false;
                    }
                    int CycleOffset = DetermineCycle();
                    DateTime timePeriod = dateTimePicker1.Value;
                    long ldate = G1.TimeToUnix(timePeriod);
                    string cmd = "";
                    if (tchecked)
                    {
                        dr["approve"] = "Y";
                        cmd = "INSERT INTO `tc_signoffs_sgno` (`CycleOffset`, `empy!AccountingID`, `empy!ID_Manager`) VALUES ('" + CycleOffset.ToString() + "', '" + empno + "', '" + LoginForm.username + "' );";
                    }
                    else
                    {
                        dr["approve"] = "";
                        cmd = "SELECT * from `tc_signoffs_sgno` WHERE `CycleOffset` = '" + CycleOffset.ToString() + "' and `empy!AccountingID` ='" + empno + "';";
                        DataTable dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count > 0)
                            cmd = "DELETE from `tc_signoffs_sgno` WHERE `CycleOffset` = '" + CycleOffset.ToString() + "' and `empy!AccountingID` ='" + empno + "';";
                    }

                    try
                    {
                        G1.update_db_data(cmd);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
            }
            return tchecked;
        }
        /***********************************************************************************************/
        private void CheckOtherGrid(DataTable dt, string empno, bool tchecked)
        {
            //            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string emp = dt.Rows[i]["empno"].ObjToString();
                if (emp == empno)
                {
                    if (tchecked)
                        dt.Rows[i]["approve"] = "Y";
                    else
                        dt.Rows[i]["approve"] = "";
                    break;
                }
            }
        }
        /***********************************************************************************************/
        private void menuAddHolidays_ItemClick(object sender, ItemClickEventArgs e)
        {
            //AddHolidays holidayForm = new AddHolidays();
            //holidayForm.Show();
        }
        /***********************************************************************************************/
        private void menuEmployeeList_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Employees employForm = new Employees(empno);
            //employForm.Show();
        }
        /***********************************************************************************************/
        private void menuSkins_ItemClick(object sender, ItemClickEventArgs e)
        {
        }
        /***********************************************************************************************/
        void skinForm_SkinSelected(string s)
        {
            if (s.ToUpper().IndexOf("SKIN : ") >= 0)
            {
                string skin = s.Replace("Skin : ", "");
                if (skin.Trim().Length == 0)
                    skin = "Windows Default";
                if (skin == "Windows Default")
                {
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                    this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    this.LookAndFeel.SetSkinStyle(skin);
                    //                    this.barAndDockingController1.LookAndFeel.UseDefaultLookAndFeel = false;
                    //                    this.barAndDockingController1.LookAndFeel.SkinName = skin;
                }
            }
            else if (s.ToUpper().IndexOf("COLOR : ") >= 0)
            {
                string color = s.Replace("Color : ", "");
                this.gridMain.Appearance.EvenRow.BackColor = Color.FromName(color);
                this.gridMain.Appearance.EvenRow.BackColor2 = Color.FromName(color);
                this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
            else if (s.ToUpper().IndexOf("NO COLOR ON") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = false;
            }
            else if (s.ToUpper().IndexOf("NO COLOR OFF") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
        }
        /***********************************************************************************************/
        private void ForceVisiblePunchPanel(int punch)
        {
            if (punch == 1)
                bandPunch1.Visible = true;
            else if (punch == 2)
                bandPunch2.Visible = true;
            else if (punch == 3)
                bandPunch3.Visible = true;
            else if (punch == 4)
                bandPunch4.Visible = true;
            else if (punch == 5)
                bandPunch5.Visible = true;
        }
        /***********************************************************************************************/
        private string CheckForNextPunch(string empno, int row, bool force = true, bool previous = false)
        {
            string punch = "BAD";
            DataTable dt = (DataTable)dgv.DataSource;
            string name = "";
            string data = "";
            for (int i = 1; i < 5; i++)
            {
                name = "IN" + i.ToString();
                data = dt.Rows[row][name].ObjToString();
                if (string.IsNullOrWhiteSpace(data))
                {
                    if (force)
                        ForceVisiblePunchPanel(i);
                    if (previous)
                        return "BTNPUNCHOUT" + (i - 1).ToString();
                    return "BTNPUNCHIN" + i.ToString();
                }
                name = "OUT" + i.ToString();
                data = dt.Rows[row][name].ObjToString();
                if (string.IsNullOrWhiteSpace(data))
                {
                    if (force)
                        ForceVisiblePunchPanel(i);
                    if (previous)
                        return "BTNPUNCHIN" + i.ToString();
                    return "BTNPUNCHOUT" + i.ToString();
                }
            }
            return punch;
        }
        /***********************************************************************************************/
        private void DetermineColorPunch(string salaried)
        {
            if (btnAddNextPunch.BackColor == Color.Red)
                btnAddNextPunch.Text = "PUNCH IN";
            else
                btnAddNextPunch.Text = "PUNCH OUT";
        }
        /***********************************************************************************************/
        private void SetPunchButtonColor(DateTime now, string salaried = "")
        {
            DataTable dt = (DataTable)dgv.DataSource;
            //DateTime now = G1.GetCurrentDateTime();
            int row = LocatePunchRow(dt, now);
            if (row < 0)
            {
                btnAddNextPunch.BackColor = Color.Red;
                btnAddNextPunch.ForeColor = Color.White;
                DetermineColorPunch(salaried);
                return;
            }
            string NextPunch = CheckForNextPunch(empno, row);
            if (NextPunch == "BAD")
            {
                btnAddNextPunch.BackColor = Color.Red;
                btnAddNextPunch.ForeColor = Color.White;
                DetermineColorPunch(salaried);
                return;
            }
            if (NextPunch.IndexOf("PUNCHOUT") >= 0)
            {
                btnAddNextPunch.BackColor = Color.Green;
                btnAddNextPunch.ForeColor = Color.White;
            }
            else
            {
                btnAddNextPunch.BackColor = Color.Red;
                btnAddNextPunch.ForeColor = Color.White;
            }

            DetermineColorPunch(salaried);
        }
        public static DateTime UnixTimeStampToDateTime(long ticks)
        {
            // Unix timestamp is seconds past epoch
            double unixTimeStamp = (double)ticks;
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            double dValue = (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            return (long)dValue;
        }
        /***********************************************************************************************/
        private void btnAddNextPunch_Click(object sender, EventArgs e)
        {
            Button button = (Button)(sender);
            DateTime now = DateTime.Now;
            AddNextPunch(button, now);
            button = this.btnAddNextPunch;
        }
        /***********************************************************************************************/
        private void AddNextPunch(Button button, DateTime dateIn)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DateTime now = DateTime.Now;
            now = dateIn;
            long ldate = G1.TimeToUnix(now);
            DateTime nnow = ldate.UnixToDateTime();
            int row = LocatePunchRow(dt, now);
            if (row < 0)
            {
                MessageBox.Show("***ERROR*** Cannot Determine Punch Row! Call IT!");
                return;
            }
            gridMain.FocusedRowHandle = row;
            DataRow dRow = GetCurrentGridRow();
            string empno = dRow["empno"].ObjToString();
            if (String.IsNullOrWhiteSpace(empno))
                empno = workEmpNo;
            //Button button = (Button)(sender);
            string ButtonName = button.Name.ToUpper();
            string NextPunch = CheckForNextPunch(empno, row);
            if (NextPunch == "BAD")
            {
                MessageBox.Show("***ERROR*** Selecting Next Punch. Check All Data Closely or Call IT!");
                return;
            }
            ButtonName = NextPunch;
            bool punchOkay = CheckPreviousPunch(dt, row, ButtonName);
            if (!punchOkay)
                return;
            if (ButtonName.ToUpper().IndexOf("PUNCHIN") > 0)
            {
                AddPunchNow(empno, now);
                string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
                string name = "IN" + ButtonName.ToUpper().Replace("BTNPUNCHIN", "");
                if (string.IsNullOrWhiteSpace(dRow[name].ObjToString()))
                {
                    dRow[name] = time;
                    CheckForErrors(dt, true);
                    SetPunchButtonColor(now);
                }
                else
                {
                    MessageBox.Show("***ERROR*** Punch has already been made!");
                }
            }
            else if (ButtonName.ToUpper().IndexOf("PUNCHOUT") > 0)
            {
                AddPunchNow(empno, now);
                string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
                string name = "OUT" + ButtonName.ToUpper().Replace("BTNPUNCHOUT", "");
                if (string.IsNullOrWhiteSpace(dRow[name].ObjToString()))
                {
                    dRow[name] = time;
                    //                    TotalRow();
                    CleanOutEmployeeTimes(dt);
                    //GetEmployeePunches(empno);
                    dt = (DataTable)dgv.DataSource;
                    CheckForErrors(dt, true);
                    SetPunchButtonColor(now);
                }
                else
                {
                    MessageBox.Show("***ERROR*** Punch has already been made!");
                }
            }
        }
        /***********************************************************************************************/
        private void AddNextManualPunch(Button button, DateTime dateIn)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DateTime now = DateTime.Now;
            now = dateIn;
            long ldate = G1.TimeToUnix(now);
            DateTime nnow = ldate.UnixToDateTime();
            int row = LocatePunchRow(dt, now);
            if (row < 0)
            {
                MessageBox.Show("***ERROR*** Cannot Determine Punch Row! Call IT!");
                return;
            }
            gridMain.FocusedRowHandle = row;
            DataRow dRow = GetCurrentGridRow();
            string empno = dRow["empno"].ObjToString();
            //Button button = (Button)(sender);
            string ButtonName = button.Name.ToUpper();

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;

            string NextPunch = "BTNPUNCHIN1";
            if (currentColumn.IndexOf("in") == 0)
                NextPunch = currentColumn.Replace("in", "BTNPUNCHIN");
            else
                NextPunch = currentColumn.Replace("out", "BTNPUNCHOUT");

            //string NextPunch = CheckForNextPunch(empno, row, true, true );
            if (NextPunch == "BAD")
            {
                MessageBox.Show("***ERROR*** Selecting Next Punch. Check All Data Closely or Call IT!");
                return;
            }
            ButtonName = NextPunch;
            bool punchOkay = CheckPreviousPunch(dt, row, ButtonName);
            if (!punchOkay)
                return;
            if (ButtonName.ToUpper().IndexOf("PUNCHIN") > 0)
            {
                AddPunchNow(empno, now);
                string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
                string name = "IN" + ButtonName.ToUpper().Replace("BTNPUNCHIN", "");
                //if (string.IsNullOrWhiteSpace(dRow[name].ObjToString()))
                //{
                if (time == "00:00")
                    time = "Midnight";
                dRow[name] = time;
                CheckForErrors(dt, true);
                SetPunchButtonColor(now);
                CalcHours(dt, row);
                //}
                //else
                //{
                //    MessageBox.Show("***ERROR*** Punch has already been made!");
                //}
            }
            else if (ButtonName.ToUpper().IndexOf("PUNCHOUT") > 0)
            {
                AddPunchNow(empno, now);
                string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
                string name = "OUT" + ButtonName.ToUpper().Replace("BTNPUNCHOUT", "");
                //if (string.IsNullOrWhiteSpace(dRow[name].ObjToString()))
                //{
                if (time == "00:00")
                    time = "Midnight";
                dRow[name] = time;
                //                    TotalRow();
                //CleanOutEmployeeTimes(dt);
                //GetEmployeePunches(empno);
                dt = (DataTable)dgv.DataSource;
                CheckForErrors(dt, true);
                SetPunchButtonColor(now);
                CalcHours(dt, row);
                //}
                //else
                //{
                //    MessageBox.Show("***ERROR*** Punch has already been made!");
                //}
            }
        }
        /***********************************************************************************************/
        private void CalcHours(DataTable dt, int row)
        {
            string str = "";
            string start = "";
            string stop = "";
            double hours = 0D;
            double totalHours = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double overtime = 0D;
            DateTime timeIn = DateTime.Now;
            DateTime timeOut = DateTime.Now;
            TimeSpan ts;
            DateTime newTime = DateTime.Now;

            for (int i = 1; i <= 5; i++)
            {
                start = "in" + i.ToString();
                stop = "out" + i.ToString();
                str = dt.Rows[row][start].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.ToUpper() == "MIDNIGHT")
                    str = "00:00";
                if (str.IndexOf(":") < 0)
                    str += ":00";
                timeIn = str.ObjToDateTime();
                //timeIn = ValidateTime(timeIn);
                //newTime = new DateTime(timeIn.Year, timeIn.Month, timeIn.Day);
                //ts = timeIn - newTime;

                str = dt.Rows[row][stop].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.ToUpper() == "MIDNIGHT")
                    str = "00:00";
                if (str.IndexOf(":") < 0)
                    str += ":00";
                timeOut = str.ObjToDateTime();
                //timeOut = ValidateTime(timeOut);
                //newTime = new DateTime(timeOut.Year, timeOut.Month, timeOut.Day);
                //ts = timeOut - newTime;
                //if (ts.TotalHours <= 12)
                //    timeOut = timeOut.AddDays(1);

                ts = timeOut - timeIn;
                hours = ts.TotalHours;
                hours = CalculateTime(timeIn, timeOut);
                if (hours > 0D)
                    totalHours += hours;
            }

            double vacation = dt.Rows[row]["vacation"].ObjToDouble();
            double holiday = dt.Rows[row]["holiday"].ObjToDouble();
            double sick = dt.Rows[row]["sick"].ObjToDouble();

            //dt.Rows[row]["hours"] = totalHours + vacation + holiday + sick;
            dt.Rows[row]["hours"] = totalHours;
            if (row <= 6)
                dt.Rows[row]["week1"] = totalHours;
            else
                dt.Rows[row]["week2"] = totalHours;
            dt.Rows[row]["overtime"] = overtime;
            //dt.Rows[row]["total"] = totalHours + vacation + holiday + sick;
            dt.Rows[row]["total"] = totalHours;
        }
        /***********************************************************************************************/
        private void menuDetailReport_ItemClick(object sender, ItemClickEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void menuEditPreferences_ItemClick(object sender, ItemClickEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void chkUnapproved_CheckedChanged(object sender, EventArgs e)
        {
            if (SavedSuperDt == null)
                return;
            if (sender != null)
            {
                CheckBox chk = (CheckBox)(sender);
                if (!chk.Checked)
                {
                    this.Cursor = Cursors.WaitCursor;
                    ReLoadAll();
                    this.Cursor = Cursors.Default;
                    dgv.DataSource = SavedSuperDt;
                    dgv.RefreshDataSource();
                    workingUnapproved = false;
                    return;
                }
            }

            workingUnapproved = true;
            DataTable dx = (DataTable)dgv.DataSource;

            DataTable dt = SavedSuperDt.Clone();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                if (dx.Rows[i]["approve"].ObjToString() != "Y")
                    dt.ImportRow(dx.Rows[i]);
            }
            dt.AcceptChanges();
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private string ActiveTabPage = "";
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            DataTable dt = null;
            TabPage tPage = e.TabPage;
            if (tPage == null)
                return;
            string page = tPage.Name.ToString();
            ActiveTabPage = page;
            //if (page.ToUpper() == "TABPTO")
            //{
            //    RunPTO();
            //    Bitmap emptyImage = new Bitmap(1, 1);
            //    this.picEmployee.Image = emptyImage;
            //    if (workGroup && is_supervisor)
            //        CheckForRequests();
            //}
            //else if (page.ToUpper() == "TABREPORT")
            //{
            //    if (dgv4 != null)
            //    {
            //        if (dgv4.DataSource != null)
            //        {
            //            dt = (DataTable)dgv4.DataSource;
            //            dt.Dispose();
            //            dgv4.DataSource = null;
            //        }
            //    }
            //    btnGenerateReport_Click(null, null);
            //    if (dgv4 != null)
            //    {
            //        if (dgv4.DataSource != null)
            //        {
            //            dt = (DataTable)dgv4.DataSource;
            //            ShowPictureRow(dt, 0);
            //        }
            //    }
            //    if (workGroup && is_supervisor)
            //        CheckForRequests();
            //}
            if (page.ToUpper() == "TABMAIN")
            {
                dt = (DataTable)dgv.DataSource;
                if (dt != null)
                {
                    dt = AddUpOther(dt);
                    int row = gridMain.FocusedRowHandle;
                    ShowPictureRow(dt, row);
                    if (workGroup && is_supervisor)
                        CheckForRequests();
                    btnAddPunch.Visible = true;
                    btnAddPunch.Refresh();
                }
            }
            else if (page.ToUpper() == "TABCONTRACTLABOR")
            {
                btnAddPunch.Visible = false;
                btnAddPunch.Refresh();
            }
            else if (page.ToUpper() == "TABPAGEOTHER")
            {
                btnAddPunch.Visible = false;
                btnAddPunch.Refresh();
            }
            else if (page.ToUpper() == "TABMYTIMEOFF")
            {
                LoadMyTimeOffRequests();
                if (workGroup && is_supervisor)
                    CheckForRequests();
                btnAddPunch.Visible = false;
                btnAddPunch.Refresh();
            }
            //else if (page.ToUpper() == "TABTIMEOFFPROC")
            //{
            //    LoadTimeOffRequests();
            //    if (workGroup && is_supervisor)
            //        CheckForRequests();
            //}
        }
        /***********************************************************************************************/
        private DataTable AddUpOther(DataTable dt)
        {
            DateTime date = DateTime.Now;
            DateTime timesheetDate = DateTime.Now;
            double otherPay = 0D;
            double pay = 0D;

            DataTable dx = (DataTable)dgv8.DataSource;

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["other"] = 0D;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                date = dx.Rows[i]["date"].ObjToDateTime();
                pay = dx.Rows[i]["paymentAmount"].ObjToDouble();
                if (pay <= 0D)
                    continue;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    timesheetDate = dt.Rows[j]["date"].ObjToDateTime();
                    if (timesheetDate == date)
                    {
                        otherPay = dt.Rows[j]["other"].ObjToDouble() + pay;
                        //if ( G1.isHR() )
                            dt.Rows[j]["other"] = otherPay;
                    }
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void RunPTO()
        {
            DataTable timDt = (DataTable)dgv.DataSource;

            LoadPTOData();

            DataTable dt = (DataTable)dgv2.DataSource;

            string empno = "";
            double pto = 0D;
            double docked = 0D;
            string note = "";
            string approve = "";

            for (int i = (timDt.Rows.Count - 1); i >= 0; i--)
            {
                empno = timDt.Rows[i]["empno"].ObjToString();

                DataRow[] dRow = dt.Select("empno = '" + empno + "'");
                if (dRow.Length > 0)
                {
                    approve = timDt.Rows[i]["approve"].ObjToString();
                    if (approve.ToUpper() == "Y")
                    {
                        pto = timDt.Rows[i]["qpto"].ObjToDouble();
                        dRow[0]["pto"] = pto;
                        docked = timDt.Rows[i]["docked"].ObjToDouble();
                        dRow[0]["docked"] = docked;
                        note = timDt.Rows[i]["notes"].ObjToString();
                        dRow[0]["comment"] = note;
                        dRow[0]["approve"] = approve;
                        if (pto == 0D && docked == 0D)
                        {
                            dt.Rows.Remove(dRow[0]);
                        }
                    }
                    else
                    {
                        dt.Rows.Remove(dRow[0]);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void SetReportApprovals(DataTable dt)
        {
            string approve = "";
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit3;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approve = dt.Rows[i]["approve"].ObjToString();
                if (approve.ToUpper() != "Y")
                    dt.Rows[i]["approve"] = "";
            }
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";
        }
        /***********************************************************************************************/
        private void SetPTOApprovals(DataTable dt)
        {
            string approve = "";
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                approve = dt.Rows[i]["approve"].ObjToString();
                if (approve.ToUpper() != "Y")
                    dt.Rows[i]["approve"] = "";
            }
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";
        }
        /***********************************************************************************************/
        DataTable ptoDt = null;
        private void LoadPTOData()
        {
            //if ( ptoDt != null )
            //{
            //    return;
            //}
            string cmd = "Select * from `er` where `empno` > '100' and `empno` <> '500' and `status` <> 'deactivate';";
            DataTable oldDt = G1.get_db_data(cmd);
            if (oldDt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot locate employees!");
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("record");
            dt.Columns.Add("empno");
            dt.Columns.Add("emrno");
            dt.Columns.Add("name");
            dt.Columns.Add("hiredate");
            dt.Columns.Add("birthdate");
            dt.Columns.Add("pto_now", Type.GetType("System.Double"));
            dt.Columns.Add("pto_inc", Type.GetType("System.Double"));
            dt.Columns.Add("december", Type.GetType("System.Double"));
            dt.Columns.Add("status");
            dt.Columns.Add("vacation", Type.GetType("System.Double"));
            dt.Columns.Add("pto", Type.GetType("System.Double"));
            dt.Columns.Add("holiday", Type.GetType("System.Double"));
            dt.Columns.Add("sick", Type.GetType("System.Double"));
            dt.Columns.Add("other", Type.GetType("System.Double"));
            dt.Columns.Add("no_pay", Type.GetType("System.Double"));
            dt.Columns.Add("docked", Type.GetType("System.Double"));
            dt.Columns.Add("other", Type.GetType("System.Double"));
            dt.Columns.Add("bank", Type.GetType("System.Double"));
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("medical");
            dt.Columns.Add("dental");
            dt.Columns.Add("pto_taken", Type.GetType("System.Double"));
            dt.Columns.Add("comment");
            dt.Columns.Add("approve");

            double pto = 0D;
            double pto_inc = 0D;
            double pto_new = 0D;
            string status = "";
            double vacation = 0D;
            double holiday = 0D;
            double sick = 0D;
            double no_pay = 0D;
            double other = 0D;
            double bank = 0D;
            double paid = 0D;
            string medical = "";
            string dental = "";
            string record = "";
            string parttime = "";
            string str = "";
            Bitmap emptyImage = new Bitmap(1, 1);

            for (int i = 0; i < oldDt.Rows.Count; i++)
            {
                string empno = oldDt.Rows[i]["empno"].ObjToString();
                cmd = "Select * from `er` where `empno` = '" + empno + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                parttime = dx.Rows[0]["parttime"].ObjToString();
                if (!string.IsNullOrWhiteSpace(parttime))
                {
                    str = parttime.Substring(0, 1);
                    if (str.ToUpper() == "Y")
                        continue; // Don't load empty Parttime people
                }
                string name = dx.Rows[0]["name"].ObjToString();
                DateTime bdate = dx.Rows[0]["birthdate"].ObjToDateTime();
                DateTime hdate = dx.Rows[0]["hiredate"].ObjToDateTime();
                DateTime tdate = dx.Rows[0]["termdate"].ObjToDateTime();

                if (tdate.ToString().IndexOf("0001") < 0)
                    continue;
                record = oldDt.Rows[i]["record"].ObjToString();
                pto = oldDt.Rows[i]["pto_now"].ObjToDouble();
                pto_inc = oldDt.Rows[i]["pto_inc"].ObjToDouble();
                pto_new = oldDt.Rows[i]["december"].ObjToDouble();
                status = oldDt.Rows[i]["status"].ObjToString().ToUpper();
                vacation = oldDt.Rows[i]["vacation"].ObjToDouble();
                holiday = oldDt.Rows[i]["holiday"].ObjToDouble();
                sick = oldDt.Rows[i]["sick"].ObjToDouble();
                no_pay = oldDt.Rows[i]["no_pay"].ObjToDouble();
                other = oldDt.Rows[i]["other"].ObjToDouble();
                bank = oldDt.Rows[i]["bank"].ObjToDouble();
                paid = oldDt.Rows[i]["paid"].ObjToDouble();
                medical = oldDt.Rows[i]["medical"].ObjToString();
                dental = oldDt.Rows[i]["dental"].ObjToString();

                DataRow dRow = dt.NewRow();
                dRow["record"] = record;
                dRow["empno"] = empno;
                dRow["name"] = name;
                dRow["hiredate"] = hdate.ToString("MM/dd/yyyy");
                dRow["birthdate"] = bdate.ToString("MM/dd/yyyy");
                dRow["pto_now"] = pto;
                dRow["pto_inc"] = pto_inc;
                dRow["december"] = pto_new;
                dRow["status"] = status;
                dRow["vacation"] = vacation;
                dRow["holiday"] = holiday;
                dRow["sick"] = sick;
                dRow["no_pay"] = no_pay;
                dRow["other"] = other;
                dRow["bank"] = bank;
                dRow["paid"] = paid;
                dRow["medical"] = medical;
                dRow["dental"] = dental;
                dt.Rows.Add(dRow);
            }
            NumberDataTable(dt);
            SetPTOApprovals(dt);
            dgv2.DataSource = dt;
            //            ptoDt = dt;
        }
        /***********************************************************************************************/
        private void NumberDataTable(DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void UpdateEmployeeNotes(string empno)
        {
            if (cellRowHandle >= 0)
            {
                if (cellColumn.ToUpper() == "NOTES")
                {
                    string notes = cellChanging;
                    CellValueChangedEventArgs e = new CellValueChangedEventArgs(cellRowHandle, this.bandedGridColumn23, notes);
                    DataTable dt = (DataTable)dgv.DataSource;
                    dt.Rows[cellRowHandle]["notes"] = notes;
                    gridMain_CellValueChanged(null, e);
                }
            }
            cellRowHandle = -1;
            cellColumn = "";
            cellChanging = "";
        }
        /***********************************************************************************************/
        private void UpdateCycleNote()
        {
            string empno = "";
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            try
            {
                int cycle = DetermineCycle();
                empno = dt.Rows[0]["empno"].ObjToString();
                string note = txtCycleNote.Text.Trim();
                note = G1.protect_data(note);
                string cmd = "Select * from `tc_cyclenotes_cnot` where `CycleOffset` = '" + cycle + "' ";
                cmd += " and `empy!AccountingID` = '" + empno + "' ";
                cmd += "order by `empy!AccountingID`,`CycleOffset`;";
                DataTable dd = G1.get_db_data(cmd);
                if (dd.Rows.Count <= 0)
                {
                    cmd = "INSERT INTO `tc_cyclenotes_cnot` (`CycleOffset`, `empy!AccountingID`, `Note`) VALUES ('" + cycle.ToString() + "', '" + empno + "', '" + note + "' );";
                    DataTable ddd = G1.get_db_data(cmd);
                }
                else
                {
                    cmd = "Update `tc_cyclenotes_cnot` Set `Note` = '" + note + "' where `CycleOffset` = '" + cycle.ToString() + "' and `empy!AccountingID` = '" + empno + "';";
                    DataTable ddd = G1.get_db_data(cmd);
                }
                UpdateEmployeeNotes(empno);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Cycle Notes on Employee " + empno + "!");
            }
        }
        /***********************************************************************************************/
        private string GetCycleNote(string empno, int cycle)
        {
            string note = "";
            string cmd = "Select * from `tc_cyclenotes_cnot` where `CycleOffset` = '" + cycle + "' ";
            cmd += " and `empy!AccountingID` = '" + empno + "' ";
            cmd += "order by `empy!AccountingID`,`CycleOffset`;";
            DataTable dd = G1.get_db_data(cmd);
            if (dd.Rows.Count > 0)
                note = dd.Rows[0]["Note"].ObjToString();
            return note;
        }
        /***********************************************************************************************/
        private void menuSetPassword_ItemClick(object sender, ItemClickEventArgs e)
        {
            //int tryCount = 0;
            //string cmd = "Select * from `er` where `empno` = '" + cUser.UserID + "';";
            //DataTable dt = G1.get_db_data(cmd);
            //if (dt.Rows.Count <= 0)
            //{
            //    MessageBox.Show("***ERROR*** Problem with existing user! Call I/T!");
            //    return;
            //}
            //string oldHash = dt.Rows[0]["pwHash"].ObjToString();
            //for (;;)
            //{
            //    string oldPassword = "";
            //    using (Ask fmrmyform = new Ask("Please Enter Your Current Password > "))
            //    {
            //        fmrmyform.Text = "";
            //        fmrmyform.ShowDialog();
            //        oldPassword = fmrmyform.Answer.Trim();
            //        string hash = MySQL.CalculateMD5Hash(oldPassword);
            //        if (hash != oldHash)
            //        {
            //            tryCount++;
            //            if (tryCount >= 3)
            //            {
            //                MessageBox.Show("***ERROR*** Too many attempts to change pw! Getting out or call I/T for help!");
            //                return;
            //            }
            //            MessageBox.Show("***Warning*** Wrong Current Password!\nTry Again!");
            //            continue;
            //        }
            //        break;
            //    }
            //}
            //sqlUserObject userInfo = new sqlUserObject() { userid = cUser.UserID };
            //Password pform = new Password(userInfo);
            //pform.ShowDialog();
        }
        /***********************************************************************************************/
        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            DataTable dx = dt.Copy();
            NumberDataTable(dx);
            SetReportApprovals(dx);
            LoadNoteDates(dx);
            LoadEmployeeSupervisors(dx);
            SavedReportDt = dx;
            SavedReportRow = -1;
            dgv4.DataSource = dx;
        }
        /***********************************************************************************************/
        private void LoadNoteDates(DataTable dt)
        {
            if (1 == 1)
                return; // Perhaps not needed as long as notes is in with the punches
            string notes = "";
            string note2 = "";
            string note3 = "";
            string note4 = "";
            string note5 = "";
            string word = "";
            string note = "";
            string cyclenotes = "";
            string empno = "";
            string cmd = "";

            Font f = gridMain4.Columns["notes"].AppearanceCell.Font;

            int noteWidth = DetermineCommentWidth(gridMain4.Columns["notes"].Width, f);
            int spacerWidth = DetermineCommentWidth(gridMain4.Columns["spacer"].Width, f);
            int space2Width = DetermineCommentWidth(gridMain4.Columns["spacer2"].Width, f);
            int space3Width = DetermineCommentWidth(gridMain4.Columns["spacer3"].Width, f);
            int space4Width = DetermineCommentWidth(gridMain4.Columns["spacer4"].Width, f);


            if (G1.get_column_number(dt, "spacer") < 0)
                dt.Columns.Add("spacer");
            if (G1.get_column_number(dt, "spacer2") < 0)
                dt.Columns.Add("spacer2");
            if (G1.get_column_number(dt, "spacer3") < 0)
                dt.Columns.Add("spacer3");
            if (G1.get_column_number(dt, "spacer4") < 0)
                dt.Columns.Add("spacer4");
            int CycleOffset = DetermineCycle();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                empno = dt.Rows[i]["empno"].ObjToString();
                notes = dt.Rows[i]["notes"].ObjToString();
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    cmd = "Select * from `tc_notesday_dnot` where `UnixTime` >= '" + saveReportLdate + "' and `UnixTime` <= '" + saveReportEdate + "' ";
                    cmd += " and `empy!AccountingID` = '" + empno + "' ";
                    cmd += "order by `empy!AccountingID`,`UnixTime`;";

                    DataTable dd = G1.get_db_data(cmd);
                    long ldate = 0L;
                    DateTime dayDate = DateTime.Now;
                    notes = "";
                    for (int j = 0; j < dd.Rows.Count; j++)
                    {
                        ldate = dd.Rows[j]["UnixTime"].ObjToInt64();
                        DateTime date = ldate.UnixToDateTime();
                        note = dd.Rows[j]["Note"].ObjToString();
                        if (string.IsNullOrWhiteSpace(note))
                            continue;
                        notes += date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + note + " ";
                        //                        dt.Rows[i]["notes"] = notes;
                    }
                    string[] Lines = notes.Split(' ');
                    notes = "";
                    note2 = "";
                    note3 = "";
                    note4 = "";
                    note5 = "";
                    int pass = 1;
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        word = Lines[j].Trim();
                        if (string.IsNullOrWhiteSpace(word))
                            continue;
                        if (pass == 1)
                        {
                            if ((notes.Length + word.Length) > noteWidth)
                                pass = 2;
                            else
                                notes += word + " ";
                        }
                        if (pass == 2)
                        {
                            if ((note2.Length + word.Length) > spacerWidth)
                                pass = 3;
                            else
                                note2 += word + " ";
                        }
                        if (pass == 3)
                        {
                            if ((note3.Length + word.Length) > space2Width)
                                pass = 4;
                            else
                                note3 += word + " ";
                        }
                        if (pass == 4)
                        {
                            if ((note4.Length + word.Length) > space3Width)
                                pass = 5;
                            else
                                note4 += word + " ";
                        }
                        if (pass == 5)
                            note5 += word + " ";
                    }
                    dt.Rows[i]["notes"] = notes;
                    dt.Rows[i]["spacer"] = note2;
                    if (!string.IsNullOrWhiteSpace(note3))
                        dt.Rows[i]["spacer2"] = note3;
                    if (!string.IsNullOrWhiteSpace(note4))
                        dt.Rows[i]["spacer3"] = note4;
                    if (!string.IsNullOrWhiteSpace(note5))
                        dt.Rows[i]["spacer4"] = note5;
                }
            }
        }
        /***********************************************************************************************/
        private int DetermineCommentWidth(int pixelWidth, Font f)
        {
            SizeF size;
            string str = "";
            int numberOfCharacters = 0;
            using (Graphics g = dgv4.CreateGraphics())
            {
                StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
                while (true)
                {
                    str += "a";
                    size = g.MeasureString(str, f);
                    if (size.Width > pixelWidth)
                        break;
                    numberOfCharacters++;
                }
            }
            return numberOfCharacters;
        }
        /***********************************************************************************************/
        private void LoadEmployeeSupervisors(DataTable timDt)
        {
            string cmd = "Select * from `jobs` order by `super`,`jobcode`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot locate employees!");
                return;
            }

            string super = "";
            string jobcode = "";
            string oldjobcode = "";
            string tmpJob = "";

            if (G1.get_column_number(timDt, "supervisor") < 0)
                timDt.Columns.Add("supervisor");

            FillJobCodes(timDt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    if (i == 48)
                    {
                    }
                    jobcode = dt.Rows[i]["jobcode"].ObjToString();
                    if (jobcode == oldjobcode)
                        continue;
                    oldjobcode = jobcode;
                    super = dt.Rows[i]["super"].ObjToString();

                    cmd = "Select * from `er` where `empno` = '" + super + "';";
                    DataTable sups = G1.get_db_data(cmd);
                    if (sups.Rows.Count > 0)
                    {
                        super = sups.Rows[0]["name"].ObjToString();
                        string[] Lines = super.Split(' ');
                        if (Lines.Length > 0)
                            super = Lines[0].Trim();
                    }

                    for (int k = 0; k < timDt.Rows.Count; k++)
                    {
                        tmpJob = timDt.Rows[k]["jobcode"].ObjToString();
                        if (tmpJob == jobcode)
                            timDt.Rows[k]["supervisor"] = super;
                    }

                    //DataRow[] dRows = timDt.Select("jobcode=" + jobcode); // Somehow this caused an exception on i=48, jobcode 46
                    //for (int j = 0; j < dRows.Length; j++)
                    //    dRows[j]["supervisor"] = super;
                }
                catch (Exception ex)
                {
                }
            }
            string emp = "";
            cmd = "Select * from `er` where `preferred_supervisor` <> '';";
            dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                super = dt.Rows[i]["preferred_supervisor"].ObjToString();
                emp = dt.Rows[i]["empno"].ObjToString();
                DataRow[] dRows = timDt.Select("`empno` = '" + emp + "'");
                for (int j = 0; j < dRows.Length; j++)
                    dRows[j]["supervisor"] = super;
            }
        }
        /***********************************************************************************************/
        private void FillJobCodes(DataTable dt)
        {
            string jobcode = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jobcode = dt.Rows[i]["jobcode"].ObjToString();
                if (string.IsNullOrWhiteSpace(jobcode))
                    dt.Rows[i]["jobcode"] = "0";
            }
        }
        /***********************************************************************************************/
        private void menuReportPrintPreview_ItemClick(object sender, ItemClickEventArgs e)
        { // To Do
        }
        /***********************************************************************************************/
        private void menuReportPrint_ItemClick(object sender, ItemClickEventArgs e)
        { // To Do
        }
        /***********************************************************************************************/
        private void picEmployee_Click(object sender, EventArgs e)
        {
            if (this.timer1.Enabled)
                timer1_Tick(null, null);
            Bitmap map = (Bitmap)(picEmployee.Image);
            ShowPicture(map);
        }
        /***********************************************************************************************/
        private void menuEditHourStatus_ItemClick(object sender, ItemClickEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void swapPTODockedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridView2.GetFocusedDataRow();
            double docked = dr["qdocked"].ObjToDouble();
            double pto = dr["pto"].ObjToDouble();
            dr["pto"] = docked;
            dr["qdocked"] = pto;
            gridView2.RefreshData();
        }
        /***********************************************************************************************/
        private string cellChanging = "";
        private string cellColumn = "";
        private int cellRowHandle = -1;
        private void gridMain_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            cellColumn = e.Column.ObjToString();
            cellRowHandle = e.RowHandle;
            cellChanging = e.Value.ObjToString();
        }
        /***********************************************************************************************/
        private void AddDgvLine(DataTable dt, string line)
        {
            DataRow dR = dt.NewRow();
            dR["line"] = line;
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private void AddDgvLine2(DataTable dt, string line)
        {
            string[] Lines = line.Split('\n');
            for (int i = 0; i < Lines.Length; i++)
            {
                line = Lines[i].Trim();
                DataRow dR = dt.NewRow();
                dR["line"] = line;
                rtb.AppendText(line + "\n");
                dt.Rows.Add(dR);
            }
        }
        /***********************************************************************************************/
        private string LoadLine(string param1, string answer1, string param2 = "", string answer2 = "")
        {
            if (answer1 == "0")
                answer1 = "0.00";
            string line = "| ";
            line += param1.PadRight(20, '.');
            line += ": " + answer1;
            line = line.PadRight(39);
            if (!String.IsNullOrWhiteSpace(param2))
            {
                if (answer2 == "0")
                    answer2 = "0.00";
                line += param2.PadRight(14, '.');
                line += ": " + answer2;
            }
            //            line = line.PadRight(78) + "|";
            return line;
        }
        /***********************************************************************************************/
        private void btnDoDetail_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            rtb.Clear();

            string empno = "";
            string name = "";
            double pto = 0D;
            double pto_inc = 0D;
            double pto_new = 0D;
            string status = "";
            double vacation = 0D;
            double holiday = 0D;
            double sick = 0D;
            double no_pay = 0D;
            double other = 0D;
            double bank = 0D;
            double paid = 0D;
            string medical = "";
            string dental = "";
            string cmd = "";
            string record = "";
            string dow = "";
            DateTime date;
            string line = "";
            string error = "";
            string detail = "";

            long lastTime = 0L;
            int groupRow = 0;
            double week1 = 0D;
            double week2 = 0D;
            long diff = 0L;

            long firsttime = 0L;
            long lasttime = 0L;
            DateTime lastDtime;

            int count = 0;
            int totalCount = 0;
            int over6 = 0;

            DataTable dgvDt = new DataTable();
            dgvDt.Columns.Add("line");

            string dash = "===============================================================================";

            DataTable dt = (DataTable)(dgv.DataSource);

            DateTime timePeriod = dateTimePicker1.Value;

            long ldate = G1.TimeToUnix(timePeriod);
            timePeriod = timePeriod.AddDays(7D);
            long hdate = G1.TimeToUnix(timePeriod);
            timePeriod = timePeriod.AddDays(7D);
            timePeriod = timePeriod.AddMinutes(-1); // This gets the time back to 23:59:00
            long edate = G1.TimeToUnix(timePeriod);

            DateTime firstDate = ldate.UnixToDateTime();
            DateTime lastDate = edate.UnixToDateTime();

            DateTime saveFirstDate = firstDate;
            DateTime saveLastDate = lastDate;

            long saveLdate = ldate;
            long saveEdate = edate;

            bool printed = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //if (i >= 12) // For testing
                //    break;
                empno = dt.Rows[i]["empno"].ToString();
                if (String.IsNullOrWhiteSpace(empno))
                    continue;
                cmd = "Select * from `tc_punches_pchs` where `UTS_Added` >= '" + saveLdate + "' and `UTS_Added` <= '" + saveEdate + "' ";
                if (!String.IsNullOrWhiteSpace(empno))
                    cmd += " and `empy!AccountingID` = '" + empno + "' ";

                cmd += "order by `empy!AccountingID`,`UTS_Added`;";

                DataTable ddx = G1.get_db_data(cmd);
                //                DataTable empDt = GetEmployeePunches(empno);
                if (ddx == null)
                    continue;
                count = 0;
                for (int j = 0; j < ddx.Rows.Count; j++)
                {
                    bool deleted = ddx.Rows[j]["deleted"].ObjToBool();
                    if (deleted)
                        continue;
                    count++;
                }
                cmd = "Select * from `er` where `empno` = '" + empno + "';";
                DataTable empDt = G1.get_db_data(cmd);
                if (empDt.Rows.Count > 0)
                {
                    name = empDt.Rows[0]["name"].ObjToString();
                    if (empDt.Rows[0]["salaried"].ObjToString().ToUpper() == "S")
                        continue;
                }
                try
                {
                    detail = "";
                    status = "";
                    count = 0;
                    over6 = 0;
                    week1 = 0D;
                    week2 = 0D;
                    error = "NO";
                    totalCount = 0;
                    for (int j = 0; j < ddx.Rows.Count; j++)
                    {
                        bool deleted = ddx.Rows[j]["Deleted"].ObjToBool();
                        if (deleted)
                            continue;
                        totalCount++;
                        bool manual = ddx.Rows[j]["ManualEntry"].ObjToBool();
                        string emp = ddx.Rows[j]["empy!AccountingID"].ObjToString();
                        ldate = ddx.Rows[j]["UTS_Added"].ObjToInt64();
                        ldate = ConvertUTS(ldate); // Trim Seconds
                        date = ldate.UnixToDateTime();
                        dow = date.DayOfWeek.ToString();
                        if (count == 0)
                        {
                            if (count > 6)
                                over6 = count;
                            week1 = 0D;
                            week2 = 0D;
                            status = "in";
                            firsttime = ldate;
                            lastDtime = date;
                            firstDate = date;
                            lastTime = ldate;
                            count = 1;
                            error = "YES";
                            detail += "\n| DateTime: " + date + " " + status + " ";
                        }
                        else
                        {
                            lasttime = ldate;
                            long jdate = dt_to_days(firstDate);
                            long kdate = dt_to_days(date);
                            if (kdate != jdate)
                            {
                                if (count > 6)
                                    over6 = count;
                                detail += "|  Punches = " + count.ToString() + "\n";
                                status = "in";
                                firsttime = ldate;
                                lastDtime = date;
                                firstDate = date;
                                count = 1;
                                error = "YES";
                                detail += "| \n| DateTime: " + date + " " + status + " ";
                            }
                            else
                            {
                                if (status == "in")
                                {
                                    double dHours = (double)(ldate - firsttime) / 3600D;
                                    double localHours = dHours;
                                    status = "out";
                                    lastDtime = date;
                                    DateTime localTime = ldate.UnixToDateTime();
                                    TimeSpan ts = localTime - saveFirstDate;
                                    if (ts.TotalDays < 7)
                                        week1 += RoundValue(localHours);
                                    else
                                        week2 += RoundValue(localHours);
                                    error = "NO";
                                    //                                    detail += "| DateTime: " + date + " " + status + "\n";
                                    int idx = date.ToString().IndexOf(' ');
                                    string outtime = date.ToString();
                                    if (idx > 0)
                                        outtime = date.ToString().Substring(idx).Trim();
                                    //                                    detail += date + " " + status + "\n";
                                    detail += outtime + " " + status + "\n";
                                    count++;
                                }
                                else
                                {
                                    status = "in";
                                    lastDtime = date;
                                    firstDate = date;
                                    firsttime = ldate;
                                    error = "YES";
                                    count++;
                                    detail += "| DateTime: " + date + " " + status + " ";
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }

                if (count > 6)
                    over6 = count;
                detail += "|  Punches = " + count.ToString() + "\n";

                double totalHours = week1 + week2;
                bool report = false;
                if (chkAll.Checked)
                    report = true;
                else
                {
                    if (chkErrors.Checked && error == "YES")
                        report = true;
                    if (chkUnder80.Checked && totalHours < 80D)
                        report = true;
                    if (chkOddPunches.Checked && (totalCount % 2) == 1)
                        report = true;
                    if (chkOddPunches.Checked && over6 > 6)
                        report = true;
                }
                detail += "| Week1=" + week1.ToString("###.00") + " Week2=" + week2.ToString("###.00") + " Total=" + totalHours.ToString("###.00") + "\n";
                detail += "| ERROR=" + error + " Total Punches = " + totalCount.ToString() + "\n";
                if (report)
                {
                    rtb.AppendText("\n" + dash + "\n");
                    AddDgvLine(dgvDt, dash);

                    line = LoadLine("Employee Number", empno);
                    rtb.AppendText(line + "\n");
                    AddDgvLine(dgvDt, line);

                    line = LoadLine("Employee Name", name);
                    rtb.AppendText(line + "\n");
                    AddDgvLine(dgvDt, line);

                    AddDgvLine2(dgvDt, detail);

                    printed = true;
                }
            }
            if (!printed)
            {
                rtb.AppendText("\n" + dash + "\n");
                AddDgvLine(dgvDt, dash);

                line = "Nothing to Report!";
                rtb.AppendText(line + "\n");
                AddDgvLine(dgvDt, line);
            }
            dgv3.DataSource = dgvDt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void chkErrors_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }
        /***********************************************************************************************/
        private void chkUnder80_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }
        /***********************************************************************************************/
        private void chkOddPunches_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }
        /***********************************************************************************************/
        private void menuEditHelp_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            //EMR.EMR_Help(true, "TimeClock");
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void menuHelpItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            string salaried = GetSalaried(LoginForm.username);
            if (is_supervisor)
            {
                this.Cursor = Cursors.WaitCursor;
                //EMR.EMR_Help(false, "TimeClock", "Supervisor Time CLock");
                this.Cursor = Cursors.Default;
            }
            else if (salaried == "S")
            {
                this.Cursor = Cursors.WaitCursor;
                //EMR.EMR_Help(false, "TimeClock", "Salary Time CLock");
                this.Cursor = Cursors.Default;
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                //EMR.EMR_Help(false, "TimeClock", "Employee Time CLock");
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            //            string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2") + ":" + now.Second.ToString("D2");
            string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2") + ":" + now.Second.ToString("D2");
            string text = this.Text;
            text = "";
            int idx = text.IndexOf("Current Time :");
            if (idx >= 0)
                text = text.Substring(0, idx).Trim();
            text += " Current Time : " + time;

            string currentTime = text;

            //string outNow = IfClockedOutNow();
            //if (!String.IsNullOrWhiteSpace(outNow))
            //    text += " ClockOut Now = " + outNow + " Hours";

            this.Text = "TimeSheet for " + empName + " " + text;
            //MinuteToEndOfDay(outNow, currentTime );
        }
        /***********************************************************************************************/
        private void MinuteToEndOfDay(string outNow, string currentTime)
        {
            if (btnAddNextPunch.BackColor == Color.Red)
                return;
            string text = "";
            double outHours = outNow.ObjToDouble();
            //            outHours = 2.23D;
            outHours -= 8D;
            bool negate = false;
            if (outHours < 0D)
                negate = true;
            int hours = (int)(outHours);
            outHours = outHours - (hours);
            if (outHours < 0D)
                negate = true;
            int minutes = (int)(outHours * 60D);
            minutes = Math.Abs(minutes);
            if (negate)
                text = "    Time Left = " + Math.Abs(hours) + ":" + minutes.ToString("D2");
            else
            {

                text = "    Time Over = " + Math.Abs(hours) + ":" + minutes.ToString("D2");
            }
            this.Text = this.Text + text;
        }
        /***********************************************************************************************/
        private string IfClockedOutNow()
        {
            if (btnAddNextPunch.BackColor == Color.Red)
                return "";
            DataTable dt = (DataTable)dgv.DataSource;
            DateTime now = DateTime.Now;
            long ldate = G1.TimeToUnix(now);
            DateTime nnow = ldate.UnixToDateTime();
            int row = LocatePunchRow(dt, now);
            if (row < 0)
                return "";
            gridMain.FocusedRowHandle = row;
            DataRow dRow = GetCurrentGridRow();
            string empno = dRow["empno"].ObjToString();
            string NextPunch = CheckForNextPunch(empno, row, false);
            if (NextPunch == "BAD")
                return "";

            bool punchOkay = CheckPreviousPunch(dt, row, NextPunch);
            if (!punchOkay)
                return "";

            string backpunch = NextPunch;
            backpunch = backpunch.Replace("BTNPUNCHOUT", "IN");

            double Hours = 0D;
            double TotalHours = 0D;

            for (int i = 1; i < 5; i++)
            {
                string punch = "IN" + i.ToString();
                string clockIn = dRow[punch].ObjToString();
                DateTime timein = clockIn.ObjToDateTime();
                if (backpunch == punch)
                {
                    string time = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2");
                    TimeSpan tss = now - timein;
                    TotalHours += tss.TotalHours;
                    break;
                }
                punch = "OUT" + i.ToString();
                string clockOut = dRow[punch].ObjToString();
                DateTime timeout = clockOut.ObjToDateTime();
                TimeSpan ts = timeout - timein;
                TotalHours += ts.TotalHours;
            }
            return TotalHours.ToString("###.00");
        }
        /***********************************************************************************************/
        private void showPTOHistoryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (workGroup)
            {
                DataRow dr = gridMain.GetFocusedDataRow();
                string empno = dr["empno"].ObjToString();
                string name = dr["name"].ObjToString();
                //ptohistory ptoForm = new ptohistory(empno, name);
                //ptoForm.Show();
            }
        }
        /***********************************************************************************************/
        private void btnRequestTimeOff_Click(object sender, EventArgs e)
        {
            //double ptonow = txtAvailablePTO.Text.ObjToDouble();
            //double december = CalculateYearEndDecember(workUserName);
            //DataTable dt = (DataTable)dgv5.DataSource;
            //TimeOffRequest timeForm = new TimeOffRequest(workUserName, empName, superList, ptonow, december, this.dateTimePicker1.Value, dt );
            //timeForm.ShowDialog();
            //LoadMyTimeOffRequests();
            DataTable dt = (DataTable)dgv5.DataSource;

            DataRow dR = dt.NewRow();
            dR["date"] = DateTime.Now;
            dR["empno"] = workUserName;
            dR["name"] = workMyName;
            dR["approved"] = "";
            dR["approvedBy"] = "";
            dR["hours"] = 0D;
            dR["comment"] = "";
            dR["pto_taken"] = 0D;
            dR["pto_now"] = 0D;
            dR["december"] = 0D;
            dt.Rows.Add(dR);

            G1.NumberDataTable(dt);

            dgv5.DataSource = dt;
            dgv5.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        { // ramma zamma
            if (!is_supervisor)
            {
                DataRow dr = gridView5.GetFocusedDataRow();
                dr["approved"] = "";
                gridView5.RefreshEditor(true);
            }
        }
        /***********************************************************************************************/
        private bool ApproveDisApproveTime6(object sender, DataRow dr)
        {
            string empno = dr["empno"].ObjToString();
            string record = dr["record"].ObjToString();

            string ColumnName = gridView6.FocusedColumn.FieldName.ToUpper();

            bool tchecked = false;

            CheckEdit checkbox = (CheckEdit)sender;
            if (checkbox.Checked)
                tchecked = true;
            if (ColumnName.Trim().ToUpper() == "APPROVED")
            {
                if (!is_supervisor)
                {
                    MessageBox.Show("***WARNING*** You are not setup for Approving Time Off Requests!!!");
                    return false;
                }
                string supervisor = LoginForm.username;
                if (tchecked)
                {
                    dr["approved"] = "Y";
                    dr["approvedby"] = supervisor;
                    G1.update_db_table("tc_timerequest", "record", record, new string[] { "approved", "Y", "approved_by", supervisor });
                }
                else
                {
                    dr["approved"] = "";
                    dr["approvedby"] = "";
                    G1.update_db_table("tc_timerequest", "record", record, new string[] { "approved", "", "approved_by", "" });
                }
            }
            gridView6.RefreshData();
            return tchecked;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit5_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridView6.GetFocusedDataRow();
            string empno = dr["empno"].ObjToString();

            bool tchecked = ApproveDisApproveTime6(sender, dr);

            gridView6.RefreshData();
            dgv6.Refresh();
        }
        /***********************************************************************************************/
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (dgv6.Visible)
                G1.SpyGlass(gridView6);
        }
        /***********************************************************************************************/
        private void cmbMyProc_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTimeOffRequests();
        }
        /***********************************************************************************************/
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color backColor = System.Drawing.SystemColors.ControlText;
            string tabName = tabControl1.TabPages[e.Index].Text;

            /* some logic to determine the color of the text - basically if the name
            contains an asterix make the text red, then make a weak effort to remove the
            asterix from what we actually show
            */
            if (tabName.IndexOf("*") > 0)
            {
                backColor = Color.Red;
                tabName = tabName.TrimEnd('*');
            }

            DrawTabText(tabControl1, e, backColor, tabName);
        }
        /***********************************************************************************************/
        public static void DrawTabText(TabControl tabControl, DrawItemEventArgs e, string caption)
        {
            Color backColor = (Color)System.Drawing.SystemColors.Control;
            Color foreColor = (Color)System.Drawing.SystemColors.ControlText;
            DrawTabText(tabControl, e, backColor, foreColor, caption);
        }
        /***********************************************************************************************/
        public static void DrawTabText(TabControl tabControl, DrawItemEventArgs e, System.Drawing.Color backColor, string caption)
        {
            Color foreColor = (Color)System.Drawing.SystemColors.Control;
            DrawTabText(tabControl, e, backColor, foreColor, caption);
        }
        /***********************************************************************************************/
        public static void DrawTabText(TabControl tabControl, DrawItemEventArgs e, System.Drawing.Color foreColor, System.Drawing.Color backColor, string caption)
        {
            Font tabFont;
            Brush foreBrush = new SolidBrush(foreColor);
            Rectangle r = e.Bounds;
            Brush backBrush = new SolidBrush(backColor);
            string tabName = tabControl.TabPages[e.Index].Text;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;

            e.Graphics.FillRectangle(backBrush, r);

            r = new Rectangle(r.X, r.Y + 3, r.Width, r.Height - 3);
            if (e.Index == tabControl.SelectedIndex)
            {
                tabFont = new Font(e.Font, FontStyle.Italic);
                tabFont = new Font(tabFont, FontStyle.Regular | FontStyle.Italic);
            }
            else
            {
                tabFont = e.Font;
            }

            e.Graphics.DrawString(caption, tabFont, foreBrush, r, sf);

            sf.Dispose();
            if (e.Index == tabControl.SelectedIndex)
            {
                tabFont.Dispose();
                backBrush.Dispose();
            }
            else
            {
                backBrush.Dispose();
                foreBrush.Dispose();
            }
        }
        /***********************************************************************************************/
        private void txtCycleNote_Leave(object sender, EventArgs e)
        {
            //if (!workGroup)
            //    UpdateCycleNote();
        }
        /***********************************************************************************************/
        private void btnClock_Click(object sender, EventArgs e)
        {
            //btnClock.Hide();
            btnDecimal.Show();
            btnDecimal.BringToFront();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void btnDecimal_Click(object sender, EventArgs e)
        {
            //btnDecimal.Hide();
            btnClock.Show();
            btnClock.BringToFront();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void menuAddNewEmployee_ItemClick(object sender, ItemClickEventArgs e)
        {
            //PTOAddNewEmployee ptoForm = new PTOAddNewEmployee();
            //ptoForm.AddNewPTODone += PtoForm_AddNewPTODone;
            //ptoForm.Show();
        }
        /***********************************************************************************************/
        private void PtoForm_AddNewPTODone(string newempno, string name, string hiredate, string birthdate, string salarystatus, string tax_ms, string ps_ms, string supervisor, string department, string safe, string profit)
        {
            if (!String.IsNullOrWhiteSpace(newempno) && !String.IsNullOrWhiteSpace(name))
            {
                if (G1.validate_date(hiredate) && G1.validate_date(birthdate))
                {
                    string empno = "";
                    bool found = false;
                    DataTable dt = (DataTable)dgv.DataSource;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        empno = dt.Rows[i]["empno"].ObjToString();
                        if (empno == newempno)
                        {
                            found = true;
                            MessageBox.Show("***ERROR*** Employee Number (" + empno + ") already exists");
                            break;
                        }
                    }
                    if (!found)
                    {
                        long ldate = G1.date_to_days(birthdate);
                        birthdate = G1.days_to_date(ldate);

                        ldate = G1.date_to_days(hiredate);
                        hiredate = G1.days_to_date(ldate);
                        AddNewPtoEmployee(newempno, name, hiredate, birthdate, salarystatus, supervisor, department, safe, profit);
                        DataTable dx = (DataTable)dgv.DataSource;
                        dx.Rows.Clear();
                        dgv.DataSource = dx;
                        GetAllPunches();
                        MessageBox.Show("Okay! New Employee Added to PTO.");
                        dx = (DataTable)dgv.DataSource;
                        int last_row = dx.Rows.Count;
                        gridMain.SelectRow(last_row - 1);
                        gridMain.FocusedRowHandle = last_row - 1;
                        dgv.Refresh();
                        gridMain.RefreshData();
                        this.Refresh();
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void AddNewPtoEmployee(string empno, string name, string hiredate, string birthdate, string salaryStatus, string supervisor, string department, string safe, string profit)
        {
            string record = G1.create_record("er", "record", "-1");
            if (!String.IsNullOrWhiteSpace(record) && record != "-1")
            {
                hiredate = G1.date_to_sql(hiredate);
                birthdate = G1.date_to_sql(birthdate);
                G1.update_record("`er`", record, "`empno`", empno);
                G1.update_record("`er`", record, "`name`", name.ToUpper());
                G1.update_record("`er`", record, "`status`", "");
                G1.update_record("`er`", record, "`birthdate`", birthdate);
                G1.update_record("`er`", record, "`hiredate`", hiredate);
                G1.update_record("`er`", record, "`salaried`", salaryStatus);
                G1.update_record("`er`", record, "`preferred_supervisor`", supervisor);

                string emprecord = G1.create_record("employees", "record", "-1");
                if (!String.IsNullOrWhiteSpace(emprecord) && emprecord != "-1")
                {
                    DateTime now = DateTime.Now;
                    string year = now.Year.ToString("D4");
                    G1.update_record("`employees`", record, "`empno`", empno);
                    G1.update_record("`employees`", record, "`name`", name.ToUpper());
                    G1.update_record("`employees`", record, "`hiredate`", hiredate);
                    G1.update_record("`employees`", record, "`year`", year);
                    G1.update_record("`employees`", record, "`department`", department);
                    G1.update_record("`employees`", record, "`merc`", "100");
                    G1.update_record("`employees`", record, "`safe`", safe);
                    G1.update_record("`employees`", record, "`share`", profit);
                }
            }
        }
        /***********************************************************************************************/
        private void menuExportPTO_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("Are you sure you want to EXPORT PTO for this time period now?", "Export PTO Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;
            this.Cursor = Cursors.WaitCursor;
            DateTime PostDate = dateTimePicker2.Value;
            PostDate = PostDate.AddDays(1);
            string date = PostDate.Month.ToString("D2") + "/" + PostDate.Day.ToString("D2") + "/" + PostDate.Year.ToString("D4");
            date = G1.date_to_sql(date);
            string cmd = "DELETE from `ptotimeclock` where `timeclockdate` = '" + date + "';";
            G1.get_db_data(cmd);

            string str = "";
            string record = "";
            string empno = "";
            double qpto = 0D;
            double pto = 0D;
            double docked = 0D;
            string notes = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                empno = dt.Rows[i]["empno"].ObjToString();
                if (String.IsNullOrWhiteSpace(empno))
                    continue;
                pto = dt.Rows[i]["pto"].ObjToDouble();
                qpto = dt.Rows[i]["qpto"].ObjToDouble();
                docked = dt.Rows[i]["docked"].ObjToDouble();
                if (pto == 0D && qpto == 0D && docked == 0D)
                    continue;
                notes = dt.Rows[i]["notes"].ObjToString();
                record = G1.create_record("ptotimeclock", "record", "-1");
                if (!String.IsNullOrWhiteSpace(record) && record != "-1")
                {
                    str = "0.0";
                    if (pto != 0D)
                        str = RoundValue(pto).ToString("###.00");
                    else if (qpto != 0D)
                        str = RoundValue(qpto).ToString("###.00");
                    G1.update_record("`ptotimeclock`", record, "`empno`", empno);
                    G1.update_record("`ptotimeclock`", record, "`pto`", str);
                    str = "0.0";
                    if (docked != 0D)
                        str = RoundValue(docked).ToString("###.00");
                    G1.update_record("`ptotimeclock`", record, "`docked`", str);
                    G1.update_record("`ptotimeclock`", record, "`timeclockdate`", date);
                }
            }
            this.Cursor = Cursors.Default;
            MessageBox.Show("Okay! All PTO data has been Exported.");
        }
        /***********************************************************************************************/
        private void editEmployeeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string empno = dr["empno"].ObjToString();
            string name = dr["name"].ObjToString();

            string cmd = "Select * from `er` where `empno` = '" + empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            string record = "";
            if (dt.Rows.Count > 0)
                record = dt.Rows[0]["record"].ObjToString();


            //PTOAddNewEmployee ptoForm = new PTOAddNewEmployee(empno, name, record);
            //ptoForm.AddNewPTODone += PtoForm_AddNewPTODone;
            //ptoForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            DataTable dt = (DataTable)dgv.DataSource;
            double hours = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double dValue = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                hours += dt.Rows[i]["hours"].ObjToDouble();
                dValue = dt.Rows[i]["week1"].ObjToDouble();
                if (dValue != 0D)
                    week1 = dValue;
                dValue = dt.Rows[i]["week2"].ObjToDouble();
                if (dValue != 0D)
                    week2 = dValue;
            }
            double value = e.TotalValue.ObjToDouble();
            if (field.ToUpper() == "TOTAL")
                e.TotalValue = hours;
            if (field.ToUpper() == "HOURS")
                e.TotalValue = hours;
            if (field.ToUpper() == "WEEK1")
                e.TotalValue = week1;
            if (field.ToUpper() == "WEEK2")
                e.TotalValue = week2;
            if (field.ToUpper() == "OTHER")
            {
                if (!G1.isHR())
                    e.TotalValue = 0D;
            }
        }
        /***********************************************************************************************/
        private string oldWhat = "";
        private void gridView5_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper().IndexOf("APPROVED") >= 0)
            {
                string column = view.FocusedColumn.FieldName;
                DataTable dt = (DataTable)dgv5.DataSource;
                DataRow dr = gridView5.GetFocusedDataRow();
                int rowhandle = gridView5.FocusedRowHandle;
                int row = gridView5.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                if (!is_supervisor)
                {
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void btnCalendar_Click(object sender, EventArgs e)
        {
            Calendar calendarForm = new Calendar(workMyName, workUserName, this.dateTimePicker1.Value);
            calendarForm.Show();
        }
        /***********************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string empno = LoginForm.workUserRecord;
            string service = "";
            decimal baseRate = 0;
            decimal rate = 0;
            DataRow dRow = null;
            string record = "";
            string cmd = "";
            DateTime date = this.dateTimePicker1.Value;

            using (EditContractServices contractForm = new EditContractServices("PartTime", true, workUserName))
            {
                DialogResult result = contractForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    cmd = "Delete from `tc_punches_pchs` WHERE `calledBy` = '-1';";
                    G1.get_db_data(cmd);

                    DataTable dx = (DataTable)contractForm.ServiceAnswer;
                    DataRow[] dRows = dx.Select("select='Y'");
                    if (dRows.Length > 0)
                    {
                        DataTable selectDt = dRows.CopyToDataTable();
                        DataTable dt = (DataTable)dgv7.DataSource;
                        for (int i = 0; i < selectDt.Rows.Count; i++)
                        {
                            service = selectDt.Rows[i]["laborService"].ObjToString();
                            baseRate = selectDt.Rows[i]["baserate"].ObjToDecimal();
                            rate = selectDt.Rows[i]["rate"].ObjToDecimal();
                            if (rate <= 0)
                                rate = baseRate;
                            if (rate <= 0)
                                rate = workRate;

                            //cmd = "Delete from `tc_punches_pchs` WHERE `funeralNo` = '-1';";
                            //G1.get_db_data(cmd);
                            //record = G1.create_record("tc_punches_pchs", "funeralNo", "-1");
                            //if (G1.BadRecord("tc_punches_pchs", record))
                            //    return;

                            dRow = dt.NewRow();
                            //dRow["record"] = record;
                            dRow["date"] = G1.DTtoMySQLDT(date);
                            dRow["service"] = service;
                            dRow["funeralNo"] = "";
                            dRow["calledBy"] = "";
                            dRow["deceasedName"] = "";
                            dRow["rate"] = rate;
                            dRow["week"] = "1";
                            dt.Rows.Add(dRow);

                            //G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "punchType", "CONTRACT", "empy!AccountingID", workEmpNo, "service", service, "rate", baseRate.ToString(), "date", date.ToString("MM/dd/yyyy"), "funeralNo", "" });
                        }
                        G1.NumberDataTable(dt);
                        gridMain7.RefreshData();
                        gridMain7.RefreshEditor(true);
                        dgv7.Refresh();
                        gridMain7.ExpandAllGroups();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain7_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain7.FocusedColumn;
            DataRow dr = gridMain7.GetFocusedDataRow();
            string type = currCol.FieldName;
            string record = "";

            if (type.ToUpper() == "DATE")
            {
                DataTable dt = (DataTable)dgv7.DataSource;

                string str = dr["date"].ObjToString();
                DateTime myDate = DateTime.Now;
                if (!String.IsNullOrWhiteSpace(str))
                    myDate = str.ObjToDateTime();
                string title = "Enter Date Service was performed:";
                using (GetDate dateForm = new GetDate(myDate, title))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        myDate = dateForm.myDateAnswer;
                        DateTime newDate = new DateTime(myDate.Year, myDate.Month, myDate.Day);
                        dr["date"] = G1.DTtoMySQLDT(newDate);
                        dr["mod"] = "Y";
                        TimeSpan ts = newDate - this.dateTimePicker1.Value;
                        if (ts.TotalDays <= 7)
                            dr["week"] = "1";
                        else
                            dr["week"] = "2";
                        record = dr["record"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "punchType", "CONTRACT", "empy!AccountingID", workEmpNo, "date", newDate.ToString("MM/dd/yyyy") });

                        DataTable dx = (DataTable)dgv7.DataSource;
                        DataView tempview = dt.DefaultView;
                        tempview.Sort = "week asc, date asc";
                        dx = tempview.ToTable();
                        dgv7.DataSource = dx;

                        gridMain7.RefreshData();
                        gridMain7.RefreshEditor(true);
                        gridMain7.ExpandAllGroups();
                        timeSheetContracModified = true;
                    }
                }
            }
            gridMain7.RefreshData();
            gridMain7.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void gridMain7_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain7.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            G1.delete_db_table("tc_punches_pchs", "record", record);

            DataTable dx = (DataTable)dgv7.DataSource;
            dx.Rows.Remove(dr);
            gridMain7.RefreshData();
            gridMain7.RefreshEditor(true);

            timeSheetContracModified = true;
        }
        /***********************************************************************************************/
        private void gridMain7_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            BandedGridView view = sender as BandedGridView;
            if (view == null)
                return;

            string column = e.Column.FieldName.Trim().ToUpper();
            string data = e.Value.ObjToString();
            if (String.IsNullOrWhiteSpace(data))
                return;
            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;

            DataRow dr = gridMain7.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();

            if (column.ToUpper() == "TIMEIN1")
            {
                if (data.ToUpper().IndexOf("PM") > 0)
                    data = reformatToProperTime(date, data);
                string[] Lines = data.Split(':');
                string sHour = "00";
                string sMinute = "00";
                string sSeconds = "00";
                if (Lines.Length >= 1)
                    sHour = Lines[0].Trim();
                if (Lines.Length >= 2)
                    sMinute = Lines[1].Trim();
                if (!G1.validate_numeric(sHour) || !G1.validate_numeric(sMinute))
                    return;
                int iMinute = sMinute.ObjToInt32();
                int iHour = sHour.ObjToInt32();
                if (iHour <= 0 || iHour >= 24)
                    iHour = 0;
                iMinute = sMinute.ObjToInt32();
                if (iMinute <= 0 || iMinute >= 60)
                    iMinute = 0;
                if (iHour == 17 && iMinute == 0)
                    sSeconds = "01";
                DateTime ClockTime = new DateTime(date.Year, date.Month, date.Day, iHour, iMinute, sSeconds.ObjToInt32());
                date1 = ClockTime.ObjToDateTime();
                dr["timeIn1"] = date1.ToString("HH:mm");
            }
            if (column.ToUpper() == "TIMEOUT1")
            {
                string timeIn = dr["TIMEIN1"].ObjToString();
                string[] Lines = timeIn.Split(':');
                string sHour = "00";
                string sMinute = "00";
                string sSeconds = "00";
                if (Lines.Length >= 1)
                    sHour = Lines[0].Trim();
                if (Lines.Length >= 2)
                    sMinute = Lines[1].Trim();
                int iMinute = sMinute.ObjToInt32();
                int iHour = sHour.ObjToInt32();
                int timeInMin = iMinute;
                int timeInHour = iHour;
                DateTime checkTimeIn = new DateTime(date.Year, date.Month, date.Day, iHour, iMinute, 0);

                if (data.ToUpper().IndexOf("PM") > 0)
                    data = reformatToProperTime(date, data);
                Lines = data.Split(':');
                sHour = "00";
                sMinute = "00";
                sSeconds = "00";
                if (Lines.Length >= 1)
                    sHour = Lines[0].Trim();
                if (Lines.Length >= 2)
                    sMinute = Lines[1].Trim();
                if (!G1.validate_numeric(sHour) || !G1.validate_numeric(sMinute))
                    return;
                iMinute = sMinute.ObjToInt32();
                iHour = sHour.ObjToInt32();
                if (iHour <= 0 || iHour >= 24)
                    iHour = 0;
                iMinute = sMinute.ObjToInt32();
                if (iMinute <= 0 || iMinute >= 60)
                    iMinute = 0;

                DateTime checkTimeOut = new DateTime(date.Year, date.Month, date.Day, iHour, iMinute, 0);
                if (checkTimeOut < checkTimeIn)
                {
                    checkTimeOut = checkTimeOut.AddDays(1);
                    date = checkTimeOut;
                    date2 = date;
                }

                DateTime ClockTime = new DateTime(date.Year, date.Month, date.Day, iHour, iMinute, sSeconds.ObjToInt32());
                date2 = ClockTime.ObjToDateTime();
                dr["timeOut1"] = date2.ToString("HH:mm");
            }

            timeSheetContracModified = true;

            date1 = dr["timeIn1"].ObjToDateTime();
            date2 = dr["timeOut1"].ObjToDateTime();
            double rate = dr["rate"].ObjToDouble();

            double hours = CalculateHours(date, dr["timeIn1"].ObjToString(), dr["timeOut1"].ObjToString() );

            TimeSpan ts = date2 - date1;
            //hours = ts.TotalHours;
            double totalPay = hours * rate;
            if (hours <= 0D)
            {
                dr["paymentAmount"] = 0D;
                dr["hours"] = 0D;
            }
            else
            {
                dr["paymentAmount"] = totalPay;
                dr["hours"] = hours;
            }
        }
        /***********************************************************************************************/
        private double CalculateHours ( DateTime date, string timeIn, string timeOut )
        {
            double hours = 0D;
            if (timeIn.ToUpper().IndexOf("PM") > 0)
                timeIn = reformatToProperTime(date, timeIn);
            string[] Lines = timeIn.Split(':');
            string sHour = "00";
            string sMinute = "00";
            string sSeconds = "00";
            if (Lines.Length >= 1)
                sHour = Lines[0].Trim();
            if (Lines.Length >= 2)
                sMinute = Lines[1].Trim();
            int iMinute = sMinute.ObjToInt32();
            int iHour = sHour.ObjToInt32();
            int timeInMin = iMinute;
            int timeInHour = iHour;
            DateTime checkTimeIn = new DateTime(date.Year, date.Month, date.Day, iHour, iMinute, 0);

            if (timeOut.ToUpper().IndexOf("PM") > 0)
                timeOut = reformatToProperTime(date, timeOut);
            Lines = timeOut.Split(':');
            sHour = "00";
            sMinute = "00";
            sSeconds = "00";
            if (Lines.Length >= 1)
                sHour = Lines[0].Trim();
            if (Lines.Length >= 2)
                sMinute = Lines[1].Trim();
            if (!G1.validate_numeric(sHour) || !G1.validate_numeric(sMinute))
                return 0D;
            iMinute = sMinute.ObjToInt32();
            iHour = sHour.ObjToInt32();
            if (iHour <= 0 || iHour >= 24)
                iHour = 0;
            iMinute = sMinute.ObjToInt32();
            if (iMinute <= 0 || iMinute >= 60)
                iMinute = 0;

            DateTime checkTimeOut = new DateTime(date.Year, date.Month, date.Day, iHour, iMinute, 0);
            if (checkTimeOut < checkTimeIn)
            {
                checkTimeOut = checkTimeOut.AddDays(1);
                date = checkTimeOut;
            }

            DateTime ClockTime = new DateTime(date.Year, date.Month, date.Day, iHour, iMinute, sSeconds.ObjToInt32());

            TimeSpan ts = ClockTime - checkTimeIn;
            hours = ts.TotalHours;
            return hours;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            double dValue = 0D;
            if (e.Column.FieldName.ToUpper() == "VACATION")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue == 0D)
                    e.DisplayText = "";
            }
            else if (e.Column.FieldName.ToUpper() == "HOLIDAY")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue == 0D)
                    e.DisplayText = "";
            }
            else if (e.Column.FieldName.ToUpper() == "SICK")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue == 0D)
                    e.DisplayText = "";
            }
            else if (e.Column.FieldName.ToUpper() == "OTHER")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue == 0D)
                    e.DisplayText = "";
                else
                {
                    if (!G1.isHR())
                        e.DisplayText = "*.**";
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DAY")
            {
                string dow = e.DisplayText.ToUpper();
                if (dow == "FRIDAY")
                    dow = "FRI/SAT";
                else if (dow == "SATURDAY")
                    dow = "SAT/SUN";
                else if (dow.ToUpper() == "SUNDAY")
                    dow = "SUN/MON";
                else if (dow.ToUpper() == "MONDAY")
                    dow = "MON/TUE";
                else if (dow.ToUpper() == "TUESDAY")
                    dow = "TUE/WED";
                else if (dow.ToUpper() == "WEDNESDAY")
                    dow = "WED/THU";
                else if (dow.ToUpper() == "THURSDAY")
                    dow = "THU/FRI";
                e.DisplayText = dow;
            }
            else if (e.Column.FieldName.ToUpper() == "NEWDATE")
            {
                string str = e.DisplayText.ObjToString();
                if (G1.validate_date(str))
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf ("IN") == 0 || e.Column.FieldName.ToUpper().IndexOf("OUT") == 0 )
            {
                string str = e.DisplayText;
                if ( str.ToUpper() == "MIDNGHT")
                {
                }
                if (str.ToUpper() != "MIDNIGHT")
                {
                    if (str.IndexOf(":") > 0)
                    {
                        string[] Lines = str.Split(':');
                        if (Lines.Length >= 2)
                            e.DisplayText = Lines[0] + ":" + Lines[1];
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void chkEmployeeApproved_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (LoginForm.username.ToUpper() != workUserName.ToUpper())
            {
                DialogResult result = MessageBox.Show("***Question***\nYou are NOT the employee\nDo you still want to change this flag?", "Employee Approval Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel || result == DialogResult.No)
                {
                    ResetEmployeeApproved();
                    return;
                }
                timeSheetModified = true;
                employeeApproved = true;
                return;
            }
            if (managerApprovedIn)
            {
                MessageBox.Show("***ERROR***\nTimesheet has already been approved by the manager!\nYou cannot change it without the Managers approval!", "Employee Approval Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetEmployeeApproved();
                return;
            }
            employeeApproved = true;
            timeSheetModified = true;
        }
        /***********************************************************************************************/
        private void chkManagerApproved_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            bool isManager = Employees.isManager();
            if (!isManager)
            {
                DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");
                DataTable userDt = G1.get_db_data("Select * from `users`;");
                string location = "";
                DataRow[] dRows = null;
                string manager = "";
                string[] Lines = null;
                string fName = "";
                string lName = "";
                string[] Lines2 = workLocation.Split('~');
                for (int j = 0; j < Lines2.Length; j++)
                {
                    location = Lines2[j].Trim();
                    if (String.IsNullOrWhiteSpace(location))
                        continue;
                    dRows = funDt.Select("LocationCode='" + location + "'");
                    if (dRows.Length > 0)
                    {
                        manager = dRows[0]["manager"].ObjToString();
                        Lines = manager.Split(' ');
                        if (Lines.Length > 1)
                        {
                            fName = Lines[0].Trim();
                            lName = Lines[1].Trim();
                            dRows = userDt.Select("firstName='" + fName + "' AND lastName = '" + lName + "'");
                            if (dRows.Length > 0)
                            {
                                isManager = true;
                                //string IamManager = dRows[0]["isManager"].ObjToString();
                                //if (IamManager == "Y")
                                //    isManager = true;
                            }
                        }
                    }
                }

            }
            if (!isManager)
            {
                if (G1.isAdmin() || G1.isHR())
                {
                    DialogResult result = MessageBox.Show("***Question***\nYou are NOT the manager\nDo you still want to change this flag?", "Manager Approval Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Cancel || result == DialogResult.No)
                    {
                        ResetManagerApproved();
                        return;
                    }
                    timeSheetModified = true;
                    if (chkManagerApproved.Checked)
                        chkEmployeeApproved.Checked = true;
                    managerApproved = true;
                    managerApprovedIn = false;
                    return;
                }
                else
                {
                    ResetManagerApproved();
                    return;
                }
                loading = true;
                chkManagerApproved.Checked = false;
                loading = false;
                timeSheetModified = true;
                return;
            }
            managerApproved = true;
            timeSheetModified = true;
            managerApprovedIn = false;
        }
        /***********************************************************************************************/
        private void ResetManagerApproved()
        {
            loading = true;
            chkManagerApproved.Checked = managerApprovedIn;
            loading = false;
        }
        /***********************************************************************************************/
        private void ResetEmployeeApproved()
        {
            loading = true;
            chkEmployeeApproved.Checked = employeeApprovedIn;
            loading = false;
        }
        /***********************************************************************************************/
        private void LookupActualUsername()
        {
            actualUsername = "";
            if (String.IsNullOrWhiteSpace(empno))
                return;
            string cmd = "Select * from `users` WHERE `record` = '" + empno + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            actualUsername = dx.Rows[0]["userName"].ObjToString();
        }
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (managerApprovedIn)
            {
                e.Valid = false;
                MessageBox.Show("***ERROR***\nTimesheet has already been approved by the manager!\nYou cannot change it without the Managers approval!", "Employee Approval Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SendKeys.Send("{ESC}");
                return;
            }
        }
        /***********************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain8);
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain7);
        }
        /***********************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        { // Select Other Services
            this.Cursor = Cursors.WaitCursor;
            string empno = LoginForm.workUserRecord;
            string service = "";
            decimal baseRate = 0;
            decimal rate = 0;
            DataRow dRow = null;
            string record = "";
            string cmd = "";
            DateTime date = this.dateTimePicker1.Value;

            using (EditContractServices contractForm = new EditContractServices("Other", true, workUserName))
            {
                DialogResult result = contractForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    cmd = "Delete from `tc_punches_pchs` WHERE `calledBy` = '-1';";
                    G1.get_db_data(cmd);

                    DataTable dx = (DataTable)contractForm.ServiceAnswer;
                    DataRow[] dRows = dx.Select("select='Y'");
                    if (dRows.Length > 0)
                    {
                        DataTable selectDt = dRows.CopyToDataTable();
                        DataTable dt = (DataTable)dgv8.DataSource;
                        for (int i = 0; i < selectDt.Rows.Count; i++)
                        {
                            service = selectDt.Rows[i]["laborService"].ObjToString();
                            baseRate = selectDt.Rows[i]["baserate"].ObjToDecimal();
                            rate = selectDt.Rows[i]["rate"].ObjToDecimal();
                            if (rate <= 0)
                                rate = baseRate;
                            if (rate <= 0)
                                rate = workRate;

                            //cmd = "Delete from `tc_punches_pchs` WHERE `funeralNo` = '-1';";
                            //G1.get_db_data(cmd);
                            //record = G1.create_record("tc_punches_pchs", "funeralNo", "-1");
                            //if (G1.BadRecord("tc_punches_pchs", record))
                            //    return;

                            dRow = dt.NewRow();
                            //dRow["record"] = record;
                            dRow["date"] = G1.DTtoMySQLDT(date);
                            dRow["service"] = service;
                            dRow["funeralNo"] = "";
                            dRow["calledBy"] = "";
                            dRow["deceasedName"] = "";
                            dRow["rate"] = rate;
                            dRow["week"] = "1";
                            dRow["paymentAmount"] = rate;
                            dRow["timeIn"] = "13:00:00";
                            dRow["timeOut"] = "13:00:00";
                            dt.Rows.Add(dRow);

                            //G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "punchType", "CONTRACT", "empy!AccountingID", workEmpNo, "service", service, "rate", baseRate.ToString(), "date", date.ToString("MM/dd/yyyy"), "funeralNo", "" });
                        }
                        G1.NumberDataTable(dt);
                        gridMain8.RefreshData();
                        gridMain8.RefreshEditor(true);
                        dgv8.Refresh();
                        gridMain8.ExpandAllGroups();

                        timeSheetOtherModified = true;
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        { // Delete Other Services
            DataRow dr = gridMain8.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            G1.delete_db_table("tc_punches_pchs", "record", record);

            DataTable dx = (DataTable)dgv8.DataSource;
            dx.Rows.Remove(dr);
            gridMain8.RefreshData();
            gridMain8.RefreshEditor(true);

            timeSheetOtherModified = true;
        }
        /***********************************************************************************************/
        private void gridMain8_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain8_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper().IndexOf("CALLEDBY") >= 0)
            {
                string column = view.FocusedColumn.FieldName;
                DataTable dt = (DataTable)dgv8.DataSource;
                DataRow dr = gridMain8.GetFocusedDataRow();
                int rowhandle = gridMain8.FocusedRowHandle;
                int row = gridMain8.GetDataSourceRowIndex(rowhandle);
                string str = e.Value.ObjToString();
                str = str.Replace(",", "");
                if ( G1.validate_numeric ( str ))
                {
                    e.Valid = false;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain8_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain8.FocusedColumn;
            DataRow dr = gridMain8.GetFocusedDataRow();
            string type = currCol.FieldName;
            string record = "";

            if (type.ToUpper() == "DATE")
            {
                DataTable dt = (DataTable)dgv8.DataSource;

                string str = dr["date"].ObjToString();
                DateTime myDate = DateTime.Now;
                if (!String.IsNullOrWhiteSpace(str))
                    myDate = str.ObjToDateTime();
                string title = "Enter Date Service was performed:";
                using (GetDate dateForm = new GetDate(myDate, title))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        myDate = dateForm.myDateAnswer;
                        DateTime newDate = new DateTime(myDate.Year, myDate.Month, myDate.Day);
                        dr["date"] = G1.DTtoMySQLDT(newDate);
                        dr["mod"] = "Y";
                        TimeSpan ts = newDate - this.dateTimePicker1.Value;
                        if (ts.TotalDays < 7)
                            dr["week"] = "1";
                        else
                            dr["week"] = "2";
                        record = dr["record"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.update_db_table("tc_punches_pchs", "record", record, new string[] { "punchType", "OTHER", "empy!AccountingID", workEmpNo, "date", newDate.ToString("MM/dd/yyyy") });

                        DataTable dx = (DataTable)dgv8.DataSource;
                        DataView tempview = dt.DefaultView;
                        tempview.Sort = "week asc, date asc";
                        dx = tempview.ToTable();
                        dgv8.DataSource = dx;

                        gridMain8.RefreshData();
                        gridMain8.RefreshEditor(true);
                        gridMain8.ExpandAllGroups();
                        timeSheetOtherModified = true;
                    }
                }
            }
            else if ( type.ToUpper() == "CALLEDBY")
            {
                string str = dr["calledBy"].ObjToString();
                str = str.Replace(",", "");
                if ( G1.validate_numeric ( str ))
                {
                }
            }
            gridMain8.RefreshData();
            gridMain8.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void gridMain8_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            BandedGridView view = sender as BandedGridView;
            if (view == null)
                return;

            string column = e.Column.FieldName.Trim().ToUpper();
            string data = e.Value.ObjToString();
            if (String.IsNullOrWhiteSpace(data))
                return;

            DataRow dr = gridMain8.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();

            double rate = dr["rate"].ObjToDouble();
            dr["paymentAmount"] = rate;

            timeSheetOtherModified = true;
        }
        /***********************************************************************************************/
        //private void menuClearRow_Click(object sender, EventArgs e)
        //{
        //    DataRow dr = gridMain.GetFocusedDataRow();
        //    string empno = dr["empno"].ObjToString();
        //    DateTime date = dr["date"].ObjToDateTime();
        //    //date = date.AddDays(-1);
        //    string cmd = "Select * from `tc_punches_pchs` WHERE `date` = '" + date.ToString("yyyy-MM-dd") + "' AND `empy!accountingID` = '" + workEmpNo + "';";
        //    DataTable dx = G1.get_db_data(cmd);
        //    string record = "";
        //    for (int i = 0; i < dx.Rows.Count; i++)
        //    {
        //        record = dx.Rows[i]["record"].ObjToString();
        //        G1.delete_db_table("tc_punches_pchs", "record", record);
        //    }
        //    try
        //    {
        //        LoadTimePeriod();
        //        GetEmployeePunches(empno);
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    if (!btnPunchIn2.Visible)
        //        btnAddPunch_Click(null, null);
        //    if (!btnPunchIn3.Visible)
        //        btnAddPunch_Click(null, null);
        //    if (!btnPunchIn4.Visible)
        //        btnAddPunch_Click(null, null);

        //    PerformGrouping();
        //}
        /***********************************************************************************************/
        private void barButtonPrintAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            iTextSharp.text.Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;
            string outputPdfPath = @"C:/rag/pdfAllTime.pdf";
            string timeFile = @"C:/rag/pdfTime.pdf";
            string contractFile = @"C:/rag/pdfContract.pdf";
            string otherFile = @"C:/rag/pdfOther.pdf";

            try
            {
                if (File.Exists(outputPdfPath))
                {
                    File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                    File.Delete(outputPdfPath);
                }
                if (File.Exists(timeFile))
                {
                    File.SetAttributes(timeFile, FileAttributes.Normal);
                    File.Delete(timeFile);
                }
                if (File.Exists(contractFile))
                {
                    File.SetAttributes(contractFile, FileAttributes.Normal);
                    File.Delete(contractFile);
                }
                if (File.Exists(otherFile))
                {
                    File.SetAttributes(otherFile, FileAttributes.Normal);
                    File.Delete(otherFile);
                }
            }
            catch (Exception ex)
            {
            }

            sourceDocument = new iTextSharp.text.Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();

            DateTime startTime = this.dateTimePicker1.Value;
            DateTime stopTime = this.dateTimePicker2.Value;


            using (TimeClock timeForm = new TimeClock(startTime, stopTime, empno, workUserName, empName, true))
            {
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    timeForm.ShowDialog();
                }
                catch (Exception ex)
                {
                }

                try
                {
                    MergeAllPDF(pdfCopyProvider, timeFile, contractFile, otherFile);
                    File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                }
                catch (Exception ex)
                {
                }

                if (File.Exists(timeFile))
                {
                    File.SetAttributes(timeFile, FileAttributes.Normal);
                    File.Delete(timeFile);
                }

                if (File.Exists(contractFile))
                {
                    File.SetAttributes(contractFile, FileAttributes.Normal);
                    File.Delete(contractFile);
                }
                if (File.Exists(otherFile))
                {
                    File.SetAttributes(otherFile, FileAttributes.Normal);
                    File.Delete(otherFile);
                }
                this.Cursor = Cursors.Default;
            }
            sourceDocument.Close();

            ViewPDF myView = new ViewPDF("SMFS Employee Timesheets", outputPdfPath);
            myView.ShowDialog();
        }
        /***********************************************************************************************/
        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }
        /***********************************************************************************************/
        private static void MergeAllPDF(PdfCopy pdfCopyProvider, string File1, string File2, string File3)
        {
            string[] fileArray = new string[4];
            fileArray[0] = File1;
            fileArray[1] = File2;
            fileArray[2] = File3;

            PdfReader reader = null;
            PdfImportedPage importedPage;


            //files list wise Loop  
            try
            {
                for (int f = 0; f < fileArray.Length - 1; f++)
                {
                    try
                    {
                        if (!File.Exists(fileArray[f]))
                            continue;
                        int pages = TotalPageCount(fileArray[f]);

                        reader = new PdfReader(fileArray[f]);
                        //Add pages in new file  
                        for (int i = 1; i <= pages; i++)
                        {
                            try
                            {
                                importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                                pdfCopyProvider.AddPage(importedPage);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int row = gridMain.FocusedRowHandle;
            if (row < 0)
                return;
            row = gridMain.GetDataSourceRowIndex(row);
            string field = view.FocusedColumn.FieldName.ToUpper();
            if (field.ToUpper() == "OTHER")
            {
                if (!G1.isHR())
                {
                    string data = dr["OTHER"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        //double dValue = data.ObjToDouble();
                        //if (dValue != 0D)
                        //    dr["Other"] = 0D;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab )
            {
                int row = gridMain.FocusedRowHandle;
                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName;
                //gridMain.OptionsNavigation.EnterMoveNextColumn;
            }
        }
        /***********************************************************************************************/
        private void pictureBox5_Click(object sender, EventArgs e)
        { // Remove Vacation Request
            DataRow dr = gridView5.GetFocusedDataRow();
            string data = dr["approved"].ObjToString();
            if ( data.ToUpper() == "Y" )
            {
                bool canDo = CheckCanDo();
                if (!canDo)
                {
                    MessageBox.Show("*** ERROR *** You may not remove an APPROVED vacation request!!", "Remove Vacation Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }


            DateTime fromDate = dr["fromdate"].ObjToDateTime();
            DateTime toDate = dr["todate"].ObjToDateTime();
            if (fromDate == toDate)
                data = "for date " + fromDate.ToString("MM/dd/yyyy");
            else
                data = "for dates " + fromDate.ToString("MM/dd/yyyy") + " to " + toDate.ToString("MM/dd/yyyy");

            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to REMOVE this Vacation Request " + data + " ?", "Remove Vacation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            string record = dr["record"].ObjToString();

            G1.delete_db_table("tc_timerequest", "record", record);

            LoadMyTimeOffRequests();
        }
        /***********************************************************************************************/
        private bool CheckCanDo ()
        {
            bool canDo = false;
            if (is_timekeeper || is_supervisor || G1.isHR() || G1.isAdmin() || Employees.isManager())
                canDo = true;
            return canDo;
        }
        /***********************************************************************************************/
        private void gridView5_CellValueChanged(object sender, CellValueChangedEventArgs e)
        { // Allow Comment to be changed
            DataRow dr = gridView5.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv5.DataSource;
            int rowHandle = gridView5.FocusedRowHandle;
            int row = gridView5.GetDataSourceRowIndex(rowHandle);
            string field = e.Column.FieldName.Trim();

            bool canDo = CheckCanDo();

            if (field.ToUpper() == "COMMENT")
            {
                string approved = dr["approved"].ObjToString();
                if ( approved.ToUpper() == "Y" )
                {
                    if (canDo)
                    {
                        DialogResult result = MessageBox.Show("*** QUESTION *** Are you sure you want to change this employees comment?", "Change Vacation Comment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == DialogResult.No)
                            return;
                    }
                    else
                    {
                        MessageBox.Show("*** ERROR *** You may not change the comment of an APPROVED vacation request!!", "Change Vacation Comment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
                string record = dr["record"].ObjToString();
                string comment = dr["comment"].ObjToString();
                G1.update_db_table("tc_timerequest", "record", record, new string[] { "comment", comment });
                return;
            }
        }
        /***********************************************************************************************/
        private void menuClearRow_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string empno = dr["empno"].ObjToString();
            DateTime date = dr["date"].ObjToDateTime();
            int row = gridMain.FocusedRowHandle;

            ClearCell(dr, "IN1");
            ClearCell(dr, "OUT1");
            ClearCell(dr, "IN2");
            ClearCell(dr, "OUT2");
            ClearCell(dr, "IN3");
            ClearCell(dr, "OUT3");
            ClearCell(dr, "IN4");
            ClearCell(dr, "OUT4");
            ClearCell(dr, "IN5");
            ClearCell(dr, "OUT5");

            DataTable dt = (DataTable)dgv.DataSource;
            CalcHours(dt, row);
        }
        /***********************************************************************************************/
        private void ClearCell ( DataRow dr, string field )
        {
            dr[field] = "";
            dr["mod"] = "Y";
            timeSheetModified = true;
        }
        /***********************************************************************************************/
        private void clearCellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string empno = dr["empno"].ObjToString();
            DateTime date = dr["date"].ObjToDateTime();
            string field = gridMain.FocusedColumn.FieldName;
            dr[field] = "";
            dr["mod"] = "Y";
            timeSheetModified = true;
            DataTable dt = (DataTable)dgv.DataSource;
            int row = gridMain.FocusedRowHandle;
            CalcHours( dt, row );
        }
        /***********************************************************************************************/
        private string oldColumn = "";
        private void gridView5_ShownEditor(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            int row = gridView5.FocusedRowHandle;

            GridColumn currCol = gridView5.FocusedColumn;
            DataRow dr = gridView5.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime myDate = DateTime.Now;
            oldColumn = name;
            double yearlyVacation = 0D;
            double yearlySick = 0D;
            double pto_now = 0D;

            bool doDate = false;
            if (name == "fromdate")
                doDate = true;
            else if (name == "todate")
                doDate = true;

            if (doDate)
            {
                myDate = dr[name].ObjToDateTime();
                if (myDate.Year < 100)
                {
                    if (name == "todate")
                    {
                        myDate = dr["todate"].ObjToDateTime();
                        if (myDate.Year < 100)
                            myDate = G1.GetDateTimeNow();
                    }
                    else
                        myDate = G1.GetDateTimeNow();
                }
                str = gridView5.Columns[name].Caption;
                using (GetDate dateForm = new GetDate(myDate, str, 5 ))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            myDate = dateForm.myDateAnswer;
                            dr[name] = G1.DTtoMySQLDT(myDate);
                            DateTime fromDate = dr["fromdate"].ObjToDateTime();
                            DateTime toDate = dr["todate"].ObjToDateTime();
                            if (toDate >= fromDate && fromDate.Year > 100 )
                            {
                                bool valid = VerifyVacation(fromDate, toDate);
                                if ( !valid )
                                {
                                    dr["mod"] = "";
                                    dr[name] = "";
                                    return;
                                }
                                TimeSpan ts = toDate - fromDate;
                                double hours = (ts.TotalDays + 1D) * 8D;

                                Employees.SetupBenefits(workHireDate, fromDate, ref yearlyVacation, ref yearlySick);
                                if (workVacationOverride > 0D)
                                    yearlyVacation = workVacationOverride;
                                double pto_taken = CalcPTOupto(workEmpNo, workHireDate, fromDate, toDate, ref pto_now );
                                if ( ( pto_taken + hours) > yearlyVacation )
                                {
                                    double diff = (pto_taken + hours) - yearlyVacation;
                                    MessageBox.Show("*** PROBLEM *** This vacation request will exceed\navailable PTO by " + G1.ReformatMoney (diff) + " Hours!", "PTO Exceeded Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                    dr["mod"] = "";
                                    dr[name] = "";
                                    return;
                                }

                                dr["hours"] = hours;

                                //RecalcPTO(dt);
                            }
                            UpdateMod(dr);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            gridView5.RefreshData();
            gridView5.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            btnSaveVacation.Show();
            btnSaveVacation.Refresh();
        }
        /***********************************************************************************************/
        private bool VerifyVacation(DateTime startDate, DateTime stopDate)
        {
            bool good = false;

            string date1 = startDate.ToString("yyyy-MM-dd");
            string date2 = stopDate.ToString("yyyy-MM-dd");

            string cmd = "Select * from `tc_er` WHERE `username` = '" + workUserName + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            string myLocation = dt.Rows[0]["location"].ObjToString();
            if (String.IsNullOrWhiteSpace(myLocation))
                return false;

            cmd = "Select * from `tc_hr_groups` WHERE `locations` LIKE '%" + myLocation + "%';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            string locations = dt.Rows[0]["locations"].ObjToString();
            string[] Lines = locations.Split('~');
            string query = "";
            string location = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                location = Lines[i].Trim();
                query += "'" + location + "',";
            }
            query = query.TrimEnd(',');

            try
            {
                cmd = "Select * from `tc_timerequest` r JOIN `tc_er` e ON r.`empno` = e.`username` JOIN `users` u ON r.`empno` = u.`username` WHERE '" + date1 + "' >= `fromDate` AND '" + date1 + "' <= `toDate` AND e.`location` IN (" + query + ");";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string otheremp = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                    string xdate1 = dt.Rows[0]["fromDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    string xdate2 = dt.Rows[0]["toDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    MessageBox.Show("*** PROBLEM *** One of your Vacation dates overlaps with\n" + otheremp + "!\nDates are " + xdate1 + " to " + xdate2 + "\nThis data CANNOT be saved!\nYou must try to resolve!", "Conflicting Vacation Dates Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
                cmd = "Select * from `tc_timerequest` r JOIN `tc_er` e ON r.`empno` = e.`username` JOIN `users` u ON r.`empno` = u.`username` WHERE '" + date2 + "' >= `fromDate` AND '" + date2 + "' <= `toDate` AND e.`location` IN (" + query + ");";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string otheremp = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
                    string xdate1 = dt.Rows[0]["fromDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    string xdate2 = dt.Rows[0]["toDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                    MessageBox.Show("*** PROBLEM *** One of your Vacation dates overlaps with\n" + otheremp + "!\nDates are " + xdate1 + " to " + xdate2 + "\nThis data CANNOT be saved!\nYou must try to resolve!", "Conflicting Vacation Dates Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
            }
            catch (Exception ex)
            {
            }

            return true;
        }
        /***********************************************************************************************/
        private void btnSaveVacation_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv5.DataSource;
            string record = "";
            string mod = "";
            string str = "";
            string name = "";
            string comment = "";
            double hours = 0D;

            DateTime stopDate = DateTime.Now;
            DateTime startDate = DateTime.Now;
            string cDate1 = "";
            string cDate2 = "";
            DateTime date = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod != "Y")
                    continue;
                name = dt.Rows[i]["name"].ObjToString();
                comment = dt.Rows[i]["comment"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( record ))
                    record = G1.create_record("tc_timerequest", "user", "-1");
                if (!String.IsNullOrWhiteSpace(record) && record != "-1")
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    startDate = dt.Rows[i]["fromdate"].ObjToDateTime();
                    stopDate = dt.Rows[i]["todate"].ObjToDateTime();
                    hours = dt.Rows[i]["hours"].ObjToDouble();

                    cDate1 = startDate.ToString("MM/dd/yyyy");
                    cDate2 = stopDate.ToString("MM/dd/yyyy");
                    G1.update_db_table("tc_timerequest", "record", record, new string[] { "empno", workUserName, "supervisor", workMyTimeKeeper, "fromdate", cDate1, "todate", cDate2, "requested_hours", hours.ToString(), "OtherInformation", "", "date_requested", date.ToString("MM/dd/yyyy"), "name", name, "comment", comment });
                    dt.Rows[i]["mod"] = "";
                    dt.Rows[i]["record"] = record.ObjToInt32();
                }
            }
            btnSaveVacation.Hide();
            btnSaveVacation.Refresh();
        }
        /***********************************************************************************************/
        private void gridView5_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            bool doDate = false;
            bool doTime = false;
            if (name == "date")
                doDate = true;
            else if (name == "fromdate")
                doDate = true;
            else if (name == "todate")
                doDate = true;

            if (doDate)
            {
                DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                if (date.Year < 30)
                    e.DisplayText = "";
                else
                {
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }

            if (doTime)
            {
                if (!String.IsNullOrWhiteSpace(e.DisplayText.Trim()))
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("HH:mm");
                }
            }
        }
        /***********************************************************************************************/
        private void gridView5_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridView5.GetFocusedDataRow();
            string column = gridView5.FocusedColumn.FieldName.ToUpper();

            int rowHandle = gridView5.FocusedRowHandle;
            int row = gridView5.GetDataSourceRowIndex(rowHandle);
        }
        /***********************************************************************************************/
    }
}
