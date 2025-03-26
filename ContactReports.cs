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
    public partial class ContactReports : DevExpress.XtraEditors.XtraForm
    {
        private DevExpress.XtraGrid.Views.Grid.GridView workGV = null;
        bool loading = false;
        private string workContract = "";
        private bool funModified = false;
        private bool otherModified = false;
        private string workAgent = "";
        /****************************************************************************************/
        EditCust editCust = null;
        /****************************************************************************************/

        public ContactReports( string agent, DevExpress.XtraGrid.Views.Grid.GridView dgv )
        {
            InitializeComponent();

            workAgent = agent;
            workGV = dgv;
        }
        /****************************************************************************************/
        private void ContactReports_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            btnSaveData.Hide();

            funModified = false;
            otherModified = false;

            RemoveTabPage("Report Information");
            chkReports.Hide();
            lblSelect.Hide();

            LoadData();

            G1.SetupToolTip(pictureBox12, "Add New Report");
            G1.SetupToolTip(pictureBox11, "Remove Report");
            G1.SetupToolTip(picRowUp, "Move Current Report Up 1 Row");
            G1.SetupToolTip(picRowDown, "Move Current Report Down 1 Row");

            gridMain6.ExpandAllGroups();
            gridMain6.RefreshEditor(true);
            gridMain6.RefreshData();
            dgv6.Refresh();
            gridMain6.Focus();
            dgv6.Focus();
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
            string cmd = "Select * from `contacts_reports` order by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");

            string field = "";
            string caption = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["order"] = i + 1;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***************************************************************************************/
        public void FireEventFunServicesSetModified()
        {
            funModified = true;
            this.btnSaveAll.Show();
            this.btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            int row = AddNewRow();

            int rowHandle = row - 1;
            gridMain.FocusedRowHandle = rowHandle;
            gridMain.SelectRow(rowHandle);
            if ( gridMain.VisibleColumns.Count > 0)
            {
                GridColumn firstColumn = gridMain.Columns["report"];
                gridMain.FocusedColumn = gridMain.Columns[firstColumn.FieldName];
            }
            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            this.ForceRefresh();
        }
        /***********************************************************************************************/
        private int AddNewRow()
        {
            int row = -1;
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;

                string record = G1.create_record("contacts_reports", "spare", "-1");
                if (G1.BadRecord("contacts_reports", record))
                    return row;

                DataRow dRow = dt.NewRow();
                dRow["num"] = (dt.Rows.Count + 1).ToString();
                dRow["record"] = record;
                dRow["report"] = "New Report";
                dRow["order"] = dt.Rows.Count;

                dt.Rows.Add(dRow);

                G1.update_db_table("contacts_reports", "record", record, new string[] { "order", dt.Rows.Count.ToString(), "spare", "" });

                row = dt.Rows.Count;
                dgv.DataSource = dt;
                dgv.Refresh();
                gridMainDep_CellValueChanged(null, null);
            }
            catch ( Exception ex )
            {
            }
            return row;
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
            if (current.Name.ToUpper() == "TABREPORT")
            {
                dt.AcceptChanges();
                string record = dr["record"].ObjToString();
                string report = dr["report"].ObjToString();
                G1.update_db_table("contacts_reports", "record", record, new string[]{ "report", report });
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

            if (current.Name.ToUpper() == "TABREPORT")
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

            if (current.Name.ToUpper() == "TABREPORT")
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

            if (current.Name.ToUpper() == "TABREPORT")
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

            if (current.Name.ToUpper() == "TABREPORT")
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
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);

            string report = dr["report"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Report\n(" + report + ") ?", "Delete Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv.DataSource;

            string record = dr["record"].ObjToString();
            string rec = "";

            DataTable dx = G1.get_db_data("Select * from `contacts_reports_data` where `reportRecord` = '" + record + "';");
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                rec = dx.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(rec))
                    G1.delete_db_table("contacts_reports_data", "record", rec);
            }

            G1.delete_db_table("contacts_reports", "record", record);

            try
            {
                gridMain.DeleteRow(gridMain.FocusedRowHandle);
                dt.Rows.Remove(dr);
                dt.AcceptChanges();
            }
            catch (Exception ex)
            {
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();
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
        private void LoadOtherData()
        {
            trackingDt = G1.get_db_data("Select * from `tracking`;");
            trackDt = G1.get_db_data("Select * from `track`;");
            ciLookup6.SelectedIndexChanged += CiLookup6_SelectedIndexChanged;
            //ciLookup6.KeyPress += CiLookup6_KeyPress;
            ciLookup6.Popup += CiLookup6_Popup;

            string dbfield = "";
            string data = "";
            DataRow[] dR = null;
            string cmd = "Select * from `cust_extended_layout` WHERE `group` <> 'Vital Statistics' ORDER BY `order`;";
            //            string cmd = "Select * from `cust_extended_layout` ORDER by `order`;";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("num");
            dx.Columns.Add("mod");
            dx.Columns.Add("data");
            dx.Columns.Add("add");
            dx.Columns.Add("edit");
            dx.Columns.Add("tracking");
            dx.Columns.Add("dropOnly");
            dx.Columns.Add("addContact");
            cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                int groupNumber = 1;
                string oldGroup = "";
                string group = "";
                string type = "";
                string help = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    group = dx.Rows[i]["group"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldGroup))
                        oldGroup = group;
                    if (String.IsNullOrWhiteSpace(group))
                        group = oldGroup;
                    if (group != oldGroup)
                        groupNumber++;
                    oldGroup = group;
                    group = groupNumber.ToString() + ". " + group;
                    dx.Rows[i]["group"] = group;
                    dbfield = dx.Rows[i]["dbfield"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dbfield))
                    {
                        if (G1.get_column_number(dt, dbfield) >= 0)
                        {
                            data = dt.Rows[0][dbfield].ObjToString();
                            dx.Rows[i]["data"] = data;
                        }
                    }
                    dR = trackingDt.Select("tracking='" + dbfield + "'");
                    if (dR.Length > 0)
                    {
                        dx.Rows[i]["help"] = "Tracking";
                        dx.Rows[i]["tracking"] = "T";
                        dx.Rows[i]["dropOnly"] = dR[0]["dropOnly"].ObjToString();
                        dx.Rows[i]["addContact"] = dR[0]["addContact"].ObjToString();
                    }
                    else
                    {
                        help = dx.Rows[i]["help"].ObjToString();
                        type = dx.Rows[i]["type"].ObjToString();
                        if (type.ToUpper() == "DATE" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select Date";
                        else if (type.ToUpper() == "FULLDATE" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select Date";
                        else if (type.ToUpper() == "DAY" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select the Day of the Week";
                    }
                }
            }
            else
            {
                int groupNumber = 1;
                string oldGroup = "";
                string group = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    group = dx.Rows[i]["group"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldGroup))
                        oldGroup = group;
                    if (String.IsNullOrWhiteSpace(group))
                        group = oldGroup;
                    if (group != oldGroup)
                        groupNumber++;
                    oldGroup = group;
                    group = groupNumber.ToString() + ". " + group;
                    dx.Rows[i]["group"] = group;
                    dbfield = dx.Rows[i]["dbfield"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dbfield))
                    {
                        if (G1.get_column_number(dt, dbfield) >= 0)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                data = dt.Rows[0][dbfield].ObjToString();
                                dx.Rows[i]["data"] = data;
                            }
                        }
                    }
                    dR = trackingDt.Select("tracking='" + dbfield + "'");
                    if (dR.Length > 0)
                        dx.Rows[i]["help"] = "Tracking";
                }
            }
            gridMain6.Columns["num"].Visible = false;
            G1.NumberDataTable(dx);
            dgv6.DataSource = dx;
            otherModified = false;
            gridMain6.ExpandAllGroups();
            gridMain6.RefreshEditor(true); // RAMMA ZAMMA
            gridMain6.RefreshData();
            dgv6.Refresh();
            dgv6.Focus();
            gridMain6.Focus();
        }
        /***************************************************************************************/
        private void CiLookup6_Popup(object sender, EventArgs e)
        {
            //popupForm6 = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            //popupForm6.ListBox.MouseMove += ListBox6_MouseMove;
            //popupForm6.ListBox.MouseDown += ListBox6_MouseDown;
            //popupForm6.ListBox.SelectedValueChanged += ListBox6_SelectedValueChanged;
        }
        /****************************************************************************************/
        //private int lastIndex = -1;
        //private int whichRowChanged = -1;
        //private FuneralDemo funDemo = null;
        /****************************************************************************************/
        private void ListBox6_SelectedValueChanged(object sender, EventArgs e)
        {
            if (1 == 1) //ramma zamma
                return;
        }
        /****************************************************************************************/
        private void ListBox6_MouseMove(object sender, MouseEventArgs e)
        {
            PopupListBox listBoxControl = sender as PopupListBox;
            ComboBoxEdit cmb = listBoxControl.OwnerEdit as ComboBoxEdit;
            int index = listBoxControl.IndexFromPoint(new Point(e.X, e.Y));
            if (index < 0)
            {
                if (e.Y > listBoxControl.Height)
                {
                }
            }
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
                string dbField = dt.Rows[row]["dbField"].ObjToString();
                string myData = dt.Rows[row]["data"].ObjToString().Trim();
                myData = G1.protect_data(myData);
                string str = "";

                ComboBoxEdit combo = (ComboBoxEdit)sender;
                //            string what = combo.Text.Trim().ToUpper();
                string what = combo.Text.Trim();
                what = G1.protect_data(what);
                //dr["data"] = what; // ramma zamma

                funModified = true;
                btnSaveAll.Show();

                DataTable tempDt = null;

            }
            catch ( Exception ex )
            {
            }
        }
        /***************************************************************************************/
        private void CiLookup6_SelectedIndexChangedAgain ( string what )
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            try
            {
                string help = dt.Rows[row]["help"].ObjToString();
                string dbField = dt.Rows[row]["dbField"].ObjToString();
                string myData = dt.Rows[row]["data"].ObjToString().Trim();
                myData = G1.protect_data(myData);
                string str = "";

                //ComboBoxEdit combo = (ComboBoxEdit)sender;
                //            string what = combo.Text.Trim().ToUpper();
                //string what = combo.Text.Trim();
                what = G1.protect_data(what);
                //dr["data"] = what; // ramma zamma

                funModified = true;
                btnSaveAll.Show();

                DataTable tempDt = null;

            }
            catch (Exception ex)
            {
            }
        }
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
        /***************************************************************************************/
        private void CiLookup_DataChanged(string what)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            string help = dt.Rows[row]["help"].ObjToString();
            string dbField = dt.Rows[row]["dbField"].ObjToString();

            what = G1.protect_data(what);

            //ComboBoxEdit combo = (ComboBoxEdit)sender;
            //string what = combo.Text.Trim();
            dr["data"] = what;

            funModified = true;
            btnSaveAll.Show();

            if (help.ToUpper() == "TRACKING")
            {
                DataRow[] dR = null;
                string cmd = "reference LIKE '" + dbField + "~%'";
                DataRow[] dRows = dt.Select(cmd);
                if (dRows.Length > 0)
                {
                    string[] Lines = null;
                    string field = "";
                    string answer = "";
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        Lines = dRows[i]["reference"].ObjToString().Split('~');
                        if (Lines.Length <= 1)
                            continue;
                        field = Lines[1].Trim();
                        dR = trackDt.Select("answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                        if (dR.Length > 0)
                        {
                            answer = ProcessReference(dR, field);
                            //answer = dR[0][field].ObjToString();
                            dRows[i]["data"] = answer;
                            dRows[i]["mod"] = "Y";
                        }
                        else
                        {
                            dRows[i]["data"] = "";
                            dRows[i]["mod"] = "";
                        }
                    }
                }
                dt.AcceptChanges();
            }
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
            btnSaveData.Show();
            btnSaveData.Refresh();

            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            string what = dr["data"].ObjToString();

            what = G1.protect_data(what);

            DataTable dt6 = (DataTable)dgv6.DataSource;
            int rowHandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);
            string field = dt6.Rows[row]["field"].ObjToString();
            string record = dt6.Rows[row]["record"].ObjToString();
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

            if (current.Name.ToUpper() == "TABREPORT")
            {
                picRowDown.Show();
                picRowDown.Refresh();
                picRowUp.Show();
                picRowUp.Refresh();
                pictureBox12.Show();
                pictureBox12.Refresh();
                pictureBox11.Show();
                pictureBox11.Refresh();

                gridMain.ClearSelection();
                gridMain.RefreshData();

                RemoveTabPage("Report Information");
            }
            else if (current.Name.ToUpper() == "TABDATA")
            {
                picRowDown.Hide();
                picRowDown.Refresh();
                picRowUp.Hide();
                picRowUp.Refresh();
                pictureBox12.Hide();
                pictureBox12.Refresh();
                pictureBox11.Hide();
                pictureBox11.Refresh();

                LoadReportData();

                gridMain6.ClearSelection();
                gridMain6.RefreshData();
            }
        }
        /****************************************************************************************/
        private void LoadReportData ()
        {
            string record = getCurrentReportRecord();
            if (String.IsNullOrWhiteSpace(record))
                return;
            string cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);

            string field = "";
            string caption = "";
            repositoryCount = 0;

            ContactsPreneed form = (ContactsPreneed)G1.IsFormOpen("ContactsPreneed");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    if (form != null)
                    {
                        field = dt.Rows[i]["field"].ObjToString();

                        if (G1.get_column_number(workGV, field) < 0)
                            continue;

                        if (!workGV.Columns[field].Visible)
                            continue;
                        caption = workGV.Columns[field].Caption;
                        columnEdit = workGV.Columns[field].ColumnEditName.ObjToString();
                        if (String.IsNullOrWhiteSpace(columnEdit))
                            continue;


                        RepositoryItemComboBox itemBox = form.FireEventGrabNewSomething(field);
                        //RepositoryItemComboBox itemBox = form.FireEventGrabSomething(columnEdit);
                        if (itemBox != null)
                        {
                            repositoryCaptions[repositoryCount] = field;
                            repositoryNames[repositoryCount] = field;
                            Repository[repositoryCount] = itemBox;
                            repositoryCount++;
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);

            dgv6.DataSource = dt;
            dgv6.Refresh();

            ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
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
        private void picRowUp_Click(object sender, EventArgs e)
        {
            if (dgv == null)
                return;
            if (gridMain == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            MoveRowUp(dt, row);

            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            btnSaveAll.Show();
            funModified = true;
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
            if (dgv == null)
                return;
            if (gridMain == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            MoveRowDown(dt, row);

            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
            funModified = true;
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string record = dr["record"].ObjToString();

            tabControl1.TabPages.Add(tabData);

            tabControl1.SelectedTab = tabData;
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
        }
        /***********************************************************************************************/
        private string getCurrentReportRecord ()
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            return record;
        }
        /***********************************************************************************************/
        private int AddNewDataRow()
        {
            int row = -1;
            try
            {
                DataTable dt = (DataTable)dgv6.DataSource;

                string reportRecord = getCurrentReportRecord();

                string record = G1.create_record("contacts_reports_data", "spare", "-1");
                if (G1.BadRecord("contacts_reports_data", record))
                    return row;

                DataRow dRow = dt.NewRow();
                dRow["num"] = (dt.Rows.Count + 1).ToString();
                dRow["record"] = record;
                dRow["reportRecord"] = reportRecord;
                dRow["field"] = "New Field";
                dRow["mod"] = "Y";
                dRow["order"] = dt.Rows.Count;

                dt.Rows.Add(dRow);

                G1.update_db_table("contacts_reports_data", "record", record, new string[] { "order", dt.Rows.Count.ToString(), "spare", "", "reportRecord", reportRecord, "field", "New Field" });

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
            DataTable reportDt = (DataTable)dgv.DataSource;
            string record = getCurrentReportRecord();
            if (String.IsNullOrWhiteSpace(record))
                return;

            string customReport = "";
            DataRow[] dRows = reportDt.Select("record='" + record + "'");
            if (dRows.Length > 0)
                customReport = dRows[0]["report"].ObjToString();

            string cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
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

            //cmd = "Select * from `contacts_preneed` WHERE ";
            //bool found = false;

            ////bool isCustom = false;
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    field = dt.Rows[i]["field"].ObjToString();
            //    if ( field.ToUpper() == "{CUSTOM}")
            //    {
            //        isCustom = true;
            //        continue;
            //    }
            //    data = dt.Rows[i]["data"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(data))
            //        continue;
            //    status = dt.Rows[i]["status"].ObjToString();
            //    if (status.ToUpper() == "OFF")
            //        continue;

            //    operand = dt.Rows[i]["operand"].ObjToString();

            //    body = data.Trim();

            //    date = body.ObjToDateTime();
            //    if (date.Year < 1000)
            //    {
            //        if (!G1.validate_numeric(body))
            //        {
            //            if (found)
            //                cmd += " AND ";
            //            cmd += " `" + field + "` " + operand + " '" + body + "' ";
            //            found = true;
            //            continue;
            //        }
            //        today = DateTime.Now;
            //        if (operand == ">")
            //        {
            //            iBody = body.ObjToInt32();
            //            today = today.AddDays(iBody * -1);
            //            if (field.ToUpper() != "AGE")
            //                operand = "<";
            //        }
            //        else if (operand == ">=")
            //        {
            //            iBody = body.ObjToInt32();
            //            today = today.AddDays(iBody * -1);
            //            if (field.ToUpper() != "AGE")
            //                operand = "<=";
            //        }
            //        else if (operand == "<")
            //        {
            //            iBody = body.ObjToInt32();
            //            today = today.AddDays(iBody * -1);
            //            if (field.ToUpper() != "AGE")
            //                operand = ">";
            //        }
            //        else if (operand == "<=")
            //        {
            //            iBody = body.ObjToInt32();
            //            today = today.AddDays(iBody * -1);
            //            if (field.ToUpper() != "AGE")
            //                operand = ">=";
            //        }
            //        else if (operand == "!=")
            //        {
            //            iBody = body.ObjToInt32();
            //            today = today.AddDays(iBody * -1);
            //            if (field.ToUpper() != "AGE")
            //                operand = ">=";
            //        }
            //        else if (operand == "=")
            //        {
            //            iBody = body.ObjToInt32();
            //        }
            //        else
            //            continue;
            //        if (found)
            //            cmd += " AND ";
            //        if (field.ToUpper() == "AGE")
            //            cmd += " `" + field + "` " + operand + " '" + iBody.ToString() + "' ";
            //        else
            //            cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
            //        found = true;
            //    }
            //    else
            //    {
            //        if (!G1.validate_numeric(body))
            //        {
            //            if (G1.validate_date(body))
            //            {
            //                date = body.ObjToDateTime();
            //                body = date.ToString("yyyy-MM-dd");
            //            }
            //            if (found)
            //                cmd += " AND ";
            //            cmd += " `" + field + "` " + operand + " '" + body + "' ";
            //            found = true;
            //            continue;
            //        }
            //        else
            //            continue;
            //        if (found)
            //            cmd += " AND ";
            //        if (field.ToUpper() == "AGE")
            //            cmd += " `" + field + "` " + operand + " '" + iBody.ToString() + "' ";
            //        else
            //            cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
            //        found = true;
            //    }
            //}

            //if (!found)
            //{
            //    MessageBox.Show("Search Criteria is Empty!", "Search Criteria Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    return;
            //}
            //if (!String.IsNullOrWhiteSpace(workAgent))
            //{
            //    if (workAgent.ToUpper() != "ALL")
            //        cmd += " AND `agent` = '" + workAgent + "' ";
            //}

            //cmd += ";";

            bool isCustom = false;

            cmd = BuildReportQuery(dt, workAgent, ref isCustom);
            dx = G1.get_db_data(cmd);

            if (dx != null)
            {
                this.Cursor = Cursors.WaitCursor;
                int height = this.Height;

                ContactsPreneed form = null;
                if (!isCustom)
                    form = new ContactsPreneed(dx);
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

                //form.Show();
                form.Location = new Point(100, 100);
                form.Height = height + 100;
                form.Refresh();

                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        public static string BuildReportQuery ( DataTable dt, string workAgent, ref bool isCustom )
        {
            string cmd = "Select * from `contacts_preneed` WHERE ";
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
                if ( data.ToUpper().IndexOf ( "TODAY") == 0 )
                {
                    date = DateTime.Now;
                    data = data.ToUpper().Replace("TODAY", "").Trim();
                    Lines = data.Split(' ');
                    if ( Lines.Length >= 2 )
                    {
                        int days = Lines[1].Trim().ObjToInt32();
                        if (Lines[0].Trim() == "-")
                            days = days * -1;
                        date = date.AddDays(days);
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
                if (date.Year < 1000 || gotToday )
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

            if (!found && !isCustom )
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
        //private void runReport(DataTable dt, string report, string agent = "", string email = "", string sendWhere = "", string sendUsername = "", string displayFormat = "")
        //{
        //    string field = "";
        //    string data = "";
        //    string status = "";

        //    DataTable dx = null;
        //    string[] Lines = null;
        //    string operand = "";
        //    string body = "";
        //    int iBody = 0;
        //    DateTime date = DateTime.Now;
        //    DateTime today = DateTime.Now;

        //    DataTable workDt = null;

        //    string cmd = "Select * from `contacts_preneed` WHERE ";
        //    bool found = false;

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        field = dt.Rows[i]["field"].ObjToString();
        //        data = dt.Rows[i]["data"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(data))
        //            continue;
        //        status = dt.Rows[i]["status"].ObjToString();
        //        if (status.ToUpper() == "OFF")
        //            continue;

        //        operand = dt.Rows[i]["operand"].ObjToString();

        //        body = data.Trim();

        //        date = body.ObjToDateTime();
        //        if (date.Year < 1000)
        //        {
        //            if (!G1.validate_numeric(body))
        //            {
        //                if (found)
        //                    cmd += " AND ";
        //                cmd += " `" + field + "` " + operand + " '" + body + "' ";
        //                found = true;
        //                continue;
        //            }
        //            today = DateTime.Now;
        //            if (operand == ">")
        //            {
        //                iBody = body.ObjToInt32();
        //                today = today.AddDays(iBody * -1);
        //                if (field.ToUpper() != "AGE")
        //                    operand = "<";
        //            }
        //            else if (operand == ">=")
        //            {
        //                iBody = body.ObjToInt32();
        //                today = today.AddDays(iBody * -1);
        //                if (field.ToUpper() != "AGE")
        //                    operand = "<=";
        //            }
        //            else if (operand == "<")
        //            {
        //                iBody = body.ObjToInt32();
        //                today = today.AddDays(iBody * -1);
        //                if (field.ToUpper() != "AGE")
        //                    operand = ">";
        //            }
        //            else if (operand == "<=")
        //            {
        //                iBody = body.ObjToInt32();
        //                today = today.AddDays(iBody * -1);
        //                if (field.ToUpper() != "AGE")
        //                    operand = ">=";
        //            }
        //            else if (operand == "!=")
        //            {
        //                iBody = body.ObjToInt32();
        //                today = today.AddDays(iBody * -1);
        //                if (field.ToUpper() != "AGE")
        //                    operand = ">=";
        //            }
        //            else if (operand == "=")
        //            {
        //                iBody = body.ObjToInt32();
        //            }
        //            else
        //                continue;
        //            if (found)
        //                cmd += " AND ";
        //            if (field.ToUpper() == "AGE")
        //                cmd += " `" + field + "` " + operand + " '" + iBody.ToString() + "' ";
        //            else
        //                cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
        //            found = true;
        //        }
        //        else
        //        {
        //            if (!G1.validate_numeric(body))
        //            {
        //                if (G1.validate_date(body))
        //                {
        //                    date = body.ObjToDateTime();
        //                    body = date.ToString("yyyy-MM-dd");
        //                }
        //                if (found)
        //                    cmd += " AND ";
        //                cmd += " `" + field + "` " + operand + " '" + body + "' ";
        //                found = true;
        //                continue;
        //            }
        //            else
        //                continue;
        //            if (found)
        //                cmd += " AND ";
        //            if (field.ToUpper() == "AGE")
        //                cmd += " `" + field + "` " + operand + " '" + iBody.ToString() + "' ";
        //            else
        //                cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
        //            found = true;
        //        }
        //    }

        //    if (!found)
        //    {
        //        if (!autoRun)
        //            MessageBox.Show("Search Criteria is Empty!", "Search Criteria Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        //        return;
        //    }
        //    if (!String.IsNullOrWhiteSpace(workAgent))
        //    {
        //        if (workAgent.ToUpper() != "ALL")
        //            cmd += " AND `agent` = '" + workAgent + "' ";
        //    }

        //    cmd += ";";
        //    dx = G1.get_db_data(cmd);

        //    if (dx != null)
        //    {
        //        this.Cursor = Cursors.WaitCursor;
        //        int height = this.Height;

        //        ContactsPreneed form = new ContactsPreneed(dx, autoRun, agent, email, report, sendWhere, sendUsername, displayFormat);
        //        form.Text = report;
        //        //leadForm.StartPosition = FormStartPosition.CenterParent;
        //        form.Show();
        //        //form.Anchor = AnchorStyles.None;

        //        form.AutoSize = true; //this causes the form to grow only. Don't set it if you want to resize automatically using AnchorStyles, as I did below.
        //        form.FormBorderStyle = FormBorderStyle.Sizable; //I think this is not necessary to solve the problem, but I have left it there just in case :-)
        //        form.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        //                            | System.Windows.Forms.AnchorStyles.Left)
        //                            | System.Windows.Forms.AnchorStyles.Right)));

        //        //form.Show();
        //        form.Location = new Point(100, 100);
        //        form.Height = height + 100;
        //        form.Refresh();

        //        this.Cursor = Cursors.Default;
        //    }
        //}
        /****************************************************************************************/
        private void runReportToolStripMenuItem_ClickX(object sender, EventArgs e)
        {
            string record = getCurrentReportRecord();
            if (String.IsNullOrWhiteSpace(record))
                return;

            string cmd = "Select * from `contacts_reports_data` WHERE `reportRecord` = '" + record + "' ORDER by `order`;";
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

            DataTable workDt = null;

            cmd = "Select * from `contacts_preneed` WHERE ";
            bool found = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                if (String.IsNullOrWhiteSpace(data))
                    continue;
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "OFF")
                    continue;

                operand = dt.Rows[i]["operand"].ObjToString();

                body = data.Trim();

                date = body.ObjToDateTime();
                if (date.Year < 1000)
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
                        today = today.AddDays(iBody * -1);
                        operand = ">";
                    }
                    else
                        continue;
                    if (found)
                        cmd += " AND ";
                    cmd += " `" + field + "` " + operand + " '" + today.ToString("yyyy-MM-dd") + "' ";
                    found = true;
                }
            }

            if (!found)
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

            if (dx != null)
            {
                this.Cursor = Cursors.WaitCursor;
                int height = this.Height;
                ContactsPreneed form = new ContactsPreneed(dx);
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
            TabPage current = tabControl1.SelectedTab;
            if (current.Name.ToUpper() == "TABDATA")
            {
                tabControl1.SelectedTab = tabReport;
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
        private void gridMain6_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            int focusedRow = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(focusedRow);

            DataRow dr = gridMain6.GetFocusedDataRow();
            string field = dr["field"].ObjToString();

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
        private void picDataUp_Click(object sender, EventArgs e)
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
            btnSaveData.Show();
            //funModified = true;
        }
        /****************************************************************************************/
        private void picDataDown_Click(object sender, EventArgs e)
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
            dgv.Refresh();
            //funModified = true;
        }
        /****************************************************************************************/
    }
}