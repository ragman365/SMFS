using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
using Tracking;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraReports.Design;
using DevExpress.XtraEditors.Controls;
using DevExpress.Utils;
using DevExpress.XtraBars;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors.ViewInfo;

using System.Runtime.InteropServices;
using System.Drawing;
using DevExpress.XtraEditors.Popup;
using DevExpress.Utils.Win;
using DevExpress.XtraGrid.Views.Base;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContactReportsAgents : DevExpress.XtraEditors.XtraForm
    {
        private DevExpress.XtraGrid.Views.Grid.GridView workGV = null;
        bool loading = false;
        private string workContract = "";
        private bool funModified = false;
        private bool otherModified = false;
        private string workAgent = "";
        private DataTable workDt = null;
        /****************************************************************************************/
        private DataTable originalDt = null;
        private bool autoRun = false;
        private bool autoForce = false;
        private string workReport = "";
        private string sendTo = "";
        private string sendWhere = "";
        private string sendUsername = "";
        private string da = "";
        private string forceReportName = "";
        private string workReportIn = "";
        private string workModule = "";
        private string workSendTo = "";
        /****************************************************************************************/
        EditCust editCust = null;
        /****************************************************************************************/

        public ContactReportsAgents( DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView dgv, string agent, string module )
        {
            InitializeComponent();

            workDt = dt;
            workGV = dgv;
            workAgent = agent;
            workModule = module;
        }
        /****************************************************************************************/
        public ContactReportsAgents(bool auto, bool force, string send, string sendTo, string username, string report, string ReportName = "" )
        {
            InitializeComponent();
            autoRun = auto;
            autoForce = force;
            sendWhere = send;
            workSendTo = sendTo;
            sendUsername = username;
            workReportIn = report;
            forceReportName = ReportName;
            RunAutoReports();
            if (auto)
                this.Close();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            if ( String.IsNullOrWhiteSpace ( forceReportName ) )
            {
                int idx = workReportIn.IndexOf('{');
                if ( idx > 0 )
                {
                    workReport = workReportIn.Substring(idx);
                    workReport = workReport.Replace("{", "");
                    workReport = workReport.Replace("}", "").Trim();
                    forceReportName = workReport;
                }
            }

            bool force = false;
            string module = "";
            G1.AddToAudit("System", "AutoRun", "AT Agent Contacts Report", "Starting Agent Contacts Autorun . . . . . . . ", "");
            workReport = "Agent Contacts Report for " + DateTime.Now.ToString("MM/dd/yyyy");
            string cmd = "Select * from `contacts_reports_data` WHERE `agent` <> '' ";
            if (!String.IsNullOrWhiteSpace(forceReportName))
                cmd += " AND `report` = '" + forceReportName + "' ";
            cmd += ";";

            DataTable agentDt = null;
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                cmd = "Select * from `contacts_reports` WHERE `report` = '" + forceReportName + "';";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    module = dt.Rows[0]["module"].ObjToString();
                    workModule = module;
                    string rec = dt.Rows[0]["record"].ObjToString();
                    cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + rec + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                    force = true;
                }
            }
            else
            {
                agentDt = dt.Copy();

                agentDt = LoadEmails(agentDt);

                cmd = "Select * from `contacts_reports` WHERE `report` = '" + forceReportName + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    module = dt.Rows[0]["module"].ObjToString();
                    workModule = module;
                    string rec = dt.Rows[0]["record"].ObjToString();
                    cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + rec + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                        return;
                    force = true;

                    for ( int i=0; i<agentDt.Rows.Count; i++)
                    {
                        workAgent = agentDt.Rows[i]["agent"].ObjToString();
                        workSendTo = agentDt.Rows[i]["email"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( workSendTo) && String.IsNullOrWhiteSpace ( sendWhere ))
                            sendWhere = "Email";
                        runReport(dt, forceReportName, workAgent, workSendTo, sendWhere, LoginForm.username, "");
                    }
                }
                return;
            }

            if ( 1 == 1 )
            {
                runReport(dt, forceReportName, workAgent, workSendTo, sendWhere, LoginForm.username, "" );
                return;
            }
            DateTime date = DateTime.Now;
            int presentDay = date.Day;
            int dayToRun = 0;
            string status = "";
            string frequency = "";
            string startDOW = "";

            string agent = "";
            string report = "";
            string data = "";
            string manual = "";
            string email = "";
            string username = "";
            string displayFormat = "";
            DateTime lastRunDate = DateTime.Now;
            DateTime startRunDate = DateTime.Now;
            DataTable dx = null;

            DateTime now = DateTime.Now;
            string record = "";

            dt = LoadEmails(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                report = dt.Rows[i]["report"].ObjToString();
                if (force)
                    report = forceReportName;
                if (String.IsNullOrWhiteSpace(report))
                    return;

                agent = dt.Rows[i]["agent"].ObjToString();
                frequency = dt.Rows[i]["frequency"].ObjToString();
                email = dt.Rows[i]["email"].ObjToString();
                username = dt.Rows[i]["username"].ObjToString();

                manual = dt.Rows[i]["manual"].ObjToString().ToUpper();
                displayFormat = dt.Rows[i]["displayFormat"].ObjToString().ToUpper();

                status = dt.Rows[i]["status"].ObjToString().ToUpper();
                if (status == "OFF")
                    continue;

                startDOW = dt.Rows[i]["startDOW"].ObjToString();
                startRunDate = dt.Rows[i]["startRunDate"].ObjToDateTime();

                if ( startRunDate.Year > 1000 )
                {
                    if (now < startRunDate)
                        continue;
                }

                lastRunDate = dt.Rows[i]["lastRunDate"].ObjToDateTime();

                if (!String.IsNullOrWhiteSpace(frequency) && !String.IsNullOrWhiteSpace(startDOW))
                {
                    bool run = checkRunOrNot(frequency, lastRunDate, startDOW);
                    if (!run)
                        continue;
                }

                bool isCustom = false;
                if (manual != "Y")
                {
                    cmd = "Select * from `contacts_reports` WHERE `report` = '" + report + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return;
                    record = dx.Rows[0]["record"].ObjToString();
                    workModule = dx.Rows[0]["module"].ObjToString();

                    cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
                    dx = G1.get_db_data(cmd);

                    //cmd = ContactsPreneed.BuildReportQuery(workModule, dx, agent, ref isCustom);
                    //dx = G1.get_db_data(cmd);
                }
                else
                {
                    string query = dt.Rows[i]["data"].ObjToString();

                    dx = BuildQueryTable(query);
                }

                runReport(dx, report, agent, email, sendWhere , username, displayFormat );

                record = dt.Rows[i]["record"].ObjToString();

                G1.update_db_table("contacts_reports_data", "record", record, new string[] {"lastRunDate", DateTime.Now.ToString("yyyyMMdd") });

                if (force)
                    break;
            }
        }
        /****************************************************************************************/
        private bool checkRunOrNot ( string frequency, DateTime lastRunDate, string startDOW )
        {
            bool run = false;
            if (frequency.ToUpper() == "DAILY")
            {
                run = true;
                return run;
            }

            DateTime today = DateTime.Now;
            bool dowIsOkay = false;
            string dow = "";
            if ( startDOW.ToUpper() == "SUNDAY" && today.DayOfWeek == DayOfWeek.Sunday )
                dowIsOkay = true;
            else if (startDOW.ToUpper() == "MONDAY" && today.DayOfWeek == DayOfWeek.Monday)
                dowIsOkay = true;
            else if (startDOW.ToUpper() == "TUESDAY" && today.DayOfWeek == DayOfWeek.Tuesday)
                dowIsOkay = true;
            else if (startDOW.ToUpper() == "WEDNESDAY" && today.DayOfWeek == DayOfWeek.Wednesday )
                dowIsOkay = true;
            else if (startDOW.ToUpper() == "THURSDAY" && today.DayOfWeek == DayOfWeek.Thursday)
                dowIsOkay = true;
            else if (startDOW.ToUpper() == "FRIDAY" && today.DayOfWeek == DayOfWeek.Friday)
                dowIsOkay = true;
            else if (startDOW.ToUpper() == "SATURDAY" && today.DayOfWeek == DayOfWeek.Saturday)
                dowIsOkay = true;

            int months = 0;
            int days = 0;
            TimeSpan ts = today - today;
            if (lastRunDate.Year > 1000)
            {
                ts = today - lastRunDate;
                months = G1.GetMonthsBetween(today, lastRunDate );
                days = ts.Days;
            }
            if (frequency.ToUpper() == "WEEKLY")
            {
                if (dowIsOkay)
                {
                    var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today);
                    months = dateSpan.Months + (dateSpan.Years * 12);
                    if ( dateSpan.Days >= 7 )
                        run = true;
                }
            }
            else if (frequency.ToUpper() == "MONTHLY")
            {
                var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today );
                months = dateSpan.Months + (dateSpan.Years * 12);
                if (dowIsOkay && months >= 1)
                {
                    run = true;
                }
                else if ( startDOW.ToUpper() == "EXACT")
                {
                    int result = months % 1;
                    if ( result == 0 )
                        run = true;
                }
            }
            else if (frequency.ToUpper() == "QUARTERLY")
            {
                var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today);
                months = dateSpan.Months + (dateSpan.Years * 12);
                if (dowIsOkay && months >= 3)
                {
                    run = true;
                }
                else if ( startDOW.ToUpper() == "EXACT")
                {
                    if (dateSpan.Days == 0)
                    {
                        int result = months % 3;
                        if (result == 0)
                            run = true;
                    }
                }
            }
            else if (frequency.ToUpper() == "YEARLY")
            {
                var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today);
                months = dateSpan.Months + ( dateSpan.Years * 12 );
                if (dowIsOkay && months >= 12)
                {
                    run = true;
                }
                else if (startDOW.ToUpper() == "EXACT")
                {
                    if (dateSpan.Days == 0)
                    {
                        int result = months % 12;
                        if (result == 0)
                            run = true;
                    }
                }
            }
            else if (frequency.ToUpper() == "2 MONTHS")
            {
                var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today);
                months = dateSpan.Months + (dateSpan.Years * 12);
                if (dowIsOkay && months >= 2 )
                {
                    run = true;
                }
                else if (startDOW.ToUpper() == "EXACT")
                {
                    if (dateSpan.Days == 0)
                    {
                        int result = months % 2;
                        if (result == 0)
                            run = true;
                    }
                }
            }
            else if (frequency.ToUpper() == "4 MONTHS")
            {
                var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today);
                months = dateSpan.Months + (dateSpan.Years * 12);
                if (dowIsOkay && months >= 4 )
                {
                    run = true;
                }
                else if (startDOW.ToUpper() == "EXACT")
                {
                    if (dateSpan.Days == 0)
                    {
                        int result = months % 4;
                        if (result == 0)
                            run = true;
                    }
                }
            }
            else if (frequency.ToUpper() == "8 MONTHS")
            {
                var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today);
                months = dateSpan.Months + (dateSpan.Years * 12);
                if (dowIsOkay && months >= 8 )
                {
                    run = true;
                }
                else if (startDOW.ToUpper() == "EXACT")
                {
                    if (dateSpan.Days == 0)
                    {
                        int result = months % 8;
                        if (result == 0)
                            run = true;
                    }
                }
            }
            else if (frequency.ToUpper() == "10 MONTHS")
            {
                var dateSpan = DateTimeSpan.CompareDates(lastRunDate, today);
                months = dateSpan.Months + (dateSpan.Years * 12);
                if (dowIsOkay && months >= 10 )
                {
                    run = true;
                }
                else if (startDOW.ToUpper() == "EXACT")
                {
                    if (dateSpan.Days == 0)
                    {
                        int result = months % 10;
                        if (result == 0)
                            run = true;
                    }
                }
            }
            return run;
        }
        /****************************************************************************************/
        private void ContactReportsAgents_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            btnSaveData.Hide();
            btnRunReport.Hide();

            funModified = false;
            otherModified = false;

            RemoveTabPage("Report Information");
            chkReports.Hide();
            lblSelect.Hide();

            LoadData();

            LoadDisplayFormats();

            if (!G1.isAdminOrSuper())
                tabControl1.TabPages.Remove(tabTesting);

            gridMain6.ExpandAllGroups();
            gridMain6.RefreshEditor(true);
            gridMain6.RefreshData();
            dgv6.Refresh();
            gridMain6.Focus();
            dgv6.Focus();
        }
        /****************************************************************************************/
        private void LoadDisplayFormats ()
        {
            this.repositoryItemComboBox3.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = 'AgentPreneeds' AND `module` = 'Primary' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                repositoryItemComboBox3.Items.Add(name);
            }
        }
        /****************************************************************************************/
        private void RemoveTabPage(string tabName)
        {
            for (int i = (tabControl1.TabPages.Count - 1); i >= 0; i--)
            {
                TabPage tp = tabControl1.TabPages[i];
                if (tp.Text.ToUpper() == tabName.ToUpper())
                    tabControl1.TabPages.RemoveAt(i);
            }
        }
        /***********************************************************************************************/
        public DataTable saveMembersDt = null;
        public bool preprocessDone = false;

        private void LoadData()
        {
            //string cmd = "Select * from `contacts_reports` order by `order`;";
            //DataTable dt = G1.get_db_data(cmd);

            DataTable dt = workDt.Copy();
            dt.Columns.Add("num");
            dt.Columns.Add("mod");

            DataRow[] dRows = dt.Select("name='All'");
            if (dRows.Length > 0)
                dt.Rows.Remove(dRows[0]);

            if ( !String.IsNullOrWhiteSpace ( workAgent ))
            {
                dRows = dt.Select("name='" + workAgent + "'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();

            }

            dt = LoadEmails(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***************************************************************************************/
        private DataTable LoadEmails ( DataTable dt)
        {
            if (G1.get_column_number(dt, "email") < 0)
                dt.Columns.Add("email");
            if (G1.get_column_number(dt, "username") < 0)
                dt.Columns.Add("username");

            string cmd = "Select * from `users`;";
            DataTable userDt = G1.get_db_data(cmd);

            string fName = "";
            string lName = "";
            string agent = "";
            string email = "";
            string username = "";
            string[] Lines = null;
            DataRow[] dRows = null;

            if ( G1.get_column_number ( dt, "lastName") < 0 )
            {
                dt.Columns.Add("lastName");
                dt.Columns.Add("firstName");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    agent = dt.Rows[i]["agent"].ObjToString();
                    Lines = agent.Split(',');
                    if ( Lines.Length >= 2 )
                    {
                        dt.Rows[i]["lastName"] = Lines[0].Trim();
                        dt.Rows[i]["firstName"] = Lines[1].Trim();
                    }
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lName = dt.Rows[i]["lastName"].ObjToString();
                fName = dt.Rows[i]["firstName"].ObjToString();
                dRows = userDt.Select("firstName='" + fName + "' AND lastName='" + lName + "'");
                if (dRows.Length > 0)
                {
                    email = dRows[0]["email"].ObjToString();
                    dt.Rows[i]["email"] = email;

                    username = dRows[0]["username"].ObjToString();
                    dt.Rows[i]["username"] = username;
                }
            }

            return dt;
        }
        /***************************************************************************************/
        public void FireEventFunServicesSetModified()
        {
            funModified = true;
            this.btnSaveAll.Show();
            this.btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void gridMainDep_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            int order = 0;

            TabPage current = tabControl1.SelectedTab;
            if (current.Name.ToUpper() == "TABAGENT")
            {
                dt.AcceptChanges();
                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName;
                if ( currentColumn.ToUpper() == "EMAIL")
                {
                    string fName = dr["firstName"].ObjToString();
                    string lName = dr["lastName"].ObjToString();
                    string email = dr["email"].ObjToString();
                    string cmd = "Select * from `users` WHERE `lastName` = '" + lName + "' AND `firstname` = '" + fName + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        string record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("users", "record", record, new string[] { "email", email });
                    }
                    else
                    {
                        MessageBox.Show("Agent name not found in User Table!", "Agent/User Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        dr["email"] = "";
                        gridMain.RefreshEditor(true);
                    }
                }
            }

            funModified = true;
            //btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void gridMainDep_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
                return;
            }
        }
        /****************************************************************************************/
        private DevExpress.XtraGrid.GridControl GetCurrentDataGrid()
        {
            DevExpress.XtraGrid.GridControl currentDGV = null;
            TabPage current = tabControl1.SelectedTab;

            if (current.Name.ToUpper() == "TABAGENT")
            {
                if (dgv.Visible)
                    currentDGV = dgv;
            }
            else if (current.Name.ToUpper() == "TABDATA")
            {
                if (dgv6.Visible)
                    currentDGV = dgv6;
            }
            return currentDGV;
        }
        /****************************************************************************************/
        private DataTable GetCurrentDataTable()
        {
            DataTable dt = null;
            TabPage current = tabControl1.SelectedTab;

            if (current.Name.ToUpper() == "TABAGENT")
            {
                if (dgv.Visible)
                    dt = (DataTable)dgv.DataSource;
            }
            else if (current.Name.ToUpper() == "TABDATA")
            {
                if (dgv6.Visible)
                    dt = (DataTable)dgv6.DataSource;
            }
            return dt;
        }
        /****************************************************************************************/
        private DataRow GetCurrentDataRow()
        {
            DataRow dr = null;
            TabPage current = tabControl1.SelectedTab;

            if (current.Name.ToUpper() == "TABAGENT")
            {
                if (dgv.Visible)
                    dr = gridMain.GetFocusedDataRow();
            }
            else if (current.Name.ToUpper() == "TABDATA")
            {
                if (dgv6.Visible)
                    dr = gridMain6.GetFocusedDataRow();
            }
            return dr;
        }
        /****************************************************************************************/
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView GetCurrentGridView()
        {
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gv = null;
            TabPage current = tabControl1.SelectedTab;

            if (current.Name.ToUpper() == "TABAGENT")
            {
                if (dgv.Visible)
                    gv = gridMain;
            }
            else if (current.Name.ToUpper() == "TABDATA")
            {
                if (dgv6.Visible)
                    gv = gridMain6;
            }
            return gv;
        }
        /****************************************************************************************/
        private void gridMainDep_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = GetCurrentDataTable();
            if (dt == null)
                return;
            try
            {
                string delete = dt.Rows[row]["mod"].ObjToString();
                if (delete.ToUpper() == "D")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show("***ERROR*** Not Showing Deleted Members for Contract " + workContract + " Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void panelAll_Paint(object sender, PaintEventArgs e)
        {
        }
        /****************************************************************************************/
        private void panelFamilyTop_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelFamilyTop.Bounds;
            Graphics g = panelFamilyTop.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = this.panelAll.Width - 2;
            int high = rect.Height - 2;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /****************************************************************************************/
        private void panelBottom_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelBottom.Bounds;
            Graphics g = panelBottom.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width - 2;
            int high = rect.Height - 2;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string FamilyModifiedDone;
        protected void OnFamilyModified()
        {
            if (FamilyModifiedDone != null)
            {
                //                DataRow dr = gridMainDep.GetFocusedDataRow();
                FamilyModifiedDone.Invoke("YES");
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {

            funModified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private DataTable myDt = null;
        private string currentColumn = "";
        private string columnEdit = "";
        private int repositoryCount = 0;
        private string[] repositoryNames = new string[10];
        private string[] repositoryCaptions = new string[10];
        private RepositoryItemComboBox[] Repository = new RepositoryItemComboBox[10];
        /****************************************************************************************/
        private void gridMainDep_MouseDown(object sender, MouseEventArgs e)
        {
            //            var hitInfo = gridMainDep.CalcHitInfo(e.Location);
            var hitInfo = GetCurrentGridView().CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                GridColumn column = hitInfo.Column;
                currentColumn = column.FieldName.Trim();

                ContactsPreneed form = (ContactsPreneed)G1.IsFormOpen("ContactsPreneed");
                if (form != null)
                {
                    RepositoryItemComboBox itemBox = form.FireEventGrabSomething(columnEdit);
                    repositoryCaptions[repositoryCount] = column.FieldName;
                    repositoryNames[repositoryCount] = columnEdit;
                    Repository[repositoryCount] = itemBox;
                    repositoryCount++;
                }

            }
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            //this.Cursor = Cursors.WaitCursor;
            //funModified = false;
            //if (otherModified)
            //{
            //    DateTime srvDate = DateTime.MinValue;
            //    DataTable dt = (DataTable)dgv6.DataSource;
            //    DataRow[] dRows = dt.Select("dbfield='SRVDATE'");
            //    if (dRows.Length > 0)
            //        srvDate = dRows[0]["data"].ObjToDateTime();
            //    //FunFamily.SaveOtherData(workContract, dt, workFuneral);
            //    T1.SaveOtherData(workContract, dt, workFuneral);
            //    //SaveOtherData(workContract, dt, workFuneral);

            //    otherModified = false;
            //}
            //btnSaveAll.Hide();
            //this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        RepositoryItemComboBox ciLookup6 = new RepositoryItemComboBox();
        /***************************************************************************************/
        private string ProcessReference(DataRow[] dR, string field, int index = 0)
        {
            if (dR.Length <= 0)
                return "";
            string answer = "";
            if (String.IsNullOrWhiteSpace(field))
                return answer;
            try
            {
                string[] Lines = null;
                if (field.IndexOf("~") >= 0)
                {
                    Lines = field.Split('~');
                    if (Lines.Length <= 1)
                        return answer;
                    field = Lines[1];
                }

                if (field.IndexOf("+") < 0)
                    answer = dR[index][field].ObjToString();
                else
                {
                    Lines = field.Split('+');
                    string str = "";
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        field = Lines[j].Trim();
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(field))
                            {
                                str = dR[index][field].ObjToString();
                                answer += str + " ";
                            }
                        }
                        catch (Exception ex)
                        {
                            if (field == ",")
                                answer = answer.Trim();
                            answer += field;
                            if (field == ",")
                                answer += " ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            answer = answer.Trim();
            answer = answer.TrimStart(',');
            return answer;
        }
        /***************************************************************************************/
        private string ProcessReference(string field, string address, string city, string county, string state, string zip, string phone )
        {
            string answer = "";
            if (String.IsNullOrWhiteSpace(field))
                return answer;
            try
            {
                string[] Lines = null;
                string str = "";
                if (field.IndexOf("~") >= 0)
                {
                    Lines = field.Split('~');
                    if (Lines.Length <= 1)
                        return answer;
                    field = Lines[1];
                }

                if (field.IndexOf("+") < 0)
                {
                    //answer = dR[index][field].ObjToString();
                    //answer = field;
                    if (!String.IsNullOrWhiteSpace(field))
                    {
                        if (field.ToUpper().IndexOf("ADDRESS") >= 0)
                            answer = address;
                        else if (field.ToUpper().IndexOf("CITY") >= 0)
                            answer = city;
                        else if (field.ToUpper().IndexOf("COUNTY") >= 0)
                            answer = county;
                        else if (field.ToUpper().IndexOf("STATE") >= 0)
                            answer = state;
                        else if (field.ToUpper().IndexOf("ZIP") >= 0)
                            answer = zip;
                        else if (field.ToUpper().IndexOf("PHONE") >= 0)
                            answer = phone;
                    }
                }
                else
                {
                    Lines = field.Split('+');
                    str = "";
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        field = Lines[j].Trim();
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(field))
                            {
                                if (field.ToUpper().IndexOf("ADDRESS") >= 0)
                                {
                                    str = address;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("CITY") >= 0)
                                {
                                    str = city;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("COUNTY") >= 0)
                                {
                                    str = county;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("STATE") >= 0)
                                {
                                    str = state;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("ZIP") >= 0)
                                {
                                    str = zip;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("PHONE") >= 0)
                                {
                                    str = phone;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf(",") >= 0)
                                {
                                    answer = answer.Trim();
                                    if ( !String.IsNullOrWhiteSpace ( answer ))
                                        answer += field + " ";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (field == ",")
                                answer = answer.Trim();
                            answer += field;
                            if (field == ",")
                                answer += " ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return answer;
        }
        /****************************************************************************************/
        private bool specialLoading = false;
        private void gridMain6_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (specialLoading)
                return;
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            if ( currentColumn.ToUpper() == "REPORT")
            {
                string manual = dr["manual"].ObjToString().ToUpper();
                if ( manual != "Y" )
                {
                    MessageBox.Show("This is not a manual report.\nSo, the name cannot be edited!", "Editing Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    dr[currentColumn] = oldWhat;
                    gridMain6.RefreshEditor(true);
                    return;
                }
            }

            dr["mod"] = "Y";
            otherModified = true;
            funModified = true;

            string what = dr["data"].ObjToString();
            string report = dr["report"].ObjToString();

            what = G1.protect_data(what);

            DataTable dt6 = (DataTable)dgv6.DataSource;
            int rowHandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);
            string field = dt6.Rows[row]["field"].ObjToString();
            string record = dt6.Rows[row]["record"].ObjToString();
            string frequency = dt6.Rows[row]["frequency"].ObjToString();
            DateTime lastRunDate = dt6.Rows[row]["lastRunDate"].ObjToDateTime();
            DateTime startRunDate = dt6.Rows[row]["startRunDate"].ObjToDateTime();
            string startDOW = dt6.Rows[row]["startDOW"].ObjToString();
            string status = dt6.Rows[row]["status"].ObjToString();

            string displayFormat = dt6.Rows[row]["displayFormat"].ObjToString();

            G1.update_db_table("contacts_reports_data", "record", record, new string[] { "report", report, "data", what, "frequency", frequency, "lastRunDate", lastRunDate.ToString("yyyyMMdd"), "displayFormat", displayFormat, "startRunDate", startRunDate.ToString("yyyyMMdd"), "startDOW", startDOW, "status", status });
        }
        /****************************************************************************************/
        private string FixUsingFieldData(string field)
        {
            string newField = field;
            string cmd = "Select * from `tracking` where `tracking` = '" + field + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string useData = dx.Rows[0]["using"].ObjToString();
                if (!String.IsNullOrWhiteSpace(useData))
                    newField = useData;
            }
            return newField;
        }
        /***************************************************************************************/
        public bool trackChange = true;
        public string whichTab = "MAIN";
        public string mainTab = "";
        public int mainRow = 0;
        public string otherTab = "";
        public int otherRow = 0;
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int rowHandle = 0; // Ramma Zamma
            TabPage current = (sender as TabControl).SelectedTab;
            if (current == null)
                return;

            if (current.Name.ToUpper() == "TABAGENT")
            {
                gridMain.ClearSelection();
                gridMain.RefreshData();

                RemoveTabPage("Report Information");
            }
            else if (current.Name.ToUpper() == "TABDATA")
            {
                LoadReportData();

                gridMain6.ClearSelection();
                gridMain6.RefreshData();
            }
        }
        /****************************************************************************************/
        private void LoadReportData ()
        {
            //string record = getCurrentReportRecord();
            //if (String.IsNullOrWhiteSpace(record))
            //    return;
            string cmd = "Select * from `contacts_reports_data` where `agent` = '" + workAgent + "' AND `module` = '" + workModule + "';";
            DataTable dt = G1.get_db_data(cmd);

            //repositoryItemComboBox11.Items.Clear();

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    repositoryItemComboBox11.Items.Add(dt.Rows[i]["report"].ObjToString());
            //}

            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);

            dgv6.DataSource = dt;
            dgv6.Refresh();

            //ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
        }
        /***************************************************************************************/
        private void CiLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DevExpress.XtraEditors.ComboBoxEdit box = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            string newData = box.Text;

            dt.Rows[row]["data"] = newData;
        }
        /***********************************************************************************************/
        private bool isRepository(string field, ref DataTable dt)
        {
            bool gotit = false;
            DataRow dRow = null;
            string item = "";
            for (int i = 0; i < repositoryCount; i++)
            {
                if (field.ToUpper() == repositoryCaptions[i].Trim().ToUpper())
                {
                    gotit = true;
                    DevExpress.XtraEditors.Controls.ComboBoxItemCollection box = (DevExpress.XtraEditors.Controls.ComboBoxItemCollection)Repository[i].Items;
                    for (int j = 0; j < box.Count; j++)
                    {
                        item = box[j].ToString();
                        dRow = dt.NewRow();
                        dRow["stuff"] = item;
                        dt.Rows.Add(dRow);
                    }
                    break;
                }
            }
            return gotit;
        }
        /****************************************************************************************/
        void FunFamilyNew_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 39 )
            {
                e.KeyChar = '`';
                e.Handled = false;
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string record = dr["record"].ObjToString();
            workAgent = dr["name"].ObjToString();

            tabControl1.TabPages.Add(tabData);

            tabControl1.SelectedTab = tabData;
        }
        /****************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        { // Add New Report Data Row

            string report = SelectReport();
            if (String.IsNullOrWhiteSpace(report))
                return;

            int row = AddNewDataRow( report );

            int rowHandle = row - 1;
            gridMain6.FocusedRowHandle = rowHandle;
            gridMain6.SelectRow(rowHandle);
            if (gridMain6.VisibleColumns.Count > 0)
            {
                GridColumn firstColumn = gridMain6.Columns["field"];
                gridMain6.FocusedColumn = gridMain6.Columns[firstColumn.FieldName];
            }
            gridMain6.RefreshEditor(true);
            gridMain6.RefreshData();
            this.ForceRefresh();
        }
        /****************************************************************************************/
        private string selectedReport = "";
        private string SelectReport()
        {
            string report = "";
            selectedReport = "";
            string cmd = "Select * from `contacts_reports` WHERE `module` = '" + workModule + "' order by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);

            string lines = "Manual Report\n";
            //string lines = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                lines += dt.Rows[i]["report"].ObjToString() + "\n";

            using (SelectFromList listForm = new SelectFromList(lines, false))
            {
                listForm.Text = "Select Desired Report :";
                //listForm.ListDone += ListForm_PictureDone;
                listForm.ShowDialog();
                string what = SelectFromList.theseSelections;
                if (String.IsNullOrWhiteSpace(what))
                    return report;
                report = what;
            }
            return report;
        }
        ///***********************************************************************************************/
        //private void ViewForm_ManualDone(DataTable dd, DataRow dx)
        //{
        //    selectedReport = "";
        //    if (dx != null)
        //        selectedReport = dx["report"].ObjToString();
        //}
        /***********************************************************************************************/
        private string getCurrentReportRecord ()
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            return record;
        }
        /***********************************************************************************************/
        private int AddNewDataRow(string report = "" )
        {
            int row = -1;
            if (String.IsNullOrWhiteSpace(report))
                report = "New Report";
            try
            {
                DataTable dt = (DataTable)dgv6.DataSource;

                string reportRecord = getCurrentReportRecord();

                string record = G1.create_record("contacts_reports_data", "spare", "-1");
                if (G1.BadRecord("contacts_reports_data", record))
                    return row;

                string manual = "";
                if (report.ToUpper() == "MANUAL REPORT")
                    manual = "Y";

                DataRow dRow = dt.NewRow();
                dRow["num"] = (dt.Rows.Count + 1).ToString();
                dRow["record"] = record;
                dRow["module"] = workModule;
                //dRow["reportRecord"] = reportRecord;
                dRow["report"] = report;
                dRow["agent"] = workAgent;
                dRow["field"] = "";
                dRow["mod"] = "Y";
                dRow["manual"] = manual;
                dRow["order"] = dt.Rows.Count;

                dt.Rows.Add(dRow);

                G1.update_db_table("contacts_reports_data", "record", record, new string[] { "order", dt.Rows.Count.ToString(), "spare", "", "agent", workAgent, "report", report, "manual", manual, "module", workModule });

                row = dt.Rows.Count;
                dgv6.DataSource = dt;
                dgv6.Refresh();
                gridMainDep_CellValueChanged(null, null);
            }
            catch (Exception ex)
            {
            }
            return row;
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        { // Delete Report Data Row
            DataRow dr = gridMain6.GetFocusedDataRow();
            if (dr == null)
                return;

            string field = dr["report"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Report\n(" + field + ") ?", "Delete Report Row Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt == null)
                return;

            string record = dr["record"].ObjToString();

            G1.delete_db_table("contacts_reports_data", "record", record);

            try
            {
                gridMain6.DeleteRow(gridMain6.FocusedRowHandle);
                dt.Rows.Remove(dr);
                dt.AcceptChanges();
            }
            catch (Exception ex)
            {
            }

            G1.NumberDataTable(dt);
            dgv6.DataSource = dt;
            dgv6.RefreshDataSource();
            dgv6.Refresh();
        }
        /****************************************************************************************/
        private void btnSaveData_Click(object sender, EventArgs e)
        {
            if (1 == 1)
                return;
            DataTable dt = (DataTable)dgv6.DataSource;

            string record = "";
            string field = "";
            string data = "";
            string status = "";
            string help = "";
            string operand = "";
            string mod = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if ( mod.ToUpper() == "D")
                {
                    continue;
                }
                field = dt.Rows[i]["field"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString();
                help = dt.Rows[i]["help"].ObjToString();
                operand = dt.Rows[i]["operand"].ObjToString();
                G1.update_db_table("contacts_reports_data", "record", record, new string[] { "order", i.ToString(), "spare", "", "field", field, "data", data, "status", status, "help", help, "operand", operand });
            }

            btnSaveData.Hide();
        }
        /****************************************************************************************/
        private void runReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string record = getCurrentReportRecord();
            if (String.IsNullOrWhiteSpace(record))
            {
                gridMain_DoubleClick ( null, null );
                return;
            }

            string cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);

            runReport(dt, "" );
        }
        /****************************************************************************************/
        private void runReport ( DataTable dt, string report, string agent = "", string email = "", string sendWhere = "", string sendUsername = "", string displayFormat = "" )
        {
            string field = "";
            string data = "";
            string status = "";

            DataTable dx = null;
            string[] Lines = null;
            string operand = "";
            string body = "";
            int iBody = 0;
            DateTime date = DateTime.Now;
            DateTime today = DateTime.Now;

            DataTable workDt = null;

            bool isCustom = false;

            string cmd = ContactsPreneed.BuildReportQuery(workModule, dt, workAgent, ref isCustom);

            if (String.IsNullOrWhiteSpace(cmd))
                return;

            dx = G1.get_db_data(cmd);

            if ( !String.IsNullOrWhiteSpace ( agent) && agent.ToUpper() != "ALL")
            {
                DataRow[] dRows = dx.Select("agent='" + agent + "'");
                if (dRows.Length > 0)
                    dx = dRows.CopyToDataTable();
            }

            if ( dx != null )
            {
                if (autoRun && !String.IsNullOrWhiteSpace(sendWhere) && workSendTo.ToUpper() == "AGENT")
                {
                    AutoRunContacts(workModule, dx, dt, forceReportName, workAgent, sendWhere );
                    //G1.AddToAudit("System", "AutoRun", "Agent Contacts Report Ran", "RAN Agent Contacts Autorun . . . . . . . ", "");
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                int height = this.Height;

                DevExpress.XtraEditors.XtraForm form = null;
                if (workModule.ToUpper() == "CONTACTS")
                {
                    form = new Contacts();
                    if (!isCustom)
                    {
                        form = new Contacts(dx, dt, false, report, workAgent, sendWhere, workSendTo, autoRun );
                        //form.Show();
                    }

                    else
                        form = new Contacts(dx, dt, true, report, workAgent, sendWhere, workSendTo);
                }
                else
                {
                    form = new ContactsPreneed();
                    if (!isCustom)
                        form = new ContactsPreneed(dx, report );
                    else
                        form = new ContactsPreneed(dx, autoRun, dt, true, report, workAgent, sendWhere, workSendTo );
                }

                //if ( autoRun )
                //{
                //    if ( !String.IsNullOrWhiteSpace ( sendWhere ) && !String.IsNullOrWhiteSpace ( workSendTo ))
                //    {
                //        return;
                //    }
                //}

                //ContactsPreneed form = new ContactsPreneed( dx, autoRun, agent, email, report, sendWhere, sendUsername, displayFormat, isCustom, dt );
                form.Text = report;
                //leadForm.StartPosition = FormStartPosition.CenterParent;
                form.Show();

                if ( autoRun )
                    G1.AddToAudit("System", "AutoRun", "Agent Contacts Report Ran", "RAN Agent Contacts Autorun . . . . . . . ", "");

                //form.Anchor = AnchorStyles.None;

                form.AutoSize = true; //this causes the form to grow only. Don't set it if you want to resize automatically using AnchorStyles, as I did below.
                form.FormBorderStyle = FormBorderStyle.Sizable; //I think this is not necessary to solve the problem, but I have left it there just in case :-)
                form.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                    | System.Windows.Forms.AnchorStyles.Left)
                                    | System.Windows.Forms.AnchorStyles.Right)));

                //form.Show();
                form.Location = new Point(100, 100);
                form.Height = height + 100;
                form.Refresh();

                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void AutoRunContacts ( string workModule, DataTable dx, DataTable dt, string report, string workAgent, string sendWhere )
        {
            DataTable groupDt = G1.GetGroupBy(dx, "agent");
            if (groupDt.Rows.Count <= 0)
                return;
            DataRow[] dRows = null;

            workAgent = "Walker, Michael";
            workAgent = "Muse, Vernon";
            if (!String.IsNullOrWhiteSpace(workAgent))
            {
                dRows = groupDt.Select("agent='" + workAgent + "'");
                if (dRows.Length <= 0)
                    return;
                groupDt = dRows.CopyToDataTable();
            }

            groupDt = LoadEmails(groupDt);
            DataTable agentDt = null;

            string agent = "";
            string email = "";
            for ( int i=0; i<groupDt.Rows.Count; i++)
            {
                agent = groupDt.Rows[i]["agent"].ObjToString();
                email = groupDt.Rows[i]["email"].ObjToString();

                dRows = dx.Select("agent='" + agent + "'");
                if (dRows.Length <= 0)
                    continue;
                agentDt = dRows.CopyToDataTable();

                if (String.IsNullOrWhiteSpace(email))
                    email = "robbyxyzzy@gmail.com";

                if ( workModule.ToUpper() == "CONTACTS")
                {
                    Contacts form = new Contacts(agentDt, true, agent, email, report, sendWhere, LoginForm.username, "", true, dt);
                    form.Show();
                }
                else if (workModule.ToUpper() == "CONTACTS PRENEED")
                {
                    ContactsPreneed form = new ContactsPreneed (agentDt, true, agent, email, report, sendWhere, LoginForm.username, "", true, dt);
                    form.Show();
                }
            }
        }
        /****************************************************************************************/
        private void ContactReports_FormClosing(object sender, FormClosingEventArgs e)
        {
            TabPage current = tabControl1.SelectedTab;
            if (current.Name.ToUpper() == "TABDATA")
            {
                tabControl1.SelectedTab = tabAgent;
                e.Cancel = true;
                return;
            }
        }
        /****************************************************************************************/
        private void btnRunReport_Click(object sender, EventArgs e)
        {
            runReportToolStripMenuItem_Click ( null, null );
        }
        /****************************************************************************************/
        private void gridMain6_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain6.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = null;
            string report = dr["report"].ObjToString();
            string manual = dr["manual"].ObjToString().ToUpper();
            string displayFormat = dr["displayFormat"].ObjToString().ToUpper();
            if ( manual != "Y" )
            {
                if (String.IsNullOrWhiteSpace(report))
                    return;

                string cmd = "Select * from `contacts_reports` WHERE `report` = '" + report + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;
                string record = dt.Rows[0]["record"].ObjToString();

                cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
                dt = G1.get_db_data(cmd);
            }
            else
            {
                string query = dr["data"].ObjToString();

                dt = BuildQueryTable(query);
            }

            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("This report has no parameters to run!", "Empty Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            runReport(dt, report, "", "", "", "", displayFormat );
        }
        /****************************************************************************************/
        private DataTable BuildQueryTable ( string query )
        {
            string[] Lines = query.Split('~');

            string field = "";
            string data = "";
            string operand = "";
            string help = "";

            string[] moreLines = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("status");
            dt.Columns.Add("field");
            dt.Columns.Add("operand");
            dt.Columns.Add("data");
            dt.Columns.Add("help");
            dt.Columns.Add("mod");
            dt.Columns.Add("order", Type.GetType("System.Int32"));

            DataRow dRow = null;

            for (int i = 0; i < Lines.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(Lines[i]))
                    continue;

                moreLines = Lines[i].Split('#');
                if (moreLines.Length < 2)
                    continue;
                help = moreLines[1].Trim();

                moreLines = moreLines[0].Split(' ');
                if (moreLines.Length < 3)
                    continue;

                field = moreLines[0].Trim();
                operand = moreLines[1].Trim();
                //data = moreLines[2].Trim();

                data = "";
                for (int k = 2; k < moreLines.Length; k++)
                {
                    if (String.IsNullOrWhiteSpace(moreLines[k]))
                        continue;
                    data += moreLines[k].Trim() + " ";
                }

                data = data.TrimEnd(' ');

                dRow = dt.NewRow();
                dRow["field"] = field;
                dRow["operand"] = operand;
                dRow["data"] = data;
                dRow["help"] = help;
                dt.Rows.Add(dRow);
            }

            return dt;
        }
        /****************************************************************************************/
        private string lastField = "";
        /****************************************************************************************/
        private void gridMain6_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            int focusedRow = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(focusedRow);

            DataRow dr = gridMain6.GetFocusedDataRow();
            string field = dr["field"].ObjToString();

            if (currentColumn.ToUpper() != "DATA" && currentColumn.ToUpper() != "FIELD")
            {
                if (currentColumn.ToUpper() == "OPERAND" || currentColumn.ToUpper() == "DISPLAYFORMAT" )
                    return;
                if (currentColumn.ToUpper() == "STARTDOW")
                    return;
                if (currentColumn.ToUpper() == "STATUS")
                    return;
                if (currentColumn.ToUpper() == "FREQUENCY")
                    return;
                currCol.ColumnEdit = null;
                return;
            }

            if (field == lastField)
                return;

            lastField = field;

            ciLookup6.Items.Clear();
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            myDt.Rows.Clear();

            ContactsPreneed form = (ContactsPreneed)G1.IsFormOpen("ContactsPreneed");
            RepositoryItemComboBox itemBox = form.FireEventGrabNewSomething(field);
            if (itemBox != null)
            {
                for (int i = 0; i < itemBox.Items.Count; i++)
                {
                    string str = itemBox.Items[i].ToString();
                    ciLookup6.Items.Add(str);
                }
                currCol.ColumnEdit = ciLookup6;
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private void gridMain6_ShownEditorx(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            int focusedRow = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(focusedRow);

            DataRow dr = gridMain6.GetFocusedDataRow();
            string field = dr["field"].ObjToString();

            oldWhat = dr[currentColumn].ObjToString();

            if (currentColumn.ToUpper() != "DATA")
                return;

            ciLookup6.Items.Clear();
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            myDt.Rows.Clear();

            isRepository(field, ref myDt);

            if (myDt.Rows.Count > 0)
            {
                for (int i = 0; i < myDt.Rows.Count; i++)
                    ciLookup6.Items.Add(myDt.Rows[i]["stuff"].ObjToString());
                currCol.ColumnEdit = ciLookup6;
            }
            else
            {
                currCol.ColumnEdit = null;
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void btnEdit_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain6.GetFocusedDataRow();
            if (dr == null)
                return;

            string manual = dr["manual"].ObjToString().ToUpper();
            if ( manual != "Y" )
            {
                MessageBox.Show("This is not a manual report.\nSo, parameters cannot be added!", "Parm Editing Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string query = dr["data"].ObjToString();

            DataTable dt = BuildQueryTable(query);

            ContactEditParms parmsForm = new ContactEditParms(workAgent, workGV, dt, workModule );
            parmsForm.contactParmsDone += ParmsForm_contactParmsDone;
            parmsForm.ShowDialog();
        }
        /****************************************************************************************/
        private void ParmsForm_contactParmsDone( string parms )
        {
            DataRow dr = gridMain6.GetFocusedDataRow();
            if (dr == null)
                return;

            if (parms.ToUpper().IndexOf("NEW FIELD") >= 0)
                return;

            DataTable dt = (DataTable)dgv6.DataSource;

            dr["data"] = parms;

            string record = dr["record"].ObjToString();

            G1.update_db_table("contacts_reports_data", "record", record, new string[] { "data", parms });

            gridMain6.RefreshData();
            gridMain6.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain6_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            string colName = e.Column.FieldName.ToUpper();
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
                return;
            }

            if ( (colName == "DATA" || colName == "HELP" ) && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string newData = "";
                string newHelp = "";
                bool gotHelp = false;
                string str = "";
                string c = "";

                int row = e.ListSourceRowIndex;
                int rowHandle = gridMain6.FocusedRowHandle;
                DataRow dR = gridMain6.GetDataRow(e.ListSourceRowIndex);
                if (dR == null)
                    return;

                string data = e.DisplayText.Trim();
                if (colName == "HELP")
                    data = dR["DATA"].ObjToString();

                if (String.IsNullOrWhiteSpace(data))
                    return;


                for ( int i=0; i<data.Length; i++)
                {
                    c = data.Substring(i, 1);
                    if ( c == "#")
                    {
                        gotHelp = true;
                        if (!String.IsNullOrWhiteSpace(newHelp))
                            newHelp += " & ";
                        continue;
                    }
                    if ( gotHelp )
                    {
                        if ( c == "~")
                        {
                            gotHelp = false;
                            newData += " & ";
                            continue;
                        }
                        newHelp += c;
                        continue;
                    }
                    newData += c;
                }
                if (colName == "HELP")
                    e.DisplayText = newHelp;
                else
                    e.DisplayText = newData;
            }
        }
        /****************************************************************************************/
        private void gridMain6_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "LASTRUNDATE")
            {
                DataTable dt = (DataTable)dgv6.DataSource;
                DataRow dr = gridMain6.GetFocusedDataRow();
                int rowhandle = gridMain6.FocusedRowHandle;
                int row = gridMain6.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["lastRunDate"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                dt.Rows[row]["mod"] = "Y";
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "STARTRUNDATE")
            {
                DataTable dt = (DataTable)dgv6.DataSource;
                DataRow dr = gridMain6.GetFocusedDataRow();
                int rowhandle = gridMain6.FocusedRowHandle;
                int row = gridMain6.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["startRunDate"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                dt.Rows[row]["mod"] = "Y";
            }
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            txtReult.Text = "";
            txtReult.Refresh();

            string startDOW = cmbDOW.Text;
            string frequency = cmbFrequency.Text;
            DateTime lastRunDate = textLRR.Text.ObjToDateTime();
            string lrd = lastRunDate.ToString("MM/dd/yyyy");
            textLRR.Text = lrd;
            textLRR.Refresh();

            bool run = checkRunOrNot(frequency, lastRunDate, startDOW);
            if (run)
                txtReult.Text = "RUN";
            else
                txtReult.Text = "BAD";
            txtReult.Refresh();
        }
        /****************************************************************************************/
    }
}