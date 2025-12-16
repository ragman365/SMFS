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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContactsLeadList : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";
        private bool listUpdated = false;
        private DataTable originalDt = null;
        private string newUser = "";
        private DataTable workDt = null;
        /****************************************************************************************/
        public ContactsLeadList()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public ContactsLeadList( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            //tt.SetToolTip(this.pictureBox12, "Add New Contact");
            //tt.SetToolTip(this.pictureBox11, "Remove Contact");
        }
        /****************************************************************************************/
        private void ContactsLeadList_Load(object sender, EventArgs e)
        {
            SetupToolTips();

            loading = true;

            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            //LoadData();

            if (workDt == null)
            {
                LoadEmployees();
                //LoadLocations();
                loadLocatons();
            }
            else
            {
                dgv.DataSource = workDt;
            }

            modified = false;
            loading = false;
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
            DataTable dt = null;

            string contractsFile = "fcontracts";
            string customersFile = "fcustomers";

            string cmd = "SELECT * FROM `fcust_extended` e LEFT JOIN `fcontracts` p ON p.`contractNumber` = e.`contractNumber` left join `fcustomers` d ON e.`contractNumber` = d.`contractNumber` LEFT JOIN `icontracts` i ON i.`contractNumber` = e.`contractNumber` LEFT JOIN `icustomers` j ON j.`contractNumber` = e.`contractNumber` WHERE e.`ServiceID` <> '' ";

            string date1 = this.dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string date2 = this.dateTimePicker2.Value.ToString("yyyy-MM-dd");
            cmd += " AND p.`deceasedDate` >= '" + date1 + "' AND p.`deceasedDate` <= '" + date2 + "' ";
            //cmd += " AND p.`contractNumber` NOT LIKE 'SX%' ";

            cmd += " ORDER BY p.`deceasedDate` DESC ";
            cmd += ";";

            dt = G1.get_db_data(cmd);

            //LoadEmployees();
            LoadFuneralLocations(dt);
            //LoadLocations();

            dt.Columns.Add("num");

            Trust85.FindContract(dt, "SX25098");

            //DataView tempview = dt.DefaultView;
            //tempview.Sort = "serviceId, lastName, firstName";
            //dt = tempview.ToTable();

            G1.NumberDataTable(dt);

            originalDt = dt.Copy();

            dgv.DataSource = dt;

            newUser = "";

            string who = cmbEmployee.Text;
            if (who.ToUpper() != "ALL")
            {
                DataTable dd = (DataTable)cmbEmployee.DataSource;
                DataRow[] dRows = dd.Select("name='" + who + "'");
                if (dRows.Length > 0)
                {
                    newUser = dRows[0]["username"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( newUser ))
                        loadLocatons();
                }
            }
            else
            {
                newUser = "";
                loadLocatons();
            }
            chkComboLocation_EditValueChanged(null, null);

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadEmployees()
        {
            //repositoryItemComboBox2.Items.Clear();

            string cmd = "Select * from `tc_er` t JOIN `users` u ON t.`username` = u.`username` WHERE `empStatus` LIKE 'Full%' ";
            //string location = cmbLocation.Text.Trim();
            //if (!String.IsNullOrWhiteSpace(location) && location.ToUpper() != "ALL")
            //    cmd += " AND `location` = '" + location + "' ";
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

            for (int i = 0; i < dt.Rows.Count; i++)
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

                //repositoryItemComboBox2.Items.Add(name);
                dt.Rows[i]["name"] = name;
            }

            DataRow dR = dt.NewRow();
            dR["name"] = "All";
            dt.Rows.InsertAt(dR, 0);

            cmbEmployee.DataSource = dt;

            DataRow[] dRows = dt.Select("username='" + LoginForm.username + "'");
            if (dRows.Length > 0)
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

                if (!G1.isAdmin() && !G1.isHR())
                {
                    dt = dRows.CopyToDataTable();
                    cmbEmployee.DataSource = dt;
                }
                //gridMain.Columns["agent"].Visible = false;
                //dt.Rows.Clear();
                //dR = dt.NewRow();
                //dR["name"] = name;
                //dt.Rows.Add(dR);
            }
            //else
            //    gridMain.Columns["agent"].Visible = true;
            cmbEmployee.Text = primaryName;
        }
        /***********************************************************************************************/
        private void LoadLocations()
        {
        }
        /*******************************************************************************************/
        //private DataTable funeralsDt = null;
        //public static void LoadFuneralLocations(DataTable dt)
        //{
        //    DataTable funeralsDt = null;
        //    if (funeralsDt == null)
        //        funeralsDt = G1.get_db_data("Select * from `funeralhomes`;");
        //    string contract = "";
        //    string contractNumber = "";
        //    string trust = "";
        //    string loc = "";
        //    DateTime date = DateTime.Now;
        //    DataRow[] dR = null;
        //    if (G1.get_column_number(dt, "loc") < 0)
        //        dt.Columns.Add("loc");
        //    if (G1.get_column_number(dt, "location") < 0)
        //        dt.Columns.Add("location");
        //    if (G1.get_column_number(dt, "manager") < 0)
        //        dt.Columns.Add("manager");

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
        //            if (contractNumber == "SX221474")
        //            {
        //            }
        //            if (contractNumber == "L17035UI")
        //            {
        //            }
        //            contract = dt.Rows[i]["serviceId"].ObjToString();
        //            contract = Trust85.decodeContractNumber(contract, true, ref trust, ref loc);

        //            if (String.IsNullOrWhiteSpace(loc))
        //                continue;
        //            //dR = funeralsDt.Select("keycode='" + loc + "'");
        //            dR = funeralsDt.Select("atneedcode='" + loc + "'");
        //            if (dR.Length > 0)
        //            {
        //                dt.Rows[i]["loc"] = loc;
        //                dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
        //                dt.Rows[i]["manager"] = dR[0]["manager"].ObjToString();
        //            }
        //            else
        //            {
        //                dR = funeralsDt.Select("keycode='" + loc + "'");
        //                if (dR.Length > 0)
        //                {
        //                    dt.Rows[i]["loc"] = dR[0]["atneedcode"].ObjToString();
        //                    dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
        //                    dt.Rows[i]["manager"] = dR[0]["manager"].ObjToString();
        //                }
        //                else
        //                {
        //                    dt.Rows[i]["loc"] = loc;
        //                    dt.Rows[i]["location"] = loc;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //}
        /*******************************************************************************************/
        private DataTable funeralsDt = null;
        public static void LoadFuneralLocations(DataTable dt)
        {
            DataTable funeralsDt = null;
            if (funeralsDt == null)
                funeralsDt = G1.get_db_data("Select * from `funeralhomes`;");
            string contract = "";
            string contractNumber = "";
            string trust = "";
            string loc = "";
            DateTime date = DateTime.Now;
            DataRow[] dR = null;
            if (G1.get_column_number(dt, "loc") < 0)
                dt.Columns.Add("loc");
            if (G1.get_column_number(dt, "location") < 0)
                dt.Columns.Add("location");
            if (G1.get_column_number(dt, "manager") < 0)
                dt.Columns.Add("manager");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "SX221474")
                    {
                    }
                    if (contractNumber == "L17035UI")
                    {
                    }
                    contract = dt.Rows[i]["serviceId"].ObjToString();
                    contract = Trust85.decodeContractNumber(contract, true, ref trust, ref loc);

                    if (String.IsNullOrWhiteSpace(loc))
                        continue;
                    //dR = funeralsDt.Select("keycode='" + loc + "'");
                    dR = funeralsDt.Select("atneedcode='" + loc + "'");
                    if (dR.Length > 0)
                    {
                        dt.Rows[i]["loc"] = loc;
                        dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
                        dt.Rows[i]["manager"] = dR[0]["manager"].ObjToString();
                    }
                    else
                    {
                        dR = funeralsDt.Select("keycode='" + loc + "'");
                        if (dR.Length > 0)
                        {
                            dt.Rows[i]["loc"] = dR[0]["atneedcode"].ObjToString();
                            dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
                            dt.Rows[i]["manager"] = dR[0]["manager"].ObjToString();
                        }
                        else
                        {
                            dt.Rows[i]["loc"] = loc;
                            dt.Rows[i]["location"] = loc;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string assignedLocations = "";

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            if ( !String.IsNullOrWhiteSpace ( newUser ))
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


            chkComboLocation.Properties.DataSource = locDt;

            locations = locations.TrimEnd('|');
            chkComboLocation.EditValue = locations;
            chkComboLocation.Text = locations;
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

        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            modified = true;
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
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
        private string workLocation = "";
        private string workContract = "";
        private string workServiceId = "";
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string contractNumber = dr["contractNumber"].ObjToString();
            workContract = contractNumber;
            workLocation = dr["location"].ObjToString();
            workServiceId = dr["serviceId"].ObjToString();

            string agent = cmbEmployee.Text.Trim();

            using ( PreneedContactLeadList leadForm = new PreneedContactLeadList ( contractNumber, agent, dt ))
            {
                leadForm.contactLeadDone += leadForm_contactLeadDone;
                leadForm.ShowDialog();
            }
        }
        /****************************************************************************************/
        private void leadForm_contactLeadDone(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return;

            string agent = cmbEmployee.Text.Trim();
            if (String.IsNullOrWhiteSpace(agent))
            {
                MessageBox.Show("There is not an agent currently assigned!", "Invalid Agent Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string record = "";
            string location = workLocation;
            DateTime date = DateTime.Now;
            string apptDate = "";
            string prefix = "";
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string suffix = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string email = "";
            string phone = "";
            string phoneType = "";
            string homePhone = "";
            string mobilePhone = "";
            string workPhone = "";
            string relationship = "";
            string str = "";
            string contactStatus = "";
            DateTime dob = DateTime.Now;
            int age = 0;

            string cmd = "DELETE from `contacts_preneed` WHERE `agent` = '-1'";
            G1.get_db_data(cmd);

            DataTable oldDt = null;
            DataTable dx = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = DateTime.Now;
                apptDate = date.ToString("yyyy-MM-dd");

                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                middleName = dt.Rows[i]["depMI"].ObjToString();
                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();
                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                

                address = dt.Rows[i]["address"].ObjToString();
                city = dt.Rows[i]["city"].ObjToString();
                state = dt.Rows[i]["state"].ObjToString();
                zip = dt.Rows[i]["zip"].ObjToString();

                email = dt.Rows[i]["email"].ObjToString();
                phone = dt.Rows[i]["phone"].ObjToString().Trim();
                phoneType = dt.Rows[i]["phoneType"].ObjToString().Trim();
                homePhone = "";
                mobilePhone = "";
                workPhone = "";
                if (phoneType.ToUpper() == "HOME")
                    homePhone = phone;
                else if (phoneType == "Cell")
                    mobilePhone = phone;
                else if (phoneType == "WORK")
                    workPhone = phone;

                dob = dt.Rows[i]["depDOB"].ObjToDateTime();
                age = G1.CalculateAgeCorrect(dob, date);

                cmd = "Select * from `contacts_preneed` WHERE `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' AND `middleName` = '" + middleName + "';";
                oldDt = G1.get_db_data(cmd);
                if (oldDt.Rows.Count > 0)
                {
                    record = oldDt.Rows[0]["record"].ObjToString();
                    contactStatus = oldDt.Rows[0]["contactStatus"].ObjToString();
                    G1.update_db_table("contacts_preneed", "record", record, new string[] { "agent", agent });
                    contactStatus = oldDt.Rows[0]["contactStatus"].ObjToString();
                    if (contactStatus.Trim().ToUpper() == "RELEASED")
                        G1.update_db_table("contacts_preneed", "record", record, new string[] { "contactStatus", "" });
                    //continue;
                }
                else
                {

                    record = G1.create_record("contacts_preneed", "agent", "-1");
                    if (G1.BadRecord("contacts_preneed", record))
                        return;
                }
                G1.update_db_table("contacts_preneed", "record", record, new string[] { "agent", agent, "prospectCreationDate", apptDate, "funeralHome", location, "totalTouches", "1" });
                G1.update_db_table("contacts_preneed", "record", record, new string[] { "lastName", lastName, "firstName", firstName, "middleName", middleName, "email", email, "prefix", prefix, "suffix", suffix });
                G1.update_db_table("contacts_preneed", "record", record, new string[] { "city", city, "state", state, "zip", zip, "address", address });
                G1.update_db_table("contacts_preneed", "record", record, new string[] { "homePhone", homePhone, "workPhone", workPhone, "mobilePhone", mobilePhone, "primaryPhone", phone });
                G1.update_db_table("contacts_preneed", "record", record, new string[] { "referenceFuneral", workServiceId, "referenceTrust", workContract, "funeralRelationship", relationship });

                if (!String.IsNullOrWhiteSpace(workContract))
                {
                    cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        firstName = dx.Rows[0]["firstName"].ObjToString();
                        lastName = dx.Rows[0]["lastName"].ObjToString();
                        middleName = dx.Rows[0]["middleName"].ObjToString();
                        prefix = dx.Rows[0]["prefix"].ObjToString();
                        suffix = dx.Rows[0]["suffix"].ObjToString();

                        G1.update_db_table("contacts_preneed", "record", record, new string[] { "refDeceasedPrefix", prefix, "refDeceasedFirstName", firstName, "refDeceasedMiddleName", middleName, "refDeceasedLastName", lastName, "refDeceasedSuffix", suffix });
                    }
                }

                listUpdated = true;
            }
        }
        /****************************************************************************************/
        public delegate void d_contactListDone( bool updated );
        public event d_contactListDone contactListDone;
        protected void OnDone ( bool updated)
        {
            if (contactListDone != null)
                contactListDone( updated );
        }
        /****************************************************************************************/
        private void Contacts_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Validate();

            if (listUpdated)
                OnDone(listUpdated);

            gridMain.RefreshEditor(true);
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
        private void goToFuneralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = dr["contractNumber"].ObjToString();
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
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.gridMain.ExpandAllGroups();
            //gridMain.OptionsView.ShowFooter = showFooters;
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            DataRow[] dRows = null;
            DataTable locDt = (DataTable)this.chkComboLocation.Properties.DataSource;
            string procLoc = "";
            string jewelLoc = "";
            string[] tempIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            if (tempIDs.Length <= 0)
                return procLoc.Length > 0 ? " loc IN (" + procLoc + ") " : "";

            string[] locIDs = new string[tempIDs.Length];
            string name = "";
            for ( int i=0; i<tempIDs.Length; i++)
            {
                name = tempIDs[i].Trim();
                dRows = locDt.Select("LocationCode='" + name + "'");
                if (dRows.Length > 0)
                    locIDs[i] = dRows[0]["atneedCode"].ObjToString();
            }


            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                    dRows = locDt.Select("atneedcode='" + locIDs[i].Trim() + "'");
                    if (dRows.Length > 0)
                    {
                        jewelLoc = dRows[0]["merchandiseCode"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(jewelLoc))
                            procLoc += ",'" + jewelLoc.Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " loc IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText_1(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if (name.ToUpper().IndexOf("DATE") >= 0)
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
            else if (name.ToUpper() == "LOCATION")
            {
                string str = e.DisplayText;
                if (str.IndexOf("-") > 0)
                {
                    int idx = str.IndexOf("-");
                    str = str.Substring(idx + 1);
                    e.DisplayText = str.Trim();
                }
            }
        }
        /****************************************************************************************/
    }
}