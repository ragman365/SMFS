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
    public partial class EditManagers : DevExpress.XtraEditors.XtraForm
    {
        private string workTable = "";
        private string workColumns = "";
        private bool modified = false;
        private string workAs = "";
        /****************************************************************************************/
        public EditManagers( string workingAs = "" )
        {
            InitializeComponent();
            workAs = workingAs;
            //if (String.IsNullOrWhiteSpace(workAs))
            //    workAs = "M";
        }
        /****************************************************************************************/
        private void EditManagers_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData ()
        {
            string manager = "";
            DataTable manDt = new DataTable();
            manDt.Columns.Add("num");
            manDt.Columns.Add("ma");
            manDt.Columns.Add("name");
            manDt.Columns.Add("order", Type.GetType("System.Int32"));

            if (String.IsNullOrWhiteSpace(workAs))
            {
                btnRun.Hide();
                workAs = "M";
            }


            DataRow[] dRows = null;
            DataRow dR = null;

            string cmd = "Select * from `funeralhomes`;";
            DataTable funDt = G1.get_db_data(cmd);
            if (workAs == "M")
            {
                for (int i = 0; i < funDt.Rows.Count; i++)
                {
                    manager = funDt.Rows[i]["manager"].ObjToString();
                    if (String.IsNullOrWhiteSpace(manager))
                        continue;

                    dRows = manDt.Select("name='" + manager + "'");
                    if (dRows.Length <= 0)
                    {
                        dR = manDt.NewRow();
                        dR["name"] = manager;
                        dR["ma"] = "M";
                        dR["order"] = 1;
                        manDt.Rows.Add(dR);
                    }
                }
            }

            string firstName = "";
            string lastName = "";
            string middleName = "";
            string name = "";

            if (workAs == "A")
            {
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
                    if (dRows.Length <= 0)
                    {
                        dR = manDt.NewRow();
                        dR["ma"] = "A";
                        dR["name"] = name;
                        dR["order"] = 3;
                        manDt.Rows.Add(dR);
                    }
                }
            }

            DataView tempview = manDt.DefaultView;
            tempview.Sort = "order asc, name asc";
            manDt = tempview.ToTable();

            dgv.DataSource = manDt;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
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
            //DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you really want to exit WITHOUT saving your data?", "Data Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.Yes)
            //    return;
            //e.Cancel = true;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manager = dr["name"].ObjToString();
            if (String.IsNullOrWhiteSpace(manager))
                return;
            string who = dr["ma"].ObjToString();
            FunManager funForm = new FunManager(null, manager, who );
            funForm.Show();
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manager = "";
            string who = "";
            FunManager funForm = new FunManager(dt, manager, who);
            funForm.Show();
        }
        /****************************************************************************************/
    }
}