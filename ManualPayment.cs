using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Drawing;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraEditors;
/***************************************************************************************/

namespace SMFS
{
    /***************************************************************************************/
    public partial class ManualPayment : DevExpress.XtraEditors.XtraForm
    {
        public static DateTime ManualPaymentPaidDate = DateTime.Now;
        private bool saveMajorSwitch = false;
        private bool loading = true;
        private bool isPaid = false;
        private string workContract = "";
        private string workName = "";
        private DataTable workDt = null;
        private double payment = 0D;
        //private double retained = 0D;
        private double originalDownPayment = 0D;
        private int originalMethod = 0;
        private int numPayments = 0;
        private double totalInterest = 0D;
        private double interest = 0D;
        private double downPayment = 0D;
        private string dueDate = "";
        private string issueDate = "";
        private string apr = "";
        private double balanceDue = 0D;
        private double financeDays = 0D;
        private double monthlyPayment = 0D;
        private double totalFinanced = 0D;
        private double runningCB = 0D;
        private string paymentType = "";
        private double unpaid_interest = 0D;
        private double old_unpaid_interest = 0D;
        double maxTrust85 = 0D;
        double totalTrust85 = 0D;
        double difference = 0D;

        private bool correctedCA = false;
        private double c_trust85 = 0D;
        private double c_trust100 = 0D;
        private double c_interest = 0D;
        private double c_principal = 0D;
        private double c_apr = 0D;

        private string payerRecord = "";

        private double originalContractValue = 0D;
        private DateTime originalIssueDate = DateTime.Now;

        DateTime issue;
        DateTime testDate = new DateTime(2017, 12, 1);
        private bool testing = false;
        private int editRow = -1;
        private bool editing = false;
        private string editRowData = "";
        private string paymentFile = "payments";
        private string contractFile = "contracts";
        private string customerFile = "customers";
        private bool insuranceContract = false;
        private bool workDownPayment = false;
        private string payer = "";
        private double workTrust85C = 0D;
        private double workTrust85Max = 0D;
        private DataTable bankAccounts = null;
        private bool numPaymentsChanged = false;
        private double trustPercent = 0D;
        private string TrustPaid = "";
        private string workTCARecord = "";
        private bool workPDF = false;
        private string ccRecord = "";
        private DateTime exDate = DateTime.Now;
        private DataTable feeDt = null;
        private bool workJustTCA = false;
        /***************************************************************************************/
        public ManualPayment(string contract, string name)
        {
            InitializeComponent();
            workContract = contract;
            workName = name;
            testing = true;
        }
        /***************************************************************************************/
        public ManualPayment(string contract, string name, DataTable dt, double Trust85C, double Trust85Max, int row = -1, bool changeDownPayment = false, bool tcaReport = false, bool justTCA = false )
        {
            InitializeComponent();
            workContract = contract;
            workName = name;
            workDt = dt;
            editRow = row;
            workTrust85C = Trust85C;
            workTrust85Max = Trust85Max;
            if (editRow >= 0)
            {
                editing = true;
                workDownPayment = changeDownPayment;
            }
            else
                editRow = 0;
            if (tcaReport)
            {
                workPDF = true;
                ManualPayment_Load (null, null);
            }
        }
        /***************************************************************************************/
        public ManualPayment(string contract, string name, DataTable dt, double apr, double trust85, double trust100, double interest, double principal, double maxTrust85P, double trust85Paid )
        {
            InitializeComponent();
            workContract = contract;
            workName = name;
            workDt = dt;
            c_apr = apr;
            c_trust85 = trust85;
            c_trust100 = trust100;
            c_interest = interest;
            c_principal = principal;
            workTrust85C = trust85Paid;
            workTrust85Max = maxTrust85P;
            correctedCA = true;

            editRow = 0;
        }
        /***************************************************************************************/
        public ManualPayment( bool justTCA, string contract, string name, DataTable dt, double Trust85C, double Trust85Max )
        {
            InitializeComponent();
            workContract = contract;
            workName = name;
            workDt = dt;
            workTrust85C = Trust85C;
            workTrust85Max = Trust85Max;
            editRow = 0;
            workJustTCA = justTCA;
        }
        /***************************************************************************************/
        private void ManualPayment_Load(object sender, EventArgs e)
        {
            try
            {
                LoadBankAccounts();

                feeDt = LoadCCFeeTable();

                CheckCC();

                isPaid = false;

                saveMajorSwitch = DailyHistory.majorSwitch;
                DailyHistory.majorSwitch = true;
                btnLapsed.Hide();
                if (testing)
                    btnPost.Hide();
                runningCB = 0D;
                if (workDt.Rows.Count > 0)
                {
                    if (G1.get_column_number(workDt, "runningCB") >= 0)
                    {
                        int lastRow = 0;
                        if (DailyHistory.reverseSort)
                            lastRow = workDt.Rows.Count - 1;
                        runningCB = workDt.Rows[lastRow]["runningCB"].ObjToDouble();
                        runningCB = G1.RoundValue(runningCB);
                    }
                    string manual = "";
                    if (editRow >= 0 && editRow != 9999)
                    {
                        manual = workDt.Rows[editRow]["edited"].ObjToString();
                        editRowData = manual;
                    }
                    //if (manual.ToUpper() != "MANUAL" && manual.ToUpper() != "TRUSTADJ" )
                    //    workDt.Rows[editRow]["retained"] = 0D;
                }
                paymentFile = "payments";
                insuranceContract = false;
                if (DailyHistory.isInsurance(workContract))
                {
                    paymentFile = "ipayments";
                    customerFile = "icustomers";
                    contractFile = "icontracts";
                    insuranceContract = true;
                }
                CleanupWork(workDt);
                this.Text = "Manual Payment (" + workContract + ") " + workName;
                if (testing)
                    LoadTestData(ref balanceDue, ref totalInterest);

                LoadData();

                loading = false;
                //            CalcNewStuff();
                CalculateDetails();

                string dueDate = GetData("Due Date");
                if (dueDate == "12/31/2039")
                {
                    btnPost.BackColor = Color.Red;
                    btnPost.ForeColor = Color.White;
                }
                DeterminePaymentType();

                if ( workPDF )
                {
                    TrustPaid = "YES";
                    printPreviewToolStripMenuItem_Click(null, null);
                    this.Close();
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***************************************************************************************/
        private void DeterminePaymentType ()
        {
            paymentType = "";
            if (!editing)
                return;
            string payment = GetData("Payment");
            string debit = GetData("Debit");
            string credit = GetData("Credit");
            double dPayment = payment.ObjToDouble();
            double dDebit = debit.ObjToDouble();
            double dCredit = credit.ObjToDouble();
            if (dPayment != 0D)
                paymentType = "PAYMENT";
            else if (dDebit != 0D || dCredit != 0D)
                paymentType = "DEBIT/CREDIT";
        }
        /***************************************************************************************/
        public static void CleanupWork(DataTable dt)
        {
            if (dt == null)
                return;
            string fill = "";
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                fill = dt.Rows[i]["fill"].ObjToString().ToUpper();
                if (fill == "D")
                    dt.Rows.RemoveAt(i);
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ManualDone;
        protected void OnDone(string done)
        {
            if (ManualDone != null)
                ManualDone.Invoke(done);
        }
        /***************************************************************************************/
        private void LoadBankAccounts ()
        {
            string cmd = "Select * from `bank_accounts` where `show_dropdown` = '1';";
            bankAccounts = G1.get_db_data(cmd);
            this.repositoryItemComboBox2.Items.Clear();
            string account_no = "";
            for ( int i=0; i<bankAccounts.Rows.Count; i++)
            {
                account_no = bankAccounts.Rows[i]["account_no"].ObjToString();
                this.repositoryItemComboBox2.Items.Add(account_no);
            }
        }
        /***************************************************************************************/
        private bool CheckIfNeedBankAccount ()
        {
            bool good = true;
            bool doUpdateBankAccount = ImportDailyDeposits.getUpdateBankAccounts();
            if (!doUpdateBankAccount)
                return good;
            string bankAccount = GetData("Bank Account");
            if ( String.IsNullOrWhiteSpace ( bankAccount ))
            {
                good = false;
                DialogResult result = MessageBox.Show("A Bank Account MUST be entered!", "Bank Account Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return good;
        }
        /***************************************************************************************/
        private bool ValidateCA ()
        {
            bool valid = true;
            if (insuranceContract)
                return valid;
            string s_debit = GetData("Debit");
            string s_credit = GetData("Credit");
            string debitReason = GetData("Debit Reason");
            string creditReason = GetData("Credit Reason");

            double debit = s_debit.ObjToDouble();
            double credit = s_credit.ObjToDouble();

            string trust85P = GetData("Trust85P");
            string trust100P = GetData("Trust100P");

            double trust85 = trust85P.ObjToDouble();
            double trust100 = trust100P.ObjToDouble();

            if ( debit > 0D )
            {
                if ( trust85 > 0D || trust100 > 0D)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Trust85 or Trust100 MUST BE NEGATIVE for a DEBIT!\nDo you want to Override?", "Debit/Trust85 Problem Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                        valid = false;
                    else
                    {
                        if (!G1.ValidateOverridePassword())
                            valid = false;
                        else
                            valid = true;
                    }
                }
                else if ( trust85 == 0D || trust100 == 0D)
                {
                    DialogResult result = MessageBox.Show("***WARNING*** Trust85 or Trust100 is ZERO for a DEBIT!\nAccept Anyway?", "Debit/Trust85 Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                        valid = false;
                }
            }
            if ( credit > 0D)
            {
                if (trust85 < 0D || trust100 < 0D)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Trust85 or Trust100 MUST BE POSITIVE for a CREDIT!\nDo you want to Override?", "Credit/Trust85 Problem Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                        valid = false;
                    else
                    {
                        if (!G1.ValidateOverridePassword())
                            valid = false;
                        else
                            valid = true;
                    }
                }
            }

            return valid;
        }
        /***************************************************************************************/
        private bool CheckTrustBeginningBalance ()
        {
            if (insuranceContract)
                return true;

            DateTime datePaid = GetData("Date Paid").ObjToDateTime();
            double trust85P = GetData("Trust85P").ObjToDouble();
            double trust100P = GetData("Trust100P").ObjToDouble();
            if (trust100P == 0D)
                return true;

            DateTime startDate = new DateTime(datePaid.Year, datePaid.Month, 1);
            int days = DateTime.DaysInMonth(startDate.Year, datePaid.Month );
            DateTime endDate = new DateTime(datePaid.Year, datePaid.Month, days);
            double totalTrust85 = 0D;
            DateTime date = DateTime.Now;

            string cmd = "Select * from `trust2013r` where `contractNumber` = '" + workContract + "' AND `payDate8` = '" + endDate.ToString("yyyyMMdd") + "' ORDER BY `payDate8`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return true;

            DialogResult result = MessageBox.Show("Contract appears to have data already in\n\nTrust Beginning Balance!\n\nDo you still want to post this transaction?", "Trust Beginning Balance Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return false;

            MessageBox.Show("If so, don't forget to run or correct the\n\nTrust Beginning Balance for\n\nContract " + workContract + "!", "Trust Beginning Balance Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            return true;
        }
        /***************************************************************************************/
        private bool CheckDueDate()
        {
            bool rtn = true;
            string nextDueDate = GetData("Next Due Date");
            DateTime date = nextDueDate.ObjToDateTime();
            int day = date.Day;
            if (DailyHistory.isInsurance(workContract))
            {
                if (day != 1 && day != 15)
                {
                    MessageBox.Show("*** ERROR *** Next Due Date for Insurance\nmust be set on 1st or 15th!", "Next Due Date Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    rtn = false;
                }
            }
            else
            {
                if (nextDueDate != "12/31/2039")
                {
                    if (day != 1)
                    {
                        MessageBox.Show("*** ERROR *** Next Due Date for Trust\nmust be set on 1st!", "Next Due Date Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        rtn = false;
                    }
                }
            }
            return rtn;
        }
        /***************************************************************************************/
        private void btnPost_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CheckDueDate())
                    return;

                if (!CheckTrustBeginningBalance())
                    return;

                if ( LoginForm.useNewTCACalculation )
                {
                    UpdatePossibleCC();
                    btnPost_ClickNew(null, null);
                    return;
                }
                if (!ValidateCA())
                    return;

                int idx = 0;
                string str = "";

                double D_trust85P = 0D;
                double D_trust100P = 0D;
                double D_payment = 0D;
                double D_credit = 0D;
                double retained = 0D;

                double paidOffTrust85 = 0D;
                double paidOffTrust100 = 0D;


                if (isPaid)
                {
                    DateTime docp = GetData("Date Paid").ObjToDateTime();

                    bool success = UpdateForcedPayoff(workContract, docp, difference, originalContractValue, maxTrust85, totalTrust85);
                    string s_payment = GetData("Payment");
                    string s_trust85P = GetData("Trust85P");
                    string s_trust100P = GetData("Trust100P");
                    string s_credit = GetData("Credit");

                    D_trust85P = s_trust85P.ObjToDouble();
                    D_trust100P = s_trust100P.ObjToDouble();
                    D_payment = s_payment.ObjToDouble();
                    D_credit = s_credit.ObjToDouble();
                    retained = (D_payment + D_credit) - D_trust100P;
                    retained = G1.RoundValue(retained);

                    str = label2.Text;
                    idx = str.IndexOf("Payoff");
                    if ( idx > 0 )
                    {
                        str = str.Substring(idx);
                        idx = str.IndexOf(":");
                        if ( idx > 0 )
                        {
                            str = str.Substring(idx);
                            str = str.Replace(":", "").Trim();
                            string[] Lines = str.Split('/');
                            if ( Lines.Length >= 2 )
                            {
                                paidOffTrust85 = Lines[0].ObjToDouble();
                                paidOffTrust100 = Lines[1].ObjToDouble();
                            }
                        }
                    }
                }
                string dateDue = GetData("Due Date");
                dateDue = GetData("Next Due Date");
                if (dateDue == "12/31/2039")
                {
                    DialogResult result = MessageBox.Show("Contract appears to be PAID UP!\nDo you still want to post this transaction?", "Post Paid Up Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.No)
                        return;
                }
                if (!CheckIfNeedBankAccount())
                    return;
                string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;

                string lapsed = dx.Rows[0]["lapsed"].ObjToString();
                if (lapsed.ToUpper() == "Y")
                {
                    sendMessage();
                    return;
                }
                string contractRecord = dx.Rows[0]["record"].ObjToString();
                trustPercent = dx.Rows[0]["trustPercent"].ObjToDouble();
                double totalInterest = dx.Rows[0]["totalInterest"].ObjToDouble();
                double balanceDue = dx.Rows[0]["balanceDue"].ObjToDouble();
                if (!insuranceContract)
                {
                    if (workDt.Rows.Count > 0)
                    {
                        int lastRow = 0;
                        if (DailyHistory.reverseSort)
                            lastRow = workDt.Rows.Count - 1;
                        balanceDue = workDt.Rows[lastRow]["newbalance"].ObjToDouble();
                        balanceDue = G1.RoundValue(balanceDue);
                    }
                }
                else
                {
                    DateTime docp = GetData("Date Paid").ObjToDateTime();
                    balanceDue = Policies.CalcMonthlyPremium(payer, docp );
                }

                cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + workContract + "';";
                DataTable ddx = G1.get_db_data(cmd);

                DataTable dt = (DataTable)dgv.DataSource;

                string lastName = ddx.Rows[0]["lastName"].ObjToString();
                string firstName = ddx.Rows[0]["firstName"].ObjToString();

                string payment = GetData("Payment");
                string principal = GetData("Principal");
                string datePaid = GetData("Date Paid");
                string dueDate = GetData("Due Date");
                string nextDueDate = GetData("Next Due Date");
                string location = GetData("Location");
                string agent = GetData("Agent");
                string checknumber = GetData("Check Number");
                string interest = GetData("Interest");
                string ccFee = GetData("CC Fee");
                string trust85P = GetData("Trust85P");
                string trust100P = GetData("Trust100P");
                string debit = GetData("Debit");
                string credit = GetData("Credit");
                string debitReason = GetData("Debit Reason");
                string creditReason = GetData("Credit Reason");
                string creditBalance = GetData("Credit Balance");
                string user = GetData("User");
                string depositNumber = GetData("Deposit Number");
                string bankAccount = GetData("Bank Account");
                string unpaid_interest = GetData("Unpaid Interest");
                string numPayments = GetData("# PAYMENTS");

                //string retained = GetData("Retained");

                totalInterest += interest.ObjToDouble();

                str = lblNewBalance.Text;
                idx = str.IndexOf("$");
                if (idx >= 0)
                {
                    str = str.Substring(idx);
                    str = str.Replace("$", "");
                    str = str.Replace(",", "");
                }
                else
                {
                    str = GetData("Balance");
                    str = str.Replace("$", "");
                    str = str.Replace(",", "");
                }

                balanceDue = str.ObjToDouble();
                balanceDue = G1.RoundValue(balanceDue);

                string record = "";
                if (editing)
                    record = workDt.Rows[editRow]["record"].ObjToString();
                else
                    record = G1.create_record(paymentFile, "lastName", "-1");
                if (G1.BadRecord(paymentFile, record))
                    return;

                try
                {
                    bool changingDownPayment = false;
                    if (editing)
                    {
                        string downPayment = GetData("Down Payment");
                        double dPayment = GetMoney("Down Payment");
                        if (dPayment != originalDownPayment)
                        {
                            changingDownPayment = true;
                            balanceDue = GetBalanceDue();
                            double newbalanceDue = balanceDue + originalDownPayment - dPayment;
                            G1.update_db_table(paymentFile, "record", record, new string[] { "downPayment", dPayment.ToString() });
                            G1.update_db_table(contractFile, "record", contractRecord, new string[] { "balanceDue", newbalanceDue.ToString(), "downPayment", dPayment.ToString() });
                            //string pbalance = G1.ReformatMoney(balanceDue);
                            //lblBalance.Text = "Balance :$" + pbalance;
                        }
                    }
                    string edited = "Manual";
                    if (DailyHistory.gotCemetery(workContract))
                        edited = "Cemetery";
                    G1.update_db_table(paymentFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", payment, "interestPaid", interest, "debitAdjustment", debit, "creditAdjustment", credit, "debitReason", debitReason, "creditReason", creditReason, "unpaid_interest", unpaid_interest });
                    G1.update_db_table(paymentFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", trust85P, "trust100P", trust100P, "location", location, "agentNumber", agent, "userId", user, "depositNumber", depositNumber, "edited", edited, "bank_account", bankAccount });
                    if (isPaid)
                    {
                        D_trust85P = trust85P.ObjToDouble();
                        D_trust100P = trust100P.ObjToDouble();
                        D_payment = payment.ObjToDouble();
                        D_credit = credit.ObjToDouble();
                        retained = (D_payment + D_credit) - D_trust100P;
                        retained = G1.RoundValue(retained);
                        G1.update_db_table(paymentFile, "record", record, new string[] { "retained", retained.ToString(), "new", "finale" });
                    }
                    if (insuranceContract)
                    {
                        G1.update_db_table(paymentFile, "record", record, new string[] {"numMonthPaid", numPayments });
                    }
                    string audit = "DueDate:" + dueDate + " Pmt: " + payment.ToString();
                    if (editing)
                        audit = "Paid Date: " + datePaid + " Pmt/Credit/Debit: " + payment.ToString() + "/" + credit.ToString() + "/" + debit.ToString();

                    if (!editing)
                    {
                        if (!insuranceContract)
                        {
                            if (balanceDue <= 0D)
                                nextDueDate = "12/31/2039";
                        }
                        G1.update_db_table(contractFile, "record", contractRecord, new string[] { "balanceDue", balanceDue.ToString(), "dueDate8", nextDueDate, "lastDatePaid8", datePaid, "creditBalance", creditBalance, "unpaid_interest", unpaid_interest });
                        if ( insuranceContract && !String.IsNullOrWhiteSpace ( payer))
                        {
                            if ( !String.IsNullOrWhiteSpace ( payerRecord))
                                G1.update_db_table("payers", "record", payerRecord, new string[] { "dueDate8", nextDueDate, "lastDatePaid8", datePaid, "creditBalance", creditBalance });
                        }
                        G1.AddToAudit(LoginForm.username, "ManualPayment", "Add", audit, workContract);
                    }
                    else
                    {
                        if (!changingDownPayment)
                            RecalcBalance(contractRecord, creditBalance);
                        G1.AddToAudit(LoginForm.username, "ManualPayment", "Edit", audit, workContract);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updating Payments Record " + record);
                }

                TrustPaid = "";

                printPreviewToolStripMenuItem_Click(null, null);
                //if ( isPaid )
                //{
                //    PutMoney("Payment", 0D);
                //    PutMoney("Interest", 0D);
                //    PutMoney("Principal", 0D);
                //    PutMoney("Trust85P", paidOffTrust85);
                //    PutMoney("Trust100P", paidOffTrust100);
                //    TrustPaid = "YES";
                //    printPreviewToolStripMenuItem_Click(null, null);
                //}
                OnDone("POST");
            }
            catch ( Exception ex)
            {
            }
            if (correctedCA)
                this.Close();
        }
        /***************************************************************************************/
        private bool CheckCC ()
        {
            ccRecord = "";
            string lookup = workContract;
            if (!String.IsNullOrWhiteSpace(payer))
                lookup = payer;
            string cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + lookup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            string str = dt.Rows[0]["expirationDate"].ObjToString();
            if (str.IndexOf("/") < 0)
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            string[] Lines = str.Split('/');
            if (Lines.Length < 2)
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            str = Lines[0].Trim();
            int month = str.ObjToInt32();
            if (month <= 0 || month > 12)
            {
                MessageBox.Show("*** ERROR *** Expiration Date Appears To Have An Invalid Month?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }

            int year = Lines[1].Trim().ObjToInt32();
            year += 2000;
            int days = DateTime.DaysInMonth(year, month);
            exDate = new DateTime(year, month, days);
            if (exDate < DateTime.Now)
            {
                MessageBox.Show("*** INFO *** Credit Card Expiration Date Appears to have been met!", "Expiration Date Met Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            ccRecord = dt.Rows[0]["record"].ObjToString();
            return true;
        }
        /***************************************************************************************/
        private void UpdatePossibleCC ()
        {
            if (String.IsNullOrWhiteSpace(ccRecord))
                return;
            string cmd = "Select * from `creditcards` WHERE `record` = '" + ccRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            string payment = GetData("Payment");
            string principal = GetData("Principal");
            string datePaid = GetData("Date Paid");
            string dueDate = GetData("Due Date");
            string nextDueDate = GetData("Next Due Date");
            string location = GetData("Location");
            string agent = GetData("Agent");
            string checknumber = GetData("Check Number");
            string interest = GetData("Interest");
            string ccFee = GetData("CC Fee");
            string trust85P = GetData("Trust85P");
            string trust100P = GetData("Trust100P");
            string debit = GetData("Debit");
            string credit = GetData("Credit");
            string debitReason = GetData("Debit Reason");
            string creditReason = GetData("Credit Reason");
            string creditBalance = GetData("Credit Balance");
            string user = GetData("User");
            string depositNumber = GetData("Deposit Number");
            string bankAccount = GetData("Bank Account");
            string unpaid_interest = GetData("Unpaid Interest");
            string numPayments = GetData("# PAYMENTS");

            int nPayments = (int)Convert.ToInt64(Convert.ToDouble(numPayments));

            string str = dt.Rows[0]["remainingPayments"].ObjToString();
            int remainingPayments = (int)Convert.ToInt64(Convert.ToDouble(str));

            if (remainingPayments != 999)
            {
                remainingPayments -= nPayments;
                if (remainingPayments < 0)
                    remainingPayments = 0;
                if (remainingPayments <= 1)
                {
                    string message = "";
                    if (!String.IsNullOrWhiteSpace(payer))
                        message = payer + " ";
                    else
                        message = workContract + " ";
                    message += "Credit Card Customer Appears To Be Down to Less Than 2 Remaining Payments On Credit Card!";
                    Messages.SendTheMessage(LoginForm.username, "cjenkins", "Credit Card Customer", message);
                    //Messages.SendTheMessage(LoginForm.username, "robby", "Credit Card Customer", message);
                }
                G1.update_db_table("creditcards", "record", ccRecord, new string[] { "remainingPayments", remainingPayments.ToString() });
            }
        }
        /***************************************************************************************/
        private void btnPost_ClickNew(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateCA())
                    return;

                int idx = 0;
                string str = "";

                double D_trust85P = 0D;
                double D_trust100P = 0D;
                double D_payment = 0D;
                double D_credit = 0D;
                double retained = 0D;

                double paidOffTrust85 = 0D;
                double paidOffTrust100 = 0D;


                if (isPaid)
                {
                    //    DateTime docp = GetData("Date Paid").ObjToDateTime();

                    //    bool success = UpdateForcedPayoff(workContract, docp, difference, originalContractValue, maxTrust85, totalTrust85);
                    //    string s_payment = GetData("Payment");
                    //    string s_trust85P = GetData("Trust85P");
                    //    string s_trust100P = GetData("Trust100P");
                    //    string s_credit = GetData("Credit");

                    //    D_trust85P = s_trust85P.ObjToDouble();
                    //    D_trust100P = s_trust100P.ObjToDouble();
                    //    D_payment = s_payment.ObjToDouble();
                    //    D_credit = s_credit.ObjToDouble();
                    //    retained = (D_payment + D_credit) - D_trust100P;
                    //    retained = G1.RoundValue(retained);
                    //string PaymentDetil = "   Payoff Trust Credit : " + G1.ReformatMoney(difference) + " / " + G1.ReformatMoney(credit100P);

                    str = label2.Text;
                    idx = str.IndexOf("Payoff");
                    if (idx > 0)
                    {
                        str = str.Substring(idx);
                        idx = str.IndexOf(":");
                        if (idx > 0)
                        {
                            str = str.Substring(idx);
                            str = str.Replace(":", "").Trim();
                            string[] Lines = str.Split('/');
                            if (Lines.Length >= 2)
                            {
                                paidOffTrust85 = Lines[0].ObjToDouble();
                                paidOffTrust100 = Lines[1].ObjToDouble();
                            }
                        }
                    }
                }
                if ( workJustTCA )
                {
                    paidOffTrust85 = GetMoney("Trust85P");
                    paidOffTrust100 = GetMoney("Trust100P");
                }
                string dateDue = GetData("Due Date");
                dateDue = GetData("Next Due Date");
                if (dateDue == "12/31/2039" && !workJustTCA )
                {
                    DialogResult result = MessageBox.Show("Contract appears to be PAID UP!\nDo you still want to post this transaction?", "Post Paid Up Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.No)
                        return;
                }
                if (!CheckIfNeedBankAccount())
                    return;
                string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return;

                string lapsed = dx.Rows[0]["lapsed"].ObjToString();
                if (lapsed.ToUpper() == "Y")
                {
                    sendMessage();
                    return;
                }
                string contractRecord = dx.Rows[0]["record"].ObjToString();
                trustPercent = dx.Rows[0]["trustPercent"].ObjToDouble();
                double totalInterest = dx.Rows[0]["totalInterest"].ObjToDouble();
                double balanceDue = dx.Rows[0]["balanceDue"].ObjToDouble();
                if (!insuranceContract)
                {
                    if (workDt.Rows.Count > 0)
                    {
                        int lastRow = 0;
                        if (DailyHistory.reverseSort)
                            lastRow = workDt.Rows.Count - 1;
                        balanceDue = workDt.Rows[lastRow]["newbalance"].ObjToDouble();
                        balanceDue = G1.RoundValue(balanceDue);
                    }
                }
                else
                {
                    DateTime docp = GetData("Date Paid").ObjToDateTime();
                    balanceDue = Policies.CalcMonthlyPremium(payer, docp);
                }

                cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + workContract + "';";
                DataTable ddx = G1.get_db_data(cmd);

                DataTable dt = (DataTable)dgv.DataSource;

                string lastName = ddx.Rows[0]["lastName"].ObjToString();
                string firstName = ddx.Rows[0]["firstName"].ObjToString();

                string payment = GetData("Payment");
                string principal = GetData("Principal");
                string datePaid = GetData("Date Paid");
                string dueDate = GetData("Due Date");
                string nextDueDate = GetData("Next Due Date");
                string location = GetData("Location");
                string agent = GetData("Agent");
                string checknumber = GetData("Check Number");
                string interest = GetData("Interest");
                string ccFee = GetData("CC Fee");
                string trust85P = GetData("Trust85P");
                string trust100P = GetData("Trust100P");
                string debit = GetData("Debit");

                string credit = GetData("Credit");
                D_credit = credit.ObjToDouble();
                double D_interest = interest.ObjToDouble();

                string debitReason = GetData("Debit Reason");
                string creditReason = GetData("Credit Reason");
                string creditBalance = GetData("Credit Balance");
                string user = GetData("User");
                string depositNumber = GetData("Deposit Number");
                string bankAccount = GetData("Bank Account");
                string unpaid_interest = GetData("Unpaid Interest");
                string numPayments = GetData("# PAYMENTS");
                string refund = GetData("Is Refund");
                if (insuranceContract && refund.ToUpper() == "Y")
                    numPayments = "0";

                //string retained = GetData("Retained");

                totalInterest += interest.ObjToDouble();

                str = lblNewBalance.Text;
                idx = str.IndexOf("$");
                if (idx >= 0)
                {
                    str = str.Substring(idx);
                    str = str.Replace("$", "");
                    str = str.Replace(",", "");
                }
                else
                {
                    str = GetData("Balance");
                    str = str.Replace("$", "");
                    str = str.Replace(",", "");
                }

                balanceDue = str.ObjToDouble();
                balanceDue = G1.RoundValue(balanceDue);

                string record = "";
                if (editing && editRow != 9999)
                {
                    record = workDt.Rows[editRow]["record"].ObjToString();
                    if (G1.BadRecord(paymentFile, record))
                        return;
                }
                else
                {
                    if (!workJustTCA)
                    {
                        record = G1.create_record(paymentFile, "lastName", "-1");
                        if (G1.BadRecord(paymentFile, record))
                            return;
                    }
                }
                //if (G1.BadRecord(paymentFile, record))
                //    return;

                try
                {
                    bool changingDownPayment = false;
                    if (editing)
                    {
                        string downPayment = GetData("Down Payment");
                        double dPayment = GetMoney("Down Payment");
                        if (dPayment != originalDownPayment || editRow == 9999 )
                        {
                            changingDownPayment = true;
                            balanceDue = GetBalanceDue();
                            double newbalanceDue = balanceDue + originalDownPayment - dPayment;
                            G1.update_db_table(paymentFile, "record", record, new string[] { "downPayment", dPayment.ToString() });
                            if (editRow == 9999)
                            {
                                G1.update_db_table(paymentFile, "record", record, new string[] { "paymentAmount", "0.00" });
                                payment = "0.00";
                            }
                            G1.update_db_table(contractFile, "record", contractRecord, new string[] { "balanceDue", newbalanceDue.ToString(), "downPayment", dPayment.ToString() });
                            //string pbalance = G1.ReformatMoney(balanceDue);
                            //lblBalance.Text = "Balance :$" + pbalance;
                        }
                    }
                    string edited = "Manual";
                    if (refund.ToUpper() == "Y")
                        edited = "REFUND";
                    if (DailyHistory.gotCemetery(workContract))
                        edited = "Cemetery";
                    if (!workJustTCA)
                    {
                        G1.update_db_table(paymentFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", payment, "interestPaid", interest, "ccFee", ccFee, "debitAdjustment", debit, "creditAdjustment", credit, "debitReason", debitReason, "creditReason", creditReason, "unpaid_interest", unpaid_interest });
                        G1.update_db_table(paymentFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", trust85P, "trust100P", trust100P, "location", location, "agentNumber", agent, "userId", user, "depositNumber", depositNumber, "edited", edited, "bank_account", bankAccount });
                    }
                    //if (isPaid)
                    //{
                    //    D_trust85P = trust85P.ObjToDouble();
                    //    D_trust100P = trust100P.ObjToDouble();
                    //    D_payment = payment.ObjToDouble();
                    //    D_credit = credit.ObjToDouble();
                    //    retained = (D_payment + D_credit )- D_trust100P;
                    //    retained = G1.RoundValue(retained);
                    //    G1.update_db_table(paymentFile, "record", record, new string[] { "retained", retained.ToString(), "new", "finale" });
                    //}
                    if (insuranceContract)
                    {
                        G1.update_db_table(paymentFile, "record", record, new string[] { "numMonthPaid", numPayments });
                    }
                    string audit = "DueDate:" + dueDate + " Pmt: " + payment.ToString();
                    if (editing)
                        audit = "Paid Date: " + datePaid + " Pmt/Credit/Debit: " + payment.ToString() + "/" + credit.ToString() + "/" + debit.ToString();

                    if (!editing)
                    {
                        if (!insuranceContract)
                        {
                            if (balanceDue <= 0D || workJustTCA )
                                nextDueDate = "12/31/2039";
                        }
                        if ( D_credit > 0D && D_interest == 0D )
                            G1.update_db_table(contractFile, "record", contractRecord, new string[] { "balanceDue", balanceDue.ToString(), "dueDate8", nextDueDate, "creditBalance", creditBalance, "unpaid_interest", unpaid_interest });
                        else
                            G1.update_db_table(contractFile, "record", contractRecord, new string[] { "balanceDue", balanceDue.ToString(), "dueDate8", nextDueDate, "lastDatePaid8", datePaid, "creditBalance", creditBalance, "unpaid_interest", unpaid_interest });
                        if (insuranceContract && !String.IsNullOrWhiteSpace(payer))
                        {
                            if (!String.IsNullOrWhiteSpace(payerRecord))
                                G1.update_db_table("payers", "record", payerRecord, new string[] { "dueDate8", nextDueDate, "lastDatePaid8", datePaid, "creditBalance", creditBalance });
                        }
                        if ( !workJustTCA )
                            G1.AddToAudit(LoginForm.username, "ManualPayment", "Add", audit, workContract);
                    }
                    else
                    {
                        if (!changingDownPayment)
                            RecalcBalance(contractRecord, creditBalance);
                        G1.AddToAudit(LoginForm.username, "ManualPayment", "Edit", audit, workContract);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updating Payments Record " + record);
                }

                TrustPaid = "";

                if ( !workJustTCA )
                    printPreviewToolStripMenuItem_Click(null, null);
                if (isPaid && paidOffTrust85 != 0D && D_credit == 0D )
                {
                    PutMoney("Payment", 0D);
                    PutMoney("Principal", 0D);
                    PutMoney("Trust85P", paidOffTrust85);
                    PutMoney("Trust100P", paidOffTrust100);
                    TrustPaid = "YES";

                    try
                    {
                        record = G1.create_record(paymentFile, "lastName", "-1");
                        if (G1.BadRecord(paymentFile, record))
                            return;
                        retained = -1D * paidOffTrust100;

                        string TCADepositNumber = ImportDailyDeposits.getNextTCANumber();
                        TCADepositNumber = "TCA-" + TCADepositNumber;

                        if (workContract.Contains("LI"))
                        {
                            PutMoney("Interest", retained);
                            G1.update_db_table(paymentFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", "0.00", "interestPaid", retained.ToString(), "ccFee", "0.00", "debitAdjustment", "0.00", "creditAdjustment", "0.00", "debitReason", "", "creditReason", "TCA", "unpaid_interest", "0.00" });
                            G1.update_db_table(paymentFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", paidOffTrust85.ToString(), "trust100P", paidOffTrust100.ToString(), "retained", "0.00", "location", location, "agentNumber", agent, "userId", user, "depositNumber", TCADepositNumber, "edited", "TrustAdj", "bank_account", bankAccount });
                        }
                        else
                        {
                            PutMoney("Interest", 0D);
                            G1.update_db_table(paymentFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", "0.00", "interestPaid", "0.00", "ccFee", "0.00", "debitAdjustment", "0.00", "creditAdjustment", "0.00", "debitReason", "", "creditReason", "TCA", "unpaid_interest", "0.00" });
                            G1.update_db_table(paymentFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", paidOffTrust85.ToString(), "trust100P", paidOffTrust100.ToString(), "retained", retained.ToString(), "location", location, "agentNumber", agent, "userId", user, "depositNumber", TCADepositNumber, "edited", "TrustAdj", "bank_account", bankAccount });
                        }

                        printPreviewToolStripMenuItem_Click(null, null);
                    }
                    catch ( Exception ex)
                    {
                        MessageBox.Show("A critical exception has occurred while attempting to write detail (" + record + ") for a PaidOff Contract :\n" + ex.Message, "PaidOff Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                OnDone("POST");
            }
            catch (Exception ex)
            {
            }
            if (correctedCA)
                this.Close();
        }
        /***************************************************************************************/
        private void UpdateForcedPayoff ()
        {
            if (!isPaid)
                return;

            try
            {
                DateTime docp = GetData("Date Paid").ObjToDateTime();

                string record = G1.create_record("forced_paidoff", "spareText", "-1");
                if (G1.BadRecord("forced_paidoff", record))
                    return;
                G1.update_db_table("forced_paidoff", "record", record, new string[] { "contractNumber", workContract, "difference", difference.ToString(), "contractValue", originalContractValue.ToString(), "maxTrust85", maxTrust85.ToString(), "totalTrust85", totalTrust85.ToString(), "datePaid", docp.ToString("yyyy-MM-dd"), "spareText", "" });
            }
            catch ( Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to write detail for a PaidOff Contract :\n" + ex.Message, "PaidOff Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /***************************************************************************************/
        public static bool UpdateForcedPayoff( string contractNumber, DateTime docp, double difference, double contractValue, double maxTrust85, double totalTrust85)
        {
            bool paid = false;
            try
            {
                string record = G1.create_record("forced_paidoff", "spareText", "-1");
                if (G1.BadRecord("forced_paidoff", record))
                    return paid;
                G1.update_db_table("forced_paidoff", "record", record, new string[] { "contractNumber", contractNumber, "difference", difference.ToString(), "contractValue", contractValue.ToString(), "maxTrust85", maxTrust85.ToString(), "totalTrust85", totalTrust85.ToString(), "datePaid", docp.ToString("yyyy-MM-dd"), "spareText", "" });
                paid = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to write detail for a PaidOff Contract :\n" + ex.Message, "PaidOff Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            return paid;
        }
        /***************************************************************************************/
        private void RecalcBalance(string contractRecord, string creditBalance)
        {
            string status = "";
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double ccFee = 0D;
            double principal = payment - interest;
            double balance = 0D;
            double sBalance = DailyHistory.GetFinanceValue(workContract);

            string cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToString().ObjToDouble();
                ccFee = dt.Rows[i]["ccFee"].ObjToString().ObjToDouble();
                principal = payment - interest - ccFee;
                balance = sBalance - principal + debit - credit;
                if (status.ToUpper() == "D")
                    balance = sBalance;
                string reason = dt.Rows[i]["debitReason"].ObjToString() + " " + dt.Rows[i]["creditReason"].ObjToString();
                sBalance = balance;
            }
            if (insuranceContract && !String.IsNullOrWhiteSpace(payer))
                sBalance = Policies.CalcMonthlyPremium(payer, DateTime.Now );
            G1.update_db_table(contractFile, "record", contractRecord, new string[] { "balanceDue", sBalance.ToString(), "creditBalance", creditBalance });
            if ( insuranceContract && !String.IsNullOrWhiteSpace ( payer))
            {
                if ( !String.IsNullOrWhiteSpace ( payerRecord ))
                    G1.update_db_table("payers", "record", payerRecord, new string[] { "creditBalance", creditBalance });
            }
        }
        /***************************************************************************************/
        private double PutMoney(string name, double money, bool old = false )
        {
            double value = 0D;
            string str = "";
            DataTable dt = (DataTable)dgv.DataSource;
            int row = FindRow(name);
            if (row >= 0)
            {
                value = G1.RoundValue(money);
                str = G1.ReformatMoney(value);
                if (old)
                    dt.Rows[row]["lastpayment"] = str;
                else
                    dt.Rows[row]["newpayment"] = str;
            }
            return value;
        }
        /***************************************************************************************/
        private double GetMoney(string name, bool old = false )
        {
            double value = 0D;
            string str = "";
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                int row = FindRow(name);
                if (row >= 0)
                {
                    str = dt.Rows[row]["newpayment"].ObjToString();
                    if (old)
                        str = dt.Rows[row]["lastpayment"].ObjToString();
                    value = str.ObjToDouble();
                    value = G1.RoundValue(value);
                }
            }
            catch ( Exception ex)
            {
            }
            return value;
        }
        /***************************************************************************************/
        private void PutData(string name, string data)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int row = FindRow(name);
            if (row >= 0)
                dt.Rows[row]["newpayment"] = data;
        }
        /***************************************************************************************/
        private string GetData(string name, bool old = false)
        {
            string str = "";
            DataTable dt = (DataTable)dgv.DataSource;
            int row = FindRow(name);
            if (row >= 0)
            {
                str = dt.Rows[row]["newpayment"].ObjToString();
                if (old)
                    str = dt.Rows[row]["lastpayment"].ObjToString();
            }
            return str;
        }
        /***************************************************************************************/
        private void btnAbort_Click(object sender, EventArgs e)
        {
            OnDone("ABORT");
        }
        /***************************************************************************************/
        private double CalculateInterest(DateTime dueDate, double apr, double balance, int days = -1 )
        {
            int yearlyDays = 365;
            if (DateTime.IsLeapYear(dueDate.Year))
                yearlyDays = 366;

            string date = dueDate.Month.ToString("D2") + "/" + dueDate.Day.ToString("D2") + "/" + dueDate.Year.ToString("D4");

            if (days == -1)
                days = DailyHistory.GetDaysSinceLastPayment(workContract, date);

            double dailyInterest = apr / (double)(yearlyDays) * (double)(days);
            interest = dailyInterest * balance;
            interest = G1.RoundDown(interest);
            return interest;
        }
        /***************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `" + contractFile + "` c JOIN `" + customerFile + "` b ON c.`contractNumber` = b.`contractNumber` where c.`contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            old_unpaid_interest = dx.Rows[0]["unpaid_interest"].ObjToDouble();
            string title = "Manual Payment for " + dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["lastName"].ObjToString();
            if (insuranceContract)
                title = "Manual Payment for Payer (" + dx.Rows[0]["payer"].ObjToString() + ") " + dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["lastName"].ObjToString();
            this.Text = title;
            payer = dx.Rows[0]["payer"].ObjToString();

            if (dx.Rows[0]["lapsed"].ObjToString().ToUpper().Trim() == "Y")
                btnLapsed.Show();

            financeDays = dx.Rows[0]["numberOfPayments"].ObjToDouble();

            payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            payerRecord = "";
            if (insuranceContract)
                payment = Policies.CalcMonthlyPremium(payer, DateTime.Now);

            if (DailyHistory.isInsurance(workContract) && payment > 500D)
                payment = Policies.CalcMonthlyPremium(payer, DateTime.Now);

            if ( insuranceContract && !String.IsNullOrWhiteSpace ( payer))
            {
                cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                DataTable ddt = G1.get_db_data(cmd);
                if ( ddt.Rows.Count > 0 )
                    payerRecord = ddt.Rows[0]["record"].ObjToString();
            }

            double amtOfMonthlyPayt = payment;
            monthlyPayment = payment;
            totalFinanced = DailyHistory.GetFinanceValue(dx.Rows[0]);
            string pamount = G1.ReformatMoney(payment);
            numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            double newbalance = 0D;
            if (workDt != null)
            {
                if (!editing && !insuranceContract )
                {
                    DateTime iDate = DailyHistory.getNextDueDate(workDt, monthlyPayment, ref newbalance);
                    DateTime lastDueDate = FindMismatches.VerifyDueDate(workContract);
                    //lastDueDate = lastDueDate.AddMonths(1);
                    dueDate = iDate.ToString("MM/dd/yyyy");
                    dueDate = lastDueDate.ToString("MM/dd/yyyy");
                }
            }
            issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            apr = dx.Rows[0]["APR"].ObjToString();

            if (correctedCA)
                apr = c_apr.ObjToString();

            downPayment = dx.Rows[0]["downPayment"].ObjToDouble();
            if (downPayment == 0D)
                downPayment = DailyHistory.GetDownPaymentFromPayments(workContract);
            string pdownPayment = G1.ReformatMoney(downPayment);
            originalDownPayment = downPayment;

            balanceDue = dx.Rows[0]["balanceDue"].ObjToString().ObjToDouble();
            if (workDt.Rows.Count > 0)
            {
                int lastRow = 0;
                if (DailyHistory.reverseSort)
                    lastRow = workDt.Rows.Count - 1;
                balanceDue = workDt.Rows[lastRow]["balance"].ObjToDouble();
                balanceDue = G1.RoundValue(balanceDue);
            }
            if ( insuranceContract )
                balanceDue = Policies.CalcMonthlyPremium(payer, DateTime.Now );

            string pbalance = G1.ReformatMoney(balanceDue);
            lblBalance.Text = "Balance :$" + pbalance;

            DateTime lastDate = DateTime.Now;

            if ( workJustTCA )
            {
                string last = DailyHistory.GetLastPaymentDate(workContract);
                if (G1.validate_date(last))
                    lastDate = last.ObjToDateTime();
            }

            int days = DailyHistory.GetDaysSinceLastPayment(workContract);
            lblDays.Text = "Days since Last Payment :" + days.ToString();


            double rate = apr.ObjToDouble() / 100D;

            interest = G1.RoundValue(balanceDue * rate / 12.0D);
            unpaid_interest = 0D;
            //interest = CalculateInterest(DateTime.Now, rate, balanceDue );
            interest = CalculateInterest(lastDate, rate, balanceDue);

            if (workJustTCA)
                interest = 0D;
            string pinterest = G1.ReformatMoney(interest);
            if (correctedCA)
                pinterest = G1.ReformatMoney(c_interest);
            double principal = payment - interest;
            principal = G1.RoundValue(principal);
            if (principal < 0D)
            {
                unpaid_interest = Math.Abs(principal);
                principal = 0D;
                interest = payment;
            }

            issue = issueDate.ObjToDateTime();
            //DateTime testDate = new DateTime(2017, 12, 1);
            //retained = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double ccFee = 0D;
            double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);
            originalContractValue = contractValue;

            originalMethod = ImportDailyDeposits.CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, originalDownPayment, financeDays, payment, principal, rate, ref trust85P, ref trust100P);

            cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + workContract + "';";
            dx = G1.get_db_data(cmd);

            string bankAccount = "";

            string agent = dx.Rows[0]["agentCode"].ObjToString();
            if (!testing)
            {
                if (workDt.Rows.Count > 0)
                {
                    agent = "DITTO";
                    if (editing)
                        bankAccount = "DITTO";
                }
            }

            DateTime date = DateTime.Now;
            if (workJustTCA)
                date = lastDate;

            string datePaid = date.Month.ToString() + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
            if ( !workJustTCA )
                datePaid = ManualPaymentPaidDate.ToString("MM/dd/yyyy");
            else
                datePaid = date.ToString("MM/dd/yyyy");

            string p_trust85P = G1.ReformatMoney(trust85P);
            string p_trust100P = G1.ReformatMoney(trust100P);
            if ( correctedCA )
            {
                p_trust85P = G1.ReformatMoney(c_trust85);
                p_trust100P = G1.ReformatMoney(c_trust100);
            }
            string p_principal = G1.ReformatMoney(principal);
            string p_creditBalance = G1.ReformatMoney(runningCB);
            string p_unpaid_interest = G1.ReformatMoney(unpaid_interest);
            string p_ccFee = G1.ReformatMoney(ccFee);
            //string p_retained = G1.ReformatMoney(retained);

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("description");
            dt.Columns.Add("lastpayment");
            dt.Columns.Add("newpayment");

            LoadDataRow(dt, "checkNumber", "Check Number", "");

            LoadDataRow(dt, "dueDate8", "Due Date", dueDate);
            LoadDataRow(dt, "dueDate8", "Next Due Date", dueDate);
            LoadDataRow(dt, "payDate8", "Date Paid", datePaid);
            if (correctedCA)
            {
                //LoadDataRow(dt, "paymentAmount", "Payment", "0.00");
                LoadDataRow(dt, "ap", "Payment", "0.00");
            }
            else
            {
                //LoadDataRow(dt, "paymentAmount", "Payment", pamount);
                LoadDataRow(dt, "ap", "Payment", pamount);
            }
            LoadDataRow(dt, "prince", "Principal", p_principal);
            LoadDataRow(dt, "interestPaid", "Interest", pinterest);

            LoadDataRow(dt, "ccFee", "CC Fee", p_ccFee );

            //LoadDataRow(dt, "retained", "Retained", p_retained);

            if (!insuranceContract)
            {
                if (workDownPayment)
                {
                    LoadDataRow(dt, "downPayment", "Down Payment", pdownPayment);
                }
                LoadDataRow(dt, "trust85P", "Trust85P", p_trust85P);
                LoadDataRow(dt, "trust100P", "Trust100P", p_trust100P);
            }

            if (correctedCA)
            {
                if ( c_principal < 0D)
                {
                    double newPrincipal = Math.Abs(c_principal);
                    LoadDataRow(dt, "debit", "Debit", G1.ReformatMoney(newPrincipal));
                    LoadDataRow(dt, "debitReason", "debit Reason", "Correcting Bad FDLIC Interest Rate");
                }
                else
                {
                    LoadDataRow(dt, "debit", "Debit", "0.00");
                    LoadDataRow(dt, "debitReason", "debit Reason", "");
                }
            }
            else
            {
                LoadDataRow(dt, "debit", "Debit", "0.00");
                LoadDataRow(dt, "debitReason", "Debit Reason", "");
            }

            if ( correctedCA )
            {
                if (c_principal > 0D)
                {
                    double newPrincipal = Math.Abs(c_principal);
                    LoadDataRow(dt, "credit", "Credit", G1.ReformatMoney(newPrincipal));
                    LoadDataRow(dt, "creditReason", "Credit Reason", "Correcting Bad FDLIC Interest Rate");
                }
                else
                {
                    LoadDataRow(dt, "credit", "Credit", "0.00");
                    LoadDataRow(dt, "creditReason", "Credit Reason", "");
                }
            }
            else
            {
                LoadDataRow(dt, "credit", "Credit", "0.00");
                LoadDataRow(dt, "creditReason", "Credit Reason", "");
            }

            if (insuranceContract)
            {
                string refund = "";
                if (editRowData.ToUpper() == "REFUND")
                    refund = "Y";
                LoadDataRow(dt, "refund", "Is Refund", refund );
            }


            LoadDataRow(dt, "location", "Location", "Manual");

            string depositNumber = "I" + date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
            LoadDataRow(dt, "depositNumber", "Deposit Number", depositNumber);
            LoadDataRow(dt, "userId", "User", LoginForm.username);
            LoadDataRow(dt, "agentNumber", "Agent", agent);

            LoadDataRow(dt, "Balance", "Balance", "0.00");
            LoadDataRow(dt, "Credit Balance", "Credit Balance", p_creditBalance);

            double NumPayments = payment / monthlyPayment;
            if (monthlyPayment <= 0D)
                NumPayments = 0D;
            string pNumPayments = G1.ReformatMoney(NumPayments);
            LoadDataRow(dt, "# Payments", "# Payments", pNumPayments);

            LoadDataRow(dt, "unpaid_interest", "Unpaid Interest", p_unpaid_interest);

            LoadDataRow(dt, "bank_account", "Bank Account", bankAccount);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            UpdateInsuranceBalance();

            if (!editing )
                CalculateDetails();
            else
            {
                if (insuranceContract)
                {
                    balanceDue = Policies.CalcMonthlyPremium(payer, DateTime.Now );
                    pbalance = G1.ReformatMoney(balanceDue);
                }

                if (workDownPayment)
                    lblNewBalance.Text = "New Balance :$" + pbalance;
                else
                {
                    double balance = GetMoney("Balance");
                    string balanceStr = G1.ReformatMoney(balance);
                    lblNewBalance.Text = "New Balance :$" + balanceStr;
                }
            }
            if (workDownPayment)
            {
                lblDays.Hide();
                PutMoney("Down Payment", pdownPayment.ObjToDouble(), true);
                PutMoney("Trust85P", p_trust85P.ObjToDouble(), true);
                PutMoney("Trust100P", p_trust100P.ObjToDouble(), true);
                PutMoney("CC Fee", p_ccFee.ObjToDouble(), true);
            }
            PutMoney("Unpaid Interest", old_unpaid_interest, true);

            if ( correctedCA )
            {
                PutMoney("Trust85P", c_trust85.ObjToDouble());
                PutMoney("Trust100P", c_trust100.ObjToDouble());
                PutMoney("interestPaid", c_interest.ObjToDouble());
                PutMoney("paymentAmount", 0D);
                PutMoney("principal", c_principal );
                PutMoney("# Payments", 0D);
                PutMoney("Credit Balance", 0D);
                balanceDue = balanceDue - c_principal;
                balanceDue = G1.RoundValue(balanceDue);
                pbalance = G1.ReformatMoney(balanceDue);
                lblNewBalance.Text = "New Balance :$" + pbalance;

                CalcTrust85Expected(balanceDue, amtOfMonthlyPayt, c_trust85);

                isPaid = false;
            }
            if (editRow != 0)
            {
                double debit = GetMoney("debit");
                if (debit != 0D)
                {
                    string reason = workDt.Rows[editRow]["debitReason"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( reason ))
                        PutData("Debit Reason", reason);
                }
                double credit = GetMoney("credit");
                if (credit != 0D)
                {
                    string reason = workDt.Rows[editRow]["creditReason"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(reason))
                        PutData("Credit Reason", reason);
                }
            }
            if ( workJustTCA )
                PutData("Bank Account", "NONE");
        }
        /***************************************************************************************/
        private void UpdateInsuranceBalance()
        {
            if (insuranceContract && !String.IsNullOrWhiteSpace(payer))
            {
                double sBalance = Policies.CalcMonthlyPremium(payer, DateTime.Now );
                PutMoney("Balance", sBalance, true);
                PutMoney("Balance", sBalance);
            }
        }
        /***************************************************************************************/
        private void LoadDataRow(DataTable dt, string name, string formalName, string answer = "")
        {
            DataRow dRow = dt.NewRow();
            dRow["description"] = formalName;
            if ( formalName == "Payment")
            {
            }
            int backRow = 0;
            int increment = 1;
            if (DailyHistory.reverseSort)
            {
                backRow = workDt.Rows.Count - 1;
                increment = -1;
                if ((editRow + increment) < 0)
                    increment = 0;
            }

            if (G1.get_column_number(workDt, name) >= 0)
            {
                string data = "";
                if (workDt.Rows.Count > 0)
                {
                    data = workDt.Rows[backRow][name].ObjToString();
                    if ( name.ToUpper() == "BALANCE")
                    {
                        double b = data.ObjToDouble();
                        b = G1.RoundValue(b);
                        data = b.ObjToString();
                    }
                    if (editing)
                    {
                        if ((editRow + increment) < workDt.Rows.Count)
                            data = workDt.Rows[editRow + increment][name].ObjToString();
                        else
                            data = "";
                    }

                    if (data.IndexOf(" 00:00:00") > 0)
                        data = data.Replace(" 00:00:00", "");
                    dRow["lastpayment"] = data;
                }
                if (!String.IsNullOrWhiteSpace(answer))
                {
                    if (answer.ToUpper() == "DITTO")
                        answer = data;
                    if (editing)
                        try
                        {
                            answer = workDt.Rows[editRow][name].ObjToString();
                        }
                        catch ( Exception ex )
                        {
                        }
                    dRow["newpayment"] = answer;
                }
            }
            else
            {
                if (answer.ToUpper() == "DITTO")
                    answer = "";
                dRow["newpayment"] = answer;
            }
            dt.Rows.Add(dRow);
        }
        /***************************************************************************************/
        private int FindRow(string desc)
        {
            int row = -1;
            string description = "";
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return row;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                description = dt.Rows[i]["description"].ObjToString().ToUpper();
                if (description == desc.ToUpper())
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /***************************************************************************************/
        private bool CheckIfGoodEntry ( string description )
        {
            bool good = true;
            if (!editing)
                return good;
            if ( description == "PAYMENT")
            {
                if ( paymentType == "DEBIT/CREDIT")
                {
                    good = false;
                    DialogResult result = MessageBox.Show("You are editing a debit or credit!\nYou may not modify the payment!", "Invalid Edit Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    PutMoney("Payment", 0D);
                }
            }
            else if ( description == "DEBIT" || description == "CREDIT")
            {
                if (paymentType == "PAYMENT")
                {
                    good = false;
                    DialogResult result = MessageBox.Show("You are editing a payment!\nYou may not modify the debit or credit!", "Invalid Edit Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    if (description == "DEBIT")
                        PutMoney("Debit", 0D);
                    else if (description == "CREDIT")
                        PutMoney("Credit", 0D);
                }
            }
            return good;
        }
        /***************************************************************************************/
        private string lineChanging = "";
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string description = dr["description"].ObjToString().Trim().ToUpper();
            lineChanging = description;
            bool doit = false;
            if (description == "PAYMENT" || description == "DATE PAID" || description == "CREDIT" || description == "DEBIT" || description == "DUE DATE" || description == "CC FEE")
                doit = true;
            if (description == "PAYMENT" || description == "DATE PAID" || description == "CREDIT" || description == "DEBIT" || description == "DUE DATE" || description == "CC FEE" || description == "INTEREST" )
            {
                if ( description == "CC FEE")
                {
                    if ( !isCCBank() )
                    {
                        PutMoney("CC FEE", 0D);
                        MessageBox.Show("***ERROR*** You must assign a Credit Card Bank to do this!", "Credit Card Fee Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }
                if (!CheckIfGoodEntry(description))
                    return;
                if (description == "CREDIT" || description == "DEBIT")
                    PutMoney("PAYMENT", 0D);
                CalculateDetails();
                if (!DailyHistory.isInsurance(workContract))
                {
                    if (description == "CREDIT")
                    {
                        DialogResult result = MessageBox.Show("***QUESTION*** Should Agent Commissions be paid on this Credit Adjustment?", "Credit Adjustment Question Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == DialogResult.No)
                        {
                            PutData("Agent", "XXX");
                        }
                    }
                }
            }
            else if (description == "# PAYMENTS")
            {
                double numPay = GetMoney("# PAYMENTS");
                if ( insuranceContract )
                {
                    string refund = GetData("Is Refund");
                    if (refund.ToUpper() == "Y")
                    {
                        numPay = 0D;
                        PutMoney("# PAYMENTS", numPay);
                    }
                }
                DateTime date = dueDate.ObjToDateTime();
                date = date.AddMonths(Convert.ToInt32(numPay));
                PutData("Next Due Date", date.ToString("MM/dd/yyyy"));

                numPaymentsChanged = true;
            }
            else if (description == "IS REFUND")
            {
                if (insuranceContract)
                {
                    string refund = GetData("Is Refund");
                    if ( refund.ToUpper() == "YES")
                    {
                        PutData("Is Refund", "Y");
                        refund = "Y";
                    }
                    if (refund.ToUpper() == "Y")
                        PutMoney("# PAYMENTS", 0D);
                    double numPay = GetMoney("# PAYMENTS");
                    if (refund.ToUpper() == "Y")
                        numPay = 0D;
                    DateTime date = dueDate.ObjToDateTime();
                    date = date.AddMonths(Convert.ToInt32(numPay));
                    PutData("Next Due Date", date.ToString("MM/dd/yyyy"));
                }
            }
            else if (description == "PRINCIPAL")
            {
                CalculateNewBalance();
            }
            else if (description == "DOWN PAYMENT")
            {
                CalculateDetails();
            }
        }
        /***************************************************************************************/
        private double GetBalanceDue()
        {
            double balanceDue = 0D;
            string tt = lblBalance.Text;
            int idx = tt.IndexOf("$");
            if (idx > 0)
            {
                tt = tt.Substring(idx + 1);
                tt = tt.Replace(",", "");
                balanceDue = tt.ObjToDouble();
            }
            return balanceDue;
        }
        /***************************************************************************************/
        private void CalculateDetails()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            CalcNewStuff();
        }
        /***************************************************************************************/
        private void ChangeDownPayment ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            double ccFee = GetMoney("CC FEE");
            double dPayment = GetMoney("Down Payment");
            double diff = originalDownPayment - dPayment;
            double trust100P = dPayment;
            double trust85P = dPayment * 0.85D;
            PutMoney("Trust85P", trust85P);
            PutMoney("Trust100P", trust100P);
            PutMoney("Down Payment", dPayment);
            PutMoney("CC Fee", ccFee);
            double balance = balanceDue + diff;
            string pbalance = G1.ReformatMoney(balance);
            lblNewBalance.Text = "New Balance :$" + pbalance;
        }
        /***************************************************************************************/
        private void CalcNewStuff()
        {
            if ( LoginForm.useNewTCACalculation )
            {
                CalcNewStuffNew();
                return;
            }
            if (correctedCA)
                return;
            if ( workDownPayment)
            {
                ChangeDownPayment();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            string str = GetData("Date Paid");
            if (!G1.validate_date(str))
            {
                MessageBox.Show("***ERROR*** Invalid Date Entered ");
                return;
            }
            DateTime date = str.ObjToDateTime();

            double newBalance = 0D;
            double retained = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double payment = GetMoney("Payment");
            double credit = GetMoney("Credit");
            double debit = GetMoney("Debit");
            double amtOfMonthlyPayt = DailyHistory.GetMonthlyPayment(workContract);
            double contractValue = DailyHistory.GetContractValue(workContract);
            double downPayment = DailyHistory.GetDownPayment(workContract);
            double dPayment = GetMoney("Down Payment");
            if (dPayment != originalDownPayment)
                downPayment = dPayment;
            if (!editing)
                dPayment = 0D;
            double rate = apr.ObjToDouble() / 100D;

            if (correctedCA)
                rate = c_apr / 100D;

            int days = -1;

            string datePaid = GetData("Date Paid");
            DateTime docp = datePaid.ObjToDateTime();
            if (editing)
            {
                balanceDue = GetMoney("Balance", true);
                string lastDatePaid = GetData("Date Paid", true);
                if (G1.validate_date(lastDatePaid) && G1.validate_date(datePaid))
                {
                    TimeSpan ts = datePaid.ObjToDateTime() - lastDatePaid.ObjToDateTime();
                    days = (int)(ts.TotalDays);
                }
            }

            double newpayment = dPayment + payment + credit - debit;
            interest = 0D;
            if ( editing)
                interest = GetMoney("Interest");
            if (newpayment != 0D && !editing)
                interest = CalculateInterest(date, rate, balanceDue, days);
            double newprincipal = payment - interest - debit + credit;

            double balance = 0D;

            ImportDailyDeposits.HandleUnpaidInterest(workContract, payment, ref interest, ref unpaid_interest, ref newprincipal, ref balance);

            //if (newprincipal > 0D)
            //    unpaid_interest = 0D;

            //unpaid_interest = 0D;
            //if ( newprincipal < 0D)
            //{
            //    unpaid_interest = Math.Abs(newprincipal);
            //    newprincipal = 0D;
            //    interest = payment;
            //}

            PutMoney("Unpaid Interest", unpaid_interest );

            originalIssueDate = issueDate.ObjToDateTime();
            originalContractValue = contractValue;

            bool gotDebit = false;

            if (debit > 0 && !editing )
            {
                gotDebit = true;
                //newprincipal = debit * -1D;
                newprincipal = debit - interest;
                newprincipal = newprincipal * -1D;
            }

            originalMethod = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, issueDate, contractValue, originalDownPayment, financeDays, payment, newprincipal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);
            //            originalMethod = ImportDailyDeposits.CalcTrust85(issueDate, contractValue, originalDownPayment, financeDays, newpayment, newprincipal, rate, ref trust85P, ref trust100P);
            if (gotDebit && !editing )
            {
                interest = interest * -1D;
            }
            if ( DailyHistory.isRiles ( workContract ))
            {
                trust100P = 0D;
                trust85P = 0D;
                retained = 0D;
                if ( debit == 0D && credit == 0D )
                {
                    //if (trustPercent <= 0D)
                    //    trustPercent = 0.15;
                    trust100P = payment;
                    trust85P = payment * trustPercent / 100D;
                }
            }
            else if ( DailyHistory.gotCemetery ( workContract ))
            {
            }
            int row = FindRow("payment");
            double amount = dt.Rows[row]["newpayment"].ObjToDouble();
            amount = G1.RoundValue(amount);
            string pamount = G1.ReformatMoney(amount);
            dt.Rows[row]["newpayment"] = pamount;

            //row = FindRow("retained");
            //retained = dt.Rows[row]["newpayment"].ObjToDouble();
            //retained = G1.RoundValue(retained);
            //string pretained = G1.ReformatMoney(retained);
            //dt.Rows[row]["newpayment"] = pretained;

            string p_trust85P = G1.ReformatMoney(trust85P);
            row = FindRow("trust85P");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_trust85P;

            string p_trust100P = G1.ReformatMoney(trust100P);
            row = FindRow("trust100P");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_trust100P;

            double principal = payment - interest;
            principal = newprincipal;
            string p_principal = G1.ReformatMoney(principal);
            row = FindRow("principal");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_principal;

            string p_interest = G1.ReformatMoney(interest);
            row = FindRow("interest");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_interest;

            double value = balanceDue - principal - credit + debit;
            value = G1.RoundValue(value);
            newBalance = value;
            string p_balance = G1.ReformatMoney(value);
            lblNewBalance.Text = "New Balance :$" + p_balance;
            PutMoney("balance", value);

            days = DailyHistory.GetDaysSinceLastPayment(workContract, date);
            lblDays.Text = "Days since Last Payment :" + days.ToString();

            double months = 1D;

            //if (!loading && !editing)
            //{
            DateTime nextDueDate = DateTime.Now;
            double creditBalance = 0D;
            if (insuranceContract && 1 == 1)
            {
                DateTime dueDate = GetData("Due Date", false).ObjToDateTime();
                //datePaid = GetData("Date Paid", false).ObjToDateTime();
                double expected = Policies.CalcMonthlyPremium(payer, datePaid.ObjToDateTime() );
                monthlyPayment = expected;
                string lastDueDate = GetData("Due Date", true);
                DateTime dDate = lastDueDate.ObjToDateTime();
                if (gotDebit)
                    principal = Math.Abs(principal);
                months = DailyHistory.CheckMonthsForInsurance(workContract, payer, expected, principal, date, dDate);
                if ( gotDebit )
                {
                    months = months * -1D;
                    string refund = GetData("Is Refund");
                    if (refund.ToUpper() == "Y")
                        months = 0D;
                    int imon = Convert.ToInt32(months);
                    dDate = dDate.AddMonths(imon);
                }
                if (CalcNextDueDate(newpayment, expected, ref nextDueDate, ref creditBalance, ref months))
                {
                    row = FindRow("Next Due Date");
                    dt.Rows[row]["newpayment"] = nextDueDate.ToString("MM/dd/yyyy");
                    if (newBalance <= 0D && !insuranceContract)
                        dt.Rows[row]["newpayment"] = "12/31/2039";

                    string cBalance = G1.ReformatMoney(creditBalance);
                    row = FindRow("Credit Balance");
                    if (row >= 0)
                        dt.Rows[row]["newpayment"] = cBalance;
                }
            }
            else
            {
                if (CalcNextDueDate(newpayment, value, ref nextDueDate, ref creditBalance, ref months))
                {
                    row = FindRow("Next Due Date");
                    dt.Rows[row]["newpayment"] = nextDueDate.ToString("MM/dd/yyyy");
                    if (newBalance <= 0D && !insuranceContract)
                        dt.Rows[row]["newpayment"] = "12/31/2039";

                    string cBalance = G1.ReformatMoney(creditBalance);
                    row = FindRow("Credit Balance");
                    if (row >= 0)
                        dt.Rows[row]["newpayment"] = cBalance;
                }
            }
            //            }

            if (!insuranceContract)
            {
                double NumPayments = payment / monthlyPayment;
                if (months > NumPayments)
                    NumPayments = months;
                else if (debit > 0D && months < 0)
                    NumPayments = months;
                PutMoney("# Payments", NumPayments);
            }
            else
            {
                string refund = GetData("Is Refund");
                if (refund.ToUpper() == "Y")
                    months = 0D;
                PutMoney("# Payments", months);
            }

            UpdateInsuranceBalance();

            maxTrust85 = 0D;
            totalTrust85 = 0D;
            isPaid = false;
            bool trustThreshold = false;
            bool balanceThreshold = false;
            //if (!workContract.Contains("LI"))
            //{
                if (!insuranceContract && newBalance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
                    isPaid = Customers.CheckForcedPayoff(workContract, amtOfMonthlyPayt, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, ref trustThreshold, ref balanceThreshold, trust85P);
            //}


            double Trust85Paid = workTrust85C + trust85P;
            difference = maxTrust85 - totalTrust85;
            difference = G1.RoundValue(difference);
            difference = maxTrust85 - Trust85Paid;
            difference = G1.RoundValue(difference);
            if (difference < 0D)
            {
                difference = 0D;
                isPaid = false;
            }

            txtTrust85Paid.Text = "$" + G1.ReformatMoney(Trust85Paid);
            if ( isPaid )
            {
                txtTrust85Paid.ForeColor = Color.White;
                txtTrust85Paid.BackColor = Color.Pink;
                if (isPaid)
                {
                    txtTrust85Paid.BackColor = Color.LimeGreen;
                    PutData("Next Due Date", "12/31/2039");
                    row = FindRow("trust85P");
                    if (row >= 0)
                    {
                        trust85P = trust85P + difference;
                        if (trust85P <= 0D)
                            trust85P = 0D;
                        p_trust85P = G1.ReformatMoney(trust85P);
                        dt.Rows[row]["newpayment"] = p_trust85P;
                        txtTrust85Paid.Text = "$" + G1.ReformatMoney(totalTrust85 + difference);
                        row = FindRow("trust100P");
                        {
                            trust100P = trust85P / 0.85D;
                            trust100P = G1.RoundValue(trust100P);
                            p_trust100P = G1.ReformatMoney(trust100P);
                            dt.Rows[row]["newpayment"] = p_trust100P;
                        }
                    }
                    txtTrust85Paid.Refresh();
                }
            }
            else
            {
                txtTrust85Paid.BackColor = Color.White;
                txtTrust85Paid.ForeColor = Color.Black;
                txtTrust85Paid.Refresh();
            }

            string PaymentDetil = " of " + G1.ReformatMoney(workTrust85Max) + " Expected";
            if (difference != 0D && isPaid)
            {
                difference = G1.RoundValue(difference);
                double credit100P = difference / 0.85D;
                credit100P = G1.RoundValue(credit100P);
                PaymentDetil += "   Payoff Trust Credit : " + G1.ReformatMoney(difference) + " / " + G1.ReformatMoney(credit100P);
            }
            label2.Text = PaymentDetil;

            //lblCalcTrust85.Text = "Trust85C: $" + G1.ReformatMoney(Trust85Paid) + " of " + G1.ReformatMoney(workTrust85Max) + " Expected";
        }
        /***************************************************************************************/
        private void CalcNewStuffNew()
        {
            if (correctedCA)
                return;
            if (workDownPayment)
            {
                ChangeDownPayment();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dr = gridMain.GetFocusedDataRow();
            string description = dr["description"].ObjToString().Trim().ToUpper();

            string str = GetData("Date Paid");
            if (!G1.validate_date(str))
            {
                MessageBox.Show("***ERROR*** Invalid Date Entered ");
                return;
            }
            DateTime date = str.ObjToDateTime();

            double newBalance = 0D;
            double retained = 0D;
            double trust85P = 0D;
            double trust100P = 0D;

            double payment = GetMoney("Payment");
            if (workJustTCA)
            {
                payment = 0D;
                PutMoney("Payment", payment);
            }

            double debit = 0D;
            double ccFee = GetMoney("CC Fee");
            if ( ccFee < 0D )
            {
                debit = GetMoney("Debit");
                if (debit == 0D)
                {
                    PutMoney("Payment", 0D);
                    PutMoney("Trust85P", 0D);
                    PutMoney("Trust100P", 0D);
                    PutMoney("# Payments", 0D);
                    PutMoney("Principal", 0D);
                    PutMoney("Debit", ccFee * -1D);
                }
                return;
            }
            double fee = 0D;
            bool isCC = isCCBank();
            bool doit = false;
            if (description == "PAYMENT" || ccFee == 0D)
                doit = true;
            if (description == "CC FEE")
                doit = false;
            if (isCC && doit )
            {
                fee = GetCCFee(feeDt, workContract, payer );
                if (fee > 0D)
                {
                    fee += 1D;
                    ccFee = payment / fee;
                    ccFee = G1.RoundValue(ccFee);
                    ccFee = payment - ccFee;
                    ccFee = G1.RoundValue(ccFee);

                }
            }

            payment -= ccFee;
            payment = G1.RoundValue(payment);

            PutMoney("CC Fee", ccFee );

            double credit = GetMoney("Credit");
            debit = GetMoney("Debit");
            double amtOfMonthlyPayt = DailyHistory.GetMonthlyPayment(workContract);
            if (workJustTCA)
                amtOfMonthlyPayt = 0D;
            double contractValue = DailyHistory.GetContractValue(workContract);
            double downPayment = DailyHistory.GetDownPayment(workContract);
            double dPayment = GetMoney("Down Payment");
            if (dPayment != originalDownPayment)
                downPayment = dPayment;
            if (!editing)
                dPayment = 0D;
            double rate = apr.ObjToDouble() / 100D;

            if (correctedCA)
                rate = c_apr / 100D;

            int days = -1;

            string datePaid = GetData("Date Paid");
            DateTime docp = datePaid.ObjToDateTime();
            if (editing)
            {
                balanceDue = GetMoney("Balance", true);
                string lastDatePaid = GetData("Date Paid", true);
                if (G1.validate_date(lastDatePaid) && G1.validate_date(datePaid))
                {
                    TimeSpan ts = datePaid.ObjToDateTime() - lastDatePaid.ObjToDateTime();
                    days = (int)(ts.TotalDays);
                }
            }

            double newpayment = dPayment + payment + credit - debit;
            interest = 0D;
            if (editing)
                interest = GetMoney("Interest");
            else if (debit != 0D)
                interest = GetMoney("Interest");
            else if (credit != 0D)
                interest = GetMoney("Interest");

            if (newpayment != 0D && !editing)
            {
                if ( lineChanging.ToUpper() == "INTEREST")
                {
                    if ( credit == 0D && debit == 0D && interest != 0D )
                        interest = CalculateInterest(date, rate, balanceDue, days);
                }
                else
                    interest = CalculateInterest(date, rate, balanceDue, days);
            }
            double newprincipal = payment - interest - debit + credit;

            double balance = 0D;

            if ( debit == 0D )
                ImportDailyDeposits.HandleUnpaidInterest(workContract, payment, ref interest, ref unpaid_interest, ref newprincipal, ref balance);
            if (debit != 0D)
            {
                if ( interest > 0D )
                    interest = interest * -1D;
            }

            //if (newprincipal > 0D)
            //    unpaid_interest = 0D;

            //unpaid_interest = 0D;
            //if ( newprincipal < 0D)
            //{
            //    unpaid_interest = Math.Abs(newprincipal);
            //    newprincipal = 0D;
            //    interest = payment;
            //}

            PutMoney("Unpaid Interest", unpaid_interest);

            originalIssueDate = issueDate.ObjToDateTime();
            originalContractValue = contractValue;

            bool gotDebit = false;

            if (debit > 0 && !editing)
            {
                gotDebit = true;
                //newprincipal = debit * -1D;
                if (interest > 0D)
                {
                    newprincipal = debit - interest;
                    newprincipal = newprincipal * -1D;
                }
                else
                    newprincipal = 0D;
            }

            originalMethod = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, issueDate, contractValue, originalDownPayment, financeDays, payment, newprincipal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);
            //            originalMethod = ImportDailyDeposits.CalcTrust85(issueDate, contractValue, originalDownPayment, financeDays, newpayment, newprincipal, rate, ref trust85P, ref trust100P);
            if (gotDebit && !editing)
            {
                if ( interest > 0D )
                    interest = interest * -1D;
            }
            if (DailyHistory.isRiles(workContract))
            {
                trust100P = 0D;
                trust85P = 0D;
                retained = 0D;
                if (debit == 0D && credit == 0D)
                {
                    //if (trustPercent <= 0D)
                    //    trustPercent = 15D;
                    trust100P = payment;
                    trust85P = payment * trustPercent / 100D;
                }
            }
            else if ( DailyHistory.gotCemetery ( workContract ))
            {
                trust100P = 0D;
                trust85P = 0D;
                retained = 0D;
                if (debit == 0D && credit == 0D)
                {
                    if (trustPercent <= 0D)
                        trustPercent = 15D;
                    trust100P = payment * trustPercent / 100D;
                    trust85P = payment * trustPercent / 100D;
                }
            }
            else if ( workJustTCA )
            {
                trust85P = 0D;
                trust100P = 0D;
            }

            int row = FindRow("payment");
            double amount = dt.Rows[row]["newpayment"].ObjToDouble();
            amount = G1.RoundValue(amount);
            string pamount = G1.ReformatMoney(amount);
            dt.Rows[row]["newpayment"] = pamount;

            //row = FindRow("retained");
            //retained = dt.Rows[row]["newpayment"].ObjToDouble();
            //retained = G1.RoundValue(retained);
            //string pretained = G1.ReformatMoney(retained);
            //dt.Rows[row]["newpayment"] = pretained;

            string p_trust85P = G1.ReformatMoney(trust85P);
            row = FindRow("trust85P");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_trust85P;

            string p_trust100P = G1.ReformatMoney(trust100P);
            row = FindRow("trust100P");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_trust100P;

            double principal = payment - interest;
            principal = newprincipal;
            string p_principal = G1.ReformatMoney(principal);
            row = FindRow("principal");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_principal;

            string p_interest = G1.ReformatMoney(interest);
            row = FindRow("interest");
            if (row >= 0)
                dt.Rows[row]["newpayment"] = p_interest;

            double value = balanceDue - principal - credit + debit;
            value = G1.RoundValue(value);
            if (DailyHistory.gotCemetery(workContract))
                value = 0D;
            newBalance = value;
            string p_balance = G1.ReformatMoney(value);
            lblNewBalance.Text = "New Balance :$" + p_balance;
            PutMoney("balance", value);

            days = DailyHistory.GetDaysSinceLastPayment(workContract, date);
            lblDays.Text = "Days since Last Payment :" + days.ToString();

            double months = 1D;

            //if (!loading && !editing)
            //{
            DateTime nextDueDate = DateTime.Now;
            double creditBalance = 0D;
            if (insuranceContract && 1 == 1)
            {
                DateTime dueDate = GetData("Due Date", false).ObjToDateTime();
                //datePaid = GetData("Date Paid", false).ObjToDateTime();
                double expected = Policies.CalcMonthlyPremium(payer, datePaid.ObjToDateTime());
                monthlyPayment = expected;
                string lastDueDate = GetData("Due Date", true);
                DateTime dDate = lastDueDate.ObjToDateTime();
                dDate = dueDate;
                if (gotDebit)
                    principal = Math.Abs(principal);
                months = DailyHistory.CheckMonthsForInsurance(workContract, payer, expected, principal, date, dDate);
                if (gotDebit)
                {
                    months = months * -1D;
                    string refund = GetData("Is Refund");
                    if (refund.ToUpper() == "Y")
                        months = 0D;
                    int imon = Convert.ToInt32(months);
                    dDate = dDate.AddMonths(imon);
                }
                if (CalcNextDueDate(newpayment, expected, ref nextDueDate, ref creditBalance, ref months))
                {
                    row = FindRow("Next Due Date");
                    dt.Rows[row]["newpayment"] = nextDueDate.ToString("MM/dd/yyyy");
                    if (newBalance <= 0D && !insuranceContract)
                        dt.Rows[row]["newpayment"] = "12/31/2039";

                    string cBalance = G1.ReformatMoney(creditBalance);
                    row = FindRow("Credit Balance");
                    if (row >= 0)
                        dt.Rows[row]["newpayment"] = cBalance;
                }
            }
            else
            {
                DateTime lastDueDate = FindMismatches.VerifyDueDate(workContract);

                double actualMonthPaid = newpayment / monthlyPayment;

                double dueDatePayment = payment + runningCB;
                double newActualMonthPaid = dueDatePayment / monthlyPayment;
                if ((runningCB + (payment % monthlyPayment)) > monthlyPayment)
                    actualMonthPaid += 1D;
                if ( lineChanging.ToUpper() == "INTEREST")
                {
                    if (credit > 0D && interest == 0D)
                        actualMonthPaid -= 1D;
                    if (actualMonthPaid < 0D)
                        actualMonthPaid = 0D;
                }
                payment = dueDatePayment;

                if (CalcNextDueDate(newpayment, value, ref nextDueDate, ref creditBalance, ref months))
                {
                    int iMonth = (int) Math.Truncate(actualMonthPaid);
                    nextDueDate = lastDueDate.AddMonths(iMonth);
                    row = FindRow("Next Due Date");
                    dt.Rows[row]["newpayment"] = nextDueDate.ToString("MM/dd/yyyy");
                    if (newBalance <= 0D && !insuranceContract)
                        dt.Rows[row]["newpayment"] = "12/31/2039";

                    string cBalance = G1.ReformatMoney(creditBalance);
                    row = FindRow("Credit Balance");
                    if (row >= 0)
                        dt.Rows[row]["newpayment"] = cBalance;
                }
            }
            //            }

            if (!insuranceContract)
            {
                double NumPayments = payment / monthlyPayment;
                if (monthlyPayment <= 0D)
                    NumPayments = 0D;
                if (months > NumPayments)
                    NumPayments = months;
                else if (debit > 0D && months < 0)
                    NumPayments = months;
                PutMoney("# Payments", NumPayments);
            }
            else
            {
                PutMoney("# Payments", months);
            }

            UpdateInsuranceBalance();

            maxTrust85 = 0D;
            totalTrust85 = 0D;
            isPaid = false;
            bool trustThreshold = false;
            bool balanceThreshold = false;

            if (!insuranceContract && newBalance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
                isPaid = Customers.CheckForcedPayoff(workContract, amtOfMonthlyPayt, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, ref trustThreshold, ref balanceThreshold, trust85P);


            double Trust85Paid = workTrust85C + trust85P;
            if (workJustTCA)
            {
                Trust85Paid = maxTrust85 - workTrust85C;
                Trust85Paid = G1.RoundValue(Trust85Paid);
                PutMoney("Trust85P", Trust85Paid);
                double t100 = Trust85Paid / 0.85D;
                t100 = G1.RoundValue(t100);
                PutMoney("Trust100P", t100 );
            }
            difference = maxTrust85 - totalTrust85;
            difference = G1.RoundValue(difference);
            difference = maxTrust85 - Trust85Paid;
            difference = G1.RoundValue(difference);
            if (!isPaid)
                difference = 0D;
            string payOffMethod = LoginForm.allowPayOffMethod;
            if ( String.IsNullOrWhiteSpace (payOffMethod ) )
            {
                difference = 0D;
                isPaid = false;
            }
            if ( isPaid && payOffMethod.ToUpper() == "DEBIT")
            {
                if ( difference > 0D)
                {
                    difference = 0D;
                    isPaid = false;
                }
            }
            else if ( isPaid && payOffMethod.ToUpper() == "CREDIT")
            {
                if (difference < 0D)
                {
                    difference = 0D;
                    //isPaid = false;
                }
            }
            //if (difference < 0D)
            //{
            //    difference = 0D;
            //    isPaid = false;
            //}

            txtTrust85Paid.Text = "$" + G1.ReformatMoney(Trust85Paid);
            if (isPaid)
            {
                txtTrust85Paid.ForeColor = Color.White;
                txtTrust85Paid.BackColor = Color.Pink;
                if (isPaid)
                {
                    txtTrust85Paid.BackColor = Color.LimeGreen;
                    PutData("Next Due Date", "12/31/2039");
                    row = FindRow("trust85P");
                    if (row >= 0)
                    {
                        //    trust85P = trust85P + difference;
                        //    if (trust85P <= 0D)
                        //        trust85P = 0D;
                        //    p_trust85P = G1.ReformatMoney(trust85P);
                        //    dt.Rows[row]["newpayment"] = p_trust85P;
                        if ( workJustTCA )
                            txtTrust85Paid.Text = "$" + G1.ReformatMoney( maxTrust85 );
                        else
                            txtTrust85Paid.Text = "$" + G1.ReformatMoney(totalTrust85 + difference);
                        //    row = FindRow("trust100P");
                        //    {
                        //        trust100P = trust85P / 0.85D;
                        //        trust100P = G1.RoundValue(trust100P);
                        //        p_trust100P = G1.ReformatMoney(trust100P);
                        //        dt.Rows[row]["newpayment"] = p_trust100P;
                        //    }
                    }
                    txtTrust85Paid.Refresh();
                }
            }
            else
            {
                txtTrust85Paid.BackColor = Color.White;
                txtTrust85Paid.ForeColor = Color.Black;
                txtTrust85Paid.Refresh();
            }

            string PaymentDetil = " of " + G1.ReformatMoney(workTrust85Max) + " Expected";
            if (difference != 0D && isPaid)
            {
                difference = G1.RoundValue(difference);
                double credit100P = difference / 0.85D;
                credit100P = G1.RoundValue(credit100P);
                PaymentDetil += "   Payoff Trust Credit : " + G1.ReformatMoney(difference) + " / " + G1.ReformatMoney(credit100P);
            }
            else if ( workPDF )
            {
                if (editRow >= workDt.Rows.Count)
                    editRow = workDt.Rows.Count - 1;
                if (editRow >= 0)
                {
                    trust100P = workDt.Rows[editRow]["trust100P"].ObjToDouble();
                    trust85P = workDt.Rows[editRow]["trust85P"].ObjToDouble();
                    PutMoney("Trust100P", trust100P);
                    PutMoney("Trust85P", trust85P);
                    PaymentDetil += "   Payoff Trust Credit : " + G1.ReformatMoney(trust85P) + " / " + G1.ReformatMoney(trust100P);
                }
            }
            label2.Text = PaymentDetil;

            if (workJustTCA)
            {
                interest = GetMoney("Trust100P");
                interest = interest * -1D;
                PutMoney("Interest", interest);
            }

            if ( ccFee < 0D )
            {
            }

            //lblCalcTrust85.Text = "Trust85C: $" + G1.ReformatMoney(Trust85Paid) + " of " + G1.ReformatMoney(workTrust85Max) + " Expected";
        }
        /***************************************************************************************/
        private bool isCCBank ()
        {
            string bank = GetData("Bank Account");

            if (String.IsNullOrWhiteSpace(bank))
                return false;

            string ledger = "";
            string account = "";
            string cmd = "";
            DataTable dt = null;

            string[] Lines = bank.Split('~');
            if (Lines.Length < 3)
            {
                if ( Lines.Length < 1 )
                    return false;
                account = Lines[0].Trim();
                cmd = "Select * from `bank_accounts` where `account_no` = '" + account + "';";
                dt = G1.get_db_data(cmd);
            }
            else
            {
                ledger = Lines[1].Trim();
                account = Lines[2].Trim();
                cmd = "Select * from `bank_accounts` where `general_ledger_no` = '" + ledger + "' and `account_no` = '" + account + "';";
                dt = G1.get_db_data(cmd);
            }


            if (dt.Rows.Count <= 0)
                return false;
            string title = dt.Rows[0]["account_title"].ObjToString();
            if (title.ToUpper().IndexOf("CREDIT CARD") > 0)
            {
                //double ccFee = GetCCFee( feeDt );
                return true;
            }

            return false;
        }
        /***********************************************************************************************/
        public static DataTable LoadCCFeeTable()
        {
            DateTime date = DateTime.Now;
            DataTable dt = G1.get_db_data("Select * from `creditcard_fees` ORDER BY `beginDate` DESC;");
            dt.Columns.Add("bDate");
            dt.Columns.Add("eDate");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["endDate"].ObjToDateTime();
                if (date.Year <= 1000)
                    dt.Rows[i]["endDate"] = G1.DTtoMySQLDT(DateTime.Now);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["beginDate"].ObjToDateTime();
                dt.Rows[i]["bDate"] = date.ToString("yyyyMMdd");

                date = dt.Rows[i]["endDate"].ObjToDateTime();
                dt.Rows[i]["eDate"] = date.ToString("yyyyMMdd");
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "endDate DESC";
            dt = tempview.ToTable();
            return dt;
        }
        /***********************************************************************************************/
        public static double GetCCFee ( DataTable feeDt, string workContract, string payer )
        {
            if (feeDt == null)
                return 0D;
            if (feeDt.Rows.Count <= 0)
                return 0D;

            string allowFee = "";

            string lookup = workContract;
            if (!String.IsNullOrWhiteSpace(payer))
                lookup = payer;
            string cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + lookup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                allowFee = dt.Rows[0]["allowFee"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(allowFee))
                    return 0D;
                if (allowFee == "N")
                    return 0D;
            }

            double ccFee = 0D;
            try
            {
                DataRow[] dRows = feeDt.Select("eDate>='" + DateTime.Now.ToString("yyyyMMdd") + "'");
                if (dRows.Length > 0)
                {
                    //DataTable dd = dRows.CopyToDataTable();
                    ccFee = dRows[0]["fee"].ObjToDouble();
                    ccFee = ccFee / 100D;
                    ccFee = G1.RoundValue(ccFee);
                }
            }
            catch (Exception ex)
            {
            }
            return ccFee;
        }
        /***************************************************************************************/
        private void CalcTrust85Expected ( double newBalance, double amtOfMonthlyPayt, double trust85P )
        {
            if (insuranceContract)
                return;
            maxTrust85 = 0D;
            totalTrust85 = 0D;
            isPaid = false;
            bool trustThreshold = false;
            bool balanceThreshold = false;
            double contractValue = 0D;
            //if ( newBalance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
            //if (!workContract.Contains("LI"))
                isPaid = Customers.CheckForcedPayoff(workContract, amtOfMonthlyPayt, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, ref trustThreshold, ref balanceThreshold, workTrust85C);


            double Trust85Paid = workTrust85C + trust85P;
            difference = maxTrust85 - totalTrust85;
            difference = G1.RoundValue(difference);
            difference = maxTrust85 - Trust85Paid;
            difference = G1.RoundValue(difference);
            //if (difference < 0D)
            //    difference = 0D;

            txtTrust85Paid.Text = "$" + G1.ReformatMoney(Trust85Paid);
            //if (Trust85Paid >= workTrust85Max)
            if (isPaid)
            {
                txtTrust85Paid.ForeColor = Color.White;
                txtTrust85Paid.BackColor = Color.Pink;
                if (isPaid)
                {
                    txtTrust85Paid.BackColor = Color.LimeGreen;
                    PutData("Next Due Date", "12/31/2039");
                }
                txtTrust85Paid.Refresh();
            }
            else
            {
                txtTrust85Paid.BackColor = Color.White;
                txtTrust85Paid.ForeColor = Color.Black;
                txtTrust85Paid.Refresh();
            }
            label2.Text = " of " + G1.ReformatMoney(workTrust85Max) + " Expected";

            if ( Trust85Paid < workTrust85Max )
            {
                string lastDueDate = GetData("Due Date", true);

                string DatePaid = GetData("Date Paid", true);
                PutData("Date Paid", DateTime.Now.ToString("MM/dd/yyyy"));

                PutData("Due Date", DatePaid);
                PutData("Next Due Date", DatePaid);
                txtTrust85Paid.ForeColor = Color.White;
                txtTrust85Paid.BackColor = Color.Pink;
            }
        }
        /***************************************************************************************/
        private double ReformatAmount(string field)
        {
            double rv = 0D;
            DataTable dt = (DataTable)dgv.DataSource;
            int row = FindRow(field);
            string str = dt.Rows[row]["newpayment"].ObjToString();
            if (G1.validate_numeric(str))
            {
                double amount = dt.Rows[row]["newpayment"].ObjToDouble();
                amount = G1.RoundValue(amount);
                string pamount = G1.ReformatMoney(amount);
                dt.Rows[row]["newpayment"] = pamount;
                rv = amount;
            }
            return rv;
        }
        /***************************************************************************************/
        private void CalculateNewBalance()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            double payment = GetMoney("Payment");
            double credit = GetMoney("Credit");
            double debit = GetMoney("Debit");
            double principal = GetMoney("Principal");
            double newpayment = principal + credit - debit;

            double value = balanceDue - principal - credit + debit;
            value = G1.RoundValue(value);
            string p_balance = G1.ReformatMoney(value);
            lblNewBalance.Text = "New Balance :$" + p_balance;
            PutMoney("balance", value);
        }
        /***********************************************************************************************/
        public static bool IsLastPayment(string contract, string record, ref DateTime lastDatePaid8)
        {
            bool rv = false;
            string paymentFile = "payments";
            if (contract.ToUpper().IndexOf("ZZ") == 0)
                paymentFile = "ipayments";
            string cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + contract + "' and `fill` <> 'D' ORDER by `record` DESC LIMIT 2;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string rec = dx.Rows[0]["record"].ObjToString();
                if (rec == record)
                {
                    if (dx.Rows.Count > 1)
                        lastDatePaid8 = dx.Rows[1]["payDate8"].ObjToDateTime();
                    rv = true;
                }
            }
            return rv;
        }
        /***************************************************************************************/
        public static bool ReversePayment(string contract, string record, string reason)
        {
            DateTime lastPaid8 = DateTime.Now;
            bool lastPayment = IsLastPayment(contract, record, ref lastPaid8);

            string paymentFile = "payments";
            string contractFile = "contracts";
            string payer = "";
            string cmd = "";
            DataTable dt = null;
            if (contract.ToUpper().IndexOf("ZZ") == 0)
            {
                paymentFile = "ipayments";
                contractFile = "icontracts";
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contract + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    payer = dt.Rows[0]["payer"].ObjToString();
            }

            cmd = "Select * from `" + paymentFile + "` where `record` = '" + record + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            string cnum = dt.Rows[0]["contractNumber"].ObjToString();
            double interest = dt.Rows[0]["interestPaid"].ObjToDouble();
            double payment = dt.Rows[0]["paymentAmount"].ObjToDouble();
            double credit = dt.Rows[0]["creditAdjustment"].ObjToDouble();
            double debit = dt.Rows[0]["debitAdjustment"].ObjToDouble();
            DateTime payDate = dt.Rows[0]["payDate8"].ObjToDateTime();
            double ccFee = dt.Rows[0]["ccFee"].ObjToDouble();

            double principal = payment - interest;

            cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + cnum + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return false;

            string cRecord = ddx.Rows[0]["record"].ObjToString();

            double expectedPayment = ddx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            int numberOfPayments = (int)(payment / expectedPayment);

            double balanceDue = ddx.Rows[0]["balanceDue"].ObjToDouble();
            double originalBalance = balanceDue;
            double totalInterest = ddx.Rows[0]["totalInterest"].ObjToDouble();

            //            sBalance - principal + debit - credit; // Normal Calc

            balanceDue = balanceDue + principal + credit - debit;
            balanceDue = G1.RoundValue(balanceDue);
            totalInterest = totalInterest - interest;
            balanceDue = G1.RoundValue(balanceDue);
            totalInterest = G1.RoundValue(totalInterest);

            string dueDate8 = ddx.Rows[0]["dueDate8"].ObjToString();
            string lastDatePaid8 = ddx.Rows[0]["lastDatePaid8"].ObjToString();
            if (numberOfPayments >= 1)
            {
                int reversePay = (-1) * numberOfPayments; // This is good as is, I think
                DateTime dd = dueDate8.ObjToDateTime();
                dd = dd.AddMonths(reversePay);
                dueDate8 = dd.Month.ToString("D2") + "/" + dd.Day.ToString("D2") + "/" + dd.Year.ToString("D4");
                dueDate8 = G1.DTtoMySQLDT(dueDate8).ObjToString();

                dd = lastDatePaid8.ObjToDateTime();
                if (lastPayment)
                    dd = lastPaid8;
                dd = dd.AddMonths(reversePay);
                lastDatePaid8 = dd.Month.ToString("D2") + "/" + dd.Day.ToString("D2") + "/" + dd.Year.ToString("D4");
                lastDatePaid8 = G1.DTtoMySQLDT(lastDatePaid8).ObjToString();
                if (lastDatePaid8.IndexOf("0000") >= 0)
                    lastDatePaid8 = dueDate8;
            }



            //            G1.update_db_table("contracts", "record", cRecord, new string[] { "balanceDue", balanceDue.ToString(), "totalInterest", totalInterest.ToString(), "dueDate8", dueDate8, "lastDatePaid8", lastDatePaid8 });
            G1.update_db_table(contractFile, "record", cRecord, new string[] { "balanceDue", balanceDue.ToString() });
            if (lastPayment)
            {
                G1.update_db_table(contractFile, "record", cRecord, new string[] { "dueDate8", dueDate8, "lastDatePaid8", lastDatePaid8 });
                if (!String.IsNullOrWhiteSpace(payer))
                    ManualPayment.UpdatePayer(payer, dueDate8.ObjToDateTime(), lastDatePaid8.ObjToDateTime());
            }


            string what = "Cnum:" + cnum + " PayDate:" + payDate.ToString("MM/dd/yyyy") + " Payment: " + payment.ToString() + " Original Balance Due: " + originalBalance.ToString();
            G1.AddToAudit(LoginForm.username, "Remove/Reverse Payment", paymentFile, what, cnum);

            G1.update_db_table(paymentFile, "record", record, new string[] { "fill", "D", "debitReason", reason });
            return true;
        }
        /***********************************************************************************************/
        public static void UpdatePayer ( string payer, DateTime dueDate8, DateTime DOLP, bool isContract = false )
        {
            if (String.IsNullOrWhiteSpace(payer))
                return;
            try
            {
                string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                if (isContract)
                    cmd = "Select * from `payers` where `contractNumber` = '" + payer + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "dueDate8", dueDate8.ToString("MM/dd/yyyy"), "lastDatePaid8", DOLP.ToString("MM/dd/yyyy") });
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static void UpdatePayer(string payer, DateTime dueDate8, DateTime DOLP, double creditBalance, bool isContract = false )
        {
            if (String.IsNullOrWhiteSpace(payer))
                return;
            try
            {
                string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                if (isContract)
                    cmd = "Select * from `payers` where `contractNumber` = '" + payer + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "dueDate8", dueDate8.ToString("MM/dd/yyyy"), "lastDatePaid8", DOLP.ToString("MM/dd/yyyy"), "creditBalance", creditBalance.ToString() });
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static void UpdatePayer(string payer, string firstName, string lastName, bool isContract = false )
        {
            if (String.IsNullOrWhiteSpace(payer))
                return;
            try
            {
                string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                if (isContract)
                    cmd = "Select * from `payers` where `contractNumber` = '" + payer + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "firstName", firstName, "lastName", lastName });
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static void UpdatePayer(string payer, DateTime deceasedDate, bool isContract = false )
        {
            if (String.IsNullOrWhiteSpace(payer))
                return;
            try
            {
                string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                if (isContract)
                    cmd = "Select * from `payers` where `contractNumber` = '" + payer + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "deceasedDate", deceasedDate.ToString("MM/dd/yyyy") });
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static void UpdatePayer(string payer, double premium, double annualPremium, bool isContract = false )
        {
            if (String.IsNullOrWhiteSpace(payer))
                return;
            try
            {
                string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
                if ( isContract )
                    cmd = "Select * from `payers` where `contractNumber` = '" + payer + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string record = dt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("payers", "record", record, new string[] { "amtOfMonthlyPayt", premium.ToString(), "annualPremium", annualPremium.ToString() });
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 150, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            bool topmost = this.TopMost;
            if (this.TopMost == true)
                this.TopMost = false;
            //G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if (workPDF)
            {
                string filename = @"c:/rag/Manual.pdf";
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                printableComponentLink1.ExportToPdf(filename);
            }
            else
            {
                printableComponentLink1.ShowPreviewDialog(this);
            }
            if (topmost)
                this.TopMost = true;
            //            printableComponentLink1.ShowPreview();

           // G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 150, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16, FontStyle.Regular );
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);
            //Printer.DrawQuadBorder(1, 1, 12, 6, BorderSide.All, 1, Color.Black);
            //Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            font = new Font("Ariel", 8, FontStyle.Regular);
            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

            Printer.SetQuadSize(24, 12);
            Printer.DrawQuad(1, 2, 4, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //Printer.SetQuadSize(24, 12);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = "Manual Payment Report";
            if (correctedCA)
            {
                title += " (CA Correction)";
                Printer.DrawQuad(12, 3, 8, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            else if (TrustPaid.ToUpper() == "YES")
            {
                title += " (Trust Paid-Off Adjustment)";
                Printer.DrawQuad(9, 3, 14, 2, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            else
                Printer.DrawQuad(12, 3, 8, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(5, 2, 8, 3, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            string name = workName;
            if (insuranceContract)
                name += " (" + payer + ")";

            font = new Font("Ariel", 7, FontStyle.Regular);
            Printer.DrawQuad(1, 4, 7, 1, "Contract :" + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(4, 4, 5, 1, "Name :" + name, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
            //Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            font = new Font("Ariel", 8, FontStyle.Regular);
            Printer.DrawQuad(1, 6, 6, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 8, 6, 2, lblDays.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            string totalPaidLine = lblNewBalance.Text;
            if (isPaid)
                totalPaidLine += "    " + txtTrust85Paid.Text + label2.Text;
            Printer.DrawQuad(1, 10, 18, 2, totalPaidLine, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

           // Printer.DrawQuad(1, 10, 6, 3, label2.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);

            Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.Bottom, 1, Color.Black);
            ////            Printer.DrawQuadTicks();
        }
        /****************************************************************************************/
        private void LoadTestData(ref double newBalance, ref double totalInt)
        {
            string cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("debit", Type.GetType("System.Double"));
            dt.Columns.Add("credit", Type.GetType("System.Double"));
            dt.Columns.Add("prince", Type.GetType("System.Double"));
            dt.Columns.Add("reason");
            DateTime dueDate = DateTime.Now;
            DateTime payDate = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (dueDate.Year.ToString("D4").IndexOf("000") >= 0)
                    dt.Rows[i]["dueDate8"] = dt.Rows[i]["payDate8"];
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "dueDate8 desc";
            //            tempview.Sort = "loc asc, agentName asc";
            dt = tempview.ToTable();

            double startBalance = DailyHistory.GetFinanceValue(workContract);
            double sBalance = startBalance;
            string status = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                double payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                double debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                double credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                status = dt.Rows[i]["fill"].ObjToString();
            }
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                double payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                double debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                double credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                double interest = dt.Rows[i]["interestPaid"].ObjToString().ObjToDouble();
                double principal = payment - interest;
                double balance = sBalance - principal + debit - credit;
                if (status.ToUpper() == "D")
                    balance = sBalance;
                string reason = dt.Rows[i]["debitReason"].ObjToString() + " " + dt.Rows[i]["creditReason"].ObjToString();
                dt.Rows[i]["balance"] = G1.RoundValue ( balance );
                dt.Rows[i]["prince"] = G1.RoundValue(principal);
                dt.Rows[i]["debit"] = debit;
                dt.Rows[i]["credit"] = credit;
                dt.Rows[i]["reason"] = reason.Trim();
                sBalance = balance;
            }
            newBalance = DailyHistory.ReCalculateDetails(workContract, ref totalInt);
            workDt = dt;
        }
        /***********************************************************************************************/
        private bool CalcNextDueDate(double payment, double newBalance, ref DateTime nextDueDate, ref double creditBalance, ref double months)
        {
            months = 1;
            if (loading)
                return false;
            if (monthlyPayment <= 0D)
                return false;
            months = payment / monthlyPayment;
            if ( insuranceContract )
            {
                string refund = GetData("Is Refund");
                if (refund.ToUpper() == "Y")
                    months = 0D;
            }
            string str = GetData("Due Date");
            string lastDueDate = GetData("Due Date", true);
            double debit = GetMoney("Debit");
            double credit = GetMoney("Credit");
            double interest = GetMoney("Interest");
            string date = GetData("Pay Date");
            DateTime docp = date.ObjToDateTime();
            if (docp.Year < 1800)
                docp = DateTime.Now;

            nextDueDate = DateTime.Now;
            DateTime currentDueDate = DateTime.Now;

            double oldbalance = 0D;

            DailyHistory.ReCalculateDueDate(workContract, docp, monthlyPayment, payment, debit, credit, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
            if (lineChanging.ToUpper() == "INTEREST")
            {
                if (interest == 0D && credit > 0D)
                {
                    nextDueDate = currentDueDate;
                    PutData("Next Due Date", nextDueDate.ToString("MM/dd/yyyy"));
                    months = 0;
                }
            }
            return true;
        }
        /***************************************************************************************/
        private void btnMinus_Click(object sender, EventArgs e)
        { // Correct Previous Payment
            loading = true;
            DataTable dt = (DataTable)dgv.DataSource;

            double balance = lblBalance.Text.ObjToDouble();
            double newbalance = lblNewBalance.Text.ObjToDouble();

            string nextDueDate = GetData("Next Due Date", true);
            DateTime date = nextDueDate.ObjToDateTime();
//            date = date.AddMonths(-1);
            nextDueDate = date.ToString("MM/dd/yyyy");
            PutData("Next Due Date", nextDueDate);

            string dueDate = GetData("Due Date", true);
            PutData("Due Date", dueDate);

            string DatePaid = GetData("Date Paid", true);
            PutData("Date Paid", DatePaid );

            double payment = GetMoney("Payment");
            double principal = GetMoney("Principal", true);
            double interest = GetMoney("Interest", true) * (-1D);

            PutMoney("Payment", 0D);
            PutMoney("Principal", 0D);
            PutMoney("Interest", 0D);

            double trust85P = GetMoney("trust85P", true) * (-1D);
            double trust100P = GetMoney("trust100P", true) * (-1D);
            PutMoney("Debit", principal);
            PutMoney("Credit Balance", 0D);
            PutData("Debit Reason", "Debit Adjust Payment");

            //            CalcNewStuff();

            PutMoney("Payment", 0D);
            PutMoney("Interest", interest);
            PutMoney("Principal", 0D);
            if (!insuranceContract)
            {
                PutMoney("Trust85P", trust85P);
                PutMoney("Trust100P", trust100P);
            }
            CalculateNewBalance();

            //double newBalance = balanceDue + payment;
            //string p_balance = G1.ReformatMoney(newBalance);
            //lblNewBalance.Text = "New Balance :$" + p_balance;

            loading = false;
        }
        /***************************************************************************************/
        private void setManualPaymentPaidDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string paidDate = ManualPaymentPaidDate.ToString("MM/dd/yyyy");
            using (Ask askForm = new Ask("Enter Pay Date for Manual Payments?", paidDate))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != DialogResult.OK)
                    return;
                string str = askForm.Answer;
                if (!G1.validate_date(str))
                    MessageBox.Show("***ERROR*** Not a valid Date!");
                else
                {
                    ManualPaymentPaidDate = str.ObjToDateTime();
                    PutData("Date Paid", str);
                    CalculateDetails();
                }
            }
        }
        /***************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            GridView view = sender as GridView;

            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            string where = dt.Rows[row]["description"].ObjToString().Trim().ToUpper();
            //if ( where.ToUpper() == "NEXT DUE DATE")
            //{
            //    DateTime date = dt.Rows[row]["newpayment"].ObjToDateTime();
            //    if ( date.Year == 2039 && date.Month == 12 && date.Day == 31)

            //}
            if ( where != "LOCATION" && where != "BANK ACCOUNT")
            {
                this.bandedGridColumn13.ColumnEdit = null;
                return;
            }
            if ( where == "LOCATION")
            {
                try
                {
                    this.bandedGridColumn13.ColumnEdit = this.repositoryItemComboBox1;
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
            else if (where == "BANK ACCOUNT")
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
                    if ( dx.Rows.Count > 0 )
                    {
                        string accountTitle = dx.Rows[0]["account_title"].ObjToString();
                        string location = dx.Rows[0]["location"].ObjToString();
                        string bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                        string bankAccount = dx.Rows[0]["account_no"].ObjToString();
                        string data = location + "~" + bank_gl + "~" + bankAccount;
                        PutData("Bank Account", data);

                        if (!isCCBank())
                            PutMoney("CC FEE", 0D);
                        double debit = GetMoney("Debit");
                        double credit = GetMoney("Credit");
                        if ( debit == 0D && credit == 0D )
                            CalcNewStuff();
                    }
                }
            }
        }
        /***************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();

            DateTime date = DateTime.Now;

            string type = "I";
            //if (what == "LKBX")
            //    type = "T";
            //else if (what == "ACH")
            //    type = "A";
            //else if ( what == "CC")
            //    type = "C";
            string depositNumber = type + date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
            PutData("Deposit Number", depositNumber);
        }
        /***************************************************************************************/
        private void chkDownPayment_CheckedChanged(object sender, EventArgs e)
        {
        }
        /***************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            double payment = GetMoney("Payment");
            double principal = GetMoney("Principal");
            double interest = GetMoney("Interest");
            double trust85 = GetMoney("Trust85P");
            double trust100 = GetMoney("Trust100P");
            double dApr = apr.ObjToDouble();
            double months = financeDays;

            DataTable dt = (DataTable)dgv.DataSource;

            ExplainTrust trustForm = new ExplainTrust(workContract, originalMethod, originalContractValue, originalDownPayment, originalIssueDate,
                payment, interest, principal, dApr, trust100, trust85, months, dt);
            trustForm.ShowDialog();
        }
        /***********************************************************************************************/
        private void sendMessage()
        {
            string subject = "Message from Manual Payments";

            string cnum = workContract;
            double payment = GetMoney("Payment");
            string message = "Contract (" + cnum + ") has made a payment of $" + G1.ReformatMoney(payment) + ".\n";

            string cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + cnum + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string name = dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["lastName"].ObjToString();
                message = "Customer " + name + " (" + cnum + ") has made a payment of $" + G1.ReformatMoney(payment) + ".\n\n";
            }

            string paymentsFile = "payments";
            if (cnum.ToUpper().IndexOf("ZZ") == 0)
                paymentsFile = "ipayments";

            cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + cnum + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                double balance = dx.Rows[0]["balanceDue"].ObjToDouble();
                DateTime fromDate = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                DateTime lapseDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                string lapse = dx.Rows[0]["lapsed"].ObjToString().ToUpper();
                DateTime toDate = DateTime.Now;
                message += "Current balance is $" + G1.ReformatMoney(balance) + " and their last payment was made on " + fromDate.ToString("MM/dd/yyyy") + ".\n\n";
                double contractValue = DailyHistory.GetContractValue(cnum);
                double downPayment = dx.Rows[0]["downPayment"].ObjToDouble();
                message += "Original Contract Value was $" + G1.ReformatMoney(contractValue) + " and their down payment was $" + G1.ReformatMoney(downPayment) + ".\n\n";
                dx = G1.get_db_data("Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' order by `payDate8` DESC;");
                ManualPayment.CleanupWork(dx);
                int payments = dx.Rows.Count;
                string word = " payments";
                if (payments == 1)
                    word = " payment";

                message += "Customer has made " + payments.ToString() + word + " with $" + G1.ReformatMoney(payment) + " as the monthly payment.\n\n";
                message += "Current Due Date is " + dueDate.ToString("MM/dd/yyyy") + "\n\n";

                if (lapse == "Y")
                {
                    message += "Customer lapsed contract on " + lapseDate.ToString("MM/dd/yyyy") + ".\n\n";
                    message += "Do you want to Reinstate?";
                }
                dx.Dispose();
            }

            Messages messageForm = new Messages(subject, message);
            messageForm.Show();
        }
        /***************************************************************************************/
        private void reinstateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string contractNumber = workContract;
            DialogResult result = MessageBox.Show("Are you sure you want to REINSTATE customer (" + contractNumber + ") ?", "Reinstate Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;

            string cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime today = DateTime.Now;
                string reinstateDate = today.ToString("yyyy-MM-dd");
                string record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table(customerFile, "record", record, new string[] { "lapsed", "" });

                cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);

                record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table(contractFile, "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00", "reinstateDate8", reinstateDate });
                if ( insuranceContract && !String.IsNullOrWhiteSpace (payer))
                {
                    if ( !String.IsNullOrWhiteSpace ( payerRecord))
                        G1.update_db_table ( "payers", "record", payerRecord, new string[] { "lapsed", "", "lapseDate8", "0000-00-00", "reinstateDate8", reinstateDate });
                }
                G1.AddToAudit(LoginForm.username, "Customers", "Reinstate", "Set", contractNumber);
            }
        }
        /***************************************************************************************/
        private void btnLapsed_Click(object sender, EventArgs e)
        {
            sendMessage();
        }
        /***************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            if (!workDownPayment)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string description = dt.Rows[row]["description"].ObjToString().ToUpper();
            bool showLine = false;
            if (description == "DUE DATE")
                showLine = true;
            if (description == "LOCATION")
                showLine = true;
            if (description == "AGENT")
                showLine = true;
            if (description == "DATE PAID")
                showLine = true;
            if (description == "DOWN PAYMENT")
                showLine = true;
            if (description == "TRUST85P")
                showLine = true;
            if (description == "TRUST100P")
                showLine = true;
            if (description == "CC FEE")
                showLine = true;
            if (description == "BANK ACCOUNT")
                showLine = true;
            if (!showLine)
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***************************************************************************************/
        private void ManualPayment_FormClosing(object sender, FormClosingEventArgs e)
        {
            DailyHistory.majorSwitch = saveMajorSwitch;
        }
        /***************************************************************************************/
        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();

            DateTime date = DateTime.Now;

            PutData("Bank Account", what);
        }
        /***************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "NEWPAYMENT")
            {
                string str = View.GetRowCellValue(e.RowHandle, "newpayment").ObjToString();
                if (str != null)
                {
                    if (str == "12/31/2039")
                        e.Appearance.BackColor = Color.LimeGreen;
                }
            }
        }
        /***************************************************************************************/
    }
}