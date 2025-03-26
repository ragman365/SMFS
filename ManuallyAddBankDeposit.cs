using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
using Word = Microsoft.Office.Interop.Word;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ManuallyAddBankDeposit : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DateTime workDate;
        private double workCredit = 0D;
        private double workDebit = 0D;
        private string workBankAccount = "";
        private string workDescription = "";

        //private string workService = "";
        //private double workPrice = 0D;
        //private double workCurrentPrice = 0D;
        /***********************************************************************************************/
        public DateTime wDate { get { return workDate; } }
        public string wBankAccount { get { return workBankAccount; } }
        public double wCredit { get { return workCredit; } }
        public double wDebit { get { return workDebit; } }
        public string wDescription { get { return workDescription; } }
        /***********************************************************************************************/
        public ManuallyAddBankDeposit( DateTime date, double credit, double debit, string bankAccount, string description )
        {
            InitializeComponent();
            workDate = date;
            workCredit = credit;
            workDebit = debit;
            workBankAccount = bankAccount;
            workDescription = description;
        }
        /***********************************************************************************************/
        private void ManuallyAddBankDeposit_Load(object sender, EventArgs e)
        {
            txtDate.Text = workDate.ToString("MM/dd/yyyy");

            txtBankAccount.Text = workBankAccount;

            string str = G1.ReformatMoney(workCredit);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtCredit.Text = str;

            str = G1.ReformatMoney(workDebit);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtDebit.Text = str;

            txtDescription.Text = workDescription;

        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string contract);
        public event d_void_eventdone_string SelectDone;
        protected void OnSelectDone( string contract )
        {
            SelectDone?.Invoke(contract);
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            workDate = txtDate.Text.ObjToDateTime();
            workBankAccount = txtBankAccount.Text;
            workCredit = txtCredit.Text.ObjToDouble();
            workDebit = txtDebit.Text.ObjToDouble();
            workDescription = txtDescription.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /***********************************************************************************************/
        private void SomethingChanged ()
        {
            double money = txtCredit.Text.ObjToDouble();
            string str = G1.ReformatMoney(money);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtCredit.Text = str;

            money = txtDebit.Text.ObjToDouble();
            str = G1.ReformatMoney(money);
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            txtDebit.Text = str;
        }
        /***********************************************************************************************/
        private void txtCustomerPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SomethingChanged();
        }
        /***********************************************************************************************/
        private void txtCurrentPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SomethingChanged();
        }
        /***********************************************************************************************/
        private void txtType_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtBankAccount.Focus();
            }
        }
        /***********************************************************************************************/
        private void txtService_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtCredit.Focus();
            }
        }
        /***********************************************************************************************/
        private void txtCustomerPrice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtDebit.Focus();
            }
        }
        /***********************************************************************************************/
        private void txtCurrentPrice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
            }
        }
        /***********************************************************************************************/
        private void txtDate_MouseDown(object sender, MouseEventArgs e)
        {
            DateTime myDate = this.txtDate.Text.ObjToDateTime();
            using (GetDate dateForm = new GetDate(myDate, "Enter Bank Deposit Date"))
            {
                dateForm.ShowDialog();
                if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    myDate = dateForm.myDateAnswer;
                    this.txtDate.Text = myDate.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void btnSelectBank_Click(object sender, EventArgs e)
        {
            using (SelectBank bankForm = new SelectBank())
            {
                bankForm.TopMost = true;
                bankForm.ShowDialog();
                if (bankForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                string bankRecord = bankForm.Answer;
                if (String.IsNullOrWhiteSpace(bankRecord))
                    return;
                string cmd = "Select * from `bank_accounts` where `record` = '" + bankRecord + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    txtBankAccount.Text = dx.Rows[0]["account_no"].ObjToString();
                    SomethingChanged();
                }
            }
        }
        /***********************************************************************************************/
        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            SomethingChanged();
        }
        /***********************************************************************************************/
    }
}