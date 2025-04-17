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
using System.Web;
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
using Newtonsoft.Json;

using System.Threading;
using System.Net.Http;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;

//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Calendar.v3;
using MySql.Data.Types;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.Export;
using System.Globalization;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContactsPreneed : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";

        //private const string calID = "xxxxxxxxx...@group.calendar.google.com";
        //private const string UserId = "user-id";
        //private static string gFolder = "";
        private bool foundLocalPreference = false;
        private bool superuser = false;
        private bool showAgent = true;
        private DataTable workDt = null;
        private bool workAuto = false;
        private string workAgent = "";
        private string workEmail = "";
        private string workReport = "";
        private string sendWhere = "";
        private string sendUsername = "";
        private string workFormat = "";
        private bool initialLoad = true;
        private DataTable customDt = null;

        //static string[] Scopes = { CalendarService.Scope.Calendar };

        DataTable allType = null;
        private bool isCustom = false;
        private string customReport = "";
        /****************************************************************************************/
        public ContactsPreneed()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public ContactsPreneed( DataTable dt, string Report = "" )
        {
            InitializeComponent();
            workDt = dt;
            workReport = Report;
        }
        /****************************************************************************************/
        public ContactsPreneed(DataTable dt, DataTable dx, bool custom = false, string Report = "" )
        {
            InitializeComponent();
            workDt = dt;
            customDt = dx;
            isCustom = custom;
            customReport = Report;
        }
        /****************************************************************************************/
        public ContactsPreneed(DataTable dt, bool auto, string agent, string email, string report, string send, string username, string displayFormat, bool custom, DataTable dx )
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
        private void ContactsPreneed_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            btnShowAnniversary.BackColor = Color.Transparent;

            SetupToolTips();

            //if (!G1.isAdmin())
            //    btnTestGoogle.Hide();

            cmbContactType.Hide();
            cmbLocation.Hide();

            if (!String.IsNullOrWhiteSpace(workReport))
                this.Text = "Report : " + workReport;
            else if (!String.IsNullOrWhiteSpace(customReport))
                this.Text = "Report : " + customReport;

            if (!string.IsNullOrWhiteSpace(workAgent))
                this.Text += " for " + workAgent;

            loading = true;

            string preference = G1.getPreference(LoginForm.username, "Agent Preneeds", "Allow SuperUser Access");
            //if (LoginForm.isRobby)
            //    preference = "YES";
            if (preference.ToUpper() == "YES")
                superuser = true;


            //G1.loadGroupCombo(cmbSelectColumns, "AgentPreneeds", "Primary", true, LoginForm.username);
            //cmbSelectColumns.Text = "Original";


            string saveName = "AgentPreneeds Primary";
            string skinName = "";

            if (!String.IsNullOrWhiteSpace(workFormat))
                SetupSelectedColumns("AgentPreneeds", workFormat, dgv);
            else
                SetupSelectedColumns("AgentPreneeds", "Primary", dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                //if (skinName != "DevExpress Style")
                //    skinForm_SkinSelected("Skin : " + skinName);
            }

            RemoveResults();

            if (String.IsNullOrWhiteSpace(workFormat))
                workFormat = "Primary";
            loadGroupCombo(cmbSelectColumns, "AgentPreneeds", workFormat);
            cmbSelectColumns.Text = workFormat;

            DateTime now = DateTime.Now;
//            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            LoadDBTable("ref_relations", "relationship", this.repositoryItemComboBox3);
            LoadDBTable("ref_contact_status", "contact_status", this.repositoryItemComboBox4);
            LoadDBTable("ref_lead source", "lead source", this.repositoryItemComboBox5);

            if (!G1.isAdmin() && !superuser && !G1.RobbyServer )
            {
                assignNewAgentToolStripMenuItem.Dispose();
                showAgent = false;
            }

            LoadContactTypes();
            LoadEmployees();
            LoadLocations();
            loadRepositoryLocatons();
            LoadReports();

            LoadData();

            //SetupSelectedColumns();

            CleanupFieldColumns();

            if (G1.isAdmin() || G1.isHR())
            {
                if (!gridMain.Columns["agent"].Visible)
                    gridMain.Columns["agent"].Visible = true;
            }

            if ( isCustom && customDt != null )
            {
                string field = "";
                string caption = "";
                string operand = "";
                ClearAllPositions( gridMain );
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
                for ( int i=0; i<customDt.Rows.Count; i++)
                {
                    try
                    {
                        field = customDt.Rows[i]["field"].ObjToString();
                        if (field.ToUpper() == "{CUSTOM}")
                            continue;
                        operand = customDt.Rows[i]["operand"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(operand))
                            continue;
                        if ( G1.get_column_number ( gridMain, field ) < 0 )
                        {
                            if (field.ToUpper() == "NAME")
                            {
                                newWidth = gridMain.Columns["lastName"].Width;
                                width = gridMain.Columns["firstName"].Width;
                                newWidth += width;
                                if (newWidth <= 0)
                                    newWidth = 60;
                                if ( G1.get_column_number ( gridMain, field ) < 0 )
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
                                    if ( G1.get_column_number ( gridMain, parm ) < 0 )
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
                    catch ( Exception ex)
                    {
                    }
                }

                if ( customDt != null )
                {
                    DataView tempview = dx.DefaultView;
                    tempview.Sort = "nextScheduledTouchDate asc, color asc";
                    dx = tempview.ToTable();
                }
                dgv.DataSource = dx;
            }

            gridMain.RefreshEditor(true);
            this.Refresh();

            modified = false;
            loading = false;

            if (workDt != null)
            {
                //skinForm_SkinSelected("Skin : Glass Ocean");
                //skinForm_SkinSelected("Skin : Caramel");
                if ( !isCustom)
                    skinForm_SkinSelected("Skin : Windows Default");
                else
                {
                    this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                    this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                }
                miscToolStripMenuItem.Dispose();
            }

            if (!String.IsNullOrWhiteSpace(customReport) || !String.IsNullOrWhiteSpace(workReport))
            {
                this.panelClaimTop.Hide();
                screenOptionsToolStripMenuItem.Dispose();
            }

            if (workFormat != "Primary")
                cmbSelectColumns_SelectedIndexChanged(cmbSelectColumns, null);

            if ( workAuto )
            {
                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }
        }
        /***********************************************************************************************/
        private void LoadReports ()
        {
            string report = "";
            string cmd = "Select * from `contacts_reports` where `module` = 'Contacts Preneed';";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                report = dx.Rows[i]["report"].ObjToString();
                cmbReport.Items.Add(report);
            }
        }
        /***********************************************************************************************/
        private bool decodeSpecialParm ( string field, ref string newField, ref string parm )
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
            catch ( Exception ex )
            {
                rv = false;
            }
            return rv;
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
                    if (isCustom)
                    {
                        DevExpress.Skins.SkinManager.EnableFormSkins();
                        this.LookAndFeel.SetSkinStyle("Basic");
                        //DevExpress.Skins.SkinManager.EnableFormSkins();
                        //this.LookAndFeel.SetSkinStyle("Wheat");
                        //this.LookAndFeel.UseDefaultLookAndFeel = true;
                        ////DevExpress.Skins.SkinManager.DisableFormSkins();
                        //this.gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                        //this.gridMain.Appearance.OddRow.Options.UseBackColor = false;

                        this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.White;
                        this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.White;
                        this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                        this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        this.LookAndFeel.SetSkinStyle(skin);
                        this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.LightGreen;
                        this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.LightGreen;
                        this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                        this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                    }
                }
                else
                {
                    this.panelClaimTop.BackColor = Color.Transparent;
                    this.menuStrip1.BackColor = Color.Transparent;
                    this.gridMain.PaintStyleName = "Skin";
                    DevExpress.Skins.SkinManager.EnableFormSkins();
                    this.LookAndFeel.UseDefaultLookAndFeel = true;
                    DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skin);
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SkinName = skin;
                    gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                    gridMain.Appearance.OddRow.Options.UseBackColor = false;
                    this.panelClaimTop.Refresh();
                    //OnSkinChange(skin);

                    //DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = skin;
                    //this.LookAndFeel.SetSkinStyle(skin);
                    //this.dgv.LookAndFeel.SetSkinStyle(skin);
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
        private void RemoveResults ()
        {
            int col = G1.get_column_number(gridMain, "results");
            if (col < 0)
                return;
            GridColumn column = (GridColumn) gridMain.Columns["results"];
            gridMain.Columns.Remove(column);
        }
        /***********************************************************************************************/
        private void loadRepositoryLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string assignedLocations = "";

            string[] Lines = null;

            string newUser = cmbEmployee.Text;

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            if (!String.IsNullOrWhiteSpace(newUser))
            {
                DataTable uDt = (DataTable) cmbEmployee.DataSource;
                if (uDt.Rows.Count > 0)
                    newUser = uDt.Rows[0]["username"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( newUser))
                {
                    Lines = cmbEmployee.Text.Trim().Split(',');
                    if ( Lines.Length > 1 )
                    {
                        string lastName = Lines[0].Trim();
                        string firstName = Lines[1].Trim();
                        uDt = G1.get_db_data("Select * from `users` WHERE `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "'");
                        if (uDt.Rows.Count > 0)
                            newUser = uDt.Rows[0]["username"].ObjToString();
                    }
                }
                cmd = "Select * from `users` where `username` = '" + newUser + "';";
            }

            newUser = "";

            DataTable userDt = G1.get_db_data(cmd);
            if (userDt.Rows.Count > 0)
                assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();

            string locationCode = "";
            string keyCode = "";
             Lines = null;
            string locations = "";
            string location = "";

            for (int i = locDt.Rows.Count - 1; i >= 0; i--)
            {
                keyCode = locDt.Rows[i]["keycode"].ObjToString();
                if (keyCode.IndexOf("-") > 0)
                    locDt.Rows.RemoveAt(i);
            }
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                locationCode = locDt.Rows[i]["locationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                    continue;
                Lines = assignedLocations.Split('~');
                for (int j = 0; j < Lines.Length; j++)
                {
                    location = Lines[j].Trim();
                    if (String.IsNullOrWhiteSpace(location))
                        continue;
                    if (location.ToUpper() == locationCode.ToUpper())
                    {
                        location = locDt.Rows[i]["atNeedCode"].ObjToString();
                        location = locDt.Rows[i]["LocationCode"].ObjToString();
                        locations += location + "|";
                        newLocDt.ImportRow(locDt.Rows[i]);
                    }
                }
            }
            if (!LoginForm.administrator)
                locDt = newLocDt;

            DataView tempview = locDt.DefaultView;
            //tempview.Sort = "atneedcode";
            tempview.Sort = "LocationCode";
            locDt = tempview.ToTable();

            repositoryItemComboBox6.Items.Add("All");
            for (int i = 0; i < locDt.Rows.Count; i++)
                repositoryItemComboBox6.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
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
        private void LoadData( int rowHandle = -1, string nextRecord = "" )
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            string date2 = G1.DateTimeToSQLDateTime(date);

            string employee = cmbEmployee.Text.Trim();
            string location = cmbLocation.Text.Trim();
            if (location.Trim().ToUpper() == "ALL")
                location = "";
            string searchBy = cmbSearch.Text;

            CheckOldRecords();

            //string cmd = "SELECT contacts_preneed.* FROM contacts_preneed INNER JOIN(SELECT agent, funeralHome, MAX(prospectCreationDate) AS latest FROM contacts_preneed GROUP BY agent, funeralHome) r ON contacts_preneed.prospectCreationDate = r.latest AND contacts_preneed.agent = r.agent ";

            //string cmd = "SELECT contacts_preneed.*FROM contacts_preneed INNER JOIN(SELECT agent, funeralHome, MAX(prospectCreationDate) AS latest FROM contacts_preneed GROUP BY agent, funeralHome) r ";

            string cmd = "SELECT contacts_preneed.*, date_format(dob,'%Y-%m-%d 00:00:00') as s1 FROM contacts_preneed ";
            //cmd += " WHERE ";

            bool needWhere = true;

            if (chkUseDates.Checked)
            {
                cmd += " WHERE ";
                if (searchBy == "Creation Date")
                    cmd += " `prospectCreationDate` >= '" + date1 + "' AND `prospectCreationDate` <= '" + date2 + "' ";
                else if (searchBy == "Last Touch Date")
                    cmd += " `lastTouchDate` >= '" + date1 + "' AND `lastTouchDate` <= '" + date2 + "' ";
                else if (searchBy == "Next Touch Date")
                    cmd += " `nextScheduledTouchDate` >= '" + date1 + "' AND `nextScheduledTouchDate` <= '" + date2 + "' ";

                if (!String.IsNullOrWhiteSpace(employee) && employee.ToUpper() != "ALL")
                    cmd += " AND contacts_preneed.`agent` = '" + employee + "' ";
                if (!String.IsNullOrWhiteSpace(location))
                    cmd += " AND  contacts_preneed.`funeralHome` = '" + location + "' ";
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(employee) && employee.ToUpper() != "ALL")
                {
                    cmd += " WHERE contacts_preneed.`agent` = '" + employee + "' ";
                    if (!String.IsNullOrWhiteSpace(location))
                        cmd += " AND  contacts_preneed.`funeralHome` = '" + location + "' ";
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(location))
                        cmd += " WHERE contacts_preneed.`funeralHome` = '" + location + "' ";
                }
            }

            string saveCmd = cmd;

            //cmd += " AND `dob` > '2001-01-01' ";

            if (searchBy == "Creation Date")
            {
                cmd += " ORDER BY agent, funeralHome, oldRecord asc, prospectCreationDate ASC, nextScheduledTouchDate ASC, lastTouchDate ASC;";
            }
            else if (searchBy == "Last Touch Date")
            {
                cmd += " ORDER BY agent, funeralHome, oldRecord asc, lastTouchDate DESC;";
            }
            else if (searchBy == "Next Touch Date")
            {
                cmd += " ORDER BY agent, funeralHome, oldRecord asc, nextScheduledTouchDate DESC;";
            }
            else
                cmd += " ORDER BY agent, funeralHome, oldRecord asc, prospectCreationDate DESC;";

            DataTable dt = null;
            if (workDt != null)
            {
                dt = workDt;
                if (location.Trim().ToUpper() != "ALL" && !String.IsNullOrWhiteSpace ( location ))
                {
                    DataRow[] dRows = dt.Select("funeralhome='" + location + "'");
                    if (dRows.Length > 0)
                        dt = dRows.CopyToDataTable();
                }
            }
            else
                dt = G1.get_db_data(cmd);

            if ( G1.get_column_number ( dt, "ExtraName") < 0 )
                dt.Columns.Add("ExtraName");
            if (G1.get_column_number(dt, "ExtraName2") < 0)
                dt.Columns.Add("ExtraName2");

            dt = G1.RemoveDuplicates(dt, "record");

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string prefix = "";
            string suffix = "";
            string extraName = "";
            string agent = "";
            string home = "";
            string name = "";
            DateTime lastTouchDate = DateTime.Now;
            DateTime nextTouchDate = DateTime.Now;
            string strDate = "";
            DataView tempview = dt.DefaultView;
            if (1 != 1)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    prefix = dt.Rows[i]["prefix"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    middleName = dt.Rows[i]["middleName"].ObjToString();
                    suffix = dt.Rows[i]["suffix"].ObjToString();
                    agent = dt.Rows[i]["agent"].ObjToString();
                    date = dt.Rows[i]["prospectCreationDate"].ObjToDateTime();
                    lastTouchDate = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                    nextTouchDate = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                    if (searchBy == "Creation Date")
                    {
                        strDate = date.ToString("yyyyMMdd") + "~" + nextTouchDate.ToString("yyyyMMdd") + "~" + lastTouchDate.ToString("yyyyMMdd");
                    }
                    if (searchBy == "Last Touch Date")
                    {
                        //date = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                        strDate = lastTouchDate.ToString("yyyyMMdd") + "~" + nextTouchDate.ToString("yyyyMMdd");
                    }
                    else if (searchBy == "Next Touch Date")
                    {
                        //date = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                        strDate = nextTouchDate.ToString("yyyyMMdd") + "~" + lastTouchDate.ToString("yyyyMMdd");
                    }
                    home = dt.Rows[i]["funeralHome"].ObjToString();
                    name = prefix + " " + firstName + " " + middleName + " " + lastName + " " + suffix;

                    extraName = agent + "~" + home + "~" + name + "~" + strDate;
                    dt.Rows[i]["extraName"] = extraName;
                    extraName = agent + "~" + home + "~" + name + "~";
                    dt.Rows[i]["extraName2"] = extraName;
                }

                tempview = dt.DefaultView;
                tempview.Sort = "extraName asc";
                dt = tempview.ToTable();

                dt = dt.AsEnumerable().GroupBy(x => x.Field<string>("extraName2")).Select(x => x.Last()).CopyToDataTable();
            }

            //DataView tempview = dt.DefaultView;
            //tempview.Sort = "extraName asc";
            //dt = tempview.ToTable();

            //dt = dt.AsEnumerable().GroupBy(x => x.Field<string>("extraName2")).Select(x => x.Last()).CopyToDataTable();

            name = "";
            string[] Lines = null;
            extraName = "";

            tempview = dt.DefaultView;
            tempview.Sort = "oldRecord asc, record DESC";
            dt = tempview.ToTable();

            string record = "";
            string oldRecord = "";
            int touches = 0;
            bool gotAge = false;
            if (G1.get_column_number(dt, "age") >= 0)
                gotAge = true;
            DateTime now = DateTime.Now;
            DateTime bDay = DateTime.Now;
            int age = 0;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                if (i == 0)
                {
                }
                if (gotAge)
                {
                    bDay = dt.Rows[i]["DOB"].ObjToDateTime();
                    if ( bDay.Year > 1000 )
                    {
                        age = G1.CalculateAgeCorrect(bDay, now);
                        dt.Rows[i]["age"] = age;
                    }
                }
                record = dt.Rows[i]["oldRecord"].ObjToString();
                //touches = dt.Rows[i]["totalTouches"].ObjToInt32();
                //if (touches > 0)
                //    touches = touches - 1;
                //else
                //    touches = 0;
                //dt.Rows[i]["totalTouches"] = touches;
                if (record == oldRecord)
                    dt.Rows.RemoveAt(i + 1);
                else
                {
                    oldRecord = record;
                }
            }

            tempview = dt.DefaultView;
            if (searchBy == "Creation Date")
            {
                tempview.Sort = "agent, funeralHome, prospectCreationDate ASC, nextScheduledTouchDate ASC, lastTouchDate ASC";
            }
            else if (searchBy == "Last Touch Date")
            {
                tempview.Sort = "agent, funeralHome, lastTouchDate DESC";
            }
            else if (searchBy == "Next Touch Date")
            {
                tempview.Sort = "agent, funeralHome, nextScheduledTouchDate DESC";
            }
            else
                tempview.Sort = "agent, funeralHome, prospectCreationDate DESC";
            dt = tempview.ToTable();

            //dt = G1.RemoveDuplicates(dt, "agent", "funeralHome", false );
            //dt = G1.RemoveDuplicates(dt, "extraName");

            AddMod(dt, gridMain);

            SetupCompleted ( dt );

            dt = cleanupPhones(dt);

            if ( initialLoad )
                dt = SetupGreenAndRed(dt);

            G1.NumberDataTable(dt);

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

            if ( rowHandle != -1 )
            {
                gridMain.FocusedRowHandle = rowHandle;
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private int GetNextDays ()
        {
            string str = cmbNextDays.Text.ToUpper();
            int nextDays = 7;
            if (str == "14 DAYS")
                nextDays = 14;
            else if (str == "30 DAYS")
                nextDays = 30;
            return nextDays;
        }
        /***********************************************************************************************/
        private DataTable SetupGreenAndRed ( DataTable dt)
        {
            DateTime nextDate = DateTime.Now;
            DateTime today = DateTime.Now;

            if ( G1.get_column_number ( dt, "color") < 0 )
                dt.Columns.Add("color", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["color"] = 0D;

            if (G1.get_column_number(dt, "dateofbirth") < 0)
                dt.Columns.Add("dateofbirth");

            if (G1.get_column_number(dt, "funeralDeceased") < 0)
                dt.Columns.Add("funeralDeceased");

            DateTime dob = DateTime.Now;
            DateTime birth = DateTime.Now;
            int month = 0;
            int day = 0;
            int nextDays = GetNextDays();

            DataTable birthDt = dt.Clone();
            int lastRow = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                nextDate = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();

                if (nextDate.Year > 1000)
                {
                    //today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    if (nextDate < today)
                        dt.Rows[i]["color"] = 1D;  // Color.Pink;
                    else if (nextDate < today.AddDays(5))
                        dt.Rows[i]["color"] = 2D; // Color.LightGreen;
                    else
                        dt.Rows[i]["color"] = 3D;
                }
                else
                    dt.Rows[i]["color"] = 3D;

                dob = dt.Rows[i]["dob"].ObjToDateTime();
                month = dob.Month;
                day = dob.Day;

                birth = new DateTime(today.Year, month, day);
                if (birth >= today && birth <= today.AddDays(nextDays))
                {
                    //dt.Rows[i]["dateofbirth"] = "1";
                    //dt.Rows[i]["color"] = 5D;
                    birthDt.ImportRow(dt.Rows[i]);
                    lastRow = birthDt.Rows.Count;
                    birthDt.Rows[lastRow - 1]["notes"] = "Birthday Soon";
                }
            }

            dt = ProcessDeceased(dt, ref birthDt );

            if ( birthDt.Rows.Count > 0 )
            {
                for ( int i=0; i<birthDt.Rows.Count; i++)
                {
                    dt.ImportRow(birthDt.Rows[i]);
                    lastRow = dt.Rows.Count;
                    dt.Rows[lastRow-1]["color"] = 0;
                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "color asc, nextScheduledTouchDate asc";
            dt = tempview.ToTable();

            return dt;
        }
        /***********************************************************************************************/
        private DataTable ProcessDeceased(DataTable dt, ref DataTable birthDt )
        {
            string funeral = "";
            string cmd = "";
            DateTime deceasedDate = DateTime.Now;
            DataTable dx = null;
            if (G1.get_column_number(dt, "funeralDeceased") < 0)
                dt.Columns.Add("funeralDeceased");

            int nextDays = GetNextDays();
            int month = 0;
            int day = 0;
            DateTime dDay = DateTime.Now;
            DateTime today = DateTime.Now;
            int lastRow = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                funeral = dt.Rows[i]["referenceFuneral"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( funeral ))
                {
                    cmd = "Select * from `fcustomers` WHERE `serviceId` = '" + funeral + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                        if (deceasedDate.Year > 1000)
                        {
                            //dt.Rows[i]["funeralDeceased"] = deceasedDate.ToString("yyyy-MM-dd");
                            month = deceasedDate.Month;
                            day = deceasedDate.Day;

                            dDay = new DateTime(today.Year, month, day);
                            if (dDay >= today && dDay <= today.AddDays(nextDays))
                            {
                                birthDt.ImportRow(dt.Rows[i]);
                                lastRow = birthDt.Rows.Count;
                                birthDt.Rows[lastRow - 1]["notes"] = "DOD Anniversary";
                                birthDt.Rows[lastRow-1]["funeralDeceased"] = deceasedDate.ToString("yyyy-MM-dd");
                            }
                        }
                    }
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void CheckOldRecords ()
        {
            string cmd = "SELECT contacts_preneed.* FROM contacts_preneed WHERE `oldRecord` = '-1' ";
            cmd += " ORDER BY agent, funeralHome, prospectCreationDate ASC, nextScheduledTouchDate ASC, lastTouchDate ASC;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                dt = fixOldRecords(dt);
        }
        /***********************************************************************************************/
        private DataTable fixOldRecords ( DataTable dt )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "record asc, lastName asc,firstName asc, middleName ASC";
            dt = tempview.ToTable();

            DataRow[] dRows = null;
            string record = "";
            string oldRecord = "";
            string lastName = "";
            string firstName = "";
            string middlename = "";
            string prefix = "";
            string suffix = "";
            string fullName = "";
            string funeralHome = "";
            string agent = "";
            bool gotMod = true;
            if (G1.get_column_number(dt, "mod") < 0)
            {
                dt.Columns.Add("mod");
                gotMod = false;
            }

            this.Cursor = Cursors.WaitCursor;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                oldRecord = dt.Rows[i]["oldRecord"].ObjToString();
                if ( oldRecord == "-1")
                {
                    funeralHome = dt.Rows[i]["funeralHome"].ObjToString();
                    agent = dt.Rows[i]["agent"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    middlename = dt.Rows[i]["middleName"].ObjToString();
                    prefix = dt.Rows[i]["prefix"].ObjToString();
                    suffix = dt.Rows[i]["suffix"].ObjToString();
                    fullName = G1.BuildFullName(prefix, firstName, middlename, lastName, suffix);

                    dRows = dt.Select("lastName='" + lastName + "' AND firstName = '" + firstName + "' AND middleName = '" + middlename + "' AND prefix = '" + prefix + "' AND suffix = '" + suffix + "' AND funeralHome = '" + funeralHome + "' AND agent = '" + agent + "'" );
                    if ( dRows.Length > 0 )
                    {
                        for ( int j=0; j<dRows.Length; j++)
                        {
                            oldRecord = dRows[j]["oldRecord"].ObjToString();
                            if (oldRecord == "-1")
                            {
                                dRows[j]["oldRecord"] = record.ObjToInt32();
                                dRows[j]["mod"] = "Y";
                            }
                        }
                    }
                }
            }
            for ( int i = 0; i < dt.Rows.Count; i++)
            {
                if ( dt.Rows[i]["mod"].ObjToString().ToUpper() == "Y" )
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    oldRecord = dt.Rows[i]["oldRecord"].ObjToString();
                    G1.update_db_table("contacts_preneed", "record", record, new string[] {"oldRecord", oldRecord });
                }
            }

            if (!gotMod)
                dt.Columns.Remove("mod");

            this.Cursor = Cursors.Default;
            return dt;
        }
        /***********************************************************************************************/
        private DataTable GetCalendarTouches ()
        {
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            string date2 = G1.DateTimeToSQLDateTime(date);

            string employee = cmbEmployee.Text.Trim();
            string location = cmbLocation.Text.Trim();
            if (location.Trim().ToUpper() == "ALL")
                location = "";
            string searchBy = cmbSearch.Text;

            //string cmd = "SELECT contacts_preneed.* FROM contacts_preneed INNER JOIN(SELECT agent, funeralHome, MAX(prospectCreationDate) AS latest FROM contacts_preneed GROUP BY agent, funeralHome) r ON contacts_preneed.prospectCreationDate = r.latest AND contacts_preneed.agent = r.agent ";
            string cmd = "SELECT contacts_preneed.* FROM contacts_preneed ";
            //cmd += " WHERE ";

            bool needWhere = true;

            if (chkUseDates.Checked)
            {
                cmd += " WHERE ";
                if (searchBy == "Creation Date")
                    cmd += " `prospectCreationDate` >= '" + date1 + "' AND `prospectCreationDate` <= '" + date2 + "' ";
                else if (searchBy == "Last Touch Date")
                    cmd += " `lastTouchDate` >= '" + date1 + "' AND `lastTouchDate` <= '" + date2 + "' ";
                else if (searchBy == "Next Touch Date")
                    cmd += " `nextScheduledTouchDate` >= '" + date1 + "' AND `nextScheduledTouchDate` <= '" + date2 + "' ";

                if (!String.IsNullOrWhiteSpace(employee) && employee.ToUpper() != "ALL")
                    cmd += " AND contacts_preneed.`agent` = '" + employee + "' ";
                if (!String.IsNullOrWhiteSpace(location))
                    cmd += " AND  contacts_preneed.`funeralHome` = '" + location + "' ";
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(employee) && employee.ToUpper() != "ALL")
                {
                    cmd += " WHERE contacts_preneed.`agent` = '" + employee + "' ";
                    if (!String.IsNullOrWhiteSpace(location))
                        cmd += " AND  contacts_preneed.`funeralHome` = '" + location + "' ";
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(location))
                        cmd += " WHERE contacts_preneed.`funeralHome` = '" + location + "' ";
                }
            }

            if (cmd.ToUpper().IndexOf("WHERE") < 0)
                cmd += " WHERE ";
            else
                cmd += " AND ";
            cmd += "`lastTouchDate` > '1000-01-01' OR `nextScheduledTouchDate` > '1000-01-01' ";

            if (searchBy == "Creation Date")
                cmd += " ORDER BY agent, funeralHome, prospectCreationDate;";
            else if (searchBy == "Last Touch Date")
                cmd += " ORDER BY agent, funeralHome, lastTouchDate;";
            else if (searchBy == "Next Touch Date")
                cmd += " ORDER BY agent, funeralHome, nextScheduledTouchDate;";
            else
                cmd += " ORDER BY agent, funeralHome, prospectCreationDate;";

            DataTable dt = G1.get_db_data(cmd);

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
        private void LoadDBTable(string dbTable, string dbField, DevExpress.XtraEditors.Repository.RepositoryItemComboBox combo)
        {
            if (String.IsNullOrWhiteSpace(dbTable))
                return;
            if (dbTable.ToUpper() == "NONE")
            {
                combo.Items.Clear();
                return;
            }
            DataTable rx = G1.get_db_data("Select * from `" + dbTable + "`;");

            if (dbTable.ToUpper() == "REF_RELATIONS")
            {
                DataView tempview = rx.DefaultView;
                tempview.Sort = "relationship asc";
                rx = tempview.ToTable();
            }
            combo.Items.Clear();

            string name = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                name = rx.Rows[i][dbField].ToString().Trim();
                if (String.IsNullOrWhiteSpace(name))
                    continue;
                combo.Items.Add(name);
            }
        }
        /***********************************************************************************************/
        private void LoadEmployees()
        {
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string name = "";

            repositoryItemComboBox2.Items.Clear();

            string cmd = "Select * from `agents` WHERE `agentCode` = 'XYZZY'";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            AddOtherAgents(dt);
            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName,firstName";
            dt = tempview.ToTable();

            dt.Columns.Add("name");
            dt.Columns.Add("username");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                //middleName = dt.Rows[i]["middleName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();

                if (String.IsNullOrWhiteSpace(lastName))
                    continue;

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                //if (!String.IsNullOrWhiteSpace(middleName))
                //    name += " " + middleName;

                //cmbEmployee.Items.Add(name);

                repositoryItemComboBox2.Items.Add(name);
                dt.Rows[i]["name"] = name;
            }

            DataRow dR = dt.NewRow();
            dR["name"] = "All";
            dt.Rows.InsertAt(dR, 0);

            cmbEmployee.DataSource = dt;

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dRows = dx.Select("username='" + LoginForm.username + "'");
            if (dRows.Length > 0 && !G1.isAdminOrSuper() )
            {
                firstName = dRows[0]["firstName"].ObjToString();
                //middleName = dRows[0]["middleName"].ObjToString();
                lastName = dRows[0]["lastName"].ObjToString();

                name = lastName;
                if (!String.IsNullOrWhiteSpace(firstName))
                    name += ", " + firstName;
                //if (!String.IsNullOrWhiteSpace(middleName))
                //    name = " " + middleName;

                cmbEmployee.Text = name;
                primaryName = name;
                gridMain.Columns["agent"].Visible = false;
                dt.Rows.Clear();
                repositoryItemComboBox2.Items.Clear();
                dR = dt.NewRow();
                dR["name"] = name;
                dR["username"] = dRows[0]["username"].ObjToString();
                dt.Rows.Add(dR);
                cmbEmployee.DataSource = dt;
                gridMain.Columns["agent"].Visible = false;
                showAgent = false;
            }
            else
                gridMain.Columns["agent"].Visible = true;
            cmbEmployee.Text = primaryName;
        }
        /***********************************************************************************************/
        private void LoadEmployeesx ()
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

            dt = AddOtherAgents(dt);

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
            if ( dRows.Length > 0 && !superuser )
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
                dt.Rows.Clear();
                repositoryItemComboBox2.Items.Clear();
                dR = dt.NewRow();
                dR["name"] = name;
                dt.Rows.Add(dR);
                cmbEmployee.DataSource = dt;
                gridMain.Columns["agent"].Visible = false;
                showAgent = false;
            }
            else
                gridMain.Columns["agent"].Visible = true;
            cmbEmployee.Text = primaryName;

        }
        /***********************************************************************************************/
        private DataTable AddOtherAgents ( DataTable dt )
        {
            string cmd = "Select * from `agents`;";
            DataTable agentDt = G1.get_db_data(cmd);

            string firstName = "";
            string lastName = "";
            DataRow[] dRows = null;
            DataRow dRow = null;
            bool found = false;
            for ( int i=0; i<agentDt.Rows.Count; i++)
            {
                firstName = agentDt.Rows[i]["firstName"].ObjToString();
                lastName = agentDt.Rows[i]["lastName"].ObjToString();

                dRows = dt.Select("firstName='" + firstName + "' AND `lastName` = '" + lastName + "'");
                if ( dRows.Length <= 0 )
                {
                    dRow = dt.NewRow();
                    dRow["firstName"] = firstName;
                    dRow["lastName"] = lastName;
                    dt.Rows.Add(dRow);
                    found = true;
                }
            }

            if ( found )
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "lastName asc, firstName asc";
                dt = tempview.ToTable();
            }
            return dt;
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
            gridBand2.Fixed = FixedStyle.Left;
            //gridMain.Columns["amountDiscount"].Visible = false;
            //gridMain.Columns["contractNumber"].Visible = false;
        }
        /***********************************************************************************************/
        private void LoadContactTypes ()
        {
            repositoryItemComboBox1.Items.Clear();
            cmbContactType.Items.Clear();
            cmbContactType.Items.Add("All");

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

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contactType = dt.Rows[i]["contactType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contactType))
                {
                    repositoryItemComboBox1.Items.Add(contactType);
                    cmbContactType.Items.Add(contactType);

                    dRow = typeDt.NewRow();
                    dRow["contactType"] = contactType;
                    typeDt.Rows.Add(dRow);
                }

                category = dt.Rows[i]["category"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( category))
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
                    if ( dRows.Length <= 0 )
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

            //if (catDt.Rows.Count > 0)
            //    typeDt.Merge(catDt);

            DataView tempview = catDt.DefaultView;
            tempview.Sort = "order asc";
            catDt = tempview.ToTable();


            chkContactType.Properties.DataSource = catDt;

            allType = typeDt;

            cmbContactType.Text = "All";
            trackDt = G1.get_db_data("Select * from `track`;");
            ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
        }
        /***********************************************************************************************/
        private void LoadLocations()
        {
            string location = "";
            DataTable locDt = null;

            string cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable usersDt = G1.get_db_data(cmd);
            if (usersDt.Rows.Count > 0 && !superuser )
            {
                cmbLocation.Items.Add("All");
                string assignedLocations = usersDt.Rows[0]["assignedLocations"].ObjToString();
                if (!String.IsNullOrWhiteSpace ( assignedLocations ) )
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
                for ( int i=0; i<locDt.Rows.Count; i++)
                {
                    cmbLocation.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
                }
                //DataRow dRow = locDt.NewRow();
                //dRow["LocationCode"] = "All";
                //locDt.Rows.InsertAt(dRow, 0);
                //cmbLocation.DataSource = locDt;
            }
            cmbLocation.Text = "All";

            DataRow dRow = null;
            locDt = new DataTable();
            locDt.Columns.Add("locationCode");

            for ( int i=0; i<cmbLocation.Items.Count; i++)
            {
                location = cmbLocation.Items[i].ObjToString();
                if (location == "All")
                    continue;
                dRow = locDt.NewRow();
                dRow["locationCode"] = location;
                locDt.Rows.Add(dRow);
            }

            chkLocations.Properties.DataSource = locDt;
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
            if ( chkDoNotCall.Checked )
            {
                string status = dt.Rows[row]["contactStatus"].ObjToString().ToUpper();
                if (status == "DO NOT CALL" )
                {
                    e.Visible = false;
                    e.Handled = true;
                }
                else if ( status.IndexOf ( "ALREADY") >= 0 )
                {
                    e.Visible = false;
                    e.Handled = true;
                }
                else if (status == "DECEASED")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }

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

            DataTable locDt = (DataTable) chkLocations.Properties.DataSource;

            found = false;
            what = chkLocations.Text.Trim();
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
                    if ( funeralHome == category )
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

            if ( btnShowAnniversary.BackColor == Color.PaleGreen )
            {
                if (e.Visible)
                {
                    string showWhat = cmbAnniversary.Text.ToUpper();
                    DateTime today = DateTime.Now;
                    DateTime dob = dt.Rows[row]["dob"].ObjToDateTime();
                    if (showWhat.ToUpper() == "DECEASED ANNIVERSARY")
                    {
                        if (G1.get_column_number(dt, "funeralDeceased") < 0)
                            return;
                        dob = dt.Rows[row]["funeralDeceased"].ObjToDateTime();
                    }
                    int month = dob.Month;
                    int day = dob.Day;

                    int days = 7;
                    string str = cmbNextDays.Text.ToUpper();
                    if (str == "14 DAYS")
                        days = 14;
                    else if (str == "30 DAYS" )
                        days = 30;

                    DateTime birth = new DateTime(today.Year, month, day);
                    if (birth < today || birth > today.AddDays(days))
                    {
                        e.Visible = false;
                        e.Handled = true;
                    }
                }
            }
        }
        /****************************************************************************************/
        private bool CheckRowFiltered( int row )
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
            if (chkDoNotCall.Checked)
            {
                string status = dt.Rows[row]["contactStatus"].ObjToString().ToUpper();
                if (status == "DO NOT CALL")
                {
                    return true;
                }
                else if (status.IndexOf("ALREADY") >= 0)
                {
                    return true;
                }
                else if (status == "DECEASED")
                {
                    return true;
                }
            }

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
                    return true;
                }
            }

            DataTable locDt = (DataTable)chkLocations.Properties.DataSource;

            found = false;
            what = chkLocations.Text.Trim();
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
                DateTime nextDate = dt.Rows[row]["nextScheduledTouchDate"].ObjToDateTime();

                if ( e.Column.FieldName.ToUpper() == "REFERENCETRUST")
                {
                    string contractNumber = e.DisplayText.ToUpper();
                    if (contractNumber.IndexOf("SX") == 0)
                        e.DisplayText = "";
                }

                bool doDate = false;
                if (e.Column.FieldName == "apptDate")
                    doDate = true;
                else if (e.Column.FieldName == "nextScheduledTouchDate")
                    doDate = true;
                else if (e.Column.FieldName == "dob")
                    doDate = true;

                DateTime today = DateTime.Now;

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
                            if (e.Column.FieldName == "nextScheduledTouchDate")
                            {
                                //if (date.Year > 1000)
                                //{
                                //    today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                                //    if (date < today)
                                //        e.Appearance.BackColor = Color.Pink;
                                //    else if (date <= DateTime.Now)
                                //        e.Appearance.BackColor = Color.LightGreen;
                                //}
                            }
                        }
                    }
                }
                if (nextDate.Year < 1000)
                    return;
                today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if ( nextDate < today )
                {
                    //if ( nextDate < today.AddDays ( -7))
                    //    e.Appearance.BackColor = Color.Pink;
                    //else
                    //    e.Appearance.BackColor = Color.LightGreen;
                    //e.Appearance.BackColor = Color.Pink;
                }
                //else if ( nextDate < today.AddDays ( 5 ))
                //    e.Appearance.BackColor = Color.LightGreen;

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

            try
            {
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                DataRow dr = gridMain.GetFocusedDataRow();
                if (dr == null)
                    return;

                dr["mod"] = "Y";
                //btnSave.Show();
                //btnSave.Refresh();

                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName;
                if (currentColumn.ToUpper() == "RESULTS")
                {
                }
                if (currentColumn.ToUpper() == "NUM")
                    return;

                string what = dr[currentColumn].ObjToString();
                string record = dr["record"].ObjToString();

                if (currentColumn.ToUpper() == "FIRSTNAME")
                    Update_PreNeed(record, "firstName", what);
                else if (currentColumn.ToUpper() == "LASTNAME")
                    Update_PreNeed(record, "lastName", what);
                else if (currentColumn.ToUpper() == "MIDDLENAME")
                    Update_PreNeed(record, "middleName", what);
                else if (currentColumn.ToUpper() == "FUNERALHOME")
                {
                    if ( !ValidateFuneralHome ( what ))
                    {
                        dr[currentColumn] = oldWhat;
                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                    }
                    else
                        Update_PreNeed(record, "funeralHome", what);
                }
                else
                {
                    try
                    {
                        Update_PreNeed(record, currentColumn, what);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                if (e == null)
                    return;
                if (e.Column.FieldName.ToUpper() == "ZIP")
                {
                    string zipCode = dr["zip"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(zipCode))
                    {
                        string city = "";
                        string state = "";
                        string county = "";
                        bool rv = FunFamilyNew.LookupZipcode(zipCode, ref city, ref state, ref county);
                        if (rv)
                        {
                            if (!String.IsNullOrWhiteSpace(state))
                            {
                                string cmd = "Select * from `ref_states` where `state` = '" + state + "';";
                                DataTable dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                    state = dx.Rows[0]["abbrev"].ObjToString();
                            }
                            if (!String.IsNullOrWhiteSpace(city))
                                dr["city"] = city;
                            if (!String.IsNullOrWhiteSpace(state))
                                dr["state"] = state;
                            //if (!String.IsNullOrWhiteSpace(county))
                            //    dr["county"] = county;
                        }
                    }
                }
                if (e.Column.FieldName.ToUpper() == "NEXTTOUCHRESULT")
                {
                    moveNextToLast();
                }
                else if (e.Column.FieldName.ToUpper() == "CONTACTSTATUS")
                {
                    if (what == "Presentation Made, Sold, Finalized")
                    {
                        dr["completed"] = "1";
                        Update_PreNeed(record, "completed", "1");
                        gridMain.RefreshEditor(true);
                        dgv.Refresh();
                    }
                    else
                    {
                        dr["completed"] = "";
                        Update_PreNeed(record, "completed", "");
                        gridMain.RefreshEditor(true);
                        dgv.Refresh();
                    }
                }
                else if (e.Column.FieldName.ToUpper() == "DOB")
                {
                    if (G1.validate_date(what))
                    {
                        DateTime bDay = what.ObjToDateTime();
                        int age = G1.CalculateAgeCorrect(bDay, DateTime.Now);
                        dr["age"] = age;
                    }
                }
                gridMain.RefreshData();
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void Update_PreNeed ( string record, string field, string data )
        {
            try
            {
                if (String.IsNullOrWhiteSpace(record))
                    return;
                if (!String.IsNullOrWhiteSpace(record))
                {
                    if (field.ToUpper() == "FUNERALHOME")
                    {
                        if (!ValidateFuneralHome(data))
                            return;
                    }

                    G1.update_db_table("contacts_preneed", "record", record, new string[] { field, data });
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool ValidateFuneralHome ( string data )
        {
            bool found = false;
            string funeralHome = "";
            for ( int i=0; i<cmbLocation.Items.Count; i++)
            {
                funeralHome = cmbLocation.Items[i].ObjToString().Trim();
                if ( funeralHome == data )
                {
                    found = true;
                    break;
                }
            }
            return found;
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
            if (agent == primaryName || G1.isAdmin() || G1.isHR() )
            {
                DialogResult result = MessageBox.Show("Do you want to RELEASE This Contact for other Agents ","Release Preneed Contact Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if ( result == DialogResult.Yes )
                {
                    string record = dr["record"].ObjToString();
                    G1.update_db_table("contacts_preneed", "record", record, new string[] { "contactStatus","Released","Agent","", "addContact", "" });
                    //G1.delete_db_table("contacts_preneed", "record", record);

                    //dt = (DataTable)dgv.DataSource;
                    //int row = gridMain.GetDataSourceRowIndex(gridMain.FocusedRowHandle);
                    try
                    {
                        dt.Rows.RemoveAt(row);
                        dt.AcceptChanges();
                        //gridMain.DeleteRow(gridMain.FocusedRowHandle);
                    }
                    catch (Exception ex)
                    {
                    }
                    dt.AcceptChanges();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                    dgv.RefreshDataSource();
                    dgv.Refresh();


                    //LoadData();
                }
                else
                {
                    dr["contactStatus"] = "Do Not Call";
                    string record = dr["record"].ObjToString();
                    G1.update_db_table("contacts_preneed", "record", record, new string[] {"contactStatus", "Do Not Call" });
                    MessageBox.Show("Contact Status Changed to 'Do Not Call'", "Delete Contact Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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

            string cmd = "DELETE from `contacts_preneed` WHERE `agent` = '-1'";
            G1.get_db_data(cmd);

            string record = G1.create_record("contacts_preneed", "agent", "-1");
            if (G1.BadRecord("contacts_preneed", record))
                return;

            string agent = cmbEmployee.Text.Trim();
            //string contactType = cmbContactType.Text.Trim();

            string location = cmbLocation.Text.Trim();

            DateTime date = DateTime.Now;
            string apptDate = date.ToString("yyyy-MM-dd");
            G1.update_db_table("contacts_preneed", "record", record, new string[] { "agent", agent, "prospectCreationDate", apptDate, "funeralHome", location, "totalTouches", "0", "oldRecord", record });

            DataRow dRow = dt.NewRow();
            DateTime now = DateTime.Now;
            dRow["record"] = record;
            dRow["oldRecord"] = record;
            //dRow["apptDate"] = G1.DTtoMySQLDT(date);
            dRow["prospectCreationDate"] = G1.DTtoMySQLDT(date);
            dRow["mod"] = "Y";
            dRow["completed"] = "0";
            //dRow["contactType"] = contactType;
            dRow["agent"] = cmbEmployee.Text.Trim();
            dRow["agentName"] = cmbEmployee.Text.Trim();
            dRow["funeralHome"] = location;
            dt.Rows.InsertAt(dRow, 0);
            //dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            //G1.GoToLastRow(gridMain);
            //G1.GoToFirstRow(gridMain);

            gridMain_CellValueChanged(null, null);

            newAddition = true;

            using (editDG editForm = new editDG(gridMain, dt, 0, record))
            {
                editForm.editDone += EditForm_editDone;
                editForm.ShowDialog();
            }
        }
        private bool newAddition = false;
        /****************************************************************************************/
        private void EditForm_editDone(DataTable dx, int row, string CancelStatus )
        {
            if (CancelStatus == "YES")
            {
                if ( newAddition )
                {
                    DialogResult result = MessageBox.Show("***Question***\nNew Contact!\nWould you like to SAVE your New Contact?", "New Contact Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        newAddition = false;
                        DataTable dxx = (DataTable)dgv.DataSource;
                        string rec = dxx.Rows[row]["record"].ObjToString();
                        G1.delete_db_table("contacts_preneed", "record", rec );

                        gridMain.DeleteRow(row);
                        gridMain.RefreshData();
                        gridMain.RefreshEditor(true);
                        return;
                    }
                }
            }
            else if (String.IsNullOrWhiteSpace(CancelStatus))
            {
                if (newAddition)
                {
                    DialogResult result = MessageBox.Show("***Question***\nNew Contact!\nWould you like to SAVE your New Contact?", "New Contact Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        newAddition = false;
                        DataTable dxx = (DataTable)dgv.DataSource;
                        string rec = dxx.Rows[row]["record"].ObjToString();
                        G1.delete_db_table("contacts_preneed", "record", rec);

                        gridMain.DeleteRow(row);
                        gridMain.RefreshData();
                        gridMain.RefreshEditor(true);
                        return;
                    }
                }
            }

            PleaseWait pleaseForm = new PleaseWait("Please Wait!\nUpdating Contact!");
            pleaseForm.Show();
            pleaseForm.Refresh();

            DataTable dt = (DataTable)dgv.DataSource;
            string caption = "";
            string data = "";
            string field = "";
            string type = "";
            string record = dt.Rows[row]["record"].ObjToString();
            string modList = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                caption = dx.Rows[i]["field"].ObjToString();
                field = dx.Rows[i]["actualField"].ObjToString();
                if ( field.ToUpper() == "RESULTS")
                {
                }
                data = dx.Rows[i]["data"].ObjToString();
                if (G1.get_column_number(dt, field) >= 0)
                {
                    try
                    {
                        type = dt.Columns[field].DataType.ToString().ToUpper();
                        if (type.IndexOf("MYSQLDATETIME") >= 0)
                            dt.Rows[row][field] = G1.DTtoMySQLDT(data);
                        else if (type.IndexOf("DOUBLE") >= 0)
                            dt.Rows[row][field] = data.ObjToDouble();
                        else if (type.IndexOf("DECIMAL") >= 0)
                            dt.Rows[row][field] = data.ObjToDecimal();
                        else if (type.IndexOf("INT32") >= 0)
                            dt.Rows[row][field] = data.ObjToInt32();
                        else if (type.IndexOf("INT64") >= 0)
                            dt.Rows[row][field] = data.ObjToInt64();
                        else
                        {
                            dt.Rows[row][field] = data.ToString();
                            if ( data.IndexOf (",") >= 0 )
                            {
                                G1.update_db_table("contacts_preneed", "record", record, new string [] { field, data } );
                                continue;
                            }
                        }
                        if (String.IsNullOrWhiteSpace(data))
                            data = "NODATA";
                        modList += field + "," + data + ",";
                    }
                    catch ( Exception ex)
                    {
                    }
                    //dt.Rows[row][field] = data;
                }
            }
            modList = modList.TrimEnd(',');
            G1.update_db_table("contacts_preneed", "record", record, modList);
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);

            PositionToRecord(dt, record );

            newAddition = false;

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /****************************************************************************************/
        private void PositionToRecord ( DataTable dt, string record, bool old = false )
        {
            if ( newAddition )
            {
                gridMain.SelectRow(0);
                gridMain.FocusedRowHandle = 0;
                gridMain.RefreshEditor(true);
                return;
            }

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
            string searchBy = cmbSearch.Text;
            if (String.IsNullOrWhiteSpace(agent))
                agent = cmbEmployee.Text;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                prefix = dt.Rows[i]["prefix"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                suffix = dt.Rows[i]["suffix"].ObjToString();
                agent = dt.Rows[i]["agent"].ObjToString();
                date = dt.Rows[i]["prospectCreationDate"].ObjToDateTime();
                if (searchBy == "Last Touch Date")
                    date = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                else if (searchBy == "Next Touch Date")
                    date = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                home = dt.Rows[i]["funeralHome"].ObjToString();
                name = prefix + " " + firstName + " " + middleName + " " + lastName + " " + suffix;

                extraName = agent + "~" + home + "~" + name + "~" + date.ToString("yyyyMMdd");
                dt.Rows[i]["extraName"] = extraName;
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "extraName asc";
            dt = tempview.ToTable();

            if (initialLoad)
                dt = SetupGreenAndRed(dt);

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);

            dt = (DataTable)dgv.DataSource;

            string oldRecord = "";
            if ( old )
            {
                gridMain.SelectRow(0);
                gridMain.FocusedRowHandle = 0;
                gridMain.RefreshEditor(true);
                return;
            }
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                oldRecord = dt.Rows[i]["record"].ObjToString();
                if (old)
                    oldRecord = dt.Rows[i]["oldRecord"].ObjToString();
                if ( oldRecord == record )
                {
                    gridMain.SelectRow(i);
                    gridMain.FocusedRowHandle = i;
                    gridMain.RefreshEditor(true);
                    break;
                }
            }
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
                        RepositoryItemComboBox cBox = (RepositoryItemComboBox) gridMain.Columns[i].ColumnEdit;
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
        public RepositoryItemComboBox FireEventGrabSomething(string what)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("item");
            DataRow dRow = null;
            string item = "";
            for ( int i=0; i< dgv.RepositoryItems.Count; i++)
            {
                try
                {
                    item = dgv.RepositoryItems[i].Name.Trim();
                    if (item == what)
                    {
                        return (DevExpress.XtraEditors.Repository.RepositoryItemComboBox)dgv.RepositoryItems[i];
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            return null;
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

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if ( currentColumn == "funeralHome")
            {
                //DataTable funDt = (DataTable)cmbLocation.DataSource;
                string funeralHome = dr["funeralHome"].ObjToString();
                if (!ValidateFuneralHome(funeralHome))
                {

                    string record = dr["record"].ObjToString();
                    string cmd = "Select * from `contacts_preneed` WHERE `record` = '" + record + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count >= 0)
                    {
                        funeralHome = dx.Rows[0]["funeralHome"].ObjToString();
                        dr["funeralHome"] = funeralHome;
                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                    }
                }
            }

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
            else if ( name.ToUpper() == "FUNERALHOME")
            {
                string str = e.DisplayText;
                if ( str.IndexOf ( "-" ) > 0 )
                {
                    int idx = str.IndexOf("-");
                    str = str.Substring(idx+1);
                    e.DisplayText = str.Trim();
                }
            }
            bool doDate = false;
            bool doTime = false;
            if (name == "apptDate")
                doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;
            else if (name == "nextScheduledTouchDate")
                doDate = true;
            if (doDate)
            {
                DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                if (date.Year < 30)
                    e.DisplayText = "";
                else
                    e.DisplayText = date.ToString("MM/dd/yyyy");
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
        PreneedContactHistory newHistoryForm = null;
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string record = dr["record"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string middleName = dr["middleName"].ObjToString();
            string location = dr["funeralHome"].ObjToString();

            string oldNotes = dr["notes"].ObjToString();

            using ( PreneedContactHistory historyForm = new PreneedContactHistory ( gridMain, dt, row, record, lastName, firstName, middleName, location, dr ))
            {
                newHistoryForm = historyForm;
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
                    if ( notes != oldNotes )
                    {
                        if (dRows.Length > 1)
                        {
                            for ( int i=0; i<dRows.Length; i++)
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
                        if ( oldNotes == "Birthday Soon" || oldNotes == "DOD Anniversary")
                        {
                            dr["notes"] = oldNotes;
                        }
                    }
                }
            }
            gridMain.FocusedColumn = gridMain.Columns["nextScheduledTouchDate"];
            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            dgv.Refresh();

            PositionToRow(record);
        }
        /****************************************************************************************/
        private void PositionToRow ( string record )
        {
            string rec = "";
            bool filtered = false;
            int row = -1;
            gridMain.ClearSelection();

            string firstName = "";
            string lastName = "";

            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
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
                catch ( Exception ex)
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
        private string HistoryForm_contactHistoryDone(DataTable dt, bool somethingDeleted)
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
            string notes = "";
            string completed = "";
            string nextCompleted = "";
            string mod = "";
            bool foundDelete = false;
            DataRow[] dRows = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                results = dt.Rows[i]["results"].ObjToString();
                notes = dt.Rows[i]["notes"].ObjToString();
                completed = dt.Rows[i]["completed"].ObjToString();
                nextCompleted = dt.Rows[i]["nextCompleted"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();

                dRows = dx.Select("record='" + record + "'");
                if ( dRows.Length > 0 )
                {
                    found = true;
                    if (mod == "D")
                    {
                        G1.delete_db_table("contacts_preneed", "record", record);

                        dx.Rows.Remove(dRows[0]);
                        G1.NumberDataTable(dx);
                        foundDelete = true;
                    }
                    else
                    {
                        DateTime nextDate = dRows[0]["nextScheduledTouchDate"].ObjToDateTime();
                        G1.copy_dr_row(dt.Rows[i], dRows[0] );
                        //dRows[0]["results"] = results;
                        //dRows[0]["completed"] = completed;
                        //dRows[0]["mod"] = mod;
                    }
                    if ( nextCompleted == "1")
                    {
                        addNextContactToolStripMenuItem_Click(null, null);
                        //gridMain_DoubleClick(null, null);
                    }
                }
            }

            if ( found )
            {
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }

            if ( somethingDeleted )
            {
                int rowHandle = gridMain.FocusedRowHandle;
                LoadData(rowHandle);
            }
            return nextCompleted;
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
            if (workAuto)
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

                RemoteProcessing.AutoRunSend(workReport + " for " + today.ToString("MM/dd/yyyy"), filename, workAgent, sendWhere, "", workEmail, sendUsername);
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
        private string CleanupReportName ( string report )
        {
            report = report.Replace (">=", "GE");
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
            string title = this.Text;
            if (isCustom)
                title = customReport;
            Printer.DrawQuad(6, 8, 8, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (record == "-1")
            {
                e.Valid = false;
                return;
            }

            if (view.FocusedColumn.FieldName.ToUpper().IndexOf("PHONE") > 0 )
            {
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string phone = e.Value.ObjToString();
                string newPhone = AgentProspectReport.reformatPhone(phone, true);
                e.Value = newPhone;
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DOB" )
            {
                string sDate = e.Value.ToString();
                if ( G1.validate_date ( sDate ))
                {
                    DateTime date = sDate.ObjToDateTime();
                    sDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4") + " 12:00:00";
                    //sDate = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");
                    //e.Value = sDate;
                    e.Value = G1.DTtoMySQLDT(date);
                    int rowhandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowhandle);
                    //dr["dob"] = (MySqlDateTime) G1.DTtoMySQLDT(date);
                    e.Valid = true;
                }
            }
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            //if ( e.Column.FieldName.ToUpper() == "FUNERALHOME") // Not Working
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    string funeralHome = view.GetRowCellValue(e.RowHandle, "funeralHome").ObjToString();
            //    if (!String.IsNullOrWhiteSpace(funeralHome))
            //    {
            //        DataTable locDt = (DataTable)cmbLocation.DataSource;
            //        if (locDt != null)
            //        {
            //            DataRow[] dRows = locDt.Select("locationCode='" + funeralHome + "'");
            //            if (dRows.Length <= 0)
            //            {
            //            }
            //        }
            //    }
            //}
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
            //if (name == "apptDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;

            if (name.ToUpper().IndexOf("DATE") >= 0)
                doDate = true;
            else if (name.ToUpper() == "DOB" )
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
                            DateTime date = myDate.ObjToDateTime();

                            dr[name] = G1.DTtoMySQLDT(myDate);
                            if ( name.ToUpper() == "DOB")
                            {
                                dr["age"] = G1.CalculateAgeCorrect(date, DateTime.Now);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        //dr[name] = G1.DTtoMySQLDT(myDate);
                        UpdateMod(dr);
                        gridMain_CellValueChanged(null, null);
                    }
                    else if ( dateForm.DialogResult == System.Windows.Forms.DialogResult.Cancel )
                    {
                        try
                        {
                            dr[name] = G1.DTtoMySQLDT("0000-00-00 00:00:00");
                            UpdateMod(dr);
                            gridMain_CellValueChanged(null, null);
                        }
                        catch ( Exception ex)
                        {
                        }
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
                try
                {
                    int rowHandle = hitInfo.RowHandle;
                    gridMain.SelectRow(rowHandle);
                    gridMain.RefreshEditor(true);
                    //dgv.RefreshDataSource();
                    DataTable dt = (DataTable)dgv.DataSource;

                    GridColumn column = hitInfo.Column;
                    currentColumn = column.FieldName.Trim();
                    DataRow dr = gridMain.GetFocusedDataRow();
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    string data = dt.Rows[row][currentColumn].ObjToString();

                    if (currentColumn.ToUpper() == "NOTES")
                    {
                        if (!String.IsNullOrWhiteSpace(data))
                        {
                        }
                    }

                    //if ( currentColumn == "contactName")
                    //{
                    //    this.Validate();
                    //    string contactType = dr["contactType"].ObjToString();
                    //    if (String.IsNullOrWhiteSpace(contactType))
                    //        return;
                    //    if (contactType == oldContactType)
                    //        return;
                    //    oldContactType = contactType;

                    //    string viewDetail = DetermineView(contactType);

                    //    string answer = "";
                    //    ciLookup.Items.Clear();
                    //    if (myDt == null)
                    //    {
                    //        myDt = new DataTable();
                    //        myDt.Columns.Add("stuff");
                    //    }
                    //    myDt.Rows.Clear();
                    //    string cmd = "Select * from `track` where `contactType` = '" + contactType + "';";
                    //    DataTable dx = G1.get_db_data(cmd);
                    //    for ( int i=0; i<dx.Rows.Count; i++)
                    //    {
                    //        answer = dx.Rows[i]["answer"].ObjToString();
                    //        if ( String.IsNullOrWhiteSpace ( answer))
                    //        {
                    //            if ( viewDetail.ToUpper() == "PERSON")
                    //            {
                    //                answer = GetPerson(dx.Rows[i]);
                    //            }
                    //        }
                    //        if ( !String.IsNullOrWhiteSpace ( answer ))
                    //            AddToMyDt(answer);
                    //    }

                    //    ciLookup.Items.Clear();
                    //    for (int i = 0; i < myDt.Rows.Count; i++)
                    //        ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());

                    //    gridMain.Columns[currentColumn].ColumnEdit = ciLookup;
                    //    gridMain.RefreshData();
                    //    gridMain.RefreshEditor(true);
                    //}
                }
                catch (Exception ex)
                {
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
            string cmd = "Select * from `contactTypes` WHERE `contactTypes` = '" + contactType + "';";
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
            if (loading)
                return;
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
            initialLoad = false;
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
            if (1 == 1)
                return;
            e.RowHeight = 18;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                Font f = gridMain.Appearance.Row.Font;
                int rowHeight = f.Height;
                string str = "";
                int maxLength = 0;
                int length = 0;
                int thisRow = gridMain.GetDataSourceRowIndex(e.RowHandle);
                int periods = 0;
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    //if (name == "CASH" || name == "DEPOSITNUMBER" || name == "CREDIT CARD" || name == "CCDEPOSITNUMBER")
                    doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                //viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                var junkstr = dt.Rows[thisRow][column.FieldName];
                                length = junkstr.ObjToString().Length;
                                maxLength = Math.Max(length, maxLength);
                                viewInfo.EditValue = junkstr;
                                int cnt = junkstr.ObjToString().Count(c => c == '.');
                                periods = Math.Max(cnt, periods);
                                //viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                //viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, gridMain.RowHeight);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, rowHeight);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                {
                    maxHeight += 18;
                    maxHeight += (periods/2) * 18;
                    if (maxHeight < 18)
                        maxHeight = 18;
                    else
                        e.RowHeight = maxHeight;
                }
                else
                    e.RowHeight = 18;
            }
        }

        private void gridMain_CalcRowHeightY(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                int finalHeight = 0;
                bool doit = false;
                string name = "";
                string str = "";
                int count = 0;
                string[] Lines = null;
                GridColumn column = gridMain.Columns["notes"];
                using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                {
                    using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                    {
                        str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            Lines = str.Split('\n');
                            count = Lines.Length + 1;
                        }
                        viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                        viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                        using (Graphics graphics = dgv.CreateGraphics())
                        using (GraphicsCache cache = new GraphicsCache(graphics))
                        {
                            viewInfo.CalcViewInfo(graphics);
                            var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                            newHeight = Math.Max(height, finalHeight);
                            if (newHeight > maxHeight)
                            {
                                maxHeight = newHeight * count;
                                if (maxHeight > 0 && maxHeight > e.RowHeight)
                                {
                                    if (maxHeight > finalHeight)
                                        finalHeight = maxHeight;
                                }
                            }
                        }
                    }
                }

                column = gridMain.Columns["results"];
                using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                {
                    using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                    {
                        str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            Lines = str.Split('\n');
                            count = Lines.Length + 1;
                        }
                        viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                        viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                        using (Graphics graphics = dgv.CreateGraphics())
                        using (GraphicsCache cache = new GraphicsCache(graphics))
                        {
                            viewInfo.CalcViewInfo(graphics);
                            var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                            newHeight = Math.Max(height, finalHeight);
                            if (newHeight > maxHeight)
                            {
                                maxHeight = newHeight * count;
                                if (maxHeight > 0 && maxHeight > e.RowHeight)
                                {
                                    if (maxHeight > finalHeight)
                                        finalHeight = maxHeight;
                                }
                            }
                        }
                    }
                }
                if (finalHeight > 0 && finalHeight > e.RowHeight)
                    e.RowHeight = finalHeight;
            }
        }
        private void gridMain_CalcRowHeightx(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                int finalHeight = 0;
                bool doit = false;
                string name = "";
                string str = "";
                int count = 0;
                string[] Lines = null;
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if (name == "RESULTS" || name == "NOTES")
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    Lines = str.Split('\n');
                                    count = Lines.Length + 1;
                                }
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, finalHeight);
                                    if (newHeight > maxHeight)
                                    {
                                        maxHeight = newHeight * count;
                                        if (maxHeight > 0 && maxHeight > e.RowHeight)
                                        {
                                            if (maxHeight > finalHeight)
                                                finalHeight = maxHeight;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (finalHeight > 0 && finalHeight > e.RowHeight)
                    e.RowHeight = finalHeight;
            }
        }
        /****************************************************************************************/
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string locaton = cmbLocation.Text.Trim();
            //LoadEmployees();
            LoadData();
        }
        /****************************************************************************************/
        private void addNextContactToolStripMenuItem_Click(object sender, EventArgs e)
        { // Remove this from being accessed, Duplicate Contacts given next contact completed xyzzy
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dR = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string rec = dR["record"].ObjToString();
            DateTime nextTouchDate = dR["nextScheduledTouchDate"].ObjToDateTime();
            string nextTouchTime = dR["nextScheduledTouchTime"].ObjToString();
            string scheduledActivity = dR["scheduledActivity"].ObjToString();
            string nextTouchResult = dR["nextTouchResult"].ObjToString();
            string contactLevel = dR["contactStatus"].ObjToString();
            DateTime prospectCreationDate = dR["prospectCreationDate"].ObjToDateTime();
            int totalTouches = dR["totalTouches"].ObjToInt32();
            contactLevel = "";

            DataTable oldDt = G1.get_db_data("Select * from `contacts_preneed` WHERE `record` = '" + rec + "';");

            string cmd = "DELETE from `contacts_preneed` WHERE `agent` = '-1'";
            G1.get_db_data(cmd);

            string record = G1.create_record("contacts_preneed", "agent", "-1");
            if (G1.BadRecord("contacts_preneed", record))
                return;

            this.Cursor = Cursors.WaitCursor;
            string cName = "";
            string data = "";
            string str = "";

            for (int i = 0; i < oldDt.Columns.Count; i++)
            {
                cName = oldDt.Columns[i].ColumnName.ToString();
                if (cName.ToUpper() == "RECORD")
                    continue;
                data = oldDt.Rows[0][cName].ObjToString();
                str = cName + "," + data;
                G1.update_db_table("contacts_preneed", "record", record, str);
            }

            DateTime now = DateTime.Now;
            string cDate = now.ToString("yyyyMMdd");
            string agent = oldDt.Rows[0]["agent"].ObjToString();

            string firstName = dR["firstName"].ObjToString();
            string middleName = dR["middleName"].ObjToString();
            string lastName = dR["lastName"].ObjToString();
            string prefix = dR["prefix"].ObjToString();
            string suffix = dR["suffix"].ObjToString();
            string oldRecord = dR["oldRecord"].ObjToString();

            cmd = "Select * from `contacts_preneed` where `oldRecord` = '" + oldRecord + "';";
            DataTable ddx = G1.get_db_data(cmd);

            G1.update_db_table("contacts_preneed", "record", record, new string[] { "prospectCreationDate", prospectCreationDate.ToString("yyyy-MM-dd"), "agent", agent, "totalTouches", ddx.Rows.Count.ToString(), "oldRecord", oldRecord , "totalTouches", totalTouches.ToString()});

            //G1.update_db_table("contacts_preneed", "record", record, new string[] { "lastTouchDate", nextTouchDate.ToString("yyyyMMdd"), "lastTouchTime", nextTouchTime, "lastTouchActivity", scheduledActivity, "lastTouchResult",  nextTouchResult, "notes", "", "contactStatus", "","scheduledActivity", "Something New" });
            G1.update_db_table("contacts_preneed", "record", record, new string[] { "lastTouchDate", nextTouchDate.ToString("yyyyMMdd"), "lastTouchTime", nextTouchTime, "lastTouchActivity", scheduledActivity, "lastTouchResult", nextTouchResult, "notes", "", "contactStatus", "", "scheduledActivity", "", "nextTouchResult", "", "nextScheduledTouchDate", "0001-01-01", "nextScheduledTouchTime", "" });

            this.Cursor = Cursors.Default;

            DataTable dx = (DataTable)dgv.DataSource; // xyzzy
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                if ( record == oldRecord )
                {
                    rowHandle = i;
                    break;
                }
            }


            LoadData( rowHandle, oldRecord );

            dx = (DataTable)dgv.DataSource; // xyzzy
            if ( dx.Rows.Count > 0 )
            {
                if (newHistoryForm != null)
                    newHistoryForm.FireEventModified(dx);
            }

            DataRow [] dRows = dx.Select("record='" + rec + "'");
            if ( dRows.Length > 0 )
            {
            }

        }
        /****************************************************************************************/
        private void moveNextToLast ()
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dR = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string rec = dR["record"].ObjToString();
            DateTime nextTouchDate = dR["nextScheduledTouchDate"].ObjToDateTime();
            string nextTouchTime = dR["nextScheduledTouchTime"].ObjToString();
            string nextTouchResult = dR["nextTouchResult"].ObjToString();
            string scheduledActivity = dR["scheduledActivity"].ObjToString();

            DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            if (now < nextTouchDate)
                return;

            this.Cursor = Cursors.WaitCursor;
            string cName = "";
            string data = "";
            string str = "";

            now = DateTime.Now;
            string cDate = now.ToString("yyyyMMdd");

            string firstName = dR["firstName"].ObjToString();
            string middleName = dR["middleName"].ObjToString();
            string lastName = dR["lastName"].ObjToString();
            string prefix = dR["prefix"].ObjToString();
            string suffix = dR["suffix"].ObjToString();
            string record = dR["record"].ObjToString();
            string agent = dR["agent"].ObjToString();

            dR["lastTouchDate"] = G1.DTtoMySQLDT(nextTouchDate);
            dR["lastTouchTime"] = nextTouchTime;
            dR["lastTouchResult"] = nextTouchResult;
            dR["lastTouchActivity"] = scheduledActivity;

            G1.update_db_table("contacts_preneed", "record", record, new string[] { "lastTouchDate", nextTouchDate.ToString("yyyyMMdd"), "lastTouchTime", nextTouchTime, "lastTouchActivity", scheduledActivity, "lastTouchResult", nextTouchResult });

            gridMain.RefreshEditor(true);
            dgv.Refresh();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void chkExcludeCompleted_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnLeadList_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ContactsLeadList leadForm = new ContactsLeadList();
            leadForm.contactListDone += LeadForm_contactListDone;
            leadForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void LeadForm_contactListDone(bool updated)
        {
            if (updated)
            {
                LoadData();
                gridMain.Columns["prospectCreationDate"].SortMode = DevExpress.XtraGrid.ColumnSortMode.Value;
                gridMain.Columns["prospectCreationDate"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
                gridMain.Columns["prospectCreationDate"].SortIndex = 0;
                gridMain.Columns["prospectCreationDate"].OptionsColumn.ImmediateUpdateRowPosition = DefaultBoolean.True;
                gridMain.RefreshEditor(true);
                G1.GoToFirstRow(gridMain);
            }
        }
        /****************************************************************************************/
        private void btnCalendar_Click(object sender, EventArgs e)
        {
            string employee = cmbEmployee.Text.Trim();
            if (employee.Trim().ToUpper() == "ALL")
                employee = "";
            string location = cmbLocation.Text.Trim();
            if (location.Trim().ToUpper() == "ALL")
                location = "";

            double open = 0D;
            double high = 0D;
            double low = 0D;
            double close = 0D;
            double volume = 0D;
            DateTime stockDate = DateTime.Now;

            //get_stock_price("ibm", ref stockDate, ref open, ref high, ref low, ref close, ref volume);

            string agent = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string prefix = "";
            string suffix = "";
            string name = "";
            DateTime createDate = DateTime.Now;
            DateTime lastTouchDate = DateTime.Now;
            DateTime nextTouchDate = DateTime.Now;
            string lastTouchTime = "";
            string result = "";

            DateTime date = DateTime.Now;

            string searchBy = cmbSearch.Text.Trim();
            //DateTime lastTouchTime = DateTime.Now;

            PleaseWait pleaseForm = new PleaseWait("Please Wait!\nPreparing Calender!");
            pleaseForm.Show();
            pleaseForm.Refresh();

            GoogleCalendarManager.InitCalander(employee);

            DataTable dt = (DataTable)dgv.DataSource;

            DataTable ddd = GetCalendarTouches(); // This is much faster if it works okay
            dt = ddd.Copy();

            employee = "Robby";
            name = "Robby";
            location = "Bay Springs";
            result = "Play Ball";
            date = new DateTime(2024, 12, 20);

            //GoogleCalendarManager.AddCalanderEvent("Pre-Need", employee, name + " " + "Next Touch Date", location, result, date, date);
            //if (1 == 1)
            //    return;

            //GoogleCalendarManager.GetRobbyEvents();

            //GoogleCalendarManager.AddRobbyEvent();

            //GoogleCalendarManager.GetCalendarService();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                prefix = dt.Rows[i]["prefix"].ObjToString();
                suffix = dt.Rows[i]["suffix"].ObjToString();
                name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);
                result = "";

                //if (searchBy == "Creation Date")
                //    date = dt.Rows[i]["prospectCreationDate"].ObjToDateTime();
                //else if (searchBy == "Last Touch Date")
                //{
                //    date = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                //    lastTouchTime = dt.Rows[i]["lastTouchTime"].ObjToString();
                //    string strDateIn = date.ToString("MM/dd/yyyy") + " " + lastTouchTime;
                //    string strDateOut = "";
                //    decodeDateTime(strDateIn, ref date, ref strDateOut);

                //    result = dt.Rows[i]["lastTouchResult"].ObjToString();
                //}
                //else if (searchBy == "Next Touch Date")
                //{
                //    date = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                //    result = dt.Rows[i]["scheduledActivity"].ObjToString();
                //}

                date = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                if (date.Year > 100)
                {
                    lastTouchTime = dt.Rows[i]["lastTouchTime"].ObjToString();
                    string strDateIn = date.ToString("MM/dd/yyyy") + " " + lastTouchTime;
                    string strDateOut = "";
                    decodeDateTime(strDateIn, ref date, ref strDateOut);

                    result = dt.Rows[i]["lastTouchResult"].ObjToString();

                    GoogleCalendarManager.AddCalanderEvent("Pre-Need", employee, name + " " + "Last Touch Date", location, result, date, date);
                }
                date = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                if (date.Year > 100)
                {
                    lastTouchTime = dt.Rows[i]["nextScheduledTouchTime"].ObjToString();
                    string strDateIn = date.ToString("MM/dd/yyyy") + " " + lastTouchTime;
                    string strDateOut = "";
                    decodeDateTime(strDateIn, ref date, ref strDateOut);

                    result = dt.Rows[i]["scheduledActivity"].ObjToString();
                    GoogleCalendarManager.AddCalanderEvent("Pre-Need", employee, name + " " + "Next Touch Date", location, result, date, date);
                }
            }

            Calendar3 calendarForm = new Calendar3( dateTimePicker1.Value, searchBy );
            calendarForm.Show();

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /****************************************************************************************/
        private void btnCalendar_Clickx(object sender, EventArgs e)
        {
            string employee = cmbEmployee.Text.Trim();
            if (employee.Trim().ToUpper() == "ALL")
                employee = "";
            string location = cmbLocation.Text.Trim();
            if (location.Trim().ToUpper() == "ALL")
                location = "";

            string agent = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string prefix = "";
            string suffix = "";
            string name = "";
            DateTime createDate = DateTime.Now;
            DateTime lastTouchDate = DateTime.Now;
            DateTime nextTouchDate = DateTime.Now;
            string lastTouchTime = "";
            string result = "";

            DateTime date = DateTime.Now;

            string searchBy = cmbSearch.Text.Trim();
            //DateTime lastTouchTime = DateTime.Now;

            PleaseWait pleaseForm = new PleaseWait("Please Wait!\nPreparing Calender!");
            pleaseForm.Show();
            pleaseForm.Refresh();

            GoogleCalendarManager.InitCalander(employee);
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                prefix = dt.Rows[i]["prefix"].ObjToString();
                suffix = dt.Rows[i]["suffix"].ObjToString();
                name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);
                result = "";

                //if (searchBy == "Creation Date")
                //    date = dt.Rows[i]["prospectCreationDate"].ObjToDateTime();
                //else if (searchBy == "Last Touch Date")
                //{
                //    date = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                //    lastTouchTime = dt.Rows[i]["lastTouchTime"].ObjToString();
                //    string strDateIn = date.ToString("MM/dd/yyyy") + " " + lastTouchTime;
                //    string strDateOut = "";
                //    decodeDateTime(strDateIn, ref date, ref strDateOut);

                //    result = dt.Rows[i]["lastTouchResult"].ObjToString();
                //}
                //else if (searchBy == "Next Touch Date")
                //{
                //    date = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                //    result = dt.Rows[i]["scheduledActivity"].ObjToString();
                //}

                date = dt.Rows[i]["lastTouchDate"].ObjToDateTime();
                if (date.Year > 100)
                {
                    lastTouchTime = dt.Rows[i]["lastTouchTime"].ObjToString();
                    string strDateIn = date.ToString("MM/dd/yyyy") + " " + lastTouchTime;
                    string strDateOut = "";
                    decodeDateTime(strDateIn, ref date, ref strDateOut);

                    result = dt.Rows[i]["lastTouchResult"].ObjToString();

                    GoogleCalendarManager.AddCalanderEvent("Pre-Need", employee, name + " " + "Last Touch Date", location, result, date, date);
                }
                date = dt.Rows[i]["nextScheduledTouchDate"].ObjToDateTime();
                if (date.Year > 100)
                {
                    lastTouchTime = dt.Rows[i]["nextScheduledTouchTime"].ObjToString();
                    string strDateIn = date.ToString("MM/dd/yyyy") + " " + lastTouchTime;
                    string strDateOut = "";
                    decodeDateTime(strDateIn, ref date, ref strDateOut);

                    result = dt.Rows[i]["scheduledActivity"].ObjToString();
                    GoogleCalendarManager.AddCalanderEvent("Pre-Need", employee, name + " " + "Next Touch Date", location, result, date, date);
                }
            }

            DataTable ddd = GoogleCalendarManager.GetCalendarEvents();

            Calendar3 calendarForm = new Calendar3(dateTimePicker1.Value, searchBy);
            calendarForm.Show();

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;
        }
        /****************************************************************************************/
        private bool decodeDateTime(string dateStr, ref DateTime dateOut, ref string strDate)
        {
            string str = "";
            string[] Lines = null;
            DateTime date = DateTime.Now;
            Lines = dateStr.Split(' ');
            if (Lines.Length <= 1)
                return false;
            if (!G1.validate_date(Lines[0]))
                return false;
            date = Lines[0].ObjToDateTime();
            str = Lines[1].Trim();
            if (String.IsNullOrWhiteSpace(str))
                return false;
            int hour = 0;
            int min = 0;
            int sec = 0;
            strDate = "";
            Lines = str.Split(':');
            if (Lines.Length > 0)
            {
                bool addHours = false;
                if (dateStr.Trim().ToUpper().IndexOf("PM") > 0)
                    addHours = true;
                hour = Lines[0].ObjToInt32();
                if (addHours)
                    hour += 12;
                if (hour < 0 || hour > 23)
                    return false;
                if (Lines.Length > 1)
                {
                    min = Lines[1].ObjToInt32();
                    if (min < 0 || min > 59)
                        return false;
                    if (Lines.Length > 2)
                    {
                        sec = Lines[2].ObjToInt32();
                        if (sec < 0 || sec > 59)
                            return false;
                    }
                }
            }

            dateOut = new DateTime(date.Year, date.Month, date.Day, hour, min, sec);
            if (sec > 0)
                strDate = dateOut.ToString("MM/dd/yyyy HH:mm:ss");
            else
                strDate = dateOut.ToString("MM/dd/yyyy HH:mm");
            return true;
        }
        /****************************************************************************************/
        private void cmbSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void chkDoNotCall_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void assignNewAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selections = "";
            string name = "";

            DataTable dt = (DataTable)cmbEmployee.DataSource;


            for (int i = 0; i < repositoryItemComboBox2.Items.Count; i++)
            {
                name = repositoryItemComboBox2.Items[i].ObjToString();
                if (name.ToUpper() == "ALL")
                    continue;
                selections += name + "\n";
            }
            selections = selections.TrimEnd('\n');

            using (SelectFromList listForm = new SelectFromList(selections, false, true ))
            {
                listForm.ListDone += ListForm_ListDone;
                listForm.ShowDialog();
            }
        }
        /****************************************************************************************/
        private void ListForm_ListDone(string s)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            int [] rows = gridMain.GetSelectedRows();
            if (rows.Length <= 0)
                return;

            this.Cursor = Cursors.WaitCursor;
            int row = 0;
            int irow = 0;
            string record = "";
            for ( int i=0;i<rows.Length; i++)
            {
                row = rows[i];
                irow = gridMain.GetDataSourceRowIndex(row);
                dt.Rows[irow]["agent"] = s;
                record = dt.Rows[irow]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    Update_PreNeed(record, "agent", s);
                }
            }
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable cleanupPhones ( DataTable dt )
        {
            string phone = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                phone = dt.Rows[i]["mobilePhone"].ObjToString();
                phone = AgentProspectReport.reformatPhone(phone, true);
                dt.Rows[i]["mobilePhone"] = phone;

                phone = dt.Rows[i]["homePhone"].ObjToString();
                phone = AgentProspectReport.reformatPhone(phone, true);
                dt.Rows[i]["homePhone"] = phone;

                phone = dt.Rows[i]["workPhone"].ObjToString();
                phone = AgentProspectReport.reformatPhone(phone, true);
                dt.Rows[i]["workPhone"] = phone;
            }
            return dt;
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            //SetupSelectedColumns();


            if (loading)
                return;
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("AgentPreneeds", comboName, dgv);
                string name = "AgentPreneeds " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("AgentPreneeds", "Primary", dgv);
                string name = "AgentPreneeds Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }

            RemoveResults();

            CleanupFieldColumns();

            if ( workDt != null )
            {
                int height = this.Height;
                this.Location = new Point(100, 100);
                this.Height = height + 100;
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'AgentPreNeeds' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "PreNeed";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    if ( G1.get_column_number ( (GridView) dgv.MainView, name ) >= 0 )
                        ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;

            SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "AgentPreNeeds", "Primary", actualName );
            sform.Done += new SelectDisplayColumns.d_void_selectionDone(sxform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sxform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "AgentPreNeeds";
            string skinName = "";
            SetupSelectedColumns("AgentPreNeeds", name, dgv);
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
        /***********************************************************************************************/
        void sform_Done()
        {
            CleanupFieldColumns();
            dgv.Refresh();
            this.Refresh();
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
            string saveName = "AgentPreneeds " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);

            if ( agentVisible )
                gridMain.Columns["agent"].Visible = true;
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "AgentPreneeds " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /****************************************************************************************/
        private void goToPreneedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            string contractNumber = dr["referenceTrust"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( contractNumber))
            {
                if (isValidPreneed(contractNumber))
                {
                    this.Cursor = Cursors.WaitCursor;
                    CustomerDetails clientForm = new CustomerDetails(contractNumber);
                    clientForm.Show();
                    this.Cursor = Cursors.Default;
                }
            }
        }
        /****************************************************************************************/
        private bool isValidPreneed ( string contractNumber )
        {
            if (String.IsNullOrWhiteSpace(contractNumber))
                return false;
            string cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("Contract Number (" + contractNumber + ")\ndoes not have a valid customer record!", "Invalid Customer Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("Contract Number (" + contractNumber + ")\ndoes not have a valid contract record!", "Invalid Contract Record Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            return true;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox7_SelectedValueChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            string primary = combo.Text;
            if (!String.IsNullOrWhiteSpace(primary))
            {
                try
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    dr["primaryPhone"] = primary;
                    dr["mod"] = "Y";
                    int rowhandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowhandle);
                    string record = dr["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        return;
                    try
                    {
                        Update_PreNeed(record, "primaryPhone", primary);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void repositoryItemMemoEdit1_EditValueChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string record = dt.Rows[row]["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;
            try
            {
                DevExpress.XtraEditors.MemoEdit memo = (DevExpress.XtraEditors.MemoEdit)sender;
                string data = dt.Rows[row]["results"].ObjToString();
                data = dr["results"].ObjToString();
                data = memo.Text;
                Update_PreNeed(record, "results", data);
                dr["results"] = data;
                gridMain.RefreshEditor(true);
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void repositoryItemMemoEdit2_EditValueChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string record = dt.Rows[row]["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;
            try
            {
                DevExpress.XtraEditors.MemoEdit memo = (DevExpress.XtraEditors.MemoEdit)sender;
                string data = dt.Rows[row]["notes"].ObjToString();
                data = dr["notes"].ObjToString();
                data = memo.Text;
                Update_PreNeed(record, "notes", data);
                dr["notes"] = data;
                gridMain.RefreshEditor(true);
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void repositoryItemMemoEdit2_MouseDown(object sender, MouseEventArgs e)
        { // Notes
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string data = dt.Rows[row]["notes"].ObjToString();
            string record = dt.Rows[row]["record"].ObjToString();
            //data = dr["notes"].ObjToString();

            using (EditTextData fmrmyform = new EditTextData ("notes", data ))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                string p = fmrmyform.Answer.Trim();
                if (!String.IsNullOrWhiteSpace(p))
                {
                    dt.Rows[row]["notes"] = p;
                    dr["notes"] = p;
                    gridMain.RefreshEditor(true);
                    try
                    {
                        Update_PreNeed(record, "notes", p);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        /****************************************************************************************/
        private void repositoryItemMemoEdit1_MouseDown(object sender, MouseEventArgs e)
        { // Results
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string data = dt.Rows[row]["results"].ObjToString();
            string record = dt.Rows[row]["record"].ObjToString();
            //data = dr["notes"].ObjToString();
            using (EditTextData fmrmyform = new EditTextData("notes", data))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                string p = fmrmyform.Answer.Trim();
                if (!String.IsNullOrWhiteSpace(p))
                {
                    dt.Rows[row]["results"] = p;
                    dr["results"] = p;
                    gridMain.RefreshEditor(true);
                    try
                    {
                        Update_PreNeed(record, "results", p);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        /****************************************************************************************/
        private void get_stock_price( string ticker, ref DateTime date, ref double open, ref double high, ref double low, ref double close, ref double volume )
        {
            string json;
            open = 0D;
            high = 0D;
            low = 0D;
            close = 0D;
            volume = 0D;
            date = DateTime.Now;
            string stuff = "";
            string[] Lines = null;
            string[] Other = null;
            bool rv = false;
            string title = "";
            string what = "";
            double value = 0D;
            using (var web = new WebClient())
            {
                var url = $" https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol=" + ticker + "&apikey=7DXNLA8AABATMUIF";
                json = web.DownloadString(url);
                var trade = JsonConvert.DeserializeObject<dynamic>(json);
                try
                {
                    stuff = json.ToString();
                    Lines = stuff.Split('\n');
                    for ( int i=0; i<Lines.Length; i++)
                    {
                        rv = ParseStock(Lines[i], ref title, ref what, ref value);
                        if ( rv )
                        {
                            if (title == "high")
                                high = value;
                            else if (title == "low")
                                low = value;
                            else if (title == "price")
                                close = value;
                            else if (title == "open")
                                open = value;
                            else if (title == "volume")
                                volume = value;
                            else if (title == "latest trading day")
                                date = what.ObjToDateTime();
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private bool ParseStock ( string str, ref string title, ref string what, ref double value )
        {
            title = "";
            what = "";
            value = 0D;
            string[] Lines = str.Split(':');
            if (Lines.Length <= 1)
                return false;
            title = Lines[0].Trim();
            title = title.Replace("\"", "");
            title = title.Replace(",", "");
            title = title.Trim();
            int idx = title.IndexOf('.');
            if ( idx > 0 && idx < (title.Length-1) )
            {
                title = title.Substring(idx + 1).Trim();
            }
            what = Lines[1].Trim();
            what = what.Replace("\"", "");
            what = what.Replace(",", "");
            what = what.Trim();
            if (G1.validate_numeric(what))
                value = what.ObjToDouble();
            return true;
        }
        /****************************************************************************************/
        private void editDetailMenu_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string record = dr["record"].ObjToString();

            using (editDG editForm = new editDG(gridMain, dt, row, record))
            {
                editForm.editDone += EditForm_editDone;
                editForm.ShowDialog();
            }
        }
        /****************************************************************************************/
        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            string agent = cmbEmployee.Text;

            ContactReports reports = new ContactReports( "Contacts Preneed", agent, gridMain );
            reports.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void toolAgentReportList_Click(object sender, EventArgs e)
        {
            DataTable agentDt = (DataTable) cmbEmployee.DataSource;
            string agent = cmbEmployee.Text.Trim();
            if (agent.ToUpper() == "ALL")
                agent = "";

            ContactReportsAgents agentsForm = new ContactReportsAgents(agentDt, gridMain, agent, "Contacts Preneed" );
            agentsForm.Show();
        }
        /****************************************************************************************/
        private void chkContactType_EditValueChanged(object sender, EventArgs e)
        {
            //allType = (DataTable) chkContactType.Properties.DataSource;

            //string contacts = getContactQuery();

            //string[] Lines = contacts.Split(',');
            //for ( int i=0; i<Lines.Length; i++)
            //{

            //}

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /*******************************************************************************************/
        private string getContactQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkContactType.EditValue.ToString().Split('|');
            string location = "";
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    location = locIDs[i].Trim().ToUpper();
                    procLoc += location;
                }
            }
            return procLoc;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string record = dr["record"].ObjToString();
            if (record == "-1")
                return;

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string contactType = combo.Text.Trim();
            dr["contactType"] = contactType;

            dr["mod"] = "Y";

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;

            string what = dr[currentColumn].ObjToString();

            Update_PreNeed(record, "contactType", what);

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnInitialSetup_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            initialLoad = true;
            LoadData();
            //dt = SetupGreenAndRed(dt);

            //G1.NumberDataTable(dt);
            //dgv.DataSource = dt;

            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);
            //dgv.Refresh();

            //this.Refresh();

            //gridMain.PostEditor();

            //dt = (DataTable)dgv.DataSource;
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            try
            {
                string record = dr["record"].ObjToString();
                if (record == "-1")
                    return;
                string oldWhat = dr["contactStatus"].ObjToString();

                DevExpress.XtraEditors.CheckEdit box = (DevExpress.XtraEditors.CheckEdit)sender;
                if (box.Checked)
                {
                    Update_PreNeed(record, "completed", "1");
                    dr["completed"] = "1";
                    Update_PreNeed(record, "contactStatus", "Presentation Made, Sold, Finalized");
                    dr["contactStatus"] = "Presentation Made, Sold, Finalized";
                    gridMain.RefreshEditor(true);
                    dgv.Refresh();
                }
                else
                {
                    Update_PreNeed(record, "completed", "0");
                    dr["completed"] = "0";
                    Update_PreNeed(record, "contactStatus", "");
                    dr["contactStatus"] = "";
                    gridMain.RefreshEditor(true);
                    dgv.Refresh();
                }
            }
            catch ( Exception ex)
            {
            }
            gridMain.RefreshData();
            gridMain.PostEditor();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox4_EditValueChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string record = dr["record"].ObjToString();
            if (record == "-1")
                return;
            string oldWhat = dr["contactStatus"].ObjToString();

            ComboBoxEdit box = (ComboBoxEdit)sender;
            string what = box.Text.Trim();
            if (what == "Presentation Made, Sold, Finalized")
            {
                Update_PreNeed(record, "completed", "1");
                dr["completed"] = "1";
                Update_PreNeed(record, "contactStatus", "Presentation Made, Sold, Finalized");
                dr["contactStatus"] = "Presentation Made, Sold, Finalized";
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
            else
            {
                string completed = dr["completed"].ObjToString();
                Update_PreNeed(record, "completed", "0");
                dr["completed"] = "0";
                Update_PreNeed(record, "contactStatus", what);
                dr["contactStatus"] = what;
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
            gridMain.RefreshData();
            gridMain.PostEditor();
        }
        /****************************************************************************************/
        private void chkLocations_EditValueChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            GridGroupRowInfo info = e.Info as GridGroupRowInfo;
            string location = info.GroupText;
            location = location.Replace("), )", "");
            info.GroupText = location;
        }
        /****************************************************************************************/
        private void btnTestGoogle_Click(object sender, EventArgs e)
        {
            //GoogleCalendarManager.AddRobbyEvent();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox6_Validating(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            if ( gridMain.FocusedColumn.FieldName == "funeralHome")
            {
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string funeralHome = dr["funeralHome"].ObjToString();
                if (!ValidateFuneralHome(funeralHome))
                    e.Cancel = false;
                else
                    oldWhat = funeralHome;
            }
        }
        /****************************************************************************************/
        private void repositoryItemComboBox6_SelectedValueChanged(object sender, EventArgs e)
        {
            if (1 == 1 )
                return;
            DevExpress.XtraEditors.ComboBoxEdit box = (DevExpress.XtraEditors.ComboBoxEdit)sender;

            string answer = box.Text;
            string str = "";

            bool found = false;

            for ( int i=0; i<repositoryItemComboBox6.Items.Count; i++)
            {
                str = repositoryItemComboBox6.Items[i].ObjToString().Trim();
                if ( answer.Length < str.Length )
                {
                    str = str.Substring(0, answer.Length);
                    if ( str == answer )
                    {
                        found = true;
                        break;
                    }
                }
                else if ( answer.Length == str.Length )
                {
                    if ( str == answer )
                    {
                        found = true;
                        break;
                    }
                }
            }
            if ( !found )
            {
                box.Text = oldWhat;
                box.RefreshEditValue();
            }
        }
        /****************************************************************************************/
        private void duplicateContactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string rec = dr["record"].ObjToString();
            if ( rec == "-1")
                return;
            string lastName = dr["lastName"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string middleName = dr["middleName"].ObjToString();
            string location = dr["funeralHome"].ObjToString();
            string agent = dr["agent"].ObjToString();
            string agentName = dr["agentName"].ObjToString();

            string address = dr["address"].ObjToString();
            string city = dr["city"].ObjToString();
            string state = dr["city"].ObjToString();
            string zip = dr["zip"].ObjToString();

            string referenceFuneral = dr["referenceFuneral"].ObjToString();
            string referenceTrust = dr["referenceTrust"].ObjToString();
            string funeralRelationship = dr["funeralRelationship"].ObjToString();
            string trustRelationship = dr["trustRelationship"].ObjToString();
            string prefix = dr["refDeceasedPrefix"].ObjToString();
            string suffix = dr["refDeceasedSuffix"].ObjToString();
            string deceasedFirstName = dr["refDeceasedFirstName"].ObjToString();
            string deceasedMiddleName = dr["refDeceasedMiddleName"].ObjToString();
            string deceasedLastName = dr["refDeceasedLastName"].ObjToString();

            string record = G1.create_record("contacts_preneed", "agent", "-1");
            if (G1.BadRecord("contacts_preneed", record))
                return;

            DateTime date = DateTime.Now;
            string apptDate = date.ToString("yyyy-MM-dd");
            G1.update_db_table("contacts_preneed", "record", record, new string[] { "agent", agent, "prospectCreationDate", apptDate, "funeralHome", location, "totalTouches", "0", "oldRecord", record });

            DataRow dRow = dt.NewRow();
            DateTime now = DateTime.Now;
            dRow["record"] = record;
            dRow["oldRecord"] = record;
            //dRow["apptDate"] = G1.DTtoMySQLDT(date);
            dRow["prospectCreationDate"] = G1.DTtoMySQLDT(date);
            dRow["mod"] = "Y";
            dRow["completed"] = "0";
            //dRow["contactType"] = contactType;
            dRow["agent"] = agent;
            dRow["agentName"] = agentName;
            dRow["funeralHome"] = location;

            dRow["firstName"] = firstName;
            dRow["lastName"] = lastName;
            dRow["MiddleName"] = "DUPLICATE";

            dRow["address"] = address;
            dRow["city"] = city;
            dRow["state"] = state;
            dRow["zip"] = zip;

            dRow["referenceFuneral"] = referenceFuneral;
            dRow["referenceTrust"] = referenceTrust;
            dRow["funeralRelationship"] = funeralRelationship;
            dRow["trustRelationship"] = trustRelationship;

            dRow["refDeceasedPrefix"] = prefix;
            dRow["refDeceasedSuffix"] = suffix;
            dRow["refDeceasedFirstName"] = deceasedFirstName;
            dRow["refDeceasedMiddleName"] = deceasedMiddleName;
            dRow["refDeceasedLastName"] = deceasedLastName;

            G1.update_db_table("contacts_preneed", "record", record, new string[] { "firstName", firstName, "middleName", "DUPLICATE", "lastName", lastName, "agentName", agentName });
            G1.update_db_table("contacts_preneed", "record", record, new string[] { "address", address, "city", city, "state", state, "zip", zip, "referenceFuneral", referenceFuneral, "referenceTrust", referenceTrust, "funeralRelationship", funeralRelationship, "trustRelationship", trustRelationship });
            G1.update_db_table("contacts_preneed", "record", record, new string[] { "refDeceasedPrefix", prefix, "refDeceasedSuffix", suffix, "refDeceasedFirstName", deceasedFirstName, "refDeceasedMiddleName", deceasedMiddleName, "refDeceasedLastName", deceasedLastName });

            dt.Rows.InsertAt(dRow, 0);
            //dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
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
                        ColorizeCell(e.Appearance, Color.Pink );
                    }
                    else if (color == 2D)
                    {
                        e.Appearance.BackColor = Color.LightGreen;
                        ColorizeCell(e.Appearance, Color.LightGreen );
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
        private void ColorizeCell(object appearanceObject, Color color )
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
        private void goToFuneralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            string serviceId = dr["referenceFuneral"].ObjToString();
            if (!String.IsNullOrWhiteSpace(serviceId))
            {
                string cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceId + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

                    this.Cursor = Cursors.WaitCursor;
                    Form form = G1.IsFormOpen("EditCust", contractNumber);
                    if (form != null)
                    {
                        form.Show();
                        form.WindowState = FormWindowState.Maximized;
                        form.Visible = true;
                        form.BringToFront();
                    }
                    else
                    {
                        EditCust custForm = new EditCust(contractNumber);
                        custForm.Tag = contractNumber;
                        //custForm.custClosing += CustForm_custClosing;
                        custForm.Show();
                    }
                    this.Cursor = Cursors.Default;
                }
            }
        }
        /****************************************************************************************/
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string serviceId = dr["referenceFuneral"].ObjToString();

            string str = "";

            for ( int i=0; i<contextMenuStrip1.Items.Count; i++)
            {
                str = contextMenuStrip1.Items[i].Text;
                if ( str == "Go To Funeral")
                {
                    if (String.IsNullOrWhiteSpace(serviceId))
                        contextMenuStrip1.Items[i].Visible = false;
                    else
                        contextMenuStrip1.Items[i].Visible = true;
                }
            }
        }
        /****************************************************************************************/
        private DataTable ProcessBirthdays(DataTable dt, string cmd, string saveCmd)
        {
            //dt.Columns.Add("Int32_s1", typeof(int), "s1");

            DataTable dx = null;
            DataRow[] xRows = dt.Select("s1<>'0000-00-00 00:00:00'");
            if (xRows.Length > 0)
                dx = xRows.CopyToDataTable();
            else
                dx = dt.Copy();

            dx.Columns.Add("sDate", typeof(DateTime), "s1");

            DataRow[] dRows = dx.Select("sDate>#1800-01-01#");
            if (dRows.Length <= 0)
                return dt;

            dx = dRows.CopyToDataTable();

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                dx.Rows[i]["firstName"] = "Birthday";
                dt.ImportRow(dx.Rows[i]);
            }

            return dt;
        }
        /****************************************************************************************/
        private void btnShowAnniversary_Click(object sender, EventArgs e)
        {
            if (btnShowAnniversary.BackColor == Color.Transparent)
                btnShowAnniversary.BackColor = Color.PaleGreen;
            else
                btnShowAnniversary.BackColor = Color.Transparent;

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            this.Refresh();

            gridMain.PostEditor();
        }
        /****************************************************************************************/
        private void cmbNextDays_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            this.Refresh();

            gridMain.PostEditor();
        }
        /****************************************************************************************/
        private void cmbAnniversary_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            this.Refresh();

            gridMain.PostEditor();
        }
        /****************************************************************************************/
        public static string BuildReportQuery(string module, DataTable dt, string workAgent, ref bool isCustom)
        {
            string cmd = "Select * from `contacts_preneed` WHERE ";
            if ( module.ToUpper() == "CONTACTS")
                cmd = "Select * from `contacts` WHERE ";

            bool found = false;
            string field = "";
            string operand = "";
            string data = "";
            string status = "";
            string body = "";
            DateTime date = DateTime.Now;
            DateTime today = DateTime.Now;
            int iBody = 0;
            string[] Lines = null;
            bool gotToday = false;

            isCustom = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                gotToday = false;
                field = dt.Rows[i]["field"].ObjToString();
                if (field.ToUpper() == "{CUSTOM}")
                {
                    isCustom = true;
                    continue;
                }
                data = dt.Rows[i]["data"].ObjToString();
                if (String.IsNullOrWhiteSpace(data))
                    continue;
                if (data.ToUpper().IndexOf("TODAY") == 0)
                {
                    date = DateTime.Now;
                    data = data.ToUpper().Replace("TODAY", "").Trim();
                    Lines = data.Split(' ');
                    if (Lines.Length >= 2)
                    {
                        int days = Lines[1].Trim().ObjToInt32();
                        if (Lines[0].Trim() == "-")
                            days = days * -1;
                        date = date.AddDays(days);
                        data = date.ToString("yyyy-MM-dd");
                        gotToday = true;
                    }
                    else
                    {
                        data = date.ToString("yyyy-MM-dd");
                        gotToday = true;
                    }
                }
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "OFF")
                    continue;

                operand = dt.Rows[i]["operand"].ObjToString();

                body = data.Trim();

                date = body.ObjToDateTime();
                if (date.Year < 1000 || gotToday)
                {
                    if (!G1.validate_numeric(body))
                    {
                        if (found)
                            cmd += " AND ";
                        cmd += " `" + field + "` " + operand + " '" + body + "' ";
                        found = true;
                        continue;
                    }
                    today = DateTime.Now;
                    if (operand == ">")
                    {
                        iBody = body.ObjToInt32();
                        today = today.AddDays(iBody * -1);
                        if (gotToday)
                            today = date;
                        //if (field.ToUpper() != "AGE")
                        //    operand = "<";
                    }
                    else if (operand == ">=")
                    {
                        iBody = body.ObjToInt32();
                        today = today.AddDays(iBody * -1);
                        if (gotToday)
                            today = date;
                        //if (field.ToUpper() != "AGE")
                        //    operand = "<=";
                    }
                    else if (operand == "<")
                    {
                        iBody = body.ObjToInt32();
                        today = today.AddDays(iBody * -1);
                        if (gotToday)
                            today = date;
                        //if (field.ToUpper() != "AGE")
                        //    operand = ">";
                    }
                    else if (operand == "<=")
                    {
                        iBody = body.ObjToInt32();
                        today = today.AddDays(iBody * -1);
                        if (gotToday)
                            today = date;
                        //if (field.ToUpper() != "AGE")
                        //    operand = ">=";
                    }
                    else if (operand == "!=")
                    {
                        iBody = body.ObjToInt32();
                        today = today.AddDays(iBody * -1);
                        if (gotToday)
                            today = date;
                        //if (field.ToUpper() != "AGE")
                        //    operand = ">=";
                    }
                    else if (operand == "=")
                    {
                        iBody = body.ObjToInt32();
                    }
                    else
                        continue;
                    if (found)
                        cmd += " AND ";
                    if (field.ToUpper() == "AGE")
                        cmd += " `" + field + "` " + operand + " '" + iBody.ToString() + "' ";
                    else
                        cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
                    found = true;
                }
                else
                {
                    if (!G1.validate_numeric(body))
                    {
                        if (G1.validate_date(body))
                        {
                            date = body.ObjToDateTime();
                            body = date.ToString("yyyy-MM-dd");
                        }
                        if (found)
                            cmd += " AND ";
                        cmd += " `" + field + "` " + operand + " '" + body + "' ";
                        found = true;
                        continue;
                    }
                    else
                        continue;
                    if (found)
                        cmd += " AND ";
                    if (field.ToUpper() == "AGE")
                        cmd += " `" + field + "` " + operand + " '" + iBody.ToString() + "' ";
                    else
                        cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
                    found = true;
                }
            }

            if (!found && !isCustom)
            {
                MessageBox.Show("Search Criteria is Empty!", "Search Criteria Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return "";
            }
            if (!String.IsNullOrWhiteSpace(workAgent))
            {
                if (workAgent.ToUpper() != "ALL")
                    cmd += " AND `agent` = '" + workAgent + "' ";
            }

            cmd += ";";
            return cmd;
        }
        /****************************************************************************************/
        private void btnRunReport_Click(object sender, EventArgs e)
        {
            string customReport = cmbReport.Text.Trim();
            if (String.IsNullOrWhiteSpace(customReport))
                return;


            string cmd = "Select * from `contacts_reports` WHERE `module` = 'Contacts Preneed' AND `report` = '" + customReport + "';";
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

            cmd = BuildReportQuery("Contacts Preneed", dt, agent, ref isCustom);
            dx = G1.get_db_data(cmd);

            if (dx != null)
            {
                this.Cursor = Cursors.WaitCursor;
                int height = this.Height;

                ContactsPreneed form = null;
                if (!isCustom)
                    form = new ContactsPreneed(dx, customReport );
                else
                    form = new ContactsPreneed(dx, dt, true, customReport);

                //leadForm.StartPosition = FormStartPosition.CenterParent;
                form.Show();
                //form.Anchor = AnchorStyles.None;

                form.AutoSize = true; //this causes the form to grow only. Don't set it if you want to resize automatically using AnchorStyles, as I did below.
                form.FormBorderStyle = FormBorderStyle.Sizable; //I think this is not necessary to solve the problem, but I have left it there just in case :-)
                form.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                    | System.Windows.Forms.AnchorStyles.Left)
                                    | System.Windows.Forms.AnchorStyles.Right)));

                form.Show();
                form.Location = new Point(this.Parent.Left + 500, this.Parent.Top + 500);
                form.Height = height + 200;
                form.StartPosition = FormStartPosition.CenterParent;
                form.SetBounds(this.Parent.Left + 100, this.Parent.Top + 100, form.Width, height + 50);
                form.Refresh();

                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
    }
}