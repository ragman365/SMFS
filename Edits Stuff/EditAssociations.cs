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
using DevExpress.XtraEditors.Repository;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditAssociations : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        public static string trackingSelection = "";
        private DataTable originalDt = null;
        private string _answer = "";
        private bool loading = true;
        public string A_Answer { get { return _answer; } }
        /****************************************************************************************/
        public EditAssociations()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditAssociations_Load(object sender, EventArgs e)
        {
            this.btnSave.Hide();
            _answer = "";
            this.Text = "Edit Associations";
            string cmd = "Select * from `associations`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            originalDt = dt;
            dgv.DataSource = dt;
            loadLocatons();
            loading = false;
        }
        /***********************************************************************************************/
        private DataTable funDt = null;
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            string locationCode = "";
            for ( int i=0; i<locDt.Rows.Count; i++)
            {
                locationCode = locDt.Rows[i]["locationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                    locDt.Rows[i]["locationCode"] = locDt.Rows[i]["name"].ObjToString();
            }

            funDt = locDt.Copy();

            DataView tempview = funDt.DefaultView;
            tempview.Sort = "locationCode asc";
            funDt = tempview.ToTable();

            this.repositoryItemCheckedComboBoxEdit1.Items.Clear();

            int count = 0;
            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                locationCode = funDt.Rows[i]["locationCode"].ObjToString();
                this.repositoryItemCheckedComboBoxEdit1.Items.Add(locationCode);
                count++;
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string record = "";
            string association = "";
            string locations = "";
            string mod = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                if (mod == "D" && String.IsNullOrWhiteSpace(record))
                    continue;
                if ( mod == "D")
                {
                    G1.delete_db_table("associations", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(mod))
                    continue;

                if (String.IsNullOrWhiteSpace(record))
                    record = "-1";
                if (record == "-1")
                    record = G1.create_record("associations", "association", "-1");
                if (G1.BadRecord("associations", record))
                    continue;
                association = dt.Rows[i]["association"].ObjToString();
                locations = dt.Rows[i]["locations"].ObjToString();
                G1.update_db_table("associations", "record", record, new string[] { "association", association, "locations", locations });
                dt.Rows[i]["mod"] = "";
                dt.Rows[i]["record"] = record;
            }
            modified = false;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private void picDelete_Click(object sender, EventArgs e)
        {
            int row = 0;
            int[] Rows = gridMain.GetSelectedRows();
            for (int i = 0; i < Rows.Length; i++)
            {
                modified = true;
                row = Rows[i];
                DataRow dr = gridMain.GetDataRow(row);
                dr["mod"] = "D";
                btnSave.Show();
            }
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
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            modified = true;
        }
        /****************************************************************************************/
        private void EditTracking_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Add/Edit Associations Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            //trackingSelection = dr["locations"].ObjToString();
            _answer = dr["locations"].ObjToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
    }
}