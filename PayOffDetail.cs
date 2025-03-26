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
using DevExpress.XtraRichEdit.Layout;
using DevExpress.XtraGrid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PayOffDetail : DevExpress.XtraEditors.XtraForm
    {
        private bool workCharlotte = false;
        private string workWhat = "";
        private string workPDFfile = "";
        private double workOldBalance = 0D;
        private DateTime workdolp = DateTime.Now;
        private DateTime workdocp = DateTime.Now;
        private double workapr = 0D;
        private string workContractNumber = "";
        private DateTime workDueDate = DateTime.Now;
        private DataTable workDt = null;
        private DataTable payDt = null;
        private string name = "";
        private DateTime charlotteDate = DateTime.Now;
        private double totalRetained = 0D;
        private double trustPercent = 0D;
        private DateTime mainIssueDate = DateTime.Now;
        private bool isRiles = false;


        private double trust100Pending = 0D;
        private double trust100History = 0D;
        private double totalTrust100P = 0D;

        private double recalculatedPrincipal = 0D;



        private double recalculatedTotalInterest = 0D;
        private double actualTotalInterest = 0D;
        private bool performedCA = false;
        private double savePayoff = 0D;
        private double saveInterest = 0D;
        private double saveCorrectedInterest = 0D;
        private int saveDaysSinceLastPayment = 0;
        /****************************************************************************************/
        public PayOffDetail( string contractNumber, double oldBalance, DateTime dolp, DateTime docp, double apr, DateTime dueDate, DataTable dt, string whatNow = "" )
        {
            InitializeComponent();
            workOldBalance = oldBalance;
            workdolp = dolp;
            workdocp = docp;
            workapr = apr;
            workContractNumber = contractNumber;
            workDueDate = dueDate;
            payDt = dt;
            workWhat = whatNow;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        public PayOffDetail(string contractNumber )
        {
            workContractNumber = contractNumber;
            workCharlotte = true;

            InitializeComponent();

            AddSummaryColumn("paymentCurrMonth", gridMain5);
            AddSummaryColumn("dailyHistory", gridMain5);
        }
        /****************************************************************************************/
        private void PayOffDetail_Load(object sender, EventArgs e)
        {
            txtAPR.Text = workapr.ToString();
            txtAPR_2.Text = workapr.ToString();

            string cmd = "Select * from `customers` c LEFT JOIN `contracts` p ON c.`contractNumber` = p.`contractNumber` WHERE c.`contractNumber` = '" + workContractNumber + "';";
            workDt = G1.get_db_data(cmd);

            if ( workDt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Invalid Contract (" + workContractNumber + ")");
                this.Close();
                return;
            }

            isRiles = DailyHistory.isRiles(workContractNumber);

            name = workDt.Rows[0]["firstName"].ObjToString() + " " + workDt.Rows[0]["lastName"].ObjToString();
            trustPercent = 0D;
            if (workDt.Rows.Count > 0)
            {
                trustPercent = workDt.Rows[0]["trustPercent"].ObjToDouble();
                mainIssueDate = workDt.Rows[0]["issueDate8"].ObjToDateTime();
                if (mainIssueDate.Year < 1800)
                    mainIssueDate = DailyHistory.GetIssueDate(mainIssueDate, workContractNumber, workDt);
            }
            if (isRiles)
                trustPercent = 100D;

            if (trustPercent <= 0D)
            {
                if (mainIssueDate <= new DateTime(2006, 6, 30))
                    trustPercent = 50D;
                else
                    trustPercent = 85D;
            }
            else if ( trustPercent < 1.0D )
            {
                trustPercent = trustPercent * 100D;
            }



            this.Text = "Payoff Information for Contract : (" + workContractNumber + ") " + name;
            DataTable dt = new DataTable();
            dt.Columns.Add("description");
            dt.Columns.Add("value");
            dt.Columns.Add("trust100P");
            dt.Columns.Add("dailyTrust100");

            DataRow dR = dt.NewRow();
            dR["description"] = "APR";
            workapr = G1.RoundValue(workapr);
            dR["value"] = G1.ReformatMoney(workapr);
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Last Balance Due";
            workOldBalance = G1.RoundValue(workOldBalance);
            dR["value"] = G1.ReformatMoney(workOldBalance);
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Date of Last Payment";
            dR["value"] = workdolp.ToString("MM/dd/yyyy");
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Date of Final Payment";
            dR["value"] = workdocp.ToString("MM/dd/yyyy");
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Days Between Payments";
            dR["value"] = "";
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Interest from Last Payment";
            dR["value"] = "";
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Unpaid Interest";
            dR["value"] = "";
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Corrected Interest";
            dR["value"] = "";
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Payoff Amount Due";
            dR["value"] = "";
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "";
            dR["value"] = "";
            dt.Rows.Add(dR);

            dgv.DataSource = dt;

            //CalculatePayoff();

            btnRun_Click(null, null);
            double originalDownPayment = RecalcDetail();

            CalculatePayoff();

            LoadCharlotteData();

            dR = dt.NewRow();
            dR["description"] = "Beginning Balance As Of Date";
            dR["value"] = txtAsOf.Text;
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Trust Beginning Balance";
            dR["value"] = txtCPTrust85P.Text;
            dR["trust100P"] = convertToTrust100(txtCPTrust85P.Text);

            dR["dailyTrust100"] = G1.ReformatMoney(trust100History);
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Pending Payments Trust Beginning Balance";
            dR["value"] = txtTrust85Pending.Text;
            dR["trust100P"] = convertToTrust100(txtTrust85Pending.Text);

            dR["dailyTrust100"] = G1.ReformatMoney(trust100Pending);

            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Total Trust Beginning Balance";
            dR["value"] = txtTotalTrust85P.Text;
            dR["trust100P"] = convertToTrust100(txtTotalTrust85P.Text);

            dR["dailyTrust100"] = G1.ReformatMoney(totalTrust100P);
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Beginning Balance Underfunded By";
            dR["value"] = txtFixTrust85P.Text;
            dR["trust100P"] = convertToTrust100(txtFixTrust85P.Text);
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Current Retained";
            dR["value"] = G1.ReformatMoney(totalRetained);
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["description"] = "Current Due Date";
            dR["value"] = workDueDate.ToString("MM/dd/yyyy");
            dt.Rows.Add(dR);

            dgv.DataSource = dt;

            if (workCharlotte)
            {
                tabControl1.SelectTab("tabPage2");
            }
            else if (workWhat.Trim().ToUpper() == "PACKET PAYOFF")
                packetPayoff();
        }
        /****************************************************************************************/
        private void packetPayoff ()
        {
            tabControl1.SelectTab("tabPage1");
            workPDFfile = @"c:/rag/workPDF1.pdf";
            G1.GrantFileAccess ( workPDFfile );
            printPreviewToolStripMenuItem_Click(null, null);

            tabControl1.SelectTab("tabPage3");
            workPDFfile = @"c:/rag/workPDF2.pdf";
            G1.GrantFileAccess(workPDFfile);
            printPreviewToolStripMenuItem_Click(null, null);

            tabControl1.SelectTab("tabPage5");
            workPDFfile = @"c:/rag/workPDF3.pdf";
            G1.GrantFileAccess(workPDFfile);
            printPreviewToolStripMenuItem_Click(null, null);

            this.Close();
        }
        /****************************************************************************************/
        private string convertToTrust100 ( string s_trust85 )
        {
            double contractValue = DailyHistory.GetContractValuePlus(workContractNumber);
            double maxTrust85P = contractValue * 0.85D;
            double trust85P = contractValue * 0.85D;
            if (trustPercent > 0D)
            {
                trust85P = contractValue * (trustPercent / 100D);
                maxTrust85P = contractValue * (trustPercent / 100D);
            }

            double trust85 = s_trust85.ObjToDouble();
            trust85 = G1.RoundValue(trust85);
            double trust100 = trust85 / 0.85D;
            trust100 = G1.RoundValue(trust100);
            string s_trust100 = G1.ReformatMoney(trust100);
            return s_trust100;
        }
        /****************************************************************************************/
        private void LoadCharlotteData ()
        {
            string cmd = "Select * from `trust2013r` where `contractNumber` = '" + workContractNumber + "' ORDER BY `payDate8` DESC;";
            DataTable dx = G1.get_db_data(cmd);
            AddDailyHistory(dx);
            //if (LoginForm.username.ToUpper() == "ROBBY")
            //    AddDailyHistory(dx);
            //else
            //    gridMain5.Columns["dailyHistory"].Visible = false;
            G1.NumberDataTable(dx);
            dgv5.DataSource = dx;
        }
        /****************************************************************************************/
        private void AddDailyHistory ( DataTable dt)
        {
            if (payDt == null)
                return;

            if (G1.get_column_number(dt, "dailyHistory") < 0)
                dt.Columns.Add("dailyHistory", Type.GetType("System.Double"));
            if (G1.get_column_number(payDt, "myDate") < 0)
                payDt.Columns.Add("myDate");
            for ( int i=0; i<payDt.Rows.Count; i++)
            {
                payDt.Rows[i]["myDate"] = payDt.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMM");
            }
            DateTime oldDate = DateTime.Now;
            DateTime date = DateTime.Now;
            DataRow[] dRows = null;
            string date1 = "";
            string date2 = "";
            int days = 0;
            double trust85 = 0D;
            string fill = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["payDate8"].ObjToDateTime();
                oldDate = new DateTime(date.Year, date.Month, 1);
                date1 = oldDate.ToString("yyyyMM");
                dRows = payDt.Select("myDate='" + date1 + "' AND fill<>'D'");
                if ( dRows.Length > 0 )
                {
                    trust85 = 0D;
                    for (int j = 0; j < dRows.Length; j++)
                        trust85 += dRows[j]["trust85P"].ObjToDouble();
                    dt.Rows[i]["dailyHistory"] = trust85;
                }
                else
                    dt.Rows[i]["dailyHistory"] = 0D;
            }
        }
        /****************************************************************************************/
        private double RecalcDetail ()
        {
            recalculatedPrincipal = 0D;
            if (payDt == null)
                return 0D;

            btnCA.Hide();
            double originalDownPayment = 0D;
            int numPayments = workDt.Rows[0]["numberOfPayments"].ObjToInt32();
            double payment = workDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double amtOfMonthlyPayt = workDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double monthlyPayment = workDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            DataTable dt = payDt.Copy();

            if ( G1.get_column_number ( dt, "cumulativeTrust85") < 0 )
                dt.Columns.Add("cumulativeTrust85", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "cumulativeTrust100") < 0)
                dt.Columns.Add("cumulativeTrust100", Type.GetType("System.Double"));

            dgv3.DataSource = null;

            if (payDt.Rows.Count <= 0)
            {
                dgv3.DataSource = dt;
                return originalDownPayment;
            }

            double serTot = workDt.Rows[0]["serviceTotal"].ObjToString().ObjToDouble();
            double merTot = workDt.Rows[0]["merchandiseTotal"].ObjToString().ObjToDouble();
            double downPayment = workDt.Rows[0]["downPayment"].ObjToString().ObjToDouble();
            if (downPayment == 0D)
                downPayment = DailyHistory.GetDownPaymentFromPayments(workContractNumber);
            originalDownPayment = downPayment;
            double totalPurchase = serTot + merTot - downPayment;
            totalPurchase = DailyHistory.GetFinanceValue(workDt.Rows[0]);

            double dp = payDt.Rows[0]["downPayment"].ObjToDouble();
            if (downPayment != dp)
                downPayment = dp;

            double contractValue = DailyHistory.GetContractValuePlus(workContractNumber);
            double maxTrust85P = contractValue * 0.85D;
            if (trustPercent > 0D)
                maxTrust85P = contractValue * (trustPercent / 100D);


            double startBalance = totalPurchase;
            double balance = contractValue - downPayment;

            var loanAmount = balance;
            var myApr = txtAPR.Text.ObjToDouble();
            if (myApr != workapr)
                btnCA.Show();
            var numberOfYears = 5;

            // rate of interest and number of payments for monthly payments
            var rateOfInterest = myApr / 1200;
            var numberOfPayments = numPayments;

            // loan amount = (interest rate * loan amount) / (1 - (1 + interest rate)^(number of payments * -1))
            var paymentAmount = (rateOfInterest * loanAmount) / (1 - Math.Pow(1 + rateOfInterest, numberOfPayments * -1));
            if ( monthlyPayment <= 0D)
            {
                paymentAmount = G1.RoundValue(paymentAmount);
                monthlyPayment = paymentAmount;
                payment = paymentAmount;
                amtOfMonthlyPayt = paymentAmount;
            }


            DateTime issueDate = workDt.Rows[0]["issueDate8"].ObjToDateTime();
            if (issueDate.Year < 1800)
                issueDate = DailyHistory.GetIssueDate(issueDate, workContractNumber, null);

            double interest = 0D;
            double principal = downPayment;
            double rate = txtAPR.Text.ObjToDouble() / 100D;
            double trust85P = 0D;
            double trust100P = 0D;

            double cumulativeTrust85 = 0D;
            double cumulativeTrust100 = 0D;

            int method = ImportDailyDeposits.CalcTrust85(payment, issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment, numPayments.ObjToDouble(), originalDownPayment, principal, rate, ref trust85P, ref trust100P);

            //var loanAmount = balance;
            //var myApr = workapr;
            //var numberOfYears = 5;

            //// rate of interest and number of payments for monthly payments
            //var rateOfInterest = myApr / 1200;
            //var numberOfPayments = numPayments;

            //// loan amount = (interest rate * loan amount) / (1 - (1 + interest rate)^(number of payments * -1))
            //var paymentAmount = (rateOfInterest * loanAmount) / (1 - Math.Pow(1 + rateOfInterest, numberOfPayments * -1));

            DateTime dofp = workDt.Rows[0]["issueDate8"].ObjToDateTime();
            if (payDt.Rows.Count == 1)
                dofp = payDt.Rows[0]["payDate8"].ObjToDateTime();
            else
                dofp = payDt.Rows[1]["payDate8"].ObjToDateTime();
            DateTime dolp = dofp;
            DateTime date = DateTime.Now;
            double retained = 0D;
            dofp = dt.Rows[0]["payDate8"].ObjToDateTime();
            double debit = 0D;
            double credit = 0D;
            int days = 0;

            double localAPR = txtAPR.Text.ObjToDouble();
            double newRate = (localAPR / 10D) / 12.0D;
            newRate = localAPR / 100D;
            string lockInterest = "";
            double adjustedAPR = 0D;
            string status = "";
            string creditReason = "";

            TimeSpan ts;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status == "D")
                    continue;

                date = dt.Rows[i]["payDate8"].ObjToDateTime();
                if ( date.ToString("yyyy-MM-dd") == "2020-05-06")
                {
                }
                downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();

                //downPayment = dt.Rows[i]["dpp"].ObjToDouble();
                //payment = dt.Rows[i]["ap"].ObjToDouble();

                creditReason = dt.Rows[i]["creditReason"].ObjToString();
                if ( creditReason.ToUpper() == "TCA")
                {
                    continue;
                }
                //if (payment == 0D && downPayment == 0D)
                //{
                //    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                //    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                //    payment = credit;
                //    if (debit != 0D)
                //        payment = debit * -1D;
                //    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                //    principal = payment - interest;
                //    balance = balance - principal;
                //    dt.Rows[i]["principal"] = principal;
                //    dt.Rows[i]["prince"] = principal;
                //    dt.Rows[i]["balance"] = balance;
                //    dt.Rows[i]["days"] = 0D;

                //    //continue;
                //}

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                //if ( debit == 0D && credit == 0D )
                //    dolp = dofp;
                ts = date - dolp;
                days = ts.Days;
                if (debit != 0D || credit != 0D)
                    days = 0;
                dofp = date;

                adjustedAPR = newRate;
                lockInterest = dt.Rows[i]["lockInterest"].ObjToString().ToUpper();
                if (lockInterest == "Y")
                {
                    adjustedAPR = dt.Rows[i]["apr"].ObjToDouble();
                    if (adjustedAPR > 1.0D)
                        adjustedAPR = adjustedAPR / 100.0D;
                }

                interest = DailyHistory.CalculateInterest(dolp, days, adjustedAPR, balance);
                interest = G1.RoundValue(interest);

                //interest = G1.RoundValue(balance * rate / 12.0D);
                if (downPayment > 0D)
                    payment = downPayment;
                if (i == 0)
                    interest = 0D;

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                if (debit != 0D)
                {
                    payment = debit * -1D;
                }
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                if ( credit > 0D)
                {
                    payment = credit;
                }

                principal = payment - interest;
                if (downPayment == 0D)
                    balance = balance - principal;
                else
                    principal = 0D;
                dt.Rows[i]["interestPaid"] = interest;
                dt.Rows[i]["principal"] = principal;
                dt.Rows[i]["prince"] = principal;
                dt.Rows[i]["balance"] = balance;
                dt.Rows[i]["NumPayments"] = 0D;
                if ( downPayment == 0D)
                    dt.Rows[i]["NumPayments"] = payment / monthlyPayment;
                dt.Rows[i]["days"] = days.ObjToDouble();
                //recalculatedPrincipal += principal;

                trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                trust100P = dt.Rows[i]["trust100P"].ObjToDouble();

                //if (date > DailyHistory.majorDate)
                //{

                if (debit == 0D && credit == 0D)
                {
                    recalculatedPrincipal += principal;

                    method = ImportDailyDeposits.CalcTrust85P(date, amtOfMonthlyPayt, issueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments.ObjToDouble(), payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);


                    //method = ImportDailyDeposits.CalcTrust85(monthlyPayment, issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment, numPayments.ObjToDouble(), payment, principal, rate, ref trust85P, ref trust100P);

                    if (i > 0)
                    {
                        dt.Rows[i]["trust85P"] = trust85P;
                        dt.Rows[i]["trust100P"] = trust100P;
                    }
                    else
                    {
                        trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                        trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                    }

                }
                else
                {
                    if (debit > 0D || credit > 0D)
                    {
                        dt.Rows[i]["interestPaid"] = payDt.Rows[i]["interestPaid"].ObjToDouble();
                        dt.Rows[i]["principal"] = payDt.Rows[i]["prince"].ObjToDouble();
                        dt.Rows[i]["prince"] = payDt.Rows[i]["prince"].ObjToDouble();
                        dt.Rows[i]["balance"] = payDt.Rows[i]["balance"].ObjToDouble();
                        dt.Rows[i]["NumPayments"] = payDt.Rows[i]["NumPayments"].ObjToDouble();
                        dt.Rows[i]["days"] = payDt.Rows[i]["days"].ObjToDouble();
                        recalculatedPrincipal += payDt.Rows[i]["prince"].ObjToDouble();

                        trust85P = payDt.Rows[i]["trust85P"].ObjToDouble();
                        trust100P = payDt.Rows[i]["trust100P"].ObjToDouble();
                    }
                }
                //}
                cumulativeTrust85 += trust85P;
                cumulativeTrust100 += trust100P;
                dt.Rows[i]["cumulativeTrust85"] = cumulativeTrust85;
                dt.Rows[i]["cumulativeTrust100"] = cumulativeTrust100;

                if (debit == 0D && credit == 0D)
                    dolp = date;
            }

            recalculatedPrincipal = G1.RoundValue(recalculatedPrincipal);

            DailyHistory.CalculateRetainedInterest(dt, "", true);
            DailyHistory.RecalcRetained(workContractNumber, dt);

            totalRetained = SumColumn(dt, "retained");



            double endingBalance = 0D;
            double trust85Pending = 0D;
            double totalTrust85P = 0D;
            double fixTrust85P = 0D;
            CalcTrust2013(originalDownPayment, ref endingBalance, ref trust85Pending);

            txtTrust100P.Text = G1.ReformatMoney(contractValue);
            txtTrust85P.Text = G1.ReformatMoney(maxTrust85P);
            txtCPTrust85P.Text = G1.ReformatMoney(endingBalance);
            txtTrust85Pending.Text = G1.ReformatMoney(trust85Pending);

            totalTrust85P = endingBalance + trust85Pending;
            txtTotalTrust85P.Text = G1.ReformatMoney(totalTrust85P);

            fixTrust85P = maxTrust85P - totalTrust85P;
            txtFixTrust85P.Text = G1.ReformatMoney(fixTrust85P);

            txtAsOf.Text = charlotteDate.ToString("MM/dd/yyyy");

            double total100P = SumColumn(dt, "trust100P");
            if (total100P <= 0D)
            {
                double totalPayments = SumColumn(dt, "paymentAmount");
                totalRetained = totalPayments - total100P;
            }

            trust100Pending = 0D;
            trust100History = 0D;
            totalTrust100P = 0D;

            GetDailyTrust100 (originalDownPayment, ref endingBalance, ref trust100Pending, true );
            GetDailyTrust100(originalDownPayment, ref endingBalance, ref trust100History, false );
            totalTrust100P = trust100Pending + trust100History;
            totalTrust100P = G1.RoundValue(totalTrust100P);

            G1.NumberDataTable(dt);
            dgv3.DataSource = dt;
            return originalDownPayment;
        }
        /****************************************************************************************/
        private void CalculatePayoff ()
        {
            if (payDt == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            double apr = dt.Rows[0]["value"].ObjToDouble();
            double oldBalance = dt.Rows[1]["value"].ObjToDouble();
            DateTime dolp = dt.Rows[2]["value"].ObjToDateTime();
            DateTime docp = dt.Rows[3]["value"].ObjToDateTime();

            double payment = 0D;
            double principal = 0D;
            double interest = 0D;
            double unpaid_interest = 0D;
            double correctedInterest = 0D;
            int days = 0;
            ImportDailyDeposits.CalcPrincipalInterest(oldBalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest );

            dt.Rows[4]["value"] = days.ToString();
            saveDaysSinceLastPayment = days;

            interest = G1.RoundValue(unpaid_interest);
            dt.Rows[5]["value"] = G1.ReformatMoney(interest);
            double oldInterest = interest;

            double payoff = oldBalance + interest;
            payoff = G1.RoundValue(payoff);

            ImportDailyDeposits.HandleUnpaidInterest(workContractNumber, payment, ref interest, ref unpaid_interest, ref principal, ref payoff);
            unpaid_interest = unpaid_interest - oldInterest;
            dt.Rows[6]["value"] = G1.ReformatMoney(unpaid_interest);

            dt.Rows[7]["value"] = "0.00";

            payoff = oldBalance + unpaid_interest + oldInterest;
            dt.Rows[8]["value"] = G1.ReformatMoney(payoff);

            dgv.DataSource = dt;

            double numPayments = 0D;
            double interestPaid = 0D;

            DataTable aDt = (DataTable)dgv3.DataSource;
            for ( int i=0; i<payDt.Rows.Count; i++)
            {
                numPayments += payDt.Rows[i]["numPayments"].ObjToDouble();
                interestPaid += payDt.Rows[i]["interestPaid"].ObjToDouble();
            }
            interestPaid = G1.RoundValue(interestPaid);
            double correctInterest = 0D;
            for ( int i=0; i<aDt.Rows.Count; i++)
            {
                if (Convert.ToDouble(i) > numPayments)
                    break;
                correctInterest += aDt.Rows[i]["interestPaid"].ObjToDouble();
            }
            correctInterest = G1.RoundValue(correctInterest);

            dt.Rows[7]["value"] = G1.ReformatMoney(correctInterest - interestPaid );

            payoff = oldBalance + unpaid_interest + oldInterest + correctInterest - interestPaid;
            payoff = G1.RoundValue(payoff);
            dt.Rows[8]["value"] = G1.ReformatMoney(payoff);

            savePayoff = payoff;
            saveInterest = oldInterest;
            saveCorrectedInterest = correctInterest - interestPaid;
            saveCorrectedInterest = G1.RoundValue(saveCorrectedInterest);
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string description = dr["description"].ObjToString();
            //if (description == "APR")
            //    dr["value"] = G1.ReformatMoney(workapr);
            //else if (description == "Date of Last Payment" )
            //    dr["value"] = workdolp.ToString("MM/dd/yyyy");
            if ( description == "Date of Last Payment" || description == "Date of Final Payment" )
            {
                string str = dr["value"].ObjToString();
                if ( !G1.validate_date ( str))
                {
                    MessageBox.Show("***ERROR*** Invalid Date Entered!");
                    return;
                }
            }
            else if ( description == "Days Between Payments" )
            {
                string str = dr["value"].ObjToString();
                if ( !G1.validate_numeric ( str))
                {
                    MessageBox.Show("***ERROR*** Invalue Days Entered!");
                    return;
                }
                int days = str.ObjToInt32();
                DateTime dolp = dt.Rows[2]["value"].ObjToDateTime();
                DateTime docp = dolp.AddDays(days);
                dt.Rows[3]["value"] = docp.ToString("MM/dd/yyyy");
            }
            else if (description == "Last Balance Due")
            {
                dr["value"] = G1.ReformatMoney(workOldBalance);
            }
            CalculatePayoff();
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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
            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;
            else if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            else if (dgv6.Visible)
                printableComponentLink1.Component = dgv6;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 80, 10);
            if ( dgv6.Visible )
                Printer.setupPrinterMargins(10, 10, 10, 10);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            if (!dgv.Visible && !dgv6.Visible)
                printingSystem1.Document.AutoFitToPagesWidth = 1;

            if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, true);
            else if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, true);
            else if (dgv4.Visible)
                G1.AdjustColumnWidths(gridMain4, 0.65D, true);
            else if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, true);

            printableComponentLink1.CreateDocument();
            if (workWhat.Trim().ToUpper() == "PACKET PAYOFF" )
            {
                string filename = "";
                //string filename = path + @"\" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                filename = workPDFfile;
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                printableComponentLink1.ExportToPdf(filename);
            }
            else
                printableComponentLink1.ShowPreview();

            if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, false );
            else if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, false );
            else if (dgv4.Visible)
                G1.AdjustColumnWidths(gridMain4, 0.65D, false );
            else if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, false );
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
            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;
            else if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            else if (dgv6.Visible)
                printableComponentLink1.Component = dgv6;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            if ( !dgv.Visible && !dgv6.Visible )
                printingSystem1.Document.AutoFitToPagesWidth = 1;

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
            if (String.IsNullOrWhiteSpace(workWhat))
            {
                if (dgv6.Visible)
                    return;
            }

            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string text = this.Text;
            if (dgv3.Visible)
            {
                text = "Recalculated Daily History for Contract : (" + workContractNumber + ") " + name;
            }
            else if (dgv4.Visible)
            {
                text = "Amortization for Contract : (" + workContractNumber + ") " + name;
            }
            else if (dgv5.Visible)
            {
                text = "Charlotte Data for Payoff for Contract : (" + workContractNumber + ") " + name;
            }
            Printer.DrawQuad(4, 8, 8, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain4_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("trust85P", gridMain3 );
            AddSummaryColumn("trust100P", gridMain3);
            AddSummaryColumn("dpp", gridMain3);
            AddSummaryColumn("downPayment", gridMain3);
            AddSummaryColumn("ap", gridMain3);
            AddSummaryColumn("paymentAmount", gridMain3);
            AddSummaryColumn("interestPaid", gridMain3);
            AddSummaryColumn("debit", gridMain3);
            AddSummaryColumn("ccFee", gridMain3);
            AddSummaryColumn("credit", gridMain3);
            AddSummaryColumn("retained", gridMain3);
            AddSummaryColumn("principal", gridMain3);
            AddSummaryColumn("prince", gridMain3);
            AddSummaryColumn("NumPayments", gridMain3, "{0:0.00}");

            gridMain3.Columns["downPayment"].Visible = false;
            gridMain3.Columns["paymentAmount"].Visible = false;

            AddSummaryColumn("trust85P", gridMain4);
            AddSummaryColumn("trust100P", gridMain4);
            AddSummaryColumn("downPayment", gridMain4);
            AddSummaryColumn("paymentAmount", gridMain4);
            AddSummaryColumn("interestPaid", gridMain4);
            AddSummaryColumn("debit", gridMain4);
            AddSummaryColumn("credit", gridMain4);
            AddSummaryColumn("retained", gridMain4);
            AddSummaryColumn("principal", gridMain4);
            AddSummaryColumn("prince", gridMain4);
            AddSummaryColumn("NumPayments", gridMain4, "{0:0.00}");

            AddSummaryColumn("paymentCurrMonth", gridMain5 );
            AddSummaryColumn("dailyHistory", gridMain5);

            gridMain3.Columns["days"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }

        private void gridMain3_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private DataTable ReCalculateAmort()
        {
            if (payDt == null)
                return null;
            double newAPR = txtAPR_2.Text.ObjToDouble();
            int numPayments = workDt.Rows[0]["numberOfPayments"].ObjToInt32();
            double payment = workDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            DataTable dt = payDt.Clone();
            if (payDt.Rows.Count < 1)
                return dt;
            if (newAPR <= 0D)
                return dt;

            double serTot = workDt.Rows[0]["serviceTotal"].ObjToString().ObjToDouble();
            double merTot = workDt.Rows[0]["merchandiseTotal"].ObjToString().ObjToDouble();
            double downPayment = workDt.Rows[0]["downPayment"].ObjToString().ObjToDouble();
            if (downPayment == 0D)
                downPayment = DailyHistory.GetDownPaymentFromPayments(workContractNumber);
            double allowInsurance = workDt.Rows[0]["allowInsurance"].ObjToDouble();
            double allowMerchandise = workDt.Rows[0]["allowMerchandise"].ObjToDouble();
            double cashAdvance = workDt.Rows[0]["cashAdvance"].ObjToDouble();
            double totalPurchase = serTot + merTot - downPayment - allowInsurance - allowMerchandise + cashAdvance;
            totalPurchase = DailyHistory.GetFinanceValue(workDt.Rows[0]);

            dt.Columns.Add("cInterest", Type.GetType("System.Double"));
            dt.Columns.Add("cRetained", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "cumulativeTrust85") < 0)
                dt.Columns.Add("cumulativeTrust85", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "cumulativeTrust100") < 0)
                dt.Columns.Add("cumulativeTrust100", Type.GetType("System.Double"));


            double dp = 0D;
            double cumulativeTrust85 = 0D;
            double cumulativeTrust100 = 0D;
            DateTime startDate = DateTime.Now;
            DataRow dR = dt.NewRow();
            if (payDt.Rows.Count >= 1)
            {
                dR["dueDate8"] = G1.DTtoMySQLDT(payDt.Rows[0]["dueDate8"].ObjToDateTime());
                dR["downPayment"] = payDt.Rows[0]["downPayment"].ObjToDouble();
                dp = payDt.Rows[0]["downPayment"].ObjToDouble();
            }
            else
            {
                startDate = workDt.Rows[0]["issueDate8"].ObjToDateTime();
                startDate = startDate.AddDays(30);
                dR["dueDate8"] = G1.DTtoMySQLDT(startDate);
                dp = workDt.Rows[0]["downPayment"].ObjToDouble();
                dR["downPayment"] = dp;
                downPayment = dp;
            }
            if (downPayment != dp)
                downPayment = dp;

            double contractValue = DailyHistory.GetContractValuePlus(workContractNumber);
            double maxTrust85P = contractValue * 0.85D;
            if (trustPercent > 0D)
                maxTrust85P = contractValue * (trustPercent / 100D);
            double calcTrust85P = 0D;
            double calcTrust100P = 0D;
            decimal cInterest = 0;
            decimal cRetained = 0;


            double startBalance = totalPurchase;
            decimal balance = (decimal)(contractValue - downPayment);
            //balance = (decimal)(contractValue);

            DateTime issueDate = workDt.Rows[0]["issueDate8"].ObjToDateTime();
            if (issueDate.Year < 1800)
                issueDate = DailyHistory.GetIssueDate(issueDate, workContractNumber, null);

            decimal interest = (decimal)0D;
            decimal principal = (decimal)downPayment;
            double rate = newAPR.ObjToDouble() / 100D;
            double trust85P = 0D;
            double trust100P = 0D;

            int method = ImportDailyDeposits.CalcTrust85(payment, issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment, numPayments.ObjToDouble(), downPayment, (double)principal, rate, ref trust85P, ref trust100P);

            dR["balance"] = balance;
            dR["principal"] = downPayment;
            dR["NumPayments"] = 0D;
            dR["trust85P"] = trust85P;
            dR["trust100P"] = trust100P;

            cumulativeTrust85 += trust85P;
            cumulativeTrust100 += trust100P;
            dR["cumulativeTrust85"] = cumulativeTrust85;
            dR["cumulativeTrust100"] = cumulativeTrust100;

            dt.Rows.Add(dR);

            var loanAmount = balance;
            var myApr = newAPR;
            var numberOfYears = 5;

            // rate of interest and number of payments for monthly payments
            var rateOfInterest = myApr / 1200;
            var numberOfPayments = numPayments;

            double interestRate = (myApr / 100D);
            var paymentAmount = ((double)loanAmount) * (Math.Pow((1 + interestRate / 12), numPayments) * interestRate) / (12 * (Math.Pow((1 + interestRate / 12), numPayments) - 1));
            paymentAmount = Math.Abs(paymentAmount);
            if (Double.IsInfinity(paymentAmount))
                paymentAmount = 0;
            if (interestRate <= 0D)
                paymentAmount = payment;
            else if (payment <= 0D)
                payment = paymentAmount;
            if ( loanAmount <= 0)
            {
                paymentAmount = 0;
                payment = 0;
            }

            txtCalculatedPayment.Text = G1.ReformatMoney(paymentAmount);
            txtContractPayment.Text = G1.ReformatMoney(payment);
            if (chkUseCalcualatedPayment.Checked)
                payment = paymentAmount;

            DateTime dofp = startDate;
            if ( payDt.Rows.Count > 1 )
                dofp = payDt.Rows[1]["payDate8"].ObjToDateTime();
            DateTime dolp = dofp;
            double retained = 0D;
            int myCount = 0;
            //for (int i = 0; i < numPayments; i++)
            for (; ; )
            {
                dR = dt.NewRow();
                //dR["dueDate8"] = G1.DTtoMySQLDT(dofp);
                dR["paymentAmount"] = payment;
                interest = (decimal)((double)balance * rate / 12.0D);
                interest = (decimal)G1.RoundValue((double)interest);
                if (balance <= (decimal)payment)
                {
                    principal = balance;
                    dR["paymentAmount"] = (double)(principal + interest);
                }
                else
                {
                    principal = (decimal)((decimal)payment - interest);
                    principal = (decimal)G1.RoundValue((double)principal);
                }
                balance = balance - principal;
                balance = (decimal)G1.RoundValue((double)balance);
                cInterest += interest;
                dR["cInterest"] = cInterest;
                dR["interestPaid"] = interest;
                dR["principal"] = principal;
                dR["prince"] = principal;
                dR["balance"] = balance;
                dR["NumPayments"] = 1D;

                method = ImportDailyDeposits.CalcTrust85(payment, issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment, numPayments.ObjToDouble(), payment, (double)principal, rate, ref trust85P, ref trust100P);

                if ( balance <= (decimal)0)
                {
                    calcTrust85P = SumColumn(dt, "trust85P");
                    calcTrust100P = SumColumn(dt, "trust100P");
                    trust85P = maxTrust85P - calcTrust85P;
                    trust100P = contractValue - calcTrust100P;
                }

                dR["trust85P"] = trust85P;
                dR["trust100P"] = trust100P;

                cumulativeTrust85 += trust85P;
                cumulativeTrust100 += trust100P;
                dR["cumulativeTrust85"] = cumulativeTrust85;
                dR["cumulativeTrust100"] = cumulativeTrust100;

                dt.Rows.Add(dR);

                dofp = dofp.AddMonths(1);
                if (balance <= (decimal)0)
                    break;
                myCount++;
                if (myCount >= numPayments)
                    break;
            }
            DailyHistory.CalculateRetainedInterest(dt, "" );
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                interest = dt.Rows[i]["retained"].ObjToDecimal();
                cRetained += interest;
                dt.Rows[i]["cRetained"] = cRetained;
            }

            DailyHistory.RecalcRetained(workContractNumber, dt);
            return dt;
        }
        /****************************************************************************************/
        private double SumColumn ( DataTable dt, string colName )
        {
            double total = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                total += dt.Rows[i][colName].ObjToDouble();
            }
            return total;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            dgv4.DataSource = null;

            DataTable dt = ReCalculateAmort();
            if (payDt == null)
                return;
            if (payDt.Rows.Count <= 0)
            {
                dgv4.DataSource = dt;
                return;
            }

            G1.NumberDataTable(dt);
            dgv4.DataSource = dt;
        }
        /***********************************************************************************************/
        private void CalcTrust2013 ( double originalDownPayment, ref double endingBalance, ref double trust85Pending )
        {
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            trust85Pending = 0D;
            endingBalance = 0D;
            double removals = 0D;
            double value = 0D;
            string fill = "";
            charlotteDate = DateTime.Now;

            cmd = "Select * from `trust2013r` where `contractNumber` = '" + workContractNumber + "' ORDER BY `payDate8` DESC limit 1;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();

                charlotteDate = dx.Rows[0]["payDate8"].ObjToDateTime();
                //dx = DailyHistory.GetPaymentData(workContractNumber, charlotteDate, originalDownPayment, true);
                dx = GetPaymentData(charlotteDate, true);
                trust85Pending = 0D;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    fill = dx.Rows[j]["fill"].ObjToString();
                    if (fill.ToUpper() != "D")
                    {
                        value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                        trust85Pending += value;
                    }
                }
            }
            else
            {
                charlotteDate = new DateTime(2000, 1, 1);
                //dx = DailyHistory.GetPaymentData(workContractNumber, charlotteDate, originalDownPayment, true);
                dx = GetPaymentData(charlotteDate, true);
                trust85Pending = 0D;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    fill = dx.Rows[j]["fill"].ObjToString();
                    if (fill.ToUpper() != "D")
                    {
                        value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                        trust85Pending += value;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void GetDailyTrust100(double originalDownPayment, ref double endingBalance, ref double trust100Pending, bool greater = false )
        {
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            trust100Pending = 0D;
            endingBalance = 0D;
            double removals = 0D;
            double value = 0D;
            string fill = "";
            DateTime charlotteDate = DateTime.Now;

            cmd = "Select * from `trust2013r` where `contractNumber` = '" + workContractNumber + "' ORDER BY `payDate8` DESC limit 1;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();

                charlotteDate = dx.Rows[0]["payDate8"].ObjToDateTime();
                //dx = DailyHistory.GetPaymentData(workContractNumber, charlotteDate, originalDownPayment, greater );
                dx = GetPaymentData(charlotteDate, greater);
                trust100Pending = 0D;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    fill = dx.Rows[j]["fill"].ObjToString();
                    if (fill.ToUpper() != "D")
                    {
                        value = Math.Round(dx.Rows[j]["trust100P"].ObjToDouble(), 2);
                        trust100Pending += value;
                    }
                }
            }
            else
            {
                charlotteDate = new DateTime(2000, 1, 1);
                //dx = DailyHistory.GetPaymentData(workContractNumber, charlotteDate, originalDownPayment, greater );
                dx = GetPaymentData(charlotteDate, greater);
                trust100Pending = 0D;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    fill = dx.Rows[j]["fill"].ObjToString();
                    if (fill.ToUpper() != "D")
                    {
                        value = Math.Round(dx.Rows[j]["trust100P"].ObjToDouble(), 2);
                        trust100Pending += value;
                    }
                }
            }
        }
        /****************************************************************************************/
        private DataTable GetPaymentData ( DateTime maxDate, bool greater )
        {
            DataTable dx = payDt.Clone();
            if (G1.get_column_number(payDt, "newPayDate8") < 0)
                payDt.Columns.Add("newPayDate8");
            for ( int i=0; i<payDt.Rows.Count; i++)
            {
                payDt.Rows[i]["newPayDate8"] = payDt.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");
            }
            DataRow[] dRows = null;
            if ( greater )
            {
                dRows = payDt.Select("newPayDate8 > '" + maxDate.ToString("yyyyMMdd") + "'");
                if (dRows.Length > 0)
                    dx = dRows.CopyToDataTable();
            }
            else
            {
                dRows = payDt.Select("newPayDate8 <= '" + maxDate.ToString("yyyyMMdd") + "'");
                if (dRows.Length > 0)
                    dx = dRows.CopyToDataTable();
            }
            return dx;
        }
        /****************************************************************************************/
        private void gridMain5_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 100)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void btnRecalc_Click(object sender, EventArgs e)
        {
            string str = txtAPR.Text;
            double newAPR = str.ObjToDouble();
            if (newAPR <= 0D)
                return;
            DataTable dt = (DataTable)dgv3.DataSource;
            dt.Rows.Clear();
            dgv3.DataSource = dt;

            RecalcDetail();
        }
        /****************************************************************************************/
        private void btnCA_Click(object sender, EventArgs e)
        {
            double numPayments = 0D;
            double interestPaid = 0D;
            double correctInterest = 0D;

            double trust85Paid = 0D;
            double correctTrust85 = 0D;

            double trust100Paid = 0D;
            double correctTrust100 = 0D;

            double principalPaid = 0D;
            double correctPrincipal = 0D;

            double apr = txtAPR.Text.ObjToDouble();

            DataTable aDt = (DataTable)dgv3.DataSource;
            for (int i = 0; i < payDt.Rows.Count; i++)
            {
                numPayments += payDt.Rows[i]["numPayments"].ObjToDouble();
                interestPaid += payDt.Rows[i]["interestPaid"].ObjToDouble();
                principalPaid += payDt.Rows[i]["principal"].ObjToDouble();
                trust85Paid += payDt.Rows[i]["trust85P"].ObjToDouble();
                trust100Paid += payDt.Rows[i]["trust100P"].ObjToDouble();
            }
            interestPaid = G1.RoundValue(interestPaid);
            principalPaid = G1.RoundValue(principalPaid);
            trust85Paid = G1.RoundValue(trust85Paid);
            trust100Paid = G1.RoundValue(trust100Paid);

            for (int i = 0; i < aDt.Rows.Count; i++)
            {
                //if (Convert.ToDouble(i) > numPayments)
                //    break;
                correctInterest += aDt.Rows[i]["interestPaid"].ObjToDouble();
                correctPrincipal += aDt.Rows[i]["principal"].ObjToDouble();
                correctTrust85 += aDt.Rows[i]["trust85P"].ObjToDouble();
                correctTrust100 += aDt.Rows[i]["trust100P"].ObjToDouble();
            }
            correctInterest = G1.RoundValue(correctInterest);
            correctPrincipal = G1.RoundValue(correctPrincipal);
            correctTrust85 = G1.RoundValue(correctTrust85);
            correctTrust100 = G1.RoundValue(correctTrust100);

            double c_interest = correctInterest - interestPaid;
            double c_trust85 = correctTrust85 - trust85Paid;
            double c_trust100 = correctTrust100 - trust100Paid;
            double c_principal = correctPrincipal - principalPaid;

            if ( c_principal < 0D)
            {
                c_interest = interestPaid - correctInterest;
                c_trust85 = trust85Paid - correctTrust85;
                c_trust100 = trust100Paid - correctTrust100;
            }

            c_interest = G1.RoundValue(c_interest);
            c_trust85 = G1.RoundValue(c_trust85);
            c_trust100 = G1.RoundValue(c_trust100);
            c_principal = G1.RoundValue(c_principal);

            double maxTrust85P = txtTrust85P.Text.ObjToDouble();
            c_interest = 0D;

            ManualPayment manualForm = new ManualPayment(workContractNumber, name, payDt, apr, c_trust85, c_trust100, c_interest, c_principal, maxTrust85P, trust85Paid );
            manualForm.ManualDone += ManualForm_ManualDone;
            manualForm.ShowDialog();
        }
        /****************************************************************************************/
        private void ManualForm_ManualDone(string s)
        {
            performedCA = true;
            MessageBox.Show("***INFO***\nIf this C/A was added because of an improper Interest Rate\nRemember to change the Rate in the contract tab as well\nbefore moving on.", "CA Added Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /****************************************************************************************/
        private void PayOffDetail_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnDone();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_bool();
        public event d_void_eventdone_bool CA_Done;
        protected void OnDone()
        {
            if ( performedCA )
            if (CA_Done != null)
                CA_Done.Invoke();
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABPAGE5")
                CalculatePayoffSummary();
        }
        /****************************************************************************************/
        private int extraRows = 0;
        private void CalculatePayoffSummary()
        {
            string firstName = "";
            string lastName = "";

            string cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable userDt = G1.get_db_data(cmd);
            if ( userDt.Rows.Count > 0 )
            {
                firstName = userDt.Rows[0]["firstName"].ObjToString();
                lastName = userDt.Rows[0]["lastName"].ObjToString();
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("c1");
            dt.Columns.Add("c2");
            dt.Columns.Add("c3");
            dt.Columns.Add("c4");
            dt.Columns.Add("c5");

            DataRow dr = null;
            for (int i = 0; i < 40; i++)
            {
                dr = dt.NewRow();
                dt.Rows.Add(dr);
            }

            dt.Rows[5]["c1"] = name;
            dt.Rows[6]["c1"] = workContractNumber;

            dt.Rows[9]["c1"] = "Gross Amount of Trust";
            dt.Rows[11]["c1"] = "Less: Insurance";
            dt.Rows[14]["c1"] = "Net Amount of Trust";

            dt.Rows[16]["c1"] = "Less:";
            dt.Rows[17]["c2"] = "Down Payment";
            dt.Rows[18]["c2"] = "Monthly Payments";

            double contractValue = DailyHistory.GetContractValuePlus(workContractNumber);
            string str = G1.ReformatMoney(contractValue);
            dt.Rows[9]["c5"] = str;

            double allowInsurance = 0D;

            str = G1.ReformatMoney ( allowInsurance );
            dt.Rows[11]["c5"] = str;

            //if ( saveDaysSinceLastPayment > 0 )
            //{
            //    savePayoff = savePayoff - saveInterest;
            //}

            double downPayment = 0D;
            double payments = 0D;

            CalcSummaryDetails(ref downPayment, ref payments);

            str = G1.ReformatMoney(downPayment);
            dt.Rows[17]["c4"] = str;

            double total = downPayment + payments;
            double diff = contractValue - total;
            diff = G1.RoundValue(diff);
            double newDiff = savePayoff - diff;
            newDiff = G1.RoundValue(newDiff);

            //payments = payments - newDiff - saveInterest;
            payments = contractValue - savePayoff - downPayment + saveInterest;

            payments = recalculatedPrincipal;

            str = G1.ReformatMoney(payments);
            dt.Rows[18]["c4"] = str;

            double allPayments = downPayment + payments;
            allPayments = G1.RoundValue(allPayments);
            str = G1.ReformatMoney(downPayment + payments);
            dt.Rows[19]["c5"] = str;

            extraRows = 0;
            double subBalance = 0D;
            if (saveDaysSinceLastPayment > 0)
            {
                extraRows = 6;

                dt.Rows[21]["c1"] = "Balance";
                subBalance = contractValue - allPayments;
                subBalance = G1.RoundValue(subBalance);
                str = G1.ReformatMoney(subBalance);
                dt.Rows[21]["c5"] = str;

                dt.Rows[21+3]["c1"] = "Number of days since last payment";
                dt.Rows[21+3]["c4"] = saveDaysSinceLastPayment.ObjToString();

                dt.Rows[23+3]["c1"] = "Interest Due";
                str = G1.ReformatMoney(saveInterest);
                dt.Rows[23+3]["c5"] = str;
            }
            else
            {
                subBalance = contractValue - allPayments;
                subBalance = G1.RoundValue(subBalance);
            }

            double balanceDue = subBalance + saveInterest;
            balanceDue = G1.RoundValue(balanceDue);
            str = G1.ReformatMoney(balanceDue);
            dt.Rows[23+extraRows]["c5"] = str;
            dt.Rows[23+extraRows]["c1"] = "Balance Due";

            double ccFee = G1.LookupCCFee(DateTime.Now);
            if (ccFee <= 0D)
                ccFee = 0.03D;
            if ( ccFee > 0D )
            {
                ccFee += 1D;
                double newBalance = balanceDue * ccFee;
                newBalance = G1.RoundValue(newBalance);

                str = G1.ReformatMoney ( newBalance );
                dt.Rows[25 + extraRows]["c5"] = str;
                dt.Rows[25 + extraRows]["c1"] = "Balance Due if paid by Credit Card";
//                dt.Rows[25 + extraRows]["c2"] = "Credit Card";
                extraRows += 2;
            }

            dt.Rows[26+extraRows]["c1"] = DateTime.Now.ToString("MM/dd/yyyy");
            dt.Rows[26+extraRows]["c2"] = "Prepared By " + firstName + " " + lastName;


            dgv6.DataSource = dt;
        }
        /****************************************************************************************/
        private void CalcSummaryDetails ( ref double downPayment, ref double payments )
        {
            downPayment = 0D;
            payments = 0D;
            double payment = 0D;
            double newPayment = 0D;
            double newPa = 0D;
            DataTable dt = (DataTable) dgv3.DataSource;
            if (dt == null)
                return;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
                payment = dt.Rows[i]["principal"].ObjToDouble();
                newPayment = dt.Rows[i]["prince"].ObjToDouble();
                payments += dt.Rows[i]["principal"].ObjToDouble();
                newPa += dt.Rows[i]["prince"].ObjToDouble();
            }
            if ( workContractNumber.ToUpper().Contains ( "LI"))
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["retained"] = 0D;
            }
            payments += saveInterest;
            payments += saveCorrectedInterest;
            payments = G1.RoundValue(payments);
            if ( downPayment <= 0D)
            {
                string cmd = "Select * from `contracts` where `contractNumber` = '" + workContractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    downPayment = dt.Rows[0]["downPayment"].ObjToDouble();
            }
            downPayment = G1.RoundValue(downPayment);
        }
        /****************************************************************************************/
        private void gridMain6_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "C1")
            {
                if (e.RowHandle == 5 || e.RowHandle == 6)
                {
                    Font oldFont = e.Appearance.Font;
                    FontStyle style = FontStyle.Bold;
                    float size = oldFont.Size;
                    Font newFont = new Font(oldFont.Name, size,style);
                    e.Appearance.Font = newFont;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "C4")
            {
                if (e.RowHandle == 18 )
                {
                    Font oldFont = e.Appearance.Font;
                    FontStyle style = FontStyle.Underline;
                    float size = oldFont.Size;
                    Font newFont = new Font(oldFont.Name, size, style);
                    e.Appearance.Font = newFont;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "C5")
            {
                DataTable dt = (DataTable)dgv6.DataSource;
                string field = dt.Rows[e.RowHandle]["c1"].ObjToString().ToUpper();
                if ( !String.IsNullOrWhiteSpace ( field ))
                {
                }
                if (e.RowHandle == 19)
                {
                    Font oldFont = e.Appearance.Font;
                    FontStyle style = FontStyle.Underline;
                    float size = oldFont.Size;
                    Font newFont = new Font(oldFont.Name, size, style);
                    e.Appearance.Font = newFont;
                }
                if (e.RowHandle == (23+extraRows))
                {
                    Font oldFont = e.Appearance.Font;
                    FontStyle style = FontStyle.Underline;
                    float size = oldFont.Size;
                    Font newFont = new Font(oldFont.Name, size, style);
                    e.Appearance.Font = newFont;
                }
                if (e.RowHandle == (25 + extraRows))
                {
                    Font oldFont = e.Appearance.Font;
                    FontStyle style = FontStyle.Underline;
                    float size = oldFont.Size;
                    Font newFont = new Font(oldFont.Name, size, style);
                    e.Appearance.Font = newFont;
                }
                if (field == "INTEREST DUE" )
                {
                    Font oldFont = e.Appearance.Font;
                    FontStyle style = FontStyle.Underline;
                    float size = oldFont.Size;
                    Font newFont = new Font(oldFont.Name, size, style);
                    e.Appearance.Font = newFont;
                }
                if (field == "BALANCE DUE")
                {
                    Font oldFont = e.Appearance.Font;
                    FontStyle style = FontStyle.Underline;
                    float size = oldFont.Size;
                    Font newFont = new Font(oldFont.Name, size, style);
                    e.Appearance.Font = newFont;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain3_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            if (field.ToUpper() != "DAYS")
                return;
            DataTable dt = (DataTable)dgv3.DataSource;
            if (G1.get_column_number(dt, "days") < 0)
                return;
            int days = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                days += dt.Rows[i]["days"].ObjToInt32();
            }
            double months = days.ObjToDouble();
            months = months / 365D;
            months = months * 12D;

            string str = G1.ReformatMoney(months);
            str = str.Replace("$", "");
            e.TotalValue = days.ToString() + "/" + str;
        }
        /****************************************************************************************/
        private void gridMain3_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv3.DataSource;
            string status = dt.Rows[row]["fill"].ObjToString().ToUpper();
            if (status.ToUpper() == "D" )
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtScale.Text = balance;
                ScaleCells();
                return;
            }
            // Initialize the flag to false.
            bool nonNumberEntered = false;

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Determine whether the keystroke is a number from the keypad.
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    // Determine whether the keystroke is a backspace.
                    if (e.KeyCode != Keys.Back)
                    {
                        // A non-numerical keystroke was pressed.
                        // Set the flag to true and evaluate in KeyPress event.
                        if (e.KeyCode != Keys.OemPeriod)
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if (nonNumberEntered)
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!", "Scale Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain3.Columns["balance"].AppearanceCell.Font.Size;
                mainFont = gridMain3.Columns["balance"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain3.Columns.Count; i++)
            {
                gridMain3.Columns[i].AppearanceCell.Font = font;
            }
            gridMain3.Appearance.GroupFooter.Font = font;
            gridMain3.AppearancePrint.FooterPanel.Font = font;
            newFont = font;
            gridMain3.RefreshData();
            gridMain3.RefreshEditor(true);
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (newFont != null)
                e.Appearance.Font = newFont;
        }
        /****************************************************************************************/
    }
}
