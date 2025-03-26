﻿using System;
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
using MySql.Data.Types;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditSurcharges : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        /****************************************************************************************/
        public EditSurcharges()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditSurcharges_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            this.Text = "Edit Batesville Surcharges";
            string cmd = "Select * from `batesville_surcharges` ORDER by `beginDate`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("bDate");
            dt.Columns.Add("eDate");
            DateTime date = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["beginDate"].ObjToDateTime();
                dt.Rows[i]["bDate"] = date.ToString("MM/dd/yyyy");

                date = dt.Rows[i]["endDate"].ObjToDateTime();
                dt.Rows[i]["eDate"] = date.ToString("MM/dd/yyyy");
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            int top = this.Top + 20;
            int left = this.Left + 20;
            this.SetBounds(left, top, this.Width, this.Height);
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
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Row ?", "Delete Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
            string formButton = "";
            string formName = "";

            DateTime beginDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            double surcharge = 0D;

            string cmd = "DELETE from `batesville_surcharges` WHERE `record` >= '0'";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    continue;
                record = G1.create_record("batesville_surcharges", "surcharge", "-1");
                if (G1.BadRecord("batesville_surcharges", record))
                    return;
                beginDate = dt.Rows[i]["bDate"].ObjToDateTime();
                endDate = dt.Rows[i]["eDate"].ObjToDateTime();
                surcharge = dt.Rows[i]["surcharge"].ObjToDouble();

                G1.update_db_table("batesville_surcharges", "record", record, new string[] { "beginDate", beginDate.ToString("MM/dd/yyyy"), "endDate", endDate.ToString("MM/dd/yyyy"), "surcharge", surcharge.ToString() });
            }
            modified = false;
            btnSaveAll.Hide();

            ReProcessInventory(dt);
        }
        /****************************************************************************************/
        private void ReProcessInventory ( DataTable dt )
        {
            string mod = "";
            string record = "";
            DateTime beginDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            double surcharge = 0D;

            string date1 = "";
            string date2 = "";

            string cmd = "";
            DataTable dx = null;

            double gross = 0D;
            double discount = 0D;
            double net = 0D;

            double oldNet = 0D;
            double oldSurcharge = 0D;

            this.Cursor = Cursors.WaitCursor;

            PleaseWait waitForm = new PleaseWait("Please Wait.\nUpdating Inventory!");
            waitForm.Show();
            waitForm.Refresh();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    continue;
                beginDate = dt.Rows[i]["bDate"].ObjToDateTime();
                endDate = dt.Rows[i]["eDate"].ObjToDateTime();
                if (endDate.Year < 1000)
                    endDate = DateTime.Now;
                surcharge = dt.Rows[i]["surcharge"].ObjToDouble();

                date1 = beginDate.ToString("yyyy-MM-dd");
                date2 = endDate.ToString("yyyy-MM-dd");

                cmd = "Select * from `inventory` where `DateReceived` >='" + date1 + "' AND `DateReceived` <= '" + date2 + "';";
                dx = G1.get_db_data(cmd);

                for ( int j=0; j<dx.Rows.Count; j++)
                {
                    record = dx.Rows[j]["record"].ObjToString();

                    gross = dx.Rows[j]["gross"].ObjToDouble();
                    if (gross <= 0D)
                        continue;
                    discount = dx.Rows[j]["discount"].ObjToDouble();

                    oldSurcharge = dx.Rows[j]["surcharge"].ObjToDouble();
                    oldNet = dx.Rows[j]["net"].ObjToDouble();

                    net = gross - discount - surcharge;
                    net = G1.RoundValue(net);

                    if ( net != oldNet || oldSurcharge != surcharge )
                    {
                        G1.update_db_table("inventory", "record", record, new string[] {"net", net.ToString(), "surcharge", surcharge.ToString() });
                    }
                }
            }
            G1.StopWait(ref waitForm );

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
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv.DataSource;

            string field = gridMain.FocusedColumn.FieldName.ToUpper();
            if ( field == "BDATE")
            {
                DateTime date = dr["bDate"].ObjToDateTime();
                dt.Rows[row]["beginDate"] = G1.DTtoMySQLDT(date);
            }
            else if (field == "EDATE")
            {
                DateTime date = dr["eDate"].ObjToDateTime();
                dt.Rows[row]["endDate"] = G1.DTtoMySQLDT(date);
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
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
        }
        /****************************************************************************************/
    }
}