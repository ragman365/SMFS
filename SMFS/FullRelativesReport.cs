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
using System.Threading;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FullRelativesReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";

        //CalendarService calService;
        private const string calID = "xxxxxxxxx...@group.calendar.google.com";
        private const string UserId = "user-id";
        //private static string gFolder = System.Web.HttpContext.Current.Server.MapPath("/App_Data/MyGoogleStorage");
        //private static string gFolder = System.Web.HttpContext.Current.Server.ObjToString();
        private static string gFolder = "";
        private bool foundLocalPreference = false;
        private bool superuser = false;
        private bool showAgent = true;
        private DataTable originalDt = null;
        /****************************************************************************************/
        public FullRelativesReport()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox12, "Add New Contact");
            tt.SetToolTip(this.pictureBox11, "Remove Contact");
        }
        /****************************************************************************************/
        private void FullRelativesReport_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            loading = true;

            //string preference = G1.getPreference(LoginForm.username, "FullRelatives", "Allow SuperUser Access");
            //if (preference.ToUpper() == "YES")
            //    superuser = true;


            string saveName = "FullRelatives Primary";
            string skinName = "";

            SetupSelectedColumns("FullRelatives", "Primary", dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                //if (skinName != "DevExpress Style")
                //    skinForm_SkinSelected("Skin : " + skinName);
            }

            loadGroupCombo(cmbSelectColumns, "FullRelatives", "Primary");
            cmbSelectColumns.Text = "Primary";


            DateTime now = DateTime.Now;
//            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            LoadDBTable("ref_relations", "relationship", this.repositoryItemComboBox3);
            LoadDBTable("ref_contact_status", "contact_status", this.repositoryItemComboBox4);
            LoadDBTable("ref_lead source", "lead source", this.repositoryItemComboBox5);

            if (!G1.isAdmin() || !superuser)
            {
                //assignNewAgentToolStripMenuItem.Dispose();
                showAgent = false;
            }

            LoadEmployees();
            LoadLocations();
            loadRepositoryLocatons();

            //LoadData();

            //SetupSelectedColumns();

            CleanupFieldColumns();

            gridMain.RefreshEditor(true);
            this.Refresh();

            modified = false;
            loading = false;

            //cmbSelectColumns_SelectedIndexChanged(null, null);
        }
        /***********************************************************************************************/
        private void loadRepositoryLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string assignedLocations = "";

            string newUser = cmbEmployee.Text;

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            if (!String.IsNullOrWhiteSpace(newUser))
                cmd = "Select * from `users` where `username` = '" + newUser + "';";

            newUser = "";

            DataTable userDt = G1.get_db_data(cmd);
            if (userDt.Rows.Count > 0)
                assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();

            string locationCode = "";
            string keyCode = "";
            string[] Lines = null;
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
        private void LoadData()
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
            string searchBy = "";

            //string cmd = "SELECT contacts_preneed.* FROM contacts_preneed INNER JOIN(SELECT agent, funeralHome, MAX(prospectCreationDate) AS latest FROM contacts_preneed GROUP BY agent, funeralHome) r ON contacts_preneed.prospectCreationDate = r.latest AND contacts_preneed.agent = r.agent ";
            string cmd = "SELECT * FROM fcustomers f JOIN fcust_extended x ON f.`contractNumber` = x.`contractNumber` JOIN `relatives` r ON f.`contractNumber` = r.`contractNumber` WHERE f.`deceasedDate` >= '" + date1 + "' AND f.`deceasedDate` <= '" + date2 + "' ";
            cmd += " AND r.`depRelationship` <> 'DISCLOSURES' ";
            cmd += " AND r.`depRelationship` <> 'PB' ";
            cmd += " AND r.`depRelationship` <> 'HPB' ";
            cmd += " AND r.`depRelationship` <> 'MUSICIAN' ";
            cmd += " AND r.`depRelationship` <> 'FUNERAL DIRECTOR' ";
            cmd += " AND r.`depRelationship` <> 'PALLBEARER' ";

            //cmd += " WHERE ";

            cmd += " ORDER BY x.`serviceLoc`, f.`deceasedDate`;";

            DataTable dt = G1.get_db_data(cmd);

            if (G1.get_column_number(dt, "funeralHome") < 0)
                dt.Columns.Add("funeralHome");

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");
            DataRow[] dRows = null;
            string serviceLoc = "";
            string arranger = "";
            int idx = 0;
            string[] Lines = null;

            if (G1.get_column_number(dt, "arranger") < 0)
                dt.Columns.Add("arranger");

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( serviceLoc ))
                {
                    dRows = funDt.Select("atneedcode='" + serviceLoc + "'");
                    if (dRows.Length > 0)
                        dt.Rows[i]["funeralHome"] = dRows[0]["LocationCode"].ObjToString();
                }
                arranger = dt.Rows[i]["Funeral Arranger"].ObjToString();
                idx = arranger.IndexOf("[");
                if (idx > 0)
                    arranger = arranger.Substring(0, idx-1);
                Lines = arranger.Split(' ');
                arranger = "";
                for (int j = 0; j < Lines.Length; j++)
                {
                    if ( !String.IsNullOrWhiteSpace ( Lines[j]))
                        arranger += Lines[j].Trim() + " ";
                }
                dt.Rows[i]["arranger"] = arranger.Trim();
            }

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string prefix = "";
            string suffix = "";
            string extraName = "";
            string agent = "";
            string home = "";
            string name = "";

            AddMod(dt, gridMain);

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            originalDt = dt;

            this.Cursor = Cursors.Default;
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
        private void LoadEmployees ()
        {

            string cmd = "Select * from `arrangers` ORDER by `lastName`;";
            DataTable dt = G1.get_db_data(cmd);

            //dt = KillDuplicates(dt);

            dt.Columns.Add("name");

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string prefix = "";
            string suffix = "";
            string name = "";
            DataTable dx = new DataTable();
            dx.Columns.Add("name");
            DataRow dRow = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);

                name = lastName + ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name += " " + middleName;
                dt.Rows[i]["name"] = name;

                dRow = dx.NewRow();
                dRow["name"] = name;
                dx.Rows.Add(dRow);
            }

            cmd = "Select * from `directors` ORDER by `lastName`;";
            dt = G1.get_db_data(cmd);
            dt.Columns.Add("name");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);
                name = lastName + ", " + firstName;
                if (!String.IsNullOrWhiteSpace(middleName))
                    name += " " + middleName;
                dt.Rows[i]["name"] = name;

                dRow = dx.NewRow();
                dRow["name"] = name;
                dx.Rows.Add(dRow);
            }

            DataView tempview = dx.DefaultView;
            tempview.Sort = "name asc";
            dx = tempview.ToTable();

            dx = G1.RemoveDuplicates(dx, "name");

            repositoryItemComboBox2.Items.Clear();

            DataRow dR = dx.NewRow();
            dR["name"] = "All";
            dx.Rows.InsertAt(dR, 0);

            cmbEmployee.DataSource = dx;
        }
        /***********************************************************************************************/
        private DataTable KillDuplicates(DataTable dt)
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "lastName asc, firstName asc";
            dt = tempview.ToTable();
            string lastName = "";
            string firstName = "";
            string license = "";
            string location = "";

            string lastLastName = "";
            string lastFirstName = "";
            string lastLicense = "";
            string lastLocation = "";

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                lastName = dt.Rows[i]["lastName"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                license = dt.Rows[i]["license"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();

                if (String.IsNullOrWhiteSpace(lastName))
                    dt.Rows[i]["mod"] = "D";
                else
                {
                    if (lastName == lastLastName && firstName == lastFirstName && license == lastLicense && location == lastLocation)
                    {
                        dt.Rows[i]["mod"] = "D";
                    }
                    lastLastName = lastName;
                    lastFirstName = firstName;
                    lastLicense = license;
                    lastLocation = location;
                }
            }
            tempview = dt.DefaultView;
            tempview.Sort = "record asc";
            dt = tempview.ToTable();
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
        private void LoadLocations()
        {
            cmbLocation.Items.Clear();
            cmbLocation.Items.Add("All");
            string cmd = "Select * from `funeralhomes` order by `LocationCode`;";
            DataTable locDt = G1.get_db_data(cmd);
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                cmbLocation.Items.Add(locDt.Rows[i]["locationCode"].ObjToString());
            }
            //DataRow dRow = locDt.NewRow();
            //dRow["LocationCode"] = "All";
            //locDt.Rows.InsertAt(dRow, 0);
            //cmbLocation.DataSource = locDt;
            cmbLocation.Text = "All";
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

            string deceased = dt.Rows[row]["deceased"].ObjToString();
            if ( deceased == "1" )
            {
                e.Visible = false;
                e.Handled = true;
            }

            //string cType = cmbContractType.Text.Trim().ToUpper();
            //if (cType == "ALL")
            //    return;

            //string contactType = dt.Rows[row]["contactType"].ObjToString().ToUpper();
            //if ( contactType != cType )
            //{
            //    e.Visible = false;
            //    e.Handled = true;
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
                else if (e.Column.FieldName == "nextScheduledTouchDate")
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
                            if (e.Column.FieldName == "nextScheduledTouchDate")
                            {
                                if (date.Year > 1000)
                                {
                                    DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                                    if (date < today)
                                        e.Appearance.BackColor = Color.Pink;
                                    else if (date <= DateTime.Now)
                                        e.Appearance.BackColor = Color.LightGreen;
                                }
                            }
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
            //btnSave.Show();
            //btnSave.Refresh();

            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() == "NUM")
                return;

            string what = dr[currentColumn].ObjToString();
            string record = dr["record"].ObjToString();

            if (currentColumn.ToUpper() == "firstName")
                Update_PreNeed(record, "firstName", what);
            else if (currentColumn.ToUpper() == "lastName")
                Update_PreNeed(record, "lastName", what);
            else if (currentColumn.ToUpper() == "middleName")
                Update_PreNeed(record, "middleName", what);
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
            else if (what == "Presnetation Made, Sold, Finalized")
            {
                dr["completed"] = "1";
                Update_PreNeed(record, "completed", "1");
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
            gridMain.RefreshData();
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
                    G1.update_db_table("contacts_preneed", "record", record, new string[] { field, data });
                }
            }
            catch (Exception ex)
            {
            }
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
                DialogResult result = MessageBox.Show("Do you want to Delete This Preneed Contact?", "Delete Preneed Contact Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if ( result == DialogResult.Yes )
                {
                    string record = dr["record"].ObjToString();
                    G1.delete_db_table("contacts_preneed", "record", record);

                    LoadData();
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
            string contactType = "";

            string location = cmbLocation.Text.Trim();

            DateTime date = DateTime.Now;
            string apptDate = date.ToString("yyyy-MM-dd");
            G1.update_db_table("contacts_preneed", "record", record, new string[] { "agent", agent, "prospectCreationDate", apptDate, "funeralHome", location, "totalTouches", "1" });

            DataRow dRow = dt.NewRow();
            DateTime now = DateTime.Now;
            dRow["record"] = record;
            //dRow["apptDate"] = G1.DTtoMySQLDT(date);
            dRow["prospectCreationDate"] = G1.DTtoMySQLDT(date);
            dRow["mod"] = "Y";
            dRow["completed"] = "0";
            dRow["contactType"] = contactType;
            dRow["agent"] = cmbEmployee.Text.Trim();
            dRow["agentName"] = cmbEmployee.Text.Trim();
            dRow["funeralHome"] = location;
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            G1.GoToLastRow(gridMain);

            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string serviceId = dr["ServiceId"].ObjToString();
            if (String.IsNullOrWhiteSpace(serviceId))
                return;

            string cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + serviceId + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string contract = dx.Rows[0]["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;

                Form form = G1.IsFormOpen("EditCust", contract);
                if (form != null)
                {
                    form.Show();
                    form.WindowState = FormWindowState.Maximized;
                    form.Visible = true;
                    form.BringToFront();
                }
                else
                {
                    EditCust custForm = new EditCust(contract);
                    custForm.Tag = contract;
                    custForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void HistoryForm_contactHistoryDone(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return;

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
                        G1.delete_db_table("contacts_preneed", "record", record);

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
            printableComponentLink1.ShowPreview();
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
            if (view.FocusedColumn.FieldName.ToUpper() == "PHONE")
            {
                //DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string phone = e.Value.ObjToString();
                string newPhone = AgentProspectReport.reformatPhone(phone, true);
                e.Value = newPhone;
            }
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
            //if (name == "apptDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;

            if (name.ToUpper().IndexOf("DATE") >= 0)
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
                                    count = Lines.Length + 1;
                                }
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
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
                SetupSelectedColumns("FullRelatives", comboName, dgv);
                string name = "FullRelatives " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }
            else
            {
                SetupSelectedColumns("FullRelatives", "Primary", dgv);
                string name = "FullRelatives Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
            }

            CleanupFieldColumns();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'FullRelatives' order by seq";
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
                procType = "FullRelatives";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
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
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "FullRelatives", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
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
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "FullRelatives " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "FullRelatives " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /****************************************************************************************/
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            string location = cmbLocation.Text;
            if (location.ToUpper() == "ALL")
            {
                dgv.DataSource = originalDt;
                dgv.Refresh();
                return;
            }
            DataTable dt = originalDt.Clone();
            DataRow[] dRows = originalDt.Select("funeralHome='" + location + "'");
            if ( dRows.Length > 0 )
            {
                dt = dRows.CopyToDataTable();
                dgv.DataSource = dt;
                dgv.Refresh();
            }
            else
            {
                dgv.DataSource = dt;
                dgv.Refresh();
            }
        }
        /****************************************************************************************/
        private void cmbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            string arranger = cmbEmployee.Text.Trim();
            if (arranger.ToUpper() == "ALL")
            {
                dgv.DataSource = originalDt;
                dgv.Refresh();
                return;
            }

            string[] Lines = arranger.Split(',');
            if ( Lines.Length > 0 )
            {
                string lastName = Lines[0].Trim();
                arranger = "";
                if (Lines.Length >= 2)
                    arranger = Lines[1].Trim();
                for (int j = 2; j < Lines.Length; j++)
                    arranger += " " + Lines[j].Trim();
                arranger += " " + lastName;
            }
            DataRow[] dRows = null;
            DataTable dt = originalDt.Clone();

            dRows = originalDt.Select("arranger='" + arranger + "'");
            if (dRows.Length > 0)
            {
                dt = dRows.CopyToDataTable();
                dgv.DataSource = dt;
                dgv.Refresh();
            }
            else
            {
                dgv.DataSource = dt;
                dgv.Refresh();
            }
        }
        /****************************************************************************************/
    }
}