using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using System.Linq;
using System.Diagnostics;
using System.IO;
using DevExpress.XtraGrid;
using DevExpress.XtraPrinting;
using System.Data.OleDb;
using GeneralLib;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
using DevExpress.XtraGrid.Columns;

using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Collections;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DevExpress.Utils;

using MySql.Data.MySqlClient;
using System.Configuration;
using System.Threading;
using MySql.Data.Types;


using System.Net;
using System.Net.Sockets;
using System.IO.Compression;

using iTextSharp.text.pdf;
using System.IO;
using System.Text.RegularExpressions;

using System.Windows.Forms.VisualStyles;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
using System.Drawing.Drawing2D;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Employees : DevExpress.XtraEditors.XtraForm
    {
        string workReport = "EmployeePay";
        private bool foundLocalPreference = false;
        private string work_empno = "";
        private string work_myName = "";
        private string work_username = "";
        private bool loading = true;
        private bool justTimeKeeper = false;
        private bool justManager = false;
        private DataTable mainDt = null;
        private DataTable funDt = null;
        public static bool showRates = true;
        private string workGroupName = "";
        /***********************************************************************************************/
        public Employees(string empno, string name)
        {
            work_empno = empno;
            work_myName = name;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void Employees_Load(object sender, EventArgs e)
        {
            this.Text = "Employee List (" + work_myName + ")";

            work_username = LoginForm.username;

            btnSave.Hide();
            btnSaveSick.Hide();
            btnImportSick.Show();
            btnImportSick.Refresh();

            string saveName = "TimeSheets " + workReport + " Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                if (skinName != "DevExpress Style")
                    skinForm_SkinSelected("Skin : " + skinName);
            }

            loadGroupCombo(cmbSelectColumns, "TimeSheets " + workReport, "Primary");
            LoadFuneralHomeManagers();

            SetupEmployeeTimes();

            GetAllEmployees();

            string answer = G1.getPreference(LoginForm.username, "TimeClock", "Take Pictures");

            if (answer != "YES")
                dgv.ContextMenuStrip = null;

            if (G1.isHR())
            {
                string cmd = "Select * from `users` WHERE `userName` = '" + LoginForm.username + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    if (dx.Rows[0]["ctrlRstatus"].ObjToString().ToUpper() == "ON")
                        showRates = false;
                }
            }
            else
            {
                if (this.menuReports != null)
                    this.menuReports.Dispose();
                this.menuReports = null;
                contextMenuStrip1.Dispose();
                contextMenuStrip1 = null;
            }

            if (!G1.isHR() && !G1.isAdmin())
            {
                labTestUser.Hide();
                txtUser.Hide();
            }

            LoadPayrollTimeKeepers();
            LoadLocations();
            LoadTimeKeepers();

            SetupTotalsSummary();

            tabControl1.SelectedTab = tabPage2;
            if (justTimeKeeper && !G1.isAdmin() && !G1.isHR())
            {
                tabControl1.TabPages.Remove(tabPage1);
                cmbSuper.Hide();
                label2.Hide();
                cmbLocation.Hide();
                label3.Hide();
                menuStrip1.Items.Remove(editToolStripMenuItem);
            }
            else if (justManager && !G1.isAdmin() && !G1.isHR())
            {
                tabControl1.TabPages.Remove(tabPage1);
                cmbSuper.Hide();
                label2.Hide();
                cmbLocation.Hide();
                label3.Hide();
                menuStrip1.Items.Remove(editToolStripMenuItem);
            }

            if (!justTimeKeeper && !justManager && !G1.isAdmin() && !G1.isHR())
            {
                tabControl1.TabPages.Remove(tabPageTimeOff);
                cmbSuper.Hide();
                label2.Hide();
                cmbLocation.Hide();
                label3.Hide();
                menuStrip1.Items.Remove(editToolStripMenuItem);
            }


            if (justTimeKeeper)
            {
                this.Text = "Employees Available for TimeKeeper " + work_myName;
            }
            else if (justManager)
            {
                this.Text = "Employees Available for Manager " + work_myName;
            }

            btnSaveData.Hide();

            int width = gridMain2.Columns["totalPay"].Width;
            int visiblewidth = gridMain2.Columns["totalPay"].VisibleWidth;

            if (dgv2.Focused)
            {
            }

            dgv2.Select();

            dgv2.Focus();
            if (dgv2.Focused)
            {
            }

            gridMain2.RefreshData();
            dgv2.Refresh();

            gridMain2.FocusedColumn = gridMain2.Columns["lastName"];
            gridMain2.RefreshData();
            dgv2.Refresh();

            ScaleCells();

            Rectangle rect = this.Bounds;

            width = rect.Width - 100;
            int left = rect.Left;

            if (!G1.isAdmin() && !G1.isHR())
            {
                width = width - 400;
                left = left + 100;
            }

            this.SetBounds(left, rect.Top, width, rect.Height);

            HideBenefits();

            CleanupColumns();

            LoadHrGroups();

            if ( !G1.isHR () )
            {
                tabControl1.TabPages["tabSickDays"].Dispose();
            }

            loading = false;
        }
        /***********************************************************************************************/
        private bool CheckCanDo()
        {
            bool canDo = false;
            if ( justTimeKeeper || justManager || G1.isHR() || G1.isAdmin() || Employees.isManager())
                canDo = true;
            return canDo;
        }
        /***********************************************************************************************/
        private void HideBenefits ()
        {
            if (!G1.isAdmin() && !G1.isHR())
            {
                gridMain2.Columns["approvedVacation"].Visible = false;
                gridMain2.Columns["approvedSick"].Visible = false;
                gridMain2.Columns["availableVacation"].Visible = false;
                gridMain2.Columns["availableSick"].Visible = false;
            }
        }
        /***********************************************************************************************/
        private void LoadFuneralHomeManagers()
        {
            string cmd = "Select * from `funeralhomes`;";
            funDt = G1.get_db_data(cmd);
        }
        /***********************************************************************************************/
        private void HideGridChooser(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                if (!gridMain.Columns[i].Visible)
                    gridMain.Columns[i].OptionsColumn.ShowInCustomizationForm = false;
            }
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("totalHours", gridMain2);
            AddSummaryColumn("hours", gridMain2);
            AddSummaryColumn("week1hours", gridMain2);
            AddSummaryColumn("week2hours", gridMain2);
            AddSummaryColumn("cweek1hours", gridMain2);
            AddSummaryColumn("cweek2hours", gridMain2);
            AddSummaryColumn("pay", gridMain2);
            AddSummaryColumn("othours", gridMain2);
            AddSummaryColumn("otpay", gridMain2);
            AddSummaryColumn("contractHours", gridMain2);
            AddSummaryColumn("contractPay", gridMain2);
            AddSummaryColumn("otherPay", gridMain2);
            AddSummaryColumn("totalPay", gridMain2);
            AddSummaryColumn("vacationhours", gridMain2);
            AddSummaryColumn("vacationpay", gridMain2);
            AddSummaryColumn("holidayhours", gridMain2);
            AddSummaryColumn("holidaypay", gridMain2);
            AddSummaryColumn("sickhours", gridMain2);
            AddSummaryColumn("sickpay", gridMain2);
            AddSummaryColumn("week1othours", gridMain2);
            AddSummaryColumn("week2othours", gridMain2);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void GetAllEmployees()
        {
            this.Cursor = Cursors.WaitCursor;
            dgv.DataSource = null;

            justTimeKeeper = false;
            justManager = false;

            bool isManager = SMFS.AmIaManager();
            if (isManager)
                justManager = true;
            string locations = "";

            DataTable dx = null;

            string cmd = "SELECT DISTINCT * FROM `users` j LEFT JOIN tc_er e ON j.userName = e.username ";
            if (!G1.isHR() && !G1.isAdmin())
            {
                cmd += " WHERE e.`username` = '" + LoginForm.username + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    if (isManager)
                    {
                        cmd = "Select * from `users` j WHERE j.`username` = '" + LoginForm.username + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            return;
                        dx.Columns.Add("isTimeKeeper");
                        dx.Columns.Add("isManager");
                        dx.Columns.Add("location");
                        dx.Rows[0]["isManager"] = "Y";
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                }
                else
                {
                    if (isManager)
                        dx.Rows[0]["isManager"] = "Y";
                }

                if (dx.Rows[0]["isManager"].ObjToString() != "Y")
                {
                    if (dx.Rows[0]["isTimeKeeper"].ObjToString() != "Y")
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                }

                string name = dx.Rows[0]["lastName"].ObjToString() + ", " + dx.Rows[0]["firstName"].ObjToString();
                string location = "";
                if (isManager)
                {
                    location = dx.Rows[0]["location"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( location))
                        location = dx.Rows[0]["assignedLocations"].ObjToString();
                }

                if (dx.Rows[0]["isManager"].ObjToString() == "Y")
                    justManager = true;
                if (dx.Rows[0]["isTimeKeeper"].ObjToString() == "Y")
                {
                    justTimeKeeper = true;
                    if ( !isManager )
                        location = dx.Rows[0]["location"].ObjToString();
                }
                if (justManager || justTimeKeeper)
                {
                    if (!String.IsNullOrWhiteSpace(location))
                    {
                        string[] Lines = location.Split('~');
                        if (Lines.Length >= 1)
                        {
                            for (int j = 0; j < Lines.Length; j++)
                            {
                                locations += "'" + Lines[j].Trim() + "',";
                            }
                            locations = locations.TrimEnd(',');
                        }
                    }
                }

                cmd = "SELECT DISTINCT * FROM `users` j LEFT JOIN tc_er e ON j.userName = e.username ";
                if (!chkIncludeAll.Checked)
                {
                    cmd += " WHERE j.`noTimeSheet` <> 'Y' ";
                    if (!chkIncludeTerminated.Checked)
                        cmd += " AND j.`termDate` <= '1000-01-01' ";
                }
                else if (!chkIncludeTerminated.Checked)
                    cmd += " WHERE j.`termDate` <= '1000-01-01' ";
                if (justTimeKeeper)
                {
                    if (!String.IsNullOrWhiteSpace(location))
                    {
                        if (!String.IsNullOrWhiteSpace(locations))
                            cmd += " AND ( `TimeKeeper` = '" + name + "' ) OR (`location` IN (" + locations + ") ) ";
                        else
                            cmd += " AND ( `TimeKeeper` = '" + name + "' OR `location` = '" + location + "' ) ";
                    }
                    else
                        cmd += " AND `TimeKeeper` = '" + name + "' ";
                }
                else if (justManager)
                {
                    if (!String.IsNullOrWhiteSpace(locations))
                    {
                        if (!String.IsNullOrWhiteSpace(locations))
                            cmd += " AND (`location` IN (" + locations + ") ) ";
                    }
                }


                cmd += " ORDER BY j.`lastName`, j.`firstName`";

                dx = G1.get_db_data(cmd);

                //MessageBox.Show("IS Man=" + isManager + " Locations=" + locations );


                //MessageBox.Show(cmd);
            }
            else
            {
                chkIncludeAll.Checked = true;
                chkIncludeTerminated.Checked = true;
                cmd = "SELECT DISTINCT * FROM `users` j LEFT JOIN tc_er e ON j.userName = e.username ";
                if (!chkIncludeAll.Checked)
                {
                    cmd += " WHERE j.`noTimeSheet` <> 'Y' ";
                    if (!chkIncludeTerminated.Checked)
                        cmd += " AND j.`termDate` <= '1000-01-01' ";
                }
                else if (!chkIncludeTerminated.Checked)
                    cmd += " WHERE j.`termDate` <= '1000-01-01' ";
                cmd += " ORDER BY j.`lastName`, j.`firstName`";

                dx = G1.get_db_data(cmd);
            }

            if (isManager)
                justManager = true;


            dx.Columns.Add("service");
            dx.Columns.Add("mod");
            if (G1.get_column_number(dx, "employeeApproved") < 0)
                dx.Columns.Add("employeeApproved");
            if (G1.get_column_number(dx, "managerApproved") < 0)
                dx.Columns.Add("managerApproved");


            Bitmap emptyImage = new Bitmap(1, 1);

            string local_user = Environment.GetEnvironmentVariable("USERNAME").ToUpper();
            string local_profile = Environment.GetEnvironmentVariable("USERPROFILE").ToUpper();



            string date = "";
            DateTime ddate = DateTime.Now;

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                ddate = dx.Rows[i]["hireDate"].ObjToDateTime();
                dx.Rows[i]["service"] = calc_age(ddate.ToString("MM/dd/yyyy"));
                //if (local_user.ToUpper() == "ROBBY")
                //{
                //    double money = dx.Rows[i]["rate"].ObjToDouble();
                //    dRow["rate"] = money.ToString("###.00");
                //}
                //dRow["supervisor"] = dx.Rows[i]["preferred_supervisor"].ObjToString();

                //Byte[] bytes = dx.Rows[i]["picture"].ObjToBytes();
                //Image myImage = emptyImage;
                //if (bytes != null)
                //{
                //    myImage = G1.byteArrayToImage(bytes);
                //    dRow["picture"] = (Bitmap)(myImage);
                //}
            }
            //LoadSupervisors(timDt);
            //VerifySupervisor(timDt);
            //LoadEmrNo(timDt);
            //CheckForSupervisor();
            //if (local_user.ToString() != "ROBBY")
            //    gridMain.Columns["rate"].Visible = false;

            if (!G1.isHR())
                gridMain.Columns["rate"].Visible = false;

            SetupSelection(dx, this.repositoryItemCheckEdit1, "excludePayroll");
            SetupSelection(dx, this.repositoryItemCheckEdit1, "isSupervisor");
            SetupSelection(dx, this.repositoryItemCheckEdit1, "isManager");
            SetupSelection(dx, this.repositoryItemCheckEdit1, "isTimeKeeper");
            SetupSelection(dx, this.repositoryItemCheckEdit1, "noTimeSheet");
            SetupSelection(dx, this.repositoryItemCheckEdit1, "isBPM");
            SetupSelection(dx, this.repositoryItemCheckEdit1, "salaried");
            SetupSelection(dx, this.repositoryItemCheckEdit1, "flux");

            LoadManagers(dx);

            dx = LoadBenefitsTaken(dx);

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;

            //SetupEmployeeTimes();

            DataTable empDt = dx.Copy();

            empDt.Columns.Add("hours", Type.GetType("System.Double"));
            empDt.Columns.Add("week1hours", Type.GetType("System.Double"));
            empDt.Columns.Add("week2hours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek1hours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek2hours", Type.GetType("System.Double"));
            empDt.Columns.Add("pay", Type.GetType("System.Double"));
            empDt.Columns.Add("othours", Type.GetType("System.Double"));
            empDt.Columns.Add("otpay", Type.GetType("System.Double"));
            empDt.Columns.Add("contractHours", Type.GetType("System.Double"));
            empDt.Columns.Add("contractPay", Type.GetType("System.Double"));
            empDt.Columns.Add("otherPay", Type.GetType("System.Double"));
            empDt.Columns.Add("totalHours", Type.GetType("System.Double"));
            empDt.Columns.Add("totalPay", Type.GetType("System.Double"));

            empDt.Columns.Add("vacationhours", Type.GetType("System.Double"));
            empDt.Columns.Add("holidayhours", Type.GetType("System.Double"));
            empDt.Columns.Add("sickhours", Type.GetType("System.Double"));
            empDt.Columns.Add("vacationpay", Type.GetType("System.Double"));
            empDt.Columns.Add("holidaypay", Type.GetType("System.Double"));
            empDt.Columns.Add("sickpay", Type.GetType("System.Double"));

            //empDt.Columns.Add("employeeApproved");
            //empDt.Columns.Add("managerApproved");

            empDt = LoadApprovals(empDt);

            G1.NumberDataTable(empDt);
            dgv2.DataSource = empDt;
            mainDt = empDt;

            CleanupColumns();

            bool canDo = CheckCanDo();
            if (canDo)
                LoadTimeOffRequests();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void CleanupColumns ()
        {
            if (!G1.isHR())
            {
                gridMain2.Columns["pay"].Visible = false;
                gridMain2.Columns["contractPay"].Visible = false;
                gridMain2.Columns["otherPay"].Visible = false;
                gridMain2.Columns["otpay"].Visible = false;
                gridMain2.Columns["totalPay"].Visible = false;
                gridMain2.Columns["vacationpay"].Visible = false;
                gridMain2.Columns["holidaypay"].Visible = false;
                gridMain2.Columns["sickpay"].Visible = false;
                gridMain2.Columns["rate"].Visible = false;
                gridMain.Columns["rate"].Visible = false;

                if (!G1.isAdmin())
                {
                    gridMain2.Columns["week1othours"].Visible = false;
                    gridMain2.Columns["week2othours"].Visible = false;
                    gridMain2.Columns["hours"].Visible = false;
                    gridMain2.Columns["othours"].Visible = false;
                    gridMain2.Columns["contractHours"].Visible = false;
                    gridMain2.Columns["EmpType"].Visible = false;

                    cmbSelectColumns.Hide();
                    btnSelectColumns.Hide();

                    label5.Hide();
                    cmbExemptNonExempt.Hide();
                }
            }

            if (!G1.isHR())
            {
                gridMain.DestroyCustomization();
                HideGridChooser(gridMain);
                gridMain2.DestroyCustomization();
                HideGridChooser(gridMain2);
            }
        }
        /****************************************************************************************/
        private DataTable LoadBenefitsTaken ( DataTable dt )
        {
            DateTime startdate = this.dateTimePicker1.Value;
            string startDate = startdate.ToString("yyyyMMdd");
            DateTime stopdate = this.dateTimePicker2.Value;
            string endDate = stopdate.ToString("yyyyMMdd");

            dt = VerifyDouble(dt, "approvedVacation");
            dt = VerifyDouble(dt, "approvedSick");
            dt = VerifyDouble(dt, "approvedHoliday");

            dt = VerifyDouble(dt, "availableVacation");
            dt = VerifyDouble(dt, "availableSick");

            DateTime date = new DateTime(stopdate.Year, 1, 1);
            if (startdate < date)
                startdate = date;
            startDate = startdate.ToString("yyyyMMdd");
            DateTime hireDate = DateTime.Now;

            string cmd = "Select * from `tc_approvals` a JOIN `tc_pay` t ON a.`username` = t.`username` where a.`enddate` >= '" + startDate + "' AND a.`enddate` <= '" + endDate + "' AND `managerApproved` = 'Y' ORDER by a.`username`;";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                DataView tempview = dx.DefaultView;
                tempview.Sort = "username";
                dx = tempview.ToTable();

                DataRow[] dRows = null;
                string username = "";
                string oldUsername = "";
                double totalVacation = 0D;
                double totalSick = 0D;
                double totalHoliday = 0D;
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    username = dx.Rows[i]["username"].ObjToString();
                    if ( username.ToUpper() == "KEITHS")
                    {
                    }
                    if (String.IsNullOrWhiteSpace(oldUsername))
                        oldUsername = username;
                    if ( username != oldUsername )
                    {
                        if ( oldUsername.ToUpper() == "KEITHS")
                        {
                        }
                        dRows = dt.Select("username='" + oldUsername + "'");
                        if ( dRows.Length > 0 )
                        {
                            if ( totalVacation > 0D )
                            {
                            }
                            dRows[0]["approvedVacation"] = totalVacation;
                            dRows[0]["approvedSick"] = totalSick;
                            dRows[0]["approvedHoliday"] = totalHoliday;
                        }
                        oldUsername = username;
                        totalVacation = 0D;
                        totalSick = 0D;
                        totalHoliday = 0D;
                    }
                    totalVacation += dx.Rows[i]["vacationHours1"].ObjToDouble();
                    totalSick += dx.Rows[i]["sickHours1"].ObjToDouble();
                    totalHoliday += dx.Rows[i]["holidayHours1"].ObjToDouble();
                }
                if ( !String.IsNullOrWhiteSpace ( oldUsername ))
                {
                    if (oldUsername.ToUpper() == "KEITHS")
                    {
                    }
                    dRows = dt.Select("username='" + oldUsername + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["approvedVacation"] = totalVacation;
                        dRows[0]["approvedSick"] = totalSick;
                        dRows[0]["approvedHoliday"] = totalHoliday;
                    }
                }
            }

            double yearlyVacation = 0D;
            double yearlySick = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                hireDate = dt.Rows[i]["hireDate"].ObjToDateTime();
                SetupBenefits(hireDate, stopdate, ref yearlyVacation, ref yearlySick);
                dt.Rows[i]["availableVacation"] = yearlyVacation;
                dt.Rows[i]["availableSick"] = yearlySick;
            }
            return dt;
        }
        /***************************************************************************************/
        private void RecalcAllBenefits ()
        {
            DateTime startdate = this.dateTimePicker1.Value;
            string startDate = startdate.ToString("yyyyMMdd");
            DateTime stopdate = this.dateTimePicker2.Value;
            string endDate = stopdate.ToString("yyyyMMdd");

            DataTable dt = (DataTable)dgv.DataSource;

            dt = VerifyDouble(dt, "approvedVacation");
            dt = VerifyDouble(dt, "approvedSick");
            dt = VerifyDouble(dt, "approvedHoliday");

            dt = VerifyDouble(dt, "availableVacation");
            dt = VerifyDouble(dt, "availableSick");

            DateTime date = new DateTime(stopdate.Year, 1, 1);
            if (startdate < date)
                startdate = date;
            startDate = startdate.ToString("yyyyMMdd");
            DateTime hireDate = DateTime.Now;

            double yearlyVacation = 0D;
            double yearlySick = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                hireDate = dt.Rows[i]["hireDate"].ObjToDateTime();
                SetupBenefits(hireDate, stopdate, ref yearlyVacation, ref yearlySick);
                dt.Rows[i]["availableVacation"] = yearlyVacation;
                dt.Rows[i]["availableSick"] = yearlySick;
            }

            dgv.DataSource = dt;

            DataTable dx = (DataTable)dgv2.DataSource;

            dx = VerifyDouble(dx, "approvedVacation");
            dx = VerifyDouble(dx, "approvedSick");
            dx = VerifyDouble(dx, "approvedHoliday");

            dx = VerifyDouble(dx, "availableVacation");
            dx = VerifyDouble(dx, "availableSick");

            DataRow[] dRows = null;
            string username = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                username = dx.Rows[i]["username"].ObjToString();
                dRows = dt.Select("username='" + username + "'");
                if ( dRows.Length > 0 )
                {
                    dx.Rows[i]["approvedVacation"] = dRows[0]["approvedVacation"].ObjToDouble();
                    dx.Rows[i]["approvedSick"] = dRows[0]["approvedSick"].ObjToDouble();
                    dx.Rows[i]["approvedHoliday"] = dRows[0]["approvedHoliday"].ObjToDouble();

                    dx.Rows[i]["availableVacation"] = dRows[0]["availableVacation"].ObjToDouble();
                    dx.Rows[i]["availableSick"] = dRows[0]["availableSick"].ObjToDouble();
                }
            }


            HideBenefits();

            dgv2.DataSource = dx;
        }
        /***************************************************************************************/
        public static void SetupBenefits(DateTime hireDate, DateTime endDate, ref double yearlyVacation, ref double yearlySick )
        {
            DateTime now = endDate;
            yearlyVacation = 0D;
            double vacationTaken = 0D;
            yearlySick = 0D;
            double sickTaken = 0D;

            if (hireDate.Year < 100)
                return;

            TimeSpan ts = now - hireDate;
            if (ts.TotalDays <= 0)
                return;
            double years = ts.TotalDays / 365D;
            if (years >= 11D)
                yearlyVacation = 15D * 8D;
            else if (years >= 2D)
                yearlyVacation = 10D * 8D;
            else if (years >= 1D)
                yearlyVacation = 5D * 8D;

            int iyears = 0;
            int months = 0;
            int days = 0;

            G1.CalculateYourAge(hireDate, now, ref iyears, ref months, ref days);

            //str = G1.ReformatMoney(yearlyVacation);
            //txtYearlyVacatiion.Text = str;

            if (years >= 10D)
                yearlySick = 10D * 8D;
            else if (years >= 6D)
                yearlySick = 6D * 8D;
            else if (years >= 2D)
                yearlySick = 4D * 8D;
            else if (years >= 1D)
                yearlySick = 2D * 8D;

            //str = G1.ReformatMoney(yearlySick);
            //txtYearlySick.Text = str;
        }
        /***************************************************************************************/
        private void SetupBenefits( DataRow [] dRows, DateTime endDate )
        {
            DateTime hireDate = dRows[0]["hireDate"].ObjToDateTime();

            DateTime now = endDate;
            double yearlyVacation = 0D;
            double vacationTaken = 0D;
            double yearlySick = 0D;
            double sickTaken = 0D;

            if (hireDate.Year < 100)
                return;

            TimeSpan ts = now - hireDate;
            if (ts.TotalDays <= 0)
                return;
            double years = ts.TotalDays / 365D;
            if (years >= 11D)
                yearlyVacation = 15D;
            else if (years >= 2D)
                yearlyVacation = 10D;
            else if (years >= 1D)
                yearlyVacation = 5D;

            int iyears = 0;
            int months = 0;
            int days = 0;

            G1.CalculateYourAge(hireDate, now, ref iyears, ref months, ref days);

            //str = G1.ReformatMoney(yearlyVacation);
            //txtYearlyVacatiion.Text = str;

            if (years >= 10D)
                yearlySick = 10D;
            else if (years >= 6D)
                yearlySick = 6D;
            else if (years >= 2D)
                yearlySick = 4D;
            else if (years >= 1D)
                yearlySick = 2D;

            //str = G1.ReformatMoney(yearlySick);
            //txtYearlySick.Text = str;
        }
        /****************************************************************************************/
        private void LoadManagers(DataTable dt)
        {
            if (funDt == null)
                return;
            if (funDt.Rows.Count <= 0)
                return;
            if (dt == null)
                return;
            string location = "";
            string manager = "";
            string fName = "";
            string lName = "";
            string isManager = "";
            string[] Lines = null;
            string[] Lines2 = null;
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                Lines2 = location.Split('~');
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
                            dRows = dt.Select("firstName='" + fName + "' AND lastName = '" + lName + "'");
                            if (dRows.Length > 0)
                            {
                                isManager = dRows[0]["isManager"].ObjToString();
                                if (isManager != "Y")
                                    dRows[0]["isManager"] = "Y";
                            }
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private DataTable LoadApprovals(DataTable empDt)
        {
            if (G1.get_column_number(empDt, "employeeApproved") < 0)
                empDt.Columns.Add("employeeApproved");
            if (G1.get_column_number(empDt, "managerApproved") < 0)
                empDt.Columns.Add("managerApproved");

            DateTime timePeriod1 = dateTimePicker1.Value;
            DateTime timePeriod2 = dateTimePicker2.Value;
            string sDate = timePeriod1.ToString("yyyyMMdd");
            string eDate = timePeriod2.ToString("yyyyMMdd");

            string cmd = "SELECT * from `tc_approvals` WHERE `startdate` = '" + sDate + "' AND `endDate` = '" + eDate + "';";
            DataTable dx = G1.get_db_data(cmd);
            string user = "";
            DataRow[] dRows = null;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                user = dx.Rows[i]["username"].ObjToString();
                if (String.IsNullOrWhiteSpace(user))
                    continue;
                dRows = empDt.Select("username='" + user + "'");
                if (dRows.Length > 0)
                {
                    dRows[0]["employeeApproved"] = dx.Rows[i]["employeeApproved"].ObjToString();
                    dRows[0]["managerApproved"] = dx.Rows[i]["managerApproved"].ObjToString();
                }
            }
            return empDt;
        }
        /****************************************************************************************/
        private void SetupEmployeeTimes()
        {
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
            catch ( Exception ex )
            {
                startDate = DateTime.Now;
            }
            //int count = 0;
            //for (; ; )
            //{
            //    DayOfWeek dow = now.DayOfWeek;
            //    if (dow == DayOfWeek.Friday)
            //    {
            //        count++;
            //        if (count >= 2)
            //        {
            //            startDate = now;
            //            break;
            //        }
            //        now = now.AddDays(-1);
            //        continue;
            //    }
            //    now = now.AddDays(-1);
            //}

            newDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
            startDate = newDate;
            stopDate = startDate.AddDays(14);
            newDate = new DateTime(stopDate.Year, stopDate.Month, stopDate.Day, 23, 59, 59);
            stopDate = newDate;

            this.dateTimePicker1.Value = startDate;
            this.dateTimePicker2.Value = stopDate;

            stopDate = this.dateTimePicker2.Value;
            DateTime checkDate = stopDate.AddDays(-14);
            DateTime date1 = new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, 17, 0, 0);
            if (DateTime.Now <= date1)
            {
                this.dateTimePicker1.Value = this.dateTimePicker1.Value.AddDays(-14);
                this.dateTimePicker2.Value = this.dateTimePicker2.Value.AddDays(-14);
            }
        }
        /***********************************************************************************************/
        private void LoadPayrollTimeKeepers()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            DataRow[] dRows = dt.Select("isTimeKeeper='Y'");
            cmbSuper.Items.Clear();
            cmbSuper.Items.Add("All");
            cmbSuper.Text = "All";
            string name = "";
            for (int i = 0; i < dRows.Length; i++)
            {
                name = dRows[i]["lastName"].ObjToString() + ", " + dRows[i]["firstName"].ObjToString();
                cmbSuper.Items.Add(name);
            }
        }
        /****************************************************************************************/
        private void LoadLocations()
        {
            string cmd = "Select * from `funeralhomes`;";
            DataTable dx = G1.get_db_data(cmd);

            DataView tempview = dx.DefaultView;
            tempview.Sort = "LocationCode";
            dx = tempview.ToTable();

            DataRow dR = dx.NewRow();
            dR["LocationCode"] = "Home Office";
            dx.Rows.InsertAt(dR, 0);

            this.repositoryItemComboBox4.Items.Clear();
            for (int i = 0; i < dx.Rows.Count; i++)
                this.repositoryItemComboBox4.Items.Add(dx.Rows[i]["LocationCode"].ObjToString());
            cmbLocation.Items.Add("All");
            for (int i = 0; i < dx.Rows.Count; i++)
                cmbLocation.Items.Add(dx.Rows[i]["LocationCode"].ObjToString());

            cmbLocation.Text = "All";
        }
        /***********************************************************************************************/
        private string calc_age(string hiredate)
        {
            DateTime date = G1.ParseDateTime(hiredate);
            return G1.GetAge(date, DateTime.Today).ToString();
        }
        /***********************************************************************************************/
        private void CheckForSupervisor()
        {
            string answer = G1.getPreference(LoginForm.username, "Super Menu", "Allow Access");
            if (answer != "YES")
            {
                gridMain.Columns["jobtype"].Visible = false;
                gridMain.Columns["jobcode"].Visible = false;
            }
        }
        /***********************************************************************************************/
        private void LoadSupervisors(DataTable dt)
        {
            string empno = "";
            string jobcode = "";
            string cmd = "Select * from `jobs`;";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jobcode = dt.Rows[i]["jobcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(jobcode))
                    continue;
                try
                {
                    DataRow[] dRows = dx.Select("`jobcode` = '" + jobcode + "'");
                    if (dRows.Length > 0)
                    {
                        empno = dRows[0]["super"].ObjToString();
                        cmd = "Select * from `er` where `empno` = '" + empno + "';";
                        DataTable dd = G1.get_db_data(cmd);
                        if (dd.Rows.Count > 0)
                        {
                            string preferred_supervisor = dd.Rows[0]["preferred_supervisor"].ObjToString();
                            if (String.IsNullOrWhiteSpace(preferred_supervisor))
                                dt.Rows[i]["supervisor"] = dd.Rows[0]["name"].ObjToString();
                            else
                                dt.Rows[i]["supervisor"] = preferred_supervisor;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR**** On Jobcode " + jobcode + "!");
                }
            }
        }
        /***********************************************************************************************/
        private void VerifySupervisor(DataTable dt)
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
                        dt.Rows[i]["supervisor"] = super;
                }
            }
        }
        /***********************************************************************************************/
        private void LoadEmrNo(DataTable dt)
        {
            string empno = "";
            string emrno = "";
            string fname = "";
            string lname = "";
            string cmd = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                empno = dt.Rows[i]["empno"].ObjToString();
                cmd = "Select * from `er` where `empno` = '" + empno + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count == 0)
                    continue;
                emrno = dx.Rows[0]["emrno"].ObjToString();
                if (!string.IsNullOrWhiteSpace(emrno) && emrno != "0")
                    continue;
                fname = dx.Rows[0]["firstname"].ObjToString();
                lname = dx.Rows[0]["lastname"].ObjToString();
                cmd = "Select * from `users` where `lname` = '" + lname + "' and `fname` = '" + fname + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    dt.Rows[i]["emrno"] = dx.Rows[0]["userid"].ObjToString();
            }
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            e.RowHeight = e.RowHeight * 4;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            DataTable dt = (DataTable)(dgv.DataSource);
            int row = e.RowHandle;
            if (e.Column.FieldName.ToUpper() == "NUM")
                e.DisplayText = (row + 1).ToString();
            else if (e.Column.FieldName.ToUpper() == "RATE")
            {
                if (!showRates)
                    e.DisplayText = "***.**";
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            DataTable dt = (DataTable)dgv.DataSource;

            string empno = dr["record"].ObjToString();
            string name = dr["firstName"].ObjToString() + " " + dr["lastName"].ObjToString();
            string userName = dr["userName"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            //TimeClock timeForm = new TimeClock(empno, userName, name);
            //timeForm.Show();

            EmployeeDemo employeeForm = new EmployeeDemo(empno);
            employeeForm.EmployeeDone += EmployeeForm_EmployeeDone;
            employeeForm.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void EmployeeForm_EmployeeDone(string empNo)
        {
            string cmd = "";
            DataTable dx = null;
            //string userName = s;
            string workRecord = empNo;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow[] dRows = dt.Select("record='" + workRecord + "'");
            if (dRows.Length > 0)
            {
                cmd = "Select * from users WHERE `record` = '" + workRecord + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;
                string username = dx.Rows[0]["userName"].ObjToString();
                dRows[0]["firstName"] = dx.Rows[0]["firstName"].ObjToString();
                dRows[0]["lastName"] = dx.Rows[0]["lastName"].ObjToString();
                dRows[0]["userName"] = username;

                DateTime date = dx.Rows[0]["birthDate"].ObjToDateTime();
                dRows[0]["birthDate"] = G1.DTtoMySQLDT(date);

                date = dx.Rows[0]["hireDate"].ObjToDateTime();
                dRows[0]["hireDate"] = G1.DTtoMySQLDT(date);

                date = dx.Rows[0]["termDate"].ObjToDateTime();
                dRows[0]["termDate"] = G1.DTtoMySQLDT(date);

                gridMain.RefreshEditor(true);

                cmd = "Select * from `tc_er` WHERE `username` = '" + username + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;
                string isTimeKeeper = dx.Rows[0]["isTimeKeeper"].ObjToString();
                dRows[0]["isTimeKeeper"] = isTimeKeeper;

                string isManager = dx.Rows[0]["isManager"].ObjToString();
                dRows[0]["isManager"] = isManager;

                string isSupervisor = dx.Rows[0]["isSupervisor"].ObjToString();
                dRows[0]["isSupervisor"] = isSupervisor;

                string salaried = dx.Rows[0]["salaried"].ObjToString();
                dRows[0]["salaried"] = salaried;

                string isBiWeekly = dx.Rows[0]["isBiWeekly"].ObjToString();
                dRows[0]["isBiWeekly"] = isBiWeekly;

                string isBPM = dx.Rows[0]["isBPM"].ObjToString();
                dRows[0]["isBPM"] = isBPM;

                string flux = dx.Rows[0]["flux"].ObjToString();
                dRows[0]["flux"] = flux;

                string timeKeeper = dx.Rows[0]["timeKeeper"].ObjToString();
                string empStatus = dx.Rows[0]["empStatus"].ObjToString();
                string empType = dx.Rows[0]["empType"].ObjToString();

                dRows[0]["TimeKeeper"] = timeKeeper;
                dRows[0]["empStatus"] = empStatus;
                dRows[0]["empType"] = empType;


                gridMain.RefreshEditor(true);
            }
        }
        ///***********************************************************************************************/
        //private void EmployeeForm_CustomerModifiedDone(string s, string x)
        //{
        //    if (String.IsNullOrWhiteSpace(s))
        //        return;
        //    if (string.IsNullOrWhiteSpace(x))
        //        return;

        //    string cmd = "";
        //    DataTable dx = null;
        //    string userName = s;
        //    string workRecord = x;
        //    DataTable dt = (DataTable)dgv.DataSource;
        //    if (dt.Rows.Count <= 0)
        //        return;
        //    DataRow[] dRows = dt.Select("record='" + workRecord + "'");
        //    if (dRows.Length > 0)
        //    {
        //        cmd = "Select * from users WHERE `record` = '" + workRecord + "';";
        //        dx = G1.get_db_data(cmd);
        //        if (dx.Rows.Count <= 0)
        //            return;
        //        string username = dx.Rows[0]["userName"].ObjToString();
        //        dRows[0]["firstName"] = dx.Rows[0]["firstName"].ObjToString();
        //        dRows[0]["lastName"] = dx.Rows[0]["lastName"].ObjToString();
        //        dRows[0]["userName"] = username;

        //        DateTime date = dx.Rows[0]["birthDate"].ObjToDateTime();
        //        dRows[0]["birthDate"] = G1.DTtoMySQLDT(date);

        //        date = dx.Rows[0]["hireDate"].ObjToDateTime();
        //        dRows[0]["hireDate"] = G1.DTtoMySQLDT(date);

        //        date = dx.Rows[0]["termDate"].ObjToDateTime();
        //        dRows[0]["termDate"] = G1.DTtoMySQLDT(date);

        //        gridMain.RefreshEditor(true);

        //        cmd = "Select * from `tc_er` WHERE `username` = '" + username + "';";
        //        dx = G1.get_db_data(cmd);
        //        if (dx.Rows.Count <= 0)
        //            return;
        //        string isTimeKeeper = dx.Rows[0]["isTimeKeeper"].ObjToString();
        //        dRows[0]["isTimeKeeper"] = isTimeKeeper;

        //        string timeKeeper = dx.Rows[0]["timeKeeper"].ObjToString();
        //        string empStatus = dx.Rows[0]["empStatus"].ObjToString();
        //        string empType = dx.Rows[0]["empType"].ObjToString();

        //        dRows[0]["TimeKeeper"] = timeKeeper;
        //        dRows[0]["empStatus"] = empStatus;
        //        dRows[0]["empType"] = empType;


        //        gridMain.RefreshEditor(true);
        //    }
        //}
        /***********************************************************************************************/
        private void SaveEmployeePicture(string empno, Image image)
        {
            if (image != null && !String.IsNullOrWhiteSpace(empno))
            {
                if (!String.IsNullOrWhiteSpace(empno))
                {
                    string cmd = "Select * from `er` where `empno` = '" + empno + "';";
                    DataTable dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count > 0)
                    {
                        string record = dd.Rows[0]["record"].ObjToString();
                        Byte[] picture = G1.imageToByteArray(image);
                        //G1.update_paper_blob("er", "record", record, "picture", picture);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private bool gotTerm = false;
        private void chkIncludeTerminated_CheckedChanged(object sender, EventArgs e)
        {
            if (gotTerm)
                return;
            if (loading)
                return;
            gotTerm = true;
            loading = true;
            bool include = chkIncludeTerminated.Checked;

            GetAllEmployees();

            chkIncludeTerminated.Checked = include;

            gotTerm = false;
            loading = false;
        }
        /***********************************************************************************************/
        //private void gridMain_DoubleClick(object sender, EventArgs e)
        //{
        //    string answer = G1.GetPreference("TimeClock", "Take Pictures");
        //    if (answer != "YES")
        //        return;
        //    DataRow dr = gridMain.GetFocusedDataRow();
        //    string empno = dr["empno"].ObjToString();
        //    string emrno = dr["emrno"].ObjToString();
        //    string name = dr["name"].ObjToString();
        //    string cmd = "Select * from `er` where `empno` = '" + empno + "';";
        //    DataTable dx = G1.get_db2_data(cmd);
        //    string record = "";
        //    if (dx.Rows.Count > 0)
        //        record = dx.Rows[0]["record"].ObjToString();
        //    Bitmap map = (Bitmap)(dr["picture"]);
        //    Badge formBadge = new Badge(record, empno, emrno, name, map);
        //    formBadge.Show();
        //}
        /***********************************************************************************************/
        private void LoadTimeKeepers()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            DataRow[] dRows = dt.Select("isTimeKeeper='Y'");
            if (dRows.Length <= 0)
                return;
            DataTable dx = dRows.CopyToDataTable();

            this.repositoryItemComboBox1.Items.Clear();
            string timeKeeper = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                timeKeeper = dx.Rows[i]["lastName"].ObjToString() + ", " + dx.Rows[i]["firstName"].ObjToString();
                this.repositoryItemComboBox1.Items.Add(timeKeeper);
            }
        }
        /***********************************************************************************************/
        private void Contextmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string super = menu.Text;
            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetDataRow(rowHandle);
            string empno = dr["empno"].ObjToString();
            string cmd = "Select * from `er` where `empno` = '" + empno + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string work_record = dt.Rows[0]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(work_record))
                    G1.update_db_table("er", "record", work_record, new string[] { "preferred_supervisor", super });
            }
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
        DataTable SavedSuperDt = null;
        int SavedSuperRow = -1;
        string SavedSupervisor = "";
        string savedJobCodes = "";
        /***********************************************************************************************/
        private void Super_ItemClick(object sender, ItemClickEventArgs e)
        {
            string super = e.Item.Caption.Trim();
            SavedSupervisor = super;
            string jobcodes = e.Item.Tag.ObjToString().Trim();

            if (super.Trim().ToUpper() == "ALL EMPLOYEES" && SavedSuperDt != null)
            {
                dgv.DataSource = SavedSuperDt;
                gridMain.FocusedRowHandle = SavedSuperRow;
                SavedSuperDt = null;
                return;
            }
            if (String.IsNullOrWhiteSpace(super))
            {
                dgv.DataSource = SavedSuperDt;
                gridMain.FocusedRowHandle = SavedSuperRow;
                SavedSuperDt = null;
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

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["supervisor"] = super;

            LoadOtherEmployees(dt, super);
            VerifySupervisor(dt, super);

            dgv.DataSource = dt;
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
        private void xSuper_ItemClick(object sender, ItemClickEventArgs e)
        {
            string super = e.Item.Caption.Trim();
            SavedSupervisor = super;
            string jobcodes = e.Item.Tag.ObjToString().Trim();

            if (super.Trim().ToUpper() == "ALL EMPLOYEES" && SavedSuperDt != null)
            {
                dgv.DataSource = SavedSuperDt;
                gridMain.FocusedRowHandle = SavedSuperRow;
                SavedSuperDt = null;
                return;
            }
            if (String.IsNullOrWhiteSpace(super))
            {
                dgv.DataSource = SavedSuperDt;
                gridMain.FocusedRowHandle = SavedSuperRow;
                SavedSuperDt = null;
                return;
            }

            if (SavedSuperDt == null)
            {
                SavedSuperDt = (DataTable)dgv.DataSource;
                SavedSuperRow = gridMain.FocusedRowHandle;
            }
            string cmd = jobcodes.TrimEnd(',');

            savedJobCodes = cmd;
            DataRow[] dRows = SavedSuperDt.Select("`supervisor` = '" + super + "'");

            DataTable dt = SavedSuperDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            dt.AcceptChanges();
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetupPrintPage();
            printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetupPrintPage();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void SetupPrintPage()
        {
            printableComponentLink1.Component = dgv;
            printableComponentLink1.Landscape = true;
            if (dgv2.Visible)
            {
                printableComponentLink1.Component = dgv2;
                printableComponentLink1.Landscape = true;
                //printingSystem1.Document.AutoFitToPagesWidth = 1;
            }
            else if (dgv6.Visible)
            {
                printableComponentLink1.Component = dgv6;
                printableComponentLink1.Landscape = true;
                printingSystem1.Document.AutoFitToPagesWidth = 1;
            }
            else if (dgv7.Visible)
            {
                printableComponentLink1.Component = dgv7;
                printableComponentLink1.Landscape = false;
                printingSystem1.Document.AutoFitToPagesWidth = 1;
            }

            Printer.setupPrinterMargins(10, 10, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
            gridMain.Columns["num"].Visible = false;
            gridMain.Columns["picture"].Width = 40;

        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
            gridMain.Columns["num"].Visible = true;
            gridMain.Columns["picture"].Width = 75;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);

            if (dgv.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Employee List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv2.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Employee Hours Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv7.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Employee Sick Days Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.ToString("MM/dd/yyyy") + " - ";
            date = this.dateTimePicker2.Value;
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
        }

        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderAreax(object sender, CreateAreaEventArgs e)
        {
            // Date in upper left corner
            PageInfoBrick printDate = e.Graph.DrawPageInfo(PageInfo.DateTime, "{0:MM/dd/yyyy HH:mm}", Color.DarkBlue, new RectangleF(0, 0, 200, 18), BorderSide.None);

            // Create and Draw the Report Title, Include Thick bottom border
            DateTime date1 = DateTime.Now;
            string title = "";
            if (SavedSuperRow >= 0)
                title = "SMFS Employees Reporting to " + SavedSupervisor;
            else
                title = "SMFS Employee List";
            if (dgv2.Visible)
                title = "SMFS Employee Hours";

            TextBrick textBrick = e.Graph.DrawString(title, Color.Black, new RectangleF(0, 18, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
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
            pageNumberInfo.Rect = new RectangleF(100, 0, 84, 18);
            pageNumberInfo.HorzAlignment = HorzAlignment.Far;

            bool doit = true;

            // UserName Brick
            string str = "";
            TextBrick userIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            //            userIDLabel.Text = "USERID";
            userIDLabel.Text = "";
            userIDLabel.Rect = new RectangleF(0, 18, 250, 18);
            //PageInfoBrick userIDInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            //userIDInfo.PageInfo = PageInfo.UserName;
            //userIDInfo.Rect = new RectangleF(60, 18, 84, 18);

            // Create RightTopPanel and Paint
            PanelBrick rightTopPanel = new PanelBrick();
            rightTopPanel.BorderWidth = 0;
            rightTopPanel.Bricks.Add(pageNumberLabel);
            rightTopPanel.Bricks.Add(pageNumberInfo);
            if (doit)
                rightTopPanel.Bricks.Add(userIDLabel);
            //            rightTopPanel.Bricks.Add(userIDInfo);
            //            e.Graph.DrawBrick(rightTopPanel, new RectangleF(816, 0, 144, 36));
            e.Graph.DrawBrick(rightTopPanel, new RectangleF(0, 45, 250, 36));

            // File Date Brick
            TextBrick fileIDLabel = new TextBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.DarkBlue);
            fileIDLabel.Text = "TimeSheet Date";
            fileIDLabel.Rect = new RectangleF(0, 80, 144, 18);
            //title = this.dateTimePicker1.Text;
            //title += " -to- " + this.dateTimePicker2.Text;
            if (SavedSuperRow >= 0)
            {
                title = "Job Codes (" + savedJobCodes + ")";
                TextBrick fileBrick = e.Graph.DrawString(title, Color.Navy, new RectangleF(0, 80, e.Graph.ClientPageSize.Width, 24), DevExpress.XtraPrinting.BorderSide.Bottom);
                fileBrick.BorderWidth = 2;
                fileBrick.Font = new Font("Arial", 16);
                fileBrick.HorzAlignment = HorzAlignment.Center;
                fileBrick.VertAlignment = VertAlignment.Top;
            }
        }
        /***********************************************************************************************/
        private bool gotInclude = false;
        private void chkIncludeAll_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (gotInclude)
                return;
            gotInclude = true;
            string save = CheckForSave();
            if (save.ToUpper() == "CANCEL")
                return;
            bool include = chkIncludeAll.Checked;

            GetAllEmployees();
            loading = true;
            chkIncludeAll.Checked = include;
            loading = false;
            gotInclude = false;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = null, string columnName = "")
        {
            if (selectnew == null)
                selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = null;
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "N";
            selectnew.ValueGrayed = null;
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][columnName].ObjToString().ToUpper() != "Y")
                    dt.Rows[i][columnName] = "N";
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            DataChanged();
        }
        /***********************************************************************************************/
        private void DataChanged()
        {
            if (loading)
                return;

            btnSave.Show();

            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "1";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            string column = gridMain.FocusedColumn.FieldName.Trim();
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string isOn = dr[column].ObjToString();
            if (isOn.ToUpper() == "Y")
                dr[column] = "N";
            else
                dr[column] = "Y";
            DataChanged();

            if (column.ToUpper() == "ISTIMEKEEPER")
                LoadTimeKeepers();
        }
        /***********************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {

            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                GridColumn column = hitInfo.Column;
                rowHandle = hitInfo.RowHandle;
                string currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper().IndexOf("DATE") >= 0)
                {
                    //DataRow dr = gridMain.GetFocusedDataRow();
                    DataRow dr = gridMain.GetDataRow(rowHandle);
                    DateTime date = dr[currentColumn].ObjToDateTime();
                    using (GetDate dateForm = new GetDate(date, currentColumn))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr[currentColumn] = G1.DTtoMySQLDT(date);
                            gridMain.RefreshEditor(true);
                            dgv.RefreshDataSource();
                            dgv.Refresh();
                            DataChanged();
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private string CheckForSave()
        {
            if (!btnSave.Visible)
                return "";

            DialogResult result = MessageBox.Show("***Question***\nChanges have been made!\nWould you like to SAVE your Changes?", "Employees Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                btnSave.Hide();
                btnSave.Refresh();
                return "";
            }
            if (result == DialogResult.Cancel)
                return "CANCEL";

            btnSave_Click(null, null);
            return "SAVE";
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string mod = "";
            string userName = "";
            string lastName = "";
            string firstName = "";
            DateTime hireDate = DateTime.Now;
            DateTime termDate = DateTime.Now;
            DateTime birthDate = DateTime.Now;
            string isTimeKeeper = "";
            string isManager = "";
            string isSupervisor = "";
            string noTimeSheet = "";
            string timeKeeper = "";
            string salaried = "";
            string isBiWeekly = "";
            string isBPM = "";
            string excludePayroll = "";
            string flux = "";
            string record = "";
            string empRecord = "";
            string empStatus = "";
            string empType = "";
            string location = "";
            string cmd = "";
            DataTable ddd = null;

            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString().ToUpper();
                if (mod != "1")
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                userName = dt.Rows[i]["userName"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                noTimeSheet = dt.Rows[i]["noTimeSheet"].ObjToString();
                hireDate = dt.Rows[i]["hireDate"].ObjToDateTime();
                termDate = dt.Rows[i]["termDate"].ObjToDateTime();
                birthDate = dt.Rows[i]["birthDate"].ObjToDateTime();
                G1.update_db_table("users", "record", record, new string[] { "firstName", firstName, "lastName", lastName, "noTimeSheet", noTimeSheet, "hireDate", hireDate.ToString("MM/dd/yyyy"), "termDate", termDate.ToString("MM/dd/yyyy"), "birthDate", birthDate.ToString("MM/dd/yyyy") });

                record = dt.Rows[i]["record1"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                {
                    cmd = "Select * from `tc_er` where `username` = '" + userName + "';";
                    ddd = G1.get_db_data(cmd);
                    if (ddd.Rows.Count > 0)
                        record = ddd.Rows[0]["record"].ObjToString();
                    else
                        record = G1.create_record("tc_er", "Location", "-1");
                }
                if (G1.BadRecord("tc_er", record))
                {
                    MessageBox.Show("*** ERROR *** Creating TimeSheet Data for " + firstName + " " + lastName + "!", "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    continue;
                }
                dt.Rows[i]["record1"] = record.ObjToInt64();

                isSupervisor = dt.Rows[i]["isSupervisor"].ObjToString();
                isManager = dt.Rows[i]["isManager"].ObjToString();
                isTimeKeeper = dt.Rows[i]["isTimeKeeper"].ObjToString();
                timeKeeper = dt.Rows[i]["timeKeeper"].ObjToString();
                salaried = dt.Rows[i]["salaried"].ObjToString();
                isBiWeekly = dt.Rows[i]["isBiWeekly"].ObjToString();
                isBPM = dt.Rows[i]["isBPM"].ObjToString();
                flux = dt.Rows[i]["flux"].ObjToString();
                location = dt.Rows[i]["Location"].ObjToString();
                empStatus = dt.Rows[i]["empStatus"].ObjToString();
                empType = dt.Rows[i]["empType"].ObjToString();
                excludePayroll = dt.Rows[i]["excludePayroll"].ObjToString();

                G1.update_db_table("tc_er", "record", record, new string[] { "username", userName, "empStatus", empStatus, "empType", empType, "TimeKeeper", timeKeeper, "Location", location, "isTimeKeeper", isTimeKeeper, "isSupervisor", isSupervisor, "isManager", isManager, "salaried", salaried, "isBiWeekly", isBiWeekly, "flux", flux, "isBPM", isBPM, "excludePayroll", excludePayroll, "user", LoginForm.username });
            }
            btnSave.Hide();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            string noTimeSheet = dr["noTimeSheet"].ObjToString();
            if (noTimeSheet.ToUpper() == "Y")
                dr["noTimeSheet"] = "";
            else
                dr["noTimeSheet"] = "Y";
            DataChanged();
        }
        /***********************************************************************************************/
        private void myTimeSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string empno = LoginForm.workUserRecord;

            DateTime start = dateTimePicker1.Value;
            DateTime stop = dateTimePicker2.Value;

            //using (TimeClock timeForm = new TimeClock(start, stop, work_empno, LoginForm.username, work_myName))
            using (TimeClock timeForm = new TimeClock ( work_empno, LoginForm.username, work_myName))
            {
                timeForm.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
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
            else if (e.Column.FieldName.ToUpper().IndexOf("EDITCONTRACT") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dx = (DataTable)dgv.DataSource;
                if (dx == null)
                    return;

                int row = e.ListSourceRowIndex;

                string user = dx.Rows[row]["userName"].ObjToString();
                string empno = dx.Rows[row]["record"].ObjToString();
                //string name = dx.Rows[row]["firstName"].ObjToString() + " " + dr["lastName"].ObjToString();
                string EmpStatus = dx.Rows[row]["EmpStatus"].ObjToString();
                if (EmpStatus.ToUpper().IndexOf("PARTTIME") < 0)
                {
                    e.DisplayText = "XX";
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            if (dx == null)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            ComboBoxEdit edit = (ComboBoxEdit)sender;
            string str = edit.Text;
            dr["TimeKeeper"] = str;

            gridMain.RefreshEditor(true);

            DataChanged();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox2_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            if (dx == null)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            string user = dr["empStatus"].ObjToString();
            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox3_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            if (dx == null)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            string user = dr["empType"].ObjToString();
            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void contractLaborServicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHR() && !G1.isAdmin())
            {
                MessageBox.Show("*** Sorry *** This Function is not available at this time!!", "PartTime Labor Services Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            string empno = LoginForm.workUserRecord;
            EditContractServices contractForm = new EditContractServices( "PartTime", empno);
            contractForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        string oldWhat = "";
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                string column = view.FocusedColumn.FieldName;
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row][column] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                //dt.Rows[row]["mod"] = "Y";
            }
        }
        /***********************************************************************************************/
        private DataTable VerifyDouble ( DataTable dt, string name )
        {
            if (G1.get_column_number(dt, name) < 0)
                dt.Columns.Add(name, Type.GetType("System.Double"));
            return dt;
        }
        /***********************************************************************************************/
        private DataTable VerifyColumns ( DataTable dt )
        {
            dt = VerifyDouble(dt, "hours" );
            dt = VerifyDouble(dt, "week1hours" );
            dt = VerifyDouble(dt, "week2hours");
            dt = VerifyDouble(dt, "cweek1hours");
            dt = VerifyDouble(dt, "cweek2hours");
            dt = VerifyDouble(dt, "pay");
            dt = VerifyDouble(dt, "othours");
            dt = VerifyDouble(dt, "otpay");
            dt = VerifyDouble(dt, "contractHours");
            dt = VerifyDouble(dt, "contractPay");
            dt = VerifyDouble(dt, "otherPay");
            dt = VerifyDouble(dt, "totalHours");
            dt = VerifyDouble(dt, "totalPay");

            dt = VerifyDouble(dt, "vacationhours");
            dt = VerifyDouble(dt, "holidayhours");
            dt = VerifyDouble(dt, "sickhours");
            dt = VerifyDouble(dt, "vacationpay");
            dt = VerifyDouble(dt, "holidaypay");
            dt = VerifyDouble(dt, "sickpay");

            dt = VerifyDouble(dt, "week1vhours");
            dt = VerifyDouble(dt, "week2vhours");

            dt = VerifyDouble(dt, "cweek1vhours");
            dt = VerifyDouble(dt, "cweek2vhours");

            dt = VerifyDouble (dt, "week1hhours");
            dt = VerifyDouble (dt, "week2hhours" );
            dt = VerifyDouble (dt, "cweek1hhours" );
            dt = VerifyDouble (dt, "cweek2hhours" );

            dt = VerifyDouble (dt, "week1shours" );
            dt = VerifyDouble (dt, "week2shours" );
            dt = VerifyDouble (dt, "cweek1shours");
            dt = VerifyDouble (dt, "cweek2shours" );

            dt = VerifyDouble (dt, "week1othours" );
            dt = VerifyDouble (dt, "week2othours" );

            dt = VerifyDouble(dt, "totalothours");
            dt = VerifyDouble(dt, "week1Days");
            dt = VerifyDouble(dt, "week2Days");

            return dt;
        }
        /***********************************************************************************************/
        private DataTable GetPastData ()
        {
            DateTime date = this.dateTimePicker1.Value;
            string startDate = date.ToString("yyyyMMdd");
            date = this.dateTimePicker2.Value;
            string endDate = date.ToString("yyyyMMdd");

            DataTable dx = null;
            string cmd = "";

            if (justTimeKeeper)
            {
                cmd = "SELECT DISTINCT * FROM `users` j LEFT JOIN tc_er e ON j.userName = e.username ";
                if (!G1.isHR() && !G1.isAdmin())
                {
                    cmd += " WHERE e.`username` = '" + LoginForm.username + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return dx;
                    if (dx.Rows[0]["isTimeKeeper"].ObjToString() != "Y")
                        return dx;

                    string name = dx.Rows[0]["lastName"].ObjToString() + ", " + dx.Rows[0]["firstName"].ObjToString();
                    string location = dx.Rows[0]["location"].ObjToString();
                    justTimeKeeper = true;

                    cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "' ";
                    //                    cmd = "SELECT DISTINCT * FROM `users` j LEFT JOIN tc_er e ON j.userName = e.username ";
                    if (!String.IsNullOrWhiteSpace(location))
                    {
                        cmd += " AND ( `TimeKeeper` = '" + name + "' OR `location` = '" + location + "' ) ";
                        //cmd += " AND e.`username` <> '" + LoginForm.username + "' ";
                    }
                    else
                        cmd += " AND `TimeKeeper` = '" + name + "' ";
                    //                    cmd += " ORDER BY j.`lastName`, j.`firstName`";

                    cmd += ";";

                    dx = G1.get_db_data(cmd);
                }
            }
            else
            {

                cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "' ";
                //cmd = "Select * from `tc_pay` p LEFT JOIN `tc_er` r ON p.`username` = r.`username` LEFT JOIN `users` u ON p.`username` = u.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "';";
                dx = G1.get_db_data(cmd);
            }
            return dx;
        }
        /***********************************************************************************************/
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (btnLoad.BackColor != Color.Red)
            {
                DataTable ddb = GetPastData();
                if (ddb.Rows.Count > 0)
                {
                    menuReadPrevious_Click(null, null);
                    return;
                }
            }

            saveRow = gridMain2.FocusedRowHandle;
            saveTopRow = gridMain2.TopRowIndex;

            DataTable dt = (DataTable)dgv.DataSource;

            DataTable empDt = dt.Copy();
            empDt.Columns.Add("hours", Type.GetType("System.Double"));
            empDt.Columns.Add("week1hours", Type.GetType("System.Double"));
            empDt.Columns.Add("week2hours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek1hours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek2hours", Type.GetType("System.Double"));
            empDt.Columns.Add("pay", Type.GetType("System.Double"));
            empDt.Columns.Add("othours", Type.GetType("System.Double"));
            empDt.Columns.Add("otpay", Type.GetType("System.Double"));
            empDt.Columns.Add("contractHours", Type.GetType("System.Double"));
            empDt.Columns.Add("contractPay", Type.GetType("System.Double"));
            empDt.Columns.Add("otherPay", Type.GetType("System.Double"));
            empDt.Columns.Add("totalHours", Type.GetType("System.Double"));
            empDt.Columns.Add("totalPay", Type.GetType("System.Double"));
            empDt.Columns.Add("vacationhours", Type.GetType("System.Double"));
            empDt.Columns.Add("holidayhours", Type.GetType("System.Double"));
            empDt.Columns.Add("sickhours", Type.GetType("System.Double"));
            empDt.Columns.Add("vacationpay", Type.GetType("System.Double"));
            empDt.Columns.Add("holidaypay", Type.GetType("System.Double"));
            empDt.Columns.Add("sickpay", Type.GetType("System.Double"));

            empDt.Columns.Add("week1vhours", Type.GetType("System.Double"));
            empDt.Columns.Add("week2vhours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek1vhours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek2vhours", Type.GetType("System.Double"));

            empDt.Columns.Add("week1hhours", Type.GetType("System.Double"));
            empDt.Columns.Add("week2hhours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek1hhours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek2hhours", Type.GetType("System.Double"));

            empDt.Columns.Add("week1shours", Type.GetType("System.Double"));
            empDt.Columns.Add("week2shours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek1shours", Type.GetType("System.Double"));
            empDt.Columns.Add("cweek2shours", Type.GetType("System.Double"));

            empDt.Columns.Add("week1othours", Type.GetType("System.Double"));
            empDt.Columns.Add("week2othours", Type.GetType("System.Double"));

            DateTime timePeriod1 = dateTimePicker1.Value;
            //timePeriod1 = timePeriod1.AddMinutes(301); // This gets the time to 5:01 PM
            long ldate = G1.TimeToUnix(timePeriod1);

            DateTime timePeriod = dateTimePicker2.Value;
            //timePeriod = timePeriod.AddHours(5); // This gets the time to 4:59 PM
            //timePeriod = timePeriod.AddMinutes(-420); // This gets the time back to 23:59:00
            long edate = G1.TimeToUnix(timePeriod);

            TimeSpan ts = timePeriod - timePeriod1;

            btnSaveData.Hide();
            btnSaveData.Refresh();

            bool showSave = true;
            if ((ts.Days - 1) > 14)
                showSave = false;

            long adate = 0L;

            DateTime firstDate = ldate.UnixToDateTime().ToLocalTime();
            //firstDate = firstDate.AddDays(-1);
            DateTime lastDate = edate.UnixToDateTime().ToLocalTime();
            //lastDate = lastDate.AddDays(-1);

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
            double otherPay = 0D;
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

            double week1hours = 0D;
            double week2hours = 0D;
            double cweek1hours = 0D;
            double cweek2hours = 0D;

            double week1othours = 0D;
            double week2othours = 0D;

            string id = "";
            string punchType = "";
            DataRow[] dRows = null;
            DateTime midDate = firstDate.AddDays(7);

            string userName = "";

            DateTime newDate = DateTime.Now;

            string str = lastDate.ToString("MM/dd/yyyy");
            lastDate = str.ObjToDateTime();
            DateTime cutoffDate = DateTime.Now;

            string salaried = "";
            string isBiWeekly = "";

            DataTable testDt = null;
            dRows = null;
            string testUser = txtUser.Text.Trim().ToUpper();

            //testUser = "147";

            try
            {
                double week2Hours = 0D;
                string cmd = "Select * from `tc_punches_pchs` where `date` >= '" + firstDate.ToString("yyyyMMdd") + "' AND `date` <= '" + lastDate.ToString("yyyyMMdd") + "' ";
                if (!String.IsNullOrWhiteSpace(testUser))
                {
                    if (  G1.validate_numeric ( testUser ))
                        cmd += " AND `empy!AccountingID` = '" + testUser + "' ";
                    else
                        cmd += " AND `username` = '" + testUser + "' ";
                }
                cmd += "order by `empy!AccountingID`,`date`;";
                DataTable dx = G1.get_db_data(cmd);

                //if (!String.IsNullOrWhiteSpace(testUser))
                //{
                //    dRows = dx.Select("username='" + testUser + "'");
                //    if (dRows.Length > 0)
                //    {
                //        testDt = dRows.CopyToDataTable();
                //    }
                //}

                DateTime termDate = DateTime.Now;

                bool gotChanges = false;
                string empNo = "";
                DataTable dxx = null;
                string excludePayroll = "";

                for ( int i=0; i<empDt.Rows.Count; i++)
                {
                    empNo = empDt.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(empNo))
                        continue;
                    dRows = dx.Select("`empy!AccountingID`='" + empNo + "'");
                    if (dRows.Length <= 0)
                        continue;

                    dxx = dRows.CopyToDataTable();

                    CleanupRow(empDt, i);
                    excludePayroll = empDt.Rows[i]["excludePayroll"].ObjToString().ToUpper();
                    if (excludePayroll == "Y")
                        continue;

                    gotChanges = false;
                    dxx = LoadPayrollDetails(dxx, empNo, ref gotChanges); // Load Click

                    if ( gotChanges )
                    {
                        CalculateSpecialPay(empDt, dxx, i);
                    }
                    else
                    {
                        for (int j = 0; j < dxx.Rows.Count; j++)
                        {
                            CalculateEmployeeDetail(empDt, dxx, j);
                        }

                        CalcOvertime(empDt, i);
                    }
                }

                G1.NumberDataTable(empDt);
                dgv2.DataSource = empDt;

                empDt = LoadApprovals(empDt);

                //dgv2.Refresh();

                mainDt = empDt;

                string TimeKeeper = cmbSuper.Text.Trim();
                if (String.IsNullOrWhiteSpace(TimeKeeper) || TimeKeeper.ToUpper() == "ALL")
                {
                    string location = cmbLocation.Text.Trim();
                    if (!String.IsNullOrWhiteSpace(location) && location.ToUpper() != "ALL")
                    {
                        cmbLocation_SelectedIndexChanged(null, null);
                    }
                }
                else
                    cmbSuper_SelectedIndexChanged(null, null);

                //ScaleCells();

                //gridMain2.FocusedColumn = gridMain2.Columns["empStatus"];
                if (saveRow >= 0)
                {
                    gridMain2.TopRowIndex = saveTopRow;
                    gridMain2.FocusedRowHandle = saveRow;
                    gridMain2.SelectRow(saveRow);
                    //gridMain2.FocusedRowHandle = saveRow;
                    saveRow = -1;
                    //gridMain2.RefreshData();
                    //gridMain2.RefreshEditor(true);
                    ScaleCells();
                }
                else
                {
                    gridMain2.RefreshData();
                    dgv2.Refresh();
                }

                SetupTotalsSummary();

                if (G1.isHR() && showSave)
                {
                    btnSaveData.Show();
                    btnSaveData.Refresh();
                    DialogResult result = MessageBox.Show("***Question*** Do you want to SAVE this Pay and Time Data to the Database NOW ?", "Save Time Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                    if (result == DialogResult.Yes)
                    {
                        PleaseWait pleaseForm = new PleaseWait("Please Wait!\nSaving All Payroll Data!");
                        pleaseForm.Show();
                        pleaseForm.Refresh();

                        btnSaveData_Click(null, null);

                        pleaseForm.FireEvent1();
                        pleaseForm.Dispose();
                        pleaseForm = null;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void CalculateSpecialPay (DataTable empDt, DataTable dx, int idx )
        {
            string EmpStatus = "";
            string EmpType = "";
            string salaried = "";
            string flux = "";
            double rate = 0D;
            double biWeekly = 0D;
            double salary = 0D;

            EmpStatus = dx.Rows[0]["EmpStatus"].ObjToString();
            EmpType = dx.Rows[0]["EmpType"].ObjToString();
            salaried = dx.Rows[0]["salaried"].ObjToString();
            flux = dx.Rows[0]["flux"].ObjToString();
            rate = dx.Rows[0]["rate2"].ObjToDouble();
            biWeekly = dx.Rows[0]["biWeekly"].ObjToDouble();
            salary = dx.Rows[0]["salary"].ObjToDouble();

            int lastRow = 0;
            bool different = false;

            DataTable dxx = dx.Clone();

            DataTable empDX = empDt.Clone();
            empDX.ImportRow (empDt.Rows[idx]);

            if (G1.get_column_number(empDX, "week1Days") < 0)
                empDX.Columns.Add("week1Days", Type.GetType("System.Double"));
            if (G1.get_column_number(empDX, "week2Days") < 0)
                empDX.Columns.Add("week2Days", Type.GetType("System.Double"));


            CleanupRow(empDX, 0);

            double dValue = 0D;

            for ( int i=1; i<dx.Rows.Count; i++)
            {
                different = false;
                if (i == (dx.Rows.Count - 1))
                    different = true;
                else
                    different = CheckDifferent(dx, i);
                if (different)
                {
                    if (lastRow > 0)
                    {
                        EmpStatus = dx.Rows[lastRow]["EmpStatus"].ObjToString();
                        EmpType = dx.Rows[lastRow]["EmpType"].ObjToString();
                        salaried = dx.Rows[lastRow]["salaried"].ObjToString();
                        flux = dx.Rows[lastRow]["flux"].ObjToString();
                        rate = dx.Rows[lastRow]["rate2"].ObjToDouble();
                        biWeekly = dx.Rows[lastRow]["biWeekly"].ObjToDouble();
                        salary = dx.Rows[lastRow]["salary"].ObjToDouble();
                    }
                    empDX.Rows[0]["rate"] = rate;
                    empDX.Rows[0]["EmpStatus"] = EmpStatus;
                    empDX.Rows[0]["EmpType"] = EmpType;
                    empDX.Rows[0]["flux"] = flux;
                    empDX.Rows[0]["biWeekly"] = biWeekly;
                    empDX.Rows[0]["salary"] = salary;
                    empDX.Rows[0]["salaried"] = salaried;
                    for ( int j=lastRow; j<=i; j++)
                    {
                        dx.Rows[j]["rate"] = rate;
                        dx.Rows[j]["salaried"] = salaried;

                        dValue = empDX.Rows[0]["week1Days"].ObjToDouble();
                        dValue += dx.Rows[j]["week1Days"].ObjToDouble();
                        empDX.Rows[0]["week1Days"] = dValue;

                        dValue = empDX.Rows[0]["week2Days"].ObjToDouble();
                        dValue += dx.Rows[j]["week2Days"].ObjToDouble();
                        empDX.Rows[0]["week2Days"] = dValue;

                        CalculateEmployeeDetail(empDX, dx, j);
                    }
                    CalcOvertime(empDX, -1, true );


                    lastRow = i + 1;
                }
            }
            AddTogether(empDt, idx, empDX, "hours");
            AddTogether(empDt, idx, empDX, "pay");
            AddTogether(empDt, idx, empDX, "contractHours");
            AddTogether(empDt, idx, empDX, "contractPay");
            AddTogether(empDt, idx, empDX, "otherPay");
            AddTogether(empDt, idx, empDX, "totalPay");
            AddTogether(empDt, idx, empDX, "othours");
            AddTogether(empDt, idx, empDX, "otpay");
            AddTogether(empDt, idx, empDX, "week1hours");
            AddTogether(empDt, idx, empDX, "week2hours");
            AddTogether(empDt, idx, empDX, "cweek1hours");
            AddTogether(empDt, idx, empDX, "cweek2hours");
            AddTogether(empDt, idx, empDX, "vacationhours");
            AddTogether(empDt, idx, empDX, "holidayhours");
            AddTogether(empDt, idx, empDX, "sickhours");
            AddTogether(empDt, idx, empDX, "vacationpay");
            AddTogether(empDt, idx, empDX, "holidaypay");
            AddTogether(empDt, idx, empDX, "sickpay");
            AddTogether(empDt, idx, empDX, "week1vhours");
            AddTogether(empDt, idx, empDX, "week2vhours");
            AddTogether(empDt, idx, empDX, "week1hhours");
            AddTogether(empDt, idx, empDX, "week2hhours");
            AddTogether(empDt, idx, empDX, "week1shours");
            AddTogether(empDt, idx, empDX, "week2shours");
            AddTogether(empDt, idx, empDX, "week1othours");
            AddTogether(empDt, idx, empDX, "week2othours");
            AddTogether(empDt, idx, empDX, "totalHours");
            AddTogether(empDt, idx, empDX, "totalothours");
            AddTogether(empDt, idx, empDX, "week1Days");
            AddTogether(empDt, idx, empDX, "week2Days");
        }
        /***********************************************************************************************/
        private void AddTogether ( DataTable empDt, int i, DataTable empDX, string field )
        {
            try
            {
                if (G1.get_column_number(empDt, field) < 0)
                    empDt.Columns.Add(field, Type.GetType("System.Double"));

                double dValue = empDt.Rows[i][field].ObjToDouble();
                dValue += empDX.Rows[0][field].ObjToDouble();
                dValue = G1.RoundValue(dValue);
                empDt.Rows[i][field] = dValue;
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private bool CheckDifferent ( DataTable dx, int i )
        {
            bool different = false;
            if (dx.Rows[i]["EmpStatus"].ObjToString() != dx.Rows[i + 1]["EmpStatus"].ObjToString())
                return true;
            if (dx.Rows[i]["EmpType"].ObjToString() != dx.Rows[i + 1]["EmpType"].ObjToString())
                return true;
            if (dx.Rows[i]["salaried"].ObjToString() != dx.Rows[i + 1]["salaried"].ObjToString())
                return true;
            if (dx.Rows[i]["flux"].ObjToString() != dx.Rows[i + 1]["flux"].ObjToString())
                return true;
            if (dx.Rows[i]["rate"].ObjToDouble() != dx.Rows[i + 1]["rate"].ObjToDouble())
                return true;
            if (dx.Rows[i]["biWeekly"].ObjToDouble() != dx.Rows[i + 1]["biWeekly"].ObjToDouble())
                return true;
            if (dx.Rows[i]["salary"].ObjToDouble() != dx.Rows[i + 1]["salary"].ObjToDouble())
                return true;
            return different;
        }
        /***********************************************************************************************/
        private void CalculateEmployeeDetail ( DataTable empDt, DataTable dx, int i )
        {
            DateTime timePeriod1 = dateTimePicker1.Value;
            long ldate = G1.TimeToUnix(timePeriod1);

            DateTime timePeriod = dateTimePicker2.Value;
            long edate = G1.TimeToUnix(timePeriod);

            TimeSpan ts = timePeriod - timePeriod1;

            bool showSave = true;
            if ((ts.Days - 1) > 14)
                showSave = false;

            long adate = 0L;

            DateTime firstDate = ldate.UnixToDateTime().ToLocalTime();
            DateTime lastDate = edate.UnixToDateTime().ToLocalTime();

            DateTime firstRealDate = new DateTime(firstDate.Year, firstDate.Month, firstDate.Day, 17, 0, 1); // One (1) Second After 5PM
            DateTime lastRealDate = new DateTime(lastDate.Year, lastDate.Month, lastDate.Day, 17, 0, 0); // Exactly 5PM
            DateTime testDate = DateTime.Now;
            DateTime testDate2 = DateTime.Now;

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
            double otherPay = 0D;
            double totalPay = 0D;
            double midHours = 0D;
            double vacationhours = 0D;
            double holidayhours = 0D;
            double vacationpay = 0D;
            double holidaypay = 0D;

            double vacation = 0D;
            double holiday = 0D;

            double sick = 0D;
            double sickhours = 0D;
            double sickpay = 0D;
            double availableSick = 0D;
            double sickTaken = 0D;

            double week1hours = 0D;
            double week2hours = 0D;
            double cweek1hours = 0D;
            double cweek2hours = 0D;

            double week1othours = 0D;
            double week2othours = 0D;

            string id = "";
            string punchType = "";
            DataRow[] dRows = null;
            DateTime midDate = firstDate.AddDays(7);
            midDate = new DateTime(midDate.Year, midDate.Month, midDate.Day, 17, 0, 0);

            string userName = "";

            DateTime newDate = DateTime.Now;

            string str = lastDate.ToString("MM/dd/yyyy");
            lastDate = str.ObjToDateTime();
            DateTime cutoffDate = DateTime.Now;

            string salaried = "";
            string isBiWeekly = "";

            DateTime termDate = DateTime.Now;
            DateTime hireDate = DateTime.Now;

            DataTable testDt = null;
            dRows = null;

            try
            {
                id = dx.Rows[i]["empy!AccountingID"].ObjToString();
                dRows = empDt.Select("record='" + id + "'");
                if (dRows.Length <= 0)
                    return;
                salaried = dRows[0]["salaried"].ObjToString();
                isBiWeekly = dRows[0]["isBiWeekly"].ObjToString();
                userName = dRows[0]["username"].ObjToString().ToUpper();
                termDate = dRows[0]["termDate"].ObjToDateTime();
                hireDate = dRows[0]["hireDate"].ObjToDateTime();

                if (userName.ToUpper() == "KENA")
                {
                }

                date = dx.Rows[i]["date"].ObjToDateTime();
                if (termDate.Year > 1900)
                {
                    if (termDate < date)
                    {
                        return;
                    }
                }
                if (hireDate > date)
                    return;

                cutoffDate = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0);

                date1 = dx.Rows[i]["timeIn"].ObjToDateTime();
                date1 = new DateTime(date.Year, date.Month, date.Day, date1.Hour, date1.Minute, date1.Second);
                date2 = dx.Rows[i]["timeOut"].ObjToDateTime();
                date2 = new DateTime(date.Year, date.Month, date.Day, date2.Hour, date2.Minute, date2.Second);
                if (date2 > lastRealDate)
                    return;

                vacationhours = dx.Rows[i]["vacation"].ObjToDouble();
                holidayhours = dx.Rows[i]["holiday"].ObjToDouble();
                sickhours = dx.Rows[i]["sick"].ObjToDouble();

                punchType = dx.Rows[i]["punchType"].ObjToString().Trim().ToUpper();
                if (punchType != "OTHER")
                {
                    if (date1 == date2)
                    {
                        if (vacationhours == 0D && holidayhours == 0D && sickhours == 0D)
                            return;
                        date1 = date.AddHours(13);
                        date2 = date.AddHours(13);
                    }
                }

                //date1 = TimeClock.ValidateTime(date1);
                //date2 = TimeClock.ValidateTime(date2);
                punchType = dx.Rows[i]["punchType"].ObjToString().Trim().ToUpper();
                if (punchType.ToUpper() == "OTHER")
                {
                    date1 = date.AddHours(13);
                    date2 = date.AddHours(13);
                }

                punchType = dx.Rows[i]["punchType"].ObjToString().Trim().ToUpper();

                newDate = new DateTime(date.Year, date.Month, date.Day, date1.Hour, date1.Minute, 0);
                date = date.AddDays(1);
                if (newDate < cutoffDate)
                {
                    //if (punchType != "CONTRACT" && punchType != "OTHER")
                    //    date = date.AddDays(1);
                }

                if ( punchType.ToUpper() == "CONTRACT")
                {
                    firstDate = firstDate.AddDays(1);
                    lastDate = lastDate.AddDays(1);
                }

                if (date <= firstDate)
                {
                    str = date2.ToString("MM/dd/yyyy");
                    newDate = str.ObjToDateTime();
                    ts = date1 - newDate;
                    if (ts.TotalMinutes < 1020)
                        return;
                }
                if (date >= lastDate)
                {
                    str = date1.ToString("MM/dd/yyyy");
                    newDate = str.ObjToDateTime();
                    ts = date1 - newDate;
                    if ( date > lastDate )
                    {
                        return;
                    }
                    if (ts.TotalMinutes > 1020)
                        return;
                }

                vacationhours = dx.Rows[i]["vacation"].ObjToDouble();
                holidayhours = dx.Rows[i]["holiday"].ObjToDouble();
                sickhours = dx.Rows[i]["sick"].ObjToDouble();
                if ( sickhours > 0D )
                {
                    VerifySickHours("SICK", userName, hireDate, dx, i, ref sickhours);
                    dx.Rows[i]["sick"] = sickhours;
                }
                if (vacationhours > 0D)
                {
                    VerifySickHours("VACATION", userName, hireDate, dx, i, ref vacationhours);
                    dx.Rows[i]["vacation"] = vacationhours;
                }
                if (userName.ToUpper() == "BILLM")
                {
                }

                ts = date2 - date1;
                hours = ts.TotalHours;
                hours = TimeClock.CalculateTime(date1, date2);
                id = dx.Rows[i]["empy!AccountingID"].ObjToString();
                if (!String.IsNullOrWhiteSpace(id))
                {
                    dRows = empDt.Select("record='" + id + "'");
                    if (dRows.Length > 0)
                    {
                        userName = dRows[0]["username"].ObjToString();
                        if (userName.ToUpper() == "JESSEM")
                        {
                        }
                        rate = dRows[0]["rate"].ObjToDouble();
                        if (vacationhours > 0D)
                        {
                            vacation = dRows[0]["vacationhours"].ObjToDouble();
                            vacation += vacationhours;
                            dRows[0]["vacationhours"] = vacation;
                            dRows[0]["vacationpay"] = vacation * rate;
                        }
                        if (holidayhours > 0D)
                        {
                            holiday = dRows[0]["holidayhours"].ObjToDouble();
                            holiday += holidayhours;
                            dRows[0]["holidayhours"] = holiday;
                            dRows[0]["holidaypay"] = holiday * rate;
                        }
                        if (sickhours > 0D)
                        {
                            sick = dRows[0]["sickhours"].ObjToDouble();
                            sick += sickhours;
                            dRows[0]["sickhours"] = sick;
                            dRows[0]["sickpay"] = sick * rate;
                        }
                        if (punchType == "CONTRACT" && salaried != "Y")
                        {
                            if (userName.ToUpper() == "JESSEM")
                            {
                            }
                            rate = dx.Rows[i]["rate"].ObjToDouble();
                            total = dRows[0]["contractHours"].ObjToDouble();
                            total += hours;
                            dRows[0]["contractHours"] = total;
                            pay = hours * rate;
                            total = pay + dRows[0]["contractPay"].ObjToDouble();
                            dRows[0]["contractPay"] = total;
                            total = pay + dRows[0]["totalPay"].ObjToDouble();
                            dRows[0]["totalPay"] = total;
                            if (date <= midDate)
                            {
                                midHours = dRows[0]["cweek1hours"].ObjToDouble();
                                dRows[0]["cweek1hours"] = midHours + hours;
                            }
                            else
                            {
                                midHours = dRows[0]["cweek2hours"].ObjToDouble();
                                dRows[0]["cweek2hours"] = midHours + hours;
                            }
                        }
                        else if (punchType == "OTHER")
                        {
                            rate = dx.Rows[i]["rate"].ObjToDouble();
                            total = dRows[0]["otherPay"].ObjToDouble();
                            total += rate;
                            dRows[0]["otherPay"] = total;
                            total = pay + dRows[0]["pay"].ObjToDouble() + dRows[0]["contractPay"].ObjToDouble() + total;
                            dRows[0]["totalPay"] = total;
                        }
                        else
                        {
                            rate = dRows[0]["rate"].ObjToDouble();
                            total = dRows[0]["hours"].ObjToDouble();
                            total += hours;
                            dRows[0]["hours"] = total;
                            pay = total * rate;
                            dRows[0]["pay"] = pay;
                            total = pay + dRows[0]["contractPay"].ObjToDouble();
                            dRows[0]["totalPay"] = total;
                            if (date1 <= midDate.AddDays ( -1) )
                            {
                                midHours = dRows[0]["week1hours"].ObjToDouble();
                                total = midHours + hours;
                                dRows[0]["week1hours"] = midHours + hours;
                                AddTo(vacationhours, dRows, 0, "week1vhours");
                                AddTo(sickhours, dRows, 0, "week1shours");
                                AddTo(holidayhours, dRows, 0, "week1hhours");
                            }
                            else
                            {
                                midHours = dRows[0]["week2hours"].ObjToDouble();
                                total = midHours + hours;
                                dRows[0]["week2hours"] = midHours + hours;
                                AddTo(vacationhours, dRows, 0, "week2vhours");
                                AddTo(sickhours, dRows, 0, "week2shours");
                                AddTo(holidayhours, dRows, 0, "week2hhours");
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private void VerifySickHours ( string what, string username, DateTime hireDate, DataTable dxx, int ii, ref double sickHours )
        {
            DateTime beginDate = dateTimePicker1.Value;
            DateTime firstDate = dateTimePicker1.Value;
            int year = firstDate.Year;
            firstDate = new DateTime(year, 1, 1);
            DateTime stopdate = dxx.Rows[ii]["date"].ObjToDateTime();
            stopdate = stopdate.AddDays(1);

            bool gotNextYear = false;
            DateTime lastDate = stopdate;
            if (lastDate.Year > year)
            {
                firstDate = new DateTime(lastDate.Year, 1, 1);
                gotNextYear = true;
            }

            lastDate = beginDate.AddDays(-1);

            double approvedSick = 0D;
            double approvedVacation = 0D;
            double carryOver = 0D;

            string cmd = "Select * from `tc_pay` where `startdate` >= '" + firstDate.ToString("yyyyMMdd") + "' and `enddate` <= '" + lastDate.ToString("yyyyMMdd") + "' ";
            cmd += " AND `username` = '" + username + "' ";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    approvedSick += dx.Rows[j]["sickhours"].ObjToDouble();
                    approvedVacation += dx.Rows[j]["vacationHours"].ObjToDouble();
                }
            }

            DateTime date = DateTime.Now;
            for (int i = 0; i < ii; i++)
            {
                date = dxx.Rows[i]["date"].ObjToDateTime();
                date = date.AddDays(1);
                if (date >= firstDate && date <= stopdate)
                {
                    approvedSick += dxx.Rows[i]["sick"].ObjToDouble();
                    approvedVacation += dxx.Rows[i]["vacation"].ObjToDouble();
                }
            }

            double yearlyVacation = 0D;
            double yearlySick = 0D;

            SetupBenefits(hireDate, stopdate, ref yearlyVacation, ref yearlySick);

            double carryHours = GetCarryOverSick(username, firstDate);
            yearlySick += carryHours;
            if (what.ToUpper() == "SICK")
            {
                if ((approvedSick + sickHours) > yearlySick)
                {
                    sickHours = yearlySick - (approvedSick + sickHours);
                    if (sickHours <= 0D)
                        sickHours = 0D;
                }
            }
            else
            {
                if ((approvedVacation + sickHours) > yearlyVacation)
                {
                    sickHours = yearlyVacation - (approvedVacation + sickHours);
                    if (sickHours <= 0D)
                        sickHours = 0D;
                }
            }
        }
        /***********************************************************************************************/
        private double GetCarryOverSick ( string username, DateTime date )
        {
            double carryHours = 0D;
            string cmd = "Select * from `tc_sick` WHERE `username` = '" + username + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return carryHours;
            int year = date.Year - 1;
            string column = "Y" + year.ToString("D4");
            try
            {
                double carryDays = dt.Rows[0][column].ObjToDouble();
                carryHours = carryDays * 8D;
            }
            catch ( Exception ex )
            {
            }
            return carryHours;
        }
        /***********************************************************************************************/
        private void AddTo(double something, DataRow[] dRows, int idx, string field)
        {
            double from = dRows[idx][field].ObjToDouble();
            double to = from + something;
            dRows[idx][field] = to;
        }
        /***********************************************************************************************/
        private void CalcOvertime(DataTable empDt, int fixedRow = -1, bool proRating = false )
        {
            double rate = 0D;
            double otrate = 0D;
            double hours = 0D;
            double pay = 0D;
            double newHours = 0D;
            double otpay = 0D;
            double total = 0D;
            double contractHours = 0D;
            double contractPay = 0D;
            double otherPay = 0D;
            double totalPay = 0D;
            double vacationhours = 0D;
            double vacationpay = 0D;
            double holidayhours = 0D;
            double holidaypay = 0D;
            double sickhours = 0D;
            double sickpay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double cweek1 = 0D;
            double cweek2 = 0D;
            double week1vhours = 0D;
            double week2vhours = 0D;
            double week1hhours = 0D;
            double week2hhours = 0D;
            double week1shours = 0D;
            double week2shours = 0D;

            double week1hours = 0D;
            double week2hours = 0D;
            double week1othours = 0D;
            double week2othours = 0D;
            double othours = 0D;
            double totalHours = 0D;

            double basePay = 0D;
            double fRate = 0D;

            string empStatus = "";
            string empType = "";
            string salaried = "";
            string isBiWeekly = "";
            string flux = "";
            string username = "";
            string excludePay = "";

            DateTime hireDate = DateTime.Now;
            DateTime termDate = DateTime.Now;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            empDt = VerifyColumns(empDt);

            int startRow = 0;
            int lastRow = empDt.Rows.Count;

            if ( fixedRow >= 0 )
            {
                startRow = fixedRow;
                lastRow = fixedRow + 1;
            }

            for ( int i = startRow; i < lastRow; i++)
            {
                othours = 0D;
                otpay = 0D;
                pay = 0D;

                username = empDt.Rows[i]["username"].ObjToString();

                startDate = dateTimePicker1.Value;
                endDate = dateTimePicker2.Value;

                rate = empDt.Rows[i]["rate"].ObjToDouble();
                hireDate = empDt.Rows[i]["hireDate"].ObjToDateTime();
                termDate = empDt.Rows[i]["termDate"].ObjToDateTime();

                if (hireDate > endDate)
                    continue;
                if (termDate.Year > 1000)
                {
                    if (termDate < startDate)
                        continue;
                }

                empStatus = empDt.Rows[i]["EmpStatus"].ObjToString().ToUpper().Trim(); // FullTime or PartTime or Both
                empType = empDt.Rows[i]["EmpType"].ObjToString().ToUpper().Trim(); // Exempt or Non-Exempt
                empStatus = empStatus.Replace(" ", "");
                empType = empType.Replace(" ", "");

                excludePay = empDt.Rows[i]["excludePayroll"].ObjToString().ToUpper();
                if (excludePay == "Y")
                    continue;

                if (String.IsNullOrWhiteSpace(empStatus) || String.IsNullOrWhiteSpace(empType))
                    continue;

                salaried = empDt.Rows[i]["salaried"].ObjToString().ToUpper();
                isBiWeekly = empDt.Rows[i]["isBiWeekly"].ObjToString().ToUpper();
                flux = empDt.Rows[i]["flux"].ObjToString().ToUpper();

                vacationpay = empDt.Rows[i]["vacationpay"].ObjToDouble();
                holidaypay = empDt.Rows[i]["holidaypay"].ObjToDouble();
                sickpay = empDt.Rows[i]["sickpay"].ObjToDouble();

                otrate = rate * 1.5D;
                otrate = G1.RoundValue(otrate);

                hours = empDt.Rows[i]["hours"].ObjToDouble();
                week1 = empDt.Rows[i]["week1hours"].ObjToDouble();
                cweek1 = empDt.Rows[i]["cweek1hours"].ObjToDouble();
                week2 = empDt.Rows[i]["week2hours"].ObjToDouble();
                cweek2 = empDt.Rows[i]["cweek2hours"].ObjToDouble();

                week1vhours = empDt.Rows[i]["week1vhours"].ObjToDouble();
                week2vhours = empDt.Rows[i]["week2vhours"].ObjToDouble();
                week1hhours = empDt.Rows[i]["week1hhours"].ObjToDouble();
                week2hhours = empDt.Rows[i]["week2hhours"].ObjToDouble();
                week1shours = empDt.Rows[i]["week1shours"].ObjToDouble();
                week2shours = empDt.Rows[i]["week2shours"].ObjToDouble();

                week1hours = week1 + cweek1 - week1vhours - week1shours - week1hhours;
                week2hours = week2 + cweek2 - week2vhours - week2shours - week2hhours;

                week1othours = 0D;
                if (week1hours > 40D)
                    week1othours = week1hours - 40D;
                week2othours = 0D;
                if (week2hours > 40D)
                    week2othours = week2hours - 40D;
                othours = week1othours + week2othours;

                empDt.Rows[i]["week1hours"] = week1;
                empDt.Rows[i]["week2hours"] = week2;
                empDt.Rows[i]["cweek1hours"] = cweek1;
                empDt.Rows[i]["cweek2hours"] = cweek2;
                if (G1.get_column_number(empDt, "week1othours") < 0)
                    empDt.Columns.Add("week1othours", Type.GetType("System.Double"));
                empDt.Rows[i]["week1othours"] = week1othours;

                if (G1.get_column_number(empDt, "week2othours") < 0)
                    empDt.Columns.Add("week2othours", Type.GetType("System.Double"));
                empDt.Rows[i]["week2othours"] = week2othours;

                if (G1.get_column_number(empDt, "othours") < 0)
                    empDt.Columns.Add("othours", Type.GetType("System.Double"));
                empDt.Rows[i]["othours"] = othours;


                if ( RunningSingle )
                {
                }
                if (salaried == "Y")
                    CalculateSalaried(empDt, i, proRating );
                else if ( empStatus == "FULLTIME"  && empType == "NON-EXEMPT")
                    CalculateFullTimeNonExempt(empDt, i);
                else if ( empStatus == "FULLTIME" && empType == "EXEMPT")
                {
                    if ( empStatus == "FULLTIME" && empType == "EXEMPT" )
                        CalculateFullTimeExempt(empDt, i);
                    else
                        CalculateFullTimeNonExempt(empDt, i);
                }
                else if (empStatus == "PARTTIME")
                {
                    //totalPay = empDt.Rows[i]["totalPay"].ObjToDouble();
                    //totalPay = G1.RoundValue(totalPay);
                    //empDt.Rows[i]["totalPay"] = totalPay;
                    CalculatePartTime(empDt, i, proRating );
                }
                else if (empStatus == "FULLTIME" && empType == "NON-EXEMPT")
                    CalculateFullTimeNonExempt(empDt, i);
                else
                {
                    MessageBox.Show("*** Warning ***\nPay Combination Not Supported Yet for\n(" + username + " )\nFluctuating=" + flux + "\n" + empStatus + "\n" + empType, "Employee Pay Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    continue;
                }
                totalHours = empDt.Rows[i]["hours"].ObjToDouble() + empDt.Rows[i]["contractHours"].ObjToDouble();
                empDt.Rows[i]["totalHours"] = totalHours;

                totalPay = empDt.Rows[i]["totalPay"].ObjToDouble();
                totalPay = G1.RoundValue(totalPay);
                if (excludePay == "Y")
                    totalPay = 0D;
                empDt.Rows[i]["totalPay"] = totalPay;

                totalPay = empDt.Rows[i]["Pay"].ObjToDouble();
                totalPay = G1.RoundValue(totalPay);
                if (excludePay == "Y")
                    totalPay = 0D;
                empDt.Rows[i]["Pay"] = totalPay;
            }
        }
        /***********************************************************************************************/
        private void CalculateSalaried(DataTable empDt, int i, bool proRating = false )
        {
            double rate = 0D;
            double otrate = 0D;
            double hours = 0D;
            double pay = 0D;
            double newHours = 0D;
            double otpay = 0D;
            double total = 0D;
            double contractHours = 0D;
            double contractPay = 0D;
            double otherPay = 0D;
            double totalPay = 0D;
            double vacationhours = 0D;
            double vacationpay = 0D;
            double holidayhours = 0D;
            double holidaypay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double cweek1 = 0D;
            double cweek2 = 0D;
            double week1vhours = 0D;
            double week2vhours = 0D;
            double week1hhours = 0D;
            double week2hhours = 0D;
            double week1shours = 0D;
            double week2shours = 0D;

            double week1hours = 0D;
            double week2hours = 0D;
            double week1othours = 0D;
            double week2othours = 0D;
            double othours = 0D;

            double basePay = 0D;
            double fRate = 0D;

            double sickhours = 0D;
            double sickpay = 0D;
            double availableSick = 0D;
            double sickTaken = 0D;

            string empStatus = "";
            string empType = "";
            string salaried = "";
            string isBiWeekly = "";
            string flux = "";
            string username = "";

            othours = 0D;
            otpay = 0D;
            pay = 0D;

            username = empDt.Rows[i]["username"].ObjToString();

            if ( RunningSingle )
            {
            }

            rate = empDt.Rows[i]["rate"].ObjToDouble();
            empStatus = empDt.Rows[i]["EmpStatus"].ObjToString().ToUpper(); // FullTime or PartTime or Both
            empStatus = empStatus.Replace(" ", "");
            empType = empDt.Rows[i]["EmpType"].ObjToString().ToUpper(); // exempt or Non-Exempt
            salaried = empDt.Rows[i]["salaried"].ObjToString().ToUpper();
            isBiWeekly = empDt.Rows[i]["isBiWeekly"].ObjToString().ToUpper();
            flux = empDt.Rows[i]["flux"].ObjToString().ToUpper();
            string noTimeSheet = empDt.Rows[i]["noTimeSheet"].ObjToString();

            double biWeekly = empDt.Rows[i]["biWeekly"].ObjToDouble();

            vacationpay = empDt.Rows[i]["vacationpay"].ObjToDouble();
            holidaypay = empDt.Rows[i]["holidaypay"].ObjToDouble();
            sickpay = empDt.Rows[i]["sickpay"].ObjToDouble();
            sickTaken = empDt.Rows[i]["approvedSick"].ObjToDouble();

            contractPay = empDt.Rows[i]["contractPay"].ObjToDouble();
            otherPay = empDt.Rows[i]["otherPay"].ObjToDouble();

            otrate = rate * 1.5D;
            otrate = G1.RoundValue(otrate);

            hours = empDt.Rows[i]["hours"].ObjToDouble();
            week1 = empDt.Rows[i]["week1hours"].ObjToDouble();
            cweek1 = empDt.Rows[i]["cweek1hours"].ObjToDouble();
            week2 = empDt.Rows[i]["week2hours"].ObjToDouble();
            cweek2 = empDt.Rows[i]["cweek2hours"].ObjToDouble();
            week1vhours = empDt.Rows[i]["week1vhours"].ObjToDouble();
            week2vhours = empDt.Rows[i]["week2vhours"].ObjToDouble();
            week1hhours = empDt.Rows[i]["week1hhours"].ObjToDouble();
            week2hhours = empDt.Rows[i]["week2hhours"].ObjToDouble();
            week1shours = empDt.Rows[i]["week1shours"].ObjToDouble();
            week2shours = empDt.Rows[i]["week2shours"].ObjToDouble();

            availableSick = empDt.Rows[i]["availableSick"].ObjToDouble();
            sickhours = empDt.Rows[i]["sickHours"].ObjToDouble();

            double week1Days = empDt.Rows[i]["week1Days"].ObjToDouble();
            double week2Days = empDt.Rows[i]["week2Days"].ObjToDouble();
            double percent = 0D;


            double week1OtHours = 0D;
            double week2OtHours = 0D;

            if (flux == "Y")
            {
                hours = week1 + cweek1 - week1vhours - week1shours - week1hhours;
                basePay = rate * 40D;
                if (hours > 40D)
                {
                    fRate = basePay / hours / 2D;
                    fRate = G1.RoundValue(fRate);
                    othours = hours - 40D;
                    if (othours > 0)
                        week1OtHours = othours;
                    otpay += fRate * (hours - 40D);
                    otpay = G1.RoundValue(otpay);
                    if (otpay < 0D)
                        otpay = 0D;
                    pay = basePay;
                    pay = basePay - (week1vhours * rate) - (week1shours * rate) - (week1hhours * rate);
                }
                else
                {
                    pay = basePay;
                    pay = basePay - (week1vhours * rate) - (week1shours * rate) - (week1hhours * rate);
                }
                hours = week2 + cweek2 - week2vhours - week2shours - week2hhours;
                if (hours > 40D)
                {
                    fRate = basePay / hours / 2D;
                    fRate = G1.RoundValue(fRate);
                    otpay += fRate * (hours - 40D);
                    othours = hours - 40D;
                    if (othours > 0)
                        week2OtHours = othours;
                    otpay = G1.RoundValue(otpay);
                    if (otpay < 0D)
                        otpay = 0D;
                    pay += basePay - (week2vhours * rate) - (week2shours * rate) - (week2hhours * rate);
                    //pay += basePay - (week2shours * rate) - (week2hhours * rate);
                    //pay += basePay;
                }
                else
                {
                    pay += basePay - (week2vhours * rate) - (week2shours * rate) - (week2hhours * rate);
                    //pay += basePay - (week2shours * rate) - (week2hhours * rate);
                    //pay += basePay;
                }
                //totalPay = pay + otpay + contractPay + otherPay + vacationpay + holidaypay + sickpay;
                totalPay = pay + otpay + contractPay + otherPay + vacationpay + holidaypay + sickpay;
                if ( proRating )
                {
                }
                if (empType == "NON-EXEMPT" && empStatus == "FULLTIME")
                {
                    if ((week1 + week2) <= 80D)
                        otpay = 0D;
                    totalPay = biWeekly + otpay + contractPay + otherPay;
                    pay = biWeekly;
                }
                else if (salaried == "Y")
                {
                    pay = biWeekly;
                    if ( week1 == 0D || week2 == 0D )
                    {
                        if (week1 == 0D && week2 != 0D)
                            biWeekly = biWeekly * 0.5D;
                        else if (week1 != 0D && week2 == 0D)
                            biWeekly = biWeekly * 0.5D;
                        pay = biWeekly;
                    }
                    totalPay = biWeekly + otpay + contractPay + otherPay;
                }
                totalPay = G1.RoundValue(totalPay);
                empDt.Rows[i]["totalPay"] = totalPay;
            }
            else if (empType == "NON-EXEMPT") // Salaried Non-Exempt - Same as Flux
            {
                hours = week1 + cweek1 - week1vhours - week1shours - week1hhours;
                basePay = rate * 40D;
                //if (isBiWeekly.ToUpper() == "Y")
                //    basePay = rate * 20D;
                if (hours > 40D)
                {
                    fRate = basePay / hours / 2D;
                    fRate = G1.RoundValue(fRate);
                    othours = hours - 40D - week1vhours - week1shours - week1hhours;
                    otpay += fRate * (hours - 40D);
                    otpay = G1.RoundValue(otpay);
                    if (otpay < 0D)
                        otpay = 0D;
                    pay = basePay;
                    pay = basePay - (week1vhours * rate) - (week1shours * rate) - (week1hhours * rate);
                }
                else
                {
                    pay = basePay;
                    pay = basePay - (week1vhours * rate) - (week1shours * rate) - (week1hhours * rate);
                }
                hours = week2 + cweek2 - week2vhours - week2shours - week2hhours;
                if (hours > 40D)
                {
                    fRate = basePay / hours / 2D;
                    fRate = G1.RoundValue(fRate);
                    otpay += fRate * (hours - 40D);
                    othours = hours - 40D;
                    otpay = G1.RoundValue(otpay);
                    if (otpay < 0D)
                        otpay = 0D;
                    pay += basePay - (week2vhours * rate) - (week2shours * rate) - (week2hhours * rate);
                }
                else
                {
                    pay += basePay - (week2vhours * rate) - (week2shours * rate) - (week2hhours * rate);
                }
                totalPay = pay + otpay + contractPay + otherPay + vacationpay + holidaypay + sickpay;
                totalPay = G1.RoundValue(totalPay);
                empDt.Rows[i]["totalPay"] = totalPay;
            }
            else if (empStatus == "PARTTIME" && empType == "EXEMPT")
            {
                if (proRating)
                {
                }
                basePay = biWeekly;
                basePay = G1.RoundValue(basePay);
                empDt.Rows[i]["totalPay"] = basePay;
                pay = basePay - vacationpay;
            }
            else // Salaried Exempt - Straight Pay
            {
                if (proRating)
                {
                }
                hours = week1 - week1vhours - week1shours - week1hhours;
                basePay = rate * 40D;
                if (salaried == "Y")
                    basePay = biWeekly;
                if (hours > 40D)
                    pay = basePay;
                else
                    pay = basePay;

                hours = week2 - week2vhours - week2shours - week2hhours;
                basePay = rate * 40D;
                if (salaried == "Y")
                    basePay = biWeekly;
                if (hours > 40D)
                    pay += basePay;
                else
                    pay += basePay;
                if (salaried == "Y")
                {
                    pay = biWeekly;
                    if ( proRating )
                    {
                        pay = 0D;
                        percent = week1Days / 10D;
                        if (week1 > 0D)
                            pay = biWeekly * percent;
                        percent = week2Days / 10D;
                        if (week2 > 0D)
                            pay += biWeekly * percent;
                        pay = G1.RoundValue ( pay );
                    }
                    pay = checkHireTermDates(empDt, i, pay);
                    empDt.Rows[i]["sickPay"] = 0D;
                    if ( sickhours > 0D )
                    {
                        double diff = availableSick - (sickhours + sickTaken);
                        if ( diff < 0 )
                        {
                            pay = pay - (Math.Abs(diff) * rate);
                            if (pay < 0D)
                                pay = 0D;
                        }
                    }
                    if (week1 == 0 && week2 == 0 && noTimeSheet != "Y" )
                        pay = 0D;
                }
                totalPay = pay;
                totalPay = G1.RoundValue(totalPay);
                empDt.Rows[i]["totalPay"] = totalPay + otherPay;
            }

            empDt.Rows[i]["othours"] = othours;
            empDt.Rows[i]["otpay"] = otpay;
            empDt.Rows[i]["pay"] = pay;
        }
        /***********************************************************************************************/
        private double checkHireTermDates ( DataTable empDt, int i, double pay )
        {
            DateTime hireDate = empDt.Rows[i]["hireDate"].ObjToDateTime();
            DateTime termDate = empDt.Rows[i]["termDate"].ObjToDateTime();

            DateTime firstDate = this.dateTimePicker1.Value;
            DateTime lastDate = this.dateTimePicker2.Value;

            string username = empDt.Rows[i]["username"].ObjToString();

            if ( hireDate >= firstDate && hireDate <= lastDate )
            {
                double workDays = determineWorkDays(hireDate, lastDate);
                double periodDays = determineWorkDays(firstDate, lastDate);
                periodDays = periodDays - 1;
                double percent = workDays / periodDays;
                pay = pay * percent;
                pay = G1.RoundValue(pay);
            }
            if ( termDate.Year > 1900 )
            {
                if ( termDate >= firstDate && termDate <= lastDate )
                {
                    double workDays = determineWorkDays(firstDate, termDate);
                    double periodDays = determineWorkDays(firstDate, lastDate);
                    periodDays = periodDays - 1;
                    double percent = workDays / periodDays;
                    pay = pay * percent;
                    pay = G1.RoundValue(pay);
                }
            }

            return pay;
        }
        /***********************************************************************************************/
        private double determineWorkDays ( DateTime date1, DateTime date2 )
        {
            double days = 0D;
            for (; ; )
            {
                if (date1.DayOfWeek != DayOfWeek.Saturday && date1.DayOfWeek != DayOfWeek.Sunday)
                    days = days + 1D;
                date1 = date1.AddDays(1);
                if (date1 > date2)
                    break;
            }
            return days;
        }
        /***********************************************************************************************/
        private void CalculateFullTimeNonExempt (DataTable empDt, int i)
        {
            double rate = 0D;
            double otrate = 0D;
            double hours = 0D;
            double pay = 0D;
            double newHours = 0D;
            double otpay = 0D;
            double total = 0D;
            double contractHours = 0D;
            double contractPay = 0D;
            double otherPay = 0D;
            double totalPay = 0D;
            double vacationhours = 0D;
            double vacationpay = 0D;
            double holidayhours = 0D;
            double holidaypay = 0D;
            double sickhours = 0D;
            double sickpay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double cweek1 = 0D;
            double cweek2 = 0D;
            double week1vhours = 0D;
            double week2vhours = 0D;
            double week1hhours = 0D;
            double week2hhours = 0D;
            double week1shours = 0D;
            double week2shours = 0D;

            double week1hours = 0D;
            double week2hours = 0D;
            double week1othours = 0D;
            double week2othours = 0D;
            double othours = 0D;

            double basePay = 0D;
            double fRate = 0D;

            string empStatus = "";
            string empType = "";
            string salaried = "";
            string isBiWeekly = "";
            string flux = "";
            string username = "";

            othours = 0D;
            otpay = 0D;
            pay = 0D;

            username = empDt.Rows[i]["username"].ObjToString().ToUpper();
            rate = empDt.Rows[i]["rate"].ObjToDouble();
            empStatus = empDt.Rows[i]["EmpStatus"].ObjToString().ToUpper(); // FullTime or PartTime or Both
            empType = empDt.Rows[i]["EmpType"].ObjToString().ToUpper(); // exempt or Non-Exempt
            salaried = empDt.Rows[i]["salaried"].ObjToString().ToUpper();
            isBiWeekly = empDt.Rows[i]["isBiWeekly"].ObjToString().ToUpper();
            flux = empDt.Rows[i]["flux"].ObjToString().ToUpper();

            double biWeekly = empDt.Rows[i]["biWeekly"].ObjToDouble();

            double fullTimeHours = empDt.Rows[i]["fullTimeHours"].ObjToDouble();

            vacationpay = empDt.Rows[i]["vacationpay"].ObjToDouble();
            holidaypay = empDt.Rows[i]["holidaypay"].ObjToDouble();
            sickpay = empDt.Rows[i]["sickpay"].ObjToDouble();

            if ( RunningSingle )
            {
            }

            contractPay = empDt.Rows[i]["contractPay"].ObjToDouble();
            otherPay = empDt.Rows[i]["otherPay"].ObjToDouble();

            otrate = rate * 1.5D;
            otrate = G1.RoundValue(otrate);

            hours = empDt.Rows[i]["hours"].ObjToDouble();
            week1 = empDt.Rows[i]["week1hours"].ObjToDouble();
            cweek1 = empDt.Rows[i]["cweek1hours"].ObjToDouble();
            week2 = empDt.Rows[i]["week2hours"].ObjToDouble();
            cweek2 = empDt.Rows[i]["cweek2hours"].ObjToDouble();
            week1vhours = empDt.Rows[i]["week1vhours"].ObjToDouble();
            week2vhours = empDt.Rows[i]["week2vhours"].ObjToDouble();
            week1hhours = empDt.Rows[i]["week1hhours"].ObjToDouble();
            week2hhours = empDt.Rows[i]["week2hhours"].ObjToDouble();
            week1shours = empDt.Rows[i]["week1shours"].ObjToDouble();
            week2shours = empDt.Rows[i]["week2shours"].ObjToDouble();

            //hours = week1 + cweek1 - week1vhours - week1shours - week1hhours;
            hours = week1 + cweek1;
            if (hours > 40D)
            {
                othours = hours - 40D;
                if (empType == "EXEMPT" && empStatus == "FULLTIME")
                    othours = 0D;
                otpay += otrate * othours;
                otpay = G1.RoundValue(otpay);
                if (otpay < 0D)
                    otpay = 0D;
                pay = 40D * rate;
            }
            else
                pay = hours * rate;

            //hours = week2 + cweek2 - week2vhours - week2shours - week2hhours;
            hours = week2 + cweek2;
            if (hours > 40D)
            {
                othours = hours - 40D;
                if (empType == "EXEMPT" && empStatus == "FULLTIME")
                    othours = 0D;
                otpay += otrate * othours;
                otpay = G1.RoundValue(otpay);
                if (otpay < 0D)
                    otpay = 0D;
                pay += 40D * rate;
            }
            else
            {
                pay += hours * rate;
            }

            totalPay = pay + otpay + contractPay + otherPay + vacationpay + holidaypay + sickpay;
            empDt.Rows[i]["totalPay"] = totalPay;

            empDt.Rows[i]["othours"] = othours;
            empDt.Rows[i]["otpay"] = otpay;
            empDt.Rows[i]["pay"] = pay;
        }
        /***********************************************************************************************/
        private void CalculateFullTimeExempt(DataTable empDt, int i)
        {
            double rate = 0D;
            double otrate = 0D;
            double hours = 0D;
            double pay = 0D;
            double newHours = 0D;
            double otpay = 0D;
            double total = 0D;
            double contractHours = 0D;
            double contractPay = 0D;
            double otherPay = 0D;
            double totalPay = 0D;
            double vacationhours = 0D;
            double vacationpay = 0D;
            double holidayhours = 0D;
            double holidaypay = 0D;
            double sickhours = 0D;
            double sickpay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double cweek1 = 0D;
            double cweek2 = 0D;
            double week1vhours = 0D;
            double week2vhours = 0D;
            double week1hhours = 0D;
            double week2hhours = 0D;
            double week1shours = 0D;
            double week2shours = 0D;

            double week1hours = 0D;
            double week2hours = 0D;
            double week1othours = 0D;
            double week2othours = 0D;
            double othours = 0D;

            double basePay = 0D;
            double fRate = 0D;

            string empStatus = "";
            string empType = "";
            string salaried = "";
            string isBiWeekly = "";
            string flux = "";
            string username = "";

            othours = 0D;
            otpay = 0D;
            pay = 0D;

            username = empDt.Rows[i]["username"].ObjToString();
            rate = empDt.Rows[i]["rate"].ObjToDouble();
            empStatus = empDt.Rows[i]["EmpStatus"].ObjToString().ToUpper(); // FullTime or PartTime or Both
            empType = empDt.Rows[i]["EmpType"].ObjToString().ToUpper(); // exempt or Non-Exempt
            salaried = empDt.Rows[i]["salaried"].ObjToString().ToUpper();
            isBiWeekly = empDt.Rows[i]["isBiWeekly"].ObjToString().ToUpper();
            flux = empDt.Rows[i]["flux"].ObjToString().ToUpper();

            double biWeekly = empDt.Rows[i]["biWeekly"].ObjToDouble();

            vacationpay = empDt.Rows[i]["vacationpay"].ObjToDouble();
            holidaypay = empDt.Rows[i]["holidaypay"].ObjToDouble();
            sickpay = empDt.Rows[i]["sickpay"].ObjToDouble();

            contractPay = empDt.Rows[i]["contractPay"].ObjToDouble();
            otherPay = empDt.Rows[i]["otherPay"].ObjToDouble();

            otrate = rate * 1.5D;
            otrate = G1.RoundValue(otrate);

            hours = empDt.Rows[i]["hours"].ObjToDouble();
            week1 = empDt.Rows[i]["week1hours"].ObjToDouble();
            cweek1 = empDt.Rows[i]["cweek1hours"].ObjToDouble();
            week2 = empDt.Rows[i]["week2hours"].ObjToDouble();
            cweek2 = empDt.Rows[i]["cweek2hours"].ObjToDouble();
            week1vhours = empDt.Rows[i]["week1vhours"].ObjToDouble();
            week2vhours = empDt.Rows[i]["week2vhours"].ObjToDouble();
            week1hhours = empDt.Rows[i]["week1hhours"].ObjToDouble();
            week2hhours = empDt.Rows[i]["week2hhours"].ObjToDouble();
            week1shours = empDt.Rows[i]["week1shours"].ObjToDouble();
            week2shours = empDt.Rows[i]["week2shours"].ObjToDouble();

            hours = week1 + cweek1 - week1vhours - week1shours - week1hhours;
            basePay = rate * 40D;
            if (salaried == "Y")
                basePay = biWeekly;
            if (hours > 40D)
                pay = basePay;
            else
            {
                if (biWeekly <= 0D)
                    pay = hours * rate;
                else
                    pay = basePay;
            }

            hours = week2 + cweek2 - week2vhours - week2shours - week2hhours;
            basePay = rate * 40D;
            if (hours > 40D)
                pay += basePay;
            else
            {
                if (biWeekly <= 0D)
                    pay += hours * rate;
                else
                    pay += basePay;
            }
            totalPay = pay;

            if (salaried == "Y")
            {
                if (flux == "Y")
                    totalPay = biWeekly + otpay;
                else
                    totalPay = biWeekly;
            }

            if ( empType.ToUpper() == "EXEMPT" && empStatus.ToUpper() == "FULLTIME")
            {
                othours = 0D;
                otpay = 0D;
            }    

            empDt.Rows[i]["totalPay"] = totalPay + otherPay;

            empDt.Rows[i]["othours"] = othours;
            empDt.Rows[i]["otpay"] = otpay;
            empDt.Rows[i]["pay"] = pay;
        }
        /***********************************************************************************************/
        private void CalculatePartTime(DataTable empDt, int i, bool proRating )
        {
            double rate = 0D;
            double otrate = 0D;
            double hours = 0D;
            double pay = 0D;
            double newHours = 0D;
            double otpay = 0D;
            double total = 0D;
            double contractHours = 0D;
            double contractPay = 0D;
            double otherPay = 0D;
            double totalPay = 0D;
            double vacationhours = 0D;
            double vacationpay = 0D;
            double holidayhours = 0D;
            double holidaypay = 0D;
            double sickhours = 0D;
            double sickpay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double cweek1 = 0D;
            double cweek2 = 0D;
            double week1vhours = 0D;
            double week2vhours = 0D;
            double week1hhours = 0D;
            double week2hhours = 0D;
            double week1shours = 0D;
            double week2shours = 0D;

            double week1hours = 0D;
            double week2hours = 0D;
            double week1othours = 0D;
            double week2othours = 0D;
            double othours = 0D;

            double basePay = 0D;
            double fRate = 0D;

            string empStatus = "";
            string empType = "";
            string salaried = "";
            string isBiWeekly = "";
            string flux = "";
            string username = "";

            othours = 0D;
            otpay = 0D;
            pay = 0D;

            username = empDt.Rows[i]["username"].ObjToString();
            rate = empDt.Rows[i]["rate"].ObjToDouble();
            empStatus = empDt.Rows[i]["EmpStatus"].ObjToString().ToUpper(); // FullTime or PartTime or Both
            empType = empDt.Rows[i]["EmpType"].ObjToString().ToUpper(); // exempt or Non-Exempt
            salaried = empDt.Rows[i]["salaried"].ObjToString().ToUpper();
            isBiWeekly = empDt.Rows[i]["isBiWeekly"].ObjToString().ToUpper();
            flux = empDt.Rows[i]["flux"].ObjToString().ToUpper();

            vacationpay = empDt.Rows[i]["vacationpay"].ObjToDouble();
            holidaypay = empDt.Rows[i]["holidaypay"].ObjToDouble();
            sickpay = empDt.Rows[i]["sickpay"].ObjToDouble();

            contractPay = empDt.Rows[i]["contractPay"].ObjToDouble();
            otherPay = empDt.Rows[i]["otherPay"].ObjToDouble();

            otrate = rate * 1.5D;
            otrate = G1.RoundValue(otrate);

            hours = empDt.Rows[i]["hours"].ObjToDouble();
            week1 = empDt.Rows[i]["week1hours"].ObjToDouble();
            cweek1 = empDt.Rows[i]["cweek1hours"].ObjToDouble();
            week2 = empDt.Rows[i]["week2hours"].ObjToDouble();
            cweek2 = empDt.Rows[i]["cweek2hours"].ObjToDouble();
            week1vhours = empDt.Rows[i]["week1vhours"].ObjToDouble();
            week2vhours = empDt.Rows[i]["week2vhours"].ObjToDouble();
            week1hhours = empDt.Rows[i]["week1hhours"].ObjToDouble();
            week2hhours = empDt.Rows[i]["week2hhours"].ObjToDouble();
            week1shours = empDt.Rows[i]["week1shours"].ObjToDouble();
            week2shours = empDt.Rows[i]["week2shours"].ObjToDouble();

            if ( proRating )
            {
            }

            if (empType.ToUpper() != "NON-EXEMPT")
            {
                hours = week1 + cweek1 - week1vhours - week1shours - week1hhours;
                basePay = rate * 40D;
                if (hours > 40D)
                    pay = basePay;
                else
                    pay = basePay;

                hours = week2 + cweek2 - week2vhours - week2shours - week2hhours;
                basePay = rate * 40D;
                if (hours > 40D)
                    pay += basePay;
                else
                    pay += basePay;
                totalPay = pay;

                empDt.Rows[i]["totalPay"] = contractPay + otherPay;
                empDt.Rows[i]["pay"] = contractPay + otherPay;
            }
            else
            {
                hours = cweek1 - week1vhours - week1shours - week1hhours;
                if (hours > 40D)
                {
                    othours = hours - 40D;
                    otpay += otrate * othours;
                    otpay = G1.RoundValue(otpay);
                    if (otpay < 0D)
                        otpay = 0D;
                    pay = 40D * rate;
                }
                else
                    pay = hours * rate;

                hours = cweek2 - week2vhours - week2shours - week2hhours;
                if (hours > 40D)
                {
                    othours = hours - 40D;
                    otpay += otrate * othours;
                    otpay = G1.RoundValue(otpay);
                    if (otpay < 0D)
                        otpay = 0D;
                    pay += 40D * rate;
                }
                else
                {
                    pay += hours * rate;
                }

                totalPay = pay + otpay + contractPay + otherPay;
                totalPay = pay + otpay + otherPay;
                empDt.Rows[i]["totalPay"] = totalPay;

                empDt.Rows[i]["othours"] = othours;
                empDt.Rows[i]["otpay"] = otpay;
                empDt.Rows[i]["contractPay"] = pay;
                empDt.Rows[i]["pay"] = 0D;
            }


            //empDt.Rows[i]["totalPay"] = totalPay;

            //empDt.Rows[i]["othours"] = othours;
            //empDt.Rows[i]["otpay"] = otpay;
            //empDt.Rows[i]["pay"] = pay;
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            btnSaveData.Hide();
            btnSaveData.Refresh();

            btnLoad.BackColor = Color.Khaki;
            btnLoad.ForeColor = Color.Black;

            DateTime startDate = dateTimePicker1.Value;
            startDate = startDate.AddDays(-14);

            DateTime stopDate = dateTimePicker2.Value;
            stopDate = stopDate.AddDays(-14);

            this.dateTimePicker1.Value = startDate;
            this.dateTimePicker2.Value = stopDate;

            GetAllEmployees();

            string super = cmbSuper.Text.Trim();
            string location = cmbLocation.Text.Trim();
            if (super != "All")
                cmbSuper_SelectedIndexChanged(null, null);
            else if (location != "All")
                cmbLocation_SelectedIndexChanged(null, null);
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            btnSaveData.Hide();
            btnSaveData.Refresh();

            btnLoad.BackColor = Color.Khaki;
            btnLoad.ForeColor = Color.Black;

            DateTime startDate = dateTimePicker1.Value;
            startDate = startDate.AddDays(14);

            DateTime stopDate = dateTimePicker2.Value;
            stopDate = stopDate.AddDays(14);

            this.dateTimePicker1.Value = startDate;
            this.dateTimePicker2.Value = stopDate;

            GetAllEmployees();

            string super = cmbSuper.Text.Trim();
            string location = cmbLocation.Text.Trim();
            if (super != "All")
                cmbSuper_SelectedIndexChanged(null, null);
            else if (location != "All")
                cmbLocation_SelectedIndexChanged(null, null);
        }
        /***********************************************************************************************/
        private int saveRow = -1;
        private int saveTopRow = -1;
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();

            saveRow = gridMain2.FocusedRowHandle;
            saveTopRow = gridMain2.TopRowIndex;

            DataTable dt = (DataTable)dgv2.DataSource;

            string empno = dr["record"].ObjToString();
            string name = dr["firstName"].ObjToString() + " " + dr["lastName"].ObjToString();
            string userName = dr["userName"].ObjToString();
            string oldRecord = dr["record1"].ObjToString();
            oldRecord = "";

            DateTime date = this.dateTimePicker1.Value;
            string sDate = date.ToString("yyyyMMdd");
            date = this.dateTimePicker2.Value;
            string eDate = date.ToString("yyyyMMdd");
            string cmd = "Select * from `tc_pay` WHERE `username` = '" + userName + "' AND `startdate` = '" + sDate + "' AND `enddate` <= '" + eDate + "';";
            DataTable tcDt = G1.get_db_data(cmd);
            if (tcDt.Rows.Count > 0)
                oldRecord = tcDt.Rows[0]["record"].ObjToString();


            this.Cursor = Cursors.WaitCursor;
            DateTime startTime = this.dateTimePicker1.Value;
            DateTime stopTime = this.dateTimePicker2.Value;

            using (TimeClock timeForm = new TimeClock(startTime, stopTime, empno, userName, name, false, oldRecord, dr ))
            {
                timeForm.TimeClockDone += TimeForm_TimeClockDone;
                timeForm.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void TimeForm_TimeClockDone(string workEmpNo, string workUserName )
        {
            DataTable empDt = (DataTable)dgv2.DataSource;

            DateTime timePeriod = dateTimePicker1.Value;
            long ldate = G1.TimeToUnix(timePeriod);

            timePeriod = dateTimePicker2.Value;
            //timePeriod = timePeriod.AddMinutes(-1); // This gets the time back to 23:59:00
            long edate = G1.TimeToUnix(timePeriod);

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

            string id = "";
            string punchType = "";
            DataRow[] dRows = null;
            DateTime midDate = firstDate.AddDays(7);

            dRows = empDt.Select("record='" + workEmpNo + "'");
            dRows[0]["totalHours"] = 0D;
            dRows[0]["hours"] = 0D;
            dRows[0]["contractHours"] = 0D;
            dRows[0]["pay"] = 0D;
            dRows[0]["contractPay"] = 0D;
            dRows[0]["otherPay"] = 0D;
            dRows[0]["totalPay"] = 0D;
            dRows[0]["week1hours"] = 0D;
            dRows[0]["week2hours"] = 0D;
            dRows[0]["vacationhours"] = 0D;
            dRows[0]["vacationpay"] = 0D;
            dRows[0]["holidayhours"] = 0D;
            dRows[0]["holidaypay"] = 0D;
            dRows[0]["sickhours"] = 0D;
            dRows[0]["sickpay"] = 0D;

            string cmd = "Select * from `tc_punches_pchs` where `date` >= '" + firstDate.ToString("yyyyMMdd") + "' and `date` <= '" + lastDate.ToString("yyyyMMdd") + "' ";
            cmd += " AND `empy!AccountingID` = '" + workEmpNo + "' ";
            cmd += "order by `empy!AccountingID`,`date`;";
            DataTable dx = G1.get_db_data(cmd);

            dRows = empDt.Select("record='" + workEmpNo + "'");
            if ( dRows.Length > 0 )
            {
                dRows[0]["cweek1hours"] = 0D;
                dRows[0]["cweek2hours"] = 0D;
            }

            DateTime newDate = DateTime.Now;
            string str = lastDate.ToString("MM/dd/yyyy");
            lastDate = str.ObjToDateTime();

            TimeSpan ts;

            DateTime cutoffDate = DateTime.Now;

            int i = gridMain2.GetFocusedDataSourceRowIndex();

            CleanupRow(empDt, i);

            bool gotChanges = false;
            dx = LoadPayrollDetails(dx, workEmpNo, ref gotChanges ); // TimeClock Done

            if (gotChanges)
            {
            }
            else
            {
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    CalculateEmployeeDetail(empDt, dx, j);
                }

                CalcOvertime(empDt, i );
            }

            cmd = "Select * from `tc_approvals` where `startdate` >= '" + firstDate.ToString("yyyyMMdd") + "' and `enddate` <= '" + lastDate.ToString("yyyyMMdd") + "' ";
            cmd += " AND `username` = '" + workUserName + "' ";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                //if (dRows.Length > 0)
                //{
                //    dRows[0]["employeeApproved"] = dx.Rows[0]["employeeApproved"].ObjToString();
                //    dRows[0]["managerApproved"] = dx.Rows[0]["managerApproved"].ObjToString();
                //}
            }
            
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
            dgv2.Refresh();

            RecalcAllBenefits();

            if ( saveRow >= 0 )
            {
                gridMain2.FocusedRowHandle = saveRow;
                saveRow = -1;
                gridMain2.RefreshData();
                gridMain2.RefreshEditor(true);
            }
        }
        /***********************************************************************************************/
        private DataTable LoadPayrollDetails ( DataTable dx, string userId, ref bool gotChanges )
        {
            gotChanges = false; 

            string cmd = "Select * from `tc_rates` WHERE `userId` = '" + userId + "' ORDER BY `effectiveDate`;";
            DataTable dt = G1.get_db_data(cmd);

            DateTime effectiveDate = DateTime.Now;
            DateTime date = DateTime.Now;

            if (G1.get_column_number(dx, "EmpStatus") < 0)
                dx.Columns.Add("EmpStatus");
            if (G1.get_column_number(dx, "EmpType") < 0)
                dx.Columns.Add("EmpType");
            if (G1.get_column_number(dx, "rate2") < 0)
                dx.Columns.Add("rate2", Type.GetType("System.Double"));
            if (G1.get_column_number(dx, "biWeekly") < 0)
                dx.Columns.Add("biWeekly", Type.GetType("System.Double"));
            if (G1.get_column_number(dx, "salary") < 0)
                dx.Columns.Add("salary", Type.GetType("System.Double"));
            if (G1.get_column_number(dx, "salaried") < 0)
                dx.Columns.Add("salaried");
            if (G1.get_column_number(dx, "flux") < 0)
                dx.Columns.Add("flux");
            if (G1.get_column_number(dx, "day") < 0)
                dx.Columns.Add("day", Type.GetType("System.Double"));
            if (G1.get_column_number(dx, "week1Days") < 0)
                dx.Columns.Add("week1Days", Type.GetType("System.Double"));
            if (G1.get_column_number(dx, "week2Days") < 0)
                dx.Columns.Add("week2Days", Type.GetType("System.Double"));

            if (dt.Rows.Count <= 0)
                return dx;


            DateTime timePeriod1 = dateTimePicker1.Value;
            long ldate = G1.TimeToUnix(timePeriod1);

            DateTime timePeriod = dateTimePicker2.Value;
            long edate = G1.TimeToUnix(timePeriod);

            DateTime firstDate = ldate.UnixToDateTime().ToLocalTime();
            DateTime lastDate = edate.UnixToDateTime().ToLocalTime();

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;
            DateTime midDate = firstDate.AddDays(7);

            DateTime startDate = DateTime.MaxValue;
            DateTime stopDate = DateTime.MinValue;

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                date = dx.Rows[i]["date"].ObjToDateTime();
                if (date > stopDate)
                    stopDate = date;
                if (date < startDate)
                    startDate = date;
            }

            DateTime oldDate = DateTime.MinValue;
            string punchType = "";
            double dValue = 0D;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                punchType = dx.Rows[i]["punchType"].ObjToString().ToUpper();
                date = dx.Rows[i]["date"].ObjToDateTime();
                if (date != oldDate && punchType == "NONE")
                {
                    dx.Rows[i]["day"] = 1D;
                    if ( date <= midDate )
                    {
                        dValue = dx.Rows[i]["week1Days"].ObjToDouble();
                        dx.Rows[i]["week1Days"] = 1D;
                    }
                    else
                    {
                        dValue = dx.Rows[i]["week2Days"].ObjToDouble();
                        dx.Rows[i]["week2Days"] = 1D;
                    }
                }
                oldDate = date;
                for ( int j=0; j<dt.Rows.Count; j++)
                {
                    effectiveDate = dt.Rows[j]["effectiveDate"].ObjToDateTime();
                    if (effectiveDate >= startDate && date <= stopDate)
                    {
                        if (date >= effectiveDate)
                        {
                            dx.Rows[i]["EmpStatus"] = dt.Rows[j]["EmpStatus"].ObjToString();
                            dx.Rows[i]["EmpType"] = dt.Rows[j]["EmpType"].ObjToString();
                            dx.Rows[i]["salaried"] = dt.Rows[j]["salaried"].ObjToString();
                            dx.Rows[i]["flux"] = dt.Rows[j]["flux"].ObjToString();
                            dx.Rows[i]["rate2"] = dt.Rows[j]["rate"].ObjToDouble();
                            dx.Rows[i]["biWeekly"] = dt.Rows[j]["biWeekly"].ObjToDouble();
                            dx.Rows[i]["salary"] = dt.Rows[j]["salary"].ObjToDouble();
                            gotChanges = true;
                        }
                    }
                }
            }

            return dx;
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Text.Trim().ToUpper() == "EMPLOYEE TIMES")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                if ( editToolStripMenuItem.Visible == true )
                    menuStrip1.Items.Remove(editToolStripMenuItem);
                gridMain2.RefreshEditor(true);
                gridMain2.RefreshData();
                dgv2.Refresh();
            }
            else
            {
                if ( G1.isHR() || G1.isAdmin() )
                    menuStrip1.Items.Add (editToolStripMenuItem);
                gridMain.RefreshEditor(true);
                gridMain.RefreshData();
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void cmbSuper_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            loading = true;
            cmbLocation.Text = "All";
            cmbLocation.Refresh();
            loading = false;


            string super = cmbSuper.Text.Trim();
            DataTable dt = (DataTable)dgv.DataSource;
            if (super.ToUpper() == "ALL")
            {
                DataTable empDt = mainDt.Copy();
                G1.NumberDataTable(empDt);
                dgv2.DataSource = empDt;
                dgv.DataSource = empDt;
            }
            DataRow[] dRows = mainDt.Select("TimeKeeper='" + super + "'");
            if (dRows.Length > 0)
            {
                DataTable empDt = dRows.CopyToDataTable();
                G1.NumberDataTable(empDt);
                dgv2.DataSource = empDt;
                dgv.DataSource = empDt;
            }
        }
        /***********************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            EmployeeDemo employeeForm = new EmployeeDemo();
            employeeForm.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;

            GetAllEmployees();

            gridMain.FocusedRowHandle = rowHandle;
            gridMain.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string name = lastName + ", " + firstName;
            string username = dr["userName"].ObjToString();
            if (String.IsNullOrWhiteSpace(username))
                return;

            string record = "";

            DialogResult result = MessageBox.Show("*** CONFIRM *** Are you SURE you want to DELETE\nEmployee (" + name + " ) ?", "Delete Employee Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;

            string cmd = "Select * from `tc_er` WHERE `username` = '" + username + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("tc_er", "record", record);
            }

            cmd = "Select * from `users` WHERE `username` = '" + username + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("users", "record", record);
            }

            dx = (DataTable)dgv.DataSource;
            dx.Rows.Remove(dr);
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;

            int[] rows = gridMain2.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            if (rows.Length > 0)
                lastRow = rows.Length;

            string empno = "";
            string name = "";
            string userName = "";
            int row = 0;
            DataRow dr = null;

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

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();

                row = rows[i];
                row = gridMain2.GetDataSourceRowIndex(row);

                dr = dt.Rows[row];

                empno = dr["record"].ObjToString();
                name = dr["firstName"].ObjToString() + " " + dr["lastName"].ObjToString();
                userName = dr["userName"].ObjToString();

                DateTime startTime = this.dateTimePicker1.Value;
                DateTime stopTime = this.dateTimePicker2.Value;


                using (TimeClock timeForm = new TimeClock(startTime, stopTime, empno, userName, name, true))
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
                        MergeAllPDF(pdfCopyProvider, timeFile, contractFile, otherFile );
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
        private static void MergeAllPDF(PdfCopy pdfCopyProvider, string File1, string File2, string File3 )
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
        private void printEmployeeDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        //private bool showRates = false;
        private void dgv_ProcessGridKey(object sender, KeyEventArgs e)
        {
            SetControlKey(sender, e);
        }
        /***********************************************************************************************/
        private void SetControlKey (object sender, KeyEventArgs e)
        {
            if (loading)
                return;
            if (e.Control && e.KeyCode == Keys.R)
            {
                loading = true;
                if (showRates)
                    showRates = false;
                else
                    showRates = true;
                if (dgv2.Visible)
                {
                    gridMain2.FocusedColumn = gridMain2.Columns["empStatus"];
                    gridMain2.RefreshData();
                    gridMain2.FocusedColumn = gridMain2.Columns["empStatus"];
                    gridMain2.RefreshData();
                    //if (!showRates)
                    //    gridMain2.OptionsView.ShowFooter = false;
                    //else
                    //    gridMain2.OptionsView.ShowFooter = true;
                    gridMain2.FocusedColumn = gridMain2.Columns["empStatus"];
                    gridMain2.RefreshData();
                }
                else
                {
                    gridMain.RefreshData();
                    dgv.Refresh();
                    int rowHandle = gridMain.FocusedRowHandle;
                    gridMain.SelectRow(rowHandle);
                    gridMain.FocusedRowHandle = rowHandle;
                    gridMain.FocusedColumn = gridMain.Columns["lastName"];
                }
                loading = false;
            }
        }
        /***********************************************************************************************/
        private void dgv2_ProcessGridKey(object sender, KeyEventArgs e)
        {
            SetControlKey(sender, e);
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            DataTable dt = (DataTable)(dgv.DataSource);
            int row = e.RowHandle;
            if (e.Column.FieldName.ToUpper() == "NUM")
                e.DisplayText = (row + 1).ToString();
            else if (e.Column.FieldName.ToUpper() == "RATE")
            {
                if (!showRates)
                    e.DisplayText = "***.**";
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("PAY") >= 0)
            {
                if (!showRates)
                    e.DisplayText = "***.**";
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;

            bool doit = false;
            string column = e.Column.FieldName.ToUpper();
            if (column == "PAY")
                doit = true;
            else if (column == "CONTRACTPAY")
                doit = true;
            else if (column == "TOTALPAY")
                doit = true;
            else if (column == "RATE")
                doit = true;
            else if (column == "OTPAY")
                doit = true;
            else if (column == "OTHOURS")
                doit = true;
            else if (column == "TOTALHOURS")
                doit = true;
            else if (column == "HOURS")
                doit = true;
            else if (column == "CONTRACTHOURS")
                doit = true;
            else if (column == "WEEK1HOURS")
                doit = true;
            else if (column == "WEEK2HOURS")
                doit = true;
            if (doit)
            {
                double dValue = e.DisplayText.ObjToDouble();
                if (dValue == 0D)
                    e.DisplayText = "";
            }
            else if (e.DisplayText == "0.00")
                e.DisplayText = "";
        }
        /***********************************************************************************************/
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            loading = true;
            cmbSuper.Text = "All";
            cmbSuper.Refresh();
            loading = false;

            string location = cmbLocation.Text.Trim();
            DataTable dt = (DataTable)dgv.DataSource;
            if (location.ToUpper() == "ALL")
            {
                DataTable empDt = mainDt.Copy();
                G1.NumberDataTable(empDt);
                dgv2.DataSource = empDt;
                dgv.DataSource = empDt;

            }
            DataRow[] dRows = mainDt.Select("location='" + location + "'");
            if (dRows.Length > 0)
            {
                DataTable empDt = dRows.CopyToDataTable();
                G1.NumberDataTable(empDt);
                dgv2.DataSource = empDt;
                dgv.DataSource = empDt;
            }
        }
        /***********************************************************************************************/
        private void btnSaveData_Click(object sender, EventArgs e)
        {
            DataTable dt = mainDt;
            DateTime startdate = this.dateTimePicker1.Value;
            string startDate = startdate.ToString("yyyyMMdd");
            DateTime stopdate = this.dateTimePicker2.Value;
            string endDate = stopdate.ToString("yyyyMMdd");
            string username = "";
            double rate = 0D;
            double otrate = 0D;
            double hours = 0D;
            double pay = 0D;
            double newHours = 0D;
            double otpay = 0D;
            double total = 0D;
            double contractHours = 0D;
            double contractPay = 0D;
            double otherPay = 0D;
            double totalPay = 0D;
            double vacationhours = 0D;
            double vacationpay = 0D;
            double holidayhours = 0D;
            double holidaypay = 0D;
            double sickhours = 0D;
            double sickpay = 0D;
            double week1 = 0D;
            double week2 = 0D;
            double cweek1 = 0D;
            double cweek2 = 0D;
            double week1vhours = 0D;
            double week2vhours = 0D;
            double week1hhours = 0D;
            double week2hhours = 0D;
            double week1shours = 0D;
            double week2shours = 0D;

            double week1hours = 0D;
            double week2hours = 0D;
            double cweek1hours = 0D;
            double cweek2hours = 0D;
            double week1othours = 0D;
            double week2othours = 0D;
            double othours = 0D;

            double basePay = 0D;
            double fRate = 0D;

            string empStatus = "";
            string empType = "";
            string salaried = "";
            string isBiWeekly = "";
            string flux = "";
            double salary = 0D;
            double biWeekly = 0D;

            double totalHours = 0D;
            double vacationPay = 0D;
            double holidayPay = 0D;
            double sickPay = 0D;

            DataTable dx = null;
            string record = "";

            string cmd = "DELETE from `tc_pay` WHERE `user` = '-1'";
            G1.get_db_data(cmd);

            cmd = "DELETE from `tc_pay` WHERE ``startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "';";

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    username = dt.Rows[i]["username"].ObjToString();
                    if ( username.ToUpper() == "WILLS")
                    {

                    }
                    cmd = "Select * from `tc_pay` WHERE `username` = '" + username + "' AND `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        record = dx.Rows[0]["record"].ObjToString();
                    else
                    {
                        record = G1.create_record("tc_pay", "user", "-1");
                        if (G1.BadRecord("tc_pay", record))
                            break;
                    }
                    G1.update_db_table("tc_pay", "record", record, new string[] { "user", LoginForm.username, "username", username, "startdate", startDate, "enddate", endDate });

                    rate = dt.Rows[i]["rate"].ObjToDouble();
                    salary = dt.Rows[i]["salary"].ObjToDouble();
                    biWeekly = dt.Rows[i]["biWeekly"].ObjToDouble();

                    salaried = dt.Rows[i]["salaried"].ObjToString();
                    flux = dt.Rows[i]["flux"].ObjToString();

                    hours = dt.Rows[i]["hours"].ObjToDouble();
                    pay = dt.Rows[i]["pay"].ObjToDouble();
                    contractHours = dt.Rows[i]["contractHours"].ObjToDouble();
                    contractPay = dt.Rows[i]["contractPay"].ObjToDouble();
                    otherPay = dt.Rows[i]["otherPay"].ObjToDouble();
                    totalHours = dt.Rows[i]["totalHours"].ObjToDouble();
                    totalPay = dt.Rows[i]["totalPay"].ObjToDouble();
                    othours = dt.Rows[i]["othours"].ObjToDouble();
                    otpay = dt.Rows[i]["otpay"].ObjToDouble();
                    week1hours = dt.Rows[i]["week1hours"].ObjToDouble();
                    week2hours = dt.Rows[i]["week2hours"].ObjToDouble();
                    cweek1hours = dt.Rows[i]["cweek1hours"].ObjToDouble();
                    cweek2hours = dt.Rows[i]["cweek2hours"].ObjToDouble();

                    week1vhours = dt.Rows[i]["week1vhours"].ObjToDouble();
                    week2vhours = dt.Rows[i]["week2vhours"].ObjToDouble();
                    week1hhours = dt.Rows[i]["week1hhours"].ObjToDouble();
                    week2hhours = dt.Rows[i]["week2hhours"].ObjToDouble();
                    week1shours = dt.Rows[i]["week1shours"].ObjToDouble();
                    week2shours = dt.Rows[i]["week2shours"].ObjToDouble();

                    vacationhours = dt.Rows[i]["vacationhours"].ObjToDouble();
                    holidayhours = dt.Rows[i]["holidayhours"].ObjToDouble();
                    sickhours = dt.Rows[i]["sickhours"].ObjToDouble();
                    vacationPay = dt.Rows[i]["vacationPay"].ObjToDouble();
                    holidayPay = dt.Rows[i]["holidayPay"].ObjToDouble();
                    sickPay = dt.Rows[i]["sickPay"].ObjToDouble();

                    empType = dt.Rows[i]["EmpType"].ObjToString();
                    empStatus = dt.Rows[i]["EmpStatus"].ObjToString();

                    G1.update_db_table("tc_pay", "record", record, new string[] { "rate", rate.ToString(), "salary", salary.ToString(), "biWeekly", biWeekly.ToString(), "salaried", salaried, "flux", flux, "copied", "Y" });

                    G1.update_db_table("tc_pay", "record", record, new string[] { "totalHours", totalHours.ToString(), "hours", hours.ToString(), "pay", pay.ToString(), "contractHours", contractHours.ToString(), "contractPay", contractPay.ToString(),"totalPay", totalPay.ToString(), "otpay", otpay.ToString(), "othours", othours.ToString(), "week1hours", week1hours.ToString(),"week2hours", week2hours.ToString(), "cweek1hours", cweek1hours.ToString(), "cweek2hours", cweek2hours.ToString(), "otherPay", otherPay.ToString() });
                    G1.update_db_table("tc_pay", "record", record, new string[] { "vacationhours", vacationhours.ToString(), "vacationpay", vacationPay.ToString(), "holidayhours", holidayhours.ToString(), "holidaypay", holidayPay.ToString(), "sickhours", sickhours.ToString(), "sickpay", sickPay.ToString(), "EmpType", empType, "EmpStatus", empStatus });
                    G1.update_db_table("tc_pay", "record", record, new string[] { "week1vhours", week1vhours.ToString(), "week2vhours", week2vhours.ToString(), "week1hhours", week1hhours.ToString(), "week2hhours", week2hhours.ToString(), "week1shours", week1shours.ToString(), "week2shours", week2shours.ToString() });
                }
                catch ( Exception ex)
                {
                }
            }
            btnSaveData.Hide();
            btnSaveData.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain2_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    int maxHeight = 0;
            //    int newHeight = 0;
            //    bool doit = false;
            //    string name = "";
            //    int count = 0;
            //    string[] Lines = null;
            //    foreach (GridColumn column in gridMain2.Columns)
            //    {
            //        name = column.FieldName.ToUpper();
            //        if (name == "EXTRAPAY" || name == "EXTRAHOURS" )
            //            doit = true;
            //        if (doit)
            //        {
            //            using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
            //            {
            //                using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
            //                {
            //                    viewInfo.EditValue = gridMain2.GetRowCellValue(e.RowHandle, column.FieldName);
            //                    Lines = viewInfo.Editable.ObjToString().Split('\n');
            //                    if (Lines.Length > count)
            //                        count = Lines.Length;
            //                    if ( Lines.Length > 1 )
            //                    {
            //                    }
            //                    viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv2.Height);
            //                    using (Graphics graphics = dgv2.CreateGraphics())
            //                    using (GraphicsCache cache = new GraphicsCache(graphics))
            //                    {
            //                        viewInfo.CalcViewInfo(graphics);
            //                        var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
            //                        newHeight = Math.Max(height, maxHeight);
            //                        if (newHeight > maxHeight)
            //                            maxHeight = newHeight;
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    if (maxHeight > 0)
            //    {
            //        if (count == 1)
            //            e.RowHeight = maxHeight + 5;
            //        else
            //            e.RowHeight = maxHeight + ((count + 1) * 5);
            //    }
            //}
        }
        /***********************************************************************************************/
        private void menuReadPrevious_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker1.Value;
            string startDate = date.ToString("yyyyMMdd");
            DateTime edate = this.dateTimePicker2.Value;
            string endDate = edate.ToString("yyyyMMdd");

            DateTime currentDate = DateTime.Now;
            bool allowRed = false;
            if (currentDate >= date && currentDate <= edate)
                allowRed = true;
            if (currentDate >= date.AddDays(14) && currentDate <= edate.AddDays(14))
                allowRed = true;
            if (G1.RobbyServer)
                allowRed = true;
            if ( allowRed )
            {
                if (G1.isHR())
                {
                    //btnSaveData.Show();
                    //btnSaveData.Refresh();
                    btnLoad.BackColor = Color.Red;
                    btnLoad.ForeColor = Color.White;
                }
            }

            DataTable dx = null;
            string cmd = "";

            if (justTimeKeeper)
            {
                cmd = "SELECT DISTINCT * FROM `users` j LEFT JOIN tc_er e ON j.userName = e.username ";
                if (!G1.isHR() && !G1.isAdmin())
                {
                    cmd += " WHERE e.`username` = '" + LoginForm.username + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return;
                    if (dx.Rows[0]["isTimeKeeper"].ObjToString() != "Y")
                        return;

                    string name = dx.Rows[0]["lastName"].ObjToString() + ", " + dx.Rows[0]["firstName"].ObjToString();
                    string location = dx.Rows[0]["location"].ObjToString();
                    justTimeKeeper = true;

                    cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "' ";
//                    cmd = "SELECT DISTINCT * FROM `users` j LEFT JOIN tc_er e ON j.userName = e.username ";
                    if (!String.IsNullOrWhiteSpace(location))
                    {
                        cmd += " AND ( `TimeKeeper` = '" + name + "' OR `location` = '" + location + "' ) ";
                        //cmd += " AND e.`username` <> '" + LoginForm.username + "' ";
                    }
                    else
                        cmd += " AND `TimeKeeper` = '" + name + "' ";
//                    cmd += " ORDER BY j.`lastName`, j.`firstName`";

                    cmd += ";";

                    dx = G1.get_db_data(cmd);
                }
            }
            else
            {

                cmd = "Select DISTINCT * from `users` j RIGHT JOIN `tc_pay` p ON j.`username` = p.`username` LEFT JOIN `tc_er` r ON p.`username` = r.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "' ";
                //cmd = "Select * from `tc_pay` p LEFT JOIN `tc_er` r ON p.`username` = r.`username` LEFT JOIN `users` u ON p.`username` = u.`username` WHERE `startDate` = '" + startDate + "' AND `endDate` = '" + endDate + "';";
                dx = G1.get_db_data(cmd);
            }

            string copied = "";
            string salaried = "";
            string flux = "";
            double rate = 0D;
            double salary = 0D;
            double biWeekly = 0D;
            string record = "";
            if (G1.isAdmin() || G1.isHR())
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record1"].ObjToString();
                    copied = dx.Rows[i]["copied"].ObjToString().ToUpper();
                    if (copied != "Y")
                    {
                        salaried = dx.Rows[i]["salaried1"].ObjToString();
                        dx.Rows[i]["salaried"] = salaried;
                        flux = dx.Rows[i]["flux1"].ObjToString();
                        dx.Rows[i]["flux"] = flux;
                        rate = dx.Rows[i]["rate1"].ObjToDouble();
                        dx.Rows[i]["rate"] = rate;
                        salary = dx.Rows[i]["salary1"].ObjToDouble();
                        dx.Rows[i]["salary"] = salary;
                        biWeekly = dx.Rows[i]["biWeekly1"].ObjToDouble();
                        dx.Rows[i]["biWeekly"] = biWeekly;
                    }
                    else
                    {
                    }
                }
            }

            dgv2.DataSource = dx;

            RecalcAllBenefits();

            if ( G1.isHR() )
            {
                //gridMain2.Columns["totalPay"].OptionsColumn.AllowEdit = true;
                for ( int i=0; i<gridMain2.Columns.Count; i++)
                {
                    if ( gridMain2.Columns[i].DisplayFormat.FormatType == FormatType.Numeric)
                        gridMain2.Columns[i].OptionsColumn.AllowEdit = true;
                }
            }

            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
            dgv2.Refresh();
            this.Refresh();

            ScaleCells();

            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
            dgv2.Refresh();
            this.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain2);
        }
        /***********************************************************************************************/
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //gridMain2.RefreshEditor(true);
            //dgv2.Refresh();

            if (dgv.Visible)
            {
                dgv.RefreshDataSource();
                gridMain.RefreshData();
                dgv.Refresh();
                this.Refresh();
            }
            else if (dgv2.Visible)
            {
                dgv2.RefreshDataSource();
                gridMain2.RefreshData();
                dgv2.Refresh();
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void cmbExemptNonExempt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( dgv.Visible )
            {
                dgv.RefreshDataSource();
                gridMain.RefreshData();
                dgv.Refresh();
                this.Refresh();
            }
            else if ( dgv2.Visible )
            {
                dgv2.RefreshDataSource();
                gridMain2.RefreshData();
                dgv2.Refresh();
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            string payFilter = cmbPayStatus.Text.Trim();
            string payTypeFilter = cmbExemptNonExempt.Text.Trim();
            if (payFilter == "All" && payTypeFilter == "All")
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                string payStatus = dt.Rows[row]["EmpStatus"].ObjToString().Trim();
                if ( payFilter != "All")
                {
                    if ( payStatus != payFilter )
                    {
                        if (payStatus.IndexOf("&") < 0)
                        {
                            e.Visible = false;
                            e.Handled = true;
                            return;
                        }
                    }
                }
                string payType = dt.Rows[row]["EmpType"].ObjToString().Trim();
                if (payTypeFilter != "All")
                {
                    if (payType != payTypeFilter)
                    {
                        e.Visible = false;
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            string payFilter = cmbPayStatus.Text.Trim();
            string payTypeFilter = cmbExemptNonExempt.Text.Trim();
            if (payFilter == "All" && payTypeFilter == "All")
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                string payStatus = dt.Rows[row]["EmpStatus"].ObjToString().Trim();
                if (payFilter != "All")
                {
                    if (payStatus != payFilter)
                    {
                        if (payStatus.IndexOf("&") < 0)
                        {
                            e.Visible = false;
                            e.Handled = true;
                            return;
                        }
                    }
                }
                string payType = dt.Rows[row]["EmpType"].ObjToString().Trim();
                if (payTypeFilter != "All")
                {
                    if (payType != payTypeFilter)
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /***********************************************************************************************/
        public static bool isManager ()
        {
            string username = LoginForm.username.Trim();
            string cmd = "Select * from `tc_er` where `username` = '" + username + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;
            if (dx.Rows[0]["isManager"].ObjToString().ToUpper() == "Y")
                return true;
            return false;
        }
        /***********************************************************************************************/
        private void Employees_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (G1.isHR())
            {
                string cmd = "Select * from `users` WHERE `username` = '" + LoginForm.username + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;
                string record = dx.Rows[0]["record"].ObjToString();
                string status = "OFF";
                if ( !showRates )
                    status = "ON";
                G1.update_db_table("users", "record", record, new string[] { "ctrlRstatus", status });
            }
        }
        /****************************************************************************************/
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv2, "TimeSheets " + workReport, "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = workReport + " Primary";
            string saveName = "TimeSheets " + workReport + " " + name;
            string skinName = "";
            SetupSelectedColumns("TimeSheets " + workReport, name, dgv2);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, saveName, ref skinName);
            gridMain2.OptionsView.ShowFooter = true;
            SetupTotalsSummary();
            string field = "";
            string select = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                if (G1.get_column_number(gridMain, field) >= 0)
                {
                    if (select == "0")
                        gridMain2.Columns[field].Visible = false;
                    else
                        gridMain2.Columns[field].Visible = true;
                }
            }
            dgv2.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv2.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "TimeSheets " + workReport;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain2 = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv2.MainView;
            for (int i = 0; i < gridMain2.Columns.Count; i++)
                gridMain2.Columns[i].Visible = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    if ( G1.DoesGridViewColumnExist ( gridMain2, name ))
                        ((GridView)dgv2.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /****************************************************************************************/
        private void lockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "TimeSheets " + workReport + " " + name;
            G1.SaveLocalPreferences(this, gridMain2, LoginForm.username, saveName);

            //G1.SaveLocalPreferences(this, gridMain, LoginForm.username, "DailyHistoryLayout" );
            foundLocalPreference = true;
        }
        /****************************************************************************************/
        private void unLockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = comboName;
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "TimeSheets " + workReport + " " + name;
                G1.RemoveLocalPreferences(LoginForm.username, saveName);
                foundLocalPreference = false;
            }

            //G1.RemoveLocalPreferences(LoginForm.username, "DailyHistoryLayout");
            foundLocalPreference = false;
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
                    this.gridMain2.Appearance.EvenRow.BackColor = System.Drawing.Color.LightGreen;
                    this.gridMain2.Appearance.EvenRow.BackColor2 = System.Drawing.Color.LightGreen;
                    this.gridMain2.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                    this.gridMain2.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    this.panelTop.BackColor = Color.Transparent;
                    this.menuStrip1.BackColor = Color.Transparent;
                    this.gridMain2.PaintStyleName = "Skin";
                    DevExpress.Skins.SkinManager.EnableFormSkins();
                    this.LookAndFeel.UseDefaultLookAndFeel = true;
                    DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skin);
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.dgv2.LookAndFeel.SetSkinStyle(skin);
                    this.dgv2.LookAndFeel.SkinName = skin;
                    gridMain2.Appearance.EvenRow.Options.UseBackColor = false;
                    gridMain2.Appearance.OddRow.Options.UseBackColor = false;
                    this.panelTop.Refresh();
                    OnSkinChange(skin);

                    //DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = skin;
                    //this.LookAndFeel.SetSkinStyle(skin);
                    //this.dgv.LookAndFeel.SetSkinStyle(skin);
                }
            }
            else if (s.ToUpper().IndexOf("COLOR : ") >= 0)
            {
                string color = s.Replace("Color : ", "");
                this.gridMain2.Appearance.EvenRow.BackColor = Color.FromName(color);
                this.gridMain2.Appearance.EvenRow.BackColor2 = Color.FromName(color);
                this.gridMain2.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                this.gridMain2.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                this.gridMain2.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain2.Appearance.OddRow.Options.UseBackColor = true;
            }
            else if (s.ToUpper().IndexOf("NO COLOR ON") >= 0)
            {
                this.gridMain2.Appearance.EvenRow.Options.UseBackColor = false;
                this.gridMain2.Appearance.OddRow.Options.UseBackColor = false;
            }
            else if (s.ToUpper().IndexOf("NO COLOR OFF") >= 0)
            {
                this.gridMain2.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain2.Appearance.OddRow.Options.UseBackColor = true;
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string SkinChange;
        protected void OnSkinChange(string done)
        {
            if (SkinChange != null)
                SkinChange.Invoke(done);
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string procName = "TimeSheets " + workReport;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procName + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain2.Columns.Count; i++)
                gridMain2.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv2.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                if (name.Trim().ToUpper() == "PRIMARY")
                    primaryName = name;
                cmb.Items.Add(name);
            }
            if (!String.IsNullOrWhiteSpace(primaryName))
                cmb.Text = primaryName;
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string comboName = cmbSelectColumns.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("TimeSheets " + workReport, comboName, dgv);
                string name = "TimeSheets " + workReport + " " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("TimeSheets " + workReport, "Primary", dgv);
                string name = "TimeSheets" + workReport + " Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv2, gridMain2, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }

            CleanupColumns();

            ScaleCells();
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain2.Columns["lastName"].AppearanceCell.Font.Size;
                mainFont = gridMain2.Columns["lastName"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain2.Columns.Count; i++)
            {
                gridMain2.Columns[i].AppearanceCell.Font = font;
                gridMain2.Columns[i].AppearanceHeader.Font = font;
            }
            gridMain2.Appearance.GroupFooter.Font = font;
            gridMain2.AppearancePrint.FooterPanel.Font = font;
            gridMain2.Appearance.FocusedRow.Font = font;
            newFont = font;
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
            dgv2.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (newFont != null)
                e.Appearance.Font = newFont;
            if ( !showRates )
            {
                string field = e.Column.FieldName.Trim().ToUpper();
                if ( field.IndexOf ( "PAY") >= 0 )
                {
                    int dx = e.Bounds.Height;
                    Brush brush = e.Cache.GetGradientBrush(e.Bounds, Color.Wheat, Color.FloralWhite, LinearGradientMode.Vertical);
                    Rectangle r = e.Bounds;
                    //Draw a 3D border
                    BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
                    AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
                    borderAppearance.BorderColor = Color.DarkGray;
                    painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
                    //Fill the inner region of the cell
                    r.Inflate(-1, -1);
                    //e.Cache.FillRectangle(brush, r);
                    //Draw a summary value
                    r.Inflate(-2, 0);
                    e.Info.DisplayText = "***.**";
                    e.Appearance.DrawString(e.Cache, e.Info.DisplayText, r);
                    //Prevent default drawing of the cell
                    e.Handled = true;
                }
            }
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtScale.Text = balance;
                ScaleCells();
                return;
            }
            // Initialize the flag to false.
            bool nonNumberEntered = false;

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Determine whether the keystroke is a number from the keypad.
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    // Determine whether the keystroke is a backspace.
                    if (e.KeyCode != Keys.Back)
                    {
                        // A non-numerical keystroke was pressed.
                        // Set the flag to true and evaluate in KeyPress event.
                        if (e.KeyCode != Keys.OemPeriod)
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if (nonNumberEntered)
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void otherServicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isHR() && !G1.isAdmin())
            {
                MessageBox.Show("*** Sorry *** This Function is not available at this time!!", "PartTime Labor Services Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            string empno = LoginForm.workUserRecord;
            EditContractServices contractForm = new EditContractServices ( "Other", empno);
            contractForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadHrGroups ()
        {
            string cmd = "Select * from `tc_hr_groups`;";
            DataTable dx = G1.get_db_data ( cmd );

            cmbHrGroups.Items.Clear();
            cmbHrGroups.Items.Add("All");

            for ( int i=0; i<dx.Rows.Count; i++ )
            {

                cmbHrGroups.Items.Add(dx.Rows[i]["groupName"].ObjToString());
            }
        }
        /***********************************************************************************************/
        private void LoadTimeOffRequests()
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit5;
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";

            if ( !G1.isHR() && !G1.isSpecial() )
            {
                gridView6.Columns["approved"].OptionsColumn.AllowEdit = false;
                gridView6.Columns["approved"].OptionsColumn.ReadOnly = true;

                gridView6.Columns["pto_now"].Visible = false;
                //gridView6.Columns["pto_taken"].Visible = false;
                gridView6.Columns["december"].Visible = false;
            }

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
            dt.Columns.Add("Location");
            dt.Columns.Add("comment");
            dt.Columns.Add("pto_taken", Type.GetType("System.Double"));
            dt.Columns.Add("pto_now", Type.GetType("System.Double"));
            dt.Columns.Add("pto_inc", Type.GetType("System.Double"));
            dt.Columns.Add("hiredate");
            dt.Columns.Add("december", Type.GetType("System.Double"));

            bool canDo = CheckCanDo();

            string cmd = "Select * from `tc_er` WHERE `username` = '" + work_username + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string myLocation = dx.Rows[0]["location"].ObjToString();
            if (String.IsNullOrWhiteSpace(myLocation))
                return;

            if (!G1.isHR())
            {
                cmbHrGroups.Hide();
                label8.Hide();
            }

            //if (justManager || justTimeKeeper)
            //    return;

            string hrgroup = cmbHrGroups.Text;

            if ( G1.isHR() && hrgroup.Trim() != "All")
            {
                cmd = "Select * from `tc_hr_groups` WHERE `groupName` = '" + hrgroup + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count == 0)
                {
                    cmd = "Select * from `tc_hr_groups` WHERE `locations` LIKE '%" + myLocation + "%';";
                    dx = G1.get_db_data(cmd);
                }
            }
            else
            {
                cmd = "Select * from `tc_hr_groups` WHERE `locations` LIKE '%" + myLocation + "%';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    workGroupName = dx.Rows[0]["groupName"].ObjToString();
                    if (G1.isHR() && hrgroup.Trim() == "All")
                        workGroupName = "All Groups";
                }
            }
            if (dx.Rows.Count <= 0)
                return;

            string locations = dx.Rows[0]["locations"].ObjToString();
            if (justManager || justTimeKeeper)
                locations = myLocation;

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
                DateTime date = this.dateTimePicker1.Value;
                DateTime fDate = new DateTime(date.Year, 1, 1);
                DateTime tDate = new DateTime(date.Year, 12, 31);
                string date1 = fDate.ToString("yyyy-MM-dd");
                string date2 = tDate.ToString("yyyy-MM-dd");

                if ( G1.isHR() && hrgroup == "All")
                    cmd = "Select * from `tc_timerequest` r JOIN `tc_er` e ON r.`empno` = e.`username` JOIN `users` u ON r.`empno` = u.`username` WHERE `fromDate` >= '" + date1 + "' AND `toDate` <= '" + date2 + "' ";
                else
                    cmd = "Select * from `tc_timerequest` r JOIN `tc_er` e ON r.`empno` = e.`username` JOIN `users` u ON r.`empno` = u.`username` WHERE `fromDate` >= '" + date1 + "' AND `toDate` <= '" + date2 + "' AND e.`location` IN (" + query + ") ";
                if (cmbMyProc.Text.ToUpper() == "APPROVED")
                    cmd += " and `approved` = 'Y' ";
                else if (cmbMyProc.Text.ToUpper() == "UNAPPROVED")
                    cmd += " and `approved` <> 'Y' ";
                cmd += " order by `fromdate`; ";
                dx = G1.get_db_data(cmd);
            }
            catch (Exception ex)
            {
            }

            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;
            DateTime Date = DateTime.Now;
            DateTime hireDate = DateTime.Now;
            double yearlyVacation = 0D;
            double yearlySick = 0D;
            string workEmpNo = "";

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                empno = dx.Rows[i]["empno"].ObjToString();
                workEmpNo = dx.Rows[i]["record2"].ObjToString();
                name = dx.Rows[i]["name"].ObjToString();
                pto_now = dx.Rows[i]["pto_now"].ObjToDouble();
                december = dx.Rows[i]["december"].ObjToDouble();

                supervisor = dx.Rows[i]["supervisor"].ObjToString();
                approved_by = dx.Rows[i]["approved_by"].ObjToString();
                approved = dx.Rows[i]["approved"].ObjToString();
                fromDate = dx.Rows[i]["fromdate"].ObjToDateTime();
                toDate = dx.Rows[i]["todate"].ObjToDateTime();
                Date = dx.Rows[i]["date_requested"].ObjToDateTime();
                hireDate = dx.Rows[i]["hireDate"].ObjToDateTime();
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
                dRow["hireDate"] = hireDate.Month.ToString("D2") + "/" + hireDate.Day.ToString("D2") + "/" + hireDate.Year.ToString("D4");
                dRow["hours"] = hours;
                dRow["comment"] = comment;
                dRow["Location"] = dx.Rows[i]["Location"].ObjToString();

                Employees.SetupBenefits (hireDate, fromDate, ref yearlyVacation, ref yearlySick);
                double pto_taken = TimeClock.CalcPTOupto ( workEmpNo, hireDate, fromDate, toDate, ref pto_now );

                dRow["pto_taken"] = pto_taken;
                dRow["pto_now"] = pto_now;
                dRow["december"] = yearlyVacation;
                dt.Rows.Add(dRow);
            }

            if (!canDo)
                gridView6.Columns["empno"].Visible = false;

            G1.NumberDataTable(dt);

            dgv6.DataSource = dt;

            if (!justTimeKeeper && !justManager && !G1.isAdmin() && !G1.isHR())
            {
                btnCalendar.Hide();
                btnCalendar.Refresh();
            }
            if ( !G1.isAdmin() && !G1.isHR() )
            {
                pictureBox5.Hide();
                pictureBox5.Refresh();
            }
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridView6);
        }
        /***********************************************************************************************/
        private void btnRefreshTime_Click(object sender, EventArgs e)
        {
            LoadTimeOffRequests();
        }
        /***********************************************************************************************/
        private void cmbMyProc_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTimeOffRequests();
        }
        /***********************************************************************************************/
        private void cmbHrGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTimeOffRequests();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit5_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridView6.GetFocusedDataRow();

            DataTable dt = (DataTable)dgv6.DataSource;

            if ( !G1.isHR() )
            {
                MessageBox.Show("*** Information *** You do not have permission to approve vacation!", "Vacation Approval Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string record = dr["record"].ObjToString();
            string approved = dr["approved"].ObjToString();
            if ( approved.ToUpper() != "Y" )
            {
                G1.update_db_table("tc_timerequest", "record", record, new string[] { "approved", "Y", "approved_by", work_username });
                dr["approvedby"] = work_username;
            }
        }
        /***********************************************************************************************/
        private void btnCalendar_Click(object sender, EventArgs e)
        {
            //            private string work_empno = "";
            //private string work_myName = "";
            //private string work_username = "";

            string hrgroup = cmbHrGroups.Text;

            if (G1.isHR() && hrgroup != "All")
                workGroupName = hrgroup;

            DataTable dt = (DataTable)dgv6.DataSource;
            Calendar2 calendarForm = new Calendar2(dt, work_empno, work_myName, dateTimePicker1.Value, workGroupName );
            calendarForm.Text = workGroupName + " Calendar";
            calendarForm.CalendarDone += CalendarForm_CalendarDone;
            calendarForm.Show();
        }
        /***********************************************************************************************/
        private void CalendarForm_CalendarDone(DataTable dt)
        {
        }
        /***********************************************************************************************/
        private void pictureBox5_Click(object sender, EventArgs e)
        { // Remove Vacation Request
            DataRow dr = gridView6.GetFocusedDataRow();
            string data = dr["approved"].ObjToString();
            if (data.ToUpper() == "Y")
            {
                if (!G1.isHR())
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
            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt == null)
                return;

            string record = dr["record"].ObjToString();
            int rowHandle = gridView6.FocusedRowHandle;
            int row = gridView6.GetFocusedDataSourceRowIndex();
            dt.Rows.Remove(dr);
            gridView6.DeleteRow(rowHandle);

            G1.delete_db_table("tc_timerequest", "record", record);
        }
        /***********************************************************************************************/
        private void payrollReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            PayrollReport reportForm = new PayrollReport();
            reportForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void historicCommissionFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistoricCommissionFormat histForm = new HistoricCommissionFormat();
            histForm.Show();
        }
        /***********************************************************************************************/
        private void runHistoricEmployeeCommissionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            dt = AddAgents(dt);

            HistoricEmployeeCommissions histForm = new HistoricEmployeeCommissions( dt );
            histForm.Show();
        }
        /***********************************************************************************************/
        private DataTable AddAgents ( DataTable dt )
        {
            string firstName = "";
            string lastName = "";
            string preferredName = "";

            string fName = "";
            string lName = "";

            DataRow[] dRows = null;
            DataRow dRow = null;

            DataTable dx = dt.Copy();

            string cmd = "Select DISTINCT firstName,lastName from `agents` ORDER by lastName;";
            DataTable agentDt = G1.get_db_data(cmd);

            for ( int i=0; i<agentDt.Rows.Count; i++)
            {
                firstName = agentDt.Rows[i]["firstName"].ObjToString();
                lastName = agentDt.Rows[i]["lastName"].ObjToString();

                dRows = dx.Select("firstName='" + firstName  + "' AND lastName = '" + lastName + "'");
                if (dRows.Length > 0)
                    continue;

                dRows = dx.Select("preferredName='" + firstName + "' AND lastName = '" + lastName + "'");
                if (dRows.Length > 0)
                    continue;
                dRow = dt.NewRow();
                dRow["firstName"] = firstName;
                dRow["lastName"] = lastName;
                dt.Rows.Add(dRow);
            }
            return dt;
        }
        /***********************************************************************************************/
        private void showEmployeeCommissionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();

            if (!dgv.Visible && !dgv2.Visible)
                return;

            //DataTable dt = (DataTable)dgv.DataSource;

            string empno = dr["record"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string userName = dr["userName"].ObjToString();
            string preferredName = dr["preferredName"].ObjToString();



            HistoricEmployeeCommissions histForm = new HistoricEmployeeCommissions ( lastName, firstName, preferredName );
            histForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain2.FocusedRowHandle;
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            bool bad = false;
            string field = e.Column.FieldName.Trim();
            string data = dr[field].ObjToString().Trim();
            string record = dr["record1"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                bad = true;
            if (record == "0" || record == "-1")
                bad = true;
            if ( bad )
            {
                dr[field] = oldWhat2;
                gridMain2.RefreshEditor(true);
                return;
            }
            try
            {
                DateTime date = this.dateTimePicker1.Value;
                string startDate = date.ToString("yyyyMMdd");
                date = this.dateTimePicker2.Value;
                string endDate = date.ToString("yyyyMMdd");
                string userName = dr["username"].ObjToString();

                if (G1.validate_numeric(data))
                {
                    G1.update_db_table("tc_pay", "record", record, new string[] { field, data });
                    string who = "Pay Period Ending " + endDate;
                    string what = field + " From " + oldWhat2 + " To " + data;
                    G1.AddToAudit(LoginForm.username, "Payroll", who, what, userName);
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private string oldWhat2 = "";
        /***********************************************************************************************/
        private void gridMain2_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain2.GetFocusedDataRow();
            string field = gridMain2.FocusedColumn.FieldName.ToUpper();
            string data = dr[field].ObjToString();
            oldWhat2 = data;
        }
        /***********************************************************************************************/
        private void showEmployeeCommissionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();

            if (!dgv.Visible && !dgv2.Visible)
                return;

            //DataTable dt = (DataTable)dgv.DataSource;

            string empno = dr["record"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string userName = dr["userName"].ObjToString();
            string preferredName = dr["preferredName"].ObjToString();



            HistoricEmployeeCommissions histForm = new HistoricEmployeeCommissions(lastName, firstName, preferredName);
            histForm.Show();
        }
        /***********************************************************************************************/
        private bool RunningSingle = false;
        private void recalculatePayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime timePeriod1 = dateTimePicker1.Value;
            //timePeriod1 = timePeriod1.AddMinutes(301); // This gets the time to 5:01 PM
            long ldate = G1.TimeToUnix(timePeriod1);

            DateTime timePeriod = dateTimePicker2.Value;
            //timePeriod = timePeriod.AddHours(5); // This gets the time to 4:59 PM
            //timePeriod = timePeriod.AddMinutes(-420); // This gets the time back to 23:59:00
            long edate = G1.TimeToUnix(timePeriod);

            TimeSpan ts = timePeriod - timePeriod1;

            btnSaveData.Hide();
            btnSaveData.Refresh();

            bool showSave = true;
            if ((ts.Days - 1) > 14)
                showSave = false;

            long adate = 0L;

            DateTime firstDate = ldate.UnixToDateTime().ToLocalTime();
            //firstDate = firstDate.AddDays(-1);
            DateTime lastDate = edate.UnixToDateTime().ToLocalTime();



            DataRow dr = gridMain2.GetFocusedDataRow();

            DataTable empDt = (DataTable)dgv2.DataSource;
            DataTable dx = (DataTable)dgv2.DataSource;

            string empno = dr["record"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string userName = dr["userName"].ObjToString();
            string preferredName = dr["preferredName"].ObjToString();

            int i = gridMain2.GetFocusedDataSourceRowIndex();

            RunningSingle = true;

            string cmd = "Select * from `tc_punches_pchs` where `date` >= '" + firstDate.ToString("yyyyMMdd") + "' AND `date` <= '" + lastDate.ToString("yyyyMMdd") + "' ";
            cmd += " AND `empy!AccountingID` = '" + empno + "' ";
            cmd += "order by `empy!AccountingID`,`date`;";
            dx = G1.get_db_data(cmd);

            CleanupRow(empDt, i);

            bool gotChanges = false;
            dx = LoadPayrollDetails(dx, empno, ref gotChanges ); // Individual Pay

            if (gotChanges)
            {
                CalculateSpecialPay(empDt, dx, i); // Individual Pay

                //CalcOvertime(empDt, i);
            }
            else
            {
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    CalculateEmployeeDetail(empDt, dx, j);
                }

                CalcOvertime(empDt, i);
            }

            RunningSingle = false;

            //G1.NumberDataTable(empDt);
            //dgv2.DataSource = empDt;

            //empDt = LoadApprovals(empDt);

            //mainDt = empDt;


            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
            dgv2.Refresh();

        }
        /***********************************************************************************************/
        private void CleanupRow ( DataTable empDt, int i )
        {
            try
            {

                VerifyColumns(empDt);

                empDt.Rows[i]["hours"] = 0D;
                empDt.Rows[i]["week1hours"] = 0D;
                empDt.Rows[i]["week2hours"] = 0D;
                empDt.Rows[i]["cweek1hours"] = 0D;
                empDt.Rows[i]["cweek2hours"] = 0D;
                empDt.Rows[i]["pay"] = 0D;
                empDt.Rows[i]["othours"] = 0D;
                empDt.Rows[i]["otpay"] = 0D;
                empDt.Rows[i]["contractHours"] = 0D;
                empDt.Rows[i]["contractPay"] = 0D;
                empDt.Rows[i]["otherPay"] = 0D;
                empDt.Rows[i]["totalHours"] = 0D;
                empDt.Rows[i]["totalPay"] = 0D;
                empDt.Rows[i]["vacationhours"] = 0D;
                empDt.Rows[i]["holidayhours"] = 0D;
                empDt.Rows[i]["sickhours"] = 0D;
                empDt.Rows[i]["vacationpay"] = 0D;
                empDt.Rows[i]["holidaypay"] = 0D;
                empDt.Rows[i]["sickpay"] = 0D;

                empDt.Rows[i]["week1vhours"] = 0D;
                empDt.Rows[i]["week2vhours"] = 0D;
                empDt.Rows[i]["cweek1vhours"] = 0D;
                empDt.Rows[i]["cweek2vhours"] = 0D;

                empDt.Rows[i]["week1hhours"] = 0D;
                empDt.Rows[i]["week2hhours"] = 0D;
                empDt.Rows[i]["cweek1hhours"] = 0D;
                empDt.Rows[i]["cweek2hhours"] = 0D;

                empDt.Rows[i]["week1shours"] = 0D;
                empDt.Rows[i]["week2shours"] = 0D;
                empDt.Rows[i]["cweek1shours"] = 0D;
                empDt.Rows[i]["cweek2shours"] = 0D;

                empDt.Rows[i]["week1othours"] = 0D;
                empDt.Rows[i]["week2othours"] = 0D;

                empDt.Rows[i]["totalothours"] = 0D;
                empDt.Rows[i]["week1Days"] = 0D;
                empDt.Rows[i]["week2Days"] = 0D;
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void employeeDemographicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();

            DataTable dt = (DataTable)dgv2.DataSource;

            string empno = dr["record"].ObjToString();
            string name = dr["firstName"].ObjToString() + " " + dr["lastName"].ObjToString();
            string userName = dr["userName"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            //TimeClock timeForm = new TimeClock(empno, userName, name);
            //timeForm.Show();

            EmployeeDemo employeeForm = new EmployeeDemo(empno);
            employeeForm.EmployeeDone += EmployeeForm_EmployeeDone;
            employeeForm.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private string actualFile = "";
        private string importedFile = "";
        private void btnImportSick_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    importedFile = file;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }

                    dgv7.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    DataTable workDt = null;
                    try
                    {
                        workDt = ExcelWriter.ReadFile2(file);

                        workDt = PreProcessData(workDt, "EMPLOYEE");

                        workDt.Columns.Add("found");
                        string employee = "";
                        string firstName = "";
                        string lastName = "";
                        string[] Lines = null;
                        DataRow[] dRows = null;
                        DataTable dt = (DataTable)dgv.DataSource;
                        for (int i = 0; i < workDt.Rows.Count; i++)
                        {
                            employee = workDt.Rows[i]["Employee"].ObjToString();
                            if (String.IsNullOrWhiteSpace(employee))
                                continue;
                            Lines = employee.Split(' ');
                            if ( Lines.Length <= 1 )
                            {
                                workDt.Rows[i]["found"] = "NOT FOUND";
                                continue;
                            }
                            firstName = Lines[0].Trim();
                            lastName = Lines[1].Trim();
                            dRows = dt.Select("firstName='" + firstName + "' AND lastName = '" + lastName + "'");
                            if ( dRows.Length <= 0)
                            {
                                workDt.Rows[i]["found"] = "NOT FOUND";
                                continue;
                            }
                        }

                        workDt.TableName = actualFile;
                    }
                    catch (Exception ex)
                    {
                    }
                    workDt.TableName = actualFile;

                    string title = workDt.TableName.Trim();
                    title = title.Replace(".csv", "");

                    //tabPage1.Text = title;
                    this.Text = title;

                    if (G1.get_column_number(workDt, "num") < 0)
                        workDt.Columns.Add("num");
                    G1.NumberDataTable(workDt);
                    dgv7.DataSource = workDt;


                    btnSaveSick.Show();
                    btnSaveSick.Refresh();
                }
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable PreProcessData(DataTable dt, string search )
        {
            int firstRow = -1;
            bool newFormat = false;
            string str = "";
            DataTable newDt = dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ObjToString().ToUpper();
                    if (str == search)
                    {
                        firstRow = i;
                        break;
                    }
                }
                if (firstRow >= 0)
                    break;
            }
            if (firstRow < 0)
                firstRow = 0;

            if (firstRow < 0)
                return newDt;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                str = dt.Rows[firstRow][i].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (G1.get_column_number(dt, str) >= 0)
                    str = str + "2";
                newDt.Columns[i].ColumnName = str;
                newDt.Columns[i].Caption = str;

                dt.Columns[i].ColumnName = str;
                dt.Columns[i].Caption = str;
            }
            for (int i = (firstRow + 1); i < dt.Rows.Count; i++)
            {
                newDt.ImportRow(dt.Rows[i]);
            }
            return newDt;
        }
        /***********************************************************************************************/
        private void chkShowNotFound_CheckedChanged(object sender, EventArgs e)
        {
            gridMain7.RefreshData();
            gridMain7.RefreshEditor(true);
            dgv7.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain7_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv7 == null)
                return;
            if (dgv7.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv7.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            if (chkShowNotFound.Checked)
            {
                string found = dt.Rows[row]["found"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( found ) )
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
        }
        /***********************************************************************************************/
        private void btnSaveSick_Click(object sender, EventArgs e)
        {
            DataTable dt7 = (DataTable)dgv7.DataSource;
            string employee = "";
            string firstName = "";
            string lastName = "";
            string record = "";
            string username = "";
            double sickDays = 0D;
            string cmd = "";
            DataTable sickDt = null;
            string[] Lines = null;
            DataRow[] dRows = null;
            DataTable dt = (DataTable)dgv.DataSource;

            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt7.Rows.Count; i++)
            {
                employee = dt7.Rows[i]["Employee"].ObjToString();
                if (String.IsNullOrWhiteSpace(employee))
                    continue;
                Lines = employee.Split(' ');
                if (Lines.Length <= 1)
                    continue;
                firstName = Lines[0].Trim();
                lastName = Lines[1].Trim();
                dRows = dt.Select("firstName='" + firstName + "' AND lastName = '" + lastName + "'");
                if (dRows.Length <= 0)
                    continue;
                sickDays = dt7.Rows[i]["Sick Days"].ObjToDouble();
                record = dRows[0]["record1"].ObjToString();
                username = dRows[0]["username"].ObjToString();

                cmd = "Select * from `tc_sick` where `username` = '" + username + "';";
                sickDt = G1.get_db_data(cmd);
                if (sickDt.Rows.Count <= 0)
                {
                    record = G1.create_record("tc_sick", "username", username);
                    if (G1.BadRecord("tc_sick", record))
                        continue;
                }
                else
                    record = sickDt.Rows[0]["record"].ObjToString();

                G1.update_db_table("tc_sick", "record", record, new string[] { "Y2022", sickDays.ToString()});
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}
