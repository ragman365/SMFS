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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditContractServices : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workEmpNo = "";
        private string workEmpUserName = "";
        private string workWhat = "";

        private bool workSelecting = false;
        /****************************************************************************************/
        private DataTable _answer = null;
        /****************************************************************************************/
        public DataTable ServiceAnswer { get { return _answer; } }
        /****************************************************************************************/
        public EditContractServices( string what, string empNo )
        {
            InitializeComponent();
            workEmpNo = empNo;
            workWhat = what;
        }
        /****************************************************************************************/
        public EditContractServices ( string what, bool selecting, string empUserName )
        {
            InitializeComponent();
            workEmpUserName = empUserName;
            workSelecting = selecting;
            workWhat = what;

            _answer = null;
        }
        /****************************************************************************************/
        private void EditContractServices_Load(object sender, EventArgs e)
        {
            this.Text = "Edit PartTime Services";
            if (workWhat.ToUpper() == "OTHER")
                this.Text = "Edit Other Services";

            if (!String.IsNullOrWhiteSpace(workEmpUserName))
            {
                this.Text = "Edit PartTime Services for " + workEmpUserName;
                if (workWhat.ToUpper() == "OTHER")
                    this.Text = "Edit Other Services for " + workEmpUserName;
            }

            this.TopMost = true;

            btnSaveAll.Hide();
            string service = "";
            DataRow[] dRows = null;

            string cmd = "Select * from `tc_contract_labor_services` ORDER BY `order`;";
            if ( workWhat.ToUpper() == "OTHER")
                cmd = "Select * from `tc_other_labor_services` ORDER BY `order`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("rate", Type.GetType("System.Decimal"));


            if (!String.IsNullOrWhiteSpace(workEmpUserName))
            {
                cmd = "Select * from `tc_contract_labor_setup` WHERE `employee` = '" + workEmpUserName + "';";
                if ( workWhat.ToUpper() == "OTHER")
                    cmd = "Select * from `tc_other_labor_setup` WHERE `employee` = '" + workEmpUserName + "';";
                DataTable dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    service = dx.Rows[i]["laborService"].ObjToString();
                    dRows = dt.Select("laborService='" + service + "'");
                    if (dRows.Length > 0)
                        dRows[0]["rate"] = dx.Rows[i]["rate"].ObjToDecimal();
                }
            }
            else
            {
                gridMain.Columns["rate"].Visible = false;
                gridMain.Columns["baserate"].OptionsColumn.AllowEdit = true;
                gridMain.Columns["baserate"].OptionsColumn.ReadOnly = false;
            }

            if ( !G1.isHR())
            {
                gridMain.Columns["rate"].Visible = false;
                gridMain.Columns["baserate"].Visible = false;
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            loading = false;

            if ( workSelecting )
            {
                btnInsert.Hide();
                pictureBox11.Hide();
                pictureBox12.Hide();
                picRowDown.Hide();
                picRowUp.Hide();
                btnSaveAll.Text = "Exit Select";
                btnSaveAll.Hide();
                SetupSelection(dt, this.repositoryItemCheckEdit1, "select");
            }
            else
            {
                gridMain.Columns["select"].Visible = false;
            }

            //int top = this.Top + 20;
            //int left = this.Left + 20;
            //this.SetBounds(left, top, this.Width, this.Height);
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
            if (loading)
                return;
            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row
            DataRow dr = gridMain.GetFocusedDataRow();
            string data = dr["laborService"].ObjToString();
            DialogResult result;
            if ( workWhat.ToUpper() == "OTHER")
                result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Other Service (" + data + ") ?", "Delete Other Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            else
                result = MessageBox.Show("***Question*** Are you sure you want to DELETE this PartTime Service (" + data + ") ?", "Delete PartTime Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
            G1.SpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = null, string columnName = "")
        {
            if (selectnew == null)
                selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = null;
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "N";
            selectnew.ValueGrayed = null;
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][columnName].ObjToString().ToUpper() != "Y")
                    dt.Rows[i][columnName] = "N";
            }
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            if ( workSelecting )
            {
                btnSaveAll.Hide();
                btnSaveAll.Refresh();
                DataTable dx = (DataTable)dgv.DataSource;
                _answer = dx;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
                //DialogResult result = MessageBox.Show("***Question*** Do you want to exit with these selections?", "Selections Made Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                //if (result == DialogResult.Yes)
                //{
                //    btnSaveAll.Hide();
                //    btnSaveAll.Refresh();
                //    DataTable dx = (DataTable)dgv.DataSource;
                //    _answer = dx;
                //    this.DialogResult = DialogResult.OK;
                //    this.Close();
                //    return;
                //}
                //if (result == DialogResult.No)
                //{
                //    btnSaveAll.Hide();
                //    btnSaveAll.Refresh();
                //    this.DialogResult = DialogResult.OK;
                //    this.Close();
                //    return;
                //}
                //return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string service = "";
            decimal baserate = 0;


            string cmd = "DELETE from `tc_contract_labor_services` WHERE `laborService` = '-1'";
            if ( workWhat.ToUpper() == "OTHER" )
                cmd = "DELETE from `tc_other_labor_services` WHERE `laborService` = '-1'";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        if ( workWhat.ToUpper() == "OTHER")
                            G1.delete_db_table("tc_other_labor_services", "record", record);
                        else
                            G1.delete_db_table("tc_contract_labor_services", "record", record);
                    }
                    continue;
                }
                if (mod != "Y")
                    continue;
                if (workWhat.ToUpper() == "OTHER")
                {
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("tc_other_labor_services", "laborService", "-1");
                    if (G1.BadRecord("tc_other_labor_services", record))
                        return;

                    service = dt.Rows[i]["laborService"].ObjToString();
                    baserate = dt.Rows[i]["baserate"].ObjToDecimal();
                    G1.update_db_table("tc_other_labor_services", "record", record, new string[] { "laborService", service, "baserate", baserate.ToString(), "user", LoginForm.username, "order", i.ToString() });
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("tc_contract_labor_services", "laborService", "-1");
                    if (G1.BadRecord("tc_contract_labor_services", record))
                        return;

                    service = dt.Rows[i]["laborService"].ObjToString();
                    baserate = dt.Rows[i]["baserate"].ObjToDecimal();
                    G1.update_db_table("tc_contract_labor_services", "record", record, new string[] { "laborService", service, "baserate", baserate.ToString(), "user", LoginForm.username, "order", i.ToString() });
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

                RecheckOrder(dt);

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
        private void RecheckOrder ( DataTable dt )
        {
            int order = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                order = dt.Rows[i]["order"].ObjToInt32();
                if (order != i)
                    dt.Rows[i]["mod"] = "Y";
            }
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

            RecheckOrder(dt);
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
            if (workSelecting)
            {
                if ( btnSaveAll.Visible )
                    btnSaveAll_Click(null, null);
                return;
            }
            if (!btnSaveAll.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "YEAR" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string year = e.DisplayText;
                year = year.Replace(",", "");
                e.DisplayText = year;
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            if (!workSelecting)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = dt.Clone();
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["select"] = "Y";
            dx.ImportRow(dr);
            _answer = dx;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
    }
}