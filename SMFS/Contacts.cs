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
    public partial class Contacts : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string primaryName = "";
        /****************************************************************************************/
        public Contacts()
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
        private void Contacts_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            SetupToolTips();

            loading = true;

            DateTime now = DateTime.Now;
//            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            LoadContactTypes();
            LoadEmployees();
            LoadLocations();

            LoadData();

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

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            date = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            string date2 = G1.DateTimeToSQLDateTime(date);

            string employee = cmbEmployee.Text.Trim();


            string cmd = "Select * from `contacts` WHERE `apptDate` >= '" + date1 + "' AND `apptDate` <= '" + date2 + "' ";
            if (!String.IsNullOrWhiteSpace(employee) && employee.ToUpper() != "ALL" )
                cmd += " AND `agent` = '" + employee + "' ";
            cmd += " ORDER BY `apptDate` desc ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            AddMod(dt, gridMain);

            SetupCompleted ( dt );

            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
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
            if ( dRows.Length > 0 )
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
            }
        }
        /***********************************************************************************************/
        private void LoadContactTypes ()
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
        private void LoadLocations()
        {
            string cmd = "Select * from `funeralhomes` order by `LocationCode`;";
            DataTable locDt = G1.get_db_data(cmd);
            DataRow dRow = locDt.NewRow();
            dRow["LocationCode"] = "All";
            locDt.Rows.InsertAt(dRow, 0);
            cmbLocation.DataSource = locDt;
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
            if (currentColumn.ToUpper() == "contactName")
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
                DialogResult result = MessageBox.Show("Permanently Delete This Contact?", "Delete Contact Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
            using ( ContactHistory historyForm = new ContactHistory ( contactType, contactName ))
            {
                historyForm.contactHistoryDone += HistoryForm_contactHistoryDone;
                historyForm.ShowDialog();
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
            EditContacts contactForm = new EditContacts(true, "", "");
            contactForm.Show();
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
    }
}