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
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditContacts : DevExpress.XtraEditors.XtraForm
    {
        private string workField = "";
        private bool modified = false;
        public static string trackingSelection = "";
        private DataTable originalDt = null;
        private string workLocation = "";
        private string workRecord = "";
        private bool loading = true;
        private bool workingContactType = false;
        private string workContactType = "";
        private string workDetail = "";
        /****************************************************************************************/
        public EditContacts( string field, string location = "", string record = "", bool allowContactType = false )
        {
            InitializeComponent();
            workField = field;
            workContactType = "";
            trackingSelection = "";
            workLocation = location;
            workRecord = record;
            workingContactType = false;
        }
        /****************************************************************************************/
        public EditContacts(bool useContactTypes, string contactType, string detail, string location = "", string record = "")
        {
            InitializeComponent();
            workContactType = contactType;
            workField = "";
            trackingSelection = "";
            workLocation = location;
            workRecord = record;
            workingContactType = true;
            workDetail = detail;
        }
        /****************************************************************************************/
        private void EditContacts_Load(object sender, EventArgs e)
        {
            this.btnSave.Hide();

            if (!workingContactType)
            {
                gridMain.Columns["contactType"].Visible = false;
                lblTracks.Hide();
                cmbTrack.Hide();
            }

            if ( String.IsNullOrWhiteSpace ( workDetail ))
            {
                if (workField.Trim().ToUpper() == "SRVCLERGY")
                    workDetail = "PERSON";
            }

            this.Text = "Edit Data for Tracking " + workField;
            if ( String.IsNullOrWhiteSpace ( workField ))
                this.Text = "Edit Data for Tracking and Contacts";

            string cmd = "Select * from `track` where `tracking` = '" + workField + "';";
            cmd = "Select * from `track` t ;";

            cmd = "SELECT *, (SELECT `apptDate` FROM `contacts` c WHERE c.`contactName` = t.`answer` ORDER BY c.`apptDate` DESC LIMIT 1) AS `apptContactDate` FROM `track` t;";

            //cmd = "SELECT *, (SELECT `apptDate` FROM `contacts` c WHERE c.`contactName` = t.`contactName` ORDER by `c.`apptDate` DESC LIMIT 1 ) AS `apptContact` FROM `track`  where `tracking` = '" + workField + "';";

            //if (!String.IsNullOrWhiteSpace(workContactType))
            //{
            //    this.Text = "Edit Data for Tracking " + workContactType;
            //    cmd = "Select * from `track` where `contactType` = '" + workContactType + "';";
            //}
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            originalDt = dt;
            dgv.DataSource = dt;
            //if (workDetail.ToUpper() == "PLACE")
            //{
            //    gridMain.Columns["depPrefix"].Visible = false;
            //    gridMain.Columns["depFirstName"].Visible = false;
            //    gridMain.Columns["depMI"].Visible = false;
            //    gridMain.Columns["depLastName"].Visible = false;
            //    gridMain.Columns["depSuffix"].Visible = false;
            //}
            //else if ( workDetail.ToUpper() == "PERSON" )
            //{
            //    gridMain.Columns["answer"].Visible = false;
            //}
            loadLocatons();
            if (!String.IsNullOrWhiteSpace(workLocation))
            {
                chkComboLocNames.EditValue = "All | " + workLocation;
                chkComboLocNames.Text = "All | " + workLocation;
            }
            FindRecordRow(dt);

            LoadAssociations();
            LoadContactTypes();

            LoadTracks();

            loading = false;
        }
        /***********************************************************************************************/
        private void LoadContactTypes()
        {
            repositoryItemComboBox1.Items.Clear();
            cmbContractType.Items.Clear();
            cmbContractType.Items.Add("All");

            string contactType = "";

            string cmd = "Select * from `contacttypes`;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contactType = dt.Rows[i]["contactType"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contactType))
                {
                    repositoryItemComboBox1.Items.Add(contactType);
                    cmbContractType.Items.Add(contactType);
                }
            }

            cmbContractType.Text = "All";
            //trackDt = G1.get_db_data("Select * from `track`;");
            //ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
        }
        /***********************************************************************************************/
        private void LoadTracks ()
        {
            string tracking = "";
            string use = "";
            string cmd = "Select * from `tracking`;";
            DataTable dt = G1.get_db_data(cmd);
            cmbTrack.Items.Clear();
            cmbTrack.Items.Add("All");
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                tracking = dt.Rows[i]["tracking"].ObjToString();
                use = dt.Rows[i]["using"].ObjToString();
                if (tracking != use)
                    continue;
                cmbTrack.Items.Add(tracking);
            }
            cmbTrack.Text = "All";

            if ( String.IsNullOrWhiteSpace ( workField ))
            {
                if (String.IsNullOrWhiteSpace(workContactType))
                    return;
                cmd = "Select * from `track` WHERE `contactType` = '" + workContactType + "' GROUP by `contactType`;";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0 )
                    workField = dt.Rows[0]["tracking"].ObjToString();
            }
        }
        /***********************************************************************************************/
        private void LoadAssociations ()
        {
            string cmd = "Select * from `associations`;";
            DataTable dt = G1.get_db_data(cmd);

            cmbAssociations.Items.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
                cmbAssociations.Items.Add(dt.Rows[i]["association"].ObjToString());
            cmbAssociations.Refresh();
        }
        /***********************************************************************************************/
        private void FindRecordRow ( DataTable dt )
        {
            if (String.IsNullOrWhiteSpace(workRecord))
                return;
            string record = "";

            for (int i = 0; i < gridMain.DataRowCount; i++)
            {
                if (gridMain.GetRowCellValue(i, "record").ObjToString() == workRecord)
                {
                    gridMain.ClearSelection();
                    gridMain.FocusedRowHandle = i;

                    gridMain.RefreshData();
                    gridMain.RefreshEditor(true);
                    //dgv.Refresh();
                    gridMain.SelectRow(i);
                    gridMain.FocusedRowHandle = i;
                    break;
                }
            }
        }
        /***********************************************************************************************/
        private DataTable funDt = null;
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            string locationCode = "";
            for ( int i=0; i<locDt.Rows.Count; i++)
            {
                locationCode = locDt.Rows[i]["locationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                    locDt.Rows[i]["locationCode"] = locDt.Rows[i]["name"].ObjToString();
            }
            DataRow dRow = locDt.NewRow();
            dRow["locationCode"] = "All";
            locDt.Rows.InsertAt(dRow, 0);

            chkComboLocNames.Properties.DataSource = locDt;
            funDt = locDt.Copy();

            DataView tempview = funDt.DefaultView;
            tempview.Sort = "locationCode asc";
            funDt = tempview.ToTable();

            this.repositoryItemComboBox1.Items.Clear();

            int count = 0;
            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                locationCode = funDt.Rows[i]["locationCode"].ObjToString();
                this.repositoryItemComboBox1.Items.Add(locationCode);
                count++;
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string record = "";
            string answer = "";
            string location = "";
            string mod = "";
            string address = "";
            string city = "";
            string county = "";
            string state = "";
            string zip = "";
            string phone = "";
            string contactType = "";
            string ff = "";
            string email = "";
            string pocName = "";
            string pocTitle = "";
            string pocPhone = "";
            string pocEmail = "";

            string prefix = "";
            string firstName = "";
            string mi = "";
            string lastName = "";
            string suffix = "";

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                if (mod == "D" && String.IsNullOrWhiteSpace(record))
                    continue;
                if ( mod == "D")
                {
                    G1.delete_db_table("track", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(mod))
                    continue;

                if (String.IsNullOrWhiteSpace(record))
                    record = "-1";
                if (record == "-1")
                    record = G1.create_record("track", "answer", "-1");
                if (G1.BadRecord("track", record))
                    continue;
                answer = dt.Rows[i]["answer"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    location = EditCust.activeFuneralHomeName;
                address = dt.Rows[i]["address"].ObjToString();
                city = dt.Rows[i]["city"].ObjToString();
                county = dt.Rows[i]["county"].ObjToString();
                state = dt.Rows[i]["state"].ObjToString();
                zip = dt.Rows[i]["zip"].ObjToString();
                phone = dt.Rows[i]["phone"].ObjToString();
                contactType = dt.Rows[i]["contactType"].ObjToString();
                email = dt.Rows[i]["email"].ObjToString();
                pocName = dt.Rows[i]["pocName"].ObjToString();
                pocTitle = dt.Rows[i]["pocTitle"].ObjToString();
                pocPhone = dt.Rows[i]["pocPhone"].ObjToString();
                pocEmail = dt.Rows[i]["pocEmail"].ObjToString();
                if (workingContactType)
                {
                    ff = dt.Rows[i]["tracking"].ObjToString();
                    G1.update_db_table("track", "record", record, new string[] { "contactType", contactType, "tracking", ff, "answer", answer, "location", location, "address", address, "city", city, "county", county, "state", state, "zip", zip, "phone", phone, "email", email, "pocName", pocName, "pocTitle", pocTitle, "pocPhone", pocPhone, "pocEmail", pocEmail });
                }
                else
                {
                    G1.update_db_table("track", "record", record, new string[] { "contactType", contactType, "tracking", workField, "answer", answer, "location", location, "address", address, "city", city, "county", county, "state", state, "zip", zip, "phone", phone, "email", email, "pocName", pocName, "pocTitle", pocTitle, "pocPhone", pocPhone, "pocEmail", pocEmail });
                }

                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                mi = dt.Rows[i]["depMI"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();

                //if (workField.ToUpper() == "SRVCLERGY")
                if ( workDetail.ToUpper() == "PERSON")
                {
                    answer = prefix;
                    if (!String.IsNullOrWhiteSpace(answer))
                        answer += " ";
                    answer += firstName;
                    if (!String.IsNullOrWhiteSpace(answer))
                        answer += " ";
                    answer += mi;
                    if (!String.IsNullOrWhiteSpace(answer))
                        answer += " ";
                    answer += lastName;
                    if (!String.IsNullOrWhiteSpace(answer))
                        answer += " ";
                    answer += suffix;
                    G1.update_db_table("track", "record", record, new string[] { "answer", answer, "depPrefix", prefix, "depFirstName", firstName, "depMI", mi, "depLastName", lastName, "depSuffix", suffix });
                    dt.Rows[i]["answer"] = answer;
                }

                dt.Rows[i]["mod"] = "";
                dt.Rows[i]["record"] = record;
            }
            modified = false;
            btnSave.Hide();
            if (!String.IsNullOrWhiteSpace(workRecord))
                this.Close();
            this.Cursor = Cursors.Default;

        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
            //if (e.Column.FieldName.ToUpper() == "ANSWER" && workField.ToUpper() == "SRVCLERGY")
            if ( e.Column.FieldName.ToUpper() == "ANSWER" && workDetail.ToUpper() == "PERSON" )
            {
                string prefix = "";
                string firstName = "";
                string mi = "";
                string lastName = "";
                string suffix = "";
                string answer = dr["answer"].ObjToString();

                G1.ParseOutName(answer, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                dr["depPrefix"] = prefix;
                dr["depFirstName"] = firstName;
                dr["depMI"] = mi;
                dr["depLastName"] = lastName;
                dr["depSuffix"] = suffix;
            }
        }
        /****************************************************************************************/
        private void picDelete_Click(object sender, EventArgs e)
        {
            int row = 0;
            int[] Rows = gridMain.GetSelectedRows();
            for (int i = 0; i < Rows.Length; i++)
            {
                modified = true;
                row = Rows[i];
                DataRow dr = gridMain.GetDataRow(row);
                dr["mod"] = "D";
                btnSave.Show();
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
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (loading)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
            string cType = cmbContractType.Text.Trim().ToUpper();
            if (cType == "ALL")
                return;

            string contactType = dt.Rows[row]["contactType"].ObjToString().ToUpper();
            if (contactType != cType)
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            if (!String.IsNullOrWhiteSpace(workContactType))
                dRow["contactType"] = workContactType;
            if (!String.IsNullOrWhiteSpace(workField))
                dRow["tracking"] = workField;
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            int row = dt.Rows.Count - 1;
            gridMain.SelectRow(row);
            gridMain.FocusedRowHandle = row;
            gridMain.RefreshData();
            dgv.RefreshDataSource();
            dgv.Refresh();

            modified = true;
        }
        /****************************************************************************************/
        private void EditTracking_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Add/Edit Tracking Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string record = dr["record"].ObjToString();

            try
            {
                string contactType = dr["contactType"].ObjToString();
                string contactName = dr["answer"].ObjToString();

                if (String.IsNullOrWhiteSpace(contactName))
                {
                    string cmd = "Select * from `contactTypes` WHERE `contactType` = '" + contactType + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return;

                    string workDetail = dx.Rows[0]["detail"].ObjToString();
                    if (workDetail.ToUpper() == "PERSON")
                        contactName = Contacts.GetPerson(dr);
                }
                if (!String.IsNullOrWhiteSpace(contactName))
                {
                    this.Cursor = Cursors.WaitCursor;
                    ContactHistory historyForm = new ContactHistory(gridMain, dt, row, record, contactType, contactName, null );
                    historyForm.Show();
                    this.Cursor = Cursors.Default;
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string names = getLocationNameQuery();
            if (!String.IsNullOrWhiteSpace(names))
            {
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);

                names = getLocationNameQuery();
                dRows = dt.Select(names);
                DataTable newdt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    newdt.ImportRow(dRows[i]);
                G1.NumberDataTable(newdt);
                dgv.DataSource = newdt;
            }
            else
            {
                names = getLocationNameQuery();
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
            }
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `location` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void btnShowAll_Click(object sender, EventArgs e)
        {
            string text = btnShowAll.Text.Trim().ToUpper();
            if (text == "SHOW ALL")
            {
                DataTable newdt = originalDt.Copy();
                G1.NumberDataTable(newdt);
                dgv.DataSource = newdt;
                btnShowAll.Text = "Show Filtered";
            }
            else
            {
                chkComboLocNames_EditValueChanged(null, null);
                btnShowAll.Text = "Show All";
            }
            btnShowAll.Refresh();
        }
        /****************************************************************************************/
        private void changeLocationToAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string record = dr["record"].ObjToString();
            dr["location"] = "All";
            dr["mod"] = "Y";
            dt.Rows[row]["location"] = "All";
            btnSave.Show();
            modified = true;
        }
        /****************************************************************************************/
        private void changeLocationFromAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = chkComboLocNames.Text.Trim();
            string[] Lines = data.Split('|');
            if (Lines.Length <= 0)
                return;
            string location = Lines[0].Trim();

            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowHandle);
            //DataTable dt = (DataTable)dgv.DataSource;
            //string record = dr["record"].ObjToString();
            //dr["location"] = location;
            //dr["mod"] = "Y";
            //dt.Rows[row]["location"] = location;
            //btnSave.Show();
            //modified = true;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            using (EditAssociations editForm = new EditAssociations())
            {
                editForm.ShowDialog();
                if (editForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                string locations = editForm.A_Answer;
                if (String.IsNullOrWhiteSpace(locations))
                    return;
                chkComboLocNames.EditValue = locations;
                chkComboLocNames.Text = locations;

                LoadAssociations();

                chkComboLocNames.Text = locations;
            }
        }
        /****************************************************************************************/
        private void cmbAssociations_SelectedIndexChanged(object sender, EventArgs e)
        {
            string association = cmbAssociations.Text;
            if (String.IsNullOrWhiteSpace(association))
                return;
            string cmd = "Select * from `associations` where `association` = '" + association + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            chkComboLocNames.EditValue = dt.Rows[0]["locations"].ObjToString();
            chkComboLocNames.Text = dt.Rows[0]["locations"].ObjToString();
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

            Printer.setupPrinterMargins(10, 5, 80, 50);

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

            Printer.setupPrinterMargins(10, 5, 80, 50);

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

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            //if (workReport == "ACH Detail Report")
            //    title = "The First Drafts";
            //else
            //    title = "The First (All)";
            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //DateTime date = this.dateTimePicker1.Value;
            //string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void cmbTrack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string track = cmbTrack.Text.Trim();
            if (String.IsNullOrWhiteSpace(track))
                return;
            string cmd = "Select * from `track` WHERE `tracking` = '" + track + "';";
            if (track.ToUpper() == "ALL")
                cmd = "Select * from `track`;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            G1.NumberDataTable(dx);

            int row = 0;
            string str = "";

            string contactType = cmbContractType.Text.Trim();

            loading = true;

            DataTable dt = (DataTable)dgv.DataSource;
            dt.Rows.Clear();
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                G1.copy_dt_row(dx, i, dt, dt.Rows.Count);
                row = dt.Rows.Count - 1;
                if (track.ToUpper() != "ALL")
                {
                    str = dt.Rows[row]["contactType"].ObjToString();
                    if (str != track)
                    {
                        if (!String.IsNullOrWhiteSpace(contactType))
                            dt.Rows[row]["contactType"] = contactType;
                        else
                            dt.Rows[row]["contactType"] = track;
                        dt.Rows[row]["mod"] = "Y";
                    }
                }
            }
            dgv.DataSource = dt;
            dgv.Refresh();

            btnSave.Show();
            btnSave.Refresh();

            loading = false;
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void cmbContractType_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /****************************************************************************************/
    }
}