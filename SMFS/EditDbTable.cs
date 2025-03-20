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
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.BandedGrid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditDbTable : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private string workTable = "";
        private string workFields = "";
        private string workWidths = "";
        /****************************************************************************************/
        public EditDbTable( string table, string fields, string widths )
        {
            InitializeComponent();
            workTable = table;
            workFields = fields;
            workWidths = widths;
        }
        /****************************************************************************************/
        private void EditDbTable_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            this.Text = "Edit " + workTable;

            string cmd = "Select * from `" + workTable + "` ORDER BY `order`;";
            DataTable workDt = G1.get_db_data(cmd);

            string[] Lines = workFields.Split(',');
            string[] lineWidths = workWidths.Split(',');

            string field = "";
            string toType = "";
            DataRow dR = null;

            workDt.Columns.Add("num");
            workDt.Columns.Add("mod");

            //AddNewColumn(gridMain, "num", "Num", "", FormatType.None, 60, true);

            int width = 0;
            int totalWidth = gridMain.Columns["num"].Width;
            int j = 1;
            totalWidth = 0;
            G1.ClearAllPositions(gridMain);
            G1.SetColumnPosition(gridMain, "num", j++);

            for (int i = 0; i < Lines.Length; i++)
            {
                try
                {
                    width = lineWidths[i].ObjToInt32();
                    totalWidth += width;

                    field = Lines[i].Trim();
                    field = field.Replace("(", "");
                    field = field.Replace(")", "");
                    if (G1.get_column_number(workDt, field) < 0)
                        continue;
                    toType = workDt.Columns[field].DataType.ToString().ToUpper();

                    if (toType.IndexOf("MYSQLDATETIME") >= 0)
                    {
                        //dt.Columns.Add(field, Type.GetType("System.DateTime"));
                    }
                    else if (toType.IndexOf("DOUBLE") >= 0)
                    {
                        G1.AddNewColumn(gridMain, field, field, "N2", FormatType.Numeric, width, true);

                    }
                    else if (toType.IndexOf("DECIMAL") >= 0)
                    {
                        G1.AddNewColumn(gridMain, field, field, "N2", FormatType.Numeric, width, true);
                    }
                    else if (toType.IndexOf("INT32") >= 0)
                    {
                        G1.AddNewColumn(gridMain, field, field, "N0", FormatType.Numeric, width, true);
                    }
                    else if (toType.IndexOf("INT64") >= 0)
                    {
                        G1.AddNewColumn(gridMain, field, field, "N0", FormatType.Numeric, width, true);
                    }
                    else if (toType.ToUpper() == "SYSTEM.BYTE[]")
                        continue;
                    else
                    {
                        G1.AddNewColumn(gridMain, field, field, "", FormatType.None, width, true);
                    }

                    G1.SetColumnPosition(gridMain, field, j++, width);
                    gridMain.Columns[field].OptionsColumn.FixedWidth = true;
                }
                catch (Exception ex)
                {
                }
            }

            ////gridBand4.Columns["casketDescription"].Width = 80;

            //gridMain.OptionsView.ColumnAutoWidth = false;

            //gridMain.Bands[0].Width = totalWidth;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            gridMain.PostEditor();
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
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
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
            modified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Delete Current Row

            DataRow dr = gridMain.GetFocusedDataRow();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this row?", "Delete Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "D";
            dt.Rows[row]["Mod"] = "D";
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

            string cmd = "";

            cmd = "DELETE from `" + workTable + "` WHERE `record` = '-1';";
            G1.get_db_data(cmd);

            string data = "";
            string[] Lines = workFields.Split(',');
            string field = "";
            string str = "";

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
                            G1.delete_db_table(workTable, "record", record);
                            dt.Rows[i]["record"] = "-1";
                        }
                        continue;
                    }

                    if (String.IsNullOrWhiteSpace(mod))
                        continue;

                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record(workTable, "order", "-1" );
                    if (G1.BadRecord("contactTypes", record))
                        continue;
                    data = "";
                    for ( int j=0; j<Lines.Length; j++)
                    {
                        field = Lines[j].Trim();
                        str = dt.Rows[i][field].ObjToString();
                        str = str.Replace(",", "");
                        if ( str.IndexOf ( "," ) > 0 )
                        {
                            G1.update_db_table(workTable, "record", record, new string[] { field, str });
                            continue;
                        }
                        if (!String.IsNullOrWhiteSpace(data))
                            data += ",";
                        data += field + "," + str;
                    }

                    data += ",order," + i.ToString();
                    G1.update_db_table(workTable, "record", record, data );

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