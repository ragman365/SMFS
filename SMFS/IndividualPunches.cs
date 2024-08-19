using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using System.Linq;
using System.Diagnostics;
using System.IO;
using DevExpress.XtraGrid;
using DevExpress.XtraPrinting;
using System.Data.OleDb;
using GeneralLib;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;

using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Collections;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DevExpress.Utils;

using MySql.Data.MySqlClient;
using System.Configuration;
using System.Threading;
using MySql.Data.Types;

using System.Net;
using System.Net.Sockets;
using System.IO.Compression;

using System.Windows.Forms.VisualStyles;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class IndividualPunches : Form
    {
        private bool modified = false;
        private DataTable work_dt = null;
        private string work_empno = "";
        private string work_date = "";
        private string work_name = "";
        private bool is_supervisor = false;
        /***********************************************************************************************/
        public IndividualPunches( string empno, string name, string date, bool supervisor = false )
        {
            InitializeComponent();
            work_empno = empno;
            work_date = date;
            work_name = name;
            is_supervisor = supervisor;
        }
        /***********************************************************************************************/
        private void IndividualPunches_Load(object sender, EventArgs e)
        {
            //if (!is_supervisor)
            //{
            //    btnAdd.Hide();
            //    btnDelete.Hide();
            //    gridView.Columns["user"].Visible = false;
            //    gridView.Columns["computer"].Visible = false;
            //    contextMenuStrip1.Enabled = false;
            //}

            this.Text = "Edit Punches for (" + work_empno + ") " + work_name + " for " + work_date;

            DateTime date = work_date.ObjToDateTime();
            //long ldate = date.ToUnix();
            //long edate = date.AddDays(1).ToUnix();
            long ldate = G1.TimeToUnix(date);
            long edate = G1.TimeToUnix(date.AddDays(1));
            string cmd = "Select * from `tc_punches_pchs` where `UTS_Added` >= '" + ldate + "' and `UTS_Added` <= '" + edate + "' ";
            cmd += " and `empy!AccountingID` = '" + work_empno + "' ";
            cmd += "order by `empy!AccountingID`,`UTS_Added`;";

            DataTable dx = G1.get_db_data(cmd);
            //DataTable dx = new DataTable();
            //dx.Columns.Add("date");
            dx.Columns.Add("time");
            dx.Columns.Add("mod");
            dx.Columns.Add("status");

            try
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    ldate = dx.Rows[i]["UTS_Added"].ObjToInt64();
                    date = ldate.UnixToDateTime();
                    string[] Lines = date.ToString().Split(' ');
                    if (Lines.Length < 3)
                        continue;
                    string date1 = Lines[0].Trim();
                    string time1 = Lines[1].Trim() + " " + Lines[2].Trim();
                    dx.Rows[i]["date"] = date1;
                    dx.Rows[i]["time"] = time1;
                    bool deleted = dx.Rows[i]["Deleted"].ObjToBool();
                    bool staged = dx.Rows[i]["Staged"].ObjToBool();
                    bool manualentry = dx.Rows[i]["ManualEntry"].ObjToBool();
                    if (deleted)
                        dx.Rows[i]["status"] = "Delete";
                    else if (manualentry)
                        dx.Rows[i]["status"] = "Entered";
                    else if (staged)
                        dx.Rows[i]["status"] = "Staged";
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR " + ex.Message.ToString());
            }

            work_dt = dx;
            dgv.DataSource = dx;
        }
        /***********************************************************************************************/
        private void deletePunchsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int [] selectedRows = gridView.GetSelectedRows();
            for ( int i=0; i<selectedRows.Length; i++ )
            {
                int row = selectedRows[i];
                work_dt.Rows[row]["status"] = "Delete";
                work_dt.Rows[row]["mod"] = "Y";
            }
            dgv.DataSource = work_dt;
            modified = true;
        }
        /***********************************************************************************************/
        private void unDeletePunchsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] selectedRows = gridView.GetSelectedRows();
            for (int i = 0; i < selectedRows.Length; i++)
            {
                int row = selectedRows[i];
                work_dt.Rows[row]["status"] = "";
                work_dt.Rows[row]["mod"] = "Y";
            }
            dgv.DataSource = work_dt;
            modified = true;
        }
        /***********************************************************************************************/
        private void gridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            DataTable dt = (DataTable)(dgv.DataSource);
            int row = e.RowHandle;
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                e.DisplayText = (row + 1).ToString();
            }
        }
        /***********************************************************************************************/
        private bool CheckModified ()
        {
            if (modified)
                return true;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return false;
            for ( int i=0; i<dt.Rows.Count; i++ )
            {
                string mod = dt.Rows[i]["mod"].ObjToString();
                if ( !string.IsNullOrWhiteSpace ( mod ))
                {
                    modified = true;
                    break;
                }
            }
            return modified;
        }
        /***********************************************************************************************/
        private void IndividualPunches_FormClosing(object sender, FormClosingEventArgs e)
        {
            modified = CheckModified();

            if (modified)
            {
                DialogResult result;
                string question = "Changes appear to have been made! ";
                question += "Do you want to SAVE these changes now?\n";
                result = DevExpress.XtraEditors.XtraMessageBox.Show(question, "Save Changes Dialog",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel)
                    e.Cancel = true;
                else if (result == DialogResult.Yes)
                    SaveData();
                OnDone();
            }
        }
        /***********************************************************************************************/
        private void SaveData()
        {
            string machine = System.Environment.MachineName.ObjToString();

            string cmd = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string status = dt.Rows[i]["status"].ObjToString().ToUpper();
                string mod = dt.Rows[i]["mod"].ObjToString();
                if (status == "DELETE")
                    mod = "Y";
                if (!string.IsNullOrWhiteSpace(mod))
                {
                    long ldate = dt.Rows[i]["UTS_Added"].ObjToInt64();
                    if (status == "DELETE")
                    {
                        cmd = "UPDATE `tc_punches_pchs` Set `Deleted` = '1', `user` = '" + LoginForm.username + "', `computer` = '" + machine + "' where `empy!AccountingID` = '" + work_empno + "' and `UTS_Added` = '" + ldate.ToString() + "';";
                    }
                    else if (string.IsNullOrWhiteSpace(status))
                    {
                        cmd = "UPDATE `tc_punches_pchs` Set `Deleted` = '0', `user` = '" + LoginForm.username + "', `computer` = '" + machine + "' where `empy!AccountingID` = '" + work_empno + "' and `UTS_Added` = '" + ldate.ToString() + "';";
                    }
                    else if (status == "ADD")
                    {
                        bool alreadyExists = VerifyPunchEntry(ldate, work_empno);
                        if (alreadyExists)
                        {
                            MessageBox.Show("***ERROR*** Added Punch Time for Employee Already Exists! Add or Subtract a second! PUNCH NOT ADDED!");
                            continue;
                        }
                        cmd = "INSERT INTO `tc_punches_pchs` (`UTS_Added`, `empy!AccountingID`, `ManualEntry`, `user`, `computer`) VALUES ('" + ldate.ToString() + "', '" + work_empno + "', '1', '" + LoginForm.username + "', '" + machine + "' );";
                    }
                    try
                    {
                        G1.update_db_data(cmd);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
            }
        }
        /***********************************************************************************************/
        public static bool VerifyPunchEntry ( long punchTime, string empno )
        {
            bool alreadyExists = false;
            //if (1 == 1)
            //    return false;
            string cmd = "SELECT * FROM `tc_punches_pchs` WHERE `UTS_Added` = '" + punchTime.ToString() + "' and `empy!AccountingID` = '" + empno + "';";
            try
            {
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    alreadyExists = true;
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            return alreadyExists;
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridView.GetFocusedDataRow();
            dr["mod"] = "Y";
            dr["status"] = "Delete";
        }
        /***********************************************************************************************/
        private long ValidateTime ( string time )
        {
            string[] Lines = null;
            int idx = time.IndexOf(":");
            if ( idx > 0 )
                Lines = time.Trim().Split(':');
            else if ( time.IndexOf ( "." ) > 0 )
                Lines = time.Trim().Split('.');
            else
            {
                MessageBox.Show("***ERROR*** Time is not in proper format!");

            }
            if (Lines.Length <= 1)
                return -1L;
            int hours = Lines[0].ObjToInt32();
            if (hours < 0 || hours > 23)
                return -1L;
            int minutes = Lines[1].ObjToInt32();
            if (minutes < 0 || minutes > 59)
                return -1L;
            int seconds = 0;
            if ( Lines.Length >= 3 )
            {
                seconds = Lines[2].ObjToInt32();
                if (seconds < 0 || seconds > 59)
                    return -1L;
            }
            DateTime date = work_date.ObjToDateTime();
            date = date.AddHours(hours);
            date = date.AddMinutes(minutes);
            date = date.AddSeconds(seconds);

            DateTime myTime = date;

            long ldate = G1.TimeToUnix(myTime);
            date = ldate.UnixToDateTime();

            return ldate;
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string newTime = "";
            using (Ask fmrmyform = new Ask("Enter New Time for Punch (Ex: 13:50:20) > "))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                newTime = fmrmyform.Answer.Trim().ToUpper();
            }
            long ldate = ValidateTime(newTime);
            if ( ldate > 0L )
            {
                bool alreadyExists = VerifyPunchEntry(ldate, work_empno);
                if (alreadyExists)
                {
                    MessageBox.Show("***ERROR*** Added Punch Time for Employee Already Exists! Add or Subtract a second! PUNCH NOT ADDED!");
                    return;
                }
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dRow = dt.NewRow();
                DateTime date = ldate.UnixToDateTime();
                string[] Lines = date.ToString().Split(' ');
                if (Lines.Length < 3)
                    return;
                string date1 = Lines[0].Trim();
                string time1 = Lines[1].Trim() + " " + Lines[2].Trim();
                dRow["date"] = date1;
                dRow["time"] = time1;
                dRow["status"] = "Add";
                dRow["mod"] = "Y";
                //dRow["UTS_Added"] = ldate;
                dt.Rows.Add(dRow);
                dgv.DataSource = dt;
            }
        }
        /***************************************************************************************/
        public delegate void d_EditPunchesDone();
        public event d_EditPunchesDone EditPunchesDone;
        protected void OnDone()
        {
            if (EditPunchesDone != null)
                EditPunchesDone();
        }
        /***********************************************************************************************/
    }
}
