using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using GeneralLib;
using DevExpress.XtraGrid.Columns;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class AddEditUsers : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        public AddEditUsers()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void LoadPermissions ()
        {
            string cmd = "Select * from `preferencelist`;";
            DataTable dt = G1.get_db_data(cmd);
            int col = G1.get_column_number(dt, "default");
            string name = "";
            for ( int i=col+1; i<dt.Columns.Count; i++)
            {
                name = dt.Columns[i].ColumnName.Trim();
                this.repositoryItemComboBox1.Items.Add(name);
            }
        }
        /****************************************************************************************/
        private void AddEditUsers_Load(object sender, EventArgs e)
        {
            LoadPermissions();
            SetupLocations();
            LoadData();
        }
        /***********************************************************************************************/
        private DataTable funDt = null;
        private void SetupLocations()
        {
            if (funDt == null)
            {
                string cmd = "Select * from `funeralhomes`;";
                funDt = G1.get_db_data(cmd);
            }
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            if ( !LoginForm.administrator)
            {
                this.contextMenuStrip1.Dispose();
                btnDelete.Hide();
            }
            string cmd = "Select * from `users` order by `lastName`,`firstName`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            dt.Columns.Add("Password");
            dt.Columns.Add("permissions");
            string pwd = "";
            string classification = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                pwd = dt.Rows[i]["pwd"].ObjToString();
                if (!String.IsNullOrWhiteSpace(pwd))
                    dt.Rows[i]["Password"] = "*********";
                pwd = dt.Rows[i]["admin"].ObjToString();
                if (pwd.ToUpper() == "TRUE")
                    dt.Rows[i]["permissions"] = "Admin";
                classification = dt.Rows[i]["classification"].ObjToString();
                if (!String.IsNullOrWhiteSpace(classification))
                    dt.Rows[i]["permissions"] = classification;
            }
            this.dgv.DataSource = dt;
            if (saveRowHandle >= 0)
                gridMain.FocusedRowHandle = saveRowHandle;
        }
        /****************************************************************************************/
        private void toggleActiveStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( dt.Rows.Count <= 1 )
            {
                MessageBox.Show("***ERROR*** You cannot force the only user to inactive.", "Status Change Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string status = dr["status"].ObjToString();
            if (status == "ACTIVE")
                status = "INACTIVE";
            else
                status = "ACTIVE";
            G1.update_db_table("users", "record", record, new string[] { "status", status });
            dr["status"] = status.ToLower();
            dgv.Refresh();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        int saveRowHandle = -1;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `users` where `record` = '" + LoginForm.workUserRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Locating User in DataBase.", "Add Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if ( !LoginForm.administrator )
            {
                MessageBox.Show("***ERROR*** You must have administrative permission to add new users!", "Add Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            addUser addForm = new addUser();
            addForm.ShowDialog();
            saveRowHandle = -1;
            LoadData();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            saveRowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string status = dr["status"].ObjToString();
            if (status.ToUpper() == "INACTIVE")
            {
                MessageBox.Show("***ERROR*** You cannot change an Inactive User.", "Status Change Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (record == LoginForm.workUserRecord)
            {
                addUser addForm = new addUser(false, record);
                addForm.ShowDialog();
                LoadData();
                return;
            }
            string cmd = "Select * from `users` where `record` = '" + LoginForm.workUserRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Locating User in DataBase.", "Edit User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string admin = dt.Rows[0]["admin"].ObjToString();
            if ( admin.ToUpper() != "TRUE")
            {
                MessageBox.Show("***ERROR*** You must have administrative permission to edit other users information!", "Edit User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            addUser add1Form = new addUser(false, record);
            add1Form.ShowDialog();
            LoadData();
        }
        /****************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( dt.Rows.Count <= 1 )
            {
                MessageBox.Show("***ERROR*** You cannot delete the last user in the database!", "Add/Edit User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string name = dr["firstName"].ObjToString() + " " + dr["lastName"].ObjToString();
            DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to remove user " + name + "?", "Add/Edit User Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if ( result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, User not removed!", "Add/Edit User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                G1.delete_db_table("users", "record", record);
                MessageBox.Show("***INFO*** Okay, User " + name + " has been REMOVED!", "Add/Edit User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Removing user " + name + "! " + ex.Message.ToString(), "Add/Edit User Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /****************************************************************************************/
        private void btnPreferences_Click(object sender, EventArgs e)
        {
            PreferenceList preForm = new PreferenceList();
            preForm.ShowDialog();
        }
        /****************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string record = dr["record"].ObjToString();
            string classification = dr["permissions"].ObjToString();
            string email = dr["email"].ObjToString();
            string agent = dr["agentCode"].ObjToString();
            string assignedLocations = dr["assignedLocations"].ObjToString();
            string admin = "0";
            if (classification.ToUpper() == "ADMIN")
                admin = "1";
            G1.update_db_table("users", "record", record, new string[] {"classification", classification, "email", email, "agentCode", agent, "admin", admin, "assignedLocations", assignedLocations });
        }
        /****************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            int row = gridMain.FocusedRowHandle;
            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            DataRow dr = gridMain.GetFocusedDataRow();
            string data = dr["assignedLocations"].ObjToString();
            if (funDt == null)
                return;
            if ( currentColumn.ToUpper() == "PERMISSIONS")
            {
                return;
            }

            this.repositoryItemCheckedComboBoxEdit1.Items.Clear();

            string locationCode = "";
            int count = 0;
            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                locationCode = funDt.Rows[i]["locationCode"].ObjToString();
                if (data.Contains(locationCode))
                    this.repositoryItemCheckedComboBoxEdit1.Items.Add(locationCode, true);
                else
                    this.repositoryItemCheckedComboBoxEdit1.Items.Add(locationCode, false);
                count++;
            }

            this.repositoryItemCheckedComboBoxEdit1.ForceUpdateEditValue = DevExpress.Utils.DefaultBoolean.True;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit) sender;
            string what = combo.Text;
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "PERMISSIONS")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = dt.Rows[row]["permissions"].ObjToString();
            }
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_Validating(object sender, CancelEventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            string what = combo.Text;
            if (what == "HR")
            {
                using (Ask fmrmyform = new Ask("Enter HR Password > "))
                {
                    fmrmyform.Text = "";
                    fmrmyform.ShowDialog();
                    string p = fmrmyform.Answer.Trim(); //VkjaW3euXJwgZFaV1KZAJg== P@ssword.1090
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        string PasswordHash = LoginForm.Hash(p);

                        if ( PasswordHash != "VkjaW3euXJwgZFaV1KZAJg==")
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    else
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }
        /****************************************************************************************/
    }
}