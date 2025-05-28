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
    public partial class SelectDisplayColumns : Form
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
        /***********************************************************************************************/
        public SelectDisplayColumns(DataGridView dgv, string procName, string groupName, string moduleName, string actualname = "")
        {
            BandedGridView = false;
            work_dgv = dgv;
            if (!String.IsNullOrWhiteSpace(groupName))
                groupKey = groupName;
            if (!String.IsNullOrWhiteSpace(moduleName))
                moduleKey = moduleName;
            if (!String.IsNullOrWhiteSpace(procName))
                procKey = procName;
            actualName = actualname;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public SelectDisplayColumns(DataGridView dgv, string groupName, string moduleName, string actualname = "" )
        {
            BandedGridView = false;
            work_dgv = dgv;
            procKey = "";
            if (!String.IsNullOrWhiteSpace(groupName) )
                groupKey = groupName;
            if (!String.IsNullOrWhiteSpace(moduleName))
                moduleKey = moduleName;
            actualName = actualname;
            InitializeComponent();
        }
/***********************************************************************************************/
        public SelectDisplayColumns(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView dgv, string groupName, string moduleName, string actualname = "")
        {
            BandedGridView = true;
            xdgv = dgv;
            if (!String.IsNullOrWhiteSpace(groupName))
                groupKey = groupName;
            if (!String.IsNullOrWhiteSpace(moduleName))
                moduleKey = moduleName;
            actualName = actualname;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public SelectDisplayColumns(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView dgv, string procName, string groupName, string moduleName, string actualname = "")
        {
            BandedGridView = true;
            xdgv = dgv;
            if (!String.IsNullOrWhiteSpace(groupName))
                groupKey = groupName;
            if (!String.IsNullOrWhiteSpace(moduleName))
                moduleKey = moduleName;
            if (!String.IsNullOrWhiteSpace(procName))
                procKey = procName;
            actualName = actualname;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public SelectDisplayColumns(DevExpress.XtraGrid.GridControl dgv, string groupName, string moduleName, string actualname = "")
        {
            GridControl = true;
            gdgv = dgv;
            if (!String.IsNullOrWhiteSpace(groupName))
                groupKey = groupName;
            if (!String.IsNullOrWhiteSpace(moduleName))
                moduleKey = moduleName;
            actualName = actualname;
            InitializeComponent();
        }
        /***********************************************************************************************/
        public SelectDisplayColumns(DevExpress.XtraGrid.GridControl dgv, string groupName, string moduleName, string actualname = "", string username = "" )
        {
            GridControl = true;
            gdgv = dgv;
            if (!String.IsNullOrWhiteSpace(groupName))
                groupKey = groupName;
            if (!String.IsNullOrWhiteSpace(moduleName))
                moduleKey = moduleName;
            actualName = actualname;
            workUserName = username;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void SelectDisplayColumns_Load(object sender, EventArgs e)
        {
            this.Text = "Select Columns for " + groupKey;
            load_checkboxes();
            DataTable ddx = (DataTable)dgv.DataSource;
            load_group_combo();
            if (!String.IsNullOrWhiteSpace(actualName))
            {
                SetupUsers( actualName );
                loading = true;
                if (actualName.Trim().ToUpper().IndexOf("(C)") >= 0)
                {
                    chkCommon.Checked = true;
                    actualName = actualName.Replace("(C) ", "").Trim();
                }
                else
                    HideActiveUsers();
                comboBox1.Text = actualName;
                loading = false;
            }
            G1.NumberDataTable(ddx);
            dgv.DataSource = ddx;
        }
        /***********************************************************************************************/
        private void HideActiveUsers ()
        {
            chkUsers.Hide();
            label2.Hide();
            if ( !LoginForm.administrator )
                chkCommon.Hide();
        }
        /***********************************************************************************************/
        private void SetupUsers( string name )
        {
            string cmd = "Select * from `users` WHERE `status` = 'active';";
            DataTable dt = G1.get_db_data(cmd);
            chkUsers.Properties.DataSource = dt;
            if (!LoginForm.administrator)
                HideActiveUsers();
        }
        /***********************************************************************************************/
        private DataTable CreateNewTable ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("field");
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
            if (xdgv != null)
            {
                load_xtra_checkboxes();
                return;
            }
            if (gdgv != null)
            {
                load_gdgv_checkboxes();
                return;
            }
            workDt = CreateNewTable();
            DataRow dR = null;
            for (int i = 0; i < work_dgv.Columns.Count; i++)
            {
                string name = work_dgv.Columns[i].Name.ToString().Trim();
                bool selected = work_dgv.Columns[i].Visible;
                dR = workDt.NewRow();
                dR["field"] = name;
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
        /***********************************************************************************************/
        private void load_xtra_checkboxes()
        {
            string oldName = "";
            workDt = CreateNewTable();
            DataRow dR = null;
            for (int i = 0; i < xdgv.Columns.Count; i++)
            {
                string name = xdgv.Columns[i].FieldName.ToString().Trim();
                if (oldName == name)
                    continue;
                oldName = name;
                string caption = xdgv.Columns[i].Caption.ToString().Trim();
                if (String.IsNullOrWhiteSpace(caption))
                    caption = name;
                bool selected = false;
                if (xdgv.Columns[i].VisibleIndex >= 0)
                    selected = true;
                dR = workDt.NewRow();
                dR["field"] = name;
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
        /***********************************************************************************************/
        private void load_gdgv_checkboxes()
        {
            check_count = 1;
            int count = 1;
            string oldName = "";

            int index = 0;
            string select = "";
            DataTable dt = new DataTable();
            dt.Columns.Add("Names");
            dt.Columns.Add("Caption");
            dt.Columns.Add("Selected");
            dt.Columns.Add("Index", Type.GetType("System.Int32"));

            this.SuspendLayout();

            for (int i = 0; i < ((GridView)gdgv.MainView).Columns.Count; i++)
            {
                string name = ((GridView)gdgv.MainView).Columns[i].FieldName.ToString().Trim();
                if (oldName == name)
                    continue;
                oldName = name;
                string caption = ((GridView)gdgv.MainView).Columns[i].Caption.ToString().Trim();
                if (String.IsNullOrWhiteSpace(caption))
                    caption = name;
                select = "";
                if (((GridView)gdgv.MainView).Columns[i].VisibleIndex >= 0)
                    select = "Y";
                index = ((GridView)gdgv.MainView).Columns[i].SortIndex;
                index = ((GridView)gdgv.MainView).Columns[i].VisibleIndex;
                DataRow dRow = dt.NewRow();
                dRow["Names"] = name;
                dRow["Caption"] = caption;
                dRow["Selected"] = select;
                dRow["Index"] = index;
                dt.Rows.Add(dRow);
            }

            oldName = "";

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Index asc";
            dt = tempview.ToTable();

            workDt = CreateNewTable();
            DataRow dR = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Names"].ObjToString().Trim();
                if (oldName == name)
                    continue;
                oldName = name;
                string caption = dt.Rows[i]["Caption"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(caption))
                    caption = name;
                bool selected = false;
                if (dt.Rows[i]["Selected"].ObjToString().ToUpper() == "Y" )
                    selected = true;

                dR = workDt.NewRow();
                dR["field"] = name;
                if (selected)
                    dR["select"] = "1";
                else
                    dR["select"] = "0";
                workDt.Rows.Add(dR);
            }
            SetupSelection(workDt);
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
            //this.ResumeLayout();
            loading = false;
            workDt = (DataTable)dgv.DataSource;
        }
        /***************************************************************************************/
        private void Check_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            modified = true;
        }
        /***************************************************************************************/
        public delegate void d_void_selectionDone( DataTable dt );
        public event d_void_selectionDone Done;
        protected void OnDone()
        {
            if (Done != null)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                Done( dt );
            }
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
                DataTable dt = (DataTable)dgv.DataSource;
                int count = dt.Rows.Count;
                string name = "";
                string select = "";
                for (int i = 0; i < count; i++)
                {
                    name = dt.Rows[i]["field"].ObjToString();
                    if (String.IsNullOrWhiteSpace(name))
                        continue;
                    select = dt.Rows[i]["select"].ObjToString();
                    if (BandedGridView)
                    {
                        if (select == "1" )
                            xdgv.Columns[name].Visible = true;
                        else
                            xdgv.Columns[name].Visible = false;
                    }
                    else if (GridControl)
                    {
                        if ( select == "1" )
                            ((GridView)gdgv.MainView).Columns[name].Visible = true;
                        else
                            ((GridView)gdgv.MainView).Columns[name].Visible = false;
                    }
                    else
                    {
                        if (select == "1")
                            work_dgv.Columns[name].Visible = true;
                        else
                            work_dgv.Columns[name].Visible = false;
                    }
                }
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
        private void cleanout_groupfile(string name)
        {
            if (name.Trim().ToUpper().IndexOf("(C)") >= 0)
            {
                chkCommon.Checked = true;
                name = name.Replace("(C) ", "").Trim();
            }

            string cmd = "Delete from procfiles where name = '" + name + "' and ProcType = '" + groupKey + "' ";
            string user = workUserName;
            if (!String.IsNullOrWhiteSpace(user))
            {
                if (chkCommon.Checked)
                    cmd += " AND `user` = 'Common';";
                else
                    cmd += " AND `user` = '" + user + "';";
            }
            else
            {
                cmd += " AND ( `user` = 'Common' OR `user` = '' );";
            }
            G1.update_db_data(cmd);
        }
/***********************************************************************************************/
        private void set_all_unchecked()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int count = dt.Rows.Count;
            for (int i = 0; i < count; i++)
                dt.Rows[i]["select"] = "0";
            dgv.DataSource = dt;
        }
/***********************************************************************************************/
        private void set_all_checked()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int count = dt.Rows.Count;
            for (int i = 0; i < count; i++)
                dt.Rows[i]["select"] = "1";
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void write_group_format(string name)
        {
            cleanout_groupfile(name);
            DataTable dt = (DataTable)dgv.DataSource;
            int seq = 0;
            int count = dt.Rows.Count;
            string field = "";
            string select = "";
            string user = workUserName;
            if (String.IsNullOrWhiteSpace(user) || chkCommon.Checked)
                user = "Common";
            string activeUsers = chkUsers.Text;
            for (int i = 0; i < count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    string record = G1.create_record("procfiles", "seq", "-1");
                    if (record.Trim().Length > 0)
                    {
                        seq++;
                        G1.update_db_table("procfiles", "record", record, new string[] { "seq", seq.ToString(), "Name", name, "Description", field, "ProcType", groupKey, "module", moduleKey, "user", user });
                        if ( !String.IsNullOrWhiteSpace ( activeUsers ))
                            G1.update_db_table("procfiles", "record", record, new string[] { "activeUsers", activeUsers });
                    }
                }
            }
        }
/***********************************************************************************************/
        private void load_group_combo()
        {
            this.comboBox1.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + groupKey + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                this.comboBox1.Items.Add ( name );
            }
        }
/***********************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        { // Save Format As
            string filename = "";
            string str      = "Please Enter New Group Format Name ?";
            using (Ask askForm = new Ask(str))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                filename = askForm.Answer.Trim();
            }
            if (filename.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where Name = '" + filename + "' and ProcType = '" + groupKey + "' ";
            string user = workUserName;
            if (String.IsNullOrWhiteSpace(user) || chkCommon.Checked)
                user = "Common";
            cmd += " AND (`user` = '" + user + "' OR `user` = '' )";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DialogResult result;
                string question = "The Group Format already exists!\n";
                question       += "Do you want me to OVERWRITE ?";
                result = MessageBox.Show(question, "Group SaveAs Dialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
            }
            write_group_format(filename);
            load_group_combo();
        }
/***********************************************************************************************/
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        { // Save Current Group
            string filename = this.comboBox1.Text.Trim();
            if (filename.Trim().Length == 0)
            {
                DialogResult result;
                string question = "The Group Name is BLANK!\n";
                result = MessageBox.Show(question, "Group Save Dialog" );
                return;
            }
            write_group_format(filename);
        }
/***********************************************************************************************/
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        { // Open New Group Format
            if (loading)
                return;
            string group = comboBox1.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + groupKey + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                set_all_unchecked();
            DataTable dx = (DataTable)dgv.DataSource;
            DataRow[] dRow = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                dRow = dx.Select("field='" + name + "'");
                if (dRow.Length > 0)
                    dRow[0]["select"] = "1";
                //set_column_checked(name);
            }
            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
        }
/***********************************************************************************************/
        private void set_column_checked(string column )
        {
            int count = this.Controls.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.Controls[i].Tag != null)
                {
                    string cname = this.Controls[i].Tag.ToString().Trim();
                    if (cname.Trim().ToUpper() == "RAGCHECK")
                    {
                        CheckBox check = (CheckBox)(this.Controls[i]);
                        string col = check.Name.Trim();
                        if (column.Trim().ToUpper() == col.Trim().ToUpper())
                        {
                            check.Checked = true;
                            break;
                        }
                    }
                }
            }
        }
/***********************************************************************************************/
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        { // Delete Current Format
            string filename = this.comboBox1.Text.Trim();
            if (filename.Trim().Length == 0)
            {
                DialogResult result;
                string question = "The Group Name is BLANK!\n";
                result = MessageBox.Show(question, "Group Delete Dialog");
                return;
            }
            else
            {
                DialogResult result;
                string question = "Are you sure you want to delete this Group?";
                result = MessageBox.Show(question, "Group Delete Dialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
            }
            cleanout_groupfile(filename);
            load_group_combo();
            set_all_checked();
            this.comboBox1.Text = "";
            this.Refresh();
        }
/***********************************************************************************************/
        private void btnOn_Click(object sender, EventArgs e)
        { // Toggle All On
            try
            {
                int count = this.Controls.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this.Controls[i].Tag != null)
                    {
                        string cname = this.Controls[i].Tag.ToString().Trim();
                        if (cname.Trim().ToUpper() == "RAGCHECK")
                        {
                            CheckBox check = (CheckBox)(this.Controls[i]);
                            string name = check.Text.Trim();
                            if (name.ToUpper() == "RECORD")
                                check.Checked = false;
                            else
                                check.Checked = true;
                        }
                    }
                }
                OnDone();
            }
            catch
            {
            }
        }
/***********************************************************************************************/
        private void btnOff_Click(object sender, EventArgs e)
        { // Toggle All Off
            try
            {
                int count = this.Controls.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this.Controls[i].Tag != null)
                    {
                        string cname = this.Controls[i].Tag.ToString().Trim();
                        if (cname.Trim().ToUpper() == "RAGCHECK")
                        {
                            CheckBox check = (CheckBox)(this.Controls[i]);
                            string name = check.Text.Trim();
                            check.Checked = false;
                        }
                    }
                }
                OnDone();
            }
            catch
            {
            }
        }
        /***********************************************************************************************/
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            modified = true;
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
        private void chkUsers_EditValueChanged(object sender, EventArgs e)
        {
            string name = actualName;

        }
        /***********************************************************************************************/
        private void chkCommon_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCommon.Checked)
            {
                chkUsers.Show();
                label2.Show();
                chkCommon.Show();
            }
            else
                HideActiveUsers();
        }
        /***********************************************************************************************/
    }
}
