using System;
using System.Data;
using System.Windows.Forms;

using GeneralLib;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FuneralHomes : Form
    {
        /***********************************************************************************************/
        public FuneralHomes()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void FuneralHomes_Load(object sender, EventArgs e)
        {
            checkPreferences();
            LoadData();
            LoadDBTableFields("funeral_groups", this.repositoryItemComboBox1, "shortname");
            LoadDBTableFields("casket_groups", this.repositoryItemComboBox2, "shortname");
            LoadDBTableAgents(this.repositoryItemCheckedComboBoxEdit1);
            //LoadDBTableUsers(this.repositoryItemCheckedComboBoxEdit3);

            DataTable dt = (DataTable)dgv.DataSource;
            LoadFuneralHomeGroups(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private DataTable LoadFuneralHomeGroups ( DataTable dt )
        {
            string LocationCode = "";
            string groupname = "";
            DataRow[] dRows = null;

            dt.Columns.Add("funeralgroupname");

            string cmd = "Select * from `funeralhomegroups` ORDER BY `order`;";
            DataTable dx = G1.get_db_data(cmd);

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    LocationCode = dt.Rows[i]["LocationCode"].ObjToString();
                    dRows = dx.Select("funeralhomes LIKE '%" + LocationCode + "%'");
                    if (dRows.Length > 0)
                        dt.Rows[i]["funeralgroupname"] = dRows[0]["groupname"].ObjToString();
                }
            }
            catch ( Exception ex )
            {
            }

            return dt;
        }
        /****************************************************************************************/
        private void LoadYearCombo()
        {
            DateTime now = DateTime.Now;
            for (int i = 2012; i < now.Year + 1; i++)
                cmbYear.Items.Add(i.ToString());
            cmbYear.Text = now.Year.ToString();
        }
        /***********************************************************************************************/
        private void checkPreferences()
        {
            //string preference = G1.getPreference(LoginForm.username, "Funeral Homes", "Allow Add");
            //if (preference != "YES")
            //    btnAdd.Hide();
            //preference = G1.getPreference(LoginForm.username, "Funeral Homes", "Allow Delete");
            //if (preference != "YES")
            //    btnDelete.Hide();
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("needtoorder");
            CheckNeedOrders( dt );
            SetupDirectors();
            SetupArrangers();
            G1.NumberDataTable(dt);
            this.dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void btnAddFuneral_Click(object sender, EventArgs e)
        {
            AddHomeNew addhomeFormNew = new AddHomeNew();
            addhomeFormNew.ShowDialog();
            LoadData();
        }
        /***********************************************************************************************/
        private void dgv_DoubleClick(object sender, EventArgs e)
        {
            int row = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( record ))
            {
                AddHomeNew addhomeformNew = new AddHomeNew(record);
                addhomeformNew.ListDone += Addhomeform_ListDone;
                addhomeformNew.Show();

                //AddHome addhomeformNew = new AddHome(record);
                //addhomeformNew.Show();
            }
        }
        /***********************************************************************************************/
        private void Addhomeform_ListDone(string s)
        {
            LoadData();
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if ( record == s )
                {
                    gridMain.FocusedRowHandle = i;
                    gridMain.RefreshEditor(true);
                }
            }
        }
        /***********************************************************************************************/
        private void editFuneralHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                AddHomeNew addhomeform = new AddHomeNew(record);
                addhomeform.ShowDialog();
                LoadData();
            }
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "Funeral Homes", "Allow Add", true);
            if (preference != "YES")
                return;

            AddHomeNew addhomeForm = new AddHomeNew();
            addhomeForm.ShowDialog();
            LoadData();
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "Funeral Homes", "Allow Delete", true);
            if (preference != "YES")
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string desc = dr["name"].ObjToString();
                DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to DELETE " + desc + "?", "Delete Funeral Home Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("***INFO*** Okay, Funeral Home not deleted!", "Delete Funeral Home Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                try
                {
                    G1.delete_db_table("funeralhomes", "record", record);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Deleting Funeral Home " + desc + "!");
                }
            }
        }
        /***********************************************************************************************/
        private void CheckNeedOrders ( DataTable dt )
        {
            if ( dt == null )
                dt = (DataTable)dgv.DataSource;
            string record = "";
            string cmd = "";
            string str = "";
            string casketRecord = "";
            string locationCode = "";
            string casketdescription = "";
            DataTable dx = null;
            DataTable dd = null;
            int minimum = 0;
            int onhand = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                cmd = "Select * from `inventory_on_hand` where `!homeRecord` = '" + record + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                locationCode = dt.Rows[i]["LocationCode"].ObjToString();
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    str = dx.Rows[j]["minimumOnHand"].ObjToString();
                    if ( G1.validate_numeric ( str ))
                    {
                        minimum = str.ObjToInt32();
                        if ( minimum > 0 )
                        {
                            casketRecord = dx.Rows[j]["!casketRecord"].ObjToString();
                            cmd = "Select * from `inventorylist` where `record` = '" + casketRecord + "';";
                            dd = G1.get_db_data(cmd);
                            if (dd.Rows.Count <= 0)
                                continue;
                            casketdescription = dd.Rows[0]["casketdesc"].ObjToString();
                            cmd = "Select * from `inventory` where `LocationCode` = '" + locationCode + "' and `casketdescription` = '" + casketdescription + "' and `ServiceID` = '';";
                            dd = G1.get_db_data(cmd);
                            int count = dd.Rows.Count;
                            if (count <= minimum)
                            {
                                dt.Rows[i]["needtoorder"] = "YES";
                                break;
                            }
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void btnNeeded_Click(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "Funeral Homes", "Inventory Needed", true);
            if (preference != "YES")
                return;
            AddHomeNew homeForm = new AddHomeNew("", true);
            homeForm.Show();
        }
        /***********************************************************************************************/
        private void LoadDBTableAgents(RepositoryItemCheckedComboBoxEdit combo )
        {
            string command = "Select * FROM `agents` order by `agentCode`;";
            DataTable rx = G1.get_db_data(command);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return; // Somehow the table does not exist
            combo.Items.Clear();
            string name = "";
            string agentCode = "";
            string fname = "";
            string lname = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                agentCode = rx.Rows[i]["agentCode"].ObjToString().Trim();
                fname = rx.Rows[i]["firstName"].ObjToString().Trim();
                lname = rx.Rows[i]["lastName"].ObjToString().Trim();
                name = "(" + agentCode + "), " + lname + ", " + fname;
                combo.Items.Add(name);
            }
        }
        /***********************************************************************************************/
        private void LoadDBTableUsers(RepositoryItemCheckedComboBoxEdit combo)
        {
            string command = "Select * FROM `users` order by `username`;";
            DataTable rx = G1.get_db_data(command);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return; // Somehow the table does not exist
            combo.Items.Clear();
            string name = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                name = rx.Rows[i]["username"].ObjToString();
                combo.Items.Add(name);
            }
        }
        /***********************************************************************************************/
        private void LoadDBTableFields(string table, RepositoryItemComboBox combo, string loadField = "")
        {
            if (String.IsNullOrWhiteSpace(table))
                return;
            if (table.ToUpper() == "NONE")
            {
                combo.Items.Clear();
                return;
            }
            string command = "Select * FROM `" + table + "` order by `order`,`record`;";
            DataTable rx = G1.get_db_data(command);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return; // Somehow the table does not exist
            combo.Items.Clear();
            combo.Items.Add("Clear");
            string name = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                name = rx.Rows[i][loadField].ToString().Trim();
                combo.Items.Add(name);
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (e.Column.FieldName.ToUpper() == "GROUPNAME")
            {
                string record = dr["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    string groupname = dr["groupname"].ObjToString();
//                    G1.update_db_table("funeralhomes", "record", record, new string[] { "groupname", groupname} );
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string groupname = dr["groupname"].ObjToString();
                if (what.ToUpper() == "CLEAR")
                    what = "";
                G1.update_db_table("funeralhomes", "record", record, new string[] { "groupname", what });
            }
        }
        /***********************************************************************************************/
        private void groupNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FuneralGroups funForm = new FuneralGroups();
            funForm.Show();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string groupname = dr["casketgroup"].ObjToString();
                if (what.ToUpper() == "CLEAR")
                    what = "";
                G1.update_db_table("funeralhomes", "record", record, new string[] { "casketgroup", what });
            }
        }
        /***********************************************************************************************/
        private void casketGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CasketGroups casketForm = new CasketGroups();
            casketForm.Show();
        }
        /***********************************************************************************************/
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
            string field = "assignedAgents";
            GridColumn column = gridMain.FocusedColumn;
            field = column.FieldName;
            dr[field] = items;
            dt.Rows[row][field] = items;

            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("funeralhomes", "record", record, new string[] { field, items });
        }
        /***********************************************************************************************/
        private DataTable directorsDt = null;
        private void SetupDirectors ()
        {
            if ( directorsDt == null)
            {
                string cmd = "Select * from `directors` order by `location`;";
                directorsDt = G1.get_db_data(cmd);
            }

            DataView tempview = directorsDt.DefaultView;
            tempview.Sort = "lastName asc, firstName asc, middleName asc";
            directorsDt = tempview.ToTable();
        }
        /***********************************************************************************************/
        private DataTable arrangersDt = null;
        private void SetupArrangers()
        {
            if ( arrangersDt == null)
            {
                string cmd = "Select * from `arrangers` order by `location`;";
                arrangersDt = G1.get_db_data(cmd);
            }

            DataView tempview = arrangersDt.DefaultView;
            tempview.Sort = "lastName asc, firstName asc, middleName asc";
            arrangersDt = tempview.ToTable();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckedComboBoxEdit2_EditValueChanged(object sender, EventArgs e)
        {
            string items = string.Empty;
            foreach (CheckedListBoxItem item in this.repositoryItemCheckedComboBoxEdit2.GetItems())
            {
                if (item.CheckState == CheckState.Checked)
                    items += item.Value.ObjToString() + "~";
            }

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            dr["assignedDirectors"] = items;
            dt.Rows[row]["assignedDirectors"] = items;

            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("funeralhomes", "record", record, new string[] { "assignedDirectors", items });
        }
        /***********************************************************************************************/
        private void gridMain_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GridView view = sender as GridView;
        }
        /***********************************************************************************************/
        private void repositoryItemCheckedComboBoxEdit2_Popup(object sender, EventArgs e)
        {
            //int row = gridMain.FocusedRowHandle;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string location = dr["LocationCode"].ObjToString();
            //if (String.IsNullOrWhiteSpace(location))
            //    return;
            //if (directorsDt == null)
            //    return;
            //DataRow[] dRows = directorsDt.Select("location='" + location + "'");
            //if (dRows.Length <= 0)
            //    return;

            ////this.cmbYear.Focus();

            //this.repositoryItemCheckedComboBoxEdit2.Items.Clear();

            //string lastName = "";
            //string firstName = "";
            //string middleName = "";
            //string license = "";
            //string name = "";
            //for (int i = 0; i < dRows.Length; i++)
            //{
            //    lastName = dRows[i]["lastName"].ObjToString();
            //    firstName = dRows[i]["firstName"].ObjToString();
            //    middleName = dRows[i]["middleName"].ObjToString();
            //    license = dRows[i]["license"].ObjToString();
            //    location = dRows[i]["location"].ObjToString();
            //    name = lastName + ", " + firstName + ", " + license + ", (" + location + ")";
            //    name = name.Trim();
            //    this.repositoryItemCheckedComboBoxEdit2.Items.Add(name);
            //}

            //dRows = directorsDt.Select("location<>'" + location + "'");
            //if (dRows.Length <= 0)
            //    return;
            //for (int i = 0; i < dRows.Length; i++)
            //{
            //    lastName = dRows[i]["lastName"].ObjToString();
            //    firstName = dRows[i]["firstName"].ObjToString();
            //    middleName = dRows[i]["middleName"].ObjToString();
            //    license = dRows[i]["license"].ObjToString();
            //    location = dRows[i]["location"].ObjToString();
            //    name = lastName + ", " + firstName + ", " + license + ", (" + location + ")";
            //    name = name.Trim();
            //    this.repositoryItemCheckedComboBoxEdit2.Items.Add(name);
            //}
            //this.repositoryItemCheckedComboBoxEdit2.AllowFocused = true;
            //this.repositoryItemCheckedComboBoxEdit2.RefreshDataSource();
            //gridMain.Columns["assignedDirectors"].ColumnEdit = this.repositoryItemCheckedComboBoxEdit2;
        }
        /***********************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            int row = gridMain.FocusedRowHandle;
            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "ASSIGNEDDIRECTORS")
            {
                Arrangers_ShownEditor();
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();
            string location = dr["LocationCode"].ObjToString();
            string data = dr["assignedDirectors"].ObjToString();
            if (String.IsNullOrWhiteSpace(location))
                return;
            if (directorsDt == null)
                return;
            DataRow[] dRows = directorsDt.Select("location='" + location + "'");
            if (dRows.Length <= 0)
                return;

            this.repositoryItemCheckedComboBoxEdit2.Items.Clear();

            string lastName = "";
            string firstName = "";
            string middleName = "";
            string license = "";
            string name = "";
            int count = 0;
            for (int i = 0; i < dRows.Length; i++)
            {
                lastName = dRows[i]["lastName"].ObjToString();
                firstName = dRows[i]["firstName"].ObjToString();
                middleName = dRows[i]["middleName"].ObjToString();
                license = dRows[i]["license"].ObjToString();
                location = dRows[i]["location"].ObjToString();
                name = lastName + ", " + firstName + ", " + license + ", (" + location + ")";
                name = name.Trim();
                if (data.Contains(name))
                    this.repositoryItemCheckedComboBoxEdit2.Items.Add(name, true );
                else
                    this.repositoryItemCheckedComboBoxEdit2.Items.Add(name, false);
                count++;
            }

            dRows = directorsDt.Select("location<>'" + location + "'");
            if (dRows.Length <= 0)
                return;
            for (int i = 0; i < dRows.Length; i++)
            {
                lastName = dRows[i]["lastName"].ObjToString();
                firstName = dRows[i]["firstName"].ObjToString();
                middleName = dRows[i]["middleName"].ObjToString();
                license = dRows[i]["license"].ObjToString();
                location = dRows[i]["location"].ObjToString();
                name = lastName + ", " + firstName + ", " + license + ", (" + location + ")";
                name = name.Trim();
                if (data.Contains(name))
                    this.repositoryItemCheckedComboBoxEdit2.Items.Add(name, true);
                else
                    this.repositoryItemCheckedComboBoxEdit2.Items.Add(name, false);
                count++;
            }

            this.repositoryItemCheckedComboBoxEdit2.ForceUpdateEditValue = DevExpress.Utils.DefaultBoolean.True;


            //this.repositoryItemCheckedComboBoxEdit2.AllowFocused = true;
            //this.repositoryItemCheckedComboBoxEdit2.RefreshDataSource();
            //gridMain.Columns["assignedDirectors"].ColumnEdit = this.repositoryItemCheckedComboBoxEdit2;
            //gridMain.RefreshEditor(true);
            //gridMain.RefreshData();
        }
        /***********************************************************************************************/
        private void Arrangers_ShownEditor()
        {
            int row = gridMain.FocusedRowHandle;
            GridColumn currCol = gridMain.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "ASSIGNEDARRANGERS")
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string location = dr["LocationCode"].ObjToString();
            string data = dr["assignedArrangers"].ObjToString();
            if (String.IsNullOrWhiteSpace(location))
                return;
            if (arrangersDt == null)
                return;
            DataRow[] dRows = arrangersDt.Select("location='" + location + "'");
            if (dRows.Length <= 0)
                return;

            this.repositoryItemCheckedComboBoxEdit3.Items.Clear();

            string lastName = "";
            string firstName = "";
            string middleName = "";
            string license = "";
            string name = "";
            int count = 0;
            for (int i = 0; i < dRows.Length; i++)
            {
                lastName = dRows[i]["lastName"].ObjToString();
                firstName = dRows[i]["firstName"].ObjToString();
                middleName = dRows[i]["middleName"].ObjToString();
                license = dRows[i]["license"].ObjToString();
                location = dRows[i]["location"].ObjToString();
                name = lastName + ", " + firstName + ", " + license + ", (" + location + ")";
                name = name.Trim();
                if (data.Contains(name))
                    this.repositoryItemCheckedComboBoxEdit3.Items.Add(name, true);
                else
                    this.repositoryItemCheckedComboBoxEdit3.Items.Add(name, false);
                count++;
            }

            dRows = arrangersDt.Select("location<>'" + location + "'");
            if (dRows.Length <= 0)
                return;
            for (int i = 0; i < dRows.Length; i++)
            {
                lastName = dRows[i]["lastName"].ObjToString();
                firstName = dRows[i]["firstName"].ObjToString();
                middleName = dRows[i]["middleName"].ObjToString();
                license = dRows[i]["license"].ObjToString();
                location = dRows[i]["location"].ObjToString();
                name = lastName + ", " + firstName + ", " + license + ", (" + location + ")";
                name = name.Trim();
                if (data.Contains(name))
                    this.repositoryItemCheckedComboBoxEdit3.Items.Add(name, true);
                else
                    this.repositoryItemCheckedComboBoxEdit3.Items.Add(name, false);
                count++;
            }

            this.repositoryItemCheckedComboBoxEdit3.ForceUpdateEditValue = DevExpress.Utils.DefaultBoolean.True;


            //this.repositoryItemCheckedComboBoxEdit2.AllowFocused = true;
            //this.repositoryItemCheckedComboBoxEdit2.RefreshDataSource();
            //gridMain.Columns["assignedDirectors"].ColumnEdit = this.repositoryItemCheckedComboBoxEdit2;
            //gridMain.RefreshEditor(true);
            //gridMain.RefreshData();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckedComboBoxEdit3_EditValueChanged(object sender, EventArgs e)
        {
            string items = string.Empty;
            foreach (CheckedListBoxItem item in this.repositoryItemCheckedComboBoxEdit3.GetItems())
            {
                if (item.CheckState == CheckState.Checked)
                    items += item.Value.ObjToString() + "~";
            }

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            dr["assignedArrangers"] = items;
            dt.Rows[row]["assignedArrangers"] = items;

            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("funeralhomes", "record", record, new string[] { "assignedArrangers", items });
        }
        /***********************************************************************************************/
        private void repositoryItemCheckedComboBoxEdit3_Popup(object sender, EventArgs e)
        {
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string data = dr["assignedArrangers"].ObjToString();
            //string items = string.Empty;
            //foreach (CheckedListBoxItem item in this.repositoryItemCheckedComboBoxEdit3.GetItems())
            //{
            //    if (item.CheckState == CheckState.Checked)
            //        items += item.Value.ObjToString() + "~";
            //    item.CheckState = CheckState.Checked;
            //}
        }
        /***********************************************************************************************/
        private void btnMassPriceList_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            MassFuneralHomes massForm = new MassFuneralHomes( dt );
            massForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}
