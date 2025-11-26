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
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditMapping : DevExpress.XtraEditors.XtraForm
    {
        private string workMap = "";
        private string workTable = "contacts_preneed_mapping";
        private DataTable workTableDt = null;
        private DataTable importDt = null;
        private string workColumns = "";
        private bool modified = false;
        /****************************************************************************************/
        public EditMapping( string mapName, DataTable dt )
        {
            InitializeComponent();
            workMap = mapName;
            importDt = dt;
        }
        /****************************************************************************************/
        private void EditMapping_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            this.Text = "Edit Map " + workMap;
            string cmd = "Select * from `" + workTable + "` WHERE `map` = '" + workMap + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");

            string map_field = "";

            DataTable workDt = dt.Clone();

            for ( int i=0; i<importDt.Columns.Count; i++)
            {
                map_field = importDt.Columns[i].ColumnName.Trim();
                if (String.IsNullOrWhiteSpace(map_field))
                    continue;
                if (map_field.ToUpper() == "NUM")
                    continue;

                repositoryItemCheckedComboBoxEdit1.Items.Add(map_field);
            }

            workDt = LoadPreneedColumns( workDt );

            workDt = LoadMappedFields(workDt);

            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;

            int top = this.Top + 20;
            int left = this.Left + 20;
            this.SetBounds(left, top, this.Width, this.Height);
        }
        /****************************************************************************************/
        private DataTable LoadMappedFields ( DataTable workDt )
        {
            string data_field = "";
            string map_field = "";
            string cmd = "Select * from `" + workTable + "` WHERE `map` = '" + workMap + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return workDt;

            DataRow[] dRows = null;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                map_field = dt.Rows[i]["map_field"].ObjToString();
                if ( String.IsNullOrWhiteSpace(map_field) )
                    continue;
                data_field = dt.Rows[i]["data_field"].ObjToString();
                dRows = workDt.Select("data_field='" + data_field + "'");
                if (dRows.Length > 0)
                    dRows[0]["map_field"] = map_field;
            }
            return workDt;
        }
        /****************************************************************************************/
        private DataTable LoadPreneedColumns ( DataTable workDt )
        {
            string command = "select column_name,data_type,column_key,character_maximum_length,column_default from information_schema.`COLUMNS` where table_schema = 'smfs'";
            command += " and table_name = 'contacts_preneed';";
            workTableDt = G1.get_db_data(command);
            if (workTableDt.Rows.Count <= 0)
                return workDt;

            DataRow dRow = null;
            string data_field = "";
            for ( int i=0; i<workTableDt.Rows.Count; i++)
            {
                data_field = workTableDt.Rows[i]["column_name"].ObjToString();
                if (data_field.ToUpper() == "RECORD")
                    continue;
                dRow = workDt.NewRow();
                dRow["data_field"] = data_field;
                workDt.Rows.Add(dRow);
            }
            return workDt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
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
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string mod = "";
            string data_field = "";
            string map_field = "";

            string cmd = "DELETE from `" + workTable + "` WHERE `map` = '" + workMap + "';";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod == "D")
                        continue;
                    record = G1.create_record(workTable, "map", workMap);
                    if (G1.BadRecord(workTable, record))
                        return;
                    data_field = dt.Rows[i]["data_field"].ObjToString();
                    map_field = dt.Rows[i]["map_field"].ObjToString();
                    G1.update_db_table(workTable, "record", record, new string[] { "data_field", data_field, "map_field", map_field });
                }
                catch ( Exception ex)
                {
                }
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
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                return;
            e.Cancel = true;
        }
        /****************************************************************************************/
        private void repositoryItemCheckedComboBoxEdit1_EditValueChanged(object sender, EventArgs e)
        {
            string items = string.Empty;
            foreach (CheckedListBoxItem item in this.repositoryItemCheckedComboBoxEdit1.GetItems())
            {
                if (item.CheckState == CheckState.Checked)
                    items += item.Value.ObjToString() + "~";
            }

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string field = "map_field";
            GridColumn column = gridMain.FocusedColumn;
            field = column.FieldName;
            dr[field] = items;
            dt.Rows[row][field] = items;
        }
        /****************************************************************************************/
    }
}