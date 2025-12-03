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
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditBurialAssociation : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private bool modified = false;
        /****************************************************************************************/
        public EditBurialAssociation()
        {
            InitializeComponent();
            workTable = "ref_burial_association";
            workTable = "burial_association";
        }
        /****************************************************************************************/
        private void EditBurialAssociation_Load(object sender, EventArgs e)
        {
            PleaseWait pleaseForm = G1.StartWait("Please Wait, Loading Edit Tables!");

            btnSaveAll.Hide();
            string cmd = "Select * from `" + workTable + "`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            loadComboCompanies();
            loadFuneralHomes();
            loadStates();

            int top = this.Top + 20;
            int left = this.Left + 20;
            this.SetBounds(left, top, this.Width, this.Height);

            G1.StopWait(ref pleaseForm);
            pleaseForm = null;
        }
        /***********************************************************************************************/
        private void loadComboCompanies()
        {
            string cmd = "Select * from `policies` ";
            cmd += " WHERE `deceasedDate` <= '0001-01-01' AND `lapsed` <> 'Y' ";
            cmd += " AND `report` = 'Not Third Party' ";
            cmd += " AND `lapsed` <> 'Y' ";
            cmd += " AND `lapsedDate8` <= '0100-01-01' ";
            cmd += " AND ( `liability` >= '0.00' AND `liability` <= '450.00' ) ";
            cmd += " GROUP BY `companyCode` ";
            cmd += " ORDER by `companyCode` ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            string companyCode = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                companyCode = dt.Rows[i]["companyCode"].ObjToString();
                repositoryItemComboBox2.Items.Add(companyCode);
            }
        }
        /***********************************************************************************************/
        private void loadFuneralHomes()
        {
            string cmd = "Select * from `funeralhomes`;";

            DataTable dt = G1.get_db_data(cmd);

            string location = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["LocationCode"].ObjToString();
                repositoryItemComboBox1.Items.Add(location);
            }
        }
        /***********************************************************************************************/
        private void loadStates()
        {
            string cmd = "Select * from `ref_states`;";
            DataTable dt = G1.get_db_data(cmd);

            string state = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                state = dt.Rows[i]["abbrev"].ObjToString();
                repositoryItemComboBox4.Items.Add(state);
            }
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Row
            DataTable dt = (DataTable) dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string data = "";
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            if (e.Column.FieldName.Trim().ToUpper() == "LOCATIONCODE")
            {
                data = dr["locationCode"].ObjToString();
                if ( data.Length != 2 )
                {
                    MessageBox.Show("*** ERROR *** Location Code must be two (2) digits!", "Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    dr["locationCode"] = oldWhat;
                }
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "ZIP")
            {
                try
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
                catch (Exception ex)
                {
                }
            }
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row
            DataRow dr = gridMain.GetFocusedDataRow();
            string data = dr["burialAssociation"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Data (" + data + ") ?", "Delete Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "D";
            dt.Rows[row]["mod"] = "D";
            gridMain_CellValueChanged(null, null);
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
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
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            gridMain.PostEditor();

            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string data = "";

            string burialAssociation = "";
            string locationCode = "";
            string funeralHome = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";


            string payable = "";
            string rtnAddress = "";
            string rtnCity = "";
            string rtnState = "";
            string rtnZip = "";

            string payer_prefixes = "";
            string SDI_Key_Code = "";

            string cmd = "DELETE from `" + workTable + "` WHERE `record` >= '0'";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    continue;
                try
                {
                    record = G1.create_record( workTable, "SDI_Key_Code", "-1");
                    if (G1.BadRecord(workTable, record))
                        return;

                    SDI_Key_Code = dt.Rows[i]["SDI_Key_Code"].ObjToString();
                    payable = dt.Rows[i]["payable"].ObjToString();
                    burialAssociation = dt.Rows[i]["burial_association"].ObjToString();
                    address = dt.Rows[i]["address"].ObjToString();
                    city = dt.Rows[i]["city"].ObjToString();
                    state = dt.Rows[i]["state"].ObjToString();
                    zip = dt.Rows[i]["zip"].ObjToString();

                    payer_prefixes = dt.Rows[i]["payer_prefixes"].ObjToString();
                    rtnAddress = dt.Rows[i]["rtnAddress"].ObjToString();
                    rtnCity = dt.Rows[i]["rtnCity"].ObjToString();
                    rtnState = dt.Rows[i]["rtnState"].ObjToString();
                    rtnZip = dt.Rows[i]["rtnZip"].ObjToString();

                    G1.update_db_table(workTable, "record", record, new string[] { "SDI_Key_Code", SDI_Key_Code, "burial_association", burialAssociation, "payable", payable, "payer_prefixes", payer_prefixes, "address", address, "city", city, "state", state, "zip", zip, "rtnAddress", rtnAddress, "rtnCity", rtnCity, "rtnState", rtnState, "rtnZip", rtnZip });
                }
                catch ( Exception ex)
                {
                }
            }
            modified = false;
            btnSaveAll.Hide();
        }
        /****************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /***********************************************************************************************/
        private void massRowsUp(DataTable dt, int row)
        {
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            try
            {
                G1.NumberDataTable(dt);
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["Count"] = i.ToString();
                int moverow = rows[0];
                string mod = "";
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    mod = dt.Rows[row]["mod"].ObjToString();
                    if (mod.ToUpper() == "D")
                        continue;
                    dt.Rows[row]["Count"] = (row - 1).ToString();
                    //dt.Rows[row - 1]["Count"] = row.ToString();
                    //var dRow = gridMain.GetDataRow(row);
                    dt.Rows[row]["mod"] = "M";
                    modified = true;
                }
                dt.Rows[moverow - 1]["Count"] = (moverow + (rows.Length - 1)).ToString();
                G1.sortTable(dt, "Count", "asc");
                dt.Columns.Remove("Count");
                G1.NumberDataTable(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            string mod = "";
            dt.Rows[row]["Count"] = (row + 1).ToString();
            for (int i = row; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i + 1]["mod"].ObjToString().ToUpper();
                if (mod != "D")
                {
                    dt.Rows[i + 1]["Count"] = row.ToString();
                    break;
                }
            }
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***************************************************************************************/
        private void MoveRowDownx(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
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
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSaveAll.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                int row = view.FocusedRowHandle;
                string column = view.FocusedColumn.FieldName.Trim();
                string data = dt.Rows[row][column].ObjToString();

                if (view.FocusedColumn.FieldName.ToUpper() == "LOCATIONCODE")
                {
                    oldWhat = data;
                    return;
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            if (1 == 1)
                return;
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "LOCATIONCODE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string data = dr["locationCode"].ObjToString();
                if (data.Length != 2)
                {
                    MessageBox.Show("*** ERROR *** Location Code must be two (2) digits!", "Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    e.Value = oldWhat;
                }
                else
                    oldWhat = e.Value.ObjToString();
                return;
            }
        }
        /****************************************************************************************/
    }
}