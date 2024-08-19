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
/***********************************************************************************************/
namespace SMFS
{
/***********************************************************************************************/
    public partial class SelectColumns : Form
    {
        DataGridView work_dgv = null;
        DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView xdgv = null;
        DevExpress.XtraGrid.GridControl gdgv = null;
        private  bool BandedGridView = false;
        private bool GridControl = false;
        private int check_count = 0;
        private string groupKey = "Group";
        private string moduleKey = "SMFS";
        private string procKey = "Proc";
        private string actualName = "";
        private bool modified = false;
        private bool loading = true;
        /***********************************************************************************************/
        public SelectColumns(DataGridView dgv, string procName, string groupName, string moduleName, string actualname = "")
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
        public SelectColumns(DataGridView dgv, string groupName, string moduleName, string actualname = "" )
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
        public SelectColumns(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView dgv, string groupName, string moduleName, string actualname = "")
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
        public SelectColumns(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView dgv, string procName, string groupName, string moduleName, string actualname = "")
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
        public SelectColumns(DevExpress.XtraGrid.GridControl dgv, string groupName, string moduleName, string actualname = "")
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
        private void SelectColumns_Load(object sender, EventArgs e)
        {
            this.Text = "Select Columns for " + groupKey;
            load_checkboxes();
            load_group_combo();
            if ( !String.IsNullOrWhiteSpace ( actualName ))
                comboBox1.Text = actualName;
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
            int x                = this.checkBox1.Left;
			int y                = this.checkBox1.Top;
			int width            = this.checkBox1.Width + 5;
			int height           = this.checkBox1.Height;
			int limit            = (int) ((this.Bottom - this.Top) / 2.0);
			limit                = (int) (this.Bottom - this.Top) - this.checkBox1.Height;
 			int max_y            = y;
 			check_count          = 1;
			int count            = 1;
           for (int i = 0; i < work_dgv.Columns.Count; i++)
            {
                string name = work_dgv.Columns[i].Name.ToString().Trim();
                bool selected = work_dgv.Columns[i].Visible;
                if (i == 0)
                {
                    checkBox1.Name = name;
                    checkBox1.Text = name;
                    if (selected)
                        checkBox1.Checked = true;
                    checkBox1.Tag = "RAGCHECK";
                    checkBox1.Refresh();
                    continue;
                }
                if ((y + height) > limit || count >= 10)
                {
                    y = this.checkBox1.Top;
                    x = x + width;
                    count = 0;
                }
                else
                    y = y + height;
                if ((y + height) > max_y)
                    max_y = (y + height);
                add_checkbox(name, name, x, y, width, height, selected);
                count++;
                check_count++;
            }
            loading = false;
      }
        /***********************************************************************************************/
        private void load_xtra_checkboxes()
        {
            int x = this.checkBox1.Left;
            int y = this.checkBox1.Top;
            int width = this.checkBox1.Width + 5;
            int height = this.checkBox1.Height;
            int limit = (int)((this.Bottom - this.Top) / 2.0);
            limit = (int)(this.Bottom - this.Top) - this.checkBox1.Height;
            limit = limit - height;
            int max_y = y;
            check_count = 1;
            int count = 1;
            string oldName = "";
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
                if (i == 0)
                {
                    checkBox1.Name = name;
                    checkBox1.Text = caption;
                    if (selected)
                        checkBox1.Checked = true;
                    checkBox1.Tag = "RAGCHECK";
                    checkBox1.Refresh();
                    continue;
                }
                if ((y + height) > limit || count >= 20)
                {
                    y = this.checkBox1.Top;
                    x = x + width;
                    count = 0;
                }
                else
                    y = y + height;
                if ((y + height) > max_y)
                    max_y = (y + height);
                add_checkbox(name, caption, x, y, width, height, selected);
                count++;
                check_count++;
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void load_gdgv_checkboxes()
        {
            int x = this.checkBox1.Left;
            int y = this.checkBox1.Top;
            int width = this.checkBox1.Width + 5;
            int height = this.checkBox1.Height;
            int limit = (int)((this.Bottom - this.Top) / 2.0);
            limit = (int)(this.Bottom - this.Top) - this.checkBox1.Height;
            limit = limit - height - height;
            int max_y = y - height - height;
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

//            G1.sortTable(dt, "Names", "asc");

            oldName = "";

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Index asc";
            dt = tempview.ToTable();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Names"].ObjToString().Trim();
                if (name.Trim().ToUpper() == "CONTRACTVALUE")
                {

                }
                if (oldName == name)
                    continue;
                oldName = name;
                string caption = dt.Rows[i]["Caption"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(caption))
                    caption = name;
                bool selected = false;
                if (dt.Rows[i]["Selected"].ObjToString().ToUpper() == "Y" )
                    selected = true;
                if (i == 0)
                {
                    checkBox1.Name = name;
                    checkBox1.Text = caption;
                    if (selected)
                        checkBox1.Checked = true;
                    checkBox1.Tag = "RAGCHECK";
                    checkBox1.Refresh();
                    continue;
                }
                if ((y + height) > limit || count >= 20)
                {
                    y = this.checkBox1.Top;
                    x = x + width;
                    count = 0;
                }
                else
                    y = y + height;
                if ((y + height) > max_y)
                    max_y = (y + height);
                add_checkbox(name, caption, x, y, width, height, selected);
                count++;
                check_count++;
            }
            this.ResumeLayout();
            loading = false;
        }
        /***************************************************************************************/
        private void add_checkbox(string name, string caption, int x, int y, int width, int height, bool selected)
		{
			// Suspend the form layout and add a Checkbox
//			this.SuspendLayout    ();
			CheckBox check        = new CheckBox ();
			check.Location        = new Point ( x, y );
			check.Size            = new Size ( width, height );
			check.Name            = name;
			check.Text            = caption;
			check.Tag             = "RAGCHECK";
			check.TabIndex        = 1;
            if (selected)
                check.Checked = true;
            check.CheckedChanged += Check_CheckedChanged;
			this.Controls.AddRange(new Control[]{check});
//			this.ResumeLayout();
            check.Refresh();
		}
        /***************************************************************************************/
        private void Check_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            modified = true;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone();
        public event d_void_eventdone Done;
        protected void OnDone()
        {
            if (Done != null)
            {
                Done();
            }
        }
/***********************************************************************************************/
        private void SelectColumns_FormClosing(object sender, FormClosingEventArgs e)
        { // Okay, closing
            if (modified)
            {
                DialogResult result = MessageBox.Show("Changes Made! Do you want to honor these changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
                int count = this.Controls.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this.Controls[i].Tag != null)
                    {
                        string cname = this.Controls[i].Tag.ToString().Trim();
                        if (cname.Trim().ToUpper() == "RAGCHECK")
                        {
                            CheckBox check = (CheckBox)(this.Controls[i]);
                            if (BandedGridView)
                            {
                                string name = check.Name.Trim();
                                if (check.Checked)
                                    xdgv.Columns[name].Visible = true;
                                else
                                    xdgv.Columns[name].Visible = false;
                            }
                            else if ( GridControl )
                            {
                                string name = check.Name.Trim();
                                if (check.Checked)
                                {
                                    if (gdgv.MainView != null)
                                        ((GridView)gdgv.MainView).Columns[name].Visible = true;
                                }
                                else
                                {
                                    if (gdgv.MainView != null)
                                        ((GridView)gdgv.MainView).Columns[name].Visible = false;
                                }
                            }
                            else
                            {
                                string name = check.Text.Trim();
                                if (check.Checked)
                                    work_dgv.Columns[name].Visible = true;
                                else
                                    work_dgv.Columns[name].Visible = false;
                            }
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
        private void eToolStripMenuItem_Click(object sender, EventArgs e)
        { // Close Application
            this.Close();
        }
/***********************************************************************************************/
        private void cleanout_groupfile(string name)
        {
            string cmd = "Delete from procfiles where name = '" + name + "' and ProcType = '" + groupKey + "';";
            G1.update_db_data(cmd);
        }
/***********************************************************************************************/
        private void set_all_unchecked()
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
                        check.Checked = false;
                    }
                }
            }
        }
/***********************************************************************************************/
        private void set_all_checked()
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
                        check.Checked = true;
                    }
                }
            }
        }
/***********************************************************************************************/
        private void write_group_format(string name)
        {
            cleanout_groupfile(name);
            int seq = 0;
            int count = this.Controls.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.Controls[i].Tag != null)
                {
                    string cname = this.Controls[i].Tag.ToString().Trim();
                    if (cname.Trim().ToUpper() == "RAGCHECK")
                    {
                        CheckBox check = (CheckBox)(this.Controls[i]);
                        string col = "";
                        if (BandedGridView)
                            col = check.Name.Trim();
                        else if ( GridControl)
                            col = check.Name.Trim();
                        else
                            col = check.Text.Trim();
                        if (check.Checked)
                        {
                            string record = G1.create_record("procfiles", "seq", "-1");
                            if (record.Trim().Length > 0)
                            {
                                seq++;
                                G1.update_db_table("procfiles", "record", record, new string[] { "seq", seq.ToString(), "Name", name, "Description", col, "ProcType", groupKey, "module", moduleKey });
                            }
                        }
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
            string cmd = "Select * from procfiles where Name = '" + filename + "' and ProcType = '" + groupKey + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DialogResult result;
                string question = "The Group Format already exists!\n";
                question       += "Do you want me to OVERWRITE ?";
                result = MessageBox.Show(question, "Group SaveAs Dialog",MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                set_column_checked(name);
            }
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
    }
}
