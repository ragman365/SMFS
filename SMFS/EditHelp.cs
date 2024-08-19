using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using Ionic.Zip;
using System.Runtime.InteropServices;
using GeneralLib;
using EMRControlLib;
/***********************************************************************************************/
namespace SMFS
{
/***********************************************************************************************/
    public partial class EditHelp : Form
    {
        private string lastRecord = "";
        private string lastTitle = "";
        private string lastFile = "";
        private string lastWho = "";
/***********************************************************************************************/
        private bool AllowAddChangeDelete = false;
        private bool AllowConfigure = false;
        private bool AllowAssignUsers = false;
/***********************************************************************************************/
        private string SystemHelp = "";
        private bool HelpInterface = false;
        private bool loading = false;
        private PleaseWait pleaseForm = null;
        public static string HelpDB = "LOCAL";
        public static string HelpSystem = "";
        public static string HelpModule = "";
        public static string HelpTitle = "";
        public static bool HelpEdit = false;
        private string workSystem = "";
        /***********************************************************************************************/
        public EditHelp( string system = "" )
        {
            InitializeComponent();
            workSystem = system;
        }
/***********************************************************************************************/
        private void EditHelp_Load(object sender, EventArgs e)
        {
            rtb2.RichTextBox.Multiline = true;
            HelpEdit = true;
            pleaseForm = new PleaseWait();
            pleaseForm.Show();
            this.Cursor = Cursors.WaitCursor;
            loading = true;
            panelTop.Visible = false;
            //if (HelpDB == "LOCAL")
            //    HelpSystem = "Local";
            try
            {
                if (!SetupConfiguration())
                {
                    this.Close();
                    return;
                }

                LoadSystem();
                //if (1 == 1)
                //    return;
                LoadHelp();
                if (!HelpInterface)
                {
                    rtb2.Dock = DockStyle.Fill;
                    rtb2.RichTextBox.Dock = DockStyle.Fill;
                    panelRTB2.Dock = DockStyle.Fill;
//                    rtb1.Hide();
                }
                else
                {
                    if (!HelpEdit)
                    {
                        rtb2.Hide();
                        rtb2.RichTextBox.Hide();
                        panelRTB2.Hide();
                        rtb1.Dock = DockStyle.Fill;
                        rtb1.ReadOnly = true;
                        btnUp.Hide();
                        btnDown.Hide();
                        btnAdd.Hide();
                        btnDelete.Hide();
                        panelRightTop.Visible = false;
                        this.setPasswordToolStripMenuItem.Visible = false;
                        this.setPasswordToolStripMenuItem.Enabled = false;
                        this.setPasswordToolStripMenuItem1.Visible = false;
                        this.setPasswordToolStripMenuItem1.Enabled = false;
                        this.setPasswordToolStripMenuItem2.Visible = false;
                        this.setPasswordToolStripMenuItem2.Enabled = false;
                        this.clearPasswordToolStripMenuItem.Visible = false;
                        this.clearPasswordToolStripMenuItem.Enabled = false;
                        this.clearPasswordToolStripMenuItem1.Visible = false;
                        this.clearPasswordToolStripMenuItem1.Enabled = false;
                        this.clearPasswordToolStripMenuItem2.Visible = false;
                        this.clearPasswordToolStripMenuItem2.Enabled = false;
                        this.Text = "Help";
                        if (!String.IsNullOrWhiteSpace(HelpModule))
                        {
                            this.Text += " for " + HelpModule;
                            if (!String.IsNullOrWhiteSpace(HelpTitle))
                                this.Text += " - " + HelpTitle;
                        }
                        else if (!String.IsNullOrWhiteSpace(HelpTitle))
                            this.Text += " for " + HelpTitle;
                        this.gridView1.Columns["assigned"].Visible = false;
                        this.gridView2.Columns["assigned"].Visible = false;
                        this.gridView3.Columns["assigned"].Visible = false;
                    }
                    else
                    {
                        rtb2.Dock = DockStyle.Fill;
                        rtb2.RichTextBox.Dock = DockStyle.Fill;
                        panelRTB2.Dock = DockStyle.Fill;
                        rtb1.Hide();
                        rtb1.ReadOnly = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** " + ex.Message);
            }
            this.Cursor = Cursors.Default;
            loading = false;
            if (!LoginForm.administrator)
            {
                menuAdmin.Visibility = BarItemVisibility.Never;
            }
            pleaseForm.FireEvent1();
            panelSystemAll.Visible = true;
        }
        /***********************************************************************************************/
        private bool SetupConfiguration()
        {
            AllowAddChangeDelete = false;
            AllowConfigure = false;
            //string preference = User.getInstance.UserPreferences.AdministrativePreference.GetPreference("CDS", "Allow Add/Change/Delete Rules");
            //if (preference.Trim().ToUpper() == "YES")
            //    AllowAddChangeDelete = true;

            //preference = User.getInstance.UserPreferences.AdministrativePreference.GetPreference("CDS", "Allow Configure Rules");
            //if (preference.Trim().ToUpper() == "YES")
            //    AllowConfigure = true;
            //preference = User.getInstance.UserPreferences.AdministrativePreference.GetPreference("CDS", "Allow Assign Users");
            //if (preference.Trim().ToUpper() == "YES")
            //    AllowAssignUsers = true;
            //preference = User.getInstance.UserPreferences.AdministrativePreference.GetPreference("CDS", "Allow Access To User History");
            //if (preference.Trim().ToUpper() == "YES")
            //    AllowUserHistory = true;
            AllowAddChangeDelete = true;
            AllowConfigure = true;
            AllowAssignUsers = true;

            if (!AllowAddChangeDelete && !AllowConfigure && !AllowAssignUsers )
            {
                MessageBox.Show("***ERROR*** You do not have permission to run this module! Call I/T");
                return false;
            }

            if (!AllowAddChangeDelete)
            {
                this.dgv.Enabled = false;
                this.rtb1.Enabled = false;
                this.rtb2.Enabled = false;
            }
            if (!AllowConfigure)
            {
            }
            if (!AllowAssignUsers)
            {
            }
            return true;
        }
/***********************************************************************************************/
        private void LoadSystem()
        {
            dgv2.Dock = DockStyle.Fill;
            string cmd = "Select * from `help` where `removed` = '0' and `type` = 'system' ";
            if (!String.IsNullOrWhiteSpace(workSystem))
            {
                cmd += " and `system` = '" + workSystem + "' ";
                HelpSystem = workSystem;
            }
            cmd += " order by `sequence`; ";
            DataTable dx = get_help_data(cmd);
            dx.Columns.Add("num");
            dx.Columns.Add("mod");
            dx.Columns.Add("select");
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                string record = dx.Rows[i]["record"].ToString();
                string system = dx.Rows[i]["system"].ToString().Replace('\n',';');
                string module = dx.Rows[i]["module"].ToString().Replace('\n',';');
                dx.Rows[i]["system"] = system;
                dx.Rows[i]["module"] = module;
                dx.Rows[i]["num"] = (i + 1).ToString();
                dx.Rows[i]["mod"] = "";
                dx.Rows[i]["record"] = record;
            }
            dgv2.DataSource = dx;
            if (!String.IsNullOrWhiteSpace(HelpSystem))
            {
                HelpInterface = true;
                splitContainerControl3.PanelVisibility = SplitPanelVisibility.Panel2;
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    string str = dx.Rows[i]["system"].ObjToString().ToUpper();
                    if (str == HelpSystem.ToUpper())
                    {
                        gridView2.FocusedRowHandle = i;
                        break;
                    }
                    //string str = dx.Rows[i]["module"].ObjToString().ToUpper();
                    //if (str == Help.HelpModule.ToUpper())
                    //{
                    //    gridView1.FocusedRowHandle = i;
                    //    break;
                    //}
                }
            }
            else
            {
                if (dx.Rows.Count > 0)
                    SystemHelp = dx.Rows[0]["system"].ObjToString();
            }
        }
/***********************************************************************************************/
        private void LoadHelp( bool allow = true )
        {
            dgv.Dock = DockStyle.Fill;
            DataRow dr = gridView2.GetFocusedDataRow();
            if (dr == null)
                return;

            string system = dr["system"].ObjToString();
            string module = dr["module"].ObjToString();
            string record = dr["record"].ObjToString();
            string cmd = "Select * from `help` where `backRecord` = '" + record + "' and `type` = 'help' and `removed` = '0' order by `sequence`;";
            DataTable dx = get_help_data(cmd);
            dx.Columns.Add("num");
            dx.Columns.Add("mod");
            dx.Columns.Add("select");
            if (!allow)
                dx.Rows.Clear();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ToString();
                module = dx.Rows[i]["module"].ToString().Replace('\n',';');
                dx.Rows[i]["module"] = module;
                dx.Rows[i]["num"] = (i + 1).ToString();
                dx.Rows[i]["mod"] = "";
                dx.Rows[i]["record"] = record;
            }
            try
            {
                dgv.DataSource = dx;
                if (!String.IsNullOrWhiteSpace(HelpModule))
                {
                    HelpInterface = true;
                    splitContainerControl1.PanelVisibility = SplitPanelVisibility.Panel2;
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        string str = dx.Rows[i]["module"].ObjToString().ToUpper();
                        if (str == HelpModule.ToUpper())
                        {
                            gridView1.FocusedRowHandle = i;
                            break;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Loading Help " + ex.Message.ToString());
            }
        }
/***********************************************************************************************/
        private DataTable get_help_data( string cmd )
        {
            if (HelpSystem == "Local")
                return G1.get_db_data(cmd);
            else
                return G1.get_db_data(cmd);
        }
/***********************************************************************************************/
        private string create_help_record( string tablename, string fieldname, string initvalue )
        {
            if (HelpSystem == "Local")
                return G1.create_record(tablename, fieldname, initvalue);
            else
                return G1.create_record(tablename, fieldname, initvalue);
        }
/***********************************************************************************************/
        private bool update_help_table(string db, string keyfield, string record, string[] fields)
        {
            if (HelpSystem == "Local")
                return G1.update_db_table(db, keyfield, record, fields);
            else
                return G1.update_db_table(db, keyfield, record, fields);
        }
/***********************************************************************************************/
        private void LoadCurrentHelp()
        {
            if (rtb2.RichTextBox.Modified )
            {
                if (DevExpress.XtraEditors.XtraMessageBox.Show("Current Help File Modified!\nDo you want to save it now?", "Help File Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    btnSaveFile_Click(null, null);
            }
            gridView1.GroupPanelText = SystemHelp;
            DataRow dr = gridView1.GetFocusedDataRow();
            if (dr == null)
            {
                rtb1.Clear();
                rtb2.RichTextBox.Clear();
                string cmdx = "Select * from `help` where `backRecord` = '-999' and `type` = 'file' and `removed` = '0' order by `sequence`;";
                DataTable dxx = get_help_data(cmdx);
                dgv6.DataSource = dxx;
                return;
            }

            string module = dr["module"].ObjToString();
            string record = dr["record"].ObjToString();
            string p      = dr["p"].ObjToString();
            if (ValidatePassword(p, module ))
            {
                string cmd = "Select * from `help` where `backRecord` = '" + record + "' and `type` = 'file' and `removed` = '0' order by `sequence`;";
                DataTable dx = get_help_data(cmd);
                NumberDataTable(dx);
                dgv6.DataSource = dx;
                if (!String.IsNullOrWhiteSpace(HelpTitle))
                {
                    splitContainerControl1.PanelVisibility = SplitPanelVisibility.Panel2;
                    splitContainerControl2.PanelVisibility = SplitPanelVisibility.Panel2;
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        string str = dx.Rows[i]["title"].ObjToString().ToUpper();
                        if (str == HelpTitle.ToUpper())
                        {
                            gridView3.FocusedRowHandle = i;
                            break;
                        }
                    }
                }
                LoadCurrentFile();
            }
            rtb1.Modified = false;
            rtb2.RichTextBox.Modified = false;
        }
/***********************************************************************************************/
        private void LoadCurrentFile()
        {
            if (rtb2.RichTextBox.Modified )
            {
                if (DevExpress.XtraEditors.XtraMessageBox.Show("Current Help File Modified called " + lastTitle + "!\nDo you want to save it now?", "Help File Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    SaveFile(lastRecord, lastFile);
            }

            DataTable dt = (DataTable)dgv6.DataSource;
            int rowHandle = gridView3.FocusedRowHandle;
            if (rowHandle < 0)
            {
                lastFile = "";
                lastTitle = "";
                lastWho = "";
                lastRecord = "";
                rtb1.Clear();
                rtb2.RichTextBox.Clear();
                rtb2.RichTextBox.Modified = false;
                rtb1.Modified = false;
                return;
            }

            DataRow dr = gridView3.GetFocusedDataRow();

            if (dr != null)
            {
                string module = dr["module"].ObjToString();
                string record = dr["record"].ObjToString();
                string title  = dr["title"].ObjToString();
                string who    = dr["assigned"].ObjToString();
                string filename = dr["filename"].ObjToString();
                rtb1.Clear();
                rtb2.RichTextBox.Clear();
                string p = dr["p"].ObjToString();
                if (ValidatePassword(p, title))
                {
                    lastFile = filename;
                    lastTitle = title;
                    lastWho = who;
                    lastRecord = record;
                    if (!String.IsNullOrWhiteSpace(filename))
                    {

                        ReadRtbFile(filename);
                    }
                }
                rtb1.Modified = false;
                rtb2.RichTextBox.Modified = false;
            }
        }
/***********************************************************************************************/
        private bool ValidatePassword(string p, string area = "")
        {
            if (!String.IsNullOrWhiteSpace(p))
            {
                string dp = GetDecriptedWord(p);
                using (Ask fmrmyform = new Ask("Enter Password To Access " + area + " > "))
                {
                    fmrmyform.Text = "";
                    fmrmyform.ShowDialog();
                    string answer = fmrmyform.Answer.Trim().ToUpper();
                    if (String.IsNullOrWhiteSpace(answer))
                        return false; // Loser!
                    if (answer != dp)
                    {
                        MessageBox.Show("***ERROR*** Invalid Password!!!");
                        return false;
                    }
                }
            }
            return true;
        }
/***********************************************************************************************/
        private void RemoveRtbFile(string filename)
        {
            if (!String.IsNullOrWhiteSpace(filename))
            {
                string path = G1.spreadpath + "/help/";
                G1.verify_path(path);
                string fullpath = path + filename;
                try
                {
                    if (File.Exists(fullpath))
                        File.Delete(fullpath);
                    else if (File.Exists(fullpath + ".zip"))
                        File.Delete(fullpath + ".zip");
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** Deleting File " + ex.ToString());
                }
            }
        }
/***********************************************************************************************/
        private RichTextBox ReadStraightRtbFile(string filename)
        {
            RichTextBox rtb1 = new RichTextBox();
            rtb1.Clear();
            if (!String.IsNullOrWhiteSpace(filename))
            {
                string path = G1.spreadpath + "/help/";
                G1.verify_path(path);
                string fullpath = path + filename;
                if (File.Exists(fullpath))
                    rtb1.LoadFile(fullpath);
                //else if (File.Exists(fullpath + ".zip"))
                //{
                //    OpenZipFile(fullpath);
                //    string newpath = G1.spreadpath + "/help/" + filename + G1.spreadpath + "/help/" + filename;
                //    if (File.Exists(newpath))
                //    {
                //        rtb1.LoadFile(newpath);
                //        Directory.Delete(fullpath, true ); // Cleanup Unzipped Stuff
                //    }
                //}
            }
            rtb1.Modified = false;
            return rtb1;
        }
/***********************************************************************************************/
        private void ReadRtbFile(string filename)
        {
            rtb1.Clear();
            rtb2.RichTextBox.Clear();
            if (!String.IsNullOrWhiteSpace(filename))
            {
                string path = G1.spreadpath + "/help/";
                G1.verify_path(path);
                string fullpath = path + filename;
                if (File.Exists(fullpath))
                {
                    rtb1.LoadFile(fullpath);
                    rtb2.RichTextBox.LoadFile(fullpath);
                }
                //else if (File.Exists(fullpath + ".zip"))
                //{
                //    OpenZipFile(fullpath);
                //    string extraDirectory = G1.spreadpath;
                //    extraDirectory = extraDirectory.Replace("C:", "");

                //    string newpath = G1.spreadpath + "/help/" + filename + extraDirectory + "/help/" + filename;
                //    if (File.Exists(newpath))
                //    {
                //        rtb1.LoadFile(newpath);
                //        rtb2.RichTextBox.LoadFile(newpath);
                //        Directory.Delete(fullpath, true ); // Cleanup Unzipped Stuff
                //    }
                //}
            }
            lastFile = filename;
            rtb1.Modified = false;
            rtb2.RichTextBox.Modified = false;
        }
/***********************************************************************************************/
        private void WriteRtbFile(string record, string filename)
        {
            if (!String.IsNullOrWhiteSpace(filename))
            {
                string path = G1.spreadpath + "/help/";
                G1.verify_path(path);
                string fullpath = path + filename;
                rtb2.RichTextBox.SaveFile(fullpath);
                //if (File.Exists(fullpath))
                //{
                //    if (File.Exists(fullpath + ".zip"))
                //        File.Delete(fullpath + ".zip");
                //    using (ZipFile zip = new ZipFile(fullpath + ".zip"))
                //    {
                //        zip.Password = GetEncriptedWord();
                //        zip.AddFile(fullpath);
                //        zip.Save();
                //    }
                //    File.Delete(fullpath);
                //}
            }
        }
/***********************************************************************************************/
        private void NumberDataTable(DataTable dt)
        {
            try
            {
                if (G1.get_column_number(dt, "num") < 0)
                    dt.Columns.Add("num");
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["num"] = (i + 1).ToString();
                dt.AcceptChanges();
            }
            catch
            {
            }
        }
/***********************************************************************************************/
        private void SaveFile( string record, string filename )
        {
            if (String.IsNullOrWhiteSpace(filename))
                return;
            try
            {
                WriteRtbFile(record, filename);
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Writing Help File " + ex.Message.ToString());
            }
            rtb2.RichTextBox.Modified = false;
        }
/***********************************************************************************************/
        private void btnSaveFile_Click(object sender, EventArgs e)
        {
			DataRow dr = gridView3.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridView3.FocusedRowHandle;
            string record = dr["record"].ToString();
            string module = dr["module"].ToString();
            string title = dr["title"].ObjToString();
            string filename = dr["filename"].ObjToString();
            try
            {
                WriteRtbFile(record, filename);
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Writing Help File " + ex.Message.ToString());
            }
            rtb2.RichTextBox.Modified = false;
        }
/***********************************************************************************************/
        private void btnAbortFile_Click(object sender, EventArgs e)
        {
            LoadCurrentFile();
        }
/***********************************************************************************************/
        private void btnAddNewHelp_Click(object sender, EventArgs e)
        {
			DataRow drr = gridView2.GetFocusedDataRow();
            int rowHandle = gridView2.FocusedRowHandle;
            string systemrecord = drr["record"].ToString(); // Assign this record as the Modules backrecord
            
            
            DataTable dt = (DataTable)(dgv.DataSource);
            DataRow dr = dt.NewRow();
            string module = "New Module " + dt.Rows.Count.ToString();
            string record = G1.create_record("help", "system", "-1");
            if (record != "")
            {
                G1.update_db_table ( "help", "record", record, new string[] { "module", module, "type", "help", "sequence", dt.Rows.Count.ToString(), "backrecord", systemrecord } );
                dr["record"] = record;
                dr["module"] = module;
                dr["sequence"] = dt.Rows.Count;
                dt.Rows.Add(dr);
                dt.AcceptChanges();
                dgv.DataSource = dt;
                try
                {
                    dr["num"] = dt.Rows.Count.ToString();
                }
                catch
                {
                }
            }
        }
/***********************************************************************************************/
        private void btnRemoveHelp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            if (DevExpress.XtraEditors.XtraMessageBox.Show("Do you want to DELETE this Entire Help Module now?", "Delete Current Help Module Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;
			DataRow dr = gridView1.GetFocusedDataRow();
            int rowHandle = gridView1.FocusedRowHandle;
            string record = dr["record"].ToString();

            DataTable dx = get_help_data("Select * from `help` where `backrecord` = '" + record + "';");
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                string titlerecord = dx.Rows[i]["record"].ObjToString();
                string filename = dx.Rows[i]["filename"].ObjToString();
                RemoveRtbFile(filename);
            }

            string cmd = "DELETE from `help` where `record` = '" + record + "';";
            get_help_data(cmd);

            cmd = "DELETE from `help` where `backrecord` = '" + record + "';";
            get_help_data(cmd);

            dt.Rows.RemoveAt(rowHandle);
            dt.AcceptChanges();
            for (int i = 0; i < dt.Rows.Count; i++)
                dr["num"] = (i + 1).ToString();
            dgv.DataSource = dt;
            if (rowHandle > (dt.Rows.Count-1) )
            {
                gridView1.FocusedRowHandle = rowHandle - 1;
                gridView1.RefreshData();
                dgv.Refresh();
            }
        }
/***************************************************************************************/
        private void MoveRowUp(DataTable dt, int row)
        {
            dt.Columns.Add( "Count", Type.GetType ( "System.Int32" ));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row - 1).ToString();
            string record = dt.Rows[row]["record"].ObjToString();
            G1.update_db_table("help", "record", record, new string[] { "sequence", (row-1).ToString() });

            dt.Rows[row - 1]["Count"] = row.ToString();
            record = dt.Rows[row-1]["record"].ObjToString();
            G1.update_db_table("help", "record", record, new string[] { "sequence", (row).ToString() });

            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            NumberDataTable(dt);
        }
/***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add( "Count", Type.GetType ( "System.Int32" ));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();

            dt.Rows[row]["Count"] = (row + 1).ToString();
            string record = dt.Rows[row]["record"].ObjToString();
            G1.update_db_table("help", "record", record, new string[] { "sequence", (row+1).ToString() });

            dt.Rows[row + 1]["Count"] = row.ToString();
            record = dt.Rows[row+1]["record"].ObjToString();
            G1.update_db_table("help", "record", record, new string[] { "sequence", (row).ToString() });

            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            NumberDataTable(dt);
        }
/***********************************************************************************************/
        private void btnUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv6.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView3.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridView3.FocusedRowHandle;
            if (rowHandle == 0 )
                return; // Already at the first row
            dgv6.DataSource = null;
            MoveRowUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv6.DataSource = dt;
            gridView3.FocusedRowHandle = rowHandle - 1;
            gridView3.UnselectRow(0);
            gridView3.SelectRow(rowHandle - 1);
        }
/***********************************************************************************************/
        private void btnDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv6.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView3.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridView3.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count-1) )
                return; // Already at the last row
            dgv6.DataSource = null;
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv6.DataSource = dt;
            gridView3.FocusedRowHandle = rowHandle + 1;
            gridView3.UnselectRow(0);
            gridView3.SelectRow(rowHandle + 1);
        }
/***********************************************************************************************/
        private void btnHelpUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView1.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridView1.FocusedRowHandle;
            if (rowHandle == 0 )
                return; // Already at the first row
            dgv.DataSource = null;
            MoveRowUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridView1.FocusedRowHandle = rowHandle - 1;
            gridView1.UnselectRow(0);
            gridView1.SelectRow(rowHandle - 1);
        }
/***********************************************************************************************/
        private void btnHelpDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView1.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridView1.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count-1) )
                return; // Already at the last row
            dgv.DataSource = null;
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridView1.FocusedRowHandle = rowHandle + 1;
            gridView1.UnselectRow(0);
            gridView1.SelectRow(rowHandle + 1);
        }
/***********************************************************************************************/
        private void gridView3_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
			DataRow dr = gridView3.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridView3.FocusedRowHandle;
            string record = dr["record"].ToString();
            string module = dr["module"].ToString();
            string filename = dr["filename"].ObjToString();
            lastWho = dr["assigned"].ObjToString();
            lastTitle = dr["title"].ObjToString();
            lastRecord = record;
            lastFile = filename;
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("help", "record", record, new string[] { "title", lastTitle, "assigned", lastWho });
        }
/***********************************************************************************************/
        private void gridView3_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            LoadCurrentFile();
        }
/***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
			DataRow dd = gridView1.GetFocusedDataRow();
            string module = dd["module"].ObjToString();
            string record = dd["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;

            DataTable dt = (DataTable)(dgv6.DataSource);
            DataRow dr = dt.NewRow();
            string title = "New Title " + dt.Rows.Count.ToString();
            string newrecord = G1.create_record("help", "system", "-1");
            if (!String.IsNullOrWhiteSpace ( newrecord ) )
            {
                string filepath = G1.spreadpath + "/help";
                G1.verify_path(filepath);
                string filename = Path.GetRandomFileName();
                G1.update_db_table ( "help", "record", newrecord, new string[] { "module", module, "type", "file", "sequence", dt.Rows.Count.ToString(), "backRecord", record, "title", title, "filename", filename } );
                dr["record"] = newrecord;
                dr["module"] = module;
                dr["title"] = title;
                dr["sequence"] = dt.Rows.Count;
                dr["backRecord"] = record;
                dr["filename"] = filename;
                dt.Rows.Add(dr);
                dt.AcceptChanges();
                dgv6.DataSource = dt;
                rtb1.Clear();
                rtb1.Modified = false;
                rtb2.RichTextBox.Clear();
                rtb2.RichTextBox.Modified = false;
                NumberDataTable(dt);
            }
        }
/***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("Do you want to DELETE this Help File now?", "Delete Current Help File Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;
            DataTable dt = (DataTable)(dgv6.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView3.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string filename = dr["filename"].ObjToString();
                if (!String.IsNullOrWhiteSpace(filename))
                {
                    string fullpath = G1.spreadpath + "/help/" + filename;
                    if (File.Exists(fullpath))
                        File.Delete(fullpath);
                }
                string cmd = "DELETE from `help` where `record` = '" + record + "';";
                get_help_data(cmd);
            }
            int rowHandle = gridView3.FocusedRowHandle;
            dt.Rows.RemoveAt(rowHandle);
            dt.AcceptChanges();
            NumberDataTable(dt);
            dgv6.DataSource = dt;
            if (rowHandle > (dt.Rows.Count-1) )
            {
                gridView3.FocusedRowHandle = rowHandle - 1;
                gridView3.RefreshData();
                dgv6.Refresh();
            }
        }
/***********************************************************************************************/
        private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            LoadCurrentHelp();
        }
/***********************************************************************************************/
        private void gridView1_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridView1.GetFocusedDataRow();
            if (dr == null)
                return;
            string module = dr["module"].ObjToString();
            string record = dr["record"].ObjToString();
            string who    = dr["assigned"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("help", "record", record, new string[] { "module", module, "assigned", who });
        }
/***********************************************************************************************/
        private void gridView2_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            DataRow dr = gridView2.GetFocusedDataRow();
            if (dr == null)
                return;
            string system = dr["system"].ObjToString();
            string p = dr["p"].ObjToString();
            if (ValidatePassword(p))
            {
                if (!String.IsNullOrWhiteSpace(system))
                {
                    SystemHelp = system;
                    LoadHelp();
                    LoadCurrentHelp();
                }
            }
            else
            { // Can't Access This System
                SystemHelp = system;
                LoadHelp( false );
                LoadCurrentHelp();
            }
        }
/***********************************************************************************************/
        private void gridView2_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridView2.GetFocusedDataRow();
            if (dr == null)
                return;
            string system = dr["system"].ObjToString();
            string record = dr["record"].ObjToString();
            string who    = dr["assigned"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("help", "record", record, new string[] { "system", system, "assigned", who });
        }
/***********************************************************************************************/
        private void btnAddSystem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv2.DataSource);
            DataRow dr = dt.NewRow();
            string system = "New System " + dt.Rows.Count.ToString();
            string record = G1.create_record("help", "system", "-1");
            if (record != "")
            {
                G1.update_db_table("help", "record", record, new string[] { "system", system, "type", "system", "sequence", dt.Rows.Count.ToString() });
                dr["record"] = record;
                dr["system"] = system;
                dr["module"] = system;
                dr["sequence"] = dt.Rows.Count;
                dt.Rows.Add(dr);
                dt.AcceptChanges();
                dgv2.DataSource = dt;
                try
                {
                    dr["num"] = dt.Rows.Count.ToString();
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void btnAdd_X(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv2.DataSource);
            DataRow dr = dt.NewRow();
            string system =  "New System " + dt.Rows.Count.ToString();
            string record = G1.create_record("help", "system", "-1");
            if (record != "")
            {
                G1.update_db_table ( "help", "record", record, new string[] { "system", system, "type", "system", "sequence", dt.Rows.Count.ToString() } );
                dr["record"] = record;
                dr["system"] = system;
                dr["module"] = system;
                dr["sequence"] = dt.Rows.Count;
                dt.Rows.Add(dr);
                dt.AcceptChanges();
                dgv2.DataSource = dt;
                try
                {
                    dr["num"] = dt.Rows.Count.ToString();
                }
                catch
                {
                }
            }
        }
/***********************************************************************************************/
        private void btnDeleteSystem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv2.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            if (DevExpress.XtraEditors.XtraMessageBox.Show("Do you want to DELETE this Entire Help System now?", "Delete Current Help System Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;
			DataRow dr = gridView2.GetFocusedDataRow();
            int rowHandle = gridView2.FocusedRowHandle;
            string record = dr["record"].ToString();
            DataTable dx = get_help_data ( "SELECT * from `help` where `backrecord` = '" + record + "';" );

            for ( int i=0; i<dx.Rows.Count; i++ )
            {
                string modulerecord = dx.Rows[i]["record"].ObjToString();
                DataTable dxx = get_help_data ( "SELECT * from `help` where `backrecord` = '" + modulerecord + "';" );
                for ( int j=0; j<dxx.Rows.Count; j++ )
                {
                    string titlerecord = dxx.Rows[j]["record"].ObjToString();
                    string filename = dxx.Rows[j]["filename"].ObjToString();
                    get_help_data ( "DELETE from `help` where `record` = '" + titlerecord + "';" );
                    RemoveRtbFile(filename);
                }
            }
            get_help_data("DELETE from `help` where `backrecord` = '" + record + "';"); // Delete All Module Records
            get_help_data("DELETE from `help` where `record` = '" + record + "';" ); // Delete Actual System Record

            dt.Rows.RemoveAt(rowHandle);
            dt.AcceptChanges();
            NumberDataTable(dt);
            dgv2.DataSource = dt;
            if (rowHandle > (dt.Rows.Count-1) )
            {
                gridView2.FocusedRowHandle = rowHandle - 1;
                gridView2.RefreshData();
                dgv2.Refresh();
            }
        }
/***********************************************************************************************/
        private void btnSystemUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv2.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView2.GetFocusedDataRow();
            int rowHandle = gridView2.FocusedRowHandle;
            if (rowHandle == 0 )
                return; // Already at the first row
            dgv2.DataSource = null;
            MoveRowUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv2.DataSource = dt;
            gridView2.FocusedRowHandle = rowHandle - 1;
            gridView2.UnselectRow(0);
            gridView2.SelectRow ( rowHandle - 1 );
        }
/***********************************************************************************************/
        private void btnSystemDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv2.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView2.GetFocusedDataRow();
            int rowHandle = gridView2.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count-1) )
                return; // Already at the last row
            dgv2.DataSource = null;
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv2.DataSource = dt;
            gridView2.FocusedRowHandle = rowHandle + 1;
            gridView2.UnselectRow(0);
            gridView2.SelectRow ( rowHandle + 1 );
        }
/***********************************************************************************************/
        private void OpenZipFile( string fullpath )
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(fullpath + ".zip"))
                {
                    try
                    {
                        zip.Password = GetEncriptedWord();
                        zip.ExtractAll(fullpath);
                    }
                    catch( Exception ex )
                    {
                        MessageBox.Show("***ERROR*** Opening Zip File " + ex.Message.ToString());
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Reading Zip File " + ex.Message.ToString());
            }
        }
/***********************************************************************************************/
        private string GetEncriptedWord( string word = "" )
        {
            if (String.IsNullOrWhiteSpace(word))
                word = "xyzzy";
            string encryptedString = EncryptStringSample.StringCipher.Encrypt(word, "GSYAHAGCBDUUADIADKOPAAAW");
            return encryptedString;
        }
/***********************************************************************************************/
        private string GetDecriptedWord( string word )
        {
            if (String.IsNullOrWhiteSpace(word))
                return "";
            string decryptedString = EncryptStringSample.StringCipher.Decrypt(word, "GSYAHAGCBDUUADIADKOPAAAW");
            return decryptedString;
        }
/***********************************************************************************************/
        private void setPasswordToolStripMenuItem_Click(object sender, EventArgs e)
        { // Set Password for a Title
            DataTable dt = (DataTable)(dgv6.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridView3.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string p = dr["p"].ObjToString();
            if (!String.IsNullOrWhiteSpace(p))
            {
                MessageBox.Show("***ERROR*** File already has a password!");
                return;
            }
            string pas = SetUserPassword(record);
            if (!String.IsNullOrWhiteSpace(pas))
            {
                dr["p"] = pas;
                dt.AcceptChanges();
            }
        }
/***********************************************************************************************/
        private void clearPasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv6.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridView3.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string p = dr["p"].ObjToString();
            if (String.IsNullOrWhiteSpace(p))
            {
                MessageBox.Show("***ERROR*** File does not have a password!");
                return;
            }
            if (ValidatePassword(p))
            {
                G1.update_db_table("help", "record", record, new string[] { "p", "" });
                dr["p"] = "";
                dt.AcceptChanges();
            }
        }
/***********************************************************************************************/
        private void setPasswordToolStripMenuItem1_Click(object sender, EventArgs e)
        { // Set Password for a Module
            DataTable dt = (DataTable)(dgv.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridView1.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string p = dr["p"].ObjToString();
            if (!String.IsNullOrWhiteSpace(p))
            {
                MessageBox.Show("***ERROR*** File already has a password!");
                return;
            }
            string pas = SetUserPassword(record);
            if (!String.IsNullOrWhiteSpace(pas))
            {
                dr["p"] = pas;
                dt.AcceptChanges();
            }
        }
/***********************************************************************************************/
        private void clearPasswordToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridView1.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string p = dr["p"].ObjToString();
            if (String.IsNullOrWhiteSpace(p))
            {
                MessageBox.Show("***ERROR*** File does not have a password!");
                return;
            }
            if (ValidatePassword(p))
            {
                G1.update_db_table("help", "record", record, new string[] { "p", "" });
                dr["p"] = "";
                dt.AcceptChanges();
            }
        }
/***********************************************************************************************/
        private void setPasswordToolStripMenuItem2_Click(object sender, EventArgs e)
        { // Set Password for a System
            DataTable dt = (DataTable)(dgv2.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridView2.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string p = dr["p"].ObjToString();
            if (!String.IsNullOrWhiteSpace(p))
            {
                MessageBox.Show("***ERROR*** File already has a password!");
                return;
            }
            string pas = SetUserPassword(record);
            if (!String.IsNullOrWhiteSpace(pas))
            {
                dr["p"] = pas;
                dt.AcceptChanges();
            }
        }
/***********************************************************************************************/
        private void clearPasswordToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)(dgv2.DataSource);
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridView2.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string p = dr["p"].ObjToString();
            if (String.IsNullOrWhiteSpace(p))
            {
                MessageBox.Show("***ERROR*** File does not have a password!");
                return;
            }
            if (ValidatePassword(p))
            {
                G1.update_db_table("help", "record", record, new string[] { "p", "" });
                dr["p"] = "";
                dt.AcceptChanges();
            }
        }
/***********************************************************************************************/
        private string SetUserPassword(string record)
        {
            if (!String.IsNullOrWhiteSpace(record))
            {
                using (Ask fmrmyform = new Ask("Enter Password > "))
                {
                    fmrmyform.Text = "";
                    fmrmyform.ShowDialog();
                    string p = fmrmyform.Answer.Trim().ToUpper();
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        if (DevExpress.XtraEditors.XtraMessageBox.Show("Are you sure you want to add a password to this entry?\nYou must remember this because it is not reversible!!!", "Help Password Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            string pas = GetEncriptedWord(p);
                            G1.update_db_table ( "help", "record", record, new string[] { "p", pas } );
                            return pas;
                        }
                    }
                }
            }
            return "";
        }
/***********************************************************************************************/
        private void menuExit_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Close();
        }
/***********************************************************************************************/
        private int LineLength = 80;
        private string CreateLine()
        {
            string str = "";
            str = str.PadRight(LineLength);
//            string str = rptr.blank_fill("", LineLength);
            return str;
        }
/***********************************************************************************************/
        private string CopyString(string target, int position, string source)
        {
            string result = target.Substring(0, position);
            result += source;
            int remaining = target.Length - position - source.Length;
            if (remaining > 0)
                result += target.Substring(position + source.Length, remaining);
            return result;
        }
/***********************************************************************************************/
        private void SetCenter()
        {
            rtbPrint.RichTextBox.SelectionAlignment = HorizontalAlignment.Center;
        }
/***********************************************************************************************/
        private void SetLeft()
        {
            rtbPrint.RichTextBox.SelectionAlignment = HorizontalAlignment.Left;
        }
/***********************************************************************************************/
        private void CenterString(ref string str, string data)
        {
            int len = data.Length;
            int pos = (LineLength / 2) - (len / 2);
            str = CopyString(str, pos, data);
        }
/***********************************************************************************************/
        private void AddLineToRTB(string line, string font, float size = 9F )
        {
            if ( String.IsNullOrWhiteSpace ( font ) )
                G1.Toggle_Font(rtbPrint.RichTextBox, "Times New Roman", size);
            else
                G1.Toggle_Font(rtbPrint.RichTextBox, font, size);
            rtbPrint.RichTextBox.AppendText(line);
        }
/***********************************************************************************************/
        private void AddLineToRTB(string line, float size = 9F )
        {
            G1.Toggle_Font(rtbPrint.RichTextBox, "Times New Roman", size);
            rtbPrint.RichTextBox.AppendText(line);
        }
/***********************************************************************************************/
        private void BuildPrintReport( bool clear = true )
        {
            if ( clear )
                rtbPrint.RichTextBox.Clear();
            int[] rows = gridView1.GetSelectedRows();
            EMRRichTextBox rich = new EMRRichTextBox();
            int page = 1;
            string str = txtStartPage.Text;
            if (G1.validate_numeric(str))
                page = str.ObjToInt32();
            for (int i = 0; i < rows.Length; i++)
            {
                DataRow dr = gridView1.GetDataRow(rows[i]);
                string module = dr["module"].ObjToString();
                string record = dr["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    continue;
                DataTable dt = get_help_data("Select * from `help` where `backrecord` = '" + record + "' order by `sequence`;");
                int[] pages = new int[dt.Rows.Count];
                int totalPages = 0;
                for ( int j=0; j<dt.Rows.Count; j++ )
                {
                    string title = dt.Rows[j]["title"].ObjToString();
                    string filename = dt.Rows[j]["filename"].ObjToString();
                    RichTextBox rtb = ReadStraightRtbFile(filename);
                    rich.RichTextBox.Clear();
                    rich.RichTextBox.AppendRtf(rtb.Rtf);
                    int numberOfPages = rich.RichTextBox.Count(rich.RichTextBox.TextLength);
                    totalPages += numberOfPages;
                    rtb.Clear();
                    rtb.Dispose();
                    rtb = null;
                    pages[j] = numberOfPages;
                }

                SetCenter();
                AddLineToRTB(module + "\n", 14F);
                AddLineToRTB("\n\n");
                SetLeft();
                int count = 0;
                page++;

                for ( int j=0; j<dt.Rows.Count; j++ )
                {
                    string title = dt.Rows[j]["title"].ObjToString();
                    count++;
                    title = "  " + count + ") " + title;
                    int len = title.Length;
                    int fill = 70 - len;
                    title = title + new String('.', fill);
//                    title = rptr.fill_string(title, ".", fill);
                    title += page.ToString();
                    AddLineToRTB(title + "\n", "Lucida Console", 10F);
                    page += pages[j];
                }
                AddLineToRTB("\f");
                for ( int j=0; j<dt.Rows.Count; j++ )
                {
                    string title = dt.Rows[j]["title"].ObjToString();
                    string filename = dt.Rows[j]["filename"].ObjToString();
                    RichTextBox rtb = ReadStraightRtbFile(filename);
                    rtbPrint.RichTextBox.AppendRtf(rtb.Rtf);
                    AddLineToRTB("\f");
                    rtb.Clear();
                    rtb.Dispose();
                    rtb = null;
                }
            }
        }
/***********************************************************************************************/
        private void btnPrint_ItemClick(object sender, ItemClickEventArgs e)
        {
        }
/***********************************************************************************************/
		private EMRRichTextBox rtbPrint = new EMRRichTextBox();
		private int checkPrint;
		private int pageNumber;
/***********************************************************************************************/
        private void printDocument1_BeginPrint(object sender, PrintEventArgs e)
        {
			pageNumber = 1;
            string str = txtStartPage.Text;
            if (G1.validate_numeric(str))
                pageNumber = str.ObjToInt32();
			checkPrint = 0;
        }
/***********************************************************************************************/
        private void btnPrintPreview_ItemClick(object sender, ItemClickEventArgs e)
        {
            DataTable dt = (DataTable)(dgv2.DataSource);
            if (dt.Rows.Count <= 0)
                return;
			DataRow dr = gridView2.GetFocusedDataRow();
            int rowHandle = gridView2.FocusedRowHandle;
            if (rowHandle < 0)
                return;
            string system = dr["system"].ObjToString();
            rtbPrint.RichTextBox.Clear();
            AddLineToRTB("\n\n");
            SetCenter();
            AddLineToRTB(system + "\n", 12F);
            AddLineToRTB("\n\n");
            BuildPrintReport( false );
			printPreviewDialog1.ShowDialog();
        }
/***********************************************************************************************/
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.PageSettings.Margins.Bottom = 10;
			checkPrint = rtbPrint.RichTextBox.Print(checkPrint, rtbPrint.RichTextBox.TextLength, e);

            string page = "Page - " + pageNumber.ToString();

            int x = (e.PageBounds.Width / 2) - 30;
            Point drawPoint = new Point(x, e.PageBounds.Height - 55);
            var topFont = new Font("Times New Roman", 12);
            e.Graphics.DrawString(page, topFont, Brushes.Black, drawPoint);

            if (checkPrint < rtbPrint.RichTextBox.TextLength)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;
            pageNumber++;
        }
/***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildPrintReport();
			printPreviewDialog1.ShowDialog();
        }
/***********************************************************************************************/
        private void printToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            BuildPrintReport();
            using (PrintDialog pd = new PrintDialog())
            {
                pd.Document = printDocument1;
                if (pd.ShowDialog() == DialogResult.OK)
                    printDocument1.Print();
            }
        }
/***********************************************************************************************/
        private void menuShowOpenFiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            //ShowOpenedFiles showForm = new ShowOpenedFiles();
            //showForm.Show();
        }
/***********************************************************************************************/
        private void btnExpandSystem_Click(object sender, EventArgs e)
        {
            if (gridView2.Columns["assigned"].Visible == true)
                gridView2.Columns["assigned"].Visible = false;
            else
                gridView2.Columns["assigned"].Visible = true;
        }
/***********************************************************************************************/
        private void btnExpandHelp_Click(object sender, EventArgs e)
        {
            if (gridView1.Columns["assigned"].Visible == true)
                gridView1.Columns["assigned"].Visible = false;
            else
                gridView1.Columns["assigned"].Visible = true;
        }
/***********************************************************************************************/
        private void btnExpand_Click(object sender, EventArgs e)
        {
            if (gridView3.Columns["assigned"].Visible == true)
                gridView3.Columns["assigned"].Visible = false;
            else
                gridView3.Columns["assigned"].Visible = true;
        }
        /***********************************************************************************************/
    }
}
/***********************************************************************************************/
namespace EncryptStringSample
{
/***********************************************************************************************/
    public static class StringCipher
    {
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }
/***********************************************************************************************/
        public static string Decrypt(string cipherText, string passPhrase)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            using(PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using(RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using(ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                    {
                        using(MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using(CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }
/***********************************************************************************************/
    }
/***********************************************************************************************/
}
