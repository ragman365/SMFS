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
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditFunOptions : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        /****************************************************************************************/
        public EditFunOptions()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditFunOptions_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            modified = false;
            //SetupTotalsSummary();
            LoadData();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("detail", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void LoadData ()
        {
            string manager = "";
            string location = "";
            DataTable manDt = new DataTable();
            manDt.Columns.Add("num");
            manDt.Columns.Add("location");
            manDt.Columns.Add("ma");
            manDt.Columns.Add("name");

            DataRow[] dRows = null;
            DataRow dR = null;

            string cmd = "Select * from `funeralhomes`;";
            DataTable funDt = G1.get_db_data(cmd);
            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                manager = funDt.Rows[i]["manager"].ObjToString();
                if (String.IsNullOrWhiteSpace(manager))
                    continue;

                location = funDt.Rows[i]["LocationCode"].ObjToString();

                //dRows = manDt.Select("name='" + manager + "'");
                //if (dRows.Length <= 0)
                //{
                dR = manDt.NewRow();
                dR["ma"] = "M";
                dR["name"] = manager;
                dR["location"] = location;
                manDt.Rows.Add(dR);
                //}
            }

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string name = "";

            cmd = "Select * from `arrangers`;";
            funDt = G1.get_db_data(cmd);
            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                firstName = funDt.Rows[i]["firstName"].ObjToString().Trim();
                lastName = funDt.Rows[i]["lastName"].ObjToString().Trim();
                middleName = funDt.Rows[i]["middleName"].ObjToString().Trim();
                name = firstName + " " + lastName;
                if (String.IsNullOrWhiteSpace(name))
                    continue;

                dRows = manDt.Select("name='" + name + "'");
                if ( dRows.Length > 0 )
                {
                    dRows[0]["ma"] = "MA";
                    continue;
                }

                dRows = manDt.Select("ma='a' AND name='" + name + "'");
                if (dRows.Length <= 0)
                {
                    dR = manDt.NewRow();
                    dR["ma"] = "A";
                    dR["name"] = name;
                    manDt.Rows.Add(dR);
                }
            }
            DataView tempview = manDt.DefaultView;
            tempview.Sort = "ma,name";
            manDt = tempview.ToTable();


            G1.NumberDataTable(manDt);

            dgv.DataSource = manDt;

            loadupCommissionOptions(manDt);

            manDt.Columns.Add("mod");

            G1.SetColumnWidth(gridMain, "num", 65);
            G1.SetColumnWidth(gridMain, "ma", 65);
            G1.SetColumnWidth(gridMain, "name", 150);
            G1.SetColumnWidth(gridMain, "location", 150);
        }
        /****************************************************************************************/
        DataTable optionDt = null;
        private void loadupCommissionOptions ( DataTable dt )
        {
            string option = "";
            string defaults = "";
            string who = "";
            string cmd = "Select * from `funcommoptions` ORDER by `order`;";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                option = dx.Rows[i]["option"].ObjToString();
                who = dx.Rows[i]["who"].ObjToString();
                G1.AddNewColumn(gridMain, option, option, "", FormatType.None, 80, true);
                G1.SetColumnWidth(gridMain, option, 80);

                dt.Columns.Add(option);

                defaults = dx.Rows[i]["defaults"].ObjToString();
                for (int j = 0; j < dt.Rows.Count; j++)
                    dt.Rows[j][option] = defaults;
            }

            G1.SetColumnPositions(dt, gridMain);

            LoadCommissionData(dt);

            optionDt = dx.Copy();
        }
        /****************************************************************************************/
        private void LoadCommissionData ( DataTable dt )
        {
            string mod = "";
            string name = "";
            string location = "";
            string who = "";
            string option = "";
            string answer = "";
            DataRow[] dRows = null;
            string cmd = "Select * from `funcommissiondata`;";
            DataTable dx = G1.get_db_data(cmd);

            int startColumn = G1.get_column_number(dt, "name");
            startColumn = startColumn + 1;

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    name = dt.Rows[i]["name"].ObjToString();
                    location = dt.Rows[i]["location"].ObjToString();
                    who = dt.Rows[i]["ma"].ObjToString();
                    if (who == "M" || who == "A")
                        dRows = dx.Select("name='" + name + "' AND ma='" + who + "' AND `location` = '" + location + "'");
                    else
                        dRows = dx.Select("name='" + name + "' AND (ma='M' OR ma='A') AND `location` = '" + location + "'");
                    if (dRows.Length > 0)
                    {
                        for (int j = 0; j < dRows.Length; j++)
                        {
                            answer = dRows[j]["answer"].ObjToString();
                            option = dRows[j]["option"].ObjToString();
                            if (G1.get_column_number(dt, option) >= 0)
                            {
                                dt.Rows[i][option] = answer;
                            }
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dt.Rows[row]["mod"] = "Y";
            btnSave.Show();
            btnSave.Refresh();
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            //string delete = dt.Rows[row]["mod"].ObjToString();
            //if (delete.ToUpper() == "D")
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
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
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Options Changed Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string manager = dr["name"].ObjToString();
            //if (String.IsNullOrWhiteSpace(manager))
            //    return;
            //string ma = dr["ma"].ObjToString();
            //this.Cursor = Cursors.WaitCursor;
            //FunManager funForm = new FunManager(null, manager, ma);
            //funForm.Show();
            //this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string mod = "";
            string name = "";
            string location = "";
            string who = "";
            string option = "";
            string answer = "";
            string record = "";
            DataTable dx = null;
            DataRow[] dRows = null;
            string cmd = "";
            DataTable dt = (DataTable)dgv.DataSource;

            int startColumn = G1.get_column_number(dt, "name");
            startColumn = startColumn + 1;

            this.Cursor = Cursors.WaitCursor;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod != "Y")
                    continue;
                name = dt.Rows[i]["name"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();
                who = dt.Rows[i]["ma"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();
                cmd = "Select * from `funcommissiondata` where `name` = '" + name + "' AND (`ma` = 'M' OR `ma` = 'A');";
                if ( !String.IsNullOrWhiteSpace ( location ))
                    cmd = "Select * from `funcommissiondata` where `name` = '" + name + "' AND `location` = '" + location + "' AND (`ma` = 'M' OR `ma` = 'A');";
                dx = G1.get_db_data(cmd);

                for ( int j=startColumn; j<dt.Columns.Count; j++)
                {
                    option = dt.Columns[j].ColumnName.ObjToString().Trim();
                    if (option.ToUpper() == "MOD") // Don't Save This as an option
                        continue;
                    dRows = optionDt.Select("option='" + option + "'");
                    if (dRows.Length > 0)
                        who = dRows[0]["who"].ObjToString();
                    answer = dt.Rows[i][j].ObjToString();
                    dRows = dx.Select("option='" + option + "'");
                    try
                    {
                        if (dRows.Length > 0)
                        {
                            record = dRows[0]["record"].ObjToString();
                            G1.update_db_table("funcommissiondata", "record", record, new string[] { "name", name, "ma", who, "location", location, "option", option, "answer", answer });
                        }
                        else
                        {
                            record = G1.create_record("funcommissiondata", "option", "-1");
                            if (G1.BadRecord("funcommissiondata", record))
                                continue;
                            G1.update_db_table("funcommissiondata", "record", record, new string[] { "name", name, "ma", who, "location", location, "option", option, "answer", answer });
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                }
            }

            btnSave.Hide();
            btnSave.Refresh();
            modified = false;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
    }
}