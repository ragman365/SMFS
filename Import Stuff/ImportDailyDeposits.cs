using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
using DevExpress.XtraRichEdit.Internal;
using DevExpress.Data;
//using Microsoft.Office.Interop.Word;

//using System;
//using System.Data;
//using System.Drawing;
//using System.Windows.Forms;

//using DevExpress.XtraGrid.Views.Grid;
//using DevExpress.XtraGrid.Views.Base;
//using System.Globalization;
//using System.IO;
//using DevExpress.XtraPrinting;
//using DevExpress.Utils;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportDailyDeposits : DevExpress.XtraEditors.XtraForm
    {
        DataTable workDt = null;
        private string workingSMFS_ACH_File = "";
        private PleaseWait pleaseForm = null;
        private string ImportFileName = "";
        private string ImportFileDate = "";
        private string lkbx_ach_account = "";
        private string ach_account = "";
        private string cc_account = "";
        private string tfbx_account = "";
        private DataTable achDt = null;
        private bool workACH = false;
        private bool workCC = false;
        private bool workDraft = false;
        private bool workTheFirst = false;
        private bool workBankCC = false;
        private bool loading = true;
        private int paymentsAfterRemoved = 0;
        private int paymentsAfterDeath = 0;
        DataTable originalDt = null;
        private DataTable bankDt = null;
        private bool bankImportSuccess = false;
        /***********************************************************************************************/
        public ImportDailyDeposits(DataTable dt, bool ach = false, bool theFirst = false )
        {
            InitializeComponent();
            workDt = dt;
            workACH = ach;
            workTheFirst = theFirst;
        }
        /***********************************************************************************************/
        public ImportDailyDeposits(DataTable dt, string type)
        {
            InitializeComponent();
            workDt = dt;
            if (type.ToUpper() == "ACH")
                workACH = true;
            else if (type.ToUpper() == "CC")
                workCC = true;
            else if (type.ToUpper() == "SMFS DRAFTS")
                workDraft = true;
            else if ( type.ToUpper() == "BANKCC")
            {
                workBankCC = true;
                bankDt = dt;
                workDt = new DataTable();
            }
        }
        /***********************************************************************************************/
        public ImportDailyDeposits( string SMFS_ACH_File )
        {
            InitializeComponent();
            workDt = null;
            workDraft = true;
            workingSMFS_ACH_File = SMFS_ACH_File;
        }
        /***********************************************************************************************/
        private void ImportDailyDeposits_Load(object sender, EventArgs e)
        {
            if (LoginForm.username.ToUpper() != "ROBBY")
                txtContract.Hide();
            lblACHDate.Hide();
            LoadComboWho();
            this.dateTimePicker1.Hide();
            if (workACH || workDraft )
            {
                btnImportFile.Text = "Import ACH File";
                gridBand2.Caption = "ACH Data";
                this.Text = "Import ACH File";
                lblACHDate.Show();
                this.dateTimePicker1.Show();
            }
            else if (workCC)
            {
                btnImportFile.Text = "Import Credit Card File";
                gridBand2.Caption = "Credit Card Data";
                this.Text = "Import Credit Card File";
            }
            else if (workBankCC)
            {
                btnImportFile.Text = "Import Credit Card File";
                gridBand2.Caption = "Credit Card Data";
                this.Text = "Import Credit Card File";
            }
            else if (workDraft)
            {
                btnImportFile.Text = "Import SMFS ACH File";
                gridBand2.Caption = "SMFS ACH Data";
                this.Text = "Import SMFS ACH File";
                //lblACHDate.Show();
                //this.dateTimePicker1.Show();
            }
            if ( workTheFirst )
            {
                btnImportFile.Text = "Import TFBX File";
                gridBand2.Caption = "The First Data";
                this.Text = "Import TFBX File";
                lblACHDate.Text = "TFBX Process Date";
                lblACHDate.Show();
                this.dateTimePicker1.Show();
            }

            PullActiveData();
            HideSendMessage();
            gridBand1.Caption = "Total Trust : ";
            gridBand4.Caption = "Total Ins   : ";
            gridBand5.Caption = "Total Deposit : ";
            picLoader.Hide();
            labelMaximum.Hide();
            lblTotal.Hide();
            barImport.Hide();
            this.btnImportFile.Hide();
            G1.AddSummaryColumn("payment", gridMain);
            G1.AddSummaryColumn("interest", gridMain);
            G1.AddSummaryColumn("principal", gridMain);
            G1.AddSummaryColumn("posted", gridMain);
            if (bankDt != null)
            {
                gridMain.Columns["ID"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                gridMain.Columns["ID"].SummaryItem.DisplayFormat = "{0:C2}";
                gridBand5.Caption = "Total Funerals : ";
                gridBand19.Caption = "Total Down Payment : ";
                gridBand20.Caption = "Total Deposit : ";
            }
            else
            {
                gridBand5.Children.Clear();
                gridBand5.Children.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] { this.gridBand2, this.gridBand3});
            }

            G1.AddSummaryColumn("payment", gridMain2);
            G1.AddSummaryColumn("interest", gridMain2);
            G1.AddSummaryColumn("principal", gridMain2);
            G1.AddSummaryColumn("ccFee", gridMain2, "{0:C2}");

            G1.AddSummaryColumn("payment", gridMain3);

            LoadBankAccounts();

            loading = false;

            if (!String.IsNullOrWhiteSpace(workingSMFS_ACH_File))
            {
                ImportFileName = "";
                ImportFileDate = "";
                DataTable dt = null;
                dt = ImportDraftfile(workingSMFS_ACH_File);
                if (dt != null && dt.Rows.Count > 0)
                {
                    CalcMonthsPaid(dt);
                    CalculateGoodBadUgly(dt);
                    ProcessTrust85(dt);
                    DoubleCheckTrust85(dt);
                    CalculatePosted(dt);
                    picLoader.Hide();
                    LoadPaidOffTrusts(dt);
                    LoadSecNat(dt);
                    dgv.DataSource = dt;
                    originalDt = dt;
                    btnImportFile.Show();
                    btnImportFile.Refresh();
                    //CheckPaidOffContracts(dt); // Just use here for testing
                }
                tabControl1.SelectedIndex = 0;
                dgv2.DataSource = null;
                dgv3.DataSource = null;
            }
            if ( workBankCC )
            {
                ImportFileName = "";
                ImportFileDate = "";
                DataTable dt = null;
                dt = ImportBankfile();
                if (dt != null && dt.Rows.Count > 0)
                {
                    CalcMonthsPaid ( dt, true );
                    CalculateGoodBadUgly(dt);
                    ProcessTrust85(dt);
                    DoubleCheckTrust85(dt);
                    CalculatePosted(dt);
                    picLoader.Hide();
                    LoadPaidOffTrusts(dt);
                    LoadSecNat(dt);
                    dgv.DataSource = dt;
                    originalDt = dt;
                    btnImportFile.Show();
                    btnImportFile.Refresh();
                    //CheckPaidOffContracts(dt); // Just use here for testing
                }
            }
        }
        /***************************************************************************************/
        private void LoadBankAccounts()
        {
            string location = "";
            string bank_gl = "";
            string bankAccount = "";
            string cmd = "Select * from `bank_accounts` where `lkbx_ach` = '1';";
            DataTable dx  = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                lkbx_ach_account = location + "~" + bank_gl + "~" + bankAccount;
            }

            cmd = "Select * from `bank_accounts` where `cc` = '1';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                cc_account = location + "~" + bank_gl + "~" + bankAccount;
            }
            cmd = "Select * from `bank_accounts` where `ach` = '1';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                ach_account = location + "~" + bank_gl + "~" + bankAccount;
            }
            cmd = "Select * from `bank_accounts` where `tfbx` = '1';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                tfbx_account = location + "~" + bank_gl + "~" + bankAccount;
            }
            if (!getUpdateBankAccounts())
                return;
            bankAccount = lkbx_ach_account;
            if ( workTheFirst )
                bankAccount = tfbx_account;
            else if (workCC)
                bankAccount = cc_account;
            else if (workACH)
                bankAccount = ach_account;
            else if (workDraft)
                bankAccount = ach_account;
            this.gridBand2.Caption = "Lock Box Data - " + bankAccount;
        }
        /***********************************************************************************************/
        private void LoadComboWho()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("keycode");

            DataRow dRow = dt.NewRow();
            dRow["keycode"] = "ALL";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["keycode"] = "TRUSTS";
            dt.Rows.Add(dRow);

            dRow = dt.NewRow();
            dRow["keycode"] = "INSURANCE";
            dt.Rows.Add(dRow);

            if ( bankDt != null )
            {
                dRow = dt.NewRow();
                dRow["keycode"] = "FUNERALS";
                dt.Rows.Add(dRow);

                dRow = dt.NewRow();
                dRow["keycode"] = "DOWN PAYMENTS";
                dt.Rows.Add(dRow);
            }

            cmbWho.Properties.DataSource = dt;
            cmbWho.EditValue = "ALL";
        }
        /***********************************************************************************************/
        private void PullActiveData()
        {
            string cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` ";
            cmd += " where p.`lapsed` <> 'Y' and d.`lapsed` <> 'Y' ";
            if (!workACH)
                cmd += " AND p.`coverageType` <> 'I' "; // T
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("agreement");
            dt.Columns.Add("select");
            dt.Columns.Add("fullname");
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("purchase", Type.GetType("System.Double"));
            dt.Columns.Add("DOLP");
            dt.Columns.Add("cbal", Type.GetType("System.Double"));
            dt.Columns.Add("cint", Type.GetType("System.Double"));
            dt.Columns.Add("financed", Type.GetType("System.Double"));
            workDt = dt.Copy();
        }
        /***********************************************************************************************/
        private void HideSendMessage()
        {
            if (LoginForm.administrator)
                return;
            for (int i = 0; i < contextMenuStrip1.Items.Count; i++)
            {
                if (contextMenuStrip1.Items[i].Name.ToUpper().IndexOf("SENDMESSAGE") >= 0)
                    contextMenuStrip1.Items[i].Dispose();
            }
        }
        /***********************************************************************************************/
        private void CalcMonthsPaid( DataTable  dt, bool isBankCC = false )
        {
            if ( G1.get_column_number ( dt, "monthsPaid") < 0 )
                dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            double expected = 0D;
            double paid = 0D;
            double ccFee = 0D;
            double months = 0D;
            double newMonths = 0D;
            string code = "";
            DateTime dueDate = DateTime.Now;
            DateTime nextDate = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                code = dt.Rows[i]["code"].ObjToString();
                if (code == "02")
                {
                    //months = CheckMonthsForInsurance(dt, i);
                    //dt.Rows[i]["monthsPaid"] = months;
                }
                else
                {
                    ccFee = 0D;
                    expected = dt.Rows[i]["expected"].ObjToDouble();
                    paid = dt.Rows[i]["payment"].ObjToDouble();
                    if ( isBankCC )
                    {
                        ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                        paid = paid - ccFee;
                    }
                    months = 0D;
                    if (expected > 0D)
                        months = paid / expected;
                    dt.Rows[i]["monthsPaid"] = months;
                    if ( months > 1D)
                    {
                        dueDate = dt.Rows[i]["dueDate"].ObjToDateTime();
                        nextDate = dt.Rows[i]["nextDueDate"].ObjToDateTime();
                        if ( nextDate > dueDate && nextDate.ToString("MM/dd/yyyy") != "12/31/2039" )
                        {
                            var dateSpan = DateTimeSpan.CompareDates(dueDate, nextDate);
                            newMonths = dateSpan.Months;
                            if ( newMonths > months)
                                dt.Rows[i]["monthsPaid"] = newMonths;
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private double CheckMonthsForInsurance(DataTable dt, int row)
        {
            double months = CheckMonthsForInsurance(dt.Rows[row]);
            return months;
        }
        /***********************************************************************************************/
        private double CheckMonthsForInsurance(DataRow dRow)
        {
            double months = 0D;
            try
            {
                double expected = dRow["expected"].ObjToDouble();
                double paid = dRow["payment"].ObjToDouble();
                string payer = dRow["payer"].ObjToString().Trim();
                string contractNumber = dRow["cnum"].ObjToString();
                double creditBalance = dRow["creditBalance"].ObjToDouble();
                //                paid += creditBalance;

                DateTime pDate = dRow["payDate8"].ObjToDateTime();
                DateTime dDate = dRow["dueDate8"].ObjToDateTime();

                months = DailyHistory.CheckMonthsForInsurance(contractNumber, payer, expected, paid, pDate, dDate );
            }
            catch ( Exception ex)
            {
            }
            return months;
        }
        /***********************************************************************************************/
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    ImportFileName = "";
                    ImportFileDate = "";
                    DataTable dt = null;
                    if (workACH)
                        dt = ImportACHfile(file);
                    else if (workDraft)
                        dt = ImportDraftfile(file);
                    else if (workCC)
                        dt = ImportCCfile(file);
                    else
                        dt = ImportDailyfile(file);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        //CalcMonthsPaid(dt);
                        CalculateGoodBadUgly(dt);
                        ProcessTrust85(dt);
                        DoubleCheckTrust85(dt);
                        CalculatePosted(dt);
                        picLoader.Hide();
                        LoadPaidOffTrusts(dt);
                        LoadSecNat(dt);
                        dgv.DataSource = dt;
                        originalDt = dt;
                        btnImportFile.Show();
                        btnImportFile.Refresh();
                        //CheckPaidOffContracts(dt); // Just use here for testing
                    }
                    tabControl1.SelectedIndex = 0;
                    dgv2.DataSource = null;
                    dgv3.DataSource = null;
                }
            }
            if (picLoader != null)
                picLoader.Hide();
        }
        /***********************************************************************************************/
        private void LoadSecNat ( DataTable dt)
        {
            DataTable dx = dt.Clone();
            dx.Columns.Add("secnatAmount", Type.GetType("System.Double"));
            int row = 0;

            try
            {
                string status = "";
                string code = "";
                string payer = "";
                double monthlyPremium = 0D;
                double historicPremium = 0D;
                double monthlySecNat = 0D;
                double monthly3rdParty = 0D;
                double payment = 0D;
                double expected = 0D;
                double dMonths = 0D;
                int months = 0;
                DateTime secDate = new DateTime(2020, 7, 1);
                DateTime date = DateTime.Now;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    code = dt.Rows[i]["code"].ObjToString().ToUpper().Trim();
                    if (code != "02")
                        continue;
                    payer = dt.Rows[i]["payer"].ObjToString();
                    payment = dt.Rows[i]["payment"].ObjToDouble();
                    expected = dt.Rows[i]["expected"].ObjToDouble();
                    dMonths = dt.Rows[i]["monthsPaid"].ObjToDouble();
                    if (expected == payment)
                        continue;

                    date = dt.Rows[i]["date"].ObjToDateTime();
                    if (date < secDate)
                        continue;

                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty );
                    if (monthlySecNat <= 0D)
                        continue;
                    if (payment > (monthlyPremium - monthlySecNat))
                    {
                        dx.ImportRow(dt.Rows[i]);
                        row = dx.Rows.Count - 1;
                        dx.Rows[row]["secnatAmount"] = monthlySecNat;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            dgv6.DataSource = dx;
        }
        /***********************************************************************************************/
        private void LoadPaidOffTrusts( DataTable dt)
        {
            DataTable dx = dt.Clone();

            try
            {
                string status = "";
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    status = dt.Rows[i]["empty2"].ObjToString().ToUpper().Trim();
                    if (status == "BROKEN")
                        dx.ImportRow(dt.Rows[i]);
                    else if (status == "EXCEEDED")
                        dx.ImportRow(dt.Rows[i]);
                }
            }
            catch ( Exception ex)
            {
            }
            dgv4.DataSource = dx;
        }
        /***********************************************************************************************/
        private void DoubleCheckTrust85 ( DataTable dt)
        {
            if (G1.get_column_number(dt, "empty2") < 0)
                dt.Columns.Add("empty2");
            if (G1.get_column_number(dt, "retained") < 0)
                dt.Columns.Add("retained", Type.GetType("System.Double"));
            string contractNumber = "";
            double payments = 0D;
            double Trust85Max = 0D;
            double Trust85Real = 0D;
            double Trust85P = 0D;
            double Trust100P = 0D;
            double retained = 0D;
            double trust85Calc = 0D;
            double trust85Paid = 0D;
            double trust85Real = 0D;
            double payment = 0D;
            string found = "";
            string code = "";
            DateTime nextDueDate = DateTime.Now;
            DateTime payDate = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                code = dt.Rows[i]["code"].ObjToString();
                if (code != "01")
                    continue;
                dt.Rows[i]["retained"] = 0D;
                contractNumber = dt.Rows[i]["cnum"].ObjToString();
                payments = GetCurrentTrust85(dt, contractNumber);
                Trust85Max = dt.Rows[i]["Trust85Max"].ObjToDouble();
                Trust85Real = dt.Rows[i]["Trust85Paid"].ObjToDouble();
                Trust85P = dt.Rows[i]["Trust85P"].ObjToDouble();
                Trust100P = dt.Rows[i]["Trust100P"].ObjToDouble();
                nextDueDate = dt.Rows[i]["nextDueDate"].ObjToDateTime();
                payDate = dt.Rows[i]["date"].ObjToDateTime();
                payment = dt.Rows[i]["payment"].ObjToDouble();
                found = dt.Rows[i]["found"].ObjToString();
                if (String.IsNullOrWhiteSpace(found))
                {
                    if (Trust85Real > Trust85Max)
                        dt.Rows[i]["empty2"] = " Exceeded";
                    else if ((Trust85Real + payments) >= Trust85Max)
                        dt.Rows[i]["empty2"] = " Broken";
                    //Trust85.CalcTrust85Data(contractNumber, DateTime.Now, ref trust85Calc, ref trust85Paid, ref trust85Real);
                    //Trust85Real = trust85Paid + Trust85P;
                    //if ( nextDueDate.Year >= 2039)
                    //{
                    //    DeterminePayOffTrust85(contractNumber, payDate, payment, ref Trust85P, ref Trust100P, ref retained);
                    //    dt.Rows[i]["retained"] = retained;
                    //    dt.Rows[i]["Trust85P"] = Trust85P;
                    //    dt.Rows[i]["Trust100P"] = Trust100P;
                    //    dt.Rows[i]["empty2"] = " Broken";
                    //}
                    //else if (Trust85Real > Trust85Max)
                    //{
                    //    retained = Trust85Real - Trust85Max;
                    //    retained = G1.RoundValue(retained);
                    //    dt.Rows[i]["retained"] = retained;
                    //    Trust85P = Trust85P - retained;
                    //    Trust85P = G1.RoundValue(Trust85P);
                    //    dt.Rows[i]["Trust85P"] = Trust85P;
                    //    Trust100P = Trust85P / .85D;
                    //    Trust100P = G1.RoundValue(Trust100P);
                    //    dt.Rows[i]["Trust100P"] = Trust100P;
                    //    dt.Rows[i]["empty2"] = " Exceeded";
                    //    retained = payment - Trust100P;
                    //    dt.Rows[i]["retained"] = retained;
                    //}
                    //else if ((Trust85Real + payments) >= Trust85Max)
                    //{
                    //    retained = Trust85Real - Trust85Max;
                    //    retained = G1.RoundValue(retained);
                    //    dt.Rows[i]["retained"] = retained;
                    //    Trust85P = Trust85P - retained;
                    //    Trust85P = G1.RoundValue(Trust85P);
                    //    dt.Rows[i]["Trust85P"] = Trust85P;
                    //    Trust100P = Trust85P / .85D;
                    //    Trust100P = G1.RoundValue(Trust100P);
                    //    dt.Rows[i]["Trust100P"] = Trust100P;
                    //    dt.Rows[i]["empty2"] = " Broken";
                    //    retained = payment - Trust100P;
                    //    dt.Rows[i]["retained"] = retained;
                    //}
                }
            }
        }
        /***********************************************************************************************/
        private bool DeterminePayOffTrust85 ( string contractNumber, DateTime payDate8, double payment, ref double trust85P, ref double trust100P, ref double retained )
        {
            bool good = false;
            string cmd = "Select * from `Trust2013r` where `contractNumber` = '" + contractNumber + "' AND `payDate8` <= '" + payDate8.ToString("yyyy-MM-dd") + "' ORDER BY `payDate8` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return good;
            double Trust85Real = dt.Rows[0]["endingBalance"].ObjToDouble();
            DateTime charlotteDate = dt.Rows[0]["payDate8"].ObjToDateTime();

            double contractValue = DailyHistory.GetContractValue (contractNumber );

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` > '" + charlotteDate.ToString("yyyy-MM-dd") + "';";
            dt = G1.get_db_data(cmd);
            double sinceTrust85 = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
                sinceTrust85 += dt.Rows[i]["trust85P"].ObjToDouble();
            double TotalTrust85Paid = Trust85Real + sinceTrust85;

            double goal85 = contractValue * 0.85D;
            goal85 = G1.RoundDown(goal85);
            trust85P = goal85 - TotalTrust85Paid;
            trust85P = G1.RoundDown(trust85P);
            trust100P = trust85P / 0.85D;
            trust100P = G1.RoundDown(trust100P);
            retained = payment - trust100P;
            retained = G1.RoundDown(retained);

            return good;
        }
        /***********************************************************************************************/
        private void ProcessTrust85(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string contractNumber = "";
            string str = "";
            string cmd = "";
            double contractValue = 0D;
            double trust85Max = 0D;
            double trust100Max = 0D;
            double trust85Calc = 0D;
            double trust85Paid = 0D;
            double trust85Real = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                picLoader.Refresh();
                if (str.ToUpper() == "NO" || str.ToUpper() == "L")
                    continue;
                contractNumber = dt.Rows[i]["cnum"].ObjToString();
                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                DataTable contractDt = G1.get_db_data(cmd);
                if (contractDt.Rows.Count <= 0)
                    continue;
                contractValue = DailyHistory.GetContractValuePlus(contractDt.Rows[0]);
                dt.Rows[i]["contractValue"] = contractValue;
                DailyHistory.CalcTrust85(contractDt, ref trust85Max, ref trust100Max);
                trust85Max = contractValue * 0.85D;
                dt.Rows[i]["Trust85Max"] = trust85Max;
                Trust85.CalcTrust85Data(contractNumber, DateTime.Now, ref trust85Calc, ref trust85Paid, ref trust85Real);
                dt.Rows[i]["Trust85Paid"] = trust85Real;
                //dt.Rows[i]["Trust85Paid"] = trust85Paid;
            }
        }
        /***********************************************************************************************/
        private bool CalculateGoodBadUgly(DataTable dt)
        {
            bool good = true;
            if (dt != null && dt.Rows.Count > 0)
            {
                double total = 0D;
                double value = 0D;
                double badValue = 0D;
                double trustTotal = 0D;
                double insuranceTotal = 0D;
                double trustBad = 0D;
                double insuranceBad = 0D;
                double funeralBad = 0D;
                double funeralTotal = 0D;
                double dpBad = 0D;
                double dpTotal = 0D;
                double miscBad = 0D;
                string str = "";
                string cnum = "";
                string code = "";
                string location = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    picLoader.Refresh();

                    cnum = dt.Rows[i]["cnum"].ObjToString();
                    code = dt.Rows[i]["code"].ObjToString();
                    location = dt.Rows[i]["locationcode"].ObjToString();
                    value = dt.Rows[i]["payment"].ObjToDouble();
                    total += value;
                    str = dt.Rows[i]["found"].ObjToString();
                    if (str.ToUpper() == "NO" || str.ToUpper() == "L")
                    {
                        dt.Rows[i]["locationcode"] = location;
                    }
                    if (code != "01")
                    {
                        if (code == "02")
                        {
                            if (str.ToUpper() == "NO" || str.ToUpper() == "L")
                            {
                                insuranceBad += value;
                                cnum = dt.Rows[i]["cnum"].ObjToString();
                            }
                            else
                                insuranceTotal += value;
                        }
                        else if ( code == "03")
                        {
                            if (str.ToUpper() == "NO" || str.ToUpper() == "L")
                            {
                                funeralBad += value;
                                cnum = dt.Rows[i]["cnum"].ObjToString();
                            }
                            else
                                funeralTotal += value;
                        }
                        else if (code == "04")
                        {
                            dpTotal += value;
                        }
                        else
                        {
                            if (String.IsNullOrWhiteSpace(str))
                                trustTotal += value;
                            else
                            {
                                //                                trustBad += value;
                                miscBad += value;
                                cnum = dt.Rows[i]["cnum"].ObjToString();
                            }
                        }
                    }
                    else
                    {
                        if (str.ToUpper() == "NO" || str.ToUpper() == "L")
                        {
                            trustBad += value;
                            cnum = dt.Rows[i]["cnum"].ObjToString();
                        }
                        else
                            trustTotal += value;
                    }

                }
                double totalTrust = trustTotal + trustBad;
                double totalInsurance = insuranceTotal + insuranceBad;
                double goodValue = total - badValue;
                gridBand1.Caption = "Total Trust : $" + G1.ReformatMoney(trustTotal + trustBad) + "  Trust Found : $" + G1.ReformatMoney(trustTotal) + " Trust Not Found : $" + G1.ReformatMoney(trustBad);
                gridBand4.Caption = "Total Ins   : $" + G1.ReformatMoney(insuranceTotal + insuranceBad) + "  Ins Found   : $" + G1.ReformatMoney(insuranceTotal) + " Ins Not Found   : $" + G1.ReformatMoney(insuranceBad);
                if ( bankDt == null )
                    gridBand5.Caption = " Total Deposit : $" + G1.ReformatMoney(total) + "  Misc Not Found   : $" + G1.ReformatMoney(miscBad);
                else
                {
                    gridBand5.Caption = " Total Funeral : $" + G1.ReformatMoney(funeralTotal) + "  Misc Not Found   : $" + G1.ReformatMoney(funeralBad);
                    gridBand19.Caption = " Total Down Payment : $" + G1.ReformatMoney(dpTotal);
                    gridBand20.Caption = " Total Deposit : $" + G1.ReformatMoney(total) + "  Misc Not Found   : $" + G1.ReformatMoney(miscBad);

                    //if (dpTotal > 0D)
                    //    gridBand20.Caption = " Total Deposit : $" + G1.ReformatMoney(total) + "  Misc Not Found   : $" + G1.ReformatMoney(miscBad) + " Total Trust DP : $" + G1.ReformatMoney(dpTotal);
                    //else
                    //    gridBand20.Caption = " Total Deposit : $" + G1.ReformatMoney(total) + "  Misc Not Found   : $" + G1.ReformatMoney(miscBad);
                }
                if (insuranceBad > 0D || trustBad > 0D || miscBad > 0D )
                    good = false;
            }
            return good;
        }
        /***********************************************************************************************/
        private string parseFilename(string filename)
        {
            string rv = "";
            string tempfile = filename;
            if (filename.ToUpper().IndexOf(".XLS") > 0 || filename.ToUpper().IndexOf(".XLSX") > 0)
            {
                MessageBox.Show("***ERROR*** Not a CSV of FLAT ASCII File!");
                return "-1";
            }
            tempfile = tempfile.Replace("\\", "/");
            try
            {
                int idx = tempfile.IndexOf(":");
                if (idx >= 0)
                    tempfile = tempfile.Substring(idx + 1);
                string[] Lines = tempfile.Split('/');
                if (Lines.Length > 0)
                    rv = Lines[Lines.Length - 1].Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Parsing Filename " + filename);
            }
            return rv;
        }
        /***********************************************************************************************/
        public static string parseFileDate(string filename)
        {
            string rv = "";
            string tempfile = filename.ToUpper();
            tempfile = tempfile.Replace(".TXT", "");
            try
            {
                char c = 'x';
                for (int i = 0; i < tempfile.Length; i++)
                {
                    c = tempfile[i];
                    if (rv.Length > 0)
                    {
                        if ((c >= '0' && c <= '9') || c == '-')
                            rv += c.ObjToString();
                    }
                    else if (c >= '0' && c <= '9')
                        rv += c.ObjToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Parsing Filename " + filename);
            }
            return rv;
        }
        /***********************************************************************************************/
        private DataTable ImportDailyfile(string filename)
        {
            if ( workTheFirst )
            {
                DateTime TFBXdate = dateTimePicker1.Value;
                DialogResult result = MessageBox.Show("Are you sure you want to assign " + TFBXdate.ToString("MM/dd/yyyy") + " as the IMPORT DATE for this TFBX File?", "TFBX Import", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return null;
            }
            picLoader.Show();
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("name");
            dt.Columns.Add("code");
            dt.Columns.Add("locationcode");
            dt.Columns.Add("type");
            dt.Columns.Add("cnum");
            dt.Columns.Add("found");
            dt.Columns.Add("expected", Type.GetType("System.Double"));
            dt.Columns.Add("payment", Type.GetType("System.Double"));
            dt.Columns.Add("retained", Type.GetType("System.Double"));
            dt.Columns.Add("date");
            dt.Columns.Add("dueDate");
            dt.Columns.Add("agent");
            dt.Columns.Add("fname");
            dt.Columns.Add("lname");
            dt.Columns.Add("firstPayDate");
            dt.Columns.Add("contractDate");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("apr", Type.GetType("System.Double"));
            dt.Columns.Add("interest", Type.GetType("System.Double"));
            dt.Columns.Add("interestTotal", Type.GetType("System.Double"));
            dt.Columns.Add("amtOfMonthlyPayt", Type.GetType("System.Double"));
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("!contractRecord");
            dt.Columns.Add("oldbalance", Type.GetType("System.Double"));
            dt.Columns.Add("oldTotalInt", Type.GetType("System.Double"));
            dt.Columns.Add("principal", Type.GetType("System.Double"));
            dt.Columns.Add("totalPaid", Type.GetType("System.Double"));
            dt.Columns.Add("trust85P", Type.GetType("System.Double"));
            dt.Columns.Add("trust100P", Type.GetType("System.Double"));
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("days", Type.GetType("System.Int32"));
            dt.Columns.Add("issueDate");
            dt.Columns.Add("line");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Max", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Paid", Type.GetType("System.Double"));
            dt.Columns.Add("duplicates");
            dt.Columns.Add("payer");
            dt.Columns.Add("empty2");
            dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            dt.Columns.Add("oldCreditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("unpaid_interest", Type.GetType("System.Double"));
            dt.Columns.Add("ID");
            dt.Columns.Add("fill1");

            if (!File.Exists(filename))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                return null;
            }
            try
            {
                ImportFileName = filename;
                string tempfile = parseFilename(ImportFileName);
                if (tempfile == "-1")
                    return dt;
                ImportFileDate = parseFileDate(tempfile);
                if (!G1.validate_date(ImportFileDate))
                    ImportFileDate = DateTime.Now.ToString("MM/dd/yyyy");
                string line = "";
                int row = 0;
                int rowCount = 0;
                int lineWidth = 0;
                int width = 0;
                using (StreamReader sr = new StreamReader(filename))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        width = line.Trim().Length;
                        if (width > lineWidth)
                            lineWidth = width;
                        picLoader.Refresh();

                        rowCount++;
                    }
                    sr.Close();
                }

                bool newVersion = false;
                if (lineWidth == 36)
                    newVersion = true;

                barImport.Minimum = 0;
                barImport.Maximum = rowCount;
                lblTotal.Text = "of " + rowCount.ToString();
                barImport.Show();
                lblTotal.Show();
                labelMaximum.Show();
                lblTotal.Refresh();

                using (StreamReader sr = new StreamReader(filename))
                {
                    try
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            Application.DoEvents();
                            barImport.Value = row;
                            barImport.Refresh();
                            labelMaximum.Text = row.ToString();
                            labelMaximum.Refresh();

                            picLoader.Refresh();
                            bool rv = false;

                            //                        line = "0206FL-L3539410000330000376220190116";
                            // line = 010500B17026UI0127.94001279420250301 Length = 36
                            if ( workTheFirst )
                                rv = ParseTheFirstPayment(dt, line, newVersion );
                            else
                                rv = ParseOutPayment(dt, line);
                            //if (1 == 1)
                            //    break;
                            row++;
                        }
                    }
                    catch ( Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Reading File " + ex.Message.ToString());
                    }
                    sr.Close();
                    barImport.Value = rowCount;
                    labelMaximum.Text = rowCount.ToString();
                    labelMaximum.Refresh();

                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            G1.NumberDataTable(dt);
            //            picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private void CalculatePosted ( DataTable dt)
        {
            if (G1.get_column_number(dt, "posted") < 0)
                dt.Columns.Add("posted", Type.GetType("System.Double"));
            double interest = 0D;
            double principal = 0D;
            double posted = 0D;
            double creditBalance = 0D;
            double payment = 0D;
            string code = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                interest = dt.Rows[i]["interest"].ObjToDouble();
                principal = dt.Rows[i]["principal"].ObjToDouble();
                creditBalance = dt.Rows[i]["oldCreditBalance"].ObjToDouble();
                payment = dt.Rows[i]["payment"].ObjToDouble();

                code = dt.Rows[i]["code"].ObjToString();
                if (code == "02")
                {
                    posted = principal + interest;
                    dt.Rows[i]["posted"] = principal + interest;
                }
                else
                {
                    //posted = principal + interest - creditBalance;
                    posted = principal + interest;
                    //dt.Rows[i]["posted"] = principal + interest - creditBalance;
                    dt.Rows[i]["posted"] = principal + interest;
                }
            }
        }
        /***********************************************************************************************/
        private DataTable ReprocessACHData ( DataTable importDt)
        {
            SpecialACH = false;

            if (G1.get_column_number(importDt, "Report Type:") < 0)
                return importDt;

            importDt.Columns.RemoveAt(0);
            importDt.Columns.RemoveAt(0);
            importDt.Columns.RemoveAt(0);

            string colName = "";

            int row = -1;
            for ( int i=0; i<importDt.Rows.Count; i++)
            {
                Application.DoEvents();
                colName = importDt.Rows[i][0].ObjToString();
                if ( colName == "Payment Name")
                {
                    row = i;
                    for ( int j=0; j<importDt.Columns.Count; j++)
                    {
                        colName = importDt.Rows[i][j].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( colName))
                            importDt.Columns[j].ColumnName = colName;
                    }
                    break;
                }
            }
            if (row < 0)
                return importDt;

            for (int i = row; i >= 0; i--)
                importDt.Rows.RemoveAt(i);

            int col = G1.get_column_number(importDt, "Recipient ID");
            if (col < 0)
            {
                MessageBox.Show("***ERROR*** Cannot Find Column Recipient ID in Import File!!");
                return importDt;
            }
            importDt.Columns[col].ColumnName = "Customer Number";


            col = G1.get_column_number(importDt, "Recipient Name");
            if (col < 0)
            {
                MessageBox.Show("***ERROR*** Cannot Find Column Recipient Name in Import File!!");
                return importDt;
            }
            importDt.Columns[col].ColumnName = "Name On Account";

            col = G1.get_column_number(importDt, "Payment Amount (USD)");
            if (col < 0)
            {
                MessageBox.Show("***ERROR*** Cannot Find Column Payment Amount (USD) in Import File!!");
                return importDt;
            }
            importDt.Columns[col].ColumnName = "Amount";

            col = G1.get_column_number(importDt, "Tax ID Name");
            if (col < 0)
            {
                MessageBox.Show("***ERROR*** Cannot Find Column Tax ID Name in Import File!!");
                return importDt;
            }
            importDt.Columns[col].ColumnName = "Location Name";

            SpecialACH = true;

            string contract = "";
            string location = "";
            string trust = "";
            string loc = "";
            string oldloc = "";
            DataTable dx = null;
            string cmd = "Select * from `funeralhomes`;";
            DataTable funDt = G1.get_db_data(cmd);
            DataRow[] dRows = null;
            bool found = false;
            for ( int i=0; i<importDt.Rows.Count; i++)
            {
                try
                {
                    found = false;
                    contract = importDt.Rows[i]["Customer Number"].ObjToString();
                    cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                        dRows = funDt.Select("keycode='" + loc + "'");
                        if (dRows.Length > 0)
                        {
                            location = dRows[0]["LocationCode"].ObjToString();
                            found = true;
                        }
                    }
                    else
                    {
                        if ( contract == "UC-937")
                        {
                        }
                        if (contract == "UC-7511")
                        {
                        }

                        contract = contract.TrimStart('0');
                        contract = contract.Replace("NEW", "");
                        contract = contract.ToUpper().Replace("INSURANCE", "").Trim();
                        contract = contract.Replace(" ", "");
                        cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            for (int j = 0; j < dx.Rows.Count; j++)
                            {
                                location = dx.Rows[j]["oldloc"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(location))
                                {
                                    dRows = funDt.Select("keycode='" + location + "'");
                                    if (dRows.Length > 0)
                                    {
                                        location = dRows[0]["LocationCode"].ObjToString();
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (found)
                        importDt.Rows[i]["Location Name"] = location;
                    else
                    {
                        location = FindLastPaymentLocation(contract, ref oldloc);
                        //    importDt.Rows[i]["Location Name"] = "Brookhaven Funeral";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
            return importDt;
        }
        /***********************************************************************************************/
        public static DataTable payerFunDt = null;
        public static string FindLastPaymentLocation ( string payer, ref string oldloc )
        {
            if ( payer == "CC-50-425")
            {
            }
            string cmd = "";
            string cnum = "";
            if (payerFunDt == null)
            {
                cmd = "Select * from `funeralhomes`;";
                payerFunDt = G1.get_db_data(cmd);
            }

            string oldLocation = "";
            string bestLocation = "";
            string location = "";
            oldloc = "";
            DataRow[] dRows = null;
            DataTable dt = null;
            cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    cnum = dx.Rows[j]["contractNumber"].ObjToString();
                    location = dx.Rows[j]["oldloc"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(location))
                    {
                        oldloc = location;
                        if (!String.IsNullOrWhiteSpace(location))
                        {
                            dRows = payerFunDt.Select("keycode='" + location + "'");
                            if (dRows.Length > 0)
                            {
                                location = dRows[0]["LocationCode"].ObjToString();
                                break;
                            }
                        }
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(cnum) && dx.Rows.Count > 0)
            {
                DateTime lastRealPayDate = new DateTime(100, 1, 1);
                DateTime lastPayDate = new DateTime(100, 1, 1);
                DateTime payDate8 = DateTime.Now;
                string loc = "";
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    cnum = dx.Rows[j]["contractNumber"].ObjToString();
                    if (cnum.ToUpper().Contains("MM") || cnum.ToUpper().Contains("OO"))
                        continue;
                    cmd = "Select * from `ipayments` where `contractNumber` = '" + cnum + "' ORDER by `payDate8` DESC LIMIT 20;";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        for (int k = 0; k < dt.Rows.Count; k++)
                        {
                            loc = dt.Rows[0]["location"].ObjToString();
                            if (loc.ToUpper() == "NONE" || loc.ToUpper() == "ACH")
                                continue;
                            payDate8 = dt.Rows[0]["payDate8"].ObjToDateTime();
                            if (payDate8 > lastRealPayDate && !String.IsNullOrWhiteSpace(loc))
                            {
                                lastRealPayDate = payDate8;
                                bestLocation = loc;
                                break;
                            }
                        }
                    }
                }
                if (String.IsNullOrWhiteSpace(bestLocation))
                {
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        cnum = dx.Rows[j]["contractNumber"].ObjToString();
                        if (cnum.ToUpper().Contains("MM") || cnum.ToUpper().Contains("OO"))
                            continue;
                        cmd = "Select * from `old_ipayments` where `contractNumber` = '" + cnum + "' ORDER by `payDate8` DESC LIMIT 20;";
                        dt = G1.get_db_data(cmd);
                        if (dt.Rows.Count > 0)
                        {
                            for (int k = 0; k < dt.Rows.Count; k++)
                            {
                                loc = dt.Rows[0]["location"].ObjToString();
                                if (loc.ToUpper() == "NONE" || loc.ToUpper() == "ACH")
                                    continue;
                                payDate8 = dt.Rows[0]["payDate8"].ObjToDateTime();
                                if (payDate8 > lastPayDate && !String.IsNullOrWhiteSpace(loc))
                                {
                                    lastPayDate = payDate8;
                                    oldLocation = loc;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(bestLocation))
                location = bestLocation;
            else if (!String.IsNullOrWhiteSpace(oldLocation))
                location = oldLocation;
            if (String.IsNullOrWhiteSpace(location))
                location = "Brookhaven Funeral";

            return location;
        }
        /***********************************************************************************************/
        private bool SpecialACH = false;
        private DataTable ImportACHfile(string filename)
        {
            achDt = null;
            picLoader.Show();

            DateTime ACHdate = dateTimePicker1.Value;
            DialogResult result = MessageBox.Show("Are you sure you want to assign " + ACHdate.ToString("MM/dd/yyyy") + " as the IMPORT DATE for this ACH File?", "ACH Import", MessageBoxButtons.YesNo);
            if ( result == DialogResult.No )
                return null;

            DataTable importDt = Import.ImportCSVfile(filename);

            string fname = parseFilename(filename);
            if ( !String.IsNullOrWhiteSpace ( fname))
            {
                ImportFileName = fname;
                ImportFileDate = parseFileDate(fname);
                if ( G1.validate_date ( ImportFileDate))
                {
                    DateTime date = ImportFileDate.ObjToDateTime();
                    ImportFileDate = date.ToString("MM/dd/yyyy");
                }
            }

            importDt = ReprocessACHData(importDt);

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("name");
            dt.Columns.Add("code");
            dt.Columns.Add("locationcode");
            dt.Columns.Add("type");
            dt.Columns.Add("cnum");
            dt.Columns.Add("found");
            dt.Columns.Add("expected", Type.GetType("System.Double"));
            dt.Columns.Add("payment", Type.GetType("System.Double"));
            dt.Columns.Add("date");
            dt.Columns.Add("dueDate");
            dt.Columns.Add("agent");
            dt.Columns.Add("fname");
            dt.Columns.Add("lname");
            dt.Columns.Add("firstPayDate");
            dt.Columns.Add("contractDate");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("apr", Type.GetType("System.Double"));
            dt.Columns.Add("interest", Type.GetType("System.Double"));
            dt.Columns.Add("interestTotal", Type.GetType("System.Double"));
            dt.Columns.Add("amtOfMonthlyPayt", Type.GetType("System.Double"));
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("!contractRecord");
            dt.Columns.Add("oldbalance", Type.GetType("System.Double"));
            dt.Columns.Add("oldTotalInt", Type.GetType("System.Double"));
            dt.Columns.Add("principal", Type.GetType("System.Double"));
            dt.Columns.Add("totalPaid", Type.GetType("System.Double"));
            dt.Columns.Add("trust85P", Type.GetType("System.Double"));
            dt.Columns.Add("trust100P", Type.GetType("System.Double"));
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("days", Type.GetType("System.Int32"));
            dt.Columns.Add("issueDate");
            dt.Columns.Add("line");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Max", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Paid", Type.GetType("System.Double"));
            dt.Columns.Add("duplicates");
            dt.Columns.Add("payer");
            dt.Columns.Add("otherPayer");
            dt.Columns.Add("empty2");
            dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            dt.Columns.Add("oldCreditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("unpaid_interest", Type.GetType("System.Double"));
            dt.Columns.Add("ID");
            dt.Columns.Add("fill1");

            dgv.DataSource = dt;
            if (importDt == null)
                return null;

            if (!File.Exists(filename))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                return null;
            }
            try
            {
                ImportFileName = filename;
                string tempfile = parseFilename(ImportFileName);
                if (tempfile == "-1")
                    return importDt;
                int lastRow = importDt.Rows.Count;
                //lastRow = 23; // For Testing !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                string cnum = "";
                string cmd = "";
                string contractNumber = "";
                DataTable tempDt = null;
                DataTable copyDt = importDt.Clone();

                for (int i = 0; i < lastRow; i++)
                {
                    Application.DoEvents();
                    cnum = importDt.Rows[i]["Customer Number"].ObjToString();
                    cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    tempDt = G1.get_db_data(cmd);
                    if (tempDt.Rows.Count <= 0)
                    {
                        cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                        tempDt = G1.get_db_data(cmd);
                        if (tempDt.Rows.Count > 0)
                        {
                            cnum = tempDt.Rows[0]["contractNumber"].ObjToString();
                            cmd = "Select * from `tied_customers` where `contractNumber` = '" + cnum + "';";
                            tempDt = G1.get_db_data(cmd);
                            if (tempDt.Rows.Count > 0)
                            {
                                for (int j = 0; j < tempDt.Rows.Count; j++)
                                {
                                    contractNumber = tempDt.Rows[j]["tied_cnum"].ObjToString();
                                    G1.copy_dt_row(importDt, i, copyDt, (copyDt.Rows.Count));
                                    if (contractNumber != cnum)
                                    {
                                        copyDt.Rows[(copyDt.Rows.Count - 1)]["Customer Number"] = tempDt.Rows[j]["payer"].ObjToString();
                                        copyDt.Rows[(copyDt.Rows.Count - 1)]["Amount"] = tempDt.Rows[j]["amount"].ObjToString();
                                    }
                                    else
                                        copyDt.Rows[(copyDt.Rows.Count - 1)]["Amount"] = tempDt.Rows[j]["amount"].ObjToString();
                                }

                            }
                            else
                                G1.copy_dt_row(importDt, i, copyDt, (copyDt.Rows.Count));
                        }
                        else
                            G1.copy_dt_row(importDt, i, copyDt, (copyDt.Rows.Count));
                    }
                    else
                        G1.copy_dt_row(importDt, i, copyDt, copyDt.Rows.Count);
                    picLoader.Refresh();
                }

                lastRow = copyDt.Rows.Count;

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                lblTotal.Text = "of " + lastRow.ToString();
                barImport.Show();
                lblTotal.Show();
                labelMaximum.Show();
                lblTotal.Refresh();

                for (int i = 0; i < lastRow; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i + 1;
                    barImport.Refresh();
                    labelMaximum.Text = (i + 1).ToString();
                    labelMaximum.Refresh();

                    cnum = copyDt.Rows[i]["Customer Number"].ObjToString();
                    bool rv = ParseACHPayment(copyDt, i, dt);
                    picLoader.Refresh();
                }

                barImport.Value = lastRow;
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();

                achDt = copyDt.Clone();
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            G1.NumberDataTable(dt);
            //            picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ImportDraftfile(string filename)
        {
            achDt = null;
            picLoader.Show();

            string cnum = "";
            string cmd = "";
            string contractNumber = "";
            string payer = "";
            DataTable payerDt = null;

            DateTime ACHdate = dateTimePicker1.Value;
            DialogResult result = MessageBox.Show("Are you sure you want to assign " + ACHdate.ToString("MM/dd/yyyy") + " as the IMPORT DATE for this ACH File?", "ACH Import", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
                return null;

            DataTable importDt = Import.ImportCSVfile(filename);

            if ( importDt.Rows.Count > 0)
            {
                if ( G1.get_column_number ( importDt, "code") < 0 )
                {
                    importDt.Columns["Customer Number"].ColumnName = "contractNumber";
                    importDt.Columns.Add("code");
                    importDt.Columns.Add("payer");
                    if ( G1.get_column_number ( importDt, "Effective Date") < 0 )
                        importDt.Columns.Add("Effective Date");
                    DateTime effectiveDate = this.dateTimePicker1.Value;
                    for ( int i=0; i<importDt.Rows.Count; i++)
                    {
                        contractNumber = importDt.Rows[i]["contractNumber"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( contractNumber ))
                        {
                            if (DailyHistory.isInsurance(contractNumber))
                            {
                                importDt.Rows[i]["code"] = "02";
                                importDt.Rows[i]["payer"] = contractNumber;
                            }
                            else
                                importDt.Rows[i]["code"] = "01";
                            importDt.Rows[i]["Effective Date"] = effectiveDate.ToString("MM/dd/yyyy");
                        }
                    }
                }
            }

            string fname = parseFilename(filename);
            if (!String.IsNullOrWhiteSpace(fname))
            {
                ImportFileName = fname;
                ImportFileDate = parseFileDate(fname);
                if (G1.validate_date(ImportFileDate))
                {
                    DateTime date = ImportFileDate.ObjToDateTime();
                    ImportFileDate = date.ToString("MM/dd/yyyy");
                }
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("name");
            dt.Columns.Add("code");
            dt.Columns.Add("locationcode");
            dt.Columns.Add("type");
            dt.Columns.Add("cnum");
            dt.Columns.Add("found");
            dt.Columns.Add("expected", Type.GetType("System.Double"));
            dt.Columns.Add("payment", Type.GetType("System.Double"));
            dt.Columns.Add("date");
            dt.Columns.Add("dueDate");
            dt.Columns.Add("agent");
            dt.Columns.Add("fname");
            dt.Columns.Add("lname");
            dt.Columns.Add("firstPayDate");
            dt.Columns.Add("contractDate");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("apr", Type.GetType("System.Double"));
            dt.Columns.Add("interest", Type.GetType("System.Double"));
            dt.Columns.Add("interestTotal", Type.GetType("System.Double"));
            dt.Columns.Add("amtOfMonthlyPayt", Type.GetType("System.Double"));
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("!contractRecord");
            dt.Columns.Add("oldbalance", Type.GetType("System.Double"));
            dt.Columns.Add("oldTotalInt", Type.GetType("System.Double"));
            dt.Columns.Add("principal", Type.GetType("System.Double"));
            dt.Columns.Add("totalPaid", Type.GetType("System.Double"));
            dt.Columns.Add("trust85P", Type.GetType("System.Double"));
            dt.Columns.Add("trust100P", Type.GetType("System.Double"));
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("days", Type.GetType("System.Int32"));
            dt.Columns.Add("issueDate");
            dt.Columns.Add("line");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Max", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Paid", Type.GetType("System.Double"));
            dt.Columns.Add("duplicates");
            dt.Columns.Add("payer");
            dt.Columns.Add("otherPayer");
            dt.Columns.Add("empty2");
            dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            dt.Columns.Add("oldCreditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("unpaid_interest", Type.GetType("System.Double"));
            dt.Columns.Add("ID");
            dt.Columns.Add("fill1");
            if (importDt == null)
                return null;

            if (!File.Exists(filename))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                return null;
            }
            try
            {
                ImportFileName = filename;
                string tempfile = parseFilename(ImportFileName);
                if (tempfile == "-1")
                    return importDt;
                int lastRow = importDt.Rows.Count;
                //lastRow = 23; // For Testing !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                DataTable tempDt = null;
                DataTable copyDt = importDt.Clone();
                string code = "";

                for (int i = 0; i < lastRow; i++)
                {
                    Application.DoEvents();
                    code = importDt.Rows[i]["code"].ObjToString();
                    if ( code == "02")
                        cnum = importDt.Rows[i]["payer"].ObjToString();
                    else
                        cnum = importDt.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    tempDt = G1.get_db_data(cmd);
                    if (tempDt.Rows.Count <= 0)
                    {
                        cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                        tempDt = G1.get_db_data(cmd);
                        if (tempDt.Rows.Count > 0)
                        {
                            cnum = tempDt.Rows[0]["contractNumber"].ObjToString();
                            cmd = "Select * from `tied_customers` where `contractNumber` = '" + cnum + "';";
                            tempDt = G1.get_db_data(cmd);
                            if (tempDt.Rows.Count > 0)
                            {
                                for (int j = 0; j < tempDt.Rows.Count; j++)
                                {
                                    contractNumber = tempDt.Rows[j]["tied_cnum"].ObjToString();
                                    G1.copy_dt_row(importDt, i, copyDt, (copyDt.Rows.Count));
                                    if (contractNumber != cnum)
                                    {
                                        copyDt.Rows[(copyDt.Rows.Count - 1)]["contractNumber"] = tempDt.Rows[j]["payer"].ObjToString();
                                        copyDt.Rows[(copyDt.Rows.Count - 1)]["payment"] = tempDt.Rows[j]["payment"].ObjToString();
                                    }
                                    else
                                        copyDt.Rows[(copyDt.Rows.Count - 1)]["payment"] = tempDt.Rows[j]["payment"].ObjToString();
                                }

                            }
                            else
                                G1.copy_dt_row(importDt, i, copyDt, (copyDt.Rows.Count));
                        }
                        else
                            G1.copy_dt_row(importDt, i, copyDt, (copyDt.Rows.Count));
                    }
                    else
                        G1.copy_dt_row(importDt, i, copyDt, copyDt.Rows.Count);
                    picLoader.Refresh();
                }

                lastRow = copyDt.Rows.Count;

                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                lblTotal.Text = "of " + lastRow.ToString();
                barImport.Show();
                lblTotal.Show();
                labelMaximum.Show();
                lblTotal.Refresh();

                for (int i = 0; i < lastRow; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i + 1;
                    barImport.Refresh();
                    labelMaximum.Text = (i + 1).ToString();
                    labelMaximum.Refresh();

                    cnum = copyDt.Rows[i]["contractNumber"].ObjToString();
                    bool rv = ParseDraftPayment(copyDt, i, dt);
                    picLoader.Refresh();
                }

                barImport.Value = lastRow;
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();

                achDt = copyDt.Clone();
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            G1.NumberDataTable(dt);
            //            picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ImportBankfile()
        {
            achDt = null;
            picLoader.Show();

            string cnum = "";
            string cmd = "";
            string contractNumber = "";
            string payer = "";

            DataTable importDt = bankDt.Copy();

            this.gridBand2.Caption = "Bank Credit Card Data";
            this.gridBand9.Caption = "Bank Credit Card Data";
            this.gridBand11.Caption = "Bank Credit Card Data";
            this.gridBand13.Caption = "Bank Credit Card Data";
            this.gridBand15.Caption = "Bank Credit Card Data";
            this.gridBand17.Caption = "Bank Credit Card Data";

            DataTable dt = new DataTable();

            dt.Columns.Add("num");
            dt.Columns.Add("name");
            dt.Columns.Add("code");
            dt.Columns.Add("locationcode");
            dt.Columns.Add("type");
            dt.Columns.Add("cnum");
            dt.Columns.Add("found");
            dt.Columns.Add("expected", Type.GetType("System.Double"));
            dt.Columns.Add("payment", Type.GetType("System.Double"));
            dt.Columns.Add("retained", Type.GetType("System.Double"));
            dt.Columns.Add("date");
            dt.Columns.Add("dueDate");
            dt.Columns.Add("agent");
            dt.Columns.Add("fname");
            dt.Columns.Add("lname");
            dt.Columns.Add("firstPayDate");
            dt.Columns.Add("contractDate");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("apr", Type.GetType("System.Double"));
            dt.Columns.Add("interest", Type.GetType("System.Double"));
            dt.Columns.Add("interestTotal", Type.GetType("System.Double"));
            dt.Columns.Add("amtOfMonthlyPayt", Type.GetType("System.Double"));
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("!contractRecord");
            dt.Columns.Add("oldbalance", Type.GetType("System.Double"));
            dt.Columns.Add("oldTotalInt", Type.GetType("System.Double"));
            dt.Columns.Add("principal", Type.GetType("System.Double"));
            dt.Columns.Add("totalPaid", Type.GetType("System.Double"));
            dt.Columns.Add("trust85P", Type.GetType("System.Double"));
            dt.Columns.Add("trust100P", Type.GetType("System.Double"));
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("days", Type.GetType("System.Int32"));
            dt.Columns.Add("issueDate");
            dt.Columns.Add("line");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Max", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Paid", Type.GetType("System.Double"));
            dt.Columns.Add("duplicates");
            dt.Columns.Add("payer");
            dt.Columns.Add("empty2");
            dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            dt.Columns.Add("oldCreditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("unpaid_interest", Type.GetType("System.Double"));
            dt.Columns.Add("ID");
            dt.Columns.Add("fill1");
            dt.Columns.Add("depositNumber");
            dt.Columns.Add("bankAccount");
            dt.Columns.Add("ccFee", Type.GetType("System.Double" ));

            string importWhat = "";

            try
            {
                int lastRow = importDt.Rows.Count;
                //lastRow = 23; // For Testing !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                DataTable tempDt = null;
                string depositNumber = "";
                string bankAccount = "";
                double ccFee = 0D;
                string code = "";
                barImport.Minimum = 0;
                barImport.Maximum = lastRow;
                lblTotal.Text = "of " + lastRow.ToString();
                barImport.Show();
                lblTotal.Show();
                labelMaximum.Show();
                lblTotal.Refresh();

                string line = "";
                int row = 0;

                for (int i = 0; i < lastRow; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i + 1;
                    barImport.Refresh();
                    labelMaximum.Text = (i + 1).ToString();
                    labelMaximum.Refresh();

                    line = importDt.Rows[i]["line"].ObjToString();
                    bankAccount = importDt.Rows[i]["bankAccount"].ObjToString();
                    depositNumber = importDt.Rows[i]["depositNumber"].ObjToString();
                    ccFee = importDt.Rows[i]["fee"].ObjToDouble();

                    bool rv = ParseOutPayment(dt, line, importDt, i );

                    row = dt.Rows.Count - 1;
                    if ( row >= 0 )
                    {
                        dt.Rows[row]["bankAccount"] = bankAccount;
                        dt.Rows[row]["depositNumber"] = depositNumber;
                        dt.Rows[row]["ccFee"] = ccFee;
                        dt.Rows[row]["type"] = "CC";
                        code = dt.Rows[row]["code"].ObjToString();
                        if ( code == "04" || code == "03")
                            dt.Rows[row]["found"] = "";
                        gridMain.Columns["ID"].Caption = "CC Fee";
                    }

                    picLoader.Refresh();
                }

                barImport.Value = lastRow;
                labelMaximum.Text = lastRow.ToString();
                labelMaximum.Refresh();
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            G1.NumberDataTable(dt);
            //            picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ImportCCfile(string filename)
        {
            picLoader.Show();

            DataTable importDt = Import.ImportCCfile(filename);

            string fname = parseFilename(filename);
            if (!String.IsNullOrWhiteSpace(fname))
            {
                ImportFileName = fname;
                ImportFileDate = parseFileDate(fname);
                if (G1.validate_date(ImportFileDate))
                {
                    DateTime date = ImportFileDate.ObjToDateTime();
                    ImportFileDate = date.ToString("MM/dd/yyyy");
                }
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("name");
            dt.Columns.Add("code");
            dt.Columns.Add("locationcode");
            dt.Columns.Add("type");
            dt.Columns.Add("cnum");
            dt.Columns.Add("found");
            dt.Columns.Add("expected", Type.GetType("System.Double"));
            dt.Columns.Add("payment", Type.GetType("System.Double"));
            dt.Columns.Add("date");
            dt.Columns.Add("dueDate");
            dt.Columns.Add("agent");
            dt.Columns.Add("fname");
            dt.Columns.Add("lname");
            dt.Columns.Add("firstPayDate");
            dt.Columns.Add("contractDate");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("apr", Type.GetType("System.Double"));
            dt.Columns.Add("interest", Type.GetType("System.Double"));
            dt.Columns.Add("interestTotal", Type.GetType("System.Double"));
            dt.Columns.Add("amtOfMonthlyPayt", Type.GetType("System.Double"));
            dt.Columns.Add("lastDatePaid8");
            dt.Columns.Add("!contractRecord");
            dt.Columns.Add("oldbalance", Type.GetType("System.Double"));
            dt.Columns.Add("oldTotalInt", Type.GetType("System.Double"));
            dt.Columns.Add("principal", Type.GetType("System.Double"));
            dt.Columns.Add("totalPaid", Type.GetType("System.Double"));
            dt.Columns.Add("trust85P", Type.GetType("System.Double"));
            dt.Columns.Add("trust100P", Type.GetType("System.Double"));
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("days", Type.GetType("System.Int32"));
            dt.Columns.Add("issueDate");
            dt.Columns.Add("line");
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Max", Type.GetType("System.Double"));
            dt.Columns.Add("Trust85Paid", Type.GetType("System.Double"));
            dt.Columns.Add("duplicates");
            dt.Columns.Add("payer");
            dt.Columns.Add("empty2");
            dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            dt.Columns.Add("oldCreditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("unpaid_interest", Type.GetType("System.Double"));
            dt.Columns.Add("ID");
            dt.Columns.Add("fill1");

            if (importDt == null)
                return null;

            if (!File.Exists(filename))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                return null;
            }
            try
            {
                ImportFileName = filename;
                string tempfile = parseFilename(ImportFileName);
                if (tempfile == "-1")
                    return importDt;

                for (int i = 0; i < importDt.Rows.Count; i++)
                {
                    bool rv = ParseCCPayment(importDt, i, dt);
                    picLoader.Refresh();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
            G1.NumberDataTable(dt);
            //            picLoader.Hide();
            return dt;
        }
        /***********************************************************************************************/
        private double GetLastInterestPaid(string contractNumber, string payer, ref DateTime dolp, ref double expected)
        {
            expected = 0D;
            double rv = 0D;
            string cmd = "";
            DataTable dt = null;
            string paymentFile = "payments";
            if (contractNumber.ToUpper().IndexOf("ZZ") == 0)
                paymentFile = "ipayments";
            if (contractNumber.ToUpper().IndexOf("OO") == 0)
                paymentFile = "ipayments";
            if (contractNumber.ToUpper().IndexOf("MM") == 0)
                paymentFile = "ipayments";
            if ( paymentFile != "payments" && !String.IsNullOrWhiteSpace ( payer))
            {
                cmd = "SELECT * FROM `icustomers` WHERE `payer`= '" + payer + "';";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0)
                {
                    string list = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string contract = dt.Rows[i]["contractNumber"].ObjToString();
                        list += "'" + contract + "',";
                    }
                    list = list.TrimEnd(',');
                    list = "(" + list + ")";
                    cmd = "Select * from `" + paymentFile + "` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
                }
                else
                    cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC LIMIT 5";
            }
            else
                cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC LIMIT 5";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                ManualPayment.CleanupWork(dt);
                double credit = 0D;
                double debit = 0D;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        rv = dt.Rows[i]["interestPaid"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        //if (credit > 0D && rv == 0D) // Credits with zero (0.00) Interest do not count as (DOLP) // Removed because of M23002LI on 10/15/2024
                        //    continue;
                        if (credit != 0D || debit != 0D)
                            continue;
                        dolp = dt.Rows[i]["payDate8"].ObjToDateTime();
                        expected = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        break;
                    }
                }
            }
            return rv;
        }
        /***********************************************************************************************/
        private bool ParseACHPayment(DataTable importDt, int row, DataTable dt)
        {
            string found = "NO";
            string payer = "";

            if (G1.get_column_number(dt, "nextDueDate") < 0)
                dt.Columns.Add("nextDueDate");

            if (G1.get_column_number(dt, "duplicate") < 0)
                dt.Columns.Add("duplicate");

            if (G1.get_column_number(dt, "payer") < 0)
                dt.Columns.Add("payer");

            if (G1.get_column_number(dt, "monthsPaid") < 0)
                dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));

            if (G1.get_column_number(dt, "found") < 0)
                dt.Columns.Add("found" );

            if (G1.get_column_number(importDt, "code") < 0)
                importDt.Columns.Add("code");

            if (G1.get_column_number(importDt, "payer") < 0)
                importDt.Columns.Add("payer");

            if (G1.get_column_number(importDt, "found") < 0)
                importDt.Columns.Add("found");

            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            DateTime nextDueDate = DateTime.Now;
            DateTime lastDueDate = new DateTime(1910, 12, 31);
            double creditBalance = 0D;
            bool didIt = false;
            bool rtn = true;
            string originalPayer = "";
            bool duplicate = false;
            double originalCreditBalance = 0D;
            DateTime dueDate8 = DateTime.Now;
            string oldloc = "";

            try
            {

                if (!SpecialACH)
                {
                    string origin = importDt.Rows[row]["Payment Origin"].ObjToString();
                    if (origin.ToUpper() != "ORIGINAL SIGNATURE")
                        return false;
                }

                DateTime docp = this.dateTimePicker1.Value;

                //string str = importDt.Rows[row]["Transaction Date"].ObjToString();
                //string[] Lines = str.Split(' ');
                //str = Lines[0].Trim();
                //DateTime docp = str.ObjToDateTime();
                //if (1 == 1) // This allows Sandra to put in a correct Process Date from the bank allowing for holidays.
                //    docp = this.dateTimePicker1.Value;

                string date = docp.ToString("MM/dd/yyyy");

                string str = importDt.Rows[row]["amount"].ObjToString();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                double payment = 0D;
                if (G1.validate_numeric(str))
                    payment = str.ObjToDouble();

                string cnum = importDt.Rows[row]["Customer Number"].ObjToString();

                if (String.IsNullOrWhiteSpace(cnum))
                    cnum = "EMPTY #";

                string code = "01";
                string status = "PROCESSED";
                if ( !SpecialACH )
                    status = importDt.Rows[row]["status"].ObjToString();
                importDt.Rows[row]["code"] = code;

                string locationName = "";
                locationName = importDt.Rows[row]["Location Name"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationName) && SpecialACH)
                    locationName = "Brookhaven Funeral";

                string paymentsFile = "payments";
                bool insurance = false;

                originalPayer = cnum;

                cnum = cnum.TrimStart('0');
                cnum = cnum.Replace("NEW", "");
                cnum = cnum.ToUpper().Replace("INSURANCE", "").Trim();
                cnum = cnum.Replace(" ", "");

                if ( String.IsNullOrWhiteSpace ( cnum))
                {
                    string badName = "BAD PAYER NUMBER (" + originalPayer + ")";
                    if (G1.get_column_number(importDt, "Name On Account") >= 0)
                        badName += " " + importDt.Rows[row]["Name On Account"].ObjToString();
                    DataRow dR = dt.NewRow();
                    dR["code"] = "BAD";
                    dR["found"] = found;
                    dR["locationcode"] = locationName;
                    dR["type"] = "ACH";
                    dR["cnum"] = originalPayer;
                    dR["expected"] = 0D;
                    dR["payment"] = payment;
                    dR["date"] = date;
                    dR["name"] = badName;
                    dR["payer"] = originalPayer;
                    dR["otherPayer"] = payer;
                    dt.Rows.Add(dR);
                    return false;
                }

                double expected = 0D;

                string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                DataTable dxx = G1.get_db_data(cmd);
                if (dxx.Rows.Count <= 0)
                {
                    payer = cnum;
                    //if ( !String.IsNullOrWhiteSpace(cnum))
                    //    originalPayer = cnum;
                    paymentsFile = "ipayments";
                    //cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    //DataTable ddx = G1.get_db_data(cmd);
                    //if (ddx.Rows.Count > 0)
                    //{
                    //    insurance = true;
                    //    importDt.Rows[row]["payer"] = cnum;
                    //    importDt.Rows[row]["code"] = "02";
                    //    code = "02";
                    //    cnum = ddx.Rows[0]["contractNumber"].ObjToString();
                    //    insurance = true;
                    //}
                    string newPayer = "";
                    bool isLapsed = false;
                    cnum = FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref expected, ref isLapsed );
                    if ( !String.IsNullOrWhiteSpace ( newPayer))
                        payer = newPayer;



                    if ( SpecialACH )
                        expected = Policies.CalcMonthlyPremium(payer, docp);
                    else
                        expected = Policies.CalcMonthlyPremium(payer, docp);

                    if ( !String.IsNullOrWhiteSpace (cnum))
                    {
                        insurance = true;
                        importDt.Rows[row]["payer"] = originalPayer;
                        importDt.Rows[row]["code"] = "02";
                        if ( isLapsed )
                            importDt.Rows[row]["found"] = "L";
                        code = "02";
                        insurance = true;
                    }
                    else
                    {
                        string badName = "BAD PAYER NUMBER (" + originalPayer + ")";
                        if (G1.get_column_number(importDt, "Name On Account") >= 0)
                            badName += " " + importDt.Rows[row]["Name On Account"].ObjToString();
                        DataRow dR = dt.NewRow();
                        dR["code"] = "BAD";
                        dR["found"] = found;
                        dR["locationcode"] = locationName;
                        dR["type"] = "ACH";
                        dR["cnum"] = originalPayer;
                        dR["expected"] = expected;
                        dR["payment"] = payment;
                        dR["date"] = date;
                        dR["name"] = badName;
                        dR["payer"] = originalPayer;
                        dR["otherPayer"] = payer;
                        dt.Rows.Add(dR);
                        return false;
                    }
                }
                if (status.Trim().ToUpper() != "PROCESSED")
                {
                    DataRow dR = dt.NewRow();
                    dR["code"] = status;
                    dR["found"] = found;
                    dR["locationcode"] = locationName;
                    dR["type"] = "ACH";
                    dR["cnum"] = cnum;
                    dR["expected"] = expected;
                    dR["payment"] = payment;
                    dR["date"] = date;
                    dR["payer"] = originalPayer;
                    dR["otherPayer"] = payer;
                    dt.Rows.Add(dR);
                    return false;
                }

                found = "";
                string location = "ACH";
                DataRow[] dRows = null;
                if (insurance)
                {
                    DataTable ddx = null;
                    if ( SpecialACH )
                    {
                        if (payer == "UC-7511")
                        {
                        }
                        locationName = FindLastPaymentLocation(payer, ref oldloc);
                        importDt.Rows[row]["Location Name"] = locationName;
                        //cmd = "Select * from `ipayments` where `contractNumber` = '" + cnum + "' ORDER by `payDate8` DESC LIMIT 2;";
                        //ddx = G1.get_db_data(cmd);
                        //if ( ddx.Rows.Count > 0 )
                        //{
                        //    str = ddx.Rows[0]["location"].ObjToString();
                        //    if (!String.IsNullOrWhiteSpace(str))
                        //    {
                        //        locationName = str;
                        //        importDt.Rows[row]["Location Name"] = locationName;
                        //    }
                        //    else
                        //    {
                        //        locationName = FindLastPaymentLocation(payer, ref oldloc);
                        //        importDt.Rows[row]["Location Name"] = locationName;
                        //    }
                        //}
                        //if ( ddx.Rows.Count <= 0 || String.IsNullOrWhiteSpace ( locationName))
                        //{
                        //    locationName = FindLastPaymentLocation(payer, ref oldloc);
                        //    importDt.Rows[row]["Location Name"] = locationName;
                        //}

                    }
                    cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        dueDate8 = ddx.Rows[0]["dueDate8"].ObjToDateTime();
                        if (dueDate8.Year < 1000)
                            dueDate8 = docp;
                        //expected = Policies.CalcMonthlyPremium(payer, dueDate8);
                        expected = Policies.CalcMonthlyPremium(payer, docp);

                        originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                        dRows = ddx.Select("contractNumber = '" + cnum + "'");
                        str = ddx.Rows[0]["lapsed"].ObjToString();
                        if (str.ToUpper() == "Y")
                        {
                            found = "L";
                        }
                    }
                    else
                        found = "NO";
                }
                else
                {
                    dRows = workDt.Select("contractNumber = '" + cnum + "'");
                    if (dRows.Length <= 0)
                    {
                        found = "NO";
                        cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                        }
                    }
                    string trust = "";
                    string loc = "";
                    string ccc = Trust85.decodeContractNumber(cnum, ref trust, ref loc);
                    location = loc;
                }

                DataRow dRow = dt.NewRow();
                dRow["code"] = code;
                dRow["locationcode"] = locationName;
                dRow["type"] = "ACH";
                dRow["cnum"] = cnum;
                dRow["expected"] = expected;
                dRow["payment"] = payment;
                dRow["date"] = date;
                dRow["payer"] = originalPayer;
                dRow["otherPayer"] = payer;
                dRow["oldCreditBalance"] = originalCreditBalance;
                string agent = "";
                string fname = "";
                string lname = "";
                string firstPayDate = "";
                string contractDate = "";
                string issueDate = "";
                double balance = 0D;
                double oldbalance = 0D;
                double oldTotalInt = 0D;
                double oldOldBalance = 0D;
                double interest = 0D;
                double apr = 0D;
                double interestTotal = 0D;
                double amtOfMonthlyPayment = 0D;
                double contractValue = 0D;
                double maxTrust85 = 0D;
                double totalTrust85 = 0D;
                int numPayments = 0;
                string contractRecord = "";
                double principal = 0D;
                double totalPaid = 0D;
                double trust85P = 0D;
                double trust100P = 0D;
                double pExpected = 0D;
                double months = 0D;
                double unpaid_interest = 0D;
                double newTrust85 = 0D;
                double prevTrust85P = 0D;
                DateTime dolp = DateTime.Now;
                DateTime currentDueDate = DateTime.Now;
                int days = 0;
                if (dRows.Length > 0)
                {
                    agent = dRows[0]["agentCode"].ObjToString();
                    fname = dRows[0]["firstName"].ObjToString();
                    lname = dRows[0]["lastName"].ObjToString();
                    firstPayDate = dRows[0]["firstPayDate"].ObjToString();
                    contractDate = dRows[0]["contractDate"].ObjToString();
                    issueDate = dRows[0]["issueDate8"].ObjToString();
                    amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    if (SpecialACH)
                        amtOfMonthlyPayment = expected;
                    interestTotal = dRows[0]["TotalInterest"].ObjToDouble();
                    contractRecord = dRows[0]["record1"].ObjToString();

                    oldbalance = dRows[0]["balanceDue"].ObjToDouble();
                    oldTotalInt = dRows[0]["totalInterest"].ObjToDouble();
                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
                    didIt = true;

                    dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");
                    dRow["lastDatePaid8"] = dRows[0]["lastDatePaid8"].ObjToString();
                    dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();
                    if (dolp.Year < 1900)
                        dolp = issueDate.ObjToDateTime();

                    oldTotalInt = GetLastInterestPaid(cnum, payer, ref dolp, ref pExpected);
                    if (payment == pExpected)
                    {
                        expected = pExpected;
                        dRow["expected"] = expected;
                    }
                    else if ( (expected / payment) > 24D )
                    {
                        expected = pExpected;
                        dRow["expected"] = expected;
                    }

                    apr = dRows[0]["APR"].ObjToDouble();
                    dRow["dueDate"] = dRows[0]["dueDate8"].ObjToString();
                    dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");

                    //                dolp = new DateTime(2018, 4, 19);
                    duplicate = FindDOLP(dt, cnum, ref dolp, ref oldOldBalance, ref lastDueDate, ref prevTrust85P ); // Don't use this oldBalance
                    if (oldOldBalance > 0D)
                        oldbalance = oldOldBalance;
                    if (dolp.Year < 1300)
                    {
                        DateTime findDueDate = DateTime.Now;
                        DateTime findDOLP = DateTime.Now;
                        if (FindBestDateInfo(originalPayer, ref findDueDate, ref findDOLP))
                        {
                            dRow["dueDate"] = findDueDate.ToString("MM/dd/yyyy");
                            dRow["lastDatePaid8"] = findDOLP.ToString("MM/dd/yyyy");
                            dolp = findDOLP;
                        }
                        //                        dolp = new DateTime(2050, 12, 31);
                    }

                    try
                    {
                        if (dolp > docp)
                        {
                            cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' ORDER By `payDate8` DESC LIMIT 5";
                            DataTable dddx = G1.get_db_data(cmd);
                            if (dddx.Rows.Count > 0)
                            {
                                ManualPayment.CleanupWork(dddx);
                                for (int k = 0; k < dddx.Rows.Count; k++)
                                {
                                    DateTime d = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                    //                                if (d >= dolp)
                                    if (d.Year > 1850)
                                    {
                                        dolp = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    dRow["lastDatePaid8"] = dolp.ToString("MM/dd/yyyy");

                    //                DateTime docp = date.ObjToDateTime();
                    CalcPrincipalInterest(oldbalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest);
                    balance = oldbalance - principal;
                    balance = G1.RoundDown(balance);
                    interestTotal = oldTotalInt + interest;
                    interestTotal = G1.RoundDown(interestTotal);

                    HandleUnpaidInterest(cnum, payment, ref interest, ref unpaid_interest, ref principal, ref balance);

                    double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                    double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                    double downPayment = dRows[0]["downPayment"].ObjToDouble();
                    if (downPayment == 0D)
                        downPayment = DailyHistory.GetDownPaymentFromPayments(cnum);
                    double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                    numPayments = (int)financeDays;
                    double amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    contractValue = DailyHistory.GetContractValue(dRows[0]);
                    CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, apr, ref trust85P, ref trust100P);
                    if (days <= 0)
                        dRow["duplicates"] = "POSSIBLE DUPLICATE";
                }
                else
                {
                    cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    if (insurance)
                        cmd = "Select * from `icontracts` where `contractNumber` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        str = ddx.Rows[0]["lapsed"].ObjToString();
                        if (str.ToUpper() == "Y")
                        {
                            found = "L";
                        }
                    }
                }

                if ( !didIt )
                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);

                if (lastDueDate.Year > 1910)
                {
                    if (!DailyHistory.isInsurance(cnum))
                    {
                        lastDueDate = FindMismatches.VerifyDueDate(cnum);
                        double previousMonths = 0D;
                        if (FindPreviousPayments(dt, cnum, ref previousMonths))
                            months += previousMonths;
                    }

                    nextDueDate = lastDueDate.AddMonths(Convert.ToInt32(Math.Truncate(months)));
                }

                dRow["found"] = found;
                dRow["type"] = "ACH";
                dRow["agent"] = agent;
                dRow["fname"] = fname;
                dRow["lname"] = lname;
                if (!String.IsNullOrWhiteSpace(payer))
                    dRow["name"] = fname + " " + lname + " (" + originalPayer + ")";
                else
                    dRow["name"] = fname + " " + lname;
                if (duplicate)
                    dRow["duplicate"] = "Y";
                dRow["firstPayDate"] = firstPayDate;
                dRow["contractDate"] = contractDate;
                dRow["balance"] = balance;
                dRow["oldbalance"] = oldbalance;
                dRow["oldTotalInt"] = oldTotalInt;
                dRow["interest"] = interest;
                dRow["principal"] = principal;
                dRow["days"] = days;
                dRow["apr"] = (apr / 100.0D);
                dRow["!contractRecord"] = contractRecord;
                dRow["amtOfMonthlyPayt"] = amtOfMonthlyPayment;
                dRow["interestTotal"] = interestTotal;
                dRow["totalPaid"] = totalPaid;
                dRow["issueDate"] = issueDate;
                dRow["trust85P"] = trust85P;
                dRow["trust100P"] = trust100P;
                dRow["creditBalance"] = creditBalance;
                dRow["line"] = "";
                dRow["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                dRow["unpaid_interest"] = unpaid_interest;
                if (!DailyHistory.isInsurance(cnum) && balance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
                {
                    //newTrust85 = 0D;
                    //if (duplicate && dolp == docp)
                    //{
                    //    FindTrust85(dt, cnum, ref newTrust85);
                    //}
                    //bool isPaid = false;
                    //if (!cnum.Contains("LI"))
                    //    isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, trust85P + newTrust85);
                    //if (isPaid)
                    //{
                    //    double Trust85Paid = totalTrust85;
                    //    double difference = maxTrust85 - totalTrust85;
                    //    difference = G1.RoundValue(difference);

                    //    dRow["difference"] = difference;

                    //    dRow["duplicates"] = "FINALE! FINALE! FINALE!";
                    //    dRow["nextDueDate"] = "12/31/2039";
                    //    trust85P = trust85P + difference;
                    //    if (trust85P <= 0D)
                    //        trust85P = 0D;
                    //    dRow["trust85P"] = trust85P;
                    //    trust100P = trust85P / 0.85D;
                    //    trust100P = G1.RoundValue(trust100P);
                    //    dRow["trust100P"] = trust100P;
                    //}
                    string payoff = processNewPayOff(dt, dRow, cnum, duplicate, dolp, docp, amtOfMonthlyPayment, numPayments, contractValue, trust85P, trust100P, balance );
                }
                dt.Rows.Add(dRow);
            }
            catch (Exception ex)
            {

            }
            return rtn;
        }
        /***********************************************************************************************/
        private bool ParseDraftPayment(DataTable importDt, int row, DataTable dt)
        {
            string found = "NO";
            string payer = "";

            if (G1.get_column_number(dt, "nextDueDate") < 0)
                dt.Columns.Add("nextDueDate");

            if (G1.get_column_number(dt, "duplicate") < 0)
                dt.Columns.Add("duplicate");

            if (G1.get_column_number(dt, "payer") < 0)
                dt.Columns.Add("payer");

            if (G1.get_column_number(dt, "monthsPaid") < 0)
                dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));

            if (G1.get_column_number(dt, "found") < 0)
                dt.Columns.Add("found");

            if (G1.get_column_number(dt, "ID") < 0)
                dt.Columns.Add("ID");

            if (G1.get_column_number(importDt, "code") < 0)
                importDt.Columns.Add("code");

            if (G1.get_column_number(importDt, "payer") < 0)
                importDt.Columns.Add("payer");

            if (G1.get_column_number(importDt, "found") < 0)
                importDt.Columns.Add("found");

            if (G1.get_column_number(importDt, "ID") < 0)
                importDt.Columns.Add("ID");

            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            DateTime nextDueDate = DateTime.Now;
            DateTime lastDueDate = new DateTime(1910, 12, 31);
            double creditBalance = 0D;
            bool didIt = false;
            bool rtn = true;
            string originalPayer = "";
            bool duplicate = false;
            double originalCreditBalance = 0D;
            double actualMonthPaid = 0D;
            DateTime oldDueDate = DateTime.Now;
            string where = "Here1";

            try
            {

                string str = "";
                DateTime docp = importDt.Rows[row]["Effective Date"].ObjToDateTime();
                string date = docp.ToString("MM/dd/yyyy");

                str = importDt.Rows[row]["Amount"].ObjToString();
                str = str.Replace("$", "");
                str = str.Replace(",", "");
                double payment = 0D;
                if (G1.validate_numeric(str))
                    payment = str.ObjToDouble();

                string code = importDt.Rows[row]["code"].ObjToString();
                string ID = importDt.Rows[row]["ID"].ObjToString();

                payer = "";

                string cnum = importDt.Rows[row]["contractNumber"].ObjToString();
                if (code == "02")
                {
                    cnum = importDt.Rows[row]["payer"].ObjToString();
                    payer = cnum;
                }

                if (String.IsNullOrWhiteSpace(cnum))
                    cnum = "EMPTY #";

                string status = "PROCESSED";
                importDt.Rows[row]["code"] = code;

                string locationName = "";
                locationName = importDt.Rows[row]["Location"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationName) && SpecialACH)
                    locationName = "Colonial Bay Springs";


                string paymentsFile = "payments";
                bool insurance = false;

                originalPayer = cnum;

                cnum = cnum.TrimStart('0');
                cnum = cnum.Replace("NEW", "");
                cnum = cnum.ToUpper().Replace("INSURANCE", "").Trim();
                cnum = cnum.Replace(" ", "");

                if (cnum == "WM13055UI")
                {
                }

                if (String.IsNullOrWhiteSpace(cnum))
                {
                    string badName = "BAD PAYER NUMBER (" + originalPayer + ")";
                    if (G1.get_column_number(importDt, "Name On Account") >= 0)
                        badName += " " + importDt.Rows[row]["Name On Account"].ObjToString();
                    DataRow dR = dt.NewRow();
                    dR["code"] = "BAD";
                    dR["found"] = found;
                    dR["locationcode"] = "";
                    dR["type"] = "ACH";
                    dR["cnum"] = originalPayer;
                    dR["expected"] = 0D;
                    dR["payment"] = payment;
                    dR["date"] = date;
                    dR["name"] = badName;
                    dR["payer"] = originalPayer;
                    dR["otherPayer"] = payer;
                    dR["ID"] = ID;
                    dt.Rows.Add(dR);
                    return false;
                }

                double expected = 0D;

                string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                DataTable dxx = G1.get_db_data(cmd);
                if (dxx.Rows.Count <= 0)
                {
                    payer = cnum;
                    //if ( !String.IsNullOrWhiteSpace(cnum))
                    //    originalPayer = cnum;
                    paymentsFile = "ipayments";
                    //cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    //DataTable ddx = G1.get_db_data(cmd);
                    //if (ddx.Rows.Count > 0)
                    //{
                    //    insurance = true;
                    //    importDt.Rows[row]["payer"] = cnum;
                    //    importDt.Rows[row]["code"] = "02";
                    //    code = "02";
                    //    cnum = ddx.Rows[0]["contractNumber"].ObjToString();
                    //    insurance = true;
                    //}
                    string newPayer = "";
                    bool isLapsed = false;
                    cnum = FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref expected, ref isLapsed);
                    if (!String.IsNullOrWhiteSpace(newPayer))
                        payer = newPayer;
                    if (!String.IsNullOrWhiteSpace(cnum))
                    {
                        insurance = true;
                        expected = Policies.CalcMonthlyPremium ( payer, docp );
                        importDt.Rows[row]["payer"] = originalPayer;
                        importDt.Rows[row]["code"] = "02";
                        if (isLapsed)
                            importDt.Rows[row]["found"] = "L";
                        code = "02";
                        insurance = true;
                    }
                    else
                    {
                        string badName = "BAD PAYER NUMBER (" + originalPayer + ")";
                        if (G1.get_column_number(importDt, "Name On Account") >= 0)
                            badName += " " + importDt.Rows[row]["Name On Account"].ObjToString();
                        DataRow dR = dt.NewRow();
                        dR["code"] = "BAD";
                        dR["found"] = found;
                        dR["locationcode"] = locationName;
                        dR["type"] = "ACH";
                        dR["cnum"] = originalPayer;
                        dR["expected"] = expected;
                        dR["payment"] = payment;
                        dR["date"] = date;
                        dR["name"] = badName;
                        dR["payer"] = originalPayer;
                        dR["otherPayer"] = payer;
                        dR["ID"] = ID;
                        dt.Rows.Add(dR);
                        return false;
                    }
                }
                if (status.Trim().ToUpper() != "PROCESSED")
                {
                    DataRow dR = dt.NewRow();
                    dR["code"] = status;
                    dR["found"] = found;
                    dR["locationcode"] = locationName;
                    dR["type"] = "ACH";
                    dR["cnum"] = cnum;
                    dR["expected"] = expected;
                    dR["payment"] = payment;
                    dR["date"] = date;
                    dR["payer"] = originalPayer;
                    dR["otherPayer"] = payer;
                    dR["ID"] = ID;
                    dt.Rows.Add(dR);
                    return false;
                }

                found = "";
                string location = "ACH";
                DataRow[] dRows = null;
                if (insurance)
                {
                    cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        oldDueDate = ddx.Rows[0]["dueDate8"].ObjToDateTime();
                        originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                        dRows = ddx.Select("contractNumber = '" + cnum + "'");
                        str = ddx.Rows[0]["lapsed"].ObjToString();
                        if (str.ToUpper() == "Y")
                        {
                            found = "L";
                        }
                    }
                    else
                        found = "NO";
                }
                else
                {
                    dRows = workDt.Select("contractNumber = '" + cnum + "'");
                    if (dRows.Length <= 0)
                    {
                        found = "NO";
                        cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            oldDueDate = ddx.Rows[0]["dueDate8"].ObjToDateTime();
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                            else
                                found = "";
                        }
                    }
                    else
                    {
                        cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                    }
                    string trust = "";
                    string loc = "";
                    string ccc = Trust85.decodeContractNumber(cnum, ref trust, ref loc);
                    location = loc;
                }

                where = "Here2";
                DataRow dRow = dt.NewRow();
                dRow["code"] = code;
                dRow["locationcode"] = locationName;
                dRow["type"] = "ACH";
                dRow["cnum"] = cnum;
                dRow["expected"] = expected;
                dRow["payment"] = payment;
                dRow["date"] = date;
                dRow["payer"] = originalPayer;
                dRow["otherPayer"] = payer;
                dRow["oldCreditBalance"] = originalCreditBalance;
                dRow["ID"] = ID;
                string agent = "";
                string fname = "";
                string lname = "";
                string firstPayDate = "";
                string contractDate = "";
                string issueDate = "";
                double balance = 0D;
                double oldbalance = 0D;
                double oldTotalInt = 0D;
                double oldOldBalance = 0D;
                double interest = 0D;
                double apr = 0D;
                double interestTotal = 0D;
                double amtOfMonthlyPayment = 0D;
                double contractValue = 0D;
                int numPayments = 0;
                double maxTrust85 = 0D;
                double totalTrust85 = 0D;
                string contractRecord = "";
                double principal = 0D;
                double totalPaid = 0D;
                double trust85P = 0D;
                double trust100P = 0D;
                double pExpected = 0D;
                double months = 0D;
                double unpaid_interest = 0D;
                double newTrust85 = 0D;
                double prevTrust85P = 0D;
                double previousMonths = 0D;
                DateTime dolp = DateTime.Now;
                DateTime currentDueDate = DateTime.Now;
                int days = 0;
                if (dRows.Length > 0)
                {
                    agent = dRows[0]["agentCode"].ObjToString();
                    fname = dRows[0]["firstName"].ObjToString();
                    lname = dRows[0]["lastName"].ObjToString();
                    firstPayDate = dRows[0]["firstPayDate"].ObjToString();
                    contractDate = dRows[0]["contractDate"].ObjToString();
                    issueDate = dRows[0]["issueDate8"].ObjToString();
                    amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();

                    if (insurance)
                        amtOfMonthlyPayment = Policies.CalcMonthlyPremium(payer, docp);

                    interestTotal = dRows[0]["TotalInterest"].ObjToDouble();
                    contractRecord = dRows[0]["record1"].ObjToString();

                    oldbalance = dRows[0]["balanceDue"].ObjToDouble();
                    oldTotalInt = dRows[0]["totalInterest"].ObjToDouble();

                    if (FindPreviousPayments(dt, cnum, ref previousMonths))
                        originalCreditBalance = 0D;

                    double oldCreditBalance = FindPrevCreditBalance(dt, cnum, 0D); // DRAFTS
                    if (amtOfMonthlyPayment <= 0D)
                        actualMonthPaid = 0D;
                    else
                        actualMonthPaid = payment / amtOfMonthlyPayment;
                    //MessageBox.Show("Payment=" + payment.ToString() + " MonthlyPayment = " + amtOfMonthlyPayment + " Act Monty Paid = " + actualMonthPaid.ToString());
                    double dueDatePayment = payment + oldCreditBalance + originalCreditBalance;
                    double newActualMonthPaid = dueDatePayment / amtOfMonthlyPayment;
                    if ((oldCreditBalance + originalCreditBalance + (payment % amtOfMonthlyPayment)) > amtOfMonthlyPayment)
                        actualMonthPaid += 1D;

                    if (amtOfMonthlyPayment <= 0D)
                        actualMonthPaid = 0D;
                    //if ( !insurance )
                    //    payment = dueDatePayment;

                    oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();

                    where = "Here3";
                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
                    where = "Here4";
                    didIt = true;

                    originalCreditBalance = 0D;
                    currentDueDate = oldDueDate;

                    //dRow["monthsPaid"] = Math.Truncate(months);
                    //actualMonthPaid = Math.Truncate(actualMonthPaid);
                    if (!DailyHistory.isInsurance(cnum))
                        actualMonthPaid += 0.0000001D;
                    //MessageBox.Show("Act Monty Paid = " + actualMonthPaid.ToString());
                    dRow["monthsPaid"] = actualMonthPaid;
                    months = actualMonthPaid;
                    dRow["expected"] = amtOfMonthlyPayment;
                    dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");
                    dRow["lastDatePaid8"] = dRows[0]["lastDatePaid8"].ObjToString();
                    dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();
                    if (dolp.Year < 1900)
                        dolp = issueDate.ObjToDateTime();

                    where = "Here5";
                    oldTotalInt = GetLastInterestPaid(cnum, payer, ref dolp, ref pExpected);
                    if (payment == pExpected)
                    {
                        expected = pExpected;
                        dRow["expected"] = expected;
                    }
                    else if ((expected / payment) > 24D)
                    {
                        expected = pExpected;
                        dRow["expected"] = expected;
                    }

                    apr = dRows[0]["APR"].ObjToDouble();
                    dRow["dueDate"] = dRows[0]["dueDate8"].ObjToString();
                    dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");

                    //                dolp = new DateTime(2018, 4, 19);
                    lastDueDate = oldDueDate;

                    duplicate = FindDOLP(dt, cnum, ref dolp, ref oldOldBalance, ref lastDueDate, ref prevTrust85P ); // Don't use this oldBalance
                    if (oldOldBalance > 0D)
                        oldbalance = oldOldBalance;
                    if (dolp.Year < 1300)
                    {
                        DateTime findDueDate = DateTime.Now;
                        DateTime findDOLP = DateTime.Now;
                        if (FindBestDateInfo(originalPayer, ref findDueDate, ref findDOLP))
                        {
                            dRow["dueDate"] = findDueDate.ToString("MM/dd/yyyy");
                            dRow["lastDatePaid8"] = findDOLP.ToString("MM/dd/yyyy");
                            dolp = findDOLP;
                        }
                        //                        dolp = new DateTime(2050, 12, 31);
                    }

                    try
                    {
                        if (dolp > docp)
                        {
                            cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' ORDER By `payDate8` DESC LIMIT 5";
                            DataTable dddx = G1.get_db_data(cmd);
                            if (dddx.Rows.Count > 0)
                            {
                                ManualPayment.CleanupWork(dddx);
                                for (int k = 0; k < dddx.Rows.Count; k++)
                                {
                                    DateTime d = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                    //                                if (d >= dolp)
                                    if (d.Year > 1850)
                                    {
                                        dolp = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    dRow["lastDatePaid8"] = dolp.ToString("MM/dd/yyyy");

                    //                DateTime docp = date.ObjToDateTime();
                    CalcPrincipalInterest(oldbalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest);
                    balance = oldbalance - principal;
                    balance = G1.RoundDown(balance);
                    interestTotal = oldTotalInt + interest;
                    interestTotal = G1.RoundDown(interestTotal);

                    HandleUnpaidInterest(cnum, payment, ref interest, ref unpaid_interest, ref principal, ref balance);

                    double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                    double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                    double downPayment = dRows[0]["downPayment"].ObjToDouble();
                    if ( downPayment == 0D )
                        downPayment = DailyHistory.GetDownPaymentFromPayments(cnum);
                    double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                    numPayments = (int)financeDays;
                    double amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    contractValue = DailyHistory.GetContractValue(dRows[0]);
                    where = "Here7";
                    CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, apr, ref trust85P, ref trust100P);
                    if (days <= 0)
                        dRow["duplicates"] = "POSSIBLE DUPLICATE";
                    where = "Here71";
                }
                else
                {
                    cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    if (insurance)
                        cmd = "Select * from `icontracts` where `contractNumber` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        str = ddx.Rows[0]["lapsed"].ObjToString();
                        if (str.ToUpper() == "Y")
                        {
                            found = "L";
                        }
                    }
                }

                where = "Here72";
                if (!didIt)
                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);

                where = "Here73";
                if (lastDueDate.Year > 1910)
                {
                    if (!DailyHistory.isInsurance(cnum))
                    {
                        where = "Here74";
                        lastDueDate = FindMismatches.VerifyDueDate(cnum);
                        previousMonths = 0D;
                        where = "Here75";
                        if (FindPreviousPayments(dt, cnum, ref previousMonths))
                            months += previousMonths;
                        where = "Here76";
                    }

                    try
                    {
                        where = "Here741";
                        if (amtOfMonthlyPayment > 0D)
                        {
                            months = Math.Truncate((double)months);
                            int iimonths = Convert.ToInt32(months);
                            nextDueDate = lastDueDate.AddMonths(iimonths);
                            //MessageBox.Show("LastDueDate = " + lastDueDate.ToString("MM/dd/yyyy") + " iimonths=" + iimonths.ToString());
                            //MessageBox.Show("NextDueDate = " + nextDueDate.ToString("MM/dd/yyyy"));
                        }

                        where = "Here742";
                    }
                    catch ( Exception ex )
                    {
                        where = "Here777";
                        MessageBox.Show("LastDueDate = " + lastDueDate.ToString("MM/dd/yyyy") + " " + ex.Message.ToString());
                        nextDueDate = lastDueDate;
                    }
                    where = "Here77";
                }

                where = "Here8";
                dRow["found"] = found;
                dRow["type"] = "ACH";
                dRow["agent"] = agent;
                dRow["fname"] = fname;
                dRow["lname"] = lname;
                if (!String.IsNullOrWhiteSpace(payer))
                    dRow["name"] = fname + " " + lname + " (" + originalPayer + ")";
                else
                    dRow["name"] = fname + " " + lname;
                if (duplicate)
                    dRow["duplicate"] = "Y";
                dRow["firstPayDate"] = firstPayDate;
                dRow["contractDate"] = contractDate;
                dRow["balance"] = balance;
                dRow["oldbalance"] = oldbalance;
                dRow["oldTotalInt"] = oldTotalInt;
                dRow["interest"] = interest;
                dRow["principal"] = principal;
                dRow["days"] = days;
                dRow["apr"] = (apr / 100.0D);
                dRow["!contractRecord"] = contractRecord;
                dRow["amtOfMonthlyPayt"] = amtOfMonthlyPayment;
                dRow["interestTotal"] = interestTotal;
                dRow["totalPaid"] = totalPaid;
                dRow["issueDate"] = issueDate;
                dRow["trust85P"] = trust85P;
                dRow["trust100P"] = trust100P;
                dRow["creditBalance"] = creditBalance;
                dRow["line"] = "";
                dRow["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                dRow["unpaid_interest"] = unpaid_interest;
                dRow["ID"] = ID;
                where = "Here9";
                if (!DailyHistory.isInsurance(cnum) && balance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
                {
                    //newTrust85 = 0D;
                    //if (duplicate && dolp == docp)
                    //{
                    //    FindTrust85(dt, cnum, ref newTrust85);
                    //}
                    //bool isPaid = false;
                    //if (!cnum.Contains("LI"))
                    //    isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, trust85P + newTrust85 );
                    //if (isPaid)
                    //{
                    //    double Trust85Paid = totalTrust85;
                    //    double difference = maxTrust85 - totalTrust85;
                    //    difference = G1.RoundValue(difference);

                    //    dRow["difference"] = difference;

                    //    dRow["duplicates"] = "FINALE! FINALE! FINALE!";
                    //    dRow["nextDueDate"] = "12/31/2039";
                    //    trust85P = trust85P + difference;
                    //    if (trust85P <= 0D)
                    //        trust85P = 0D;
                    //    dRow["trust85P"] = trust85P;

                    //    trust100P = trust85P / 0.85D;
                    //    trust100P = G1.RoundValue(trust100P);
                    //    dRow["trust100P"] = trust100P;
                    //}
                    string payoff = processNewPayOff(dt, dRow, cnum, duplicate, dolp, docp, amtOfMonthlyPayment, numPayments, contractValue, trust85P, trust100P, balance );
                }
                dt.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
                MessageBox.Show(" X " + where + " ***ERROR*** " + ex.Message.ToString());
            }
            return rtn;
        }
        /***********************************************************************************************/
        private bool ParseCCPayment(DataTable importDt, int row, DataTable dt)
        {
            string found = "NO";

            //string origin = importDt.Rows[row]["Payment Origin"].ObjToString();
            //if (origin.ToUpper() != "ORIGINAL SIGNATURE")
            //    return false;

            if (G1.get_column_number(dt, "nextDueDate") < 0)
                dt.Columns.Add("nextDueDate");

            if (G1.get_column_number(dt, "duplicate") < 0)
                dt.Columns.Add("duplicate");

            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            bool duplicate = false;

            DateTime nextDueDate = DateTime.Now;
            DateTime lastDueDate = new DateTime(1910, 12, 31);

            string str = importDt.Rows[row]["Settlement Date/Time"].ObjToString();
            string[] Lines = str.Split(' ');
            str = Lines[0].Trim();
            DateTime docp = str.ObjToDateTime();
            string date = docp.ToString("MM/dd/yyyy");

            str = importDt.Rows[row]["Authorization Amount"].ObjToString();
            double payment = 0D;
            if (G1.validate_numeric(str))
                payment = str.ObjToDouble();

            string cnum = importDt.Rows[row]["Customer ID"].ObjToString();
            if (String.IsNullOrWhiteSpace(cnum))
                cnum = "EMPTY #";

            string status = importDt.Rows[row]["Transaction Status"].ObjToString();

            string paymentsFile = "payments";
            string payer = "";
            bool isLapsed = false;
            string newPayer = "";
            double insExpected = 0D;
            double originalCreditBalance = 0D;
            bool insurance = false;

            string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
            DataTable dxx = G1.get_db_data(cmd);

            if (dxx.Rows.Count <= 0)
            {
                paymentsFile = "ipayments";
                cnum = cnum.TrimStart('0');
                cmd = "Select * from `icustomers` i JOIN `icontracts` c ON i.`contractNumber` = c.`contractNumber` where `payer` = '" + cnum + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                {
                    payer = cnum;
                    cnum = ddx.Rows[0]["contractNumber"].ObjToString();
                    cnum = FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref insExpected, ref isLapsed);
                    if (isLapsed)
                        found = "L";
                    originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                    insurance = true;
                }
                else
                {
                    DataRow dR = dt.NewRow();
                    dR["code"] = "BAD";
                    dR["found"] = found;
                    //dR["locationcode"] = "CC";
                    dR["type"] = "CC";
                    dR["cnum"] = cnum;
                    dR["expected"] = 0D;
                    dR["payment"] = payment;
                    dR["date"] = date;
                    dt.Rows.Add(dR);
                    return false;
                }
            }
            else
                originalCreditBalance = dxx.Rows[0]["creditBalance"].ObjToDouble();

            if (status.Trim().ToUpper() != "SETTLED SUCCESSFULLY")
            {
                DataRow dR = dt.NewRow();
                dR["code"] = status;
                dR["found"] = found;
                //dR["locationcode"] = "CC";
                dR["type"] = "CC";
                dR["cnum"] = cnum;
                dR["expected"] = 0D;
                if (paymentsFile == "ipayments")
                    dR["expected"] = insExpected;
                dR["payment"] = payment;
                dR["date"] = date;
                dt.Rows.Add(dR);
                return false;
            }

            bool rtn = true;
            found = "";
            string code = "";
            string location = "CC";
            double expected = 0D;
            DataRow[] dRows = workDt.Select("contractNumber = '" + cnum + "'");
            if (dRows.Length <= 0)
            {
                found = "NO";
                cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                {
                    dRows = ddx.Select("contractNumber = '" + cnum + "'");
                    str = ddx.Rows[0]["lapsed"].ObjToString();
                    if (str.ToUpper() == "Y")
                    {
                        found = "L";
                    }
                }
            }

            DataRow dRow = dt.NewRow();
            dRow["code"] = code;
            dRow["locationcode"] = location;
            dRow["type"] = "CC";
            dRow["cnum"] = cnum;
            dRow["expected"] = expected;
            dRow["payment"] = payment;
            dRow["date"] = date;
            string agent = "";
            string fname = "";
            string lname = "";
            string firstPayDate = "";
            string contractDate = "";
            string issueDate = "";
            double balance = 0D;
            double oldbalance = 0D;
            double oldTotalInt = 0D;
            double oldOldBalance = 0D;
            double interest = 0D;
            double apr = 0D;
            double interestTotal = 0D;
            double amtOfMonthlyPayment = 0D;
            double contractValue = 0D;
            int numPayments = 0;
            double maxTrust85 = 0D;
            double totalTrust85 = 0D;
            string contractRecord = "";
            double principal = 0D;
            double totalPaid = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double creditBalance = 0D;
            double pExpected = 0D;
            double months = 0D;
            double unpaid_interest = 0D;
            double newTrust85 = 0D;
            double prevTrust85P = 0D;
            double actualMonthPaid = 0D;
            double dueDatePayment = 0D;
            double previousMonths = 0D;
            DateTime oldDueDate = DateTime.Now;
            DateTime dolp = DateTime.Now;
            DateTime currentDueDate = DateTime.Now;
            int days = 0;
            bool didIt = false;
            if (dRows.Length > 0)
            {
                agent = dRows[0]["agentCode"].ObjToString();
                fname = dRows[0]["firstName"].ObjToString();
                lname = dRows[0]["lastName"].ObjToString();
                firstPayDate = dRows[0]["firstPayDate"].ObjToString();
                contractDate = dRows[0]["contractDate"].ObjToString();
                issueDate = dRows[0]["issueDate8"].ObjToString();
                amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                interestTotal = dRows[0]["TotalInterest"].ObjToDouble();
                contractRecord = dRows[0]["record1"].ObjToString();

                oldbalance = dRows[0]["balanceDue"].ObjToDouble();
                oldTotalInt = dRows[0]["totalInterest"].ObjToDouble();

                if (FindPreviousPayments(dt, cnum, ref previousMonths))
                    originalCreditBalance = 0D;

                double oldCreditBalance = FindPrevCreditBalance(dt, cnum, 0D); // The CC
                actualMonthPaid = payment / amtOfMonthlyPayment;
                dueDatePayment = payment + oldCreditBalance + originalCreditBalance;
                double newActualMonthPaid = dueDatePayment / amtOfMonthlyPayment;
                if ((oldCreditBalance + originalCreditBalance + (payment % amtOfMonthlyPayment)) > amtOfMonthlyPayment)
                    actualMonthPaid += 1D;
                //if (!insurance)
                //    payment = dueDatePayment;

                oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();
                currentDueDate = oldDueDate;

                DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
                didIt = true;

                dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");
                dRow["lastDatePaid8"] = dRows[0]["lastDatePaid8"].ObjToString();
                dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();
                if (dolp.Year < 1900)
                    dolp = issueDate.ObjToDateTime();

                oldTotalInt = GetLastInterestPaid(cnum, payer, ref dolp, ref pExpected);

                apr = dRows[0]["APR"].ObjToDouble();
                dRow["dueDate"] = dRows[0]["dueDate8"].ObjToString();
                dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");

                //                dolp = new DateTime(2018, 4, 19);
                duplicate = FindDOLP(dt, cnum, ref dolp, ref oldOldBalance, ref lastDueDate, ref prevTrust85P ); // Don't use this oldbalance
                if (oldOldBalance > 0D)
                    oldbalance = oldOldBalance;
                if (dolp.Year < 1300)
                    dolp = new DateTime(2050, 12, 31);

                try
                {
                    if (dolp > docp)
                    {
                        cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' ORDER By `payDate8` DESC LIMIT 5";
                        DataTable dddx = G1.get_db_data(cmd);
                        if (dddx.Rows.Count > 0)
                        {
                            ManualPayment.CleanupWork(dddx);
                            for (int k = 0; k < dddx.Rows.Count; k++)
                            {
                                DateTime d = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                //                                if (d >= dolp)
                                if (d.Year > 1850)
                                {
                                    dolp = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                dRow["lastDatePaid8"] = dolp.ToString("MM/dd/yyyy");

                //                DateTime docp = date.ObjToDateTime();
                CalcPrincipalInterest(oldbalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest);
                balance = oldbalance - principal;
                balance = G1.RoundDown(balance);
                interestTotal = oldTotalInt + interest;
                interestTotal = G1.RoundDown(interestTotal);

                HandleUnpaidInterest(cnum, payment, ref interest, ref unpaid_interest, ref principal, ref balance);

                double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                double downPayment = dRows[0]["downPayment"].ObjToDouble();
                if (downPayment == 0D)
                    downPayment = DailyHistory.GetDownPaymentFromPayments(cnum);
                double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                numPayments = (int)financeDays;
                double amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                contractValue = DailyHistory.GetContractValue(dRows[0]);
                CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, apr, ref trust85P, ref trust100P);
                if (days <= 0)
                    dRow["duplicates"] = "POSSIBLE DUPLICATE";
            }
            else
            {
                cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                {
                    str = ddx.Rows[0]["lapsed"].ObjToString();
                    if (str.ToUpper() == "Y")
                    {
                        found = "L";
                    }
                    originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                }
            }
            if ( !didIt )
                DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate );

            if (lastDueDate.Year > 1910)
            {
                if (!DailyHistory.isInsurance(cnum))
                {
                    lastDueDate = FindMismatches.VerifyDueDate(cnum);
                    previousMonths = 0D;
                    if (FindPreviousPayments(dt, cnum, ref previousMonths))
                        months += previousMonths;
                }

                nextDueDate = lastDueDate.AddMonths(Convert.ToInt32(Math.Truncate(months)));
            }


            dRow["found"] = found;
            dRow["agent"] = agent;
            dRow["fname"] = fname;
            dRow["lname"] = lname;
            if (!String.IsNullOrWhiteSpace(payer))
                dRow["name"] = fname + " " + lname + " (" + payer + ")";
            else
                dRow["name"] = fname + " " + lname;
            if (duplicate)
                dRow["duplicate"] = "Y";
            dRow["type"] = "CC";
            dRow["firstPayDate"] = firstPayDate;
            dRow["contractDate"] = contractDate;
            dRow["balance"] = balance;
            dRow["oldbalance"] = oldbalance;
            dRow["oldTotalInt"] = oldTotalInt;
            dRow["interest"] = interest;
            dRow["principal"] = principal;
            dRow["days"] = days;
            dRow["apr"] = (apr / 100.0D);
            dRow["!contractRecord"] = contractRecord;
            dRow["amtOfMonthlyPayt"] = amtOfMonthlyPayment;
            dRow["interestTotal"] = interestTotal;
            dRow["totalPaid"] = totalPaid;
            dRow["issueDate"] = issueDate;
            dRow["trust85P"] = trust85P;
            dRow["trust100P"] = trust100P;
            dRow["creditBalance"] = creditBalance;
            dRow["line"] = "";
            dRow["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
            dRow["unpaid_interest"] = unpaid_interest;
            if (!DailyHistory.isInsurance(cnum) && balance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
            {
                //newTrust85 = 0D;
                //if (duplicate && dolp == docp)
                //{
                //    FindTrust85(dt, cnum, ref newTrust85);
                //}
                //bool isPaid = false;
                //if (!cnum.Contains("LI"))
                //    isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, trust85P + newTrust85 );
                //if (isPaid)
                //{
                //    double Trust85Paid = totalTrust85;
                //    double difference = maxTrust85 - totalTrust85;
                //    difference = G1.RoundValue(difference);

                //    dRow["difference"] = difference;

                //    dRow["duplicates"] = "FINALE! FINALE! FINALE!";
                //    dRow["nextDueDate"] = "12/31/2039";
                //    trust85P = trust85P + difference;
                //    if (trust85P <= 0D)
                //        trust85P = 0D;
                //    dRow["trust85P"] = trust85P;
                //    trust100P = trust85P / 0.85D;
                //    trust100P = G1.RoundValue(trust100P);
                //    dRow["trust100P"] = trust100P;
                //}
                string payoff = processNewPayOff(dt, dRow, cnum, duplicate, dolp, docp, amtOfMonthlyPayment, numPayments, contractValue, trust85P, trust100P, balance );
            }
            dt.Rows.Add(dRow);
            return rtn;
        }
        /***********************************************************************************************/
        private bool ParseOutPayment(DataTable dt, string line, DataTable bankDt = null, int bankRow = 0)
        {
            if (G1.get_column_number(dt, "nextDueDate") < 0)
                dt.Columns.Add("nextDueDate");

            if (G1.get_column_number(dt, "duplicate") < 0)
                dt.Columns.Add("duplicate");

            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            DateTime nextDueDate = DateTime.Now;
            DateTime lastDueDate = new DateTime(1910, 12, 31);
            double creditBalance = 0D;
            bool rtn = true;
            string found = "NO";
            string payer = "";
            double insExpected = 0D;
            bool duplicate = false;
            double originalCreditBalance = 0D;
            bool insurance = false;
            double retained = 0D;

            double ccFee = 0D;
            double ap = 0D;

            string cnum = "";

            try
            {
                originalCreditBalance = 0D;
                if (line.Length < 36)
                    line = line.PadRight(36);
                string code = line.Substring(0, 2);
                //if (code != "01")
                //    return false;
                string location = line.Substring(2, 2);
                cnum = line.Substring(4, 10);
                string str = line.Substring(14, 7);
                //if (!G1.validate_numeric(str))
                //    rtn = false;
                double expected = str.ObjToDouble() / 100.0D;
                expected = G1.RoundValue(expected);
                str = line.Substring(21, 7);
                //if (!G1.validate_numeric(str))
                //    rtn = false;
                double payment = str.ObjToDouble() / 100.0D;
                payment = G1.RoundValue(payment);

                if (bankDt != null)
                    ccFee = bankDt.Rows[bankRow]["ccFee"].ObjToDouble();

                ap = payment - ccFee;

                string date = line.Substring(28);
                DateTime rdate = date.ObjToDateTime();
                DateTime docp = date.ObjToDateTime();
                cnum = cnum.TrimStart('0');
                cnum = cnum.TrimStart('o');
                cnum = cnum.TrimStart('O');
                //if (code == "01")
                //    cnum = cnum.TrimEnd('0');
                string testContract = txtContract.Text;
                if (!String.IsNullOrWhiteSpace(testContract))
                {
                    if (cnum.ToUpper() != testContract.ToUpper())
                        return false;
                }
                //if (cnum == "WM13055UI")
                //{
                //}
                if (cnum == "CC-965")
                {
                }

                string paymentsFile = "payments";
                if (code == "99")
                    expected = 0.0D;
                string cmd = "";
                found = "";
                DataRow[] dRows = null;
                if (code == "02")
                {
                    insurance = true;
                    string newPayer = "";
                    cnum = cnum.TrimStart('0');
                    payer = cnum;
                    paymentsFile = "ipayments";
                    bool isLapsed = false;
                    if (payer == "UC-1084")
                    {
                    }
                    cnum = FindPayerContract(payer, ap.ObjToString(), ref newPayer, ref insExpected, ref isLapsed);
                    if (expected == 0D && insExpected > 0D)
                        expected = insExpected;
                    if (isLapsed)
                        found = "L";
                    //cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    //DataTable ddx = G1.get_db_data(cmd);
                    //if (ddx.Rows.Count > 0)
                    if (!String.IsNullOrWhiteSpace(cnum))
                    {
                        if (!String.IsNullOrWhiteSpace(newPayer))
                            payer = newPayer;
                        found = "";
                        //                        cnum = ddx.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                        }
                        if (isLapsed)
                            found = "L";
                    }
                    else
                    {
                        found = "NO";
                        cnum = payer;
                    }
                }
                else if (code == "03")
                {
                }
                else
                {
                    dRows = workDt.Select("contractNumber = '" + cnum + "'");
                    if (dRows.Length <= 0)
                    {
                        found = "NO";
                        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                        }
                        else
                        {
                            cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                            ddx = G1.get_db_data(cmd);
                            if (ddx.Rows.Count > 0)
                            {
                                found = "";
                                originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                                dRows = ddx.Select("contractNumber = '" + cnum + "'");
                                str = ddx.Rows[0]["lapsed"].ObjToString();
                                if (str.ToUpper() == "Y")
                                {
                                    found = "L";
                                }
                            }
                        }
                    }
                    else
                    {
                        cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                        found = "";
                    }
                }

                DataRow dRow = dt.NewRow();
                dRow["code"] = code;
                dRow["locationcode"] = location;
                dRow["type"] = "LKBX";
                dRow["fill1"] = "";
                dRow["cnum"] = cnum;
                dRow["expected"] = expected;
                dRow["payment"] = payment;
                dRow["date"] = date;
                dRow["oldCreditBalance"] = originalCreditBalance;
                string agent = "";
                string fname = "";
                string lname = "";
                string firstPayDate = "";
                string contractDate = "";
                string issueDate = "";
                double balance = 0D;
                double oldbalance = 0D;
                double oldTotalInt = 0D;
                double oldOldBalance = 0D;
                double interest = 0D;
                double apr = 0D;
                double interestTotal = 0D;
                double amtOfMonthlyPayment = 0D;
                double dueDatePayment = ap;
                string contractRecord = "";
                double principal = 0D;
                double maxTrust85 = 0D;
                double totalTrust85 = 0D;
                double contractValue = 0D;
                double amtOfMonthlyPayt = 0D;
                double totalPaid = 0D;
                double trust85P = 0D;
                double trust100P = 0D;
                double pExpected = 0D;
                double months = 0D;
                double unpaid_interest = 0D;
                double actualMonthPaid = 0D;
                double previousMonths = 0D;
                DateTime dolp = DateTime.Now;
                double newTrust85 = 0D;
                int numPayments = 0;
                double prevTrust85P = 0D;
                DateTime currentDueDate = DateTime.Now;
                DateTime oldDueDate = DateTime.Now;
                bool didIt = false;
                int days = 0;
                if (code == "01" || code == "02")
                {
                    if (dRows == null)
                    {
                        dRow["line"] = line;
                        dRow["found"] = "NO";
                        if (!String.IsNullOrWhiteSpace(payer))
                            dRow["name"] = "BAD PAYER NUMBER (" + payer + ")";
                        else
                            dRow["name"] = "BAD CONTRACT NUMBER (" + cnum + ")";
                        dt.Rows.Add(dRow);
                        return rtn;
                    }
                    if (dRows.Length > 0)
                    {
                        agent = dRows[0]["agentCode"].ObjToString();
                        fname = dRows[0]["firstName"].ObjToString();
                        lname = dRows[0]["lastName"].ObjToString();
                        firstPayDate = dRows[0]["firstPayDate"].ObjToString();
                        contractDate = dRows[0]["contractDate"].ObjToString();
                        issueDate = dRows[0]["issueDate8"].ObjToString();
                        amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                        if (DailyHistory.isInsurance(cnum))
                        {
                            oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();
                            if (oldDueDate.Year < 2019)
                                oldDueDate = docp;
                            trust85P = 0D;
                            trust100P = 0D;
                            //amtOfMonthlyPayment = Policies.CalcMonthlyPremium(payer, oldDueDate);
                            amtOfMonthlyPayment = Policies.CalcMonthlyPremium(payer, docp);
                        }
                        if (expected <= 0D && amtOfMonthlyPayment > 0D)
                        {
                            expected = amtOfMonthlyPayment;
                            dRow["expected"] = expected;
                        }
                        interestTotal = dRows[0]["TotalInterest"].ObjToDouble();
                        contractRecord = dRows[0]["record1"].ObjToString();

                        oldbalance = dRows[0]["balanceDue"].ObjToDouble();
                        oldTotalInt = dRows[0]["totalInterest"].ObjToDouble();

                        if (FindPreviousPayments(dt, cnum, ref previousMonths))
                            originalCreditBalance = 0D;

                        double oldCreditBalance = FindPrevCreditBalance(dt, cnum, 0D); // PARSE OUT PAYMENT
                        actualMonthPaid = payment / amtOfMonthlyPayment;
                        dueDatePayment = payment + oldCreditBalance + originalCreditBalance;
                        double newActualMonthPaid = dueDatePayment / amtOfMonthlyPayment;
                        double ramma = payment % amtOfMonthlyPayment;
                        //if (payment < amtOfMonthlyPayment)
                        //    ramma = 0D;
                        double ramma2 = oldCreditBalance + originalCreditBalance + ramma;
                        if ((oldCreditBalance + originalCreditBalance + ramma) > amtOfMonthlyPayment)
                            actualMonthPaid += 1D;
                        //if (!insurance)
                        //    payment = dueDatePayment;

                        oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();


                        //double oldCreditBalance = FindPrevCreditBalance(dt, cnum, originalCreditBalance);
                        //dueDatePayment = ap + oldCreditBalance;
                        //oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();
                        //currentDueDate = oldDueDate;
                        //if (currentDueDate.Year < 100)
                        //{
                        //}
                        DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, dueDatePayment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
                        didIt = true;

                        originalCreditBalance = 0D;

                        //dRow["monthsPaid"] = Math.Truncate(months);
                        //actualMonthPaid = Math.Truncate(actualMonthPaid);
                        dRow["monthsPaid"] = actualMonthPaid;
                        actualMonthPaid += 0.00000000001D;
                        months = actualMonthPaid;


                        //if (originalCreditBalance > 0D)
                        //{
                        //    months = Math.Truncate(months);
                        //    if (months > 1D)
                        //        months = months - 1D;
                        //}

                        dRow["lastDatePaid8"] = dRows[0]["lastDatePaid8"].ObjToString();
                        dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();

                        oldTotalInt = GetLastInterestPaid(cnum, payer, ref dolp, ref pExpected);

                        apr = dRows[0]["APR"].ObjToDouble();
                        dRow["dueDate"] = dRows[0]["dueDate8"].ObjToString();
                        dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");
                        dRow["dueDate"] = oldDueDate.ToString("MM/dd/yyyy");
                        //dRow["monthsPaid"] = Math.Truncate(months); // Ramma Zamma
                        dRow["monthsPaid"] = months;

                        //                dolp = new DateTime(2018, 4, 19);
                        lastDueDate = oldDueDate;
                        duplicate = FindDOLP(dt, cnum, ref dolp, ref oldOldBalance, ref lastDueDate, ref prevTrust85P); // Don't use this oldbalance
                        if (oldOldBalance > 0D)
                            oldbalance = oldOldBalance;
                        if (dolp.Year < 1975)
                            dolp = issueDate.ObjToDateTime();
                        if (dolp.Year < 1300)
                            dolp = new DateTime(2050, 12, 31);

                        try
                        {
                            if (dolp > docp)
                            {
                                cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' ORDER By `payDate8` DESC LIMIT 5";
                                DataTable dddx = G1.get_db_data(cmd);
                                if (dddx.Rows.Count > 0)
                                {
                                    ManualPayment.CleanupWork(dddx);
                                    for (int k = 0; k < dddx.Rows.Count; k++)
                                    {
                                        DateTime d = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                        //                                if (d >= dolp)
                                        if (d.Year > 1850)
                                        {
                                            dolp = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        dRow["lastDatePaid8"] = dolp.ToString("MM/dd/yyyy");

                        CalcPrincipalInterest(oldbalance, dolp, docp, ap, apr, ref principal, ref interest, ref days, ref unpaid_interest);

                        if (duplicate && dolp == docp)
                            oldbalance = oldOldBalance;

                        balance = oldbalance - principal;
                        balance = G1.RoundDown(balance);
                        interestTotal = oldTotalInt + interest;
                        interestTotal = G1.RoundDown(interestTotal);
                        double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                        double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                        double downPayment = dRows[0]["downPayment"].ObjToDouble();
                        if (downPayment == 0D)
                            downPayment = DailyHistory.GetDownPaymentFromPayments(cnum);
                        double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                        amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                        contractValue = DailyHistory.GetContractValue(dRows[0]);

                        HandleUnpaidInterest(cnum, ap, ref interest, ref unpaid_interest, ref principal, ref balance);

                        cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' order by `paydate8` DESC, `tmstamp` DESC;";
                        DataTable dx = G1.get_db_data(cmd);
                        double financeBalance = DailyHistory.GetFinanceValue(dRows[0]);
                        numPayments = (int)financeDays;
                        //DailyHistory.CalculateNewStuff(dx, apr, numPayments, financeBalance, issueDate.ObjToDateTime());
                        //if (dx.Rows.Count > 0)
                        //{
                        //    creditBalance = dx.Rows[0]["runningCB"].ObjToDouble();
                        //}

                        CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, ap, principal, apr, ref trust85P, ref trust100P);
                        if (days <= 0)
                        {
                            dRow["duplicates"] = "POSSIBLE DUPLICATE"; // Needs testing before releasing
                                                                       //DataRow[] dR = dt.Select("cnum='" + cnum + "'");
                                                                       //if (dR.Length >= 1)
                                                                       //{
                                                                       //    DateTime ddlp = dR[0]["date"].ObjToDateTime();
                                                                       //    dRow["duplicates"] = "";
                                                                       //}
                        }
                    }
                    else
                    {
                        cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                        }
                    }
                }
                else if ( code == "03")
                {
                    cmd = "Select * from `fcustomers` where `serviceId` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        fname = ddx.Rows[0]["firstName"].ObjToString();
                        lname = ddx.Rows[0]["lastName"].ObjToString();
                    }
                }

                //if (lastDueDate.Year > 1910)
                //    docp = lastDueDate;

                if (!didIt)
                {
                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, ap, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);

                    if (originalCreditBalance > 0D)
                    {
                        months = Math.Truncate(months);
                        if (months > 1D)
                            months = months - 1D;
                    }
                }
                if (lastDueDate.Year > 1910)
                {
                    if (!DailyHistory.isInsurance(cnum))
                    {
                        lastDueDate = FindMismatches.VerifyDueDate(cnum);
                        previousMonths = 0D;
                        if (FindPreviousPayments(dt, cnum, ref previousMonths))
                            months += previousMonths;
                    }

                    nextDueDate = lastDueDate.AddMonths(Convert.ToInt32 ( Math.Truncate(months)));
                }

                dRow["found"] = found;
                dRow["agent"] = agent;
                dRow["fname"] = fname;
                dRow["lname"] = lname;
                if (!String.IsNullOrWhiteSpace(payer))
                    dRow["name"] = fname + " " + lname + " (" + payer + ")";
                else
                    dRow["name"] = fname + " " + lname;
                if ( DailyHistory.isInsurance ( cnum))
                {
                    trust85P = 0D;
                    trust100P = 0D;
                    balance = Policies.CalcMonthlyPremium(payer, docp );
                    principal = dRow["payment"].ObjToDouble();
                    interest = 0D;
                }
                if (duplicate)
                    dRow["duplicate"] = "Y";
                bool problem = CheckDeceasedRemoved(cnum);
                dRow["firstPayDate"] = firstPayDate;
                dRow["contractDate"] = contractDate;
                dRow["balance"] = balance;
                dRow["oldbalance"] = oldbalance;
                dRow["oldTotalInt"] = oldTotalInt;
                dRow["interest"] = interest;
                dRow["principal"] = principal;
                //dRow["ID"] = G1.ReformatMoney(ccFee);
                dRow["days"] = days;
                dRow["apr"] = (apr / 100.0D);
                dRow["!contractRecord"] = contractRecord;
                dRow["amtOfMonthlyPayt"] = amtOfMonthlyPayment;
                dRow["interestTotal"] = interestTotal;
                dRow["totalPaid"] = totalPaid;
                dRow["issueDate"] = issueDate;
                dRow["trust85P"] = trust85P;
                dRow["trust100P"] = trust100P;
                //if ((payment - trust100P) > 0D)
                //    dRow["retained"] = payment - trust100P;
                dRow["creditBalance"] = creditBalance;
                dRow["payer"] = payer;
                dRow["line"] = line;
                dRow["unpaid_interest"] = unpaid_interest;
                //dRow["retained"] = 555D;
                dRow["retained"] = 0D;
                if (principal <= 0D)
                    nextDueDate = oldDueDate;
                dRow["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                if (!DailyHistory.isInsurance(cnum) && balance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff )
                {
                    //newTrust85 = 0D;
                    //if (duplicate && dolp == docp)
                    //{
                    //    FindTrust85(dt, cnum, ref newTrust85);
                    //}
                    // bool isPaid = false;
                    //if (!cnum.Contains("LI"))
                    //    isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, trust85P + newTrust85 );
                    //double Trust85Paid = totalTrust85;
                    //double difference = maxTrust85 - totalTrust85;
                    //difference = G1.RoundValue(difference);

                    //string payOffMethod = LoginForm.allowPayOffMethod;
                    //if ( LoginForm.useNewTCACalculation )
                    //{
                    //    if (String.IsNullOrWhiteSpace(payOffMethod))
                    //    {
                    //        difference = 0D;
                    //        isPaid = false;
                    //    }
                    //    if (isPaid && payOffMethod.ToUpper() == "DEBIT")
                    //    {
                    //        if (difference > 0D)
                    //        {
                    //            difference = 0D;
                    //            isPaid = false;
                    //        }
                    //    }
                    //    else if (isPaid && payOffMethod.ToUpper() == "CREDIT")
                    //    {
                    //        if (difference < 0D)
                    //        {
                    //            difference = 0D;
                    //            //isPaid = false;
                    //        }
                    //    }
                    //    if (isPaid && difference != 0D)
                    //    {
                    //        //double Trust85Paid = totalTrust85;
                    //        //double difference = maxTrust85 - totalTrust85;
                    //        difference = G1.RoundValue(difference);

                    //        dRow["difference"] = difference;

                    //        trust85P = difference;
                    //        trust100P = trust85P / 0.85D;

                    //        dRow["duplicates"] = "FINALE! FINALE! FINALE! w/ T85 : " + G1.ReformatMoney(trust85P) + " / " + G1.ReformatMoney(trust100P);
                    //        dRow["nextDueDate"] = "12/31/2039";
                    //        //trust85P = trust85P + difference;
                    //        //if (trust85P <= 0D)
                    //        //    trust85P = 0D;
                    //        //dRow["trust85P"] = trust85P;
                    //        //trust100P = trust85P / 0.85D;
                    //        //trust100P = G1.RoundValue(trust100P);
                    //        //dRow["trust100P"] = trust100P;
                    //    }
                    //}
                    //else
                    //{
                    //    if (isPaid )
                    //    {
                    //        difference = G1.RoundValue(difference);

                    //        dRow["difference"] = difference;

                    //        //trust85P = difference;
                    //        //trust100P = trust85P / 0.85D;

                    //        dRow["duplicates"] = "FINALE! FINALE! FINALE!";
                    //        dRow["nextDueDate"] = "12/31/2039";
                    //        trust85P = trust85P + difference;
                    //        if (trust85P <= 0D)
                    //            trust85P = 0D;
                    //        trust85P = G1.RoundValue(trust85P);
                    //        dRow["trust85P"] = trust85P;
                    //        trust100P = trust85P / 0.85D;
                    //        trust100P = G1.RoundValue(trust100P);
                    //        dRow["trust100P"] = trust100P;
                    //    }
                    //}
                    string payoff = processNewPayOff(dt, dRow, cnum, duplicate, dolp, docp, amtOfMonthlyPayment, numPayments, contractValue, trust85P, trust100P, balance );
                }
                if (balance <= 0D)
                {
                    if (paymentsFile != "ipayments")
                        dRow["nextDueDate"] = "12/31/2039";
                }
                dt.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
                int thisRow = dt.Rows.Count;
                MessageBox.Show("***ERROR*** (" + cnum + " " + thisRow.ToString() + ") " + line + " " + ex.Message.ToString());
            }
            return rtn;
        }
        /***********************************************************************************************/
        private string processNewPayOff ( DataTable dt, DataRow dRow, string cnum, bool duplicate, DateTime dolp, DateTime docp, double amtOfMonthlyPayment, int numPayments, double contractValue, double trust85P, double trust100P, double balance  )
        {
            string result = "";
            double newTrust85 = 0D;
            double maxTrust85 = 0D;
            double totalTrust85 = 0D;
            if (duplicate && dolp == docp)
            {
                FindTrust85(dt, cnum, ref newTrust85);
            }
            bool isPaid = false;
            bool trustThreshold = false;
            bool balanceThreshold = false;

            //if (!cnum.Contains("LI"))
            isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, ref trustThreshold, ref balanceThreshold, trust85P + newTrust85);

            double Trust85Paid = totalTrust85;
            double difference = maxTrust85 - totalTrust85;
            difference = G1.RoundValue(difference);

            double minimumBalance = LoginForm.minimumForceBalance;

            if (balance <= LoginForm.minimumForceBalance)
                balanceThreshold = true;
            else
                isPaid = false;

            bool overPaid = false;
            if (difference < 0D)
                overPaid = true;

            string payOffMethod = LoginForm.allowPayOffMethod;
            if (LoginForm.useNewTCACalculation)
            {
                if (String.IsNullOrWhiteSpace(payOffMethod))
                {
                    difference = 0D;
                    isPaid = false;
                }
                if (isPaid && payOffMethod.ToUpper() == "DEBIT")
                {
                    if (difference > 0D)
                    {
                        difference = 0D;
                        isPaid = false;
                    }
                }
                else if (isPaid && payOffMethod.ToUpper() == "CREDIT")
                {
                    //if (difference < 0D)
                    //{
                    //    difference = 0D;
                    //    //isPaid = false;
                    //}
                }
                if (isPaid && difference > 0D)
                {
                    //double Trust85Paid = totalTrust85;
                    //double difference = maxTrust85 - totalTrust85;
                    difference = G1.RoundValue(difference);

                    dRow["difference"] = difference;

                    trust85P = difference;
                    trust100P = trust85P / 0.85D;

                    dRow["duplicates"] = "FINALE! FINALE! FINALE! TCA T85/T100 : " + G1.ReformatMoney(trust85P) + " / " + G1.ReformatMoney(trust100P);
                    dRow["nextDueDate"] = "12/31/2039";
                    //trust85P = trust85P + difference;
                    //if (trust85P <= 0D)
                    //    trust85P = 0D;
                    //dRow["trust85P"] = trust85P;
                    //trust100P = trust85P / 0.85D;
                    //trust100P = G1.RoundValue(trust100P);
                    //dRow["trust100P"] = trust100P;
                }
                else if ( isPaid )
                {
                    dRow["nextDueDate"] = "12/31/2039";
                }
            }
            else
            {
                if (isPaid)
                {
                    difference = G1.RoundValue(difference);

                    dRow["difference"] = difference;

                    //trust85P = difference;
                    //trust100P = trust85P / 0.85D;

                    dRow["duplicates"] = "FINALE! FINALE! FINALE!";
                    dRow["nextDueDate"] = "12/31/2039";
                    trust85P = trust85P + difference;
                    if (trust85P <= 0D)
                        trust85P = 0D;
                    trust85P = G1.RoundValue(trust85P);
                    dRow["trust85P"] = trust85P;
                    trust100P = trust85P / 0.85D;
                    trust100P = G1.RoundValue(trust100P);
                    dRow["trust100P"] = trust100P;
                }
            }
            if (overPaid)
            {
                dRow["nextDueDate"] = "12/31/2039";
                if (String.IsNullOrWhiteSpace(dRow["duplicates"].ObjToString()))
                    dRow["duplicates"] = "OVERPAID";
            }
            result = dRow["duplicates"].ObjToString();
            return result;
        }
        /***********************************************************************************************/
        private bool ParseTheFirstPayment(DataTable dt, string line, bool newVersion )
        {
            if (G1.get_column_number(dt, "nextDueDate") < 0)
                dt.Columns.Add("nextDueDate");

            if (G1.get_column_number(dt, "duplicate") < 0)
                dt.Columns.Add("duplicate");

            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            DateTime nextDueDate = DateTime.Now;
            DateTime lastDueDate = new DateTime(1910, 12, 31);
            double creditBalance = 0D;
            bool rtn = true;
            string found = "NO";
            string payer = "";
            double insExpected = 0D;
            bool duplicate = false;
            double originalCreditBalance = 0D;
            bool insurance = false;
            double retained = 0D;

            DateTime effectiveDate = this.dateTimePicker1.Value;

            string cnum = "";

            // line = 0206FL-L3539410000330000376220190116;
            // line = 010500B17026UI0127.94001279420250301 Length = 36

                /* 1) (2)(3)(4)(5)(6)
                    01 05 00B17026UI 0127.94 0012794 20250301

                    (1)  01 - Trust Or Ins Code              2 digit field(We provide on our coupon)

                    (2)  05 - Location Code                  2 digit field(We provide on our coupon)

                    (3)  00B17026UI - Customer Account       10 digit field, leading 0's vary depending on actual account #  (We provide on our coupon) 

                    (4)  0127.94 - Coupon Payment Amount     7 digit fixed "0000.00" format, leading 0's vary on actual coupon payment amount  (We provide on our coupon)

                    (5)  0012794 - Actual Payment Amount     7 digit field, leading 0's vary depending on actual amount, last 2 digits are always cents (You provide)

                    (6)  20250301 - Date of payment          8 digit YYYYMMDD format(You provide)
                */


            try
            {
                originalCreditBalance = 0D;
                if (line.Length < 36)
                    line = line.PadRight(36);
                string code = line.Substring(0, 2);
                //if (code != "01")
                //    return false;
                int length = line.Length;
                int idx = 0;
                string location = line.Substring(2, 2);
                cnum = line.Substring(4, 10);
                idx = length - 8 - 7 - 7;
                string str = line.Substring(idx, 7);
                double expected = str.ObjToDouble() / 100.0D;
                if (newVersion)
                    expected = str.ObjToDouble();
                //double expected = str.ObjToDouble();
                expected = G1.RoundValue(expected);
                idx = length - 8 - 7;
                str = line.Substring(idx, 7);
                //if (!G1.validate_numeric(str))
                //    rtn = false;
                double payment = str.ObjToDouble() / 100.0D;
                payment = G1.RoundValue(payment);
                if ( payment == 11.65D )
                {
                }
                idx = length - 8;
                string date = line.Substring(idx);
                date = effectiveDate.ToString("MM/dd/yyyy");
                DateTime rdate = date.ObjToDateTime();
                DateTime docp = date.ObjToDateTime();
                cnum = cnum.TrimStart('0');
                cnum = cnum.TrimStart('o');
                cnum = cnum.TrimStart('O');
                //if (code == "01")
                //    cnum = cnum.TrimEnd('0');
                string testContract = txtContract.Text;
                if (!String.IsNullOrWhiteSpace(testContract))
                {
                    if (cnum.ToUpper() != testContract.ToUpper())
                        return false;
                }
                //if (cnum == "WM13055UI")
                //{
                //}
                if (cnum == "CC-965")
                {
                }

                string paymentsFile = "payments";
                if (code == "99")
                    expected = 0.0D;
                string cmd = "";
                found = "";
                DataRow[] dRows = null;
                if (code == "02")
                {
                    insurance = true;
                    string newPayer = "";
                    cnum = cnum.TrimStart('0');
                    payer = cnum;
                    paymentsFile = "ipayments";
                    bool isLapsed = false;
                    if (payer == "UC-1084")
                    {
                    }
                    cnum = FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref insExpected, ref isLapsed);
                    if (expected == 0D && insExpected > 0D)
                        expected = insExpected;
                    if (isLapsed)
                        found = "L";
                    //cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    //DataTable ddx = G1.get_db_data(cmd);
                    //if (ddx.Rows.Count > 0)
                    cnum = cnum.Trim();
                    if (!String.IsNullOrWhiteSpace(cnum))
                    {
                        if (!String.IsNullOrWhiteSpace(newPayer))
                            payer = newPayer;
                        found = "";
                        //                        cnum = ddx.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                        }
                        if (isLapsed)
                            found = "L";
                    }
                    else
                    {
                        found = "NO";
                        cnum = payer;
                    }
                }
                else
                {
                    dRows = workDt.Select("contractNumber = '" + cnum + "'");
                    if (dRows.Length <= 0)
                    {
                        found = "NO";
                        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                        }
                    }
                    else
                    {
                        cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                    }
                }

                DataRow dRow = dt.NewRow();
                dRow["code"] = code;
                dRow["locationcode"] = location;
                dRow["type"] = "TFBX";
                dRow["fill1"] = "TFBX";
                dRow["cnum"] = cnum;
                dRow["expected"] = expected;
                dRow["payment"] = payment;
                dRow["date"] = date;
                dRow["oldCreditBalance"] = originalCreditBalance;
                string agent = "";
                string fname = "";
                string lname = "";
                string firstPayDate = "";
                string contractDate = "";
                string issueDate = "";
                double balance = 0D;
                double oldbalance = 0D;
                double oldTotalInt = 0D;
                double oldOldBalance = 0D;
                double interest = 0D;
                double apr = 0D;
                double interestTotal = 0D;
                double previousMonths = 0D;

                double maxTrust85 = 0D;
                double totalTrust85 = 0D;
                double contractValue = 0D;
                int numPayments = 0;

                double amtOfMonthlyPayment = 0D;
                double dueDatePayment = payment;
                string contractRecord = "";
                double principal = 0D;
                double totalPaid = 0D;
                double trust85P = 0D;
                double trust100P = 0D;
                double pExpected = 0D;
                double months = 0D;
                double unpaid_interest = 0D;
                double newTrust85 = 0D;
                double newBalance = 0D;
                double prevTrust85P = 0D;
                double actualMonthPaid = 0D;
                DateTime dolp = DateTime.Now;
                DateTime currentDueDate = DateTime.Now;
                DateTime oldDueDate = DateTime.Now;
                DateTime newDueDate = DateTime.Now;
                bool didIt = false;
                int days = 0;
                if (dRows == null)
                {
                    dRow["line"] = line;
                    dRow["found"] = "NO";
                    if (!String.IsNullOrWhiteSpace(payer))
                        dRow["name"] = "BAD PAYER NUMBER (" + payer + ")";
                    else
                        dRow["name"] = "BAD CONTRACT NUMBER (" + cnum + ")";
                    dt.Rows.Add(dRow);
                    return rtn;
                }
                if (dRows.Length > 0)
                {
                    agent = dRows[0]["agentCode"].ObjToString();
                    fname = dRows[0]["firstName"].ObjToString();
                    lname = dRows[0]["lastName"].ObjToString();
                    firstPayDate = dRows[0]["firstPayDate"].ObjToString();
                    contractDate = dRows[0]["contractDate"].ObjToString();
                    issueDate = dRows[0]["issueDate8"].ObjToString();
                    amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    if (DailyHistory.isInsurance(cnum))
                    {
                        oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();
                        if (oldDueDate.Year < 2019)
                            oldDueDate = docp;
                        trust85P = 0D;
                        trust100P = 0D;
                        //amtOfMonthlyPayment = Policies.CalcMonthlyPremium(payer, oldDueDate);
                        amtOfMonthlyPayment = Policies.CalcMonthlyPremium(payer, docp);
                        if ( amtOfMonthlyPayment == 0D )
                            amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    }
                    if (expected <= 0D && amtOfMonthlyPayment > 0D)
                    {
                        expected = amtOfMonthlyPayment;
                        dRow["expected"] = expected;
                    }
                    interestTotal = dRows[0]["TotalInterest"].ObjToDouble();
                    contractRecord = dRows[0]["record1"].ObjToString();

                    oldbalance = dRows[0]["balanceDue"].ObjToDouble();
                    oldTotalInt = dRows[0]["totalInterest"].ObjToDouble();

                    if (FindPreviousPayments(dt, cnum, ref previousMonths))
                        originalCreditBalance = 0D;

                    double oldCreditBalance = FindPrevCreditBalance(dt, cnum, 0D); // The First
                    actualMonthPaid = payment / amtOfMonthlyPayment;
                    dueDatePayment = payment + oldCreditBalance + originalCreditBalance;
                    double newActualMonthPaid = dueDatePayment / amtOfMonthlyPayment;
                    if ((oldCreditBalance + originalCreditBalance + (payment % amtOfMonthlyPayment)) > amtOfMonthlyPayment)
                        actualMonthPaid += 1D;
                    //if (!insurance)
                    //    payment = dueDatePayment;

                    oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();



                    //double oldCreditBalance = FindPrevCreditBalance(dt, cnum, originalCreditBalance);

                    //if (FindPreviousPayments(dt, cnum, ref previousMonths))
                    //    originalCreditBalance = 0D;

                    //double oldCreditBalance = FindPrevCreditBalance(dt, cnum, 0D);
                    //dueDatePayment = payment + oldCreditBalance + originalCreditBalance;
                    //oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();
                    currentDueDate = oldDueDate;

                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, dueDatePayment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref newBalance, ref currentDueDate);
                    oldbalance = newBalance;
                    didIt = true;

                    originalCreditBalance = 0D;

                    //dRow["monthsPaid"] = Math.Truncate(months);
                    //actualMonthPaid = Math.Truncate(actualMonthPaid);
                    dRow["monthsPaid"] = actualMonthPaid;
                    actualMonthPaid += 0.00000000001D;
                    months = actualMonthPaid;


                    //if (originalCreditBalance > 0D)
                    //{
                    //    months = Math.Truncate(months);
                    //    if (months > 1D)
                    //        months = months - 1D;
                    //}


                    dRow["lastDatePaid8"] = dRows[0]["lastDatePaid8"].ObjToString();
                    dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();

                    oldTotalInt = GetLastInterestPaid(cnum, payer, ref dolp, ref pExpected);

                    apr = dRows[0]["APR"].ObjToDouble();
                    //newDueDate = FindMismatches.VerifyDueDate(cnum);
                    dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");
                    dRow["dueDate"] = oldDueDate.ToString("MM/dd/yyyy");

                    dRow["monthsPaid"] = Math.Truncate(months);

                    //                dolp = new DateTime(2018, 4, 19);
                    lastDueDate = oldDueDate;
                    duplicate = FindDOLP(dt, cnum, ref dolp, ref oldOldBalance, ref lastDueDate, ref prevTrust85P ); // Don't use this oldbalance
                    if (oldOldBalance > 0D)
                        oldbalance = oldOldBalance;
                    if (dolp.Year < 1975)
                        dolp = issueDate.ObjToDateTime();
                    if (dolp.Year < 1300)
                        dolp = new DateTime(2050, 12, 31);

                    try
                    {
                        if (dolp > docp)
                        {
                            cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' ORDER By `payDate8` DESC LIMIT 5";
                            DataTable dddx = G1.get_db_data(cmd);
                            if (dddx.Rows.Count > 0)
                            {
                                ManualPayment.CleanupWork(dddx);
                                for (int k = 0; k < dddx.Rows.Count; k++)
                                {
                                    DateTime d = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                    //                                if (d >= dolp)
                                    if (d.Year > 1850)
                                    {
                                        dolp = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    dRow["lastDatePaid8"] = dolp.ToString("MM/dd/yyyy");

                    CalcPrincipalInterest(oldbalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest);

                    balance = oldbalance - principal;
                    if (balance < 0D)
                        balance = G1.RoundValue(balance);
                    else
                        balance = G1.RoundDown(balance);
                    interestTotal = oldTotalInt + interest;
                    interestTotal = G1.RoundDown(interestTotal);
                    double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                    double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                    double downPayment = dRows[0]["downPayment"].ObjToDouble();
                    if (downPayment == 0D)
                        downPayment = DailyHistory.GetDownPaymentFromPayments(cnum);
                    double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                    double amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    contractValue = DailyHistory.GetContractValue(dRows[0]);

                    HandleUnpaidInterest(cnum, payment, ref interest, ref unpaid_interest, ref principal, ref balance);

                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    DataTable dx = G1.get_db_data(cmd);
                    double financeBalance = DailyHistory.GetFinanceValue(dRows[0]);
                    numPayments = (int)financeDays;
                    //DailyHistory.CalculateNewStuff(dx, apr, numPayments, financeBalance, issueDate.ObjToDateTime());
                    //if (dx.Rows.Count > 0)
                    //{
                    //    creditBalance = dx.Rows[0]["runningCB"].ObjToDouble();
                    //}

                    CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, apr, ref trust85P, ref trust100P);
                    if (days <= 0)
                    {
                        dRow["duplicates"] = "POSSIBLE DUPLICATE"; // Needs testing before releasing
                        //DataRow[] dR = dt.Select("cnum='" + cnum + "'");
                        //if (dR.Length >= 1)
                        //{
                        //    DateTime ddlp = dR[0]["date"].ObjToDateTime();
                        //    dRow["duplicates"] = "";
                        //}
                    }
                }
                else
                {
                    cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        str = ddx.Rows[0]["lapsed"].ObjToString();
                        if (str.ToUpper() == "Y")
                        {
                            found = "L";
                        }
                        originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                        oldDueDate = ddx.Rows[0]["dueDate8"].ObjToDateTime();
                        amtOfMonthlyPayment = ddx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();

                    }
                }

                //if (lastDueDate.Year > 1910)
                //    docp = lastDueDate;

                if (!didIt)
                {
                    if (FindPreviousPayments(dt, cnum, ref previousMonths))
                        originalCreditBalance = 0D;

                    double oldCreditBalance = FindPrevCreditBalance(dt, cnum, 0D); // The First
                    actualMonthPaid = payment / amtOfMonthlyPayment;
                    dueDatePayment = payment + oldCreditBalance + originalCreditBalance;
                    double newActualMonthPaid = dueDatePayment / amtOfMonthlyPayment;
                    if ((oldCreditBalance + originalCreditBalance + (payment % amtOfMonthlyPayment)) > amtOfMonthlyPayment)
                        actualMonthPaid += 1D;
                    //if (!insurance)
                    //    payment = dueDatePayment;

                    //oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();
                    currentDueDate = oldDueDate;

                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
                    if (creditBalance > 0D)
                    {
                    }
                    //dRow["monthsPaid"] = actualMonthPaid;
                    actualMonthPaid += 0.00000000001D;
                    months = actualMonthPaid;
                    originalCreditBalance = 0D;

                }
                if (lastDueDate.Year > 1910)
                {
                    if (!DailyHistory.isInsurance(cnum))
                    {
                        lastDueDate = FindMismatches.VerifyDueDate(cnum);
                        previousMonths = 0D;
                        if (FindPreviousPayments(dt, cnum, ref previousMonths))
                            months += previousMonths;
                    }

                    nextDueDate = lastDueDate.AddMonths(Convert.ToInt32(Math.Truncate(months)));
                }

                dRow["found"] = found;
                dRow["agent"] = agent;
                dRow["fname"] = fname;
                dRow["lname"] = lname;
                if (!String.IsNullOrWhiteSpace(payer))
                    dRow["name"] = fname + " " + lname + " (" + payer + ")";
                else
                    dRow["name"] = fname + " " + lname;
                if (DailyHistory.isInsurance(cnum))
                {
                    trust85P = 0D;
                    trust100P = 0D;
                    balance = Policies.CalcMonthlyPremium(payer, docp);
                    principal = dRow["payment"].ObjToDouble();
                    interest = 0D;
                }
                if (duplicate)
                    dRow["duplicate"] = "Y";
                bool problem = CheckDeceasedRemoved(cnum);
                dRow["firstPayDate"] = firstPayDate;
                dRow["contractDate"] = contractDate;
                dRow["balance"] = balance;
                dRow["oldbalance"] = oldbalance;
                dRow["oldTotalInt"] = oldTotalInt;
                dRow["interest"] = interest;
                dRow["principal"] = principal;
                dRow["days"] = days;
                dRow["apr"] = (apr / 100.0D);
                dRow["!contractRecord"] = contractRecord;
                dRow["amtOfMonthlyPayt"] = amtOfMonthlyPayment;
                dRow["interestTotal"] = interestTotal;
                dRow["totalPaid"] = totalPaid;
                dRow["issueDate"] = issueDate;
                dRow["trust85P"] = trust85P;
                dRow["trust100P"] = trust100P;
                //if (trust100P > 0D)
                //    dRow["retained"] = payment - trust100P;
                dRow["creditBalance"] = creditBalance;
                dRow["payer"] = payer;
                dRow["line"] = line;
                dRow["unpaid_interest"] = unpaid_interest;
                //dRow["retained"] = 555D;
                dRow["retained"] = 0D;
                if (principal <= 0D)
                {
                    if ( nextDueDate < oldDueDate )
                        nextDueDate = oldDueDate;
                }
                dRow["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                if (!DailyHistory.isInsurance(cnum) && balance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
                {
                    //newTrust85 = 0D;
                    //if (duplicate && dolp == docp)
                    //{
                    //    FindTrust85(dt, cnum, ref newTrust85);
                    //}
                    //bool isPaid = false;
                    //if (!cnum.Contains("LI"))
                    //    isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, trust85P + newTrust85 );
                    //if (isPaid)
                    //{
                    //    double Trust85Paid = totalTrust85;
                    //    double difference = maxTrust85 - totalTrust85;
                    //    difference = G1.RoundValue(difference);

                    //    dRow["difference"] = difference;

                    //    dRow["duplicates"] = "FINALE! FINALE! FINALE!";
                    //    dRow["nextDueDate"] = "12/31/2039";
                    //    trust85P = trust85P + difference;
                    //    if (trust85P <= 0D)
                    //        trust85P = 0D;
                    //    dRow["trust85P"] = trust85P;
                    //    trust100P = trust85P / 0.85D;
                    //    trust100P = G1.RoundValue(trust100P);
                    //    dRow["trust100P"] = trust100P;
                    //}
                    string payoff = processNewPayOff(dt, dRow, cnum, duplicate, dolp, docp, amtOfMonthlyPayment, numPayments, contractValue, trust85P, trust100P, balance );
                }
                if (balance <= 0D)
                {
                    if (paymentsFile != "ipayments")
                        dRow["nextDueDate"] = "12/31/2039";
                }
                dt.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
                int thisRow = dt.Rows.Count;
                MessageBox.Show("***ERROR*** (" + cnum + " " + thisRow.ToString() + ") " + line + " " + ex.Message.ToString());
            }
            return rtn;
        }
        /***********************************************************************************************/
        public static void HandleUnpaidInterest(double payment, double lastUnpaidInterest, ref double interest, ref double unpaid_interest, ref double principal, ref double balance)
        {
            //unpaid_interest = 0D;
            if (lastUnpaidInterest > 0D)
            {
                double newprincipal = 0D;
                double oldInterest = interest;
                interest = interest + lastUnpaidInterest;
                double originalInterest = interest;
                if (interest > payment)
                {
                    interest = payment;
                    unpaid_interest = originalInterest - interest;
                    unpaid_interest = G1.RoundDown(unpaid_interest);
                    newprincipal = 0D;
                    balance = balance + unpaid_interest;
                }
                else
                {
                    newprincipal = payment - interest;
                    unpaid_interest = 0D;
                    balance = balance + (principal - newprincipal);
                }
                newprincipal = G1.RoundValue(newprincipal);
                //unpaid_interest = principal - newprincipal;
                //unpaid_interest = G1.RoundValue(unpaid_interest);
                principal = newprincipal;
                if (newprincipal > 0D)
                    unpaid_interest = 0D;
            }
        }
        /***********************************************************************************************/
        public static void HandleUnpaidInterest ( string contractNumber, double payment, ref double interest, ref double unpaid_interest, ref double principal, ref double balance )
        {
            //unpaid_interest = 0D;
            string cmd = "";
            if (DailyHistory.isInsurance(contractNumber))
                cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
            else
                cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                double lastUnpaidInterest = dx.Rows[0]["unpaid_interest"].ObjToDouble();
                if (lastUnpaidInterest > 0D)
                {
                    double newprincipal = 0D;
                    double oldInterest = interest;
                    interest = interest + lastUnpaidInterest;
                    double originalInterest = interest;
                    if (interest > payment)
                    {
                        interest = payment;
                        unpaid_interest = originalInterest - interest;
                        unpaid_interest = G1.RoundDown(unpaid_interest);
                        newprincipal = 0D;
                        balance = balance + unpaid_interest;
                    }
                    else
                    {
                        newprincipal = payment - interest;
                        unpaid_interest = 0D;
                        balance = balance + (principal - newprincipal) ;
                    }
                    newprincipal = G1.RoundValue(newprincipal);
                    //unpaid_interest = principal - newprincipal;
                    //unpaid_interest = G1.RoundValue(unpaid_interest);
                    principal = newprincipal;
                    if (newprincipal > 0D)
                        unpaid_interest = 0D;
                }
            }
        }
        /***********************************************************************************************/
        private bool CheckDeceasedRemoved ( string contractNumber )
        {
            if (String.IsNullOrWhiteSpace(contractNumber))
                return false;
            if (DailyHistory.isInsurance(contractNumber))
                return false;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            string trustRemoved = dt.Rows[0]["trustRemoved"].ObjToString();
            bool problem = false;
            if ( deceasedDate.Year > 100)
            {
                problem = true;
                paymentsAfterDeath++;
            }
            if ( trustRemoved.ToUpper() == "YES" || trustRemoved.ToUpper() == "Y" )
            {
                problem = true;
                paymentsAfterRemoved++;
            }
            return problem;
        }
        /***********************************************************************************************/
        private double FindPrevCreditBalance(DataTable dt, string contractNumber, double originalCreditBalance)
        {
            double creditBal = originalCreditBalance;
            DataRow[] dRows = dt.Select("cnum='" + contractNumber + "'");
            if (dRows.Length >= 1)
            {
                for (int i = 0; i < dRows.Length; i++)
                    creditBal = dRows[i]["creditBalance"].ObjToDouble();
            }
            return creditBal;
        }
        /***********************************************************************************************/
        private bool FindDOLP(DataTable dt, string contractNumber, ref DateTime dolp, ref double oldBalance, ref DateTime lastDueDate, ref double trust85P )
        {
            trust85P = 0D;
            bool found = false;
            DataRow[] dRows = dt.Select("cnum='" + contractNumber + "'");
            if (dRows.Length >= 1)
            {
                dolp = dRows[0]["date"].ObjToDateTime();
                for (int i = 0; i < dRows.Length; i++)
                {
                    oldBalance = dRows[i]["balance"].ObjToDouble();
                    lastDueDate = dRows[i]["nextDueDate"].ObjToDateTime();
                    trust85P += dRows[i]["trust85P"].ObjToDouble();
                }
                found = true;
            }
            return found;
        }
        /***********************************************************************************************/
        private bool FindPreviousPayments(DataTable dt, string contractNumber, ref double previousMonths )
        {
            previousMonths = 0D;
            bool found = false;
            try
            {
                DataRow[] dRows = dt.Select("cnum='" + contractNumber + "'");
                if (dRows.Length >= 1)
                {
                    for (int i = 0; i < dRows.Length; i++)
                        previousMonths += dRows[i]["monthsPaid"].ObjToDouble();
                    found = true;
                }
            }
            catch ( Exception ex)
            {
            }
            return found;
        }
        /***********************************************************************************************/
        private bool FindTrust85(DataTable dt, string contractNumber, ref double trust85 )
        {
            bool found = false;
            trust85 = 0D;
            DataRow[] dRows = dt.Select("cnum='" + contractNumber + "'");
            if (dRows.Length >= 1)
            {
                for (int i = 0; i < dRows.Length; i++)
                {
                    trust85 += dRows[i]["trust85P"].ObjToDouble();
                }
                found = true;
            }
            return found;
        }
        /***********************************************************************************************/
        public static bool CalcPrincipalInterest(double oldBalance, DateTime dolp, DateTime docp, double payment, double apr, ref double principal, ref double interest, ref int days, ref double unpaid_interest )
        {
            unpaid_interest = 0D;
            TimeSpan ts = docp - dolp;
            days = ts.Days;
            int yearlyDays = 365;
            if (DateTime.IsLeapYear(docp.Year))
                yearlyDays = 366;
            double divide = 100.0D;
            if (apr < 1.0D)
                divide = 1.0D;
            double dailyInterest = apr / divide / (double)(yearlyDays) * (double)(days);
            interest = dailyInterest * oldBalance; //0.0036986301369863017
            interest = G1.RoundDown(interest);
            if (interest < 0D && docp >= DailyHistory.interestDate)  // Don't allow Interest to go negative after this date
                interest = 0D;
            decimal prince = (decimal)(payment - interest);
            prince = decimal.Round(prince, 2);
            principal = (double)prince;
            //principal = payment - interest;
            //principal = G1.RoundDown(principal);
            if (principal < 0D)
            {
                unpaid_interest = Math.Abs(principal);
                principal = 0D;
                interest = payment;
            }
            double remainder = G1.RoundValue (payment - (principal + interest));
            if (remainder != 0D)
            {
                interest += remainder;
                //interest = G1.RoundDown(interest);
                //principal = payment - interest;
                //principal = G1.RoundDown(principal);
            }
            return true;
        }
    /***********************************************************************************************/
    public static int CalcTrust85P ( DateTime docp, double amtOfMonthlyPayt, string issueDate, double contractValue, double downpayment, double financeMonths, double payment, double principal, double debit, double credit, double apr, ref double trust85P, ref double trust100P, ref double retained )
        {
            double saveTrust85 = trust85P;
            double saveTrust100 = trust100P;
            if ( docp > DailyHistory.secondDate )
            {
                if (principal < 0D && debit == 0D )
                {
                    trust85P = 0D;
                    trust100P = 0D;
                    retained = payment;
                    if ( debit == 0D)
                        return 0;
                }
            }
            if ( debit > 0D)
            {
                retained = payment - trust100P;
                retained = debit - Math.Abs(trust100P);
                retained = retained * -1D;
                return 0;
            }
            double savePayment = payment;
            double saveCredit = credit;
            double saveDebit = debit;
            double savePrincipal = principal;
            double interest = payment - principal;
            bool gotDebit = false;
            if (debit > 0D)
            {
                gotDebit = true;
                //payment = debit;
                //principal = debit;
                interest = debit + principal;
                if (interest < 0D)
                    interest = interest * -1D;
                payment = debit + interest;
                principal = principal * -1D;
            }
            else if ( credit > 0D)
            {
                payment = credit;
                //principal = 0D;
            }
            if (downpayment > 0D && principal == 0D && payment == 0D )
                principal = downpayment;
            int method = 0;
//            if ( docp > DailyHistory.secondDate )
                method = CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downpayment, financeMonths, payment, principal, apr, ref trust85P, ref trust100P);
            //if ( docp <= DailyHistory.secondDate )
            //{
            //    trust85P = saveTrust85;
            //    trust100P = saveTrust100;
            //}
            retained = 0D;
            if ( payment > 0D)
                retained = payment - trust100P;

            retained = CalculateRetained(payment, credit, debit, interest, trust100P);

            if (gotDebit)
            {
                trust100P = trust100P * -1D;
                trust85P = trust85P * -1D;
                retained = retained * -1D;
            }
            return method;
        }
        /***********************************************************************************************/
        public static double CalculateRetained ( double payment, double credit, double debit, double interest, double trust100P)
        {
            double retained = 0D;
            if (payment > 0D)
                retained = payment - trust100P;
            else if (credit > 0D)
                retained = credit - trust100P;
            else if (debit > 0D)
            {
                retained = debit - Math.Abs (trust100P);
                retained = retained * -1D;
            }
            retained = G1.RoundValue(retained);
            return retained;
        }
        /***********************************************************************************************/
        public static int CalcTrust85(double amtOfMonthlyPayt, string issueDate, double contractValue, double downpayment, double financeMonths, double payment, double principal, double apr, ref double trust85P, ref double trust100P)
        {
            trust85P = 0;
            trust100P = 0;
            DateTime testDate = new DateTime(2017, 12, 1);
            DateTime contractDate = issueDate.ObjToDateTime();
            DateTime fiftyDate = new DateTime(2006, 6, 30);
            int method = 0;
            if (apr <= 0D)
            { // No Interest Rate
                principal = G1.RoundValue(principal);
                trust85P = principal * .85D;
                if ( contractDate <= fiftyDate )
                    trust85P = principal * .5D;
                trust85P = G1.RoundDown(trust85P);
                trust100P = principal;
                method = 1;
                return method;
            }
            else if (contractDate >= testDate)
            { // Contracts after 12/1/2017
                principal = G1.RoundValue(principal);
                trust85P = principal * .85D;
                if (contractDate <= fiftyDate)
                    trust85P = principal * .5D;
                //else
                //    trust85P += 0.0005D;
                //double diff = trust85P % 1.00D;
                //diff = Math.Ceiling(trust85P * 1000D) / 1000D;
                //trust85P = G1.RoundValue(trust85P);
                trust85P = G1.RoundDown(trust85P);
                trust100P = principal;
                method = 2;
                return method;
            }
            else
            { // Contracts prior to 12/1/2017 // This calculates exactly what is calculated in the AS-400
                method = 3;
                trust100P = contractValue;
                if (financeMonths > 0D)
                {
                    decimal trust100D = (decimal)((contractValue - downpayment) / financeMonths);
                    trust100P = (double)(trust100D);
                    //trust100P = (contractValue - downpayment) / financeMonths;
                    trust100P = G1.RoundDown(trust100P);
                    //trust100P = G1.RoundValue(trust100P);
                    //trust100P = G1.RoundUp(trust100P);

                    double ratio = 0.88494399D;
                    if (amtOfMonthlyPayt > 0D)
                    {
                        if (payment != amtOfMonthlyPayt)
                        {
                            trust100D = (decimal)((payment / amtOfMonthlyPayt * trust100P));
                            //trust100P = payment / amtOfMonthlyPayt * trust100P;
                            trust100P = (double)(trust100D);
                            trust100P = G1.RoundDown(trust100P);
                        }
                    }
                    else
                    {
                        trust100P = payment * ratio;
                        trust100P = G1.RoundDown(trust100P);
                    }
                    //trust100P = G1.RoundValue(trust100P);
                    //trust100P = G1.RoundDown(trust100P);

                    //                    trust100P = (contractValue) / financeMonths;
                    //int count = (int)(principal / trust100P); // For amounts greater than 1 equal payment
                    //count = (int)(payment / trust100P); // For amounts greater than 1 equal payment
                    //if (count > 1)
                    //    trust100P = trust100P * count; // Customer made more than one payment
                    //else if (count <= 0)
                    //{
                    //    double amount = payment / trust100P;
                    //    amount = amount * trust100P;
                    //    trust100P = G1.RoundValue(amount);
                    //}
                }
                if (downpayment == payment && payment == principal)
                    trust100P = payment;
                decimal trust85D = (decimal)(trust100P * .85D);
                if (contractDate <= fiftyDate)
                    trust85D = (decimal)(trust100P * .5D);
                trust85P = (double)(trust85D);
                //trust85P = trust100P * .85D; // trust85 is 85% of trust100
//                trust85P = G1.RoundValue(trust85P);
                trust85P = G1.RoundDown(trust85P);
                if (payment == 0D && principal == 0D) // This is when manual payments are made with only debit or credit
                {
                    trust100P = 0D;
                    trust85P = 0D;
                }
                if (principal < 0D && payment < 0D)
                { // Must be debit
                    trust100P = trust100P * (-1D);
//                    trust85P = 0D * (-1D);
                    trust85P = trust85P * (-1D);
                }
                else if ( principal < 0D && payment == 0D)
                { // Looks like a real debit
                    trust85P = G1.RoundDown(trust85P);
                    trust100P = G1.RoundDown(trust100P);
                    trust100P = trust100P * (-1D);
                    trust85P = trust85P * (-1D);
                }
                else
                {
                    trust85P = G1.RoundDown(trust85P);
                }
                return method;
            }
            trust85P = G1.RoundDown(trust85P);
            trust100P = G1.RoundDown(trust100P);
            return method;
        }
        /***********************************************************************************************/
        private bool checkDuplicatePayment(string contract)
        {
            if (String.IsNullOrWhiteSpace(contract))
            {
                MessageBox.Show("***ERROR*** Invalid Key!\nContract must be unique and not blank!", "Import Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Duplicate Key!\nYou must enter a unique Contract!", "Import Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            return false;
        }
        /***********************************************************************************************/
        private void btnImportFile_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (!CheckImportFile(dt))
                return;

            bool good = CalculateGoodBadUgly(dt);
            //if ( !good)
            //{
            //    MessageBox.Show("***ERROR*** There are still contracts or payers not found!\nYou must fix NOT FOUND issues before moving on!", "Import Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            bool doLine = false;
            if (G1.get_column_number(dt, "line") >= 0)
                doLine = true;

            if (G1.get_column_number(dt, "fill1") < 0)
                dt.Columns.Add("fill1");

            string cnum = "";
            string date = "";
            string dueDate = "";
            string oldDate = "";
            string code = "";
            string locationcode = "";
            double expected = 0D;
            double payment = 0D;
            string found = "";
            string cmd = "";
            string record = "";
            string paymentRecord = "";
            string agent = "";
            string lname = "";
            string fname = "";
            double principal = 0D;
            double interest = 0D;
            double apr = 0D;
            double balance = 0D;
            double oldbalance = 0D;
            double interestTotal = 0D;
            double oldTotalInt = 0D;
            string lastDatePaid8 = "";
            TimeSpan ts;
            DateTime lastDatePaid8Date = DateTime.Now;
            DateTime currentDatePaid = DateTime.Now;
            string contractRecord = "";
            double trust85P = 0D;
            double trust100P = 0D;
            double retained = 0D;
            double creditBalance = 0D;
            double oldPayment = 0D;
            double oldInterest = 0D;
            double oldPrincipal = 0D;
            string line = "";
            bool insurance = false;
            DateTime nextDueDate = DateTime.Now;
            DateTime saveDueDate = DateTime.Now;
            DateTime docp = DateTime.Now;
            DateTime oDate = DateTime.Now;
            DateTime oldDueDate = DateTime.Now;
            string file = ImportFileName;
            DateTime currentDueDate = DateTime.Now;
            bool doUpdateBankAccount = getUpdateBankAccounts();
            string payer = "";
            double unpaid_interest = 0D;
            string ID = "";
            string fill1 = "";
            bool gotTCA = false;
            string TCADepositNumber = "";

            bool isBankCC = false;
            if ( G1.get_column_number ( dt, "bankAccount") >= 0 )
                isBankCC = true;

            int i = 0;

            int lastrow = dt.Rows.Count;
            //            lastrow = 1;
            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            lblTotal.Text = "of " + lastrow.ToString();
            picLoader.Show();

            DateTime dateNow = DateTime.Now;
            string depositNumber = "T" + dateNow.Year.ToString("D4") + dateNow.Month.ToString("D2") + dateNow.Day.ToString("D2");
            if (workACH || workDraft )
                depositNumber = "A" + dateNow.Year.ToString("D4") + dateNow.Month.ToString("D2") + dateNow.Day.ToString("D2");

            string tempfile = parseFilename(ImportFileName);
            if (tempfile == "-1")
            {
                MessageBox.Show("***ERROR*** Problem Decoding Import File Name!");
                return;
            } 
            ImportFileDate = parseFileDate(tempfile);
            if (!G1.validate_date(ImportFileDate))
                ImportFileDate = DateTime.Now.ToString("MM/dd/yyyy");

            string bankRecord = "";

            DataTable insDt = null;
            string insRecord = "";


            int where = 1;
            string paymentsFile = "";
            string contractsFile = "";
            string customersFile = "";
            string finale = "";
            string duplicates = "";
            string empty2 = "";

            double ccFee = 0D;
            DataTable dx = null;

            string bankAccount = lkbx_ach_account;
            if (workTheFirst)
                bankAccount = tfbx_account;
            else if (workCC)
                bankAccount = cc_account;
            else if (workACH)
                bankAccount = ach_account;
            else if (workDraft)
                bankAccount = ach_account;

            this.gridBand2.Caption = "Lock Box Data - " + bankAccount;
            double numMonths = 0D;
            double contractValue = 0D;

            try
            {
                int theEnd = dt.Rows.Count;
                //                theEnd = 1; // For Debug Purposes
                for (i = 0; i < theEnd; i++)
                {
                    barImport.Value = (i+1);
                    barImport.Refresh();
                    labelMaximum.Text = (i+1).ToString();
                    labelMaximum.Refresh();
                    picLoader.Refresh();

                    found = dt.Rows[i]["found"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(found))
                        continue;
                    cnum = dt.Rows[i]["cnum"].ObjToString();
                    if (String.IsNullOrWhiteSpace(cnum))
                        continue;
                    code = dt.Rows[i]["code"].ObjToString();

                    payer = dt.Rows[i]["payer"].ObjToString();

                    bankAccount = lkbx_ach_account;
                    if (workTheFirst)
                        bankAccount = tfbx_account;
                    else if (workCC)
                        bankAccount = cc_account;
                    else if (workACH)
                        bankAccount = ach_account;
                    else if (workDraft)
                        bankAccount = ach_account;

                    date = dt.Rows[i]["date"].ObjToString();
                    docp = date.ObjToDateTime();
                    line = "";

                    dueDate = dt.Rows[i]["dueDate"].ObjToString();
                    oldDueDate = dueDate.ObjToDateTime();
                    nextDueDate = dt.Rows[i]["nextDueDate"].ObjToDateTime();
                    saveDueDate = nextDueDate;
                    expected = dt.Rows[i]["expected"].ObjToDouble();
                    payment = dt.Rows[i]["payment"].ObjToDouble();
                    interest = dt.Rows[i]["interest"].ObjToDouble();
                    apr = dt.Rows[i]["apr"].ObjToDouble();
                    apr = apr * 100D;
                    principal = payment - interest;
                    locationcode = dt.Rows[i]["locationcode"].ObjToString();
                    date = GetSQLDate(dt, i, "date");
                    dueDate = GetSQLDate(dt, i, "dueDate");
                    agent = dt.Rows[i]["agent"].ObjToString();
                    lname = dt.Rows[i]["lname"].ObjToString();
                    fname = dt.Rows[i]["fname"].ObjToString();
                    balance = dt.Rows[i]["balance"].ObjToDouble();
                    oldbalance = dt.Rows[i]["oldbalance"].ObjToDouble();
                    trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                    retained = dt.Rows[i]["retained"].ObjToDouble();
                    interestTotal = dt.Rows[i]["interestTotal"].ObjToDouble();
                    oldTotalInt = dt.Rows[i]["oldTotalInt"].ObjToDouble();
                    lastDatePaid8 = dt.Rows[i]["lastDatePaid8"].ObjToString();
                    numMonths = dt.Rows[i]["monthsPaid"].ObjToDouble();
                    unpaid_interest = dt.Rows[i]["unpaid_interest"].ObjToDouble();
                    ID = dt.Rows[i]["ID"].ObjToString();
                    fill1 = dt.Rows[i]["fill1"].ObjToString();
                    contractRecord = dt.Rows[i]["!contractRecord"].ObjToString();

                    ccFee = 0D;

                    if ( isBankCC ) // This does not do anything because updates to Trust Down Payments and Funeral Credit Cards are performed back in CCImport when this finishes.
                    {
                        depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                        ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                        if (code == "04") // Trust Down Payment
                            continue;
                        if ( code == "03") // Funeral Credit Card Payment
                        {
                            cmd = "Select * from `fcust_extended` where `serviceId` = '" + cnum + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count <= 0)
                                continue;
                            string serviceId = cnum;
                            cnum = dx.Rows[0]["contractNumber"].ObjToString();
                            cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + cnum + "' AND `type` = 'Credit Card';";
                            dx = G1.get_db_data(cmd);
                            if ( dx.Rows.Count > 0 )
                            { // Do Nothing Here
                            }
                            continue;
                        }
                    }
                    finale = "";
                    duplicates = dt.Rows[i]["duplicates"].ObjToString().ToUpper(); // Import Button Pressed 
                    if (duplicates.IndexOf("FINALE!") >= 0)
                    {
                        finale = "finale";
                        double amtOfMonthlyPayment = 0D;
                        int numPayments = 0;
                        double maxTrust85 = 0D;
                        double totalTrust85 = 0D;

                        bool isPaid = false;
                        bool trustThreshold = false;
                        bool balanceThreshold = false;
                        //if (!cnum.Contains("LI"))
                            isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, ref trustThreshold, ref balanceThreshold, trust85P );
                        double Trust85Paid = totalTrust85 + trust85P;
                        double difference = maxTrust85 - totalTrust85;
                        difference = G1.RoundValue(difference);

                        if (balance <= LoginForm.minimumForceBalance)
                            balanceThreshold = true;
                        else
                            isPaid = false;

                        //difference = dt.Rows[i]["difference"].ObjToDouble();

                        string payOffMethod = LoginForm.allowPayOffMethod;
                        difference = G1.RoundValue(difference);
                        if ( LoginForm.useNewTCACalculation )
                        {
                            if (String.IsNullOrWhiteSpace(payOffMethod))
                            {
                                difference = 0D;
                                isPaid = false;
                            }
                            if (isPaid && payOffMethod.ToUpper() == "DEBIT")
                            {
                                if (difference > 0D)
                                {
                                    difference = 0D;
                                    isPaid = false;
                                }
                            }
                            else if (isPaid && payOffMethod.ToUpper() == "CREDIT")
                            {
                                if (difference < 0D)
                                {
                                    difference = 0D;
                                    //isPaid = false;
                                }
                            }
                        }
                        else
                        {
                            difference = dt.Rows[i]["difference"].ObjToDouble();
                            difference = G1.RoundValue(difference);
                        }

                        if (isPaid && difference != 0D)
                        {
                            bool success = ManualPayment.UpdateForcedPayoff(cnum, docp, difference, contractValue, maxTrust85, totalTrust85);
                        }
                        //if (1 == 1)
                        //    return;
                    }
                    if (doLine)
                        line = dt.Rows[i]["line"].ObjToString();
                    oldPrincipal = 0D;
                    where = 2;

                    insurance = false;
                    paymentsFile = "payments";
                    contractsFile = "contracts";
                    customersFile = "customers";
                    if (code == "02")
                    {
                        paymentsFile = "ipayments";
                        contractsFile = "icontracts";
                        customersFile = "icustomers";
                        insurance = true;
                    }

                    DataTable cDt = G1.get_db_data("Select * from `" + contractsFile + "` where `contractNumber` = '" + cnum + "';");
                    if (cDt.Rows.Count < 0)
                    {
                        dt.Rows[i]["num"] = "*ERROR*";
                        dt.Rows[i]["found"] = "NO";
                        continue;
                    }
                    where = 3;

                    if (!String.IsNullOrWhiteSpace(contractRecord))
                    {
                        //                        balance = oldbalance - principal;
                        interestTotal = interestTotal + interest;
                        balance = G1.RoundValue(balance);
                        interestTotal = G1.RoundValue(interestTotal);
                        where = 11;

                        double months = 0D;
                        currentDueDate = oldDueDate;
                        DailyHistory.ReCalculateDueDate(cnum, docp, expected, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
                        dueDate = currentDueDate.ToString("MM/dd/yyyy");
                        if ( insurance )
                        {
                            nextDueDate = dt.Rows[i]["nextDueDate"].ObjToDateTime();
                            months = dt.Rows[i]["monthsPaid"].ObjToDouble();
                            creditBalance = dt.Rows[i]["creditBalance"].ObjToDouble();
                        }
                        else
                        {
                            if (saveDueDate.Year == 2039)
                                nextDueDate = saveDueDate;
                            else
                                nextDueDate = dt.Rows[i]["nextDueDate"].ObjToDateTime();
                        }
                        empty2 = dt.Rows[i]["empty2"].ObjToString().ToUpper();
                        dueDate = nextDueDate.Month.ToString("D2") + "/" + nextDueDate.Day.ToString("D2") + "/" + nextDueDate.Year.ToString("D4");
                        dueDate = G1.DTtoMySQLDT(dueDate).ObjToString();
                        if ((balance <= 0D && !insurance) )
                            dueDate = "12/31/2039";
                        if (finale.ToUpper() == "FINALE" || empty2 == "EXCEEDED" || empty2 == "BROKEN")
                            dueDate = "12/31/2039";

                            //                        G1.update_db_table("contracts", "record", contractRecord, new string[] { "totalInterest", interestTotal.ToString(), "balanceDue", balance.ToString(), "dueDate8", dueDate, "lastDatePaid8", date});

                        if (contractsFile.ToUpper() == "ICONTRACTS")
                        {
                            if (!String.IsNullOrWhiteSpace(payer))
                            {
                                insDt = G1.get_db_data("Select * from `payers` WHERE `payer` = '" + payer + "';");
                                if (insDt.Rows.Count > 0)
                                {
                                    insRecord = insDt.Rows[0]["record"].ObjToString();
                                    G1.update_db_table("payers", "record", insRecord, new string[] { "dueDate8", dueDate.ObjToDateTime().ToString("MM/dd/yyyy"), "lastDatePaid8", date });
                                }
                            }
                        }
                        if (DailyHistory.isInsurance(cnum))
                            G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "dueDate8", dueDate, "lastDatePaid8", date, "creditBalance", creditBalance.ToString(), "unpaid_interest", unpaid_interest.ToString() });
                        else
                            G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "balanceDue", balance.ToString(), "dueDate8", dueDate, "lastDatePaid8", date, "creditBalance", creditBalance.ToString(), "unpaid_interest", unpaid_interest.ToString() });

                        lastDatePaid8Date = lastDatePaid8.ObjToDateTime();
                        where = 12;
                        dueDate = currentDueDate.ToString("MM/dd/yyyy");


                        bankRecord = G1.create_record("bank_file", "code", "-1");
                        if (G1.BadRecord("bank_file", bankRecord))
                            break;

                        if (workACH)
                        {
                            where = 4;
                            cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' and `payDate8` = '" + date + "';";
                            DataTable dddx = G1.get_db_data(cmd);
                            if (dddx.Rows.Count > 0)
                                ManualPayment.CleanupWork(dddx);
                            if (dddx.Rows.Count > 0)
                            {
                                paymentRecord = dddx.Rows[0]["record"].ObjToString();
                                oldPayment = dddx.Rows[0]["paymentAmount"].ObjToDouble();
                                oldInterest = dddx.Rows[0]["interestPaid"].ObjToDouble();
                                oldPrincipal = oldPayment - oldInterest;
                                oDate = dddx.Rows[0]["payDate8"].ObjToDateTime();
                                oldDate = oDate.ToString("yyyy-MM-dd");
                                if (oldDate == date)
                                {
                                    if (oldPayment == payment)
                                        continue;
                                }
                                if (1 == 1)
                                    continue; // Do No Harm!
                            }
                            else
                            {
                                where = 4;

                                paymentRecord = G1.create_record(paymentsFile, "lastName", "-1");
                                if (G1.BadRecord(paymentsFile, paymentRecord))
                                    break;
                            }
                        }
                        else
                        {
                            where = 5;
                            paymentRecord = G1.create_record(paymentsFile, "lastName", "-1");
                            if (G1.BadRecord(paymentsFile, paymentRecord))
                                break;
                        }
                        where = 6;

                        G1.update_db_table("bank_file", "record", bankRecord, new string[] { "filename", tempfile, "!paymentRecord", paymentRecord, "contractNumber", cnum, "code", code, "contractDate", date, "expected", expected.ToString(), "payment", payment.ToString(), "location", locationcode, "agent", agent, "lastName", lname, "firstName", fname, "found", found });
                        picLoader.Refresh();
                        G1.update_db_table("bank_file", "record", bankRecord, new string[] { "principal", principal.ToString(), "interest", interest.ToString(), "apr", apr.ToString(), "dueDate8", dueDate, "totalInterest", oldTotalInt.ToString(), "balanceDue", oldbalance.ToString(), "lastDatePaid8", lastDatePaid8 });

                        picLoader.Refresh();

                        if ( !doUpdateBankAccount)
                            bankAccount = ""; // Force to "" for now.
                        try
                        {

                            G1.update_db_table(paymentsFile, "record", paymentRecord, new string[] { "contractNumber", cnum, "dueDate8", dueDate, "payDate8", date, "paymentAmount", payment.ToString(), "interestPaid", interest.ToString(), "depositNumber", depositNumber, "location", locationcode, "agentNumber", agent, "lastName", lname, "firstName", fname, "userId", LoginForm.username, "ccFee", ccFee.ToString() });
                            currentDatePaid = date.ObjToDateTime();
                            if (!DailyHistory.isInsurance(cnum))
                            {
                                if (G1.GetMonthsBetween ( currentDatePaid, lastDatePaid8Date ) > 6 )
                                    Messages.SendLatePayment(cnum);
                                DateTime calcDueDate = FindMismatches.VerifyDueDate(cnum);
                                nextDueDate = dt.Rows[i]["nextDueDate"].ObjToDateTime();
                                if (nextDueDate.Year != 2039)
                                {
                                    if (calcDueDate != nextDueDate)
                                    {
                                        if (finale.ToUpper() != "FINALE")
                                        {
                                            dt.Rows[i]["nextDueDate"] = calcDueDate.ToString("MM/dd/yyyy");
                                            G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "dueDate8", G1.DTtoMySQLDT(calcDueDate).ObjToString() });
                                        }
                                    }
                                }
                            }
                            where = 7;
                            if (finale.ToUpper() == "FINALE")
                            {
                                G1.update_db_table(paymentsFile, "record", paymentRecord, new string[] { "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "new", "finale", "bank_account", bankAccount, "unpaid_interest", unpaid_interest.ToString(), "s50", ID, "retained", retained.ObjToString(), "new", "finale" });
                                //contractValue = DailyHistory.GetContractValue(cnum);

                                //bool success = ManualPayment.UpdateForcedPayoff(cnum, docp, difference, contractValue, maxTrust85, totalTrust85);
                            }
                            else
                                G1.update_db_table(paymentsFile, "record", paymentRecord, new string[] { "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "new", ImportFileDate, "bank_account", bankAccount, "unpaid_interest", unpaid_interest.ToString(), "s50", ID, "retained", retained.ObjToString() });
                            where = 8;
                            if (doLine)
                                G1.update_db_table(paymentsFile, "record", paymentRecord, new string[] { "importedData", line });
                            where = 9;
                            G1.update_db_table(paymentsFile, "record", paymentRecord, new string[] { "oldBalance", oldbalance.ToString(), "oldDueDate8", dueDate, "oldDOLP", lastDatePaid8, "fill1", fill1 });
                            if (paymentsFile.ToUpper() == "IPAYMENTS")
                                G1.update_db_table(paymentsFile, "record", paymentRecord, new string[] { "numMonthPaid", numMonths.ToString(), "payer", payer });

                            CheckCC(cnum, payer, numMonths);

                            where = 10;
                            if (duplicates.ToUpper().IndexOf("TCA") > 0)
                            {
                                try
                                {
                                    gotTCA = GrabTCA(duplicates, ref trust85P, ref trust100P);
                                    if (gotTCA)
                                    {
                                        record = G1.create_record(paymentsFile, "lastName", "-1");
                                        if (!G1.BadRecord(paymentsFile, record))
                                        {
                                            retained = 0D;
                                            interest = 0D;
                                            if (cnum.Contains("LI"))
                                                interest = -1D * trust100P;
                                            else
                                                retained = -1D * trust100P;

                                            TCADepositNumber = getNextTCANumber();
                                            TCADepositNumber = "TCA-" + TCADepositNumber;

                                            G1.update_db_table(paymentsFile, "record", record, new string[] { "contractNumber", cnum, "lastName", lname, "firstName", fname, "paymentAmount", "0.00", "interestPaid", interest.ToString(), "debitAdjustment", "0.00", "creditAdjustment", "0.00", "debitReason", "", "creditReason", "TCA", "unpaid_interest", "0.00", "fill1", fill1 });
                                            G1.update_db_table(paymentsFile, "record", record, new string[] { "CheckNumber", "", "dueDate8", dueDate, "payDate8", date, "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "retained", retained.ToString(), "location", locationcode, "agentNumber", agent, "userId", LoginForm.username, "depositNumber", TCADepositNumber, "edited", "TrustAdj", "bank_account", bankAccount });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("A critical exception has occurred while attempting to write detail (" + record + ") for a PaidOff Contract :\n" + ex.Message, "PaidOff Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        picLoader.Refresh();
                        //if (!String.IsNullOrWhiteSpace(contractRecord))
                        //{
                        //    //                        balance = oldbalance - principal;
                        //    interestTotal = interestTotal + interest;
                        //    balance = G1.RoundValue(balance);
                        //    interestTotal = G1.RoundValue(interestTotal);
                        //    where = 11;

                        //    double months = 0D;
                        //    DailyHistory.ReCalculateDueDate(cnum, docp, expected, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months);
                        //    dueDate = nextDueDate.Month.ToString("D2") + "/" + nextDueDate.Day.ToString("D2") + "/" + nextDueDate.Year.ToString("D4");
                        //    dueDate = G1.DTtoMySQLDT(dueDate).ObjToString();
                        //    if (balance <= 0D && !insurance)
                        //        dueDate = "12/31/2039";

                        //    //                        G1.update_db_table("contracts", "record", contractRecord, new string[] { "totalInterest", interestTotal.ToString(), "balanceDue", balance.ToString(), "dueDate8", dueDate, "lastDatePaid8", date});

                        //    if (DailyHistory.isInsurance(cnum))
                        //        G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "dueDate8", dueDate, "lastDatePaid8", date, "creditBalance", creditBalance.ToString() });
                        //    else
                        //        G1.update_db_table(contractsFile, "record", contractRecord, new string[] { "balanceDue", balance.ToString(), "dueDate8", dueDate, "lastDatePaid8", date, "creditBalance", creditBalance.ToString() });
                        //    where = 12;
                    }
                }
                barImport.Value = lastrow;
                string message = "Daily Deposit Data Import of " + lastrow + " Rows Complete . . .";
                MessageBox.Show(message, "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                btnImportFile.Hide();
                btnImportFile.Refresh();
            }
            catch (Exception ex)
            {
                dt.Rows[i]["num"] = "*ERROR*";
                string message = "***ERROR*** Importing Daily Deposits on Row " + i.ToString() + " Where = " + where.ToString() + " " + ex.Message.ToString();
                MessageBox.Show(message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.None,
                     MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);  // MB_TOPMOST
            }
            picLoader.Hide();
            CheckPaidOffContracts(dt);

            bankImportSuccess = false;
            if (bankDt != null)
                bankImportSuccess = true;
        }
        /***************************************************************************************/
        private void CheckCC ( string workContract, string payer, double numMonths  )
        {
            string message = "";
            if (!String.IsNullOrWhiteSpace(payer))
                message = payer + " ";
            else
                message = workContract + " ";

            string ccRecord = "";
            string lookup = workContract;
            if (!String.IsNullOrWhiteSpace(payer))
                lookup = payer;
            string cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + lookup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string str = dt.Rows[0]["expirationDate"].ObjToString();
            if (str.IndexOf("/") < 0)
            {
                message += "*** ERROR *** Expiration Date Appears to be Invalid!";
                Messages.SendTheMessage(LoginForm.username, "cjenkins", "Credit Card Customer", message);
                //MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string[] Lines = str.Split('/');
            if (Lines.Length < 2)
            {
                message += "*** ERROR *** Expiration Date Appears to be Invalid1";
                Messages.SendTheMessage(LoginForm.username, "cjenkins", "Credit Card Customer", message);
                //MessageBox.Show("*** ERROR *** Expiration Date Appears to be Invalid?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            str = Lines[0].Trim();
            int month = str.ObjToInt32();
            if (month <= 0 || month > 12)
            {
                message += "*** ERROR *** Expiration Date Appears To Have An Invalid Month!";
                Messages.SendTheMessage(LoginForm.username, "cjenkins", "Credit Card Customer", message);
                //MessageBox.Show("*** ERROR *** Expiration Date Appears To Have An Invalid Month?", "Expiration Date Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            int year = Lines[1].Trim().ObjToInt32();
            year += 2000;
            int days = DateTime.DaysInMonth(year, month);
            DateTime exDate = new DateTime(year, month, days);
            if (exDate < DateTime.Now)
            {
                message += "*** INFO *** Credit Card Expiration Date Appears to have been met!";
                Messages.SendTheMessage(LoginForm.username, "cjenkins", "Credit Card Customer", message);
                //MessageBox.Show("*** INFO *** Credit Card Expiration Date Appears to have been met!", "Expiration Date Met Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            ccRecord = dt.Rows[0]["record"].ObjToString();

            string numPayments = numMonths.ToString();

            int nPayments = (int)Convert.ToInt64(Convert.ToDouble(numPayments));

            str = dt.Rows[0]["remainingPayments"].ObjToString();
            int remainingPayments = (int)Convert.ToInt64(Convert.ToDouble(str));

            if (remainingPayments != 999)
            {
                remainingPayments -= nPayments;
                if (remainingPayments < 0)
                    remainingPayments = 0;
                if (remainingPayments <= 1)
                {
                    message += "Credit Card Customer Appears To Be Down to Less Than 2 Remaining Payments On Credit Card!";
                    Messages.SendTheMessage(LoginForm.username, "cjenkins", "Credit Card Customer", message);
                    //Messages.SendTheMessage(LoginForm.username, "robby", "Credit Card Customer", message);
                }
                G1.update_db_table("creditcards", "record", ccRecord, new string[] { "remainingPayments", remainingPayments.ToString() });
            }
        }
        /***********************************************************************************************/
        private bool GrabTCA ( string duplicates, ref double trust85P, ref double trust100P )
        {
            //dRow["duplicates"] = "FINALE! FINALE! FINALE! TCA T85/T100 : " + G1.ReformatMoney(trust85P) + " / " + G1.ReformatMoney(trust100P);

            bool result = false;
            trust85P = 0D;
            trust100P = 0D;
            if (duplicates.ToUpper().IndexOf("TCA") < 0)
                return result;
            int idx = duplicates.ToUpper().IndexOf(":");
            duplicates = duplicates.Substring(idx);
            duplicates = duplicates.ToUpper().Replace(":", "").Trim();

            string[] Lines = duplicates.Split('/');
            if (Lines.Length < 2)
                return false;
            trust85P = Lines[0].Trim().ObjToDouble();
            trust100P = Lines[1].Trim().ObjToDouble();
            result = true;

            return result;
        }
        /***********************************************************************************************/
        private void CheckPaidOffContracts ( DataTable dt)
        {
            string found = "";
            string cnum = "";
            string code = "";
            double expected = 0D;
            double balance = 0D;
            DataTable dx = new DataTable();
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("balanceDue", Type.GetType("System.Double"));
            dx.Columns.Add("date");

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                found = dt.Rows[i]["found"].ObjToString();
                if (!String.IsNullOrWhiteSpace(found))
                    continue;
                cnum = dt.Rows[i]["cnum"].ObjToString();
                if (String.IsNullOrWhiteSpace(cnum))
                    continue;
                code = dt.Rows[i]["code"].ObjToString();
                if (code != "01")
                    continue;
                expected = dt.Rows[i]["expected"].ObjToDouble();
                balance = dt.Rows[i]["balance"].ObjToDouble();
//                expected = 5000D;
                if ( balance < expected )
                {
                    DataRow dRow = dx.NewRow();
                    dRow["contractNumber"] = cnum;
                    dRow["balanceDue"] = balance;
                    dRow["date"] = dt.Rows[i]["date"].ObjToString();
                    dx.Rows.Add(dRow);
                }
            }
            //if ( dx.Rows.Count > 0 )
            //{
            //    pleaseForm = new PleaseWait( "Wait, looking for Paid Out Contracts!");
            //    pleaseForm.Show();
            //    PaymentsReport payForm = new PaymentsReport(dx);
            //    payForm.Show();
            //    pleaseForm.FireEvent1();
            //}
            //else
            //{
            //    MessageBox.Show("***Info*** There are no contract within a payment from being paid-off.");
            //}
        }
        /***********************************************************************************************/
        private bool CheckImportFile(DataTable dt)
        {
            if (1 == 1)
                return true;
            string cmd = "Select * from `payments` where `new` = '" + ImportFileDate + "' order by `record` DESC;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return true;
            if (dx.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show("Data Already Exists.\nAre you sure you want OVERWRITE previous Posted Data?", "Import Bank FIle Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return false;
            }
            ResetImportData(dt, dx);
            return true;
        }
        /***********************************************************************************************/
        private void ResetImportData(DataTable dt, DataTable dx)
        {
            string record = "";

            string found = "";
            string cnum = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                found = dt.Rows[i]["found"].ObjToString();
                if (found.ToUpper() == "NO")
                    continue;
                cnum = dx.Rows[i]["contractNumber"].ObjToString();
                record = dx.Rows[i]["record"].ObjToString();
                ManualPayment.ReversePayment(cnum, record, "Reprocessing");
            }
        }
        /***********************************************************************************************/
        private void ReCalculate(DataRow[] dRows)
        {
            for (int i = 0; i < dRows.Length; i++)
            {
                string date = dRows[i]["date"].ObjToString();
                double payment = dRows[i]["payment"].ObjToDouble();
                double principal = 0D;
                double interest = 0D;
                double trust85P = 0D;
                double trust100P = 0D;
                double totalPaid = 0D;
                int days = 0;
                double balance = 0D;
                double unpaid_interest = 0D;

                string issueDate = dRows[i]["issueDate8"].ObjToString();
                double amtOfMonthlyPayment = dRows[i]["amtOfMonthlyPayt"].ObjToDouble();
                double interestTotal = dRows[i]["TotalInterest"].ObjToDouble();

                double oldbalance = dRows[i]["balanceDue"].ObjToDouble();
                double oldTotalInt = dRows[i]["totalInterest"].ObjToDouble();
                double apr = dRows[i]["APR"].ObjToDouble();
                DateTime dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();
                DateTime docp = date.ObjToDateTime();
                CalcPrincipalInterest(oldbalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest );
                balance = oldbalance - principal;
                balance = G1.RoundDown(balance);
                interestTotal = oldTotalInt + interest;
                interestTotal = G1.RoundDown(interestTotal);
                double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                double downPayment = dRows[0]["downPayment"].ObjToDouble();
                double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                double amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                double contractValue = DailyHistory.GetContractValue(dRows[0]);
                CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, apr, ref trust85P, ref trust100P);

                dRows[i]["balance"] = balance;
                dRows[i]["interest"] = interest;
                dRows[i]["principal"] = principal;
                dRows[i]["days"] = days;
                dRows[i]["interestTotal"] = interestTotal;
                dRows[i]["totalPaid"] = totalPaid;
                dRows[i]["trust85P"] = trust85P;
                dRows[i]["trust100P"] = trust100P;
            }
        }
        /***********************************************************************************************/
        private string GetSQLDate(DataTable dt, int row, string columnName)
        {
            string date = dt.Rows[row][columnName].ObjToString();
            string sql_date = G1.date_to_sql(date).Trim();
            if (sql_date == "0001-01-01")
                sql_date = "0000-00-00";
            return sql_date;
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            G1.ShowHideFindPanel(grid);
            //if (grid.OptionsFind.AlwaysVisible == true)
            //    grid.OptionsFind.AlwaysVisible = false;
            //else
            //    grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
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
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else if (dgv4.Visible)
                printableComponentLink1.Component = dgv4;
            else if (dgv5.Visible)
                printableComponentLink1.Component = dgv5;
            else if (dgv6.Visible)
                printableComponentLink1.Component = dgv6;

            //if (dgv.Visible)
            //    G1.AdjustColumnWidths(gridMain, 0.65D, true);
            //else if (dgv2.Visible)
            //    G1.AdjustColumnWidths(gridMain2, 0.65D, true);
            //else if (dgv3.Visible)
            //    G1.AdjustColumnWidths(gridMain3, 0.65D, true);
            //else if (dgv4.Visible)
            //    G1.AdjustColumnWidths(gridMain4, 0.65D, true);
            //else if (dgv5.Visible)
            //    G1.AdjustColumnWidths(gridMain5, 0.65D, true);
            //else if (dgv6.Visible)
            //    G1.AdjustColumnWidths(gridMain6, 0.65D, true);


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

            if (!dgv3.Visible)
                printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            //if (dgv.Visible)
            //    G1.AdjustColumnWidths(gridMain, 0.65D, false );
            //else if (dgv2.Visible)
            //    G1.AdjustColumnWidths(gridMain2, 0.65D, false );
            //else if (dgv3.Visible)
            //    G1.AdjustColumnWidths(gridMain3, 0.65D, false );
            //else if (dgv4.Visible)
            //    G1.AdjustColumnWidths(gridMain4, 0.65D, false );
            //else if (dgv5.Visible)
            //    G1.AdjustColumnWidths(gridMain5, 0.65D, false );
            //else if (dgv6.Visible)
            //    G1.AdjustColumnWidths(gridMain6, 0.65D, false );
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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
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
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            if (!String.IsNullOrWhiteSpace(ImportFileDate))
            {
                if ( G1.validate_date ( ImportFileDate ))
                    Printer.DrawQuad(10, 9, 2, 3, "Import File Date : " + ImportFileDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }

            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "";
            if ( dgv.Visible )
            {
                title = "LKBX Daily Deposit Detail Report";
                if ( workTheFirst )
                    title = "TFBX Daily Deposit Detail Report";
                else if ( workACH )
                    title = "ACH Daily Deposit Detail Report";
                else if (workDraft)
                    title = "ACH Daily Deposit Detail Report";
                else if ( workCC )
                    title = "CC Daily Deposit Detail Report";
                if (chkBreak.Checked)
                    title += " (Exceeded)";
                if (bankDt != null)
                    title = "Bank Credit Card Detail Report";
            }
            if (dgv2.Visible)
            {
                title = "LKBX Trust Payment Report";
                if (workTheFirst)
                    title = "TFBX Trust Payment Report";
                else if (workACH)
                    title = "ACH Trust Payment Report";
                else if (workCC)
                    title = "CC Trust Payment Report";
                if ( bankDt != null )
                    title = "Bank Credit Card Payment Report";
            }
            else if (dgv3.Visible)
            {
                title = "LKBX Payments Received from Provider";
                if (workTheFirst)
                    title = "TFBX Payments Received from Provider";
                else if ( workACH)
                    title = "ACH Payments Received from Provider";
                else if ( workCC)
                    title = "CC Payments Received from Provider";
                if (bankDt != null)
                    title = "Bank Credit Card Payments Received";
            }
            else if (dgv4.Visible)
            {
                title = "LKBX Paid-Up Trust85";
                if (workTheFirst)
                    title = "TFBX Paid-Up Trust85";
                if (workACH)
                    title = "ACH Paid-Up Trust85";
                else if (workCC)
                    title = "CC Paid-Up Trust85";
            }
            else if (dgv5.Visible)
            {
                title = "LKBX After Paid Off";
                if (workTheFirst)
                    title = "TFBX After Paid Off";
                if (workACH)
                    title = "ACH After Paid Off";
                else if (workCC)
                    title = "CC After Paid Off";
            }
            else if (dgv6.Visible)
            {
                title = "LKBX Sec Nat Excess Payments";
                if (workTheFirst)
                    title = "TFBX Sec Nat Excess Payments";
                if (workACH)
                    title = "ACH Sec Nat Excess Payments";
                else if (workCC)
                    title = "CC Sec Nat Excess Payments";
            }
            Printer.DrawQuad(5, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            //            Printer.DrawQuadTicks();
        }
        /***********************************************************************************************/
        private void mainGrid_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string cnum = dr["cnum"].ObjToString();
            if (workDt != null)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                double payments = GetCurrentPayments(dt, cnum);
                DataRow[] dRows = workDt.Select("contractNumber = '" + cnum + "'");
                if (dRows.Length > 0)
                {
                    DailyHistory dailyForm = new DailyHistory(cnum, dr, dRows[0], payments);
                    dailyForm.Show();
                }
                else
                {
                    DailyHistory dailyForm = new DailyHistory(cnum, null, null);
                    dailyForm.Show();
                }
            }
            else
            {
                DailyHistory dailyForm = new DailyHistory(cnum, null, null);
                dailyForm.Show();
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClickx(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["cnum"].ObjToString();
            string code = dr["code"].ObjToString();
            string contractFile = "contracts";
            if (code == "02")
                contractFile = "icontracts";
            if (!String.IsNullOrWhiteSpace(contract))
            {
                DataTable dx = G1.get_db_data("Select * from `" + contractFile + "` where `contractNumber` = '" + contract + "';");
                if (dx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Cannot find contract!");
                    return;
                }
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["cnum"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable) dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string found = dr["found"].ObjToString();
            if ( found.ToUpper() == "L")
            {
                double payments = GetCurrentPayments(dt, contract);
                DailyHistory dailyForm = new DailyHistory(contract, dr, null, payments);
                dailyForm.ShowDialog();
                DataTable dx = null;
                if (DailyHistory.isInsurance(contract))
                    dx = G1.get_db_data("Select * from `icontracts` i JOIN `icustomers` c ON i.`contractNumber` = c.`contractNumber` where i.`contractNumber` = '" + contract + "';");
                else
                    dx = G1.get_db_data("Select * from `contracts` i JOIN `customers` c ON i.`contractNumber` = c.`contractNumber` where i.`contractNumber` = '" + contract + "';");
                if ( dx.Rows.Count > 0 )
                {
                    found = dx.Rows[0]["lapsed"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( found))
                    {
                        dr["found"] = "";
                        dt.Rows[row]["found"] = "";
                        dr["!contractRecord"] = dx.Rows[0]["record"].ObjToString();
                        dt.Rows[row]["!contractRecord"] = dx.Rows[0]["record"].ObjToString();

                        string line = dr["line"].ObjToString();
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(line))
                            {
                                dt.Rows[row]["cnum"] = DecodeContractNumber(line);
                                ProcessPayment(dt, row, line);
                            }
                        }
                        catch ( Exception ex)
                        {
                            MessageBox.Show("***ERROR*** ReProcessing Import Line!", "Reimport Line Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                        dt.AcceptChanges();
                        CalculateGoodBadUgly(dt);
                        CalculatePosted(dt);
                        dgv.RefreshDataSource();
                        dgv.Refresh();
                    }
                }
                return;
            }
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
            {
                cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    contract = ddx.Rows[0]["contractNumber"].ObjToString();
            }
            this.Cursor = Cursors.WaitCursor;
            dt = (DataTable)dgv.DataSource;
            G1.UpdatePreviousCustomer(contract, LoginForm.username);
            bool insurance = false;
            if (contract.ToUpper().IndexOf("ZZ") == 0)
                insurance = true;
            if (contract.ToUpper().IndexOf("MM") == 0)
                insurance = true;
            if (contract.ToUpper().IndexOf("OO") == 0)
                insurance = true;
            if (insurance)
            {
                cmd = "Select * from `policies` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                    this.Cursor = Cursors.Default;
                    return;
                }

                string payer = dr["payer"].ObjToString();
                payer = payer.ToUpper().Replace("INSURANCE", "").Trim();

                string otherPayer = payer;
                if (G1.get_column_number(dt, "otherPayer") >= 0)
                    otherPayer = dr["otherPayer"].ObjToString();
                string payers = "";
                if (otherPayer != payer)
                    payers = "('" + payer + "','" + otherPayer + "')";
                cmd = "Select * from `icustomers` d JOIN `icontracts` c ON d.`contractNumber` = c.`contractNumber` ";
                //                    cmd += " WHERE p.`contractNumber` = '" + contract + "' ";
                if (String.IsNullOrWhiteSpace(payers))
                {
                    if ( !String.IsNullOrWhiteSpace ( contract))
                        cmd += " WHERE d.`contractNumber` = '" + contract + "' ";
                    else
                        cmd += " WHERE d.`payer` = '" + payer + "' ORDER BY d.`contractNumber` DESC";
                }
                else
                    cmd += " WHERE d.`payer` IN " + payers + " ORDER BY d.`contractNumber` DESC";
                cmd += ";";

                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    contract = dx.Rows[0]["contractNumber"].ObjToString();
                    Policies policyForm = new Policies(contract);
                    policyForm.Show();
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
            }
            else
            {
                CalcNewInfo(contract, ddx);
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CalcNewInfo ( string contract, DataTable dx )
        {
            string cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            double balanceDue = 0D;
            double dAPR = dx.Rows[0]["apr"].ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToInt32();
            double monthlyPayment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            DateTime lastDate = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
            double startBalance = dx.Rows[0]["balanceDue"].ObjToDouble();
            DailyHistory.CalculateNewStuff(dt, dAPR, numPayments, startBalance, lastDate);
            DateTime nextDueDate = DailyHistory.getNextDueDate(dt, monthlyPayment, ref balanceDue);
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "FOUND")
            {
                if (e.RowHandle >= 0)
                {
                    string str = e.DisplayText.ToUpper();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        e.Appearance.BackColor = Color.Red;
                        e.Appearance.ForeColor = Color.Yellow;
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DUPLICATES")
            {
                if (e.RowHandle >= 0)
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    string duplicate = dt.Rows[row]["duplicates"].ObjToString();
                    string dup = dt.Rows[row]["duplicate"].ObjToString();
                    if (duplicate.ToUpper().IndexOf("POSSIBLE") >= 0)
                    {
                        if (dup == "Y")
                        {
                            e.Appearance.BackColor = Color.LightGreen;
                            e.Appearance.ForeColor = Color.Black;
                        }
                        else
                        {
                            e.Appearance.BackColor = Color.Red;
                            e.Appearance.ForeColor = Color.Yellow;
                        }
                    }
                    else if ( duplicate.ToUpper().IndexOf ( "FINALE!") >= 0 )
                    {
                        e.Appearance.BackColor = Color.LimeGreen;
                        e.Appearance.ForeColor = Color.Black;
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "TRUST85PAID")
            {
                if (e.RowHandle >= 0)
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    string code = dt.Rows[row]["code"].ObjToString();
                    if (code == "01")
                    {
                        string broke = dt.Rows[row]["empty2"].ObjToString().ToUpper().Trim();
                        if ( broke == "EXCEEDED")
                        {
                            e.Appearance.BackColor = Color.Red;
                            e.Appearance.ForeColor = Color.Yellow;
                        }
                        else if ( broke == "BROKEN")
                        {
                            e.Appearance.BackColor = Color.LightGreen;
                            e.Appearance.ForeColor = Color.Black;
                        }
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "INTEREST")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                double interest = dt.Rows[row]["interest"].ObjToDouble();
                double principal = dt.Rows[row]["principal"].ObjToDouble();
                double payment = dt.Rows[row]["payment"].ObjToDouble();
                if ( payment > 0D && principal == 0D && interest == payment)
                {
                    dt.Rows[row]["empty2"] = "CHECK";
                    e.Appearance.BackColor = Color.Red;
                    e.Appearance.ForeColor = Color.Yellow;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "EMPTY2")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                string data = dt.Rows[row]["empty2"].ObjToString();
                if ( data.ToUpper() == "CHECK")
                {
                    e.Appearance.BackColor = Color.Red;
                    e.Appearance.ForeColor = Color.Yellow;
                }
            }
        }
        /***********************************************************************************************/
        private double GetCurrentPayments ( DataTable dt, string contractNumber)
        {
            double currentPayments = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return currentPayments;
            DataRow[] dRows = dt.Select("cnum='" + contractNumber + "'");
            if (dRows.Length <= 0)
                return currentPayments;
            for ( int i=0; i<dRows.Length; i++)
                currentPayments += dRows[i]["payment"].ObjToDouble();
            return currentPayments;
        }
        /***********************************************************************************************/
        private double GetCurrentTrust85(DataTable dt, string contractNumber)
        {
            double currentPayments = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return currentPayments;
            DataRow[] dRows = dt.Select("cnum='" + contractNumber + "'");
            if (dRows.Length <= 0)
                return currentPayments;
            for (int i = 0; i < dRows.Length; i++)
                currentPayments += dRows[i]["trust85P"].ObjToDouble();
            return currentPayments;
        }
        /***********************************************************************************************/
        private void reProcessPaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["cNum"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string line = dr["line"].ObjToString();
            //ProcessPayment1(dt, row, line);
            ProcessPayment(dt, row, line);
            dt.AcceptChanges();
            CalculateGoodBadUgly(dt);
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private bool ProcessPayment ( DataTable dt, int rowIndex, string line )
        {
            if (G1.get_column_number(dt, "nextDueDate") < 0)
                dt.Columns.Add("nextDueDate");

            if (G1.get_column_number(dt, "duplicate") < 0)
                dt.Columns.Add("duplicate");

            DateTime nextDueDate = DateTime.Now;
            DateTime lastDueDate = new DateTime(1910, 12, 31);
            double creditBalance = 0D;
            bool rtn = true;
            string found = "NO";
            string payer = "";
            double insExpected = 0D;
            bool duplicate = false;
            double originalCreditBalance = 0D;
            bool insurance = false;

            DateTime dolp = DateTime.Now;

            string cnum = "";

            bool editing = false;

            try
            {
                originalCreditBalance = 0D;
                if (line.Length < 36)
                    line = line.PadRight(36);
                string code = line.Substring(0, 2);
                //if (code != "01")
                //    return false;
                string location = line.Substring(2, 2);
                cnum = line.Substring(4, 10);
                if (rowIndex >= 0)
                {
                    cnum = dt.Rows[rowIndex]["cnum"].ObjToString();
                    editing = true;
                }

                int length = line.Length;
                int idx = 0;
                string str = "";
                double expected = 0D;
                double payment = 0D;
                string date = "";

                if (!workTheFirst)
                {
                    str = line.Substring(14, 7);
                    expected = str.ObjToDouble() / 100.0D;
                    expected = G1.RoundValue(expected);
                    str = line.Substring(21, 7);
                    payment = str.ObjToDouble() / 100.0D;
                    payment = G1.RoundValue(payment);
                    date = line.Substring(28);
                }
                else
                {
                    idx = length - 8 - 7 - 7;
                    str = line.Substring(idx, 7);
                    expected = str.ObjToDouble() / 100.0D;
                    expected = G1.RoundValue(expected);
                    idx = length - 8 - 7;
                    str = line.Substring(idx, 7);
                    payment = str.ObjToDouble() / 100.0D;
                    payment = G1.RoundValue(payment);
                    idx = length - 8;
                    date = line.Substring(idx);
                }
                DateTime rdate = date.ObjToDateTime();
                DateTime docp = date.ObjToDateTime();
                cnum = cnum.TrimStart('0');
                cnum = cnum.TrimStart('o');
                cnum = cnum.TrimStart('O');
                //if ( code == "01")
                //    cnum = cnum.TrimEnd('0');
                string testContract = txtContract.Text;
                if (!String.IsNullOrWhiteSpace(testContract))
                {
                    if (cnum.ToUpper() != testContract.ToUpper())
                        return false;
                }
                if (cnum == "L18096LI")
                {
                }
                string paymentsFile = "payments";
                if (code == "99")
                    expected = 0.0D;
                string cmd = "";
                found = "";
                DataRow[] dRows = null;
                DataRow dRow = null;
                if (code == "02")
                {
                    insurance = true;
                    dRows = workDt.Select("contractNumber = '" + cnum + "'");
                    string newPayer = "";
                    cnum = cnum.TrimStart('0');
                    payer = cnum;
                    paymentsFile = "ipayments";
                    bool isLapsed = false;
                    cnum = FindPayerContract(payer, payment.ObjToString(), ref newPayer, ref insExpected, ref isLapsed);
                    if (expected == 0D && insExpected > 0D)
                        expected = insExpected;
                    if (isLapsed)
                        found = "L";
                    //cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    //DataTable ddx = G1.get_db_data(cmd);
                    //if (ddx.Rows.Count > 0)
                    if (!String.IsNullOrWhiteSpace(cnum))
                    {
                        if (!String.IsNullOrWhiteSpace(newPayer))
                            payer = newPayer;
                        found = "";
                        //                        cnum = ddx.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            str = ddx.Rows[0]["lapsed"].ObjToString();
                            if (str.ToUpper() == "Y")
                            {
                                found = "L";
                            }
                        }
                        if (isLapsed)
                            found = "L";
                    }
                    else
                    {
                        found = "NO";
                        cnum = payer;
                    }
                    if (rowIndex >= 0)
                        dRow = dt.Rows[rowIndex];
                    else
                        dRow = dt.NewRow();
                }
                else
                {
                    dRows = workDt.Select("contractNumber = '" + cnum + "'");

                    if (rowIndex >= 0)
                    {
                        cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` where p.`contractNumber` = '" + cnum + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            dRows = ddx.Select("contractNumber = '" + cnum + "'");
                            originalCreditBalance = ddx.Rows[0]["creditBalance"].ObjToDouble();
                            lastDueDate = ddx.Rows[0]["dueDate8"].ObjToDateTime();
                        }
                    }

                    if (rowIndex >= 0)
                        dRow = dt.Rows[rowIndex];
                    else
                        dRow = dt.NewRow();
                }

//                DataRow dRow = dt.NewRow();
                dRow["code"] = code;
                dRow["locationcode"] = location;
                dRow["cnum"] = cnum;
                dRow["expected"] = expected;
                dRow["payment"] = payment;
                dRow["date"] = date.ObjToDateTime().ToString("MM/dd/yyyy");
                dRow["oldCreditBalance"] = originalCreditBalance;
                string agent = "";
                string fname = "";
                string lname = "";
                string firstPayDate = "";
                string contractDate = "";
                string issueDate = "";
                double balance = 0D;
                double oldbalance = 0D;
                double oldTotalInt = 0D;
                double oldOldBalance = 0D;
                double interest = 0D;
                double apr = 0D;
                double interestTotal = 0D;
                double amtOfMonthlyPayment = 0D;
                string contractRecord = "";
                double principal = 0D;
                double totalPaid = 0D;
                double trust85P = 0D;
                double trust100P = 0D;
                double pExpected = 0D;
                double months = 0D;
                double unpaid_interest = 0D;
                double contractValue = 0D;
                double prevTrust85P = 0D;
                int numPayments = 0;
                double previousMonths = 0D;
                double actualMonthPaid = 0D;
                double dueDatePayment = 0D;
                DateTime oldDueDate = DateTime.Now;
                DateTime currentDueDate = DateTime.Now;
                bool didIt = false;
                int days = 0;
                if (dRows == null)
                {
                    dRow["line"] = line;
                    dRow["found"] = "NO";
                    if (!String.IsNullOrWhiteSpace(payer))
                        dRow["name"] = "BAD PAYER NUMBER (" + payer + ")";
                    else
                        dRow["name"] = "BAD CONTRACT NUMBER (" + cnum + ")";
                    dt.Rows.Add(dRow);
                    return rtn;
                }
                if (dRows.Length > 0)
                {
                    agent = dRows[0]["agentCode"].ObjToString();
                    fname = dRows[0]["firstName"].ObjToString();
                    lname = dRows[0]["lastName"].ObjToString();
                    firstPayDate = dRows[0]["firstPayDate"].ObjToString();
                    contractDate = dRows[0]["contractDate"].ObjToString();
                    issueDate = dRows[0]["issueDate8"].ObjToString();
                    amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    if (DailyHistory.isInsurance(cnum))
                    {
                        trust85P = 0D;
                        trust100P = 0D;
                        amtOfMonthlyPayment = Policies.CalcMonthlyPremium(payer, docp);
                    }
                    if (expected <= 0D && amtOfMonthlyPayment > 0D)
                    {
                        expected = amtOfMonthlyPayment;
                        dRow["expected"] = expected;
                    }
                    interestTotal = dRows[0]["TotalInterest"].ObjToDouble();
                    contractRecord = dRows[0]["record1"].ObjToString();

                    oldbalance = dRows[0]["balanceDue"].ObjToDouble();
                    oldTotalInt = dRows[0]["totalInterest"].ObjToDouble(); // Process Payment Line

                    if (FindPreviousPayments(dt, cnum, ref previousMonths))
                        originalCreditBalance = 0D;

                    double oldCreditBalance = FindPrevCreditBalance(dt, cnum, 0D); // Process Line
                    oldCreditBalance = dRow["oldCreditBalance"].ObjToDouble();
                    actualMonthPaid = payment / amtOfMonthlyPayment;
                    dueDatePayment = payment + oldCreditBalance + originalCreditBalance;
                    double newActualMonthPaid = dueDatePayment / amtOfMonthlyPayment;
                    if ((oldCreditBalance + originalCreditBalance + (payment % amtOfMonthlyPayment)) > amtOfMonthlyPayment)
                        actualMonthPaid += 1D;
                    //if (!insurance)
                    //    payment = dueDatePayment;

                    oldDueDate = dRows[0]["dueDate8"].ObjToDateTime();
                    currentDueDate = oldDueDate;

                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);

                    didIt = true;

                    originalCreditBalance = 0D;
                    actualMonthPaid += 0.00000000001D;
                    months = actualMonthPaid;

                    dRow["lastDatePaid8"] = dRows[0]["lastDatePaid8"].ObjToString();
                    dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();

                    oldTotalInt = GetLastInterestPaid(cnum, payer, ref dolp, ref pExpected);

                    apr = dRows[0]["APR"].ObjToDouble();
                    dRow["dueDate"] = dRows[0]["dueDate8"].ObjToString();
                    dRow["dueDate"] = oldDueDate.ToString("MM/dd/yyyy");

                    //                dolp = new DateTime(2018, 4, 19);
                    if (editing)
                    {
                        dolp = dRow["lastDatePaid8"].ObjToDateTime();
                    }
                    else
                        duplicate = FindDOLP(dt, cnum, ref dolp, ref oldOldBalance, ref lastDueDate, ref prevTrust85P ); // Don't use this oldbalance
                    if (oldOldBalance > 0D)
                        oldbalance = oldOldBalance;
                    if (dolp.Year < 1975)
                        dolp = issueDate.ObjToDateTime();
                    if (dolp.Year < 1300)
                        dolp = new DateTime(2050, 12, 31);

                    try
                    {
                        if (dolp > docp)
                        {
                            cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' ORDER By `payDate8` DESC LIMIT 5";
                            DataTable dddx = G1.get_db_data(cmd);
                            if (dddx.Rows.Count > 0)
                            {
                                ManualPayment.CleanupWork(dddx);
                                for (int k = 0; k < dddx.Rows.Count; k++)
                                {
                                    DateTime d = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                    //                                if (d >= dolp)
                                    if (d.Year > 1850)
                                    {
                                        dolp = dddx.Rows[k]["payDate8"].ObjToDateTime();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    dRow["lastDatePaid8"] = dolp.ToString("MM/dd/yyyy");

                    //                DateTime docp = date.ObjToDateTime();
                    CalcPrincipalInterest(oldbalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest );
                    balance = oldbalance - principal;
                    balance = G1.RoundDown(balance);
                    interestTotal = oldTotalInt + interest;
                    interestTotal = G1.RoundDown(interestTotal);
                    double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                    double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                    double downPayment = dRows[0]["downPayment"].ObjToDouble();
                    if (downPayment == 0D)
                        downPayment = DailyHistory.GetDownPaymentFromPayments(cnum);
                    double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                    double amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    contractValue = DailyHistory.GetContractValue(dRows[0]);

                    HandleUnpaidInterest(cnum, payment, ref interest, ref unpaid_interest, ref principal, ref balance);

                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + cnum + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    DataTable dx = G1.get_db_data(cmd);
                    double financeBalance = DailyHistory.GetFinanceValue(dRows[0]);
                    numPayments = (int)financeDays;
                    //DailyHistory.CalculateNewStuff(dx, apr, numPayments, financeBalance, issueDate.ObjToDateTime());
                    //if (dx.Rows.Count > 0)
                    //{
                    //    creditBalance = dx.Rows[0]["runningCB"].ObjToDouble();
                    //}

                    CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, apr, ref trust85P, ref trust100P);
                    if (days <= 0)
                    {
                        dRow["duplicates"] = "POSSIBLE DUPLICATE"; // Needs testing before releasing
                        //DataRow[] dR = dt.Select("cnum='" + cnum + "'");
                        //if (dR.Length >= 1)
                        //{
                        //    DateTime ddlp = dR[0]["date"].ObjToDateTime();
                        //    dRow["duplicates"] = "";
                        //}
                    }
                }
                else
                {
                    cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        str = ddx.Rows[0]["lapsed"].ObjToString();
                        if (str.ToUpper() == "Y")
                        {
                            found = "L";
                        }
                    }
                }

                //if (lastDueDate.Year > 1910)
                //    docp = lastDueDate;

                if (!didIt)
                {
                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);
                    if (creditBalance > 0D)
                    {
                    }
                }
                if (lastDueDate.Year > 1910)
                {
                    if (!DailyHistory.isInsurance(cnum))
                    {
                        lastDueDate = FindMismatches.VerifyDueDate(cnum);
                        previousMonths = 0D;
                        if (FindPreviousPayments(dt, cnum, ref previousMonths))
                            months += previousMonths;
                    }

                    nextDueDate = lastDueDate.AddMonths(Convert.ToInt32(Math.Truncate(months)));
                }

                dRow["found"] = found;
                dRow["agent"] = agent;
                dRow["fname"] = fname;
                dRow["lname"] = lname;
                if (!String.IsNullOrWhiteSpace(payer))
                    dRow["name"] = fname + " " + lname + " (" + payer + ")";
                else
                    dRow["name"] = fname + " " + lname;
                if (DailyHistory.isInsurance(cnum))
                {
                    trust85P = 0D;
                    trust100P = 0D;
                    balance = Policies.CalcMonthlyPremium(payer, docp );
                }
                if (duplicate)
                    dRow["duplicate"] = "Y";
                dRow["firstPayDate"] = firstPayDate;
                dRow["contractDate"] = contractDate;
                dRow["balance"] = balance;
                dRow["oldbalance"] = oldbalance;
                dRow["oldTotalInt"] = oldTotalInt;
                dRow["interest"] = interest;
                dRow["principal"] = principal;
                dRow["days"] = days;
                dRow["apr"] = (apr / 100.0D);
                dRow["!contractRecord"] = contractRecord;
                dRow["amtOfMonthlyPayt"] = amtOfMonthlyPayment;
                dRow["interestTotal"] = interestTotal;
                dRow["totalPaid"] = totalPaid;
                dRow["issueDate"] = issueDate;
                dRow["trust85P"] = trust85P;
                dRow["trust100P"] = trust100P;
                dRow["retained"] = 0D;
                dRow["creditBalance"] = creditBalance;
                dRow["payer"] = payer;
                dRow["line"] = line;
                dRow["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                if (!DailyHistory.isInsurance(cnum) && balance <= LoginForm.minimumForceBalance && LoginForm.CalculateForcedPayoff)
                {
                    //newTrust85 = 0D;
                    //if (duplicate && dolp == docp)
                    //{
                    //    FindTrust85(dt, cnum, ref newTrust85);
                    //}
                    //bool isPaid = false;
                    //if (!cnum.Contains("LI"))
                    //    isPaid = Customers.CheckForcedPayoff(cnum, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, trust85P + newTrust85 );
                    //if (isPaid)
                    //{
                    //    double Trust85Paid = totalTrust85;
                    //    double difference = maxTrust85 - totalTrust85;
                    //    difference = G1.RoundValue(difference);

                    //    dRow["difference"] = difference;

                    //    dRow["duplicates"] = "FINALE! FINALE! FINALE!";
                    //    dRow["nextDueDate"] = "12/31/2039";
                    //    trust85P = trust85P + difference;
                    //    if (trust85P <= 0D)
                    //        trust85P = 0D;
                    //    dRow["trust85P"] = trust85P;
                    //    trust100P = trust85P / 0.85D;
                    //    trust100P = G1.RoundValue(trust100P);
                    //    dRow["trust100P"] = trust100P;
                    //}
                    string payoff = processNewPayOff(dt, dRow, cnum, duplicate, dolp, docp, amtOfMonthlyPayment, numPayments, contractValue, trust85P, trust100P, balance );
                }
                if (balance <= 0D)
                {
                    if (paymentsFile != "ipayments")
                        dRow["nextDueDate"] = "12/31/2039";
                }
                if (rowIndex < 0)
                    dt.Rows.Add(dRow);
                else
                    dgv.RefreshDataSource();

                CalculatePosted(dt);
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
            catch (Exception ex)
            {
                int thisRow = dt.Rows.Count;
                MessageBox.Show("***ERROR*** (" + cnum + " " + thisRow.ToString() + ") " + line + " " + ex.Message.ToString());
            }
            return rtn;
        }
        /***********************************************************************************************/
        private bool ProcessPayment1(DataTable dt, int rowIndex, string line)
        {
            bool rtn = true;
            try
            {
                if (line.Length < 36)
                    line = line.PadRight(36);
                string code = line.Substring(0, 2);
                if (code != "01")
                    return false;
                string location = line.Substring(2, 2);
                string cnum = line.Substring(4, 10);
                if (rowIndex >= 0)
                    cnum = dt.Rows[rowIndex]["cnum"].ObjToString();
                string str = line.Substring(14, 7);
                if (!G1.validate_numeric(str))
                    rtn = false;
                double expected = str.ObjToDouble() / 100.0D;
                expected = G1.RoundValue(expected);
                str = line.Substring(21, 7);
                if (!G1.validate_numeric(str))
                    rtn = false;
                double payment = str.ObjToDouble() / 100.0D;
                payment = G1.RoundValue(payment);
                string date = line.Substring(28);
                DateTime rdate = date.ObjToDateTime();
                cnum = cnum.TrimStart('0');
                if (code == "99")
                    expected = 0.0D;
                DataRow[] dRows = workDt.Select("contractNumber = '" + cnum + "'");
                if (rowIndex >= 0)
                {
                    string cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` where p.`contractNumber` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                        dRows = ddx.Select("contractNumber = '" + cnum + "'");
                }

                DataRow dRow = null;
                if (rowIndex >= 0)
                    dRow = dt.Rows[rowIndex];
                else
                    dRow = dt.NewRow();
                dRow["code"] = code;
                dRow["locationcode"] = location;
                dRow["cnum"] = cnum;
                dRow["expected"] = expected;
                dRow["payment"] = payment;
                dRow["date"] = date;
                string agent = "";
                string fname = "";
                string lname = "";
                string firstPayDate = "";
                string contractDate = "";
                string issueDate = "";
                string found = "NO";
                double balance = 0D;
                double oldbalance = 0D;
                double oldTotalInt = 0D;
                double interest = 0D;
                double apr = 0D;
                double interestTotal = 0D;
                double amtOfMonthlyPayment = 0D;
                string contractRecord = "";
                double principal = 0D;
                double totalPaid = 0D;
                double trust85P = 0D;
                double trust100P = 0D;
                DateTime currentDueDate = DateTime.Now;
                int days = 0;
                double unpaid_interest = 0D;
                if (dRows.Length > 0)
                {
                    found = "";
                    agent = dRows[0]["agentCode"].ObjToString();
                    fname = dRows[0]["firstName"].ObjToString();
                    lname = dRows[0]["lastName"].ObjToString();
                    firstPayDate = dRows[0]["firstPayDate"].ObjToString();
                    contractDate = dRows[0]["contractDate"].ObjToString();
                    issueDate = dRows[0]["issueDate8"].ObjToString();
                    amtOfMonthlyPayment = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    interestTotal = dRows[0]["TotalInterest"].ObjToDouble();
                    contractRecord = dRows[0]["record1"].ObjToString();


                    oldbalance = dRows[0]["balanceDue"].ObjToDouble();
                    oldTotalInt = dRows[0]["totalInterest"].ObjToDouble();

                    DateTime docp = date.ObjToDateTime();
                    DateTime nextDueDate = DateTime.Now;
                    double creditBalance = 0D;
                    double months = 0D;

                    DailyHistory.ReCalculateDueDate(cnum, docp, amtOfMonthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldbalance, ref currentDueDate);

                    apr = dRows[0]["APR"].ObjToDouble();
                    dRow["dueDate"] = dRows[0]["dueDate8"].ObjToString();
                    dRow["dueDate"] = currentDueDate.ToString("MM/dd/yyyy");
                    dRow["lastDatePaid8"] = dRows[0]["lastDatePaid8"].ObjToString();
                    dRow["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                    DateTime dolp = dRows[0]["lastDatePaid8"].ObjToDateTime();
                    CalcPrincipalInterest(oldbalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest );
                    balance = oldbalance - principal;
                    balance = G1.RoundDown(balance);
                    interestTotal = oldTotalInt + interest;
                    interestTotal = G1.RoundDown(interestTotal);
                    double serviceAmt = dRows[0]["serviceTotal"].ObjToDouble();
                    double merchandiseAmt = dRows[0]["merchandiseTotal"].ObjToDouble();
                    double downPayment = dRows[0]["downPayment"].ObjToDouble();
                    if (downPayment == 0D)
                        downPayment = DailyHistory.GetDownPaymentFromPayments(cnum);
                    double financeDays = dRows[0]["numberOfPayments"].ObjToDouble();
                    double amtOfMonthlyPayt = dRows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    double contractValue = DailyHistory.GetContractValue(dRows[0]);
                    CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, apr, ref trust85P, ref trust100P);
                }
                else
                {
                    string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        str = ddx.Rows[0]["lapsed"].ObjToString();
                        if (str.ToUpper() == "Y")
                        {
                            found = "L";
                        }
                    }
                }
                dRow["found"] = found;
                dRow["agent"] = agent;
                dRow["fname"] = fname;
                dRow["lname"] = lname;
                dRow["name"] = fname + " " + lname;
                dRow["firstPayDate"] = firstPayDate;
                dRow["contractDate"] = contractDate;
                dRow["balance"] = balance;
                dRow["oldbalance"] = oldbalance;
                dRow["oldTotalInt"] = oldTotalInt;
                dRow["interest"] = interest;
                dRow["principal"] = principal;
                dRow["days"] = days;
                dRow["apr"] = (apr / 100.0D);
                dRow["!contractRecord"] = contractRecord;
                dRow["amtOfMonthlyPayt"] = amtOfMonthlyPayment;
                dRow["interestTotal"] = interestTotal;
                dRow["totalPaid"] = totalPaid;
                dRow["issueDate"] = issueDate;
                dRow["trust85P"] = trust85P;
                dRow["trust100P"] = trust100P;
                dRow["line"] = line;
                if (rowIndex < 0)
                    dt.Rows.Add(dRow);

            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Processing Deposit " + ex.Message.ToString());
            }
            return rtn;
        }
        /***********************************************************************************************/
        private void checkInterestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            string cnum = dr["cnum"].ObjToString();
            double balance = dr["oldBalance"].ObjToDouble();

            double apr = dr["apr"].ObjToDouble();
            DateTime fromDate = dr["lastDatePaid8"].ObjToDateTime();
            DateTime toDate = dr["date"].ObjToDateTime();

            CheckInterest checkForm = new CheckInterest(apr, fromDate, toDate, balance);
            checkForm.Show();
        }
        /***********************************************************************************************/
        private void sendMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            string subject = "Message from Daily Deposits";

            string cnum = dr["cnum"].ObjToString();
            double payment = dr["payment"].ObjToDouble();
            string message = "Contract (" + cnum + ") has made a payment of $" + G1.ReformatMoney(payment) + ".\n";

            string cmd = "Select * from `customers` where `contractNumber` = '" + cnum + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string name = dx.Rows[0]["firstName"].ObjToString() + " " + dx.Rows[0]["lastName"].ObjToString();
                message = "Customer " + name + " (" + cnum + ") has made a payment of $" + G1.ReformatMoney(payment) + ".\n\n";
            }

            string paymentsFile = "payments";
            if (cnum.ToUpper().IndexOf("ZZ") == 0)
                paymentsFile = "ipayments";

            cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                double balance = dx.Rows[0]["balanceDue"].ObjToDouble();
                DateTime fromDate = dx.Rows[0]["lastDatePaid8"].ObjToDateTime();
                DateTime lapseDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
                string lapse = dx.Rows[0]["lapsed"].ObjToString().ToUpper();
                DateTime toDate = dr["date"].ObjToDateTime();
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
        /***********************************************************************************************/
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["cnum"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to Clear Lapsed for customer (" + contractNumber + ") ?", "Clear Lapsed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;

            DailyHistory.ClearLapsed(contractNumber);
        }
        /***********************************************************************************************/
        //private void eliminatePaymentToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (!LoginForm.administrator)
        //    {
        //        MessageBox.Show("***ERROR*** You do not have permission to remove this payment!");
        //        return;
        //    }
        //    DataTable dx = (DataTable)dgv.DataSource;
        //    int rowHandle = gridMain.FocusedRowHandle;
        //    int row = gridMain.GetDataSourceRowIndex(rowHandle);

        //    DataRow dr = gridMain.GetFocusedDataRow();

        //    string contractNumber = dx.Rows[row]["cnum"].ObjToString();
        //    string paymentFile = "payments";
        //    if (contractNumber.ToUpper().IndexOf("ZZ") == 0)
        //        paymentFile = "ipayments";

        //    DialogResult result = MessageBox.Show("Are you sure you want to Eliminate Payment for Customer (" + contractNumber + ") ?", "Eliminate Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
        //    if (result == DialogResult.No)
        //        return;
        //    DateTime dateNow = DateTime.Now;
        //    string depositNumber = "T" + dateNow.Year.ToString("D4") + dateNow.Month.ToString("D2") + dateNow.Day.ToString("D2");
        //    if (workACH)
        //        depositNumber = "A" + dateNow.Year.ToString("D4") + dateNow.Month.ToString("D2") + dateNow.Day.ToString("D2");
        //    string cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + contractNumber + "' and `depositNumber` = '" + depositNumber + "';";

        //    DateTime date = dr["date"].ObjToDateTime();
        //    string myDate = date.ToString("yyyy-MM-dd");
        //    double payment = dr["payment"].ObjToDouble();

        //    cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + contractNumber + "' and `payDate8` = '" + myDate + "' and `paymentAmount` = '" + payment.ToString() + "' order by `tmstamp` DESC;";
        //    DataTable dt = G1.get_db_data(cmd);
        //    if (dt.Rows.Count <= 0)
        //    {
        //        //                MessageBox.Show("***ERROR*** Cannot find Payment for Deposit Number " + depositNumber + "!");
        //        MessageBox.Show("***ERROR*** Cannot find Payment for " + payment.ToString() + " for Date " + myDate + "!");
        //        return;
        //    }
        //    string record = dt.Rows[0]["record"].ObjToString();
        //    string oldBalance = dt.Rows[0]["oldBalance"].ObjToString();
        //    string oldDueDate8 = dt.Rows[0]["oldDueDate8"].ObjToString();
        //    string oldDOLP = dt.Rows[0]["oldDOLP"].ObjToString();
        //    G1.delete_db_table("payments", "record", record);

        //    cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
        //    dt = G1.get_db_data(cmd);
        //    if (dt.Rows.Count > 0)
        //    {
        //        record = dt.Rows[0]["record"].ObjToString();
        //        G1.update_db_table("contracts", "record", record, new string[] { "balanceDue", oldBalance, "dueDate8", oldDueDate8, "lastDatePaid8", oldDOLP });
        //        dx.Rows.RemoveAt(row);
        //        dgv.DataSource = dx;
        //        gridMain.RefreshData();
        //        dgv.Refresh();
        //    }
        //}
        /***********************************************************************************************/
        private void chkGroupData_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                if (chkGroupData.Checked)
                {
                    gridMain.Columns["code"].GroupIndex = 0;
                    gridMain.Columns["locationcode"].GroupIndex = 1;
                    gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                    gridMain.ExpandAllGroups();
                    gridMain.OptionsPrint.ExpandAllGroups = true;
                    gridMain.OptionsPrint.PrintGroupFooter = true;
                }
                else
                {
                    gridMain.Columns["code"].GroupIndex = -1;
                    gridMain.Columns["locationcode"].GroupIndex = -1;
                    gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                    gridMain.CollapseAllGroups();
                    gridMain.OptionsPrint.ExpandAllGroups = false;
                    gridMain.OptionsPrint.PrintGroupFooter = true;
                }
                gridMain.RefreshData();
                dgv.Refresh();
            }
            else if (dgv2.Visible)
            {
                if (chkGroupData.Checked)
                {
                    gridMain2.Columns["code"].GroupIndex = 0;
                    gridMain2.Columns["locationcode"].GroupIndex = 1;
                    gridMain2.OptionsBehavior.AutoExpandAllGroups = true;
                    gridMain2.ExpandAllGroups();
                    gridMain2.OptionsPrint.ExpandAllGroups = true;
                    gridMain2.OptionsPrint.PrintGroupFooter = true;
                }
                else
                {
                    gridMain2.Columns["code"].GroupIndex = -1;
                    gridMain2.Columns["locationcode"].GroupIndex = -1;
                    gridMain2.OptionsBehavior.AutoExpandAllGroups = false;
                    gridMain2.CollapseAllGroups();
                    gridMain2.OptionsPrint.ExpandAllGroups = false;
                    gridMain2.OptionsPrint.PrintGroupFooter = true;
                }
                gridMain2.RefreshData();
                dgv2.Refresh();
            }
            else if (dgv3.Visible)
            {
                if (chkGroupData.Checked)
                {
                    gridMain3.Columns["code"].GroupIndex = 0;
                    gridMain3.Columns["locationcode"].GroupIndex = 1;
                    gridMain3.OptionsBehavior.AutoExpandAllGroups = true;
                    gridMain3.ExpandAllGroups();
                    gridMain3.OptionsPrint.ExpandAllGroups = true;
                    gridMain3.OptionsPrint.PrintGroupFooter = true;
                }
                else
                {
                    gridMain3.Columns["code"].GroupIndex = -1;
                    gridMain3.Columns["locationcode"].GroupIndex = -1;
                    gridMain3.OptionsBehavior.AutoExpandAllGroups = false;
                    gridMain3.CollapseAllGroups();
                    gridMain3.OptionsPrint.ExpandAllGroups = false;
                    gridMain3.OptionsPrint.PrintGroupFooter = true;
                }
                gridMain3.RefreshData();
                dgv3.Refresh();
            }
        }
        /***********************************************************************************************/
        private bool FindBestDateInfo(string payer, ref DateTime dueDate, ref DateTime lastPaidDate)
        {
            string contractNumber = "";
            string cmd = "";
            payer = payer.ToUpper().Replace("NEW", "");
            payer = payer.ToUpper().Replace("INSURANCE", "");
            payer = payer.Trim();
            string newPayer = payer;
            DataTable ddx = null;
            string[] Lines = null;
            if (payer.IndexOf('/') > 0)
                Lines = payer.Split('/');
            else if (payer.IndexOf(',') > 0)
                Lines = payer.Split(',');
            else
                Lines = payer.Split('/');
            for (int i = 0; i < Lines.Length; i++)
            {
                payer = Lines[i].Trim();
                newPayer = "0" + payer;
                cmd = "Select * from `icustomers` where `payer` = '" + payer + "' ORDER BY `contractNumber` DESC;";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    newPayer = payer;
                    cmd = "Select * from `icustomers` where `payer` = '0" + payer + "' ORDER BY `contractNumber` DESC;";
                    ddx = G1.get_db_data(cmd);
                }
                if (ddx.Rows.Count <= 0)
                {
                    int idx = payer.Length;
                    string lastChar = payer.Substring(idx - 1, 1);
                    char ch = (char)lastChar[0];
                    if (char.IsLetter(ch))
                    {
                        newPayer = payer.TrimEnd(ch);
                        cmd = "Select * from `icustomers` where `payer` = '" + newPayer + "' ORDER BY `contractNumber` DESC;";
                        ddx = G1.get_db_data(cmd);
                    }
                }
                if (ddx.Rows.Count > 0)
                    break;
            }
            if (ddx.Rows.Count <= 0)
                return false;
            bool found = false;
            if (ddx.Rows.Count > 0)
            {
                DataTable ddd = null;
                string originalContractNumber = contractNumber;
                DateTime date = DateTime.Now;
                for (int i = 0; i < ddx.Rows.Count; i++)
                {
                    contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `icontracts` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + contractNumber + "';";
                    ddd = G1.get_db_data(cmd);
                    if (ddd.Rows.Count > 0)
                    {
                        date = ddd.Rows[0]["dueDate8"].ObjToDateTime();
                        if (date.Year > 1800)
                        {
                            dueDate = date;
                            lastPaidDate = ddd.Rows[0]["lastDatePaid8"].ObjToDateTime();
                            found = true;
                            break;
                        }
                    }
                }
            }
            return found;
        }
        /***********************************************************************************************/
        public static string FindPayerContract(string payer, string payment, ref string newPayer, ref double expected, ref bool isLapsed )
        {
            string contractNumber = "";
            if ( payer == "147243")
            {
            }
            string cmd = "";
            payer = payer.ToUpper().Replace("NEW", "");
            payer = payer.ToUpper().Replace("INSURANCE", "");
            payer = payer.Trim();
            isLapsed = false;
            DataTable ddx = null;
            ddx = G1.get_db_data("Select * from `payers` WHERE `payer` = '" + payer + "';");
            if (ddx.Rows.Count > 0)
            {
                newPayer = payer;
                contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
                expected = ddx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                string lapsed = ddx.Rows[0]["lapsed"].ObjToString();
                DateTime lapseDate = ddx.Rows[0]["lapseDate8"].ObjToDateTime();
                DateTime reinstateDate = ddx.Rows[0]["reinstateDate8"].ObjToDateTime();
                DateTime dueDate = ddx.Rows[0]["dueDate8"].ObjToDateTime();
                if (lapsed == "Y" || lapseDate.Year > 500)
                {
                    if (lapseDate > reinstateDate)
                        isLapsed = true;
                    else if (lapsed == "Y")
                        isLapsed = true;
                    if (lapseDate.Year > 500 && reinstateDate.Year > 500)
                    {
                        if (reinstateDate > lapseDate)
                            isLapsed = false;
                    }
                }
                return contractNumber;
            }

            cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` where `payer` = '" + payer + "' ORDER BY p.`contractNumber` DESC;";
            ddx = G1.get_db_data(cmd);
            //if (ddx.Rows.Count <= 0)
            //{
            //    int idx = payer.Length;
            //    string lastChar = payer.Substring(idx - 1, 1);
            //    char ch = (char)lastChar[0];
            //    if (char.IsLetter(ch))
            //    {
            //        newPayer = payer.TrimEnd(ch);
            //        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` where `payer` = '" + newPayer + "' ORDER BY p.`contractNumber` DESC;";
            //        ddx = G1.get_db_data(cmd);
            //    }
            //}
            DateTime deceasedDate = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;
            isLapsed = false;
            contractNumber = "";
            string contract = "";
            if (ddx.Rows.Count > 0)
            {
                string lapsed = "";
                double lastExpected = ddx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                for (int i = 0; i < ddx.Rows.Count; i++)
                {
                    deceasedDate = ddx.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 100)
                        continue;
                    contract = ddx.Rows[i]["contractNumber"].ObjToString();
                    if (contract.ToUpper().IndexOf("MM") >= 0)
                        continue;
                    if (contract.ToUpper().IndexOf("OO") >= 0)
                        continue;
                    dueDate8 = ddx.Rows[i]["dueDate8"].ObjToDateTime();
                    if (dueDate8.Year < 100)
                        continue;
                    contractNumber = contract;
                    lastExpected = ddx.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();

                    lapsed = ddx.Rows[i]["lapsed"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(lapsed))
                        isLapsed = true;
                    else
                    {
                        isLapsed = false;
                        break;
                    }
                }
                if (ddx.Rows.Count > 1)
                {
                    DataTable ddd = null;
                    bool found = false;
                    string premium = "";
                    string originalContractNumber = contractNumber;
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                        if (originalContractNumber.ToUpper().IndexOf("OO") >= 0 && contractNumber.ToUpper().IndexOf("ZZ") >= 0)
                            originalContractNumber = contractNumber;
                        if (originalContractNumber.ToUpper().IndexOf("MM") >= 0 && contractNumber.ToUpper().IndexOf("ZZ") >= 0)
                            originalContractNumber = contractNumber;
                        deceasedDate = ddx.Rows[i]["deceasedDate"].ObjToDateTime();
                        if (deceasedDate.Year > 100)
                            continue;
                        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + contractNumber + "';";
                        ddd = G1.get_db_data(cmd);
                        if (ddd.Rows.Count > 0)
                        {
                            premium = ddd.Rows[0]["amtOfMonthlyPayt"].ObjToString();
                            if ( premium.ObjToDouble() > 0D)
                                lastExpected = premium.ObjToDouble();
                            if (premium.ObjToDouble() == payment.ObjToDouble())
                            {
                                expected = premium.ObjToDouble();
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        contractNumber = originalContractNumber;
                        expected = lastExpected;
                    }
                }
                else
                {
                    expected = ddx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                }
            }
            return contractNumber;
        }
        /***********************************************************************************************/
        private string FindPayerContractx(string payer, string payment, ref string newPayer)
        {
            string contractNumber = "";
            string cmd = "";
            payer = payer.ToUpper().Replace("NEW", "");
            payer = payer.ToUpper().Replace("INSURANCE", "");
            payer = payer.Trim();
            newPayer = payer;
            DataTable ddx = null;
            string[] Lines = null;
            if (payer.IndexOf('/') > 0)
                Lines = payer.Split('/');
            else if (payer.IndexOf(',') > 0)
                Lines = payer.Split(',');
            else
                Lines = payer.Split('/');
            for (int i = 0; i < Lines.Length; i++)
            {
                payer = Lines[i].Trim();
                newPayer = "0" + payer;
                cmd = "Select * from `icustomers` where `payer` = '0" + payer + "' ORDER BY `contractNumber` DESC;";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    newPayer = payer;
                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "' ORDER BY `contractNumber` DESC;";
                    ddx = G1.get_db_data(cmd);
                }
                if (ddx.Rows.Count <= 0)
                {
                    int idx = payer.Length;
                    string lastChar = payer.Substring(idx - 1, 1);
                    char ch = (char)lastChar[0];
                    if (char.IsLetter(ch))
                    {
                        newPayer = payer.TrimEnd(ch);
                        cmd = "Select * from `icustomers` where `payer` = '" + newPayer + "' ORDER BY `contractNumber` DESC;";
                        ddx = G1.get_db_data(cmd);
                    }
                }
                if (ddx.Rows.Count > 0)
                    break;
            }
            if (ddx.Rows.Count <= 0)
            {

            }
            if (ddx.Rows.Count > 0)
            {
                contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
                if (ddx.Rows.Count > 1)
                {
                    DataTable ddd = null;
                    bool found = false;
                    string premium = "";
                    string originalContractNumber = contractNumber;
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                        cmd = "Select * from `icustomers` p JOIN `icontracts` d ON p.`contractNumber` = d.`contractNumber` WHERE p.`contractNumber` = '" + contractNumber + "';";
                        ddd = G1.get_db_data(cmd);
                        if (ddd.Rows.Count > 0)
                        {
                            premium = ddd.Rows[0]["amtOfMonthlyPayt"].ObjToString();
                            if (premium == payment)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                        contractNumber = originalContractNumber;
                }
            }
            return contractNumber;
        }
        /***********************************************************************************************/
        private void cmbWho_EditValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            DetermineView();
        }
        /***********************************************************************************************/
        private void DetermineView()
        {
            if (loading)
                return;
            if (originalDt == null)
                return;
            string names = getWhoQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            CalculateGoodBadUgly(dt);
            this.gridMain.ExpandAllGroups();
        }
        /*******************************************************************************************/
        private string getWhoQuery()
        {
            string procLoc = "";
            string who = "";
            string[] locIDs = this.cmbWho.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    who = locIDs[i].Trim();
                    if (who == "ALL")
                    {
                        procLoc = "";
                        break;
                    }
                    if (who == "INSURANCE")
                        who = "02";
                    else if (who == "FUNERALS")
                        who = "03";
                    else if (who == "DOWN PAYMENTS")
                        who = "04";
                    else
                        who = "01";
                    procLoc += "'" + who + "'";
                }
            }
            return procLoc.Length > 0 ? " `code` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //DataTable dt = (DataTable)dgv.DataSource;
            //CalculateGoodBadUgly(dt);
        }
        /***********************************************************************************************/
        private void duplicatePaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["cNum"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            try
            {
                DataTable tempDt = dt.Clone();
                G1.copy_dt_row(dt, row, tempDt, 0);

                DataRow dR = dt.NewRow();
                dt.Rows.InsertAt(dR, row);
                G1.copy_dt_row(tempDt, 0, dt, row);
            }
            catch ( Exception ex)
            {

            }

            //string line = dr["line"].ObjToString();
            //ProcessPayment(dt, row, line);
            dt.AcceptChanges();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            CalculateGoodBadUgly(dt);
            LoadPaidOffTrusts(dt);
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void editAchLine ()
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["cNum"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DataTable newDt = dt.Clone();
            newDt.ImportRow(dt.Rows[row]);

            EditACHLine editLine = new EditACHLine ( newDt );
            DialogResult result = editLine.ShowDialog();
            if (result == DialogResult.OK)
            {
                string contractNumber = newDt.Rows[0]["cnum"].ObjToString();
                if (achDt == null)
                    return;
                achDt.Rows.Clear();
                DataRow dRow = achDt.NewRow();
                dRow["Customer Number"] = newDt.Rows[0]["cnum"].ObjToString();
                dRow["Location Name"] = newDt.Rows[0]["locationcode"].ObjToString();
                dRow["Amount"] = newDt.Rows[0]["payment"].ObjToString();
                string code = newDt.Rows[0]["code"].ObjToString();
                dRow["code"] = code;
                if (code == "02")
                    dRow["Payment Type"] = "ACH";
                dRow["Transaction Date"] = newDt.Rows[0]["date"].ObjToString();
                dRow["Name On Account"] = newDt.Rows[0]["name"].ObjToString();

                dRow["Payment Origin"] = "ORIGINAL SIGNATURE";
                dRow["Status"] = "PROCESSED";
                achDt.Rows.Add(dRow);

                DataTable newAchDt = dt.Clone();

                bool rv = ParseACHPayment(achDt, 0, newAchDt);

                G1.copy_dt_row(newAchDt, 0, dt, row);

                LoadPaidOffTrusts(dt);

                G1.NumberDataTable(dt);

                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                dgv.Refresh();

                newAchDt.Dispose();
                newAchDt = null;
                achDt.Dispose();
                achDt = null;
                newDt.Dispose();
                newDt = null;
            }
        }
        /***********************************************************************************************/
        private void editInputLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( workACH )
            {
                editAchLine();
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["cNum"].ObjToString();
            //if (String.IsNullOrWhiteSpace(contract))
            //    return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string line = dr["line"].ObjToString();
            DateTime effectiveDate = this.dateTimePicker1.Value;
            EditImportLine editLine = new EditImportLine(dt, row, line, effectiveDate, workTheFirst );
            DialogResult result = editLine.ShowDialog();
            {
                if (result == DialogResult.OK)
                {
                    string newLine = EditImportLine.returnLine;
                    dr["line"] = newLine;
                    dt.Rows[row]["cnum"] = DecodeContractNumber(newLine);
                    ProcessPayment(dt, row, newLine);
                    dt.AcceptChanges();
                    CalculateGoodBadUgly(dt);
                    CalculatePosted(dt);
                    LoadPaidOffTrusts(dt);
                    dgv.RefreshDataSource();
                    dgv.Refresh();
                }
            }
        }
        /***********************************************************************************************/
        private string DecodeContractNumber ( string line )
        {
            string cnum = line.Substring(4, 10);
            cnum = cnum.TrimStart('0');
            cnum = cnum.TrimStart('o');
            cnum = cnum.TrimStart('O');
            return cnum;
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABPAYMENTS")
                LoadTabPayments();
            else if (current.Name.Trim().ToUpper() == "TABRECEIVED")
                LoadTabReceived();
        }
        /***********************************************************************************************/
        private void LoadTabPayments()
        {
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = dt.Copy();
            if ( G1.get_column_number ( dx, "retained") < 0 )
                dx.Columns.Add("retained", Type.GetType("System.Double"));
            double payment = 0D;
            double trust100P = 0D;
            double retained = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                payment = dx.Rows[i]["payment"].ObjToDouble();
                trust100P = dx.Rows[i]["trust100P"].ObjToDouble();
                retained = payment - trust100P;
                dx.Rows[i]["retained"] = retained;
            }
            dgv2.DataSource = dx;
        }
        /***********************************************************************************************/
        private void LoadTabReceived()
        {
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = dt.Copy();
            dgv3.DataSource = dx;
        }
        /***********************************************************************************************/
        private void chkBreak_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if ( chkBreak.Checked )
            {
                string broke = dt.Rows[row]["empty2"].ObjToString().ToUpper().Trim();
                if (broke == "EXCEEDED" || broke == "BROKEN")
                {
                    e.Visible = true;
                    e.Handled = true;
                }
                else
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            if ( chkErrors.Checked )
            {
                string duplicates = dt.Rows[row]["duplicates"].ObjToString().ToUpper().Trim();
                string dup = dt.Rows[row]["duplicate"].ObjToString();
                string status = dt.Rows[row]["found"].ObjToString().Trim().ToUpper();
                if (status == "NO" || status == "L")
                {
                    e.Visible = true;
                    e.Handled = true;
                }
                else if ( !String.IsNullOrWhiteSpace ( duplicates) && dup != "Y")
                {
                    e.Visible = true;
                    e.Handled = true;
                }
                else
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
        }
        /***********************************************************************************************/
        private void goToAlllCustomerDetailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["cnum"].ObjToString();
            string code = dr["code"].ObjToString();
            string contractFile = "contracts";
            if (code == "02")
                contractFile = "icontracts";
            if (!String.IsNullOrWhiteSpace(contract))
            {
                DataTable dx = G1.get_db_data("Select * from `" + contractFile + "` where `contractNumber` = '" + contract + "';");
                if (dx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Cannot find contract!");
                    return;
                }
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        public static bool getUpdateBankAccounts()
        {
            string cmd = "Select * from `options` where `option` = 'Update Payment Bank Accounts (Y/N)';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            string answer = dt.Rows[0]["answer"].ObjToString().ToUpper();
            if (String.IsNullOrWhiteSpace(answer))
                return false;
            if (answer.Substring(0, 1) == "Y")
                return true;
            return false;
        }
        /***********************************************************************************************/
        private void gridMain4_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string cnum = dr["cnum"].ObjToString();
            if (workDt != null)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                double payments = GetCurrentPayments(dt, cnum);
                DataRow[] dRows = workDt.Select("contractNumber = '" + cnum + "'");
                if (dRows.Length > 0)
                {
                    DailyHistory dailyForm = new DailyHistory(cnum, dr, dRows[0], payments);
                    dailyForm.Show();
                }
                else
                {
                    DailyHistory dailyForm = new DailyHistory(cnum, null, null);
                    dailyForm.Show();
                }
            }
            else
            {
                DailyHistory dailyForm = new DailyHistory(cnum, null, null);
                dailyForm.Show();
            }
        }
        /***********************************************************************************************/
        private void chkErrors_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void forceDebitForThisPaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int rrow = gridMain.GetDataSourceRowIndex(rowHandle);

            string code = dr["code"].ObjToString();
            string contract = dr["cnum"].ObjToString();
            string payer = dr["payer"].ObjToString();
            string name = dr["name"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to create a Reverse Payment for customer (" + name + ") ?", "Reverse Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            if (String.IsNullOrWhiteSpace(contract))
                return;

            string paymentsFile = "payments";
            if (code == "02")
                paymentsFile = "ipayments";

            string cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + contract + "' ORDER BY `payDate8` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Cannot Find Payment!");
                return;
            }
            int row = 0;

            string record = dt.Rows[row]["record"].ObjToString();
            double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
            double payment = dt.Rows[row]["paymentAmount"].ObjToDouble();
            double credit = dt.Rows[row]["creditAdjustment"].ObjToDouble();
            double debit = dt.Rows[row]["debitAdjustment"].ObjToDouble();
            double interest = dt.Rows[row]["interestPaid"].ObjToDouble();
            double trust100P = dt.Rows[row]["trust100P"].ObjToDouble();
            double trust85P = dt.Rows[row]["trust85P"].ObjToDouble();

            DataTable dx = dt.Clone();
            G1.copy_dt_row(dt, row, dx, 0);

            downPayment = downPayment * -1D;
            payment = payment * -1D;
            credit = credit * -1D;
            debit = debit * -1D;
            interest = interest * -1D;
            trust100P = trust100P * -1D;
            trust85P = trust85P * -1D;

            string datePaid = dt.Rows[row]["payDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
            string dueDate = dt.Rows[row]["dueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
            string lastName = dt.Rows[row]["lastName"].ObjToString();
            string firstName = dt.Rows[row]["firstName"].ObjToString();
            string checknumber = dt.Rows[row]["checkNumber"].ObjToString();
            string location = dt.Rows[row]["location"].ObjToString();
            string agent = dt.Rows[row]["agentNumber"].ObjToString();
            string user = dt.Rows[row]["userId"].ObjToString();
            user = LoginForm.username;
            string depositNumber = dt.Rows[row]["depositNumber"].ObjToString();
            string debitReason = dt.Rows[row]["debitReason"].ObjToString();
            string creditReason = dt.Rows[row]["creditReason"].ObjToString();
            creditReason += " RTN FROM BANK";

            double oldBalance = dt.Rows[row]["oldBalance"].ObjToDouble();
            DateTime oldDueDate = dt.Rows[row]["oldDueDate8"].ObjToDateTime();
            DateTime oldDOLP = dt.Rows[row]["oldDOLP"].ObjToDateTime();

            record = G1.create_record(paymentsFile, "lastName", "-1");
            G1.update_db_table(paymentsFile, "record", record, new string[] { "contractNumber", contract, "lastName", lastName, "firstName", firstName, "paymentAmount", payment.ToString(), "interestPaid", interest.ToString(), "debitAdjustment", debit.ToString(), "creditAdjustment", credit.ToString(), "debitReason", debitReason, "creditReason", creditReason });
            G1.update_db_table(paymentsFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "location", location, "agentNumber", agent, "userId", user, "depositNumber", depositNumber, "edited", "Manual" });

            string contractFile = "contracts";
            if (code == "02")
                contractFile = "icontracts";

            cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            if ( code == "02")
                cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";

            dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table(contractFile, "record", record, new string[] { "balanceDue", oldBalance.ToString(), "lastDatePaid8", oldDOLP.ToString("yyyy-MM-dd"), "dueDate8", oldDueDate.ToString("yyyy-MM-dd")});
            }

            string audit = "Paid Date: " + datePaid + " Pmt/Credit/Debit: " + payment.ToString() + "/" + credit.ToString() + "/" + debit.ToString();
            G1.AddToAudit(LoginForm.username, "ManualPayment", "Bank Reversal", audit, contract);

            MessageBox.Show("Bank Reverse Payment Has Been Made!", "Bank Reverse Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /***********************************************************************************************/
        private void gridMain6_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain6.GetFocusedDataRow();
            string cnum = dr["cnum"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(cnum);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ImportDailyDeposits_FormClosing(object sender, FormClosingEventArgs e)
        {
            G1.CleanupDataGrid(ref dgv);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv2);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv3);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv4);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv5);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv6);
            GC.Collect();

            if (bankDt != null)
                OnImportDone();
        }
        /***********************************************************************************************/
        public static string getNextTCANumber()
        {
            string request = "";
            string cmd = "Select * from `options` where `option` = 'TCA Starting Number';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "-1";
            int year = 0;
            int seq = 0;
            request = dt.Rows[0]["answer"].ObjToString();
            string[] Lines = request.Split('-');
            if (Lines.Length > 1)
            {
                year = Lines[0].ObjToInt32();
                seq = Lines[1].ObjToInt32();
            }
            else
            {
                seq = Lines[0].ObjToInt32();
                if (seq > 1900)
                {
                    year = DateTime.Now.Year;
                    seq = 0;
                }
            }
            string reinstateRequestRecord = dt.Rows[0]["record"].ObjToString();
            if (year < DateTime.Now.Year)
            {
                year = DateTime.Now.Year;
                seq = 1;
            }
            else
                seq++;

            string str = year.ToString("D4") + "-" + seq.ToString();
            string nextReinstateNumber = str;
            G1.update_db_table("options", "record", reinstateRequestRecord, new string[] { "answer", str });
            return seq.ToString("D4");
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ImportDone;
        protected void OnImportDone()
        {
            if (ImportDone != null)
            {
                if ( bankImportSuccess )
                {
                    ImportDone.Invoke( "SUCCESS" );
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            string columnName = e.Column.FieldName.ToUpper();
            if ( columnName == "ID" && bankDt != null )
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.ListSourceRowIndex;
                double ccFee = dt.Rows[row]["ccFee"].ObjToDouble();
                e.DisplayText = G1.ReformatMoney(ccFee);
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (bankDt == null)
                return;
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                double ccFee = 0D;
                for (int i = 0; i < bankDt.Rows.Count; i++)
                    ccFee += bankDt.Rows[i]["fee"].ObjToDouble();
                e.TotalValueReady = true;
                e.TotalValue = ccFee;
            }
        }
        /***********************************************************************************************/
    }
}
