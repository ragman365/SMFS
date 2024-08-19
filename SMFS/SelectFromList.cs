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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class SelectFromList : DevExpress.XtraEditors.XtraForm
    {
        private string workSelections = "";
        public static string theseSelections = "";
        private bool workMulti = false;
        private bool workIgnoreCommas = false;
        private bool workDelete = false;
        private bool deleting = false;
        /***********************************************************************************************/
        public SelectFromList( string selections, bool multi = false, bool ignoreCommas = false, bool allowDelete = false )
        {
            InitializeComponent();
            workSelections = selections;
            workMulti = multi;
            theseSelections = "";
            workIgnoreCommas = ignoreCommas;
            workDelete = allowDelete;
        }
        /***********************************************************************************************/
        private void SelectFromList_Load(object sender, EventArgs e)
        {
            if (!workMulti)
                ckSelectAll.Hide();
            if (!workDelete)
                contextMenuStrip1.Dispose();

            string[] Lines = workSelections.Split('\n');
            string[] lines = null;
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("selections");
            dt.Columns.Add("record");
            string str = "";
            for ( int i=0; i<Lines.Length; i++)
            {
                str = Lines[i].Trim();
                if (!String.IsNullOrWhiteSpace(str))
                {
                    if (workIgnoreCommas)
                    {
                        DataRow dR = dt.NewRow();
                        dR["selections"] = str;
                        dt.Rows.Add(dR);
                    }
                    else
                    {
                        lines = str.Split(',');
                        DataRow dR = dt.NewRow();
                        if (lines.Length > 0)
                            dR["selections"] = lines[0];
                        if (lines.Length > 1)
                            dR["record"] = lines[1];
                        dt.Rows.Add(dR);
                    }
                }
            }
            if (workMulti)
                SetupSelection(dt);
            else
                gridMain.Columns["select"].Visible = false;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            if ( G1.get_column_number ( dt, "select") < 0 )
                dt.Columns.Add("select");
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit4;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void btnExit_Click(object sender, EventArgs e)
        {
            OnListDone();
            this.Close();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ListDone;
        protected void OnListDone()
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string selection = dr["selections"].ObjToString();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                if ( !workIgnoreCommas )
                    selection += "," + record;
            }
            if ( workMulti )
            {
                string select = "";
                selection = "";
                DataTable dt = (DataTable)dgv.DataSource;
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "1")
                    {
                        selection += dt.Rows[i]["selections"].ObjToString();
                        record = dt.Rows[i]["record"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(record))
                        {
                            if (!workIgnoreCommas)
                                selection += "," + record;
                        }
                        selection += "\n";
                    }
                    dt.Rows[i]["select"] = "0";
                }
                selection = selection.TrimEnd('\n');
            }
            if (String.IsNullOrWhiteSpace(selection))
                this.Close();
            theseSelections = selection;
            if (ListDone != null)
            {
                if (!string.IsNullOrWhiteSpace(selection))
                {
                    if (deleting)
                        selection += "~Delete";
                    ListDone.Invoke(selection);
                }
            }
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            OnListDone();
            this.Close();
        }
        /***********************************************************************************************/
        private void SelectFromList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!workMulti)
                return;
            bool modified = false;
            string select = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    modified = true;
                    break;
                }
            }
            if ( modified )
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been selected!\nDo you want to exit with the selected data?", "Data Selected Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                {
                    OnListDone();
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string select = dr["select"].ObjToString();
            select = dt.Rows[row]["select"].ObjToString();

            string doit = "0";
            if (select == "0")
                doit = "1";
            dr["select"] = doit;
            dt.Rows[row]["select"] = doit;
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private bool selectedAll = false;
        private void ckSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool whatToDo = false;
            if (!selectedAll)
                whatToDo = true;
            DataTable dx = (DataTable)dgv.DataSource;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                if (whatToDo)
                    dx.Rows[i]["select"] = "1";
                else
                    dx.Rows[i]["select"] = "0";
            }
            selectedAll = whatToDo;

            dgv.RefreshDataSource();
            gridMain.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleting = true;
            OnListDone();
            this.Close();
        }
        /***********************************************************************************************/
    }
}