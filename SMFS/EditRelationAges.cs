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
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditRelationAges : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        private DataTable funDt = null;
        /****************************************************************************************/
        public EditRelationAges()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditRelationAges_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            btnSaveAll.Hide();

            string cmd = "Select * from `relation_categories`;";
            DataTable catDt = G1.get_db_data(cmd);

            cmd = "Select * from `ref_relations`;";
            funDt = G1.get_db_data(cmd);

            string category = "";
            string caption = "";

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            G1.AddNewColumn(gridMain, "num", "Num", "", FormatType.None, 100, true);
            dt.Columns.Add("ageRange");
            G1.AddNewColumn(gridMain, "ageRange", "Age Range", "", FormatType.None, 100, true);

            for ( int i=0; i<catDt.Rows.Count; i++)
            {
                category = catDt.Rows[i]["relation_category"].ObjToString();
                caption = catDt.Rows[i]["relationships"].ObjToString();
                caption = "(" + category + ")\n" + caption;

                G1.AddNewColumn(gridMain, category, caption, "", FormatType.None, 175, true);

                dt.Columns.Add(category);
            }

            int col = 1;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                category = dt.Columns[i].ColumnName.ObjToString();
                G1.SetColumnPosition(gridMain, category, ++col);
                if (category.ToUpper() == "NUM" || category.ToUpper() == "AGERANGE")
                    continue;
                gridMain.Columns[category].ColumnEdit = repositoryItemCheckEdit1;
            }

            SetupSelection(dt);

            LoadData(dt);

            dt.Columns.Add("mod");

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            this.Cursor = Cursors.Arrow;

            //LoadRelationships();

            //int top = this.Top + 20;
            //int left = this.Left + 20;
            //this.SetBounds(left, top, this.Width, this.Height);
        }
        /***********************************************************************************************/
        private void LoadData ( DataTable dt )
        {
            string ageRange = "";
            string category = "";
            string onOff = "";
            int col = 0;
            int row = -1;

            int ageRangeCol = G1.get_column_number(dt, "ageRange");
            if (ageRangeCol < 0)
                return;

            string cmd = "Select * from `relation_age_ranges`;";
            DataTable ageDt = G1.get_db_data(cmd);

            for (int i = 0; i < ageDt.Rows.Count; i++)
            {
                ageRange = ageDt.Rows[i]["ageRange"].ObjToString();
                if (String.IsNullOrWhiteSpace(ageRange))
                    continue;
                category = ageDt.Rows[i]["relation_category"].ObjToString();
                if (String.IsNullOrWhiteSpace(category))
                    continue;
                col = G1.get_column_number(dt, category);
                if (col < 0)
                    continue;
                dt = FindOrAddRow(dt, ageRange, ref row);
                if (row < 0)
                    continue;
                dt.Rows[row][category] = "1";
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = (ageRangeCol + 1); j < dt.Columns.Count; j++)
                {
                    onOff = dt.Rows[i][j].ObjToString();
                    if (onOff != "1")
                        dt.Rows[i][j] = "0";
                }
            }
        }
        /***********************************************************************************************/
        private DataTable FindOrAddRow ( DataTable dt, string ageRange, ref int row )
        {
            row = -1;
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["ageRange"].ObjToString();
                if ( str == ageRange )
                {
                    row = i;
                    break;
                }
            }
            if ( row < 0 )
            {
                DataRow dR = dt.NewRow();
                dR["ageRange"] = ageRange;
                dt.Rows.Add(dR);

                row = dt.Rows.Count - 1;
            }
            return dt;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //    dt.Rows[i]["select"] = "0";
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Row
            DataTable dt = (DataTable) dgv.DataSource;
            DataRow dRow = dt.NewRow();
            string category = "";

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                category = dt.Columns[i].ColumnName.ObjToString();
                if (category.ToUpper() == "NUM" || category.ToUpper() == "AGERANGE")
                    continue;
                dRow[category] = "0";
            }

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
            string data = dr["data"].ObjToString();
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
            string mod = "";
            string record = "";
            string category = "";
            string ageRange = "";

            this.Cursor = Cursors.WaitCursor;

            string cmd = "DELETE from `relation_age_ranges` WHERE `record` >= '0'";
            G1.get_db_data(cmd);

            int ageRangeCol = G1.get_column_number(dt, "ageRange");
            string onOff = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    continue;

                ageRange = dt.Rows[i]["ageRange"].ObjToString();
                if (String.IsNullOrWhiteSpace(ageRange))
                    continue;

                for (int j = (ageRangeCol + 1); j < dt.Columns.Count; j++)
                {
                    category = dt.Columns[j].ColumnName.Trim();
                    if (String.IsNullOrWhiteSpace(category))
                        continue;
                    onOff = dt.Rows[i][j].ObjToString().Trim();
                    if (String.IsNullOrWhiteSpace(onOff))
                        continue;
                    if (onOff == "0")
                        continue;

                    record = G1.create_record("relation_age_ranges", "ageRange", "-1");
                    if (G1.BadRecord(workTable, record))
                        return;
                    G1.update_db_table("relation_age_ranges", "record", record, new string[] { "ageRange", ageRange, "relation_category", category });
                }
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
    }
}