using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Filtering.Templates;
/***********************************************************************************************/
namespace SMFS
{
/***********************************************************************************************/
    public partial class SelectUserEmail : Form
    {
        DataGridView work_dgv = null;
        DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView xdgv = null;
        DevExpress.XtraGrid.GridControl gdgv = null;
        private  bool BandedGridView = false;
        private bool GridControl = false;
        private int check_count = 0;
        private string workUserName = "";
        private string groupKey = "Group";
        private string moduleKey = "SMFS";
        private string procKey = "Proc";
        private string actualName = "";
        private bool modified = false;
        private bool loading = true;
        private DataTable workDt = null;
        private string workUsers = "";
        private int workRow = -1;
        private DataRow workDr = null;
        /***********************************************************************************************/
        public SelectUserEmail( int row, string usersIn, DataRow dr )
        {
            workUsers = usersIn;
            workRow = row;
            workDr = dr;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void SelectUserEmail_Load(object sender, EventArgs e)
        {
            this.Text = "Select User Emails";

            load_checkboxes();
        }
        /***********************************************************************************************/
        private DataTable CreateNewTable ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("user");
            dt.Columns.Add("userName");
            dt.Columns.Add("select");
            return dt;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt = null )
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt != null)
            {
                if (G1.get_column_number(dt, "select") < 0)
                {
                    dt.Columns.Add("select");
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["select"] = "0";
                }
            }
        }
        /***********************************************************************************************/
        private void load_checkboxes()
        {
            string cmd = "Select * from `users` where `status` = 'active' order by `lastName`, `firstName`;";
            DataTable dt = G1.get_db_data(cmd);

            string firstName = "";
            string lastName = "";
            string userName = "";

            workDt = CreateNewTable();
            DataRow dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                userName = dt.Rows[i]["userName"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                string name = firstName + " " + lastName;
                bool selected = false;
                if (workUsers.Contains(name))
                    selected = true;
                dR = workDt.NewRow();
                dR["user"] = name;
                dR["userName"] = userName;

                if (selected)
                    dR["select"] = "1";
                else
                    dR["select"] = "0";
                workDt.Rows.Add(dR);
            }

            SetupSelection(workDt);

            loading = false;
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
        }
        /***************************************************************************************/
        public delegate void d_void_selectionDone( int row, string names, DataRow dr );
        public event d_void_selectionDone Done;
        protected void OnDone()
        {
            if (Done != null)
            {
                string users = ProcessUsers();
                Done( workRow, users, workDr );
            }
        }
        /***********************************************************************************************/
        private string ProcessUsers ()
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string users = "";
            string name = "";
            string userName = "";
            string select = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if ( select == "1")
                {
                    userName = dt.Rows[i]["userName"].ObjToString();
                    name = dt.Rows[i]["user"].ObjToString();

                    if (!String.IsNullOrWhiteSpace(users))
                        users += "~";
                    users += "(" + userName + ") " + name;
                }
            }
            return users;
        }
        /***********************************************************************************************/
        private void SelectColumns_FormClosing(object sender, FormClosingEventArgs e)
        { // Okay, closing
            if (modified)
            {
                DialogResult result = MessageBox.Show("Changes Made! Do you want to honor these changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == DialogResult.No)
                    return;
            }
            try
            {
                OnDone();
            }
            catch
            {
            }
        }
        /***********************************************************************************************/
        private void eToolStripMenuItem_Click(object sender, EventArgs e)
        { // Close Application
            this.Close();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            string select = dr["select"].ObjToString();
            string doit = "0";
            doit = "0";
            if (select == "0")
                doit = "1";
            loading = true;
            dr["select"] = doit;
            loading = false;
            modified = true;
            gridMain.RefreshData();
            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /***********************************************************************************************/
    }
}
