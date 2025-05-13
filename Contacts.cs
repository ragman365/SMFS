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
using System.IO;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using System.Diagnostics.Contracts;
using DevExpress.XtraGrid.Columns;
using System.Configuration;
using DevExpress.XtraPrinting;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Export;
using System.Security.AccessControl;
using System.Security.Principal;
using DevExpress.XtraGrid.Views.BandedGrid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Contacts : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool foundLocalPreference = false;
        private bool superuser = false;
        private bool showAgent = true;
        private bool modified = false;
        private string primaryName = "";
        private bool funeralsDone = false;
        private bool initialLoad = true;
        private DataTable workDt = null;
        private bool workAuto = false;
        private string workAgent = "";
        private string workEmail = "";
        private string workReport = "";
        private string sendWhere = "";
        private string sendUsername = "";
        private string workFormat = "";
        private DataTable customDt = null;

        DataTable allType = null;
        PleaseWait waitForm = null;
        private bool isCustom = false;
        private string customReport = "";
        private string workSendWhere = "";
        private string workSendTo = "";

        private DataTable _contactsDt = null;
        public DataTable ContactsAnswer { get { return _contactsDt; } }
        /****************************************************************************************/
        public Contacts()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public Contacts(DataTable dt, string Report = "" )
        {
            InitializeComponent();
            workDt = dt;
            workReport = Report;
        }
        /****************************************************************************************/
        public Contacts(DataTable dt, DataTable dx, bool custom = false, string Report = "", string agent = "", string sendWhere = "", string sendTo = "" )
        {
            InitializeComponent();
            workDt = dt;
            customDt = dx;
            isCustom = custom;
            workAgent = agent;
            customReport = Report;
            workSendWhere = sendWhere;
            workSendTo = sendTo;
        }
        /****************************************************************************************/
        public Contacts(DataTable dt, bool auto, string agent, string email, string report, string send, string username, string displayFormat, bool custom, DataTable dx)
        {
            InitializeComponent();
            workDt = dt;
            workAuto = auto;
            workAgent = agent;
            workEmail = email;
            workReport = report;
            sendWhere = send;
            sendUsername = username;
            workFormat = displayFormat;
            isCustom = custom;
            customDt = dx;
            customReport = report;
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox12, "Add New Contact");
            tt.SetToolTip(this.pictureBox11, "Remove Contact");
        }
        /****************************************************************************************/
        private void Contacts_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            SetupToolTips();

            cmbLocation.Hide();
            cmbContractType.Hide();

            if (!String.IsNullOrWhiteSpace(workReport))
                this.Text = "Report : " + workReport;
            else if (!String.IsNullOrWhiteSpace(customReport))
                this.Text = "Report : " + customReport;

            if (!string.IsNullOrWhiteSpace(workAgent))
                this.Text += " for " + workAgent;

            loading = true;

            string preference = G1.getPreference(LoginForm.username, "Agent Preneeds", "Allow SuperUser Access");
            if (preference.ToUpper() == "YES")
                superuser = true;

            string saveName = "AgentContacts Primary";
            string skinName = "";

            if (!String.IsNullOrWhiteSpace(workFormat))
                SetupSelectedColumns("AgentContacts", workFormat, dgv);
            else
                SetupSelectedColumns("AgentContacts", "Primary", dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                //if (skinName != "DevExpress Style")
                //    skinForm_SkinSelected("Skin : " + skinName);
            }

            //RemoveResults();

            if (String.IsNullOrWhiteSpace(workFormat))
                workFormat = "Primary";
            loadGroupCombo(cmbSelectColumns, "AgentContacts", workFormat);
            cmbSelectColumns.Text = workFormat;


            DateTime now = DateTime.Now;
//            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            if (!G1.isAdmin() && !superuser && !G1.RobbyServer)
            {
                //assignNewAgentToolStripMenuItem.Dispose();
                showAgent = false;
            }

            LoadContactTypes();
            LoadEmployees();
            LoadLocations();
            LoadReports();

            loading = false;

            LoadData();

            if (isCustom && customDt != null)
            {
                string field = "";
                string caption = "";
                string operand = "";
                ClearAllPositions(gridMain);
                int j = 0;
                int width = 0;
                int newWidth = 0;
                string firstName = "";
                string lastName = "";
                string middleName = "";
                string prefix = "";
                string suffux = "";
                string str = "";
                string parm = "";
                string newField = "";
                DateTime date = DateTime.Now;
                string dow = "";
                DataTable dx = (DataTable)dgv.DataSource;
                G1.SetColumnPosition(gridMain, "num", ++j, 50);
                for (int i = 0; i < customDt.Rows.Count; i++)
                {
                    try
                    {
                        field = customDt.Rows[i]["field"].ObjToString();
                        if (field.ToUpper() == "{CUSTOM}")
                            continue;
                        operand = customDt.Rows[i]["operand"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(operand))
                            continue;
                        if (G1.get_column_number(gridMain, field) < 0)
                        {
                            if (field.ToUpper() == "NAME")
                            {
                                newWidth = gridMain.Columns["lastName"].Width;
                                width = gridMain.Columns["firstName"].Width;
                                newWidth += width;
                                if (newWidth <= 0)
                                    newWidth = 60;
                                if (G1.get_column_number(gridMain, field) < 0)
                                    G1.AddNewColumn(gridMain, field, field, "", FormatType.None, newWidth, true);
                                gridMain.Columns[field].Width = newWidth;

                                dx.Columns.Add(field);
                                for (int k = 0; k < dx.Rows.Count; k++)
                                {
                                    firstName = dx.Rows[k]["firstName"].ObjToString();
                                    lastName = dx.Rows[k]["lastName"].ObjToString();
                                    firstName += " " + lastName;
                                    dx.Rows[k]["name"] = firstName;
                                }
                                width = gridMain.Columns[field].Width;
                                gridMain.Columns[field].OptionsColumn.FixedWidth = true;
                                G1.SetColumnPosition(gridMain, field, ++j, width);
                            }
                            else if (field.IndexOf("{") > 0)
                            {
                                decodeSpecialParm(field, ref newField, ref parm);
                                if (!String.IsNullOrWhiteSpace(parm))
                                {
                                    width = 50;
                                    if (parm.ToUpper() == "DOW")
                                        width = 100;
                                    if (G1.get_column_number(gridMain, parm) < 0)
                                        G1.AddNewColumn(gridMain, parm, parm, "", FormatType.None, width, true);

                                    dx.Columns.Add(parm);
                                    for (int k = 0; k < dx.Rows.Count; k++)
                                    {
                                        if (parm.ToUpper() == "DOW")
                                        {
                                            firstName = dx.Rows[k][newField].ObjToString();
                                            if (G1.validate_date(firstName))
                                            {
                                                date = firstName.ObjToDateTime();
                                                dow = G1.DayOfWeekText(date);
                                                dx.Rows[k][parm] = dow;
                                            }
                                        }
                                    }
                                    gridMain.Columns[parm].OptionsColumn.FixedWidth = true;
                                    G1.SetColumnPosition(gridMain, parm, ++j, width);
                                }
                            }
                        }
                        else
                            G1.SetColumnPosition(gridMain, field, ++j, width);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                if (customDt != null)
                {
                    DataView tempview = dx.DefaultView;
                    tempview.Sort = "apptDate asc, color asc";
                    dx = tempview.ToTable();
                }
                dgv.DataSource = dx;
            }

            if (!String.IsNullOrWhiteSpace(customReport) || !String.IsNullOrWhiteSpace(workReport))
            {
                this.panelClaimTop.Hide();
                screenOptionsToolStripMenuItem.Dispose();
                miscToolStripMenuItem.Dispose();
            }

            if (workAuto )
            {
                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }

            modified = false;
            loading = false;
        }
        /***********************************************************************************************/
        private bool decodeSpecialParm(string field, ref string newField, ref string parm)
        {
            bool rv = true;
            newField = "";
            parm = "";
            try
            {
                int idx = field.IndexOf("{");
                if (idx > 0)
                {
                    newField = field.Substring(0, idx);
                    newField = newField.Replace("{", "").Trim();
                    parm = field.Substring(idx);
                    parm = parm.Replace("{", "");
                    parm = parm.Replace("}", "").Trim();
                }
            }
            catch (Exception ex)
            {
                rv = false;
            }
            return rv;
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = (AdvBandedGridView)gridMain;
            string name = "";
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                name = gMain.Columns[i].Name.ToUpper();
                if (name != "NUM")
                    gMain.Columns[i].Visible = false;
                else
                    gMain.Columns[i].Visible = true;
                gridMain.Columns[i].OptionsColumn.FixedWidth = true;
            }
        }
        /***********************************************************************************************/
        private bool SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return false;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "PreNeed";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    if (G1.get_column_number((GridView)dgv.MainView, name) >= 0)
                        ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
            return true;
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DataRow[] dRows = dt.Select("name='Primary'");
                if (dRows.Length <= 0)
                    cmb.Items.Add("Primary");
            }
            else
                cmb.Items.Add("Primary");
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
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void LoadData(int rowHandle = -1, string nextRecord = "" )
        {
            if (loading)
                return;

            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            string date2 = G1.DateTimeToSQLDateTime(date);

            string employee = cmbEmployee.Text.Trim();

            string record = "";
            string oldRecord = "";

            string cmd = "Select * from `contacts` WHERE `apptDate` >= '" + date1 + "' AND `apptDate` <= '" + date2 + "' ";
            if (!String.IsNullOrWhiteSpace(employee) && employee.ToUpper() != "ALL" )
                cmd += " AND `agent` = '" + employee + "' ";
            cmd += " ORDER BY `apptDate` desc ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            if ( workDt != null )
            {
                if (workDt.Rows.Count > 0)
                    dt = workDt.Copy();
            }
            AddMod(dt, gridMain);

            SetupCompleted ( dt );

            GetFunerals( dt );

            dt = SetupGreenAndRed(dt);

            dgv.DataSource = dt;

            if (!String.IsNullOrWhiteSpace(nextRecord))
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["oldRecord"].ObjToString();
                    if (record == nextRecord)
                    {
                        rowHandle = i;
                        break;
                    }
                }
            }

            if (rowHandle != -1)
            {
                gridMain.FocusedRowHandle = rowHandle;
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private int DetermineDays ( DateTime serviceDate, string interval, int frequency )
        {
            int days = 2;
            int day = 1;
            int year = 0;
            int month = 0;

            if (interval == "Days")
                days = frequency;
            else if (interval == "Weeks")
                days = frequency * 7;
            else if (interval == "Months")
            {
                day = serviceDate.Day;
                year = serviceDate.Year;
                month = serviceDate.Month;
                month = month + frequency;
                if (month > 12)
                {
                    int years = month / 12;
                    year = year + years;
                    month = month % 12;
                    if (month <= 0)
                        month = 1;
                }
                DateTime nextDate = new DateTime(year, month, day);
                TimeSpan ts = nextDate - serviceDate;
                days = ts.Days;
            }
            return days;
        }
        /***********************************************************************************************/
        private DataTable GetFunerals ( DataTable dt )
        {
            if (funeralsDone)
                return dt;

            PleaseWait pleaseForm = G1.StartWait("Please Wait, Loading Past Funerals!");

            string cmd = "Delete from `contacts` where `contactName` = '-1';";
            G1.get_db_data(cmd);

            cmd = "select * from `contacttypes` WHERE `scheduledTask` <> '' AND `from` = 'Funeral' ORDER by `frequency`;";
            DataTable contactDt = G1.get_db_data(cmd);

            int frequency = 2;
            string interval = "Days";
            string scheduledTask = "2 Day Follow-Up";

            if ( contactDt.Rows.Count > 0 )
            {
                frequency = contactDt.Rows[0]["frequency"].ObjToInt32();
                interval = contactDt.Rows[0]["interval"].ObjToString();
                scheduledTask = contactDt.Rows[0]["scheduledTask"].ObjToString();
            }


            string date1 = DateTime.Now.ToString("yyyy-MM-dd");
            string date2 = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");

            cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` LEFT JOIN `relatives` q ON e.`contractNumber` = q.`contractNumber` WHERE e.`ServiceID` <> '' ";
            cmd += " AND p.`deceasedDate` >= '" + date2 + "' AND p.`deceasedDate` <= '" + date1 + "' ";
            cmd += " AND q.`depRelationship` = 'CLERGY' ";
            cmd += ";";

            DataTable dx = G1.get_db_data(cmd);

            DateTime date = DateTime.Now;
            string date3 = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd");
            cmd = "Select * from `contacts` WHERE `apptDate` >= '" + date2 + "' AND `apptDate` <= '" + date3 + "';";
            DataTable conDt = G1.get_db_data(cmd);
            conDt.Columns.Add("NewDate");
            for ( int i=0; i<conDt.Rows.Count; i++)
            {
                date = conDt.Rows[i]["apptDate"].ObjToDateTime();
                conDt.Rows[i]["NewDate"] = date.ToString("yyyy-MM-dd");
            }

            string director = "";
            string serviceId = "";
            string contractNumber = "";
            DateTime serviceDate = DateTime.Now;
            string fullName = "";
            DataRow[] dRows = null;
            DataRow dRow = null;
            int idx = 0;
            string license = "";
            string prefix = "";
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string suffix = "";
            string phone = "";
            string primaryPhone = "";
            string email = "";
            string primaryEmail = "";
            string apptDate = "";
            string dec = "";


            string employee = cmbEmployee.Text.Trim();

            DataTable newDt = dt.Clone();
            DataTable trackDt = null;

            cmd = "Select * from `directors`;";
            DataTable dirDt = G1.get_db_data(cmd);

            cmd = "Select * from `agents`;";
            DataTable agentDt = G1.get_db_data(cmd);

            int lastRow = dx.Rows.Count;
            //lastRow = 2;
            string record = "";
            int days = 2;
            int month = 1;
            int year = 0;

            try
            {
                for (int i = 0; i < lastRow; i++)
                {
                    director = dx.Rows[i]["Funeral Director"].ObjToString();
                    idx = director.IndexOf("[");
                    if (idx > 0)
                    {
                        license = director.Substring(idx);
                        license = license.Replace("[", "");
                        license = license.Replace("]", "").Trim();

                        director = director.Substring(0, idx - 1);

                        dRows = dirDt.Select("license='" + license + "'");
                        if (dRows.Length > 0)
                        {
                            lastName = dRows[0]["lastName"].ObjToString();
                            firstName = dRows[0]["firstName"].ObjToString();
                            middleName = dRows[0]["middleName"].ObjToString();

                            dRows = agentDt.Select("firstName='" + firstName + "' AND lastName='" + lastName + "'");
                            if (dRows.Length > 0)
                                director = lastName + ", " + firstName;
                            else
                            {
                                dRows = agentDt.Select("firstName='" + middleName + "' AND lastName='" + lastName + "'");
                                if (dRows.Length > 0)
                                    director = lastName + ", " + middleName;
                                else
                                    director = lastName + ", " + firstName;
                            }
                        }
                    }

                    if (employee.ToUpper() != "ALL")
                    {
                        if (employee != director)
                            continue;
                    }
                    serviceId = dx.Rows[i]["serviceId"].ObjToString();
                    serviceDate = dx.Rows[i]["serviceDate"].ObjToDateTime();
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();

                    prefix = dx.Rows[i]["depPrefix"].ObjToString();
                    suffix = dx.Rows[i]["depSuffix"].ObjToString();
                    firstName = dx.Rows[i]["depFirstName"].ObjToString();
                    lastName = dx.Rows[i]["depLastName"].ObjToString();
                    middleName = dx.Rows[i]["depMI"].ObjToString();

                    phone = "";
                    email = "";

                    cmd = "Select * from `track` where `depPrefix` = '" + prefix + "' AND `depFirstName` = '" + firstName + "' AND `depMI` = '" + middleName + "' AND `depLastName` = '" + lastName + "' AND `depSuffix` = '" + suffix + "';";
                    trackDt = G1.get_db_data(cmd);
                    if (trackDt.Rows.Count > 0)
                    {
                        phone = trackDt.Rows[0]["phone"].ObjToString();
                        email = trackDt.Rows[0]["email"].ObjToString();
                    }


                    fullName = dx.Rows[i]["fullName"].ObjToString();
                    days = DetermineDays(serviceDate, interval, frequency);
                    apptDate = serviceDate.AddDays(days).ToString("yyyy-MM-dd");

                    dRows = conDt.Select("serviceId='" + serviceId + "' AND contactName='" + fullName + "' AND agent='" + director + "' AND NewDate='" + apptDate + "'");
                    if (dRows.Length <= 0)
                    {
                        record = G1.create_record("contacts", "contactName", "-1");
                        if (G1.BadRecord("contacts", record))
                            break;
                        dRow = newDt.NewRow();
                        dRow["record"] = record;
                        dRow["contactName"] = fullName;
                        dRow["contactType"] = "Clergy";
                        dRow["serviceId"] = serviceId;
                        dRow["apptDate"] = G1.DTtoMySQLDT(serviceDate.AddDays(2));
                        dRow["agent"] = director;
                        dRow["completed"] = "0";
                        dRow["serviceId"] = serviceId;
                        dRow["scheduledTask"] = scheduledTask;

                        primaryPhone = dx.Rows[i]["phone"].ObjToString();
                        if (String.IsNullOrWhiteSpace(primaryPhone))
                            primaryPhone = phone;
                        dRow["primaryPhone"] = phone;

                        primaryEmail = dx.Rows[i]["email"].ObjToString();
                        if (String.IsNullOrWhiteSpace(primaryEmail))
                            primaryEmail = email;
                        dRow["email"] = primaryEmail;


                        prefix = dx.Rows[i]["prefix"].ObjToString();
                        firstName = dx.Rows[i]["firstName"].ObjToString();
                        lastName = dx.Rows[i]["lastName"].ObjToString();
                        middleName = dx.Rows[i]["middleName"].ObjToString();
                        suffix = dx.Rows[i]["suffix"].ObjToString();

                        dRow["refDeceasedPrefix"] = prefix;
                        dRow["refDeceasedFirstName"] = firstName;
                        dRow["refDeceasedLastName"] = lastName;
                        dRow["refDeceasedMiddleName"] = middleName;
                        dRow["refDeceasedSuffix"] = suffix;

                        newDt.Rows.Add(dRow);

                        G1.update_db_table("contacts", "record", record, new string[] { "agent", director, "apptDate", apptDate, "contactType", "Clergy", "contactName", fullName, "serviceId", serviceId, "primaryPhone", phone, "email", primaryEmail, "scheduledTask", scheduledTask });
                        G1.update_db_table("contacts", "record", record, new string[] { "refDeceasedPrefix", prefix, "refDeceasedFirstName", firstName, "refDeceasedLastName", lastName, "refDeceasedMiddleName", middleName, "refDeceasedSuffix", suffix, "completed", "0" });
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            if (newDt.Rows.Count > 0)
                dt.Merge(newDt);

            dt = FollowUpLastContact(dt, dirDt, agentDt );

            G1.StopWait(ref pleaseForm);
            pleaseForm = null;

            if (newDt.Rows.Count > 0)
            {
                waitForm = G1.StartWait("Loaded " + newDt.Rows.Count + " Past Funerals!");
                G1.sleep(1000);
                ClearWaitMessage();
            }

            funeralsDone = true;
            return dt;
        }
        /***********************************************************************************************/
        private DataTable FollowUpLastContact ( DataTable dt, DataTable dirDt, DataTable agentDt )
        {
            string cmd = "select * from `contacttypes` WHERE `scheduledTask` <> '' AND `from` = 'Last Contact' ORDER by `frequency`;";
            DataTable contactDt = G1.get_db_data(cmd);

            string contactType = "Clergy";
            int frequency = 2;
            string interval = "Days";
            string scheduledTask = "Follow-Up";

            if (contactDt.Rows.Count > 0)
            {
                contactType = contactDt.Rows[0]["contactType"].ObjToString();
                frequency = contactDt.Rows[0]["frequency"].ObjToInt32();
                interval = contactDt.Rows[0]["interval"].ObjToString();
                scheduledTask = contactDt.Rows[0]["scheduledTask"].ObjToString();
            }

            string employee = cmbEmployee.Text.Trim();

            DataTable newDt = dt.Clone();
            DataTable trackDt = null;

            string record = "";
            int days = 2;
            int month = 1;
            int year = 0;

            string date1 = DateTime.Now.ToString("yyyy-MM-dd");
            string date2 = DateTime.Now.AddDays(-365).ToString("yyyy-MM-dd");

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;

            DateTime date = DateTime.Now;
            cmd = "Select * from `contacts` WHERE `lastContactDate` >= '" + date2 + "' AND `lastContactDate` <= '" + date1 + "' AND `contactType` = '" + contactType + "' ";
            if (employee.ToUpper() != "ALL")
                cmd += " AND `agent` = '" + employee + "' ";
            cmd += "GROUP BY `contactName` ORDER BY `lastContactDate` ASC ";
            cmd += ";";

            DataTable conDt = G1.get_db_data(cmd);
            conDt.Columns.Add("NewDate");
            for (int i = 0; i < conDt.Rows.Count; i++)
            {
                date = conDt.Rows[i]["apptDate"].ObjToDateTime();
                conDt.Rows[i]["NewDate"] = date.ToString("yyyy-MM-dd");
            }

            DateTime lastContactDate = DateTime.Now;
            string contactName = "";
            string serviceId = "";
            string director = "";
            string primaryPhone = "";
            string phone;
            string primaryEmail = "";
            string prefix = "";
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string suffix = "";

            string apptDate = "";
            DataRow dRow = null;
            DataTable dx = null;
            for ( int i=0; i<conDt.Rows.Count; i++)
            {
                lastContactDate = conDt.Rows[i]["lastContactDate"].ObjToDateTime();
                contactName = conDt.Rows[i]["contactName"].ObjToString();
                serviceId = conDt.Rows[i]["serviceId"].ObjToString();
                director = conDt.Rows[i]["agent"].ObjToString();
                primaryPhone = conDt.Rows[i]["primaryPhone"].ObjToString();
                primaryEmail = conDt.Rows[i]["email"].ObjToString();

                prefix = conDt.Rows[i]["refDeceasedPrefix"].ObjToString();
                firstName = conDt.Rows[i]["refDeceasedFirstName"].ObjToString();
                lastName = conDt.Rows[i]["refDeceasedLastName"].ObjToString();
                middleName = conDt.Rows[i]["refDeceasedMiddleName"].ObjToString();
                suffix = conDt.Rows[i]["refDeceasedSuffix"].ObjToString();

                cmd = "Select * from `contacts` WHERE `contactName` = '" + contactName + "' AND `apptDate` > '" + lastContactDate.ToString("yyyy-MM-dd") + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    days = DetermineDays(lastContactDate, interval, frequency);
                    apptDate = lastContactDate.AddDays(days).ToString("yyyy-MM-dd");

                    record = G1.create_record("contacts", "contactName", "-1");
                    if (G1.BadRecord("contacts", record))
                        break;
                    date = apptDate.ObjToDateTime();
                    if (date >= startDate && date <= stopDate)
                    {
                        dRow = newDt.NewRow();
                        dRow["record"] = record;
                        dRow["contactName"] = contactName;
                        dRow["contactType"] = contactType;
                        dRow["serviceId"] = serviceId;
                        dRow["apptDate"] = G1.DTtoMySQLDT(apptDate);
                        dRow["agent"] = director;
                        dRow["completed"] = "0";
                        dRow["scheduledTask"] = scheduledTask;

                        dRow["primaryPhone"] = primaryPhone;

                        dRow["email"] = primaryEmail;



                        dRow["refDeceasedPrefix"] = prefix;
                        dRow["refDeceasedFirstName"] = firstName;
                        dRow["refDeceasedLastName"] = lastName;
                        dRow["refDeceasedMiddleName"] = middleName;
                        dRow["refDeceasedSuffix"] = suffix;

                        newDt.Rows.Add(dRow);
                    }

                    G1.update_db_table("contacts", "record", record, new string[] { "agent", director, "apptDate", apptDate, "contactType", "Clergy", "contactName", contactName, "serviceId", serviceId, "primaryPhone", primaryPhone, "email", primaryEmail, "scheduledTask", scheduledTask });
                    G1.update_db_table("contacts", "record", record, new string[] { "refDeceasedPrefix", prefix, "refDeceasedFirstName", firstName, "refDeceasedLastName", lastName, "refDeceasedMiddleName", middleName, "refDeceasedSuffix", suffix, "completed", "0" });
                }
            }

            if (newDt.Rows.Count > 0)
                dt.Merge(newDt);

            return dt;
        }
        /***********************************************************************************************/
        private void SetupCompleted(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            string completed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                completed = dt.Rows[i]["completed"].ObjToString();
                if ( completed == "1")
                    dt.Rows[i]["completed"] = "1";
                else
                    dt.Rows[i]["completed"] = "0";
            }
        }
        /***********************************************************************************************/
        private void LoadEmployees ()
        {
            repositoryItemComboBox2.Items.Clear();

            string cmd = "Select * from `tc_er` t JOIN `users` u ON t.`username` = u.`username` WHERE `empStatus` LIKE 'Full%' ";
            string location = cmbLocation.Text.Trim();
            if (!String.IsNullOrWhiteSpace(location) && location.ToUpper() != "ALL")
                cmd += " AND `location` = '" + location + "' ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            DataRow[] dr = dt.Select("lastName<>''");
            if (dr.Length > 0)
                dt = dr.CopyToDataTable();

            string firstName = "";
            string middleName = "";
            string lastName = "";
            string name = "";

            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName,firstName,middleName";
            dt = tempview.ToTable();

            dt.Columns.Add("name");

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();

                if (String.IsNullOrWhiteSpace(lastName))
                    continue;

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name += " " + middleName;

                //cmbEmployee.Items.Add(name);

                repositoryItemComboBox2.Items.Add(name);
                dt.Rows[i]["name"] = name;
            }

            DataRow dR = dt.NewRow();
            dR["name"] = "All";
            dt.Rows.InsertAt(dR, 0);

            cmbEmployee.DataSource = dt;

            DataRow[] dRows = dt.Select("username='" + LoginForm.username + "'");
            if (dRows.Length > 0 && !G1.isAdminOrSuper())
            {
                firstName = dRows[0]["firstName"].ObjToString();
                middleName = dRows[0]["middleName"].ObjToString();
                lastName = dRows[0]["lastName"].ObjToString();

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name = " " + middleName;

                cmbEmployee.Text = name;
                primaryName = name;
                gridMain.Columns["agent"].Visible = false;
                showAgent = false;
            }
        }
        /***********************************************************************************************/
        private void LoadContactTypesX ()
        {
            repositoryItemComboBox1.Items.Clear();
            cmbContractType.Items.Clear();
            cmbContractType.Items.Add("All");

            string contactType = "";

            string cmd = "Select * from `contacttypes`;";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contactType = dt.Rows[i]["contactType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contactType))
                {
                    repositoryItemComboBox1.Items.Add(contactType);
                    cmbContractType.Items.Add(contactType);
                }
            }

            cmbContractType.Text = "All";
            trackDt = G1.get_db_data("Select * from `track`;");
            ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
        }
        /***********************************************************************************************/
        private void LoadContactTypes()
        {
            repositoryItemComboBox1.Items.Clear();
            cmbContractType.Items.Clear();
            cmbContractType.Items.Add("All");

            chkContactType.Properties.Items.Clear();

            string contactType = "";
            string category = "";

            string cmd = "Select * from `contacttypes`;";
            DataTable dt = G1.get_db_data(cmd);
            DataTable typeDt = dt.Clone();
            DataTable catDt = dt.Clone();
            DataRow[] dRows = null;
            DataRow dRow = null;

            dRow = typeDt.NewRow();
            dRow["category"] = "All";
            typeDt.Rows.Add(dRow);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contactType = dt.Rows[i]["contactType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contactType))
                {
                    repositoryItemComboBox1.Items.Add(contactType);
                    cmbContractType.Items.Add(contactType);

                    dRow = typeDt.NewRow();
                    dRow["contactType"] = contactType;
                    typeDt.Rows.Add(dRow);
                }

                category = dt.Rows[i]["category"].ObjToString();
                if (!String.IsNullOrWhiteSpace(category))
                {
                    dRows = catDt.Select("contactType='" + contactType + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = catDt.NewRow();
                        dRow["contactType"] = contactType;
                        dRow["category"] = category;
                        dRow["order"] = i;
                        catDt.Rows.Add(dRow);
                    }
                    dRows = catDt.Select("contactType='" + category + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = catDt.NewRow();
                        dRow["contactType"] = category;
                        dRow["category"] = category;
                        dRow["order"] = 99;
                        catDt.Rows.Add(dRow);
                    }
                }
                else
                {
                    dRows = catDt.Select("contactType='" + contactType + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = catDt.NewRow();
                        dRow["contactType"] = contactType;
                        dRow["category"] = category;
                        dRow["order"] = i;
                        catDt.Rows.Add(dRow);
                    }
                }
            }

            if (catDt.Rows.Count > 0)
                typeDt.Merge(catDt);

            DataView tempview = catDt.DefaultView;
            tempview.Sort = "order asc";
            catDt = tempview.ToTable();


            chkContactType.Properties.DataSource = catDt;

            allType = typeDt;

            cmbContractType.Text = "All";
            trackDt = G1.get_db_data("Select * from `track`;");
            ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
        }
        /***********************************************************************************************/
        private void LoadLocationsx()
        {
            string cmd = "Select * from `funeralhomes` order by `LocationCode`;";
            DataTable locDt = G1.get_db_data(cmd);
            DataRow dRow = locDt.NewRow();
            dRow["LocationCode"] = "All";
            locDt.Rows.InsertAt(dRow, 0);
            cmbLocation.DataSource = locDt;
        }
        /***********************************************************************************************/
        private void LoadLocations()
        {
            string location = "";
            DataTable locDt = null;

            string cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable usersDt = G1.get_db_data(cmd);
            if (usersDt.Rows.Count > 0 && !superuser)
            {
                cmbLocation.Items.Add("All");
                string assignedLocations = usersDt.Rows[0]["assignedLocations"].ObjToString();
                if (!String.IsNullOrWhiteSpace(assignedLocations))
                {
                    string[] Lines = assignedLocations.Split('~');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        location = Lines[i].Trim();
                        if (!String.IsNullOrWhiteSpace(location))
                            cmbLocation.Items.Add(location);
                    }
                }
                else
                {
                    cmd = "Select * from `funeralhomes` order by `LocationCode`;";
                    locDt = G1.get_db_data(cmd);
                    for (int i = 0; i < locDt.Rows.Count; i++)
                    {
                        cmbLocation.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
                    }
                }
            }
            else
            {
                cmbLocation.Items.Add("All");
                cmd = "Select * from `funeralhomes` order by `LocationCode`;";
                locDt = G1.get_db_data(cmd);
                for (int i = 0; i < locDt.Rows.Count; i++)
                {
                    cmbLocation.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
                }
                //DataRow dRow = locDt.NewRow();
                //dRow["LocationCode"] = "All";
                //locDt.Rows.InsertAt(dRow, 0);
                //cmbLocation.DataSource = locDt;
            }
            cmbLocation.Text = "All";

            cmd = "Select * from `funeralhomes` order by `LocationCode`;";
            locDt = G1.get_db_data(cmd);

            DataRow[] dRows = null;
            DataRow dRow = null;
            DataTable tempDt = new DataTable();
            tempDt.Columns.Add("locationCode");
            tempDt.Columns.Add("atneedcode");

            for (int i = 0; i < cmbLocation.Items.Count; i++)
            {
                location = cmbLocation.Items[i].ObjToString();
                if (location == "All")
                    continue;

                dRows = locDt.Select("locationCode='" + location + "'");
                if (dRows.Length > 0)
                {
                    dRow = tempDt.NewRow();
                    dRow["locationCode"] = location;
                    dRow["atneedcode"] = dRows[0]["atneedcode"].ObjToString();
                    tempDt.Rows.Add(dRow);
                }
            }

            chkLocations.Properties.DataSource = tempDt;
        }
        /***************************************************************************************/
        private void CiLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            //if (help.ToUpper() == "TRACKING")
            //{
            //    DataRow[] dR = null;
            //    string cmd = "reference LIKE '" + dbField + "~%'";
            //    DataRow[] dRows = dt.Select(cmd);
            //    if (dRows.Length > 0)
            //    {
            //        string[] Lines = null;
            //        string field = "";
            //        string answer = "";
            //        for (int i = 0; i < dRows.Length; i++)
            //        {
            //            Lines = dRows[i]["reference"].ObjToString().Split('~');
            //            if (Lines.Length <= 1)
            //                continue;
            //            field = Lines[1].Trim();
            //            dbField = FixUsingFieldData(dbField);

            //            dR = trackDt.Select("tracking='" + dbField.Trim() + "' AND answer='" + what.Trim() + "' AND ( location='" + EditCust.activeFuneralHomeName + "' OR location='All' )");
            //            answer = ProcessReference(dR, field);
            //            dRows[i]["data"] = answer.Trim();
            //            dRows[i]["mod"] = "Y";
            //        }
            //    }
            //    dt.AcceptChanges();
            //}
        }
        /***********************************************************************************************/
        private void CheckForSaving()
        {
            //if (!funModified)
            //    return;
            //DialogResult result = MessageBox.Show("***Question***\nPayments have been modified!\nWould you like to SAVE your Payments?", "Payments Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.No)
            //    return;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilterX(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            if (chkExcludeCompleted.Checked)
            {
                string completed = dt.Rows[row]["completed"].ObjToString();
                if (completed == "1")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }

            string cType = cmbContractType.Text.Trim().ToUpper();
            if (cType == "ALL")
                return;

            string contactType = dt.Rows[row]["contactType"].ObjToString().ToUpper();
            if ( contactType != cType )
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            if (dt.Rows[row].RowState == DataRowState.Deleted)
                return;
            if (chkExcludeCompleted.Checked)
            {
                string completed = dt.Rows[row]["completed"].ObjToString();
                if (completed == "1")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            //if (chkDoNotCall.Checked)
            //{
            //    string status = dt.Rows[row]["contactStatus"].ObjToString().ToUpper();
            //    if (status == "DO NOT CALL")
            //    {
            //        e.Visible = false;
            //        e.Handled = true;
            //    }
            //    else if (status.IndexOf("ALREADY") >= 0)
            //    {
            //        e.Visible = false;
            //        e.Handled = true;
            //    }
            //    else if (status == "DECEASED")
            //    {
            //        e.Visible = false;
            //        e.Handled = true;
            //    }
            //}

            string contactType = dt.Rows[row]["contactType"].ObjToString();
            string category = "";

            bool found = false;
            DataRow[] dRows = null;
            string[] Lines = null;

            string what = chkContactType.Text.Trim();
            if (!String.IsNullOrWhiteSpace(what))
            {

                DataTable catDt = (DataTable)chkContactType.Properties.DataSource;

                Lines = what.Split('|');
                for (int i = 0; i < Lines.Length; i++)
                {
                    category = Lines[i].Trim();
                    if (String.IsNullOrWhiteSpace(category))
                        continue;
                    dRows = allType.Select("category='" + category + "'");

                    if (category == contactType)
                    {
                        found = true;
                        break;
                    }
                    dRows = catDt.Select("category='" + category + "'");
                    if (dRows.Length > 0)
                    {
                        for (int j = 0; j < dRows.Length; j++)
                        {
                            category = dRows[j]["contactType"].ObjToString();
                            if (category == contactType)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }

                if (!found)
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }

            DataTable locDt = (DataTable)chkLocations.Properties.DataSource;

            found = false;
            string trust = "";
            string loc = "";
            what = chkLocations.Text.Trim();
            string serviceId = dt.Rows[row]["serviceId"].ObjToString();
            Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
            if (!String.IsNullOrWhiteSpace(loc))
            {
                dRows = locDt.Select("atneedcode='" + loc + "'");
                if (dRows.Length <= 0)
                    return;

                //string funeralHome = dt.Rows[row]["funeralHome"].ObjToString();
                string funeralHome = dRows[0]["locationCode"].ObjToString();

                if (!String.IsNullOrWhiteSpace(what))
                {
                    string location = "";
                    Lines = what.Split('|');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        category = Lines[i].Trim();
                        if (String.IsNullOrWhiteSpace(category))
                            continue;
                        if (funeralHome == category)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        e.Visible = false;
                        e.Handled = true;
                    }
                }
            }

            //if (btnShowAnniversary.BackColor == Color.PaleGreen)
            //{
            //    if (e.Visible)
            //    {
            //        string showWhat = cmbAnniversary.Text.ToUpper();
            //        DateTime today = DateTime.Now;
            //        DateTime dob = dt.Rows[row]["dob"].ObjToDateTime();
            //        if (showWhat.ToUpper() == "DECEASED ANNIVERSARY")
            //        {
            //            if (G1.get_column_number(dt, "funeralDeceased") < 0)
            //                return;
            //            dob = dt.Rows[row]["funeralDeceased"].ObjToDateTime();
            //        }
            //        int month = dob.Month;
            //        int day = dob.Day;

            //        int days = 7;
            //        string str = cmbNextDays.Text.ToUpper();
            //        if (str == "14 DAYS")
            //            days = 14;
            //        else if (str == "30 DAYS")
            //            days = 30;

            //        DateTime birth = new DateTime(today.Year, month, day);
            //        if (birth < today || birth > today.AddDays(days))
            //        {
            //            e.Visible = false;
            //            e.Handled = true;
            //        }
            //    }
            //}
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
            int rowHandle = e.RowHandle;
            if (rowHandle < 0)
                return;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dgv.DataSource == null)
                return;
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                bool doDate = false;
                if (e.Column.FieldName == "apptDate")
                    doDate = true;
                //else if (e.Column.FieldName == "lastContactDate")
                //    doDate = true;

                if (doDate)
                {
                    if (!String.IsNullOrWhiteSpace(e.DisplayText.Trim()))
                    {
                        DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                        if (date.Year < 30)
                            e.DisplayText = "";
                        else
                        {
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool justSaved = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //if (e == null)
            //    return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            dr["mod"] = "Y";


            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            string what = dr[currentColumn].ObjToString();
            if (currentColumn.ToUpper() == "CONTACTNAME")
            {
                what = dr[currentColumn].ObjToString();

                if (String.IsNullOrWhiteSpace(what))
                    return;
                bool found = false;

                string contactType = dr["contactType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contactType))
                {

                    DataTable cDt = null;
                    string cmd = "Select * from `track` WHERE `contactType` = '" + contactType + "' AND `answer` LIKE '%" + what + "%' ;";
                    cDt = G1.get_db_data(cmd);
                    if ( cDt.Rows.Count > 0 )
                    {
                        what = cDt.Rows[0]["answer"].ObjToString();
                        dr["contactName"] = what;
                    }
                }
            }
            if (currentColumn.ToUpper() == "NUM")
                return;
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                if ( currentColumn.ToUpper() == "APPTDATE")
                {
                    DateTime date = what.ObjToDateTime();
                    what = date.ToString("yyyy-MM-dd");
                }
                try
                {
                    G1.update_db_table("contacts", "record", record, new string[] { currentColumn, what });
                }
                catch ( Exception ex)
                {
                }
            }

            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            modified = true;
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Remove Existing Payment
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string agent = dr["agent"].ObjToString();
            if (agent == primaryName || G1.isAdmin())
            {
                DialogResult result = MessageBox.Show("Permanently Delete This Contact Entry?", "Delete Contact Entry Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if ( result == DialogResult.Yes )
                {
                    string record = dr["record"].ObjToString();
                    G1.delete_db_table("contacts", "record", record);

                    //dt.Rows.Remove(dr);
                    gridMain.DeleteRow(row);

                    G1.NumberDataTable(dt);
                }
            }
            else
            {
                MessageBox.Show("Do do not have permission to\ndelete this contact!", "Delete Contact Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Contact
            DataTable dt = (DataTable)dgv.DataSource;

            string cmd = "DELETE from `contacts` WHERE `agent` = '-1'";
            G1.get_db_data(cmd);

            string record = G1.create_record("contacts", "agent", "-1");
            if (G1.BadRecord("contacts", record))
                return;

            string agent = cmbEmployee.Text.Trim();
            string contactType = cmbContractType.Text.Trim();

            DateTime date = DateTime.Now;
            string apptDate = date.ToString("yyyy-MM-dd");
            G1.update_db_table("contacts", "record", record, new string[] { "agent", agent, "apptDate", apptDate, "contactType", contactType });

            DataRow dRow = dt.NewRow();
            DateTime now = DateTime.Now;
            dRow["record"] = record;
            dRow["apptDate"] = G1.DTtoMySQLDT(date);
            dRow["mod"] = "Y";
            dRow["completed"] = "0";
            dRow["contactType"] = contactType;
            dRow["agent"] = agent;
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;


            GoToLastRow(gridMain);

            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void GoToLastRow (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain )
        {
            if (gridMain == null)
                return;
            if (gridMain.GridControl == null)
                return;
            DevExpress.XtraGrid.GridControl dgv = gridMain.GridControl;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;

            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = dt.Rows.Count - 1;
                gridMain.SelectRow(row);
                gridMain.FocusedRowHandle = row;
                gridMain.RefreshData();
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            //AddMod(dt, gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if ( name.ToUpper().IndexOf("DATE") >= 0 )
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
            bool doDate = false;
            bool doTime = false;
            if (name == "apptDate")
                doDate = true;
            else if (name == "lastContactDate")
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
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string record = dr["record"].ObjToString();
            string contactName = dr["contactName"].ObjToString();
            string contactType = dr["contactType"].ObjToString();
            if (String.IsNullOrWhiteSpace(contactName))
                return;

            string oldNotes = dr["notes"].ObjToString();

            using (ContactHistory historyForm = new ContactHistory(gridMain, dt, row, record, contactType, contactName, dr))
            {
                historyForm.contactHistoryDone += HistoryForm_contactHistoryDone;
                historyForm.ShowDialog();
                string lastRecord = record;
                bool modified = historyForm.isModified;
                string nextCompleted = historyForm.nextCompleted;
                lastRecord = historyForm.lastRecord;
                DataRow[] dRows = null;
                if (!String.IsNullOrWhiteSpace(lastRecord))
                    record = lastRecord;
                if (modified)
                    PositionToRecord(dt, record);
                else
                {
                    dt = (DataTable)dgv.DataSource;
                    dRows = dt.Select("oldRecord='" + record + "'");
                    if (dRows.Length > 0)
                        record = dRows[0]["record"].ObjToString();
                    PositionToRow(record);
                    //PositionToRecord(dt, record, true );
                }
                string cmd = "Select * from `contacts_preneed` WHERE `record` = '" + record + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string notes = dx.Rows[0]["notes"].ObjToString();
                    if (notes != oldNotes)
                    {
                        if (dRows.Length > 1)
                        {
                            for (int i = 0; i < dRows.Length; i++)
                            {
                                dr["notes"] = notes;
                                dRows[i]["notes"] = notes;
                            }
                        }
                        else
                        {
                            dr["notes"] = notes;
                            dt.Rows[row]["notes"] = notes;
                        }
                        if (oldNotes == "Birthday Soon" || oldNotes == "DOD Anniversary")
                        {
                            dr["notes"] = oldNotes;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void PositionToRecord(DataTable dt, string record, bool old = false)
        {
            //if (newAddition)
            //{
            //    gridMain.SelectRow(0);
            //    gridMain.FocusedRowHandle = 0;
            //    gridMain.RefreshEditor(true);
            //    return;
            //}

            string prefix = "";
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string suffix = "";
            string agent = "";
            DateTime date = DateTime.Now;
            string home = "";
            string name = "";
            string extraName = "";
            //string searchBy = cmbSearch.Text;
            if (String.IsNullOrWhiteSpace(agent))
                agent = cmbEmployee.Text;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //prefix = dt.Rows[i]["prefix"].ObjToString();
                //firstName = dt.Rows[i]["firstName"].ObjToString();
                //lastName = dt.Rows[i]["lastName"].ObjToString();
                //middleName = dt.Rows[i]["middleName"].ObjToString();
                //suffix = dt.Rows[i]["suffix"].ObjToString();
                agent = dt.Rows[i]["agent"].ObjToString();
                date = dt.Rows[i]["apptDate"].ObjToDateTime();
                //if (searchBy == "Last Touch Date")
                //    date = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                //else if (searchBy == "Next Touch Date")
                //    date = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                home = dt.Rows[i]["funeralHome"].ObjToString();
                //name = prefix + " " + firstName + " " + middleName + " " + lastName + " " + suffix;

                name = dt.Rows[i]["contactName"].ObjToString();

                extraName = agent + "~" + home + "~" + name + "~" + date.ToString("yyyyMMdd");
                dt.Rows[i]["extraName"] = extraName;
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "extraName asc";
            dt = tempview.ToTable();

            //if (initialLoad)
            //    dt = SetupGreenAndRed(dt);

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);

            dt = (DataTable)dgv.DataSource;

            string oldRecord = "";
            if (old)
            {
                gridMain.SelectRow(0);
                gridMain.FocusedRowHandle = 0;
                gridMain.RefreshEditor(true);
                return;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                oldRecord = dt.Rows[i]["record"].ObjToString();
                if (old)
                    oldRecord = dt.Rows[i]["oldRecord"].ObjToString();
                if (oldRecord == record)
                {
                    gridMain.SelectRow(i);
                    gridMain.FocusedRowHandle = i;
                    gridMain.RefreshEditor(true);
                    break;
                }
            }
        }
        /****************************************************************************************/
        private void PositionToRow(string record)
        {
            string rec = "";
            bool filtered = false;
            int row = -1;
            gridMain.ClearSelection();

            string firstName = "";
            string lastName = "";

            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    filtered = CheckRowFiltered(i);
                    if (filtered)
                    {
                        rec = dt.Rows[i]["record"].ObjToString();
                        if (rec != record)
                            continue;
                        if (row < 0)
                            row = 0;
                        gridMain.SelectRow(row);
                        gridMain.FocusedRowHandle = row;
                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                        dgv.Refresh();
                        break;
                    }
                    row++;
                    rec = dt.Rows[i]["record"].ObjToString();
                    if (rec == record)
                    {
                        gridMain.SelectRow(row);
                        gridMain.FocusedRowHandle = row;
                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                        dgv.Refresh();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    gridMain.SelectRow(0);
                    gridMain.FocusedRowHandle = 0;
                    gridMain.RefreshEditor(true);
                    gridMain.RefreshData();
                    dgv.Refresh();
                }
            }
        }
        /****************************************************************************************/
        private bool CheckRowFiltered(int row)
        {
            if (dgv == null)
                return false;
            if (dgv.DataSource == null)
                return false;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return false;
            if (dt.Rows[row].RowState == DataRowState.Deleted)
                return false;
            if (chkExcludeCompleted.Checked)
            {
                string completed = dt.Rows[row]["completed"].ObjToString();
                if (completed == "1")
                    return true;
            }
            //if (chkDoNotCall.Checked)
            //{
            //    string status = dt.Rows[row]["contactStatus"].ObjToString().ToUpper();
            //    if (status == "DO NOT CALL")
            //    {
            //        return true;
            //    }
            //    else if (status.IndexOf("ALREADY") >= 0)
            //    {
            //        return true;
            //    }
            //    else if (status == "DECEASED")
            //    {
            //        return true;
            //    }
            //}

            string contactType = dt.Rows[row]["contactType"].ObjToString();
            string category = "";

            bool found = false;
            DataRow[] dRows = null;
            string[] Lines = null;

            string what = cmbContractType.Text.Trim();
            if (!String.IsNullOrWhiteSpace(what))
            {

                //DataTable catDt = (DataTable)cmbContractType.Properties.DataSource;
                DataTable catDt = (DataTable)cmbContractType.DataSource;

                Lines = what.Split('|');
                for (int i = 0; i < Lines.Length; i++)
                {
                    category = Lines[i].Trim();
                    if (String.IsNullOrWhiteSpace(category))
                        continue;
                    dRows = allType.Select("category='" + category + "'");

                    if (category == contactType)
                    {
                        found = true;
                        break;
                    }
                    dRows = catDt.Select("category='" + category + "'");
                    if (dRows.Length > 0)
                    {
                        for (int j = 0; j < dRows.Length; j++)
                        {
                            category = dRows[j]["contactType"].ObjToString();
                            if (category == contactType)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }

                if (!found)
                {
                    return true;
                }
            }

            //DataTable locDt = (DataTable)chkLocations.Properties.DataSource;
            DataTable locDt = (DataTable)cmbLocation.DataSource;

            found = false;
            what = cmbLocation.Text.Trim();
            string funeralHome = dt.Rows[row]["funeralHome"].ObjToString();

            if (!String.IsNullOrWhiteSpace(what))
            {
                string location = "";
                Lines = what.Split('|');
                for (int i = 0; i < Lines.Length; i++)
                {
                    category = Lines[i].Trim();
                    if (String.IsNullOrWhiteSpace(category))
                        continue;
                    if (funeralHome == category)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return true;
                }
            }
            return false;
        }
        /****************************************************************************************/
        private string HistoryForm_contactHistoryDone(DataTable dt, bool somethingDeleted )
        {
            if (dt.Rows.Count <= 0)
            {
                if (somethingDeleted)
                {
                    int rowHandle = gridMain.FocusedRowHandle;
                    LoadData(rowHandle);
                }
                return "";
            }

            DataTable dx = (DataTable)dgv.DataSource;

            bool found = false;
            string record = "";
            string results = "";
            string completed = "";
            string mod = "";
            bool foundDelete = false;
            DataRow[] dRows = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                results = dt.Rows[i]["results"].ObjToString();
                completed = dt.Rows[i]["completed"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();

                dRows = dx.Select("record='" + record + "'");
                if ( dRows.Length > 0 )
                {
                    found = true;
                    if (mod == "D")
                    {
                        G1.delete_db_table("contacts", "record", record);

                        dx.Rows.Remove(dRows[0]);
                        G1.NumberDataTable(dx);
                        foundDelete = true;
                    }
                    else
                    {
                        G1.copy_dr_row(dt.Rows[i], dRows[0] );
                        //dRows[0]["results"] = results;
                        //dRows[0]["completed"] = completed;
                        //dRows[0]["mod"] = mod;
                    }
                }
            }

            if ( found )
            {
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }

            return completed;
        }
        /****************************************************************************************/
        private void Contacts_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Validate();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

            //string initialized = dt.Rows[row]["initialized"].ObjToString();

            //string saveDescription = dr["localDescription"].ObjToString();
            //string saveBank = dr["bankAccount"].ObjToString();

            //try
            //{
            //    string type = dr["type"].ObjToString().ToUpper();
            //    string what = dr["status"].ObjToString().ToUpper();
            //    row = gridMain.GetDataSourceRowIndex(row);
            //    //if ( !loading )
            //    //    dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
            //    if (what.ToUpper() == "DEPOSITED")
            //    {
            //        string bankAccount = GetDepositBankAccount(type);
            //        if (!String.IsNullOrWhiteSpace(bankAccount))
            //        {
            //            dr["bankAccount"] = bankAccount;
            //            dt.Rows[row]["bankAccount"] = bankAccount;
            //            gridMain.RefreshEditor(true);
            //            dgv.RefreshDataSource();
            //            dgv.Refresh();
            //        }
            //    }
            //    else
            //    {
            //        saveBank = "";
            //        saveDescription = "";
            //        dr["bankAccount"] = "";
            //        dr["localDescription"] = "";
            //        dt.Rows[row]["bankAccount"] = "";
            //        dt.Rows[row]["localDescription"] = "";
            //    }
            //    if (!String.IsNullOrWhiteSpace(saveDescription))
            //    {
            //        dr["bankAccount"] = saveBank;
            //        dr["localDescription"] = saveDescription;
            //        dt.Rows[row]["bankAccount"] = saveBank;
            //        dt.Rows[row]["localDescription"] = saveDescription;
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();

            if (workAuto )
            {
                //string filename = "";
                DateTime today = DateTime.Now;
                string path = "C:/SMFS_Reports/Contact_Preneeds";
                G1.verify_path(path);
                string report = CleanupReportName(workReport);
                string filename = path + @"\" + report + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                //filename = workPDFfile;
                filename = G1.RandomizeFilename(filename);

                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    GrantFileAccess(filename);

                    FileAttributes attributes = File.GetAttributes(filename);
                    if ((attributes & FileAttributes.Archive) == FileAttributes.Archive)
                    {
                        attributes = RemoveAttribute(attributes, FileAttributes.Archive);
                        File.SetAttributes(filename, attributes);
                    }

                    File.Delete(filename);
                }

                printableComponentLink1.ExportToPdf(filename);

                RemoteProcessing.AutoRunSend(workReport + " for " + today.ToString("MM/dd/yyyy"), filename, workAgent, sendWhere, "", workEmail, sendUsername, true );
            }
            else
                printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private static void GrantFileAccess(string file)
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(file);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
        /***********************************************************************************************/
        private string CleanupReportName(string report)
        {
            report = report.Replace(">=", "GE");
            report = report.Replace("<=", "LE");
            report = report.Replace("=", "Equal");
            report = report.Replace("<", "Less");
            report = report.Replace(">", "Greater");
            return report;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
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
            //            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(5, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            //if (view.FocusedColumn.FieldName.ToUpper() == "STATUS")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //    string status = dr["status"].ObjToString().ToUpper();
            //    if ( status == "CANCELLED")
            //    {
            //        string record = dr["record"].ObjToString();
            //        if (!String.IsNullOrWhiteSpace(record))
            //        {
            //            string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
            //            DataTable dx = G1.get_db_data(cmd);
            //            if (dx.Rows.Count > 0)
            //            {
            //                for (int i = 0; i < dx.Rows.Count; i++)
            //                {
            //                    record = dx.Rows[0]["record"].ObjToString();
            //                    G1.update_db_table("cust_payment_details", "record", record, new string[] { "status", "Cancelled" });

            //                    btnSavePayments_Click(null, null);
            //                    btnSavePayments.Hide();
            //                    btnSavePayments.Refresh();
            //                    justSaved = true;
            //                }
            //            }
            //        }
            //    }
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "DATEENTERED")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //    DateTime date = oldWhat.ObjToDateTime();
            //    dt.Rows[row]["dateEntered"] = G1.DTtoMySQLDT(date);
            //    e.Value = G1.DTtoMySQLDT(date);
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "TRUST_POLICY")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();
            //}
            //else if (view.FocusedColumn.FieldName.ToUpper() == "PAYMENT")
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int rowhandle = gridMain.FocusedRowHandle;
            //    int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //    oldWhat = e.Value.ObjToString();

            //    string record = dr["record"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(record))
            //    {
            //        string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
            //        DataTable dx = G1.get_db_data(cmd);
            //        if ( dx.Rows.Count > 0 )
            //        {
            //            double payment = dr["payment"].ObjToDouble();
            //            payment = oldWhat.ObjToDouble();
            //            record = dx.Rows[0]["record"].ObjToString();
            //            G1.update_db_table("cust_payment_details", "record", record, new string[] {"paid", payment.ToString() });

            //            btnSavePayments_Click(null, null);
            //            btnSavePayments.Hide();
            //            btnSavePayments.Refresh();
            //            funModified = false;
            //            justSaved = true;
            //        }
            //    }
            //}
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            //if (e.Column.FieldName.ToUpper() == "CHECKLIST")
            //{
            //    string type = view.GetRowCellValue(e.RowHandle, "type").ObjToString().ToUpper();
            //    if (type != "INSURANCE" && type != "POLICY" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" && type != "3RD PARTY")
            //    {
            //        e.RepositoryItem = null;
            //        return;
            //    }
            //    string status = view.GetRowCellValue(e.RowHandle, "status").ObjToString();
            //    if (status.ToUpper() == "FILED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //    else if ( status.ToUpper() == "DEPOSITED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit1;
            //    else
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //}
        }
        /****************************************************************************************/
        private string oldColumn = "";
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        /****************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int row = gridMain.FocusedRowHandle;

            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime myDate = DateTime.Now;
            oldColumn = name;

            bool doDate = false;
            if (name == "apptDate")
                doDate = true;
            else if (name == "lastContactDate")
                doDate = true;

            if (doDate)
            {
                myDate = dr[name].ObjToDateTime();
                str = gridMain.Columns[name].Caption;
                using (GetDate dateForm = new GetDate(myDate, str))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            myDate = dateForm.myDateAnswer;
                            dr[name] = G1.DTtoMySQLDT(myDate);
                        }
                        catch (Exception ex)
                        {
                        }
                        //dr[name] = G1.DTtoMySQLDT(myDate);
                        UpdateMod(dr);
                        gridMain_CellValueChanged(null, null);
                    }
                }
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private string currentColumn = "";
        private string oldContactType = "";
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain.SelectRow(rowHandle);
                dgv.RefreshDataSource();
                DataTable dt = (DataTable)dgv.DataSource;

                GridColumn column = hitInfo.Column;
                currentColumn = column.FieldName.Trim();
                string data = dt.Rows[rowHandle][currentColumn].ObjToString();
                DataRow dr = gridMain.GetFocusedDataRow();

                if ( currentColumn == "contactName")
                {
                    this.Validate();
                    string contactType = dr["contactType"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contactType))
                        return;
                    if (contactType == oldContactType)
                        return;
                    oldContactType = contactType;

                    string viewDetail = DetermineView(contactType);

                    string answer = "";
                    ciLookup.Items.Clear();
                    if (myDt == null)
                    {
                        myDt = new DataTable();
                        myDt.Columns.Add("stuff");
                    }
                    myDt.Rows.Clear();
                    string cmd = "Select * from `track` where `contactType` = '" + contactType + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    for ( int i=0; i<dx.Rows.Count; i++)
                    {
                        answer = dx.Rows[i]["answer"].ObjToString();
                        if ( String.IsNullOrWhiteSpace ( answer))
                        {
                            if ( viewDetail.ToUpper() == "PERSON")
                            {
                                answer = GetPerson(dx.Rows[i]);
                            }
                        }
                        if ( !String.IsNullOrWhiteSpace ( answer ))
                            AddToMyDt(answer);
                    }

                    ciLookup.Items.Clear();
                    for (int i = 0; i < myDt.Rows.Count; i++)
                        ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());

                    gridMain.Columns[currentColumn].ColumnEdit = ciLookup;
                    gridMain.RefreshData();
                    gridMain.RefreshEditor(true);
                }
            }
        }
        /****************************************************************************************/
        public static string GetPerson ( DataRow dRow )
        {
            string prefix = dRow["depPrefix"].ObjToString();
            string lastName = dRow["depLastName"].ObjToString();
            string firstName = dRow["depFirstName"].ObjToString();
            string middleName = dRow["depMI"].ObjToString();
            string suffix = dRow["depSuffix"].ObjToString();
            string name = prefix;
            name = BuildName(name, lastName);
            if (!String.IsNullOrWhiteSpace(name))
                name += ",";
            name = BuildName(name, firstName);
            name = BuildName(name, middleName);
            name = BuildName(name, suffix);
            return name;
        }
        /***********************************************************************************************/
        public static string BuildName(string name, string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                if (!String.IsNullOrWhiteSpace(name))
                    name += " ";
                name += text;
            }
            return name;
        }
        /****************************************************************************************/
        private string DetermineView ( string contactType )
        {
            string detail = "PLACE";
            string cmd = "Select * from `contactTypes` WHERE `contactType` = '" + contactType + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
                detail = dt.Rows[0]["detail"].ObjToString();
            return detail;
        }
        /****************************************************************************************/
        private DataTable myDt = null;
        private void AddToMyDt(string data)
        {
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            DataRow dRow = myDt.NewRow();
            dRow["stuff"] = data;
            myDt.Rows.Add(dRow);
        }
        /****************************************************************************************/
        private void cmbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void cmbEmployee_DropDown(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox cbo = (System.Windows.Forms.ComboBox)sender;
            cbo.PreviewKeyDown += new PreviewKeyDownEventHandler(comboBox_PreviewKeyDown);
        }
        /****************************************************************************************/
        private void comboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            System.Windows.Forms.ComboBox cbo = (System.Windows.Forms.ComboBox)sender;
            cbo.PreviewKeyDown -= comboBox_PreviewKeyDown;
            if (cbo.DroppedDown) cbo.Focus();
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void cmbContractType_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                string str = "";
                int count = 0;
                string[] Lines = null;
                foreach (GridColumn column in gridMain.Columns)
                {
                    doit = false;
                    name = column.FieldName.ToUpper();
                    if (name == "RESULTS" )
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                                if ( !String.IsNullOrWhiteSpace ( str ))
                                {
                                    Lines = str.Split('\n');
                                    count = Lines.Length;
                                }
                                int oldHeight = e.RowHeight;
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(oldHeight, maxHeight);
                                    if (newHeight > maxHeight)
                                    {
                                        maxHeight = newHeight * count;
                                    }
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0 && maxHeight > e.RowHeight )
                    e.RowHeight = maxHeight;
            }
        }
        /****************************************************************************************/
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string locaton = cmbLocation.Text.Trim();
            LoadEmployees();
            LoadData();
        }
        /****************************************************************************************/
        private void btnShowDatabase_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            EditContacts contactForm = new EditContacts(true, "", "");
            contactForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void addNextContactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dR = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string cmd = "DELETE from `contacts` WHERE `agent` = '-1'";
            G1.get_db_data(cmd);

            string record = G1.create_record("contacts", "agent", "-1");
            if (G1.BadRecord("contacts", record))
                return;

            string agent = dR["agent"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( agent))
                agent = cmbEmployee.Text.Trim();
            string contactType = dR["contactType"].ObjToString();
            string contactName = dR["contactName"].ObjToString();

            DateTime date = DateTime.Now;
            string apptDate = date.ToString("yyyy-MM-dd");
            G1.update_db_table("contacts", "record", record, new string[] { "agent", agent, "apptDate", apptDate, "contactType", contactType, "contactName", contactName });

            DataRow dRow = dt.NewRow();
            DateTime now = DateTime.Now;
            dRow["record"] = record;
            dRow["apptDate"] = G1.DTtoMySQLDT(date);
            dRow["mod"] = "Y";
            dRow["completed"] = "0";
            dRow["contactType"] = contactType;
            dRow["contactName"] = contactName;
            dRow["agent"] = agent;
            dt.Rows.InsertAt(dRow, row);
            //dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();


            //GoToLastRow(gridMain);

            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void chkExcludeCompleted_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkLocations_EditValueChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkContactType_EditValueChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void ClearWaitMessage ()
        {
            if ( waitForm != null )
            {
                G1.StopWait(ref waitForm);
                waitForm = null;
            }
        }
        /***********************************************************************************************/
        private DataTable SetupGreenAndRed(DataTable dt)
        {
            DateTime nextDate = DateTime.Now;
            DateTime today = DateTime.Now;

            if (G1.get_column_number(dt, "color") < 0)
                dt.Columns.Add("color", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["color"] = 0D;

            DateTime dob = DateTime.Now;
            DateTime birth = DateTime.Now;
            int month = 0;
            int day = 0;
            DateTime lastContactDate = DateTime.Now;
            initialLoad = false;

            int lastRow = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lastContactDate = dt.Rows[i]["lastContactDate"].ObjToDateTime();

                if (lastContactDate < today )
                    dt.Rows[i]["color"] = 1D;  // Color.Pink;
                else if (lastContactDate < today.AddDays(5))
                    dt.Rows[i]["color"] = 2D; // Color.LightGreen;
            }
            return dt;
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            //if (1 == 1)
            //    return;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                if (e.RowHandle == gridMain.FocusedRowHandle)
                {
                    this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                    return;
                }

                int col = G1.get_column_number(dt, "color");
                if (col >= 0)
                {
                    double color = dt.Rows[row]["color"].ObjToDouble();
                    if (color == 1D)
                    {
                        e.Appearance.BackColor = Color.Pink;
                        ColorizeCell(e.Appearance, Color.Pink);
                    }
                    else if (color == 2D)
                    {
                        e.Appearance.BackColor = Color.LightGreen;
                        ColorizeCell(e.Appearance, Color.LightGreen);
                    }
                    else if (color == 5D)
                    {
                        e.Appearance.BackColor = Color.Blue;
                        ColorizeCell(e.Appearance, Color.Blue);
                    }
                    else
                    {
                        e.Appearance.BackColor = Color.Transparent;
                        ColorizeCell(e.Appearance, Color.Transparent);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void ColorizeCell(object appearanceObject, Color color)
        {
            AppearanceObject app = appearanceObject as AppearanceObject;
            if (app != null)
            {
                app.ForeColor = Color.Black;
            }
            else
            {
                XlFormattingObject obj = appearanceObject as XlFormattingObject;
                if (obj != null)
                {
                    //obj.BackColor = Color.Red;
                    obj.BackColor = color;
                }
            }
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;

            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "AgentContacts", "Primary", actualName);
            sform.Done += new SelectDisplayColumns.d_void_selectionDone(sxform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sxform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "AgentContacts";
            string skinName = "";
            SetupSelectedColumns("AgentContacts", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            //gridMain.OptionsView.ShowFooter = showFooters;
            //SetupTotalsSummary();
            string field = "";
            string select = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                if (G1.get_column_number(gridMain, field) >= 0)
                {
                    if (select == "0")
                        gridMain.Columns[field].Visible = false;
                    else
                        gridMain.Columns[field].Visible = true;
                }
            }
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                bool found = SetupSelectedColumns("AgentContacts", comboName, dgv);
                if (!found)
                    return;
                string name = "AgentContacts " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("AgentContacts", "Primary", dgv);
                string name = "AgentContacts Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }

            //RemoveResults();

            CleanupFieldColumns();

            if (workDt != null)
            {
                int height = this.Height;
                this.Location = new Point(100, 100);
                this.Height = height + 100;
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void CleanupFieldColumns()
        {
            //if (LoginForm.classification.ToUpper() != "FIELD")
            //    return;
            if (!showAgent)
            {
                gridMain.Columns["agent"].Visible = false;
            }
            //gridBand2.Fixed = FixedStyle.Left;
            //gridMain.Columns["amountDiscount"].Visible = false;
            //gridMain.Columns["contractNumber"].Visible = false;
        }
        /****************************************************************************************/
        private void lockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool agentVisible = false;
            if (gridMain.Columns["agent"].Visible)
            {
                agentVisible = true;
                gridMain.Columns["agent"].Visible = false;
            }
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "AgentContacts " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);

            if (agentVisible)
                gridMain.Columns["agent"].Visible = true;
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "AgentContacts " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /***********************************************************************************************/
        private void LoadReports()
        {
            string report = "";
            string cmd = "Select * from `contacts_reports` where `module` = 'Contacts';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                report = dx.Rows[i]["report"].ObjToString();
                cmbReport.Items.Add(report);
            }
        }
        /****************************************************************************************/
        private void btnRunReport_Click(object sender, EventArgs e)
        {
            string customReport = cmbReport.Text.Trim();
            if (String.IsNullOrWhiteSpace(customReport))
                return;


            string cmd = "Select * from `contacts_reports` WHERE `module` = 'Contacts' AND `report` = '" + customReport + "';";
            DataTable ddd = G1.get_db_data(cmd);
            if (ddd.Rows.Count <= 0)
                return;
            string record = ddd.Rows[0]["record"].ObjToString();
            cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);

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

            bool isCustom = false;

            string agent = cmbEmployee.Text.Trim();
            if (agent.ToUpper() == "ALL")
                agent = "";
            if (string.IsNullOrWhiteSpace(agent))
                agent = workAgent;

            cmd = ContactsPreneed.BuildReportQuery("Contacts", dt, agent, ref isCustom);
            dx = G1.get_db_data(cmd);

            if (dx != null)
            {
                this.Cursor = Cursors.WaitCursor;
                int height = this.Height;

                Contacts form = null;
                if (!isCustom)
                    form = new Contacts(dx, customReport );
                else
                    form = new Contacts(dx, dt, true, customReport, agent );

                //leadForm.StartPosition = FormStartPosition.CenterParent;
                //form.Show();
                form.Anchor = AnchorStyles.None;

                form.AutoSize = true; //this causes the form to grow only. Don't set it if you want to resize automatically using AnchorStyles, as I did below.
                form.FormBorderStyle = FormBorderStyle.Sizable; //I think this is not necessary to solve the problem, but I have left it there just in case :-)
                //form.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                //                    | System.Windows.Forms.AnchorStyles.Left)
                //                    | System.Windows.Forms.AnchorStyles.Right)));

                form.Show();
                form.Location = new Point(this.Parent.Left+500, this.Parent.Top+500);
                form.Height = height + 200;
                form.StartPosition = FormStartPosition.CenterParent;
                form.SetBounds(this.Parent.Left + 100, this.Parent.Top + 100, form.Width, height + 50);
                form.Refresh();

                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            string agent = cmbEmployee.Text;

            ContactReports reports = new ContactReports("Contacts", agent, gridMain );
            reports.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        public RepositoryItemComboBox FireEventGrabSomething(string what)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("item");
            DataRow dRow = null;
            string item = "";
            for (int i = 0; i < dgv.RepositoryItems.Count; i++)
            {
                try
                {
                    item = dgv.RepositoryItems[i].Name.Trim();
                    if (item == what)
                    {
                        return (DevExpress.XtraEditors.Repository.RepositoryItemComboBox)dgv.RepositoryItems[i];
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return null;
        }
        /****************************************************************************************/
        public RepositoryItemComboBox FireEventGrabNewSomething(string what)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("item");
            DataRow dRow = null;
            string item = "";
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                try
                {
                    item = gridMain.Columns[i].FieldName.Trim();

                    //item = dgv.RepositoryItems[i].Name.Trim();
                    if (item == what)
                    {
                        RepositoryItemComboBox cBox = (RepositoryItemComboBox)gridMain.Columns[i].ColumnEdit;
                        return cBox;
                        //return (DevExpress.XtraEditors.Repository.RepositoryItemComboBox)dgv.RepositoryItems[i];
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return null;
        }
        /****************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataTable agentDt = (DataTable)cmbEmployee.DataSource;
            string agent = cmbEmployee.Text.Trim();
            if (agent.ToUpper() == "ALL")
                agent = "";

            ContactReportsAgents agentsForm = new ContactReportsAgents(agentDt, gridMain, agent, "Contacts" );
            agentsForm.Show();
        }
        /****************************************************************************************/
    }
}