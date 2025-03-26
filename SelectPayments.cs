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
using DevExpress.XtraGrid;
using System.Web.UI.WebControls;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SelectPayments : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workContract = "";
        private DateTime charlotteDate = DateTime.Now;
        private DataTable payDt = null;
        private DataRow workDrow = null;
        private DateTime filedDate = DateTime.Now;
        private double filedAmount = 0D;
        /****************************************************************************************/
        public SelectPayments( string contract, DataRow dRow )
        {
            InitializeComponent();
            workContract = contract;
            workDrow = dRow;

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("trust85P", gridMain);
            AddSummaryColumn("trust100P", gridMain);
            AddSummaryColumn("dpp", gridMain);
            AddSummaryColumn("downPayment", gridMain);
            AddSummaryColumn("ap", gridMain);
            AddSummaryColumn("paymentAmount", gridMain);
            AddSummaryColumn("interestPaid", gridMain);
            AddSummaryColumn("debitAdjustment", gridMain);
            AddSummaryColumn("ccFee", gridMain);
            AddSummaryColumn("creditAdjustment", gridMain);
            AddSummaryColumn("retained", gridMain);
            AddSummaryColumn("principal", gridMain);
            AddSummaryColumn("prince", gridMain);
            AddSummaryColumn("NumPayments", gridMain, "{0:0.00}");

            gridMain.Columns["downPayment"].Visible = false;
            gridMain.Columns["paymentAmount"].Visible = false;

            gridMain.Columns["days"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;

            DailyHistory.CleanupVisibility(gridMain);
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
        /****************************************************************************************/
        private void SelectPayments_Load(object sender, EventArgs e)
        {
            this.Text = "Select Payments";

//            filedDate = workDrow["fun_DateFiled"].ObjToDateTime();
            filedAmount = workDrow["fun_AmtFiled"].ObjToDouble();

            LoadDetails();

            loading = false;
        }
        /****************************************************************************************/
        private void LoadDetails()
        {
            btnAccept.Hide();

            string cmd = "Select * from `payments` where `contractNumber` = '" + workContract + "' ORDER by 'payDate8';";
            payDt = G1.get_db_data ( cmd );
            if (payDt.Rows.Count <= 0)
                return;

            cmd = "Select * from `customers` c LEFT JOIN `contracts` p ON c.`contractNumber` = p.`contractNumber` WHERE c.`contractNumber` = '" + workContract + "';";
            DataTable workDt = G1.get_db_data(cmd);

            if (workDt.Rows.Count <= 0)
            {
                this.Close();
                return;
            }

            bool isRiles = DailyHistory.isRiles(workContract);

            DateTime dateDPPaid = workDt.Rows[0]["dateDPPaid"].ObjToDateTime();
            DateTime deceasedDate = workDt.Rows[0]["deceasedDate"].ObjToDateTime();
            txtDeceasedDate.Text = deceasedDate.ToString("MM/dd/yyyy");

            DateTime mainIssueDate = DateTime.Now;

            string name = workDt.Rows[0]["firstName"].ObjToString() + " " + workDt.Rows[0]["lastName"].ObjToString();
            this.Text = "Select Payments for (" + workContract + ") " + name;
            double trustPercent = 0D;
            if (workDt.Rows.Count > 0)
            {
                trustPercent = workDt.Rows[0]["trustPercent"].ObjToDouble();
                mainIssueDate = workDt.Rows[0]["issueDate8"].ObjToDateTime();
                if (mainIssueDate.Year < 1800)
                    mainIssueDate = DailyHistory.GetIssueDate(mainIssueDate, workContract, workDt);
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
            else if (trustPercent < 1.0D)
            {
                trustPercent = trustPercent * 100D;
            }

            double originalDownPayment = 0D;
            int numPayments = workDt.Rows[0]["numberOfPayments"].ObjToInt32();
            double payment = workDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double amtOfMonthlyPayt = workDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double monthlyPayment = workDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            DataTable dt = payDt.Copy();

            if (G1.get_column_number(dt, "cumulativeTrust85") < 0)
                dt.Columns.Add("cumulativeTrust85", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "cumulativeTrust100") < 0)
                dt.Columns.Add("cumulativeTrust100", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "principal") < 0)
                dt.Columns.Add("principal", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "prince") < 0)
                dt.Columns.Add("prince", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "balance") < 0)
                dt.Columns.Add("balance", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "NumPayments") < 0)
                dt.Columns.Add("NumPayments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "days") < 0)
                dt.Columns.Add("days", Type.GetType("System.Double"));

            dgv.DataSource = null;

            double serTot = workDt.Rows[0]["serviceTotal"].ObjToString().ObjToDouble();
            double merTot = workDt.Rows[0]["merchandiseTotal"].ObjToString().ObjToDouble();
            double downPayment = workDt.Rows[0]["downPayment"].ObjToString().ObjToDouble();
            if (downPayment == 0D)
                downPayment = DailyHistory.GetDownPaymentFromPayments(workContract);
            originalDownPayment = downPayment;
            double totalPurchase = serTot + merTot - downPayment;
            totalPurchase = DailyHistory.GetFinanceValue(workDt.Rows[0]);

            double dp = payDt.Rows[0]["downPayment"].ObjToDouble();
            if (downPayment != dp)
                downPayment = dp;

            double contractValue = DailyHistory.GetContractValuePlus(workContract);
            double maxTrust85P = contractValue * 0.85D;
            if (trustPercent > 0D)
                maxTrust85P = contractValue * (trustPercent / 100D);


            double startBalance = totalPurchase;
            double balance = contractValue - downPayment;

            var loanAmount = balance;
            double myApr = workDt.Rows[0]["apr"].ObjToDouble();
            txtAPR.Text = myApr.ToString();

            var numberOfYears = 5;

            // rate of interest and number of payments for monthly payments
            var rateOfInterest = myApr / 1200;
            var numberOfPayments = numPayments;

            // loan amount = (interest rate * loan amount) / (1 - (1 + interest rate)^(number of payments * -1))
            var paymentAmount = (rateOfInterest * loanAmount) / (1 - Math.Pow(1 + rateOfInterest, numberOfPayments * -1));
            if (monthlyPayment <= 0D)
            {
                paymentAmount = G1.RoundValue(paymentAmount);
                monthlyPayment = paymentAmount;
                payment = paymentAmount;
                amtOfMonthlyPayt = paymentAmount;
            }


            DateTime issueDate = workDt.Rows[0]["issueDate8"].ObjToDateTime();
            if (issueDate.Year < 1800)
                issueDate = DailyHistory.GetIssueDate(issueDate, workContract, null);

            double interest = 0D;
            double principal = downPayment;
            double rate = myApr / 100D;
            double trust85P = 0D;
            double trust100P = 0D;

            double cumulativeTrust85 = 0D;
            double cumulativeTrust100 = 0D;

            int method = ImportDailyDeposits.CalcTrust85(payment, issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment, numPayments.ObjToDouble(), originalDownPayment, principal, rate, ref trust85P, ref trust100P);

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

            double localAPR = myApr;
            double newRate = (localAPR / 10D) / 12.0D;
            newRate = localAPR / 100D;
            string lockInterest = "";
            double adjustedAPR = 0D;
            string status = "";
            string creditReason = "";
            double recalculatedPrincipal = 0D;
            string dueDate = "";

            dt = DailyHistory.GetPaymentData(workContract, charlotteDate, originalDownPayment, false);
            DailyHistory.AddAP(dt);

            DateTime dTime;
            dt.Columns.Add("sortDate");

            TimeSpan ts;

            if (G1.get_column_number(dt, "cumulativeTrust85") < 0)
                dt.Columns.Add("cumulativeTrust85", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "cumulativeTrust100") < 0)
                dt.Columns.Add("cumulativeTrust100", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "NumPayments") < 0)
                dt.Columns.Add("NumPayments", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add ( "select" );
            if (G1.get_column_number(dt, "highlight") < 0)
                dt.Columns.Add("highlight");
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");

            DataTable ddx = dt.Clone();

            for (int i = (dt.Rows.Count-1); i >= 0; i--)
            {
                dueDate = dt.Rows[i]["dueDate8"].ObjToString();
                dTime = dueDate.ObjToDateTime();
                dueDate = dTime.Year.ToString("D4") + dTime.Month.ToString("D2") + dTime.Day.ToString("D2");
                dt.Rows[i]["sortDate"] = dueDate;

                downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();

                //downPayment = dt.Rows[i]["dpp"].ObjToDouble();
                //payment = dt.Rows[i]["ap"].ObjToDouble();

                creditReason = dt.Rows[i]["creditReason"].ObjToString();
                if (creditReason.ToUpper() == "TCA")
                {
                    ddx.ImportRow(dt.Rows[i]);
                    continue;
                }

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
                if (credit > 0D)
                {
                    payment = credit;
                }

                dt.Rows[i]["NumPayments"] = 0D;
                if (downPayment == 0D)
                    dt.Rows[i]["NumPayments"] = payment / monthlyPayment;

                ddx.ImportRow(dt.Rows[i]);
            }
            //G1.sortTable(dt, "sortDate", "ASC");

            dt = ddx.Copy();
            dt.Rows[0]["dueDate8"] = G1.DTtoMySQLDT(issueDate.ToString("MM/dd/yyyy"));

            //TimeSpan ts;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status == "D")
                    continue;

                date = dt.Rows[i]["payDate8"].ObjToDateTime();
                downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();

                //downPayment = dt.Rows[i]["dpp"].ObjToDouble();
                //payment = dt.Rows[i]["ap"].ObjToDouble();

                creditReason = dt.Rows[i]["creditReason"].ObjToString();
                if (creditReason.ToUpper() == "TCA")
                {
                    continue;
                }

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                //if ( debit == 0D && credit == 0D )
                //    dolp = dofp;
                ts = date - dolp;
                days = ts.Days;
                if (debit != 0D || credit != 0D)
                    days = 0;
                dofp = date;

                trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                trust100P = dt.Rows[i]["trust100P"].ObjToDouble();

                if (date <= DailyHistory.majorDate && trust85P == 0D )
                {

                    if (debit == 0D && credit == 0D)
                    {

                        method = ImportDailyDeposits.CalcTrust85P(date, amtOfMonthlyPayt, issueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments.ObjToDouble(), payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);

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
                            trust85P = payDt.Rows[i]["trust85P"].ObjToDouble();
                            trust100P = payDt.Rows[i]["trust100P"].ObjToDouble();
                        }
                    }
                    cumulativeTrust85 += trust85P;
                    cumulativeTrust100 += trust100P;
                    dt.Rows[i]["cumulativeTrust85"] = cumulativeTrust85;
                    dt.Rows[i]["cumulativeTrust100"] = cumulativeTrust100;

                    if (debit == 0D && credit == 0D)
                        dolp = date;

                    if ( cumulativeTrust85 > filedAmount )
                        dt.Rows[i]["highlight"] = "Y";
                }
                else
                {
                    cumulativeTrust85 += trust85P;
                    cumulativeTrust100 += trust100P;
                    dt.Rows[i]["cumulativeTrust85"] = cumulativeTrust85;
                    dt.Rows[i]["cumulativeTrust100"] = cumulativeTrust100;

                    if (debit == 0D && credit == 0D)
                        dolp = date;

                    if (cumulativeTrust85 > filedAmount)
                        dt.Rows[i]["highlight"] = "Y";
                }
            }

            payDt = dt.Copy();

            recalculatedPrincipal = G1.RoundValue(recalculatedPrincipal);

            DailyHistory.CalculateRetainedInterest(dt, "", true);
            DailyHistory.RecalcRetained(workContract, dt);

            double totalRetained = SumColumn(dt, "retained");



            double endingBalance = 0D;
            double trust85Pending = 0D;
            double totalTrust85P = 0D;
            double fixTrust85P = 0D;
            CalcTrust2013(originalDownPayment, ref endingBalance, ref trust85Pending);

            double diff = cumulativeTrust85 - filedAmount;

            txtTrust100P.Text = G1.ReformatMoney(contractValue);
            txtTrust85P.Text = G1.ReformatMoney(maxTrust85P);
            txtCPTrust85P.Text = G1.ReformatMoney(endingBalance);
            txtTrust85Pending.Text = G1.ReformatMoney(diff);

            txtFiled.Text = G1.ReformatMoney(filedAmount);

            totalTrust85P = endingBalance + trust85Pending;
            txtTotalTrust85P.Text = G1.ReformatMoney(totalTrust85P);
            txtTotalTrust85P.Text = G1.ReformatMoney(cumulativeTrust85);

            fixTrust85P = maxTrust85P - totalTrust85P;
            //txtFixTrust85P.Text = G1.ReformatMoney(fixTrust85P);

            txtAsOf.Text = charlotteDate.ToString("MM/dd/yyyy");

            double total100P = SumColumn(dt, "trust100P");
            if (total100P <= 0D)
            {
                double totalPayments = SumColumn(dt, "paymentAmount");
                totalRetained = totalPayments - total100P;
            }

            double trust100Pending = 0D;
            double trust100History = 0D;
            double totalTrust100P = 0D;

            GetDailyTrust100(originalDownPayment, ref endingBalance, ref trust100Pending, true);
            GetDailyTrust100(originalDownPayment, ref endingBalance, ref trust100History, false);
            totalTrust100P = trust100Pending + trust100History;
            totalTrust100P = G1.RoundValue(totalTrust100P);

            SetupSelection(dt);

            loading = false;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt = null)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["select"] = "0";
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified = true;
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        public delegate void d_void_eventdone_string(DataTable modDt, DataRow dRow );
        public event d_void_eventdone_string ModuleDone;
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            if (ModuleDone == null)
                return;
            DialogResult result = MessageBox.Show("***Question*** Data has been modified.\nDo you want to honor these selections?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            if ( result == DialogResult.Cancel )
                e.Cancel = true;
            else
            {
                string select = "";
                DataTable dt = (DataTable)dgv.DataSource;
                DataTable dd = dt.Clone();
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "1")
                        dd.ImportRow(dt.Rows[i]);
                }
                ModuleDone.Invoke(dd, workDrow );
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
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
        /***********************************************************************************************/
        private void CalcTrust2013(double originalDownPayment, ref double endingBalance, ref double trust85Pending)
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

            cmd = "Select * from `trust2013r` where `contractNumber` = '" + workContract + "' ORDER BY `payDate8` DESC limit 1;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();

                charlotteDate = dx.Rows[0]["payDate8"].ObjToDateTime();
                dx = DailyHistory.GetPaymentData(workContract, charlotteDate, originalDownPayment, true);
                //dx = GetPaymentData(charlotteDate, true);
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
                dx = DailyHistory.GetPaymentData(workContract, charlotteDate, originalDownPayment, true);
                //dx = GetPaymentData(charlotteDate, true);
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
        private void GetDailyTrust100(double originalDownPayment, ref double endingBalance, ref double trust100Pending, bool greater = false)
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

            cmd = "Select * from `trust2013r` where `contractNumber` = '" + workContract + "' ORDER BY `payDate8` DESC limit 1;";
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
        private DataTable GetPaymentData(DateTime maxDate, bool greater)
        {
            DataTable dx = payDt.Clone();
            if (G1.get_column_number(payDt, "newPayDate8") < 0)
                payDt.Columns.Add("newPayDate8");
            for (int i = 0; i < payDt.Rows.Count; i++)
            {
                payDt.Rows[i]["newPayDate8"] = payDt.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");
            }
            DataRow[] dRows = null;
            if (greater)
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
        private double SumColumn(DataTable dt, string colName)
        {
            double total = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                total += dt.Rows[i][colName].ObjToDouble();
            }
            return total;
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            if (field.ToUpper() != "DAYS")
                return;
            DataTable dt = (DataTable)dgv.DataSource;
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
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            string select = dr["select"].ObjToString();
            string doit = "0";
            doit = "0";
            if (select == "0")
                doit = "1";
            loading = true;
            dr["select"] = doit;
            if (doit == "1")
                dr["mod"] = "Y";
            else
                dr["mod"] = "";

            loading = false;
            modified = true;
            gridMain.RefreshData();
            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();

            CheckModified();
        }
        /****************************************************************************************/
        private void CheckModified ()
        {
            modified = false;
            string select = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if ( select == "1")
                {
                    modified = true;
                    break;
                }
            }
            if (!modified)
                btnAccept.Hide();
            else
                btnAccept.Show();
            btnAccept.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                if (column == "TRUST85P")
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    string highlight = dt.Rows[row]["highlight"].ObjToString();
                    if (highlight == "Y")
                        e.Appearance.BackColor = Color.Pink;
                    else
                        e.Appearance.BackColor = Color.Transparent;
                }
            }
        }
        /****************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (ModuleDone == null)
                return;

            string select = "";
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dd = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                    dd.ImportRow(dt.Rows[i]);
            }

            ModuleDone.Invoke(dd, workDrow);

            btnAccept.Hide();
            btnAccept.Show();
            modified = false;
            this.Close();
        }
        /****************************************************************************************/
    }
}