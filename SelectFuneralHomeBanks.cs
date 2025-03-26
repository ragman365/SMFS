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
    public partial class SelectFuneralHomeBanks : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private string _answer = "";
        private string workFuneralHome = "";
        private string workFuneralRecord = "";
        private bool loading = true;
        private bool workCemetery = false;
        public string Answer { get { return _answer; } }
        /***********************************************************************************************/
        public SelectFuneralHomeBanks( string funeralHome, string record, bool isCemetery = false )
        {
            InitializeComponent();
            workFuneralHome = funeralHome;
            workFuneralRecord = record;
            workCemetery = isCemetery;
        }
        /***********************************************************************************************/
        private void SelectFuneralHomeBanks_Load(object sender, EventArgs e)
        {
            _answer = "";
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            btnSave.Hide();
            this.Text = "Select Deposit Bank Accounts for " + workFuneralHome;
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `bank_accounts` where `show_dropdown` = '1';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("cashLocal");
            dt.Columns.Add("checkLocal");
            dt.Columns.Add("num");

            G1.NumberDataTable(dt);

            SetupSelection(dt, repositoryItemCheckEdit1, "cashLocal");
            SetupSelection(dt, repositoryItemCheckEdit2, "checkLocal");

            LoadBankAccounts(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadBankAccounts ( DataTable dt )
        {
            string cmd = "Select * from `funeralhomes` where `record` = '" + workFuneralRecord + "';";
            if ( workCemetery )
                cmd = "Select * from `cemeteries` where `record` = '" + workFuneralRecord + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string cashLocal = dx.Rows[0]["cashLocal"].ObjToString();
            SetupAccount(dt, "cashLocal", cashLocal );

            string checkLocal = dx.Rows[0]["checkLocal"].ObjToString();
            SetupAccount(dt, "checkLocal", checkLocal);
        }
        /***********************************************************************************************/
        private void SetupAccount ( DataTable dt, string field, string what )
        {
            what = what.TrimEnd('~');
            string [] Lines = what.Split('~');
            string record = "";
            string general_ledger_no = "";
            string account_no = "";
            string[] account = null;
            string str = "";

            for (int j = 0; j < Lines.Length; j++)
            {
                record = Lines[j].Trim();
                if (String.IsNullOrWhiteSpace(record))
                    continue;
                account = record.Split('/');
                if (account.Length < 2)
                    continue;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if ( dt.Rows[i]["general_ledger_no"].ObjToString() == account[0].Trim())
                    {
                        if ( dt.Rows[i]["account_no"].ObjToString() == account[1].Trim())
                            dt.Rows[i][field] = "1";
                    }
                    //str = dt.Rows[i]["record"].ObjToString();
                    //if (str == record)
                    //    dt.Rows[i][field] = "1";
                }
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
            //SetupSelection(dt, repositoryItemCheckEdit1, "cashLocal");
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
            //SetupSelection(dt, repositoryItemCheckEdit2, "checkLocal");
            dr["checkLocal"] = "1";
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            btnSave.Visible = false;
            string cashLocalBankAccount = "";
            string checkLocalBankAccount = "";
            string str = "";
            string general_ledger_no = "";
            string account_no = "";
            string record = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    general_ledger_no = dt.Rows[i]["general_ledger_no"].ObjToString();
                    account_no = dt.Rows[i]["account_no"].ObjToString();

                    record = general_ledger_no + "/" + account_no;

                    str = dt.Rows[i]["cashLocal"].ObjToString();
                    if (str == "1")
                        cashLocalBankAccount += record + "~";

                    str = dt.Rows[i]["checkLocal"].ObjToString();
                    if (str == "1")
                        checkLocalBankAccount += record + "~";
                }
                catch ( Exception ex)
                {
                }
            }
            if ( workCemetery )
                G1.update_db_table("cemeteries", "record", workFuneralRecord, new string[] { "cashLocal", cashLocalBankAccount, "checkLocal", checkLocalBankAccount  });
            else
                G1.update_db_table("funeralhomes", "record", workFuneralRecord, new string[] { "cashLocal", cashLocalBankAccount, "checkLocal", checkLocalBankAccount });
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSave.Visible = true;
        }
        /***********************************************************************************************/
    }
}