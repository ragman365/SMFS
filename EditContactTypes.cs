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
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Controls;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditContactTypes : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        /****************************************************************************************/
        public EditContactTypes()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditContactTypes_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            string cmd = "Select * from `contactTypes` GROUP BY `contactType` ORDER BY `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Row
            DataTable dt = (DataTable) dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["detail"] = "Place";
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
            modified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row

            DataRow dr = gridMain.GetFocusedDataRow();
            string data = dr["contactType"].ObjToString();
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
            string record = "";
            string mod = "";
            string contactType= "";
            string detail = "";
            string category = "";
            int frequency = 0;

            string cmd = "";

            //cmd = "DELETE from `contactTypes` WHERE `record` >= '0'";
            //G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    mod = dt.Rows[i]["mod"].ObjToString();

                    if (mod == "D")
                    {
                        if (record == "-1")
                            continue;
                        if (!String.IsNullOrWhiteSpace(record))
                        {
                            G1.delete_db_table("contactTypes", "record", record);
                            dt.Rows[i]["record"] = "-1";
                        }
                        continue;
                    }

                    if (String.IsNullOrWhiteSpace(mod))
                        continue;

                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("contactTypes", "contactType", "-1");
                    if (G1.BadRecord("contactTypes", record))
                        continue;
                    contactType = dt.Rows[i]["contactType"].ObjToString();
                    detail = dt.Rows[i]["detail"].ObjToString();
                    frequency = dt.Rows[i]["frequency"].ObjToInt32();
                    category = dt.Rows[i]["category"].ObjToString();
                    G1.update_db_table("contactTypes", "record", record, new string[] { "contactType", contactType, "detail", detail, "category", category, "frequency", frequency.ToString(), "order", i.ToString() });

                    dt.Rows[i]["record"] = record;
                }
                catch (Exception ex)
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contactType = dr["contactType"].ObjToString();
            if (String.IsNullOrWhiteSpace(contactType))
                return;
            string detail = dr["detail"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            EditTracking trackForm = new EditTracking(true, contactType, detail );
            trackForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRow dr = null;
            int rowHandle = 0;
            int row = 0;
            string record = "";
            string contactType = "";
            string category = "";
            string contactDetail = "";
            string scheduledTask = "";
            string interval = "";
            string cmd = "";
            DataTable dx = null;

            TabControl tabControl = (TabControl)sender;
            int selectedIndex = tabControl.SelectedIndex;
            DataTable dt = (DataTable)dgv.DataSource;
            string pageName = tabControl.TabPages[selectedIndex].Name.Trim();
            if ( pageName == "tabPageDetails")
            {
                btnSaveDetail.Hide();
                modifiedDetail = false;
                rowHandle = gridMain.FocusedRowHandle;
                row = gridMain.GetFocusedDataSourceRowIndex();
                record = dt.Rows[row]["record"].ObjToString();
                contactType = dt.Rows[row]["contactType"].ObjToString();
                category = dt.Rows[row]["category"].ObjToString();
                scheduledTask = dt.Rows[row]["scheduledTask"].ObjToString();
                interval = dt.Rows[row]["interval"].ObjToString();

                cmd = "Select * from `contacttypes` WHERE `contactType`='" + contactType + "' AND `category` = '" + category + "';";
                dx = G1.get_db_data(cmd);
                dx.Columns.Add("mod");
                G1.NumberDataTable(dx);
                dgv2.DataSource = dx;
            }
            else
            {
            }
        }
        /****************************************************************************************/
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            int selectedIndex = tabControl.SelectedIndex;
            DataTable dt = (DataTable)dgv.DataSource;
            string pageName = tabControl.TabPages[selectedIndex].Name.Trim();
            if (pageName == "tabPageContactTypes")
            {
                if (modifiedDetail)
                {
                    DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                        return;
                    e.Cancel = true;
                }
            }
        }
        /****************************************************************************************/
        bool modifiedDetail = false;
        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            dr["mod"] = "Y";
            modifiedDetail = true;
            btnSaveDetail.Show();
            btnSaveDetail.Refresh();
        }
        /****************************************************************************************/
        private void picAddDetail_Click(object sender, EventArgs e)
        { // Add Contact Detail Row
            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = dt.Rows[0];
            DataRow dRow = dt.NewRow();
            dRow["contactType"] = dr["contactType"].ObjToString();
            dRow["category"] = dr["category"].ObjToString();
            dRow["detail"] = dr["detail"].ObjToString();
            dRow["mod"] = "Y";
            modifiedDetail = true;
            btnSaveDetail.Show();
            btnSaveDetail.Refresh();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            gridMain2.RefreshData();
        }
        /****************************************************************************************/
        private void btnSaveDetail_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            string record = "";
            string mod = "";
            string contactType = "";
            string detail = "";
            string category = "";
            int frequency = 0;
            string scheduledTask = "";
            string interval = "";
            string from = "";

            string cmd = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    mod = dt.Rows[i]["mod"].ObjToString();

                    if (mod == "D")
                    {
                        if (record == "-1")
                            continue;
                        if (!String.IsNullOrWhiteSpace(record))
                        {
                            G1.delete_db_table("contactTypes", "record", record);
                            dt.Rows[i]["record"] = "-1";
                        }
                        continue;
                    }

                    if (String.IsNullOrWhiteSpace(mod))
                        continue;

                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("contactTypes", "contactType", "-1");
                    if (G1.BadRecord("contactTypes", record))
                        continue;
                    contactType = dt.Rows[i]["contactType"].ObjToString();
                    detail = dt.Rows[i]["detail"].ObjToString();
                    frequency = dt.Rows[i]["frequency"].ObjToInt32();
                    category = dt.Rows[i]["category"].ObjToString();
                    scheduledTask = dt.Rows[i]["scheduledTask"].ObjToString();
                    interval = dt.Rows[i]["interval"].ObjToString();
                    from = dt.Rows[i]["from"].ObjToString();
                    G1.update_db_table("contactTypes", "record", record, new string[] { "contactType", contactType, "detail", detail, "category", category, "frequency", frequency.ToString(), "scheduledTask", scheduledTask, "interval", interval, "from", from });

                    dt.Rows[i]["record"] = record;
                }
                catch (Exception ex)
                {
                }
            }
            modifiedDetail = false;
            btnSaveDetail.Hide();
        }
        /****************************************************************************************/
    }
}