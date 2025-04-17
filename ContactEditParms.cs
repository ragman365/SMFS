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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContactEditParms : DevExpress.XtraEditors.XtraForm
    {
        private DevExpress.XtraGrid.Views.Grid.GridView workGV = null;
        bool loading = false;
        private string workContract = "";
        private bool funModified = false;
        private bool otherModified = false;
        private string workAgent = "";
        private string workParms = "";
        private DataTable workDt = null;
        private string workModule = "";
        /****************************************************************************************/
        EditCust editCust = null;
        /****************************************************************************************/

        public ContactEditParms( string parms, DevExpress.XtraGrid.Views.Grid.GridView dgv, DataTable dt, string module )
        {
            InitializeComponent();

            workParms = parms;
            workGV = dgv;
            workDt = dt;
            workModule = module;
        }
        /****************************************************************************************/
        private void ContactEditParms_Load(object sender, EventArgs e)
        {
            //btnSaveAll.Hide();
            btnSaveData.Hide();

            funModified = false;
            otherModified = false;

            LoadReportData();

            G1.SetupToolTip(pictureBox3, "Add New Query");
            G1.SetupToolTip(pictureBox4, "Remove Query");
            G1.SetupToolTip(picRowUp, "Move Current Query Up 1 Row");
            G1.SetupToolTip(picRowDown, "Move Current Query Down 1 Row");

            gridMain6.ExpandAllGroups();
            gridMain6.RefreshEditor(true);
            gridMain6.RefreshData();
            dgv6.Refresh();
            gridMain6.Focus();
            dgv6.Focus();

            ciLookup6.SelectedIndexChanged += CiLookup6_SelectedIndexChanged;
        }
        /***********************************************************************************************/
        public DataTable saveMembersDt = null;
        public bool preprocessDone = false;

        /***************************************************************************************/
        public void FireEventFunServicesSetModified()
        {
            funModified = true;
            //this.btnSaveAll.Show();
            //this.btnSaveAll.Refresh();
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
            //btnSaveAll.Show();
            //btnSaveAll.Refresh();
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
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        RepositoryItemComboBox ciLookup6 = new RepositoryItemComboBox();
        /****************************************************************************************/
        private void ReloadTrack()
        {
            if (trackDt != null)
            {
                trackDt.Rows.Clear();
                trackDt.Dispose();
                trackDt = null;
            }
            trackDt = G1.get_db_data("Select * from `track`;");
        }
        /****************************************************************************************/
        private bool specialLoading = false;
        private void gridMain6_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (specialLoading)
                return;
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            dr["mod"] = "Y";
            otherModified = true;
            funModified = true;
            //btnSaveData.Show();
            //btnSaveData.Refresh();

            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            string what = dr["data"].ObjToString();

            what = G1.protect_data(what);

            DataTable dt6 = (DataTable)dgv6.DataSource;
            int rowHandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);
            string field = dt6.Rows[row]["field"].ObjToString();
        }
        /***************************************************************************************/
        public bool trackChange = true;
        public string whichTab = "MAIN";
        public string mainTab = "";
        public int mainRow = 0;
        public string otherTab = "";
        public int otherRow = 0;
        /****************************************************************************************/
        private void LoadReportData ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("field");
            dt.Columns.Add("operand");
            dt.Columns.Add("data");
            dt.Columns.Add("help");
            dt.Columns.Add("mod");
            dt.Columns.Add("order", Type.GetType("System.Int32"));

            dt = processInputParms(dt);

            G1.NumberDataTable(dt);

            dgv6.DataSource = dt;
            dgv6.Refresh();
        }
        /***********************************************************************************************/
        private DataTable processInputParms ( DataTable dt )
        {
            if (workDt == null)
                return dt;
            string field = "";
            string operand = "";
            string data = "";
            string help = "";
            DataRow dRow = null;
            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                field = workDt.Rows[i]["field"].ObjToString();
                operand = workDt.Rows[i]["operand"].ObjToString();
                data = workDt.Rows[i]["data"].ObjToString();
                help = workDt.Rows[i]["help"].ObjToString();

                dRow = dt.NewRow();
                dRow["field"] = field;
                dRow["operand"] = operand;
                dRow["data"] = data;
                dRow["help"] = help;

                dt.Rows.Add(dRow);
            }
            return dt;
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
        private void picRowUp_Click(object sender, EventArgs e)
        {
            if (dgv6 == null)
                return;
            if (gridMain6 == null)
                return;

            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowHandle = gridMain6.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);

            MoveRowUp(dt, row);

            dt.AcceptChanges();
            dgv6.DataSource = dt;
            gridMain6.ClearSelection();
            gridMain6.SelectRow(rowHandle - 1);
            gridMain6.FocusedRowHandle = rowHandle - 1;
            gridMain6.RefreshData();
            dgv6.Refresh();
            //btnSaveAll.Show();
            funModified = true;
            otherModified = true;
        }
        /***************************************************************************************/
        private void MoveRowUp(DataTable dt, int row)
        {
            dt.AcceptChanges();
            if (G1.get_column_number(dt, "Count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();

            dt.Rows[row]["Count"] = (row - 1).ToString();
            string order = dt.Rows[row]["order"].ObjToString();
            string record = dt.Rows[row]["record"].ObjToString();

            dt.Rows[row - 1]["Count"] = row.ToString();
            string order1 = dt.Rows[row-1]["order"].ObjToString();
            string record1 = dt.Rows[row-1]["record"].ObjToString();

            dt.Rows[row]["order"] = order1;
            G1.update_db_table("contacts_reports", "record", record, new string[] { "order", order1 });

            dt.Rows[row - 1]["order"] = order;
            G1.update_db_table("contacts_reports", "record", record1, new string[] { "order", order });

            G1.sortTable(dt, "Count", "asc");

            dt.Columns.Remove("Count");
            G1.NumberDataTable(dt);

        }
        /****************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            if (dgv6 == null)
                return;
            if (gridMain6 == null)
                return;

            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowHandle = gridMain6.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);

            MoveRowDown(dt, row);

            dt.AcceptChanges();
            dgv6.DataSource = dt;
            gridMain6.ClearSelection();
            gridMain6.SelectRow(rowHandle + 1);
            gridMain6.FocusedRowHandle = rowHandle + 1;
            gridMain6.RefreshData();
            dgv6.Refresh();
            funModified = true;
            otherModified = true;
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            string order = dt.Rows[row]["order"].ObjToString();
            string record = dt.Rows[row]["record"].ObjToString();

            dt.Rows[row + 1]["Count"] = row.ToString();
            string order1 = dt.Rows[row + 1]["order"].ObjToString();
            string record1 = dt.Rows[row + 1]["record"].ObjToString();

            dt.Rows[row]["order"] = order1;
            G1.update_db_table("contacts_reports", "record", record, new string[] { "order", order1 });

            dt.Rows[row + 1]["order"] = order;
            G1.update_db_table("contacts_reports", "record", record1, new string[] { "order", order });

            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /****************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        { // Add New Report Data Row
            int row = AddNewDataRow();

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

            otherModified = true;
        }
        /***********************************************************************************************/
        private string getCurrentReportRecord ()
        {
            string record = "";
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string record = dr["record"].ObjToString();
            return record;
        }
        /***********************************************************************************************/
        private int AddNewDataRow()
        {
            int row = -1;
            try
            {
                DataTable dt = (DataTable)dgv6.DataSource;

                //string reportRecord = getCurrentReportRecord();

                //string record = G1.create_record("contacts_reports_data", "spare", "-1");
                //if (G1.BadRecord("contacts_reports_data", record))
                //    return row;

                DataRow dRow = dt.NewRow();
                dRow["num"] = (dt.Rows.Count + 1).ToString();
                //dRow["record"] = record;
                //dRow["reportRecord"] = reportRecord;
                dRow["field"] = "New Field";
                dRow["mod"] = "Y";
                dRow["order"] = dt.Rows.Count;

                dt.Rows.Add(dRow);

                //G1.update_db_table("contacts_reports_data", "record", record, new string[] { "order", dt.Rows.Count.ToString(), "spare", "", "reportRecord", reportRecord, "field", "New Field" });

                row = dt.Rows.Count;
                dgv6.DataSource = dt;
                dgv6.Refresh();
                //gridMainDep_CellValueChanged(null, null);
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

            string field = dr["field"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Data Field from Report\n(" + field + ") ?", "Delete Data Row Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv6.DataSource;
            if (dt == null)
                return;

            //string record = dr["record"].ObjToString();

            //G1.delete_db_table("contacts_reports_data", "record", record);

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

            otherModified = true;
        }
        /****************************************************************************************/
        private void btnSaveData_Click(object sender, EventArgs e)
        {
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
            //string record = getCurrentReportRecord();
            //if (String.IsNullOrWhiteSpace(record))
            //    return;

            //string cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
            //DataTable dt = G1.get_db_data(cmd);

            DataTable dt = (DataTable)dgv6.DataSource;

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
            bool gotStatus = true;
            if (G1.get_column_number(dt, "status") < 0)
                gotStatus = false;

            DataTable workDt = null;

            string cmd = "Select * from `contacts_preneed` WHERE ";
            if ( workModule.ToUpper() == "CONTACTS")
                cmd = "Select * from `contacts` WHERE ";
            bool found = false;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                if (String.IsNullOrWhiteSpace(data))
                    continue;
                if (gotStatus)
                {
                    status = dt.Rows[i]["status"].ObjToString();
                    if (status.ToUpper() == "OFF")
                        continue;
                }

                operand = dt.Rows[i]["operand"].ObjToString();

                body = data.Trim();

                date = body.ObjToDateTime();
                if ( date.Year < 1000 )
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
                        operand = "<";
                    }
                    else if (operand == "<")
                    {
                        iBody = body.ObjToInt32();
                        today = today.AddDays(iBody * -1 );
                        operand = ">";
                    }
                    else
                        continue;
                    if (found)
                        cmd += " AND ";
                    cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
                    found = true;
                }
                else
                {
                    cmd += " `" + field + "` " + operand + " '" + date.ToString("yyyy-MM-dd") + "' ";
                    found = true;
                }
            }

            if ( !found )
            {
                MessageBox.Show("Search Criteria is Empty!", "Search Criteria Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (!String.IsNullOrWhiteSpace(workAgent))
            {
                if (workAgent.ToUpper() != "ALL")
                    cmd += " AND `agent` = '" + workAgent + "' ";
            }

            cmd += ";";
            dx = G1.get_db_data(cmd);

            if ( dx != null )
            {
                this.Cursor = Cursors.WaitCursor;
                int height = this.Height;
                DevExpress.XtraEditors.XtraForm form = null;
                if ( workModule.ToUpper() == "CONTACTS")
                    form = new Contacts( dx, "Some Report" );
                else
                    form = new ContactsPreneed(dx, "Some Report" );
                //leadForm.StartPosition = FormStartPosition.CenterParent;
                form.Show();
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
        private void ContactReports_FormClosing(object sender, FormClosingEventArgs e)
        {
            gridMain6.PostEditor();

            if ( otherModified )
            {
                this.TopMost = false;
                DialogResult result = MessageBox.Show("***Question***\nDetails have been modified!\nWould you like to SAVE these Details?", "Details Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                    return;
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            OnDone();
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

            string status = dr["status"].ObjToString();
            if (status.ToUpper() == "OFF")
                status = "On";
            else
                status = "Off";

            dr["status"] = status;
            gridMain6.RefreshData();
            gridMain6.RefreshEditor(true);

            string record = dr["record"].ObjToString();
            G1.update_db_table("contacts_reports_data", "record", record, new string[] { "status", status });
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

            if (currentColumn.ToUpper() != "DATA" && currentColumn.ToUpper() != "FIELD" )
            {
                if (currentColumn.ToUpper() == "OPERAND")
                    return;
                currCol.ColumnEdit = null;
                return;
            }

            if (currentColumn.ToUpper() == "FIELD")
                return;

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
                for ( int i=0; i<itemBox.Items.Count; i++)
                {
                    string str = itemBox.Items[i].ToString();
                    ciLookup6.Items.Add(str);
                }
                currCol.ColumnEdit = null;
                currCol.ColumnEdit = ciLookup6;
                gridMain6.RefreshData();
                gridMain6.RefreshEditor(true);
                gridMain6.PostEditor();
            }
        }
        /****************************************************************************************/
        public delegate void d_contactParmsDone( string parms );
        public event d_contactParmsDone contactParmsDone;
        private void OnDone()
        {
            if (contactParmsDone != null && otherModified )
            {
                string parms = decodeParms();

                contactParmsDone(parms);
            }
        }
        /****************************************************************************************/
        private string decodeParms ()
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            string parms = "";
            string field = "";
            string operand = "";
            string data = "";
            string help = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                operand = dt.Rows[i]["operand"].ObjToString();
                if (String.IsNullOrWhiteSpace(operand))
                    operand = "NoOperand";
                data = dt.Rows[i]["data"].ObjToString();
                if (String.IsNullOrWhiteSpace(data))
                    data = "NoData";
                help = dt.Rows[i]["help"].ObjToString();
                if (String.IsNullOrWhiteSpace(help))
                    help = "NoHelp";

                parms += field + " " + operand + " " + data + " #" + help + "~";
            }
            parms = parms.TrimEnd('~');
            return parms;
        }
        /***************************************************************************************/
        private void CiLookup6_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            try
            {
                string help = dt.Rows[row]["help"].ObjToString();
                //string dbField = dt.Rows[row]["dbField"].ObjToString();
                string myData = dt.Rows[row]["data"].ObjToString().Trim();
                myData = G1.protect_data(myData);
                string str = "";

                ComboBoxEdit combo = (ComboBoxEdit)sender;
                //            string what = combo.Text.Trim().ToUpper();
                string what = combo.Text.Trim();
                what = G1.protect_data(what);
                dr["data"] = what; // ramma zamma

                funModified = true;
                //btnSaveAll.Show();

                DataTable tempDt = null;

            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
    }
}