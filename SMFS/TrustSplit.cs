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
    public partial class TrustSplit : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool loading = true;
        private DateTime workDate = DateTime.Now;
        private string workReport = "";
        private string workContract = "";
        private DataTable workDt = null;
        private DataRow workDr = null;
        private string workColumn = "";
        private string workPreOrPost = "";
        /***********************************************************************************************/
        public TrustSplit ( DateTime date, string report, string contractNumber, DataTable dt, DataRow dr, string preOrPost )
        {
            InitializeComponent();
            workDate = date;
            workReport = report;
            workContract = contractNumber;
            workDt = dt;
            workDr = dr;
            workPreOrPost = preOrPost;
        }
        /***********************************************************************************************/
        private void TrustSplit_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            btnSave.Hide();
            this.Text = "Select Deposit Bank Accounts for ";
            this.Cursor = Cursors.WaitCursor;

            string trustCompany = workDr["trust"].ObjToString();
            if (String.IsNullOrWhiteSpace(trustCompany))
            {
                if (workReport == "Post 2002 Report - SN & FT")
                    trustCompany = "SNFT";
                else if (workReport == "Post 2002 Report - Unity")
                    trustCompany = "Unity";
                else if (workReport == "Post 2002 Report - FDLIC")
                    trustCompany = "FDLIC";
                else if (workReport == "Post 2002 Report - CD")
                    trustCompany = "CD";
                else if (workReport == "Pre 2002 Report")
                    trustCompany = "Pre2002";
                else
                    return;
            }


            string cmd = "Select * from `trust_data_edits` WHERE `trustName` = '" + trustCompany + "' AND `status` = 'Line Edit' AND `date` = '" + workDate.ToString("yyyy-MM-dd") + "' AND `preOrPost` = '" + workPreOrPost + "' ";
            cmd += " AND `contractNumber` = '" + workContract + "' ";
            cmd += ";";


            DataTable dx = G1.get_db_data(cmd);

            //dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
            loading = false;
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
            //DialogResult result = MessageBox.Show("***Question***\nBank Accounts have been selected!\nWould you like to save your changes?", "Select Banks Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            //if (result == DialogResult.Cancel)
            //{
            //    e.Cancel = true;
            //    return;
            //}
            //btnSave.Visible = false;
            //if (result == DialogResult.No)
            //    return;
            //btnSave_Click(null, null);
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