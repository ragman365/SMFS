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
using DevExpress.XtraGrid.Views.BandedGrid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditArrangers : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        /****************************************************************************************/
        public EditArrangers()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditArrangers_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            loadLocations();
            string cmd = "Select * from `arrangers` ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");

            dt = KillDuplicates(dt);

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            //BindingSource bindingSource1 = new BindingSource();
            //bindingSource1.DataSource = dt;
            //dgv.DataSource = bindingSource1;
        }
        /***********************************************************************************************/
        private DataTable KillDuplicates ( DataTable dt)
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

            for ( int i=dt.Rows.Count-1; i>= 0; i--)
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
                        modified = true;
                    }
                    lastLastName = lastName;
                    lastFirstName = firstName;
                    lastLicense = license;
                    lastLocation = location;
                }
            }
            tempview = dt.DefaultView;
            tempview.Sort = "order asc";
            dt = tempview.ToTable();
            return dt;
        }
        /***********************************************************************************************/
        private void loadLocations()
        {
            string location = "";
            this.repositoryItemComboBox1.Items.Clear();
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            for ( int i=0; i<locDt.Rows.Count; i++)
            {
                location = locDt.Rows[i]["locationCode"].ObjToString();
                this.repositoryItemComboBox1.Items.Add(location);
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
            modified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row
            DataRow dr = gridMain.GetFocusedDataRow();
            string lastName = dr["lastName"].ObjToString();
            string firstName = dr["firstName"].ObjToString();
            string middleName = dr["middleName"].ObjToString();
            string data = lastName + ", " + firstName + " " + middleName;
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Arranger (" + data + ") ?", "Delete Arranger Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
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
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string data = "";
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string license = "";
            string location = "";

            this.Cursor = Cursors.WaitCursor;

            dt = G1.GetGridViewTable(gridMain, dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                if (mod == "D")
                {
                    if (record == "-1")
                        continue;
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        G1.delete_db_table("arrangers", "record", record);
                        dt.Rows[i]["record"] = "-1";
                    }
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("arrangers", "middleName", "-1");
                if (G1.BadRecord("arrangers", record))
                    return;
                lastName = dt.Rows[i]["lastName"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                middleName = dt.Rows[i]["middleName"].ObjToString();
                license = dt.Rows[i]["license"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();
                G1.update_db_table("arrangers", "record", record, new string[] { "lastName", lastName, "firstName", firstName, "middleName", middleName, "license", license, "location", location, "order", i.ToString() });
                dt.Rows[i]["record"] = record;
            }
            modified = false;
            btnSaveAll.Hide();

            this.Cursor = Cursors.Default;
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
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
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
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
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
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
    }
}