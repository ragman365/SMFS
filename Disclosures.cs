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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using iTextSharp.text.pdf;
using System.IO;
//using iTextSharp.text;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Disclosures : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string _answer = "";
        private string workContract = "";
        private string workFuneralRecord = "";
        private bool loading = true;
        public string Answer { get { return _answer; } }
        /***********************************************************************************************/
        public Disclosures ( string contractNumber )
        {
            InitializeComponent();
            workContract = contractNumber;
        }
        /***********************************************************************************************/
        private void Disclosures_Load(object sender, EventArgs e)
        {
            _answer = "";
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            btnSave.Hide();
            this.Text = "Select Deposit Bank Accounts for ";
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `bank_accounts` where `show_dropdown` = '1';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("cashLocal");
            dt.Columns.Add("checkLocal");
            dt.Columns.Add("checkRemote");
            dt.Columns.Add("ccAccount");
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            SetupSelection(dt, repositoryItemCheckEdit1, "cashLocal");
            SetupSelection(dt, repositoryItemCheckEdit2, "checkLocal");
            SetupSelection(dt, repositoryItemCheckEdit3, "checkRemote");
            SetupSelection(dt, repositoryItemCheckEdit4, "ccAccount");
            LoadBankAccounts(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadBankAccounts ( DataTable dt )
        {
            string cmd = "Select * from `funeralhomes` where `record` = '" + workFuneralRecord + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string str = "";
            string cashLocal = dx.Rows[0]["cashLocal"].ObjToString();
            string checkLocal = dx.Rows[0]["checkLocal"].ObjToString();
            string checkRemote = dx.Rows[0]["checkRemote"].ObjToString();
            string ccAccount = dx.Rows[0]["ccAccount"].ObjToString();
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["account_no"].ObjToString();
                if (str == cashLocal)
                    dt.Rows[i]["cashLocal"] = "1";
                if (str == checkLocal)
                    dt.Rows[i]["checkLocal"] = "1";
                if (str == checkRemote)
                    dt.Rows[i]["checkRemote"] = "1";
                if (str == ccAccount)
                    dt.Rows[i]["ccAccount"] = "1";
            }
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew, string column )
        {
            bool saveLoad = loading;
            loading = true;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i][column] = "0";
            loading = saveLoad;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void EditBankAccounts_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSave.Visible)
                return;
            DialogResult result = MessageBox.Show("***Question***\nBank Accounts have been selected!\nWould you like to save your changes?", "Select Banks Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            btnSave.Visible = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Show();
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            SetupSelection(dt, repositoryItemCheckEdit1, "cashLocal");
            dr["cashLocal"] = "1";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Show();
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            SetupSelection(dt, repositoryItemCheckEdit1, "checkLocal");
            dr["checkLocal"] = "1";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit3_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Show();
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            SetupSelection(dt, repositoryItemCheckEdit1, "checkRemote");
            dr["checkRemote"] = "1";
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            btnSave.Show();
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            SetupSelection(dt, repositoryItemCheckEdit1, "ccAccount");
            dr["ccAccount"] = "1";
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            btnSave.Visible = false;
            string cashLocalBankAccount = "";
            string checkLocalBankAccount = "";
            string checkRemoteBankAccount = "";
            string ccAccountBankAccount = "";
            string str = "";
            DataTable dt = (DataTable)dgv.DataSource;
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    try
            //    {
            //        str = dt.Rows[i]["cashLocal"].ObjToString();
            //        if (str == "1")
            //            cashLocalBankAccount = dt.Rows[i]["account_no"].ObjToString();

            //        str = dt.Rows[i]["checkLocal"].ObjToString();
            //        if (str == "1")
            //            checkLocalBankAccount = dt.Rows[i]["account_no"].ObjToString();

            //        str = dt.Rows[i]["checkRemote"].ObjToString();
            //        if (str == "1")
            //            checkRemoteBankAccount = dt.Rows[i]["account_no"].ObjToString();

            //        str = dt.Rows[i]["ccAccount"].ObjToString();
            //        if (str == "1")
            //            ccAccountBankAccount = dt.Rows[i]["account_no"].ObjToString();
            //    }
            //    catch ( Exception ex)
            //    {
            //    }
            //}
            //G1.update_db_table("funeralhomes", "record", workFuneralRecord, new string[] { "cashLocal", cashLocalBankAccount, "checkLocal", checkLocalBankAccount, "checkRemote", checkRemoteBankAccount, "ccAccount", ccAccountBankAccount });
        }
        /***********************************************************************************************/
    }
}