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
    public partial class EditInventoryOther : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        /****************************************************************************************/
        public EditInventoryOther()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditInventoryOther_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            modified = false;
            //SetupTotalsSummary();
            LoadData();
            loadCategories();
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
            string cmd = "Select * from `inventory_cats`;";
            DataTable catDt = G1.get_db_data(cmd);

            DataView tempview = catDt.DefaultView;
            tempview.Sort = "category,name";
            catDt = tempview.ToTable();

            G1.NumberDataTable(catDt);
            dgv.DataSource = catDt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
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
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Data Changed Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void loadCategories()
        {
            repositoryItemComboBox1.Items.Add("Vault");
            repositoryItemComboBox1.Items.Add("Infant Casket");
            repositoryItemComboBox1.Items.Add("Misc Merchandise");
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = "";
            string cat = "";
            string record = "";
            string cmd = "";
            DataTable dt = (DataTable)dgv.DataSource;

            this.Cursor = Cursors.WaitCursor;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    name = dt.Rows[i]["name"].ObjToString();
                    cat = dt.Rows[i]["category"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("inventory_cats", "name", "-1");
                    if (G1.BadRecord("inventory_cats", record))
                        break;
                    G1.update_db_table("inventory_cats", "record", record, new string[] { "category", cat, "name", name });
                }
                catch ( Exception ex )
                {
                }
            }

            btnSave.Hide();
            btnSave.Refresh();
            modified = false;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void picAddVault_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
            btnSave.Show();
            btnSave.Refresh();
            modified = false;
        }
        /****************************************************************************************/
        private void picDeleteVault_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            dr = gridMain.GetFocusedDataRow();

            string record = dr["record"].ObjToString();
            string name = dr["name"].ObjToString();
            string cat = dr["category"].ObjToString();

            string text = "***Question*** Are you sure you want to DELETE (" + name + ") from Category " + cat + "?";
            DialogResult result = MessageBox.Show(text, "Delete Item Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            if (!String.IsNullOrWhiteSpace(record))
                G1.delete_db_table("inventory_cats", "record", record);

            LoadData();
        }
        /****************************************************************************************/
    }
}