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
using MySql.Data.Types;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SelectPermissions : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private string userGroups = "";
        private int editRow = -1;
        /****************************************************************************************/
        public SelectPermissions( string groups, int row )
        {
            InitializeComponent();
            userGroups = groups;
            editRow = row;
        }
        /****************************************************************************************/
        private void SelectPermissions_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("userGroup");

            if ( SelectDone != null )
                btnSaveAll.Text = "Save Permissions";

            AddRow(dt, "Admin");
            AddRow(dt, "SuperUser");
            AddRow(dt, "HomeOffice");
            AddRow(dt, "Field");
            AddRow(dt, "HR");

            SetupSelection(dt);

            string group = "";
            DataRow[] dRows = null;
            string[] Lines = userGroups.Split('~');
            for ( int i=0; i<Lines.Length; i++)
            {
                group = Lines[i].Trim();
                if ( !String.IsNullOrWhiteSpace ( group ))
                {
                    dRows = dt.Select("userGroup='" + group + "'");
                    if ( dRows.Length > 0 )
                        dRows[0]["select"] = "1";
                }
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

        }
        /***********************************************************************************************/
        private DataTable AddRow ( DataTable dt, string group )
        {
            DataRow dRow = dt.NewRow();
            dRow["userGroup"] = group;
            dt.Rows.Add(dRow);
            return dt;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            dr["mod"] = "Y";

            modified = true;
            btnSaveAll.Show();
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
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( SelectDone != null )
            {
                OnSelectDone ( dt);
                modified = false;
                btnSaveAll.Hide();
                this.Close();
            }
            modified = false;
            btnSaveAll.Hide();
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
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your changes?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Yes)
            {
                btnSaveAll_Click(null, null);
                return;
            }
            //e.Cancel = true;
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

            //string field = gridMain.FocusedColumn.FieldName.ToUpper();
            //if ( field == "BDATE")
            //{
            //    DateTime date = dr["bdate"].ObjToDateTime();
            //    dt.Rows[row]["date"] = G1.DTtoMySQLDT(date);
            //}
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
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string select = dr["select"].ObjToString();
            select = dt.Rows[row]["select"].ObjToString();

            string doit = "0";
            if (select == "0")
                doit = "1";
            dr["select"] = doit;
            dt.Rows[row]["select"] = doit;
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();

            modified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt, int row );
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            SelectDone?.Invoke(dt, editRow );
        }
        /****************************************************************************************/
    }
}